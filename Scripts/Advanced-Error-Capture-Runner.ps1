#Requires -Version 7.5
<#
.SYNOPSIS
    Advanced Error Capture and Application Runner for BusBuddy

.DESCRIPTION
    Implements the complete error capture workflow:
    1. Start error capture/monitoring
    2. Launch BusBuddy application
    3. Monitor for runtime errors and crashes
    4. Stop capture when application closes or user continues

.PARAMETER CaptureMode
    Type of error capture: 'Console', 'File', 'Both', 'Debug'

.PARAMETER OutputPath
    Directory for error capture logs (default: logs/error-capture)

.PARAMETER RealTimeDisplay
    Show real-time error output in console

.PARAMETER WaitForUserAction
    Wait for explicit user action to stop monitoring

.EXAMPLE
    .\Scripts\Advanced-Error-Capture-Runner.ps1 -CaptureMode Both -RealTimeDisplay

.NOTES
    Phase 1 Focus: Get the application running with comprehensive error capture
    Follows BusBuddy instructions.md: "more time developing, less time fixing"
#>

param(
    [ValidateSet('Console', 'File', 'Both', 'Debug')]
    [string]$CaptureMode = 'Both',

    [string]$OutputPath = "logs/error-capture",

    [switch]$RealTimeDisplay,

    [switch]$WaitForUserAction,

    [int]$MaxCaptureTime = 1800 # 30 minutes max
)

# ============================================================================
# SETUP AND INITIALIZATION
# ============================================================================

$ErrorActionPreference = 'Continue'

# Import PowerShell Gallery terminal monitoring tools
try {
    Import-Module Show-Progress, PSWriteColor, psInlineProgress -Force -ErrorAction SilentlyContinue
    $Global:TerminalMonitoringAvailable = $true
    Write-Host "✅ Enhanced terminal monitoring enabled (PowerShell Gallery tools)" -ForegroundColor Green
}
catch {
    $Global:TerminalMonitoringAvailable = $false
    Write-Host "⚠️ Advanced terminal monitoring not available - using basic mode" -ForegroundColor Yellow
    Write-Host "   Install with: Install-Module Show-Progress, PSWriteColor, psInlineProgress -Force" -ForegroundColor Gray
}

$Global:ErrorCaptureSession = @{
    StartTime          = Get-Date
    Errors             = @()
    Warnings           = @()
    Process            = $null
    CaptureJob         = $null
    LogFile            = $null
    IsCapturing        = $false
    TerminalMonitoring = $Global:TerminalMonitoringAvailable
}

Write-Host "🚌 BusBuddy Advanced Error Capture Runner" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor DarkGray

if ($Global:TerminalMonitoringAvailable) {
    Write-Color -Text "📊 Capture Mode: ", $CaptureMode -Color Yellow, White
    Write-Color -Text "📁 Output Path: ", $OutputPath -Color Yellow, White
    Write-Color -Text "⏱️ Max Capture Time: ", "$($MaxCaptureTime / 60) minutes" -Color Yellow, White
    Write-Color -Text "🌊 Terminal Flow Monitoring: ", "Enhanced (Gallery Tools)" -Color Green, Cyan
}
else {
    Write-Host "📊 Capture Mode: $CaptureMode" -ForegroundColor Yellow
    Write-Host "📁 Output Path: $OutputPath" -ForegroundColor Yellow
    Write-Host "⏱️ Max Capture Time: $($MaxCaptureTime / 60) minutes" -ForegroundColor Yellow
    Write-Host "🌊 Terminal Flow Monitoring: Basic" -ForegroundColor Gray
}
Write-Host ""

# ============================================================================
# ERROR CAPTURE FUNCTIONS
# ============================================================================

function Initialize-ErrorCapture {
    param(
        [string]$LogDirectory
    )

    Write-Host "🔧 Initializing error capture system..." -ForegroundColor Cyan

    # Create log directory
    if (-not (Test-Path $LogDirectory)) {
        New-Item -ItemType Directory -Path $LogDirectory -Force | Out-Null
    }

    # Setup log file with timestamp
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $Global:ErrorCaptureSession.LogFile = Join-Path $LogDirectory "busbuddy-errors-$timestamp.log"

    # Initialize log file
    $header = @"
# BusBuddy Error Capture Session
# Started: $(Get-Date)
# Capture Mode: $CaptureMode
# Real-time Display: $RealTimeDisplay
═══════════════════════════════════════════════════════════

"@

    Set-Content -Path $Global:ErrorCaptureSession.LogFile -Value $header -Encoding UTF8

    Write-Host "✅ Error capture initialized" -ForegroundColor Green
    Write-Host "📄 Log file: $($Global:ErrorCaptureSession.LogFile)" -ForegroundColor Gray
    Write-Host ""
}

function Start-ApplicationWithCapture {
    Write-Host "🚀 Starting BusBuddy application with error capture..." -ForegroundColor Cyan

    try {
        # Ensure we're in the correct directory
        $projectRoot = Get-Location
        $wpfProject = Join-Path $projectRoot "BusBuddy.WPF\BusBuddy.WPF.csproj"

        if (-not (Test-Path $wpfProject)) {
            throw "BusBuddy.WPF project not found at: $wpfProject"
        }

        # Start the application WITHOUT redirecting streams (to keep WPF responsive)
        $processInfo = New-Object System.Diagnostics.ProcessStartInfo
        $processInfo.FileName = "dotnet"
        $processInfo.Arguments = "run --project `"$wpfProject`""
        $processInfo.UseShellExecute = $true  # Changed to true for WPF compatibility
        $processInfo.CreateNoWindow = $false
        $processInfo.WorkingDirectory = $projectRoot

        # Start the process
        $Global:ErrorCaptureSession.Process = [System.Diagnostics.Process]::Start($processInfo)
        $Global:ErrorCaptureSession.IsCapturing = $true

        # Wait a moment for the process to initialize
        Start-Sleep -Seconds 2

        Write-Host "✅ Application started (PID: $($Global:ErrorCaptureSession.Process.Id))" -ForegroundColor Green
        Write-Host "⏳ Monitoring application lifecycle..." -ForegroundColor Yellow
        Write-Host "📱 Application should be responsive and interactive" -ForegroundColor Green
        Write-Host ""

        # Start alternative monitoring (process-based, not stream-based)
        Start-ProcessMonitoring

        return $true
    }
    catch {
        Write-Host "❌ Failed to start application: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Start-ProcessMonitoring {
    Write-Host "👁️ Starting process monitoring..." -ForegroundColor Cyan

    # Create background job for process monitoring (not stream monitoring)
    $Global:ErrorCaptureSession.CaptureJob = Start-Job -ScriptBlock {
        param($ProcessId, $LogFile, $CaptureMode, $RealTimeDisplay)

        $errors = @()
        $warnings = @()
        $process = Get-Process -Id $ProcessId -ErrorAction SilentlyContinue

        # Monitor process health and system events
        while ($process -and -not $process.HasExited) {
            try {
                # Refresh process info
                $process = Get-Process -Id $ProcessId -ErrorAction SilentlyContinue

                if (-not $process) {
                    $logEntry = "$(Get-Date -Format 'HH:mm:ss.fff') PROCESS: Application process ended unexpectedly"
                    $errors += $logEntry
                    if ($CaptureMode -eq 'File' -or $CaptureMode -eq 'Both') {
                        Add-Content -Path $LogFile -Value $logEntry -Encoding UTF8
                    }
                    break
                }

                # Check for high CPU usage or memory issues (basic health monitoring)
                try {
                    $cpuUsage = $process.CPU
                    $memoryUsage = $process.WorkingSet64 / 1MB

                    # Log significant resource usage (over 500MB or high CPU)
                    if ($memoryUsage -gt 500) {
                        $logEntry = "$(Get-Date -Format 'HH:mm:ss.fff') WARNING: High memory usage: $([math]::Round($memoryUsage, 2)) MB"
                        $warnings += $logEntry
                        if ($CaptureMode -eq 'File' -or $CaptureMode -eq 'Both') {
                            Add-Content -Path $LogFile -Value $logEntry -Encoding UTF8
                        }
                        if ($RealTimeDisplay) {
                            Write-Host "⚠️ $logEntry" -ForegroundColor Yellow
                        }
                    }
                }
                catch {
                    # Process access issues
                }

                # Check Windows Event Log for application errors (last 30 seconds)
                try {
                    $recentTime = (Get-Date).AddSeconds(-30)
                    $appErrors = Get-WinEvent -FilterHashtable @{
                        LogName   = 'Application'
                        Level     = 2  # Error level
                        StartTime = $recentTime
                    } -MaxEvents 5 -ErrorAction SilentlyContinue |
                    Where-Object { $_.ProcessId -eq $ProcessId -or $_.Message -match "BusBuddy|dotnet" }

                    foreach ($error in $appErrors) {
                        $logEntry = "$(Get-Date -Format 'HH:mm:ss.fff') SYSTEM_ERROR: $($error.LevelDisplayName) - $($error.Message)"
                        $errors += $logEntry
                        if ($CaptureMode -eq 'File' -or $CaptureMode -eq 'Both') {
                            Add-Content -Path $LogFile -Value $logEntry -Encoding UTF8
                        }
                        if ($RealTimeDisplay) {
                            Write-Host "🔴 $logEntry" -ForegroundColor Red
                        }
                    }
                }
                catch {
                    # Event log access issues - continue monitoring
                }

                Start-Sleep -Seconds 5  # Check every 5 seconds
            }
            catch {
                # Process monitoring error - continue
                Start-Sleep -Seconds 5
            }
        }

        return @{
            Errors   = $errors
            Warnings = $warnings
            ExitCode = if ($process) { $process.ExitCode } else { -1 }
            EndTime  = Get-Date
        }
    } -ArgumentList $Global:ErrorCaptureSession.Process.Id, $Global:ErrorCaptureSession.LogFile, $CaptureMode, $RealTimeDisplay

    Write-Host "✅ Process monitoring started (non-intrusive)" -ForegroundColor Green
}

function Wait-ForApplicationClose {
    Write-Host "⏳ Waiting for application to close..." -ForegroundColor Yellow
    Write-Host "   • Close the application window (X button)" -ForegroundColor Gray
    Write-Host "   • Or click Continue on any error dialogs" -ForegroundColor Gray
    Write-Host "   • Press Ctrl+C to force stop monitoring" -ForegroundColor Gray
    Write-Host ""

    $startTime = Get-Date
    $maxWaitTime = [TimeSpan]::FromSeconds($MaxCaptureTime)

    try {
        while ($Global:ErrorCaptureSession.IsCapturing) {
            # Check if process has exited
            if ($Global:ErrorCaptureSession.Process -and $Global:ErrorCaptureSession.Process.HasExited) {
                Write-Host "✅ Application has closed (Exit Code: $($Global:ErrorCaptureSession.Process.ExitCode))" -ForegroundColor Green
                break
            }

            # Check for timeout
            if ((Get-Date) - $startTime -gt $maxWaitTime) {
                Write-Host "⏰ Maximum capture time reached ($($MaxCaptureTime / 60) minutes)" -ForegroundColor Yellow
                break
            }

            # Check if user wants to continue manually
            if ($WaitForUserAction) {
                if ([Console]::KeyAvailable) {
                    $key = [Console]::ReadKey($true)
                    if ($key.Key -eq 'C' -and $key.Modifiers -eq 'Control') {
                        Write-Host "🛑 User requested stop (Ctrl+C)" -ForegroundColor Yellow
                        break
                    }
                }
            }

            Start-Sleep -Seconds 1

            # Show periodic status
            $elapsed = (Get-Date) - $startTime
            if ($elapsed.TotalSeconds % 30 -eq 0) {
                Write-Host "⏱️ Monitoring for $([int]$elapsed.TotalMinutes) minutes..." -ForegroundColor DarkGray
            }
        }
    }
    catch {
        Write-Host "⚠️ Monitoring interrupted: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

function Stop-ErrorCapture {
    Write-Host "🛑 Stopping error capture..." -ForegroundColor Yellow

    $Global:ErrorCaptureSession.IsCapturing = $false

    # Get results from monitoring job
    if ($Global:ErrorCaptureSession.CaptureJob) {
        $results = Receive-Job -Job $Global:ErrorCaptureSession.CaptureJob -Wait
        Remove-Job -Job $Global:ErrorCaptureSession.CaptureJob -Force

        if ($results) {
            $Global:ErrorCaptureSession.Errors += $results.Errors
            $Global:ErrorCaptureSession.Warnings += $results.Warnings
        }
    }

    # Clean up process if still running
    if ($Global:ErrorCaptureSession.Process -and -not $Global:ErrorCaptureSession.Process.HasExited) {
        try {
            $Global:ErrorCaptureSession.Process.Kill()
            Write-Host "🔪 Application process terminated" -ForegroundColor Yellow
        }
        catch {
            Write-Host "⚠️ Could not terminate process: $($_.Exception.Message)" -ForegroundColor Yellow
        }
    }

    Write-Host "✅ Error capture stopped" -ForegroundColor Green
}

function Show-CaptureResults {
    Write-Host ""
    Write-Host "📊 Error Capture Results" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor DarkGray

    $duration = (Get-Date) - $Global:ErrorCaptureSession.StartTime
    Write-Host "⏱️ Capture Duration: $([int]$duration.TotalMinutes) minutes, $([int]$duration.Seconds) seconds" -ForegroundColor Gray
    Write-Host "🔴 Errors Captured: $($Global:ErrorCaptureSession.Errors.Count)" -ForegroundColor $(if ($Global:ErrorCaptureSession.Errors.Count -gt 0) { "Red" } else { "Green" })
    Write-Host "⚠️ Warnings Captured: $($Global:ErrorCaptureSession.Warnings.Count)" -ForegroundColor $(if ($Global:ErrorCaptureSession.Warnings.Count -gt 0) { "Yellow" } else { "Green" })

    if ($Global:ErrorCaptureSession.LogFile -and (Test-Path $Global:ErrorCaptureSession.LogFile)) {
        Write-Host "📄 Log File: $($Global:ErrorCaptureSession.LogFile)" -ForegroundColor Gray
        Write-Host "📏 Log Size: $([math]::Round((Get-Item $Global:ErrorCaptureSession.LogFile).Length / 1KB, 2)) KB" -ForegroundColor Gray
    }

    Write-Host ""

    # Show recent errors if any
    if ($Global:ErrorCaptureSession.Errors.Count -gt 0) {
        Write-Host "🔴 Recent Errors:" -ForegroundColor Red
        $Global:ErrorCaptureSession.Errors | Select-Object -Last 5 | ForEach-Object {
            Write-Host "   $($_)" -ForegroundColor DarkRed
        }
        Write-Host ""
    }

    # Show recent warnings if any
    if ($Global:ErrorCaptureSession.Warnings.Count -gt 0) {
        Write-Host "⚠️ Recent Warnings:" -ForegroundColor Yellow
        $Global:ErrorCaptureSession.Warnings | Select-Object -Last 3 | ForEach-Object {
            Write-Host "   $($_)" -ForegroundColor DarkYellow
        }
        Write-Host ""
    }

    # Provide recommendations
    if ($Global:ErrorCaptureSession.Errors.Count -eq 0 -and $Global:ErrorCaptureSession.Warnings.Count -eq 0) {
        Write-Host "✅ No errors or warnings detected - application ran successfully!" -ForegroundColor Green
    }
    elseif ($Global:ErrorCaptureSession.Errors.Count -gt 0) {
        Write-Host "🔧 Recommendations:" -ForegroundColor Cyan
        Write-Host "   • Review error log for detailed analysis" -ForegroundColor Gray
        Write-Host "   • Check application startup dependencies" -ForegroundColor Gray
        Write-Host "   • Verify database connection and configuration" -ForegroundColor Gray
    }
}

# ============================================================================
# MAIN EXECUTION WORKFLOW
# ============================================================================

function Start-BusBuddyWithErrorCapture {
    try {
        Write-Host "🎯 Phase 1: Initialize Error Capture" -ForegroundColor Magenta
        Initialize-ErrorCapture -LogDirectory $OutputPath

        Write-Host "🎯 Phase 2: Start Application with Monitoring" -ForegroundColor Magenta
        $appStarted = Start-ApplicationWithCapture

        if (-not $appStarted) {
            throw "Failed to start BusBuddy application"
        }

        Write-Host "🎯 Phase 3: Monitor Application Lifecycle" -ForegroundColor Magenta
        Wait-ForApplicationClose

        Write-Host "🎯 Phase 4: Finalize and Report Results" -ForegroundColor Magenta
        Stop-ErrorCapture
        Show-CaptureResults

        Write-Host ""
        Write-Host "🎉 Error capture session completed successfully!" -ForegroundColor Green

    }
    catch {
        Write-Host ""
        Write-Host "❌ Error capture session failed: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "💡 Check the log file for more details: $($Global:ErrorCaptureSession.LogFile)" -ForegroundColor Yellow

        # Clean up on error
        if ($Global:ErrorCaptureSession.IsCapturing) {
            Stop-ErrorCapture
        }

        exit 1
    }
}

# ============================================================================
# SCRIPT ENTRY POINT
# ============================================================================

# Ensure we're in the project root
if (-not (Test-Path "BusBuddy.sln")) {
    Write-Host "❌ Must be run from BusBuddy project root directory" -ForegroundColor Red
    exit 1
}

# Start the complete workflow
Start-BusBuddyWithErrorCapture
