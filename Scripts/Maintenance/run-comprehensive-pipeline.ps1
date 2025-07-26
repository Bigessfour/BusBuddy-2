# BusBuddy Comprehensive Build & Run Pipeline
# Industry-standard pipeline with full logging and error analysis

param(
    [switch]$Quiet
)

$ErrorActionPreference = 'Continue'
$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$logDir = Join-Path $PSScriptRoot 'logs'
$logFile = Join-Path $logDir "build-run-pipeline-$timestamp.log"

# Ensure logs directory exists
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

# Logging function
function Write-Log {
    param(
        [string]$Message,
        [string]$Level = 'INFO'
    )
    $entry = "[$timestamp] [$Level] $Message"
    if (-not $Quiet) {
        Write-Host $entry
    }
    $entry | Out-File -FilePath $logFile -Append -Encoding UTF8
}

# Pipeline phases
try {
    Write-Log '🏗️ STARTING COMPREHENSIVE BUILD & RUN PIPELINE' 'PIPELINE'

    # Environment Setup Phase
    Write-Log 'Environment Setup Phase' 'PHASE'
    Write-Log "PowerShell Version: $($PSVersionTable.PSVersion)"
    Write-Log "Working Directory: $PSScriptRoot"
    Write-Log "Log File: $logFile"

    # Always use workspace root for solution/project validation
    $workspaceRoot = Resolve-Path "$PSScriptRoot\..\.." | Select-Object -ExpandProperty Path
    Write-Log "Workspace Root: $workspaceRoot"
    # Project Structure Validation
    Write-Log 'Validating Project Structure' 'VALIDATE'
    if (-not (Test-Path "$workspaceRoot\BusBuddy.sln")) {
        Write-Log 'ERROR: BusBuddy.sln not found in workspace root!' 'ERROR'
        exit 1
    }
    if (-not (Test-Path "$workspaceRoot\BusBuddy.WPF\BusBuddy.WPF.csproj")) {
        Write-Log 'ERROR: BusBuddy.WPF project not found in workspace root!' 'ERROR'
        exit 1
    }
    Write-Log '✅ Project structure validated'

    # PowerShell Profiles Loading
    Write-Log 'Loading PowerShell Profiles' 'PHASE'
    if (Test-Path '.\load-bus-buddy-profiles.ps1') {
        Write-Log 'Loading Bus Buddy profiles...'
        try {
            & '.\load-bus-buddy-profiles.ps1' -Quiet
            Write-Log '✅ Profiles loaded successfully'
        }
        catch {
            Write-Log "⚠️ Profile loading failed: $_" 'WARN'
        }
    }
    else {
        Write-Log '⚠️ No Bus Buddy profiles found' 'WARN'
    }

    # Package Restoration Phase
    Write-Log 'Package Restoration Phase' 'PHASE'
    Write-Log 'Restoring NuGet packages...'
    $restoreOutput = dotnet restore BusBuddy.sln --verbosity normal --nologo 2>&1
    $restoreOutput | Out-File -FilePath $logFile -Append -Encoding UTF8

    if ($LASTEXITCODE -eq 0) {
        Write-Log '✅ Package restoration completed'
    }
    else {
        Write-Log '❌ Package restoration failed!' 'ERROR'
        Write-Log 'PIPELINE FAILED - Check log for details' 'ERROR'
        exit 1
    }

    # Solution Build Phase
    Write-Log 'Solution Build Phase' 'PHASE'
    Write-Log 'Building solution...'
    $buildOutput = dotnet build BusBuddy.sln --verbosity normal --nologo 2>&1
    $buildOutput | Out-File -FilePath $logFile -Append -Encoding UTF8

    if ($LASTEXITCODE -eq 0) {
        Write-Log '✅ Build completed successfully'
    }
    else {
        Write-Log '❌ Build failed!' 'ERROR'
        Write-Log 'PIPELINE FAILED - Check log for details' 'ERROR'
        Write-Log "Full log available at: $logFile" 'INFO'
        exit 1
    }

    # Application Startup Phase
    Write-Log 'Application Startup Phase' 'PHASE'
    Write-Log 'Starting BusBuddy application...'

    try {
        $runOutput = dotnet run --project 'BusBuddy.WPF\BusBuddy.WPF.csproj' --verbosity normal 2>&1
        $runOutput | Out-File -FilePath $logFile -Append -Encoding UTF8

        if ($LASTEXITCODE -eq 0) {
            Write-Log '✅ Application started successfully'
        }
        else {
            Write-Log '❌ Application startup failed!' 'ERROR'
            Write-Log 'RUNTIME ERROR DETECTED' 'ERROR'
        }
    }
    catch {
        Write-Log "❌ Application startup exception: $_" 'ERROR'
    }

    # Pipeline Completion Phase
    Write-Log 'Pipeline Completion Phase' 'PHASE'
    Write-Log '📊 PIPELINE SUMMARY:'
    Write-Log "Log File: $logFile"
    Write-Log "Exit Code: $LASTEXITCODE"
    Write-Log "Timestamp: $timestamp"
    Write-Log '🏁 PIPELINE COMPLETED' 'PIPELINE'

    # Results Display
    if (-not $Quiet) {
        Write-Host "`n📋 PIPELINE RESULTS:" -ForegroundColor Cyan
        Write-Host "📄 Full log: $logFile" -ForegroundColor Cyan
        Write-Host "🕐 Completed: $timestamp" -ForegroundColor Green

        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ PIPELINE FAILED - Check logs for details" -ForegroundColor Red
        }
        else {
            Write-Host "✅ PIPELINE SUCCEEDED" -ForegroundColor Green
        }

        Write-Host "`n🔍 To analyze results:" -ForegroundColor Yellow
        Write-Host "Get-Content '$logFile' | Select-String 'ERROR|WARN|PHASE'" -ForegroundColor Yellow
    }
}
catch {
    Write-Log "❌ PIPELINE EXCEPTION: $_" 'ERROR'
    throw
}
