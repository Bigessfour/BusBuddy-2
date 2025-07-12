# Azure SQL Database Setup Script for BusBuddy
# This script creates a FREE Azure SQL Database using serverless tier

param(
    [Parameter(Mandatory = $true)]
    [string]$YourInitials,

    [Parameter(Mandatory = $true)]
    [SecureString]$AdminPassword,

    [string]$ResourceGroup = 'BusBuddy-RG',
    [string]$Location = 'eastus',
    [string]$ServerName = "busbuddy-server-$YourInitials",
    [string]$DatabaseName = 'BusBuddyDB',
    [string]$AdminUser = 'busbuddy_admin'
)

Write-Host '🚀 Setting up Azure SQL Database for BusBuddy...' -ForegroundColor Green

# Convert SecureString to plain text for Azure CLI
$PlainPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($AdminPassword))

try {
    # Check if logged in to Azure
    Write-Host '📋 Checking Azure login status...' -ForegroundColor Yellow
    $account = az account show --query 'user.name' -o tsv 2>$null
    if (!$account) {
        Write-Host '🔐 Please log in to Azure...' -ForegroundColor Yellow
        az login
    } else {
        Write-Host "✅ Logged in as: $account" -ForegroundColor Green
    }

    # 1. Create resource group
    Write-Host "📁 Creating resource group: $ResourceGroup..." -ForegroundColor Yellow
    az group create --name $ResourceGroup --location $Location --output table

    # 2. Create SQL Server
    Write-Host "🖥️ Creating SQL Server: $ServerName..." -ForegroundColor Yellow
    az sql server create --name $ServerName --resource-group $ResourceGroup --location $Location --admin-user $AdminUser --admin-password $PlainPassword --output table

    # 3. Create database with FREE serverless tier
    Write-Host "💾 Creating FREE serverless database: $DatabaseName..." -ForegroundColor Yellow
    az sql db create --name $DatabaseName --resource-group $ResourceGroup --server $ServerName --edition GeneralPurpose --family Gen5 --capacity 2 --compute-model Serverless --use-free-limit --free-limit-exhaustion-behavior AutoPause --output table

    # 4. Get current IP and add firewall rule
    Write-Host '🔥 Setting up firewall rule for your IP...' -ForegroundColor Yellow
    $MyIP = (Invoke-WebRequest -Uri 'https://ipinfo.io/ip' -UseBasicParsing).Content.Trim()
    Write-Host "📍 Your IP: $MyIP" -ForegroundColor Cyan
    az sql server firewall-rule create --resource-group $ResourceGroup --server $ServerName --name 'MyIP' --start-ip-address $MyIP --end-ip-address $MyIP --output table

    # 5. Get connection string
    Write-Host '🔗 Getting connection string...' -ForegroundColor Yellow
    $ConnectionString = az sql db show-connection-string --name $DatabaseName --server $ServerName --client ado.net --auth-type SqlPassword --output tsv

    # Replace placeholders in connection string
    $ConnectionString = $ConnectionString -replace '<username>', $AdminUser
    $ConnectionString = $ConnectionString -replace '<password>', $PlainPassword

    Write-Host "`n✅ Azure SQL Database setup complete!" -ForegroundColor Green
    Write-Host "`n📋 Connection Details:" -ForegroundColor Cyan
    Write-Host "Server: $ServerName.database.windows.net" -ForegroundColor White
    Write-Host "Database: $DatabaseName" -ForegroundColor White
    Write-Host "Username: $AdminUser" -ForegroundColor White
    Write-Host "`n🔗 Connection String:" -ForegroundColor Cyan
    Write-Host $ConnectionString -ForegroundColor White

    # Save connection string to file
    $ConnectionString | Out-File -FilePath 'azure-connection-string.txt' -Encoding UTF8
    Write-Host "`n💾 Connection string saved to: azure-connection-string.txt" -ForegroundColor Green

    Write-Host "`n🎯 Next Steps:" -ForegroundColor Yellow
    Write-Host '1. Copy the connection string above' -ForegroundColor White
    Write-Host '2. Update your appsettings.json file' -ForegroundColor White
    Write-Host '3. Run database migration to Azure' -ForegroundColor White
    Write-Host '4. Test the connection' -ForegroundColor White

} catch {
    Write-Host "❌ Error during setup: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
} finally {
    # Clear password from memory
    $PlainPassword = $null
}
