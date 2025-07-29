# BusBuddy PowerShell Module

## Overview

The BusBuddy PowerShell Module provides a comprehensive set of commands and utilities for developing, building, and managing the BusBuddy application. This module is designed to streamline development workflows and enforce best practices.

## Module Structure

The module follows a modular, category-based approach for organization:

```
BusBuddy/
├── BusBuddy.psm1            # Main module file
├── BusBuddy.settings.ini    # Configurable settings
├── Functions/               # Organized function categories
│   ├── Build/               # Build and compilation functions
│   ├── Database/            # Database management functions
│   ├── Diagnostics/         # Diagnostics and analysis tools
│   ├── Development/         # Development workflow functions
│   ├── GitHub/              # GitHub integration functions
│   └── Utilities/           # General utility functions
```

## Key Features

- **Modular Design**: Functions are organized by category for easier maintenance
- **Configuration System**: Customizable settings in INI format
- **PowerShell 7.5 Optimized**: Takes advantage of the latest PowerShell features
- **Enhanced Logging**: Comprehensive error reporting and status updates
- **GitHub Integration**: Streamlined GitHub workflows and repository management
- **🚀 Phase 2 Ready**: Specialized commands for activity scheduling and route optimization
- **🤖 AI Integration**: MCP server status monitoring and AI workflow automation
- **🔍 Proactive Monitoring**: Zero-build-error philosophy with warning detection
- **😊 Developer Experience**: Motivational support and happiness boost functionality
- **Database Management**: Tools for managing database migrations and connections
- **Developer Experience**: Productivity commands and developer happiness features

## Quick Start

```powershell
# Start a development session
bb-dev-session

# Build the solution
bb-build

# Run the application
bb-run

# View all available commands
bb-commands
```

## Command Categories

- **Essential Commands**: `bb-build`, `bb-run`, `bb-test`, `bb-health`
- **Development Commands**: `bb-dev-session`, `bb-dev-workflow`, `bb-env-check`
- **Database Commands**: `bb-db-diag`, `bb-db-update`, `bb-db-seed`
- **GitHub Commands**: `bb-github-workflow`, `bb-git-check`, `bb-repo-align`
- **Utility Commands**: `bb-commands`, `bb-happiness`, `bb-info`

## Configuration

The module behavior can be customized through the `BusBuddy.settings.ini` file. Use the following commands to manage settings:

```powershell
# View current settings
Get-BusBuddySettings

# Change a setting
Set-BusBuddySettings -Section "Display" -Name "EnableEmoji" -Value $false -PersistChanges

# Export settings to a custom location
Get-BusBuddySettings -ExportToFile -ExportPath "C:\MySettings.ini"
```

## Extending the Module

To add new functions:

1. Create a new PS1 file in the appropriate category folder
2. Define your function using proper PowerShell 7.5 conventions
3. The module will automatically load and export your function

## Requirements

- PowerShell 7.5.2 or later
- .NET 8.0 or later
- Windows OS
