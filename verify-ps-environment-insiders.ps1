# ============================================================================
# POWERSHELL ENVIRONMENT VERIFICATION FOR VS CODE INSIDERS v1.103.0
# Bus Buddy WPF Project - Development Environment Setup
# ============================================================================

param(
    [switch]$Fix,
    [switch]$Verbose,
    [switch]$ApplyOptimized,
    [switch]$Force
)

# VS Code Insiders Version Information
$InsidersVersion = @{
    Version  = "1.103.0-insider"
    Commit   = "3b2551cd990a02383190cc7670021ec6c30b86df"
    Date     = "2025-07-18T05:03:37.281Z"
    Electron = "35.6.0"
    Chromium = "134.0.6998.205"
    NodeJS   = "22.15.1"
    V8       = "13.4.114.21-electron.0"
    OS       = "Windows_NT x64 10.0.26100"
}

Write-Host "🚌 Bus Buddy PowerShell Environment Verification" -ForegroundColor Cyan
Write-Host "📊 Target: VS Code Insiders v$($InsidersVersion.Version)" -ForegroundColor Yellow
Write-Host "=" * 80 -ForegroundColor Gray

# ============================================================================
# PHASE 1: ENVIRONMENT DETECTION
# ============================================================================

function Test-PowerShellVersion {
    Write-Host "`n🔍 Checking PowerShell Version..." -ForegroundColor Cyan

    $psVersion = $PSVersionTable.PSVersion
    $expectedVersion = [Version]"7.5.2"

    Write-Host "   Current: PowerShell $psVersion" -ForegroundColor White
    Write-Host "   Expected: PowerShell $expectedVersion or higher" -ForegroundColor Gray

    if ($psVersion -ge $expectedVersion) {
        Write-Host "   ✅ PowerShell version is compatible" -ForegroundColor Green
        return $true
    } else {
        Write-Host "   ❌ PowerShell version is too old" -ForegroundColor Red
        return $false
    }
}

function Test-VSCodeInsiders {
    Write-Host "`n🔍 Checking VS Code Insiders Installation..." -ForegroundColor Cyan

    # Check for VS Code Insiders in common locations
    $insidersPath = @(
        "${env:LOCALAPPDATA}\Programs\Microsoft VS Code Insiders\Code - Insiders.exe",
        "${env:PROGRAMFILES}\Microsoft VS Code Insiders\Code - Insiders.exe",
        "${env:PROGRAMFILES(X86)}\Microsoft VS Code Insiders\Code - Insiders.exe"
    ) | Where-Object { Test-Path $_ } | Select-Object -First 1

    if ($insidersPath) {
        Write-Host "   ✅ VS Code Insiders found: $insidersPath" -ForegroundColor Green

        # Check if we're running inside VS Code Insiders
        if ($env:VSCODE_PID) {
            Write-Host "   ✅ Running inside VS Code Insiders (PID: $env:VSCODE_PID)" -ForegroundColor Green
        } else {
            Write-Host "   ⚠️  Not currently running inside VS Code Insiders" -ForegroundColor Yellow
        }
        return $true
    } else {
        Write-Host "   ❌ VS Code Insiders not found" -ForegroundColor Red
        return $false
    }
}

function Test-PowerShellExtension {
    Write-Host "`n🔍 Checking PowerShell Extension Status..." -ForegroundColor Cyan

    # Check for PowerShell Extension in VS Code Insiders
    $extensionPath = "${env:USERPROFILE}\.vscode-insiders\extensions"

    if (Test-Path $extensionPath) {
        $psExtensions = Get-ChildItem $extensionPath -Filter "*powershell*" -Directory | Sort-Object Name -Descending

        if ($psExtensions) {
            $latestExtension = $psExtensions[0]
            Write-Host "   ✅ PowerShell Extension found: $($latestExtension.Name)" -ForegroundColor Green

            # Check version
            if ($latestExtension.Name -match "ms-vscode\.powershell-(\d+\.\d+\.\d+)") {
                $extensionVersion = $matches[1]
                Write-Host "   📦 Extension Version: v$extensionVersion" -ForegroundColor White

                if ([Version]$extensionVersion -ge [Version]"2025.2.0") {
                    Write-Host "   ✅ Extension version is compatible with VS Code Insiders v1.103.0" -ForegroundColor Green
                    return $true
                } else {
                    Write-Host "   ⚠️  Extension version may need updating" -ForegroundColor Yellow
                    return $false
                }
            }
        } else {
            Write-Host "   ❌ PowerShell Extension not found" -ForegroundColor Red
            return $false
        }
    } else {
        Write-Host "   ❌ VS Code Insiders extensions directory not found" -ForegroundColor Red
        return $false
    }
}

function Test-WorkspaceSettings {
    Write-Host "`n🔍 Checking Workspace Settings..." -ForegroundColor Cyan

    $settingsPath = ".\.vscode\settings.json"
    $optimizedPath = ".\.vscode\settings-insiders-optimized.json"

    if (Test-Path $settingsPath) {
        Write-Host "   ✅ settings.json found" -ForegroundColor Green

        try {
            $settings = Get-Content $settingsPath -Raw | ConvertFrom-Json

            # Check critical PowerShell settings
            $criticalSettings = @(
                "powershell.startAutomatically",
                "powershell.enableProfileLoading",
                "powershell.powerShellExePath",
                "terminal.integrated.defaultProfile.windows"
            )

            $missingSettings = @()
            foreach ($setting in $criticalSettings) {
                $keys = $setting.Split('.')
                $current = $settings
                $found = $true

                foreach ($key in $keys) {
                    if ($current.PSObject.Properties.Name -contains $key) {
                        $current = $current.$key
                    } else {
                        $found = $false
                        break
                    }
                }

                if (-not $found) {
                    $missingSettings += $setting
                }
            }

            if ($missingSettings.Count -eq 0) {
                Write-Host "   ✅ All critical PowerShell settings present" -ForegroundColor Green
            } else {
                Write-Host "   ⚠️  Missing settings: $($missingSettings -join ', ')" -ForegroundColor Yellow
            }

        } catch {
            Write-Host "   ❌ Error parsing settings.json: $($_.Exception.Message)" -ForegroundColor Red
            return $false
        }
    } else {
        Write-Host "   ❌ settings.json not found" -ForegroundColor Red
        return $false
    }

    if (Test-Path $optimizedPath) {
        Write-Host "   ✅ Optimized settings file available" -ForegroundColor Green
        return $true
    } else {
        Write-Host "   ⚠️  Optimized settings file not found" -ForegroundColor Yellow
        return $false
    }
}

function Test-BusBuddyProfiles {
    Write-Host "`n🔍 Checking Bus Buddy PowerShell Profiles..." -ForegroundColor Cyan

    $profiles = @(
        ".\BusBuddy-PowerShell-Profile.ps1",
        ".\BusBuddy-Advanced-Workflows.ps1",
        ".\init-busbuddy-environment.ps1",
        ".\load-bus-buddy-profiles.ps1"
    )

    $foundProfiles = 0
    foreach ($profile in $profiles) {
        if (Test-Path $profile) {
            Write-Host "   ✅ $profile" -ForegroundColor Green
            $foundProfiles++
        } else {
            Write-Host "   ❌ $profile" -ForegroundColor Red
        }
    }

    if ($foundProfiles -eq $profiles.Count) {
        Write-Host "   ✅ All Bus Buddy profiles present" -ForegroundColor Green
        return $true
    } else {
        Write-Host "   ⚠️  Missing $($profiles.Count - $foundProfiles) profile(s)" -ForegroundColor Yellow
        return $false
    }
}

# ============================================================================
# PHASE 2: INTEGRATION TESTING
# ============================================================================

function Test-PowerShellIntegration {
    Write-Host "`n🔍 Testing PowerShell Extension Integration..." -ForegroundColor Cyan

    # Test if PowerShell extension commands are available
    try {
        # Check if we can access PowerShell extension features
        if ($env:VSCODE_PID) {
            Write-Host "   ✅ VS Code integration active (PID: $env:VSCODE_PID)" -ForegroundColor Green

            # Test PowerShell Editor Services
            if (Get-Module -Name PowerShellEditorServices -ListAvailable) {
                Write-Host "   ✅ PowerShell Editor Services available" -ForegroundColor Green
                return $true
            } else {
                Write-Host "   ⚠️  PowerShell Editor Services not loaded" -ForegroundColor Yellow
                return $false
            }
        } else {
            Write-Host "   ⚠️  Not running in VS Code Insiders integrated terminal" -ForegroundColor Yellow
            return $false
        }
    } catch {
        Write-Host "   ❌ Error testing PowerShell integration: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Test-BusBuddyCommands {
    Write-Host "`n🔍 Testing Bus Buddy Commands..." -ForegroundColor Cyan

    $commands = @("bb-build", "bb-run", "bb-test", "bb-health", "bb-diagnostic")
    $availableCommands = 0

    foreach ($command in $commands) {
        if (Get-Command $command -ErrorAction SilentlyContinue) {
            Write-Host "   ✅ $command" -ForegroundColor Green
            $availableCommands++
        } else {
            Write-Host "   ❌ $command" -ForegroundColor Red
        }
    }

    if ($availableCommands -eq $commands.Count) {
        Write-Host "   ✅ All Bus Buddy commands available" -ForegroundColor Green
        return $true
    } else {
        Write-Host "   ⚠️  Missing $($commands.Count - $availableCommands) command(s)" -ForegroundColor Yellow
        return $false
    }
}

# ============================================================================
# PHASE 3: OPTIMIZATION AND FIXES
# ============================================================================

function Apply-OptimizedSettings {
    param([switch]$Force)

    Write-Host "`n🛠️  Applying Optimized Settings for VS Code Insiders v1.103.0..." -ForegroundColor Cyan

    $currentSettings = ".\.vscode\settings.json"
    $optimizedSettings = ".\.vscode\settings-insiders-optimized.json"
    $backupSettings = ".\.vscode\settings-backup-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"

    if (-not (Test-Path $optimizedSettings)) {
        Write-Host "   ❌ Optimized settings file not found: $optimizedSettings" -ForegroundColor Red
        return $false
    }

    # Create backup of current settings
    if (Test-Path $currentSettings) {
        Copy-Item $currentSettings $backupSettings
        Write-Host "   📄 Backup created: $backupSettings" -ForegroundColor Yellow
    }

    try {
        # Validate optimized settings JSON
        $optimizedContent = Get-Content $optimizedSettings -Raw
        $null = $optimizedContent | ConvertFrom-Json

        # Apply optimized settings
        Copy-Item $optimizedSettings $currentSettings -Force
        Write-Host "   ✅ Optimized settings applied successfully" -ForegroundColor Green

        return $true
    } catch {
        Write-Host "   ❌ Error applying optimized settings: $($_.Exception.Message)" -ForegroundColor Red

        # Restore backup if it exists
        if (Test-Path $backupSettings) {
            Copy-Item $backupSettings $currentSettings -Force
            Write-Host "   🔄 Settings restored from backup" -ForegroundColor Yellow
        }

        return $false
    }
}

function Fix-PowerShellPaths {
    Write-Host "`n🛠️  Verifying PowerShell Paths..." -ForegroundColor Cyan

    $pwshPath = "C:\Program Files\PowerShell\7\pwsh.exe"

    if (Test-Path $pwshPath) {
        Write-Host "   ✅ PowerShell 7.5.2 path verified: $pwshPath" -ForegroundColor Green
        return $true
    } else {
        Write-Host "   ❌ PowerShell 7.5.2 not found at expected path" -ForegroundColor Red

        # Try to find PowerShell 7 in other locations
        $alternatePaths = @(
            "${env:PROGRAMFILES}\PowerShell\7\pwsh.exe",
            "${env:PROGRAMFILES(X86)}\PowerShell\7\pwsh.exe",
            "${env:LOCALAPPDATA}\Microsoft\powershell\pwsh.exe"
        )

        foreach ($path in $alternatePaths) {
            if (Test-Path $path) {
                Write-Host "   ✅ Found PowerShell at: $path" -ForegroundColor Green
                return $path
            }
        }

        return $false
    }
}

function Initialize-BusBuddyEnvironment {
    Write-Host "`n🛠️  Initializing Bus Buddy Environment..." -ForegroundColor Cyan

    try {
        if (Test-Path ".\init-busbuddy-environment.ps1") {
            . ".\init-busbuddy-environment.ps1" -Quiet
            Write-Host "   ✅ Bus Buddy environment initialized" -ForegroundColor Green
            return $true
        } else {
            Write-Host "   ❌ init-busbuddy-environment.ps1 not found" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "   ❌ Error initializing Bus Buddy environment: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# ============================================================================
# MAIN EXECUTION
# ============================================================================

Write-Host "`n🚀 Starting PowerShell Environment Verification..." -ForegroundColor Green

# Phase 1: Environment Detection
$results = @{
    PowerShellVersion   = Test-PowerShellVersion
    VSCodeInsiders      = Test-VSCodeInsiders
    PowerShellExtension = Test-PowerShellExtension
    WorkspaceSettings   = Test-WorkspaceSettings
    BusBuddyProfiles    = Test-BusBuddyProfiles
}

# Phase 2: Integration Testing
$results.PowerShellIntegration = Test-PowerShellIntegration
$results.BusBuddyCommands = Test-BusBuddyCommands

# Phase 3: Apply fixes if requested
if ($Fix -or $ApplyOptimized) {
    Write-Host "`n🛠️  APPLYING FIXES AND OPTIMIZATIONS" -ForegroundColor Yellow
    Write-Host "=" * 50 -ForegroundColor Gray

    if ($ApplyOptimized) {
        $results.OptimizedSettingsApplied = Apply-OptimizedSettings -Force:$Force
    }

    $results.PowerShellPathsFixed = Fix-PowerShellPaths
    $results.EnvironmentInitialized = Initialize-BusBuddyEnvironment
}

# Final Summary
Write-Host "`n📊 VERIFICATION SUMMARY" -ForegroundColor Cyan
Write-Host "=" * 50 -ForegroundColor Gray

$passedTests = ($results.Values | Where-Object { $_ -eq $true }).Count
$totalTests = $results.Count

foreach ($test in $results.GetEnumerator()) {
    $status = if ($test.Value) { "✅ PASS" } else { "❌ FAIL" }
    $color = if ($test.Value) { "Green" } else { "Red" }
    Write-Host "   $($test.Key): $status" -ForegroundColor $color
}

Write-Host "`n🎯 Overall Score: $passedTests/$totalTests tests passed" -ForegroundColor $(if ($passedTests -eq $totalTests) { "Green" } else { "Yellow" })

if ($passedTests -eq $totalTests) {
    Write-Host "`n🎉 PowerShell environment is properly configured for VS Code Insiders v1.103.0!" -ForegroundColor Green
    Write-Host "🚌 Bus Buddy development environment is ready for Syncfusion v30.1.40 alignment!" -ForegroundColor Cyan
} else {
    Write-Host "`n⚠️  Some issues detected. Run with -Fix to attempt automatic resolution." -ForegroundColor Yellow
    if ($ApplyOptimized -and -not $results.OptimizedSettingsApplied) {
        Write-Host "💡 Tip: Review the optimized settings file for any JSON syntax errors." -ForegroundColor Blue
    }
}

Write-Host "`n📖 Usage Examples:" -ForegroundColor Blue
Write-Host "   .\verify-ps-environment-insiders.ps1                    # Verification only" -ForegroundColor Gray
Write-Host "   .\verify-ps-environment-insiders.ps1 -Fix               # Fix issues automatically" -ForegroundColor Gray
Write-Host "   .\verify-ps-environment-insiders.ps1 -ApplyOptimized    # Apply optimized settings" -ForegroundColor Gray
Write-Host "   .\verify-ps-environment-insiders.ps1 -ApplyOptimized -Force  # Force apply settings" -ForegroundColor Gray

return $results
