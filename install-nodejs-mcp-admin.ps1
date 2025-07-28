#Requires -Version 7.5
#Requires -RunAsAdministrator

<#
.SYNOPSIS
    Administrative Node.js and MCP Server Installation for BusBuddy
.DESCRIPTION
    Installs Node.js and configures MCP servers with administrative privileges
.NOTES
    Author: BusBuddy Development Team
    Date: July 28, 2025
    Requires: PowerShell 7.5+, Administrator privileges
#>

param(
    [switch]$Force,
    [switch]$SkipNodeCheck,
    [string]$NodeVersion = "20.17.0"  # Latest LTS
)

# Set strict error handling
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

Write-Host "🚌 BusBuddy MCP Setup (Administrator Mode)" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

# Function to check if running as administrator
function Test-IsAdministrator {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

# Verify administrator privileges
if (-not (Test-IsAdministrator)) {
    Write-Error "❌ This script must be run as Administrator!"
    exit 1
}

Write-Host "✅ Administrator privileges confirmed" -ForegroundColor Green

# Step 1: Check if Node.js is already installed
Write-Host "`n📦 Checking Node.js installation..." -ForegroundColor Yellow

$nodeInstalled = $false
try {
    $nodeVersion = node --version 2>$null
    if ($nodeVersion) {
        Write-Host "✅ Node.js already installed: $nodeVersion" -ForegroundColor Green
        $nodeInstalled = $true

        $npmVersion = npm --version 2>$null
        Write-Host "✅ npm version: $npmVersion" -ForegroundColor Green
    }
} catch {
    Write-Host "ℹ️ Node.js not found, proceeding with installation..." -ForegroundColor Yellow
}

# Step 2: Install Node.js if needed
if (-not $nodeInstalled -or $Force) {
    Write-Host "`n🔽 Downloading Node.js $NodeVersion..." -ForegroundColor Yellow

    $nodeUrl = "https://nodejs.org/dist/v$NodeVersion/node-v$NodeVersion-x64.msi"
    $installerPath = "$env:TEMP\nodejs-$NodeVersion.msi"

    try {
        # Download Node.js installer
        Invoke-WebRequest -Uri $nodeUrl -OutFile $installerPath -UseBasicParsing
        Write-Host "✅ Downloaded Node.js installer" -ForegroundColor Green

        # Install Node.js silently
        Write-Host "🔧 Installing Node.js (this may take a few minutes)..." -ForegroundColor Yellow
        $installProcess = Start-Process -FilePath "msiexec.exe" -ArgumentList "/i", $installerPath, "/quiet", "/norestart" -Wait -PassThru

        if ($installProcess.ExitCode -eq 0) {
            Write-Host "✅ Node.js installed successfully!" -ForegroundColor Green

            # Refresh environment variables
            $env:Path = [System.Environment]::GetEnvironmentVariable("Path", "Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path", "User")

            # Verify installation
            Start-Sleep -Seconds 3
            $nodeVersion = node --version 2>$null
            $npmVersion = npm --version 2>$null

            if ($nodeVersion -and $npmVersion) {
                Write-Host "✅ Installation verified - Node.js: $nodeVersion, npm: $npmVersion" -ForegroundColor Green
            } else {
                Write-Warning "⚠️ Installation may require a system restart to update PATH"
                Write-Host "Please restart your PowerShell session or computer and run this script again." -ForegroundColor Yellow
                exit 0
            }
        } else {
            Write-Error "❌ Node.js installation failed with exit code: $($installProcess.ExitCode)"
            exit 1
        }

        # Clean up installer
        Remove-Item -Path $installerPath -Force -ErrorAction SilentlyContinue

    } catch {
        Write-Error "❌ Failed to download or install Node.js: $($_.Exception.Message)"
        exit 1
    }
}

# Step 3: Install MCP servers
Write-Host "`n📡 Installing MCP servers..." -ForegroundColor Yellow

$mcpServers = @(
    "@modelcontextprotocol/server-filesystem",
    "@modelcontextprotocol/server-git",
    "@modelcontextprotocol/server-github",
    "@modelcontextprotocol/server-sqlite"
)

foreach ($server in $mcpServers) {
    try {
        Write-Host "Installing $server..." -ForegroundColor Cyan
        & npm install -g $server --loglevel=error

        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ $server installed successfully" -ForegroundColor Green
        } else {
            Write-Warning "⚠️ Failed to install $server (exit code: $LASTEXITCODE)"
        }
    } catch {
        Write-Warning "⚠️ Error installing ${server}: $($_.Exception.Message)"
    }
}

# Step 4: Create MCP configuration
Write-Host "`n⚙️ Creating MCP configuration..." -ForegroundColor Yellow

$mcpConfigDir = "$env:APPDATA\Claude"
$mcpConfigPath = "$mcpConfigDir\claude_desktop_config.json"

# Ensure config directory exists
if (-not (Test-Path $mcpConfigDir)) {
    New-Item -Path $mcpConfigDir -ItemType Directory -Force | Out-Null
    Write-Host "✅ Created MCP config directory: $mcpConfigDir" -ForegroundColor Green
}

# Find npm global directory
$npmGlobalDir = npm root -g 2>$null
if ($npmGlobalDir) {
    Write-Host "✅ Found npm global directory: $npmGlobalDir" -ForegroundColor Green
} else {
    $npmGlobalDir = "$env:APPDATA\npm\node_modules"
    Write-Host "⚠️ Using fallback npm directory: $npmGlobalDir" -ForegroundColor Yellow
}

# Create comprehensive MCP configuration
$mcpConfig = @{
    mcpServers = @{
        filesystem = @{
            command = "node"
            args = @("$npmGlobalDir\@modelcontextprotocol\server-filesystem\dist\index.js")
            env = @{
                ALLOWED_DIRECTORIES = @(
                    $PWD.Path,
                    "$env:USERPROFILE\Documents",
                    "$env:USERPROFILE\Desktop"
                ) -join ";"
            }
        }
        git = @{
            command = "node"
            args = @("$npmGlobalDir\@modelcontextprotocol\server-git\dist\index.js")
            env = @{
                ALLOWED_REPOSITORIES = $PWD.Path
            }
        }
        github = @{
            command = "node"
            args = @("$npmGlobalDir\@modelcontextprotocol\server-github\dist\index.js")
            env = @{
                GITHUB_PERSONAL_ACCESS_TOKEN = ""  # User needs to fill this
            }
        }
        sqlite = @{
            command = "node"
            args = @("$npmGlobalDir\@modelcontextprotocol\server-sqlite\dist\index.js")
        }
    }
}

# Save MCP configuration
try {
    $mcpConfig | ConvertTo-Json -Depth 10 | Set-Content -Path $mcpConfigPath -Encoding UTF8
    Write-Host "✅ MCP configuration saved to: $mcpConfigPath" -ForegroundColor Green
} catch {
    Write-Error "❌ Failed to save MCP configuration: $($_.Exception.Message)"
}

# Step 5: Update VS Code settings
Write-Host "`n🔧 Updating VS Code settings..." -ForegroundColor Yellow

$vscodeSettingsPath = "$env:APPDATA\Code\User\settings.json"

if (Test-Path $vscodeSettingsPath) {
    try {
        $settings = Get-Content $vscodeSettingsPath -Raw | ConvertFrom-Json

        # Add MCP settings
        $settings | Add-Member -NotePropertyName "github.copilot.mcp.enabled" -NotePropertyValue $true -Force
        $settings | Add-Member -NotePropertyName "github.copilot.mcp.configPath" -NotePropertyValue $mcpConfigPath -Force

        # Save updated settings
        $settings | ConvertTo-Json -Depth 10 | Set-Content -Path $vscodeSettingsPath -Encoding UTF8
        Write-Host "✅ VS Code settings updated" -ForegroundColor Green

    } catch {
        Write-Warning "⚠️ Could not update VS Code settings: $($_.Exception.Message)"
    }
} else {
    Write-Warning "⚠️ VS Code settings file not found. You may need to configure MCP manually."
}

# Step 6: Create workspace-specific MCP config
Write-Host "`n📁 Creating workspace MCP configuration..." -ForegroundColor Yellow

$workspaceMcpPath = ".\.vscode\mcp.json"
$workspaceMcpDir = ".\.vscode"

if (-not (Test-Path $workspaceMcpDir)) {
    New-Item -Path $workspaceMcpDir -ItemType Directory -Force | Out-Null
}

$workspaceMcpConfig = @{
    mcpServers = @{
        "busbuddy-filesystem" = @{
            command = "node"
            args = @("$npmGlobalDir\@modelcontextprotocol\server-filesystem\dist\index.js")
            env = @{
                ALLOWED_DIRECTORIES = $PWD.Path
            }
        }
        "busbuddy-git" = @{
            command = "node"
            args = @("$npmGlobalDir\@modelcontextprotocol\server-git\dist\index.js")
            env = @{
                ALLOWED_REPOSITORIES = $PWD.Path
            }
        }
    }
}

try {
    $workspaceMcpConfig | ConvertTo-Json -Depth 10 | Set-Content -Path $workspaceMcpPath -Encoding UTF8
    Write-Host "✅ Workspace MCP configuration created: $workspaceMcpPath" -ForegroundColor Green
} catch {
    Write-Warning "⚠️ Could not create workspace MCP config: $($_.Exception.Message)"
}

# Step 7: Final verification
Write-Host "`n🔍 Final verification..." -ForegroundColor Yellow

$verificationResults = @()

# Check Node.js
try {
    $nodeVer = node --version 2>$null
    $npmVer = npm --version 2>$null
    if ($nodeVer -and $npmVer) {
        $verificationResults += "✅ Node.js: $nodeVer, npm: $npmVer"
    } else {
        $verificationResults += "❌ Node.js/npm not accessible"
    }
} catch {
    $verificationResults += "❌ Node.js verification failed"
}

# Check MCP servers
foreach ($server in $mcpServers) {
    $serverPath = "$npmGlobalDir\$server"
    if (Test-Path $serverPath) {
        $verificationResults += "✅ MCP Server: $server"
    } else {
        $verificationResults += "❌ MCP Server missing: $server"
    }
}

# Check configuration files
if (Test-Path $mcpConfigPath) {
    $verificationResults += "✅ MCP Config: $mcpConfigPath"
} else {
    $verificationResults += "❌ MCP Config missing"
}

if (Test-Path $workspaceMcpPath) {
    $verificationResults += "✅ Workspace MCP Config: $workspaceMcpPath"
} else {
    $verificationResults += "❌ Workspace MCP Config missing"
}

# Display results
Write-Host "`n📊 Installation Summary:" -ForegroundColor Cyan
Write-Host "========================" -ForegroundColor Cyan
foreach ($result in $verificationResults) {
    Write-Host $result
}

Write-Host "`n🎉 MCP Setup Complete!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Restart VS Code to load new MCP configuration" -ForegroundColor White
Write-Host "2. Open GitHub Copilot and verify MCP servers are available" -ForegroundColor White
Write-Host "3. Test MCP functionality with filesystem and git operations" -ForegroundColor White

if ($mcpConfig.mcpServers.github.env.GITHUB_PERSONAL_ACCESS_TOKEN -eq "") {
    Write-Host "`n⚠️ Note: GitHub MCP server requires a personal access token" -ForegroundColor Yellow
    Write-Host "Edit $mcpConfigPath and add your GitHub token for full functionality" -ForegroundColor White
}

Write-Host "`n🚌 BusBuddy MCP integration is ready!" -ForegroundColor Cyan
