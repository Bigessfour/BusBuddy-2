#Requires -Version 7.5

<#
.SYNOPSIS
    BusBuddy Help System
.DESCRIPTION
    Provides help and documentation for BusBuddy PowerShell commands
.NOTES
    File Name      : Help.ps1
    Author         : BusBuddy Development Team
    Prerequisite   : PowerShell 7.5.2
    Copyright 2025 - BusBuddy
#>

function global:bb-help {
    param (
        [Parameter(Position = 0, Mandatory = $false)]
        [string]$Command,

        [Parameter(Mandatory = $false)]
        [switch]$List,

        [Parameter(Mandatory = $false)]
        [switch]$Detailed,

        [Parameter(Mandatory = $false)]
        [switch]$ShowExamples,

        [Parameter(Mandatory = $false)]
        [switch]$OpenDocumentation
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
            [bool]$ShowExamples = $false,
            [string]$Category = "General"
        )

        Write-Host "$Name" -ForegroundColor Yellow -NoNewline

        # Display category tag
        $categoryColor = switch ($Category) {
            "Build" { "Green" }
            "Database" { "Blue" }
            "Diagnostics" { "Magenta" }
            "Development" { "Cyan" }
            "Utilities" { "Gray" }
            default { "White" }
        }

        Write-Host " [$Category]" -ForegroundColor $categoryColor -NoNewline
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
            Category = "Build"
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
            Category = "Build"
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
            Category = "Diagnostics"
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
            Category = "Build"
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
            Category = "Development"
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
            Category = "Development"
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
            Category = "Diagnostics"
            Usage = "bb-diagnostic [-Full] [-Export <FilePath>] [-CheckDependencies]"
            Examples = @(
                "bb-diagnostic",
                "bb-diagnostic -Full",
                "bb-diagnostic -Export ./diagnostic-report.json"
            )
        },
        @{
            Name = "bb-environment"
            Description = "Display or set BusBuddy environment variables"
            Category = "Utilities"
            Usage = "bb-environment [-List] [-Set <n> <Value>] [-Reset]"
            Examples = @(
                "bb-environment -List",
                "bb-environment -Set DatabaseProvider LocalDB",
                "bb-environment -Reset"
            )
        },
        @{
            Name = "bb-db-switch"
            Description = "Switch between database providers (LocalDB, Azure, SQLite)"
            Category = "Database"
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
            Category = "Database"
            Usage = "bb-db-setup [-LocalDB] [-Azure] [-SQLite] [-CreateIfNotExists]"
            Examples = @(
                "bb-db-setup -LocalDB",
                "bb-db-setup -Azure -CreateIfNotExists",
                "bb-db-setup -SQLite"
            )
        },
        @{
            Name = "bb-help"
            Description = "Show help information for BusBuddy commands"
            Category = "Utilities"
            Usage = "bb-help [<CommandName>] [-List] [-Detailed] [-ShowExamples] [-OpenDocumentation]"
            Examples = @(
                "bb-help",
                "bb-help bb-build",
                "bb-help -List -Detailed",
                "bb-help -OpenDocumentation"
            )
        }
    )

    # Main execution logic
    Write-BusBuddyHeader

    if ($OpenDocumentation) {
        $scriptRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))))
        $docPath = Join-Path $scriptRoot "Documentation\PowerShell-7.5.2-Reference.md"

        if (Test-Path $docPath) {
            Write-Host "📄 Opening PowerShell documentation..." -ForegroundColor Cyan

            # Try to find the best way to open the documentation
            if (Get-Command code -ErrorAction SilentlyContinue) {
                Start-Process -FilePath code -ArgumentList $docPath
            } else {
                # Fall back to default application
                Invoke-Item $docPath
            }
        } else {
            Write-Host "❌ Documentation file not found at: $docPath" -ForegroundColor Red
        }

        return
    }

    if ($List) {
        if ($Detailed) {
            # Group commands by category
            $commandsByCategory = $commands | Group-Object -Property { $_.Category }

            foreach ($category in $commandsByCategory) {
                Write-Host "[$($category.Name)]" -ForegroundColor Cyan
                Write-Host "==================================================" -ForegroundColor Cyan

                foreach ($cmd in $category.Group) {
                    Write-CommandHelp -Name $cmd.Name -Description $cmd.Description -Usage $cmd.Usage -Examples $cmd.Examples -ShowExamples $ShowExamples -Category $cmd.Category
                }

                Write-Host ""
            }
        } else {
            Write-Host "Available BusBuddy Commands:" -ForegroundColor Green
            Write-Host ""

            foreach ($cmd in $commands) {
                Write-CommandHelp -Name $cmd.Name -Description $cmd.Description -Usage $cmd.Usage -Examples $cmd.Examples -ShowExamples $ShowExamples -Category $cmd.Category
            }
        }

        Write-Host ""
        Write-Host "For detailed help on a specific command, run: bb-help <CommandName> -Detailed" -ForegroundColor Gray
        Write-Host "To view full documentation, run: bb-help -OpenDocumentation" -ForegroundColor Gray
        return
    }

    if ($Command) {
        $matchedCommand = $commands | Where-Object { $_.Name -eq $Command }

        if ($null -eq $matchedCommand) {
            Write-Host "❌ Command '$Command' not found." -ForegroundColor Red
            Write-Host "Run 'bb-help -List' to see all available commands." -ForegroundColor Yellow
            return
        }

        Write-Host "Help for command: " -NoNewline
        Write-Host "$Command" -ForegroundColor Green -NoNewline
        Write-Host " [$($matchedCommand.Category)]" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "Description: " -ForegroundColor Gray
        Write-Host "  $($matchedCommand.Description)"
        Write-Host ""
        Write-Host "Usage: " -ForegroundColor Gray
        Write-Host "  $($matchedCommand.Usage)"

        if ($ShowExamples -or $Detailed) {
            if ($matchedCommand.Examples.Count -gt 0) {
                Write-Host ""
                Write-Host "Examples: " -ForegroundColor Gray
                foreach ($example in $matchedCommand.Examples) {
                    Write-Host "  $example" -ForegroundColor DarkCyan
                }
            }
        }

        return
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
    Write-Host "Command Categories:" -ForegroundColor Cyan
    Write-Host "  [Build] - Commands for building and running the application" -ForegroundColor Green
    Write-Host "  [Database] - Database management commands" -ForegroundColor Blue
    Write-Host "  [Diagnostics] - Health checks and diagnostics" -ForegroundColor Magenta
    Write-Host "  [Development] - Development workflow commands" -ForegroundColor Cyan
    Write-Host "  [Utilities] - Utility commands" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Available Commands:" -ForegroundColor Gray
    Write-Host "  Run 'bb-help -List' to see all available commands" -ForegroundColor White
    Write-Host "  Run 'bb-help <CommandName>' for help on a specific command" -ForegroundColor White
    Write-Host "  Run 'bb-help -List -Detailed' for detailed help on all commands" -ForegroundColor White
    Write-Host "  Run 'bb-help -OpenDocumentation' to open the full documentation" -ForegroundColor White
    Write-Host ""
}

# Export functions
Export-ModuleMember -Function bb-help
