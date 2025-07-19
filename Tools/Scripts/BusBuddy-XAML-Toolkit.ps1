#Requires -Version 7.0
<#
.SYNOPSIS
    Bus Buddy XAML Toolkit - Integrated PowerShell XAML Analysis Suite

.DESCRIPTION
    Collection of XAML analysis functions integrated with the Bus Buddy project.
    Provides quick access to XAML syntax detection, structure analysis, and quality inspection.

.NOTES
    Designed to work with the Bus Buddy project structure and coding standards.
    Add to PowerShell profile for persistent availability.

    Project: Bus Buddy WPF Application
    Created: $(Get-Date -Format 'yyyy-MM-dd')
    Version: 2.0
#>

# Import the main XAML analysis tools
$ToolsPath = Join-Path $PSScriptRoot 'Tools\Scripts'

if (Test-Path $ToolsPath) {
    . (Join-Path $ToolsPath 'XAML-Syntax-Analyzer.ps1')
    . (Join-Path $ToolsPath 'Read-Only-Analysis-Tools.ps1')
}

#region Bus Buddy XAML Analysis Functions

function Invoke-BusBuddyXamlAnalysis {
    <#
    .SYNOPSIS
        Quick XAML syntax analysis for Bus Buddy project
    .PARAMETER Path
        Path to XAML file or Views directory
    .PARAMETER Detailed
        Show detailed analysis
    .EXAMPLE
        Invoke-BusBuddyXamlAnalysis -Path "BusBuddy.WPF\Views" -Detailed
    .ALIAS
        bb-xaml-analyze
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false)]
        [string]$Path = 'BusBuddy.WPF\Views',

        [switch]$Detailed
    )

    Write-Host '🚌 Bus Buddy XAML Analyzer' -ForegroundColor Cyan

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        Write-Host '❌ Bus Buddy project root not found' -ForegroundColor Red
        return
    }

    $targetPath = if ([System.IO.Path]::IsPathRooted($Path)) {
        $Path
    } else {
        Join-Path $projectRoot $Path
    }

    if (-not (Test-Path $targetPath)) {
        Write-Host "❌ Path not found: $targetPath" -ForegroundColor Red
        return
    }

    # Run the analysis
    Start-XamlAnalysis -Path $targetPath -Detailed:$Detailed -ExportReport

    Write-Host "`n💡 Tip: Use 'Invoke-BusBuddyXamlInspection' for quality analysis or 'Get-BusBuddyXamlStructure' for architectural review" -ForegroundColor Yellow
}

function Invoke-BusBuddyXamlInspection {
    <#
    .SYNOPSIS
        Comprehensive XAML quality inspection for Bus Buddy
    .PARAMETER Path
        Path to XAML file or directory
    .PARAMETER Quick
        Quick scan only
    .PARAMETER Deep
        Full comprehensive analysis
    .PARAMETER Report
        Generate HTML report
    .EXAMPLE
        Invoke-BusBuddyXamlInspection -Path "MainWindow.xaml" -Deep -Report
    .ALIAS
        bb-xaml-inspect
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false)]
        [string]$Path = 'BusBuddy.WPF\Views',

        [switch]$Quick,
        [switch]$Deep,
        [switch]$Security,
        [switch]$Accessibility,
        [switch]$Report
    )

    Write-Host '🚌 Bus Buddy XAML Quality Inspector' -ForegroundColor Cyan

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        Write-Host '❌ Bus Buddy project root not found' -ForegroundColor Red
        return
    }

    $targetPath = if ([System.IO.Path]::IsPathRooted($Path)) {
        $Path
    } else {
        Join-Path $projectRoot $Path
    }

    if (-not (Test-Path $targetPath)) {
        Write-Host "❌ Path not found: $targetPath" -ForegroundColor Red
        return
    }

    # Default to quick scan if no option specified
    if (-not ($Quick -or $Deep)) {
        $Quick = $true
    }

    $reportPath = Join-Path $projectRoot 'logs\XAML-Quality-Report.html'

    Start-XamlQualityInspection -Path $targetPath -QuickScan:$Quick -DeepScan:$Deep -SecurityCheck:$Security -AccessibilityCheck:$Accessibility -GenerateReport:$Report -ReportPath $reportPath

    if ($Report) {
        Write-Host "`n📄 Report saved to: $reportPath" -ForegroundColor Green
        Write-Host "💡 Tip: Open with 'Invoke-Item `"$reportPath`"' to view in browser" -ForegroundColor Yellow
    }
}

function Get-BusBuddyXamlStructure {
    <#
    .SYNOPSIS
        Analyze XAML structure and architecture for Bus Buddy
    .PARAMETER Path
        Path to XAML file
    .PARAMETER ShowTree
        Display visual structure tree
    .PARAMETER Bindings
        Analyze data bindings
    .PARAMETER Resources
        Validate resources
    .PARAMETER Performance
        Show performance metrics
    .EXAMPLE
        Get-BusBuddyXamlStructure -Path "Views\Main\MainWindow.xaml" -ShowTree -Bindings -Performance
    .ALIAS
        bb-xaml-structure
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [switch]$ShowTree,
        [switch]$Bindings,
        [switch]$Resources,
        [switch]$Performance
    )

    Write-Host '🚌 Bus Buddy XAML Structure Detective' -ForegroundColor Cyan

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        Write-Host '❌ Bus Buddy project root not found' -ForegroundColor Red
        return
    }

    $targetPath = if ([System.IO.Path]::IsPathRooted($Path)) {
        $Path
    } else {
        Join-Path $projectRoot $Path
    }

    if (-not (Test-Path $targetPath)) {
        Write-Host "❌ Path not found: $targetPath" -ForegroundColor Red
        return
    }

    Start-XamlDetectiveAnalysis -Path $targetPath -ShowStructure:$ShowTree -CheckBindings:$Bindings -ValidateResources:$Resources -Performance:$Performance
}

function Test-BusBuddyXamlValidation {
    <#
    .SYNOPSIS
        Quick XAML validation for Bus Buddy project files
    .PARAMETER Path
        Path to validate (defaults to Views directory)
    .EXAMPLE
        Test-BusBuddyXamlValidation
        Test-BusBuddyXamlValidation -Path "BusBuddy.WPF\Resources"
    .ALIAS
        bb-xaml-validate
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false)]
        [string]$Path = 'BusBuddy.WPF\Views'
    )

    Write-Host '🚌 Bus Buddy XAML Validator' -ForegroundColor Cyan

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        Write-Host '❌ Bus Buddy project root not found' -ForegroundColor Red
        return
    }

    $targetPath = if ([System.IO.Path]::IsPathRooted($Path)) {
        $Path
    } else {
        Join-Path $projectRoot $Path
    }

    if (-not (Test-Path $targetPath)) {
        Write-Host "❌ Path not found: $targetPath" -ForegroundColor Red
        return
    }

    # Get XAML files
    $xamlFiles = Get-ChildItem $targetPath -Filter '*.xaml' -Recurse

    if ($xamlFiles.Count -eq 0) {
        Write-Host "⚠️ No XAML files found in: $targetPath" -ForegroundColor Yellow
        return
    }

    Write-Host "📂 Validating $($xamlFiles.Count) XAML files..." -ForegroundColor White

    $errors = 0
    $warnings = 0

    foreach ($file in $xamlFiles) {
        try {
            $content = Get-Content $file.FullName -Raw
            $xml = [xml]$content

            # Validate XML structure
            if ($xml -and $xml.DocumentElement) {
                Write-Host '✅ ' -ForegroundColor Green -NoNewline
                Write-Host (Split-Path $file.FullName -Leaf) -ForegroundColor White
            } else {
                Write-Host '❌ ' -ForegroundColor Red -NoNewline
                Write-Host (Split-Path $file.FullName -Leaf) -ForegroundColor White
                continue
            }

            # Check for common Bus Buddy issues
            if ($content -match 'x:Name="[a-z]') {
                Write-Host '  ⚠️ Non-PascalCase element names detected' -ForegroundColor Yellow
                $warnings++
            }

            if ($content -match 'Syncfusion' -and $content -notmatch 'FluentDark|FluentLight') {
                Write-Host '  ⚠️ Syncfusion controls without theme reference' -ForegroundColor Yellow
                $warnings++
            }

        } catch {
            Write-Host '❌ ' -ForegroundColor Red -NoNewline
            Write-Host (Split-Path $file.FullName -Leaf) -ForegroundColor White
            Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
            $errors++
        }
    }

    Write-Host "`n📊 Validation Summary:" -ForegroundColor Cyan
    Write-Host "  Files: $($xamlFiles.Count)" -ForegroundColor White
    Write-Host "  Errors: $errors" -ForegroundColor $(if ($errors -gt 0) { 'Red' } else { 'Green' })
    Write-Host "  Warnings: $warnings" -ForegroundColor $(if ($warnings -gt 0) { 'Yellow' } else { 'Green' })

    if ($errors -eq 0 -and $warnings -eq 0) {
        Write-Host '🎉 All XAML files are valid!' -ForegroundColor Green
    }
}

function New-BusBuddyXamlReport {
    <#
    .SYNOPSIS
        Generate comprehensive XAML analysis report for Bus Buddy
    .PARAMETER OutputPath
        Output directory for reports
    .EXAMPLE
        New-BusBuddyXamlReport
    .ALIAS
        bb-xaml-report
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false)]
        [string]$OutputPath = 'logs'
    )

    Write-Host '🚌 Bus Buddy XAML Comprehensive Report Generator' -ForegroundColor Cyan

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        Write-Host '❌ Bus Buddy project root not found' -ForegroundColor Red
        return
    }

    $outputDir = if ([System.IO.Path]::IsPathRooted($OutputPath)) {
        $OutputPath
    } else {
        Join-Path $projectRoot $OutputPath
    }

    if (-not (Test-Path $outputDir)) {
        New-Item -Path $outputDir -ItemType Directory -Force | Out-Null
    }

    $timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
    $reportDir = Join-Path $outputDir "XAML-Analysis-$timestamp"
    New-Item -Path $reportDir -ItemType Directory -Force | Out-Null

    Write-Host "📁 Generating reports in: $reportDir" -ForegroundColor White

    # Run all analyses
    $viewsPath = Join-Path $projectRoot 'BusBuddy.WPF\Views'
    $resourcesPath = Join-Path $projectRoot 'BusBuddy.WPF\Resources'

    if (Test-Path $viewsPath) {
        Write-Host "`n🔍 Analyzing Views..." -ForegroundColor Yellow
        Start-XamlAnalysis -Path $viewsPath -Detailed -ExportReport -ReportPath (Join-Path $reportDir 'syntax-analysis.json')
        Start-XamlQualityInspection -Path $viewsPath -DeepScan -SecurityCheck -AccessibilityCheck -GenerateReport -ReportPath (Join-Path $reportDir 'quality-report.html')
    }

    if (Test-Path $resourcesPath) {
        Write-Host "`n🎨 Analyzing Resources..." -ForegroundColor Yellow
        Start-XamlAnalysis -Path $resourcesPath -Detailed -ExportReport -ReportPath (Join-Path $reportDir 'resources-analysis.json')
    }

    # Create summary report
    $summaryPath = Join-Path $reportDir 'summary.md'
    $summary = @"
# Bus Buddy XAML Analysis Summary

Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')

## Project Structure
- Views Path: $viewsPath
- Resources Path: $resourcesPath

## Reports Generated
- **Quality Report**: [quality-report.html](quality-report.html)
- **Syntax Analysis**: [syntax-analysis.json](syntax-analysis.json)
- **Resources Analysis**: [resources-analysis.json](resources-analysis.json)

## Quick Actions
- Open quality report: ``Invoke-Item "$reportDir\quality-report.html"``
- View syntax issues: ``Get-Content "$reportDir\syntax-analysis.json" | ConvertFrom-Json``

## Bus Buddy Specific Checks
- ✅ Syncfusion FluentDark theme compliance
- ✅ MVVM pattern adherence
- ✅ Naming convention validation
- ✅ Resource usage optimization

---
Generated by Bus Buddy XAML Toolkit
"@

    $summary | Out-File $summaryPath -Encoding UTF8

    Write-Host "`n✨ Comprehensive analysis complete!" -ForegroundColor Green
    Write-Host "📄 Summary: $summaryPath" -ForegroundColor White
    Write-Host '🌐 Quality Report: ' -ForegroundColor White -NoNewline
    Write-Host (Join-Path $reportDir 'quality-report.html') -ForegroundColor Cyan

    Write-Host "`n💡 Quick commands:" -ForegroundColor Yellow
    Write-Host "  Open reports: Invoke-Item `"$reportDir`"" -ForegroundColor Gray
    Write-Host "  View quality: Invoke-Item `"$(Join-Path $reportDir 'quality-report.html')`"" -ForegroundColor Gray
}

function Show-BusBuddyXamlHelp {
    <#
    .SYNOPSIS
        Show help for Bus Buddy XAML toolkit functions
    .ALIAS
        bb-xaml-help
    #>

    Write-Host '🚌 Bus Buddy XAML Toolkit - Available Commands' -ForegroundColor Cyan
    Write-Host '=' * 60 -ForegroundColor Cyan

    $commands = @(
        @{ Name = 'Invoke-BusBuddyXamlAnalysis'; Alias = 'bb-xaml-analyze'; Description = 'Quick XAML syntax analysis' }
        @{ Name = 'Invoke-BusBuddyXamlInspection'; Alias = 'bb-xaml-inspect'; Description = 'Comprehensive quality inspection' }
        @{ Name = 'Get-BusBuddyXamlStructure'; Alias = 'bb-xaml-structure'; Description = 'Architectural and structure analysis' }
        @{ Name = 'Test-BusBuddyXamlValidation'; Alias = 'bb-xaml-validate'; Description = 'Quick validation of XAML files' }
        @{ Name = 'New-BusBuddyXamlReport'; Alias = 'bb-xaml-report'; Description = 'Generate comprehensive analysis report' }
        @{ Name = 'Show-BusBuddyXamlHelp'; Alias = 'bb-xaml-help'; Description = 'Show this help message' }
    )

    foreach ($cmd in $commands) {
        Write-Host "`n🔧 " -ForegroundColor Yellow -NoNewline
        Write-Host $cmd.Alias -ForegroundColor White -NoNewline
        Write-Host ' (' -ForegroundColor Gray -NoNewline
        Write-Host $cmd.Name -ForegroundColor DarkGray -NoNewline
        Write-Host ') - ' -ForegroundColor Gray -NoNewline
        Write-Host $cmd.Description -ForegroundColor Gray

        # Show example usage
        $example = switch ($cmd.Alias) {
            'bb-xaml-analyze' { "bb-xaml-analyze -Path `"Views\Main`" -Detailed" }
            'bb-xaml-inspect' { "bb-xaml-inspect -Path `"MainWindow.xaml`" -Deep -Report" }
            'bb-xaml-structure' { "bb-xaml-structure -Path `"Views\Dashboard`" -ShowTree -Bindings" }
            'bb-xaml-validate' { 'bb-xaml-validate' }
            'bb-xaml-report' { 'bb-xaml-report' }
            default { "Get-Help $($cmd.Name) -Examples" }
        }

        Write-Host '  Example: ' -ForegroundColor DarkGray -NoNewline
        Write-Host $example -ForegroundColor DarkCyan
    }

    Write-Host "`n💡 Tips:" -ForegroundColor Yellow
    Write-Host '  • All paths are relative to Bus Buddy project root' -ForegroundColor Gray
    Write-Host '  • Use -Detailed, -Deep, or -Report for more comprehensive analysis' -ForegroundColor Gray
    Write-Host '  • Reports are saved to the logs directory' -ForegroundColor Gray
    Write-Host '  • For detailed help: Get-Help <command-name> -Full' -ForegroundColor Gray

    Write-Host "`n🎯 Bus Buddy Specific Features:" -ForegroundColor Yellow
    Write-Host '  • Syncfusion theme validation' -ForegroundColor Gray
    Write-Host '  • MVVM pattern checking' -ForegroundColor Gray
    Write-Host '  • Coding standards compliance' -ForegroundColor Gray
    Write-Host '  • Performance optimization suggestions' -ForegroundColor Gray
}

#endregion

#region Helper Functions

function Get-BusBuddyProjectRoot {
    <#
    .SYNOPSIS
        Find the Bus Buddy project root directory
    #>

    $currentPath = $PWD.Path

    # Look for Bus Buddy solution file
    while ($currentPath) {
        if (Test-Path (Join-Path $currentPath 'BusBuddy.sln')) {
            return $currentPath
        }

        $parentPath = Split-Path $currentPath -Parent
        if ($parentPath -eq $currentPath) {
            break
        }
        $currentPath = $parentPath
    }

    # Check common Bus Buddy locations
    $commonPaths = @(
        'C:\Users\steve.mckitrick\Desktop\Bus Buddy',
        (Join-Path $env:USERPROFILE 'Desktop\Bus Buddy'),
        (Join-Path $env:USERPROFILE 'source\repos\BusBuddy'),
        (Join-Path $PWD.Path 'Bus Buddy')
    )

    foreach ($path in $commonPaths) {
        if (Test-Path (Join-Path $path 'BusBuddy.sln')) {
            return $path
        }
    }

    return $null
}

function Test-XamlSyntax {
    <#
    .SYNOPSIS
        Quick XAML syntax validation
    .PARAMETER FilePath
        Path to XAML file
    #>
    param([string]$FilePath)

    try {
        $content = Get-Content $FilePath -Raw
        $xml = [xml]$content
        # Validate the XML structure is well-formed
        return ($xml -and $xml.DocumentElement)
    } catch {
        return $false
    }
}

#endregion

# Auto-completion for Bus Buddy paths
Register-ArgumentCompleter -CommandName Invoke-BusBuddyXamlAnalysis, Invoke-BusBuddyXamlInspection, Get-BusBuddyXamlStructure, Test-BusBuddyXamlValidation -ParameterName Path -ScriptBlock {
    param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameters)

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) { return @() }

    $searchPaths = @(
        'BusBuddy.WPF\Views',
        'BusBuddy.WPF\Resources',
        'BusBuddy.WPF\Controls',
        'BusBuddy.WPF\Assets'
    )

    $suggestions = @()

    foreach ($searchPath in $searchPaths) {
        $fullPath = Join-Path $projectRoot $searchPath
        if (Test-Path $fullPath) {
            $xamlFiles = Get-ChildItem $fullPath -Filter '*.xaml' -Recurse -File
            foreach ($file in $xamlFiles) {
                $relativePath = $file.FullName.Replace("$projectRoot\", '')
                if ($relativePath -like "$wordToComplete*") {
                    $suggestions += $relativePath
                }
            }
        }
    }

    return $suggestions | Sort-Object | Select-Object -Unique
}

# Create aliases for backward compatibility and convenience
Set-Alias -Name bb-xaml-analyze -Value Invoke-BusBuddyXamlAnalysis
Set-Alias -Name bb-xaml-inspect -Value Invoke-BusBuddyXamlInspection
Set-Alias -Name bb-xaml-structure -Value Get-BusBuddyXamlStructure
Set-Alias -Name bb-xaml-validate -Value Test-BusBuddyXamlValidation
Set-Alias -Name bb-xaml-report -Value New-BusBuddyXamlReport
Set-Alias -Name bb-xaml-help -Value Show-BusBuddyXamlHelp

# Export functions for use (only if running as module)
if ($MyInvocation.MyCommand.ModuleName) {
    # Note: Functions are available when script is dot-sourced
    # Available functions: Invoke-BusBuddyXamlAnalysis, Invoke-BusBuddyXamlInspection, Get-BusBuddyXamlStructure, Test-BusBuddyXamlValidation, New-BusBuddyXamlReport, Show-BusBuddyXamlHelp, Get-BusBuddyProjectRoot, Test-XamlSyntax
}

# Show welcome message if loaded interactively
if ($MyInvocation.InvocationName -eq '.') {
    Write-Host '🚌 Bus Buddy XAML Toolkit Loaded!' -ForegroundColor Green
    Write-Host "Type 'bb-xaml-help' for available commands" -ForegroundColor Yellow
}
