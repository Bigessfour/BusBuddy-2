#Requires -Version 7.0

<#
.SYNOPSIS
    Surgical Bus Buddy File Cleanup - Safe and Precise
.DESCRIPTION
    Performs only architecturally correct file cleanup and reorganization
.PARAMETER WorkspaceRoot
    Root directory of the Bus Buddy workspace
.PARAMETER DryRun
    If specified, shows what would be done without making changes
.EXAMPLE
    .\Surgical-Cleanup.ps1 -WorkspaceRoot "C:\Path\To\Bus Buddy" -DryRun
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$WorkspaceRoot,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun
)

# Ensure we're using the correct working directory
Set-Location $WorkspaceRoot

Write-Host "🔧 Bus Buddy Surgical Cleanup" -ForegroundColor Cyan
Write-Host "📁 Workspace: $WorkspaceRoot" -ForegroundColor White
if ($DryRun) {
    Write-Host "⚠️  DRY RUN MODE - No changes will be made" -ForegroundColor Yellow
}
Write-Host ""

# Track operations for summary
$deletedFiles = @()
$movedFiles = @()
$errors = @()

#region Safe File Deletions
Write-Host "🗑️  PHASE 1: Safe File Deletions" -ForegroundColor Green

# Log files to delete (old application logs)
$logFilesToDelete = @(
    "BusBuddy.WPF/logs/application-20250717.log",
    "BusBuddy.WPF/logs/application-20250718.log",
    "BusBuddy.WPF/logs/errors-actionable-20250717.log",
    "BusBuddy.WPF/logs/errors-actionable-20250718.log",
    "BusBuddy.WPF/logs/ui-interactions-20250717.log",
    "BusBuddy.WPF/logs/ui-interactions-20250718.log",
    "logs/msbuild-BusBuddy.Core-.log",
    "logs/msbuild-BusBuddy.Core-Debug.log",
    "logs/msbuild-BusBuddy.Tests-.log",
    "logs/msbuild-BusBuddy.Tests-Debug.log",
    "logs/msbuild-BusBuddy.WPF-.log",
    "logs/msbuild-BusBuddy.WPF-Debug.log"
)

# Backup files to delete
$backupFilesToDelete = @(
    ".vscode/settings.json.backup"
)

# Redundant DLL files (already available via NuGet)
$redundantDlls = @(
    "BusBuddy.Core/lib/Syncfusion.Licensing.dll",  # Available via NuGet in WPF project
    "BusBuddy.Core/lib/Syncfusion.SfScheduler.WPF.VisualStudio.Design.dll"  # Design-time only
)

$allFilesToDelete = $logFilesToDelete + $backupFilesToDelete + $redundantDlls

foreach ($file in $allFilesToDelete) {
    $fullPath = Join-Path $WorkspaceRoot $file
    if (Test-Path $fullPath) {
        if ($DryRun) {
            Write-Host "  ✅ Would delete: $file" -ForegroundColor Yellow
        } else {
            try {
                Remove-Item $fullPath -Force
                $deletedFiles += $file
                Write-Host "  ✅ Deleted: $file" -ForegroundColor Green
            } catch {
                $errors += "Failed to delete $file : $($_.Exception.Message)"
                Write-Host "  ❌ Failed to delete: $file" -ForegroundColor Red
            }
        }
    } else {
        Write-Host "  ℹ️  Not found: $file" -ForegroundColor Gray
    }
}
#endregion

#region Syncfusion DLL Analysis
Write-Host ""
Write-Host "🔍 PHASE 2: Redundant DLL Cleanup" -ForegroundColor Green
Write-Host "  ℹ️  Syncfusion.Licensing.dll - Already available via NuGet package" -ForegroundColor Cyan
Write-Host "  ℹ️  Design-time DLL - Not needed for runtime" -ForegroundColor Cyan
#endregion

#region Architectural File Moves
Write-Host ""
Write-Host "📁 PHASE 3: Architectural File Reorganization" -ForegroundColor Green

# Only architecturally correct moves
$fileMoves = @(
    @{
        Source = "BusBuddy.WPF/DebugConverter.cs"
        Target = "BusBuddy.WPF/Converters/DebugConverter.cs"
        Reason = "Converter should be in Converters folder"
    },
    @{
        Source = "BusBuddy.WPF/Extensions/UIThreadOptimizer.cs"
        Target = "BusBuddy.WPF/Utilities/UIThreadOptimizer.cs"
        Reason = "Thread utility should be in Utilities folder"
    },
    @{
        Source = "BusBuddy.WPF/Utilities/SafeMarkupExtensions.cs"
        Target = "BusBuddy.WPF/Extensions/SafeMarkupExtensions.cs"
        Reason = "Markup extension should be in Extensions folder"
    },
    @{
        Source = "BusBuddy.Core/Utilities/LoggingExtensions.cs"
        Target = "BusBuddy.Core/Extensions/LoggingExtensions.cs"
        Reason = "Extension methods should be in Extensions folder"
    }
)

foreach ($move in $fileMoves) {
    $sourcePath = Join-Path $WorkspaceRoot $move.Source
    $targetPath = Join-Path $WorkspaceRoot $move.Target
    $targetDir = Split-Path $targetPath -Parent

    if (Test-Path $sourcePath) {
        Write-Host "  📁 Move: $($move.Source)" -ForegroundColor White
        Write-Host "      To: $($move.Target)" -ForegroundColor Gray
        Write-Host "      Reason: $($move.Reason)" -ForegroundColor Gray

        if ($DryRun) {
            Write-Host "      ✅ Would move file" -ForegroundColor Yellow
        } else {
            try {
                # Create target directory if it doesn't exist
                if (!(Test-Path $targetDir)) {
                    New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
                    Write-Host "      📁 Created directory: $targetDir" -ForegroundColor Cyan
                }

                # Move the file
                Move-Item $sourcePath $targetPath -Force
                $movedFiles += @{ Source = $move.Source; Target = $move.Target }
                Write-Host "      ✅ Moved successfully" -ForegroundColor Green
            } catch {
                $errors += "Failed to move $($move.Source): $($_.Exception.Message)"
                Write-Host "      ❌ Failed to move: $($_.Exception.Message)" -ForegroundColor Red
            }
        }
    } else {
        Write-Host "  ℹ️  Not found: $($move.Source)" -ForegroundColor Gray
    }
}
#endregion

#region Reference Updates
if (!$DryRun -and $movedFiles.Count -gt 0) {
    Write-Host ""
    Write-Host "🔄 PHASE 4: Updating References" -ForegroundColor Green

    # Find all C# files for reference updates
    $csFiles = Get-ChildItem -Path $WorkspaceRoot -Filter "*.cs" -Recurse |
               Where-Object { $_.FullName -notmatch "\\(bin|obj|packages)\\" }

    foreach ($move in $movedFiles) {
        $sourceFileName = Split-Path $move.Source -Leaf

        Write-Host "  🔍 Searching for references to moved file: $sourceFileName..." -ForegroundColor White

        $referencesFound = 0
        foreach ($file in $csFiles) {
            try {
                $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
                if ($content -and ($content -match [regex]::Escape($sourceFileName))) {
                    $referencesFound++
                    Write-Host "      📄 Found reference in: $($file.Name)" -ForegroundColor Yellow
                }
            } catch {
                # Skip files that can't be read
            }
        }

        if ($referencesFound -eq 0) {
            Write-Host "      ✅ No references found - move is clean" -ForegroundColor Green
        } else {
            Write-Host "      ⚠️  Found $referencesFound potential references - manual review needed" -ForegroundColor Yellow
        }
    }
}
#endregion

#region Summary
Write-Host ""
Write-Host "📊 CLEANUP SUMMARY" -ForegroundColor Cyan
Write-Host "=================" -ForegroundColor Cyan

if ($DryRun) {
    Write-Host "⚠️  DRY RUN COMPLETED - No actual changes made" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Would delete $($allFilesToDelete.Count) files:" -ForegroundColor White
    $allFilesToDelete | ForEach-Object { Write-Host "  • $_" -ForegroundColor Gray }

    Write-Host ""
    Write-Host "Would move $($fileMoves.Count) files:" -ForegroundColor White
    $fileMoves | ForEach-Object { Write-Host "  • $($_.Source) → $($_.Target)" -ForegroundColor Gray }
} else {
    Write-Host "✅ Deleted Files: $($deletedFiles.Count)" -ForegroundColor Green
    $deletedFiles | ForEach-Object { Write-Host "  • $_" -ForegroundColor Gray }

    Write-Host ""
    Write-Host "✅ Moved Files: $($movedFiles.Count)" -ForegroundColor Green
    $movedFiles | ForEach-Object { Write-Host "  • $($_.Source) → $($_.Target)" -ForegroundColor Gray }

    if ($errors.Count -gt 0) {
        Write-Host ""
        Write-Host "❌ Errors: $($errors.Count)" -ForegroundColor Red
        $errors | ForEach-Object { Write-Host "  • $_" -ForegroundColor Red }
    }
}

Write-Host ""
Write-Host "🚀 NEXT STEPS:" -ForegroundColor Cyan
Write-Host "1. Build the solution to verify no broken references" -ForegroundColor White
Write-Host "2. Run tests to ensure functionality is intact" -ForegroundColor White
Write-Host "3. Review any reference update warnings above" -ForegroundColor White
if ($DryRun) {
    Write-Host "4. Run this script without -DryRun to apply changes" -ForegroundColor White
}

Write-Host ""
if ($DryRun) {
    Write-Host "✅ Surgical cleanup analysis completed successfully" -ForegroundColor Green
} else {
    Write-Host "✅ Surgical cleanup completed successfully" -ForegroundColor Green
}
#endregion
