# Test Git commands with pager disabled
$env:GIT_PAGER = ""

Write-Host "Testing Git commands with pager disabled..." -ForegroundColor Cyan
Write-Host "Current Git version:" -ForegroundColor Yellow
& { $env:GIT_PAGER=""; git --version }

Write-Host "`nCurrent Git status:" -ForegroundColor Yellow
& { $env:GIT_PAGER=""; git status --short }

Write-Host "`nGit test completed." -ForegroundColor Green
Write-Host "If you see Git output above without any 'press RETURN' messages, the pager is successfully disabled." -ForegroundColor Green
