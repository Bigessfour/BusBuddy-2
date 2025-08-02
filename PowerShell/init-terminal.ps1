# BusBuddy Terminal Initialization Script
# This script ensures the PowerShell environment loads correctly every time

# Force set location to BusBuddy workspace
Set-Location "C:\Users\steve.mckitrick\Desktop\BusBuddy"

# Import the BusBuddy module with force reload
Remove-Module BusBuddy -ErrorAction SilentlyContinue
Remove-Module XamlValidation -ErrorAction SilentlyContinue
Import-Module ".\PowerShell\Modules\BusBuddy\BusBuddy.psm1" -Force -DisableNameChecking
Import-Module ".\PowerShell\XamlValidation.psm1" -Force -DisableNameChecking

# Verify validation commands are available
if (Get-Command bb-validate-xml -ErrorAction SilentlyContinue) {
    Write-Host "✅ bb-validate-xml command is available" -ForegroundColor Green
} else {
    Write-Host "❌ bb-validate-xml command is NOT available" -ForegroundColor Red
}

if (Get-Command bb-xaml-validate -ErrorAction SilentlyContinue) {
    Write-Host "✅ bb-xaml-validate command is available" -ForegroundColor Green
} else {
    Write-Host "❌ bb-xaml-validate command is NOT available" -ForegroundColor Red
}

# Show available commands for verification
Write-Host "🚀 Available bb commands:" -ForegroundColor Cyan
Get-Command bb-* | Select-Object Name | Sort-Object Name | Format-Wide -Column 3

Write-Host ""
Write-Host "🎯 Ready for XAML validation!" -ForegroundColor Magenta
Write-Host "   Run: bb-validate-xml <file> to validate a single XAML file" -ForegroundColor Yellow
Write-Host "   Run: bb-xaml-validate to validate ALL XAML files" -ForegroundColor Yellow
Write-Host ""
