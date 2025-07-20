# Reset Git Pager Settings
# This script will update your PowerShell profile to automatically
# disable Git pager every time you start PowerShell

# Explicitly disable Git pager for the current session
$env:GIT_PAGER = ""

# Create a function to update Git configuration
function Update-GitConfig {
    param (
        [string]$Setting,
        [string]$Value
    )

    $process = Start-Process -FilePath "git" -ArgumentList "config", "--global", $Setting, $Value -NoNewWindow -Wait -PassThru
    return $process.ExitCode -eq 0
}

# Apply pager settings
Write-Host "Disabling Git pager..." -ForegroundColor Cyan
Update-GitConfig "core.pager" ""
Update-GitConfig "pager.branch" "false"
Update-GitConfig "pager.config" "false"
Update-GitConfig "pager.diff" "false"
Update-GitConfig "pager.log" "false"
Update-GitConfig "pager.show" "false"
Update-GitConfig "pager.status" "false"

# Update PowerShell profile to always disable Git pager
$profileContent = @'
# Disable Git pager for better PowerShell integration
$env:GIT_PAGER = ""
'@

$profilePath = $PROFILE.CurrentUserAllHosts
if (!(Test-Path $profilePath)) {
    New-Item -Path $profilePath -ItemType File -Force | Out-Null
    Write-Host "Created PowerShell profile at $profilePath" -ForegroundColor Green
}

if ((Get-Content $profilePath -Raw -ErrorAction SilentlyContinue) -notmatch 'GIT_PAGER') {
    Add-Content -Path $profilePath -Value "`n$profileContent" -Force
    Write-Host "Updated PowerShell profile to always disable Git pager" -ForegroundColor Green
}

Write-Host "`nGit pager has been disabled for the current session and all future PowerShell sessions." -ForegroundColor Green
Write-Host "You can now use Git commands without pager interruptions." -ForegroundColor Green
