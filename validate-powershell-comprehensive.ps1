#Requires -Version 7.5

<#
.SYNOPSIS
    Comprehensive PowerShell Validation for BusBuddy Project
.DESCRIPTION
    Validates all PowerShell files for syntax errors, best practices, and compatibility issues.
    Provides detailed reports and automated fixes where possible.
.EXAMPLE
    .\validate-powershell-comprehensive.ps1 -Fix
.NOTES
    Author: BusBuddy Development Team
    Date: July 28, 2025
    Purpose: Complete PowerShell file validation and repair
#>

param(
    [Parameter(Mandatory = $false)]
    [switch]$Fix,

    [Parameter(Mandatory = $false)]
    [switch]$DetailedOutput,

    [Parameter(Mandatory = $false)]
    [string]$OutputPath = ".\PowerShell-Validation-Report-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
)

$ErrorActionPreference = "Continue"
Set-StrictMode -Version Latest

Write-Host "🔍 COMPREHENSIVE POWERSHELL VALIDATION" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Initialize validation results
$validationResults = @{
    ValidationDate = Get-Date
    TotalFiles = 0
    ValidFiles = 0
    FilesWithErrors = 0
    FilesWithWarnings = 0
    FileResults = @{}
    Summary = @{}
    Recommendations = @()
}

# Step 1: Find all PowerShell files
Write-Host "📂 Step 1: Discovering PowerShell Files" -ForegroundColor Yellow
Write-Host "───────────────────────────────────────" -ForegroundColor Yellow

$powerShellFiles = @()
$searchPatterns = @("*.ps1", "*.psm1", "*.psd1")

foreach ($pattern in $searchPatterns) {
    $files = Get-ChildItem -Path . -Filter $pattern -Recurse -ErrorAction SilentlyContinue
    $powerShellFiles += $files
}

$validationResults.TotalFiles = $powerShellFiles.Count
Write-Host "✅ Found $($powerShellFiles.Count) PowerShell files" -ForegroundColor Green

if ($powerShellFiles.Count -eq 0) {
    Write-Host "❌ No PowerShell files found to validate" -ForegroundColor Red
    exit 1
}

# Group files by type
$filesByType = $powerShellFiles | Group-Object Extension
Write-Host ""
Write-Host "📊 File Distribution:" -ForegroundColor Cyan
foreach ($group in $filesByType) {
    Write-Host "   $($group.Name): $($group.Count) files" -ForegroundColor White
}

Write-Host ""

# Step 2: Validate each file
Write-Host "🔍 Step 2: Validating PowerShell Files" -ForegroundColor Yellow
Write-Host "──────────────────────────────────────" -ForegroundColor Yellow

foreach ($file in $powerShellFiles) {
    Write-Host "🔍 Validating: $($file.Name)" -ForegroundColor Cyan

    $fileResult = @{
        Path = $file.FullName
        RelativePath = $file.FullName.Replace($PWD, ".")
        Type = $file.Extension
        Size = $file.Length
        LastModified = $file.LastWriteTime
        SyntaxErrors = @()
        Warnings = @()
        StyleIssues = @()
        Status = "Unknown"
        ParseSuccess = $false
        HasRequiresStatement = $false
        PowerShellVersion = $null
    }

    try {
        # Read file content
        $content = Get-Content $file.FullName -Raw -ErrorAction Stop
        $fileResult.LineCount = ($content -split "`n").Count

        # Check for #Requires statement
        if ($content -match '#Requires -Version\s+(\d+\.\d+)') {
            $fileResult.HasRequiresStatement = $true
            $fileResult.PowerShellVersion = $matches[1]
        }

        # Parse the PowerShell file
        $tokens = $null
        $parseErrors = $null

        try {
            $parsedAst = [System.Management.Automation.Language.Parser]::ParseFile(
                $file.FullName,
                [ref]$tokens,
                [ref]$parseErrors
            )

            $fileResult.ParseSuccess = $true
            $fileResult.TokenCount = $tokens.Count

            if ($parseErrors.Count -eq 0) {
                Write-Host "   ✅ Syntax: Valid" -ForegroundColor Green
                $fileResult.Status = "Valid"
                $validationResults.ValidFiles++
            } else {
                Write-Host "   ❌ Syntax: $($parseErrors.Count) errors" -ForegroundColor Red
                $fileResult.Status = "SyntaxErrors"
                $fileResult.SyntaxErrors = $parseErrors | ForEach-Object {
                    @{
                        Message = $_.Message
                        Line = $_.Extent.StartLineNumber
                        Column = $_.Extent.StartColumnNumber
                        Text = $_.Extent.Text
                        ErrorId = $_.ErrorId
                    }
                }
                $validationResults.FilesWithErrors++

                # Display errors
                foreach ($parseError in $parseErrors) {
                    Write-Host "     Line $($parseError.Extent.StartLineNumber): $($parseError.Message)" -ForegroundColor Red
                }
            }

        } catch {
            Write-Host "   ❌ Parse Failed: $($_.Exception.Message)" -ForegroundColor Red
            $fileResult.Status = "ParseFailed"
            $fileResult.SyntaxErrors += @{
                Message = $_.Exception.Message
                Type = "ParseException"
            }
            $validationResults.FilesWithErrors++
        }

        # Additional validations

        # Check for common issues
        $commonIssues = @()

        # Check for using variables in script blocks (common issue)
        if ($content -match '\$using:') {
            $usingMatches = [regex]::Matches($content, '\$using:(\w+)')
            foreach ($match in $usingMatches) {
                $commonIssues += @{
                    Type = "UsingVariable"
                    Message = "Using variable '$($match.Groups[1].Value)' may cause scope issues"
                    Recommendation = "Consider using -ArgumentList parameter or different scoping approach"
                }
            }
        }

        # Check for hardcoded paths
        if ($content -match 'C:\\|D:\\') {
            $commonIssues += @{
                Type = "HardcodedPath"
                Message = "Hardcoded drive paths detected"
                Recommendation = "Use relative paths or environment variables"
            }
        }

        # Check for missing error handling
        $dotnetCommands = [regex]::Matches($content, 'dotnet\s+\w+')
        $tryBlocks = [regex]::Matches($content, 'try\s*\{')

        if ($dotnetCommands.Count -gt 0 -and $tryBlocks.Count -eq 0) {
            $commonIssues += @{
                Type = "MissingErrorHandling"
                Message = "External commands without try/catch blocks"
                Recommendation = "Wrap external commands in try/catch blocks"
            }
        }

        # Check for PowerShell version compatibility
        if (-not $fileResult.HasRequiresStatement) {
            $commonIssues += @{
                Type = "MissingRequires"
                Message = "No #Requires statement found"
                Recommendation = "Add '#Requires -Version 7.5' at the top of the file"
            }
        }

        $fileResult.StyleIssues = $commonIssues

        if ($commonIssues.Count -gt 0) {
            Write-Host "   ⚠️ Issues: $($commonIssues.Count) style/best practice issues" -ForegroundColor Yellow
            $validationResults.FilesWithWarnings++

            if ($DetailedOutput) {
                foreach ($issue in $commonIssues) {
                    Write-Host "     $($issue.Type): $($issue.Message)" -ForegroundColor Yellow
                }
            }
        }

    } catch {
        Write-Host "   ❌ Validation Failed: $($_.Exception.Message)" -ForegroundColor Red
        $fileResult.Status = "ValidationFailed"
        $fileResult.SyntaxErrors += @{
            Message = $_.Exception.Message
            Type = "ValidationException"
        }
        $validationResults.FilesWithErrors++
    }

    $validationResults.FileResults[$file.FullName] = $fileResult
    Write-Host ""
}

# Step 3: Generate summary and recommendations
Write-Host "📊 Step 3: Generating Summary and Recommendations" -ForegroundColor Yellow
Write-Host "─────────────────────────────────────────────────" -ForegroundColor Yellow

$validationResults.Summary = @{
    OverallStatus = if ($validationResults.FilesWithErrors -eq 0) { "Healthy" }
                   elseif ($validationResults.FilesWithErrors -lt $validationResults.TotalFiles / 2) { "NeedsAttention" }
                   else { "Critical" }

    ValidPercentage = [math]::Round(($validationResults.ValidFiles / $validationResults.TotalFiles) * 100, 1)
    ErrorPercentage = [math]::Round(($validationResults.FilesWithErrors / $validationResults.TotalFiles) * 100, 1)
    WarningPercentage = [math]::Round(($validationResults.FilesWithWarnings / $validationResults.TotalFiles) * 100, 1)

    MostCommonIssues = @()
    CriticalFiles = @()
}

# Identify most common issues
$allIssues = $validationResults.FileResults.Values | ForEach-Object {
    $_.SyntaxErrors + $_.StyleIssues
} | Group-Object { if ($_.Type) { $_.Type } else { "SyntaxError" } } | Sort-Object Count -Descending

$validationResults.Summary.MostCommonIssues = $allIssues | Select-Object -First 5 | ForEach-Object {
    @{ Type = $_.Name; Count = $_.Count }
}

# Identify critical files
$validationResults.Summary.CriticalFiles = $validationResults.FileResults.Values |
    Where-Object { $_.Status -eq "SyntaxErrors" -or $_.Status -eq "ParseFailed" } |
    Select-Object -ExpandProperty RelativePath

# Generate recommendations
$recommendations = @()

if ($validationResults.FilesWithErrors -gt 0) {
    $recommendations += "CRITICAL: Fix syntax errors in $($validationResults.FilesWithErrors) files before proceeding"
}

if ($allIssues | Where-Object { $_.Name -eq "UsingVariable" }) {
    $recommendations += "Fix Using variable scope issues - use -ArgumentList or different scoping"
}

if ($allIssues | Where-Object { $_.Name -eq "MissingRequires" }) {
    $recommendations += "Add #Requires -Version 7.5 statements to files missing them"
}

if ($allIssues | Where-Object { $_.Name -eq "MissingErrorHandling" }) {
    $recommendations += "Add try/catch blocks around external command calls"
}

$validationResults.Recommendations = $recommendations

# Display results
Write-Host ""
Write-Host "📊 VALIDATION SUMMARY" -ForegroundColor Cyan
Write-Host "═══════════════════" -ForegroundColor Cyan
Write-Host "Total Files: $($validationResults.TotalFiles)" -ForegroundColor White
Write-Host "Valid Files: $($validationResults.ValidFiles) ($($validationResults.Summary.ValidPercentage)%)" -ForegroundColor Green
Write-Host "Files with Errors: $($validationResults.FilesWithErrors) ($($validationResults.Summary.ErrorPercentage)%)" -ForegroundColor Red
Write-Host "Files with Warnings: $($validationResults.FilesWithWarnings) ($($validationResults.Summary.WarningPercentage)%)" -ForegroundColor Yellow
Write-Host ""

$statusColor = switch ($validationResults.Summary.OverallStatus) {
    "Healthy" { "Green" }
    "NeedsAttention" { "Yellow" }
    "Critical" { "Red" }
    default { "White" }
}

Write-Host "Overall Status: " -NoNewline
Write-Host $validationResults.Summary.OverallStatus -ForegroundColor $statusColor
Write-Host ""

if ($validationResults.Summary.MostCommonIssues.Count -gt 0) {
    Write-Host "🔥 Most Common Issues:" -ForegroundColor Red
    foreach ($issue in $validationResults.Summary.MostCommonIssues) {
        Write-Host "   • $($issue.Type): $($issue.Count) occurrences" -ForegroundColor Yellow
    }
    Write-Host ""
}

if ($validationResults.Summary.CriticalFiles -and $validationResults.Summary.CriticalFiles.Count -gt 0) {
    Write-Host "🚨 Files Requiring Immediate Attention:" -ForegroundColor Red
    foreach ($file in $validationResults.Summary.CriticalFiles) {
        Write-Host "   • $file" -ForegroundColor Red
    }
    Write-Host ""
}

if ($validationResults.Recommendations.Count -gt 0) {
    Write-Host "💡 Recommendations:" -ForegroundColor Cyan
    foreach ($rec in $validationResults.Recommendations) {
        Write-Host "   • $rec" -ForegroundColor Yellow
    }
    Write-Host ""
}

# Step 4: Save detailed report
Write-Host "💾 Step 4: Saving Detailed Report" -ForegroundColor Yellow
Write-Host "─────────────────────────────────" -ForegroundColor Yellow

try {
    $validationResults | ConvertTo-Json -Depth 10 | Out-File -FilePath $OutputPath -Encoding UTF8
    Write-Host "✅ Detailed report saved to: $OutputPath" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to save report: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 5: Auto-fix if requested
if ($Fix -and $validationResults.FilesWithErrors -gt 0) {
    Write-Host ""
    Write-Host "🔧 Step 5: Applying Automatic Fixes" -ForegroundColor Yellow
    Write-Host "───────────────────────────────────" -ForegroundColor Yellow

    foreach ($filePath in $validationResults.FileResults.Keys) {
        $fileResult = $validationResults.FileResults[$filePath]

        if ($fileResult.Status -eq "SyntaxErrors") {
            Write-Host "🔧 Attempting fixes for: $($fileResult.RelativePath)" -ForegroundColor Cyan

            # Apply common fixes
            $content = Get-Content $filePath -Raw
            $originalContent = $content
            $fixesApplied = @()

            # Fix 1: Add missing #Requires statement
            if (-not $fileResult.HasRequiresStatement) {
                $content = "#Requires -Version 7.5`n`n$content"
                $fixesApplied += "Added #Requires statement"
            }

            # Fix 2: Fix common using variable issues
            if ($content -match '\$using:(\w+)') {
                Write-Host "   ⚠️ Using variables detected - manual fix required" -ForegroundColor Yellow
                $fixesApplied += "Manual fix needed for using variables"
            }

            # Fix 3: Add basic error handling template
            if ($content -match 'dotnet\s+\w+' -and $content -notmatch 'try\s*\{') {
                Write-Host "   💡 Consider adding try/catch blocks for external commands" -ForegroundColor Yellow
                $fixesApplied += "Recommended: Add error handling"
            }

            # Apply fixes if any were made
            if ($content -ne $originalContent) {
                try {
                    $backupPath = "$filePath.backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
                    Copy-Item $filePath $backupPath

                    Set-Content -Path $filePath -Value $content -Encoding UTF8
                    Write-Host "   ✅ Applied fixes: $($fixesApplied -join ', ')" -ForegroundColor Green
                    Write-Host "   💾 Backup created: $backupPath" -ForegroundColor Gray
                } catch {
                    Write-Host "   ❌ Failed to apply fixes: $($_.Exception.Message)" -ForegroundColor Red
                }
            } else {
                Write-Host "   ℹ️ No automatic fixes available" -ForegroundColor Gray
            }
        }
    }
}

Write-Host ""
Write-Host "✅ VALIDATION COMPLETE" -ForegroundColor Green
Write-Host "════════════════════" -ForegroundColor Green

# Return summary for programmatic use
return $validationResults.Summary
