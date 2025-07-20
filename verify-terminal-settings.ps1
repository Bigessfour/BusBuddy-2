#Requires -Version 7.0
<#
.SYNOPSIS
    VS Code Terminal Configuration Verifier

.DESCRIPTION
    Verifies and corrects VS Code terminal configuration for Bus Buddy development.
    This script checks the settings.json file and ensures the proper PowerShell 7.5.2
    terminal profile is configured to automatically load Bus Buddy profiles.

.NOTES
    Version: 1.0
    Date: July 19, 2025
    Author: BusBuddy Team

.EXAMPLE
    .\verify-terminal-settings.ps1

.EXAMPLE
    .\verify-terminal-settings.ps1 -Fix
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [switch]$Fix,

    [Parameter(Mandatory = $false)]
    [switch]$Backup
)

Write-Host "🔍 Verifying VS Code Terminal Configuration..." -ForegroundColor Cyan

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

        # Check for .vscode directory
        $vscodePath = Join-Path $currentPath ".vscode"
        if (Test-Path $vscodePath) {
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

$projectRoot = Find-BusBuddyProjectRoot
if (-not $projectRoot) {
    Write-Host "❌ Bus Buddy project root not found. Are you in the correct directory?" -ForegroundColor Red
    return $false
}

# Check for .vscode directory
$vscodeDir = Join-Path $projectRoot ".vscode"
if (-not (Test-Path $vscodeDir)) {
    Write-Host "❌ .vscode directory not found in project root: $projectRoot" -ForegroundColor Red

    if ($Fix) {
        Write-Host "📁 Creating .vscode directory..." -ForegroundColor Yellow
        try {
            New-Item -ItemType Directory -Path $vscodeDir -Force | Out-Null
            Write-Host "✅ Created .vscode directory" -ForegroundColor Green
        } catch {
            Write-Host "❌ Failed to create .vscode directory: $_" -ForegroundColor Red
            return $false
        }
    } else {
        Write-Host "💡 Run with -Fix parameter to create the .vscode directory" -ForegroundColor Yellow
        return $false
    }
}

# Check for settings.json
$settingsPath = Join-Path $vscodeDir "settings.json"
$settingsExists = Test-Path $settingsPath

# Expected terminal configuration
$expectedTerminalConfig = @{
    "terminal.integrated.defaultProfile.windows" = "PowerShell 7.5.2"
    "terminal.integrated.profiles.windows" = @{
        "PowerShell 7.5.2" = @{
            "path" = "pwsh.exe"
            "args" = @(
                "-NoProfile"
                "-NoExit"
                "-Command"
                "& '`${workspaceFolder}\\BusBuddy-PowerShell-Profile.ps1'; & '`${workspaceFolder}\\.vscode\\BusBuddy-Advanced-Workflows.ps1'"
            )
            "icon" = "terminal-powershell"
        }
    }
}

# If settings.json exists, check terminal configuration
$settingsContent = $null
$currentConfig = $null
$needsUpdate = $false

if ($settingsExists) {
    try {
        $settingsContent = Get-Content $settingsPath -Raw | ConvertFrom-Json

        # Check if terminal configuration exists
        $hasDefaultProfile = $null -ne $settingsContent.'terminal.integrated.defaultProfile.windows'
        $hasProfiles = $null -ne $settingsContent.'terminal.integrated.profiles.windows'
        $hasPwshProfile = $hasProfiles -and ($null -ne $settingsContent.'terminal.integrated.profiles.windows'.'PowerShell 7.5.2')

        if (-not $hasDefaultProfile -or -not $hasPwshProfile) {
            Write-Host "⚠️ Terminal configuration missing or incomplete in settings.json" -ForegroundColor Yellow
            $needsUpdate = $true
        } else {
            # Verify PowerShell 7.5.2 configuration
            $pwshProfile = $settingsContent.'terminal.integrated.profiles.windows'.'PowerShell 7.5.2'
            $correctPath = $pwshProfile.path -eq "pwsh.exe"
            $hasCorrectArgs = $false

            if ($pwshProfile.args -and $pwshProfile.args.Count -ge 4) {
                $commandArg = $pwshProfile.args[3]
                $hasCorrectArgs = $commandArg -match "BusBuddy-PowerShell-Profile" -and $commandArg -match "BusBuddy-Advanced-Workflows"
            }

            if (-not $correctPath -or -not $hasCorrectArgs) {
                Write-Host "⚠️ PowerShell 7.5.2 terminal profile is not correctly configured" -ForegroundColor Yellow
                $needsUpdate = $true
            } else {
                Write-Host "✅ Terminal configuration is correct in settings.json" -ForegroundColor Green
            }
        }
    } catch {
        Write-Host "❌ Failed to parse settings.json: $_" -ForegroundColor Red
        $needsUpdate = $true
    }
} else {
    Write-Host "⚠️ settings.json not found in .vscode directory" -ForegroundColor Yellow
    $needsUpdate = $true
}

# Fix settings if needed
if ($needsUpdate -and $Fix) {
    Write-Host "🔧 Updating VS Code terminal configuration..." -ForegroundColor Yellow

    # Backup existing settings if requested
    if ($Backup -and $settingsExists) {
        $backupPath = "$settingsPath.bak"
        try {
            Copy-Item -Path $settingsPath -Destination $backupPath -Force
            Write-Host "✅ Backed up existing settings to: $backupPath" -ForegroundColor Green
        } catch {
            Write-Host "❌ Failed to create backup: $_" -ForegroundColor Red
        }
    }

    # Create or update settings.json
    try {
        if ($settingsExists -and $settingsContent) {
            # Merge settings
            $settingsContent | Add-Member -NotePropertyName "terminal.integrated.defaultProfile.windows" -NotePropertyValue $expectedTerminalConfig["terminal.integrated.defaultProfile.windows"] -Force
            $settingsContent | Add-Member -NotePropertyName "terminal.integrated.profiles.windows" -NotePropertyValue $expectedTerminalConfig["terminal.integrated.profiles.windows"] -Force

            $settingsContent | ConvertTo-Json -Depth 10 | Set-Content -Path $settingsPath -Force
        } else {
            # Create new settings file with terminal configuration
            $expectedTerminalConfig | ConvertTo-Json -Depth 10 | Set-Content -Path $settingsPath -Force
        }

        Write-Host "✅ Updated VS Code terminal configuration in settings.json" -ForegroundColor Green
    } catch {
        Write-Host "❌ Failed to update settings.json: $_" -ForegroundColor Red
        return $false
    }
} elseif ($needsUpdate) {
    Write-Host "💡 Run with -Fix parameter to update terminal configuration" -ForegroundColor Yellow
}

# Check if pwsh.exe exists
try {
    $pwshPath = Get-Command "pwsh.exe" -ErrorAction Stop | Select-Object -ExpandProperty Source
    Write-Host "✅ PowerShell 7.5.2 (pwsh.exe) found at: $pwshPath" -ForegroundColor Green
} catch {
    Write-Host "❌ PowerShell 7.5.2 (pwsh.exe) not found in PATH" -ForegroundColor Red
    Write-Host "💡 Install PowerShell 7.5.2 from: https://github.com/PowerShell/PowerShell/releases" -ForegroundColor Yellow
}

# Verify profile paths
$mainProfilePath = Join-Path $projectRoot "BusBuddy-PowerShell-Profile.ps1"
$advancedWorkflowsPath = Join-Path $projectRoot ".vscode\BusBuddy-Advanced-Workflows.ps1"

if (-not (Test-Path $mainProfilePath)) {
    Write-Host "❌ Main profile not found: $mainProfilePath" -ForegroundColor Red
} else {
    Write-Host "✅ Main profile found: $mainProfilePath" -ForegroundColor Green
}

if (-not (Test-Path $advancedWorkflowsPath)) {
    Write-Host "❌ Advanced workflows not found: $advancedWorkflowsPath" -ForegroundColor Red
} else {
    Write-Host "✅ Advanced workflows found: $advancedWorkflowsPath" -ForegroundColor Green
}

Write-Host "`n📋 Terminal Verification Summary:" -ForegroundColor Cyan
if ($needsUpdate -and -not $Fix) {
    Write-Host "⚠️ Terminal configuration needs updating. Run with -Fix parameter to apply changes." -ForegroundColor Yellow
} elseif ($needsUpdate -and $Fix) {
    Write-Host "✅ Terminal configuration has been updated. Restart VS Code for changes to take effect." -ForegroundColor Green
} else {
    Write-Host "✅ Terminal configuration is correct. Bus Buddy profiles should load automatically." -ForegroundColor Green
}

Write-Host "`n💡 How to verify terminal setup:" -ForegroundColor Yellow
Write-Host "  1. Close and reopen VS Code" -ForegroundColor Gray
Write-Host "  2. Open a new terminal (Ctrl+`) - it should load profiles automatically" -ForegroundColor Gray
Write-Host "  3. Type 'bb-health' to check if Bus Buddy commands are available" -ForegroundColor Gray
Write-Host "  4. If commands aren't available, run: . .\load-bus-buddy-profiles.ps1" -ForegroundColor Gray

return (-not $needsUpdate) -or ($needsUpdate -and $Fix)
