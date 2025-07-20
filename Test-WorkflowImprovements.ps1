#Requires -Version 7.0
<#
.SYNOPSIS
    Test Bus Buddy Workflow Improvements

.DESCRIPTION
    Validates all the new streamlined workflow features and improvements
    Tests aliases, functions, and advanced workflow integration

.NOTES
    Run this script to verify that all workflow improvements are working correctly
#>

param(
    [switch]$Detailed,
    [switch]$SkipFunctionTests,
    [switch]$SkipAdvancedTests
)

Write-Host "🧪 Testing Bus Buddy Workflow Improvements" -ForegroundColor Blue
Write-Host "==========================================" -ForegroundColor Blue

$testResults = @{
    PassedTests = 0
    FailedTests = 0
    SkippedTests = 0
    Details = @()
}

function Test-Feature {
    param(
        [string]$Name,
        [scriptblock]$TestScript,
        [string]$Category = "General"
    )

    Write-Host "`n🔍 Testing: $Name" -ForegroundColor Cyan

    try {
        $result = & $TestScript
        if ($result) {
            Write-Host "  ✅ PASS" -ForegroundColor Green
            $testResults.PassedTests++
            $testResults.Details += [PSCustomObject]@{
                Name = $Name
                Category = $Category
                Status = "PASS"
                Error = $null
            }
        } else {
            Write-Host "  ❌ FAIL" -ForegroundColor Red
            $testResults.FailedTests++
            $testResults.Details += [PSCustomObject]@{
                Name = $Name
                Category = $Category
                Status = "FAIL"
                Error = "Test returned false"
            }
        }
    }
    catch {
        Write-Host "  ❌ FAIL: $($_.Exception.Message)" -ForegroundColor Red
        $testResults.FailedTests++
        $testResults.Details += [PSCustomObject]@{
            Name = $Name
            Category = $Category
            Status = "FAIL"
            Error = $_.Exception.Message
        }
    }
}

function Test-CommandExists {
    param([string]$CommandName)

    return (Get-Command $CommandName -ErrorAction SilentlyContinue) -ne $null
}

function Test-AliasExists {
    param([string]$AliasName)

    return (Get-Alias $AliasName -ErrorAction SilentlyContinue) -ne $null
}

# Test 1: Core Build Aliases
Test-Feature "bb-build alias exists" {
    Test-AliasExists "bb-build"
} -Category "Aliases"

Test-Feature "bb-run alias exists" {
    Test-AliasExists "bb-run"
} -Category "Aliases"

Test-Feature "bb-test alias exists" {
    Test-AliasExists "bb-test"
} -Category "Aliases"

Test-Feature "bb-clean alias exists" {
    Test-AliasExists "bb-clean"
} -Category "Aliases"

# Test 2: UI Workflow Aliases
Test-Feature "bb-ui-cycle alias exists" {
    Test-AliasExists "bb-ui-cycle"
} -Category "Aliases"

Test-Feature "bb-theme-check alias exists" {
    Test-AliasExists "bb-theme-check"
} -Category "Aliases"

Test-Feature "bb-validate-ui alias exists" {
    Test-AliasExists "bb-validate-ui"
} -Category "Aliases"

# Test 3: Log Monitoring Aliases
Test-Feature "bb-logs-tail alias exists" {
    Test-AliasExists "bb-logs-tail"
} -Category "Aliases"

Test-Feature "bb-logs-errors alias exists" {
    Test-AliasExists "bb-logs-errors"
} -Category "Aliases"

Test-Feature "bb-logs-ui alias exists" {
    Test-AliasExists "bb-logs-ui"
} -Category "Aliases"

# Test 4: Function Tests (if not skipped)
if (-not $SkipFunctionTests) {
    Test-Feature "Invoke-BusBuddyBuild function exists" {
        Test-CommandExists "Invoke-BusBuddyBuild"
    } -Category "Functions"

    Test-Feature "Invoke-BusBuddyRun function exists" {
        Test-CommandExists "Invoke-BusBuddyRun"
    } -Category "Functions"

    Test-Feature "Watch-BusBuddyLogs function exists" {
        Test-CommandExists "Watch-BusBuddyLogs"
    } -Category "Functions"

    Test-Feature "Get-BusBuddyProjectRoot function works" {
        $root = Get-BusBuddyProjectRoot
        return ($root -ne $null) -and (Test-Path $root)
    } -Category "Functions"
}

# Test 5: Advanced Workflow Tests (if not skipped)
if (-not $SkipAdvancedTests) {
    Test-Feature "bb-dev-session alias exists" {
        Test-AliasExists "bb-dev-session"
    } -Category "Advanced"

    Test-Feature "bb-quick-test alias exists" {
        Test-AliasExists "bb-quick-test"
    } -Category "Advanced"

    Test-Feature "bb-diagnostic alias exists" {
        Test-AliasExists "bb-diagnostic"
    } -Category "Advanced"

    Test-Feature "bb-hot-reload alias exists" {
        Test-AliasExists "bb-hot-reload"
    } -Category "Advanced"

    Test-Feature "Start-BusBuddyDevSession function exists" {
        Test-CommandExists "Start-BusBuddyDevSession"
    } -Category "Advanced"

    Test-Feature "Invoke-BusBuddyFullDiagnostic function exists" {
        Test-CommandExists "Invoke-BusBuddyFullDiagnostic"
    } -Category "Advanced"
}

# Test 6: Script File Tests
Test-Feature "Enhanced log monitoring script exists" {
    $scriptPath = Join-Path $PSScriptRoot "Tools\Scripts\Watch-BusBuddyLogs.ps1"
    Test-Path $scriptPath
} -Category "Scripts"

Test-Feature "Theme check script exists" {
    $scriptPath = Join-Path $PSScriptRoot "Tools\Scripts\bb-theme-check.ps1"
    Test-Path $scriptPath
} -Category "Scripts"

Test-Feature "Advanced workflows script exists" {
    $scriptPath = Join-Path $PSScriptRoot "BusBuddy-Advanced-Workflows.ps1"
    Test-Path $scriptPath
} -Category "Scripts"

# Test 7: Project Structure Tests
Test-Feature "Bus Buddy solution file exists" {
    $root = Get-BusBuddyProjectRoot
    if ($root) {
        Test-Path (Join-Path $root "BusBuddy.sln")
    } else {
        $false
    }
} -Category "Project"

Test-Feature "WPF project exists" {
    $root = Get-BusBuddyProjectRoot
    if ($root) {
        Test-Path (Join-Path $root "BusBuddy.WPF\BusBuddy.WPF.csproj")
    } else {
        $false
    }
} -Category "Project"

Test-Feature "Tools directory exists" {
    $root = Get-BusBuddyProjectRoot
    if ($root) {
        Test-Path (Join-Path $root "Tools")
    } else {
        $false
    }
} -Category "Project"

# Test 8: PowerShell 7.5.2 Feature Tests
Test-Feature "PowerShell version is 7.0+" {
    $PSVersionTable.PSVersion.Major -ge 7
} -Category "Environment"

Test-Feature "Parallel processing available" {
    $null -ne (Get-Command "ForEach-Object" | Where-Object { $_.Parameters.ContainsKey("Parallel") })
} -Category "Environment"

# Display Results
Write-Host "`n📊 TEST RESULTS SUMMARY" -ForegroundColor Blue
Write-Host "======================" -ForegroundColor Blue
Write-Host "Total Tests: $($testResults.PassedTests + $testResults.FailedTests)" -ForegroundColor White
Write-Host "Passed: $($testResults.PassedTests)" -ForegroundColor Green
Write-Host "Failed: $($testResults.FailedTests)" -ForegroundColor $(if($testResults.FailedTests -gt 0) { "Red" } else { "Green" })
Write-Host "Success Rate: $([math]::Round($testResults.PassedTests / ($testResults.PassedTests + $testResults.FailedTests) * 100, 1))%" -ForegroundColor $(if($testResults.FailedTests -eq 0) { "Green" } else { "Yellow" })

if ($Detailed -or $testResults.FailedTests -gt 0) {
    Write-Host "`n📋 DETAILED RESULTS" -ForegroundColor Blue
    Write-Host "==================" -ForegroundColor Blue

    $testResults.Details | Group-Object Category | ForEach-Object {
        Write-Host "`n🏷️ $($_.Name) Tests:" -ForegroundColor Magenta
        $_.Group | ForEach-Object {
            $color = switch ($_.Status) {
                "PASS" { "Green" }
                "FAIL" { "Red" }
                "SKIP" { "Yellow" }
            }
            $icon = switch ($_.Status) {
                "PASS" { "✅" }
                "FAIL" { "❌" }
                "SKIP" { "⏭️" }
            }
            Write-Host "  $icon $($_.Name)" -ForegroundColor $color
            if ($_.Error) {
                Write-Host "     Error: $($_.Error)" -ForegroundColor Gray
            }
        }
    }
}

# Recommendations
Write-Host "`n💡 WORKFLOW RECOMMENDATIONS" -ForegroundColor Yellow
Write-Host "===========================" -ForegroundColor Yellow

if ($testResults.FailedTests -eq 0) {
    Write-Host "🎉 All tests passed! Your workflow is ready for streamlined development." -ForegroundColor Green
    Write-Host ""
    Write-Host "🚀 QUICK START COMMANDS:" -ForegroundColor Cyan
    Write-Host "   bb-build && bb-run     - Quick build and test cycle" -ForegroundColor Gray
    Write-Host "   bb-ui-cycle            - Complete UI beautification workflow" -ForegroundColor Gray
    Write-Host "   bb-logs-tail -Follow   - Monitor application logs" -ForegroundColor Gray
    Write-Host "   bb-dev-session         - Start full development session" -ForegroundColor Gray
} else {
    Write-Host "⚠️ Some tests failed. Please check the following:" -ForegroundColor Yellow

    $failedTests = $testResults.Details | Where-Object { $_.Status -eq "FAIL" }
    $failedTests | ForEach-Object {
        Write-Host "   • $($_.Name): $($_.Error)" -ForegroundColor Red
    }

    Write-Host ""
    Write-Host "🔧 TROUBLESHOOTING:" -ForegroundColor Cyan
    Write-Host "   1. Ensure you're in the Bus Buddy project directory" -ForegroundColor Gray
    Write-Host "   2. Reload the PowerShell profile: . .\BusBuddy-PowerShell-Profile.ps1" -ForegroundColor Gray
    Write-Host "   3. Check that all required scripts are in the Tools\Scripts directory" -ForegroundColor Gray
}

Write-Host ""

# Return success status
return $testResults.FailedTests -eq 0
