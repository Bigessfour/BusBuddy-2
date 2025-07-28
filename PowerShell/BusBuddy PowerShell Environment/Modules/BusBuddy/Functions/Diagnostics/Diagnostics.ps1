#Requires -Version 7.5

<#
.SYNOPSIS
    Diagnostic-related functions for BusBuddy
.DESCRIPTION
    Contains functions for health checks and diagnostics
.NOTES
    File Name      : Diagnostics.ps1
    Author         : BusBuddy Development Team
    Prerequisite   : PowerShell 7.5.2
    Copyright 2025 - BusBuddy
#>

function global:bb-health {
    param (
        [Parameter(Mandatory = $false)]
        [switch]$Quick,

        [Parameter(Mandatory = $false)]
        [switch]$Verbose,

        [Parameter(Mandatory = $false)]
        [string]$Export
    )

    Write-Host "🩺 Running BusBuddy health check..." -ForegroundColor Cyan

    # Basic health checks
    $results = @{}

    # Check PowerShell version
    $psVersion = $PSVersionTable.PSVersion
    $psVersionOk = $psVersion.Major -ge 7 -and $psVersion.Minor -ge 5
    $results["PowerShell"] = @{
        "Version" = "$psVersion"
        "Status" = if ($psVersionOk) { "OK" } else { "WARNING" }
        "Message" = if ($psVersionOk) { "PowerShell 7.5+ detected" } else { "PowerShell 7.5+ recommended" }
    }

    # Check .NET SDK
    try {
        $dotnetVersion = (dotnet --version)
        $dotnetVersionOk = $dotnetVersion.StartsWith("8.")
        $results[".NET SDK"] = @{
            "Version" = "$dotnetVersion"
            "Status" = if ($dotnetVersionOk) { "OK" } else { "WARNING" }
            "Message" = if ($dotnetVersionOk) { ".NET 8 SDK detected" } else { ".NET 8 SDK recommended" }
        }
    } catch {
        $results[".NET SDK"] = @{
            "Version" = "Unknown"
            "Status" = "ERROR"
            "Message" = "Could not detect .NET SDK"
        }
    }

    # Check if solution file exists
    $solutionExists = Test-Path "BusBuddy.sln"
    $results["Solution File"] = @{
        "Status" = if ($solutionExists) { "OK" } else { "ERROR" }
        "Message" = if ($solutionExists) { "BusBuddy.sln found" } else { "BusBuddy.sln not found" }
    }

    # Check if project files exist
    $coreProjectExists = Test-Path "BusBuddy.Core/BusBuddy.Core.csproj"
    $wpfProjectExists = Test-Path "BusBuddy.WPF/BusBuddy.WPF.csproj"
    $results["Project Files"] = @{
        "Status" = if ($coreProjectExists -and $wpfProjectExists) { "OK" } else { "ERROR" }
        "Message" = if ($coreProjectExists -and $wpfProjectExists) {
            "Core and WPF projects found"
        } else {
            "Missing project files"
        }
    }

    # Check database connection
    $dbConfigExists = Test-Path "BusBuddy.Core/appsettings.json"
    $results["Database Config"] = @{
        "Status" = if ($dbConfigExists) { "OK" } else { "WARNING" }
        "Message" = if ($dbConfigExists) { "Database configuration found" } else { "Database configuration missing" }
    }

    # Output results
    Write-Host ""
    Write-Host "Health Check Results:" -ForegroundColor Cyan
    Write-Host "=====================" -ForegroundColor Cyan

    $overallStatus = "OK"

    foreach ($key in $results.Keys) {
        $item = $results[$key]
        $statusColor = switch ($item.Status) {
            "OK" { "Green" }
            "WARNING" { "Yellow" }
            "ERROR" { "Red" }
            default { "White" }
        }

        if ($item.Status -eq "ERROR") {
            $overallStatus = "ERROR"
        } elseif ($item.Status -eq "WARNING" -and $overallStatus -ne "ERROR") {
            $overallStatus = "WARNING"
        }

        Write-Host "$key : " -NoNewline
        Write-Host $item.Status -ForegroundColor $statusColor -NoNewline

        if ($item.ContainsKey("Version")) {
            Write-Host " ($($item.Version))" -NoNewline
        }

        Write-Host " - $($item.Message)"
    }

    Write-Host ""
    Write-Host "Overall Status: " -NoNewline
    $overallStatusColor = switch ($overallStatus) {
        "OK" { "Green" }
        "WARNING" { "Yellow" }
        "ERROR" { "Red" }
        default { "White" }
    }
    Write-Host $overallStatus -ForegroundColor $overallStatusColor

    # Export results if requested
    if ($Export) {
        $results | ConvertTo-Json -Depth 5 | Set-Content -Path $Export
        Write-Host "Results exported to: $Export" -ForegroundColor Cyan
    }

    return $results
}

function global:bb-diagnostic {
    param (
        [Parameter(Mandatory = $false)]
        [switch]$Full,

        [Parameter(Mandatory = $false)]
        [switch]$CheckDependencies,

        [Parameter(Mandatory = $false)]
        [string]$Export
    )

    Write-Host "🔍 Running BusBuddy diagnostics..." -ForegroundColor Cyan

    $diagnosticResults = @{}

    # Basic diagnostics
    $diagnosticResults["BasicHealth"] = bb-health -Quick

    # More detailed diagnostics if requested
    if ($Full) {
        # Check for project structure issues
        Write-Host "Analyzing project structure..." -ForegroundColor Yellow
        $diagnosticResults["ProjectStructure"] = @{
            "MissingFiles" = @()
            "EmptyDirectories" = @()
            "Status" = "OK"
        }

        # Check for essential files
        $essentialFiles = @(
            "BusBuddy.sln",
            "BusBuddy.Core/BusBuddy.Core.csproj",
            "BusBuddy.WPF/BusBuddy.WPF.csproj",
            "BusBuddy.Core/appsettings.json",
            "BusBuddy.WPF/App.xaml",
            "BusBuddy.WPF/App.xaml.cs"
        )

        foreach ($file in $essentialFiles) {
            if (-not (Test-Path $file)) {
                $diagnosticResults["ProjectStructure"]["MissingFiles"] += $file
                $diagnosticResults["ProjectStructure"]["Status"] = "ERROR"
            }
        }

        # Check for empty directories
        $directories = Get-ChildItem -Directory -Recurse | Where-Object {
            $_.FullName -notlike "*\bin\*" -and
            $_.FullName -notlike "*\obj\*" -and
            $_.FullName -notlike "*\.git\*"
        }

        foreach ($dir in $directories) {
            if ((Get-ChildItem $dir.FullName -Force -Recurse | Measure-Object).Count -eq 0) {
                $diagnosticResults["ProjectStructure"]["EmptyDirectories"] += $dir.FullName
            }
        }
    }

    # Check dependencies if requested
    if ($CheckDependencies -or $Full) {
        Write-Host "Checking NuGet dependencies..." -ForegroundColor Yellow

        $projectFiles = Get-ChildItem -Recurse -Filter "*.csproj"
        $diagnosticResults["Dependencies"] = @{
            "Projects" = @{}
            "Status" = "OK"
        }

        foreach ($project in $projectFiles) {
            $projectContent = Get-Content $project.FullName -Raw
            $packageRefs = [regex]::Matches($projectContent, '<PackageReference\s+Include="([^"]+)"\s+Version="([^"]+)"')

            $projectDependencies = @{}
            foreach ($match in $packageRefs) {
                $packageName = $match.Groups[1].Value
                $packageVersion = $match.Groups[2].Value
                $projectDependencies[$packageName] = $packageVersion
            }

            $diagnosticResults["Dependencies"]["Projects"][$project.Name] = $projectDependencies
        }
    }

    # Output results
    Write-Host ""
    Write-Host "Diagnostic Results:" -ForegroundColor Cyan
    Write-Host "===================" -ForegroundColor Cyan

    if ($Full) {
        if ($diagnosticResults["ProjectStructure"]["MissingFiles"].Count -gt 0) {
            Write-Host "Missing Essential Files:" -ForegroundColor Red
            foreach ($file in $diagnosticResults["ProjectStructure"]["MissingFiles"]) {
                Write-Host "  - $file" -ForegroundColor Red
            }
        } else {
            Write-Host "Essential Files: " -NoNewline
            Write-Host "OK" -ForegroundColor Green
        }

        if ($diagnosticResults["ProjectStructure"]["EmptyDirectories"].Count -gt 0) {
            Write-Host "Empty Directories:" -ForegroundColor Yellow
            foreach ($dir in $diagnosticResults["ProjectStructure"]["EmptyDirectories"]) {
                Write-Host "  - $dir" -ForegroundColor Yellow
            }
        }
    }

    # Export results if requested
    if ($Export) {
        $diagnosticResults | ConvertTo-Json -Depth 10 | Set-Content -Path $Export
        Write-Host "Diagnostic results exported to: $Export" -ForegroundColor Cyan
    }

    return $diagnosticResults
}

# Export functions
Export-ModuleMember -Function bb-health, bb-diagnostic
