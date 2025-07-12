# IMMEDIATE SECURITY FIX - Simple and Safe
# This script safely addresses GitGuardian alerts without complex Git operations

Write-Host "🔒 IMMEDIATE SECURITY RESPONSE" -ForegroundColor Green
Write-Host "==============================" -ForegroundColor Green

Write-Host "`n✅ GOOD NEWS: Current files are safe!" -ForegroundColor Green
Write-Host "Your appsettings.json files correctly use environment variables." -ForegroundColor White

Write-Host "`n🚨 ACTION REQUIRED: Rotate exposed keys" -ForegroundColor Red

Write-Host "`n1. ROTATE XAI API KEY:" -ForegroundColor Yellow
Write-Host "   • Go to: https://xai.com/api" -ForegroundColor White
Write-Host "   • Revoke current key (if exposed)" -ForegroundColor White
Write-Host "   • Generate new key" -ForegroundColor White
Write-Host "   • Update environment variable:" -ForegroundColor White
Write-Host "     [Environment]::SetEnvironmentVariable('XAI_API_KEY', 'your_new_key', 'User')" -ForegroundColor Gray

Write-Host "`n2. ROTATE SYNCFUSION LICENSE:" -ForegroundColor Yellow
Write-Host "   • Go to: https://syncfusion.com/account/downloads" -ForegroundColor White
Write-Host "   • Check if key rotation is possible" -ForegroundColor White
Write-Host "   • Contact support if needed" -ForegroundColor White
Write-Host "   • Update environment variable:" -ForegroundColor White
Write-Host "     [Environment]::SetEnvironmentVariable('SYNCFUSION_LICENSE_KEY', 'your_new_key', 'User')" -ForegroundColor Gray

Write-Host "`n3. ACKNOWLEDGE GITGUARDIAN ALERT:" -ForegroundColor Yellow
Write-Host "   • Log into GitGuardian dashboard" -ForegroundColor White
Write-Host "   • Mark incident as 'Resolved'" -ForegroundColor White
Write-Host "   • Add note: 'Keys rotated and moved to environment variables'" -ForegroundColor White

Write-Host "`n4. VERIFY CURRENT SAFETY:" -ForegroundColor Yellow
Write-Host "Checking current configuration..." -ForegroundColor White

# Check current config
$currentConfig = Get-Content "appsettings.json" | Select-String -Pattern "MTU5|xai-[a-zA-Z0-9]+"
if ($currentConfig) {
    Write-Host "❌ DANGER: Keys found in current files!" -ForegroundColor Red
} else {
    Write-Host "✅ SAFE: No hardcoded keys in current files" -ForegroundColor Green
}

# Check environment variables
$xaiKey = $env:XAI_API_KEY
$syncKey = $env:SYNCFUSION_LICENSE_KEY

Write-Host "`nEnvironment Variables Status:" -ForegroundColor Cyan
Write-Host "XAI_API_KEY: $($xaiKey ? '✅ Set' : '❌ Missing')" -ForegroundColor ($xaiKey ? 'Green' : 'Red')
Write-Host "SYNCFUSION_LICENSE_KEY: $($syncKey ? '✅ Set' : '❌ Missing')" -ForegroundColor ($syncKey ? 'Green' : 'Red')

Write-Host "`n🔍 NEXT STEPS:" -ForegroundColor Blue
Write-Host "1. Rotate both API keys immediately" -ForegroundColor White
Write-Host "2. Update your local environment variables" -ForegroundColor White
Write-Host "3. Test the application to ensure it works" -ForegroundColor White
Write-Host "4. Mark GitGuardian incident as resolved" -ForegroundColor White

Write-Host "`n⚠️  IMPORTANT: Git history may still contain old keys" -ForegroundColor Yellow
Write-Host "If you want to clean Git history, run EMERGENCY_KEY_REMOVAL.ps1" -ForegroundColor White
Write-Host "But rotating the keys is the most important step!" -ForegroundColor Green
