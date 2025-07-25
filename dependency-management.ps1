#!/usr/bin/env pwsh
<#
.SYNOPSIS
    BusBuddy Dependency Management and Security Scanner

.DESCRIPTION
    Comprehensive PowerShell script for managing NuGet dependencies, vulnerability scanning,
    version validation, and dependency health monitoring for the BusBuddy application.

.NOTES
    Author: GitHub Copilot
    Date: 2025-07-25
    Version: 1.0

    Dependencies: .NET SDK 8.0+, PowerShell 7.5.2+
#>

param(
    [Parameter(HelpMessage = "Scan for vulnerable packages")]
    [switch]$ScanVulnerabilities,

    [Parameter(HelpMessage = "Validate version pinning across projects")]
    [switch]$ValidateVersions,

    [Parameter(HelpMessage = "Generate dependency report")]
    [switch]$GenerateReport,

    [Parameter(HelpMessage = "Update packages to latest versions")]
    [switch]$UpdatePackages,

    [Parameter(HelpMessage = "Restore packages only")]
    [switch]$RestoreOnly,

    [Parameter(HelpMessage = "Run all checks")]
    [switch]$Full
)

# 🚌 BusBuddy Dependency Management
Write-Host "🚌 BusBuddy Dependency Management & Security Scanner" -ForegroundColor Cyan
Write-Host "📅 $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
Write-Host ""

# Set all switches if Full is specified
if ($Full) {
    $ScanVulnerabilities = $true
    $ValidateVersions = $true
    $GenerateReport = $true
    $RestoreOnly = $true
}

# Results container
$Results = @{
    Timestamp          = Get-Date
    VulnerabilityCount = 0
    VersionMismatches  = @()
    PackageCount       = 0
    Projects           = @()
    Recommendations    = @()
}

function Write-Section {
    param([string]$Title, [string]$Color = "Cyan")
    Write-Host ""
    Write-Host "🔍 $Title" -ForegroundColor $Color
    Write-Host ("─" * ($Title.Length + 4)) -ForegroundColor DarkGray
}

function Invoke-PackageRestore {
    Write-Section "Package Restoration"

    try {
        Write-Host "📦 Restoring NuGet packages..." -ForegroundColor Yellow

        $restoreResult = dotnet restore "BusBuddy.sln" --verbosity normal --no-cache 2>&1

        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Package restoration completed successfully" -ForegroundColor Green

            # Parse restore output for useful information
            $restoreLines = $restoreResult -split "`n"
            $restoredPackages = $restoreLines | Where-Object { $_ -match "Restored" } | Measure-Object

            Write-Host "📊 Restored packages: $($restoredPackages.Count)" -ForegroundColor Gray

            $Results.PackageCount = $restoredPackages.Count
            return $true
        }
        else {
            Write-Host "❌ Package restoration failed" -ForegroundColor Red
            Write-Host $restoreResult -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "❌ Error during package restoration: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Invoke-VulnerabilityScanning {
    Write-Section "Vulnerability Scanning"

    try {
        Write-Host "🔍 Scanning for vulnerable packages..." -ForegroundColor Yellow

        # Get all projects
        $projects = @("BusBuddy.Core", "BusBuddy.WPF")
        $vulnerabilityFound = $false

        foreach ($project in $projects) {
            Write-Host "  📋 Checking $project..." -ForegroundColor Gray

            $vulnResult = dotnet list "$project/$project.csproj" package --vulnerable 2>&1

            if ($vulnResult -match "no vulnerable packages") {
                Write-Host "    ✅ No vulnerabilities found" -ForegroundColor Green
            }
            elseif ($vulnResult -match "vulnerable") {
                Write-Host "    ⚠️ Vulnerabilities detected!" -ForegroundColor Red
                Write-Host $vulnResult -ForegroundColor Yellow
                $vulnerabilityFound = $true
                $Results.VulnerabilityCount++
            }
            else {
                Write-Host "    ℹ️ $vulnResult" -ForegroundColor Gray
            }

            $Results.Projects += @{
                Name       = $project
                Vulnerable = $vulnerabilityFound
                ScanResult = $vulnResult
            }
        }

        if (-not $vulnerabilityFound) {
            Write-Host "🛡️ All packages are secure - no vulnerabilities detected" -ForegroundColor Green
        }
        else {
            Write-Host "⚠️ Vulnerabilities found! Review and update affected packages" -ForegroundColor Red
            $Results.Recommendations += "Update vulnerable packages immediately"
        }

    }
    catch {
        Write-Host "❌ Error during vulnerability scanning: $($_.Exception.Message)" -ForegroundColor Red
    }
}

function Test-VersionPinning {
    Write-Section "Version Pinning Validation"

    try {
        Write-Host "📌 Validating version pinning..." -ForegroundColor Yellow

        # Check Directory.Build.props for version variables
        $buildPropsPath = "Directory.Build.props"
        if (Test-Path $buildPropsPath) {
            $buildPropsContent = Get-Content $buildPropsPath -Raw

            # Extract version properties
            $syncfusionVersion = [regex]::Match($buildPropsContent, '<SyncfusionVersion>(.*?)</SyncfusionVersion>').Groups[1].Value
            $efVersion = [regex]::Match($buildPropsContent, '<EntityFrameworkVersion>(.*?)</EntityFrameworkVersion>').Groups[1].Value
            $extensionsVersion = [regex]::Match($buildPropsContent, '<MicrosoftExtensionsVersion>(.*?)</MicrosoftExtensionsVersion>').Groups[1].Value

            Write-Host "  📋 Defined versions:" -ForegroundColor Gray
            Write-Host "    🔷 Syncfusion: $syncfusionVersion" -ForegroundColor Cyan
            Write-Host "    🔷 Entity Framework: $efVersion" -ForegroundColor Cyan
            Write-Host "    🔷 Microsoft Extensions: $extensionsVersion" -ForegroundColor Cyan

            # Validate that versions are pinned (not using wildcards)
            $versions = @($syncfusionVersion, $efVersion, $extensionsVersion)
            $unpinnedVersions = $versions | Where-Object { $_ -match '\*' -or $_ -match '\+' }

            if ($unpinnedVersions.Count -eq 0) {
                Write-Host "✅ All versions are properly pinned" -ForegroundColor Green
            }
            else {
                Write-Host "⚠️ Found unpinned versions: $($unpinnedVersions -join ', ')" -ForegroundColor Yellow
                $Results.VersionMismatches += "Unpinned versions detected"
                $Results.Recommendations += "Pin all package versions to avoid unexpected upgrades"
            }
        }
        else {
            Write-Host "⚠️ Directory.Build.props not found" -ForegroundColor Yellow
        }

        # Check individual project files for version consistency
        $projects = Get-ChildItem -Path "." -Filter "*.csproj" -Recurse
        foreach ($project in $projects) {
            Write-Host "  📁 Checking $($project.Name)..." -ForegroundColor Gray

            $projectContent = Get-Content $project.FullName -Raw
            $packageRefs = [regex]::Matches($projectContent, '<PackageReference\s+Include="([^"]+)"\s+Version="([^"]+)"')

            foreach ($match in $packageRefs) {
                $packageName = $match.Groups[1].Value
                $version = $match.Groups[2].Value

                # Check if version uses variables (recommended) or is hardcoded
                if ($version -match '\$\(.*\)') {
                    # Good: Using variable
                    continue
                }
                elseif ($version -match '\*|\+') {
                    Write-Host "    ⚠️ Unpinned version: $packageName = $version" -ForegroundColor Yellow
                    $Results.VersionMismatches += "$packageName = $version in $($project.Name)"
                }
            }
        }

    }
    catch {
        Write-Host "❌ Error during version validation: $($_.Exception.Message)" -ForegroundColor Red
    }
}

function New-DependencyReport {
    Write-Section "Dependency Report Generation"

    try {
        Write-Host "📊 Generating dependency report..." -ForegroundColor Yellow

        # Get package lists for each project
        $projects = @("BusBuddy.Core", "BusBuddy.WPF")
        $allPackages = @()

        foreach ($project in $projects) {
            Write-Host "  📋 Analyzing $project..." -ForegroundColor Gray

            $packageList = dotnet list "$project/$project.csproj" package --format json 2>$null
            if ($packageList) {
                $packageData = $packageList | ConvertFrom-Json

                foreach ($framework in $packageData.projects[0].frameworks) {
                    foreach ($package in $framework.topLevelPackages) {
                        $allPackages += [PSCustomObject]@{
                            Project   = $project
                            Framework = $framework.framework
                            Package   = $package.id
                            Version   = $package.resolvedVersion
                            Requested = $package.requestedVersion
                        }
                    }
                }
            }
        }

        $Results.PackageCount = $allPackages.Count

        Write-Host "📦 Total packages: $($allPackages.Count)" -ForegroundColor Cyan

        # Group by package name to show version consistency
        $packageGroups = $allPackages | Group-Object Package
        $versionInconsistencies = $packageGroups | Where-Object { ($_.Group | Select-Object -Unique Version).Count -gt 1 }

        if ($versionInconsistencies.Count -gt 0) {
            Write-Host "⚠️ Version inconsistencies found:" -ForegroundColor Yellow
            foreach ($inconsistency in $versionInconsistencies) {
                Write-Host "    📦 $($inconsistency.Name):" -ForegroundColor Red
                $inconsistency.Group | ForEach-Object {
                    Write-Host "      - $($_.Project): $($_.Version)" -ForegroundColor Gray
                }
                $Results.VersionMismatches += "$($inconsistency.Name) has version inconsistencies"
            }
            $Results.Recommendations += "Resolve package version inconsistencies across projects"
        }
        else {
            Write-Host "✅ All package versions are consistent across projects" -ForegroundColor Green
        }

        # Save report to file
        $reportPath = "dependency-report-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
        $Results | ConvertTo-Json -Depth 10 | Out-File $reportPath -Encoding UTF8
        Write-Host "💾 Report saved to: $reportPath" -ForegroundColor Cyan

    }
    catch {
        Write-Host "❌ Error generating dependency report: $($_.Exception.Message)" -ForegroundColor Red
    }
}

function Update-Packages {
    Write-Section "Package Updates"

    Write-Host "⚠️ Package updates disabled for version-pinned project" -ForegroundColor Yellow
    Write-Host "📋 To update packages:" -ForegroundColor Gray
    Write-Host "  1. Review latest versions manually" -ForegroundColor Gray
    Write-Host "  2. Update version variables in Directory.Build.props" -ForegroundColor Gray
    Write-Host "  3. Test thoroughly before committing" -ForegroundColor Gray
    Write-Host "  4. Run full dependency scan after updates" -ForegroundColor Gray

    $Results.Recommendations += "Manual package updates recommended for version-pinned projects"
}

function Show-Summary {
    Write-Section "Summary & Recommendations"

    Write-Host "📊 Dependency Management Summary:" -ForegroundColor Cyan
    Write-Host "  📦 Total packages: $($Results.PackageCount)" -ForegroundColor Gray
    Write-Host "  🛡️ Vulnerabilities: $($Results.VulnerabilityCount)" -ForegroundColor $(if ($Results.VulnerabilityCount -eq 0) { "Green" } else { "Red" })
    Write-Host "  ⚠️ Version issues: $($Results.VersionMismatches.Count)" -ForegroundColor $(if ($Results.VersionMismatches.Count -eq 0) { "Green" } else { "Yellow" })
    Write-Host ""

    if ($Results.Recommendations.Count -gt 0) {
        Write-Host "💡 Recommendations:" -ForegroundColor Yellow
        $Results.Recommendations | ForEach-Object {
            Write-Host "  • $_" -ForegroundColor Gray
        }
    }
    else {
        Write-Host "✅ All dependency checks passed!" -ForegroundColor Green
    }

    Write-Host ""
    Write-Host "🔄 Next Actions:" -ForegroundColor Cyan
    Write-Host "  • Run regularly: .\dependency-management.ps1 -ScanVulnerabilities" -ForegroundColor Gray
    Write-Host "  • Before releases: .\dependency-management.ps1 -Full" -ForegroundColor Gray
    Write-Host "  • Monitor: Add to CI/CD pipeline for automated scanning" -ForegroundColor Gray
}

# 🚀 Main Execution Flow
try {
    # Always restore packages first
    if ($RestoreOnly -or $ScanVulnerabilities -or $ValidateVersions -or $GenerateReport) {
        if (-not (Invoke-PackageRestore)) {
            Write-Host "❌ Cannot proceed without successful package restoration" -ForegroundColor Red
            exit 1
        }
    }

    # Execute requested operations
    if ($ScanVulnerabilities) {
        Invoke-VulnerabilityScanning
    }

    if ($ValidateVersions) {
        Test-VersionPinning
    }

    if ($GenerateReport) {
        New-DependencyReport
    }

    if ($UpdatePackages) {
        Update-Packages
    }

    # Show summary if any operations were performed
    if ($ScanVulnerabilities -or $ValidateVersions -or $GenerateReport -or $UpdatePackages -or $RestoreOnly) {
        Show-Summary
    }
    else {
        # Show help if no parameters provided
        Write-Host "🚌 BusBuddy Dependency Management" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "Available operations:" -ForegroundColor Yellow
        Write-Host "  -RestoreOnly          : Restore NuGet packages only" -ForegroundColor Gray
        Write-Host "  -ScanVulnerabilities  : Check for security vulnerabilities" -ForegroundColor Gray
        Write-Host "  -ValidateVersions     : Validate version pinning consistency" -ForegroundColor Gray
        Write-Host "  -GenerateReport       : Create detailed dependency report" -ForegroundColor Gray
        Write-Host "  -UpdatePackages       : Show update guidance (manual process)" -ForegroundColor Gray
        Write-Host "  -Full                 : Run all checks" -ForegroundColor Gray
        Write-Host ""
        Write-Host "Examples:" -ForegroundColor Cyan
        Write-Host "  .\dependency-management.ps1 -RestoreOnly" -ForegroundColor Gray
        Write-Host "  .\dependency-management.ps1 -ScanVulnerabilities" -ForegroundColor Gray
        Write-Host "  .\dependency-management.ps1 -Full" -ForegroundColor Gray
    }

}
catch {
    Write-Host "❌ Critical error in dependency management: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "✅ Dependency management complete!" -ForegroundColor Green
