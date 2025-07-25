# ========================================
# BusBuddy Quick Launch Script - Phase 1 Enhanced
# ========================================

param(
    [switch]$SeedData,
    [switch]$Migrate,
    [switch]$HealthCheck,
    [switch]$Quiet
)

$ErrorActionPreference = 'Continue'
$startTime = Get-Date

if (!$Quiet) {
    Write-Host '🚌 BusBuddy Quick Launch - Phase 1' -ForegroundColor Cyan
    Write-Host ('═' * 50) -ForegroundColor Gray
    Write-Host ('⏰ Started: ' + $startTime.ToString('HH:mm:ss')) -ForegroundColor Yellow
    Write-Host ''
}

# Set working directory to script location
Set-Location $PSScriptRoot

# Ensure logs directory exists
if (!(Test-Path "logs")) {
    New-Item -ItemType Directory -Path "logs" -Force | Out-Null
}

# Log startup
$logFile = "logs/run-log.txt"
$logEntry = "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - BusBuddy launch started"
Add-Content -Path $logFile -Value $logEntry

# Health check if requested
if ($HealthCheck) {
    if (!$Quiet) { Write-Host '🏥 Running Phase 1 health check...' -ForegroundColor Green }

    $healthChecks = @(
        @{ Name = "Solution file"; Path = "BusBuddy.sln" },
        @{ Name = "WPF project"; Path = "BusBuddy.WPF\BusBuddy.WPF.csproj" },
        @{ Name = "Core project"; Path = "BusBuddy.Core\BusBuddy.Core.csproj" },
        @{ Name = "Driver model"; Path = "BusBuddy.Core\Models\Driver.cs" },
        @{ Name = "Vehicle model"; Path = "BusBuddy.Core\Models\Vehicle.cs" },
        @{ Name = "Activity model"; Path = "BusBuddy.Core\Models\Activity.cs" }
    )

    $allHealthy = $true
    foreach ($check in $healthChecks) {
        if (Test-Path $check.Path) {
            if (!$Quiet) { Write-Host ("  ✅ " + $check.Name) -ForegroundColor Green }
        } else {
            if (!$Quiet) { Write-Host ("  ❌ " + $check.Name + " missing") -ForegroundColor Red }
            $allHealthy = $false
        }
    }

    if (!$allHealthy) {
        Write-Host "❌ Health check failed - missing Phase 1 components" -ForegroundColor Red
        exit 1
    }

    if (!$Quiet) { Write-Host "✅ Phase 1 health check passed" -ForegroundColor Green; Write-Host "" }
}

# Run migrations if requested
if ($Migrate) {
    if (!$Quiet) { Write-Host '🗄️ Running database migrations...' -ForegroundColor Cyan }

    $migrateResult = dotnet ef database update --project BusBuddy.Core --startup-project BusBuddy.WPF 2>&1
    if ($LASTEXITCODE -eq 0) {
        if (!$Quiet) { Write-Host "✅ Database migrations completed" -ForegroundColor Green }
        Add-Content -Path $logFile -Value "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - Database migrations completed successfully"
    } else {
        if (!$Quiet) { Write-Host "❌ Database migration failed" -ForegroundColor Red }
        Add-Content -Path $logFile -Value "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - Database migration failed: $migrateResult"
    }

    if (!$Quiet) { Write-Host "" }
}

# Seed data if requested (Phase 1 focused)
if ($SeedData) {
    if (!$Quiet) { Write-Host '🌱 Seeding Phase 1 data...' -ForegroundColor Yellow }

    if (!$Quiet) {
        Write-Host "  📊 Drivers: 15-20 sample drivers" -ForegroundColor Gray
        Write-Host "  🚌 Vehicles: 10-15 sample vehicles" -ForegroundColor Gray
        Write-Host "  📅 Activities: 25-30 sample sports/event schedules" -ForegroundColor Gray
        Write-Host "  ⚠️  Data seeding implementation pending" -ForegroundColor Yellow
    }

    Add-Content -Path $logFile -Value "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - Data seeding requested (implementation pending)"
    if (!$Quiet) { Write-Host "" }
}

# Check if project exists
if (-not (Test-Path 'BusBuddy.WPF\BusBuddy.WPF.csproj')) {
    Write-Host '❌ BusBuddy.WPF project not found!' -ForegroundColor Red
    Write-Host '   Expected: BusBuddy.WPF\BusBuddy.WPF.csproj' -ForegroundColor Yellow
    Add-Content -Path $logFile -Value "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - Error: WPF project not found"
    exit 1
}

# Build check
if (!$Quiet) { Write-Host '🔨 Checking build status...' -ForegroundColor Cyan }

$buildResult = dotnet build BusBuddy.sln --verbosity quiet --nologo 2>&1
if ($LASTEXITCODE -eq 0) {
    if (!$Quiet) { Write-Host "✅ Build successful" -ForegroundColor Green }
    Add-Content -Path $logFile -Value "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - Build successful"
} else {
    if (!$Quiet) {
        Write-Host "❌ Build failed" -ForegroundColor Red
        Write-Host "Error: $buildResult" -ForegroundColor Red
    }
    Add-Content -Path $logFile -Value "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - Build failed: $buildResult"
    exit 1
}

# Set environment
$env:DOTNET_ENVIRONMENT = "Development"

# Launch application
if (!$Quiet) {
    Write-Host ""
    Write-Host '🚀 Launching BusBuddy...' -ForegroundColor Green
    Write-Host "📍 Phase 1 Navigation: Dashboard → Drivers/Vehicles/Activities" -ForegroundColor Magenta
    Write-Host ""
}

Add-Content -Path $logFile -Value "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - Application launch initiated"

# Use & operator to avoid parameter binding issues
if (!$Quiet) { Write-Host '🚀 Executing: dotnet run --project BusBuddy.WPF\BusBuddy.WPF.csproj' -ForegroundColor Yellow }
if (!$Quiet) { Write-Host '' }

try {
    # Use call operator to avoid PowerShell parsing issues
    & dotnet run --project "BusBuddy.WPF\BusBuddy.WPF.csproj"

    $exitCode = $LASTEXITCODE
    if (!$Quiet) { Write-Host '' }
    if ($exitCode -eq 0) {
        if (!$Quiet) { Write-Host '✅ Application exited successfully' -ForegroundColor Green }
        Add-Content -Path $logFile -Value "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - Application completed successfully"
    } else {
        if (!$Quiet) { Write-Host "❌ Application exited with code: $exitCode" -ForegroundColor Red }
        Add-Content -Path $logFile -Value "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - Application exited with code: $exitCode"
    }
}
catch {
    if (!$Quiet) {
        Write-Host '❌ Error running application:' -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
    Add-Content -Path $logFile -Value "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - Launch error: $($_.Exception.Message)"
}

if (!$Quiet) { Write-Host '' }
if (!$Quiet) { Write-Host '📊 Phase 1 run complete' -ForegroundColor Cyan }
