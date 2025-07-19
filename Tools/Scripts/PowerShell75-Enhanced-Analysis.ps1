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
        'ConvertTo-CliXml'        = { Get-Command ConvertTo-CliXml -ErrorAction SilentlyContinue }
        'ConvertFrom-CliXml'      = { Get-Command ConvertFrom-CliXml -ErrorAction SilentlyContinue }
        'ImprovedTabCompletion'   = { $PSVersionTable.PSVersion -ge [Version]'7.5.0' }
        'EnhancedJsonHandling'    = { (Get-Command ConvertTo-Json).Parameters.ContainsKey('DateKind') }
        'PerformanceImprovements' = { $PSVersionTable.PSVersion -ge [Version]'7.5.0' }
        'NewGuidEmpty'            = { (Get-Command New-Guid).Parameters.ContainsKey('Empty') }
        'TestJsonEnhancements'    = { (Get-Command Test-Json).Parameters.ContainsKey('IgnoreComments') }
    }

    $results = @{}
    # PowerShell 7.5.2 optimized parallel feature detection
    $parallelResults = $features.GetEnumerator() | ForEach-Object -Parallel {
        $feature = $_
        try {
            $result = [bool](& $feature.Value)
            return [PSCustomObject]@{ Key = $feature.Key; Value = $result }
        } catch {
            return [PSCustomObject]@{ Key = $feature.Key; Value = $false }
        }
    } -ThrottleLimit 4

    foreach ($result in $parallelResults) {
        $results[$result.Key] = $result.Value
    }

    return $results
}

function Invoke-PowerShellScriptAnalyzer {
    <#
    .SYNOPSIS
    PowerShell 7.5.2 optimized PSScriptAnalyzer with parallel processing
    .PARAMETER Path
    Path to PowerShell files to analyze
    .PARAMETER Severity
    Severity level (Error, Warning, Information)
    #>
    param(
        [Parameter(Mandatory)]
        [string]$Path,
        [ValidateSet('Error', 'Warning', 'Information')]
        [string]$Severity = 'Error'
    )

    # Check if PSScriptAnalyzer is available
    if (-not (Get-Module -ListAvailable PSScriptAnalyzer)) {
        Write-Warning 'PSScriptAnalyzer module not found. Install with: Install-Module PSScriptAnalyzer'
        return
    }

    Import-Module PSScriptAnalyzer -ErrorAction SilentlyContinue

    $psFiles = Get-ChildItem -Path $Path -Filter '*.ps1' -Recurse -ErrorAction SilentlyContinue

    if ($psFiles.Count -eq 0) {
        Write-Host "No PowerShell files found in $Path" -ForegroundColor Yellow
        return
    }

    Write-Host "🔍 Analyzing $($psFiles.Count) PowerShell files with PSScriptAnalyzer..." -ForegroundColor Cyan

    # PowerShell 7.5.2 parallel analysis
    $results = $psFiles | ForEach-Object -Parallel {
        $file = $_
        $severity = $using:Severity

        try {
            $violations = Invoke-ScriptAnalyzer -Path $file.FullName -Severity $severity -ErrorAction Stop
            return [PSCustomObject]@{
                File       = $file.Name
                FullPath   = $file.FullName
                Violations = $violations
                Success    = $true
            }
        } catch {
            return [PSCustomObject]@{
                File     = $file.Name
                FullPath = $file.FullName
                Error    = $_.Exception.Message
                Success  = $false
            }
        }
    } -ThrottleLimit 6

    $totalViolations = [System.Collections.Generic.List[object]]::new()
    $errorFiles = [System.Collections.Generic.List[string]]::new()

    foreach ($result in $results) {
        if ($result.Success) {
            if ($result.Violations) {
                $totalViolations.AddRange($result.Violations)
                Write-Host "❌ $($result.File): $($result.Violations.Count) violations" -ForegroundColor Red
            } else {
                Write-Host "✅ $($result.File): No violations" -ForegroundColor Green
            }
        } else {
            $errorFiles.Add($result.File)
            Write-Host "⚠️ $($result.File): Analysis failed - $($result.Error)" -ForegroundColor Yellow
        }
    }

    Write-Host "`n📊 Summary:" -ForegroundColor Cyan
    Write-Host "   Files analyzed: $($results.Count)" -ForegroundColor White
    Write-Host "   Total violations: $($totalViolations.Count)" -ForegroundColor $(if ($totalViolations.Count -eq 0) { 'Green' } else { 'Red' })
    Write-Host "   Failed analyses: $($errorFiles.Count)" -ForegroundColor $(if ($errorFiles.Count -eq 0) { 'Green' } else { 'Yellow' })

    return [PSCustomObject]@{
        TotalFiles      = $results.Count
        TotalViolations = $totalViolations.Count
        FailedAnalyses  = $errorFiles.Count
        Details         = $results
        Violations      = $totalViolations.ToArray()
    }
}

function Export-BusBuddyDiagnosticData {
    <#
    .SYNOPSIS
    PowerShell 7.5.2 optimized diagnostic data export using ConvertTo-CliXml
    .PARAMETER OutputPath
    Path to export diagnostic data
    .PARAMETER IncludeScriptAnalysis
    Include PSScriptAnalyzer results
    #>
    param(
        [string]$OutputPath = "BusBuddy-Diagnostics-$(Get-Date -Format 'yyyyMMdd-HHmmss').xml",
        [switch]$IncludeScriptAnalysis
    )

    Write-Host '🔧 Collecting Bus Buddy diagnostic data...' -ForegroundColor Cyan

    $diagnosticData = [PSCustomObject]@{
        Timestamp         = Get-Date
        PowerShellVersion = $PSVersionTable.PSVersion
        Features          = Test-PowerShell75Features
        ProjectRoot       = Get-BusBuddyProjectRoot
        XamlFiles         = @()
        CSharpFiles       = @()
        ScriptAnalysis    = $null
    }

    $projectRoot = $diagnosticData.ProjectRoot
    if ($projectRoot) {
        # Collect XAML file information
        $xamlFiles = Get-ChildItem -Path $projectRoot -Filter '*.xaml' -Recurse -ErrorAction SilentlyContinue
        $diagnosticData.XamlFiles = $xamlFiles | Select-Object Name, FullName, Length, LastWriteTime

        # Collect C# file information
        $csharpFiles = Get-ChildItem -Path $projectRoot -Filter '*.cs' -Recurse -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -notmatch '\\(bin|obj|packages)\\' }
        $diagnosticData.CSharpFiles = $csharpFiles | Select-Object Name, FullName, Length, LastWriteTime

        if ($IncludeScriptAnalysis) {
            $scriptPath = Join-Path $projectRoot 'Tools\Scripts'
            if (Test-Path $scriptPath) {
                $diagnosticData.ScriptAnalysis = Invoke-PowerShellScriptAnalyzer -Path $scriptPath
            }
        }
    }

    try {
        # Use PowerShell 7.5.2 ConvertTo-CliXml for efficient serialization
        $diagnosticData | ConvertTo-CliXml -Depth 10 | Out-File -FilePath $OutputPath -Encoding UTF8 -ErrorAction Stop
        Write-Host "✅ Diagnostic data exported to: $OutputPath" -ForegroundColor Green
        return $OutputPath
    } catch {
        Write-Error "Failed to export diagnostic data: $($_.Exception.Message)"
        return $null
    }
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
            IsValid     = $false
            Errors      = @("File not found: $FilePath")
            Performance = @{ AnalysisTime = 0 }
        }
    }

    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

    try {
        $content = Get-Content $FilePath -Raw
        $lines = $content -split "`n"

        # Use PowerShell 7.5 enhanced array operations (significantly faster)
        $analysis = @{
            IsValid        = $true
            Errors         = @()  # Using @() for better performance in PS 7.5
            Warnings       = @()
            BraceAnalysis  = @{
                OpenBraces  = @()
                CloseBraces = @()
                Mismatches  = @()
            }
            MethodAnalysis = @{
                Methods              = @()
                OrphanedCode         = @()
                IncompleteSignatures = @()
            }
            Performance    = @{}
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
                        Type    = 'Opening'
                        Line    = $lineNum + 1
                        Column  = $column
                        Context = $line.Trim()
                    }
                    $analysis.BraceAnalysis.OpenBraces += $braceInfo
                    $braceStack += $braceInfo  # PS 7.5 optimized += operation
                } else {
                    $braceInfo = @{
                        Type    = 'Closing'
                        Line    = $lineNum + 1
                        Column  = $column
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
                Line            = $lineNumber
                Signature       = $methodMatch.Value.Trim()
                HasOpeningBrace = $methodMatch.Value.Contains('{')
            }
        }

        # Detect orphaned code (code outside proper method/class context)
        $classPattern = '(?m)^\s*(public|private|protected|internal)?\s*(static)?\s*(partial)?\s*class\s+\w+'
        $namespacePattern = '(?m)^\s*namespace\s+[\w.]+'

        $classMatches = [Regex]::Matches($content, $classPattern, [RegexOptions]::Multiline)
        $namespaceMatches = [Regex]::Matches($content, $namespacePattern, [RegexOptions]::Multiline)

        if ($classMatches.Count -eq 0 -and $content.Length -gt 100) {
            $analysis.Warnings += 'No class declarations found in C# file'
        }

        if ($namespaceMatches.Count -eq 0 -and $content.Length -gt 100) {
            $analysis.Warnings += 'No namespace declarations found in C# file'
        }

        $analysis.IsValid = $analysis.Errors.Count -eq 0

        $stopwatch.Stop()
        $analysis.Performance = @{
            AnalysisTime      = $stopwatch.ElapsedMilliseconds
            LinesAnalyzed     = $lines.Count
            PowerShellVersion = $PSVersionTable.PSVersion.ToString()
            OptimizedArrayOps = $true  # PS 7.5 feature
        }

        return $analysis

    } catch {
        $stopwatch.Stop()
        return @{
            IsValid     = $false
            Errors      = @("Analysis error: $($_.Exception.Message)")
            Performance = @{
                AnalysisTime = $stopwatch.ElapsedMilliseconds
                Failed       = $true
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
        IsValid  = $false
        Errors   = @()
        Features = @{
            TrailingCommas = $AllowTrailingCommas.IsPresent
            Comments       = $IgnoreComments.IsPresent
            PowerShell75   = $true
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
            $analysis.Errors += 'JSON validation failed with PowerShell 7.5 enhanced validation'
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
            IsValid            = $false
            Errors             = @()
            Warnings           = @()
            SyncfusionAnalysis = @{
                Controls   = @()
                Themes     = @()
                Namespaces = @()
            }
            Performance        = @{}
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
                Name      = $controlName
                Line      = $lineNumber
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
                Name        = $themeName
                Line        = $lineNumber
                IsSupported = $themeName -in @('FluentDark', 'FluentLight', 'Fluent')
            }
        }

        # Validation rules
        if ($analysis.SyncfusionAnalysis.Controls.Count -gt 0) {
            $hasValidNamespace = $content -match 'xmlns:syncfusion="http://schemas\.syncfusion\.com/wpf"'
            if (-not $hasValidNamespace) {
                $analysis.Errors += 'Syncfusion controls detected but proper namespace declaration missing'
            }

            $validThemes = $analysis.SyncfusionAnalysis.Themes | Where-Object { $_.IsSupported }
            if ($validThemes.Count -eq 0 -and $analysis.SyncfusionAnalysis.Controls.Count -gt 0) {
                $analysis.Warnings += 'Syncfusion controls found but no valid theme specified'
            }
        }

        $stopwatch.Stop()
        $analysis.Performance = @{
            AnalysisTime      = $stopwatch.ElapsedMilliseconds
            ControlsFound     = $analysis.SyncfusionAnalysis.Controls.Count
            ThemesFound       = $analysis.SyncfusionAnalysis.Themes.Count
            PowerShellVersion = $PSVersionTable.PSVersion.ToString()
            EnhancedRegex     = $true  # PS 7.5 feature
        }

        return $analysis

    } catch {
        $stopwatch.Stop()
        return @{
            IsValid     = $false
            Errors      = @("XAML analysis error: $($_.Exception.Message)")
            Performance = @{
                AnalysisTime = $stopwatch.ElapsedMilliseconds
                Failed       = $true
            }
        }
    }
}

function Show-PowerShell75Capabilities {
    <#
    .SYNOPSIS
    Displays PowerShell 7.5 capabilities available in our analysis tools
    #>

    Write-Host '🚀 PowerShell 7.5 Analysis Capabilities' -ForegroundColor Green
    Write-Host '=' * 50 -ForegroundColor Green

    $features = Test-PowerShell75Features

    foreach ($feature in $features.GetEnumerator()) {
        $status = if ($feature.Value) { '✅' } else { '❌' }
        $color = if ($feature.Value) { 'Green' } else { 'Red' }
        Write-Host "$status $($feature.Key)" -ForegroundColor $color
    }

    Write-Host "`n🔧 Performance Improvements:" -ForegroundColor Cyan
    Write-Host '  • Array += operations: Up to 82x faster' -ForegroundColor White
    Write-Host '  • Enhanced regex performance' -ForegroundColor White
    Write-Host '  • Improved .NET method invocation' -ForegroundColor White
    Write-Host '  • Better tab completion' -ForegroundColor White

    Write-Host "`n📊 New Analysis Features:" -ForegroundColor Cyan
    Write-Host '  • ConvertTo-CliXml / ConvertFrom-CliXml' -ForegroundColor White
    Write-Host '  • Enhanced Test-Json with IgnoreComments' -ForegroundColor White
    Write-Host '  • Improved error reporting with RecommendedAction' -ForegroundColor White
    Write-Host '  • Better path validation' -ForegroundColor White
}

# Export the enhanced functions
# Note: Functions are available when script is dot-sourced
# Available functions: Test-PowerShell75Features, Get-EnhancedCSharpAnalysis, Get-EnhancedJsonAnalysis, Get-EnhancedXamlAnalysis, Show-PowerShell75Capabilities
