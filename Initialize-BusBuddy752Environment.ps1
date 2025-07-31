#Requires -Version 7.5
<#
.SYNOPSIS
    BusBuddy PowerShell 7.5.2 Environment Initializer
.DESCRIPTION
    PowerShell 7.5.2 compliant initialization script following Microsoft guidelines.
    Replaces custom profile loading with standard PowerShell practices.

    Reference: https://learn.microsoft.com/en-us/powershell/scripting/learn/shell/creating-profiles
.PARAMETER InstallProfile
    Install the profile to the standard PowerShell profile location
.PARAMETER Force
    Force overwrite existing profile
.EXAMPLE
    .\Initialize-BusBuddy752Environment.ps1
.EXAMPLE
    .\Initialize-BusBuddy752Environment.ps1 -InstallProfile
#>

[CmdletBinding()]
param(
    [switch]$InstallProfile,
    [switch]$Force,
    [switch]$Quiet
)

#region PowerShell 7.5.2 Standard Initialization

## Validate PowerShell version (Microsoft requirement)
if ($PSVersionTable.PSVersion -lt [Version]'7.5.0') {
    throw "PowerShell 7.5+ required for BusBuddy. Current version: $($PSVersionTable.PSVersion)"
}

## Set working directory to BusBuddy workspace
$BusBuddyWorkspace = $PSScriptRoot
if (-not (Test-Path (Join-Path $BusBuddyWorkspace "BusBuddy.sln"))) {
    throw "BusBuddy.sln not found. Please run from BusBuddy workspace root."
}

Set-Location $BusBuddyWorkspace

## Function to write status (PowerShell 7.5.2 pattern)
function Write-InitStatus {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Message,

        [ValidateSet('Info', 'Success', 'Warning', 'Error')]
        [string]$Level = 'Info'
    )

    if ($Quiet) { return }

    $Colors = @{
        'Info'    = 'Cyan'
        'Success' = 'Green'
        'Warning' = 'Yellow'
        'Error'   = 'Red'
    }

    Write-Host $Message -ForegroundColor $Colors[$Level]
}

#endregion

#region BusBuddy Module Loading (PowerShell 7.5.2 Compliant)

Write-InitStatus "🚌 Initializing BusBuddy PowerShell 7.5.2 Environment..." -Level Info

## Import BusBuddy module using standard PowerShell patterns
$BusBuddyModulePath = Join-Path $BusBuddyWorkspace "PowerShell\BusBuddy PowerShell Environment\Modules\BusBuddy"

if (-not (Test-Path $BusBuddyModulePath)) {
    throw "BusBuddy module not found at: $BusBuddyModulePath"
}

try {
    # Standard PowerShell 7.5.2 module import
    Import-Module $BusBuddyModulePath -Force -DisableNameChecking
    Write-InitStatus "✅ BusBuddy module loaded successfully" -Level Success
}
catch {
    Write-InitStatus "❌ Failed to load BusBuddy module: $($_.Exception.Message)" -Level Error
    throw
}

## Load optional AI integration modules
$AIModulePath = Join-Path $BusBuddyWorkspace "PowerShell\BusBuddy PowerShell Environment\Modules\BusBuddy.AI"
if (Test-Path $AIModulePath) {
    try {
        Import-Module $AIModulePath -Force -DisableNameChecking
        Write-InitStatus "✅ BusBuddy.AI module loaded" -Level Success
    }
    catch {
        Write-InitStatus "⚠️  BusBuddy.AI module failed to load: $($_.Exception.Message)" -Level Warning
    }
}

#endregion

#region Profile Installation (PowerShell 7.5.2 Standard)

if ($InstallProfile) {
    Write-InitStatus "📄 Installing PowerShell 7.5.2 compliant profile..." -Level Info

    ## Check if profile exists (Microsoft pattern)
    $ProfileExists = Test-Path $PROFILE.CurrentUserCurrentHost

    if ($ProfileExists -and -not $Force) {
        $Response = Read-Host "Profile exists at $($PROFILE.CurrentUserCurrentHost). Overwrite? (y/N)"
        if ($Response -ne 'y' -and $Response -ne 'Y') {
            Write-InitStatus "Profile installation cancelled" -Level Warning
            return
        }
    }

    ## Create profile directory if needed (Microsoft recommended)
    $ProfileDir = Split-Path $PROFILE.CurrentUserCurrentHost -Parent
    if (-not (Test-Path $ProfileDir)) {
        New-Item -ItemType Directory -Path $ProfileDir -Force | Out-Null
    }

    ## Copy compliant profile
    $SourceProfile = Join-Path $BusBuddyWorkspace "PowerShell\Microsoft.PowerShell_profile.ps1"
    if (Test-Path $SourceProfile) {
        Copy-Item $SourceProfile $PROFILE.CurrentUserCurrentHost -Force
        Write-InitStatus "✅ PowerShell 7.5.2 profile installed to: $($PROFILE.CurrentUserCurrentHost)" -Level Success
        Write-InitStatus "   Restart PowerShell or run: . `$PROFILE" -Level Info
    } else {
        Write-InitStatus "❌ Source profile not found: $SourceProfile" -Level Error
    }
}

#endregion

#region Environment Validation

Write-InitStatus "🔍 Validating BusBuddy environment..." -Level Info

## Check available commands
$BusBuddyCommands = Get-Command bb-* -ErrorAction SilentlyContinue
Write-InitStatus "   Available commands: $($BusBuddyCommands.Count)" -Level Info

## Check workspace structure
$RequiredPaths = @(
    "BusBuddy.sln",
    "BusBuddy.WPF\BusBuddy.WPF.csproj",
    "BusBuddy.Core\BusBuddy.Core.csproj"
)

$PathsValid = $true
foreach ($Path in $RequiredPaths) {
    if (-not (Test-Path (Join-Path $BusBuddyWorkspace $Path))) {
        Write-InitStatus "   ❌ Missing: $Path" -Level Warning
        $PathsValid = $false
    }
}

if ($PathsValid) {
    Write-InitStatus "   ✅ Workspace structure valid" -Level Success
} else {
    Write-InitStatus "   ⚠️  Some workspace components missing" -Level Warning
}

#endregion

#region Usage Information

Write-InitStatus "" -Level Info
Write-InitStatus "🎯 BusBuddy PowerShell 7.5.2 Environment Ready!" -Level Success
Write-InitStatus "   Workspace: $BusBuddyWorkspace" -Level Info
Write-InitStatus "   Available commands:" -Level Info
Write-InitStatus "     bb-build    - Build the solution" -Level Info
Write-InitStatus "     bb-run      - Run the application" -Level Info
Write-InitStatus "     bb-test     - Run tests" -Level Info
Write-InitStatus "     bb-clean    - Clean build artifacts" -Level Info
Write-InitStatus "     bb-health   - Environment health check" -Level Info
Write-InitStatus "" -Level Info
Write-InitStatus "💡 To install permanent profile: .\Initialize-BusBuddy752Environment.ps1 -InstallProfile" -Level Info

#endregion
