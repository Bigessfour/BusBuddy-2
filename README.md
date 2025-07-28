# 🚌 BusBuddy - School Transportation Management System

## 🎯 **Mission Statement**

BusBuddy is a transportation management system designed to help school districts efficiently manage their bus fleets, drivers, routes, and schedules. Our goal is to create an intuitive application that improves safety, reduces administrative overhead, and optimizes transportation resources.

## ✅ **Project Status (July 28, 2025)**

- **Current Phase**: Phase 2 Development
- **Build Status**: ✅ FULLY OPERATIONAL - Repository restored and optimized
- **Repository Status**: ✅ Clean Git history, all files fetchable via GitHub
- **Core Features**: Drivers management, vehicle fleet, route planning, activity scheduling
- **Recent Achievements (July 28, 2025)**:
  - ✅ **CRITICAL FIX**: Git repository corruption resolved - full restoration completed
  - ✅ **File Fetchability**: All 494 files now properly accessible via GitHub API
  - ✅ Multi-environment database strategy implemented
  - ✅ SQL Server LocalDB for development environment
  - ✅ Azure SQL for production operations
  - ✅ SQLite support maintained for legacy compatibility
  - ✅ Database provider switching utilities created
  - ✅ Repository synchronized with remote main branch

## 🔧 **Repository Restoration (July 28, 2025)**

**CRITICAL ISSUE RESOLVED**: The Git repository experienced corruption that prevented file commits and GitHub synchronization. This has been completely resolved.

### What Was Fixed
- **Git Repository Corruption**: `fatal: bad object HEAD` errors resolved
- **File Fetchability**: All files now properly accessible via GitHub API
- **Remote Synchronization**: Clean Git history established with proper tracking
- **Development Workflow**: All Git operations fully restored

### Repository Health Status
- ✅ **Git Operations**: All commands working (add, commit, push, pull)
- ✅ **File Access**: 494 files fetchable via GitHub web interface and API
- ✅ **Remote Tracking**: Properly synchronized with `origin/main`
- ✅ **Team Collaboration**: Ready for collaborative development

### Access Methods
- **GitHub Web**: [https://github.com/Bigessfour/BusBuddy-2](https://github.com/Bigessfour/BusBuddy-2)
- **API Access**: `https://api.github.com/repos/Bigessfour/BusBuddy-2/contents/`
- **Raw Files**: `https://raw.githubusercontent.com/Bigessfour/BusBuddy-2/main/[filename]`
- **Clone**: `git clone https://github.com/Bigessfour/BusBuddy-2.git`

📄 **Detailed Report**: See [REPOSITORY-RESTORATION-REPORT.md](REPOSITORY-RESTORATION-REPORT.md) for complete technical details.

## 🗓️ **Upcoming Tasks**

1. **Data Layer Enhancements**:
   - Implement repository patterns for data access
   - Add comprehensive validation layer
   - Optimize query performance

2. **UI Improvements**:
   - Complete dashboard visualization
   - Enhance Syncfusion FluentDark theme implementation
   - Add real-time data updates

3. **Production Readiness**:
   - Complete Azure deployment scripts
   - Implement comprehensive error logging
   - Add user authentication and authorization

## 🚀 **Quick Start Guide**

### RECOMMENDED: BusBuddy PowerShell Environment

**✅ FULLY OPERATIONAL**: The BusBuddy development environment is now completely restored and ready for use. The specialized PowerShell 7.5.2 environment provides enhanced error reporting, automated diagnostics, and standardized workflows.

### Option 1: Enhanced PowerShell Workflow (Recommended)

1. **Open the Project**:
   ```
   Open VS Code → Open Folder → Navigate to BusBuddy folder
   ```

2. **Initialize the PowerShell Environment**:
   ```powershell
   pwsh -ExecutionPolicy Bypass -File "load-bus-buddy-profiles.ps1"
   ```

3. **Set Database Provider** (Choose one):
   ```powershell
   # For development (recommended)
   .\switch-database-provider.ps1 -Provider LocalDB
   .\setup-localdb.ps1

   # For production
   .\switch-database-provider.ps1 -Provider Azure

   # For legacy support
   .\switch-database-provider.ps1 -Provider SQLite
   ```

4. **Build the Solution**:
   ```powershell
   bb-build  # Enhanced build with comprehensive error reporting
   ```

5. **Run the Application**:
   ```powershell
   bb-run    # Run with runtime monitoring and diagnostics
   ```

6. **Health Check and Diagnostics**:
   ```powershell
   bb-health # System health verification with detailed reporting
   ```

### Option 2: Direct Commands (Alternative)

For quick testing or when PowerShell environment is not available:

1. **Build the Solution**:
   ```powershell
   dotnet build BusBuddy.sln
   ```

2. **Run the Application**:
   ```powershell
   dotnet run --project BusBuddy.WPF/BusBuddy.WPF.csproj
   ```

### VS Code Tasks Integration

You can also access these commands through VS Code tasks:
- Press `Ctrl+Shift+P` → "Tasks: Run Task" → "BB: Run App"
- Press `Ctrl+Shift+P` → "Tasks: Run Task" → "PS Fixed: Health Check"

## 🏗️ **Technology Stack**

- **UI Framework**: WPF with Syncfusion Essential Studio 30.1.40
- **Backend**: .NET 8.0 with Entity Framework Core
- **Database**: SQL Server LocalDB (dev), Azure SQL (prod), SQLite (legacy)
- **Architecture**: MVVM pattern with clean separation of concerns
- **Development Environment**: PowerShell 7.5.2 with BusBuddy development system
- **Build Tools**: Custom PowerShell modules with enhanced diagnostics
- **Error Handling**: Comprehensive logging with Serilog integration

## 📊 **Key Features**

### Current Functionality
- **Drivers Management**: Personnel records, qualifications, scheduling
- **Vehicle Fleet**: Bus inventory, maintenance records, assignments
- **Route Planning**: Efficient route creation and management
- **Activity Scheduling**: Field trips, special events, non-standard routes

### Coming Soon
- Enhanced reporting dashboard
- Maintenance scheduling
- Data visualization improvements

## 🔄 **What's New in Phase 2**

Phase 2 development has delivered significant improvements to the BusBuddy platform:

### Multi-Environment Database Strategy
- **SQL Server LocalDB**: Optimized for development workflows
- **Azure SQL**: Enterprise-ready for production deployments
- **SQLite**: Maintained for backward compatibility
- **Automated Setup**: PowerShell scripts for environment configuration

### Enhanced Development Environment
- **PowerShell 7.5.2 Integration**: Specialized development environment
- **Build Pipeline**: Advanced error detection and reporting
- **VS Code Tasks**: Comprehensive task integration
- **Environment Validation**: Automated checks and validations

### Architecture Improvements
- **Entity Framework Core**: Optimized for multiple database providers
- **Repository Pattern**: Improved data access layer (in progress)
- **Serilog Integration**: Comprehensive application logging
- **Environment Helpers**: Simplified environment-specific configuration

### Database Migration and Deployment
- **Migration Scripts**: Entity Framework migrations for all providers
- **Azure Deployment**: Automated Azure SQL deployment
- **LocalDB Setup**: Automated developer environment setup
- **Connection Management**: Enhanced connection string handling

## 🧰 **Development Guide**

### BusBuddy Development Workflow

```powershell
# MANDATORY: Initialize the PowerShell environment
pwsh -ExecutionPolicy Bypass -File "load-bus-buddy-profiles.ps1"

# Set up database for development (SQL LocalDB)
.\switch-database-provider.ps1 -Provider LocalDB
.\setup-localdb.ps1

# Build the solution
bb-build        # Enhanced build with error analysis

# Run the application
bb-run          # Run with diagnostics and monitoring

# Check system health
bb-health       # Comprehensive health check

# Clean solution
bb-clean        # Enhanced cleaning with validation
```

### Emergency Fallback Only

In emergency situations where the PowerShell environment is unavailable, these standard commands can be used as temporary alternatives. Note that they lack the error reporting and diagnostics of the primary workflow:

```powershell
# EMERGENCY USE ONLY - Standard commands lack error reporting and diagnostics
dotnet build BusBuddy.sln
dotnet run --project BusBuddy.WPF/BusBuddy.WPF.csproj
```

### Git Workflow
```powershell
# Check status of your changes
git status

# Stage your changes
git add .

# Commit with a descriptive message
git commit -m "Description of your changes"

# Push to remote repository
git push
```

## 📖 **Documentation**

Comprehensive documentation is available in the `Documentation/` folder:

- [Database Configuration](./Documentation/DATABASE-CONFIGURATION.md) - Multi-environment database setup
- [PowerShell 7.5.2 Features](./Documentation/PowerShell-7.5.2-Reference.md) - Development environment reference
- [Phase 2 Implementation Plan](./Documentation/PHASE-2-IMPLEMENTATION-PLAN.md) - Current development roadmap
- [Package Management](./Documentation/PACKAGE-MANAGEMENT.md) - NuGet dependency management

### VS Code Integration
- **Recommended Extensions**: PowerShell, C# Dev Kit, Task Explorer
- **Debugging**: Use F5 or the Debug menu after configuring launch.json
- **Tasks**: Access via Ctrl+Shift+P → "Tasks: Run Task" for build and run operations

### VS Code Launch Configuration
Add this to your `.vscode/launch.json` file for debugging:

```json
{
  "name": "Debug BusBuddy",
  "type": "coreclr",
  "request": "launch",
  "preLaunchTask": "Build Solution",
  "program": "${workspaceFolder}/BusBuddy.WPF/bin/Debug/net8.0-windows/BusBuddy.WPF.dll",
  "args": [],
  "cwd": "${workspaceFolder}/BusBuddy.WPF",
  "stopAtEntry": false,
  "console": "internalConsole"
}
```

### PowerShell Development Environment

BusBuddy uses a custom PowerShell 7.5.2 environment for all development operations:

- **Environment Initialization**: `load-bus-buddy-profiles.ps1` loads the required modules and functions
- **Enhanced Commands**: Custom `bb-*` commands provide advanced functionality for building, running, and testing
- **Error Reporting**: Comprehensive error capture and analysis with actionable recommendations
- **Build Pipeline**: Advanced build pipeline with dependency validation and environment checking
- **VS Code Integration**: Task integration for all common development workflows

For complete details on the PowerShell environment, see [PowerShell 7.5.2 Reference](./Documentation/PowerShell-7.5.2-Reference.md).

### Syncfusion Controls
BusBuddy uses Syncfusion Essential Studio for WPF 30.1.40 for its UI components:

```csharp
// Required license registration in App.xaml.cs
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY");
```

### Syncfusion Theme Implementation
Add this to your `App.xaml` to implement the FluentDark theme:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/Syncfusion.Themes.FluentDark.WPF;component/fluent.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

## 🔧 **Troubleshooting**

### Database Configuration

BusBuddy supports multiple database providers:

1. **LocalDB (Development)**: SQL Server LocalDB for development
   ```powershell
   # Set up LocalDB for development
   .\setup-localdb.ps1

   # Switch to LocalDB provider
   .\switch-database-provider.ps1 -Provider LocalDB
   ```

2. **Azure SQL (Production)**: Azure SQL Database for production deployments
   ```powershell
   # Deploy schema to Azure SQL
   .\deploy-azure-sql.ps1 -ServerName busbuddy-sql -DatabaseName BusBuddy -AdminUsername busbaddyadmin -ResourceGroup BusBuddy -CreateIfNotExists

   # Switch to Azure SQL provider
   .\switch-database-provider.ps1 -Provider Azure
   ```

3. **SQLite (Legacy)**: SQLite database for Phase 1 compatibility
   ```powershell
   # Switch to SQLite provider (legacy)
   .\switch-database-provider.ps1 -Provider SQLite
   ```

### Common Issues
- **Build Errors**: Check the Problems panel in VS Code for specific errors
- **UI Rendering**: Verify Syncfusion theme registration in App.xaml.cs
- **Runtime Crashes**: Use try/catch blocks and check the Output window
- **Database Issues**: Verify connection string and check EF Core migrations

### Getting Help
- Review detailed instructions in `.vscode/instructions.md`
- Use VS Code's debugging tools to diagnose issues
- If using PowerShell helpers, run `bb-health` for comprehensive diagnostics

## 📁 **Project Structure**

```
BusBuddy/
├── BusBuddy.Core/          # Business logic layer
│   ├── Models/             # Domain entities
│   ├── Services/           # Business services
│   ├── Data/               # Data access
│   └── Migrations/         # EF Core migrations
├── BusBuddy.WPF/           # Presentation layer
│   ├── ViewModels/         # MVVM ViewModels
│   ├── Views/              # XAML Views
│   ├── Controls/           # Custom controls
│   └── Services/           # UI services
├── BusBuddy.Tests/         # Unit & integration tests
├── BusBuddy.UITests/       # UI automation tests
└── Documentation/          # Project documentation
```

## 📚 **Documentation**

For detailed documentation and instructions:
- See `.vscode/instructions.md` for comprehensive development guidance
- Check the `Documentation/` folder for architectural and usage guides
- Visit the [Syncfusion WPF Documentation](https://help.syncfusion.com/wpf/welcome-to-syncfusion-essential-wpf) for UI component guides

### VS Code Task Configurations
Key tasks available in the VS Code Tasks menu (Ctrl+Shift+P → "Tasks: Run Task"):

| Task Name | Description |
|-----------|-------------|
| `Direct: Build Solution (CMD)` | Clean build of the solution |
| `Direct: Run Application (FIXED)` | Run the application with proper paths |
| `BB: Run App` | Run with PowerShell helpers (if loaded) |
| `🛡️ BB: Dependency Security Scan` | Scan for package vulnerabilities |
| `PS Fixed: Health Check` | Run system health check |

---

## Contributing

BusBuddy is designed to help school districts manage transportation efficiently. Your contributions to improve the application are welcome!

## License

This project is licensed under the MIT License - see the LICENSE file for details.
