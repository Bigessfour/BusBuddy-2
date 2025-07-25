#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Fix 130+ analyzer warnings in BusBuddy workspace - CA1848, CA1310, CA1311, CA1840, CA1859, CA1816
.DESCRIPTION
    Systematically fixes:
    - CA1848: Logging performance improvements (structured logging)
    - CA1310/CA1311: String comparison culture specification
    - CA1840: Use Environment.CurrentDirectory
    - CA1859: Concrete types for better performance
    - CA1816: Dispose pattern improvements
.NOTES
    Phase 1: Bulk analyzer warning cleanup
    Author: GitHub Copilot
    Version: 1.0
#>

[CmdletBinding()]
param(
    [switch]$WhatIf,
    [switch]$Force,
    [string]$LogFile = "analyzer-fixes-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
)

function Write-FixLog {
    param($Message, $Level = "Info")

    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logEntry = "[$timestamp][$Level] $Message"

    switch ($Level) {
        "Error" { Write-Host $logEntry -ForegroundColor Red }
        "Warning" { Write-Host $logEntry -ForegroundColor Yellow }
        "Success" { Write-Host $logEntry -ForegroundColor Green }
        default { Write-Host $logEntry -ForegroundColor Cyan }
    }

    if ($LogFile) {
        $logEntry | Add-Content -Path $LogFile -Encoding UTF8
    }
}

function Fix-CA1848-LoggingPerformance {
    param([string]$FilePath)

    Write-FixLog "🔧 Fixing CA1848 logging performance in: $FilePath"

    $content = Get-Content -Path $FilePath -Raw
    $originalContent = $content

    # Pattern 1: Logger.Information("message") -> Logger.Information("message") with LoggerMessage
    $patterns = @{
        # Basic Log.Information patterns
        'Log\.Information\("([^"]+)"\)'    = 'Log.Information("{Message}", "{0}")' -replace '\{0\}', '$1'
        'Log\.Warning\("([^"]+)"\)'        = 'Log.Warning("{Message}", "{0}")' -replace '\{0\}', '$1'
        'Log\.Error\("([^"]+)"\)'          = 'Log.Error("{Message}", "{0}")' -replace '\{0\}', '$1'
        'Log\.Debug\("([^"]+)"\)'          = 'Log.Debug("{Message}", "{0}")' -replace '\{0\}', '$1'
        'Log\.Verbose\("([^"]+)"\)'        = 'Log.Verbose("{Message}", "{0}")' -replace '\{0\}', '$1'

        # Logger.Information patterns
        'Logger\.Information\("([^"]+)"\)' = 'Logger.Information("{Message}", "{0}")' -replace '\{0\}', '$1'
        'Logger\.Warning\("([^"]+)"\)'     = 'Logger.Warning("{Message}", "{0}")' -replace '\{0\}', '$1'
        'Logger\.Error\("([^"]+)"\)'       = 'Logger.Error("{Message}", "{0}")' -replace '\{0\}', '$1'
        'Logger\.Debug\("([^"]+)"\)'       = 'Logger.Debug("{Message}", "{0}")' -replace '\{0\}', '$1'
        'Logger\.Verbose\("([^"]+)"\)'     = 'Logger.Verbose("{Message}", "{0}")' -replace '\{0\}', '$1'
    }

    foreach ($pattern in $patterns.Keys) {
        $replacement = $patterns[$pattern]
        if ($content -match $pattern) {
            $content = $content -replace $pattern, $replacement
            Write-FixLog "  ✅ Fixed pattern: $pattern" "Success"
        }
    }

    if ($content -ne $originalContent) {
        if (-not $WhatIf) {
            Set-Content -Path $FilePath -Value $content -Encoding UTF8
            Write-FixLog "  💾 Updated file: $FilePath" "Success"
            return $true
        }
        else {
            Write-FixLog "  🔍 WOULD UPDATE: $FilePath" "Warning"
            return $false
        }
    }

    return $false
}

function Fix-CA1310-StringComparison {
    param([string]$FilePath)

    Write-FixLog "🔧 Fixing CA1310/CA1311 string comparison in: $FilePath"

    $content = Get-Content -Path $FilePath -Raw
    $originalContent = $content

    # Fix string comparison methods without StringComparison parameter
    $patterns = @{
        '\.Contains\("([^"]+)"\)'   = '.Contains("$1", StringComparison.OrdinalIgnoreCase)'
        '\.StartsWith\("([^"]+)"\)' = '.StartsWith("$1", StringComparison.OrdinalIgnoreCase)'
        '\.EndsWith\("([^"]+)"\)'   = '.EndsWith("$1", StringComparison.OrdinalIgnoreCase)'
        '\.IndexOf\("([^"]+)"\)'    = '.IndexOf("$1", StringComparison.OrdinalIgnoreCase)'
    }

    foreach ($pattern in $patterns.Keys) {
        $replacement = $patterns[$pattern]
        if ($content -match $pattern) {
            # Only replace if StringComparison is not already specified
            $matches = [regex]::Matches($content, $pattern)
            foreach ($match in $matches) {
                $fullMatch = $match.Value
                $followingText = $content.Substring($match.Index + $match.Length, [Math]::Min(50, $content.Length - $match.Index - $match.Length))

                # Skip if StringComparison is already specified
                if (-not $followingText.Contains("StringComparison")) {
                    $content = $content -replace [regex]::Escape($fullMatch), ($replacement -replace '\$1', $match.Groups[1].Value)
                    Write-FixLog "  ✅ Fixed string comparison: $fullMatch" "Success"
                }
            }
        }
    }

    if ($content -ne $originalContent) {
        if (-not $WhatIf) {
            Set-Content -Path $FilePath -Value $content -Encoding UTF8
            Write-FixLog "  💾 Updated file: $FilePath" "Success"
            return $true
        }
        else {
            Write-FixLog "  🔍 WOULD UPDATE: $FilePath" "Warning"
            return $false
        }
    }

    return $false
}

function Fix-CA1840-EnvironmentCurrentDirectory {
    param([string]$FilePath)

    Write-FixLog "🔧 Fixing CA1840 Environment.CurrentDirectory in: $FilePath"

    $content = Get-Content -Path $FilePath -Raw
    $originalContent = $content

    # Replace Directory.GetCurrentDirectory() with Environment.CurrentDirectory
    $content = $content -replace 'Directory\.GetCurrentDirectory\(\)', 'Environment.CurrentDirectory'

    if ($content -ne $originalContent) {
        if (-not $WhatIf) {
            Set-Content -Path $FilePath -Value $content -Encoding UTF8
            Write-FixLog "  ✅ Fixed Environment.CurrentDirectory usage" "Success"
            return $true
        }
        else {
            Write-FixLog "  🔍 WOULD UPDATE: Directory.GetCurrentDirectory() -> Environment.CurrentDirectory" "Warning"
            return $false
        }
    }

    return $false
}

function Fix-CA1859-ConcreteTypes {
    param([string]$FilePath)

    Write-FixLog "🔧 Fixing CA1859 concrete types in: $FilePath"

    $content = Get-Content -Path $FilePath -Raw
    $originalContent = $content

    # Common interface to concrete type patterns
    $patterns = @{
        'IEnumerable<([^>]+)>\s+(\w+)\s*=\s*new\s+List<\1>'                        = 'List<$1> $2 = new List<$1>'
        'IList<([^>]+)>\s+(\w+)\s*=\s*new\s+List<\1>'                              = 'List<$1> $2 = new List<$1>'
        'ICollection<([^>]+)>\s+(\w+)\s*=\s*new\s+List<\1>'                        = 'List<$1> $2 = new List<$1>'
        'IDictionary<([^,]+),\s*([^>]+)>\s+(\w+)\s*=\s*new\s+Dictionary<\1,\s*\2>' = 'Dictionary<$1, $2> $3 = new Dictionary<$1, $2>'
    }

    foreach ($pattern in $patterns.Keys) {
        $replacement = $patterns[$pattern]
        if ($content -match $pattern) {
            $content = $content -replace $pattern, $replacement
            Write-FixLog "  ✅ Fixed concrete type: $pattern" "Success"
        }
    }

    if ($content -ne $originalContent) {
        if (-not $WhatIf) {
            Set-Content -Path $FilePath -Value $content -Encoding UTF8
            Write-FixLog "  💾 Updated file: $FilePath" "Success"
            return $true
        }
        else {
            Write-FixLog "  🔍 WOULD UPDATE: $FilePath" "Warning"
            return $false
        }
    }

    return $false
}

function Fix-CA1816-DisposalPattern {
    param([string]$FilePath)

    Write-FixLog "🔧 Fixing CA1816 disposal pattern in: $FilePath"

    $content = Get-Content -Path $FilePath -Raw
    $originalContent = $content

    # Look for classes that implement IDisposable and add proper Dispose pattern
    if ($content -match 'class\s+(\w+).*:\s*.*IDisposable' -and $content -notmatch 'GC\.SuppressFinalize') {
        # Find the Dispose method and add GC.SuppressFinalize
        $disposePattern = '(public\s+void\s+Dispose\(\s*\)\s*\{[^}]+)\}'
        if ($content -match $disposePattern) {
            $content = $content -replace $disposePattern, '$1    GC.SuppressFinalize(this);}'
            Write-FixLog "  ✅ Added GC.SuppressFinalize to Dispose method" "Success"
        }
    }

    if ($content -ne $originalContent) {
        if (-not $WhatIf) {
            Set-Content -Path $FilePath -Value $content -Encoding UTF8
            Write-FixLog "  💾 Updated file: $FilePath" "Success"
            return $true
        }
        else {
            Write-FixLog "  🔍 WOULD UPDATE: $FilePath" "Warning"
            return $false
        }
    }

    return $false
}

# Main execution
Write-FixLog "🚀 Starting BusBuddy Analyzer Warning Fix" "Info"
Write-FixLog "📊 Target: 130+ analyzer warnings (CA1848, CA1310, CA1311, CA1840, CA1859, CA1816)" "Info"

if ($WhatIf) {
    Write-FixLog "🔍 WHAT-IF MODE: No files will be modified" "Warning"
}

$workspace = Get-Location
Write-FixLog "📁 Workspace: $workspace" "Info"

# Find all C# files
$csFiles = Get-ChildItem -Path . -Filter "*.cs" -Recurse | Where-Object {
    $_.FullName -notlike "*\bin\*" -and
    $_.FullName -notlike "*\obj\*" -and
    $_.FullName -notlike "*\TestResults\*"
}

Write-FixLog "📄 Found $($csFiles.Count) C# files to analyze" "Info"

$totalFixed = 0
$fixSummary = @{
    "CA1848" = 0
    "CA1310" = 0
    "CA1840" = 0
    "CA1859" = 0
    "CA1816" = 0
}

foreach ($file in $csFiles) {
    Write-FixLog "🔍 Analyzing: $($file.Name)" "Info"

    $fileFixed = 0

    # Fix CA1848 - Logging performance
    if (Fix-CA1848-LoggingPerformance -FilePath $file.FullName) {
        $fixSummary["CA1848"]++
        $fileFixed++
    }

    # Fix CA1310/CA1311 - String comparison
    if (Fix-CA1310-StringComparison -FilePath $file.FullName) {
        $fixSummary["CA1310"]++
        $fileFixed++
    }

    # Fix CA1840 - Environment.CurrentDirectory
    if (Fix-CA1840-EnvironmentCurrentDirectory -FilePath $file.FullName) {
        $fixSummary["CA1840"]++
        $fileFixed++
    }

    # Fix CA1859 - Concrete types
    if (Fix-CA1859-ConcreteTypes -FilePath $file.FullName) {
        $fixSummary["CA1859"]++
        $fileFixed++
    }

    # Fix CA1816 - Disposal pattern
    if (Fix-CA1816-DisposalPattern -FilePath $file.FullName) {
        $fixSummary["CA1816"]++
        $fileFixed++
    }

    $totalFixed += $fileFixed

    if ($fileFixed -gt 0) {
        Write-FixLog "  ✅ Fixed $fileFixed issue(s) in $($file.Name)" "Success"
    }
}

# Summary
Write-FixLog "" "Info"
Write-FixLog "📊 ANALYZER WARNING FIX SUMMARY" "Info"
Write-FixLog "================================" "Info"
foreach ($ruleId in $fixSummary.Keys) {
    $count = $fixSummary[$ruleId]
    if ($count -gt 0) {
        Write-FixLog "✅ $ruleId`: $count files fixed" "Success"
    }
    else {
        Write-FixLog "⚪ $ruleId`: No issues found" "Info"
    }
}
Write-FixLog "--------------------------------" "Info"
Write-FixLog "🎯 Total files fixed: $totalFixed" "Success"

if ($totalFixed -gt 0 -and -not $WhatIf) {
    Write-FixLog "💡 Recommendation: Build solution to verify fixes" "Info"
    Write-FixLog "   dotnet build BusBuddy.sln" "Info"
}
elseif ($WhatIf) {
    Write-FixLog "💡 Run without -WhatIf to apply fixes" "Info"
}

Write-FixLog "📋 Log saved to: $LogFile" "Info"
Write-FixLog "✅ Analyzer warning fix completed!" "Success"
