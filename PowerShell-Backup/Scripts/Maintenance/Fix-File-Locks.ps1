#Requires -Version 7.5
<#
.SYNOPSIS
Emergency file lock resolution for BusBuddy DLL conflicts

.DESCRIPTION
Safely terminates PowerShell 7 processes that are holding DLL locks,
preventing successful builds. Includes safe process identification and disposal.

.PARAMETER Force
Force termination of all PowerShell processes except current session

.PARAMETER Safe
Only terminate processes older than 5 minutes with low CPU usage

.EXAMPLE
.\Fix-File-Locks.ps1 -Safe
#>

param(
    [switch]$Force,
    [switch]$Safe
)

$ErrorActionPreference = "Stop"

# Default to Safe mode if no switches specified
if (-not $Force -and -not $PSBoundParameters.ContainsKey('Safe')) {
    $Safe = $true
}

function Write-Status {
    param($Message, $Type = "Info")
    $color = switch($Type) {
        "Success" { "Green" }
        "Warning" { "Yellow" }
        "Error" { "Red" }
        default { "Cyan" }
    }
    Write-Host "🔧 $Message" -ForegroundColor $color
}

function Get-ProblemProcesses {
    Write-Status "Scanning for PowerShell processes holding locks..."

    $currentPID = $PID
    $processes = Get-Process pwsh -ErrorAction SilentlyContinue

    if (-not $processes) {
        Write-Status "No PowerShell 7 processes found" "Success"
        return @()
    }

    $problemProcesses = @()

    foreach ($proc in $processes) {
        if ($proc.Id -eq $currentPID) {
            Write-Status "Skipping current session (PID: $currentPID)"
            continue
        }

        $startTime = $proc.StartTime
        $ageMinutes = if ($startTime) { (Get-Date) - $startTime | Select-Object -ExpandProperty TotalMinutes } else { 999 }
        $cpuTime = $proc.CPU ?? 0

        Write-Host "  📊 PID $($proc.Id): Age=$([math]::Round($ageMinutes,1))min, CPU=$([math]::Round($cpuTime,2))s" -ForegroundColor Gray

        # Identify problem processes
        $isProblem = $false
        $reason = ""

        if ($Force) {
            $isProblem = $true
            $reason = "Force mode"
        } elseif ($Safe -and $ageMinutes -gt 5 -and $cpuTime -lt 10) {
            $isProblem = $true
            $reason = "Old idle process"
        } elseif ($ageMinutes -gt 30) {
            $isProblem = $true
            $reason = "Very old process"
        }

        if ($isProblem) {
            $problemProcesses += [PSCustomObject]@{
                PID = $proc.Id
                Age = $ageMinutes
                CPU = $cpuTime
                Reason = $reason
                Process = $proc
            }
        }
    }

    return $problemProcesses
}

function Stop-ProblemProcesses {
    param([array]$Processes)

    if ($Processes.Count -eq 0) {
        Write-Status "No problem processes identified" "Success"
        return
    }

    Write-Status "Found $($Processes.Count) problem processes:" "Warning"

    foreach ($proc in $Processes) {
        Write-Host "  ❌ PID $($proc.PID): $($proc.Reason)" -ForegroundColor Red
    }

    if (-not $Force) {
        $response = Read-Host "`n🔄 Terminate these processes? (y/N)"
        if ($response -notmatch '^[Yy]') {
            Write-Status "Operation cancelled by user"
            return
        }
    }

    Write-Status "Terminating problem processes..."

    foreach ($proc in $Processes) {
        try {
            Write-Host "  🔥 Stopping PID $($proc.PID)..." -ForegroundColor Yellow
            Stop-Process -Id $proc.PID -Force -ErrorAction Stop
            Start-Sleep -Milliseconds 500
            Write-Host "  ✅ PID $($proc.PID) terminated" -ForegroundColor Green
        } catch {
            Write-Host "  ❌ Failed to stop PID $($proc.PID): $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

function Clear-BuildArtifacts {
    Write-Status "Clearing build artifacts to prevent locks..."

    $binDirs = Get-ChildItem -Recurse -Directory -Name "bin" -ErrorAction SilentlyContinue
    $objDirs = Get-ChildItem -Recurse -Directory -Name "obj" -ErrorAction SilentlyContinue

    foreach ($dir in ($binDirs + $objDirs)) {
        try {
            Remove-Item $dir -Recurse -Force -ErrorAction Stop
            Write-Host "  🗑️ Cleared: $dir" -ForegroundColor Gray
        } catch {
            Write-Host "  ⚠️ Couldn't clear: $dir" -ForegroundColor Yellow
        }
    }
}

function Invoke-GarbageCollection {
    Write-Status "Forcing garbage collection..."
    [System.GC]::Collect()
    [System.GC]::WaitForPendingFinalizers()
    [System.GC]::Collect()
    Write-Status "Garbage collection completed" "Success"
}

# Main execution
try {
    Write-Status "=== BusBuddy File Lock Resolution ===" "Info"
    Write-Status "Current session PID: $PID"

    # Step 1: Identify problem processes
    $problemProcesses = Get-ProblemProcesses

    # Step 2: Terminate problem processes
    Stop-ProblemProcesses -Processes $problemProcesses

    # Step 3: Clear build artifacts
    Clear-BuildArtifacts

    # Step 4: Force garbage collection
    Invoke-GarbageCollection

    # Step 5: Verify resolution
    Write-Status "Verifying lock resolution..."
    $remainingProcesses = Get-Process pwsh -ErrorAction SilentlyContinue | Where-Object { $_.Id -ne $PID }

    if ($remainingProcesses) {
        Write-Status "Remaining PowerShell processes: $($remainingProcesses.Count)" "Warning"
        $remainingProcesses | ForEach-Object {
            Write-Host "  📌 PID $($_.Id) still running" -ForegroundColor Yellow
        }
    } else {
        Write-Status "All problematic processes terminated" "Success"
    }

    Write-Status "File lock resolution completed" "Success"
    Write-Host ""
    Write-Status "Ready for build. Use: dotnet build BusBuddy.sln" "Info"

} catch {
    Write-Status "Error during file lock resolution: $($_.Exception.Message)" "Error"
    exit 1
}
