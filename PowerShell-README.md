# BusBuddy PowerShell Scripts

This directory contains essential PowerShell scripts for the BusBuddy project.

## Root PowerShell Scripts

### `load-bus-buddy-profiles.ps1`
- **Purpose**: Main entry point for PowerShell environment setup
- **Description**: Loads the comprehensive BusBuddy PowerShell module (5,434 lines) and associated AI integration modules
- **Usage**: `.\load-bus-buddy-profiles.ps1` or `.\load-bus-buddy-profiles.ps1 -Quiet`
- **Note**: This script must remain in the root directory as it's referenced in many other scripts and documentation

### `build-busbuddy-simple.ps1`
- **Purpose**: Simple build script for BusBuddy with error handling
- **Description**: Builds the BusBuddy solution, ensuring the core DLL is available
- **Usage**: `.\build-busbuddy-simple.ps1 -Verbose` or `.\build-busbuddy-simple.ps1 -SkipProfiles`
- **Note**: This is a simplified entry point for building the project

### `run-with-error-capture.ps1`
- **Purpose**: Complete build and error capture workflow
- **Description**: Builds the solution and then runs the error capture system
- **Usage**: `.\run-with-error-capture.ps1` or `.\run-with-error-capture.ps1 -AutoFix`
- **Note**: Specialized workflow combining build and error handling

## PowerShell Directory Structure

For more comprehensive PowerShell scripts and modules, see the `PowerShell` directory which contains:

- `PowerShell/BusBuddy PowerShell Environment/Modules/BusBuddy/BusBuddy.psm1`: Main PowerShell module (5,434 lines)
- `PowerShell/Scripts/Build/`: Build scripts and utilities
- `PowerShell/Scripts/Configuration/`: Configuration scripts
- `PowerShell/Scripts/Maintenance/`: System maintenance tools
- `PowerShell/Scripts/Testing/`: Test automation scripts
- `PowerShell/Scripts/Utilities/`: General utility scripts

## Guidelines

- Root scripts are kept minimal for ease of access
- More specialized scripts are organized in the PowerShell directory structure
- Always run `load-bus-buddy-profiles.ps1` first to ensure the PowerShell environment is properly configured
