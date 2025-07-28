#Requires -Version 7.5
<#
.SYNOPSIS
    Registers Tavily integration commands for BusBuddy PowerShell environment.

.DESCRIPTION
    This script registers commands with the 'bb-' prefix for Tavily integration
    in the BusBuddy PowerShell environment, making them available in the BusBuddy
    development workflow.

.NOTES
    File Name      : register-tavily-commands.ps1
    Author         : BusBuddy Development Team
    Prerequisite   : PowerShell 7.5.2+, BusBuddy PowerShell Environment

.EXAMPLE
    . .\register-tavily-commands.ps1

    Sources the script to register Tavily commands in the current PowerShell session.
#>

# Ensure we can find tavily-tool.ps1
$scriptPath = $PSScriptRoot
$tavilyToolPath = Join-Path $scriptPath "tavily-tool.ps1"

if (-not (Test-Path $tavilyToolPath)) {
    Write-Warning "Could not find tavily-tool.ps1 at: $tavilyToolPath"

    # Try one level up
    $tavilyToolPath = Join-Path (Split-Path $scriptPath -Parent) "Scripts\tavily-tool.ps1"
    if (-not (Test-Path $tavilyToolPath)) {
        Write-Error "tavily-tool.ps1 not found. Cannot register Tavily commands."
        return
    }
}

# Function to register commands
function Register-TavilyCommands {
    [CmdletBinding()]
    param()

    Write-Host "🔍 Registering BusBuddy Tavily commands..." -ForegroundColor Cyan

    # Search-BBTavily command (general search)
    function global:Search-BBTavily {
        param(
            [Parameter(Position = 0, Mandatory = $true)]
            [string]$Query,

            [Parameter(Mandatory = $false)]
            [int]$MaxResults = 5,

            [Parameter(Mandatory = $false)]
            [switch]$Detail
        )

        & $tavilyToolPath -Search $Query -MaxResults $MaxResults -IncludeMetadata:$Detail
    }

    # Search-BBTavilyCode command (code-specific search)
    function global:Search-BBTavilyCode {
        param(
            [Parameter(Position = 0, Mandatory = $true)]
            [string]$Query,

            [Parameter(Mandatory = $false)]
            [int]$MaxResults = 5,

            [Parameter(Mandatory = $false)]
            [switch]$Detail
        )

        & $tavilyToolPath -CodeSearch $Query -MaxResults $MaxResults -IncludeMetadata:$Detail
    }

    # Search-BBTavilyDocs command (documentation search)
    function global:Search-BBTavilyDocs {
        param(
            [Parameter(Position = 0, Mandatory = $true)]
            [string]$Query,

            [Parameter(Mandatory = $false)]
            [int]$MaxResults = 5,

            [Parameter(Mandatory = $false)]
            [switch]$Detail
        )

        & $tavilyToolPath -DocSearch $Query -MaxResults $MaxResults -IncludeMetadata:$Detail
    }

    # Initialize-BBTavily command (initialize environment)
    function global:Initialize-BBTavily {
        param(
            [Parameter(Mandatory = $false)]
            [string]$ApiKey
        )

        if ([string]::IsNullOrEmpty($ApiKey)) {
            & $tavilyToolPath -Initialize
        }
        else {
            & $tavilyToolPath -ApiKey $ApiKey -Initialize
        }
    }
    
    # Test-BBTavily command (validate setup)
    function global:Test-BBTavily {
        param(
            [Parameter(Mandatory = $false)]
            [string]$ApiKey
        )

        if ([string]::IsNullOrEmpty($ApiKey)) {
            & $tavilyToolPath -ValidateSetup
        }
        else {
            & $tavilyToolPath -ApiKey $ApiKey -ValidateSetup
        }
    }

    # Search-BBSyncfusion command (Syncfusion-specific search)
    function global:Search-BBSyncfusion {
        param(
            [Parameter(Position = 0, Mandatory = $true)]
            [string]$Query,

            [Parameter(Mandatory = $false)]
            [int]$MaxResults = 5
        )

        $syncfusionQuery = "Syncfusion WinForms $Query"
        & $tavilyToolPath -CodeSearch $syncfusionQuery -MaxResults $MaxResults
    }

    # Command aliases for convenience
    Set-Alias -Name bb-tavily -Value Search-BBTavily -Scope Global
    Set-Alias -Name bb-tavily-code -Value Search-BBTavilyCode -Scope Global
    Set-Alias -Name bb-tavily-docs -Value Search-BBTavilyDocs -Scope Global
    Set-Alias -Name bb-syncfusion -Value Search-BBSyncfusion -Scope Global
    Set-Alias -Name bb-tavily-setup -Value Initialize-BBTavily -Scope Global
    Set-Alias -Name bb-tavily-test -Value Test-BBTavily -Scope Global

    # Shorter aliases for quick access
    Set-Alias -Name bbt -Value Search-BBTavily -Scope Global
    Set-Alias -Name bbtc -Value Search-BBTavilyCode -Scope Global
    Set-Alias -Name bbtd -Value Search-BBTavilyDocs -Scope Global
    Set-Alias -Name bbsf -Value Search-BBSyncfusion -Scope Global

    # Create command summary
    $commandHelp = @"
🔍 BusBuddy Tavily Commands:

  bb-tavily <query>            - General search (alias: bbt)
  bb-tavily-code <query>       - Code-specific search (alias: bbtc)
  bb-tavily-docs <query>       - Documentation search (alias: bbtd)
  bb-syncfusion <query>        - Syncfusion-specific search (alias: bbsf)
  bb-tavily-setup [apiKey]     - Initialize Tavily environment
  bb-tavily-test               - Validate Tavily API setup

Examples:
  bb-tavily "bus route optimization"
  bb-tavily-code "EF Core migrations"
  bb-syncfusion "DockingManager"
  bb-tavily-test
"@

    Write-Host "`n$commandHelp`n" -ForegroundColor Yellow
    Write-Host "✅ BusBuddy Tavily commands registered successfully" -ForegroundColor Green

    # Initialize environment if first time
    if ([string]::IsNullOrEmpty($env:TAVILY_API_KEY)) {
        Write-Host "No Tavily API key found in environment." -ForegroundColor Yellow
        Write-Host "Run bb-tavily-setup to configure or bb-tavily-test to validate setup." -ForegroundColor Yellow
    }

    # Export function for PowerShell module integration
    Export-ModuleMember -Function Search-BBTavily, Search-BBTavilyCode, Search-BBTavilyDocs, Search-BBSyncfusion, Initialize-BBTavily, Test-BBTavily -Alias bb-tavily, bb-tavily-code, bb-tavily-docs, bb-syncfusion, bb-tavily-setup, bb-tavily-test, bbt, bbtc, bbtd, bbsf
}

# Execute registration function
Register-TavilyCommands

# Add to BusBuddy profile path if run directly
if ($MyInvocation.Line -notmatch '^\. ') {
    Write-Host "To add these commands to your BusBuddy PowerShell profile, add this line:" -ForegroundColor Cyan
    Write-Host ". `"$PSCommandPath`"" -ForegroundColor Yellow
}
