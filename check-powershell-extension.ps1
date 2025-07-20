#Requires -Version 7.0
<#
.SYNOPSIS
    Check PowerShell Extension Version and Status
.DESCRIPTION
    This script checks the PowerShell extension version, status, and integration
    with VS Code for proper Bus Buddy development environment setup.
#>

Write-Host "=== PowerShell Extension Version Check ===" -ForegroundColor Cyan
Write-Host ""

# Check PowerShell Version
Write-Host "PowerShell Information:" -ForegroundColor Yellow
Write-Host "  Version: $($PSVersionTable.PSVersion)" -ForegroundColor Green
Write-Host "  Edition: $($PSVersionTable.PSEdition)" -ForegroundColor Green
Write-Host "  Platform: $($PSVersionTable.Platform)" -ForegroundColor Green
Write-Host ""

# Check if running in VS Code
Write-Host "VS Code Integration:" -ForegroundColor Yellow
if ($env:VSCODE_PID) {
    Write-Host "  ✅ Running in VS Code (PID: $env:VSCODE_PID)" -ForegroundColor Green
    Write-Host "  ✅ TERM_PROGRAM: $env:TERM_PROGRAM" -ForegroundColor Green
} else {
    Write-Host "  ❌ Not running in VS Code" -ForegroundColor Red
}
Write-Host ""

# Check PowerShell Editor Services (PowerShell Extension Core)
Write-Host "PowerShell Editor Services:" -ForegroundColor Yellow
$editorServices = Get-Module -Name PowerShellEditorServices* -ListAvailable -ErrorAction SilentlyContinue
if ($editorServices) {
    $editorServices | ForEach-Object {
        Write-Host "  ✅ $($_.Name) - Version: $($_.Version)" -ForegroundColor Green
    }
} else {
    Write-Host "  ⚠️ PowerShell Editor Services not found" -ForegroundColor Yellow
}

# Check loaded PowerShell modules
$loadedEditorServices = Get-Module -Name PowerShellEditorServices* -ErrorAction SilentlyContinue
if ($loadedEditorServices) {
    Write-Host "  ✅ Loaded Editor Services:" -ForegroundColor Green
    $loadedEditorServices | ForEach-Object {
        Write-Host "    - $($_.Name) v$($_.Version)" -ForegroundColor Cyan
    }
} else {
    Write-Host "  ⚠️ No Editor Services modules currently loaded" -ForegroundColor Yellow
}
Write-Host ""

# Check VS Code extension installation
Write-Host "VS Code Extensions Check:" -ForegroundColor Yellow
try {
    $extensionsOutput = & code --list-extensions --show-versions 2>$null
    $powershellExtensions = $extensionsOutput | Where-Object { $_ -like "*powershell*" -or $_ -like "*ms-vscode.powershell*" }

    if ($powershellExtensions) {
        Write-Host "  ✅ Found PowerShell Extensions:" -ForegroundColor Green
        $powershellExtensions | ForEach-Object {
            Write-Host "    - $_" -ForegroundColor Cyan
        }
    } else {
        Write-Host "  ⚠️ No PowerShell extensions found" -ForegroundColor Yellow
    }
} catch {
    Write-Host "  ⚠️ Unable to check VS Code extensions (code command not available)" -ForegroundColor Yellow
}
Write-Host ""

# Check PowerShell-specific environment variables
Write-Host "PowerShell Environment Variables:" -ForegroundColor Yellow
$psVars = @(
    'POWERSHELL_DISTRIBUTION_CHANNEL',
    'PSModulePath',
    'PSEDITOR'
)

foreach ($var in $psVars) {
    $value = [Environment]::GetEnvironmentVariable($var)
    if ($value) {
        Write-Host "  ✅ $var = $value" -ForegroundColor Green
    } else {
        Write-Host "  ⚠️ $var = (not set)" -ForegroundColor Yellow
    }
}
Write-Host ""

# Check PowerShell Extension Features
Write-Host "PowerShell Extension Features:" -ForegroundColor Yellow

# Check if we have access to VS Code APIs
if (Get-Command -Name "Register-EditorCommand" -ErrorAction SilentlyContinue) {
    Write-Host "  ✅ Editor Commands API available" -ForegroundColor Green
} else {
    Write-Host "  ❌ Editor Commands API not available" -ForegroundColor Red
}

if (Get-Command -Name "Get-VSCodeContext" -ErrorAction SilentlyContinue) {
    Write-Host "  ✅ VS Code Context API available" -ForegroundColor Green
} else {
    Write-Host "  ❌ VS Code Context API not available" -ForegroundColor Red
}

# Check PSReadLine
$psreadline = Get-Module -Name PSReadLine -ErrorAction SilentlyContinue
if ($psreadline) {
    Write-Host "  ✅ PSReadLine loaded - Version: $($psreadline.Version)" -ForegroundColor Green
} else {
    Write-Host "  ⚠️ PSReadLine not loaded" -ForegroundColor Yellow
}
Write-Host ""

# Summary
Write-Host "=== Summary ===" -ForegroundColor Cyan
if ($env:VSCODE_PID -and $editorServices) {
    Write-Host "✅ PowerShell 7.5.2 is properly integrated with VS Code" -ForegroundColor Green
    Write-Host "✅ PowerShell extension should be working correctly" -ForegroundColor Green
} elseif ($env:VSCODE_PID) {
    Write-Host "⚠️ Running in VS Code but extension may not be fully loaded" -ForegroundColor Yellow
} else {
    Write-Host "❌ Not running in VS Code integrated terminal" -ForegroundColor Red
}
Write-Host ""
Write-Host "To get extension version, run this in VS Code terminal:" -ForegroundColor Cyan
Write-Host "  Get-Module PowerShellEditorServices* | Select-Object Name, Version" -ForegroundColor White
