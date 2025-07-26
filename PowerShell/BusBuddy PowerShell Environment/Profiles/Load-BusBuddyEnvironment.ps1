#Requires -Version 7.5

<#
.SYNOPSIS
    Main BusBuddy PowerShell Environment Loader
.DESCRIPTION
    Loads the complete BusBuddy PowerShell development environment with all modules,
    functions, and configurations required for BusBuddy development.
.PARAMETER Quiet
    Suppress startup messages and progress indicators
.PARAMETER Force
    Force reload all modules and configurations
.PARAMETER ValidateOnly
    Only validate the environment without loading
.EXAMPLE
    & ".\Load-BusBuddyEnvironment.ps1"
    Load the complete BusBuddy PowerShell environment
.EXAMPLE
    & ".\Load-BusBuddyEnvironment.ps1" -Quiet
    Load environment silently without progress messages
#>

[CmdletBinding()]
param(
    [switch]$Quiet,
    [switch]$Force,
    [switch]$ValidateOnly
)

# Environment Variables
$Script:BusBuddyEnvironmentPath = $PSScriptRoot
$Script:BusBuddyProjectRoot = Split-Path -Path (Split-Path -Path (Split-Path -Path $BusBuddyEnvironmentPath -Parent) -Parent) -Parent
$Script:BusBuddyModulesPath = Join-Path $BusBuddyEnvironmentPath "Modules"
$Script:BusBuddyScriptsPath = Join-Path $BusBuddyEnvironmentPath "Scripts"
$Script:BusBuddyConfigPath = Join-Path $BusBuddyEnvironmentPath "Configuration"

function Write-BusBuddyMessage {
    param(
        [string]$Message,
        [string]$Type = "Info"
    )

    if (-not $Quiet) {
        $color = switch ($Type) {
            "Success" { "Green" }
            "Warning" { "Yellow" }
            "Error" { "Red" }
            "Info" { "Cyan" }
            default { "White" }
        }
        Write-Host "🚌 $Message" -ForegroundColor $color
    }
}

function Test-BusBuddyEnvironment {
    <#
    .SYNOPSIS
        Validates the BusBuddy PowerShell environment setup
    #>

    $validationResults = @{
        ProjectRoot       = Test-Path $Script:BusBuddyProjectRoot
        ModulesPath       = Test-Path $Script:BusBuddyModulesPath
        ScriptsPath       = Test-Path $Script:BusBuddyScriptsPath
        ConfigPath        = Test-Path $Script:BusBuddyConfigPath
        PowerShellVersion = $PSVersionTable.PSVersion -ge [version]"7.5.0"
    }

    $allValid = $validationResults.Values | ForEach-Object { $_ } | Where-Object { -not $_ } | Measure-Object | Select-Object -ExpandProperty Count

    if ($allValid -eq 0) {
        Write-BusBuddyMessage "Environment validation passed" "Success"
        return $true
    }
    else {
        Write-BusBuddyMessage "Environment validation failed" "Error"
        $validationResults.GetEnumerator() | Where-Object { -not $_.Value } | ForEach-Object {
            Write-BusBuddyMessage "  ❌ $($_.Key): Failed" "Error"
        }
        return $false
    }
}

function Import-BusBuddyModules {
    <#
    .SYNOPSIS
        Imports all BusBuddy PowerShell modules
    #>

    if (Test-Path $Script:BusBuddyModulesPath) {
        $moduleFiles = Get-ChildItem -Path $Script:BusBuddyModulesPath -Filter "*.psm1" -Recurse

        foreach ($moduleFile in $moduleFiles) {
            try {
                if ($Force) {
                    Remove-Module -Name $moduleFile.BaseName -Force -ErrorAction SilentlyContinue
                }
                Import-Module -Name $moduleFile.FullName -Force:$Force -Global
                Write-BusBuddyMessage "Loaded module: $($moduleFile.BaseName)" "Success"
            }
            catch {
                Write-BusBuddyMessage "Failed to load module $($moduleFile.BaseName): $($_.Exception.Message)" "Error"
            }
        }
    }
}

function Set-BusBuddyAliases {
    <#
    .SYNOPSIS
        Sets up BusBuddy command aliases
    #>

    # Core BusBuddy aliases
    $aliases = @{
        'bb-health'       = 'Invoke-BusBuddyHealthCheck'
        'bb-build'        = 'Invoke-BusBuddyBuild'
        'bb-run'          = 'Start-BusBuddyApplication'
        'bb-test'         = 'Invoke-BusBuddyTests'
        'bb-diagnostic'   = 'Invoke-BusBuddyDiagnostic'
        'bb-debug-start'  = 'Start-BusBuddyDebugFilter'
        'bb-debug-export' = 'Export-BusBuddyDebugData'
        'bb-env-reload'   = 'Reset-BusBuddyEnvironment'
    }

    foreach ($alias in $aliases.GetEnumerator()) {
        if (Get-Command $alias.Value -ErrorAction SilentlyContinue) {
            Set-Alias -Name $alias.Key -Value $alias.Value -Scope Global -Force
            Write-BusBuddyMessage "Set alias: $($alias.Key) → $($alias.Value)" "Info"
        }
    }
}

function Initialize-BusBuddyEnvironment {
    <#
    .SYNOPSIS
        Initializes the complete BusBuddy PowerShell environment
    #>

    Write-BusBuddyMessage "Initializing BusBuddy PowerShell Environment" "Info"
    Write-BusBuddyMessage "Project Root: $Script:BusBuddyProjectRoot" "Info"
    Write-BusBuddyMessage "Environment Path: $Script:BusBuddyEnvironmentPath" "Info"

    # Validate environment
    if (-not (Test-BusBuddyEnvironment)) {
        Write-BusBuddyMessage "Environment validation failed. Cannot continue." "Error"
        return $false
    }

    if ($ValidateOnly) {
        Write-BusBuddyMessage "Validation only mode - environment is valid" "Success"
        return $true
    }

    # Import modules
    Write-BusBuddyMessage "Loading BusBuddy modules..." "Info"
    Import-BusBuddyModules

    # Set up aliases
    Write-BusBuddyMessage "Setting up BusBuddy aliases..." "Info"
    Set-BusBuddyAliases

    # Set location to project root
    Set-Location $Script:BusBuddyProjectRoot
    Write-BusBuddyMessage "Set location to project root" "Info"

    # Add environment variables
    $env:BUSBUDDY_PROJECT_ROOT = $Script:BusBuddyProjectRoot
    $env:BUSBUDDY_ENVIRONMENT_PATH = $Script:BusBuddyEnvironmentPath

    Write-BusBuddyMessage "BusBuddy PowerShell Environment loaded successfully!" "Success"
    Write-BusBuddyMessage "Available commands: bb-health, bb-build, bb-run, bb-test, bb-diagnostic" "Info"

    return $true
}

# Function to reset and reload the environment
function Reset-BusBuddyEnvironment {
    <#
    .SYNOPSIS
        Resets and reloads the BusBuddy PowerShell environment
    #>

    Write-BusBuddyMessage "Resetting BusBuddy PowerShell Environment..." "Warning"

    # Remove all BusBuddy modules
    Get-Module | Where-Object { $_.Name -like "*BusBuddy*" } | Remove-Module -Force

    # Remove aliases
    $aliases = @('bb-health', 'bb-build', 'bb-run', 'bb-test', 'bb-diagnostic', 'bb-debug-start', 'bb-debug-export', 'bb-env-reload')
    foreach ($alias in $aliases) {
        Remove-Alias -Name $alias -Force -ErrorAction SilentlyContinue
    }

    # Reload the environment
    & $PSCommandPath -Force:$true -Quiet:$Quiet
}

# Main execution
try {
    $result = Initialize-BusBuddyEnvironment
    if (-not $result) {
        Write-BusBuddyMessage "Failed to initialize BusBuddy PowerShell Environment" "Error"
        exit 1
    }
}
catch {
    Write-BusBuddyMessage "Error initializing BusBuddy PowerShell Environment: $($_.Exception.Message)" "Error"
    Write-BusBuddyMessage "Stack Trace: $($_.ScriptStackTrace)" "Error"
    exit 1
}

# Export key functions for external access
Export-ModuleMember -Function Test-BusBuddyEnvironment, Reset-BusBuddyEnvironment -Variable BusBuddyProjectRoot, BusBuddyEnvironmentPath
