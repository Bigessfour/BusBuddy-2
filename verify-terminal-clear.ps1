#Requires -Version 7.0
<#
.SYNOPSIS
    Verify Terminal Clear Functionality

.DESCRIPTION
    This script verifies that the Terminal Clear functionality works correctly
    with the Bus Buddy PowerShell profile integration.

.NOTES
    Version: 1.0
    Date: July 19, 2025
    Author: Bus Buddy Team

.EXAMPLE
    # Verify terminal clear functionality
    .\verify-terminal-clear.ps1
#>

Write-Host "🔍 Verifying Terminal Clear functionality..." -ForegroundColor Cyan

# Test if Clear-Host function is available
$clearHostExists = $null -ne (Get-Command -Name Clear-Host -ErrorAction SilentlyContinue)
if ($clearHostExists) {
    Write-Host "✅ Clear-Host function is available" -ForegroundColor Green

    # Check if it's properly aliased
    $clsAlias = Get-Alias -Name cls -ErrorAction SilentlyContinue
    $clearAlias = Get-Alias -Name clear -ErrorAction SilentlyContinue

    if ($clsAlias -and $clsAlias.Definition -eq "Clear-Host") {
        Write-Host "✅ cls alias is properly configured" -ForegroundColor Green
    } else {
        Write-Host "❌ cls alias is not properly configured" -ForegroundColor Red
    }

    if ($clearAlias -and $clearAlias.Definition -eq "Clear-Host") {
        Write-Host "✅ clear alias is properly configured" -ForegroundColor Green
    } else {
        Write-Host "❌ clear alias is not properly configured" -ForegroundColor Red
    }

    # Show function definition
    Write-Host "`n📋 Current Clear-Host function definition:" -ForegroundColor Cyan
    ${function:Clear-Host}

    Write-Host "`n💡 Testing Clear-Host functionality..." -ForegroundColor Yellow
    Write-Host "This should clear the terminal when executed:" -ForegroundColor Gray
    Write-Host "  Clear-Host" -ForegroundColor White

    Write-Host "`nYou can also test the aliases:" -ForegroundColor Gray
    Write-Host "  cls" -ForegroundColor White
    Write-Host "  clear" -ForegroundColor White

    Write-Host "`n📌 VS Code Terminal Actions:" -ForegroundColor Cyan
    Write-Host "1. Click the trash can icon in the terminal toolbar" -ForegroundColor Gray
    Write-Host "2. Press Ctrl+K in the terminal" -ForegroundColor Gray
    Write-Host "3. Right-click in the terminal and Select-Object 'Clear'" -ForegroundColor Gray

    Write-Host "`n✅ Verification complete! Try the clear methods above." -ForegroundColor Green
} else {
    Write-Host "❌ Clear-Host function is not available" -ForegroundColor Red
    Write-Host "💡 Run the fix-terminal-clear.ps1 script and restart your terminal" -ForegroundColor Yellow
}
