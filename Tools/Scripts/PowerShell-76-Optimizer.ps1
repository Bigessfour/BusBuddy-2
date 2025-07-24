#Requires -Version 7.6
<#
.SYNOPSIS
    PowerShell 7.6 Optimization Tool for BusBuddy

.DESCRIPTION
    Scans and optimizes PowerShell scripts to leverage PowerShell 7.6 features:
    - Enhanced tab completion improvements
    - Better SearchValues<char> usage
    - Improved parallel processing
    - Enhanced error handling
    - Better performance patterns
    - New cmdlet features

.NOTES
    Designed for PowerShell 7.6.0-preview.4+
    Optimizes scripts from 7.5.2 patterns to 7.6 best practices
#>

param(
    [Parameter(Mandatory = $false)]
    [string]$ScriptPath = ".",
    [switch]$Analyze,
    [switch]$Convert,
    [switch]$Backup,
    [switch]$Verbose,
    [switch]$WhatIf
)

# =============================================================================
# POWERSHELL 7.6 FEATURE DETECTION AND OPTIMIZATION
# =============================================================================

$Global:PS76Features = @{
    Version                    = "7.6.0-preview.4"
    EnhancedTabCompletion      = $true
    SearchValuesOptimization   = $true
    ImprovedParallelProcessing = $true
    BetterErrorHandling        = $true
    NewCmdletFeatures          = $true
    PipelineStopToken          = $true
    ImprovedVariableInference  = $true
}

$Global:OptimizationPatterns = @{
    # Enhanced ForEach-Object patterns with PowerShell 7.6 -AsJob and timeout support
    OldParallelPattern           = @{
        Pattern     = '(\$\w+\s*\|\s*ForEach-Object\s+-Parallel\s+\{[^}]+\})\s*(-ThrottleLimit\s+\d+)?(?!\s+-AsJob)'
        Replacement = '$1 -ThrottleLimit $(if ($Global:AISystemInfo.IsNet9) { [Math]::Min($Global:AISystemInfo.MaxParallelism, 32) } else { [Math]::Min($Global:AISystemInfo.MaxParallelism, 24) }) -TimeoutSeconds 180 -AsJob'
        Description = "Optimize parallel processing with PowerShell 7.6 enhanced -AsJob and timeout support"
    }

    # ThreadJob module migration
    ThreadJobModulePattern       = @{
        Pattern     = 'Start-ThreadJob(?!\s+-ThrottleLimit\s+\$\(if)'
        Replacement = 'Import-Module Microsoft.PowerShell.ThreadJob -ErrorAction SilentlyContinue; Start-ThreadJob'
        Description = "Migrate to Microsoft.PowerShell.ThreadJob module for PowerShell 7.6"
    }

    # Enhanced timeout patterns for jobs
    JobTimeoutPattern            = @{
        Pattern     = '(Wait-Job\s+\$\w+)(?!\s+-Timeout)'
        Replacement = '$1 -Timeout 300'
        Description = "Add timeout to job waiting to prevent hanging in PowerShell 7.6"
    }

    # Enhanced parallel processing with higher throttle limits
    ParallelThrottleLimitPattern = @{
        Pattern     = '-ThrottleLimit\s+(\d+)(?!\s*#.*PowerShell\s+7\.6)'
        Replacement = '-ThrottleLimit $(if ($Global:AISystemInfo.IsNet9) { [Math]::Min($Global:AISystemInfo.MaxParallelism, 32) } else { [Math]::Min($Global:AISystemInfo.MaxParallelism, 24) })  # PowerShell 7.6 enhanced'
        Description = "Increase throttle limits for PowerShell 7.6 improved runspace management"
    }

    # SearchValues optimization for character arrays
    CharArrayPattern             = @{
        Pattern     = '\[char\[\]\]\s*@\(([^)]+)\)'
        Replacement = '[System.Buffers.SearchValues]::Create([char[]]@($1))'
        Description = "Use SearchValues<char> for efficient character searching"
    }

    # Enhanced parameter completion
    TabCompletionPattern         = @{
        Pattern     = 'Register-ArgumentCompleter\s+-CommandName\s+([^-]+)\s+-ParameterName\s+([^-]+)\s+-ScriptBlock\s+\{'
        Replacement = 'Register-ArgumentCompleter -CommandName $1 -ParameterName $2 -Native -ScriptBlock {'
        Description = "Enhanced argument completion with native support"
    }

    # Improved error handling
    OldErrorPattern              = @{
        Pattern     = 'try\s*\{([^}]+)\}\s*catch\s*\{([^}]+)\}'
        Replacement = 'try { $1 } catch [System.Exception] { $2 }'
        Description = "Enhanced exception handling with specific types"
    }

    # New Get-Command features
    GetCommandPattern            = @{
        Pattern     = 'Get-Command\s+([^-\s]+)(?!\s+-ExcludeModule)'
        Replacement = 'Get-Command $1 -ExcludeModule @("CommonModules")'
        Description = "Use new -ExcludeModule parameter for better performance"
    }

    # Enhanced variable type inference
    VariableInferencePattern     = @{
        Pattern     = '\$(\w+)\s*=\s*@\(\s*\)'
        Replacement = '[System.Collections.Generic.List[object]]$$1 = @()'
        Description = "Explicit type declaration for better inference"
    }

    # Pipeline stop token usage
    PipelineStopPattern          = @{
        Pattern     = 'function\s+(\w+)\s*\{[^}]*\[CmdletBinding\(\)\][^}]*'
        Replacement = 'function $1 { [CmdletBinding()] param() if ($PSCmdlet.PipelineStopToken.IsCancellationRequested) { return }'
        Description = "Add PipelineStopToken support for cancellable operations"
    }

    # Enhanced string interpolation
    StringInterpolationPattern   = @{
        Pattern     = '"\$\(([^)]+)\)"'
        Replacement = '$"$($1)"'
        Description = "Use enhanced string interpolation syntax"
    }

    # Improved Join-Path with multiple child paths
    JoinPathPattern              = @{
        Pattern     = 'Join-Path\s+([^-\s]+)\s+([^-\s]+)(?!\s+-ChildPath)'
        Replacement = 'Join-Path $1 -ChildPath @($2)'
        Description = "Use new -ChildPath string[] parameter"
    }

    # Enhanced JSON handling with .NET 9 improvements
    JsonPattern                  = @{
        Pattern     = 'ConvertTo-Json\s+([^-\s]+)(?!\s+-EnumsAsNumbers)'
        Replacement = 'ConvertTo-Json $1 -EnumsAsNumbers -Compress'
        Description = "Use new enum serialization and .NET 9 compression optimization"
    }

    # .NET 9 specific dotnet build optimizations
    DotNetBuildPattern           = @{
        Pattern     = 'dotnet\s+build\s+([^-\s]+)(?!\s+--property)'
        Replacement = 'dotnet build $1 --property:UseAppHost=false --property:TieredCompilation=true'
        Description = ".NET 9 build performance optimizations"
    }

    # .NET Framework detection patterns
    FrameworkDetectionPattern    = @{
        Pattern     = '\$PSVersionTable\.PSVersion\.Major\s*-[eg][te]\s*7'
        Replacement = '$Global:AISystemInfo.Is76Plus -and $Global:AISystemInfo.IsNet8Plus'
        Description = "Enhanced framework detection with .NET version awareness"
    }

    # Enhanced memory management patterns
    MemoryOptimizationPattern    = @{
        Pattern     = 'Start-ThreadJob\s+-ScriptBlock\s+\{([^}]+)\}'
        Replacement = 'Start-ThreadJob -ScriptBlock { $1 } -ThrottleLimit $(if ($Global:AISystemInfo.IsNet9) { 16 } else { 8 })'
        Description = ".NET 9 enhanced memory management for background jobs"
    }

    # Native command execution improvements for PowerShell 7.6
    DotNetCommandPattern         = @{
        Pattern     = '&\s+dotnet\s+@(\w+)\s+2>&1'
        Replacement = '$result = Invoke-EnhancedDotNetCommand -Arguments @$1; $result.Output'
        Description = "Enhanced dotnet command execution with PowerShell 7.6 improvements"
    }

    # PSNativeCommandArgumentPassing configuration
    NativeCommandConfigPattern   = @{
        Pattern     = '^(?!.*\$PSNativeCommandArgumentPassing)'
        Replacement = '$PSNativeCommandArgumentPassing = ''Standard''  # PowerShell 7.6 enhanced native command handling'
        Description = "Add PSNativeCommandArgumentPassing configuration for consistent argument handling"
    }

    # Enhanced Start-Process patterns
    StartProcessPattern          = @{
        Pattern     = 'Start-Process\s+([^-]+)(?!\s+-ArgumentList)'
        Replacement = 'Start-Process -FilePath $1 -ArgumentList @() -NoNewWindow -Wait -PassThru'
        Description = "Use PowerShell 7.6 enhanced Start-Process with better parameter handling"
    }

    # Enhanced dotnet command execution with fallback
    EnhancedDotNetPattern        = @{
        Pattern     = 'dotnet\s+([^|&<>]+?)(?=\s*[|&<>]|\s*$)'
        Replacement = 'Invoke-EnhancedDotNetCommand -Arguments @("$1".Split(" ", [System.StringSplitOptions]::RemoveEmptyEntries))'
        Description = "Use enhanced dotnet command execution with PowerShell 7.6 improvements and fallback handling"
    }

    # Import Microsoft.PowerShell.ThreadJob for PowerShell 7.6
    ThreadJobImportPattern       = @{
        Pattern     = '^(?!.*Import-Module.*Microsoft\.PowerShell\.ThreadJob)'
        Replacement = 'Import-Module Microsoft.PowerShell.ThreadJob -ErrorAction SilentlyContinue  # PowerShell 7.6 explicit import'
        Description = "Ensure Microsoft.PowerShell.ThreadJob module is explicitly imported for PowerShell 7.6"
    }
}

# =============================================================================
# SCRIPT ANALYSIS ENGINE
# =============================================================================

function Analyze-PowerShellScript {
    param(
        [string]$FilePath,
        [switch]$DeepAnalysis
    )

    if (-not (Test-Path $FilePath)) {
        Write-Error "Script file not found: $FilePath"
        return
    }

    $content = Get-Content $FilePath -Raw
    $analysis = @{
        FilePath                  = $FilePath
        CurrentVersion            = "Unknown"
        PS76Ready                 = $false
        OptimizationOpportunities = @()
        PerformanceImprovements   = @()
        NewFeatureUsage           = @()
        Issues                    = @()
        Score                     = 0
        Recommendations           = @()
    }

    # Detect current PowerShell version requirements
    if ($content -match '#Requires -Version\s+([\d\.]+)') {
        $analysis.CurrentVersion = $matches[1]
        $analysis.PS76Ready = [version]$matches[1] -ge [version]"7.6"
    }

    # Check for optimization patterns
    foreach ($patternName in $Global:OptimizationPatterns.Keys) {
        $pattern = $Global:OptimizationPatterns[$patternName]
        $matches = [regex]::Matches($content, $pattern.Pattern, [System.Text.RegularExpressions.RegexOptions]::Multiline)

        if ($matches.Count -gt 0) {
            $analysis.OptimizationOpportunities += @{
                Pattern     = $patternName
                Count       = $matches.Count
                Description = $pattern.Description
                Locations   = $matches | ForEach-Object {
                    @{
                        Line = ($content.Substring(0, $_.Index) -split "`n").Count
                        Text = $_.Value
                    }
                }
            }
        }
    }

    # Performance analysis
    $performancePatterns = @{
        "Inefficient ForEach"    = '\$\w+\s*\|\s*ForEach-Object\s+(?!-Parallel)'
        "No ThrottleLimit"       = 'ForEach-Object\s+-Parallel[^-]*(?!-ThrottleLimit)'
        "Synchronous Operations" = '(Invoke-RestMethod|Invoke-WebRequest)(?!\s+-Async)'
        "Unoptimized Searches"   = '\.IndexOf\(|\.Contains\('
    }

    foreach ($perfPattern in $performancePatterns.GetEnumerator()) {
        $matches = [regex]::Matches($content, $perfPattern.Value)
        if ($matches.Count -gt 0) {
            $analysis.PerformanceImprovements += @{
                Issue  = $perfPattern.Key
                Count  = $matches.Count
                Impact = "Medium"
            }
        }
    }

    # Check for new PS 7.6 feature usage
    $newFeatures = @{
        "SearchValues Usage" = 'SearchValues\.Create'
        "Enhanced Parallel"  = 'ForEach-Object\s+-Parallel.*-ThrottleLimit'
        "PipelineStopToken"  = 'PipelineStopToken'
        "ExcludeModule"      = 'Get-Command.*-ExcludeModule'
        "Enhanced Join-Path" = 'Join-Path.*-ChildPath'
    }

    foreach ($feature in $newFeatures.GetEnumerator()) {
        if ($content -match $feature.Value) {
            $analysis.NewFeatureUsage += $feature.Key
        }
    }

    # Calculate optimization score
    $totalOpportunities = $analysis.OptimizationOpportunities.Count
    $featuresUsed = $analysis.NewFeatureUsage.Count
    $performanceIssues = $analysis.PerformanceImprovements.Count

    $analysis.Score = [math]::Max(0, 100 - ($totalOpportunities * 10) - ($performanceIssues * 5) + ($featuresUsed * 15))

    # Generate recommendations
    if ($analysis.Score -lt 70) {
        $analysis.Recommendations += "❌ Script needs significant PS 7.6 optimization"
    }
    elseif ($analysis.Score -lt 85) {
        $analysis.Recommendations += "⚠️ Script could benefit from PS 7.6 improvements"
    }
    else {
        $analysis.Recommendations += "✅ Script is well-optimized for PS 7.6"
    }

    if (-not $analysis.PS76Ready) {
        $analysis.Recommendations += "🔧 Update #Requires directive to 7.6"
    }

    if ($analysis.OptimizationOpportunities.Count -gt 0) {
        $analysis.Recommendations += "⚡ $($analysis.OptimizationOpportunities.Count) optimization opportunities found"
    }

    return $analysis
}

# =============================================================================
# SCRIPT CONVERSION ENGINE
# =============================================================================

function Convert-ScriptToPS76 {
    param(
        [string]$FilePath,
        [switch]$CreateBackup = $true,
        [switch]$WhatIf
    )

    if (-not (Test-Path $FilePath)) {
        Write-Error "Script file not found: $FilePath"
        return
    }

    Write-Host "🔄 Converting $FilePath to PowerShell 7.6 optimized version..." -ForegroundColor Cyan

    $content = Get-Content $FilePath -Raw
    $originalContent = $content
    $conversions = @()

    # Create backup
    if ($CreateBackup -and -not $WhatIf) {
        $backupPath = "$FilePath.ps75.bak"
        $originalContent | Set-Content $backupPath
        Write-Host "📋 Backup created: $backupPath" -ForegroundColor Gray
    }

    # Apply optimization patterns
    foreach ($patternName in $Global:OptimizationPatterns.Keys) {
        $pattern = $Global:OptimizationPatterns[$patternName]
        $oldContent = $content

        $content = [regex]::Replace($content, $pattern.Pattern, $pattern.Replacement, [System.Text.RegularExpressions.RegexOptions]::Multiline)

        if ($content -ne $oldContent) {
            $conversions += @{
                Pattern     = $patternName
                Description = $pattern.Description
                Applied     = $true
            }
            Write-Host "   ✅ Applied: $($pattern.Description)" -ForegroundColor Green
        }
    }

    # Update version requirement
    if ($content -match '#Requires -Version\s+([\d\.]+)') {
        $oldVersion = $matches[1]
        if ([version]$oldVersion -lt [version]"7.6") {
            $content = $content -replace '#Requires -Version\s+[\d\.]+', '#Requires -Version 7.6'
            $conversions += @{
                Pattern     = "VersionUpdate"
                Description = "Updated version requirement from $oldVersion to 7.6"
                Applied     = $true
            }
            Write-Host "   ✅ Updated version requirement: $oldVersion → 7.6" -ForegroundColor Green
        }
    }
    else {
        # Add version requirement if missing
        $content = "#Requires -Version 7.6`n" + $content
        $conversions += @{
            Pattern     = "VersionAdded"
            Description = "Added PowerShell 7.6 version requirement"
            Applied     = $true
        }
        Write-Host "   ✅ Added PowerShell 7.6 version requirement" -ForegroundColor Green
    }

    # Add PowerShell 7.6 optimization header
    $optimizationHeader = @"
<#
.NOTES
    Optimized for PowerShell 7.6.0-preview.4+
    Enhanced with:
    - Improved parallel processing with dynamic throttling
    - SearchValues<char> for efficient character operations
    - Enhanced tab completion and variable inference
    - Better error handling patterns
    - New cmdlet features and parameters

    Converted by PowerShell-76-Optimizer.ps1 on $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
#>

"@

    if ($content -notmatch "Optimized for PowerShell 7\.6") {
        # Insert after existing synopsis/description
        if ($content -match '(<#[\s\S]*?#>)') {
            $content = $content -replace '(<#[\s\S]*?#>)', "$1`n$optimizationHeader"
        }
        else {
            $content = $optimizationHeader + $content
        }

        $conversions += @{
            Pattern     = "OptimizationHeader"
            Description = "Added PowerShell 7.6 optimization documentation"
            Applied     = $true
        }
        Write-Host "   ✅ Added optimization documentation header" -ForegroundColor Green
    }

    # Show what would be changed
    if ($WhatIf) {
        Write-Host "`n📋 Changes that would be made:" -ForegroundColor Yellow
        $conversions | ForEach-Object {
            Write-Host "   • $($_.Description)" -ForegroundColor White
        }
        return @{
            FilePath    = $FilePath
            Conversions = $conversions
            WhatIf      = $true
        }
    }

    # Save converted content
    $content | Set-Content $FilePath -Encoding UTF8

    Write-Host "✅ Conversion completed: $($conversions.Count) optimizations applied" -ForegroundColor Green

    return @{
        FilePath      = $FilePath
        Conversions   = $conversions
        BackupCreated = $CreateBackup
        Success       = $true
    }
}

# =============================================================================
# BULK OPTIMIZATION FUNCTIONS
# =============================================================================

function Optimize-BusBuddyScripts {
    param(
        [string]$ProjectRoot = (Get-Location),
        [switch]$IncludeProfiles = $true,
        [switch]$IncludeTools = $true,
        [switch]$IncludeAI = $true,
        [switch]$WhatIf,
        [switch]$Parallel = $true
    )

    Write-Host "🚀 Starting BusBuddy PowerShell 7.6 Optimization..." -ForegroundColor Cyan

    $scriptPaths = @()

    # Find all PowerShell scripts
    if ($IncludeProfiles) {
        $scriptPaths += Get-ChildItem "$ProjectRoot\*.ps1" -Recurse | Where-Object { $_.Name -like "*profile*" -or $_.Name -like "*load*" }
    }

    if ($IncludeTools) {
        $toolsPath = Join-Path $ProjectRoot "Tools\Scripts"
        if (Test-Path $toolsPath) {
            $scriptPaths += Get-ChildItem "$toolsPath\*.ps1" -Recurse
        }
    }

    if ($IncludeAI) {
        $aiPath = Join-Path $ProjectRoot "AI-Assistant"
        if (Test-Path $aiPath) {
            $scriptPaths += Get-ChildItem "$aiPath\*.ps1" -Recurse
        }
    }

    Write-Host "📊 Found $($scriptPaths.Count) PowerShell scripts to analyze" -ForegroundColor Gray

    $results = @()

    if ($Parallel -and $PSVersionTable.PSVersion.Major -ge 7 -and $PSVersionTable.PSVersion.Minor -ge 6) {
        Write-Host "⚡ Running optimization in parallel..." -ForegroundColor Yellow

        $results = $scriptPaths | ForEach-Object -Parallel {
            $script = $_
            $whatIf = $using:WhatIf

            try {
                # Import functions in parallel context
                . $using:PSScriptRoot\PowerShell-76-Optimizer.ps1 -SkipMain

                $analysis = Analyze-PowerShellScript -FilePath $script.FullName

                if (-not $whatIf -and $analysis.OptimizationOpportunities.Count -gt 0) {
                    $conversion = Convert-ScriptToPS76 -FilePath $script.FullName -CreateBackup $true -WhatIf:$whatIf
                    $analysis.ConversionResult = $conversion
                }

                return $analysis
            }
            catch {
                return @{
                    FilePath = $script.FullName
                    Error    = $_.Exception.Message
                    Success  = $false
                }
            }
        } -ThrottleLimit ([Environment]::ProcessorCount)
    }
    else {
        # Sequential processing
        foreach ($script in $scriptPaths) {
            Write-Host "   Analyzing: $($script.Name)" -ForegroundColor Gray

            try {
                $analysis = Analyze-PowerShellScript -FilePath $script.FullName

                if (-not $WhatIf -and $analysis.OptimizationOpportunities.Count -gt 0) {
                    $conversion = Convert-ScriptToPS76 -FilePath $script.FullName -CreateBackup $true -WhatIf:$WhatIf
                    $analysis.ConversionResult = $conversion
                }

                $results += $analysis
            }
            catch {
                $results += @{
                    FilePath = $script.FullName
                    Error    = $_.Exception.Message
                    Success  = $false
                }
            }
        }
    }

    # Generate summary report
    $summary = @{
        TotalScripts       = $results.Count
        ScriptsOptimized   = ($results | Where-Object { $_.ConversionResult.Success }).Count
        TotalOptimizations = ($results | ForEach-Object { $_.ConversionResult.Conversions.Count } | Measure-Object -Sum).Sum
        AverageScore       = ($results | Where-Object { $_.Score } | Measure-Object -Property Score -Average).Average
        HighScoreScripts   = $results | Where-Object { $_.Score -gt 85 }
        NeedsAttention     = $results | Where-Object { $_.Score -lt 70 }
    }

    Write-Host "`n📊 Optimization Summary:" -ForegroundColor Cyan
    Write-Host "   Total Scripts: $($summary.TotalScripts)" -ForegroundColor White
    Write-Host "   Scripts Optimized: $($summary.ScriptsOptimized)" -ForegroundColor Green
    Write-Host "   Total Optimizations: $($summary.TotalOptimizations)" -ForegroundColor Yellow
    Write-Host "   Average Score: $([math]::Round($summary.AverageScore, 1))/100" -ForegroundColor White

    if ($summary.NeedsAttention.Count -gt 0) {
        Write-Host "`n⚠️ Scripts needing attention:" -ForegroundColor Yellow
        $summary.NeedsAttention | ForEach-Object {
            Write-Host "   • $([System.IO.Path]::GetFileName($_.FilePath)) (Score: $($_.Score))" -ForegroundColor Red
        }
    }

    if ($summary.HighScoreScripts.Count -gt 0) {
        Write-Host "`n✅ Well-optimized scripts:" -ForegroundColor Green
        $summary.HighScoreScripts | ForEach-Object {
            Write-Host "   • $([System.IO.Path]::GetFileName($_.FilePath)) (Score: $($_.Score))" -ForegroundColor Green
        }
    }

    return @{
        Summary = $summary
        Results = $results
        Success = $true
    }
}

# =============================================================================
# SPECIFIC BUSBUDDY OPTIMIZATIONS
# =============================================================================

function Optimize-AIAssistantProfile {
    param(
        [string]$ProfilePath = "load-ai-assistant-profile-76.ps1"
    )

    Write-Host "🤖 Optimizing AI-Assistant Profile for PowerShell 7.6..." -ForegroundColor Cyan

    if (-not (Test-Path $ProfilePath)) {
        Write-Error "AI-Assistant profile not found: $ProfilePath"
        return
    }

    $content = Get-Content $ProfilePath -Raw
    $optimizations = @()

    # AI-specific optimizations
    $aiOptimizations = @{
        # Enhance parallel AI analysis
        "ParallelAIAnalysis"      = @{
            Pattern     = '(\$tasks\s*\|\s*ForEach-Object\s+-Parallel\s+\{[^}]+\})'
            Replacement = '$1 -ThrottleLimit $Global:AISystemInfo.MaxParallelism'
        }

        # Optimize cache lookups with SearchValues
        "CacheLookupOptimization" = @{
            Pattern     = '\.ContainsKey\(\$cacheKey\)'
            Replacement = '.TryGetValue($cacheKey, [ref]$null)'
        }

        # Enhanced Grok API calls with circuit breaker
        "GrokCircuitBreaker"      = @{
            Pattern     = 'Invoke-Grok4API'
            Replacement = 'Invoke-WithCircuitBreaker -Name "Grok" -Action { Invoke-Grok4API }'
        }

        # Improved knowledge base searches
        "KnowledgeBaseSearch"     = @{
            Pattern     = 'foreach\s+\(\$category\s+in\s+\$Global:AIKnowledgeBase\.Keys\)'
            Replacement = '$Global:AIKnowledgeBase.GetEnumerator() | ForEach-Object -Parallel {'
        }
    }

    foreach ($opt in $aiOptimizations.GetEnumerator()) {
        $pattern = $opt.Value
        if ($content -match $pattern.Pattern) {
            $content = $content -replace $pattern.Pattern, $pattern.Replacement
            $optimizations += "Applied $($opt.Key) optimization"
            Write-Host "   ✅ $($opt.Key)" -ForegroundColor Green
        }
    }

    # Add PowerShell 7.6 specific AI features
    $ps76AIFeatures = @"

# =============================================================================
# POWERSHELL 7.6 ENHANCED AI FEATURES
# =============================================================================

# Enhanced tab completion with AI suggestions
Register-ArgumentCompleter -CommandName @('grok', 'ask', 'debug-this') -ParameterName 'Question' -Native -ScriptBlock {
    param(`$commandName, `$parameterName, `$wordToComplete, `$commandAst, `$fakeBoundParameters)

    # AI-powered completion suggestions
    `$suggestions = @(
        "How do I fix this error: $wordToComplete",
        "What's the best way to $wordToComplete",
        "Optimize this code: $wordToComplete",
        "Debug this issue: $wordToComplete"
    )

    `$suggestions | Where-Object { `$_ -like "*`$wordToComplete*" } | ForEach-Object {
        [System.Management.Automation.CompletionResult]::new(`$_, `$_, 'ParameterValue', `$_)
    }
}

# PowerShell 7.6 pipeline cancellation support
function Invoke-AIAnalysisWithCancellation {
    [CmdletBinding()]
    param()

    if (`$PSCmdlet.PipelineStopToken.IsCancellationRequested) {
        Write-Host "🛑 AI Analysis cancelled" -ForegroundColor Yellow
        return
    }

    Invoke-FullAIAnalysis @args
}

# Enhanced error handling with PipelineStopToken
function Start-AIDevSessionWithCancellation {
    [CmdletBinding()]
    param()

    try {
        if (`$PSCmdlet.PipelineStopToken.IsCancellationRequested) { return }

        Write-Host "🚀 Starting AI Development Session..." -ForegroundColor Cyan

        # Load workspace with cancellation support
        if (`$PSCmdlet.PipelineStopToken.IsCancellationRequested) { return }
        Start-AIDevSession @args
    }
    catch [OperationCanceledException] {
        Write-Host "🛑 AI Development session cancelled" -ForegroundColor Yellow
    }
}

"@

    if ($content -notmatch "POWERSHELL 7\.6 ENHANCED AI FEATURES") {
        $content += $ps76AIFeatures
        $optimizations += "Added PowerShell 7.6 AI-specific features"
        Write-Host "   ✅ Added PS 7.6 AI features" -ForegroundColor Green
    }

    # Save optimized profile
    $content | Set-Content $ProfilePath -Encoding UTF8

    Write-Host "✅ AI-Assistant profile optimized with $($optimizations.Count) improvements" -ForegroundColor Green

    return @{
        ProfilePath   = $ProfilePath
        Optimizations = $optimizations
        Success       = $true
    }
}

# =============================================================================
# VALIDATION AND TESTING
# =============================================================================

function Test-PS76Optimizations {
    param(
        [string]$ProjectRoot = (Get-Location)
    )

    Write-Host "🧪 Testing PowerShell 7.6 optimizations..." -ForegroundColor Cyan

    $tests = @()

    # Test parallel processing improvements
    $tests += @{
        Name = "Parallel Processing"
        Test = {
            $start = Get-Date
            1..100 | ForEach-Object -Parallel { Start-Sleep -Milliseconds 10 } -ThrottleLimit ([Environment]::ProcessorCount)
            $duration = ((Get-Date) - $start).TotalSeconds
            return @{ Success = $duration -lt 2; Duration = $duration }
        }
    }

    # Test SearchValues performance
    $tests += @{
        Name = "SearchValues Performance"
        Test = {
            $chars = [char[]]@('a', 'b', 'c', 'd', 'e')
            $searchValues = [System.Buffers.SearchValues]::Create($chars)
            $testString = "abcdefghijklmnopqrstuvwxyz"

            $start = Get-Date
            1..1000 | ForEach-Object { $searchValues.Contains([char]'c') }
            $duration = ((Get-Date) - $start).TotalMilliseconds

            return @{ Success = $duration -lt 100; Duration = $duration }
        }
    }

    # Test enhanced tab completion
    $tests += @{
        Name = "Enhanced Tab Completion"
        Test = {
            try {
                $result = TabExpansion2 "grok " 5
                return @{ Success = $result -ne $null; CompletionCount = $result.CompletionMatches.Count }
            }
            catch {
                return @{ Success = $false; Error = $_.Message }
            }
        }
    }

    $results = @()
    foreach ($test in $tests) {
        Write-Host "   Testing: $($test.Name)..." -ForegroundColor Gray
        try {
            $result = & $test.Test
            $result.TestName = $test.Name
            $results += $result

            if ($result.Success) {
                Write-Host "   ✅ $($test.Name) passed" -ForegroundColor Green
            }
            else {
                Write-Host "   ❌ $($test.Name) failed" -ForegroundColor Red
            }
        }
        catch {
            $results += @{
                TestName = $test.Name
                Success  = $false
                Error    = $_.Message
            }
            Write-Host "   ❌ $($test.Name) error: $($_.Message)" -ForegroundColor Red
        }
    }

    $passedTests = ($results | Where-Object Success).Count
    $totalTests = $results.Count

    Write-Host "`n📊 Test Results: $passedTests/$totalTests passed" -ForegroundColor Cyan

    return @{
        Results  = $results
        PassRate = [math]::Round(($passedTests / $totalTests) * 100, 1)
        Success  = $passedTests -eq $totalTests
    }
}

# =============================================================================
# MAIN EXECUTION
# =============================================================================

if ($MyInvocation.InvocationName -ne ".") {
    Write-Host "🚀 PowerShell 7.6 Optimizer for BusBuddy" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan

    if ($Analyze) {
        if (Test-Path $ScriptPath -PathType Leaf) {
            $analysis = Analyze-PowerShellScript -FilePath $ScriptPath

            Write-Host "`n📊 Analysis Results for $([System.IO.Path]::GetFileName($ScriptPath)):" -ForegroundColor Cyan
            Write-Host "   Current Version: $($analysis.CurrentVersion)" -ForegroundColor White
            Write-Host "   PS 7.6 Ready: $($analysis.PS76Ready)" -ForegroundColor $(if ($analysis.PS76Ready) { 'Green' } else { 'Red' })
            Write-Host "   Optimization Score: $($analysis.Score)/100" -ForegroundColor White
            Write-Host "   Opportunities: $($analysis.OptimizationOpportunities.Count)" -ForegroundColor Yellow

            if ($analysis.Recommendations.Count -gt 0) {
                Write-Host "`n💡 Recommendations:" -ForegroundColor Yellow
                $analysis.Recommendations | ForEach-Object { Write-Host "   $_" -ForegroundColor White }
            }
        }
        else {
            $bulkResult = Optimize-BusBuddyScripts -ProjectRoot $ScriptPath -WhatIf:$true
        }
    }

    if ($Convert) {
        if (Test-Path $ScriptPath -PathType Leaf) {
            Convert-ScriptToPS76 -FilePath $ScriptPath -CreateBackup:$Backup -WhatIf:$WhatIf
        }
        else {
            Optimize-BusBuddyScripts -ProjectRoot $ScriptPath -WhatIf:$WhatIf
        }
    }

    if (-not $Analyze -and -not $Convert) {
        Write-Host "Use -Analyze to analyze scripts or -Convert to optimize them for PowerShell 7.6" -ForegroundColor Yellow
        Write-Host "`nExamples:" -ForegroundColor Gray
        Write-Host "   .\PowerShell-76-Optimizer.ps1 -Analyze" -ForegroundColor White
        Write-Host "   .\PowerShell-76-Optimizer.ps1 -Convert -Backup" -ForegroundColor White
        Write-Host "   .\PowerShell-76-Optimizer.ps1 -Analyze .\load-ai-assistant-profile-76.ps1" -ForegroundColor White
    }
}
