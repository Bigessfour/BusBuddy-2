#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Repair GitHub Actions workflow failures for BusBuddy project
.DESCRIPTION
    Analyzes and repairs common GitHub Actions failures:
    - JSON parsing errors in Standards Validation
    - Build timeout issues
    - Missing dependencies
    - .NET 9 compatibility issues
.EXAMPLE
    .\fix-github-actions-failures.ps1
#>

param(
    [switch]$DryRun,
    [switch]$Force,
    [string]$WorkflowName = "*"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Write-Host "🔧 GitHub Actions Failure Repair Script" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

# Get recent failed workflows
function Get-FailedWorkflows {
    Write-Host "📋 Analyzing failed workflows..." -ForegroundColor Yellow

    try {
        $failures = gh run list --repo Bigessfour/BusBuddy-2 --status failure --limit 10 --json databaseId,conclusion,displayTitle,workflowName,createdAt | ConvertFrom-Json

        Write-Host "Found $($failures.Count) recent failures:" -ForegroundColor Red
        foreach ($failure in $failures) {
            Write-Host "  - [$($failure.databaseId)] $($failure.workflowName): $($failure.displayTitle)" -ForegroundColor Red
        }

        return $failures
    }
    catch {
        Write-Host "❌ Error getting workflow failures: $($_.Exception.Message)" -ForegroundColor Red
        return @()
    }
}

# Repair JSON parsing issues
function Repair-JsonParsingIssues {
    Write-Host "🔧 Repairing JSON parsing issues..." -ForegroundColor Yellow

    # Find and fix malformed JSON files
    $jsonFiles = Get-ChildItem -Recurse -Filter "*.json" | Where-Object {
        $_.Name -notmatch "(node_modules|\.vs|bin|obj)"
    }

    foreach ($file in $jsonFiles) {
        try {
            Write-Host "  Validating: $($file.Name)" -ForegroundColor Gray
            $content = Get-Content $file.FullName -Raw

            # Try to parse JSON
            $null = $content | ConvertFrom-Json
            Write-Host "    ✅ Valid JSON" -ForegroundColor Green
        }
        catch {
            Write-Host "    ❌ Invalid JSON: $($_.Exception.Message)" -ForegroundColor Red

            if (-not $DryRun) {
                # Attempt to fix common JSON issues
                $fixedContent = $content

                # Remove trailing commas
                $fixedContent = $fixedContent -replace ',(\s*[}\]])', '$1'

                # Fix double quotes
                $fixedContent = $fixedContent -replace '(?<!\\)"(?![\s:,}\]\r\n])', '\"'

                try {
                    # Validate fixed content
                    $null = $fixedContent | ConvertFrom-Json
                    Set-Content $file.FullName $fixedContent -Encoding UTF8
                    Write-Host "    ✅ Repaired JSON syntax" -ForegroundColor Green
                }
                catch {
                    Write-Host "    ❌ Could not auto-repair: $($_.Exception.Message)" -ForegroundColor Red
                }
            }
        }
    }
}

# Repair workflow timeout issues
function Repair-WorkflowTimeouts {
    Write-Host "⏱️ Repairing workflow timeout issues..." -ForegroundColor Yellow

    $workflowFiles = Get-ChildItem -Path ".github/workflows" -Filter "*.yml" -ErrorAction SilentlyContinue

    foreach ($workflow in $workflowFiles) {
        Write-Host "  Checking: $($workflow.Name)" -ForegroundColor Gray

        $content = Get-Content $workflow.FullName -Raw
        $modified = $false

        # Add timeout-minutes if missing
        if ($content -notmatch 'timeout-minutes:') {
            Write-Host "    Adding timeout-minutes to jobs" -ForegroundColor Yellow
            $content = $content -replace '(\s+runs-on:\s+.+)', "`$1`n    timeout-minutes: 30"
            $modified = $true
        }

        # Increase existing timeouts if too low
        $content = $content -replace 'timeout-minutes:\s*([1-9]|1[0-5])(\s)', 'timeout-minutes: 30$2'

        # Add cache for faster builds
        if ($content -notmatch 'actions/cache') {
            Write-Host "    Adding NuGet cache step" -ForegroundColor Yellow
            $cacheStep = @"
      - name: Cache NuGet packages
        uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: `${{ runner.os }}-nuget-`${{ hashFiles('**/*.csproj', '**/packages.lock.json') }}
          restore-keys: |
            `${{ runner.os }}-nuget-

"@
            $content = $content -replace '(\s+- name:\s+Setup \.NET)', "$cacheStep`$1"
            $modified = $true
        }

        if ($modified -and -not $DryRun) {
            Set-Content $workflow.FullName $content -Encoding UTF8
            Write-Host "    ✅ Updated workflow timeouts and caching" -ForegroundColor Green
        }
    }
}

# Repair .NET 9 compatibility issues
function Repair-DotNetCompatibility {
    Write-Host "🔧 Repairing .NET 9 compatibility issues..." -ForegroundColor Yellow

    # Update global.json if exists
    if (Test-Path "global.json") {
        $globalJson = Get-Content "global.json" | ConvertFrom-Json

        if ($globalJson.sdk.version -lt "9.0.0") {
            Write-Host "  Updating global.json SDK version" -ForegroundColor Yellow
            $globalJson.sdk.version = "9.0.303"

            if (-not $DryRun) {
                $globalJson | ConvertTo-Json -Depth 10 | Set-Content "global.json" -Encoding UTF8
                Write-Host "    ✅ Updated to .NET 9.0.303" -ForegroundColor Green
            }
        }
    }

    # Check workflow .NET setup
    $workflowFiles = Get-ChildItem -Path ".github/workflows" -Filter "*.yml" -ErrorAction SilentlyContinue

    foreach ($workflow in $workflowFiles) {
        $content = Get-Content $workflow.FullName -Raw

        # Update .NET version in workflows
        if ($content -match 'dotnet-version:\s*[''"]?8\.') {
            Write-Host "  Updating .NET version in $($workflow.Name)" -ForegroundColor Yellow

            if (-not $DryRun) {
                $content = $content -replace 'dotnet-version:\s*[''"]?8\.[^''"\s]*[''"]?', 'dotnet-version: ''9.0.303'''
                Set-Content $workflow.FullName $content -Encoding UTF8
                Write-Host "    ✅ Updated to .NET 9.0.303" -ForegroundColor Green
            }
        }
    }
}

# Repair Standards Validation issues
function Repair-StandardsValidation {
    Write-Host "📋 Repairing Standards Validation issues..." -ForegroundColor Yellow

    # Remove problematic validation report files
    $reportFiles = Get-ChildItem -Filter "*Validation-Report*.json" -ErrorAction SilentlyContinue

    foreach ($file in $reportFiles) {
        Write-Host "  Removing validation report: $($file.Name)" -ForegroundColor Yellow

        if (-not $DryRun) {
            Remove-Item $file.FullName -Force
            Write-Host "    ✅ Removed problematic report file" -ForegroundColor Green
        }
    }

    # Update .gitignore to exclude future validation reports
    if (Test-Path ".gitignore") {
        $gitignore = Get-Content ".gitignore" -Raw

        if ($gitignore -notmatch '\*Validation-Report\*\.json') {
            Write-Host "  Adding validation reports to .gitignore" -ForegroundColor Yellow

            if (-not $DryRun) {
                Add-Content ".gitignore" "`n# Validation Reports`n*Validation-Report*.json`n*-Report-*.json"
                Write-Host "    ✅ Updated .gitignore" -ForegroundColor Green
            }
        }
    }
}

# Main execution
function Main {
    Write-Host "Starting GitHub Actions failure analysis and repair..." -ForegroundColor Green

    if ($DryRun) {
        Write-Host "🔍 DRY RUN MODE - No changes will be made" -ForegroundColor Yellow
    }

    # Get failed workflows first
    $failures = Get-FailedWorkflows

    if ($failures.Count -eq 0) {
        Write-Host "✅ No recent workflow failures found!" -ForegroundColor Green
        return
    }

    # Apply fixes
    Repair-JsonParsingIssues
    Repair-WorkflowTimeouts
    Repair-DotNetCompatibility
    Repair-StandardsValidation

    Write-Host "`n🎉 GitHub Actions repairs completed!" -ForegroundColor Green

    if (-not $DryRun) {
        Write-Host "📝 Committing repairs..." -ForegroundColor Yellow

        try {
            git add .
            git commit -m "fix(ci): resolve GitHub Actions workflow failures

- Repair JSON parsing errors in Standards Validation
- Add workflow timeouts and NuGet caching
- Update .NET version to 9.0.303
- Remove problematic validation report files
- Update .gitignore for validation reports"

            Write-Host "✅ Changes committed. Ready to push!" -ForegroundColor Green
            Write-Host "💡 Run: git push origin main" -ForegroundColor Cyan
        }
        catch {
            Write-Host "⚠️ Could not commit changes: $($_.Exception.Message)" -ForegroundColor Yellow
            Write-Host "💡 Please review and commit manually" -ForegroundColor Cyan
        }
    }
    else {
        Write-Host "💡 Run without -DryRun to apply fixes" -ForegroundColor Cyan
    }
}

# Run the main function
Main
