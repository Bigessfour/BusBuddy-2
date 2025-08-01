# BusBuddy PowerShell Structure (Clean Architecture)

This folder contains the PowerShell modules and scripts for the BusBuddy project, organized using a clean, standard PowerShell module layout following PS 7.5.2 best practices.

## Clean Structure Overview:

### 📁 **Modules/** - All PowerShell modules consolidated here
- **BusBuddy/**: Main module (5,434+ lines) with complete manifest
  - `BusBuddy.psd1`: Module manifest with proper exports and metadata
  - `BusBuddy.psm1`: Core module file with 40+ bb-* commands
  - `Functions/`: Organized function categories (AI/, Build/, Database/, Diagnostics/, Utilities/)
- **BusBuddy.ExceptionCapture/**: Exception handling and monitoring
  - `BusBuddy.ExceptionCapture.psd1`: Module manifest
  - `BusBuddy.ExceptionCapture.psm1`: Exception capture functionality
- **BusBuddy.Rules/**: PowerShell standards and rules enforcement
  - `BusBuddy.Rules.psd1`: Module manifest
  - `BusBuddy.Rules.psm1`: Rules and validation logic

### 📁 **Profiles/** - PowerShell profile management
- `Microsoft.PowerShell_profile.ps1`: PS 7.5.2 compliant profile with multi-threading support

### 📁 **Scripts/** - Categorized scripts for all development tasks
- **Build/**: Build and deployment automation
- **Configuration/**: System and environment configuration
- **GitHub/**: Git operations and GitHub automation
- **Maintenance/**: System maintenance and cleanup
- **Setup/**: Installation and initial configuration
- **Testing/**: Test automation and validation
- **Utilities/**: General-purpose utility scripts
- **Verification/**: Implementation and compliance verification

## Key Improvements from Cleanup:
✅ **Eliminated deep nesting** - Removed "BusBuddy PowerShell Environment" wrapper layer  
✅ **Consolidated modules** - All modules now in single `Modules/` folder  
✅ **Added manifests** - Proper .psd1 files for all modules  
✅ **Organized scripts** - Root-level scripts moved to appropriate categories  
✅ **Standard layout** - Follows Microsoft PowerShell module guidelines  
✅ **PS 7.5.2 compliant** - Ready for modern PowerShell features

## Usage:
The main BusBuddy module can be imported directly:
```powershell
Import-Module .\PowerShell\Modules\BusBuddy\BusBuddy.psd1
```

All bb-* commands will be available after module import.
