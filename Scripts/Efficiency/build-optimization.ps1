# ═══════════════════════════════════════════════════════════════════════════════
# 🔧 BUILD PERFORMANCE OPTIMIZATION SCRIPT
# ═══════════════════════════════════════════════════════════════════════════════
# Optimizes build performance through smart caching, parallel execution,
# and incremental build strategies aligned with Phase 1 priorities.

param(
    [switch]$EnableParallel,
    [switch]$OptimizeCache,
    [switch]$AnalyzeBottlenecks,
    [switch]$ApplyOptimizations
)

# Performance targets (aligned with PerformanceOptimizer.cs)
$TARGET_BUILD_TIME_SECONDS = 30
$TARGET_RESTORE_TIME_SECONDS = 10
$TARGET_CLEAN_TIME_SECONDS = 5

function Write-OptimizationLog {
    param($Message, $Level = "INFO")
    $timestamp = Get-Date -Format "HH:mm:ss"
    $color = switch ($Level) {
        "SUCCESS" { "Green" }
        "WARN" { "Yellow" }
        "ERROR" { "Red" }
        default { "Cyan" }
    }
    Write-Host "[$timestamp] $Message" -ForegroundColor $color
}

function Measure-BuildPerformance {
    <#
    .SYNOPSIS
    Measures current build performance to establish baseline
    #>
    Write-OptimizationLog "📊 Measuring current build performance..." "INFO"

    $results = @{}

    # Clean build measurement
    $cleanStart = Get-Date
    $cleanResult = dotnet clean BusBuddy.sln --verbosity minimal 2>&1
    $cleanTime = (Get-Date) - $cleanStart
    $results.CleanTimeSeconds = $cleanTime.TotalSeconds

    # Restore measurement
    $restoreStart = Get-Date
    $restoreResult = dotnet restore BusBuddy.sln --verbosity minimal 2>&1
    $restoreTime = (Get-Date) - $restoreStart
    $results.RestoreTimeSeconds = $restoreTime.TotalSeconds

    # Build measurement
    $buildStart = Get-Date
    $buildResult = dotnet build BusBuddy.sln --no-restore --verbosity minimal 2>&1
    $buildTime = (Get-Date) - $buildStart
    $results.BuildTimeSeconds = $buildTime.TotalSeconds

    $results.TotalTimeSeconds = $cleanTime.TotalSeconds + $restoreTime.TotalSeconds + $buildTime.TotalSeconds

    Write-OptimizationLog "⏱️ Performance Results:" "INFO"
    Write-OptimizationLog "   Clean: $($results.CleanTimeSeconds.ToString('F2'))s" "INFO"
    Write-OptimizationLog "   Restore: $($results.RestoreTimeSeconds.ToString('F2'))s" "INFO"
    Write-OptimizationLog "   Build: $($results.BuildTimeSeconds.ToString('F2'))s" "INFO"
    Write-OptimizationLog "   Total: $($results.TotalTimeSeconds.ToString('F2'))s" "INFO"

    return $results
}

function Optimize-BuildCache {
    <#
    .SYNOPSIS
    Optimizes NuGet cache and build output caching
    #>
    Write-OptimizationLog "🗄️ Optimizing build cache..." "INFO"

    # Check NuGet cache size before
    $cacheInfo = dotnet nuget locals all --list 2>&1
    Write-OptimizationLog "Current cache locations:" "INFO"
    $cacheInfo | ForEach-Object { Write-OptimizationLog "   $_" "INFO" }

    # Optimize: Clear only http-cache (keep global-packages for speed)
    Write-OptimizationLog "Clearing HTTP cache only (preserving packages)..." "INFO"
    dotnet nuget locals http-cache --clear

    # Optimize: Use --no-dependencies for faster restores in development
    Write-OptimizationLog "Setting up optimized restore..." "INFO"
    $optimizedRestoreResult = dotnet restore BusBuddy.sln --no-dependencies --verbosity minimal 2>&1

    if ($LASTEXITCODE -eq 0) {
        Write-OptimizationLog "✅ Cache optimization completed" "SUCCESS"
    } else {
        Write-OptimizationLog "⚠️ Cache optimization had warnings (continuing)" "WARN"
    }
}

function Enable-ParallelBuild {
    <#
    .SYNOPSIS
    Configures optimal parallel build settings
    #>
    Write-OptimizationLog "⚡ Configuring parallel build settings..." "INFO"

    $processorCount = $env:NUMBER_OF_PROCESSORS
    $optimalThreads = [Math]::Max(1, [Math]::Min($processorCount, 4)) # Cap at 4 for stability

    Write-OptimizationLog "Available processors: $processorCount" "INFO"
    Write-OptimizationLog "Optimal build threads: $optimalThreads" "INFO"

    # Test parallel build
    $parallelStart = Get-Date
    $parallelResult = dotnet build BusBuddy.sln --no-restore -m:$optimalThreads --verbosity minimal 2>&1
    $parallelTime = (Get-Date) - $parallelStart

    if ($LASTEXITCODE -eq 0) {
        Write-OptimizationLog "✅ Parallel build completed in $($parallelTime.TotalSeconds.ToString('F2'))s" "SUCCESS"
        return $optimalThreads
    } else {
        Write-OptimizationLog "⚠️ Parallel build had issues, reverting to single-threaded" "WARN"
        return 1
    }
}

function Analyze-BuildBottlenecks {
    <#
    .SYNOPSIS
    Identifies build performance bottlenecks
    #>
    Write-OptimizationLog "🔍 Analyzing build bottlenecks..." "INFO"

    # Analyze with detailed verbosity to identify slow components
    $detailStart = Get-Date
    $verboseOutput = dotnet build BusBuddy.sln --no-restore --verbosity detailed 2>&1
    $detailTime = (Get-Date) - $detailStart

    # Parse output for timing information
    $timingLines = $verboseOutput | Where-Object { $_ -match "\d+\.\d+s|elapsed|time" }

    Write-OptimizationLog "🎯 Build Analysis Results:" "INFO"
    Write-OptimizationLog "   Detailed build time: $($detailTime.TotalSeconds.ToString('F2'))s" "INFO"

    # Identify slow projects
    $projectLines = $verboseOutput | Where-Object { $_ -match "Building.*\.csproj" }
    if ($projectLines.Count -gt 0) {
        Write-OptimizationLog "   Projects being built:" "INFO"
        $projectLines | ForEach-Object {
            Write-OptimizationLog "     $_" "INFO"
        }
    }

    # Check for potential optimizations
    $warnings = $verboseOutput | Where-Object { $_ -match "warning|slow|performance" }
    if ($warnings.Count -gt 0) {
        Write-OptimizationLog "⚠️ Performance warnings found:" "WARN"
        $warnings | Select-Object -First 5 | ForEach-Object {
            Write-OptimizationLog "     $_" "WARN"
        }
    }
}

function Apply-BuildOptimizations {
    <#
    .SYNOPSIS
    Applies recommended build optimizations
    #>
    Write-OptimizationLog "🔧 Applying build optimizations..." "INFO"

    # Create optimized build script
    $optimizedBuildScript = @"
@echo off
REM Optimized BusBuddy Build Script
REM Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')

echo 🚌 Starting optimized BusBuddy build...

REM Use parallel build with optimal thread count
dotnet build BusBuddy.sln --no-restore -m:$($env:NUMBER_OF_PROCESSORS) --verbosity minimal

if %ERRORLEVEL% EQU 0 (
    echo ✅ Build completed successfully
) else (
    echo ❌ Build failed
    exit /b %ERRORLEVEL%
)
"@

    $scriptPath = Join-Path $PWD "build-optimized.cmd"
    $optimizedBuildScript | Out-File -FilePath $scriptPath -Encoding ASCII

    Write-OptimizationLog "✅ Created optimized build script: $scriptPath" "SUCCESS"

    # Test the optimized build
    Write-OptimizationLog "Testing optimized build..." "INFO"
    $testStart = Get-Date
    & $scriptPath
    $testTime = (Get-Date) - $testStart

    if ($LASTEXITCODE -eq 0) {
        Write-OptimizationLog "✅ Optimized build test successful in $($testTime.TotalSeconds.ToString('F2'))s" "SUCCESS"
    } else {
        Write-OptimizationLog "❌ Optimized build test failed" "ERROR"
    }
}

# ═══════════════════════════════════════════════════════════════════════════════
# MAIN EXECUTION
# ═══════════════════════════════════════════════════════════════════════════════

Write-OptimizationLog "🎯 BusBuddy Build Performance Optimization" "INFO"
Write-OptimizationLog "Target build time: ${TARGET_BUILD_TIME_SECONDS}s" "INFO"

# Baseline measurement
$baseline = Measure-BuildPerformance

# Apply optimizations based on parameters
if ($OptimizeCache -or $ApplyOptimizations) {
    Optimize-BuildCache
}

if ($EnableParallel -or $ApplyOptimizations) {
    $optimalThreads = Enable-ParallelBuild
}

if ($AnalyzeBottlenecks) {
    Analyze-BuildBottlenecks
}

if ($ApplyOptimizations) {
    Apply-BuildOptimizations

    # Final measurement
    Write-OptimizationLog "📊 Re-measuring performance after optimizations..." "INFO"
    $optimized = Measure-BuildPerformance

    $improvement = $baseline.TotalTimeSeconds - $optimized.TotalTimeSeconds
    $improvementPercent = ($improvement / $baseline.TotalTimeSeconds) * 100

    Write-OptimizationLog "🎉 Optimization Results:" "SUCCESS"
    Write-OptimizationLog "   Before: $($baseline.TotalTimeSeconds.ToString('F2'))s" "SUCCESS"
    Write-OptimizationLog "   After: $($optimized.TotalTimeSeconds.ToString('F2'))s" "SUCCESS"
    Write-OptimizationLog "   Improvement: $($improvement.ToString('F2'))s ($($improvementPercent.ToString('F1'))%)" "SUCCESS"

    if ($optimized.TotalTimeSeconds -le $TARGET_BUILD_TIME_SECONDS) {
        Write-OptimizationLog "🎯 TARGET ACHIEVED! Build time under ${TARGET_BUILD_TIME_SECONDS}s" "SUCCESS"
    } else {
        $remaining = $optimized.TotalTimeSeconds - $TARGET_BUILD_TIME_SECONDS
        Write-OptimizationLog "⏳ $($remaining.ToString('F2'))s remaining to reach target" "INFO"
    }
}

Write-OptimizationLog "🏁 Build optimization complete" "SUCCESS"
