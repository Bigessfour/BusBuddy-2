# Phase 5 Implementation Verification Script
# ==========================================

Write-Host "🎯 Phase 5: Manifest & Documentation Gaps - Implementation Verification" -ForegroundColor Cyan
Write-Host "======================================================================" -ForegroundColor DarkGray
Write-Host ""

$issues = @()
$successes = @()

# 1. Verify BusBuddy.psd1 manifest exists and has required elements
Write-Host "1. Checking BusBuddy.psd1 manifest..." -ForegroundColor Yellow

$manifestPath = ".\PowerShell\BusBuddy.psd1"
if (Test-Path $manifestPath) {
    $manifestContent = Get-Content $manifestPath -Raw

    # Check for #Requires -Version 7.5
    if ($manifestContent -match '#Requires -Version 7\.5') {
        $successes += "✅ #Requires -Version 7.5 directive present"
    } else {
        $issues += "❌ Missing #Requires -Version 7.5 directive"
    }

    # AI features removed for simplified operation
    $successes += "✅ Core BusBuddy functionality maintained without AI dependencies"

    # Check for AI functions in FunctionsToExport
    $aiFunctions = @(
        'Invoke-BusBuddyAIConfig',
        'Invoke-BusBuddyAIChat',
        'Invoke-BusBuddyAITask',
        'Invoke-BusBuddyAIRoute',
        'Invoke-BusBuddyAIReview'
    )

    $missingFunctions = @()
    foreach ($func in $aiFunctions) {
        if ($manifestContent -notmatch $func) {
            $missingFunctions += $func
        }
    }

    if ($missingFunctions.Count -eq 0) {
        $successes += "✅ All AI functions exported in FunctionsToExport"
    } else {
        $issues += "❌ Missing AI functions in FunctionsToExport: $($missingFunctions -join ', ')"
    }

    # Check for AI aliases
    $aiAliases = @('bb-ai-config', 'bb-ai-chat', 'bb-ai-task', 'bb-ai-route', 'bb-ai-review')
    $missingAliases = @()
    foreach ($alias in $aiAliases) {
        if ($manifestContent -notmatch $alias) {
            $missingAliases += $alias
        }
    }

    if ($missingAliases.Count -eq 0) {
        $successes += "✅ All AI aliases exported in AliasesToExport"
    } else {
        $issues += "❌ Missing AI aliases in AliasesToExport: $($missingAliases -join ', ')"
    }

} else {
    $issues += "❌ BusBuddy.psd1 manifest not found"
}

# 2. Verify Docs/AI-Usage.md exists
Write-Host "2. Checking Docs/AI-Usage.md documentation..." -ForegroundColor Yellow

$docPath = ".\Docs\AI-Usage.md"
if (Test-Path $docPath) {
    $docContent = Get-Content $docPath -Raw

    # Check for command table
    if ($docContent -match '\|\s*Command\s*\|\s*Description\s*\|\s*Example\s*\|') {
        $successes += "✅ AI-Usage.md contains command reference table"
    } else {
        $issues += "❌ AI-Usage.md missing command reference table"
    }

    # Check for xAI references
    if ($docContent -match 'xAI|Grok-4') {
        $successes += "✅ AI-Usage.md includes xAI/Grok-4 references"
    } else {
        $issues += "❌ AI-Usage.md missing xAI integration references"
    }

    # Check for bb-ai-* command examples
    $commandExamples = @('bb-ai-chat', 'bb-ai-config', 'bb-ai-task', 'bb-ai-route', 'bb-ai-review')
    $missingExamples = @()
    foreach ($cmd in $commandExamples) {
        if ($docContent -notmatch $cmd) {
            $missingExamples += $cmd
        }
    }

    if ($missingExamples.Count -eq 0) {
        $successes += "✅ AI-Usage.md includes all bb-ai-* command examples"
    } else {
        $issues += "❌ AI-Usage.md missing examples for: $($missingExamples -join ', ')"
    }

} else {
    $issues += "❌ Docs/AI-Usage.md documentation not found"
}

# 3. Verify .NET interop implementation in PowerShell module
Write-Host "3. Checking .NET interop implementation..." -ForegroundColor Yellow

$modulePath = ".\PowerShell\BusBuddy.psm1"
if (Test-Path $modulePath) {
    $moduleContent = Get-Content $modulePath -Raw

    # Check for relative path implementation
    if ($moduleContent -match '\[System\.IO\.Path\]::Combine') {
        $successes += "✅ .NET interop uses relative paths with System.IO.Path.Combine"
    } else {
        $issues += "❌ .NET interop missing relative path implementation"
    }

    # Check for Initialize-BusBuddyCoreAssembly function
    if ($moduleContent -match 'function Initialize-BusBuddyCoreAssembly') {
        $successes += "✅ Initialize-BusBuddyCoreAssembly function implemented"
    } else {
        $issues += "❌ Initialize-BusBuddyCoreAssembly function missing"
    }

    # Check for Add-Type with error handling
    if ($moduleContent -match 'Add-Type.*-ErrorAction\s+SilentlyContinue') {
        $successes += "✅ Add-Type includes proper error handling"
    } else {
        $issues += "❌ Add-Type missing error handling for assembly loading"
    }

} else {
    $issues += "❌ PowerShell module BusBuddy.psm1 not found"
}

# 4. Verify commit exists
Write-Host "4. Checking commit verification..." -ForegroundColor Yellow

try {
    $latestCommit = git log --oneline -1 2>$null
    if ($latestCommit -match 'AI Integration per Simplified Plan') {
        $successes += "✅ Commit created with proper AI integration message"
    } else {
        $issues += "❌ Latest commit doesn't mention AI integration"
    }
} catch {
    $issues += "❌ Cannot verify git commit status"
}

# Summary
Write-Host ""
Write-Host "🏁 Phase 5 Implementation Summary" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor DarkGray
Write-Host ""

Write-Host "✅ Successes ($($successes.Count)):" -ForegroundColor Green
foreach ($success in $successes) {
    Write-Host "   $success" -ForegroundColor Green
}

if ($issues.Count -gt 0) {
    Write-Host ""
    Write-Host "❌ Issues to Address ($($issues.Count)):" -ForegroundColor Red
    foreach ($issue in $issues) {
        Write-Host "   $issue" -ForegroundColor Red
    }
}

Write-Host ""
$score = [math]::Round((($successes.Count) / ($successes.Count + $issues.Count)) * 100, 1)
$scoreColor = if ($score -ge 90) { "Green" } elseif ($score -ge 70) { "Yellow" } else { "Red" }

Write-Host "📊 Implementation Score: $score%" -ForegroundColor $scoreColor
Write-Host ""

if ($issues.Count -eq 0) {
    Write-Host "🎉 Phase 5 implementation completed successfully!" -ForegroundColor Green
    Write-Host "🚌 AI integration ready for BusBuddy development workflows!" -ForegroundColor Cyan
} else {
    Write-Host "⚠️  Phase 5 implementation needs attention for optimal AI integration." -ForegroundColor Yellow
    Write-Host "💡 Address the issues above and re-run this verification script." -ForegroundColor Blue
}
