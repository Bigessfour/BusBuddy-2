#Requires -Version 7.5

<#
.SYNOPSIS
    Fixes file encodings for PowerShell and text files
.DESCRIPTION
    Scans and fixes file encodings to ensure consistent UTF-8 encoding
.NOTES
    Author: BusBuddy Development Team
    Date: July 28, 2025
#>

param(
    [Parameter(Mandatory = $false)]
    [string]$Path = ".",

    [Parameter(Mandatory = $false)]
    [switch]$Fix,

    [Parameter(Mandatory = $false)]
    [switch]$Report,

    [Parameter(Mandatory = $false)]
    [string]$OutputFile = "encoding-scan-results.json",

    [Parameter(Mandatory = $false)]
    [switch]$ShowDetails
)# Function to detect file encoding
function Get-FileEncoding {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string]$FilePath
    )

    try {
        # Read the first few bytes to determine encoding
        $bytes = [byte[]]::new(4)
        $stream = [System.IO.File]::OpenRead($FilePath)
        $bytesRead = $stream.Read($bytes, 0, 4)
        $stream.Close()

        # Check for BOM markers
        if ($bytesRead -ge 2 -and $bytes[0] -eq 0xFE -and $bytes[1] -eq 0xFF) {
            return 'UTF-16BE'
        }
        if ($bytesRead -ge 2 -and $bytes[0] -eq 0xFF -and $bytes[1] -eq 0xFE) {
            if ($bytesRead -ge 4 -and $bytes[2] -eq 0x00 -and $bytes[3] -eq 0x00) {
                return 'UTF-32LE'
            }
            return 'UTF-16LE'
        }
        if ($bytesRead -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
            return 'UTF-8-BOM'
        }
        if ($bytesRead -ge 4 -and $bytes[0] -eq 0x00 -and $bytes[1] -eq 0x00 -and $bytes[2] -eq 0xFE -and $bytes[3] -eq 0xFF) {
            return 'UTF-32BE'
        }

        # No BOM found, try to detect based on content
        # This is a simplified approach and may not be 100% accurate
        $content = [System.IO.File]::ReadAllBytes($FilePath)

        # Check for NULL bytes which would indicate UTF-16/UTF-32
        $nullBytes = 0
        for ($i = 0; $i -lt [Math]::Min($content.Length, 100); $i++) {
            if ($content[$i] -eq 0) {
                $nullBytes++
            }
        }

        if ($nullBytes -gt 0) {
            # If we have null bytes, it's likely some form of Unicode
            return 'Binary/Unicode'
        }

        # Check if it's ASCII or UTF-8 without BOM
        $isASCII = $true
        foreach ($byte in $content) {
            if ($byte -gt 127) {
                $isASCII = $false
                break
            }
        }

        if ($isASCII) {
            return 'ASCII'
        }

        # If we got here, it's likely UTF-8 without BOM or some other encoding
        return 'UTF-8'
    }
    catch {
        Write-Error "Error detecting encoding for $FilePath`: $_"
        return 'Unknown'
    }
}

# Function to convert file to UTF-8
function ConvertTo-UTF8 {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string]$FilePath,

        [Parameter(Mandatory = $false)]
        [switch]$RemoveBOM
    )

    try {
        $content = [System.IO.File]::ReadAllText($FilePath)

        if ($RemoveBOM) {
            [System.IO.File]::WriteAllText($FilePath, $content, [System.Text.UTF8Encoding]::new($false))
            return $true
        }
        else {
            [System.IO.File]::WriteAllText($FilePath, $content, [System.Text.UTF8Encoding]::new($true))
            return $true
        }
    }
    catch {
        Write-Error "Error converting $FilePath to UTF-8: $_"
        return $false
    }
}

# Main script logic
Write-Host "🔍 Scanning for file encoding issues..." -ForegroundColor Cyan

# File types to check (PowerShell and common text files)
$fileTypes = @('*.ps1', '*.psm1', '*.psd1', '*.cs', '*.xaml', '*.xml', '*.json', '*.md', '*.txt')

# Directories to exclude
$excludeDirs = @('*\bin\*', '*\obj\*', '*\.git\*', '*\packages\*')

# Get all matching files
$files = Get-ChildItem -Path $Path -Recurse -File -Include $fileTypes |
         Where-Object {
             $file = $_.FullName
             -not ($excludeDirs | Where-Object { $file -like $_ } | Select-Object -First 1)
         }

$results = @{
    Summary = @{
        TotalFiles = $files.Count
        NonUTF8Files = 0
        FixedFiles = 0
        FailedFixes = 0
        SkippedFiles = 0
    }
    Details = @{}
}

foreach ($file in $files) {
    $encoding = Get-FileEncoding -FilePath $file.FullName
    $needsFix = ($encoding -ne 'UTF-8' -and $encoding -ne 'UTF-8-BOM' -and $encoding -ne 'ASCII')

    $results.Details[$file.FullName] = @{
        Path = $file.FullName
        RelativePath = $file.FullName.Replace((Get-Location).Path, '').TrimStart('\')
        Encoding = $encoding
        NeedsFix = $needsFix
        Fixed = $false
        Error = $null
    }

    if ($needsFix) {
        $results.Summary.NonUTF8Files++

        if ($Verbose) {
            Write-Host "Found non-UTF8 file: $($file.FullName) - $encoding" -ForegroundColor Yellow
        }

        if ($Fix) {
            Write-Host "Converting $($file.FullName) from $encoding to UTF-8..." -ForegroundColor Yellow
            $success = ConvertTo-UTF8 -FilePath $file.FullName -RemoveBOM:($encoding -eq 'UTF-8-BOM')

            if ($success) {
                $results.Details[$file.FullName].Fixed = $true
                $results.Summary.FixedFiles++
                Write-Host "✅ Fixed encoding for $($file.FullName)" -ForegroundColor Green
            }
            else {
                $results.Details[$file.FullName].Error = "Failed to convert encoding"
                $results.Summary.FailedFixes++
                Write-Host "❌ Failed to fix encoding for $($file.FullName)" -ForegroundColor Red
            }
        }
        else {
            $results.Summary.SkippedFiles++
        }
    }
    elseif ($Verbose) {
        Write-Host "✅ File already in correct encoding: $($file.FullName) - $encoding" -ForegroundColor Green
    }
}

# Display summary
Write-Host "`n📊 Encoding Scan Summary:" -ForegroundColor Cyan
Write-Host "  Total files scanned: $($results.Summary.TotalFiles)" -ForegroundColor White
Write-Host "  Files with non-UTF8 encoding: $($results.Summary.NonUTF8Files)" -ForegroundColor $(if ($results.Summary.NonUTF8Files -gt 0) { "Yellow" } else { "Green" })

if ($Fix) {
    Write-Host "  Files fixed: $($results.Summary.FixedFiles)" -ForegroundColor Green
    Write-Host "  Files failed to fix: $($results.Summary.FailedFixes)" -ForegroundColor $(if ($results.Summary.FailedFixes -gt 0) { "Red" } else { "Green" })
}
else {
    Write-Host "  Files that need fixing: $($results.Summary.NonUTF8Files)" -ForegroundColor $(if ($results.Summary.NonUTF8Files -gt 0) { "Yellow" } else { "Green" })
    if ($results.Summary.NonUTF8Files -gt 0) {
        Write-Host "`nRun with -Fix parameter to convert files to UTF-8" -ForegroundColor Yellow
    }
}

# Generate report if requested
if ($Report) {
    $reportPath = Join-Path (Get-Location) $OutputFile
    $results | ConvertTo-Json -Depth 5 | Set-Content -Path $reportPath -Encoding UTF8
    Write-Host "`n📄 Report saved to: $reportPath" -ForegroundColor Cyan
}

# Return results for PowerShell 7.5.2 compatibility validation
return $results
