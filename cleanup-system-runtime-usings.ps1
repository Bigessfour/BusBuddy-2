#Requires -Version 7.5
<#
.SYNOPSIS
Cleans up unnecessary System.Runtime.CompilerServices using statements
.DESCRIPTION
Removes System.Runtime.CompilerServices usings from files that don't actually use the functionality
#>

$ErrorActionPreference = "Stop"

Write-Host "🧹 Cleaning up unnecessary System.Runtime.CompilerServices usings..." -ForegroundColor Cyan

# Get all files with the using statement
$filesWithUsing = Get-ChildItem -Path . -Include "*.cs" -Recurse |
    Where-Object {
        $_.FullName -notlike "*\bin\*" -and
        $_.FullName -notlike "*\obj\*" -and
        (Get-Content $_.FullName -Raw) -match "using System\.Runtime\.CompilerServices;"
    }

$cleanedCount = 0

foreach ($file in $filesWithUsing) {
    $content = Get-Content $file.FullName -Raw

    # Check if file actually uses CallerMemberName or InlineArray attributes
    $usesCallerMemberName = $content -match "\[CallerMemberName\]"
    $usesInlineArray = $content -match "\[InlineArray\]"
    $usesCompilerGenerated = $content -match "\[CompilerGenerated\]"

    # Skip if file actually needs the using
    if ($usesCallerMemberName -or $usesInlineArray -or $usesCompilerGenerated) {
        Write-Host "✅ Keeping: $($file.Name) (uses CompilerServices attributes)" -ForegroundColor Green
        continue
    }

    # Remove the using statement
    $newContent = $content -replace "using System\.Runtime\.CompilerServices;\r?\n", ""

    if ($newContent -ne $content) {
        Set-Content -Path $file.FullName -Value $newContent -NoNewline
        Write-Host "🧹 Cleaned: $($file.Name)" -ForegroundColor Yellow
        $cleanedCount++
    }
}

Write-Host "✨ Cleanup complete! Removed unnecessary usings from $cleanedCount files." -ForegroundColor Green
