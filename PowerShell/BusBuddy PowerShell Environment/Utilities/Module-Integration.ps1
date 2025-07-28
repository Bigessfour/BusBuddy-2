#Requires -Version 7.5
<#
.SYNOPSIS
    Bus Buddy PowerShell Module Integration

.DESCRIPTION
    Integrates the new Bus Buddy PowerShell module with existing VS Code tasks,
    profile loading, and development workflows. This script updates the existing
    profile loading mechanisms to use the new module structure.

.PARAMETER UpdateProfiles
    Update existing PowerShell profiles to use the new module

.PARAMETER UpdateTasks
    Update VS Code tasks to use module commands

.PARAMETER TestIntegration
    Test the integration by running key commands

.EXAMPLE
    .\Module-Integration.ps1 -UpdateProfiles -UpdateTasks -TestIntegration

.NOTES
    This script should be run from the Bus Buddy project root directory.
    It will create backup copies of modified files.
#>

param(
    [switch]$UpdateProfiles,
    [switch]$UpdateTasks,
    [switch]$TestIntegration
)

# Import the module first to ensure it works
$modulePath = Join-Path $PWD "PowerShell\BusBuddy.psm1"
if (-not (Test-Path $modulePath)) {
    Write-Host "❌ Bus Buddy module not found at: $modulePath" -ForegroundColor Red
    exit 1
}

Write-Host "🚌 Bus Buddy PowerShell Module Integration" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════" -ForegroundColor DarkGray

# Test module loading
try {
    Import-Module $modulePath -Force
    Write-Host "✅ Module loaded successfully" -ForegroundColor Green
}
catch {
    Write-Host "❌ Failed to load module: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

if ($UpdateProfiles) {
    Write-Host ""
    Write-Host "📝 Updating PowerShell Profiles..." -ForegroundColor Yellow

    # Create new profile loader that uses the module
    $newProfileContent = @'
#Requires -Version 7.0
<#
.SYNOPSIS
    Bus Buddy PowerShell Profile Loader (Module-based)

.DESCRIPTION
    Loads the Bus Buddy PowerShell module for development workflow automation.
    This replaces the previous script-based profile with a professional module.

.PARAMETER Quiet
    Suppress loading messages
#>

param([switch]$Quiet)

# Load the Bus Buddy module
$modulePath = Join-Path $PSScriptRoot "PowerShell\BusBuddy.psm1"

if (Test-Path $modulePath) {
    try {
        Import-Module $modulePath -Force

        if (-not $Quiet) {
            Write-Host "🚌 Bus Buddy PowerShell Module loaded successfully!" -ForegroundColor Green
            Write-Host "Available commands: bb-build, bb-run, bb-test, bb-happiness, bb-commands" -ForegroundColor Cyan

            # Quick environment check
            $envValid = Test-BusBuddyEnvironment
            if (-not $envValid) {
                Write-Host "⚠️ Environment issues detected. Run 'bb-env-check' for details." -ForegroundColor Yellow
            }
        }
    } catch {
        Write-Host "❌ Failed to load Bus Buddy module: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "❌ Bus Buddy module not found at: $modulePath" -ForegroundColor Red
    Write-Host "Please ensure the PowerShell module is properly installed." -ForegroundColor Yellow
}

# Backward compatibility aliases for existing scripts
if (Get-Command Invoke-BusBuddyBuild -ErrorAction SilentlyContinue) {
    Set-Alias -Name 'build-busbuddy' -Value 'bb-build' -Force
    Set-Alias -Name 'run-busbuddy' -Value 'bb-run' -Force
    Set-Alias -Name 'test-busbuddy' -Value 'bb-test' -Force
}
'@

    # Write the new profile loader
    $newProfilePath = Join-Path $PWD "load-bus-buddy-profiles.ps1"

    # Backup existing profile if it exists
    if (Test-Path $newProfilePath) {
        $backupPath = "$newProfilePath.backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
        Copy-Item $newProfilePath $backupPath
        Write-Host "  📋 Backed up existing profile to: $backupPath" -ForegroundColor Gray
    }

    Set-Content -Path $newProfilePath -Value $newProfileContent -Encoding UTF8
    Write-Host "  ✅ Updated profile loader: $newProfilePath" -ForegroundColor Green
}

if ($UpdateTasks) {
    Write-Host ""
    Write-Host "⚙️ VS Code Task Integration..." -ForegroundColor Yellow

    # Create a new PowerShell task that uses the module
    $moduleTaskTemplate = @'
{
    "label": "🚌 BB: Load Module and Build",
    "type": "shell",
    "command": "pwsh.exe",
    "args": [
        "-ExecutionPolicy", "Bypass",
        "-Command",
        "Import-Module '.\\PowerShell\\BusBuddy.psm1' -Force; bb-build -Clean -Restore"
    ],
    "group": {
        "kind": "build",
        "isDefault": true
    },
    "runOptions": {
        "instanceLimit": 1
    },
    "isBackground": false,
    "detail": "🚌 Load Bus Buddy module and build solution with clean and restore"
}
'@

    Write-Host "  📋 Sample task configuration created for VS Code integration" -ForegroundColor Gray
    Write-Host "  💡 Add this to your .vscode/tasks.json file:" -ForegroundColor Blue
    Write-Host $moduleTaskTemplate -ForegroundColor DarkGray
}

if ($TestIntegration) {
    Write-Host ""
    Write-Host "🧪 Testing Module Integration..." -ForegroundColor Yellow

    # Test core commands
    $tests = @(
        @{ Name = 'Environment Check'; Command = 'Test-BusBuddyEnvironment' }
        @{ Name = 'Project Root Detection'; Command = 'Get-BusBuddyProjectRoot' }
        @{ Name = 'Command Discovery'; Command = 'Get-BusBuddyCommands -Category Essential' }
        @{ Name = 'Happiness Function'; Command = 'Get-BusBuddyHappiness -Count 1' }
    )

    foreach ($test in $tests) {
        Write-Host "  Testing: $($test.Name)..." -ForegroundColor Cyan -NoNewline

        try {
            $result = Invoke-Expression $test.Command
            if ($result) {
                Write-Host " ✅" -ForegroundColor Green
            }
            else {
                Write-Host " ⚠️ (No result)" -ForegroundColor Yellow
            }
        }
        catch {
            Write-Host " ❌ Error: $($_.Exception.Message)" -ForegroundColor Red
        }
    }

    Write-Host ""
    Write-Host "🔧 Testing Build Command..." -ForegroundColor Yellow
    try {
        $buildResult = Invoke-BusBuddyBuild -Verbosity quiet
        if ($buildResult) {
            Write-Host "  ✅ Build test passed" -ForegroundColor Green
        }
        else {
            Write-Host "  ⚠️ Build test had issues (check project state)" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "  ❌ Build test failed: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "🎉 Integration Complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "1. Use the new module commands: bb-build, bb-run, bb-test" -ForegroundColor White
Write-Host "2. Try bb-dev-session for complete development setup" -ForegroundColor White
Write-Host "3. Get motivation with bb-happiness when you need it! 😊" -ForegroundColor White
Write-Host "4. Discover all commands with bb-commands" -ForegroundColor White
Write-Host ""

# Display module info
Get-BusBuddyInfo
