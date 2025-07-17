# Bus Buddy PowerShell Tools

This folder contains PowerShell tools and scripts for the Bus Buddy WPF development environment, specifically optimized for **Syncfusion WPF 30.1.40**, **WPF scheduling system debugging**, and **novice developer productivity**.

## 📁 Folder Structure

```
Tools/
├── Scripts/                    # PowerShell automation scripts
│   ├── Format-XamlFiles.ps1   # XAML formatting and validation
│   └── test-debug-filter.ps1  # Debug filtering with scheduling focus
├── Config/                     # Configuration files
│   └── Novice-Setup.ps1       # Novice-friendly development setup
└── README.md                   # This file
```

## 🚀 Quick Start for Novices

### 1. Load the Development Environment
```powershell
# Load the novice-friendly setup (automatically detects project paths)
. .\Tools\Config\Novice-Setup.ps1
```

### 2. Start Development Session
```powershell
# Complete development environment setup
bb-dev-start
```

### 3. Daily Workflow Commands
```powershell
# Format XAML before commits (always run this!)
bb-fix-xaml

# Build and run
bb-build
bb-run

# Monitor for scheduling issues
bb-debug-start
```

## 🔧 Tool Details

### 📝 Format-XamlFiles.ps1
**Purpose**: Comprehensive XAML processing for Bus Buddy WPF project

**Key Features**:
- **Syncfusion 30.1.40 Compliance**: Removes deprecated `VisualStyle` attributes
- **Scheduling Validation**: Special validation for DateTime patterns and data binding
- **Safe Processing**: Automatic backup, XML validation before/after changes
- **Consistent Formatting**: 4-space indentation, proper line breaks

**Usage Examples**:
```powershell
# Format all XAML files with validation
.\Format-XamlFiles.ps1 -Format -Validate

# Pre-commit validation (recommended for all commits)
.\Format-XamlFiles.ps1 -PreCommitMode

# Remove deprecated Syncfusion attributes safely
.\Format-XamlFiles.ps1 -RemoveDeprecated -Format

# Validate only (no changes)
.\Format-XamlFiles.ps1 -Validate
```

**Scheduling-Specific Validations**:
- DateTimePattern validation for Syncfusion scheduler controls
- Missing FallbackValue detection in critical bindings
- Deprecated attribute detection and removal
- Required namespace validation

### 🔍 test-debug-filter.ps1
**Purpose**: Advanced debug filtering with WPF scheduling focus

**Key Features**:
- **Scheduling Priority**: Enhanced patterns for DateTime, route assignments, scheduler conflicts
- **Real-time Monitoring**: Stream mode for live debugging
- **UI Notifications**: Desktop alerts for critical issues
- **Categorized Output**: Priority-based filtering (Critical/High/Medium/Low)

**Usage Examples**:
```powershell
# Real-time monitoring with scheduling focus
.\test-debug-filter.ps1 -Mode Stream -SchedulingFocus -UINotifications

# Process existing log files for critical issues
.\test-debug-filter.ps1 -Mode File -Priority 1

# Enhanced scheduling debugging (recommended for route/schedule work)
.\test-debug-filter.ps1 -SchedulingFocus -Priority 2
```

**Scheduling Error Patterns Detected**:
- **Critical**: DateTime binding failures, scheduler crashes, data validation errors
- **High**: Syncfusion binding issues, schedule conflicts, input validation failures
- **Medium**: Performance warnings, binding warnings, configuration issues
- **Enhanced**: Invalid dates, route assignments, time validation, recurrence errors

### ⚙️ Novice-Setup.ps1
**Purpose**: Simplified development environment for new developers

**Key Features**:
- **Auto-Detection**: Automatically finds project root and sets up paths
- **Safe Defaults**: All operations include automatic backup and validation
- **Clear Messaging**: Color-coded progress messages and helpful error explanations
- **Health Monitoring**: Comprehensive environment and project health checks

**Available Commands** (after loading):
```powershell
# Essential Commands
bb-dev-start      # Complete development session startup
bb-health         # Comprehensive health check
bb-fix-xaml       # Format and validate XAML files
bb-debug-start    # Start debug monitoring

# Daily Workflow
bb-check-xaml     # Validate XAML (no changes)
bb-build          # Quick build
bb-run            # Quick run
bb-clean          # Clean build artifacts

# Navigation
bb-root           # Go to project root
bb-logs           # Go to logs folder
bb-tools          # Go to tools folder

# Help
Show-BusBuddyHelp # Show all available commands
```

## 🎯 VS Code Integration

### Tasks (Ctrl+Shift+P → "Tasks: Run Task")
- **🔧 Format All XAML Files**: Pre-commit formatting
- **🔍 Validate XAML Syntax**: Syntax checking only
- **🧹 Remove Deprecated Syncfusion Attributes**: Safe cleanup
- **📋 Pre-Commit XAML Check**: Complete validation before commits
- **🔥 Run Debug Filter (Scheduling Focus)**: Real-time debugging
- **🩺 Weekly Health Check**: Comprehensive project health
- **🚀 Full Development Cycle**: Complete workflow

### Problem Matchers
The tools integrate with VS Code's problem detection:
- XAML syntax errors appear in Problems panel
- Debug filter critical/high issues are highlighted
- Build errors are properly categorized

## 📋 Development Workflow for Novices

### Daily Development
1. **Start Development Session**:
   ```powershell
   bb-dev-start
   ```

2. **Before Making Changes**:
   ```powershell
   bb-check-xaml
   ```

3. **After XAML Changes**:
   ```powershell
   bb-fix-xaml
   ```

4. **During Development**:
   ```powershell
   bb-debug-start  # In separate terminal
   ```

### Before Committing
1. **Pre-Commit Check**:
   ```powershell
   bb-fix-xaml
   bb-build
   ```

2. **Or use VS Code Task**: "📋 Pre-Commit XAML Check"

### Weekly Maintenance
1. **Health Check**:
   ```powershell
   bb-health
   ```

2. **Or use VS Code Task**: "🩺 Weekly Health Check"

## 🔒 Safety Features

### Automatic Backups
- All XAML modifications create timestamped backups
- Backups are automatically cleaned up on successful operations
- Failed operations restore from backup automatically

### Validation Gates
- XML syntax validation before and after all changes
- Build validation prevents broken commits
- Pre-commit hooks ensure code quality

### Error Recovery
- Graceful fallback for all operations
- Clear error messages with suggested solutions
- Automatic rollback on critical failures

## 🎨 Syncfusion 30.1.40 Compliance

### What Gets Fixed Automatically
- Removal of deprecated `syncfusionskin:SfSkinManager.VisualStyle="FluentDark"` attributes
- Consistent namespace declarations
- Proper indentation and formatting
- XML syntax validation

### Scheduling-Specific Enhancements
- DateTimePattern validation for Syncfusion controls
- FallbackValue recommendations for scheduling data binding
- Route assignment conflict detection
- Time validation error patterns

## 📊 Monitoring and Reporting

### Debug Filter Output
- **Real-time console output** with color coding
- **File output** in `logs/debug-filtered-*.log`
- **Desktop notifications** for critical issues
- **Priority-based filtering** to reduce noise

### Health Check Reports
- Environment validation results
- XAML syntax check summary
- Build status verification
- Log analysis summary

## 💡 Tips for Novice Developers

1. **Always run `bb-dev-start` at beginning of development session**
2. **Use `bb-fix-xaml` before every commit**
3. **Keep `bb-debug-start` running in background during development**
4. **Run `bb-health` weekly to catch issues early**
5. **Check VS Code Problems panel for XAML issues**
6. **Use Tab completion for all `bb-*` commands**

## 🚨 Common Issues and Solutions

### XAML Formatting Fails
- **Cause**: XML syntax errors in source files
- **Solution**: Check VS Code Problems panel, fix syntax errors first

### Debug Filter No Output
- **Cause**: No log files or wrong log path
- **Solution**: Run application first to generate logs, check `logs/` folder

### Build Failures After XAML Changes
- **Cause**: Invalid XAML or missing references
- **Solution**: Restore from backup, check error messages, fix incrementally

### Environment Check Fails
- **Cause**: Wrong PowerShell version or missing tools
- **Solution**: Install PowerShell 7+, ensure project structure is correct

## 📚 Additional Resources

- **PowerShell Help**: Use `Get-Help <command-name> -Detailed` for any function
- **VS Code Tasks**: Ctrl+Shift+P → "Tasks: Run Task" for GUI access
- **Syncfusion Documentation**: [WPF Controls Documentation](https://help.syncfusion.com/wpf/welcome-to-syncfusion-essential-wpf)
- **Bus Buddy Architecture**: See main README.md for project overview

---

**Version**: 1.0
**Focus**: WPF Scheduling, Syncfusion 30.1.40, Novice Developer Experience
**Last Updated**: July 17, 2025
