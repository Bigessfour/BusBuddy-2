#Requires -Version 7.0
<#
.SYNOPSIS
BusBuddy Solid Workflow Implementation

.DESCRIPTION
Implements the proven workflow patterns from SOLID-WORKFLOW-GUIDE.md
Uses reliable CMD-based commands and automated error detection.

.EXAMPLE
.\BusBuddy-Solid-Workflow.ps1 -Action "Health"
Run health check

.EXAMPLE
.\BusBuddy-Solid-Workflow.ps1 -Action "Build"
Quick build

.EXAMPLE
.\BusBuddy-Solid-Workflow.ps1 -Action "FullCycle"
Complete development cycle
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("Health", "Build", "FullCycle", "Fix", "Clean")]
    [string]$Action = "Health",

    [switch]$AutoFix
)

# Global variables for workflow state
$Script:WorkflowResults = @{}
$Script:StartTime = Get-Date
$Script:LogPath = "logs\workflow-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"

# ===================================================================
# CORE WORKFLOW FUNCTIONS
# ===================================================================

function Write-WorkflowLog {
    param(
        [string]$Message,
        [ValidateSet("INFO", "SUCCESS", "WARN", "ERROR")]
        [string]$Level = "INFO"
    )

    $timestamp = Get-Date -Format "HH:mm:ss"
    $logEntry = "[$timestamp] [$Level] $Message"

    $color = switch ($Level) {
        "ERROR" { "Red" }
        "WARN" { "Yellow" }
        "SUCCESS" { "Green" }
        "INFO" { "Cyan" }
        default { "White" }
    }

    Write-Host $logEntry -ForegroundColor $color

    # Ensure log directory exists
    $logDir = Split-Path $Script:LogPath -Parent
    if (-not (Test-Path $logDir)) {
        New-Item -Path $logDir -ItemType Directory -Force | Out-Null
    }

    Add-Content -Path $Script:LogPath -Value $logEntry -Encoding UTF8
}

function Invoke-ReliableCommand {
    param(
        [string]$Command,
        [string]$Description
    )

    Write-WorkflowLog "🚀 Starting: $Description" "INFO"
    $commandStart = Get-Date

    try {
        # Use reliable CMD approach from SOLID-WORKFLOW-GUIDE
        Write-WorkflowLog "Executing: $Command" "INFO"
        $output = cmd /c "$Command 2>&1"
        $exitCode = $LASTEXITCODE
        $duration = (Get-Date) - $commandStart

        $result = @{
            Command     = $Command
            Description = $Description
            ExitCode    = $exitCode
            Output      = $output
            Duration    = $duration.TotalSeconds
            Success     = ($exitCode -eq 0)
            Timestamp   = Get-Date
        }

        if ($result.Success) {
            Write-WorkflowLog "✅ SUCCESS: $Description ($($duration.TotalSeconds.ToString('F2'))s)" "SUCCESS"
        }
        else {
            Write-WorkflowLog "❌ FAILED: $Description (Exit: $exitCode)" "ERROR"
            if ($output) {
                Write-WorkflowLog "Output: $($output | Select-Object -First 3 | Out-String)" "ERROR"
            }
        }

        return $result
    }
    catch {
        Write-WorkflowLog "💥 EXCEPTION: $Description - $($_.Exception.Message)" "ERROR"
        return @{
            Command     = $Command
            Description = $Description
            ExitCode    = -1
            Output      = $_.Exception.Message
            Duration    = 0
            Success     = $false
            Timestamp   = Get-Date
        }
    }
}

function Test-WorkspaceHealth {
    Write-WorkflowLog "🏥 Running workspace health check" "INFO"

    $healthResults = @{}

    # Check required files
    $requiredFiles = @("BusBuddy.sln", "BusBuddy.Core", "BusBuddy.WPF")
    $missingFiles = @()

    foreach ($file in $requiredFiles) {
        if (-not (Test-Path $file)) {
            $missingFiles += $file
        }
    }

    $healthResults.FileStructure = @{
        Success = ($missingFiles.Count -eq 0)
        Missing = $missingFiles
    }

    # Check .NET SDK
    $healthResults.DotNetCheck = Invoke-ReliableCommand "dotnet --version" "Check .NET SDK"

    # Quick build test
    $healthResults.QuickBuild = Invoke-ReliableCommand "dotnet build BusBuddy.sln --verbosity minimal --nologo" "Quick Build Test"

    # Overall health assessment
    $healthResults.Overall = @{
        Success   = $healthResults.FileStructure.Success -and $healthResults.DotNetCheck.Success -and $healthResults.QuickBuild.Success
        Timestamp = Get-Date
    }

    if ($healthResults.Overall.Success) {
        Write-WorkflowLog "🎉 Workspace is healthy!" "SUCCESS"
    }
    else {
        Write-WorkflowLog "⚠️ Workspace has issues that need attention" "WARN"
    }

    return $healthResults
}

function Start-QuickBuild {
    Write-WorkflowLog "🔨 Starting quick build" "INFO"

    return Invoke-ReliableCommand "dotnet build BusBuddy.sln --verbosity normal" "Quick Build"
}

function Start-FullCycle {
    Write-WorkflowLog "🔄 Starting full development cycle" "INFO"

    $cycle = @{}

    # Step 1: Clean
    $cycle.Clean = Invoke-ReliableCommand "dotnet clean BusBuddy.sln --verbosity minimal" "Clean Solution"
    if (-not $cycle.Clean.Success) {
        Write-WorkflowLog "❌ Full cycle stopped at Clean step" "ERROR"
        return $cycle
    }

    # Step 2: Restore
    $cycle.Restore = Invoke-ReliableCommand "dotnet restore BusBuddy.sln --verbosity minimal" "Restore Packages"
    if (-not $cycle.Restore.Success) {
        Write-WorkflowLog "❌ Full cycle stopped at Restore step" "ERROR"
        return $cycle
    }

    # Step 3: Build
    $cycle.Build = Invoke-ReliableCommand "dotnet build BusBuddy.sln --verbosity normal" "Build Solution"
    if (-not $cycle.Build.Success) {
        Write-WorkflowLog "❌ Full cycle stopped at Build step" "ERROR"
        return $cycle
    }

    # Step 4: Test (if available)
    if (Test-Path "BusBuddy.Tests") {
        $cycle.Test = Invoke-ReliableCommand "dotnet test BusBuddy.sln --verbosity minimal --nologo" "Run Tests"
    }

    # Calculate overall success
    $successfulSteps = $cycle.Values | Where-Object { $_.Success }
    $totalSteps = $cycle.Values.Count
    $overallSuccess = $successfulSteps.Count -eq $totalSteps

    $cycle.Summary = @{
        Success         = $overallSuccess
        SuccessfulSteps = $successfulSteps.Count
        TotalSteps      = $totalSteps
        TotalDuration   = ($cycle.Values | Measure-Object -Property Duration -Sum).Sum
    }

    if ($overallSuccess) {
        Write-WorkflowLog "🎉 Full cycle completed successfully!" "SUCCESS"
    }
    else {
        Write-WorkflowLog "⚠️ Full cycle completed with issues" "WARN"
    }

    return $cycle
}

function Repair-CommonIssues {
    Write-WorkflowLog "🔧 Attempting to fix common issues" "INFO"

    $fixes = @{}

    # Check current build status
    $buildResult = Invoke-ReliableCommand "dotnet build BusBuddy.sln --verbosity normal" "Pre-fix Build Check"

    if (-not $buildResult.Success) {
        Write-WorkflowLog "🔍 Analyzing build errors for auto-fix opportunities" "INFO"

        $output = $buildResult.Output -join "`n"

        # Check for common namespace issues
        if ($output -match "'(\w+)' is a namespace but is used like a type") {
            Write-WorkflowLog "📝 Detected namespace conflict: $($matches[1])" "WARN"
            $fixes.NamespaceConflict = $matches[1]
        }

        # Check for missing references
        if ($output -match "could not be found") {
            Write-WorkflowLog "📦 Detected missing reference issues" "WARN"
            $fixes.MissingReferences = $true
        }

        # Attempt package restore fix
        Write-WorkflowLog "🔄 Trying package restore fix" "INFO"
        $fixes.PackageRestore = Invoke-ReliableCommand "dotnet restore BusBuddy.sln --force" "Force Package Restore"

        # Retry build after fixes
        $fixes.RetryBuild = Invoke-ReliableCommand "dotnet build BusBuddy.sln --verbosity normal" "Retry Build After Fixes"

        if ($fixes.RetryBuild.Success) {
            Write-WorkflowLog "🎉 Auto-fix successful!" "SUCCESS"
        }
        else {
            Write-WorkflowLog "⚠️ Auto-fix didn't resolve all issues - manual intervention needed" "WARN"
        }
    }
    else {
        Write-WorkflowLog "✅ No build errors detected to fix" "SUCCESS"
        $fixes.Status = "No issues found"
    }

    return $fixes
}

function Start-CleanRebuild {
    Write-WorkflowLog "🧹 Starting clean rebuild" "INFO"

    $rebuild = @{}

    # Clean everything
    $rebuild.Clean = Invoke-ReliableCommand "dotnet clean BusBuddy.sln" "Clean Solution"

    # Clear package caches
    $rebuild.ClearCache = Invoke-ReliableCommand "dotnet nuget locals all --clear" "Clear NuGet Cache"

    # Restore fresh
    $rebuild.Restore = Invoke-ReliableCommand "dotnet restore BusBuddy.sln --force --no-cache" "Fresh Package Restore"

    # Build
    $rebuild.Build = Invoke-ReliableCommand "dotnet build BusBuddy.sln --verbosity normal" "Clean Build"

    $rebuild.Success = $rebuild.Clean.Success -and $rebuild.Restore.Success -and $rebuild.Build.Success

    if ($rebuild.Success) {
        Write-WorkflowLog "🎉 Clean rebuild completed successfully!" "SUCCESS"
    }
    else {
        Write-WorkflowLog "❌ Clean rebuild encountered issues" "ERROR"
    }

    return $rebuild
}

# ===================================================================
# MAIN WORKFLOW EXECUTION
# ===================================================================

function Main {
    Write-WorkflowLog "🎯 BusBuddy Solid Workflow Starting" "INFO"
    Write-WorkflowLog "Action: $Action | Workspace: $PWD" "INFO"

    switch ($Action) {
        "Health" {
            $Script:WorkflowResults = Test-WorkspaceHealth
        }

        "Build" {
            $Script:WorkflowResults = @{ Build = Start-QuickBuild }
        }

        "FullCycle" {
            $Script:WorkflowResults = Start-FullCycle
        }

        "Fix" {
            $Script:WorkflowResults = Repair-CommonIssues
        }

        "Clean" {
            $Script:WorkflowResults = Start-CleanRebuild
        }

        default {
            Write-WorkflowLog "❓ Unknown action: $Action" "WARN"
        }
    }

    $totalTime = (Get-Date) - $Script:StartTime
    Write-WorkflowLog "🏁 Workflow completed in $($totalTime.TotalSeconds.ToString('F2')) seconds" "INFO"
    Write-WorkflowLog "📄 Full log: $Script:LogPath" "INFO"

    return $Script:WorkflowResults
}

# Execute main function
if ($MyInvocation.InvocationName -ne '.') {
    $results = Main

    # Determine exit code based on results
    $success = $false

    if ($results.Overall) {
        $success = $results.Overall.Success
    }
    elseif ($results.Build) {
        $success = $results.Build.Success
    }
    elseif ($results.Summary) {
        $success = $results.Summary.Success
    }
    elseif ($results.RetryBuild) {
        $success = $results.RetryBuild.Success
    }
    else {
        $success = $true  # Default to success for health checks and other operations
    }

    exit $(if ($success) { 0 } else { 1 })
}
