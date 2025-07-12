# BusBuddy Database NULL Fix PowerShell Script
# This script runs the SQL fix against your SQL Server database

param(
    [string]$ServerName = 'localhost\SQLEXPRESS',
    [string]$DatabaseName = 'BusBuddyDb',
    [string]$SqlFile = 'fix_null_values.sql'
)

Write-Host 'BusBuddy Database NULL Fix Utility' -ForegroundColor Green
Write-Host '===================================' -ForegroundColor Green

# Check if SQL Server module is available
if (Get-Module -ListAvailable -Name SqlServer) {
    Import-Module SqlServer
    Write-Host 'Using SqlServer PowerShell module' -ForegroundColor Yellow
} elseif (Get-Module -ListAvailable -Name SQLPS) {
    Import-Module SQLPS -DisableNameChecking
    Write-Host 'Using SQLPS PowerShell module' -ForegroundColor Yellow
} else {
    Write-Host 'SQL Server PowerShell module not found. Using sqlcmd...' -ForegroundColor Yellow

    # Try to run with sqlcmd
    try {
        $sqlcmdPath = Get-Command sqlcmd -ErrorAction Stop
        Write-Host "Found sqlcmd at: $($sqlcmdPath.Source)" -ForegroundColor Green

        $fullSqlPath = Join-Path $PSScriptRoot $SqlFile
        if (-not (Test-Path $fullSqlPath)) {
            Write-Host "SQL file not found: $fullSqlPath" -ForegroundColor Red
            exit 1
        }

        Write-Host "Executing SQL script: $fullSqlPath" -ForegroundColor Yellow
        Write-Host "Server: $ServerName" -ForegroundColor Yellow
        Write-Host "Database: $DatabaseName" -ForegroundColor Yellow

        & sqlcmd -S $ServerName -d $DatabaseName -E -i $fullSqlPath -b

        if ($LASTEXITCODE -eq 0) {
            Write-Host "`nSQL script executed successfully!" -ForegroundColor Green
            Write-Host 'Your SqlNullValueException errors should now be resolved.' -ForegroundColor Green
        } else {
            Write-Host "`nSQL script execution failed with exit code: $LASTEXITCODE" -ForegroundColor Red
        }

    } catch {
        Write-Host 'sqlcmd not found. Please install SQL Server command line tools or SQL Server PowerShell module.' -ForegroundColor Red
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }

    exit $LASTEXITCODE
}

# Try to connect and run the script using PowerShell SQL modules
try {
    Write-Host 'Connecting to SQL Server...' -ForegroundColor Yellow

    $connectionString = "Server=$ServerName;Database=$DatabaseName;Integrated Security=True;TrustServerCertificate=True;"

    $fullSqlPath = Join-Path $PSScriptRoot $SqlFile
    if (-not (Test-Path $fullSqlPath)) {
        Write-Host "SQL file not found: $fullSqlPath" -ForegroundColor Red
        exit 1
    }

    $sqlContent = Get-Content $fullSqlPath -Raw

    Write-Host 'Executing SQL script...' -ForegroundColor Yellow
    Invoke-Sqlcmd -ConnectionString $connectionString -Query $sqlContent -Verbose

    Write-Host "`nSQL script executed successfully!" -ForegroundColor Green
    Write-Host 'Your SqlNullValueException errors should now be resolved.' -ForegroundColor Green

} catch {
    Write-Host "`nError executing SQL script:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red

    # Fallback to sqlcmd if PowerShell modules fail
    Write-Host "`nTrying fallback to sqlcmd..." -ForegroundColor Yellow
    try {
        & sqlcmd -S $ServerName -d $DatabaseName -E -i $fullSqlPath -b

        if ($LASTEXITCODE -eq 0) {
            Write-Host "`nSQL script executed successfully via sqlcmd!" -ForegroundColor Green
        } else {
            Write-Host "`nSQL script execution failed with exit code: $LASTEXITCODE" -ForegroundColor Red
        }
    } catch {
        Write-Host 'Both PowerShell SQL modules and sqlcmd failed.' -ForegroundColor Red
        Write-Host 'Please run the fix_null_values.sql script manually in SQL Server Management Studio.' -ForegroundColor Red
    }
}

Write-Host "`nPress any key to continue..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')
