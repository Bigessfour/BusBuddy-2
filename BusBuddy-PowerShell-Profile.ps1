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

# Import Advanced Workflows
$AdvancedWorkflowsPath = Join-Path $PSScriptRoot 'BusBuddy-Advanced-Workflows.ps1'
if (Test-Path $AdvancedWorkflowsPath) {
    . $AdvancedWorkflowsPath
    Write-Host '🚀 Advanced Development Workflows loaded' -ForegroundColor Blue
}

# Import Read-Only Analysis Tools
$ReadOnlyToolsPath = Join-Path $PSScriptRoot 'Tools\Scripts\Read-Only-Analysis-Tools.ps1'
$ErrorAnalysisPath = Join-Path $PSScriptRoot 'Tools\Scripts\Error-Analysis.ps1'

if (Test-Path $ReadOnlyToolsPath) {
    Import-Module $ReadOnlyToolsPath -Force
    Write-Host '📊 Read-Only Analysis Tools loaded' -ForegroundColor Cyan
}

if (Test-Path $ErrorAnalysisPath) {
    # Error analysis script available
    Write-Host '📊 Error Analysis Tools available' -ForegroundColor Cyan
}

# ==== CORE HELPER FUNCTIONS ====

function Get-BusBuddyProjectRoot {
    <#
    .SYNOPSIS
        Find the Bus Buddy project root directory
    #>
    $current = $PWD.Path

    # Look for solution file to identify project root
    while ($current) {
        if (Test-Path (Join-Path $current "BusBuddy.sln")) {
            return $current
        }

        $parent = Split-Path $current -Parent
        if ($parent -eq $current) { break }
        $current = $parent
    }

    return $null
}

# ==== STREAMLINED WORKFLOW FUNCTIONS ====

function Invoke-BusBuddyBuild {
    <#
    .SYNOPSIS
        Build the Bus Buddy solution with optimized settings for UI iteration
    #>
    param(
        [switch]$Clean,
        [switch]$NoLogo,
        [switch]$Quiet
    )

    $root = Get-BusBuddyProjectRoot
    if (-not $root) {
        Write-Host '❌ Bus Buddy project not found' -ForegroundColor Red
        return $false
    }

    Push-Location $root

    try {
        $buildArgs = @('build', 'BusBuddy.sln')

        if ($Clean) {
            Write-Host '🧹 Cleaning solution...' -ForegroundColor Cyan
            dotnet clean BusBuddy.sln --verbosity quiet
        }

        if ($NoLogo) { $buildArgs += '--nologo' }
        if ($Quiet) { $buildArgs += '--verbosity', 'quiet' }

        Write-Host '🔨 Building Bus Buddy solution...' -ForegroundColor Green
        $buildResult = & dotnet @buildArgs

        if ($LASTEXITCODE -eq 0) {
            Write-Host '✅ Build completed successfully' -ForegroundColor Green
            return $true
        } else {
            Write-Host '❌ Build failed' -ForegroundColor Red
            return $false
        }
    } finally {
        Pop-Location
    }
}

function Invoke-BusBuddyRestore {
    <#
    .SYNOPSIS
        Restore NuGet packages for the Bus Buddy solution
    #>
    param(
        [switch]$Force,
        [switch]$NoCache,
        [switch]$Verbose
    )

    $root = Get-BusBuddyProjectRoot
    if (-not $root) {
        Write-Host '❌ Bus Buddy project not found' -ForegroundColor Red
        return $false
    }

    Push-Location $root

    try {
        $restoreArgs = @('restore', 'BusBuddy.sln')

        if ($Force) { $restoreArgs += '--force' }
        if ($NoCache) { $restoreArgs += '--no-cache' }
        if ($Verbose) { $restoreArgs += '--verbosity', 'normal' }

        Write-Host '📦 Restoring NuGet packages...' -ForegroundColor Cyan
        $restoreResult = & dotnet @restoreArgs

        if ($LASTEXITCODE -eq 0) {
            Write-Host '✅ Package restore completed successfully' -ForegroundColor Green
            return $true
        } else {
            Write-Host '❌ Package restore failed' -ForegroundColor Red
            return $false
        }
    } finally {
        Pop-Location
    }
}

function Invoke-BusBuddyRun {
    <#
    .SYNOPSIS
        Run the Bus Buddy WPF application
    #>
    param(
        [switch]$NoBuild,
        [switch]$Debug
    )

    $root = Get-BusBuddyProjectRoot
    if (-not $root) {
        Write-Host '❌ Bus Buddy project not found' -ForegroundColor Red
        return
    }

    Push-Location $root

    try {
        $runArgs = @('run', '--project', 'BusBuddy.WPF/BusBuddy.WPF.csproj')

        if ($NoBuild) { $runArgs += '--no-build' }
        if ($Debug) { $runArgs += '--configuration', 'Debug' }

        Write-Host '🚌 Starting Bus Buddy application...' -ForegroundColor Yellow
        & dotnet @runArgs
    } finally {
        Pop-Location
    }
}

function Invoke-BusBuddyTest {
    <#
    .SYNOPSIS
        Run Bus Buddy tests
    #>
    param(
        [switch]$NoBuild,
        [string]$Filter = $null
    )

    $root = Get-BusBuddyProjectRoot
    if (-not $root) {
        Write-Host '❌ Bus Buddy project not found' -ForegroundColor Red
        return
    }

    Push-Location $root

    try {
        $testArgs = @('test', 'BusBuddy.Tests/BusBuddy.Tests.csproj')

        if ($NoBuild) { $testArgs += '--no-build' }
        if ($Filter) { $testArgs += '--filter', $Filter }

        Write-Host '🧪 Running Bus Buddy tests...' -ForegroundColor Magenta
        & dotnet @testArgs
    } finally {
        Pop-Location
    }
}

function Invoke-BusBuddyClean {
    <#
    .SYNOPSIS
        Clean the Bus Buddy solution and temporary files
    #>
    $root = Get-BusBuddyProjectRoot
    if (-not $root) {
        Write-Host '❌ Bus Buddy project not found' -ForegroundColor Red
        return
    }

    Push-Location $root

    try {
        Write-Host '🧹 Cleaning Bus Buddy solution...' -ForegroundColor Cyan
        dotnet clean BusBuddy.sln --verbosity quiet

        # Clean bin and obj directories
        Write-Host '🧹 Cleaning bin and obj directories...' -ForegroundColor Cyan
        Get-ChildItem -Path . -Recurse -Directory -Name @('bin', 'obj') | ForEach-Object {
            $fullPath = Join-Path $PWD.Path $_
            if (Test-Path $fullPath) {
                Remove-Item $fullPath -Recurse -Force -ErrorAction SilentlyContinue
                Write-Host "   Removed: $_" -ForegroundColor Gray
            }
        }

        Write-Host '✅ Clean completed successfully' -ForegroundColor Green
    } finally {
        Pop-Location
    }
}

function Invoke-BusBuddyUICycle {
    <#
    .SYNOPSIS
        Complete UI beautification iteration cycle: validate themes, rebuild, run app
    #>
    param(
        [switch]$SkipThemeCheck,
        [switch]$SkipBuild,
        [switch]$SkipRun
    )

    Write-Host '🎨 Starting Bus Buddy UI Iteration Cycle' -ForegroundColor Magenta
    Write-Host '=======================================' -ForegroundColor Magenta

    $success = $true

    # Step 1: Theme validation
    if (-not $SkipThemeCheck) {
        Write-Host "`n🎨 Step 1: Validating themes..." -ForegroundColor Cyan
        $themeResult = Invoke-BusBuddyThemeCheck
        if (-not $themeResult) {
            Write-Host '⚠️ Theme validation found issues, but continuing...' -ForegroundColor Yellow
        }
    }

    # Step 2: Build
    if (-not $SkipBuild) {
        Write-Host "`n🔨 Step 2: Building solution..." -ForegroundColor Cyan
        $buildResult = Invoke-BusBuddyBuild -NoLogo -Quiet
        if (-not $buildResult) {
            Write-Host '❌ Build failed, stopping UI cycle' -ForegroundColor Red
            return $false
        }
    }

    # Step 3: Run app for preview
    if (-not $SkipRun) {
        Write-Host "`n🚌 Step 3: Launching app for UI preview..." -ForegroundColor Cyan
        Write-Host '💡 Close the application when done reviewing UI changes' -ForegroundColor Yellow
        Invoke-BusBuddyRun -NoBuild
    }

    Write-Host "`n✅ UI iteration cycle completed" -ForegroundColor Green
    return $true
}

function Invoke-BusBuddyValidateUI {
    <#
    .SYNOPSIS
        Run the Validate UI task and show results
    #>
    $root = Get-BusBuddyProjectRoot
    if (-not $root) {
        Write-Host '❌ Bus Buddy project not found' -ForegroundColor Red
        return
    }

    Push-Location $root

    try {
        Write-Host '🔍 Running UI validation...' -ForegroundColor Cyan
        $validationScript = 'Tools/Scripts/Syncfusion-Implementation-Validator.ps1'

        if (Test-Path $validationScript) {
            & pwsh -ExecutionPolicy Bypass -File $validationScript
        } else {
            Write-Host '❌ UI validation script not found' -ForegroundColor Red
        }
    } finally {
        Pop-Location
    }
}

function Invoke-BusBuddyThemeCheck {
    <#
    .SYNOPSIS
        Run the theme check script and return success status
    #>
    $root = Get-BusBuddyProjectRoot
    if (-not $root) {
        Write-Host '❌ Bus Buddy project not found' -ForegroundColor Red
        return $false
    }

    Push-Location $root

    try {
        Write-Host '🎨 Running theme consistency check...' -ForegroundColor Magenta
        $themeScript = 'Tools/Scripts/bb-theme-check.ps1'

        if (Test-Path $themeScript) {
            $result = & pwsh -ExecutionPolicy Bypass -File $themeScript
            return $LASTEXITCODE -eq 0
        } else {
            Write-Host '❌ Theme check script not found' -ForegroundColor Red
            return $false
        }
    } finally {
        Pop-Location
    }
}

# ==== LOG MONITORING FUNCTIONS ====

function Watch-BusBuddyLogs {
    <#
    .SYNOPSIS
        Monitor Bus Buddy application logs in real-time using enhanced log monitor
    #>
    param(
        [int]$Lines = 10,
        [switch]$Follow,
        [string]$LogType = "application"
    )

    $enhancedLogScript = Join-Path $PSScriptRoot "Tools\Scripts\Watch-BusBuddyLogs.ps1"

    if (Test-Path $enhancedLogScript) {
        $args = @()
        $args += "-LogType", $LogType
        $args += "-Lines", $Lines
        if ($Follow) { $args += "-Follow" }
        $args += "-Colorized"

        & pwsh -ExecutionPolicy Bypass -File $enhancedLogScript @args
    } else {
        # Fallback to basic implementation
        $root = Get-BusBuddyProjectRoot
        if (-not $root) {
            Write-Host '❌ Bus Buddy project not found' -ForegroundColor Red
            return
        }

        $logsPath = Join-Path $root "BusBuddy.WPF\logs"

        # Ensure logs directory exists
        if (-not (Test-Path $logsPath)) {
            Write-Host "📁 Creating logs directory: $logsPath" -ForegroundColor Yellow
            New-Item -ItemType Directory -Path $logsPath -Force | Out-Null
        }

        # Find the most recent log file
        $pattern = switch ($LogType) {
            "application" { "application-*.log" }
            "errors" { "errors-actionable-*.log" }
            "ui" { "ui-interactions-*.log" }
            default { "*.log" }
        }

        $logFiles = Get-ChildItem -Path $logsPath -Filter $pattern | Sort-Object LastWriteTime -Descending

        if ($logFiles.Count -eq 0) {
            Write-Host "📝 No $LogType log files found. Starting log monitor..." -ForegroundColor Yellow
            Write-Host "💡 Run the application to generate logs" -ForegroundColor Gray

            # Monitor for new log files
            do {
                Start-Sleep -Seconds 2
                $logFiles = Get-ChildItem -Path $logsPath -Filter $pattern | Sort-Object LastWriteTime -Descending
            } while ($logFiles.Count -eq 0)
        }

        $latestLog = $logFiles[0]
        Write-Host "📖 Monitoring: $($latestLog.Name)" -ForegroundColor Green

        if ($Follow) {
            Write-Host "👀 Following log (Ctrl+C to stop)..." -ForegroundColor Cyan
            Get-Content $latestLog.FullName -Tail $Lines -Wait
        } else {
            Write-Host "📋 Last $Lines lines:" -ForegroundColor Cyan
            Get-Content $latestLog.FullName -Tail $Lines
        }
    }
}

function Watch-BusBuddyErrors {
    <#
    .SYNOPSIS
        Monitor Bus Buddy error logs with actionable information
    #>
    param(
        [int]$Lines = 10,
        [switch]$Follow
    )

    Write-Host '🚨 Monitoring actionable errors...' -ForegroundColor Red
    Watch-BusBuddyLogs -Lines $Lines -Follow:$Follow -LogType "errors"
}

function Watch-BusBuddyUILogs {
    <#
    .SYNOPSIS
        Monitor Bus Buddy UI interaction logs
    #>
    param(
        [int]$Lines = 10,
        [switch]$Follow
    )

    Write-Host '🖱️ Monitoring UI interactions...' -ForegroundColor Blue
    Watch-BusBuddyLogs -Lines $Lines -Follow:$Follow -LogType "ui"
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

# ==== STREAMLINED LOCAL WORKFLOW ALIASES ====
# Core build and run workflow
Set-Alias -Name 'bb-build' -Value 'Invoke-BusBuddyBuild'
Set-Alias -Name 'bb-restore' -Value 'Invoke-BusBuddyRestore'
Set-Alias -Name 'bb-run' -Value 'Invoke-BusBuddyRun'
Set-Alias -Name 'bb-test' -Value 'Invoke-BusBuddyTest'
Set-Alias -Name 'bb-clean' -Value 'Invoke-BusBuddyClean'

# UI iteration workflow
Set-Alias -Name 'bb-ui-cycle' -Value 'Invoke-BusBuddyUICycle'
Set-Alias -Name 'bb-validate-ui' -Value 'Invoke-BusBuddyValidateUI'
Set-Alias -Name 'bb-theme-check' -Value 'Invoke-BusBuddyThemeCheck'

# Log monitoring
Set-Alias -Name 'bb-logs-tail' -Value 'Watch-BusBuddyLogs'
Set-Alias -Name 'bb-logs-errors' -Value 'Watch-BusBuddyErrors'
Set-Alias -Name 'bb-logs-ui' -Value 'Watch-BusBuddyUILogs'

# Navigation aliases for convenience
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
        Write-Host 'Get-Process ' -NoNewline -ForegroundColor Blue
        Write-Host $currentPath -NoNewline -ForegroundColor White
        Write-Host ' > ' -NoNewline -ForegroundColor Gray
    }

    return ' '
}

# Welcome message
Write-Host ''
Write-Host '🚌 ' -NoNewline -ForegroundColor Yellow
Write-Host 'Bus Buddy PowerShell Profile Loaded!' -ForegroundColor Cyan
Write-Host ''
Write-Host '🚀 STREAMLINED WORKFLOW COMMANDS:' -ForegroundColor Green
Write-Host "   • 'bb-build' - Build the solution quickly" -ForegroundColor Gray
Write-Host "   • 'bb-restore' - Restore NuGet packages" -ForegroundColor Gray
Write-Host "   • 'bb-run' - Run the WPF application" -ForegroundColor Gray
Write-Host "   • 'bb-ui-cycle' - Complete UI iteration: validate → build → run" -ForegroundColor Gray
Write-Host "   • 'bb-logs-tail -Follow' - Monitor app logs in real-time" -ForegroundColor Gray
Write-Host "   • 'bb-logs-errors -Follow' - Monitor actionable errors" -ForegroundColor Gray
Write-Host ''
Write-Host '💡 QUICK TEST CYCLES:' -ForegroundColor Magenta
Write-Host "   • 'bb-build && bb-run' - Build and run in one command" -ForegroundColor Gray
Write-Host "   • 'bb-ui-cycle' - Full UI beautification workflow" -ForegroundColor Gray
Write-Host "   • 'bb-theme-check; bb-build; bb-run' - Complete validation cycle" -ForegroundColor Gray
Write-Host ''
Write-Host '🔧 DEVELOPMENT TOOLS:' -ForegroundColor Blue
Write-Host "   • 'bb-xaml-help' - XAML analysis commands" -ForegroundColor Gray
Write-Host "   • 'bb-health' - Comprehensive XAML health analysis" -ForegroundColor Gray
Write-Host "   • 'bb-root' - Navigate to project root" -ForegroundColor Gray
Write-Host "   • 'bb-views' - Navigate to Views directory" -ForegroundColor Gray
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

