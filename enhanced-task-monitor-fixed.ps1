#Requires -Version 7.5

<#
.SYNOPSIS
    Enhanced Task Monitor for BusBuddy development environment
.DESCRIPTION
    Provides enhanced monitoring of build, run, and diagnostic tasks
    with real-time feedback and error capture.
.PARAMETER TaskName
    Name of the task to monitor
.PARAMETER Command
    Command to execute
.PARAMETER Arguments
    Arguments for the command
.PARAMETER WaitForCompletion
    Wait for the task to complete
.PARAMETER CaptureOutput
    Capture and analyze command output
.PARAMETER ShowRealTime
    Display output in real-time
.EXAMPLE
    .\enhanced-task-monitor-fixed.ps1 -TaskName "Build" -Command "dotnet" -Arguments "build","BusBuddy.sln"
.EXAMPLE
    & "enhanced-task-monitor-fixed.ps1"; Start-BusBuddyTask -TaskType Build
#>

[CmdletBinding()]
param (
    [string]$TaskName = "",
    [string]$Command = "",
    [string[]]$Arguments = @(),
    [switch]$WaitForCompletion,
    [switch]$CaptureOutput,
    [switch]$ShowRealTime
)

# Helper functions for task monitoring
function Start-BusBuddyTask {
    [CmdletBinding()]
    param (
        [ValidateSet('Build', 'Run', 'Health')]
        [string]$TaskType = 'Build'
    )

    switch ($TaskType) {
        'Build' {
            & $PSCommandPath -TaskName "Build BusBuddy" -Command "dotnet" -Arguments "build","BusBuddy.sln" -WaitForCompletion -CaptureOutput -ShowRealTime
        }
        'Run' {
            & $PSCommandPath -TaskName "Run BusBuddy" -Command "dotnet" -Arguments "run","--project","BusBuddy.WPF/BusBuddy.WPF.csproj" -WaitForCompletion -CaptureOutput -ShowRealTime
        }
        'Health' {
            & $PSCommandPath -TaskName "Health Check" -Command "pwsh" -Arguments "-ExecutionPolicy","Bypass","-File","run-with-exception-capture.ps1" -WaitForCompletion -CaptureOutput -ShowRealTime
        }
    }
}

# Check for dependency issues
function Test-DependencyIssues {
    param (
        [string[]]$Output
    )

    $issues = @()

    # Check for Serilog version mismatch
    if ($Output -match 'System.IO.FileNotFoundException.*Serilog') {
        $issues += @{
            Type = "Serilog Version Mismatch"
            Description = "Serilog assembly version mismatch detected"
            Fix = "Add binding redirect in BusBuddy.WPF.exe.config to redirect all Serilog versions to 4.3.0"
            Command = "Create 'BusBuddy.WPF\bin\Debug\net9.0-windows\BusBuddy.WPF.exe.config' with proper binding redirects"
        }
    }

    # Check for missing dependencies
    if ($Output -match 'Could not load file or assembly') {
        $issues += @{
            Type = "Missing Assembly"
            Description = "A required assembly could not be loaded"
            Fix = "Restore NuGet packages and check for missing references"
            Command = "dotnet restore --force"
        }
    }

    # Check for EF Core issues
    if ($Output -match 'Microsoft\.EntityFrameworkCore') {
        $issues += @{
            Type = "Entity Framework Issue"
            Description = "Entity Framework Core error detected"
            Fix = "Verify database connection and EF Core configuration"
            Command = "Check connection strings and migrations"
        }
    }

    return $issues
}

# Process output and check for common issues
function Process-TaskOutput {
    param (
        [string[]]$Output,
        [string]$LogFile
    )

    $errorCount = 0
    $warningCount = 0
    $issues = @()

    foreach ($line in $Output) {
        if ([string]::IsNullOrWhiteSpace($line)) { continue }

        if ($line -match 'error|exception|fail' -and $line -notmatch 'warning') {
            $errorCount++
            Write-Host $line -ForegroundColor Red
        }
        elseif ($line -match 'warning') {
            $warningCount++
            Write-Host $line -ForegroundColor Yellow
        }
        else {
            Write-Host $line
        }

        Add-Content -Path $LogFile -Value $line
    }

    $issues = Test-DependencyIssues -Output $Output

    return @{
        ErrorCount = $errorCount
        WarningCount = $warningCount
        Issues = $issues
    }
}

# Main task execution
if ($Command -and $TaskName) {
    $logDir = Join-Path $PSScriptRoot "logs"
    if (-not (Test-Path $logDir)) {
        New-Item -Path $logDir -ItemType Directory -Force | Out-Null
    }

    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $logFile = Join-Path $logDir "task-$($TaskName.Replace(' ', '-'))-$timestamp.log"

    Write-Host "Starting task: $TaskName" -ForegroundColor Cyan
    Write-Host "Command: $Command $($Arguments -join ' ')" -ForegroundColor Cyan
    Write-Host "Log file: $logFile" -ForegroundColor Cyan
    Write-Host "-------------------------------------------------" -ForegroundColor Yellow

    $startTime = Get-Date

    if ($WaitForCompletion -and $CaptureOutput) {
        $output = & $Command $Arguments 2>&1
        $exitCode = $LASTEXITCODE

        $results = Process-TaskOutput -Output $output -LogFile $logFile

        $endTime = Get-Date
        $duration = $endTime - $startTime

        Write-Host "-------------------------------------------------" -ForegroundColor Yellow
        Write-Host "Task completed in $($duration.TotalSeconds.ToString("0.00")) seconds" -ForegroundColor Cyan
        Write-Host "Errors: $($results.ErrorCount)" -ForegroundColor ($results.ErrorCount -gt 0 ? "Red" : "Green")
        Write-Host "Warnings: $($results.WarningCount)" -ForegroundColor ($results.WarningCount -gt 0 ? "Yellow" : "Green")

        if ($results.Issues.Count -gt 0) {
            Write-Host "Detected Issues:" -ForegroundColor Yellow
            foreach ($issue in $results.Issues) {
                Write-Host "  - $($issue.Type): $($issue.Description)" -ForegroundColor Yellow
                Write-Host "    Fix: $($issue.Fix)" -ForegroundColor Green
                Write-Host "    Command: $($issue.Command)" -ForegroundColor Cyan
            }
        }

        return @{
            ExitCode = $exitCode
            Duration = $duration
            ErrorCount = $results.ErrorCount
            WarningCount = $results.WarningCount
            LogFile = $logFile
            Issues = $results.Issues
        }
    }
    elseif ($ShowRealTime) {
        # Real-time output processing
        $process = Start-Process -FilePath $Command -ArgumentList $Arguments -NoNewWindow -PassThru -RedirectStandardOutput "$env:TEMP\stdout.tmp" -RedirectStandardError "$env:TEMP\stderr.tmp"

        while (-not $process.HasExited) {
            if (Test-Path "$env:TEMP\stdout.tmp") {
                Get-Content "$env:TEMP\stdout.tmp" -Tail 1 | ForEach-Object {
                    if ($_) {
                        Write-Host $_
                        Add-Content -Path $logFile -Value $_
                    }
                }
            }
            if (Test-Path "$env:TEMP\stderr.tmp") {
                Get-Content "$env:TEMP\stderr.tmp" -Tail 1 | ForEach-Object {
                    if ($_) {
                        Write-Host $_ -ForegroundColor Red
                        Add-Content -Path $logFile -Value $_
                    }
                }
            }
            Start-Sleep -Milliseconds 100
        }

        # Clean up temp files
        Remove-Item "$env:TEMP\stdout.tmp" -ErrorAction SilentlyContinue
        Remove-Item "$env:TEMP\stderr.tmp" -ErrorAction SilentlyContinue

        $endTime = Get-Date
        $duration = $endTime - $startTime

        Write-Host "-------------------------------------------------" -ForegroundColor Yellow
        Write-Host "Task completed in $($duration.TotalSeconds.ToString("0.00")) seconds" -ForegroundColor Cyan
        Write-Host "Exit code: $($process.ExitCode)" -ForegroundColor ($process.ExitCode -eq 0 ? "Green" : "Red")

        return @{
            ExitCode = $process.ExitCode
            Duration = $duration
            LogFile = $logFile
        }
    }
    else {
        # Simple execution without capturing or waiting
        Start-Process -FilePath $Command -ArgumentList $Arguments -NoNewWindow

        Write-Host "Task started (not waiting for completion)" -ForegroundColor Cyan
        return @{
            Started = $true
            LogFile = $logFile
        }
    }
}
elseif ($PSCmdlet.MyInvocation.BoundParameters.Count -eq 0) {
    # When run with no parameters, export the Start-BusBuddyTask function
    Export-ModuleMember -Function Start-BusBuddyTask
    return
}
else {
    Write-Host "Please provide both TaskName and Command parameters" -ForegroundColor Red
    return
}
