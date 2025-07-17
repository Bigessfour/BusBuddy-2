# Syncfusion Theme Conflict Resolution Script
# This script helps identify theme conflicts by temporarily disabling the direct VisualStyle attributes in XAML files
# It creates backup files and can restore them when testing is complete

# Parameters
param(
    [Parameter(Mandatory = $false)]
    [switch]$Restore = $false,

    [Parameter(Mandatory = $false)]
    [string]$Path = 'c:\Users\steve.mckitrick\Desktop\Bus Buddy\BusBuddy.WPF',

    [Parameter(Mandatory = $false)]
    [switch]$ResourceDictionaryOnly = $false
)

# Set error action preference
$ErrorActionPreference = 'Stop'

# Function to log messages with timestamps
function Log-Message {
    param([string]$Message, [string]$Type = 'INFO')

    $timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
    Write-Host "[$timestamp] [$Type] $Message"
}

# Function to backup a file before modifying it
function Backup-File {
    param([string]$FilePath)

    $backupPath = "$FilePath.backup"

    # Check if backup already exists
    if (Test-Path $backupPath) {
        Log-Message "Backup already exists for $FilePath" 'WARN'
        return $false
    }

    try {
        Copy-Item -Path $FilePath -Destination $backupPath -Force
        Log-Message "Created backup: $backupPath" 'INFO'
        return $true
    } catch {
        Log-Message "Failed to create backup for $FilePath: $_" 'ERROR'
        return $false
    }
}

# Function to restore files from backups
function Restore-FromBackup {
    param([string]$Directory)

    $backupFiles = Get-ChildItem -Path $Directory -Filter '*.backup' -Recurse

    if ($backupFiles.Count -eq 0) {
        Log-Message "No backup files found in $Directory" 'WARN'
        return
    }

    Log-Message "Found $($backupFiles.Count) backup files to restore" 'INFO'

    foreach ($backupFile in $backupFiles) {
        $originalPath = $backupFile.FullName.Substring(0, $backupFile.FullName.Length - 7)

        try {
            Copy-Item -Path $backupFile.FullName -Destination $originalPath -Force
            Remove-Item -Path $backupFile.FullName -Force
            Log-Message "Restored $originalPath" 'INFO'
        } catch {
            Log-Message "Failed to restore $originalPath: $_" 'ERROR'
        }
    }

    Log-Message 'Restoration complete' 'INFO'
}

# Function to temporarily remove VisualStyle attributes from a file
function Remove-VisualStyleAttribute {
    param([string]$FilePath)

    # Read the file content
    $content = Get-Content -Path $FilePath -Raw

    # Define the pattern for the SfSkinManager.VisualStyle attribute
    $pattern = 'syncfusionskin:SfSkinManager\.VisualStyle="FluentDark"'

    # Check if the pattern exists in the file
    if ($content -match $pattern) {
        # Backup the file
        if (!(Backup-File -FilePath $FilePath)) {
            return
        }

        # Replace the pattern with a comment
        $modifiedContent = $content -replace $pattern, '<!-- TEMP REMOVED: syncfusionskin:SfSkinManager.VisualStyle="FluentDark" -->'

        # Write the modified content back to the file
        Set-Content -Path $FilePath -Value $modifiedContent

        Log-Message "Modified $FilePath - temporarily removed VisualStyle attributes" 'INFO'
    } else {
        Log-Message "No VisualStyle attributes found in $FilePath" 'INFO'
    }
}

# Main script execution
if ($Restore) {
    Log-Message 'Starting restoration process...' 'INFO'
    Restore-FromBackup -Directory $Path
} else {
    Log-Message 'Starting theme conflict resolution process...' 'INFO'

    if ($ResourceDictionaryOnly) {
        Log-Message 'Processing only the resource dictionary...' 'INFO'
        $files = Get-ChildItem -Path "$Path\Resources\Themes" -Filter 'BusBuddyResourceDictionary.xaml'
    } else {
        Log-Message 'Processing all XAML files...' 'INFO'
        $files = Get-ChildItem -Path $Path -Filter '*.xaml' -Recurse
    }

    Log-Message "Found $($files.Count) files to process" 'INFO'

    foreach ($file in $files) {
        Remove-VisualStyleAttribute -FilePath $file.FullName
    }

    Log-Message 'Processing complete' 'INFO'
    Log-Message 'Run the application to test if theme conflicts are resolved' 'INFO'
    Log-Message 'When done testing, run this script with -Restore to restore original files' 'INFO'
}
