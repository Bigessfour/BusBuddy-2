#Requires -Version 7.0
<#
.SYNOPSIS
    Verify and fix VS Code terminal configuration for Bus Buddy

.DESCRIPTION
    This script verifies that VS Code terminal settings are correctly configured
    to use PowerShell 7.5.2 and load the Bus Buddy profiles automatically.

    It can fix incorrect settings if found.

.NOTES
    Version: 1.0
    Date: July 19, 2025
    Author: BusBuddy Team

.EXAMPLE
    # Verify terminal configuration
    .\verify-terminal-config.ps1

.EXAMPLE
    # Verify and fix terminal configuration
    .\verify-terminal-config.ps1 -Fix
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [switch]$Fix
)

# Find project root
function Find-BusBuddyProjectRoot {
    param(
        [Parameter(Mandatory = $false)]
        [int]$MaxDepth = 5
    )

    $currentPath = $PWD.Path
    $depth = 0

    while ($depth -lt $MaxDepth) {
        # Check for solution file
        $solutionPath = Join-Path $currentPath "BusBuddy.sln"
        if (Test-Path $solutionPath) {
            return $currentPath
        }

        # Check for profile file
        $profilePath = Join-Path $currentPath "BusBuddy-PowerShell-Profile.ps1"
        if (Test-Path $profilePath) {
            return $currentPath
        }

        # Move up one directory
        $parent = Split-Path $currentPath -Parent
        if ($parent -eq $currentPath -or [string]::IsNullOrEmpty($parent)) {
            break
        }
        $currentPath = $parent
        $depth++
    }

    return $null
}

# Find VS Code settings file
function Find-VSCodeSettingsFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ProjectRoot
    )

    $settingsPath = Join-Path $ProjectRoot ".vscode\settings.json"
    if (Test-Path $settingsPath) {
        return $settingsPath
    }

    return $null
}

Write-Host "🔍 Verifying VS Code terminal configuration..." -ForegroundColor Cyan

# Find project root
$projectRoot = Find-BusBuddyProjectRoot
if (-not $projectRoot) {
    Write-Host "❌ Bus Buddy project root not found. Are you in the correct directory?" -ForegroundColor Red
    return $false
}

# Find VS Code settings file
$settingsPath = Find-VSCodeSettingsFile -ProjectRoot $projectRoot
if (-not $settingsPath) {
    Write-Host "❌ VS Code settings file not found at: .vscode\settings.json" -ForegroundColor Red

    if ($Fix) {
        Write-Host "🔧 Creating .vscode directory..." -ForegroundColor Yellow
        $vscodePath = Join-Path $projectRoot ".vscode"
        if (-not (Test-Path $vscodePath)) {
            New-Item -ItemType Directory -Path $vscodePath -Force | Out-Null
        }

        $settingsPath = Join-Path $vscodePath "settings.json"

        # Create minimal settings file
        $minimalSettings = @{
            "terminal.integrated.defaultProfile.windows" = "PowerShell 7.5.2"
            "terminal.integrated.profiles.windows" = @{
                "PowerShell 7.5.2" = @{
                    "path" = "pwsh.exe"
                    "args" = @(
                        "-NoProfile",
                        "-NoExit",
                        "-Command",
                        "& '`${workspaceFolder}\\BusBuddy-PowerShell-Profile.ps1'; & '`${workspaceFolder}\\.vscode\\BusBuddy-Advanced-Workflows.ps1'"
                    )
                    "icon" = "terminal-powershell"
                }
            }
        }

        $minimalSettings | ConvertTo-Json -Depth 5 | Set-Content $settingsPath -Encoding UTF8
        Write-Host "✅ Created new VS Code settings file with correct terminal configuration" -ForegroundColor Green

        return $true
    }

    return $false
}

# Read settings file
try {
    $settings = Get-Content $settingsPath -Raw | ConvertFrom-Json -ErrorAction Stop
} catch {
    Write-Host "❌ Failed to parse VS Code settings file: $_" -ForegroundColor Red
    return $false
}

# Check terminal configuration
$terminalConfig = $settings.'terminal.integrated.defaultProfile.windows'
$terminalProfiles = $settings.'terminal.integrated.profiles.windows'

$isCorrectDefault = $terminalConfig -eq "PowerShell 7.5.2"
$hasCorrectProfile = $false
$correctArgs = $false

if ($terminalProfiles -and $terminalProfiles.'PowerShell 7.5.2') {
    $hasCorrectProfile = $true
    $ps752Profile = $terminalProfiles.'PowerShell 7.5.2'

    # Check path
    $correctPath = $ps752Profile.path -eq "pwsh.exe"

    # Check args
    if ($ps752Profile.args) {
        $args = $ps752Profile.args
        # Convert from array or PSCustomObject to array
        if ($args -isnot [System.Array]) {
            $args = @($args)
        }

        $hasNoProfile = $args -contains "-NoProfile"
        $hasNoExit = $args -contains "-NoExit"
        $hasCommand = $args -contains "-Command"

        # Check if the command loads the Bus Buddy profiles
        $commandArg = $args | Where-Object { $_ -match "BusBuddy-PowerShell-Profile\.ps1" -and $_ -match "BusBuddy-Advanced-Workflows\.ps1" }
        $hasCorrectCommand = $null -ne $commandArg

        $correctArgs = $hasNoProfile -and $hasNoExit -and $hasCommand -and $hasCorrectCommand
    }
}

# Display results
Write-Host "`n📋 Terminal Configuration Results:" -ForegroundColor Cyan
Write-Host "Default profile set to PowerShell 7.5.2: $($isCorrectDefault ? '✅' : '❌')" -ForegroundColor ($isCorrectDefault ? 'Green' : 'Red')
Write-Host "PowerShell 7.5.2 profile exists: $($hasCorrectProfile ? '✅' : '❌')" -ForegroundColor ($hasCorrectProfile ? 'Green' : 'Red')

if ($hasCorrectProfile) {
    Write-Host "PowerShell 7.5.2 path correctly set to pwsh.exe: $($correctPath ? '✅' : '❌')" -ForegroundColor ($correctPath ? 'Green' : 'Red')
    Write-Host "PowerShell 7.5.2 arguments correctly configured: $($correctArgs ? '✅' : '❌')" -ForegroundColor ($correctArgs ? 'Green' : 'Red')
}

$configCorrect = $isCorrectDefault -and $hasCorrectProfile -and $correctPath -and $correctArgs

if ($configCorrect) {
    Write-Host "`n✅ VS Code terminal configuration is correct!" -ForegroundColor Green
    Write-Host "New terminals will automatically load Bus Buddy profiles." -ForegroundColor Cyan
} else {
    Write-Host "`n❌ VS Code terminal configuration is incorrect or incomplete." -ForegroundColor Red

    if ($Fix) {
        Write-Host "`n🔧 Fixing terminal configuration..." -ForegroundColor Yellow

        # Create correct configuration
        if (-not $settings.'terminal.integrated.profiles.windows') {
            $settings | Add-Member -MemberType NoteProperty -Name 'terminal.integrated.profiles.windows' -Value @{}
        }

        $settings.'terminal.integrated.defaultProfile.windows' = "PowerShell 7.5.2"
        $settings.'terminal.integrated.profiles.windows'.'PowerShell 7.5.2' = @{
            "path" = "pwsh.exe"
            "args" = @(
                "-NoProfile",
                "-NoExit",
                "-Command",
                "& '`${workspaceFolder}\\BusBuddy-PowerShell-Profile.ps1'; & '`${workspaceFolder}\\.vscode\\BusBuddy-Advanced-Workflows.ps1'"
            )
            "icon" = "terminal-powershell"
        }

        # Save updated settings
        try {
            $settings | ConvertTo-Json -Depth 10 | Set-Content $settingsPath -Encoding UTF8
            Write-Host "✅ Fixed VS Code terminal configuration" -ForegroundColor Green
            Write-Host "New terminals will now automatically load Bus Buddy profiles." -ForegroundColor Cyan
        } catch {
            Write-Host "❌ Failed to update VS Code settings file: $_" -ForegroundColor Red
            return $false
        }
    } else {
        Write-Host "`n💡 Run this script with -Fix to automatically fix the configuration:" -ForegroundColor Yellow
        Write-Host "  .\verify-terminal-config.ps1 -Fix" -ForegroundColor Gray
    }
}

# Display additional information
Write-Host "`n📋 Bus Buddy Terminal Integration:" -ForegroundColor Cyan
Write-Host "For the best experience, ensure PowerShell 7.5.2 is installed:" -ForegroundColor Gray
Write-Host "  • Download from: https://github.com/PowerShell/PowerShell/releases" -ForegroundColor Gray
Write-Host "  • Verify with: pwsh --version" -ForegroundColor Gray
Write-Host "`nIntegration with Git and GitHub CLI:" -ForegroundColor Gray
Write-Host "  • Run: . .\load-bus-buddy-profiles.ps1 -ConfigureGit" -ForegroundColor Gray
Write-Host "`nAutomatic profile loading in any terminal:" -ForegroundColor Gray
Write-Host "  • Run: . .\load-bus-buddy-profiles.ps1 -ConfigureGlobal" -ForegroundColor Gray

return $configCorrect
