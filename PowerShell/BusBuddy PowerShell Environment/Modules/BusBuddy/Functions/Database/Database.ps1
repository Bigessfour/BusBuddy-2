#Requires -Version 7.5

<#
.SYNOPSIS
    Database-related functions for BusBuddy
.DESCRIPTION
    Contains functions for managing BusBuddy database connections and providers
.NOTES
    File Name      : Database.ps1
    Author         : BusBuddy Development Team
    Prerequisite   : PowerShell 7.5.2
    Copyright 2025 - BusBuddy
#>

function global:Set-BusBuddyDatabaseProvider {
    param (
        [Parameter(Mandatory = $true)]
        [ValidateSet("LocalDB", "Azure", "SQLite")]
        [string]$Provider,

        [Parameter(Mandatory = $false)]
        [switch]$Configure
    )

    $scriptRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))))
    $switchScriptPath = Join-Path $scriptRoot "switch-database-provider.ps1"

    if (-not (Test-Path $switchScriptPath)) {
        Write-Error "❌ Database provider switch script not found at: $switchScriptPath"
        return
    }

    & $switchScriptPath -Provider $Provider

    if ($Configure) {
        switch ($Provider) {
            "LocalDB" {
                $setupScriptPath = Join-Path $scriptRoot "setup-localdb.ps1"
                if (Test-Path $setupScriptPath) {
                    & $setupScriptPath
                }
                else {
                    Write-Warning "LocalDB setup script not found at: $setupScriptPath"
                }
            }
            "Azure" {
                Write-Host "To configure Azure SQL, run the deploy-azure-sql.ps1 script with appropriate parameters" -ForegroundColor Yellow
            }
            "SQLite" {
                Write-Host "SQLite database will be created automatically when the application runs" -ForegroundColor Yellow
            }
        }
    }
}

function global:Initialize-BusBuddyDatabase {
    param (
        [Parameter(Mandatory = $false)]
        [switch]$LocalDB,

        [Parameter(Mandatory = $false)]
        [switch]$Azure,

        [Parameter(Mandatory = $false)]
        [switch]$SQLite,

        [Parameter(Mandatory = $false)]
        [switch]$CreateIfNotExists
    )

    $scriptRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path))))

    if ($LocalDB -or (-not $LocalDB -and -not $Azure -and -not $SQLite)) {
        $setupScriptPath = Join-Path $scriptRoot "setup-localdb.ps1"
        if (Test-Path $setupScriptPath) {
            Write-Host "🔧 Setting up LocalDB database..." -ForegroundColor Cyan
            & $setupScriptPath -CreateIfNotExists:$CreateIfNotExists
        }
        else {
            Write-Error "❌ LocalDB setup script not found at: $setupScriptPath"
        }
    }

    if ($Azure) {
        $deployScriptPath = Join-Path $scriptRoot "deploy-azure-sql.ps1"
        if (Test-Path $deployScriptPath) {
            Write-Host "🔧 Setting up Azure SQL database..." -ForegroundColor Cyan
            & $deployScriptPath -CreateIfNotExists:$CreateIfNotExists
        }
        else {
            Write-Error "❌ Azure SQL deployment script not found at: $deployScriptPath"
        }
    }

    if ($SQLite) {
        Write-Host "🔧 Setting up SQLite database..." -ForegroundColor Cyan
        Write-Host "SQLite database will be created automatically when the application runs" -ForegroundColor Yellow
        # Optionally, we could add code here to explicitly create the SQLite database
    }
}

# Export functions
Export-ModuleMember -Function Set-BusBuddyDatabaseProvider, Initialize-BusBuddyDatabase
