#Requires -Version 7.0
<#
.SYNOPSIS
    Bus Buddy Enhanced Log Monitoring

.DESCRIPTION
    Advanced log monitoring script for Bus Buddy development
    Provides real-time log tailing with filtering and error highlighting

.NOTES
    Optimized for solo development with actionable error detection
    Integrates with Serilog configuration in appsettings.json
#>

param(
    [string]$LogType = "application",
    [int]$Lines = 10,
    [switch]$Follow,
    [switch]$ErrorsOnly,
    [switch]$UIOnly,
    [switch]$Colorized,
    [string]$Filter = $null,
    [string]$LogPath = $null
)

# Enhanced log monitoring functions
function Get-BusBuddyProjectRoot {
    $current = $PWD.Path

    # Look for solution file to identify project root
    while ($current) {
        if (Test-Path (Join-Path $current "BusBuddy.sln")) {
            return $current
        }

        $parent = Split-Path $current -Parent
        if ($parent -eq $current) { break }
        $current = $parent
    }

    return $null
}

function Get-LatestLogFile {
    param(
        [string]$LogsDirectory,
        [string]$Pattern
    )

    if (-not (Test-Path $LogsDirectory)) {
        Write-Host "📁 Creating logs directory: $LogsDirectory" -ForegroundColor Yellow
        New-Item -ItemType Directory -Path $LogsDirectory -Force | Out-Null
        return $null
    }

    $logFiles = Get-ChildItem -Path $LogsDirectory -Filter $Pattern -ErrorAction SilentlyContinue |
                Sort-Object LastWriteTime -Descending

    if ($logFiles.Count -gt 0) {
        return $logFiles[0]
    }

    return $null
}

function Format-LogLine {
    param(
        [string]$Line,
        [switch]$Colorized
    )

    if (-not $Colorized) {
        return $Line
    }

    # Color-code log levels
    if ($Line -match '\[ERR\]|\[ERROR\]') {
        Write-Host $Line -ForegroundColor Red
    }
    elseif ($Line -match '\[WRN\]|\[WARNING\]') {
        Write-Host $Line -ForegroundColor Yellow
    }
    elseif ($Line -match '\[INF\]|\[INFO\]') {
        Write-Host $Line -ForegroundColor White
    }
    elseif ($Line -match '\[DBG\]|\[DEBUG\]') {
        Write-Host $Line -ForegroundColor Gray
    }
    elseif ($Line -match '\[VRB\]|\[VERBOSE\]') {
        Write-Host $Line -ForegroundColor DarkGray
    }
    elseif ($Line -match '🚨|❌|ERROR') {
        Write-Host $Line -ForegroundColor Red
    }
    elseif ($Line -match '⚠️|WARNING') {
        Write-Host $Line -ForegroundColor Yellow
    }
    elseif ($Line -match '✅|SUCCESS') {
        Write-Host $Line -ForegroundColor Green
    }
    elseif ($Line -match '🔧|🎯|💡') {
        Write-Host $Line -ForegroundColor Cyan
    }
    else {
        Write-Host $Line -ForegroundColor White
    }
}

function Watch-BusBuddyLogFile {
    param(
        [string]$FilePath,
        [int]$TailLines,
        [switch]$Follow,
        [switch]$Colorized,
        [string]$Filter,
        [switch]$ErrorsOnly,
        [switch]$UIOnly
    )

    if (-not (Test-Path $FilePath)) {
        Write-Host "📝 Log file not found: $FilePath" -ForegroundColor Yellow
        Write-Host "💡 Waiting for log file to be created..." -ForegroundColor Gray

        # Wait for file to be created
        do {
            Start-Sleep -Seconds 2
        } while (-not (Test-Path $FilePath))

        Write-Host "✅ Log file created: $FilePath" -ForegroundColor Green
    }

    $fileName = Split-Path $FilePath -Leaf
    Write-Host "📖 Monitoring: $fileName" -ForegroundColor Cyan

    if ($Follow) {
        Write-Host "👀 Following log (Ctrl+C to stop)..." -ForegroundColor Green

        # Use PowerShell 7.5.2 optimized approach
        if ($ErrorsOnly) {
            Get-Content $FilePath -Tail $TailLines -Wait |
            Where-Object { $_ -match 'ERR|ERROR|Exception|🚨|❌' } |
            ForEach-Object { Format-LogLine $_ -Colorized:$Colorized }
        }
        elseif ($UIOnly) {
            Get-Content $FilePath -Tail $TailLines -Wait |
            Where-Object { $_ -match 'UI|View|Window|Button|Control|Theme|Style|🖱️|🎨' } |
            ForEach-Object { Format-LogLine $_ -Colorized:$Colorized }
        }
        elseif ($Filter) {
            Get-Content $FilePath -Tail $TailLines -Wait |
            Where-Object { $_ -match $Filter } |
            ForEach-Object { Format-LogLine $_ -Colorized:$Colorized }
        }
        else {
            Get-Content $FilePath -Tail $TailLines -Wait |
            ForEach-Object { Format-LogLine $_ -Colorized:$Colorized }
        }
    }
    else {
        Write-Host "📋 Last $TailLines lines:" -ForegroundColor Cyan

        $content = Get-Content $FilePath -Tail $TailLines

        if ($ErrorsOnly) {
            $content = $content | Where-Object { $_ -match 'ERR|ERROR|Exception|🚨|❌' }
        }
        elseif ($UIOnly) {
            $content = $content | Where-Object { $_ -match 'UI|View|Window|Button|Control|Theme|Style|🖱️|🎨' }
        }
        elseif ($Filter) {
            $content = $content | Where-Object { $_ -match $Filter }
        }

        $content | ForEach-Object { Format-LogLine $_ -Colorized:$Colorized }
    }
}

# Main execution
function Start-LogMonitoring {
    Write-Host "📖 Bus Buddy Enhanced Log Monitoring" -ForegroundColor Blue
    Write-Host "====================================" -ForegroundColor Blue

    # Get project root
    $root = Get-BusBuddyProjectRoot
    if (-not $root) {
        Write-Host "❌ Bus Buddy project not found" -ForegroundColor Red
        Write-Host "💡 Navigate to the Bus Buddy project directory and try again" -ForegroundColor Gray
        exit 1
    }

    # Determine log file path
    if ($LogPath) {
        $targetLogFile = $LogPath
    }
    else {
        $logsDirectory = Join-Path $root "BusBuddy.WPF\logs"

        # Determine log pattern based on type
        $pattern = switch ($LogType.ToLower()) {
            "application" { "application-*.log" }
            "app" { "application-*.log" }
            "errors" { "errors-actionable-*.log" }
            "error" { "errors-actionable-*.log" }
            "ui" { "ui-interactions-*.log" }
            "interactions" { "ui-interactions-*.log" }
            default { "application-*.log" }
        }

        # Get latest log file
        $latestLog = Get-LatestLogFile -LogsDirectory $logsDirectory -Pattern $pattern

        if ($latestLog) {
            $targetLogFile = $latestLog.FullName
        }
        else {
            Write-Host "📝 No log files found matching pattern: $pattern" -ForegroundColor Yellow
            Write-Host "💡 Run the Bus Buddy application to generate logs" -ForegroundColor Gray

            # Wait for log files to be created
            Write-Host "⏳ Waiting for log files..." -ForegroundColor Cyan
            do {
                Start-Sleep -Seconds 2
                $latestLog = Get-LatestLogFile -LogsDirectory $logsDirectory -Pattern $pattern
            } while (-not $latestLog)

            $targetLogFile = $latestLog.FullName
        }
    }

    # Display monitoring info
    Write-Host ""
    Write-Host "🎯 Log Type: $LogType" -ForegroundColor Magenta
    Write-Host "📁 Log File: $(Split-Path $targetLogFile -Leaf)" -ForegroundColor Gray
    Write-Host "📏 Lines: $Lines" -ForegroundColor Gray

    if ($ErrorsOnly) {
        Write-Host "🚨 Filter: Errors Only" -ForegroundColor Red
    }
    elseif ($UIOnly) {
        Write-Host "🎨 Filter: UI Interactions Only" -ForegroundColor Blue
    }
    elseif ($Filter) {
        Write-Host "🔍 Filter: $Filter" -ForegroundColor Yellow
    }

    Write-Host ""

    # Start monitoring
    Watch-BusBuddyLogFile -FilePath $targetLogFile -TailLines $Lines -Follow:$Follow -Colorized:$true -Filter $Filter -ErrorsOnly:$ErrorsOnly -UIOnly:$UIOnly
}

# Execute if run directly
if ($MyInvocation.InvocationName -ne '.') {
    # Set default colorization
    if (-not $PSBoundParameters.ContainsKey('Colorized')) {
        $Colorized = $true
    }

    Start-LogMonitoring
}
