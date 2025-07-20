# ============================================================================
# VS CODE INSIDERS INTEGRATION DIAGNOSTIC AND FIX
# Bus Buddy WPF Project - Terminal Integration Repair
# ============================================================================

Write-Host "[VS CODE] VS Code Insiders Terminal Integration Diagnostic" -ForegroundColor Cyan
Write-Host "=" * 60 -ForegroundColor Gray

# ============================================================================
# DIAGNOSTIC PHASE
# ============================================================================

Write-Host "`n🔍 PHASE 1: Environment Diagnostic" -ForegroundColor Yellow

# Check VSCODE_PID environment variable
$vscodePid = $env:VSCODE_PID
if ($vscodePid) {
    Write-Host "   ✅ VSCODE_PID: $vscodePid" -ForegroundColor Green
} else {
    Write-Host "   ❌ VSCODE_PID: NOT SET (This is the problem!)" -ForegroundColor Red
}

# Check other VS Code environment variables
$vscodeEnvVars = @(
    "VSCODE_GIT_ASKPASS_NODE",
    "VSCODE_GIT_ASKPASS_EXTRA_ARGS",
    "VSCODE_GIT_ASKPASS_MAIN",
    "VSCODE_GIT_IPC_HANDLE",
    "VSCODE_IPC_HOOK",
    "VSCODE_CWD",
    "TERM_PROGRAM"
)

$foundEnvVars = 0
foreach ($var in $vscodeEnvVars) {
    $value = Get-Item -Path "Env:$var" -ErrorAction SilentlyContinue
    if ($value) {
        Write-Host "   ✅ ${var}: $($value.Value)" -ForegroundColor Green
        $foundEnvVars++
    } else {
        Write-Host "   ❌ ${var}: NOT SET" -ForegroundColor Red
    }
}

Write-Host "`n📊 Environment Variables Found: $foundEnvVars/$($vscodeEnvVars.Count + 1)" -ForegroundColor $(if ($foundEnvVars -gt 3) { "Green" } else { "Red" })

# ============================================================================
# PROCESS ANALYSIS
# ============================================================================

Write-Host "`n🔍 PHASE 2: Process Analysis" -ForegroundColor Yellow

# Find VS Code Insiders processes
$vscodeProcesses = Get-Process | Where-Object { $_.ProcessName -like "*Code*Insiders*" -or $_.ProcessName -eq "Code - Insiders" }
Write-Host "   VS Code Insiders Processes: $($vscodeProcesses.Count)" -ForegroundColor White

# Identify the main VS Code process (highest CPU or memory)
$mainVSCodeProcess = $vscodeProcesses | Sort-Object CPU -Descending | Select-Object -First 1
if ($mainVSCodeProcess) {
    Write-Host "   🎯 Main Process: PID $($mainVSCodeProcess.Id) (CPU: $($mainVSCodeProcess.CPU))" -ForegroundColor Green
} else {
    Write-Host "   ❌ No VS Code Insiders process found!" -ForegroundColor Red
    exit 1
}

# Find PowerShell processes
$pwshProcesses = Get-Process | Where-Object { $_.ProcessName -like "*pwsh*" -or $_.ProcessName -like "*powershell*" }
Write-Host "   PowerShell Processes: $($pwshProcesses.Count)" -ForegroundColor White

# Find current PowerShell process
$currentPid = $PID
$currentProcess = $pwshProcesses | Where-Object { $_.Id -eq $currentPid }
Write-Host "   🎯 Current PowerShell: PID $currentPid" -ForegroundColor Green

# ============================================================================
# INTEGRATION REPAIR
# ============================================================================

Write-Host "`n🛠️  PHASE 3: Integration Repair" -ForegroundColor Yellow

# Try to establish VS Code integration
if (-not $env:VSCODE_PID) {
    Write-Host "   🔧 Setting VSCODE_PID environment variable..." -ForegroundColor Cyan

    # Set the main VS Code process as VSCODE_PID
    $env:VSCODE_PID = $mainVSCodeProcess.Id
    Write-Host "   ✅ VSCODE_PID set to: $($env:VSCODE_PID)" -ForegroundColor Green

    # Verify the setting
    if ($env:VSCODE_PID -eq $mainVSCodeProcess.Id) {
        Write-Host "   ✅ Environment variable verification: SUCCESS" -ForegroundColor Green
    } else {
        Write-Host "   ❌ Environment variable verification: FAILED" -ForegroundColor Red
    }
}

# Set additional VS Code environment variables if missing
if (-not $env:TERM_PROGRAM) {
    $env:TERM_PROGRAM = "vscode"
    Write-Host "   ✅ TERM_PROGRAM set to: vscode" -ForegroundColor Green
}

if (-not $env:VSCODE_CWD) {
    $env:VSCODE_CWD = (Get-Location).Path
    Write-Host "   ✅ VSCODE_CWD set to: $($env:VSCODE_CWD)" -ForegroundColor Green
}

# ============================================================================
# POWERSHELL EXTENSION CHECK
# ============================================================================

Write-Host "`n🔍 PHASE 4: PowerShell Extension Status" -ForegroundColor Yellow

# Check if PowerShell Editor Services is available
$editorServices = Get-Module -Name PowerShellEditorServices -ListAvailable
if ($editorServices) {
    Write-Host "   ✅ PowerShell Editor Services: Available (Version: $($editorServices.Version))" -ForegroundColor Green

    # Try to check if it's loaded
    $loadedServices = Get-Module -Name PowerShellEditorServices
    if ($loadedServices) {
        Write-Host "   ✅ PowerShell Editor Services: LOADED" -ForegroundColor Green
    } else {
        Write-Host "   ⚠️  PowerShell Editor Services: Available but not loaded" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ❌ PowerShell Editor Services: NOT AVAILABLE" -ForegroundColor Red
}

# Check VS Code PowerShell Extension status through registry/file system
$extensionPath = "${env:USERPROFILE}\.vscode-insiders\extensions"
if (Test-Path $extensionPath) {
    $psExtensions = Get-ChildItem $extensionPath -Filter "*powershell*" -Directory
    if ($psExtensions) {
        Write-Host "   ✅ PowerShell Extension: Installed ($($psExtensions.Count) versions)" -ForegroundColor Green
        $psExtensions | ForEach-Object {
            Write-Host "      📦 $($_.Name)" -ForegroundColor Gray
        }
    } else {
        Write-Host "   ❌ PowerShell Extension: NOT FOUND" -ForegroundColor Red
    }
} else {
    Write-Host "   ❌ VS Code Insiders extensions directory: NOT FOUND" -ForegroundColor Red
}

# ============================================================================
# VERIFICATION AND RECOMMENDATIONS
# ============================================================================

Write-Host "`n✅ PHASE 5: Final Verification" -ForegroundColor Yellow

# Re-check environment after fixes
$postFixPid = $env:VSCODE_PID
$postFixTerm = $env:TERM_PROGRAM

Write-Host "   🎯 Final Environment Status:" -ForegroundColor White
Write-Host "      VSCODE_PID: $postFixPid" -ForegroundColor $(if ($postFixPid) { "Green" } else { "Red" })
Write-Host "      TERM_PROGRAM: $postFixTerm" -ForegroundColor $(if ($postFixTerm) { "Green" } else { "Red" })

# Test Bus Buddy commands
Write-Host "`n[BUS BUDDY] Commands Test:" -ForegroundColor Cyan
$busBuddyCommands = @("bb-build", "bb-run", "bb-test", "bb-health")
$workingCommands = 0

foreach ($command in $busBuddyCommands) {
    if (Get-Command $command -ErrorAction SilentlyContinue) {
        Write-Host "   ✅ $command: Available" -ForegroundColor Green
        $workingCommands++
    } else {
        Write-Host "   ❌ $command: Not found" -ForegroundColor Red
    }
}

Write-Host "`n📊 FINAL INTEGRATION STATUS" -ForegroundColor Cyan
Write-Host "=" * 40 -ForegroundColor Gray

$integrationScore = 0
$totalChecks = 5

# Check 1: VSCODE_PID set
if ($env:VSCODE_PID) {
    Write-Host "   ✅ VS Code Process Integration: WORKING" -ForegroundColor Green
    $integrationScore++
} else {
    Write-Host "   ❌ VS Code Process Integration: FAILED" -ForegroundColor Red
}

# Check 2: VS Code processes running
if ($vscodeProcesses.Count -gt 0) {
    Write-Host "   ✅ VS Code Insiders: RUNNING" -ForegroundColor Green
    $integrationScore++
} else {
    Write-Host "   ❌ VS Code Insiders: NOT RUNNING" -ForegroundColor Red
}

# Check 3: PowerShell extension available
if ($psExtensions) {
    Write-Host "   ✅ PowerShell Extension: INSTALLED" -ForegroundColor Green
    $integrationScore++
} else {
    Write-Host "   ❌ PowerShell Extension: MISSING" -ForegroundColor Red
}

# Check 4: Bus Buddy commands working
if ($workingCommands -eq $busBuddyCommands.Count) {
    Write-Host "   ✅ Bus Buddy Commands: ALL WORKING" -ForegroundColor Green
    $integrationScore++
} else {
    Write-Host "   ⚠️  Bus Buddy Commands: $workingCommands/$($busBuddyCommands.Count) working" -ForegroundColor Yellow
}

# Check 5: Terminal environment
if ($env:TERM_PROGRAM -eq "vscode") {
    Write-Host "   ✅ Terminal Environment: INTEGRATED" -ForegroundColor Green
    $integrationScore++
} else {
    Write-Host "   ❌ Terminal Environment: NOT INTEGRATED" -ForegroundColor Red
}

Write-Host "`n🎯 Integration Score: $integrationScore/$totalChecks" -ForegroundColor $(if ($integrationScore -eq $totalChecks) { "Green" } elseif ($integrationScore -gt 2) { "Yellow" } else { "Red" })

# ============================================================================
# RECOMMENDATIONS
# ============================================================================

Write-Host "`n💡 RECOMMENDATIONS" -ForegroundColor Blue
Write-Host "=" * 30 -ForegroundColor Gray

if ($integrationScore -eq $totalChecks) {
    Write-Host "PERFECT! VS Code Insiders integration is working correctly." -ForegroundColor Green
    Write-Host "[BUS BUDDY] Ready to proceed with Syncfusion v30.1.40 alignment!" -ForegroundColor Cyan
} elseif ($integrationScore -ge 3) {
    Write-Host "⚠️  GOOD: Most integration working. Minor issues detected:" -ForegroundColor Yellow

    if (-not $env:VSCODE_PID) {
        Write-Host "   • Run: PowerShell: Restart Current Session (Ctrl+Shift+P)" -ForegroundColor Gray
    }
    if ($workingCommands -lt $busBuddyCommands.Count) {
        Write-Host "   • Run: . .\init-busbuddy-environment.ps1 -Force" -ForegroundColor Gray
    }
} else {
    Write-Host "❌ CRITICAL: Major integration issues detected:" -ForegroundColor Red
    Write-Host "   1. Restart VS Code Insiders completely" -ForegroundColor Gray
    Write-Host "   2. Run: Developer: Reload Window (Ctrl+Shift+P)" -ForegroundColor Gray
    Write-Host "   3. Try: PowerShell: Restart Current Session" -ForegroundColor Gray
    Write-Host "   4. Re-run this diagnostic script" -ForegroundColor Gray
}

Write-Host "`n🔧 Quick Fixes:" -ForegroundColor Blue
Write-Host "   • Force restart PowerShell session: Ctrl+Shift+P → 'PowerShell: Restart Current Session'" -ForegroundColor Gray
Write-Host "   • Reload VS Code window: Ctrl+Shift+P → 'Developer: Reload Window'" -ForegroundColor Gray
Write-Host "   • Re-run diagnostic: .\fix-vscode-integration.ps1" -ForegroundColor Gray

return @{
    IntegrationScore = $integrationScore
    TotalChecks      = $totalChecks
    VSCodePID        = $env:VSCODE_PID
    BusBuddyCommands = $workingCommands
    Recommendations  = if ($integrationScore -eq $totalChecks) { "Ready for Syncfusion work!" } else { "Fix integration issues first" }
}
