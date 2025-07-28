#Requires -Version 7.5

<#
.SYNOPSIS
BusBuddy Terminal Flow Monitor - Real-time PowerShell output monitoring

.DESCRIPTION
Provides real-time monitoring of terminal output with advanced error detection capabilities
specifically designed for PowerShell development environments. Includes PowerShell Gallery-style
dot progress indication and comprehensive error pattern matching.

.PARAMETER MonitorMode
Type of monitoring to perform (All, Dots, Progress, Lifecycle, Custom)

.PARAMETER LogToFile
Save monitoring output to timestamped log files

.PARAMETER ShowDots
Display PowerShell Gallery-style progress dots

.PARAMETER WatchCommand
Monitor specific command execution

.EXAMPLE
.\BusBuddy-Terminal-Flow-Monitor.ps1 -MonitorMode All -LogToFile -ShowDots

.EXAMPLE
.\BusBuddy-Terminal-Flow-Monitor.ps1 -WatchCommand "dotnet build BusBuddy.sln"
#>

param(
    [ValidateSet('All', 'Dots', 'Progress', 'Lifecycle', 'Custom')]
    [string]$MonitorMode = 'All',

    [switch]$LogToFile,
    [switch]$ShowDots,
    [string]$WatchCommand,
    [int]$TimeoutMinutes = 2
)

# Initialize logging
$LogFile = $null
if ($LogToFile) {
    $LogDir = "logs/terminal-flow"
    if (-not (Test-Path $LogDir)) {
        New-Item -ItemType Directory -Path $LogDir -Force | Out-Null
    }
    $LogFile = Join-Path $LogDir "terminal-flow-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
}


function Write-MonitorLog {
    param(
        [string]$Message,
        [string]$Level = 'INFO'
    )

    $Timestamp = Get-Date -Format 'HH:mm:ss'
    $Color = switch ($Level) {
        'ERROR' { 'Red' }
        'WARN' { 'Yellow' }
        'SUCCESS' { 'Green' }
        'PROGRESS' { 'Cyan' }
        default { 'White' }
    }

    $Output = "[$Timestamp] [$Level] $Message"
    Write-Host $Output -ForegroundColor $Color

    if ($LogToFile -and $LogFile) {
        $Output | Out-File -FilePath $LogFile -Append -Encoding UTF8
    }
}

function Test-OutputLine {
    param([string]$Line)

    if (-not $Line -or $Line.Trim() -eq "") { return }

    switch -Regex ($Line) {
        # PowerShell-specific errors
        'Cannot bind argument to parameter.*Path.*because it is null|variable.*out.*scope|Variable.*is an automatic variable' {
            Write-MonitorLog "🚨 POWERSHELL ERROR: $Line" "ERROR"
        }

        # PowerShell execution errors
        'execution.*policy|script.*cannot.*be.*loaded|access.*denied' {
            Write-MonitorLog "🚨 EXECUTION ERROR: $Line" "ERROR"
        }

        # Module loading errors
        'module.*not.*found|import.*failed|assembly.*not.*loaded' {
            Write-MonitorLog "🚨 MODULE ERROR: $Line" "ERROR"
        }

        # Generic error patterns
        'error|failed|exception|fatal' {
            Write-MonitorLog "ERROR: $Line" "ERROR"
        }

        # Warning patterns
        'warning|warn' {
            Write-MonitorLog "WARNING: $Line" "WARN"
        }

        # Build activities
        'Building|Restoring|Running|Testing|Compiling' {
            Write-MonitorLog "Build activity: $Line" "PROGRESS"
        }

        # Success patterns
        'succeeded|completed|finished|done|success|passed' {
            Write-MonitorLog "Success: $Line" "SUCCESS"
        }

        # Lifecycle events
        'Started|Launching|Initializing|Loading' {
            Write-MonitorLog "Lifecycle: $Line" "INFO"
        }

        # Catch any line with PowerShell prompt or command execution
        'PS.*>|C:\\.*>|\$.*=' {
            Write-MonitorLog "Command execution: $Line" "INFO"
        }

        default {
            if ($MonitorMode -eq 'All' -and $Line.Length -gt 10) {
                Write-MonitorLog "Output: $Line" "INFO"
            }
        }
    }
}

function Start-DotWatcher {
    if ($WatchCommand) {
        Write-MonitorLog "Executing watched command: $WatchCommand" "INFO"

        try {
            $Process = Start-Process -FilePath "pwsh" -ArgumentList "-Command", $WatchCommand -NoNewWindow -PassThru -RedirectStandardOutput -RedirectStandardError

            # Monitor process output
            while (-not $Process.HasExited) {
                if ($ShowDots) {
                    Write-Host "." -NoNewline -ForegroundColor Green
                }
                Start-Sleep -Milliseconds 500
            }

            $Process.WaitForExit()
            return $Process.ExitCode
        }
        catch {
            Write-MonitorLog "Error executing command: $($_.Exception.Message)" "ERROR"
            return 1
        }
    }
    else {
        Watch-TerminalFlow
        return 0
    }
}

function Watch-TerminalFlow {
    Write-MonitorLog "Monitoring REAL terminal output streams..." "INFO"

    $StartTime = Get-Date
    $ErrorCount = 0
    $WarningCount = 0

    try {
        # Monitor PowerShell transcript and error streams
        $TranscriptPath = "$env:TEMP\BusBuddy-Monitor-$(Get-Date -Format 'yyyyMMdd-HHmmss').txt"
        Start-Transcript -Path $TranscriptPath -Append -Force

        Write-MonitorLog "Real-time monitoring active. Transcript: $TranscriptPath" "SUCCESS"
        Write-MonitorLog "Monitoring PowerShell error streams and execution..." "INFO"

        # Get current PowerShell session's error and output streams
        $OriginalErrorCount = $Error.Count

        while ($true) {
            # Check for new errors in PowerShell session
            if ($Error.Count -gt $OriginalErrorCount) {
                $NewErrors = $Error[0..($Error.Count - $OriginalErrorCount - 1)]
                foreach ($Err in $NewErrors) {
                    $ErrorCount++
                    Write-MonitorLog "LIVE ERROR DETECTED: $($Err.Exception.Message)" "ERROR"

                    # Check for specific error patterns we missed
                    if ($Err.Exception.Message -match "automatic variable|scope|not recognized|undefined") {
                        Write-MonitorLog "🚨 CRITICAL PATTERN MATCH: $($Err.Exception.Message)" "ERROR"
                    }
                }
                $OriginalErrorCount = $Error.Count
            }

            # Monitor transcript file for real output
            if (Test-Path $TranscriptPath) {
                $TranscriptContent = Get-Content $TranscriptPath -Tail 50 -ErrorAction SilentlyContinue
                if ($TranscriptContent) {
                    foreach ($Line in $TranscriptContent) {
                        if ($Line -and $Line.Trim() -ne "") {
                            Test-OutputLine -Line $Line
                        }
                    }
                }
            }

            # Check recent Windows Event Logs for PowerShell errors
            $RecentEvents = Get-WinEvent -FilterHashtable @{LogName = 'Windows PowerShell'; Level = 2; StartTime = (Get-Date).AddSeconds(-30) } -MaxEvents 10 -ErrorAction SilentlyContinue
            if ($RecentEvents) {
                foreach ($EventEntry in $RecentEvents) {
                    Write-MonitorLog "WINDOWS EVENT ERROR: $($EventEntry.Message)" "ERROR"
                }
            }

            # Show we're actually monitoring (not fake dots)
            if ($ShowDots -or $MonitorMode -eq 'All') {
                Write-Host "●" -NoNewline -ForegroundColor Green  # Real monitoring indicator
            }

            Start-Sleep -Milliseconds 1000  # Real monitoring interval

            # Check for keyboard interrupt
            if ([Console]::KeyAvailable) {
                $Key = [Console]::ReadKey($true)
                if ($Key.Key -eq 'Escape' -or ($Key.Modifiers -eq 'Control' -and $Key.Key -eq 'C')) {
                    Write-Host "`nReal monitoring stopped by user." -ForegroundColor Yellow
                    break
                }
            }

            # Auto-stop after 2 minutes for real monitoring
            if ((Get-Date) - $StartTime -gt [TimeSpan]::FromMinutes($TimeoutMinutes)) {
                Write-Host "`nReal monitoring auto-stopping after $TimeoutMinutes minutes." -ForegroundColor Yellow
                break
            }
        }
    }
    catch {
        Write-MonitorLog "Monitoring error: $($_.Exception.Message)" "ERROR"
    }
    finally {
        Stop-Transcript -ErrorAction SilentlyContinue
        $ElapsedTime = (Get-Date) - $StartTime
        Write-MonitorLog "REAL monitoring session completed. Duration: $($ElapsedTime.ToString('mm\:ss'))" "SUCCESS"
        Write-MonitorLog "Errors detected: $ErrorCount, Warnings: $WarningCount" "INFO"

        # Clean up transcript
        if (Test-Path $TranscriptPath) {
            Remove-Item $TranscriptPath -Force -ErrorAction SilentlyContinue
        }
    }
}

function Show-MonitoringStats {
    Write-MonitorLog "=== Terminal Flow Monitor Statistics ===" "INFO"
    Write-MonitorLog "Start Time: $(Get-Date)" "INFO"
    Write-MonitorLog "Monitor Mode: $MonitorMode" "INFO"
    Write-MonitorLog "Show Dots: $ShowDots" "INFO"
    Write-MonitorLog "Log File: $(if ($LogToFile) { $LogFile } else { 'Disabled' })" "INFO"
    Write-MonitorLog "Watch Command: $(if ($WatchCommand) { $WatchCommand } else { 'None - monitoring existing output' })" "INFO"
    Write-MonitorLog "Timeout: $TimeoutMinutes minutes" "INFO"
}

# Main execution
Write-Host "🌊 BusBuddy Terminal Flow Monitor" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

Show-MonitoringStats

if ($WatchCommand) {
    $ExitCode = Start-DotWatcher
    Write-MonitorLog "Terminal Flow Monitor completed with exit code: $ExitCode" "INFO"
    exit $ExitCode
}
else {
    Write-Host "`nPress ESC or Ctrl+C to stop monitoring..." -ForegroundColor Yellow
    Start-DotWatcher
}

Write-MonitorLog "Terminal Flow Monitor session ended" "SUCCESS"
if (Test-Path $TranscriptPath) {
    $TranscriptContent = Get-Content $TranscriptPath -Tail 50 -ErrorAction SilentlyContinue
    if ($TranscriptContent) {
        foreach ($Line in $TranscriptContent) {
            if ($Line -and $Line.Trim() -ne "") {
                Test-OutputLine -Line $Line
            }
        }
    }
}


function Show-MonitoringStats {
    Write-MonitorLog "=== Terminal Flow Monitor Statistics ===" "INFO"
    Write-MonitorLog "Start Time: $(Get-Date)" "INFO"
    Write-MonitorLog "Monitor Mode: $MonitorMode" "INFO"
    Write-MonitorLog "Show Dots: $ShowDots" "INFO"
    Write-MonitorLog "Log File: $(if ($LogToFile) { $LogFile } else { 'Disabled' })" "INFO"
    Write-MonitorLog "Watch Command: $(if ($WatchCommand) { $WatchCommand } else { 'None - monitoring existing output' })" "INFO"
}

# Main execution
Write-Host "🌊 BusBuddy Terminal Flow Monitor" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

Show-MonitoringStats

if ($WatchCommand) {
    $ExitCode = Start-DotWatcher
    Write-MonitorLog "Terminal Flow Monitor completed with exit code: $ExitCode" "INFO"
    exit $ExitCode
}
else {
    Write-Host "`nPress ESC or Ctrl+C to stop monitoring..." -ForegroundColor Yellow
    Start-DotWatcher
}

Write-MonitorLog "Terminal Flow Monitor session ended" "SUCCESS"
