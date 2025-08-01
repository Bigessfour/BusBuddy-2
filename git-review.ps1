Write-Host "========================================" -ForegroundColor Cyan
Write-Host "BusBuddy Git Repository Review (Enhanced)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Function to run git commands and capture output
function Invoke-GitCommand {
    param([string]$Command)
    try {
        $result = Invoke-Expression "git $Command" 2>&1
        return $result
    }
    catch {
        Write-Host "Error executing: git $Command" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        return $null
    }
}

# Function to run GitHub CLI commands
function Invoke-GhCommand {
    param([string]$Command)
    try {
        $result = Invoke-Expression "gh $Command" 2>&1
        return $result
    }
    catch {
        Write-Host "Error executing: gh $Command" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        return $null
    }
}

# Check if we're in a git repository
$gitRoot = Invoke-GitCommand "rev-parse --show-toplevel"
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Not in a git repository!" -ForegroundColor Red
    exit 1
}

Write-Host "Repository root: $gitRoot" -ForegroundColor Green
Write-Host ""

# Current branch
$currentBranch = Invoke-GitCommand "branch --show-current"
Write-Host "Current branch: $currentBranch" -ForegroundColor Yellow
Write-Host ""

# Porcelain status
Write-Host "Repository Status (Porcelain):" -ForegroundColor Magenta
$porcelainStatus = Invoke-GitCommand "status --porcelain"
if ($porcelainStatus) {
    $porcelainStatus | ForEach-Object {
        $status = $_.Substring(0, 2)
        $file = $_.Substring(3)
        switch ($status.Trim()) {
            "M" { Write-Host "  Modified: $file" -ForegroundColor Yellow }
            "A" { Write-Host "  Added: $file" -ForegroundColor Green }
            "D" { Write-Host "  Deleted: $file" -ForegroundColor Red }
            "??" { Write-Host "  Untracked: $file" -ForegroundColor Cyan }
            "MM" { Write-Host "  Modified (staged and unstaged): $file" -ForegroundColor Magenta }
            default { Write-Host "  $status : $file" -ForegroundColor White }
        }
    }
} else {
    Write-Host "  Working tree clean" -ForegroundColor Green
}
Write-Host ""

# Show file counts
$untrackedCount = (Invoke-GitCommand "ls-files --others --exclude-standard" | Measure-Object).Count
$modifiedCount = (Invoke-GitCommand "diff --name-only" | Measure-Object).Count
$stagedCount = (Invoke-GitCommand "diff --cached --name-only" | Measure-Object).Count

Write-Host "File Summary:" -ForegroundColor Magenta
Write-Host "  Untracked files: $untrackedCount" -ForegroundColor Cyan
Write-Host "  Modified files: $modifiedCount" -ForegroundColor Yellow
Write-Host "  Staged files: $stagedCount" -ForegroundColor Green
Write-Host ""

# Recent commits
Write-Host "Recent Commits:" -ForegroundColor Magenta
Invoke-GitCommand "log --oneline -5" | ForEach-Object {
    Write-Host "  $_" -ForegroundColor Gray
}
Write-Host ""

# Remote status
Write-Host "Remote Status:" -ForegroundColor Magenta
$remoteStatus = Invoke-GitCommand "status -b --porcelain"
if ($remoteStatus) {
    $remoteStatus | Select-Object -First 1 | ForEach-Object {
        Write-Host "  $_" -ForegroundColor Gray
    }
}
Write-Host ""

# Offer to stage and commit
if ($untrackedCount -gt 0 -or $modifiedCount -gt 0) {
    $response = Read-Host "Do you want to stage and commit all changes? (y/N)"
    if ($response -eq 'y' -or $response -eq 'Y') {
        Write-Host "Staging all changes..." -ForegroundColor Yellow
        Invoke-GitCommand "add ."

        $commitMessage = Read-Host "Enter commit message (or press Enter for auto-generated)"
        if ([string]::IsNullOrWhiteSpace($commitMessage)) {
            $timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
            $commitMessage = "Auto-commit: Repository sync $timestamp"
        }

        Write-Host "Committing changes..." -ForegroundColor Yellow
        Invoke-GitCommand "commit -m `"$commitMessage`""

        $pushResponse = Read-Host "Do you want to push to remote? (y/N)"
        if ($pushResponse -eq 'y' -or $pushResponse -eq 'Y') {
            Write-Host "Pushing to remote..." -ForegroundColor Yellow
            Invoke-GitCommand "push origin HEAD"
            Write-Host "Repository sync complete!" -ForegroundColor Green
        }
    }
} else {
    Write-Host "No changes to commit." -ForegroundColor Green
}

Write-Host ""
Write-Host "Git review complete!" -ForegroundColor Cyan
