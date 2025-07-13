
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

## Key Patterns & Conventions
- **MVVM**: All UI follows Model-View-ViewModel, no code-behind except event logging
- **Dependency Injection**: All services/ViewModels registered in DI
- **Strict Nullable Reference Types**: Warnings as errors, use `string?`, `object?`
- **Logging**: Use `ILogger<T>` everywhere, log all user actions and errors
- **Async/Await**: All data operations are async
- **ObservableCollection<T>**: For all data-bound collections
- **C# Naming**: PascalCase for public, camelCase for private/fields
- **No Legacy UI**: No Windows Forms, WinForms, or web controls

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
- **All panels**: Use real Syncfusion-based views, no placeholders
- **Build/Run**: Fully automated, sequential, with visible output

## Documentation & Support
- **README.md**: Full architecture, setup, and usage guide
- **MULTI_LAPTOP_SETUP.md**: Multi-device dev setup
- **SETUP_NEW_LAPTOP.md**: New device onboarding
- **DEVELOPMENT_SUMMARY.md**: Dev process and troubleshooting
- **Syncfusion licensing**: See README and appsettings

---
**For Copilot/AI:**
- Use this context for all code suggestions
- Always prefer Syncfusion, MVVM, async, and logging
- No legacy UI, no code-behind logic except event logging
- All code must be .NET 8, WPF, and Syncfusion compliant
