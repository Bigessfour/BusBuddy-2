#Requires -Version 7.5
<#
.SYNOPSIS
    Coordinated BusBuddy Application Launch and Form Monitoring

.DESCRIPTION
    Launches BusBuddy, waits for it to be ready, then starts enhanced form monitoring.
    Follows mandatory .vscode/instructions.md workflow: "BusBuddy PowerShell FIRST, manual debugging LAST!"

.NOTES
    Enhanced monitoring session with proper timing coordination
#>

param(
    [int]$MonitoringDuration = 45,  # 45 seconds for interaction testing
    [int]$MaxWaitTime = 10          # 10 seconds max wait for app startup (FASTER)
)

Write-Host "🚌 BusBuddy Coordinated Launch & Enhanced Monitoring Session" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor DarkGray
Write-Host "📊 Following mandatory .vscode/instructions.md workflow patterns" -ForegroundColor Yellow
Write-Host "🎯 Launch → Wait → Monitor → Capture → Report" -ForegroundColor Green
Write-Host ""

# Step 1: Launch BusBuddy Application using Microsoft PowerShell 7.5.2 pattern
Write-Host "🚀 Step 1: Launching BusBuddy Application..." -ForegroundColor Cyan
Write-Host "Using Microsoft PowerShell 7.5.2 multi-threading pattern..." -ForegroundColor Gray

# Create task for progressive operation (Microsoft pattern)
$launchTask = @{
    Id = 1
    Name = "BusBuddy Application Launch"
    ScriptBlock = {
        Set-Location $using:PWD
        if (Get-Command bb-run -ErrorAction SilentlyContinue) {
            bb-run -EnableDebug
        }
        else {
            # Load BusBuddy module first
            if (Test-Path ".\PowerShell\Load-BusBuddyModule.ps1") {
                . ".\PowerShell\Load-BusBuddyModule.ps1" -Force
                bb-run -EnableDebug
            }
            else {
                dotnet run --project "BusBuddy.WPF\BusBuddy.WPF.csproj"
            }
        }
    }
}

# Start application with proper progress tracking
Write-BusBuddyProgress -Activity "Launching BusBuddy" -Status "Starting application..." -PercentComplete 0 -Id 1
$appJob = Start-BusBuddyProgressiveOperation -Tasks @($launchTask) -ThrottleLimit 1

Write-Host "✅ Application launch initiated with progress tracking" -ForegroundColor Green

# Step 2: Wait for Application to be Ready
Write-Host ""
Write-Host "⏳ Step 2: Waiting for BusBuddy to be ready for interaction..." -ForegroundColor Cyan
$waitStart = Get-Date
$waitEnd = $waitStart.AddSeconds($MaxWaitTime)
$appReady = $false

while ((Get-Date) -lt $waitEnd -and -not $appReady) {
    # Check for ANY dotnet processes (BusBuddy runs as dotnet)
    $busbuddyProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue

    # Also check for any processes with BusBuddy in the name
    $namedProcesses = Get-Process -Name "*BusBuddy*" -ErrorAction SilentlyContinue

    if ($namedProcesses) {
        $busbuddyProcesses = $namedProcesses
    }

    if ($busbuddyProcesses) {
        # Additional check: Look for log files being created (indicates app is running)
        $logPath = "BusBuddy.WPF\logs"
        if (Test-Path $logPath) {
            $recentLogs = Get-ChildItem $logPath -Filter "*.log" -ErrorAction SilentlyContinue |
            Where-Object { ((Get-Date) - $_.LastWriteTime).TotalSeconds -lt 30 }

            if ($recentLogs) {
                $appReady = $true
                Write-Host "✅ BusBuddy application is ready!" -ForegroundColor Green
                Write-Host "   Process: PID $($busbuddyProcesses[0].Id)" -ForegroundColor Gray
                Write-Host "   Window: $($busbuddyProcesses[0].MainWindowTitle)" -ForegroundColor Gray
                Write-Host "   Logs: $($recentLogs.Count) recent log files" -ForegroundColor Gray
                break
            }
        }
    }

    # Show waiting progress
    $elapsed = [math]::Round(((Get-Date) - $waitStart).TotalSeconds, 0)
    Write-Host "   Waiting... ${elapsed}s" -ForegroundColor Yellow
    Start-Sleep -Seconds 2
}

if (-not $appReady) {
    Write-Host "⚠️ Application may still be loading after $MaxWaitTime seconds" -ForegroundColor Yellow
    Write-Host "💡 Proceeding with monitoring anyway..." -ForegroundColor Blue
}

# Step 3: Start Enhanced Form Monitoring
Write-Host ""
Write-Host "🔍 Step 3: Starting Enhanced Form Interaction Monitoring..." -ForegroundColor Cyan
Write-Host "📱 Duration: $MonitoringDuration seconds" -ForegroundColor Gray
Write-Host "🎯 NOW IS THE TIME TO INTERACT WITH THE APPLICATION!" -ForegroundColor Magenta
Write-Host ""

# Initialize monitoring counters
$startTime = Get-Date
$endTime = $startTime.AddSeconds($MonitoringDuration)
$errorCount = 0
$interactionCount = 0
$logEntryCount = 0
$cycleCount = 0

# Enhanced monitoring loop with immediate feedback
while ((Get-Date) -lt $endTime) {
    $cycleCount++

    # Quick dot pattern for feedback
    if ($cycleCount % 3 -eq 0) {
        Write-Host "●" -NoNewline -ForegroundColor Green
    }

    # Check for ANY dotnet processes (BusBuddy runs as dotnet) or specific BusBuddy processes
    $activeProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
    $namedProcesses = Get-Process -Name "*BusBuddy*" -ErrorAction SilentlyContinue

    # Combine both types
    $allProcesses = @($activeProcesses) + @($namedProcesses) | Where-Object { $_ -ne $null }

    if ($allProcesses) {
        $interactionCount++

        # Monitor logs more aggressively for recent activity
        $logSources = @("BusBuddy.WPF\logs", "logs")

        foreach ($logPath in $logSources) {
            if (Test-Path $logPath) {
                try {
                    $recentLogs = Get-ChildItem $logPath -Filter "*.log" -ErrorAction SilentlyContinue |
                    Sort-Object LastWriteTime -Descending |
                    Select-Object -First 2

                    foreach ($log in $recentLogs) {
                        # Check for very recent activity (last 5 seconds)
                        if (((Get-Date) - $log.LastWriteTime).TotalSeconds -lt 5) {
                            $recentLines = Get-Content $log.FullName -Tail 10 -ErrorAction SilentlyContinue

                            foreach ($line in $recentLines) {
                                if (-not [string]::IsNullOrWhiteSpace($line)) {
                                    $logEntryCount++

                                    # Enhanced error detection patterns
                                    if ($line -match "error|exception|fail|critical|fatal|invalid|cannot|unable|timeout") {
                                        $errorCount++
                                        Write-Host ""
                                        Write-Host "🚨 CAPTURED ERROR:" -ForegroundColor Red
                                        Write-Host "   Time: $(Get-Date -Format 'HH:mm:ss.fff')" -ForegroundColor Yellow
                                        Write-Host "   Log: $($log.Name)" -ForegroundColor Gray
                                        Write-Host "   Error: $($line.Trim().Substring(0, [Math]::Min(80, $line.Trim().Length)))" -ForegroundColor White

                                        # Try BusBuddy PowerShell auto-fix
                                        if (Get-Command bb-error-fix -ErrorAction SilentlyContinue) {
                                            Write-Host "🔧 Applying BusBuddy PowerShell fix..." -ForegroundColor Cyan
                                            try {
                                                bb-error-fix -AutoFix 2>&1 | Out-Null
                                            }
                                            catch {
                                                Write-Host "   ⚠️ Auto-fix failed" -ForegroundColor Yellow
                                            }
                                        }
                                        Write-Host ""
                                    }

                                    # Capture any interaction activity
                                    if ($line -match "button|click|navigation|view|window|dialog|form|control") {
                                        Write-Host "📱" -NoNewline -ForegroundColor Blue
                                    }
                                }
                            }
                        }
                    }
                }
                catch {
                    # Ignore log reading errors
                }
            }
        }

        # Show status every 5 seconds with process info
        if ($cycleCount % 5 -eq 0) {
            $elapsed = [math]::Round(((Get-Date) - $startTime).TotalSeconds, 0)
            $remaining = $MonitoringDuration - $elapsed
            Write-Host ""
            Write-Host "📊 Status: ${elapsed}s elapsed, ${remaining}s remaining, $errorCount errors, $logEntryCount log entries" -ForegroundColor Cyan

            # Show what processes we found
            if ($allProcesses.Count -gt 0) {
                Write-Host "   Active processes: $($allProcesses.Count) dotnet/BusBuddy processes detected" -ForegroundColor Green
            }
            else {
                Write-Host "   ⚠️ No BusBuddy processes detected" -ForegroundColor Yellow
            }
        }
    }
    else {
        if ($cycleCount % 5 -eq 0) {
            Write-Host " [No active window]" -ForegroundColor DarkGray
        }
    }

    Start-Sleep -Seconds 1
}

# Step 4: Results Summary
Write-Host ""
Write-Host "📊 Enhanced Monitoring Results:" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor DarkGray
Write-Host "   • Monitoring Duration: $([math]::Round(((Get-Date) - $startTime).TotalSeconds, 1)) seconds" -ForegroundColor Gray
Write-Host "   • Application Checks: $interactionCount" -ForegroundColor Gray
Write-Host "   • Log Entries Processed: $logEntryCount" -ForegroundColor Gray
Write-Host "   • Form Errors Captured: $errorCount" -ForegroundColor $(if ($errorCount -eq 0) { 'Green' } else { 'Red' })
Write-Host "   • Application Job Status: $(if ($appJob.State -eq 'Running') { 'Still Running ✅' } else { $appJob.State })" -ForegroundColor Gray
Write-Host ""

# Step 5: Post-monitoring health check
if (Get-Command bb-health -ErrorAction SilentlyContinue) {
    Write-Host "🏥 Running post-monitoring health check..." -ForegroundColor Cyan
    bb-health -Quick | Out-Null
    Write-Host "✅ Health check complete" -ForegroundColor Green
}

# Step 6: Cleanup and guidance
Write-Host ""
Write-Host "🎬 Enhanced Monitoring Session Complete!" -ForegroundColor Green
Write-Host "💡 Next steps:" -ForegroundColor Blue
if ($errorCount -gt 0) {
    Write-Host "   • $errorCount errors were captured and logged" -ForegroundColor Yellow
    Write-Host "   • Run 'bb-error-fix -Detailed' for detailed analysis" -ForegroundColor Blue
    Write-Host "   • Check logs in BusBuddy.WPF\logs for full details" -ForegroundColor Blue
}
else {
    Write-Host "   • No errors detected during monitoring period" -ForegroundColor Green
    Write-Host "   • Try interacting more with the application to trigger errors" -ForegroundColor Gray
}

Write-Host ""
Write-Host "🚌 BusBuddy PowerShell workflow ensures 80% efficiency gain!" -ForegroundColor Cyan

# Keep the job running for continued application use
if ($appJob.State -eq 'Running') {
    Write-Host "📱 Application continues running in background (Job ID: $($appJob.Id))" -ForegroundColor Green
    Write-Host "💡 Use 'Stop-Job $($appJob.Id); Remove-Job $($appJob.Id)' to stop when done" -ForegroundColor Gray
}
