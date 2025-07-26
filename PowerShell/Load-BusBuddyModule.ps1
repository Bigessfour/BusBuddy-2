#Requires -Version 7.0
<#
.SYNOPSIS
    Bus Buddy PowerShell Module Loader

.DESCRIPTION
    Convenient script to load the Bus Buddy PowerShell module with proper path detection
    and environment validation. Handles module reloading and provides status feedback.

.PARAMETER Force
    Force reload the module even if already loaded

.PARAMETER Quiet
    Suppress verbose loading messages

.PARAMETER ShowInfo
    Display module information after loading

.EXAMPLE
    .\Load-BusBuddyModule.ps1

.EXAMPLE
    .\Load-BusBuddyModule.ps1 -Force -ShowInfo

.NOTES
    This script should be run from the Bus Buddy project root directory.
#>

param(
    [switch]$Force,
    [switch]$Quiet,
    [switch]$ShowInfo
)

# Color-coded status messages
function Write-LoaderStatus {
    param([string]$Message, [string]$Status = 'Info')

    $colors = @{
        'Success' = 'Green'
        'Warning' = 'Yellow'
        'Error' = 'Red'
        'Info' = 'Cyan'
    }

    $icons = @{
        'Success' = '✅'
        'Warning' = '⚠️'
        'Error' = '❌'
        'Info' = '🚌'
    }

    if (-not $Quiet) {
        Write-Host "$($icons[$Status]) $Message" -ForegroundColor $colors[$Status]
    }
}

# Validate PowerShell version
if ($PSVersionTable.PSVersion.Major -lt 7) {
    Write-LoaderStatus "PowerShell 7.0+ required (found $($PSVersionTable.PSVersion))" -Status Error
    exit 1
}

# Find project root
$projectRoot = $PWD
$maxDepth = 5
$depth = 0

while ($depth -lt $maxDepth) {
    if (Test-Path (Join-Path $projectRoot "BusBuddy.sln")) {
        break
    }

    $parent = Split-Path $projectRoot -Parent
    if (-not $parent -or $parent -eq $projectRoot) {
        Write-LoaderStatus "Bus Buddy project root not found" -Status Error
        Write-Host "Please run this script from the Bus Buddy project directory." -ForegroundColor Yellow
        exit 1
    }

    $projectRoot = $parent
    $depth++
}

# Locate module file
$modulePath = Join-Path $projectRoot "PowerShell\BusBuddy.psm1"
if (-not (Test-Path $modulePath)) {
    Write-LoaderStatus "Bus Buddy module not found at: $modulePath" -Status Error
    Write-Host "Expected location: PowerShell\BusBuddy.psm1" -ForegroundColor Yellow
    exit 1
}

Write-LoaderStatus "Found Bus Buddy project at: $projectRoot" -Status Success

# Check if module is already loaded
$existingModule = Get-Module -Name "BusBuddy" -ErrorAction SilentlyContinue

if ($existingModule -and -not $Force) {
    Write-LoaderStatus "Bus Buddy module already loaded (version $($existingModule.Version))" -Status Warning
    Write-Host "Use -Force to reload the module." -ForegroundColor Yellow
} else {
    # Remove existing module if force reload
    if ($existingModule -and $Force) {
        Write-LoaderStatus "Removing existing module..." -Status Info
        Remove-Module -Name "BusBuddy" -Force
    }

    # Import the module
    try {
        Write-LoaderStatus "Loading Bus Buddy PowerShell module..." -Status Info
        Import-Module $modulePath -Force:$Force -ErrorAction Stop

        $loadedModule = Get-Module -Name "BusBuddy"
        Write-LoaderStatus "Module loaded successfully (version $($loadedModule.Version))" -Status Success

        # Quick environment check
        $envValid = Test-BusBuddyEnvironment
        if ($envValid) {
            Write-LoaderStatus "Development environment validated" -Status Success
        } else {
            Write-LoaderStatus "Environment validation found issues" -Status Warning
            Write-Host "Run 'bb-env-check' for detailed information." -ForegroundColor Yellow
        }

    } catch {
        Write-LoaderStatus "Failed to load module: $($_.Exception.Message)" -Status Error
        exit 1
    }
}

# Show module info if requested
if ($ShowInfo) {
    Write-Host ""
    Get-BusBuddyInfo
}

# Display quick start commands
if (-not $Quiet) {
    Write-Host ""
    Write-Host "🚀 Quick Start Commands:" -ForegroundColor Cyan
    Write-Host "  bb-dev-session    # Start development session" -ForegroundColor Green
    Write-Host "  bb-build          # Build solution" -ForegroundColor Green
    Write-Host "  bb-run            # Run application" -ForegroundColor Green
    Write-Host "  bb-happiness      # Get motivation 😊" -ForegroundColor Green
    Write-Host "  bb-commands       # Show all commands" -ForegroundColor Green
    Write-Host ""
}
