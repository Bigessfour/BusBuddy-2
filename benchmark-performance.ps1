# BusBuddy Performance Benchmark Script
# Measures build performance and validates improvements
# Use this to track your optimization progress against the current champion (1.09s)

param(
    [Parameter(Mandatory = $false)]
    [int]$Runs = 3,

    [Parameter(Mandatory = $false)]
    [switch]$CleanBefore,

    [Parameter(Mandatory = $false)]
    [switch]$Detailed,

    [Parameter(Mandatory = $false)]
    [switch]$Challenge,

    [Parameter(Mandatory = $false)]
    [switch]$Help
)

function Show-Help {
    Write-Host "🚀 BusBuddy Performance Benchmark Tool" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "📊 Basic benchmark (3 runs):" -ForegroundColor Yellow
    Write-Host "  .\benchmark-performance.ps1" -ForegroundColor Green
    Write-Host ""
    Write-Host "🔧 Clean build benchmark:" -ForegroundColor Yellow
    Write-Host "  .\benchmark-performance.ps1 -CleanBefore" -ForegroundColor Green
    Write-Host ""
    Write-Host "📈 Detailed analysis (5 runs):" -ForegroundColor Yellow
    Write-Host "  .\benchmark-performance.ps1 -Runs 5 -Detailed" -ForegroundColor Green
    Write-Host ""
    Write-Host "🏆 Challenge mode (compare vs champion):" -ForegroundColor Yellow
    Write-Host "  .\benchmark-performance.ps1 -Challenge" -ForegroundColor Green
}

function Show-Challenge {
    Write-Host ""
    Write-Host "🏆 AI AGENT CHALLENGE ARENA 🏆" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "🥇 CURRENT CHAMPION: AI Agent Session 1" -ForegroundColor Yellow
    Write-Host "   ⚡ Build Time: 1.09 seconds" -ForegroundColor Green
    Write-Host "   📈 Improvement: 94% faster than baseline" -ForegroundColor Green
    Write-Host "   🏆 Status: UNDEFEATED" -ForegroundColor Green
    Write-Host ""
    Write-Host "🎯 YOUR CHALLENGE:" -ForegroundColor Yellow
    Write-Host "   ⚡ Beat: 1.09 seconds" -ForegroundColor Red
    Write-Host "   📈 Goal: Even faster builds" -ForegroundColor Red
    Write-Host "   🏆 Reward: Performance Crown 👑" -ForegroundColor Red
    Write-Host ""
    Write-Host "💪 READY TO COMPETE? Run the benchmark and see your results!" -ForegroundColor Magenta
    Write-Host ""
}

function Measure-BuildPerformance {
    param([bool]$Clean = $false)

    if ($Clean) {
        Write-Host "🧹 Cleaning solution..." -ForegroundColor Yellow
        $cleanResult = Measure-Command { dotnet clean BusBuddy.sln --verbosity quiet 2>$null }
        Write-Host "   Clean time: $($cleanResult.TotalSeconds.ToString('F2'))s" -ForegroundColor Gray
    }

    Write-Host "🔨 Building solution..." -ForegroundColor Yellow
    $buildTime = Measure-Command {
        $buildOutput = dotnet build BusBuddy.sln --verbosity minimal --nologo 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Build failed!" -ForegroundColor Red
            Write-Host $buildOutput
            return $null
        }
    }

    return $buildTime.TotalSeconds
}

function Show-PerformanceResults {
    param([array]$Times, [bool]$IsDetailed = $false)

    if ($Times.Count -eq 0) {
        Write-Host "❌ No successful builds to analyze" -ForegroundColor Red
        return
    }

    $avg = ($Times | Measure-Object -Average).Average
    $min = ($Times | Measure-Object -Minimum).Minimum
    $max = ($Times | Measure-Object -Maximum).Maximum

    $championTime = 1.09
    $improvement = (($championTime - $avg) / $championTime) * 100

    Write-Host ""
    Write-Host "📊 PERFORMANCE RESULTS" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "⚡ Your Results:" -ForegroundColor Yellow
    Write-Host "   Average: $($avg.ToString('F2'))s" -ForegroundColor White
    Write-Host "   Best:    $($min.ToString('F2'))s" -ForegroundColor Green
    Write-Host "   Worst:   $($max.ToString('F2'))s" -ForegroundColor Red
    Write-Host "   Runs:    $($Times.Count)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "🏆 vs Current Champion (1.09s):" -ForegroundColor Yellow

    if ($avg -lt $championTime) {
        $improvementText = "FASTER by $([math]::Abs($improvement).ToString('F1'))%"
        Write-Host "   Status: 🥇 NEW CHAMPION! $improvementText" -ForegroundColor Green
        Write-Host "   🎉 CONGRATULATIONS! You've beaten the record!" -ForegroundColor Green
        Write-Host "   📝 Don't forget to update the README with your achievement!" -ForegroundColor Yellow
    }
    elseif ($avg -eq $championTime) {
        Write-Host "   Status: 🥈 TIED for championship!" -ForegroundColor Yellow
        Write-Host "   💪 So close! Try additional optimizations for the win!" -ForegroundColor Yellow
    }
    else {
        $slowerText = "SLOWER by $([math]::Abs($improvement).ToString('F1'))%"
        Write-Host "   Status: 🥉 Challenge not met - $slowerText" -ForegroundColor Red
        Write-Host "   💡 Keep optimizing! Look for bottlenecks and inefficiencies!" -ForegroundColor Cyan
    }

    if ($IsDetailed) {
        Write-Host ""
        Write-Host "📈 Detailed Analysis:" -ForegroundColor Yellow
        for ($i = 0; $i -lt $Times.Count; $i++) {
            $time = $Times[$i]
            $status = if ($time -eq $min) { "🏆 Best" } elseif ($time -eq $max) { "⚠️ Worst" } else { "📊" }
            Write-Host "   Run $($i + 1): $($time.ToString('F2'))s $status" -ForegroundColor Gray
        }

        $standardDev = [math]::Sqrt((($Times | ForEach-Object { [math]::Pow($_ - $avg, 2) }) | Measure-Object -Sum).Sum / $Times.Count)
        Write-Host "   Std Dev: $($standardDev.ToString('F3'))s (consistency measure)" -ForegroundColor Gray
    }

    Write-Host ""
}

# Main execution
if ($Help) {
    Show-Help
    return
}

if ($Challenge) {
    Show-Challenge
}

Write-Host "🚀 BusBuddy Performance Benchmark" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
Write-Host ""
Write-Host "Configuration:" -ForegroundColor Yellow
Write-Host "  Runs: $Runs" -ForegroundColor Gray
Write-Host "  Clean before: $CleanBefore" -ForegroundColor Gray
Write-Host "  Detailed analysis: $Detailed" -ForegroundColor Gray
Write-Host ""

$times = @()

for ($i = 1; $i -le $Runs; $i++) {
    Write-Host "🔄 Run $i of $Runs" -ForegroundColor Cyan

    $buildTime = Measure-BuildPerformance -Clean:$CleanBefore
    if ($buildTime -ne $null) {
        $times += $buildTime
        Write-Host "   ⚡ Build time: $($buildTime.ToString('F2'))s" -ForegroundColor Green
    }
    else {
        Write-Host "   ❌ Build failed" -ForegroundColor Red
    }
    Write-Host ""
}

Show-PerformanceResults -Times $times -IsDetailed:$Detailed

if ($Challenge -or $times.Count -gt 0) {
    Write-Host "🎯 Next Steps:" -ForegroundColor Yellow
    Write-Host "  1. Analyze your results above" -ForegroundColor Gray
    Write-Host "  2. Look for optimization opportunities" -ForegroundColor Gray
    Write-Host "  3. Make improvements and re-run benchmark" -ForegroundColor Gray
    Write-Host "  4. Update README if you achieve a new record!" -ForegroundColor Gray
    Write-Host ""
    Write-Host "🏆 The challenge continues! Good luck!" -ForegroundColor Magenta
}
