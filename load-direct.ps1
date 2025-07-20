#Requires -Version 7.0
<#
.SYNOPSIS
    Direct Bus Buddy PowerShell Profile Loader

.DESCRIPTION
    This script directly loads the Bus Buddy PowerShell profiles using absolute paths,
    bypassing any issues with VS Code terminal integration. It now uses the persistent
    profile helper to ensure profile state persists across terminal clears.

    Administrator privileges are recommended but not required.
    Some advanced features may be limited without administrator privileges.

.NOTES
    Version: 1.3
    Date: July 19, 2025
    Author: BusBuddy Team

.EXAMPLE
    # Load profiles
    . .\load-direct.ps1
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [switch]$Quiet
)

$projectRoot = $PSScriptRoot

# Try to load via persistent helper first
$persistentHelperPath = Join-Path $projectRoot "persistent-profile-helper.ps1"
if (Test-Path $persistentHelperPath) {
    try {
        if (-not $Quiet) {
            Write-Host "🔄 Loading Bus Buddy profiles via persistent helper..." -ForegroundColor Cyan
        }

        # Source the persistent helper
        . $persistentHelperPath -Quiet:$Quiet

        # If we get here, the helper worked
        if (-not $Quiet -and $env:BusBuddyProfilesLoaded -eq "true") {
            Write-Host "✅ Bus Buddy profiles loaded via persistent helper" -ForegroundColor Green
            Write-Host "💡 Profile state will persist across terminal clears" -ForegroundColor Yellow

            # Display admin status
            if ($env:BusBuddyAdminMode -eq "true") {
                Write-Host "🔐 Running in administrator mode - all features enabled" -ForegroundColor Green
            } else {
                Write-Host "⚠️ Not running in administrator mode - some features may be limited" -ForegroundColor Yellow
                Write-Host "💡 Restart VS Code as administrator for full functionality" -ForegroundColor Yellow
            }
        }

        return $true
    } catch {
        if (-not $Quiet) {
            Write-Host "⚠️ Failed to load via persistent helper: $_" -ForegroundColor Yellow
            Write-Host "💡 Falling back to direct loading..." -ForegroundColor Yellow
        }
        # Continue to fallback method
    }
}

# Fallback to direct loading if persistent helper failed or doesn't exist
$mainProfilePath = Join-Path $projectRoot "BusBuddy-PowerShell-Profile.ps1"
$advancedWorkflowsPath = Join-Path $projectRoot ".vscode\BusBuddy-Advanced-Workflows.ps1"

if (-not $Quiet) {
    Write-Host "🚌 Loading Bus Buddy PowerShell profiles directly..." -ForegroundColor Cyan
}

# Load main profile
if (Test-Path $mainProfilePath) {
    try {
        . $mainProfilePath
        if (-not $Quiet) {
            Write-Host "✅ Loaded main profile: $mainProfilePath" -ForegroundColor Green
        }
    } catch {
        Write-Host "❌ Failed to load main profile: $_" -ForegroundColor Red
        return $false
    }
} else {
    Write-Host "❌ Main profile not found: $mainProfilePath" -ForegroundColor Red
    return $false
}

# Load advanced workflows
if (Test-Path $advancedWorkflowsPath) {
    try {
        # Set a quiet flag that the script will use
        $Script:BusBuddyAdvancedWorkflowsQuiet = $Quiet

        # Dot source using Invoke-Expression to ensure the entire script is executed
        Invoke-Expression (". '$advancedWorkflowsPath' -Quiet:`$$Quiet")

        if (-not $Quiet) {
            Write-Host "✅ Loaded advanced workflows: $advancedWorkflowsPath" -ForegroundColor Green
        }
    } catch {
        Write-Host "⚠️ Failed to load advanced workflows: $_" -ForegroundColor Yellow
        return $false
    }
} else {
    Write-Host "⚠️ Advanced workflows not found: $advancedWorkflowsPath" -ForegroundColor Yellow
}

if (-not $Quiet) {
    Write-Host "`n🚌 Bus Buddy PowerShell profiles loaded directly!" -ForegroundColor Green
    Write-Host "💡 You should now have access to all Bus Buddy commands" -ForegroundColor Yellow
    Write-Host "⚠️ Note: Profile state may not persist across terminal clears" -ForegroundColor Yellow
    Write-Host "💡 Run fix-terminal-config.ps1 to fix terminal persistence" -ForegroundColor Yellow

    # Check and display admin status
    $isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    if ($isAdmin) {
        $env:BusBuddyAdminMode = "true"
        Write-Host "🔐 Running in administrator mode - all features enabled" -ForegroundColor Green
    } else {
        $env:BusBuddyAdminMode = "false"
        Write-Host "⚠️ Not running in administrator mode - some features may be limited" -ForegroundColor Yellow
        Write-Host "💡 Restart VS Code as administrator for full functionality" -ForegroundColor Yellow
    }
}

# Set environment variable manually since we're not using the persistent helper
$env:BusBuddyProfilesLoaded = "true"

return $true
