# BusBuddy Runtime Logging Script
# Captures all output (build + runtime) to log files for debugging

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$logDir = ".\logs"
$buildLog = "$logDir\build-$timestamp.log"
$runtimeLog = "$logDir\runtime-$timestamp.log"
$fullLog = "$logDir\full-$timestamp.log"

# Ensure logs directory exists
if (-not (Test-Path $logDir)) {
    New-Item -ItemType Directory -Path $logDir -Force | Out-Null
}

Write-Host "🚌 Starting BusBuddy with comprehensive logging..." -ForegroundColor Cyan
Write-Host "📝 Logs will be saved to:" -ForegroundColor Yellow
Write-Host "   Build:   $buildLog" -ForegroundColor Gray
Write-Host "   Runtime: $runtimeLog" -ForegroundColor Gray
Write-Host "   Full:    $fullLog" -ForegroundColor Gray
Write-Host ""

try {
    # Step 1: Build with logging
    Write-Host "🔨 Building application..." -ForegroundColor Cyan
    $buildOutput = dotnet build "BusBuddy.WPF\BusBuddy.WPF.csproj" --verbosity detailed 2>&1
    $buildOutput | Out-File -FilePath $buildLog -Encoding UTF8
    $buildOutput | Out-File -FilePath $fullLog -Encoding UTF8

    $buildExitCode = $LASTEXITCODE
    Write-Host "Build exit code: $buildExitCode" -ForegroundColor $(if ($buildExitCode -eq 0) { "Green" } else { "Red" })

    if ($buildExitCode -eq 0) {
        Write-Host "✅ Build successful, starting application..." -ForegroundColor Green
        Write-Host ""

        # Step 2: Run with detailed logging
        Write-Host "🚀 Running application with full output capture..." -ForegroundColor Cyan

        # Add separator to logs
        "=== APPLICATION STARTUP ===" | Out-File -FilePath $runtimeLog -Encoding UTF8
        "=== APPLICATION STARTUP ===" | Out-File -FilePath $fullLog -Encoding UTF8 -Append

        # Run application and capture ALL output
        $runtimeOutput = dotnet run --project "BusBuddy.WPF\BusBuddy.WPF.csproj" --verbosity detailed 2>&1
        $runtimeOutput | Out-File -FilePath $runtimeLog -Encoding UTF8 -Append
        $runtimeOutput | Out-File -FilePath $fullLog -Encoding UTF8 -Append

        $runtimeExitCode = $LASTEXITCODE
        Write-Host "Runtime exit code: $runtimeExitCode" -ForegroundColor $(if ($runtimeExitCode -eq 0) { "Green" } else { "Red" })

        # Display last few lines of runtime log
        Write-Host ""
        Write-Host "📋 Last 10 lines of runtime output:" -ForegroundColor Yellow
        Get-Content $runtimeLog | Select-Object -Last 10 | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }

    }
    else {
        Write-Host "❌ Build failed, not attempting to run" -ForegroundColor Red
        # Display last few lines of build log
        Write-Host ""
        Write-Host "📋 Last 10 lines of build output:" -ForegroundColor Yellow
        Get-Content $buildLog | Select-Object -Last 10 | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
    }

}
catch {
    $errorMsg = "PowerShell error: $($_.Exception.Message)"
    Write-Host $errorMsg -ForegroundColor Red
    $errorMsg | Out-File -FilePath $fullLog -Encoding UTF8 -Append
}

Write-Host ""
Write-Host "📁 Full logs available at:" -ForegroundColor Cyan
Write-Host "   $buildLog" -ForegroundColor Gray
Write-Host "   $runtimeLog" -ForegroundColor Gray
Write-Host "   $fullLog" -ForegroundColor Gray
