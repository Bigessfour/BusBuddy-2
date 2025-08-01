#Requires -Version 7.5
<#
.SYNOPSIS
    Fix MSB3027/MSB3021 assembly file lock issues
.DESCRIPTION
    Resolves file locks on BusBuddy.Core.dll caused by PowerShell module loading the assembly
.EXAMPLE
    .\fix-assembly-lock.ps1
#>

[CmdletBinding()]
param(
    [switch]$Force
)

$scriptRoot = $PSScriptRoot
Write-Host "📂 Script root: $scriptRoot" -ForegroundColor Gray
Write-Host "🔓 Resolving Assembly File Locks for BusBuddy.Core.dll" -ForegroundColor Cyan
Write-Host ""

# Step 1: Identify the locked file
$lockedFile = Join-Path $scriptRoot "BusBuddy.Core\bin\Debug\net9.0-windows\BusBuddy.Core.dll"
Write-Host "🔍 Checking file lock: $lockedFile" -ForegroundColor Yellow

if (Test-Path $lockedFile) {
    Write-Host "✅ File exists: $lockedFile" -ForegroundColor Green
} else {
    Write-Host "⚠️ File does not exist: $lockedFile. Will proceed with cleanup." -ForegroundColor Yellow
}

# Step 2: Find and stop processes locking the file
Write-Host ""
Write-Host "🔍 Finding and stopping processes that have the file locked..." -ForegroundColor Yellow

try {
    $lockingProcesses = Get-Process | Where-Object { $_.Modules.FileName -eq $lockedFile } -ErrorAction SilentlyContinue
    if ($lockingProcesses) {
        foreach ($proc in $lockingProcesses) {
            Write-Host "🛑 Stopping process $($proc.ProcessName) (PID: $($proc.Id)) locking the file." -ForegroundColor Magenta
            Stop-Process -Id $proc.Id -Force
        }
    } else {
        Write-Host "ℹ️ No processes found locking the file directly. Checking PowerShell processes." -ForegroundColor Blue
    }

    # Also check PowerShell processes specifically as they are common culprits
    $pwshProcesses = Get-Process pwsh -ErrorAction SilentlyContinue
    if ($pwshProcesses) {
        Write-Host ""
        Write-Host "📋 Current PowerShell processes:" -ForegroundColor Yellow
        foreach ($proc in $pwshProcesses) {
            if ($proc.Id -ne $PID) { # Don't kill the current session
                Write-Host "🛑 Stopping background PowerShell process $($proc.ProcessName) (PID: $($proc.Id))." -ForegroundColor Magenta
                Stop-Process -Id $proc.Id -Force
            } else {
                Write-Host "✅ Current PowerShell session (PID: $($proc.Id)) will be kept alive." -ForegroundColor Green
            }
        }
    }
} catch {
    Write-Host "⚠️ Could not determine or stop locking processes: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Step 3: Unload assemblies in current PowerShell session
Write-Host ""
Write-Host "🔄 Attempting to unload BusBuddy assemblies from current session..." -ForegroundColor Yellow

try {
    # Get loaded assemblies
    $loadedAssemblies = [System.AppDomain]::CurrentDomain.GetAssemblies() | Where-Object {
        $_.Location -like "*BusBuddy*" -and $_.Location -like "*.dll"
    }

    if ($loadedAssemblies) {
        Write-Host "📋 Found loaded BusBuddy assemblies:" -ForegroundColor Yellow
        $loadedAssemblies | ForEach-Object {
            Write-Host "  $($_.Location)" -ForegroundColor Gray
        }

        # Note: We can't truly unload assemblies in .NET Framework/.NET Core
        # But we can clear variables and force garbage collection
        Write-Host ""
        Write-Host "🧹 Forcing garbage collection to release references..." -ForegroundColor Yellow
        [System.GC]::Collect()
        [System.GC]::WaitForPendingFinalizers()
        [System.GC]::Collect()
        Write-Host "✅ Garbage collection completed" -ForegroundColor Green
    } else {
        Write-Host "ℹ️ No BusBuddy assemblies found in current session" -ForegroundColor Blue
    }
} catch {
    Write-Host "⚠️ Error during assembly cleanup: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Step 4: Clean up bin/obj directories
Write-Host ""
Write-Host "🧹 Cleaning build outputs..." -ForegroundColor Yellow

$projectDirs = @(
    (Join-Path $scriptRoot "BusBuddy.Core"),
    (Join-Path $scriptRoot "BusBuddy.WPF"),
    (Join-Path $scriptRoot "BusBuddy.Tests"),
    (Join-Path $scriptRoot "BusBuddy.UITests")
)

foreach ($projectDir in $projectDirs) {
    if (Test-Path $projectDir) {
        $binDir = Join-Path $projectDir "bin"
        $objDir = Join-Path $projectDir "obj"

        if (Test-Path $binDir) {
            try {
                Remove-Item $binDir -Recurse -Force
                Write-Host "✅ Cleaned: $binDir" -ForegroundColor Green
            } catch {
                Write-Host "⚠️ Could not clean $binDir : $($_.Exception.Message)" -ForegroundColor Yellow
            }
        }

        if (Test-Path $objDir) {
            try {
                Remove-Item $objDir -Recurse -Force
                Write-Host "✅ Cleaned: $objDir" -ForegroundColor Green
            } catch {
                Write-Host "⚠️ Could not clean $objDir : $($_.Exception.Message)" -ForegroundColor Yellow
            }
        }
    }
}

# Step 5: Test build
Write-Host ""
Write-Host "🔨 Testing build after cleanup..." -ForegroundColor Cyan

try {
    $solutionFile = Join-Path $scriptRoot "BusBuddy.sln"
    $buildResult = & dotnet build $solutionFile --verbosity minimal --nologo 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ BUILD SUCCESS!" -ForegroundColor Green
        Write-Host ""
        Write-Host "🎯 Solution built successfully. File locks resolved!" -ForegroundColor Green
    } else {
        Write-Host "❌ Build still failing:" -ForegroundColor Red
        $buildResult | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }

        Write-Host ""
        Write-Host "💡 Additional steps to try:" -ForegroundColor Blue
        Write-Host "  1. Close VS Code and any other IDEs, then re-run this script." -ForegroundColor Blue
        Write-Host "  2. Manually kill all PowerShell processes: Get-Process pwsh | Stop-Process -Force" -ForegroundColor Blue
        Write-Host "  3. Restart your computer." -ForegroundColor Blue
    }
} catch {
    Write-Host "❌ Build test failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "🔓 Assembly file lock resolution completed" -ForegroundColor Cyan
