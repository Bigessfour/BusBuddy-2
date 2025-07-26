# 🚌 BusBuddy PowerShell Profile System

**Centralized PowerShell development environment for the BusBuddy project**

## 📁 Directory Structure

```
PowerShell-Profile/
├── 📋 load-bus-buddy-profiles.ps1     # Main profile loader
├── 📖 README.md                       # This documentation
│
├── 📂 Core/                           # Core profile components
│   ├── load-bus-buddy-profiles.ps1    # Core bb-* commands
│   ├── BusBuddy-PowerShell-Profile-7.5.2.ps1  # Enhanced PS 7.5 features
│   └── Setup-GlobalProfile.ps1        # Global profile installation
│
├── 📂 Modules/                        # PowerShell module system
│   ├── BusBuddy.psm1                 # Main PowerShell module (2100+ lines)
│   ├── BusBuddy.psd1                 # Module manifest
│   ├── Load-BusBuddyModule.ps1       # Module loader
│   ├── Module-Integration.ps1         # Integration utilities
│   ├── BusBuddy-ML-Learning.psm1     # Machine learning features
│   ├── BusBuddy-Phase2.psm1          # Phase 2 development features
│   └── README.md                      # Module documentation
│
├── 📂 Workflows/                      # Automation and workflow tools
│   ├── BusBuddy-Solid-Workflow.ps1   # Proven workflow patterns
│   ├── Enhanced-Terminal-Persistence.ps1  # Terminal session management
│   └── fix-terminal-workflow.ps1     # Workflow fixes and optimizations
│
└── 📂 Utilities/                      # Development utilities
    ├── bb-error-fix.ps1              # Automated error detection/fixing
    ├── bb-tools-inventory.ps1        # Development tools inventory
    └── dependency-management.ps1      # Package management and security
```

## 🚀 Quick Start

### Basic Usage
```powershell
# Load basic profile (Core functions only)
. .\PowerShell-Profile\load-bus-buddy-profiles.ps1 -ProfileLevel Basic

# Standard profile (Core + Modules)
. .\PowerShell-Profile\load-bus-buddy-profiles.ps1 -ProfileLevel Standard

# Advanced profile (Core + Modules + Enhanced features)
. .\PowerShell-Profile\load-bus-buddy-profiles.ps1 -ProfileLevel Advanced

# Full profile (Everything)
. .\PowerShell-Profile\load-bus-buddy-profiles.ps1 -ProfileLevel Full
```

### Available Commands After Loading

#### Core Commands (All Profile Levels)
- `bb-build` - Build Bus Buddy solution
- `bb-run` - Run Bus Buddy application
- `bb-test` - Run test suite
- `bb-clean` - Clean solution
- `bb-health` - System health check
- `bb-diagnostic` - Comprehensive diagnostics

#### Profile Management Commands
- `Get-BusBuddyProfileStatus` - Show profile system status
- `Reload-BusBuddyProfile -Level Advanced` - Reload profile system

## 📊 Profile Levels

### 🔰 Basic
- **What's Loaded**: Core functions only
- **Use Case**: Minimal overhead, basic development commands
- **Files**: `Core/load-bus-buddy-profiles.ps1`

### 📋 Standard (Default)
- **What's Loaded**: Core functions + PowerShell modules
- **Use Case**: Regular development with enhanced functionality
- **Files**: Core + `Modules/BusBuddy.psm1`

### ⭐ Advanced
- **What's Loaded**: Standard + Enhanced PS 7.5.2 features
- **Use Case**: Power users leveraging PowerShell 7.5 capabilities
- **Files**: Standard + `Core/BusBuddy-PowerShell-Profile-7.5.2.ps1`

### 🚀 Full
- **What's Loaded**: Everything (Core + Modules + Workflows + Utilities)
- **Use Case**: Complete development environment with all tools
- **Files**: All components loaded

## 🔧 Component Details

### Core Components
Located in `Core/` directory - essential functionality for all users.

#### load-bus-buddy-profiles.ps1
- Defines the fundamental `bb-*` commands
- Health checking and diagnostics
- Workspace detection and validation
- Error handling with fallback functionality

#### BusBuddy-PowerShell-Profile-7.5.2.ps1
- Advanced PowerShell 7.5 optimizations
- Enhanced environment detection
- Performance monitoring capabilities
- Comprehensive error handling

#### Setup-GlobalProfile.ps1
- Installs Bus Buddy functionality system-wide
- Integrates with VS Code and other editors
- Backup and restore capabilities
- Cross-session persistence

### Module System
Located in `Modules/` directory - comprehensive PowerShell module functionality.

#### BusBuddy.psm1 (2100+ lines)
- Professional PowerShell module optimized for PS 7.5
- Advanced command suite with full error handling
- JSON handling and performance optimizations
- Comprehensive logging and monitoring

#### Module Integration
- Automatic alias creation
- Cross-platform compatibility
- Development tool integration
- Enhanced command completion

### Workflow Tools
Located in `Workflows/` directory - automation and productivity tools.

#### BusBuddy-Solid-Workflow.ps1
- Implements proven development workflow patterns
- Automated build and deployment processes
- Error detection and recovery
- Performance monitoring and logging

#### Enhanced-Terminal-Persistence.ps1
- Persistent terminal session management
- State preservation across sessions
- Automatic profile loading
- Enhanced debugging capabilities

### Utilities
Located in `Utilities/` directory - specialized development tools.

#### bb-error-fix.ps1
- Automated error detection and analysis
- Intelligent error resolution suggestions
- Integration with VS Code and other editors
- Performance impact monitoring

#### dependency-management.ps1
- Package security scanning
- Version validation and pinning
- Dependency analysis and reporting
- Automated update recommendations

## 🔗 Integration Points

### VS Code Integration
The profile system integrates seamlessly with VS Code through:

1. **Task System**: VS Code tasks automatically load profiles
2. **Terminal Integration**: PowerShell 7.5.2 terminal with auto-loading
3. **Settings Configuration**: Optimized settings for profile functionality
4. **Extension Support**: Works with PowerShell and other extensions

### Automated Loading
Profiles can be automatically loaded through:

1. **VS Code Tasks**: Most tasks include profile loading
2. **Terminal Startup**: Automatic loading in PowerShell sessions
3. **Global Profile**: System-wide availability when installed
4. **Manual Loading**: Explicit loading with parameters

## 📈 Performance Considerations

### Loading Times
- **Basic**: < 1 second (minimal overhead)
- **Standard**: 1-3 seconds (module loading)
- **Advanced**: 2-4 seconds (enhanced features)
- **Full**: 3-6 seconds (all components)

### Memory Usage
- **Basic**: Minimal impact (< 10MB)
- **Standard**: Moderate impact (10-25MB)
- **Advanced**: Higher impact (25-50MB)
- **Full**: Maximum impact (50-100MB)

### Optimization Features
- Lazy loading of components
- Cached path resolution
- Efficient module imports
- Background loading where possible

## 🛠️ Troubleshooting

### Common Issues

#### Profile Not Loading
```powershell
# Check profile status
Get-BusBuddyProfileStatus

# Reload profile
Reload-BusBuddyProfile -Level Standard
```

#### Commands Not Available
```powershell
# Verify core commands loaded
Get-Command bb-* -ErrorAction SilentlyContinue

# Load core components manually
. .\PowerShell-Profile\Core\load-bus-buddy-profiles.ps1
```

#### Path Issues
```powershell
# Check workspace detection
$global:BusBuddyWorkspaceRoot
$global:BusBuddyProfileRoot

# Verify file structure
Get-BusBuddyProfileStatus
```

### Error Recovery
The profile system includes robust error handling:

1. **Graceful Degradation**: Falls back to basic functionality on errors
2. **Error Reporting**: Clear error messages with resolution suggestions
3. **Recovery Mode**: Provides `bb-help` command for guidance
4. **Manual Loading**: Individual components can be loaded separately

## 🚀 Future Enhancements

### Planned Features
- [ ] Module auto-updates
- [ ] Enhanced performance monitoring
- [ ] Cloud-based profile synchronization
- [ ] AI-powered error resolution
- [ ] Cross-platform optimization

### Contribution Guidelines
See `CONTRIBUTING.md` in the project root for guidelines on:
- Adding new profile components
- Modifying existing functionality
- Testing and validation procedures
- Documentation requirements

## 📞 Support

For issues with the PowerShell Profile System:

1. Check this documentation
2. Run `Get-BusBuddyProfileStatus` for diagnostics
3. Use `bb-help` in error recovery mode
4. Refer to the main project documentation
5. Create an issue in the project repository

---

**Version**: 1.0.0
**Created**: July 25, 2025
**PowerShell Version**: Optimized for PowerShell 7.5.2
**Compatibility**: Windows, macOS, Linux (PowerShell Core)
