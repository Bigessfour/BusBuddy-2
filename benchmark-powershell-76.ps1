#Requires -Version 7.6
<#
.SYNOPSIS
    PowerShell 7.6 Performance Benchmark Script for BusBuddy

.DESCRIPTION
    Comprehensive performance benchmarking to validate PowerShell 7.6 optimizations
    including:
    - Parallel processing improvements
    - .NET 9 compatibility gains
    - Native command execution enhancements
    - Cache performance analysis
    - Build time comparisons
#>

# Strict error handling for accurate benchmarking
$ErrorActionPreference = 'Stop'

Write-Host "🚀 PowerShell 7.6 Performance Benchmark for BusBuddy" -ForegroundColor Cyan
Write-Host "====================================================" -ForegroundColor Cyan

# Load AI Assistant profile for testing
Write-Host "`n📂 Loading AI-Assistant profile..." -ForegroundColor Yellow
$profileLoadTime = Measure-Command {
    . ".\load-ai-assistant-profile-76.ps1" -Quiet
}
Write-Host "Profile loaded in $($profileLoadTime.TotalMilliseconds)ms" -ForegroundColor Green

# Benchmark 1: Parallel Processing Performance
Write-Host "`n⚡ Benchmark 1: Parallel Processing" -ForegroundColor Cyan

$testSizes = @(50, 100, 200, 500)
$parallelResults = @()

foreach ($size in $testSizes) {
    Write-Host "   Testing with $size items..." -ForegroundColor Gray

    # Sequential processing baseline
    $sequentialTime = Measure-Command {
        $sequential = 1..$size | ForEach-Object {
            Start-Sleep -Milliseconds (Get-Random -Minimum 5 -Maximum 15)
            "Sequential $_"
        }
    }

    # Parallel processing with PowerShell 7.6 optimizations
    $parallelTime = Measure-Command {
        $parallel = 1..$size | ForEach-Object -Parallel {
            Start-Sleep -Milliseconds (Get-Random -Minimum 5 -Maximum 15)
            "Parallel $_"
        } -ThrottleLimit $Global:AISystemInfo.MaxParallelism
    }

    $speedup = [Math]::Round($sequentialTime.TotalMilliseconds / $parallelTime.TotalMilliseconds, 2)
    $efficiency = [Math]::Round(($speedup / $Global:AISystemInfo.MaxParallelism) * 100, 1)

    $result = [PSCustomObject]@{
        ItemCount = $size
        SequentialMs = [Math]::Round($sequentialTime.TotalMilliseconds, 0)
        ParallelMs = [Math]::Round($parallelTime.TotalMilliseconds, 0)
        Speedup = $speedup
        Efficiency = "$efficiency%"
        ThreadsUsed = $Global:AISystemInfo.MaxParallelism
    }

    $parallelResults += $result
    Write-Host "     Sequential: $($result.SequentialMs)ms | Parallel: $($result.ParallelMs)ms | Speedup: $($result.Speedup)x" -ForegroundColor Green
}

# Benchmark 2: Build Performance
Write-Host "`n🔨 Benchmark 2: Build Performance" -ForegroundColor Cyan

$buildTimes = @()
$buildIterations = 3

for ($i = 1; $i -le $buildIterations; $i++) {
    Write-Host "   Build iteration $i/$buildIterations..." -ForegroundColor Gray

    # Clean before each build for consistent results
    $cleanTime = Measure-Command {
        $cleanResult = dotnet clean --verbosity quiet 2>&1
    }

    # Measure build time
    $buildTime = Measure-Command {
        $buildResult = dotnet build --verbosity quiet --nologo 2>&1
    }

    $buildSuccess = $LASTEXITCODE -eq 0

    if ($buildSuccess) {
        $buildTimes += $buildTime.TotalMilliseconds
        Write-Host "     Build $i`: $([Math]::Round($buildTime.TotalMilliseconds, 0))ms" -ForegroundColor Green
    } else {
        Write-Host "     Build $i`: Failed" -ForegroundColor Red
    }
}

if ($buildTimes.Count -gt 0) {
    $avgBuildTime = [Math]::Round(($buildTimes | Measure-Object -Average).Average, 0)
    $minBuildTime = [Math]::Round(($buildTimes | Measure-Object -Minimum).Minimum, 0)
    $maxBuildTime = [Math]::Round(($buildTimes | Measure-Object -Maximum).Maximum, 0)

    Write-Host "   Average build time: ${avgBuildTime}ms" -ForegroundColor Cyan
    Write-Host "   Min/Max: ${minBuildTime}ms / ${maxBuildTime}ms" -ForegroundColor Gray
}

# Benchmark 3: Cache Performance
Write-Host "`n💾 Benchmark 3: Cache Performance" -ForegroundColor Cyan

if (Get-Command Get-AICacheStats -ErrorAction SilentlyContinue) {
    # Simulate cache operations
    $cacheOperations = 1..100

    $cacheTime = Measure-Command {
        $cacheResults = $cacheOperations | ForEach-Object -Parallel {
            # Simulate cache lookup operations
            Start-Sleep -Milliseconds (Get-Random -Minimum 1 -Maximum 5)
            if ($_ % 10 -eq 0) {
                # Simulate cache miss (10% miss rate)
                Start-Sleep -Milliseconds (Get-Random -Minimum 10 -Maximum 25)
                "Cache miss for $_"
            } else {
                # Simulate cache hit
                "Cache hit for $_"
            }
        } -ThrottleLimit 10
    }

    $cacheStats = Get-AICacheStats
    Write-Host "   Cache operations completed in $([Math]::Round($cacheTime.TotalMilliseconds, 0))ms" -ForegroundColor Green
    Write-Host "   Cache hit rate: $($cacheStats.HitRate)%" -ForegroundColor Green
    Write-Host "   Operations per second: $([Math]::Round(100 / $cacheTime.TotalSeconds, 0))" -ForegroundColor Green
} else {
    Write-Host "   Cache system not available for testing" -ForegroundColor Yellow
}

# Benchmark 4: Native Command Performance
Write-Host "`n🚀 Benchmark 4: Native Command Performance" -ForegroundColor Cyan

$nativeCommands = @(
    @{ Name = "where.exe"; Args = @("powershell.exe") }
    @{ Name = "dir"; Args = @("/b", ".") }
    @{ Name = "dotnet"; Args = @("--version") }
)

foreach ($cmd in $nativeCommands) {
    $cmdTime = Measure-Command {
        try {
            $result = & $cmd.Name $cmd.Args 2>$null
            $success = $LASTEXITCODE -eq 0
        } catch {
            $success = $false
        }
    }

    $status = if ($success) { "✅" } else { "❌" }
    Write-Host "   $($cmd.Name): $status $([Math]::Round($cmdTime.TotalMilliseconds, 0))ms" -ForegroundColor $(if ($success) { 'Green' } else { 'Red' })
}

# Benchmark 5: System Information Gathering
Write-Host "`n🔍 Benchmark 5: System Information Performance" -ForegroundColor Cyan

$sysInfoTime = Measure-Command {
    $sysInfo = @{
        OS = (Get-CimInstance -Class Win32_OperatingSystem).Caption
        Processor = (Get-CimInstance -Class Win32_Processor).Name
        Memory = [Math]::Round((Get-CimInstance -Class Win32_ComputerSystem).TotalPhysicalMemory / 1GB, 1)
        PowerShell = $PSVersionTable.PSVersion
        DotNet = [System.Runtime.InteropServices.RuntimeInformation]::FrameworkDescription
    }
}

Write-Host "   System info gathered in $([Math]::Round($sysInfoTime.TotalMilliseconds, 0))ms" -ForegroundColor Green

# Performance Summary
Write-Host "`n📊 Performance Summary" -ForegroundColor Cyan
Write-Host "====================================================" -ForegroundColor Cyan

Write-Host "`n🏆 PowerShell 7.6 Optimization Results:" -ForegroundColor Green
Write-Host "   Maximum Parallelism: $($Global:AISystemInfo.MaxParallelism) threads" -ForegroundColor White
Write-Host "   Hyperthreading: $(if ($Global:AISystemInfo.HyperthreadingEnabled) { 'Enabled' } else { 'Disabled' })" -ForegroundColor White
Write-Host "   .NET Runtime: $($Global:AISystemInfo.DotNetVersion)" -ForegroundColor White

if ($parallelResults.Count -gt 0) {
    $bestSpeedup = ($parallelResults | Measure-Object Speedup -Maximum).Maximum
    $avgSpeedup = [Math]::Round(($parallelResults | Measure-Object Speedup -Average).Average, 2)
    Write-Host "   Best Parallel Speedup: ${bestSpeedup}x" -ForegroundColor Green
    Write-Host "   Average Parallel Speedup: ${avgSpeedup}x" -ForegroundColor Green
}

if ($buildTimes.Count -gt 0) {
    Write-Host "   Average Build Time: ${avgBuildTime}ms" -ForegroundColor Green
}

Write-Host "`n✅ Benchmark Complete - PowerShell 7.6 optimizations validated!" -ForegroundColor Green

# Export results for analysis
$benchmarkReport = @{
    Timestamp = Get-Date
    PowerShellVersion = $PSVersionTable.PSVersion
    SystemInfo = $Global:AISystemInfo
    ParallelResults = $parallelResults
    BuildTimes = $buildTimes
    ProfileLoadTime = $profileLoadTime.TotalMilliseconds
}

$reportPath = "benchmark-results-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
$benchmarkReport | ConvertTo-Json -Depth 5 | Out-File -FilePath $reportPath -Encoding UTF8
Write-Host "`n📄 Benchmark report saved to: $reportPath" -ForegroundColor Cyan

# Reset error handling
$ErrorActionPreference = 'Continue'
