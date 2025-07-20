#Requires -Version 7.0
<#
.SYNOPSIS
    Bus Buddy PowerShell Function Build Task
.DESCRIPTION
    Uses the bb-build function from BusBuddy-Advanced-Workflows.ps1
#>

$ErrorActionPreference = 'Continue'

try {
    Write-Host '🚌 Starting Bus Buddy PowerShell Build...' -ForegroundColor Cyan
    Write-Host "Working Directory: $PWD" -ForegroundColor Gray

    # Load the advanced workflows
    $workflowsPath = Join-Path $PWD 'BusBuddy-Advanced-Workflows.ps1'
    if (Test-Path $workflowsPath) {
        Write-Host "📜 Loading Bus Buddy Advanced Workflows..." -ForegroundColor Yellow
        . $workflowsPath

        Write-Host "🔨 Executing bb-build function..." -ForegroundColor Yellow
        $result = bb-build

        if ($result) {
            Write-Host "`n✅ bb-build completed successfully!" -ForegroundColor Green
            $exitCode = 0
        } else {
            Write-Host "`n❌ bb-build failed!" -ForegroundColor Red
            $exitCode = 1
        }
    } else {
        Write-Host "❌ BusBuddy-Advanced-Workflows.ps1 not found at: $workflowsPath" -ForegroundColor Red
        Write-Host "📁 Current directory contents:" -ForegroundColor Yellow
        Get-ChildItem | Select-Object Name, Mode | Format-Table -AutoSize
        $exitCode = 1
    }

    Write-Host "`n📊 PowerShell build task finished. Press any key to close terminal..." -ForegroundColor White
    Read-Host

    exit $exitCode

} catch {
    Write-Host "💥 PowerShell build task crashed: $_" -ForegroundColor Red
    Write-Host "Exception Type: $($_.Exception.GetType().FullName)" -ForegroundColor Red
    Read-Host "Press Enter to continue..."
    exit 1
}
