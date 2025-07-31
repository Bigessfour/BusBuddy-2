#Requires -Version 7.0
<#
.SYNOPSIS
Simplified BusBuddy build script for troubleshooting

.DESCRIPTION
Step-by-step build process with error handling and verbose output
#>

param(
    [switch]$Verbose,
    [switch]$SkipProfiles
)

# Set error handling
$ErrorActionPreference = "Stop"

try {
    Write-Host "=== BusBuddy Build Script ===" -ForegroundColor Cyan
    Write-Host "PowerShell Version: $($PSVersionTable.PSVersion)" -ForegroundColor Gray
    Write-Host "Working Directory: $PWD" -ForegroundColor Gray
    Write-Host ""

    # Step 1: Set location
    Write-Host "Step 1: Setting location..." -ForegroundColor Yellow
    Set-Location 'C:\Users\biges\Desktop\BusBuddy\BusBuddy'
    Write-Host "✅ Location set to: $PWD" -ForegroundColor Green
    Write-Host ""

    # Step 2: Check .NET SDK
    Write-Host "Step 2: Checking .NET SDK..." -ForegroundColor Yellow
    $dotnetVersion = dotnet --version 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ .NET SDK Version: $dotnetVersion" -ForegroundColor Green
    } else {
        throw ".NET SDK not found or not working"
    }
    Write-Host ""

    # Step 3: Check solution file
    Write-Host "Step 3: Checking solution file..." -ForegroundColor Yellow
    if (Test-Path 'BusBuddy.sln') {
        Write-Host "✅ BusBuddy.sln found" -ForegroundColor Green
    } else {
        throw "BusBuddy.sln not found"
    }
    Write-Host ""

    # Step 4: Load profiles (optional)
    if (-not $SkipProfiles) {
        Write-Host "Step 4: Checking for BusBuddy profiles..." -ForegroundColor Yellow
        if (Test-Path '.\load-bus-buddy-profiles.ps1') {
            Write-Host "Found load-bus-buddy-profiles.ps1, attempting to load..." -ForegroundColor Cyan
            try {
                & '.\load-bus-buddy-profiles.ps1' -ErrorAction Stop
                Write-Host "✅ Profiles loaded successfully" -ForegroundColor Green

                # Check for bb-build command
                if (Get-Command bb-build -ErrorAction SilentlyContinue) {
                    Write-Host "✅ bb-build command available" -ForegroundColor Green
                    $useBbBuild = $true
                } else {
                    Write-Host "⚠️ bb-build command not found, using dotnet build" -ForegroundColor Yellow
                    $useBbBuild = $false
                }
            } catch {
                Write-Host "❌ Error loading profiles: $($_.Exception.Message)" -ForegroundColor Red
                Write-Host "Continuing with direct dotnet build..." -ForegroundColor Yellow
                $useBbBuild = $false
            }
        } else {
            Write-Host "⚠️ load-bus-buddy-profiles.ps1 not found" -ForegroundColor Yellow
            $useBbBuild = $false
        }
    } else {
        Write-Host "Step 4: Skipping profile loading (SkipProfiles flag set)" -ForegroundColor Yellow
        $useBbBuild = $false
    }
    Write-Host ""

    # Step 5: Build
    Write-Host "Step 5: Building solution..." -ForegroundColor Yellow
    if ($useBbBuild) {
        Write-Host "🔨 Using bb-build from profile..." -ForegroundColor Cyan
        bb-build
    } else {
        Write-Host "🔨 Using direct dotnet build..." -ForegroundColor Cyan
        $verbosityLevel = if ($Verbose) { "normal" } else { "minimal" }
        dotnet build BusBuddy.sln --verbosity $verbosityLevel
    }

    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✅ Build completed successfully!" -ForegroundColor Green
    } else {
        throw "Build failed with exit code: $LASTEXITCODE"
    }

} catch {
    Write-Host ""
    Write-Host "❌ Build failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "PowerShell Version: $($PSVersionTable.PSVersion)" -ForegroundColor Gray
    Write-Host "Working Directory: $PWD" -ForegroundColor Gray
    exit 1
}
