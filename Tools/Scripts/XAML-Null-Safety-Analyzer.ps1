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
        Detect potentially unsafe binding expressions in XAML - PowerShell 7.5.2 optimized
    #>
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    $issues = [System.Collections.Generic.List[XamlNullSafetyIssue]]::new()
    $xamlFiles = Get-ChildItem $Path -Filter '*.xaml' -Recurse -ErrorAction SilentlyContinue

    # PowerShell 7.5.2 parallel processing for file analysis
    $parallelResults = $xamlFiles | ForEach-Object -Parallel {
        $file = $_
        $fileIssues = [System.Collections.Generic.List[XamlNullSafetyIssue]]::new()

        try {
            $content = Get-Content $file.FullName -Raw -ErrorAction Stop
            $lines = Get-Content $file.FullName -ErrorAction Stop

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
                        $fileIssues.Add($issue)
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
                    $fileIssues.Add($issue)
                }
            }
        } catch {
            # Log error but continue processing other files
            Write-Warning "Failed to process file $($file.FullName): $($_.Exception.Message)"
        }

        return $fileIssues.ToArray()
    } -ThrottleLimit 6

    # Flatten results from parallel processing
    foreach ($result in $parallelResults) {
        if ($result) {
            $issues.AddRange($result)
        }
    }

    return $issues.ToArray()
}

function Test-SyncfusionNamespaces {
    <#
    .SYNOPSIS
        PowerShell 7.5.2 optimized Syncfusion namespace validation
    .PARAMETER Path
        Path to XAML file or directory to validate
    #>
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    $missingNamespaces = [System.Collections.Generic.List[string]]::new()

    if (Test-Path $Path -PathType Container) {
        $xamlFiles = Get-ChildItem -Path $Path -Filter '*.xaml' -Recurse -ErrorAction SilentlyContinue
    } else {
        $xamlFiles = @(Get-Item $Path -ErrorAction SilentlyContinue | Where-Object { $_.Extension -eq '.xaml' })
    }

    # Parallel processing for large file sets
    $results = $xamlFiles | ForEach-Object -Parallel {
        $file = $_
        $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue

        if ($content -and $content -notmatch 'xmlns:syncfusion=' -and ($content -match 'sf:' -or $content -match 'syncfusion:')) {
            return $file.Name
        }
    } -ThrottleLimit 8 -ErrorAction SilentlyContinue

    if ($results) {
        $missingNamespaces.AddRange($results)
    }

    if ($missingNamespaces.Count -eq 0) {
        Write-Host '✅ All XAML files have proper Syncfusion namespaces!' -ForegroundColor Green
        return $true
    } else {
        Write-Host "❌ Found $($missingNamespaces.Count) files with missing Syncfusion namespaces:" -ForegroundColor Red
        $missingNamespaces | ForEach-Object { Write-Host "  - $_" -ForegroundColor Yellow }

        foreach ($file in $missingNamespaces) {
            throw "Missing Syncfusion namespace in $file"
        }
        return $false
    }
}

function Test-BusBuddyNullSafety {
    [CmdletBinding()
    param(
        [string]$Path = 'BusBuddy.WPF\Views'
    )

    # Replace Write-Host with proper streams
    Write-Information '🛡️ Bus Buddy Null Safety Analyzer' -Tags 'XamlNullSafety' -InformationAction Continue

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        Write-Error '❌ Bus Buddy project root not found'
        return
    }

    $targetPath = if ([System.IO.Path]::IsPathRooted($Path)) { $Path } else { Join-Path $projectRoot $Path }

    $issues = Find-UnsafeBindings -Path $targetPath

    if ($issues.Count -eq 0) {
        Write-Information '✅ No null safety issues found!' -Tags 'XamlNullSafety' -InformationAction Continue
        return
    }

    Write-Warning "⚠️ Found $($issues.Count) potential null safety issues:"

    foreach ($issue in $issues) {
        $fileName = Split-Path $issue.FilePath -Leaf
        Write-Information "`n📄 $fileName (Line $($issue.LineNumber))" -Tags 'XamlNullSafety' -InformationAction Continue
        Write-Error "   Issue: $($issue.IssueType)"
        Write-Information "   Current: $($issue.CurrentBinding)" -Tags 'XamlNullSafety' -InformationAction Continue
        Write-Information "   Suggest: $($issue.SaferAlternative)" -Tags 'XamlNullSafety' -InformationAction Continue
        Write-Warning "   Why: $($issue.Explanation)"
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


# Backward Compatibility Aliases
Set-Alias -Name 'bb-null-check' -Value 'Test-BusBuddyNullSafety' -Force

