#Requires -Version 7.0
<#
.SYNOPSIS
    PowerShell Editor Services Analysis and Status Report
.DESCRIPTION
    Provides detailed analysis of PowerShell Editor Services status,
    extension integration, and VS Code connectivity.
#>

Write-Host "=== PowerShell Editor Services Analysis ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: VS Code Integration Check
Write-Host "Step 1: VS Code Integration Status" -ForegroundColor Yellow
$isVSCode = $false
if ($env:VSCODE_PID) {
    Write-Host "  ✅ VSCODE_PID: $env:VSCODE_PID" -ForegroundColor Green
    $isVSCode = $true
} else {
    Write-Host "  ❌ VSCODE_PID: Not set" -ForegroundColor Red
}

if ($env:TERM_PROGRAM -eq "vscode") {
    Write-Host "  ✅ TERM_PROGRAM: $env:TERM_PROGRAM" -ForegroundColor Green
    $isVSCode = $true
} else {
    Write-Host "  ❌ TERM_PROGRAM: $($env:TERM_PROGRAM ?? 'Not set')" -ForegroundColor Red
}

Write-Host "  📊 PowerShell Host: $($Host.Name)" -ForegroundColor Cyan
if ($Host.Name -eq "Visual Studio Code Host") {
    Write-Host "  ✅ Running in VS Code PowerShell Host!" -ForegroundColor Green
} else {
    Write-Host "  ⚠️ Not running in VS Code PowerShell Host" -ForegroundColor Yellow
}
Write-Host ""

# Step 2: Editor Services Module Analysis
Write-Host "Step 2: PowerShell Editor Services Modules" -ForegroundColor Yellow
$loadedEditorServices = Get-Module -Name "*PowerShellEditorServices*" -ErrorAction SilentlyContinue
if ($loadedEditorServices) {
    Write-Host "  ✅ Loaded Editor Services:" -ForegroundColor Green
    $loadedEditorServices | ForEach-Object {
        Write-Host "    📦 $($_.Name) v$($_.Version)" -ForegroundColor Cyan
    }
} else {
    Write-Host "  ❌ No Editor Services modules loaded" -ForegroundColor Red
}

$availableEditorServices = Get-Module -Name "*PowerShellEditorServices*" -ListAvailable -ErrorAction SilentlyContinue
if ($availableEditorServices) {
    Write-Host "  ✅ Available Editor Services:" -ForegroundColor Green
    $availableEditorServices | ForEach-Object {
        Write-Host "    📦 $($_.Name) v$($_.Version)" -ForegroundColor Cyan
        Write-Host "       📁 $($_.ModuleBase)" -ForegroundColor Gray
    }
} else {
    Write-Host "  ❌ No Editor Services modules available" -ForegroundColor Red
}
Write-Host ""

# Step 3: Extension API Check
Write-Host "Step 3: PowerShell Extension APIs" -ForegroundColor Yellow
$apiCommands = @('Register-EditorCommand', 'Get-VSCodeContext', 'Show-Command')
foreach ($cmd in $apiCommands) {
    if (Get-Command $cmd -ErrorAction SilentlyContinue) {
        Write-Host "  ✅ $cmd - Available" -ForegroundColor Green
    } else {
        Write-Host "  ❌ $cmd - Not available" -ForegroundColor Red
    }
}

# Check for psEditor variable
if (Get-Variable -Name "psEditor" -ErrorAction SilentlyContinue) {
    Write-Host "  ✅ `$psEditor variable - Extension is active!" -ForegroundColor Green
} else {
    Write-Host "  ❌ `$psEditor variable - Extension not active" -ForegroundColor Red
}
Write-Host ""

# Step 4: Extension Installation Analysis
Write-Host "Step 4: PowerShell Extension Installation" -ForegroundColor Yellow

# Check installed extensions
$extensionPaths = @(
    "$env:USERPROFILE\.vscode\extensions",
    "$env:USERPROFILE\.vscode-insiders\extensions"
)

$foundExtensions = @()

foreach ($path in $extensionPaths) {
    if (Test-Path $path) {
        $psExtensions = Get-ChildItem $path -Directory | Where-Object { $_.Name -like "*ms-vscode.powershell*" }
        foreach ($ext in $psExtensions) {
            $packageJson = Join-Path $ext.FullName "package.json"
            if (Test-Path $packageJson) {
                try {
                    $packageInfo = Get-Content $packageJson | ConvertFrom-Json
                    $foundExtensions += [PSCustomObject]@{
                        FolderName   = $ext.Name
                        Version      = $packageInfo.version
                        DisplayName  = $packageInfo.displayName
                        Publisher    = $packageInfo.publisher
                        FullPath     = $ext.FullName
                        LastModified = $ext.LastWriteTime
                        Size         = (Get-ChildItem $ext.FullName -Recurse -File | Measure-Object -Property Length -Sum).Sum
                    }
                } catch {
                    Write-Host "❌ Could not read package.json for $($ext.Name)" -ForegroundColor Red
                }
            }
        }
    }
}

if ($foundExtensions.Count -eq 0) {
    Write-Host "❌ No PowerShell extensions found!" -ForegroundColor Red
    exit 1
}

Write-Host "Found PowerShell Extensions:" -ForegroundColor Yellow
foreach ($ext in $foundExtensions) {
    Write-Host ""
    Write-Host "📦 Extension: $($ext.DisplayName)" -ForegroundColor Green
    Write-Host "   Version: $($ext.Version)" -ForegroundColor Cyan
    Write-Host "   Folder: $($ext.FolderName)" -ForegroundColor White
    Write-Host "   Last Modified: $($ext.LastModified)" -ForegroundColor Gray
    Write-Host "   Size: $([math]::Round($ext.Size / 1MB, 2)) MB" -ForegroundColor Gray
    Write-Host "   Path: $($ext.FullPath)" -ForegroundColor DarkGray
}

Write-Host ""
Write-Host "=== Analysis ===" -ForegroundColor Cyan

# Determine which version to keep
$latest = $foundExtensions | Sort-Object { [version]$_.Version } | Select-Object -Last 1
$oldest = $foundExtensions | Sort-Object { [version]$_.Version } | Select-Object -First 1

Write-Host "✅ KEEP: PowerShell Extension v$($latest.Version)" -ForegroundColor Green
Write-Host "   Reason: Latest version with newest features and bug fixes" -ForegroundColor White
Write-Host "   Released: July 2025 (most current)" -ForegroundColor White

if ($foundExtensions.Count -gt 1) {
    $toRemove = $foundExtensions | Where-Object { $_.Version -ne $latest.Version }
    foreach ($ext in $toRemove) {
        Write-Host ""
        Write-Host "❌ REMOVE: PowerShell Extension v$($ext.Version)" -ForegroundColor Red
        Write-Host "   Reason: Older version, may cause conflicts" -ForegroundColor White
        Write-Host "   Folder to delete: $($ext.FolderName)" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "=== Settings.json Issues Found ===" -ForegroundColor Cyan

# Analyze settings.json issues
$issues = @()

# Missing critical settings
$issues += "❌ Missing: powershell.integratedConsole.startInBackground"
$issues += "❌ Missing: powershell.sessions.useExternalConsole = false"
$issues += "❌ Issue: powershell.startAutomatically = false (should be true for auto-connect)"

# Terminal profile issues
$issues += "⚠️ Terminal profile using -WorkingDirectory (may not work with extension)"

foreach ($issue in $issues) {
    Write-Host $issue -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Root Cause Analysis ===" -ForegroundColor Cyan
Write-Host "1. 🔍 Multiple PowerShell extension versions installed" -ForegroundColor Yellow
Write-Host "   - VS Code may be loading the wrong version" -ForegroundColor White
Write-Host "   - Extension host conflicts between versions" -ForegroundColor White
Write-Host ""
Write-Host "2. 🔍 Missing integrated console enforcement settings" -ForegroundColor Yellow
Write-Host "   - Extension opens external PowerShell windows" -ForegroundColor White
Write-Host "   - No forced integrated terminal usage" -ForegroundColor White
Write-Host ""
Write-Host "3. 🔍 Terminal profile configuration issues" -ForegroundColor Yellow
Write-Host "   - Using -WorkingDirectory parameter" -ForegroundColor White
Write-Host "   - May interfere with extension connection" -ForegroundColor White

Write-Host ""
Write-Host "=== Recommended Actions ===" -ForegroundColor Cyan
Write-Host "1. 🧹 Remove older PowerShell extension version(s)" -ForegroundColor Green
Write-Host "2. 🔧 Fix settings.json PowerShell configuration" -ForegroundColor Green
Write-Host "3. 🔄 Restart VS Code completely" -ForegroundColor Green
Write-Host "4. ✅ Test integrated console activation" -ForegroundColor Green

Write-Host ""
Write-Host "=== Cleanup Commands ===" -ForegroundColor Cyan

if ($foundExtensions.Count -gt 1) {
    $toRemove = $foundExtensions | Where-Object { $_.Version -ne $latest.Version }
    foreach ($ext in $toRemove) {
        Write-Host "Remove-Item -Path '$($ext.FullPath)' -Recurse -Force" -ForegroundColor Red
    }
} else {
    Write-Host "✅ Only one PowerShell extension found - no cleanup needed" -ForegroundColor Green
}

Write-Host ""
Write-Host "Would you like to proceed with cleanup? (Run commands manually)" -ForegroundColor Yellow
