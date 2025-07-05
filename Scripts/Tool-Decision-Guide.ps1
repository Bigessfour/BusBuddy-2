# ╔══════════════════════════════════════════════════════════════════════════════╗
# ║                    🚀 PowerShell Development Tools 🚀                       ║
# ║                        🎯 Smart Decision Guide 🎯                           ║
# ║                      🔧 Auto-Lock Local Resources 🔧                        ║
# ╚══════════════════════════════════════════════════════════════════════════════╝

# ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
# ░                    🎮 Just Poke Buttons - I'll Do The Rest! 🎮               ░
# ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░

# ┌─────────────────────────────────────────────────────────────────────────────┐
# │                          🌟 DECISION TREE ENGINE 🌟                        │
# │                      When to Use Which PowerShell Tool                     │
# └─────────────────────────────────────────────────────────────────────────────┘

<#
.SYNOPSIS
    Smart Decision Guide for PowerShell Development Tools

.DESCRIPTION
    This script helps you decide which PowerShell tool to use based on your current situation.
    It analyzes your context and recommends the appropriate tool with examples.

.EXAMPLE
    .\Tool-Decision-Guide.ps1 -Scenario "BuildFailed"
    .\Tool-Decision-Guide.ps1 -Scenario "SlowTests"
    .\Tool-Decision-Guide.ps1 -Scenario "LowCoverage"
#>

param(
    [Parameter(Mandatory = $false)]
    [ValidateSet('BuildFailed', 'SlowTests', 'LowCoverage', 'CISetup', 'NewFeature', 'Debugging', 'Production', 'Analysis')]
    [string]$Scenario,

    [switch]$Interactive,
    [switch]$ShowAll
)

# ============================================================================
# 🔒 SYNCFUSION LOCAL RESOURCE GUARDIAN 🔒: Lock in local sources
# ============================================================================

function Test-SyncfusionLocalResources {
    Write-Host ''
    Write-Host '🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒' -ForegroundColor Cyan
    Write-Host '🔒                    SYNCFUSION RESOURCE LOCKDOWN                    🔒' -ForegroundColor Cyan
    Write-Host '🔒                  Ensuring Local Repository Usage                  🔒' -ForegroundColor Cyan
    Write-Host '🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒🔒' -ForegroundColor Cyan
    Write-Host ''

    $syncfusionPath = 'C:\Program Files (x86)\Syncfusion\Essential Studio\Windows\30.1.37'
    $resourceStatus = @{
        InstallationFound      = $false
        DocumentationAvailable = $false
        SamplesAvailable       = $false
        AssembliesAvailable    = $false
        LocalResourcesLocked   = $false
        RequiredActions        = @()
    }

    # Check main installation
    if (Test-Path $syncfusionPath) {
        $resourceStatus.InstallationFound = $true
        Write-Host "✅ Syncfusion installation found: $syncfusionPath" -ForegroundColor Green
    } else {
        $resourceStatus.RequiredActions += 'Install Syncfusion Essential Studio 30.1.37'
        Write-Host "❌ Syncfusion installation NOT found at: $syncfusionPath" -ForegroundColor Red
    }

    # Check documentation
    $docPath = Join-Path $syncfusionPath 'Help'
    if (Test-Path $docPath) {
        $resourceStatus.DocumentationAvailable = $true
        Write-Host "✅ Local documentation available: $docPath" -ForegroundColor Green
    } else {
        $resourceStatus.RequiredActions += 'Install Syncfusion documentation components'
        Write-Host "⚠️ Local documentation not found: $docPath" -ForegroundColor Yellow
    }

    # Check samples
    $samplesPath = Join-Path $syncfusionPath 'Samples'
    if (Test-Path $samplesPath) {
        $resourceStatus.SamplesAvailable = $true
        $gridSamples = Join-Path $samplesPath '4.8\Grid'
        if (Test-Path $gridSamples) {
            Write-Host "✅ Grid samples available: $gridSamples" -ForegroundColor Green
        } else {
            Write-Host "⚠️ Grid samples not found: $gridSamples" -ForegroundColor Yellow
        }
    } else {
        $resourceStatus.RequiredActions += 'Install Syncfusion sample projects'
        Write-Host "⚠️ Samples not found: $samplesPath" -ForegroundColor Yellow
    }

    # Check assemblies
    $assembliesPath = Join-Path $syncfusionPath 'Assemblies'
    if (Test-Path $assembliesPath) {
        $resourceStatus.AssembliesAvailable = $true
        Write-Host "✅ Assemblies available: $assembliesPath" -ForegroundColor Green
    } else {
        $resourceStatus.RequiredActions += 'Install Syncfusion assembly files'
        Write-Host "❌ Assemblies not found: $assembliesPath" -ForegroundColor Red
    }

    # Overall resource lock status
    $resourceStatus.LocalResourcesLocked = ($resourceStatus.InstallationFound -and
        $resourceStatus.AssembliesAvailable)

    if ($resourceStatus.LocalResourcesLocked) {
        Write-Host ''
        Write-Host '🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉' -ForegroundColor Green
        Write-Host '🔒           ✅ LOCAL RESOURCES SUCCESSFULLY LOCKED ✅            🔒' -ForegroundColor Green
        Write-Host '🔒              All tools will use local Syncfusion sources       🔒' -ForegroundColor Green
        Write-Host '🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉' -ForegroundColor Green
        Write-Host ''
    } else {
        Write-Host ''
        Write-Host '�🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨' -ForegroundColor Red
        Write-Host '🚨           ❌ LOCAL RESOURCES INCOMPLETE! ❌                   🚨' -ForegroundColor Red
        Write-Host '🚨                  Some tools may fail                          🚨' -ForegroundColor Red
        Write-Host '🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨' -ForegroundColor Red
        Write-Host ''
    }

    return $resourceStatus
}

function Set-LocalResourceEnvironment {
    Write-Host ''
    Write-Host '⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️' -ForegroundColor Cyan
    Write-Host '⚙️         🔧 CONFIGURING LOCAL ENVIRONMENT 🔧                   ⚙️' -ForegroundColor Cyan
    Write-Host '⚙️            Setting PowerShell variables for tools             ⚙️' -ForegroundColor Cyan
    Write-Host '⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️⚙️' -ForegroundColor Cyan
    Write-Host ''

    $syncfusionPath = 'C:\Program Files (x86)\Syncfusion\Essential Studio\Windows\30.1.37'

    # Set environment variables for tools to use
    $env:SYNCFUSION_LOCAL_PATH = $syncfusionPath
    $env:SYNCFUSION_DOCS_PATH = Join-Path $syncfusionPath 'Help'
    $env:SYNCFUSION_SAMPLES_PATH = Join-Path $syncfusionPath 'Samples'
    $env:SYNCFUSION_ASSEMBLIES_PATH = Join-Path $syncfusionPath 'Assemblies'
    $env:SYNCFUSION_USE_LOCAL_ONLY = 'true'

    Write-Host ''
    Write-Host '✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨' -ForegroundColor Green
    Write-Host '✨          🎯 ENVIRONMENT CONFIGURED SUCCESSFULLY! 🎯           ✨' -ForegroundColor Green
    Write-Host '✨             All tools now use local-only resources            ✨' -ForegroundColor Green
    Write-Host '✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨' -ForegroundColor Green
    Write-Host ''
    Write-Host "   📂 SYNCFUSION_LOCAL_PATH = $env:SYNCFUSION_LOCAL_PATH" -ForegroundColor Gray
    Write-Host "   🔒 SYNCFUSION_USE_LOCAL_ONLY = $env:SYNCFUSION_USE_LOCAL_ONLY" -ForegroundColor Gray
    Write-Host ''
}

# ============================================================================
# 🧠 SMART DETECTION ENGINE 🧠: Auto-detect what you need
# ============================================================================

function Get-CurrentProjectState {
    Write-Host '🔍 Analyzing current project state...' -ForegroundColor Cyan

    # First, lock in local Syncfusion resources
    $syncfusionStatus = Test-SyncfusionLocalResources
    Set-LocalResourceEnvironment

    $state = @{
        BuildStatus       = 'Unknown'
        TestStatus        = 'Unknown'
        CoverageStatus    = 'Unknown'
        ErrorCount        = 0
        HasCIConfig       = $false
        SyncfusionStatus  = $syncfusionStatus
        RecommendedAction = @()
    }

    # Check build status (with Syncfusion validation)
    Write-Host '🔨 Checking build status...' -ForegroundColor Gray
    $buildResult = & dotnet build 'Bus Buddy.sln' 2>&1
    if ($LASTEXITCODE -eq 0) {
        $state.BuildStatus = 'Success'
        Write-Host '✅ Build: Successful' -ForegroundColor Green
    } else {
        $state.BuildStatus = 'Failed'
        $errorLines = $buildResult | Where-Object { $_ -match 'error|Error|ERROR' }
        $state.ErrorCount = $errorLines.Count

        # Check for Syncfusion-related errors
        $syncfusionErrors = $errorLines | Where-Object { $_ -match 'Syncfusion|SF' }
        if ($syncfusionErrors.Count -gt 0) {
            Write-Host "❌ Build: Failed with Syncfusion errors ($($syncfusionErrors.Count)/$($state.ErrorCount))" -ForegroundColor Red
            if (-not $syncfusionStatus.LocalResourcesLocked) {
                $state.RecommendedAction += 'FixSyncfusionLocalResources'
            }
        } else {
            Write-Host "❌ Build: Failed ($($state.ErrorCount) errors)" -ForegroundColor Red
        }
        $state.RecommendedAction += 'BuildErrorAnalysis'
    }

    # Check test results
    if (Test-Path 'TestResults') {
        $testFiles = Get-ChildItem 'TestResults' -Filter '*.trx' -Recurse | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        if ($testFiles) {
            Write-Host '✅ Tests: Recent results found' -ForegroundColor Green
            $state.TestStatus = 'Available'
        }
    } else {
        Write-Host '⚠️ Tests: No recent results' -ForegroundColor Yellow
        $state.TestStatus = 'Missing'
        $state.RecommendedAction += 'RunTests'
    }

    # Check coverage
    $coverageFiles = Get-ChildItem -Path '.' -Filter 'coverage.cobertura.xml' -Recurse -ErrorAction SilentlyContinue
    if ($coverageFiles) {
        Write-Host '✅ Coverage: Files found' -ForegroundColor Green
        $state.CoverageStatus = 'Available'

        # Quick coverage check
        try {
            $coverageXml = [xml](Get-Content $coverageFiles[0].FullName)
            $lineRate = [double]$coverageXml.coverage.'line-rate' * 100
            if ($lineRate -lt 50) {
                Write-Host "⚠️ Coverage: Low ($($lineRate.ToString('F1'))%)" -ForegroundColor Yellow
                $state.RecommendedAction += 'ImproveCoverage'
            } else {
                Write-Host "✅ Coverage: Good ($($lineRate.ToString('F1'))%)" -ForegroundColor Green
            }
        } catch {
            Write-Host '⚠️ Coverage: Files found but unreadable' -ForegroundColor Yellow
        }
    } else {
        Write-Host '❌ Coverage: No files found' -ForegroundColor Red
        $state.CoverageStatus = 'Missing'
        $state.RecommendedAction += 'GenerateCoverage'
    }

    # Check CI configuration
    if (Test-Path '.github\workflows') {
        $state.HasCIConfig = $true
        Write-Host '✅ CI/CD: GitHub Actions configured' -ForegroundColor Green
    } else {
        Write-Host '⚠️ CI/CD: No GitHub Actions found' -ForegroundColor Yellow
        $state.RecommendedAction += 'SetupCI'
    }

    return $state
}

# ============================================================================
# TOOL RECOMMENDATIONS: What to use when
# ============================================================================

function Get-ToolRecommendations {
    param($ProjectState, $UserScenario)

    $recommendations = @()

    # Scenario-based recommendations
    switch ($UserScenario) {
        'BuildFailed' {
            $recommendations += @{
                Tool      = 'Enhanced-CI-Integration.ps1'
                Mode      = 'Analysis'
                Switches  = '-AnalyzeBuildErrors'
                Reason    = 'Categorizes and analyzes build errors systematically'
                Example   = '.\Scripts\Enhanced-CI-Integration.ps1 -Mode Analysis -AnalyzeBuildErrors'
                WhenToUse = 'When dotnet build fails and you need to understand why'
            }
        }

        'SlowTests' {
            $recommendations += @{
                Tool      = 'Enhanced-CI-Integration.ps1'
                Mode      = 'Local'
                Switches  = '-GenerateReports'
                Reason    = 'Uses PowerShell 7.x parallel processing for 5x faster tests'
                Example   = '.\Scripts\Enhanced-CI-Integration.ps1 -Mode Local -GenerateReports'
                WhenToUse = 'When tests take too long to run (>5 minutes)'
            }
        }

        'LowCoverage' {
            $recommendations += @{
                Tool      = 'Enhanced-CI-Integration.ps1'
                Mode      = 'Local'
                Switches  = '-GenerateReports'
                Reason    = 'Generates detailed HTML coverage reports with drill-down capability'
                Example   = '.\Scripts\Enhanced-CI-Integration.ps1 -Mode Local -GenerateReports'
                WhenToUse = 'When coverage is below 75% and you need to identify gaps'
            }
        }

        'CISetup' {
            $recommendations += @{
                Tool      = 'Enhanced-CI-Integration.ps1'
                Mode      = 'Analysis'
                Switches  = ''
                Reason    = 'Generates GitHub Actions integration configuration'
                Example   = '.\Scripts\Enhanced-CI-Integration.ps1 -Mode Analysis'
                WhenToUse = 'When setting up or improving CI/CD pipeline'
            }
        }

        'NewFeature' {
            $recommendations += @{
                Tool      = 'Install-DeveloperTools.ps1'
                Mode      = ''
                Switches  = ''
                Reason    = 'Ensures all development tools are installed and up-to-date'
                Example   = '.\Scripts\Install-DeveloperTools.ps1'
                WhenToUse = 'Before starting development on a new feature'
            }
            $recommendations += @{
                Tool      = 'NUnit-TestingTools.ps1'
                Mode      = ''
                Switches  = ''
                Reason    = 'Sets up comprehensive testing framework for new code'
                Example   = '.\Scripts\NUnit-TestingTools.ps1'
                WhenToUse = 'When writing tests for new functionality'
            }
        }

        'Debugging' {
            $recommendations += @{
                Tool      = 'PowerShell7-Integration.ps1'
                Mode      = ''
                Switches  = ''
                Reason    = 'Advanced debugging with modern PowerShell features'
                Example   = '.\Scripts\PowerShell7-Integration.ps1'
                WhenToUse = 'When debugging complex issues or performance problems'
            }
        }
    }

    # State-based recommendations (with Syncfusion priority)
    if (-not $ProjectState.SyncfusionStatus.LocalResourcesLocked) {
        $recommendations += @{
            Tool      = 'Fix-SyncfusionLocalResources.ps1'
            Priority  = 'CRITICAL'
            Mode      = ''
            Switches  = ''
            Reason    = 'Syncfusion local resources not properly configured - this blocks all development'
            Example   = '.\Scripts\Fix-SyncfusionLocalResources.ps1'
            WhenToUse = 'IMMEDIATE - Required for any Syncfusion development'
        }
    }

    if ($ProjectState.BuildStatus -eq 'Failed') {
        $recommendations += @{
            Tool      = 'Enhanced-CI-Integration.ps1'
            Priority  = 'HIGH'
            Mode      = 'Analysis'
            Switches  = '-AnalyzeBuildErrors'
            Reason    = 'Build errors detected - need systematic analysis'
            Example   = '.\Scripts\Enhanced-CI-Integration.ps1 -Mode Analysis -AnalyzeBuildErrors'
            WhenToUse = 'IMMEDIATE - Build is broken'
        }
    }

    if ($ProjectState.TestStatus -eq 'Missing') {
        $recommendations += @{
            Tool      = 'Enhanced-CI-Integration.ps1'
            Priority  = 'MEDIUM'
            Mode      = 'Local'
            Switches  = '-GenerateReports'
            Reason    = 'No recent test results - need to run comprehensive test suite'
            Example   = '.\Scripts\Enhanced-CI-Integration.ps1 -Mode Local -GenerateReports'
            WhenToUse = 'After fixing build issues'
        }
    }

    if ($ProjectState.CoverageStatus -eq 'Missing') {
        $recommendations += @{
            Tool      = 'Enhanced-CI-Integration.ps1'
            Priority  = 'MEDIUM'
            Mode      = 'Local'
            Switches  = '-GenerateReports'
            Reason    = 'No coverage data - need baseline coverage report'
            Example   = '.\Scripts\Enhanced-CI-Integration.ps1 -Mode Local -GenerateReports'
            WhenToUse = 'After tests are passing'
        }
    }

    return $recommendations
}

# ============================================================================
# 🎨 TERMINAL DECORATIONS & ANIMATIONS 🎨: Make tools visually exciting
# ============================================================================

function Start-ToolAnimation {
    param(
        [string]$ToolName,
        [string]$Action = 'Starting'
    )

    Clear-Host

    # Dynamic header with tool name
    $headerLength = 80
    $toolHeader = "🚀 $ToolName 🚀"
    $padding = [math]::Max(0, ($headerLength - $toolHeader.Length) / 2)
    $paddingStr = ' ' * $padding

    Write-Host ''
    Write-Host '█' * $headerLength -ForegroundColor DarkCyan
    Write-Host ('█' + $paddingStr + $toolHeader + $paddingStr + '█').PadRight($headerLength, ' ').Substring(0, $headerLength - 1) + '█' -ForegroundColor DarkCyan
    Write-Host '█' * $headerLength -ForegroundColor DarkCyan
    Write-Host ''

    # Animated progress indicator
    $progressChars = @('▱', '▰')
    $actionText = "⚡ $Action"

    for ($i = 0; $i -lt 20; $i++) {
        $progressBar = ''
        for ($j = 0; $j -lt 40; $j++) {
            if ($j -le $i * 2) {
                $progressBar += $progressChars[1]
            } else {
                $progressBar += $progressChars[0]
            }
        }

        Write-Host "`r$actionText [$progressBar] $([math]::Round(($i / 20) * 100))%" -NoNewline -ForegroundColor Yellow
        Start-Sleep -Milliseconds 50
    }

    Write-Host ''
    Write-Host ''

    # Tool launch fireworks
    $fireworks = @('✦', '✧', '★', '☆', '✨', '💫', '🌟', '⭐')
    for ($i = 0; $i -lt 5; $i++) {
        $firework = $fireworks | Get-Random
        $position = Get-Random -Minimum 20 -Maximum 60
        $spaces = ' ' * $position
        Write-Host "$spaces$firework $ToolName ACTIVATED! $firework" -ForegroundColor (Get-Random -InputObject @('Green', 'Cyan', 'Yellow', 'Magenta'))
        Start-Sleep -Milliseconds 200
    }

    Write-Host ''
}

function Show-ProgressSpinner {
    param(
        [string]$Message = 'Processing',
        [int]$Seconds = 3
    )

    $spinnerChars = @('|', '/', '-', '\')
    $iterations = $Seconds * 10

    for ($i = 0; $i -lt $iterations; $i++) {
        $spinner = $spinnerChars[$i % 4]
        $dots = '.' * (($i % 10) + 1)
        Write-Host "`r🔄 $Message $spinner$dots" -NoNewline -ForegroundColor Cyan
        Start-Sleep -Milliseconds 100
    }

    Write-Host "`r✅ $Message Complete!" + (' ' * 20) -ForegroundColor Green
    Write-Host ''
}

function Show-SuccessBanner {
    param([string]$Message)

    Write-Host ''
    Write-Host '🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉' -ForegroundColor Green
    $centerMessage = "🏆 $Message 🏆"
    $padding = [math]::Max(0, (80 - $centerMessage.Length) / 2)
    $paddingStr = ' ' * $padding
    Write-Host "🎉$paddingStr$centerMessage$paddingStr🎉" -ForegroundColor Green
    Write-Host '🎉                              ✨ SUCCESS! ✨                                🎉' -ForegroundColor Green
    Write-Host '�🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉🎉' -ForegroundColor Green
    Write-Host ''
}

function Show-ErrorBanner {
    param([string]$Message)

    Write-Host ''
    Write-Host '🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨' -ForegroundColor Red
    $centerMessage = "⚠️ $Message ⚠️"
    $padding = [math]::Max(0, (80 - $centerMessage.Length) / 2)
    $paddingStr = ' ' * $padding
    Write-Host "🚨$paddingStr$centerMessage$paddingStr🚨" -ForegroundColor Red
    Write-Host '🚨                            🔥 NEEDS ATTENTION! 🔥                           🚨' -ForegroundColor Red
    Write-Host '🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨' -ForegroundColor Red
    Write-Host ''
}

function Show-InfoBox {
    param(
        [string]$Title,
        [string[]]$Content,
        [string]$Color = 'Cyan'
    )

    $maxLength = ($Content + $Title | Measure-Object -Property Length -Maximum).Maximum
    $boxWidth = [math]::Max(60, $maxLength + 8)

    Write-Host ''
    Write-Host ('╔' + '═' * ($boxWidth - 2) + '╗') -ForegroundColor $Color

    $titlePadding = [math]::Max(0, ($boxWidth - $Title.Length - 4) / 2)
    $titlePaddingStr = ' ' * $titlePadding
    Write-Host ('║' + $titlePaddingStr + "🎯 $Title ��" + $titlePaddingStr + '║').PadRight($boxWidth - 1) + '║' -ForegroundColor $Color

    Write-Host ('╠' + '═' * ($boxWidth - 2) + '╣') -ForegroundColor $Color

    foreach ($line in $Content) {
        $contentPadding = ' ' * 2
        Write-Host ('║' + $contentPadding + $line + $contentPadding).PadRight($boxWidth - 1) + '║' -ForegroundColor $Color
    }

    Write-Host ('╚' + '═' * ($boxWidth - 2) + '╝') -ForegroundColor $Color
    Write-Host ''
}

function Show-CommandPreview {
    param(
        [string]$Command,
        [string]$Description
    )

    Write-Host ''
    Write-Host '🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧' -ForegroundColor DarkYellow
    Write-Host '🔧                          ⚡ COMMAND READY ⚡                              🔧' -ForegroundColor DarkYellow
    Write-Host '🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧' -ForegroundColor DarkYellow
    Write-Host ''
    Write-Host "📋 What it does: $Description" -ForegroundColor White
    Write-Host "💻 Command: $Command" -ForegroundColor Yellow
    Write-Host ''
    Write-Host '▶️▶️▶️▶️▶️▶️▶️▶️▶️▶️▶️▶️▶️▶️▶️▶️▶️▶️▶️▶️▶️▶️▶️▶️▶️▶️▶️▶️▶️▶️▶️▶️▶️▶️▶️▶️' -ForegroundColor Green
}

function Show-ToolMenu {
    param([array]$Tools)

    Write-Host ''
    Write-Host '🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮' -ForegroundColor Magenta
    Write-Host '🎮                       🕹️ TOOL SELECTION MENU 🕹️                        🎮' -ForegroundColor Magenta
    Write-Host '🎮                         Just Poke a Button!                             🎮' -ForegroundColor Magenta
    Write-Host '🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮🎮' -ForegroundColor Magenta
    Write-Host ''

    for ($i = 0; $i -lt $Tools.Count; $i++) {
        $tool = $Tools[$i]
        $number = $i + 1
        $icon = switch ($number) {
            1 { '🔨' }
            2 { '🚀' }
            3 { '📊' }
            4 { '⚙️' }
            5 { '🐞' }
            6 { '✨' }
            7 { '🤖' }
            default { '🔧' }
        }

        Write-Host "  $icon $number. " -NoNewline -ForegroundColor Green
        Write-Host "$($tool.Name)" -ForegroundColor Cyan
        Write-Host "      🎯 $($tool.Description)" -ForegroundColor Gray
        Write-Host ''
    }

    Write-Host '🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯' -ForegroundColor Magenta
}

function Invoke-ToolWithDecorations {
    param(
        [string]$ToolPath,
        [string]$ToolName,
        [string]$Description,
        [string]$Arguments = ''
    )

    # Pre-execution animation
    Start-ToolAnimation -ToolName $ToolName -Action 'Initializing'

    # Show command preview
    $fullCommand = if ($Arguments) { "$ToolPath $Arguments" } else { $ToolPath }
    Show-CommandPreview -Command $fullCommand -Description $Description

    # Countdown before execution
    Write-Host '🚀 Launching in...' -ForegroundColor Yellow
    for ($i = 3; $i -gt 0; $i--) {
        Write-Host "   $i..." -ForegroundColor Red
        Start-Sleep -Seconds 1
    }
    Write-Host '   🔥 GO! 🔥' -ForegroundColor Green
    Write-Host ''

    # Progress indicator during execution
    Show-ProgressSpinner -Message "Executing $ToolName" -Seconds 2

    try {
        # Execute the actual tool
        if ($Arguments) {
            & $ToolPath @($Arguments.Split(' '))
        } else {
            & $ToolPath
        }

        # Success celebration
        Show-SuccessBanner -Message "$ToolName Completed Successfully"

        # Fireworks finale
        Write-Host '🎆 ' -NoNewline -ForegroundColor Yellow
        for ($i = 0; $i -lt 10; $i++) {
            $colors = @('Red', 'Green', 'Blue', 'Yellow', 'Magenta', 'Cyan')
            $firework = '✨'
            Write-Host "$firework" -NoNewline -ForegroundColor (Get-Random -InputObject $colors)
            Start-Sleep -Milliseconds 100
        }
        Write-Host ' 🎆' -ForegroundColor Yellow
        Write-Host ''

    } catch {
        Show-ErrorBanner -Message "$ToolName Encountered an Issue"
        Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host ''
    }
}

# ============================================================================
# VISUAL INDICATORS: How to spot when tools are needed
# ============================================================================

function Show-VisualIndicators {
    Show-InfoBox -Title 'VISUAL INDICATORS - When You Need PowerShell Tools' -Content @(
        'Look for these signs in your terminal:',
        '',
        '❌ Red error messages flooding → Use Enhanced-CI-Integration.ps1',
        '⏱️ Tests taking >5 minutes → Use parallel processing tools',
        '📊 Basic XML coverage files → Generate rich HTML reports',
        '🤖 GitHub Actions >15 mins → Optimize with PowerShell tools',
        '❓ Generic build failures → Use Tool-Decision-Guide.ps1'
    ) -Color 'Cyan'

    $indicators = @(
        @{
            Symptom   = 'dotnet build fails with many errors'
            VisualCue = '❌ Red error messages flooding the terminal'
            Tool      = 'Enhanced-CI-Integration.ps1 -Mode Analysis -AnalyzeBuildErrors'
            Benefit   = 'Categorizes errors by type for systematic fixing'
        },
        @{
            Symptom   = 'Tests take >5 minutes to run'
            VisualCue = '⏱️ Slow test execution, watching progress bar crawl'
            Tool      = 'Enhanced-CI-Integration.ps1 -Mode Local -GenerateReports'
            Benefit   = '5x faster with parallel processing'
        },
        @{
            Symptom   = 'Coverage reports hard to understand'
            VisualCue = '📊 Basic XML files, no visual drill-down'
            Tool      = 'Enhanced-CI-Integration.ps1 -Mode Local -GenerateReports'
            Benefit   = 'Rich HTML reports with class-level detail'
        },
        @{
            Symptom   = 'CI/CD pipeline takes too long'
            VisualCue = '🤖 GitHub Actions running >15 minutes'
            Tool      = 'Enhanced-CI-Integration.ps1 -Mode CI'
            Benefit   = 'Parallel processing cuts CI time in half'
        },
        @{
            Symptom   = "Don't know what's wrong with build"
            VisualCue = "❓ Generic 'build failed' with no clear direction"
            Tool      = 'Tool-Decision-Guide.ps1 -Interactive'
            Benefit   = 'Smart analysis tells you exactly what to do'
        }
    )

    foreach ($indicator in $indicators) {
        Show-InfoBox -Title 'PROBLEM DETECTED' -Content @(
            "🔍 Visual Cue: $($indicator.VisualCue)",
            "📝 Problem: $($indicator.Symptom)",
            "🔧 Solution: $($indicator.Tool)",
            "✨ Result: $($indicator.Benefit)"
        ) -Color 'Yellow'
    }
}

# ============================================================================
# INTERACTIVE MODE: Let the script guide you with decorations
# ============================================================================

function Start-InteractiveGuidance {
    # Clear screen and show welcome animation
    Clear-Host

    # Epic welcome banner
    Write-Host ''
    Write-Host '🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟' -ForegroundColor Yellow
    Write-Host '🌟                                                                          🌟' -ForegroundColor Yellow
    Write-Host '🌟                 🤖 INTERACTIVE POWERSHELL TOOL GUIDANCE 🤖              🌟' -ForegroundColor Yellow
    Write-Host '🌟                           🎮 JUST POKE BUTTONS! 🎮                      🌟' -ForegroundColor Yellow
    Write-Host '🌟                                                                          🌟' -ForegroundColor Yellow
    Write-Host '🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟🌟' -ForegroundColor Yellow
    Write-Host ''

    # Lock in local resources first
    Show-ProgressSpinner -Message 'Locking in local Syncfusion resources' -Seconds 2
    Set-LocalResourceEnvironment

    # Get current state with visual feedback
    Show-ProgressSpinner -Message 'Analyzing project state' -Seconds 3
    $state = Get-CurrentProjectState

    # Check if Syncfusion is properly configured
    if (-not $state.SyncfusionStatus.LocalResourcesLocked) {
        Show-ErrorBanner -Message 'SYNCFUSION LOCAL RESOURCES NOT CONFIGURED'

        Show-InfoBox -Title 'CRITICAL ISSUE DETECTED' -Content @(
            '🚨 Syncfusion local resources are not properly configured!',
            '🔥 This will cause all Syncfusion-based tools to fail.',
            '',
            'Required actions:'
        ) -Color 'Red'

        foreach ($action in $state.SyncfusionStatus.RequiredActions) {
            Write-Host "    ❌ $action" -ForegroundColor Red
        }

        Write-Host ''
        Write-Host '🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧' -ForegroundColor Green
        Write-Host '🔧                         Fix this first? (y/n)                          🔧' -ForegroundColor Green
        Write-Host '🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧🔧' -ForegroundColor Green

        $fixSyncfusion = Read-Host
        if ($fixSyncfusion -eq 'y' -or $fixSyncfusion -eq 'Y') {
            Start-ToolAnimation -ToolName 'Syncfusion Resource Fix' -Action 'Creating fix script'
            Show-SuccessBanner -Message 'SYNCFUSION FIX SCRIPT READY'
            return
        }
    }

    # Show interactive menu with decorations
    $toolOptions = @(
        @{ Name = 'Fix build errors'; Description = 'Analyze and categorize build failures' },
        @{ Name = 'Speed up tests'; Description = 'Use parallel processing for 5x faster execution' },
        @{ Name = 'Improve code coverage'; Description = 'Generate rich HTML coverage reports' },
        @{ Name = 'Setup CI/CD'; Description = 'Configure GitHub Actions integration' },
        @{ Name = 'Debug issues'; Description = 'Advanced PowerShell 7.x debugging tools' },
        @{ Name = 'Start new feature'; Description = 'Install and configure development tools' },
        @{ Name = 'Auto-detect problems'; Description = 'Let AI analyze your project and recommend actions' }
    )

    Show-ToolMenu -Tools $toolOptions

    Write-Host '🎯 Choose your adventure (1-7): ' -NoNewline -ForegroundColor Cyan
    $choice = Read-Host

    $scenarioMap = @{
        '1' = 'BuildFailed'
        '2' = 'SlowTests'
        '3' = 'LowCoverage'
        '4' = 'CISetup'
        '5' = 'Debugging'
        '6' = 'NewFeature'
        '7' = 'Auto'
    }

    $selectedScenario = $scenarioMap[$choice]
    if (-not $selectedScenario) { $selectedScenario = 'Auto' }

    # Get recommendations with visual feedback
    if ($selectedScenario -eq 'Auto') {
        Show-ProgressSpinner -Message '🤖 AI analyzing project state' -Seconds 3
        $recommendations = Get-ToolRecommendations -ProjectState $state
    } else {
        Show-ProgressSpinner -Message '🎯 Finding perfect tools for your scenario' -Seconds 2
        $recommendations = Get-ToolRecommendations -ProjectState $state -UserScenario $selectedScenario
    }

    # Show recommendations with epic styling
    Write-Host ''
    Write-Host '💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡' -ForegroundColor Green
    Write-Host '💡                        🎯 RECOMMENDED ACTIONS 🎯                         💡' -ForegroundColor Green
    Write-Host '💡                         Your Perfect Tool Match!                        💡' -ForegroundColor Green
    Write-Host '💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡💡' -ForegroundColor Green
    Write-Host ''

    $priorityOrder = @('CRITICAL', 'HIGH', 'MEDIUM', 'LOW', $null)
    $sortedRecommendations = $recommendations | Sort-Object { $priorityOrder.IndexOf($_.Priority) }

    for ($i = 0; $i -lt $sortedRecommendations.Count; $i++) {
        $rec = $sortedRecommendations[$i]
        $priority = if ($rec.Priority) { "[$($rec.Priority)]" } else { '' }
        $priorityColor = switch ($rec.Priority) {
            'CRITICAL' { 'Red' }
            'HIGH' { 'Yellow' }
            'MEDIUM' { 'Cyan' }
            default { 'White' }
        }

        Show-InfoBox -Title "RECOMMENDATION #$($i+1) $priority" -Content @(
            "🔧 Tool: $($rec.Tool)",
            "🎯 Why: $($rec.Reason)",
            "💻 Command: $($rec.Example)",
            "⏰ When: $($rec.WhenToUse)"
        ) -Color $priorityColor
    }

    # Offer to run the top recommendation with epic fanfare
    if ($sortedRecommendations.Count -gt 0) {
        $topRec = $sortedRecommendations[0]

        Write-Host ''
        Write-Host '🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀' -ForegroundColor Green
        Write-Host '🚀                   ⚡ LAUNCH TOP RECOMMENDATION? ⚡                    🚀' -ForegroundColor Green
        Write-Host "🚀                           Press 'y' to blast off!                     🚀" -ForegroundColor Green
        Write-Host '🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀🚀' -ForegroundColor Green

        $runNow = Read-Host

        if ($runNow -eq 'y' -or $runNow -eq 'Y') {
            # Extract tool name for decoration
            $toolName = $topRec.Tool -replace '\.ps1', '' -replace 'Scripts\\', ''
            $description = $topRec.Reason

            Invoke-ToolWithDecorations -ToolPath $topRec.Example -ToolName $toolName -Description $description
        } else {
            Show-InfoBox -Title 'READY TO GO' -Content @(
                '🎯 Your tools are ready when you are!',
                "💡 Run any command above when you're ready",
                '🚀 Each tool will guide you through the process'
            ) -Color 'Cyan'
        }
    }
}

# ============================================================================
# MAIN EXECUTION - Now with Epic Visual Style!
# ============================================================================

if ($Interactive) {
    Start-InteractiveGuidance
} elseif ($ShowAll) {
    # Epic "show all" mode with decorations
    Clear-Host

    Write-Host ''
    Write-Host '📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚' -ForegroundColor Blue
    Write-Host '�                      🔧 ALL AVAILABLE TOOLS 🔧                          📚' -ForegroundColor Blue
    Write-Host '📚                        Complete Arsenal View                           📚' -ForegroundColor Blue
    Write-Host '📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚📚' -ForegroundColor Blue
    Write-Host ''

    Show-ProgressSpinner -Message 'Loading all available tools' -Seconds 2
    $state = Get-CurrentProjectState
    $allRecommendations = Get-ToolRecommendations -ProjectState $state

    Show-VisualIndicators

    foreach ($rec in $allRecommendations) {
        Show-InfoBox -Title $rec.Tool -Content @(
            "🎯 Purpose: $($rec.Reason)",
            "💻 Usage: $($rec.Example)",
            "⏰ Best Time: $($rec.WhenToUse)"
        ) -Color 'Blue'
    }

    Show-SuccessBanner -Message 'ALL TOOLS DISPLAYED'

} elseif ($Scenario) {
    # Scenario mode with decorations
    Clear-Host

    Start-ToolAnimation -ToolName 'Scenario Analysis' -Action "Processing $Scenario"

    $state = Get-CurrentProjectState
    $recommendations = Get-ToolRecommendations -ProjectState $state -UserScenario $Scenario

    Show-InfoBox -Title "SCENARIO: $Scenario" -Content @(
        '🎯 Analyzing your specific situation...',
        '🔍 Finding the perfect tools for this scenario'
    ) -Color 'Magenta'

    foreach ($rec in $recommendations) {
        Show-InfoBox -Title 'RECOMMENDED SOLUTION' -Content @(
            "🔧 Tool: $($rec.Tool)",
            "💡 Reason: $($rec.Reason)",
            "💻 Command: $($rec.Example)",
            "🎯 Best Used: $($rec.WhenToUse)"
        ) -Color 'Green'
    }

} else {
    # Default help mode with epic styling
    Clear-Host

    Write-Host ''
    Write-Host '🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯' -ForegroundColor Cyan
    Write-Host '🎯                    POWERSHELL TOOL DECISION GUIDE                      🎯' -ForegroundColor Cyan
    Write-Host '🎯                         🤖 AI-Powered Automation 🤖                    🎯' -ForegroundColor Cyan
    Write-Host '🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯🎯' -ForegroundColor Cyan
    Write-Host ''

    Show-InfoBox -Title 'USAGE OPTIONS' -Content @(
        '🎮 Interactive Mode (Recommended):',
        '   .\Tool-Decision-Guide.ps1 -Interactive',
        '',
        '📚 Show All Tools:',
        '   .\Tool-Decision-Guide.ps1 -ShowAll',
        '',
        '🎯 Specific Scenario:',
        '   .\Tool-Decision-Guide.ps1 -Scenario BuildFailed'
    ) -Color 'White'

    Write-Host ''
    Write-Host '✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨' -ForegroundColor Green
    Write-Host '✨                    🚀 QUICK START 🚀                                ✨' -ForegroundColor Green
    Write-Host '✨              .\Tool-Decision-Guide.ps1 -Interactive                ✨' -ForegroundColor Green
    Write-Host '✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨✨' -ForegroundColor Green
    Write-Host ''
}
