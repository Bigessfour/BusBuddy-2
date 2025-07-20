#Requires -Version 7.0
<#
.SYNOPSIS
    Force PowerShell Extension Activation and Get Version
.DESCRIPTION
    This script attempts to activate the PowerShell extension and get version information
    using multiple methods including direct VS Code commands and extension queries.
#>

Write-Host "=== PowerShell Extension Activation & Version Check ===" -ForegroundColor Cyan
Write-Host ""

# Method 1: Try to get extension info via VS Code API
Write-Host "Method 1: VS Code Extension Query" -ForegroundColor Yellow
try {
    $extensionInfo = code --list-extensions --show-versions | Where-Object { $_ -match "ms-vscode\.powershell" }
    if ($extensionInfo) {
        Write-Host "  ✅ Found: $extensionInfo" -ForegroundColor Green
        $version = ($extensionInfo -split '@')[1]
        Write-Host "  ✅ Extension Version: $version" -ForegroundColor Green
    } else {
        Write-Host "  ❌ PowerShell extension not found in VS Code" -ForegroundColor Red
    }
} catch {
    Write-Host "  ❌ Error querying VS Code extensions: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Method 2: Check PowerShell extension files directly
Write-Host "Method 2: Direct Extension File Check" -ForegroundColor Yellow
$extensionPaths = @(
    "$env:USERPROFILE\.vscode\extensions",
    "$env:USERPROFILE\.vscode-insiders\extensions"
)

foreach ($path in $extensionPaths) {
    if (Test-Path $path) {
        $psExtensions = Get-ChildItem $path -Directory | Where-Object { $_.Name -like "*ms-vscode.powershell*" }
        if ($psExtensions) {
            foreach ($ext in $psExtensions) {
                Write-Host "  ✅ Found extension folder: $($ext.Name)" -ForegroundColor Green
                $packageJson = Join-Path $ext.FullName "package.json"
                if (Test-Path $packageJson) {
                    try {
                        $packageInfo = Get-Content $packageJson | ConvertFrom-Json
                        Write-Host "    Version: $($packageInfo.version)" -ForegroundColor Cyan
                        Write-Host "    Display Name: $($packageInfo.displayName)" -ForegroundColor Cyan
                        Write-Host "    Publisher: $($packageInfo.publisher)" -ForegroundColor Cyan
                    } catch {
                        Write-Host "    ❌ Could not read package.json" -ForegroundColor Red
                    }
                }
            }
        } else {
            Write-Host "  ⚠️ No PowerShell extensions found in: $path" -ForegroundColor Yellow
        }
    } else {
        Write-Host "  ⚠️ Extension path not found: $path" -ForegroundColor Yellow
    }
}
Write-Host ""

# Method 3: Try to activate PowerShell extension via commands
Write-Host "Method 3: Extension Activation Attempt" -ForegroundColor Yellow
try {
    # Try to import PowerShell Editor Services if available
    $editorServicesPath = Get-ChildItem -Path $extensionPaths -Recurse -Filter "PowerShellEditorServices.psd1" -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($editorServicesPath) {
        Write-Host "  ✅ Found PowerShell Editor Services at: $($editorServicesPath.FullName)" -ForegroundColor Green
        try {
            Import-Module $editorServicesPath.FullName -Force
            Write-Host "  ✅ Successfully imported PowerShell Editor Services" -ForegroundColor Green
        } catch {
            Write-Host "  ❌ Failed to import: $($_.Exception.Message)" -ForegroundColor Red
        }
    } else {
        Write-Host "  ❌ PowerShell Editor Services not found" -ForegroundColor Red
    }
} catch {
    Write-Host "  ❌ Error during activation: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Method 4: Check VS Code processes and environment
Write-Host "Method 4: VS Code Process Check" -ForegroundColor Yellow
$vscodeProcesses = Get-Process -Name "Code" -ErrorAction SilentlyContinue
if ($vscodeProcesses) {
    Write-Host "  ✅ VS Code processes running: $($vscodeProcesses.Count)" -ForegroundColor Green
    foreach ($proc in $vscodeProcesses) {
        Write-Host "    PID: $($proc.Id), Window Title: $($proc.MainWindowTitle)" -ForegroundColor Cyan
    }
} else {
    Write-Host "  ❌ No VS Code processes found" -ForegroundColor Red
}

# Check environment variables that VS Code sets
$vscodeVars = @('VSCODE_PID', 'VSCODE_CWD', 'TERM_PROGRAM')
foreach ($var in $vscodeVars) {
    $value = [Environment]::GetEnvironmentVariable($var)
    if ($value) {
        Write-Host "  ✅ $var = $value" -ForegroundColor Green
    } else {
        Write-Host "  ❌ $var = (not set)" -ForegroundColor Red
    }
}
Write-Host ""

# Method 5: Try manual PowerShell extension commands
Write-Host "Method 5: Manual Extension Command Test" -ForegroundColor Yellow
$testCommands = @(
    'Get-Command Register-EditorCommand -ErrorAction SilentlyContinue',
    'Get-Command Show-Command -ErrorAction SilentlyContinue',
    'Get-Module PowerShellEditorServices* -ListAvailable'
)

foreach ($cmd in $testCommands) {
    try {
        $result = Invoke-Expression $cmd
        if ($result) {
            Write-Host "  ✅ $cmd - Found" -ForegroundColor Green
            if ($cmd -like "*Get-Module*") {
                $result | ForEach-Object { Write-Host "    $($_.Name) v$($_.Version)" -ForegroundColor Cyan }
            }
        } else {
            Write-Host "  ❌ $cmd - Not found" -ForegroundColor Red
        }
    } catch {
        Write-Host "  ❌ $cmd - Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}
Write-Host ""

# Summary and recommendations
Write-Host "=== Recommendations ===" -ForegroundColor Cyan
Write-Host "1. If extension is not installed:" -ForegroundColor Yellow
Write-Host "   - Open VS Code Extensions (Ctrl+Shift+X)" -ForegroundColor White
Write-Host "   - Search for 'PowerShell'" -ForegroundColor White
Write-Host "   - Install 'PowerShell' by Microsoft" -ForegroundColor White
Write-Host ""
Write-Host "2. If extension is installed but not working:" -ForegroundColor Yellow
Write-Host "   - Restart VS Code completely" -ForegroundColor White
Write-Host "   - Use Command Palette: 'PowerShell: Restart Extension Host'" -ForegroundColor White
Write-Host "   - Check VS Code settings for PowerShell configuration" -ForegroundColor White
Write-Host ""
Write-Host "3. Alternative: Use PowerShell Integrated Console" -ForegroundColor Yellow
Write-Host "   - Command Palette: 'PowerShell: Show Integrated Console'" -ForegroundColor White
Write-Host "   - This should force the extension to activate" -ForegroundColor White
Write-Host ""
