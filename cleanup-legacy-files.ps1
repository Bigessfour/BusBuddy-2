#!/usr/bin/env pwsh
#Requires -Version 7.0

<#
.SYNOPSIS
BusBuddy Legacy File Cleanup Script

.DESCRIPTION
Comprehensive cleanup script to remove legacy files that contaminate the build process.
This script follows Phase 1 cleanup priorities and preserves essential project files.

.PARAMETER WhatIf
Show what would be deleted without actually deleting files

.PARAMETER Force
Force deletion without confirmation prompts

.PARAMETER BackupFirst
Create a backup before deletion (recommended)

.EXAMPLE
./cleanup-legacy-files.ps1 -WhatIf
Show what would be cleaned up

.EXAMPLE
./cleanup-legacy-files.ps1 -BackupFirst -Force
Clean up with backup, no prompts
#>

[CmdletBinding()]
param(
    [switch]$WhatIfPreview,
    [switch]$Force,
    [switch]$BackupFirst
)

# Set strict mode for better error handling
Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

# Define cleanup categories with patterns
$CleanupCategories = @{
    'WPF Temporary Files'   = @{
        Patterns    = @(
            'BusBuddy.WPF\BusBuddy.WPF_*_wpftmp.csproj'
            'BusBuddy.WPF\obj\BusBuddy.WPF_*_wpftmp.*'
            'BusBuddy.WPF\obj\Debug\**\BusBuddy.WPF_*_wpftmp.*'
        )
        Description = 'WPF temporary project files and build artifacts'
        Priority    = 1
    }
    'Backup Files'          = @{
        Patterns    = @(
            '*.backup'
            '*.ruleset.backup'
            '*.Designer.cs.backup'
            '*.xaml.designer.cs.backup'
        )
        Description = 'Backup files created during development'
        Priority    = 2
    }
    'Temporary Build Files' = @{
        Patterns    = @(
            '**\obj\**\*.tmp'
            '**\bin\**\*.tmp'
            '**\*.tmp_proj'
            '**\*.cache'
        )
        Description = 'Temporary build and cache files'
        Priority    = 3
    }
    'Legacy Project Files'  = @{
        Patterns    = @(
            '*.old'
            '*.orig'
            '*_old.*'
            '*_backup.*'
        )
        Description = 'Legacy project files and old versions'
        Priority    = 4
    }
    'Development Artifacts' = @{
        Patterns    = @(
            '**\.vs\**'
            '**\TestResults\**\*.coverage'
            '**\TestResults\**\*.trx'
        )
        Description = 'Development environment artifacts'
        Priority    = 5
    }
}

function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = 'White'
    )

    Write-Host $Message -ForegroundColor $Color
}

function Get-LegacyFiles {
    param(
        [hashtable]$Categories
    )

    $allFiles = @()

    foreach ($categoryName in $Categories.Keys) {
        $category = $Categories[$categoryName]
        Write-ColorOutput "🔍 Scanning for $categoryName..." -Color Cyan

        foreach ($pattern in $category.Patterns) {
            try {
                $files = Get-ChildItem -Path $pattern -Recurse -Force -ErrorAction SilentlyContinue
                if ($files) {
                    $allFiles += $files | ForEach-Object {
                        [PSCustomObject]@{
                            Category    = $categoryName
                            Priority    = $category.Priority
                            Description = $category.Description
                            File        = $_
                            Size        = if ($_.PSIsContainer) { 0 } else { $_.Length }
                        }
                    }
                    Write-ColorOutput "   Found $($files.Count) files matching: $pattern" -Color Yellow
                }
            }
            catch {
                Write-Warning "Error scanning pattern '$pattern': $($_.Exception.Message)"
            }
        }
    }

    return $allFiles | Sort-Object Priority, Category, { $_.File.FullName }
}

function Format-FileSize {
    param([long]$Bytes)

    if ($Bytes -lt 1KB) { return "$Bytes B" }
    elseif ($Bytes -lt 1MB) { return "{0:N1} KB" -f ($Bytes / 1KB) }
    elseif ($Bytes -lt 1GB) { return "{0:N1} MB" -f ($Bytes / 1MB) }
    else { return "{0:N1} GB" -f ($Bytes / 1GB) }
}

function Show-CleanupSummary {
    param([array]$LegacyFiles)

    Write-ColorOutput "`n📊 Legacy File Cleanup Summary" -Color Cyan
    Write-ColorOutput "=" * 50 -Color Gray

    $totalSize = ($LegacyFiles | Measure-Object -Property Size -Sum).Sum
    $totalFiles = $LegacyFiles.Count

    Write-ColorOutput "Total legacy files found: $totalFiles" -Color White
    Write-ColorOutput "Total size to be freed: $(Format-FileSize $totalSize)" -Color Green

    # Group by category
    $grouped = $LegacyFiles | Group-Object Category | Sort-Object { $_.Group[0].Priority }

    foreach ($group in $grouped) {
        $categorySize = ($group.Group | Measure-Object -Property Size -Sum).Sum
        Write-ColorOutput "`n🗂️  $($group.Name): $($group.Count) files ($(Format-FileSize $categorySize))" -Color Yellow
        Write-ColorOutput "   $($group.Group[0].Description)" -Color Gray

        if ($group.Count -le 10) {
            foreach ($file in $group.Group) {
                $relativePath = $file.File.FullName.Replace((Get-Location).Path, '.')
                Write-ColorOutput "   📄 $relativePath ($(Format-FileSize $file.Size))" -Color DarkGray
            }
        }
        else {
            Write-ColorOutput "   📄 (First 5 of $($group.Count) files)" -Color DarkGray
            foreach ($file in ($group.Group | Select-Object -First 5)) {
                $relativePath = $file.File.FullName.Replace((Get-Location).Path, '.')
                Write-ColorOutput "   📄 $relativePath ($(Format-FileSize $file.Size))" -Color DarkGray
            }
            Write-ColorOutput "   📄 ... and $($group.Count - 5) more files" -Color DarkGray
        }
    }
}

function Backup-BeforeCleanup {
    param([array]$LegacyFiles)

    $backupDir = "legacy-backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
    $backupPath = Join-Path (Get-Location) $backupDir

    Write-ColorOutput "💾 Creating backup at: $backupPath" -Color Green
    New-Item -Path $backupPath -ItemType Directory -Force | Out-Null

    foreach ($legacyFile in $LegacyFiles) {
        if (-not $legacyFile.File.PSIsContainer) {
            $relativePath = $legacyFile.File.FullName.Replace((Get-Location).Path, '')
            $backupFilePath = Join-Path $backupPath $relativePath
            $backupFileDir = Split-Path $backupFilePath -Parent

            if (-not (Test-Path $backupFileDir)) {
                New-Item -Path $backupFileDir -ItemType Directory -Force | Out-Null
            }

            Copy-Item -Path $legacyFile.File.FullName -Destination $backupFilePath -Force
        }
    }

    Write-ColorOutput "✅ Backup created successfully" -Color Green
    return $backupPath
}

function Remove-LegacyFiles {
    param(
        [array]$LegacyFiles,
        [switch]$Force
    )

    $deleted = 0
    $totalSize = 0
    $errors = @()

    foreach ($legacyFile in $LegacyFiles) {
        try {
            $relativePath = $legacyFile.File.FullName.Replace((Get-Location).Path, '.')

            if ($WhatIfPreview) {
                Write-ColorOutput "Would delete: $relativePath" -Color Yellow
                continue
            }

            if (-not $Force) {
                $response = Read-Host "Delete $relativePath? (y/N/a=all)"
                if ($response -eq 'a') {
                    $Force = $true
                }
                elseif ($response -ne 'y') {
                    continue
                }
            }

            Remove-Item -Path $legacyFile.File.FullName -Force -Recurse
            Write-ColorOutput "🗑️  Deleted: $relativePath" -Color Red
            $deleted++
            $totalSize += $legacyFile.Size
        }
        catch {
            $errors += "Failed to delete $($legacyFile.File.FullName): $($_.Exception.Message)"
            Write-Warning "Failed to delete: $relativePath"
        }
    }

    return @{
        DeletedCount = $deleted
        TotalSize    = $totalSize
        Errors       = $errors
    }
}

function main {
    Write-ColorOutput "🧹 BusBuddy Legacy File Cleanup Script" -Color Cyan
    Write-ColorOutput "=" * 50 -Color Gray

    # Verify we're in the right directory
    if (-not (Test-Path "BusBuddy.sln")) {
        Write-Error "❌ This script must be run from the BusBuddy root directory"
        exit 1
    }

    # Scan for legacy files
    Write-ColorOutput "`n🔍 Scanning for legacy files..." -Color Cyan
    $legacyFiles = Get-LegacyFiles -Categories $CleanupCategories

    if ($legacyFiles.Count -eq 0) {
        Write-ColorOutput "✅ No legacy files found! Your project is clean." -Color Green
        return
    }

    # Show summary
    Show-CleanupSummary -LegacyFiles $legacyFiles

    # Create backup if requested
    if ($BackupFirst -and -not $WhatIfPreview) {
        $backupPath = Backup-BeforeCleanup -LegacyFiles $legacyFiles
        Write-ColorOutput "`n💾 Backup created at: $backupPath" -Color Green
    }

    # Confirm deletion
    if (-not $WhatIfPreview -and -not $Force) {
        Write-ColorOutput "`n⚠️  WARNING: This will permanently delete $($legacyFiles.Count) files" -Color Red
        $confirm = Read-Host "Continue with deletion? (y/N)"
        if ($confirm -ne 'y') {
            Write-ColorOutput "❌ Cleanup cancelled by user" -Color Yellow
            return
        }
    }

    # Perform cleanup
    Write-ColorOutput "`n🗑️  Starting cleanup..." -Color Cyan
    $result = Remove-LegacyFiles -LegacyFiles $legacyFiles -Force:$Force

    # Show results
    Write-ColorOutput "`n📊 Cleanup Results" -Color Cyan
    Write-ColorOutput "=" * 30 -Color Gray

    if ($WhatIfPreview) {
        Write-ColorOutput "Would delete: $($legacyFiles.Count) files" -Color Yellow
        Write-ColorOutput "Would free: $(Format-FileSize ($legacyFiles | Measure-Object -Property Size -Sum).Sum)" -Color Yellow
    }
    else {
        Write-ColorOutput "Files deleted: $($result.DeletedCount)" -Color Green
        Write-ColorOutput "Space freed: $(Format-FileSize $result.TotalSize)" -Color Green

        if ($result.Errors.Count -gt 0) {
            Write-ColorOutput "`n❌ Errors encountered:" -Color Red
            foreach ($error in $result.Errors) {
                Write-ColorOutput "   $error" -Color Red
            }
        }
    }

    Write-ColorOutput "`n✅ Legacy file cleanup completed!" -Color Green

    # Recommend next steps
    Write-ColorOutput "`n💡 Recommended next steps:" -Color Cyan
    Write-ColorOutput "   1. Run 'dotnet clean BusBuddy.sln'" -Color White
    Write-ColorOutput "   2. Run 'dotnet restore BusBuddy.sln'" -Color White
    Write-ColorOutput "   3. Run 'dotnet build BusBuddy.sln'" -Color White
    Write-ColorOutput "   4. Test application functionality" -Color White
}

# Execute main function
try {
    main
}
catch {
    Write-Error "❌ Script failed: $($_.Exception.Message)"
    exit 1
}
