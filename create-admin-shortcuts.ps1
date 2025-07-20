#Requires -Version 7.0
<#
.SYNOPSIS
    Creates shortcuts for Bus Buddy scripts with administrator privileges

.DESCRIPTION
    This script creates Windows shortcuts that automatically run with
    administrator privileges for key Bus Buddy PowerShell scripts.

.NOTES
    Version: 1.0
    Date: July 19, 2025
    Author: Bus Buddy Team

.EXAMPLE
    # Create admin shortcuts
    .\create-admin-shortcuts.ps1
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [switch]$DesktopShortcuts,

    [Parameter(Mandatory = $false)]
    [switch]$StartMenuShortcuts
)

Write-Host "🔧 Creating Administrator Privilege Shortcuts..." -ForegroundColor Cyan

# Project root directory
$projectRoot = $PSScriptRoot

# List of scripts to create shortcuts for
$scriptsToShortcut = @(
    @{
        Name = "Bus Buddy Admin Terminal"
        Path = "persistent-profile-helper.ps1"
        Description = "Open PowerShell with Bus Buddy profiles and admin privileges"
        Icon = "powershell.exe"
    },
    @{
        Name = "Bus Buddy Health Check (Admin)"
        Path = "Tools\Scripts\health-check.ps1"
        Description = "Run comprehensive health check with admin privileges"
        Icon = "powershell.exe"
    },
    @{
        Name = "Bus Buddy Development Session (Admin)"
        Path = ".vscode\BusBuddy-Advanced-Workflows.ps1"
        Description = "Start Bus Buddy development session with admin privileges"
        Icon = "devenv.exe"
    }
)

# Function to create a shortcut
function New-AdminShortcut {
    param(
        [string]$Name,
        [string]$TargetPath,
        [string]$Arguments,
        [string]$Description,
        [string]$WorkingDirectory,
        [string]$IconLocation,
        [string]$ShortcutPath
    )

    try {
        # Create a Windows Script Host object
        $WshShell = New-Object -ComObject WScript.Shell

        # Create the shortcut
        $Shortcut = $WshShell.CreateShortcut($ShortcutPath)
        $Shortcut.TargetPath = $TargetPath
        $Shortcut.Arguments = $Arguments
        $Shortcut.Description = $Description
        $Shortcut.WorkingDirectory = $WorkingDirectory
        $Shortcut.IconLocation = $IconLocation

        # Save the shortcut
        $Shortcut.Save()

        # Set the shortcut to run as administrator
        $bytes = [System.IO.File]::ReadAllBytes($ShortcutPath)
        $bytes[0x15] = $bytes[0x15] -bor 0x20 # Set the run as administrator flag
        [System.IO.File]::WriteAllBytes($ShortcutPath, $bytes)

        Write-Host "  ✅ Created shortcut: $ShortcutPath" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "  ❌ Failed to create shortcut: $_" -ForegroundColor Red
        return $false
    }
}

# Determine shortcut locations
$desktopPath = [System.Environment]::GetFolderPath('Desktop')
$startMenuPath = [System.Environment]::GetFolderPath('StartMenu')
$startMenuFolder = Join-Path $startMenuPath "Bus Buddy"

# Create Start Menu folder if needed
if ($StartMenuShortcuts -and -not (Test-Path $startMenuFolder)) {
    try {
        New-Item -Path $startMenuFolder -ItemType Directory -Force | Out-Null
        Write-Host "  ✅ Created Start Menu folder: $startMenuFolder" -ForegroundColor Green
    }
    catch {
        Write-Host "  ❌ Failed to create Start Menu folder: $_" -ForegroundColor Red
        $StartMenuShortcuts = $false
    }
}

# Track created shortcuts
$createdShortcuts = 0

# Create shortcuts for each script
ForEach-Object ($script in $scriptsToShortcut) {
    $scriptPath = Join-Path $projectRoot $script.Path

    # Skip if script doesn't exist
    if (-not (Test-Path $scriptPath)) {
        Write-Host "  ⚠️ Script not found: $scriptPath" -ForegroundColor Yellow
        continue
    }

    # PowerShell executable path
    $pwshPath = "C:\Program Files\PowerShell\7\pwsh.exe"

    # Arguments to run script with admin privileges
    $arguments = "-NoExit -ExecutionPolicy Bypass -File `"$scriptPath`""

    # Create Desktop shortcut if requested
    if ($DesktopShortcuts) {
        $shortcutPath = Join-Path $desktopPath "$($script.Name).lnk"
        $result = New-AdminShortcut -Name $script.Name `
                                   -TargetPath $pwshPath `
                                   -Arguments $arguments `
                                   -Description $script.Description `
                                   -WorkingDirectory $projectRoot `
                                   -IconLocation $script.Icon `
                                   -ShortcutPath $shortcutPath
        if ($result) { $createdShortcuts++ }
    }

    # Create Start Menu shortcut if requested
    if ($StartMenuShortcuts) {
        $shortcutPath = Join-Path $startMenuFolder "$($script.Name).lnk"
        $result = New-AdminShortcut -Name $script.Name `
                                   -TargetPath $pwshPath `
                                   -Arguments $arguments `
                                   -Description $script.Description `
                                   -WorkingDirectory $projectRoot `
                                   -IconLocation $script.Icon `
                                   -ShortcutPath $shortcutPath
        if ($result) { $createdShortcuts++ }
    }
}

# Final summary
if ($createdShortcuts -gt 0) {
    Write-Host "`n🎉 Successfully created $createdShortcuts administrator shortcuts!" -ForegroundColor Green

    if ($DesktopShortcuts) {
        Write-Host "  • Desktop shortcuts created in: $desktopPath" -ForegroundColor White
    }

    if ($StartMenuShortcuts) {
        Write-Host "  • Start Menu shortcuts created in: $startMenuFolder" -ForegroundColor White
    }

    Write-Host "`n💡 These shortcuts will automatically run with administrator privileges" -ForegroundColor Yellow
} else {
    Write-Host "`n⚠️ No shortcuts were created" -ForegroundColor Yellow
    Write-Host "  • Use -DesktopShortcuts to create Desktop shortcuts" -ForegroundColor Gray
    Write-Host "  • Use -StartMenuShortcuts to create Start Menu shortcuts" -ForegroundColor Gray
}

Write-Host "`n✅ Shortcut creation complete!" -ForegroundColor Green
