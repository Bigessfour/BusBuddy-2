# Bus Buddy Admin Launcher
# Helper script to launch administrator tools with elevated privileges

param(
    [string]$AdminAction = "Status",
    [switch]$ForceElevation
)

function Test-Administrator {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Start-ElevatedSession {
    param([string]$ScriptPath, [string]$Arguments)

    Write-Host "🔐 LAUNCHING ELEVATED POWERSHELL SESSION" -ForegroundColor Yellow
    Write-Host "Script: $ScriptPath" -ForegroundColor Cyan
    Write-Host "Arguments: $Arguments" -ForegroundColor Cyan

    try {
        Start-Process -FilePath "pwsh.exe" -ArgumentList "-File `"$ScriptPath`" $Arguments" -Verb RunAs -Wait
        Write-Host "✓ Elevated session completed" -ForegroundColor Green
    } catch {
        Write-Error "Failed to start elevated session: $($_.Exception.Message)"

        # Fallback to Windows PowerShell if pwsh fails
        try {
            Write-Host "Attempting with Windows PowerShell..." -ForegroundColor Yellow
            Start-Process -FilePath "powershell.exe" -ArgumentList "-File `"$ScriptPath`" $Arguments" -Verb RunAs -Wait
            Write-Host "✓ Elevated session completed (Windows PowerShell)" -ForegroundColor Green
        } catch {
            Write-Error "Failed with both PowerShell versions: $($_.Exception.Message)"
        }
    }
}

# Main logic
$isAdmin = Test-Administrator

Write-Host "BUS BUDDY ADMIN LAUNCHER" -ForegroundColor Magenta
Write-Host "=" * 30 -ForegroundColor Cyan
Write-Host "Current Status: $($isAdmin ? 'Administrator' : 'Standard User')" -ForegroundColor ($isAdmin ? 'Green' : 'Yellow')

if ($isAdmin) {
    Write-Host "Already running as administrator. Executing directly..." -ForegroundColor Green
    & ".\Scripts\Admin-Tools.ps1" -Action $AdminAction
} else {
    Write-Host "Standard user detected. Available options:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "ADMIN ACTIONS AVAILABLE:" -ForegroundColor Cyan
    Write-Host "  Status      - Show admin status (safe)" -ForegroundColor White
    Write-Host "  Install     - Install admin-only tools" -ForegroundColor White
    Write-Host "  Configure   - Configure development environment" -ForegroundColor White
    Write-Host "  Services    - Install system services" -ForegroundColor White
    Write-Host "  Test        - Test admin privileges" -ForegroundColor White
    Write-Host "  All         - Run complete admin setup" -ForegroundColor White
    Write-Host ""

    if ($ForceElevation -or $AdminAction -ne "Status") {
        $scriptPath = Join-Path (Get-Location) "Scripts\Admin-Tools.ps1"
        $arguments = "-Action $AdminAction"
        Start-ElevatedSession -ScriptPath $scriptPath -Arguments $arguments
    } else {
        Write-Host "STANDARD USER ALTERNATIVES:" -ForegroundColor Green
        Write-Host "  Use: .\Scripts\Install-DeveloperTools.ps1" -ForegroundColor White
        Write-Host "  Use: .\Scripts\DevHub.ps1" -ForegroundColor White
        Write-Host "  Use: .\Scripts\PowerShell7-Integration.ps1" -ForegroundColor White
        Write-Host ""
        Write-Host "To run admin tools, use: -ForceElevation parameter" -ForegroundColor Yellow
        Write-Host "Example: .\Scripts\Launch-AdminTools.ps1 -AdminAction Install -ForceElevation" -ForegroundColor Cyan
    }
}

# Show current capabilities
Write-Host ""
Write-Host "CURRENT DEVELOPMENT CAPABILITIES:" -ForegroundColor Magenta
$tools = @{
    "PowerShell Version" = $PSVersionTable.PSVersion
    "PSScriptAnalyzer" = (Get-Module PSScriptAnalyzer -ListAvailable) ? "Available" : "Not Available"
    "Pester" = (Get-Module Pester -ListAvailable) ? "Available" : "Not Available"
    "Terminal-Icons" = (Get-Module Terminal-Icons -ListAvailable) ? "Available" : "Not Available"
    "Console GUI Tools" = (Get-Module Microsoft.PowerShell.ConsoleGuiTools -ListAvailable) ? "Available" : "Not Available"
    "dotnet-ef" = (Get-Command dotnet-ef -ErrorAction SilentlyContinue) ? "Available" : "Not Available"
    "Git" = (Get-Command git -ErrorAction SilentlyContinue) ? "Available" : "Not Available"
}

$tools.GetEnumerator() | ForEach-Object {
    $color = if ($_.Value -like "*Available*") { "Green" } else { "Red" }
    Write-Host "  $($_.Key): $($_.Value)" -ForegroundColor $color
}
