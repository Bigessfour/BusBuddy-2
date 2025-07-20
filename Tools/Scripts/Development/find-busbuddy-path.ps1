#Requires -Version 7.0
<#
.SYNOPSIS
    Find Bus Buddy Project Directory
.DESCRIPTION
    This script helps find the correct path to the Bus Buddy project directory
.NOTES
    Run this from any PowerShell window to find the correct path
#>

Write-Host "=== Finding Bus Buddy Project Directory ===" -ForegroundColor Cyan
Write-Host ""

# Common possible locations
$possiblePaths = @(
    "C:\Users\$env:USERNAME\Desktop\Bus Buddy",
    "C:\Users\$env:USERNAME\Desktop\BusBuddy",
    "C:\Users\$env:USERNAME\Desktop\Bus-Buddy",
    "C:\Users\$env:USERNAME\Documents\Bus Buddy",
    "C:\Users\$env:USERNAME\Documents\BusBuddy",
    "C:\Users\$env:USERNAME\Source\Bus Buddy",
    "C:\Users\$env:USERNAME\Source\Repos\Bus Buddy",
    "C:\Users\$env:USERNAME\Source\Repos\BusBuddy"
)

Write-Host "Checking possible locations for Bus Buddy project..." -ForegroundColor Yellow
Write-Host ""

$found = $false
foreach ($path in $possiblePaths) {
    if (Test-Path $path) {
        # Check if it's actually the Bus Buddy project by looking for the solution file
        $solutionFile = Join-Path $path "BusBuddy.sln"
        if (Test-Path $solutionFile) {
            Write-Host "✅ FOUND Bus Buddy project at:" -ForegroundColor Green
            Write-Host "   $path" -ForegroundColor Cyan
            Write-Host ""
            Write-Host "📋 Copy and paste this command in PowerShell:" -ForegroundColor Yellow
            Write-Host "   Set-Location '$path'" -ForegroundColor White
            Write-Host ""
            Write-Host "📋 Then run the fix script:" -ForegroundColor Yellow
            Write-Host "   .\fix-powershell-extension.ps1" -ForegroundColor White
            $found = $true
            break
        } else {
            Write-Host "⚠️ Directory exists but no BusBuddy.sln found: $path" -ForegroundColor Yellow
        }
    }
}

if (-not $found) {
    Write-Host "❌ Bus Buddy project not found in common locations" -ForegroundColor Red
    Write-Host ""
    Write-Host "🔍 Manual search suggestions:" -ForegroundColor Yellow
    Write-Host "1. Search for 'BusBuddy.sln' in File Explorer" -ForegroundColor White
    Write-Host "2. Or run this command to search your entire C: drive:" -ForegroundColor White
    Write-Host "   Get-ChildItem -Path C:\ -Name 'BusBuddy.sln' -Recurse -ErrorAction SilentlyContinue" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "3. Or search just your user directory:" -ForegroundColor White
    Write-Host "   Get-ChildItem -Path C:\Users\$env:USERNAME -Name 'BusBuddy.sln' -Recurse -ErrorAction SilentlyContinue" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "Current PowerShell location: $PWD" -ForegroundColor Cyan
