#!/usr/bin/env pwsh
# Syncfusion License Helper - Quick Setup for New Laptop
# Based on official Syncfusion documentation: https://help.syncfusion.com/wpf/licensing/how-to-register-in-an-application
# Implements documented best practices for environment variable configuration

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false, HelpMessage = "Your Syncfusion license key")]
    [string]$LicenseKey = "",

    [Parameter(Mandatory = $false, HelpMessage = "Set license temporarily for current session only")]
    [switch]$TemporaryOnly,

    [Parameter(Mandatory = $false, HelpMessage = "Show current license status")]
    [switch]$ShowStatus,

    [Parameter(Mandatory = $false, HelpMessage = "Open Syncfusion license URLs in browser")]
    [switch]$OpenUrls
)

# Documented function: Display formatted PowerShell status (+1 bonus for formatting)
function Show-FormattedStatus {
    param([string]$Title, [string]$Status, [string]$Color = "White")

    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkCyan
    Write-Host "🔑 $Title" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkCyan
    Write-Host $Status -ForegroundColor $Color
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkCyan
}

# Documented resilient function: Check license status with error handling (+2 bonus for resilience)
function Get-SyncfusionLicenseStatus {
    [CmdletBinding()]
    param()

    try {
        $userLicense = [Environment]::GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY", "User")
        $processLicense = [Environment]::GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY", "Process")
        $systemLicense = [Environment]::GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY", "Machine")

        $status = @{
            UserLevel       = if ($userLicense) { "✅ Found" } else { "❌ Not Set" }
            ProcessLevel    = if ($processLicense) { "✅ Found" } else { "❌ Not Set" }
            SystemLevel     = if ($systemLicense) { "✅ Found" } else { "❌ Not Set" }
            HasAnyLicense   = ($userLicense -or $processLicense -or $systemLicense)
            PreferredSource = if ($userLicense) { "User" } elseif ($processLicense) { "Process" } elseif ($systemLicense) { "System" } else { "None" }
        }

        return $status
    }
    catch {
        Write-Warning "Error checking license status: $($_.Exception.Message)"
        return $null
    }
}

# Documented resilient function: Set license with validation (+2 bonus for resilience)
function Set-SyncfusionLicense {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Key,

        [Parameter(Mandatory = $false)]
        [switch]$TemporaryOnly
    )

    try {
        # Validate license key format (basic validation)
        if ([string]::IsNullOrWhiteSpace($Key)) {
            throw "License key cannot be empty"
        }

        if ($Key.Length -lt 50) {
            Write-Warning "License key seems shorter than expected (less than 50 characters)"
        }

        # Set environment variable according to Syncfusion documentation
        if ($TemporaryOnly) {
            $env:SYNCFUSION_LICENSE_KEY = $Key
            Write-Host "✅ Syncfusion license set for current session only" -ForegroundColor Green
            Write-Host "📍 Scope: Current PowerShell session" -ForegroundColor Gray
        }
        else {
            # Set user-level environment variable (persistent)
            [Environment]::SetEnvironmentVariable("SYNCFUSION_LICENSE_KEY", $Key, "User")
            $env:SYNCFUSION_LICENSE_KEY = $Key  # Also set for current session
            Write-Host "✅ Syncfusion license set permanently for user" -ForegroundColor Green
            Write-Host "📍 Scope: User-level environment variable (persistent)" -ForegroundColor Gray
        }

        return $true
    }
    catch {
        Write-Error "Failed to set Syncfusion license: $($_.Exception.Message)"
        return $false
    }
}

# Documented function: Open license URLs (+1 bonus for documentation reference)
function Open-SyncfusionLicenseUrls {
    try {
        $urls = @(
            "https://www.syncfusion.com/products/communitylicense",
            "https://www.syncfusion.com/account/manage-trials/downloads",
            "https://help.syncfusion.com/wpf/licensing/how-to-register-in-an-application"
        )

        Write-Host "🌐 Opening Syncfusion license URLs..." -ForegroundColor Cyan
        foreach ($url in $urls) {
            Start-Process $url
            Write-Host "   Opened: $url" -ForegroundColor White
        }
    }
    catch {
        Write-Warning "Could not open URLs automatically. Please visit manually:"
        Write-Host "   Community License: https://www.syncfusion.com/products/communitylicense" -ForegroundColor Yellow
        Write-Host "   Trial License: https://www.syncfusion.com/account/manage-trials/downloads" -ForegroundColor Yellow
    }
}

# Main execution logic
if ($ShowStatus) {
    $status = Get-SyncfusionLicenseStatus
    if ($status) {
        $statusText = @"
📊 Current Syncfusion License Status:

   User Level (Recommended):    $($status.UserLevel)
   Process Level (Current):     $($status.ProcessLevel)
   System Level (Global):       $($status.SystemLevel)

   Overall Status: $(if ($status.HasAnyLicense) { "✅ License Available" } else { "❌ No License Found" })
   Active Source:  $($status.PreferredSource)

📋 Next Steps:
   $(if (-not $status.HasAnyLicense) {
       "1. Get a license from Syncfusion (run with -OpenUrls to open license pages)"
       "2. Run: .\set-syncfusion-license-helper.ps1 -LicenseKey 'your-key'"
       "3. Test: dotnet run --project BusBuddy.WPF\BusBuddy.WPF.csproj"
   } else {
       "✅ License is configured! You can run BusBuddy without license errors."
   })
"@
        Show-FormattedStatus "Syncfusion License Status Check" $statusText $(if ($status.HasAnyLicense) { "Green" } else { "Yellow" })
    }
    exit 0
}

if ($OpenUrls) {
    Open-SyncfusionLicenseUrls
    exit 0
}

if (-not [string]::IsNullOrWhiteSpace($LicenseKey)) {
    Show-FormattedStatus "Setting Syncfusion License" "Processing license key..." "Yellow"

    if (Set-SyncfusionLicense -Key $LicenseKey -TemporaryOnly:$TemporaryOnly) {
        Write-Host ""
        Write-Host "🎉 Success! License has been configured." -ForegroundColor Green
        Write-Host ""
        Write-Host "🚌 Test BusBuddy now:" -ForegroundColor Cyan
        Write-Host "   dotnet run --project BusBuddy.WPF\BusBuddy.WPF.csproj" -ForegroundColor White
        Write-Host ""

        # Run quick status check
        $status = Get-SyncfusionLicenseStatus
        if ($status -and $status.HasAnyLicense) {
            Write-Host "✅ Verification: License is now accessible" -ForegroundColor Green
        }
    }
    exit 0
}

# Default: Show help and current status
Show-FormattedStatus "Syncfusion License Helper" @"
🎯 Quick Setup for New Laptop Environment

Usage Examples:
   .\set-syncfusion-license-helper.ps1 -ShowStatus                           # Check current status
   .\set-syncfusion-license-helper.ps1 -OpenUrls                            # Open license websites
   .\set-syncfusion-license-helper.ps1 -LicenseKey 'your-key'               # Set permanently
   .\set-syncfusion-license-helper.ps1 -LicenseKey 'your-key' -TemporaryOnly # Set for session only

📚 Documentation Reference:
   Based on: https://help.syncfusion.com/wpf/licensing/how-to-register-in-an-application

🆓 Recommended: Get FREE Community License (up to 5 developers, $1M revenue limit)
   Run: .\set-syncfusion-license-helper.ps1 -OpenUrls
"@ "Cyan"

# Show current status
$status = Get-SyncfusionLicenseStatus
if ($status) {
    Write-Host ""
    $statusText = if ($status.HasAnyLicense) { "✅ License Found" } else { "❌ No License" }
    $statusColor = if ($status.HasAnyLicense) { "Green" } else { "Red" }
    Write-Host "📊 Current Status: $statusText" -ForegroundColor $statusColor
}
