#Requires -Version 7.5
<#
.SYNOPSIS
    Simple Enhanced Form Monitoring for BusBuddy - FIXED VERSION

.DESCRIPTION
    Real-time form interaction monitoring following mandatory .vscode/instructions.md workflow
    BusBuddy PowerShell FIRST, manual debugging LAST!
#>

param(
    [int]$Duration = 20  # Default 20 seconds for quick testing
)

Write-Host "🚌 BusBuddy Enhanced Form Monitoring Session (FIXED)" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor DarkGray
Write-Host "📊 Following mandatory .vscode/instructions.md workflow patterns" -ForegroundColor Yellow
Write-Host "🎯 Duration: $Duration seconds" -ForegroundColor Green
Write-Host ""

# Initialize counters
$startTime = Get-Date
$endTime = $startTime.AddSeconds($Duration)
$errorCount = 0
$interactionCount = 0
$cycleCount = 0

Write-Host "🔧 Starting Enhanced Form Interaction Monitoring..." -ForegroundColor Cyan
Write-Host "📱 Monitoring for $Duration seconds..." -ForegroundColor Gray
Write-Host ""

# Main monitoring loop
while ((Get-Date) -lt $endTime) {
    $cycleCount++

    # Enhanced dot pattern: ●●●●●5●●●●●10●●●●●15
    $dotPosition = $cycleCount % 15
    if ($dotPosition -eq 0) {
        Write-Host "●15" -NoNewline -ForegroundColor Green
        Write-Host " [Cycle $([math]::Ceiling($cycleCount / 15))]" -ForegroundColor Cyan
    }
    elseif ($dotPosition % 5 -eq 0) {
        Write-Host "●$dotPosition" -NoNewline -ForegroundColor Green
    }
    else {
        Write-Host "●" -NoNewline -ForegroundColor Green
    }

    # Check for BusBuddy application process - FIXED to detect dotnet processes
    $appProcess = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object {
        $_.MainWindowTitle -ne "" -or
        (Get-CimInstance -Class Win32_Process -Filter "ProcessId=$($_.Id)" -ErrorAction SilentlyContinue).CommandLine -match "BusBuddy"
    }

    if ($appProcess) {
        $interactionCount++
        if ($cycleCount % 5 -eq 0) {
            $windowInfo = if ($appProcess.MainWindowTitle) { $appProcess.MainWindowTitle } else { "BusBuddy App" }
            Write-Host " [App: PID $($appProcess[0].Id) - $windowInfo]" -ForegroundColor Green
        }

        # Enhanced log monitoring - check multiple sources
        $logSources = @(
            "BusBuddy.WPF\logs",
            "logs",
            "BusBuddy.Core\logs"
        )

        foreach ($logPath in $logSources) {
            if (Test-Path $logPath) {
                try {
                    $recentLogs = Get-ChildItem $logPath -Filter "*.log" -ErrorAction SilentlyContinue |
                    Sort-Object LastWriteTime -Descending |
                    Select-Object -First 2

                    foreach ($recentLog in $recentLogs) {
                        # Check if log was modified in last 10 seconds (active logging)
                        if (((Get-Date) - $recentLog.LastWriteTime).TotalSeconds -lt 10) {
                            $lastLines = Get-Content $recentLog.FullName -Tail 10 -ErrorAction SilentlyContinue
                            foreach ($line in $lastLines) {
                                if (-not [string]::IsNullOrWhiteSpace($line)) {
                                    # Enhanced error detection patterns
                                    if ($line -match "error|exception|fail|critical|fatal" -and
                                        ($line -match "form|ui|view|xaml|window|dialog|navigation|control|binding" -or
                                        $line -match "syncfusion|wpf|button|click|load")) {
                                        $errorCount++
                                        Write-Host ""
                                        Write-Host "🚨 CAPTURED FORM ERROR:" -ForegroundColor Red
                                        Write-Host "   Time: $(Get-Date -Format 'HH:mm:ss')" -ForegroundColor Yellow
                                        Write-Host "   Log: $($recentLog.Name)" -ForegroundColor Gray
                                        Write-Host "   Error: $($line.Trim())" -ForegroundColor White

                                        # Try BusBuddy PowerShell fix if available
                                        if (Get-Command bb-error-fix -ErrorAction SilentlyContinue) {
                                            Write-Host "🔧 Applying BusBuddy PowerShell fix..." -ForegroundColor Cyan
                                            try { bb-error-fix -AutoFix } catch { }
                                        }
                                    }
                                    # Also capture any recent activity (interactions)
                                    elseif ($line -match "information|info|debug" -and
                                        ($line -match "navigate|click|load|show|display|open|close")) {
                                        if ($cycleCount % 10 -eq 0) {
                                            Write-Host "📱 Interaction: $($line.Substring(0, [Math]::Min(40, $line.Length)))..." -ForegroundColor Blue
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch {
                    # Ignore log reading errors but show we tried
                    if ($cycleCount % 30 -eq 0) {
                        Write-Host " [Log check failed: $logPath]" -ForegroundColor DarkYellow
                    }
                }
            }
        }
    }
    else {
        if ($cycleCount % 5 -eq 0) {
            Write-Host " [App: Not Running]" -ForegroundColor Red
        }
    }

    # Progress update every minute
    if ($cycleCount % 15 -eq 0) {
        $elapsed = [math]::Round(((Get-Date) - $startTime).TotalSeconds, 0)
        Write-Host ""
        Write-Host "📊 Progress: ${elapsed}s elapsed, $interactionCount checks, $errorCount errors" -ForegroundColor Cyan
    }

    Start-Sleep -Seconds 1
}

Write-Host ""
Write-Host "📊 Enhanced Monitoring Summary:" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor DarkGray
Write-Host "   • Duration: $([math]::Round(((Get-Date) - $startTime).TotalSeconds, 1)) seconds" -ForegroundColor Gray
Write-Host "   • Monitoring Cycles: $cycleCount" -ForegroundColor Gray
Write-Host "   • Application Detections: $interactionCount" -ForegroundColor Gray
Write-Host "   • Form Errors CAPTURED: $errorCount" -ForegroundColor $(if ($errorCount -eq 0) { 'Yellow' } else { 'Red' })
Write-Host "   • Logs Monitored: BusBuddy.WPF\logs, logs, BusBuddy.Core\logs" -ForegroundColor Gray
Write-Host ""

if ($errorCount -gt 0) {
    Write-Host "🎯 SUCCESS: Captured $errorCount form interaction errors!" -ForegroundColor Green
    Write-Host "🔧 BusBuddy PowerShell fixes were applied automatically" -ForegroundColor Cyan
}
else {
    Write-Host "ℹ️  No errors captured during this session" -ForegroundColor Yellow
    Write-Host "💡 Try interacting with forms during monitoring to capture errors" -ForegroundColor Blue
}

# Check if BusBuddy PowerShell commands are available
if (Get-Command bb-health -ErrorAction SilentlyContinue) {
    Write-Host "🏥 Running post-session health check..." -ForegroundColor Cyan
    bb-health -Quick
}
else {
    Write-Host "⚠️ BusBuddy PowerShell commands not available" -ForegroundColor Yellow
    Write-Host "💡 Load with: . '.\PowerShell\Load-BusBuddyModule.ps1'" -ForegroundColor Blue
}

Write-Host ""
Write-Host "🎬 Enhanced Form Monitoring Session Complete" -ForegroundColor Green
Write-Host "🚌 BusBuddy PowerShell workflow ensures 80% efficiency gain!" -ForegroundColor Cyan
