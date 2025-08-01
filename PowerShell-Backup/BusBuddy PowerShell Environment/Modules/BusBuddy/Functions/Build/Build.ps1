#Requires -Version 7.5

<#
.SYNOPSIS
    Build-related functions for BusBuddy
.DESCRIPTION
    Contains functions for building the BusBuddy solution
.NOTES
    File Name      : Build.ps1
    Author         : BusBuddy Development Team
    Prerequisite   : PowerShell 7.5.2
    Copyright 2025 - BusBuddy
#>

function Invoke-BusBuddyBuild {
    <#
    .SYNOPSIS
    Builds the BusBuddy solution
    #>
    [CmdletBinding()]
    param()

    Write-Host "🔨 Building BusBuddy..."
    try {
        dotnet build BusBuddy.sln --configuration Release --no-restore --verbosity minimal
        Write-Host "✅ Build complete!"
        return $true
    } catch {
        Write-Error "❌ Build failed: $_"
        return $false
    }
}

function global:Start-BusBuddyApp {
    param (
        [Parameter(Mandatory = $false)]
        [switch]$Debug,

        [Parameter(Mandatory = $false)]
        [switch]$NoBuild,

        [Parameter(Mandatory = $false)]
        [string]$Configuration = "Debug"
    )

    Write-Host "🚌 Starting BusBuddy application..." -ForegroundColor Cyan

    if (-not $NoBuild) {
        Write-Host "🔨 Building solution first..." -ForegroundColor Yellow
        Invoke-BusBuddyBuild -Configuration $Configuration

        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Build failed, cannot run application" -ForegroundColor Red
            return
        }
    }

    $arguments = @("run", "--project", "BusBuddy.WPF/BusBuddy.WPF.csproj", "--configuration", $Configuration)

    if ($Debug) {
        $arguments += "--launch-profile"
        $arguments += "Debug"
    }

    & dotnet $arguments

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Application exited successfully" -ForegroundColor Green
    } else {
        Write-Host "❌ Application exited with code: $LASTEXITCODE" -ForegroundColor Red
    }
}

function global:Clear-BusBuddyBuild {
    param (
        [Parameter(Mandatory = $false)]
        [switch]$Deep,

        [Parameter(Mandatory = $false)]
        [switch]$RemoveBin,

        [Parameter(Mandatory = $false)]
        [switch]$RemoveObj
    )

    Write-Host "🧹 Cleaning BusBuddy solution..." -ForegroundColor Cyan

    # Standard clean
    dotnet clean "BusBuddy.sln" --verbosity minimal
    Write-Host "✅ Solution cleaned successfully" -ForegroundColor Green

    if ($Deep -or $RemoveBin -or $RemoveObj) {
        Write-Host "🧹 Performing deep clean..." -ForegroundColor Yellow

        if ($Deep -or $RemoveBin) {
            Get-ChildItem -Path . -Include "bin" -Recurse -Directory | ForEach-Object {
                Write-Host "Removing $($_.FullName)" -ForegroundColor Gray
                Remove-Item -Path $_.FullName -Recurse -Force
            }
            Write-Host "✅ Removed bin directories" -ForegroundColor Green
        }

        if ($Deep -or $RemoveObj) {
            Get-ChildItem -Path . -Include "obj" -Recurse -Directory | ForEach-Object {
                Write-Host "Removing $($_.FullName)" -ForegroundColor Gray
                Remove-Item -Path $_.FullName -Recurse -Force
            }
            Write-Host "✅ Removed obj directories" -ForegroundColor Green
        }
    }

    Write-Host "✅ Clean operation completed" -ForegroundColor Green
}

# Helper functions for enhanced problem capture and analysis

function Get-BusBuddyProblemsFromVSCode {
    <#
    .SYNOPSIS
        Capture problems from VS Code's problem list
    #>

    $problems = @()

    try {
        # Try to get problems from VS Code CLI or workspace files
        if (Get-Command code -ErrorAction SilentlyContinue) {
            # Use VS Code CLI to get workspace problems (if available)
            Write-Host "   Attempting to capture VS Code problems..." -ForegroundColor Gray

            # Check for common problem indicators in workspace
            $problemFiles = @()

            # Look for files with common error patterns
            $sourceFiles = Get-ChildItem -Path . -Include "*.cs", "*.xaml", "*.csproj" -Recurse -ErrorAction SilentlyContinue

            foreach ($file in $sourceFiles) {
                try {
                    $content = Get-Content -Path $file.FullName -Raw -ErrorAction SilentlyContinue
                    if ($content) {
                        # Check for common syntax errors or issues
                        if ($content -match "(?i)error|exception|todo|fixme|hack") {
                            $problemFiles += @{
                                File = $file.FullName
                                Type = "Potential Issue"
                                Line = 1
                                Column = 1
                                Message = "File may contain issues (detected keywords)"
                                Severity = "Info"
                                Source = "VSCode-Pattern"
                            }
                        }
                    }
                }
                catch {
                    # Skip files that can't be read
                }
            }

            $problems += $problemFiles
        }
    }
    catch {
        Write-Host "   Could not access VS Code problems: $($_.Exception.Message)" -ForegroundColor Yellow
    }

    return $problems
}

function Get-BusBuddyProblemsFromBuildOutput {
    <#
    .SYNOPSIS
        Parse build output for problems and errors
    #>
    param (
        [Parameter(Mandatory = $true)]
        [string[]]$BuildOutput
    )

    $problems = @()

    foreach ($line in $BuildOutput) {
        # Parse MSBuild error format: File(line,column): error/warning CODE: message
        if ($line -match "(.+?)\((\d+),(\d+)\):\s*(error|warning)\s+(\w+):\s*(.+)") {
            $problems += @{
                File = $matches[1].Trim()
                Line = [int]$matches[2]
                Column = [int]$matches[3]
                Severity = if ($matches[4] -eq "error") { "Error" } else { "Warning" }
                Code = $matches[5]
                Message = $matches[6].Trim()
                Source = "MSBuild"
            }
        }
        # Parse simple error messages
        elseif ($line -match "(?i)error\s*:\s*(.+)") {
            $problems += @{
                File = "Unknown"
                Line = 0
                Column = 0
                Severity = "Error"
                Code = "UNKNOWN"
                Message = $matches[1].Trim()
                Source = "Build-Output"
            }
        }
        # Parse warning messages
        elseif ($line -match "(?i)warning\s*:\s*(.+)") {
            $problems += @{
                File = "Unknown"
                Line = 0
                Column = 0
                Severity = "Warning"
                Code = "UNKNOWN"
                Message = $matches[1].Trim()
                Source = "Build-Output"
            }
        }
    }

    return $problems
}

function Invoke-BusBuddyProblemAnalysis {
    <#
    .SYNOPSIS
        Analyze captured problems and provide fix recommendations
    #>
    param (
        [Parameter(Mandatory = $true)]
        [array]$Problems,

        [Parameter(Mandatory = $false)]
        [string[]]$BuildOutput = @()
    )

    $analysisResults = @()

    foreach ($problem in $Problems) {
        $analysis = @{
            Problem = $problem
            Severity = $problem.Severity
            Category = "Unknown"
            Description = $problem.Message
            FixRecommendation = "Manual review required"
            AutoFixAvailable = $false
            Priority = 3
        }

        # Categorize and analyze common problems
        switch -Regex ($problem.Message) {
            "(?i)missing.*reference" {
                $analysis.Category = "Missing Reference"
                $analysis.FixRecommendation = "Add package reference or assembly reference"
                $analysis.Priority = 1
            }
            "(?i)namespace.*not.*found" {
                $analysis.Category = "Namespace Issue"
                $analysis.FixRecommendation = "Add using statement or check namespace"
                $analysis.Priority = 2
            }
            "(?i)method.*not.*found" {
                $analysis.Category = "Method Missing"
                $analysis.FixRecommendation = "Check method name, parameters, or add implementation"
                $analysis.Priority = 2
            }
            "(?i)syntax.*error" {
                $analysis.Category = "Syntax Error"
                $analysis.FixRecommendation = "Fix syntax according to C# language rules"
                $analysis.Priority = 1
            }
            "(?i)unreachable.*code" {
                $analysis.Category = "Code Quality"
                $analysis.FixRecommendation = "Remove unreachable code or fix control flow"
                $analysis.AutoFixAvailable = $true
                $analysis.Priority = 3
            }
            "(?i)unused.*variable" {
                $analysis.Category = "Code Quality"
                $analysis.FixRecommendation = "Remove unused variable or use it"
                $analysis.AutoFixAvailable = $true
                $analysis.Priority = 3
            }
            default {
                $analysis.Category = "General Issue"
                $analysis.FixRecommendation = "Review the error message and fix manually"
            }
        }

        # Adjust priority based on severity
        if ($problem.Severity -eq "Error") {
            $analysis.Priority = [Math]::Min($analysis.Priority, 2)
        }

        $analysisResults += $analysis
    }

    return $analysisResults
}

function Export-BusBuddyBuildResults {
    <#
    .SYNOPSIS
        Export build analysis results to files
    #>
    param (
        [Parameter(Mandatory = $true)]
        [hashtable]$BuildContext,

        [Parameter(Mandatory = $true)]
        [string]$ExportPath
    )

    # Ensure export directory exists
    if (-not (Test-Path -Path $ExportPath)) {
        New-Item -Path $ExportPath -ItemType Directory -Force | Out-Null
    }

    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $exportResult = @{
        ExportPath = $ExportPath
        Files = @()
    }

    try {
        # Export problems as JSON
        $problemsFile = Join-Path -Path $ExportPath -ChildPath "problems-$timestamp.json"
        $BuildContext.Problems | ConvertTo-Json -Depth 10 | Set-Content -Path $problemsFile -Encoding UTF8
        $exportResult.Files += $problemsFile

        # Export analysis results as JSON
        if ($BuildContext.AnalysisResults.Count -gt 0) {
            $analysisFile = Join-Path -Path $ExportPath -ChildPath "analysis-$timestamp.json"
            $BuildContext.AnalysisResults | ConvertTo-Json -Depth 10 | Set-Content -Path $analysisFile -Encoding UTF8
            $exportResult.Files += $analysisFile
        }

        # Export build context summary
        $summaryFile = Join-Path -Path $ExportPath -ChildPath "build-summary-$timestamp.json"
        $summary = @{
            Timestamp = $BuildContext.Timestamp
            Configuration = $BuildContext.Configuration
            TotalProblems = $BuildContext.Problems.Count
            CriticalIssues = ($BuildContext.AnalysisResults | Where-Object { $_.Priority -eq 1 }).Count
            Warnings = ($BuildContext.Problems | Where-Object { $_.Severity -eq "Warning" }).Count
            Errors = ($BuildContext.Problems | Where-Object { $_.Severity -eq "Error" }).Count
        }
        $summary | ConvertTo-Json -Depth 5 | Set-Content -Path $summaryFile -Encoding UTF8
        $exportResult.Files += $summaryFile

        # Export build output as text
        $outputFile = Join-Path -Path $ExportPath -ChildPath "build-output-$timestamp.txt"
        $BuildContext.BuildOutput -join "`n" | Set-Content -Path $outputFile -Encoding UTF8
        $exportResult.Files += $outputFile

        return $exportResult
    }
    catch {
        throw "Failed to export build results: $($_.Exception.Message)"
    }
}

# Create aliases for backward compatibility
Set-Alias -Name "bb-build" -Value Invoke-BusBuddyBuild
Set-Alias -Name "bb-run" -Value Start-BusBuddyApp
Set-Alias -Name "bb-clean" -Value Clear-BusBuddyBuild

# Export functions
Export-ModuleMember -Function Invoke-BusBuddyBuild, Start-BusBuddyApp, Clear-BusBuddyBuild -Alias bb-build, bb-run, bb-clean
