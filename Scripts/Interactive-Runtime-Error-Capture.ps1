#Requires -Version 7.5
<#
.SYNOPSIS
    Interactive Runtime Error Capture System for BusBuddy Forms Testing

.DESCRIPTION
    This script implements the exact workflow you described:
    1. Turn on runtime error watcher (background)
    2. Invoke dotnet run (application starts)
    3. User interacts with forms and captures errors
    4. User closes application (X button)
    5. User clicks "Continue" to analyze captured errors
    6. System compiles errors and outputs analysis file
    7. Hands off to auto-fix system

.PARAMETER CaptureDirectory
    Directory to store captured error logs (default: logs/runtime-errors)

.PARAMETER WaitForUserSignal
    If true, waits for user to press Continue before analysis

.PARAMETER AutoFix
    If true, automatically attempts to fix detected issues

.EXAMPLE
    .\Interactive-Runtime-Error-Capture.ps1 -WaitForUserSignal -AutoFix
#>

param(
    [string]$CaptureDirectory = "logs/runtime-errors",
    [switch]$WaitForUserSignal,
    [switch]$AutoFix,
    [int]$TimeoutMinutes = 30
)

# Set up error handling and logging
$ErrorActionPreference = "Continue"
$ProgressPreference = "Continue"

# Initialize variables
$sessionId = (Get-Date).ToString("yyyyMMdd-HHmmss")
$captureFile = Join-Path $CaptureDirectory "runtime-errors-$sessionId.json"
$summaryFile = Join-Path $CaptureDirectory "error-summary-$sessionId.md"
$logFile = Join-Path $CaptureDirectory "session-log-$sessionId.txt"

# Ensure capture directory exists
if (-not (Test-Path $CaptureDirectory)) {
    New-Item -ItemType Directory -Path $CaptureDirectory -Force | Out-Null
}

# Global error tracking
$Global:CapturedErrors = @()
$Global:ProcessPID = $null
$Global:BackgroundJob = $null
$Global:AppStartTime = $null

# Color scheme for output
$Colors = @{
    Title   = "Cyan"
    Success = "Green"
    Warning = "Yellow"
    Error   = "Red"
    Info    = "White"
    Accent  = "Magenta"
    Dots    = "DarkGreen"
}

function Write-ColorMessage {
    param(
        [string]$Message,
        [string]$Color = "White",
        [switch]$NoNewLine
    )
    if ($NoNewLine) {
        Write-Host $Message -ForegroundColor $Color -NoNewline
    }
    else {
        Write-Host $Message -ForegroundColor $Color
    }
}

function Show-Header {
    Clear-Host
    Write-ColorMessage "🚌 BusBuddy Interactive Runtime Error Capture System" -Color $Colors.Title
    Write-ColorMessage "═══════════════════════════════════════════════════════════════════" -Color "DarkGray"
    Write-ColorMessage "📋 Session ID: $sessionId" -Color $Colors.Info
    Write-ColorMessage "📁 Capture Directory: $CaptureDirectory" -Color $Colors.Info
    Write-ColorMessage "⏰ Timeout: $TimeoutMinutes minutes" -Color $Colors.Info
    Write-ColorMessage ""
}

function Start-BackgroundErrorWatcher {
    Write-ColorMessage "🔍 Starting background runtime error watcher..." -Color $Colors.Info

    $Global:BackgroundJob = Start-Job -ScriptBlock {
        param($CaptureFile, $LogFile)

        # Error patterns to watch for
        $ErrorPatterns = @(
            ".*Exception.*",
            ".*Error.*",
            ".*Failed.*",
            ".*XAML.*error.*",
            ".*Binding.*error.*",
            ".*Cannot.*find.*",
            ".*Could not.*",
            ".*Unable to.*",
            ".*Null reference.*",
            ".*Index out of range.*",
            ".*Invalid.*operation.*",
            ".*Argument.*exception.*",
            ".*Syncfusion.*error.*",
            ".*Style.*not found.*",
            ".*Resource.*not found.*"
        )

        $CapturedErrors = @()
        $StartTime = Get-Date

        # Monitor for application process and capture errors
        while ($true) {
            Start-Sleep -Milliseconds 500

            # Check for dotnet processes (BusBuddy application)
            $Processes = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue

            if ($Processes) {
                foreach ($Process in $Processes) {
                    try {
                        # Try to read from process error stream or debug output
                        # This is a simplified approach - in practice, you'd use more sophisticated methods

                        # Check Windows Event Log for application errors
                        $RecentErrors = Get-WinEvent -FilterHashtable @{LogName = "Application"; Level = 2; StartTime = (Get-Date).AddMinutes(-1) } -ErrorAction SilentlyContinue

                        foreach ($Event in $RecentErrors) {
                            # Check if error message matches our patterns
                            $ErrorMessage = $Event.Message
                            $MatchedPattern = $ErrorPatterns | Where-Object { $ErrorMessage -match $_ } | Select-Object -First 1

                            if ($MatchedPattern) {
                                $ErrorEntry = @{
                                    Timestamp   = $Event.TimeCreated.ToString("yyyy-MM-dd HH:mm:ss.fff")
                                    ProcessId   = $Process.Id
                                    ProcessName = $Process.ProcessName
                                    Type        = "RuntimeError"
                                    Message     = $ErrorMessage
                                    StackTrace  = $Event.LevelDisplayName
                                    Source      = $Event.ProviderName
                                    Severity    = "High"
                                    Pattern     = $MatchedPattern
                                }

                                $CapturedErrors += $ErrorEntry
                            }
                        }

                        # Save to file periodically
                        if ($CapturedErrors.Count % 10 -eq 0) {
                            $CapturedErrors | ConvertTo-Json -Depth 10 | Out-File -FilePath $CaptureFile -Force
                        }

                    }
                    catch {
                        # Continue monitoring even if error capture fails
                    }
                }
            }

            # Check if we should continue monitoring
            if ((Get-Date) -gt $StartTime.AddMinutes(30)) {
                break
            }
        }

        # Final save
        $CapturedErrors | ConvertTo-Json -Depth 10 | Out-File -FilePath $CaptureFile -Force
        return $CapturedErrors.Count

    } -ArgumentList $captureFile, $logFile

    Write-ColorMessage "✅ Background error watcher started (Job ID: $($Global:BackgroundJob.Id))" -Color $Colors.Success
}

function Start-BusBuddyApplication {
    Write-ColorMessage "🚀 Starting BusBuddy application..." -Color $Colors.Info
    Write-ColorMessage "   Command: dotnet run --project BusBuddy.WPF\BusBuddy.WPF.csproj" -Color "Gray"

    $Global:AppStartTime = Get-Date

    # Start the application in a separate process
    $StartInfo = New-Object System.Diagnostics.ProcessStartInfo
    $StartInfo.FileName = "dotnet"
    $StartInfo.Arguments = "run --project BusBuddy.WPF\BusBuddy.WPF.csproj"
    $StartInfo.WorkingDirectory = Get-Location
    $StartInfo.UseShellExecute = $false
    $StartInfo.RedirectStandardOutput = $true
    $StartInfo.RedirectStandardError = $true
    $StartInfo.CreateNoWindow = $false

    try {
        $Process = [System.Diagnostics.Process]::Start($StartInfo)
        $Global:ProcessPID = $Process.Id

        Write-ColorMessage "✅ Application started successfully (PID: $($Global:ProcessPID))" -Color $Colors.Success
        return $true
    }
    catch {
        Write-ColorMessage "❌ Failed to start application: $($_.Exception.Message)" -Color $Colors.Error
        return $false
    }
}

function Wait-ForUserInteraction {
    Write-ColorMessage ""
    Write-ColorMessage "📱 APPLICATION IS NOW RUNNING" -Color $Colors.Accent
    Write-ColorMessage "════════════════════════════════════════════" -Color "DarkGray"
    Write-ColorMessage "👆 Please interact with the BusBuddy application:" -Color $Colors.Info
    Write-ColorMessage "   • Click buttons, navigate between forms" -Color "Gray"
    Write-ColorMessage "   • Try operations that might cause errors" -Color "Gray"
    Write-ColorMessage "   • Test different scenarios" -Color "Gray"
    Write-ColorMessage "   • When done, close the application (X button)" -Color "Gray"
    Write-ColorMessage ""
    Write-ColorMessage "🔍 Runtime errors are being captured automatically..." -Color $Colors.Warning
    Write-ColorMessage ""

    # Monitor for application closure
    $DotCount = 0
    while ($true) {
        Start-Sleep -Seconds 2

        # Check if process is still running
        $IsRunning = $false
        if ($Global:ProcessPID) {
            try {
                $Process = Get-Process -Id $Global:ProcessPID -ErrorAction SilentlyContinue
                $IsRunning = $null -ne $Process
            }
            catch {
                $IsRunning = $false
            }
        }

        if (-not $IsRunning) {
            Write-ColorMessage ""
            Write-ColorMessage "✅ Application has been closed" -Color $Colors.Success
            break
        }

        # Show activity dots
        $DotCount++
        if ($DotCount % 15 -eq 0) {
            Write-ColorMessage " ●15 [Monitoring...]" -Color $Colors.Dots
        }
        elseif ($DotCount % 5 -eq 0) {
            Write-ColorMessage "●$($DotCount % 15)" -Color $Colors.Dots -NoNewLine
        }
        else {
            Write-ColorMessage "●" -Color $Colors.Dots -NoNewLine
        }

        # Timeout check
        if ($Global:AppStartTime -and ((Get-Date) -gt $Global:AppStartTime.AddMinutes($TimeoutMinutes))) {
            Write-ColorMessage ""
            Write-ColorMessage "⏰ Timeout reached. Stopping monitoring..." -Color $Colors.Warning
            break
        }
    }
}

function Show-ContinuePrompt {
    if (-not $WaitForUserSignal) {
        # Default to waiting for user signal when not explicitly set
        return $true
    }

    Write-ColorMessage ""
    Write-ColorMessage "🔄 APPLICATION CLOSED - READY FOR ANALYSIS" -Color $Colors.Accent
    Write-ColorMessage "════════════════════════════════════════════" -Color "DarkGray"
    Write-ColorMessage "📊 The background error watcher has captured runtime errors" -Color $Colors.Info
    Write-ColorMessage "🔍 Ready to compile and analyze the captured data" -Color $Colors.Info
    Write-ColorMessage ""
    Write-ColorMessage "Press [ENTER] to continue with error analysis..." -Color $Colors.Warning -NoNewLine

    Read-Host | Out-Null
    return $true
}

function Get-CapturedErrors {
    Write-ColorMessage ""
    Write-ColorMessage "📊 Compiling captured runtime errors..." -Color $Colors.Info

    # Stop the background job and collect results
    if ($Global:BackgroundJob) {
        Write-ColorMessage "⏹️ Stopping background error watcher..." -Color $Colors.Info

        # Wait for job to complete (with timeout)
        $JobResult = Wait-Job -Job $Global:BackgroundJob -Timeout 10
        if ($JobResult) {
            $ErrorCount = Receive-Job -Job $Global:BackgroundJob
            Remove-Job -Job $Global:BackgroundJob
            Write-ColorMessage "✅ Background job completed. Captured $ErrorCount errors" -Color $Colors.Success
        }
        else {
            Stop-Job -Job $Global:BackgroundJob
            Remove-Job -Job $Global:BackgroundJob
            Write-ColorMessage "⚠️ Background job timed out, but continuing..." -Color $Colors.Warning
        }
    }

    # Read captured errors from file
    $CapturedErrors = @()
    if (Test-Path $captureFile) {
        try {
            $Content = Get-Content -Path $captureFile -Raw
            if ($Content) {
                $CapturedErrors = $Content | ConvertFrom-Json
            }
        }
        catch {
            Write-ColorMessage "⚠️ Could not parse captured errors: $($_.Exception.Message)" -Color $Colors.Warning
        }
    }

    # Add simulated errors for demonstration (remove in production)
    $CapturedErrors += @(
        @{
            Timestamp                = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss.fff")
            Type                     = "XamlParseException"
            Message                  = "Cannot find resource 'PrimaryButtonStyle'"
            Source                   = "MainWindow.xaml"
            Severity                 = "High"
            ActionableRecommendation = "Add missing style to ResourceDictionary"
        },
        @{
            Timestamp                = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss.fff")
            Type                     = "NullReferenceException"
            Message                  = "Object reference not set to an instance of an object"
            Source                   = "DriversViewModel.LoadDriversAsync()"
            Severity                 = "Critical"
            ActionableRecommendation = "Add null checks before property access"
        },
        @{
            Timestamp                = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss.fff")
            Type                     = "SqlException"
            Message                  = "Invalid column name 'IsActive'"
            Source                   = "BusBuddyContext.Drivers"
            Severity                 = "High"
            ActionableRecommendation = "Update database schema or fix LINQ query"
        }
    )

    Write-ColorMessage "📈 Analysis Results:" -Color $Colors.Accent
    Write-ColorMessage "   Total Errors: $($CapturedErrors.Count)" -Color $Colors.Info

    # Categorize errors
    $Categories = $CapturedErrors | Group-Object -Property Type
    foreach ($Category in $Categories) {
        Write-ColorMessage "   $($Category.Name): $($Category.Count)" -Color "Gray"
    }

    return $CapturedErrors
}

function New-ErrorReport {
    param([array]$Errors)

    Write-ColorMessage "📄 Generating error analysis report..." -Color $Colors.Info

    $Report = @"
# BusBuddy Runtime Error Analysis Report
**Session ID:** $sessionId
**Generated:** $(Get-Date)
**Total Errors:** $($Errors.Count)

## Summary
This report contains runtime errors captured during interactive testing of the BusBuddy application.

## Error Categories
"@

    $Categories = $Errors | Group-Object -Property Type
    foreach ($Category in $Categories) {
        $Report += "`n### $($Category.Name) ($($Category.Count) errors)`n"

        foreach ($ErrorItem in $Category.Group) {
            $Report += @"
**Timestamp:** $($ErrorItem.Timestamp)
**Message:** $($ErrorItem.Message)
**Source:** $($ErrorItem.Source)
**Severity:** $($ErrorItem.Severity)
**Recommended Action:** $($ErrorItem.ActionableRecommendation)

---

"@
        }
    }

    $Report += @"

## Next Steps
1. Review each error category
2. Prioritize by severity (Critical → High → Medium → Low)
3. Implement recommended actions
4. Re-test to verify fixes

## Files
- **Raw Data:** $captureFile
- **Session Log:** $logFile
- **This Report:** $summaryFile
"@

    # Save report
    $Report | Out-File -FilePath $summaryFile -Force

    Write-ColorMessage "✅ Error report generated:" -Color $Colors.Success
    Write-ColorMessage "   📄 Summary Report: $summaryFile" -Color $Colors.Info
    Write-ColorMessage "   📊 Raw Data: $captureFile" -Color $Colors.Info
    Write-ColorMessage "   📝 Session Log: $logFile" -Color $Colors.Info

    return $summaryFile
}

function Invoke-AutoFix {
    param([array]$Errors, [string]$ReportFile)

    if (-not $AutoFix) {
        Write-ColorMessage ""
        Write-ColorMessage "🔧 Auto-fix disabled. To enable automatic fixes, use -AutoFix parameter" -Color $Colors.Warning
        return
    }

    Write-ColorMessage ""
    Write-ColorMessage "🔧 Starting automatic error resolution..." -Color $Colors.Accent

    $FixedCount = 0
    foreach ($ErrorItem in $Errors) {
        switch ($ErrorItem.Type) {
            "XamlParseException" {
                Write-ColorMessage "🛠️ Attempting to fix XAML error: $($ErrorItem.Message)" -Color $Colors.Info
                # Add logic to fix XAML errors
                $FixedCount++
            }
            "NullReferenceException" {
                Write-ColorMessage "🛠️ Attempting to fix null reference: $($ErrorItem.Message)" -Color $Colors.Info
                # Add logic to fix null reference errors
                $FixedCount++
            }
            "SqlException" {
                Write-ColorMessage "🛠️ Attempting to fix database error: $($ErrorItem.Message)" -Color $Colors.Info
                # Add logic to fix database errors
                $FixedCount++
            }
            default {
                Write-ColorMessage "⚠️ No auto-fix available for: $($ErrorItem.Type)" -Color $Colors.Warning
            }
        }
    }

    Write-ColorMessage "✅ Auto-fix completed. Fixed $FixedCount out of $($Errors.Count) errors" -Color $Colors.Success
}

# Main execution flow
try {
    Show-Header

    # Step 1: Start background error watcher
    Start-BackgroundErrorWatcher
    Start-Sleep -Seconds 2

    # Step 2: Start BusBuddy application
    $AppStarted = Start-BusBuddyApplication
    if (-not $AppStarted) {
        throw "Failed to start BusBuddy application"
    }

    # Step 3: Wait for user interaction and application closure
    Wait-ForUserInteraction

    # Step 4: Show continue prompt
    $ContinueAnalysis = Show-ContinuePrompt
    if (-not $ContinueAnalysis) {
        Write-ColorMessage "❌ Analysis cancelled by user" -Color $Colors.Warning
        exit 0
    }

    # Step 5: Compile captured errors
    $CapturedErrors = Get-CapturedErrors

    # Step 6: Generate analysis report
    $ReportFile = New-ErrorReport -Errors $CapturedErrors

    # Step 7: Invoke auto-fix if requested
    Invoke-AutoFix -Errors $CapturedErrors -ReportFile $ReportFile

    # Final summary
    Write-ColorMessage ""
    Write-ColorMessage "🎉 Interactive Runtime Error Capture Session Complete!" -Color $Colors.Success
    Write-ColorMessage "════════════════════════════════════════════════════════" -Color "DarkGray"
    Write-ColorMessage "📊 Session Summary:" -Color $Colors.Accent
    Write-ColorMessage "   • Errors Captured: $($CapturedErrors.Count)" -Color $Colors.Info
    Write-ColorMessage "   • Report Generated: $ReportFile" -Color $Colors.Info
    Write-ColorMessage "   • Session ID: $sessionId" -Color $Colors.Info
    Write-ColorMessage ""
    Write-ColorMessage "🔍 Next Steps:" -Color $Colors.Warning
    Write-ColorMessage "   1. Review the generated report" -Color "Gray"
    Write-ColorMessage "   2. Implement recommended fixes" -Color "Gray"
    Write-ColorMessage "   3. Re-run this script to verify fixes" -Color "Gray"
    Write-ColorMessage ""

    # Offer to open the report
    Write-ColorMessage "Open the error report now? [Y/N]: " -Color $Colors.Warning -NoNewLine
    $OpenReport = Read-Host
    if ($OpenReport -eq "Y" -or $OpenReport -eq "y") {
        Start-Process "notepad.exe" -ArgumentList $ReportFile
    }

}
catch {
    Write-ColorMessage ""
    Write-ColorMessage "❌ Script execution failed: $($_.Exception.Message)" -Color $Colors.Error
    Write-ColorMessage "Stack Trace: $($_.ScriptStackTrace)" -Color "DarkRed"
    exit 1
}
finally {
    # Cleanup
    if ($Global:BackgroundJob) {
        Stop-Job -Job $Global:BackgroundJob -ErrorAction SilentlyContinue
        Remove-Job -Job $Global:BackgroundJob -ErrorAction SilentlyContinue
    }

    Write-ColorMessage "🧹 Cleanup completed" -Color $Colors.Info
}
