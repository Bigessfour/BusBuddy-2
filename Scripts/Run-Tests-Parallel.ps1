# Simple Parallel Test Runner with Coverage
param(
    [switch]$Coverage,
    [string]$Filter = ''
)

$ProjectPath = 'BusBuddy.Tests\BusBuddy.Tests.csproj'
$TestResultsDir = 'TestResults'

# Ensure test results directory exists
if (!(Test-Path $TestResultsDir)) {
    New-Item -ItemType Directory -Path $TestResultsDir -Force
}

Write-Host 'Starting parallel test execution...' -ForegroundColor Green

$TestArgs = @(
    'test', $ProjectPath,
    '--configuration', 'Release',
    '--no-build',
    '--logger', 'console;verbosity=normal',
    '--settings', 'BusBuddy.Tests\test.runsettings'
)

if ($Coverage) {
    $TestArgs += @('--collect:XPlat Code Coverage', '--results-directory', $TestResultsDir)
}

if ($Filter) {
    $TestArgs += @('--filter', $Filter)
}

# Execute tests
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
$result = & dotnet @TestArgs
$stopwatch.Stop()

Write-Host "`nTest execution completed in $($stopwatch.Elapsed.TotalSeconds) seconds" -ForegroundColor Cyan

# Display test results summary
if ($result -match 'Total tests: (\d+)') {
    $totalTests = $matches[1]
    Write-Host "Total tests executed: $totalTests" -ForegroundColor White
}

if ($result -match 'Passed: (\d+)') {
    $passedTests = $matches[1]
    Write-Host "Passed: $passedTests" -ForegroundColor Green
}

if ($result -match 'Failed: (\d+)') {
    $failedTests = $matches[1]
    if ($failedTests -gt 0) {
        Write-Host "Failed: $failedTests" -ForegroundColor Red
    } else {
        Write-Host 'Failed: 0' -ForegroundColor Green
    }
}

if ($result -match 'Skipped: (\d+)') {
    $skippedTests = $matches[1]
    Write-Host "Skipped: $skippedTests" -ForegroundColor Yellow
}

# Show coverage information if requested
if ($Coverage) {
    $coverageFiles = Get-ChildItem -Path $TestResultsDir -Filter '*.cobertura.xml' -Recurse
    if ($coverageFiles) {
        Write-Host "`nCoverage files generated:" -ForegroundColor Cyan
        $coverageFiles | ForEach-Object {
            Write-Host "  $($_.FullName)" -ForegroundColor Gray
        }
    }
}

exit $LASTEXITCODE
