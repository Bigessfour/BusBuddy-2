#Requires -Version 7.0
<#
.SYNOPSIS
    Initialize Bus Buddy PowerShell Development Environment for VS Code
.DESCRIPTION
    This script properly initializes the Bus Buddy development environment
    by working with the PowerShell extension to load profiles correctly.
.NOTES
    Use this script to ensure proper PowerShell 7.5.2 and extension integration
#>

param(
    [switch]$Quiet,
    [switch]$Force,
    [switch]$ShowVersion
)

# Initialize environment
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Continue'

# Clear any previous errors
$Error.Clear()

if ($ShowVersion) {
    Write-Host "=== PowerShell Environment Information ===" -ForegroundColor Cyan
    Write-Host "PowerShell Version: $($PSVersionTable.PSVersion)" -ForegroundColor Green
    Write-Host "Platform: $($PSVersionTable.Platform)" -ForegroundColor Green
    Write-Host "OS: $($PSVersionTable.OS)" -ForegroundColor Green
    Write-Host "Working Directory: $PWD" -ForegroundColor Green

    # Check if we're in VS Code
    if ($env:VSCODE_PID) {
        Write-Host "Running in VS Code (PID: $env:VSCODE_PID)" -ForegroundColor Blue
    }

    # Check PowerShell extension
    if (Get-Module -Name PowerShellEditorServices* -ErrorAction SilentlyContinue) {
        Write-Host "PowerShell Extension: Connected" -ForegroundColor Green
    } else {
        Write-Host "PowerShell Extension: Not detected" -ForegroundColor Yellow
    }
}

# Find Bus Buddy project root
$projectRoot = $PWD.Path
$maxDepth = 3
$depth = 0

while ($depth -lt $maxDepth) {
    if (Test-Path (Join-Path $projectRoot "BusBuddy.sln")) {
        break
    }
    $parent = Split-Path $projectRoot -Parent
    if ($parent -eq $projectRoot) { break }
    $projectRoot = $parent
    $depth++
}

if (-not (Test-Path (Join-Path $projectRoot "BusBuddy.sln"))) {
    Write-Warning "Bus Buddy solution not found. Please run from Bus Buddy project directory."
    exit 1
}

# Set working directory
Set-Location $projectRoot

if (-not $Quiet) {
    Write-Host "🚌 Bus Buddy Development Environment" -ForegroundColor Green
    Write-Host "   Project Root: $projectRoot" -ForegroundColor Cyan
    Write-Host "   PowerShell: $($PSVersionTable.PSVersion)" -ForegroundColor Cyan
}

# Load Bus Buddy profiles
$profileLoader = Join-Path $projectRoot "load-bus-buddy-profiles.ps1"
if (Test-Path $profileLoader) {
    try {
        & $profileLoader -Quiet:$Quiet -Force:$Force
        if (-not $Quiet) {
            Write-Host "✅ Bus Buddy profiles loaded successfully" -ForegroundColor Green
        }
    } catch {
        Write-Warning "Failed to load Bus Buddy profiles: $_"
    }
} else {
    if (-not $Quiet) {
        Write-Host "⚠️ Bus Buddy profile loader not found" -ForegroundColor Yellow
    }
}

# Verify commands are available
$commands = @('bb-build', 'bb-run', 'bb-test', 'bb-health')
$availableCommands = $commands | Where-Object { Get-Command $_ -ErrorAction SilentlyContinue }

if (-not $Quiet) {
    if ($availableCommands.Count -gt 0) {
        Write-Host "🔧 Available Bus Buddy commands:" -ForegroundColor Green
        $availableCommands | ForEach-Object { Write-Host "   • $_" -ForegroundColor White }
    } else {
        Write-Host "⚠️ No Bus Buddy commands available - profiles may not have loaded" -ForegroundColor Yellow
        Write-Host "   You can still use standard dotnet commands" -ForegroundColor Cyan
    }
}

# Set terminal title if in VS Code
if ($env:VSCODE_PID -and -not $Quiet) {
    $Host.UI.RawUI.WindowTitle = "Bus Buddy PowerShell 7.5.2"
}

if (-not $Quiet) {
    Write-Host ""
    Write-Host "🚀 Ready for Bus Buddy development!" -ForegroundColor Green
    Write-Host "   Use 'bb-build' to build, 'bb-run' to run, 'bb-test' to test" -ForegroundColor Cyan
    Write-Host ""
}
