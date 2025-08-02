# 🚀 BusBuddy GitHub Push Protocol
# Enhanced Git workflow with comprehensive validation and documentation

function Invoke-BusBuddyGitPush {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false)]
        [string]$CommitMessage = "Auto-commit: BusBuddy development progress",
        
        [Parameter(Mandatory = $false)]
        [string]$Branch = "main",
        
        [Parameter(Mandatory = $false)]
        [switch]$Force,
        
        [Parameter(Mandatory = $false)]
        [switch]$DryRun,
        
        [Parameter(Mandatory = $false)]
        [switch]$ValidateFirst
    )
    
    Write-Host "🚌 BusBuddy GitHub Push Protocol" -ForegroundColor Cyan
    Write-Host "Repository: BusBuddy Development Session" -ForegroundColor Gray
    Write-Host "Timestamp: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
    Write-Host ""
    
    # Validate we're in the correct directory
    if (-not (Test-Path "BusBuddy.sln")) {
        Write-Error "❌ Not in BusBuddy root directory. Navigate to project root first."
        return
    }
    
    # Pre-push validation if requested
    if ($ValidateFirst) {
        Write-Host "🔍 Running pre-push validation..." -ForegroundColor Yellow
        
        # Check build status
        Write-Host "   • Checking build status..."
        $buildResult = dotnet build BusBuddy.sln --verbosity quiet 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "⚠️  Build has errors. Consider fixing before push."
            Write-Host "Build output:" -ForegroundColor Red
            Write-Host $buildResult -ForegroundColor Red
        } else {
            Write-Host "   ✅ Build successful" -ForegroundColor Green
        }
        
        # Check for large files
        Write-Host "   • Checking for large files..."
        $largeFiles = Get-ChildItem -Recurse -File | Where-Object { $_.Length -gt 10MB }
        if ($largeFiles) {
            Write-Warning "⚠️  Large files detected:"
            $largeFiles | ForEach-Object { Write-Host "     $($_.FullName) ($([math]::Round($_.Length/1MB, 2)) MB)" }
        }
        
        # Check git status
        Write-Host "   • Checking git status..."
        $gitStatus = git status --porcelain
        if (-not $gitStatus) {
            Write-Host "   ✅ No changes to commit" -ForegroundColor Green
            return
        }
        Write-Host "   📝 Changes detected: $($gitStatus.Count) files" -ForegroundColor Cyan
    }
    
    # Show current status
    Write-Host "📊 Current Git Status:" -ForegroundColor Cyan
    git status --short
    Write-Host ""
    
    # Stage changes
    Write-Host "📦 Staging changes..." -ForegroundColor Yellow
    if ($DryRun) {
        Write-Host "   [DRY RUN] Would stage all changes" -ForegroundColor Magenta
    } else {
        git add .
        if ($LASTEXITCODE -eq 0) {
            Write-Host "   ✅ All changes staged" -ForegroundColor Green
        } else {
            Write-Error "❌ Failed to stage changes"
            return
        }
    }
    
    # Create enhanced commit message with context
    $enhancedMessage = @"
$CommitMessage

Session Details:
- Date: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
- Focus: MVP Phase 1 - XAML corruption fixes and package management
- Key Changes: Package version conflicts resolved, project file rebuilt
- Build Status: In progress (XAML parsing errors being addressed)
- Next Steps: Complete XAML fixes, restore clean build capability

Technical Changes:
- Fixed Microsoft.Extensions.DependencyInjection version conflicts
- Rebuilt BusBuddy.WPF.csproj to remove corruption
- Fixed multiple XAML files with parsing errors
- Updated Directory.Packages.props with centralized versioning
- Cleared NuGet cache and resolved package restore issues

Files Modified:
- Directory.Packages.props: Added centralized package versioning
- BusBuddy.WPF.csproj: Complete rebuild to fix corruption
- Multiple XAML files: Fixed XML parsing errors
- GROK-README.md: Updated with current session status
"@
    
    # Commit changes
    Write-Host "💾 Committing changes..." -ForegroundColor Yellow
    if ($DryRun) {
        Write-Host "   [DRY RUN] Would commit with message:" -ForegroundColor Magenta
        Write-Host $enhancedMessage -ForegroundColor Gray
    } else {
        git commit -m $enhancedMessage
        if ($LASTEXITCODE -eq 0) {
            Write-Host "   ✅ Changes committed" -ForegroundColor Green
        } else {
            Write-Error "❌ Failed to commit changes"
            return
        }
    }
    
    # Push to remote
    Write-Host "🚀 Pushing to remote..." -ForegroundColor Yellow
    if ($DryRun) {
        Write-Host "   [DRY RUN] Would push to origin/$Branch" -ForegroundColor Magenta
    } else {
        if ($Force) {
            git push origin $Branch --force
        } else {
            git push origin $Branch
        }
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "   ✅ Successfully pushed to origin/$Branch" -ForegroundColor Green
        } else {
            Write-Error "❌ Failed to push to remote"
            Write-Host "Attempting to pull and merge..." -ForegroundColor Yellow
            git pull origin $Branch --rebase
            if ($LASTEXITCODE -eq 0) {
                Write-Host "   ✅ Rebased successfully, attempting push again..." -ForegroundColor Green
                git push origin $Branch
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "   ✅ Push successful after rebase" -ForegroundColor Green
                } else {
                    Write-Error "❌ Push failed even after rebase"
                    return
                }
            } else {
                Write-Error "❌ Rebase failed - manual intervention required"
                return
            }
        }
    }
    
    # Success summary
    Write-Host ""
    Write-Host "🎉 GitHub Push Protocol Complete!" -ForegroundColor Green
    Write-Host "Repository Status: Up to date with remote" -ForegroundColor Green
    Write-Host "Branch: $Branch" -ForegroundColor Green
    Write-Host "Commit: $(git rev-parse --short HEAD)" -ForegroundColor Green
    Write-Host ""
    Write-Host "🔗 Next Steps:" -ForegroundColor Cyan
    Write-Host "   • Continue XAML corruption fixes" -ForegroundColor White
    Write-Host "   • Restore build capability" -ForegroundColor White
    Write-Host "   • Resume MVP Phase 1 development" -ForegroundColor White
}

# Quick aliases for common operations
function bb-git-push { Invoke-BusBuddyGitPush @args }
function bb-git-validate-push { Invoke-BusBuddyGitPush -ValidateFirst @args }
function bb-git-dry-run { Invoke-BusBuddyGitPush -DryRun @args }
function bb-git-force-push { Invoke-BusBuddyGitPush -Force @args }

# Quick status check
function bb-git-status {
    Write-Host "🚌 BusBuddy Git Status" -ForegroundColor Cyan
    Write-Host "Repository: $(Split-Path -Leaf (Get-Location))" -ForegroundColor Gray
    Write-Host ""
    
    # Current branch
    $currentBranch = git branch --show-current
    Write-Host "📍 Current Branch: $currentBranch" -ForegroundColor Green
    
    # Status summary
    $status = git status --porcelain
    if ($status) {
        Write-Host "📝 Changes: $($status.Count) files modified" -ForegroundColor Yellow
        git status --short
    } else {
        Write-Host "✅ Working directory clean" -ForegroundColor Green
    }
    
    # Remote status
    git fetch origin --quiet 2>$null
    $ahead = git rev-list --count origin/$currentBranch..HEAD 2>$null
    $behind = git rev-list --count HEAD..origin/$currentBranch 2>$null
    
    if ($ahead -gt 0) {
        Write-Host "⬆️  Ahead of remote by $ahead commits" -ForegroundColor Cyan
    }
    if ($behind -gt 0) {
        Write-Host "⬇️  Behind remote by $behind commits" -ForegroundColor Yellow
    }
    if ($ahead -eq 0 -and $behind -eq 0) {
        Write-Host "🔄 Up to date with remote" -ForegroundColor Green
    }
}

# Export functions for module usage
Export-ModuleMember -Function Invoke-BusBuddyGitPush, bb-git-push, bb-git-validate-push, bb-git-dry-run, bb-git-force-push, bb-git-status

Write-Host "🚀 BusBuddy GitHub Protocol Loaded" -ForegroundColor Green
Write-Host "Available commands:" -ForegroundColor Cyan
Write-Host "  bb-git-push            - Standard commit and push"
Write-Host "  bb-git-validate-push   - Validate before pushing"
Write-Host "  bb-git-dry-run         - Preview changes without committing"
Write-Host "  bb-git-force-push      - Force push (use carefully)"
Write-Host "  bb-git-status          - Enhanced status display"
