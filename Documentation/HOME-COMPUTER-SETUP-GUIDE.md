# 🏠 BusBuddy Home Computer Development Setup Guide

> **Complete setup guide to replicate your work computer's BusBuddy development environment on your home computer**

## 📋 Quick Start Checklist

- [ ] Install VS Code 1.102.2+
- [ ] Install PowerShell 7.5.2
- [ ] Install .NET 8.0 SDK
- [ ] Install Git
- [ ] Clone BusBuddy repository
- [ ] Install required VS Code extensions
- [ ] Configure VS Code settings
- [ ] Run environment validation
- [ ] Test build and run

---

## 🔧 Required Software Installations

### 1. Visual Studio Code (Version 1.102.2+)
```powershell
# Download from: https://code.visualstudio.com/
# OR use Winget (if available):
winget install Microsoft.VisualStudioCode

# Verify installation
code --version
```

### 2. PowerShell 7.5.2 (CRITICAL - Exact Version Required)
```powershell
# Download from: https://github.com/PowerShell/PowerShell/releases/tag/v7.5.2
# OR use Winget:
winget install Microsoft.PowerShell

# Verify installation
pwsh --version
# Should show: PowerShell 7.5.2
```

### 3. .NET 8.0 SDK
```powershell
# Download from: https://dotnet.microsoft.com/download/dotnet/8.0
# OR use Winget:
winget install Microsoft.DotNet.SDK.8

# Verify installation
dotnet --version
# Should show: 8.0.xxx
```

### 4. Git for Windows
```powershell
# Download from: https://git-scm.com/download/win
# OR use Winget:
winget install Git.Git

# Verify installation
git --version
```

### 5. Optional: GitHub CLI
```powershell
# For enhanced GitHub integration
winget install GitHub.cli

# Verify installation
gh --version
```

---

## 📁 Repository Setup

### 1. Clone the Repository
```powershell
# Navigate to your desired development folder
cd "C:\Users\YourUsername\Desktop"

# Clone the repository
git clone https://github.com/Bigessfour/BusBuddy-2.git BusBuddy

# Navigate to the project
cd BusBuddy
```

### 2. Verify Project Structure
```powershell
# Should see these key folders:
ls
# Expected: BusBuddy.Core/, BusBuddy.WPF/, BusBuddy.Tests/, .vscode/, Documentation/, Tools/
```

---

## 🔌 VS Code Extensions Setup

### 1. Install Required Extensions

Open VS Code and install these extensions (or use the command palette):

#### **CRITICAL Extensions** (Must Install):
```bash
# PowerShell Development
ms-vscode.powershell
ms-vscode.powershell-preview

# C# and .NET Development
ms-dotnettools.csharp
ms-dotnettools.csdevkit
ms-dotnettools.vscode-dotnet-runtime
ms-dotnettools.vscode-dotnet-pack

# XAML Development
ms-dotnettools.xaml
noesistechnologies.noesisgui-tools

# Task Management (EXCLUSIVE METHOD)
spmeesseman.vscode-taskexplorer

# Enhanced Tools
josefpihrt-vscode.roslynator
ms-vscode.vscode-json
redhat.vscode-xml
```

#### **Recommended Extensions**:
```bash
# AI Development
github.copilot
github.copilot-chat

# Git Integration
eamodio.gitlens
github.vscode-pull-request-github

# Productivity
aaron-bond.better-comments
streetsidesoftware.code-spell-checker
ms-vscode.vscode-yaml
```

### 2. Install Extensions via Command Line
```powershell
# Navigate to project folder and run:
code --install-extension ms-vscode.powershell
code --install-extension ms-vscode.powershell-preview
code --install-extension ms-dotnettools.csharp
code --install-extension ms-dotnettools.csdevkit
code --install-extension ms-dotnettools.xaml
code --install-extension spmeesseman.vscode-taskexplorer
code --install-extension josefpihrt-vscode.roslynator
```

---

## ⚙️ VS Code Configuration

### 1. Copy Settings Files

The `.vscode` folder in the repository contains the required configuration. When you open the BusBuddy folder in VS Code, it should automatically apply these settings.

Key configuration files:
- `.vscode/settings.json` - VS Code settings
- `.vscode/extensions.json` - Extension recommendations
- `.vscode/tasks.json` - Build and run tasks

### 2. Verify PowerShell Integration

1. Open VS Code in the BusBuddy folder:
   ```powershell
   code .
   ```

2. Open the integrated terminal (`Ctrl+`` ` or Terminal > New Terminal)

3. Verify PowerShell 7.5.2 is being used:
   ```powershell
   $PSVersionTable.PSVersion
   # Should show: 7.5.2
   ```

4. Verify VS Code environment:
   ```powershell
   $env:VSCODE_PID  # Should show a process ID
   $env:TERM_PROGRAM  # Should show "vscode"
   ```

---

## 🚀 BusBuddy Environment Setup

### 1. Load BusBuddy Development Profiles

The project includes PowerShell profiles for enhanced development:

```powershell
# In VS Code terminal, run:
.\load-bus-buddy-profiles.ps1

# You should see:
# 🚌 Bus Buddy PowerShell Profile Loaded!
# 🚀 Advanced Development Workflows loaded
# 🔗 GitHub Automation loaded
```

### 2. Test BusBuddy Commands

After loading profiles, test these commands:

```powershell
# Basic commands
bb-build     # Build the solution
bb-run       # Run the application
bb-test      # Run tests
bb-health    # Health check

# Advanced commands
bb-diagnostic    # Comprehensive system analysis
bb-dev-session   # Complete dev setup
bb-quick-test    # Quick build & test cycle
```

### 3. Environment Validation

Run the comprehensive environment validation:

```powershell
# This will check all requirements and dependencies
bb-diagnostic -IncludeTests -IncludeDependencies -IncludeMetrics -Detailed

# Or run the health check
bb-health -Verbose
```

---

## 🏗️ Build and Run Verification

### 1. First Build Test

Use the Task Explorer (preferred method):

1. Press `Ctrl+Shift+P`
2. Type "Task Explorer"
3. Select "Task Explorer: Run Task"
4. Choose "PS Fixed: Build Solution"

Or use PowerShell commands:
```powershell
bb-build
```

### 2. First Run Test

Using Task Explorer:
1. Task Explorer > "BB: Run App"

Or using PowerShell:
```powershell
bb-run
```

### 3. Verify Application Launch

The BusBuddy WPF application should launch successfully showing:
- Main window with navigation
- Dashboard view
- No critical errors in logs

---

## 🔍 Troubleshooting Common Issues

### PowerShell Extension Not Working

```powershell
# Check extension status
.\Tools\Scripts\Setup\check-powershell-extension.ps1

# Force extension fix if needed
.\Tools\Scripts\Setup\fix-powershell-extension.ps1
```

### VS Code Integration Issues

```powershell
# Fix VS Code integration
.\Tools\Scripts\Setup\fix-vscode-integration.ps1

# Safe integration fix
.\Tools\Scripts\Setup\fix-vscode-integration-safe.ps1
```

### Build Errors

```powershell
# Clean and restore packages
bb-clean
dotnet restore --force
bb-build
```

### PowerShell Profile Not Loading

```powershell
# Manual profile loading
Set-Location "C:\Path\To\BusBuddy"
.\load-bus-buddy-profiles.ps1 -Quiet

# Or use the initialization script
.\Tools\Scripts\Setup\init-busbuddy-environment.ps1 -ShowVersion
```

---

## 🎯 Development Workflow

### Daily Development Commands

```powershell
# 1. Start development session
bb-dev-session

# 2. Quick build and test cycle
bb-quick-test

# 3. UI iteration workflow
bb-ui-cycle

# 4. Health monitoring
bb-health
```

### Using Task Explorer (Recommended)

1. Install Task Explorer extension
2. Press `Ctrl+Shift+P` → "Task Explorer: Run Task"
3. Use these tasks:
   - "PS Fixed: Build Solution"
   - "BB: Run App"
   - "PS Fixed: Test Solution"
   - "PS Fixed: Health Check"

### PowerShell Development Features

Your environment includes these enhanced features:

- **Parallel processing** in reports and analysis
- **Background job support** with `-AsJob` parameter
- **Enhanced error aggregation** and reporting
- **XAML validation integration**
- **Real-time log monitoring**
- **Automated GitHub Actions integration**

---

## 📊 Verification Checklist

After setup, verify these work:

- [ ] VS Code opens BusBuddy workspace correctly
- [ ] PowerShell 7.5.2 loads in integrated terminal
- [ ] BusBuddy profiles load automatically
- [ ] `bb-build` command builds successfully
- [ ] `bb-run` command launches the application
- [ ] `bb-health` shows all green status
- [ ] Task Explorer shows BusBuddy tasks
- [ ] Extensions are installed and working
- [ ] No critical errors in VS Code Problems panel

---

## 📞 Support and Resources

### Documentation Links
- [PowerShell Standards](./POWERSHELL-STANDARDS.md)
- [Workflow Organization](./WORKFLOW-ORGANIZATION-REPORT.md)
- [VS Code Debug Enhancements](./VSCODE-DEBUG-ENHANCEMENTS.md)

### Quick Help Commands
```powershell
# Show available commands
bb-help

# Get system information
bb-diagnostic

# Export debug information
bb-debug-export

# GitHub integration help
bb-github-help
```

### Common Paths to Update

When setting up on your home computer, update these paths if different:

1. **PowerShell Path**: Default is `C:\Program Files\PowerShell\7\pwsh.exe`
2. **Project Location**: Update workspace folder paths as needed
3. **User Profile**: Update `${env:USERPROFILE}` references

---

## 🎉 You're Ready!

Once all checks pass, your home computer will have the same powerful BusBuddy development environment as your work computer, including:

- ✅ **PowerShell 7.5.2** with enhanced workflows
- ✅ **VS Code 1.102.2+** with optimal settings
- ✅ **BusBuddy command suite** (`bb-*` commands)
- ✅ **Task Explorer integration**
- ✅ **Advanced debugging capabilities**
- ✅ **GitHub Actions automation**
- ✅ **XAML development tools**
- ✅ **Real-time monitoring and health checks**

Happy coding! 🚌💻
