# BusBuddy Master Integration Script - PowerShell 7.5 Enhanced
# Ensures all tools work together in a single, consistent approach
# NO VARIATIONS - STRICT STANDARDS ENFORCEMENT
# ENHANCED: Uses PowerShell 7.5 performance improvements

#Requires -Version 7.5

param(
    [Parameter(Mandatory = $false)]
    [ValidateSet('Analyze', 'Validate', 'Report', 'Health', 'PS75Test')]
    [string]$Mode = 'Analyze',

    [Parameter(Mandatory = $false)]
    [string]$TargetFile,

    [Parameter(Mandatory = $false)]
    [switch]$Comprehensive
)

# STRICT: Import all analysis tools in controlled order
Write-Host '🔧 Loading BusBuddy Analysis Suite...' -ForegroundColor Cyan

$ToolsPath = "$PSScriptRoot"
$RequiredTools = @(
    'Read-Only-Analysis-Tools.ps1',
    'Error-Analysis.ps1',
    'Syncfusion-Implementation-Validator.ps1'
)

# Validate all tools exist before proceeding
foreach ($tool in $RequiredTools) {
    $toolPath = Join-Path $ToolsPath $tool
    if (-not (Test-Path $toolPath)) {
        Write-Error "CRITICAL: Missing required tool: $tool"
        Write-Host "Expected location: $toolPath" -ForegroundColor Red
        exit 1
    }
}

# Load tools in specific order
try {
    Import-Module "$ToolsPath\Read-Only-Analysis-Tools.ps1" -Force -ErrorAction Stop
    Write-Host '  ✅ Read-Only Analysis Tools loaded' -ForegroundColor Green

    Import-Module "$ToolsPath\Error-Analysis.ps1" -Force -ErrorAction Stop
    Write-Host '  ✅ Error Analysis Tools loaded' -ForegroundColor Green

    Import-Module "$ToolsPath\Syncfusion-Implementation-Validator.ps1" -Force -ErrorAction Stop
    Write-Host '  ✅ Syncfusion Validator loaded' -ForegroundColor Green
} catch {
    Write-Error "Failed to load analysis tools: $_"
    exit 1
}

# INTEGRATION FUNCTIONS - SINGLE CONSISTENT APPROACH

function Invoke-MasterAnalysis {
    <#
    .SYNOPSIS
    Master analysis function using all tools consistently
    .DESCRIPTION
    Single entry point for all analysis - prevents variations
    #>
    param(
        [string]$FilePath,
        [switch]$DetailedReport
    )

    Write-Host "🔍 MASTER ANALYSIS: $FilePath" -ForegroundColor Yellow
    Write-Host '=' * 80 -ForegroundColor Gray

    $masterResult = @{
        FilePath        = $FilePath
        Timestamp       = Get-Date
        OverallHealth   = 'Unknown'
        Categories      = @{
            Syntax      = @()
            Structure   = @()
            Standards   = @()
            Performance = @()
        }
        Recommendations = @()
        FixStrategies   = @()
    }

    # 1. SYNTAX ANALYSIS (File-type specific)
    $extension = [System.IO.Path]::GetExtension($FilePath).ToLower()

    switch ($extension) {
        '.cs' {
            Write-Host '🔍 C# Syntax Analysis...' -ForegroundColor Cyan
            $syntaxResult = Analyze-CSharpSyntax -FilePath $FilePath
            $masterResult.Categories.Syntax = $syntaxResult.Errors

            if ($syntaxResult.BraceAnalysis -and $syntaxResult.BraceAnalysis.UnmatchedOpening.Count -gt 0) {
                $masterResult.FixStrategies += 'CRITICAL: Unmatched braces detected - requires manual brace matching'
                $masterResult.OverallHealth = 'Critical'
            }
        }

        '.xaml' {
            Write-Host '🔍 XAML Structure Analysis...' -ForegroundColor Cyan
            $xamlResult = Analyze-XamlStructure -FilePath $FilePath
            $masterResult.Categories.Structure = $xamlResult.Errors

            if (-not $xamlResult.IsValid) {
                $masterResult.FixStrategies += 'XAML: XML structure corruption - validate namespace declarations'
                $masterResult.OverallHealth = 'Critical'
            }

            # Syncfusion Standards Check
            Write-Host '🎨 Syncfusion Standards Validation...' -ForegroundColor Magenta
            $syncfusionResult = Test-SyncfusionCompliance -FilePath $FilePath
            $masterResult.Categories.Standards = $syncfusionResult.Violations
        }

        '.ps1' {
            Write-Host '🔍 PowerShell Analysis...' -ForegroundColor Blue
            $psResult = Analyze-PowerShellSyntax -FilePath $FilePath
            $masterResult.Categories.Syntax = $psResult.Errors

            if (-not $psResult.IsValid) {
                $masterResult.FixStrategies += 'PowerShell: Use syntax validation before execution'
            }
        }
    }

    # 2. DETERMINE OVERALL HEALTH
    if ($masterResult.OverallHealth -eq 'Unknown') {
        $totalIssues = ($masterResult.Categories.Syntax + $masterResult.Categories.Structure + $masterResult.Categories.Standards).Count

        $masterResult.OverallHealth = switch ($totalIssues) {
            0 { 'Excellent' }
            { $_ -le 3 } { 'Good' }
            { $_ -le 10 } { 'Warning' }
            default { 'Critical' }
        }
    }

    # 3. GENERATE RECOMMENDATIONS
    if ($masterResult.Categories.Syntax.Count -gt 0) {
        $masterResult.Recommendations += 'Address syntax errors before proceeding with functionality changes'
    }

    if ($masterResult.Categories.Structure.Count -gt 0) {
        $masterResult.Recommendations += 'Fix structural issues to prevent file corruption'
    }

    if ($masterResult.Categories.Standards.Count -gt 0) {
        $masterResult.Recommendations += 'Align with Syncfusion official documentation standards'
    }

    return $masterResult
}

function Test-SyncfusionCompliance {
    <#
    .SYNOPSIS
    Tests XAML compliance with official Syncfusion standards
    .DESCRIPTION
    READ-ONLY: Validates against documented Syncfusion patterns
    #>
    param([string]$FilePath)

    $content = Get-Content $FilePath -Raw
    $violations = @()

    # Theme compliance check
    if ($content -match 'syncfusion:' -and $content -notmatch 'Theme="Fluent(Dark|Light)"') {
        $violations += 'Missing or incorrect Syncfusion theme declaration'
    }

    # Namespace compliance
    if ($content -match 'syncfusion:' -and $content -notmatch 'xmlns:syncfusion="http://schemas\.syncfusion\.com/wpf"') {
        $violations += 'Incorrect Syncfusion namespace declaration'
    }

    # Control property validation
    $syncfusionControls = [regex]::Matches($content, 'syncfusion:(\w+)', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    foreach ($match in $syncfusionControls) {
        $controlName = $match.Groups[1].Value
        # Add specific validation rules per control type
        switch ($controlName) {
            'SfButton' {
                if ($content -notmatch 'SizeMode=') {
                    $violations += 'SfButton missing recommended SizeMode property'
                }
            }
            'DockingManager' {
                if ($content -notmatch 'UseDocumentContainer=') {
                    $violations += 'DockingManager missing UseDocumentContainer property'
                }
            }
        }
    }

    return @{
        IsCompliant   = $violations.Count -eq 0
        Violations    = $violations
        ControlsFound = $syncfusionControls.Count
    }
}

function Show-MasterReport {
    param($Analysis)

    Write-Host "`n📊 MASTER ANALYSIS REPORT" -ForegroundColor Green
    Write-Host '=' * 50 -ForegroundColor Green
    Write-Host "File: $($Analysis.FilePath)" -ForegroundColor White
    Write-Host "Health: $($Analysis.OverallHealth)" -ForegroundColor $(
        switch ($Analysis.OverallHealth) {
            'Excellent' { 'Green' }
            'Good' { 'Yellow' }
            'Warning' { 'DarkYellow' }
            'Critical' { 'Red' }
            default { 'Gray' }
        }
    )
    Write-Host "Timestamp: $($Analysis.Timestamp)" -ForegroundColor Gray

    # Show categories
    Write-Host "`n📋 ISSUE CATEGORIES:" -ForegroundColor Cyan
    Write-Host "Syntax Issues: $($Analysis.Categories.Syntax.Count)" -ForegroundColor White
    Write-Host "Structure Issues: $($Analysis.Categories.Structure.Count)" -ForegroundColor White
    Write-Host "Standards Issues: $($Analysis.Categories.Standards.Count)" -ForegroundColor White

    # Show recommendations
    if ($Analysis.Recommendations.Count -gt 0) {
        Write-Host "`n💡 RECOMMENDATIONS:" -ForegroundColor Yellow
        foreach ($rec in $Analysis.Recommendations) {
            Write-Host "  • $rec" -ForegroundColor White
        }
    }

    # Show fix strategies
    if ($Analysis.FixStrategies.Count -gt 0) {
        Write-Host "`n🔧 FIX STRATEGIES:" -ForegroundColor Magenta
        foreach ($strategy in $Analysis.FixStrategies) {
            Write-Host "  • $strategy" -ForegroundColor White
        }
    }
}

# MAIN EXECUTION - SINGLE CONSISTENT WORKFLOW
Write-Host '🚀 BusBuddy Master Integration - Single Consistent Approach' -ForegroundColor Green
Write-Host "Mode: $Mode" -ForegroundColor White

switch ($Mode) {
    'Analyze' {
        if ($TargetFile) {
            $result = Invoke-MasterAnalysis -FilePath $TargetFile -DetailedReport:$Comprehensive
            Show-MasterReport -Analysis $result
        } else {
            Write-Host "Usage: -Mode Analyze -TargetFile 'path\to\file'" -ForegroundColor Yellow
        }
    }

    'Health' {
        Write-Host '🏥 WORKSPACE HEALTH CHECK' -ForegroundColor Green
        $projectAnalysis = Analyze-ProjectErrors -WorkspaceFolder (Get-Location).Path

        Write-Host "Total Errors: $($projectAnalysis.TotalErrors)" -ForegroundColor $(if ($projectAnalysis.TotalErrors -gt 0) { 'Red' }else { 'Green' })
        Write-Host "Total Warnings: $($projectAnalysis.TotalWarnings)" -ForegroundColor $(if ($projectAnalysis.TotalWarnings -gt 0) { 'Yellow' }else { 'Green' })

        # Integration health
        $integrationHealth = 'Good'
        if ($projectAnalysis.TotalErrors -gt 20) { $integrationHealth = 'Critical' }
        elseif ($projectAnalysis.TotalErrors -gt 5) { $integrationHealth = 'Warning' }

        Write-Host "Integration Health: $integrationHealth" -ForegroundColor $(
            switch ($integrationHealth) {
                'Good' { 'Green' }
                'Warning' { 'Yellow' }
                'Critical' { 'Red' }
            }
        )
    }

    'Validate' {
        Write-Host '✅ VALIDATION MODE - All Tools Integration Check' -ForegroundColor Cyan

        # Check VS Code settings alignment
        $settingsPath = '.vscode\settings.json'
        if (Test-Path $settingsPath) {
            Write-Host '✅ VS Code settings found' -ForegroundColor Green
        } else {
            Write-Host '❌ VS Code settings missing' -ForegroundColor Red
        }

        # Check PowerShell profile
        $profilePath = 'BusBuddy-PowerShell-Profile.ps1'
        if (Test-Path $profilePath) {
            Write-Host '✅ PowerShell profile found' -ForegroundColor Green
        } else {
            Write-Host '❌ PowerShell profile missing' -ForegroundColor Red
        }

        # Check tool availability
        foreach ($tool in $RequiredTools) {
            $toolPath = Join-Path $ToolsPath $tool
            if (Test-Path $toolPath) {
                Write-Host "✅ $tool available" -ForegroundColor Green
            } else {
                Write-Host "❌ $tool missing" -ForegroundColor Red
            }
        }
    }

    'PS75Test' {
        Write-Host '🧪 POWERSHELL 7.5 FEATURE TEST' -ForegroundColor Magenta
        Show-PowerShell75Capabilities

        # Test enhanced analysis on sample data
        Write-Host "`n🔬 Testing Enhanced Analysis..." -ForegroundColor Cyan

        if ($TargetFile) {
            $extension = [System.IO.Path]::GetExtension($TargetFile).ToLower()
            switch ($extension) {
                '.cs' {
                    $enhancedResult = Get-EnhancedCSharpAnalysis -FilePath $TargetFile
                    Write-Host "Enhanced C# Analysis completed in $($enhancedResult.Performance.AnalysisTime)ms" -ForegroundColor Green
                }
                '.xaml' {
                    $enhancedResult = Get-EnhancedXamlAnalysis -FilePath $TargetFile
                    Write-Host "Enhanced XAML Analysis completed in $($enhancedResult.Performance.AnalysisTime)ms" -ForegroundColor Green
                }
                default {
                    Write-Host "File type not supported for enhanced analysis: $extension" -ForegroundColor Yellow
                }
            }
        } else {
            Write-Host 'Use -TargetFile to test enhanced analysis on specific files' -ForegroundColor Yellow
        }
    }
}

Write-Host "`n✅ Master Integration Complete - Single Consistent Approach Maintained" -ForegroundColor Green
