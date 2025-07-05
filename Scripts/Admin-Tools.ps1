# Bus Buddy Administrator Tools
# Enhanced PowerShell tools that require administrator privileges

#Requires -RunAsAdministrator

param(
    [string]$Action = "Install",
    [switch]$ConfigureSystem,
    [switch]$SetupDevelopment,
    [switch]$InstallServices
)

function Write-AdminBanner {
    Write-Host "🔐 BUS BUDDY ADMINISTRATOR TOOLS" -ForegroundColor Red
    Write-Host "=" * 50 -ForegroundColor Yellow
    Write-Host "Running with Administrator privileges" -ForegroundColor Green
    Write-Host "PowerShell Version: $($PSVersionTable.PSVersion)" -ForegroundColor Cyan
    Write-Host ""
}

function Install-AdminOnlyTools {
    Write-Host "📦 INSTALLING ADMINISTRATOR-ONLY TOOLS:" -ForegroundColor Yellow

    # 1. Windows Update Management
    try {
        Install-Module -Name PSWindowsUpdate -Force -AllowClobber -Scope AllUsers
        Write-Host "✓ PSWindowsUpdate (Windows Update management)" -ForegroundColor Green
    } catch {
        Write-Warning "Failed to install PSWindowsUpdate: $($_.Exception.Message)"
    }

    # 2. Active Directory Tools (if domain joined)
    try {
        if ((Get-WmiObject -Class Win32_ComputerSystem).PartOfDomain) {
            Install-WindowsFeature -Name RSAT-AD-PowerShell -IncludeAllSubFeature
            Write-Host "✓ Active Directory PowerShell Module" -ForegroundColor Green
        }
    } catch {
        Write-Warning "Failed to install AD PowerShell tools: $($_.Exception.Message)"
    }

    # 3. Hyper-V Management (if available)
    try {
        $hyperv = Get-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V-All
        if ($hyperv.State -eq "Enabled") {
            Install-Module -Name Hyper-V -Force -Scope AllUsers
            Write-Host "✓ Hyper-V PowerShell Module" -ForegroundColor Green
        }
    } catch {
        Write-Warning "Hyper-V not available or failed to install"
    }

    # 4. System Administration Tools
    try {
        Install-Module -Name PowerShellForGitHub -Force -AllowClobber -Scope AllUsers
        Write-Host "✓ PowerShellForGitHub (GitHub automation)" -ForegroundColor Green
    } catch {
        Write-Warning "Failed to install PowerShellForGitHub: $($_.Exception.Message)"
    }

    # 5. Security and Compliance Tools
    try {
        Install-Module -Name Microsoft.PowerShell.SecretManagement -Force -AllowClobber -Scope AllUsers
        Install-Module -Name Microsoft.PowerShell.SecretStore -Force -AllowClobber -Scope AllUsers
        Write-Host "✓ Secret Management Tools" -ForegroundColor Green
    } catch {
        Write-Warning "Failed to install Secret Management tools: $($_.Exception.Message)"
    }
}

function Set-DevelopmentEnvironment {
    Write-Host "🔧 CONFIGURING DEVELOPMENT ENVIRONMENT:" -ForegroundColor Yellow

    # 1. Set PowerShell Execution Policy
    try {
        Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope LocalMachine -Force
        Write-Host "✓ Execution Policy set to RemoteSigned" -ForegroundColor Green
    } catch {
        Write-Warning "Failed to set execution policy: $($_.Exception.Message)"
    }

    # 2. Enable Developer Mode
    try {
        $regPath = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock"
        Set-ItemProperty -Path $regPath -Name "AllowDevelopmentWithoutDevLicense" -Value 1 -Type DWord
        Write-Host "✓ Developer Mode enabled" -ForegroundColor Green
    } catch {
        Write-Warning "Failed to enable Developer Mode: $($_.Exception.Message)"
    }

    # 3. Configure Git globally (if Git is installed)
    try {
        $gitPath = Get-Command git -ErrorAction SilentlyContinue
        if ($gitPath) {
            git config --system core.autocrlf true
            git config --system core.longpaths true
            Write-Host "✓ Git system configuration applied" -ForegroundColor Green
        }
    } catch {
        Write-Warning "Failed to configure Git: $($_.Exception.Message)"
    }

    # 4. Create system-wide PowerShell profile
    try {
        $profilePath = "$PSHOME\Profile.ps1"
        if (-not (Test-Path $profilePath)) {
            $profileContent = @"
# System-wide PowerShell Profile for Bus Buddy Development
# Auto-load development modules
Import-Module Terminal-Icons -ErrorAction SilentlyContinue
Import-Module Microsoft.PowerShell.ConsoleGuiTools -ErrorAction SilentlyContinue

# Set location to Bus Buddy project if it exists
if (Test-Path "C:\Users\$env:USERNAME\Desktop\Bus Buddy") {
    Set-Location "C:\Users\$env:USERNAME\Desktop\Bus Buddy"
}

# Development aliases
Set-Alias -Name bb-build -Value "dotnet build"
Set-Alias -Name bb-test -Value "dotnet test"
Set-Alias -Name bb-run -Value "dotnet run"

Write-Host "Bus Buddy Development Environment Loaded" -ForegroundColor Green
"@
            $profileContent | Out-File -FilePath $profilePath -Encoding UTF8
            Write-Host "✓ System-wide PowerShell profile created" -ForegroundColor Green
        }
    } catch {
        Write-Warning "Failed to create system profile: $($_.Exception.Message)"
    }
}

function Install-SystemServices {
    Write-Host "🚀 INSTALLING SYSTEM SERVICES:" -ForegroundColor Yellow

    # 1. Windows Terminal (if not installed)
    try {
        $terminal = Get-AppxPackage -Name Microsoft.WindowsTerminal
        if (-not $terminal) {
            winget install --id=Microsoft.WindowsTerminal -e --source winget
            Write-Host "✓ Windows Terminal installed" -ForegroundColor Green
        } else {
            Write-Host "✓ Windows Terminal already installed" -ForegroundColor Cyan
        }
    } catch {
        Write-Warning "Failed to install Windows Terminal: $($_.Exception.Message)"
    }

    # 2. PowerShell 7 (latest version)
    try {
        winget install --id Microsoft.Powershell --source winget
        Write-Host "✓ PowerShell 7 updated to latest version" -ForegroundColor Green
    } catch {
        Write-Warning "Failed to update PowerShell 7: $($_.Exception.Message)"
    }

    # 3. Git (if not installed)
    try {
        $git = Get-Command git -ErrorAction SilentlyContinue
        if (-not $git) {
            winget install --id Git.Git -e --source winget
            Write-Host "✓ Git installed" -ForegroundColor Green
        } else {
            Write-Host "✓ Git already installed" -ForegroundColor Cyan
        }
    } catch {
        Write-Warning "Failed to install Git: $($_.Exception.Message)"
    }

    # 4. .NET SDK (latest)
    try {
        winget install Microsoft.DotNet.SDK.8 --source winget
        Write-Host "✓ .NET SDK 8 installed/updated" -ForegroundColor Green
    } catch {
        Write-Warning "Failed to install .NET SDK: $($_.Exception.Message)"
    }
}

function Set-PowerShellConfiguration {
    Write-Host "⚙️ CONFIGURING POWERSHELL SETTINGS:" -ForegroundColor Yellow

    # 1. Install PowerShell modules to system location
    $modules = @(
        "PSReadLine",
        "Terminal-Icons",
        "Microsoft.PowerShell.ConsoleGuiTools",
        "powershell-yaml",
        "PSScriptAnalyzer",
        "Pester"
    )

    foreach ($module in $modules) {
        try {
            Install-Module -Name $module -Force -AllowClobber -Scope AllUsers -ErrorAction Stop
            Write-Host "✓ $module (system-wide)" -ForegroundColor Green
        } catch {
            Write-Warning "Failed to install $module system-wide: $($_.Exception.Message)"
        }
    }

    # 2. Configure PSReadLine system-wide
    try {
        $psReadLineConfig = @"
# PSReadLine Configuration
Set-PSReadLineOption -PredictionSource History
Set-PSReadLineOption -PredictionViewStyle ListView
Set-PSReadLineOption -EditMode Windows
Set-PSReadLineKeyHandler -Key Tab -Function Complete
"@
        $configPath = "$PSHOME\PSReadLineProfile.ps1"
        $psReadLineConfig | Out-File -FilePath $configPath -Encoding UTF8
        Write-Host "✓ PSReadLine configured system-wide" -ForegroundColor Green
    } catch {
        Write-Warning "Failed to configure PSReadLine: $($_.Exception.Message)"
    }
}

function Test-AdminPrivileges {
    Write-Host "🔍 TESTING ADMINISTRATOR CAPABILITIES:" -ForegroundColor Yellow

    # Test various admin operations
    $tests = @(
        @{ Name = "Registry Write"; Test = { Set-ItemProperty -Path "HKLM:\SOFTWARE" -Name "TestKey" -Value "Test" -ErrorAction Stop; Remove-ItemProperty -Path "HKLM:\SOFTWARE" -Name "TestKey" -ErrorAction Stop } },
        @{ Name = "Service Control"; Test = { Get-Service | Select-Object -First 1 | Stop-Service -WhatIf } },
        @{ Name = "Event Log Access"; Test = { Get-EventLog -LogName System -Newest 1 } },
        @{ Name = "File System Admin"; Test = { Test-Path "$env:SystemRoot\System32" } },
        @{ Name = "Module Installation"; Test = { Find-Module PSScriptAnalyzer } }
    )

    foreach ($test in $tests) {
        try {
            & $test.Test | Out-Null
            Write-Host "✓ $($test.Name)" -ForegroundColor Green
        } catch {
            Write-Host "✗ $($test.Name): $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

function Show-AdminStatus {
    Write-Host "📊 ADMINISTRATOR STATUS REPORT:" -ForegroundColor Cyan

    $status = @{
        "PowerShell Version" = $PSVersionTable.PSVersion
        "Execution Policy" = Get-ExecutionPolicy
        "Current User" = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name
        "Is Administrator" = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")
        "Machine Name" = $env:COMPUTERNAME
        "Domain Status" = (Get-WmiObject -Class Win32_ComputerSystem).PartOfDomain
    }

    $status.GetEnumerator() | ForEach-Object {
        $color = if ($_.Key -eq "Is Administrator" -and $_.Value) { "Green" } else { "White" }
        Write-Host "   $($_.Key): $($_.Value)" -ForegroundColor $color
    }
}

# Main execution logic
Write-AdminBanner

switch ($Action.ToLower()) {
    "install" {
        Install-AdminOnlyTools
        if ($ConfigureSystem) { Set-DevelopmentEnvironment }
        if ($InstallServices) { Install-SystemServices }
    }
    "configure" {
        Set-DevelopmentEnvironment
        Set-PowerShellConfiguration
    }
    "services" {
        Install-SystemServices
    }
    "test" {
        Test-AdminPrivileges
    }
    "status" {
        Show-AdminStatus
    }
    "all" {
        Install-AdminOnlyTools
        Set-DevelopmentEnvironment
        Install-SystemServices
        Set-PowerShellConfiguration
        Test-AdminPrivileges
        Show-AdminStatus
    }
    default {
        Write-Host "Bus Buddy Administrator Tools" -ForegroundColor Red
        Write-Host "Available actions:" -ForegroundColor White
        Write-Host "  Install     - Install admin-only tools" -ForegroundColor Gray
        Write-Host "  Configure   - Configure development environment" -ForegroundColor Gray
        Write-Host "  Services    - Install system services" -ForegroundColor Gray
        Write-Host "  Test        - Test admin privileges" -ForegroundColor Gray
        Write-Host "  Status      - Show admin status" -ForegroundColor Gray
        Write-Host "  All         - Run all configurations" -ForegroundColor Gray
        Write-Host "`nExample: .\Admin-Tools.ps1 -Action All -ConfigureSystem -InstallServices" -ForegroundColor Cyan

        Show-AdminStatus
    }
}
