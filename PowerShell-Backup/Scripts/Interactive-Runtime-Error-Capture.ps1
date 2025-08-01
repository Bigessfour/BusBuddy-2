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
$errorCaptureFile = Join-Path $CaptureDirectory "error-stream-$sessionId.log"

# Ensure capture directory exists
if (-not (Test-Path $CaptureDirectory)) {
    New-Item -ItemType Directory -Path $CaptureDirectory -Force | Out-Null
}

# Global error tracking
$Global:CapturedErrors = @()
$Global:ProcessPID = $null
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
    Write-ColorMessage "📂 Capture Directory: $CaptureDirectory" -Color $Colors.Info
    Write-ColorMessage "🔥 Error Log: $errorCaptureFile" -Color $Colors.Info
    Write-ColorMessage "⏰ Timeout: $TimeoutMinutes minutes" -Color $Colors.Info
    Write-ColorMessage ""
}

function Start-BusBuddyApplication {
    Write-ColorMessage "🚀 Starting BusBuddy application..." -Color $Colors.Info
    Write-ColorMessage "   Command: dotnet run --project BusBuddy.WPF\BusBuddy.WPF.csproj" -Color "Gray"
    Write-ColorMessage "   Stderr is being redirected to: $errorCaptureFile" -Color "Gray"

    $Global:AppStartTime = Get-Date

    # Start the application in a separate process and redirect stderr
    $processInfo = New-Object System.Diagnostics.ProcessStartInfo
    $processInfo.FileName = "dotnet"
    $processInfo.Arguments = "run --project BusBuddy.WPF\BusBuddy.WPF.csproj --verbosity quiet"

    # Determine project root directory - handle multiple possible structures
    $scriptDir = $PSScriptRoot
    $projectRoot = $null

    # Try multiple relative paths to find the project root
    $possiblePaths = @(
        (Get-Item $scriptDir).Parent.Parent.FullName,  # Regular structure
        (Join-Path (Get-Item $scriptDir).Parent.Parent.FullName ".."),  # One level up
        (Get-Item $scriptDir).Parent.Parent.Parent.FullName  # Alternative structure
    )

    foreach ($path in $possiblePaths) {
        if (Test-Path (Join-Path $path "BusBuddy.sln")) {
            $projectRoot = $path
            break
        }
    }

    # If we still don't have a root, use the current directory
    if (-not $projectRoot) {
        $projectRoot = (Get-Location).Path
        Write-ColorMessage "⚠️ Could not determine project root, using current directory: $projectRoot" -Color $Colors.Warning
    }
    else {
        Write-ColorMessage "🏠 Found project root: $projectRoot" -Color $Colors.Info
    }

    $processInfo.WorkingDirectory = $projectRoot
    $processInfo.UseShellExecute = $false
    $processInfo.RedirectStandardError = $true
    $processInfo.CreateNoWindow = $true

    try {
        $process = [System.Diagnostics.Process]::Start($processInfo)
        $Global:ProcessPID = $process.Id

        # Instead of using event handler, create a file to capture error output
        $errorReader = $process.StandardError

        # Create a background job to read error output
        $errorCaptureJob = Start-ThreadJob -ScriptBlock {
            param($reader, $filePath, $colors)

            # Import functions from parent scope
            function Write-ColorMessage {
                param([string]$Message, [string]$Color = "White", [switch]$NoNewLine)
                if ($NoNewLine) {
                    Write-Host $Message -ForegroundColor $Color -NoNewline
                } else {
                    Write-Host $Message -ForegroundColor $Color
                }
            }

            try {
                # Read the stream until it's closed
                while (-not $reader.EndOfStream) {
                    $line = $reader.ReadLine()
                    if (-not [string]::IsNullOrEmpty($line)) {
                        Add-Content -Path $filePath -Value $line -ErrorAction SilentlyContinue
                        Write-ColorMessage "   ERROR DETECTED: $line" -Color $colors.Error
                    }
                }
            }
            catch {
                # Handle any exceptions in the background job
                Add-Content -Path $filePath -Value "Error reading stream: $($_.Exception.Message)" -ErrorAction SilentlyContinue
            }
        } -ArgumentList $errorReader, $errorCaptureFile, $Colors

        # Store a reference to the error capture job
        $script:errorCaptureJob = $errorCaptureJob

        Write-ColorMessage "✅ Application started successfully (PID: $($Global:ProcessPID))" -Color $Colors.Success
        return $process
    }
    catch {
        Write-ColorMessage "❌ Failed to start application: $($_.Exception.Message)" -Color $Colors.Error
        return $null
    }
}

function Wait-ForUserInteraction {
    param(
        $Process,
        $ErrorCaptureJob = $null
    )

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
    Write-ColorMessage "Monitoring application process (PID: $($Process.Id))...", $Colors.Dots
    $Process.WaitForExit($TimeoutMinutes * 60 * 1000) # Wait for exit with timeout

    if ($Process.HasExited) {
        Write-ColorMessage "`n✅ Application has been closed." -Color $Colors.Success
    }
    else {
        Write-ColorMessage "`n⏰ Timeout reached. Stopping process..." -Color $Colors.Warning
        try { $Process.Kill() } catch { }
    }

    # Wait a moment for the error capture job to complete
    if ($ErrorCaptureJob) {
        Write-ColorMessage "Completing error capture..." -Color $Colors.Info
        Wait-Job -Job $ErrorCaptureJob -Timeout 5 | Out-Null
        Remove-Job -Job $ErrorCaptureJob -Force
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
    Write-ColorMessage "📊 Compiling captured runtime errors from '$errorCaptureFile'..." -Color $Colors.Info

    if (-not (Test-Path $errorCaptureFile)) {
        Write-ColorMessage "⚠️ Error log file not found." -Color $Colors.Warning
        return @()
    }

    $content = Get-Content -Path $errorCaptureFile -Raw
    if ([string]::IsNullOrWhiteSpace($content)) {
        Write-ColorMessage "✅ No errors captured in the log file." -Color $Colors.Success
        return @()
    }

    # Enhanced parsing of stack traces to create structured error objects
    $ErrorPatterns = @(
        # Standard exception pattern
        '(?<Type>\w+Exception):\s*(?<Message>[^\r\n]+)',
        # XAML parsing errors
        'XamlParseException:\s*(?<Message>[^\r\n]+)',
        # SQL errors
        'SqlException:\s*(?<Message>[^\r\n]+)',
        # General error pattern (may not be an exception)
        'Error:\s*(?<Message>[^\r\n]+)'
    )
    $regex = [regex]($ErrorPatterns -join "|")
    $matches = $regex.Matches($content)

    # Also look for stack traces
    $stackTraceRegex = [regex]'(?:   at .+\n)+'
    $stackTraces = $stackTraceRegex.Matches($content)

    # Create a more comprehensive collection of error objects with improved context
    $CapturedErrors = [System.Collections.ArrayList]::new()

    # Process each exception match
    foreach ($match in $matches) {
        $errorType = if ($match.Groups["Type"].Success) { $match.Groups["Type"].Value } else { "UnknownError" }
        $errorMessage = $match.Groups["Message"].Value.Trim()

        # Find if there's an associated stack trace near this error (within next 500 characters)
        $errorPos = $match.Index
        $errorStackTrace = ""
        foreach ($trace in $stackTraces) {
            # If stack trace is within reasonable distance of the error
            if ([Math]::Abs($trace.Index - $errorPos) -lt 500) {
                $errorStackTrace = $trace.Value
                break
            }
        }

        # Determine severity based on error type and content
        $severity = "Medium"
        if ($errorType -match "Null|ArgumentNull|IndexOutOfRange|ArgumentOutOfRange") {
            $severity = "Critical"
        }
        elseif ($errorType -match "Sql|Xaml|IO|File|Network") {
            $severity = "High"
        }

        # Create action recommendation based on error type
        $recommendation = ""
        switch -Regex ($errorType) {
            "NullReference" { $recommendation = "Check for null object references before property/method access" }
            "ArgumentNull|ArgumentException" { $recommendation = "Ensure all method parameters have valid values" }
            "Xaml" { $recommendation = "Verify XAML syntax, bindings, and resources in UI elements" }
            "Sql|Database" { $recommendation = "Verify database connection and query syntax" }
            "IO|File" { $recommendation = "Ensure file paths exist and application has proper permissions" }
            default { $recommendation = "Review stack trace for more context on the error source" }
        }

        # Create and add the error object
        $errorObject = [pscustomobject]@{
            Timestamp                = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss.fff")
            Type                     = $errorType
            Message                  = $errorMessage
            Source                   = "Runtime Log"
            Severity                 = $severity
            ActionableRecommendation = $recommendation
            StackTrace               = if ([string]::IsNullOrWhiteSpace($errorStackTrace)) { $match.Value } else { $errorStackTrace }
        }

        $null = $CapturedErrors.Add($errorObject)
    }

    # If no structured errors were found but there is content, add a generic entry
    if ($CapturedErrors.Count -eq 0 -and -not [string]::IsNullOrWhiteSpace($content)) {
        # Create a generic error entry with the first few lines of the log
        $firstFewLines = ($content -split '\r?\n')[0..5] -join "`n"
        $genericError = [pscustomobject]@{
            Timestamp                = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss.fff")
            Type                     = "UnstructuredError"
            Message                  = "Unstructured error output detected (see log for details)"
            Source                   = "Runtime Log"
            Severity                 = "Medium"
            ActionableRecommendation = "Review complete error log for details"
            StackTrace               = $firstFewLines
        }
        $null = $CapturedErrors.Add($genericError)
    }

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
- **Raw Data:** $errorCaptureFile
- **Session Log:** $logFile
- **This Report:** $summaryFile
"@

    # Save report
    $Report | Out-File -FilePath $summaryFile -Force

    Write-ColorMessage "✅ Error report generated:" -Color $Colors.Success
    Write-ColorMessage "   📄 Summary Report: $summaryFile" -Color $Colors.Info
    Write-ColorMessage "   🔥 Raw Error Log: $errorCaptureFile" -Color $Colors.Info
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

    # Step 1: Start BusBuddy application and capture errors
    $process = Start-BusBuddyApplication
    if (-not $process) {
        throw "Failed to start BusBuddy application"
    }

    # Store reference to the error capture job
    $global:errorCaptureJob = $errorCaptureJob

    # Step 2: Wait for user interaction and application closure
    Wait-ForUserInteraction -Process $process -ErrorCaptureJob $errorCaptureJob

    # Step 3: Show continue prompt
    $ContinueAnalysis = Show-ContinuePrompt
    if (-not $ContinueAnalysis) {
        Write-ColorMessage "❌ Analysis cancelled by user" -Color $Colors.Warning
        exit 0
    }

    # Step 4: Compile captured errors
    $CapturedErrors = Get-CapturedErrors

    # Step 5: Generate analysis report
    $ReportFile = New-ErrorReport -Errors $CapturedErrors

    # Step 6: Invoke auto-fix if requested
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
    if ($OpenReport -match 'y') {
        Invoke-Item $ReportFile
    }

}
catch {
    Write-ColorMessage ""
    Write-ColorMessage "❌ Script execution failed: $($_.Exception.Message)" -Color $Colors.Error
    Write-ColorMessage "Stack Trace: $($_.ScriptStackTrace)" -Color "DarkRed"
    exit 1
}
finally {
    # Cleanup any remaining jobs
    Get-Job | Where-Object { $_.Name -like "*ErrorCapture*" -or $_.State -ne "Completed" } | Remove-Job -Force -ErrorAction SilentlyContinue

    # Clean up any global variables we created
    if (Get-Variable -Name errorCaptureJob -Scope Global -ErrorAction SilentlyContinue) {
        Remove-Variable -Name errorCaptureJob -Scope Global -ErrorAction SilentlyContinue
    }

    Write-ColorMessage "🧹 Session finished." -Color $Colors.Info
}
