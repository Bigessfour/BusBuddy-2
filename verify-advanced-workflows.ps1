#Requires -Version 7.0
<#
.SYNOPSIS
    Verify Bus Buddy Advanced Workflows Loading

.DESCRIPTION
    This script verifies that the advanced workflows are fully loaded
    and displays the available commands to ensure everything is working correctly.

.NOTES
    Version: 1.0
    Date: July 19, 2025
    Author: Bus Buddy Team
#>

[CmdletBinding()]
param()

# First, reload the profiles
Write-Host "🔄 Reloading Bus Buddy profiles..." -ForegroundColor Cyan

# Check if reload function exists
if (Get-Command -Name Update-BusBuddyProfiles -ErrorAction SilentlyContinue) {
    Update-BusBuddyProfiles
} else {
    # Try loading directly
    $loadDirect = Join-Path $PSScriptRoot "load-direct.ps1"
    if (Test-Path $loadDirect) {
        . $loadDirect
    } else {
        Write-Host "❌ Cannot find load-direct.ps1" -ForegroundColor Red
        exit 1
    }
}

# Verify advanced workflow commands are available
Write-Host "`n🔍 Checking for advanced workflow commands..." -ForegroundColor Cyan

$expectedCommands = @(
    'bb-build',
    'bb-clean',
    'bb-test',
    'bb-restore',
    'bb-run',
    'bb-publish',
    'bb-dev-session',
    'bb-quick-test',
    'bb-diagnostic',
    'bb-health',
    'bb-report'
)

$availableCommands = @()
$missingCommands = @()

foreach ($command in $expectedCommands) {
    if (Get-Command -Name $command -ErrorAction SilentlyContinue) {
        $availableCommands += $command
    } else {
        $missingCommands += $command
    }
}

# Display results
Write-Host "`n📊 Advanced Workflows Verification Results:" -ForegroundColor Cyan
Write-Host "  Commands found: $($availableCommands.Count) of $($expectedCommands.Count)" -ForegroundColor White

if ($availableCommands.Count -gt 0) {
    Write-Host "`n✅ Available commands:" -ForegroundColor Green
    foreach ($command in $availableCommands) {
        Write-Host "  • $command" -ForegroundColor Gray
    }
}

if ($missingCommands.Count -gt 0) {
    Write-Host "`n❌ Missing commands:" -ForegroundColor Red
    foreach ($command in $missingCommands) {
        Write-Host "  • $command" -ForegroundColor Red
    }

    Write-Host "`n⚠️ Advanced workflows may not be properly loaded." -ForegroundColor Yellow
    Write-Host "💡 Try running: . `"$PSScriptRoot\.vscode\BusBuddy-Advanced-Workflows.ps1`"" -ForegroundColor Yellow
} else {
    Write-Host "`n🎉 All advanced workflow commands are available!" -ForegroundColor Green
    Write-Host "💡 Your Bus Buddy development environment is properly configured." -ForegroundColor Cyan
}

# Check if welcome message function exists
$welcomeFunctionName = "Show-BusBuddyAdvancedWorkflowsWelcome"
$welcomeFunction = Get-ChildItem function:$welcomeFunctionName -ErrorAction SilentlyContinue

if ($welcomeFunction) {
    Write-Host "`n🔄 Displaying advanced workflows welcome message..." -ForegroundColor Cyan
    & $welcomeFunction
} else {
    Write-Host "`n💡 For a complete list of available commands, run:" -ForegroundColor Yellow
    Write-Host "  Get-Command -Name bb-* | Sort-Object Name | Format-Table -Property Name" -ForegroundColor Gray
}
