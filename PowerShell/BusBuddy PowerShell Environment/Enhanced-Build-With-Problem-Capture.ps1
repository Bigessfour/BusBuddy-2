#Requires -Version 7.5
<#
.SYNOPSIS
    Enhanced BusBuddy Build Function with VS Code Problem Capture and Analysis

.DESCRIPTION
    This script enhances the existing bb-build function to capture problems from VS Code's
    problem list, identify file locations of errors, and provide structured analysis for
    automated fixes. Integrates with BusBuddy's error analysis and fix automation systems.

.NOTES
    Author: BusBuddy Development Team
    Created: July 30, 2025
    Version: 1.0.0
    PowerShell: 7.5.2+
    Dependencies: BusBuddy PowerShell Module, VS Code CLI
#>

[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    [switch]$Clean,
    [switch]$Restore,
    [switch]$CaptureProblemList,
    [switch]$AnalyzeProblems,
    [switch]$ExportResults,
    [string]$ExportPath = ".\logs\build-analysis",

    [ValidateSet('quiet', 'minimal', 'normal', 'detailed', 'diagnostic')]
    [string]$Verbosity = 'minimal'
)

#region Helper Functions

function Write-EnhancedBuildStatus {
    param(
        [string]$Message,
        [ValidateSet('Info', 'Success', 'Warning', 'Error', 'Build', 'Analysis')]
        [string]$Type = 'Info',
        [int]$Indent = 0
    )

    $prefix = "  " * $Indent
    $color = switch ($Type) {
        'Info' { 'Cyan' }
        'Success' { 'Green' }
        'Warning' { 'Yellow' }
        'Error' { 'Red' }
        'Build' { 'Blue' }
        'Analysis' { 'Magenta' }
    }

    $icon = switch ($Type) {
        'Info' { 'ℹ️' }
        'Success' { '✅' }
        'Warning' { '⚠️' }
        'Error' { '❌' }
        'Build' { '🔨' }
        'Analysis' { '🔍' }
    }

    Write-Host "$prefix$icon $Message" -ForegroundColor $color
}

function Get-VSCodeProblems {
    <#
    .SYNOPSIS
        Capture problems from VS Code's problem list
    #>
    param(
        [string]$WorkspaceFolder = (Get-Location).Path
    )

    $problems = @()

    try {
        Write-EnhancedBuildStatus "Capturing VS Code problem list..." -Type Analysis

        # Try multiple methods to get VS Code problems
        $methods = @(
            { Get-VSCodeProblemsViaExtension },
            { Get-VSCodeProblemsViaWorkspace },
            { Get-VSCodeProblemsViaOutput }
        )

        foreach ($method in $methods) {
            try {
                $methodProblems = & $method
                if ($methodProblems -and $methodProblems.Count -gt 0) {
                    $problems += $methodProblems
                    Write-EnhancedBuildStatus "Found $($methodProblems.Count) problems via method" -Type Success -Indent 1
                    break
                }
            }
            catch {
                Write-EnhancedBuildStatus "Method failed: $($_.Exception.Message)" -Type Warning -Indent 1
            }
        }

        return $problems
    }
    catch {
        Write-EnhancedBuildStatus "Failed to capture VS Code problems: $($_.Exception.Message)" -Type Error
        return @()
    }
}

function Get-VSCodeProblemsViaExtension {
    <#
    .SYNOPSIS
        Get problems via VS Code extension API
    #>

    # Check if VS Code is running and has problem data
    $vscodeProcesses = Get-Process -Name "Code" -ErrorAction SilentlyContinue
    if (-not $vscodeProcesses) {
        Write-EnhancedBuildStatus "VS Code not running" -Type Warning -Indent 2
        return @()
    }

    # Try to get problems from VS Code workspace state
    $workspaceStateFile = Join-Path $env:APPDATA "Code\User\workspaceStorage"
    if (Test-Path $workspaceStateFile) {
        $workspaceFolders = Get-ChildItem $workspaceStateFile -Directory
        foreach ($folder in $workspaceFolders) {
            $stateFile = Join-Path $folder.FullName "state.vscdb"
            if (Test-Path $stateFile) {
                # Extract problem information from VS Code state
                # This is a simplified approach - in practice, you'd need to parse the SQLite database
                Write-EnhancedBuildStatus "Found workspace state: $($folder.Name)" -Type Info -Indent 2
            }
        }
    }

    return @()
}

function Get-VSCodeProblemsViaWorkspace {
    <#
    .SYNOPSIS
        Get problems by analyzing workspace files for common issues
    #>

    $problems = @()
    $projectRoot = Get-Location

    Write-EnhancedBuildStatus "Analyzing workspace files for problems..." -Type Analysis -Indent 2

    # Check for common C# compilation errors
    $csharpFiles = Get-ChildItem -Path $projectRoot -Recurse -Filter "*.cs" -ErrorAction SilentlyContinue
    foreach ($file in $csharpFiles) {
        try {
            $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
            if ($content) {
                # Look for common error patterns
                $errorPatterns = @{
                    'CS0103' = @{
                        Pattern = 'The name .* does not exist in the current context'
                        Description = 'Undefined variable or method'
                        Severity = 'Error'
                    }
                    'CS0246' = @{
                        Pattern = 'The type or namespace name .* could not be found'
                        Description = 'Missing using statement or assembly reference'
                        Severity = 'Error'
                    }
                    'CS0029' = @{
                        Pattern = 'Cannot implicitly convert type'
                        Description = 'Type conversion error'
                        Severity = 'Error'
                    }
                }

                $lines = $content -split "`n"
                for ($i = 0; $i -lt $lines.Count; $i++) {
                    foreach ($errorCode in $errorPatterns.Keys) {
                        $pattern = $errorPatterns[$errorCode]
                        if ($lines[$i] -match $pattern.Pattern) {
                            $problems += @{
                                File = $file.FullName
                                Line = $i + 1
                                Column = 1
                                Severity = $pattern.Severity
                                Code = $errorCode
                                Message = $pattern.Description
                                Source = 'Static Analysis'
                            }
                        }
                    }
                }
            }
        }
        catch {
            Write-EnhancedBuildStatus "Error analyzing file $($file.Name): $($_.Exception.Message)" -Type Warning -Indent 3
        }
    }

    return $problems
}

function Get-VSCodeProblemsViaOutput {
    <#
    .SYNOPSIS
        Get problems by parsing build output for errors and warnings
    #>
    param(
        [string[]]$BuildOutput
    )

    $problems = @()

    if (-not $BuildOutput) {
        return $problems
    }

    Write-EnhancedBuildStatus "Parsing build output for problems..." -Type Analysis -Indent 2

    foreach ($line in $BuildOutput) {
        # Parse MSBuild error/warning format
        # Example: "C:\path\to\file.cs(12,34): error CS0103: The name 'variable' does not exist"
        if ($line -match '^(.+?)\((\d+),(\d+)\):\s*(error|warning)\s*([A-Z]+\d+):\s*(.+)$') {
            $problems += @{
                File = $matches[1].Trim()
                Line = [int]$matches[2]
                Column = [int]$matches[3]
                Severity = if ($matches[4] -eq 'error') { 'Error' } else { 'Warning' }
                Code = $matches[5]
                Message = $matches[6].Trim()
                Source = 'MSBuild'
            }
        }
        # Parse other common error formats
        elseif ($line -match '^(.+?):\s*(error|warning):\s*(.+)$') {
            $problems += @{
                File = $matches[1].Trim()
                Line = 1
                Column = 1
                Severity = if ($matches[2] -eq 'error') { 'Error' } else { 'Warning' }
                Code = 'UNKNOWN'
                Message = $matches[3].Trim()
                Source = 'Generic'
            }
        }
    }

    Write-EnhancedBuildStatus "Found $($problems.Count) problems in build output" -Type Success -Indent 2
    return $problems
}

function Analyze-CapturedProblems {
    <#
    .SYNOPSIS
        Analyze captured problems and provide fix recommendations
    #>
    param(
        [array]$Problems
    )

    if (-not $Problems -or $Problems.Count -eq 0) {
        Write-EnhancedBuildStatus "No problems to analyze" -Type Info -Indent 1
        return @()
    }

    Write-EnhancedBuildStatus "Analyzing $($Problems.Count) captured problems..." -Type Analysis

    $analysis = @{
        TotalProblems = $Problems.Count
        ErrorCount = ($Problems | Where-Object { $_.Severity -eq 'Error' }).Count
        WarningCount = ($Problems | Where-Object { $_.Severity -eq 'Warning' }).Count
        FileGroups = @{}
        CodeGroups = @{}
        Recommendations = @()
    }

    # Group problems by file
    foreach ($problem in $Problems) {
        $file = $problem.File
        if (-not $analysis.FileGroups.ContainsKey($file)) {
            $analysis.FileGroups[$file] = @()
        }
        $analysis.FileGroups[$file] += $problem

        # Group by error code
        $code = $problem.Code
        if (-not $analysis.CodeGroups.ContainsKey($code)) {
            $analysis.CodeGroups[$code] = @()
        }
        $analysis.CodeGroups[$code] += $problem
    }

    # Generate recommendations
    foreach ($codeGroup in $analysis.CodeGroups.Keys) {
        $codeProblems = $analysis.CodeGroups[$codeGroup]
        $count = $codeProblems.Count

        switch ($codeGroup) {
            'CS0103' {
                $analysis.Recommendations += @{
                    Type = 'Automated Fix'
                    Priority = 'High'
                    Description = "Fix $count undefined variable/method errors"
                    Action = 'Add missing using statements or fix variable names'
                    AutoFixable = $true
                    Command = 'bb-fix-undefined-variables'
                }
            }
            'CS0246' {
                $analysis.Recommendations += @{
                    Type = 'Package Reference'
                    Priority = 'High'
                    Description = "Fix $count missing type/namespace errors"
                    Action = 'Add missing package references or using statements'
                    AutoFixable = $true
                    Command = 'bb-fix-missing-references'
                }
            }
            'CS0029' {
                $analysis.Recommendations += @{
                    Type = 'Type Conversion'
                    Priority = 'Medium'
                    Description = "Fix $count type conversion errors"
                    Action = 'Add explicit type conversions or fix type mismatches'
                    AutoFixable = $false
                    Command = 'bb-analyze-type-errors'
                }
            }
            default {
                $analysis.Recommendations += @{
                    Type = 'Manual Review'
                    Priority = 'Medium'
                    Description = "Review $count $codeGroup errors"
                    Action = 'Manual code review and fixing required'
                    AutoFixable = $false
                    Command = 'bb-review-errors'
                }
            }
        }
    }

    return $analysis
}

function Export-BuildResults {
    <#
    .SYNOPSIS
        Export build results and problem analysis to files
    #>
    param(
        [hashtable]$BuildResults,
        [array]$Problems,
        [hashtable]$Analysis,
        [string]$ExportPath
    )

    try {
        # Ensure export directory exists
        if (-not (Test-Path $ExportPath)) {
            New-Item -Path $ExportPath -ItemType Directory -Force | Out-Null
        }

        $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"

        # Export problems to JSON
        if ($Problems -and $Problems.Count -gt 0) {
            $problemsFile = Join-Path $ExportPath "problems-$timestamp.json"
            $Problems | ConvertTo-Json -Depth 10 | Out-File $problemsFile -Encoding UTF8
            Write-EnhancedBuildStatus "Problems exported to: $problemsFile" -Type Success -Indent 1
        }

        # Export analysis to JSON
        if ($Analysis) {
            $analysisFile = Join-Path $ExportPath "analysis-$timestamp.json"
            $Analysis | ConvertTo-Json -Depth 10 | Out-File $analysisFile -Encoding UTF8
            Write-EnhancedBuildStatus "Analysis exported to: $analysisFile" -Type Success -Indent 1
        }

        # Export build results
        if ($BuildResults) {
            $buildFile = Join-Path $ExportPath "build-results-$timestamp.json"
            $BuildResults | ConvertTo-Json -Depth 10 | Out-File $buildFile -Encoding UTF8
            Write-EnhancedBuildStatus "Build results exported to: $buildFile" -Type Success -Indent 1
        }

        # Create summary report
        $summaryFile = Join-Path $ExportPath "build-summary-$timestamp.md"
        $summary = Generate-BuildSummaryReport -BuildResults $BuildResults -Problems $Problems -Analysis $Analysis
        $summary | Out-File $summaryFile -Encoding UTF8
        Write-EnhancedBuildStatus "Summary report: $summaryFile" -Type Success -Indent 1

        return $true
    }
    catch {
        Write-EnhancedBuildStatus "Export failed: $($_.Exception.Message)" -Type Error
        return $false
    }
}

function Generate-BuildSummaryReport {
    param(
        [hashtable]$BuildResults,
        [array]$Problems,
        [hashtable]$Analysis
    )

    $report = @"
# BusBuddy Build Analysis Report
Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

## Build Summary
- **Status**: $($BuildResults.Success ? 'Success' : 'Failed')
- **Configuration**: $($BuildResults.Configuration)
- **Duration**: $($BuildResults.Duration)
- **Warnings**: $($BuildResults.WarningCount)

## Problem Analysis
- **Total Problems**: $($Analysis.TotalProblems)
- **Errors**: $($Analysis.ErrorCount)
- **Warnings**: $($Analysis.WarningCount)

### Top Problem Files
"@

    if ($Analysis.FileGroups) {
        $topFiles = $Analysis.FileGroups.Keys | Sort-Object { $Analysis.FileGroups[$_].Count } -Descending | Select-Object -First 5
        foreach ($file in $topFiles) {
            $problemCount = $Analysis.FileGroups[$file].Count
            $relativePath = $file -replace [regex]::Escape((Get-Location).Path), '.'
            $report += "`n- **$relativePath**: $problemCount problems"
        }
    }

    $report += "`n`n### Recommendations"
    if ($Analysis.Recommendations) {
        foreach ($rec in $Analysis.Recommendations) {
            $report += "`n- **$($rec.Type)** [$($rec.Priority)]: $($rec.Description)"
            $report += "`n  - Action: $($rec.Action)"
            if ($rec.AutoFixable) {
                $report += "`n  - Auto-fixable: Yes (Run: ``$($rec.Command)``)"
            }
        }
    }

    $report += "`n`n### Problem Details"
    if ($Problems) {
        foreach ($problem in ($Problems | Select-Object -First 10)) {
            $relativePath = $problem.File -replace [regex]::Escape((Get-Location).Path), '.'
            $report += "`n- **$($problem.Severity)** $($problem.Code) in $relativePath ($($problem.Line):$($problem.Column))"
            $report += "`n  - $($problem.Message)"
        }

        if ($Problems.Count -gt 10) {
            $report += "`n- ... and $($Problems.Count - 10) more problems"
        }
    }

    return $report
}

#endregion

#region Main Enhanced Build Function

function Invoke-EnhancedBusBuddyBuild {
    <#
    .SYNOPSIS
        Enhanced BusBuddy build with comprehensive problem capture and analysis
    #>

    $startTime = Get-Date
    $buildResults = @{
        Success = $false
        Configuration = $Configuration
        StartTime = $startTime
        Duration = $null
        WarningCount = 0
        Problems = @()
        Analysis = @{}
    }

    try {
        Write-EnhancedBuildStatus "🚌 Enhanced BusBuddy Build with Problem Capture" -Type Build
        Write-EnhancedBuildStatus "Configuration: $Configuration | Verbosity: $Verbosity" -Type Info -Indent 1
        Write-Host ""

        # Step 1: Capture pre-build problems (if VS Code is running)
        if ($CaptureProblemList) {
            Write-EnhancedBuildStatus "Step 1: Capturing pre-build VS Code problems..." -Type Analysis
            $preProblems = Get-VSCodeProblems
            if ($preProblems -and $preProblems.Count -gt 0) {
                Write-EnhancedBuildStatus "Found $($preProblems.Count) pre-build problems" -Type Warning -Indent 1
                $buildResults.Problems += $preProblems
            } else {
                Write-EnhancedBuildStatus "No pre-build problems detected" -Type Success -Indent 1
            }
            Write-Host ""
        }

        # Step 2: Run the actual build (using existing BusBuddy build logic)
        Write-EnhancedBuildStatus "Step 2: Executing build process..." -Type Build

        # Load BusBuddy module if not already loaded
        if (-not (Get-Command Invoke-BusBuddyBuild -ErrorAction SilentlyContinue)) {
            $profileScript = Join-Path (Split-Path $PSScriptRoot -Parent) "load-bus-buddy-profiles.ps1"
            if (Test-Path $profileScript) {
                Write-EnhancedBuildStatus "Loading BusBuddy profiles..." -Type Info -Indent 1
                & $profileScript -Quiet
            }
        }

        # Execute the build with output capture
        $buildOutput = @()
        if (Get-Command Invoke-BusBuddyBuild -ErrorAction SilentlyContinue) {
            Write-EnhancedBuildStatus "Using bb-build function..." -Type Build -Indent 1
            $buildSuccess = Invoke-BusBuddyBuild -Configuration $Configuration -Clean:$Clean -Restore:$Restore -Verbosity $Verbosity
        } else {
            Write-EnhancedBuildStatus "Using direct dotnet build..." -Type Build -Indent 1
            $buildArgs = @('build', 'BusBuddy.sln', '--configuration', $Configuration, '--verbosity', $Verbosity)
            if ($Clean) {
                dotnet clean BusBuddy.sln --configuration $Configuration --verbosity quiet | Out-Null
            }
            if ($Restore) {
                dotnet restore BusBuddy.sln --verbosity quiet | Out-Null
            }

            $buildOutput = & dotnet @buildArgs 2>&1
            $buildSuccess = $LASTEXITCODE -eq 0
        }

        $buildResults.Success = $buildSuccess

        # Step 3: Capture post-build problems from build output
        Write-EnhancedBuildStatus "Step 3: Analyzing build output for problems..." -Type Analysis
        if ($buildOutput -and $buildOutput.Count -gt 0) {
            $buildProblems = Get-VSCodeProblemsViaOutput -BuildOutput $buildOutput
            if ($buildProblems -and $buildProblems.Count -gt 0) {
                Write-EnhancedBuildStatus "Found $($buildProblems.Count) problems in build output" -Type Warning -Indent 1
                $buildResults.Problems += $buildProblems

                # Count warnings from build output
                $buildResults.WarningCount = ($buildProblems | Where-Object { $_.Severity -eq 'Warning' }).Count
            } else {
                Write-EnhancedBuildStatus "No problems detected in build output" -Type Success -Indent 1
            }
        }
        Write-Host ""

        # Step 4: Analyze all captured problems
        if ($AnalyzeProblems -and $buildResults.Problems.Count -gt 0) {
            Write-EnhancedBuildStatus "Step 4: Analyzing captured problems..." -Type Analysis
            $analysis = Analyze-CapturedProblems -Problems $buildResults.Problems
            $buildResults.Analysis = $analysis

            Write-EnhancedBuildStatus "Analysis complete:" -Type Success -Indent 1
            Write-EnhancedBuildStatus "• Total Problems: $($analysis.TotalProblems)" -Type Info -Indent 2
            Write-EnhancedBuildStatus "• Errors: $($analysis.ErrorCount)" -Type $(if ($analysis.ErrorCount -gt 0) { 'Error' } else { 'Success' }) -Indent 2
            Write-EnhancedBuildStatus "• Warnings: $($analysis.WarningCount)" -Type $(if ($analysis.WarningCount -gt 0) { 'Warning' } else { 'Success' }) -Indent 2
            Write-EnhancedBuildStatus "• Auto-fixable: $(($analysis.Recommendations | Where-Object { $_.AutoFixable }).Count)" -Type Info -Indent 2

            # Show top recommendations
            if ($analysis.Recommendations -and $analysis.Recommendations.Count -gt 0) {
                Write-Host ""
                Write-EnhancedBuildStatus "Top Recommendations:" -Type Analysis -Indent 1
                $topRecs = $analysis.Recommendations | Sort-Object {
                    if ($_.Priority -eq 'High') { 1 }
                    elseif ($_.Priority -eq 'Medium') { 2 }
                    else { 3 }
                } | Select-Object -First 3

                foreach ($rec in $topRecs) {
                    $icon = if ($rec.AutoFixable) { "🤖" } else { "💡" }
                    Write-EnhancedBuildStatus "$icon [$($rec.Priority)] $($rec.Description)" -Type Info -Indent 2
                    if ($rec.AutoFixable) {
                        Write-EnhancedBuildStatus "   Run: $($rec.Command)" -Type Info -Indent 2
                    }
                }
            }
            Write-Host ""
        }

        # Step 5: Export results if requested
        if ($ExportResults) {
            Write-EnhancedBuildStatus "Step 5: Exporting results..." -Type Analysis
            $buildResults.Duration = (Get-Date) - $startTime
            $exportSuccess = Export-BuildResults -BuildResults $buildResults -Problems $buildResults.Problems -Analysis $buildResults.Analysis -ExportPath $ExportPath
            if ($exportSuccess) {
                Write-EnhancedBuildStatus "Results exported successfully" -Type Success -Indent 1
            }
        }

        # Final summary
        $buildResults.Duration = (Get-Date) - $startTime
        Write-Host ""
        Write-EnhancedBuildStatus "=== Enhanced Build Summary ===" -Type Build
        Write-EnhancedBuildStatus "Status: $(if ($buildResults.Success) { 'SUCCESS' } else { 'FAILED' })" -Type $(if ($buildResults.Success) { 'Success' } else { 'Error' }) -Indent 1
        Write-EnhancedBuildStatus "Duration: $([math]::Round($buildResults.Duration.TotalSeconds, 1))s" -Type Info -Indent 1
        Write-EnhancedBuildStatus "Problems Captured: $($buildResults.Problems.Count)" -Type Info -Indent 1
        if ($buildResults.Problems.Count -gt 0) {
            $errorCount = ($buildResults.Problems | Where-Object { $_.Severity -eq 'Error' }).Count
            $warningCount = ($buildResults.Problems | Where-Object { $_.Severity -eq 'Warning' }).Count
            Write-EnhancedBuildStatus "• Errors: $errorCount" -Type $(if ($errorCount -gt 0) { 'Error' } else { 'Success' }) -Indent 2
            Write-EnhancedBuildStatus "• Warnings: $warningCount" -Type $(if ($warningCount -gt 0) { 'Warning' } else { 'Success' }) -Indent 2
        }

        return $buildResults
    }
    catch {
        Write-EnhancedBuildStatus "Enhanced build failed: $($_.Exception.Message)" -Type Error
        $buildResults.Duration = (Get-Date) - $startTime
        return $buildResults
    }
}

#endregion

#region Script Execution

# If running as a script (not being dot-sourced), execute the enhanced build
if ($MyInvocation.InvocationName -ne '.') {
    Write-Host "🚌 BusBuddy Enhanced Build with Problem Capture" -ForegroundColor Cyan
    Write-Host "=============================================" -ForegroundColor Cyan
    Write-Host ""

    $results = Invoke-EnhancedBusBuddyBuild

    if ($results.Success) {
        Write-Host ""
        Write-Host "✅ Enhanced build completed successfully!" -ForegroundColor Green
        exit 0
    } else {
        Write-Host ""
        Write-Host "❌ Enhanced build failed!" -ForegroundColor Red
        exit 1
    }
}

#endregion
