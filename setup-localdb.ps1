# BusBuddy LocalDB Setup Script
# This script sets up SQL Server LocalDB for the BusBuddy application
# It creates the database if it doesn't exist and applies any pending migrations

$ErrorActionPreference = "Stop"

Write-Host "🚌 BusBuddy LocalDB Setup" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""

# Check if SQL LocalDB is installed
try {
    Write-Host "🔍 Checking SQL Server LocalDB installation..." -ForegroundColor Yellow
    & sqllocaldb info | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "SQL LocalDB command failed"
    }
    Write-Host "✅ SQL Server LocalDB is installed" -ForegroundColor Green
}
catch {
    Write-Host "❌ SQL Server LocalDB is not installed or not in the PATH" -ForegroundColor Red
    Write-Host "   Please install SQL Server LocalDB from https://go.microsoft.com/fwlink/?LinkID=866658" -ForegroundColor Yellow
    Write-Host "   After installation, restart your computer and run this script again." -ForegroundColor Yellow
    exit 1
}

# Check if MSSQLLocalDB instance exists, create it if it doesn't
Write-Host "🔍 Checking for MSSQLLocalDB instance..." -ForegroundColor Yellow
$instanceExists = & sqllocaldb info MSSQLLocalDB | Where-Object { $_ -match "MSSQLLocalDB" }
if (-not $instanceExists) {
    Write-Host "🔧 Creating MSSQLLocalDB instance..." -ForegroundColor Yellow
    & sqllocaldb create MSSQLLocalDB
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to create MSSQLLocalDB instance" -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ MSSQLLocalDB instance created" -ForegroundColor Green
}
else {
    Write-Host "✅ MSSQLLocalDB instance exists" -ForegroundColor Green
}

# Make sure the instance is running
Write-Host "🔍 Checking if MSSQLLocalDB instance is running..." -ForegroundColor Yellow
$instanceStatus = & sqllocaldb info MSSQLLocalDB | Where-Object { $_ -match "State:" }
if ($instanceStatus -match "Stopped") {
    Write-Host "🔧 Starting MSSQLLocalDB instance..." -ForegroundColor Yellow
    & sqllocaldb start MSSQLLocalDB
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to start MSSQLLocalDB instance" -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ MSSQLLocalDB instance started" -ForegroundColor Green
}
else {
    Write-Host "✅ MSSQLLocalDB instance is running" -ForegroundColor Green
}

# Check for dotnet tool entity framework
Write-Host "🔍 Checking for Entity Framework Core tools..." -ForegroundColor Yellow
$efExists = dotnet tool list --global | Where-Object { $_ -match "dotnet-ef" }
if (-not $efExists) {
    Write-Host "🔧 Installing Entity Framework Core tools..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to install Entity Framework Core tools" -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ Entity Framework Core tools installed" -ForegroundColor Green
}
else {
    Write-Host "✅ Entity Framework Core tools are installed" -ForegroundColor Green
}

# Set environment variable to indicate development environment
$env:ASPNETCORE_ENVIRONMENT = "Development"
Write-Host "🔧 Set environment to Development" -ForegroundColor Green

# Create the database and apply migrations
Write-Host "🔧 Creating BusBuddy database (if it doesn't exist)..." -ForegroundColor Yellow
Set-Location $PSScriptRoot

try {
    Write-Host "🔧 Applying Entity Framework migrations..." -ForegroundColor Yellow
    dotnet ef database update --project BusBuddy.Core --startup-project BusBuddy.WPF
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to apply migrations" -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ Migrations applied successfully" -ForegroundColor Green
}
catch {
    Write-Host "❌ Error applying migrations: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host "✅ BusBuddy LocalDB setup complete!" -ForegroundColor Green
Write-Host "   Connection string: Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=BusBuddy;Integrated Security=True" -ForegroundColor Yellow
Write-Host "   You can now run the application with 'bb-run' or 'dotnet run --project BusBuddy.WPF/BusBuddy.WPF.csproj'" -ForegroundColor Yellow
Write-Host "=======================================" -ForegroundColor Cyan
