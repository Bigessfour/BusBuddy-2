#Requires -Version 7.5
<#
.SYNOPSIS
Resource cleanup and auto-disposal management for BusBuddy automation scripts

.DESCRIPTION
Provides automatic resource cleanup, process disposal, and session management
to prevent file locks and resource leaks in PowerShell automation workflows.

.PARAMETER Mode
Cleanup mode: Auto (default), Aggressive, Conservative

.PARAMETER UnloadModules
Unload BusBuddy PowerShell modules after operations

.EXAMPLE
.\Resource-Cleanup-Manager.ps1 -Mode Auto -UnloadModules
#>

param(
    [ValidateSet("Auto", "Aggressive", "Conservative")]
    [string]$Mode = "Auto",

    [switch]$UnloadModules,
    [switch]$Force
)

$ErrorActionPreference = "Stop"

class ResourceManager {
    [hashtable]$TrackedResources
    [datetime]$StartTime
    [bool]$AutoDisposal

    ResourceManager() {
        $this.TrackedResources = @{}
        $this.StartTime = Get-Date
        $this.AutoDisposal = $true
    }

    [void]TrackResource([string]$Name, [object]$Resource) {
        $this.TrackedResources[$Name] = @{
            Resource = $Resource
            Created = Get-Date
            Type = $Resource.GetType().Name
        }
        Write-Host "📋 Tracking resource: $Name ($($Resource.GetType().Name))" -ForegroundColor Gray
    }

    [void]DisposeResource([string]$Name) {
        if ($this.TrackedResources.ContainsKey($Name)) {
            $item = $this.TrackedResources[$Name]
            try {
                if ($item.Resource -is [System.IDisposable]) {
                    $item.Resource.Dispose()
                    Write-Host "🗑️ Disposed: $Name" -ForegroundColor Green
                } else {
                    Write-Host "ℹ️ Not disposable: $Name" -ForegroundColor Yellow
                }
            } catch {
                Write-Host "❌ Failed to dispose $Name : $($_.Exception.Message)" -ForegroundColor Red
            }
            $this.TrackedResources.Remove($Name)
        }
    }

    [void]DisposeAll() {
        Write-Host "🧹 Disposing all tracked resources..." -ForegroundColor Cyan
        $names = @($this.TrackedResources.Keys)
        foreach ($name in $names) {
            $this.DisposeResource($name)
        }
    }
}

function Write-CleanupStatus {
    param($Message, $Type = "Info")
    $color = switch($Type) {
        "Success" { "Green" }
        "Warning" { "Yellow" }
        "Error" { "Red" }
        default { "Cyan" }
    }
    Write-Host "🧹 $Message" -ForegroundColor $color
}

function Initialize-ResourceManager {
    Write-CleanupStatus "Initializing resource manager..."
    $global:BusBuddyResourceManager = [ResourceManager]::new()

    # Register cleanup on exit
    Register-EngineEvent PowerShell.Exiting -Action {
        if ($global:BusBuddyResourceManager) {
            $global:BusBuddyResourceManager.DisposeAll()
        }
    } | Out-Null

    Write-CleanupStatus "Resource manager initialized" "Success"
}

function Clear-FileHandles {
    param([string]$Mode)

    Write-CleanupStatus "Clearing file handles (Mode: $Mode)..."

    try {
        # Force garbage collection
        [System.GC]::Collect()
        [System.GC]::WaitForPendingFinalizers()
        [System.GC]::Collect()

        if ($Mode -eq "Aggressive") {
            # Clear all file handles more aggressively
            [System.GC]::Collect(2, [System.GCCollectionMode]::Forced)
            [System.GC]::WaitForPendingFinalizers()
        }

        Write-CleanupStatus "File handles cleared" "Success"
    } catch {
        Write-CleanupStatus "Error clearing file handles: $($_.Exception.Message)" "Error"
    }
}

function Clear-PowerShellSessions {
    param([string]$Mode, [bool]$Force)

    Write-CleanupStatus "Managing PowerShell sessions..."

    $sessions = Get-PSSession -ErrorAction SilentlyContinue

    if ($sessions) {
        Write-CleanupStatus "Found $($sessions.Count) PowerShell sessions" "Warning"

        foreach ($session in $sessions) {
            try {
                Remove-PSSession $session -ErrorAction Stop
                Write-CleanupStatus "Removed session: $($session.Name)" "Success"
            } catch {
                Write-CleanupStatus "Failed to remove session $($session.Name): $($_.Exception.Message)" "Error"
            }
        }
    }

    # Handle background jobs
    $jobs = Get-Job -ErrorAction SilentlyContinue
    if ($jobs) {
        Write-CleanupStatus "Found $($jobs.Count) background jobs" "Warning"

        foreach ($job in $jobs) {
            try {
                if ($job.State -eq "Running" -and ($Mode -eq "Aggressive" -or $Force)) {
                    Stop-Job $job -ErrorAction Stop
                }
                Remove-Job $job -Force -ErrorAction Stop
                Write-CleanupStatus "Cleaned job: $($job.Name)" "Success"
            } catch {
                Write-CleanupStatus "Failed to clean job $($job.Name): $($_.Exception.Message)" "Error"
            }
        }
    }
}

function Clear-ModuleCache {
    param([bool]$UnloadModules)

    if (-not $UnloadModules) {
        Write-CleanupStatus "Skipping module unload (UnloadModules not specified)"
        return
    }

    Write-CleanupStatus "Unloading BusBuddy modules..."

    $busBuddyModules = Get-Module | Where-Object { $_.Name -like "*BusBuddy*" }

    if ($busBuddyModules) {
        foreach ($module in $busBuddyModules) {
            try {
                Remove-Module $module -Force -ErrorAction Stop
                Write-CleanupStatus "Unloaded module: $($module.Name)" "Success"
            } catch {
                Write-CleanupStatus "Failed to unload module $($module.Name): $($_.Exception.Message)" "Error"
            }
        }
    } else {
        Write-CleanupStatus "No BusBuddy modules to unload"
    }
}

function Clear-TemporaryFiles {
    param([string]$Mode)

    Write-CleanupStatus "Clearing temporary files..."

    $tempPaths = @(
        ".\bin\Debug\**\*.tmp",
        ".\bin\Release\**\*.tmp",
        ".\obj\**\*.tmp",
        ".\logs\*.tmp",
        ".\*.tmp"
    )

    $removedCount = 0
    foreach ($pattern in $tempPaths) {
        try {
            $files = Get-ChildItem $pattern -ErrorAction SilentlyContinue
            foreach ($file in $files) {
                Remove-Item $file.FullName -Force -ErrorAction Stop
                $removedCount++
            }
        } catch {
            # Silently continue for inaccessible files unless in aggressive mode
            if ($Mode -eq "Aggressive") {
                Write-CleanupStatus "Couldn't remove temp file: $($_.Exception.Message)" "Warning"
            }
        }
    }

    Write-CleanupStatus "Removed $removedCount temporary files" "Success"
}

function Test-ResourceLeaks {
    Write-CleanupStatus "Checking for resource leaks..."

    # Check for orphaned processes
    $orphanedProcesses = Get-Process pwsh -ErrorAction SilentlyContinue |
        Where-Object { $_.Id -ne $PID -and (Get-Date) - $_.StartTime -gt [timespan]::FromMinutes(10) }

    if ($orphanedProcesses) {
        Write-CleanupStatus "Found $($orphanedProcesses.Count) potentially orphaned PowerShell processes" "Warning"
        foreach ($proc in $orphanedProcesses) {
            Write-Host "  🔍 PID $($proc.Id): Age $([math]::Round(((Get-Date) - $proc.StartTime).TotalMinutes, 1)) minutes" -ForegroundColor Yellow
        }
    }

    # Check memory usage
    $currentProcess = Get-Process -Id $PID
    $memoryMB = [math]::Round($currentProcess.WorkingSet64 / 1MB, 1)

    if ($memoryMB -gt 100) {
        Write-CleanupStatus "High memory usage detected: ${memoryMB}MB" "Warning"
    } else {
        Write-CleanupStatus "Memory usage normal: ${memoryMB}MB" "Success"
    }
}

function Invoke-CleanupSequence {
    param([string]$Mode, [bool]$UnloadModules, [bool]$Force)

    Write-CleanupStatus "Starting cleanup sequence (Mode: $Mode)..."

    # Step 1: Clear PowerShell sessions and jobs
    Clear-PowerShellSessions -Mode $Mode -Force $Force

    # Step 2: Clear file handles
    Clear-FileHandles -Mode $Mode

    # Step 3: Clear module cache if requested
    Clear-ModuleCache -UnloadModules $UnloadModules

    # Step 4: Clear temporary files
    Clear-TemporaryFiles -Mode $Mode

    # Step 5: Dispose tracked resources
    if ($global:BusBuddyResourceManager) {
        $global:BusBuddyResourceManager.DisposeAll()
    }

    # Step 6: Final memory cleanup
    if ($Mode -eq "Aggressive") {
        Write-CleanupStatus "Performing aggressive memory cleanup..."
        [System.GC]::Collect(2, [System.GCCollectionMode]::Forced)
        [System.GC]::WaitForPendingFinalizers()
        [System.GC]::Collect()
    }

    Write-CleanupStatus "Cleanup sequence completed" "Success"
}

# Main execution
try {
    Write-CleanupStatus "=== BusBuddy Resource Cleanup Manager ===" "Info"
    Write-CleanupStatus "Mode: $Mode | Unload Modules: $UnloadModules | Force: $Force"
    Write-Host ""

    # Initialize resource manager
    Initialize-ResourceManager

    # Check current state
    Test-ResourceLeaks
    Write-Host ""

    # Execute cleanup
    Invoke-CleanupSequence -Mode $Mode -UnloadModules $UnloadModules -Force $Force
    Write-Host ""

    # Final verification
    Test-ResourceLeaks

    Write-CleanupStatus "Resource cleanup completed successfully" "Success"

} catch {
    Write-CleanupStatus "Cleanup failed: $($_.Exception.Message)" "Error"
    exit 1
}

# Export functions for use in other scripts (only when loaded as a module)
if ($MyInvocation.InvocationName -ne "." -and $MyInvocation.InvocationName -ne "&" -and $MyInvocation.MyCommand.ModuleName) {
    Export-ModuleMember -Function Initialize-ResourceManager, Clear-FileHandles, Clear-PowerShellSessions
}
