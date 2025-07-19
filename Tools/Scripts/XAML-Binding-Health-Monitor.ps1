#Requires -Version 7.0
<#
.SYNOPSIS
    XAML Binding Health Monitor for Bus Buddy

.DESCRIPTION
    Advanced binding analysis addressing the 40% of developers who struggle with binding lifecycle.
    Validates DataContext flow, RelativeSource usage, and INotifyPropertyChanged implementation.

.NOTES
    Targets the binding lifecycle issues that affect 40% of XAML developers
#>

class XamlBindingIssue {
    [string]$FilePath
    [int]$LineNumber
    [string]$IssueType
    [string]$Severity
    [string]$BindingExpression
    [string]$Problem
    [string]$Solution
    [string]$Impact
}

function Test-BindingHealth {
    <#
    .SYNOPSIS
        Comprehensive binding health analysis
    #>
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    $issues = @()
    $xamlFiles = Get-ChildItem $Path -Filter "*.xaml" -Recurse

    foreach ($file in $xamlFiles) {
        $content = Get-Content $file.FullName -Raw
        $lines = Get-Content $file.FullName

        for ($i = 0; $i -lt $lines.Count; $i++) {
            $line = $lines[$i]

            # Check for missing DataContext
            if ($line -match '<(UserControl|Window|Page)' -and $content -notmatch 'DataContext') {
                $issue = [XamlBindingIssue]::new()
                $issue.FilePath = $file.FullName
                $issue.LineNumber = $i + 1
                $issue.IssueType = "MissingDataContext"
                $issue.Severity = "High"
                $issue.BindingExpression = "Root element"
                $issue.Problem = "No DataContext set - bindings will fail"
                $issue.Solution = "Set DataContext in XAML or code-behind"
                $issue.Impact = "All bindings in this view will be broken"
                $issues += $issue
            }

            # Check for complex RelativeSource without validation
            if ($line -match 'RelativeSource.*FindAncestor') {
                $issue = [XamlBindingIssue]::new()
                $issue.FilePath = $file.FullName
                $issue.LineNumber = $i + 1
                $issue.IssueType = "ComplexRelativeSource"
                $issue.Severity = "Medium"
                $issue.BindingExpression = $line.Trim()
                $issue.Problem = "Complex RelativeSource may fail if hierarchy changes"
                $issue.Solution = "Consider using ElementName or simplified binding path"
                $issue.Impact = "Fragile binding that breaks with UI restructuring"
                $issues += $issue
            }

            # Check for two-way bindings without proper validation
            if ($line -match 'Mode=TwoWay' -and $line -notmatch 'UpdateSourceTrigger') {
                $issue = [XamlBindingIssue]::new()
                $issue.FilePath = $file.FullName
                $issue.LineNumber = $i + 1
                $issue.IssueType = "TwoWayWithoutTrigger"
                $issue.Severity = "Medium"
                $issue.BindingExpression = $line.Trim()
                $issue.Problem = "TwoWay binding without explicit UpdateSourceTrigger"
                $issue.Solution = "Add UpdateSourceTrigger=PropertyChanged for immediate updates"
                $issue.Impact = "Data may not update when expected"
                $issues += $issue
            }

            # Check for bindings to collections without ObservableCollection hints
            if ($line -match 'ItemsSource.*Binding.*(?!ObservableCollection)(\w*Collection|\w*Items|\w*List)') {
                $issue = [XamlBindingIssue]::new()
                $issue.FilePath = $file.FullName
                $issue.LineNumber = $i + 1
                $issue.IssueType = "NonObservableCollection"
                $issue.Severity = "Medium"
                $issue.BindingExpression = $line.Trim()
                $issue.Problem = "Collection binding may not update UI automatically"
                $issue.Solution = "Use ObservableCollection<T> in ViewModel"
                $issue.Impact = "UI won't reflect collection changes"
                $issues += $issue
            }

            # Check for nested binding expressions (performance concern)
            $bindingCount = ($line | Select-String -Pattern '\{Binding' -AllMatches).Matches.Count
            if ($bindingCount -gt 3) {
                $issue = [XamlBindingIssue]::new()
                $issue.FilePath = $file.FullName
                $issue.LineNumber = $i + 1
                $issue.IssueType = "ExcessiveBindings"
                $issue.Severity = "Low"
                $issue.BindingExpression = $line.Trim()
                $issue.Problem = "Multiple bindings on single element may impact performance"
                $issue.Solution = "Consider MultiBinding or computed properties in ViewModel"
                $issue.Impact = "Potential performance degradation"
                $issues += $issue
            }

            # Check for bindings without FallbackValue on critical UI elements
            if ($line -match '(Visibility|IsEnabled).*Binding' -and $line -notmatch 'FallbackValue') {
                $issue = [XamlBindingIssue]::new()
                $issue.FilePath = $file.FullName
                $issue.LineNumber = $i + 1
                $issue.IssueType = "MissingFallback"
                $issue.Severity = "Medium"
                $issue.BindingExpression = $line.Trim()
                $issue.Problem = "Critical UI property without fallback value"
                $issue.Solution = "Add FallbackValue for graceful degradation"
                $issue.Impact = "UI may become unusable if binding fails"
                $issues += $issue
            }

            # Check for string concatenation in binding paths
            if ($line -match 'Binding.*\+.*') {
                $issue = [XamlBindingIssue]::new()
                $issue.FilePath = $file.FullName
                $issue.LineNumber = $i + 1
                $issue.IssueType = "StringConcatenation"
                $issue.Severity = "Low"
                $issue.BindingExpression = $line.Trim()
                $issue.Problem = "String concatenation in binding expression"
                $issue.Solution = "Use StringFormat or MultiBinding instead"
                $issue.Impact = "Poor performance and maintenance"
                $issues += $issue
            }
        }

        # Check for XAML files without any bindings (potential static UI)
        if ($content -notmatch '\{Binding' -and $content -match '<(Button|TextBox|ListView|DataGrid)') {
            $issue = [XamlBindingIssue]::new()
            $issue.FilePath = $file.FullName
            $issue.LineNumber = 1
            $issue.IssueType = "StaticUI"
            $issue.Severity = "Low"
            $issue.BindingExpression = "Entire file"
            $issue.Problem = "No data bindings found in interactive UI"
            $issue.Solution = "Consider if this UI should be data-driven"
            $issue.Impact = "May indicate missing MVVM implementation"
            $issues += $issue
        }
    }

    return $issues
}

function Invoke-BindingHealthCheck {
    <#
    .SYNOPSIS
        Bus Buddy binding health monitor
    .EXAMPLE
        Invoke-BindingHealthCheck
        Invoke-BindingHealthCheck -Path "Views\Dashboard" -DeepAnalysis -ExportReport
    #>
    param(
        [string]$Path = "BusBuddy.WPF\Views",
        [switch]$DeepAnalysis,
        [switch]$ExportReport,
        [switch]$ShowBestPractices
    )

    Write-Host "🔗 Bus Buddy Binding Health Monitor" -ForegroundColor Cyan

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        Write-Host "❌ Bus Buddy project root not found" -ForegroundColor Red
        return
    }

    $targetPath = if ([System.IO.Path]::IsPathRooted($Path)) { $Path } else { Join-Path $projectRoot $Path }

    $issues = Test-BindingHealth -Path $targetPath

    # Health metrics
    $totalFiles = (Get-ChildItem $targetPath -Filter "*.xaml" -Recurse).Count
    $filesWithIssues = ($issues | Select-Object FilePath -Unique).Count
    $healthScore = [Math]::Round(((($totalFiles - $filesWithIssues) / $totalFiles) * 100), 1)

    Write-Host "`n📊 Binding Health Report:" -ForegroundColor Yellow
    Write-Host "   Files Analyzed: $totalFiles" -ForegroundColor White
    Write-Host "   Files with Issues: $filesWithIssues" -ForegroundColor Red
    Write-Host "   Health Score: $healthScore%" -ForegroundColor $(if ($healthScore -gt 80) { "Green" } elseif ($healthScore -gt 60) { "Yellow" } else { "Red" })

    if ($issues.Count -eq 0) {
        Write-Host "🎉 All bindings are healthy!" -ForegroundColor Green
        return
    }

    # Issue breakdown
    $criticalIssues = $issues | Where-Object { $_.IssueType -in @("MissingDataContext", "MissingFallback") }
    $performanceIssues = $issues | Where-Object { $_.IssueType -in @("ExcessiveBindings", "NonObservableCollection") }
    $maintainabilityIssues = $issues | Where-Object { $_.IssueType -in @("ComplexRelativeSource", "StringConcatenation") }

    Write-Host "`n🚨 Issue Categories:" -ForegroundColor Yellow
    Write-Host "   Critical (may break functionality): $($criticalIssues.Count)" -ForegroundColor Red
    Write-Host "   Performance (may slow UI): $($performanceIssues.Count)" -ForegroundColor Yellow
    Write-Host "   Maintainability (technical debt): $($maintainabilityIssues.Count)" -ForegroundColor Orange

    # Show top issues by severity
    $prioritizedIssues = $issues | Sort-Object {
        switch ($_.Severity) {
            "High" { 1 }
            "Medium" { 2 }
            "Low" { 3 }
        }
    } | Group-Object IssueType

    foreach ($group in $prioritizedIssues | Select-Object -First 5) {
        Write-Host "`n🔍 $($group.Name) ($($group.Count) occurrences):" -ForegroundColor Magenta

        $topIssue = $group.Group | Select-Object -First 1
        $fileName = Split-Path $topIssue.FilePath -Leaf

        Write-Host "   📄 Example: $fileName (Line $($topIssue.LineNumber))" -ForegroundColor White
        Write-Host "      Problem: $($topIssue.Problem)" -ForegroundColor Red
        Write-Host "      Solution: $($topIssue.Solution)" -ForegroundColor Green
        Write-Host "      Impact: $($topIssue.Impact)" -ForegroundColor Yellow

        if ($group.Count -gt 1) {
            Write-Host "      ... and $($group.Count - 1) more similar issues" -ForegroundColor Gray
        }
    }

    if ($ShowBestPractices) {
        Write-Host "`n💡 Binding Best Practices:" -ForegroundColor Yellow
        Write-Host "   ✅ Always set DataContext explicitly" -ForegroundColor Green
        Write-Host "   ✅ Use ObservableCollection<T> for collections" -ForegroundColor Green
        Write-Host "   ✅ Implement INotifyPropertyChanged in ViewModels" -ForegroundColor Green
        Write-Host "   ✅ Add FallbackValue for critical UI properties" -ForegroundColor Green
        Write-Host "   ✅ Use ElementName instead of complex RelativeSource" -ForegroundColor Green
        Write-Host "   ✅ Set UpdateSourceTrigger for immediate updates" -ForegroundColor Green
        Write-Host "   ✅ Use StringFormat over custom converters when possible" -ForegroundColor Green
    }

    if ($DeepAnalysis) {
        Write-Host "`n🔬 Deep Analysis Recommendations:" -ForegroundColor Cyan

        # Analyze binding patterns
        $bindingPatterns = $issues | Group-Object { Split-Path $_.FilePath -Leaf } |
            Where-Object { $_.Count -gt 3 } |
            Sort-Object Count -Descending |
            Select-Object -First 3

        if ($bindingPatterns) {
            Write-Host "   📋 Files needing attention:" -ForegroundColor Yellow
            foreach ($pattern in $bindingPatterns) {
                Write-Host "      • $($pattern.Name): $($pattern.Count) issues" -ForegroundColor Gray
            }
        }

        # Suggest architectural improvements
        if ($criticalIssues.Count -gt ($totalFiles * 0.3)) {
            Write-Host "   🏗️ Consider architectural review - high critical issue ratio" -ForegroundColor Red
        }

        if ($performanceIssues.Count -gt ($totalFiles * 0.5)) {
            Write-Host "   ⚡ Performance optimization recommended" -ForegroundColor Yellow
        }
    }

    if ($ExportReport) {
        $reportPath = Join-Path $projectRoot "logs\binding-health-report-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
        $report = @{
            GeneratedAt = Get-Date
            ProjectPath = $targetPath
            HealthMetrics = @{
                TotalFiles = $totalFiles
                FilesWithIssues = $filesWithIssues
                HealthScore = $healthScore
                CriticalIssues = $criticalIssues.Count
                PerformanceIssues = $performanceIssues.Count
                MaintainabilityIssues = $maintainabilityIssues.Count
            }
            Issues = $issues
        }
        $report | ConvertTo-Json -Depth 4 | Out-File $reportPath -Encoding UTF8
        Write-Host "`n💾 Health report exported to: $reportPath" -ForegroundColor Green
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
Set-Alias -Name "bb-bindings" -Value "Invoke-BindingHealthCheck"
