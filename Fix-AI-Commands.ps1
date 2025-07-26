#Requires -Version 7.5

<#
.SYNOPSIS
    AI Command Diagnostic and Fix Tool
.DESCRIPTION
    Identifies and fixes common PowerShell syntax issues related to AI commands
.EXAMPLE
    .\Fix-AI-Commands.ps1
#>

Write-Host "🤖 BusBuddy AI Commands Diagnostic Tool" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════" -ForegroundColor Cyan

# Function to list available AI-related commands properly
function Get-BusBuddyAICommands {
    Write-Host "📋 Available AI Commands:" -ForegroundColor Yellow

    # Check for BusBuddy module commands
    $busBuddyCommands = @()
    try {
        $busBuddyCommands = Get-Command -Module BusBuddy -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -like '*AI*' } |
        Select-Object Name, ModuleName, CommandType
    }
    catch {
        Write-Host "⚠️ BusBuddy module not loaded" -ForegroundColor Yellow
    }

    # Check for PSAISuite commands
    $psaiCommands = @()
    try {
        if (Get-Module PSAISuite -ErrorAction SilentlyContinue) {
            $psaiCommands = Get-Command -Module PSAISuite |
            Select-Object Name, ModuleName, CommandType
        }
    }
    catch {
        Write-Host "⚠️ PSAISuite module not available" -ForegroundColor Yellow
    }

    # Display results
    if ($busBuddyCommands.Count -gt 0) {
        Write-Host "🚌 BusBuddy AI Commands:" -ForegroundColor Green
        $busBuddyCommands | Format-Table -AutoSize
    }
    else {
        Write-Host "❌ No BusBuddy AI commands found" -ForegroundColor Red
    }

    if ($psaiCommands.Count -gt 0) {
        Write-Host "🤖 PSAISuite Commands:" -ForegroundColor Green
        $psaiCommands | Format-Table -AutoSize
    }
    else {
        Write-Host "❌ PSAISuite not loaded. Install with: Install-Module PSAISuite -Force" -ForegroundColor Red
    }

    return @{
        BusBuddyCommands = $busBuddyCommands
        PSAICommands     = $psaiCommands
    }
}

# Function to demonstrate correct syntax
function Show-CorrectSyntax {
    Write-Host "✅ Correct PowerShell Syntax Examples:" -ForegroundColor Green
    Write-Host "────────────────────────────────────────" -ForegroundColor Gray

    Write-Host "❌ WRONG:" -ForegroundColor Red
    Write-Host "   Get-Command -Module BusBuddy | Where-Object { .Name -like '*AI*' }" -ForegroundColor Red

    Write-Host "✅ CORRECT:" -ForegroundColor Green
    Write-Host "   Get-Command -Module BusBuddy | Where-Object { `$_.Name -like '*AI*' }" -ForegroundColor Green

    Write-Host ""
    Write-Host "📝 Key Points:" -ForegroundColor Yellow
    Write-Host "   • Always use `$_ to reference the current pipeline object" -ForegroundColor White
    Write-Host "   • The dot (.) by itself is not valid PowerShell syntax" -ForegroundColor White
    Write-Host "   • Use `$_.PropertyName to access object properties in Where-Object" -ForegroundColor White
}

# Function to test AI integration
function Test-AIIntegration {
    Write-Host "🧪 Testing AI Integration:" -ForegroundColor Cyan

    # Test PSAISuite
    if (Get-Module PSAISuite -ErrorAction SilentlyContinue) {
        Write-Host "✅ PSAISuite module is loaded" -ForegroundColor Green

        # Test if environment variables are set
        $envVars = @('OPENAI_API_KEY', 'XAI_API_KEY', 'ANTHROPIC_API_KEY')
        foreach ($var in $envVars) {
            $value = [System.Environment]::GetEnvironmentVariable($var)
            if ($value) {
                Write-Host "✅ $var is configured" -ForegroundColor Green
            }
            else {
                Write-Host "⚠️ $var not configured" -ForegroundColor Yellow
            }
        }
    }
    else {
        Write-Host "❌ PSAISuite module not loaded" -ForegroundColor Red
        Write-Host "   Install with: Install-Module PSAISuite -Force" -ForegroundColor Yellow
    }

    # Test BusBuddy AI features
    $aiScripts = Get-ChildItem -Path "Scripts\AI" -Filter "*.ps1" -ErrorAction SilentlyContinue
    if ($aiScripts) {
        Write-Host "✅ Found $($aiScripts.Count) AI scripts in Scripts\AI\" -ForegroundColor Green
        $aiScripts | ForEach-Object { Write-Host "   • $($_.Name)" -ForegroundColor Gray }
    }
    else {
        Write-Host "⚠️ No AI scripts found in Scripts\AI\" -ForegroundColor Yellow
    }
}

# Function to install required modules
function Install-RequiredModules {
    Write-Host "📦 Installing Required AI Modules:" -ForegroundColor Cyan

    $modules = @('PSAISuite', 'Show-Progress', 'PSWriteColor', 'psInlineProgress')

    foreach ($module in $modules) {
        try {
            if (-not (Get-Module -ListAvailable -Name $module)) {
                Write-Host "Installing $module..." -ForegroundColor Yellow
                Install-Module -Name $module -Force -Scope CurrentUser
                Write-Host "✅ $module installed successfully" -ForegroundColor Green
            }
            else {
                Write-Host "✅ $module already installed" -ForegroundColor Green
            }
        }
        catch {
            Write-Host "❌ Failed to install $module : $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

# Main execution
try {
    Show-CorrectSyntax
    Write-Host ""

    $commands = Get-BusBuddyAICommands
    Write-Host ""

    Test-AIIntegration
    Write-Host ""

    Write-Host "🔧 Would you like to install missing AI modules? (Y/N)" -ForegroundColor Cyan
    $response = Read-Host
    if ($response -eq 'Y' -or $response -eq 'y') {
        Install-RequiredModules
    }

    Write-Host ""
    Write-Host "✅ AI Commands Diagnostic Complete!" -ForegroundColor Green
    Write-Host "📖 Use the correct syntax shown above for PowerShell commands" -ForegroundColor Yellow
}
catch {
    Write-Host "❌ Error during diagnostic: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Stack Trace: $($_.ScriptStackTrace)" -ForegroundColor Gray
}
