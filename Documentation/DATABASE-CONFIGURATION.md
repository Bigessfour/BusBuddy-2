# BusBuddy Database Configuration Update

## Overview
This document summarizes the implementation of a multi-environment database strategy for BusBuddy:
- SQL Server LocalDB for development
- Azure SQL Database for production
- SQLite for legacy support (Phase 1 compatibility)

## Key Components

### 1. Connection Strings
Updated connection strings in:
- `BusBuddy.Core/appsettings.json`
- `BusBuddy.WPF/appsettings.json`
- `BusBuddy.Core/appsettings.Development.json`
- `BusBuddy.Core/appsettings.Production.json`

### 2. Environment Helper
Enhanced the `EnvironmentHelper` class with methods to:
- Detect current environment (Development/Production)
- Identify database provider (LocalDB/Azure/SQLite)
- Select appropriate connection string

### 3. Database Context
Modified `BusBuddyDbContext.cs` to:
- Support SQL Server LocalDB for development
- Optimize Azure SQL for production
- Maintain SQLite for legacy support (Phase 1 compatibility)
- Configure provider-specific optimizations
- Eliminate duplicate context factories that cause EF conflicts

### 4. Utility Scripts
Created PowerShell scripts for database management:
- `Scripts\Setup\setup-localdb.ps1`: Sets up LocalDB for development
- `deploy-azure-sql.ps1`: Deploys schema to Azure SQL
- `switch-database-provider.ps1`: Switches between providers

## Usage Instructions

### Development Environment (LocalDB)
```powershell
# Set up LocalDB
.\Scripts\Setup\setup-localdb.ps1

# Switch to LocalDB provider
.\switch-database-provider.ps1 -Provider LocalDB
```

### Production Environment (Azure SQL)
```powershell
# Deploy to Azure SQL
.\deploy-azure-sql.ps1 -ServerName busbuddy-sql -DatabaseName BusBuddy -AdminUsername admin -ResourceGroup BusBuddy -CreateIfNotExists

# Switch to Azure provider
.\switch-database-provider.ps1 -Provider Azure
```

### Legacy Support (SQLite)
```powershell
# Switch to SQLite provider
.\switch-database-provider.ps1 -Provider SQLite
```

## Implementation Details

### Connection String Format
- **LocalDB**: `Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=BusBuddy;Integrated Security=True;MultipleActiveResultSets=True`
- **Azure SQL**: `Server=tcp:busbuddy-sql.database.windows.net,1433;Initial Catalog=BusBuddy;Persist Security Info=False;User ID=${AZURE_SQL_USER};Password=${AZURE_SQL_PASSWORD};MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;`
- **SQLite**: `Data Source=BusBuddy.db`

### Environment Detection
The system detects the environment using:
1. `IConfiguration["Environment"]` setting
2. `ASPNETCORE_ENVIRONMENT` environment variable
3. Falls back to "Production" if neither is specified

### Provider Selection
The provider is selected from:
1. `DatabaseProvider` configuration setting
2. Falls back to "LocalDB" if not specified
