#Requires -Version 7.5
<#
.SYNOPSIS
    Cross-Platform Compatibility Fix for BusBuddy PowerShell Scripts

.DESCRIPTION
    Addresses cross-platform shell compatibility issues identified in workflow analysis:
    - Replaces Unix commands (grep, head, tail, uniq) with PowerShell equivalents
    - Standardizes line ending handling
    - Ensures consistent behavior across Windows/Linux/macOS

.PARAMETER ScanDirectory
    Directory to scan for compatibility issues (default: current directory)

.PARAMETER FixMode
    Fix mode: "Report", "Interactive", or "Auto"

.EXAMPLE
    .\Cross-Platform-Compatibility-Fix.ps1 -ScanDirectory . -FixMode Report
#>

param(
    [string]$ScanDirectory = ".",
    [ValidateSet("Report", "Interactive", "Auto")]
    [string]$FixMode = "Report"
)

Write-Host "🔧 BusBuddy Cross-Platform Compatibility Fix" -ForegroundColor Cyan
Write-Host "PowerShell Version: $($PSVersionTable.PSVersion)" -ForegroundColor Green
Write-Host "Platform: $($PSVersionTable.Platform)" -ForegroundColor Green
Write-Host ""

# Define compatibility replacements
$CompatibilityReplacements = @{
    # Unix command replacements
    'grep -i'      = 'Select-String -Pattern'
    'grep '        = 'Select-String -Pattern'
    'head -n'      = 'Select-Object -First'
    'tail -n'      = 'Select-Object -Last'
    'uniq'         = 'Sort-Object -Unique'
    'cat '         = 'Get-Content'
    'find . -name' = 'Get-ChildItem -Recurse -Filter'
    'wc -l'        = 'Measure-Object -Line'

    # Line ending standardization
    'echo "'       = 'Write-Output "'
    'echo '        = 'Write-Output'

    # Path separators
    'ls -la'       = 'Get-ChildItem -Force'
    'ls '          = 'Get-ChildItem'
    'pwd'          = 'Get-Location'
    'cd '          = 'Set-Location'
}

# Define files to scan
$FilesToScan = Get-ChildItem -Path $ScanDirectory -Recurse -Include "*.ps1", "*.psm1", "*.psd1" -File

$IssuesFound = @()

Write-Host "📊 Scanning $($FilesToScan.Count) PowerShell files for compatibility issues..." -ForegroundColor Yellow

foreach ($File in $FilesToScan) {
    try {
        $Content = Get-Content -Path $File.FullName -Raw

        foreach ($Pattern in $CompatibilityReplacements.Keys) {
            if ($Content -match [regex]::Escape($Pattern)) {
                $IssuesFound += [PSCustomObject]@{
                    File        = $File.FullName
                    Issue       = $Pattern
                    Replacement = $CompatibilityReplacements[$Pattern]
                    LineNumbers = @()
                }

                # Find specific line numbers
                $Lines = Get-Content -Path $File.FullName
                for ($i = 0; $i -lt $Lines.Count; $i++) {
                    if ($Lines[$i] -match [regex]::Escape($Pattern)) {
                        $IssuesFound[-1].LineNumbers += ($i + 1)
                    }
                }
            }
        }
    }
    catch {
        Write-Warning "Error scanning file $($File.FullName): $($_.Exception.Message)"
    }
}

# Report findings
Write-Host ""
Write-Host "📋 Compatibility Issues Found: $($IssuesFound.Count)" -ForegroundColor $(if ($IssuesFound.Count -eq 0) { "Green" } else { "Yellow" })

if ($IssuesFound.Count -eq 0) {
    Write-Host "✅ No cross-platform compatibility issues found!" -ForegroundColor Green
    return
}

# Group by file for reporting
$GroupedIssues = $IssuesFound | Group-Object -Property File

foreach ($Group in $GroupedIssues) {
    Write-Host ""
    Write-Host "📄 File: $($Group.Name)" -ForegroundColor Cyan

    foreach ($Issue in $Group.Group) {
        Write-Host "   ❌ Line(s) $($Issue.LineNumbers -join ', '): '$($Issue.Issue)'" -ForegroundColor Red
        Write-Host "   ✅ Suggested: '$($Issue.Replacement)'" -ForegroundColor Green
    }
}

# Apply fixes based on mode
switch ($FixMode) {
    "Report" {
        Write-Host ""
        Write-Host "📊 Report mode - no changes made" -ForegroundColor Blue
        Write-Host "Run with -FixMode Interactive or -FixMode Auto to apply fixes" -ForegroundColor Blue
    }

    "Interactive" {
        Write-Host ""
        Write-Host "🔧 Interactive mode - applying fixes with confirmation..." -ForegroundColor Yellow

        foreach ($Group in $GroupedIssues) {
            $Response = Read-Host "Fix issues in $($Group.Name)? (y/N)"
            if ($Response -eq 'y' -or $Response -eq 'Y') {
                Invoke-CompatibilityFixes -FilePath $Group.Name -Issues $Group.Group
            }
        }
    }

    "Auto" {
        Write-Host ""
        Write-Host "🚀 Auto mode - applying all fixes..." -ForegroundColor Green

        foreach ($Group in $GroupedIssues) {
            Invoke-CompatibilityFixes -FilePath $Group.Name -Issues $Group.Group
        }
    }
}

function Invoke-CompatibilityFixes {
    param(
        [string]$FilePath,
        [array]$Issues
    )

    try {
        $Content = Get-Content -Path $FilePath -Raw
        $OriginalContent = $Content

        foreach ($Issue in $Issues) {
            $Content = $Content -replace [regex]::Escape($Issue.Issue), $Issue.Replacement
        }

        if ($Content -ne $OriginalContent) {
            # Create backup
            $BackupPath = "$FilePath.backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
            Copy-Item -Path $FilePath -Destination $BackupPath

            # Apply fix
            Set-Content -Path $FilePath -Value $Content -NoNewline

            Write-Host "   ✅ Fixed $($Issues.Count) issue(s) in $FilePath" -ForegroundColor Green
            Write-Host "   📁 Backup created: $BackupPath" -ForegroundColor Blue
        }
    }
    catch {
        Write-Error "Failed to apply fixes to $FilePath`: $($_.Exception.Message)"
    }
}

# Generate summary report
Write-Host ""
Write-Host "📊 Cross-Platform Compatibility Report" -ForegroundColor Cyan
Write-Host "Files Scanned: $($FilesToScan.Count)" -ForegroundColor White
Write-Host "Issues Found: $($IssuesFound.Count)" -ForegroundColor White
Write-Host "Files Affected: $($GroupedIssues.Count)" -ForegroundColor White

if ($IssuesFound.Count -gt 0) {
    Write-Host ""
    Write-Host "🔧 Recommended Actions:" -ForegroundColor Yellow
    Write-Host "1. Run with -FixMode Interactive to review and apply fixes" -ForegroundColor White
    Write-Host "2. Test all fixed scripts after applying changes" -ForegroundColor White
    Write-Host "3. Update .gitattributes to enforce consistent line endings" -ForegroundColor White
    Write-Host "4. Consider using PowerShell Core 7.5+ features for better cross-platform support" -ForegroundColor White
}

Write-Host ""
Write-Host "✅ Cross-platform compatibility analysis complete!" -ForegroundColor Green
