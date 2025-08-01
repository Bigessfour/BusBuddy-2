# BusBuddy PowerShell Structure

This folder contains the PowerShell modules and scripts for the BusBuddy project.

## Main Components:

1. **BusBuddy PowerShell Environment**: Contains the main module (5,434 lines)
   - Modules/BusBuddy/BusBuddy.psm1: Main module file
   - Functions/: Various function categories

2. **Scripts/**: Organized script folders for different purposes
   - Build/: Build and deployment scripts
   - Maintenance/: System maintenance scripts
   - Setup/: Installation and configuration
   - Testing/: Test automation scripts
   - Utilities/: Utility scripts and tools
   - GitHub/: GitHub automation scripts
   - Verification/: Implementation verification scripts

3. **Modules/**: PowerShell modules
   - BusBuddy.ExceptionCapture/: Exception handling module

4. **Rules/**: PowerShell analysis and enforcement rules
   - BusBuddy-PowerShell.psm1: PowerShell rules module

The main BusBuddy module is loaded via the load-bus-buddy-profiles.ps1 script in the root directory.
