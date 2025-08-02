# Build.ps1 - Build-related functions for BusBuddy
# Microsoft PowerShell dot-source script

function Invoke-BusBuddyBuild {
    <#
    .SYNOPSIS
    Build the BusBuddy solution
    .DESCRIPTION
    Executes dotnet build on the BusBuddy solution with error handling
    .PARAMETER Clean
    Perform a clean build
    .EXAMPLE
    Invoke-BusBuddyBuild
    Invoke-BusBuddyBuild -Clean
    #>
    [CmdletBinding()]
    param(
        [switch]$Clean
    )

    try {
        Write-Host "🏗️ Building BusBuddy..." -ForegroundColor Cyan

        if ($Clean) {
            Write-Host "🧹 Cleaning first..." -ForegroundColor Yellow
            dotnet clean "$BusBuddyWorkspace\BusBuddy.sln"
        }

        $result = dotnet build "$BusBuddyWorkspace\BusBuddy.sln"

        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Build successful!" -ForegroundColor Green
        } else {
            Write-Host "❌ Build failed!" -ForegroundColor Red
        }

        return $result
    }
    catch {
        Write-Error "Build error: $($_.Exception.Message)"
    }
}

function Invoke-BusBuddyRun {
    <#
    .SYNOPSIS
    Run the BusBuddy WPF application
    .DESCRIPTION
    Executes dotnet run on the BusBuddy WPF project
    .EXAMPLE
    Invoke-BusBuddyRun
    #>
    [CmdletBinding()]
    param()

    try {
        Write-Host "🚀 Starting BusBuddy application..." -ForegroundColor Cyan
        dotnet run --project "$BusBuddyWorkspace\BusBuddy.WPF\BusBuddy.WPF.csproj"
    }
    catch {
        Write-Error "Run error: $($_.Exception.Message)"
    }
}

function Invoke-BusBuddyTest {
    <#
    .SYNOPSIS
    Run BusBuddy tests
    .DESCRIPTION
    Executes dotnet test on the BusBuddy solution
    .EXAMPLE
    Invoke-BusBuddyTest
    #>
    [CmdletBinding()]
    param()

    try {
        Write-Host "🧪 Running BusBuddy tests..." -ForegroundColor Cyan
        dotnet test "$BusBuddyWorkspace\BusBuddy.sln"
    }
    catch {
        Write-Error "Test error: $($_.Exception.Message)"
    }
}

Write-Verbose "Build functions loaded: Invoke-BusBuddyBuild, Invoke-BusBuddyRun, Invoke-BusBuddyTest"
