#Requires -Version 7.0
<#
.SYNOPSIS
    🧹 BusBuddy Root Directory Organization Script
    
.DESCRIPTION
    Organizes the root directory by moving legacy files to appropriate locations,
    creating proper folder structure, and cleaning up unnecessary clutter.
    
.PARAMETER WhatIf
    Shows what would be moved without actually moving files
    
.PARAMETER Force
    Forces the organization even if some operations might be destructive
    
.EXAMPLE
    .\organize-root-directory.ps1 -WhatIf
    .\organize-root-directory.ps1 -Force
#>

param(
    [switch]$WhatIf,
    [switch]$Force
)

# Set error handling
$ErrorActionPreference = 'Stop'

# Define color scheme
$Colors = @{
    Header    = 'Cyan'
    Success   = 'Green'
    Warning   = 'Yellow'
    Error     = 'Red'
    Info      = 'White'
    Highlight = 'Magenta'
}

function Write-ColorMessage {
    param(
        [string]$Message,
        [string]$Color = 'White',
        [string]$Prefix = ''
    )
    
    if ($Prefix) {
        Write-Host "$Prefix " -ForegroundColor $Colors[$Color] -NoNewline
    }
    Write-Host $Message -ForegroundColor $Colors[$Color]
}

function Test-GitRepository {
    if (-not (Test-Path '.git')) {
        throw "This script must be run from the root of a Git repository"
    }
}

function New-DirectoryIfNotExists {
    param([string]$Path)
    
    if (-not (Test-Path $Path)) {
        if ($WhatIf) {
            Write-ColorMessage "WOULD CREATE: $Path" 'Info' '📁'
        } else {
            New-Item -ItemType Directory -Path $Path -Force | Out-Null
            Write-ColorMessage "Created directory: $Path" 'Success' '📁'
        }
    }
}

function Move-FileToLocation {
    param(
        [string]$Source,
        [string]$Destination,
        [string]$Reason = ""
    )
    
    if (Test-Path $Source) {
        $destDir = Split-Path $Destination -Parent
        New-DirectoryIfNotExists $destDir
        
        if ($WhatIf) {
            Write-ColorMessage "WOULD MOVE: $Source → $Destination" 'Warning' '📦'
            if ($Reason) { Write-ColorMessage "    Reason: $Reason" 'Info' '   ' }
        } else {
            try {
                Move-Item -Path $Source -Destination $Destination -Force
                Write-ColorMessage "Moved: $Source → $Destination" 'Success' '📦'
                if ($Reason) { Write-ColorMessage "    Reason: $Reason" 'Info' '   ' }
            } catch {
                Write-ColorMessage "Failed to move $Source`: $($_.Exception.Message)" 'Error' '❌'
            }
        }
    }
}

function Remove-FileIfExists {
    param(
        [string]$Path,
        [string]$Reason = ""
    )
    
    if (Test-Path $Path) {
        if ($WhatIf) {
            Write-ColorMessage "WOULD DELETE: $Path" 'Warning' '🗑️'
            if ($Reason) { Write-ColorMessage "    Reason: $Reason" 'Info' '   ' }
        } else {
            try {
                Remove-Item -Path $Path -Force -Recurse
                Write-ColorMessage "Deleted: $Path" 'Success' '🗑️'
                if ($Reason) { Write-ColorMessage "    Reason: $Reason" 'Info' '   ' }
            } catch {
                Write-ColorMessage "Failed to delete $Path`: $($_.Exception.Message)" 'Error' '❌'
            }
        }
    }
}

function Start-RootDirectoryOrganization {
    Write-ColorMessage "🧹 BusBuddy Root Directory Organization" 'Header'
    Write-ColorMessage "===========================================" 'Header'
    
    if ($WhatIf) {
        Write-ColorMessage "PREVIEW MODE - No files will be moved" 'Warning' '👁️'
    }
    
    if ($Force) {
        Write-ColorMessage "FORCE MODE - Proceeding with potentially destructive operations" 'Warning' '⚡'
    }
    
    # Validate we're in the right place
    Test-GitRepository
    
    # Create organized directory structure
    Write-ColorMessage "`n📁 Creating organized directory structure..." 'Info'
    
    # Scripts organization
    New-DirectoryIfNotExists "Scripts"
    New-DirectoryIfNotExists "Scripts\Development"
    New-DirectoryIfNotExists "Scripts\Maintenance" 
    New-DirectoryIfNotExists "Scripts\Legacy"
    New-DirectoryIfNotExists "Scripts\PowerShell"
    
    # Configuration organization  
    New-DirectoryIfNotExists "Configuration"
    New-DirectoryIfNotExists "Configuration\Development"
    New-DirectoryIfNotExists "Configuration\Production"
    
    # Archive for legacy items
    New-DirectoryIfNotExists "Archive"
    New-DirectoryIfNotExists "Archive\Legacy-Scripts"
    New-DirectoryIfNotExists "Archive\Old-Configurations"
    
    Write-ColorMessage "`n📦 Organizing PowerShell scripts..." 'Info'
    
    # Move development scripts
    Move-FileToLocation "debug-app-startup.ps1" "Scripts\Development\debug-app-startup.ps1" "Development debugging script"
    Move-FileToLocation "run-busbuddy.ps1" "Scripts\Development\run-busbuddy.ps1" "Development runner script"
    Move-FileToLocation "run-with-logging.ps1" "Scripts\Development\run-with-logging.ps1" "Development logging script"
    Move-FileToLocation "dependency-management.ps1" "Scripts\Development\dependency-management.ps1" "Active dependency management"
    
    # Move maintenance scripts
    Move-FileToLocation "check-repository-size.ps1" "Scripts\Maintenance\check-repository-size.ps1" "Repository maintenance"
    Move-FileToLocation "fix-analyzer-warnings.ps1" "Scripts\Maintenance\fix-analyzer-warnings.ps1" "Code analysis maintenance"
    Move-FileToLocation "phase1-completion-verification.ps1" "Scripts\Maintenance\phase1-completion-verification.ps1" "Phase verification"
    Move-FileToLocation "run-comprehensive-pipeline.ps1" "Scripts\Maintenance\run-comprehensive-pipeline.ps1" "CI/CD pipeline"
    
    # Move legacy/cleanup scripts to archive
    Move-FileToLocation "cleanup-legacy-files.ps1" "Archive\Legacy-Scripts\cleanup-legacy-files.ps1" "Legacy cleanup script (completed)"
    Move-FileToLocation "comprehensive-legacy-cleanup.ps1" "Archive\Legacy-Scripts\comprehensive-legacy-cleanup.ps1" "Legacy cleanup script (completed)"
    Move-FileToLocation "purge-legacy-references.ps1" "Archive\Legacy-Scripts\purge-legacy-references.ps1" "Legacy cleanup script (completed)"
    Move-FileToLocation "ci-cd-dependency-integration.ps1" "Archive\Legacy-Scripts\ci-cd-dependency-integration.ps1" "Legacy CI/CD script"
    
    # Move Steve-related files to archive (legacy experiment files)
    
    # Move Syncfusion setup scripts  
    Move-FileToLocation "set-syncfusion-license-helper.ps1" "Scripts\PowerShell\set-syncfusion-license-helper.ps1" "Syncfusion license management"
    Move-FileToLocation "setup-syncfusion-license.ps1" "Scripts\PowerShell\setup-syncfusion-license.ps1" "Syncfusion license setup"
    
    Write-ColorMessage "`n🔧 Organizing configuration files..." 'Info'
    
    # Move configuration files to appropriate locations
    Move-FileToLocation "appsettings.azure.json" "Configuration\Production\appsettings.azure.json" "Azure production configuration"
    # Note: Keep appsettings.json in root as it's referenced by projects
    
    # Move development configuration
    Move-FileToLocation "PSScriptAnalyzerSettings.psd1" "Configuration\Development\PSScriptAnalyzerSettings.psd1" "PowerShell analysis configuration"
    
    Write-ColorMessage "`n🗑️ Removing unnecessary files..." 'Info'
    
    # Remove empty or unnecessary directories
    if (Test-Path "packages" -PathType Container) {
        $packagesContent = Get-ChildItem "packages" -Force
        if ($packagesContent.Count -eq 0) {
            Remove-FileIfExists "packages" "Empty packages directory (using global packages)"
        }
    }
    
    # Remove legacy marker file
    Remove-FileIfExists "-la" "Legacy file listing artifact"
    
    Write-ColorMessage "`n📋 Validating critical files remain in root..." 'Info'
    
    # Validate essential root files are still present
    $essentialFiles = @(
        'BusBuddy.sln',
        'Directory.Build.props', 
        'global.json',
        'NuGet.config',
        'README.md',
        'LICENSE',
        'CONTRIBUTING.md',
        '.gitignore',
        '.editorconfig'
    )
    
    foreach ($file in $essentialFiles) {
        if (Test-Path $file) {
            Write-ColorMessage "✅ Essential file preserved: $file" 'Success' '📄'
        } else {
            Write-ColorMessage "⚠️ Essential file missing: $file" 'Warning' '📄'
        }
    }
    
    Write-ColorMessage "`n📊 Organization summary..." 'Info'
    
    # Count files in each category
    $scriptsCount = if (Test-Path "Scripts") { (Get-ChildItem "Scripts" -Recurse -File).Count } else { 0 }
    $configCount = if (Test-Path "Configuration") { (Get-ChildItem "Configuration" -Recurse -File).Count } else { 0 }
    $archiveCount = if (Test-Path "Archive") { (Get-ChildItem "Archive" -Recurse -File).Count } else { 0 }
    
    Write-ColorMessage "📁 Scripts organized: $scriptsCount files" 'Success' '  '
    Write-ColorMessage "🔧 Configuration files: $configCount files" 'Success' '  '
    Write-ColorMessage "📦 Archived files: $archiveCount files" 'Success' '  '
    
    # Show new directory structure
    Write-ColorMessage "`n🏗️ New organized structure:" 'Header'
    if (Test-Path "Scripts") {
        Write-ColorMessage "Scripts/" 'Highlight'
        Get-ChildItem "Scripts" -Directory | ForEach-Object {
            Write-ColorMessage "  └── $($_.Name)/" 'Info'
            $fileCount = (Get-ChildItem $_.FullName -File).Count
            Write-ColorMessage "      ($fileCount files)" 'Info'
        }
    }
    
    if (Test-Path "Configuration") {
        Write-ColorMessage "Configuration/" 'Highlight' 
        Get-ChildItem "Configuration" -Directory | ForEach-Object {
            Write-ColorMessage "  └── $($_.Name)/" 'Info'
            $fileCount = (Get-ChildItem $_.FullName -File).Count
            Write-ColorMessage "      ($fileCount files)" 'Info'
        }
    }
    
    if (Test-Path "Archive") {
        Write-ColorMessage "Archive/" 'Highlight'
        Get-ChildItem "Archive" -Directory | ForEach-Object {
            Write-ColorMessage "  └── $($_.Name)/" 'Info'
            $fileCount = (Get-ChildItem $_.FullName -File).Count
            Write-ColorMessage "      ($fileCount files)" 'Info'
        }
    }
    
    Write-ColorMessage "`n✅ Root directory organization complete!" 'Success'
    
    if (-not $WhatIf) {
        Write-ColorMessage "`n📝 Next steps:" 'Info'
        Write-ColorMessage "1. Review the organized structure" 'Info' '  '
        Write-ColorMessage "2. Update any scripts that reference moved files" 'Info' '  '
        Write-ColorMessage "3. Update .gitignore if needed" 'Info' '  '
        Write-ColorMessage "4. Commit the organized structure" 'Info' '  '
        Write-ColorMessage "5. Update documentation to reflect new paths" 'Info' '  '
    }
}

# Main execution
try {
    Start-RootDirectoryOrganization
} catch {
    Write-ColorMessage "❌ Organization failed: $($_.Exception.Message)" 'Error'
    exit 1
}
