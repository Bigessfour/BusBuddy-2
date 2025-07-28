#!/usr/bin/env pwsh
#Requires -Version 7.5

<#
.SYNOPSIS
    BusBuddy Help System
.DESCRIPTION
    Provides help and documentation for BusBuddy PowerShell commands
.NOTES
    File Name      : bb-help.ps1
    Author         : BusBuddy Development Team
    Prerequisite   : PowerShell 7.5.2
    Copyright 2025 - BusBuddy
#>

param (
    [Parameter(Position = 0, Mandatory = $false)]
    [string]$Command,

    [Parameter(Mandatory = $false)]
    [switch]$List,

    [Parameter(Mandatory = $false)]
    [switch]$Detailed,

    [Parameter(Mandatory = $false)]
    [switch]$ShowExamples
)

function Write-BusBuddyHeader {
    Write-Host ""
    Write-Host "🚌 BusBuddy PowerShell Command Reference" -ForegroundColor Cyan
    Write-Host "=======================================" -ForegroundColor Cyan
    Write-Host ""
}

function Write-CommandHelp {
    param (
        [string]$Name,
        [string]$Description,
        [string]$Usage,
        [string[]]$Examples = @(),
        [bool]$ShowExamples = $false
    )

    Write-Host "$Name" -ForegroundColor Yellow -NoNewline
    Write-Host " - $Description"

    if ($Detailed) {
        Write-Host "  Usage: " -ForegroundColor Gray -NoNewline
        Write-Host "$Usage"

        if ($ShowExamples -and $Examples.Count -gt 0) {
            Write-Host "  Examples:" -ForegroundColor Gray
            foreach ($example in $Examples) {
                Write-Host "    $example" -ForegroundColor DarkCyan
            }
            Write-Host ""
        }
    }
}

# Define all BusBuddy commands
$commands = @(
    @{
        Name = "bb-build"
        Description = "Build the BusBuddy solution with comprehensive error reporting"
        Usage = "bb-build [-Clean] [-Verbose] [-Configuration <Debug|Release>]"
        Examples = @(
            "bb-build",
            "bb-build -Clean",
            "bb-build -Configuration Release"
        )
    },
    @{
        Name = "bb-run"
        Description = "Run the BusBuddy application with runtime monitoring and diagnostics"
        Usage = "bb-run [-Debug] [-NoBuild] [-Configuration <Debug|Release>]"
        Examples = @(
            "bb-run",
            "bb-run -Debug",
            "bb-run -NoBuild"
        )
    },
    @{
        Name = "bb-health"
        Description = "Perform health checks on the BusBuddy development environment"
        Usage = "bb-health [-Quick] [-Verbose] [-Export <FilePath>]"
        Examples = @(
            "bb-health",
            "bb-health -Quick",
            "bb-health -Export ./health-report.json"
        )
    },
    @{
        Name = "bb-clean"
        Description = "Clean the BusBuddy solution with validation"
        Usage = "bb-clean [-Deep] [-RemoveBin] [-RemoveObj]"
        Examples = @(
            "bb-clean",
            "bb-clean -Deep",
            "bb-clean -RemoveBin -RemoveObj"
        )
    },
    @{
        Name = "bb-test"
        Description = "Run BusBuddy unit and integration tests"
        Usage = "bb-test [-Filter <TestName>] [-Category <CategoryName>] [-NoBuild]"
        Examples = @(
            "bb-test",
            "bb-test -Filter 'Driver*'",
            "bb-test -Category 'Integration'"
        )
    },
    @{
        Name = "bb-dev-session"
        Description = "Start a complete BusBuddy development session"
        Usage = "bb-dev-session [-OpenIDE] [-InitDB] [-UpdatePackages]"
        Examples = @(
            "bb-dev-session",
            "bb-dev-session -OpenIDE",
            "bb-dev-session -InitDB -UpdatePackages"
        )
    },
    @{
        Name = "bb-diagnostic"
        Description = "Run comprehensive diagnostics on the BusBuddy solution"
        Usage = "bb-diagnostic [-Full] [-Export <FilePath>] [-CheckDependencies]"
        Examples = @(
            "bb-diagnostic",
            "bb-diagnostic -Full",
            "bb-diagnostic -Export ./diagnostic-report.json"
        )
    },
    @{
        Name = "bb-debug-start"
        Description = "Start debug monitoring for the BusBuddy application"
        Usage = "bb-debug-start [-Filter <FilterPattern>] [-LogToFile]"
        Examples = @(
            "bb-debug-start",
            "bb-debug-start -Filter 'Error*'",
            "bb-debug-start -LogToFile"
        )
    },
    @{
        Name = "bb-environment"
        Description = "Display or set BusBuddy environment variables"
        Usage = "bb-environment [-List] [-Set <Name> <Value>] [-Reset]"
        Examples = @(
            "bb-environment -List",
            "bb-environment -Set DatabaseProvider LocalDB",
            "bb-environment -Reset"
        )
    },
    @{
        Name = "bb-db-switch"
        Description = "Switch between database providers (LocalDB, Azure, SQLite)"
        Usage = "bb-db-switch -Provider <LocalDB|Azure|SQLite> [-Configure]"
        Examples = @(
            "bb-db-switch -Provider LocalDB",
            "bb-db-switch -Provider Azure -Configure",
            "bb-db-switch -Provider SQLite"
        )
    },
    @{
        Name = "bb-db-setup"
        Description = "Set up database for development, production, or testing"
        Usage = "bb-db-setup [-LocalDB] [-Azure] [-SQLite] [-CreateIfNotExists]"
        Examples = @(
            "bb-db-setup -LocalDB",
            "bb-db-setup -Azure -CreateIfNotExists",
            "bb-db-setup -SQLite"
        )
    },
    @{
        Name = "bb-code-cleanup"
        Description = "Clean up code and apply coding standards"
        Usage = "bb-code-cleanup [-Fix] [-Format] [-Style <Style>]"
        Examples = @(
            "bb-code-cleanup",
            "bb-code-cleanup -Fix",
            "bb-code-cleanup -Format -Style BusBuddy"
        )
    },
    @{
        Name = "bb-help"
        Description = "Show help information for BusBuddy commands"
        Usage = "bb-help [<CommandName>] [-List] [-Detailed] [-ShowExamples]"
        Examples = @(
            "bb-help",
            "bb-help bb-build",
            "bb-help -List -Detailed"
        )
    }
)

# Main execution logic
Write-BusBuddyHeader

if ($List) {
    Write-Host "Available BusBuddy Commands:" -ForegroundColor Green
    Write-Host ""

    foreach ($cmd in $commands) {
        Write-CommandHelp -Name $cmd.Name -Description $cmd.Description -Usage $cmd.Usage -Examples $cmd.Examples -ShowExamples $ShowExamples
    }

    Write-Host ""
    Write-Host "For detailed help on a specific command, run: bb-help <CommandName> -Detailed" -ForegroundColor Gray
    exit 0
}

if ($Command) {
    $matchedCommand = $commands | Where-Object { $_.Name -eq $Command }

    if ($null -eq $matchedCommand) {
        Write-Host "❌ Command '$Command' not found." -ForegroundColor Red
        Write-Host "Run 'bb-help -List' to see all available commands." -ForegroundColor Yellow
        exit 1
    }

    Write-Host "Help for command: " -NoNewline
    Write-Host "$Command" -ForegroundColor Green
    Write-Host ""
    Write-Host "Description: " -ForegroundColor Gray
    Write-Host "  $($matchedCommand.Description)"
    Write-Host ""
    Write-Host "Usage: " -ForegroundColor Gray
    Write-Host "  $($matchedCommand.Usage)"

    if ($ShowExamples -and $matchedCommand.Examples.Count -gt 0) {
        Write-Host ""
        Write-Host "Examples: " -ForegroundColor Gray
        foreach ($example in $matchedCommand.Examples) {
            Write-Host "  $example" -ForegroundColor DarkCyan
        }
    }

    exit 0
}

# Default behavior if no specific command requested
Write-Host "BusBuddy PowerShell Environment Help" -ForegroundColor Green
Write-Host ""
Write-Host "The BusBuddy development environment provides specialized PowerShell commands" -ForegroundColor Yellow
Write-Host "to simplify development, testing, and deployment tasks." -ForegroundColor Yellow
Write-Host ""
Write-Host "Basic Usage:" -ForegroundColor Gray
Write-Host "  1. Load the PowerShell environment:" -ForegroundColor White
Write-Host "     pwsh -ExecutionPolicy Bypass -File 'load-bus-buddy-profiles.ps1'" -ForegroundColor DarkCyan
Write-Host ""
Write-Host "  2. Use any bb-* command to perform tasks:" -ForegroundColor White
Write-Host "     bb-build          # Build the solution" -ForegroundColor DarkCyan
Write-Host "     bb-run            # Run the application" -ForegroundColor DarkCyan
Write-Host "     bb-health         # Check system health" -ForegroundColor DarkCyan
Write-Host ""
Write-Host "Available Commands:" -ForegroundColor Gray
Write-Host "  Run 'bb-help -List' to see all available commands" -ForegroundColor White
Write-Host "  Run 'bb-help <CommandName>' for help on a specific command" -ForegroundColor White
Write-Host ""
Write-Host "Documentation:" -ForegroundColor Gray
Write-Host "  See 'Documentation/PowerShell-7.5.2-Reference.md' for complete documentation" -ForegroundColor White
Write-Host ""
