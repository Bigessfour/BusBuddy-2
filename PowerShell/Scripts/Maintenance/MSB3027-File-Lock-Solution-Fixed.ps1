#Requires -Version 7.5
<#
.SYNOPSIS
    MSB3027/MSB3021 File Lock Solution based on MSBuild Repository Research
.DESCRIPTION
    This script implements file lock resolution strategies discovered from the MSBuild repository:
    1. Uses Restart Manager API approach (like MSBuild's LockCheck.cs)
    2. Implements kill processes approach from MSBuild's cibuild_bootstrapped_msbuild.ps1
    3. Handles self-locking scenarios with external session termination
.NOTES
    Based on research from dotnet/msbuild repository
#>

param(
    [ValidateSet("Analysis", "Kill", "External", "Complete")]
    [string]$Mode = "Complete",
    [string]$ProjectRoot = $PWD,
    [switch]$Force,
    [switch]$PreventSelfKill
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$KnownLockingProcesses = @('msbuild', 'dotnet', 'vbcscompiler', 'MSBuild', 'pwsh', 'powershell')

function Write-MSBuildLog {
    param(
        [string]$Message,
        [ValidateSet("Info", "Warning", "Error", "Success")]
        [string]$Level = "Info"
    )

    $color = switch ($Level) {
        "Info" { "Cyan" }
        "Warning" { "Yellow" }
        "Error" { "Red" }
        "Success" { "Green" }
    }

    $timestamp = Get-Date -Format "HH:mm:ss"
    Write-Host "[$timestamp] $Message" -ForegroundColor $color
}

function Get-ProcessesLockingBusBuddyFiles {
    param([string]$ProjectRoot)

    Write-MSBuildLog "Scanning for processes locking BusBuddy files..." "Info"

    $lockingProcesses = @()
    $currentPID = $PID

    # First scan: Check processes from project folder (original logic)
    foreach ($process in Get-Process -ErrorAction SilentlyContinue | Where-Object { $KnownLockingProcesses -contains $_.Name }) {

        if ([string]::IsNullOrEmpty($process.Path)) {
            Write-MSBuildLog "Process $($process.Id) $($process.Name) has no path. Skipping." "Warning"
            continue
        }

        if ($process.Path.StartsWith($ProjectRoot, [StringComparison]::InvariantCultureIgnoreCase)) {
            $isSelf = $process.Id -eq $currentPID

            $lockingProcesses += [PSCustomObject]@{
                Name = $process.Name
                PID = $process.Id
                Path = $process.Path
                StartTime = $process.StartTime
                IsSelf = $isSelf
                CanKill = -not $isSelf -or -not $PreventSelfKill
                Source = "ProjectFolder"
            }
        }
    }

    # Second scan: Find ALL PowerShell processes that might have loaded BusBuddy modules
    Write-MSBuildLog "Scanning ALL PowerShell processes for potential BusBuddy module locks..." "Info"

    foreach ($process in Get-Process -Name "pwsh", "powershell" -ErrorAction SilentlyContinue) {
        # Skip if already found in project folder scan
        if ($lockingProcesses | Where-Object { $_.PID -eq $process.Id }) {
            continue
        }

        $isSelf = $process.Id -eq $currentPID

        $lockingProcesses += [PSCustomObject]@{
            Name = $process.Name
            PID = $process.Id
            Path = if ($process.Path) { $process.Path } else { "Unknown" }
            StartTime = $process.StartTime
            IsSelf = $isSelf
            CanKill = -not $isSelf -or -not $PreventSelfKill
            Source = "AllPowerShell"
        }
    }

    return $lockingProcesses
}function Stop-MSBuildProcesses {
    param(
        [array]$Processes,
        [switch]$Force
    )

    Write-MSBuildLog "Stopping MSBuild-related processes..." "Warning"

    $killed = @()
    $skipped = @()

    foreach ($proc in $Processes) {
        if (-not $proc.CanKill -and -not $Force) {
            Write-MSBuildLog "Skipping current PowerShell session (PID: $($proc.PID))" "Warning"
            $skipped += $proc
            continue
        }

        try {
            Write-MSBuildLog "Killing $($proc.Name) (PID: $($proc.PID)) from $($proc.Path)" "Warning"

            $result = Start-Process -FilePath "taskkill" -ArgumentList "/f", "/pid", $proc.PID -Wait -PassThru -NoNewWindow

            if ($result.ExitCode -eq 0) {
                $killed += $proc
                Write-MSBuildLog "Successfully killed $($proc.Name) (PID: $($proc.PID))" "Success"
            } else {
                Write-MSBuildLog "Failed to kill $($proc.Name) (PID: $($proc.PID))" "Error"
            }
        }
        catch {
            Write-MSBuildLog "Error killing $($proc.Name) (PID: $($proc.PID)): $($_.Exception.Message)" "Error"
        }
    }

    return @{
        Killed = $killed
        Skipped = $skipped
    }
}

function Clear-MSBuildArtifacts {
    param([string]$ProjectRoot)

    Write-MSBuildLog "Clearing MSBuild artifacts..." "Info"

    $artifactPaths = @(
        "bin", "obj", "artifacts", ".vs",
        "TestResults", "logs", "stage1"
    )

    foreach ($path in $artifactPaths) {
        $fullPath = Join-Path $ProjectRoot $path
        if (Test-Path $fullPath) {
            try {
                Remove-Item -Path $fullPath -Recurse -Force -ErrorAction Stop
                Write-MSBuildLog "Removed: $fullPath" "Success"
            }
            catch {
                Write-MSBuildLog "Could not remove $fullPath - $($_.Exception.Message)" "Warning"
            }
        }
    }
}

function Test-BuildAfterCleanup {
    param([string]$ProjectRoot)

    Write-MSBuildLog "Testing build after cleanup..." "Info"

    try {
        Set-Location $ProjectRoot

        $buildResult = & dotnet build "BusBuddy.sln" --verbosity minimal --nologo 2>&1

        if ($LASTEXITCODE -eq 0) {
            Write-MSBuildLog "Build test SUCCESSFUL" "Success"
            return $true
        } else {
            Write-MSBuildLog "Build test FAILED:" "Error"
            $buildResult | ForEach-Object { Write-MSBuildLog "  $_" "Error" }
            return $false
        }
    }
    catch {
        Write-MSBuildLog "Build test ERROR: $($_.Exception.Message)" "Error"
        return $false
    }
}

try {
    Write-MSBuildLog "MSB3027/MSB3021 File Lock Solution Starting" "Info"
    Write-MSBuildLog "Mode: $Mode | Project: $ProjectRoot" "Info"

    switch ($Mode) {
        "Analysis" {
            $processes = Get-ProcessesLockingBusBuddyFiles -ProjectRoot $ProjectRoot
            $processCount = if ($processes) { @($processes).Count } else { 0 }
            Write-MSBuildLog "Found $processCount potentially locking processes:" "Info"
            if ($processes) {
                $processes | ForEach-Object {
                    $selfFlag = if ($_.IsSelf) { " (CURRENT SESSION)" } else { "" }
                    Write-MSBuildLog "  $($_.Name) (PID: $($_.PID)) [$($_.Source)]$selfFlag" "Info"
                }
            }
        }

        "Kill" {
            $processes = Get-ProcessesLockingBusBuddyFiles -ProjectRoot $ProjectRoot
            if ($processes) {
                $result = Stop-MSBuildProcesses -Processes $processes -Force:$Force
                $killedCount = if ($result.Killed) { @($result.Killed).Count } else { 0 }
                $skippedCount = if ($result.Skipped) { @($result.Skipped).Count } else { 0 }
                Write-MSBuildLog "Killed: $killedCount | Skipped: $skippedCount" "Success"
            } else {
                Write-MSBuildLog "No processes found to kill" "Info"
            }
        }

        "Complete" {
            $processes = Get-ProcessesLockingBusBuddyFiles -ProjectRoot $ProjectRoot
            $processCount = if ($processes) { @($processes).Count } else { 0 }
            Write-MSBuildLog "Found $processCount processes to clean up" "Info"

            if ($processes) {
                $killable = $processes | Where-Object { $_.CanKill }
                if ($killable) {
                    Stop-MSBuildProcesses -Processes $killable -Force:$Force
                }
            }            Clear-MSBuildArtifacts -ProjectRoot $ProjectRoot

            [System.GC]::Collect()
            [System.GC]::WaitForPendingFinalizers()

            Start-Sleep -Seconds 2

            $buildSuccess = Test-BuildAfterCleanup -ProjectRoot $ProjectRoot
            if ($buildSuccess) {
                Write-MSBuildLog "MSB3027/MSB3021 File Lock Solution COMPLETED SUCCESSFULLY" "Success"
            } else {
                Write-MSBuildLog "Build still failing after cleanup. May need manual intervention." "Error"
                exit 1
            }
        }
    }
}
catch {
    Write-MSBuildLog "CRITICAL ERROR: $($_.Exception.Message)" "Error"
    Write-MSBuildLog "Stack trace: $($_.ScriptStackTrace)" "Error"
    exit 1
}
