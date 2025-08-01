#Requires -Version 7.5
function Test-BusBuddyModularSetup {
    <#
    .SYNOPSIS
        Tests the modular BusBuddy PowerShell environment setup

    .DESCRIPTION
        Validates that the modular PowerShell module structure is correctly
        set up and functioning as expected. Performs comprehensive checks of
        the directory structure, function loading, and module configuration.

    .PARAMETER Detailed
        Show detailed validation results

    .PARAMETER FixIssues
        Attempt to fix common issues automatically

    .OUTPUTS
        PSCustomObject with test results
    #>
    [CmdletBinding()]
    param(
        [switch]$Detailed,
        [switch]$FixIssues
    )

    Write-BusBuddyStatus "🔍 Testing BusBuddy Modular PowerShell Setup" -Status Info

    $results = @{
        Passed = $true
        Issues = @()
        Fixes = @()
        Stats = @{
            CategoryCount = 0
            FunctionCount = 0
            AliasCount = 0
        }
    }

    # Get module path
    $moduleRoot = Split-Path (Split-Path $PSCommandPath -Parent) -Parent

    Write-Host "Testing module at: $moduleRoot" -ForegroundColor Gray

    # 1. Check module structure
    $requiredFiles = @(
        'BusBuddy.psm1',
        'BusBuddy.settings.ini',
        'README.md'
    )

    foreach ($file in $requiredFiles) {
        $filePath = Join-Path $moduleRoot $file
        if (-not (Test-Path $filePath)) {
            $results.Passed = $false
            $results.Issues += "Missing required file: $file"

            if ($FixIssues -and $file -eq 'BusBuddy.settings.ini') {
                try {
                    $defaultSettings = @"
# BusBuddy Module Settings
# This file contains configurable settings for the BusBuddy PowerShell module
# Modify these settings to customize the module behavior to your preferences

[General]
# Enable verbose logging for module operations
VerboseLogging = false
# Show welcome message when module loads
ShowWelcomeMessage = true
# Check environment when module loads
AutoCheckEnvironment = true

[Development]
# Default build configuration
DefaultBuildConfiguration = Debug
# Default test configuration
DefaultTestConfiguration = Debug
# Enable code analysis during build
EnableCodeAnalysis = true
# Automatically restore NuGet packages before build
AutoRestorePackages = true
"@
                    Set-Content -Path $filePath -Value $defaultSettings -Encoding UTF8
                    $results.Fixes += "Created default settings file: $file"
                }
                catch {
                    $results.Issues += "Failed to create $($file): $($_.Exception.Message)"
                }
            }
        }
    }

    # 2. Check function categories
    $categoriesPath = Join-Path $moduleRoot "Functions"
    if (-not (Test-Path $categoriesPath)) {
        $results.Passed = $false
        $results.Issues += "Missing Functions directory"

        if ($FixIssues) {
            try {
                New-Item -Path $categoriesPath -ItemType Directory -Force | Out-Null
                $results.Fixes += "Created Functions directory"
            }
            catch {
                $results.Issues += "Failed to create Functions directory: $($_.Exception.Message)"
            }
        }
    }
    else {
        $categories = Get-ChildItem -Path $categoriesPath -Directory
        $results.Stats.CategoryCount = $categories.Count

        if ($categories.Count -eq 0) {
            $results.Passed = $false
            $results.Issues += "No function categories found"

            if ($FixIssues) {
                try {
                    foreach ($cat in @('Build', 'Database', 'Development', 'Diagnostics', 'Utilities', 'GitHub')) {
                        New-Item -Path (Join-Path $categoriesPath $cat) -ItemType Directory -Force | Out-Null
                    }
                    $results.Fixes += "Created default category directories"
                }
                catch {
                    $results.Issues += "Failed to create category directories: $($_.Exception.Message)"
                }
            }
        }
        else {
            # Check for functions in each category
            $functionCount = 0

            foreach ($category in $categories) {
                $functions = Get-ChildItem -Path $category.FullName -Filter "*.ps1" -File
                $functionCount += $functions.Count

                if ($Detailed) {
                    Write-Host "Category: $($category.Name) - $($functions.Count) functions" -ForegroundColor Cyan

                    if ($functions.Count -gt 0 -and $Detailed) {
                        foreach ($function in $functions) {
                            Write-Host "  - $($function.BaseName)" -ForegroundColor Gray
                        }
                    }
                }
            }

            $results.Stats.FunctionCount = $functionCount

            if ($functionCount -eq 0) {
                $results.Passed = $false
                $results.Issues += "No functions found in any category"
            }
        }
    }

    # 3. Check module configuration
    try {
        $moduleConfig = $script:BusBuddyModuleConfig

        if (-not $moduleConfig) {
            $results.Passed = $false
            $results.Issues += "Module configuration not initialized"
        }
        else {
            if ($Detailed) {
                Write-Host ""
                Write-Host "Module Configuration:" -ForegroundColor Cyan
                Write-Host "  Version: $($moduleConfig.Version)" -ForegroundColor Gray
                Write-Host "  Function Categories: $($moduleConfig.FunctionCategories.Count)" -ForegroundColor Gray
                Write-Host "  Loaded Functions: $($moduleConfig.LoadedFunctions.Count)" -ForegroundColor Gray
            }

            # Check loaded functions
            if ($moduleConfig.LoadedFunctions.Count -eq 0) {
                $results.Passed = $false
                $results.Issues += "No functions loaded in module configuration"
            }
        }
    }
    catch {
        $results.Passed = $false
        $results.Issues += "Error accessing module configuration: $($_.Exception.Message)"
    }

    # 4. Check aliases
    try {
        $moduleAliases = Get-Alias | Where-Object { $_.Name -like "bb-*" }
        $results.Stats.AliasCount = $moduleAliases.Count

        if ($moduleAliases.Count -eq 0) {
            $results.Passed = $false
            $results.Issues += "No bb-* aliases defined"
        }
        elseif ($Detailed) {
            Write-Host ""
            Write-Host "Aliases:" -ForegroundColor Cyan
            Write-Host "  Found $($moduleAliases.Count) bb-* aliases" -ForegroundColor Gray

            if ($Detailed) {
                foreach ($alias in $moduleAliases | Sort-Object Name) {
                    Write-Host "  - $($alias.Name) -> $($alias.ReferencedCommand)" -ForegroundColor Gray
                }
            }
        }
    }
    catch {
        $results.Passed = $false
        $results.Issues += "Error checking aliases: $($_.Exception.Message)"
    }

    # 5. Check loader script
    $projectRoot = Get-BusBuddyProjectRoot
    if ($projectRoot) {
        $loaderPath = Join-Path $projectRoot "load-bus-buddy-profiles.ps1"

        if (Test-Path $loaderPath) {
            $loaderContent = Get-Content $loaderPath -Raw
            $usingModularApproach = $loaderContent -match "Import-Module.*BusBuddy"

            if (-not $usingModularApproach) {
                $results.Passed = $false
                $results.Issues += "Loader script not updated to use modular approach"

                if ($FixIssues) {
                    try {
                        Update-BusBuddyProfileLoader -CreateBackup
                        $results.Fixes += "Updated loader script to use modular approach"
                    }
                    catch {
                        $results.Issues += "Failed to update loader script: $($_.Exception.Message)"
                    }
                }
            }
            elseif ($Detailed) {
                Write-Host ""
                Write-Host "Loader Script:" -ForegroundColor Cyan
                Write-Host "  ✅ Using modular approach" -ForegroundColor Green
            }
        }
    }

    # Display summary
    Write-Host ""
    Write-Host "📊 Modular Setup Test Results:" -ForegroundColor Cyan
    Write-Host "══════════════════════════════════════════" -ForegroundColor DarkGray

    # Statistics
    Write-Host "Module Statistics:" -ForegroundColor Yellow
    Write-Host "  Categories: $($results.Stats.CategoryCount)" -ForegroundColor Gray
    Write-Host "  Functions: $($results.Stats.FunctionCount)" -ForegroundColor Gray
    Write-Host "  Aliases: $($results.Stats.AliasCount)" -ForegroundColor Gray

    # Issues
    if ($results.Issues.Count -gt 0) {
        Write-Host ""
        Write-Host "Issues Found ($($results.Issues.Count)):" -ForegroundColor Red
        foreach ($issue in $results.Issues) {
            Write-Host "  • $issue" -ForegroundColor Red
        }
    }

    # Fixes
    if ($results.Fixes.Count -gt 0) {
        Write-Host ""
        Write-Host "Applied Fixes ($($results.Fixes.Count)):" -ForegroundColor Green
        foreach ($fix in $results.Fixes) {
            Write-Host "  ✓ $fix" -ForegroundColor Green
        }
    }

    # Overall result
    Write-Host ""
    if ($results.Passed) {
        Write-BusBuddyStatus "✅ Modular setup validation PASSED" -Status Success
    }
    else {
        Write-BusBuddyStatus "❌ Modular setup validation FAILED" -Status Error

        if (-not $FixIssues) {
            Write-Host "💡 Run with -FixIssues to attempt automatic repairs" -ForegroundColor Yellow
        }
    }

    return [PSCustomObject]$results
}
