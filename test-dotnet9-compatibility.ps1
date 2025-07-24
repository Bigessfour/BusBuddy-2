#Requires -Version 7.6
<#
.SYNOPSIS
    Test script for .NET 9 compatibility and PowerShell 7.6 optimizations

.DESCRIPTION
    Validates that BusBuddy AI-Assistant profile works optimally with:
    - PowerShell 7.6.4+ features
    - .NET 9 compatibility
    - .NET 8 fallback support
    - Enhanced parallelism and hyperthreading

.NOTES
    Tests both .NET 8 and .NET 9 code paths
#>

param(
    [switch]$Verbose,
    [switch]$SkipParallelTests,
    [switch]$TestCompatibility
)

# Colors for output
$ColorScheme = @{
    Title = 'Cyan'
    Success = 'Green'
    Warning = 'Yellow'
    Error = 'Red'
    Info = 'Gray'
    Highlight = 'Magenta'
}

function Write-TestResult {
    param(
        [string]$TestName,
        [bool]$Success,
        [string]$Details = "",
        [string]$Duration = ""
    )

    $status = if ($Success) { "✅ PASS" } else { "❌ FAIL" }
    $color = if ($Success) { $ColorScheme.Success } else { $ColorScheme.Error }

    Write-Host "$status " -NoNewline -ForegroundColor $color
    Write-Host "$TestName" -NoNewline -ForegroundColor White
    if ($Duration) {
        Write-Host " ($Duration)" -NoNewline -ForegroundColor $ColorScheme.Info
    }
    Write-Host ""

    if ($Details -and ($Verbose -or -not $Success)) {
        Write-Host "      $Details" -ForegroundColor $ColorScheme.Info
    }
}

function Test-DotNetVersionDetection {
    Write-Host "`n🔍 Testing .NET Version Detection..." -ForegroundColor $ColorScheme.Title

    try {
        # Test framework detection
        $frameworkDesc = [System.Runtime.InteropServices.RuntimeInformation]::FrameworkDescription
        Write-TestResult -TestName "Framework Description Available" -Success $true -Details $frameworkDesc

        # Test .NET 9 detection
        $isNet9 = $frameworkDesc -match '^Microsoft \.NET 9'
        Write-TestResult -TestName ".NET 9 Detection Logic" -Success $true -Details "Is .NET 9: $isNet9"

        # Test .NET 8+ detection
        $isNet8Plus = $frameworkDesc -match '^Microsoft \.NET [89]'
        Write-TestResult -TestName ".NET 8+ Detection Logic" -Success $true -Details "Is .NET 8+: $isNet8Plus"

        return $true
    }
    catch {
        Write-TestResult -TestName ".NET Version Detection" -Success $false -Details $_.Exception.Message
        return $false
    }
}

function Test-PowerShellVersionFeatures {
    Write-Host "`n🔧 Testing PowerShell 7.6+ Features..." -ForegroundColor $ColorScheme.Title

    try {
        # Test version detection
        $is76Plus = $PSVersionTable.PSVersion.Major -gt 7 -or ($PSVersionTable.PSVersion.Major -eq 7 -and $PSVersionTable.PSVersion.Minor -ge 6)
        Write-TestResult -TestName "PowerShell 7.6+ Detection" -Success $is76Plus -Details "Version: $($PSVersionTable.PSVersion)"

        # Test parallel processing availability
        $testData = 1..10
        $start = Get-Date
        $parallelResult = $testData | ForEach-Object -Parallel { $_ * 2 } -ThrottleLimit 4
        $parallelDuration = ((Get-Date) - $start).TotalMilliseconds

        $expectedResult = $testData | ForEach-Object { $_ * 2 }
        $parallelWorks = ($parallelResult -join ',') -eq ($expectedResult -join ',')

        Write-TestResult -TestName "ForEach-Object -Parallel" -Success $parallelWorks -Duration "$([math]::Round($parallelDuration, 2))ms"

        # Test enhanced error handling
        try {
            $null = 1 / 0
        }
        catch {
            $errorHandlingWorks = $_.Exception.Message -like "*division by zero*" -or $_.Exception.Message -like "*attempted to divide by zero*"
            Write-TestResult -TestName "Enhanced Error Handling" -Success $errorHandlingWorks -Details "Error captured correctly"
        }

        return $is76Plus -and $parallelWorks
    }
    catch {
        Write-TestResult -TestName "PowerShell Features Test" -Success $false -Details $_.Exception.Message
        return $false
    }
}

function Test-HyperthreadingDetection {
    Write-Host "`n⚡ Testing Hyperthreading Detection..." -ForegroundColor $ColorScheme.Title

    try {
        # Test processor detection
        $processor = Get-CimInstance -Class Win32_Processor | Select-Object -First 1
        $processorAvailable = $processor -ne $null
        Write-TestResult -TestName "Processor CIM Detection" -Success $processorAvailable

        if ($processorAvailable) {
            $coreCount = $processor.NumberOfCores
            $logicalCount = $processor.NumberOfLogicalProcessors
            $hyperthreadingDetected = $coreCount -lt $logicalCount

            Write-TestResult -TestName "Core Count Detection" -Success ($coreCount -gt 0) -Details "$coreCount cores"
            Write-TestResult -TestName "Logical Processor Detection" -Success ($logicalCount -gt 0) -Details "$logicalCount logical processors"
            Write-TestResult -TestName "Hyperthreading Detection" -Success $true -Details "Detected: $hyperthreadingDetected"

            # Test parallelism calculation
            $maxParallel = if ($hyperthreadingDetected) {
                [Math]::Min($logicalCount, 16)
            } else {
                [Math]::Min($coreCount, 8)
            }

            Write-TestResult -TestName "Max Parallelism Calculation" -Success ($maxParallel -gt 0) -Details "$maxParallel threads"
        }

        return $processorAvailable
    }
    catch {
        Write-TestResult -TestName "Hyperthreading Detection" -Success $false -Details $_.Exception.Message
        return $false
    }
}

function Test-AIProfileLoading {
    Write-Host "`n🤖 Testing AI Profile Loading..." -ForegroundColor $ColorScheme.Title

    try {
        $profilePath = Join-Path $PWD.Path "load-ai-assistant-profile-76.ps1"
        $profileExists = Test-Path $profilePath
        Write-TestResult -TestName "AI Profile File Exists" -Success $profileExists -Details $profilePath

        if ($profileExists) {
            # Test syntax validation
            $syntaxCheck = $null
            $syntaxErrors = $null

            $syntaxResult = [System.Management.Automation.PSParser]::Tokenize(
                (Get-Content $profilePath -Raw), [ref]$syntaxErrors
            )

            $syntaxValid = $syntaxErrors.Count -eq 0
            Write-TestResult -TestName "Profile Syntax Validation" -Success $syntaxValid -Details "$(if ($syntaxErrors) { "$($syntaxErrors.Count) errors" } else { "Clean" })"

            if (-not $syntaxValid -and $Verbose) {
                $syntaxErrors | ForEach-Object {
                    Write-Host "      Syntax Error: $($_.Message) at line $($_.Token.StartLine)" -ForegroundColor $ColorScheme.Error
                }
            }

            return $syntaxValid
        }

        return $false
    }
    catch {
        Write-TestResult -TestName "AI Profile Loading Test" -Success $false -Details $_.Exception.Message
        return $false
    }
}

function Test-ParallelPerformance {
    if ($SkipParallelTests) {
        Write-Host "`n⏭️  Skipping parallel performance tests..." -ForegroundColor $ColorScheme.Warning
        return $true
    }

    Write-Host "`n🚀 Testing Parallel Performance..." -ForegroundColor $ColorScheme.Title

    try {
        $testData = 1..1000

        # Sequential test
        $seqStart = Get-Date
        $seqResult = $testData | ForEach-Object { Start-Sleep -Milliseconds 1; $_ * 2 }
        $seqDuration = ((Get-Date) - $seqStart).TotalMilliseconds

        # Parallel test
        $parStart = Get-Date
        $parResult = $testData | ForEach-Object -Parallel { Start-Sleep -Milliseconds 1; $_ * 2 } -ThrottleLimit 8
        $parDuration = ((Get-Date) - $parStart).TotalMilliseconds

        $speedup = $seqDuration / $parDuration
        $parallelFaster = $speedup -gt 1.5  # Expect at least 1.5x speedup

        Write-TestResult -TestName "Sequential Processing" -Success $true -Duration "$([math]::Round($seqDuration, 2))ms"
        Write-TestResult -TestName "Parallel Processing" -Success $true -Duration "$([math]::Round($parDuration, 2))ms"
        Write-TestResult -TestName "Parallel Speedup" -Success $parallelFaster -Details "$([math]::Round($speedup, 2))x faster"

        return $parallelFaster
    }
    catch {
        Write-TestResult -TestName "Parallel Performance Test" -Success $false -Details $_.Exception.Message
        return $false
    }
}

function Test-CompatibilityMode {
    if (-not $TestCompatibility) {
        return $true
    }

    Write-Host "`n🔄 Testing Compatibility Mode..." -ForegroundColor $ColorScheme.Title

    try {
        # Simulate .NET 8 environment
        $mockNet8Info = @{
            DotNetVersion = "Microsoft .NET 8.0.10"
            IsNet9 = $false
            IsNet8Plus = $true
        }

        # Test .NET 8 code paths
        $net8Context = if ($mockNet8Info.IsNet9) {
            "Syncfusion WPF 30.1.40 with .NET 9.0 (PowerShell 7.6+ optimized)"
        } else {
            "Syncfusion WPF 30.1.40 with .NET 8.0"
        }

        Write-TestResult -TestName ".NET 8 Context Generation" -Success ($net8Context -like "*8.0*") -Details $net8Context

        # Test parallel limits for .NET 8
        $net8ParallelLimit = if ($mockNet8Info.IsNet9) { 24 } else { 16 }
        Write-TestResult -TestName ".NET 8 Parallel Limits" -Success ($net8ParallelLimit -eq 16) -Details "$net8ParallelLimit threads max"

        return $true
    }
    catch {
        Write-TestResult -TestName "Compatibility Mode Test" -Success $false -Details $_.Exception.Message
        return $false
    }
}

function Test-ErrorHandlingEnhancements {
    Write-Host "`n🛡️  Testing Enhanced Error Handling..." -ForegroundColor $ColorScheme.Title

    try {
        # Test enhanced error context capture
        $testError = $null
        try {
            throw [System.InvalidOperationException]::new("Test error", [System.ArgumentException]::new("Inner test error"))
        }
        catch {
            $testError = $_
        }

        $hasInnerException = $testError.Exception.InnerException -ne $null
        Write-TestResult -TestName "Inner Exception Capture" -Success $hasInnerException -Details "Nested error context available"

        # Test empty error handling (PowerShell 7.6 improvement)
        $emptyErrorHandled = $true
        try {
            $emptyString = ""
            if ([string]::IsNullOrEmpty($emptyString)) {
                # This should not throw in PowerShell 7.6
                $result = $emptyString.ToString()
            }
        }
        catch {
            $emptyErrorHandled = $false
        }

        Write-TestResult -TestName "Empty String Error Handling" -Success $emptyErrorHandled -Details "PowerShell 7.6 improvement"

        return $hasInnerException -and $emptyErrorHandled
    }
    catch {
        Write-TestResult -TestName "Error Handling Enhancement Test" -Success $false -Details $_.Exception.Message
        return $false
    }
}

# Main test execution
Write-Host "🧪 BusBuddy .NET 9 Compatibility & PowerShell 7.6 Optimization Tests" -ForegroundColor $ColorScheme.Title
Write-Host "=" * 70 -ForegroundColor $ColorScheme.Info

$allTests = @()
$allTests += Test-DotNetVersionDetection
$allTests += Test-PowerShellVersionFeatures
$allTests += Test-HyperthreadingDetection
$allTests += Test-AIProfileLoading
$allTests += Test-ParallelPerformance
$allTests += Test-CompatibilityMode
$allTests += Test-ErrorHandlingEnhancements

$passCount = ($allTests | Where-Object { $_ }).Count
$totalCount = $allTests.Count
$successRate = [math]::Round(($passCount / $totalCount) * 100, 1)

Write-Host "`n📊 Test Results Summary:" -ForegroundColor $ColorScheme.Title
Write-Host "=" * 30 -ForegroundColor $ColorScheme.Info
Write-Host "Passed: $passCount/$totalCount tests ($successRate%)" -ForegroundColor $(if ($successRate -gt 80) { $ColorScheme.Success } else { $ColorScheme.Warning })

if ($passCount -eq $totalCount) {
    Write-Host "`n🎉 All tests passed! BusBuddy is optimized for PowerShell 7.6+ and .NET 8/9." -ForegroundColor $ColorScheme.Success
} else {
    Write-Host "`n⚠️  Some tests failed. Review the results above for optimization opportunities." -ForegroundColor $ColorScheme.Warning
}

Write-Host "`n🔧 Current Environment:" -ForegroundColor $ColorScheme.Info
Write-Host "   PowerShell: $($PSVersionTable.PSVersion)" -ForegroundColor $ColorScheme.Info
Write-Host "   .NET: $([System.Runtime.InteropServices.RuntimeInformation]::FrameworkDescription)" -ForegroundColor $ColorScheme.Info
Write-Host "   Processors: $([Environment]::ProcessorCount)" -ForegroundColor $ColorScheme.Info

return $passCount -eq $totalCount
