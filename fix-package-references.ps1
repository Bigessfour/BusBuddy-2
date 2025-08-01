#Requires -Version 7.5
<#
.SYNOPSIS
    Fixes duplicate package references in BusBuddy project files
.DESCRIPTION
    This script scans project files for duplicate package references and removes them.
    It specifically targets the packages mentioned in the build warnings.
#>

[CmdletBinding()]
param(
    [Parameter()]
    [switch]$DryRun,

    [Parameter()]
    [switch]$Backup
)

Write-Host "📦 BusBuddy Package Reference Fixer" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

$ErrorActionPreference = "Stop"
$projectRoot = $PSScriptRoot

# List of problematic package references
$duplicatePackages = @(
    "Serilog",
    "Serilog.Sinks.Console",
    "Serilog.Sinks.File",
    "Serilog.Settings.Configuration",
    "Serilog.Enrichers.Thread",
    "Serilog.Enrichers.Environment",
    "Serilog.Enrichers.Process",
    "Serilog.Enrichers.ClientInfo"
)

# Projects to check
$projectFiles = @(
    "BusBuddy.Core\BusBuddy.Core.csproj",
    "BusBuddy.WPF\BusBuddy.WPF.csproj",
    "BusBuddy.Tests\BusBuddy.Tests.csproj",
    "BusBuddy.UITests\BusBuddy.UITests.csproj"
)

# Make sure we're in the project root
Push-Location $projectRoot
Write-Host "Working directory: $(Get-Location)"

function Repair-ProjectReferences {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ProjectFilePath
    )

    if (-not (Test-Path $ProjectFilePath)) {
        Write-Host "❌ Project file not found: $ProjectFilePath" -ForegroundColor Red
        return $false
    }

    Write-Host "Processing $ProjectFilePath..." -ForegroundColor Yellow

    # Create backup if requested
    if ($Backup) {
        $backupPath = "$ProjectFilePath.bak"
        Copy-Item -Path $ProjectFilePath -Destination $backupPath -Force
        Write-Host "  📄 Created backup at $backupPath" -ForegroundColor Gray
    }

    # Load project XML
    [xml]$projectXml = Get-Content -Path $ProjectFilePath
    $modified = $false

    # Keep track of packages we've already seen
    $processedPackages = @{}
    $duplicatesRemoved = 0

    # Find ItemGroup nodes that contain PackageReference elements
    $itemGroups = $projectXml.SelectNodes("//ItemGroup[PackageReference]")

    foreach ($itemGroup in $itemGroups) {
        $packagesToRemove = @()

        foreach ($packageRef in $itemGroup.PackageReference) {
            $packageId = $packageRef.Include

            if ($duplicatePackages -contains $packageId) {
                if ($processedPackages.ContainsKey($packageId)) {
                    # This is a duplicate, mark it for removal
                    $packagesToRemove += $packageRef
                    $duplicatesRemoved++
                    $modified = $true
                    Write-Host "  🔍 Found duplicate: $packageId" -ForegroundColor Yellow
                } else {
                    # First time seeing this package, keep track of it
                    $processedPackages[$packageId] = $true
                    Write-Host "  ✅ Keeping: $packageId $(if ($packageRef.Version) { "v" + $packageRef.Version })" -ForegroundColor Green
                }
            }
        }

        # Remove duplicates if not in dry-run mode
        if (-not $DryRun) {
            foreach ($packageToRemove in $packagesToRemove) {
                $itemGroup.RemoveChild($packageToRemove) | Out-Null
                Write-Host "  🗑️ Removed duplicate: $($packageToRemove.Include)" -ForegroundColor Gray
            }
        }
    }

    # Save changes if not in dry-run mode and if changes were made
    if ($modified -and (-not $DryRun)) {
        $projectXml.Save($ProjectFilePath)
        Write-Host "  💾 Saved changes to $ProjectFilePath" -ForegroundColor Green
    }

    return @{
        Modified = $modified
        DuplicatesRemoved = $duplicatesRemoved
    }
}

try {
    $totalDuplicatesRemoved = 0
    $totalFilesModified = 0

    if ($DryRun) {
        Write-Host "🔍 DRY RUN MODE - No files will be modified" -ForegroundColor Cyan
    }

    foreach ($projectFile in $projectFiles) {
        $result = Repair-ProjectReferences -ProjectFilePath $projectFile

        if ($result.Modified) {
            $totalFilesModified++
            $totalDuplicatesRemoved += $result.DuplicatesRemoved
        }
    }

    Write-Host "`n📊 Summary:" -ForegroundColor Cyan
    Write-Host "  Files processed: $($projectFiles.Count)" -ForegroundColor White
    Write-Host "  Files modified: $totalFilesModified" -ForegroundColor $(if ($totalFilesModified -gt 0) { "Yellow" } else { "Green" })
    Write-Host "  Duplicates removed: $totalDuplicatesRemoved" -ForegroundColor $(if ($totalDuplicatesRemoved -gt 0) { "Yellow" } else { "Green" })

    if ($DryRun) {
        Write-Host "`n✅ Dry run completed. Run without -DryRun to apply changes." -ForegroundColor Green
    } else {
        Write-Host "`n✅ Changes applied successfully." -ForegroundColor Green
    }

    if ($totalDuplicatesRemoved -gt 0) {
        Write-Host "`n🔄 Don't forget to restore packages after fixing references:" -ForegroundColor Cyan
        Write-Host "  dotnet restore BusBuddy.sln" -ForegroundColor White
    }
}
catch {
    Write-Host "❌ Error: $_" -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor DarkRed
}
finally {
    # Return to original directory
    Pop-Location
}
