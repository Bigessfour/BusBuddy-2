# BusBuddy Azure SQL Deployment Script
# This script deploys the BusBuddy database schema to Azure SQL
# It requires Azure CLI to be installed and authenticated

param(
    [Parameter(Mandatory = $true)]
    [string]$ServerName,

    [Parameter(Mandatory = $true)]
    [string]$DatabaseName,

    [Parameter(Mandatory = $true)]
    [string]$AdminUsername,

    [Parameter(Mandatory = $false)]
    [string]$ResourceGroup,

    [Parameter(Mandatory = $false)]
    [switch]$CreateIfNotExists
)

$ErrorActionPreference = "Stop"

Write-Host "🚌 BusBuddy Azure SQL Deployment" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""

# Check if Azure CLI is installed
try {
    Write-Host "🔍 Checking Azure CLI installation..." -ForegroundColor Yellow
    & az version | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Azure CLI command failed"
    }
    Write-Host "✅ Azure CLI is installed" -ForegroundColor Green
}
catch {
    Write-Host "❌ Azure CLI is not installed or not in the PATH" -ForegroundColor Red
    Write-Host "   Please install Azure CLI from https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" -ForegroundColor Yellow
    Write-Host "   After installation, run 'az login' to authenticate and run this script again." -ForegroundColor Yellow
    exit 1
}

# Check if logged in to Azure
Write-Host "🔍 Checking Azure CLI authentication..." -ForegroundColor Yellow
$loginStatus = & az account show 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Not logged in to Azure CLI" -ForegroundColor Red
    Write-Host "   Please run 'az login' to authenticate and run this script again." -ForegroundColor Yellow
    exit 1
}
Write-Host "✅ Authenticated with Azure CLI" -ForegroundColor Green

# Set environment variable to indicate production environment
$env:ASPNETCORE_ENVIRONMENT = "Production"
Write-Host "🔧 Set environment to Production" -ForegroundColor Green

# Create resource group if specified and doesn't exist
if ($CreateIfNotExists -and $ResourceGroup) {
    Write-Host "🔍 Checking if resource group $ResourceGroup exists..." -ForegroundColor Yellow
    $rgExists = & az group exists --name $ResourceGroup
    if ($rgExists -eq "false") {
        Write-Host "🔧 Creating resource group $ResourceGroup..." -ForegroundColor Yellow
        & az group create --name $ResourceGroup --location eastus
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Failed to create resource group" -ForegroundColor Red
            exit 1
        }
        Write-Host "✅ Resource group $ResourceGroup created" -ForegroundColor Green
    }
    else {
        Write-Host "✅ Resource group $ResourceGroup exists" -ForegroundColor Green
    }
}

# Create Azure SQL server if it doesn't exist
if ($CreateIfNotExists) {
    Write-Host "🔍 Checking if Azure SQL server $ServerName exists..." -ForegroundColor Yellow
    $serverExists = & az sql server list --query "[?name=='$ServerName'].name" -o tsv
    if (-not $serverExists) {
        # Ask for password securely
        $adminPassword = Read-Host "Enter SQL Server admin password" -AsSecureString
        $bstr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($adminPassword)
        $plainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($bstr)

        Write-Host "🔧 Creating Azure SQL server $ServerName..." -ForegroundColor Yellow
        & az sql server create --name $ServerName --resource-group $ResourceGroup --location eastus --admin-user $AdminUsername --admin-password $plainPassword
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Failed to create Azure SQL server" -ForegroundColor Red
            exit 1
        }
        Write-Host "✅ Azure SQL server $ServerName created" -ForegroundColor Green

        # Configure firewall to allow Azure services
        Write-Host "🔧 Configuring firewall to allow Azure services..." -ForegroundColor Yellow
        & az sql server firewall-rule create --resource-group $ResourceGroup --server $ServerName --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Failed to create firewall rule" -ForegroundColor Red
            exit 1
        }
        Write-Host "✅ Firewall configured to allow Azure services" -ForegroundColor Green

        # Configure firewall to allow current IP
        Write-Host "🔧 Configuring firewall to allow current IP..." -ForegroundColor Yellow
        $currentIp = (Invoke-WebRequest -Uri "https://api.ipify.org").Content
        & az sql server firewall-rule create --resource-group $ResourceGroup --server $ServerName --name AllowCurrentIP --start-ip-address $currentIp --end-ip-address $currentIp
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Failed to create firewall rule for current IP" -ForegroundColor Red
            exit 1
        }
        Write-Host "✅ Firewall configured to allow current IP ($currentIp)" -ForegroundColor Green
    }
    else {
        Write-Host "✅ Azure SQL server $ServerName exists" -ForegroundColor Green
    }

    # Create database if it doesn't exist
    Write-Host "🔍 Checking if database $DatabaseName exists on server $ServerName..." -ForegroundColor Yellow
    $dbExists = & az sql db list --resource-group $ResourceGroup --server $ServerName --query "[?name=='$DatabaseName'].name" -o tsv
    if (-not $dbExists) {
        Write-Host "🔧 Creating database $DatabaseName..." -ForegroundColor Yellow
        & az sql db create --resource-group $ResourceGroup --server $ServerName --name $DatabaseName --service-objective Basic
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Failed to create database" -ForegroundColor Red
            exit 1
        }
        Write-Host "✅ Database $DatabaseName created" -ForegroundColor Green
    }
    else {
        Write-Host "✅ Database $DatabaseName exists" -ForegroundColor Green
    }
}

# Update connection string in appsettings.Production.json
Write-Host "🔧 Updating connection string in appsettings.Production.json..." -ForegroundColor Yellow
$appSettingsPath = Join-Path $PSScriptRoot "BusBuddy.Core\appsettings.Production.json"
$appSettings = Get-Content $appSettingsPath -Raw | ConvertFrom-Json

# Prompt for password securely
$adminPassword = Read-Host "Enter SQL Server admin password for connection string" -AsSecureString
$bstr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($adminPassword)
$plainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($bstr)

# Update connection string
$connectionString = "Server=tcp:$ServerName.database.windows.net,1433;Initial Catalog=$DatabaseName;Persist Security Info=False;User ID=$AdminUsername;Password=$plainPassword;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
$appSettings.ConnectionStrings.DefaultConnection = $connectionString

# Save the updated settings
$appSettings | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath
Write-Host "✅ Connection string updated in appsettings.Production.json" -ForegroundColor Green

# Update environment variables for deployment
$env:AZURE_SQL_USER = $AdminUsername
$env:AZURE_SQL_PASSWORD = $plainPassword
$env:DatabaseProvider = "Azure"

# Apply migrations to Azure SQL
Write-Host "🔧 Applying Entity Framework migrations to Azure SQL..." -ForegroundColor Yellow
Set-Location $PSScriptRoot

try {
    dotnet ef database update --project BusBuddy.Core --startup-project BusBuddy.WPF --configuration Release
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to apply migrations to Azure SQL" -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ Migrations applied successfully to Azure SQL" -ForegroundColor Green
}
catch {
    Write-Host "❌ Error applying migrations to Azure SQL: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host "✅ BusBuddy Azure SQL deployment complete!" -ForegroundColor Green
Write-Host "   Server: $ServerName.database.windows.net" -ForegroundColor Yellow
Write-Host "   Database: $DatabaseName" -ForegroundColor Yellow
Write-Host "   Username: $AdminUsername" -ForegroundColor Yellow
Write-Host "=======================================" -ForegroundColor Cyan
