#Requires -Version 7.0
<#
.SYNOPSIS
    Ensures the PowerShell session is running with administrator privileges

.DESCRIPTION
    This script checks if the current PowerShell session is running with administrator
    privileges. If not, it restarts the session with elevated privileges.

.NOTES
    Version: 1.0
    Date: July 19, 2025
    Author: Bus Buddy Team

.EXAMPLE
    # Check and elevate if needed
    .\ensure-admin-privileges.ps1
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [switch]$Force,

    [Parameter(Mandatory = $false)]
    [switch]$Quiet
)

function Test-AdminPrivileges {
    $isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    return $isAdmin
}

# Check if already running as admin
$isAdmin = Test-AdminPrivileges
if ($isAdmin -and -not $Force) {
    if (-not $Quiet) {
        Write-Host "✅ Already running with administrator privileges" -ForegroundColor Green
    }
    # Set environment variable
    $env:BusBuddyAdminMode = "true"
    return $true
}

# Not running as admin, or forced elevation
if (-not $Quiet) {
    Write-Host "🔐 Elevating to administrator privileges..." -ForegroundColor Cyan
}

try {
    # Get current script path
    $scriptPath = $MyInvocation.MyCommand.Definition
    $workspaceRoot = Split-Path -Parent $scriptPath

    # Prepare command to run after elevation
    $afterElevationScript = Join-Path $workspaceRoot "persistent-profile-helper.ps1"

    # Start new elevated PowerShell process
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = "pwsh.exe"
    $psi.Arguments = "-NoProfile -NoExit -Command ""& '$afterElevationScript'"""
    $psi.Verb = "RunAs"
    $psi.WorkingDirectory = $workspaceRoot

    # Start the process
    [System.Diagnostics.Process]::Start($psi) | Out-Null

    if (-not $Quiet) {
        Write-Host "✅ Launched elevated PowerShell session" -ForegroundColor Green
        Write-Host "💡 You can close this non-elevated session" -ForegroundColor Yellow
    }

    # Exit current session if not in VS Code (to avoid closing VS Code terminal)
    if ($host.Name -ne 'Visual Studio Code Host') {
        exit
    }

    return $true
}
catch {
    Write-Host "❌ Failed to elevate session: $_" -ForegroundColor Red
    return $false
}
