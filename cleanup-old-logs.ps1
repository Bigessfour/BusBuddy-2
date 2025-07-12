# Log Consolidation Cleanup Script
# This script removes old individual log files since we're now using consolidated logging

Write-Host '🧹 BusBuddy Log Consolidation Cleanup' -ForegroundColor Green
Write-Host '=====================================' -ForegroundColor Green

$logsDir = 'C:\Users\steve.mckitrick\Desktop\Bus Buddy\logs'

if (-not (Test-Path $logsDir)) {
    Write-Host "❌ Logs directory not found: $logsDir" -ForegroundColor Red
    exit 1
}

# Files to clean up (old separated log files)
$oldLogPatterns = @(
    'performance-*.log'
    'ui-operations-*.log'
    'busbuddy-2025*.log'  # Old main logs
    'errors-2025*.log'    # Old error logs (we now use busbuddy-errors-)
)

Write-Host "`n🔍 Scanning for old log files to clean up..." -ForegroundColor Yellow

$totalFilesRemoved = 0
$totalSizeFreed = 0

foreach ($pattern in $oldLogPatterns) {
    $files = Get-ChildItem -Path $logsDir -Name $pattern -ErrorAction SilentlyContinue

    if ($files) {
        Write-Host "`n📁 Found $($files.Count) files matching: $pattern" -ForegroundColor Cyan

        foreach ($file in $files) {
            $fullPath = Join-Path $logsDir $file
            $fileInfo = Get-Item $fullPath
            $fileSizeMB = [math]::Round($fileInfo.Length / 1MB, 2)

            Write-Host "   🗑️  Removing: $file ($fileSizeMB MB)" -ForegroundColor Gray
            Remove-Item $fullPath -Force

            $totalFilesRemoved++
            $totalSizeFreed += $fileSizeMB
        }
    } else {
        Write-Host "✅ No files found for pattern: $pattern" -ForegroundColor Green
    }
}

# Show what's left
Write-Host "`n📊 Cleanup Summary:" -ForegroundColor Blue
Write-Host "Files removed: $totalFilesRemoved" -ForegroundColor White
Write-Host "Space freed: $([math]::Round($totalSizeFreed, 2)) MB" -ForegroundColor White

Write-Host "`n📋 Remaining log files:" -ForegroundColor Blue
$remainingLogs = Get-ChildItem -Path $logsDir -Name '*.log' | Sort-Object
foreach ($log in $remainingLogs) {
    $fullPath = Join-Path $logsDir $log
    $fileInfo = Get-Item $fullPath
    $fileSizeMB = [math]::Round($fileInfo.Length / 1MB, 2)
    Write-Host "   📄 $log ($fileSizeMB MB)" -ForegroundColor White
}

Write-Host "`n✅ Log consolidation cleanup complete!" -ForegroundColor Green
Write-Host 'Now using 2 consolidated log files:' -ForegroundColor White
Write-Host '   • busbuddy-consolidated-YYYY-MM-DD.log (main application logs)' -ForegroundColor Cyan
Write-Host '   • busbuddy-errors-YYYY-MM-DD.log (warnings and errors only)' -ForegroundColor Red
