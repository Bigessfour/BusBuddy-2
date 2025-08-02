# 🚌 BusBuddy PowerShell XAML Validator
# Based on Microsoft PowerShell + WPF techniques
# Validates XAML files and code-behind element references

param(
    [string]$ProjectPath = (Get-Location),
    [switch]$Detailed,
    [switch]$FixCodeBehind,
    [string]$OutputFormat = "Console" # Console, Json, Workflow
)

# Add required assemblies for XAML processing
Add-Type -AssemblyName PresentationFramework
Add-Type -AssemblyName PresentationCore
Add-Type -AssemblyName WindowsBase

function Test-BusBuddyXamlComplete {
    param(
        [string]$ProjectPath,
        [switch]$Detailed,
        [switch]$FixCodeBehind,
        [string]$OutputFormat
    )

    $results = @{
        TotalXamlFiles = 0
        ValidXamlFiles = 0
        InvalidXamlFiles = 0
        CodeBehindIssues = 0
        ElementMissingIssues = 0
        Issues = @()
        ValidatedFiles = @()
    }

    # Find all XAML files in the project
    $xamlFiles = Get-ChildItem -Path $ProjectPath -Recurse -Filter "*.xaml" |
                 Where-Object { $_.Directory.Name -notlike "*bin*" -and $_.Directory.Name -notlike "*obj*" }

    $results.TotalXamlFiles = $xamlFiles.Count

    Write-Host "🔍 Found $($xamlFiles.Count) XAML files in project" -ForegroundColor Cyan

    foreach ($xamlFile in $xamlFiles) {
        Write-Host "   📄 Validating: $($xamlFile.Name)" -ForegroundColor Yellow

        $fileResult = Test-XamlFileComplete -FilePath $xamlFile.FullName -Detailed:$Detailed

        if ($fileResult.IsValid) {
            $results.ValidXamlFiles++
            Write-Host "      ✅ XAML syntax valid" -ForegroundColor Green
        } else {
            $results.InvalidXamlFiles++
            Write-Host "      ❌ XAML syntax errors found" -ForegroundColor Red
            $results.Issues += $fileResult
        }

        # Check for code-behind file
        $codeBehindPath = $xamlFile.FullName -replace '\.xaml$', '.xaml.cs'
        if (Test-Path $codeBehindPath) {
            $codeBehindResult = Test-CodeBehindElements -XamlPath $xamlFile.FullName -CodeBehindPath $codeBehindPath

            if ($codeBehindResult.MissingElements.Count -gt 0) {
                $results.CodeBehindIssues++
                $results.ElementMissingIssues += $codeBehindResult.MissingElements.Count
                $results.Issues += $codeBehindResult

                Write-Host "      ⚠️  Code-behind references $($codeBehindResult.MissingElements.Count) missing elements" -ForegroundColor Magenta

                if ($FixCodeBehind) {
                    # Generate missing elements in XAML
                    Add-MissingElementsToXaml -XamlPath $xamlFile.FullName -MissingElements $codeBehindResult.MissingElements
                }
            } else {
                Write-Host "      ✅ Code-behind elements validated" -ForegroundColor Green
            }
        }

        $results.ValidatedFiles += $fileResult
    }

    # Output results
    switch ($OutputFormat) {
        "Json" {
            $results | ConvertTo-Json -Depth 10
        }
        "Workflow" {
            Write-WorkflowOutput $results
        }
        default {
            Write-ConsoleOutput $results
        }
    }

    return $results
}

function Test-XamlFileComplete {
    param(
        [string]$FilePath,
        [switch]$Detailed
    )

    $result = @{
        FilePath = $FilePath
        IsValid = $false
        LoadedSuccessfully = $false
        NamedElements = @()
        Errors = @()
        XamlObject = $null
    }

    try {
        # Read XAML content
        $xamlContent = Get-Content -Path $FilePath -Raw

        # Create XML reader for validation
        $stringReader = New-Object System.IO.StringReader($xamlContent)
        $xmlReader = [System.Xml.XmlReader]::Create($stringReader)

        # Load XAML using WPF XamlReader (this validates WPF-specific syntax)
        $xamlObject = [System.Windows.Markup.XamlReader]::Load($xmlReader)

        $result.LoadedSuccessfully = $true
        $result.XamlObject = $xamlObject
        $result.IsValid = $true

        # Extract named elements using PowerShell + WPF techniques
        $result.NamedElements = Get-NamedElementsFromXaml -XamlObject $xamlObject

        Write-Verbose "✅ Successfully loaded XAML: $FilePath"

    } catch {
        $result.Errors += @{
            Type = "XamlLoadError"
            Message = $_.Exception.Message
            Line = if ($_.Exception.LineNumber) { $_.Exception.LineNumber } else { "Unknown" }
        }
        Write-Verbose "❌ Failed to load XAML: $($_.Exception.Message)"
    } finally {
        if ($xmlReader) { $xmlReader.Close() }
        if ($stringReader) { $stringReader.Close() }
    }

    return $result
}

function Get-NamedElementsFromXaml {
    param($XamlObject)

    $namedElements = @()

    # Use WPF LogicalTreeHelper to walk the visual tree
    function Get-ChildElements($element) {
        $children = @()

        try {
            # Get logical children
            $logicalChildren = [System.Windows.LogicalTreeHelper]::GetChildren($element)
            foreach ($child in $logicalChildren) {
                $children += $child
                $children += Get-ChildElements $child
            }
        } catch {
            # Fallback for elements without logical children
        }

        return $children
    }

    # Get all elements including the root
    $allElements = @($XamlObject) + (Get-ChildElements $XamlObject)

    # Extract named elements
    foreach ($element in $allElements) {
        if ($element -and $element.Name) {
            $namedElements += @{
                Name = $element.Name
                Type = $element.GetType().Name
                FullTypeName = $element.GetType().FullName
            }
        }
    }

    return $namedElements
}

function Test-CodeBehindElements {
    param(
        [string]$XamlPath,
        [string]$CodeBehindPath
    )

    $result = @{
        XamlPath = $XamlPath
        CodeBehindPath = $CodeBehindPath
        ReferencedElements = @()
        AvailableElements = @()
        MissingElements = @()
        ValidElements = @()
    }

    # Get XAML named elements
    $xamlResult = Test-XamlFileComplete -FilePath $XamlPath
    if ($xamlResult.IsValid) {
        $result.AvailableElements = $xamlResult.NamedElements | ForEach-Object { $_.Name }
    }

    # Parse code-behind for element references
    $codeBehindContent = Get-Content -Path $CodeBehindPath -Raw

    # Find element references (like StatusText.Text, ContentFrame.Navigate, etc.)
    $elementReferences = [regex]::Matches($codeBehindContent, '\b([A-Z][a-zA-Z0-9_]*)\.(Text|Navigate|Content|Click|IsEnabled|Visibility|[A-Z][a-zA-Z0-9_]*)')

    foreach ($match in $elementReferences) {
        $elementName = $match.Groups[1].Value

        # Skip common .NET types and keywords
        if ($elementName -notin @('System', 'String', 'Console', 'MessageBox', 'Exception', 'Task', 'Thread', 'File', 'Directory', 'Path')) {
            $result.ReferencedElements += $elementName
        }
    }

    # Remove duplicates
    $result.ReferencedElements = $result.ReferencedElements | Sort-Object -Unique

    # Find missing elements
    foreach ($referencedElement in $result.ReferencedElements) {
        if ($referencedElement -in $result.AvailableElements) {
            $result.ValidElements += $referencedElement
        } else {
            $result.MissingElements += $referencedElement
        }
    }

    return $result
}

function Add-MissingElementsToXaml {
    param(
        [string]$XamlPath,
        [array]$MissingElements
    )

    Write-Host "🔧 Adding missing elements to XAML: $XamlPath" -ForegroundColor Yellow

    # This would implement auto-fixing logic
    # For now, just report what would be added
    foreach ($element in $MissingElements) {
        Write-Host "   + Would add: <TextBlock x:Name='$element' />" -ForegroundColor Cyan
    }
}

function Write-ConsoleOutput {
    param($Results)

    Write-Host "`n🎯 BusBuddy XAML Validation Results" -ForegroundColor Green
    Write-Host "================================" -ForegroundColor Green
    Write-Host "📊 Total XAML Files: $($Results.TotalXamlFiles)" -ForegroundColor White
    Write-Host "✅ Valid XAML Files: $($Results.ValidXamlFiles)" -ForegroundColor Green
    Write-Host "❌ Invalid XAML Files: $($Results.InvalidXamlFiles)" -ForegroundColor Red
    Write-Host "⚠️  Code-behind Issues: $($Results.CodeBehindIssues)" -ForegroundColor Yellow
    Write-Host "🔗 Missing Element References: $($Results.ElementMissingIssues)" -ForegroundColor Magenta

    if ($Results.Issues.Count -gt 0) {
        Write-Host "`n📋 Issues Found:" -ForegroundColor Red
        foreach ($issue in $Results.Issues) {
            if ($issue.MissingElements) {
                Write-Host "   File: $($issue.CodeBehindPath | Split-Path -Leaf)" -ForegroundColor Yellow
                foreach ($missing in $issue.MissingElements) {
                    Write-Host "      Missing: $missing" -ForegroundColor Red
                }
            }
        }
    }

    $exitCode = if ($Results.InvalidXamlFiles -gt 0 -or $Results.CodeBehindIssues -gt 0) { 1 } else { 0 }
    Write-Host "`n🎯 Validation Complete - Exit Code: $exitCode" -ForegroundColor $(if ($exitCode -eq 0) { "Green" } else { "Red" })

    return $exitCode
}

function Write-WorkflowOutput {
    param($Results)

    # GitHub Actions workflow output format
    Write-Host "::group::🎯 BusBuddy XAML Validation Results"
    Write-Host "total-xaml-files=$($Results.TotalXamlFiles)"
    Write-Host "valid-xaml-files=$($Results.ValidXamlFiles)"
    Write-Host "invalid-xaml-files=$($Results.InvalidXamlFiles)"
    Write-Host "code-behind-issues=$($Results.CodeBehindIssues)"
    Write-Host "missing-elements=$($Results.ElementMissingIssues)"

    if ($Results.Issues.Count -gt 0) {
        Write-Host "::group::❌ Issues Found"
        foreach ($issue in $Results.Issues) {
            if ($issue.MissingElements) {
                foreach ($missing in $issue.MissingElements) {
                    Write-Host "::error file=$($issue.CodeBehindPath)::Missing XAML element: $missing"
                }
            }
        }
        Write-Host "::endgroup::"
    }

    Write-Host "::endgroup::"

    # Set workflow output
    $exitCode = if ($Results.InvalidXamlFiles -gt 0 -or $Results.CodeBehindIssues -gt 0) { 1 } else { 0 }
    Write-Host "validation-status=$(if ($exitCode -eq 0) { 'success' } else { 'failure' })"

    return $exitCode
}

# Export functions for module use (only when loaded as module)
if ($MyInvocation.MyCommand.ModuleName) {
    Export-ModuleMember -Function Test-BusBuddyXamlComplete, Test-XamlFileComplete, Test-CodeBehindElements
}

# If running as script (not module), execute validation
if ($MyInvocation.ScriptName -eq $PSCommandPath) {
    $exitCode = Test-BusBuddyXamlComplete -ProjectPath $ProjectPath -Detailed:$Detailed -FixCodeBehind:$FixCodeBehind -OutputFormat $OutputFormat
    exit $exitCode
}
