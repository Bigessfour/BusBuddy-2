#Requires -Version 5.1
<#
.SYNOPSIS
    Administrator Launcher for Dell Inspiron PowerShell 7.6.4 Optimizer

.DESCRIPTION
    This script checks for administrator privileges and launches the optimization script.
    If not running as admin, it will prompt to restart with elevated privileges.

.NOTES
    Run this script to automatically launch the optimizer with proper privileges.
#>

param(
    [switch]$FullOptimization,
    [switch]$PowerShellOnly,
    [switch]$HardwareOnly,
    [switch]$DellSpecific,
    [switch]$SkipDriverUpdates,
    [switch]$WhatIf
)

# Check if running as Administrator
function Test-IsAdmin {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]$currentUser
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

Write-Host "🚀 Dell Inspiron PowerShell 7.6.4 Optimizer Launcher" -ForegroundColor Cyan
Write-Host "=" * 55 -ForegroundColor Cyan

$optimizerScript = Join-Path $PSScriptRoot "optimize-dell-inspiron-ps76.ps1"

if (-not (Test-Path $optimizerScript)) {
    Write-Host "❌ Optimizer script not found: $optimizerScript" -ForegroundColor Red
    Write-Host "   Make sure 'optimize-dell-inspiron-ps76.ps1' is in the same directory." -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

if (Test-IsAdmin) {
    Write-Host "✅ Running with Administrator privileges" -ForegroundColor Green
    Write-Host "🔄 Launching Dell Inspiron Optimizer..." -ForegroundColor Cyan
    Write-Host ""

    # Build parameter string
    $params = @()
    if ($FullOptimization) { $params += "-FullOptimization" }
    if ($PowerShellOnly) { $params += "-PowerShellOnly" }
    if ($HardwareOnly) { $params += "-HardwareOnly" }
    if ($DellSpecific) { $params += "-DellSpecific" }
    if ($SkipDriverUpdates) { $params += "-SkipDriverUpdates" }
    if ($WhatIf) { $params += "-WhatIf" }

    $paramString = $params -join " "

    try {
        # Execute the optimizer script
        if ($paramString) {
            & $optimizerScript @params
        } else {
            & $optimizerScript
        }

        Write-Host "`n✅ Optimization completed!" -ForegroundColor Green
        Write-Host "💡 Restart PowerShell to apply profile changes." -ForegroundColor Yellow
    }
    catch {
        Write-Host "`n❌ Optimization failed: $($_.Message)" -ForegroundColor Red
    }

    Write-Host "`nPress Enter to exit..." -ForegroundColor Gray
    Read-Host
}
else {
    Write-Host "⚠️ Not running as Administrator" -ForegroundColor Yellow
    Write-Host "🔐 Administrator privileges are required for:" -ForegroundColor Cyan
    Write-Host "   • Power plan optimizations" -ForegroundColor White
    Write-Host "   • System service configuration" -ForegroundColor White
    Write-Host "   • Registry modifications" -ForegroundColor White
    Write-Host "   • Driver and firmware checks" -ForegroundColor White
    Write-Host "   • Windows bloatware removal" -ForegroundColor White
    Write-Host ""

    $restart = Read-Host "Would you like to restart as Administrator? (Y/n)"

    if ($restart -eq '' -or $restart -eq 'Y' -or $restart -eq 'y') {
        Write-Host "🚀 Restarting with Administrator privileges..." -ForegroundColor Green

        # Build argument list
        $arguments = @("-File", "`"$($MyInvocation.MyCommand.Path)`"")
        if ($FullOptimization) { $arguments += "-FullOptimization" }
        if ($PowerShellOnly) { $arguments += "-PowerShellOnly" }
        if ($HardwareOnly) { $arguments += "-HardwareOnly" }
        if ($DellSpecific) { $arguments += "-DellSpecific" }
        if ($SkipDriverUpdates) { $arguments += "-SkipDriverUpdates" }
        if ($WhatIf) { $arguments += "-WhatIf" }

        try {
            Start-Process PowerShell -ArgumentList $arguments -Verb RunAs
            exit 0
        }
        catch {
            Write-Host "❌ Failed to restart as Administrator: $($_.Message)" -ForegroundColor Red
            Write-Host "💡 Please manually run PowerShell as Administrator and execute:" -ForegroundColor Yellow
            Write-Host "   .\optimize-dell-inspiron-ps76.ps1" -ForegroundColor White
            Read-Host "Press Enter to exit"
            exit 1
        }
    }
    else {
        Write-Host "❌ Optimization cancelled. Administrator privileges are required." -ForegroundColor Red
        Write-Host "💡 To run manually:" -ForegroundColor Yellow
        Write-Host "   1. Right-click PowerShell" -ForegroundColor White
        Write-Host "   2. Select 'Run as Administrator'" -ForegroundColor White
        Write-Host "   3. Navigate to: $PSScriptRoot" -ForegroundColor White
        Write-Host "   4. Run: .\optimize-dell-inspiron-ps76.ps1" -ForegroundColor White
        Read-Host "Press Enter to exit"
        exit 0
    }
}
