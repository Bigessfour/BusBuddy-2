#!/usr/bin/env pwsh
# Enhanced parallel test execution script for Bus Buddy

param(
    [string]$Configuration = 'Release',
    [string]$Filter = '',
    [switch]$Parallel,
    [switch]$Coverage,
    [switch]$Verbose,
    [int]$MaxDegreeOfParallelism = 0 # 0 = auto-detect
)

Write-Host '🚀 Bus Buddy Enhanced Test Runner' -ForegroundColor Cyan
Write-Host '=================================' -ForegroundColor Cyan

# Set parallelism
if ($MaxDegreeOfParallelism -eq 0) {
    $MaxDegreeOfParallelism = [System.Environment]::ProcessorCount
    Write-Host "🔧 Auto-detected $MaxDegreeOfParallelism CPU cores" -ForegroundColor Green
}

# Clean previous results
Write-Host '🧹 Cleaning previous test results...' -ForegroundColor Yellow
if (Test-Path 'TestResults') {
    Remove-Item 'TestResults' -Recurse -Force
}
New-Item -ItemType Directory -Path 'TestResults' -Force | Out-Null

# Build the solution first
Write-Host '🔨 Building solution...' -ForegroundColor Yellow
$buildResult = dotnet build --configuration $Configuration --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host '❌ Build failed!' -ForegroundColor Red
    exit 1
}
Write-Host '✅ Build completed successfully' -ForegroundColor Green

# Prepare test command
$testCommand = @(
    'test'
    'BusBuddy.Tests\BusBuddy.Tests.csproj'
    '--configuration', $Configuration
    '--no-build'
    '--logger', "console;verbosity=$(if($Verbose){'detailed'}else{'normal'})"
    '--results-directory', 'TestResults'
)

# Add filter if specified
if ($Filter) {
    $testCommand += '--filter', $Filter
}

# Add coverage collection
if ($Coverage) {
    $testCommand += '--collect', 'XPlat Code Coverage'
}

# Add parallel settings
if ($Parallel) {
    $testCommand += '--settings', 'BusBuddy.Tests\test.runsettings'
    Write-Host "🏃 Running tests in parallel with $MaxDegreeOfParallelism workers" -ForegroundColor Green
} else {
    Write-Host '🚶 Running tests sequentially' -ForegroundColor Yellow
}

# Run different test categories in optimal order
Write-Host "`n📊 Test Execution Plan:" -ForegroundColor Cyan
Write-Host '1. Fast unit tests (Models, Utilities) - Parallel' -ForegroundColor Green
Write-Host '2. Service tests - Parallel' -ForegroundColor Green
Write-Host '3. UI tests - Sequential' -ForegroundColor Yellow
Write-Host '4. Integration tests - Sequential' -ForegroundColor Yellow

$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

# Phase 1: Fast parallel tests
Write-Host "`n🏃‍♂️ Phase 1: Running fast unit tests in parallel..." -ForegroundColor Cyan
$phase1Command = $testCommand + @('--filter', 'Category=Unit&Category!=UI')
if ($Filter) {
    $phase1Command = $testCommand + @('--filter', "($($Filter))&Category=Unit&Category!=UI")
}
$phase1Result = & dotnet @phase1Command

# Phase 2: Service tests in parallel
Write-Host "`n🏃‍♂️ Phase 2: Running service tests in parallel..." -ForegroundColor Cyan
$phase2Command = $testCommand + @('--filter', 'Category=Services')
if ($Filter) {
    $phase2Command = $testCommand + @('--filter', "($($Filter))&Category=Services")
}
$phase2Result = & dotnet @phase2Command

# Phase 3: UI tests sequentially
Write-Host "`n🚶‍♂️ Phase 3: Running UI tests sequentially..." -ForegroundColor Yellow
$phase3Command = $testCommand + @('--filter', 'Category=UI|Category=Sequential')
if ($Filter) {
    $phase3Command = $testCommand + @('--filter', "($($Filter))&(Category=UI|Category=Sequential)")
}
$phase3Result = & dotnet @phase3Command

$stopwatch.Stop()

Write-Host "`n⏱️  Total execution time: $($stopwatch.Elapsed.TotalSeconds.ToString('F2')) seconds" -ForegroundColor Cyan

# Generate coverage report if coverage was collected
if ($Coverage -and (Test-Path 'TestResults\*\coverage.cobertura.xml')) {
    Write-Host "`n📈 Generating coverage report..." -ForegroundColor Cyan

    # Check if reportgenerator is available
    $reportGen = Get-Command 'reportgenerator' -ErrorAction SilentlyContinue
    if ($reportGen) {
        $coverageFiles = Get-ChildItem 'TestResults\*\coverage.cobertura.xml' | Select-Object -ExpandProperty FullName
        reportgenerator "-reports:$($coverageFiles -join ';')" '-targetdir:TestResults\CoverageReport' '-reporttypes:Html;HtmlSummary'
        Write-Host '✅ Coverage report generated in TestResults\CoverageReport' -ForegroundColor Green
    } else {
        Write-Host '⚠️  reportgenerator not found. Install with: dotnet tool install --global dotnet-reportgenerator-globaltool' -ForegroundColor Yellow
    }
}

# Summary
Write-Host "`n📋 Test Execution Summary:" -ForegroundColor Cyan
Write-Host "✅ Parallel execution enabled: $Parallel" -ForegroundColor $(if ($Parallel) { 'Green' }else { 'Yellow' })
Write-Host "⚡ Max parallelism: $MaxDegreeOfParallelism" -ForegroundColor Green
Write-Host "🕒 Total time: $($stopwatch.Elapsed.TotalSeconds.ToString('F2'))s" -ForegroundColor Green

if ($LASTEXITCODE -eq 0) {
    Write-Host '🎉 All tests completed successfully!' -ForegroundColor Green
} else {
    Write-Host '⚠️  Some tests failed. Check the output above for details.' -ForegroundColor Yellow
}

exit $LASTEXITCODE
