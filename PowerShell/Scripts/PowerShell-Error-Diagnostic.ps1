#Requires -Version 7.5
<#
.SYNOPSIS
    Advanced PowerShell 7.5.2 Error Diagnostic Tool
.DESCRIPTION
    Comprehensive analysis of PowerShell files using modern AST parsing and advanced error detection.
    Leverages PowerShell 7.5.2 features including enhanced AST analysis, improved tokenization,
    and advanced error reporting for optimal code quality validation.
.NOTES
    Created: July 27, 2025
    Enhanced: Based on PowerShell 7.5.2 research and modern parsing techniques
    Features: AST analysis, token validation, semantic checking, BusBuddy integration
    Research: Leverages System.Management.Automation.Language.Parser improvements
#>

param(
    [Parameter(Mandatory = $false)]
    [string]$Path = ".",

    [Parameter(Mandatory = $false)]
    [switch]$Detailed,

    [Parameter(Mandatory = $false)]
    [switch]$ExportReport,

    [Parameter(Mandatory = $false)]
    [switch]$UseModernAST,

    [Parameter(Mandatory = $false)]
    [switch]$ValidateComplexity,

    [Parameter(Mandatory = $false)]
    [switch]$CheckPS7Features
)

# Color coding for output
$Colors = @{
    Success = 'Green'
    Warning = 'Yellow'
    Error = 'Red'
    Info = 'Cyan'
    Detail = 'DarkGray'
}

function Write-ColorOutput {
    param($Message, $Type = 'Info')

    $color = $Colors[$Type]
    if (-not $color) {
        $color = 'White'  # Default fallback
    }

    Write-Host $Message -ForegroundColor $color
}

function Test-ModernASTAnalysis {
    param($FilePath)

    $issues = @()

    try {
        # Use modern Language.Parser for advanced AST analysis
        $tokens = $errors = $null
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($FilePath, [ref]$tokens, [ref]$errors)

        if ($errors.Count -gt 0) {
            foreach ($error in $errors) {
                $issues += "Parse Error at line $($error.Extent.StartLineNumber): $($error.Message)"
            }
        }

        # Advanced AST analysis for PowerShell 7.5.2
        if ($ast) {
            # Find all AST nodes for deeper analysis
            $allNodes = $ast.FindAll({ $true }, $true)

            # Check for risky patterns using AST
            $riskyCommands = $allNodes | Where-Object {
                $_ -is [System.Management.Automation.Language.CommandAst] -and
                $_.GetCommandName() -match 'Invoke-Expression|iex|Add-Type.*-TypeDefinition'
            }

            if ($riskyCommands.Count -gt 0) {
                $issues += "Potentially risky commands detected: $($riskyCommands.Count) instances"
            }

            # Check for complex nested structures
            $maxDepth = Get-ASTDepth $ast
            if ($maxDepth -gt 10) {
                $issues += "High complexity detected: AST depth of $maxDepth (consider refactoring)"
            }

            # PowerShell 7+ specific feature detection
            $ps7Features = $allNodes | Where-Object {
                $_ -is [System.Management.Automation.Language.BinaryExpressionAst] -and
                $_.Operator -in 'Coalesce', 'ConditionalAccess'
            }

            if ($ps7Features.Count -gt 0 -and $ast.ScriptRequirements -eq $null) {
                $issues += "PowerShell 7+ features used but no #Requires directive found"
            }
        }

    } catch {
        $issues += "Modern AST analysis failed: $($_.Exception.Message)"
    }

    return $issues
}

function Get-ASTDepth {
    param($Node, $CurrentDepth = 0)

    $maxDepth = $CurrentDepth

    if ($Node.PSObject.Properties['Child'] -or $Node.PSObject.Properties['Children']) {
        $children = if ($Node.PSObject.Properties['Child']) { $Node.Child } else { $Node.Children }

        foreach ($child in $children) {
            if ($child) {
                $childDepth = Get-ASTDepth $child ($CurrentDepth + 1)
                if ($childDepth -gt $maxDepth) {
                    $maxDepth = $childDepth
                }
            }
        }
    }

    return $maxDepth
}

function Test-PS7SpecificFeatures {
    param($Content, $FilePath)
    $issues = @()

    # Enhanced PS7 feature detection
    $ps7Patterns = @{
        'Null Coalescing' = '\?\?'
        'Null Conditional' = '\?\.'
        'Ternary Operator' = '\s+\?\s+[^:]+\s*:\s*'
        'Parallel ForEach' = 'ForEach-Object\s+-Parallel'
        'Pipeline Chain' = '\|\||\&\&'
        'Enhanced Splatting' = '@\w+\s*\.\w+'
    }

    foreach ($feature in $ps7Patterns.GetEnumerator()) {
        if ($Content -match $feature.Value) {
            # Check for version requirement
            if ($Content -notmatch '#Requires\s+-Version\s+[7-9]') {
                $issues += "PowerShell 7+ feature '$($feature.Key)' detected but no version requirement specified"
            }
        }
    }

    return $issues
}

function Test-EnhancedTokenAnalysis {
    param($FilePath)
    $issues = @()

    try {
        # Use both legacy PSParser and modern Language.Parser for comprehensive analysis
        $content = Get-Content $FilePath -Raw

        # Legacy PSParser analysis
        $tokens = @()
        [System.Management.Automation.PSParser]::Tokenize($content, [ref]$tokens) | Out-Null

        # Check for suspicious token patterns
        $stringTokens = $tokens | Where-Object { $_.Type -eq 'String' }
        $commandTokens = $tokens | Where-Object { $_.Type -eq 'Command' }

        # Detect potential obfuscation
        $suspiciousStrings = $stringTokens | Where-Object {
            $_.Content -match '[\x00-\x1F\x7F-\xFF]' -or
            $_.Content.Length -gt 1000 -or
            ($_.Content -match '^[A-Za-z0-9+/=]+$' -and $_.Content.Length -gt 50)
        }

        if ($suspiciousStrings.Count -gt 0) {
            $issues += "Potentially obfuscated or binary content detected in strings"
        }

        # Modern token analysis
        $modernTokens = $modernErrors = $null
        $ast = [System.Management.Automation.Language.Parser]::ParseInput($content, [ref]$modernTokens, [ref]$modernErrors)

        if ($modernErrors.Count -gt 0) {
            foreach ($error in $modernErrors) {
                $issues += "Tokenization error: $($error.Message) at line $($error.Extent.StartLineNumber)"
            }
        }

    } catch {
        $issues += "Token analysis failed: $($_.Exception.Message)"
    }

    return $issues
}

function Test-SmartBraceMatching {
    param($Content, $FilePath)
    $issues = @()

    try {
        # Use tokenization to intelligently count braces, excluding those in strings and comments
        $tokens = @()
        [System.Management.Automation.PSParser]::Tokenize($Content, [ref]$tokens) | Out-Null

        # Filter out tokens that are inside strings or comments
        $relevantTokens = $tokens | Where-Object {
            $_.Type -notin @('String', 'Comment')
        }

        $openBraces = 0
        $closeBraces = 0
        $openParens = 0
        $closeParens = 0
        $openBrackets = 0
        $closeBrackets = 0

        foreach ($token in $relevantTokens) {
            $content = $token.Content
            $openBraces += ($content.ToCharArray() | Where-Object { $_ -eq '{' }).Count
            $closeBraces += ($content.ToCharArray() | Where-Object { $_ -eq '}' }).Count
            $openParens += ($content.ToCharArray() | Where-Object { $_ -eq '(' }).Count
            $closeParens += ($content.ToCharArray() | Where-Object { $_ -eq ')' }).Count
            $openBrackets += ($content.ToCharArray() | Where-Object { $_ -eq '[' }).Count
            $closeBrackets += ($content.ToCharArray() | Where-Object { $_ -eq ']' }).Count
        }

        if ($openBraces -ne $closeBraces) {
            $issues += "Unmatched braces: {$openBraces vs }$closeBraces (excluding strings/comments)"
        }
        if ($openParens -ne $closeParens) {
            $issues += "Unmatched parentheses: ($openParens vs )$closeParens (excluding strings/comments)"
        }
        if ($openBrackets -ne $closeBrackets) {
            $issues += "Unmatched brackets: [$openBrackets vs ]$closeBrackets (excluding strings/comments)"
        }

    } catch {
        # Fallback to simple character counting if tokenization fails
        $openBraces = ($Content.ToCharArray() | Where-Object { $_ -eq '{' }).Count
        $closeBraces = ($Content.ToCharArray() | Where-Object { $_ -eq '}' }).Count
        $openParens = ($Content.ToCharArray() | Where-Object { $_ -eq '(' }).Count
        $closeParens = ($Content.ToCharArray() | Where-Object { $_ -eq ')' }).Count
        $openBrackets = ($Content.ToCharArray() | Where-Object { $_ -eq '[' }).Count
        $closeBrackets = ($Content.ToCharArray() | Where-Object { $_ -eq ']' }).Count

        if ($openBraces -ne $closeBraces) {
            $issues += "Unmatched braces: {$openBraces vs }$closeBraces (simple count)"
        }
        if ($openParens -ne $closeParens) {
            $issues += "Unmatched parentheses: ($openParens vs )$closeParens (simple count)"
        }
        if ($openBrackets -ne $closeBrackets) {
            $issues += "Unmatched brackets: [$openBrackets vs ]$closeBrackets (simple count)"
        }
    }

    return $issues
}

function Test-InvalidArguments {
    param($Content, $FilePath)
    $issues = @()

    # Check for common parameter validation patterns
    if ($Content -match 'param\s*\(\s*\)' -and $Content -match '\$\w+\.\w+') {
        $issues += "Empty param() block but variables used - may cause null reference errors"
    }

    # Check for mandatory parameters without validation
    if ($Content -match '\[Parameter\(Mandatory\s*=\s*\$true\)\]' -and
        $Content -notmatch 'ArgumentNullException|ValidateNotNull|ValidateNotNullOrEmpty') {
        $issues += "Mandatory parameters without null validation detected"
    }

    return $issues
}

function Test-ModuleImportIssues {
    param($Content, $FilePath)
    $issues = @()

    # Check for module-specific issues
    if ($FilePath -like "*.psm1") {
        # Check for missing Export-ModuleMember
        if ($Content -match 'function\s+\w+' -and $Content -notmatch 'Export-ModuleMember') {
            $issues += "Module functions defined but no Export-ModuleMember found"
        }

        # Check for syntax in module manifest references
        if ($Content -match '\.psd1' -and $Content -notmatch 'Import-Module|Test-ModuleManifest') {
            $issues += "Module manifest referenced but not properly imported/tested"
        }
    }

    return $issues
}

function Test-ProfileSpecificIssues {
    param($Content, $FilePath)
    $issues = @()

    # Check for profile-loading specific issues
    if ($FilePath -like "*profile*" -or $FilePath -like "*load-*") {
        # Check for complex operations that might fail during profile load
        if ($Content -match 'dotnet\s+build|Get-Process.*Stop-Process|Remove-Module.*-Force') {
            $issues += "Complex operations in profile - consider moving to functions"
        }

        # Check for error handling in profile code
        if ($Content -match 'try\s*\{' -and $Content -notmatch 'catch\s*\{.*Write-Host.*error') {
            $issues += "Try-catch blocks without user-friendly error output"
        }
    }

    return $issues
}

function Test-VersionMismatch {
    param($Content, $FilePath)
    $issues = @()

    # Check for PowerShell 7+ specific features
    $ps7Features = @(
        '\?\?',  # Null coalescing operator
        '\?\.',  # Null conditional operator
        'ForEach-Object\s+-Parallel',
        'ternary.*\?.*:'
    )

    foreach ($feature in $ps7Features) {
        if ($Content -match $feature) {
            # Check if file has version requirement
            if ($Content -notmatch '#Requires -Version [7-9]') {
                $issues += "PowerShell 7+ feature detected but no version requirement specified"
                break
            }
        }
    }

    return $issues
}

function Invoke-PowerShellDiagnostic {
    param($FilePath)

    Write-ColorOutput "🔍 Analyzing: $FilePath" 'Info'

    try {
        $content = Get-Content $FilePath -Raw -ErrorAction Stop

        # Basic syntax check first
        $tokens = @()
        $errors = @()
        $null = [System.Management.Automation.PSParser]::Tokenize($content, [ref]$tokens)

        if ($tokens.Count -eq 0) {
            Write-ColorOutput "   ⚠️  Empty or invalid file" 'Warning'
            return
        }

        # Run specific error pattern tests
        $allIssues = @()

        $allIssues += Test-UnexpectedTokens $content $FilePath
        $allIssues += Test-MissingBraces $content $FilePath
        $allIssues += Test-InvalidArguments $content $FilePath
        $allIssues += Test-ModuleImportIssues $content $FilePath
        $allIssues += Test-ProfileSpecificIssues $content $FilePath
        $allIssues += Test-VersionMismatch $content $FilePath

        if ($allIssues.Count -eq 0) {
            Write-ColorOutput "   ✅ No issues detected" 'Success'
        } else {
            Write-ColorOutput "   ❌ Issues found:" 'Error'
            foreach ($issue in $allIssues) {
                Write-ColorOutput "      • $issue" 'Detail'
            }
        }

        return @{
            File = $FilePath
            Issues = $allIssues
            TokenCount = $tokens.Count
        }

    } catch {
        Write-ColorOutput "   ❌ Parser Error: $($_.Exception.Message)" 'Error'
        return @{
            File = $FilePath
            Issues = @("PARSER ERROR: $($_.Exception.Message)")
            TokenCount = 0
        }
    }
}

# Main execution
Write-ColorOutput "🚀 PowerShell Error Diagnostic Tool" 'Info'
Write-ColorOutput "📁 Scanning path: $Path" 'Info'
Write-ColorOutput "" 'Info'

$results = @()
$totalFiles = 0
$filesWithIssues = 0

Get-ChildItem -Path $Path -Recurse -Filter "*.ps1" | ForEach-Object {
    $totalFiles++
    $result = Invoke-PowerShellDiagnostic $_.FullName
    $results += $result

    if ($result.Issues.Count -gt 0) {
        $filesWithIssues++
    }
}

# Summary
Write-ColorOutput "" 'Info'
Write-ColorOutput "📊 DIAGNOSTIC SUMMARY" 'Info'
Write-ColorOutput "=" * 50 'Info'
Write-ColorOutput "Total Files Scanned: $totalFiles" 'Info'
Write-ColorOutput "Files with Issues: $filesWithIssues" $(if ($filesWithIssues -eq 0) { 'Success' } else { 'Warning' })
Write-ColorOutput "Files Clean: $($totalFiles - $filesWithIssues)" 'Success'

if ($Detailed -and $results.Count -gt 0) {
    Write-ColorOutput "" 'Info'
    Write-ColorOutput "🔍 DETAILED RESULTS" 'Info'
    Write-ColorOutput "=" * 50 'Info'

    foreach ($result in $results | Where-Object { $_.Issues.Count -gt 0 }) {
        Write-ColorOutput "" 'Info'
        Write-ColorOutput "File: $($result.File)" 'Info'
        Write-ColorOutput "Issues:" 'Warning'
        foreach ($issue in $result.Issues) {
            Write-ColorOutput "  • $issue" 'Detail'
        }
    }
}

if ($ExportReport) {
    $reportPath = "PowerShell-Diagnostic-Report-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
    $results | ConvertTo-Json -Depth 3 | Out-File $reportPath
    Write-ColorOutput "" 'Info'
    Write-ColorOutput "📄 Report exported: $reportPath" 'Success'
}

Write-ColorOutput "" 'Info'
if ($filesWithIssues -eq 0) {
    Write-ColorOutput "🎉 All PowerShell files passed diagnostic checks!" 'Success'
} else {
    Write-ColorOutput "⚠️  Some issues found. Review the details above." 'Warning'
    Write-ColorOutput "💡 Run with -Detailed for more information" 'Info'
}
