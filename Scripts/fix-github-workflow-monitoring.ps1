#Requires -Version 7.0
<#
.SYNOPSIS
Fix and enhance GitHub Actions workflow monitoring for BusBuddy

.DESCRIPTION
This script fixes the GitHub CLI JSON field compatibility issues and provides
a streamlined workflow monitoring solution that works with the current GitHub CLI version.
#>

[CmdletBinding()]
param(
    [switch]$SetupEnvironment,
    [switch]$TestWorkflows,
    [int]$Count = 5
)

function Write-FixLog {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "HH:mm:ss"
    $color = switch ($Level) {
        "ERROR" { "Red" }
        "WARN" { "Yellow" }
        "SUCCESS" { "Green" }
        default { "Cyan" }
    }
    Write-Host "[$timestamp] [$Level] $Message" -ForegroundColor $color
}

function Get-WorkflowStatusSimple {
    <#
    .SYNOPSIS
    Get GitHub Actions workflow status using compatible fields
    #>
    param([int]$Limit = 5)

    Write-FixLog "🔍 Getting workflow status (simple approach)" "INFO"

    try {
        # Use only the fields that are guaranteed to work
        $runs = gh run list --limit $Limit --json "status,conclusion,workflowName,createdAt,headBranch" | ConvertFrom-Json

        if ($runs.Count -gt 0) {
            Write-FixLog "✅ Retrieved $($runs.Count) workflow runs successfully" "SUCCESS"

            Write-Host "`n📊 Recent Workflow Runs:" -ForegroundColor Cyan
            Write-Host "═══════════════════════════════════════════════════" -ForegroundColor DarkGray

            foreach ($run in $runs) {
                $statusIcon = switch ($run.status) {
                    "completed" {
                        switch ($run.conclusion) {
                            "success" { "✅" }
                            "failure" { "❌" }
                            default { "⚠️" }
                        }
                    }
                    "in_progress" { "🔄" }
                    default { "⏸️" }
                }

                $timeAgo = [math]::Round(((Get-Date) - [DateTime]$run.createdAt).TotalMinutes, 0)
                Write-Host "$statusIcon $($run.workflowName) ($($run.status)$(if($run.conclusion){" - $($run.conclusion)"})) - $timeAgo min ago" -ForegroundColor Gray
            }

            return $runs
        }
        else {
            Write-FixLog "ℹ️ No workflow runs found" "WARN"
            return @()
        }
    }
    catch {
        Write-FixLog "❌ Failed to get workflow status: $($_.Exception.Message)" "ERROR"
        return $null
    }
}

function Test-WorkflowHealthCheck {
    <#
    .SYNOPSIS
    Test the health of GitHub Actions workflows
    #>
    Write-FixLog "🏥 Testing workflow health" "INFO"

    # Check GitHub CLI authentication
    $authStatus = gh auth status 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-FixLog "✅ GitHub CLI authenticated" "SUCCESS"
    }
    else {
        Write-FixLog "❌ GitHub CLI not authenticated. Run: gh auth login" "ERROR"
        return $false
    }

    # Check if we're in a Git repository
    if (Test-Path ".git") {
        Write-FixLog "✅ Git repository detected" "SUCCESS"
    }
    else {
        Write-FixLog "❌ Not in a Git repository" "ERROR"
        return $false
    }

    # Check for workflow files
    $workflowFiles = Get-ChildItem -Path ".github/workflows" -Filter "*.yml" -ErrorAction SilentlyContinue
    if ($workflowFiles.Count -gt 0) {
        Write-FixLog "✅ Found $($workflowFiles.Count) workflow files" "SUCCESS"
        foreach ($file in $workflowFiles) {
            Write-Host "   📄 $($file.Name)" -ForegroundColor Gray
        }
    }
    else {
        Write-FixLog "⚠️ No workflow files found in .github/workflows/" "WARN"
    }

    return $true
}

function Fix-BusBuddyGitHubIntegration {
    <#
    .SYNOPSIS
    Apply fixes to BusBuddy GitHub integration
    #>
    Write-FixLog "🔧 Applying GitHub integration fixes" "INFO"

    # Set GitHub token from CLI
    try {
        $token = gh auth token
        $env:GITHUB_TOKEN = $token
        Write-FixLog "✅ GitHub token set from CLI authentication" "SUCCESS"
    }
    catch {
        Write-FixLog "⚠️ Could not get token from GitHub CLI" "WARN"
    }

    # Test simple workflow call
    $testRuns = Get-WorkflowStatusSimple -Limit 3

    if ($testRuns -and $testRuns.Count -gt 0) {
        Write-FixLog "✅ GitHub Actions integration working" "SUCCESS"
        return $true
    }
    else {
        Write-FixLog "❌ GitHub Actions integration still failing" "ERROR"
        return $false
    }
}

function Show-WorkflowSummary {
    <#
    .SYNOPSIS
    Show a summary of workflow status and recommendations
    #>
    Write-FixLog "📋 Generating workflow summary" "INFO"

    $runs = Get-WorkflowStatusSimple -Limit 10

    if ($runs -and $runs.Count -gt 0) {
        $successful = ($runs | Where-Object { $_.conclusion -eq "success" }).Count
        $failed = ($runs | Where-Object { $_.conclusion -eq "failure" }).Count
        $inProgress = ($runs | Where-Object { $_.status -eq "in_progress" }).Count

        Write-Host "`n📊 Workflow Summary (Last 10 runs):" -ForegroundColor Cyan
        Write-Host "═══════════════════════════════════════" -ForegroundColor DarkGray
        Write-Host "✅ Successful: $successful" -ForegroundColor Green
        Write-Host "❌ Failed: $failed" -ForegroundColor Red
        Write-Host "🔄 In Progress: $inProgress" -ForegroundColor Yellow

        $successRate = if ($runs.Count -gt 0) { [math]::Round(($successful / $runs.Count) * 100, 1) } else { 0 }
        Write-Host "📈 Success Rate: $successRate%" -ForegroundColor $(if ($successRate -gt 80) { "Green" } elseif ($successRate -gt 60) { "Yellow" } else { "Red" })

        if ($failed -gt 0) {
            Write-Host "`n💡 Recommendations:" -ForegroundColor Blue
            Write-Host "• Check failed workflow logs: gh run view <run-id>" -ForegroundColor Gray
            Write-Host "• Review .github/workflows/*.yml for configuration issues" -ForegroundColor Gray
            Write-Host "• Ensure Windows runner compatibility for WPF builds" -ForegroundColor Gray
        }
    }
}

# Main execution
Write-FixLog "🚌 BusBuddy GitHub Actions Workflow Fix Starting" "INFO"

if ($SetupEnvironment) {
    $healthOk = Test-WorkflowHealthCheck
    if ($healthOk) {
        $fixOk = Fix-BusBuddyGitHubIntegration
        if ($fixOk) {
            Write-FixLog "✅ Environment setup completed successfully" "SUCCESS"
        }
    }
}

if ($TestWorkflows) {
    Show-WorkflowSummary
}

# Default action: quick status check
if (-not $SetupEnvironment -and -not $TestWorkflows) {
    Get-WorkflowStatusSimple -Limit $Count
}

Write-FixLog "🏁 GitHub Actions workflow fix completed" "SUCCESS"
