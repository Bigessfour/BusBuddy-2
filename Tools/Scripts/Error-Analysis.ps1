# Comprehensive Error Analysis for BusBuddy
# Uses READ-ONLY tools to detect and locate specific errors

param(
    [string]$TargetFile,
    [switch]$DetailedAnalysis,
    [switch]$ScanAll
)

# Import our read-only analysis tools
Import-Module "$PSScriptRoot\Read-Only-Analysis-Tools.ps1" -Force

function Show-ErrorAnalysis {
    param($Analysis, $FilePath)

    Write-Host "`n📁 File: $FilePath" -ForegroundColor Cyan
    Write-Host "=" * 80 -ForegroundColor Gray

    if ($Analysis.IsValid) {
        Write-Host "✅ No issues detected" -ForegroundColor Green
        return
    }

    # Show errors
    if ($Analysis.Errors.Count -gt 0) {
        Write-Host "`n❌ ERRORS ($($Analysis.Errors.Count)):" -ForegroundColor Red
        foreach ($errorItem in $Analysis.Errors) {
            if ($errorItem -is [string]) {
                Write-Host "  • $errorItem" -ForegroundColor Red
            } else {
                Write-Host "  • Line $($errorItem.Line), Col $($errorItem.Column): $($errorItem.Message)" -ForegroundColor Red
                if ($errorItem.Code) {
                    Write-Host "    Code: $($errorItem.Code)" -ForegroundColor DarkRed
                }
            }
        }
    }

    # Show warnings
    if ($Analysis.Warnings -and $Analysis.Warnings.Count -gt 0) {
        Write-Host "`n⚠️  WARNINGS ($($Analysis.Warnings.Count)):" -ForegroundColor Yellow
        foreach ($warning in $Analysis.Warnings) {
            if ($warning -is [string]) {
                Write-Host "  • $warning" -ForegroundColor Yellow
            } else {
                Write-Host "  • Line $($warning.Line), Col $($warning.Column): $($warning.Message)" -ForegroundColor Yellow
            }
        }
    }

    # Show brace analysis for C# files
    if ($Analysis.BraceAnalysis) {
        Write-Host "`n🔧 BRACE ANALYSIS:" -ForegroundColor Magenta
        Write-Host "  Opening braces: $($Analysis.BraceAnalysis.OpenBraces.Count)" -ForegroundColor White
        Write-Host "  Closing braces: $($Analysis.BraceAnalysis.CloseBraces.Count)" -ForegroundColor White

        if ($Analysis.BraceAnalysis.UnmatchedOpening.Count -gt 0) {
            Write-Host "  🚨 Unmatched opening braces:" -ForegroundColor Red
            foreach ($brace in $Analysis.BraceAnalysis.UnmatchedOpening) {
                Write-Host "    Line $($brace.Line): $($brace.Context)" -ForegroundColor Red
            }
        }

        if ($Analysis.BraceAnalysis.UnmatchedClosing.Count -gt 0) {
            Write-Host "  🚨 Unmatched closing braces:" -ForegroundColor Red
            foreach ($brace in $Analysis.BraceAnalysis.UnmatchedClosing) {
                Write-Host "    Line $($brace.Line): $($brace.Context)" -ForegroundColor Red
            }
        }
    }
}

function Show-PatternAnalysis {
    param($Patterns, $FilePath)

    if ($Patterns.Count -eq 0) {
        return
    }

    Write-Host "`n🔍 PATTERN ANALYSIS: $FilePath" -ForegroundColor Cyan
    Write-Host "=" * 80 -ForegroundColor Gray

    $groupedPatterns = $Patterns | Group-Object Pattern
    foreach ($group in $groupedPatterns) {
        Write-Host "`n📍 Pattern: $($group.Name)" -ForegroundColor Yellow
        foreach ($match in $group.Group) {
            Write-Host "  Line $($match.Line): $($match.Match)" -ForegroundColor White
            Write-Host "    Context: $($match.LineContent)" -ForegroundColor Gray

            if ($DetailedAnalysis) {
                Write-Host "    Full context:" -ForegroundColor DarkGray
                foreach ($contextLine in $match.Context) {
                    Write-Host "    $contextLine" -ForegroundColor DarkGray
                }
            }
        }
    }
}

function Test-SpecificFile {
    param([string]$FilePath)

    $extension = [System.IO.Path]::GetExtension($FilePath).ToLower()

    switch ($extension) {
        ".ps1" {
            Write-Host "🔍 Analyzing PowerShell file..." -ForegroundColor Cyan
            $analysis = Analyze-PowerShellSyntax -FilePath $FilePath
            Show-ErrorAnalysis -Analysis $analysis -FilePath $FilePath

            if ($DetailedAnalysis) {
                $patterns = Find-PowerShellPatterns -FilePath $FilePath
                Show-PatternAnalysis -Patterns $patterns -FilePath $FilePath
            }
        }

        ".xaml" {
            Write-Host "🔍 Analyzing XAML file..." -ForegroundColor Cyan
            $analysis = Analyze-XamlStructure -FilePath $FilePath
            Show-ErrorAnalysis -Analysis $analysis -FilePath $FilePath

            if ($DetailedAnalysis) {
                Write-Host "`n📊 XAML Structure:" -ForegroundColor Magenta
                if ($analysis.Structure) {
                    Write-Host "  Root: $($analysis.Structure.RootElement)" -ForegroundColor White
                    Write-Host "  Namespaces: $($analysis.Structure.Namespaces.Count)" -ForegroundColor White
                    foreach ($ns in $analysis.Structure.Namespaces) {
                        $prefix = if ($ns.Prefix) { $ns.Prefix } else { "(default)" }
                        Write-Host "    $prefix -> $($ns.URI)" -ForegroundColor Gray
                    }
                }
            }
        }

        ".cs" {
            Write-Host "🔍 Analyzing C# file..." -ForegroundColor Cyan
            $analysis = Analyze-CSharpSyntax -FilePath $FilePath
            Show-ErrorAnalysis -Analysis $analysis -FilePath $FilePath
        }

        default {
            Write-Host "❓ Unknown file type: $extension" -ForegroundColor Yellow
        }
    }
}

# Main execution
Write-Host "🚀 BusBuddy Error Analysis Tool" -ForegroundColor Green
Write-Host "=" * 50 -ForegroundColor Green

if ($TargetFile) {
    if (Test-Path $TargetFile) {
        Test-SpecificFile -FilePath $TargetFile
    } else {
        Write-Error "File not found: $TargetFile"
    }
} elseif ($ScanAll) {
    Write-Host "🔍 Scanning entire project..." -ForegroundColor Cyan
    $projectAnalysis = Analyze-ProjectErrors -WorkspaceFolder (Get-Location).Path

    Write-Host "`n📊 PROJECT SUMMARY:" -ForegroundColor Green
    Write-Host "Total Errors: $($projectAnalysis.TotalErrors)" -ForegroundColor $(if($projectAnalysis.TotalErrors -gt 0){'Red'}else{'Green'})
    Write-Host "Total Warnings: $($projectAnalysis.TotalWarnings)" -ForegroundColor $(if($projectAnalysis.TotalWarnings -gt 0){'Yellow'}else{'Green'})

    Write-Host "`n📂 BY CATEGORY:" -ForegroundColor Cyan
    Write-Host "PowerShell: $($projectAnalysis.Categories.PowerShell.Count) errors" -ForegroundColor White
    Write-Host "XAML: $($projectAnalysis.Categories.XAML.Count) errors" -ForegroundColor White
    Write-Host "C# Compilation: $($projectAnalysis.Categories.Compilation.Count) errors" -ForegroundColor White

    if ($DetailedAnalysis) {
        foreach ($file in $projectAnalysis.Files.Keys) {
            Show-ErrorAnalysis -Analysis $projectAnalysis.Files[$file] -FilePath $file
        }
    }
} else {
    Write-Host "Usage:" -ForegroundColor Yellow
    Write-Host "  .\Error-Analysis.ps1 -TargetFile 'path\to\file.ps1'" -ForegroundColor White
    Write-Host "  .\Error-Analysis.ps1 -ScanAll" -ForegroundColor White
    Write-Host "  .\Error-Analysis.ps1 -ScanAll -DetailedAnalysis" -ForegroundColor White
}

Write-Host "`n✅ Analysis complete!" -ForegroundColor Green
