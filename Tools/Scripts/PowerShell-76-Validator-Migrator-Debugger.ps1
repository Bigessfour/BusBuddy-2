#Requires -Version 7.6
<#
.SYNOPSIS
    PowerShell 7.6 Comprehensive Validator, Migrator, and Debugger for BusBuddy

.DESCRIPTION
    Advanced toolset for PowerShell 7.6 compatibility validation, migration assistance,
    and debugging support. Implements all latest PowerShell 7.6 features and guidelines.

.FEATURES
    - PowerShell 7.6 syntax validation and migration
    - .NET 9 compatibility checking
    - Performance optimization recommendations
    - Breaking change detection and remediation
    - Tab completion improvements validation
    - Cmdlet enhancement verification
    - Engine improvement utilization
    - Experimental feature management

.EXAMPLE
    PS> .\PowerShell-76-Validator-Migrator-Debugger.ps1 -ValidateProject
    PS> .\PowerShell-76-Validator-Migrator-Debugger.ps1 -MigrateToPS76
    PS> .\PowerShell-76-Validator-Migrator-Debugger.ps1 -DebugIssues

.NOTES
    Based on PowerShell 7.6 What's New documentation
    https://learn.microsoft.com/en-us/powershell/scripting/whats-new/what-s-new-in-powershell-76
#>

[CmdletBinding()]
param(
    [Parameter(ParameterSetName = 'Validate')]
    [switch]$ValidateProject,

    [Parameter(ParameterSetName = 'Migrate')]
    [switch]$MigrateToPS76,

    [Parameter(ParameterSetName = 'Debug')]
    [switch]$DebugIssues,

    [Parameter(ParameterSetName = 'All')]
    [switch]$RunAll,

    [string]$ProjectPath = (Get-Location),
    [switch]$GenerateReport,
    [switch]$AutoFix,
    [switch]$Quiet
)

# Strict error handling for accurate validation
$ErrorActionPreference = 'Stop'
$ProgressPreference = if ($Quiet) { 'SilentlyContinue' } else { 'Continue' }

#region PowerShell 7.6 Feature Detection and Validation

class PowerShell76Features {
    [hashtable]$ModuleUpdates = @{
        'Microsoft.PowerShell.PSResourceGet' = '1.1.0'
        'PSReadLine' = '2.3.6'
        'Microsoft.PowerShell.ThreadJob' = '2.2.0'
        'ThreadJob' = '2.1.0'  # Proxy module for backward compatibility
    }

    [string[]]$BreakingChanges = @(
        'WildcardPattern.Escape backtick handling'
        'Join-Path -ChildPath parameter now string[]'
        'Event source name trailing space removal'
    )

    [string[]]$TabCompletionImprovements = @(
        'Named and Statement block type inference'
        'New-ItemProperty -PropertyType completer'
        'Get-Command -Noun quote support'
        'Get-Module -PSEdition quote support'
        'SearchValues<char> optimizations'
        'Variable type inference improvements'
        'Hashtable key completion tooltips'
        'Assignment type inference'
        'Attribute argument completion'
    )

    [string[]]$CmdletImprovements = @(
        'Get-Command -ExcludeModule parameter'
        'Get-Item FileName property for ADS'
        'Get-ItemProperty error handling'
        'New-Item -Force validation'
        'Start-Transcript PSObject support'
        'Start-Process -Wait efficiency'
    )

    [string[]]$EngineImprovements = @(
        'AIShell module telemetry'
        'EnumSingleTypeConverter helper'
        'X509Certificate2 DnsNameList updates'
        'PipelineStopToken for Cmdlets'
        'AppLocker fallback'
        '.NET method invocation logging'
        'SystemPolicy Unix compatibility'
    )

    [string[]]$ExperimentalFeatures = @(
        'PSNativeWindowsTildeExpansion'
        'PSRedirectToVariable'
        'PSSerializeJSONLongEnumAsNumber'
    )
}

class ValidationResult {
    [string]$Category
    [string]$Item
    [string]$Status  # Pass, Fail, Warning, Info
    [string]$Message
    [string]$Recommendation
    [string]$AutoFixAvailable
    [hashtable]$Metadata = @{}
}

class MigrationAction {
    [string]$Type  # Replace, Add, Remove, Modify
    [string]$FilePath
    [int]$LineNumber
    [string]$OldContent
    [string]$NewContent
    [string]$Reason
    [string]$Category
}

#endregion

#region PowerShell 7.6 Validator Functions

function Test-PowerShell76Compatibility {
    [CmdletBinding()]
    param(
        [string]$Path = $ProjectPath,
        [switch]$IncludePerformanceChecks
    )

    Write-Host "🔍 PowerShell 7.6 Compatibility Validation" -ForegroundColor Cyan
    Write-Host "===========================================" -ForegroundColor Cyan

    $results = @()
    $psFiles = Get-ChildItem -Path $Path -Recurse -Include "*.ps1", "*.psm1", "*.psd1" -ErrorAction SilentlyContinue

    if (-not $psFiles) {
        Write-Warning "No PowerShell files found in path: $Path"
        return $results
    }

    # Test 1: PowerShell Version Requirements
    $results += Test-PowerShellVersionRequirements -Files $psFiles

    # Test 2: Breaking Changes Detection
    $results += Test-BreakingChanges -Files $psFiles

    # Test 3: Module Dependencies
    $results += Test-ModuleDependencies -Files $psFiles

    # Test 4: Tab Completion Optimization Opportunities
    $results += Test-TabCompletionOptimizations -Files $psFiles

    # Test 5: Cmdlet Enhancement Opportunities
    $results += Test-CmdletEnhancements -Files $psFiles

    # Test 6: Engine Improvement Utilization
    $results += Test-EngineImprovements -Files $psFiles

    # Test 7: Experimental Feature Usage
    $results += Test-ExperimentalFeatures -Files $psFiles

    if ($IncludePerformanceChecks) {
        # Test 8: Performance Optimization Opportunities
        $results += Test-PerformanceOptimizations -Files $psFiles
    }

    return $results
}

function Test-PowerShellVersionRequirements {
    param([System.IO.FileInfo[]]$Files)

    $results = @()

    foreach ($file in $Files) {
        $content = Get-Content -Path $file.FullName -Raw -ErrorAction SilentlyContinue
        if (-not $content) { continue }

        # Check for #Requires -Version
        if ($content -match '#Requires\s+-Version\s+(\d+\.\d+)') {
            $requiredVersion = [version]$matches[1]
            $targetVersion = [version]'7.6'

            if ($requiredVersion -lt $targetVersion) {
                $results += [ValidationResult]@{
                    Category = 'Version Requirements'
                    Item = $file.Name
                    Status = 'Warning'
                    Message = "File requires PowerShell $requiredVersion, consider updating to 7.6"
                    Recommendation = "Update #Requires -Version to 7.6 for latest features"
                    AutoFixAvailable = 'Yes'
                    Metadata = @{ FilePath = $file.FullName; CurrentVersion = $requiredVersion }
                }
            } elseif ($requiredVersion -eq $targetVersion) {
                $results += [ValidationResult]@{
                    Category = 'Version Requirements'
                    Item = $file.Name
                    Status = 'Pass'
                    Message = "File correctly requires PowerShell 7.6"
                    Recommendation = ''
                    AutoFixAvailable = 'No'
                }
            }
        } else {
            $results += [ValidationResult]@{
                Category = 'Version Requirements'
                Item = $file.Name
                Status = 'Warning'
                Message = "No #Requires -Version specified"
                Recommendation = "Add #Requires -Version 7.6 to ensure compatibility"
                AutoFixAvailable = 'Yes'
                Metadata = @{ FilePath = $file.FullName }
            }
        }
    }

    return $results
}

function Test-BreakingChanges {
    param([System.IO.FileInfo[]]$Files)

    $results = @()

    foreach ($file in $Files) {
        $content = Get-Content -Path $file.FullName -Raw -ErrorAction SilentlyContinue
        if (-not $content) { continue }

        # Breaking Change 1: WildcardPattern.Escape backtick handling
        if ($content -match '\[WildcardPattern\]::Escape\([^)]*`[^)]*\)') {
            $results += [ValidationResult]@{
                Category = 'Breaking Changes'
                Item = $file.Name
                Status = 'Fail'
                Message = "WildcardPattern.Escape usage may be affected by backtick handling changes"
                Recommendation = "Review WildcardPattern.Escape calls for lone backtick handling"
                AutoFixAvailable = 'Manual'
                Metadata = @{ FilePath = $file.FullName; ChangeType = 'WildcardPattern.Escape' }
            }
        }

        # Breaking Change 2: Join-Path -ChildPath parameter change
        if ($content -match 'Join-Path\s+[^-\r\n]*-ChildPath\s+[^-\s\r\n]+(?!\s*,|\s*@)') {
            $results += [ValidationResult]@{
                Category = 'Breaking Changes'
                Item = $file.Name
                Status = 'Warning'
                Message = "Join-Path -ChildPath parameter is now string[] - verify single string usage"
                Recommendation = "Ensure Join-Path -ChildPath usage is compatible with string[] type"
                AutoFixAvailable = 'Manual'
                Metadata = @{ FilePath = $file.FullName; ChangeType = 'Join-Path' }
            }
        }

        # Check for event source usage that might be affected by trailing space removal
        if ($content -match 'New-EventLog|Write-EventLog|Get-EventLog') {
            $results += [ValidationResult]@{
                Category = 'Breaking Changes'
                Item = $file.Name
                Status = 'Info'
                Message = "Event log usage detected - verify source names don't rely on trailing spaces"
                Recommendation = "Check event source names for trailing space dependencies"
                AutoFixAvailable = 'Manual'
                Metadata = @{ FilePath = $file.FullName; ChangeType = 'EventSource' }
            }
        }
    }

    return $results
}

function Test-ModuleDependencies {
    param([System.IO.FileInfo[]]$Files)

    $results = @()
    $features = [PowerShell76Features]::new()

    foreach ($file in $Files) {
        $content = Get-Content -Path $file.FullName -Raw -ErrorAction SilentlyContinue
        if (-not $content) { continue }

        # Check for updated modules
        foreach ($module in $features.ModuleUpdates.Keys) {
            if ($content -match "Import-Module\s+['\`"]?$module['\`"]?|#requires\s+-modules?\s+$module") {
                $recommendedVersion = $features.ModuleUpdates[$module]
                $results += [ValidationResult]@{
                    Category = 'Module Dependencies'
                    Item = "$($file.Name) - $module"
                    Status = 'Info'
                    Message = "Module $module usage detected"
                    Recommendation = "Ensure using version $recommendedVersion or later"
                    AutoFixAvailable = 'Manual'
                    Metadata = @{ FilePath = $file.FullName; Module = $module; RecommendedVersion = $recommendedVersion }
                }
            }
        }

        # Check for ThreadJob module usage (backward compatibility)
        if ($content -match 'ThreadJob' -and $content -notmatch 'Microsoft\.PowerShell\.ThreadJob') {
            $results += [ValidationResult]@{
                Category = 'Module Dependencies'
                Item = "$($file.Name) - ThreadJob"
                Status = 'Warning'
                Message = "ThreadJob module usage detected - consider updating to Microsoft.PowerShell.ThreadJob"
                Recommendation = "Update to use Microsoft.PowerShell.ThreadJob v2.2.0 for latest features"
                AutoFixAvailable = 'Yes'
                Metadata = @{ FilePath = $file.FullName; OldModule = 'ThreadJob'; NewModule = 'Microsoft.PowerShell.ThreadJob' }
            }
        }
    }

    return $results
}

function Test-TabCompletionOptimizations {
    param([System.IO.FileInfo[]]$Files)

    $results = @()

    foreach ($file in $Files) {
        $content = Get-Content -Path $file.FullName -Raw -ErrorAction SilentlyContinue
        if (-not $content) { continue }

        # Check for completion opportunities
        if ($content -match 'Register-ArgumentCompleter|ValidateSet') {
            $results += [ValidationResult]@{
                Category = 'Tab Completion'
                Item = $file.Name
                Status = 'Info'
                Message = "Custom completion detected - can benefit from PowerShell 7.6 improvements"
                Recommendation = "Review completion implementation for new 7.6 features"
                AutoFixAvailable = 'Manual'
                Metadata = @{ FilePath = $file.FullName; OptimizationType = 'ArgumentCompleter' }
            }
        }

        # Check for hashtable usage that could benefit from key completion tooltips
        if ($content -match '@\{[^}]*\}' -and $content -match 'param\s*\(') {
            $results += [ValidationResult]@{
                Category = 'Tab Completion'
                Item = $file.Name
                Status = 'Info'
                Message = "Hashtable parameters detected - can add completion tooltips"
                Recommendation = "Consider adding tooltips for hashtable key completions"
                AutoFixAvailable = 'Manual'
                Metadata = @{ FilePath = $file.FullName; OptimizationType = 'HashtableTooltips' }
            }
        }
    }

    return $results
}

function Test-CmdletEnhancements {
    param([System.IO.FileInfo[]]$Files)

    $results = @()

    foreach ($file in $Files) {
        $content = Get-Content -Path $file.FullName -Raw -ErrorAction SilentlyContinue
        if (-not $content) { continue }

        # Check for Get-Command usage that could benefit from -ExcludeModule
        if ($content -match 'Get-Command' -and $content -notmatch '-ExcludeModule') {
            $results += [ValidationResult]@{
                Category = 'Cmdlet Enhancements'
                Item = $file.Name
                Status = 'Info'
                Message = "Get-Command usage could benefit from new -ExcludeModule parameter"
                Recommendation = "Consider using -ExcludeModule to filter command results"
                AutoFixAvailable = 'Manual'
                Metadata = @{ FilePath = $file.FullName; Enhancement = 'Get-Command-ExcludeModule' }
            }
        }

        # Check for Start-Process usage that could benefit from efficiency improvements
        if ($content -match 'Start-Process.*-Wait') {
            $results += [ValidationResult]@{
                Category = 'Cmdlet Enhancements'
                Item = $file.Name
                Status = 'Pass'
                Message = "Start-Process -Wait benefits from PowerShell 7.6 polling efficiency improvements"
                Recommendation = "No action needed - automatically benefits from improvements"
                AutoFixAvailable = 'No'
                Metadata = @{ FilePath = $file.FullName; Enhancement = 'Start-Process-Efficiency' }
            }
        }

        # Check for Start-Transcript usage
        if ($content -match 'Start-Transcript') {
            $results += [ValidationResult]@{
                Category = 'Cmdlet Enhancements'
                Item = $file.Name
                Status = 'Info'
                Message = "Start-Transcript now supports PSObject wrapped strings"
                Recommendation = "Can use PSObject wrapped strings for transcript paths"
                AutoFixAvailable = 'Manual'
                Metadata = @{ FilePath = $file.FullName; Enhancement = 'Start-Transcript-PSObject' }
            }
        }
    }

    return $results
}

function Test-EngineImprovements {
    param([System.IO.FileInfo[]]$Files)

    $results = @()

    foreach ($file in $Files) {
        $content = Get-Content -Path $file.FullName -Raw -ErrorAction SilentlyContinue
        if (-not $content) { continue }

        # Check for cmdlet implementations that could use PipelineStopToken
        if ($content -match 'class\s+\w+\s*:\s*Cmdlet|extends\s+Cmdlet|\[Cmdlet\]') {
            $results += [ValidationResult]@{
                Category = 'Engine Improvements'
                Item = $file.Name
                Status = 'Info'
                Message = "Cmdlet implementation can use new PipelineStopToken"
                Recommendation = "Consider using PipelineStopToken for pipeline cancellation handling"
                AutoFixAvailable = 'Manual'
                Metadata = @{ FilePath = $file.FullName; Improvement = 'PipelineStopToken' }
            }
        }

        # Check for certificate usage that could benefit from DnsNameList improvements
        if ($content -match 'X509Certificate2.*DnsNameList') {
            $results += [ValidationResult]@{
                Category = 'Engine Improvements'
                Item = $file.Name
                Status = 'Pass'
                Message = "X509Certificate2 DnsNameList benefits from PowerShell 7.6 improvements"
                Recommendation = "No action needed - automatically benefits from improvements"
                AutoFixAvailable = 'No'
                Metadata = @{ FilePath = $file.FullName; Improvement = 'X509Certificate2-DnsNameList' }
            }
        }

        # Check for enum usage that could benefit from EnumSingleTypeConverter helper
        if ($content -match '\[enum\]|\[System\.Enum\]|\.GetEnumNames\(\)') {
            $results += [ValidationResult]@{
                Category = 'Engine Improvements'
                Item = $file.Name
                Status = 'Info'
                Message = "Enum usage can benefit from new EnumSingleTypeConverter helper"
                Recommendation = "Consider using EnumSingleTypeConverter for enum operations"
                AutoFixAvailable = 'Manual'
                Metadata = @{ FilePath = $file.FullName; Improvement = 'EnumSingleTypeConverter' }
            }
        }
    }

    return $results
}

function Test-ExperimentalFeatures {
    param([System.IO.FileInfo[]]$Files)

    $results = @()

    foreach ($file in $Files) {
        $content = Get-Content -Path $file.FullName -Raw -ErrorAction SilentlyContinue
        if (-not $content) { continue }

        # Check for tilde usage that could benefit from PSNativeWindowsTildeExpansion
        if ($content -match '~[/\\]') {
            $results += [ValidationResult]@{
                Category = 'Experimental Features'
                Item = $file.Name
                Status = 'Info'
                Message = "Tilde path usage detected - can benefit from PSNativeWindowsTildeExpansion"
                Recommendation = "Enable PSNativeWindowsTildeExpansion experimental feature"
                AutoFixAvailable = 'Manual'
                Metadata = @{ FilePath = $file.FullName; Feature = 'PSNativeWindowsTildeExpansion' }
            }
        }

        # Check for redirection that could benefit from PSRedirectToVariable
        if ($content -match '2>&1|>\s*\$\w+') {
            $results += [ValidationResult]@{
                Category = 'Experimental Features'
                Item = $file.Name
                Status = 'Info'
                Message = "Output redirection detected - can benefit from PSRedirectToVariable"
                Recommendation = "Consider PSRedirectToVariable experimental feature for variable redirection"
                AutoFixAvailable = 'Manual'
                Metadata = @{ FilePath = $file.FullName; Feature = 'PSRedirectToVariable' }
            }
        }

        # Check for JSON serialization with enums
        if ($content -match 'ConvertTo-Json.*enum') {
            $results += [ValidationResult]@{
                Category = 'Experimental Features'
                Item = $file.Name
                Status = 'Info'
                Message = "JSON enum serialization detected - can benefit from PSSerializeJSONLongEnumAsNumber"
                Recommendation = "Consider PSSerializeJSONLongEnumAsNumber for large enum handling"
                AutoFixAvailable = 'Manual'
                Metadata = @{ FilePath = $file.FullName; Feature = 'PSSerializeJSONLongEnumAsNumber' }
            }
        }
    }

    return $results
}

function Test-PerformanceOptimizations {
    param([System.IO.FileInfo[]]$Files)

    $results = @()

    foreach ($file in $Files) {
        $content = Get-Content -Path $file.FullName -Raw -ErrorAction SilentlyContinue
        if (-not $content) { continue }

        # Check for ForEach-Object -Parallel opportunities
        if ($content -match 'ForEach-Object\s+(?!.*-Parallel)' -and $content -match '\$_') {
            $results += [ValidationResult]@{
                Category = 'Performance'
                Item = $file.Name
                Status = 'Info'
                Message = "ForEach-Object usage could benefit from -Parallel parameter"
                Recommendation = "Consider using ForEach-Object -Parallel for parallel processing"
                AutoFixAvailable = 'Manual'
                Metadata = @{ FilePath = $file.FullName; Optimization = 'Parallel-ForEach' }
            }
        }

        # Check for string operations that could use SearchValues<char>
        if ($content -match '\.IndexOfAny\(|\.Contains\(.*char\[\]') {
            $results += [ValidationResult]@{
                Category = 'Performance'
                Item = $file.Name
                Status = 'Info'
                Message = "String operations can benefit from SearchValues<char> optimizations"
                Recommendation = "PowerShell 7.6 automatically optimizes these operations"
                AutoFixAvailable = 'No'
                Metadata = @{ FilePath = $file.FullName; Optimization = 'SearchValues' }
            }
        }
    }

    return $results
}

#endregion

#region Migration Functions

function Start-PowerShell76Migration {
    [CmdletBinding()]
    param(
        [string]$Path = $ProjectPath,
        [switch]$CreateBackup = $true,
        [switch]$InteractiveMode = $false
    )

    Write-Host "🚀 PowerShell 7.6 Migration Assistant" -ForegroundColor Cyan
    Write-Host "=====================================" -ForegroundColor Cyan

    $validationResults = Test-PowerShell76Compatibility -Path $Path
    $migrationActions = @()

    foreach ($result in $validationResults | Where-Object { $_.AutoFixAvailable -eq 'Yes' }) {
        $action = Convert-ValidationToMigrationAction -ValidationResult $result
        if ($action) {
            $migrationActions += $action
        }
    }

    if ($migrationActions.Count -eq 0) {
        Write-Host "✅ No automatic migrations available." -ForegroundColor Green
        return
    }

    Write-Host "`n📋 Migration Actions Required:" -ForegroundColor Yellow
    $migrationActions | Group-Object Category | ForEach-Object {
        Write-Host "   $($_.Name): $($_.Count) action(s)" -ForegroundColor Gray
    }

    if ($InteractiveMode) {
        $confirm = Read-Host "`nProceed with migration? (y/N)"
        if ($confirm -notmatch '^[Yy]') {
            Write-Host "Migration cancelled." -ForegroundColor Yellow
            return
        }
    }

    if ($CreateBackup) {
        $backupPath = New-MigrationBackup -Path $Path
        Write-Host "📦 Backup created: $backupPath" -ForegroundColor Green
    }

    # Execute migration actions
    $successCount = 0
    foreach ($action in $migrationActions) {
        try {
            Invoke-MigrationAction -Action $action
            $successCount++
            Write-Host "✅ $($action.Type): $($action.FilePath)" -ForegroundColor Green
        }
        catch {
            Write-Host "❌ Failed: $($action.FilePath) - $($_.Exception.Message)" -ForegroundColor Red
        }
    }

    Write-Host "`n🎉 Migration completed: $successCount/$($migrationActions.Count) actions successful" -ForegroundColor Cyan
}

function Convert-ValidationToMigrationAction {
    param([ValidationResult]$ValidationResult)

    switch ($ValidationResult.Category) {
        'Version Requirements' {
            if ($ValidationResult.Metadata.ContainsKey('CurrentVersion')) {
                return [MigrationAction]@{
                    Type = 'Replace'
                    FilePath = $ValidationResult.Metadata.FilePath
                    LineNumber = 1
                    OldContent = "#Requires -Version $($ValidationResult.Metadata.CurrentVersion)"
                    NewContent = "#Requires -Version 7.6"
                    Reason = "Update PowerShell version requirement to 7.6"
                    Category = 'Version Requirements'
                }
            } else {
                return [MigrationAction]@{
                    Type = 'Add'
                    FilePath = $ValidationResult.Metadata.FilePath
                    LineNumber = 1
                    OldContent = ''
                    NewContent = "#Requires -Version 7.6`n"
                    Reason = "Add PowerShell 7.6 version requirement"
                    Category = 'Version Requirements'
                }
            }
        }

        'Module Dependencies' {
            if ($ValidationResult.Metadata.ContainsKey('OldModule')) {
                return [MigrationAction]@{
                    Type = 'Replace'
                    FilePath = $ValidationResult.Metadata.FilePath
                    LineNumber = 0
                    OldContent = $ValidationResult.Metadata.OldModule
                    NewContent = $ValidationResult.Metadata.NewModule
                    Reason = "Update to new module name"
                    Category = 'Module Dependencies'
                }
            }
        }
    }

    return $null
}

function Invoke-MigrationAction {
    param([MigrationAction]$Action)

    switch ($Action.Type) {
        'Replace' {
            $content = Get-Content -Path $Action.FilePath -Raw
            $newContent = $content -replace [regex]::Escape($Action.OldContent), $Action.NewContent
            Set-Content -Path $Action.FilePath -Value $newContent -NoNewline
        }

        'Add' {
            $content = Get-Content -Path $Action.FilePath -Raw -ErrorAction SilentlyContinue
            $newContent = $Action.NewContent + $content
            Set-Content -Path $Action.FilePath -Value $newContent -NoNewline
        }

        'Remove' {
            $content = Get-Content -Path $Action.FilePath -Raw
            $newContent = $content -replace [regex]::Escape($Action.OldContent), ''
            Set-Content -Path $Action.FilePath -Value $newContent -NoNewline
        }
    }
}

function New-MigrationBackup {
    param([string]$Path)

    $timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
    $backupPath = Join-Path (Split-Path $Path) "backup-ps76-migration-$timestamp"

    Copy-Item -Path $Path -Destination $backupPath -Recurse -Force
    return $backupPath
}

#endregion

#region Debug and Analysis Functions

function Start-PowerShell76Debugging {
    [CmdletBinding()]
    param(
        [string]$Path = $ProjectPath,
        [switch]$IncludePerformanceAnalysis,
        [switch]$VerboseOutput
    )

    Write-Host "🔧 PowerShell 7.6 Debugging Assistant" -ForegroundColor Cyan
    Write-Host "=====================================" -ForegroundColor Cyan

    # Comprehensive system analysis
    $systemInfo = Get-PowerShell76SystemInfo
    Write-Host "`n💻 System Information:" -ForegroundColor Yellow
    $systemInfo | Format-Table -AutoSize

    # Validation results
    $validationResults = Test-PowerShell76Compatibility -Path $Path -IncludePerformanceChecks:$IncludePerformanceAnalysis

    # Categorize results
    $categories = $validationResults | Group-Object Category

    Write-Host "`n📊 Analysis Results:" -ForegroundColor Yellow
    foreach ($category in $categories) {
        Write-Host "   $($category.Name):" -ForegroundColor Cyan
        $statusCounts = $category.Group | Group-Object Status
        foreach ($status in $statusCounts) {
            $color = switch ($status.Name) {
                'Pass' { 'Green' }
                'Fail' { 'Red' }
                'Warning' { 'Yellow' }
                'Info' { 'Gray' }
                default { 'White' }
            }
            Write-Host "     $($status.Name): $($status.Count)" -ForegroundColor $color
        }
    }

    # Show critical issues
    $criticalIssues = $validationResults | Where-Object { $_.Status -eq 'Fail' }
    if ($criticalIssues) {
        Write-Host "`n🚨 Critical Issues Requiring Attention:" -ForegroundColor Red
        foreach ($issue in $criticalIssues) {
            Write-Host "   ❌ $($issue.Item): $($issue.Message)" -ForegroundColor Red
            if ($issue.Recommendation) {
                Write-Host "      💡 $($issue.Recommendation)" -ForegroundColor Yellow
            }
        }
    }

    # Show optimization opportunities
    $optimizations = $validationResults | Where-Object { $_.Category -eq 'Performance' -or $_.Category -eq 'Tab Completion' }
    if ($optimizations) {
        Write-Host "`n⚡ Optimization Opportunities:" -ForegroundColor Cyan
        foreach ($opt in $optimizations | Select-Object -First 5) {
            Write-Host "   🔧 $($opt.Item): $($opt.Message)" -ForegroundColor Cyan
            if ($opt.Recommendation) {
                Write-Host "      💡 $($opt.Recommendation)" -ForegroundColor Gray
            }
        }
    }

    if ($VerboseOutput) {
        Write-Host "`n📋 Detailed Results:" -ForegroundColor Yellow
        $validationResults | Format-Table Category, Item, Status, Message -Wrap
    }

    return $validationResults
}

function Get-PowerShell76SystemInfo {
    $features = [PowerShell76Features]::new()

    return [PSCustomObject]@{
        PowerShellVersion = $PSVersionTable.PSVersion
        PSEdition = $PSVersionTable.PSEdition
        OS = $PSVersionTable.OS
        Platform = $PSVersionTable.Platform
        DotNetVersion = [System.Runtime.InteropServices.RuntimeInformation]::FrameworkDescription
        ProcessorCount = [System.Environment]::ProcessorCount
        MaxParallelism = [System.Environment]::ProcessorCount
        HyperthreadingDetected = [System.Environment]::ProcessorCount -gt (Get-CimInstance Win32_Processor | Measure-Object NumberOfCores -Sum).Sum
        ModuleVersions = @{
            PSResourceGet = (Get-Module Microsoft.PowerShell.PSResourceGet -ListAvailable | Select-Object -First 1).Version
            PSReadLine = (Get-Module PSReadLine -ListAvailable | Select-Object -First 1).Version
            ThreadJob = (Get-Module Microsoft.PowerShell.ThreadJob -ListAvailable | Select-Object -First 1).Version
        }
        ExperimentalFeatures = Get-ExperimentalFeature | Where-Object Enabled | Select-Object -ExpandProperty Name
    }
}

#endregion

#region Report Generation

function New-PowerShell76Report {
    [CmdletBinding()]
    param(
        [ValidationResult[]]$ValidationResults,
        [string]$OutputPath = "PowerShell76-Report-$(Get-Date -Format 'yyyyMMdd-HHmmss').html"
    )

    $systemInfo = Get-PowerShell76SystemInfo
    $features = [PowerShell76Features]::new()

    $html = @"
<!DOCTYPE html>
<html>
<head>
    <title>PowerShell 7.6 Compatibility Report - BusBuddy</title>
    <style>
        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 20px; background-color: #f5f5f5; }
        .container { max-width: 1200px; margin: 0 auto; background-color: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
        h1 { color: #0078d4; border-bottom: 2px solid #0078d4; padding-bottom: 10px; }
        h2 { color: #323130; margin-top: 30px; }
        .status-pass { color: #107c10; font-weight: bold; }
        .status-fail { color: #d13438; font-weight: bold; }
        .status-warning { color: #ff8c00; font-weight: bold; }
        .status-info { color: #0078d4; font-weight: bold; }
        table { width: 100%; border-collapse: collapse; margin: 10px 0; }
        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
        th { background-color: #f8f9fa; }
        .summary-box { background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 10px 0; }
        .feature-list { list-style-type: none; padding: 0; }
        .feature-list li { background-color: #e8f4f8; margin: 5px 0; padding: 8px; border-radius: 3px; }
    </style>
</head>
<body>
    <div class="container">
        <h1>🚀 PowerShell 7.6 Compatibility Report - BusBuddy</h1>
        <p><strong>Generated:</strong> $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')</p>

        <h2>📊 Summary</h2>
        <div class="summary-box">
            <p><strong>Total Items Analyzed:</strong> $($ValidationResults.Count)</p>
            <p><strong>Status Breakdown:</strong></p>
            <ul>
"@

    $statusCounts = $ValidationResults | Group-Object Status
    foreach ($status in $statusCounts) {
        $cssClass = "status-$($status.Name.ToLower())"
        $html += "                <li class=`"$cssClass`">$($status.Name): $($status.Count)</li>`n"
    }

    $html += @"
            </ul>
        </div>

        <h2>💻 System Information</h2>
        <table>
            <tr><th>Property</th><th>Value</th></tr>
            <tr><td>PowerShell Version</td><td>$($systemInfo.PowerShellVersion)</td></tr>
            <tr><td>PS Edition</td><td>$($systemInfo.PSEdition)</td></tr>
            <tr><td>.NET Version</td><td>$($systemInfo.DotNetVersion)</td></tr>
            <tr><td>Processor Count</td><td>$($systemInfo.ProcessorCount)</td></tr>
            <tr><td>Hyperthreading</td><td>$($systemInfo.HyperthreadingDetected)</td></tr>
        </table>

        <h2>📋 Detailed Results</h2>
        <table>
            <tr><th>Category</th><th>Item</th><th>Status</th><th>Message</th><th>Recommendation</th></tr>
"@

    foreach ($result in $ValidationResults) {
        $statusClass = "status-$($result.Status.ToLower())"
        $html += "            <tr><td>$($result.Category)</td><td>$($result.Item)</td><td class=`"$statusClass`">$($result.Status)</td><td>$($result.Message)</td><td>$($result.Recommendation)</td></tr>`n"
    }

    $html += @"
        </table>

        <h2>🆕 PowerShell 7.6 New Features</h2>
        <div class="summary-box">
            <h3>Updated Modules</h3>
            <ul class="feature-list">
"@

    foreach ($module in $features.ModuleUpdates.Keys) {
        $version = $features.ModuleUpdates[$module]
        $html += "                <li>$module v$version</li>`n"
    }

    $html += @"
            </ul>

            <h3>Breaking Changes</h3>
            <ul class="feature-list">
"@

    foreach ($change in $features.BreakingChanges) {
        $html += "                <li>$change</li>`n"
    }

    $html += @"
            </ul>

            <h3>Experimental Features</h3>
            <ul class="feature-list">
"@

    foreach ($feature in $features.ExperimentalFeatures) {
        $html += "                <li>$feature</li>`n"
    }

    $html += @"
            </ul>
        </div>

        <h2>🔗 Resources</h2>
        <ul>
            <li><a href="https://learn.microsoft.com/en-us/powershell/scripting/whats-new/what-s-new-in-powershell-76">What's New in PowerShell 7.6</a></li>
            <li><a href="https://github.com/PowerShell/PowerShell/blob/master/CHANGELOG/preview.md">PowerShell Changelog</a></li>
        </ul>

        <p><em>Report generated by PowerShell 7.6 Validator, Migrator, and Debugger</em></p>
    </div>
</body>
</html>
"@

    Set-Content -Path $OutputPath -Value $html -Encoding UTF8
    Write-Host "📄 Report generated: $OutputPath" -ForegroundColor Green

    return $OutputPath
}

#endregion

#region Main Execution Logic

function Invoke-PowerShell76Analysis {
    param(
        [string]$Mode,
        [string]$Path = $ProjectPath
    )

    switch ($Mode) {
        'Validate' {
            $results = Test-PowerShell76Compatibility -Path $Path -IncludePerformanceChecks

            if ($GenerateReport) {
                $reportPath = New-PowerShell76Report -ValidationResults $results
                Write-Host "📊 Report generated: $reportPath" -ForegroundColor Cyan
            }

            return $results
        }

        'Migrate' {
            Start-PowerShell76Migration -Path $Path -InteractiveMode:(!$AutoFix)
        }

        'Debug' {
            Start-PowerShell76Debugging -Path $Path -IncludePerformanceAnalysis -VerboseOutput:(!$Quiet)
        }

        'All' {
            Write-Host "🔄 Running Complete PowerShell 7.6 Analysis..." -ForegroundColor Cyan

            # Validation
            $validationResults = Test-PowerShell76Compatibility -Path $Path -IncludePerformanceChecks

            # Debug Analysis
            Start-PowerShell76Debugging -Path $Path -IncludePerformanceAnalysis

            # Migration (if auto-fix enabled)
            if ($AutoFix) {
                Start-PowerShell76Migration -Path $Path -InteractiveMode:$false
            }

            # Generate Report
            if ($GenerateReport) {
                $reportPath = New-PowerShell76Report -ValidationResults $validationResults
                Write-Host "📊 Complete analysis report: $reportPath" -ForegroundColor Cyan
            }

            return $validationResults
        }
    }
}

# Execute based on parameters
if ($ValidateProject) {
    Invoke-PowerShell76Analysis -Mode 'Validate'
}
elseif ($MigrateToPS76) {
    Invoke-PowerShell76Analysis -Mode 'Migrate'
}
elseif ($DebugIssues) {
    Invoke-PowerShell76Analysis -Mode 'Debug'
}
elseif ($RunAll) {
    Invoke-PowerShell76Analysis -Mode 'All'
}
else {
    Write-Host "🔧 PowerShell 7.6 Validator, Migrator, and Debugger" -ForegroundColor Cyan
    Write-Host "Usage:"
    Write-Host "  -ValidateProject     Run compatibility validation"
    Write-Host "  -MigrateToPS76       Migrate scripts to PowerShell 7.6"
    Write-Host "  -DebugIssues         Debug and analyze issues"
    Write-Host "  -RunAll              Run all operations"
    Write-Host ""
    Write-Host "Options:"
    Write-Host "  -GenerateReport      Generate HTML report"
    Write-Host "  -AutoFix             Automatically apply fixes"
    Write-Host "  -Quiet               Suppress verbose output"
    Write-Host ""
    Write-Host "Examples:"
    Write-Host "  .\PowerShell-76-Validator-Migrator-Debugger.ps1 -ValidateProject -GenerateReport"
    Write-Host "  .\PowerShell-76-Validator-Migrator-Debugger.ps1 -RunAll -AutoFix"
}

#endregion
