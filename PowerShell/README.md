# 🚌 Bus Buddy PowerShell Module - PowerShell 7.5 Optimized

A professional PowerShell module optimized for PowerShell 7.5+ and Bus Buddy WPF development environment automation.

## ✨ Features

### 🚀 PowerShell 7.5 Optimizations
- **Enhanced Performance** - Leverages PowerShell 7.5 array `+=` optimizations (98% improvement)
- **Advanced JSON Handling** - Enhanced `Test-Json` with comments and trailing comma support
- **Structured Error Reporting** - PowerShell 7.5 enhanced error details with recommended actions
- **CLI XML Support** - Uses new `ConvertTo-CliXml` and `ConvertFrom-CliXml` cmdlets
- **Feature Detection** - Automatic PowerShell 7.5 feature availability testing

### 🔨 Build & Development Automation
- **bb-build** - Build the Bus Buddy solution with advanced options
- **bb-run** - Run the WPF application with debugging support
- **bb-test** - Execute test suite with coverage options
- **bb-clean** - Clean build artifacts and caches
- **bb-restore** - Restore NuGet packages with force options

### 🚀 Development Session Management
- **bb-dev-session** - Start comprehensive development session
- **bb-health** - Project health check and validation
- **bb-env-check** - Environment and dependency validation

### 😊 Developer Experience
- **bb-happiness** - Sarcastic motivational quotes for developers
- **bb-commands** - Discover all available commands
- **bb-info** - Module information and status
- **Test-PowerShell75Features** - Comprehensive PowerShell 7.5 feature testing

## 🚀 Quick Start

### Requirements
- **PowerShell 7.5+** - Required for optimal performance and features
- **.NET 9.0+** - Recommended for best PowerShell 7.5 experience
- **Bus Buddy Project** - Must be run from Bus Buddy project directory

### Installation
```powershell
# Navigate to Bus Buddy project root
cd "path\to\BusBuddy"

# Import the module
Import-Module .\PowerShell\BusBuddy.psm1

# Start development session
bb-dev-session
```

### Essential Commands
```powershell
# Build and run the application
bb-build -Clean -Restore
bb-run

# Get some motivation
bb-happiness

# See all available commands
bb-commands
```

## � PowerShell 7.5 Enhanced Commands

### `Test-PowerShell75Features`
Comprehensive testing of PowerShell 7.5 features and compatibility.

```powershell
Test-PowerShell75Features                 # Basic feature check
Test-PowerShell75Features -ShowBenchmarks # Include performance benchmarks
```

### Enhanced Error Handling
All commands now use PowerShell 7.5 structured error reporting with recommended actions.

```powershell
# Errors now include specific remediation steps
bb-build  # If it fails, you'll get actionable recommendations
```

### Advanced Configuration Validation
Enhanced JSON configuration validation with PowerShell 7.5 features.

```powershell
# Test configuration files with comment support
Test-BusBuddyConfiguration -ConfigPath "appsettings.json" -AllowComments -AllowTrailingCommas
```

## �📚 Command Reference

### Core Development Commands

#### `bb-build` (Invoke-BusBuddyBuild)
Build the Bus Buddy solution with enhanced options.

```powershell
bb-build                          # Standard build
bb-build -Clean                   # Clean before building
bb-build -Configuration Release   # Release build
bb-build -Restore                 # Restore packages first
bb-build -Verbosity detailed      # Detailed output
```

#### `bb-run` (Invoke-BusBuddyRun)
Run the Bus Buddy WPF application.

```powershell
bb-run                            # Build and run
bb-run -NoBuild                   # Skip build, run directly
bb-run -EnableDebug               # Enable debug logging
bb-run -Configuration Release     # Run release build
```

#### `bb-test` (Invoke-BusBuddyTest)
Execute the test suite.

```powershell
bb-test                           # Run all tests
bb-test -Filter "UnitTests"       # Filter specific tests
bb-test -Coverage                 # Include code coverage
bb-test -Configuration Release    # Test release build
```

### Advanced Commands

#### `bb-dev-session` (Start-BusBuddyDevSession)
Start a comprehensive development session.

```powershell
bb-dev-session                    # Full session setup
bb-dev-session -SkipBuild         # Skip initial build
bb-dev-session -OpenIDE           # Open VS Code
```

#### `bb-health` (Invoke-BusBuddyHealthCheck)
Perform project health check.

```powershell
bb-health                         # Standard health check
bb-health -Quick                  # Quick validation
bb-health -Detailed               # Detailed analysis
```

### Utility Commands

#### `bb-happiness` (Get-BusBuddyHappiness)
Get motivational developer quotes.

```powershell
bb-happiness                      # Single random quote
bb-happiness -Count 3             # Multiple quotes
bb-happiness -All                 # Show all quotes
```

#### `bb-commands` (Get-BusBuddyCommands)
List available commands.

```powershell
bb-commands                       # All commands
bb-commands -Category Essential   # Essential commands only
bb-commands -ShowAliases          # Include function names
```

## 🎯 Sample Quotes

The `bb-happiness` command provides sarcastic but motivating quotes:

- *"You're doing great... or at least better than that bus that's always late."*
- *"Your code compiles! That puts you ahead of 73% of developers today."*
- *"Transportation fact: Your debugging skills are faster than city traffic."*
- *"Keep going! You're more reliable than weekend bus schedules."*

## 🛠 Requirements

- **PowerShell 7.5+** - Required for enhanced performance and new features
- **.NET 9.0+** - Recommended for optimal PowerShell 7.5 experience
- **Bus Buddy Project** - Must be run from Bus Buddy project directory

### Feature Compatibility
- **PowerShell 7.5+**: Full feature support with performance optimizations
- **PowerShell 7.0-7.4**: Basic functionality (some features may be unavailable)
- **Windows PowerShell 5.1**: Not supported (Core edition required)

## 📁 Module Structure

```
PowerShell/
├── BusBuddy.psm1      # Main module file
├── BusBuddy.psd1      # Module manifest
└── README.md          # This documentation
```

## 🔧 Environment Validation

The module automatically validates:
- ✅ PowerShell version (7.0+ required)
- ✅ .NET SDK version (8.0+ required)
- ✅ Project structure and critical files
- ✅ Configuration file validity
- ✅ Build system functionality

## 🎨 Professional Features

### Industry Standards
- **Proper Module Structure** - Follows PowerShell module best practices
- **Function Exports** - Clean public API with explicit exports
- **Parameter Validation** - Comprehensive input validation
- **Error Handling** - Robust error handling and reporting
- **Documentation** - Complete help documentation for all functions

### Developer Experience
- **Colored Output** - Status indicators with appropriate colors
- **Progress Feedback** - Real-time feedback for long operations
- **Context Awareness** - Auto-detects project root and configuration
- **Alias Support** - Short, memorable command aliases

### Quality Assurance
- **Environment Checks** - Validates development environment
- **Build Validation** - Ensures successful compilation
- **Health Monitoring** - Comprehensive project health analysis
- **Error Recovery** - Graceful handling of common issues

## 🚀 Advanced Usage

### Custom Build Configurations
```powershell
# Development build with full restore
bb-build -Configuration Debug -Clean -Restore -Verbosity detailed

# Production build
bb-build -Configuration Release -NoLogo
```

### Testing Workflows
```powershell
# Quick test cycle
bb-test -Filter "UnitTests" -Logger "console;verbosity=minimal"

# Full test suite with coverage
bb-test -Coverage -Configuration Release
```

### Development Sessions
```powershell
# Complete development setup
bb-dev-session -OpenIDE

# Quick validation workflow
bb-env-check
bb-health -Quick
bb-build -Clean
```

## 🎯 Integration with Existing Workflow

This module organizes and enhances your existing PowerShell scripts:

- **Replaces scattered scripts** with organized module functions
- **Maintains compatibility** with existing VS Code tasks
- **Enhances reliability** with professional error handling
- **Improves discoverability** with command categorization
- **Adds consistency** with standardized output formatting

## 📈 Module Metrics

- **16 Functions** - Complete development workflow coverage including PowerShell 7.5 enhancements
- **11 Aliases** - Memorable short commands
- **4 Categories** - Organized by functionality
- **10 Happiness Quotes** - Developer motivation included
- **100% PowerShell 7.5** - Optimized for latest PowerShell features
- **98% Performance Improvement** - Array operations using PS 7.5 optimizations

### PowerShell 7.5 Feature Usage
- ✅ **Enhanced JSON handling** with comments and trailing comma support
- ✅ **Structured error reporting** with recommended actions
- ✅ **CLI XML serialization** for advanced data export
- ✅ **Performance optimizations** for array and collection operations
- ✅ **Advanced parameter validation** with improved tab completion

---

**The Bus Buddy PowerShell Module: Making development workflows as reliable as public transportation should be!** 🚌✨
