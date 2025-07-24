#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Dell Inspiron 16 5640 Admin Optimization Launcher

.DESCRIPTION
    Interactive launcher for Dell Inspiron PowerShell 7.6.4 optimizations.
    Leverages administrator privileges for comprehensive system optimization.

.NOTES
    Must be run as Administrator
    Optimized for Dell Inspiron 16 5640 with Intel Core i5-1334U
#>

param(
    [switch]$QuickStart,
    [switch]$Silent
)

Write-Host "🔧 Dell Inspiron 16 5640 Admin Optimization Launcher" -ForegroundColor Cyan
Write-Host "=" * 55 -ForegroundColor Cyan

# Verify we're running as admin
if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Host "❌ This script must be run as Administrator" -ForegroundColor Red
    Write-Host "Right-click PowerShell and select 'Run as administrator'" -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "✅ Running with Administrator privileges" -ForegroundColor Green
Write-Host ""

# System information
$systemInfo = @{
    ComputerModel = (Get-CimInstance -Class Win32_ComputerSystem).Model
    ProcessorName = (Get-CimInstance -Class Win32_Processor).Name
    TotalMemoryGB = [math]::Round((Get-CimInstance -Class Win32_ComputerSystem).TotalPhysicalMemory / 1GB, 2)
    OSVersion = (Get-CimInstance -Class Win32_OperatingSystem).Caption
    PowerShellVersion = $PSVersionTable.PSVersion
}

Write-Host "📊 System Information:" -ForegroundColor Yellow
Write-Host "   Model: $($systemInfo.ComputerModel)" -ForegroundColor White
Write-Host "   CPU: $($systemInfo.ProcessorName)" -ForegroundColor White
Write-Host "   RAM: $($systemInfo.TotalMemoryGB) GB" -ForegroundColor White
Write-Host "   OS: $($systemInfo.OSVersion)" -ForegroundColor White
Write-Host "   PowerShell: $($systemInfo.PowerShellVersion)" -ForegroundColor White
Write-Host ""

if ($QuickStart) {
    Write-Host "🚀 Running Quick PowerShell-only optimization..." -ForegroundColor Cyan
    & ".\optimize-dell-inspiron-ps76.ps1" -PowerShellOnly
    Write-Host "✅ Quick optimization complete!" -ForegroundColor Green
    return
}

# Interactive menu
function Show-OptimizationMenu {
    Write-Host "🎯 Select optimization level:" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "1. 🔧 PowerShell Only" -ForegroundColor White
    Write-Host "   └─ Profile optimization, startup improvements, performance tuning" -ForegroundColor Gray
    Write-Host ""
    Write-Host "2. 🖥️  Hardware Only" -ForegroundColor White
    Write-Host "   └─ CPU settings, memory optimization, NVMe SSD tuning" -ForegroundColor Gray
    Write-Host ""
    Write-Host "3. 🔌 Dell Specific" -ForegroundColor White
    Write-Host "   └─ Driver checks, firmware validation, Dell cleanup" -ForegroundColor Gray
    Write-Host ""
    Write-Host "4. 🚀 Full Optimization" -ForegroundColor White
    Write-Host "   └─ Everything + Windows 11 debloating and service optimization" -ForegroundColor Gray
    Write-Host ""
    Write-Host "5. 📋 WhatIf Mode" -ForegroundColor White
    Write-Host "   └─ Preview all changes without making them" -ForegroundColor Gray
    Write-Host ""
    Write-Host "6. 🧪 Test Current Setup" -ForegroundColor White
    Write-Host "   └─ Run PowerShell 7.6 compatibility tests" -ForegroundColor Gray
    Write-Host ""
    Write-Host "7. ❌ Exit" -ForegroundColor White
    Write-Host ""
}

do {
    Show-OptimizationMenu
    $choice = Read-Host "Enter your choice (1-7)"
    Write-Host ""

    switch ($choice) {
        "1" {
            Write-Host "🔧 Running PowerShell-only optimizations..." -ForegroundColor Cyan
            try {
                & ".\optimize-dell-inspiron-ps76.ps1" -PowerShellOnly
                Write-Host "✅ PowerShell optimization complete!" -ForegroundColor Green
            }
            catch {
                Write-Host "❌ PowerShell optimization failed: $($_.Message)" -ForegroundColor Red
            }
        }
        "2" {
            Write-Host "🖥️ Running hardware optimizations..." -ForegroundColor Cyan
            try {
                & ".\optimize-dell-inspiron-ps76.ps1" -HardwareOnly
                Write-Host "✅ Hardware optimization complete!" -ForegroundColor Green
            }
            catch {
                Write-Host "❌ Hardware optimization failed: $($_.Message)" -ForegroundColor Red
            }
        }
        "3" {
            Write-Host "🔌 Running Dell-specific optimizations..." -ForegroundColor Cyan
            try {
                & ".\optimize-dell-inspiron-ps76.ps1" -DellSpecific
                Write-Host "✅ Dell-specific optimization complete!" -ForegroundColor Green
            }
            catch {
                Write-Host "❌ Dell-specific optimization failed: $($_.Message)" -ForegroundColor Red
            }
        }
        "4" {
            Write-Host "🚀 Running full system optimization..." -ForegroundColor Cyan
            Write-Host "⚠️  WARNING: This will make comprehensive system changes!" -ForegroundColor Yellow
            $confirm = Read-Host "Are you sure you want to proceed? (y/N)"
            if ($confirm -eq 'y' -or $confirm -eq 'Y') {
                try {
                    & ".\optimize-dell-inspiron-ps76.ps1" -FullOptimization
                    Write-Host "✅ Full optimization complete!" -ForegroundColor Green
                }
                catch {
                    Write-Host "❌ Full optimization failed: $($_.Message)" -ForegroundColor Red
                }
            }
            else {
                Write-Host "Operation cancelled." -ForegroundColor Yellow
            }
        }
        "5" {
            Write-Host "📋 Running in WhatIf mode (preview only)..." -ForegroundColor Cyan
            try {
                & ".\optimize-dell-inspiron-ps76.ps1" -FullOptimization -WhatIf
                Write-Host "✅ Preview complete! No changes were made." -ForegroundColor Green
            }
            catch {
                Write-Host "❌ Preview failed: $($_.Message)" -ForegroundColor Red
            }
        }
        "6" {
            Write-Host "🧪 Running PowerShell 7.6 compatibility tests..." -ForegroundColor Cyan
            try {
                if (Test-Path ".\test-powershell76-optimizations.ps1") {
                    & ".\test-powershell76-optimizations.ps1" -QuickTest
                    Write-Host "✅ Compatibility test complete!" -ForegroundColor Green
                }
                else {
                    Write-Host "❌ Test script not found: test-powershell76-optimizations.ps1" -ForegroundColor Red
                }
            }
            catch {
                Write-Host "❌ Compatibility test failed: $($_.Message)" -ForegroundColor Red
            }
        }
        "7" {
            Write-Host "👋 Goodbye!" -ForegroundColor Cyan
            break
        }
        default {
            Write-Host "❌ Invalid choice. Please select 1-7." -ForegroundColor Red
        }
    }

    if ($choice -ne "7" -and -not $Silent) {
        Write-Host ""
        Read-Host "Press Enter to continue"
        Write-Host ""
    }

} while ($choice -ne "7")

Write-Host ""
Write-Host "💡 Recommendations:" -ForegroundColor Yellow
Write-Host "   • Restart PowerShell to apply profile changes" -ForegroundColor White
Write-Host "   • Monitor system performance with Test-DellPerformance" -ForegroundColor White
Write-Host "   • Run periodic compatibility tests to validate optimizations" -ForegroundColor White
Write-Host ""
