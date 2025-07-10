
# Stop on first error
$ErrorActionPreference = 'Stop'

# Color helper
function Write-Color($Text, $Color) {
    Write-Host $Text -ForegroundColor $Color
}

# Git status check
if (Test-Path .git) {
    $gitStatus = git status --porcelain
    if ($gitStatus) {
        Write-Color 'WARNING: You have uncommitted changes!' Yellow
        Write-Host $gitStatus
    } else {
        Write-Color 'Git working directory is clean.' Green
    }
}

Write-Color 'Cleaning solution...' Cyan
if (-not (dotnet clean BusBuddy.sln)) {
    Write-Color 'CLEAN FAILED' Red
    [console]::beep(1000, 500)
    exit 1
}

Write-Color 'Restoring packages...' Cyan
if (-not (dotnet restore BusBuddy.sln)) {
    Write-Color 'RESTORE FAILED' Red
    [console]::beep(1000, 500)
    exit 1
}

Write-Color 'Building solution...' Cyan
if (-not (dotnet build BusBuddy.sln)) {
    Write-Color 'BUILD FAILED' Red
    [console]::beep(1000, 500)
    exit 1
}

Write-Color 'Running tests...' Cyan
$testResult = dotnet test BusBuddy.Tests/BusBuddy.Tests.csproj --logger 'trx;LogFileName=test-results.trx'
if ($LASTEXITCODE -ne 0) {
    Write-Color 'TESTS FAILED' Red
    [console]::beep(1000, 700)
    if (Test-Path './BusBuddy.Tests/TestResults/test-results.trx') {
        Invoke-Item './BusBuddy.Tests/TestResults/test-results.trx'
    }
    exit 1
}

# Success summary
Write-Color 'BUILD AND TEST SUCCEEDED!' Green
Write-Host 'Check test-results.trx for details.'
Write-Color 'You are ready to commit or deploy.' Green
Write-Host ''
Write-Color 'Next: Review your production checklist:' Yellow
Write-Host '  PRODUCTION_CHECKLIST.md'
if (Test-Path './PRODUCTION_CHECKLIST.md') {
    Write-Host 'Opening checklist...'
    Invoke-Item './PRODUCTION_CHECKLIST.md'
}
