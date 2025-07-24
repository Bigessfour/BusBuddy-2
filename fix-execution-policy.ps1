#Requires -Version 7.6
<#
.SYNOPSIS
    Fix PowerShell execution policy for BusBuddy development

.DESCRIPTION
    This script configures the PowerShell execution policy to work properly with
    BusBuddy development environment following PowerShell 7.6 best practices.

    Based on Microsoft's guidance from:
    https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_execution_policies

.PARAMETER Scope
    The scope for execution policy change (Process, CurrentUser, LocalMachine)

.PARAMETER Policy
    The execution policy to set (Bypass, RemoteSigned, AllSigned)

.PARAMETER Persistent
    Whether to make the change persistent (affects CurrentUser or LocalMachine scope)

.EXAMPLE
    # Quick fix for current session (recommended for development)
    .\fix-execution-policy.ps1

.EXAMPLE
    # Make persistent change for current user
    .\fix-execution-policy.ps1 -Scope CurrentUser -Policy RemoteSigned -Persistent

.EXAMPLE
    # System-wide change (requires admin)
    .\fix-execution-policy.ps1 -Scope LocalMachine -Policy RemoteSigned -Persistent

.NOTES
    PowerShell 7.6 Enhanced Features Used:
    - Enhanced error handling with null coalescing
    - Improved security checks
    - Modern parameter validation
#>

[CmdletBinding()]
param(
    [Parameter()]
    [ValidateSet('Process', 'CurrentUser', 'LocalMachine')]
    [string]$Scope = 'Process',

    [Parameter()]
    [ValidateSet('Bypass', 'RemoteSigned', 'AllSigned')]
    [string]$Policy = 'Bypass',

    [Parameter()]
    [switch]$Persistent
)

Write-Host "🔧 BusBuddy Execution Policy Fix" -ForegroundColor Cyan
Write-Host "===============================" -ForegroundColor Cyan

# Check PowerShell version
if ($PSVersionTable.PSVersion.Major -lt 7) {
    Write-Warning "This script is designed for PowerShell 7.6+. Current version: $($PSVersionTable.PSVersion)"
    Write-Host "Consider upgrading to PowerShell 7.6 for best compatibility." -ForegroundColor Yellow
}

# Adjust scope based on Persistent parameter
if ($Persistent -and $Scope -eq 'Process') {
    Write-Host "⚠️ Persistent flag set but scope is Process. Changing scope to CurrentUser." -ForegroundColor Yellow
    $Scope = 'CurrentUser'
}

# Check admin privileges for LocalMachine scope
if ($Scope -eq 'LocalMachine') {
    try {
        $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
        $principal = [Security.Principal.WindowsPrincipal]$currentUser
        $isAdmin = $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

        if (-not $isAdmin) {
            Write-Warning "LocalMachine scope requires administrator privileges."
            Write-Host "Options:" -ForegroundColor Yellow
            Write-Host "1. Run this script as administrator" -ForegroundColor Green
            Write-Host "2. Use CurrentUser scope instead" -ForegroundColor Green
            Write-Host "3. Use Process scope for temporary fix" -ForegroundColor Green

            $choice = Read-Host "Choose an option (1/2/3) or press Enter to use CurrentUser scope"

            switch ($choice) {
                '1' {
                    Write-Host "Restarting as administrator..." -ForegroundColor Cyan
                    Start-Process pwsh.exe -ArgumentList "-File `"$PSCommandPath`" -Scope LocalMachine -Policy $Policy -Persistent:$Persistent" -Verb RunAs
                    exit
                }
                '2' { $Scope = 'CurrentUser' }
                '3' { $Scope = 'Process'; $Persistent = $false }
                default { $Scope = 'CurrentUser' }
            }
        }
    }
    catch {
        Write-Warning "Could not determine admin status: $($_.Exception.Message ?? 'Unknown error')"
        $Scope = 'CurrentUser'
    }
}

Write-Host "`n📋 Configuration Summary:" -ForegroundColor Yellow
Write-Host "  Scope: $Scope" -ForegroundColor Gray
Write-Host "  Policy: $Policy" -ForegroundColor Gray
Write-Host "  Persistent: $(if ($Persistent) { 'Yes' } else { 'No (Session only)' })" -ForegroundColor Gray

# Show current execution policy
Write-Host "`n📊 Current Execution Policy Status:" -ForegroundColor Yellow
try {
    $policies = Get-ExecutionPolicy -List
    $policies | ForEach-Object {
        $color = switch ($_.ExecutionPolicy) {
            'Bypass' { 'Green' }
            'RemoteSigned' { 'Cyan' }
            'AllSigned' { 'Blue' }
            'Restricted' { 'Red' }
            'Undefined' { 'Gray' }
            default { 'White' }
        }
        Write-Host "  $($_.Scope): $($_.ExecutionPolicy)" -ForegroundColor $color
    }
}
catch {
    Write-Warning "Could not retrieve current execution policy: $($_.Exception.Message ?? 'Unknown error')"
}

# Apply the execution policy change
Write-Host "`n🔧 Applying Execution Policy Change..." -ForegroundColor Cyan

try {
    Set-ExecutionPolicy -ExecutionPolicy $Policy -Scope $Scope -Force
    Write-Host "✅ Execution policy successfully set to $Policy for $Scope scope" -ForegroundColor Green

    # Verify the change
    $newPolicy = Get-ExecutionPolicy -Scope $Scope
    if ($newPolicy -eq $Policy) {
        Write-Host "✅ Change verified successfully" -ForegroundColor Green
    } else {
        Write-Warning "Verification failed. Expected: $Policy, Actual: $newPolicy"
    }
}
catch {
    Write-Error "Failed to set execution policy: $($_.Exception.Message ?? 'Unknown error')"
    exit 1
}

# Show effectiveness
Write-Host "`n📈 New Effective Policy:" -ForegroundColor Yellow
try {
    $effectivePolicy = Get-ExecutionPolicy
    Write-Host "  Effective Policy: $effectivePolicy" -ForegroundColor Green
}
catch {
    Write-Warning "Could not determine effective policy: $($_.Exception.Message ?? 'Unknown error')"
}

# Test the BusBuddy compatibility wrapper
Write-Host "`n🧪 Testing BusBuddy Compatibility Wrapper..." -ForegroundColor Cyan
$wrapperPath = "$PSScriptRoot\PowerShell-Compatibility-Wrapper.psm1"

if (Test-Path $wrapperPath) {
    try {
        # Unblock the file
        if (Get-Command Unblock-File -ErrorAction SilentlyContinue) {
            Unblock-File -Path $wrapperPath -ErrorAction SilentlyContinue
            Write-Host "✅ Compatibility wrapper unblocked" -ForegroundColor Green
        }

        # Test import
        Import-Module $wrapperPath -Force -ErrorAction Stop
        Write-Host "✅ Compatibility wrapper imported successfully" -ForegroundColor Green

        # Clean up test import
        Remove-Module (Split-Path $wrapperPath -LeafBase) -ErrorAction SilentlyContinue
    }
    catch {
        Write-Warning "Compatibility wrapper test failed: $($_.Exception.Message ?? 'Unknown error')"
        Write-Host "This may indicate the execution policy change didn't fully resolve the issue." -ForegroundColor Yellow
    }
} else {
    Write-Host "⚠️ Compatibility wrapper not found at: $wrapperPath" -ForegroundColor Yellow
}

# Provide guidance
Write-Host "`n💡 Next Steps:" -ForegroundColor Yellow

if ($Scope -eq 'Process') {
    Write-Host "  • This change is temporary and will reset when you close PowerShell" -ForegroundColor Gray
    Write-Host "  • To make permanent, run with -Persistent flag and CurrentUser scope" -ForegroundColor Gray
}

Write-Host "  • Test BusBuddy profile loading:" -ForegroundColor Gray
Write-Host "    . `"$PSScriptRoot\persistent-profile-helper.ps1`"" -ForegroundColor Cyan

Write-Host "  • For more info on execution policies:" -ForegroundColor Gray
Write-Host "    Get-Help about_Execution_Policies" -ForegroundColor Cyan

Write-Host "`n🎉 Execution policy fix completed!" -ForegroundColor Green
