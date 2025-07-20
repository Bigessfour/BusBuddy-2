# GitHub-Actions-Monitor.ps1
# Bus Buddy - GitHub Actions Workflow Monitoring and Issue Resolution

<#
.SYNOPSIS
    Monitors GitHub Actions workflows, waits for completion, retrieves results, and fixes issues

.DESCRIPTION
    This script provides comprehensive monitoring of GitHub Actions workflows including:
    - Triggering workflows and waiting for completion with proper async handling
    - Retrieving and parsing workflow results
    - Automated issue detection and resolution
    - Integration with Bus Buddy development workflow

.EXAMPLE
    .\GitHub-Actions-Monitor.ps1 -TriggerWorkflow

.EXAMPLE
    .\GitHub-Actions-Monitor.ps1 -MonitorLatest -AutoFix

.EXAMPLE
    .\GitHub-Actions-Monitor.ps1 -AnalyzeFailures -GenerateReport
#>

param(
    [switch]$TriggerWorkflow,
    [switch]$MonitorLatest,
    [switch]$AutoFix,
    [switch]$AnalyzeFailures,
    [switch]$GenerateReport,
    [string]$RepoOwner = "Bigessfour",
    [string]$RepoName = "BusBuddy-WPF",
    [int]$TimeoutMinutes = 30,
    [int]$PollIntervalSeconds = 30
)

# Import required modules
Import-Module -Name Microsoft.PowerShell.Utility -Force

function Write-StatusMessage {
    param(
        [string]$Message,
        [string]$Type = "Info"
    )

    $timestamp = Get-Date -Format "HH:mm:ss"
    $icon = switch ($Type) {
        "Success" { "✅" }
        "Warning" { "⚠️" }
        "Error" { "❌" }
        "Info" { "ℹ️" }
        "Progress" { "🔄" }
        default { "📝" }
    }

    $color = switch ($Type) {
        "Success" { "Green" }
        "Warning" { "Yellow" }
        "Error" { "Red" }
        "Info" { "Cyan" }
        "Progress" { "Blue" }
        default { "White" }
    }

    Write-Host "[$timestamp] $icon $Message" -ForegroundColor $color
}

function Test-GitHubCLI {
    <#
    .SYNOPSIS
        Verifies GitHub CLI is available and authenticated
    #>

    Write-StatusMessage "Checking GitHub CLI availability..." -Type "Progress"

    $ghCli = Get-Command gh -ErrorAction SilentlyContinue
    if (-not $ghCli) {
        Write-StatusMessage "GitHub CLI not found. Please install: winget install GitHub.cli" -Type "Error"
        return $false
    }

    try {
        $authStatus = gh auth status 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-StatusMessage "GitHub CLI not authenticated. Run: gh auth login" -Type "Error"
            return $false
        }

        Write-StatusMessage "GitHub CLI is available and authenticated" -Type "Success"
        return $true
    } catch {
        Write-StatusMessage "Error checking GitHub CLI: $($_.Exception.Message)" -Type "Error"
        return $false
    }
}

function Start-GitHubWorkflow {
    <#
    .SYNOPSIS
        Triggers a GitHub Actions workflow and returns the run ID
    #>

    Write-StatusMessage "Triggering GitHub Actions workflow..." -Type "Progress"

    try {
        # Stage and commit any pending changes first
        $hasChanges = Invoke-GitCommitIfNeeded

        if ($hasChanges) {
            Write-StatusMessage "Changes committed and pushed" -Type "Success"
        }

        # Trigger workflow via push (since we just pushed) or manual dispatch
        $runsBefore = Get-GitHubWorkflowRuns -Limit 1
        $latestRunBefore = $runsBefore | Select-Object -First 1

        # Wait a moment for the workflow to appear
        Start-Sleep -Seconds 10

        $runsAfter = Get-GitHubWorkflowRuns -Limit 5
        $newRun = $runsAfter | Where-Object {
            $latestRunBefore -eq $null -or $_.databaseId -gt $latestRunBefore.databaseId
        } | Select-Object -First 1

        if ($newRun) {
            Write-StatusMessage "Workflow triggered successfully. Run ID: $($newRun.databaseId)" -Type "Success"
            return $newRun.databaseId
        } else {
            Write-StatusMessage "No new workflow run detected. Using latest run for monitoring." -Type "Warning"
            $latestRun = $runsAfter | Select-Object -First 1
            return $latestRun.databaseId
        }
    } catch {
        Write-StatusMessage "Error triggering workflow: $($_.Exception.Message)" -Type "Error"
        return $null
    }
}

function Invoke-GitCommitIfNeeded {
    <#
    .SYNOPSIS
        Commits and pushes any pending changes to trigger workflow
    #>

    Write-StatusMessage "Checking for uncommitted changes..." -Type "Progress"

    try {
        # Check git status
        $gitStatus = git status --porcelain 2>&1

        if ([string]::IsNullOrWhiteSpace($gitStatus)) {
            Write-StatusMessage "No uncommitted changes found" -Type "Info"
            return $false
        }

        Write-StatusMessage "Found uncommitted changes. Staging and committing..." -Type "Progress"

        # Stage all changes
        git add -A

        # Create commit with timestamp
        $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        $commitMessage = "🤖 Automated workflow trigger - $timestamp"

        git commit -m $commitMessage

        if ($LASTEXITCODE -ne 0) {
            Write-StatusMessage "Failed to commit changes" -Type "Error"
            return $false
        }

        # Push changes
        git push origin main

        if ($LASTEXITCODE -ne 0) {
            Write-StatusMessage "Failed to push changes" -Type "Error"
            return $false
        }

        Write-StatusMessage "Changes committed and pushed successfully" -Type "Success"
        return $true
    } catch {
        Write-StatusMessage "Error committing changes: $($_.Exception.Message)" -Type "Error"
        return $false
    }
}

function Get-GitHubWorkflowRuns {
    <#
    .SYNOPSIS
        Retrieves GitHub workflow runs
    #>
    param(
        [int]$Limit = 10
    )

    try {
        $runs = gh run list --repo "$RepoOwner/$RepoName" --limit $Limit --json databaseId, status, conclusion, name, createdAt, updatedAt, htmlUrl, headBranch | ConvertFrom-Json
        return $runs
    } catch {
        Write-StatusMessage "Error fetching workflow runs: $($_.Exception.Message)" -Type "Error"
        return @()
    }
}

function Wait-ForWorkflowCompletion {
    <#
    .SYNOPSIS
        Waits for a specific workflow run to complete with proper async handling
    #>
    param(
        [string]$RunId,
        [int]$TimeoutMinutes = 30,
        [int]$PollIntervalSeconds = 30
    )

    Write-StatusMessage "Monitoring workflow run $RunId (timeout: ${TimeoutMinutes}m)" -Type "Progress"

    $startTime = Get-Date
    $timeoutTime = $startTime.AddMinutes($TimeoutMinutes)

    do {
        try {
            $runDetails = gh run view $RunId --repo "$RepoOwner/$RepoName" --json status, conclusion, name, createdAt, updatedAt, jobs | ConvertFrom-Json

            $status = $runDetails.status
            $conclusion = $runDetails.conclusion
            $elapsed = [math]::Round(((Get-Date) - $startTime).TotalMinutes, 1)

            Write-StatusMessage "Status: $status | Conclusion: $conclusion | Elapsed: ${elapsed}m" -Type "Progress"

            # Check if completed
            if ($status -eq "completed") {
                Write-StatusMessage "Workflow completed with result: $conclusion" -Type $(if ($conclusion -eq "success") { "Success" } else { "Warning" })
                return @{
                    Status     = $status
                    Conclusion = $conclusion
                    Details    = $runDetails
                    Success    = ($conclusion -eq "success")
                }
            }

            # Check timeout
            if ((Get-Date) -gt $timeoutTime) {
                Write-StatusMessage "Workflow monitoring timed out after ${TimeoutMinutes} minutes" -Type "Error"
                return @{
                    Status     = "timeout"
                    Conclusion = "timeout"
                    Details    = $runDetails
                    Success    = $false
                }
            }

            # Wait before next poll
            Write-StatusMessage "Waiting ${PollIntervalSeconds}s before next status check..." -Type "Info"
            Start-Sleep -Seconds $PollIntervalSeconds
        } catch {
            Write-StatusMessage "Error checking workflow status: $($_.Exception.Message)" -Type "Error"
            Start-Sleep -Seconds $PollIntervalSeconds
        }
    } while ($true)
}

function Get-WorkflowResults {
    <#
    .SYNOPSIS
        Retrieves detailed workflow results including logs and artifacts
    #>
    param(
        [string]$RunId
    )

    Write-StatusMessage "Retrieving detailed workflow results for run $RunId..." -Type "Progress"

    try {
        # Get run details
        $runDetails = gh run view $RunId --repo "$RepoOwner/$RepoName" --json status, conclusion, name, createdAt, updatedAt, jobs, htmlUrl | ConvertFrom-Json

        # Get job details
        $jobs = @()
        foreach ($job in $runDetails.jobs) {
            $jobDetail = gh run view $RunId --job $job.databaseId --repo "$RepoOwner/$RepoName" --json name, status, conclusion, steps | ConvertFrom-Json
            $jobs += $jobDetail
        }

        # Get artifacts
        $artifacts = gh run download $RunId --repo "$RepoOwner/$RepoName" --dry-run 2>&1

        $results = @{
            RunDetails  = $runDetails
            Jobs        = $jobs
            Artifacts   = $artifacts
            Success     = ($runDetails.conclusion -eq "success")
            HasFailures = ($runDetails.conclusion -in @("failure", "cancelled"))
        }

        Write-StatusMessage "Retrieved workflow results successfully" -Type "Success"
        return $results
    } catch {
        Write-StatusMessage "Error retrieving workflow results: $($_.Exception.Message)" -Type "Error"
        return $null
    }
}

function Invoke-IssueAnalysis {
    <#
    .SYNOPSIS
        Analyzes workflow failures and identifies potential fixes
    #>
    param(
        [object]$WorkflowResults
    )

    Write-StatusMessage "Analyzing workflow issues..." -Type "Progress"

    if (-not $WorkflowResults -or -not $WorkflowResults.HasFailures) {
        Write-StatusMessage "No failures to analyze" -Type "Info"
        return @()
    }

    $issues = @()

    foreach ($job in $WorkflowResults.Jobs) {
        if ($job.conclusion -ne "success") {
            foreach ($step in $job.steps) {
                if ($step.conclusion -eq "failure") {
                    $issue = Analyze-StepFailure -Job $job -Step $step
                    if ($issue) {
                        $issues += $issue
                    }
                }
            }
        }
    }

    Write-StatusMessage "Found $($issues.Count) issues for analysis" -Type "Info"
    return $issues
}

function Analyze-StepFailure {
    <#
    .SYNOPSIS
        Analyzes a specific step failure and suggests fixes
    #>
    param(
        [object]$Job,
        [object]$Step
    )

    $stepName = $step.name
    $jobName = $job.name

    # Common failure patterns and fixes
    $issue = @{
        JobName          = $jobName
        StepName         = $stepName
        Type             = "Unknown"
        Description      = "Step failed"
        SuggestedFix     = "Manual review required"
        AutoFixAvailable = $false
    }

    # Analyze common patterns
    switch -Regex ($stepName) {
        ".*[Cc]odecov.*" {
            $issue.Type = "Codecov"
            $issue.Description = "Codecov upload failed - likely missing token"
            $issue.SuggestedFix = "Configure CODECOV_TOKEN in repository secrets"
            $issue.AutoFixAvailable = $false
        }
        ".*[Bb]uild.*" {
            $issue.Type = "Build"
            $issue.Description = "Build compilation failed"
            $issue.SuggestedFix = "Check for compilation errors and fix code issues"
            $issue.AutoFixAvailable = $true
        }
        ".*[Tt]est.*" {
            $issue.Type = "Test"
            $issue.Description = "Test execution failed"
            $issue.SuggestedFix = "Review test failures and fix failing tests"
            $issue.AutoFixAvailable = $true
        }
        ".*[Rr]estore.*" {
            $issue.Type = "Restore"
            $issue.Description = "Package restoration failed"
            $issue.SuggestedFix = "Clear NuGet cache and restore packages"
            $issue.AutoFixAvailable = $true
        }
        ".*[Ss]ecurity.*|.*[Cc]ode[Qq][Ll].*" {
            $issue.Type = "Security"
            $issue.Description = "Security scan failed"
            $issue.SuggestedFix = "Review security findings and update dependencies"
            $issue.AutoFixAvailable = $false
        }
    }

    return $issue
}

function Invoke-AutoFix {
    <#
    .SYNOPSIS
        Attempts to automatically fix identified issues
    #>
    param(
        [array]$Issues
    )

    Write-StatusMessage "Attempting to auto-fix issues..." -Type "Progress"

    $fixedCount = 0

    foreach ($issue in $Issues) {
        if (-not $issue.AutoFixAvailable) {
            Write-StatusMessage "Skipping $($issue.Type) issue - no auto-fix available" -Type "Warning"
            continue
        }

        Write-StatusMessage "Attempting to fix: $($issue.Description)" -Type "Progress"

        try {
            switch ($issue.Type) {
                "Build" {
                    $fixed = Invoke-BuildFix
                    if ($fixed) { $fixedCount++ }
                }
                "Test" {
                    $fixed = Invoke-TestFix
                    if ($fixed) { $fixedCount++ }
                }
                "Restore" {
                    $fixed = Invoke-RestoreFix
                    if ($fixed) { $fixedCount++ }
                }
            }
        } catch {
            Write-StatusMessage "Error fixing $($issue.Type): $($_.Exception.Message)" -Type "Error"
        }
    }

    Write-StatusMessage "Auto-fixed $fixedCount out of $($Issues.Count) issues" -Type "Success"
    return $fixedCount -gt 0
}

function Invoke-BuildFix {
    <#
    .SYNOPSIS
        Attempts to fix build issues
    #>

    Write-StatusMessage "Attempting build fix..." -Type "Progress"

    try {
        # Clean and restore
        dotnet clean BusBuddy.sln
        dotnet restore BusBuddy.sln --force --no-cache

        # Try build
        $buildResult = dotnet build BusBuddy.sln --no-restore --verbosity normal 2>&1

        if ($LASTEXITCODE -eq 0) {
            Write-StatusMessage "Build fix successful" -Type "Success"
            return $true
        } else {
            Write-StatusMessage "Build still failing after fix attempt" -Type "Warning"
            Write-StatusMessage "Build output: $buildResult" -Type "Info"
            return $false
        }
    } catch {
        Write-StatusMessage "Error during build fix: $($_.Exception.Message)" -Type "Error"
        return $false
    }
}

function Invoke-TestFix {
    <#
    .SYNOPSIS
        Attempts to fix test issues
    #>

    Write-StatusMessage "Attempting test fix..." -Type "Progress"

    try {
        # Run tests with verbose output to identify issues
        $testResult = dotnet test BusBuddy.sln --no-build --verbosity detailed 2>&1

        if ($LASTEXITCODE -eq 0) {
            Write-StatusMessage "All tests now passing" -Type "Success"
            return $true
        } else {
            Write-StatusMessage "Some tests still failing - manual review needed" -Type "Warning"
            # Could add specific test fix logic here
            return $false
        }
    } catch {
        Write-StatusMessage "Error during test fix: $($_.Exception.Message)" -Type "Error"
        return $false
    }
}

function Invoke-RestoreFix {
    <#
    .SYNOPSIS
        Attempts to fix package restoration issues
    #>

    Write-StatusMessage "Attempting restore fix..." -Type "Progress"

    try {
        # Clear NuGet caches
        dotnet nuget locals all --clear

        # Restore with force
        dotnet restore BusBuddy.sln --force --no-cache --verbosity normal

        if ($LASTEXITCODE -eq 0) {
            Write-StatusMessage "Package restoration successful" -Type "Success"
            return $true
        } else {
            Write-StatusMessage "Package restoration still failing" -Type "Warning"
            return $false
        }
    } catch {
        Write-StatusMessage "Error during restore fix: $($_.Exception.Message)" -Type "Error"
        return $false
    }
}

function New-WorkflowReport {
    <#
    .SYNOPSIS
        Generates a comprehensive workflow analysis report
    #>
    param(
        [object]$WorkflowResults,
        [array]$Issues
    )

    Write-StatusMessage "Generating workflow report..." -Type "Progress"

    $reportPath = "workflow-analysis-$(Get-Date -Format 'yyyyMMdd-HHmmss').md"

    $report = @"
# Bus Buddy - GitHub Actions Workflow Analysis Report

**Generated:** $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
**Repository:** $RepoOwner/$RepoName

## Workflow Summary

| Property | Value |
|----------|-------|
| Status | $($WorkflowResults.RunDetails.status) |
| Conclusion | $($WorkflowResults.RunDetails.conclusion) |
| Success | $($WorkflowResults.Success) |
| Has Failures | $($WorkflowResults.HasFailures) |
| Jobs Count | $($WorkflowResults.Jobs.Count) |
| Issues Found | $($Issues.Count) |

## Job Details

"@

    foreach ($job in $WorkflowResults.Jobs) {
        $report += @"

### Job: $($job.name)
- **Status:** $($job.status)
- **Conclusion:** $($job.conclusion)
- **Steps:** $($job.steps.Count)

"@
    }

    if ($Issues.Count -gt 0) {
        $report += @"

## Issues Analysis

"@
        foreach ($issue in $Issues) {
            $report += @"

### $($issue.Type) Issue
- **Job:** $($issue.JobName)
- **Step:** $($issue.StepName)
- **Description:** $($issue.Description)
- **Suggested Fix:** $($issue.SuggestedFix)
- **Auto-Fix Available:** $($issue.AutoFixAvailable)

"@
        }
    }

    $report += @"

## Recommendations

1. Review failed steps and apply suggested fixes
2. Run local builds and tests before pushing
3. Monitor workflow results after fixes
4. Consider adding more robust error handling

---
*Generated by Bus Buddy GitHub Actions Monitor*
"@

    $report | Out-File -FilePath $reportPath -Encoding UTF8
    Write-StatusMessage "Report saved to: $reportPath" -Type "Success"

    return $reportPath
}

# Main execution flow
function Start-GitHubActionsWorkflow {
    Write-StatusMessage "🚌 Bus Buddy - GitHub Actions Workflow Monitor" -Type "Info"
    Write-StatusMessage "=============================================" -Type "Info"

    # Verify GitHub CLI
    if (-not (Test-GitHubCLI)) {
        return
    }

    $runId = $null
    $workflowResults = $null
    $issues = @()

    try {
        # Step 1: Trigger workflow if requested
        if ($TriggerWorkflow) {
            $runId = Start-GitHubWorkflow
            if (-not $runId) {
                Write-StatusMessage "Failed to trigger workflow" -Type "Error"
                return
            }
        } elseif ($MonitorLatest) {
            # Get latest run
            $latestRuns = Get-GitHubWorkflowRuns -Limit 1
            if ($latestRuns.Count -gt 0) {
                $runId = $latestRuns[0].databaseId
                Write-StatusMessage "Monitoring latest workflow run: $runId" -Type "Info"
            } else {
                Write-StatusMessage "No workflow runs found" -Type "Error"
                return
            }
        }

        # Step 2: Wait for completion with proper async handling
        if ($runId) {
            $completionResult = Wait-ForWorkflowCompletion -RunId $runId -TimeoutMinutes $TimeoutMinutes -PollIntervalSeconds $PollIntervalSeconds

            if (-not $completionResult.Success) {
                Write-StatusMessage "Workflow did not complete successfully" -Type "Warning"
            }
        }

        # Step 3: Retrieve results
        if ($runId) {
            $workflowResults = Get-WorkflowResults -RunId $runId
        }

        # Step 4: Analyze issues
        if ($workflowResults -and ($AnalyzeFailures -or $AutoFix)) {
            $issues = Invoke-IssueAnalysis -WorkflowResults $workflowResults
        }

        # Step 5: Auto-fix if requested
        if ($AutoFix -and $issues.Count -gt 0) {
            $fixesApplied = Invoke-AutoFix -Issues $issues

            if ($fixesApplied) {
                Write-StatusMessage "Fixes applied. Consider re-running workflow to verify." -Type "Success"
            }
        }

        # Step 6: Generate report if requested
        if ($GenerateReport -and $workflowResults) {
            $reportPath = New-WorkflowReport -WorkflowResults $workflowResults -Issues $issues
            Write-StatusMessage "Analysis complete. Report: $reportPath" -Type "Success"
        }

        # Summary
        Write-StatusMessage "Workflow monitoring completed" -Type "Success"
        if ($workflowResults) {
            Write-StatusMessage "Final status: $($workflowResults.RunDetails.conclusion)" -Type $(if ($workflowResults.Success) { "Success" } else { "Warning" })
        }

    } catch {
        Write-StatusMessage "Critical error in workflow monitoring: $($_.Exception.Message)" -Type "Error"
    }
}

# Execute based on parameters
Start-GitHubActionsWorkflow
