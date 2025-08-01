#Requires -Version 7.5

<#
.SYNOPSIS
    Utility functions for BusBuddy
.DESCRIPTION
    Contains utility functions for the BusBuddy development environment
.NOTES
    File Name      : Utilities.ps1
    Author         : BusBuddy Development Team
    Prerequisite   : PowerShell 7.5.2
    Copyright 2025 - BusBuddy
#>

function global:Get-BusBuddyEnvironment {
    param (
        [Parameter(Mandatory = $false)]
        [switch]$List,

        [Parameter(Mandatory = $false)]
        [string]$Set,

        [Parameter(Mandatory = $false)]
        [string]$Value,

        [Parameter(Mandatory = $false)]
        [switch]$Reset
    )

    $busBuddyEnvVars = @(
        "BUSBUDDY_PS_LOADED",
        "BUSBUDDY_ENVIRONMENT",
        "BUSBUDDY_DB_PROVIDER",
        "BUSBUDDY_CONNECTION_STRING",
        "BUSBUDDY_LOG_LEVEL"
    )

    if ($List) {
        Write-Host "🔍 BusBuddy Environment Variables:" -ForegroundColor Cyan
        Write-Host "=================================" -ForegroundColor Cyan

        foreach ($var in $busBuddyEnvVars) {
            $value = [Environment]::GetEnvironmentVariable($var)
            $color = if ($null -eq $value) { "Gray" } else { "Green" }
            Write-Host "$var = " -NoNewline
            Write-Host "$value" -ForegroundColor $color
        }

        return
    }

    if ($Set -and $Value) {
        if ($busBuddyEnvVars -contains $Set) {
            [Environment]::SetEnvironmentVariable($Set, $Value, "Process")
            Write-Host "✅ Set $Set = $Value" -ForegroundColor Green
        } else {
            Write-Warning "Unknown BusBuddy environment variable: $Set"
            Write-Host "Valid variables are: $($busBuddyEnvVars -join ', ')" -ForegroundColor Yellow
        }

        return
    }

    if ($Reset) {
        Write-Host "🔄 Resetting BusBuddy environment variables..." -ForegroundColor Yellow

        foreach ($var in $busBuddyEnvVars) {
            if ($var -ne "BUSBUDDY_PS_LOADED") {
                [Environment]::SetEnvironmentVariable($var, $null, "Process")
            }
        }

        # Set default values
        [Environment]::SetEnvironmentVariable("BUSBUDDY_ENVIRONMENT", "Development", "Process")

        Write-Host "✅ Environment variables reset to defaults" -ForegroundColor Green

        return
    }

    # Default behavior - show help
    Write-Host "❓ Get-BusBuddyEnvironment command usage:" -ForegroundColor Cyan
    Write-Host "  Get-BusBuddyEnvironment -List                 # List all BusBuddy environment variables" -ForegroundColor Yellow
    Write-Host "  Get-BusBuddyEnvironment -Set VAR VALUE        # Set a BusBuddy environment variable" -ForegroundColor Yellow
    Write-Host "  Get-BusBuddyEnvironment -Reset                # Reset environment variables to defaults" -ForegroundColor Yellow
}

function global:Invoke-BusBuddyTest {
    param (
        [Parameter(Mandatory = $false)]
        [string]$Filter,

        [Parameter(Mandatory = $false)]
        [string]$Category,

        [Parameter(Mandatory = $false)]
        [switch]$NoBuild
    )

    Write-Host "🧪 Running BusBuddy tests..." -ForegroundColor Cyan

    if (-not $NoBuild) {
        Write-Host "🔨 Building solution first..." -ForegroundColor Yellow
        Invoke-BusBuddyBuild

        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Build failed, cannot run tests" -ForegroundColor Red
            return
        }
    }

    $testArgs = @("test", "BusBuddy.sln")

    if ($Filter) {
        $testArgs += "--filter"
        $testArgs += $Filter
    }

    if ($Category) {
        if ($Filter) {
            # If we already have a filter, we need to combine them
            $testArgs[-1] = "$($testArgs[-1])&Category=$Category"
        } else {
            $testArgs += "--filter"
            $testArgs += "Category=$Category"
        }
    }

    & dotnet $testArgs

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Tests completed successfully" -ForegroundColor Green
    } else {
        Write-Host "❌ Tests failed with exit code: $LASTEXITCODE" -ForegroundColor Red
    }
}

function global:Start-BusBuddyDevSession {
    param (
        [Parameter(Mandatory = $false)]
        [switch]$OpenIDE,

        [Parameter(Mandatory = $false)]
        [switch]$InitDB,

        [Parameter(Mandatory = $false)]
        [switch]$UpdatePackages
    )

    Write-Host "🚀 Starting BusBuddy development session..." -ForegroundColor Cyan

    # Run health check
    Get-BusBuddyHealth -Quick

    # Initialize database if requested
    if ($InitDB) {
        Write-Host "🔧 Initializing database..." -ForegroundColor Yellow
        bb-db-setup -LocalDB -CreateIfNotExists
    }

    # Update packages if requested
    if ($UpdatePackages) {
        Write-Host "📦 Updating NuGet packages..." -ForegroundColor Yellow
        dotnet restore --force
        Write-Host "✅ Packages updated" -ForegroundColor Green
    }

    # Open IDE if requested
    if ($OpenIDE) {
        Write-Host "🔧 Opening IDE..." -ForegroundColor Yellow

        # Try to find the best IDE to open
        if (Get-Command code -ErrorAction SilentlyContinue) {
            Start-Process -FilePath code -ArgumentList "."
        } elseif (Test-Path "C:\Program Files\Microsoft Visual Studio\2022\*\Common7\IDE\devenv.exe") {
            $devenv = Get-ChildItem "C:\Program Files\Microsoft Visual Studio\2022\*\Common7\IDE\devenv.exe" | Select-Object -First 1
            Start-Process -FilePath $devenv.FullName -ArgumentList "BusBuddy.sln"
        } elseif (Test-Path "C:\Program Files (x86)\Microsoft Visual Studio\2019\*\Common7\IDE\devenv.exe") {
            $devenv = Get-ChildItem "C:\Program Files (x86)\Microsoft Visual Studio\2019\*\Common7\IDE\devenv.exe" | Select-Object -First 1
            Start-Process -FilePath $devenv.FullName -ArgumentList "BusBuddy.sln"
        } else {
            Write-Warning "Could not find Visual Studio or VS Code to open. Please open your IDE manually."
        }
    }

    Write-Host "✅ Development session initialized" -ForegroundColor Green
    Write-Host "Ready to start coding! Use bb-build and bb-run to build and run the application." -ForegroundColor Yellow
}

# Export functions
Export-ModuleMember -Function Get-BusBuddyEnvironment, Invoke-BusBuddyTest, Start-BusBuddyDevSession

# Create aliases for backward compatibility
New-Alias -Name bb-environment -Value Get-BusBuddyEnvironment -Force
New-Alias -Name bb-test -Value Invoke-BusBuddyTest -Force
New-Alias -Name bb-dev-session -Value Start-BusBuddyDevSession -Force
Export-ModuleMember -Alias bb-environment, bb-test, bb-dev-session
