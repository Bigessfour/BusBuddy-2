# BusBuddy

This is the main repository for the BusBuddy project, a school transportation management system built with WPF and .NET 8.

## Recent Updates (August 2, 2025)

✅ **Critical Build Fixes Completed**
- Fixed CS1022 syntax errors in GoogleEarthViewModel.cs
- Implemented missing IGeoDataService interface with dependency injection
- Resolved compilation blocking issues 
- Enhanced Google Earth integration infrastructure

## Getting Started

1.  Ensure you have the .NET 8 SDK installed.
2.  Run `dotnet restore` to install dependencies.
3.  Use the provided PowerShell scripts in the `PowerShell` directory for development tasks.

## Building and Running

-   **Build:** `dotnet build BusBuddy.sln`
-   **Run:** `dotnet run --project BusBuddy.WPF/BusBuddy.WPF.csproj`

For more advanced workflows, use the `bb-*` commands available in the PowerShell module.

## Architecture

### Project Structure
- **BusBuddy.Core**: Business logic, data models, and services
- **BusBuddy.WPF**: WPF presentation layer with MVVM pattern
- **BusBuddy.Tests**: Comprehensive test suite

### Key Technologies
- **.NET 8**: Target framework
- **WPF**: Desktop application framework
- **Syncfusion WPF 30.1.42**: Enhanced UI controls
- **Entity Framework Core**: Data access layer
- **Serilog**: Structured logging
- **PowerShell 7.5.2**: Development automation

### Services
- **IGeoDataService**: Google Earth integration for route visualization
- **Phase1DataSeedingService**: Development data seeding
- **Comprehensive logging**: Structured logging with Serilog

## Development Workflow

Use the built-in PowerShell commands for efficient development:

```powershell
bb-health          # Project health check
bb-build           # Build solution
bb-run             # Run application
bb-test            # Run tests
bb-dev-session     # Complete development session
```

## Google Earth Integration

The project includes Google Earth Engine integration for route visualization:
- Geographic data visualization
- Route mapping and analysis
- Real-time map updates

## Contributing

Please follow the established coding standards and use the PowerShell development tools for consistency.

## License

[Add license information here]
