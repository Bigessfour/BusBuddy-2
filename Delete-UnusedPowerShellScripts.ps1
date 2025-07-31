#Requires -Version 7.5
<#
.SYNOPSIS
    Cleans up unused PowerShell scripts in the BusBuddy repository.

.DESCRIPTION
    This script deletes unused PowerShell scripts from the BusBuddy repository without creating backups.
    These scripts have been identified as obsolete, duplicate, or one-time fix scripts that are no longer needed.

.NOTES
    File Name: Delete-UnusedPowerShellScripts.ps1
    Author: GitHub Copilot
    Date: July 31, 2025
#>

$ErrorActionPreference = "Continue"

# Define files to delete
$filesToDelete = @(
    # Duplicate/Obsolete Maintenance Scripts
    "PowerShell\Scripts\Maintenance\MSB3027-File-Lock-Solution.ps1",
    # Redundant Scripts
    "PowerShell\Load-BusBuddy-ExceptionCapture.ps1",
    "PowerShell\BusBuddy PowerShell Environment\Enhanced-Build-With-Problem-Capture.ps1",
    # Legacy Test Scripts
    "PowerShell\BusBuddy PowerShell Environment\Scripts\simple-form-monitoring.ps1",
    "PowerShell\BusBuddy PowerShell Environment\Scripts\enhanced-form-monitoring.ps1",
    "PowerShell\BusBuddy PowerShell Environment\Scripts\coordinated-monitoring.ps1",
    # Duplicate Utilities
    "PowerShell\Scripts\bb-help.ps1",
    "PowerShell\Scripts\fix-file-encodings.ps1",
    "PowerShell\Scripts\Fix-PowerShell-Paging.ps1",
    "PowerShell\Scripts\fix-tavily-scripts.ps1",
    # Fixed Issues (No Longer Needed)
    "PowerShell\Scripts\Fix-GitHub-Workflows.ps1",
    "PowerShell\Scripts\Fix-GitHub-Workflows-UTF8.ps1",
    "PowerShell\Scripts\Phase2-Code-Quality-Fix.ps1"
)

# Function to delete files
function Remove-UnusedFile {
    param(
        [string]$FilePath
    )

    $fullPath = Join-Path -Path $PSScriptRoot -ChildPath $FilePath

    if (Test-Path -Path $fullPath) {
        try {
            Remove-Item -Path $fullPath -Force
            Write-Host "✅ Deleted: $FilePath" -ForegroundColor Green
            return $true
        }
        catch {
            Write-Host "❌ Failed to delete: $FilePath - $($_.Exception.Message)" -ForegroundColor Red
            return $false
        }
    }
    else {
        Write-Host "⚠️ File not found: $FilePath" -ForegroundColor Yellow
        return $false
    }
}

# Execute deletion
Write-Host "🚮 Deleting unused PowerShell scripts..." -ForegroundColor Cyan
Write-Host "===================================" -ForegroundColor DarkGray

$successCount = 0
$failCount = 0

foreach ($file in $filesToDelete) {
    if (Remove-UnusedFile -FilePath $file) {
        $successCount++
    }
    else {
        $failCount++
    }
}

# Summary
Write-Host ""
Write-Host "🚮 Cleanup Complete" -ForegroundColor Cyan
Write-Host "===================================" -ForegroundColor DarkGray
Write-Host "✅ Successfully deleted: $successCount files" -ForegroundColor Green
if ($failCount -gt 0) {
    Write-Host "❌ Failed to delete: $failCount files" -ForegroundColor Red
}
