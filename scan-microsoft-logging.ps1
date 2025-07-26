#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Scans the entire BusBuddy workspace for Microsoft.Extensions.Logging references

.DESCRIPTION
    This script performs a comprehensive scan of the workspace to identify any remaining
    Microsoft.Extensions.Logging references after the migration to Serilog with enrichments.

.PARAMETER WorkspacePath
    The root path of the workspace to scan (defaults to current directory)

.PARAMETER OutputFormat
    Output format: 'Console', 'JSON', or 'CSV' (default: Console)

.PARAMETER IncludePackageReferences
    Include package references in project files (default: true)

.PARAMETER ExcludeArchive
    Exclude Archive and Legacy directories from scan (default: true)

.EXAMPLE
    .\scan-microsoft-logging.ps1

.EXAMPLE
    .\scan-microsoft-logging.ps1 -OutputFormat JSON -IncludePackageReferences $true
#>

param(
    [string]$WorkspacePath = ".",
    [ValidateSet("Console", "JSON", "CSV")]
    [string]$OutputFormat = "Console",
    [bool]$IncludePackageReferences = $true,
    [bool]$ExcludeArchive = $true
)

# Initialize results collection
$results = @{
    Summary         = @{
        TotalFilesScanned         = 0
        FilesWithMicrosoftLogging = 0
        TotalOccurrences          = 0
        ScanTimestamp             = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        WorkspacePath             = (Resolve-Path $WorkspacePath).Path
    }
    Findings        = @()
    Recommendations = @()
}

# Define search patterns for Microsoft.Extensions.Logging
$searchPatterns = @{
    UsingStatements   = @(
        "using Microsoft\.Extensions\.Logging",
        "using Microsoft\.Extensions\.DependencyInjection",
        "using Microsoft\.Extensions\.Hosting"
    )
    LoggingInterfaces = @(
        "ILogger<",
        "ILoggerFactory",
        "IServiceCollection"
    )
    LoggingMethods    = @(
        "\.LogDebug\(",
        "\.LogInformation\(",
        "\.LogWarning\(",
        "\.LogError\(",
        "\.LogCritical\(",
        "\.LogTrace\(",
        "LogLevel\."
    )
    PackageReferences = @(
        "Microsoft\.Extensions\.Logging",
        "Microsoft\.Extensions\.DependencyInjection",
        "Microsoft\.Extensions\.Hosting"
    )
}

# Define file extensions to scan
$fileExtensions = @("*.cs", "*.csproj", "*.json", "*.xaml", "*.ps1")

# Define directories to exclude
$excludePaths = @("bin", "obj", "packages", ".git", ".vs", "node_modules")
if ($ExcludeArchive) {
    $excludePaths += @("Archive", "Legacy", "Old-Configurations")
}

Write-Host "🔍 Starting Microsoft.Extensions.Logging scan..." -ForegroundColor Cyan
Write-Host "📁 Workspace: $((Resolve-Path $WorkspacePath).Path)" -ForegroundColor Yellow
Write-Host "📅 Timestamp: $($results.Summary.ScanTimestamp)" -ForegroundColor Yellow
Write-Host ""

# Function to check if path should be excluded
function Test-ExcludePath {
    param([string]$Path)

    foreach ($exclude in $excludePaths) {
        if ($Path -like "*\$exclude\*" -or $Path -like "*/$exclude/*") {
            return $true
        }
    }
    return $false
}

# Function to scan a single file
function Test-FileForMicrosoftLogging {
    param(
        [string]$FilePath,
        [hashtable]$Patterns
    )

    if (Test-ExcludePath $FilePath) {
        return $null
    }

    try {
        $content = Get-Content -Path $FilePath -Raw -ErrorAction Stop
        $fileFindings = @()

        foreach ($category in $Patterns.Keys) {
            foreach ($pattern in $Patterns[$category]) {
                $regexMatches = [regex]::Matches($content, $pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

                foreach ($match in $regexMatches) {
                    # Get line number
                    $lineNumber = ($content.Substring(0, $match.Index) -split "`n").Count

                    # Get context (line with match)
                    $lines = $content -split "`n"
                    $contextLine = if ($lineNumber -le $lines.Count) { $lines[$lineNumber - 1].Trim() } else { "" }

                    $fileFindings += @{
                        Category     = $category
                        Pattern      = $pattern
                        Match        = $match.Value
                        LineNumber   = $lineNumber
                        Context      = $contextLine
                        FilePath     = $FilePath
                        RelativePath = (Resolve-Path $FilePath -Relative)
                    }
                }
            }
        }

        return $fileFindings
    }
    catch {
        Write-Warning "❌ Error scanning file $FilePath`: $($_.Exception.Message)"
        return $null
    }
}

# Get all files to scan
Write-Host "📂 Collecting files to scan..." -ForegroundColor Cyan
$allFiles = @()

foreach ($extension in $fileExtensions) {
    $files = Get-ChildItem -Path $WorkspacePath -Filter $extension -Recurse -File |
    Where-Object { -not (Test-ExcludePath $_.FullName) }
    $allFiles += $files
}

$results.Summary.TotalFilesScanned = $allFiles.Count
Write-Host "📄 Found $($allFiles.Count) files to scan" -ForegroundColor Green
Write-Host ""

# Scan all files
Write-Host "🔍 Scanning files for Microsoft.Extensions.Logging references..." -ForegroundColor Cyan
$progressCounter = 0

foreach ($file in $allFiles) {
    $progressCounter++
    $percentComplete = [math]::Round(($progressCounter / $allFiles.Count) * 100, 1)

    Write-Progress -Activity "Scanning files" -Status "Processing $($file.Name)" -PercentComplete $percentComplete

    $fileFindings = Test-FileForMicrosoftLogging -FilePath $file.FullName -Patterns $searchPatterns

    if ($fileFindings -and $fileFindings.Count -gt 0) {
        $results.Findings += $fileFindings
        $results.Summary.FilesWithMicrosoftLogging++
        $results.Summary.TotalOccurrences += $fileFindings.Count
    }
}

Write-Progress -Activity "Scanning files" -Completed

# Generate recommendations
Write-Host "📝 Generating recommendations..." -ForegroundColor Cyan

if ($results.Summary.TotalOccurrences -eq 0) {
    $results.Recommendations += "✅ No Microsoft.Extensions.Logging references found - migration appears complete!"
    $results.Recommendations += "🎉 Serilog migration was successful"
}
else {
    $results.Recommendations += "⚠️ Found $($results.Summary.TotalOccurrences) Microsoft.Extensions.Logging references in $($results.Summary.FilesWithMicrosoftLogging) files"

    # Group findings by category
    $groupedFindings = $results.Findings | Group-Object Category

    foreach ($group in $groupedFindings) {
        switch ($group.Name) {
            "UsingStatements" {
                $results.Recommendations += "🔧 Remove unused Microsoft.Extensions.Logging using statements"
            }
            "LoggingInterfaces" {
                $results.Recommendations += "🔄 Replace Microsoft.Extensions.Logging interfaces with Serilog ILogger"
            }
            "LoggingMethods" {
                $results.Recommendations += "🔄 Convert Microsoft.Extensions.Logging method calls to Serilog syntax"
            }
            "PackageReferences" {
                $results.Recommendations += "📦 Remove Microsoft.Extensions.Logging package references from project files"
            }
        }
    }
}

# Output results
Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "🔍 MICROSOFT.EXTENSIONS.LOGGING SCAN RESULTS" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host ""

switch ($OutputFormat) {
    "Console" {
        # Summary
        Write-Host "📊 SUMMARY" -ForegroundColor Yellow
        Write-Host "Files Scanned: $($results.Summary.TotalFilesScanned)" -ForegroundColor White
        Write-Host "Files with Microsoft Logging: $($results.Summary.FilesWithMicrosoftLogging)" -ForegroundColor White
        Write-Host "Total Occurrences: $($results.Summary.TotalOccurrences)" -ForegroundColor White
        Write-Host ""

        # Findings
        if ($results.Findings.Count -gt 0) {
            Write-Host "🔍 DETAILED FINDINGS" -ForegroundColor Yellow

            $groupedByFile = $results.Findings | Group-Object FilePath

            foreach ($fileGroup in $groupedByFile) {
                $relativePath = (Resolve-Path $fileGroup.Name -Relative)
                Write-Host ""
                Write-Host "📄 $relativePath" -ForegroundColor Cyan

                foreach ($finding in $fileGroup.Group) {
                    $color = switch ($finding.Category) {
                        "UsingStatements" { "Yellow" }
                        "LoggingInterfaces" { "Red" }
                        "LoggingMethods" { "Red" }
                        "PackageReferences" { "Magenta" }
                        default { "White" }
                    }

                    Write-Host "  Line $($finding.LineNumber): [$($finding.Category)] $($finding.Match)" -ForegroundColor $color
                    Write-Host "    Context: $($finding.Context)" -ForegroundColor Gray
                }
            }
        }

        # Recommendations
        Write-Host ""
        Write-Host "💡 RECOMMENDATIONS" -ForegroundColor Yellow
        foreach ($rec in $results.Recommendations) {
            Write-Host $rec -ForegroundColor Green
        }
    }

    "JSON" {
        $jsonOutput = $results | ConvertTo-Json -Depth 10 -Compress:$false
        $outputFile = Join-Path $WorkspacePath "microsoft-logging-scan-results.json"
        $jsonOutput | Out-File -FilePath $outputFile -Encoding UTF8
        Write-Host "📄 Results saved to: $outputFile" -ForegroundColor Green
        Write-Host $jsonOutput
    }

    "CSV" {
        if ($results.Findings.Count -gt 0) {
            $csvData = $results.Findings | Select-Object Category, RelativePath, LineNumber, Match, Context
            $outputFile = Join-Path $WorkspacePath "microsoft-logging-scan-results.csv"
            $csvData | Export-Csv -Path $outputFile -NoTypeInformation -Encoding UTF8
            Write-Host "📄 Results saved to: $outputFile" -ForegroundColor Green
            $csvData | Format-Table -AutoSize
        }
        else {
            Write-Host "✅ No findings to export to CSV" -ForegroundColor Green
        }
    }
}

Write-Host ""
Write-Host "=" * 80 -ForegroundColor Cyan
Write-Host "🏁 Scan completed at $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Cyan
Write-Host "=" * 80 -ForegroundColor Cyan

# Exit with appropriate code
if ($results.Summary.TotalOccurrences -eq 0) {
    exit 0  # Success - no Microsoft logging found
}
else {
    exit 1  # Warning - Microsoft logging references still exist
}
