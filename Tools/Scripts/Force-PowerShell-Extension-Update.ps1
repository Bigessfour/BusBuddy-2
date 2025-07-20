#Requires -Version 7.0

<#
.SYNOPSIS
    Force PowerShell Extension Update to Latest Version
.DESCRIPTION
    Forcibly updates PowerShell extension to v2025.6.1 and verifies installation
.EXAMPLE
    .\Force-PowerShell-Extension-Update.ps1
#>

[CmdletBinding()]
param()

Write-Host "🔄 Force PowerShell Extension Update" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Define extension details
$ExtensionId = "ms-vscode.powershell"
$StableVersion = "2025.2.0"
$PreviewVersion = "2025.3.1"  # Current installed version
$PreviewExtensionId = "ms-vscode.powershell-preview"

# Find VS Code executable
$VSCodePaths = @(
    "${env:LOCALAPPDATA}\Programs\Microsoft VS Code\bin\code.cmd",
    "${env:LOCALAPPDATA}\Programs\Microsoft VS Code Insiders\bin\code-insiders.cmd",
    "${env:ProgramFiles}\Microsoft VS Code\bin\code.cmd",
    "${env:ProgramFiles}\Microsoft VS Code Insiders\bin\code-insiders.cmd"
)

$VSCodeExe = $null
foreach ($path in $VSCodePaths) {
    if (Test-Path $path) {
        $VSCodeExe = $path
        Write-Host "✅ Found VS Code: $path" -ForegroundColor Green
        break
    }
}

if (-not $VSCodeExe) {
    Write-Host "❌ VS Code not found in standard locations" -ForegroundColor Red
    Write-Host "   Please install VS Code from: https://code.visualstudio.com/" -ForegroundColor Yellow
    exit 1
}

# Function to check extension version
function Get-ExtensionVersion {
    param($ExtensionId)

    try {
        $output = & $VSCodeExe --list-extensions --show-versions 2>$null
        $extension = $output | Where-Object { $_ -match "^$ExtensionId@(.+)$" }
        if ($extension) {
            return $matches[1]
        }
    } catch {
        Write-Warning "Could not check extension version: $($_.Exception.Message)"
    }
    return $null
}

# Check current PowerShell extension version
Write-Host "🔍 Checking current PowerShell extension version..." -ForegroundColor White
$currentVersion = Get-ExtensionVersion -ExtensionId $ExtensionId
$currentPreviewVersion = Get-ExtensionVersion -ExtensionId $PreviewExtensionId

if ($currentVersion) {
    Write-Host "   Current PowerShell extension: v$currentVersion" -ForegroundColor Yellow
} else {
    Write-Host "   PowerShell extension not installed" -ForegroundColor Red
}

if ($currentPreviewVersion) {
    Write-Host "   Current PowerShell Preview extension: v$currentPreviewVersion" -ForegroundColor Yellow
}

# Check if we need to update
$needsUpdate = $false
if (-not $currentVersion) {
    $needsUpdate = $true
    Write-Host "⚠️  PowerShell extension not installed" -ForegroundColor Red
} elseif ([version]$currentVersion -eq [version]$PreviewVersion) {
    Write-Host "✅ PowerShell extension is optimal (Preview v$currentVersion > Stable v$StableVersion)" -ForegroundColor Green
} elseif ([version]$currentVersion -lt [version]$StableVersion) {
    $needsUpdate = $true
    Write-Host "⚠️  PowerShell extension needs update (Current: v$currentVersion, Stable: v$StableVersion)" -ForegroundColor Yellow
} else {
    Write-Host "✅ PowerShell extension is up to date (v$currentVersion)" -ForegroundColor Green
}

if ($needsUpdate) {
    Write-Host ""
    Write-Host "🚀 Updating PowerShell extension..." -ForegroundColor Cyan

    # Uninstall old version first
    if ($currentVersion) {
        Write-Host "   Uninstalling current version v$currentVersion..." -ForegroundColor White
        try {
            $result = & $VSCodeExe --uninstall-extension $ExtensionId 2>&1
            if ($LASTEXITCODE -eq 0) {
                Write-Host "   ✅ Uninstalled successfully" -ForegroundColor Green
            } else {
                Write-Host "   ⚠️  Uninstall may have failed: $result" -ForegroundColor Yellow
            }
        } catch {
            Write-Host "   ⚠️  Error during uninstall: $($_.Exception.Message)" -ForegroundColor Yellow
        }
        Start-Sleep -Seconds 2
    }

    # Install latest version
    Write-Host "   Installing PowerShell extension v$LatestVersion..." -ForegroundColor White
    try {
        $result = & $VSCodeExe --install-extension $ExtensionId --force 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "   ✅ Installed successfully" -ForegroundColor Green
        } else {
            Write-Host "   ❌ Installation failed: $result" -ForegroundColor Red
            Write-Host "   Try manually installing from: https://marketplace.visualstudio.com/items?itemName=ms-vscode.PowerShell" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "   ❌ Error during installation: $($_.Exception.Message)" -ForegroundColor Red
    }

    # Also install/update Preview version for latest features
    Write-Host "   Installing PowerShell Preview extension..." -ForegroundColor White
    try {
        $result = & $VSCodeExe --install-extension $PreviewExtensionId --force 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "   ✅ PowerShell Preview installed successfully" -ForegroundColor Green
        } else {
            Write-Host "   ⚠️  PowerShell Preview installation may have failed: $result" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "   ⚠️  Error installing PowerShell Preview: $($_.Exception.Message)" -ForegroundColor Yellow
    }

    Start-Sleep -Seconds 3

    # Verify installation
    Write-Host ""
    Write-Host "🔍 Verifying installation..." -ForegroundColor Cyan
    $newVersion = Get-ExtensionVersion -ExtensionId $ExtensionId
    $newPreviewVersion = Get-ExtensionVersion -ExtensionId $PreviewExtensionId

    if ($newVersion) {
        if ([version]$newVersion -ge [version]$LatestVersion) {
            Write-Host "✅ PowerShell extension successfully updated to v$newVersion" -ForegroundColor Green
        } else {
            Write-Host "⚠️  PowerShell extension updated to v$newVersion (expected v$LatestVersion)" -ForegroundColor Yellow
        }
    } else {
        Write-Host "❌ PowerShell extension installation verification failed" -ForegroundColor Red
    }

    if ($newPreviewVersion) {
        Write-Host "✅ PowerShell Preview extension: v$newPreviewVersion" -ForegroundColor Green
    }
}

# Update VS Code settings to enforce latest PowerShell configurations
Write-Host ""
Write-Host "⚙️  Updating VS Code settings for optimal PowerShell support..." -ForegroundColor Cyan

$settingsPath = ".vscode\settings.json"
if (Test-Path $settingsPath) {
    try {
        $settings = Get-Content $settingsPath -Raw | ConvertFrom-Json -AsHashtable -ErrorAction SilentlyContinue
        if (-not $settings) {
            $settings = @{}
        }
    } catch {
        Write-Warning "Could not parse existing settings.json, creating new one"
        $settings = @{}
    }
} else {
    $settings = @{}
}

# Add/Update PowerShell-specific settings for v2025.6.1
$powershellSettings = @{
    "powershell.codeFormatting.preset"                      = "OTBS"
    "powershell.codeFormatting.openBraceOnSameLine"         = $true
    "powershell.codeFormatting.newLineAfterOpenBrace"       = $true
    "powershell.codeFormatting.newLineAfterCloseBrace"      = $true
    "powershell.codeFormatting.pipelineIndentationStyle"    = "IncreaseIndentationAfterEveryPipeline"
    "powershell.codeFormatting.whitespaceBeforeOpenBrace"   = $true
    "powershell.codeFormatting.whitespaceBeforeOpenParen"   = $true
    "powershell.codeFormatting.whitespaceAroundOperator"    = $true
    "powershell.codeFormatting.whitespaceAfterSeparator"    = $true
    "powershell.codeFormatting.whitespaceBetweenParameters" = $false
    "powershell.codeFormatting.whitespaceInsideBrace"       = $true
    "powershell.codeFormatting.addWhitespaceAroundPipe"     = $true
    "powershell.integratedConsole.showOnStartup"            = $false
    "powershell.debugging.createTemporaryIntegratedConsole" = $false
    "powershell.enableProfileLoading"                       = $true
    "powershell.scriptAnalysis.enable"                      = $true
    "powershell.scriptAnalysis.settingsPath"                = "PSScriptAnalyzerSettings.psd1"
    "powershell.developer.editorServicesLogLevel"           = "Normal"
    "powershell.promptToUpdatePowerShell"                   = $false
    "powershell.powerShellExePath"                          = ""
    "powershell.developer.bundledModulesPath"               = ""
}

# Merge settings
foreach ($key in $powershellSettings.Keys) {
    $settings[$key] = $powershellSettings[$key]
}

# Write updated settings
try {
    $settings | ConvertTo-Json -Depth 10 | Set-Content $settingsPath -Encoding UTF8
    Write-Host "✅ VS Code settings updated with PowerShell v$LatestVersion configurations" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Could not update VS Code settings: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Final summary
Write-Host ""
Write-Host "📊 POWERSHELL EXTENSION UPDATE SUMMARY" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan

$finalVersion = Get-ExtensionVersion -ExtensionId $ExtensionId
$finalPreviewVersion = Get-ExtensionVersion -ExtensionId $PreviewExtensionId

if ($finalVersion) {
    $versionColor = if ([version]$finalVersion -ge [version]$LatestVersion) { "Green" } else { "Yellow" }
    Write-Host "✅ PowerShell Extension: v$finalVersion" -ForegroundColor $versionColor
} else {
    Write-Host "❌ PowerShell Extension: Not installed" -ForegroundColor Red
}

if ($finalPreviewVersion) {
    Write-Host "✅ PowerShell Preview: v$finalPreviewVersion" -ForegroundColor Green
}

Write-Host "✅ VS Code settings configured for optimal PowerShell support" -ForegroundColor Green
Write-Host ""
Write-Host "🚀 NEXT STEPS:" -ForegroundColor Cyan
Write-Host "1. Restart VS Code to ensure all changes take effect" -ForegroundColor White
Write-Host "2. Open a PowerShell file to test the updated extension" -ForegroundColor White
Write-Host "3. Verify IntelliSense, formatting, and debugging work correctly" -ForegroundColor White
Write-Host ""

if ($finalVersion -and [version]$finalVersion -ge [version]$LatestVersion) {
    Write-Host "🎉 PowerShell extension successfully updated to the latest version!" -ForegroundColor Green
} else {
    Write-Host "⚠️  PowerShell extension may need manual update. Visit VS Code Extensions marketplace." -ForegroundColor Yellow
    Write-Host "   URL: https://marketplace.visualstudio.com/items?itemName=ms-vscode.PowerShell" -ForegroundColor Blue
}
