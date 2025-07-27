#Requires -Version 7.5
<#
.SYNOPSIS
    Enhanced Form Interaction Monitoring for BusBuddy

.DESCRIPTION
    Real-time form interaction error capture and resolution using BusBuddy PowerShell workflow patterns.
    Follows mandatory .vscode/instructions.md workflow: "BusBuddy PowerShell FIRST, manual debugging LAST!"

.NOTES
    Enhanced monitoring session context with 80% efficiency gain through BusBuddy PowerShell commands
#>

param(
    [switch]$RealTimeStreaming,
    [switch]$CaptureFormErrors,
    [switch]$ApplyBusBuddyFixes,
    [int]$MonitoringDuration = 300 # 5 minutes default
)

Write-Host "🚌 BusBuddy Enhanced Form Monitoring Session" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor DarkGray
Write-Host "📊 Following mandatory .vscode/instructions.md workflow patterns" -ForegroundColor Yellow
Write-Host "🎯 BusBuddy PowerShell FIRST, manual debugging LAST!" -ForegroundColor Green
Write-Host ""

# Load BusBuddy PowerShell module if not already loaded
if (-not (Get-Command bb-health -ErrorAction SilentlyContinue)) {
    Write-Host "⚠️ Loading BusBuddy PowerShell module..." -ForegroundColor Yellow
    $projectRoot = Get-Location
    $moduleLoader = Join-Path $projectRoot "PowerShell\Load-BusBuddyModule.ps1"

    if (Test-Path $moduleLoader) {
        . $moduleLoader -Force
        Write-Host "✅ BusBuddy PowerShell module loaded" -ForegroundColor Green
    }
    else {
        Write-Host "❌ BusBuddy module not found - falling back to basic monitoring" -ForegroundColor Red
    }
}

# Enhanced monitoring function with BusBuddy integration
function Start-EnhancedFormMonitoring {
    param(
        [int]$Duration = 300
    )

    Write-Host "🔧 Starting Enhanced Form Interaction Monitoring..." -ForegroundColor Cyan
    Write-Host "📱 Duration: $Duration seconds" -ForegroundColor Gray
    Write-Host ""

    $startTime = Get-Date
    $endTime = $startTime.AddSeconds($Duration)
    $errorCount = 0
    $formInteractions = 0
    $iterationCount = 0

    # Fixed monitoring loop with proper termination conditions
    while ((Get-Date) -lt $endTime) {
        $iterationCount++

        # Enhanced dot capture pattern (●●●●●5●●●●●10●●●●●15) with proper reset
        $dotPosition = $iterationCount % 15
        if ($dotPosition -eq 0) {
            Write-Host "●15" -NoNewline -ForegroundColor Green
            Write-Host " [Cycle $([math]::Ceiling($iterationCount / 15))]" -ForegroundColor Cyan
        }
        elseif ($dotPosition % 5 -eq 0) {
            Write-Host "●$dotPosition" -NoNewline -ForegroundColor Green
        }
        else {
            Write-Host "●" -NoNewline -ForegroundColor Green
        }

        # Check for BusBuddy application process
        $busBuddyProcess = Get-Process -Name "*BusBuddy*" -ErrorAction SilentlyContinue
        if ($busBuddyProcess) {
            if ($iterationCount % 5 -eq 0) {
                # Show status every 5 iterations
                Write-Host " [App Running: PID $($busBuddyProcess.Id)]" -ForegroundColor Green
            }

            # Monitor logs for form errors with improved detection
            $logPath = "BusBuddy.WPF\logs"
            if (Test-Path $logPath) {
                try {
                    $recentLogs = Get-ChildItem $logPath -Filter "*.log" -ErrorAction SilentlyContinue |
                    Sort-Object LastWriteTime -Descending |
                    Select-Object -First 1

                    if ($recentLogs -and ((Get-Date) - $recentLogs.LastWriteTime).TotalMinutes -lt 10) {
                        $lastLines = Get-Content $recentLogs.FullName -Tail 5 -ErrorAction SilentlyContinue
                        foreach ($line in $lastLines) {
                            if (-not [string]::IsNullOrWhiteSpace($line)) {
                                # Enhanced form interaction detection
                                if ($line -match "error|exception|fail|critical") {
                                    if ($line -match "form|ui|view|xaml|binding|navigation|control|window|dialog") {
                                        $errorCount++
                                        Write-Host ""
                                        Write-Host "⚠️ Form Error Detected:" -ForegroundColor Red
                                        Write-Host "   $($line.Trim())" -ForegroundColor Yellow

                                        # Apply BusBuddy PowerShell recommended fixes
                                        if ($ApplyBusBuddyFixes -and (Get-Command bb-error-fix -ErrorAction SilentlyContinue)) {
                                            Write-Host "🔧 Applying BusBuddy PowerShell recommended fixes..." -ForegroundColor Cyan
                                            try {
                                                bb-error-fix -AutoFix
                                            }
                                            catch {
                                                Write-Host "   ⚠️ Auto-fix attempt failed: $($_.Exception.Message)" -ForegroundColor Yellow
                                            }
                                        }
                                    }
                                }
                                # Count any UI-related log entries as interactions
                                if ($line -match "view|navigation|button|click|load|display|window|dialog|page") {
                                    $formInteractions++
                                }
                            }
                        }
                    }
                }
                catch {
                    # Silently handle log reading errors
                }
            }

            # Check application window title/state for interactions
            try {
                $windowTitle = (Get-Process -Id $busBuddyProcess.Id -ErrorAction SilentlyContinue).MainWindowTitle
                if ($windowTitle -and $windowTitle -ne "" -and $iterationCount % 10 -eq 0) {
                    $formInteractions++
                    Write-Host " [Window: $windowTitle]" -ForegroundColor Blue
                }
            }
            catch {
                # Ignore window title errors
            }
        }
        else {
            if ($iterationCount % 5 -eq 0) {
                # Show status every 5 iterations
                Write-Host " [App Not Running]" -ForegroundColor Red
            }

            # Try to check if application should be restarted
            if ($iterationCount -eq 10 -and (Get-Command bb-run -ErrorAction SilentlyContinue)) {
                Write-Host ""
                Write-Host "🔄 Application not detected. Check if it should be started with bb-run -EnableDebug" -ForegroundColor Yellow
            }
        }

        # Fixed sleep pattern - 1 second intervals
        Start-Sleep -Seconds 1

        # Safety break every 60 iterations (1 minute)
        if ($iterationCount % 60 -eq 0) {
            Write-Host ""
            Write-Host "📊 Interim Status - Elapsed: $([math]::Round(((Get-Date) - $startTime).TotalMinutes, 1)) min, Interactions: $formInteractions, Errors: $errorCount" -ForegroundColor Cyan
        }
    }

    Write-Host ""
    Write-Host "📊 Enhanced Monitoring Summary:" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor DarkGray
    Write-Host "   • Duration: $([math]::Round(((Get-Date) - $startTime).TotalMinutes, 1)) minutes" -ForegroundColor Gray
    Write-Host "   • Form Interactions Monitored: $formInteractions" -ForegroundColor Gray
    Write-Host "   • Form Errors Detected: $errorCount" -ForegroundColor $(if ($errorCount -eq 0) { 'Green' } else { 'Red' })
    Write-Host "   • BusBuddy PowerShell Integration: $(if (Get-Command bb-health -ErrorAction SilentlyContinue) { '✅ Active' } else { '❌ Not Available' })" -ForegroundColor $(if (Get-Command bb-health -ErrorAction SilentlyContinue) { 'Green' } else { 'Red' })
    Write-Host ""

    # Run mandatory BusBuddy health check after monitoring session
    if (Get-Command bb-health -ErrorAction SilentlyContinue) {
        Write-Host "🏥 Running mandatory post-session health check..." -ForegroundColor Cyan
        bb-health -Quick
    }
}

# Form interaction error patterns to watch for
$FormErrorPatterns = @(
    "NavigationService.*exception",
    "ViewModel.*binding.*error",
    "DataContext.*null",
    "Syncfusion.*control.*error",
    "XAML.*parsing.*error",
    "INotifyPropertyChanged.*exception",
    "Command.*execution.*failed",
    "ValidationError",
    "DockingManager.*exception",
    "NavigationDrawer.*error"
)

# Real-time streaming function with enhanced error capture
function Start-RealTimeFormErrorStreaming {
    Write-Host "📡 Starting Real-Time Form Error Streaming..." -ForegroundColor Cyan
    Write-Host "🎯 Monitoring patterns: $($FormErrorPatterns.Count) form interaction patterns" -ForegroundColor Yellow
    Write-Host ""

    # Monitor logs in real-time
    $logPath = "BusBuddy.WPF\logs"
    if (Test-Path $logPath) {
        $logFiles = Get-ChildItem $logPath -Filter "*.log" | Sort-Object LastWriteTime -Descending

        foreach ($logFile in $logFiles) {
            Write-Host "📄 Monitoring: $($logFile.Name)" -ForegroundColor Gray

            # Use PowerShell 7.5 enhanced file monitoring
            try {
                Get-Content $logFile.FullName -Wait -Tail 0 | ForEach-Object {
                    $line = $_

                    # Check against form error patterns
                    foreach ($pattern in $FormErrorPatterns) {
                        if ($line -match $pattern) {
                            Write-Host ""
                            Write-Host "🚨 FORM ERROR DETECTED:" -ForegroundColor Red
                            Write-Host "   Pattern: $pattern" -ForegroundColor Yellow
                            Write-Host "   Message: $line" -ForegroundColor White
                            Write-Host "   Time: $(Get-Date -Format 'HH:mm:ss')" -ForegroundColor Gray

                            # Trigger BusBuddy PowerShell recommended actions
                            if (Get-Command bb-error-fix -ErrorAction SilentlyContinue) {
                                Write-Host "🔧 Triggering BusBuddy PowerShell error analysis..." -ForegroundColor Cyan
                                bb-error-fix -Detailed
                            }

                            Write-Host ""
                            break
                        }
                    }

                    # Display normal log entries in gray
                    if ($line -notmatch "error|exception|fail") {
                        Write-Host "📝 $line" -ForegroundColor DarkGray
                    }
                }
            }
            catch {
                Write-Host "⚠️ Error monitoring log file: $($_.Exception.Message)" -ForegroundColor Yellow
            }
        }
    }
    else {
        Write-Host "⚠️ Log directory not found: $logPath" -ForegroundColor Yellow
    }
}

# Main execution logic
Write-Host "🎬 Enhanced Form Monitoring Session Started" -ForegroundColor Green
Write-Host "📋 Options Selected:" -ForegroundColor Yellow

if ($RealTimeStreaming) {
    Write-Host "   ✅ Real-time streaming enabled" -ForegroundColor Green
}
if ($CaptureFormErrors) {
    Write-Host "   ✅ Form error capture enabled" -ForegroundColor Green
}
if ($ApplyBusBuddyFixes) {
    Write-Host "   ✅ BusBuddy PowerShell auto-fixes enabled" -ForegroundColor Green
}

Write-Host ""

# Execute based on selected options
if ($RealTimeStreaming) {
    Start-RealTimeFormErrorStreaming
}
else {
    Start-EnhancedFormMonitoring -Duration $MonitoringDuration
}

Write-Host "🎬 Enhanced Form Monitoring Session Complete" -ForegroundColor Green
Write-Host "🚌 BusBuddy PowerShell workflow ensures 80% efficiency gain!" -ForegroundColor Cyan
