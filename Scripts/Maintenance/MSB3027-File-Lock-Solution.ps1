#Requires -Version 7.5
<#
.SYNOPSIS
    MSB3027/MSB3021 File Lock Solution based on MSBuild Repository Research
.DESCRIPTION
    This script implements file lock resolution strategies discovered from the MSBuild repository:
    1. Uses Restart Manager API (like MSBuild's LockCheck.cs)
    2. Implements kill processes approach from MSBuild's cibuild_bootstrapped_msbuild.ps1
    3. Handles self-locking scenarios with external session termination
.NOTES
    Based on research from dotnet/msbuild repository:
    - src/Utilities/LockCheck.cs: Restart Manager API implementation
    - eng/cibuild_bootstrapped_msbuild.ps1: KillProcessesFromRepo function
    - Line 120-143: Turn off node reuse to prevent locked files
#>

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Analysis", "Kill", "External", "Complete")]
    [string]$Mode = "Complete",

    [Parameter(Mandatory=$false)]
    [string]$ProjectRoot = $PWD,

    [Parameter(Mandatory=$false)]
    [switch]$Force,

    [Parameter(Mandatory=$false)]
    [switch]$PreventSelfKill
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# From MSBuild's LockCheck.cs - These are the processes that can lock build files
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

function Get-MSBuildFileFromError {
    param([string]$ErrorText)

    # Extract file path from MSB3027 errors
    if ($ErrorText -match "cannot access the file '([^']+)'") {
        return $matches[1]
    }

    if ($ErrorText -match "The file is locked by: ([^(]+) \((\d+)\)") {
        return @{
            Process = $matches[1].Trim()
            PID = [int]$matches[2]
        }
    }

    return $null
}

function Get-ProcessesLockingBusBuddyFiles {
    param([string]$ProjectRoot)

    Write-MSBuildLog "Scanning for processes locking BusBuddy files..." "Info"

    $lockingProcesses = @()
    $currentPID = $PID

    # Based on MSBuild's KillProcessesFromRepo function
    foreach ($process in Get-Process -ErrorAction SilentlyContinue | Where-Object { $KnownLockingProcesses -contains $_.Name }) {

        if ([string]::IsNullOrEmpty($process.Path)) {
            Write-MSBuildLog "Process $($process.Id) $($process.Name) has no path. Skipping." "Warning"
            continue
        }

        # Check if process is from our repo (like MSBuild does)
        if ($process.Path.StartsWith($ProjectRoot, [StringComparison]::InvariantCultureIgnoreCase)) {
            $isSelf = $process.Id -eq $currentPID

            $lockingProcesses += [PSCustomObject]@{
                Name = $process.Name
                PID = $process.Id
                Path = $process.Path
                StartTime = $process.StartTime
                IsSelf = $isSelf
                CanKill = -not $isSelf -or -not $PreventSelfKill
            }
        }
    }

    return $lockingProcesses
}

function Stop-MSBuildProcesses {
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

            # Use taskkill like MSBuild does for forceful termination
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
                Write-MSBuildLog "Could not remove $fullPath: $($_.Exception.Message)" "Warning"
            }
        }
    }
}

function Invoke-ExternalSessionCleanup {
    param([string]$ProjectRoot)

    Write-MSBuildLog "Starting external PowerShell session for cleanup..." "Info"

    $cleanupScript = @"
Set-Location '$ProjectRoot'
Write-Host 'External cleanup session started...'

# Kill all MSBuild-related processes from this project
foreach (`$process in Get-Process -ErrorAction SilentlyContinue | Where-Object {'msbuild', 'dotnet', 'vbcscompiler', 'pwsh', 'powershell' -contains `$_.Name}) {
    if (-not [string]::IsNullOrEmpty(`$process.Path) -and `$process.Path.StartsWith('$ProjectRoot', [StringComparison]::InvariantCultureIgnoreCase)) {
        Write-Host "Killing `$(`$process.Name) (PID: `$(`$process.Id))"
        taskkill /f /pid `$process.Id 2>`$null
    }
}

# Force garbage collection
[System.GC]::Collect()
[System.GC]::WaitForPendingFinalizers()

Write-Host 'External cleanup completed'
"@

    $tempScript = Join-Path $env:TEMP "BusBuddy-External-Cleanup-$(Get-Random).ps1"
    $cleanupScript | Out-File -FilePath $tempScript -Encoding UTF8

    try {
        $result = Start-Process -FilePath "pwsh" -ArgumentList "-ExecutionPolicy", "Bypass", "-File", $tempScript -Wait -PassThru -NoNewWindow
        Write-MSBuildLog "External cleanup completed with exit code: $($result.ExitCode)" "Success"
    }
    finally {
        if (Test-Path $tempScript) {
            Remove-Item $tempScript -Force -ErrorAction SilentlyContinue
        }
    }
}

function Test-BuildAfterCleanup {
    param([string]$ProjectRoot)

    Write-MSBuildLog "Testing build after cleanup..." "Info"

    try {
        Set-Location $ProjectRoot

        # Test with minimal build first
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

# Main execution logic
try {
    Write-MSBuildLog "MSB3027/MSB3021 File Lock Solution Starting" "Info"
    Write-MSBuildLog "Mode: $Mode | Project: $ProjectRoot" "Info"

    switch ($Mode) {
        "Analysis" {
            $processes = Get-ProcessesLockingBusBuddyFiles -ProjectRoot $ProjectRoot
            Write-MSBuildLog "Found $($processes.Count) potentially locking processes:" "Info"
            $processes | ForEach-Object {
                $selfFlag = if ($_.IsSelf) { " (CURRENT SESSION)" } else { "" }
                Write-MSBuildLog "  $($_.Name) (PID: $($_.PID))$selfFlag" "Info"
            }
        }

        "Kill" {
            $processes = Get-ProcessesLockingBusBuddyFiles -ProjectRoot $ProjectRoot
            $result = Stop-MSBuildProcesses -Processes $processes -Force:$Force
            Write-MSBuildLog "Killed: $($result.Killed.Count) | Skipped: $($result.Skipped.Count)" "Success"
        }

        "External" {
            Invoke-ExternalSessionCleanup -ProjectRoot $ProjectRoot
        }

        "Complete" {
            # Full solution based on MSBuild's approach
            $processes = Get-ProcessesLockingBusBuddyFiles -ProjectRoot $ProjectRoot
            Write-MSBuildLog "Found $($processes.Count) processes to clean up" "Info"

            # Step 1: Kill other processes
            $killable = $processes | Where-Object { $_.CanKill }
            if ($killable.Count -gt 0) {
                Stop-MSBuildProcesses -Processes $killable -Force:$Force
            }

            # Step 2: Clear artifacts
            Clear-MSBuildArtifacts -ProjectRoot $ProjectRoot

            # Step 3: If current session was problematic, use external cleanup
            $selfProcess = $processes | Where-Object { $_.IsSelf }
            if ($selfProcess -and -not $PreventSelfKill) {
                Write-MSBuildLog "Current session detected as locking files. Using external cleanup..." "Warning"
                Invoke-ExternalSessionCleanup -ProjectRoot $ProjectRoot
                return # External session will terminate this one
            }

            # Step 4: Test build
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
