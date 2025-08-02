# Database.ps1 - Database-related functions for BusBuddy
# Microsoft PowerShell dot-source script

function Invoke-BusBuddyMigration {
    <#
    .SYNOPSIS
    Apply Entity Framework migrations
    .DESCRIPTION
    Runs dotnet ef database update for BusBuddy
    .EXAMPLE
    Invoke-BusBuddyMigration
    #>
    [CmdletBinding()]
    param()

    try {
        Write-Host "🗄️ Applying database migrations..." -ForegroundColor Cyan
        Set-Location "$BusBuddyWorkspace\BusBuddy.Core"
        dotnet ef database update
        Set-Location $BusBuddyWorkspace
        Write-Host "✅ Migrations applied!" -ForegroundColor Green
    }
    catch {
        Write-Error "Migration error: $($_.Exception.Message)"
        Set-Location $BusBuddyWorkspace
    }
}

function Reset-BusBuddyDatabase {
    <#
    .SYNOPSIS
    Reset the BusBuddy database
    .DESCRIPTION
    Drops and recreates the BusBuddy database
    .EXAMPLE
    Reset-BusBuddyDatabase
    #>
    [CmdletBinding()]
    param()

    try {
        Write-Host "🗄️ Resetting database..." -ForegroundColor Yellow
        Set-Location "$BusBuddyWorkspace\BusBuddy.Core"
        dotnet ef database drop --force
        dotnet ef database update
        Set-Location $BusBuddyWorkspace
        Write-Host "✅ Database reset complete!" -ForegroundColor Green
    }
    catch {
        Write-Error "Database reset error: $($_.Exception.Message)"
        Set-Location $BusBuddyWorkspace
    }
}

function Get-BusBuddyMigrations {
    <#
    .SYNOPSIS
    List Entity Framework migrations
    .DESCRIPTION
    Shows all available migrations for BusBuddy
    .EXAMPLE
    Get-BusBuddyMigrations
    #>
    [CmdletBinding()]
    param()

    try {
        Write-Host "📋 Listing migrations..." -ForegroundColor Cyan
        Set-Location "$BusBuddyWorkspace\BusBuddy.Core"
        dotnet ef migrations list
        Set-Location $BusBuddyWorkspace
    }
    catch {
        Write-Error "Migration list error: $($_.Exception.Message)"
        Set-Location $BusBuddyWorkspace
    }
}

Write-Verbose "Database functions loaded: Invoke-BusBuddyMigration, Reset-BusBuddyDatabase, Get-BusBuddyMigrations"
