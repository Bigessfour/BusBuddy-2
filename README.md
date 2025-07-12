# Bus Buddy - Transportation Management System

**Current State:**
- **100% Pure WPF Application** - Built exclusively with WPF (Windows Presentation Foundation) using Syncfusion's professional WPF UI component suite
- **Zero Legacy Dependencies** - Completely modernized architecture with no Windows Forms, WinForms, or legacy UI frameworks
- **.NET 8 (net8.0-windows)** - Latest .NET framework with cutting-edge WPF architecture patterns and performance optimizations
- **Premium Syncfusion WPF Components** - Professional-grade UI controls including DataGrid, DockingManager, Charts, Ribbon, and Office2019Colorful theming
- **Advanced MVVM Architecture** - Follows WPF best practices with ViewModels, Data Binding, Command patterns, and INotifyPropertyChanged implementations
- **Modern WPF Theming** - Syncfusion Office2019Colorful theme applied consistently across all UI components
- **Syncfusion Licensing** - Properly handled via environment variable (`SYNCFUSION_LICENSE_KEY`) or `appsettings.json` fallback
- **Enterprise-Ready** - Production-ready codebase with comprehensive error handling, WPF-specific logging, and extensive testing

[![CI/CD Pipeline](https://github.com/Bigessfour/BusBuddy_Syncfusion/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/Bigessfour/BusBuddy_Syncfusion/actions/workflows/ci-cd.yml)
[![codecov](https://codecov.io/gh/Bigessfour/BusBuddy_Syncfusion/branch/master/graph/badge.svg)](https://codecov.io/gh/Bigessfour/BusBuddy_Syncfusion)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Syncfusion](https://img.shields.io/badge/Syncfusion-30.1.37-orange.svg)](https://www.syncfusion.com/)

A comprehensive school bus transportation management system built with **C# .NET 8 WPF** and **Syncfusion's professional WPF UI component suite**. This modern, pure WPF application leverages the latest in Microsoft's presentation framework technology to deliver a rich, responsive desktop experience with zero dependencies on legacy UI frameworks.

## Why Pure WPF (Windows Presentation Foundation)?

This project is built **exclusively with WPF** to provide:
- **Rich Desktop Experience**: Native Windows application with hardware-accelerated graphics and smooth animations
- **Professional UI Components**: Leverages Syncfusion's mature WPF control suite with Office2019Colorful theming
- **Powerful Data Binding**: Declarative XAML binding for real-time UI synchronization and responsive interfaces
- **Pure MVVM Architecture**: Industry-standard pattern for maintainable desktop applications without legacy UI dependencies
- **Scalable Performance**: Optimized for handling large datasets in transportation management with WPF virtualization
- **Enterprise Features**: Advanced WPF controls like DockingManager, DataGrid, Charts, and Ribbon interfaces
- **Modern .NET Integration**: Full compatibility with .NET 8 and latest C# features in a pure WPF environment
- **No Legacy Baggage**: Zero Windows Forms, WinForms, or other deprecated UI framework dependencies

## Key WPF Architecture Benefits
- **Pure WPF Desktop UI**: Native Windows desktop application with hardware acceleration and modern theming
- **Advanced Data Binding**: Powerful XAML data binding with INotifyPropertyChanged for real-time UI updates
- **Modern MVVM Pattern**: Clean separation of concerns with Model-View-ViewModel architecture (no code-behind dependencies)
- **Premium Syncfusion WPF Components**: Professional UI controls optimized specifically for WPF desktop performance
- **Responsive WPF Design**: Adaptive layouts using WPF's flexible layout system for different screen resolutions
- **Native Windows Performance**: Direct Windows API integration through WPF for optimal performance
- **Office2019Colorful Theming**: Consistent, professional appearance across all Syncfusion WPF controls
- **Zero Legacy Dependencies**: No Windows Forms, WinForms, or deprecated UI framework references

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
- **Modern Syncfusion WPF Interface**: Professional desktop UI with rich controls and Office2019Colorful theming
- **Pure MVVM Architecture**: Clean separation using ViewModels, Commands, and Data Binding (zero code-behind complexity)
- **Advanced Database Integration**: Entity Framework Core with SQL Server for robust data management via WPF data binding
- **Modern Dependency Injection**: .NET 8 dependency injection container for clean architecture without legacy dependencies
- **Real-time WPF Data Binding**: Live UI updates through INotifyPropertyChanged and ObservableCollection implementations
- **Comprehensive WPF Logging**: Structured logging integrated throughout the pure WPF application architecture
- **Rich WPF Form Validation**: Advanced validation with immediate visual feedback using WPF validation frameworks
- **WPF-Native Exception Handling**: WPF-specific error handling with professional dialog presentation and user experience
- **No Legacy UI Dependencies**: Completely free of Windows Forms, WinForms, or other deprecated UI frameworks

## Technology Stack
- **.NET 8 Pure WPF Application** - Modern Windows Presentation Foundation desktop application (zero legacy UI dependencies)
- **Syncfusion WPF Premium Components** - Professional-grade UI controls (DataGrid, DockingManager, Charts, Ribbon) with Office2019Colorful theming
- **Entity Framework Core 8** - Latest object-relational mapping for data access with WPF data binding support
- **SQL Server** - Enterprise database backend with WPF-optimized data access patterns
- **Advanced MVVM Pattern** - Model-View-ViewModel architecture pattern specifically for WPF with INotifyPropertyChanged
- **Microsoft Extensions** - Logging, Dependency Injection, Configuration management optimized for WPF applications
- **XAML** - Declarative markup for rich WPF user interfaces with professional styling and theming
- **Modern C# 12/.NET 8 Features** - Latest language features integrated with WPF development patterns

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

- **Modern License Registration**: Automatically handled at WPF application startup via environment variable or config file
- **Legacy-Free Implementation**: All deprecated licensing code has been removed - modern WPF registration only
- **Pure WPF Components**: Uses exclusively Syncfusion WPF controls (zero Windows Forms, WinForms, or web components)
- **Office2019Colorful Theming**: Professional theming applied consistently across all components
- **WPF Compliance Documentation**: See [Syncfusion WPF Licensing Documentation](https://help.syncfusion.com/wpf/licensing/license-key)

### Syncfusion WPF Components Used
- **DockingManager**: Advanced docking and tabbed interfaces for professional desktop applications
- **DataGrid**: High-performance data grids with sorting/filtering optimized for large datasets
- **Charts**: Professional charting and visualization controls with WPF hardware acceleration
- **Ribbon**: Modern ribbon interface components following Office design patterns
- **Navigation**: TabControl, TreeView, and menu controls with Office2019Colorful theming
- **Office2019Colorful Theme**: Consistent professional appearance across all Syncfusion WPF controls
- **SfSkinManager**: Advanced theming system for unified visual experience


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

**Status**: Active Development - Pure WPF application with core modules implemented. Additional Syncfusion WPF features and modules in progress. Fully modernized WPF architecture with zero legacy dependencies as of July 2025.

**Architecture Commitment**: This project maintains a 100% pure WPF architecture with no Windows Forms, WinForms, or legacy UI framework dependencies. All UI components use Syncfusion's professional WPF controls with Office2019Colorful theming for a consistent, modern desktop experience.
