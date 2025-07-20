#Requires -Version 7.0
<#
.SYNOPSIS
    Monitor Serilog output during Bus Buddy build

.DESCRIPTION
    This script monitors Serilog output during the Bus Buddy build process.
    It safely loads the PowerShell profiles and runs the build with Serilog
    filtering without parameter passing issues.

.NOTES
    Version: 1.0
    Date: July 19, 2025
#>

$ErrorActionPreference = 'Stop'

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

    # Run build and filter for Serilog output
    Write-Host "`n📝 Building with Serilog monitoring..." -ForegroundColor Cyan
    bb-build | Where-Object { $_ -match 'Serilog|ERROR|WARN' }
}
catch {
    Write-Host "❌ Error during Serilog monitoring: $_" -ForegroundColor Red
    Write-Host "Error details: $($_.Exception)" -ForegroundColor Red
    exit 1
}
