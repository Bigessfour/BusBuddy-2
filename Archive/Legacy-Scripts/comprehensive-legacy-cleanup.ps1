#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Comprehensive Legacy Cleanup for BusBuddy Repository

.DESCRIPTION
    This script removes legacy files, outdated scripts, and unnecessary artifacts
    that are bloating the repository. Current repo size: 240MB approaching 300MB limit.

.NOTES
    Author: GitHub Copilot
    Date: 2025-07-25
    Version: 1.0

    SAFETY: This script will show what it will delete before actually deleting
#>

param(
    [switch]$DryRun = $true,
    [switch]$Force = $false
)

Write-Host "🧹 BusBuddy Comprehensive Legacy Cleanup" -ForegroundColor Cyan
Write-Host "📅 $(Get-Date)" -ForegroundColor Gray
Write-Host ""

if ($DryRun) {
    Write-Host "🔍 DRY RUN MODE - No files will be deleted" -ForegroundColor Yellow
    Write-Host "Use -DryRun:`$false to actually delete files" -ForegroundColor Yellow
    Write-Host ""
}

# Legacy PowerShell Scripts to Remove
$legacyPowerShellFiles = @(
    "benchmark-performance.ps1",
    "benchmark-powershell-76.ps1",
    "benchmark-results-20250723-205555.json",
    "BusBuddy-PowerShell-Profile-7.5.2.ps1",
    ".powershell-profile-75-standards.ps1",
    "demo-tokenizer.ps1",
    "Intel-Hybrid-Performance-Optimizer.ps1",
    "PowerShell-Compatibility-Wrapper.psm1",
    "PowerShell-Optimization-Profile.ps1",
    "PowerShell-Session-Persistence.ps1",
    "project-recovery.ps1",
    "run-dell-optimization.ps1",
    "start-admin-optimization.ps1",
    "test-ai-assistant-76.ps1",
    "test-dotnet9-compatibility.ps1",
    "test-load-admin-functions.ps1",
    "test-powershell76-optimizations.ps1"
)

# Legacy Batch Files to Remove
$legacyBatchFiles = @(
    "check-admin.bat",
    "run-admin-optimization.bat"
)

# Legacy Documentation to Remove
$legacyDocuments = @(
    "greenfield_reset.md",
    "PHASE-1-COMPLETION-REPORT.md",
    "PHASE-1-FOUNDATION-REVIEW.md",
    "PHASE-2-PLAN.md",
    "POWERSHELL-PROFILE-GUIDE.md",
    "PowerShell-ExecutionPolicy-Fix-Guide.md",
    "PRIVATE-REPO-CLEANUP-NOTES.md",
    "STEVE-HAPPINESS-MATRIX-V2-REPORT.md",
    "OPTIMIZATION-EDUCATION.md"
)

# Legacy Config Files to Remove
$legacyConfigFiles = @(
    "analyzer-fixes-20250725-102234.log",
    "all-workspace-files.txt",
    "currently-tracked-files.txt",
    "codecov.yml",
    "testsettings.runsettings.xml",
    ".env.template"
)

# Legacy Directories to Remove
$legacyDirectories = @(
    ".ai-assistant",
    "Documentation",
    "Properties",
    "Services",
    "TestResults"
)

# Files/Directories that should be in .gitignore (build artifacts)
$buildArtifacts = @(
    "BusBuddy.Core/bin",
    "BusBuddy.Core/obj",
    "BusBuddy.WPF/bin",
    "BusBuddy.WPF/obj",
    "logs/*.log"
)

function Remove-LegacyItems {
    param(
        [string[]]$Items,
        [string]$ItemType,
        [switch]$IsDirectory
    )

    Write-Host "🔍 Checking $ItemType..." -ForegroundColor Cyan

    foreach ($item in $Items) {
        if (Test-Path $item) {
            if ($IsDirectory) {
                $size = (Get-ChildItem -Recurse $item | Measure-Object -Property Length -Sum).Sum
                $sizeMB = [math]::Round($size / 1MB, 2)
                Write-Host "  📁 $item (Directory - $sizeMB MB)" -ForegroundColor Yellow
            } else {
                $size = (Get-Item $item).Length
                $sizeKB = [math]::Round($size / 1KB, 2)
                Write-Host "  📄 $item ($sizeKB KB)" -ForegroundColor Yellow
            }

            if (-not $DryRun) {
                try {
                    if ($IsDirectory) {
                        Remove-Item -Recurse -Force $item
                        Write-Host "    ✅ Deleted directory: $item" -ForegroundColor Green
                    } else {
                        Remove-Item -Force $item
                        Write-Host "    ✅ Deleted file: $item" -ForegroundColor Green
                    }
                } catch {
                    Write-Host "    ❌ Failed to delete: $item - $($_.Exception.Message)" -ForegroundColor Red
                }
            }
        }
    }
}

function Show-RepositoryStats {
    Write-Host ""
    Write-Host "📊 Repository Statistics" -ForegroundColor Cyan

    # Count files by type
    $psFiles = (Get-ChildItem -Recurse -Filter "*.ps1" | Measure-Object).Count
    $csFiles = (Get-ChildItem -Recurse -Filter "*.cs" | Measure-Object).Count
    $xamlFiles = (Get-ChildItem -Recurse -Filter "*.xaml" | Measure-Object).Count
    $jsonFiles = (Get-ChildItem -Recurse -Filter "*.json" | Measure-Object).Count

    Write-Host "  📜 PowerShell files: $psFiles" -ForegroundColor Gray
    Write-Host "  📝 C# files: $csFiles" -ForegroundColor Gray
    Write-Host "  🎨 XAML files: $xamlFiles" -ForegroundColor Gray
    Write-Host "  📋 JSON files: $jsonFiles" -ForegroundColor Gray

    # Repository size
    $totalSize = (Get-ChildItem -Recurse | Measure-Object -Property Length -Sum).Sum
    $sizeMB = [math]::Round($totalSize / 1MB, 2)
    Write-Host "  💾 Total size: $sizeMB MB" -ForegroundColor Gray
}

# Show initial stats
Write-Host "📊 BEFORE CLEANUP:" -ForegroundColor Magenta
Show-RepositoryStats

Write-Host ""
Write-Host "🧹 LEGACY CLEANUP ANALYSIS:" -ForegroundColor Magenta

# Remove legacy PowerShell files
Remove-LegacyItems -Items $legacyPowerShellFiles -ItemType "Legacy PowerShell Scripts"

# Remove legacy batch files
Remove-LegacyItems -Items $legacyBatchFiles -ItemType "Legacy Batch Files"

# Remove legacy documentation
Remove-LegacyItems -Items $legacyDocuments -ItemType "Legacy Documentation"

# Remove legacy config files
Remove-LegacyItems -Items $legacyConfigFiles -ItemType "Legacy Configuration Files"

# Remove legacy directories
Remove-LegacyItems -Items $legacyDirectories -ItemType "Legacy Directories" -IsDirectory

Write-Host ""
Write-Host "🔍 BUILD ARTIFACTS ANALYSIS:" -ForegroundColor Magenta

# Check for build artifacts that should be in .gitignore
foreach ($artifact in $buildArtifacts) {
    if (Test-Path $artifact) {
        if ($artifact -like "*/bin" -or $artifact -like "*/obj") {
            $size = (Get-ChildItem -Recurse $artifact | Measure-Object -Property Length -Sum).Sum
            $sizeMB = [math]::Round($size / 1MB, 2)
            Write-Host "  ⚠️ Build artifact: $artifact ($sizeMB MB) - Should be in .gitignore" -ForegroundColor Yellow

            if (-not $DryRun) {
                Remove-Item -Recurse -Force $artifact
                Write-Host "    ✅ Deleted build artifact: $artifact" -ForegroundColor Green
            }
        } elseif ($artifact -like "logs/*.log") {
            $logFiles = Get-ChildItem -Path "logs" -Filter "*.log"
            foreach ($logFile in $logFiles) {
                $sizeKB = [math]::Round($logFile.Length / 1KB, 2)
                Write-Host "  📝 Log file: $($logFile.Name) ($sizeKB KB)" -ForegroundColor Yellow

                if (-not $DryRun) {
                    Remove-Item -Force $logFile.FullName
                    Write-Host "    ✅ Deleted log file: $($logFile.Name)" -ForegroundColor Green
                }
            }
        }
    }
}

# Show final stats if not dry run
if (-not $DryRun) {
    Write-Host ""
    Write-Host "📊 AFTER CLEANUP:" -ForegroundColor Magenta
    Show-RepositoryStats
}

Write-Host ""
Write-Host "🎯 RECOMMENDATIONS:" -ForegroundColor Cyan
Write-Host "  1. Run 'git add -A && git commit -m `"Clean: Remove legacy files and build artifacts`"'" -ForegroundColor Gray
Write-Host "  2. Update .gitignore to ensure bin/, obj/, *.log are excluded" -ForegroundColor Gray
Write-Host "  3. Consider moving Tools/ directory to a separate branch if still needed" -ForegroundColor Gray
Write-Host "  4. Archive old documentation in a separate docs branch" -ForegroundColor Gray

if ($DryRun) {
    Write-Host ""
    Write-Host "🚀 To execute cleanup: .\comprehensive-legacy-cleanup.ps1 -DryRun:`$false" -ForegroundColor Green
}

Write-Host ""
Write-Host "✅ Legacy cleanup analysis complete!" -ForegroundColor Green
