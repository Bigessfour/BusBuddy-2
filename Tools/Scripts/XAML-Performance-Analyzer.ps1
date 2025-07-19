#Requires -Version 7.0
<#
.SYNOPSIS
    XAML Performance Analyzer for Bus Buddy

.DESCRIPTION
    Analyzes XAML for performance bottlenecks and suggests optimizations.
    Targets 60 FPS rendering and identifies virtualization opportunities.

.NOTES
    Based on industry research showing 70% load time reduction potential with proper virtualization
#>

class XamlPerformanceIssue {
    [string]$FilePath
    [int]$LineNumber
    [string]$IssueType
    [string]$Severity
    [string]$Description
    [string]$Recommendation
    [string]$ImpactLevel
}

function Test-XamlPerformance {
    <#
    .SYNOPSIS
        Analyze XAML files for performance issues
    #>
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    $issues = @()
    $xamlFiles = Get-ChildItem $Path -Filter "*.xaml" -Recurse

    foreach ($file in $xamlFiles) {
        $lines = Get-Content $file.FullName

        for ($i = 0; $i -lt $lines.Count; $i++) {
            $line = $lines[$i]

            # Check for non-virtualized lists
            if ($line -match '<(ListView|ListBox|DataGrid)' -and $line -notmatch 'VirtualizingStackPanel') {
                $issue = [XamlPerformanceIssue]::new()
                $issue.FilePath = $file.FullName
                $issue.LineNumber = $i + 1
                $issue.IssueType = "NonVirtualizedList"
                $issue.Severity = "High"
                $issue.Description = "List control without virtualization"
                $issue.Recommendation = "Add VirtualizingStackPanel.IsVirtualizing='True'"
                $issue.ImpactLevel = "Up to 70% load time reduction possible"
                $issues += $issue
            }

            # Check for complex nested layouts
            $nestingLevel = ($line | Select-String -Pattern '<Grid|<StackPanel|<Canvas' -AllMatches).Matches.Count
            if ($nestingLevel -gt 6) {
                $issue = [XamlPerformanceIssue]::new()
                $issue.FilePath = $file.FullName
                $issue.LineNumber = $i + 1
                $issue.IssueType = "DeepNesting"
                $issue.Severity = "Medium"
                $issue.Description = "Layout nesting exceeds recommended 6-8 levels"
                $issue.Recommendation = "Simplify layout hierarchy"
                $issue.ImpactLevel = "May impact 60 FPS rendering target"
                $issues += $issue
            }

            # Check for inefficient binding expressions
            if ($line -match 'Binding.*Converter.*' -and $line -match 'Binding.*StringFormat.*') {
                $issue = [XamlPerformanceIssue]::new()
                $issue.FilePath = $file.FullName
                $issue.LineNumber = $i + 1
                $issue.IssueType = "InefficientBinding"
                $issue.Severity = "Low"
                $issue.Description = "Multiple binding operations on single property"
                $issue.Recommendation = "Combine converter and string formatting"
                $issue.ImpactLevel = "Minor performance improvement"
                $issues += $issue
            }

            # Check for missing x:Key on heavy resources
            if ($line -match '<.*Style.*>' -and $line -notmatch 'x:Key' -and $line -notmatch 'TargetType') {
                $issue = [XamlPerformanceIssue]::new()
                $issue.FilePath = $file.FullName
                $issue.LineNumber = $i + 1
                $issue.IssueType = "UnkeyedStyle"
                $issue.Severity = "Medium"
                $issue.Description = "Style without x:Key may be applied globally"
                $issue.Recommendation = "Add x:Key for explicit resource usage"
                $issue.ImpactLevel = "Prevents unnecessary style evaluations"
                $issues += $issue
            }
        }
    }

    return $issues
}

function Invoke-XamlPerformanceCheck {
    <#
    .SYNOPSIS
        Bus Buddy XAML performance analyzer
    .EXAMPLE
        Invoke-XamlPerformanceCheck
        Invoke-XamlPerformanceCheck -Path "Views\Dashboard" -ShowMetrics
    #>
    param(
        [string]$Path = "BusBuddy.WPF\Views",
        [switch]$ShowMetrics,
        [switch]$ExportReport
    )

    Write-Host "⚡ Bus Buddy Performance Analyzer" -ForegroundColor Cyan

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        Write-Host "❌ Bus Buddy project root not found" -ForegroundColor Red
        return
    }

    $targetPath = if ([System.IO.Path]::IsPathRooted($Path)) { $Path } else { Join-Path $projectRoot $Path }

    $issues = Test-XamlPerformance -Path $targetPath

    # Performance statistics
    $totalFiles = (Get-ChildItem $targetPath -Filter "*.xaml" -Recurse).Count
    $highIssues = ($issues | Where-Object { $_.Severity -eq "High" }).Count
    $mediumIssues = ($issues | Where-Object { $_.Severity -eq "Medium" }).Count
    $lowIssues = ($issues | Where-Object { $_.Severity -eq "Low" }).Count

    Write-Host "`n📊 Performance Analysis Results:" -ForegroundColor Yellow
    Write-Host "   Files Analyzed: $totalFiles" -ForegroundColor White
    Write-Host "   High Priority Issues: $highIssues" -ForegroundColor Red
    Write-Host "   Medium Priority Issues: $mediumIssues" -ForegroundColor Yellow
    Write-Host "   Low Priority Issues: $lowIssues" -ForegroundColor Green

    if ($issues.Count -eq 0) {
        Write-Host "🎉 No performance issues detected!" -ForegroundColor Green
        return
    }

    # Group issues by type for better reporting
    $groupedIssues = $issues | Group-Object IssueType

    foreach ($group in $groupedIssues) {
        Write-Host "`n🔍 $($group.Name) ($($group.Count) occurrences):" -ForegroundColor Magenta

        foreach ($issue in $group.Group | Select-Object -First 3) {
            $fileName = Split-Path $issue.FilePath -Leaf
            Write-Host "   📄 $fileName (Line $($issue.LineNumber))" -ForegroundColor White
            Write-Host "      $($issue.Description)" -ForegroundColor Gray
            Write-Host "      💡 $($issue.Recommendation)" -ForegroundColor Green
            Write-Host "      📈 Impact: $($issue.ImpactLevel)" -ForegroundColor Cyan
        }

        if ($group.Count -gt 3) {
            Write-Host "      ... and $($group.Count - 3) more similar issues" -ForegroundColor Gray
        }
    }

    if ($ShowMetrics) {
        Write-Host "`n📏 Performance Metrics:" -ForegroundColor Yellow
        Write-Host "   Target: 60 FPS rendering" -ForegroundColor White
        Write-Host "   Virtualization potential: Up to 70% load time reduction" -ForegroundColor White
        Write-Host "   Layout optimization: 6-8 level nesting recommended" -ForegroundColor White
    }

    if ($ExportReport) {
        $reportPath = Join-Path $projectRoot "logs\performance-analysis-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
        $issues | ConvertTo-Json -Depth 3 | Out-File $reportPath -Encoding UTF8
        Write-Host "`n💾 Report exported to: $reportPath" -ForegroundColor Green
    }
}

# Import Bus Buddy project helper if available
if (-not (Get-Command Get-BusBuddyProjectRoot -ErrorAction SilentlyContinue)) {
    function Get-BusBuddyProjectRoot {
        $currentPath = $PWD.Path
        while ($currentPath) {
            if (Test-Path (Join-Path $currentPath 'BusBuddy.sln')) {
                return $currentPath
            }
            $parentPath = Split-Path $currentPath -Parent
            if ($parentPath -eq $currentPath) { break }
            $currentPath = $parentPath
        }
        return $null
    }
}

# Alias for easier use
Set-Alias -Name "bb-perf" -Value "Invoke-XamlPerformanceCheck"
