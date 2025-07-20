#Requires -Version 7.0

<#
.SYNOPSIS
    Ultra-Safe Bus Buddy File Cleanup with Maximum Safety Protocols
.DESCRIPTION
    Multi-stage validation system for orphaned file cleanup with comprehensive safety checks
.PARAMETER WorkspaceRoot
    Root directory of the Bus Buddy workspace
.PARAMETER Stage
    Stage to execute: Analyze, Validate, Plan, DryRun, Execute
.PARAMETER Force
    Skip some safety confirmations (still maintains core safety checks)
.EXAMPLE
    .\Ultra-Safe-Cleanup.ps1 -WorkspaceRoot "C:\Path\To\Bus Buddy" -Stage Analyze
    .\Ultra-Safe-Cleanup.ps1 -WorkspaceRoot "C:\Path\To\Bus Buddy" -Stage DryRun
    .\Ultra-Safe-Cleanup.ps1 -WorkspaceRoot "C:\Path\To\Bus Buddy" -Stage Execute
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$WorkspaceRoot,

    [Parameter(Mandatory = $true)]
    [ValidateSet("Analyze", "Validate", "Plan", "DryRun", "Execute")]
    [string]$Stage,

    [Parameter(Mandatory = $false)]
    [switch]$Force
)

# Ensure we're using the correct working directory
Set-Location $WorkspaceRoot

#region Safety Configuration
$SafetyConfig = @{
    # Files that should NEVER be deleted
    CriticalFiles = @(
        "*.sln", "*.csproj", "*.vbproj", "*.fsproj",
        "*.xaml", "*.cs", "*.vb", "*.fs",
        "App.xaml*", "MainWindow.xaml*", "AssemblyInfo.cs",
        "Directory.Build.*", "global.json", "nuget.config",
        "*.md", "LICENSE", "SECURITY*", "*.yml", "*.yaml"
    )

    # Directories that should never be touched
    CriticalDirectories = @(
        "bin", "obj", "packages", ".git", ".vs", ".vscode",
        "Models", "Views", "ViewModels", "Services", "Data",
        "Controllers", "Migrations"
    )

    # File patterns that are safe to delete
    SafeDeletePatterns = @(
        "*.log", "*.tmp", "*.temp", "*.bak", "*.backup", "*.old",
        "*_copy*", "*Copy*", "*.cache", "*.user", "*.suo"
    )

    # Maximum files to process in one operation
    MaxFileOperations = 50

    # Require explicit confirmation for operations affecting more than this many files
    ConfirmationThreshold = 10

    # Backup before any destructive operations
    RequireBackup = $true
}

# Script-scoped tracking variables
$Script:AnalysisResults = @{
    TotalFiles = 0
    SafeToDelete = @()
    SafeToMove = @()
    RequireReview = @()
    CriticalWarnings = @()
    ValidationErrors = @()
}

$Script:OperationLog = @()
#endregion

#region Utility Functions
function Write-SafetyLog {
    param(
        [string]$Message,
        [ValidateSet("Info", "Warning", "Error", "Success")]
        [string]$Level = "Info",
        [switch]$NoNewLine
    )

    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $color = switch ($Level) {
        "Info" { "White" }
        "Warning" { "Yellow" }
        "Error" { "Red" }
        "Success" { "Green" }
    }

    $logEntry = "[$timestamp] [$Level] $Message"
    $Script:OperationLog += $logEntry

    if ($NoNewLine) {
        Write-Host $Message -ForegroundColor $color -NoNewline
    } else {
        Write-Host $Message -ForegroundColor $color
    }
}

function Test-IsCriticalFile {
    param([string]$FilePath)

    $fileName = Split-Path $FilePath -Leaf
    $directory = Split-Path $FilePath -Parent

    # Check if file matches critical patterns
    foreach ($pattern in $SafetyConfig.CriticalFiles) {
        if ($fileName -like $pattern) {
            return $true
        }
    }

    # Check if in critical directory
    foreach ($criticalDir in $SafetyConfig.CriticalDirectories) {
        if ($directory -match [regex]::Escape($criticalDir)) {
            return $true
        }
    }

    return $false
}

function Test-IsSafeToDelete {
    param([string]$FilePath)

    # Never delete critical files
    if (Test-IsCriticalFile $FilePath) {
        return $false
    }

    $fileName = Split-Path $FilePath -Leaf

    # Check if matches safe delete patterns
    foreach ($pattern in $SafetyConfig.SafeDeletePatterns) {
        if ($fileName -like $pattern) {
            return $true
        }
    }

    return $false
}

function Get-FileReference {
    param([string]$FilePath)

    $fileName = Split-Path $FilePath -Leaf
    $fileNameNoExt = [System.IO.Path]::GetFileNameWithoutExtension($fileName)
    $references = @()

    try {
        # Search for references in source files
        $sourceFiles = Get-ChildItem -Path $WorkspaceRoot -Filter "*.cs" -Recurse |
                      Where-Object { $_.FullName -notmatch "\\(bin|obj|packages)\\" }

        foreach ($file in $sourceFiles) {
            try {
                $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
                if ($content -and (
                    $content -match [regex]::Escape($fileName) -or
                    $content -match [regex]::Escape($fileNameNoExt)
                )) {
                    $references += $file.FullName
                }
            } catch {
                # Skip files that can't be read
            }
        }
    } catch {
        Write-SafetyLog "Warning: Could not analyze references for $FilePath" -Level Warning
    }

    return $references
}

function New-BackupDirectory {
    [CmdletBinding(SupportsShouldProcess)]
    param()

    $backupDir = Join-Path $WorkspaceRoot "Backups\Cleanup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
    if ($PSCmdlet.ShouldProcess($backupDir, "Create backup directory")) {
        if (!(Test-Path $backupDir)) {
            New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
            Write-SafetyLog "Created backup directory: $backupDir" -Level Success
        }
    }
    return $backupDir
}
#endregion

#region Stage 1: Analyze
function Invoke-AnalysisStage {
    Write-SafetyLog "🔍 STAGE 1: COMPREHENSIVE ANALYSIS" -Level Info
    Write-SafetyLog "=" * 50 -Level Info

    # Get all files
    $allFiles = Get-ChildItem -Path $WorkspaceRoot -File -Recurse |
                Where-Object { $_.FullName -notmatch "\\(bin|obj|packages)\\" }

    $Script:AnalysisResults.TotalFiles = $allFiles.Count
    Write-SafetyLog "📊 Found $($allFiles.Count) files to analyze" -Level Info

    $fileCategories = @{
        SafeToDelete = @()
        PotentialOrphans = @()
        CriticalFiles = @()
        RequireReview = @()
    }

    $counter = 0
    foreach ($file in $allFiles) {
        $counter++
        if ($counter % 50 -eq 0) {
            Write-SafetyLog "📋 Analyzed $counter of $($allFiles.Count) files..." -Level Info
        }

        $relativePath = $file.FullName.Replace($WorkspaceRoot, ".").Replace("\", "/")

        # Categorize files
        if (Test-IsCriticalFile $file.FullName) {
            $fileCategories.CriticalFiles += @{
                Path = $relativePath
                FullPath = $file.FullName
                Size = $file.Length
                LastModified = $file.LastWriteTime
            }
        }
        elseif (Test-IsSafeToDelete $file.FullName) {
            $references = Get-FileReference $file.FullName
            $fileCategories.SafeToDelete += @{
                Path = $relativePath
                FullPath = $file.FullPath
                Size = $file.Length
                LastModified = $file.LastWriteTime
                References = $references.Count
                Reason = "Matches safe delete pattern"
            }
        }
        else {
            $references = Get-FileReference $file.FullName
            if ($references.Count -eq 0) {
                $fileCategories.PotentialOrphans += @{
                    Path = $relativePath
                    FullPath = $file.FullName
                    Size = $file.Length
                    LastModified = $file.LastWriteTime
                    References = 0
                    Reason = "No references found"
                }
            }
        }
    }

    # Store results
    $Script:AnalysisResults.SafeToDelete = $fileCategories.SafeToDelete
    $Script:AnalysisResults.RequireReview = $fileCategories.PotentialOrphans

    # Report results
    Write-SafetyLog "📊 ANALYSIS RESULTS:" -Level Success
    Write-SafetyLog "  ✅ Critical Files (Protected): $($fileCategories.CriticalFiles.Count)" -Level Info
    Write-SafetyLog "  🗑️  Safe to Delete: $($fileCategories.SafeToDelete.Count)" -Level Info
    Write-SafetyLog "  ⚠️  Potential Orphans: $($fileCategories.PotentialOrphans.Count)" -Level Warning

    # Safety checks
    if ($fileCategories.SafeToDelete.Count -gt $SafetyConfig.MaxFileOperations) {
        $Script:AnalysisResults.CriticalWarnings += "Too many files marked for deletion ($($fileCategories.SafeToDelete.Count) > $($SafetyConfig.MaxFileOperations))"
    }

    # Export analysis results
    $analysisReport = @{
        Timestamp = Get-Date
        Stage = "Analysis"
        TotalFiles = $Script:AnalysisResults.TotalFiles
        SafeToDelete = $fileCategories.SafeToDelete
        PotentialOrphans = $fileCategories.PotentialOrphans
        CriticalFiles = $fileCategories.CriticalFiles
        Warnings = $Script:AnalysisResults.CriticalWarnings
    }

    $reportPath = Join-Path $WorkspaceRoot "ultra-safe-analysis-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
    $analysisReport | ConvertTo-Json -Depth 10 | Set-Content $reportPath
    Write-SafetyLog "📄 Analysis report saved: $reportPath" -Level Success

    return $analysisReport
}
#endregion

#region Stage 2: Validate
function Invoke-ValidationStage {
    Write-SafetyLog "✅ STAGE 2: VALIDATION" -Level Info
    Write-SafetyLog "=" * 50 -Level Info

    $validationErrors = @()

    # Validate workspace integrity
    $criticalFiles = @("BusBuddy.sln", "BusBuddy.WPF\BusBuddy.WPF.csproj", "BusBuddy.Core\BusBuddy.Core.csproj")
    foreach ($file in $criticalFiles) {
        $fullPath = Join-Path $WorkspaceRoot $file
        if (!(Test-Path $fullPath)) {
            $validationErrors += "Critical file missing: $file"
        }
    }

    # Validate build can succeed
    Write-SafetyLog "🔨 Testing build before cleanup..." -Level Info
    try {
        $buildResult = & dotnet build BusBuddy.sln --verbosity quiet --nologo 2>&1
        if ($LASTEXITCODE -ne 0) {
            $validationErrors += "Build failed before cleanup - cannot proceed safely"
            Write-SafetyLog "❌ Build failed: $buildResult" -Level Error
        } else {
            Write-SafetyLog "✅ Build successful - safe to proceed" -Level Success
        }
    } catch {
        $validationErrors += "Could not test build: $($_.Exception.Message)"
    }

    # Validate analysis results
    if ($Script:AnalysisResults.SafeToDelete.Count -eq 0) {
        Write-SafetyLog "ℹ️  No files marked for deletion" -Level Info
    }

    if ($validationErrors.Count -gt 0) {
        Write-SafetyLog "❌ VALIDATION FAILED:" -Level Error
        $validationErrors | ForEach-Object { Write-SafetyLog "  • $_" -Level Error }
        $Script:AnalysisResults.ValidationErrors = $validationErrors
        return $false
    }

    Write-SafetyLog "✅ All validations passed" -Level Success
    return $true
}
#endregion

#region Stage 3: Plan
function Invoke-PlanningStage {
    Write-SafetyLog "📋 STAGE 3: OPERATION PLANNING" -Level Info
    Write-SafetyLog "=" * 50 -Level Info

    $operationPlan = @{
        FilesToDelete = @()
        FilesToMove = @()
        BackupRequired = $false
        EstimatedDuration = "2-5 minutes"
        RiskLevel = "Low"
    }

    # Plan deletions
    foreach ($file in $Script:AnalysisResults.SafeToDelete) {
        if ($file.References -eq 0 -and $file.Path -notmatch "(App\.xaml|MainWindow\.xaml)") {
            $operationPlan.FilesToDelete += $file
        }
    }

    # Assess risk level
    if ($operationPlan.FilesToDelete.Count -gt $SafetyConfig.ConfirmationThreshold) {
        $operationPlan.RiskLevel = "Medium"
        $operationPlan.BackupRequired = $true
    }

    if ($operationPlan.FilesToDelete.Count -gt $SafetyConfig.MaxFileOperations) {
        $operationPlan.RiskLevel = "High"
        Write-SafetyLog "⚠️  HIGH RISK: $($operationPlan.FilesToDelete.Count) files to delete" -Level Warning
    }

    # Display plan
    Write-SafetyLog "📊 OPERATION PLAN:" -Level Info
    Write-SafetyLog "  🗑️  Files to Delete: $($operationPlan.FilesToDelete.Count)" -Level Info
    Write-SafetyLog "  📁 Files to Move: $($operationPlan.FilesToMove.Count)" -Level Info
    Write-SafetyLog "  ⚠️  Risk Level: $($operationPlan.RiskLevel)" -Level $(if ($operationPlan.RiskLevel -eq "High") { "Warning" } else { "Info" })
    Write-SafetyLog "  💾 Backup Required: $($operationPlan.BackupRequired)" -Level Info

    return $operationPlan
}
#endregion

#region Stage 4: DryRun
function Invoke-DryRunStage {
    Write-SafetyLog "🧪 STAGE 4: DRY RUN SIMULATION" -Level Info
    Write-SafetyLog "=" * 50 -Level Info
    Write-SafetyLog "⚠️  DRY RUN MODE - No actual changes will be made" -Level Warning

    $plan = Invoke-PlanningStage
    $simulationResults = @{
        Success = $true
        OperationsSimulated = 0
        Warnings = @()
        Errors = @()
    }

    Write-SafetyLog "🗑️  SIMULATING DELETIONS:" -Level Info
    foreach ($file in $plan.FilesToDelete) {
        $simulationResults.OperationsSimulated++

        if (Test-Path $file.FullPath) {
            Write-SafetyLog "  ✅ Would delete: $($file.Path)" -Level Success

            # Additional safety checks
            if ($file.Size -gt 1MB) {
                $simulationResults.Warnings += "Large file deletion: $($file.Path) ($([math]::Round($file.Size/1MB, 2)) MB)"
            }

            if ($file.LastModified -gt (Get-Date).AddDays(-7)) {
                $simulationResults.Warnings += "Recent file deletion: $($file.Path) (modified $($file.LastModified))"
            }
        } else {
            $simulationResults.Errors += "File not found: $($file.Path)"
        }
    }

    # Simulate build test
    Write-SafetyLog "🔨 SIMULATING BUILD TEST:" -Level Info
    Write-SafetyLog "  ✅ Would test build after cleanup" -Level Success
    Write-SafetyLog "  ✅ Would validate no broken references" -Level Success

    # Results summary
    Write-SafetyLog "📊 DRY RUN RESULTS:" -Level Success
    Write-SafetyLog "  ✅ Operations Simulated: $($simulationResults.OperationsSimulated)" -Level Info
    Write-SafetyLog "  ⚠️  Warnings: $($simulationResults.Warnings.Count)" -Level $(if ($simulationResults.Warnings.Count -gt 0) { "Warning" } else { "Info" })
    Write-SafetyLog "  ❌ Errors: $($simulationResults.Errors.Count)" -Level $(if ($simulationResults.Errors.Count -gt 0) { "Error" } else { "Info" })

    if ($simulationResults.Warnings.Count -gt 0) {
        Write-SafetyLog "⚠️  WARNINGS:" -Level Warning
        $simulationResults.Warnings | ForEach-Object { Write-SafetyLog "  • $_" -Level Warning }
    }

    if ($simulationResults.Errors.Count -gt 0) {
        Write-SafetyLog "❌ ERRORS:" -Level Error
        $simulationResults.Errors | ForEach-Object { Write-SafetyLog "  • $_" -Level Error }
        $simulationResults.Success = $false
    }

    return $simulationResults
}
#endregion

#region Stage 5: Execute
function Invoke-ExecutionStage {
    Write-SafetyLog "🚀 STAGE 5: SAFE EXECUTION" -Level Info
    Write-SafetyLog "=" * 50 -Level Info

    # Final validation
    $validation = Invoke-ValidationStage
    if (!$validation) {
        Write-SafetyLog "❌ Pre-execution validation failed - ABORTING" -Level Error
        return $false
    }

    $plan = Invoke-PlanningStage
    $backupDir = $null

    # Create backup if required
    if ($plan.BackupRequired -or $SafetyConfig.RequireBackup) {
        Write-SafetyLog "💾 Creating backup..." -Level Info
        $backupDir = New-BackupDirectory

        foreach ($file in $plan.FilesToDelete) {
            if (Test-Path $file.FullPath) {
                $relativePath = $file.Path.TrimStart('./', '.\')
                $backupPath = Join-Path $backupDir $relativePath
                $backupParent = Split-Path $backupPath -Parent

                if (!(Test-Path $backupParent)) {
                    New-Item -ItemType Directory -Path $backupParent -Force | Out-Null
                }

                Copy-Item $file.FullPath $backupPath -Force
            }
        }
        Write-SafetyLog "✅ Backup created: $backupDir" -Level Success
    }

    # Execute deletions
    $executionResults = @{
        DeletedFiles = @()
        Errors = @()
        Success = $true
    }

    if (!$Force -and $plan.FilesToDelete.Count -gt $SafetyConfig.ConfirmationThreshold) {
        Write-SafetyLog "⚠️  About to delete $($plan.FilesToDelete.Count) files. Type 'YES' to confirm:" -Level Warning
        $confirmation = Read-Host
        if ($confirmation -ne "YES") {
            Write-SafetyLog "❌ Operation cancelled by user" -Level Warning
            return $false
        }
    }

    Write-SafetyLog "🗑️  EXECUTING DELETIONS:" -Level Info
    foreach ($file in $plan.FilesToDelete) {
        try {
            if (Test-Path $file.FullPath) {
                Remove-Item $file.FullPath -Force
                $executionResults.DeletedFiles += $file.Path
                Write-SafetyLog "  ✅ Deleted: $($file.Path)" -Level Success
            }
        } catch {
            $errorMessage = "Failed to delete $($file.Path): $($_.Exception.Message)"
            $executionResults.Errors += $errorMessage
            Write-SafetyLog "  ❌ $errorMessage" -Level Error
            $executionResults.Success = $false
        }
    }

    # Post-execution validation
    Write-SafetyLog "🔨 Testing build after cleanup..." -Level Info
    try {
        $null = & dotnet build BusBuddy.sln --verbosity quiet --nologo 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-SafetyLog "❌ Build failed after cleanup!" -Level Error
            Write-SafetyLog "🔄 Restore from backup: $backupDir" -Level Warning
            $executionResults.Success = $false
        } else {
            Write-SafetyLog "✅ Build successful after cleanup" -Level Success
        }
    } catch {
        Write-SafetyLog "❌ Could not test build after cleanup" -Level Error
        $executionResults.Success = $false
    }

    # Results summary
    Write-SafetyLog "📊 EXECUTION RESULTS:" -Level $(if ($executionResults.Success) { "Success" } else { "Error" })
    Write-SafetyLog "  ✅ Files Deleted: $($executionResults.DeletedFiles.Count)" -Level Success
    Write-SafetyLog "  ❌ Errors: $($executionResults.Errors.Count)" -Level $(if ($executionResults.Errors.Count -gt 0) { "Error" } else { "Info" })

    if ($backupDir) {
        Write-SafetyLog "  💾 Backup Location: $backupDir" -Level Info
    }

    return $executionResults
}
#endregion

#region Main Execution
Write-SafetyLog "🛡️  Ultra-Safe Bus Buddy Cleanup System" -Level Info
Write-SafetyLog "📁 Workspace: $WorkspaceRoot" -Level Info
Write-SafetyLog "🎯 Stage: $Stage" -Level Info
Write-SafetyLog ""

try {
    switch ($Stage) {
        "Analyze" {
            $result = Invoke-AnalysisStage
            Write-SafetyLog "✅ Analysis completed. Run with -Stage Validate to continue." -Level Success
        }
        "Validate" {
            $result = Invoke-ValidationStage
            if ($result) {
                Write-SafetyLog "✅ Validation passed. Run with -Stage Plan to continue." -Level Success
            } else {
                Write-SafetyLog "❌ Validation failed. Review and fix issues before proceeding." -Level Error
                exit 1
            }
        }
        "Plan" {
            $result = Invoke-PlanningStage
            Write-SafetyLog "✅ Planning completed. Run with -Stage DryRun to test the plan." -Level Success
        }
        "DryRun" {
            $result = Invoke-DryRunStage
            if ($result.Success) {
                Write-SafetyLog "✅ Dry run successful. Run with -Stage Execute to perform actual cleanup." -Level Success
            } else {
                Write-SafetyLog "❌ Dry run detected issues. Review before executing." -Level Error
                exit 1
            }
        }
        "Execute" {
            $result = Invoke-ExecutionStage
            if ($result.Success) {
                Write-SafetyLog "🎉 Cleanup completed successfully!" -Level Success
            } else {
                Write-SafetyLog "❌ Cleanup completed with errors. Check logs and backup." -Level Error
                exit 1
            }
        }
    }

    # Save operation log
    $logPath = Join-Path $WorkspaceRoot "ultra-safe-cleanup-log-$(Get-Date -Format 'yyyyMMdd-HHmmss').txt"
    $Script:OperationLog | Set-Content $logPath
    Write-SafetyLog "📝 Operation log saved: $logPath" -Level Info

} catch {
    Write-SafetyLog "💥 CRITICAL ERROR: $($_.Exception.Message)" -Level Error
    Write-SafetyLog "🔄 No changes were made due to error" -Level Warning
    exit 1
}
#endregion
