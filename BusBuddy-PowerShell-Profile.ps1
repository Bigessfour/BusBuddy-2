# BusBuddy PowerShell Profile
# Loads environment variables, sets up aliases, and prepares session for BusBuddy admin/dev workflows

# Set location to workspace root
Set-Location "$PSScriptRoot"

# Import compatibility wrapper if present using PowerShell 7.6 best practices
if (Test-Path "$PSScriptRoot\PowerShell-Compatibility-Wrapper.psm1") {
    try {
        # Use Unblock-File to handle execution policy for local development modules
        if (Get-Command Unblock-File -ErrorAction SilentlyContinue) {
            Unblock-File -Path "$PSScriptRoot\PowerShell-Compatibility-Wrapper.psm1" -ErrorAction SilentlyContinue
        }

        # Import with bypass for local development modules
        Import-Module "$PSScriptRoot\PowerShell-Compatibility-Wrapper.psm1" -Force -Global -ErrorAction Stop
        Write-Host "   ✅ PowerShell Compatibility Wrapper loaded successfully" -ForegroundColor Green
    }
    catch [System.Management.Automation.PSSecurityException] {
        # Handle execution policy issues specifically
        Write-Host "   ⚠️ PowerShell Compatibility Wrapper requires execution policy adjustment" -ForegroundColor Yellow
        Write-Host "   💡 For development, consider: Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass" -ForegroundColor Cyan
        Write-Host "   ℹ️ Continuing without compatibility wrapper - basic functionality available" -ForegroundColor Gray
    }
    catch {
        Write-Warning "PowerShell Compatibility Wrapper could not be loaded: $($_.Exception.Message)"
        Write-Host "   ℹ️ This is not critical - basic functionality will work without it" -ForegroundColor Gray
    }
}

# Set up common aliases
Set-Alias -Name 'bb-admin' -Value 'Start-AdminOptimizationSession' -Scope Global -Force
Set-Alias -Name 'bb-ai' -Value 'Start-AIAssistantIntegration' -Scope Global -Force
Set-Alias -Name 'bb-health' -Value 'Get-BusBuddySystemStatus' -Scope Global -Force

# Load environment variables from .env.template if present
if (Test-Path "$PSScriptRoot\.env.template") {
    Get-Content "$PSScriptRoot\.env.template" | ForEach-Object {
        if ($_ -match "^(\w+)=(.+)$") {
            $name, $value = $matches[1], $matches[2]
            [System.Environment]::SetEnvironmentVariable($name, $value, 'Process')
        }
    }
}

# Display profile loaded message
Write-Host "✅ BusBuddy PowerShell profile loaded." -ForegroundColor Green
