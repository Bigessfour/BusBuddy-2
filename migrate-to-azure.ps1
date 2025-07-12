# Azure Database Migration Script for BusBuddy

# Prerequisites: Install Azure CLI and SQL Server Management Studio (SSMS) or Azure Data Studio

# Step 1: Export current database
Write-Host "=== BusBuddy Azure Migration Script ===" -ForegroundColor Green

# Backup current database
$backupPath = "C:\temp\BusBuddyDB_backup.bak"
$localConnectionString = "Server=localhost\SQLEXPRESS;Database=BusBuddyDB;Trusted_Connection=True;"

Write-Host "Creating backup of local database..." -ForegroundColor Yellow
sqlcmd -S "localhost\SQLEXPRESS" -E -Q "BACKUP DATABASE BusBuddyDB TO DISK='$backupPath'"

if (Test-Path $backupPath) {
    Write-Host "✓ Backup created: $backupPath" -ForegroundColor Green
} else {
    Write-Host "✗ Backup failed!" -ForegroundColor Red
    exit 1
}

# Step 2: Create schema script
Write-Host "Generating schema script..." -ForegroundColor Yellow
$schemaScript = "C:\temp\BusBuddyDB_schema.sql"

# Export schema using SqlPackage (part of SQL Server Data Tools)
# Alternatively, you can use SSMS to generate scripts
Write-Host "Please use SSMS to generate scripts for schema and data:" -ForegroundColor Cyan
Write-Host "1. Right-click BusBuddyDB in SSMS" -ForegroundColor White
Write-Host "2. Tasks > Generate Scripts..." -ForegroundColor White
Write-Host "3. Choose 'Script entire database and all database objects'" -ForegroundColor White
Write-Host "4. Advanced > Types of data to script: 'Schema only' first, then 'Data only'" -ForegroundColor White
Write-Host "5. Save as: $schemaScript" -ForegroundColor White

Read-Host "Press Enter when schema script is ready..."

# Step 3: Apply to Azure
Write-Host "Ready to apply to Azure SQL Database" -ForegroundColor Yellow
Write-Host "After setting up Azure SQL Database:" -ForegroundColor Cyan
Write-Host "1. Update connection string in appsettings.json" -ForegroundColor White
Write-Host "2. Run: dotnet ef database update" -ForegroundColor White
Write-Host "3. Or execute the generated schema script in Azure Data Studio" -ForegroundColor White

Write-Host "Migration preparation complete!" -ForegroundColor Green
