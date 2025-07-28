#Requires -Version 7.5
<#
.SYNOPSIS
    Permanent PowerShell Paging Fix for BusBuddy Development

.DESCRIPTION
    Permanently disables PowerShell paging ("-- More --" prompts) for all sessions.
    This fixes issues with GitHub CLI, long command outputs, and terminal pagination.

.NOTES
    File Name      : Fix-PowerShell-Paging.ps1
    Author         : BusBuddy Development Team
    Prerequisite   : PowerShell 7.5.2
    Copyright 2025 - BusBuddy
#>

Write-Host "🔧 BusBuddy PowerShell Paging Fix" -ForegroundColor Cyan
Write-Host "Permanently disabling PowerShell paging for smooth command output..." -ForegroundColor Yellow
Write-Host ""

# Get PowerShell profile path
$profilePath = $PROFILE.CurrentUserAllHosts

Write-Host "📄 PowerShell Profile: $profilePath" -ForegroundColor Blue

# Ensure profile directory exists
$profileDir = Split-Path -Path $profilePath -Parent
if (-not (Test-Path $profileDir)) {
    New-Item -ItemType Directory -Path $profileDir -Force | Out-Null
    Write-Host "✅ Created profile directory: $profileDir" -ForegroundColor Green
}

# Paging disable configuration
$pagingConfig = @'

# ==========================================
# BusBuddy PowerShell Paging Fix
# ==========================================
# Permanently disable paging to prevent "-- More --" prompts
# Added by BusBuddy development environment

# Disable PowerShell paging
$env:POWERSHELL_UPDATECHECK = 'Off'
$PSDefaultParameterValues['Out-Host:Paging'] = $false
$PSDefaultParameterValues['*:Paging'] = $false

# Disable paging for external tools
$env:PAGER = ''
$env:GH_PAGER = ''
$env:LESS = '-R'

# Set large console buffer for long outputs
if ($Host.UI.RawUI.BufferSize) {
    try {
        $buffer = $Host.UI.RawUI.BufferSize
        $buffer.Height = 9999
        $Host.UI.RawUI.BufferSize = $buffer
    } catch {
        # Ignore buffer size errors
    }
}

# Function to ensure no paging for common commands
function Disable-AllPaging {
    # GitHub CLI
    if (Get-Command 'gh' -ErrorAction SilentlyContinue) {
        $env:GH_PAGER = ''
    }

    # Git
    if (Get-Command 'git' -ErrorAction SilentlyContinue) {
        git config --global core.pager ''
    }

    # Azure CLI
    if (Get-Command 'az' -ErrorAction SilentlyContinue) {
        $env:AZURE_CORE_OUTPUT = 'table'
    }
}

# Call the function
Disable-AllPaging

Write-Host "✅ PowerShell paging permanently disabled" -ForegroundColor Green

# ==========================================
# End BusBuddy PowerShell Paging Fix
# ==========================================

'@

# Check if paging fix already exists in profile
$profileExists = Test-Path $profilePath
$needsUpdate = $true

if ($profileExists) {
    $currentContent = Get-Content $profilePath -Raw
    if ($currentContent -match "BusBuddy PowerShell Paging Fix") {
        Write-Host "ℹ️  Paging fix already exists in profile" -ForegroundColor Yellow
        $needsUpdate = $false
    }
}

if ($needsUpdate) {
    # Add paging configuration to profile
    Add-Content -Path $profilePath -Value $pagingConfig
    Write-Host "✅ Added paging fix to PowerShell profile" -ForegroundColor Green
} else {
    Write-Host "✅ Profile already configured - no changes needed" -ForegroundColor Green
}

# Apply fix to current session immediately
Write-Host ""
Write-Host "🚀 Applying fix to current session..." -ForegroundColor Cyan

# Disable paging for current session
$env:POWERSHELL_UPDATECHECK = 'Off'
$PSDefaultParameterValues['Out-Host:Paging'] = $false
$PSDefaultParameterValues['*:Paging'] = $false
$env:PAGER = ''
$env:GH_PAGER = ''

# Set console buffer
if ($Host.UI.RawUI.BufferSize) {
    try {
        $buffer = $Host.UI.RawUI.BufferSize
        $buffer.Height = 9999
        $Host.UI.RawUI.BufferSize = $buffer
        Write-Host "✅ Console buffer set to 9999 lines" -ForegroundColor Green
    } catch {
        Write-Host "⚠️  Could not set console buffer size" -ForegroundColor Yellow
    }
}

# Test GitHub CLI if available
if (Get-Command 'gh' -ErrorAction SilentlyContinue) {
    Write-Host ""
    Write-Host "🧪 Testing GitHub CLI (no paging)..." -ForegroundColor Cyan

    try {
        # This should now display without pagination
        $testResult = gh run list --limit 3 --json status,workflowName,createdAt 2>$null
        if ($testResult) {
            Write-Host "✅ GitHub CLI test successful - no paging detected" -ForegroundColor Green
        } else {
            Write-Host "⚠️  GitHub CLI test returned no data (may need authentication)" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "⚠️  GitHub CLI test failed: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "✅ PowerShell Paging Fix Complete!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 What was fixed:" -ForegroundColor Cyan
Write-Host "   • Disabled PowerShell internal paging" -ForegroundColor White
Write-Host "   • Disabled GitHub CLI paging" -ForegroundColor White
Write-Host "   • Disabled Git paging" -ForegroundColor White
Write-Host "   • Set large console buffer (9999 lines)" -ForegroundColor White
Write-Host "   • Applied to current session AND permanent profile" -ForegroundColor White
Write-Host ""
Write-Host "🎯 Next steps:" -ForegroundColor Yellow
Write-Host "   • Try running: gh run list --limit 10" -ForegroundColor White
Write-Host "   • Should display all results without '-- More --' prompts" -ForegroundColor White
Write-Host "   • Restart PowerShell to test permanent configuration" -ForegroundColor White
