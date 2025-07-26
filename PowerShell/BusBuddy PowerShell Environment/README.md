# BusBuddy PowerShell Environment

This directory contains the organized PowerShell development environment for the BusBuddy project, following PowerShell 7.5.2 standards and best practices.

## 📁 Directory Structure

### `Profiles/`
PowerShell profiles and profile loading scripts:
- Core development profiles
- Environment-specific configurations
- Auto-loading profile scripts

### `Modules/`
Custom PowerShell modules organized by functionality:
- BusBuddy core module functions
- Development workflow modules
- Utility and helper modules

### `Scripts/`
Standalone PowerShell scripts for specific tasks:
- Build and deployment scripts
- Maintenance and diagnostic scripts
- One-time setup and migration scripts

### `Documentation/`
PowerShell-specific documentation:
- PowerShell 7.5.2 feature reference
- Module documentation
- Best practices and coding standards

### `Configuration/`
Configuration files and settings:
- PowerShell execution policies
- Module configuration files
- Environment-specific settings

### `Utilities/`
Utility functions and helper scripts:
- Common function libraries
- Cross-module utilities
- Development helpers

### `Templates/`
PowerShell script and module templates:
- Script templates with proper headers
- Module scaffolding templates
- Function templates with documentation

## 🚀 Getting Started

1. **Load the Development Environment:**
   ```powershell
   # From project root
   & ".\PowerShell\BusBuddy PowerShell Environment\Profiles\Load-BusBuddyEnvironment.ps1"
   ```

2. **Access BusBuddy Commands:**
   ```powershell
   # Core commands will be available with bb- prefix
   bb-health          # Health check
   bb-build           # Build solution
   bb-run             # Run application
   bb-diagnostic      # Full diagnostic
   ```

3. **Validate PowerShell 7.5.2 Compliance:**
   ```powershell
   # All scripts in this environment are PowerShell 7.5.2 compliant
   Invoke-MandatorySyntaxCheck -Path ".\PowerShell\BusBuddy PowerShell Environment"
   ```

## 📋 Standards and Conventions

### PowerShell 7.5.2 Compliance
- All scripts include `#Requires -Version 7.5` directive
- Use PowerShell 7.5.2-specific features appropriately
- Mandatory syntax validation for all scripts

### Naming Conventions
- **Profiles:** Use descriptive names with environment context
- **Modules:** Use `BusBuddy.ModuleName` format
- **Scripts:** Use Verb-Noun PowerShell naming convention
- **Functions:** Use standard PowerShell approved verbs

### Documentation Standards
- All functions include comprehensive help documentation
- Modules include proper manifest files
- Scripts include header comments with purpose and usage

### Error Handling
- Use structured error handling with try/catch blocks
- Implement proper parameter validation
- Include meaningful error messages and logging

## 🔧 Maintenance

### Regular Tasks
- Run syntax validation on all scripts
- Update module versions and dependencies
- Review and update documentation
- Validate PowerShell 7.5.2 feature usage

### Quality Assurance
- All scripts pass PSScriptAnalyzer validation
- Code follows established PowerShell best practices
- Proper testing and validation procedures

---

**Note:** This environment is specifically designed for PowerShell 7.5.2 and the BusBuddy project development workflow. All components are integrated with the main project build and deployment processes.
