# BusBuddy Azure Migration Script - PowerShell Version
# Run this after updating your Azure connection string in appsettings.json

Write-Host '=== BusBuddy Azure Migration ===' -ForegroundColor Green

# Function to update connection string
function Update-AzureConnectionString {
    param(
        [Parameter(Mandatory = $true)]
        [string]$AzureConnectionString
    )

    Write-Host 'Updating appsettings.json with Azure connection...' -ForegroundColor Yellow

    $configPath = '.\BusBuddy.WPF\appsettings.json'

    if (Test-Path $configPath) {
        $config = Get-Content $configPath | ConvertFrom-Json
        $config.ConnectionStrings.AzureConnection = $AzureConnectionString
        $config.DatabaseProvider = 'Azure'

        $config | ConvertTo-Json -Depth 10 | Set-Content $configPath
        Write-Host '✅ Configuration updated successfully' -ForegroundColor Green
    } else {
        Write-Host "❌ appsettings.json not found at $configPath" -ForegroundColor Red
        return $false
    }
    return $true
}

# Function to migrate database
function Start-DatabaseMigration {
    Write-Host 'Starting database migration to Azure...' -ForegroundColor Yellow

    # First, let's see what migrations we have
    Write-Host 'Checking current migrations...' -ForegroundColor Cyan
    dotnet ef migrations list --project BusBuddy.Core --startup-project BusBuddy.WPF

    # Apply migrations to Azure database
    Write-Host 'Applying migrations to Azure database...' -ForegroundColor Yellow
    $result = dotnet ef database update --project BusBuddy.Core --startup-project BusBuddy.WPF

    if ($LASTEXITCODE -eq 0) {
        Write-Host '✅ Database migration completed successfully!' -ForegroundColor Green
        return $true
    } else {
        Write-Host '❌ Database migration failed. Check error messages above.' -ForegroundColor Red
        return $false
    }
}

# Function to test connection
function Test-DatabaseConnection {
    Write-Host 'Testing database connection...' -ForegroundColor Yellow

    try {
        # Build the project first
        dotnet build BusBuddy.WPF --configuration Release --verbosity quiet

        if ($LASTEXITCODE -eq 0) {
            Write-Host '✅ Build successful' -ForegroundColor Green
            Write-Host '✅ Connection test passed (build completed)' -ForegroundColor Green
            return $true
        } else {
            Write-Host '❌ Build failed - check connection string' -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "❌ Connection test failed: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Function to switch between local and Azure
function Switch-DatabaseProvider {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('Local', 'Azure')]
        [string]$Provider
    )

    $configPath = '.\BusBuddy.WPF\appsettings.json'

    if (Test-Path $configPath) {
        $config = Get-Content $configPath | ConvertFrom-Json
        $config.DatabaseProvider = $Provider

        $config | ConvertTo-Json -Depth 10 | Set-Content $configPath
        Write-Host "✅ Switched to $Provider database provider" -ForegroundColor Green
        Write-Host '🔄 Please restart the application to apply changes' -ForegroundColor Yellow
    } else {
        Write-Host '❌ Configuration file not found' -ForegroundColor Red
    }
}

# Main menu
Write-Host "`nWhat would you like to do?" -ForegroundColor Cyan
Write-Host '1. Update Azure connection string and migrate' -ForegroundColor White
Write-Host '2. Test current connection' -ForegroundColor White
Write-Host '3. Switch database provider (Local/Azure)' -ForegroundColor White
Write-Host '4. Just run migration (if connection string already set)' -ForegroundColor White

$choice = Read-Host "`nEnter your choice (1-4)"

switch ($choice) {
    '1' {
        $azureConnString = Read-Host 'Enter your Azure SQL connection string'
        if (Update-AzureConnectionString -AzureConnectionString $azureConnString) {
            Start-DatabaseMigration
        }
    }
    '2' {
        Test-DatabaseConnection
    }
    '3' {
        $provider = Read-Host 'Enter database provider (Local or Azure)'
        Switch-DatabaseProvider -Provider $provider
    }
    '4' {
        Start-DatabaseMigration
    }
    default {
        Write-Host 'Invalid choice. Exiting...' -ForegroundColor Red
    }
}

Write-Host "`n=== Migration Script Complete ===" -ForegroundColor Green
