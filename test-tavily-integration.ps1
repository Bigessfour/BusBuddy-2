#Requires -Version 7.5
<#
.SYNOPSIS
    Test script for BusBuddy Tavily Expert integration

.DESCRIPTION
    Tests the integrated Tavily Expert search functionality within the BusBuddy PowerShell environment.
    Validates API connectivity, function loading, and search capabilities.

.PARAMETER TestMode
    Type of test to perform

.PARAMETER Quick
    Perform quick validation only

.EXAMPLE
    .\test-tavily-integration.ps1 -TestMode Basic

.EXAMPLE
    .\test-tavily-integration.ps1 -TestMode Full -Quick
#>
param(
    [ValidateSet('Basic', 'Search', 'Workflow', 'Full')]
    [string]$TestMode = 'Basic',

    [switch]$Quick
)

Write-Host "🧪 BusBuddy Tavily Integration Test" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════" -ForegroundColor DarkCyan
Write-Host ""

$testResults = @{
    EnvironmentCheck = $false
    FunctionLoad = $false
    ApiConnectivity = $false
    SearchTest = $false
    WorkflowTest = $false
    OverallSuccess = $false
}

# Test 1: Environment Check
Write-Host "🔍 Test 1: Environment Validation" -ForegroundColor Yellow
Write-Host "─────────────────────────────────" -ForegroundColor Yellow

try {
    # Check PowerShell version
    if ($PSVersionTable.PSVersion -ge [version]"7.5.0") {
        Write-Host "✅ PowerShell 7.5+: $($PSVersionTable.PSVersion)" -ForegroundColor Green
    } else {
        Write-Host "⚠️ PowerShell version: $($PSVersionTable.PSVersion) (7.5+ recommended)" -ForegroundColor Yellow
    }

    # Check API key
    $apiKey = $env:TAVILY_API_KEY
    if ($apiKey -and $apiKey -ne "tvly-EXAMPLE-KEY") {
        Write-Host "✅ TAVILY_API_KEY: Set (length: $($apiKey.Length))" -ForegroundColor Green
        $testResults.EnvironmentCheck = $true
    } else {
        Write-Host "❌ TAVILY_API_KEY: Not set or using example key" -ForegroundColor Red
        Write-Host "   Set with: `$env:TAVILY_API_KEY = 'your-key'" -ForegroundColor Gray
    }

    # Check project root
    if (Test-Path "BusBuddy.sln") {
        Write-Host "✅ BusBuddy project root detected" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Not in BusBuddy project root" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Environment check failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 2: Function Loading
Write-Host "🔧 Test 2: Function Loading" -ForegroundColor Yellow
Write-Host "──────────────────────────" -ForegroundColor Yellow

try {
    # Check if module is loaded
    $busBuddyModule = Get-Module BusBuddy
    if ($busBuddyModule) {
        Write-Host "✅ BusBuddy module loaded: v$($busBuddyModule.Version)" -ForegroundColor Green
    } else {
        Write-Host "⚠️ BusBuddy module not loaded, attempting to load..." -ForegroundColor Yellow
        if (Test-Path ".\PowerShell\BusBuddy PowerShell Environment\Modules\BusBuddy\BusBuddy.psm1") {
            Import-Module ".\PowerShell\BusBuddy PowerShell Environment\Modules\BusBuddy\BusBuddy.psm1" -Force
            Write-Host "✅ BusBuddy module loaded successfully" -ForegroundColor Green
        }
    }

    # Check for Tavily functions
    $tavilyFunctions = @(
        'Invoke-BusBuddyTavilySearch',
        'Start-BusBuddyAIWorkflow',
        'Get-BusBuddyAIAssistance'
    )

    $loadedFunctions = 0
    foreach ($func in $tavilyFunctions) {
        if (Get-Command $func -ErrorAction SilentlyContinue) {
            Write-Host "✅ Function available: $func" -ForegroundColor Green
            $loadedFunctions++
        } else {
            Write-Host "❌ Function missing: $func" -ForegroundColor Red
        }
    }

    # Check aliases
    $aliases = @('bb-tavily-search', 'bb-search', 'bb-ai-workflow', 'bb-ai-help')
    $availableAliases = 0
    foreach ($alias in $aliases) {
        if (Get-Alias $alias -ErrorAction SilentlyContinue) {
            Write-Host "✅ Alias available: $alias" -ForegroundColor Green
            $availableAliases++
        } else {
            Write-Host "❌ Alias missing: $alias" -ForegroundColor Red
        }
    }

    if ($loadedFunctions -eq $tavilyFunctions.Count -and $availableAliases -eq $aliases.Count) {
        $testResults.FunctionLoad = $true
        Write-Host "✅ All functions and aliases loaded successfully" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Some functions/aliases missing ($loadedFunctions/$($tavilyFunctions.Count) functions, $availableAliases/$($aliases.Count) aliases)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Function loading test failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 3: API Connectivity (only if API key is available)
if ($testResults.EnvironmentCheck) {
    Write-Host "🌐 Test 3: API Connectivity" -ForegroundColor Yellow
    Write-Host "───────────────────────────" -ForegroundColor Yellow

    try {
        # Simple connectivity test
        $headers = @{
            'Content-Type' = 'application/json'
        }

        $testBody = @{
            api_key = $env:TAVILY_API_KEY
            query = "test connectivity"
            max_results = 1
        } | ConvertTo-Json

        Write-Host "🔄 Testing Tavily API connectivity..." -ForegroundColor Gray
        $response = Invoke-RestMethod -Uri "https://api.tavily.com/search" -Method Post -Body $testBody -Headers $headers -TimeoutSec 10

        if ($response -and $response.results) {
            Write-Host "✅ API connectivity successful" -ForegroundColor Green
            $testResults.ApiConnectivity = $true
        } else {
            Write-Host "⚠️ API responded but no results returned" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "❌ API connectivity failed: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Message -like "*401*" -or $_.Exception.Message -like "*403*") {
            Write-Host "   💡 Check your API key" -ForegroundColor Blue
        }
    }
} else {
    Write-Host "⏭️ Test 3: Skipped (API key not available)" -ForegroundColor Gray
}

Write-Host ""

# Test 4: Search Test (only if not Quick mode)
if (-not $Quick -and $testResults.FunctionLoad -and $testResults.ApiConnectivity) {
    Write-Host "🔍 Test 4: Search Functionality" -ForegroundColor Yellow
    Write-Host "──────────────────────────────" -ForegroundColor Yellow

    try {
        Write-Host "🔄 Testing search with query: 'PowerShell 7.5 features'..." -ForegroundColor Gray

        if (Get-Command Invoke-BusBuddyTavilySearch -ErrorAction SilentlyContinue) {
            $searchResult = Invoke-BusBuddyTavilySearch -Query "PowerShell 7.5 features" -MaxResults 2 -OutputFormat Json

            if ($searchResult -and $searchResult.Results.Count -gt 0) {
                Write-Host "✅ Search test successful: $($searchResult.Results.Count) results returned" -ForegroundColor Green
                $testResults.SearchTest = $true
            } else {
                Write-Host "⚠️ Search completed but no results" -ForegroundColor Yellow
            }
        } else {
            Write-Host "❌ Search function not available" -ForegroundColor Red
        }
    } catch {
        Write-Host "❌ Search test failed: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "⏭️ Test 4: Skipped (Quick mode or prerequisites not met)" -ForegroundColor Gray
}

Write-Host ""

# Test 5: Workflow Test (only in Full mode)
if ($TestMode -eq 'Full' -and $testResults.FunctionLoad) {
    Write-Host "🔧 Test 5: AI Workflow" -ForegroundColor Yellow
    Write-Host "─────────────────────" -ForegroundColor Yellow

    try {
        if (Get-Command Start-BusBuddyAIWorkflow -ErrorAction SilentlyContinue) {
            Write-Host "✅ AI Workflow function available" -ForegroundColor Green
            Write-Host "🔄 Testing workflow commands..." -ForegroundColor Gray

            # Test workflow help
            Start-BusBuddyAIWorkflow -WorkflowType Research | Out-Null

            $testResults.WorkflowTest = $true
            Write-Host "✅ AI Workflow test completed" -ForegroundColor Green
        } else {
            Write-Host "❌ AI Workflow function not available" -ForegroundColor Red
        }
    } catch {
        Write-Host "❌ Workflow test failed: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "⏭️ Test 5: Skipped (not in Full mode or prerequisites not met)" -ForegroundColor Gray
}

Write-Host ""

# Summary
Write-Host "📊 Test Summary" -ForegroundColor Cyan
Write-Host "═══════════════" -ForegroundColor DarkCyan

$passedTests = 0
$totalTests = 0

foreach ($test in $testResults.Keys) {
    if ($test -ne 'OverallSuccess') {
        $totalTests++
        if ($testResults[$test]) {
            $passedTests++
            Write-Host "✅ $test" -ForegroundColor Green
        } else {
            Write-Host "❌ $test" -ForegroundColor Red
        }
    }
}

$successRate = if ($totalTests -gt 0) { [math]::Round(($passedTests / $totalTests) * 100, 1) } else { 0 }
$testResults.OverallSuccess = $successRate -ge 80

Write-Host ""
Write-Host "Success Rate: $successRate% ($passedTests/$totalTests)" -ForegroundColor $(if ($testResults.OverallSuccess) { 'Green' } else { 'Red' })

if ($testResults.OverallSuccess) {
    Write-Host ""
    Write-Host "🎉 Tavily Integration Test PASSED!" -ForegroundColor Green
    Write-Host "Ready to use:" -ForegroundColor Yellow
    Write-Host "  bb-tavily-search 'your query here'" -ForegroundColor Cyan
    Write-Host "  bb-ai-workflow -WorkflowType Research" -ForegroundColor Cyan
    Write-Host "  bb-ai-help 'development task'" -ForegroundColor Cyan
} else {
    Write-Host ""
    Write-Host "⚠️ Integration test completed with issues" -ForegroundColor Yellow
    Write-Host "Please review the failed tests above" -ForegroundColor Gray
}

Write-Host ""
return $testResults
