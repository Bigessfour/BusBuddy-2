#Requires -Version 7.5
<#
.SYNOPSIS
    Complete Node.js and MCP Setup for BusBuddy
.DESCRIPTION
    Downloads and installs Node.js, then sets up MCP servers for VS Code integration
.EXAMPLE
    .\setup-nodejs-and-mcp.ps1
#>

param(
    [switch]$SkipNodeJS,
    [switch]$Force,
    [string]$NodeVersion = "20.11.0"
)

# Enhanced error handling
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

Write-Host "🚀 BusBuddy Node.js and MCP Setup" -ForegroundColor Cyan
Write-Host "===================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as administrator
function Test-Administrator {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

# Download Node.js installer
function Install-NodeJS {
    param([string]$Version)

    Write-Host "📦 Installing Node.js $Version..." -ForegroundColor Yellow

    # Check if Node.js is already installed
    try {
        $existingVersion = node --version 2>$null
        if ($existingVersion -and -not $Force) {
            Write-Host "✅ Node.js already installed: $existingVersion" -ForegroundColor Green
            return $true
        }
    }
    catch {
        Write-Host "📥 Node.js not found, proceeding with installation..." -ForegroundColor Yellow
    }

    # Download Node.js installer
    $nodeUrl = "https://nodejs.org/dist/v$Version/node-v$Version-x64.msi"
    $installerPath = "$env:TEMP\nodejs-installer.msi"

    try {
        Write-Host "⬇️ Downloading Node.js from: $nodeUrl" -ForegroundColor Blue
        Invoke-WebRequest -Uri $nodeUrl -OutFile $installerPath -UseBasicParsing

        # Install Node.js
        Write-Host "🔧 Installing Node.js (this may take a few minutes)..." -ForegroundColor Yellow
        $process = Start-Process -FilePath "msiexec.exe" -ArgumentList "/i", $installerPath, "/quiet", "/norestart" -Wait -PassThru

        if ($process.ExitCode -eq 0) {
            Write-Host "✅ Node.js installed successfully!" -ForegroundColor Green

            # Refresh environment variables
            $env:Path = [System.Environment]::GetEnvironmentVariable("Path", "Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path", "User")

            return $true
        }
        else {
            throw "Node.js installation failed with exit code: $($process.ExitCode)"
        }
    }
    catch {
        Write-Host "❌ Failed to install Node.js: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
    finally {
        # Clean up installer
        if (Test-Path $installerPath) {
            Remove-Item $installerPath -Force -ErrorAction SilentlyContinue
        }
    }
}

# Verify Node.js and npm installation
function Test-NodeInstallation {
    Write-Host "🔍 Verifying Node.js and npm installation..." -ForegroundColor Yellow

    try {
        $nodeVersion = node --version 2>$null
        $npmVersion = npm --version 2>$null

        if ($nodeVersion -and $npmVersion) {
            Write-Host "✅ Node.js: $nodeVersion" -ForegroundColor Green
            Write-Host "✅ npm: $npmVersion" -ForegroundColor Green
            return $true
        }
        else {
            Write-Host "❌ Node.js or npm not found in PATH" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "❌ Error checking Node.js/npm: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Install MCP servers
function Install-MCPServers {
    Write-Host "🤖 Installing MCP servers..." -ForegroundColor Yellow

    $mcpServers = @(
        @{
            Name = "@modelcontextprotocol/server-filesystem"
            Description = "File system access server"
        },
        @{
            Name = "@modelcontextprotocol/server-git"
            Description = "Git repository server"
        },
        @{
            Name = "@modelcontextprotocol/server-brave-search"
            Description = "Web search server"
        }
    )

    $installedServers = @()

    foreach ($server in $mcpServers) {
        try {
            Write-Host "📦 Installing $($server.Name)..." -ForegroundColor Blue
            $result = npm install -g $server.Name 2>&1

            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ $($server.Name) installed successfully" -ForegroundColor Green
                $installedServers += $server
            }
            else {
                Write-Host "⚠️ Failed to install $($server.Name): $result" -ForegroundColor Yellow
            }
        }
        catch {
            Write-Host "❌ Error installing $($server.Name): $($_.Exception.Message)" -ForegroundColor Red
        }
    }

    return $installedServers
}

# Create MCP configuration
function New-MCPConfiguration {
    param([array]$InstalledServers)

    Write-Host "⚙️ Creating MCP configuration..." -ForegroundColor Yellow

    # Find npm global directory for server paths
    try {
        $npmGlobalPath = npm root -g 2>$null
        if (-not $npmGlobalPath) {
            $npmGlobalPath = "$env:APPDATA\npm\node_modules"
        }
    }
    catch {
        $npmGlobalPath = "$env:APPDATA\npm\node_modules"
    }

    # Create MCP configuration
    $mcpConfig = @{
        mcpServers = @{}
    }

    # Add filesystem server
    if ($InstalledServers | Where-Object { $_.Name -eq "@modelcontextprotocol/server-filesystem" }) {
        $mcpConfig.mcpServers["filesystem"] = @{
            command = "node"
            args = @("$npmGlobalPath\@modelcontextprotocol\server-filesystem\dist\index.js")
            env = @{
                ALLOWED_DIRECTORIES = @(
                    (Get-Location).Path,
                    "$env:USERPROFILE\Documents",
                    "$env:USERPROFILE\Desktop"
                ) -join ";"
            }
        }
    }

    # Add git server
    if ($InstalledServers | Where-Object { $_.Name -eq "@modelcontextprotocol/server-git" }) {
        $mcpConfig.mcpServers["git"] = @{
            command = "node"
            args = @("$npmGlobalPath\@modelcontextprotocol\server-git\dist\index.js")
            env = @{
                ALLOWED_REPOSITORIES = (Get-Location).Path
            }
        }
    }

    # Add search server (if API key is available)
    if ($InstalledServers | Where-Object { $_.Name -eq "@modelcontextprotocol/server-brave-search" }) {
        $braveApiKey = $env:BRAVE_API_KEY
        if ($braveApiKey) {
            $mcpConfig.mcpServers["brave-search"] = @{
                command = "node"
                args = @("$npmGlobalPath\@modelcontextprotocol\server-brave-search\dist\index.js")
                env = @{
                    BRAVE_API_KEY = $braveApiKey
                }
            }
        }
        else {
            Write-Host "⚠️ Brave Search server installed but BRAVE_API_KEY not set" -ForegroundColor Yellow
        }
    }

    # Save configuration
    $mcpConfigPath = "$env:APPDATA\Code\User\globalStorage\github.copilot-labs_copilot-mcp\mcp.json"
    $mcpConfigDir = Split-Path $mcpConfigPath -Parent

    if (-not (Test-Path $mcpConfigDir)) {
        New-Item -Path $mcpConfigDir -ItemType Directory -Force | Out-Null
    }

    $mcpConfig | ConvertTo-Json -Depth 10 | Set-Content $mcpConfigPath -Encoding UTF8

    Write-Host "✅ MCP configuration saved to: $mcpConfigPath" -ForegroundColor Green

    return $mcpConfigPath
}

# Update VS Code settings
function Update-VSCodeSettings {
    Write-Host "🔧 Updating VS Code settings for MCP..." -ForegroundColor Yellow

    $settingsPath = "$env:APPDATA\Code\User\settings.json"

    try {
        # Read existing settings
        if (Test-Path $settingsPath) {
            $settings = Get-Content $settingsPath -Raw | ConvertFrom-Json -AsHashtable
        }
        else {
            $settings = @{}
        }

        # Add MCP settings
        $settings["github.copilot.mcp.enabled"] = $true
        $settings["github.copilot.mcp.configPath"] = "$env:APPDATA\Code\User\globalStorage\github.copilot-labs_copilot-mcp\mcp.json"

        # Save updated settings
        $settings | ConvertTo-Json -Depth 10 | Set-Content $settingsPath -Encoding UTF8

        Write-Host "✅ VS Code settings updated successfully" -ForegroundColor Green
    }
    catch {
        Write-Host "⚠️ Failed to update VS Code settings: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

# Main execution
try {
    Write-Host "🔍 System Check:" -ForegroundColor Blue
    Write-Host "   - PowerShell Version: $($PSVersionTable.PSVersion)" -ForegroundColor Gray
    Write-Host "   - Operating System: $(Get-CimInstance -Class Win32_OperatingSystem | Select-Object -ExpandProperty Caption)" -ForegroundColor Gray
    Write-Host "   - Administrator: $(if (Test-Administrator) { 'Yes' } else { 'No' })" -ForegroundColor Gray
    Write-Host ""

    # Step 1: Install Node.js
    if (-not $SkipNodeJS) {
        if (-not (Install-NodeJS -Version $NodeVersion)) {
            throw "Node.js installation failed"
        }

        # Verify installation
        if (-not (Test-NodeInstallation)) {
            throw "Node.js installation verification failed"
        }
    }
    else {
        Write-Host "⏭️ Skipping Node.js installation" -ForegroundColor Yellow
        if (-not (Test-NodeInstallation)) {
            throw "Node.js not found and installation was skipped"
        }
    }

    Write-Host ""

    # Step 2: Install MCP servers
    $installedServers = Install-MCPServers

    if ($installedServers.Count -eq 0) {
        Write-Host "⚠️ No MCP servers were installed successfully" -ForegroundColor Yellow
    }
    else {
        Write-Host ""
        Write-Host "✅ Successfully installed MCP servers:" -ForegroundColor Green
        foreach ($server in $installedServers) {
            Write-Host "   - $($server.Name): $($server.Description)" -ForegroundColor Gray
        }
    }

    Write-Host ""

    # Step 3: Create MCP configuration
    $configPath = New-MCPConfiguration -InstalledServers $installedServers

    # Step 4: Update VS Code settings
    Update-VSCodeSettings

    Write-Host ""
    Write-Host "🎉 Setup Complete!" -ForegroundColor Green
    Write-Host "=================" -ForegroundColor Green
    Write-Host "✅ Node.js and npm installed" -ForegroundColor Green
    Write-Host "✅ MCP servers installed: $($installedServers.Count)" -ForegroundColor Green
    Write-Host "✅ MCP configuration created" -ForegroundColor Green
    Write-Host "✅ VS Code settings updated" -ForegroundColor Green
    Write-Host ""
    Write-Host "📋 Next Steps:" -ForegroundColor Yellow
    Write-Host "1. Restart VS Code to load new settings" -ForegroundColor Gray
    Write-Host "2. Open GitHub Copilot chat and test MCP functionality" -ForegroundColor Gray
    Write-Host "3. Use @filesystem, @git, or @brave-search in Copilot chat" -ForegroundColor Gray
    Write-Host ""
    Write-Host "💡 Tip: You can verify MCP is working by typing '@filesystem list files' in Copilot chat" -ForegroundColor Cyan

}
catch {
    Write-Host ""
    Write-Host "❌ Setup failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Please check the error above and try again" -ForegroundColor Red
    exit 1
}
