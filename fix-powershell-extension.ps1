#Requires -Version 7.0
<#
.SYNOPSIS
    Remove Old PowerShell Extension and Fix Editor Services
.DESCRIPTION
    This script removes the older PowerShell extension version and ensures
    the latest version (2025.3.1) is used for Editor Services.
#>

Write-Host "=== PowerShell Extension Cleanup ===" -ForegroundColor Cyan
Write-Host ""

# Check extension directories
$extensionPaths = @(
    "$env:USERPROFILE\.vscode\extensions",
    "$env:USERPROFILE\.vscode-insiders\extensions"
)

foreach ($extensionPath in $extensionPaths) {
    if (Test-Path $extensionPath) {
        Write-Host "Checking: $extensionPath" -ForegroundColor Yellow

        # Find all PowerShell extensions
        $psExtensions = Get-ChildItem $extensionPath -Directory | Where-Object {
            $_.Name -like "*ms-vscode.powershell*"
        } | Sort-Object Name

        if ($psExtensions.Count -gt 0) {
            Write-Host "Found PowerShell extensions:" -ForegroundColor Green
            foreach ($ext in $psExtensions) {
                Write-Host "  - $($ext.Name)" -ForegroundColor Cyan

                # Get version from folder name
                if ($ext.Name -match "ms-vscode\.powershell-(.+)") {
                    $version = $matches[1]
                    Write-Host "    Version: $version" -ForegroundColor White
                }
            }

            # Identify old versions to remove
            $oldExtensions = $psExtensions | Where-Object {
                $_.Name -like "*ms-vscode.powershell-2025.0.0*"
            }

            if ($oldExtensions) {
                Write-Host ""
                Write-Host "🗑️ Removing old PowerShell extension versions:" -ForegroundColor Red
                foreach ($oldExt in $oldExtensions) {
                    try {
                        Write-Host "  Removing: $($oldExt.Name)" -ForegroundColor Yellow
                        Remove-Item $oldExt.FullName -Recurse -Force
                        Write-Host "  ✅ Successfully removed: $($oldExt.Name)" -ForegroundColor Green
                    } catch {
                        Write-Host "  ❌ Failed to remove: $($oldExt.Name) - $($_.Exception.Message)" -ForegroundColor Red
                        Write-Host "     You may need to close VS Code completely and run this script again" -ForegroundColor Yellow
                    }
                }
            } else {
                Write-Host "✅ No old PowerShell extensions found to remove" -ForegroundColor Green
            }

            # Verify latest version exists
            $latestExtension = $psExtensions | Where-Object {
                $_.Name -like "*ms-vscode.powershell-2025.3.1*"
            }

            if ($latestExtension) {
                Write-Host ""
                Write-Host "✅ Latest PowerShell extension found: $($latestExtension.Name)" -ForegroundColor Green

                # Check Editor Services in latest version
                $editorServicesPath = Join-Path $latestExtension.FullName "modules\PowerShellEditorServices\PowerShellEditorServices.psd1"
                if (Test-Path $editorServicesPath) {
                    Write-Host "✅ Editor Services found in latest extension" -ForegroundColor Green
                    Write-Host "   Path: $editorServicesPath" -ForegroundColor Cyan
                } else {
                    Write-Host "❌ Editor Services not found in latest extension" -ForegroundColor Red
                }
            } else {
                Write-Host "⚠️ Latest PowerShell extension (2025.3.1) not found" -ForegroundColor Yellow
                Write-Host "   You may need to update the PowerShell extension in VS Code" -ForegroundColor Yellow
            }
        } else {
            Write-Host "⚠️ No PowerShell extensions found in: $extensionPath" -ForegroundColor Yellow
        }
        Write-Host ""
    }
}

Write-Host "=== Recommendations ===" -ForegroundColor Cyan
Write-Host "1. Close VS Code completely" -ForegroundColor Yellow
Write-Host "2. Restart VS Code" -ForegroundColor Yellow
Write-Host "3. Open PowerShell integrated console" -ForegroundColor Yellow
Write-Host "4. Run: .\check-powershell-extension.ps1" -ForegroundColor Yellow
Write-Host ""
Write-Host "If issues persist:" -ForegroundColor Yellow
Write-Host "- Go to Extensions (Ctrl+Shift+X)" -ForegroundColor White
Write-Host "- Uninstall PowerShell extension" -ForegroundColor White
Write-Host "- Reinstall PowerShell extension (latest version)" -ForegroundColor White
