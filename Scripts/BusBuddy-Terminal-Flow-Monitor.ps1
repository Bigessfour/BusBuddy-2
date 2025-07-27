#Requires -Version 7.5
<#
.SYNOPSIS
    BusBuddy Terminal Flow Monitor - PowerShell Gallery-style dot watcher

.DESCRIPTION
    Monitors terminal output for dots, progress indicators, and application lifecycle events.
    Provides real-time feedback during builds, tests, and application startup.

.PARAMETER MonitorMode
    What to monitor: All, Dots, Progress, Lifecycle, or Custom

.PARAMETER LogToFile
    Enable logging to timestamped file in logs/terminal-flow/

.PARAMETER ShowDots
    Display dots for progress indication

.PARAMETER WatchCommand
    Specific command to monitor (optional)

.EXAMPLE
    .\BusBuddy-Terminal-Flow-Monitor.ps1 -MonitorMode All -LogToFile -ShowDots

.EXAMPLE
    .\BusBuddy-Terminal-Flow-Monitor.ps1 -MonitorMode Dots -WatchCommand "dotnet build"
#>
param(
    [ValidateSet('All', 'Dots', 'Progress', 'Lifecycle', 'Custom')]
    [string]$MonitorMode = 'All',

    [switch]$LogToFile,

    [switch]$ShowDots,

    [string]$WatchCommand = $null
)

# Initialize logging
$LogDir = "logs/terminal-flow"
if (-not (Test-Path $LogDir)) {
    New-Item -ItemType Directory -Path $LogDir -Force | Out-Null
}

$Timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$LogFile = "$LogDir/terminal-flow-$Timestamp.log"

function Write-MonitorLog {
    param(
        [string]$Message,
        [string]$Level = "INFO"
    )

    $LogEntry = "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') [$Level] $Message"

    if ($LogToFile) {
        $LogEntry | Out-File -FilePath $LogFile -Append -Encoding UTF8
    }

    # Color coding for console output
    switch ($Level) {
        "ERROR" { Write-Host $LogEntry -ForegroundColor Red }
        "WARN" { Write-Host $LogEntry -ForegroundColor Yellow }
        "SUCCESS" { Write-Host $LogEntry -ForegroundColor Green }
        "PROGRESS" { Write-Host $LogEntry -ForegroundColor Cyan }
        default { Write-Host $LogEntry -ForegroundColor White }
    }
}

function Start-DotWatcher {
    param(
        [string]$ProcessName = $null
    )

    Write-MonitorLog "🌊 Starting Terminal Flow Monitor - Dot Watcher Mode" "SUCCESS"
    Write-MonitorLog "Monitor Mode: $MonitorMode" "INFO"

    if ($WatchCommand) {
        Write-MonitorLog "Watching command: $WatchCommand" "INFO"

        # Parse command and arguments
        $CommandParts = $WatchCommand -split ' ', 2
        $Command = $CommandParts[0]
        $Arguments = if ($CommandParts.Length -gt 1) { $CommandParts[1] } else { "" }

        # Start the process and monitor output
        $ProcessInfo = New-Object System.Diagnostics.ProcessStartInfo
        $ProcessInfo.FileName = $Command
        $ProcessInfo.Arguments = $Arguments
        $ProcessInfo.RedirectStandardOutput = $true
        $ProcessInfo.RedirectStandardError = $true
        $ProcessInfo.UseShellExecute = $false
        $ProcessInfo.CreateNoWindow = $true

        $Process = New-Object System.Diagnostics.Process
        $Process.StartInfo = $ProcessInfo

        # Event handlers for output
        $OutputHandler = {
            param($ProcessSource, $EventData)
            if ($EventData.Data) {
                Test-OutputLine -Line $EventData.Data
            }
        }

        $ErrorHandler = {
            param($ProcessSource, $EventData)
            if ($EventData.Data) {
                # Process error lines through our pattern detection too
                Test-OutputLine -Line $EventData.Data
                Write-MonitorLog "STDERR: $($EventData.Data)" "ERROR"
            }
        }

        Register-ObjectEvent -InputObject $Process -EventName OutputDataReceived -Action $OutputHandler | Out-Null
        Register-ObjectEvent -InputObject $Process -EventName ErrorDataReceived -Action $ErrorHandler | Out-Null

        try {
            $Process.Start() | Out-Null
            $Process.BeginOutputReadLine()
            $Process.BeginErrorReadLine()

            Write-MonitorLog "Process started (PID: $($Process.Id))" "SUCCESS"

            # Wait for process to complete
            $Process.WaitForExit()

            Write-MonitorLog "Process completed with exit code: $($Process.ExitCode)" "INFO"
            return $Process.ExitCode
        }
        finally {
            $Process.Dispose()
        }
    }
    else {
        # Monitor existing terminal output
        Write-MonitorLog "Monitoring existing terminal output..." "INFO"
        Watch-TerminalFlow
    }
}

function Test-OutputLine {
    param([string]$Line)

    if ([string]::IsNullOrWhiteSpace($Line)) { return }

    # Enhanced error detection - catch EVERYTHING
    switch -Regex ($Line) {
        '^\s*\.\s*$' {
            if ($ShowDots -or $MonitorMode -eq 'All' -or $MonitorMode -eq 'Dots') {
                Write-Host "." -NoNewline -ForegroundColor Green
            }
            Write-MonitorLog "Dot detected" "PROGRESS"
        }

        # PowerShell scope and variable errors (the ones we missed!)
        '\$PowerShellEnvPath.*not.*recognized|\$.*not.*defined|variable.*out.*scope' {
            Write-MonitorLog "🚨 SCOPE ERROR DETECTED: $Line" "ERROR"
        }

        # EXACT PATTERNS FROM THE SCREENSHOT - NULL PATH ERRORS
        'Cannot bind argument to parameter.*Path.*because it is null' {
            Write-MonitorLog "🚨 NULL PATH PARAMETER ERROR: $Line" "ERROR"
        }

        'Value cannot be null.*Parameter.*provided Path argument was null' {
            Write-MonitorLog "🚨 NULL PATH VALUE ERROR: $Line" "ERROR"
        }

        '\$BusBuddyEnvPath.*\$ConfigPath.*\$BusBuddyEnvPath' {
            Write-MonitorLog "🚨 BUSBUDDYENVPATH VARIABLE ERROR: $Line" "ERROR"
        }

        'Test-Path.*ConfigPath.*cannot bind.*null' {
            Write-MonitorLog "🚨 TEST-PATH NULL ERROR: $Line" "ERROR"
        }

        'Join-Path.*BusBuddyEnvPath.*cannot bind.*null' {
            Write-MonitorLog "🚨 JOIN-PATH NULL ERROR: $Line" "ERROR"
        }

        'Invoke-Pester.*Path.*cannot bind.*null' {
            Write-MonitorLog "🚨 INVOKE-PESTER NULL ERROR: $Line" "ERROR"
        }

        # Generic null/parameter binding errors
        'cannot bind.*parameter|parameter.*null|argument.*null' {
            Write-MonitorLog "🚨 PARAMETER BINDING ERROR: $Line" "ERROR"
        }

        # PowerShell automatic variable errors
        'Variable.*is an automatic variable|undesired side effects|EventArgs.*automatic' {
            Write-MonitorLog "🚨 AUTOMATIC VARIABLE ERROR: $Line" "ERROR"
        }

        # Command not found errors
        'term.*not recognized|command.*not found|bb-test.*not recognized' {
            Write-MonitorLog "🚨 COMMAND NOT FOUND: $Line" "ERROR"
        }

        # Function/cmdlet errors
        'function.*not.*found|cmdlet.*does.*not.*exist|method.*not.*found' {
            Write-MonitorLog "🚨 FUNCTION ERROR: $Line" "ERROR"
        }

        # Path and file errors
        'path.*not.*found|file.*not.*exist|directory.*not.*found' {
            Write-MonitorLog "🚨 PATH ERROR: $Line" "ERROR"
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
            if ((Get-Date) - $StartTime -gt [TimeSpan]::FromMinutes(2)) {
                Write-Host "`nReal monitoring auto-stopping after 2 minutes." -ForegroundColor Yellow
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
