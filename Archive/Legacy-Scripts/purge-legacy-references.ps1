#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Remove Legacy References from README and Documentation

.DESCRIPTION
    Systematically removes all legacy AI-Assistant, behavioral learning,
    unlock-phd-mentor, and other outdated references from documentation files.

.NOTES
    Author: GitHub Copilot
    Date: 2025-07-25
    Version: 1.0
#>

param(
    [switch]$DryRun = $true
)

Write-Host "🧹 Legacy Reference Cleanup for Documentation" -ForegroundColor Cyan
Write-Host "📅 $(Get-Date)" -ForegroundColor Gray
Write-Host ""

if ($DryRun) {
    Write-Host "🔍 DRY RUN MODE - Files will be analyzed but not modified" -ForegroundColor Yellow
    Write-Host "Use -DryRun:`$false to actually clean files" -ForegroundColor Yellow
    Write-Host ""
}

# Files to clean
$filesToClean = @("README.md", "QUICK-START.md")

# Legacy patterns to remove
$legacyPatterns = @{
    "unlock-phd-mentor"   = @{
        "description"  = "PhD Mentor system references"
        "replacements" = @(
            @{ "pattern" = ".*unlock-phd-mentor.*"; "replacement" = "" }
            @{ "pattern" = ".*PhD Mentor.*"; "replacement" = "" }
        )
    }
    "AI-Assistant"        = @{
        "description"  = "AI-Assistant tool references"
        "replacements" = @(
            @{ "pattern" = ".*AI-Assistant.*"; "replacement" = "" }
            @{ "pattern" = ".*methodology-reinforcement.*"; "replacement" = "" }
        )
    }
    "behavioral-learning" = @{
        "description"  = "Behavioral learning system references"
        "replacements" = @(
            @{ "pattern" = ".*[Bb]ehavioral [Ll]earning.*"; "replacement" = "" }
            @{ "pattern" = ".*🧠.*AI THAT LEARNS.*"; "replacement" = "" }
        )
    }
}

function Remove-LegacyReferences {
    param(
        [string]$FilePath
    )

    if (-not (Test-Path $FilePath)) {
        Write-Host "⚠️ File not found: $FilePath" -ForegroundColor Yellow
        return
    }

    Write-Host "🔍 Analyzing: $FilePath" -ForegroundColor Cyan

    $content = Get-Content $FilePath -Raw
    $originalLineCount = ($content -split "`n").Count
    $modifiedContent = $content
    $changesFound = 0

    foreach ($patternGroup in $legacyPatterns.GetEnumerator()) {
        $groupName = $patternGroup.Key
        $groupInfo = $patternGroup.Value

        Write-Host "  🔎 Checking for: $($groupInfo.description)" -ForegroundColor Gray

        foreach ($replacement in $groupInfo.replacements) {
            $pattern = $replacement.pattern
            $newValue = $replacement.replacement

            $matches = [regex]::Matches($modifiedContent, $pattern, [System.Text.RegularExpressions.RegexOptions]::Multiline)
            if ($matches.Count -gt 0) {
                Write-Host "    ⚠️ Found $($matches.Count) matches for pattern: $pattern" -ForegroundColor Yellow
                $changesFound += $matches.Count

                if (-not $DryRun) {
                    $modifiedContent = [regex]::Replace($modifiedContent, $pattern, $newValue, [System.Text.RegularExpressions.RegexOptions]::Multiline)
                }
            }
        }
    }

    # Remove empty lines that might be left behind
    if (-not $DryRun -and $changesFound -gt 0) {
        # Remove multiple consecutive empty lines
        $modifiedContent = [regex]::Replace($modifiedContent, "`n`n`n+", "`n`n", [System.Text.RegularExpressions.RegexOptions]::Multiline)

        # Write back to file
        Set-Content -Path $FilePath -Value $modifiedContent -NoNewline

        $newLineCount = ($modifiedContent -split "`n").Count
        $linesRemoved = $originalLineCount - $newLineCount

        Write-Host "    ✅ Updated $FilePath" -ForegroundColor Green
        Write-Host "    📊 Lines: $originalLineCount → $newLineCount (removed $linesRemoved)" -ForegroundColor Green
    }
    elseif ($changesFound -gt 0) {
        Write-Host "    📋 Would remove $changesFound legacy references" -ForegroundColor Yellow
    }
    else {
        Write-Host "    ✅ No legacy references found" -ForegroundColor Green
    }
}

function Show-FileSummary {
    param([string]$FilePath)

    if (Test-Path $FilePath) {
        $lineCount = (Get-Content $FilePath).Count
        $size = (Get-Item $FilePath).Length
        $sizeKB = [math]::Round($size / 1KB, 2)

        Write-Host "📄 $FilePath - $lineCount lines, $sizeKB KB" -ForegroundColor Gray
    }
}

Write-Host "📊 BEFORE CLEANUP:" -ForegroundColor Magenta
foreach ($file in $filesToClean) {
    Show-FileSummary -FilePath $file
}

Write-Host ""
Write-Host "🧹 CLEANING LEGACY REFERENCES:" -ForegroundColor Magenta

foreach ($file in $filesToClean) {
    Remove-LegacyReferences -FilePath $file
    Write-Host ""
}

if (-not $DryRun) {
    Write-Host "📊 AFTER CLEANUP:" -ForegroundColor Magenta
    foreach ($file in $filesToClean) {
        Show-FileSummary -FilePath $file
    }
}

Write-Host ""
Write-Host "🎯 RECOMMENDATIONS:" -ForegroundColor Cyan
Write-Host "  1. Review cleaned files manually for context" -ForegroundColor Gray
Write-Host "  2. Test that documentation still makes sense" -ForegroundColor Gray
Write-Host "  3. Commit changes: git add *.md && git commit -m 'Clean: Remove legacy references'" -ForegroundColor Gray

if ($DryRun) {
    Write-Host ""
    Write-Host "🚀 To execute cleanup: .\purge-legacy-references.ps1 -DryRun:`$false" -ForegroundColor Green
}

Write-Host ""
Write-Host "✅ Legacy reference cleanup complete!" -ForegroundColor Green
