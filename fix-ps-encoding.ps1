#Requires -Version 7.0
<#
.SYNOPSIS
    Fix UTF - 8 BOM encoding for PowerShell files
.DESCRIPTION
    This script fixes the UTF - 8 BOM encoding issue identified by PSScriptAnalyzer
    for all PowerShell files in the Bus Buddy project.
#>

Write-Output - Host "🔧 Starting PowerShell file encoding fix..." -ForegroundColor Cyan

# Get all PowerShell files recursively
$psFiles = Get - ChildItem -Path "." -Recurse -Filter "*.ps1" -ErrorAction SilentlyContinue

Write-Output - Host "📁 Found $($psFiles.Count) PowerShell files to process" -ForegroundColor Yellow

$fixed = 0
$skipped = 0
$errors = 0

ForEach-Object ($file in $psFiles) {
    try {
        # Read the current content
        $content = Get - Content $file.FullName -Raw -ErrorAction Stop

        if ([string]::IsNullOrWhiteSpace($content)) {
            Write-Output - Host "⚠️  Skipped empty file: $($file.Name)" -ForegroundColor Yellow
            $skipped++
            continue
        }

        # Create UTF - 8 encoding with BOM
        $utf8BOM = New - Object System.Text.UTF8Encoding $true

        # Write-Output the content back with UTF - 8 BOM encoding
        [System.IO.File]::WriteAllText($file.FullName, $content, $utf8BOM)

        Write-Output - Host "✅ Fixed encoding: $($file.Name)" -ForegroundColor Green
        $fixed++

    } catch {
        Write-Output - Host "❌ Error processing $($file.Name): $_" -ForegroundColor Red
        $errors++
    }
}

Write-Output - Host "`n📊 Encoding Fix Summary:" -ForegroundColor Cyan
Write-Output - Host "  ✅ Files fixed: $fixed" -ForegroundColor Green
Write-Output - Host "  ⚠️  Files skipped: $skipped" -ForegroundColor Yellow
Write-Output - Host "  ❌ Errors: $errors" -ForegroundColor Red

if ($fixed -gt 0) {
    Write-Output - Host "`n🎉 UTF - 8 BOM encoding fix completed successfully!" -ForegroundColor Green
    Write-Output - Host "   Re - run PSScriptAnalyzer to verify the encoding warnings are resolved." -ForegroundColor Info
} else {
    Write-Output - Host "`n⚠️  No files were fixed. Check for errors above." -ForegroundColor Yellow
}
