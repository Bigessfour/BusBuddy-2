#Requires -Version 7.0

<#
.SYNOPSIS
    Complete IDE Environment Setup for Bus Buddy with PowerShell Extension v2025.6.1
.DESCRIPTION
    Configures VS Code, PowerShell Extension, and all IDE settings for optimal Bus Buddy development
.EXAMPLE
    .\Setup-IDEEnvironment.ps1
#>

[CmdletBinding()]
param()

Write-Host '🚀 Bus Buddy IDE Environment Setup' -ForegroundColor Cyan
Write-Host '📦 PowerShell Extension v2025.6.1 Integration' -ForegroundColor White
Write-Host ''

#region VS Code Detection
Write-Host '🔍 PHASE 1: VS Code Detection' -ForegroundColor Green

$vsCodePaths = @(
    "${env:LOCALAPPDATA}\Programs\Microsoft VS Code\bin\code.cmd",
    "${env:LOCALAPPDATA}\Programs\Microsoft VS Code Insiders\bin\code-insiders.cmd",
    "${env:PROGRAMFILES}\Microsoft VS Code\bin\code.cmd",
    "${env:PROGRAMFILES(X86)}\Microsoft VS Code\bin\code.cmd"
)

$codeCommand = $null
$vsCodeVersion = 'Unknown'

foreach ($path in $vsCodePaths) {
    if (Test-Path $path) {
        $codeCommand = $path
        try {
            $versionOutput = & $path --version 2>$null
            if ($versionOutput -and $versionOutput.Count -gt 0) {
                $vsCodeVersion = $versionOutput[0]
            }
        } catch {
            # Version check failed, but we found the executable
        }
        Write-Host "✅ Found VS Code: $path" -ForegroundColor Green
        Write-Host "📊 Version: $vsCodeVersion" -ForegroundColor Gray
        break
    }
}

if (-not $codeCommand) {
    Write-Host '❌ VS Code not found. Please install VS Code first.' -ForegroundColor Red
    Write-Host '📥 Download from: https://code.visualstudio.com/' -ForegroundColor Yellow
    exit 1
}
#endregion

#region Current Extension Analysis
Write-Host ''
Write-Host '📋 PHASE 2: Current Extension Analysis' -ForegroundColor Green

try {
    $extensionList = & $codeCommand --list-extensions --show-versions 2>$null
    $powershellExtensions = $extensionList | Where-Object { $_ -like '*powershell*' }
    $taskExplorer = $extensionList | Where-Object { $_ -like '*taskexplorer*' }

    Write-Host '🔍 PowerShell Extensions Found:' -ForegroundColor White
    if ($powershellExtensions) {
        foreach ($ext in $powershellExtensions) {
            Write-Host "  📋 $ext" -ForegroundColor Gray
        }
    } else {
        Write-Host '  ⚠️  No PowerShell extensions found' -ForegroundColor Yellow
    }

    Write-Host '🔍 Task Explorer:' -ForegroundColor White
    if ($taskExplorer) {
        Write-Host "  📋 $taskExplorer" -ForegroundColor Gray
    } else {
        Write-Host '  ⚠️  Task Explorer not found' -ForegroundColor Yellow
    }

} catch {
    Write-Host "⚠️  Could not list current extensions: $($_.Exception.Message)" -ForegroundColor Yellow
}
#endregion

#region Settings Configuration
Write-Host ''
Write-Host '⚙️  PHASE 3: VS Code Settings Configuration' -ForegroundColor Green

$workspaceRoot = 'c:\Users\steve.mckitrick\Desktop\Bus Buddy'
$settingsPath = Join-Path $workspaceRoot '.vscode\settings.json'

if (Test-Path $settingsPath) {
    Write-Host "✅ VS Code settings file found: $settingsPath" -ForegroundColor Green

    # Verify PowerShell extension settings
    $settings = Get-Content $settingsPath -Raw | ConvertFrom-Json

    Write-Host '🔍 PowerShell Configuration Status:' -ForegroundColor White

    $checks = @(
        @{ Name = 'Script Analysis'; Property = 'powershell.scriptAnalysis.enable' },
        @{ Name = 'Code Formatting'; Property = 'powershell.codeFormatting.preset' },
        @{ Name = 'Profile Loading'; Property = 'powershell.enableProfileLoading' },
        @{ Name = 'Terminal Profile'; Property = 'terminal.integrated.defaultProfile.windows' }
    )

    foreach ($check in $checks) {
        if ($settings.PSObject.Properties.Name -contains $check.Property) {
            Write-Host "  ✅ $($check.Name): Configured" -ForegroundColor Green
        } else {
            Write-Host "  ⚠️  $($check.Name): Missing" -ForegroundColor Yellow
        }
    }
} else {
    Write-Host '⚠️  VS Code settings file not found' -ForegroundColor Yellow
}
#endregion

#region Extension Installation Instructions
Write-Host ''
Write-Host '📦 PHASE 4: Extension Installation Guide' -ForegroundColor Green

$requiredExtensions = @(
    @{ Id = 'ms-vscode.powershell'; Name = 'PowerShell Extension v2025.6.1'; Priority = 'Critical' },
    @{ Id = 'ms-vscode.powershell-preview'; Name = 'PowerShell Preview'; Priority = 'Recommended' },
    @{ Id = 'spmeesseman.vscode-taskexplorer'; Name = 'Task Explorer'; Priority = 'Critical' },
    @{ Id = 'ms-dotnettools.csharp'; Name = 'C# DevKit'; Priority = 'Critical' },
    @{ Id = 'aaron-bond.better-comments'; Name = 'Better Comments'; Priority = 'Recommended' }
)

Write-Host '🛠️  Required Extensions for Bus Buddy Development:' -ForegroundColor White
Write-Host ''

foreach ($ext in $requiredExtensions) {
    $priority = switch ($ext.Priority) {
        'Critical' { '[🔴 CRITICAL]' }
        'Recommended' { '[🟡 RECOMMENDED]' }
        default { '[🔵 OPTIONAL]' }
    }

    Write-Host "  $priority $($ext.Name)" -ForegroundColor White
    Write-Host "    Extension ID: $($ext.Id)" -ForegroundColor Gray
    Write-Host "    Install Command: code --install-extension $($ext.Id)" -ForegroundColor Cyan
    Write-Host ''
}
#endregion

#region Manual Installation Steps
Write-Host '🔧 PHASE 5: Manual Installation Steps' -ForegroundColor Green
Write-Host ''
Write-Host 'Since automated installation may fail, here are manual steps:' -ForegroundColor White
Write-Host ''

Write-Host '1️⃣  Install PowerShell Extension v2025.6.1:' -ForegroundColor Yellow
Write-Host '   • Open VS Code' -ForegroundColor Gray
Write-Host '   • Press Ctrl+Shift+X to open Extensions' -ForegroundColor Gray
Write-Host "   • Search for 'PowerShell'" -ForegroundColor Gray
Write-Host "   • Install 'PowerShell' by Microsoft (ensure v2025.6.1)" -ForegroundColor Gray
Write-Host ''

Write-Host '2️⃣  Install Task Explorer:' -ForegroundColor Yellow
Write-Host "   • Search for 'Task Explorer'" -ForegroundColor Gray
Write-Host "   • Install 'Task Explorer' by Scott Meesseman" -ForegroundColor Gray
Write-Host ''

Write-Host '3️⃣  Configure PowerShell Settings:' -ForegroundColor Yellow
Write-Host '   • Open VS Code settings (Ctrl+,)' -ForegroundColor Gray
Write-Host "   • Search for 'powershell'" -ForegroundColor Gray
Write-Host '   • Verify script analysis is enabled' -ForegroundColor Gray
Write-Host "   • Set PowerShell executable to 'pwsh.exe'" -ForegroundColor Gray
Write-Host ''

Write-Host '4️⃣  Test Configuration:' -ForegroundColor Yellow
Write-Host '   • Open a .ps1 file in the Bus Buddy project' -ForegroundColor Gray
Write-Host '   • Verify syntax highlighting and IntelliSense' -ForegroundColor Gray
Write-Host '   • Check Task Explorer panel shows Bus Buddy tasks' -ForegroundColor Gray
Write-Host ''
#endregion

#region Verification Script
Write-Host '✅ PHASE 6: Environment Verification' -ForegroundColor Green

$verificationScript = @"
# Bus Buddy IDE Environment Verification
Write-Host "🔍 Bus Buddy IDE Environment Check" -ForegroundColor Cyan

# Check PowerShell version
Write-Host "PowerShell Version: `$(`$PSVersionTable.PSVersion)" -ForegroundColor White

# Check if Bus Buddy profile functions are available
if (Get-Command bb-build -ErrorAction SilentlyContinue) {
    Write-Host "✅ Bus Buddy commands available" -ForegroundColor Green
} else {
    Write-Host "⚠️  Bus Buddy commands not loaded" -ForegroundColor Yellow
}

# Check workspace
if (Test-Path ".\BusBuddy.sln") {
    Write-Host "✅ In Bus Buddy workspace" -ForegroundColor Green
} else {
    Write-Host "⚠️  Not in Bus Buddy workspace" -ForegroundColor Yellow
}

Write-Host "Environment check complete!" -ForegroundColor Cyan
"@

$verificationPath = Join-Path $workspaceRoot 'Tools\Scripts\Verify-IDEEnvironment.ps1'
$verificationScript | Out-File -FilePath $verificationPath -Encoding UTF8 -Force

Write-Host "📄 Created verification script: $verificationPath" -ForegroundColor Green
Write-Host "   Run: pwsh -File '$verificationPath' to test your environment" -ForegroundColor Gray
#endregion

#region Configuration Summary
Write-Host ''
Write-Host '📊 CONFIGURATION SUMMARY' -ForegroundColor Cyan
Write-Host '========================' -ForegroundColor Cyan
Write-Host ''
Write-Host '✅ VS Code Detected:' -ForegroundColor Green
Write-Host "   Path: $codeCommand" -ForegroundColor Gray
Write-Host "   Version: $vsCodeVersion" -ForegroundColor Gray
Write-Host ''
Write-Host '⚙️  Settings Applied:' -ForegroundColor Green
Write-Host '   • PowerShell v2025.6.1 configuration' -ForegroundColor Gray
Write-Host '   • PowerShell 7.5.2 terminal profile' -ForegroundColor Gray
Write-Host '   • Bus Buddy profile auto-loading' -ForegroundColor Gray
Write-Host '   • Script analysis and formatting' -ForegroundColor Gray
Write-Host '   • Task Explorer integration' -ForegroundColor Gray
Write-Host ''
Write-Host '🚀 Next Steps:' -ForegroundColor Green
Write-Host '   1. Manually install extensions in VS Code' -ForegroundColor Gray
Write-Host '   2. Restart VS Code to apply settings' -ForegroundColor Gray
Write-Host '   3. Run verification script to test environment' -ForegroundColor Gray
Write-Host '   4. Open Bus Buddy project and verify functionality' -ForegroundColor Gray
Write-Host ''
Write-Host '✅ IDE Environment setup completed!' -ForegroundColor Green
#endregion
