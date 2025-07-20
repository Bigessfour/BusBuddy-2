#Requires -Version 7.0
<#
.SYNOPSIS
    Bus Buddy Theme Check Utility

.DESCRIPTION
    Validates Syncfusion FluentDark/FluentLight theme consistency, resource usage,
    and theme-related issues in the Bus Buddy WPF application.

.NOTES
    Specialized for Syncfusion Essential Studio 30.1.40 theme system
    Focuses on theme consistency and resource validation.
#>

param(
    [string]$Path = "BusBuddy.WPF",
    [switch]$Quick,
    [switch]$FixMode,
    [string]$PreferredTheme = "FluentDark"
)

# Initialize logging
$ErrorActionPreference = "Continue"
$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$logFile = "logs/theme-check-$timestamp.log"

function Write-ThemeLog {
    param($Message, $Level = "INFO")
    $logEntry = "[$((Get-Date).ToString('HH:mm:ss'))] [$Level] $Message"
    Write-Host $logEntry -ForegroundColor $(switch($Level) {
        "ERROR" { "Red" }
        "WARNING" { "Yellow" }
        "SUCCESS" { "Green" }
        "THEME" { "Magenta" }
        default { "White" }
    })
    $logEntry | Out-File -FilePath $logFile -Append -Encoding utf8
}

function Get-ThemeFiles {
    Write-ThemeLog "🔍 Discovering theme-related files..." "INFO"

    $themeFiles = @{
        ResourceDictionaries = @()
        Views = @()
        Styles = @()
        Controls = @()
    }

    # Resource dictionaries
    $themeFiles.ResourceDictionaries = Get-ChildItem -Path $Path -Filter "*.xaml" -Recurse | Where-Object {
        $_.Name -match "(Resource|Theme|Style)" -or $_.Directory.Name -eq "Resources"
    }

    # Views with theme references
    $themeFiles.Views = Get-ChildItem -Path $Path -Filter "*.xaml" -Recurse | Where-Object {
        $content = Get-Content $_.FullName -Raw -ErrorAction SilentlyContinue
        $content -match "(FluentDark|FluentLight|DynamicResource|StaticResource)"
    }

    # Custom controls
    $themeFiles.Controls = Get-ChildItem -Path "$Path/Controls" -Filter "*.xaml" -Recurse -ErrorAction SilentlyContinue

    Write-ThemeLog "📁 Found $($themeFiles.ResourceDictionaries.Count) resource dictionaries" "INFO"
    Write-ThemeLog "📁 Found $($themeFiles.Views.Count) views with theme references" "INFO"
    Write-ThemeLog "📁 Found $($themeFiles.Controls.Count) custom controls" "INFO"

    return $themeFiles
}

function Test-ThemeConsistency {
    param($ThemeFiles)

    Write-ThemeLog "🎨 Testing theme consistency..." "THEME"

    $issues = @()
    $themeStats = @{
        FluentDark = 0
        FluentLight = 0
        Mixed = 0
        Unthemed = 0
    }

    foreach ($file in $ThemeFiles.Views) {
        $content = Get-Content $file.FullName -Raw
        $darkRefs = [regex]::Matches($content, 'FluentDark').Count
        $lightRefs = [regex]::Matches($content, 'FluentLight').Count

        if ($darkRefs -gt 0 -and $lightRefs -gt 0) {
            $themeStats.Mixed++
            $issues += [PSCustomObject]@{
                File = $file.Name
                Issue = "Mixed theme references (FluentDark: $darkRefs, FluentLight: $lightRefs)"
                Type = "ThemeConsistency"
                Severity = "ERROR"
                Recommendation = "Use consistent theme throughout file"
            }
        } elseif ($darkRefs -gt 0) {
            $themeStats.FluentDark++
        } elseif ($lightRefs -gt 0) {
            $themeStats.FluentLight++
        } else {
            # Check if file uses Syncfusion controls without theme
            if ($content -match "<syncfusion:" -and $content -notmatch "(FluentDark|FluentLight)") {
                $themeStats.Unthemed++
                $issues += [PSCustomObject]@{
                    File = $file.Name
                    Issue = "Syncfusion controls without explicit theme reference"
                    Type = "MissingTheme"
                    Severity = "WARNING"
                    Recommendation = "Add theme reference to ensure consistent styling"
                }
            }
        }
    }

    Write-ThemeLog "📊 Theme Usage Statistics:" "THEME"
    Write-ThemeLog "  FluentDark files: $($themeStats.FluentDark)" "INFO"
    Write-ThemeLog "  FluentLight files: $($themeStats.FluentLight)" "INFO"
    Write-ThemeLog "  Mixed theme files: $($themeStats.Mixed)" $(if($themeStats.Mixed -gt 0) { "ERROR" } else { "SUCCESS" })
    Write-ThemeLog "  Unthemed files: $($themeStats.Unthemed)" $(if($themeStats.Unthemed -gt 0) { "WARNING" } else { "SUCCESS" })

    return $issues
}

function Test-ResourceDictionaries {
    param($ThemeFiles)

    Write-ThemeLog "📚 Testing resource dictionaries..." "THEME"

    $issues = @()
    $resourceStats = @{
        TotalResources = 0
        ThemeResources = 0
        OrphanedResources = 0
    }

    foreach ($file in $ThemeFiles.ResourceDictionaries) {
        $content = Get-Content $file.FullName -Raw

        # Count total resources
        $resourceMatches = [regex]::Matches($content, '<(Style|DataTemplate|ControlTemplate|SolidColorBrush|LinearGradientBrush)')
        $resourceStats.TotalResources += $resourceMatches.Count

        # Count theme-specific resources
        $themeResourceMatches = [regex]::Matches($content, '(FluentDark|FluentLight)')
        $resourceStats.ThemeResources += $themeResourceMatches.Count

        # Check for proper resource key naming
        $resourceKeys = [regex]::Matches($content, 'x:Key="([^"]+)"') | ForEach-Object { $_.Groups[1].Value }

        foreach ($key in $resourceKeys) {
            # Check for theme-specific naming patterns
            if ($key -match "(Dark|Light)" -and $key -notmatch "(FluentDark|FluentLight)") {
                $issues += [PSCustomObject]@{
                    File = $file.Name
                    Issue = "Resource key '$key' uses theme naming but not Syncfusion convention"
                    Type = "NamingConvention"
                    Severity = "WARNING"
                    Recommendation = "Consider using FluentDark/FluentLight prefix for theme resources"
                }
            }

            # Check for orphaned theme resources
            if ($key -match "(FluentDark|FluentLight)") {
                # Search for usage across all XAML files
                $keyUsage = Get-ChildItem -Path $Path -Filter "*.xaml" -Recurse | ForEach-Object {
                    $xamlContent = Get-Content $_.FullName -Raw -ErrorAction SilentlyContinue
                    if ($xamlContent -match $key) { $_.Name }
                } | Where-Object { $_ -ne $file.Name }

                if (-not $keyUsage) {
                    $resourceStats.OrphanedResources++
                    $issues += [PSCustomObject]@{
                        File = $file.Name
                        Issue = "Orphaned theme resource '$key' - not used anywhere"
                        Type = "OrphanedResource"
                        Severity = "WARNING"
                        Recommendation = "Remove unused resource or verify usage"
                    }
                }
            }
        }
    }

    Write-ThemeLog "📊 Resource Dictionary Statistics:" "THEME"
    Write-ThemeLog "  Total resources: $($resourceStats.TotalResources)" "INFO"
    Write-ThemeLog "  Theme resources: $($resourceStats.ThemeResources)" "INFO"
    Write-ThemeLog "  Orphaned resources: $($resourceStats.OrphanedResources)" $(if($resourceStats.OrphanedResources -gt 0) { "WARNING" } else { "SUCCESS" })

    return $issues
}

function Test-DynamicResourceUsage {
    param($ThemeFiles)

    Write-ThemeLog "🔗 Testing dynamic resource usage..." "THEME"

    $issues = @()
    $resourceUsage = @{
        DynamicResource = 0
        StaticResource = 0
        ThemeResourcesMissingDynamic = 0
    }

    foreach ($file in $ThemeFiles.Views) {
        $content = Get-Content $file.FullName -Raw

        # Count resource types
        $dynamicMatches = [regex]::Matches($content, 'DynamicResource')
        $staticMatches = [regex]::Matches($content, 'StaticResource')

        $resourceUsage.DynamicResource += $dynamicMatches.Count
        $resourceUsage.StaticResource += $staticMatches.Count

        # Check for theme resources using StaticResource (should be Dynamic for theme switching)
        $staticThemeMatches = [regex]::Matches($content, 'StaticResource\s+([^}]+(?:FluentDark|FluentLight)[^}]*)')

        foreach ($match in $staticThemeMatches) {
            $resourceUsage.ThemeResourcesMissingDynamic++
            $issues += [PSCustomObject]@{
                File = $file.Name
                Issue = "Theme resource '$($match.Groups[1].Value)' uses StaticResource instead of DynamicResource"
                Type = "ResourceBinding"
                Severity = "WARNING"
                Recommendation = "Use DynamicResource for theme resources to enable runtime theme switching"
            }
        }
    }

    Write-ThemeLog "📊 Resource Usage Statistics:" "THEME"
    Write-ThemeLog "  DynamicResource usages: $($resourceUsage.DynamicResource)" "SUCCESS"
    Write-ThemeLog "  StaticResource usages: $($resourceUsage.StaticResource)" "INFO"
    Write-ThemeLog "  Theme resources needing Dynamic: $($resourceUsage.ThemeResourcesMissingDynamic)" $(if($resourceUsage.ThemeResourcesMissingDynamic -gt 0) { "WARNING" } else { "SUCCESS" })

    return $issues
}

function Test-AppXamlThemeConfiguration {
    Write-ThemeLog "🎯 Testing App.xaml theme configuration..." "THEME"

    $appXamlPath = Join-Path $Path "App.xaml"
    $issues = @()

    if (Test-Path $appXamlPath) {
        $content = Get-Content $appXamlPath -Raw

        # Check for theme resource dictionaries
        $themeResources = @()
        if ($content -match 'FluentDark\.WPF') { $themeResources += "FluentDark" }
        if ($content -match 'FluentLight\.WPF') { $themeResources += "FluentLight" }

        if ($themeResources.Count -eq 0) {
            $issues += [PSCustomObject]@{
                File = "App.xaml"
                Issue = "No Syncfusion theme resource dictionaries found"
                Type = "MissingTheme"
                Severity = "ERROR"
                Recommendation = "Add Syncfusion theme resource dictionaries to App.xaml"
            }
        } elseif ($themeResources.Count -gt 1) {
            $issues += [PSCustomObject]@{
                File = "App.xaml"
                Issue = "Multiple theme resource dictionaries found: $($themeResources -join ', ')"
                Type = "ThemeConflict"
                Severity = "WARNING"
                Recommendation = "Use only one theme resource dictionary at a time"
            }
        } else {
            Write-ThemeLog "✅ Single theme configuration found: $($themeResources[0])" "SUCCESS"
        }

        # Check resource dictionary order
        $resourceDictLines = $content -split "`n" | Where-Object { $_ -match 'ResourceDictionary' }
        $syncfusionThemeLineIndex = -1
        $customResourceLineIndex = -1

        for ($i = 0; $i -lt $resourceDictLines.Count; $i++) {
            if ($resourceDictLines[$i] -match '(FluentDark|FluentLight)\.WPF') {
                $syncfusionThemeLineIndex = $i
            } elseif ($resourceDictLines[$i] -match 'Resources/' -and $resourceDictLines[$i] -notmatch 'Syncfusion') {
                $customResourceLineIndex = $i
            }
        }

        if ($syncfusionThemeLineIndex -gt $customResourceLineIndex -and $customResourceLineIndex -gt -1) {
            $issues += [PSCustomObject]@{
                File = "App.xaml"
                Issue = "Syncfusion theme resources should be loaded before custom resources"
                Type = "ResourceOrder"
                Severity = "WARNING"
                Recommendation = "Move Syncfusion theme resources above custom resource dictionaries"
            }
        }

    } else {
        $issues += [PSCustomObject]@{
            File = "App.xaml"
            Issue = "App.xaml not found"
            Type = "MissingFile"
            Severity = "ERROR"
            Recommendation = "Ensure App.xaml exists and contains theme configuration"
        }
    }

    return $issues
}

function Invoke-ThemeCheck {
    Write-ThemeLog "🎨 Starting Bus Buddy Theme Check" "THEME"
    Write-ThemeLog "Target Path: $Path" "INFO"
    Write-ThemeLog "Preferred Theme: $PreferredTheme" "INFO"

    # Create logs directory if it doesn't exist
    if (-not (Test-Path "logs")) {
        New-Item -ItemType Directory -Path "logs" -Force | Out-Null
    }

    $allIssues = @()

    try {
        # Discover theme files
        $themeFiles = Get-ThemeFiles

        # Run theme validation tests
        $allIssues += Test-ThemeConsistency -ThemeFiles $themeFiles
        $allIssues += Test-ResourceDictionaries -ThemeFiles $themeFiles
        $allIssues += Test-DynamicResourceUsage -ThemeFiles $themeFiles
        $allIssues += Test-AppXamlThemeConfiguration

        # Summary report
        Write-ThemeLog "`n📊 THEME CHECK SUMMARY" "THEME"
        Write-ThemeLog "=====================" "THEME"

        $errorCount = ($allIssues | Where-Object { $_.Severity -eq "ERROR" }).Count
        $warningCount = ($allIssues | Where-Object { $_.Severity -eq "WARNING" }).Count

        Write-ThemeLog "Total Issues Found: $($allIssues.Count)" "INFO"
        Write-ThemeLog "Errors: $errorCount" $(if($errorCount -gt 0) { "ERROR" } else { "SUCCESS" })
        Write-ThemeLog "Warnings: $warningCount" $(if($warningCount -gt 0) { "WARNING" } else { "SUCCESS" })

        # Issue breakdown by type
        if ($allIssues.Count -gt 0) {
            Write-ThemeLog "`n🔍 ISSUES BY TYPE" "THEME"
            Write-ThemeLog "================" "THEME"

            $allIssues | Group-Object Type | Sort-Object Count -Descending | ForEach-Object {
                Write-ThemeLog "  $($_.Name): $($_.Count) issues" "INFO"
            }

            # Detailed issues
            Write-ThemeLog "`n📋 DETAILED ISSUES" "THEME"
            Write-ThemeLog "==================" "THEME"

            $allIssues | Group-Object Severity | ForEach-Object {
                Write-ThemeLog "`n$($_.Name) Issues:" $_.Name
                $_.Group | ForEach-Object {
                    Write-ThemeLog "  📁 $($_.File)" "INFO"
                    Write-ThemeLog "     Issue: $($_.Issue)" $_.Severity
                    Write-ThemeLog "     Fix: $($_.Recommendation)" "INFO"
                }
            }
        } else {
            Write-ThemeLog "✅ No theme issues found! Theme configuration looks excellent." "SUCCESS"
        }

        # Save detailed report
        $reportData = [PSCustomObject]@{
            Timestamp = Get-Date
            Path = $Path
            PreferredTheme = $PreferredTheme
            TotalIssues = $allIssues.Count
            Errors = $errorCount
            Warnings = $warningCount
            Issues = $allIssues
            ThemeFiles = @{
                ResourceDictionaries = $themeFiles.ResourceDictionaries.Name
                Views = $themeFiles.Views.Name
                Controls = $themeFiles.Controls.Name
            }
        }

        $reportPath = "logs/theme-check-report-$timestamp.json"
        $reportData | ConvertTo-Json -Depth 10 | Out-File -FilePath $reportPath -Encoding utf8
        Write-ThemeLog "📄 Detailed report saved to: $reportPath" "INFO"

    } catch {
        Write-ThemeLog "❌ Theme check failed: $($_.Exception.Message)" "ERROR"
        throw
    }

    Write-ThemeLog "✅ Theme check completed" "SUCCESS"
    return $allIssues.Count -eq 0
}

# Main execution
if ($MyInvocation.InvocationName -ne '.') {
    $result = Invoke-ThemeCheck

    # Return appropriate exit code for workflow integration
    if ($result) {
        exit 0  # Success
    } else {
        exit 1  # Issues found
    }
}
