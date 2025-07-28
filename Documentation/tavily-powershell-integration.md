# Integrating Tavily with BusBuddy: PowerShell Setup Guide

This guide provides instructions for integrating Tavily search capabilities into the BusBuddy PowerShell environment.

## Overview

Tavily integration enhances BusBuddy development with:
- Real-time search for documentation and code examples
- Focused Syncfusion control documentation access
- Quick answers to technical questions
- PowerShell-based search commands

## Files and Structure

The integration consists of:

1. **tavily-tool.ps1** - Core PowerShell script with Tavily API integration
2. **register-tavily-commands.ps1** - Script that registers BusBuddy commands (bb-*)
3. **tavily-expert-mcp-guide.md** - Guide for VS Code/GitHub Copilot integration

## Installation

### Step 1: Tavily API Key ✅

The Tavily API key has already been configured in your environment variables. No additional setup is required for the API key.

If you need to update or check your API key:

```powershell
# Check if API key is set
if ($env:TAVILY_API_KEY) {
    Write-Host "✅ Tavily API key is configured"
} else {
    Write-Host "❌ Tavily API key is not set"
}
```

### Step 2: Add to BusBuddy Profile

Add the Tavily commands to your BusBuddy environment by adding this line to `load-bus-buddy-profiles.ps1`:

```powershell
# Tavily Integration
. "$PSScriptRoot\Scripts\register-tavily-commands.ps1"
```

### Step 3: Verify Installation

Test the installation with:

```powershell
bb-tavily-setup
```

## Available Commands

After installation, these commands will be available:

| Command | Alias | Description |
|---------|-------|-------------|
| `bb-tavily <query>` | `bbt` | General web search |
| `bb-tavily-code <query>` | `bbtc` | Code-specific search |
| `bb-tavily-docs <query>` | `bbtd` | Documentation search |
| `bb-syncfusion <query>` | `bbsf` | Syncfusion-specific search |
| `bb-tavily-setup` | - | Initialize Tavily environment |

## Usage Examples

### General Search
```powershell
bb-tavily "bus route optimization algorithms"
```

### Code Search
```powershell
bb-tavily-code "Entity Framework Core migration best practices"
```

### Syncfusion Documentation
```powershell
bb-syncfusion "DockingManager layout management"
```

### Documentation Search
```powershell
bb-tavily-docs "PowerShell 7.5.2 threading"
```

## VS Code Integration

For VS Code integration with GitHub Copilot, see the separate guide:
[Tavily Expert MCP Guide](./tavily-expert-mcp-guide.md)

## Troubleshooting

### Verifying API Key Configuration
To confirm your Tavily API key is properly configured:
```powershell
# Verify API key is set
if ($env:TAVILY_API_KEY) {
    Write-Host "✅ Tavily API key is configured"
} else {
    Write-Host "❌ Tavily API key is not set"
}
```

### Command Not Found
If commands aren't found, ensure the registration script is properly sourced:
```powershell
. "c:\path\to\BusBuddy\Scripts\register-tavily-commands.ps1"
```

### Network Connectivity
For network issues, verify connectivity to Tavily API:
```powershell
Invoke-WebRequest -Uri "https://api.tavily.com/health" -Method GET
```

## Contributing

To enhance the Tavily integration:

1. Improve the search result formatting in `tavily-tool.ps1`
2. Add specialized search functions in `register-tavily-commands.ps1`
3. Update documentation with new examples

---

*Created for BusBuddy Project - © 2023 BusBuddy Development Team*
