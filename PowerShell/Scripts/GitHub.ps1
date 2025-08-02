# GitHub.ps1 - GitHub and Git-related functions for BusBuddy
# Microsoft PowerShell dot-source script

function Get-BusBuddyGitStatus {
    <#
    .SYNOPSIS
    Get Git status for BusBuddy
    .DESCRIPTION
    Shows the current Git status of the BusBuddy repository
    .EXAMPLE
    Get-BusBuddyGitStatus
    #>
    [CmdletBinding()]
    param()

    try {
        Write-Host "📋 Git Status" -ForegroundColor Cyan
        Write-Host "=============" -ForegroundColor Cyan

        Push-Location $BusBuddyWorkspace

        # Check if git repo
        if (Test-Path ".git") {
            # Current branch
            $branch = git branch --show-current
            Write-Host "🌿 Branch: $branch" -ForegroundColor Green

            # Status summary
            $status = git status --porcelain
            if ($status) {
                Write-Host "📝 Changes detected:" -ForegroundColor Yellow
                $status | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
            } else {
                Write-Host "✅ Working tree clean" -ForegroundColor Green
            }

            # Latest commit
            $lastCommit = git log -1 --oneline
            Write-Host "📜 Last commit: $lastCommit" -ForegroundColor White
        } else {
            Write-Host "⚠️ Not a Git repository" -ForegroundColor Yellow
        }

        Pop-Location
    }
    catch {
        Write-Error "Git status error: $($_.Exception.Message)"
        Pop-Location
    }
}

function Invoke-BusBuddyCommit {
    <#
    .SYNOPSIS
    Commit changes to BusBuddy repository
    .DESCRIPTION
    Adds and commits all changes with a message
    .PARAMETER Message
    The commit message
    .EXAMPLE
    Invoke-BusBuddyCommit -Message "Add new feature"
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Message
    )

    try {
        Push-Location $BusBuddyWorkspace

        Write-Host "📝 Committing changes..." -ForegroundColor Cyan
        git add .
        git commit -m $Message

        Write-Host "✅ Changes committed!" -ForegroundColor Green

        Pop-Location
    }
    catch {
        Write-Error "Commit error: $($_.Exception.Message)"
        Pop-Location
    }
}

function Sync-BusBuddyRepository {
    <#
    .SYNOPSIS
    Sync BusBuddy repository with remote
    .DESCRIPTION
    Pulls latest changes and pushes local changes
    .EXAMPLE
    Sync-BusBuddyRepository
    #>
    [CmdletBinding()]
    param()

    try {
        Push-Location $BusBuddyWorkspace

        Write-Host "🔄 Syncing repository..." -ForegroundColor Cyan

        # Pull latest changes
        Write-Host "⬇️ Pulling latest changes..." -ForegroundColor Yellow
        git pull

        # Push local changes
        Write-Host "⬆️ Pushing local changes..." -ForegroundColor Yellow
        git push

        Write-Host "✅ Repository synced!" -ForegroundColor Green

        Pop-Location
    }
    catch {
        Write-Error "Sync error: $($_.Exception.Message)"
        Pop-Location
    }
}

function New-BusBuddyBranch {
    <#
    .SYNOPSIS
    Create a new Git branch
    .DESCRIPTION
    Creates and switches to a new branch for BusBuddy development
    .PARAMETER BranchName
    The name of the new branch
    .EXAMPLE
    New-BusBuddyBranch -BranchName "feature/new-dashboard"
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$BranchName
    )

    try {
        Push-Location $BusBuddyWorkspace

        Write-Host "🌿 Creating new branch: $BranchName" -ForegroundColor Cyan
        git checkout -b $BranchName

        Write-Host "✅ Branch created and switched!" -ForegroundColor Green

        Pop-Location
    }
    catch {
        Write-Error "Branch creation error: $($_.Exception.Message)"
        Pop-Location
    }
}

function Get-BusBuddyIgnoreStatus {
    <#
    .SYNOPSIS
    Check .gitignore effectiveness
    .DESCRIPTION
    Analyzes which files are being tracked vs ignored
    .EXAMPLE
    Get-BusBuddyIgnoreStatus
    #>
    [CmdletBinding()]
    param()

    try {
        Push-Location $BusBuddyWorkspace

        Write-Host "🔍 Checking .gitignore effectiveness..." -ForegroundColor Cyan

        # Check for common files that should be ignored
        $shouldIgnore = @("bin", "obj", "*.user", ".vs", "TestResults")
        foreach ($pattern in $shouldIgnore) {
            $files = git ls-files $pattern 2>$null
            if ($files) {
                Write-Host "⚠️ Tracked files matching '$pattern':" -ForegroundColor Yellow
                $files | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
            } else {
                Write-Host "✅ Pattern '$pattern' properly ignored" -ForegroundColor Green
            }
        }

        Pop-Location
    }
    catch {
        Write-Error "Ignore status error: $($_.Exception.Message)"
        Pop-Location
    }
}

Write-Verbose "GitHub functions loaded: Get-BusBuddyGitStatus, Invoke-BusBuddyCommit, Sync-BusBuddyRepository, New-BusBuddyBranch, Get-BusBuddyIgnoreStatus"
