#Requires -Version 7.5

<#
.SYNOPSIS
PowerShell 7.5.2 Syntax Enforcer - Mandatory validation for all BusBuddy PowerShell scripts

.DESCRIPTION
Enforces PowerShell 7.5.2 best practices and syntax requirements for all BusBuddy scripts.
Must be used before any script deployment or module building.

.NOTES
Made mandatory for BusBuddy development workflow
#>

function Test-PowerShell752Syntax {
    <#
    .SYNOPSIS
    Validate PowerShell script against 7.5.2 requirements
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$ScriptPath,

        [Parameter()]
        [switch]$Fix,

        [Parameter()]
        [switch]$Strict
    )

    # Parameter validation
    if ([string]::IsNullOrEmpty($ScriptPath)) {
        throw "ScriptPath parameter cannot be null or empty"
    }

    $issues = @()
    $content = Get-Content -Path $ScriptPath -Raw

    Write-Host "🔍 Validating PowerShell 7.5.2 syntax: $ScriptPath" -ForegroundColor Cyan

    # 1. Check #Requires directive
    if ($content -notmatch '#Requires\s+-Version\s+7\.5') {
        $issues += @{
            Type    = "Critical"
            Rule    = "RequiresVersion"
            Message = "Missing '#Requires -Version 7.5' directive"
            Fix     = "Add '#Requires -Version 7.5' at the top of the script"
            Line    = 1
        }
    }

    # 2. Check for proper parallel processing syntax
    if ($content -match 'ForEach-Object\s+-Parallel' -and $content -notmatch '-ThrottleLimit') {
        $issues += @{
            Type    = "Warning"
            Rule    = "ParallelThrottling"
            Message = "ForEach-Object -Parallel should specify -ThrottleLimit"
            Fix     = "Add -ThrottleLimit parameter (recommended: 4-8)"
        }
    }

    # 3. Check for proper error handling in parallel blocks
    if ($content -match 'ForEach-Object\s+-Parallel') {
        # Simplified check: Look for ForEach-Object -Parallel followed by try/catch within reasonable distance
        $parallelMatches = [regex]::Matches($content, 'ForEach-Object\s+-Parallel\s*\{', 'IgnoreCase')
        $hasParallelTryCatch = $false

        foreach ($match in $parallelMatches) {
            # Extract the content after ForEach-Object -Parallel { for about 500 characters
            $startPos = $match.Index + $match.Length
            $endPos = [Math]::Min($startPos + 500, $content.Length)
            $blockContent = $content.Substring($startPos, $endPos - $startPos)

            # Check if this block contains try/catch
            if ($blockContent -match '(?s)try\s*\{.*?catch') {
                $hasParallelTryCatch = $true
                break
            }
        }

        # Only add issue if we found parallel blocks but no try/catch
        if ($parallelMatches.Count -gt 0 -and -not $hasParallelTryCatch) {
            $issues += @{
                Type    = "Critical"
                Rule    = "ParallelErrorHandling"
                Message = "Parallel blocks must include try/catch error handling"
                Fix     = "Wrap parallel block content in try/catch"
            }
        }
    }

    # 4. Check for thread job cleanup
    if ($content -match 'Start-ThreadJob' -and $content -notmatch 'Remove-Job') {
        $issues += @{
            Type    = "Critical"
            Rule    = "ThreadJobCleanup"
            Message = "Thread jobs must be cleaned up with Remove-Job"
            Fix     = "Add '$jobs | Remove-Job' after job completion"
        }
    }

    # 5. Check for proper null handling
    if ($content -match '\$null\s*-eq' -and $content -notmatch '\?\?') {
        $issues += @{
            Type    = "Suggestion"
            Rule    = "NullCoalescing"
            Message = "Consider using null coalescing operator (??)"
            Fix     = "Replace '$null -eq $var' with '$var ?? $default'"
        }
    }

    # 6. Check for using statement with IDisposable
    if ($content -match 'new\s+\w+Context\(\)' -and $content -notmatch 'using\s*\(') {
        $issues += @{
            Type    = "Critical"
            Rule    = "UsingStatement"
            Message = "IDisposable objects should use 'using' statement"
            Fix     = "Wrap in 'using ($context = new Context()) { }'"
        }
    }

    # 7. Check for proper async patterns
    if ($content -match 'async\s+\w+' -and $content -notmatch 'ConfigureAwait\(false\)') {
        $issues += @{
            Type    = "Warning"
            Rule    = "ConfigureAwait"
            Message = "Async calls should use ConfigureAwait(false) in libraries"
            Fix     = "Add .ConfigureAwait(false) to async calls"
        }
    }

    # 8. Check for proper parameter validation
    $parameterBlocks = [regex]::Matches($content, '\[Parameter\([^\]]*\)\]\s*\[([^\]]+)\]\s*\$(\w+)')
    $unvalidatedStringParams = @()

    foreach ($match in $parameterBlocks) {
        $paramType = $match.Groups[1].Value
        $paramName = $match.Groups[2].Value

        if ($paramType -eq "string" -and $content -notmatch "if\s*\(\s*\[string\]::IsNullOrEmpty\(\`$$paramName\)\)") {
            $unvalidatedStringParams += $paramName
        }
    }

    # Add single suggestion if any unvalidated string parameters found
    if ($unvalidatedStringParams.Count -gt 0) {
        $paramList = $unvalidatedStringParams -join ", "
        $issues += @{
            Type    = "Suggestion"
            Rule    = "ParameterValidation"
            Message = "String parameters should validate for null: $paramList"
            Fix     = "Add validation: if ([string]::IsNullOrEmpty(`$paramName)) { throw ... }"
        }
    }

    # 9. Check for proper module structure
    if ($ScriptPath -like "*.psm1" -and $content -notmatch 'Export-ModuleMember') {
        $issues += @{
            Type    = "Critical"
            Rule    = "ModuleExports"
            Message = "PowerShell modules must explicitly export members"
            Fix     = "Add Export-ModuleMember -Function 'FunctionName'"
        }
    }

    # 10. Check for BusBuddy-specific patterns
    if ($content -match 'Write-Host' -and $content -notmatch 'Logger\.') {
        # Count only once per script, regardless of how many Write-Host instances
        $issues += @{
            Type    = "Warning"
            Rule    = "BusBuddyLogging"
            Message = "Use Serilog Logger instead of Write-Host in BusBuddy"
            Fix     = "Replace Write-Host with Logger.Information()"
        }
    }

    # Report results
    Write-Host "`n📊 Validation Results:" -ForegroundColor Yellow
    Write-Host "   Total Issues: $($issues.Count)" -ForegroundColor Gray

    $critical = $issues | Where-Object { $_.Type -eq "Critical" }
    $warnings = $issues | Where-Object { $_.Type -eq "Warning" }
    $suggestions = $issues | Where-Object { $_.Type -eq "Suggestion" }

    if ($critical.Count -gt 0) {
        Write-Host "   🔴 Critical: $($critical.Count)" -ForegroundColor Red
        foreach ($issue in $critical) {
            Write-Host "      ❌ $($issue.Rule): $($issue.Message)" -ForegroundColor Red
            Write-Host "         Fix: $($issue.Fix)" -ForegroundColor Gray
        }
    }

    if ($warnings.Count -gt 0) {
        Write-Host "   🟡 Warnings: $($warnings.Count)" -ForegroundColor Yellow
        foreach ($issue in $warnings) {
            Write-Host "      ⚠️  $($issue.Rule): $($issue.Message)" -ForegroundColor Yellow
            Write-Host "         Fix: $($issue.Fix)" -ForegroundColor Gray
        }
    }

    if ($suggestions.Count -gt 0) {
        Write-Host "   🔵 Suggestions: $($suggestions.Count)" -ForegroundColor Blue
        foreach ($issue in $suggestions) {
            Write-Host "      💡 $($issue.Rule): $($issue.Message)" -ForegroundColor Blue
            Write-Host "         Fix: $($issue.Fix)" -ForegroundColor Gray
        }
    }

    # Determine if script passes validation
    $passes = if ($Strict) {
        $issues.Count -eq 0
    }
    else {
        $critical.Count -eq 0
    }

    if ($passes) {
        Write-Host "`n✅ Script passes PowerShell 7.5.2 validation!" -ForegroundColor Green
    }
    else {
        Write-Host "`n❌ Script FAILS PowerShell 7.5.2 validation!" -ForegroundColor Red
        if ($critical.Count -gt 0) {
            Write-Host "   Critical issues must be fixed before deployment" -ForegroundColor Red
        }
    }

    return @{
        Passes      = $passes
        Issues      = $issues
        Critical    = $critical.Count
        Warnings    = $warnings.Count
        Suggestions = $suggestions.Count
        ScriptPath  = $ScriptPath
    }
}

function New-PowerShell752Template {
    <#
    .SYNOPSIS
    Generate a PowerShell 7.5.2 compliant script template
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$ScriptName,

        [Parameter()]
        [ValidateSet("Script", "Module", "Function")]
        [string]$Type = "Script",

        [Parameter()]
        [string]$OutputPath = "."
    )

    # Parameter validation
    if ([string]::IsNullOrEmpty($ScriptName)) {
        throw "ScriptName parameter cannot be null or empty"
    }
    if ([string]::IsNullOrEmpty($OutputPath)) {
        throw "OutputPath parameter cannot be null or empty"
    }

    $template = switch ($Type) {
        "Script" {
            @"
#Requires -Version 7.5

<#
.SYNOPSIS
$ScriptName - PowerShell 7.5.2 compliant script

.DESCRIPTION
Description of what this script does

.PARAMETER InputPath
Path to input file or directory

.EXAMPLE
.\$ScriptName.ps1 -InputPath "C:\Data"

.NOTES
Created: $(Get-Date -Format 'yyyy-MM-dd')
PowerShell: 7.5.2+
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string]`$InputPath
)

# Script-level error handling
`$ErrorActionPreference = 'Stop'

# Version check
if (`$PSVersionTable.PSVersion -lt [version]'7.5.0') {
    throw "This script requires PowerShell 7.5.0 or later"
}

try {
    # Validate parameters
    if ([string]::IsNullOrEmpty(`$InputPath)) {
        throw "InputPath parameter cannot be null or empty"
    }

    if (-not (Test-Path `$InputPath)) {
        throw "Path not found: `$InputPath"
    }

    Write-Host "🚀 Starting `$ScriptName..." -ForegroundColor Cyan

    # Main script logic here
    `$results = Get-ChildItem -Path `$InputPath -Recurse | ForEach-Object -Parallel {
        param(`$item)
        try {
            # Process each item
            return [PSCustomObject]@{
                Name = `$item.Name
                Size = `$item.Length
                Status = "Processed"
            }
        } catch {
            return [PSCustomObject]@{
                Name = `$item.Name
                Size = 0
                Status = "Failed: `$(`$_.Exception.Message)"
            }
        }
    } -ThrottleLimit 4

    Write-Host "✅ Processing completed successfully" -ForegroundColor Green
    return `$results

} catch {
    Write-Error "Script failed: `$(`$_.Exception.Message)"
    throw
}
"@
        }

        "Module" {
            @"
#Requires -Version 7.5

<#
.SYNOPSIS
$ScriptName PowerShell Module

.DESCRIPTION
PowerShell 7.5.2 compliant module with proper exports

.NOTES
Created: $(Get-Date -Format 'yyyy-MM-dd')
PowerShell: 7.5.2+
#>

# Module-level variables
`$script:ModuleName = "$ScriptName"
`$script:ModuleVersion = "1.0.0"

function Get-$ScriptName {
    <#
    .SYNOPSIS
    Main function for $ScriptName module

    .PARAMETER InputData
    Data to process

    .EXAMPLE
    Get-$ScriptName -InputData "test"
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]`$InputData
    )

    try {
        if ([string]::IsNullOrEmpty(`$InputData)) {
            throw "InputData parameter cannot be null or empty"
        }

        Write-Verbose "Processing input data: `$InputData"

        # Process data here
        `$result = [PSCustomObject]@{
            Input = `$InputData
            Processed = Get-Date
            Status = "Success"
        }

        return `$result

    } catch {
        Write-Error "Function failed: `$(`$_.Exception.Message)"
        throw
    }
}

function Start-$ScriptName {
    <#
    .SYNOPSIS
    Start $ScriptName processing with parallel execution

    .PARAMETER Items
    Collection of items to process

    .EXAMPLE
    Start-$ScriptName -Items @("item1", "item2")
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string[]]`$Items
    )

    try {
        if ($null -eq `$Items -or `$Items.Count -eq 0) {
            throw "Items parameter cannot be null or empty"
        }

        Write-Verbose "Starting parallel processing of `$(`$Items.Count) items"

        `$results = `$Items | ForEach-Object -Parallel {
            param(`$item)
            try {
                # Import module in parallel scope if needed
                # Import-Module $ScriptName -Force

                return Get-$ScriptName -InputData `$item

            } catch {
                return [PSCustomObject]@{
                    Input = `$item
                    Processed = Get-Date
                    Status = "Failed: `$(`$_.Exception.Message)"
                }
            }
        } -ThrottleLimit 4

        return `$results

    } catch {
        Write-Error "Parallel processing failed: `$(`$_.Exception.Message)"
        throw
    }
}

# Export module members
Export-ModuleMember -Function 'Get-$ScriptName', 'Start-$ScriptName'
"@
        }

        "Function" {
            @"
#Requires -Version 7.5

function Invoke-$ScriptName {
    <#
    .SYNOPSIS
    PowerShell 7.5.2 compliant function

    .DESCRIPTION
    Detailed description of the function

    .PARAMETER InputObject
    Object to process

    .PARAMETER ProcessInParallel
    Enable parallel processing

    .EXAMPLE
    Invoke-$ScriptName -InputObject `$data

    .EXAMPLE
    Invoke-$ScriptName -InputObject `$collection -ProcessInParallel

    .NOTES
    Created: $(Get-Date -Format 'yyyy-MM-dd')
    PowerShell: 7.5.2+
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory, ValueFromPipeline)]
        [ValidateNotNull()]
        [object]`$InputObject,

        [Parameter()]
        [switch]`$ProcessInParallel
    )

    begin {
        # Validate PowerShell version
        if (`$PSVersionTable.PSVersion -lt [version]'7.5.0') {
            throw "This function requires PowerShell 7.5.0 or later"
        }

        `$allObjects = @()
    }

    process {
        `$allObjects += `$InputObject
    }

    end {
        try {
            if (`$ProcessInParallel -and `$allObjects.Count -gt 1) {
                Write-Verbose "Processing `$(`$allObjects.Count) objects in parallel"

                `$results = `$allObjects | ForEach-Object -Parallel {
                    param(`$obj)
                    try {
                        # Process object
                        return [PSCustomObject]@{
                            Original = `$obj
                            Processed = `$obj.ToString().ToUpper()
                            Timestamp = Get-Date
                            Status = "Success"
                        }
                    } catch {
                        return [PSCustomObject]@{
                            Original = `$obj
                            Processed = `$null
                            Timestamp = Get-Date
                            Status = "Failed: `$(`$_.Exception.Message)"
                        }
                    }
                } -ThrottleLimit 4

            } else {
                Write-Verbose "Processing `$(`$allObjects.Count) objects sequentially"

                `$results = `$allObjects | ForEach-Object {
                    [PSCustomObject]@{
                        Original = `$_
                        Processed = `$_.ToString().ToUpper()
                        Timestamp = Get-Date
                        Status = "Success"
                    }
                }
            }

            return `$results

        } catch {
            Write-Error "Function failed: `$(`$_.Exception.Message)"
            throw
        }
    }
}
"@
        }
    }

    $fileName = switch ($Type) {
        "Module" { "$ScriptName.psm1" }
        default { "$ScriptName.ps1" }
    }

    $filePath = Join-Path $OutputPath $fileName
    $template | Out-File -FilePath $filePath -Encoding UTF8

    Write-Host "✅ PowerShell 7.5.2 $Type template created: $filePath" -ForegroundColor Green
    Write-Host "📋 Template includes:" -ForegroundColor Yellow
    Write-Host "   • #Requires -Version 7.5 directive" -ForegroundColor Gray
    Write-Host "   • Proper parameter validation" -ForegroundColor Gray
    Write-Host "   • Parallel processing with throttling" -ForegroundColor Gray
    Write-Host "   • Structured error handling" -ForegroundColor Gray
    Write-Host "   • PowerShell 7.5.2 best practices" -ForegroundColor Gray

    return $filePath
}

function Invoke-MandatorySyntaxCheck {
    <#
    .SYNOPSIS
    Mandatory syntax check for all BusBuddy PowerShell scripts
    This function MUST be called before any script deployment

    .PARAMETER LegacyExemption
    Skip validation for legacy scripts (default: true for progressive adoption)
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Path,

        [Parameter()]
        [switch]$StrictMode,

        [Parameter()]
        [switch]$ExitOnFail,

        [Parameter()]
        [switch]$LegacyExemption,  # DEFAULT: Progressive adoption

        [Parameter()]
        [string[]]$ExcludePaths = @(
            "Archive\*",
            "Legacy-Scripts\*",
            "PowerShell-Profile\*",
            "Scripts\Efficiency\*",
            "BusBuddy-PowerShell-Profile-7.5.2.ps1",
            "load-*.ps1",
            "phase1-*.ps1"
        ),

        [Parameter()]
        [string[]]$OnlyValidateNew = @()  # Only validate specific new files
    )

    # Parameter validation
    if ([string]::IsNullOrEmpty($Path)) {
        throw "Path parameter cannot be null or empty"
    }

    Write-Host "🔒 MANDATORY PowerShell 7.5.2 Syntax Check" -ForegroundColor Magenta
    Write-Host "   Path: $Path" -ForegroundColor Gray

    if ($LegacyExemption) {
        Write-Host "   Mode: Progressive (Legacy scripts exempted)" -ForegroundColor Yellow
    }
    else {
        Write-Host "   Mode: Strict (All scripts validated)" -ForegroundColor Red
    }

    $allPassed = $true
    $totalIssues = 0
    $skippedCount = 0

    if (Test-Path $Path -PathType Container) {
        # Check directory
        $scripts = Get-ChildItem -Path $Path -Filter "*.ps1" -Recurse
        $modules = Get-ChildItem -Path $Path -Filter "*.psm1" -Recurse
        $allFiles = $scripts + $modules

        # Default to progressive mode unless explicitly disabled
        if (-not $PSBoundParameters.ContainsKey('LegacyExemption')) {
            $LegacyExemption = $true
        }

        # Apply exemptions if enabled
        if ($LegacyExemption) {
            $filteredFiles = @()
            foreach ($file in $allFiles) {
                $shouldSkip = $false
                $relativePath = $file.FullName.Replace($PWD.Path, "").TrimStart('\')

                foreach ($excludePattern in $ExcludePaths) {
                    if ($relativePath -like $excludePattern) {
                        $shouldSkip = $true
                        $skippedCount++
                        break
                    }
                }

                if (-not $shouldSkip) {
                    $filteredFiles += $file
                }
            }
            $allFiles = $filteredFiles
        }

        foreach ($file in $allFiles) {
            $result = Test-PowerShell752Syntax -ScriptPath $file.FullName -Strict:$StrictMode
            if (-not $result.Passes) {
                $allPassed = $false
                $totalIssues += $result.Critical + $result.Warnings
            }
        }
    }
    else {
        # Check single file
        $result = Test-PowerShell752Syntax -ScriptPath $Path -Strict:$StrictMode
        if (-not $result.Passes) {
            $allPassed = $false
            $totalIssues += $result.Critical + $result.Warnings
        }
    }

    Write-Host "`n🎯 MANDATORY CHECK RESULT:" -ForegroundColor Magenta
    if ($LegacyExemption -and $skippedCount -gt 0) {
        Write-Host "   📋 Legacy scripts exempted: $skippedCount" -ForegroundColor Yellow
    }

    if ($allPassed) {
        Write-Host "   ✅ ALL SCRIPTS PASS - Deployment allowed" -ForegroundColor Green
        if ($LegacyExemption) {
            Write-Host "   📈 Progressive mode: Only validating new/modified scripts" -ForegroundColor Cyan
        }
    }
    else {
        Write-Host "   ❌ VALIDATION FAILED - Deployment blocked" -ForegroundColor Red
        Write-Host "   Total issues: $totalIssues" -ForegroundColor Red

        if ($ExitOnFail) {
            throw "PowerShell 7.5.2 syntax validation failed. Fix issues before proceeding."
        }
    }

    return $allPassed
}

# Auto-export for module usage
if ($MyInvocation.MyCommand.Path -like "*.psm1") {
    Export-ModuleMember -Function 'Test-PowerShell752Syntax', 'New-PowerShell752Template', 'Invoke-MandatorySyntaxCheck'
}

Write-Host "🔧 PowerShell 7.5.2 Syntax Enforcer loaded" -ForegroundColor Green
Write-Host "   Use 'Test-PowerShell752Syntax -ScriptPath file.ps1' to validate" -ForegroundColor Cyan
Write-Host "   Use 'New-PowerShell752Template -ScriptName MyScript' to create template" -ForegroundColor Cyan
Write-Host "   Use 'Invoke-MandatorySyntaxCheck -Path .' for mandatory validation" -ForegroundColor Cyan
