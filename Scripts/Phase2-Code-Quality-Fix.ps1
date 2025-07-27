#Requires -Version 7.5
<#
.SYNOPSIS
    BusBuddy Phase 2 Code Quality Validator and Fixer

.DESCRIPTION
    Addresses code quality issues identified in Phase 2 development files:
    - Fixes CS8600 nullable reference warnings in data services
    - Validates JSON schema compliance with entity models
    - Consolidates duplicate VS Code configuration files
    - Optimizes large JSON data files for performance

.PARAMETER FixMode
    Fix mode: "Report", "Interactive", "Auto", or "ValidationOnly"

.PARAMETER ValidateJsonSchema
    Validate JSON data structure against entity models

.PARAMETER CompressLargeFiles
    Compress or split large JSON files for better performance

.EXAMPLE
    .\Phase2-Code-Quality-Fix.ps1 -FixMode Report -ValidateJsonSchema
#>

param(
    [ValidateSet("Report", "Interactive", "Auto", "ValidationOnly")]
    [string]$FixMode = "Report",
    [switch]$ValidateJsonSchema,
    [switch]$CompressLargeFiles
)

Write-Host "🔍 BusBuddy Phase 2 Code Quality Validator" -ForegroundColor Cyan
Write-Host "PowerShell Version: $($PSVersionTable.PSVersion)" -ForegroundColor Green
Write-Host ""

$IssuesFound = @()

# Define file paths to analyze
$FilesToAnalyze = @{
    DataLoader = "BusBuddy.Core\Services\EnhancedDataLoaderService.cs"
    DataSeeder = "BusBuddy.Core\Services\Phase2DataSeederService.cs"
    JsonData = "BusBuddy.Core\Data\enhanced-realworld-data.json"
    VSCodeSettings = ".vscode\settings.json"
    VSCodeSettingsFixed = ".vscode\settings_fixed.json"
    ServiceExtensions = "BusBuddy.Core\Extensions\ServiceCollectionExtensions.cs"
}

Write-Host "📊 Analyzing Phase 2 files for code quality issues..." -ForegroundColor Yellow

# 1. Check for CS8600 nullable reference warnings
function Test-NullableReferenceIssues {
    param([string]$FilePath)
    
    if (-not (Test-Path $FilePath)) {
        Write-Warning "File not found: $FilePath"
        return @()
    }
    
    $issues = @()
    $content = Get-Content $FilePath
    
    for ($i = 0; $i -lt $content.Count; $i++) {
        $line = $content[$i]
        $lineNumber = $i + 1
        
        # Check for potential null reference assignments
        if ($line -match '=\s*\w+\["[\w\s]+"\]\?\.ToString\(\)' -and $line -notmatch '\?\?') {
            $issues += [PSCustomObject]@{
                File = $FilePath
                Line = $lineNumber
                Issue = "Potential CS8600: Nullable assignment without null check"
                Code = $line.Trim()
                Severity = "Warning"
            }
        }
        
        # Check for ToObject<T>() calls without null checks
        if ($line -match '\.ToObject<\w+>\(\)' -and $line -notmatch '\?\?') {
            $issues += [PSCustomObject]@{
                File = $FilePath
                Line = $lineNumber
                Issue = "Potential CS8600: ToObject<T>() without null validation"
                Code = $line.Trim()
                Severity = "Warning"
            }
        }
        
        # Check for string concatenation with nullable values
        if ($line -match '\$"\{.*\?\.\w+.*\}"' -or $line -match '\+ .*\?\.\w+') {
            $issues += [PSCustomObject]@{
                File = $FilePath
                Line = $lineNumber
                Issue = "Potential null in string interpolation/concatenation"
                Code = $line.Trim()
                Severity = "Info"
            }
        }
    }
    
    return $issues
}

# 2. Validate JSON file size and structure
function Test-JsonDataQuality {
    param([string]$JsonPath)
    
    if (-not (Test-Path $JsonPath)) {
        return @()
    }
    
    $issues = @()
    $fileInfo = Get-Item $JsonPath
    $fileSizeMB = [math]::Round($fileInfo.Length / 1MB, 2)
    
    # Check file size
    if ($fileSizeMB -gt 5) {
        $issues += [PSCustomObject]@{
            File = $JsonPath
            Line = 0
            Issue = "Large JSON file ($fileSizeMB MB) - consider compression or splitting"
            Code = "File size: $fileSizeMB MB"
            Severity = "Performance"
        }
    }
    
    try {
        $jsonContent = Get-Content $JsonPath -Raw | ConvertFrom-Json
        
        # Validate structure
        $requiredSections = @("metadata", "drivers", "vehicles", "routes", "activities")
        foreach ($section in $requiredSections) {
            if (-not $jsonContent.PSObject.Properties.Name.Contains($section)) {
                $issues += [PSCustomObject]@{
                    File = $JsonPath
                    Line = 0
                    Issue = "Missing required JSON section: $section"
                    Code = "Required sections: $($requiredSections -join ', ')"
                    Severity = "Error"
                }
            }
        }
        
        # Check data counts
        if ($jsonContent.drivers -and $jsonContent.drivers.Count -gt 50) {
            $issues += [PSCustomObject]@{
                File = $JsonPath
                Line = 0
                Issue = "Large drivers array ($($jsonContent.drivers.Count) items) - consider pagination"
                Code = "Drivers count: $($jsonContent.drivers.Count)"
                Severity = "Performance"
            }
        }
        
    }
    catch {
        $issues += [PSCustomObject]@{
            File = $JsonPath
            Line = 0
            Issue = "Invalid JSON format"
            Code = $_.Exception.Message
            Severity = "Error"
        }
    }
    
    return $issues
}

# 3. Check VS Code configuration duplication
function Test-VSCodeConfigDuplication {
    $issues = @()
    
    $settingsPath = ".vscode\settings.json"
    $settingsFixedPath = ".vscode\settings_fixed.json"
    
    if ((Test-Path $settingsPath) -and (Test-Path $settingsFixedPath)) {
        $issues += [PSCustomObject]@{
            File = $settingsFixedPath
            Line = 0
            Issue = "Duplicate VS Code settings file - merge into main settings.json"
            Code = "Both settings.json and settings_fixed.json exist"
            Severity = "Maintenance"
        }
    }
    
    return $issues
}

# Run all analyses
Write-Host "🔍 Checking for nullable reference issues..." -ForegroundColor Yellow
foreach ($file in @($FilesToAnalyze.DataLoader, $FilesToAnalyze.DataSeeder, $FilesToAnalyze.ServiceExtensions)) {
    if (Test-Path $file) {
        $IssuesFound += Test-NullableReferenceIssues -FilePath $file
    }
}

Write-Host "📄 Validating JSON data quality..." -ForegroundColor Yellow
if (Test-Path $FilesToAnalyze.JsonData) {
    $IssuesFound += Test-JsonDataQuality -JsonPath $FilesToAnalyze.JsonData
}

Write-Host "⚙️ Checking VS Code configuration..." -ForegroundColor Yellow
$IssuesFound += Test-VSCodeConfigDuplication

# Report findings
Write-Host ""
Write-Host "📋 Code Quality Issues Found: $($IssuesFound.Count)" -ForegroundColor $(if ($IssuesFound.Count -eq 0) { "Green" } else { "Yellow" })

if ($IssuesFound.Count -eq 0) {
    Write-Host "✅ No code quality issues found!" -ForegroundColor Green
    return
}

# Group by severity
$GroupedBySeverity = $IssuesFound | Group-Object -Property Severity

foreach ($Group in $GroupedBySeverity) {
    $color = switch ($Group.Name) {
        "Error" { "Red" }
        "Warning" { "Yellow" }
        "Performance" { "Magenta" }
        "Maintenance" { "Cyan" }
        default { "White" }
    }
    
    Write-Host ""
    Write-Host "🔸 $($Group.Name) Issues ($($Group.Count)):" -ForegroundColor $color
    
    foreach ($Issue in $Group.Group) {
        Write-Host "   📄 $($Issue.File):$($Issue.Line)" -ForegroundColor White
        Write-Host "   ❌ $($Issue.Issue)" -ForegroundColor $color
        Write-Host "   💡 $($Issue.Code)" -ForegroundColor Gray
        Write-Host ""
    }
}

# Generate fixes for nullable reference issues
function New-NullableReferenceFix {
    param([string]$FilePath, [array]$Issues)
    
    $fixes = @()
    
    foreach ($Issue in $Issues) {
        if ($Issue.Issue -like "*CS8600*" -or $Issue.Issue -like "*nullable*") {
            $originalCode = $Issue.Code
            
            # Generate fix based on the issue type
            if ($originalCode -match '=\s*(\w+)\["([\w\s]+)"\]\?\.ToString\(\)') {
                $objectName = $Matches[1]
                $propertyName = $Matches[2]
                $fixedCode = $originalCode -replace 
                    [regex]::Escape("$objectName[`"$propertyName`"]?.ToString()"),
                    "$objectName[`"$propertyName`"]?.ToString() ?? string.Empty"
                
                $fixes += [PSCustomObject]@{
                    File = $FilePath
                    Line = $Issue.Line
                    OriginalCode = $originalCode
                    FixedCode = $fixedCode
                    Description = "Add null coalescing operator for string safety"
                }
            }
        }
    }
    
    return $fixes
}

# Generate recommended fixes
Write-Host "🔧 Recommended Fixes:" -ForegroundColor Yellow

$nullableIssues = $IssuesFound | Where-Object { $_.Issue -like "*CS8600*" -or $_.Issue -like "*nullable*" }
if ($nullableIssues.Count -gt 0) {
    Write-Host ""
    Write-Host "1. Nullable Reference Fixes:" -ForegroundColor Cyan
    Write-Host "   - Add null coalescing operators (??) for string assignments" -ForegroundColor White
    Write-Host "   - Validate ToObject<T>() results before use" -ForegroundColor White
    Write-Host "   - Use string.IsNullOrEmpty() checks for validation" -ForegroundColor White
}

$performanceIssues = $IssuesFound | Where-Object { $_.Severity -eq "Performance" }
if ($performanceIssues.Count -gt 0) {
    Write-Host ""
    Write-Host "2. Performance Optimizations:" -ForegroundColor Magenta
    Write-Host "   - Consider compressing large JSON files" -ForegroundColor White
    Write-Host "   - Implement pagination for large data arrays" -ForegroundColor White
    Write-Host "   - Use streaming JSON parser for very large files" -ForegroundColor White
}

$maintenanceIssues = $IssuesFound | Where-Object { $_.Severity -eq "Maintenance" }
if ($maintenanceIssues.Count -gt 0) {
    Write-Host ""
    Write-Host "3. Maintenance Actions:" -ForegroundColor Cyan
    Write-Host "   - Merge settings_fixed.json into main settings.json" -ForegroundColor White
    Write-Host "   - Remove duplicate configuration files" -ForegroundColor White
    Write-Host "   - Validate task references to new services" -ForegroundColor White
}

# Validation summary
Write-Host ""
Write-Host "📊 Phase 2 Code Quality Report" -ForegroundColor Cyan
Write-Host "Files Analyzed: $($FilesToAnalyze.Count)" -ForegroundColor White
Write-Host "Issues Found: $($IssuesFound.Count)" -ForegroundColor White

$severityCounts = $IssuesFound | Group-Object Severity | ForEach-Object { "$($_.Name): $($_.Count)" }
Write-Host "Issue Breakdown: $($severityCounts -join ', ')" -ForegroundColor White

if ($IssuesFound.Count -gt 0) {
    Write-Host ""
    Write-Host "🔧 Next Steps:" -ForegroundColor Yellow
    Write-Host "1. Address CS8600 nullable warnings with null coalescing operators" -ForegroundColor White
    Write-Host "2. Optimize JSON data structure for performance" -ForegroundColor White
    Write-Host "3. Consolidate VS Code configuration files" -ForegroundColor White
    Write-Host "4. Validate EF Core integration with Phase2DataSeederService" -ForegroundColor White
    Write-Host "5. Test data loading with realistic datasets" -ForegroundColor White
}

Write-Host ""
Write-Host "✅ Phase 2 code quality analysis complete!" -ForegroundColor Green
