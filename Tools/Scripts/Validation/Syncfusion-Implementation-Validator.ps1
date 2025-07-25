#Requires -Version 7.0
<#
.SYNOPSIS
    Syncfusion Implementation Validator for Bus Buddy

.DESCRIPTION
    Validates Syncfusion controls implementation, namespace usage, theme consistency,
    and common integration issues in the Bus Buddy WPF application.

.NOTES
    Designed specifically for Syncfusion Essential Studio for WPF version 30.1.40
    Validates FluentDark theme implementation and control usage patterns.
#>

param(
    [string]$Path = "BusBuddy.WPF",
    [switch]$Quick,
    [switch]$Detailed,
    [switch]$FixMode
)

# Initialize logging
$ErrorActionPreference = "Continue"
$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$logFile = "logs/syncfusion-validation-$timestamp.log"

function Write-ValidationLog {
    param($Message, $Level = "INFO")
    $logEntry = "[$((Get-Date).ToString('HH:mm:ss'))] [$Level] $Message"
    Write-Host $logEntry -ForegroundColor $(switch ($Level) {
            "ERROR" { "Red" }
            "WARNING" { "Yellow" }
            "SUCCESS" { "Green" }
            default { "White" }
        })
    $logEntry | Out-File -FilePath $logFile -Append -Encoding utf8
}

function Test-SyncfusionNamespaces {
    Write-ValidationLog "🔍 Validating Syncfusion namespace declarations..." "INFO"

    $xamlFiles = Get-ChildItem -Path $Path -Filter "*.xaml" -Recurse
    $issues = @()

    foreach ($file in $xamlFiles) {
        $content = Get-Content $file.FullName -Raw

        # Check for correct Syncfusion namespace
        if ($content -match 'syncfusion') {
            if ($content -notmatch 'xmlns:syncfusion="http://schemas\.syncfusion\.com/wpf"') {
                $issues += [PSCustomObject]@{
                    File     = $file.Name
                    Issue    = "Incorrect Syncfusion namespace declaration"
                    Line     = ($content -split "`n" | Select-String "syncfusion").LineNumber
                    Severity = "ERROR"
                }
            }
        }

        # Check for theme resource references
        if ($content -match 'FluentDark|FluentLight') {
            Write-ValidationLog "✅ Theme reference found in $($file.Name)" "SUCCESS"
        }
    }

    return $issues
}

function Test-SyncfusionControls {
    Write-ValidationLog "🔍 Validating Syncfusion control usage..." "INFO"

    $xamlFiles = Get-ChildItem -Path $Path -Filter "*.xaml" -Recurse
    $controlUsage = @{}
    $issues = @()

    $syncfusionControls = @(
        "SfDataGrid", "SfChart", "SfRichTextBoxAdv", "DockingManager",
        "NavigationDrawer", "SfDatePicker", "SfComboBox", "SfTextBox",
        "RibbonControl", "SfBusyIndicator", "SfProgressBar"
    )

    foreach ($file in $xamlFiles) {
        $content = Get-Content $file.FullName -Raw

        foreach ($control in $syncfusionControls) {
            if ($content -match "<syncfusion:$control" -or $content -match "<$control") {
                if (-not $controlUsage.ContainsKey($control)) {
                    $controlUsage[$control] = @()
                }
                $controlUsage[$control] += $file.Name

                # Check for proper theme application
                if ($content -match "<syncfusion:$control" -and $content -notmatch 'Style="{DynamicResource') {
                    $issues += [PSCustomObject]@{
                        File     = $file.Name
                        Issue    = "$control missing dynamic theme resource"
                        Control  = $control
                        Severity = "WARNING"
                    }
                }
            }
        }
    }

    Write-ValidationLog "📊 Syncfusion Controls Usage Summary:" "INFO"
    $controlUsage.GetEnumerator() | ForEach-Object {
        Write-ValidationLog "  $($_.Key): $($_.Value.Count) files" "INFO"
    }

    return $issues
}

function Test-ThemeConsistency {
    Write-ValidationLog "🎨 Validating theme consistency..." "INFO"

    $resourceFiles = Get-ChildItem -Path $Path -Filter "*.xaml" -Recurse | Where-Object {
        $_.Name -match "Resource|Theme|Style" -or $_.Directory.Name -eq "Resources"
    }

    $issues = @()
    $themeReferences = @()

    foreach ($file in $resourceFiles) {
        $content = Get-Content $file.FullName -Raw

        # Check for theme consistency
        if ($content -match 'FluentDark' -and $content -match 'FluentLight') {
            $issues += [PSCustomObject]@{
                File     = $file.Name
                Issue    = "Mixed theme references (FluentDark and FluentLight)"
                Severity = "ERROR"
            }
        }

        # Collect theme references
        $fluentDarkMatches = [regex]::Matches($content, 'FluentDark')
        $fluentLightMatches = [regex]::Matches($content, 'FluentLight')

        if ($fluentDarkMatches.Count -gt 0) {
            $themeReferences += "FluentDark: $($fluentDarkMatches.Count) in $($file.Name)"
        }
        if ($fluentLightMatches.Count -gt 0) {
            $themeReferences += "FluentLight: $($fluentLightMatches.Count) in $($file.Name)"
        }
    }

    Write-ValidationLog "🎨 Theme References Found:" "INFO"
    $themeReferences | ForEach-Object { Write-ValidationLog "  $_" "INFO" }

    return $issues
}

function Test-LicenseConfiguration {
    Write-ValidationLog "📋 Validating Syncfusion license configuration..." "INFO"

    $appFile = Join-Path $Path "App.xaml.cs"
    $issues = @()

    if (Test-Path $appFile) {
        # Check if license is registered before InitializeComponent
        $appContent = Get-Content $appFile -Raw
        $result = [PSCustomObject]@{
            IsValid = $false
            Issues  = @()
        }

        # Check for license registration in constructor
        if ($appContent -match "public App\(\s*\)\s*\{[^}]*(RegisterSyncfusionLicense|SyncfusionLicenseProvider.RegisterLicense)[^}]*\}") {
            $result.IsValid = $true
        }
        else {
            $result.Issues += "License registration not found in App constructor"
            $result.IsValid = $false
        }

        # Check if license registration happens before InitializeComponent() call
        if ($appContent -match "RegisterSyncfusionLicense.*InitializeComponent" -or
            $appContent -match "SyncfusionLicenseProvider.RegisterLicense.*InitializeComponent") {
            $result.IsValid = $true
        }
        else {
            $result.Issues += "License not registered before InitializeComponent"
            $result.IsValid = $false
        }

        # Verify presence of EnsureSyncfusionResourcesLoaded or similar method
        if ($appContent -match "EnsureSyncfusionResourcesLoaded|LoadSyncfusionResources|ConfigureSyncfusionTheme") {
            $result.IsValid = $true && $result.IsValid
        }
        else {
            $result.Issues += "No method to ensure Syncfusion resources are loaded"
            $result.IsValid = $false
        }

        return $result
    }
    else {
        $issues += [PSCustomObject]@{
            File     = "App.xaml.cs"
            Issue    = "App.xaml.cs not found"
            Severity = "ERROR"
        }
    }

    return $issues
}

function Test-AssemblyReferences {
    Write-ValidationLog "📦 Validating Syncfusion assembly references..." "INFO"

    $projectFile = Join-Path $Path "BusBuddy.WPF.csproj"
    $issues = @()

    if (Test-Path $projectFile) {
        $content = Get-Content $projectFile -Raw

        # Check for Syncfusion package references
        $syncfusionPackages = @(
            "Syncfusion.SfGrid.WPF",
            "Syncfusion.Chart.WPF",
            "Syncfusion.Docking.WPF",
            "Syncfusion.NavigationDrawer.WPF",
            "Syncfusion.Themes.FluentDark.WPF",
            "Syncfusion.Themes.FluentLight.WPF"
        )

        $foundPackages = @()
        foreach ($package in $syncfusionPackages) {
            if ($content -match $package) {
                $foundPackages += $package
            }
        }

        Write-ValidationLog "📦 Found Syncfusion packages: $($foundPackages.Count)/$($syncfusionPackages.Count)" "INFO"

        # Check version consistency
        $versionMatches = [regex]::Matches($content, 'Syncfusion\..*Version="([^"]+)"')
        $versions = $versionMatches | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique

        if ($versions.Count -gt 1) {
            $issues += [PSCustomObject]@{
                File     = "BusBuddy.WPF.csproj"
                Issue    = "Inconsistent Syncfusion package versions: $($versions -join ', ')"
                Severity = "WARNING"
            }
        }
        elseif ($versions.Count -eq 1) {
            Write-ValidationLog "✅ Consistent Syncfusion version: $($versions[0])" "SUCCESS"
        }
    }

    return $issues
}

function Invoke-SyncfusionValidation {
    Write-ValidationLog "🚀 Starting Syncfusion Implementation Validation for Bus Buddy" "INFO"
    Write-ValidationLog "Target Path: $Path" "INFO"

    # Create logs directory if it doesn't exist
    if (-not (Test-Path "logs")) {
        New-Item -ItemType Directory -Path "logs" -Force | Out-Null
    }

    $allIssues = @()

    try {
        # Run validation tests
        $allIssues += Test-SyncfusionNamespaces
        $allIssues += Test-SyncfusionControls
        $allIssues += Test-ThemeConsistency
        $allIssues += Test-LicenseConfiguration
        $allIssues += Test-AssemblyReferences

        # Summary report
        Write-ValidationLog "`n📊 VALIDATION SUMMARY" "INFO"
        Write-ValidationLog "===================" "INFO"

        $errorCount = ($allIssues | Where-Object { $_.Severity -eq "ERROR" }).Count
        $warningCount = ($allIssues | Where-Object { $_.Severity -eq "WARNING" }).Count

        Write-ValidationLog "Total Issues Found: $($allIssues.Count)" "INFO"
        Write-ValidationLog "Errors: $errorCount" $(if ($errorCount -gt 0) { "ERROR" } else { "SUCCESS" })
        Write-ValidationLog "Warnings: $warningCount" $(if ($warningCount -gt 0) { "WARNING" } else { "SUCCESS" })

        # Detailed issue report
        if ($allIssues.Count -gt 0) {
            Write-ValidationLog "`n🔍 DETAILED ISSUES" "INFO"
            Write-ValidationLog "=================" "INFO"

            $allIssues | Group-Object Severity | ForEach-Object {
                Write-ValidationLog "`n$($_.Name) Issues:" $_.Name
                $_.Group | ForEach-Object {
                    Write-ValidationLog "  📁 $($_.File): $($_.Issue)" $_.Severity
                }
            }
        }
        else {
            Write-ValidationLog "✅ No issues found! Syncfusion implementation looks good." "SUCCESS"
        }

        # Save detailed report
        $reportData = [PSCustomObject]@{
            Timestamp   = Get-Date
            Path        = $Path
            TotalIssues = $allIssues.Count
            Errors      = $errorCount
            Warnings    = $warningCount
            Issues      = $allIssues
        }

        $reportPath = "logs/syncfusion-validation-report-$timestamp.json"
        $reportData | ConvertTo-Json -Depth 10 | Out-File -FilePath $reportPath -Encoding utf8
        Write-ValidationLog "📄 Detailed report saved to: $reportPath" "INFO"

    }
    catch {
        Write-ValidationLog "❌ Validation failed: $($_.Exception.Message)" "ERROR"
        throw
    }

    Write-ValidationLog "✅ Syncfusion validation completed" "SUCCESS"
    return $allIssues.Count -eq 0
}

function Test-SyncfusionImplementation {
    [CmdletBinding()]
    param (
        [Parameter()]
        [string]$ProjectPath = (Join-Path $PSScriptRoot "..\..\..")
    )

    Write-Host "Starting Syncfusion implementation validation..." -ForegroundColor Cyan

    $wpfProjectPath = Join-Path $ProjectPath "BusBuddy.WPF"
    $resourcesPath = Join-Path $wpfProjectPath "Resources"
    $appXaml = Join-Path $wpfProjectPath "App.xaml"
    $menuStyles = Join-Path $resourcesPath "MenuStyles.xaml"
    $themeSvc = Join-Path $wpfProjectPath "Services\ThemeService.cs"

    $issues = @()
    $warnings = @()

    # Check required files exist
    Write-Host "Checking required files..." -ForegroundColor Yellow

    if (-not (Test-Path $appXaml)) {
        $issues += "App.xaml not found"
    }

    if (-not (Test-Path $menuStyles)) {
        $issues += "MenuStyles.xaml not found"
    }

    if (-not (Test-Path $themeSvc)) {
        $issues += "ThemeService.cs not found"
    }

    # Check App.xaml includes MenuStyles.xaml
    if (Test-Path $appXaml) {
        Write-Host "Checking App.xaml ResourceDictionary setup..." -ForegroundColor Yellow
        $appContent = Get-Content $appXaml -Raw

        if (-not ($appContent -match "ResourceDictionary\.MergedDictionaries")) {
            $issues += "App.xaml does not have MergedDictionaries section"
        }

        if (-not ($appContent -match "component/Resources/MenuStyles\.xaml")) {
            $issues += "App.xaml does not include MenuStyles.xaml in MergedDictionaries"
        }
    }

    # Check MenuStyles.xaml has required styles
    if (Test-Path $menuStyles) {
        Write-Host "Checking MenuStyles.xaml for required styles..." -ForegroundColor Yellow
        $menuStylesContent = Get-Content $menuStyles -Raw

        $requiredStyles = @(
            "MenuItemStyle",
            "MenuSeparatorStyle",
            "ContextMenuStyle"
        )

        foreach ($style in $requiredStyles) {
            if (-not ($menuStylesContent -match "x:Key=`"$style`"")) {
                $issues += "MenuStyles.xaml missing required style: $style"
            }
        }
    }

    # Check ThemeService.cs for proper SfSkinManager usage
    if (Test-Path $themeSvc) {
        Write-Host "Checking ThemeService.cs for proper SfSkinManager implementation..." -ForegroundColor Yellow
        $themeSvcContent = Get-Content $themeSvc -Raw

        $requiredPatterns = @(
            "SfSkinManager\.ApplyThemeAsDefaultStyle\s*=\s*true",
            "SfSkinManager\.ApplicationTheme",
            "SfSkinManager\.SetTheme\("
        )

        foreach ($pattern in $requiredPatterns) {
            if (-not ($themeSvcContent -match $pattern)) {
                $issues += "ThemeService.cs missing required SfSkinManager implementation: $pattern"
            }
        }

        # Check theme registration
        if (-not ($themeSvcContent -match "SfSkinManager\.RegisterThemeSettings\(")) {
            $warnings += "ThemeService.cs may be missing theme settings registration"
        }
    }

    # Output results
    if ($issues.Count -eq 0) {
        Write-Host "`nSyncfusion implementation validation PASSED!" -ForegroundColor Green
    }
    else {
        Write-Host "`nSyncfusion implementation validation FAILED!" -ForegroundColor Red
        Write-Host "`nCritical Issues:" -ForegroundColor Red
        foreach ($issue in $issues) {
            Write-Host "- $issue" -ForegroundColor Red
        }
    }

    if ($warnings.Count -gt 0) {
        Write-Host "`nWarnings:" -ForegroundColor Yellow
        foreach ($warning in $warnings) {
            Write-Host "- $warning" -ForegroundColor Yellow
        }
    }

    # Return validation result object
    [PSCustomObject]@{
        Success  = ($issues.Count -eq 0)
        Issues   = $issues
        Warnings = $warnings
    }
}

# Main execution
if ($MyInvocation.InvocationName -ne '.') {
    Invoke-SyncfusionValidation
}
