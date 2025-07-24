#Requires -Version 7.6
<#
.SYNOPSIS
    PowerShell 7.6.4 and .NET 9 Compatibility Validation Test

.DESCRIPTION
    Comprehensive testing script for Dell Inspiron 16 5640 PowerShell 7.6.4 optimizations:
    - PowerShell 7.6 feature validation
    - .NET 9 compatibility testing
    - Native command execution improvements
    - Enhanced parallel processing validation
    - EF Core function testing
    - Performance benchmarking

.NOTES
    Must run after PowerShell 7.6 optimizations have been applied
    Validates all PhD recommendations implementation
#>

param(
    [switch]$QuickTest,
    [switch]$FullBenchmark,
    [switch]$ValidateOptimizations,
    [switch]$TestNativeCommands,
    [switch]$TestEFCore,
    [switch]$ShowResults
)

# =============================================================================
# TEST ENVIRONMENT SETUP
# =============================================================================

$Global:TestResults = @{
    StartTime = Get-Date
    PowerShellVersion = $PSVersionTable.PSVersion
    DotNetVersion = [System.Runtime.InteropServices.RuntimeInformation]::FrameworkDescription
    TestsRun = @()
    PassedTests = 0
    FailedTests = 0
    BenchmarkResults = @{}
    Optimizations = @{}
}

Write-Host "🧪 PowerShell 7.6.4 & .NET 9 Compatibility Test Suite" -ForegroundColor Cyan
Write-Host "=" * 60 -ForegroundColor Cyan
Write-Host "PowerShell: $($Global:TestResults.PowerShellVersion)" -ForegroundColor White
Write-Host ".NET: $($Global:TestResults.DotNetVersion)" -ForegroundColor White
Write-Host "Test Started: $($Global:TestResults.StartTime)" -ForegroundColor Gray
Write-Host ""

function Write-TestResult {
    param(
        [string]$TestName,
        [bool]$Passed,
        [string]$Details = "",
        [hashtable]$Data = @{}
    )

    $status = if ($Passed) { "✅ PASS" } else { "❌ FAIL" }
    $color = if ($Passed) { "Green" } else { "Red" }

    Write-Host "   $status $TestName" -ForegroundColor $color
    if ($Details) {
        Write-Host "       $Details" -ForegroundColor Gray
    }

    $Global:TestResults.TestsRun += @{
        Name = $TestName
        Passed = $Passed
        Details = $Details
        Data = $Data
        Timestamp = Get-Date
    }

    if ($Passed) { $Global:TestResults.PassedTests++ } else { $Global:TestResults.FailedTests++ }
}

# =============================================================================
# POWERSHELL 7.6 FEATURE VALIDATION
# =============================================================================

function Test-PowerShell76Features {
    Write-Host "🔍 Testing PowerShell 7.6 Features..." -ForegroundColor Cyan

    # Test PSNativeCommandArgumentPassing
    try {
        $currentSetting = $PSNativeCommandArgumentPassing
        $passed = $currentSetting -eq 'Standard'
        Write-TestResult "PSNativeCommandArgumentPassing Configuration" $passed "Set to: $currentSetting"
    }
    catch {
        Write-TestResult "PSNativeCommandArgumentPassing Configuration" $false "Error: $($_.Message)"
    }

    # Test Microsoft.PowerShell.ThreadJob module
    try {
        $threadJobModule = Get-Module Microsoft.PowerShell.ThreadJob -ListAvailable
        $passed = $threadJobModule -ne $null
        $version = if ($threadJobModule) { $threadJobModule.Version } else { "Not Found" }
        Write-TestResult "Microsoft.PowerShell.ThreadJob Module" $passed "Version: $version"
    }
    catch {
        Write-TestResult "Microsoft.PowerShell.ThreadJob Module" $false "Error: $($_.Message)"
    }

    # Test Enhanced Parallel Processing
    try {
        $start = Get-Date
        $results = 1..10 | ForEach-Object -Parallel {
            Start-Sleep -Milliseconds 50
            $_ * 2
        } -ThrottleLimit 8 -TimeoutSeconds 30
        $duration = (Get-Date) - $start

        $passed = $results.Count -eq 10 -and $duration.TotalSeconds -lt 2
        Write-TestResult "Enhanced Parallel Processing" $passed "Duration: $([math]::Round($duration.TotalMilliseconds, 0))ms"
    }
    catch {
        Write-TestResult "Enhanced Parallel Processing" $false "Error: $($_.Message)"
    }

    # Test Pipeline Cancellation (PowerShell 7.6 feature)
    try {
        function Test-Cancellation {
            [CmdletBinding()]
            param()

            return $PSCmdlet.PipelineStopToken -ne $null
        }

        $passed = Test-Cancellation
        Write-TestResult "Pipeline Cancellation Support" $passed "PipelineStopToken available"
    }
    catch {
        Write-TestResult "Pipeline Cancellation Support" $false "Error: $($_.Message)"
    }
}

# =============================================================================
# .NET 9 COMPATIBILITY TESTING
# =============================================================================

function Test-DotNet9Compatibility {
    Write-Host "🎯 Testing .NET 9 Compatibility..." -ForegroundColor Cyan

    # Test .NET Version Detection
    try {
        $dotnetVersion = [System.Runtime.InteropServices.RuntimeInformation]::FrameworkDescription
        $isNet9 = $dotnetVersion -match '^Microsoft \.NET 9'
        $isNet8Plus = $dotnetVersion -match '^Microsoft \.NET [89]'

        Write-TestResult ".NET 9 Detection" $isNet9 ".NET Version: $dotnetVersion"
        Write-TestResult ".NET 8+ Compatibility" $isNet8Plus "Compatible: $isNet8Plus"
    }
    catch {
        Write-TestResult ".NET Version Detection" $false "Error: $($_.Message)"
    }

    # Test Enhanced Memory Management
    try {
        $beforeGC = [System.GC]::GetTotalMemory($false)

        # Create and process a large dataset
        $data = 1..1000 | ForEach-Object {
            [PSCustomObject]@{ Id = $_; Value = "Test$_" * 10 }
        }

        $processed = $data | ForEach-Object -Parallel {
            $_.Value.Length
        } -ThrottleLimit 8

        [System.GC]::Collect()
        $afterGC = [System.GC]::GetTotalMemory($true)

        $passed = $processed.Count -eq 1000
        Write-TestResult "Enhanced Memory Management" $passed "Processed 1000 items, Memory delta: $([math]::Round(($afterGC - $beforeGC) / 1MB, 2)) MB"
    }
    catch {
        Write-TestResult "Enhanced Memory Management" $false "Error: $($_.Message)"
    }

    # Test JSON Serialization Improvements
    try {
        $testObject = @{
            Numbers = 1..100
            Strings = @("Test1", "Test2", "Test3")
            DateTime = Get-Date
        }

        $start = Get-Date
        $json = ConvertTo-Json $testObject -Depth 10 -Compress
        $roundTrip = ConvertFrom-Json $json
        $duration = (Get-Date) - $start

        $passed = $roundTrip.Numbers.Count -eq 100
        Write-TestResult "JSON Serialization Performance" $passed "Duration: $([math]::Round($duration.TotalMilliseconds, 0))ms"
    }
    catch {
        Write-TestResult "JSON Serialization Performance" $false "Error: $($_.Message)"
    }
}

# =============================================================================
# NATIVE COMMAND EXECUTION TESTING
# =============================================================================

function Test-NativeCommandExecution {
    Write-Host "⚡ Testing Native Command Execution..." -ForegroundColor Cyan

    # Test Enhanced dotnet Command Execution
    try {
        if (Get-Command Invoke-EnhancedDotNetCommand -ErrorAction SilentlyContinue) {
            $start = Get-Date
            $result = Invoke-EnhancedDotNetCommand -Arguments @('--version')
            $duration = (Get-Date) - $start

            $passed = $result.Success -and $result.Output
            Write-TestResult "Invoke-EnhancedDotNetCommand" $passed "Duration: $([math]::Round($duration.TotalMilliseconds, 0))ms, Output lines: $($result.Output.Count)"
        }
        else {
            Write-TestResult "Invoke-EnhancedDotNetCommand" $false "Function not found - load AI-Assistant profile first"
        }
    }
    catch {
        Write-TestResult "Invoke-EnhancedDotNetCommand" $false "Error: $($_.Message)"
    }

    # Test Standard dotnet Command with PSNativeCommandArgumentPassing
    try {
        $start = Get-Date
        $output = dotnet --version 2>&1
        $duration = (Get-Date) - $start

        $passed = $LASTEXITCODE -eq 0 -and $output
        Write-TestResult "Standard dotnet Command" $passed "Duration: $([math]::Round($duration.TotalMilliseconds, 0))ms, Exit Code: $LASTEXITCODE"
    }
    catch {
        Write-TestResult "Standard dotnet Command" $false "Error: $($_.Message)"
    }

    # Test Start-Process Enhanced Execution
    try {
        $start = Get-Date
        $tempOutput = Join-Path $env:TEMP "ps76-test-$(Get-Date -Format 'HHmmss').txt"

        $proc = Start-Process -FilePath 'dotnet' -ArgumentList @('--version') -NoNewWindow -Wait -PassThru -RedirectStandardOutput $tempOutput
        $output = Get-Content $tempOutput -ErrorAction SilentlyContinue
        $duration = (Get-Date) - $start

        Remove-Item $tempOutput -ErrorAction SilentlyContinue

        $passed = $proc.ExitCode -eq 0 -and $output
        Write-TestResult "Enhanced Start-Process" $passed "Duration: $([math]::Round($duration.TotalMilliseconds, 0))ms, Exit Code: $($proc.ExitCode)"
    }
    catch {
        Write-TestResult "Enhanced Start-Process" $false "Error: $($_.Message)"
    }
}

# =============================================================================
# EF CORE FUNCTION TESTING
# =============================================================================

function Test-EFCoreFunctions {
    Write-Host "🗄️ Testing EF Core Functions..." -ForegroundColor Cyan

    # Test EF Core Status Function
    try {
        if (Get-Command Get-EFCoreStatus -ErrorAction SilentlyContinue) {
            $start = Get-Date
            $status = Get-EFCoreStatus -ProjectPath "." -ErrorAction SilentlyContinue
            $duration = (Get-Date) - $start

            $passed = $status -ne $null
            Write-TestResult "Get-EFCoreStatus Function" $passed "Duration: $([math]::Round($duration.TotalMilliseconds, 0))ms"
        }
        else {
            Write-TestResult "Get-EFCoreStatus Function" $false "Function not found - load AI-Assistant profile first"
        }
    }
    catch {
        Write-TestResult "Get-EFCoreStatus Function" $false "Error: $($_.Message)"
    }

    # Test EF Core Info Function
    try {
        if (Get-Command Get-EFCoreDbInfo -ErrorAction SilentlyContinue) {
            $start = Get-Date
            $info = Get-EFCoreDbInfo -ProjectPath "." -ErrorAction SilentlyContinue
            $duration = (Get-Date) - $start

            $passed = $info -ne $null
            Write-TestResult "Get-EFCoreDbInfo Function" $passed "Duration: $([math]::Round($duration.TotalMilliseconds, 0))ms"
        }
        else {
            Write-TestResult "Get-EFCoreDbInfo Function" $false "Function not found - load AI-Assistant profile first"
        }
    }
    catch {
        Write-TestResult "Get-EFCoreDbInfo Function" $false "Error: $($_.Message)"
    }
}

# =============================================================================
# PERFORMANCE BENCHMARKING
# =============================================================================

function Test-PerformanceBenchmarks {
    Write-Host "🏁 Running Performance Benchmarks..." -ForegroundColor Cyan

    # Benchmark: Parallel Processing with Different Throttle Limits
    try {
        $workItems = 1..100

        # Test with optimal throttle limit (12 for Dell Inspiron)
        $start = Get-Date
        $results1 = $workItems | ForEach-Object -Parallel {
            [System.Math]::Sqrt($_ * $_)
        } -ThrottleLimit 12
        $duration1 = (Get-Date) - $start

        # Test with conservative throttle limit
        $start = Get-Date
        $results2 = $workItems | ForEach-Object -Parallel {
            [System.Math]::Sqrt($_ * $_)
        } -ThrottleLimit 4
        $duration2 = (Get-Date) - $start

        $improvement = [math]::Round((($duration2.TotalMilliseconds - $duration1.TotalMilliseconds) / $duration2.TotalMilliseconds) * 100, 1)
        $passed = $duration1.TotalMilliseconds -lt $duration2.TotalMilliseconds

        Write-TestResult "Parallel Processing Optimization" $passed "12-thread: $([math]::Round($duration1.TotalMilliseconds, 0))ms vs 4-thread: $([math]::Round($duration2.TotalMilliseconds, 0))ms ($improvement% improvement)"

        $Global:TestResults.BenchmarkResults["ParallelProcessing"] = @{
            OptimalTime = $duration1.TotalMilliseconds
            ConservativeTime = $duration2.TotalMilliseconds
            ImprovementPercent = $improvement
        }
    }
    catch {
        Write-TestResult "Parallel Processing Optimization" $false "Error: $($_.Message)"
    }

    # Benchmark: Memory Operations
    try {
        $start = Get-Date
        $data = 1..10000 | ForEach-Object {
            [PSCustomObject]@{
                Id = $_
                Name = "Item$_"
                Value = $_ * 2
                Timestamp = Get-Date
            }
        }
        $creationTime = (Get-Date) - $start

        $start = Get-Date
        $filtered = $data | Where-Object { $_.Id -gt 5000 }
        $filterTime = (Get-Date) - $start

        $passed = $filtered.Count -eq 5000 -and $creationTime.TotalSeconds -lt 2
        Write-TestResult "Memory Operations Performance" $passed "Creation: $([math]::Round($creationTime.TotalMilliseconds, 0))ms, Filter: $([math]::Round($filterTime.TotalMilliseconds, 0))ms"

        $Global:TestResults.BenchmarkResults["MemoryOperations"] = @{
            CreationTime = $creationTime.TotalMilliseconds
            FilterTime = $filterTime.TotalMilliseconds
            ItemsProcessed = $data.Count
        }
    }
    catch {
        Write-TestResult "Memory Operations Performance" $false "Error: $($_.Message)"
    }

    # Benchmark: PowerShell Startup Time
    try {
        $measurements = 1..3 | ForEach-Object {
            $start = Get-Date
            $null = powershell -NoProfile -Command "exit"
            $duration = (Get-Date) - $start
            $duration.TotalMilliseconds
        }

        $avgStartup = ($measurements | Measure-Object -Average).Average
        $passed = $avgStartup -lt 2000  # Less than 2 seconds

        Write-TestResult "PowerShell Startup Performance" $passed "Average: $([math]::Round($avgStartup, 0))ms (3 runs)"

        $Global:TestResults.BenchmarkResults["StartupTime"] = @{
            AverageTime = $avgStartup
            Measurements = $measurements
        }
    }
    catch {
        Write-TestResult "PowerShell Startup Performance" $false "Error: $($_.Message)"
    }
}

# =============================================================================
# OPTIMIZATION VALIDATION
# =============================================================================

function Test-OptimizationValidation {
    Write-Host "🔧 Validating Applied Optimizations..." -ForegroundColor Cyan

    # Check for optimized profile
    try {
        $profileExists = Test-Path $PROFILE.CurrentUserAllHosts
        $profileContent = if ($profileExists) { Get-Content $PROFILE.CurrentUserAllHosts -Raw } else { "" }

        $hasOptimizations = $profileContent -match "Dell.*Inspiron.*Config" -or $profileContent -match "MaxParallelism"
        Write-TestResult "Optimized Profile Present" $hasOptimizations "Profile exists: $profileExists, Has optimizations: $hasOptimizations"
    }
    catch {
        Write-TestResult "Optimized Profile Present" $false "Error: $($_.Message)"
    }

    # Check system configuration
    try {
        $powerPlan = powercfg /getactivescheme
        $isHighPerf = $powerPlan -match "High performance" -or $powerPlan -match "Ultimate"
        Write-TestResult "High Performance Power Plan" $isHighPerf "Current plan detected"
    }
    catch {
        Write-TestResult "High Performance Power Plan" $false "Error: $($_.Message)"
    }

    # Check hyperthreading detection
    try {
        if (Get-Variable AISystemInfo -Scope Global -ErrorAction SilentlyContinue) {
            $htEnabled = $Global:AISystemInfo.HyperthreadingEnabled
            $maxParallel = $Global:AISystemInfo.MaxParallelism
            Write-TestResult "Hyperthreading Detection" $htEnabled "Max Parallelism: $maxParallel"
        }
        else {
            Write-TestResult "Hyperthreading Detection" $false "AISystemInfo not found - load AI-Assistant profile first"
        }
    }
    catch {
        Write-TestResult "Hyperthreading Detection" $false "Error: $($_.Message)"
    }
}

# =============================================================================
# MAIN EXECUTION
# =============================================================================

# Run tests based on parameters
if ($QuickTest -or (-not $FullBenchmark -and -not $ValidateOptimizations -and -not $TestNativeCommands -and -not $TestEFCore)) {
    Test-PowerShell76Features
    Test-DotNet9Compatibility
}

if ($TestNativeCommands -or $FullBenchmark) {
    Test-NativeCommandExecution
}

if ($TestEFCore -or $FullBenchmark) {
    Test-EFCoreFunctions
}

if ($ValidateOptimizations -or $FullBenchmark) {
    Test-OptimizationValidation
}

if ($FullBenchmark) {
    Test-PowerShell76Features
    Test-DotNet9Compatibility
    Test-NativeCommandExecution
    Test-EFCoreFunctions
    Test-OptimizationValidation
    Test-PerformanceBenchmarks
}

# Generate final report
$Global:TestResults.EndTime = Get-Date
$Global:TestResults.TotalDuration = $Global:TestResults.EndTime - $Global:TestResults.StartTime

Write-Host "`n📊 Test Results Summary:" -ForegroundColor Cyan
Write-Host "=" * 40 -ForegroundColor Cyan
Write-Host "Total Tests: $($Global:TestResults.TestsRun.Count)" -ForegroundColor White
Write-Host "Passed: $($Global:TestResults.PassedTests)" -ForegroundColor Green
Write-Host "Failed: $($Global:TestResults.FailedTests)" -ForegroundColor $(if ($Global:TestResults.FailedTests -eq 0) { 'Green' } else { 'Red' })
Write-Host "Success Rate: $([math]::Round(($Global:TestResults.PassedTests / $Global:TestResults.TestsRun.Count) * 100, 1))%" -ForegroundColor White
Write-Host "Duration: $([math]::Round($Global:TestResults.TotalDuration.TotalSeconds, 1)) seconds" -ForegroundColor Gray

if ($ShowResults -or $FailedTests -gt 0) {
    Write-Host "`n📋 Detailed Results:" -ForegroundColor Yellow
    foreach ($test in $Global:TestResults.TestsRun) {
        $status = if ($test.Passed) { "✅" } else { "❌" }
        Write-Host "   $status $($test.Name)" -ForegroundColor $(if ($test.Passed) { 'Green' } else { 'Red' })
        if ($test.Details) {
            Write-Host "      $($test.Details)" -ForegroundColor Gray
        }
    }
}

if ($Global:TestResults.BenchmarkResults.Count -gt 0) {
    Write-Host "`n🏁 Benchmark Results:" -ForegroundColor Cyan
    foreach ($benchmark in $Global:TestResults.BenchmarkResults.GetEnumerator()) {
        Write-Host "   $($benchmark.Key):" -ForegroundColor White
        foreach ($metric in $benchmark.Value.GetEnumerator()) {
            Write-Host "      $($metric.Key): $($metric.Value)" -ForegroundColor Gray
        }
    }
}

# Final recommendations
Write-Host "`n💡 Recommendations:" -ForegroundColor Yellow
if ($Global:TestResults.FailedTests -eq 0) {
    Write-Host "   ✅ All tests passed! PowerShell 7.6.4 optimizations are working correctly." -ForegroundColor Green
    Write-Host "   🚀 System is optimized for AI-Assistant development workflow." -ForegroundColor Green
}
else {
    Write-Host "   ⚠️ Some tests failed. Review the detailed results above." -ForegroundColor Yellow
    Write-Host "   🔧 Consider re-running optimizations or checking system configuration." -ForegroundColor Yellow
}

Write-Host "   📖 Use 'Test-DellPerformance' for ongoing system monitoring." -ForegroundColor White

return $Global:TestResults
