# Debug BusBuddy Application Startup
# This script runs the application with console output capture

Write-Host "🔍 DEBUGGING BUSBUDDY APPLICATION STARTUP" -ForegroundColor Cyan

# Set working directory
Set-Location -Path $PSScriptRoot

# Enable verbose error reporting
$ErrorActionPreference = "Continue"
$VerbosePreference = "Continue"

Write-Host "`n📁 Current Directory: $(Get-Location)" -ForegroundColor Yellow

Write-Host "`n🔧 Checking .NET Runtime..." -ForegroundColor Yellow
dotnet --version

Write-Host "`n🏗️ Building solution..." -ForegroundColor Yellow
$buildResult = dotnet build BusBuddy.sln 2>&1
Write-Host $buildResult

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed with exit code: $LASTEXITCODE" -ForegroundColor Red
    exit 1
}

Write-Host "`n🚀 Running application with output capture..." -ForegroundColor Yellow
Write-Host "📝 All output will be captured and displayed..." -ForegroundColor Gray

# Run with output capture and timeout
$timeout = 10  # seconds
$process = Start-Process -FilePath "dotnet" -ArgumentList "run --project BusBuddy.WPF/BusBuddy.WPF.csproj" -PassThru -RedirectStandardOutput -RedirectStandardError -WindowStyle Hidden

$output = @()
$errors = @()

# Wait with timeout
if ($process.WaitForExit($timeout * 1000)) {
    Write-Host "`n✅ Process completed with exit code: $($process.ExitCode)" -ForegroundColor Green
}
else {
    Write-Host "`n⏰ Process timed out after $timeout seconds" -ForegroundColor Yellow
    $process.Kill()
}

# Try to read output
try {
    $stdout = $process.StandardOutput.ReadToEnd()
    $stderr = $process.StandardError.ReadToEnd()

    if ($stdout) {
        Write-Host "`n📋 STANDARD OUTPUT:" -ForegroundColor Green
        Write-Host $stdout
    }

    if ($stderr) {
        Write-Host "`n❌ STANDARD ERROR:" -ForegroundColor Red
        Write-Host $stderr
    }

    if (-not $stdout -and -not $stderr) {
        Write-Host "`n⚠️ No output captured - application may have failed silently" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "`n❌ Error reading process output: $_" -ForegroundColor Red
}

Write-Host "`n🔍 Checking for log files..." -ForegroundColor Yellow
$logDirs = @(
    ".\logs",
    ".\BusBuddy.WPF\bin\Debug\net8.0-windows\logs",
    ".\BusBuddy.WPF\logs"
)

foreach ($logDir in $logDirs) {
    if (Test-Path $logDir) {
        Write-Host "📁 Found logs in: $logDir" -ForegroundColor Green
        Get-ChildItem -Path $logDir -Filter "*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 3 | ForEach-Object {
            Write-Host "  📄 $($_.Name) ($('{0:yyyy-MM-dd HH:mm:ss}' -f $_.LastWriteTime))" -ForegroundColor Gray
        }
    }
}

Write-Host "`n🏁 Debug session completed" -ForegroundColor Cyan
