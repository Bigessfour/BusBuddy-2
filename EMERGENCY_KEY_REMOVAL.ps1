# EMERGENCY: Remove API Keys from Git Repository
# Run this script to completely purge any exposed API keys

Write-Host "🚨 EMERGENCY API KEY REMOVAL" -ForegroundColor Red
Write-Host "================================" -ForegroundColor Red

# 1. Create a backup first
Write-Host "1. Creating backup..." -ForegroundColor Yellow
$backupPath = "BusBuddy_backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
git clone . "../$backupPath"
Write-Host "✅ Backup created at ../$backupPath" -ForegroundColor Green

# 2. Remove any files that might contain keys
Write-Host "2. Removing risky files from Git history..." -ForegroundColor Yellow

# Files to completely remove from history
$riskyFiles = @(
    "appsettings.json"
    "BusBuddy.WPF/appsettings.json"
    "BusBuddy.Core/appsettings.json"
    ".env"
    "*.key"
    "keys/*"
)

foreach ($file in $riskyFiles) {
    Write-Host "   Removing $file from history..." -ForegroundColor Cyan
    git filter-branch --force --index-filter "git rm --cached --ignore-unmatch '$file'" --prune-empty --tag-name-filter cat -- --all 2>$null
}

# 3. Clean up references
Write-Host "3. Cleaning up Git references..." -ForegroundColor Yellow
git for-each-ref --format="delete %(refname)" refs/original | git update-ref --stdin
git reflog expire --expire=now --all
git gc --prune=now --aggressive

# 4. Recreate appsettings.json files with environment variables only
Write-Host "4. Recreating safe configuration files..." -ForegroundColor Yellow

# Main appsettings.json
$safeAppsettings = @"
{
  "SyncfusionLicenseKey": "`${SYNCFUSION_LICENSE_KEY}",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=BusBuddyDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "XAI": {
    "ApiKey": "`${XAI_API_KEY}",
    "UseLiveAPI": false
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
"@

$safeAppsettings | Out-File -FilePath "appsettings.json" -Encoding UTF8
$safeAppsettings | Out-File -FilePath "BusBuddy.WPF/appsettings.json" -Encoding UTF8

Write-Host "✅ Safe configuration files created" -ForegroundColor Green

# 5. Commit the safe files
Write-Host "5. Committing safe configuration..." -ForegroundColor Yellow
git add appsettings.json BusBuddy.WPF/appsettings.json
git commit -m "SECURITY: Replace all config with environment variables only"

Write-Host "6. Force push to remove history..." -ForegroundColor Yellow
Write-Host "⚠️  WARNING: This will rewrite Git history!" -ForegroundColor Red
$confirm = Read-Host "Type 'YES' to force push and remove key history"

if ($confirm -eq "YES") {
    git push origin --force --all
    git push origin --force --tags
    Write-Host "✅ Repository history cleaned and pushed" -ForegroundColor Green
} else {
    Write-Host "❌ Aborted. Keys may still be in remote history." -ForegroundColor Red
}

Write-Host "`n🔒 IMMEDIATE NEXT STEPS:" -ForegroundColor Blue
Write-Host "1. Go to GitGuardian and mark incident as resolved" -ForegroundColor White
Write-Host "2. Rotate all exposed API keys:" -ForegroundColor White
Write-Host "   - xAI: https://xai.com/api" -ForegroundColor White
Write-Host "   - Syncfusion: https://syncfusion.com/account/downloads" -ForegroundColor White
Write-Host "3. Set new environment variables:" -ForegroundColor White
Write-Host "   [Environment]::SetEnvironmentVariable('XAI_API_KEY', 'new_key', 'User')" -ForegroundColor Gray
Write-Host "   [Environment]::SetEnvironmentVariable('SYNCFUSION_LICENSE_KEY', 'new_key', 'User')" -ForegroundColor Gray
Write-Host "4. Inform all collaborators to re-clone the repository" -ForegroundColor White

Write-Host "`n✅ Emergency remediation complete!" -ForegroundColor Green
