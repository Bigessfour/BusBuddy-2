#Requires -Version 7.5
<#
.SYNOPSIS
    Enhanced XML validation script for BusBuddy project files

.DESCRIPTION
    Validates XML/XAML files across the BusBuddy project using the modern Test-BusBuddyXml function.
    Generates comprehensive HTML reports with detailed error analysis and recommendations.

.NOTES
    Compatible with PowerShell 7.5.2 and .NET 9.0
    Uses updated Test-BusBuddyXml function with modern validation approach
#>

# Import the BusBuddy module
Import-Module "$PSScriptRoot\..\..\Modules\BusBuddy\BusBuddy.psm1" -Force

# Define file extensions to validate
$xmlExtensions = @('*.xml', '*.xaml', '*.csproj', '*.props', '*.config', '*.ruleset', '*.targets')

Write-Host "🔍 Starting XML validation scan..." -ForegroundColor Cyan
Write-Host "Extensions: $($xmlExtensions -join ', ')" -ForegroundColor Gray

# Find all XML files in the project
$xmlFiles = Get-ChildItem -Recurse -Include $xmlExtensions | Where-Object {
    # Exclude build artifacts and temporary directories
    $_.FullName -notmatch '\\(bin|obj|packages|TestResults|logs)\\'
}

Write-Host "Found $($xmlFiles.Count) XML files to validate" -ForegroundColor Yellow
Write-Host ""

$results = @()
$validCount = 0
$invalidCount = 0

foreach ($file in $xmlFiles) {
    Write-Host "Validating: $($file.Name)" -ForegroundColor White -NoNewline

    # Determine schema path for MSBuild files
    $schema = $null
    if ($file.Extension -in @('.csproj', '.props', '.targets')) {
        $potentialSchemas = @(
            "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Xml\Schemas\1033\MSBuild\Microsoft.Build.xsd",
            "C:\Program Files\Microsoft Visual Studio\2022\Professional\Xml\Schemas\1033\MSBuild\Microsoft.Build.xsd",
            "C:\Program Files\Microsoft Visual Studio\2022\Community\Xml\Schemas\1033\MSBuild\Microsoft.Build.xsd",
            "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Microsoft\Microsoft.Build.xsd"
        )

        foreach ($potentialSchema in $potentialSchemas) {
            if (Test-Path $potentialSchema) {
                $schema = $potentialSchema
                break
            }
        }
    }

    try {
        # Use the updated Test-BusBuddyXml function
        $result = Test-BusBuddyXml -FilePath $file.FullName -SchemaPath $schema -Verbose:$false

        $status = if ($result.IsValid) {
            $validCount++
            Write-Host " ✅ Valid" -ForegroundColor Green
            "Valid"
        } else {
            $invalidCount++
            Write-Host " ❌ Invalid" -ForegroundColor Red
            "Invalid"
        }

        $results += [PSCustomObject]@{
            File = $file.FullName
            RelativePath = $file.FullName.Replace((Get-Location).Path, "").TrimStart('\')
            Name = $file.Name
            Extension = $file.Extension
            Status = $status
            Errors = $result.Errors
            HasSchema = $schema -ne $null
            SchemaPath = $schema
            Size = $file.Length
            LastModified = $file.LastWriteTime
        }
    }
    catch {
        $invalidCount++
        Write-Host " ❌ Error: $($_.Exception.Message)" -ForegroundColor Red

        $results += [PSCustomObject]@{
            File = $file.FullName
            RelativePath = $file.FullName.Replace((Get-Location).Path, "").TrimStart('\')
            Name = $file.Name
            Extension = $file.Extension
            Status = "Error"
            Errors = @(@{ Exception = @{ Message = $_.Exception.Message } })
            HasSchema = $schema -ne $null
            SchemaPath = $schema
            Size = $file.Length
            LastModified = $file.LastWriteTime
        }
    }
}

Write-Host ""
Write-Host "📊 Validation Summary:" -ForegroundColor Cyan
Write-Host "  Total files: $($results.Count)" -ForegroundColor White
Write-Host "  Valid files: $validCount" -ForegroundColor Green
Write-Host "  Invalid files: $invalidCount" -ForegroundColor $(if ($invalidCount -eq 0) { "Green" } else { "Red" })

# Generate timestamp for report
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$reportPath = "XMLValidationReport_$timestamp.html"

# Generate comprehensive HTML report
$htmlTemplate = @"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>BusBuddy XML Validation Report - $timestamp</title>
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 0;
            padding: 20px;
            background-color: #f5f5f5;
        }
        .container {
            max-width: 1200px;
            margin: 0 auto;
            background: white;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
        h1 {
            color: #2c3e50;
            border-bottom: 3px solid #3498db;
            padding-bottom: 10px;
        }
        h2 {
            color: #34495e;
            margin-top: 30px;
        }
        .summary {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 20px;
            margin: 20px 0;
        }
        .summary-card {
            background: #ecf0f1;
            padding: 15px;
            border-radius: 6px;
            text-align: center;
        }
        .summary-number {
            font-size: 2em;
            font-weight: bold;
            color: #2c3e50;
        }
        .valid { color: #27ae60; font-weight: bold; }
        .invalid { color: #e74c3c; font-weight: bold; }
        .error { color: #e74c3c; font-weight: bold; }
        .file-result {
            margin: 15px 0;
            padding: 15px;
            border-left: 4px solid #bdc3c7;
            background: #f8f9fa;
        }
        .file-result.valid { border-left-color: #27ae60; }
        .file-result.invalid { border-left-color: #e74c3c; }
        .file-result.error { border-left-color: #e67e22; }
        .file-header {
            font-size: 1.1em;
            margin-bottom: 10px;
        }
        .file-meta {
            font-size: 0.9em;
            color: #7f8c8d;
            margin: 5px 0;
        }
        .error-details {
            background: #fdf2f2;
            border: 1px solid #f5c6cb;
            padding: 10px;
            margin: 10px 0;
            border-radius: 4px;
        }
        .error-message {
            color: #721c24;
            font-family: 'Courier New', monospace;
            font-size: 0.9em;
        }
        .schema-info {
            background: #e8f4fd;
            border: 1px solid #bee5eb;
            padding: 8px;
            margin: 5px 0;
            border-radius: 4px;
            font-size: 0.9em;
        }
        .filter-buttons {
            margin: 20px 0;
        }
        .filter-btn {
            background: #3498db;
            color: white;
            border: none;
            padding: 8px 16px;
            margin: 0 5px;
            border-radius: 4px;
            cursor: pointer;
        }
        .filter-btn:hover { background: #2980b9; }
        .filter-btn.active { background: #2c3e50; }
        .hidden { display: none; }
    </style>
</head>
<body>
    <div class='container'>
        <h1>🚌 BusBuddy XML Validation Report</h1>
        <p><strong>Generated:</strong> $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")</p>

        <h2>📊 Summary</h2>
        <div class='summary'>
            <div class='summary-card'>
                <div class='summary-number'>$($results.Count)</div>
                <div>Total Files</div>
            </div>
            <div class='summary-card'>
                <div class='summary-number valid'>$validCount</div>
                <div>Valid Files</div>
            </div>
            <div class='summary-card'>
                <div class='summary-number invalid'>$invalidCount</div>
                <div>Invalid Files</div>
            </div>
            <div class='summary-card'>
                <div class='summary-number'>$($results.Where({$_.HasSchema}).Count)</div>
                <div>Schema Validated</div>
            </div>
        </div>

        <h2>🔍 File Types</h2>
        <div class='summary'>
"@

# Add file type breakdown
$fileTypes = $results | Group-Object Extension | Sort-Object Count -Descending
foreach ($type in $fileTypes) {
    $validInType = ($type.Group | Where-Object { $_.Status -eq 'Valid' }).Count
    $invalidInType = $type.Count - $validInType
    $htmlTemplate += @"
            <div class='summary-card'>
                <div class='summary-number'>$($type.Count)</div>
                <div>$($type.Name) files</div>
                <div style='font-size: 0.8em; margin-top: 5px;'>
                    <span class='valid'>$validInType valid</span> |
                    <span class='invalid'>$invalidInType invalid</span>
                </div>
            </div>
"@
}

$htmlTemplate += @"
        </div>

        <h2>📋 Detailed Results</h2>
        <div class='filter-buttons'>
            <button class='filter-btn active' onclick='filterResults("all")'>All Files</button>
            <button class='filter-btn' onclick='filterResults("valid")'>Valid Only</button>
            <button class='filter-btn' onclick='filterResults("invalid")'>Invalid Only</button>
            <button class='filter-btn' onclick='filterResults("error")'>Errors Only</button>
        </div>

        <div id='results'>
"@

# Add individual file results
foreach ($result in $results) {
    $statusClass = $result.Status.ToLower()
    $statusIcon = switch ($result.Status) {
        "Valid" { "✅" }
        "Invalid" { "❌" }
        "Error" { "⚠️" }
    }

    $htmlTemplate += @"
            <div class='file-result $statusClass' data-status='$statusClass'>
                <div class='file-header'>
                    $statusIcon <strong>$($result.Name)</strong>: <span class='$statusClass'>$($result.Status)</span>
                </div>
                <div class='file-meta'>
                    <strong>Path:</strong> $($result.RelativePath)<br>
                    <strong>Size:</strong> $([math]::Round($result.Size / 1KB, 2)) KB |
                    <strong>Modified:</strong> $($result.LastModified.ToString("yyyy-MM-dd HH:mm"))
                </div>
"@

    if ($result.HasSchema) {
        $htmlTemplate += @"
                <div class='schema-info'>
                    📋 <strong>Schema Validation:</strong> Enabled<br>
                    <small>$($result.SchemaPath)</small>
                </div>
"@
    }

    if ($result.Errors -and $result.Errors.Count -gt 0) {
        $htmlTemplate += "<div class='error-details'><strong>Validation Errors:</strong>"
        foreach ($validationError in $result.Errors) {
            $errorMessage = if ($validationError.Exception.Message) { $validationError.Exception.Message } else { $validationError.Message }
            $htmlTemplate += "<div class='error-message'>$errorMessage</div>"
        }
        $htmlTemplate += "</div>"
    }

    $htmlTemplate += "</div>"
}

$htmlTemplate += @"
        </div>
    </div>

    <script>
        function filterResults(filter) {
            const results = document.querySelectorAll('.file-result');
            const buttons = document.querySelectorAll('.filter-btn');

            // Update button states
            buttons.forEach(btn => btn.classList.remove('active'));
            event.target.classList.add('active');

            // Filter results
            results.forEach(result => {
                if (filter === 'all' || result.dataset.status === filter) {
                    result.style.display = 'block';
                } else {
                    result.style.display = 'none';
                }
            });
        }
    </script>
</body>
</html>
"@

# Save the report
$htmlTemplate | Out-File $reportPath -Encoding UTF8

Write-Host ""
Write-Host "📄 Report generated: $reportPath" -ForegroundColor Green
Write-Host "🌐 Opening report in browser..." -ForegroundColor Cyan

# Open the report in the default browser
Start-Process $reportPath

# Display quick summary in console
if ($invalidCount -eq 0) {
    Write-Host ""
    Write-Host "🎉 All XML files are valid!" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "⚠️ Found issues in $invalidCount files. Check the HTML report for details." -ForegroundColor Yellow

    # Show first few invalid files
    $invalidFiles = $results | Where-Object { $_.Status -ne 'Valid' } | Select-Object -First 3
    foreach ($invalid in $invalidFiles) {
        Write-Host "  - $($invalid.Name): $($invalid.Status)" -ForegroundColor Red
        if ($invalid.Errors -and $invalid.Errors.Count -gt 0) {
            $firstError = $invalid.Errors[0]
            $errorMsg = if ($firstError.Exception.Message) { $firstError.Exception.Message } else { $firstError.Message }
            Write-Host "    Error: $errorMsg" -ForegroundColor Yellow
        }
    }

    if ($invalidCount -gt 3) {
        Write-Host "  ... and $($invalidCount - 3) more. See full report for details." -ForegroundColor Gray
    }
}

# Return success/failure code
if ($invalidCount -eq 0) {
    Write-Host "`n✅ All XML files are valid!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`n❌ Found $invalidCount invalid XML files. Please review and fix the issues." -ForegroundColor Red
    exit 1
}
