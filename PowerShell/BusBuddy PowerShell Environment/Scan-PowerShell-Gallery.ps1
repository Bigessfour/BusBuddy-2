#Requires -Version 7.5

<#
.SYNOPSIS
PowerShell Gallery Workflow Scanner for BusBuddy Enhancement

.DESCRIPTION
Searches PowerShell Gallery for modules and workflows that would enhance BusBuddy development:
- AI/ML integration tools
- WPF development utilities
- Testing frameworks
- Code analysis tools
- Build automation
- Documentation generators
- Performance monitoring
- Security scanning

.NOTES
Author: BusBuddy Development Team
Created: July 26, 2025
Purpose: Discover PowerShell Gallery tools for BusBuddy enhancement
#>

param(
    [switch]$AnalyzeTop50,
    [switch]$SearchAI,
    [switch]$SearchWPF,
    [switch]$SearchTesting,
    [switch]$SearchBuild,
    [switch]$SearchSecurity,
    [switch]$InstallRecommended,
    [switch]$GenerateReport,
    [string]$OutputPath = ".\PowerShell-Gallery-Analysis-Report.md"
)

# Categories for BusBuddy enhancement
$SearchCategories = @{
    'AI_ML'         = @(
        'PSAISuite', 'OpenAI', 'Azure.AI', 'ML.NET', 'PythonSelect', 'AIAssistant'
    )
    'WPF_UI'        = @(
        'WPF', 'XAML', 'UI', 'Syncfusion', 'MaterialDesign', 'Mahapps', 'ModernWpf'
    )
    'Testing'       = @(
        'Pester', 'PSScriptAnalyzer', 'PSCodeHealth', 'InvokeBuild', 'TestFramework'
    )
    'Build_Deploy'  = @(
        'PSDeploy', 'BuildHelpers', 'InvokeBuild', 'psake', 'GitHubActions', 'Azure.DevOps'
    )
    'Code_Quality'  = @(
        'PSScriptAnalyzer', 'PSCodeHealth', 'PSUsing', 'PSReadLine', 'PSFzf'
    )
    'Documentation' = @(
        'PlatyPS', 'MarkdownPS', 'PowerShellGet', 'PSReadLine', 'PSDocGenerator'
    )
    'Performance'   = @(
        'Profiler', 'Performance', 'Benchmark', 'Monitor', 'Measure'
    )
    'Security'      = @(
        'PoShSecMod', 'SecurityFever', 'PowerShellSecurity', 'DevSec', 'Compliance'
    )
    'Database'      = @(
        'SimplySql', 'SqlServer', 'dbatools', 'EntityFramework', 'PostgreSQL'
    )
    'Configuration' = @(
        'Configuration', 'PoshBot', 'PSFramework', 'PowerShellGet', 'Environment'
    )
}

# Colors for output
$Colors = @{
    Success   = 'Green'
    Warning   = 'Yellow'
    Error     = 'Red'
    Info      = 'Cyan'
    Header    = 'Magenta'
    Highlight = 'DarkYellow'
}

function Write-Section {
    param([string]$Title, [string]$Color = 'Magenta')
    Write-Host "`n=== $Title ===" -ForegroundColor $Color
}

function Write-ModuleInfo {
    param(
        [PSCustomObject]$Module,
        [string]$Relevance = "Unknown"
    )

    Write-Host "📦 $($Module.Name)" -ForegroundColor $Colors.Highlight
    Write-Host "   Version: $($Module.Version)" -ForegroundColor $Colors.Info
    Write-Host "   Downloads: $($Module.DownloadCount)" -ForegroundColor $Colors.Info
    Write-Host "   Relevance: $Relevance" -ForegroundColor $Colors.Success
    Write-Host "   Description: $($Module.Description.Substring(0, [Math]::Min(100, $Module.Description.Length)))..." -ForegroundColor $Colors.Info
    Write-Host ""
}

function Search-PowerShellGallery {
    param(
        [string[]]$Keywords,
        [int]$MaxResults = 10,
        [string]$Category = "General"
    )

    Write-Section "Searching: $Category"
    $allResults = @()

    foreach ($keyword in $Keywords) {
        try {
            Write-Host "🔍 Searching for: $keyword" -ForegroundColor $Colors.Info
            $results = Find-Module -Name "*$keyword*" -Repository PSGallery -ErrorAction SilentlyContinue |
            Sort-Object DownloadCount -Descending |
            Select-Object -First $MaxResults

            if ($results) {
                $allResults += $results | ForEach-Object {
                    $_ | Add-Member -NotePropertyName 'SearchKeyword' -NotePropertyValue $keyword -PassThru
                    $_ | Add-Member -NotePropertyName 'Category' -NotePropertyValue $Category -PassThru
                }
            }
        }
        catch {
            Write-Host "⚠️ Failed to search for $keyword`: $($_.Exception.Message)" -ForegroundColor $Colors.Warning
        }
    }

    return $allResults | Sort-Object DownloadCount -Descending | Select-Object -First ($MaxResults * 2)
}

function Get-ModuleRelevanceScore {
    param(
        [PSCustomObject]$Module,
        [string]$Category
    )

    $score = 0
    $reasons = @()

    # Download count weight (30%)
    if ($Module.DownloadCount -gt 1000000) { $score += 30; $reasons += "High adoption (1M+ downloads)" }
    elseif ($Module.DownloadCount -gt 100000) { $score += 20; $reasons += "Good adoption (100K+ downloads)" }
    elseif ($Module.DownloadCount -gt 10000) { $score += 10; $reasons += "Moderate adoption (10K+ downloads)" }

    # Category-specific relevance (40%)
    switch ($Category) {
        'AI_ML' {
            if ($Module.Name -like "*AI*" -or $Module.Description -like "*AI*") { $score += 40; $reasons += "AI/ML focused" }
            elseif ($Module.Name -like "*OpenAI*" -or $Module.Name -like "*GPT*") { $score += 35; $reasons += "OpenAI integration" }
            elseif ($Module.Description -like "*machine learning*" -or $Module.Description -like "*neural*") { $score += 30; $reasons += "ML capabilities" }
        }
        'WPF_UI' {
            if ($Module.Name -like "*WPF*" -or $Module.Description -like "*WPF*") { $score += 40; $reasons += "WPF specific" }
            elseif ($Module.Name -like "*XAML*" -or $Module.Description -like "*XAML*") { $score += 35; $reasons += "XAML support" }
            elseif ($Module.Name -like "*UI*" -or $Module.Description -like "*user interface*") { $score += 25; $reasons += "UI focused" }
        }
        'Testing' {
            if ($Module.Name -like "*Test*" -or $Module.Name -like "*Pester*") { $score += 40; $reasons += "Testing framework" }
            elseif ($Module.Description -like "*test*" -or $Module.Description -like "*quality*") { $score += 30; $reasons += "Quality assurance" }
        }
        'Build_Deploy' {
            if ($Module.Name -like "*Build*" -or $Module.Name -like "*Deploy*") { $score += 40; $reasons += "Build/Deploy tools" }
            elseif ($Module.Description -like "*CI/CD*" -or $Module.Description -like "*automation*") { $score += 30; $reasons += "Automation focus" }
        }
    }

    # Recent updates (20%)
    if ($Module.PublishedDate -gt (Get-Date).AddMonths(-6)) { $score += 20; $reasons += "Recently updated" }
    elseif ($Module.PublishedDate -gt (Get-Date).AddYears(-1)) { $score += 10; $reasons += "Updated within year" }

    # Microsoft or verified publishers (10%)
    if ($Module.Author -like "*Microsoft*" -or $Module.CompanyName -like "*Microsoft*") { $score += 10; $reasons += "Microsoft supported" }

    return @{
        Score   = $score
        Reasons = $reasons -join ", "
    }
}

function Test-ModuleForBusBuddy {
    param([PSCustomObject]$Module)

    $relevance = Get-ModuleRelevanceScore -Module $Module -Category $Module.Category
    $recommendation = "Unknown"

    if ($relevance.Score -ge 70) { $recommendation = "⭐⭐⭐ Highly Recommended" }
    elseif ($relevance.Score -ge 50) { $recommendation = "⭐⭐ Recommended" }
    elseif ($relevance.Score -ge 30) { $recommendation = "⭐ Consider" }
    else { $recommendation = "❌ Not Recommended" }

    return @{
        Module         = $Module
        Score          = $relevance.Score
        Reasons        = $relevance.Reasons
        Recommendation = $recommendation
    }
}

function New-RecommendationReport {
    param(
        [array]$AnalyzedModules,
        [string]$OutputPath
    )

    $report = @"
# PowerShell Gallery Analysis Report for BusBuddy
Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

## Executive Summary
This report analyzes PowerShell Gallery modules that could enhance the BusBuddy transportation management system development workflow.

## Top Recommendations

"@

    # Sort by score and get top recommendations
    $topRecommendations = $AnalyzedModules | Sort-Object Score -Descending | Select-Object -First 20

    foreach ($item in $topRecommendations) {
        $module = $item.Module
        $report += @"

### $($item.Recommendation) $($module.Name)
- **Version**: $($module.Version)
- **Downloads**: $($module.DownloadCount)
- **Category**: $($module.Category)
- **Score**: $($item.Score)/100
- **Reasons**: $($item.Reasons)
- **Description**: $($module.Description)
- **Install Command**: ``Install-Module -Name $($module.Name)``

"@
    }

    # Category breakdown
    $report += @"

## Analysis by Category

"@

    $categories = $AnalyzedModules | Group-Object { $_.Module.Category }
    foreach ($category in $categories) {
        $report += @"

### $($category.Name)
Top modules in this category:

"@
        $topInCategory = $category.Group | Sort-Object Score -Descending | Select-Object -First 5
        foreach ($item in $topInCategory) {
            $report += "- **$($item.Module.Name)** (Score: $($item.Score)) - $($item.Module.Description.Substring(0, [Math]::Min(80, $item.Module.Description.Length)))...`n"
        }
    }

    # Implementation recommendations
    $report += @"

## Implementation Recommendations for BusBuddy

### Phase 1: Essential Tools (Immediate Implementation)
1. **PSScriptAnalyzer** - Code quality and standards enforcement
2. **Pester** - Unit testing framework
3. **BuildHelpers** - Build automation
4. **PlatyPS** - Documentation generation

### Phase 2: Enhancement Tools (Next Sprint)
1. **PSCodeHealth** - Code health monitoring
2. **InvokeBuild** - Advanced build automation
3. **dbatools** - Database management utilities
4. **PSFramework** - Framework utilities

### Phase 3: Advanced Integration (Future Phases)
1. **PSAISuite** - AI integration (already planned)
2. **Azure.DevOps** - DevOps integration
3. **SecurityFever** - Security enhancements
4. **Performance monitoring tools**

### Integration with BusBuddy Workflow
- Add recommended modules to ``BusBuddy-PowerShell-Profile.ps1``
- Create wrapper functions for common tasks
- Integrate with existing ``bb-*`` command structure
- Update VS Code tasks to leverage new tools

## Installation Script
``````powershell
# Essential modules for BusBuddy
$essentialModules = @(
    'PSScriptAnalyzer',
    'Pester',
    'BuildHelpers',
    'PlatyPS'
)

foreach ($module in $essentialModules) {
    Install-Module -Name $module -Scope CurrentUser -Force
}
``````

---
*Report generated by BusBuddy PowerShell Gallery Scanner*
"@

    $report | Out-File -FilePath $OutputPath -Encoding UTF8
    Write-Host "📝 Report saved to: $OutputPath" -ForegroundColor $Colors.Success
}

# Main execution
Write-Host "🔍 BusBuddy PowerShell Gallery Workflow Scanner" -ForegroundColor $Colors.Header
Write-Host "=============================================" -ForegroundColor $Colors.Header

$allAnalyzedModules = @()

# Search each category
foreach ($categoryPair in $SearchCategories.GetEnumerator()) {
    $categoryName = $categoryPair.Key
    $keywords = $categoryPair.Value

    # Skip categories based on switches
    if ($SearchAI -and $categoryName -ne 'AI_ML') { continue }
    if ($SearchWPF -and $categoryName -ne 'WPF_UI') { continue }
    if ($SearchTesting -and $categoryName -ne 'Testing') { continue }
    if ($SearchBuild -and $categoryName -ne 'Build_Deploy') { continue }
    if ($SearchSecurity -and $categoryName -ne 'Security') { continue }

    $modules = Search-PowerShellGallery -Keywords $keywords -Category $categoryName -MaxResults 5

    foreach ($module in $modules) {
        $analysis = Test-ModuleForBusBuddy -Module $module
        $allAnalyzedModules += $analysis
        Write-ModuleInfo -Module $module -Relevance $analysis.Recommendation
    }
}

# Generate summary
Write-Section "Summary & Top Recommendations"
$topModules = $allAnalyzedModules | Sort-Object Score -Descending | Select-Object -First 10

Write-Host "🏆 Top 10 Recommended Modules for BusBuddy:" -ForegroundColor $Colors.Success
$rank = 1
foreach ($item in $topModules) {
    Write-Host "$rank. $($item.Recommendation) $($item.Module.Name) (Score: $($item.Score))" -ForegroundColor $Colors.Highlight
    Write-Host "   $($item.Reasons)" -ForegroundColor $Colors.Info
    $rank++
}

# Essential modules for immediate installation
$essentialModules = @(
    'PSScriptAnalyzer',
    'Pester',
    'BuildHelpers',
    'PlatyPS',
    'PSCodeHealth'
)

Write-Section "Essential Modules for Immediate Installation"
foreach ($module in $essentialModules) {
    $existing = Get-Module -ListAvailable -Name $module -ErrorAction SilentlyContinue
    if ($existing) {
        Write-Host "✅ $module (Version: $($existing.Version))" -ForegroundColor $Colors.Success
    }
    else {
        Write-Host "❌ $module (Not installed)" -ForegroundColor $Colors.Warning
        if ($InstallRecommended) {
            Write-Host "   Installing $module..." -ForegroundColor $Colors.Info
            try {
                Install-Module -Name $module -Scope CurrentUser -Force -AllowClobber
                Write-Host "   ✅ Installed successfully" -ForegroundColor $Colors.Success
            }
            catch {
                Write-Host "   ❌ Installation failed: $($_.Exception.Message)" -ForegroundColor $Colors.Error
            }
        }
    }
}

# Generate report if requested
if ($GenerateReport) {
    New-RecommendationReport -AnalyzedModules $allAnalyzedModules -OutputPath $OutputPath
}

Write-Section "Next Steps"
Write-Host "1. Review the top recommendations above" -ForegroundColor $Colors.Info
Write-Host "2. Install essential modules: Use -InstallRecommended switch" -ForegroundColor $Colors.Info
Write-Host "3. Generate full report: Use -GenerateReport switch" -ForegroundColor $Colors.Info
Write-Host "4. Integrate recommended tools into BusBuddy workflow" -ForegroundColor $Colors.Info
Write-Host "5. Update VS Code tasks and PowerShell profiles" -ForegroundColor $Colors.Info

Write-Host "`n🎉 PowerShell Gallery analysis complete!" -ForegroundColor $Colors.Success
