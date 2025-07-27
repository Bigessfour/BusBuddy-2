#Requires -Version 7.5
<#
.SYNOPSIS
    GitHub Actions Workflow Validator and Fixer

.DESCRIPTION
    Validates GitHub Actions workflow files for syntax errors and common issues.
    Fixes encoding problems, YAML syntax errors, and workflow configuration issues.

.EXAMPLE
    .\Fix-GitHub-Workflows.ps1 -ValidateOnly
    .\Fix-GitHub-Workflows.ps1 -FixIssues
#>

param(
    [switch]$ValidateOnly,
    [switch]$FixIssues
)

Write-Host "🔍 GitHub Actions Workflow Validator" -ForegroundColor Cyan
Write-Host ""

$WorkflowPath = ".github\workflows"
$IssuesFound = @()

if (-not (Test-Path $WorkflowPath)) {
    Write-Host "❌ Workflows directory not found: $WorkflowPath" -ForegroundColor Red
    return
}

# Get all workflow files
$WorkflowFiles = Get-ChildItem -Path $WorkflowPath -Filter "*.yml" -File

Write-Host "📄 Found $($WorkflowFiles.Count) workflow files to validate..." -ForegroundColor Yellow
Write-Host ""

foreach ($File in $WorkflowFiles) {
    Write-Host "🔍 Validating: $($File.Name)" -ForegroundColor Blue

    try {
        $Content = Get-Content -Path $File.FullName -Raw -Encoding UTF8
        $Lines = Get-Content -Path $File.FullName -Encoding UTF8

        # Check for encoding issues
        $EncodingIssues = @()
        if ($Content -match '[^\x00-\x7F]') {
            # Check for problematic characters
            for ($i = 0; $i -lt $Lines.Count; $i++) {
                $line = $Lines[$i]
                if ($line -match '[^\x00-\x7F]') {
                    $EncodingIssues += [PSCustomObject]@{
                        LineNumber = $i + 1
                        Issue = "Non-ASCII characters detected"
                        Line = $line
                        ProblematicChars = [regex]::Matches($line, '[^\x00-\x7F]').Value -join ', '
                    }
                }
            }
        }

        # Check for YAML syntax issues
        $YamlIssues = @()

        # Check for common YAML issues
        for ($i = 0; $i -lt $Lines.Count; $i++) {
            $line = $Lines[$i]
            $lineNumber = $i + 1

            # Check for tab characters (YAML should use spaces)
            if ($line -match "`t") {
                $YamlIssues += [PSCustomObject]@{
                    LineNumber = $lineNumber
                    Issue = "Tab characters detected (YAML requires spaces)"
                    Line = $line
                    Fix = $line -replace "`t", "  "
                }
            }

            # Check for inconsistent indentation
            if ($line -match '^(\s+)' -and $line -notmatch '^(\s*#)') {
                $indentation = $Matches[1]
                if ($indentation.Length % 2 -ne 0) {
                    $YamlIssues += [PSCustomObject]@{
                        LineNumber = $lineNumber
                        Issue = "Inconsistent indentation (should be multiples of 2)"
                        Line = $line
                        IndentationLength = $indentation.Length
                    }
                }
            }

            # Check for missing spaces after colons
            if ($line -match '^(\s*\w+):(\S)' -and $line -notmatch '^(\s*\w+):\|') {
                $YamlIssues += [PSCustomObject]@{
                    LineNumber = $lineNumber
                    Issue = "Missing space after colon"
                    Line = $line
                    Fix = $line -replace '^(\s*\w+):(\S)', '$1: $2'
                }
            }
        }

        # Check for GitHub Actions specific issues
        $ActionsIssues = @()

        # Check for required workflow elements
        if ($Content -notmatch '^name:\s*') {
            $ActionsIssues += [PSCustomObject]@{
                LineNumber = 1
                Issue = "Missing workflow name"
                Suggestion = "Add 'name: Your Workflow Name' at the top"
            }
        }

        if ($Content -notmatch '^on:\s*') {
            $ActionsIssues += [PSCustomObject]@{
                LineNumber = 1
                Issue = "Missing 'on' trigger definition"
                Suggestion = "Add 'on:' section to define when workflow runs"
            }
        }

        if ($Content -notmatch '^jobs:\s*') {
            $ActionsIssues += [PSCustomObject]@{
                LineNumber = 1
                Issue = "Missing 'jobs' section"
                Suggestion = "Add 'jobs:' section with at least one job"
            }
        }

        # Report issues for this file
        $TotalIssues = $EncodingIssues.Count + $YamlIssues.Count + $ActionsIssues.Count

        if ($TotalIssues -eq 0) {
            Write-Host "  ✅ No issues found" -ForegroundColor Green
        } else {
            Write-Host "  ⚠️  Found $TotalIssues issue(s)" -ForegroundColor Yellow

            # Encoding issues
            if ($EncodingIssues.Count -gt 0) {
                Write-Host "    📝 Encoding Issues:" -ForegroundColor Red
                foreach ($Issue in $EncodingIssues) {
                    Write-Host "      Line $($Issue.LineNumber): $($Issue.Issue)" -ForegroundColor Red
                    Write-Host "        Characters: $($Issue.ProblematicChars)" -ForegroundColor Gray
                }
            }

            # YAML issues
            if ($YamlIssues.Count -gt 0) {
                Write-Host "    📝 YAML Issues:" -ForegroundColor Yellow
                foreach ($Issue in $YamlIssues) {
                    Write-Host "      Line $($Issue.LineNumber): $($Issue.Issue)" -ForegroundColor Yellow
                    if ($Issue.Fix) {
                        Write-Host "        Fix: $($Issue.Fix)" -ForegroundColor Green
                    }
                }
            }

            # Actions issues
            if ($ActionsIssues.Count -gt 0) {
                Write-Host "    📝 Actions Issues:" -ForegroundColor Magenta
                foreach ($Issue in $ActionsIssues) {
                    Write-Host "      $($Issue.Issue)" -ForegroundColor Magenta
                    Write-Host "        Suggestion: $($Issue.Suggestion)" -ForegroundColor Blue
                }
            }
        }

        # Store issues for fixing
        $IssuesFound += [PSCustomObject]@{
            File = $File.FullName
            FileName = $File.Name
            EncodingIssues = $EncodingIssues
            YamlIssues = $YamlIssues
            ActionsIssues = $ActionsIssues
            TotalIssues = $TotalIssues
        }

    } catch {
        Write-Host "  ❌ Error validating file: $($_.Exception.Message)" -ForegroundColor Red
        $IssuesFound += [PSCustomObject]@{
            File = $File.FullName
            FileName = $File.Name
            Error = $_.Exception.Message
            TotalIssues = 1
        }
    }

    Write-Host ""
}

# Summary
$TotalFiles = $WorkflowFiles.Count
$FilesWithIssues = ($IssuesFound | Where-Object { $_.TotalIssues -gt 0 }).Count
$TotalIssues = ($IssuesFound | Measure-Object -Property TotalIssues -Sum).Sum

Write-Host "📊 Validation Summary" -ForegroundColor Cyan
Write-Host "Files Validated: $TotalFiles" -ForegroundColor White
Write-Host "Files with Issues: $FilesWithIssues" -ForegroundColor White
Write-Host "Total Issues Found: $TotalIssues" -ForegroundColor White

if ($TotalIssues -eq 0) {
    Write-Host ""
    Write-Host "✅ All workflow files are valid!" -ForegroundColor Green
    return
}

# Apply fixes if requested
if ($FixIssues) {
    Write-Host ""
    Write-Host "🔧 Applying fixes..." -ForegroundColor Cyan

    foreach ($FileIssue in $IssuesFound | Where-Object { $_.TotalIssues -gt 0 -and -not $_.Error }) {
        Write-Host "🔧 Fixing: $($FileIssue.FileName)" -ForegroundColor Blue

        $Content = Get-Content -Path $FileIssue.File -Raw -Encoding UTF8
        $Lines = Get-Content -Path $FileIssue.File -Encoding UTF8
        $Modified = $false

        # Fix encoding issues
        if ($FileIssue.EncodingIssues.Count -gt 0) {
            Write-Host "  📝 Fixing encoding issues..." -ForegroundColor Yellow

            # Common emoji/character replacements for encoding issues
            $Content = $Content -replace 'ðŸ"¥', '🔥'
            $Content = $Content -replace 'ðŸ§ª', '🧪'
            $Content = $Content -replace 'ðŸ"¦', '📦'
            $Content = $Content -replace 'ðŸŽ‰', '🎉'
            $Content = $Content -replace 'âœ…', '✅'
            $Content = $Content -replace 'ðŸš€', '🚀'
            $Content = $Content -replace 'ðŸ"', '📋'
            $Content = $Content -replace 'ðŸ"Š', '📊'
            $Content = $Content -replace 'ðŸ›¡ï¸', '🛡️'

            $Modified = $true
        }

        # Fix YAML issues
        if ($FileIssue.YamlIssues.Count -gt 0) {
            Write-Host "  📝 Fixing YAML issues..." -ForegroundColor Yellow

            foreach ($Issue in $FileIssue.YamlIssues) {
                if ($Issue.Fix) {
                    $Lines[$Issue.LineNumber - 1] = $Issue.Fix
                    $Modified = $true
                }
            }

            if ($Modified) {
                $Content = $Lines -join "`n"
            }
        }

        # Save fixed content
        if ($Modified) {
            # Create backup
            $BackupPath = "$($FileIssue.File).backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
            Copy-Item -Path $FileIssue.File -Destination $BackupPath

            # Save fixed content
            Set-Content -Path $FileIssue.File -Value $Content -Encoding UTF8

            Write-Host "  ✅ Fixed and saved. Backup: $([System.IO.Path]::GetFileName($BackupPath))" -ForegroundColor Green
        } else {
            Write-Host "  ℹ️  No automatic fixes available" -ForegroundColor Blue
        }
    }

    Write-Host ""
    Write-Host "✅ Fix process complete!" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "ℹ️  Run with -FixIssues to apply automatic fixes" -ForegroundColor Blue
}

Write-Host ""
Write-Host "🔗 To test workflows locally, consider using:" -ForegroundColor Cyan
Write-Host "   act (GitHub Actions runner): https://github.com/nektos/act" -ForegroundColor White
