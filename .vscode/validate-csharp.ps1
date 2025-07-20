# C# Corruption Detection Script for VS Code Task Integration
# This script is called by the "🔍 Full Project Validation Suite" task

param(
    [string]$ProjectPath = $PWD
)

Write-Host '🎯 C# CORRUPTION DETECTION: Analyzing source files for corruption patterns...' -ForegroundColor Cyan

try {
    $issues = @()
    $csFiles = Get-ChildItem -Path $ProjectPath -Filter "*.cs" -Recurse | Where-Object {
        $_.FullName -notmatch '\\(bin|obj|packages)\\'
    }

    Write-Host "📁 Found $($csFiles.Count) C# files to analyze..." -ForegroundColor White

    foreach ($file in $csFiles) {
        try {
            $content = Get-Content $file.FullName -Raw
            $fileName = $file.Name
            $relativePath = $file.FullName.Replace($ProjectPath, "").TrimStart('\')

            # Check for WinForms references in WPF project
            if ($content -match 'using\s+System\.Windows\.Forms') {
                $regexMatches = [regex]::Matches($content, 'using\s+System\.Windows\.Forms')
                foreach ($match in $regexMatches) {
                    $lineNumber = ($content.Substring(0, $match.Index) -split "`n").Count
                    $issues += "$relativePath($lineNumber)`: WinForms reference in WPF project"
                    Write-Host "⚠️ $fileName($lineNumber)`: WinForms reference detected" -ForegroundColor Yellow
                }
            }

            # Check for Console.WriteLine usage (should use Serilog)
            if ($content -match 'Console\.WriteLine') {
                $regexMatches2 = [regex]::Matches($content, 'Console\.WriteLine')
                foreach ($match in $regexMatches2) {
                    $lineNumber = ($content.Substring(0, $match.Index) -split "`n").Count
                    $issues += "$relativePath($lineNumber)`: Console.WriteLine should use Serilog Logger"
                    Write-Host "⚠️ $fileName($lineNumber)`: Use Logger instead of Console.WriteLine" -ForegroundColor Yellow
                }
            }

            # Check for Debug.WriteLine usage (should use Serilog)
            if ($content -match 'Debug\.WriteLine') {
                $regexMatches3 = [regex]::Matches($content, 'Debug\.WriteLine')
                foreach ($match in $regexMatches3) {
                    $lineNumber = ($content.Substring(0, $match.Index) -split "`n").Count
                    $issues += "$relativePath($lineNumber)`: Debug.WriteLine should use Serilog Logger"
                    Write-Host "⚠️ $fileName($lineNumber)`: Use Logger.Debug() instead of Debug.WriteLine" -ForegroundColor Yellow
                }
            }

            # Check for Trace.WriteLine usage (should use Serilog)
            if ($content -match 'Trace\.WriteLine') {
                $regexMatches4 = [regex]::Matches($content, 'Trace\.WriteLine')
                foreach ($match in $regexMatches4) {
                    $lineNumber = ($content.Substring(0, $match.Index) -split "`n").Count
                    $issues += "$relativePath($lineNumber)`: Trace.WriteLine should use Serilog Logger"
                    Write-Host "⚠️ $fileName($lineNumber)`: Use Logger.Verbose() instead of Trace.WriteLine" -ForegroundColor Yellow
                }
            }

            # Check for null-forgiving operator usage
            if ($content -match '\bnull!') {
                $regexMatches5 = [regex]::Matches($content, '\bnull!')
                foreach ($match in $regexMatches5) {
                    $lineNumber = ($content.Substring(0, $match.Index) -split "`n").Count
                    $issues += "$relativePath($lineNumber)`: Null-forgiving operator usage detected"
                    Write-Host "⚠️ $fileName($lineNumber)`: Review null-forgiving operator usage" -ForegroundColor Yellow
                }
            }

            # Check for auto-properties that could use ObservableProperty
            if ($content -match 'public\s+(?!class|interface|enum|struct|delegate)\w+\s+\w+\s*{\s*get;\s*set;\s*}' -and $content -notmatch '\[ObservableProperty\]') {
                $regexMatches6 = [regex]::Matches($content, 'public\s+(?!class|interface|enum|struct|delegate)(\w+\s+\w+)\s*{\s*get;\s*set;\s*}')
                foreach ($match in $regexMatches6) {
                    $lineNumber = ($content.Substring(0, $match.Index) -split "`n").Count
                    $propertyDeclaration = $match.Groups[1].Value
                    Write-Host "ℹ️ $fileName($lineNumber)`: Consider ObservableProperty for: $propertyDeclaration" -ForegroundColor Blue
                }
            }

            # Check for missing async/await patterns
            if ($content -match '\bTask\b.*\{[^}]*\breturn\b[^;]*;[^}]*\}' -and $content -notmatch '\basync\b') {
                $regexMatches7 = [regex]::Matches($content, '(\w+.*Task.*\{[^}]*return[^;]*;[^}]*\})')
                foreach ($match in $regexMatches7) {
                    $lineNumber = ($content.Substring(0, $match.Index) -split "`n").Count
                    Write-Host "ℹ️ $fileName($lineNumber)`: Consider async/await pattern for Task-returning method" -ForegroundColor Blue
                }
            }

            # Check for hardcoded connection strings
            if ($content -match '(Server|Data Source|Initial Catalog|Integrated Security)=') {
                $regexMatches8 = [regex]::Matches($content, '(Server|Data Source|Initial Catalog|Integrated Security)=')
                foreach ($match in $regexMatches8) {
                    $lineNumber = ($content.Substring(0, $match.Index) -split "`n").Count
                    $issues += "$relativePath($lineNumber)`: Potential hardcoded connection string detected"
                    Write-Host "⚠️ $fileName($lineNumber)`: Avoid hardcoded connection strings" -ForegroundColor Yellow
                }
            }

            # Check for missing IDisposable implementation on classes with unmanaged resources
            if ($content -match '\bclass\s+\w+' -and $content -match '\b(FileStream|SqlConnection|HttpClient|Timer)\b' -and $content -notmatch '\bIDisposable\b') {
                $classMatches = [regex]::Matches($content, '\bclass\s+(\w+)')
                foreach ($match in $classMatches) {
                    $lineNumber = ($content.Substring(0, $match.Index) -split "`n").Count
                    $className = $match.Groups[1].Value
                    Write-Host "ℹ️ $fileName($lineNumber)`: Class '$className' may need IDisposable implementation" -ForegroundColor Blue
                }
            }

            # Basic syntax validation - check for common corruption patterns
            $lines = $content -split "`n"
            for ($i = 0; $i -lt $lines.Count; $i++) {
                $line = $lines[$i]
                $lineNum = $i + 1

                # Check for unmatched braces in simple cases
                $openBraces = ($line -split '\{').Count - 1
                $closeBraces = ($line -split '\}').Count - 1
                if ($openBraces -gt 0 -or $closeBraces -gt 0) {
                    if ($line -match '\{[^}]*$' -and $line -notmatch '^\s*(if|else|for|while|using|try|catch|finally|switch|class|namespace|method)') {
                        # Potential incomplete brace - would need more sophisticated analysis
                    }
                }

                # Check for obvious syntax errors
                if ($line -match ';\s*;') {
                    Write-Host "⚠️ $fileName($lineNum)`: Double semicolon detected" -ForegroundColor Yellow
                }

                if ($line -match '\w+\s*\(\s*\)\s*\{' -and $line -notmatch '(if|while|for|switch|using|try|catch|finally)') {
                    # Method declaration without access modifier - might be corruption
                    if ($line -notmatch '(public|private|protected|internal|static|override|virtual|abstract)') {
                        Write-Host "ℹ️ $fileName($lineNum)`: Method without access modifier" -ForegroundColor Blue
                    }
                }
            }

            if ($issues.Where({$_ -match [regex]::Escape($relativePath)}).Count -eq 0) {
                Write-Host "✅ $fileName`: No corruption patterns detected" -ForegroundColor Green
            }

        } catch {
            $issues += "$($file.Name)`: Analysis error: $($_.Exception.Message)"
            Write-Host "❌ $($file.Name)`: Analysis error: $($_.Exception.Message)" -ForegroundColor Red
        }
    }

    # Output summary
    if ($issues.Count -eq 0) {
        Write-Host '🎉 No C# corruption patterns detected!' -ForegroundColor Green
    } else {
        Write-Host "🔧 Found $($issues.Count) potential C# issues:" -ForegroundColor Yellow

        # Group issues by severity
        $criticalIssues = $issues | Where-Object { $_ -match "(connection string|WinForms reference)" }
        $warningIssues = $issues | Where-Object { $_ -match "(Console\.WriteLine|Debug\.WriteLine|Trace\.WriteLine|null!)" }
        $infoIssues = $issues | Where-Object { $_ -notmatch "(connection string|WinForms reference|Console\.WriteLine|Debug\.WriteLine|Trace\.WriteLine|null!)" }

        if ($criticalIssues.Count -gt 0) {
            Write-Host "`n🚨 CRITICAL ISSUES ($($criticalIssues.Count)):" -ForegroundColor Red
            $criticalIssues | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
        }

        if ($warningIssues.Count -gt 0) {
            Write-Host "`n⚠️ WARNINGS ($($warningIssues.Count)):" -ForegroundColor Yellow
            $warningIssues | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
        }

        if ($infoIssues.Count -gt 0) {
            Write-Host "`nℹ️ SUGGESTIONS ($($infoIssues.Count)):" -ForegroundColor Blue
            $infoIssues | ForEach-Object { Write-Host "  $_" -ForegroundColor Blue }
        }

        Write-Host "`n💡 Review coding standards and follow project guidelines." -ForegroundColor Cyan

        # Exit with error code if we have critical issues
        if ($criticalIssues.Count -gt 0) {
            Write-Host "`n🛑 Fix critical issues before proceeding." -ForegroundColor Red
            exit 1
        }
    }

} catch {
    Write-Host "❌ C# corruption detection failed: $_" -ForegroundColor Red
    exit 1
}

exit 0
