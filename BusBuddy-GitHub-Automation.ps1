#Requires -Version 7.0
<#
.SYNOPSIS
    Bus Buddy GitHub Actions Automation and Workflow Management

.DESCRIPTION
    Comprehensive PowerShell automation for GitHub workflow management including:
    - Staging, committing, and pushing changes
    - Monitoring GitHub Actions workflow completion
    - Retrieving and analyzing workflow results
    - Automated issue resolution based on workflow feedback

.NOTES
    Requires GitHub CLI (gh) to be installed and authenticated
    Optimized for PowerShell 7.5.2+ with parallel processing
#>

# ============================================================================
# CONFIGURATION AND CONSTANTS
# ============================================================================

$Global:BusBuddyGitHubConfig = @{
    Owner = "Bigessfour"
    Repo = "BusBuddy-WPF"
    WorkflowFile = "build-and-test.yml"
    DefaultBranch = "main"
    MaxWaitTime = 1800  # 30 minutes
    PollInterval = 30   # 30 seconds
}

# ============================================================================
# GITHUB CLI VALIDATION AND SETUP
# ============================================================================

function Test-GitHubCLI {
    <#
    .SYNOPSIS
        Validates GitHub CLI installation and authentication
    #>
    [CmdletBinding()]
    param()

    try {
        # Check if gh is installed
        $ghVersion = gh --version 2>$null
        if ($LASTEXITCODE -ne 0) {
            Write-Error "GitHub CLI (gh) is not installed. Please install: https://cli.github.com/"
            return $false
        }

        Write-Host "✅ GitHub CLI found: $($ghVersion -split "`n" | Select-Object -First 1)" -ForegroundColor Green

        # Check authentication
        $authStatus = gh auth status 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "GitHub CLI not authenticated. Run: gh auth login"
            return $false
        }

        Write-Host "✅ GitHub CLI authenticated" -ForegroundColor Green
        return $true

    } catch {
        Write-Error "Failed to validate GitHub CLI: $($_.Exception.Message)"
        return $false
    }
}

function Initialize-GitHubAutomation {
    <#
    .SYNOPSIS
        Initialize GitHub automation environment
    #>
    [CmdletBinding()]
    param()

    Write-Host "🚀 Initializing Bus Buddy GitHub Automation..." -ForegroundColor Cyan

    # Validate GitHub CLI
    if (-not (Test-GitHubCLI)) {
        throw "GitHub CLI validation failed"
    }

    # Validate git repository
    $gitStatus = git status --porcelain 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "Not in a git repository"
    }

    # Get current branch
    $currentBranch = git rev-parse --abbrev-ref HEAD
    Write-Host "📍 Current branch: $currentBranch" -ForegroundColor Yellow

    # Validate remote origin
    $remoteUrl = git remote get-url origin 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "No remote origin configured"
    }

    Write-Host "🔗 Remote origin: $remoteUrl" -ForegroundColor Yellow
    Write-Host "✅ GitHub automation environment initialized" -ForegroundColor Green

    return @{
        CurrentBranch = $currentBranch
        RemoteUrl = $remoteUrl
        GitStatus = $gitStatus
    }
}

# ============================================================================
# SMART GIT OPERATIONS
# ============================================================================

function Invoke-SmartGitStaging {
    <#
    .SYNOPSIS
        Intelligent staging of changes with categorization and validation
    #>
    [CmdletBinding()]
    param(
        [string[]]$IncludePatterns = @("*.md", "*.ps1", "*.yml", "*.yaml", "*.cs", "*.xaml", "*.csproj", "*.sln"),
        [string[]]$ExcludePatterns = @("bin/", "obj/", "logs/", "TestResults/", "*.user", "*.suo"),
        [switch]$InteractiveMode,
        [switch]$DryRun
    )

    Write-Host "📋 Analyzing repository changes..." -ForegroundColor Cyan

    # Get all changes
    $changes = @{
        Modified = @(git diff --name-only)
        Untracked = @(git ls-files --others --exclude-standard)
        Deleted = @(git diff --name-only --diff-filter=D)
    }

    $stagingCandidates = @()

    # Process each type of change
    foreach ($changeType in $changes.Keys) {
        foreach ($file in $changes[$changeType]) {
            # Apply include patterns
            $shouldInclude = $false
            foreach ($pattern in $IncludePatterns) {
                if ($file -like $pattern) {
                    $shouldInclude = $true
                    break
                }
            }

            if (-not $shouldInclude) { continue }

            # Apply exclude patterns
            $shouldExclude = $false
            foreach ($pattern in $ExcludePatterns) {
                if ($file -like $pattern) {
                    $shouldExclude = $true
                    break
                }
            }

            if ($shouldExclude) { continue }

            $stagingCandidates += [PSCustomObject]@{
                File = $file
                Type = $changeType
                Category = Get-FileCategory -FilePath $file
            }
        }
    }

    if ($stagingCandidates.Count -eq 0) {
        Write-Host "ℹ️ No stageable changes found" -ForegroundColor Yellow
        return @()
    }

    # Display staging plan
    Write-Host "`n📊 Staging Plan:" -ForegroundColor Cyan
    $stagingCandidates | Group-Object Category | ForEach-Object {
        Write-Host "  📁 $($_.Name): $($_.Count) files" -ForegroundColor White
        $_.Group | ForEach-Object {
            $color = switch ($_.Type) {
                "Modified" { "Yellow" }
                "Untracked" { "Green" }
                "Deleted" { "Red" }
                default { "White" }
            }
            Write-Host "    $($_.Type): $($_.File)" -ForegroundColor $color
        }
    }

    if ($DryRun) {
        Write-Host "`n🔍 Dry run completed - no files staged" -ForegroundColor Yellow
        return $stagingCandidates
    }

    # Interactive confirmation
    if ($InteractiveMode) {
        $response = Read-Host "`nProceed with staging these files? (y/n)"
        if ($response -notmatch '^[Yy]') {
            Write-Host "❌ Staging cancelled by user" -ForegroundColor Red
            return @()
        }
    }

    # Stage files
    Write-Host "`n📦 Staging files..." -ForegroundColor Cyan
    $stagedFiles = @()

    foreach ($candidate in $stagingCandidates) {
        try {
            git add $candidate.File
            if ($LASTEXITCODE -eq 0) {
                $stagedFiles += $candidate
                Write-Host "  ✅ Staged: $($candidate.File)" -ForegroundColor Green
            } else {
                Write-Warning "❌ Failed to stage: $($candidate.File)"
            }
        } catch {
            Write-Warning "❌ Error staging $($candidate.File): $($_.Exception.Message)"
        }
    }

    Write-Host "✅ Staged $($stagedFiles.Count) files successfully" -ForegroundColor Green
    return $stagedFiles
}

function Get-FileCategory {
    <#
    .SYNOPSIS
        Categorizes files based on their type and purpose
    #>
    param([string]$FilePath)

    switch -Regex ($FilePath) {
        '\.(md|txt)$' { return "Documentation" }
        '\.(ps1|psm1|psd1)$' { return "PowerShell" }
        '\.(yml|yaml)$' { return "Workflows" }
        '\.(cs|xaml)$' { return "Source Code" }
        '\.(csproj|sln|props|targets)$' { return "Project Files" }
        '\.(json|xml|config)$' { return "Configuration" }
        default { return "Other" }
    }
}

function New-IntelligentCommit {
    <#
    .SYNOPSIS
        Creates intelligent commit messages based on staged changes
    #>
    [CmdletBinding()]
    param(
        [string]$CustomMessage,
        [object[]]$StagedFiles,
        [switch]$GenerateMessage
    )

    if ($CustomMessage) {
        $commitMessage = $CustomMessage
    } elseif ($GenerateMessage) {
        $commitMessage = Generate-CommitMessage -StagedFiles $StagedFiles
    } else {
        $commitMessage = Read-Host "Enter commit message"
    }

    if ([string]::IsNullOrWhiteSpace($commitMessage)) {
        throw "Commit message cannot be empty"
    }

    Write-Host "📝 Committing with message: $commitMessage" -ForegroundColor Cyan

    try {
        git commit -m $commitMessage
        if ($LASTEXITCODE -eq 0) {
            $commitHash = git rev-parse --short HEAD
            Write-Host "✅ Commit successful: $commitHash" -ForegroundColor Green
            return $commitHash
        } else {
            throw "Git commit failed with exit code $LASTEXITCODE"
        }
    } catch {
        Write-Error "Failed to create commit: $($_.Exception.Message)"
        throw
    }
}

function Generate-CommitMessage {
    <#
    .SYNOPSIS
        Generates intelligent commit messages based on file changes
    #>
    param([object[]]$StagedFiles)

    $categories = $StagedFiles | Group-Object Category
    $primaryCategory = ($categories | Sort-Object Count -Descending)[0]

    $emoji = switch ($primaryCategory.Name) {
        "Documentation" { "📚" }
        "PowerShell" { "🔧" }
        "Workflows" { "🚀" }
        "Source Code" { "✨" }
        "Project Files" { "📦" }
        "Configuration" { "⚙️" }
        default { "🔄" }
    }

    $scope = $primaryCategory.Name.ToLower() -replace ' ', '-'
    $summary = "Update $($primaryCategory.Name.ToLower())"

    if ($categories.Count -gt 1) {
        $otherCategories = ($categories | Where-Object { $_.Name -ne $primaryCategory.Name }).Name -join ", "
        $summary += " and $otherCategories"
    }

    $message = "$emoji $summary`n`nFiles changed:"
    foreach ($category in $categories) {
        $message += "`n- $($category.Name): $($category.Count) files"
    }

    $message += "`n`nGenerated by Bus Buddy GitHub Automation"

    return $message
}

# ============================================================================
# WORKFLOW MONITORING AND MANAGEMENT
# ============================================================================

function Start-WorkflowRun {
    <#
    .SYNOPSIS
        Pushes changes and triggers GitHub Actions workflow
    #>
    [CmdletBinding()]
    param(
        [string]$Branch = $Global:BusBuddyGitHubConfig.DefaultBranch,
        [switch]$Force,
        [switch]$WaitForCompletion
    )

    Write-Host "🚀 Pushing changes to trigger workflow..." -ForegroundColor Cyan

    try {
        # Push changes
        $pushArgs = @("push", "origin", $Branch)
        if ($Force) { $pushArgs += "--force" }

        & git @pushArgs
        if ($LASTEXITCODE -ne 0) {
            throw "Git push failed with exit code $LASTEXITCODE"
        }

        Write-Host "✅ Changes pushed successfully" -ForegroundColor Green

        # Get the latest commit hash
        $commitHash = git rev-parse HEAD
        Write-Host "📍 Commit: $($commitHash.Substring(0, 8))" -ForegroundColor Yellow

        # Wait a moment for GitHub to register the push
        Start-Sleep -Seconds 5

        # Get the triggered workflow run
        $workflowRun = Get-LatestWorkflowRun -CommitSha $commitHash

        if ($workflowRun) {
            Write-Host "🔄 Workflow triggered: $($workflowRun.html_url)" -ForegroundColor Blue

            if ($WaitForCompletion) {
                return Wait-WorkflowCompletion -WorkflowRun $workflowRun
            }

            return $workflowRun
        } else {
            Write-Warning "⚠️ Could not find triggered workflow run"
            return $null
        }

    } catch {
        Write-Error "Failed to start workflow: $($_.Exception.Message)"
        throw
    }
}

function Get-LatestWorkflowRun {
    <#
    .SYNOPSIS
        Gets the latest workflow run for a specific commit
    #>
    [CmdletBinding()]
    param(
        [string]$CommitSha,
        [int]$MaxAttempts = 10,
        [int]$RetryDelay = 3
    )

    $owner = $Global:BusBuddyGitHubConfig.Owner
    $repo = $Global:BusBuddyGitHubConfig.Repo

    for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
        try {
            Write-Host "🔍 Looking for workflow run (attempt $attempt/$MaxAttempts)..." -ForegroundColor Yellow

            $runs = gh api "/repos/$owner/$repo/actions/runs" --paginate | ConvertFrom-Json

            $matchingRun = $runs.workflow_runs | Where-Object {
                $_.head_sha -eq $CommitSha -and
                $_.workflow_id -match $Global:BusBuddyGitHubConfig.WorkflowFile
            } | Select-Object -First 1

            if ($matchingRun) {
                Write-Host "✅ Found workflow run: $($matchingRun.id)" -ForegroundColor Green
                return $matchingRun
            }

            if ($attempt -lt $MaxAttempts) {
                Write-Host "⏳ Workflow not found yet, waiting $RetryDelay seconds..." -ForegroundColor Yellow
                Start-Sleep -Seconds $RetryDelay
            }

        } catch {
            Write-Warning "❌ Error getting workflow run (attempt $attempt): $($_.Exception.Message)"
            if ($attempt -lt $MaxAttempts) {
                Start-Sleep -Seconds $RetryDelay
            }
        }
    }

    Write-Warning "⚠️ Could not find workflow run after $MaxAttempts attempts"
    return $null
}

function Wait-WorkflowCompletion {
    <#
    .SYNOPSIS
        Waits for workflow completion with progress monitoring
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [object]$WorkflowRun,
        [int]$TimeoutSeconds = $Global:BusBuddyGitHubConfig.MaxWaitTime,
        [int]$PollIntervalSeconds = $Global:BusBuddyGitHubConfig.PollInterval
    )

    $owner = $Global:BusBuddyGitHubConfig.Owner
    $repo = $Global:BusBuddyGitHubConfig.Repo
    $runId = $WorkflowRun.id

    Write-Host "⏳ Monitoring workflow completion..." -ForegroundColor Cyan
    Write-Host "🔗 Workflow URL: $($WorkflowRun.html_url)" -ForegroundColor Blue

    $startTime = Get-Date
    $lastStatus = $null

    try {
        while ($true) {
            $elapsed = ((Get-Date) - $startTime).TotalSeconds

            if ($elapsed -gt $TimeoutSeconds) {
                Write-Warning "⏰ Workflow monitoring timed out after $TimeoutSeconds seconds"
                return @{
                    Status = "timeout"
                    ElapsedSeconds = $elapsed
                    WorkflowRun = $WorkflowRun
                }
            }

            # Get current workflow status
            $currentRun = gh api "/repos/$owner/$repo/actions/runs/$runId" | ConvertFrom-Json

            # Show status updates
            if ($currentRun.status -ne $lastStatus) {
                $emoji = switch ($currentRun.status) {
                    "queued" { "📝" }
                    "in_progress" { "🔄" }
                    "completed" { "✅" }
                    default { "📍" }
                }

                Write-Host "$emoji Status: $($currentRun.status) | Conclusion: $($currentRun.conclusion) | Elapsed: $([int]$elapsed)s" -ForegroundColor Yellow
                $lastStatus = $currentRun.status
            }

            # Check if completed
            if ($currentRun.status -eq "completed") {
                $totalTime = [int]$elapsed

                $result = @{
                    Status = "completed"
                    Conclusion = $currentRun.conclusion
                    ElapsedSeconds = $totalTime
                    WorkflowRun = $currentRun
                    Success = $currentRun.conclusion -eq "success"
                }

                if ($result.Success) {
                    Write-Host "🎉 Workflow completed successfully in ${totalTime}s!" -ForegroundColor Green
                } else {
                    Write-Host "❌ Workflow failed with conclusion: $($currentRun.conclusion)" -ForegroundColor Red
                }

                return $result
            }

            # Wait before next poll
            Start-Sleep -Seconds $PollIntervalSeconds
        }

    } catch {
        Write-Error "❌ Error monitoring workflow: $($_.Exception.Message)"
        return @{
            Status = "error"
            Error = $_.Exception.Message
            ElapsedSeconds = ((Get-Date) - $startTime).TotalSeconds
            WorkflowRun = $WorkflowRun
        }
    }
}

# ============================================================================
# WORKFLOW RESULTS ANALYSIS
# ============================================================================

function Get-WorkflowResults {
    <#
    .SYNOPSIS
        Retrieves and analyzes workflow results including logs and artifacts
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [object]$WorkflowRun,
        [switch]$IncludeLogs,
        [switch]$DownloadArtifacts,
        [string]$OutputPath = "./workflow-results"
    )

    $owner = $Global:BusBuddyGitHubConfig.Owner
    $repo = $Global:BusBuddyGitHubConfig.Repo
    $runId = $WorkflowRun.id

    Write-Host "📊 Analyzing workflow results..." -ForegroundColor Cyan

    try {
        # Create output directory
        if (-not (Test-Path $OutputPath)) {
            New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
        }

        # Get detailed workflow run information
        $detailedRun = gh api "/repos/$owner/$repo/actions/runs/$runId" | ConvertFrom-Json

        # Get jobs information
        $jobs = gh api "/repos/$owner/$repo/actions/runs/$runId/jobs" | ConvertFrom-Json

        # Analyze jobs
        $jobResults = @()
        foreach ($job in $jobs.jobs) {
            $jobResult = @{
                Id = $job.id
                Name = $job.name
                Status = $job.status
                Conclusion = $job.conclusion
                StartedAt = $job.started_at
                CompletedAt = $job.completed_at
                DurationSeconds = if ($job.started_at -and $job.completed_at) {
                    ((Get-Date $job.completed_at) - (Get-Date $job.started_at)).TotalSeconds
                } else { $null }
                Steps = @()
                FailedSteps = @()
            }

            # Analyze job steps
            foreach ($step in $job.steps) {
                $stepInfo = @{
                    Name = $step.name
                    Status = $step.status
                    Conclusion = $step.conclusion
                    Number = $step.number
                    StartedAt = $step.started_at
                    CompletedAt = $step.completed_at
                }

                $jobResult.Steps += $stepInfo

                if ($step.conclusion -eq "failure") {
                    $jobResult.FailedSteps += $stepInfo
                }
            }

            $jobResults += $jobResult
        }

        # Get artifacts if requested
        $artifacts = @()
        if ($DownloadArtifacts) {
            Write-Host "📦 Downloading artifacts..." -ForegroundColor Yellow
            $artifactsResponse = gh api "/repos/$owner/$repo/actions/runs/$runId/artifacts" | ConvertFrom-Json

            foreach ($artifact in $artifactsResponse.artifacts) {
                $artifactPath = Join-Path $OutputPath "artifacts"
                if (-not (Test-Path $artifactPath)) {
                    New-Item -ItemType Directory -Path $artifactPath -Force | Out-Null
                }

                try {
                    gh api "/repos/$owner/$repo/actions/artifacts/$($artifact.id)/zip" > "$artifactPath/$($artifact.name).zip"
                    $artifacts += @{
                        Name = $artifact.name
                        DownloadPath = "$artifactPath/$($artifact.name).zip"
                        SizeBytes = $artifact.size_in_bytes
                        CreatedAt = $artifact.created_at
                    }
                    Write-Host "  ✅ Downloaded: $($artifact.name)" -ForegroundColor Green
                } catch {
                    Write-Warning "  ❌ Failed to download artifact: $($artifact.name)"
                }
            }
        }

        # Get logs if requested
        $logs = $null
        if ($IncludeLogs) {
            Write-Host "📄 Downloading logs..." -ForegroundColor Yellow
            try {
                $logsPath = Join-Path $OutputPath "logs.zip"
                gh api "/repos/$owner/$repo/actions/runs/$runId/logs" > $logsPath
                $logs = @{
                    DownloadPath = $logsPath
                    Downloaded = $true
                }
                Write-Host "  ✅ Logs downloaded to: $logsPath" -ForegroundColor Green
            } catch {
                Write-Warning "  ❌ Failed to download logs: $($_.Exception.Message)"
                $logs = @{
                    Downloaded = $false
                    Error = $_.Exception.Message
                }
            }
        }

        # Compile comprehensive results
        $results = @{
            WorkflowRun = $detailedRun
            Jobs = $jobResults
            Artifacts = $artifacts
            Logs = $logs
            Summary = @{
                TotalJobs = $jobs.jobs.Count
                SuccessfulJobs = ($jobResults | Where-Object { $_.Conclusion -eq "success" }).Count
                FailedJobs = ($jobResults | Where-Object { $_.Conclusion -eq "failure" }).Count
                TotalSteps = ($jobResults | ForEach-Object { $_.Steps.Count } | Measure-Object -Sum).Sum
                FailedSteps = ($jobResults | ForEach-Object { $_.FailedSteps.Count } | Measure-Object -Sum).Sum
                OverallSuccess = $detailedRun.conclusion -eq "success"
                TotalDurationSeconds = if ($detailedRun.created_at -and $detailedRun.updated_at) {
                    ((Get-Date $detailedRun.updated_at) - (Get-Date $detailedRun.created_at)).TotalSeconds
                } else { $null }
            }
            OutputPath = $OutputPath
            Timestamp = Get-Date
        }

        # Save results to file
        $resultsFile = Join-Path $OutputPath "workflow-results.json"
        $results | ConvertTo-Json -Depth 10 | Out-File -FilePath $resultsFile -Encoding UTF8
        Write-Host "💾 Results saved to: $resultsFile" -ForegroundColor Green

        # Display summary
        Write-Host "`n📋 Workflow Results Summary:" -ForegroundColor Cyan
        Write-Host "  🎯 Overall Status: $($results.Summary.OverallSuccess ? '✅ Success' : '❌ Failed')" -ForegroundColor ($results.Summary.OverallSuccess ? 'Green' : 'Red')
        Write-Host "  📊 Jobs: $($results.Summary.SuccessfulJobs)/$($results.Summary.TotalJobs) successful" -ForegroundColor White
        Write-Host "  🔧 Steps: $($results.Summary.TotalSteps - $results.Summary.FailedSteps)/$($results.Summary.TotalSteps) successful" -ForegroundColor White
        if ($results.Summary.TotalDurationSeconds) {
            Write-Host "  ⏱️ Duration: $([int]$results.Summary.TotalDurationSeconds) seconds" -ForegroundColor White
        }
        Write-Host "  📁 Output: $OutputPath" -ForegroundColor White

        return $results

    } catch {
        Write-Error "❌ Failed to analyze workflow results: $($_.Exception.Message)"
        throw
    }
}

function Get-WorkflowIssues {
    <#
    .SYNOPSIS
        Analyzes workflow results to identify specific issues and failures
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [object]$WorkflowResults
    )

    Write-Host "🔍 Analyzing workflow issues..." -ForegroundColor Cyan

    $issues = @()

    # Analyze failed jobs
    $failedJobs = $WorkflowResults.Jobs | Where-Object { $_.Conclusion -eq "failure" }

    foreach ($job in $failedJobs) {
        foreach ($failedStep in $job.FailedSteps) {
            $issue = @{
                Type = "StepFailure"
                Severity = "High"
                Job = $job.Name
                Step = $failedStep.Name
                StepNumber = $failedStep.Number
                Category = Get-IssueCategory -StepName $failedStep.Name
                Description = "Step '$($failedStep.Name)' failed in job '$($job.Name)'"
                SuggestedActions = Get-SuggestedActions -StepName $failedStep.Name -Category (Get-IssueCategory -StepName $failedStep.Name)
                AutoFixable = Test-AutoFixable -StepName $failedStep.Name
                Timestamp = $failedStep.CompletedAt
            }

            $issues += $issue
        }
    }

    # Analyze timing issues
    $slowJobs = $WorkflowResults.Jobs | Where-Object {
        $_.DurationSeconds -and $_.DurationSeconds -gt 600  # > 10 minutes
    }

    foreach ($job in $slowJobs) {
        $issues += @{
            Type = "Performance"
            Severity = "Medium"
            Job = $job.Name
            Step = $null
            Category = "Performance"
            Description = "Job '$($job.Name)' took $([int]$job.DurationSeconds) seconds (> 10 minutes)"
            SuggestedActions = @(
                "Review job dependencies and caching strategies",
                "Consider parallelizing slow operations",
                "Check for unnecessary wait times or retries"
            )
            AutoFixable = $false
            Timestamp = $job.CompletedAt
        }
    }

    # Analyze overall workflow health
    if ($WorkflowResults.Summary.TotalDurationSeconds -gt 1800) {  # > 30 minutes
        $issues += @{
            Type = "Performance"
            Severity = "Medium"
            Job = "Overall Workflow"
            Step = $null
            Category = "Performance"
            Description = "Workflow took $([int]$WorkflowResults.Summary.TotalDurationSeconds) seconds (> 30 minutes)"
            SuggestedActions = @(
                "Review overall workflow efficiency",
                "Consider optimizing build and test strategies",
                "Implement better caching mechanisms"
            )
            AutoFixable = $false
            Timestamp = $WorkflowResults.Timestamp
        }
    }

    Write-Host "📊 Found $($issues.Count) issues" -ForegroundColor Yellow

    # Group and display issues
    $issues | Group-Object Category | ForEach-Object {
        Write-Host "  📁 $($_.Name): $($_.Count) issues" -ForegroundColor White
        $_.Group | ForEach-Object {
            $severityColor = switch ($_.Severity) {
                "High" { "Red" }
                "Medium" { "Yellow" }
                "Low" { "Green" }
                default { "White" }
            }
            Write-Host "    $($_.Severity): $($_.Description)" -ForegroundColor $severityColor
        }
    }

    return $issues
}

function Get-IssueCategory {
    <#
    .SYNOPSIS
        Categorizes issues based on the failed step name
    #>
    param([string]$StepName)

    switch -Regex ($StepName) {
        "build|compile" { return "Build" }
        "test|testing" { return "Testing" }
        "restore|dependencies|nuget" { return "Dependencies" }
        "coverage|codecov" { return "Coverage" }
        "artifacts|upload" { return "Artifacts" }
        "setup|install" { return "Environment" }
        default { return "General" }
    }
}

function Get-SuggestedActions {
    <#
    .SYNOPSIS
        Provides suggested actions based on issue category and step name
    #>
    param(
        [string]$StepName,
        [string]$Category
    )

    $actions = switch ($Category) {
        "Build" {
            @(
                "Check for compilation errors in the code",
                "Verify all project references are correct",
                "Ensure all required NuGet packages are restored",
                "Check for syntax errors in C# or XAML files"
            )
        }
        "Testing" {
            @(
                "Review test failures and fix failing tests",
                "Check test project references and dependencies",
                "Verify test data and mock configurations",
                "Ensure test environment is properly configured"
            )
        }
        "Dependencies" {
            @(
                "Clear NuGet cache and restore packages",
                "Check package.lock.json for conflicts",
                "Verify package sources and authentication",
                "Update outdated package references"
            )
        }
        "Coverage" {
            @(
                "Check code coverage collection configuration",
                "Verify test projects are generating coverage data",
                "Review coverage report generation settings",
                "Ensure codecov token is properly configured"
            )
        }
        "Environment" {
            @(
                "Verify .NET SDK version compatibility",
                "Check runner environment requirements",
                "Validate workflow file syntax and actions versions",
                "Ensure all required tools are available"
            )
        }
        default {
            @(
                "Review step logs for specific error messages",
                "Check step configuration and parameters",
                "Verify step dependencies and prerequisites",
                "Consider manual reproduction of the step locally"
            )
        }
    }

    return $actions
}

function Test-AutoFixable {
    <#
    .SYNOPSIS
        Determines if an issue can be automatically fixed
    #>
    param([string]$StepName)

    switch -Regex ($StepName) {
        "restore|dependencies" { return $true }
        "format|style" { return $true }
        default { return $false }
    }
}

# ============================================================================
# AUTOMATED ISSUE RESOLUTION
# ============================================================================

function Invoke-AutomaticIssueFix {
    <#
    .SYNOPSIS
        Attempts to automatically fix identified issues
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [object[]]$Issues,
        [switch]$DryRun,
        [switch]$InteractiveMode
    )

    Write-Host "🔧 Starting automatic issue resolution..." -ForegroundColor Cyan

    $fixableIssues = $Issues | Where-Object { $_.AutoFixable }
    $manualIssues = $Issues | Where-Object { -not $_.AutoFixable }

    Write-Host "📊 Analysis:" -ForegroundColor Yellow
    Write-Host "  🤖 Auto-fixable: $($fixableIssues.Count)" -ForegroundColor Green
    Write-Host "  👤 Manual resolution required: $($manualIssues.Count)" -ForegroundColor Yellow

    $results = @{
        Fixed = @()
        Failed = @()
        Skipped = @()
        Manual = $manualIssues
    }

    # Process auto-fixable issues
    foreach ($issue in $fixableIssues) {
        try {
            Write-Host "`n🔧 Attempting to fix: $($issue.Description)" -ForegroundColor Cyan

            if ($InteractiveMode) {
                $response = Read-Host "Proceed with automatic fix? (y/n)"
                if ($response -notmatch '^[Yy]') {
                    $results.Skipped += $issue
                    Write-Host "⏭️ Skipped by user" -ForegroundColor Yellow
                    continue
                }
            }

            $fixResult = Invoke-IssueFix -Issue $issue -DryRun:$DryRun

            if ($fixResult.Success) {
                $results.Fixed += @{
                    Issue = $issue
                    FixResult = $fixResult
                }
                Write-Host "✅ Fixed successfully" -ForegroundColor Green
            } else {
                $results.Failed += @{
                    Issue = $issue
                    FixResult = $fixResult
                    Error = $fixResult.Error
                }
                Write-Host "❌ Fix failed: $($fixResult.Error)" -ForegroundColor Red
            }

        } catch {
            $results.Failed += @{
                Issue = $issue
                Error = $_.Exception.Message
            }
            Write-Host "❌ Fix failed with exception: $($_.Exception.Message)" -ForegroundColor Red
        }
    }

    # Display manual resolution guidance
    if ($manualIssues.Count -gt 0) {
        Write-Host "`n📋 Manual Resolution Required:" -ForegroundColor Yellow
        foreach ($issue in $manualIssues) {
            Write-Host "`n  ❌ $($issue.Description)" -ForegroundColor Red
            Write-Host "    💡 Suggested actions:" -ForegroundColor Cyan
            $issue.SuggestedActions | ForEach-Object {
                Write-Host "      • $_" -ForegroundColor White
            }
        }
    }

    # Summary
    Write-Host "`n📊 Resolution Summary:" -ForegroundColor Cyan
    Write-Host "  ✅ Successfully fixed: $($results.Fixed.Count)" -ForegroundColor Green
    Write-Host "  ❌ Failed to fix: $($results.Failed.Count)" -ForegroundColor Red
    Write-Host "  ⏭️ Skipped: $($results.Skipped.Count)" -ForegroundColor Yellow
    Write-Host "  👤 Manual resolution required: $($results.Manual.Count)" -ForegroundColor Yellow

    return $results
}

function Invoke-IssueFix {
    <#
    .SYNOPSIS
        Applies specific fixes based on issue type and category
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [object]$Issue,
        [switch]$DryRun
    )

    $fixActions = @()

    switch ($Issue.Category) {
        "Dependencies" {
            $fixActions = @(
                @{
                    Name = "Clear NuGet Cache"
                    Command = "dotnet nuget locals all --clear"
                    Description = "Clears all NuGet caches"
                },
                @{
                    Name = "Restore Packages"
                    Command = "dotnet restore BusBuddy.sln --force --no-cache"
                    Description = "Forces package restoration without cache"
                }
            )
        }
        "Build" {
            $fixActions = @(
                @{
                    Name = "Clean Solution"
                    Command = "dotnet clean BusBuddy.sln"
                    Description = "Cleans build artifacts"
                },
                @{
                    Name = "Restore Dependencies"
                    Command = "dotnet restore BusBuddy.sln"
                    Description = "Restores package dependencies"
                },
                @{
                    Name = "Rebuild Solution"
                    Command = "dotnet build BusBuddy.sln --configuration Release"
                    Description = "Rebuilds the solution"
                }
            )
        }
        default {
            return @{
                Success = $false
                Error = "No automatic fix available for category: $($Issue.Category)"
                Actions = @()
            }
        }
    }

    $results = @()

    foreach ($action in $fixActions) {
        try {
            Write-Host "  🔧 $($action.Name): $($action.Description)" -ForegroundColor Yellow

            if ($DryRun) {
                Write-Host "    🔍 Dry run: $($action.Command)" -ForegroundColor Cyan
                $results += @{
                    Action = $action.Name
                    Success = $true
                    DryRun = $true
                    Command = $action.Command
                }
            } else {
                $output = Invoke-Expression $action.Command 2>&1

                if ($LASTEXITCODE -eq 0) {
                    $results += @{
                        Action = $action.Name
                        Success = $true
                        Output = $output
                        Command = $action.Command
                    }
                    Write-Host "    ✅ Success" -ForegroundColor Green
                } else {
                    $results += @{
                        Action = $action.Name
                        Success = $false
                        Error = $output
                        ExitCode = $LASTEXITCODE
                        Command = $action.Command
                    }
                    Write-Host "    ❌ Failed (Exit Code: $LASTEXITCODE)" -ForegroundColor Red
                }
            }

        } catch {
            $results += @{
                Action = $action.Name
                Success = $false
                Error = $_.Exception.Message
                Command = $action.Command
            }
            Write-Host "    ❌ Exception: $($_.Exception.Message)" -ForegroundColor Red
        }
    }

    $overallSuccess = $results | Where-Object { $_.Success } | Measure-Object | Select-Object -ExpandProperty Count
    $totalActions = $results.Count

    return @{
        Success = $overallSuccess -eq $totalActions
        Actions = $results
        SuccessRate = if ($totalActions -gt 0) { $overallSuccess / $totalActions } else { 0 }
        Error = if ($overallSuccess -ne $totalActions) {
            "Only $overallSuccess of $totalActions actions succeeded"
        } else {
            $null
        }
    }
}

# ============================================================================
# HIGH-LEVEL WORKFLOW FUNCTIONS
# ============================================================================

function Invoke-CompleteGitHubWorkflow {
    <#
    .SYNOPSIS
        Complete end-to-end GitHub workflow: stage, commit, push, monitor, analyze, and fix
    #>
    [CmdletBinding()]
    param(
        [string]$CommitMessage,
        [switch]$GenerateCommitMessage,
        [switch]$InteractiveStaging,
        [switch]$WaitForCompletion = $true,
        [switch]$AnalyzeResults = $true,
        [switch]$AutoFix = $true,
        [switch]$DryRun,
        [string]$OutputPath = "./workflow-results"
    )

    Write-Host "🚀 Starting complete GitHub workflow automation..." -ForegroundColor Cyan
    Write-Host "🔗 Repository: $($Global:BusBuddyGitHubConfig.Owner)/$($Global:BusBuddyGitHubConfig.Repo)" -ForegroundColor Blue

    try {
        # Initialize environment
        $env = Initialize-GitHubAutomation
        Write-Host "✅ Environment initialized" -ForegroundColor Green

        # Stage changes
        Write-Host "`n📦 STAGE 1: Staging Changes" -ForegroundColor Magenta
        $stagedFiles = Invoke-SmartGitStaging -InteractiveMode:$InteractiveStaging -DryRun:$DryRun

        if ($stagedFiles.Count -eq 0) {
            Write-Host "ℹ️ No changes to commit" -ForegroundColor Yellow
            return @{
                Stage = "staging"
                Success = $true
                Message = "No changes to commit"
            }
        }

        if ($DryRun) {
            Write-Host "🔍 Dry run completed - no further actions taken" -ForegroundColor Yellow
            return @{
                Stage = "dry-run"
                Success = $true
                StagedFiles = $stagedFiles
            }
        }

        # Commit changes
        Write-Host "`n📝 STAGE 2: Creating Commit" -ForegroundColor Magenta
        $commitHash = New-IntelligentCommit -CustomMessage $CommitMessage -StagedFiles $stagedFiles -GenerateMessage:$GenerateCommitMessage

        # Start workflow
        Write-Host "`n🚀 STAGE 3: Starting Workflow" -ForegroundColor Magenta
        $workflowResult = Start-WorkflowRun -WaitForCompletion:$WaitForCompletion

        if (-not $workflowResult) {
            throw "Failed to start or find workflow run"
        }

        $result = @{
            Stage = "workflow-started"
            Success = $true
            CommitHash = $commitHash
            StagedFiles = $stagedFiles
            WorkflowRun = $workflowResult.WorkflowRun
        }

        if (-not $WaitForCompletion) {
            Write-Host "✅ Workflow started - monitoring skipped" -ForegroundColor Green
            return $result
        }

        # Analyze results
        if ($AnalyzeResults -and $workflowResult.Status -eq "completed") {
            Write-Host "`n📊 STAGE 4: Analyzing Results" -ForegroundColor Magenta
            $workflowResults = Get-WorkflowResults -WorkflowRun $workflowResult.WorkflowRun -IncludeLogs -DownloadArtifacts -OutputPath $OutputPath

            $result.WorkflowResults = $workflowResults
            $result.Stage = "analyzed"

            # Identify issues
            if (-not $workflowResults.Summary.OverallSuccess) {
                Write-Host "`n🔍 STAGE 5: Identifying Issues" -ForegroundColor Magenta
                $issues = Get-WorkflowIssues -WorkflowResults $workflowResults
                $result.Issues = $issues

                # Attempt automatic fixes
                if ($AutoFix -and $issues.Count -gt 0) {
                    Write-Host "`n🔧 STAGE 6: Automatic Issue Resolution" -ForegroundColor Magenta
                    $fixResults = Invoke-AutomaticIssueFix -Issues $issues -InteractiveMode:$InteractiveStaging
                    $result.FixResults = $fixResults
                    $result.Stage = "fixed"
                }
            }
        }

        # Final summary
        Write-Host "`n🎉 WORKFLOW COMPLETE" -ForegroundColor Green
        Write-Host "📊 Summary:" -ForegroundColor Cyan
        Write-Host "  📁 Files staged: $($stagedFiles.Count)" -ForegroundColor White
        Write-Host "  📝 Commit: $commitHash" -ForegroundColor White
        Write-Host "  🔄 Workflow: $($workflowResult.Success ? '✅ Success' : '❌ Failed')" -ForegroundColor ($workflowResult.Success ? 'Green' : 'Red')

        if ($result.Issues) {
            Write-Host "  🐛 Issues found: $($result.Issues.Count)" -ForegroundColor Red
            if ($result.FixResults) {
                Write-Host "  🔧 Issues fixed: $($result.FixResults.Fixed.Count)" -ForegroundColor Green
            }
        }

        return $result

    } catch {
        Write-Error "❌ Workflow failed: $($_.Exception.Message)"
        return @{
            Stage = "error"
            Success = $false
            Error = $_.Exception.Message
        }
    }
}

# ============================================================================
# CONVENIENCE ALIASES AND SHORTCUTS
# ============================================================================

# Short aliases for common operations
Set-Alias -Name "bb-git-auto" -Value "Invoke-CompleteGitHubWorkflow"
Set-Alias -Name "bb-git-stage" -Value "Invoke-SmartGitStaging"
Set-Alias -Name "bb-git-commit" -Value "New-IntelligentCommit"
Set-Alias -Name "bb-git-push" -Value "Start-WorkflowRun"
Set-Alias -Name "bb-git-monitor" -Value "Wait-WorkflowCompletion"
Set-Alias -Name "bb-git-analyze" -Value "Get-WorkflowResults"
Set-Alias -Name "bb-git-fix" -Value "Invoke-AutomaticIssueFix"

# Export main functions
Export-ModuleMember -Function @(
    'Initialize-GitHubAutomation',
    'Invoke-SmartGitStaging',
    'New-IntelligentCommit',
    'Start-WorkflowRun',
    'Wait-WorkflowCompletion',
    'Get-WorkflowResults',
    'Get-WorkflowIssues',
    'Invoke-AutomaticIssueFix',
    'Invoke-CompleteGitHubWorkflow'
) -Alias @(
    'bb-git-auto',
    'bb-git-stage',
    'bb-git-commit',
    'bb-git-push',
    'bb-git-monitor',
    'bb-git-analyze',
    'bb-git-fix'
)

Write-Host "🚌 Bus Buddy GitHub Automation loaded successfully!" -ForegroundColor Green
Write-Host "💡 Use 'bb-git-auto' for complete automated workflow" -ForegroundColor Cyan
Write-Host "📚 Available commands: bb-git-stage, bb-git-commit, bb-git-push, bb-git-monitor, bb-git-analyze, bb-git-fix" -ForegroundColor Yellow
