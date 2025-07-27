# 🚌 BusBuddy - School Transportation Management System

## 🎯 **Project Overview**

BusBuddy is a transportation management system designed to help school districts efficiently manage their bus fleets, drivers, routes, and schedules. Our goal is to create an intuitive application that improves safety, reduces administrative overhead, and optimizes transportation resources.

## 📋 **Table of Contents**

- [Quick Start Guide](#-quick-start-guide)
- [Project Status & Roadmap](#-project-status--roadmap)
- [Development Environment Setup](#-development-environment-setup)
- [Problem Resolution Approaches](#-problem-resolution-approaches)
- [Git & Repository Tips](#-git--repository-tips)
- [VS Code Integration](#-vs-code-integration)
- [Syncfusion Implementation](#-syncfusion-implementation)
- [Debugging & Troubleshooting](#-debugging--troubleshooting)
- [Quick Reference Commands](#-quick-reference-commands)
- [Application Architecture](#-application-architecture)

## 🚀 **Quick Start Guide**

### Basic Development Setup (5 Minutes)

1. **Open the Project**:
   ```
   Open VS Code → Open Folder → Navigate to BusBuddy folder
   ```

2. **Build the Solution**:
   - Use VS Code Task: `⌨️ Ctrl+Shift+P → "Tasks: Run Task" → "Direct: Build Solution (CMD)"`
   - Or use PowerShell: `dotnet build BusBuddy.sln`

3. **Run the Application**:
   - Use VS Code Task: `⌨️ Ctrl+Shift+P → "Tasks: Run Task" → "Direct: Run Application (FIXED)"`
   - Or use PowerShell: `dotnet run --project BusBuddy.WPF/BusBuddy.WPF.csproj`

4. **PowerShell Helpers** (Optional but Recommended):
   ```powershell
   pwsh -ExecutionPolicy Bypass -File "load-bus-buddy-profiles.ps1"
   bb-health -Quick
   ```

## 🎯 **Project Status & Roadmap**

### Current Status: Phase 2
- ✅ **Phase 1 Completed**: MainWindow → Dashboard → 3 Core Views (Drivers, Vehicles, Activities)
- 🔄 **Phase 2 In Progress**:
  - Enhancing UI/UX with consistent Syncfusion styling
  - Improving MVVM architecture
  - Expanding test coverage
  - Optimizing performance

### Key Features
- **Drivers Management**: Personnel records, qualifications, scheduling
- **Vehicle Fleet**: Bus inventory, maintenance records, assignments
- **Route Planning**: Efficient route creation and management
- **Activity Scheduling**: Field trips, special events, non-standard routes

### Next Milestone Goals
- Complete route management interface
- Implement maintenance scheduling
- Add reporting dashboard
- Enhance data visualization

## � **Development Environment Setup**

BusBuddy uses PowerShell tools to streamline development, but direct commands always work too.

### Standard Approach (Works Anywhere)
```powershell
# Build the solution
dotnet build BusBuddy.sln

# Run the application
dotnet run --project BusBuddy.WPF/BusBuddy.WPF.csproj

# Clean the solution
dotnet clean BusBuddy.sln
```

### Enhanced Development Environment (REQUIRED)
The BusBuddy PowerShell profile is REQUIRED for all development activities:

```powershell
# REQUIRED: Load the PowerShell environment (gives access to bb-* commands)
pwsh -ExecutionPolicy Bypass -File "load-bus-buddy-profiles.ps1"

# Health check
bb-health

# Build and run with enhanced monitoring
bb-build
bb-run
```

### PowerShell Profile Benefits
The BusBuddy PowerShell profile (`PowerShell\BusBuddy PowerShell Environment\Modules\BusBuddy\BusBuddy.psm1`) provides:
- Health check and diagnostic tools
- Build process with enhanced error detection
- Comprehensive error analysis and troubleshooting
- AI code generation integration
- Development workflow automation

**Note**: These tools are REQUIRED for consistent development workflow. Only use standard .NET commands as emergency fallbacks.

## � **Problem Resolution Approaches**

When encountering issues, you have multiple resolution paths:

### Standard Troubleshooting
1. **Check Build Errors**: Look at specific error messages and line numbers
2. **Use VS Code Debugging**: Set breakpoints and step through code
3. **Review Output Window**: Check for additional diagnostics and logs
4. **Clean and Rebuild**: Clear artifacts with `dotnet clean` and rebuild

### Using PowerShell Helpers
If you've loaded the PowerShell environment, these commands can help:

## � **Problem Resolution Approaches**

When encountering issues, follow these resolution paths:

### REQUIRED: PowerShell-Based Troubleshooting
```powershell
# Health check (always the first step)
bb-health

# Build with detailed output
bb-build -Verbosity detailed

# Run with debug logging
bb-run -EnableDebug

# Export diagnostics (for sharing issues)
bb-debug-export

# Advanced diagnostics
bb-diagnostic
```

### Emergency Fallback Only
Only if PowerShell environment cannot be loaded:
1. **Check Build Errors**: Run `dotnet build BusBuddy.sln` and review errors
2. **Use VS Code Debugging**: Set breakpoints and step through code
3. **Review Output Window**: Check for additional diagnostics and logs
4. **Clean and Rebuild**: Clear artifacts with `dotnet clean` and rebuild

### Problem Resolution Strategy
1. **Try small fixes first** - Target specific errors before large changes
2. **Keep what works** - Build incrementally on working components
3. **Ask for help** - Consult team members for complex issues
4. **Document solutions** - Note what worked for future reference

**Remember**: Start with simple solutions before complex ones.

## 🧰 **Git & Repository Tips**

### PowerShell Git Commands
```powershell
# PowerShell-friendly git commands
git status                                                 # Check repo status
git add .                                                  # Stage all changes
git commit -m "Add feature X"                              # Commit with message
git push                                                   # Push to remote

# PowerShell alternatives to Unix commands
git ls-files | Where-Object { $_ -match "\.cs$" }          # Find C# files
git status --porcelain | Where-Object { $_ -match "^.M" }  # Modified files only
(git ls-files | Measure-Object).Count                      # Count tracked files
```

## 🧰 **VS Code Integration**

VS Code is the primary development environment for BusBuddy. These configurations help streamline development:

### Task Integration
Use VS Code tasks to simplify common operations:

| VS Code Task | Description | How to Access |
|-------------|-------------|---------------|
| `Direct: Build Solution (CMD)` | Build the solution | Ctrl+Shift+P → "Tasks: Run Task" |
| `Direct: Run Application (FIXED)` | Run the application | Ctrl+Shift+P → "Tasks: Run Task" |
| `BB: Run App` | Run with PowerShell helpers | Ctrl+Shift+P → "Tasks: Run Task" |
| `�️ BB: Dependency Security Scan` | Security scan | Ctrl+Shift+P → "Tasks: Run Task" |

### Recommended Extensions
These extensions enhance the development experience:
- **PowerShell** (ms-vscode.powershell)
- **C# Dev Kit** (ms-dotnettools.csdevkit)
- **Task Explorer** (spmeesseman.vscode-taskexplorer)

### Debugging Configuration
For debugging the application, use this launch configuration:

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

**Tip**: Use F5 to start debugging after adding this to your launch.json.

## 🎨 **Syncfusion Controls Implementation**

Syncfusion provides the UI components for BusBuddy's interface:

### Key Controls
- **DockingManager**: Main layout for dashboard panels
- **DataGrid**: For displaying driver, vehicle, and route data
- **RibbonControl**: Navigation and command interface
- **Charts**: Data visualization for analytics

### Required Setup
```csharp
// In App.xaml.cs - Register license before UI initialization
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY");
```

### Theme Implementation
```xml
<!-- In App.xaml -->
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/Syncfusion.Themes.FluentDark.WPF;component/fluent.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### Documentation Resources
- [Syncfusion WPF Documentation](https://help.syncfusion.com/wpf/welcome-to-syncfusion-essential-wpf)
- [Control Gallery](https://help.syncfusion.com/wpf/control-gallery)
- [Theme Documentation](https://help.syncfusion.com/wpf/themes/getting-started)

## 🐛 **Debugging & Troubleshooting**

### Common Issues & Solutions

| Issue Type | Troubleshooting Steps | Resources |
|------------|----------------------|-----------|
| **Build Errors** | Check exact error message and line number | VS Code Problems panel |
| **UI Rendering** | Verify Syncfusion theme registration in App.xaml.cs | Syncfusion documentation |
| **Runtime Crashes** | Use try/catch blocks and log exceptions | Exception details window |
| **Database Issues** | Check connection string, verify migrations | SQL Server explorer |

### VS Code Debug Techniques

1. **Set breakpoints**: Click in the gutter to the left of line numbers
2. **Use watch window**: Add variables to monitor during execution
3. **Step through code**: Use F10 (step over) and F11 (step into)
4. **Monitor output**: Check the Debug Console for logs and errors

### Advanced Diagnostics

If you've loaded the PowerShell helpers, additional diagnostics are available:

```powershell
# Health check for common issues
bb-health

# Capture runtime errors in real-time
bb-debug-stream

# Export diagnostic information for sharing
bb-debug-export
```

**Remember**: Most issues can be solved with standard VS Code debugging!

---
## 🔑 **Quick Reference Commands**

### Essential Commands
```powershell
# Build and run (standard)
dotnet build BusBuddy.sln
dotnet run --project BusBuddy.WPF/BusBuddy.WPF.csproj

# PowerShell helpers (optional)
pwsh -ExecutionPolicy Bypass -File "load-bus-buddy-profiles.ps1"
bb-health
bb-build
bb-run
```

### Git Essentials
```powershell
git add .
git commit -m "Descriptive message"
git push
```

## 🏗️ **Application Architecture**

BusBuddy follows a layered architecture pattern with clean separation of concerns:

### Project Structure
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
└── BusBuddy.Tests/         # Test projects
```

### Key Design Patterns

#### MVVM Implementation
- **ViewModels**: Handle UI logic and state management
- **Commands**: Use RelayCommand pattern for UI actions
- **Data Binding**: Two-way binding to model properties
- **View Navigation**: Frame or ContentControl-based navigation

#### Core Data Concepts
- **Entity Framework**: Code-first database access
- **Repository Pattern**: Centralized data access
- **Business Services**: Implement domain logic
- **DTOs**: Transfer objects between layers

### Key Components
- **Drivers**: Personnel management
- **Vehicles**: Bus fleet management
- **Routes**: Transportation route planning
- **Activities**: Special events and field trips

### Syncfusion Integration Points
- **Main Dashboard**: DockingManager with multiple panels
- **Data Displays**: SfDataGrid for tabular data
- **Navigation**: RibbonControl for app-wide navigation
- **Visualization**: ChartControl for analytics

### Recommended Resources
- [MVVM Documentation](https://learn.microsoft.com/en-us/dotnet/architecture/maui/mvvm)
- [EF Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [Syncfusion Guides](https://help.syncfusion.com/wpf/welcome-to-syncfusion-essential-wpf)

## 🔍 **PowerShell Development Environment**

The PowerShell development environment is REQUIRED for all development activities:

```powershell
# REQUIRED: Load the PowerShell environment
pwsh -ExecutionPolicy Bypass -File "load-bus-buddy-profiles.ps1"

# Most useful commands
bb-health      # Check project health
bb-build       # Build the solution
bb-run         # Run the application
bb-diagnostic  # Comprehensive diagnostics
bb-dev-session # Complete development session setup
```

**Remember**: Always use the PowerShell environment for consistent development workflow.
### 📝 **PowerShell Coding Standards**

BusBuddy enforces strict PowerShell coding standards through PSScriptAnalyzer:

#### 🔄 **Approved PowerShell Verbs**

All PowerShell functions MUST use approved verbs as defined in Microsoft's documentation:
[PowerShell Approved Verbs](https://learn.microsoft.com/en-us/powershell/utility-modules/psscriptanalyzer/rules/useapprovedverbs)

```powershell
# Approved verb groups and examples:
# Common: Get, Set, New, Remove, Enable, Disable, Start, Stop, Read, Write
# Data: Backup, Restore, Import, Export, Sync, Update, Convert
# Lifecycle: Install, Uninstall, Register, Unregister, Request, Resume
# Security: Grant, Revoke, Block, Unblock, Protect, Unprotect
```

❌ **AVOID these non-approved verbs:**
- Display/Show (use `Write-*` or `Format-*` instead)
- Build (use `New-*` or `ConvertTo-*` instead)
- Execute (use `Invoke-*` instead)
- Parse (use `ConvertFrom-*` instead)

✅ **CORRECT function naming:**
```powershell
function Write-DependencySummary { ... }  # NOT Show-DependencySummary
function New-ProjectTemplate { ... }      # NOT Build-ProjectTemplate
function Invoke-BuildProcess { ... }      # NOT Execute-BuildProcess
function Get-SystemHealth { ... }         # NOT Check-SystemHealth
```

#### 🔧 **PSScriptAnalyzer Enforcement**

BusBuddy uses PSScriptAnalyzer to enforce these standards:

```powershell
# Validate script with BusBuddy PSScriptAnalyzer settings
Invoke-ScriptAnalyzer -Path "YourScript.ps1" -Settings ".vscode/PSScriptAnalyzerSettings.psd1"
```

The VS Code task `� BB: Mandatory PowerShell 7.5.2 Syntax Check` will verify all scripts against these standards.
