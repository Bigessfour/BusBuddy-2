# 📊 Repository Size Monitoring Script
# Run this periodically to ensure no large files creep into the repository

param(
    [int]$WarningSizeMB = 10,
    [int]$CriticalSizeMB = 50
)

Write-Host "🔍 Repository Size Analysis" -ForegroundColor Cyan
Write-Host "Warning threshold: $WarningSizeMB MB" -ForegroundColor Yellow
Write-Host "Critical threshold: $CriticalSizeMB MB" -ForegroundColor Red
Write-Host ""

# Check tracked files
Write-Host "📁 Analyzing tracked files..." -ForegroundColor Cyan
$trackedFiles = git ls-files | ForEach-Object {
    if (Test-Path $_) {
        Get-Item $_ | Select-Object Name, @{Name = "SizeMB"; Expression = { [math]::Round($_.Length / 1MB, 2) } }, FullName
    }
} | Where-Object { $_.SizeMB -gt 1 } | Sort-Object SizeMB -Descending

if ($trackedFiles) {
    $trackedFiles | Format-Table -AutoSize

    $warningFiles = $trackedFiles | Where-Object { $_.SizeMB -gt $WarningSizeMB }
    $criticalFiles = $trackedFiles | Where-Object { $_.SizeMB -gt $CriticalSizeMB }

    if ($criticalFiles) {
        Write-Host "🚨 CRITICAL: Files over $CriticalSizeMB MB found!" -ForegroundColor Red
        Write-Host "Consider using Git LFS for these files." -ForegroundColor Red
    }
    elseif ($warningFiles) {
        Write-Host "⚠️ WARNING: Files over $WarningSizeMB MB found!" -ForegroundColor Yellow
        Write-Host "Monitor these files for growth." -ForegroundColor Yellow
    }
    else {
        Write-Host "✅ All tracked files are reasonable size." -ForegroundColor Green
    }
}
else {
    Write-Host "✅ No large tracked files found." -ForegroundColor Green
}

# Check repository size
Write-Host ""
Write-Host "📊 Repository Statistics:" -ForegroundColor Cyan
$totalSize = (Get-ChildItem -Recurse -File | Where-Object { $_.FullName -notlike "*\.git\*" } | Measure-Object -Property Length -Sum).Sum / 1MB
$gitSize = (Get-ChildItem .git -Recurse -File | Measure-Object -Property Length -Sum).Sum / 1MB

Write-Host "Repository size (excluding .git): $([math]::Round($totalSize, 2)) MB" -ForegroundColor White
Write-Host "Git metadata size: $([math]::Round($gitSize, 2)) MB" -ForegroundColor Gray
Write-Host "Total workspace size: $([math]::Round($totalSize + $gitSize, 2)) MB" -ForegroundColor White

if ($totalSize -gt 500) {
    Write-Host "⚠️ Repository approaching 500MB - consider cleanup" -ForegroundColor Yellow
}
elseif ($totalSize -gt 1000) {
    Write-Host "🚨 Repository over 1GB - cleanup recommended" -ForegroundColor Red
}
else {
    Write-Host "✅ Repository size is healthy" -ForegroundColor Green
}

Write-Host ""
Write-Host "🎯 LFS Recommendation:" -ForegroundColor Cyan
if ($criticalFiles) {
    Write-Host "📦 Consider Git LFS setup for large files" -ForegroundColor Yellow
    Write-Host "Commands: git lfs install; git lfs track '*.dll'; git add .gitattributes" -ForegroundColor Gray
}
else {
    Write-Host "✅ Git LFS not needed - current approach is optimal" -ForegroundColor Green
}
