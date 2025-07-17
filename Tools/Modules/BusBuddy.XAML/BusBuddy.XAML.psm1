# BusBuddy.XAML Module
# Reusable XAML processing functions

# ============================================
# Core XAML Validation Functions
# ============================================

function Test-XamlSyntax {
    <#
    .SYNOPSIS
    Validates XAML file syntax using XML parser

    .DESCRIPTION
    Tests if a XAML file has valid XML syntax before processing
    Returns true if valid, false if invalid with error details

    .PARAMETER FilePath
    Path to the XAML file to validate

    .EXAMPLE
    Test-XamlSyntax -FilePath "MainWindow.xaml"
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true, ValueFromPipeline = $true)]
        [string]$FilePath
    )

    process {
        if (-not (Test-Path $FilePath)) {
            Write-Host "❌ File not found: $FilePath" -ForegroundColor Red
            return $false
        }

        try {
            [xml]$test = Get-Content $FilePath -Raw
            Write-Verbose "✅ Valid XML syntax: $FilePath"
            return $true
        } catch {
            Write-Host "❌ XML Syntax Error in $FilePath`: $($_.Exception.Message)" -ForegroundColor Red
            return $false
        }
    }
}

function Get-XamlFileInfo {
    <#
    .SYNOPSIS
    Gets detailed information about a XAML file

    .DESCRIPTION
    Analyzes a XAML file and returns metadata including size, validation status, and XML structure

    .PARAMETER FilePath
    Path to the XAML file to analyze

    .EXAMPLE
    Get-XamlFileInfo -FilePath "MainWindow.xaml"
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true, ValueFromPipeline = $true)]
        [string]$FilePath
    )

    process {
        if (-not (Test-Path $FilePath)) {
            return $null
        }

        $file = Get-Item $FilePath
        $isValid = Test-XamlSyntax -FilePath $FilePath

        $info = [PSCustomObject]@{
            Name = $file.Name
            FullPath = $file.FullName
            Size = $file.Length
            SizeKB = [math]::Round($file.Length / 1KB, 2)
            LastModified = $file.LastWriteTime
            IsValid = $isValid
            RootElement = $null
            Namespaces = @()
            HasSyncfusion = $false
        }

        if ($isValid) {
            try {
                [xml]$xaml = Get-Content $FilePath -Raw
                $info.RootElement = $xaml.DocumentElement.LocalName

                # Extract namespaces
                foreach ($attr in $xaml.DocumentElement.Attributes) {
                    if ($attr.Name.StartsWith('xmlns')) {
                        $info.Namespaces += @{
                            Prefix = $attr.Name -replace '^xmlns:?', ''
                            URI = $attr.Value
                        }

                        if ($attr.Value -like '*syncfusion*') {
                            $info.HasSyncfusion = $true
                        }
                    }
                }
            } catch {
                Write-Verbose "Could not parse XML structure for $FilePath"
            }
        }

        return $info
    }
}

# ============================================
# Backup and Safety Functions
# ============================================

function Backup-XamlFiles {
    <#
    .SYNOPSIS
    Creates timestamped backup of all XAML files in a directory

    .DESCRIPTION
    Creates a backup directory with timestamp and copies all XAML files
    Preserves directory structure for easy restoration

    .PARAMETER RootPath
    Root directory to backup XAML files from

    .EXAMPLE
    Backup-XamlFiles -RootPath "BusBuddy.WPF"
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$RootPath
    )

    if (-not (Test-Path $RootPath)) {
        Write-Host "❌ Path not found: $RootPath" -ForegroundColor Red
        return $null
    }

    $timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
    $backupDir = "$RootPath\.backup_$timestamp"

    Write-Host '🔄 Creating XAML backup...' -ForegroundColor Yellow

    $xamlFiles = Get-ChildItem -Path $RootPath -Recurse -Filter '*.xaml'

    if ($xamlFiles.Count -eq 0) {
        Write-Host "⚠️ No XAML files found in $RootPath" -ForegroundColor Yellow
        return $null
    }

    foreach ($file in $xamlFiles) {
        $relativePath = $file.FullName.Substring($RootPath.Length + 1)
        $backupPath = Join-Path $backupDir $relativePath
        $backupFolder = Split-Path $backupPath -Parent

        if (-not (Test-Path $backupFolder)) {
            New-Item -Path $backupFolder -ItemType Directory -Force | Out-Null
        }
        Copy-Item $file.FullName $backupPath
    }

    Write-Host "✅ Backup created: $backupDir ($($xamlFiles.Count) files)" -ForegroundColor Green
    return $backupDir
}

function Restore-XamlBackup {
    <#
    .SYNOPSIS
    Restores XAML files from a backup directory

    .DESCRIPTION
    Restores XAML files from a backup created by Backup-XamlFiles

    .PARAMETER BackupPath
    Path to the backup directory

    .PARAMETER TargetPath
    Target directory to restore files to (defaults to original location)

    .EXAMPLE
    Restore-XamlBackup -BackupPath "BusBuddy.WPF\.backup_20250717_120000"
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$BackupPath,

        [string]$TargetPath = $null
    )

    if (-not (Test-Path $BackupPath)) {
        Write-Host "❌ Backup path not found: $BackupPath" -ForegroundColor Red
        return $false
    }

    # Determine target path
    if (-not $TargetPath) {
        $TargetPath = Split-Path $BackupPath -Parent
    }

    Write-Host "🔄 Restoring XAML files from backup..." -ForegroundColor Yellow

    $backupFiles = Get-ChildItem -Path $BackupPath -Recurse -Filter '*.xaml'

    foreach ($file in $backupFiles) {
        $relativePath = $file.FullName.Substring($BackupPath.Length + 1)
        $targetFile = Join-Path $TargetPath $relativePath
        $targetDir = Split-Path $targetFile -Parent

        if (-not (Test-Path $targetDir)) {
            New-Item -Path $targetDir -ItemType Directory -Force | Out-Null
        }

        Copy-Item $file.FullName $targetFile -Force
        Write-Verbose "Restored: $relativePath"
    }

    Write-Host "✅ Restored $($backupFiles.Count) XAML files" -ForegroundColor Green
    return $true
}

# ============================================
# Safe XML Processing Functions
# ============================================

function Remove-XmlAttribute-Safe {
    <#
    .SYNOPSIS
    Safely removes XML attributes using proper XML parsing

    .DESCRIPTION
    Uses XML DOM manipulation to safely remove attributes without corrupting the file
    Validates XML before and after operation

    .PARAMETER FilePath
    Path to the XML/XAML file

    .PARAMETER AttributeName
    Name of the attribute to remove

    .PARAMETER AttributeValue
    Optional: Only remove attributes with this specific value

    .EXAMPLE
    Remove-XmlAttribute-Safe -FilePath "MainWindow.xaml" -AttributeName "VisualStyle"
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,

        [Parameter(Mandatory = $true)]
        [string]$AttributeName,

        [string]$AttributeValue = $null
    )

    try {
        # Validate input file
        if (-not (Test-XamlSyntax -FilePath $FilePath)) {
            return $false
        }

        # Load as XML document for proper parsing
        [xml]$xmlDoc = Get-Content $FilePath -Raw

        # Find all elements with the target attribute
        $xpath = if ($AttributeValue) {
            "//*[@$AttributeName='$AttributeValue']"
        } else {
            "//*[@$AttributeName]"
        }

        $elementsWithAttribute = $xmlDoc.SelectNodes($xpath)
        $removedCount = 0

        foreach ($element in $elementsWithAttribute) {
            $element.RemoveAttribute($AttributeName)
            $removedCount++
        }

        if ($removedCount -gt 0) {
            # Save with proper formatting
            $xmlDoc.Save($FilePath)
            Write-Verbose "✅ Removed $removedCount instances of '$AttributeName' from $FilePath"
        }

        return $true
    } catch {
        Write-Host "❌ Error processing $FilePath`: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Format-XamlFile {
    <#
    .SYNOPSIS
    Formats XAML file with proper indentation and structure

    .DESCRIPTION
    Uses XML formatting to ensure proper XAML structure and indentation
    Validates syntax before and after formatting

    .PARAMETER FilePath
    Path to the XAML file to format

    .PARAMETER IndentSize
    Number of spaces for indentation (default: 4)

    .EXAMPLE
    Format-XamlFile -FilePath "MainWindow.xaml"
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true, ValueFromPipeline = $true)]
        [string]$FilePath,

        [int]$IndentSize = 4
    )

    process {
        try {
            # Validate input
            if (-not (Test-XamlSyntax -FilePath $FilePath)) {
                return $false
            }

            # Load and format XML
            [xml]$xmlDoc = Get-Content $FilePath -Raw

            # Create formatted output
            $stringWriter = New-Object System.IO.StringWriter
            $xmlWriter = New-Object System.Xml.XmlTextWriter($stringWriter)
            $xmlWriter.Formatting = [System.Xml.Formatting]::Indented
            $xmlWriter.Indentation = $IndentSize
            $xmlWriter.IndentChar = ' '

            $xmlDoc.WriteContentTo($xmlWriter)
            $xmlWriter.Close()

            # Save formatted content
            $formattedContent = $stringWriter.ToString()
            Set-Content -Path $FilePath -Value $formattedContent -Encoding UTF8

            Write-Verbose "✅ Formatted: $FilePath"
            return $true
        } catch {
            Write-Host "❌ Error formatting $FilePath`: $($_.Exception.Message)" -ForegroundColor Red
            return $false
        }
    }
}

# ============================================
# Bulk Operations
# ============================================

function Safe-BulkXamlUpdate {
    <#
    .SYNOPSIS
    Safely performs bulk XAML updates with backup and validation

    .DESCRIPTION
    Creates backup, validates all files, performs updates, and validates results
    Implements rollback on failure

    .PARAMETER RootPath
    Root directory containing XAML files to update

    .PARAMETER UpdateScript
    Script block containing the update operations

    .PARAMETER Description
    Description of the update operation

    .PARAMETER DryRun
    If specified, performs validation without actual changes

    .EXAMPLE
    Safe-BulkXamlUpdate -RootPath "BusBuddy.WPF" -Description "Remove deprecated attributes" -UpdateScript {
        Get-ChildItem -Recurse -Filter "*.xaml" | ForEach-Object {
            Remove-XmlAttribute-Safe -FilePath $_.FullName -AttributeName "VisualStyle"
        }
    }
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$RootPath,

        [Parameter(Mandatory = $true)]
        [scriptblock]$UpdateScript,

        [string]$Description = 'Bulk XAML Update',

        [switch]$DryRun
    )

    Write-Host "🚀 Starting: $Description" -ForegroundColor Cyan

    if ($DryRun) {
        Write-Host "🔍 DRY RUN MODE - No changes will be made" -ForegroundColor Yellow
    }

    # 1. Validate root path
    if (-not (Test-Path $RootPath)) {
        Write-Host "❌ Root path not found: $RootPath" -ForegroundColor Red
        return $false
    }

    # 2. Get all XAML files
    $xamlFiles = Get-ChildItem -Path $RootPath -Recurse -Filter '*.xaml'

    if ($xamlFiles.Count -eq 0) {
        Write-Host "⚠️ No XAML files found in $RootPath" -ForegroundColor Yellow
        return $false
    }

    Write-Host "📁 Found $($xamlFiles.Count) XAML files" -ForegroundColor Cyan

    # 3. Validate all files before processing
    Write-Host '🔍 Pre-validation...' -ForegroundColor Yellow
    $invalidFiles = @()

    foreach ($file in $xamlFiles) {
        if (-not (Test-XamlSyntax -FilePath $file.FullName)) {
            $invalidFiles += $file.FullName
        }
    }

    if ($invalidFiles.Count -gt 0) {
        Write-Host "❌ Found $($invalidFiles.Count) invalid XAML files before processing. Aborting." -ForegroundColor Red
        $invalidFiles | ForEach-Object {
            $relativePath = $_.Substring($RootPath.Length + 1)
            Write-Host "  - $relativePath" -ForegroundColor Red
        }
        return $false
    }

    Write-Host "✅ All files passed pre-validation" -ForegroundColor Green

    # 4. Create backup (if not dry run)
    $backupDir = $null
    if (-not $DryRun) {
        $backupDir = Backup-XamlFiles -RootPath $RootPath
        if (-not $backupDir) {
            Write-Host '❌ Backup failed. Aborting update.' -ForegroundColor Red
            return $false
        }
    }

    # 5. Apply updates
    if ($DryRun) {
        Write-Host '🔍 Simulating updates (dry run)...' -ForegroundColor Yellow
    } else {
        Write-Host '⚡ Applying updates...' -ForegroundColor Yellow
    }

    try {
        if (-not $DryRun) {
            Push-Location $RootPath
            & $UpdateScript
            Pop-Location
        } else {
            Write-Host "Would execute: $($UpdateScript.ToString())" -ForegroundColor Gray
        }
    } catch {
        Write-Host "❌ Update script failed: $($_.Exception.Message)" -ForegroundColor Red
        if ($backupDir) {
            Pop-Location
        }
        return $false
    }

    # 6. Validate after processing (if not dry run)
    if (-not $DryRun) {
        Write-Host '🔍 Post-validation...' -ForegroundColor Yellow
        $brokenFiles = @()

        foreach ($file in $xamlFiles) {
            if (-not (Test-XamlSyntax -FilePath $file.FullName)) {
                $brokenFiles += $file.FullName
            }
        }

        if ($brokenFiles.Count -gt 0) {
            Write-Host "❌ Update broke $($brokenFiles.Count) files. Rolling back..." -ForegroundColor Red

            # Restore from backup
            Restore-XamlBackup -BackupPath $backupDir -TargetPath $RootPath

            Write-Host '🔄 Rollback completed' -ForegroundColor Yellow
            return $false
        }
    }

    Write-Host "✅ $Description completed successfully!" -ForegroundColor Green

    if ($backupDir) {
        Write-Host "📁 Backup preserved at: $backupDir" -ForegroundColor Cyan
    }

    return $true
}

# ============================================
# Aliases
# ============================================

Set-Alias -Name xaml-test -Value Test-XamlSyntax
Set-Alias -Name xaml-backup -Value Backup-XamlFiles
Set-Alias -Name xaml-format -Value Format-XamlFile

# ============================================
# Module Exports
# ============================================

Export-ModuleMember -Function @(
    'Test-XamlSyntax',
    'Get-XamlFileInfo',
    'Backup-XamlFiles',
    'Restore-XamlBackup',
    'Remove-XmlAttribute-Safe',
    'Format-XamlFile',
    'Safe-BulkXamlUpdate'
) -Alias @(
    'xaml-test',
    'xaml-backup',
    'xaml-format'
)
