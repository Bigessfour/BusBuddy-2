# Bus Buddy PowerShell Scripts

This directory contains all PowerShell scripts for the Bus Buddy project, organized by purpose.

## Directory Structure

### 📁 Build/
Scripts related to building and compiling the project:
- `bb-build-task.ps1` - Build task automation

### 📁 Development/
Core development scripts and profiles:
- `BusBuddy-PowerShell-Profile.ps1` - Main PowerShell profile with Bus Buddy commands
- `BusBuddy-PowerShell-Profile-vscode.ps1` - VS Code specific profile variant
- `BusBuddy-Advanced-Workflows.ps1` - Advanced workflow automation
- `BusBuddy-Advanced-Workflows-vscode.ps1` - VS Code specific workflow variant

### 📁 GitHub/
GitHub integration and automation scripts:
- `BusBuddy-GitHub-Automation.ps1` - GitHub automation workflows
- `GitHub-Actions-Monitor.ps1` - Monitor and analyze GitHub Actions

### 📁 Monitoring/
System and application monitoring scripts:
- `health-check.ps1` - System health checks
- `serilog-monitor.ps1` - Serilog monitoring
- `Watch-BusBuddyLogs.ps1` - Real-time log monitoring

### 📁 Setup/
Environment setup and configuration scripts:
- `check-powershell-extension.ps1` - Check PowerShell extension status
- `fix-powershell-extension.ps1` - Fix PowerShell extension issues
- `force-powershell-extension.ps1` - Force PowerShell extension installation

### 📁 Validation/
Code quality and validation scripts:
- `validate-csharp.ps1` - C# code validation
- `validate-syncfusion.ps1` - Syncfusion component validation
- `validate-xaml.ps1` - XAML validation
- `test-environment.ps1` - Environment testing
- `Syncfusion-Implementation-Validator.ps1` - Comprehensive Syncfusion validation
- `XAML-Binding-Health-Monitor.ps1` - XAML binding health monitoring
- `XAML-Health-Suite.ps1` - Complete XAML health analysis
- `XAML-Null-Safety-Analyzer.ps1` - XAML null safety analysis
- `XAML-Performance-Analyzer.ps1` - XAML performance analysis
- `XAML-Type-Safety-Analyzer.ps1` - XAML type safety analysis

### 📁 (Root)/
General utility scripts:
- `bb-theme-check.ps1` - Theme checking utility

## Quick Commands

If you have the PowerShell profile loaded, you can use these quick commands:

```powershell
# Load the Bus Buddy profile
. ".\Tools\Scripts\Development\BusBuddy-PowerShell-Profile.ps1"

# Common commands (available after loading profile)
bb-health        # Run health checks
bb-build         # Build the project
bb-run           # Run the application
bb-test          # Run tests
bb-clean         # Clean build artifacts
```

## Usage Notes

1. **Profile Loading**: Always load the PowerShell profile first for the best experience
2. **Script Execution**: Ensure your execution policy allows running scripts
3. **Dependencies**: Some scripts may require specific modules or tools to be installed
4. **Paths**: Scripts are designed to work from the project root directory

## Script Organization Guidelines

When adding new scripts:
- **Build**: Scripts that compile, package, or prepare builds
- **Development**: Core development tools and profiles
- **GitHub**: Git and GitHub integration scripts
- **Monitoring**: Runtime monitoring and logging scripts
- **Setup**: Environment setup and configuration
- **Validation**: Code quality, testing, and validation scripts
