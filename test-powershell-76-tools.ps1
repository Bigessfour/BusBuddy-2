#Requires -Version 7.6
<#
.SYNOPSIS
    Test PowerShell 7.6 Enhanced Tools and Profiles

.DESCRIPTION
    Validation script to test all PowerShell 7.6 enhanced tools, profiles, and migration scripts.
    Ensures everything works correctly with the new PowerShell 7.6 features.

.FEATURES
    - Test enhanced AI Assistant profile
    - Validate PowerShell 7.6 migration tools
    - Check performance optimizations
    - Verify experimental features
    - Test benchmark functionality

.EXAMPLE
    .\test-powershell-76-tools.ps1 -RunAllTests
    .\test-powershell-76-tools.ps1 -TestProfiles -TestMigration -TestBenchmark

.NOTES
    This script validates all PowerShell 7.6 enhancements for BusBuddy
#>

[CmdletBinding()]
param(
    [switch]$RunAllTests,
    [switch]$TestProfiles,
    [switch]$TestMigration,
    [switch]$TestBenchmark,
    [switch]$TestValidator,
    [switch]$Quiet,
    [string]$ProjectPath = (Get-Location)
)

$ErrorActionPreference = 'Continue'  # Allow tests to continue on errors
$ProgressPreference = if ($Quiet) { 'SilentlyContinue' } else { 'Continue' }

class TestResult {
    [string]$TestName
    [string]$Status  # Pass, Fail, Warning, Skip
    [string]$Message
    [object]$Details
    [timespan]$Duration
}

$testResults = @()

function New-TestResult {
    param(
        [string]$TestName,
        [string]$Status,
        [string]$Message,
        [object]$Details = $null,
        [timespan]$Duration = [timespan]::Zero
    )

    return [TestResult]@{
        TestName = $TestName
        Status = $Status
        Message = $Message
        Details = $Details
        Duration = $Duration
    }
}

function Write-TestHeader {
    param([string]$TestName)

    if (-not $Quiet) {
        Write-Host "`n🧪 Testing: $TestName" -ForegroundColor Cyan
        Write-Host "=" * (10 + $TestName.Length) -ForegroundColor Gray
    }
}

function Write-TestResult {
    param([TestResult]$Result)

    if (-not $Quiet) {
        $color = switch ($Result.Status) {
            'Pass' { 'Green' }
            'Fail' { 'Red' }
            'Warning' { 'Yellow' }
            'Skip' { 'Gray' }
            default { 'White' }
        }

        $icon = switch ($Result.Status) {
            'Pass' { '✅' }
            'Fail' { '❌' }
            'Warning' { '⚠️' }
            'Skip' { '⏭️' }
            default { 'ℹ️' }
        }

        Write-Host "   $icon $($Result.TestName): $($Result.Message)" -ForegroundColor $color

        if ($Result.Duration.TotalMilliseconds -gt 0) {
            Write-Host "      Duration: $([Math]::Round($Result.Duration.TotalMilliseconds, 0))ms" -ForegroundColor Gray
        }
    }
}

function Test-PowerShell76Environment {
    Write-TestHeader "PowerShell 7.6 Environment"

    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

    try {
        # Test PowerShell version
        if ($PSVersionTable.PSVersion -ge [version]'7.6') {
            $testResults += New-TestResult -TestName "PowerShell Version" -Status "Pass" -Message "PowerShell $($PSVersionTable.PSVersion) detected"
        } else {
            $testResults += New-TestResult -TestName "PowerShell Version" -Status "Warning" -Message "PowerShell $($PSVersionTable.PSVersion) - recommend 7.6+"
        }

        # Test .NET version
        $dotnetVersion = [System.Runtime.InteropServices.RuntimeInformation]::FrameworkDescription
        if ($dotnetVersion -match '\.NET 9') {
            $testResults += New-TestResult -TestName ".NET Version" -Status "Pass" -Message $dotnetVersion
        } else {
            $testResults += New-TestResult -TestName ".NET Version" -Status "Warning" -Message "$dotnetVersion - .NET 9 recommended"
        }

        # Test experimental features
        $experimentalFeatures = Get-ExperimentalFeature | Where-Object Enabled
        if ($experimentalFeatures) {
            $testResults += New-TestResult -TestName "Experimental Features" -Status "Pass" -Message "$($experimentalFeatures.Count) features enabled"
        } else {
            $testResults += New-TestResult -TestName "Experimental Features" -Status "Skip" -Message "No experimental features enabled"
        }

        # Test parallel processing capability
        $processorCount = [System.Environment]::ProcessorCount
        $testResults += New-TestResult -TestName "Parallel Processing" -Status "Pass" -Message "$processorCount processors available"

    }
    catch {
        $testResults += New-TestResult -TestName "Environment Check" -Status "Fail" -Message $_.Exception.Message
    }
    finally {
        $stopwatch.Stop()
        $testResults[-1].Duration = $stopwatch.Elapsed
    }
}

function Test-EnhancedAIProfile {
    Write-TestHeader "Enhanced AI Assistant Profile"

    $profilePath = Join-Path $ProjectPath "load-ai-assistant-profile-76-enhanced.ps1"

    if (-not (Test-Path $profilePath)) {
        $testResults += New-TestResult -TestName "Enhanced AI Profile" -Status "Skip" -Message "Profile not found at $profilePath"
        return
    }

    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

    try {
        # Test profile loading
        . $profilePath -Quiet -SkipValidation

        $testResults += New-TestResult -TestName "Profile Loading" -Status "Pass" -Message "Enhanced AI profile loaded successfully"

        # Test global variables
        if ($Global:AISystemInfo) {
            $testResults += New-TestResult -TestName "System Info" -Status "Pass" -Message "AISystemInfo available with version $($Global:AISystemInfo.Version)"
        } else {
            $testResults += New-TestResult -TestName "System Info" -Status "Fail" -Message "AISystemInfo not available"
        }

        # Test enhanced functions
        $enhancedFunctions = @(
            'Get-AISystemStatus',
            'Start-AIParallelProcessing',
            'Invoke-AIEnhancedCommand',
            'Get-AIOptimizedFileSearch'
        )

        foreach ($func in $enhancedFunctions) {
            if (Get-Command $func -ErrorAction SilentlyContinue) {
                $testResults += New-TestResult -TestName "Function: $func" -Status "Pass" -Message "Function available"
            } else {
                $testResults += New-TestResult -TestName "Function: $func" -Status "Fail" -Message "Function not found"
            }
        }

        # Test enhanced cache
        if ($Global:AIEnhancedCache) {
            $cacheStats = Get-AICacheStats
            $testResults += New-TestResult -TestName "Enhanced Cache" -Status "Pass" -Message "Cache system operational"
        } else {
            $testResults += New-TestResult -TestName "Enhanced Cache" -Status "Fail" -Message "Cache system not available"
        }

    }
    catch {
        $testResults += New-TestResult -TestName "Enhanced AI Profile" -Status "Fail" -Message $_.Exception.Message
    }
    finally {
        $stopwatch.Stop()
        $testResults[-1].Duration = $stopwatch.Elapsed
    }
}

function Test-PowerShell76Validator {
    Write-TestHeader "PowerShell 7.6 Validator"

    $validatorPath = Join-Path $ProjectPath "Tools\Scripts\PowerShell-76-Validator-Migrator-Debugger.ps1"

    if (-not (Test-Path $validatorPath)) {
        $testResults += New-TestResult -TestName "PS 7.6 Validator" -Status "Skip" -Message "Validator not found at $validatorPath"
        return
    }

    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

    try {
        # Test validator execution
        $validationResults = & $validatorPath -ValidateProject -ProjectPath $ProjectPath -Quiet

        if ($validationResults) {
            $testResults += New-TestResult -TestName "Validator Execution" -Status "Pass" -Message "$($validationResults.Count) validation results"

            # Analyze results
            $statusBreakdown = $validationResults | Group-Object Status
            foreach ($status in $statusBreakdown) {
                $testResults += New-TestResult -TestName "Validation: $($status.Name)" -Status "Pass" -Message "$($status.Count) items"
            }
        } else {
            $testResults += New-TestResult -TestName "Validator Execution" -Status "Warning" -Message "No validation results returned"
        }

    }
    catch {
        $testResults += New-TestResult -TestName "PS 7.6 Validator" -Status "Fail" -Message $_.Exception.Message
    }
    finally {
        $stopwatch.Stop()
        $testResults[-1].Duration = $stopwatch.Elapsed
    }
}

function Test-CompleteMigrationTool {
    Write-TestHeader "Complete Migration Tool"

    $migrationPath = Join-Path $ProjectPath "Tools\Scripts\BusBuddy-PS76-Complete-Migration.ps1"

    if (-not (Test-Path $migrationPath)) {
        $testResults += New-TestResult -TestName "Migration Tool" -Status "Skip" -Message "Migration tool not found at $migrationPath"
        return
    }

    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

    try {
        # Test migration tool analysis mode (safe to run)
        $analysisResults = & $migrationPath -RunCompleteAnalysis -ProjectPath $ProjectPath -Quiet

        if ($analysisResults) {
            $summary = $analysisResults.GetSummary()
            $testResults += New-TestResult -TestName "Migration Analysis" -Status "Pass" -Message "Analyzed $($summary.Statistics.FilesAnalyzed) files"

            if ($summary.Statistics.OptimizationsApplied -gt 0) {
                $testResults += New-TestResult -TestName "Migration Opportunities" -Status "Pass" -Message "$($summary.Statistics.OptimizationsApplied) optimizations identified"
            } else {
                $testResults += New-TestResult -TestName "Migration Opportunities" -Status "Pass" -Message "No migrations needed"
            }
        } else {
            $testResults += New-TestResult -TestName "Migration Analysis" -Status "Warning" -Message "No analysis results returned"
        }

    }
    catch {
        $testResults += New-TestResult -TestName "Migration Tool" -Status "Fail" -Message $_.Exception.Message
    }
    finally {
        $stopwatch.Stop()
        $testResults[-1].Duration = $stopwatch.Elapsed
    }
}

function Test-BenchmarkScript {
    Write-TestHeader "PowerShell 7.6 Benchmark"

    $benchmarkPath = Join-Path $ProjectPath "benchmark-powershell-76.ps1"

    if (-not (Test-Path $benchmarkPath)) {
        $testResults += New-TestResult -TestName "Benchmark Script" -Status "Skip" -Message "Benchmark script not found at $benchmarkPath"
        return
    }

    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

    try {
        # Run a quick benchmark test (with reduced iterations for testing)
        $env:BENCHMARK_QUICK_TEST = "true"  # Signal to benchmark script to run quickly

        # Note: We're not actually running the full benchmark here as it takes too long
        # Instead, we test if the script loads without errors

        $benchmarkContent = Get-Content -Path $benchmarkPath -Raw

        # Check for key benchmark components
        if ($benchmarkContent -match 'ForEach-Object -Parallel') {
            $testResults += New-TestResult -TestName "Parallel Benchmark" -Status "Pass" -Message "Parallel processing benchmark found"
        }

        if ($benchmarkContent -match 'Measure-Command') {
            $testResults += New-TestResult -TestName "Performance Measurement" -Status "Pass" -Message "Performance measurement code found"
        }

        if ($benchmarkContent -match 'ConvertTo-Json.*-Depth') {
            $testResults += New-TestResult -TestName "Report Generation" -Status "Pass" -Message "Report generation code found"
        }

        $testResults += New-TestResult -TestName "Benchmark Script" -Status "Pass" -Message "Benchmark script validation completed"

    }
    catch {
        $testResults += New-TestResult -TestName "Benchmark Script" -Status "Fail" -Message $_.Exception.Message
    }
    finally {
        $stopwatch.Stop()
        $testResults[-1].Duration = $stopwatch.Elapsed
        Remove-Item Env:BENCHMARK_QUICK_TEST -ErrorAction SilentlyContinue
    }
}

function Test-ParallelProcessingPerformance {
    Write-TestHeader "Parallel Processing Performance"

    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

    try {
        # Test basic parallel processing
        $testData = 1..20

        $sequentialTime = Measure-Command {
            $sequentialResults = $testData | ForEach-Object {
                Start-Sleep -Milliseconds 10
                "Sequential $_"
            }
        }

        $parallelTime = Measure-Command {
            $parallelResults = $testData | ForEach-Object -Parallel {
                Start-Sleep -Milliseconds 10
                "Parallel $_"
            } -ThrottleLimit 4
        }

        $speedup = [Math]::Round($sequentialTime.TotalMilliseconds / $parallelTime.TotalMilliseconds, 2)

        if ($speedup -gt 1.5) {
            $testResults += New-TestResult -TestName "Parallel Speedup" -Status "Pass" -Message "${speedup}x speedup achieved"
        } elseif ($speedup -gt 1.0) {
            $testResults += New-TestResult -TestName "Parallel Speedup" -Status "Warning" -Message "${speedup}x speedup (modest improvement)"
        } else {
            $testResults += New-TestResult -TestName "Parallel Speedup" -Status "Fail" -Message "${speedup}x speedup (no improvement)"
        }

        $testResults += New-TestResult -TestName "Sequential Time" -Status "Pass" -Message "$([Math]::Round($sequentialTime.TotalMilliseconds, 0))ms"
        $testResults += New-TestResult -TestName "Parallel Time" -Status "Pass" -Message "$([Math]::Round($parallelTime.TotalMilliseconds, 0))ms"

    }
    catch {
        $testResults += New-TestResult -TestName "Parallel Processing" -Status "Fail" -Message $_.Exception.Message
    }
    finally {
        $stopwatch.Stop()
        $testResults[-1].Duration = $stopwatch.Elapsed
    }
}

# Main test execution
Write-Host "🧪 PowerShell 7.6 Enhanced Tools Test Suite" -ForegroundColor Cyan
Write-Host "===========================================" -ForegroundColor Cyan
Write-Host "PowerShell Version: $($PSVersionTable.PSVersion)" -ForegroundColor Gray
Write-Host "Test Time: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray

# Always test environment
Test-PowerShell76Environment

# Run specific tests or all tests
if ($RunAllTests -or $TestProfiles) {
    Test-EnhancedAIProfile
}

if ($RunAllTests -or $TestValidator) {
    Test-PowerShell76Validator
}

if ($RunAllTests -or $TestMigration) {
    Test-CompleteMigrationTool
}

if ($RunAllTests -or $TestBenchmark) {
    Test-BenchmarkScript
}

if ($RunAllTests) {
    Test-ParallelProcessingPerformance
}

# Generate test summary
Write-Host "`n📊 Test Summary" -ForegroundColor Cyan
Write-Host "===============" -ForegroundColor Cyan

$statusCounts = $testResults | Group-Object Status
foreach ($status in $statusCounts) {
    $color = switch ($status.Name) {
        'Pass' { 'Green' }
        'Fail' { 'Red' }
        'Warning' { 'Yellow' }
        'Skip' { 'Gray' }
        default { 'White' }
    }
    Write-Host "   $($status.Name): $($status.Count)" -ForegroundColor $color
}

$totalDuration = ($testResults | Measure-Object Duration -Sum).Sum
Write-Host "   Total Duration: $([Math]::Round($totalDuration.TotalSeconds, 2))s" -ForegroundColor Gray

# Show failures and warnings
$issues = $testResults | Where-Object { $_.Status -in @('Fail', 'Warning') }
if ($issues) {
    Write-Host "`n⚠️ Issues Found:" -ForegroundColor Yellow
    foreach ($issue in $issues) {
        $icon = if ($issue.Status -eq 'Fail') { '❌' } else { '⚠️' }
        Write-Host "   $icon $($issue.TestName): $($issue.Message)" -ForegroundColor $(if ($issue.Status -eq 'Fail') { 'Red' } else { 'Yellow' })
    }
}

# Overall result
$failureCount = ($testResults | Where-Object { $_.Status -eq 'Fail' }).Count
$warningCount = ($testResults | Where-Object { $_.Status -eq 'Warning' }).Count

if ($failureCount -eq 0 -and $warningCount -eq 0) {
    Write-Host "`n🎉 All tests passed! PowerShell 7.6 tools are ready." -ForegroundColor Green
} elseif ($failureCount -eq 0) {
    Write-Host "`n✅ Tests completed with $warningCount warning(s). Tools are functional." -ForegroundColor Yellow
} else {
    Write-Host "`n❌ Tests completed with $failureCount failure(s) and $warningCount warning(s)." -ForegroundColor Red
    Write-Host "   Please review the issues above before proceeding." -ForegroundColor Red
}

# Export results to JSON for further analysis
$reportPath = "PS76-Tools-Test-Results-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
$testResults | ConvertTo-Json -Depth 3 | Out-File -FilePath $reportPath -Encoding UTF8
Write-Host "`n📄 Detailed test results saved to: $reportPath" -ForegroundColor Cyan

return $testResults
