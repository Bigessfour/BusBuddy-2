#Requires -Version 7.5
function Get-BusBuddySettings {
    <#
    .SYNOPSIS
        Get BusBuddy module settings

    .DESCRIPTION
        Displays the current BusBuddy module settings and provides options
        to export or modify the settings.

    .PARAMETER ExportToFile
        Export settings to a file

    .PARAMETER ExportPath
        Path to export settings to

    .PARAMETER Section
        Specific section to display

    .PARAMETER AsObject
        Return settings as an object instead of displaying

    .OUTPUTS
        PSCustomObject containing settings or formatted display
    #>
    [CmdletBinding()]
    param(
        [switch]$ExportToFile,
        [string]$ExportPath,
        [string]$Section,
        [switch]$AsObject
    )

    # Get module settings from global variable or load if not available
    $moduleSettings = $global:BusBuddySettings

    if (-not $moduleSettings) {
        $moduleRoot = Split-Path $PSCommandPath -Parent
        $settingsPath = Join-Path (Split-Path $moduleRoot -Parent) "BusBuddy.settings.ini"
        $moduleSettings = Import-BusBuddySettings -SettingsPath $settingsPath
    }

    # Export settings to file if requested
    if ($ExportToFile) {
        $targetPath = $ExportPath

        if (-not $targetPath) {
            $targetPath = Join-Path (Get-Location) "BusBuddy.settings.ini"
        }

        try {
            $output = "# BusBuddy Module Settings`n"
            $output += "# Exported on $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')`n"
            $output += "# Modify these settings to customize the module behavior`n"

            foreach ($sectionName in ($moduleSettings | Get-Member -MemberType NoteProperty).Name) {
                $output += "`n[$sectionName]`n"
                $section = $moduleSettings.$sectionName

                foreach ($propertyName in ($section | Get-Member -MemberType NoteProperty).Name) {
                    $value = $section.$propertyName
                    $output += "$propertyName = $($value.ToString().ToLower())`n"
                }
            }

            $output | Set-Content -Path $targetPath -Encoding UTF8
            Write-BusBuddyStatus "Settings exported to: $targetPath" -Status Success
        }
        catch {
            Write-BusBuddyError -Message "Failed to export settings: $($_.Exception.Message)" -RecommendedAction "Check file path and permissions"
        }

        return
    }

    # Return as object if requested
    if ($AsObject) {
        if ($Section -and ($moduleSettings | Get-Member -Name $Section -MemberType NoteProperty)) {
            return $moduleSettings.$Section
        }
        return $moduleSettings
    }

    # Display settings in console
    Write-Host ""
    Write-BusBuddyStatus "🔧 BusBuddy Module Settings" -Status Info
    Write-Host "══════════════════════════════════════" -ForegroundColor DarkGray

    # Filter by section if specified
    $sectionsToDisplay = if ($Section) { @($Section) } else { ($moduleSettings | Get-Member -MemberType NoteProperty).Name }

    foreach ($sectionName in $sectionsToDisplay) {
        if (-not ($moduleSettings | Get-Member -Name $sectionName -MemberType NoteProperty)) {
            Write-BusBuddyStatus "Section not found: $sectionName" -Status Warning
            continue
        }

        $section = $moduleSettings.$sectionName

        Write-Host ""
        Write-Host "📂 $sectionName" -ForegroundColor Cyan

        foreach ($propertyName in ($section | Get-Member -MemberType NoteProperty).Name) {
            $value = $section.$propertyName
            $valueColor = switch ($value.GetType().Name) {
                'Boolean' { if ($value) { 'Green' } else { 'Red' } }
                'Int32' { 'Yellow' }
                default { 'White' }
            }

            Write-Host "  $propertyName = " -ForegroundColor Gray -NoNewline
            Write-Host "$value" -ForegroundColor $valueColor
        }
    }

    Write-Host ""
    Write-Host "💡 To modify settings:" -ForegroundColor Yellow
    Write-Host "  • Edit directly: $((Join-Path (Split-Path (Split-Path $PSCommandPath -Parent) -Parent) 'BusBuddy.settings.ini'))" -ForegroundColor Gray
    Write-Host "  • Export to file: Get-BusBuddySettings -ExportToFile" -ForegroundColor Gray
    Write-Host "  • Programmatically: Set-BusBuddySettings -Section 'SectionName' -Name 'SettingName' -Value 'NewValue'" -ForegroundColor Gray
}

#Requires -Version 7.5
function Set-BusBuddySettings {
    <#
    .SYNOPSIS
        Modify BusBuddy module settings

    .DESCRIPTION
        Updates the BusBuddy module settings in memory and optionally persists
        the changes to the settings file.

    .PARAMETER Section
        Section name containing the setting

    .PARAMETER Name
        Name of the setting to modify

    .PARAMETER Value
        New value for the setting

    .PARAMETER PersistChanges
        Save changes to the settings file

    .EXAMPLE
        Set-BusBuddySettings -Section "Display" -Name "EnableEmoji" -Value $false -PersistChanges
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Section,

        [Parameter(Mandatory)]
        [string]$Name,

        [Parameter(Mandatory)]
        $Value,

        [switch]$PersistChanges
    )

    # Get module settings from global variable or load if not available
    $moduleSettings = $global:BusBuddySettings

    if (-not $moduleSettings) {
        $moduleRoot = Split-Path $PSCommandPath -Parent
        $settingsPath = Join-Path (Split-Path $moduleRoot -Parent) "BusBuddy.settings.ini"
        $moduleSettings = Import-BusBuddySettings -SettingsPath $settingsPath
    }

    # Check if section exists
    if (-not ($moduleSettings | Get-Member -Name $Section -MemberType NoteProperty)) {
        Write-BusBuddyError -Message "Section not found: $Section" -RecommendedAction "Valid sections: $(($moduleSettings | Get-Member -MemberType NoteProperty).Name -join ', ')"
        return $false
    }

    # Check if setting exists in section
    $sectionObject = $moduleSettings.$Section
    if (-not ($sectionObject | Get-Member -Name $Name -MemberType NoteProperty)) {
        Write-BusBuddyError -Message "Setting not found: $Name in section $Section" -RecommendedAction "Valid settings: $(($sectionObject | Get-Member -MemberType NoteProperty).Name -join ', ')"
        return $false
    }

    # Get current value and type
    $currentValue = $sectionObject.$Name
    $currentType = $currentValue.GetType()

    # Try to convert new value to correct type
    try {
        # Convert value to match current type
        $typedValue = switch ($currentType.Name) {
            'Boolean' { [bool]$Value }
            'Int32' { [int]$Value }
            'String' { [string]$Value }
            default { $Value }
        }

        # Update setting in memory
        $sectionObject.$Name = $typedValue

        Write-BusBuddyStatus "Updated setting: $Section.$Name = $typedValue" -Status Success

        # Persist changes if requested
        if ($PersistChanges) {
            # Export settings to file
            Get-BusBuddySettings -ExportToFile
        }

        return $true
    }
    catch {
        Write-BusBuddyError -Message "Failed to update setting: $($_.Exception.Message)" -RecommendedAction "Ensure value $Value can be converted to type $($currentType.Name)"
        return $false
    }
}
