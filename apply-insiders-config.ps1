# ============================================================================
# APPLY VS CODE INSIDERS v1.103.0 OPTIMIZED CONFIGURATION
# Bus Buddy WPF Project - PowerShell Environment Setup
# ============================================================================

param(
    [switch]$Force,
    [switch]$Backup = $true,
    [switch]$Validate = $true
)

Write-Host "🚌 Applying VS Code Insiders v1.103.0 Optimized Configuration" -ForegroundColor Cyan
Write-Host "=" * 70 -ForegroundColor Gray

# Configuration files
$currentSettings = ".\.vscode\settings.json"
$optimizedSettings = ".\.vscode\settings-insiders-optimized.json"
$backupDir = ".\.vscode\backups"
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$backupFile = "$backupDir\settings-backup-$timestamp.json"

# ============================================================================
# VALIDATION FUNCTIONS
# ============================================================================

function Test-JsonSyntax {
    param([string]$FilePath)

    try {
        $content = Get-Content $FilePath -Raw -ErrorAction Stop
        $null = $content | ConvertFrom-Json -ErrorAction Stop
        return $true
    } catch {
        Write-Host "   ❌ JSON Syntax Error: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Test-RequiredSettings {
    param([string]$FilePath)

    try {
        $settings = Get-Content $FilePath -Raw | ConvertFrom-Json

        $requiredSettings = @{
            "powershell.startAutomatically" = $true
            "powershell.enableProfileLoading" = $true
            "powershell.powerShellExePath" = "C:\Program Files\PowerShell\7\pwsh.exe"
            "terminal.integrated.defaultProfile.windows" = "PowerShell 7.5.2 (Bus Buddy)"
            "omnisharp.enableEditorConfigSupport" = $true
            "dotnet.server.useOmnisharp" = $false
        }

        $missingOrIncorrect = @()

        foreach ($setting in $requiredSettings.GetEnumerator()) {
            $keys = $setting.Key.Split('.')
            $current = $settings
            $found = $true
            $currentPath = ""

            foreach ($key in $keys) {
                $currentPath += ".$key"
                if ($null -ne $current -and $current.PSObject.Properties.Name -contains $key) {
                    $current = $current.$key
                } else {
                    $found = $false
                    break
                }
            }

            if (-not $found) {
                $missingOrIncorrect += "$($setting.Key) (Missing)"
            } elseif ($current -ne $setting.Value) {
                $actualValue = if ($null -eq $current) { "null" } else { $current.ToString() }
                $missingOrIncorrect += "$($setting.Key) (Expected: $($setting.Value), Found: $actualValue)"
            }
        }

        if ($missingOrIncorrect.Count -eq 0) {
            Write-Host "   ✅ All required settings are correct" -ForegroundColor Green
            return $true
        } else {
            Write-Host "   ⚠️  Issues found:" -ForegroundColor Yellow
            foreach ($issue in $missingOrIncorrect) {
                Write-Host "      - $issue" -ForegroundColor Gray
            }
            return $false
        }
    } catch {
        Write-Host "   ❌ Error validating settings: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}# ============================================================================
# BACKUP FUNCTIONS
# ============================================================================

function New-SettingsBackup {
    Write-Host "`n📄 Creating backup of current settings..." -ForegroundColor Yellow

    if (-not (Test-Path $currentSettings)) {
        Write-Host "   ⚠️  No current settings.json found - skipping backup" -ForegroundColor Yellow
        return $true
    }

    try {
        # Create backup directory if it doesn't exist
        if (-not (Test-Path $backupDir)) {
            New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
        }

        # Copy current settings to backup
        Copy-Item $currentSettings $backupFile -Force
        Write-Host "   ✅ Backup created: $backupFile" -ForegroundColor Green

        # Keep only last 10 backups
        $backups = @(Get-ChildItem $backupDir -Filter "settings-backup-*.json" -ErrorAction SilentlyContinue | Sort-Object CreationTime -Descending)
        if ($backups -and $backups.Count -gt 10) {
            $backups | Select-Object -Skip 10 | Remove-Item -Force -ErrorAction SilentlyContinue
            Write-Host "   🧹 Cleaned up old backups (kept last 10)" -ForegroundColor Gray
        }

        return $true
    } catch {
        Write-Host "   ❌ Error creating backup: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Restore-SettingsBackup {
    param([string]$BackupPath)

    Write-Host "`n🔄 Restoring settings from backup..." -ForegroundColor Yellow

    try {
        if (Test-Path $BackupPath) {
            Copy-Item $BackupPath $currentSettings -Force
            Write-Host "   ✅ Settings restored from: $BackupPath" -ForegroundColor Green
            return $true
        } else {
            Write-Host "   ❌ Backup file not found: $BackupPath" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "   ❌ Error restoring backup: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# ============================================================================
# MAIN APPLICATION LOGIC
# ============================================================================

# Step 1: Validate optimized settings file exists and is valid
Write-Host "`n🔍 Validating optimized settings file..." -ForegroundColor Cyan

if (-not (Test-Path $optimizedSettings)) {
    Write-Host "   ❌ Optimized settings file not found: $optimizedSettings" -ForegroundColor Red
    exit 1
}

if ($Validate) {
    if (-not (Test-JsonSyntax $optimizedSettings)) {
        Write-Host "   ❌ Optimized settings file contains JSON syntax errors" -ForegroundColor Red
        exit 1
    }

    Write-Host "   ✅ Optimized settings file is valid JSON" -ForegroundColor Green

    if (-not (Test-RequiredSettings $optimizedSettings)) {
        if (-not $Force) {
            Write-Host "   ⚠️  Optimized settings may be incomplete. Use -Force to apply anyway." -ForegroundColor Yellow
            exit 1
        } else {
            Write-Host "   ⚠️  Applying settings despite validation warnings (Force mode)" -ForegroundColor Yellow
        }
    }
}

# Step 2: Create backup if requested
if ($Backup) {
    if (-not (New-SettingsBackup)) {
        Write-Host "❌ Failed to create backup. Aborting." -ForegroundColor Red
        exit 1
    }
}

# Step 3: Apply optimized settings
Write-Host "`n🛠️  Applying optimized settings..." -ForegroundColor Cyan

try {
    Copy-Item $optimizedSettings $currentSettings -Force
    Write-Host "   ✅ Optimized settings applied successfully" -ForegroundColor Green
} catch {
    Write-Host "   ❌ Error applying optimized settings: $($_.Exception.Message)" -ForegroundColor Red

    # Attempt to restore backup
    if ($Backup -and (Test-Path $backupFile)) {
        Restore-SettingsBackup $backupFile
    }
    exit 1
}

# Step 4: Validate applied settings
if ($Validate) {
    Write-Host "`n✅ Validating applied settings..." -ForegroundColor Cyan

    if (Test-JsonSyntax $currentSettings) {
        Write-Host "   ✅ Applied settings have valid JSON syntax" -ForegroundColor Green
    } else {
        Write-Host "   ❌ Applied settings have JSON syntax errors - restoring backup" -ForegroundColor Red
        if ($Backup) {
            Restore-SettingsBackup $backupFile
        }
        exit 1
    }

    if (Test-RequiredSettings $currentSettings) {
        Write-Host "   ✅ All required settings are properly configured" -ForegroundColor Green
    } else {
        Write-Host "   ⚠️  Some settings may need attention" -ForegroundColor Yellow
    }
}

# Step 5: Display VS Code Insiders integration status
Write-Host "`n📊 VS Code Insiders Integration Status" -ForegroundColor Cyan
Write-Host "=" * 50 -ForegroundColor Gray

$integrationChecks = @{
    "VS Code Insiders Process" = [bool]$env:VSCODE_PID
    "PowerShell 7.5.2 Available" = (Test-Path "C:\Program Files\PowerShell\7\pwsh.exe")
    "Bus Buddy Profiles" = (Test-Path ".\init-busbuddy-environment.ps1")
    "Terminal Integration" = ($env:TERM_PROGRAM -eq "vscode")
}

foreach ($check in $integrationChecks.GetEnumerator()) {
    $status = if ($check.Value) { "✅ OK" } else { "❌ NOT OK" }
    $color = if ($check.Value) { "Green" } else { "Red" }
    Write-Host "   $($check.Key): $status" -ForegroundColor $color
}

# Final success message
Write-Host "`n🎉 VS Code Insiders v1.103.0 Configuration Applied!" -ForegroundColor Green
Write-Host "🚌 Bus Buddy PowerShell environment is optimized for development" -ForegroundColor Cyan

# Next steps
Write-Host "`n📋 Next Steps:" -ForegroundColor Blue
Write-Host "   1. Restart VS Code Insiders to apply all settings" -ForegroundColor Gray
Write-Host "   2. Open integrated terminal (Ctrl+`)" -ForegroundColor Gray
Write-Host "   3. Verify Bus Buddy environment loads automatically" -ForegroundColor Gray
Write-Host "   4. Test commands: bb-build, bb-run, bb-test, bb-health" -ForegroundColor Gray
Write-Host "   5. Proceed with Syncfusion v30.1.40 alignment" -ForegroundColor Gray

Write-Host "`n🔧 Troubleshooting:" -ForegroundColor Blue
Write-Host "   Run: .\verify-ps-environment-insiders.ps1 -Fix" -ForegroundColor Gray

if ($Backup) {
    Write-Host "`n🔄 Rollback if needed:" -ForegroundColor Blue
    Write-Host "   Copy-Item '$backupFile' '$currentSettings' -Force" -ForegroundColor Gray
}
