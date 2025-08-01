#Requires -Version 7.5
function Import-BusBuddySettings {
    <#
    .SYNOPSIS
        Import BusBuddy module settings from INI file

    .DESCRIPTION
        Loads and parses the BusBuddy.settings.ini file and creates a
        structured settings object for use throughout the module.

    .PARAMETER SettingsPath
        Path to the settings INI file

    .PARAMETER DefaultSettings
        Hashtable containing default settings to use if file not found

    .OUTPUTS
        PSCustomObject containing parsed settings
    #>
    [CmdletBinding()]
    param(
        [string]$SettingsPath,

        [hashtable]$DefaultSettings = @{
            General = @{
                VerboseLogging = $false
                ShowWelcomeMessage = $true
                AutoCheckEnvironment = $true
            }
            Development = @{
                DefaultBuildConfiguration = 'Debug'
                DefaultTestConfiguration = 'Debug'
                EnableCodeAnalysis = $true
                AutoRestorePackages = $true
            }
            Database = @{
                DefaultConnectionName = 'BusBuddyLocalDB'
                EnableMigrations = $true
                DefaultProvider = 'SQLite'
            }
            GitHub = @{
                EnableGitHubIntegration = $true
                AutoDetectRepository = $true
                DefaultBranch = 'main'
                WaitForWorkflowCompletion = $false
            }
            Display = @{
                EnableColorOutput = $true
                EnableEmoji = $true
                EnableProgressBars = $true
                ShowTimestamps = $false
            }
            Advanced = @{
                EnableExperimentalFeatures = $false
                LoadDotNetAssemblies = $true
                CacheTimeoutMinutes = 15
                MaxParallelTasks = 4
            }
        }
    )

    # If no settings path provided, use default location next to module
    if (-not $SettingsPath) {
        $moduleRoot = Split-Path $PSCommandPath -Parent
        $SettingsPath = Join-Path $moduleRoot "BusBuddy.settings.ini"
    }

    $settings = $DefaultSettings.Clone()

    # Check if settings file exists
    if (Test-Path $SettingsPath) {
        try {
            Write-Verbose "Loading settings from: $SettingsPath"

            # Read all lines from settings file
            $lines = Get-Content $SettingsPath

            $currentSection = $null

            foreach ($line in $lines) {
                # Skip comments and empty lines
                if ($line.Trim().StartsWith('#') -or [string]::IsNullOrWhiteSpace($line)) {
                    continue
                }

                # Check for section header [SectionName]
                if ($line.Trim() -match '^\[(.+)\]$') {
                    $currentSection = $matches[1]

                    # Create section if it doesn't exist in default settings
                    if (-not $settings.ContainsKey($currentSection)) {
                        $settings[$currentSection] = @{}
                    }

                    continue
                }

                # Parse key-value pairs
                if ($currentSection -and $line.Contains('=')) {
                    $key, $value = $line.Split('=', 2).Trim()

                    # Convert string values to appropriate types
                    $typedValue = switch -regex ($value) {
                        '^\d+$' { [int]$value }
                        '^true$|^false$' { [bool]::Parse($value) }
                        default { $value }
                    }

                    # Add to settings
                    $settings[$currentSection][$key] = $typedValue
                }
            }

            Write-Verbose "Successfully loaded settings from INI file"
        }
        catch {
            Write-Warning "Error loading settings from $SettingsPath. Using defaults. Error: $($_.Exception.Message)"
        }
    }
    else {
        Write-Verbose "Settings file not found at: $SettingsPath. Using default settings."
    }

    # Convert hashtable to PSCustomObject for easier property access
    $settingsObject = [PSCustomObject]@{}

    foreach ($section in $settings.Keys) {
        $sectionObject = [PSCustomObject]@{}

        foreach ($key in $settings[$section].Keys) {
            $sectionObject | Add-Member -MemberType NoteProperty -Name $key -Value $settings[$section][$key]
        }

        $settingsObject | Add-Member -MemberType NoteProperty -Name $section -Value $sectionObject
    }

    return $settingsObject
}
