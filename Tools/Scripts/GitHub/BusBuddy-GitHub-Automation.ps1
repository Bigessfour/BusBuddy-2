#Requires -Version 7.5

# ═══════════════════════════════════════════════════════════════════════════════
# 🚀 BusBuddy GitHub Automation Script
# ═══════════════════════════════════════════════════════════════════════════════
# Version: 1.0.0
# Created: July 25, 2025
# Purpose: Comprehensive GitHub workflow automation with output capture
# Features: Smart staging, intelligent commits, push automation, workflow monitoring
# ═══════════════════════════════════════════════════════════════════════════════

param(
    [switch]$GenerateCommitMessage,
    [switch]$WaitForCompletion,
    [switch]$AnalyzeResults,
    [switch]$AutoFix,
    [switch]$InteractiveMode,
    [switch]$IncludeLogs,
    [switch]$DownloadArtifacts,
    [string]$CommitMessage,
    [string]$Branch = "main",
    [string]$Repository = "Bigessfour/BusBuddy-2"
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = "Stop"

# ═══════════════════════════════════════════════════════════════════════════════
# 🌟 GLOBAL CONFIGURATION
# ═══════════════════════════════════════════════════════════════════════════════

$script:GitHubConfig = @{
    Repository        = $Repository
    Branch            = $Branch
    WorkflowOutputDir = "logs/github-workflows"
    MaxRetries        = 3
    RetryDelay        = 5
    TimeoutMinutes    = 30
}

# Ensure output directory exists
$outputDir = Join-Path $PWD $script:GitHubConfig.WorkflowOutputDir
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

# ═══════════════════════════════════════════════════════════════════════════════
# 🛠️ UTILITY FUNCTIONS
# ═══════════════════════════════════════════════════════════════════════════════

function Write-GitHubStatus {
    param(
        [string]$Message,
        [ValidateSet('Info', 'Success', 'Warning', 'Error')]
        [string]$Status = 'Info'
    )

    $colors = @{
        Info    = 'Cyan'
        Success = 'Green'
        Warning = 'Yellow'
        Error   = 'Red'
    }

    $icons = @{
        Info    = '📡'
        Success = '✅'
        Warning = '⚠️'
        Error   = '❌'
    }

    Write-Host "$($icons[$Status]) $Message" -ForegroundColor $colors[$Status]
}

function Invoke-GitCommand {
    param(
        [string[]]$Arguments,
        [switch]$CaptureOutput,
        [bool]$ThrowOnError = $true
    )

    try {
        if ($CaptureOutput) {
            $result = & git @Arguments 2>&1
            $exitCode = $LASTEXITCODE

            return [PSCustomObject]@{
                Output   = $result
                ExitCode = $exitCode
                Success  = ($exitCode -eq 0)
            }
        }
        else {
            & git @Arguments
            return [PSCustomObject]@{
                Output   = $null
                ExitCode = $LASTEXITCODE
                Success  = ($LASTEXITCODE -eq 0)
            }
        }
    }
    catch {
        if ($ThrowOnError) {
            throw "Git command failed: $($_.Exception.Message)"
        }
        return [PSCustomObject]@{
            Output   = $_.Exception.Message
            ExitCode = -1
            Success  = $false
        }
    }
}

function Save-WorkflowOutput {
    param(
        [string]$Content,
        [string]$FileName,
        [string]$Type = "log"
    )

    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $fullFileName = "${timestamp}-${FileName}.${Type}"
    $filePath = Join-Path $script:GitHubConfig.WorkflowOutputDir $fullFileName

    $Content | Out-File -FilePath $filePath -Encoding UTF8
    Write-GitHubStatus "Saved $Type to: $filePath" -Status Info

    return $filePath
}

# ═══════════════════════════════════════════════════════════════════════════════
# 🎯 CORE GITHUB AUTOMATION FUNCTIONS
# ═══════════════════════════════════════════════════════════════════════════════

function Invoke-SmartGitStaging {
    param(
        [switch]$InteractiveMode
    )

    Write-GitHubStatus "🔍 Analyzing repository changes..." -Status Info

    # Get current status
    $statusResult = Invoke-GitCommand -Arguments @('status', '--porcelain') -CaptureOutput
    if (-not $statusResult.Success) {
        throw "Failed to get git status: $($statusResult.Output)"
    }

    if (-not $statusResult.Output) {
        Write-GitHubStatus "No changes detected in repository" -Status Warning
        return @()
    }

    # Parse status output
    $changes = $statusResult.Output | ForEach-Object {
        if ($_ -match '^(.{2})\s(.+)$') {
            [PSCustomObject]@{
                Status            = $matches[1]
                File              = $matches[2]
                StatusDescription = switch ($matches[1]) {
                    '??' { 'Untracked' }
                    ' M' { 'Modified' }
                    'M ' { 'Staged Modified' }
                    'A ' { 'Staged Added' }
                    'D ' { 'Staged Deleted' }
                    ' D' { 'Deleted' }
                    'MM' { 'Modified (staged and unstaged)' }
                    default { $matches[1] }
                }
            }
        }
    }

    Write-GitHubStatus "Found $($changes.Count) changes:" -Status Info
    $changes | ForEach-Object {
        Write-Host "  $($_.StatusDescription): $($_.File)" -ForegroundColor Gray
    }

    if ($InteractiveMode) {
        Write-Host "`n📋 Select files to stage:" -ForegroundColor Cyan
        $selectedFiles = @()

        for ($i = 0; $i -lt $changes.Count; $i++) {
            $choice = Read-Host "Stage '$($changes[$i].File)' ($($changes[$i].StatusDescription))? [Y/n/q]"
            switch ($choice.ToLower()) {
                'q' { break }
                'n' { continue }
                default {
                    $selectedFiles += $changes[$i].File
                    Write-GitHubStatus "✓ Staged: $($changes[$i].File)" -Status Success
                }
            }
        }

        $filesToStage = $selectedFiles
    }
    else {
        # Auto-stage logic: stage all modified and new files, but ask about deletions
        $filesToStage = $changes | Where-Object {
            $_.Status -notmatch 'D'
        } | ForEach-Object { $_.File }

        $deletedFiles = $changes | Where-Object { $_.Status -match 'D' }
        if ($deletedFiles) {
            Write-GitHubStatus "⚠️ Found deleted files - manual review recommended:" -Status Warning
            $deletedFiles | ForEach-Object {
                Write-Host "  Deleted: $($_.File)" -ForegroundColor Red
            }
        }
    }

    # Stage selected files
    if ($filesToStage.Count -gt 0) {
        foreach ($file in $filesToStage) {
            $addResult = Invoke-GitCommand -Arguments @('add', $file) -CaptureOutput
            if (-not $addResult.Success) {
                Write-GitHubStatus "Failed to stage $file`: $($addResult.Output)" -Status Error
            }
        }

        Write-GitHubStatus "Successfully staged $($filesToStage.Count) files" -Status Success
    }

    return $filesToStage
}

function New-IntelligentCommit {
    param(
        [string[]]$StagedFiles,
        [switch]$GenerateMessage,
        [string]$CustomMessage
    )

    if ($StagedFiles.Count -eq 0) {
        Write-GitHubStatus "No files to commit" -Status Warning
        return $null
    }

    $commitMessage = if ($CustomMessage) {
        $CustomMessage
    }
    elseif ($GenerateMessage) {
        # Generate intelligent commit message based on file changes
        $detectedType = 'chore'

        # Simple heuristic for commit type
        if ($StagedFiles -match 'test|Test') { $detectedType = 'test' }
        elseif ($StagedFiles -match '\.md$|\.txt$') { $detectedType = 'docs' }
        elseif ($StagedFiles -match '\.cs$|\.xaml$') { $detectedType = 'feat' }

        $scope = if ($StagedFiles.Count -eq 1) {
            [System.IO.Path]::GetFileNameWithoutExtension($StagedFiles[0])
        }
        else {
            "multiple-files"
        }

        "${detectedType}($scope): Update $($StagedFiles.Count) file$(if($StagedFiles.Count -gt 1){'s'})"
    }
    else {
        Read-Host "Enter commit message"
    }

    if ([string]::IsNullOrWhiteSpace($commitMessage)) {
        throw "Commit message cannot be empty"
    }

    Write-GitHubStatus "💾 Committing changes: '$commitMessage'" -Status Info

    $commitResult = Invoke-GitCommand -Arguments @('commit', '-m', $commitMessage) -CaptureOutput
    if (-not $commitResult.Success) {
        throw "Failed to commit: $($commitResult.Output)"
    }

    Write-GitHubStatus "✅ Successfully committed changes" -Status Success

    # Capture commit details
    $commitHash = Invoke-GitCommand -Arguments @('rev-parse', 'HEAD') -CaptureOutput
    $commitDetails = @{
        Hash      = if ($commitHash.Success) { $commitHash.Output.Trim() } else { 'unknown' }
        Message   = $commitMessage
        Files     = $StagedFiles
        Timestamp = Get-Date
    }

    Save-WorkflowOutput -Content ($commitDetails | ConvertTo-Json -Depth 3) -FileName "commit-details" -Type "json"

    return $commitDetails
}

function Start-WorkflowRun {
    param(
        [switch]$WaitForCompletion,
        [int]$TimeoutMinutes = $script:GitHubConfig.TimeoutMinutes
    )

    Write-GitHubStatus "🚀 Pushing changes to GitHub..." -Status Info

    # Push to remote
    $pushResult = Invoke-GitCommand -Arguments @('push', 'origin', $script:GitHubConfig.Branch) -CaptureOutput
    if (-not $pushResult.Success) {
        throw "Failed to push: $($pushResult.Output)"
    }

    Write-GitHubStatus "✅ Successfully pushed to $($script:GitHubConfig.Branch)" -Status Success

    if (-not $WaitForCompletion) {
        Write-GitHubStatus "Push completed. Use 'bb-get-workflow-results' to monitor workflows." -Status Info
        return $null
    }

    # Wait for and monitor workflow
    Write-GitHubStatus "⏳ Monitoring workflow execution..." -Status Info

    $startTime = Get-Date
    $timeoutTime = $startTime.AddMinutes($TimeoutMinutes)

    do {
        Start-Sleep -Seconds 10

        $workflowResults = Get-WorkflowResults -Count 1 -IncludeLogs:$false

        if ($workflowResults -and $workflowResults.Count -gt 0) {
            $latestRun = $workflowResults[0]

            Write-GitHubStatus "Workflow Status: $($latestRun.status) | Conclusion: $($latestRun.conclusion)" -Status Info

            if ($latestRun.status -eq 'completed') {
                if ($latestRun.conclusion -eq 'success') {
                    Write-GitHubStatus "🎉 Workflow completed successfully!" -Status Success
                }
                else {
                    Write-GitHubStatus "❌ Workflow failed with conclusion: $($latestRun.conclusion)" -Status Error
                }
                break
            }
        }

        if ((Get-Date) -gt $timeoutTime) {
            Write-GitHubStatus "⏰ Workflow monitoring timed out after $TimeoutMinutes minutes" -Status Warning
            break
        }

    } while ($true)

    return $workflowResults
}

function Get-WorkflowResults {
    param(
        [int]$Count = 5,
        [ValidateSet('completed', 'in_progress', 'queued', 'all')]
        [string]$Status = 'all',
        [switch]$IncludeLogs,
        [switch]$DownloadArtifacts,
        [object]$WorkflowRun = $null
    )

    Write-GitHubStatus "📊 Retrieving workflow results..." -Status Info

    $results = @()

    try {
        # Try GitHub CLI first
        if (Get-Command gh -ErrorAction SilentlyContinue) {
            # Updated field names for GitHub CLI compatibility
            $ghArgs = @('run', 'list', '--limit', $Count, '--json', 'status,conclusion,createdAt,name,workflowName,databaseId,event,headBranch,url')
            if ($Status -ne 'all') {
                $ghArgs += '--status', $Status
            }

            $ghOutput = & gh @ghArgs 2>&1
            if ($LASTEXITCODE -eq 0) {
                $results = $ghOutput | ConvertFrom-Json
                # Convert databaseId to id for compatibility
                $results = $results | ForEach-Object {
                    $_ | Add-Member -NotePropertyName 'id' -NotePropertyValue $_.databaseId -Force
                    $_
                }
                Write-GitHubStatus "✅ Retrieved $($results.Count) workflow runs via GitHub CLI" -Status Success
            }
            else {
                Write-GitHubStatus "⚠️ GitHub CLI failed, trying alternative method..." -Status Warning
                $results = @()
            }
        }

        # Fallback to REST API if needed
        if ($results.Count -eq 0 -and $env:GITHUB_TOKEN) {
            $headers = @{
                Authorization = "token $env:GITHUB_TOKEN"
                Accept        = "application/vnd.github.v3+json"
            }

            $queryParams = @("per_page=$Count")
            if ($Status -ne 'all') { $queryParams += "status=$Status" }
            $fullUrl = "https://api.github.com/repos/$($script:GitHubConfig.Repository)/actions/runs?$($queryParams -join '&')"

            $response = Invoke-RestMethod -Uri $fullUrl -Headers $headers -Method Get
            $results = $response.workflow_runs | Select-Object -First $Count | ForEach-Object {
                [PSCustomObject]@{
                    id           = $_.id
                    name         = $_.name
                    workflowName = $_.name
                    status       = $_.status
                    conclusion   = $_.conclusion
                    createdAt    = $_.created_at
                    event        = $_.event
                    headBranch   = $_.head_branch
                    url          = $_.html_url
                }
            }
        }

        # Save workflow results
        if ($results.Count -gt 0) {
            $outputContent = @{
                Timestamp  = Get-Date
                Count      = $results.Count
                Repository = $script:GitHubConfig.Repository
                Results    = $results
            }

            Save-WorkflowOutput -Content ($outputContent | ConvertTo-Json -Depth 5) -FileName "workflow-results" -Type "json"

            # Download logs if requested
            if ($IncludeLogs) {
                foreach ($run in $results) {
                    try {
                        Write-GitHubStatus "📥 Downloading logs for run $($run.id)..." -Status Info

                        if (Get-Command gh -ErrorAction SilentlyContinue) {
                            & gh run download $run.id --dir "$($script:GitHubConfig.WorkflowOutputDir)/logs-$($run.id)" 2>&1 | Out-Null
                            if ($LASTEXITCODE -eq 0) {
                                Write-GitHubStatus "✅ Downloaded logs for run $($run.id)" -Status Success
                            }
                        }
                    }
                    catch {
                        Write-GitHubStatus "⚠️ Failed to download logs for run $($run.id): $($_.Exception.Message)" -Status Warning
                    }
                }
            }
        }

        return $results

    }
    catch {
        Write-GitHubStatus "❌ Error retrieving workflow results: $($_.Exception.Message)" -Status Error
        throw
    }
}

function Get-WorkflowIssues {
    param(
        [object]$WorkflowResults
    )

    $issues = @()

    if (-not $WorkflowResults -or $WorkflowResults.Count -eq 0) {
        return $issues
    }

    foreach ($run in $WorkflowResults) {
        if ($run.conclusion -ne 'success' -and $run.status -eq 'completed') {
            $issue = [PSCustomObject]@{
                RunId        = $run.id
                WorkflowName = $run.workflowName
                Status       = $run.status
                Conclusion   = $run.conclusion
                CreatedAt    = $run.createdAt
                Branch       = $run.headBranch
                Url          = $run.url
                IssueType    = switch ($run.conclusion) {
                    'failure' { 'Build/Test Failure' }
                    'cancelled' { 'Workflow Cancelled' }
                    'timed_out' { 'Workflow Timeout' }
                    default { 'Unknown Issue' }
                }
                Severity     = if ($run.conclusion -eq 'failure') { 'High' } else { 'Medium' }
            }

            $issues += $issue
        }
    }

    return $issues
}

function Invoke-AutomaticIssueFix {
    param(
        [object[]]$Issues,
        [switch]$InteractiveMode
    )

    if (-not $Issues -or $Issues.Count -eq 0) {
        Write-GitHubStatus "No issues found to fix" -Status Success
        return
    }

    Write-GitHubStatus "🔧 Found $($Issues.Count) workflow issues:" -Status Warning

    foreach ($issue in $Issues) {
        Write-Host "`n🔍 Issue: $($issue.IssueType)" -ForegroundColor Yellow
        Write-Host "   Workflow: $($issue.WorkflowName)" -ForegroundColor Gray
        Write-Host "   Conclusion: $($issue.Conclusion)" -ForegroundColor Gray
        Write-Host "   URL: $($issue.Url)" -ForegroundColor Blue

        if ($InteractiveMode) {
            $action = Read-Host "Choose action: [V]iew logs, [R]etry, [I]gnore, [Q]uit"
            switch ($action.ToLower()) {
                'v' {
                    if ($issue.Url) {
                        Start-Process $issue.Url
                    }
                }
                'r' {
                    Write-GitHubStatus "🔄 Retrying workflow..." -Status Info
                    # Could implement retry logic here
                }
                'q' { break }
                default { continue }
            }
        }
    }

    # Save issues report
    $issueReport = @{
        Timestamp   = Get-Date
        TotalIssues = $Issues.Count
        Issues      = $Issues
        Repository  = $script:GitHubConfig.Repository
    }

    Save-WorkflowOutput -Content ($issueReport | ConvertTo-Json -Depth 5) -FileName "workflow-issues" -Type "json"
}

# ═══════════════════════════════════════════════════════════════════════════════
# 🎯 MAIN WORKFLOW FUNCTIONS
# ═══════════════════════════════════════════════════════════════════════════════

function Invoke-CompleteGitHubWorkflow {
    param(
        [switch]$GenerateCommitMessage,
        [switch]$WaitForCompletion,
        [switch]$AnalyzeResults,
        [switch]$AutoFix,
        [switch]$InteractiveMode,
        [string]$CommitMessage
    )

    Write-GitHubStatus "🚀 Starting complete GitHub workflow automation..." -Status Info

    try {
        # Step 1: Smart staging
        $stagedFiles = Invoke-SmartGitStaging -InteractiveMode:$InteractiveMode

        if ($stagedFiles.Count -eq 0) {
            Write-GitHubStatus "No files to commit. Workflow completed." -Status Warning
            return
        }

        # Step 2: Intelligent commit
        $commitDetails = New-IntelligentCommit -StagedFiles $stagedFiles -GenerateMessage:$GenerateCommitMessage -CustomMessage $CommitMessage

        # Step 3: Push and monitor
        $workflowResults = Start-WorkflowRun -WaitForCompletion:$WaitForCompletion

        # Step 4: Analyze results if requested
        if ($AnalyzeResults -and $workflowResults) {
            $issues = Get-WorkflowIssues -WorkflowResults $workflowResults

            if ($AutoFix -and $issues.Count -gt 0) {
                Invoke-AutomaticIssueFix -Issues $issues -InteractiveMode:$InteractiveMode
            }
        }

        Write-GitHubStatus "🎉 Complete GitHub workflow automation finished successfully!" -Status Success

        return @{
            StagedFiles     = $stagedFiles
            CommitDetails   = $commitDetails
            WorkflowResults = $workflowResults
        }

    }
    catch {
        Write-GitHubStatus "❌ Workflow automation failed: $($_.Exception.Message)" -Status Error
        throw
    }
}

# ═══════════════════════════════════════════════════════════════════════════════
# 🎯 SCRIPT ENTRY POINT
# ═══════════════════════════════════════════════════════════════════════════════

# If script is run directly (not dot-sourced), execute the complete workflow
if ($MyInvocation.InvocationName -ne '.') {
    try {
        Invoke-CompleteGitHubWorkflow -GenerateCommitMessage:$GenerateCommitMessage -WaitForCompletion:$WaitForCompletion -AnalyzeResults:$AnalyzeResults -AutoFix:$AutoFix -CommitMessage $CommitMessage
    }
    catch {
        Write-GitHubStatus "❌ Script execution failed: $($_.Exception.Message)" -Status Error
        exit 1
    }
}

Write-GitHubStatus "📚 BusBuddy GitHub Automation script loaded successfully" -Status Success
Write-Host "Available functions:" -ForegroundColor Cyan
Write-Host "  • Invoke-CompleteGitHubWorkflow" -ForegroundColor Gray
Write-Host "  • Invoke-SmartGitStaging" -ForegroundColor Gray
Write-Host "  • New-IntelligentCommit" -ForegroundColor Gray
Write-Host "  • Start-WorkflowRun" -ForegroundColor Gray
Write-Host "  • Get-WorkflowResults" -ForegroundColor Gray
Write-Host "  • Get-WorkflowIssues" -ForegroundColor Gray
Write-Host "  • Invoke-AutomaticIssueFix" -ForegroundColor Gray
