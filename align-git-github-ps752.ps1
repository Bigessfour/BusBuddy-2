#Requires -Version 7.5.2
<#
.SYNOPSIS
    Complete VS Code, PowerShell 7.5.2, Git and GitHub Integration for Bus Buddy

.DESCRIPTION
    This script provides comprehensive setup and configuration for:
    - VS Code integration with PowerShell 7.5.2
    - Git configuration optimized for PowerShell
    - GitHub CLI integration with VS Code
    - Bus Buddy project-specific settings

.NOTES
    Author: GitHub Copilot
    Date: July 19, 2025
    Version: 2.0
    Requirements: PowerShell 7.5.2, Git 2.50+, GitHub CLI 2.74+, VS Code
#>

Write-Host "🚀 Aligning PowerShell 7.5.2 with Git and GitHub..." -ForegroundColor Cyan

# Check PowerShell version
$psVersion = $PSVersionTable.PSVersion
Write-Host "PowerShell Version: $($psVersion.Major).$($psVersion.Minor).$($psVersion.Patch)" -ForegroundColor Yellow

# Check Git version
try {
    $gitVersion = (git --version) -replace 'git version ', ''
    Write-Host "Git Version: $gitVersion" -ForegroundColor Yellow
} catch {
    Write-Host "❌ Git not found or error checking version: $_" -ForegroundColor Red
    exit 1
}

# Check GitHub CLI version
try {
    $ghVersion = (gh --version)[0] -replace 'gh version ', ''
    Write-Host "GitHub CLI Version: $ghVersion" -ForegroundColor Yellow
} catch {
    Write-Host "⚠️ GitHub CLI not found or error checking version. Consider installing it." -ForegroundColor Yellow
}

Write-Host "`n📋 Configuring Git for PowerShell 7.5.2..." -ForegroundColor Green

# Step 1: Configure Git for PowerShell 7.5.2
# ==========================================

# Fix pager settings for PowerShell 7.5.2 compatibility
Write-Host "Updating Git pager settings..." -ForegroundColor Blue
git config --global core.pager ""
git config --global pager.branch false
git config --global pager.config false
git config --global pager.diff false
git config --global pager.log false
git config --global pager.show false
git config --global pager.status false

# Configure text handling for better PowerShell compatibility
Write-Host "Optimizing text handling..." -ForegroundColor Blue
git config --global core.autocrlf true
git config --global i18n.logoutputencoding utf-8
git config --global core.quotepath false

# Security and performance settings
Write-Host "Enhancing security and performance..." -ForegroundColor Blue
git config --global credential.helper manager
git config --global core.longpaths true
git config --global push.default simple
git config --global pull.rebase false
git config --global init.defaultBranch main

# Step 2: GitHub Integration Enhancements
# ======================================

Write-Host "`n🔄 Enhancing GitHub Integration..." -ForegroundColor Green

# Test GitHub authentication if GitHub CLI is installed
if ($null -ne (Get-Command gh -ErrorAction SilentlyContinue)) {
    Write-Host "Testing GitHub authentication..." -ForegroundColor Blue
    try {
        $ghAuthStatus = gh auth status -h github.com 2>&1
        if ($ghAuthStatus -like "*Logged in to github.com*") {
            Write-Host "✅ Already authenticated with GitHub" -ForegroundColor Green
        } else {
            Write-Host "⚠️ GitHub authentication needed. Run 'gh auth login' after this script completes." -ForegroundColor Yellow
        }
    } catch {
        Write-Host "⚠️ Could not verify GitHub authentication status" -ForegroundColor Yellow
    }
}

# Step 3: Update Terminal Configuration
# ====================================

Write-Host "`n🖥️ Configuring Terminal for optimal Git integration..." -ForegroundColor Green

# Create function to add to PowerShell profile
$profileAddition = @"

# Git and GitHub integration for PowerShell 7.5.2 (Added $(Get-Date -Format "yyyy-MM-dd"))
function Initialize-GitEnvironment {
    # Disable Git paging for PowerShell
    `$env:GIT_PAGER = ""

    # Additional Git environment variables for PowerShell 7.5.2
    `$env:GIT_CEILING_DIRECTORIES = [System.IO.Path]::GetTempPath()
    `$env:GIT_TRACE2_EVENT = 0

    # Set color scheme for Git output in PowerShell
    `$env:TERM = 'xterm-256color'

    Write-Host "Git environment initialized for PowerShell 7.5.2" -ForegroundColor DarkGray
}

# Bus Buddy GitHub shortcuts
function bb-gh-open {
    gh repo view --web
}

function bb-gh-pr {
    gh pr list
}

function bb-gh-issues {
    gh issue list
}

# Initialize Git environment when profile loads
Initialize-GitEnvironment

"@

# Check if profile exists
$profilePath = $PROFILE.CurrentUserAllHosts
$profileDir = Split-Path $profilePath -Parent

# Create profile directory if it doesn't exist
if (-not (Test-Path $profileDir)) {
    New-Item -ItemType Directory -Path $profileDir -Force | Out-Null
}

# Check if profile already has our additions
if (Test-Path $profilePath) {
    $profileContent = Get-Content $profilePath -Raw
    if ($profileContent -notlike "*Initialize-GitEnvironment*") {
        Write-Host "Adding Git/GitHub integration to PowerShell profile at $profilePath" -ForegroundColor Blue
        Add-Content -Path $profilePath -Value $profileAddition
    } else {
        Write-Host "✅ PowerShell profile already contains Git/GitHub integration" -ForegroundColor Green
    }
} else {
    Write-Host "Creating new PowerShell profile with Git/GitHub integration at $profilePath" -ForegroundColor Blue
    Set-Content -Path $profilePath -Value $profileAddition
}

# Step 4: Verify VS Code Integration
# =================================

Write-Host "`n🔍 Verifying VS Code Integration..." -ForegroundColor Green

# Check VS Code settings.json for correct PowerShell and terminal configuration
$vscodePath = Join-Path $PSScriptRoot ".vscode"
$settingsPath = Join-Path $vscodePath "settings.json"

if (Test-Path $settingsPath) {
    Write-Host "VS Code settings found. Checking for correct PowerShell 7.5.2 configuration..." -ForegroundColor Blue

    $settingsContent = Get-Content $settingsPath -Raw
    $containsCorrectProfile = $settingsContent -like "*PowerShell 7.5.2*"

    if ($containsCorrectProfile) {
        Write-Host "✅ VS Code is correctly configured for PowerShell 7.5.2" -ForegroundColor Green
    } else {
        Write-Host "⚠️ VS Code might need updates for PowerShell 7.5.2 integration" -ForegroundColor Yellow
        Write-Host "   Review the settings.json file and update terminal.integrated.profiles.windows" -ForegroundColor Yellow
    }
} else {
    Write-Host "⚠️ VS Code settings.json not found" -ForegroundColor Yellow
}

# Final step: Display Git configuration
Write-Host "`n🎯 Git Configuration Summary:" -ForegroundColor Magenta
$env:GIT_PAGER = ""
git config --global --list | ForEach-Object {
    $key, $value = $_ -split '=', 2
    Write-Host "  $key" -ForegroundColor Cyan -NoNewline
    Write-Host "=$value" -ForegroundColor White
}

# Step 5: VS Code Extension Configuration
# ===============================

Write-Host "`n🧩 Configuring VS Code Extensions for Bus Buddy Development..." -ForegroundColor Green

# Check for VS Code installation
if ($null -ne (Get-Command code -ErrorAction SilentlyContinue)) {
    Write-Host "VS Code is installed and available in PATH" -ForegroundColor Blue

    # Define list of essential extensions for Bus Buddy development
    $requiredExtensions = @(
        # PowerShell development
        @{id = "ms-vscode.powershell"; name = "PowerShell Extension" },

        # Git integration
        @{id = "eamodio.gitlens"; name = "GitLens" },
        @{id = "github.vscode-github-actions"; name = "GitHub Actions" },
        @{id = "github.vscode-pull-request-github"; name = "GitHub Pull Requests and Issues" },

        # C# development
        @{id = "ms-dotnettools.csharp"; name = "C#" },
        @{id = "ms-dotnettools.dotnet-interactive-vscode"; name = ".NET Interactive" },

        # XAML support
        @{id = "teamhub.xaml-styler"; name = "XAML Styler" },
        @{id = "dabbinavo.xaml-language-features"; name = "XAML Language Features" },

        # Task management
        @{id = "spmeesseman.vscode-taskexplorer"; name = "Task Explorer" },

        # Optional but recommended
        @{id = "ms-vscode.vs-keybindings"; name = "Visual Studio Keymap" },
        @{id = "streetsidesoftware.code-spell-checker"; name = "Code Spell Checker" }
    )

    # Check for and install missing extensions
    Write-Host "Checking for required extensions..." -ForegroundColor Blue
    $installedExtensions = code --list-extensions

    foreach ($extension in $requiredExtensions) {
        if ($installedExtensions -contains $extension.id) {
            Write-Host "  ✓ $($extension.name) is already installed" -ForegroundColor Green
        } else {
            Write-Host "  → Installing $($extension.name)..." -ForegroundColor Yellow
            try {
                code --install-extension $extension.id --force
                Write-Host "    ✓ Installed successfully" -ForegroundColor Green
            } catch {
                Write-Host "    ❌ Failed to install: $_" -ForegroundColor Red
            }
        }
    }

    # Configure PowerShell extension settings
    Write-Host "`nConfiguring PowerShell extension settings..." -ForegroundColor Blue

    $vsCodeSettingsDir = Join-Path $env:APPDATA "Code\User"
    $userSettingsPath = Join-Path $vsCodeSettingsDir "settings.json"

    if (Test-Path $userSettingsPath) {
        try {
            $userSettings = Get-Content $userSettingsPath -Raw | ConvertFrom-Json -ErrorAction Stop

            # Create PowerShell settings if they don't exist
            $psSettingsToAdd = @{
                "powershell.powerShellDefaultVersion"                 = "PowerShell 7.5.2"
                "powershell.promptToUpdatePowerShell"                 = false
                "powershell.integratedConsole.showOnStartup"          = false
                "powershell.startAutomatically"                       = true
                "powershell.scriptAnalysis.enable"                    = true
                "powershell.buttons.showRunButtons"                   = true
                "powershell.codeFormatting.preset"                    = "OTBS"
                "powershell.codeFormatting.openBraceOnSameLine"       = true
                "powershell.codeFormatting.newLineAfterOpenBrace"     = true
                "powershell.codeFormatting.newLineAfterCloseBrace"    = true
                "powershell.codeFormatting.whitespaceBeforeOpenBrace" = true
                "powershell.codeFormatting.whitespaceBeforeOpenParen" = true
                "powershell.codeFormatting.whitespaceAroundOperator"  = true
                "powershell.codeFormatting.whitespaceAfterSeparator"  = true
                "powershell.codeFormatting.alignPropertyValuePairs"   = true
                "powershell.codeFormatting.useCorrectCasing"          = true
            }

            $settingsModified = $false
            foreach ($key in $psSettingsToAdd.Keys) {
                if (-not (Get-Member -InputObject $userSettings -Name $key -MemberType Properties)) {
                    $userSettings | Add-Member -MemberType NoteProperty -Name $key -Value $psSettingsToAdd[$key]
                    $settingsModified = $true
                }
            }

            # Update terminal profiles if needed
            if (-not (Get-Member -InputObject $userSettings -Name "terminal.integrated.profiles.windows" -MemberType Properties)) {
                $userSettings | Add-Member -MemberType NoteProperty -Name "terminal.integrated.profiles.windows" -Value @{
                    "PowerShell 7.5.2" = @{
                        "path" = "pwsh.exe"
                        "args" = @("-NoProfile", "-NoExit")
                        "icon" = "terminal-powershell"
                    }
                }

                $userSettings | Add-Member -MemberType NoteProperty -Name "terminal.integrated.defaultProfile.windows" -Value "PowerShell 7.5.2" -Force
                $settingsModified = $true
            }

            if ($settingsModified) {
                $userSettings | ConvertTo-Json -Depth 10 | Set-Content $userSettingsPath
                Write-Host "  ✓ VS Code user settings updated with PowerShell 7.5.2 configuration" -ForegroundColor Green
            } else {
                Write-Host "  ✓ VS Code user settings already contain PowerShell configuration" -ForegroundColor Green
            }
        } catch {
            Write-Host "  ❌ Could not update VS Code settings: $_" -ForegroundColor Red
        }
    } else {
        Write-Host "  ⚠️ VS Code user settings file not found at $userSettingsPath" -ForegroundColor Yellow
    }

    # Configure GitHub CLI integration with VS Code
    if ($null -ne (Get-Command gh -ErrorAction SilentlyContinue)) {
        Write-Host "`nConfiguring GitHub CLI to use VS Code as default editor..." -ForegroundColor Blue
        gh config set editor "code -w" | Out-Null
        Write-Host "  ✓ GitHub CLI now uses VS Code as default editor" -ForegroundColor Green
    }
} else {
    Write-Host "⚠️ VS Code not found in PATH. Install VS Code or add it to your PATH to enable extension management." -ForegroundColor Yellow
}

# Step 6: GitHub CLI UI Integration
# ================================

Write-Host "`n🔗 Setting up GitHub CLI UI Integration..." -ForegroundColor Green

if ($null -ne (Get-Command gh -ErrorAction SilentlyContinue)) {
    # Configure default behavior for GitHub CLI
    Write-Host "Configuring GitHub CLI defaults..." -ForegroundColor Blue
    gh config set git_protocol https
    gh config set prompt disabled
    gh config set pager less

    # Test authentication and provide instructions
    $authStatus = gh auth status 2>&1
    if ($authStatus -like "*Logged in to github.com*") {
        Write-Host "  ✓ GitHub CLI is authenticated" -ForegroundColor Green

        # Create Bus Buddy GitHub CLI aliases
        Write-Host "Creating helpful GitHub CLI aliases for Bus Buddy development..." -ForegroundColor Blue

        gh alias set bb-issues 'issue list --limit 10 --state open'
        gh alias set bb-prs 'pr list --limit 10 --state open'
        gh alias set bb-create-issue 'issue create --web'
        gh alias set bb-create-pr 'pr create --web'
        gh alias set bb-view 'repo view --web'

        Write-Host "  ✓ GitHub CLI aliases created. Try them with: gh bb-issues, gh bb-prs, etc." -ForegroundColor Green
    } else {
        Write-Host "  ⚠️ GitHub CLI not authenticated. Run 'gh auth login' to authenticate." -ForegroundColor Yellow
    }
} else {
    Write-Host "  ⚠️ GitHub CLI not found. Install it from: https://cli.github.com/" -ForegroundColor Yellow
}

# Step 7: Create Bus Buddy VS Code workspace file
# =============================================

Write-Host "`n📁 Creating Bus Buddy VS Code workspace file..." -ForegroundColor Green

$workspaceFilePath = Join-Path $PSScriptRoot "BusBuddy.code-workspace"

if (-not (Test-Path $workspaceFilePath)) {
    $workspaceContent = @{
        "folders"    = @(
            @{
                "path" = "."
                "name" = "Bus Buddy"
            }
        )
        "settings"   = @{
            "files.exclude"          = @{
                "**/bin"      = true
                "**/obj"      = true
                "**/.vs"      = true
                "**/packages" = true
            }
            "files.associations"     = @{
                "*.xaml"   = "xaml"
                "*.ps1"    = "powershell"
                "*.csproj" = "xml"
            }
            "editor.formatOnSave"    = true
            "dotnet.defaultSolution" = "BusBuddy.sln"
        }
        "extensions" = @{
            "recommendations" = @(
                "ms-vscode.powershell",
                "ms-dotnettools.csharp",
                "teamhub.xaml-styler",
                "dabbinavo.xaml-language-features",
                "spmeesseman.vscode-taskexplorer",
                "eamodio.gitlens",
                "github.vscode-github-actions"
            )
        }
    }

    $workspaceContent | ConvertTo-Json -Depth 10 | Set-Content $workspaceFilePath
    Write-Host "  ✓ Created workspace file: $workspaceFilePath" -ForegroundColor Green
    Write-Host "    Open this workspace file to work with the Bus Buddy project" -ForegroundColor Blue
} else {
    Write-Host "  ✓ Workspace file already exists at: $workspaceFilePath" -ForegroundColor Green
}

Write-Host "`n🎉 Setup Complete! PowerShell 7.5.2, Git, GitHub, and VS Code are now fully integrated!" -ForegroundColor Green
Write-Host "   Next steps:" -ForegroundColor Yellow
Write-Host "   1. Restart VS Code" -ForegroundColor Yellow
Write-Host "   2. Open the Bus Buddy workspace file: $workspaceFilePath" -ForegroundColor Yellow
Write-Host "   3. Run '. \$PROFILE' in your terminal to apply profile changes" -ForegroundColor Yellow
Write-Host "   4. Use Task Explorer in VS Code to run Bus Buddy tasks" -ForegroundColor Yellow
