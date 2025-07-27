#Requires -Version 7.5

<#
.SYNOPSIS
BusBuddy AI Integration Module - Mandatory AI Tools for Development

.DESCRIPTION
Provides mandatory AI integration functions for BusBuddy development using PSAISuite with xAI Grok-4-0709.
All developers MUST use these functions for code generation, debugging, and architecture decisions.

.NOTES
Author: BusBuddy Development Team
Created: July 26, 2025
Version: 1.0.0
Compliance: MANDATORY for all BusBuddy development
#>

# Initialize Serilog Logger for the module
if (-not $script:Logger) {
    try {
        # Try to use existing Serilog configuration from BusBuddy
        $script:Logger = [Serilog.Log]::ForContext('SourceContext', 'BusBuddy.AI')
    }
    catch {
        # Fallback to basic console logger if Serilog not configured
        try {
            Add-Type -AssemblyName "Serilog"
            Add-Type -AssemblyName "Serilog.Sinks.Console"
            $script:Logger = [Serilog.LoggerConfiguration]::new().WriteTo.Console().CreateLogger()
        }
        catch {
            # Ultimate fallback - use custom logging object with proper methods
            $script:Logger = [PSCustomObject]@{}

            # Add script methods to make it behave like a proper logger
            $script:Logger | Add-Member -MemberType ScriptMethod -Name Information -Value {
                param($Message)
                Write-Host "[INFO] [BusBuddy.AI] $Message" -ForegroundColor Cyan
            } -Force

            $script:Logger | Add-Member -MemberType ScriptMethod -Name Warning -Value {
                param($Message)
                Write-Host "[WARN] [BusBuddy.AI] $Message" -ForegroundColor Yellow
            } -Force

            $script:Logger | Add-Member -MemberType ScriptMethod -Name Error -Value {
                param($Message)
                Write-Host "[ERROR] [BusBuddy.AI] $Message" -ForegroundColor Red
            } -Force
        }
    }
}

# Ensure PSAISuite module is available
if (-not (Get-Module -ListAvailable -Name PSAISuite)) {
    $script:Logger.Error("PSAISuite module not found. Install with: Install-Module PSAISuite")
    return
}

Import-Module PSAISuite -Force

# MANDATORY: Ensure XAI key is configured
function Initialize-BusBuddyAI {
    <#
    .SYNOPSIS
    MANDATORY: Initialize BusBuddy AI environment

    .DESCRIPTION
    Sets up the required environment variables and validates AI connectivity.
    MUST be called before any AI operations.
    #>
    [CmdletBinding()]
    param()

    $script:Logger.Information("🤖 Initializing BusBuddy AI Integration...")

    # Set XAI key from environment
    if (-not $env:XAI_API_KEY) {
        $script:Logger.Error("❌ XAI_API_KEY environment variable not found. Please configure your xAI API key.")
        return $false
    }

    $env:XAIKey = $env:XAI_API_KEY

    # Test connectivity
    try {
        $testPrompt = "Test connection - respond with 'OK'"
        $response = Invoke-ChatCompletion -Messages $testPrompt -Model "xai:grok-4-0709" -TextOnly

        if ($response -like "*OK*") {
            $script:Logger.Information("✅ xAI Grok-4-0709 connection successful")
            return $true
        }
        else {
            $script:Logger.Information("✅ xAI Grok-4-0709 connection successful (got response: '$($response.Substring(0, [Math]::Min(50, $response.Length)))...')")
            return $true
        }
    }
    catch {
        $script:Logger.Error("❌ Failed to connect to xAI Grok-4-0709: $($_.Exception.Message)")
        return $false
    }
}

function Invoke-BusBuddyAICodeGeneration {
    <#
    .SYNOPSIS
    MANDATORY: AI-powered code generation for BusBuddy

    .DESCRIPTION
    Uses xAI Grok-4-0709 to generate BusBuddy-specific code with proper context.

    .PARAMETER ComponentType
    Type of component to generate (ViewModel, View, Service, Model, Test)

    .PARAMETER Requirements
    Detailed requirements for the component

    .PARAMETER OutputPath
    Path where generated code should be saved
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [ValidateSet("ViewModel", "View", "Service", "Model", "Test")]
        [string]$ComponentType,

        [Parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]$Requirements,

        [Parameter()]
        [ValidateNotNullOrEmpty()]
        [string]$OutputPath
    )

    # Parameter validation
    if ([string]::IsNullOrWhiteSpace($Requirements)) {
        throw [System.ArgumentException]::new("Requirements parameter cannot be null, empty, or whitespace.", "Requirements")
    }

    if ($OutputPath -and [string]::IsNullOrWhiteSpace($OutputPath)) {
        throw [System.ArgumentException]::new("OutputPath parameter cannot be empty or whitespace when provided.", "OutputPath")
    }

    if (-not (Initialize-BusBuddyAI)) {
        throw "AI initialization failed. Cannot proceed with code generation."
    }

    $prompt = @"
You are a senior C# developer working on BusBuddy, a WPF bus transportation management system.

MANDATORY REQUIREMENTS:
- Follow BusBuddy coding standards and patterns
- Use proper MVVM architecture
- Include comprehensive error handling
- Add XML documentation comments
- Follow .NET 8 best practices
- Use Syncfusion FluentDark theme components where applicable

COMPONENT TYPE: $ComponentType
REQUIREMENTS: $Requirements

Generate complete, production-ready code that integrates seamlessly with the existing BusBuddy codebase.
Include proper namespace declarations, using statements, and follow established patterns.

Ensure the code is:
1. Thread-safe where applicable
2. Properly implements IDisposable if needed
3. Includes proper validation
4. Has comprehensive error handling
5. Follows SOLID principles
"@

    $script:Logger.Information("🤖 Generating $ComponentType with xAI Grok-4-0709...")

    try {
        $generatedCode = Invoke-ChatCompletion -Messages $prompt -Model "xai:grok-4-0709" -TextOnly

        if ($OutputPath) {
            $generatedCode | Out-File -FilePath $OutputPath -Encoding UTF8
            $script:Logger.Information("✅ Generated code saved to: $OutputPath")
        }

        return $generatedCode
    }
    catch {
        $script:Logger.Error("❌ Code generation failed: $($_.Exception.Message)")
        throw
    }
}

function Invoke-BusBuddyAICodeReview {
    <#
    .SYNOPSIS
    MANDATORY: AI-powered code review for BusBuddy

    .DESCRIPTION
    Uses xAI Grok-4-0709 to perform comprehensive code review with BusBuddy-specific standards.

    .PARAMETER FilePath
    Path to the code file to review

    .PARAMETER CodeSnippet
    Code snippet to review (alternative to FilePath)
    #>
    [CmdletBinding()]
    param(
        [Parameter(ParameterSetName = "File")]
        [ValidateNotNullOrEmpty()]
        [string]$FilePath,

        [Parameter(ParameterSetName = "Snippet")]
        [ValidateNotNullOrEmpty()]
        [string]$CodeSnippet
    )

    # Parameter validation
    if ($PSCmdlet.ParameterSetName -eq "File" -and [string]::IsNullOrWhiteSpace($FilePath)) {
        throw [System.ArgumentException]::new("FilePath parameter cannot be null, empty, or whitespace.", "FilePath")
    }

    if ($PSCmdlet.ParameterSetName -eq "Snippet" -and [string]::IsNullOrWhiteSpace($CodeSnippet)) {
        throw [System.ArgumentException]::new("CodeSnippet parameter cannot be null, empty, or whitespace.", "CodeSnippet")
    }

    if (-not (Initialize-BusBuddyAI)) {
        throw "AI initialization failed. Cannot proceed with code review."
    }

    $codeContent = if ($FilePath) {
        if (-not (Test-Path $FilePath)) {
            throw [System.IO.FileNotFoundException]::new("File not found: $FilePath", $FilePath)
        }
        Get-Content -Path $FilePath -Raw
    }
    else {
        $CodeSnippet
    }

    $prompt = @"
You are a senior code reviewer for the BusBuddy project. Review the following code for:

MANDATORY CHECKS:
1. BusBuddy coding standards compliance
2. MVVM pattern adherence
3. Error handling completeness
4. Thread safety considerations
5. Memory management and disposal patterns
6. Security vulnerabilities
7. Performance optimizations
8. Code documentation quality

PROVIDE:
- Overall quality score (1-10)
- Critical issues that MUST be fixed
- Recommended improvements
- Security concerns
- Performance recommendations

CODE TO REVIEW:
$codeContent
"@

    $script:Logger.Information("🤖 Performing AI code review with xAI Grok-4-0709...")

    try {
        $reviewResults = Invoke-ChatCompletion -Messages $prompt -Model "xai:grok-4-0709" -TextOnly
        $script:Logger.Information("✅ Code review completed")
        return $reviewResults
    }
    catch {
        $script:Logger.Error("❌ Code review failed: $($_.Exception.Message)")
        throw
    }
}

function Invoke-BusBuddyAIArchitectureAnalysis {
    <#
    .SYNOPSIS
    MANDATORY: AI-powered architecture analysis for BusBuddy

    .DESCRIPTION
    Uses xAI Grok-4-0709 to analyze and recommend architectural improvements.

    .PARAMETER AnalysisType
    Type of analysis to perform
    #>
    [CmdletBinding()]
    param(
        [Parameter()]
        [ValidateSet("Overall", "Performance", "Security", "Scalability", "Maintainability")]
        [string]$AnalysisType = "Overall"
    )

    if (-not (Initialize-BusBuddyAI)) {
        throw "AI initialization failed. Cannot proceed with architecture analysis."
    }

    $projectRoot = Get-Location
    $solutionFiles = Get-ChildItem -Path $projectRoot -Recurse -Include "*.cs", "*.xaml" |
    Where-Object { $_.FullName -notlike "*\bin\*" -and $_.FullName -notlike "*\obj\*" } |
    Select-Object -First 10  # Limit for prompt size

    $codebaseOverview = $solutionFiles | ForEach-Object {
        "File: $($_.Name)`n$(Get-Content $_.FullName -Raw -ErrorAction SilentlyContinue)"
    } | Join-String -Separator "`n`n---`n`n"

    $prompt = @"
You are a senior software architect analyzing the BusBuddy transportation management system.

ANALYSIS TYPE: $AnalysisType

Based on the codebase overview below, provide:

1. Current architecture assessment
2. Identified patterns and anti-patterns
3. Scalability recommendations
4. Performance optimization opportunities
5. Security considerations
6. Maintainability improvements
7. Technology stack recommendations
8. Migration strategies for improvements

CODEBASE OVERVIEW:
$codebaseOverview
"@

    $script:Logger.Information("🤖 Performing $AnalysisType architecture analysis with xAI Grok-4-0709...")

    try {
        $analysisResults = Invoke-ChatCompletion -Messages $prompt -Model "xai:grok-4-0709" -TextOnly
        $script:Logger.Information("✅ Architecture analysis completed")
        return $analysisResults
    }
    catch {
        $script:Logger.Error("❌ Architecture analysis failed: $($_.Exception.Message)")
        throw
    }
}

# Export functions for use
Export-ModuleMember -Function @(
    'Initialize-BusBuddyAI',
    'Invoke-BusBuddyAICodeGeneration',
    'Invoke-BusBuddyAICodeReview',
    'Invoke-BusBuddyAIArchitectureAnalysis'
)

# Create aliases for easier access
New-Alias -Name "bb-ai-init" -Value "Initialize-BusBuddyAI" -Description "Initialize AI environment"
New-Alias -Name "bb-ai-generate" -Value "Invoke-BusBuddyAICodeGeneration" -Description "AI code generation"
New-Alias -Name "bb-ai-review" -Value "Invoke-BusBuddyAICodeReview" -Description "AI code review"
New-Alias -Name "bb-ai-architect" -Value "Invoke-BusBuddyAIArchitectureAnalysis" -Description "AI architecture analysis"

# Export aliases
Export-ModuleMember -Alias @(
    'bb-ai-init',
    'bb-ai-generate',
    'bb-ai-review',
    'bb-ai-architect'
)

$script:Logger.Information("🤖 BusBuddy AI Integration Module loaded successfully")
$script:Logger.Information("Available commands: bb-ai-init, bb-ai-generate, bb-ai-review, bb-ai-architect")
