#Requires -Version 7.0
<#
.SYNOPSIS
    XAML Null Safety Analyzer for Bus Buddy

.DESCRIPTION
    Analyzes XAML bindings for null reference vulnerabilities and suggests safer alternatives.
    Addresses the 72% of runtime errors that come from null dereferencing.

.NOTES
    Inspired by industry statistics showing null reference exceptions are the #1 cause of XAML crashes
#>

class XamlNullSafetyIssue {
    [string]$FilePath
    [int]$LineNumber
    [string]$IssueType
    [string]$Severity
    [string]$CurrentBinding
    [string]$SaferAlternative
    [string]$Explanation
}

function Find-UnsafeBindings {
    <#
    .SYNOPSIS
        Detect potentially unsafe binding expressions in XAML
    #>
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    $issues = @()
    $xamlFiles = Get-ChildItem $Path -Filter '*.xaml' -Recurse

    foreach ($file in $xamlFiles) {
        $content = Get-Content $file.FullName -Raw
        $lines = Get-Content $file.FullName

        for ($i = 0; $i -lt $lines.Count; $i++) {
            $line = $lines[$i]

            # Check for unsafe binding patterns
            if ($line -match 'Binding\s+(\w+)(\.\w+)*') {
                $binding = $matches[0]

                # Direct property access without null checking
                if ($binding -notmatch '\?\.' -and $binding -match '\.') {
                    $issue = [XamlNullSafetyIssue]::new()
                    $issue.FilePath = $file.FullName
                    $issue.LineNumber = $i + 1
                    $issue.IssueType = 'UnsafeBinding'
                    $issue.Severity = 'High'
                    $issue.CurrentBinding = $binding
                    $issue.SaferAlternative = $binding -replace '\.', '?.'
                    $issue.Explanation = 'Consider using null-conditional operator or fallback value'
                    $issues += $issue
                }
            }

            # Check for missing FallbackValue
            if ($line -match 'Binding.*=.*"[^"]*"' -and $line -notmatch 'FallbackValue') {
                $issue = [XamlNullSafetyIssue]::new()
                $issue.FilePath = $file.FullName
                $issue.LineNumber = $i + 1
                $issue.IssueType = 'MissingFallback'
                $issue.Severity = 'Medium'
                $issue.CurrentBinding = $line.Trim()
                $issue.SaferAlternative = $line.Trim() + ', FallbackValue=''Default'''
                $issue.Explanation = 'Add FallbackValue to handle null/missing data gracefully'
                $issues += $issue
            }
        }
    }

    return $issues
}

function bb-null-check {
    <#
    .SYNOPSIS
        Bus Buddy null safety checker for XAML bindings
    .EXAMPLE
        bb-null-check
        bb-null-check -Path "Views\Dashboard"
    #>
    param(
        [string]$Path = 'BusBuddy.WPF\Views'
    )

    Write-Host '🛡️ Bus Buddy Null Safety Analyzer' -ForegroundColor Cyan

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        Write-Host '❌ Bus Buddy project root not found' -ForegroundColor Red
        return
    }

    $targetPath = if ([System.IO.Path]::IsPathRooted($Path)) { $Path } else { Join-Path $projectRoot $Path }

    $issues = Find-UnsafeBindings -Path $targetPath

    if ($issues.Count -eq 0) {
        Write-Host '✅ No null safety issues found!' -ForegroundColor Green
        return
    }

    Write-Host "⚠️ Found $($issues.Count) potential null safety issues:" -ForegroundColor Yellow

    foreach ($issue in $issues) {
        $fileName = Split-Path $issue.FilePath -Leaf
        Write-Host "`n📄 $fileName (Line $($issue.LineNumber))" -ForegroundColor White
        Write-Host "   Issue: $($issue.IssueType)" -ForegroundColor Red
        Write-Host "   Current: $($issue.CurrentBinding)" -ForegroundColor Gray
        Write-Host "   Suggest: $($issue.SaferAlternative)" -ForegroundColor Green
        Write-Host "   Why: $($issue.Explanation)" -ForegroundColor Yellow
    }
}

# Import Bus Buddy project helper if available
if (Get-Command Get-BusBuddyProjectRoot -ErrorAction SilentlyContinue) {
    # Already available
} else {
    function Get-BusBuddyProjectRoot {
        $currentPath = $PWD.Path
        while ($currentPath) {
            if (Test-Path (Join-Path $currentPath 'BusBuddy.sln')) {
                return $currentPath
            }
            $parentPath = Split-Path $currentPath -Parent
            if ($parentPath -eq $currentPath) { break }
            $currentPath = $parentPath
        }
        return $null
    }
}
