# ============================================================================
# SIMPLE VS CODE INSIDERS v1.103.0 SETTINGS APPLICATOR
# Bus Buddy WPF Project - Direct Settings Application
# ============================================================================

Write-Host "🚌 Applying VS Code Insiders v1.103.0 Optimized Settings" -ForegroundColor Cyan
Write-Host "=" * 60 -ForegroundColor Gray

# File paths
$currentSettings = ".\.vscode\settings.json"
$optimizedSettings = ".\.vscode\settings-insiders-optimized.json"
$backupDir = ".\.vscode\backups"
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$backupFile = "$backupDir\settings-backup-$timestamp.json"

# Step 1: Validate files exist
Write-Host "`n🔍 Checking files..." -ForegroundColor Yellow

if (-not (Test-Path $optimizedSettings)) {
    Write-Host "   ❌ Optimized settings not found: $optimizedSettings" -ForegroundColor Red
    exit 1
}
Write-Host "   ✅ Optimized settings found" -ForegroundColor Green

# Step 2: Test JSON syntax
Write-Host "`n🔍 Validating JSON syntax..." -ForegroundColor Yellow
try {
    $content = Get-Content $optimizedSettings -Raw
    $null = $content | ConvertFrom-Json
    Write-Host "   ✅ JSON syntax is valid" -ForegroundColor Green
} catch {
    Write-Host "   ❌ JSON syntax error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 3: Create backup
Write-Host "`n📄 Creating backup..." -ForegroundColor Yellow
if (Test-Path $currentSettings) {
    try {
        if (-not (Test-Path $backupDir)) {
            New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
        }
        Copy-Item $currentSettings $backupFile -Force
        Write-Host "   ✅ Backup created: $backupFile" -ForegroundColor Green
    } catch {
        Write-Host "   ⚠️  Backup failed but continuing: $($_.Exception.Message)" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ⚠️  No existing settings to backup" -ForegroundColor Yellow
}

# Step 4: Apply optimized settings
Write-Host "`n🛠️  Applying optimized settings..." -ForegroundColor Cyan
try {
    Copy-Item $optimizedSettings $currentSettings -Force
    Write-Host "   ✅ Settings applied successfully!" -ForegroundColor Green
} catch {
    Write-Host "   ❌ Failed to apply settings: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 5: Verify application
Write-Host "`n✅ Verifying applied settings..." -ForegroundColor Yellow
try {
    $appliedContent = Get-Content $currentSettings -Raw
    $null = $appliedContent | ConvertFrom-Json
    Write-Host "   ✅ Applied settings are valid JSON" -ForegroundColor Green
} catch {
    Write-Host "   ❌ Applied settings have JSON errors - restoring backup" -ForegroundColor Red
    if (Test-Path $backupFile) {
        Copy-Item $backupFile $currentSettings -Force
        Write-Host "   🔄 Backup restored" -ForegroundColor Yellow
    }
    exit 1
}

# Step 6: Final status check
Write-Host "`n📊 Configuration Status" -ForegroundColor Cyan
Write-Host "=" * 40 -ForegroundColor Gray

$checks = @{
    "VS Code Insiders Available" = (Test-Path "${env:LOCALAPPDATA}\Programs\Microsoft VS Code Insiders\Code - Insiders.exe")
    "PowerShell 7.5.2 Available" = (Test-Path "C:\Program Files\PowerShell\7\pwsh.exe")
    "Settings Applied"           = (Test-Path $currentSettings)
    "Backup Created"             = (Test-Path $backupFile)
    "Bus Buddy Profiles"         = (Test-Path ".\init-busbuddy-environment.ps1")
}

foreach ($check in $checks.GetEnumerator()) {
    $status = if ($check.Value) { "✅ OK" } else { "❌ MISSING" }
    $color = if ($check.Value) { "Green" } else { "Red" }
    Write-Host "   $($check.Key): $status" -ForegroundColor $color
}

Write-Host "`n🎉 VS Code Insiders v1.103.0 Configuration Complete!" -ForegroundColor Green
Write-Host "🚌 PowerShell environment optimized for Bus Buddy development" -ForegroundColor Cyan

Write-Host "`n📋 Next Steps:" -ForegroundColor Blue
Write-Host "   1. Restart VS Code Insiders to apply all settings" -ForegroundColor Gray
Write-Host "   2. Open integrated terminal (Ctrl+\`) and verify:" -ForegroundColor Gray
Write-Host "      • Bus Buddy environment loads automatically" -ForegroundColor Gray
Write-Host "      • PowerShell extension connects properly" -ForegroundColor Gray
Write-Host "      • Commands work: bb-build, bb-run, bb-test" -ForegroundColor Gray
Write-Host "   3. Proceed with Syncfusion v30.1.40 alignment" -ForegroundColor Gray

if (Test-Path $backupFile) {
    Write-Host "`n🔄 Rollback if needed:" -ForegroundColor Blue
    Write-Host "   Copy-Item '$backupFile' '$currentSettings' -Force" -ForegroundColor Gray
}

Write-Host "`n✨ Environment ready for Syncfusion development!" -ForegroundColor Green
