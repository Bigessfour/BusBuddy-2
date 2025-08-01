#!/usr/bin/env pwsh
#Requires -Version 7.5
<#
.SYNOPSIS
    Run BusBuddy application with comprehensive exception capture using no-profile method.

.DESCRIPTION
    Uses the new no-profile approach to prevent file locks while capturing all exceptions,
    errors, and warnings to detailed log files for analysis and fixing.

.PARAMETER NoProfile
    Force no-profile mode (default: true for reliability)

.PARAMETER LogDirectory
    Directory to store log files (default: logs/)

.PARAMETER Detailed
    Include detailed debugging information

.EXAMPLE
    .\run-and-capture-exceptions.ps1
    .\run-and-capture-exceptions.ps1 -Detailed

.NOTES
    File Name: run-and-capture-exceptions.ps1
    Author: BusBuddy Development Team
    Date: August 1, 2025
    Purpose: Exception capture using no-profile method to prevent file locks
#>

param(
    [switch]$NoProfile,
    [string]$LogDirectory = "logs",
    [switch]$Detailed
)

# Ensure we're in the BusBuddy workspace
if (-not (Test-Path "BusBuddy.sln")) {
    Write-Error "Must be run from BusBuddy workspace directory"
    exit 1
}

# Set environment variable to prevent profile loading and file locks
$env:NoBusBuddyProfile = "true"

# Create logs directory
if (-not (Test-Path $LogDirectory)) {
    New-Item -ItemType Directory -Path $LogDirectory -Force | Out-Null
}

# Generate timestamp for log files
$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$logPrefix = Join-Path $LogDirectory "busbuddy-run-$timestamp"

# Define log files
$logFiles = @{
    Application = "$logPrefix-application.log"
    Exceptions  = "$logPrefix-exceptions.log"
    Build       = "$logPrefix-build.log"
    Environment = "$logPrefix-environment.log"
    Summary     = "$logPrefix-summary.log"
}

Write-Host "🔒 NO-PROFILE RUN & EXCEPTION CAPTURE" -ForegroundColor Magenta
Write-Host "Timestamp: $timestamp" -ForegroundColor Gray
Write-Host "Environment: NoBusBuddyProfile=$($env:NoBusBuddyProfile)" -ForegroundColor Gray
Write-Host "Logs Directory: $LogDirectory" -ForegroundColor Gray
Write-Host ""

# Log environment information
$envInfo = @"
=== BUSBUDDY RUN & EXCEPTION CAPTURE SESSION ===
Timestamp: $timestamp
PowerShell Version: $($PSVersionTable.PSVersion)
Environment: NoBusBuddyProfile=$($env:NoBusBuddyProfile)
Working Directory: $PWD
User: $env:USERNAME
Computer: $env:COMPUTERNAME
.NET SDK: $(try { dotnet --version } catch { "Not available" })
Solution File: $(Test-Path "BusBuddy.sln")
No-Profile Mode: $NoProfile
Detailed Logging: $Detailed

Log Files:
- Application: $($logFiles.Application)
- Exceptions: $($logFiles.Exceptions)
- Build: $($logFiles.Build)
- Environment: $($logFiles.Environment)
- Summary: $($logFiles.Summary)

"@

$envInfo | Out-File -FilePath $logFiles.Environment -Encoding UTF8

try {
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

    # Step 1: Build first using no-profile method
    Write-Host "🔨 Building solution (no-profile)..." -ForegroundColor Cyan

    $buildArgs = @(
        "-NoProfile"
        "-ExecutionPolicy", "Bypass"
        "-File", "PowerShell\Scripts\Build\build-no-profile.ps1"
    )

    if ($Detailed) {
        $buildArgs += "-Verbosity", "detailed"
    }

    $buildOutput = & pwsh.exe @buildArgs 2>&1
    $buildExitCode = $LASTEXITCODE

    # Log build output
    $buildLog = @"
=== BUILD LOG ===
Timestamp: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
Command: pwsh.exe $($buildArgs -join ' ')
Exit Code: $buildExitCode

Output:
$($buildOutput -join "`n")

"@

    $buildLog | Out-File -FilePath $logFiles.Build -Encoding UTF8

    if ($buildExitCode -ne 0) {
        Write-Host "❌ Build failed (exit code: $buildExitCode)" -ForegroundColor Red
        Write-Host "Build errors logged to: $($logFiles.Build)" -ForegroundColor Yellow

        # Extract build errors for summary
        $buildErrors = $buildOutput | Where-Object { $_ -match "error|Error|ERROR" }
        if ($buildErrors) {
            "BUILD ERRORS:`n$($buildErrors -join "`n")" | Out-File -FilePath $logFiles.Exceptions -Encoding UTF8 -Append
        }

        throw "Build failed with exit code: $buildExitCode"
    }

    Write-Host "✅ Build successful" -ForegroundColor Green

    # Step 2: Run application with exception capture
    Write-Host "🚌 Starting BusBuddy application..." -ForegroundColor Cyan
    Write-Host "Monitoring for exceptions (press Ctrl+C to stop)..." -ForegroundColor Yellow

    # Create script block for running the application
    $runScript = {
        param($LogFile, $ExceptionFile, $DetailedLogging)

        try {
            # Set up error handling
            $ErrorActionPreference = "Continue"

            # Use dotnet directly with output capture instead of Start-Process
            Write-Host "🚀 Starting BusBuddy application..." -ForegroundColor Green

            # Use a more reliable approach for capturing output
            $psi = New-Object System.Diagnostics.ProcessStartInfo
            $psi.FileName = "dotnet"
            $psi.Arguments = "run --project BusBuddy.WPF\BusBuddy.WPF.csproj"
            $psi.RedirectStandardOutput = $true
            $psi.RedirectStandardError = $true
            $psi.UseShellExecute = $false
            $psi.CreateNoWindow = $true

            $process = [System.Diagnostics.Process]::Start($psi)

            # Monitor the process
            $applicationLog = @()
            $exceptionLog = @()

            while (-not $process.HasExited) {
                Start-Sleep -Milliseconds 500

                # Read any available output
                if (-not $process.StandardOutput.EndOfStream) {
                    $line = $process.StandardOutput.ReadLine()
                    if ($line) {
                        $timestamp = Get-Date -Format "HH:mm:ss.fff"
                        $logEntry = "[$timestamp] $line"
                        $applicationLog += $logEntry

                        # Check for exceptions and errors
                        if ($line -match "(Exception|Error|Critical|Fatal|Unhandled)" -and $line -notmatch "Syncfusion|license") {
                            $exceptionEntry = "[$timestamp] EXCEPTION: $line"
                            $exceptionLog += $exceptionEntry
                            Write-Host "🚨 $exceptionEntry" -ForegroundColor Red
                        }
                    }
                }

                if (-not $process.StandardError.EndOfStream) {
                    $errorLine = $process.StandardError.ReadLine()
                    if ($errorLine) {
                        $timestamp = Get-Date -Format "HH:mm:ss.fff"
                        $errorEntry = "[$timestamp] ERROR: $errorLine"
                        $applicationLog += $errorEntry
                        $exceptionLog += $errorEntry
                        Write-Host "❌ $errorEntry" -ForegroundColor Yellow
                    }
                }
            }

            # Write logs to files
            if ($applicationLog) {
                $applicationLog -join "`n" | Out-File -FilePath $LogFile -Encoding UTF8
            }

            if ($exceptionLog) {
                $exceptionLog -join "`n" | Out-File -FilePath $ExceptionFile -Encoding UTF8 -Append
            }

            return @{
                ExitCode = $process.ExitCode
                ExceptionCount = $exceptionLog.Count
                ApplicationLogEntries = $applicationLog.Count
            }
        }
        catch {
            $errorInfo = @{
                ExitCode = -1
                ExceptionCount = 1
                ApplicationLogEntries = 0
                LaunchError = $_.Exception.Message
            }

            "LAUNCH EXCEPTION: $($_.Exception.Message)" | Out-File -FilePath $ExceptionFile -Encoding UTF8 -Append
            return $errorInfo
        }
    }

    # Run the application monitoring in a job for better control
    $job = Start-Job -ScriptBlock $runScript -ArgumentList $logFiles.Application, $logFiles.Exceptions, $Detailed

    Write-Host "Application started. Monitoring for 30 seconds or until you press Ctrl+C..." -ForegroundColor Cyan

    # Wait for job completion or timeout
    $timeout = 30 # seconds
    $result = Wait-Job -Job $job -Timeout $timeout

    if ($result) {
        $runResult = Receive-Job -Job $job
        Remove-Job -Job $job
    } else {
        Write-Host "⏰ Timeout reached. Stopping application..." -ForegroundColor Yellow
        Stop-Job -Job $job
        Remove-Job -Job $job
        $runResult = @{
            ExitCode = "Timeout"
            ExceptionCount = "Unknown"
            ApplicationLogEntries = "Unknown"
        }
    }

    $stopwatch.Stop()

    # Create summary report
    $summary = @"
=== BUSBUDDY RUN & EXCEPTION CAPTURE SUMMARY ===
Timestamp: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
Total Duration: $($stopwatch.Elapsed.TotalSeconds.ToString('F1')) seconds

BUILD RESULTS:
- Status: Success
- Exit Code: $buildExitCode
- Log: $($logFiles.Build)

APPLICATION RESULTS:
- Exit Code: $($runResult.ExitCode)
- Exception Count: $($runResult.ExceptionCount)
- Log Entries: $($runResult.ApplicationLogEntries)
- Application Log: $($logFiles.Application)
- Exception Log: $($logFiles.Exceptions)

ENVIRONMENT:
- PowerShell: $($PSVersionTable.PSVersion)
- No-Profile Mode: $NoProfile
- File Locks Prevented: ✅

LOG FILES CREATED:
- Application: $($logFiles.Application)
- Exceptions: $($logFiles.Exceptions)
- Build: $($logFiles.Build)
- Environment: $($logFiles.Environment)
- Summary: $($logFiles.Summary)

NEXT STEPS:
1. Review exception log: $($logFiles.Exceptions)
2. Analyze application log: $($logFiles.Application)
3. Check build log if needed: $($logFiles.Build)

"@

    $summary | Out-File -FilePath $logFiles.Summary -Encoding UTF8

    # Display summary
    Write-Host ""
    Write-Host "📊 EXECUTION SUMMARY" -ForegroundColor Cyan
    Write-Host "Duration: $($stopwatch.Elapsed.TotalSeconds.ToString('F1')) seconds" -ForegroundColor Gray
    Write-Host "Build Status: ✅ Success" -ForegroundColor Green
    Write-Host "Application Exit Code: $($runResult.ExitCode)" -ForegroundColor $(if ($runResult.ExitCode -eq 0) { 'Green' } else { 'Yellow' })
    Write-Host "Exception Count: $($runResult.ExceptionCount)" -ForegroundColor $(if ($runResult.ExceptionCount -eq 0) { 'Green' } elseif ($runResult.ExceptionCount -eq "Unknown") { 'Yellow' } else { 'Red' })
    Write-Host ""
    Write-Host "📁 Log Files Created:" -ForegroundColor Yellow
    foreach ($logType in $logFiles.Keys) {
        $logFile = $logFiles[$logType]
        $exists = Test-Path $logFile
        $size = if ($exists) { [math]::Round((Get-Item $logFile).Length / 1KB, 1) } else { 0 }
        Write-Host "   $logType`: $logFile ($size KB)" -ForegroundColor $(if ($exists -and $size -gt 0) { 'Gray' } else { 'DarkGray' })
    }

    if ($runResult.ExceptionCount -gt 0 -and $runResult.ExceptionCount -ne "Unknown") {
        Write-Host ""
        Write-Host "🚨 EXCEPTIONS DETECTED!" -ForegroundColor Red
        Write-Host "Review the exception log: $($logFiles.Exceptions)" -ForegroundColor Yellow
    }

}
catch {
    $stopwatch.Stop()
    Write-Host ""
    Write-Host "❌ EXECUTION FAILED" -ForegroundColor Red
    Write-Host "Duration: $($stopwatch.Elapsed.TotalSeconds.ToString('F1')) seconds" -ForegroundColor Gray
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red

    # Log the failure
    "SCRIPT FAILURE: $($_.Exception.Message)" | Out-File -FilePath $logFiles.Exceptions -Encoding UTF8 -Append

    exit 1
}
finally {
    # Clean up environment variable
    Remove-Item -Path "env:NoBusBuddyProfile" -ErrorAction SilentlyContinue
}

Write-Host ""
Write-Host "✅ Exception capture completed. Review log files for analysis." -ForegroundColor Green
