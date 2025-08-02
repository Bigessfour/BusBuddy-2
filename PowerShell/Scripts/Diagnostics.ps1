# Diagnostics.ps1 - Diagnostic functions for BusBuddy
# Microsoft PowerShell dot-source script

function Get-BusBuddyHealth {
    <#
    .SYNOPSIS
    Check BusBuddy project health
    .DESCRIPTION
    Performs comprehensive health check of the BusBuddy project
    .EXAMPLE
    Get-BusBuddyHealth
    #>
    [CmdletBinding()]
    param()

    try {
        Write-Host "🏥 BusBuddy Health Check" -ForegroundColor Cyan
        Write-Host "========================" -ForegroundColor Cyan

        # Check workspace
        Write-Host "📂 Workspace: " -NoNewline
        if (Test-Path "$BusBuddyWorkspace\BusBuddy.sln") {
            Write-Host "✅ Valid" -ForegroundColor Green
        } else {
            Write-Host "❌ Invalid" -ForegroundColor Red
        }

        # Check projects
        $projects = @("BusBuddy.Core", "BusBuddy.WPF", "BusBuddy.Tests")
        foreach ($project in $projects) {
            Write-Host "📁 $($project): " -NoNewline
            if (Test-Path "$BusBuddyWorkspace\$project\$project.csproj") {
                Write-Host "✅ Found" -ForegroundColor Green
            } else {
                Write-Host "❌ Missing" -ForegroundColor Red
            }
        }

        # Check settings
        Write-Host "⚙️ Settings: " -NoNewline
        if ($global:BusBuddySettings) {
            Write-Host "✅ Loaded" -ForegroundColor Green
        } else {
            Write-Host "❌ Not loaded" -ForegroundColor Red
        }

        # Check .NET SDK
        Write-Host "🛠️ .NET SDK: " -NoNewline
        $dotnetVersion = dotnet --version 2>$null
        if ($dotnetVersion) {
            Write-Host "✅ $dotnetVersion" -ForegroundColor Green
        } else {
            Write-Host "❌ Not found" -ForegroundColor Red
        }

        Write-Host "========================" -ForegroundColor Cyan
    }
    catch {
        Write-Error "Health check error: $($_.Exception.Message)"
    }
}

function Get-BusBuddyErrors {
    <#
    .SYNOPSIS
    Check for build errors
    .DESCRIPTION
    Analyzes the solution for compilation errors
    .EXAMPLE
    Get-BusBuddyErrors
    #>
    [CmdletBinding()]
    param()

    try {
        Write-Host "🔍 Checking for build errors..." -ForegroundColor Cyan
        $buildOutput = dotnet build "$BusBuddyWorkspace\BusBuddy.sln" --verbosity quiet 2>&1

        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ No build errors found!" -ForegroundColor Green
        } else {
            Write-Host "❌ Build errors detected:" -ForegroundColor Red
            $buildOutput | Where-Object { $_ -match "error" } | ForEach-Object {
                Write-Host "  $_" -ForegroundColor Red
            }
        }
    }
    catch {
        Write-Error "Error check failed: $($_.Exception.Message)"
    }
}

function Test-BusBuddyEnvironment {
    <#
    .SYNOPSIS
    Test the development environment
    .DESCRIPTION
    Validates the complete BusBuddy development environment
    .EXAMPLE
    Test-BusBuddyEnvironment
    #>
    [CmdletBinding()]
    param()

    try {
        Write-Host "🧪 Environment Test" -ForegroundColor Cyan

        # Test PowerShell version
        if ($PSVersionTable.PSVersion.Major -ge 7) {
            Write-Host "✅ PowerShell 7+ detected" -ForegroundColor Green
        } else {
            Write-Host "⚠️ PowerShell 7+ recommended" -ForegroundColor Yellow
        }

        # Test execution policy
        $policy = Get-ExecutionPolicy -Scope CurrentUser
        if ($policy -in @('RemoteSigned', 'Unrestricted')) {
            Write-Host "✅ Execution policy: $policy" -ForegroundColor Green
        } else {
            Write-Host "⚠️ Execution policy may need adjustment: $policy" -ForegroundColor Yellow
        }

        # Test module availability
        if (Get-Command bb-build -ErrorAction SilentlyContinue) {
            Write-Host "✅ BusBuddy commands available" -ForegroundColor Green
        } else {
            Write-Host "❌ BusBuddy commands not found" -ForegroundColor Red
        }

        Write-Host "Environment test complete" -ForegroundColor Cyan
    }
    catch {
        Write-Error "Environment test error: $($_.Exception.Message)"
    }
}

Write-Verbose "Diagnostic functions loaded: Get-BusBuddyHealth, Get-BusBuddyErrors, Test-BusBuddyEnvironment"
