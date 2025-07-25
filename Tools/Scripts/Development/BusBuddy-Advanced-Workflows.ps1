#Requires -Version 7.0
<#
.SYNOPSIS
    Bus Buddy Advanced Development Workflows

.DESCRIPTION
    Comprehensive development automation for Bus Buddy WPF application
    Includes UI iteration cycles, error monitoring, and development session management

.NOTES
    Enhanced for PowerShell 7.5.2 with parallel processing and streamlined workflows
    Optimized for solo development with UI beautification focus
#>

# ==== XAML HOT RELOAD INTEGRATION ====

function Start-BusBuddyHotReload {
    <#
    .SYNOPSIS
        Enable XAML hot reload workflow for UI beautification
    #>
    param(
        [switch]$AutoBuild,
        [switch]$MonitorLogs,
        [string]$WatchPath = "BusBuddy.WPF"
    )

    Write-Host '🔥 Starting Bus Buddy Hot Reload Workflow' -ForegroundColor Red
    Write-Host '=========================================' -ForegroundColor Red

    $root = Get-BusBuddyProjectRoot
    if (-not $root) {
        Write-Host '❌ Bus Buddy project not found' -ForegroundColor Red
        return
    }

    Push-Location $root

    try {
        # Start file watcher for XAML changes
        Write-Host '👀 Setting up XAML file watcher...' -ForegroundColor Cyan

        $watcher = New-Object System.IO.FileSystemWatcher
        $watcher.Path = Join-Path $root $WatchPath
        $watcher.Filter = "*.xaml"
        $watcher.IncludeSubdirectories = $true
        $watcher.EnableRaisingEvents = $true

        # Register event handler
        $action = {
            $path = $Event.SourceEventArgs.FullPath
            $changeType = $Event.SourceEventArgs.ChangeType
            $fileName = Split-Path $path -Leaf

            Write-Host "📝 XAML Change Detected: $fileName ($changeType)" -ForegroundColor Yellow

            if ($AutoBuild) {
                Write-Host '🔨 Auto-building...' -ForegroundColor Green
                Invoke-BusBuddyBuild -NoLogo -Quiet
            }
        }

        Register-ObjectEvent -InputObject $watcher -EventName "Changed" -Action $action

        Write-Host '✅ Hot reload monitoring active' -ForegroundColor Green
        Write-Host '💡 Edit XAML files and see changes detected automatically' -ForegroundColor Gray
        Write-Host '💡 Press Ctrl+C to stop monitoring' -ForegroundColor Gray

        if ($MonitorLogs) {
            Write-Host "`n📖 Starting log monitoring in background..." -ForegroundColor Cyan
            Start-Job -Name "BusBuddyLogMonitor" -ScriptBlock {
                $root = $using:root
                $logsPath = Join-Path $root "BusBuddy.WPF\logs"

                # Wait for log files to be created
                while (-not (Test-Path $logsPath)) {
                    Start-Sleep -Seconds 1
                }

                # Monitor latest application log
                $pattern = "application-*.log"
                do {
                    $logFiles = Get-ChildItem -Path $logsPath -Filter $pattern -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending
                    if ($logFiles.Count -gt 0) {
                        Get-Content $logFiles[0].FullName -Tail 0 -Wait
                    }
                    Start-Sleep -Seconds 2
                } while ($true)
            } | Out-Null
        }

        # Keep monitoring until interrupted
        try {
            while ($true) {
                Start-Sleep -Seconds 1
            }
        }
        finally {
            $watcher.EnableRaisingEvents = $false
            $watcher.Dispose()

            if ($MonitorLogs) {
                Get-Job -Name "BusBuddyLogMonitor" -ErrorAction SilentlyContinue | Stop-Job | Remove-Job
            }
        }
    }
    finally {
        Pop-Location
    }
}

# ==== DEVELOPMENT SESSION MANAGEMENT ====

function Start-BusBuddyDevSession {
    <#
    .SYNOPSIS
        Start complete Bus Buddy development session with all tools
    #>
    param(
        [switch]$SkipBuild,
        [switch]$SkipClean,
        [switch]$StartApp,
        [switch]$MonitorLogs,
        [string]$SessionName = "BusBuddy-Dev-$(Get-Date -Format 'HHmm')"
    )

    Write-Host "🚀 Starting Bus Buddy Development Session: $SessionName" -ForegroundColor Green
    Write-Host "=======================================================" -ForegroundColor Green

    # Session startup steps
    $steps = @(
        @{ Name = "Navigate to project"; Action = { Set-BusBuddyLocation } },
        @{ Name = "Clean solution"; Action = { if (-not $SkipClean) { Invoke-BusBuddyClean } } },
        @{ Name = "Build solution"; Action = { if (-not $SkipBuild) { Invoke-BusBuddyBuild -NoLogo } } },
        @{ Name = "Validate themes"; Action = { Invoke-BusBuddyThemeCheck } },
        @{ Name = "Validate UI"; Action = { Invoke-BusBuddyValidateUI } }
    )

    $stepCount = 1
    foreach ($step in $steps) {
        Write-Host "`n[$stepCount/$($steps.Count)] $($step.Name)..." -ForegroundColor Cyan
        & $step.Action
        $stepCount++
    }

    # Optional: Start application
    if ($StartApp) {
        Write-Host "`n🚌 Starting Bus Buddy application..." -ForegroundColor Yellow
        Start-Job -Name "BusBuddyApp" -ScriptBlock {
            $root = $using:root
            Set-Location $root
            dotnet run --project BusBuddy.WPF/BusBuddy.WPF.csproj --no-build
        } | Out-Null
        Write-Host "✅ Application started in background job" -ForegroundColor Green
    }

    # Optional: Start log monitoring
    if ($MonitorLogs) {
        Write-Host "`n📖 Starting log monitoring..." -ForegroundColor Cyan
        Start-Job -Name "BusBuddyLogMonitor" -ScriptBlock {
            $root = $using:root
            while ($true) {
                try {
                    Watch-BusBuddyLogs -Lines 5 -Follow
                }
                catch {
                    Start-Sleep -Seconds 5
                }
            }
        } | Out-Null
        Write-Host "✅ Log monitoring started in background job" -ForegroundColor Green
    }

    Write-Host "`n✅ Development session '$SessionName' ready!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📋 SESSION COMMANDS:" -ForegroundColor Magenta
    Write-Host "   bb-ui-cycle        - Complete UI iteration workflow" -ForegroundColor Gray
    Write-Host "   bb-build && bb-run - Quick build and test" -ForegroundColor Gray
    Write-Host "   bb-logs-tail -Follow - Monitor logs" -ForegroundColor Gray
    Write-Host "   bb-logs-errors -Follow - Monitor errors" -ForegroundColor Gray
    Write-Host ""
    Write-Host "🎯 ACTIVE BACKGROUND JOBS:" -ForegroundColor Yellow
    Get-Job | Where-Object { $_.Name -like "BusBuddy*" } | Format-Table Name, State -AutoSize
}

function Stop-BusBuddyDevSession {
    <#
    .SYNOPSIS
        Stop Bus Buddy development session and clean up background jobs
    #>
    Write-Host "🛑 Stopping Bus Buddy development session..." -ForegroundColor Red

    # Stop and remove all Bus Buddy background jobs
    $jobs = Get-Job | Where-Object { $_.Name -like "BusBuddy*" }

    if ($jobs.Count -gt 0) {
        Write-Host "🧹 Cleaning up $($jobs.Count) background jobs..." -ForegroundColor Cyan
        $jobs | Stop-Job | Remove-Job -Force
        Write-Host "✅ Background jobs cleaned up" -ForegroundColor Green
    }
    else {
        Write-Host "ℹ️ No background jobs to clean up" -ForegroundColor Gray
    }

    Write-Host "✅ Development session stopped" -ForegroundColor Green
}

# ==== QUICK TEST AUTOMATION ====

function Start-BusBuddyQuickTest {
    <#
    .SYNOPSIS
        Quick build-test-validate cycle for rapid iteration
    #>
    param(
        [switch]$SkipTests,
        [switch]$SkipTheme,
        [switch]$SkipUI,
        [int]$Iterations = 1
    )

    Write-Host "⚡ Starting Bus Buddy Quick Test Cycle" -ForegroundColor Yellow
    Write-Host "=====================================" -ForegroundColor Yellow

    for ($i = 1; $i -le $Iterations; $i++) {
        if ($Iterations -gt 1) {
            Write-Host "`n🔄 Iteration $i of $Iterations" -ForegroundColor Magenta
        }

        # Clean and build
        Write-Host "🧹 Cleaning and building..." -ForegroundColor Cyan
        $buildSuccess = Invoke-BusBuddyBuild -Clean -NoLogo -Quiet

        if (-not $buildSuccess) {
            Write-Host "❌ Build failed on iteration $i" -ForegroundColor Red
            break
        }

        # Run tests
        if (-not $SkipTests) {
            Write-Host "🧪 Running tests..." -ForegroundColor Cyan
            Invoke-BusBuddyTest -NoBuild
        }

        # Theme validation
        if (-not $SkipTheme) {
            Write-Host "🎨 Validating themes..." -ForegroundColor Cyan
            Invoke-BusBuddyThemeCheck | Out-Null
        }

        # UI validation
        if (-not $SkipUI) {
            Write-Host "🔍 Validating UI..." -ForegroundColor Cyan
            Invoke-BusBuddyValidateUI | Out-Null
        }

        Write-Host "✅ Quick test iteration $i completed" -ForegroundColor Green
    }

    Write-Host "`n🎯 Quick test cycle completed!" -ForegroundColor Green
}

# ==== COMPREHENSIVE DIAGNOSTIC ====

function Invoke-BusBuddyFullDiagnostic {
    <#
    .SYNOPSIS
        Comprehensive Bus Buddy project health and diagnostic analysis
    #>
    param(
        [switch]$ExportReport,
        [string]$ReportPath = "logs/diagnostic-report-$(Get-Date -Format 'yyyy-MM-dd_HH-mm-ss').json"
    )

    Write-Host "🔬 Starting Comprehensive Bus Buddy Diagnostic" -ForegroundColor Blue
    Write-Host "===============================================" -ForegroundColor Blue

    $diagnosticData = [PSCustomObject]@{
        Timestamp       = Get-Date
        ProjectRoot     = Get-BusBuddyProjectRoot
        Environment     = @{
            PowerShellVersion = $PSVersionTable.PSVersion
            DotNetVersion     = (dotnet --version 2>$null)
            OperatingSystem   = [System.Environment]::OSVersion
            MachineName       = [System.Environment]::MachineName
        }
        ProjectHealth   = @{}
        BuildStatus     = @{}
        ThemeStatus     = @{}
        UIStatus        = @{}
        LogAnalysis     = @{}
        Recommendations = @()
    }

    # Project structure analysis
    Write-Host "📁 Analyzing project structure..." -ForegroundColor Cyan
    $root = $diagnosticData.ProjectRoot
    if ($root) {
        $diagnosticData.ProjectHealth = @{
            SolutionFile   = Test-Path (Join-Path $root "BusBuddy.sln")
            WPFProject     = Test-Path (Join-Path $root "BusBuddy.WPF/BusBuddy.WPF.csproj")
            CoreProject    = Test-Path (Join-Path $root "BusBuddy.Core/BusBuddy.Core.csproj")
            TestsProject   = Test-Path (Join-Path $root "BusBuddy.Tests/BusBuddy.Tests.csproj")
            ToolsDirectory = Test-Path (Join-Path $root "Tools")
            LogsDirectory  = Test-Path (Join-Path $root "BusBuddy.WPF/logs")
        }
    }

    # Build test
    Write-Host "🔨 Testing build..." -ForegroundColor Cyan
    $buildStart = Get-Date
    $buildSuccess = Invoke-BusBuddyBuild -Clean -NoLogo -Quiet
    $buildDuration = (Get-Date) - $buildStart

    $diagnosticData.BuildStatus = @{
        Success      = $buildSuccess
        Duration     = $buildDuration.TotalSeconds
        LastExitCode = $LASTEXITCODE
    }

    # Theme analysis
    Write-Host "🎨 Analyzing themes..." -ForegroundColor Cyan
    $themeSuccess = Invoke-BusBuddyThemeCheck
    $diagnosticData.ThemeStatus = @{
        ValidationPassed = $themeSuccess
        ThemeScript      = Test-Path (Join-Path $root "Tools/Scripts/bb-theme-check.ps1")
    }

    # UI validation
    Write-Host "🔍 Analyzing UI..." -ForegroundColor Cyan
    $diagnosticData.UIStatus = @{
        ValidationScript = Test-Path (Join-Path $root "Tools/Scripts/Syncfusion-Implementation-Validator.ps1")
    }

    # Log analysis
    Write-Host "📖 Analyzing logs..." -ForegroundColor Cyan
    $logsPath = Join-Path $root "BusBuddy.WPF/logs"
    if (Test-Path $logsPath) {
        $logFiles = Get-ChildItem $logsPath -Filter "*.log" -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending
        $totalSize = if ($logFiles.Count -gt 0) { ($logFiles | Measure-Object Length -Sum).Sum } else { 0 }
        $diagnosticData.LogAnalysis = @{
            LogsDirectoryExists = $true
            TotalLogFiles       = $logFiles.Count
            MostRecentLog       = if ($logFiles.Count -gt 0) { $logFiles[0].Name } else { $null }
            TotalLogSize        = $totalSize
        }
    }
    else {
        $diagnosticData.LogAnalysis = @{
            LogsDirectoryExists = $false
            TotalLogFiles       = 0
            MostRecentLog       = $null
            TotalLogSize        = 0
        }
    }

    # Generate recommendations
    Write-Host "💡 Generating recommendations..." -ForegroundColor Cyan

    if (-not $buildSuccess) {
        $diagnosticData.Recommendations += "🔨 Build issues detected - run 'bb-build' to see detailed errors"
    }

    if (-not $themeSuccess) {
        $diagnosticData.Recommendations += "🎨 Theme inconsistencies found - run 'bb-theme-check' for details"
    }

    if (-not $diagnosticData.LogAnalysis.LogsDirectoryExists) {
        $diagnosticData.Recommendations += "📖 Logs directory missing - run the application to generate logs"
    }

    if ($diagnosticData.BuildStatus.Duration -gt 30) {
        $diagnosticData.Recommendations += "⚡ Slow build detected - consider using 'bb-build -NoLogo -Quiet' for faster builds"
    }

    if ($diagnosticData.Recommendations.Count -eq 0) {
        $diagnosticData.Recommendations += "✅ Project health looks excellent!"
    }

    # Display summary
    Write-Host "`n📊 DIAGNOSTIC SUMMARY" -ForegroundColor Blue
    Write-Host "====================" -ForegroundColor Blue
    Write-Host "Project Root: $($diagnosticData.ProjectRoot)" -ForegroundColor Gray
    Write-Host "Build Status: $(if($buildSuccess) { '✅ Success' } else { '❌ Failed' })" -ForegroundColor $(if ($buildSuccess) { 'Green' } else { 'Red' })
    Write-Host "Build Duration: $([math]::Round($buildDuration.TotalSeconds, 2)) seconds" -ForegroundColor Gray
    Write-Host "Theme Status: $(if($themeSuccess) { '✅ Consistent' } else { '⚠️ Issues Found' })" -ForegroundColor $(if ($themeSuccess) { 'Green' } else { 'Yellow' })
    Write-Host "Log Files: $($diagnosticData.LogAnalysis.TotalLogFiles)" -ForegroundColor Gray

    Write-Host "`n🎯 RECOMMENDATIONS:" -ForegroundColor Yellow
    $diagnosticData.Recommendations | ForEach-Object {
        Write-Host "   $_" -ForegroundColor Gray
    }

    # Export report if requested
    if ($ExportReport) {
        Write-Host "`n📄 Exporting diagnostic report..." -ForegroundColor Cyan
        $reportsDir = Split-Path $ReportPath -Parent
        if (-not (Test-Path $reportsDir)) {
            New-Item -ItemType Directory -Path $reportsDir -Force | Out-Null
        }

        $diagnosticData | ConvertTo-Json -Depth 10 | Out-File -FilePath $ReportPath -Encoding utf8
        Write-Host "✅ Report exported to: $ReportPath" -ForegroundColor Green
    }

    Write-Host "`n✅ Comprehensive diagnostic completed!" -ForegroundColor Blue
    return $diagnosticData
}

# ==== PROJECT REPORTING ====

function Export-BusBuddyProjectReport {
    <#
    .SYNOPSIS
        Generate comprehensive project report for analysis
    #>
    param(
        [string]$OutputPath = "logs/project-report-$(Get-Date -Format 'yyyy-MM-dd_HH-mm-ss').json"
    )

    Write-Host "📋 Generating Bus Buddy Project Report" -ForegroundColor Green
    Write-Host "======================================" -ForegroundColor Green

    # Get comprehensive diagnostic data
    $diagnostic = Invoke-BusBuddyFullDiagnostic -ExportReport:$false

    # Add additional project analysis
    $report = [PSCustomObject]@{
        ReportMetadata      = @{
            GeneratedAt   = Get-Date
            ReportVersion = "1.0"
            Generator     = "Bus Buddy Advanced Workflows"
        }
        Diagnostic          = $diagnostic
        ProjectFiles        = @{}
        DevelopmentWorkflow = @{}
    }

    # Project file analysis
    Write-Host "📁 Analyzing project files..." -ForegroundColor Cyan
    $root = Get-BusBuddyProjectRoot
    if ($root) {
        $report.ProjectFiles = @{
            XamlFiles         = (Get-ChildItem $root -Filter "*.xaml" -Recurse).Count
            CSharpFiles       = (Get-ChildItem $root -Filter "*.cs" -Recurse).Count
            PowerShellScripts = (Get-ChildItem $root -Filter "*.ps1" -Recurse).Count
            ResourceFiles     = (Get-ChildItem $root -Path "*/Resources/*" -Recurse -ErrorAction SilentlyContinue).Count
            ViewFiles         = (Get-ChildItem $root -Path "*/Views/*" -Filter "*.xaml" -Recurse -ErrorAction SilentlyContinue).Count
            ViewModelFiles    = (Get-ChildItem $root -Path "*/ViewModels/*" -Filter "*.cs" -Recurse -ErrorAction SilentlyContinue).Count
        }
    }

    # Development workflow recommendations
    Write-Host "🔧 Analyzing development workflow..." -ForegroundColor Cyan
    $report.DevelopmentWorkflow = @{
        RecommendedCommands = @(
            "bb-ui-cycle - Complete UI beautification workflow",
            "bb-build && bb-run - Quick test cycle",
            "bb-logs-tail -Follow - Real-time error monitoring",
            "Start-BusBuddyDevSession - Full development session"
        )
        AvailableAliases    = @(
            "bb-build", "bb-run", "bb-clean", "bb-test",
            "bb-ui-cycle", "bb-theme-check", "bb-validate-ui",
            "bb-logs-tail", "bb-logs-errors", "bb-logs-ui"
        )
        BackgroundJobs      = (Get-Job | Where-Object { $_.Name -like "BusBuddy*" }).Count
    }

    # Export report
    Write-Host "📄 Exporting project report..." -ForegroundColor Cyan
    $reportsDir = Split-Path $OutputPath -Parent
    if (-not (Test-Path $reportsDir)) {
        New-Item -ItemType Directory -Path $reportsDir -Force | Out-Null
    }

    $report | ConvertTo-Json -Depth 15 | Out-File -FilePath $OutputPath -Encoding utf8

    Write-Host "✅ Project report exported to: $OutputPath" -ForegroundColor Green
    return $report
}

# ==== ENHANCED ALIASES ====

# Advanced workflow aliases
Set-Alias -Name 'bb-dev-session' -Value 'Start-BusBuddyDevSession'
Set-Alias -Name 'bb-stop-session' -Value 'Stop-BusBuddyDevSession'
Set-Alias -Name 'bb-quick-test' -Value 'Start-BusBuddyQuickTest'
Set-Alias -Name 'bb-diagnostic' -Value 'Invoke-BusBuddyFullDiagnostic'
Set-Alias -Name 'bb-report' -Value 'Export-BusBuddyProjectReport'
Set-Alias -Name 'bb-hot-reload' -Value 'Start-BusBuddyHotReload'

# Development convenience aliases
Set-Alias -Name 'bb-health' -Value 'Invoke-BusBuddyFullDiagnostic'
Set-Alias -Name 'bb-check-all' -Value 'Start-BusBuddyQuickTest'

Write-Host "🚀 Bus Buddy Advanced Workflows Loaded!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "🔥 ADVANCED COMMANDS:" -ForegroundColor Red
Write-Host "   bb-dev-session     - Start complete development session" -ForegroundColor Gray
Write-Host "   bb-hot-reload      - Enable XAML hot reload monitoring" -ForegroundColor Gray
Write-Host "   bb-quick-test      - Rapid build-test-validate cycle" -ForegroundColor Gray
Write-Host "   bb-diagnostic      - Comprehensive project health check" -ForegroundColor Gray
Write-Host "   bb-report          - Generate detailed project report" -ForegroundColor Gray
Write-Host ""

function Get-SyncfusionErrors {
    param(
        [Parameter(Mandatory)]
        [string[]]$ErrorLines
    )
    $syncfusionErrors = $ErrorLines | Where-Object { $_ -match 'Syncfusion' }
    if ($syncfusionErrors) {
        Write-Host "Syncfusion-related errors detected:" -ForegroundColor Yellow
        $syncfusionErrors | ForEach-Object { Write-Host $_ -ForegroundColor Red }
    }
    else {
        Write-Host "No Syncfusion-related errors found."
    }
}

# Example usage:
# $logLines = Get-Content 'logs/build-latest.log'
# Get-SyncfusionErrors -ErrorLines $logLines
