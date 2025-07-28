#Requires -Version 7.5
<#
.SYNOPSIS
    BusBuddy MCP Server Diagnostic and Fix Tool

.DESCRIPTION
    Comprehensive diagnostic tool for Model Context Protocol (MCP) server issues
    in the BusBuddy development environment. Identifies and resolves common MCP
    configuration problems with VS Code and Copilot integration.

.EXAMPLE
    .\mcp-diagnostic-fix.ps1
    .\mcp-diagnostic-fix.ps1 -Fix
    .\mcp-diagnostic-fix.ps1 -Reinstall
#>

[CmdletBinding()]
param(
    [switch]$Fix,
    [switch]$Reinstall,
    [switch]$VerboseOutput
)

Write-Host "🤖 BusBuddy MCP Server Diagnostic Tool" -ForegroundColor Cyan
Write-Host "=====================================`n" -ForegroundColor Cyan

# Check 1: VS Code Extensions
Write-Host "1. Checking VS Code Extensions..." -ForegroundColor Yellow
$extensions = & code --list-extensions
$mcpExtension = $extensions | Where-Object { $_ -match "copilot-mcp" }
$copilotExtension = $extensions | Where-Object { $_ -match "github.copilot$" }
$copilotChatExtension = $extensions | Where-Object { $_ -match "github.copilot-chat" }

if ($mcpExtension) {
    Write-Host "   ✅ MCP Extension: $mcpExtension" -ForegroundColor Green
} else {
    Write-Host "   ❌ MCP Extension: NOT FOUND" -ForegroundColor Red
    $issues += "MCP extension not installed"
}

if ($copilotExtension) {
    Write-Host "   ✅ GitHub Copilot: $copilotExtension" -ForegroundColor Green
} else {
    Write-Host "   ❌ GitHub Copilot: NOT FOUND" -ForegroundColor Red
    $issues += "GitHub Copilot extension not installed"
}

if ($copilotChatExtension) {
    Write-Host "   ✅ Copilot Chat: $copilotChatExtension" -ForegroundColor Green
} else {
    Write-Host "   ❌ Copilot Chat: NOT FOUND" -ForegroundColor Red
    $issues += "GitHub Copilot Chat extension not installed"
}

# Check 2: MCP Configuration Files
Write-Host "`n2. Checking MCP Configuration Files..." -ForegroundColor Yellow
$globalMcpPath = "$env:USERPROFILE\.github\copilot\mcp.json"
$workspaceMcpPath = ".\mcp.json"

if (Test-Path $globalMcpPath) {
    Write-Host "   ✅ Global MCP Config: $globalMcpPath" -ForegroundColor Green
    $globalConfig = Get-Content $globalMcpPath -Raw | ConvertFrom-Json
    Write-Host "   📋 Servers configured:" -ForegroundColor Cyan
    foreach ($server in $globalConfig.mcpServers.PSObject.Properties) {
        Write-Host "      - $($server.Name): $($server.Value.serverUrl)" -ForegroundColor White
    }
} else {
    Write-Host "   ❌ Global MCP Config: NOT FOUND" -ForegroundColor Red
    $issues += "Global MCP configuration missing"
}

if (Test-Path $workspaceMcpPath) {
    Write-Host "   ✅ Workspace MCP Config: $workspaceMcpPath" -ForegroundColor Green
    $workspaceConfig = Get-Content $workspaceMcpPath -Raw | ConvertFrom-Json
    if ($workspaceConfig.mcpServers) {
        Write-Host "   📋 Workspace servers configured:" -ForegroundColor Cyan
        foreach ($server in $workspaceConfig.mcpServers.PSObject.Properties) {
            Write-Host "      - $($server.Name): $($server.Value.serverUrl)" -ForegroundColor White
        }
    }
} else {
    Write-Host "   ⚠️  Workspace MCP Config: NOT FOUND (optional)" -ForegroundColor Yellow
}

# Check 3: VS Code Settings
Write-Host "`n3. Checking VS Code Settings..." -ForegroundColor Yellow
$vscodeSettingsPath = ".\.vscode\settings.json"
if (Test-Path $vscodeSettingsPath) {
    Write-Host "   ✅ VS Code Settings: $vscodeSettingsPath" -ForegroundColor Green
    $settings = Get-Content $vscodeSettingsPath -Raw

    # Check for MCP-related settings
    if ($settings -match 'github\.copilot\.mcp') {
        Write-Host "   ✅ MCP settings found in VS Code configuration" -ForegroundColor Green
    } else {
        Write-Host "   ⚠️  No MCP settings found in VS Code configuration" -ForegroundColor Yellow
        $warnings += "No MCP settings in VS Code workspace"
    }
} else {
    Write-Host "   ❌ VS Code Settings: NOT FOUND" -ForegroundColor Red
    $issues += "VS Code settings.json missing"
}

# Check 4: Network Connectivity
Write-Host "`n4. Checking Network Connectivity..." -ForegroundColor Yellow
try {
    $tavilyUrl = "https://tavily.api.tadata.com"
    $response = Invoke-RestMethod -Uri $tavilyUrl -Method Head -TimeoutSec 10 -ErrorAction Stop
    Write-Host "   ✅ Tavily API: Accessible" -ForegroundColor Green
} catch {
    Write-Host "   ❌ Tavily API: Connection failed - $($_.Exception.Message)" -ForegroundColor Red
    $issues += "Cannot connect to Tavily API"
}

# Check 5: VS Code Process and MCP Status
Write-Host "`n5. Checking VS Code Process..." -ForegroundColor Yellow
$vscodeProcesses = Get-Process -Name "Code" -ErrorAction SilentlyContinue
if ($vscodeProcesses) {
    Write-Host "   ✅ VS Code is running ($($vscodeProcesses.Count) instances)" -ForegroundColor Green
} else {
    Write-Host "   ⚠️  VS Code is not running" -ForegroundColor Yellow
}

# Summary
Write-Host "`n📊 DIAGNOSTIC SUMMARY" -ForegroundColor Cyan
Write-Host "===================" -ForegroundColor Cyan

if ($issues.Count -eq 0) {
    Write-Host "✅ All core components appear to be configured correctly!" -ForegroundColor Green
    Write-Host "`n💡 If MCP still isn't working, try:" -ForegroundColor Yellow
    Write-Host "   1. Restart VS Code completely" -ForegroundColor White
    Write-Host "   2. Open Copilot Chat (Ctrl+Shift+I)" -ForegroundColor White
    Write-Host "   3. Look for MCP selector when typing" -ForegroundColor White
    Write-Host "   4. Try typing '@' to see available agents" -ForegroundColor White
} else {
    Write-Host "❌ Issues found:" -ForegroundColor Red
    foreach ($issue in $issues) {
        Write-Host "   - $issue" -ForegroundColor Red
    }
}

if ($warnings.Count -gt 0) {
    Write-Host "`n⚠️  Warnings:" -ForegroundColor Yellow
    foreach ($warning in $warnings) {
        Write-Host "   - $warning" -ForegroundColor Yellow
    }
}

# Fix Option
if ($Fix) {
    Write-Host "`n🔧 APPLYING FIXES..." -ForegroundColor Green

    # Fix 1: Install missing extensions
    if ($issues -contains "MCP extension not installed") {
        Write-Host "Installing MCP extension..." -ForegroundColor Yellow
        & code --install-extension automatalabs.copilot-mcp
    }

    if ($issues -contains "GitHub Copilot extension not installed") {
        Write-Host "Installing GitHub Copilot extension..." -ForegroundColor Yellow
        & code --install-extension github.copilot
    }

    if ($issues -contains "GitHub Copilot Chat extension not installed") {
        Write-Host "Installing GitHub Copilot Chat extension..." -ForegroundColor Yellow
        & code --install-extension github.copilot-chat
    }

    # Fix 2: Create missing MCP configuration
    if ($issues -contains "Global MCP configuration missing") {
        Write-Host "Creating global MCP configuration..." -ForegroundColor Yellow
        $mcpDir = "$env:USERPROFILE\.github\copilot"
        if (!(Test-Path $mcpDir)) {
            New-Item -ItemType Directory -Path $mcpDir -Force
        }

        $mcpConfig = @{
            mcpServers = @{
                "Tavily Expert" = @{
                    serverUrl = "https://tavily.api.tadata.com/mcp/tavily/endoderm-victory-fling-azuqd2"
                }
            }
        }

        $mcpConfig | ConvertTo-Json -Depth 3 | Set-Content "$mcpDir\mcp.json" -Encoding UTF8
        Write-Host "   ✅ Created global MCP configuration" -ForegroundColor Green
    }

    # Fix 3: Add MCP settings to VS Code if missing
    if ($warnings -contains "No MCP settings in VS Code workspace") {
        Write-Host "Adding MCP settings to VS Code workspace..." -ForegroundColor Yellow

        $vscodeDir = ".\.vscode"
        if (!(Test-Path $vscodeDir)) {
            New-Item -ItemType Directory -Path $vscodeDir -Force
        }

        $settingsPath = "$vscodeDir\settings.json"
        if (Test-Path $settingsPath) {
            $settings = Get-Content $settingsPath -Raw | ConvertFrom-Json
        } else {
            $settings = @{}
        }

        # Add MCP settings
        $settings."github.copilot.mcp.enabled" = $true
        $settings."github.copilot.mcp.configPath" = $globalMcpPath

        $settings | ConvertTo-Json -Depth 10 | Set-Content $settingsPath -Encoding UTF8
        Write-Host "   ✅ Added MCP settings to VS Code workspace" -ForegroundColor Green
    }

    Write-Host "`n✅ Fixes applied! Please restart VS Code to apply changes." -ForegroundColor Green
}

# Reinstall Option
if ($Reinstall) {
    Write-Host "`n🔄 REINSTALLING MCP COMPONENTS..." -ForegroundColor Magenta

    Write-Host "Uninstalling MCP extension..." -ForegroundColor Yellow
    & code --uninstall-extension automatalabs.copilot-mcp

    Start-Sleep 2

    Write-Host "Reinstalling MCP extension..." -ForegroundColor Yellow
    & code --install-extension automatalabs.copilot-mcp

    Write-Host "✅ MCP extension reinstalled. Please restart VS Code." -ForegroundColor Green
}

Write-Host "`n🚌 BusBuddy MCP Diagnostic Complete!" -ForegroundColor Cyan
