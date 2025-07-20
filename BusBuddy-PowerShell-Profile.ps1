#Requires -Version 7.0
<#
.SYNOPSIS
    Bus Buddy PowerShell Development Profile

.DESCRIPTION
    Enhanced PowerShell profile for Bus Buddy WPF development
    Includes XAML analysis tools, project helpers, and development utilities

.NOTES
    Load this profile in VS Code terminal or PowerShell session for enhanced Bus Buddy development
#>

# Import XAML Analysis Toolkit
$XamlToolkitPath = Join-Path $PSScriptRoot 'Tools\Scripts\BusBuddy-XAML-Toolkit.ps1'
if (Test-Path $XamlToolkitPath) {
    . $XamlToolkitPath
    Write-Host '🚌 Bus Buddy XAML Toolkit loaded' -ForegroundColor Green
}

# Import Advanced XAML Health Suite
$HealthSuitePath = Join-Path $PSScriptRoot 'Tools\Scripts\XAML-Health-Suite.ps1'
if (Test-Path $HealthSuitePath) {
    . $HealthSuitePath
    Write-Host '🏥 Advanced XAML Health Suite loaded' -ForegroundColor Magenta
}

# Import Read-Only Analysis Tools
$ReadOnlyToolsPath = Join-Path $PSScriptRoot 'Tools\Scripts\Read-Only-Analysis-Tools.ps1'
$ErrorAnalysisPath = Join-Path $PSScriptRoot 'Tools\Scripts\Error-Analysis.ps1'

if (Test-Path $ReadOnlyToolsPath) {
    Import-Module $ReadOnlyToolsPath -Force
    Write-Host '� Read-Only Analysis Tools loaded' -ForegroundColor Cyan
}

if (Test-Path $ErrorAnalysisPath) {
    # Error analysis script available
    Write-Host '📊 Error Analysis Tools available' -ForegroundColor Cyan
}

# Bus Buddy Project Helper Functions
function Set-BusBuddyLocation {
    <#
    .SYNOPSIS
        Navigate to Bus Buddy project root
    #>
    $root = Get-BusBuddyProjectRoot
    if ($root) {
        Set-Location $root
        Write-Host '📁 Navigated to Bus Buddy project root' -ForegroundColor Green
    } else {
        Write-Host '❌ Bus Buddy project not found' -ForegroundColor Red
    }
}

function Get-BusBuddyViews {
    <#
    .SYNOPSIS
        Navigate to Views directory
    #>
    $root = Get-BusBuddyProjectRoot
    if ($root) {
        $viewsPath = Join-Path $root 'BusBuddy.WPF\Views'
        if (Test-Path $viewsPath) {
            Set-Location $viewsPath
            Write-Host '📁 Navigated to Views directory' -ForegroundColor Green
        } else {
            Write-Host '❌ Views directory not found' -ForegroundColor Red
        }
    }
}

function Get-BusBuddyResources {
    <#
    .SYNOPSIS
        Navigate to Resources directory
    #>
    $root = Get-BusBuddyProjectRoot
    if ($root) {
        $resourcesPath = Join-Path $root 'BusBuddy.WPF\Resources'
        if (Test-Path $resourcesPath) {
            Set-Location $resourcesPath
            Write-Host '📁 Navigated to Resources directory' -ForegroundColor Green
        } else {
            Write-Host '❌ Resources directory not found' -ForegroundColor Red
        }
    }
}

function Get-BusBuddyTools {
    <#
    .SYNOPSIS
        Navigate to Tools directory
    #>
    $root = Get-BusBuddyProjectRoot
    if ($root) {
        $toolsPath = Join-Path $root 'Tools'
        if (Test-Path $toolsPath) {
            Set-Location $toolsPath
            Write-Host '📁 Navigated to Tools directory' -ForegroundColor Green
        } else {
            Write-Host '❌ Tools directory not found' -ForegroundColor Red
        }
    }
}

function Get-BusBuddyLogs {
    <#
    .SYNOPSIS
        Open logs directory
    #>
    $root = Get-BusBuddyProjectRoot
    if ($root) {
        $logsPath = Join-Path $root 'logs'
        if (Test-Path $logsPath) {
            Invoke-Item $logsPath
            Write-Host '📂 Opened logs directory' -ForegroundColor Green
        } else {
            Write-Host '❌ Logs directory not found' -ForegroundColor Red
        }
    }
}

# XAML Quick Actions
function Test-BusBuddyHealth {
    <#
    .SYNOPSIS
        Quick health check of Bus Buddy XAML files
    #>
    Write-Host '🚌 Bus Buddy Quick Health Check' -ForegroundColor Cyan

    $root = Get-BusBuddyProjectRoot
    if (-not $root) {
        Write-Host '❌ Bus Buddy project not found' -ForegroundColor Red
        return
    }

    # Quick validation
    bb-xaml-validate

    Write-Host "`n💡 For detailed analysis, use:" -ForegroundColor Yellow
    Write-Host '  bb-xaml-inspect -Deep -Report' -ForegroundColor Gray
    Write-Host '  bb-xaml-report' -ForegroundColor Gray
}

function Test-BusBuddySyntax {
    <#
    .SYNOPSIS
        Quick syntax check of current XAML file or directory
    #>
    param(
        [Parameter(Mandatory = $false)]
        [string]$Path = '.'
    )

    if ($Path -eq '.') {
        $currentFiles = Get-ChildItem '*.xaml' -ErrorAction SilentlyContinue
        if ($currentFiles.Count -gt 0) {
            Write-Host '🔍 Checking XAML files in current directory...' -ForegroundColor Cyan
            bb-xaml-analyze -Path $PWD.Path
        } else {
            Write-Host '⚠️ No XAML files found in current directory' -ForegroundColor Yellow
            Write-Host "💡 Use 'bb-syntax <path>' to specify a different location" -ForegroundColor Gray
        }
    } else {
        bb-xaml-analyze -Path $Path
    }
}

# Aliases for convenience
Set-Alias -Name 'bb-home' -Value 'bb-root'
Set-Alias -Name 'bb-v' -Value 'bb-views'
Set-Alias -Name 'bb-r' -Value 'bb-resources'
Set-Alias -Name 'bb-t' -Value 'bb-tools'
Set-Alias -Name 'bb-l' -Value 'bb-logs'

# Enhanced prompt for Bus Buddy development
function prompt {
    $currentPath = $PWD.Path
    $root = Get-BusBuddyProjectRoot

    if ($root -and $currentPath.StartsWith($root)) {
        $relativePath = $currentPath.Substring($root.Length).TrimStart('\')
        if (-not $relativePath) { $relativePath = 'root' }

        Write-Host '🚌 ' -NoNewline -ForegroundColor Yellow
        Write-Host 'Bus Buddy' -NoNewline -ForegroundColor Cyan
        Write-Host ' 📁 ' -NoNewline -ForegroundColor Gray
        Write-Host $relativePath -NoNewline -ForegroundColor White
        Write-Host ' > ' -NoNewline -ForegroundColor Gray
    } else {
        Write-Host 'PS ' -NoNewline -ForegroundColor Blue
        Write-Host $currentPath -NoNewline -ForegroundColor White
        Write-Host ' > ' -NoNewline -ForegroundColor Gray
    }

    return ' '
}

# Welcome message
Write-Host ''
Write-Host '🚌 ' -NoNewline -ForegroundColor Yellow
Write-Host 'Bus Buddy PowerShell Profile Loaded!' -ForegroundColor Cyan
Write-Host "   • Use 'bb-xaml-help' for XAML analysis commands" -ForegroundColor Gray
Write-Host "   • Use 'bb-xaml-edit' for structure-aware XAML editing" -ForegroundColor Gray
Write-Host "   • Use 'bb-xaml-format' for safe XAML formatting" -ForegroundColor Gray
Write-Host "   • Use 'bb-check' for quick project health check" -ForegroundColor Gray
Write-Host "   • Use 'bb-health' for comprehensive XAML health analysis" -ForegroundColor Magenta
Write-Host "   • Use 'bb-script-analyze' to run PSScriptAnalyzer on scripts" -ForegroundColor Gray
Write-Host "   • Use 'bb-export-diag' to export diagnostic data (ConvertTo-CliXml)" -ForegroundColor Gray
Write-Host "   • Use 'bb-validate-syncfusion' to validate Syncfusion namespaces" -ForegroundColor Gray
Write-Host "   • Use 'bb-root' to navigate to project root" -ForegroundColor Gray
Write-Host ''

# PowerShell 7.5.2 optimized aliases for enhanced analysis
Set-Alias -Name bb-xaml-edit -Value Invoke-XamlElementEdit
Set-Alias -Name bb-xaml-add -Value Invoke-XamlElementInsertion
Set-Alias -Name bb-xaml-attr -Value Invoke-XamlAttributeEdit
Set-Alias -Name bb-xaml-validate -Value Invoke-XamlValidation
Set-Alias -Name bb-xaml-format -Value Invoke-XamlFormatting
Set-Alias -Name bb-script-analyze -Value Invoke-PowerShellScriptAnalyzer
Set-Alias -Name bb-export-diag -Value Export-BusBuddyDiagnosticData
Set-Alias -Name bb-validate-syncfusion -Value Validate-SyncfusionNamespaces

# Quick XAML helpers
function New-BusBuddyXamlButton { param($File, $Parent = '//Grid', $Name = 'MyButton', $Content = 'Click Me') New-SyncfusionButton -XamlFilePath $File -ParentXPath $Parent -Name $Name -Content $Content }
function New-BusBuddyXamlBinding { param($File, $Element, $Property, $Path) Add-DataBinding -XamlFilePath $File -ElementXPath $Element -Property $Property -BindingPath $Path }

# Tab completion for bb commands - PowerShell 7.5.2 optimized
$bbCommands = @(
    'bb-xaml-analyze', 'bb-xaml-inspect', 'bb-xaml-structure', 'bb-xaml-validate', 'bb-xaml-report', 'bb-xaml-help',
    'bb-xaml-edit', 'bb-xaml-add', 'bb-xaml-attr', 'bb-xaml-format', 'bb-xaml-button', 'bb-xaml-bind',
    'bb-health', 'bb-quick-health', 'bb-null-check', 'bb-perf', 'bb-types', 'bb-bindings',
    'bb-root', 'bb-views', 'bb-resources', 'bb-tools', 'bb-logs', 'bb-check', 'bb-syntax',
    'bb-script-analyze', 'bb-export-diag', 'bb-validate-syncfusion'
)

Register-ArgumentCompleter -CommandName $bbCommands -ParameterName Path -ScriptBlock {
    param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameters)

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) { return @() }

    # PowerShell 7.5.2 optimized - Use parallel processing for large directories
    $items = Get-ChildItem $projectRoot -Recurse -ErrorAction SilentlyContinue | Where-Object {
        $_.Extension -eq '.xaml' -or $_.PSIsContainer
    } | ForEach-Object -Parallel {
        $item = $_
        $projectRoot = $using:projectRoot
        $wordToComplete = $using:wordToComplete

        $relativePath = $item.FullName.Replace("$projectRoot\", '')
        if ($relativePath -like "$wordToComplete*") {
            return $relativePath
        }
    } -ThrottleLimit 4 | Sort-Object | Select-Object -First 20

    return $items
}


# Backward Compatibility Aliases
Set-Alias -Name 'bb-logs' -Value 'Get-BusBuddyLogs' -Force
Set-Alias -Name 'bb-views' -Value 'Get-BusBuddyViews' -Force
Set-Alias -Name 'bb-check' -Value 'Test-BusBuddyHealth' -Force
Set-Alias -Name 'bb-resources' -Value 'Get-BusBuddyResources' -Force
Set-Alias -Name 'bb-tools' -Value 'Get-BusBuddyTools' -Force
Set-Alias -Name 'bb-syntax' -Value 'Test-BusBuddySyntax' -Force
Set-Alias -Name 'bb-xaml-button' -Value 'New-BusBuddyXamlButton' -Force
Set-Alias -Name 'bb-xaml-bind' -Value 'New-BusBuddyXamlBinding' -Force
Set-Alias -Name 'bb-root' -Value 'Set-BusBuddyLocation' -Force

