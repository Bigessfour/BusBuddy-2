#Requires -Version 7.5
<#
.SYNOPSIS
    BusBuddy PowerShell Module Loader - AI-First Development Environment

.DESCRIPTION
    Main loader script for BusBuddy PowerShell modules following Microsoft PowerShell module standards.
    Automatically loads both the core BusBuddy module and AI integration modules.

.NOTES
    File Name      : Load-BusBuddyModules.ps1
    Author         : BusBuddy Development Team
    Prerequisite   : PowerShell 7.5+
    Copyright      : (c) 2025 BusBuddy Project

.EXAMPLE
    .\PowerShell\Load-BusBuddyModules.ps1

.EXAMPLE
    # Load with verbose output
    .\PowerShell\Load-BusBuddyModules.ps1 -Verbose
#>

[CmdletBinding()]
param(
    [switch]$Force
)

$ErrorActionPreference = 'Continue'

# Get the PowerShell environment path
$PowerShellEnvPath = Join-Path $PSScriptRoot "BusBuddy PowerShell Environment"
$ModulesPath = Join-Path $PowerShellEnvPath "Modules"

Write-Host "🚌 Loading BusBuddy PowerShell Modules..." -ForegroundColor Cyan

try {
    # Add our modules path to PSModulePath temporarily
    $originalModulePath = $env:PSModulePath
    if ($env:PSModulePath -notlike "*$ModulesPath*") {
        $env:PSModulePath = "$ModulesPath;$env:PSModulePath"
        Write-Verbose "Added $ModulesPath to PSModulePath"
    }

    # Load core BusBuddy module
    $busBuddyModulePath = Join-Path $ModulesPath "BusBuddy"
    if (Test-Path $busBuddyModulePath) {
        Write-Host "📦 Loading BusBuddy Core Module..." -ForegroundColor Yellow
        try {
            Import-Module "BusBuddy" -Force:$Force -Global -ErrorAction Stop
            Write-Host "✅ BusBuddy Core Module loaded successfully" -ForegroundColor Green
        }
        catch {
            Write-Warning "❌ BusBuddy Core Module has syntax errors: $($_.Exception.Message)"
            Write-Host "   Continuing with AI and workflow tools only..." -ForegroundColor Yellow
        }
    }
    else {
        Write-Warning "❌ BusBuddy Core Module not found at: $busBuddyModulePath"
    }

    # Load AI module
    $aiModulePath = Join-Path $ModulesPath "BusBuddy.AI"
    if (Test-Path $aiModulePath) {
        Write-Host "🤖 Loading BusBuddy AI Module..." -ForegroundColor Yellow

        # AI features removed for simplified operation
        Write-Verbose "AI integration disabled - focusing on core BusBuddy functionality"
            Write-Host "Skipping AI module load..." -ForegroundColor Yellow
        }
        else {
            Import-Module "BusBuddy.AI" -Force:$Force -Global
            Write-Host "✅ BusBuddy AI Module loaded successfully" -ForegroundColor Green

            # Initialize AI if XAI key is available
            if ($env:XAI_API_KEY) {
                Write-Host "🤖 Initializing AI environment..." -ForegroundColor Yellow
                try {
                    $initResult = Initialize-BusBuddyAI
                    if ($initResult) {
                        Write-Host "✅ AI environment initialized successfully" -ForegroundColor Green
                    }
                }
                catch {
                    Write-Warning "⚠️ AI initialization failed: $($_.Exception.Message)"
                }
            }
            else {
                Write-Host "ℹ️ XAI_API_KEY not found. AI functions available but not initialized." -ForegroundColor Blue
            }
        }
    }
    else {
        Write-Warning "❌ BusBuddy AI Module not found at: $aiModulePath"
    }

    # Display available commands
    Write-Host "`n🎯 Available BusBuddy Commands:" -ForegroundColor Cyan

    $bbCommands = Get-Command -Name "bb-*" -ErrorAction SilentlyContinue | Sort-Object Name
    if ($bbCommands) {
        $bbCommands | ForEach-Object {
            Write-Host "   $($_.Name)" -ForegroundColor Green
        }
    }
    else {
        Write-Host "   No bb-* commands found" -ForegroundColor Yellow
    }

    # Load workflow tools
    Write-Host "`n🔧 Loading Workflow Tools..." -ForegroundColor Cyan

    # Load Pester if available
    if (Get-Module -ListAvailable -Name Pester) {
        Import-Module Pester -Force -Global
        Write-Host "✅ Pester testing framework loaded" -ForegroundColor Green

        # Create workflow functions with proper variable scope
        function global:Invoke-BusBuddyTests {
            <#
            .SYNOPSIS
            Run BusBuddy Pester tests
            #>
            param([switch]$Coverage)

            # Use hardcoded path to avoid scope issues
            $ScriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
            $BusBuddyEnvPath = Join-Path (Split-Path -Parent $ScriptDirectory) "PowerShell\BusBuddy PowerShell Environment"

            $ConfigPath = Join-Path $BusBuddyEnvPath "PesterConfig.xml"
            if (Test-Path $ConfigPath) {
                $Config = Import-Clixml $ConfigPath
                if (-not $Coverage) {
                    $Config.CodeCoverage.Enabled = $false
                }
                Invoke-Pester -Configuration $Config
            }
            else {
                Invoke-Pester -Path (Join-Path $BusBuddyEnvPath "Modules\*\Tests\*.Tests.ps1")
            }
        }

        New-Alias -Name "bb-test" -Value "Invoke-BusBuddyTests" -Description "Run BusBuddy tests" -Force
    }

    # Load PSScriptAnalyzer if available
    if (Get-Module -ListAvailable -Name PSScriptAnalyzer) {
        Import-Module PSScriptAnalyzer -Force
        Write-Host "✅ PSScriptAnalyzer code quality tool loaded" -ForegroundColor Green

        function global:Invoke-BusBuddyCodeAnalysis {
            <#
            .SYNOPSIS
            Run PSScriptAnalyzer on BusBuddy PowerShell code
            #>
            param([switch]$IncludeDefaultRules)

            # Use hardcoded path to avoid scope issues
            $ScriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
            $BusBuddyEnvPath = Join-Path (Split-Path -Parent $ScriptDirectory) "PowerShell\BusBuddy PowerShell Environment"

            $SettingsPath = Join-Path $BusBuddyEnvPath "PSScriptAnalyzerSettings.psd1"
            if (Test-Path $SettingsPath) {
                Invoke-ScriptAnalyzer -Path $BusBuddyEnvPath -Settings $SettingsPath -Recurse
            }
            else {
                Invoke-ScriptAnalyzer -Path $BusBuddyEnvPath -Recurse
            }
        }

        New-Alias -Name "bb-analyze" -Value "Invoke-BusBuddyCodeAnalysis" -Description "Analyze BusBuddy code quality" -Force
    }

    # Load platyPS if available
    if (Get-Module -ListAvailable -Name platyPS) {
        Import-Module platyPS -Force
        Write-Host "✅ platyPS documentation generator loaded" -ForegroundColor Green

        function global:New-BusBuddyDocumentation {
            <#
            .SYNOPSIS
            Generate documentation for BusBuddy modules
            #>
            param(
                [string]$ModuleName = "BusBuddy.AI",
                [string]$OutputPath = "Documentation\AI"
            )

            if (Get-Module -Name $ModuleName -ErrorAction SilentlyContinue) {
                $OutputDir = Join-Path (Get-Location) $OutputPath
                if (-not (Test-Path $OutputDir)) {
                    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
                }
                New-MarkdownHelp -Module $ModuleName -OutputFolder $OutputDir -Force
                Write-Host "✅ Documentation generated in: $OutputDir" -ForegroundColor Green
            }
            else {
                Write-Warning "Module $ModuleName not loaded"
            }
        }

        New-Alias -Name "bb-docs" -Value "New-BusBuddyDocumentation" -Description "Generate BusBuddy documentation" -Force
    }

    # Add Terminal Flow Monitor function
    function global:Start-TerminalMonitor {
        <#
        .SYNOPSIS
            Start the BusBuddy Terminal Flow Monitor

        .DESCRIPTION
            Launches the PowerShell Gallery-style terminal monitor with real-time error detection

        .PARAMETER MonitorMode
            What to monitor: All, Dots, Progress, Lifecycle, or Custom

        .PARAMETER LogToFile
            Enable logging to timestamped file

        .PARAMETER ShowDots
            Display dots for progress indication

        .PARAMETER WatchCommand
            Specific command to monitor

        .EXAMPLE
            Start-TerminalMonitor -MonitorMode All -LogToFile -ShowDots

        .EXAMPLE
            Start-TerminalMonitor -WatchCommand "dotnet build BusBuddy.sln"
        #>
        param(
            [ValidateSet('All', 'Dots', 'Progress', 'Lifecycle', 'Custom')]
            [string]$MonitorMode = 'All',

            [switch]$LogToFile,

            [switch]$ShowDots,

            [string]$WatchCommand = $null
        )

        $ScriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
        $MonitorScript = Join-Path (Split-Path -Parent $ScriptDirectory) "Tools\Scripts\BusBuddy-Terminal-Flow-Monitor.ps1"

        if (Test-Path $MonitorScript) {
            $Arguments = @(
                '-MonitorMode', $MonitorMode
            )

            if ($LogToFile) { $Arguments += '-LogToFile' }
            if ($ShowDots) { $Arguments += '-ShowDots' }
            if ($WatchCommand) { $Arguments += '-WatchCommand', $WatchCommand }

            & $MonitorScript @Arguments
        }
        else {
            Write-Warning "Terminal Flow Monitor not found at: $MonitorScript"
        }
    }

    New-Alias -Name "bb-monitor" -Value "Start-TerminalMonitor" -Description "Start BusBuddy Terminal Flow Monitor" -Force

    Write-Host "`n🚀 BusBuddy PowerShell Environment Ready!" -ForegroundColor Green
    Write-Host "📋 Available workflow commands:" -ForegroundColor Cyan
    Write-Host "   bb-test      - Run Pester tests" -ForegroundColor Gray
    Write-Host "   bb-analyze   - Run code analysis" -ForegroundColor Gray
    Write-Host "   bb-docs      - Generate documentation" -ForegroundColor Gray
    Write-Host "   bb-monitor   - Start Terminal Flow Monitor" -ForegroundColor Gray
    Write-Host "   bb-ai-*      - AI integration commands" -ForegroundColor Gray
    Write-Host "`nUse 'Get-Command bb-*' for full command reference" -ForegroundColor Cyan

}
catch {
    Write-Error "Failed to load BusBuddy modules: $($_.Exception.Message)"
}
finally {
    # Restore original PSModulePath if we modified it
    if ($originalModulePath -and $env:PSModulePath -ne $originalModulePath) {
        $env:PSModulePath = $originalModulePath
        Write-Verbose "Restored original PSModulePath"
    }
}
