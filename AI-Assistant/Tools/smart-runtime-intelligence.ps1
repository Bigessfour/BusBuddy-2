# ========================================
# BusBuddy Smart Runtime Intelligence Script
# Enhanced with Syncfusion WPF 30.1.40 API Guidance
# ========================================

param(
    [switch]$StreamLogs = $true,
    [switch]$MonitorHealth = $true,
    [string]$LogLevel = "Information",
    [switch]$SyncfusionDiagnostics = $true
)

$ErrorActionPreference = 'Continue'
$startTime = Get-Date

# Syncfusion WPF 30.1.40 API Knowledge Base
$syncfusionKnowledgeBase = @{
    'DateTimePattern'  = @{
        ErrorPatterns    = @(
            'Cannot convert.*DateTime.*"-"',
            'DateTimePattern.*not recognized',
            'String.*was not recognized as a valid DateTime'
        )
        ApiReference     = 'https://help.syncfusion.com/cr/wpf/Syncfusion.Windows.Shared.DateTimePattern.html'
        ProjectResources = @(
            'BusBuddy.WPF\Utilities\SyncfusionValidationUtility.cs',
            'BusBuddy.WPF\Extensions\SafeDateTimePatternExtension.cs'
        )
        ValidValues      = @('ShortDate', 'LongDate', 'FullDateTime', 'LongTime', 'ShortTime', 'MonthDay', 'YearMonth', 'CustomPattern')
        OfficialFix      = 'Use SafeDateTimePatternExtension markup extension or SyncfusionValidationUtility.GetSafeDateTimePattern()'
        Example          = '{Binding Path=Date, Converter={local:SafeDateTimePattern Pattern=ShortDate}}'
    }
    'SkinManager'      = @{
        ErrorPatterns    = @(
            'Theme.*not found',
            'SkinManager.*theme',
            'FluentDark.*not available'
        )
        ApiReference     = 'https://help.syncfusion.com/wpf/themes/skin-manager'
        ProjectResources = @(
            'BusBuddy.WPF\App.xaml.cs:ConfigureSyncfusionTheme()',
            'SYNCFUSION-SKINMANAGER-API-COMPLIANCE-REPORT.md'
        )
        ValidThemes      = @('FluentDark', 'FluentLight', 'MaterialDark', 'MaterialLight', 'Office2019Colorful')
        OfficialFix      = 'Ensure proper theme registration before SfSkinManager.ApplicationTheme assignment'
        Example          = 'SfSkinManager.RegisterThemeSettings("FluentDark", fluentDarkSettings); SfSkinManager.ApplicationTheme = new Theme("FluentDark");'
    }
    'ControlNamespace' = @{
        ErrorPatterns    = @(
            'The name.*does not exist in the namespace.*Syncfusion',
            'Could not load.*Syncfusion',
            'Assembly.*Syncfusion.*not found'
        )
        ApiReference     = 'https://help.syncfusion.com/wpf/control-dependencies'
        ProjectResources = @(
            'BusBuddy.WPF\BusBuddy.WPF.csproj',
            'STANDARDIZED-SYNCFUSION-REPLACEMENT-PLAN.md'
        )
        ValidNamespaces  = @(
            'xmlns:syncfusion="http://schemas.syncfusion.com/wpf"',
            'xmlns:sf="http://schemas.syncfusion.com/wpf"'
        )
        OfficialFix      = 'Check NuGet package references and XAML namespace declarations'
        Example          = 'Add PackageReference and xmlns:syncfusion="http://schemas.syncfusion.com/wpf" to XAML'
    }
    'LicenseKey'       = @{
        ErrorPatterns    = @(
            'Syncfusion license key',
            'Trial period.*expired',
            'Invalid license'
        )
        ApiReference     = 'https://help.syncfusion.com/common/essential-studio/licensing/license-key'
        ProjectResources = @(
            'BusBuddy.WPF\App.xaml.cs:RegisterSyncfusionLicense()'
        )
        OfficialFix      = 'Register license key before any Syncfusion control initialization'
        Example          = 'Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY");'
    }
}

# Function to analyze errors against Syncfusion WPF 30.1.40 API knowledge
function Analyze-SyncfusionErrors {
    param(
        [string[]]$ErrorLines,
        [string]$LogContent = ""
    )

    $detectedIssues = @()
    $allContent = ($ErrorLines -join " ") + " " + $LogContent

    foreach ($category in $syncfusionKnowledgeBase.Keys) {
        $knowledge = $syncfusionKnowledgeBase[$category]

        foreach ($pattern in $knowledge.ErrorPatterns) {
            if ($allContent -match $pattern) {
                $detectedIssues += @{
                    Category  = $category
                    Pattern   = $pattern
                    Knowledge = $knowledge
                    Severity  = if ($category -eq 'LicenseKey') { 'Critical' } else { 'High' }
                }
                break
            }
        }
    }

    return $detectedIssues
}

# Function to provide Syncfusion-guided next actions
function Get-SyncfusionGuidedActions {
    param(
        [array]$DetectedIssues,
        [string]$ExitCode = "0"
    )

    Write-Host ''
    Write-Host '🎯 Syncfusion WPF 30.1.40 API Guided Actions:' -ForegroundColor Cyan

    if ($DetectedIssues.Count -eq 0 -and $ExitCode -eq "0") {
        Write-Host '  ✅ No Syncfusion-specific issues detected' -ForegroundColor Green
        Write-Host '  📚 All controls following official API patterns' -ForegroundColor Green
        return
    }

    foreach ($issue in $DetectedIssues) {
        $knowledge = $issue.Knowledge
        $severity = if ($issue.Severity -eq 'Critical') { '🚨' } else { '⚠️' }

        Write-Host ''
        Write-Host "$severity Syncfusion Issue: $($issue.Category)" -ForegroundColor $(if ($issue.Severity -eq 'Critical') { 'Red' } else { 'Yellow' })
        Write-Host "   🔍 Pattern: $($issue.Pattern)" -ForegroundColor Gray
        Write-Host "   📖 Official API: $($knowledge.ApiReference)" -ForegroundColor Cyan

        # Show project resources
        if ($knowledge.ProjectResources) {
            Write-Host "   📁 Project Resources:" -ForegroundColor Magenta
            foreach ($resource in $knowledge.ProjectResources) {
                if (Test-Path $resource -ErrorAction SilentlyContinue) {
                    Write-Host "      ✅ $resource" -ForegroundColor Green
                }
                else {
                    Write-Host "      ❌ $resource (Missing)" -ForegroundColor Red
                }
            }
        }

        # Show valid values
        if ($knowledge.ValidValues) {
            Write-Host "   ✨ Valid Values: $($knowledge.ValidValues -join ', ')" -ForegroundColor Green
        }

        if ($knowledge.ValidThemes) {
            Write-Host "   🎨 Valid Themes: $($knowledge.ValidThemes -join ', ')" -ForegroundColor Green
        }

        if ($knowledge.ValidNamespaces) {
            Write-Host "   📦 Required Namespaces:" -ForegroundColor Green
            foreach ($ns in $knowledge.ValidNamespaces) {
                Write-Host "      $ns" -ForegroundColor Green
            }
        }

        # Show official fix
        Write-Host "   🔧 Official Fix: $($knowledge.OfficialFix)" -ForegroundColor Yellow
        Write-Host "   💡 Example: $($knowledge.Example)" -ForegroundColor Cyan
    }
}

Write-Host '🚀 BusBuddy Smart Runtime Intelligence' -ForegroundColor Cyan
Write-Host ('=' * 50) -ForegroundColor Gray
Write-Host ('📅 Launch Time: ' + $startTime.ToString('yyyy-MM-dd HH:mm:ss')) -ForegroundColor Yellow
Write-Host ''

# Pre-Launch Environment Validation
Write-Host '🔍 Pre-Launch Validation:' -ForegroundColor Green

# Check critical files and dependencies
$validationChecks = @{
    'Solution File' = 'BusBuddy.sln'
    'WPF Project'   = 'BusBuddy.WPF\BusBuddy.WPF.csproj'
    'Core Project'  = 'BusBuddy.Core\BusBuddy.Core.csproj'
    'App Config'    = 'BusBuddy.WPF\appsettings.json'
    'Log Directory' = 'logs\'
}

$allValid = $true
foreach ($check in $validationChecks.GetEnumerator()) {
    if (Test-Path $check.Value) {
        Write-Host ('  ✅ ' + $check.Key + ': Found') -ForegroundColor Green
    }
    else {
        Write-Host ('  ❌ ' + $check.Key + ': Missing (' + $check.Value + ')') -ForegroundColor Red
        $allValid = $false
    }
}

# Ensure logs directory exists
if (-not (Test-Path 'logs')) {
    New-Item -ItemType Directory -Path 'logs' -Force | Out-Null
    Write-Host '  📁 Created logs directory' -ForegroundColor Yellow
}

Write-Host ''

# Database connectivity check (if applicable)
Write-Host '💾 Database Connectivity:' -ForegroundColor Magenta
try {
    # This would check your connection string from appsettings.json
    $configPath = 'BusBuddy.WPF\appsettings.json'
    if (Test-Path $configPath) {
        $config = Get-Content $configPath | ConvertFrom-Json -ErrorAction SilentlyContinue
        if ($config.ConnectionStrings) {
            Write-Host '  ✅ Connection strings configured' -ForegroundColor Green
        }
        else {
            Write-Host '  ⚠️  No connection strings found' -ForegroundColor Yellow
        }
    }
}
catch {
    Write-Host '  ⚠️  Could not validate database config' -ForegroundColor Yellow
}

Write-Host ''

# Start application with enhanced monitoring
Write-Host '🚀 Starting BusBuddy Application...' -ForegroundColor Cyan
Write-Host ('  📊 Process ID will be tracked for monitoring') -ForegroundColor Gray
Write-Host ('  📝 Serilog output will be captured and displayed') -ForegroundColor Gray
Write-Host ('  🔄 Use Ctrl+C to stop gracefully') -ForegroundColor Gray
Write-Host ''

# Create background job to monitor log files
$logMonitorJob = $null
if ($StreamLogs -and (Test-Path 'logs')) {
    $logMonitorJob = Start-Job -ScriptBlock {
        param($LogPath, $LogLevel)

        # Monitor the most recent log file
        $logFiles = Get-ChildItem -Path $LogPath -Filter "*.log" | Sort-Object LastWriteTime -Descending
        if ($logFiles.Count -gt 0) {
            $latestLog = $logFiles[0].FullName
            Write-Host "📄 Monitoring log file: $($logFiles[0].Name)" -ForegroundColor Cyan

            # Follow the log file like 'tail -f'
            Get-Content -Path $latestLog -Wait -Tail 0 | ForEach-Object {
                $line = $_
                if ($line -match '\[(ERR|ERROR)\]') {
                    Write-Host "🚨 $line" -ForegroundColor Red
                }
                elseif ($line -match '\[(WARN|WARNING)\]') {
                    Write-Host "⚠️  $line" -ForegroundColor Yellow
                }
                elseif ($line -match '\[INF|INFO\]') {
                    Write-Host "ℹ️  $line" -ForegroundColor Cyan
                }
                elseif ($line -match '\[DBG|DEBUG\]') {
                    Write-Host "🔍 $line" -ForegroundColor Gray
                }
                else {
                    Write-Host "📝 $line" -ForegroundColor White
                }
            }
        }
    } -ArgumentList (Resolve-Path 'logs'), $LogLevel
}

Write-Host '📊 Runtime Metrics:' -ForegroundColor Magenta
$appStartTime = Get-Date

# Start the application with output capture
try {
    # Use direct dotnet command to avoid PowerShell parameter binding issues
    Write-Host '  🔧 Using direct dotnet execution to avoid parameter binding issues' -ForegroundColor Yellow

    # Execute dotnet run directly with proper error handling
    $env:DOTNET_ENVIRONMENT = "Development"
    $processInfo = Start-Process -FilePath "dotnet" -ArgumentList "run", "--project", "BusBuddy.WPF\BusBuddy.WPF.csproj", "--verbosity", "minimal" -PassThru -NoNewWindow

    if ($processInfo) {
        Write-Host ('  🆔 Process ID: ' + $processInfo.Id) -ForegroundColor Green
        Write-Host ('  📈 Memory (Initial): ' + [math]::Round($processInfo.WorkingSet64 / 1MB, 1) + ' MB') -ForegroundColor Green
        Write-Host ('  ⏱️  Startup Duration: ' + (Get-Date - $appStartTime).TotalSeconds.ToString('F1') + 's') -ForegroundColor Green

        Write-Host ''
        Write-Host '🎯 Application Status: RUNNING' -ForegroundColor Green
        Write-Host '📝 Live Serilog Output:' -ForegroundColor Cyan
        Write-Host ('─' * 50) -ForegroundColor Gray

        # Monitor process health periodically
        $healthCheckJob = Start-Job -ScriptBlock {
            param($ProcessId)

            while ($true) {
                Start-Sleep -Seconds 10
                try {
                    $proc = Get-Process -Id $ProcessId -ErrorAction SilentlyContinue
                    if ($proc) {
                        $memMB = [math]::Round($proc.WorkingSet64 / 1MB, 1)
                        $cpuTime = $proc.TotalProcessorTime.TotalSeconds
                        Write-Host "📊 Health Check - Memory: ${memMB}MB, CPU Time: ${cpuTime}s" -ForegroundColor Magenta
                    }
                    else {
                        Write-Host "⚠️  Process appears to have exited" -ForegroundColor Yellow
                        break
                    }
                }
                catch {
                    break
                }
            }
        } -ArgumentList $processInfo.Id

        # Wait for process to complete
        $processInfo.WaitForExit()

        # Cleanup
        if ($healthCheckJob) {
            Stop-Job $healthCheckJob -ErrorAction SilentlyContinue
            Remove-Job $healthCheckJob -ErrorAction SilentlyContinue
        }

        $endTime = Get-Date
        $totalRuntime = $endTime - $appStartTime

        Write-Host ''
        Write-Host ('─' * 50) -ForegroundColor Gray
        Write-Host '📊 Final Runtime Report:' -ForegroundColor Magenta
        Write-Host ('  ⏱️  Total Runtime: ' + $totalRuntime.ToString("mm\:ss")) -ForegroundColor Yellow
        Write-Host ('  🏁 Exit Code: ' + $processInfo.ExitCode) -ForegroundColor $(if ($processInfo.ExitCode -eq 0) { 'Green' } else { 'Red' })

        if ($processInfo.ExitCode -eq 0) {
            Write-Host '  ✅ Application exited normally' -ForegroundColor Green
        }
        else {
            Write-Host '  ❌ Application exited with errors' -ForegroundColor Red

            # Analyze any logged errors for Syncfusion issues
            if ($SyncfusionDiagnostics) {
                $logContent = ""
                $logFiles = Get-ChildItem -Path 'logs' -Filter "*.log" -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending
                if ($logFiles.Count -gt 0) {
                    $logContent = Get-Content $logFiles[0].FullName -Tail 50 -ErrorAction SilentlyContinue | Out-String
                }

                $syncfusionIssues = Analyze-SyncfusionErrors -ErrorLines @() -LogContent $logContent
                Get-SyncfusionGuidedActions -DetectedIssues $syncfusionIssues -ExitCode $processInfo.ExitCode.ToString()
            }
        }
    }
}
catch {
    $errorMessage = $_.Exception.Message
    Write-Host ('🚨 Failed to start application: ' + $errorMessage) -ForegroundColor Red

    # Analyze error against Syncfusion knowledge base
    if ($SyncfusionDiagnostics) {
        $syncfusionIssues = Analyze-SyncfusionErrors -ErrorLines @($errorMessage)
        Get-SyncfusionGuidedActions -DetectedIssues $syncfusionIssues -ExitCode "1"
    }
}
finally {
    # Cleanup background jobs
    if ($logMonitorJob) {
        Stop-Job $logMonitorJob -ErrorAction SilentlyContinue
        Remove-Job $logMonitorJob -ErrorAction SilentlyContinue
    }
}

Write-Host ''
Write-Host '🎯 Smart Runtime Intelligence Complete' -ForegroundColor Cyan
Write-Host ('📈 Session ended: ' + (Get-Date -Format 'HH:mm:ss')) -ForegroundColor Gray

# Offer next actions
Write-Host ''
Write-Host '💡 Standard Next Actions:' -ForegroundColor Yellow
Write-Host '  📄 Check logs/ directory for detailed Serilog output' -ForegroundColor Cyan
Write-Host '  🔍 Run Smart Build Intelligence for build analysis' -ForegroundColor Cyan
Write-Host '  🧪 Run tests to validate functionality' -ForegroundColor Cyan

# Additional Syncfusion-specific guidance
Write-Host ''
Write-Host '📚 Syncfusion WPF 30.1.40 Resources:' -ForegroundColor Magenta
Write-Host '  📖 API Documentation: https://help.syncfusion.com/wpf/welcome-to-syncfusion-essential-wpf' -ForegroundColor Cyan
Write-Host '  🛠️  Project Utilities: BusBuddy.WPF\Utilities\SyncfusionValidationUtility.cs' -ForegroundColor Cyan
Write-Host '  📋 Compliance Report: SYNCFUSION-SKINMANAGER-API-COMPLIANCE-REPORT.md' -ForegroundColor Cyan
Write-Host '  🔧 Replacement Plan: STANDARDIZED-SYNCFUSION-REPLACEMENT-PLAN.md' -ForegroundColor Cyan
