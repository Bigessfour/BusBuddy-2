#Requires -Version 7.5

<#
.SYNOPSIS
    Configure Tavily Expert MCP for BusBuddy (Using Your Existing Setup)
.DESCRIPTION
    Updates VS Code settings to use your existing Tavily Expert MCP configuration
.NOTES
    Author: BusBuddy Development Team
    Date: July 28, 2025
    Uses your existing Tavily configuration from yesterday
#>

param(
    [switch]$Force
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

Write-Host "🚌 BusBuddy Tavily MCP Configuration" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan

# Step 1: Check existing Tavily configuration
Write-Host "`n📋 Checking existing Tavily MCP configuration..." -ForegroundColor Yellow

$tavilyConfigPath = ".\tavily-mcp-config.json"
$mcpConfigPath = ".\mcp.json"

if (Test-Path $tavilyConfigPath) {
    try {
        $tavilyConfig = Get-Content $tavilyConfigPath -Raw | ConvertFrom-Json
        Write-Host "✅ Found Tavily MCP config: $tavilyConfigPath" -ForegroundColor Green
        Write-Host "   Server URL: $($tavilyConfig.mcpServers.'Tavily Expert'.serverUrl)" -ForegroundColor Gray
    } catch {
        Write-Warning "⚠️ Could not read Tavily config: $($_.Exception.Message)"
    }
} else {
    Write-Warning "⚠️ Tavily MCP config not found at: $tavilyConfigPath"
}

if (Test-Path $mcpConfigPath) {
    try {
        $mcpConfig = Get-Content $mcpConfigPath -Raw | ConvertFrom-Json
        Write-Host "✅ Found general MCP config: $mcpConfigPath" -ForegroundColor Green
        $serverNames = $mcpConfig.mcpServers.PSObject.Properties.Name
        Write-Host "   Configured servers: $($serverNames -join ', ')" -ForegroundColor Gray
    } catch {
        Write-Warning "⚠️ Could not read MCP config: $($_.Exception.Message)"
    }
} else {
    Write-Warning "⚠️ General MCP config not found at: $mcpConfigPath"
}

# Step 2: Check environment variables
Write-Host "`n🔑 Checking API keys..." -ForegroundColor Yellow

$tavilyApiKey = $env:TAVILY_API_KEY
if ($tavilyApiKey -and $tavilyApiKey -ne "tvly-EXAMPLE-KEY") {
    Write-Host "✅ TAVILY_API_KEY found in environment" -ForegroundColor Green
} else {
    Write-Warning "⚠️ TAVILY_API_KEY not found or is example key"
    Write-Host "   Current value: $tavilyApiKey" -ForegroundColor Gray
}

# Step 3: Update VS Code settings to use Tavily MCP
Write-Host "`n🔧 Updating VS Code settings for Tavily MCP..." -ForegroundColor Yellow

$vscodeSettingsPath = "$env:APPDATA\Code\User\settings.json"
$settings = @{}

# Read existing settings
if (Test-Path $vscodeSettingsPath) {
    try {
        $settingsContent = Get-Content $vscodeSettingsPath -Raw
        if ($settingsContent.Trim()) {
            $settings = $settingsContent | ConvertFrom-Json
        }
        Write-Host "✅ Loaded existing VS Code settings" -ForegroundColor Green
    } catch {
        Write-Warning "⚠️ Could not parse existing VS Code settings, creating new ones"
        $settings = @{}
    }
} else {
    Write-Host "ℹ️ Creating new VS Code settings file" -ForegroundColor Yellow
}

# Add/update MCP settings for Tavily
$settings | Add-Member -NotePropertyName "github.copilot.mcp.enabled" -NotePropertyValue $true -Force

# Use the Tavily configuration
$tavilyConfigFullPath = (Get-Item $tavilyConfigPath).FullName
$settings | Add-Member -NotePropertyName "github.copilot.mcp.configPath" -NotePropertyValue $tavilyConfigFullPath -Force

# Save updated settings
try {
    $settingsDir = Split-Path $vscodeSettingsPath -Parent
    if (-not (Test-Path $settingsDir)) {
        New-Item -Path $settingsDir -ItemType Directory -Force | Out-Null
    }

    $settings | ConvertTo-Json -Depth 10 | Set-Content -Path $vscodeSettingsPath -Encoding UTF8
    Write-Host "✅ VS Code settings updated: $vscodeSettingsPath" -ForegroundColor Green
    Write-Host "   MCP enabled: $($settings.'github.copilot.mcp.enabled')" -ForegroundColor Gray
    Write-Host "   MCP config path: $($settings.'github.copilot.mcp.configPath')" -ForegroundColor Gray
} catch {
    Write-Error "❌ Failed to update VS Code settings: $($_.Exception.Message)"
}

# Step 4: Test Tavily MCP server connectivity
Write-Host "`n🧪 Testing Tavily MCP server connectivity..." -ForegroundColor Yellow

if (Test-Path $tavilyConfigPath) {
    try {
        $tavilyConfig = Get-Content $tavilyConfigPath -Raw | ConvertFrom-Json
        $serverUrl = $tavilyConfig.mcpServers.'Tavily Expert'.serverUrl

        Write-Host "Testing connection to: $serverUrl" -ForegroundColor Cyan

        # Test HTTP connectivity
        try {
            $response = Invoke-WebRequest -Uri $serverUrl -Method HEAD -TimeoutSec 10 -ErrorAction Stop
            Write-Host "✅ Tavily MCP server is accessible (Status: $($response.StatusCode))" -ForegroundColor Green
        } catch {
            if ($_.Exception.Response.StatusCode -eq 404) {
                Write-Warning "⚠️ Server returned 404 - this might be normal for MCP endpoints"
                Write-Host "   MCP servers often don't respond to HEAD requests" -ForegroundColor Gray
            } else {
                Write-Warning "⚠️ Could not connect to Tavily server: $($_.Exception.Message)"
            }
        }
    } catch {
        Write-Warning "⚠️ Could not test Tavily server: $($_.Exception.Message)"
    }
}

# Step 5: Create workspace-specific MCP configuration combining both
Write-Host "`n📁 Creating unified workspace MCP configuration..." -ForegroundColor Yellow

$unifiedMcpPath = ".\.vscode\mcp-unified.json"
$vscodeDir = ".\.vscode"

if (-not (Test-Path $vscodeDir)) {
    New-Item -Path $vscodeDir -ItemType Directory -Force | Out-Null
}

# Combine Tavily and existing MCP configurations
$unifiedConfig = @{
    mcpServers = @{}
}

# Add Tavily Expert server
if (Test-Path $tavilyConfigPath) {
    $tavilyConfig = Get-Content $tavilyConfigPath -Raw | ConvertFrom-Json
    $unifiedConfig.mcpServers.'Tavily Expert' = $tavilyConfig.mcpServers.'Tavily Expert'
}

# Add existing local servers
if (Test-Path $mcpConfigPath) {
    $mcpConfig = Get-Content $mcpConfigPath -Raw | ConvertFrom-Json
    foreach ($serverName in $mcpConfig.mcpServers.PSObject.Properties.Name) {
        $unifiedConfig.mcpServers.$serverName = $mcpConfig.mcpServers.$serverName
    }
}

# Add BusBuddy-specific local MCP servers if they exist
$localServersPath = ".\mcp-servers"
if (Test-Path $localServersPath) {
    $unifiedConfig.mcpServers."busbuddy-filesystem" = @{
        command = "node"
        args = @("$((Get-Location).Path)\mcp-servers\filesystem-mcp-server.js")
        env = @{
            ALLOWED_DIRECTORIES = $PWD.Path
        }
    }

    $unifiedConfig.mcpServers."busbuddy-git" = @{
        command = "node"
        args = @("$((Get-Location).Path)\mcp-servers\git-mcp-server.js")
        env = @{
            ALLOWED_REPOSITORIES = $PWD.Path
        }
    }
}

try {
    $unifiedConfig | ConvertTo-Json -Depth 10 | Set-Content -Path $unifiedMcpPath -Encoding UTF8
    Write-Host "✅ Unified MCP configuration created: $unifiedMcpPath" -ForegroundColor Green

    # Update VS Code settings to use unified config
    $settings.'github.copilot.mcp.configPath' = (Get-Item $unifiedMcpPath).FullName
    $settings | ConvertTo-Json -Depth 10 | Set-Content -Path $vscodeSettingsPath -Encoding UTF8
    Write-Host "✅ VS Code settings updated to use unified config" -ForegroundColor Green

} catch {
    Write-Warning "⚠️ Could not create unified MCP config: $($_.Exception.Message)"
}

# Final summary
Write-Host "`n📊 Tavily MCP Configuration Summary:" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan

$summary = @()

if (Test-Path $tavilyConfigPath) {
    $summary += "✅ Tavily Expert MCP config: $tavilyConfigPath"
} else {
    $summary += "❌ Tavily Expert MCP config missing"
}

if ($tavilyApiKey -and $tavilyApiKey -ne "tvly-EXAMPLE-KEY") {
    $summary += "✅ TAVILY_API_KEY environment variable set"
} else {
    $summary += "⚠️ TAVILY_API_KEY needs to be set to real API key"
}

if (Test-Path $vscodeSettingsPath) {
    $summary += "✅ VS Code settings updated: $vscodeSettingsPath"
} else {
    $summary += "❌ VS Code settings not updated"
}

if (Test-Path $unifiedMcpPath) {
    $summary += "✅ Unified MCP config: $unifiedMcpPath"
} else {
    $summary += "⚠️ Unified MCP config not created"
}

foreach ($item in $summary) {
    Write-Host $item
}

Write-Host "`n🎉 Tavily MCP Configuration Complete!" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "1. Restart VS Code to load new MCP configuration" -ForegroundColor White
Write-Host "2. Open GitHub Copilot and verify Tavily Expert MCP server is available" -ForegroundColor White
Write-Host "3. Test Tavily search functionality through MCP" -ForegroundColor White

if ($tavilyApiKey -eq "tvly-EXAMPLE-KEY") {
    Write-Host "`n⚠️ Important: Update TAVILY_API_KEY environment variable" -ForegroundColor Yellow
    Write-Host "The current key appears to be an example. Get a real API key from Tavily." -ForegroundColor White
}

Write-Host "`n🚌 BusBuddy with Tavily Expert MCP is ready!" -ForegroundColor Cyan
