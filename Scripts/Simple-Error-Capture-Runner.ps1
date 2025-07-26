#Requires -Version 7.5
<#
.SYNOPSIS
    Simple Error Capture and Application Runner for BusBuddy

.DESCRIPTION
    Streamlined version that implements the exact workflow you described:
    1. Start error monitoring
    2. Launch BusBuddy WPF application
    3. Wait for user to close app window (X button) or click Continue
    4. Stop monitoring and show results

.NOTES
    Phase 1 Focus: Get it working simply and effectively
#>

param(
    [switch]$ShowRealTime,
    [string]$LogPath = "logs\error-capture"
)

# ============================================================================
# SIMPLE SETUP
# ============================================================================

$ErrorActionPreference = 'Continue'
$script:AppProcess = $null
$script:ErrorLog = $null
$script:StartTime = Get-Date
$script:Errors = @()
$script:IsMonitoring = $false

Write-Host ""
Write-Host "🚌 BusBuddy Simple Error Capture & Run" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════" -ForegroundColor DarkGray
Write-Host ""

# ============================================================================
# CORE FUNCTIONS
# ============================================================================

function Start-ErrorMonitoring {
    Write-Host "📊 Phase 1: Starting error monitoring..." -ForegroundColor Yellow

    # Create log directory and file
    if (-not (Test-Path $LogPath)) {
        New-Item -ItemType Directory -Path $LogPath -Force | Out-Null
    }

    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $script:ErrorLog = Join-Path $LogPath "busbuddy-capture-$timestamp.log"

    # Initialize log
    $header = @"
BusBuddy Error Capture Session
Started: $(Get-Date)
========================================

"@
    Set-Content -Path $script:ErrorLog -Value $header -Encoding UTF8

    $script:IsMonitoring = $true
    Write-Host "✅ Error monitoring started" -ForegroundColor Green
    Write-Host "📄 Log file: $script:ErrorLog" -ForegroundColor Gray
    Write-Host ""
}

function Start-Application {
    Write-Host "🚀 Phase 2: Launching BusBuddy application..." -ForegroundColor Yellow

    try {
        # Verify project exists
        $wpfProject = "BusBuddy.WPF\BusBuddy.WPF.csproj"
        if (-not (Test-Path $wpfProject)) {
            throw "BusBuddy.WPF project not found"
        }

        # Start application process
        $startInfo = New-Object System.Diagnostics.ProcessStartInfo
        $startInfo.FileName = "dotnet"
        $startInfo.Arguments = "run --project `"$wpfProject`""
        $startInfo.UseShellExecute = $false
        $startInfo.RedirectStandardOutput = $true
        $startInfo.RedirectStandardError = $true
        $startInfo.CreateNoWindow = $false
        $startInfo.WorkingDirectory = (Get-Location).Path

        $script:AppProcess = [System.Diagnostics.Process]::Start($startInfo)

        Write-Host "✅ Application launched (PID: $($script:AppProcess.Id))" -ForegroundColor Green
        Write-Host ""

        # Start background monitoring of process output
        Start-BackgroundErrorCapture

        return $true
    }
    catch {
        Write-Host "❌ Failed to launch application: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Start-BackgroundErrorCapture {
    # Create background runspace for monitoring
    $runspace = [runspacefactory]::CreateRunspace()
    $runspace.Open()

    $powershell = [powershell]::Create()
    $powershell.Runspace = $runspace

    $scriptBlock = {
        param($Process, $LogFile, $ShowRealTime)

        $localErrors = @()

        while (-not $Process.HasExited) {
            try {
                # Check stderr
                if (-not $Process.StandardError.EndOfStream) {
                    $errorLine = $Process.StandardError.ReadLine()
                    if ($errorLine) {
                        $logEntry = "$(Get-Date -Format 'HH:mm:ss') ERROR: $errorLine"
                        $localErrors += $logEntry
                        Add-Content -Path $LogFile -Value $logEntry

                        if ($ShowRealTime) {
                            Write-Host "🔴 $logEntry" -ForegroundColor Red
                        }
                    }
                }

                # Check stdout for warnings/errors
                if (-not $Process.StandardOutput.EndOfStream) {
                    $outputLine = $Process.StandardOutput.ReadLine()
                    if ($outputLine -and ($outputLine -match "error|exception|fail|crash" -or $outputLine -match "warning|warn")) {
                        $logEntry = "$(Get-Date -Format 'HH:mm:ss') OUTPUT: $outputLine"
                        $localErrors += $logEntry
                        Add-Content -Path $LogFile -Value $logEntry

                        if ($ShowRealTime) {
                            $color = if ($outputLine -match "warning|warn") { "Yellow" } else { "Red" }
                            Write-Host "⚠️ $logEntry" -ForegroundColor $color
                        }
                    }
                }

                Start-Sleep -Milliseconds 200
            }
            catch {
                # Process ended or stream closed
                break
            }
        }

        return $localErrors
    }

    $powershell.AddScript($scriptBlock)
    $powershell.AddArgument($script:AppProcess)
    $powershell.AddArgument($script:ErrorLog)
    $powershell.AddArgument($ShowRealTime)

    $script:CaptureJob = $powershell.BeginInvoke()
    $script:PowerShellInstance = $powershell
}

function Wait-ForApplicationEnd {
    Write-Host "⏳ Phase 3: Monitoring application..." -ForegroundColor Yellow
    Write-Host "   • Application is running - use the app normally" -ForegroundColor Gray
    Write-Host "   • Close the application window (X button) when done" -ForegroundColor Gray
    Write-Host "   • Or click Continue on any error dialogs" -ForegroundColor Gray
    Write-Host "   • Press Ctrl+C here to force stop monitoring" -ForegroundColor Gray
    Write-Host ""

    $lastStatusTime = Get-Date

    try {
        while ($script:IsMonitoring) {
            # Check if process has exited
            if ($script:AppProcess.HasExited) {
                Write-Host "✅ Application has closed normally" -ForegroundColor Green
                Write-Host "📊 Exit code: $($script:AppProcess.ExitCode)" -ForegroundColor Gray
                break
            }

            # Show periodic status (every 30 seconds)
            if ((Get-Date) - $lastStatusTime -gt [TimeSpan]::FromSeconds(30)) {
                $elapsed = (Get-Date) - $script:StartTime
                Write-Host "⏱️ Running for $([int]$elapsed.TotalMinutes) minutes..." -ForegroundColor DarkGray
                $lastStatusTime = Get-Date
            }

            Start-Sleep -Seconds 2
        }
    }
    catch [System.OperationCanceledException] {
        Write-Host "🛑 Monitoring stopped by user (Ctrl+C)" -ForegroundColor Yellow
    }
    catch {
        Write-Host "⚠️ Monitoring interrupted: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

function Stop-ErrorMonitoring {
    Write-Host ""
    Write-Host "🛑 Phase 4: Stopping error monitoring..." -ForegroundColor Yellow

    $script:IsMonitoring = $false

    # Get results from background capture
    if ($script:PowerShellInstance -and $script:CaptureJob) {
        try {
            $capturedErrors = $script:PowerShellInstance.EndInvoke($script:CaptureJob)
            if ($capturedErrors) {
                $script:Errors += $capturedErrors
            }
            $script:PowerShellInstance.Dispose()
        }
        catch {
            Write-Host "⚠️ Error retrieving capture results: $($_.Exception.Message)" -ForegroundColor Yellow
        }
    }

    # Clean up process if still running
    if ($script:AppProcess -and -not $script:AppProcess.HasExited) {
        try {
            Write-Host "🔪 Terminating application process..." -ForegroundColor Yellow
            $script:AppProcess.Kill()
            $script:AppProcess.WaitForExit(5000) # Wait up to 5 seconds
        }
        catch {
            Write-Host "⚠️ Could not terminate process cleanly" -ForegroundColor Yellow
        }
    }

    Write-Host "✅ Error monitoring stopped" -ForegroundColor Green
}

function Show-Results {
    Write-Host ""
    Write-Host "📊 Error Capture Results" -ForegroundColor Cyan
    Write-Host "════════════════════════" -ForegroundColor DarkGray

    $duration = (Get-Date) - $script:StartTime
    $minutes = [int]$duration.TotalMinutes
    $seconds = [int]$duration.Seconds

    Write-Host "⏱️ Session Duration: $minutes minutes, $seconds seconds" -ForegroundColor Gray
    Write-Host "🔴 Errors/Issues Found: $($script:Errors.Count)" -ForegroundColor $(if ($script:Errors.Count -gt 0) { "Red" } else { "Green" })

    if (Test-Path $script:ErrorLog) {
        $logSize = [math]::Round((Get-Item $script:ErrorLog).Length / 1KB, 1)
        Write-Host "📄 Log File: $script:ErrorLog" -ForegroundColor Gray
        Write-Host "📏 Log Size: $logSize KB" -ForegroundColor Gray
    }

    Write-Host ""

    if ($script:Errors.Count -eq 0) {
        Write-Host "🎉 No errors detected - application ran successfully!" -ForegroundColor Green
        Write-Host "✅ BusBuddy appears to be working correctly" -ForegroundColor Green
    }
    else {
        Write-Host "⚠️ Issues detected during application run:" -ForegroundColor Yellow

        # Show the most recent/important errors
        $script:Errors | Select-Object -Last 5 | ForEach-Object {
            Write-Host "   $($_)" -ForegroundColor Red
        }

        Write-Host ""
        Write-Host "💡 Recommendations:" -ForegroundColor Cyan
        Write-Host "   • Check the full log file for detailed analysis" -ForegroundColor Gray
        Write-Host "   • Review any error dialogs that appeared in the app" -ForegroundColor Gray
        Write-Host "   • Verify all dependencies and configuration files" -ForegroundColor Gray
    }

    Write-Host ""
    Write-Host "📁 Next steps:" -ForegroundColor Cyan
    Write-Host "   • Review log file: $script:ErrorLog" -ForegroundColor Gray
    Write-Host "   • Run again to test fixes" -ForegroundColor Gray
    Write-Host ""
}

# ============================================================================
# MAIN WORKFLOW
# ============================================================================

try {
    # Ensure we're in the right directory
    if (-not (Test-Path "BusBuddy.sln")) {
        Write-Host "❌ Must be run from BusBuddy project root directory" -ForegroundColor Red
        Write-Host "💡 Navigate to the folder containing BusBuddy.sln" -ForegroundColor Yellow
        exit 1
    }

    # Execute the 4-phase workflow
    Start-ErrorMonitoring

    $appLaunched = Start-Application
    if (-not $appLaunched) {
        throw "Application failed to launch"
    }

    Wait-ForApplicationEnd

    Stop-ErrorMonitoring

    Show-Results

    Write-Host "🎯 Error capture session completed!" -ForegroundColor Green
}
catch {
    Write-Host ""
    Write-Host "❌ Error capture session failed: $($_.Exception.Message)" -ForegroundColor Red

    if ($script:IsMonitoring) {
        Stop-ErrorMonitoring
    }

    exit 1
}
finally {
    # Cleanup
    if ($script:PowerShellInstance) {
        $script:PowerShellInstance.Dispose()
    }
}
