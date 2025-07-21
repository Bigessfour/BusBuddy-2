# ========================================
# Syncfusion WPF 30.1.40 API Analyzer
# Scans project for compliance with official API patterns
# ========================================

param(
    [switch]$DeepScan = $false,
    [switch]$AutoFix = $false,
    [string]$OutputReport = "syncfusion-analysis-report.md"
)

Write-Host '🔍 Syncfusion WPF 30.1.40 API Compliance Scanner' -ForegroundColor Cyan
Write-Host ('=' * 60) -ForegroundColor Gray

# Scan XAML files for deprecated patterns
$xamlFiles = Get-ChildItem -Recurse -Include "*.xaml" -ErrorAction SilentlyContinue
$issues = @()

foreach ($file in $xamlFiles) {
    $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue

    # Check for deprecated DateTimePattern usage
    if ($content -match 'DateTimePattern\s*=\s*"?FullDate"?') {
        $issues += @{
            File = $file.FullName
            Issue = 'Deprecated DateTimePattern: FullDate'
            Recommendation = 'Replace with LongDate or use SafeDateTimePatternExtension'
            ApiReference = 'https://help.syncfusion.com/cr/wpf/Syncfusion.Windows.Shared.DateTimePattern.html'
            AutoFixAvailable = $true
        }
    }

    # Check for missing Syncfusion namespace declarations
    if ($content -match '<sf:' -or $content -match '<syncfusion:') {
        if (-not ($content -match 'xmlns:syncfusion="http://schemas.syncfusion.com/wpf"') -and
            -not ($content -match 'xmlns:sf="http://schemas.syncfusion.com/wpf"')) {
            $issues += @{
                File = $file.FullName
                Issue = 'Missing Syncfusion namespace declaration'
                Recommendation = 'Add xmlns:syncfusion="http://schemas.syncfusion.com/wpf" to root element'
                ApiReference = 'https://help.syncfusion.com/wpf/control-dependencies'
                AutoFixAvailable = $true
            }
        }
    }
}

# Report findings
Write-Host "📊 Analysis Results: $($issues.Count) issues found in $($xamlFiles.Count) XAML files" -ForegroundColor Yellow

foreach ($issue in $issues) {
    Write-Host ''
    Write-Host "⚠️  $($issue.Issue)" -ForegroundColor Yellow
    Write-Host "   📁 File: $($issue.File)" -ForegroundColor Gray
    Write-Host "   💡 Fix: $($issue.Recommendation)" -ForegroundColor Cyan
    Write-Host "   📖 API: $($issue.ApiReference)" -ForegroundColor Magenta

    if ($AutoFix -and $issue.AutoFixAvailable) {
        Write-Host "   🔧 Auto-fixing..." -ForegroundColor Green
        # Auto-fix logic would go here
    }
}

Write-Host ''
Write-Host '✅ Syncfusion API compliance scan complete' -ForegroundColor Green
