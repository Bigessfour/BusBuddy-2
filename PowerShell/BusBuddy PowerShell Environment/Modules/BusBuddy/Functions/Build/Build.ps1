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

function global:bb-build {
    param (
        [Parameter(Mandatory = $false)]
        [switch]$Clean,

        [Parameter(Mandatory = $false)]
        [switch]$Verbose,

        [Parameter(Mandatory = $false)]
        [string]$Configuration = "Debug"
    )

    Write-Host "🔨 Building BusBuddy solution..." -ForegroundColor Cyan

    if ($Clean) {
        Write-Host "🧹 Cleaning solution first..." -ForegroundColor Yellow
        dotnet clean "BusBuddy.sln" --configuration $Configuration --verbosity minimal
    }

    $verbosityLevel = if ($Verbose) { "detailed" } else { "minimal" }

    dotnet build "BusBuddy.sln" --configuration $Configuration --verbosity $verbosityLevel

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Build completed successfully" -ForegroundColor Green
    } else {
        Write-Host "❌ Build failed with exit code: $LASTEXITCODE" -ForegroundColor Red
    }
}

function global:bb-run {
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
        bb-build -Configuration $Configuration

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

function global:bb-clean {
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

# Export functions
Export-ModuleMember -Function bb-build, bb-run, bb-clean
