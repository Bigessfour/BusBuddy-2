#Requires -Version 7.5
<#
.SYNOPSIS
Enhanced BusBuddy build workflow with intelligent command selection

.DESCRIPTION
Automatically detects and uses PowerShell profile commands (bb-*) when available,
falls back to direct dotnet commands with comprehensive error handling and diagnostics.

.PARAMETER Mode
Build mode: Auto (default), PowerShell (force bb- commands), Direct (force dotnet)

.PARAMETER SkipDiagnostics
Skip comprehensive diagnostics to speed up builds

.PARAMETER CleanFirst
Perform clean before build

.EXAMPLE
.\Enhanced-Build-Workflow.ps1 -Mode Auto -CleanFirst
#>

param(
    [ValidateSet("Auto", "PowerShell", "Direct")]
    [string]$Mode = "Auto",

    [switch]$SkipDiagnostics,
    [switch]$CleanFirst,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

function Write-BuildStatus {
    param($Message, $Type = "Info", $SubLevel = 0)

    $indent = "  " * $SubLevel
    $prefix = switch($Type) {
        "Success" { "✅" }
        "Warning" { "⚠️" }
        "Error" { "❌" }
        "Process" { "🔄" }
        "Command" { "🔨" }
        default { "📋" }
    }

    $color = switch($Type) {
        "Success" { "Green" }
        "Warning" { "Yellow" }
        "Error" { "Red" }
        "Process" { "Cyan" }
        "Command" { "Magenta" }
        default { "White" }
    }

    Write-Host "$indent$prefix $Message" -ForegroundColor $color
}

function Test-PowerShellCommands {
    Write-BuildStatus "Checking PowerShell profile integration..." "Process"

    $commands = @("bb-build", "bb-run", "bb-test", "bb-health", "bb-diagnostic")
    $available = @{}

    foreach ($cmd in $commands) {
        $available[$cmd] = (Get-Command $cmd -ErrorAction SilentlyContinue) -ne $null
        $status = if ($available[$cmd]) { "Available" } else { "Missing" }
        $type = if ($available[$cmd]) { "Success" } else { "Warning" }
        Write-BuildStatus "$cmd : $status" $type 1
    }

    $profileScore = ($available.Values | Where-Object { $_ }).Count
    Write-BuildStatus "Profile Commands Available: $profileScore/$($commands.Count)" "Info" 1

    return @{
        Score = $profileScore
        Commands = $available
        HasBuild = $available["bb-build"]
        HasDiagnostics = $available["bb-health"] -or $available["bb-diagnostic"]
    }
}

function Invoke-LoadProfiles {
    Write-BuildStatus "Loading BusBuddy PowerShell profiles..." "Process"

    if (-not (Test-Path ".\load-bus-buddy-profiles.ps1")) {
        Write-BuildStatus "Profile loader not found" "Warning" 1
        return $false
    }

    try {
        & ".\load-bus-buddy-profiles.ps1" -Quiet -ErrorAction Stop
        Write-BuildStatus "Profiles loaded successfully" "Success" 1
        return $true
    } catch {
        Write-BuildStatus "Profile loading failed: $($_.Exception.Message)" "Error" 1
        return $false
    }
}

function Invoke-PreBuildDiagnostics {
    if ($SkipDiagnostics) {
        Write-BuildStatus "Skipping diagnostics (SkipDiagnostics flag)" "Warning"
        return
    }

    Write-BuildStatus "Running pre-build diagnostics..." "Process"

    # Check .NET SDK
    $dotnetVersion = dotnet --version 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-BuildStatus ".NET SDK: $dotnetVersion" "Success" 1
    } else {
        Write-BuildStatus ".NET SDK not found" "Error" 1
        throw "Missing .NET SDK"
    }

    # Check solution file
    if (Test-Path "BusBuddy.sln") {
        Write-BuildStatus "Solution file: Found" "Success" 1
    } else {
        Write-BuildStatus "Solution file: Missing" "Error" 1
        throw "BusBuddy.sln not found"
    }

    # Check for file locks
    $processes = Get-Process pwsh -ErrorAction SilentlyContinue | Where-Object { $_.Id -ne $PID }
    if ($processes) {
        Write-BuildStatus "PowerShell processes detected: $($processes.Count)" "Warning" 1
        Write-BuildStatus "Consider running Fix-File-Locks.ps1 if build fails" "Warning" 2
    } else {
        Write-BuildStatus "No problematic PowerShell processes" "Success" 1
    }
}

function Invoke-BuildWithPowerShell {
    param([hashtable]$ProfileInfo)

    Write-BuildStatus "Using PowerShell profile commands..." "Command"

    if ($CleanFirst) {
        if ($ProfileInfo.Commands["bb-clean"]) {
            Write-BuildStatus "Cleaning with bb-clean..." "Process" 1
            bb-clean
        } else {
            Write-BuildStatus "bb-clean not available, using dotnet clean..." "Process" 1
            dotnet clean BusBuddy.sln
        }
    }

    if ($ProfileInfo.HasBuild) {
        Write-BuildStatus "Building with bb-build..." "Process" 1
        bb-build
    } else {
        Write-BuildStatus "bb-build not available, falling back to dotnet..." "Warning" 1
        Invoke-BuildWithDotnet
    }

    # Run diagnostics if available
    if ($ProfileInfo.HasDiagnostics -and -not $SkipDiagnostics) {
        Write-BuildStatus "Running post-build diagnostics..." "Process" 1
        if ($ProfileInfo.Commands["bb-health"]) {
            bb-health
        } elseif ($ProfileInfo.Commands["bb-diagnostic"]) {
            bb-diagnostic
        }
    }
}

function Invoke-BuildWithDotnet {
    Write-BuildStatus "Using direct dotnet commands..." "Command"

    if ($CleanFirst) {
        Write-BuildStatus "Cleaning solution..." "Process" 1
        dotnet clean BusBuddy.sln
        if ($LASTEXITCODE -ne 0) {
            throw "Clean failed with exit code: $LASTEXITCODE"
        }
    }

    Write-BuildStatus "Building solution..." "Process" 1
    $verbosityLevel = if ($Verbose) { "normal" } else { "minimal" }
    dotnet build BusBuddy.sln --verbosity $verbosityLevel

    if ($LASTEXITCODE -ne 0) {
        throw "Build failed with exit code: $LASTEXITCODE"
    }
}

function Select-BuildMode {
    param([hashtable]$ProfileInfo)

    switch ($Mode) {
        "PowerShell" {
            if ($ProfileInfo.Score -eq 0) {
                Write-BuildStatus "PowerShell mode requested but no profile commands available" "Warning"
                return "Direct"
            }
            return "PowerShell"
        }
        "Direct" {
            return "Direct"
        }
        "Auto" {
            if ($ProfileInfo.Score -ge 2) {
                Write-BuildStatus "Auto mode: Using PowerShell (score: $($ProfileInfo.Score))" "Info"
                return "PowerShell"
            } else {
                Write-BuildStatus "Auto mode: Using Direct (score: $($ProfileInfo.Score))" "Info"
                return "Direct"
            }
        }
    }
}

# Main execution
try {
    Write-BuildStatus "=== Enhanced BusBuddy Build Workflow ===" "Info"
    Write-BuildStatus "Mode: $Mode | Clean: $CleanFirst | Diagnostics: $(-not $SkipDiagnostics)"
    Write-Host ""

    # Step 1: Pre-build diagnostics
    Invoke-PreBuildDiagnostics
    Write-Host ""

    # Step 2: Load profiles and test commands
    $profileLoaded = Invoke-LoadProfiles
    $profileInfo = Test-PowerShellCommands
    Write-Host ""

    # Step 3: Select build mode
    $selectedMode = Select-BuildMode -ProfileInfo $profileInfo
    Write-BuildStatus "Selected build mode: $selectedMode" "Info"
    Write-Host ""

    # Step 4: Execute build
    $startTime = Get-Date

    if ($selectedMode -eq "PowerShell") {
        Invoke-BuildWithPowerShell -ProfileInfo $profileInfo
    } else {
        Invoke-BuildWithDotnet
    }

    $duration = (Get-Date) - $startTime
    Write-Host ""
    Write-BuildStatus "Build completed successfully in $([math]::Round($duration.TotalSeconds, 1)) seconds" "Success"

    # Step 5: Build summary
    Write-BuildStatus "=== Build Summary ===" "Info"
    Write-BuildStatus "Mode Used: $selectedMode" "Info" 1
    Write-BuildStatus "Profile Score: $($profileInfo.Score)/5" "Info" 1
    Write-BuildStatus "Duration: $([math]::Round($duration.TotalSeconds, 1))s" "Info" 1

} catch {
    Write-Host ""
    Write-BuildStatus "Build failed: $($_.Exception.Message)" "Error"
    Write-BuildStatus "Mode attempted: $Mode" "Error" 1
    Write-BuildStatus "Working directory: $PWD" "Error" 1
    exit 1
}
