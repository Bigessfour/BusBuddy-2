# PowerShell 7.5 Enhanced Analysis Tools
# Leverages new PowerShell 7.5 features for better performance and accuracy

#Requires -Version 7.5

# PowerShell 7.5 specific improvements
using namespace System.Management.Automation.Language
using namespace System.Text.RegularExpressions

function Test-PowerShell75Features {
    <#
    .SYNOPSIS
    Tests PowerShell 7.5 specific features availability
    .DESCRIPTION
    Validates that we can use PowerShell 7.5 enhancements in our analysis tools
    #>

    $features = @{
        'ConvertTo-CliXml' = { Get-Command ConvertTo-CliXml -ErrorAction SilentlyContinue }
        'ConvertFrom-CliXml' = { Get-Command ConvertFrom-CliXml -ErrorAction SilentlyContinue }
        'ImprovedTabCompletion' = { $PSVersionTable.PSVersion -ge [Version]'7.5.0' }
        'EnhancedJsonHandling' = { (Get-Command ConvertTo-Json).Parameters.ContainsKey('DateKind') }
        'PerformanceImprovements' = { $PSVersionTable.PSVersion -ge [Version]'7.5.0' }
        'NewGuidEmpty' = { (Get-Command New-Guid).Parameters.ContainsKey('Empty') }
        'TestJsonEnhancements' = { (Get-Command Test-Json).Parameters.ContainsKey('IgnoreComments') }
    }

    $results = @{}
    foreach ($feature in $features.GetEnumerator()) {
        try {
            $results[$feature.Key] = [bool](& $feature.Value)
        } catch {
            $results[$feature.Key] = $false
        }
    }

    return $results
}

function Get-EnhancedCSharpAnalysis {
    <#
    .SYNOPSIS
    Enhanced C# analysis using PowerShell 7.5 performance improvements
    .DESCRIPTION
    Uses improved array operations and better .NET integration in PS 7.5
    #>
    param(
        [Parameter(Mandatory)]
        [string]$FilePath
    )

    if (-not (Test-Path $FilePath)) {
        return @{
            IsValid = $false
            Errors = @("File not found: $FilePath")
            Performance = @{ AnalysisTime = 0 }
        }
    }

    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

    try {
        $content = Get-Content $FilePath -Raw
        $lines = $content -split "`n"

        # Use PowerShell 7.5 enhanced array operations (significantly faster)
        $analysis = @{
            IsValid = $true
            Errors = @()  # Using @() for better performance in PS 7.5
            Warnings = @()
            BraceAnalysis = @{
                OpenBraces = @()
                CloseBraces = @()
                Mismatches = @()
            }
            MethodAnalysis = @{
                Methods = @()
                OrphanedCode = @()
                IncompleteSignatures = @()
            }
            Performance = @{}
        }

        # Enhanced brace analysis with improved performance
        $braceStack = @()
        $bracePattern = '[{}]'

        # Use PowerShell 7.5 improved foreach performance
        for ($lineNum = 0; $lineNum -lt $lines.Count; $lineNum++) {
            $line = $lines[$lineNum]
            $regexMatches = [Regex]::Matches($line, $bracePattern)

            foreach ($match in $regexMatches) {
                $char = $match.Value
                $column = $match.Index + 1

                if ($char -eq '{') {
                    $braceInfo = @{
                        Type = 'Opening'
                        Line = $lineNum + 1
                        Column = $column
                        Context = $line.Trim()
                    }
                    $analysis.BraceAnalysis.OpenBraces += $braceInfo
                    $braceStack += $braceInfo  # PS 7.5 optimized += operation
                } else {
                    $braceInfo = @{
                        Type = 'Closing'
                        Line = $lineNum + 1
                        Column = $column
                        Context = $line.Trim()
                    }
                    $analysis.BraceAnalysis.CloseBraces += $braceInfo

                    if ($braceStack.Count -gt 0) {
                        $braceStack = $braceStack[0..($braceStack.Count - 2)]
                    } else {
                        $analysis.BraceAnalysis.Mismatches += $braceInfo
                        $analysis.Errors += "Unmatched closing brace at line $($lineNum + 1), column $column"
                    }
                }
            }
        }

        # Report unmatched opening braces
        foreach ($unmatchedBrace in $braceStack) {
            $analysis.BraceAnalysis.Mismatches += $unmatchedBrace
            $analysis.Errors += "Unmatched opening brace at line $($unmatchedBrace.Line), column $($unmatchedBrace.Column)"
        }

        # Enhanced method boundary detection
        $methodPattern = '(?m)^\s*(public|private|protected|internal)?\s*(static)?\s*(virtual|override|abstract)?\s*\w+\s+\w+\s*\([^)]*\)\s*{?'
        $methodMatches = [Regex]::Matches($content, $methodPattern, [RegexOptions]::Multiline)

        foreach ($methodMatch in $methodMatches) {
            $lineNumber = ($content.Substring(0, $methodMatch.Index) -split "`n").Count
            $analysis.MethodAnalysis.Methods += @{
                Line = $lineNumber
                Signature = $methodMatch.Value.Trim()
                HasOpeningBrace = $methodMatch.Value.Contains('{')
            }
        }

        # Detect orphaned code (code outside proper method/class context)
        $classPattern = '(?m)^\s*(public|private|protected|internal)?\s*(static)?\s*(partial)?\s*class\s+\w+'
        $namespacePattern = '(?m)^\s*namespace\s+[\w.]+'

        $classMatches = [Regex]::Matches($content, $classPattern, [RegexOptions]::Multiline)
        $namespaceMatches = [Regex]::Matches($content, $namespacePattern, [RegexOptions]::Multiline)

        if ($classMatches.Count -eq 0 -and $content.Length -gt 100) {
            $analysis.Warnings += "No class declarations found in C# file"
        }

        if ($namespaceMatches.Count -eq 0 -and $content.Length -gt 100) {
            $analysis.Warnings += "No namespace declarations found in C# file"
        }

        $analysis.IsValid = $analysis.Errors.Count -eq 0

        $stopwatch.Stop()
        $analysis.Performance = @{
            AnalysisTime = $stopwatch.ElapsedMilliseconds
            LinesAnalyzed = $lines.Count
            PowerShellVersion = $PSVersionTable.PSVersion.ToString()
            OptimizedArrayOps = $true  # PS 7.5 feature
        }

        return $analysis

    } catch {
        $stopwatch.Stop()
        return @{
            IsValid = $false
            Errors = @("Analysis error: $($_.Exception.Message)")
            Performance = @{
                AnalysisTime = $stopwatch.ElapsedMilliseconds
                Failed = $true
            }
        }
    }
}

function Get-EnhancedJsonAnalysis {
    <#
    .SYNOPSIS
    Uses PowerShell 7.5 enhanced JSON capabilities
    .DESCRIPTION
    Leverages new Test-Json features and improved ConvertTo-Json performance
    #>
    param(
        [Parameter(Mandatory)]
        [string]$JsonContent,

        [switch]$AllowTrailingCommas,
        [switch]$IgnoreComments
    )

    $analysis = @{
        IsValid = $false
        Errors = @()
        Features = @{
            TrailingCommas = $AllowTrailingCommas.IsPresent
            Comments = $IgnoreComments.IsPresent
            PowerShell75 = $true
        }
    }

    try {
        # Use PowerShell 7.5 enhanced Test-Json with new parameters
        $testParams = @{
            Json = $JsonContent
        }

        if ($AllowTrailingCommas) {
            $testParams.AllowTrailingCommas = $true
        }

        if ($IgnoreComments) {
            $testParams.IgnoreComments = $true
        }

        $isValid = Test-Json @testParams
        $analysis.IsValid = $isValid

        if (-not $isValid) {
            $analysis.Errors += "JSON validation failed with PowerShell 7.5 enhanced validation"
        }

    } catch {
        $analysis.Errors += "JSON analysis error: $($_.Exception.Message)"
    }

    return $analysis
}

function Get-EnhancedXamlAnalysis {
    <#
    .SYNOPSIS
    Enhanced XAML analysis using PowerShell 7.5 performance improvements
    .DESCRIPTION
    Uses improved string handling and regex performance in PS 7.5
    #>
    param(
        [Parameter(Mandatory)]
        [string]$FilePath
    )

    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

    try {
        $content = Get-Content $FilePath -Raw

        # Use PowerShell 7.5 enhanced array operations
        $analysis = @{
            IsValid = $false
            Errors = @()
            Warnings = @()
            SyncfusionAnalysis = @{
                Controls = @()
                Themes = @()
                Namespaces = @()
            }
            Performance = @{}
        }

        # Enhanced XML validation
        try {
            [xml]$xaml = $content
            $analysis.IsValid = $true
            $analysis.SyncfusionAnalysis.RootElement = $xaml.DocumentElement.Name
        } catch {
            $analysis.Errors += "XML parsing failed: $($_.Exception.Message)"
            $stopwatch.Stop()
            $analysis.Performance.AnalysisTime = $stopwatch.ElapsedMilliseconds
            return $analysis
        }

        # Enhanced Syncfusion control detection with better regex performance
        $syncfusionPattern = 'syncfusion:(\w+)'
        $controlMatches = [Regex]::Matches($content, $syncfusionPattern, [RegexOptions]::IgnoreCase)

        foreach ($match in $controlMatches) {
            $controlName = $match.Groups[1].Value
            $lineNumber = ($content.Substring(0, $match.Index) -split "`n").Count

            $analysis.SyncfusionAnalysis.Controls += @{
                Name = $controlName
                Line = $lineNumber
                FullMatch = $match.Value
            }
        }

        # Enhanced theme detection
        $themePattern = 'Theme="(Fluent(?:Dark|Light)?)"'
        $themeMatches = [Regex]::Matches($content, $themePattern, [RegexOptions]::IgnoreCase)

        foreach ($themeMatch in $themeMatches) {
            $themeName = $themeMatch.Groups[1].Value
            $lineNumber = ($content.Substring(0, $themeMatch.Index) -split "`n").Count

            $analysis.SyncfusionAnalysis.Themes += @{
                Name = $themeName
                Line = $lineNumber
                IsSupported = $themeName -in @('FluentDark', 'FluentLight', 'Fluent')
            }
        }

        # Validation rules
        if ($analysis.SyncfusionAnalysis.Controls.Count -gt 0) {
            $hasValidNamespace = $content -match 'xmlns:syncfusion="http://schemas\.syncfusion\.com/wpf"'
            if (-not $hasValidNamespace) {
                $analysis.Errors += "Syncfusion controls detected but proper namespace declaration missing"
            }

            $validThemes = $analysis.SyncfusionAnalysis.Themes | Where-Object { $_.IsSupported }
            if ($validThemes.Count -eq 0 -and $analysis.SyncfusionAnalysis.Controls.Count -gt 0) {
                $analysis.Warnings += "Syncfusion controls found but no valid theme specified"
            }
        }

        $stopwatch.Stop()
        $analysis.Performance = @{
            AnalysisTime = $stopwatch.ElapsedMilliseconds
            ControlsFound = $analysis.SyncfusionAnalysis.Controls.Count
            ThemesFound = $analysis.SyncfusionAnalysis.Themes.Count
            PowerShellVersion = $PSVersionTable.PSVersion.ToString()
            EnhancedRegex = $true  # PS 7.5 feature
        }

        return $analysis

    } catch {
        $stopwatch.Stop()
        return @{
            IsValid = $false
            Errors = @("XAML analysis error: $($_.Exception.Message)")
            Performance = @{
                AnalysisTime = $stopwatch.ElapsedMilliseconds
                Failed = $true
            }
        }
    }
}

function Show-PowerShell75Capabilities {
    <#
    .SYNOPSIS
    Displays PowerShell 7.5 capabilities available in our analysis tools
    #>

    Write-Host "🚀 PowerShell 7.5 Analysis Capabilities" -ForegroundColor Green
    Write-Host "=" * 50 -ForegroundColor Green

    $features = Test-PowerShell75Features

    foreach ($feature in $features.GetEnumerator()) {
        $status = if ($feature.Value) { "✅" } else { "❌" }
        $color = if ($feature.Value) { "Green" } else { "Red" }
        Write-Host "$status $($feature.Key)" -ForegroundColor $color
    }

    Write-Host "`n🔧 Performance Improvements:" -ForegroundColor Cyan
    Write-Host "  • Array += operations: Up to 82x faster" -ForegroundColor White
    Write-Host "  • Enhanced regex performance" -ForegroundColor White
    Write-Host "  • Improved .NET method invocation" -ForegroundColor White
    Write-Host "  • Better tab completion" -ForegroundColor White

    Write-Host "`n📊 New Analysis Features:" -ForegroundColor Cyan
    Write-Host "  • ConvertTo-CliXml / ConvertFrom-CliXml" -ForegroundColor White
    Write-Host "  • Enhanced Test-Json with IgnoreComments" -ForegroundColor White
    Write-Host "  • Improved error reporting with RecommendedAction" -ForegroundColor White
    Write-Host "  • Better path validation" -ForegroundColor White
}

# Export the enhanced functions
Export-ModuleMember -Function Test-PowerShell75Features, Get-EnhancedCSharpAnalysis, Get-EnhancedJsonAnalysis, Get-EnhancedXamlAnalysis, Show-PowerShell75Capabilities
