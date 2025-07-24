# 🚌 BusBuddy PowerShell Profile - Quick Start Guide

**🎯 DEFINITIVE ENTRY POINT GUIDE**
**📅 Updated:** July 24, 2025
**🚀 Version:** PowerShell 7.6+ Enhanced

---

## 🎯 **ENTRY POINT: ONE COMMAND TO RULE THEM ALL**

### **🚀 PRIMARY LAUNCH COMMAND (Recommended)**

```powershell
# Navigate to BusBuddy directory and launch
cd "c:\Users\biges\Desktop\BusBuddy\BusBuddy"
.\launch-busbuddy-profile.ps1
```

**✅ This is THE definitive entry point - no ambiguity!**

---

## 📋 **COMPLETE COMMAND REFERENCE**

### **🎯 Basic Launch Commands**

```powershell
# 1. FULL FEATURED LAUNCH (Recommended)
.\launch-busbuddy-profile.ps1

# 2. QUIET MODE (No verbose output)
.\launch-busbuddy-profile.ps1 -Quiet

# 3. SKIP AI ASSISTANT (Faster startup)
.\launch-busbuddy-profile.ps1 -SkipAI

# 4. OFFLINE MODE (No PowerShell Gallery)
.\launch-busbuddy-profile.ps1 -OfflineMode

# 5. DEVELOPMENT MODE (Skip validation, update modules)
.\launch-busbuddy-profile.ps1 -SkipValidation -UpdateModules
```

### **🔧 Advanced Parameters**

```powershell
# Complete parameter list
.\launch-busbuddy-profile.ps1 `
    -Quiet `                    # Suppress verbose output
    -SkipAI `                   # Skip AI Assistant loading
    -SkipValidation `           # Skip dependency validation
    -BackgroundInit:$false `    # Disable background initialization
    -EnableOptimizations:$false ` # Disable performance optimizations
    -UpdateModules `            # Update existing PowerShell modules
    -OfflineMode `              # Skip PowerShell Gallery operations
    -ModuleScope "AllUsers"     # Install modules for all users
```

### **📚 Help & Information**

```powershell
# Get help
.\launch-busbuddy-profile.ps1 -Help

# After loading, get detailed metrics
Get-BusBuddyLoadingReport

# Check configuration
$Global:BusBuddyConfig

# View AI Assistant status
$Global:BusBuddyAIStatus
```

---

## 🎯 **WHAT HAPPENS WHEN YOU LAUNCH**

### **⚡ Phase 1: Environment Detection**
- ✅ PowerShell 7.6+ feature detection
- ✅ System resource analysis (CPU, memory)
- ✅ PowerShell Gallery connectivity check

### **📦 Phase 2: Dependency Validation**
- ✅ .NET 8+ SDK verification
- ✅ Git installation check
- ✅ VS Code availability
- ✅ PowerShell module validation
- ✅ **AUTO-INSTALL** missing modules from PowerShell Gallery

### **🤖 Phase 3: AI Assistant Integration**
- ✅ Multi-path AI Assistant discovery
- ✅ Background loading (non-blocking)
- ✅ Graceful fallback if AI unavailable

### **🔧 Phase 4: Environment Configuration**
- ✅ Optimal .NET environment setup
- ✅ Performance optimizations
- ✅ PowerShell 7.6+ feature activation

---

## 💡 **TROUBLESHOOTING**

### **❌ Common Issues & Solutions**

#### **PowerShell Version Issues**
```powershell
# Check your PowerShell version
$PSVersionTable.PSVersion

# If less than 7.6, download from:
# https://github.com/PowerShell/PowerShell/releases
```

#### **Execution Policy Issues**
```powershell
# Allow script execution
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

#### **Missing Dependencies**
```powershell
# The launcher auto-installs missing modules, but manual install:
Install-Module PSScriptAnalyzer -Force -Scope CurrentUser
Install-Module Pester -Force -Scope CurrentUser
```

#### **AI Assistant Not Found**
```powershell
# Verify AI Assistant exists
Test-Path "AI-Assistant\Core\ai-development-assistant.ps1"

# Skip AI if needed
.\launch-busbuddy-profile.ps1 -SkipAI
```

---

## 🎯 **POST-LAUNCH VERIFICATION**

### **✅ Verify Everything Loaded Correctly**

```powershell
# 1. Check overall configuration
$Global:BusBuddyConfig | Format-Table

# 2. Verify PowerShell 7.6+ features are active
$Global:BusBuddyEnvironment.EnhancedFeaturesActive

# 3. Check AI Assistant status
$Global:BusBuddyAIStatus.Available

# 4. Get detailed loading report
Get-BusBuddyLoadingReport

# 5. View optimization metrics
$Global:BusBuddyOptimizationMetrics.LoadingPhases
```

---

## 🚀 **ADVANCED USAGE**

### **🔄 Environment Variables (Optional)**

```powershell
# Set custom AI Assistant path
$env:BUSBUDDY_AI_PATH = "C:\Custom\Path\ai-assistant.ps1"

# Set BusBuddy root directory
$env:BUS_BUDDY_ROOT = "C:\Dev\BusBuddy"
```

### **📊 Performance Optimization**

```powershell
# Maximum performance launch
.\launch-busbuddy-profile.ps1 -BackgroundInit -EnableOptimizations

# Minimal resource launch
.\launch-busbuddy-profile.ps1 -Quiet -SkipAI -OfflineMode
```

---

## 🎯 **INTEGRATION WITH VS CODE TASKS**

### **Task Explorer Integration**
- The profile integrates with existing VS Code tasks
- Use **Task Explorer** extension for optimal task management
- Enhanced task monitoring with the loaded profile

### **Available Tasks After Profile Load**
- `bb-build` - Enhanced build with monitoring
- `bb-run` - Optimized application launch
- `bb-health` - Comprehensive health check
- `bb-diagnostic` - Advanced system diagnostics

---

## 📋 **SYSTEM REQUIREMENTS**

| Component | Requirement | Status |
|-----------|-------------|--------|
| **PowerShell** | 7.6+ (Preview OK) | ✅ Required |
| **.NET SDK** | 8.0+ | ✅ Required |
| **Git** | Latest | ✅ Required |
| **VS Code** | Latest | ✅ Recommended |
| **Memory** | 4GB+ | ✅ Recommended |
| **Disk Space** | 2GB+ | ✅ Required |

---

## 🎯 **FINAL WORD: KEEP IT SIMPLE**

**The entry point is simple:**

```powershell
cd "c:\Users\biges\Desktop\BusBuddy\BusBuddy"
.\launch-busbuddy-profile.ps1
```

**That's it! No confusion, no ambiguity. This ONE command:**
- ✅ Detects your environment
- ✅ Validates dependencies
- ✅ Auto-installs missing modules
- ✅ Loads AI Assistant
- ✅ Optimizes performance
- ✅ Sets up BusBuddy development environment

**🚀 Ready to code!**
