# BusBuddy Scripts Organization

This directory contains all PowerShell scripts organized by purpose for better maintainability.

## Directory Structure

### `/Build/`
Scripts related to building, compilation, and deployment:
- `build-busbuddy-simple.ps1` - Simple build script for development
- `clean-and-restore.ps1` - Clean and restore project dependencies
- `deploy-azure-sql.ps1` - Deploy database to Azure SQL
- `dotnet-install.ps1` - Install .NET runtime and SDK

### `/Configuration/`
Scripts for configuring the development environment:
- `configure-tavily-mcp.ps1` - Configure Tavily MCP integration
- `fix-dotnet-installation.ps1` - Fix .NET installation issues
- `fix-mcp-configuration.ps1` - Fix MCP configuration issues
- `switch-database-provider.ps1` - Switch between database providers

### `/Setup/`
Scripts for initial environment setup:
- `setup-localdb.ps1` - Set up LocalDB for development
- `setup-nodejs-and-mcp.ps1` - Set up Node.js and MCP environment
- `setup-tavily-key.ps1` - Configure Tavily API key
- `setup-working-mcp.ps1` - Set up working MCP environment

### `/Testing/`
Scripts for testing and validation:
- `test-tavily-integration.ps1` - Test Tavily integration
- `validate-powershell-comprehensive.ps1` - Comprehensive PowerShell validation

### `/Utilities/`
General utility scripts:
- `install-nodejs-mcp-admin.ps1` - Install Node.js MCP admin tools
- `remove-empty-files.ps1` - Clean up empty files

### `/Maintenance/`
Scripts for system maintenance and optimization:
- Various maintenance and optimization scripts

## Usage

All scripts should be run from the project root directory. Update your commands to use the new paths:

```powershell
# Old way
.\setup-localdb.ps1

# New way
.\Scripts\Setup\setup-localdb.ps1
```

## BusBuddy PowerShell Module

For daily development, use the BusBuddy PowerShell module commands instead of calling scripts directly:

```powershell
bb-build      # Build the solution
bb-health     # Check project health
bb-test       # Run tests
bb-run        # Start the application
```

The PowerShell module is located in `PowerShell\BusBuddy PowerShell Environment\Modules\BusBuddy\`.
