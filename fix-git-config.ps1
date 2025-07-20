# Configure Git for PowerShell 7.5.2
# This script sets Git configuration to work properly with PowerShell 7.5.2
# It disables the pager for common commands to avoid terminal output issues

Write-Host "Configuring Git for PowerShell 7.5.2..." -ForegroundColor Cyan

# Force environment variable to disable pager
$env:GIT_PAGER = ""

# Disable pager for common commands with direct process execution to avoid pager issues
Start-Process -FilePath "git" -ArgumentList "config", "--global", "core.pager", """" -NoNewWindow -Wait
Start-Process -FilePath "git" -ArgumentList "config", "--global", "pager.branch", "false" -NoNewWindow -Wait
Start-Process -FilePath "git" -ArgumentList "config", "--global", "pager.config", "false" -NoNewWindow -Wait
Start-Process -FilePath "git" -ArgumentList "config", "--global", "pager.diff", "false" -NoNewWindow -Wait
Start-Process -FilePath "git" -ArgumentList "config", "--global", "pager.log", "false" -NoNewWindow -Wait
Start-Process -FilePath "git" -ArgumentList "config", "--global", "pager.show", "false" -NoNewWindow -Wait
Start-Process -FilePath "git" -ArgumentList "config", "--global", "pager.status", "false" -NoNewWindow -Wait

# Set credential.helper to avoid hanging on credentials
Start-Process -FilePath "git" -ArgumentList "config", "--global", "credential.helper", "manager-core" -NoNewWindow -Wait

# Enable long paths
Start-Process -FilePath "git" -ArgumentList "config", "--global", "core.longpaths", "true" -NoNewWindow -Wait

# Set UTF-8 encoding for compatibility
Start-Process -FilePath "git" -ArgumentList "config", "--global", "i18n.logoutputencoding", "utf-8" -NoNewWindow -Wait
Start-Process -FilePath "git" -ArgumentList "config", "--global", "core.quotepath", "false" -NoNewWindow -Wait

# Set safer push behavior
Start-Process -FilePath "git" -ArgumentList "config", "--global", "push.default", "simple" -NoNewWindow -Wait

# Output current configuration using a non-paged method
Write-Host "`nGit configuration updated. Current settings:" -ForegroundColor Green
$gitConfig = & { $env:GIT_PAGER=""; git config --global --list }
$gitConfig | ForEach-Object { Write-Host "  $_" }

Write-Host "`nGit is now configured to work properly with PowerShell 7.5.2" -ForegroundColor Green
