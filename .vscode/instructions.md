# 🚌 VS Code BusBuddy PowerShell Development Instructions

## 🎯 **CRITICAL: Always Use BusBuddy PowerShell Commands First**

When encountering any development issue, debugging problem, or runtime error in this workspace, **ALWAYS** use the built-in BusBuddy PowerShell Module commands BEFORE manual investigation:

## 🚀 **BusBuddy PowerShell Workflow (MANDATORY)**

### 1. **Runtime Issues & Application Failures**
```powershell
# STEP 1: Use BusBuddy health check for application issues
bb-health -Detailed

# STEP 2: Validate environment and dependencies
bb-env-check

# STEP 3: Get project status and validation
bb-info
```

### 2. **Build Issues & Compilation Errors**
```powershell
# STEP 1: Clean build with detailed output
bb-build -Clean -Restore -Verbosity detailed

# STEP 2: If build fails, check health status
bb-health -Quick

# STEP 3: Restore packages and try again
bb-restore
```

### 3. **Testing & Quality Assurance**
```powershell
# STEP 1: Run comprehensive test suite
bb-test -Coverage

# STEP 2: Check PowerShell 7.5 compatibility
Test-PowerShell75Features -ShowBenchmarks

# STEP 3: Validate configuration files
Test-BusBuddyConfiguration -AllowComments
```


### 4. **Development Session Management & Daily Workflow**
```powershell
# STEP 1: Start comprehensive development session
bb-dev-session -OpenIDE

# STEP 2: Run the daily composite workflow (recommended)
bb-daily

# STEP 3: Get all available commands for current context
bb-commands -Category Essential

# STEP 4: Get motivational support
bb-happiness -Count 3
```

## 🔧 **Mandatory BusBuddy PowerShell Usage Pattern**

### **Before Manual Debugging:**
1. ✅ Run bb-health -Detailed for comprehensive analysis
2. ✅ Use bb-env-check to validate environment setup
3. ✅ Check bb-info for module status and capabilities
4. ✅ Use bb-build with appropriate flags for build issues
5. ✅ Run bb-test for testing and validation issues
6. ⚠️ Only resort to manual investigation if PowerShell commands can't resolve

### **BusBuddy PowerShell Generated Solutions:**
- **Use bb-build flags**: -Clean, -Restore, -Verbosity detailed for comprehensive building
- **Follow API compliance**: PowerShell 7.5 optimized patterns and enhanced JSON handling
- **Leverage existing infrastructure**: Enhanced error handling, logging, structured reporting

## 📊 **BusBuddy PowerShell Advantage Examples**

### **Runtime Error Detection:**
- ✅ **bb-health Result**: Comprehensive project health analysis with specific recommendations
- ❌ **Manual Approach**: Hours of debugging without structured analysis

### **Build Analysis:**
- ✅ **bb-build -Verbosity detailed Result**: Structured build output with error categorization
- ❌ **Manual Approach**: Parsing verbose MSBuild output manually

### **Testing & Validation:**
- ✅ **bb-test -Coverage Result**: Complete test suite with coverage analysis
- ❌ **Manual Approach**: Manual test execution without coverage metrics

## 🎯 **Development Session Workflow**

1. **Start Session**: Always run `bb-dev-session -OpenIDE`
2. **Issue Occurs**: Use appropriate bb-command from list above
3. **Follow Guidance**: Implement PowerShell-recommended solutions
4. **Validate**: Re-run bb-health and bb-test to confirm fixes
5. **Document**: Update progress using bb-info and structured output

## 🏆 **Success Metrics**

- **Resolution Time**: BusBuddy PowerShell should reduce debugging time by 80%+
- **Solution Quality**: PowerShell provides tested, optimized recommendations
- **Consistency**: All developers use same intelligent PowerShell workflow
- **Knowledge Retention**: Module improves with PowerShell 7.5 feature usage

## 🚨 **Never Skip BusBuddy PowerShell Commands**

The BusBuddy PowerShell Module is specifically designed to:
- Understand BusBuddy's specific architecture patterns
- Provide PowerShell 7.5 optimized guidance and features
- Leverage project-specific utilities and configurations
- Generate actionable, tested recommendations with 98% performance improvement

**Skipping BusBuddy PowerShell = Missing 80% of available development capability!**

---


## 🔑 **Quick Reference Commands**

```powershell
# Essential BusBuddy PowerShell Commands
bb-dev-session -OpenIDE        # Complete development setup
bb-daily                      # Composite daily workflow (recommended)
bb-health -Detailed           # Comprehensive project analysis
bb-build -Clean -Restore      # Clean build with package restore
bb-test -Coverage             # Full test suite with coverage
bb-run -EnableDebug           # Run with debug logging
bb-env-check                  # Environment validation
bb-commands -Category Essential # List essential commands
bb-happiness                  # Get motivated!

# PowerShell 7.5 Feature Testing
Test-PowerShell75Features -ShowBenchmarks

# Configuration validation
Test-BusBuddyConfiguration -AllowComments -AllowTrailingCommas
```

**🎯 Lock this behavior into muscle memory: BusBuddy PowerShell FIRST, manual debugging LAST!**

---

## 📋 **Core BusBuddy PowerShell Commands Reference**

### 🔨 **Build & Development Commands**
- **`bb-build [options]`** - Build solution (-Clean, -Restore, -Configuration, -Verbosity)
- **`bb-run [options]`** - Run WPF application (-NoBuild, -EnableDebug, -Configuration)
- **`bb-test [options]`** - Execute test suite (-Filter, -Coverage, -Configuration)
- **`bb-clean`** - Clean build artifacts and caches
- **`bb-restore`** - Restore NuGet packages with force options

### 🚀 **Session & Environment Management**
- **`bb-dev-session [options]`** - Start development session (-SkipBuild, -OpenIDE)
- **`bb-health [options]`** - Project health check (-Quick, -Detailed)
- **`bb-env-check`** - Environment and dependency validation
- **`bb-info`** - Module information and status

### 😊 **Developer Experience & Utilities**
- **`bb-happiness [options]`** - Motivational quotes (-Count, -All)
- **`bb-commands [options]`** - List commands (-Category, -ShowAliases)
- **`Test-PowerShell75Features [options]`** - PS 7.5 testing (-ShowBenchmarks)

**🚌 Making development workflows as reliable as public transportation should be!**
