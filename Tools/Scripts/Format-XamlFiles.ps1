#Requires -Version 7.0
<#
.SYNOPSIS
    Bus Buddy XAML Formatting and Validation Tool
    
.DESCRIPTION
    Comprehensive XAML processing tool for the Bus Buddy WPF project.
    Focuses on Syncfusion WPF 30.1.40 compliance, scheduling UI validation,
    and consistent formatting for novice developers.
    
.PARAMETER Path
    Root path to search for XAML files (defaults to repository root)
    
.PARAMETER Format
    Apply consistent formatting (4-space indentation, proper line breaks)
    
.PARAMETER Validate
    Validate XML syntax and WPF/Syncfusion compliance
    
.PARAMETER RemoveDeprecated
    Remove deprecated Syncfusion attributes safely
    
.PARAMETER BackupEnabled
    Create automatic backups before modifications (default: true)
    
.PARAMETER PreCommitMode
    Run in pre-commit validation mode (format + validate only)
    
.EXAMPLE
    .\Format-XamlFiles.ps1 -Path "C:\BusBuddy" -Format -Validate
    
.EXAMPLE
    .\Format-XamlFiles.ps1 -PreCommitMode
    
.NOTES
    Author: Bus Buddy Development Team
    Version: 1.0
    Focus: WPF Scheduling UI, Syncfusion 30.1.40, Novice-Friendly
#>

param(
    [string]$Path = (Get-Location),
    [switch]$Format,
    [switch]$Validate,
    [switch]$RemoveDeprecated,
    [switch]$BackupEnabled = $true,
    [switch]$PreCommitMode,
    [switch]$Verbose
)

# ═══════════════════════════════════════════════════════════════════════════════════
# CONFIGURATION FOR BUS BUDDY WPF PROJECT
# ═══════════════════════════════════════════════════════════════════════════════════

$script:Config = @{
    IndentSize = 4
    BackupSuffix = ".backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    SyncfusionVersion = "30.1.40"
    
    # WPF Scheduling-Specific Validation Patterns
    SchedulingPatterns = @{
        DateTimeValidation = @(
            'DateTimePattern\s*=\s*"[^"]*"',
            'SelectedDate\s*=\s*"{Binding\s+[^}]*}"',
            'MinDate\s*=\s*"[^"]*"',
            'MaxDate\s*=\s*"[^"]*"'
        )
        DataBindingValidation = @(
            '{Binding\s+(?!.*FallbackValue)[^}]*}',
            'ItemsSource\s*=\s*"{Binding\s+[^}]*}"',
            'SelectedItem\s*=\s*"{Binding\s+[^}]*}"'
        )
        SyncfusionCompliance = @(
            'syncfusionskin:SfSkinManager\.VisualStyle',
            'syncfusion:SfScheduler',
            'syncfusion:SfDatePicker',
            'syncfusion:SfTimePicker'
        )
    }
    
    # Deprecated Patterns to Remove
    DeprecatedPatterns = @(
        'syncfusionskin:SfSkinManager\.VisualStyle="FluentDark"',
        'syncfusionskin:SfSkinManager\.VisualStyle="FluentLight"',
        'xmlns:sf="http://schemas.syncfusion.com/wpf"'
    )
    
    # Required Namespace Declarations
    RequiredNamespaces = @{
        'syncfusion' = 'http://schemas.syncfusion.com/wpf'
        'syncfusionskin' = 'clr-namespace:Syncfusion.SfSkinManager;assembly=Syncfusion.SfSkinManager.WPF'
        'syncfusionscheduler' = 'clr-namespace:Syncfusion.UI.Xaml.Scheduler;assembly=Syncfusion.SfScheduler.WPF'
    }
}

# ═══════════════════════════════════════════════════════════════════════════════════
# CORE FUNCTIONS
# ═══════════════════════════════════════════════════════════════════════════════════

function Write-StatusMessage {
    param(
        [string]$Message,
        [ValidateSet('Info', 'Success', 'Warning', 'Error')]$Type = 'Info'
    )
    
    $colors = @{
        'Info' = 'Cyan'
        'Success' = 'Green'
        'Warning' = 'Yellow'
        'Error' = 'Red'
    }
    
    $icons = @{
        'Info' = '🔍'
        'Success' = '✅'
        'Warning' = '⚠️'
        'Error' = '❌'
    }
    
    Write-Host "$($icons[$Type]) $Message" -ForegroundColor $colors[$Type]
}

function Test-XamlSyntax {
    <#
    .SYNOPSIS
        Validates XAML file syntax and structure
    #>
    param([string]$FilePath)
    
    try {
        [xml]$xmlDoc = Get-Content $FilePath -Raw
        
        # Additional WPF-specific validations
        $content = Get-Content $FilePath -Raw
        
        # Check for common scheduling UI issues
        $issues = @()
        
        # 1. DateTimePattern validation for Syncfusion controls
        if ($content -match 'DateTimePattern\s*=\s*"([^"]*)"') {
            $pattern = $matches[1]
            if ($pattern -notmatch '^[dMyhHmsfttz\s\/\-\.:]+$') {
                $issues += "Invalid DateTimePattern: $pattern"
            }
        }
        
        # 2. Missing FallbackValues in critical bindings
        if ($content -match 'ItemsSource\s*=\s*"{Binding\s+(?!.*FallbackValue)[^}]*}"') {
            $issues += "ItemsSource binding missing FallbackValue (recommended for scheduling data)"
        }
        
        # 3. Deprecated Syncfusion attributes
        foreach ($deprecated in $script:Config.DeprecatedPatterns) {
            if ($content -match $deprecated) {
                $issues += "Found deprecated pattern: $deprecated"
            }
        }
        
        return @{
            IsValid = $true
            Issues = $issues
            XmlDocument = $xmlDoc
        }
    }
    catch {
        return @{
            IsValid = $false
            Issues = @("XML Syntax Error: $($_.Exception.Message)")
            XmlDocument = $null
        }
    }
}

function Format-XamlContent {
    <#
    .SYNOPSIS
        Applies consistent formatting to XAML content
    #>
    param(
        [string]$Content,
        [int]$IndentSize = 4
    )
    
    try {
        # Load as XML for proper formatting
        [xml]$xmlDoc = $Content
        
        # Configure XML writer settings for consistent output
        $xmlSettings = New-Object System.Xml.XmlWriterSettings
        $xmlSettings.Indent = $true
        $xmlSettings.IndentChars = ' ' * $IndentSize
        $xmlSettings.NewLineChars = "`r`n"
        $xmlSettings.NewLineHandling = [System.Xml.NewLineHandling]::Replace
        $xmlSettings.OmitXmlDeclaration = $false
        $xmlSettings.Encoding = [System.Text.Encoding]::UTF8
        
        # Write formatted XML to string
        $stringWriter = New-Object System.IO.StringWriter
        $xmlWriter = [System.Xml.XmlWriter]::Create($stringWriter, $xmlSettings)
        
        $xmlDoc.Save($xmlWriter)
        
        $xmlWriter.Close()
        $formattedContent = $stringWriter.ToString()
        $stringWriter.Close()
        
        # Post-processing for XAML-specific formatting
        $formattedContent = $formattedContent -replace '^\s*<\?xml[^>]*\>\s*[\r\n]*', ''
        $formattedContent = $formattedContent.TrimStart()
        
        return $formattedContent
    }
    catch {
        Write-StatusMessage "Formatting failed: $($_.Exception.Message)" -Type 'Error'
        return $Content
    }
}

function Remove-DeprecatedAttributes {
    <#
    .SYNOPSIS
        Safely removes deprecated Syncfusion attributes
    #>
    param([string]$Content)
    
    $updatedContent = $Content
    
    foreach ($pattern in $script:Config.DeprecatedPatterns) {
        # Use XML-aware removal
        try {
            [xml]$xmlDoc = $updatedContent
            
            # Find elements with deprecated attributes
            $xpath = "//*[@*[contains(name(), 'VisualStyle')]]"
            $elementsWithDeprecated = $xmlDoc.SelectNodes($xpath)
            
            foreach ($element in $elementsWithDeprecated) {
                $attributesToRemove = @()
                foreach ($attr in $element.Attributes) {
                    if ($attr.Name -match 'VisualStyle' -and $attr.Value -match 'Fluent(Dark|Light)') {
                        $attributesToRemove += $attr.Name
                    }
                }
                
                foreach ($attrName in $attributesToRemove) {
                    $element.RemoveAttribute($attrName)
                    Write-StatusMessage "Removed deprecated attribute: $attrName" -Type 'Success'
                }
            }
            
            # Convert back to string
            $stringWriter = New-Object System.IO.StringWriter
            $xmlDoc.Save($stringWriter)
            $updatedContent = $stringWriter.ToString()
            $stringWriter.Close()
        }
        catch {
            # Fallback to regex if XML approach fails
            $updatedContent = $updatedContent -replace $pattern, ''
        }
    }
    
    return $updatedContent
}

function Backup-XamlFile {
    <#
    .SYNOPSIS
        Creates backup of XAML file before modification
    #>
    param([string]$FilePath)
    
    if (-not $BackupEnabled) { return $null }
    
    $backupPath = "$FilePath$($script:Config.BackupSuffix)"
    Copy-Item $FilePath $backupPath
    return $backupPath
}

function Process-XamlFile {
    <#
    .SYNOPSIS
        Main processing function for individual XAML files
    #>
    param(
        [string]$FilePath,
        [switch]$FormatFile,
        [switch]$ValidateFile,
        [switch]$RemoveDeprecatedAttribs
    )
    
    $fileName = Split-Path $FilePath -Leaf
    Write-StatusMessage "Processing: $fileName" -Type 'Info'
    
    # Read original content
    $originalContent = Get-Content $FilePath -Raw
    $currentContent = $originalContent
    $backupPath = $null
    $hasChanges = $false
    
    try {
        # Validation
        if ($ValidateFile -or $PreCommitMode) {
            $validation = Test-XamlSyntax -FilePath $FilePath
            
            if (-not $validation.IsValid) {
                Write-StatusMessage "❌ Validation failed for $fileName" -Type 'Error'
                $validation.Issues | ForEach-Object { Write-StatusMessage "  - $_" -Type 'Error' }
                return $false
            }
            
            if ($validation.Issues.Count -gt 0) {
                Write-StatusMessage "⚠️ Issues found in $fileName" -Type 'Warning'
                $validation.Issues | ForEach-Object { Write-StatusMessage "  - $_" -Type 'Warning' }
            }
        }
        
        # Create backup before any modifications
        if ($FormatFile -or $RemoveDeprecatedAttribs) {
            $backupPath = Backup-XamlFile -FilePath $FilePath
        }
        
        # Remove deprecated attributes
        if ($RemoveDeprecatedAttribs) {
            $updatedContent = Remove-DeprecatedAttributes -Content $currentContent
            if ($updatedContent -ne $currentContent) {
                $currentContent = $updatedContent
                $hasChanges = $true
                Write-StatusMessage "Removed deprecated attributes from $fileName" -Type 'Success'
            }
        }
        
        # Format content
        if ($FormatFile -or $PreCommitMode) {
            $formattedContent = Format-XamlContent -Content $currentContent -IndentSize $script:Config.IndentSize
            if ($formattedContent -ne $currentContent) {
                $currentContent = $formattedContent
                $hasChanges = $true
                Write-StatusMessage "Formatted $fileName" -Type 'Success'
            }
        }
        
        # Save changes
        if ($hasChanges) {
            Set-Content -Path $FilePath -Value $currentContent -NoNewline -Encoding UTF8
            
            # Verify the changes didn't break XML
            $finalValidation = Test-XamlSyntax -FilePath $FilePath
            if (-not $finalValidation.IsValid) {
                # Restore from backup
                if ($backupPath -and (Test-Path $backupPath)) {
                    Copy-Item $backupPath $FilePath -Force
                    Write-StatusMessage "❌ Changes broke XML syntax, restored from backup" -Type 'Error'
                }
                return $false
            }
            
            # Clean up backup if successful
            if ($backupPath -and (Test-Path $backupPath)) {
                Remove-Item $backupPath
            }
        }
        
        return $true
    }
    catch {
        Write-StatusMessage "❌ Error processing $fileName`: $($_.Exception.Message)" -Type 'Error'
        
        # Restore from backup if available
        if ($backupPath -and (Test-Path $backupPath)) {
            Copy-Item $backupPath $FilePath -Force
            Remove-Item $backupPath
        }
        
        return $false
    }
}

# ═══════════════════════════════════════════════════════════════════════════════════
# MAIN EXECUTION
# ═══════════════════════════════════════════════════════════════════════════════════

function Main {
    Write-StatusMessage "🚌 Bus Buddy XAML Processing Tool v1.0" -Type 'Info'
    Write-StatusMessage "📁 Working Directory: $Path" -Type 'Info'
    Write-StatusMessage "🎯 Focus: WPF Scheduling UI, Syncfusion $($script:Config.SyncfusionVersion)" -Type 'Info'
    
    # Set defaults for pre-commit mode
    if ($PreCommitMode) {
        $Format = $true
        $Validate = $true
        Write-StatusMessage "🔄 Running in Pre-Commit Mode (Format + Validate)" -Type 'Info'
    }
    
    # Find all XAML files
    $xamlFiles = Get-ChildItem -Path $Path -Recurse -Filter "*.xaml" | Where-Object {
        $_.FullName -notlike "*\bin\*" -and $_.FullName -notlike "*\obj\*" -and $_.FullName -notlike "*\.backup_*"
    }
    
    if ($xamlFiles.Count -eq 0) {
        Write-StatusMessage "No XAML files found in $Path" -Type 'Warning'
        return
    }
    
    Write-StatusMessage "Found $($xamlFiles.Count) XAML files to process" -Type 'Info'
    
    $successCount = 0
    $failedFiles = @()
    
    foreach ($file in $xamlFiles) {
        $success = Process-XamlFile -FilePath $file.FullName -FormatFile:$Format -ValidateFile:$Validate -RemoveDeprecatedAttribs:$RemoveDeprecated
        
        if ($success) {
            $successCount++
        } else {
            $failedFiles += $file.FullName
        }
    }
    
    # Summary
    Write-Host "`n" -NoNewline
    Write-StatusMessage "📊 Processing Complete" -Type 'Info'
    Write-StatusMessage "✅ Successful: $successCount files" -Type 'Success'
    
    if ($failedFiles.Count -gt 0) {
        Write-StatusMessage "❌ Failed: $($failedFiles.Count) files" -Type 'Error'
        if ($Verbose) {
            Write-StatusMessage "Failed files:" -Type 'Error'
            $failedFiles | ForEach-Object { Write-StatusMessage "  - $_" -Type 'Error' }
        }
    }
    
    # Pre-commit mode specific output
    if ($PreCommitMode) {
        if ($failedFiles.Count -eq 0) {
            Write-StatusMessage "🎉 All XAML files ready for commit!" -Type 'Success'
            exit 0
        } else {
            Write-StatusMessage "🚫 XAML validation failed - fix issues before committing" -Type 'Error'
            exit 1
        }
    }
}

# Execute main function
Main
