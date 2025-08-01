# PowerShell wrapper function for run-with-error-capture.bat
# Adds this to BusBuddy's PowerShell toolkit while maintaining compatibility

function Invoke-BusBuddyRunWithErrorCapture {
    [CmdletBinding()]
    param (
        [switch]$ShowOutputLog,
        [switch]$ShowErrorLog,
        [switch]$SummaryOnly
    )

    Write-Host "Running BusBuddy with batch error capture..." -ForegroundColor Cyan

    # Run the batch file
    $batchPath = Join-Path $PSScriptRoot "run-with-error-capture.bat"
    $exitCode = cmd.exe /c "$batchPath" 2>&1

    # Get the latest log files
    $latestOutputLog = Get-ChildItem -Path "logs\app_output_*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    $latestErrorLog = Get-ChildItem -Path "logs\app_errors_*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 1

    if (-not $SummaryOnly) {
        Write-Host "`nResults:" -ForegroundColor Yellow
        Write-Host "  Output Log: $($latestOutputLog.FullName)" -ForegroundColor Gray
        Write-Host "  Error Log:  $($latestErrorLog.FullName)" -ForegroundColor Gray
        Write-Host "  Exit Code:  $exitCode" -ForegroundColor $(if ($exitCode -eq 0) { "Green" } else { "Red" })
    }

    # Show log content if requested
    if ($ShowOutputLog -and $latestOutputLog) {
        Write-Host "`n=== OUTPUT LOG ===" -ForegroundColor Yellow
        Get-Content $latestOutputLog.FullName
    }

    if ($ShowErrorLog -and $latestErrorLog) {
        Write-Host "`n=== ERROR LOG ===" -ForegroundColor Yellow
        Get-Content $latestErrorLog.FullName
    }

    # Return the exit code
    return [int]$exitCode
}

# Create alias
Set-Alias -Name bb-run-batch -Value Invoke-BusBuddyRunWithErrorCapture

# Export the function
Export-ModuleMember -Function Invoke-BusBuddyRunWithErrorCapture -Alias bb-run-batch
