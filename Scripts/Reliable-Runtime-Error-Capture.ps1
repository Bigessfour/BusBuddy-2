#Requires -Version 7.5
<#
.SYNOPSIS
    Reliable Runtime Error Capture for BusBuddy WPF Application

.DESCRIPTION
    Simplified approach focusing on reliable capture of BusBuddy runtime errors:
    - Application crash detection and exit code analysis
    - Stream redirection for error/output capture
    - Windows Event Log monitoring for .NET/application errors
    - File-based error logging with real-time monitoring

.PARAMETER LogDirectory
    Directory for log files (default: logs/runtime-errors)

.PARAMETER Timeout
    Maximum monitoring time in seconds (default: 300 = 5 minutes)

.EXAMPLE
    .\Scripts\Reliable-Runtime-Error-Capture.ps1

.NOTES
    Uses file-based monitoring to avoid PowerShell runspace issues
    Focuses on capturing the -1073741510 exit code root cause
#>

param(
    [string]$LogDirectory = "logs/runtime-errors",
    [int]$Timeout = 300
)

# Import PowerShell Gallery tools if available
try {
    Import-Module Show-Progress, PSWriteColor, psInlineProgress -Force -ErrorAction Stop
    $Global:EnhancedTools = $true
    Write-Color -Text "✅ Enhanced monitoring tools loaded ", "(PowerShell Gallery)" -Color Green, Cyan
}
catch {
    $Global:EnhancedTools = $false
    Write-Host "⚠️ Basic monitoring mode - install Gallery tools for enhanced features" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "🔍 BusBuddy Reliable Runtime Error Capture" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor DarkGray
Write-Host ""

# Initialize logging
if (-not (Test-Path $LogDirectory)) {
    New-Item -ItemType Directory -Path $LogDirectory -Force | Out-Null
}

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$logFile = Join-Path $LogDirectory "busbuddy-runtime-$timestamp.log"
$errorFile = Join-Path $LogDirectory "busbuddy-errors-$timestamp.txt"
$outputFile = Join-Path $LogDirectory "busbuddy-output-$timestamp.txt"

# Initialize log files
$header = @"
BusBuddy Runtime Error Capture Session
Started: $(Get-Date)
Timeout: $Timeout seconds
Exit Code Reference:
  0 = Success
  -1073741510 = Forced termination (Ctrl+C or user close)
  -1073741819 = Access violation (memory error)
  -1073740791 = Invalid executable format
  1 = General application error
═══════════════════════════════════════════════════════════
"@

Set-Content -Path $logFile -Value $header -Encoding UTF8
Set-Content -Path $errorFile -Value "STDERR Output:`n" -Encoding UTF8
Set-Content -Path $outputFile -Value "STDOUT Output:`n" -Encoding UTF8

if ($Global:EnhancedTools) {
    Write-Color -Text "✅ Log files initialized:" -Color Green, Green
    Write-Color -Text "  📄 Main log: ", $logFile -Color Gray, Gray
    Write-Color -Text "  🔴 Error stream: ", $errorFile -Color Gray, Gray
    Write-Color -Text "  📋 Output stream: ", $outputFile -Color Gray, Gray
}
else {
    Write-Host "✅ Log files initialized:" -ForegroundColor Green
    Write-Host "  📄 Main log: $logFile" -ForegroundColor Gray
    Write-Host "  🔴 Error stream: $errorFile" -ForegroundColor Gray
    Write-Host "  📋 Output stream: $outputFile" -ForegroundColor Gray
}
Write-Host ""

# Start application with file redirection
if ($Global:EnhancedTools) {
    Write-Color -Text "🚀 Starting BusBuddy with file-based error capture..." -Color Magenta, White
}
else {
    Write-Host "🚀 Starting BusBuddy with file-based error capture..." -ForegroundColor Magenta
}

$projectRoot = Get-Location
$wpfProject = Join-Path $projectRoot "BusBuddy.WPF\BusBuddy.WPF.csproj"

if (-not (Test-Path $wpfProject)) {
    Write-Host "❌ BusBuddy.WPF project not found at: $wpfProject" -ForegroundColor Red
    exit 1
}

# Start application with output redirection
try {
    $startTime = Get-Date

    # Use Start-Process for better control and output capture
    $processParams = @{
        FilePath               = "dotnet"
        ArgumentList           = "run", "--project", "`"$wpfProject`"", "--verbosity", "detailed"
        WorkingDirectory       = $projectRoot
        RedirectStandardError  = $errorFile
        RedirectStandardOutput = $outputFile
        PassThru               = $true
        NoNewWindow            = $true
    }

    $process = Start-Process @processParams
    $appPID = $process.Id

    if ($Global:EnhancedTools) {
        Write-Color -Text "✅ Application started ", "(PID: $appPID)" -Color Green, Cyan
        Write-Color -Text "📡 Monitoring: ", "File streams, process health, Windows events" -Color Blue, White
    }
    else {
        Write-Host "✅ Application started (PID: $appPID)" -ForegroundColor Green
        Write-Host "📡 Monitoring: File streams, process health, Windows events" -ForegroundColor Blue
    }

    # Log application start
    $startEntry = "$(Get-Date -Format 'HH:mm:ss.fff') APPLICATION_START: PID $appPID started"
    Add-Content -Path $logFile -Value $startEntry -Encoding UTF8

    Write-Host ""
    if ($Global:EnhancedTools) {
        Write-Color -Text "⏳ Monitoring application runtime..." -Color Yellow, White
    }
    else {
        Write-Host "⏳ Monitoring application runtime..." -ForegroundColor Yellow
    }
    Write-Host "   • Use the application normally to trigger any errors" -ForegroundColor Gray
    Write-Host "   • Close the application window when finished" -ForegroundColor Gray
    Write-Host "   • This script will capture all errors and exit codes" -ForegroundColor Gray
    Write-Host ""

    # Monitor application with periodic updates
    $monitorStart = Get-Date
    $lastErrorCheck = Get-Date
    $lastEventCheck = Get-Date
    $errorCount = 0
    $outputLines = 0
    $eventLogErrors = @()

    # Enhanced monitoring loop
    while (-not $process.HasExited) {
        $elapsed = (Get-Date) - $monitorStart

        # Check for timeout
        if ($elapsed.TotalSeconds -gt $Timeout) {
            if ($Global:EnhancedTools) {
                Write-Color -Text "⏰ Timeout reached ", "($Timeout seconds) - stopping monitoring" -Color Yellow, Yellow
            }
            else {
                Write-Host "⏰ Timeout reached ($Timeout seconds) - stopping monitoring" -ForegroundColor Yellow
            }
            break
        }

        # Check error file for new content
        if ((Get-Date) - $lastErrorCheck -gt [TimeSpan]::FromSeconds(2)) {
            if (Test-Path $errorFile) {
                $errorContent = Get-Content $errorFile -Raw -ErrorAction SilentlyContinue
                if ($errorContent -and $errorContent.Length -gt 20) {
                    # More than just header
                    $newErrors = ($errorContent -split "`n").Count - 2  # Subtract header lines
                    if ($newErrors -gt $errorCount) {
                        $errorCount = $newErrors
                        $errorEntry = "$(Get-Date -Format 'HH:mm:ss.fff') ERROR_DETECTED: $($newErrors - $errorCount) new error(s) in STDERR"
                        Add-Content -Path $logFile -Value $errorEntry -Encoding UTF8

                        if ($Global:EnhancedTools) {
                            Write-Color -Text "🔴 New error detected ", "- check error stream" -Color Red, DarkRed
                        }
                        else {
                            Write-Host "🔴 New error detected - check error stream" -ForegroundColor Red
                        }
                    }
                }
            }
            $lastErrorCheck = Get-Date
        }

        # Check output file
        if (Test-Path $outputFile) {
            $outputContent = Get-Content $outputFile -Raw -ErrorAction SilentlyContinue
            if ($outputContent) {
                $currentOutputLines = ($outputContent -split "`n").Count
                if ($currentOutputLines -gt $outputLines) {
                    $outputLines = $currentOutputLines
                    # Look for warnings or issues in output
                    $recentOutput = $outputContent -split "`n" | Select-Object -Last 5
                    foreach ($line in $recentOutput) {
                        if ($line -match "(warning|error|exception|fail|crash)" -and $line.Length -gt 5) {
                            $warningEntry = "$(Get-Date -Format 'HH:mm:ss.fff') OUTPUT_WARNING: $line"
                            Add-Content -Path $logFile -Value $warningEntry -Encoding UTF8
                        }
                    }
                }
            }
        }

        # Check Windows Event Log periodically
        if ((Get-Date) - $lastEventCheck -gt [TimeSpan]::FromSeconds(10)) {
            try {
                $recentTime = (Get-Date).AddSeconds(-15)
                $appErrors = Get-WinEvent -FilterHashtable @{
                    LogName   = 'Application'
                    Level     = 2  # Error level
                    StartTime = $recentTime
                } -MaxEvents 5 -ErrorAction SilentlyContinue |
                Where-Object {
                    $_.ProcessId -eq $appPID -or
                    $_.Message -match "BusBuddy|dotnet|\.NET" -or
                    ($_.ProviderName -match "\.NET|Application Error" -and $_.TimeCreated -gt $monitorStart)
                }

                foreach ($eventError in $appErrors) {
                    $eventKey = "$($eventError.Id)-$($eventError.TimeCreated.ToString('HHmmss'))"
                    if ($eventKey -notin $eventLogErrors) {
                        $eventLogErrors += $eventKey
                        $eventEntry = "$(Get-Date -Format 'HH:mm:ss.fff') EVENT_LOG: [$($eventError.Id)] $($eventError.LevelDisplayName) - $($eventError.Message -replace '`n', ' ' -replace '`r', '')"
                        Add-Content -Path $logFile -Value $eventEntry -Encoding UTF8

                        if ($Global:EnhancedTools) {
                            Write-Color -Text "📋 Event Log: ", $eventError.LevelDisplayName -Color Yellow, Red
                        }
                        else {
                            Write-Host "📋 Event Log: $($eventError.LevelDisplayName)" -ForegroundColor Red
                        }
                    }
                }
            }
            catch {
                # Event log access issues - not critical
            }
            $lastEventCheck = Get-Date
        }

        # Show progress if enhanced tools available
        if ($Global:EnhancedTools -and $elapsed.TotalSeconds % 10 -eq 0) {
            $progressPercent = [math]::Min(($elapsed.TotalSeconds / $Timeout) * 100, 99)
            Write-InlineProgress -Activity "Monitoring runtime" -PercentComplete $progressPercent
        }

        # Periodic status update
        if ($elapsed.TotalSeconds % 30 -eq 0 -and $elapsed.TotalSeconds -gt 0) {
            if ($Global:EnhancedTools) {
                Write-Color -Text "📊 Status: ", "Running $([int]$elapsed.TotalMinutes)m - Errors: $errorCount - Events: $($eventLogErrors.Count)" -Color Cyan, White
            }
            else {
                Write-Host "📊 Status: Running $([int]$elapsed.TotalMinutes)m - Errors: $errorCount" -ForegroundColor Cyan
            }
        }

        Start-Sleep -Seconds 1
    }

    # Application has exited
    if ($Global:EnhancedTools) {
        Write-InlineProgress -Activity "Monitoring runtime" -Completed
    }

    $endTime = Get-Date
    $duration = $endTime - $startTime
    $exitCode = $process.ExitCode

    # Log final status
    $exitEntry = "$(Get-Date -Format 'HH:mm:ss.fff') APPLICATION_EXIT: Code $exitCode after $([int]$duration.TotalSeconds) seconds"
    Add-Content -Path $logFile -Value $exitEntry -Encoding UTF8

    # Analyze exit code
    $exitAnalysis = switch ($exitCode) {
        0 { "Success - normal termination" }
        -1073741510 { "Forced termination - user closed application or Ctrl+C" }
        -1073741819 { "Access violation - memory error or null pointer exception" }
        -1073740791 { "Invalid executable format" }
        1 { "General application error" }
        default { "Unknown error code: $exitCode" }
    }

    $analysisEntry = "$(Get-Date -Format 'HH:mm:ss.fff') EXIT_ANALYSIS: $exitAnalysis"
    Add-Content -Path $logFile -Value $analysisEntry -Encoding UTF8

    Write-Host ""
    if ($Global:EnhancedTools) {
        Write-Color -Text "✅ Application completed ", "(Exit Code: $exitCode)" -Color Green, $(if ($exitCode -eq 0) { "Green" } elseif ($exitCode -eq -1073741510) { "Yellow" } else { "Red" })
    }
    else {
        Write-Host "✅ Application completed (Exit Code: $exitCode)" -ForegroundColor $(if ($exitCode -eq 0) { "Green" } elseif ($exitCode -eq -1073741510) { "Yellow" } else { "Red" })
    }

    # Final analysis and display results
    Write-Host ""
    Write-Host "📊 Runtime Error Capture Results" -ForegroundColor Cyan
    Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor DarkGray

    if ($Global:EnhancedTools) {
        Write-Color -Text "⏱️ Duration: ", "$([int]$duration.TotalMinutes) minutes, $([int]$duration.Seconds) seconds" -Color Gray, White
        Write-Color -Text "🔢 Exit Code: ", "$exitCode" -Color Gray, $(if ($exitCode -eq 0) { "Green" } elseif ($exitCode -eq -1073741510) { "Yellow" } else { "Red" })
        Write-Color -Text "📋 Analysis: ", $exitAnalysis -Color Gray, $(if ($exitCode -eq 0) { "Green" } elseif ($exitCode -eq -1073741510) { "Yellow" } else { "Red" })
    }
    else {
        Write-Host "⏱️ Duration: $([int]$duration.TotalMinutes) minutes, $([int]$duration.Seconds) seconds" -ForegroundColor Gray
        Write-Host "🔢 Exit Code: $exitCode" -ForegroundColor $(if ($exitCode -eq 0) { "Green" } elseif ($exitCode -eq -1073741510) { "Yellow" } else { "Red" })
        Write-Host "📋 Analysis: $exitAnalysis" -ForegroundColor $(if ($exitCode -eq 0) { "Green" } elseif ($exitCode -eq -1073741510) { "Yellow" } else { "Red" })
    }

    # Check captured errors
    $capturedErrors = @()
    $capturedOutput = ""

    if (Test-Path $errorFile) {
        $errorContent = Get-Content $errorFile -Raw -ErrorAction SilentlyContinue
        if ($errorContent -and $errorContent.Length -gt 20) {
            $capturedErrors = ($errorContent -split "`n") | Where-Object { $_ -and $_ -notmatch "^STDERR Output:" }
        }
    }

    if (Test-Path $outputFile) {
        $capturedOutput = Get-Content $outputFile -Raw -ErrorAction SilentlyContinue
    }

    if ($Global:EnhancedTools) {
        Write-Color -Text "🔴 STDERR Errors: ", "$($capturedErrors.Count)" -Color $(if ($capturedErrors.Count -gt 0) { "Red" } else { "Green" }), $(if ($capturedErrors.Count -gt 0) { "Red" } else { "Green" })
        Write-Color -Text "📋 Event Log Errors: ", "$($eventLogErrors.Count)" -Color $(if ($eventLogErrors.Count -gt 0) { "Red" } else { "Green" }), $(if ($eventLogErrors.Count -gt 0) { "Red" } else { "Green" })
        Write-Color -Text "📄 Log File: ", $logFile -Color Gray, Gray
        Write-Color -Text "📏 Log Size: ", "$([math]::Round((Get-Item $logFile).Length / 1KB, 2)) KB" -Color Gray, Gray
    }
    else {
        Write-Host "🔴 STDERR Errors: $($capturedErrors.Count)" -ForegroundColor $(if ($capturedErrors.Count -gt 0) { "Red" } else { "Green" })
        Write-Host "📋 Event Log Errors: $($eventLogErrors.Count)" -ForegroundColor $(if ($eventLogErrors.Count -gt 0) { "Red" } else { "Green" })
        Write-Host "📄 Log File: $logFile" -ForegroundColor Gray
        Write-Host "📏 Log Size: $([math]::Round((Get-Item $logFile).Length / 1KB, 2)) KB" -ForegroundColor Gray
    }

    # Show recent errors if any
    if ($capturedErrors.Count -gt 0) {
        Write-Host ""
        Write-Host "🔴 Captured STDERR Errors:" -ForegroundColor Red
        $capturedErrors | Select-Object -Last 5 | ForEach-Object {
            Write-Host "   $_" -ForegroundColor DarkRed
        }
    }

    # Check for specific issues in output
    if ($capturedOutput -and $capturedOutput.Length -gt 50) {
        $outputLines = $capturedOutput -split "`n"
        $warnings = $outputLines | Where-Object { $_ -match "(warning|warn)" }
        $errors = $outputLines | Where-Object { $_ -match "(error|exception|fail)" -and $_ -notmatch "warning" }

        if ($warnings.Count -gt 0) {
            Write-Host ""
            Write-Host "⚠️ Output Warnings ($($warnings.Count)):" -ForegroundColor Yellow
            $warnings | Select-Object -Last 3 | ForEach-Object {
                Write-Host "   $_" -ForegroundColor DarkYellow
            }
        }

        if ($errors.Count -gt 0) {
            Write-Host ""
            Write-Host "🔴 Output Errors ($($errors.Count)):" -ForegroundColor Red
            $errors | Select-Object -Last 3 | ForEach-Object {
                Write-Host "   $_" -ForegroundColor DarkRed
            }
        }
    }

    Write-Host ""
    Write-Host "🔧 Recommendations:" -ForegroundColor Cyan

    if ($exitCode -eq -1073741510) {
        Write-Host "   • Exit code -1073741510 is normal when user closes the application" -ForegroundColor Yellow
        Write-Host "   • This indicates proper application shutdown, not an error" -ForegroundColor Gray
    }
    elseif ($exitCode -ne 0) {
        Write-Host "   • Non-zero exit code indicates an application issue" -ForegroundColor Red
        Write-Host "   • Check the captured error streams and event log entries" -ForegroundColor Gray
        Write-Host "   • Review logs for stack traces and exception details" -ForegroundColor Gray
    }

    if ($capturedErrors.Count -eq 0 -and $eventLogErrors.Count -eq 0 -and ($exitCode -eq 0 -or $exitCode -eq -1073741510)) {
        Write-Host "   ✅ No runtime errors detected - application is working correctly!" -ForegroundColor Green
    }

    Write-Host ""
    if ($Global:EnhancedTools) {
        Write-Color -Text "🎉 Runtime error capture completed successfully!" -Color Green, Green
    }
    else {
        Write-Host "🎉 Runtime error capture completed successfully!" -ForegroundColor Green
    }

}
catch {
    Write-Host ""
    Write-Host "❌ Error during monitoring: $($_.Exception.Message)" -ForegroundColor Red
    if ($logFile -and (Test-Path $logFile)) {
        $errorEntry = "$(Get-Date -Format 'HH:mm:ss.fff') MONITOR_ERROR: $($_.Exception.Message)"
        Add-Content -Path $logFile -Value $errorEntry -Encoding UTF8
        Write-Host "💡 Check log file: $logFile" -ForegroundColor Yellow
    }
    exit 1
}
