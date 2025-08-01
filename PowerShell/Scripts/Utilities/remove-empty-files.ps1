#Requires -Version 7.5
<#
.SYNOPSIS
    Remove empty or minimal PowerShell script files from the BusBuddy project

.DESCRIPTION
    Identifies and removes PowerShell script files that are empty or contain
    only a filepath comment. Helps keep the repository clean by removing
    placeholder scripts that are no longer needed.

.PARAMETER Path
    The root directory to scan for empty PowerShell files

.PARAMETER WhatIf
    Show what would happen without actually removing files

.PARAMETER Force
    Remove files without prompting for confirmation

.PARAMETER RemoveMinimal
    Also remove files with only filepath or basic headers

.EXAMPLE
    .\remove-empty-files.ps1 -WhatIf
    Shows which files would be removed without actually deleting them

.EXAMPLE
    .\remove-empty-files.ps1 -Path "C:\Users\steve.mckitrick\Desktop\BusBuddy" -Force
    Removes all empty PowerShell files in the BusBuddy directory without prompting

.NOTES
    Author: BusBuddy Development Team
    Date: 2024-06-03
#>

[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [Parameter()]
    [string]$Path = (Get-Location).Path,

    [Parameter()]
    [switch]$Force,

    [Parameter()]
    [switch]$RemoveMinimal
)

# Define what constitutes an "empty" file
function Test-EmptyPowerShellFile {
    param (
        [Parameter(Mandatory = $true)]
        [string]$FilePath
    )

    $content = Get-Content -Path $FilePath -Raw

    # Consider truly empty files
    if ([string]::IsNullOrWhiteSpace($content)) {
        return @{
            IsEmpty = $true
            Reason = "File is completely empty"
        }
    }

    # Consider files with just the filepath comment
    if ($RemoveMinimal -and $content -match "^#\s*filepath:.*\r?\n\s*$") {
        return @{
            IsEmpty = $true
            Reason = "File contains only filepath comment"
        }
    }

    # Consider files with minimal content (filepath + empty comment)
    if ($RemoveMinimal -and $content -match "^#\s*filepath:.*\r?\n<#\r?\n#>\s*$") {
        return @{
            IsEmpty = $true
            Reason = "File contains only minimal comments"
        }
    }

    # Consider files with just a shebang line
    if ($RemoveMinimal -and $content -match "^#Requires.*\r?\n\s*$") {
        return @{
            IsEmpty = $true
            Reason = "File contains only version requirement"
        }
    }

    return @{
        IsEmpty = $false
        Reason = "File contains substantial content"
    }
}

function Remove-EmptyPowerShellFiles {
    [CmdletBinding(SupportsShouldProcess = $true)]
    param(
        [Parameter(Mandatory = $true)]
        [string]$SearchPath
    )

    # Find all PowerShell script files
    $psFiles = Get-ChildItem -Path $SearchPath -Include "*.ps1", "*.psm1" -Recurse -File

    Write-Host "🔍 Found $($psFiles.Count) PowerShell script files in $SearchPath" -ForegroundColor Cyan

    $emptyFiles = @()
    $totalSize = 0

    # Identify empty files
    foreach ($file in $psFiles) {
        $emptyCheck = Test-EmptyPowerShellFile -FilePath $file.FullName

        if ($emptyCheck.IsEmpty) {
            $emptyFiles += [PSCustomObject]@{
                FullPath = $file.FullName
                RelativePath = $file.FullName.Replace($SearchPath, "").TrimStart("\")
                Size = $file.Length
                Reason = $emptyCheck.Reason
            }
            $totalSize += $file.Length
        }
    }

    # Report findings
    Write-Host "📊 Found $($emptyFiles.Count) empty PowerShell files ($([math]::Round($totalSize / 1KB, 2)) KB)" -ForegroundColor Yellow

    if ($emptyFiles.Count -eq 0) {
        Write-Host "✅ No empty PowerShell files to remove" -ForegroundColor Green
        return
    }

    # Show files that would be removed
    Write-Host "`n📋 Files to remove:" -ForegroundColor Cyan
    $emptyFiles | ForEach-Object {
        Write-Host "  • $($_.RelativePath) ($([math]::Round($_.Size / 1KB, 2)) KB)" -ForegroundColor Gray
        Write-Host "    Reason: $($_.Reason)" -ForegroundColor DarkGray
    }

    # Confirm removal if not forced
    if (-not $Force) {
        $confirmation = Read-Host "`nDo you want to remove these $($emptyFiles.Count) files? (Y/N)"
        if ($confirmation -ne 'Y') {
            Write-Host "❌ Operation cancelled" -ForegroundColor Red
            return
        }
    }

    # Remove files
    $removedCount = 0
    foreach ($file in $emptyFiles) {
        if ($PSCmdlet.ShouldProcess($file.FullPath, "Remove empty PowerShell file")) {
            try {
                Remove-Item -Path $file.FullPath -Force
                Write-Host "✅ Removed: $($file.RelativePath)" -ForegroundColor Green
                $removedCount++
            }
            catch {
                Write-Host "❌ Failed to remove $($file.RelativePath): $_" -ForegroundColor Red
            }
        }
    }

    Write-Host "`n🧹 Removed $removedCount of $($emptyFiles.Count) empty PowerShell files" -ForegroundColor Green
    Write-Host "   Freed up: $([math]::Round($totalSize / 1KB, 2)) KB" -ForegroundColor Cyan

    # Check for empty directories and remove them if requested
    if ($Force) {
        Write-Host "`n🔍 Checking for empty directories..." -ForegroundColor Cyan
        $emptyDirs = Get-ChildItem -Path $SearchPath -Directory -Recurse |
                    Where-Object { (Get-ChildItem -Path $_.FullName -Recurse -File).Count -eq 0 } |
                    Sort-Object -Property FullName -Descending

        if ($emptyDirs.Count -gt 0) {
            Write-Host "📁 Found $($emptyDirs.Count) empty directories" -ForegroundColor Yellow

            foreach ($dir in $emptyDirs) {
                if ($PSCmdlet.ShouldProcess($dir.FullName, "Remove empty directory")) {
                    try {
                        Remove-Item -Path $dir.FullName -Force
                        Write-Host "✅ Removed empty directory: $($dir.FullName.Replace($SearchPath, '').TrimStart('\'))" -ForegroundColor Green
                    }
                    catch {
                        Write-Host "❌ Failed to remove directory $($dir.FullName.Replace($SearchPath, '').TrimStart('\')): $_" -ForegroundColor Red
                    }
                }
            }
        }
    }
}

# Execute the main function
Remove-EmptyPowerShellFiles -SearchPath $Path
