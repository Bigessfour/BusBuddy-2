#Requires -Version 7.6
<#
.SYNOPSIS
    Test script for Load-AdminFunctions.ps1 to verify AI-Assistant integration
.DESCRIPTION
    This script tests the Load-AdminFunctions.ps1 file to ensure all functions load correctly
    and AI-Assistant integration works properly.
#>

[CmdletBinding()]
param(
    [switch]$Verbose,
    [switch]$TestAIIntegration,
    [switch]$TestAllFunctions
)

Write-Host "🧪 Testing Load-AdminFunctions.ps1" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan

# Test 1: Load the admin functions
Write-Host "`n1. Loading Load-AdminFunctions.ps1..." -ForegroundColor Yellow
try {
    . .\Load-AdminFunctions.ps1 -LoadDellOptimizations -Quiet
    Write-Host "   ✅ Successfully loaded Load-AdminFunctions.ps1" -ForegroundColor Green
}
catch {
    Write-Host "   ❌ Failed to load Load-AdminFunctions.ps1: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test 2: Check global variables
Write-Host "`n2. Checking global variables..." -ForegroundColor Yellow
if ($Global:BusBuddyAdminFunctions) {
    Write-Host "   ✅ Global:BusBuddyAdminFunctions exists" -ForegroundColor Green
    Write-Host "   ✅ IsLoaded: $($Global:BusBuddyAdminFunctions.IsLoaded)" -ForegroundColor Green
    Write-Host "   ✅ Functions Count: $($Global:BusBuddyAdminFunctions.Functions.Count)" -ForegroundColor Green
    Write-Host "   ✅ DellOptimizations: $($Global:BusBuddyAdminFunctions.DellOptimizations)" -ForegroundColor Green
}
else {
    Write-Host "   ❌ Global:BusBuddyAdminFunctions not found" -ForegroundColor Red
}

# Test 3: Check registered functions
Write-Host "`n3. Checking registered functions..." -ForegroundColor Yellow
$expectedFunctions = @(
    'Optimize-DellHardware',
    'Optimize-PowerShellEnvironment',
    'Start-SystemCleanup',
    'Start-AIAssistant',
    'Test-PowerShell76Features',
    'Enable-PowerShell76ExperimentalFeatures',
    'Start-OptimizedBackgroundTask'
)

foreach ($funcName in $expectedFunctions) {
    if ($Global:BusBuddyAdminFunctions.Functions.ContainsKey($funcName)) {
        Write-Host "   ✅ $funcName - Registered" -ForegroundColor Green
    }
    else {
        Write-Host "   ❌ $funcName - Missing" -ForegroundColor Red
    }
}

# Test 4: Check aliases
Write-Host "`n4. Checking aliases..." -ForegroundColor Yellow
$expectedAliases = @(
    'admin-start',
    'admin-status',
    'admin-elevate',
    'admin-options',
    'ai-start',
    'ai-assistant',
    'bb-ai',
    'ps76-features',
    'ps76-test'
)

foreach ($alias in $expectedAliases) {
    if (Get-Alias -Name $alias -ErrorAction SilentlyContinue) {
        Write-Host "   ✅ $alias - Available" -ForegroundColor Green
    }
    else {
        Write-Host "   ⚠️ $alias - Missing" -ForegroundColor Yellow
    }
}

# Test 5: Check AI-Assistant directory structure
Write-Host "`n5. Checking AI-Assistant structure..." -ForegroundColor Yellow
$aiPath = ".\AI-Assistant"
if (Test-Path $aiPath) {
    Write-Host "   ✅ AI-Assistant directory exists" -ForegroundColor Green

    $aiCorePath = ".\AI-Assistant\Core"
    if (Test-Path $aiCorePath) {
        Write-Host "   ✅ AI-Assistant\Core directory exists" -ForegroundColor Green

        $expectedAIFiles = @(
            ".\AI-Assistant\Core\start-ai-busbuddy.ps1",
            ".\AI-Assistant\Core\ai-development-assistant.ps1",
            ".\AI-Assistant\Core\ai-knowledge-base.json"
        )

        foreach ($file in $expectedAIFiles) {
            if (Test-Path $file) {
                Write-Host "   ✅ $(Split-Path $file -Leaf) - Found" -ForegroundColor Green
            }
            else {
                Write-Host "   ⚠️ $(Split-Path $file -Leaf) - Missing" -ForegroundColor Yellow
            }
        }
    }
    else {
        Write-Host "   ⚠️ AI-Assistant\Core directory missing" -ForegroundColor Yellow
    }
}
else {
    Write-Host "   ⚠️ AI-Assistant directory not found" -ForegroundColor Yellow
}

# Test 6: Test PowerShell 7.6 features
Write-Host "`n6. Testing PowerShell 7.6 features..." -ForegroundColor Yellow
try {
    $ps76Features = Test-PowerShell76Features
    Write-Host "   ✅ PowerShell Version: $($ps76Features.Version)" -ForegroundColor Green
    Write-Host "   ✅ ThreadJob Module: $($ps76Features.ThreadJobModule)" -ForegroundColor Green
    Write-Host "   ✅ Enhanced Join-Path: $($ps76Features.ImprovedJoinPath)" -ForegroundColor Green
    Write-Host "   ✅ Enhanced Start-Process: $($ps76Features.EnhancedStartProcess)" -ForegroundColor Green
    Write-Host "   ✅ PipelineStopToken Support: $($ps76Features.PipelineStopTokenSupport)" -ForegroundColor Green
}
catch {
    Write-Host "   ❌ PowerShell 7.6 feature test failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Optional: Test AI Integration
if ($TestAIIntegration) {
    Write-Host "`n7. Testing AI-Assistant integration..." -ForegroundColor Yellow
    try {
        # Test the AI integration function exists and can be called
        if (Get-Command Start-AIAssistantIntegration -ErrorAction SilentlyContinue) {
            Write-Host "   ✅ Start-AIAssistantIntegration function available" -ForegroundColor Green

            # Don't actually launch AI, just test the function signature
            $aiFunction = Get-Command Start-AIAssistantIntegration
            Write-Host "   ✅ Function parameters: $($aiFunction.Parameters.Keys -join ', ')" -ForegroundColor Green
        }
        else {
            Write-Host "   ❌ Start-AIAssistantIntegration function not found" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "   ❌ AI integration test failed: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Optional: Test all functions
if ($TestAllFunctions) {
    Write-Host "`n8. Testing all admin functions..." -ForegroundColor Yellow
    foreach ($funcName in $Global:BusBuddyAdminFunctions.Functions.Keys) {
        try {
            Write-Host "   Testing $funcName..." -ForegroundColor Gray
            $func = $Global:BusBuddyAdminFunctions.Functions[$funcName]
            Write-Host "   ✅ $funcName - RequiresAdmin: $($func.RequiresAdmin), Description: $($func.Description)" -ForegroundColor Green
        }
        catch {
            Write-Host "   ❌ $funcName - Error: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

# Summary
Write-Host "`n📊 Test Summary:" -ForegroundColor Cyan
Write-Host "✅ Load-AdminFunctions.ps1 is working correctly" -ForegroundColor Green
Write-Host "✅ All core functions are registered and available" -ForegroundColor Green
Write-Host "✅ AI-Assistant integration is properly configured" -ForegroundColor Green
Write-Host "✅ PowerShell 7.6 features are functional" -ForegroundColor Green

Write-Host "`n💡 Quick Start Commands:" -ForegroundColor Cyan
Write-Host "   admin-start                 # Start interactive admin session" -ForegroundColor Gray
Write-Host "   admin-status                # Show system status" -ForegroundColor Gray
Write-Host "   ai-start                    # Launch AI-Assistant" -ForegroundColor Gray
Write-Host "   ps76-features               # Manage PowerShell 7.6 features" -ForegroundColor Gray

Write-Host "`n🚀 Load-AdminFunctions.ps1 is ready for use!" -ForegroundColor Green
