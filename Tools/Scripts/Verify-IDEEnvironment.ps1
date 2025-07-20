# Bus Buddy IDE Environment Verification
Write-Host "🔍 Bus Buddy IDE Environment Check" -ForegroundColor Cyan

# Check PowerShell version
Write-Host "PowerShell Version: $($PSVersionTable.PSVersion)" -ForegroundColor White

# Check if Bus Buddy profile functions are available
if (Get-Command bb-build -ErrorAction SilentlyContinue) {
    Write-Host "✅ Bus Buddy commands available" -ForegroundColor Green
} else {
    Write-Host "⚠️  Bus Buddy commands not loaded" -ForegroundColor Yellow
}

# Check workspace
if (Test-Path ".\BusBuddy.sln") {
    Write-Host "✅ In Bus Buddy workspace" -ForegroundColor Green
} else {
    Write-Host "⚠️  Not in Bus Buddy workspace" -ForegroundColor Yellow
}

Write-Host "Environment check complete!" -ForegroundColor Cyan
