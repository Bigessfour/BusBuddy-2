# Bus Buddy - Transportation Management System

[![CI/CD Pipeline](https://github.com/Bigessfour/BusBuddy_Syncfusion/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/Bigessfour/BusBuddy_Syncfusion/actions/workflows/ci-cd.yml)
[![codecov](https://codecov.io/gh/Bigessfour/BusBuddy_Syncfusion/branch/master/graph/badge.svg)](https://codecov.io/gh/Bigessfour/BusBuddy_Syncfusion)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Syncfusion](https://img.shields.io/badge/Syncfusion-30.1.37-orange.svg)](https://www.syncfusion.com/)

A comprehensive school bus transportation management system built with C# .NET 8, Windows Forms, and Syncfusion UI components.

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

### Core Features
- **Professional UI**: Modern Syncfusion Windows Forms interface
- **Database Integration**: Entity Framework Core with SQL Server
- **Dependency Injection**: Clean architecture with service containers
- **Comprehensive Logging**: Structured logging throughout the application
- **Data Validation**: Form validation with user-friendly error messages
- **Exception Handling**: Robust error handling with graceful degradation

## Technology Stack
- **.NET 8** - Windows Forms Application
- **Entity Framework Core** - Database ORM
- **SQL Server** - Database
- **Syncfusion Windows Forms** - UI Components
- **Microsoft Extensions** - Logging, DI, Configuration

## Getting Started

### Prerequisites
- Visual Studio 2022 or VS Code
- .NET 8 SDK
- SQL Server (LocalDB or full instance)
- Syncfusion License (for UI components)

### Installation
1. Clone the repository
2. Configure the connection string in `appsettings.json`
3. Add your Syncfusion license key to configuration
4. Run database migrations:
   ```bash
   dotnet ef database update
   ```
5. Build and run the application:
   ```bash
   dotnet run
   ```

## Architecture

### Project Structure
```
Bus Buddy/
├── Data/              # Entity Framework DbContext
├── Forms/             # Windows Forms UI
├── Models/            # Entity Framework Models
├── Services/          # Business Logic Layer
├── Migrations/        # EF Database Migrations
└── Configuration/     # App settings and configuration
```

### Key Design Patterns
- **Repository Pattern**: Service layer abstraction
- **Dependency Injection**: Clean separation of concerns
- **Model-View separation**: Clear UI and business logic separation
- **Factory Pattern**: Service container management

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

### Syncfusion Test Mitigations
Special considerations for Syncfusion Windows Forms testing:
- STA apartment threading for UI components
- Proper resource disposal with comprehensive cleanup
- Timeout protection to prevent test freezing
- Enhanced logging for debugging test issues

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

**Status**: Active Development - Core modules implemented, additional features in progress.
