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

### 5. **Industry-Standard GitHub Actions Integration**
```powershell
# STEP 1: Ship code to GitHub Actions testing platform (never wait for results)
git add .
git commit -m "feat: implement feature X"
git push
# Continue coding immediately - notifications will alert on completion

# STEP 2: Async CI monitoring (during coffee breaks or natural breaks)
gh run list --limit 3  # Quick status check

# STEP 3: Handle CI failures professionally (when notifications arrive)
$failedRun = gh run list --status failure --limit 1 --json id,workflowName | ConvertFrom-Json
if ($failedRun) {
    # Get failure summary
    gh run view $failedRun.id --json jobs | ConvertFrom-Json | ForEach-Object { 
        $_.jobs | Where-Object { $_.conclusion -eq "failure" } | Select-Object name,conclusion 
    }
    # Make minimal fix and re-push immediately
}
```

## 🔧 **Mandatory BusBuddy PowerShell Usage Pattern**

### **Before Manual Debugging:**
1. ✅ Run bb-health -Detailed for comprehensive analysis
2. ✅ Use bb-env-check to validate environment setup
3. ✅ Check bb-info for module status and capabilities
4. ✅ Use bb-build with appropriate flags for build issues
5. ✅ Run bb-test for testing and validation issues
6. ⚠️ Only resort to manual investigation if PowerShell commands can't resolve

### **Industry-Standard CI/CD Integration:**
1. ✅ **Ship Fast**: Push to GitHub Actions testing platform and continue coding
2. ✅ **Async Monitoring**: Use email notifications and periodic status checks
3. ✅ **Professional Fix Cycle**: Handle CI failures during natural development breaks
4. ✅ **Notification-Driven**: Act on CI results when convenient, not immediately
5. ✅ **Parallel Development**: Work on next feature while CI validates current feature

### **BusBuddy PowerShell Generated Solutions:**
- **Use bb-build flags**: -Clean, -Restore, -Verbosity detailed for comprehensive building
- **Follow API compliance**: PowerShell 7.5 optimized patterns and enhanced JSON handling
- **Leverage existing infrastructure**: Enhanced error handling, logging, structured reporting
- **GitHub Actions Integration**: Use `gh` CLI with PowerShell-compatible commands (no `head`, `tail`, etc.)

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
2. **Code Development**: Use bb-daily for quick build-test cycles
3. **Ship to CI**: Push code with `git push` and continue coding immediately
4. **Issue Occurs**: Use appropriate bb-command from list above
5. **CI Monitoring**: Check GitHub Actions asynchronously via notifications
6. **CI Failures**: Handle professionally during natural breaks (coffee, between features)
7. **Follow Guidance**: Implement PowerShell-recommended solutions
8. **Validate**: Re-run bb-health and bb-test to confirm fixes
9. **Document**: Update progress using bb-info and structured output

## 🏭 **Industry-Standard Workflow Integration**

### **Professional Development Cycle:**
```powershell
# Morning startup
bb-daily

# Development loop (repeat throughout day)
# 1. Code features locally
# 2. Quick local validation
bb-build && bb-test
# 3. Ship to GitHub Actions (don't wait!)
git add . && git commit -m "feat: implement X" && git push
# 4. Continue to next feature immediately

# Periodic CI checks (during breaks)
gh run list --limit 3
# Handle any failures when convenient, not immediately
```

### **Notification-Driven Development:**
- ✅ **Email notifications** for CI completion (already configured)
- ✅ **GitHub mobile app** for push notifications  
- ✅ **VS Code GitHub extension** for in-editor status
- ✅ **Never wait for CI** - productivity killer in industry

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
