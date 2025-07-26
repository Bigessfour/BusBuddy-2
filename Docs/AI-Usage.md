# BusBuddy AI Usage Guide

> 🤖 **Quick Reference**: Essential AI commands for BusBuddy development workflows

## Overview

BusBuddy integrates with AI services (including xAI Grok-4) to enhance development productivity through intelligent automation, code assistance, and workflow optimization.

## Quick Command Reference

| Command | Description | Example |
|---------|-------------|---------|
| `bb-ai-chat` | General AI chat interface | `bb-ai-chat -Prompt "How do I implement MVVM in WPF?"` |
| `bb-ai-config` | Configure AI services | `bb-ai-config -Provider "xAI" -ValidateConnection` |
| `bb-ai-task` | AI-powered task automation | `bb-ai-task -TaskType "CodeGeneration" -Context "Create ViewModel"` |
| `bb-ai-route` | AI route optimization | `bb-ai-route -OptimizeFor "Performance" -AnalyzeCurrent` |
| `bb-ai-review` | AI code review | `bb-ai-review -FilePath ".\ViewModels\*.cs" -Focus "MVVM"` |

## Core AI Functions

### Configuration Management
```powershell
# Configure xAI Grok-4 integration
bb-ai-config -Provider "xAI" -ValidateConnection

# View current AI configuration
bb-ai-config -ShowCurrent

# Test AI service connectivity
bb-ai-config -TestConnection
```

### AI Chat Interface
```powershell
# Interactive AI assistance
bb-ai-chat -Prompt "Hello, help me with BusBuddy development"

# Context-aware development questions
bb-ai-chat -Prompt "How do I implement data binding in WPF?" -Context "BusBuddy"

# Code-specific assistance
bb-ai-chat -Prompt "Review this XAML code" -AttachFile "MainWindow.xaml"
```

### Task Automation
```powershell
# Generate ViewModels with AI assistance
bb-ai-task -TaskType "CodeGeneration" -Context "ViewModel for Driver management"

# Automated code refactoring suggestions
bb-ai-task -TaskType "Refactoring" -TargetFiles "*.cs" -Focus "Performance"

# Generate documentation
bb-ai-task -TaskType "Documentation" -Context "API documentation for BusBuddy.Core"
```

### Route Optimization
```powershell
# Analyze current bus routes
bb-ai-route -AnalyzeCurrent -OptimizeFor "Efficiency"

# Generate route suggestions
bb-ai-route -CreateSuggestions -BasedOn "Historical data"

# Performance analysis
bb-ai-route -OptimizeFor "Performance" -IncludeMetrics
```

### Code Review
```powershell
# AI-powered code review
bb-ai-review -FilePath ".\ViewModels\BusManagementViewModel.cs"

# Focus on specific aspects
bb-ai-review -FilePath "*.cs" -Focus "Security,Performance"

# Generate review report
bb-ai-review -GenerateReport -OutputPath ".\Reports\CodeReview.md"
```

## AI Service Providers

### xAI Grok-4 Integration

BusBuddy includes optimized integration with xAI's Grok-4 model for:

- **Code Analysis**: Advanced understanding of C# and WPF patterns
- **Architecture Review**: MVVM best practices and design pattern validation
- **Performance Optimization**: Intelligent suggestions for WPF application performance
- **Documentation Generation**: Context-aware documentation creation

#### Configuration Example
```powershell
# Set up xAI Grok-4
bb-ai-config -Provider "xAI" -ApiKey $env:XAI_API_KEY -Model "grok-4"

# Validate connection
bb-ai-config -TestConnection -Verbose
```

### Integration Tips

1. **Environment Variables**: Store API keys securely in environment variables
   ```powershell
   $env:XAI_API_KEY = "your-api-key-here"
   $env:OPENAI_API_KEY = "your-openai-key"  # If using OpenAI as fallback
   ```

2. **Context Awareness**: AI functions automatically detect BusBuddy project context
   ```powershell
   # Automatically includes project structure in prompts
   bb-ai-chat -Prompt "Help optimize this WPF application"
   ```

3. **Batch Operations**: Use AI for bulk code analysis
   ```powershell
   # Review all ViewModels
   Get-ChildItem -Path ".\ViewModels\*.cs" | ForEach-Object { bb-ai-review -FilePath $_.FullName }
   ```

## Advanced Usage

### Custom AI Workflows
```powershell
# Complete development workflow with AI assistance
function Start-AIAssistedDevelopment {
    bb-ai-config -ValidateConnection
    bb-ai-task -TaskType "ProjectAnalysis" -GenerateReport
    bb-ai-review -FilePath ".\**\*.cs" -Focus "Architecture"
    bb-ai-chat -Prompt "Summarize code quality and suggest improvements"
}
```

### Integration with BusBuddy Workflows
```powershell
# Enhanced build process with AI review
bb-build -Clean
bb-ai-review -FilePath ".\**\*.cs" -QuickCheck
bb-run
```

## Troubleshooting

### Common Issues

1. **API Key Not Set**
   ```
   Error: AI service not configured
   Solution: bb-ai-config -Provider "xAI" -SetupWizard
   ```

2. **Connection Timeout**
   ```
   Error: AI service timeout
   Solution: bb-ai-config -TestConnection -Timeout 30
   ```

3. **Rate Limiting**
   ```
   Error: Rate limit exceeded
   Solution: Use -Delay parameter or check provider limits
   ```

### Best Practices

- **Batch Processing**: Group AI requests to minimize API calls
- **Context Management**: Provide clear, specific prompts for better results
- **Error Handling**: Always validate AI service connectivity before batch operations
- **Security**: Never include sensitive data in AI prompts

## Examples and Use Cases

### Daily Development Workflow
```powershell
# Morning development session
bb-ai-config -ValidateConnection
bb-ai-chat -Prompt "What should I work on today?" -Context "BusBuddy project status"

# During development
bb-ai-review -FilePath $currentFile -Focus "BestPractices"
bb-ai-task -TaskType "UnitTest" -Context "Generate tests for new ViewModel"

# End of day
bb-ai-task -TaskType "Documentation" -Context "Update README with today's changes"
```

### Code Quality Assurance
```powershell
# Pre-commit AI review
bb-ai-review -FilePath (git diff --name-only --cached) -Focus "Security,Performance"
bb-ai-chat -Prompt "Analyze git diff for potential issues" -AttachDiff
```

## Getting Help

- Use `Get-Help bb-ai-*` for detailed command documentation
- Run `bb-ai-config -ShowExamples` for provider-specific examples
- Check `bb-health` for AI service status in overall health report

---

> 💡 **Pro Tip**: Combine AI functions with PowerShell pipelines for powerful automation workflows
> 
> 🚌 **BusBuddy Philosophy**: AI amplifies human creativity — it doesn't replace developer insight!
