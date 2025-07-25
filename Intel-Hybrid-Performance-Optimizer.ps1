# 🚀 Intel i5-1334U Performance Optimization Profile
# Hybrid Architecture Optimization for BusBuddy Development
# 10 Performance Cores + 2 Efficiency Cores + 12 Logical Processors

class BusBuddyHybridOptimization {
    [int]$PerformanceCores = 10
    [int]$EfficiencyCores = 2
    [int]$LogicalProcessors = 12
    [int]$OptimalParallelThrottle = 10
    [int]$BackgroundTaskThrottle = 2

    # Intel Thread Director optimized settings
    [hashtable]$ProcessAffinity = @{
        MainProcess     = 0x3FF      # P-cores (0-9): 1111111111
        BackgroundTasks = 0xC00  # E-cores (10-11): 1100000000
        BuildTasks      = 0x3FF       # P-cores for compilation
    }

    # .NET Runtime optimization for hybrid architecture
    [hashtable]$RuntimeConfig = @{
        "System.GC.Server"                                     = $true
        "System.GC.Concurrent"                                 = $true
        "System.GC.RetainVM"                                   = $true
        "System.GC.HeapCount"                                  = 12
        "System.Threading.ThreadPool.MinWorkerThreads"         = 12
        "System.Threading.ThreadPool.MaxWorkerThreads"         = 48
        "System.Threading.ThreadPool.MinCompletionPortThreads" = 8
        "System.Threading.ThreadPool.MaxCompletionPortThreads" = 16
    }

    [void] SetBusBuddyProcessAffinity() {
        $processes = Get-Process -Name "*BusBuddy*" -ErrorAction SilentlyContinue
        foreach ($process in $processes) {
            try {
                $process.ProcessorAffinity = [IntPtr]$this.ProcessAffinity.MainProcess
                Write-Host "🎯 Set BusBuddy process affinity to P-cores" -ForegroundColor Green
            }
            catch {
                Write-Warning "Could not set process affinity: $_"
            }
        }
    }

    [void] OptimizeParallelProcessing() {
        # Configure parallel processing for hybrid architecture
        $env:DOTNET_PROCESSOR_COUNT = $this.LogicalProcessors
        $env:DOTNET_EnableWriteXorExecute = 0
        $env:DOTNET_TieredPGO = 1
        $env:DOTNET_TC_QuickJitForLoops = 1

        Write-Host "🚀 Optimized parallel processing for $($this.LogicalProcessors) logical processors" -ForegroundColor Cyan
    }

    [void] ConfigureMemoryOptimization() {
        # Memory optimization for 16GB system
        $env:DOTNET_GCHeapCount = 12
        $env:DOTNET_GCServer = 1
        $env:DOTNET_GCConcurrent = 1
        $env:DOTNET_GCRetainVM = 1

        Write-Host "🧠 Configured memory optimization for hybrid architecture" -ForegroundColor Cyan
    }
}

# Initialize optimization
$Global:BusBuddyOptimizer = [BusBuddyHybridOptimization]::new()

# Apply optimizations
function Initialize-BusBuddyPerformance {
    [CmdletBinding()]
    param()

    Write-Host "🚀 Initializing BusBuddy Performance Optimization..." -ForegroundColor Yellow

    # Apply hybrid architecture optimizations
    $Global:BusBuddyOptimizer.OptimizeParallelProcessing()
    $Global:BusBuddyOptimizer.ConfigureMemoryOptimization()

    # Set PowerShell parallel processing defaults
    $Global:PSDefaultParameterValues = @{
        'ForEach-Object:ThrottleLimit' = $Global:BusBuddyOptimizer.OptimalParallelThrottle
        'Invoke-Command:ThrottleLimit' = $Global:BusBuddyOptimizer.OptimalParallelThrottle
        'Start-Job:ThrottleLimit'      = $Global:BusBuddyOptimizer.BackgroundTaskThrottle
    }

    Write-Host "✅ Performance optimization complete!" -ForegroundColor Green
    Write-Host "   → P-cores: $($Global:BusBuddyOptimizer.PerformanceCores)" -ForegroundColor White
    Write-Host "   → E-cores: $($Global:BusBuddyOptimizer.EfficiencyCores)" -ForegroundColor White
    Write-Host "   → Logical processors: $($Global:BusBuddyOptimizer.LogicalProcessors)" -ForegroundColor White
    Write-Host "   → Parallel throttle: $($Global:BusBuddyOptimizer.OptimalParallelThrottle)" -ForegroundColor White
}

# Performance monitoring functions
function Get-BusBuddyPerformanceStats {
    [CmdletBinding()]
    param()

    $processes = Get-Process -Name "*BusBuddy*", "*dotnet*", "*Code*" -ErrorAction SilentlyContinue
    $totalMemory = ($processes | Measure-Object WorkingSet -Sum).Sum / 1GB

    Write-Host "📊 BusBuddy Performance Statistics:" -ForegroundColor Cyan
    Write-Host "   Memory Usage: $([math]::Round($totalMemory, 2)) GB" -ForegroundColor White
    Write-Host "   Active Processes: $($processes.Count)" -ForegroundColor White

    if ($processes) {
        $processes | Sort-Object WorkingSet -Descending | Select-Object -First 5 |
        Format-Table ProcessName, @{Name = "Memory(MB)"; Expression = { [math]::Round($_.WorkingSet / 1MB, 1) } }, CPU -AutoSize
    }
}

function Test-BusBuddyParallelPerformance {
    [CmdletBinding()]
    param(
        [int]$TestSize = 1000
    )

    Write-Host "🧪 Testing parallel processing performance..." -ForegroundColor Yellow

    # Test sequential processing
    $sequential = Measure-Command {
        1..$TestSize | ForEach-Object { Start-Sleep -Milliseconds 1 }
    }

    # Test parallel processing with optimized throttle
    $parallel = Measure-Command {
        1..$TestSize | ForEach-Object -Parallel { Start-Sleep -Milliseconds 1 } -ThrottleLimit $Global:BusBuddyOptimizer.OptimalParallelThrottle
    }

    $speedup = [math]::Round($sequential.TotalMilliseconds / $parallel.TotalMilliseconds, 2)

    Write-Host "Sequential: $([math]::Round($sequential.TotalMilliseconds))ms" -ForegroundColor Red
    Write-Host "Parallel:   $([math]::Round($parallel.TotalMilliseconds))ms" -ForegroundColor Green
    Write-Host "Speedup:    ${speedup}x" -ForegroundColor Cyan
}

# Auto-initialize on profile load
Initialize-BusBuddyPerformance

# Export functions
Export-ModuleMember -Function Initialize-BusBuddyPerformance, Get-BusBuddyPerformanceStats, Test-BusBuddyParallelPerformance
