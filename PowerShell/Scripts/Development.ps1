# Development.ps1 - Development workflow functions for BusBuddy
# Microsoft PowerShell dot-source script

function Start-BusBuddyDevSession {
    <#
    .SYNOPSIS
    Start a complete development session
    .DESCRIPTION
    Initializes a full BusBuddy development environment
    .EXAMPLE
    Start-BusBuddyDevSession
    #>
    [CmdletBinding()]
    param()

    try {
        Write-Host "🚀 Starting BusBuddy Development Session" -ForegroundColor Cyan
        Write-Host "=======================================" -ForegroundColor Cyan

        # Health check first
        Get-BusBuddyHealth

        # Restore packages
        Write-Host "📦 Restoring NuGet packages..." -ForegroundColor Yellow
        dotnet restore "$BusBuddyWorkspace\BusBuddy.sln"

        # Build solution
        Write-Host "🏗️ Building solution..." -ForegroundColor Yellow
        Invoke-BusBuddyBuild

        # Run tests
        Write-Host "🧪 Running tests..." -ForegroundColor Yellow
        Invoke-BusBuddyTest

        Write-Host "✅ Development session ready!" -ForegroundColor Green
    }
    catch {
        Write-Error "Dev session error: $($_.Exception.Message)"
    }
}

function Invoke-BusBuddyClean {
    <#
    .SYNOPSIS
    Clean the BusBuddy solution
    .DESCRIPTION
    Performs dotnet clean on the solution and removes build artifacts
    .EXAMPLE
    Invoke-BusBuddyClean
    #>
    [CmdletBinding()]
    param()

    try {
        Write-Host "🧹 Cleaning BusBuddy solution..." -ForegroundColor Cyan
        dotnet clean "$BusBuddyWorkspace\BusBuddy.sln"

        # Clean additional artifacts
        $artifactPaths = @("bin", "obj", "TestResults")
        foreach ($artifact in $artifactPaths) {
            Get-ChildItem -Path $BusBuddyWorkspace -Recurse -Directory -Name $artifact -ErrorAction SilentlyContinue |
                ForEach-Object {
                    $fullPath = Join-Path $BusBuddyWorkspace $_
                    Write-Host "🗑️ Removing: $fullPath" -ForegroundColor Gray
                    Remove-Item $fullPath -Recurse -Force -ErrorAction SilentlyContinue
                }
        }

        Write-Host "✅ Clean complete!" -ForegroundColor Green
    }
    catch {
        Write-Error "Clean error: $($_.Exception.Message)"
    }
}

function Invoke-BusBuddyRestore {
    <#
    .SYNOPSIS
    Restore NuGet packages
    .DESCRIPTION
    Performs dotnet restore on the BusBuddy solution
    .EXAMPLE
    Invoke-BusBuddyRestore
    #>
    [CmdletBinding()]
    param()

    try {
        Write-Host "📦 Restoring NuGet packages..." -ForegroundColor Cyan
        dotnet restore "$BusBuddyWorkspace\BusBuddy.sln" --force
        Write-Host "✅ Restore complete!" -ForegroundColor Green
    }
    catch {
        Write-Error "Restore error: $($_.Exception.Message)"
    }
}

function Open-BusBuddyInVSCode {
    <#
    .SYNOPSIS
    Open BusBuddy workspace in VS Code
    .DESCRIPTION
    Opens the BusBuddy workspace in Visual Studio Code
    .EXAMPLE
    Open-BusBuddyInVSCode
    #>
    [CmdletBinding()]
    param()

    try {
        Write-Host "💻 Opening BusBuddy in VS Code..." -ForegroundColor Cyan
        code $BusBuddyWorkspace
    }
    catch {
        Write-Error "VS Code launch error: $($_.Exception.Message)"
    }
}

Write-Verbose "Development functions loaded: Start-BusBuddyDevSession, Invoke-BusBuddyClean, Invoke-BusBuddyRestore, Open-BusBuddyInVSCode"
