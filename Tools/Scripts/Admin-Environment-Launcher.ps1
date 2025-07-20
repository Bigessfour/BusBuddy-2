#Requires -Version 7.0

<#
.SYNOPSIS
    Administrator Environment Launcher for Bus Buddy Development
.DESCRIPTION
    Launches the Fast-Environment-Update script with administrator privileges
    Provides options for different update scenarios
.EXAMPLE
    .\Admin-Environment-Launcher.ps1
.EXAMPLE
    .\Admin-Environment-Launcher.ps1 -QuickUpdate
.EXAMPLE
    .\Admin-Environment-Launcher.ps1 -FullUpdate
#>

[CmdletBinding()]
param(
    [Parameter()]
    [switch]$QuickUpdate,

    [Parameter()]
    [switch]$FullUpdate,

    [Parameter()]
    [switch]$ForceReinstall
)

$ErrorActionPreference = 'Stop'

Write-Host '🔐 Bus Buddy Administrator Environment Launcher' -ForegroundColor Cyan
Write-Host '===============================================' -ForegroundColor Cyan
Write-Host ''

# Get the script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$FastUpdateScript = Join-Path $ScriptDir 'Fast-Environment-Update.ps1'

# Check if the update script exists
if (-not (Test-Path $FastUpdateScript)) {
    Write-Error "❌ Fast-Environment-Update.ps1 not found at: $FastUpdateScript"
    exit 1
}

# Check current privileges
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")

if (-not $isAdmin) {
    Write-Host '🔧 Elevating to Administrator privileges...' -ForegroundColor Yellow

    # Prepare arguments for elevated execution
    $scriptArgs = @()
    if ($QuickUpdate) { $scriptArgs += '-QuickUpdate' }
    if ($FullUpdate) { $scriptArgs += '-FullUpdate' }
    if ($ForceReinstall) { $scriptArgs += '-ForceReinstall' }

    # Build the command to run elevated
    $argumentList = @(
        '-NoProfile',
        '-ExecutionPolicy', 'Bypass',
        '-File', "`"$FastUpdateScript`"",
        $scriptArgs -join ' '
    )

    try {
        # Launch with elevated privileges
        Start-Process -FilePath 'pwsh' -ArgumentList $argumentList -Verb RunAs -Wait
        Write-Host '✅ Administrator environment update completed!' -ForegroundColor Green
    }
    catch {
        Write-Error "❌ Failed to launch with administrator privileges: $($_.Exception.Message)"
        exit 1
    }
}
else {
    Write-Host '✅ Already running with Administrator privileges' -ForegroundColor Green

    # Build parameters for the script
    $scriptParams = @{}
    if ($ForceReinstall) { $scriptParams['ForceReinstall'] = $true }

    # Execute the update script directly
    try {
        Write-Host '🚀 Launching Fast Environment Update...' -ForegroundColor Cyan
        & $FastUpdateScript @scriptParams
        Write-Host '✅ Environment update completed successfully!' -ForegroundColor Green
    }
    catch {
        Write-Error "❌ Environment update failed: $($_.Exception.Message)"
        exit 1
    }
}

Write-Host ''
Write-Host '🎉 Bus Buddy development environment is ready!' -ForegroundColor Green
Write-Host '   Next steps:' -ForegroundColor White
Write-Host '   1. Restart VS Code to pick up latest extensions' -ForegroundColor Gray
Write-Host '   2. Restart PowerShell terminal to use latest version' -ForegroundColor Gray
Write-Host '   3. Run build tests to verify environment' -ForegroundColor Gray
