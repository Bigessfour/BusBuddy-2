# Azure SQL Database Setup for BusBuddy

## Free Tier Details
- **Storage**: 32 GB (more than enough for BusBuddy)
- **Compute**: 5 DTUs (sufficient for development)
- **Cost**: FREE for 12 months, then ~$5/month
- **Backup**: Automatic backups included

## Setup Steps

### 1. Create Azure Account
1. Go to [azure.microsoft.com](https://azure.microsoft.com/free/)
2. Sign up with Microsoft account
3. Get $200 free credits + 12 months free services

### 2. Create SQL Database
```bash
# Using Azure CLI (recommended)
az sql server create \
  --name busbuddy-server \
  --resource-group busbuddy-rg \
  --location eastus \
  --admin-user busbuddyadmin \
  --admin-password YourSecurePassword123!

az sql db create \
  --resource-group busbuddy-rg \
  --server busbuddy-server \
  --name BusBuddyDB \
  --service-objective Basic
```

### 3. Configure Firewall
```bash
# Allow your IP
az sql server firewall-rule create \
  --resource-group busbuddy-rg \
  --server busbuddy-server \
  --name AllowYourIP \
  --start-ip-address YOUR_IP \
  --end-ip-address YOUR_IP
```

### 4. Update Connection String
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:busbuddy-server.database.windows.net,1433;Initial Catalog=BusBuddyDB;Persist Security Info=False;User ID=busbuddyadmin;Password=YourSecurePassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

### 5. Migrate Database
```bash
# From your project directory
dotnet ef database update
```

## Benefits for BusBuddy
- ✅ Work from any laptop
- ✅ Automatic backups
- ✅ High availability
- ✅ Same SQL Server features
- ✅ Easy scaling if needed
