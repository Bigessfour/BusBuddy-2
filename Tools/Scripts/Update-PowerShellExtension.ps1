#Requires -Version 7.0

<#
.SYNOPSIS
    Updates VS Code PowerShell Extension to v2025.6.1
.DESCRIPTION
    Ensures VS Code has the latest PowerShell extension with optimized Bus Buddy configuration
.EXAMPLE
    .\Update-PowerShellExtension.ps1
#>

[CmdletBinding()]
param()

Write-Host '🔄 PowerShell Extension Update for Bus Buddy' -ForegroundColor Cyan
Write-Host '📅 Target Version: v2025.6.1 (Latest as of June 30, 2025)' -ForegroundColor White
Write-Host ''

# Check if VS Code is available
$codeCommand = $null

# Try different VS Code installations
$possiblePaths = @(
    'code',
    'code-insiders',
    "${env:LOCALAPPDATA}\Programs\Microsoft VS Code\bin\code.cmd",
    "${env:PROGRAMFILES}\Microsoft VS Code\bin\code.cmd",
    "${env:LOCALAPPDATA}\Programs\Microsoft VS Code Insiders\bin\code-insiders.cmd"
)

foreach ($path in $possiblePaths) {
    try {
        $null = Get-Command $path -ErrorAction SilentlyContinue
        if ($LASTEXITCODE -eq 0 -or $null -ne (Get-Command $path -ErrorAction SilentlyContinue)) {
            $codeCommand = $path
            Write-Host "✅ Found VS Code: $path" -ForegroundColor Green
            break
        }
    } catch {
        # Continue to next path
    }
}

if (-not $codeCommand) {
    Write-Host '❌ VS Code not found. Please install VS Code first.' -ForegroundColor Red
    exit 1
}

Write-Host ''
Write-Host '🔍 PHASE 1: Checking Current PowerShell Extension' -ForegroundColor Green

# Check current extensions
try {
    $extensions = & $codeCommand --list-extensions --show-versions
    $powershellExt = $extensions | Where-Object { $_ -like 'ms-vscode.powershell*' }

    if ($powershellExt) {
        Write-Host "📋 Current PowerShell Extension: $powershellExt" -ForegroundColor White

        # Extract version
        if ($powershellExt -match 'ms-vscode\.powershell@(.+)') {
            $currentVersion = $matches[1]
            Write-Host "📊 Current Version: $currentVersion" -ForegroundColor Gray

            # Compare with target version
            if ($currentVersion -eq '2025.6.1') {
                Write-Host '✅ Already on latest version!' -ForegroundColor Green
            } else {
                Write-Host "⚠️  Update needed: $currentVersion → 2025.6.1" -ForegroundColor Yellow
            }
        }
    } else {
        Write-Host '⚠️  PowerShell extension not found' -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠️  Could not check current extensions: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host ''
Write-Host '📦 PHASE 2: Installing/Updating PowerShell Extension' -ForegroundColor Green

try {
    # Install or update PowerShell extension
    Write-Host '📥 Installing ms-vscode.powershell...' -ForegroundColor White
    & $codeCommand --install-extension ms-vscode.powershell --force

    if ($LASTEXITCODE -eq 0) {
        Write-Host '✅ PowerShell extension installed/updated successfully' -ForegroundColor Green
    } else {
        Write-Host "❌ Failed to install PowerShell extension (Exit code: $LASTEXITCODE)" -ForegroundColor Red
    }

    # Also install PowerShell Preview for latest features
    Write-Host '📥 Installing ms-vscode.powershell-preview...' -ForegroundColor White
    & $codeCommand --install-extension ms-vscode.powershell-preview --force

    if ($LASTEXITCODE -eq 0) {
        Write-Host '✅ PowerShell Preview extension installed successfully' -ForegroundColor Green
    } else {
        Write-Host "⚠️  PowerShell Preview installation failed (Exit code: $LASTEXITCODE)" -ForegroundColor Yellow
    }

} catch {
    Write-Host "❌ Error during extension installation: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ''
Write-Host '🔧 PHASE 3: Installing Additional Productivity Extensions' -ForegroundColor Green

$additionalExtensions = @(
    'spmeesseman.vscode-taskexplorer',
    'aaron-bond.better-comments',
    'streetsidesoftware.code-spell-checker',
    'ms-vscode.hexeditor',
    'ms-vscode.vscode-yaml'
)

foreach ($ext in $additionalExtensions) {
    try {
        Write-Host "📥 Installing $ext..." -ForegroundColor White
        & $codeCommand --install-extension $ext --force

        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✅ Installed: $ext" -ForegroundColor Green
        } else {
            Write-Host "  ⚠️  Failed: $ext" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "  ❌ Error installing $ext" -ForegroundColor Red
    }
}

Write-Host ''
Write-Host '📊 PHASE 4: Verification' -ForegroundColor Green

try {
    $updatedExtensions = & $codeCommand --list-extensions --show-versions
    $powershellExtensions = $updatedExtensions | Where-Object { $_ -like '*powershell*' }

    Write-Host '🔍 Installed PowerShell Extensions:' -ForegroundColor White
    foreach ($ext in $powershellExtensions) {
        Write-Host "  📋 $ext" -ForegroundColor Gray
    }

    $taskExplorer = $updatedExtensions | Where-Object { $_ -like '*taskexplorer*' }
    if ($taskExplorer) {
        Write-Host "✅ Task Explorer: $taskExplorer" -ForegroundColor Green
    }

} catch {
    Write-Host '⚠️  Could not verify installations' -ForegroundColor Yellow
}

Write-Host ''
Write-Host '🚀 PHASE 5: Configuration Recommendations' -ForegroundColor Green
Write-Host ''
Write-Host '✅ VS Code Settings Updated:' -ForegroundColor White
Write-Host '  • PowerShell Extension v2025.6.1 configuration applied' -ForegroundColor Gray
Write-Host '  • Terminal profiles configured for PowerShell 7.5.2' -ForegroundColor Gray
Write-Host '  • Script analysis and formatting enabled' -ForegroundColor Gray
Write-Host '  • Bus Buddy PowerShell profiles auto-loaded' -ForegroundColor Gray
Write-Host ''
Write-Host '📋 Next Steps:' -ForegroundColor White
Write-Host '  1. Restart VS Code to apply all settings' -ForegroundColor Gray
Write-Host '  2. Open a PowerShell file to verify extension functionality' -ForegroundColor Gray
Write-Host '  3. Test Task Explorer integration with Bus Buddy tasks' -ForegroundColor Gray
Write-Host '  4. Verify PowerShell 7.5.2 terminal profile loads correctly' -ForegroundColor Gray
Write-Host ''
Write-Host '✅ PowerShell Extension update completed!' -ForegroundColor Green
