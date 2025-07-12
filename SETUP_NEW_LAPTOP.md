# BusBuddy - New Laptop Setup Guide

## Prerequisites Installation

### 1. Install Required Software
```powershell
# Install via Windows Package Manager (winget) or download manually:

# Visual Studio 2022 Community/Professional
winget install Microsoft.VisualStudio.2022.Community

# .NET 8 SDK
winget install Microsoft.DotNet.SDK.8

# Git for Windows
winget install Git.Git

# SQL Server Express (if needed)
winget install Microsoft.SQLServer.2022.Express

# Optional: SQL Server Management Studio
winget install Microsoft.SQLServerManagementStudio
```

### 2. Install Syncfusion License
- Download Syncfusion Essential Studio for WPF (version 30.1.37)
- Install to: `C:\Program Files (x86)\Syncfusion\Essential Studio\Windows\30.1.37`
- **Important**: Use the same license key in appsettings.json

### 3. Clone Repository
```bash
git clone https://github.com/Bigessfour/BusBuddy_Syncfusion.git
cd BusBuddy_Syncfusion
```

### 4. Environment Configuration

#### A. Copy appsettings.json
Copy your current `appsettings.json` to the new laptop:
- Contains Syncfusion license key
- Database connection strings
- XAI API configuration

#### B. Database Setup Options

**Option 1: Use Shared Database (Recommended)**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SHARED_SQL_SERVER;Database=BusBuddyDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Option 2: Local Database**
```bash
# Restore database from backup
dotnet ef database update
```

## Development Environment Sync

### Visual Studio Settings Sync
1. Sign in to Visual Studio with your Microsoft account
2. Enable Settings Sync: Tools → Options → Environment → Accounts → Settings Sync
3. Sync settings include:
   - Editor preferences
   - Keyboard shortcuts
   - Extensions
   - Color themes

### Git Configuration
```bash
git config --global user.name "Your Name"
git config --global user.email "your.email@example.com"
```

### Environment Variables (if used)
Create `.env` file or set system environment variables:
```
SYNCFUSION_LICENSE_KEY=your_license_key_here
XAI_API_KEY=your_xai_key_here
```

## Troubleshooting

### Common Issues
1. **Syncfusion License**: Ensure same version and valid license
2. **Database Connection**: Update connection string for new environment
3. **NuGet Packages**: Run `dotnet restore` if packages fail to load
4. **SQL Server**: Ensure SQL Server Express is running

### Quick Verification
```bash
# Check .NET version
dotnet --version

# Check SQL Server connection
sqlcmd -S "localhost\SQLEXPRESS" -E -Q "SELECT @@VERSION"

# Build project
dotnet build BusBuddy.sln
```

## **Database Sharing Solutions for Multi-Laptop Development**

### **Current Issue**
- SQL Server database is local to this laptop
- Cannot access same data from different laptops
- Need centralized database solution

### **Solution Options**

#### **Option 1: NAS with SQL Server (Recommended)**
```powershell
# If you get a NAS, you can:
# 1. Install SQL Server on NAS (Docker container)
# 2. Update connection string to point to NAS IP
# Example connection string:
"Server=192.168.1.100,1433;Database=BusBuddyDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
```

#### **Option 2: Azure SQL Database (Cloud)**
```powershell
# Quick cloud solution:
# 1. Create Azure SQL Database
# 2. Export current database to Azure
# 3. Update connection string
"Server=tcp:busbuddy-server.database.windows.net,1433;Database=BusBuddyDB;User Id=yourusername;Password=yourpassword;Encrypt=True;TrustServerCertificate=False;"
```

#### **Option 3: Portable Database (Development)**
```powershell
# Use SQL Server LocalDB with shared file
# 1. Export database to .bacpac file
# 2. Store in OneDrive/Dropbox
# 3. Import on each laptop
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "BACKUP DATABASE BusBuddyDB TO DISK='C:\Backup\BusBuddy.bak'"
```

#### **Option 4: Database Synchronization Script**
```powershell
# Create sync script for development
# 1. Export data regularly
# 2. Sync via cloud storage
# 3. Import on other laptop
```

### **Quick Migration to Azure SQL (If Interested)**
```powershell
# 1. Install Azure CLI
winget install Microsoft.AzureCLI

# 2. Login to Azure
az login

# 3. Create resource group
az group create --name BusBuddyRG --location "East US"

# 4. Create SQL Server
az sql server create --name busbuddy-server --resource-group BusBuddyRG --location "East US" --admin-user yourusername --admin-password YourPassword123!

# 5. Create database
az sql db create --resource-group BusBuddyRG --server busbuddy-server --name BusBuddyDB --service-objective Basic

# 6. Configure firewall
az sql server firewall-rule create --resource-group BusBuddyRG --server busbuddy-server --name AllowAll --start-ip-address 0.0.0.0 --end-ip-address 255.255.255.255
```

### **For Now - Export/Import Workflow**
Until you get a NAS, here's a temporary solution:

```powershell
# Export current database
sqlcmd -S "localhost\SQLEXPRESS" -Q "BACKUP DATABASE BusBuddyDB TO DISK='C:\Temp\BusBuddy_$(Get-Date -Format 'yyyyMMdd_HHmm').bak'"

# Store backup in cloud (OneDrive, Dropbox, etc.)
# On other laptop, restore:
sqlcmd -S "localhost\SQLEXPRESS" -Q "RESTORE DATABASE BusBuddyDB FROM DISK='C:\Temp\BusBuddy_20250712_1400.bak'"
```
