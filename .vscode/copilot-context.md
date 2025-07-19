
# Bus Buddy - Codespace Context (July 2025)

## Project Overview
Bus Buddy is a modern, enterprise-grade school bus transportation management system built with **.NET 8.0**, **WPF**, and **Syncfusion's professional WPF UI suite**. The application is 100% pure WPF (no legacy UI frameworks) and leverages advanced MVVM, dependency injection, and professional theming for a rich, maintainable desktop experience.

## Architecture & Key Technologies
- **.NET 8.0 (net8.0-windows)**: Latest .NET for Windows desktop
- **WPF (Windows Presentation Foundation)**: Native UI, hardware-accelerated, modern XAML
- **Syncfusion WPF Controls**: All UI (DataGrid, DockingManager, Charts, Scheduler, Ribbon, etc.)
- **Entity Framework Core 8**: Data access, SQL Server backend
- **Microsoft.Extensions.DependencyInjection**: DI for all services/ViewModels
- **Microsoft.Extensions.Logging**: Structured logging throughout
- **Office2019Colorful Theming**: Consistent, professional look

## Solution Structure
```
Bus Buddy/
├── BusBuddy.WPF/          # WPF UI Layer (Views, ViewModels, Syncfusion Controls)
│   ├── Views/             # XAML UserControls (Dashboard, Maintenance, Student, Fuel, Schedule, etc.)
│   ├── ViewModels/        # MVVM ViewModels (INotifyPropertyChanged, Commands)
│   ├── Controls/          # Custom WPF UserControls
│   ├── Converters/        # Value converters for data binding
│   └── Resources/         # Styles, templates, images
├── BusBuddy.Core/         # Business logic, services, models
├── Data/                  # EF Core DbContext/config
├── Models/                # Entity models
├── Services/              # Business/data services
├── Migrations/            # EF Core migrations
└── Configuration/         # App settings/config
```

## Key Components & Architecture Details

### Activity Schedule Management
- **ActivityScheduleView.xaml**: Main CRUD interface with SfDataGrid for schedule listing and management buttons
- **ActivityScheduleDialog.xaml**: Modal dialog for add/edit operations with Syncfusion input controls
- **ActivityScheduleViewModel.cs**: MVVM ViewModel with proper async patterns, validation, and repository integration
- **Repository Pattern**: Uses `IActivityScheduleRepository` extending `IRepository<ActivitySchedule>` for data access
- **Validation**: Client-side validation with user-friendly error messages displayed in UI
- **Logging**: Comprehensive Serilog structured logging for all operations and user interactions

### MVVM Implementation Standards
- **ViewModels**: Inherit from `ObservableObject` (CommunityToolkit.Mvvm)
- **Properties**: Use `[ObservableProperty]` attribute for auto-generated properties
- **Commands**: Use `[RelayCommand]` attribute for auto-generated commands with async support
- **Constructor Pattern**: Fire-and-forget async loading using `Task.Run(() => { await LoadDataAsync(); })`
- **Null Safety**: Proper null checks before service calls and data operations
- **Error Handling**: Try-catch blocks with structured logging for all async operations

### Repository & Data Access Patterns
- **Generic Repository**: `IRepository<T>` provides common CRUD operations
- **Specific Repositories**: Domain-specific repositories like `IActivityScheduleRepository` with specialized queries
- **Unit of Work**: `IUnitOfWork` pattern for transaction management and repository coordination
- **Method Usage**:
  - `GetAllAsync()`, `AddAsync()`, `Update()`, `RemoveByIdAsync()` for CRUD operations
  - `SaveChangesAsync()` through Unit of Work for transaction commits

## Syncfusion Components Used
- **SfDataGrid**: Data display (students, logs, maintenance, etc.)
- **SfChart**: Analytics, metrics, visualizations
- **SfGauge**: Fuel, performance, and status metrics
- **DockingManager**: Dashboard layout, tabbed/panel UI
- **SfScheduler**: Schedule/calendar management
- **Ribbon, TabControl, TreeView**: Navigation
- **SfBusyIndicator, ProgressBar, ButtonAdv, ComboBoxAdv**: Enhanced controls

## Build & Run Workflow
1. **dotnet clean**: Clean build artifacts
2. **dotnet restore**: Restore NuGet packages
3. **dotnet build**: Build solution
4. **dotnet run**: Launch WPF app
   - All tasks are sequential, each with dedicated terminal output
   - Output must be reviewed before proceeding to next step

## Configuration & Environment
- **appsettings.json**: Local config (SQL Server, Syncfusion key)
- **appsettings.azure.json**: Azure/Cloud config
- **Environment Variables**: `SYNCFUSION_LICENSE_KEY`, `ASPNETCORE_ENVIRONMENT`
- **.editorconfig**: Strict C# style, nullable enforcement
- **.vscode/settings.json**: Copilot, IntelliSense, error handling, workspace context
- **.vscode/tasks.json**: Only clean, restore, build, run (sequential, dedicated panel)

## Coding Standards & AI Guidance
- Always use Syncfusion controls for UI
- Prefer MVVM, no business logic in code-behind
- Log all user actions and errors
- Use async/await for all data access
- Use proper nullable annotations
- Prefer ObservableCollection<T> for data binding
- Follow C# naming conventions

## Current Focus & Status
- **Dashboard**: Enhanced with Syncfusion DockingManager, DataGrid, Charts, Gauges, and dynamic panels
- **Maintenance**: Visual analytics, advanced tracking, logging
- **Fuel**: Real-time metrics, charts, gauges
- **Student**: Enhanced data grids, search/filter
- **Schedule**: Integrated SfScheduler, custom templates
- **Activity Schedule Management**: Fully functional CRUD operations with ActivityScheduleView and ActivityScheduleDialog
- **All panels**: Use real Syncfusion-based views, no placeholders
- **Build/Run**: Fully automated, sequential, with visible output
- **Status**: Current build is clean with no errors or warnings
- **Testing**: NUnit-based test suite available in BusBuddy.Tests project for business logic validation

---
**For Copilot/AI:**
- Use this context for all code suggestions
- Always prefer Syncfusion, MVVM, async, and logging
- No legacy UI, no code-behind logic except event logging
- All code must be .NET 8, WPF, and Syncfusion compliant
- Follow the troubleshooting patterns documented above for common issues
- Maintain the established architecture and patterns for consistency

````

## Recent Fixes Completed (July 19, 2025)
- ✅ **XAML Spacing Property**: Removed unsupported `Spacing` property from StackPanel controls in ActivityScheduleDialog.xaml and ActivityScheduleView.xaml
- ✅ **XAML Structure**: Fixed invalid XML structure by removing C# code fragments that were incorrectly placed after closing `</UserControl>` tags
- ✅ **ActivityScheduleViewModel**: Completely restructured class with proper braces, method nesting, and MVVM patterns
- ✅ **Repository Integration**: Fixed `IActivityScheduleRepository` usage with proper using statements and method calls
- ✅ **Async Patterns**: Implemented proper fire-and-forget async loading in ViewModel constructor using `Task.Run()`
- ✅ **Null Safety**: Added proper null checks in ActivityScheduleView.xaml.cs for DialogService calls
- ✅ **Method Mapping**: Updated to use correct repository methods (`Update()`, `RemoveByIdAsync()`) instead of non-existent methods
- ✅ **Logging**: Fixed Serilog structured logging calls with proper property formatting

## Documentation & Support
- **README.md**: Full architecture, setup, and usage guide
- **MULTI_LAPTOP_SETUP.md**: Multi-device dev setup
- **SETUP_NEW_LAPTOP.md**: New device onboarding
- **DEVELOPMENT_SUMMARY.md**: Dev process and troubleshooting
- **Syncfusion licensing**: See README and appsettings

## Troubleshooting & Common Issues Resolved

### XAML Compilation Errors
- **MC3072 Spacing Property**: Remove `Spacing` attribute from StackPanel (not supported in standard WPF namespace)
- **MC3000 Invalid XML**: Ensure no C# code exists after closing XML tags in XAML files
- **InitializeComponent()**: Auto-generated method requires clean XAML structure and successful build

### C# Compilation Errors
- **CS1514/CS1513 Brace Errors**: Ensure proper class and method brace structure
- **CS8604 Null Reference**: Add null checks before method calls, especially for casted objects
- **Repository Method Errors**: Use correct method names from `IRepository<T>` interface
- **Async Pattern Errors**: Use `Task.Run()` for fire-and-forget async in constructors

### Build Process
- **Clean → Restore → Build**: Always follow sequential process for reliable builds
- **Verification**: Check for errors after each step before proceeding
- **Status**: Current build is clean with no errors or warnings
