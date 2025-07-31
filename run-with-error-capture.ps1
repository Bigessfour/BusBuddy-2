#Requires -Version 7.5
<#
.SYNOPSIS
    Complete build and error capture workflow for BusBuddy
.DESCRIPTION
    Builds the BusBuddy solution and then runs the error capture system
.EXAMPLE
    .\run-with-error-capture.ps1
#>

param(
    [switch]$AutoFix,
    [int]$TimeoutMinutes = 30
)

$ErrorActionPreference = "Continue"

# Step 1: Build the solution first
Write-Host "🔨 Step 1: Building BusBuddy solution..." -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor DarkGray

& "$PSScriptRoot\build-busbuddy-simple.ps1"
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed with exit code $LASTEXITCODE - cannot proceed with error capture" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Step 2: Run the error capture system
Write-Host ""
Write-Host "🔍 Step 2: Starting BusBuddy error capture system..." -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor DarkGray

$errorCaptureParams = @{
    CaptureDirectory = "logs/runtime-errors"
    TimeoutMinutes = $TimeoutMinutes
}
if ($AutoFix) {
    $errorCaptureParams.Add("AutoFix", $true)
}

& "$PSScriptRoot\PowerShell\Scripts\Interactive-Runtime-Error-Capture.ps1" @errorCaptureParams
