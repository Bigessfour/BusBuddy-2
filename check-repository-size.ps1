#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Repository Size Optimization and Monitoring Script

.DESCRIPTION
    Analyzes repository size, identifies large files, cleans up logs,
    and provides optimization recommendations for BusBuddy repository.

.PARAMETER CleanLogs
    Clean up log files older than specified days (default: 7)

.PARAMETER MaxLogAge
    Maximum age in days for log files to keep (default: 7)

.PARAMETER AnalyzeOnly
    Only analyze size without making changes

.PARAMETER MaxRepoSizeMB
    Maximum acceptable repository size in MB (default: 300)

.EXAMPLE
    .\check-repository-size.ps1

.EXAMPLE
    .\check-repository-size.ps1 -CleanLogs -MaxLogAge 3

.EXAMPLE
    .\check-repository-size.ps1 -AnalyzeOnly
#>

param(
    [switch]$CleanLogs,
    [int]$MaxLogAge = 7,
    [switch]$AnalyzeOnly,
    [int]$MaxRepoSizeMB = 300,
    [switch]$Verbose
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Color output functions
function Write-Success { param($Message) Write-Host "✅ $Message" -ForegroundColor Green }
function Write-Warning { param($Message) Write-Host "⚠️ $Message" -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host "❌ $Message" -ForegroundColor Red }
function Write-Info { param($Message) Write-Host "ℹ️ $Message" -ForegroundColor Cyan }
function Write-Header { param($Message) Write-Host "`n🎯 $Message" -ForegroundColor Magenta }

# Helper function to format file sizes
function Format-FileSize {
    param([long]$Bytes)

    if ($Bytes -ge 1GB) {
        return "{0:N2} GB" -f ($Bytes / 1GB)
    }
    elseif ($Bytes -ge 1MB) {
        return "{0:N2} MB" -f ($Bytes / 1MB)
    }
    elseif ($Bytes -ge 1KB) {
        return "{0:N2} KB" -f ($Bytes / 1KB)
    }
    else {
        return "$Bytes bytes"
    }
}

# Main analysis function
function Invoke-RepositorySizeAnalysis {
    param($MaxRepoSizeMB)

    Write-Header "Repository Size Analysis"

    # Get total repository size
    Write-Info "Calculating total repository size..."
    $allFiles = Get-ChildItem -Recurse -File | Where-Object {
        $_.FullName -notlike "*\.git\*"
    }

    $totalSize = ($allFiles | Measure-Object -Property Length -Sum).Sum
    $totalSizeMB = [math]::Round($totalSize / 1MB, 2)

    Write-Info "📊 Total Repository Size: $(Format-FileSize $totalSize) ($totalSizeMB MB)"

    # Size status
    if ($totalSizeMB -gt $MaxRepoSizeMB) {
        Write-Error "Repository size ($totalSizeMB MB) exceeds recommended limit ($MaxRepoSizeMB MB)"
        $needsOptimization = $true
    }
    elseif ($totalSizeMB -gt ($MaxRepoSizeMB * 0.8)) {
        Write-Warning "Repository size ($totalSizeMB MB) approaching limit ($MaxRepoSizeMB MB)"
        $needsOptimization = $true
    }
    else {
        Write-Success "Repository size within acceptable limits"
        $needsOptimization = $false
    }

    return @{
        TotalSize         = $totalSize
        TotalSizeMB       = $totalSizeMB
        NeedsOptimization = $needsOptimization
        AllFiles          = $allFiles
    }
}

# Large files analysis
function Invoke-LargeFilesAnalysis {
    param($AllFiles)

    Write-Header "Large Files Analysis"

    # Find files larger than 10MB
    $largeFiles = $AllFiles | Where-Object { $_.Length -gt 10MB } |
    Sort-Object Length -Descending

    if ($largeFiles) {
        Write-Warning "Found $(@($largeFiles).Count) files larger than 10MB:"
        $largeFiles | ForEach-Object {
            $relativePath = $_.FullName.Replace($PWD, ".")
            Write-Host "  📁 $relativePath - $(Format-FileSize $_.Length)" -ForegroundColor Yellow
        }
    }
    else {
        Write-Success "No files larger than 10MB found"
    }

    # Find files larger than 5MB
    $mediumFiles = $AllFiles | Where-Object { $_.Length -gt 5MB -and $_.Length -le 10MB } |
    Sort-Object Length -Descending | Select-Object -First 10

    if ($mediumFiles) {
        Write-Info "Largest medium files (5-10MB):"
        $mediumFiles | ForEach-Object {
            $relativePath = $_.FullName.Replace($PWD, ".")
            Write-Host "  📄 $relativePath - $(Format-FileSize $_.Length)" -ForegroundColor Cyan
        }
    }

    return $largeFiles
}

# Directory size analysis
function Invoke-DirectorySizeAnalysis {
    Write-Header "Directory Size Analysis"

    $directories = Get-ChildItem -Directory | Where-Object {
        $_.Name -ne ".git"
    }

    $dirSizes = @()
    foreach ($dir in $directories) {
        $dirFiles = Get-ChildItem -Path $dir.FullName -Recurse -File -ErrorAction SilentlyContinue
        if ($dirFiles) {
            $dirSize = ($dirFiles | Measure-Object -Property Length -Sum).Sum
            $dirSizes += [PSCustomObject]@{
                Directory = $dir.Name
                Size      = $dirSize
                SizeMB    = [math]::Round($dirSize / 1MB, 2)
                FileCount = $dirFiles.Count
            }
        }
    }

    $dirSizes = $dirSizes | Sort-Object Size -Descending

    Write-Info "Directory sizes:"
    $dirSizes | ForEach-Object {
        $color = if ($_.SizeMB -gt 100) { "Red" } elseif ($_.SizeMB -gt 50) { "Yellow" } else { "Green" }
        Write-Host "  📁 $($_.Directory): $(Format-FileSize $_.Size) ($($_.FileCount) files)" -ForegroundColor $color
    }

    return $dirSizes
}

# Logs cleanup function
function Invoke-LogsCleanup {
    param([int]$MaxAge, [switch]$AnalyzeOnly, [switch]$Verbose)

    Write-Header "Logs Cleanup"

    if (-not (Test-Path "logs")) {
        Write-Info "No logs directory found"
        return
    }

    $cutoffDate = (Get-Date).AddDays(-$MaxAge)
    $logFiles = Get-ChildItem -Path "logs" -Recurse -File

    if (-not $logFiles) {
        Write-Info "No log files found"
        return
    }

    $oldFiles = $logFiles | Where-Object { $_.LastWriteTime -lt $cutoffDate }
    $totalLogSize = ($logFiles | Measure-Object -Property Length -Sum).Sum

    Write-Info "📊 Total logs size: $(Format-FileSize $totalLogSize)"
    Write-Info "📅 Found $($logFiles.Count) log files, $($oldFiles.Count) older than $MaxAge days"

    if ($oldFiles) {
        $oldFilesSize = ($oldFiles | Measure-Object -Property Length -Sum).Sum
        Write-Info "🗑️ Old files to clean: $(Format-FileSize $oldFilesSize)"

        if (-not $AnalyzeOnly) {
            $oldFiles | ForEach-Object {
                if ($Verbose) {
                    Write-Host "  Removing: $($_.Name)" -ForegroundColor DarkGray
                }
                Remove-Item $_.FullName -Force
            }
            Write-Success "Cleaned $($oldFiles.Count) old log files ($(Format-FileSize $oldFilesSize) freed)"
        }
        else {
            Write-Info "Would clean $($oldFiles.Count) old log files ($(Format-FileSize $oldFilesSize))"
        }
    }
    else {
        Write-Success "No old log files to clean"
    }
}

# Optimization recommendations
function Get-OptimizationRecommendations {
    param($Analysis, $LargeFiles, $DirSizes)

    Write-Header "Optimization Recommendations"

    $recommendations = @()

    # Large files recommendations
    if ($LargeFiles) {
        $recommendations += "🗂️ Consider moving large files to Git LFS or external storage"
        $recommendations += "📦 Archive large binary files to separate repository"
    }

    # Logs recommendations
    $logsDir = $DirSizes | Where-Object { $_.Directory -eq "logs" }
    if ($logsDir -and $logsDir.SizeMB -gt 50) {
        $recommendations += "📝 Schedule regular log cleanup (current: $(Format-FileSize $logsDir.Size))"
        $recommendations += "⚙️ Add log rotation to prevent unlimited growth"
    }

    # Binary directories
    $binObjSize = ($DirSizes | Where-Object { $_.Directory -in @("bin", "obj", "packages") } |
        Measure-Object -Property Size -Sum).Sum
    if ($binObjSize -gt 0) {
        $recommendations += "🧹 Add bin/obj/packages to .gitignore if not already present"
    }

    # Node modules (if any)
    $nodeModules = $DirSizes | Where-Object { $_.Directory -eq "node_modules" }
    if ($nodeModules) {
        $recommendations += "📦 Consider adding node_modules to .gitignore"
    }

    # General recommendations if repo is large
    if ($Analysis.TotalSizeMB -gt $MaxRepoSizeMB) {
        $recommendations += "🎯 Run this script weekly to monitor size growth"
        $recommendations += "📊 Consider splitting large features into separate repositories"
        $recommendations += "🗄️ Archive historical documentation to separate branch"
    }

    if ($recommendations) {
        Write-Warning "Optimization recommendations:"
        $recommendations | ForEach-Object {
            Write-Host "  $_" -ForegroundColor Yellow
        }
    }
    else {
        Write-Success "No optimization needed at this time"
    }
}

# Git recommendations
function Get-GitOptimizationRecommendations {
    Write-Header "Git Optimization Recommendations"

    # Check .gitignore
    if (Test-Path ".gitignore") {
        $gitignore = Get-Content ".gitignore" -Raw

        $recommendedPatterns = @(
            "logs/*.log",
            "logs/*.txt",
            "**/bin/",
            "**/obj/",
            "*.tmp",
            "*.temp",
            "node_modules/",
            "packages/",
            "TestResults/"
        )

        $missing = @()
        foreach ($pattern in $recommendedPatterns) {
            if ($gitignore -notmatch [regex]::Escape($pattern.Replace("*", ""))) {
                $missing += $pattern
            }
        }

        if ($missing) {
            Write-Warning "Consider adding these patterns to .gitignore:"
            $missing | ForEach-Object {
                Write-Host "  $_" -ForegroundColor Yellow
            }
        }
        else {
            Write-Success "Git ignore patterns look good"
        }
    }
    else {
        Write-Warning "No .gitignore file found - consider creating one"
    }

    # Git repository optimization commands
    Write-Info "Git optimization commands (run manually if needed):"
    Write-Host "  git gc --aggressive" -ForegroundColor Cyan
    Write-Host "  git prune" -ForegroundColor Cyan
    Write-Host "  git repack -ad" -ForegroundColor Cyan
}

# Main execution
function Main {
    Write-Host "`n🎯 BusBuddy Repository Size Optimization Tool" -ForegroundColor Magenta
    Write-Host "📅 $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray

    try {
        # Analyze repository size
        $analysis = Invoke-RepositorySizeAnalysis -MaxRepoSizeMB $MaxRepoSizeMB

        # Analyze large files
        $largeFiles = Invoke-LargeFilesAnalysis -AllFiles $analysis.AllFiles

        # Analyze directory sizes
        $dirSizes = Invoke-DirectorySizeAnalysis

        # Clean logs if requested
        if ($CleanLogs) {
            Invoke-LogsCleanup -MaxAge $MaxLogAge -AnalyzeOnly:$AnalyzeOnly -Verbose:$Verbose
        }

        # Provide optimization recommendations
        Get-OptimizationRecommendations -Analysis $analysis -LargeFiles $largeFiles -DirSizes $dirSizes

        # Git optimization recommendations
        Get-GitOptimizationRecommendations

        Write-Header "Summary"
        Write-Info "📊 Repository size: $(Format-FileSize $analysis.TotalSize)"
        Write-Info "📁 Large files (>10MB): $($largeFiles.Count)"
        Write-Info "🎯 Optimization needed: $(if ($analysis.NeedsOptimization) { 'Yes' } else { 'No' })"

        if ($AnalyzeOnly) {
            Write-Info "Analysis mode - no changes made"
        }

        Write-Success "Repository size analysis completed!"

    }
    catch {
        Write-Error "Failed to analyze repository: $($_.Exception.Message)"
        exit 1
    }
}

# Run main function
Main
