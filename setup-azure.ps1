# Quick Setup Script for BusBuddy Azure Migration

param(
    [string]$Mode = "setup",  # setup, migrate, or switch
    [string]$AzureServer = "",
    [string]$AzureDatabase = "BusBuddyDB",
    [string]$AzureUsername = "busbuddy_admin",
    [string]$AzurePassword = ""
)

Write-Host "=== BusBuddy Azure Setup Helper ===" -ForegroundColor Green

switch ($Mode.ToLower()) {
    "setup" {
        Write-Host "`n🔧 Setting up Azure configuration..." -ForegroundColor Yellow

        # Check if Azure CLI is installed
        $azureCliCheck = Get-Command "az" -ErrorAction SilentlyContinue
        if (-not $azureCliCheck) {
            Write-Host "❌ Azure CLI not found. Please install from: https://aka.ms/installazurecli" -ForegroundColor Red
            exit 1
        }

        Write-Host "✅ Azure CLI found" -ForegroundColor Green

        # Login to Azure
        Write-Host "`nLogging into Azure..." -ForegroundColor Yellow
        az login

        # List available subscriptions
        Write-Host "`nAvailable subscriptions:" -ForegroundColor Cyan
        az account list --output table

        Write-Host "`n📋 Next steps:" -ForegroundColor Cyan
        Write-Host "1. Follow the Azure setup guide in AzureSetupGuide.md" -ForegroundColor White
        Write-Host "2. Create your SQL Database in Azure Portal" -ForegroundColor White
        Write-Host "3. Run this script with 'migrate' mode when ready" -ForegroundColor White
    }

    "migrate" {
        if ([string]::IsNullOrEmpty($AzureServer) -or [string]::IsNullOrEmpty($AzurePassword)) {
            Write-Host "❌ Azure server and password required for migration" -ForegroundColor Red
            Write-Host "Usage: .\setup-azure.ps1 -Mode migrate -AzureServer 'yourserver.database.windows.net' -AzurePassword 'yourpassword'" -ForegroundColor Yellow
            exit 1
        }

        Write-Host "`n🔄 Migrating database to Azure..." -ForegroundColor Yellow

        # Update appsettings.json with Azure connection
        $azureConnectionString = "Server=tcp:$AzureServer,1433;Initial Catalog=$AzureDatabase;Persist Security Info=False;User ID=$AzureUsername;Password=$AzurePassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

        Write-Host "📝 Updating appsettings.json..." -ForegroundColor Yellow

        # Create appsettings.azure.json if it doesn't exist
        if (-not (Test-Path "appsettings.azure.json")) {
            Copy-Item "appsettings.json" "appsettings.azure.json"
        }

        # Update the connection string in the config
        $config = Get-Content "appsettings.azure.json" | ConvertFrom-Json
        $config.ConnectionStrings.DefaultConnection = $azureConnectionString
        $config.DatabaseProvider = "Azure"
        $config | ConvertTo-Json -Depth 10 | Set-Content "appsettings.azure.json"

        Write-Host "✅ Configuration updated" -ForegroundColor Green

        # Run EF migrations
        Write-Host "`n🏗️ Running database migrations..." -ForegroundColor Yellow
        dotnet ef database update --project BusBuddy.Core --startup-project BusBuddy.WPF

        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Migration successful!" -ForegroundColor Green
            Write-Host "`n🎉 Your database is now running on Azure!" -ForegroundColor Green
        } else {
            Write-Host "❌ Migration failed. Check the error messages above." -ForegroundColor Red
        }
    }

    "switch" {
        Write-Host "`n🔄 Switching database configuration..." -ForegroundColor Yellow

        $config = Get-Content "appsettings.json" | ConvertFrom-Json
        $currentProvider = $config.DatabaseProvider

        if ($currentProvider -eq "Azure") {
            $config.DatabaseProvider = "Local"
            Write-Host "✅ Switched to Local database" -ForegroundColor Green
        } else {
            $config.DatabaseProvider = "Azure"
            Write-Host "✅ Switched to Azure database" -ForegroundColor Green
        }

        $config | ConvertTo-Json -Depth 10 | Set-Content "appsettings.json"
        Write-Host "🔄 Restart the application to apply changes" -ForegroundColor Yellow
    }

    default {
        Write-Host "❌ Invalid mode. Use: setup, migrate, or switch" -ForegroundColor Red
        Write-Host "Examples:" -ForegroundColor Yellow
        Write-Host "  .\setup-azure.ps1 -Mode setup" -ForegroundColor White
        Write-Host "  .\setup-azure.ps1 -Mode migrate -AzureServer 'yourserver.database.windows.net' -AzurePassword 'yourpassword'" -ForegroundColor White
        Write-Host "  .\setup-azure.ps1 -Mode switch" -ForegroundColor White
    }
}

Write-Host "`n=== Setup Complete ===" -ForegroundColor Green
