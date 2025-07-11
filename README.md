# Bus Buddy - Transportation Management System

**Current State:**
- **100% Modern WPF Application** - Built entirely with WPF (Windows Presentation Foundation) using Syncfusion's professional WPF UI component suite
- **No Legacy Dependencies** - Completely free of WinForms or other legacy UI frameworks
- **.NET 8 (net8.0-windows)** - Latest .NET framework with modern WPF architecture patterns
- **Syncfusion WPF Components** - Professional-grade UI controls including DataGrid, DockingManager, Charts, and Ribbon
- **Modern MVVM Architecture** - Follows WPF best practices with ViewModels, Data Binding, and Command patterns
- **Syncfusion Licensing** - Properly handled via environment variable (`SYNCFUSION_LICENSE_KEY`) or `appsettings.json` fallback
- **Enterprise-Ready** - Production-ready codebase with comprehensive error handling, logging, and testing

[![CI/CD Pipeline](https://github.com/Bigessfour/BusBuddy_Syncfusion/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/Bigessfour/BusBuddy_Syncfusion/actions/workflows/ci-cd.yml)
[![codecov](https://codecov.io/gh/Bigessfour/BusBuddy_Syncfusion/branch/master/graph/badge.svg)](https://codecov.io/gh/Bigessfour/BusBuddy_Syncfusion)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Syncfusion](https://img.shields.io/badge/Syncfusion-30.1.37-orange.svg)](https://www.syncfusion.com/)

A comprehensive school bus transportation management system built with **C# .NET 8 WPF** and **Syncfusion's professional WPF UI component suite**. This modern WPF application leverages the latest in Microsoft's presentation framework technology to deliver a rich, responsive desktop experience.

## Why WPF (Windows Presentation Foundation)?

This project is built exclusively with **WPF** to provide:
- **Rich Desktop Experience**: Native Windows application with hardware-accelerated graphics
- **Professional UI Components**: Leverages Syncfusion's mature WPF control suite
- **Powerful Data Binding**: Declarative XAML binding for real-time UI synchronization
- **MVVM Architecture**: Industry-standard pattern for maintainable desktop applications
- **Scalable Performance**: Optimized for handling large datasets in transportation management
- **Enterprise Features**: Advanced controls like DockingManager, DataGrid, and Charts
- **Modern .NET Integration**: Full compatibility with .NET 8 and latest C# features

## Key WPF Architecture Benefits
- **Rich Desktop UI**: Native Windows desktop application with hardware acceleration
- **Data Binding**: Powerful XAML data binding for real-time UI updates
- **MVVM Pattern**: Clean separation of concerns with Model-View-ViewModel architecture
- **Syncfusion WPF Components**: Professional UI controls optimized for desktop performance
- **Responsive Design**: Adaptive layouts that work across different screen resolutions
- **Native Performance**: Direct Windows API integration for optimal performance

## Features

### Implemented Modules
- ✅ **Dashboard**: Modern UI with 10 management modules
- ✅ **Bus Management**: Complete fleet vehicle management with CRUD operations
- ✅ **Driver Management**: Driver information and license tracking
- ✅ **Route Management**: Route planning and assignment management
- 🚧 **Schedule Management**: Bus scheduling system (in development)
- 🚧 **Student/Passenger Management**: Student transportation tracking (in development)
- 🚧 **Maintenance Tracking**: Vehicle maintenance records (in development)
- 🚧 **Fuel Management**: Fuel consumption tracking (in development)
- 🚧 **Activity Logging**: System activity tracking (in development)
- 🚧 **Ticket Management**: Passenger ticketing system (in development)

### Core WPF Features
- **Modern Syncfusion WPF Interface**: Professional desktop UI with rich controls and theming
- **MVVM Architecture**: Clean separation using ViewModels, Commands, and Data Binding
- **Database Integration**: Entity Framework Core with SQL Server for robust data management
- **Dependency Injection**: Modern .NET dependency injection container for clean architecture
- **WPF Data Binding**: Real-time UI updates through property change notifications
- **Comprehensive WPF Logging**: Structured logging integrated throughout the WPF application
- **WPF Form Validation**: Rich validation with immediate visual feedback in the UI
- **Exception Handling**: WPF-specific error handling with user-friendly dialog presentation

## Technology Stack
- **.NET 8 WPF Application** - Modern Windows Presentation Foundation desktop application
- **Syncfusion WPF Components** - Professional-grade UI controls (DataGrid, DockingManager, Charts, Ribbon)
- **Entity Framework Core** - Object-relational mapping for data access
- **SQL Server** - Enterprise database backend
- **MVVM Pattern** - Model-View-ViewModel architecture pattern for WPF
- **Microsoft Extensions** - Logging, Dependency Injection, Configuration management
- **XAML** - Declarative markup for rich WPF user interfaces

## Getting Started

### Prerequisites
- **Visual Studio 2022** (recommended for WPF development) or **VS Code** with C# extension
- **.NET 8 SDK** (net8.0-windows target framework)
- **SQL Server** (LocalDB, Express, or full instance)
- **Syncfusion License** (Community or Commercial) for WPF UI components
- **Windows 10/11** (WPF requires Windows operating system)

### Installation
1. Clone the repository
2. Configure the connection string in `appsettings.json`
3. **Syncfusion License Key:**
   - Set the `SYNCFUSION_LICENSE_KEY` environment variable (recommended for security and CI/CD)
   - Or add your license key to `appsettings.json` under `Syncfusion:LicenseKey` or `SyncfusionLicenseKey`
   - The application will not start if the license key is missing (see `App.xaml.cs` for details)
4. **Environment Configuration:**
   - Set the `ASPNETCORE_ENVIRONMENT` environment variable to control application behavior:
     - `Development`: Enables detailed error messages and sensitive data logging (NEVER use in production)
     - `Staging`: Production-like environment with some debugging features
     - `Production`: Full production mode with security safeguards (default if not set)
   - The application will prevent sensitive data logging in production for security
5. Run database migrations:
   ```bash
   dotnet ef database update
   ```
6. Build and run the application:
   ```bash
   dotnet build
   dotnet run --project BusBuddy.WPF/BusBuddy.WPF.csproj
   ```

## Architecture

### Project Structure
```
Bus Buddy/
├── BusBuddy.WPF/          # WPF UI Layer (XAML Views, ViewModels, Syncfusion Controls)
│   ├── Views/             # WPF UserControls and Windows
│   ├── ViewModels/        # MVVM ViewModels with INotifyPropertyChanged
│   ├── Controls/          # Custom WPF UserControls
│   ├── Converters/        # WPF Value Converters for data binding
│   └── Resources/         # WPF Resources (Styles, Templates, Images)
├── BusBuddy.Core/         # Core business logic and services
├── Data/                  # Entity Framework DbContext and configurations
├── Models/                # Entity Framework entity models
├── Services/              # Business Logic Layer and data services
├── Migrations/            # EF Core database migrations
└── Configuration/         # Application settings and configuration
```

### Key Design Patterns
- **MVVM (Model-View-ViewModel)**: Core WPF architectural pattern for clean separation of concerns
- **Repository Pattern**: Service layer abstraction for data access
- **Dependency Injection**: Modern .NET DI container for loose coupling
- **Command Pattern**: WPF Commands for handling user interactions
- **Observer Pattern**: INotifyPropertyChanged for data binding in WPF
- **Factory Pattern**: Service container management and object creation

## Database Schema

### Core Entities
- **Vehicles (Buses)**: Fleet management with insurance and maintenance tracking
- **Drivers**: Driver information with license management
- **Routes**: Route planning with AM/PM assignments
- **Students/Passengers**: Student transportation records
- **Activities**: System activity and audit logging
- **Maintenance**: Vehicle service records
- **Fuel**: Consumption tracking

## Usage

### Bus Management
1. Navigate to Bus Management from the dashboard
2. View all buses in the fleet
3. Add new buses with comprehensive information
4. Edit existing bus details
5. Track maintenance and insurance information

### Driver Management
1. Access Driver Management module
2. View all drivers with license status
3. Track license expiration dates (highlighted when approaching)
4. Manage driver assignments and contact information

### Route Management
1. Open Route Management from dashboard
2. View all routes with AM/PM assignments
3. Manage route details and descriptions
4. Track active/inactive routes

## Testing and Code Coverage

### Test Suite Overview
This project includes comprehensive testing with automated code coverage reporting:

- **Unit Tests**: Testing individual components and utilities
- **Integration Tests**: Testing database interactions and service integrations
- **Syncfusion Component Tests**: Specialized tests for UI components with freeze mitigation strategies

### Code Coverage
- **Target Coverage**: 75% for project, 75% for patches
- **Coverage Reports**: Automatically generated via Codecov on every PR and push
- **Coverage Badge**: ![Coverage](https://codecov.io/gh/Bigessfour/BusBuddy_Syncfusion/branch/master/graph/badge.svg)

### Running Tests Locally
```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test --filter "Category!=Integration"

# Run integration tests only
dotnet test --filter "Category=Integration"

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```


### Syncfusion WPF Component Testing
Special considerations for testing Syncfusion WPF UI components:
- **STA Threading**: WPF components require Single-Threaded Apartment model
- **UI Thread Marshaling**: Proper dispatcher usage for WPF UI testing
- **Resource Management**: Comprehensive cleanup of WPF resources and controls
- **Timeout Protection**: Prevents test freezing in WPF component initialization
- **Visual Tree Testing**: Testing WPF visual tree and data binding scenarios

## Syncfusion WPF Licensing Compliance

This application uses Syncfusion's professional WPF UI component suite and requires proper licensing:

- **License Registration**: Automatically handled at WPF application startup via environment variable or config file
- **No Legacy Code**: All legacy licensing code has been removed - modern registration only
- **WPF-Specific Components**: Uses only Syncfusion WPF controls (no WinForms or web components)
- **Compliance Documentation**: See [Syncfusion WPF Licensing Documentation](https://help.syncfusion.com/wpf/licensing/license-key)

### Syncfusion WPF Components Used
- **DockingManager**: Advanced docking and tabbed interfaces
- **DataGrid**: High-performance data grids with sorting/filtering
- **Charts**: Professional charting and visualization controls
- **Ribbon**: Modern ribbon interface components
- **Navigation**: TabControl, TreeView, and menu controls


For detailed testing documentation, see: [SYNCFUSION_TEST_FREEZE_MITIGATIONS.md](BusBuddy.Tests/SYNCFUSION_TEST_FREEZE_MITIGATIONS.md)

## Contributing
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License
This project is for educational and demonstration purposes.

## Support
For questions or support, please refer to the development documentation in `DEVELOPMENT_SUMMARY.md`.

---

**Status**: Active Development - Modern WPF application with core modules implemented. Additional Syncfusion WPF features and modules in progress. Fully modernized WPF architecture and compliant as of July 2025.
