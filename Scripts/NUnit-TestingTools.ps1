# Bus Buddy NUnit Testing Extensions
# PowerShell tools for enhanced NUnit testing workflow

param(
    [string]$TestProject = "BusBuddy.Tests",
    [string]$OutputFormat = "trx",
    [switch]$Coverage,
    [switch]$Detailed,
    [string]$Filter = ""
)

function Install-NUnitTools {
    Write-Host "Installing NUnit PowerShell tools..." -ForegroundColor Green

    # Install NUnit test tools
    dotnet tool install --global dotnet-reportgenerator-globaltool
    dotnet tool install --global coverlet.console

    # Install PowerShell modules for test management
    Install-Module -Name Pester -Force -AllowClobber -Scope CurrentUser

    Write-Host "NUnit tools installed successfully!" -ForegroundColor Green
}

function Invoke-BusBuddyTests {
    param(
        [string]$Project = $TestProject,
        [string]$Format = $OutputFormat,
        [bool]$IncludeCoverage = $Coverage,
        [string]$TestFilter = $Filter
    )

    Write-Host "Running Bus Buddy NUnit Tests..." -ForegroundColor Green

    $testCommand = "dotnet test"

    # Add project path if specified
    if ($Project) {
        $testCommand += " `"$Project`""
    }

    # Add output format
    $testCommand += " --logger `"$Format`""

    # Add test filter if specified
    if ($TestFilter) {
        $testCommand += " --filter `"$TestFilter`""
    }

    # Add coverage collection
    if ($IncludeCoverage) {
        $testCommand += " --collect:`"XPlat Code Coverage`""
    }

    # Add verbose output if detailed
    if ($Detailed) {
        $testCommand += " --verbosity detailed"
    }

    Write-Host "Executing: $testCommand" -ForegroundColor Yellow

    # Execute the test command
    $result = Invoke-Expression $testCommand

    return $result
}

function Get-TestResults {
    param([string]$ResultsPath = "TestResults")

    Write-Host "Analyzing test results..." -ForegroundColor Cyan

    # Find latest test results
    $latestResults = Get-ChildItem -Path $ResultsPath -Filter "*.trx" -Recurse |
                    Sort-Object LastWriteTime -Descending |
                    Select-Object -First 1

    if ($latestResults) {
        [xml]$testXml = Get-Content $latestResults.FullName

        $testSummary = @{
            Total = $testXml.TestRun.ResultSummary.Counters.total
            Passed = $testXml.TestRun.ResultSummary.Counters.passed
            Failed = $testXml.TestRun.ResultSummary.Counters.failed
            Skipped = $testXml.TestRun.ResultSummary.Counters.skipped
            ExecutionTime = $testXml.TestRun.Times.finish - $testXml.TestRun.Times.start
        }

        Write-Host "`nTEST SUMMARY:" -ForegroundColor Magenta
        Write-Host "Total Tests: $($testSummary.Total)" -ForegroundColor White
        Write-Host "Passed: $($testSummary.Passed)" -ForegroundColor Green
        Write-Host "Failed: $($testSummary.Failed)" -ForegroundColor Red
        Write-Host "Skipped: $($testSummary.Skipped)" -ForegroundColor Yellow
        Write-Host "Execution Time: $($testSummary.ExecutionTime)" -ForegroundColor Cyan

        # Show failed tests if any
        if ([int]$testSummary.Failed -gt 0) {
            Write-Host "`nFAILED TESTS:" -ForegroundColor Red
            $failedTests = $testXml.TestRun.Results.UnitTestResult | Where-Object { $_.outcome -eq "Failed" }
            foreach ($test in $failedTests) {
                Write-Host "  - $($test.testName)" -ForegroundColor Red
                if ($test.Output.ErrorInfo.Message) {
                    Write-Host "    Error: $($test.Output.ErrorInfo.Message)" -ForegroundColor DarkRed
                }
            }
        }

        return $testSummary
    } else {
        Write-Warning "No test results found in $ResultsPath"
        return $null
    }
}

function New-TestCoverageReport {
    param(
        [string]$CoverageFile = "TestResults\**\coverage.cobertura.xml",
        [string]$ReportPath = "TestResults\CoverageReport"
    )

    Write-Host "Generating coverage report..." -ForegroundColor Green

    $coverageFiles = Get-ChildItem -Path $CoverageFile -Recurse

    if ($coverageFiles) {
        $reportGenerator = "reportgenerator"
        $arguments = @(
            "-reports:$($coverageFiles[0].FullName)"
            "-targetdir:$ReportPath"
            "-reporttypes:Html;Badges"
        )

        & $reportGenerator $arguments

        Write-Host "Coverage report generated at: $ReportPath" -ForegroundColor Green

        # Open report if possible
        $indexFile = Join-Path $ReportPath "index.html"
        if (Test-Path $indexFile) {
            Start-Process $indexFile
        }
    } else {
        Write-Warning "No coverage files found matching: $CoverageFile"
    }
}

function Test-BusBuddyComponents {
    Write-Host "Running Bus Buddy Component Tests..." -ForegroundColor Green

    # Define test categories for Bus Buddy
    $testCategories = @{
        "Models" = "Category=Models"
        "Repositories" = "Category=Repository"
        "Services" = "Category=Service"
        "Forms" = "Category=UI"
        "Integration" = "Category=Integration"
    }

    foreach ($category in $testCategories.Keys) {
        Write-Host "`nRunning $category tests..." -ForegroundColor Yellow

        $filter = $testCategories[$category]
        Invoke-BusBuddyTests -TestFilter $filter -Format "console"
    }
}

# Export functions for module use
if ($MyInvocation.InvocationName -ne '.') {
    # Script is being run directly
    Write-Host "Bus Buddy NUnit Testing Tools" -ForegroundColor Magenta
    Write-Host "Available functions:" -ForegroundColor White
    Write-Host "  Install-NUnitTools" -ForegroundColor Gray
    Write-Host "  Invoke-BusBuddyTests" -ForegroundColor Gray
    Write-Host "  Get-TestResults" -ForegroundColor Gray
    Write-Host "  New-TestCoverageReport" -ForegroundColor Gray
    Write-Host "  Test-BusBuddyComponents" -ForegroundColor Gray
}
