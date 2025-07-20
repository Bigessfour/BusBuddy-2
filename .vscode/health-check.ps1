#Requires -Version 7.0
<#
.SYNOPSIS
    Run Bus Buddy health checks and diagnostics

.DESCRIPTION
    This script runs health checks and diagnostics for the Bus Buddy application.
    It safely loads the PowerShell profiles and runs the health checks without
    parameter passing issues.

.NOTES
    Version: 1.0
    Date: July 19, 2025
#>

$ErrorActionPreference = 'Stop'
$VerbosePreference = 'Continue'

# Project root detection
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptPath

# Load profiles using scriptblock to avoid parameter passing issues
try {
    # First set a script-level variable for quiet mode
    $Script:BusBuddyAdvancedWorkflowsQuiet = $true

    # Source the load-bus-buddy-profiles script using direct dot sourcing
    $loaderPath = Join-Path $projectRoot "load-direct.ps1"
    Write-Host "Loading profiles from: $loaderPath" -ForegroundColor Cyan
    . $loaderPath -Quiet

    # Run health check and diagnostic
    Write-Host "`n🏥 Running health checks..." -ForegroundColor Cyan
    bb-health

    Write-Host "`n🔍 Running diagnostics..." -ForegroundColor Cyan
    bb-diagnostic
}
catch {
    Write-Host "❌ Error running health checks: $_" -ForegroundColor Red
    Write-Host "Error details: $($_.Exception)" -ForegroundColor Red
    exit 1
}
