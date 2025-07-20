# XAML Corruption Detection Script for VS Code Task Integration
# This script is called by the "🔍 Full Project Validation Suite" task

param(
    [string]$ProjectPath = $PWD
)

Write-Host '🛠️ XAML CORRUPTION CHECK: Analyzing XAML files for corruption patterns...' -ForegroundColor Cyan

# Import comprehensive XAML analysis tools if available
$xamlToolsPath = Join-Path $ProjectPath "Tools\Scripts\XAML-Tools.ps1"
$xamlHealthPath = Join-Path $ProjectPath "Tools\Scripts\XAML-Health-Suite.ps1"

$hasAdvancedTools = $false

if (Test-Path $xamlToolsPath) {
    . $xamlToolsPath
    $hasAdvancedTools = $true
}

if (Test-Path $xamlHealthPath) {
    . $xamlHealthPath
    $hasAdvancedTools = $true
}

try {
    $issues = @()
    $xamlFiles = Get-ChildItem -Path $ProjectPath -Filter "*.xaml" -Recurse

    Write-Host "📁 Found $($xamlFiles.Count) XAML files to analyze..." -ForegroundColor White

    foreach ($file in $xamlFiles) {
        try {
            $content = Get-Content $file.FullName -Raw
            $fileName = $file.Name
            $relativePath = $file.FullName.Replace($ProjectPath, "").TrimStart('\')

            # Check for double-dash corruption in XML comments
            if ($content -match '--(?!>)') {
                $regexMatches = [regex]::Matches($content, '--(?!>)')
                foreach ($match in $regexMatches) {
                    $lineNumber = ($content.Substring(0, $match.Index) -split "`n").Count
                    $issues += "$relativePath($lineNumber): Contains double-dash in XML comments"
                    Write-Host "⚠️ $fileName($lineNumber): Double-dash in XML comments" -ForegroundColor Yellow
                }
            }

            # Check for empty elements that should be self-closed
            if ($content -match '<([^>/\s]+)[^>]*>\s*</\1>') {
                $regexMatches2 = [regex]::Matches($content, '<([^>/\s]+)[^>]*>\s*</\1>')
                foreach ($match in $regexMatches2) {
                    $lineNumber = ($content.Substring(0, $match.Index) -split "`n").Count
                    $elementName = $match.Groups[1].Value
                    $issues += "$relativePath($lineNumber): Empty element '$elementName' should be self-closed"
                    Write-Host "⚠️ $fileName($lineNumber): Empty element '$elementName' should be self-closed" -ForegroundColor Yellow
                }
            }

            # Check for invalid x:Name format
            if ($content -match 'x:Name\s*=\s*"[^"]*[^A-Za-z0-9_][^"]*"') {
                $regexMatches3 = [regex]::Matches($content, 'x:Name\s*=\s*"([^"]*[^A-Za-z0-9_][^"]*)"')
                foreach ($match in $regexMatches3) {
                    $lineNumber = ($content.Substring(0, $match.Index) -split "`n").Count
                    $invalidName = $match.Groups[1].Value
                    $issues += "$relativePath($lineNumber): Invalid x:Name format: '$invalidName'"
                    Write-Host "⚠️ $fileName($lineNumber): Invalid x:Name format: '$invalidName'" -ForegroundColor Yellow
                }
            }

            # Check for malformed attribute syntax
            if ($content -match '\s[A-Za-z]+\s*=\s*[^"]') {
                $regexMatches4 = [regex]::Matches($content, '\s([A-Za-z]+)\s*=\s*([^"\s>]+)')
                foreach ($match in $regexMatches4) {
                    if ($match.Groups[2].Value -notmatch '^(true|false|\d+)$') {
                        $lineNumber = ($content.Substring(0, $match.Index) -split "`n").Count
                        $attrName = $match.Groups[1].Value
                        $attrValue = $match.Groups[2].Value
                        $issues += "$relativePath($lineNumber): Unquoted attribute value: $attrName='$attrValue'"
                        Write-Host "⚠️ $fileName($lineNumber): Unquoted attribute value: $attrName='$attrValue'" -ForegroundColor Yellow
                    }
                }
            }

            # Check for mismatched tags
            try {
                $null = [xml]$content
                Write-Host "✅ $fileName`: Well-formed XML" -ForegroundColor Green
            } catch {
                $issues += "$relativePath`: XML parsing error: $($_.Exception.Message)"
                Write-Host "❌ $fileName`: XML parsing error: $($_.Exception.Message)" -ForegroundColor Red
            }

            # Use advanced tools if available
            if ($hasAdvancedTools -and (Get-Command "Test-XamlValidity" -ErrorAction SilentlyContinue)) {
                try {
                    $advancedResult = Test-XamlValidity -FilePath $file.FullName
                    if (-not $advancedResult.IsValid) {
                        $advancedResult.Issues | ForEach-Object {
                            $issues += "$relativePath`: $_"
                            Write-Host "🔍 $fileName`: $_" -ForegroundColor Magenta
                        }
                    }
                } catch {
                    # Advanced tools not fully available, continue with basic checks
                }
            }

        } catch {
            $issues += "$($file.Name): Analysis error: $($_.Exception.Message)"
            Write-Host "❌ $($file.Name): Analysis error: $($_.Exception.Message)" -ForegroundColor Red
        }
    }

    # Output summary
    if ($issues.Count -eq 0) {
        Write-Host '🎉 No XAML corruption detected!' -ForegroundColor Green
    } else {
        Write-Host "🔧 Found $($issues.Count) potential XAML issues:" -ForegroundColor Yellow

        # Group issues by type for better reporting
        $errorIssues = $issues | Where-Object { $_ -match "(parsing error|XML parsing)" }
        $warningIssues = $issues | Where-Object { $_ -notmatch "(parsing error|XML parsing)" }

        if ($errorIssues.Count -gt 0) {
            Write-Host "`n❌ ERRORS ($($errorIssues.Count)):" -ForegroundColor Red
            $errorIssues | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
        }

        if ($warningIssues.Count -gt 0) {
            Write-Host "`n⚠️ WARNINGS ($($warningIssues.Count)):" -ForegroundColor Yellow
            $warningIssues | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
        }

        # Exit with error code if we have parsing errors
        if ($errorIssues.Count -gt 0) {
            Write-Host "`n💡 Fix XML parsing errors before proceeding with development." -ForegroundColor Cyan
            exit 1
        } else {
            Write-Host "`n💡 Review and fix warnings to improve code quality." -ForegroundColor Cyan
        }
    }

} catch {
    Write-Host "❌ XAML corruption check failed: $_" -ForegroundColor Red
    exit 1
}

exit 0
