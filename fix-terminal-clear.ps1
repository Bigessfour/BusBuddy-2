#Requires -Version 7.0
<#
.SYNOPSIS
    Fix Terminal Clear Functionality with PowerShell Profile Integration

.DESCRIPTION
    This script fixes the VS Code Terminal "Clear" action functionality when using custom PowerShell
    profiles. It adds proper Clear-Host function handling to ensure Terminal actions work correctly.

.NOTES
    Version: 1.0
    Date: July 19, 2025
    Author: Bus Buddy Team

.EXAMPLE
    # Run the script to fix terminal clear functionality
    .\fix-terminal-clear.ps1
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [switch]$Force
)

$ErrorActionPreference = 'Stop'

Write-Host "🔄 Fixing VS Code Terminal Clear functionality..." -ForegroundColor Cyan

# Paths to the profile scripts
$projectRoot = $PSScriptRoot
$mainProfilePath = Join-Path $projectRoot "BusBuddy-PowerShell-Profile.ps1"
$advancedWorkflowsPath = Join-Path $projectRoot ".vscode\BusBuddy-Advanced-Workflows.ps1"
$loadDirectPath = Join-Path $projectRoot "load-direct.ps1"
$loadBusBuddyProfilesPath = Join-Path $projectRoot "load-bus-buddy-profiles.ps1"

# Check if profile files exist
$mainProfileExists = Test-Path $mainProfilePath
$advancedWorkflowsExists = Test-Path $advancedWorkflowsPath
$loadDirectExists = Test-Path $loadDirectPath
$loadBusBuddyProfilesExists = Test-Path $loadBusBuddyProfilesPath

if (-not $mainProfileExists) {
    Write-Host "❌ Main profile not found: $mainProfilePath" -ForegroundColor Red
    return
}

# Functions to add to profiles for fixing Clear-Host functionality
$clearHostFunctionCode = @'

# Enable proper Clear-Host handling for VS Code terminal actions
function global:Clear-Host {
    if ($host.Name -eq 'Visual Studio Code Host') {
        # Use ANSI escape sequence for VS Code terminal
        $ESC = [char]0x1b
        Write-Host "$ESC[2J$ESC[0;0H" -NoNewline
    }
    else {
        # Fall back to default implementation for other hosts
        $originalClearHost = Get-Alias -Name cls -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Definition
        if ($originalClearHost) {
            & $originalClearHost
        }
        else {
            # Ultimate fallback
            [Console]::Clear()
        }
    }
}

# Ensure Clear-Host is properly aliased
Set-Alias -Name cls -Value Clear-Host -Force -Scope Global
Set-Alias -Name clear -Value Clear-Host -Force -Scope Global

'@

# Function to check if the Clear-Host function is already in a file
function Test-ClearHostFunctionExists {
    param (
        [string]$FilePath
    )

    if (-not (Test-Path $FilePath)) {
        return $false
    }

    $content = Get-Content -Path $FilePath -Raw
    return $content -match "function\s+global:Clear-Host" -or $content -match "Set-Alias\s+-Name\s+cls\s+-Value\s+Clear-Host"
}

# Function to add the Clear-Host function to a file
function Add-ClearHostFunction {
    param (
        [string]$FilePath,
        [string]$FunctionCode
    )

    if (-not (Test-Path $FilePath)) {
        Write-Host "❌ File not found: $FilePath" -ForegroundColor Red
        return $false
    }

    if (Test-ClearHostFunctionExists -FilePath $FilePath) {
        if (-not $Force) {
            Write-Host "✅ Clear-Host function already exists in: $FilePath" -ForegroundColor Green
            return $false
        }
    }

    try {
        $content = Get-Content -Path $FilePath -Raw

        # Find the appropriate insertion point (after comments and param block)
        if ($content -match "param\s*\([\s\S]*?\)\s*\r?\n") {
            $insertionPoint = $matches[0].Length + $matches.Index
            $newContent = $content.Substring(0, $insertionPoint) + "`n$FunctionCode`n" + $content.Substring($insertionPoint)
        } else {
            # If no param block, add after any initial comments
            if ($content -match "^<#[\s\S]*?#>\s*\r?\n") {
                $insertionPoint = $matches[0].Length + $matches.Index
                $newContent = $content.Substring(0, $insertionPoint) + "`n$FunctionCode`n" + $content.Substring($insertionPoint)
            } else {
                # Just add to the beginning with a blank line
                $newContent = "$FunctionCode`n`n$content"
            }
        }

        $newContent | Set-Content -Path $FilePath -Force
        Write-Host "✅ Added Clear-Host function to: $FilePath" -ForegroundColor Green
        return $true
    } catch {
        Write-Host "❌ Failed to update $FilePath`: $_" -ForegroundColor Red
        return $false
    }
}

# Add the Clear-Host function to the appropriate profile files
$modifiedFiles = @()

# Add to the main profile
if ($mainProfileExists) {
    $mainProfileModified = Add-ClearHostFunction -FilePath $mainProfilePath -FunctionCode $clearHostFunctionCode
    if ($mainProfileModified) {
        $modifiedFiles += "BusBuddy-PowerShell-Profile.ps1"
    }
}

# Create a VS Code settings update function
function Update-VSCodeSettings {
    $settingsPath = Join-Path $projectRoot ".vscode\settings.json"

    if (-not (Test-Path $settingsPath)) {
        Write-Host "⚠️ VS Code settings not found: $settingsPath" -ForegroundColor Yellow
        return $false
    }

    try {
        $settings = Get-Content -Path $settingsPath -Raw | ConvertFrom-Json

        # Check if we need to modify any terminal profile settings
        if ($settings.PSObject.Properties["terminal.integrated.profiles.windows"] -and
            $settings.PSObject.Properties["terminal.integrated.profiles.windows"].Value.PSObject.Properties["PowerShell 7.5.2"]) {

            $profile = $settings."terminal.integrated.profiles.windows"."PowerShell 7.5.2"

            # Add the Clear-Host initialization to the command if not already there
            if ($profile.args -and $profile.args -contains "-Command") {
                $commandIndex = [array]::IndexOf($profile.args, "-Command")
                if ($commandIndex -ge 0 -and $commandIndex -lt $profile.args.Count - 1) {
                    $command = $profile.args[$commandIndex + 1]

                    # Only modify if it doesn't already have Clear-Host handling
                    if ($command -notmatch "Clear-Host") {
                        # Create a new array since PowerShell handles JSON conversion better this way
                        $newArgs = @()
                        for ($i = 0; $i -lt $profile.args.Count; $i++) {
                            if ($i -eq $commandIndex + 1) {
                                # Keep the original command and add our initialization
                                $newArgs += "$command; `$function:global:Clear-Host = { `$ESC = [char]0x1b; Write-Host `"`$ESC[2J`$ESC[0;0H`" -NoNewline }"
                            } else {
                                $newArgs += $profile.args[$i]
                            }
                        }

                        # Update the args array
                        $settings."terminal.integrated.profiles.windows"."PowerShell 7.5.2".args = $newArgs

                        # Convert back to JSON and save
                        $settings | ConvertTo-Json -Depth 10 | Set-Content -Path $settingsPath -Force
                        Write-Host "✅ Updated VS Code terminal profile settings for Clear-Host support" -ForegroundColor Green
                        return $true
                    }
                }
            }
        }

        Write-Host "ℹ️ No changes needed for VS Code settings" -ForegroundColor Gray
        return $false
    } catch {
        Write-Host "❌ Failed to update VS Code settings: $_" -ForegroundColor Red
        return $false
    }
}

# Update VS Code settings if needed
$settingsUpdated = Update-VSCodeSettings

# Create verification file
$verifyPath = Join-Path $projectRoot "verify-terminal-clear.ps1"
$verifyContent = @'
#Requires -Version 7.0
<#
.SYNOPSIS
    Verify Terminal Clear Functionality

.DESCRIPTION
    This script verifies that the Terminal Clear functionality works correctly
    with the Bus Buddy PowerShell profile integration.

.NOTES
    Version: 1.0
    Date: July 19, 2025
    Author: Bus Buddy Team

.EXAMPLE
    # Verify terminal clear functionality
    .\verify-terminal-clear.ps1
#>

Write-Host "🔍 Verifying Terminal Clear functionality..." -ForegroundColor Cyan

# Test if Clear-Host function is available
$clearHostExists = $null -ne (Get-Command -Name Clear-Host -ErrorAction SilentlyContinue)
if ($clearHostExists) {
    Write-Host "✅ Clear-Host function is available" -ForegroundColor Green

    # Check if it's properly aliased
    $clsAlias = Get-Alias -Name cls -ErrorAction SilentlyContinue
    $clearAlias = Get-Alias -Name clear -ErrorAction SilentlyContinue

    if ($clsAlias -and $clsAlias.Definition -eq "Clear-Host") {
        Write-Host "✅ cls alias is properly configured" -ForegroundColor Green
    } else {
        Write-Host "❌ cls alias is not properly configured" -ForegroundColor Red
    }

    if ($clearAlias -and $clearAlias.Definition -eq "Clear-Host") {
        Write-Host "✅ clear alias is properly configured" -ForegroundColor Green
    } else {
        Write-Host "❌ clear alias is not properly configured" -ForegroundColor Red
    }

    # Show function definition
    Write-Host "`n📋 Current Clear-Host function definition:" -ForegroundColor Cyan
    ${function:Clear-Host}

    Write-Host "`n💡 Testing Clear-Host functionality..." -ForegroundColor Yellow
    Write-Host "This should clear the terminal when executed:" -ForegroundColor Gray
    Write-Host "  Clear-Host" -ForegroundColor White

    Write-Host "`nYou can also test the aliases:" -ForegroundColor Gray
    Write-Host "  cls" -ForegroundColor White
    Write-Host "  clear" -ForegroundColor White

    Write-Host "`n📌 VS Code Terminal Actions:" -ForegroundColor Cyan
    Write-Host "1. Click the trash can icon in the terminal toolbar" -ForegroundColor Gray
    Write-Host "2. Press Ctrl+K in the terminal" -ForegroundColor Gray
    Write-Host "3. Right-click in the terminal and select 'Clear'" -ForegroundColor Gray

    Write-Host "`n✅ Verification complete! Try the clear methods above." -ForegroundColor Green
} else {
    Write-Host "❌ Clear-Host function is not available" -ForegroundColor Red
    Write-Host "💡 Run the fix-terminal-clear.ps1 script and restart your terminal" -ForegroundColor Yellow
}
'@

Set-Content -Path $verifyPath -Value $verifyContent -Force
Write-Host "📝 Created verification script: verify-terminal-clear.ps1" -ForegroundColor Green

# Summary
Write-Host "`n📋 Terminal Clear Fix Summary:" -ForegroundColor Cyan

if ($modifiedFiles.Count -gt 0) {
    Write-Host "✅ Modified files:" -ForegroundColor Green
    foreach ($file in $modifiedFiles) {
        Write-Host "  • $file" -ForegroundColor Green
    }
} else {
    Write-Host "ℹ️ No profile files needed modification" -ForegroundColor Gray
}

if ($settingsUpdated) {
    Write-Host "✅ Updated VS Code settings for terminal integration" -ForegroundColor Green
}

Write-Host "`n💡 Next Steps:" -ForegroundColor Yellow
Write-Host "1. Restart VS Code or open a new terminal" -ForegroundColor Gray
Write-Host "2. Run the verification script:" -ForegroundColor Gray
Write-Host "   .\verify-terminal-clear.ps1" -ForegroundColor White
Write-Host "3. Try using Terminal > Clear command or clicking the trash icon" -ForegroundColor Gray
Write-Host "   in the terminal toolbar to test the fix" -ForegroundColor Gray

Write-Host "`n✅ Terminal Clear Fix completed!" -ForegroundColor Green
