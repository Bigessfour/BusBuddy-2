#Requires -Version 7.5
<#
.SYNOPSIS
Self-resolving build script that handles PowerShell self-locking scenarios

.DESCRIPTION
Detects when the current PowerShell session is holding file locks and automatically
executes the build in a new, clean PowerShell session to avoid the self-locking issue.

.PARAMETER Force
Force build in new session even if no self-lock detected

.PARAMETER Wait
Wait for the build to complete and display results

.EXAMPLE
.\Self-Resolving-Build.ps1 -Wait
#>

param(
    [switch]$Force,
    [switch]$Wait
)

$ErrorActionPreference = "Stop"

function Write-SelfResolveStatus {
    param($Message, $Type = "Info")
    $color = switch($Type) {
        "Success" { "Green" }
        "Warning" { "Yellow" }
        "Error" { "Red" }
        "Process" { "Cyan" }
        "Critical" { "Magenta" }
        default { "White" }
    }

    $icon = switch($Type) {
        "Success" { "✅" }
        "Warning" { "⚠️" }
        "Error" { "❌" }
        "Process" { "🔄" }
        "Critical" { "🚨" }
        default { "🔧" }
    }

    Write-Host "$icon $Message" -ForegroundColor $color
}

function Test-SelfLockScenario {
    Write-SelfResolveStatus "Checking for self-lock scenario..." "Process"

    $currentPID = $PID
    $possibleDllPaths = @(
        ".\BusBuddy.Core\bin\Debug\net9.0-windows\BusBuddy.Core.dll",
        ".\BusBuddy.Core\bin\Debug\net8.0-windows\BusBuddy.Core.dll"
    )

    $dllPath = $null
    foreach ($path in $possibleDllPaths) {
        if (Test-Path $path) {
            $dllPath = $path
            break
        }
    }

    if (-not $dllPath) {
        Write-SelfResolveStatus "Target DLL not found, no self-lock possible" "Success"
        return $false
    }

    # Try to access the file
    try {
        $fileStream = [System.IO.File]::Open($dllPath, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::Read)
        $fileStream.Close()
        Write-SelfResolveStatus "No file lock detected" "Success"
        return $false
    } catch [System.IO.IOException] {
        if ($_.Exception.Message -like "*being used by another process*") {
            Write-SelfResolveStatus "File lock detected on BusBuddy.Core.dll" "Warning"

            # Check if current process is the one holding the lock
            try {
                $processes = Get-Process pwsh -ErrorAction SilentlyContinue
                $currentSession = $processes | Where-Object { $_.Id -eq $currentPID }

                if ($currentSession) {
                    Write-SelfResolveStatus "Current PowerShell session (PID: $currentPID) may be holding the lock" "Critical"
                    return $true
                }
            } catch {
                Write-SelfResolveStatus "Cannot determine lock source, assuming self-lock" "Warning"
                return $true
            }
        }
    } catch {
        Write-SelfResolveStatus "Unexpected error checking file: $($_.Exception.Message)" "Error"
        return $false
    }

    return $false
}

function Start-CleanBuildSession {
    Write-SelfResolveStatus "Starting clean build session..." "Process"

    $scriptContent = @"
# Clean Build Session Script
Set-Location '$PWD'
Write-Host '🔧 Clean Build Session Started' -ForegroundColor Cyan
Write-Host 'PID: $PID' -ForegroundColor Gray
Write-Host 'Working Directory: $PWD' -ForegroundColor Gray
Write-Host ''

try {
    Write-Host '🧹 Performing cleanup...' -ForegroundColor Yellow
    [System.GC]::Collect()
    [System.GC]::WaitForPendingFinalizers()
    [System.GC]::Collect()

    Write-Host '🔨 Starting build...' -ForegroundColor Cyan
    dotnet build BusBuddy.sln --verbosity normal

    if (`$LASTEXITCODE -eq 0) {
        Write-Host '✅ Build completed successfully!' -ForegroundColor Green
    } else {
        Write-Host '❌ Build failed with exit code: `$LASTEXITCODE' -ForegroundColor Red
        exit `$LASTEXITCODE
    }
} catch {
    Write-Host '❌ Build error: `$(`$_.Exception.Message)' -ForegroundColor Red
    exit 1
}
"@

    $tempScript = ".\temp-clean-build-$([System.Guid]::NewGuid().ToString('N')[0..7] -join '').ps1"

    try {
        Set-Content -Path $tempScript -Value $scriptContent -Encoding UTF8
        Write-SelfResolveStatus "Created temporary build script: $tempScript" "Process"

        if ($Wait) {
            Write-SelfResolveStatus "Starting build in new session (will wait for completion)..." "Process"
            $process = Start-Process pwsh -ArgumentList "-ExecutionPolicy", "Bypass", "-File", $tempScript -Wait -PassThru -NoNewWindow
            $exitCode = $process.ExitCode

            if ($exitCode -eq 0) {
                Write-SelfResolveStatus "Clean build session completed successfully" "Success"
            } else {
                Write-SelfResolveStatus "Clean build session failed with exit code: $exitCode" "Error"
            }

        } else {
            Write-SelfResolveStatus "Starting build in new session (background)..." "Process"
            Start-Process pwsh -ArgumentList "-ExecutionPolicy", "Bypass", "-File", $tempScript -WindowStyle Hidden
            Write-SelfResolveStatus "Background build session started" "Success"
        }

    } finally {
        # Clean up temp script
        Start-Sleep -Seconds 2
        if (Test-Path $tempScript) {
            try {
                Remove-Item $tempScript -Force -ErrorAction SilentlyContinue
                Write-SelfResolveStatus "Cleaned up temporary script" "Success"
            } catch {
                Write-SelfResolveStatus "Could not clean up temporary script: $tempScript" "Warning"
            }
        }
    }
}

function Invoke-NormalBuild {
    Write-SelfResolveStatus "Performing normal build..." "Process"

    try {
        dotnet build BusBuddy.sln --verbosity minimal

        if ($LASTEXITCODE -eq 0) {
            Write-SelfResolveStatus "Normal build completed successfully" "Success"
        } else {
            Write-SelfResolveStatus "Normal build failed with exit code: $LASTEXITCODE" "Error"
            return $false
        }
    } catch {
        Write-SelfResolveStatus "Normal build error: $($_.Exception.Message)" "Error"
        return $false
    }

    return $true
}

# Main execution
try {
    Write-SelfResolveStatus "=== BusBuddy Self-Resolving Build Script ===" "Process"
    Write-SelfResolveStatus "Current PID: $PID | Force: $Force | Wait: $Wait"
    Write-Host ""

    # Check for self-lock scenario
    $isSelfLocked = Test-SelfLockScenario

    if ($isSelfLocked -or $Force) {
        if ($Force) {
            Write-SelfResolveStatus "Force mode: Building in clean session" "Warning"
        } else {
            Write-SelfResolveStatus "Self-lock detected: Building in clean session" "Critical"
        }

        Start-CleanBuildSession

    } else {
        Write-SelfResolveStatus "No self-lock detected: Proceeding with normal build" "Process"
        $success = Invoke-NormalBuild

        if (-not $success) {
            Write-SelfResolveStatus "Normal build failed, trying clean session as fallback..." "Warning"
            Start-CleanBuildSession
        }
    }

    Write-SelfResolveStatus "Self-resolving build script completed" "Success"

} catch {
    Write-SelfResolveStatus "Self-resolving build failed: $($_.Exception.Message)" "Error"
    exit 1
}
