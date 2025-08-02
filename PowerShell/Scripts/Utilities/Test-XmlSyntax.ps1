#Requires -Version 7.5
<#
.SYNOPSIS
    Validates XML files against BusBuddy XML standards

.DESCRIPTION
    Comprehensive XML validation tool that checks for well-formed XML,
    validates against schema if available, and verifies compliance with
    BusBuddy XML standards including XAML files.

.PARAMETER Path
    Path to file or directory to validate

.PARAMETER Recurse
    Process directories recursively

.PARAMETER IncludeXaml
    Include XAML files in validation

.PARAMETER Schema
    Path to XSD schema file for validation

.PARAMETER IgnorePattern
    Regular expression pattern to ignore certain files

.PARAMETER GenerateReport
    Generate a detailed HTML report of validation results

.PARAMETER Fix
    Attempt to fix common XML issues (whitespace, indentation)

.EXAMPLE
    .\Test-XmlSyntax.ps1 -Path ".\BusBuddy.WPF" -Recurse -IncludeXaml

.EXAMPLE
    .\Test-XmlSyntax.ps1 -Path ".\Config.xml" -Schema ".\Schema.xsd"

.NOTES
    Author: BusBuddy Development Team
    Date: 2024-06-03
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$Path,

    [Parameter()]
    [switch]$Recurse,

    [Parameter()]
    [switch]$IncludeXaml,

    [Parameter()]
    [string]$Schema,

    [Parameter()]
    [string]$IgnorePattern,

    [Parameter()]
    [switch]$GenerateReport,

    [Parameter()]
    [switch]$Fix
)

# Initialize statistics
$stats = @{
    FilesChecked = 0
    ValidFiles = 0
    InvalidFiles = 0
    FixedFiles = 0
    Errors = @()
    Warnings = @()
}

function Test-XmlFile {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,

        [Parameter()]
        [string]$SchemaPath
    )

    $result = @{
        IsValid = $false
        Errors = @()
        Warnings = @()
        FileType = [System.IO.Path]::GetExtension($FilePath).ToLower()
        FileName = [System.IO.Path]::GetFileName($FilePath)
        FilePath = $FilePath
    }

    try {
        # Load XML file
        Write-Verbose "Loading XML file: $FilePath"
        $xmlContent = Get-Content -Path $FilePath -Raw

        # Create XML document
        $xmlDoc = New-Object System.Xml.XmlDocument

        # Disable entity processing for security (prevents XXE attacks)
        $xmlDoc.XmlResolver = $null

        # Try to load the document to check well-formedness
        try {
            $xmlDoc.LoadXml($xmlContent)
            $result.IsValid = $true
        }
        catch [System.Xml.XmlException] {
            $result.IsValid = $false
            $errorMessage = $_.Exception.Message

            # Extract line/column information if available
            if ($errorMessage -match "Line (\d+), position (\d+)") {
                $lineNumber = [int]$Matches[1]
                $columnNumber = [int]$Matches[2]

                # Get context around the error
                $lines = $xmlContent -split "`r?`n"
                $contextStart = [Math]::Max(1, $lineNumber - 2)
                $contextEnd = [Math]::Min($lines.Count, $lineNumber + 2)

                $contextLines = @()
                for ($i = $contextStart; $i -le $contextEnd; $i++) {
                    $prefix = if ($i -eq $lineNumber) { ">" } else { " " }
                    $contextLines += "$prefix $i`: $($lines[$i-1])"
                }

                $result.Errors += [PSCustomObject]@{
                    Message = $errorMessage
                    LineNumber = $lineNumber
                    ColumnNumber = $columnNumber
                    Context = $contextLines -join "`n"
                    Type = "Syntax"
                    Severity = "Error"
                }
            }
            else {
                $result.Errors += [PSCustomObject]@{
                    Message = $errorMessage
                    LineNumber = 0
                    ColumnNumber = 0
                    Context = ""
                    Type = "Syntax"
                    Severity = "Error"
                }
            }
        }

        # If we have a schema and the XML is well-formed, validate against schema
        if ($result.IsValid -and $SchemaPath -and (Test-Path $SchemaPath)) {
            Write-Verbose "Validating against schema: $SchemaPath"
            try {
                # Set up schema validation
                $schemaReader = New-Object System.Xml.XmlTextReader $SchemaPath
                $schema = [System.Xml.Schema.XmlSchema]::Read($schemaReader, $null)
                $schemaReader.Close()

                $xmlDoc.Schemas.Add($schema) | Out-Null

                # Event handler for validation errors
                $validationErrors = @()
                $eventHandler = {
                    param($validationSender, $validationEventData)
                    $validationErrors += [PSCustomObject]@{
                        Message = $validationEventData.Message
                        LineNumber = $validationEventData.Exception.LineNumber
                        ColumnNumber = $validationEventData.Exception.LinePosition
                        Type = "Schema"
                        Severity = if ($validationEventData.Severity -eq [System.Xml.Schema.XmlSeverityType]::Error) { "Error" } else { "Warning" }
                    }
                }

                # Add event handler
                $validationEventHandler = [System.Xml.Schema.ValidationEventHandler]$eventHandler

                # Validate document
                $xmlDoc.Validate($validationEventHandler)

                # Process validation errors
                foreach ($xmlError in $validationErrors) {
                    if ($xmlError.Severity -eq "Error") {
                        $result.Errors += $xmlError
                        $result.IsValid = $false
                    }
                    else {
                        $result.Warnings += $xmlError
                    }
                }
            }
            catch {
                $result.Errors += [PSCustomObject]@{
                    Message = "Schema validation error: $_"
                    LineNumber = 0
                    ColumnNumber = 0
                    Context = ""
                    Type = "Schema"
                    Severity = "Error"
                }
                $result.IsValid = $false
            }
        }

        # Additional checks for BusBuddy XML standards
        if ($result.IsValid) {
            # Check for XML declaration
            if (-not $xmlContent.TrimStart().StartsWith("<?xml ")) {
                $result.Warnings += [PSCustomObject]@{
                    Message = "Missing XML declaration"
                    LineNumber = 1
                    ColumnNumber = 1
                    Type = "Standard"
                    Severity = "Warning"
                }
            }

            # Check for double-dash in XML comments (should be em dash)
            if ($xmlContent -match "<!--.*?--.*?-->") {
                $regexMatches = [regex]::Matches($xmlContent, "<!--(.*?)-->")
                foreach ($match in $regexMatches) {
                    $commentContent = $match.Groups[1].Value
                    $lineNumber = ($xmlContent.Substring(0, $match.Index).Split("`n")).Length

                    if ($commentContent -match "--") {
                        $result.Warnings += [PSCustomObject]@{
                            Message = "XML comment contains double-dash (--); use em dash (—) instead"
                            LineNumber = $lineNumber
                            ColumnNumber = $match.Index
                            Type = "Comment"
                            Severity = "Warning"
                        }
                    }

                    # Check for comments ending with dash
                    if ($commentContent.TrimEnd() -match "-$") {
                        $result.Warnings += [PSCustomObject]@{
                            Message = "XML comment ends with a dash character; add a space or period"
                            LineNumber = $lineNumber
                            ColumnNumber = $match.Index + $match.Length - 3
                            Type = "Comment"
                            Severity = "Warning"
                        }
                    }
                }
            }

            # Check for well-formed XML comments (no -- within comment)
            $commentMatches = [regex]::Matches($xmlContent, "<!--(.*?)-->", [System.Text.RegularExpressions.RegexOptions]::Singleline)
            foreach ($match in $commentMatches) {
                $commentContent = $match.Groups[1].Value
                $lineNumber = ($xmlContent.Substring(0, $match.Index).Split("`n")).Length

                # Check for nested comment syntax
                if ($commentContent -match "<!--" -or $commentContent -match "-->") {
                    $result.Errors += [PSCustomObject]@{
                        Message = "Malformed XML comment: contains nested comment delimiters"
                        LineNumber = $lineNumber
                        ColumnNumber = $match.Index
                        Type = "Comment"
                        Severity = "Error"
                    }
                    $result.IsValid = $false
                }
            }

            # XAML specific checks
            if ($result.FileType -eq ".xaml") {
                # Check for Syncfusion namespace in appropriate files
                if ($xmlContent -match "SfDataGrid|DockingManager|SfBusyIndicator|RibbonControlAdv|NavigationDrawer|SfChart|SfButton" -and
                    $xmlContent -notmatch "xmlns:syncfusion=") {
                    $result.Warnings += [PSCustomObject]@{
                        Message = "Syncfusion control used without syncfusion namespace declaration"
                        LineNumber = 1
                        ColumnNumber = 1
                        Type = "Xaml"
                        Severity = "Warning"
                    }
                }

                # Check for proper WPF namespaces
                if ($xmlContent -notmatch "xmlns=`"http://schemas.microsoft.com/winfx/2006/xaml/presentation`"") {
                    $result.Warnings += [PSCustomObject]@{
                        Message = "Missing WPF presentation namespace"
                        LineNumber = 1
                        ColumnNumber = 1
                        Type = "Xaml"
                        Severity = "Warning"
                    }
                }

                if ($xmlContent -notmatch "xmlns:x=`"http://schemas.microsoft.com/winfx/2006/xaml`"") {
                    $result.Warnings += [PSCustomObject]@{
                        Message = "Missing WPF XAML namespace (xmlns:x)"
                        LineNumber = 1
                        ColumnNumber = 1
                        Type = "Xaml"
                        Severity = "Warning"
                    }
                }

                # Check for FluentDark/FluentLight theme consistency with Syncfusion
                if ($xmlContent -match "xmlns:syncfusion=" -and
                    $xmlContent -notmatch "FluentDark|FluentLight") {
                    $result.Warnings += [PSCustomObject]@{
                        Message = "Syncfusion control used without FluentDark/FluentLight theme specification"
                        LineNumber = 1
                        ColumnNumber = 1
                        Type = "Xaml"
                        Severity = "Warning"
                    }
                }

                # Check for proper Syncfusion control usage
                if ($xmlContent -match "DockingManager" -and
                    $xmlContent -notmatch "DockingStyle") {
                    $result.Warnings += [PSCustomObject]@{
                        Message = "DockingManager used without DockingStyle specification"
                        LineNumber = 1
                        ColumnNumber = 1
                        Type = "Xaml"
                        Severity = "Warning"
                    }
                }
            }

            # Check for XML best practices
            if ($xmlContent -match "<.+?></[^>]+?>") {
                $result.Warnings += [PSCustomObject]@{
                    Message = "Consider using self-closing tags for elements without content"
                    Type = "Style"
                    Severity = "Info"
                }
            }
        }

        return $result
    }
    catch {
        return @{
            IsValid = $false
            Errors = @([PSCustomObject]@{
                Message = "Exception during validation: $_"
                LineNumber = 0
                ColumnNumber = 0
                Type = "Exception"
                Severity = "Error"
            })
            Warnings = @()
            FileType = [System.IO.Path]::GetExtension($FilePath).ToLower()
            FileName = [System.IO.Path]::GetFileName($FilePath)
            FilePath = $FilePath
        }
    }
}

# Additional function to fix XML comment issues - renamed to use approved verb
function Repair-XmlComments {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath
    )

    try {
        Write-Verbose "Fixing XML comments in file: $FilePath"
        $content = Get-Content -Path $FilePath -Raw

        # Replace double-dash in comments with em dash
        $fixedContent = [regex]::Replace(
            $content,
            "<!--(.*?)-->",
            {
                param($match)
                $commentText = $match.Groups[1].Value
                $fixedComment = $commentText -replace "--", "—"

                # Fix comments ending with dash
                if ($fixedComment.TrimEnd() -match "-$") {
                    $fixedComment = $fixedComment -replace "-\s*$", "- "
                }

                "<!--" + $fixedComment + "-->"
            },
            [System.Text.RegularExpressions.RegexOptions]::Singleline
        )

        # Write fixed content back to file if changes were made
        if ($fixedContent -ne $content) {
            $fixedContent | Out-File -FilePath $FilePath -Encoding UTF8
            return $true
        }

        return $false
    }
    catch {
        Write-Verbose "Error fixing XML comments: $_"
        return $false
    }
}

function Format-XmlFile {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath
    )

    try {
        Write-Verbose "Attempting to format XML file: $FilePath"
        $content = Get-Content -Path $FilePath -Raw

        # Fix XML comments first - using renamed function
        Repair-XmlComments -FilePath $FilePath | Out-Null

        # Reload content after fixing comments
        $content = Get-Content -Path $FilePath -Raw

        # Parse XML
        $xmlDoc = New-Object System.Xml.XmlDocument
        $xmlDoc.PreserveWhitespace = $false
        $xmlDoc.XmlResolver = $null
        $xmlDoc.LoadXml($content)

        # Create StringWriter and XmlWriter
        $stringWriter = New-Object System.IO.StringWriter
        $writerSettings = New-Object System.Xml.XmlWriterSettings
        $writerSettings.Indent = $true
        $writerSettings.IndentChars = "  "
        $writerSettings.NewLineChars = "`n"
        $writerSettings.Encoding = [System.Text.Encoding]::UTF8
        $writerSettings.NewLineHandling = [System.Xml.NewLineHandling]::Replace

        $xmlWriter = [System.Xml.XmlWriter]::Create($stringWriter, $writerSettings)

        # Write formatted XML
        $xmlDoc.Save($xmlWriter)
        $xmlWriter.Flush()
        $xmlWriter.Close()

        # Get formatted content
        $formattedContent = $stringWriter.ToString()

        # Write back to file
        $formattedContent | Out-File -FilePath $FilePath -Encoding UTF8

        return $true
    }
    catch {
        Write-Verbose "Error formatting XML file: $_"
        return $false
    }
}

function Find-XmlFiles {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$SearchPath,

        [Parameter()]
        [switch]$Recursive,

        [Parameter()]
        [switch]$IncludeXaml,

        [Parameter()]
        [string]$IgnorePattern
    )

    $fileTypes = @("*.xml", "*.config")
    if ($IncludeXaml) {
        $fileTypes += "*.xaml"
    }

    $searchParams = @{
        Path = $SearchPath
        Include = $fileTypes
    }

    if ($Recursive) {
        $searchParams.Recurse = $true
    }

    $files = Get-ChildItem @searchParams -File

    if ($IgnorePattern) {
        $files = $files | Where-Object { $_.FullName -notmatch $IgnorePattern }
    }

    return $files
}

function New-ValidationReport {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [array]$Results,

        [Parameter(Mandatory = $true)]
        [hashtable]$Statistics
    )

    $reportPath = Join-Path -Path (Get-Location) -ChildPath "XMLValidationReport_$(Get-Date -Format 'yyyyMMdd_HHmmss').html"

    # Pre-calculate files with warnings count for efficiency
    # Calculate files with warnings count for clarity
    $filesWithWarningsCount = ($Results | Where-Object { $_.Warnings.Count -gt 0 }).Count

    $htmlHeader = @"
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>XML Validation Report</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; margin: 0; padding: 20px; color: #333; }
        h1, h2, h3 { color: #0066cc; }
        .summary { background-color: #f0f0f0; padding: 15px; border-radius: 5px; margin-bottom: 20px; }
        .stats { display: flex; gap: 20px; flex-wrap: wrap; }
        .stat-box { background-color: #fff; padding: 10px; border-radius: 5px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); flex: 1; min-width: 150px; }
        .valid { color: green; }
        .invalid { color: red; }
        .warning { color: orange; }
        .file-result { margin-bottom: 20px; border: 1px solid #ddd; padding: 10px; border-radius: 5px; }
        .file-valid { border-left: 5px solid green; }
        .file-invalid { border-left: 5px solid red; }
        .file-warning { border-left: 5px solid orange; }
        .error, .warning-item { margin: 10px 0; padding: 10px; border-radius: 3px; }
        .error { background-color: rgba(255,0,0,0.1); }
        .warning-item { background-color: rgba(255,165,0,0.1); }
        .file-header { display: flex; justify-content: space-between; align-items: center; }
        .context { font-family: monospace; padding: 10px; background-color: #f8f8f8; border-left: 3px solid #ddd; margin: 5px 0 5px 15px; overflow-x: auto; }
        .context-line.highlight { font-weight: bold; color: red; }
        .expand-btn { background-color: #0066cc; color: white; border: none; padding: 5px 10px; border-radius: 3px; cursor: pointer; }
    </style>
</head>
<body>
    <h1>XML Validation Report</h1>
    <div class="summary">
        <h2>Summary</h2>
        <div class="stats">
            <div class="stat-box">
                <h3>Files Checked</h3>
                <p>${Statistics.FilesChecked}</p>
            </div>
            <div class="stat-box">
                <h3>Valid Files</h3>
                <p class="valid">${Statistics.ValidFiles}</p>
            </div>
            <div class="stat-box">
                <h3>Invalid Files</h3>
                <p class="invalid">${Statistics.InvalidFiles}</p>
            </div>
            <div class="stat-box">
                <h3>Files with Warnings</h3>
                <p class="warning">${filesWithWarningsCount}</p>
            </div>
            <div class="stat-box">
                <h3>Files Fixed</h3>
                <p>${Statistics.FixedFiles}</p>
            </div>
        </div>
    </div>
"@

    $htmlFooter = @"
    <script>
        document.querySelectorAll('.expand-btn').forEach(btn => {
            btn.addEventListener('click', function() {
                const details = this.closest('.file-result').querySelector('.details');
                if (details.style.display === 'none') {
                    details.style.display = 'block';
                    this.textContent = 'Hide Details';
                } else {
                    details.style.display = 'none';
                    this.textContent = 'Show Details';
                }
            });
        });
    </script>
</body>
</html>
"@

    # Build file results HTML
    $fileResultsHtml = ""
    foreach ($result in $Results) {
        $fileClass = if (-not $result.IsValid) {
            "file-invalid"
        }
        elseif ($result.Warnings.Count -gt 0) {
            "file-warning"
        }
        else {
            "file-valid"
        }

        $statusText = if (-not $result.IsValid) {
            "<span class='invalid'>❌ Invalid</span>"
        }
        elseif ($result.Warnings.Count -gt 0) {
            "<span class='warning'>⚠️ Valid with Warnings</span>"
        }
        else {
            "<span class='valid'>✅ Valid</span>"
        }

        $errorsHtml = ""
        foreach ($xmlError in $result.Errors) {
            $contextHtml = ""
            if ($xmlError.Context) {
                $contextLines = $xmlError.Context -split "`n"
                $contextHtml = "<div class='context'>"
                foreach ($line in $contextLines) {
                    $highlightClass = if ($line.StartsWith(">")) { "highlight" } else { "" }
                    $contextHtml += "<div class='context-line $highlightClass'>$($line.Replace("<", "&lt;").Replace(">", "&gt;"))</div>"
                }
                $contextHtml += "</div>"
            }

            $errorsHtml += @"
<div class="error">
    <strong>Error:</strong> ${xmlError.Message}<br>
    <strong>Line:</strong> ${xmlError.LineNumber}, <strong>Column:</strong> ${xmlError.ColumnNumber}<br>
    <strong>Type:</strong> ${xmlError.Type}
    $contextHtml
</div>
"@
        }

        $warningsHtml = ""
        foreach ($warning in $result.Warnings) {
            $warningsHtml += @"
<div class="warning-item">
    <strong>Warning:</strong> ${warning.Message}<br>
    <strong>Type:</strong> ${warning.Type}
</div>
"@
        }

        $detailsDisplay = if ($result.Errors.Count -eq 0 -and $result.Warnings.Count -eq 0) {
            "none"
        } else {
            "block"
        }

        $fileResultsHtml += @"
<div class="file-result $fileClass">
    <div class="file-header">
        <h3>${result.FileName} $statusText</h3>
        <button class="expand-btn">Hide Details</button>
    </div>
    <p>${result.FilePath}</p>
    <div class="details" style="display: $detailsDisplay">
"@

        if ($result.Errors.Count -gt 0) {
            $fileResultsHtml += @"
        <h4>Errors (${result.Errors.Count})</h4>
        $errorsHtml
"@
        }

        if ($result.Warnings.Count -gt 0) {
            $fileResultsHtml += @"
        <h4>Warnings (${result.Warnings.Count})</h4>
        $warningsHtml
"@
        }

        $fileResultsHtml += @"
    </div>
</div>
"@
    }

    # Combine all HTML parts
    $fullHtml = $htmlHeader + "<h2>File Results</h2>" + $fileResultsHtml + $htmlFooter

    # Write report to file
    $fullHtml | Out-File -FilePath $reportPath -Encoding UTF8

    return $reportPath
}

# Main script execution
Write-Host "🔍 XML Validation Tool" -ForegroundColor Cyan
Write-Host "===================" -ForegroundColor Cyan

# Validate parameters
if (-not (Test-Path -Path $Path)) {
    Write-Host "❌ Path not found: $Path" -ForegroundColor Red
    exit 1
}

# Find XML files
Write-Host "Finding XML files..." -ForegroundColor Yellow
$xmlFiles = Find-XmlFiles -SearchPath $Path -Recursive:$Recurse -IncludeXaml:$IncludeXaml -IgnorePattern $IgnorePattern

Write-Host "Found $($xmlFiles.Count) XML files to validate" -ForegroundColor Green

# Validate each file
$results = @()
$stats.FilesChecked = $xmlFiles.Count

foreach ($file in $xmlFiles) {
    Write-Host "Validating $($file.FullName)..." -ForegroundColor Yellow
    $validationResult = Test-XmlFile -FilePath $file.FullName -SchemaPath $Schema

    if ($validationResult.IsValid -and $validationResult.Warnings.Count -eq 0) {
        Write-Host "  ✅ Valid" -ForegroundColor Green
        $stats.ValidFiles++
    }
    elseif ($validationResult.IsValid) {
        Write-Host "  ⚠️ Valid with $($validationResult.Warnings.Count) warnings" -ForegroundColor Yellow
        $stats.ValidFiles++

        foreach ($warning in $validationResult.Warnings) {
            Write-Host "    - $($warning.Message)" -ForegroundColor Yellow
        }

        $stats.Warnings += $validationResult.Warnings
    }
    else {
        Write-Host "  ❌ Invalid with $($validationResult.Errors.Count) errors" -ForegroundColor Red
        $stats.InvalidFiles++

        foreach ($xmlError in $validationResult.Errors) {
            Write-Host "    - Line $($xmlError.LineNumber): $($xmlError.Message)" -ForegroundColor Red
        }

        $stats.Errors += $validationResult.Errors

        # Try to fix if requested
        if ($Fix) {
            Write-Host "  🔧 Attempting to fix issues..." -ForegroundColor Cyan
            $fixed = Format-XmlFile -FilePath $file.FullName

            if ($fixed) {
                Write-Host "    ✅ File formatted successfully" -ForegroundColor Green
                $stats.FixedFiles++

                # Re-validate after fixing
                $validationResult = Test-XmlFile -FilePath $file.FullName -SchemaPath $Schema
                if ($validationResult.IsValid) {
                    Write-Host "    ✅ File is now valid" -ForegroundColor Green
                }
                else {
                    Write-Host "    ⚠️ Formatting did not fix all issues" -ForegroundColor Yellow
                }
            }
            else {
                Write-Host "    ❌ Unable to fix file" -ForegroundColor Red
            }
        }
    }

    $results += $validationResult
}

# Print summary
Write-Host "`n📊 Validation Summary" -ForegroundColor Cyan
Write-Host "=================" -ForegroundColor Cyan
Write-Host "Files Checked: $($stats.FilesChecked)" -ForegroundColor White
Write-Host "Valid Files: $($stats.ValidFiles)" -ForegroundColor Green
Write-Host "Invalid Files: $($stats.InvalidFiles)" -ForegroundColor $(if ($stats.InvalidFiles -gt 0) { "Red" } else { "Green" })
Write-Host "Files with Warnings: $(($results | Where-Object { $_.Warnings.Count -gt 0 }).Count)" -ForegroundColor Yellow
Write-Host "Files Fixed: $($stats.FixedFiles)" -ForegroundColor Cyan

# Generate report if requested
if ($GenerateReport) {
    Write-Host "`n📄 Generating validation report..." -ForegroundColor Cyan
    $reportPath = New-ValidationReport -Results $results -Statistics $stats
    Write-Host "Report saved to: $reportPath" -ForegroundColor Green

    # Open report in default browser if available
    if ($PSVersionTable.Platform -ne 'Unix') {
        try {
            Start-Process $reportPath
        }
        catch {
            Write-Host "Could not open report automatically: $_" -ForegroundColor Yellow
        }
    }
}

# Return status code
if ($stats.InvalidFiles -gt 0) {
    exit 1
} else {
    exit 0
}

