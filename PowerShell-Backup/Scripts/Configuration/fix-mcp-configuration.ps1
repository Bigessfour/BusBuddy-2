#Requires -Version 7.5
<#
.SYNOPSIS
    Fix MCP (Model Context Protocol) Configuration for BusBuddy

.DESCRIPTION
    Comprehensive fix for MCP issues including:
    - VS Code settings configuration
    - Tavily Expert MCP server setup
    - Alternative MCP server configuration
    - Connection testing and validation

.EXAMPLE
    .\fix-mcp-configuration.ps1
    .\fix-mcp-configuration.ps1 -TestOnly
#>

param(
    [switch]$TestOnly,
    [switch]$UseAlternativeServer,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

Write-Host "🔧 BusBuddy MCP Configuration Fix" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Locate VS Code settings
Write-Host "📁 Locating VS Code settings..." -ForegroundColor Yellow

$possibleSettingsPaths = @(
    "$env:APPDATA\Code\User\settings.json",
    "$env:APPDATA\Code - Insiders\User\settings.json",
    "$env:LOCALAPPDATA\Programs\Microsoft VS Code\resources\app\out\vs\workbench\contrib\preferences\browser\settingsLayout.json"
)

$vscodeSettingsPath = $null
foreach ($path in $possibleSettingsPaths) {
    if (Test-Path $path) {
        $vscodeSettingsPath = $path
        Write-Host "✅ Found VS Code settings: $path" -ForegroundColor Green
        break
    }
}

if (-not $vscodeSettingsPath) {
    $vscodeSettingsPath = "$env:APPDATA\Code\User\settings.json"
    Write-Host "⚠️ No existing settings found, will create: $vscodeSettingsPath" -ForegroundColor Yellow
}

# Step 2: Read current settings
Write-Host ""
Write-Host "📖 Reading current VS Code settings..." -ForegroundColor Yellow

$currentSettings = @{}
if (Test-Path $vscodeSettingsPath) {
    try {
        $settingsContent = Get-Content $vscodeSettingsPath -Raw
        if ($settingsContent.Trim()) {
            $currentSettings = $settingsContent | ConvertFrom-Json -AsHashtable
        }
        Write-Host "✅ Current settings loaded" -ForegroundColor Green
    }
    catch {
        Write-Host "⚠️ Error reading settings, starting fresh: $_" -ForegroundColor Yellow
        $currentSettings = @{}
    }
}

# Step 3: Check MCP configuration files
Write-Host ""
Write-Host "🔍 Checking MCP configuration files..." -ForegroundColor Yellow

$globalMcpPath = "$env:APPDATA\Claude\mcp.json"
$localMcpPath = ".\mcp.json"

Write-Host "Global MCP: $(if (Test-Path $globalMcpPath) { '✅ Found' } else { '❌ Missing' })" -ForegroundColor $(if (Test-Path $globalMcpPath) { 'Green' } else { 'Red' })
Write-Host "Local MCP: $(if (Test-Path $localMcpPath) { '✅ Found' } else { '❌ Missing' })" -ForegroundColor $(if (Test-Path $localMcpPath) { 'Green' } else { 'Red' })

# Step 4: Create/Fix MCP configuration
Write-Host ""
Write-Host "🛠️ Creating/Fixing MCP configuration..." -ForegroundColor Yellow

if ($UseAlternativeServer) {
    # Create a working MCP configuration with a reliable server
    $mcpConfig = @{
        "mcpServers" = @{
            "filesystem" = @{
                "command" = "npx"
                "args" = @("-y", "@modelcontextprotocol/server-filesystem", "C:\Users\biges\Desktop\BusBuddy\BusBuddy")
            }
            "brave-search" = @{
                "command" = "npx"
                "args" = @("-y", "@modelcontextprotocol/server-brave-search")
                "env" = @{
                    "BRAVE_API_KEY" = ""
                }
            }
        }
    }
    Write-Host "✅ Created alternative MCP configuration (filesystem + brave-search)" -ForegroundColor Green
}
else {
    # Try to fix the existing Tavily configuration
    $mcpConfig = @{
        "mcpServers" = @{
            "tavily-expert" = @{
                "command" = "npx"
                "args" = @("-y", "@tavily/mcp-server")
                "env" = @{
                    "TAVILY_API_KEY" = ""
                }
            }
        }
    }
    Write-Host "✅ Created Tavily Expert MCP configuration" -ForegroundColor Green
}

# Save to local MCP file
$mcpConfig | ConvertTo-Json -Depth 10 | Set-Content $localMcpPath -Encoding UTF8
Write-Host "💾 Saved MCP configuration to: $localMcpPath" -ForegroundColor Green

# Save to global MCP file if directory exists
$globalMcpDir = Split-Path $globalMcpPath -Parent
if (-not (Test-Path $globalMcpDir)) {
    try {
        New-Item -ItemType Directory -Path $globalMcpDir -Force | Out-Null
        Write-Host "📁 Created directory: $globalMcpDir" -ForegroundColor Green
    }
    catch {
        Write-Host "⚠️ Could not create global MCP directory: $_" -ForegroundColor Yellow
    }
}

if (Test-Path $globalMcpDir) {
    $mcpConfig | ConvertTo-Json -Depth 10 | Set-Content $globalMcpPath -Encoding UTF8
    Write-Host "💾 Saved MCP configuration to: $globalMcpPath" -ForegroundColor Green
}

# Step 5: Update VS Code settings
Write-Host ""
Write-Host "⚙️ Updating VS Code settings..." -ForegroundColor Yellow

# Add MCP settings to current settings
$currentSettings["github.copilot.mcp.enabled"] = $true
$currentSettings["github.copilot.mcp.configPath"] = $localMcpPath

# Ensure directory exists
$settingsDir = Split-Path $vscodeSettingsPath -Parent
if (-not (Test-Path $settingsDir)) {
    New-Item -ItemType Directory -Path $settingsDir -Force | Out-Null
    Write-Host "📁 Created VS Code settings directory: $settingsDir" -ForegroundColor Green
}

# Save updated settings
try {
    $currentSettings | ConvertTo-Json -Depth 10 | Set-Content $vscodeSettingsPath -Encoding UTF8
    Write-Host "✅ Updated VS Code settings: $vscodeSettingsPath" -ForegroundColor Green
}
catch {
    Write-Host "❌ Error updating VS Code settings: $_" -ForegroundColor Red
}

# Step 6: Test configuration (if not TestOnly)
if (-not $TestOnly) {
    Write-Host ""
    Write-Host "🧪 Testing MCP configuration..." -ForegroundColor Yellow

    # Test local MCP file
    if (Test-Path $localMcpPath) {
        try {
            $testConfig = Get-Content $localMcpPath | ConvertFrom-Json
            Write-Host "✅ Local MCP config is valid JSON" -ForegroundColor Green
        }
        catch {
            Write-Host "❌ Local MCP config is invalid: $_" -ForegroundColor Red
        }
    }

    # Test VS Code settings
    if (Test-Path $vscodeSettingsPath) {
        try {
            $testSettings = Get-Content $vscodeSettingsPath | ConvertFrom-Json
            if ($testSettings."github.copilot.mcp.enabled") {
                Write-Host "✅ VS Code MCP enabled setting confirmed" -ForegroundColor Green
            }
            else {
                Write-Host "⚠️ VS Code MCP enabled setting not found" -ForegroundColor Yellow
            }
        }
        catch {
            Write-Host "❌ VS Code settings are invalid: $_" -ForegroundColor Red
        }
    }
}

# Step 7: Instructions
Write-Host ""
Write-Host "📋 Next Steps:" -ForegroundColor Cyan
Write-Host "1. Restart VS Code for settings to take effect" -ForegroundColor White
Write-Host "2. Install MCP servers: npm install -g @modelcontextprotocol/server-filesystem" -ForegroundColor White
if ($UseAlternativeServer) {
    Write-Host "3. Alternative servers configured (filesystem + brave-search)" -ForegroundColor White
}
else {
    Write-Host "3. Get Tavily API key from: https://tavily.com/" -ForegroundColor White
    Write-Host "4. Add API key to mcp.json files" -ForegroundColor White
}
Write-Host "5. Open VS Code and check if Copilot MCP is working" -ForegroundColor White

Write-Host ""
Write-Host "🎉 MCP Configuration Fix Complete!" -ForegroundColor Green
Write-Host ""

# Output file locations for reference
Write-Host "📁 File Locations:" -ForegroundColor Cyan
Write-Host "VS Code Settings: $vscodeSettingsPath" -ForegroundColor Gray
Write-Host "Local MCP Config: $localMcpPath" -ForegroundColor Gray
if (Test-Path $globalMcpPath) {
    Write-Host "Global MCP Config: $globalMcpPath" -ForegroundColor Gray
}
