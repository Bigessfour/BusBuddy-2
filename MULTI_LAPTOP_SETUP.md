# BusBuddy Multi-Laptop Development Setup

## 🎯 Quick Start Guide

### Option 1: Azure SQL Database (Recommended)
**Best for**: Multi-laptop development, automatic sync, free tier available

1. **Setup Azure Account**
   ```bash
   # Open Azure Portal
   https://portal.azure.com
   ```

2. **Follow Setup Guide**
   - Open `AzureSetupGuide.md` for detailed steps
   - Use the free Basic tier (2 GB, perfect for BusBuddy)

3. **Run Migration Script**
   ```powershell
   .\setup-azure.ps1 -Mode setup
   .\setup-azure.ps1 -Mode migrate -AzureServer "your-server.database.windows.net" -AzurePassword "yourpassword"
   ```

### Option 2: Manual Sync (Temporary)
**Best for**: Quick setup while planning Azure migration

1. **Export Database**
   ```powershell
   .\migrate-to-azure.ps1
   ```

2. **Sync via Git**
   - Commit database backup files
   - Pull on other laptop
   - Restore database

## 🔧 Configuration Files

### Local Development
- `appsettings.json` - Local SQL Server Express
- Set `"DatabaseProvider": "Local"`

### Cloud Development  
- `appsettings.azure.json` - Azure SQL Database
- Set `"DatabaseProvider": "Azure"`

### Switch Between Environments
```powershell
.\setup-azure.ps1 -Mode switch
```

## 📁 What to Sync

### ✅ Always Sync (via Git)
- Source code (`*.cs`, `*.xaml`)
- Configuration templates (`appsettings.*.json`)
- Documentation (`*.md`)
- Project files (`*.csproj`, `*.sln`)

### ❌ Never Sync
- `bin/` and `obj/` folders
- `logs/` folder
- Local database files (`*.mdf`, `*.ldf`)
- User-specific settings

## 🚀 New Laptop Setup

1. **Install Prerequisites**
   ```powershell
   # Install via winget
   winget install Microsoft.VisualStudio.2022.Community
   winget install Microsoft.DotNet.SDK.8
   winget install Microsoft.SQLServerManagementStudio
   ```

2. **Clone Repository**
   ```bash
   git clone https://github.com/Bigessfour/BusBuddy_Syncfusion.git
   cd BusBuddy_Syncfusion
   ```

3. **Setup Syncfusion License**
   - Add license key to `appsettings.json`
   - Or set environment variable `SYNCFUSION_LICENSE_KEY`

4. **Choose Database Option**
   - **Azure**: Run setup script and use cloud database
   - **Local**: Install SQL Server Express and restore backup

5. **Build and Run**
   ```bash
   dotnet restore
   dotnet build
   dotnet run --project BusBuddy.WPF
   ```

## 💡 Pro Tips

### Development Workflow
1. **Daily**: Commit and push code changes
2. **Weekly**: Backup database (if using local)
3. **Project milestones**: Full database migration

### Troubleshooting
- Connection issues: Check firewall settings for Azure
- Migration errors: Use SQL Server Management Studio to verify schema
- Sync conflicts: Use database provider switch to test locally

### Future Improvements
- Consider **Docker** containers for complete environment consistency
- Implement **Azure DevOps** for CI/CD pipelines
- Add **automated testing** with in-memory database

## 📞 Need Help?
1. Check `AzureSetupGuide.md` for detailed Azure setup
2. Run diagnostic scripts in troubleshooting mode
3. Review connection strings in both config files
