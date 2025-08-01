#!/usr/bin/env pwsh
# VS Code GitHub Actions Extension Compatibility Test

Write-Host "🚌 BusBuddy - GitHub Actions VS Code Extension Test" -ForegroundColor Cyan
Write-Host ""

# Test 1: Check if reusable workflow file exists and is valid
Write-Host "📋 Test 1: Reusable Workflow File" -ForegroundColor Yellow
$reusableWorkflow = ".github/workflows/build-reusable.yml"
if (Test-Path $reusableWorkflow) {
    Write-Host "  ✅ Found: $reusableWorkflow" -ForegroundColor Green

    # Check if it has simplified secrets definition
    $content = Get-Content $reusableWorkflow -Raw
    if ($content -match "secrets:\s*inherit" -or $content -match "required:\s*false") {
        Write-Host "  ✅ Compatible secrets definition found" -ForegroundColor Green
    } else {
        Write-Host "  ⚠️ May have compatibility issues with VS Code extension" -ForegroundColor Yellow
    }
} else {
    Write-Host "  ❌ Missing: $reusableWorkflow" -ForegroundColor Red
}

# Test 2: Check caller workflows
Write-Host ""
Write-Host "📋 Test 2: Caller Workflows" -ForegroundColor Yellow
$callerWorkflows = @(
    ".github/workflows/simplified-ci.yml",
    ".github/workflows/example-caller.yml"
)

foreach ($workflow in $callerWorkflows) {
    if (Test-Path $workflow) {
        Write-Host "  ✅ Found: $workflow" -ForegroundColor Green

        # Check if uses secrets: inherit
        $content = Get-Content $workflow -Raw
        if ($content -match "secrets:\s*inherit") {
            Write-Host "    ✅ Uses 'secrets: inherit' pattern" -ForegroundColor Green
        } else {
            Write-Host "    ⚠️ Uses explicit secrets pattern" -ForegroundColor Yellow
        }
    } else {
        Write-Host "  ⚠️ Not found: $workflow" -ForegroundColor Yellow
    }
}

# Test 3: YAML Syntax Validation
Write-Host ""
Write-Host "📋 Test 3: YAML Syntax Validation" -ForegroundColor Yellow
$allWorkflows = Get-ChildItem -Path ".github/workflows" -Filter "*.yml" -ErrorAction SilentlyContinue

foreach ($workflow in $allWorkflows) {
    try {
        # Simple YAML structure check
        $content = Get-Content $workflow.FullName -Raw
        if ($content -match "^name:" -and $content -match "^on:" -and $content -match "^jobs:") {
            Write-Host "  ✅ $($workflow.Name) - Basic structure valid" -ForegroundColor Green
        } else {
            Write-Host "  ⚠️ $($workflow.Name) - May have structure issues" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "  ❌ $($workflow.Name) - Syntax error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Test 4: VS Code Extension Recommendations
Write-Host ""
Write-Host "🔧 VS Code Extension Fix Instructions:" -ForegroundColor Cyan
Write-Host "  1. Update GitHub Actions extension (Ctrl+Shift+P → Extensions: Show Installed)" -ForegroundColor White
Write-Host "  2. Reload extension (Ctrl+Shift+P → Extensions: Reload)" -ForegroundColor White
Write-Host "  3. Reload VS Code window (Ctrl+Shift+P → Developer: Reload Window)" -ForegroundColor White
Write-Host "  4. Test by opening any workflow file and checking for syntax errors" -ForegroundColor White

Write-Host ""
Write-Host "✅ Compatibility test completed" -ForegroundColor Green
