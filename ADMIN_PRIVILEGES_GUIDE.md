# Bus Buddy Administrator Privileges Guide
# Complete guide for using administrator privileges with development tools

## ADMINISTRATOR SETUP FOR BUS BUDDY DEVELOPMENT

### 🔐 QUICK START - Running as Administrator

#### Option 1: Launch Specific Admin Tools
```powershell
# Test admin status
.\Scripts\Launch-AdminTools.ps1

# Install admin-only tools with elevation
.\Scripts\Launch-AdminTools.ps1 -AdminAction Install -ForceElevation

# Complete admin setup
.\Scripts\Launch-AdminTools.ps1 -AdminAction All -ForceElevation
```

#### Option 2: Direct PowerShell Admin Session
```powershell
# Right-click PowerShell 7 -> "Run as Administrator"
# Then run:
.\Scripts\Admin-Tools.ps1 -Action All
```

#### Option 3: Windows Terminal as Admin
```powershell
# In Windows Terminal: Ctrl+Shift+Click on PowerShell tab
# Select "Run as Administrator"
.\Scripts\Admin-Tools.ps1 -Action Status
```

### 🛠️ ADMINISTRATOR BENEFITS

#### System-Wide Installation
- **All Users Scope**: Modules installed for all users on the system
- **System PATH**: Tools available to all user sessions
- **Global Configuration**: PowerShell profiles and settings apply system-wide

#### Enhanced Tools Available (Admin Only)
- **PSWindowsUpdate**: Windows Update management
- **Active Directory Tools**: Domain administration
- **Hyper-V Management**: Virtual machine control
- **Secret Management**: Secure credential storage
- **System Registry**: Full registry access
- **Service Control**: Start/stop/configure Windows services

#### Developer Environment Configuration
- **Execution Policy**: Set to RemoteSigned system-wide
- **Developer Mode**: Enable Windows Developer Mode
- **Git Configuration**: System-wide Git settings
- **System Profile**: Auto-load development modules for all users

### 📋 ADMINISTRATOR ACTIONS AVAILABLE

#### Install (`-Action Install`)
```powershell
.\Scripts\Admin-Tools.ps1 -Action Install
```
- Installs administrator-only PowerShell modules
- Sets up system-wide development tools
- Configures security and compliance tools

#### Configure (`-Action Configure`)
```powershell
.\Scripts\Admin-Tools.ps1 -Action Configure
```
- Sets PowerShell execution policy
- Enables Windows Developer Mode
- Creates system-wide PowerShell profile
- Configures Git globally

#### Services (`-Action Services`)
```powershell
.\Scripts\Admin-Tools.ps1 -Action Services
```
- Installs Windows Terminal (if missing)
- Updates PowerShell 7 to latest version
- Installs Git (if missing)
- Updates .NET SDK to latest

#### Test (`-Action Test`)
```powershell
.\Scripts\Admin-Tools.ps1 -Action Test
```
- Tests registry write access
- Verifies service control capabilities
- Checks event log access
- Validates file system permissions

#### All (`-Action All`)
```powershell
.\Scripts\Admin-Tools.ps1 -Action All
```
- Runs all above actions in sequence
- Complete administrator setup

### 🔄 AUTOMATIC PRIVILEGE DETECTION

All Bus Buddy scripts automatically detect administrator status:

#### Standard User (Current Status)
- **Scope**: CurrentUser installations
- **Tools**: All development tools work normally
- **Limitations**: No system-wide changes

#### Administrator Mode
- **Scope**: AllUsers installations (system-wide)
- **Tools**: Additional admin-only modules
- **Capabilities**: Full system configuration

### 🎯 RECOMMENDED ADMINISTRATOR WORKFLOW

#### One-Time Setup (Run as Administrator)
```powershell
# 1. Complete admin setup
.\Scripts\Launch-AdminTools.ps1 -AdminAction All -ForceElevation

# 2. Verify installation
.\Scripts\Launch-AdminTools.ps1 -AdminAction Status -ForceElevation
```

#### Daily Development (Standard User)
```powershell
# Normal development tools work without admin
.\Scripts\DevHub.ps1
.\Scripts\Install-DeveloperTools.ps1
.\Scripts\PowerShell7-Integration.ps1
```

### 🚨 SECURITY CONSIDERATIONS

#### Safe Actions (No Admin Required)
- Running tests and builds
- Code analysis
- File operations within user directories
- Git operations
- Module installations to CurrentUser

#### Admin-Only Actions
- System-wide module installations
- Registry modifications
- Service management
- System configuration changes
- Windows feature installation

### 📊 VERIFICATION COMMANDS

#### Check Current Status
```powershell
# Quick status check
.\Scripts\Launch-AdminTools.ps1

# Detailed admin capabilities
.\Scripts\Launch-AdminTools.ps1 -AdminAction Status -ForceElevation
```

#### Verify Module Scope
```powershell
# Check where modules are installed
Get-Module PSScriptAnalyzer -ListAvailable | Select-Object Name, Version, ModuleBase

# Check system vs user installations
Get-Module -ListAvailable | Where-Object { $_.ModuleBase -like "*Program Files*" }
```

### 🔧 TROUBLESHOOTING

#### Common Issues
1. **"Access Denied"** - Need administrator privileges
2. **"Execution Policy"** - Run as admin to set system policy
3. **"Module Not Found"** - May be installed to different scope

#### Solutions
```powershell
# Fix execution policy (requires admin)
Set-ExecutionPolicy RemoteSigned -Scope LocalMachine

# Reinstall with correct scope
.\Scripts\Install-DeveloperTools.ps1  # Auto-detects scope

# Force admin mode
.\Scripts\Launch-AdminTools.ps1 -AdminAction Configure -ForceElevation
```

## SUMMARY

Your Bus Buddy development environment automatically adapts to your privilege level:
- **Standard User**: Full development capabilities with user-scope installations
- **Administrator**: Enhanced capabilities with system-wide installations and admin tools

Use `.\Scripts\Launch-AdminTools.ps1` to safely manage administrator privilege elevation when needed.
