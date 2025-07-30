# BusBuddy Database Provider Switch Script
# This script switches between LocalDB and Azure SQL providers

param (
    [Parameter(Mandatory=$true)]
    [ValidateSet("LocalDB", "Azure", "SQLite")]
    [string]$Provider
)

$ErrorActionPreference = "Stop"

Write-Host "🚌 BusBuddy Database Provider Switch" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "🔧 Switching to $Provider database provider..." -ForegroundColor Yellow

# Get the paths to the appsettings.json files
$corePath = Join-Path $PSScriptRoot "BusBuddy.Core\appsettings.json"
$wpfPath = Join-Path $PSScriptRoot "BusBuddy.WPF\appsettings.json"

# Function to update appsettings.json files
function Update-AppSettings {
    param (
        [string]$Path,
        [string]$Provider
    )

    # Read the file content
    $settings = Get-Content $Path -Raw | ConvertFrom-Json

    # Update the provider
    $settings.DatabaseProvider = $Provider

    # Write the updated content back to the file
    $settings | ConvertTo-Json -Depth 10 | Set-Content $Path

    Write-Host "✅ Updated $Path" -ForegroundColor Green
}

# Update both appsettings.json files
Update-AppSettings -Path $corePath -Provider $Provider
Update-AppSettings -Path $wpfPath -Provider $Provider

# Set environment variable
$env:DatabaseProvider = $Provider

# Provide additional instructions based on provider
switch ($Provider) {
    "LocalDB" {
        Write-Host ""
        Write-Host "📋 LocalDB Instructions:" -ForegroundColor Yellow
        Write-Host "   Run './setup-localdb.ps1' to ensure the LocalDB database is set up" -ForegroundColor Yellow
        Write-Host "   Connection string: Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=BusBuddy;Integrated Security=True" -ForegroundColor Yellow
    }
    "Azure" {
        Write-Host ""
        Write-Host "📋 Azure SQL Instructions:" -ForegroundColor Yellow
        Write-Host "   Ensure you have set the AZURE_SQL_USER and AZURE_SQL_PASSWORD environment variables" -ForegroundColor Yellow
        Write-Host "   Run './deploy-azure-sql.ps1' to deploy to Azure SQL if needed" -ForegroundColor Yellow
        Write-Host "   Connection string (from appsettings.Production.json): Server=tcp:busbuddy-sql.database.windows.net,1433;..." -ForegroundColor Yellow
    }
    "SQLite" {
        Write-Host ""
        Write-Host "📋 SQLite Instructions:" -ForegroundColor Yellow
        Write-Host "   Using legacy SQLite mode (Phase 1 compatibility)" -ForegroundColor Yellow
        Write-Host "   Connection string: Data Source=BusBuddy.db" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host "✅ BusBuddy now using $Provider database provider!" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Cyan
