#Requires -Version 7.0
#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Test Terminal Persistence for Bus Buddy Profiles

.DESCRIPTION
    This script tests if the terminal persistence is working correctly by checking
    if Bus Buddy commands are available after clearing the terminal.

    Requires administrator privileges to ensure all features can be properly tested.

.NOTES
    Version: 1.1
    Date: July 19, 2025
    Author: Bus Buddy Team
#>

Write-Host "🔍 Testing Terminal Persistence for Bus Buddy Profiles..." -ForegroundColor Cyan

# Function to check administrator privileges
function Test-AdminPrivileges {
    $isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    return $isAdmin
}

# Function to check if Bus Buddy commands are available
function Test-BusBuddyCommands {
    $commands = @(
        'bb-build', 'bb-run', 'bb-test', 'bb-clean', 'bb-restore',
        'bb-health', 'bb-diagnostic', 'bb-dev-session', 'bb-quick-test'
    )

    $missingCommands = @()
    foreach ($command in $commands) {
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

# Check admin status
$isAdmin = Test-AdminPrivileges
if ($isAdmin) {
    Write-Host "✅ Running with administrator privileges" -ForegroundColor Green

    # Check environment variable for admin mode
    if ($env:BusBuddyAdminMode -eq "true") {
        Write-Host "✅ BusBuddyAdminMode environment variable is correctly set" -ForegroundColor Green
    } else {
        Write-Host "⚠️ BusBuddyAdminMode environment variable is not set correctly" -ForegroundColor Yellow
        $env:BusBuddyAdminMode = "true"
        Write-Host "   Fixed BusBuddyAdminMode environment variable" -ForegroundColor Green
    }
} else {
    Write-Host "❌ Not running with administrator privileges" -ForegroundColor Red
    Write-Host "   Please restart VS Code as administrator for full functionality" -ForegroundColor Yellow
}

# Check if commands are available
$commandCheck = Test-BusBuddyCommands
if ($commandCheck.Available) {
    Write-Host "✅ Bus Buddy commands are available" -ForegroundColor Green
} else {
    Write-Host "❌ Some Bus Buddy commands are missing:" -ForegroundColor Red
    foreach ($cmd in $commandCheck.Missing) {
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
