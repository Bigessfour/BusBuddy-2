#Requires -Version 7.0
<#
.SYNOPSIS
    Advanced PowerShell XAML Syntax Analyzer - Detection-Only Tool

.DESCRIPTION
    Comprehensive XAML syntax detection and analysis tool inspired by community patterns.
    Focuses on detection and reporting only - no automatic fixes.

.PARAMETER Path
    Path to analyze (file or directory)

.PARAMETER Detailed
    Show detailed analysis with line-by-line breakdown

.PARAMETER ExportReport
    Export findings to JSON report

.EXAMPLE
    .\XAML-Syntax-Analyzer.ps1 -Path "C:\Project\Views" -Detailed

.EXAMPLE
    .\XAML-Syntax-Analyzer.ps1 -Path "MainWindow.xaml" -ExportReport
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Path,

    [switch]$Detailed,

    [switch]$ExportReport,

    [string]$ReportPath = "XAML-Analysis-Report.json"
)

# Color scheme for output
$Colors = @{
    Header = 'Cyan'
    Success = 'Green'
    Warning = 'Yellow'
    Error = 'Red'
    Info = 'White'
    Highlight = 'Magenta'
}

class XamlIssue {
    [string]$File
    [int]$Line
    [string]$Type
    [string]$Severity
    [string]$Message
    [string]$Context
    [string]$Suggestion
    [hashtable]$Metadata

    XamlIssue([string]$file, [int]$line, [string]$type, [string]$severity, [string]$message, [string]$context, [string]$suggestion) {
        $this.File = $file
        $this.Line = $line
        $this.Type = $type
        $this.Severity = $severity
        $this.Message = $message
        $this.Context = $context
        $this.Suggestion = $suggestion
        $this.Metadata = @{}
    }
}

class XamlAnalyzer {
    [System.Collections.Generic.List[XamlIssue]]$Issues
    [hashtable]$Statistics
    [hashtable]$Patterns

    XamlAnalyzer() {
        $this.Issues = [System.Collections.Generic.List[XamlIssue]]::new()
        $this.Statistics = @{
            FilesAnalyzed = 0
            TotalLines = 0
            IssuesFound = 0
            CriticalIssues = 0
            WarningIssues = 0
            InfoIssues = 0
        }
        $this.InitializePatterns()
    }

    [void]InitializePatterns() {
        $this.Patterns = @{
            # XML Structure Issues
            UnclosedTags = @{
                Pattern = '<(\w+)(?:\s[^>]*)?(?<!/)>'
                Description = "Potentially unclosed XML tags"
                Severity = "Critical"
            }

            MismatchedTags = @{
                Pattern = '<(/?)(\w+)'
                Description = "Mismatched opening/closing tags"
                Severity = "Critical"
            }

            # XAML-Specific Issues
            MissingNamespace = @{
                Pattern = '(?<!xmlns:?)\w+:'
                Description = "Element with undefined namespace prefix"
                Severity = "Error"
            }

            InvalidBinding = @{
                Pattern = '\{Binding\s+[^}]*\}'
                Description = "Potentially invalid binding expression"
                Severity = "Warning"
            }

            HardcodedValues = @{
                Pattern = '(Width|Height|Margin)="[\d\.,\s]+(?:px)?"'
                Description = "Hardcoded dimension values (consider using resources)"
                Severity = "Info"
            }

            # Resource Issues
            UndefinedResource = @{
                Pattern = '\{StaticResource\s+(\w+)\}'
                Description = "Reference to potentially undefined static resource"
                Severity = "Warning"
            }

            DuplicateKeys = @{
                Pattern = 'x:Key="([^"]+)"'
                Description = "Potential duplicate resource keys"
                Severity = "Error"
            }

            # Style Issues
            InlineStyles = @{
                Pattern = '(?:Foreground|Background|FontFamily|FontSize)="[^"]+"'
                Description = "Inline styling (consider using styles/themes)"
                Severity = "Info"
            }

            # Performance Issues
            NestedViewBox = @{
                Pattern = '<Viewbox[^>]*>.*<Viewbox'
                Description = "Nested Viewbox elements (performance concern)"
                Severity = "Warning"
            }

            # WPF-Specific Issues
            GridRowColumnIssues = @{
                Pattern = 'Grid\.(Row|Column)="(\d+)"'
                Description = "Grid row/column definitions"
                Severity = "Info"
            }

            # Event Handler Issues
            EventHandlers = @{
                Pattern = '(Click|Loaded|SelectionChanged)="([^"]+)"'
                Description = "Event handler references"
                Severity = "Info"
            }

            # Data Context Issues
            DataContextBinding = @{
                Pattern = 'DataContext="\{Binding[^}]*\}"'
                Description = "DataContext binding expressions"
                Severity = "Info"
            }
        }
    }

    [void]AnalyzeFile([string]$filePath) {
        Write-Host "🔍 Analyzing: " -ForegroundColor White -NoNewline
        Write-Host (Split-Path $filePath -Leaf) -ForegroundColor Magenta

        try {
            $content = Get-Content $filePath -Raw -ErrorAction Stop
            $lines = Get-Content $filePath -ErrorAction Stop

            $this.Statistics.FilesAnalyzed++
            $this.Statistics.TotalLines += $lines.Count

            # Basic XML validation
            $this.ValidateXmlStructure($filePath, $content, $lines)

            # Pattern-based analysis
            $this.AnalyzePatterns($filePath, $content, $lines)

            # Namespace analysis
            $this.AnalyzeNamespaces($filePath, $content, $lines)

            # Resource analysis
            $this.AnalyzeResources($filePath, $content, $lines)

            # Control-specific analysis
            $this.AnalyzeControls($filePath, $content, $lines)

        }
        catch {
            $issue = [XamlIssue]::new($filePath, 0, "FileAccess", "Critical", "Cannot read file: $($_.Exception.Message)", "", "Check file permissions and encoding")
            $this.Issues.Add($issue)
            Write-Host "❌ Error reading file: $($_.Exception.Message)" -ForegroundColor Red
        }
    }

    [void]ValidateXmlStructure([string]$filePath, [string]$content, [string[]]$lines) {
        try {
            # Try to parse as XML
            $xml = [xml]$content
            Write-Host "  ✅ Valid XML structure" -ForegroundColor Green
        }
        catch {
            $issue = [XamlIssue]::new($filePath, 0, "XmlStructure", "Critical", "Invalid XML: $($_.Exception.Message)", "", "Check for unclosed tags, invalid characters, or malformed XML")
            $this.Issues.Add($issue)
            Write-Host "  ❌ Invalid XML structure" -ForegroundColor Red
        }

        # Check for common XML issues line by line
        for ($i = 0; $i -lt $lines.Count; $i++) {
            $line = $lines[$i]
            $lineNumber = $i + 1

            # Check for unescaped characters
            if ($line -match '[<>&](?![a-zA-Z#][a-zA-Z0-9]*;)') {
                $issue = [XamlIssue]::new($filePath, $lineNumber, "XmlEscaping", "Warning", "Potentially unescaped XML characters", $line.Trim(), "Use XML entities (&lt;, &gt;, &amp;)")
                $this.Issues.Add($issue)
            }

            # Check for mixed quotes
            if ($line -match '="[^"]*''[^"]*"' -or $line -match "='[^']*`"[^']*'") {
                $issue = [XamlIssue]::new($filePath, $lineNumber, "QuoteMixing", "Error", "Mixed quote types in attribute", $line.Trim(), "Use consistent quote types")
                $this.Issues.Add($issue)
            }
        }
    }

    [void]AnalyzePatterns([string]$filePath, [string]$content, [string[]]$lines) {
        foreach ($patternName in $this.Patterns.Keys) {
            $pattern = $this.Patterns[$patternName]

            # Find all matches
            $matches = [regex]::Matches($content, $pattern.Pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

            foreach ($match in $matches) {
                # Find line number
                $lineNumber = $this.GetLineNumber($content, $match.Index)
                $lineContent = $lines[$lineNumber - 1]

                $issue = [XamlIssue]::new($filePath, $lineNumber, $patternName, $pattern.Severity, $pattern.Description, $lineContent.Trim(), "Review for compliance with best practices")
                $issue.Metadata["MatchValue"] = $match.Value
                $this.Issues.Add($issue)
            }
        }
    }

    [void]AnalyzeNamespaces([string]$filePath, [string]$content, [string[]]$lines) {
        # Extract namespace declarations
        $namespaces = @{}
        $nsMatches = [regex]::Matches($content, 'xmlns:?([a-zA-Z]\w*)?="([^"]+)"')

        foreach ($match in $nsMatches) {
            $prefix = if ($match.Groups[1].Success) { $match.Groups[1].Value } else { "" }
            $uri = $match.Groups[2].Value
            $namespaces[$prefix] = $uri
        }

        # Check for undefined namespace usage
        $elementMatches = [regex]::Matches($content, '</?([a-zA-Z]\w*):(\w+)')
        foreach ($match in $elementMatches) {
            $prefix = $match.Groups[1].Value
            if (-not $namespaces.ContainsKey($prefix)) {
                $lineNumber = $this.GetLineNumber($content, $match.Index)
                $lineContent = $lines[$lineNumber - 1]

                $issue = [XamlIssue]::new($filePath, $lineNumber, "UndefinedNamespace", "Error", "Undefined namespace prefix: $prefix", $lineContent.Trim(), "Add xmlns:$prefix declaration")
                $this.Issues.Add($issue)
            }
        }
    }

    [void]AnalyzeResources([string]$filePath, [string]$content, [string[]]$lines) {
        # Find all resource keys
        $resourceKeys = @{}
        $keyMatches = [regex]::Matches($content, 'x:Key="([^"]+)"')

        foreach ($match in $keyMatches) {
            $key = $match.Groups[1].Value
            $lineNumber = $this.GetLineNumber($content, $match.Index)

            if ($resourceKeys.ContainsKey($key)) {
                $issue = [XamlIssue]::new($filePath, $lineNumber, "DuplicateResourceKey", "Error", "Duplicate resource key: $key", $lines[$lineNumber - 1].Trim(), "Use unique resource keys")
                $this.Issues.Add($issue)
            }
            else {
                $resourceKeys[$key] = $lineNumber
            }
        }

        # Check for undefined resource references
        $resourceRefs = [regex]::Matches($content, '\{(?:StaticResource|DynamicResource)\s+([^}]+)\}')
        foreach ($match in $resourceRefs) {
            $resourceName = $match.Groups[1].Value.Trim()
            if (-not $resourceKeys.ContainsKey($resourceName)) {
                $lineNumber = $this.GetLineNumber($content, $match.Index)
                $issue = [XamlIssue]::new($filePath, $lineNumber, "UndefinedResource", "Warning", "Undefined resource reference: $resourceName", $lines[$lineNumber - 1].Trim(), "Define resource or check spelling")
                $this.Issues.Add($issue)
            }
        }
    }

    [void]AnalyzeControls([string]$filePath, [string]$content, [string[]]$lines) {
        # Grid analysis
        $gridMatches = [regex]::Matches($content, '<Grid[^>]*>')
        foreach ($match in $gridMatches) {
            $lineNumber = $this.GetLineNumber($content, $match.Index)

            # Check for grid definitions after grid start
            $gridEndIndex = $content.IndexOf('</Grid>', $match.Index)
            if ($gridEndIndex -gt 0) {
                $gridContent = $content.Substring($match.Index, $gridEndIndex - $match.Index)

                # Check for RowDefinitions/ColumnDefinitions
                $hasRowDefs = $gridContent -match '<Grid\.RowDefinitions>'
                $hasColDefs = $gridContent -match '<Grid\.ColumnDefinitions>'

                # Check for Grid.Row/Grid.Column usage
                $hasRowUsage = $gridContent -match 'Grid\.Row='
                $hasColUsage = $gridContent -match 'Grid\.Column='

                if (($hasRowUsage -and -not $hasRowDefs) -or ($hasColUsage -and -not $hasColDefs)) {
                    $issue = [XamlIssue]::new($filePath, $lineNumber, "GridDefinitionMismatch", "Warning", "Grid uses Row/Column without definitions", $lines[$lineNumber - 1].Trim(), "Add RowDefinitions/ColumnDefinitions")
                    $this.Issues.Add($issue)
                }
            }
        }
    }

    [int]GetLineNumber([string]$content, [int]$index) {
        return ($content.Substring(0, $index) -split "`n").Count
    }

    [void]UpdateStatistics() {
        $this.Statistics.IssuesFound = $this.Issues.Count
        $this.Statistics.CriticalIssues = ($this.Issues | Where-Object { $_.Severity -eq "Critical" }).Count
        $this.Statistics.WarningIssues = ($this.Issues | Where-Object { $_.Severity -eq "Warning" }).Count
        $this.Statistics.InfoIssues = ($this.Issues | Where-Object { $_.Severity -eq "Info" }).Count
    }

    [void]ShowSummary() {
        $this.UpdateStatistics()

        Write-Host "`n" + "="*80 -ForegroundColor Cyan
        Write-Host "📊 XAML SYNTAX ANALYSIS SUMMARY" -ForegroundColor Cyan
        Write-Host "="*80 -ForegroundColor Cyan

        Write-Host "Files Analyzed    : " -ForegroundColor White -NoNewline
        Write-Host $this.Statistics.FilesAnalyzed -ForegroundColor Magenta

        Write-Host "Total Lines       : " -ForegroundColor White -NoNewline
        Write-Host $this.Statistics.TotalLines -ForegroundColor Magenta

        Write-Host "Issues Found      : " -ForegroundColor White -NoNewline
        Write-Host $this.Statistics.IssuesFound -ForegroundColor Magenta

        Write-Host "  Critical        : " -ForegroundColor Red -NoNewline
        Write-Host $this.Statistics.CriticalIssues -ForegroundColor Red

        Write-Host "  Warnings        : " -ForegroundColor Yellow -NoNewline
        Write-Host $this.Statistics.WarningIssues -ForegroundColor Yellow

        Write-Host "  Informational   : " -ForegroundColor Green -NoNewline
        Write-Host $this.Statistics.InfoIssues -ForegroundColor Green
    }

    [void]ShowDetailedReport() {
        if ($this.Issues.Count -eq 0) {
            Write-Host "`n✅ No issues found!" -ForegroundColor Green
            return
        }

        Write-Host "`n" + "="*80 -ForegroundColor Cyan
        Write-Host "🔍 DETAILED ISSUE REPORT" -ForegroundColor Cyan
        Write-Host "="*80 -ForegroundColor Cyan

        $groupedIssues = $this.Issues | Group-Object File

        foreach ($group in $groupedIssues) {
            Write-Host "`n📁 File: " -ForegroundColor White -NoNewline
            Write-Host $group.Name -ForegroundColor Magenta

            $sortedIssues = $group.Group | Sort-Object Line

            foreach ($issue in $sortedIssues) {
                $severityColor = switch ($issue.Severity) {
                    "Critical" { Red }
                    "Error" { Red }
                    "Warning" { Yellow }
                    "Info" { Green }
                    default { White }
                }

                Write-Host "  Line $($issue.Line): " -ForegroundColor White -NoNewline
                Write-Host "[$($issue.Severity)] " -ForegroundColor $severityColor -NoNewline
                Write-Host "$($issue.Type) - $($issue.Message)" -ForegroundColor $severityColor

                if ($issue.Context) {
                    Write-Host "    Context: " -ForegroundColor White -NoNewline
                    Write-Host $issue.Context -ForegroundColor White
                }

                if ($issue.Suggestion) {
                    Write-Host "    Suggestion: " -ForegroundColor White -NoNewline
                    Write-Host $issue.Suggestion -ForegroundColor Green
                }

                Write-Host ""
            }
        }
    }

    [hashtable]ExportReport() {
        $this.UpdateStatistics()

        return @{
            Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
            Statistics = $this.Statistics
            Issues = @($this.Issues | ForEach-Object {
                @{
                    File = $_.File
                    Line = $_.Line
                    Type = $_.Type
                    Severity = $_.Severity
                    Message = $_.Message
                    Context = $_.Context
                    Suggestion = $_.Suggestion
                    Metadata = $_.Metadata
                }
            })
        }
    }
}

function Start-XamlAnalysis {
    [CmdletBinding()]
    param(
        [string]$Path,
        [switch]$Detailed,
        [switch]$ExportReport,
        [string]$ReportPath
    )

    Write-Host "🚀 XAML Syntax Analyzer v2.0" -ForegroundColor Cyan
    Write-Host "Detection-Only Tool - No Automatic Fixes" -ForegroundColor White
    Write-Host ""

    $analyzer = [XamlAnalyzer]::new()

    # Resolve path
    $targetPath = Resolve-Path $Path -ErrorAction SilentlyContinue
    if (-not $targetPath) {
        Write-Host "❌ Path not found: $Path" -ForegroundColor Red
        return
    }

    # Get XAML files
    $xamlFiles = @()
    if (Test-Path $targetPath -PathType Container) {
        $xamlFiles = Get-ChildItem $targetPath -Filter "*.xaml" -Recurse
    }
    elseif (Test-Path $targetPath -PathType Leaf) {
        if ($targetPath.Extension -eq ".xaml") {
            $xamlFiles = @(Get-Item $targetPath)
        }
        else {
            Write-Host "❌ Not a XAML file: $targetPath" -ForegroundColor Red
            return
        }
    }

    if ($xamlFiles.Count -eq 0) {
        Write-Host "⚠️ No XAML files found in: $targetPath" -ForegroundColor Yellow
        return
    }

    Write-Host "📂 Found $($xamlFiles.Count) XAML file(s) to analyze" -ForegroundColor White
    Write-Host ""

    # Analyze each file
    foreach ($file in $xamlFiles) {
        $analyzer.AnalyzeFile($file.FullName)
    }

    # Show results
    $analyzer.ShowSummary()

    if ($Detailed) {
        $analyzer.ShowDetailedReport()
    }

    # Export report if requested
    if ($ExportReport) {
        $report = $analyzer.ExportReport()
        $report | ConvertTo-Json -Depth 10 | Out-File $ReportPath -Encoding UTF8
        Write-Host "`n📄 Report exported to: $ReportPath" -ForegroundColor Green
    }

    Write-Host "`n✨ Analysis complete!" -ForegroundColor Green
}

# Main execution
if ($MyInvocation.InvocationName -ne '.') {
    Start-XamlAnalysis -Path $Path -Detailed:$Detailed -ExportReport:$ExportReport -ReportPath $ReportPath
}
