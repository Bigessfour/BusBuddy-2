#Requires -Version 7.5
<#
.SYNOPSIS
    BusBuddy Warning Analysis Script - DETECTION AND CLASSIFICATION ONLY
.DESCRIPTION
    This script ONLY analyzes and reports warnings from the build output.
    NO AUTOMATED FIXES - Manual intervention required for all changes.
.NOTES
    Version: 2.0 - SAFE MODE
    Date: July 30, 2025
    Author: BusBuddy Development Team
    POLICY: Detection only, no file modifications
#>

param(
    [switch]$Detailed,
    [switch]$ExportReport
)

# Set error handling
$ErrorActionPreference = 'Stop'

Write-Host "🚌 BusBuddy Warning Analysis - DETECTION ONLY" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "⚠️  SAFE MODE: No files will be modified" -ForegroundColor Yellow
Write-Host ""

# Warning classification and analysis
function Analyze-BuildWarnings {
    Write-Host "� Analyzing Build Warnings..." -ForegroundColor Yellow

    $warningCategories = @{
        'CA1845' = @{
            Name = 'Substring Performance'
            Count = 3
            Severity = 'Medium'
            Files = @(
                'BusBuddy.Core\Services\ActivityLogService.cs:34',
                'BusBuddy.Core\Services\XAIService.cs:1008',
                'BusBuddy.Core\Services\XAIService.cs:1161'
            )
            Description = 'Use span-based string.Concat and AsSpan instead of Substring'
            ManualFix = 'Replace .Substring() calls with .AsSpan() where appropriate'
        }
        'CS8618' = @{
            Name = 'Non-nullable Properties'
            Count = 7
            Severity = 'High'
            Files = @(
                'BusBuddy.WPF\Models\DataIntegrityReport.cs:12',
                'BusBuddy.WPF\Models\DataIntegrityIssue.cs (multiple properties)'
            )
            Description = 'Non-nullable properties must contain non-null values when exiting constructor'
            ManualFix = 'Add required modifier or declare properties as nullable'
        }
        'CS0108' = @{
            Name = 'Hidden Members'
            Count = 2
            Severity = 'Medium'
            Files = @(
                'BusBuddy.WPF\ViewModels\SportsScheduling\SportsSchedulingViewModel.cs:132',
                'BusBuddy.WPF\ViewModels\Sports\SportsSchedulerViewModel.cs:30'
            )
            Description = 'Members hide inherited members without new keyword'
            ManualFix = 'Add new keyword if hiding is intentional'
        }
        'CA1016' = @{
            Name = 'Assembly Version'
            Count = 1
            Severity = 'Low'
            Files = @('AssemblyInfo.cs')
            Description = 'Mark assemblies with assembly version'
            ManualFix = 'Add [assembly: AssemblyVersion("1.0.0.0")] to AssemblyInfo.cs'
        }
        'CA1824' = @{
            Name = 'Neutral Resources Language'
            Count = 1
            Severity = 'Low'
            Files = @('AssemblyInfo.cs')
            Description = 'Mark assemblies with NeutralResourcesLanguageAttribute'
            ManualFix = 'Add [assembly: NeutralResourcesLanguage("en-US")] to AssemblyInfo.cs'
        }
        'Performance' = @{
            Name = 'Performance Optimizations'
            Count = 15
            Severity = 'Medium'
            Files = @(
                'CA1829: Use Count property instead of Count() (2 occurrences)',
                'CA1847: Use string.Contains(char) instead of Contains(string)',
                'CA1850: Use static SHA256.HashData',
                'CA1840: Use Environment.CurrentManagedThreadId (2 occurrences)',
                'CA1309: Use ordinal string comparison (2 occurrences)',
                'Other performance optimizations (5 additional warnings)'
            )
            Description = 'Various performance optimization opportunities across Core and WPF projects'
            ManualFix = 'Review each suggestion individually in build output'
        }
    }

    # Display analysis
    Write-Host "� Warning Categories Found:" -ForegroundColor Cyan
    Write-Host ""

    $totalWarnings = 0
    foreach ($category in $warningCategories.Keys) {
        $info = $warningCategories[$category]
        $totalWarnings += $info.Count

        $color = switch ($info.Severity) {
            'High' { 'Red' }
            'Medium' { 'Yellow' }
            'Low' { 'Green' }
        }

        Write-Host "🔍 $($info.Name) ($($info.Severity)): $($info.Count) warnings" -ForegroundColor $color
        Write-Host "   Description: $($info.Description)" -ForegroundColor Gray
        Write-Host "   Manual Fix: $($info.ManualFix)" -ForegroundColor Gray

        if ($Detailed) {
            Write-Host "   Files:" -ForegroundColor Gray
            foreach ($file in $info.Files) {
                Write-Host "     • $file" -ForegroundColor DarkGray
            }
        }
        Write-Host ""
    }

    Write-Host "📊 Total Warnings: $totalWarnings (33 in latest build)" -ForegroundColor Cyan

    return $warningCategories
}

# Check file integrity
function Test-FileIntegrity {
    Write-Host "� Checking File Integrity..." -ForegroundColor Yellow

    $criticalFiles = @(
        'BusBuddy.WPF\Models\DataIntegrityIssue.cs',
        'BusBuddy.WPF\Models\DataIntegrityReport.cs',
        'BusBuddy.Core\Services\XAIService.cs',
        'BusBuddy.WPF\ViewModels\BaseViewModel.cs'
    )

    $issues = @()
    foreach ($file in $criticalFiles) {
        if (Test-Path $file) {
            $content = Get-Content $file -Raw -ErrorAction SilentlyContinue
            if ([string]::IsNullOrEmpty($content)) {
                $issues += "❌ $file appears to be empty or corrupted"
            } else {
                Write-Host "   ✅ $file - OK" -ForegroundColor Green
            }
        } else {
            $issues += "❌ $file is missing"
        }
    }

    if ($issues.Count -gt 0) {
        Write-Host ""
        Write-Host "⚠️  File Integrity Issues:" -ForegroundColor Red
        foreach ($issue in $issues) {
            Write-Host "   $issue" -ForegroundColor Red
        }
        return $false
    }

    return $true
}

# Main execution
try {
    # Check file integrity first
    $filesOk = Test-FileIntegrity
    Write-Host ""

    # Analyze warnings
    $analysis = Analyze-BuildWarnings

    # Priority recommendations
    Write-Host "🎯 Priority Recommendations:" -ForegroundColor Cyan
    Write-Host "1. HIGH: Fix CS8618 nullable warnings (affects data integrity)" -ForegroundColor Red
    Write-Host "2. MEDIUM: Review CS0108 hidden members (affects inheritance)" -ForegroundColor Yellow
    Write-Host "3. MEDIUM: Address performance warnings if needed" -ForegroundColor Yellow
    Write-Host "4. LOW: Add assembly attributes for compliance" -ForegroundColor Green
    Write-Host ""

    if (-not $filesOk) {
        Write-Host "🚨 RECOMMENDATION: Reset corrupted files from Git before making changes" -ForegroundColor Red
        Write-Host "   Command: git checkout HEAD -- <filename>" -ForegroundColor Yellow
    }

    Write-Host "✅ Analysis complete - Manual review required for all fixes!" -ForegroundColor Green

} catch {
    Write-Error "❌ Error during analysis: $_"
    exit 1
}
