#!/usr/bin/env pwsh
# Enhanced Test Development Script with Coverage Focus

param(
    [switch]$Coverage,
    [switch]$Sequential,
    [switch]$FastOnly,
    [switch]$DatabaseOnly,
    [switch]$UIOnly,
    [switch]$GenerateReport,
    [string]$Filter = ''
)

Write-Host '🧪 Bus Buddy Test Development and Coverage Tool' -ForegroundColor Cyan
Write-Host '=============================================' -ForegroundColor Cyan

# Clean and setup
Write-Host '🧹 Cleaning previous results...' -ForegroundColor Yellow
if (Test-Path 'TestResults') { Remove-Item 'TestResults' -Recurse -Force }
if (Test-Path 'CoverageReport') { Remove-Item 'CoverageReport' -Recurse -Force }
New-Item -ItemType Directory -Path 'TestResults' -Force | Out-Null

# Build solution
Write-Host '🔨 Building solution...' -ForegroundColor Yellow
$buildResult = dotnet build --configuration Release --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host '❌ Build failed!' -ForegroundColor Red
    exit 1
}
Write-Host '✅ Build successful' -ForegroundColor Green

# Determine test strategy
$runSettings = if ($Sequential) { $null } else { '--settings', 'BusBuddy.Tests\test.runsettings' }

# Base test command
$baseCommand = @(
    'test', 'BusBuddy.Tests\BusBuddy.Tests.csproj'
    '--configuration', 'Release'
    '--no-build'
    '--logger', 'console;verbosity=normal'
    '--results-directory', 'TestResults'
)

if ($runSettings) { $baseCommand += $runSettings }
if ($Coverage) { $baseCommand += '--collect', 'XPlat Code Coverage' }

# Execute based on strategy
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

if ($FastOnly) {
    Write-Host '🏃‍♂️ Running fast unit tests only...' -ForegroundColor Cyan
    $testFilter = 'Category=Unit&Category!=Database&Category!=UI'
    if ($Filter) { $testFilter = "($Filter)&$testFilter" }
    $command = $baseCommand + @('--filter', $testFilter)
    & dotnet @command
} elseif ($DatabaseOnly) {
    Write-Host '🗄️ Running database tests sequentially...' -ForegroundColor Yellow
    # Force sequential for database tests
    $command = $baseCommand + @('--filter', 'Category=Database|Category=Integration', '--settings', $null)
    $command = $command | Where-Object { $_ -ne $null }
    & dotnet @command
} elseif ($UIOnly) {
    Write-Host '🖥️ Running UI tests sequentially...' -ForegroundColor Yellow
    $command = $baseCommand + @('--filter', 'Category=UI', '--settings', $null)
    $command = $command | Where-Object { $_ -ne $null }
    & dotnet @command
} else {
    # Run all tests with appropriate strategy
    if ($Sequential) {
        Write-Host '🚶‍♂️ Running all tests sequentially...' -ForegroundColor Yellow
        if ($Filter) { $baseCommand += '--filter', $Filter }
        & dotnet @baseCommand
    } else {
        Write-Host '🏃‍♂️ Running tests with optimized parallel strategy...' -ForegroundColor Green

        # Phase 1: Fast parallel tests
        Write-Host "`n📍 Phase 1: Fast unit tests (parallel)..." -ForegroundColor Cyan
        $phase1Filter = 'Category=Unit&Category!=Database&Category!=UI'
        if ($Filter) { $phase1Filter = "($Filter)&$phase1Filter" }
        $phase1Command = $baseCommand + @('--filter', $phase1Filter)
        & dotnet @phase1Command

        # Phase 2: Database tests (sequential)
        Write-Host "`n📍 Phase 2: Database tests (sequential)..." -ForegroundColor Yellow
        $phase2Filter = 'Category=Database|Category=Integration'
        if ($Filter) { $phase2Filter = "($Filter)&($phase2Filter)" }
        $phase2Command = $baseCommand + @('--filter', $phase2Filter) | Where-Object { $_ -ne '--settings' -and $_ -ne 'BusBuddy.Tests\test.runsettings' }
        & dotnet @phase2Command

        # Phase 3: UI tests (sequential)
        Write-Host "`n📍 Phase 3: UI tests (sequential)..." -ForegroundColor Yellow
        $phase3Filter = 'Category=UI'
        if ($Filter) { $phase3Filter = "($Filter)&$phase3Filter" }
        $phase3Command = $baseCommand + @('--filter', $phase3Filter) | Where-Object { $_ -ne '--settings' -and $_ -ne 'BusBuddy.Tests\test.runsettings' }
        & dotnet @phase3Command
    }
}

$stopwatch.Stop()

# Coverage report generation
if ($Coverage) {
    Write-Host "`n📊 Processing coverage data..." -ForegroundColor Cyan

    $coverageFiles = Get-ChildItem 'TestResults\*\coverage.cobertura.xml' -ErrorAction SilentlyContinue
    if ($coverageFiles) {
        Write-Host "✅ Found $($coverageFiles.Count) coverage file(s)" -ForegroundColor Green

        # Check coverage content
        $firstFile = Get-Content $coverageFiles[0].FullName -Raw
        if ($firstFile -match 'lines-covered="(\d+)".*lines-valid="(\d+)"') {
            $covered = [int]$matches[1]
            $total = [int]$matches[2]
            if ($total -gt 0) {
                $percentage = [math]::Round(($covered / $total) * 100, 2)
                Write-Host "📈 Coverage: $covered/$total lines ($percentage%)" -ForegroundColor $(if ($percentage -gt 70) { 'Green' } elseif ($percentage -gt 50) { 'Yellow' } else { 'Red' })
            } else {
                Write-Host '⚠️ No coverage data detected - may need to rebuild or check assembly references' -ForegroundColor Yellow
            }
        }

        if ($GenerateReport) {
            # Check if reportgenerator is available
            $reportGen = Get-Command 'reportgenerator' -ErrorAction SilentlyContinue
            if ($reportGen) {
                Write-Host '📋 Generating HTML coverage report...' -ForegroundColor Cyan
                $coveragePaths = ($coverageFiles | Select-Object -ExpandProperty FullName) -join ';'
                & reportgenerator "-reports:$coveragePaths" '-targetdir:CoverageReport' '-reporttypes:Html;HtmlSummary;Badges'

                if (Test-Path 'CoverageReport\index.html') {
                    Write-Host '✅ Coverage report generated: CoverageReport\index.html' -ForegroundColor Green

                    # Try to open in browser
                    try {
                        Start-Process 'CoverageReport\index.html'
                        Write-Host '🌐 Opened coverage report in browser' -ForegroundColor Green
                    } catch {
                        Write-Host "📁 Coverage report available at: $(Resolve-Path 'CoverageReport\index.html')" -ForegroundColor Cyan
                    }
                } else {
                    Write-Host '❌ Failed to generate coverage report' -ForegroundColor Red
                }
            } else {
                Write-Host '⚠️ reportgenerator not found. Install with:' -ForegroundColor Yellow
                Write-Host '   dotnet tool install --global dotnet-reportgenerator-globaltool' -ForegroundColor Gray
            }
        }
    } else {
        Write-Host '❌ No coverage files found. Coverage collection may have failed.' -ForegroundColor Red
        Write-Host '💡 Try: ' -ForegroundColor Yellow
        Write-Host '   1. Rebuild the solution' -ForegroundColor Gray
        Write-Host '   2. Check that Bus_Buddy.csproj references are correct' -ForegroundColor Gray
        Write-Host '   3. Run with -Sequential flag to avoid parallel issues' -ForegroundColor Gray
    }
}

# Summary
Write-Host "`n📋 Execution Summary:" -ForegroundColor Cyan
Write-Host "⏱️ Total time: $($stopwatch.Elapsed.TotalSeconds.ToString('F2')) seconds" -ForegroundColor White
Write-Host "🔧 Strategy: $(if ($Sequential) { 'Sequential' } elseif ($FastOnly) { 'Fast tests only' } elseif ($DatabaseOnly) { 'Database tests only' } elseif ($UIOnly) { 'UI tests only' } else { 'Optimized parallel' })" -ForegroundColor White
Write-Host "📊 Coverage: $(if ($Coverage) { 'Enabled' } else { 'Disabled' })" -ForegroundColor $(if ($Coverage) { 'Green' } else { 'Gray' })

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n🎉 Test execution completed successfully!" -ForegroundColor Green
} else {
    Write-Host "`n⚠️ Some tests failed. Use the flags below for debugging:" -ForegroundColor Yellow
    Write-Host '   -FastOnly      : Run only fast unit tests' -ForegroundColor Gray
    Write-Host '   -DatabaseOnly  : Run database tests sequentially' -ForegroundColor Gray
    Write-Host '   -Sequential    : Disable all parallelization' -ForegroundColor Gray
    Write-Host '   -Coverage      : Enable coverage collection' -ForegroundColor Gray
    Write-Host '   -GenerateReport: Create HTML coverage report' -ForegroundColor Gray
}

exit $LASTEXITCODE
