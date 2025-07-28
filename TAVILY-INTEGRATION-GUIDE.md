# 🤖 BusBuddy Tavily Expert Integration Guide

## Overview

The BusBuddy PowerShell module now includes comprehensive integration with Tavily Expert AI search, providing powerful web search capabilities directly within your development workflow.

## ✅ Setup Verification

Your environment is already configured with:
- ✅ Tavily Expert MCP server: `https://tavily.api.tadata.com/mcp/tavily/endoderm-victory-fling-azuqd2`
- ✅ API key: `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` (set in environment variables)
- ✅ VS Code MCP configuration: `.vscode/mcp.json` and `.vscode/settings.json`
- ✅ Node.js v20.17.0 and npm 10.8.2 installed

## 🚀 Quick Start

### 1. Load the BusBuddy Module
```powershell
# If not already loaded in your PowerShell profile
Import-Module ".\PowerShell\BusBuddy PowerShell Environment\Modules\BusBuddy\BusBuddy.psm1"
```

### 2. Test Integration
```powershell
# Run the integration test
.\test-tavily-integration.ps1 -TestMode Full

# Quick test
bb-tavily-search "PowerShell 7.5 new features"
```

## 🔍 Available Commands

### Core Search Commands
```powershell
# Primary search command
bb-tavily-search "WPF MVVM best practices"

# Shorter alias
bb-search "Entity Framework migrations"

# Advanced search with options
bb-tavily-search "Azure deployment" -Technology Azure -MaxResults 8 -IncludeDetails

# Save results to file
bb-tavily-search "Syncfusion DataGrid" -OutputFormat Markdown -SaveResults
```

### AI Workflow Commands
```powershell
# Start AI-enhanced workflows
bb-ai-workflow -WorkflowType Research -Technology WPF
bb-ai-workflow -WorkflowType Troubleshooting -Interactive
bb-ai-workflow -WorkflowType Learning -Technology PowerShell

# Get contextual assistance
bb-ai-help "MVVM data binding issues"
bb-ai-help -ErrorMessage "CS8600 nullable reference" -Technology DotNet

# Context-aware search based on project state
bb-context-search -AutoDetect -IncludeProjectAnalysis
```

### Learning and Documentation
```powershell
# AI learning mentor
bb-mentor -Topic PowerShell -IncludeExamples
bb-mentor -Topic WPF -OpenDocs

# Search official documentation
bb-docs -Technology "Entity Framework" -Query "migrations" -OpenInBrowser

# Quick reference
bb-ref -Technology PowerShell
```

## 🎯 Common Use Cases

### 1. Research New Technology
```powershell
# Research a new library or framework
bb-tavily-search "Syncfusion Blazor components overview"
bb-ai-workflow -WorkflowType Research -Technology Syncfusion -Interactive
```

### 2. Troubleshoot Build Errors
```powershell
# When you have a build error
bb-ai-help -ErrorMessage "MSB3027 file lock error"

# Automatic error analysis + search
bb-ai-workflow -WorkflowType Troubleshooting
```

### 3. Learn Best Practices
```powershell
# Learn about architectural patterns
bb-tavily-search "MVVM repository pattern Entity Framework" -IncludeDetails
bb-mentor -Topic MVVM -IncludeExamples
```

### 4. Stay Updated
```powershell
# Check latest developments
bb-tavily-search ".NET 8 performance improvements 2024"
bb-tavily-search "WPF modern UI frameworks 2024"
```

## 🔧 Advanced Configuration

### Output Formats
```powershell
# JSON format for programmatic use
$results = bb-tavily-search "query" -OutputFormat Json
$results.Results[0].Title

# Markdown for documentation
bb-tavily-search "query" -OutputFormat Markdown -SaveResults

# Default text format for console
bb-tavily-search "query"  # Default format
```

### Technology-Focused Searches
```powershell
# Focus search on specific technology stack
bb-tavily-search "async await patterns" -Technology DotNet
bb-tavily-search "databinding performance" -Technology WPF
bb-tavily-search "module organization" -Technology PowerShell
```

### Context Enhancement
```powershell
# Add BusBuddy-specific context
bb-tavily-search "transportation management system architecture" -Context "BusBuddy WPF application development"

# Project-aware searching
bb-context-search -AutoDetect  # Analyzes current project state and suggests searches
```

## 📊 Integration with Existing Workflows

### Combine with BusBuddy Commands
```powershell
# Research while developing
bb-build
bb-tavily-search "build optimization techniques .NET 8"

# Error analysis with AI assistance
bb-error-fix
bb-ai-help -ErrorMessage "detected error from build"

# Health check with learning
bb-health
bb-mentor -Topic "development environment setup"
```

### Development Session Enhancement
```powershell
# Enhanced development session with AI
bb-dev-session
bb-ai-workflow -WorkflowType Planning -Interactive
bb-tavily-search "Phase 2 feature roadmap best practices"
```

## 🛠️ Troubleshooting

### Common Issues

1. **API Key Issues**
   ```powershell
   # Verify API key
   $env:TAVILY_API_KEY
   
   # Reset if needed
   $env:TAVILY_API_KEY = "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"
   ```

2. **Function Not Found**
   ```powershell
   # Reload module
   Import-Module ".\PowerShell\BusBuddy PowerShell Environment\Modules\BusBuddy\BusBuddy.psm1" -Force
   
   # Check available commands
   bb-commands -Category AI
   ```

3. **Network Issues**
   ```powershell
   # Test basic connectivity
   Test-NetConnection api.tavily.com -Port 443
   
   # Run integration test
   .\test-tavily-integration.ps1 -TestMode Basic
   ```

### Verification Commands
```powershell
# Full integration test
.\test-tavily-integration.ps1 -TestMode Full

# Quick verification
bb-tavily-search "test query" -MaxResults 1

# Check all AI commands
bb-commands -Category AI -ShowAliases
```

## 🎉 Examples for BusBuddy Development

### Phase 2 Planning Research
```powershell
bb-ai-workflow -WorkflowType Planning
bb-tavily-search "transportation management system Phase 2 features"
bb-tavily-search "WPF dashboard advanced analytics implementation"
```

### Technical Deep Dives
```powershell
bb-tavily-search "Entity Framework performance optimization large datasets"
bb-tavily-search "Syncfusion Charts real-time data binding WPF"
bb-tavily-search "PowerShell 7.5 Azure integration patterns"
```

### Problem Solving
```powershell
bb-ai-help -ErrorMessage "Syncfusion license validation failed"
bb-tavily-search "WPF memory leak debugging tools" -IncludeDetails
```

## 🔮 Future Enhancements

The integration is designed to evolve with the project:
- Automatic error research during builds
- Context-aware suggestions based on git activity  
- Integration with GitHub Issues and Pull Requests
- Project-specific knowledge base building

## 📚 Learn More

- Run `bb-mentor` for interactive learning assistance
- Use `bb-commands -Category AI` to see all available AI commands
- Try `bb-ai-workflow -WorkflowType Learning -Interactive` for guided learning sessions

---
*This integration transforms your BusBuddy development experience with AI-powered research capabilities!* 🚌✨
