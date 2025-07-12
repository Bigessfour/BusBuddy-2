# Secure Environment Setup Script
# This script helps set up environment variables safely without exposing secrets

Write-Host '🔒 BusBuddy Secure Environment Setup' -ForegroundColor Green
Write-Host '=====================================' -ForegroundColor Green

# Function to securely prompt for sensitive input
function Get-SecureInput {
    param(
        [string]$Prompt,
        [string]$VariableName
    )

    Write-Host "`n$Prompt" -ForegroundColor Yellow
    Write-Host "Variable: $VariableName" -ForegroundColor Cyan

    # Use Read-Host with -AsSecureString for security
    $secureInput = Read-Host 'Enter value (input will be hidden)' -AsSecureString
    $plainText = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureInput)
    )

    if (-not [string]::IsNullOrWhiteSpace($plainText)) {
        [Environment]::SetEnvironmentVariable($VariableName, $plainText, 'User')
        Write-Host "✅ $VariableName set successfully" -ForegroundColor Green
        return $true
    } else {
        Write-Host "❌ Empty value - skipping $VariableName" -ForegroundColor Red
        return $false
    }
}

# Check current environment variables
Write-Host "`n🔍 Checking current environment variables..." -ForegroundColor Blue

$xaiKey = $env:XAI_API_KEY
$syncfusionKey = $env:SYNCFUSION_LICENSE_KEY

Write-Host "XAI_API_KEY: $($xaiKey ? '✅ Set' : '❌ Not set')" -ForegroundColor ($xaiKey ? 'Green' : 'Red')
Write-Host "SYNCFUSION_LICENSE_KEY: $($syncfusionKey ? '✅ Set' : '❌ Not set')" -ForegroundColor ($syncfusionKey ? 'Green' : 'Red')

# Prompt for missing or update existing variables
$choice = Read-Host "`nDo you want to (S)et new values, (U)pdate existing, or (Q)uit? [S/U/Q]"

switch ($choice.ToUpper()) {
    'S' {
        Write-Host "`n🔑 Setting up new environment variables..." -ForegroundColor Blue

        if (-not $xaiKey) {
            Get-SecureInput 'Enter your xAI API Key (get from: https://xai.com/api)' 'XAI_API_KEY'
        }

        if (-not $syncfusionKey) {
            Get-SecureInput 'Enter your Syncfusion License Key (get from: https://syncfusion.com/account/downloads)' 'SYNCFUSION_LICENSE_KEY'
        }
    }
    'U' {
        Write-Host "`n🔄 Updating environment variables..." -ForegroundColor Blue

        $updateXai = Read-Host 'Update XAI_API_KEY? [y/N]'
        if ($updateXai.ToUpper() -eq 'Y') {
            Get-SecureInput 'Enter new xAI API Key' 'XAI_API_KEY'
        }

        $updateSyncfusion = Read-Host 'Update SYNCFUSION_LICENSE_KEY? [y/N]'
        if ($updateSyncfusion.ToUpper() -eq 'Y') {
            Get-SecureInput 'Enter new Syncfusion License Key' 'SYNCFUSION_LICENSE_KEY'
        }
    }
    'Q' {
        Write-Host 'Exiting setup...' -ForegroundColor Yellow
        exit 0
    }
    default {
        Write-Host 'Invalid choice. Exiting...' -ForegroundColor Red
        exit 1
    }
}

# Verify setup
Write-Host "`n✅ Verification..." -ForegroundColor Green

$newXaiKey = $env:XAI_API_KEY
$newSyncfusionKey = $env:SYNCFUSION_LICENSE_KEY

if ($newXaiKey) {
    Write-Host "XAI_API_KEY: ✅ Configured (length: $($newXaiKey.Length) chars)" -ForegroundColor Green
} else {
    Write-Host 'XAI_API_KEY: ❌ Not configured' -ForegroundColor Red
}

if ($newSyncfusionKey) {
    Write-Host "SYNCFUSION_LICENSE_KEY: ✅ Configured (length: $($newSyncfusionKey.Length) chars)" -ForegroundColor Green
} else {
    Write-Host 'SYNCFUSION_LICENSE_KEY: ❌ Not configured' -ForegroundColor Red
}

Write-Host "`n📝 Next Steps:" -ForegroundColor Blue
Write-Host '1. Restart your development environment (VS Code, Visual Studio)' -ForegroundColor White
Write-Host '2. Run the application to verify keys work' -ForegroundColor White
Write-Host '3. If keys were exposed, rotate them at the provider' -ForegroundColor White

Write-Host "`n🔒 Security Notes:" -ForegroundColor Yellow
Write-Host '• Environment variables are stored per-user' -ForegroundColor White
Write-Host '• Never commit .env files to Git' -ForegroundColor White
Write-Host '• Regularly rotate API keys' -ForegroundColor White
Write-Host '• Monitor for GitGuardian alerts' -ForegroundColor White

Write-Host "`nSetup complete! 🎉" -ForegroundColor Green
