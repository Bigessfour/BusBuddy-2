# ═══════════════════════════════════════════════════════════════════════════════
# 🔗 GIT WORKFLOW EFFICIENCY ENHANCEMENT
# ═══════════════════════════════════════════════════════════════════════════════
# Streamlines Git operations to reduce manual steps and improve workflow speed

param(
    [switch]$CreateAliases,
    [switch]$OptimizeCommits,
    [switch]$SetupHooks,
    [switch]$AnalyzeRepo
)

function Write-GitLog {
    param($Message, $Level = "INFO")
    $timestamp = Get-Date -Format "HH:mm:ss"
    $color = switch ($Level) {
        "SUCCESS" { "Green" }
        "WARN" { "Yellow" }
        "ERROR" { "Red" }
        default { "Cyan" }
    }
    Write-Host "[$timestamp] $Message" -ForegroundColor $color
}

function New-GitAliases {
    <#
    .SYNOPSIS
    Creates efficient Git aliases for common BusBuddy operations
    #>
    Write-GitLog "⚡ Creating Git efficiency aliases..." "INFO"

    $aliases = @{
        # Quick status and staging
        "st" = "status --short --branch"
        "a" = "add ."
        "aa" = "add --all"
        "unstage" = "reset HEAD --"

        # Efficient commits
        "c" = "commit"
        "cm" = "commit -m"
        "ca" = "commit --amend"
        "can" = "commit --amend --no-edit"

        # Branch operations
        "co" = "checkout"
        "cob" = "checkout -b"
        "br" = "branch"
        "brd" = "branch -d"

        # Push/pull shortcuts
        "p" = "push"
        "pl" = "pull"
        "pf" = "push --force-with-lease"
        "po" = "push origin"

        # Log and history
        "l" = "log --oneline -10"
        "lg" = "log --graph --pretty=format:'%Cred%h%Creset -%C(yellow)%d%Creset %s %Cgreen(%cr) %C(bold blue)<%an>%Creset' --abbrev-commit"
        "last" = "log -1 HEAD --stat"

        # Diff operations
        "d" = "diff"
        "dc" = "diff --cached"
        "ds" = "diff --stat"

        # Stash operations
        "s" = "stash"
        "sp" = "stash pop"
        "sl" = "stash list"

        # BusBuddy specific
        "bb-save" = "!git add . && git commit -m 'WIP: Phase 1 development checkpoint'"
        "bb-backup" = "!git add . && git commit -m 'Backup: $(date)' && git push"
        "bb-clean" = "clean -fd"
    }

    Write-GitLog "Setting up Git aliases..." "INFO"
    foreach ($alias in $aliases.GetEnumerator()) {
        git config --global alias.$($alias.Key) $alias.Value
        Write-GitLog "  $($alias.Key) -> $($alias.Value)" "SUCCESS"
    }

    Write-GitLog "✅ Git aliases configured successfully" "SUCCESS"
}

function Optimize-CommitWorkflow {
    <#
    .SYNOPSIS
    Creates optimized commit templates and workflows
    #>
    Write-GitLog "📝 Optimizing commit workflow..." "INFO"

    # Create commit message template
    $commitTemplate = @"
# 🚌 BusBuddy Commit Template
#
# Format: <type>(<scope>): <description>
#
# Types:
#   feat:     New feature
#   fix:      Bug fix
#   docs:     Documentation
#   style:    Code style (formatting, etc.)
#   refactor: Code refactoring
#   perf:     Performance improvement
#   test:     Tests
#   chore:    Maintenance
#
# Scopes (Phase 1):
#   dashboard: MainWindow dashboard
#   drivers:   Driver management
#   vehicles:  Vehicle/bus management
#   activity:  Activity scheduling
#   ui:        UI components
#   data:      Data layer
#   build:     Build system
#
# Examples:
#   feat(dashboard): add driver count display
#   fix(vehicles): resolve vehicle list loading issue
#   perf(data): optimize database queries
#   chore(build): update package versions
#
# Keep the description under 50 characters
# Use imperative mood: "add" not "added" or "adds"
"@

    $templatePath = ".gitmessage"
    $commitTemplate | Out-File -FilePath $templatePath -Encoding UTF8
    git config commit.template $templatePath

    Write-GitLog "✅ Commit template created: $templatePath" "SUCCESS"

    # Create quick commit functions
    $commitFunctions = @"
# ═══════════════════════════════════════════════════════════════════════════════
# 🚌 BUSBUDDY GIT WORKFLOW FUNCTIONS
# ═══════════════════════════════════════════════════════════════════════════════

function bb-commit {
    param(
        [Parameter(Mandatory=`$true)]
        [string]`$Message,
        [string]`$Type = "feat",
        [string]`$Scope = ""
    )

    `$commitMsg = if (`$Scope) { "`$Type(`$Scope): `$Message" } else { "`$Type: `$Message" }
    git add .
    git commit -m `$commitMsg
    Write-Host "✅ Committed: `$commitMsg" -ForegroundColor Green
}

function bb-save {
    param([string]`$Message = "Phase 1 development checkpoint")
    git add .
    git commit -m "WIP: `$Message"
    Write-Host "💾 Saved work in progress" -ForegroundColor Cyan
}

function bb-sync {
    param([switch]`$Force)

    Write-Host "🔄 Syncing with remote..." -ForegroundColor Cyan
    git add .

    `$status = git status --porcelain
    if (`$status) {
        `$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm"
        git commit -m "Auto-sync: `$timestamp"
        Write-Host "📦 Changes committed" -ForegroundColor Green
    }

    git pull origin main
    if (`$Force) {
        git push origin HEAD --force-with-lease
    } else {
        git push origin HEAD
    }

    Write-Host "✅ Sync complete" -ForegroundColor Green
}

function bb-feature {
    param(
        [Parameter(Mandatory=`$true)]
        [string]`$Name
    )

    `$branchName = "feature/`$Name"
    git checkout -b `$branchName
    Write-Host "🌿 Created feature branch: `$branchName" -ForegroundColor Green
}

function bb-status {
    Write-Host "📊 BusBuddy Git Status:" -ForegroundColor Cyan

    # Current branch
    `$branch = git branch --show-current
    Write-Host "   Branch: `$branch" -ForegroundColor Gray

    # Uncommitted changes
    `$status = git status --porcelain
    if (`$status) {
        `$changeCount = (`$status | Measure-Object).Count
        Write-Host "   Changes: `$changeCount files" -ForegroundColor Yellow
    } else {
        Write-Host "   Changes: Clean working directory" -ForegroundColor Green
    }

    # Commits ahead/behind
    `$ahead = git rev-list --count HEAD...@{upstream} 2>`$null
    if (`$ahead) {
        Write-Host "   Sync: `$ahead commits ahead" -ForegroundColor Yellow
    } else {
        Write-Host "   Sync: Up to date" -ForegroundColor Green
    }

    # Last commit
    `$lastCommit = git log -1 --pretty=format:"%h %s" 2>`$null
    if (`$lastCommit) {
        Write-Host "   Last: `$lastCommit" -ForegroundColor Gray
    }
}

Export-ModuleMember -Function bb-commit, bb-save, bb-sync, bb-feature, bb-status
"@

    $functionsPath = "Scripts\Efficiency\git-workflow-functions.psm1"
    $commitFunctions | Out-File -FilePath $functionsPath -Encoding UTF8

    Write-GitLog "✅ Git workflow functions created: $functionsPath" "SUCCESS"
    Write-GitLog "   Load with: Import-Module .\Scripts\Efficiency\git-workflow-functions.psm1" "INFO"
}

function New-GitHooks {
    <#
    .SYNOPSIS
    Sets up Git hooks for automated quality checks
    #>
    Write-GitLog "🎣 Setting up Git hooks..." "INFO"

    # Create hooks directory if it doesn't exist
    $hooksDir = ".git\hooks"
    if (-not (Test-Path $hooksDir)) {
        New-Item -Path $hooksDir -ItemType Directory -Force | Out-Null
    }

    # Pre-commit hook for basic validation
    $preCommitHook = @"
#!/bin/sh
# BusBuddy Pre-commit Hook
# Performs basic validation before commits

echo "🔍 Running pre-commit checks..."

# Check for syntax errors in PowerShell files
echo "Checking PowerShell syntax..."
for file in `$(git diff --cached --name-only --diff-filter=ACM | grep '\.ps1$'); do
    if [ -f "`$file" ]; then
        pwsh -NoProfile -Command "try { Get-Content '`$file' | Out-Null; Write-Host '✅ `$file syntax OK' } catch { Write-Host '❌ `$file syntax error' -ForegroundColor Red; exit 1 }"
        if [ `$? -ne 0 ]; then
            echo "❌ PowerShell syntax error in `$file"
            exit 1
        fi
    fi
done

# Check for TODO/FIXME in committed code
echo "Checking for TODO/FIXME..."
if git diff --cached | grep -i "TODO\|FIXME" > /dev/null; then
    echo "⚠️ Warning: TODO/FIXME found in staged changes"
    echo "Consider addressing these before committing"
fi

# Validate that builds still work (quick check)
echo "Quick build validation..."
dotnet build BusBuddy.sln --verbosity quiet --nologo > /dev/null 2>&1
if [ `$? -ne 0 ]; then
    echo "❌ Build validation failed - fix errors before committing"
    exit 1
fi

echo "✅ Pre-commit checks passed"
exit 0
"@

    $preCommitPath = "$hooksDir\pre-commit"
    $preCommitHook | Out-File -FilePath $preCommitPath -Encoding UTF8

    # Post-commit hook for notifications
    $postCommitHook = @"
#!/bin/sh
# BusBuddy Post-commit Hook
# Provides feedback after successful commits

echo "✅ Commit successful: `$(git log -1 --pretty=format:'%h %s')"
echo "📊 Repository status:"
echo "   Files: `$(git ls-files | wc -l) tracked"
echo "   Commits: `$(git rev-list --count HEAD)"
echo "   Branch: `$(git branch --show-current)"
"@

    $postCommitPath = "$hooksDir\post-commit"
    $postCommitHook | Out-File -FilePath $postCommitPath -Encoding UTF8

    Write-GitLog "✅ Git hooks configured" "SUCCESS"
    Write-GitLog "   Pre-commit: Syntax and build validation" "INFO"
    Write-GitLog "   Post-commit: Status feedback" "INFO"
}

function Get-RepositoryAnalysis {
    <#
    .SYNOPSIS
    Analyzes repository for optimization opportunities
    #>
    Write-GitLog "📊 Analyzing repository..." "INFO"

    $analysis = @{}

    # File count and size analysis
    $files = git ls-files
    $analysis.TotalFiles = $files.Count

    # Largest files
    $largeFiles = $files | ForEach-Object {
        if (Test-Path $_) {
            @{
                Path = $_
                Size = (Get-Item $_).Length
            }
        }
    } | Sort-Object Size -Descending | Select-Object -First 10

    $analysis.LargestFiles = $largeFiles

    # Commit history analysis
    $totalCommits = git rev-list --count HEAD 2>$null
    $analysis.TotalCommits = if ($totalCommits) { $totalCommits } else { 0 }

    # Recent activity
    $recentCommits = git log --oneline -10 2>$null
    $analysis.RecentCommits = if ($recentCommits) { $recentCommits.Count } else { 0 }

    # Branch analysis
    $branches = git branch -a 2>$null
    $analysis.Branches = if ($branches) { $branches.Count } else { 0 }

    # Repository size
    $gitSize = if (Test-Path ".git") {
        (Get-ChildItem ".git" -Recurse | Measure-Object -Property Length -Sum).Sum
    } else { 0 }
    $analysis.GitSizeMB = [Math]::Round($gitSize / 1MB, 2)

    Write-GitLog "📈 Repository Analysis Results:" "SUCCESS"
    Write-GitLog "   Total files: $($analysis.TotalFiles)" "INFO"
    Write-GitLog "   Total commits: $($analysis.TotalCommits)" "INFO"
    Write-GitLog "   Branches: $($analysis.Branches)" "INFO"
    Write-GitLog "   Git size: $($analysis.GitSizeMB) MB" "INFO"

    if ($analysis.LargestFiles) {
        Write-GitLog "   Largest files:" "INFO"
        $analysis.LargestFiles | Select-Object -First 5 | ForEach-Object {
            $sizeMB = [Math]::Round($_.Size / 1MB, 2)
            Write-GitLog "     $($_.Path) ($sizeMB MB)" "INFO"
        }
    }

    return $analysis
}

# ═══════════════════════════════════════════════════════════════════════════════
# MAIN EXECUTION
# ═══════════════════════════════════════════════════════════════════════════════

Write-GitLog "🔗 BusBuddy Git Workflow Efficiency Enhancement" "INFO"

if ($CreateAliases) {
    New-GitAliases
}

if ($OptimizeCommits) {
    Optimize-CommitWorkflow
}

if ($SetupHooks) {
    New-GitHooks
}

if ($AnalyzeRepo) {
    $analysis = Get-RepositoryAnalysis
}

# Default: Apply all optimizations
if (-not ($CreateAliases -or $OptimizeCommits -or $SetupHooks -or $AnalyzeRepo)) {
    Write-GitLog "Applying all Git optimizations..." "INFO"

    New-GitAliases
    Optimize-CommitWorkflow
    New-GitHooks
    $analysis = Get-RepositoryAnalysis

    Write-GitLog "🎉 Git workflow optimization complete!" "SUCCESS"
    Write-GitLog "Load functions with: Import-Module .\Scripts\Efficiency\git-workflow-functions.psm1" "INFO"
}

Write-GitLog "🏁 Git efficiency enhancement complete" "SUCCESS"
