
<##
.SYNOPSIS
    Fixes Bus Buddy build environment, loads advanced workflows, and ensures build command is available.

.DESCRIPTION
    Loads the Bus Buddy advanced workflows, validates the build function, and sets up the bb-build alias. Supports robust logging, exit codes, parameter validation, and JSON output for CI/CD.

.PARAMETER LogFile
    Path to a log file for output. If not specified, logs only to console.

.PARAMETER Production
    Enables production mode: stricter error handling, disables interactive prompts.

.PARAMETER OutputFormat
    Output format: 'Text' (default) or 'Json'.

.EXAMPLE
    . ./fix-build.ps1 -LogFile './logs/fix-build.log' -OutputFormat Json

.EXAMPLE
    . ./fix-build.ps1 -Production

.NOTES
    Exits with code 0 on success, 1 on error. Designed for use in CI/CD and production environments.
##>

[CmdletBinding()]
param(
    [string]$LogFile,
    [switch]$Production,
    [ValidateSet('Text', 'Json')][string]$OutputFormat = 'Text'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = if ($Production) { 'Stop' } else { 'Continue' }

#region Logging
function Write-Log {
    param(
        [string]$Message,
        [string]$Level = 'INFO',
        [switch]$ToConsole = $true
    )
    $timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
    $logLine = "[$timestamp][$Level] $Message"
    if ($ToConsole) { Write-Host $logLine }
    if ($LogFile) { Add-Content -Path $LogFile -Value $logLine }
}
#endregion

#region Result Object
$result = [ordered]@{
    Success = $false
    Errors  = @()
    Steps   = @()
}
#endregion

#region Paths
$currentDir = Get-Location
$advancedWorkflowsPath = Join-Path $currentDir ".vscode\BusBuddy-Advanced-Workflows.ps1"
$solutionFile = Join-Path $currentDir "BusBuddy.sln"
#endregion

Write-Log "🚌 Bus Buddy Build Environment Fix" 'INFO'
Write-Log "---------------------------------------" 'INFO'

if (Test-Path $advancedWorkflowsPath) {
    Write-Log "✓ Found advanced workflows script" 'INFO'
    try {
        Write-Log "  Loading advanced workflows..." 'INFO'
        . $advancedWorkflowsPath
        Write-Log "✓ Successfully loaded advanced workflows" 'INFO'
        $result.Steps += "Loaded advanced workflows"
    } catch {
        $msg = "❌ Error loading advanced workflows: $($_.Exception.Message)"
        Write-Log $msg 'ERROR'
        $result.Errors += $msg
        if ($OutputFormat -eq 'Json') { $result | ConvertTo-Json -Depth 5 | Write-Output }
        exit 1
    }

    if (Test-Path $solutionFile) {
        Write-Log "✓ Found solution file at: $solutionFile" 'INFO'
        $result.Steps += "Found solution file"
    } else {
        $msg = "❌ Solution file not found at: $solutionFile"
        Write-Log $msg 'ERROR'
        $result.Errors += $msg
        if ($OutputFormat -eq 'Json') { $result | ConvertTo-Json -Depth 5 | Write-Output }
        exit 1
    }

    if (Get-Command -Name Invoke-BusBuddyBuild -ErrorAction SilentlyContinue) {
        Write-Log "✓ Found Invoke-BusBuddyBuild function" 'INFO'
        Set-Alias -Name bb-build -Value Invoke-BusBuddyBuild -Force -Scope Global
        Write-Log "✓ Created global bb-build alias" 'INFO'
        $result.Steps += "Set bb-build alias"
    } else {
        Write-Log "❌ Invoke-BusBuddyBuild function not found" 'ERROR'
        Write-Log "  Checking for script contents..." 'INFO'
        $buildFunctionExists = Get-Content $advancedWorkflowsPath | Select-String -Pattern "function Invoke-BusBuddyBuild"
        if ($buildFunctionExists) {
            Write-Log "✓ Found function definition in script file" 'INFO'
            Write-Log "  Likely a scope issue - attempting direct import" 'WARN'
            $tempScriptBlock = [ScriptBlock]::Create((Get-Content $advancedWorkflowsPath -Raw))
            . $tempScriptBlock
            if (Get-Command -Name Invoke-BusBuddyBuild -ErrorAction SilentlyContinue) {
                Write-Log "✓ Successfully loaded build function" 'INFO'
                Set-Alias -Name bb-build -Value Invoke-BusBuddyBuild -Force -Scope Global
                Write-Log "✓ Created global bb-build alias" 'INFO'
                $result.Steps += "Set bb-build alias after import"
            } else {
                $msg = "❌ Still could not load build function"
                Write-Log $msg 'ERROR'
                $result.Errors += $msg
                if ($OutputFormat -eq 'Json') { $result | ConvertTo-Json -Depth 5 | Write-Output }
                exit 1
            }
        } else {
            $msg = "❌ Build function definition not found in script file"
            Write-Log $msg 'ERROR'
            $result.Errors += $msg
            if ($OutputFormat -eq 'Json') { $result | ConvertTo-Json -Depth 5 | Write-Output }
            exit 1
        }
    }

    $result.Success = $true
    Write-Log "🚌 Bus Buddy commands are now available:" 'INFO'
    Write-Log "  • bb-build - Build the solution" 'INFO'
    Write-Log "  • bb-clean - Clean build artifacts" 'INFO'
    Write-Log "  • bb-run - Run the application" 'INFO'
    Write-Log "  • bb-test - Run tests" 'INFO'
    Write-Log "  • bb-health - Check system health" 'INFO'
    Write-Log "To run a build now, type: bb-build" 'INFO'
    if ($OutputFormat -eq 'Json') { $result | ConvertTo-Json -Depth 5 | Write-Output }
    exit 0
} else {
    $msg = "❌ Advanced workflows script not found at: $advancedWorkflowsPath"
    Write-Log $msg 'ERROR'
    Write-Log "  Checking for the file in current directory..." 'INFO'
    $files = Get-ChildItem -Path $currentDir -Recurse -Filter "BusBuddy-Advanced-Workflows.ps1" -ErrorAction SilentlyContinue
    if ($files.Count -gt 0) {
        Write-Log "✓ Found workflow script at: $($files[0].FullName)" 'INFO'
        Write-Log "  Please use this path instead" 'WARN'
    } else {
        Write-Log "❌ Could not find workflow script anywhere in this directory" 'ERROR'
    }
    $result.Errors += $msg
    if ($OutputFormat -eq 'Json') { $result | ConvertTo-Json -Depth 5 | Write-Output }
    exit 1
}
