#Requires -Version 7.0
#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Fix Terminal Configuration for Bus Buddy PowerShell Profiles

.DESCRIPTION
    This script updates VS Code terminal configuration to properly handle the "Clear Terminal"
    action while preserving PowerShell profile state. It ensures the Bus Buddy advanced workflows
    menu remains available after clearing the terminal.

    Requires administrator privileges to properly configure terminal settings.

.NOTES
    Version: 1.1
    Date: July 19, 2025
    Author: Bus Buddy Team

.EXAMPLE
    # Fix terminal configuration
    .\fix-terminal-config.ps1

.EXAMPLE
    # Fix terminal configuration with backup
    .\fix-terminal-config.ps1 -Backup
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [switch]$Backup
)

# Check for administrator privileges
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "❌ This script requires administrator privileges" -ForegroundColor Red
    Write-Host "   Please run PowerShell as Administrator and try again" -ForegroundColor Yellow
    return $false
}

Write-Host "🔐 Running with administrator privileges" -ForegroundColor Green
Write-Host "🔧 Fixing VS Code Terminal Configuration..." -ForegroundColor Cyan

# Find project root
$projectRoot = $PSScriptRoot
$vscodeDir = Join-Path $projectRoot ".vscode"
$settingsPath = Join-Path $vscodeDir "settings.json"

# Check if settings.json exists
if (-not (Test-Path $settingsPath)) {
    Write-Host "❌ VS Code settings.json not found: $settingsPath" -ForegroundColor Red
    Write-Host "💡 Run verify-terminal-settings.ps1 -Fix first" -ForegroundColor Yellow
    return $false
}

# Backup existing settings if requested
if ($Backup) {
    $backupPath = "$settingsPath.bak-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
    try {
        Copy-Item -Path $settingsPath -Destination $backupPath -Force
        Write-Host "✅ Backed up existing settings to: $backupPath" -ForegroundColor Green
    } catch {
        Write-Host "⚠️ Failed to create backup: $_" -ForegroundColor Yellow
    }
}

# Read and parse settings.json
try {
    $settings = Get-Content -Path $settingsPath -Raw | ConvertFrom-Json
} catch {
    Write-Host "❌ Failed to parse settings.json: $_" -ForegroundColor Red
    return $false
}

# First, create the persistence-friendly profile helper script
$persistentHelperPath = Join-Path $projectRoot "persistent-profile-helper.ps1"
$persistentHelperContent = @'
#Requires -Version 7.0
<#
.SYNOPSIS
    Persistent Profile Helper for Bus Buddy PowerShell Profiles

.DESCRIPTION
    This script provides persistence for Bus Buddy PowerShell profiles when
    using terminal actions like "Clear Terminal". It maintains environment
    variables that track profile state across terminal sessions.

.NOTES
    Version: 1.0
    Date: July 19, 2025
    Author: Bus Buddy Team
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [switch]$Quiet
)

# Set persistent flag
if (-not (Test-Path env:BusBuddyProfilesLoaded)) {
    $env:BusBuddyProfilesLoaded = "false"
}

# Clear-Host with proper terminal handling
function global:Clear-Host {
    if ($host.Name -eq 'Visual Studio Code Host') {
        # Use ANSI escape sequence for VS Code terminal
        $ESC = [char]0x1b
        Write-Host "$ESC[2J$ESC[0;0H" -NoNewline
    }
    else {
        # Fall back to default implementation for other hosts
        $originalClearHost = Get-Alias -Name cls -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Definition
        if ($originalClearHost) {
            & $originalClearHost
        }
        else {
            # Ultimate fallback
            [Console]::Clear()
        }
    }
}

# Set aliases
Set-Alias -Name cls -Value Clear-Host -Force -Scope Global
Set-Alias -Name clear -Value Clear-Host -Force -Scope Global

# Persistent profile loader function
function Import-BusBuddyProfiles {
    param(
        [switch]$ForceReload,
        [switch]$Quiet
    )

    $projectRoot = $PSScriptRoot
    $mainProfilePath = Join-Path $projectRoot "BusBuddy-PowerShell-Profile.ps1"
    $advancedWorkflowsPath = Join-Path $projectRoot ".vscode\BusBuddy-Advanced-Workflows.ps1"

    # Skip loading if already loaded (unless force reload)
    if ($env:BusBuddyProfilesLoaded -eq "true" -and -not $ForceReload) {
        if (-not $Quiet) {
            Write-Host "ℹ️ Bus Buddy profiles already loaded" -ForegroundColor Cyan
        }
        return $true
    }

    # Load profiles
    $loadSuccess = $true

    if (-not $Quiet) {
        Write-Host "🔄 Loading Bus Buddy PowerShell profiles..." -ForegroundColor Cyan
    }

    # Load main profile
    if (Test-Path $mainProfilePath) {
        try {
            . $mainProfilePath
            if (-not $Quiet) {
                Write-Host "✅ Loaded main profile: $mainProfilePath" -ForegroundColor Green
            }
        } catch {
            Write-Host "❌ Failed to load main profile: $_" -ForegroundColor Red
            $loadSuccess = $false
        }
    } else {
        Write-Host "❌ Main profile not found: $mainProfilePath" -ForegroundColor Red
        $loadSuccess = $false
    }

    # Load advanced workflows
    if (Test-Path $advancedWorkflowsPath) {
        try {
            # Set a quiet flag that the script will use
            $Script:BusBuddyAdvancedWorkflowsQuiet = $Quiet

            # Dot source using Invoke-Expression to ensure the entire script is executed
            Invoke-Expression (". '$advancedWorkflowsPath' -Quiet:`$$Quiet")

            if (-not $Quiet) {
                Write-Host "✅ Loaded advanced workflows: $advancedWorkflowsPath" -ForegroundColor Green
            }
        } catch {
            Write-Host "⚠️ Failed to load advanced workflows: $_" -ForegroundColor Yellow
            $loadSuccess = $false
        }
    } else {
        Write-Host "⚠️ Advanced workflows not found: $advancedWorkflowsPath" -ForegroundColor Yellow
    }

    # Mark as loaded
    if ($loadSuccess) {
        $env:BusBuddyProfilesLoaded = "true"

        if (-not $Quiet) {
            Write-Host "`n🚌 Bus Buddy PowerShell profiles loaded!" -ForegroundColor Green
            Write-Host "💡 You should now have access to all Bus Buddy commands" -ForegroundColor Yellow
        }
    }

    return $loadSuccess
}

# Load profiles if not loaded yet
if ($env:BusBuddyProfilesLoaded -ne "true") {
    Import-BusBuddyProfiles -Quiet:$Quiet
}

# Create reload command
function global:Update-BusBuddyProfiles {
    param(
        [switch]$Quiet
    )

    $env:BusBuddyProfilesLoaded = "false"
    Import-BusBuddyProfiles -ForceReload -Quiet:$Quiet
}

# Create alias for reload
Set-Alias -Name bb-reload -Value Update-BusBuddyProfiles -Force -Scope Global

return $true
'@

# Write-Output the persistent helper script
try {
    Set-Content -Path $persistentHelperPath -Value $persistentHelperContent -Force
    Write-Host "✅ Created persistent profile helper: $persistentHelperPath" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to create persistent helper: $_" -ForegroundColor Red
    return $false
}

# Update terminal profile settings to use the persistent helper
$hasChanges = $false

# Check if terminal settings exist and create if needed
if (-not $settings.PSObject.Properties["terminal.integrated.profiles.windows"]) {
    $settings | Add-Member -NotePropertyName "terminal.integrated.profiles.windows" -NotePropertyValue @{} -Force
    $hasChanges = $true
}

# Check if PowerShell 7.5.2 profile exists and create if needed
if (-not $settings.'terminal.integrated.profiles.windows'.PSObject.Properties["PowerShell 7.5.2"]) {
    $settings.'terminal.integrated.profiles.windows' | Add-Member -NotePropertyName "PowerShell 7.5.2" -NotePropertyValue @{
        "path" = "pwsh.exe"
        "args" = @(
            "-NoProfile"
            "-NoExit"
            "-Command"
            ". '`${workspaceFolder}\\persistent-profile-helper.ps1'"
        )
        "icon" = "terminal-powershell"
    } -Force
    $hasChanges = $true
} else {
    # Update existing profile to use the persistent helper
    $pwshProfile = $settings.'terminal.integrated.profiles.windows'.'PowerShell 7.5.2'

    # Update args array
    if ($pwshProfile.PSObject.Properties["args"]) {
        $commandIndex = [array]::IndexOf($pwshProfile.args, "-Command")
        if ($commandIndex -ge 0 -and $commandIndex -lt $pwshProfile.args.Count - 1) {
            $pwshProfile.args[$commandIndex + 1] = ". '`${workspaceFolder}\\persistent-profile-helper.ps1'"
            $hasChanges = $true
        } else {
            # Args array exists but doesn't have -Command
            $pwshProfile.args = @(
                "-NoProfile"
                "-NoExit"
                "-Command"
                ". '`${workspaceFolder}\\persistent-profile-helper.ps1'"
            )
            $hasChanges = $true
        }
    } else {
        # No args property, add it
        $pwshProfile | Add-Member -NotePropertyName "args" -NotePropertyValue @(
            "-NoProfile"
            "-NoExit"
            "-Command"
            "& '`${workspaceFolder}\\persistent-profile-helper.ps1'"
        ) -Force
        $hasChanges = $true
    }
}

# Set default profile if not already set
if (-not $settings.PSObject.Properties["terminal.integrated.defaultProfile.windows"] -or
    $settings.'terminal.integrated.defaultProfile.windows' -ne "PowerShell 7.5.2") {
    $settings | Add-Member -NotePropertyName "terminal.integrated.defaultProfile.windows" -NotePropertyValue "PowerShell 7.5.2" -Force
    $hasChanges = $true
}

# Enable terminal session persistence
if (-not $settings.PSObject.Properties["terminal.integrated.persistentSessionReviveProcess"] -or
    $settings.'terminal.integrated.persistentSessionReviveProcess' -ne "onExit") {
    $settings | Add-Member -NotePropertyName "terminal.integrated.persistentSessionReviveProcess" -NotePropertyValue "onExit" -Force
    $hasChanges = $true
}

# Ensure PowerShell profile loading is enabled
if (-not $settings.PSObject.Properties["powershell.enableProfileLoading"] -or
    -not $settings.'powershell.enableProfileLoading') {
    $settings | Add-Member -NotePropertyName "powershell.enableProfileLoading" -NotePropertyValue $true -Force
    $hasChanges = $true
}

# Save changes if needed
if ($hasChanges) {
    try {
        $settings | ConvertTo-Json -Depth 10 | Set-Content -Path $settingsPath -Force
        Write-Host "✅ Updated VS Code settings for persistent terminal profile" -ForegroundColor Green
    } catch {
        Write-Host "❌ Failed to update settings.json: $_" -ForegroundColor Red
        return $false
    }
} else {
    Write-Host "ℹ️ No changes needed to VS Code settings" -ForegroundColor Gray
}

# Create a test file
$testFilePath = Join-Path $projectRoot "test-terminal-persistence.ps1"
$testFileContent = @'
#Requires -Version 7.0
<#
.SYNOPSIS
    Test Terminal Persistence for Bus Buddy Profiles

.DESCRIPTION
    This script tests if the terminal persistence is working correctly by checking
    if Bus Buddy commands are available after clearing the terminal.

.NOTES
    Version: 1.0
    Date: July 19, 2025
    Author: Bus Buddy Team
#>

Write-Host "🔍 Testing Terminal Persistence for Bus Buddy Profiles..." -ForegroundColor Cyan

# Function to check if Bus Buddy commands are available
function Test-BusBuddyCommands {
    $commands = @(
        'bb-build', 'bb-run', 'bb-test', 'bb-clean', 'bb-restore',
        'bb-health', 'bb-diagnostic', 'bb-dev-session', 'bb-quick-test'
    )

    $missingCommands = @()
    ForEach-Object ($command in $commands) {
        if (-not (Get-Command $command -ErrorAction SilentlyContinue)) {
            $missingCommands += $command
        }
    }

    return @{
        Available = $missingCommands.Count -eq 0
        Missing = $missingCommands
    }
}

# Check if Bus Buddy environment variable is set
$profilesLoaded = $env:BusBuddyProfilesLoaded -eq "true"
if ($profilesLoaded) {
    Write-Host "✅ Bus Buddy profiles marker is set" -ForegroundColor Green
} else {
    Write-Host "❌ Bus Buddy profiles marker is not set" -ForegroundColor Red
}

# Check if commands are available
$commandCheck = Test-BusBuddyCommands
if ($commandCheck.Available) {
    Write-Host "✅ Bus Buddy commands are available" -ForegroundColor Green
} else {
    Write-Host "❌ Some Bus Buddy commands are missing:" -ForegroundColor Red
    ForEach-Object ($cmd in $commandCheck.Missing) {
        Write-Host "  • $cmd" -ForegroundColor Red
    }
}

# Test Clear-Host function
Write-Host "`n💡 Testing Clear-Host function..." -ForegroundColor Yellow
$clearHostExists = $null -ne (Get-Command -Name Clear-Host -ErrorAction SilentlyContinue)
if ($clearHostExists) {
    Write-Host "✅ Clear-Host function is available" -ForegroundColor Green

    # Show function definition
    Write-Host "`n📋 Current Clear-Host function definition:" -ForegroundColor Cyan
    ${function:Clear-Host}
} else {
    Write-Host "❌ Clear-Host function is not available" -ForegroundColor Red
}

Write-Host "`n📋 Terminal Persistence Test:" -ForegroundColor Cyan
Write-Host "1. First, try clearing the terminal using one of these methods:" -ForegroundColor Gray
Write-Host "   • Click the trash can icon in the terminal toolbar" -ForegroundColor Gray
Write-Host "   • Press Ctrl+K in the terminal" -ForegroundColor Gray
Write-Host "   • Type 'cls' or 'clear' in the terminal" -ForegroundColor Gray
Write-Host "`n2. After clearing, run this test script again to verify persistence:" -ForegroundColor Gray
Write-Host "   .\test-terminal-persistence.ps1" -ForegroundColor White
Write-Host "`n3. If commands are missing after clearing, use:" -ForegroundColor Gray
Write-Host "   bb-reload" -ForegroundColor White

# Reload option
if (-not $commandCheck.Available -and -not $profilesLoaded) {
    Write-Host "`n💡 Attempting to reload profiles..." -ForegroundColor Yellow
    . (Join-Path $PSScriptRoot "persistent-profile-helper.ps1")

    # Check again
    $commandCheck = Test-BusBuddyCommands
    if ($commandCheck.Available) {
        Write-Host "✅ Successfully reloaded Bus Buddy profiles" -ForegroundColor Green
    } else {
        Write-Host "❌ Failed to reload profiles automatically" -ForegroundColor Red
    }
}
'@

try {
    Set-Content -Path $testFilePath -Value $testFileContent -Force
    Write-Host "✅ Created terminal persistence test script: $testFilePath" -ForegroundColor Green
} catch {
    Write-Host "⚠️ Failed to create test script: $_" -ForegroundColor Yellow
}

Write-Host "`n🎉 Terminal Configuration Fix Complete!" -ForegroundColor Green
Write-Host "`n📋 Next Steps:" -ForegroundColor Cyan
Write-Host "1. Restart VS Code to apply the changes" -ForegroundColor Gray
Write-Host "2. Open a new terminal (Ctrl+`)" -ForegroundColor Gray
Write-Host "3. Verify Bus Buddy commands are available by typing:" -ForegroundColor Gray
Write-Host "   bb-health" -ForegroundColor White
Write-Host "4. Try clearing the terminal using the trash icon or Ctrl+K" -ForegroundColor Gray
Write-Host "5. After clearing, check if commands still work by running:" -ForegroundColor Gray
Write-Host "   .\test-terminal-persistence.ps1" -ForegroundColor White
Write-Host "`n💡 If issues persist, you can manually reload the profiles with:" -ForegroundColor Yellow
Write-Host "   bb-reload" -ForegroundColor White

return $true
