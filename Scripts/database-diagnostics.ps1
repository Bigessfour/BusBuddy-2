# BusBuddy Database Diagnostics and Migration Management
# Based on the comprehensive guide provided

param(
    [switch]$DiagnoseConnection,
    [switch]$TestConnection,
    [switch]$AddMigration,
    [string]$MigrationName,
    [switch]$UpdateDatabase,
    [switch]$FullDiagnostic,
    [switch]$CheckMigrations,
    [switch]$SeedData,
    [switch]$Verbose,
    [string]$Context = "BusBuddyContext"
)

# Set location to project root
Set-Location $PSScriptRoot\..

function Write-DiagnosticHeader {
    param([string]$Title)
    Write-Host "`n=== $Title ===" -ForegroundColor Cyan
}

function Test-DatabaseConnection {
    Write-DiagnosticHeader "Database Connection Test"

    try {
        # Read connection string from appsettings.json
        $appsettings = Get-Content "BusBuddy.WPF\appsettings.json" | ConvertFrom-Json
        $connectionString = $appsettings.ConnectionStrings.DefaultConnection

        Write-Host "Connection String: $connectionString" -ForegroundColor Yellow

        # Test SQLite database file existence
        if ($connectionString -like "*Data Source=*") {
            $dbFile = ($connectionString -split "Data Source=")[1].Split(";")[0]
            $dbPath = Join-Path (Get-Location) "BusBuddy.WPF\$dbFile"

            Write-Host "Database File Path: $dbPath" -ForegroundColor Yellow

            if (Test-Path $dbPath) {
                Write-Host "✅ Database file exists" -ForegroundColor Green
                $fileInfo = Get-Item $dbPath
                Write-Host "   Size: $([math]::Round($fileInfo.Length / 1KB, 2)) KB" -ForegroundColor Gray
                Write-Host "   Modified: $($fileInfo.LastWriteTime)" -ForegroundColor Gray
            } else {
                Write-Host "❌ Database file does not exist" -ForegroundColor Red
                Write-Host "   Will be created on first migration/run" -ForegroundColor Yellow
            }
        }

        # Test EF Core tools availability
        Write-Host "`nTesting EF Core tools..." -ForegroundColor Yellow
        $efResult = dotnet ef --version 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ EF Core tools available: $efResult" -ForegroundColor Green
        } else {
            Write-Host "❌ EF Core tools not available" -ForegroundColor Red
            Write-Host "   Install with: dotnet tool install --global dotnet-ef" -ForegroundColor Yellow
        }

    } catch {
        Write-Host "❌ Connection test failed: $_" -ForegroundColor Red
    }
}

function Get-MigrationStatus {
    Write-DiagnosticHeader "Migration Status"

    try {
        # Check for existing migrations
        $migrationsPath = "BusBuddy.Core\Migrations"
        if (Test-Path $migrationsPath) {
            $migrations = Get-ChildItem $migrationsPath -Filter "*.cs" | Where-Object { $_.Name -notlike "*Designer.cs" }
            Write-Host "Found $($migrations.Count) migrations:" -ForegroundColor Yellow
            foreach ($migration in $migrations) {
                Write-Host "  - $($migration.BaseName)" -ForegroundColor Gray
            }
        } else {
            Write-Host "❌ No migrations folder found" -ForegroundColor Red
            Write-Host "   Run 'Add-Migration InitialCreate' to create first migration" -ForegroundColor Yellow
        }

        # Check database migration status
        Write-Host "`nChecking applied migrations..." -ForegroundColor Yellow
        
        # Try with BusBuddyContext first (Phase 1 primary context)
        Write-Host "Checking migrations for BusBuddyContext..." -ForegroundColor Gray
        $dbUpdateCheck = dotnet ef migrations list --project BusBuddy.Core --startup-project BusBuddy.WPF --context BusBuddyContext 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ BusBuddyContext migration status:" -ForegroundColor Green
            Write-Host $dbUpdateCheck -ForegroundColor Gray
        } else {
            Write-Host "⚠️ BusBuddyContext migration issues:" -ForegroundColor Yellow
            Write-Host $dbUpdateCheck -ForegroundColor Gray
        }
        
        # Also check BusBuddyDbContext
        Write-Host "`nChecking migrations for BusBuddyDbContext..." -ForegroundColor Gray
        $dbUpdateCheck2 = dotnet ef migrations list --project BusBuddy.Core --startup-project BusBuddy.WPF --context BusBuddyDbContext 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ BusBuddyDbContext migration status:" -ForegroundColor Green
            Write-Host $dbUpdateCheck2 -ForegroundColor Gray
        } else {
            Write-Host "⚠️ BusBuddyDbContext migration issues:" -ForegroundColor Yellow
            Write-Host $dbUpdateCheck2 -ForegroundColor Gray
        }

    } catch {
        Write-Host "❌ Migration status check failed: $_" -ForegroundColor Red
    }
}

function Add-DatabaseMigration {
    param(
        [string]$Name,
        [string]$Context = "BusBuddyContext"
    )

    if (-not $Name) {
        $Name = Read-Host "Enter migration name"
    }

    Write-DiagnosticHeader "Adding Migration: $Name (Context: $Context)"

    try {
        Write-Host "Using DbContext: $Context" -ForegroundColor Yellow
        $result = dotnet ef migrations add $Name --project BusBuddy.Core --startup-project BusBuddy.WPF --context $Context --verbose
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Migration '$Name' added successfully for $Context" -ForegroundColor Green
        } else {
            Write-Host "❌ Migration failed for $Context" -ForegroundColor Red
            Write-Host $result -ForegroundColor Gray
        }
    } catch {
        Write-Host "❌ Migration add failed: $_" -ForegroundColor Red
    }
}

function Update-DatabaseSchema {
    param([string]$Context = "BusBuddyContext")
    
    Write-DiagnosticHeader "Updating Database (Context: $Context)"

    try {
        Write-Host "Using DbContext: $Context" -ForegroundColor Yellow
        $result = dotnet ef database update --project BusBuddy.Core --startup-project BusBuddy.WPF --context $Context --verbose
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Database updated successfully for $Context" -ForegroundColor Green
        } else {
            Write-Host "❌ Database update failed for $Context" -ForegroundColor Red
            Write-Host $result -ForegroundColor Gray
        }
    } catch {
        Write-Host "❌ Database update failed: $_" -ForegroundColor Red
    }
}

function Invoke-FullDatabaseDiagnostic {
    Write-Host "`n🔍 BusBuddy Database Diagnostic Report" -ForegroundColor Cyan
    Write-Host "Generated: $(Get-Date)" -ForegroundColor Gray

    # Test connection
    Test-DatabaseConnection

    # Check migrations
    Get-MigrationStatus

    # Check EF configuration
    Write-DiagnosticHeader "Entity Framework Configuration"

    # Check for DbContext in BusBuddy.Core
    $dbContextFiles = Get-ChildItem "BusBuddy.Core" -Recurse -Filter "*Context.cs"
    Write-Host "Found DbContext files:" -ForegroundColor Yellow
    foreach ($file in $dbContextFiles) {
        Write-Host "  - $($file.Name)" -ForegroundColor Gray
    }

    # Check model classes
    $modelFiles = Get-ChildItem "BusBuddy.Core\Models" -Filter "*.cs" -ErrorAction SilentlyContinue
    if ($modelFiles) {
        Write-Host "`nFound model classes:" -ForegroundColor Yellow
        foreach ($file in $modelFiles) {
            Write-Host "  - $($file.BaseName)" -ForegroundColor Gray
        }
    }

    # Check packages
    Write-DiagnosticHeader "Package Dependencies"
    $coreProject = Get-Content "BusBuddy.Core\BusBuddy.Core.csproj"
    $efPackages = $coreProject | Select-String "Microsoft.EntityFrameworkCore"
    Write-Host "EF Core packages:" -ForegroundColor Yellow
    $efPackages | ForEach-Object { Write-Host "  - $($_.Line.Trim())" -ForegroundColor Gray }
}

function Import-RealWorldData {
    Write-DiagnosticHeader "Importing Real-World Data"

    # Check if seed script exists
    $seedScript = "Scripts\seed-realworld-data.ps1"
    if (Test-Path $seedScript) {
        Write-Host "Running seed script..." -ForegroundColor Yellow
        & $seedScript
    } else {
        Write-Host "❌ Seed script not found at $seedScript" -ForegroundColor Red
        Write-Host "   Create seed script or run manual seeding" -ForegroundColor Yellow
    }
}

# Main execution logic
if ($FullDiagnostic) {
    Invoke-FullDatabaseDiagnostic
} elseif ($TestConnection) {
    Test-DatabaseConnection
} elseif ($CheckMigrations) {
    Get-MigrationStatus
} elseif ($AddMigration) {
    Add-DatabaseMigration -Name $MigrationName -Context $Context
} elseif ($UpdateDatabase) {
    Update-DatabaseSchema -Context $Context
} elseif ($SeedData) {
    Import-RealWorldData
} else {
    Write-Host "BusBuddy Database Diagnostics and Migration Management" -ForegroundColor Cyan
    Write-Host "Usage examples:" -ForegroundColor Yellow
    Write-Host "  .\database-diagnostics.ps1 -FullDiagnostic" -ForegroundColor Gray
    Write-Host "  .\database-diagnostics.ps1 -TestConnection" -ForegroundColor Gray
    Write-Host "  .\database-diagnostics.ps1 -CheckMigrations" -ForegroundColor Gray
    Write-Host "  .\database-diagnostics.ps1 -AddMigration -MigrationName 'InitialCreate' -Context 'BusBuddyContext'" -ForegroundColor Gray
    Write-Host "  .\database-diagnostics.ps1 -UpdateDatabase -Context 'BusBuddyContext'" -ForegroundColor Gray
    Write-Host "  .\database-diagnostics.ps1 -SeedData" -ForegroundColor Gray
    Write-Host "`nAvailable Contexts: BusBuddyContext (Phase 1), BusBuddyDbContext (Full)" -ForegroundColor Cyan
}
