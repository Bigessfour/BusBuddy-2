#Requires -Version 7.5
<#
.SYNOPSIS
    Test script for enhanced bb-build functionality

.DESCRIPTION
    Simple test to validate the enhanced build function works correctly
#>

[CmdletBinding()]
param()

# Change to BusBuddy directory
Set-Location "c:\Users\steve.mckitrick\Desktop\BusBuddy"

Write-Host "🔍 Testing Enhanced BusBuddy Build Functionality" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

try {
    # Load the profiles
    Write-Host "📦 Loading BusBuddy PowerShell profiles..." -ForegroundColor Yellow
    if (Test-Path ".\load-bus-buddy-profiles.ps1") {
        & ".\load-bus-buddy-profiles.ps1" -Quiet
        Write-Host "✅ Profiles loaded successfully" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Profile loader not found" -ForegroundColor Yellow

        # Try to load module directly
        Write-Host "📦 Loading module directly..." -ForegroundColor Yellow
        Import-Module ".\PowerShell\BusBuddy PowerShell Environment\Modules\BusBuddy\BusBuddy.psm1" -Force
        Write-Host "✅ Module loaded directly" -ForegroundColor Green
    }

    Write-Host ""

    # Test the bb-build command exists
    Write-Host "🔍 Checking bb-build command availability..." -ForegroundColor Cyan
    $bbBuildCommand = Get-Command bb-build -ErrorAction SilentlyContinue
    if ($bbBuildCommand) {
        Write-Host "✅ bb-build command found" -ForegroundColor Green

        # Check if enhanced parameters are available
        $parameters = $bbBuildCommand.Parameters.Keys
        $enhancedParams = @('CaptureProblemList', 'AnalyzeProblems', 'ExportResults', 'AutoFix')

        Write-Host "🔍 Checking for enhanced parameters..." -ForegroundColor Cyan
        $foundParams = @()
        foreach ($param in $enhancedParams) {
            if ($param -in $parameters) {
                $foundParams += $param
                Write-Host "  ✅ $param - Available" -ForegroundColor Green
            } else {
                Write-Host "  ❌ $param - Missing" -ForegroundColor Red
            }
        }

        Write-Host ""
        Write-Host "📊 Enhanced Parameters Status:" -ForegroundColor Blue
        Write-Host "  Found: $($foundParams.Count)/$($enhancedParams.Count)" -ForegroundColor $(if ($foundParams.Count -eq $enhancedParams.Count) { 'Green' } else { 'Yellow' })

        if ($foundParams.Count -eq $enhancedParams.Count) {
            Write-Host ""
            Write-Host "🚀 Running enhanced build test..." -ForegroundColor Cyan

            # Test the enhanced build
            $buildResults = bb-build -CaptureProblemList -AnalyzeProblems -ExportResults -Verbosity minimal

            Write-Host ""
            Write-Host "📋 Enhanced Build Test Results:" -ForegroundColor Blue
            if ($buildResults -is [hashtable]) {
                Write-Host "  • Build Success: $($buildResults.Success)" -ForegroundColor $(if ($buildResults.Success) { 'Green' } else { 'Red' })
                Write-Host "  • Configuration: $($buildResults.Configuration)" -ForegroundColor Gray
                Write-Host "  • Problems Found: $($buildResults.Problems.Count)" -ForegroundColor Yellow
                Write-Host "  • Errors: $($buildResults.ErrorCount)" -ForegroundColor $(if ($buildResults.ErrorCount -eq 0) { 'Green' } else { 'Red' })
                Write-Host "  • Warnings: $($buildResults.WarningCount)" -ForegroundColor $(if ($buildResults.WarningCount -eq 0) { 'Green' } else { 'Yellow' })

                if ($buildResults.Analysis -and $buildResults.Analysis.Recommendations) {
                    Write-Host "  • Recommendations: $($buildResults.Analysis.Recommendations.Count)" -ForegroundColor Blue
                }

                Write-Host ""
                Write-Host "✅ Enhanced build functionality is working!" -ForegroundColor Green
            } else {
                Write-Host "  ⚠️ Build returned: $buildResults (not enhanced results)" -ForegroundColor Yellow
                Write-Host "  This indicates the enhanced features may not be fully active" -ForegroundColor Gray
            }
        } else {
            Write-Host ""
            Write-Host "❌ Enhanced parameters not available - using standard build" -ForegroundColor Red

            # Run standard build
            $buildResult = bb-build -Verbosity minimal
            Write-Host "📋 Standard build result: $buildResult" -ForegroundColor Gray
        }
    } else {
        Write-Host "❌ bb-build command not found" -ForegroundColor Red
        Write-Host "💡 Try running: dotnet build BusBuddy.sln" -ForegroundColor Blue
    }

} catch {
    Write-Host ""
    Write-Host "❌ Test failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "📋 Error details: $($_.ScriptStackTrace)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "🏁 Enhanced build test completed" -ForegroundColor Cyan
