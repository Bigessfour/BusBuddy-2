#Requires -Version 7.5
#Requires -Modules Pester

<#
.SYNOPSIS
Pester configuration for BusBuddy PowerShell modules testing

.DESCRIPTION
Configures Pester testing framework for BusBuddy PowerShell modules with proper coverage reporting,
output formatting, and integration with VS Code test explorer.

.NOTES
Author: BusBuddy Development Team
Created: July 26, 2025
Framework: Pester 5.x
Compliance: BusBuddy Phase 1 Testing Standards
#>

[Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseDeclaredVarsMoreThanAssignments', '')]
param()

# Import required modules
Import-Module Pester -Force

# Configure Pester for BusBuddy
$PesterConfig = [PesterConfiguration]::Default

# General Configuration
$PesterConfig.Run.Path = @(
    'PowerShell\BusBuddy PowerShell Environment\Tests',
    'PowerShell\BusBuddy PowerShell Environment\Modules\BusBuddy.AI\Tests'
)
$PesterConfig.Run.PassThru = $true
$PesterConfig.Run.Exit = $false

# Test Discovery
$PesterConfig.Filter.Tag = @()
$PesterConfig.Filter.ExcludeTag = @('Slow', 'Integration')

# Output Configuration
$PesterConfig.Output.Verbosity = 'Detailed'
$PesterConfig.Output.StackTraceVerbosity = 'FirstLine'

# Code Coverage
$PesterConfig.CodeCoverage.Enabled = $true
$PesterConfig.CodeCoverage.Path = @(
    'PowerShell\BusBuddy PowerShell Environment\Modules\BusBuddy.AI\*.psm1',
    'PowerShell\BusBuddy PowerShell Environment\*.ps1'
)
$PesterConfig.CodeCoverage.OutputFormat = 'JaCoCo'
$PesterConfig.CodeCoverage.OutputPath = 'logs\coverage\coverage.xml'
$PesterConfig.CodeCoverage.CoveragePercentTarget = 80

# Test Results
$PesterConfig.TestResult.Enabled = $true
$PesterConfig.TestResult.OutputFormat = 'NUnitXml'
$PesterConfig.TestResult.OutputPath = 'logs\test-results\test-results.xml'

# Should Configuration (BusBuddy specific)
$PesterConfig.Should.ErrorAction = 'Stop'

# Export the configuration
$PesterConfig | Export-Clixml -Path 'PowerShell\BusBuddy PowerShell Environment\PesterConfig.xml'

Write-Host "✅ Pester configuration created successfully" -ForegroundColor Green
Write-Host "📁 Configuration saved to: PowerShell\BusBuddy PowerShell Environment\PesterConfig.xml" -ForegroundColor Cyan
Write-Host "🧪 Test paths configured:" -ForegroundColor Yellow
$PesterConfig.Run.Path | ForEach-Object { Write-Host "   - $_" -ForegroundColor Gray }
Write-Host "📊 Coverage target: $($PesterConfig.CodeCoverage.CoveragePercentTarget)%" -ForegroundColor Yellow
