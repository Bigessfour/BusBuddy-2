# Run PowerShell 7.5.2 Alignment Script
# ====================================
# This script simply executes the align-git-github-ps752.ps1 script
# with the correct parameters and execution policy.

Write-Host "Running PowerShell 7.5.2, Git, and GitHub alignment script..." -ForegroundColor Cyan

# Get the path to the alignment script
$scriptPath = Join-Path $PSScriptRoot "align-git-github-ps752.ps1"

# Verify script exists
if (-not (Test-Path $scriptPath)) {
    Write-Host "ERROR: Alignment script not found at: $scriptPath" -ForegroundColor Red
    exit 1
}

# Execute the script with bypass execution policy
try {
    Write-Host "Executing: $scriptPath" -ForegroundColor Yellow
    & pwsh -ExecutionPolicy Bypass -NoProfile -File $scriptPath

    if ($LASTEXITCODE -eq 0 -or $null -eq $LASTEXITCODE) {
        Write-Host "✅ Alignment script completed successfully!" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Alignment script completed with exit code: $LASTEXITCODE" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Error running alignment script: $_" -ForegroundColor Red
}

Write-Host "`nPress any key to continue..." -ForegroundColor Cyan
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
