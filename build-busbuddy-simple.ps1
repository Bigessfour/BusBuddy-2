#Requires -Version 7.5
<#
.SYNOPSIS
    Simple build script for BusBuddy
.DESCRIPTION
    Builds the BusBuddy solution with error handling, ensuring the core DLL is available
.PARAMETER SkipProfiles
    Skip loading PowerShell profiles for a clean build
.PARAMETER Verbose
    Show verbose output during build process
.EXAMPLE
    .\build-busbuddy-simple.ps1 -Verbose
#>

param(
    [switch]$SkipProfiles,
    [switch]$Verbose
)

# Setup
$ErrorActionPreference = "Continue"
$VerbosityLevel = if ($Verbose) { "normal" } else { "quiet" }
$StartTime = Get-Date

function Write-Status {
    param([string]$Message, [string]$Color = "White")
    Write-Host $Message -ForegroundColor $Color
}

Write-Status "🔨 Building BusBuddy solution..." -Color Cyan

# 1. Clean solution first
Write-Status "   Cleaning solution..." -Color Gray
dotnet clean BusBuddy.sln --verbosity $VerbosityLevel
if ($LASTEXITCODE -ne 0) {
    Write-Status "❌ Clean failed with exit code $LASTEXITCODE" -Color Red
    exit $LASTEXITCODE
}

# 2. Restore packages
Write-Status "   Restoring packages..." -Color Gray
dotnet restore BusBuddy.sln --verbosity $VerbosityLevel
if ($LASTEXITCODE -ne 0) {
    Write-Status "❌ Package restore failed with exit code $LASTEXITCODE" -Color Red
    exit $LASTEXITCODE
}

# 3. Build solution
Write-Status "   Building solution..." -Color Gray
dotnet build BusBuddy.sln --verbosity $VerbosityLevel --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Status "❌ Build failed with exit code $LASTEXITCODE" -Color Red
    exit $LASTEXITCODE
}

# 4. Verify core DLL exists - search in multiple possible locations
$possiblePaths = @(
    "BusBuddy.Core\bin\Debug\net8.0\BusBuddy.Core.dll",
    ".\BusBuddy.Core\bin\Debug\net8.0\BusBuddy.Core.dll",
    "..\BusBuddy.Core\bin\Debug\net8.0\BusBuddy.Core.dll"
)

$dllExists = $false
$foundPath = ""

foreach ($path in $possiblePaths) {
    if (Test-Path $path) {
        $dllExists = $true
        $foundPath = $path
        break
    }
}

if ($dllExists) {
    Write-Status "✅ Build completed successfully - Core DLL found at: $foundPath" -Color Green
}
else {
    Write-Status "❓ Looking for DLL in all bin directories..." -Color Yellow
    # Search for the DLL in all bin directories
    $searchResults = Get-ChildItem -Path . -Recurse -Filter "BusBuddy.Core.dll" -ErrorAction SilentlyContinue
    if ($searchResults.Count -gt 0) {
        Write-Status "✅ Found Core DLL at: $($searchResults[0].FullName)" -Color Green
        $dllExists = $true
    }
    else {
        Write-Status "❌ Build completed but Core DLL not found!" -Color Red
        Write-Status "   Tried paths: $($possiblePaths -join ', ')" -Color Yellow
        exit 1
    }
}

$duration = (Get-Date) - $StartTime
Write-Status "⏱️ Build duration: $($duration.TotalSeconds.ToString('0.00')) seconds" -Color Cyan
