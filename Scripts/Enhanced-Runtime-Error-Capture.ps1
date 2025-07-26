#Requires -Version 7.5
<#
.SYNOPSIS
    Enhanced Runtime Error Capture for BusBuddy WPF Form

.DESCRIPTION
    Captures detailed runtime errors from the BusBuddy WPF application including:
    - Application crashes and unhandled exceptions
    - WPF-specific errors (binding, dependency property, etc.)
    - .NET runtime errors and stack traces
    - Windows Event Log integration
    - Real-time error streaming with PowerShell Gallery tools

.PARAMETER CaptureLevel
    Level of error capture: 'Basic', 'Detailed', 'Verbose', 'All'

.PARAMETER MonitorWPFErrors
    Enable WPF-specific error monitoring (binding errors, etc.)

.PARAMETER CaptureStackTraces
    Include full stack traces in error reports

.PARAMETER RealTimeStream
    Stream errors in real-time to console

.EXAMPLE
    .\Scripts\Enhanced-Runtime-Error-Capture.ps1 -CaptureLevel All -MonitorWPFErrors -RealTimeStream

.NOTES
    Uses PowerShell Gallery tools: Show-Progress, PSWriteColor, psInlineProgress
    Integrates with Windows Event Log and .NET diagnostics
#>

param(
    [ValidateSet('Basic', 'Detailed', 'Verbose', 'All')]
    [string]$CaptureLevel = 'All',

    [switch]$MonitorWPFErrors,

    [switch]$CaptureStackTraces,

    [switch]$RealTimeStream,

    [string]$LogDirectory = "logs/runtime-errors"
)

# Import PowerShell Gallery terminal monitoring tools
try {
    Import-Module Show-Progress, PSWriteColor, psInlineProgress -Force -ErrorAction Stop
    $Global:EnhancedMonitoring = $true
    Write-Color -Text "✅ Enhanced runtime monitoring loaded ", "(PowerShell Gallery)" -Color Green, Cyan
}
catch {
    $Global:EnhancedMonitoring = $false
    Write-Host "⚠️ Basic monitoring mode - install Gallery tools for enhanced features" -ForegroundColor Yellow
}

# Global error capture session
$Global:RuntimeErrorSession = @{
    StartTime      = Get-Date
    ApplicationPID = $null
    Process        = $null
    ErrorCount     = 0
    WarningCount   = 0
    CrashDetected  = $false
    Errors         = @()
    Warnings       = @()
    StackTraces    = @()
    WPFErrors      = @()
    LogFile        = $null
    EventLogErrors = @()
    MonitoringJob  = $null
}

Write-Host ""
Write-Host "🔍 BusBuddy Enhanced Runtime Error Capture" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor DarkGray

if ($Global:EnhancedMonitoring) {
    Write-Color -Text "📊 Capture Level: ", $CaptureLevel -Color Yellow, White
    Write-Color -Text "🎨 WPF Error Monitoring: ", $(if ($MonitorWPFErrors) { "Enabled" } else { "Disabled" }) -Color Yellow, $(if ($MonitorWPFErrors) { "Green" } else { "Gray" })
    Write-Color -Text "📚 Stack Traces: ", $(if ($CaptureStackTraces) { "Enabled" } else { "Disabled" }) -Color Yellow, $(if ($CaptureStackTraces) { "Green" } else { "Gray" })
    Write-Color -Text "🌊 Real-time Stream: ", $(if ($RealTimeStream) { "Enabled" } else { "Disabled" }) -Color Yellow, $(if ($RealTimeStream) { "Green" } else { "Gray" })
}
else {
    Write-Host "📊 Capture Level: $CaptureLevel" -ForegroundColor Yellow
    Write-Host "🎨 WPF Error Monitoring: $(if ($MonitorWPFErrors) { 'Enabled' } else { 'Disabled' })" -ForegroundColor Yellow
    Write-Host "📚 Stack Traces: $(if ($CaptureStackTraces) { 'Enabled' } else { 'Disabled' })" -ForegroundColor Yellow
}
Write-Host ""

function Initialize-RuntimeErrorCapture {
    param([string]$LogDirectory)

    if ($Global:EnhancedMonitoring) {
        Write-Color -Text "🔧 Initializing enhanced runtime error capture..." -Color Cyan, White
    }
    else {
        Write-Host "🔧 Initializing runtime error capture..." -ForegroundColor Cyan
    }

    # Create log directory
    if (-not (Test-Path $LogDirectory)) {
        New-Item -ItemType Directory -Path $LogDirectory -Force | Out-Null
    }

    # Setup comprehensive log file
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $Global:RuntimeErrorSession.LogFile = Join-Path $LogDirectory "busbuddy-runtime-errors-$timestamp.log"

    # Initialize log with comprehensive header
    $header = @"
# BusBuddy Enhanced Runtime Error Capture Session
# Started: $(Get-Date)
# Capture Level: $CaptureLevel
# WPF Error Monitoring: $MonitorWPFErrors
# Stack Traces: $CaptureStackTraces
# Real-time Stream: $RealTimeStream
# PowerShell Gallery Tools: $Global:EnhancedMonitoring
# Exit Code Meanings:
#   0 = Success
#   -1073741510 = STATUS_CONTROL_C_EXIT (Ctrl+C or forced termination)
#   -1073741819 = STATUS_ACCESS_VIOLATION (Memory access violation)
#   -1073740791 = STATUS_INVALID_IMAGE_FORMAT (Invalid executable)
═══════════════════════════════════════════════════════════

"@

    Set-Content -Path $Global:RuntimeErrorSession.LogFile -Value $header -Encoding UTF8

    if ($Global:EnhancedMonitoring) {
        Write-Color -Text "✅ Runtime error capture initialized" -Color Green, Green
        Write-Color -Text "📄 Log file: ", $Global:RuntimeErrorSession.LogFile -Color Gray, Gray
    }
    else {
        Write-Host "✅ Runtime error capture initialized" -ForegroundColor Green
        Write-Host "📄 Log file: $($Global:RuntimeErrorSession.LogFile)" -ForegroundColor Gray
    }
    Write-Host ""
}

function Start-ApplicationWithRuntimeMonitoring {
    if ($Global:EnhancedMonitoring) {
        Write-Color -Text "🚀 Starting BusBuddy with enhanced runtime monitoring..." -Color Magenta, White
    }
    else {
        Write-Host "🚀 Starting BusBuddy with runtime monitoring..." -ForegroundColor Magenta
    }

    try {
        $projectRoot = Get-Location
        $wpfProject = Join-Path $projectRoot "BusBuddy.WPF\BusBuddy.WPF.csproj"

        if (-not (Test-Path $wpfProject)) {
            throw "BusBuddy.WPF project not found at: $wpfProject"
        }

        # Configure process start with enhanced error capture
        $processInfo = New-Object System.Diagnostics.ProcessStartInfo
        $processInfo.FileName = "dotnet"
        $processInfo.Arguments = "run --project `"$wpfProject`" --verbosity detailed"
        $processInfo.UseShellExecute = $false  # Changed to false for error capture
        $processInfo.RedirectStandardError = $true
        $processInfo.RedirectStandardOutput = $true
        $processInfo.CreateNoWindow = $false
        $processInfo.WorkingDirectory = $projectRoot

        # Start process with error monitoring
        $Global:RuntimeErrorSession.Process = New-Object System.Diagnostics.Process
        $Global:RuntimeErrorSession.Process.StartInfo = $processInfo
        $Global:RuntimeErrorSession.Process.EnableRaisingEvents = $true

        # Setup error data received event
        $Global:RuntimeErrorSession.Process.add_ErrorDataReceived({
                param($sender, $e)
                if ($e.Data) {
                    $errorEntry = "$(Get-Date -Format 'HH:mm:ss.fff') STDERR: $($e.Data)"
                    $Global:RuntimeErrorSession.Errors += $errorEntry
                    $Global:RuntimeErrorSession.ErrorCount++

                    Add-Content -Path $Global:RuntimeErrorSession.LogFile -Value $errorEntry -Encoding UTF8

                    if ($RealTimeStream) {
                        if ($Global:EnhancedMonitoring) {
                            Write-Color -Text "🔴 ERROR: ", $e.Data -Color Red, DarkRed
                        }
                        else {
                            Write-Host "🔴 ERROR: $($e.Data)" -ForegroundColor Red
                        }
                    }
                }
            })

        # Setup output data received event
        $Global:RuntimeErrorSession.Process.add_OutputDataReceived({
                param($sender, $e)
                if ($e.Data) {
                    $outputEntry = "$(Get-Date -Format 'HH:mm:ss.fff') STDOUT: $($e.Data)"

                    # Check for warnings and WPF-specific errors in output
                    if ($e.Data -match "(warning|warn|WPF|binding|dependency|property)" -and $MonitorWPFErrors) {
                        $Global:RuntimeErrorSession.Warnings += $outputEntry
                        $Global:RuntimeErrorSession.WarningCount++

                        if ($e.Data -match "binding|dependency|property") {
                            $Global:RuntimeErrorSession.WPFErrors += $outputEntry
                        }

                        if ($RealTimeStream) {
                            if ($Global:EnhancedMonitoring) {
                                Write-Color -Text "⚠️ WARNING: ", $e.Data -Color Yellow, DarkYellow
                            }
                            else {
                                Write-Host "⚠️ WARNING: $($e.Data)" -ForegroundColor Yellow
                            }
                        }
                    }

                    Add-Content -Path $Global:RuntimeErrorSession.LogFile -Value $outputEntry -Encoding UTF8
                }
            })

        # Start the process
        $Global:RuntimeErrorSession.Process.Start()
        $Global:RuntimeErrorSession.ApplicationPID = $Global:RuntimeErrorSession.Process.Id

        # Begin async reading
        $Global:RuntimeErrorSession.Process.BeginErrorReadLine()
        $Global:RuntimeErrorSession.Process.BeginOutputReadLine()

        if ($Global:EnhancedMonitoring) {
            Write-Color -Text "✅ Application started ", "(PID: $($Global:RuntimeErrorSession.ApplicationPID))" -Color Green, Cyan
            Write-Color -Text "📡 Monitoring: ", "STDERR, STDOUT, Windows Events, Process Health" -Color Blue, White
        }
        else {
            Write-Host "✅ Application started (PID: $($Global:RuntimeErrorSession.ApplicationPID))" -ForegroundColor Green
            Write-Host "📡 Monitoring: STDERR, STDOUT, Windows Events" -ForegroundColor Blue
        }

        # Start comprehensive monitoring
        Start-ComprehensiveErrorMonitoring

        return $true
    }
    catch {
        if ($Global:EnhancedMonitoring) {
            Write-Color -Text "❌ Failed to start application: ", $_.Exception.Message -Color Red, DarkRed
        }
        else {
            Write-Host "❌ Failed to start application: $($_.Exception.Message)" -ForegroundColor Red
        }
        return $false
    }
}

function Start-ComprehensiveErrorMonitoring {
    if ($Global:EnhancedMonitoring) {
        Write-Color -Text "🔍 Starting comprehensive error monitoring..." -Color Blue, White
    }
    else {
        Write-Host "🔍 Starting comprehensive error monitoring..." -ForegroundColor Blue
    }

    # Create monitoring job
    $Global:RuntimeErrorSession.MonitoringJob = Start-Job -ScriptBlock {
        param($PID, $LogFile, $CaptureLevel, $CaptureStackTraces, $RealTimeStream, $EnhancedMonitoring)

        $errors = @()
        $warnings = @()
        $eventLogErrors = @()
        $crashDetected = $false

        try {
            $process = Get-Process -Id $PID -ErrorAction SilentlyContinue
            $startTime = Get-Date

            while ($process -and -not $process.HasExited) {
                try {
                    # Refresh process information
                    $process = Get-Process -Id $PID -ErrorAction SilentlyContinue

                    if (-not $process) {
                        $logEntry = "$(Get-Date -Format 'HH:mm:ss.fff') MONITOR: Process ended unexpectedly"
                        $errors += $logEntry
                        Add-Content -Path $LogFile -Value $logEntry -Encoding UTF8
                        $crashDetected = $true
                        break
                    }

                    # Monitor process health
                    try {
                        $cpuUsage = $process.CPU
                        $memoryUsage = $process.WorkingSet64 / 1MB
                        $handleCount = $process.HandleCount

                        # Log significant resource issues
                        if ($memoryUsage -gt 1000) {
                            # Over 1GB
                            $logEntry = "$(Get-Date -Format 'HH:mm:ss.fff') PERFORMANCE: High memory usage: $([math]::Round($memoryUsage, 2)) MB"
                            $warnings += $logEntry
                            Add-Content -Path $LogFile -Value $logEntry -Encoding UTF8
                        }

                        if ($handleCount -gt 10000) {
                            # High handle count
                            $logEntry = "$(Get-Date -Format 'HH:mm:ss.fff') PERFORMANCE: High handle count: $handleCount"
                            $warnings += $logEntry
                            Add-Content -Path $LogFile -Value $logEntry -Encoding UTF8
                        }
                    }
                    catch {
                        # Process access issues
                    }

                    # Check Windows Event Log for application errors
                    try {
                        $recentTime = (Get-Date).AddSeconds(-10)
                        $appErrors = Get-WinEvent -FilterHashtable @{
                            LogName   = 'Application'
                            Level     = 2  # Error level
                            StartTime = $recentTime
                        } -MaxEvents 10 -ErrorAction SilentlyContinue |
                        Where-Object {
                            $_.ProcessId -eq $PID -or
                            $_.Message -match "BusBuddy|dotnet|\.NET|CLR" -or
                            $_.ProviderName -match "\.NET|Application Error"
                        }

                        foreach ($eventError in $appErrors) {
                            $logEntry = "$(Get-Date -Format 'HH:mm:ss.fff') EVENT_LOG: [$($eventError.Id)] $($eventError.LevelDisplayName) - $($eventError.Message)"
                            $eventLogErrors += $logEntry
                            $errors += $logEntry
                            Add-Content -Path $LogFile -Value $logEntry -Encoding UTF8

                            if ($eventError.LevelDisplayName -eq "Error") {
                                $crashDetected = $true
                            }
                        }
                    }
                    catch {
                        # Event log access issues
                    }

                    Start-Sleep -Seconds 2  # Check every 2 seconds for faster response
                }
                catch {
                    # Monitoring error - log and continue
                    $logEntry = "$(Get-Date -Format 'HH:mm:ss.fff') MONITOR_ERROR: $($_.Exception.Message)"
                    $errors += $logEntry
                    Add-Content -Path $LogFile -Value $logEntry -Encoding UTF8
                    Start-Sleep -Seconds 2
                }
            }

            # Final process status
            if ($process) {
                $exitCode = $process.ExitCode
                $logEntry = "$(Get-Date -Format 'HH:mm:ss.fff') PROCESS_EXIT: Exit code $exitCode"
                Add-Content -Path $LogFile -Value $logEntry -Encoding UTF8

                # Analyze exit code
                $exitAnalysis = switch ($exitCode) {
                    0 { "Success" }
                    -1073741510 { "Forced termination (Ctrl+C or user close)" }
                    -1073741819 { "Access violation (memory error)" }
                    -1073740791 { "Invalid executable format" }
                    default { "Unknown error (code: $exitCode)" }
                }

                $analysisEntry = "$(Get-Date -Format 'HH:mm:ss.fff') EXIT_ANALYSIS: $exitAnalysis"
                Add-Content -Path $LogFile -Value $analysisEntry -Encoding UTF8
            }
        }
        catch {
            $errorEntry = "$(Get-Date -Format 'HH:mm:ss.fff') MONITORING_FATAL: $($_.Exception.Message)"
            $errors += $errorEntry
            Add-Content -Path $LogFile -Value $errorEntry -Encoding UTF8
        }

        return @{
            Errors         = $errors
            Warnings       = $warnings
            EventLogErrors = $eventLogErrors
            CrashDetected  = $crashDetected
            ExitCode       = if ($process) { $process.ExitCode } else { -1 }
            EndTime        = Get-Date
        }
    } -ArgumentList $Global:RuntimeErrorSession.ApplicationPID, $Global:RuntimeErrorSession.LogFile, $CaptureLevel, $CaptureStackTraces, $RealTimeStream, $Global:EnhancedMonitoring

    if ($Global:EnhancedMonitoring) {
        Write-Color -Text "✅ Comprehensive monitoring started ", "(background job active)" -Color Green, Cyan
    }
    else {
        Write-Host "✅ Comprehensive monitoring started" -ForegroundColor Green
    }
}

function Wait-ForApplicationWithRuntimeCapture {
    if ($Global:EnhancedMonitoring) {
        Write-Color -Text "⏳ Monitoring application runtime - close app when done..." -Color Yellow, White
    }
    else {
        Write-Host "⏳ Monitoring application runtime - close app when done..." -ForegroundColor Yellow
    }

    Write-Host "   • Use the application normally to trigger any errors" -ForegroundColor Gray
    Write-Host "   • Close the application window when finished" -ForegroundColor Gray
    Write-Host "   • Press Ctrl+C to force stop monitoring" -ForegroundColor Gray
    Write-Host ""

    # Use enhanced monitoring if available
    if ($Global:EnhancedMonitoring -and $CaptureLevel -eq 'All') {
        $monitoringSteps = @("Process Health", "Memory Usage", "Event Log", "Error Stream", "Performance")

        # Show monitoring progress
        $stepIndex = 0
        while (-not $Global:RuntimeErrorSession.Process.HasExited) {
            $currentStep = $monitoringSteps[$stepIndex % $monitoringSteps.Count]

            Write-InlineProgress -Activity "Runtime Monitoring: $currentStep" -PercentComplete (($stepIndex % 100) + 1) -ShowPercent

            Start-Sleep -Seconds 1
            $stepIndex++

            # Show periodic status
            if ($stepIndex % 30 -eq 0) {
                $elapsed = (Get-Date) - $Global:RuntimeErrorSession.StartTime
                if ($RealTimeStream) {
                    Write-Color -Text "📊 Status: ", "Running for $([int]$elapsed.TotalMinutes) minutes - Errors: $($Global:RuntimeErrorSession.ErrorCount) - Warnings: $($Global:RuntimeErrorSession.WarningCount)" -Color Cyan, White
                }
            }
        }

        Write-InlineProgress -Activity "Runtime Monitoring" -Completed
    }
    else {
        # Basic monitoring
        while (-not $Global:RuntimeErrorSession.Process.HasExited) {
            Start-Sleep -Seconds 1

            $elapsed = (Get-Date) - $Global:RuntimeErrorSession.StartTime
            if ($elapsed.TotalSeconds % 30 -eq 0) {
                Write-Host "⏱️ Runtime: $([int]$elapsed.TotalMinutes) minutes - Errors: $($Global:RuntimeErrorSession.ErrorCount)" -ForegroundColor DarkGray
            }
        }
    }

    # Capture final exit code
    $exitCode = $Global:RuntimeErrorSession.Process.ExitCode

    if ($Global:EnhancedMonitoring) {
        Write-Color -Text "✅ Application closed ", "(Exit Code: $exitCode)" -Color Green, $(if ($exitCode -eq 0) { "Green" } else { "Red" })
    }
    else {
        Write-Host "✅ Application closed (Exit Code: $exitCode)" -ForegroundColor $(if ($exitCode -eq 0) { "Green" } else { "Red" })
    }
}

function Stop-RuntimeErrorCapture {
    if ($Global:EnhancedMonitoring) {
        Write-Color -Text "🛑 Finalizing runtime error capture..." -Color Yellow, White
    }
    else {
        Write-Host "🛑 Finalizing runtime error capture..." -ForegroundColor Yellow
    }

    # Get results from monitoring job
    if ($Global:RuntimeErrorSession.MonitoringJob) {
        $results = Receive-Job -Job $Global:RuntimeErrorSession.MonitoringJob -Wait
        Remove-Job -Job $Global:RuntimeErrorSession.MonitoringJob -Force

        if ($results) {
            $Global:RuntimeErrorSession.Errors += $results.Errors
            $Global:RuntimeErrorSession.Warnings += $results.Warnings
            $Global:RuntimeErrorSession.EventLogErrors += $results.EventLogErrors
            $Global:RuntimeErrorSession.CrashDetected = $results.CrashDetected
            $Global:RuntimeErrorSession.ErrorCount = $Global:RuntimeErrorSession.Errors.Count
            $Global:RuntimeErrorSession.WarningCount = $Global:RuntimeErrorSession.Warnings.Count
        }
    }

    # Clean up process if still running
    if ($Global:RuntimeErrorSession.Process -and -not $Global:RuntimeErrorSession.Process.HasExited) {
        try {
            $Global:RuntimeErrorSession.Process.Kill()
            if ($Global:EnhancedMonitoring) {
                Write-Color -Text "🔪 Application process terminated" -Color Yellow, Yellow
            }
            else {
                Write-Host "🔪 Application process terminated" -ForegroundColor Yellow
            }
        }
        catch {
            if ($Global:EnhancedMonitoring) {
                Write-Color -Text "⚠️ Could not terminate process: ", $_.Exception.Message -Color Yellow, DarkYellow
            }
            else {
                Write-Host "⚠️ Could not terminate process: $($_.Exception.Message)" -ForegroundColor Yellow
            }
        }
    }

    if ($Global:EnhancedMonitoring) {
        Write-Color -Text "✅ Runtime error capture completed" -Color Green, Green
    }
    else {
        Write-Host "✅ Runtime error capture completed" -ForegroundColor Green
    }
}

function Show-RuntimeErrorResults {
    Write-Host ""
    Write-Host "📊 Runtime Error Capture Results" -ForegroundColor Cyan
    Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor DarkGray

    $duration = (Get-Date) - $Global:RuntimeErrorSession.StartTime
    $exitCode = if ($Global:RuntimeErrorSession.Process) { $Global:RuntimeErrorSession.Process.ExitCode } else { "Unknown" }

    if ($Global:EnhancedMonitoring) {
        Write-Color -Text "⏱️ Runtime Duration: ", "$([int]$duration.TotalMinutes) minutes, $([int]$duration.Seconds) seconds" -Color Gray, White
        Write-Color -Text "🔴 Runtime Errors: ", "$($Global:RuntimeErrorSession.ErrorCount)" -Color $(if ($Global:RuntimeErrorSession.ErrorCount -gt 0) { "Red" } else { "Green" }), $(if ($Global:RuntimeErrorSession.ErrorCount -gt 0) { "Red" } else { "Green" })
        Write-Color -Text "⚠️ Warnings: ", "$($Global:RuntimeErrorSession.WarningCount)" -Color $(if ($Global:RuntimeErrorSession.WarningCount -gt 0) { "Yellow" } else { "Green" }), $(if ($Global:RuntimeErrorSession.WarningCount -gt 0) { "Yellow" } else { "Green" })
        Write-Color -Text "🎨 WPF Errors: ", "$($Global:RuntimeErrorSession.WPFErrors.Count)" -Color $(if ($Global:RuntimeErrorSession.WPFErrors.Count -gt 0) { "Magenta" } else { "Green" }), $(if ($Global:RuntimeErrorSession.WPFErrors.Count -gt 0) { "Magenta" } else { "Green" })
        Write-Color -Text "📋 Event Log Errors: ", "$($Global:RuntimeErrorSession.EventLogErrors.Count)" -Color $(if ($Global:RuntimeErrorSession.EventLogErrors.Count -gt 0) { "Red" } else { "Green" }), $(if ($Global:RuntimeErrorSession.EventLogErrors.Count -gt 0) { "Red" } else { "Green" })
        Write-Color -Text "💥 Crash Detected: ", $(if ($Global:RuntimeErrorSession.CrashDetected) { "Yes" } else { "No" }) -Color $(if ($Global:RuntimeErrorSession.CrashDetected) { "Red" } else { "Green" }), $(if ($Global:RuntimeErrorSession.CrashDetected) { "Red" } else { "Green" })
        Write-Color -Text "🔢 Exit Code: ", "$exitCode" -Color Gray, $(if ($exitCode -eq 0) { "Green" } elseif ($exitCode -eq -1073741510) { "Yellow" } else { "Red" })
    }
    else {
        Write-Host "⏱️ Runtime Duration: $([int]$duration.TotalMinutes) minutes, $([int]$duration.Seconds) seconds" -ForegroundColor Gray
        Write-Host "🔴 Runtime Errors: $($Global:RuntimeErrorSession.ErrorCount)" -ForegroundColor $(if ($Global:RuntimeErrorSession.ErrorCount -gt 0) { "Red" } else { "Green" })
        Write-Host "⚠️ Warnings: $($Global:RuntimeErrorSession.WarningCount)" -ForegroundColor $(if ($Global:RuntimeErrorSession.WarningCount -gt 0) { "Yellow" } else { "Green" })
        Write-Host "💥 Crash Detected: $(if ($Global:RuntimeErrorSession.CrashDetected) { 'Yes' } else { 'No' })" -ForegroundColor $(if ($Global:RuntimeErrorSession.CrashDetected) { "Red" } else { "Green" })
        Write-Host "🔢 Exit Code: $exitCode" -ForegroundColor $(if ($exitCode -eq 0) { "Green" } elseif ($exitCode -eq -1073741510) { "Yellow" } else { "Red" })
    }

    if ($Global:RuntimeErrorSession.LogFile -and (Test-Path $Global:RuntimeErrorSession.LogFile)) {
        if ($Global:EnhancedMonitoring) {
            Write-Color -Text "📄 Log File: ", $Global:RuntimeErrorSession.LogFile -Color Gray, Gray
            Write-Color -Text "📏 Log Size: ", "$([math]::Round((Get-Item $Global:RuntimeErrorSession.LogFile).Length / 1KB, 2)) KB" -Color Gray, Gray
        }
        else {
            Write-Host "📄 Log File: $($Global:RuntimeErrorSession.LogFile)" -ForegroundColor Gray
            Write-Host "📏 Log Size: $([math]::Round((Get-Item $Global:RuntimeErrorSession.LogFile).Length / 1KB, 2)) KB" -ForegroundColor Gray
        }
    }

    Write-Host ""

    # Show recent runtime errors
    if ($Global:RuntimeErrorSession.ErrorCount -gt 0) {
        Write-Host "🔴 Recent Runtime Errors:" -ForegroundColor Red
        $Global:RuntimeErrorSession.Errors | Select-Object -Last 5 | ForEach-Object {
            Write-Host "   $_" -ForegroundColor DarkRed
        }
        Write-Host ""
    }

    # Show WPF-specific errors
    if ($Global:RuntimeErrorSession.WPFErrors.Count -gt 0) {
        Write-Host "🎨 WPF-Specific Errors:" -ForegroundColor Magenta
        $Global:RuntimeErrorSession.WPFErrors | Select-Object -Last 3 | ForEach-Object {
            Write-Host "   $_" -ForegroundColor DarkMagenta
        }
        Write-Host ""
    }

    # Show event log errors
    if ($Global:RuntimeErrorSession.EventLogErrors.Count -gt 0) {
        Write-Host "📋 Windows Event Log Errors:" -ForegroundColor Red
        $Global:RuntimeErrorSession.EventLogErrors | Select-Object -Last 3 | ForEach-Object {
            Write-Host "   $_" -ForegroundColor DarkRed
        }
        Write-Host ""
    }

    # Provide analysis and recommendations
    Write-Host "🔧 Analysis & Recommendations:" -ForegroundColor Cyan

    if ($exitCode -eq -1073741510) {
        Write-Host "   • Exit code -1073741510 indicates forced termination (user closed or Ctrl+C)" -ForegroundColor Yellow
        Write-Host "   • This is normal when closing the application manually" -ForegroundColor Gray
    }
    elseif ($exitCode -ne 0 -and $exitCode -ne "Unknown") {
        Write-Host "   • Non-zero exit code indicates an application error or crash" -ForegroundColor Red
        Write-Host "   • Check Windows Event Log for additional details" -ForegroundColor Gray
        Write-Host "   • Review the captured error logs for stack traces" -ForegroundColor Gray
    }

    if ($Global:RuntimeErrorSession.ErrorCount -eq 0 -and $Global:RuntimeErrorSession.WarningCount -eq 0) {
        Write-Host "   ✅ No runtime errors detected - application ran cleanly!" -ForegroundColor Green
    }
    elseif ($Global:RuntimeErrorSession.ErrorCount -gt 0) {
        Write-Host "   • Review captured errors for debugging opportunities" -ForegroundColor Yellow
        Write-Host "   • Focus on the most recent errors for current issues" -ForegroundColor Gray
    }

    if ($Global:RuntimeErrorSession.WPFErrors.Count -gt 0) {
        Write-Host "   • WPF binding/dependency errors detected - check XAML bindings" -ForegroundColor Magenta
    }
}

# ============================================================================
# MAIN EXECUTION WORKFLOW
# ============================================================================

function Start-EnhancedRuntimeErrorCapture {
    try {
        Initialize-RuntimeErrorCapture -LogDirectory $LogDirectory

        $appStarted = Start-ApplicationWithRuntimeMonitoring
        if (-not $appStarted) {
            throw "Failed to start BusBuddy application with runtime monitoring"
        }

        Wait-ForApplicationWithRuntimeCapture

        Stop-RuntimeErrorCapture
        Show-RuntimeErrorResults

        if ($Global:EnhancedMonitoring) {
            Write-Color -Text "🎉 Enhanced runtime error capture completed successfully!" -Color Green, Green
        }
        else {
            Write-Host "🎉 Runtime error capture completed successfully!" -ForegroundColor Green
        }

    }
    catch {
        Write-Host ""
        if ($Global:EnhancedMonitoring) {
            Write-Color -Text "❌ Runtime error capture failed: ", $_.Exception.Message -Color Red, DarkRed
        }
        else {
            Write-Host "❌ Runtime error capture failed: $($_.Exception.Message)" -ForegroundColor Red
        }

        if ($Global:RuntimeErrorSession.LogFile) {
            Write-Host "💡 Check the log file for details: $($Global:RuntimeErrorSession.LogFile)" -ForegroundColor Yellow
        }

        # Clean up on error
        if ($Global:RuntimeErrorSession.MonitoringJob) {
            Remove-Job -Job $Global:RuntimeErrorSession.MonitoringJob -Force
        }

        exit 1
    }
}

# Ensure we're in the project root
if (-not (Test-Path "BusBuddy.sln")) {
    Write-Host "❌ Must be run from BusBuddy project root directory" -ForegroundColor Red
    exit 1
}

# Start the enhanced runtime error capture
Start-EnhancedRuntimeErrorCapture
