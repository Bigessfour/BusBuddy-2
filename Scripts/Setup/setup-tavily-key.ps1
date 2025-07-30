#Requires -Version 7.5

<#
.SYNOPSIS
    Securely set up Tavily API key for MCP integration
.DESCRIPTION
    This script helps you securely configure your Tavily API key for the MCP server
    without hardcoding it in any files.
#>

param(
    [switch]$Check,
    [switch]$Remove
)

$KeyName = "TAVILY_API_KEY"

if ($Check) {
    Write-Host "Checking Tavily API key configuration..." -ForegroundColor Cyan

    $currentKey = [Environment]::GetEnvironmentVariable($KeyName, "User")
    if ($currentKey -and $currentKey -ne "tvly-EXAMPLE-KEY") {
        Write-Host "✅ Tavily API key is configured" -ForegroundColor Green
        Write-Host "Key starts with: $($currentKey.Substring(0, [Math]::Min(8, $currentKey.Length)))..." -ForegroundColor Yellow
    } elseif ($currentKey -eq "tvly-EXAMPLE-KEY") {
        Write-Host "⚠️ Tavily API key is set to placeholder value" -ForegroundColor Yellow
        Write-Host "Run this script without -Check to set your real key" -ForegroundColor White
    } else {
        Write-Host "❌ Tavily API key is not configured" -ForegroundColor Red
        Write-Host "Run this script without -Check to set your key" -ForegroundColor White
    }
    exit
}

if ($Remove) {
    Write-Host "Removing Tavily API key..." -ForegroundColor Yellow
    [Environment]::SetEnvironmentVariable($KeyName, $null, "User")
    Write-Host "✅ Tavily API key removed" -ForegroundColor Green
    exit
}

Write-Host "=== Tavily API Key Setup ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "This will securely set your Tavily API key as a user environment variable." -ForegroundColor White
Write-Host "The key will NOT be stored in any files or committed to git." -ForegroundColor Green
Write-Host ""

# Check if key already exists
$existingKey = [Environment]::GetEnvironmentVariable($KeyName, "User")
if ($existingKey -and $existingKey -ne "tvly-EXAMPLE-KEY") {
    Write-Host "A Tavily API key is already configured." -ForegroundColor Yellow
    $overwrite = Read-Host "Do you want to overwrite it? (y/N)"
    if ($overwrite -ne 'y' -and $overwrite -ne 'Y') {
        Write-Host "Cancelled." -ForegroundColor Yellow
        exit
    }
}

Write-Host ""
Write-Host "Please enter your Tavily API key:" -ForegroundColor Yellow
Write-Host "(It should start with 'tvly-')" -ForegroundColor Gray

# Secure input for API key
$apiKey = Read-Host "API Key" -AsSecureString
$apiKeyPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($apiKey))

# Validate key format
if (-not $apiKeyPlain.StartsWith("tvly-")) {
    Write-Host "❌ Invalid key format. Tavily keys should start with 'tvly-'" -ForegroundColor Red
    exit 1
}

if ($apiKeyPlain.Length -lt 20) {
    Write-Host "❌ Key seems too short. Please check your key." -ForegroundColor Red
    exit 1
}

# Set the environment variable
try {
    [Environment]::SetEnvironmentVariable($KeyName, $apiKeyPlain, "User")
    Write-Host ""
    Write-Host "✅ Tavily API key set successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "The key is now stored as a user environment variable." -ForegroundColor White
    Write-Host "You may need to restart VS Code for it to take effect." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To test your MCP setup:" -ForegroundColor Cyan
    Write-Host "1. Restart VS Code" -ForegroundColor White
    Write-Host "2. Open GitHub Copilot Chat (Ctrl+Shift+I)" -ForegroundColor White
    Write-Host "3. Look for 'Tavily Expert' in the MCP selector" -ForegroundColor White
} catch {
    Write-Host "❌ Failed to set environment variable: $_" -ForegroundColor Red
    exit 1
} finally {
    # Clear the plain text key from memory
    $apiKeyPlain = $null
    [System.GC]::Collect()
}
