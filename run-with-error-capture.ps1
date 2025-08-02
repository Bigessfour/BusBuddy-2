# PowerShell wrapper function for run-with-error-capture.bat
# Adds this to BusBuddy's PowerShell toolkit while maintaining compatibility

# Prevent Load Loops (For Repeated Runs)
if ($env:BusBuddyRunWithErrorCaptureLoaded -eq 'true') {
    Write-Verbose "⚠️ Script already loaded - skipping repeated initialization"
    return
}
$env:BusBuddyRunWithErrorCaptureLoaded = 'true'

function Invoke-BusBuddyRunWithErrorCapture {
    [CmdletBinding()]
    param (
        [switch]$ShowOutputLog,
        [switch]$ShowErrorLog,
        [switch]$SummaryOnly
    )

    Write-Information "Running BusBuddy with PowerShell error capture..." -InformationAction Continue

    # Create logs directory if it doesn't exist
    $logsDir = Join-Path $PSScriptRoot "logs"
    if (-not (Test-Path -Path $logsDir)) {
        New-Item -Path $logsDir -ItemType Directory -Force | Out-Null
    }

    # Generate timestamp for log files
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $outputLogPath = Join-Path $logsDir "app_output_$timestamp.log"
    $errorLogPath = Join-Path $logsDir "app_errors_$timestamp.log"

    # Run the application with output redirection
    try {
        Write-Information "Starting application..." -InformationAction Continue

        # Run using recommended pattern from guidelines
        $process = Start-Process -FilePath "dotnet" -ArgumentList "run", "--project", "BusBuddy.csproj" `
            -RedirectStandardOutput $outputLogPath -RedirectStandardError $errorLogPath `
            -NoNewWindow -PassThru -Wait

        $exitCode = $process.ExitCode
    }
    catch {
        Write-Error "Error starting BusBuddy application: $_"
        $errorMessage = $_.Exception.Message
        $errorMessage | Out-File -FilePath $errorLogPath -Append
        $exitCode = 1
    }

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

# Safe Alias Creation (Avoid Duplicates - Doc: https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.utility/get-alias)
$aliases = @{
    "bb-run-batch" = "Invoke-BusBuddyRunWithErrorCapture"
}

foreach ($alias in $aliases.Keys) {
    if (-not (Get-Alias $alias -ErrorAction SilentlyContinue)) {
        New-Alias -Name $alias -Value $aliases[$alias] -Description "BusBuddy alias"
        Write-Verbose "✅ Created alias: $alias -> $($aliases[$alias])"
    } else {
        Write-Verbose "⚠️ Alias $alias already exists - skipped creation"
    }
}

# Export the function and alias
Export-ModuleMember -Function Invoke-BusBuddyRunWithErrorCapture -Alias bb-run-batch
