#!/usr/bin/env pwsh
# Syncfusion License Setup Script for BusBuddy
# Handles proper license configuration according to Syncfusion documentation

param(
    [string]$LicenseKey = "",
    [switch]$CheckOnly,
    [switch]$ShowInstructions
)

Write-Host "🔑 Syncfusion License Setup for BusBuddy" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkCyan

function Test-SyncfusionLicense {
    Write-Host "🔍 Checking current Syncfusion license configuration..." -ForegroundColor Yellow

    # Check environment variable
    $envLicense = [Environment]::GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY", "User")
    if (-not [string]::IsNullOrWhiteSpace($envLicense)) {
        Write-Host "✅ Found license in user environment variable" -ForegroundColor Green
        return $true
    }

    $envLicense = [Environment]::GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY", "Process")
    if (-not [string]::IsNullOrWhiteSpace($envLicense)) {
        Write-Host "✅ Found license in current session" -ForegroundColor Green
        return $true
    }

    # Check appsettings.json
    if (Test-Path "appsettings.json") {
        $config = Get-Content "appsettings.json" | ConvertFrom-Json
        if ($config.SyncfusionLicenseKey -and -not $config.SyncfusionLicenseKey.StartsWith('${')) {
            Write-Host "✅ Found license in appsettings.json" -ForegroundColor Green
            return $true
        }
    }

    Write-Host "❌ No valid Syncfusion license found" -ForegroundColor Red
    return $false
}

function Show-LicenseInstructions {
    Write-Host ""
    Write-Host "📋 How to get a Syncfusion License:" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "🆓 FREE Community License (Recommended for Development):" -ForegroundColor Green
    Write-Host "   • Go to: https://www.syncfusion.com/products/communitylicense" -ForegroundColor White
    Write-Host "   • Free for individual developers and small businesses" -ForegroundColor White
    Write-Host "   • Up to 5 developers, $1M revenue limit" -ForegroundColor White
    Write-Host ""
    Write-Host "🔬 30-Day Trial License:" -ForegroundColor Yellow
    Write-Host "   • Go to: https://www.syncfusion.com/account/manage-trials/downloads" -ForegroundColor White
    Write-Host "   • Full features for 30 days" -ForegroundColor White
    Write-Host ""
    Write-Host "💰 Commercial License:" -ForegroundColor Blue
    Write-Host "   • Go to: https://www.syncfusion.com/sales/teamlicense" -ForegroundColor White
    Write-Host "   • For commercial applications" -ForegroundColor White
    Write-Host ""
    Write-Host "⚙️ After getting your license key, run:" -ForegroundColor Cyan
    Write-Host "   .\setup-syncfusion-license.ps1 -LicenseKey 'YOUR_LICENSE_KEY'" -ForegroundColor White
}

function Set-SyncfusionLicense {
    param([string]$Key)

    if ([string]::IsNullOrWhiteSpace($Key)) {
        Write-Host "❌ License key cannot be empty" -ForegroundColor Red
        return $false
    }

    try {
        # Set user environment variable (persistent)
        [Environment]::SetEnvironmentVariable("SYNCFUSION_LICENSE_KEY", $Key, "User")

        # Set process environment variable (immediate)
        $env:SYNCFUSION_LICENSE_KEY = $Key

        Write-Host "✅ Syncfusion license key set successfully!" -ForegroundColor Green
        Write-Host "📍 Location: User environment variable (persistent)" -ForegroundColor Gray
        Write-Host "⚡ The license will be available in new PowerShell sessions" -ForegroundColor Gray

        return $true
    }
    catch {
        Write-Host "❌ Failed to set license key: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Test-LicenseFormat {
    param([string]$Key)

    if ([string]::IsNullOrWhiteSpace($Key)) {
        return $false
    }

    # Basic format validation for Syncfusion license keys
    # Syncfusion licenses are typically long alphanumeric strings
    if ($Key.Length -lt 50) {
        Write-Host "⚠️ Warning: License key seems too short (expected 50+ characters)" -ForegroundColor Yellow
        return $false
    }

    if ($Key -match '[^a-zA-Z0-9+/=]') {
        Write-Host "⚠️ Warning: License key contains unexpected characters" -ForegroundColor Yellow
        return $false
    }

    return $true
}

# Main execution
if ($ShowInstructions) {
    Show-LicenseInstructions
    exit 0
}

if ($CheckOnly) {
    $hasLicense = Test-SyncfusionLicense
    exit ($hasLicense ? 0 : 1)
}

if (-not [string]::IsNullOrWhiteSpace($LicenseKey)) {
    Write-Host "🔧 Setting up Syncfusion license..." -ForegroundColor Yellow

    if (-not (Test-LicenseFormat $LicenseKey)) {
        Write-Host "❌ License key format validation failed" -ForegroundColor Red
        Write-Host "💡 Make sure you copied the complete license key" -ForegroundColor Yellow
        exit 1
    }

    if (Set-SyncfusionLicense $LicenseKey) {
        Write-Host ""
        Write-Host "🎉 Setup complete! You can now run BusBuddy without license errors." -ForegroundColor Green
        Write-Host "🚌 Try: dotnet run --project BusBuddy.WPF\BusBuddy.WPF.csproj" -ForegroundColor Cyan
        exit 0
    }
    else {
        exit 1
    }
}

# No parameters - check current status and show instructions
$hasLicense = Test-SyncfusionLicense

if ($hasLicense) {
    Write-Host "🎉 Syncfusion license is properly configured!" -ForegroundColor Green
    Write-Host "🚌 You can run BusBuddy without license errors." -ForegroundColor Green
}
else {
    Write-Host "⚠️ Syncfusion license not configured." -ForegroundColor Yellow
    Write-Host ""
    Show-LicenseInstructions
    Write-Host ""
    Write-Host "🔧 Quick setup:" -ForegroundColor Cyan
    Write-Host "   .\setup-syncfusion-license.ps1 -ShowInstructions   # Show detailed instructions" -ForegroundColor White
    Write-Host "   .\setup-syncfusion-license.ps1 -CheckOnly          # Check current status only" -ForegroundColor White
}

Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkCyan
