# Read-Only Analysis Tools for BusBuddy - ENHANCED WITH POWERSHELL 7.5 FEATURES
# SAFE: These tools only analyze and report - they never modify files
# ENHANCED: Advanced detection for Claude Sonnet 4 to provide precise fixes
# POWERSHELL 7.5: Leverages new PS 7.5 performance and functionality improvements

#Requires -Version 7.5

# PowerShell 7.5 performance optimizations
using namespace System.Management.Automation.Language
using namespace System.Text.RegularExpressions

#region PowerShell 7.5 Validation and Features

function Test-PowerShell75Compatibility {
    <#
    .SYNOPSIS
    Validates PowerShell 7.5 features are available for enhanced analysis
    .DESCRIPTION
    READ-ONLY: Tests availability of PS 7.5 features used in analysis tools
    #>

    $features = @{
        'Version'                = $PSVersionTable.PSVersion -ge [Version]'7.5.0'
        'ConvertTo-CliXml'       = Get-Command ConvertTo-CliXml -ErrorAction SilentlyContinue
        'ConvertFrom-CliXml'     = Get-Command ConvertFrom-CliXml -ErrorAction SilentlyContinue
        'TestJsonEnhancements'   = (Get-Command Test-Json).Parameters.ContainsKey('IgnoreComments')
        'ArrayPerformance'       = $PSVersionTable.PSVersion -ge [Version]'7.5.0' # += optimization
        'ImprovedErrorReporting' = $PSVersionTable.PSVersion -ge [Version]'7.5.0'
    }

    return @{
        IsCompatible       = $features.Version
        AvailableFeatures  = $features
        RecommendedActions = if (-not $features.Version) {
            @('Upgrade to PowerShell 7.5+ for optimal performance and features')
        } else {
            @('All PowerShell 7.5 features available')
        }
    }
}

function Show-PowerShell75Capabilities {
    <#
    .SYNOPSIS
    Displays PowerShell 7.5 capabilities and feature status
    .DESCRIPTION
    READ-ONLY: Shows which PS 7.5 features are available for enhanced analysis
    #>

    Write-Host '📊 PowerShell 7.5 Feature Analysis' -ForegroundColor Yellow
    Write-Host '=================================' -ForegroundColor Yellow

    $compatibility = Test-PowerShell75Compatibility

    Write-Host 'Current PowerShell Version: ' -NoNewline
    Write-Host $PSVersionTable.PSVersion -ForegroundColor $(if ($compatibility.IsCompatible) { 'Green' } else { 'Red' })

    Write-Host "`n🔧 Available Features:" -ForegroundColor Cyan
    foreach ($feature in $compatibility.AvailableFeatures.GetEnumerator()) {
        $status = if ($feature.Value) { '✅' } else { '❌' }
        $color = if ($feature.Value) { 'Green' } else { 'Red' }
        Write-Host "  $status $($feature.Key)" -ForegroundColor $color
    }

    Write-Host "`n💡 Recommendations:" -ForegroundColor Magenta
    foreach ($action in $compatibility.RecommendedActions) {
        Write-Host "  • $action" -ForegroundColor White
    }

    Write-Host "`n🚀 Enhanced Capabilities:" -ForegroundColor Yellow
    if ($compatibility.IsCompatible) {
        Write-Host '  • Optimized array operations (faster += performance)' -ForegroundColor Green
        Write-Host '  • Enhanced XML/JSON processing with CliXml cmdlets' -ForegroundColor Green
        Write-Host '  • Improved error reporting with detailed stack traces' -ForegroundColor Green
        Write-Host '  • Better memory management for large file analysis' -ForegroundColor Green
    } else {
        Write-Host '  • Upgrade to PowerShell 7.5+ to unlock enhanced features' -ForegroundColor Yellow
    }
}

#endregion

#region XAML Analysis Tools (READ-ONLY) - ENHANCED

function Analyze-XamlStructure {
    <#
    .SYNOPSIS
    Analyzes XAML file structure and reports issues with advanced detection
    .DESCRIPTION
    READ-ONLY: Only analyzes, never modifies files
    ENHANCED: Provides precise line/column information for Claude Sonnet 4
    #>
    param(
        [Parameter(Mandatory)]
        [string]$FilePath
    )

    if (-not (Test-Path $FilePath)) {
        return @{
            IsValid   = $false
            Errors    = @("File not found: $FilePath")
            Warnings  = @()
            Structure = $null
            Enhanced  = @{
                CorruptionRisk = 'High'
                FixStrategy    = 'File recreation required'
            }
        }
    }

    try {
        $content = Get-Content $FilePath -Raw
        $lines = $content -split "`n"

        # Advanced XML validation attempt
        $xmlValid = $false
        $parseError = $null
        try {
            [xml]$xaml = $content
            $xmlValid = $true
        } catch {
            $parseError = $_.Exception.Message
        }

        $analysis = @{
            IsValid     = $xmlValid
            Errors      = @()
            Warnings    = @()
            Structure   = @{
                RootElement = $null
                Namespaces  = @()
                Elements    = @()
                Attributes  = @()
                TotalLines  = $lines.Count
            }
            LineNumbers = @{}
            Enhanced    = @{
                CorruptionRisk   = 'Low'
                FixStrategy      = 'None needed'
                PreciseLocations = @()
                SyncfusionIssues = @()
                BindingIssues    = @()
                StructuralIssues = @()
                WellFormedness   = $xmlValid
            }
        }

        if (-not $xmlValid) {
            $analysis.Errors += "XML parsing failed: $parseError"
            $analysis.Enhanced.CorruptionRisk = 'Critical'
            $analysis.Enhanced.FixStrategy = 'Manual structure repair required'

            # Try to identify specific structural issues
            $unclosedTags = Find-UnclosedXmlTags -Content $content -Lines $lines
            $analysis.Enhanced.StructuralIssues += $unclosedTags

            return $analysis
        }

        # If XML is valid, proceed with detailed analysis
        $analysis.Structure.RootElement = $xaml.DocumentElement.Name

        # Enhanced namespace analysis
        foreach ($attr in $xaml.DocumentElement.Attributes) {
            if ($attr.Name.StartsWith('xmlns')) {
                $analysis.Structure.Namespaces += @{
                    Prefix = $attr.Name -replace 'xmlns:?', ''
                    URI    = $attr.Value
                    Line   = Get-LineNumberForText -Content $content -SearchText $attr.Value
                }
            }
        }

        # ENHANCED: Syncfusion-specific analysis
        $syncfusionAnalysis = Analyze-SyncfusionUsage -Content $content -Lines $lines
        $analysis.Enhanced.SyncfusionIssues = $syncfusionAnalysis
        $analysis.Warnings += $syncfusionAnalysis | Where-Object { $_.Severity -eq 'Warning' } | ForEach-Object { $_.Message }
        $analysis.Errors += $syncfusionAnalysis | Where-Object { $_.Severity -eq 'Error' } | ForEach-Object { $_.Message }

        # ENHANCED: Binding expression analysis
        $bindingAnalysis = Analyze-BindingExpressions -Content $content -Lines $lines
        $analysis.Enhanced.BindingIssues = $bindingAnalysis
        $analysis.Warnings += $bindingAnalysis | Where-Object { $_.Severity -eq 'Warning' } | ForEach-Object { $_.Message }
        $analysis.Errors += $bindingAnalysis | Where-Object { $_.Severity -eq 'Error' } | ForEach-Object { $_.Message }

        # ENHANCED: Attribute validation
        $attributeIssues = Analyze-XamlAttributes -Xaml $xaml -Content $content
        $analysis.Enhanced.PreciseLocations += $attributeIssues

        # Set corruption risk based on findings
        if ($analysis.Errors.Count -gt 0) {
            $analysis.Enhanced.CorruptionRisk = 'High'
            $analysis.Enhanced.FixStrategy = 'Targeted repairs needed at specific locations'
        } elseif ($analysis.Warnings.Count -gt 3) {
            $analysis.Enhanced.CorruptionRisk = 'Medium'
            $analysis.Enhanced.FixStrategy = 'Preventive fixes recommended'
        }

        return $analysis
    } catch {
        return @{
            IsValid   = $false
            Errors    = @("Critical analysis error: $($_.Exception.Message)")
            Warnings  = @()
            Structure = $null
            Enhanced  = @{
                CorruptionRisk = 'Critical'
                FixStrategy    = 'Manual intervention required'
            }
        }
    }
}

function Find-UnclosedXmlTags {
    param($Content, $Lines)

    $issues = @()
    $tagStack = @()

    for ($i = 0; $i -lt $Lines.Count; $i++) {
        $line = $Lines[$i]

        # Find opening tags
        $openingTags = [regex]::Matches($line, '<(\w+(?::\w+)?)[^>]*(?<!/)>')
        foreach ($match in $openingTags) {
            $tagStack += @{
                Name   = $match.Groups[1].Value
                Line   = $i + 1
                Column = $match.Index + 1
            }
        }

        # Find closing tags
        $closingTags = [regex]::Matches($line, '</(\w+(?::\w+)?)>')
        foreach ($match in $closingTags) {
            $tagName = $match.Groups[1].Value
            if ($tagStack.Count -gt 0 -and $tagStack[-1].Name -eq $tagName) {
                $tagStack = $tagStack[0..($tagStack.Count - 2)]
            } else {
                $issues += @{
                    Type     = 'Mismatched closing tag'
                    Message  = "Closing tag '$tagName' without matching opening tag"
                    Line     = $i + 1
                    Column   = $match.Index + 1
                    Severity = 'Error'
                }
            }
        }
    }

    # Report unclosed tags
    foreach ($unclosedTag in $tagStack) {
        $issues += @{
            Type     = 'Unclosed tag'
            Message  = "Unclosed tag '$($unclosedTag.Name)'"
            Line     = $unclosedTag.Line
            Column   = $unclosedTag.Column
            Severity = 'Error'
        }
    }

    return $issues
}

function Analyze-SyncfusionUsage {
    param($Content, $Lines)

    $issues = @()

    # Check for Syncfusion controls without namespace
    if ($Content -match 'syncfusion:' -and $Content -notmatch 'xmlns:syncfusion=') {
        $syncfusionMatches = [regex]::Matches($Content, 'syncfusion:\w+')
        foreach ($match in $syncfusionMatches) {
            $lineNumber = Get-LineNumberForPosition -Content $Content -Position $match.Index
            $issues += @{
                Type          = 'Missing namespace'
                Message       = 'Syncfusion control used without namespace declaration'
                Line          = $lineNumber
                Column        = $match.Index - ($Content.Substring(0, $match.Index).LastIndexOf("`n"))
                Severity      = 'Error'
                FixSuggestion = "Add xmlns:syncfusion='http://schemas.syncfusion.com/wpf' to root element"
            }
        }
    }

    # Check for deprecated Syncfusion properties
    $deprecatedPatterns = @{
        'EnableRippleAnimation' = 'Use RippleAnimationHelper instead'
        'EnableTouch'           = 'Touch is enabled by default in newer versions'
        'VisualStyle'           = 'Use Theme property instead'
    }

    foreach ($pattern in $deprecatedPatterns.Keys) {
        $regexMatches = [regex]::Matches($Content, "$pattern\s*=", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        foreach ($match in $regexMatches) {
            $lineNumber = Get-LineNumberForPosition -Content $Content -Position $match.Index
            $issues += @{
                Type          = 'Deprecated property'
                Message       = "Deprecated Syncfusion property: $pattern"
                Line          = $lineNumber
                Column        = $match.Index - ($Content.Substring(0, $match.Index).LastIndexOf("`n"))
                Severity      = 'Warning'
                FixSuggestion = $deprecatedPatterns[$pattern]
            }
        }
    }

    return $issues
}

function Analyze-BindingExpressions {
    param($Content, $Lines)

    $issues = @()

    # Check for empty bindings
    $emptyBindings = [regex]::Matches($Content, '\{Binding\s*\}')
    foreach ($match in $emptyBindings) {
        $lineNumber = Get-LineNumberForPosition -Content $Content -Position $match.Index
        $issues += @{
            Type          = 'Empty binding'
            Message       = 'Empty binding expression'
            Line          = $lineNumber
            Column        = $match.Index - ($Content.Substring(0, $match.Index).LastIndexOf("`n"))
            Severity      = 'Warning'
            FixSuggestion = 'Specify a binding path or remove the binding'
        }
    }

    # Check for malformed bindings
    $malformedBindings = [regex]::Matches($Content, '\{Binding\s+[^}]*[^}]\s*$', [System.Text.RegularExpressions.RegexOptions]::Multiline)
    foreach ($match in $malformedBindings) {
        $lineNumber = Get-LineNumberForPosition -Content $Content -Position $match.Index
        $issues += @{
            Type          = 'Malformed binding'
            Message       = 'Potentially malformed binding expression'
            Line          = $lineNumber
            Column        = $match.Index - ($Content.Substring(0, $match.Index).LastIndexOf("`n"))
            Severity      = 'Error'
            FixSuggestion = 'Ensure binding expression is properly closed with }'
            Context       = $match.Value
        }
    }

    # Check for binding syntax errors
    $bindingErrors = [regex]::Matches($Content, '\{Binding\s+[^}]*[,]\s*[,]')
    foreach ($match in $bindingErrors) {
        $lineNumber = Get-LineNumberForPosition -Content $Content -Position $match.Index
        $issues += @{
            Type          = 'Binding syntax error'
            Message       = 'Double comma in binding expression'
            Line          = $lineNumber
            Column        = $match.Index - ($Content.Substring(0, $match.Index).LastIndexOf("`n"))
            Severity      = 'Error'
            FixSuggestion = 'Remove duplicate comma in binding expression'
        }
    }

    return $issues
}

function Analyze-XamlAttributes {
    param($Xaml, $Content)

    $issues = @()

    # Check for duplicate attributes
    $allElements = $Xaml.SelectNodes('//*')
    foreach ($element in $allElements) {
        $attributeNames = @()
        foreach ($attr in $element.Attributes) {
            if ($attributeNames -contains $attr.Name) {
                $lineNumber = Get-LineNumberForText -Content $Content -SearchText $attr.Value
                $issues += @{
                    Type      = 'Duplicate attribute'
                    Message   = "Duplicate attribute '$($attr.Name)' on element '$($element.Name)'"
                    Line      = $lineNumber
                    Severity  = 'Error'
                    Element   = $element.Name
                    Attribute = $attr.Name
                }
            }
            $attributeNames += $attr.Name
        }
    }

    return $issues
}

function Get-LineNumberForPosition {
    param($Content, $Position)
    return ($Content.Substring(0, $Position) -split "`n").Count
}

function Get-LineNumberForText {
    param($Content, $SearchText)
    $index = $Content.IndexOf($SearchText)
    if ($index -ge 0) {
        return ($Content.Substring(0, $index) -split "`n").Count
    }
    return 1
}

function Find-XamlElements {
    <#
    .SYNOPSIS
    Finds XAML elements matching criteria
    .DESCRIPTION
    READ-ONLY: Only searches and reports locations
    #>
    param(
        [Parameter(Mandatory)]
        [string]$FilePath,

        [string]$ElementName,
        [string]$AttributeName,
        [string]$AttributeValue,
        [string]$XPath
    )

    try {
        [xml]$xaml = Get-Content $FilePath -Raw
        $results = @()

        if ($XPath) {
            $elements = $xaml.SelectNodes($XPath)
        } elseif ($ElementName) {
            $elements = $xaml.SelectNodes("//$ElementName")
        } else {
            $elements = $xaml.SelectNodes('//*')
        }

        foreach ($element in $elements) {
            if ($AttributeName) {
                $attr = $element.GetAttribute($AttributeName)
                if ($AttributeValue -and $attr -ne $AttributeValue) {
                    continue
                }
                if (-not $AttributeValue -and -not $attr) {
                    continue
                }
            }

            $results += @{
                ElementName = $element.Name
                XPath       = Get-XPath -Element $element
                Attributes  = @{}
                OuterXml    = $element.OuterXml
            }

            foreach ($attr in $element.Attributes) {
                $results[-1].Attributes[$attr.Name] = $attr.Value
            }
        }

        return $results
    } catch {
        Write-Error "Error searching XAML: $($_.Exception.Message)"
        return @()
    }
}

function Get-XPath {
    param($Element)

    $path = ''
    $current = $Element

    while ($current -and $current.NodeType -eq [System.Xml.XmlNodeType]::Element) {
        $siblings = @($current.ParentNode.ChildNodes | Where-Object { $_.NodeType -eq [System.Xml.XmlNodeType]::Element -and $_.Name -eq $current.Name })

        if ($siblings.Count -gt 1) {
            $index = [Array]::IndexOf($siblings, $current) + 1
            $path = "/$($current.Name)[$index]$path"
        } else {
            $path = "/$($current.Name)$path"
        }

        $current = $current.ParentNode
    }

    return $path
}

#endregion

#region PowerShell Analysis Tools (READ-ONLY)

function Analyze-PowerShellSyntax {
    <#
    .SYNOPSIS
    Analyzes PowerShell syntax and reports detailed issues
    .DESCRIPTION
    READ-ONLY: Only analyzes, never modifies files
    #>
    param(
        [Parameter(Mandatory)]
        [string]$FilePath
    )

    if (-not (Test-Path $FilePath)) {
        return @{
            IsValid  = $false
            Errors   = @("File not found: $FilePath")
            Warnings = @()
            Tokens   = @()
        }
    }

    try {
        $content = Get-Content $FilePath -Raw
        $tokens = $null
        $parseErrors = $null

        # Parse PowerShell syntax
        $ast = [System.Management.Automation.Language.Parser]::ParseInput($content, [ref]$tokens, [ref]$parseErrors)

        $analysis = @{
            IsValid   = $parseErrors.Count -eq 0
            Errors    = @()
            Warnings  = @()
            Tokens    = @()
            Functions = @()
            Variables = @()
            Commands  = @()
        }

        # Process parse errors with line numbers
        foreach ($parseError in $parseErrors) {
            $analysis.Errors += @{
                Message  = $parseError.Message
                Line     = $parseError.Extent.StartLineNumber
                Column   = $parseError.Extent.StartColumnNumber
                Severity = 'Error'
                Code     = $parseError.ErrorId
            }
        }

        # Analyze tokens for potential issues
        foreach ($token in $tokens) {
            $tokenInfo = @{
                Type   = $token.Kind.ToString()
                Text   = $token.Text
                Line   = $token.Extent.StartLineNumber
                Column = $token.Extent.StartColumnNumber
            }
            $analysis.Tokens += $tokenInfo

            # Check for problematic patterns
            if ($token.Kind -eq [System.Management.Automation.Language.TokenKind]::Variable) {
                if ($token.Text -eq '$error') {
                    $analysis.Warnings += @{
                        Message  = "Using reserved variable '$error'"
                        Line     = $token.Extent.StartLineNumber
                        Column   = $token.Extent.StartColumnNumber
                        Severity = 'Warning'
                    }
                }
            }
        }

        # Find functions
        $functionNodes = $ast.FindAll({ $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] }, $true)
        foreach ($func in $functionNodes) {
            $analysis.Functions += @{
                Name       = $func.Name
                Line       = $func.Extent.StartLineNumber
                Parameters = @($func.Parameters | ForEach-Object { $_.Name.VariablePath.UserPath })
            }
        }

        return $analysis
    } catch {
        return @{
            IsValid  = $false
            Errors   = @("Analysis error: $($_.Exception.Message)")
            Warnings = @()
            Tokens   = @()
        }
    }
}

function Find-PowerShellPatterns {
    <#
    .SYNOPSIS
    Finds specific patterns in PowerShell code
    .DESCRIPTION
    READ-ONLY: Only searches and reports locations
    #>
    param(
        [Parameter(Mandatory)]
        [string]$FilePath,

        [string[]]$Patterns = @(
            'Write-Host',
            'Invoke-Expression',
            '\$error',
            '\$\w+\s*=\s*\$null',
            'param\s*\(',
            '\[Parameter\(',
            'try\s*{',
            'catch\s*{',
            'finally\s*{'
        )
    )

    try {
        $content = Get-Content $FilePath -Raw
        $lines = $content -split "`n"
        $results = @()

        foreach ($pattern in $Patterns) {
            $patternMatches = [regex]::Matches($content, $pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

            foreach ($match in $patternMatches) {
                $lineNumber = ($content.Substring(0, $match.Index) -split "`n").Count
                $lineContent = $lines[$lineNumber - 1].Trim()

                $results += @{
                    Pattern     = $pattern
                    Match       = $match.Value
                    Line        = $lineNumber
                    Column      = $match.Index - ($content.Substring(0, $match.Index).LastIndexOf("`n"))
                    LineContent = $lineContent
                    Context     = Get-LineContext -Lines $lines -LineNumber ($lineNumber - 1) -ContextLines 2
                }
            }
        }

        return $results | Sort-Object Line
    } catch {
        Write-Error "Error analyzing patterns: $($_.Exception.Message)"
        return @()
    }
}

function Get-LineContext {
    param(
        [string[]]$Lines,
        [int]$LineNumber,
        [int]$ContextLines = 2
    )

    $start = [Math]::Max(0, $LineNumber - $ContextLines)
    $end = [Math]::Min($Lines.Count - 1, $LineNumber + $ContextLines)

    $context = @()
    for ($i = $start; $i -le $end; $i++) {
        $prefix = if ($i -eq $LineNumber) { '>>>' } else { '   ' }
        $context += "$prefix $($i + 1): $($Lines[$i])"
    }

    return $context
}

#endregion

#region C# Analysis Tools (READ-ONLY) - ENHANCED

function Analyze-CSharpSyntax {
    <#
    .SYNOPSIS
    Analyzes C# syntax with cutting-edge structural issue detection
    .DESCRIPTION
    READ-ONLY: Advanced analysis for Claude Sonnet 4 to provide precise fixes
    ENHANCED: Detects brace mismatches, method boundaries, class structure issues
    #>
    param(
        [Parameter(Mandatory)]
        [string]$FilePath
    )

    if (-not (Test-Path $FilePath)) {
        return @{
            IsValid       = $false
            Errors        = @("File not found: $FilePath")
            BraceAnalysis = $null
            Enhanced      = @{
                CorruptionRisk = 'Unknown'
                FixStrategy    = 'File not accessible'
            }
        }
    }

    try {
        $content = Get-Content $FilePath -Raw
        $lines = $content -split "`n"

        $analysis = @{
            IsValid       = $true
            Errors        = @()
            Warnings      = @()
            BraceAnalysis = @{
                OpenBraces       = @()
                CloseBraces      = @()
                Mismatches       = @()
                UnmatchedOpening = @()
                UnmatchedClosing = @()
                Balance          = 0
            }
            Enhanced      = @{
                CorruptionRisk      = 'Low'
                FixStrategy         = 'None needed'
                MethodBoundaries    = @()
                ClassStructure      = @()
                NamespaceIssues     = @()
                UsingDirectives     = @()
                PreciseLocations    = @()
                StructuralIntegrity = 'Good'
            }
        }

        # ENHANCED: Method boundary detection
        $analysis.Enhanced.MethodBoundaries = Find-CSharpMethodBoundaries -Content $content -Lines $lines

        # ENHANCED: Class structure analysis
        $analysis.Enhanced.ClassStructure = Analyze-CSharpClassStructure -Content $content -Lines $lines

        # ENHANCED: Using directive analysis
        $analysis.Enhanced.UsingDirectives = Analyze-UsingDirectives -Content $content -Lines $lines

        # ENHANCED: Advanced brace analysis with context
        $braceAnalysis = Analyze-CSharpBraces -Content $content -Lines $lines
        $analysis.BraceAnalysis = $braceAnalysis.BraceAnalysis
        $analysis.Errors += $braceAnalysis.Errors
        $analysis.Enhanced.PreciseLocations += $braceAnalysis.PreciseLocations

        # ENHANCED: Detect orphaned code (code outside method/class context)
        $orphanedCode = Find-OrphanedCode -Content $content -Lines $lines -MethodBoundaries $analysis.Enhanced.MethodBoundaries
        if ($orphanedCode.Count -gt 0) {
            $analysis.Enhanced.StructuralIntegrity = 'Compromised'
            $analysis.Enhanced.CorruptionRisk = 'Critical'
            $analysis.Enhanced.FixStrategy = 'Structural repair required - orphaned code detected'
            $analysis.Errors += $orphanedCode | ForEach-Object { "Orphaned code at line $($_.Line): $($_.Context)" }
        }

        # ENHANCED: Analyze method signature corruption
        $corruptedMethods = Find-CorruptedMethodSignatures -Content $content -Lines $lines
        if ($corruptedMethods.Count -gt 0) {
            $analysis.Enhanced.CorruptionRisk = 'High'
            $analysis.Enhanced.FixStrategy = 'Method signature repairs needed'
            $analysis.Errors += $corruptedMethods | ForEach-Object { "Corrupted method signature at line $($_.Line): $($_.Issue)" }
        }

        # Set overall corruption risk
        if ($analysis.Errors.Count -gt 5) {
            $analysis.Enhanced.CorruptionRisk = 'Critical'
            $analysis.Enhanced.FixStrategy = 'Comprehensive structural repair required'
        } elseif ($analysis.Errors.Count -gt 0) {
            $analysis.Enhanced.CorruptionRisk = 'High'
            $analysis.Enhanced.FixStrategy = 'Targeted repairs at specific locations'
        }

        $analysis.IsValid = $analysis.Errors.Count -eq 0

        return $analysis
    } catch {
        return @{
            IsValid       = $false
            Errors        = @("Critical analysis error: $($_.Exception.Message)")
            BraceAnalysis = $null
            Enhanced      = @{
                CorruptionRisk = 'Critical'
                FixStrategy    = 'Manual intervention required'
            }
        }
    }
}

function Find-CSharpMethodBoundaries {
    param($Content, $Lines)

    $methods = @()

    for ($i = 0; $i -lt $Lines.Count; $i++) {
        $line = $Lines[$i].Trim()

        # Match method signatures (simplified pattern)
        if ($line -match '^\s*(public|private|protected|internal)?\s*(static)?\s*(async)?\s*(\w+|\w+<[^>]+>)\s+(\w+)\s*\([^)]*\)\s*$') {
            $methods += @{
                Name       = $Matches[5]
                StartLine  = $i + 1
                EndLine    = $null
                Signature  = $line
                Modifiers  = @($Matches[1], $Matches[2], $Matches[3]) | Where-Object { $_ }
                ReturnType = $Matches[4]
            }
        }
    }

    # Find end lines by tracking braces
    foreach ($method in $methods) {
        $braceCount = 0
        $started = $false

        for ($i = $method.StartLine - 1; $i -lt $Lines.Count; $i++) {
            $line = $Lines[$i]

            $openBraces = ($line.ToCharArray() | Where-Object { $_ -eq '{' }).Count
            $closeBraces = ($line.ToCharArray() | Where-Object { $_ -eq '}' }).Count

            if ($openBraces -gt 0) { $started = $true }
            $braceCount += $openBraces - $closeBraces

            if ($started -and $braceCount -eq 0) {
                $method.EndLine = $i + 1
                break
            }
        }
    }

    return $methods
}

function Analyze-CSharpClassStructure {
    param($Content, $Lines)

    $classes = @()

    for ($i = 0; $i -lt $Lines.Count; $i++) {
        $line = $Lines[$i].Trim()

        # Match class declarations
        if ($line -match '^\s*(public|private|protected|internal)?\s*(static)?\s*(partial)?\s*class\s+(\w+)') {
            $classes += @{
                Name      = $Matches[4]
                Line      = $i + 1
                Modifiers = @($Matches[1], $Matches[2], $Matches[3]) | Where-Object { $_ }
                IsPartial = $Matches[3] -eq 'partial'
            }
        }
    }

    return $classes
}

function Analyze-UsingDirectives {
    param($Content, $Lines)

    $usings = @()
    $issues = @()

    for ($i = 0; $i -lt $Lines.Count; $i++) {
        $line = $Lines[$i].Trim()

        if ($line -match '^using\s+([\w\.]+);?$') {
            $usings += @{
                Namespace    = $Matches[1]
                Line         = $i + 1
                HasSemicolon = $line.EndsWith(';')
            }

            if (-not $line.EndsWith(';')) {
                $issues += @{
                    Type     = 'Missing semicolon'
                    Message  = 'Using directive missing semicolon'
                    Line     = $i + 1
                    Severity = 'Error'
                }
            }
        }
    }

    return @{
        Usings = $usings
        Issues = $issues
    }
}

function Analyze-CSharpBraces {
    param($Content, $Lines)

    $braceStack = @()
    $openBraces = @()
    $closeBraces = @()
    $errors = @()
    $preciseLocations = @()

    for ($lineNum = 0; $lineNum -lt $Lines.Count; $lineNum++) {
        $line = $Lines[$lineNum]
        $charIndex = 0

        foreach ($char in $line.ToCharArray()) {
            $charIndex++

            if ($char -eq '{') {
                $braceInfo = @{
                    Type       = 'Opening'
                    Line       = $lineNum + 1
                    Column     = $charIndex
                    Context    = $line.Trim()
                    StackDepth = $braceStack.Count
                }
                $openBraces += $braceInfo
                $braceStack += $braceInfo

                $preciseLocations += @{
                    Type       = 'OpenBrace'
                    Line       = $lineNum + 1
                    Column     = $charIndex
                    Context    = $line.Trim()
                    StackDepth = $braceStack.Count
                }
            } elseif ($char -eq '}') {
                $braceInfo = @{
                    Type       = 'Closing'
                    Line       = $lineNum + 1
                    Column     = $charIndex
                    Context    = $line.Trim()
                    StackDepth = $braceStack.Count
                }
                $closeBraces += $braceInfo

                if ($braceStack.Count -gt 0) {
                    $matchedOpening = $braceStack[-1]
                    $braceStack = $braceStack[0..($braceStack.Count - 2)]

                    $preciseLocations += @{
                        Type        = 'CloseBrace'
                        Line        = $lineNum + 1
                        Column      = $charIndex
                        Context     = $line.Trim()
                        MatchedLine = $matchedOpening.Line
                        StackDepth  = $braceStack.Count + 1
                    }
                } else {
                    $errors += "Unmatched closing brace at line $($lineNum + 1), column $charIndex"
                    $preciseLocations += @{
                        Type     = 'UnmatchedCloseBrace'
                        Line     = $lineNum + 1
                        Column   = $charIndex
                        Context  = $line.Trim()
                        Severity = 'Error'
                    }
                }
            }
        }
    }

    # Report unmatched opening braces
    foreach ($unmatchedBrace in $braceStack) {
        $errors += "Unmatched opening brace at line $($unmatchedBrace.Line), column $($unmatchedBrace.Column)"
    }

    return @{
        BraceAnalysis    = @{
            OpenBraces       = $openBraces
            CloseBraces      = $closeBraces
            UnmatchedOpening = $braceStack
            UnmatchedClosing = @()
            Balance          = $braceStack.Count
        }
        Errors           = $errors
        PreciseLocations = $preciseLocations
    }
}

function Find-OrphanedCode {
    param($Content, $Lines, $MethodBoundaries)

    $orphanedCode = @()

    for ($i = 0; $i -lt $Lines.Count; $i++) {
        $line = $Lines[$i].Trim()

        # Skip empty lines, comments, using directives, namespace declarations
        if ($line -eq '' -or $line.StartsWith('//') -or $line.StartsWith('/*') -or
            $line.StartsWith('using ') -or $line.StartsWith('namespace ') -or
            $line.StartsWith('[') -or $line -match '^\s*(public|private|protected|internal)?\s*class\s+') {
            continue
        }

        # Check if line contains executable code
        if ($line -match '^\s*\w+.*[;=]' -or $line -match '^\s*(if|for|while|switch|try|catch|return)') {
            # Check if this line is within any method boundary
            $isInMethod = $false
            foreach ($method in $MethodBoundaries) {
                if ($method.EndLine -and $i + 1 -gt $method.StartLine -and $i + 1 -lt $method.EndLine) {
                    $isInMethod = $true
                    break
                }
            }

            if (-not $isInMethod) {
                $orphanedCode += @{
                    Line    = $i + 1
                    Context = $line
                    Type    = 'Orphaned executable code'
                }
            }
        }
    }

    return $orphanedCode
}

function Find-CorruptedMethodSignatures {
    param($Content, $Lines)

    $corruptedMethods = @()

    for ($i = 0; $i -lt $Lines.Count; $i++) {
        $line = $Lines[$i]

        # Look for potential method signatures that are malformed
        if ($line -match '(public|private|protected|internal).*\(.*\)' -and $line -notmatch '^\s*(public|private|protected|internal)?\s*(static)?\s*(async)?\s*(\w+|\w+<[^>]+>)\s+(\w+)\s*\([^)]*\)\s*(\{|$)') {
            # Check for common corruption patterns
            if ($line -match '\(\s*\(' -or $line -match '\)\s*\)' -or $line -match ',\s*,') {
                $corruptedMethods += @{
                    Line    = $i + 1
                    Context = $line.Trim()
                    Issue   = 'Malformed method signature'
                    Type    = 'Syntax corruption'
                }
            }
        }
    }

    return $corruptedMethods
}

#endregion

#region Project Analysis Tools (READ-ONLY) - ENHANCED

function Analyze-ProjectErrors {
    <#
    .SYNOPSIS
    Advanced project-wide error analysis with corruption detection
    .DESCRIPTION
    READ-ONLY: Comprehensive analysis for Claude Sonnet 4 to identify systemic issues
    ENHANCED: Detects corruption patterns, provides fix strategies, prioritizes issues
    #>
    param(
        [string]$WorkspaceFolder = (Get-Location).Path
    )

    $analysis = @{
        TotalErrors    = 0
        TotalWarnings  = 0
        CorruptionRisk = 'Low'
        ProjectHealth  = 'Good'
        Categories     = @{
            Critical    = @()
            Syntax      = @()
            Compilation = @()
            XAML        = @()
            PowerShell  = @()
            Structural  = @()
            Other       = @()
        }
        Files          = @{}
        Summary        = @()
        Enhanced       = @{
            FixPriorities      = @()
            SystemicIssues     = @()
            RecommendedActions = @()
            CorruptionHotspots = @()
        }
    }

    Write-Host '🔍 Scanning project for corruption and errors...' -ForegroundColor Cyan

    # ENHANCED: Scan PowerShell files with advanced detection
    $psFiles = Get-ChildItem -Path $WorkspaceFolder -Filter '*.ps1' -Recurse
    foreach ($file in $psFiles) {
        try {
            $psAnalysis = Analyze-PowerShellSyntax -FilePath $file.FullName
            if (-not $psAnalysis.IsValid) {
                $analysis.Files[$file.FullName] = $psAnalysis
                $analysis.Categories.PowerShell += $psAnalysis.Errors
                $analysis.TotalErrors += $psAnalysis.Errors.Count

                # Check for critical PowerShell issues
                $criticalPatterns = $psAnalysis.Errors | Where-Object {
                    $_.Message -match 'syntax error|unexpected token|missing|unmatched'
                }
                if ($criticalPatterns.Count -gt 0) {
                    $analysis.Categories.Critical += $criticalPatterns
                    $analysis.Enhanced.CorruptionHotspots += @{
                        File       = $file.FullName
                        Type       = 'PowerShell Syntax Corruption'
                        Severity   = 'Critical'
                        IssueCount = $criticalPatterns.Count
                    }
                }
            }
        } catch {
            $analysis.Enhanced.CorruptionHotspots += @{
                File     = $file.FullName
                Type     = 'PowerShell Analysis Failure'
                Severity = 'Critical'
                Error    = $_.Exception.Message
            }
        }
    }

    # ENHANCED: Scan XAML files with advanced detection
    $xamlFiles = Get-ChildItem -Path $WorkspaceFolder -Filter '*.xaml' -Recurse
    foreach ($file in $xamlFiles) {
        try {
            $xamlAnalysis = Analyze-XamlStructure -FilePath $file.FullName
            if (-not $xamlAnalysis.IsValid) {
                $analysis.Files[$file.FullName] = $xamlAnalysis
                $analysis.Categories.XAML += $xamlAnalysis.Errors
                $analysis.TotalErrors += $xamlAnalysis.Errors.Count

                if ($xamlAnalysis.Enhanced.CorruptionRisk -eq 'Critical') {
                    $analysis.Categories.Critical += $xamlAnalysis.Errors
                    $analysis.Enhanced.CorruptionHotspots += @{
                        File        = $file.FullName
                        Type        = 'XAML Structure Corruption'
                        Severity    = 'Critical'
                        FixStrategy = $xamlAnalysis.Enhanced.FixStrategy
                    }
                }
            }
        } catch {
            $analysis.Enhanced.CorruptionHotspots += @{
                File     = $file.FullName
                Type     = 'XAML Analysis Failure'
                Severity = 'Critical'
                Error    = $_.Exception.Message
            }
        }
    }

    # ENHANCED: Scan C# files with advanced detection
    $csFiles = Get-ChildItem -Path $WorkspaceFolder -Filter '*.cs' -Recurse | Where-Object {
        $_.Name -notmatch '_wpftmp|\.g\.cs$|\.designer\.cs$'
    }
    foreach ($file in $csFiles) {
        try {
            $csAnalysis = Analyze-CSharpSyntax -FilePath $file.FullName
            if (-not $csAnalysis.IsValid) {
                $analysis.Files[$file.FullName] = $csAnalysis
                $analysis.Categories.Compilation += $csAnalysis.Errors
                $analysis.TotalErrors += $csAnalysis.Errors.Count

                if ($csAnalysis.Enhanced.CorruptionRisk -eq 'Critical') {
                    $analysis.Categories.Critical += $csAnalysis.Errors
                    $analysis.Enhanced.CorruptionHotspots += @{
                        File                = $file.FullName
                        Type                = 'C# Structure Corruption'
                        Severity            = 'Critical'
                        StructuralIntegrity = $csAnalysis.Enhanced.StructuralIntegrity
                        FixStrategy         = $csAnalysis.Enhanced.FixStrategy
                    }
                }

                # Check for structural issues (orphaned code, corrupted methods)
                if ($csAnalysis.Enhanced.StructuralIntegrity -eq 'Compromised') {
                    $analysis.Categories.Structural += @{
                        File    = $file.FullName
                        Issue   = 'Structural integrity compromised'
                        Details = $csAnalysis.Enhanced
                    }
                }
            }
        } catch {
            $analysis.Enhanced.CorruptionHotspots += @{
                File     = $file.FullName
                Type     = 'C# Analysis Failure'
                Severity = 'Critical'
                Error    = $_.Exception.Message
            }
        }
    }

    # ENHANCED: Analyze systemic patterns
    $analysis.Enhanced.SystemicIssues = Find-SystemicIssues -Analysis $analysis

    # ENHANCED: Generate fix priorities
    $analysis.Enhanced.FixPriorities = Generate-FixPriorities -Analysis $analysis

    # ENHANCED: Set overall project health and corruption risk
    if ($analysis.Categories.Critical.Count -gt 0) {
        $analysis.CorruptionRisk = 'Critical'
        $analysis.ProjectHealth = 'Critical - Immediate attention required'
    } elseif ($analysis.Enhanced.CorruptionHotspots.Count -gt 2) {
        $analysis.CorruptionRisk = 'High'
        $analysis.ProjectHealth = 'Poor - Multiple corruption hotspots detected'
    } elseif ($analysis.TotalErrors -gt 10) {
        $analysis.CorruptionRisk = 'Medium'
        $analysis.ProjectHealth = 'Fair - Many errors detected'
    } else {
        $analysis.ProjectHealth = 'Good'
    }

    # ENHANCED: Generate recommended actions
    $analysis.Enhanced.RecommendedActions = Generate-RecommendedActions -Analysis $analysis

    return $analysis
}

function Find-SystemicIssues {
    param($Analysis)

    $systemicIssues = @()

    # Check for multiple files with similar corruption patterns
    $corruptionTypes = $Analysis.Enhanced.CorruptionHotspots | Group-Object Type
    foreach ($group in $corruptionTypes) {
        if ($group.Count -gt 1) {
            $systemicIssues += @{
                Type          = 'Systemic corruption'
                Pattern       = $group.Name
                AffectedFiles = $group.Count
                Severity      = 'High'
                Description   = 'Multiple files affected by similar corruption pattern'
            }
        }
    }

    # Check for PowerShell syntax issues across multiple files
    $psErrorPatterns = $Analysis.Categories.PowerShell | Where-Object {
        $_.Message -match 'reserved variable|syntax error'
    }
    if ($psErrorPatterns.Count -gt 3) {
        $systemicIssues += @{
            Type          = 'PowerShell pattern issues'
            Pattern       = 'Multiple syntax violations'
            AffectedCount = $psErrorPatterns.Count
            Severity      = 'Medium'
            Description   = 'Consistent PowerShell syntax issues across project'
        }
    }

    return $systemicIssues
}

function Generate-FixPriorities {
    param($Analysis)

    $priorities = @()

    # Priority 1: Critical corruption hotspots
    foreach ($hotspot in $Analysis.Enhanced.CorruptionHotspots) {
        if ($hotspot.Severity -eq 'Critical') {
            $priorities += @{
                Priority = 1
                Type     = 'Critical corruption'
                File     = $hotspot.File
                Action   = $hotspot.FixStrategy ?? 'Manual structure repair'
                Urgency  = 'Immediate'
                Impact   = 'High - Prevents compilation/execution'
            }
        }
    }

    # Priority 2: Structural integrity issues
    foreach ($structural in $Analysis.Categories.Structural) {
        $priorities += @{
            Priority = 2
            Type     = 'Structural integrity'
            File     = $structural.File
            Action   = 'Fix orphaned code and method boundaries'
            Urgency  = 'High'
            Impact   = 'Medium - Affects code organization'
        }
    }

    # Priority 3: Compilation errors
    if ($Analysis.Categories.Compilation.Count -gt 0) {
        $priorities += @{
            Priority = 3
            Type     = 'Compilation errors'
            Count    = $Analysis.Categories.Compilation.Count
            Action   = 'Fix brace mismatches and syntax errors'
            Urgency  = 'Medium'
            Impact   = 'Medium - Prevents building'
        }
    }

    return $priorities | Sort-Object Priority
}

function Generate-RecommendedActions {
    param($Analysis)

    $actions = @()

    if ($Analysis.CorruptionRisk -eq 'Critical') {
        $actions += '🚨 IMMEDIATE: Address critical corruption hotspots before any other work'
        $actions += '🛡️ BACKUP: Create backup of current state before making changes'
        $actions += '🔧 REPAIR: Focus on structural repair of corrupted files'
    }

    if ($Analysis.Enhanced.SystemicIssues.Count -gt 0) {
        $actions += '🔄 SYSTEMATIC: Address systemic issues to prevent future corruption'
    }

    if ($Analysis.Categories.Critical.Count -gt 0) {
        $actions += '⚡ URGENT: Fix critical syntax errors that prevent compilation'
    }

    if ($Analysis.Enhanced.CorruptionHotspots.Count -eq 0 -and $Analysis.TotalErrors -lt 5) {
        $actions += '✅ MAINTENANCE: Project is in good health, continue regular development'
    }

    return $actions
}

function Get-ProjectHealthReport {
    <#
    .SYNOPSIS
    Generates a comprehensive health report for Claude Sonnet 4
    .DESCRIPTION
    READ-ONLY: Provides detailed analysis summary for AI agent decision making
    #>
    param(
        [string]$WorkspaceFolder = (Get-Location).Path
    )

    $analysis = Analyze-ProjectErrors -WorkspaceFolder $WorkspaceFolder

    $report = @{
        Timestamp           = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
        OverallHealth       = $analysis.ProjectHealth
        CorruptionRisk      = $analysis.CorruptionRisk
        CriticalIssues      = $analysis.Categories.Critical.Count
        TotalErrors         = $analysis.TotalErrors
        TotalWarnings       = $analysis.TotalWarnings
        ActionRequired      = $analysis.Enhanced.FixPriorities.Count -gt 0
        ImmediateActions    = $analysis.Enhanced.FixPriorities | Where-Object { $_.Priority -eq 1 }
        SystemicProblems    = $analysis.Enhanced.SystemicIssues
        RecommendedStrategy = $analysis.Enhanced.RecommendedActions
        CorruptionHotspots  = $analysis.Enhanced.CorruptionHotspots
        ReadyForDevelopment = $analysis.CorruptionRisk -eq 'Low' -and $analysis.TotalErrors -lt 5
    }

    return $report
}

#endregion

# Export all read-only analysis functions for Claude Sonnet 4 - PowerShell 7.5 Enhanced
Export-ModuleMember -Function Test-PowerShell75Compatibility, Show-PowerShell75Capabilities, Analyze-XamlStructure, Find-XamlElements, Analyze-PowerShellSyntax, Find-PowerShellPatterns, Analyze-CSharpSyntax, Analyze-ProjectErrors, Get-ProjectHealthReport, Find-UnclosedXmlTags, Analyze-SyncfusionUsage, Analyze-BindingExpressions, Find-CSharpMethodBoundaries, Analyze-CSharpClassStructure, Find-OrphanedCode, Find-CorruptedMethodSignatures
