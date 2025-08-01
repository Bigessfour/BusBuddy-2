#Requires -Version 7.5

<#
.SYNOPSIS
    Diagnostic-related functions for BusBuddy
.DESCRIPTION
    Contains functions for health checks and diagnostics
.NOTES
    File Name      : Diagnostics.ps1
    Author         : BusBuddy Development Team
    Prerequisite   : PowerShell 7.5.2
    Copyright 2025 - BusBuddy
#>

function global:Get-BusBuddyHealth {
    <#
    .SYNOPSIS
        Performs a health check on the BusBuddy development environment.
    .DESCRIPTION
        This function evaluates the health of the BusBuddy development environment
        by checking PowerShell version, .NET SDK version, project structure, and
        database configuration. Results are color-coded and can be exported to a file.
    .PARAMETER Quick
        When specified, performs a minimal health check with basic diagnostics.
    .PARAMETER Verbose
        Provides more detailed output during the health check.
    .PARAMETER Export
        Specifies a file path to export the results as JSON.
    .EXAMPLE
        Get-BusBuddyHealth
        Performs a standard health check with default settings.
    .EXAMPLE
        Get-BusBuddyHealth -Quick
        Performs a quick health check with minimal diagnostics.
    .EXAMPLE
        Get-BusBuddyHealth -Export "health-report.json"
        Performs a health check and exports the results to health-report.json.
    .OUTPUTS
        System.Collections.Hashtable containing the health check results.
    .NOTES
        This function is aliased to bb-health for backward compatibility.
    #>
    param (
        [Parameter(Mandatory = $false)]
        [switch]$Quick,

        [Parameter(Mandatory = $false)]
        [switch]$Verbose,

        [Parameter(Mandatory = $false)]
        [string]$Export
    )

    Write-Host "🩺 Running BusBuddy health check..." -ForegroundColor Cyan
    $startTime = Get-Date

    # Basic health checks
    $results = @{}

    # Check PowerShell version
    $psVersion = $PSVersionTable.PSVersion
    $psVersionOk = $psVersion.Major -ge 7 -and $psVersion.Minor -ge 5
    $results["PowerShell"] = @{
        "Version" = "$psVersion"
        "Status" = if ($psVersionOk) { "OK" } else { "WARNING" }
        "Message" = if ($psVersionOk) { "PowerShell 7.5+ detected" } else { "PowerShell 7.5+ recommended" }
    }

    # Check .NET SDK
    try {
        $dotnetVersion = (dotnet --version)
        $dotnetVersionOk = [Version]::TryParse($dotnetVersion, [ref]$null) -and
                           (($dotnetVersion.StartsWith("8.")) -or ($dotnetVersion.StartsWith("9.")))
        $results[".NET SDK"] = @{
            "Version" = "$dotnetVersion"
            "Status" = if ($dotnetVersionOk) { "OK" } else { "WARNING" }
            "Message" = if ($dotnetVersionOk) { ".NET 8 or 9 SDK detected" } else { ".NET 8 or 9 SDK recommended" }
        }
    } catch {
        $results[".NET SDK"] = @{
            "Version" = "Unknown"
            "Status" = "ERROR"
            "Message" = "Could not detect .NET SDK: $($_.Exception.Message)"
        }
    }

    # Check if solution file exists
    $solutionExists = Test-Path "BusBuddy.sln"
    $results["Solution File"] = @{
        "Status" = if ($solutionExists) { "OK" } else { "ERROR" }
        "Message" = if ($solutionExists) { "BusBuddy.sln found" } else { "BusBuddy.sln not found" }
    }

    # Check if project files exist
    $coreProjectExists = Test-Path "BusBuddy.Core/BusBuddy.Core.csproj"
    $wpfProjectExists = Test-Path "BusBuddy.WPF/BusBuddy.WPF.csproj"
    $results["Project Files"] = @{
        "Status" = if ($coreProjectExists -and $wpfProjectExists) { "OK" } else { "ERROR" }
        "Message" = if ($coreProjectExists -and $wpfProjectExists) {
            "Core and WPF projects found"
        } else {
            "Missing project files"
        }
    }

    # Check PATH environment for required tools
    $pathEntries = $env:PATH -split ';'
    $dotnetInPath = $pathEntries | Where-Object { Test-Path (Join-Path $_ "dotnet.exe") -ErrorAction SilentlyContinue }
    $results["Environment PATH"] = @{
        "Status" = if ($dotnetInPath) { "OK" } else { "WARNING" }
        "Message" = if ($dotnetInPath) { ".NET SDK found in PATH" } else { ".NET SDK not found in PATH" }
    }

    # Check database connection
    $dbConfigExists = Test-Path "BusBuddy.Core/appsettings.json"
    $results["Database Config"] = @{
        "Status" = if ($dbConfigExists) { "OK" } else { "WARNING" }
        "Message" = if ($dbConfigExists) { "Database configuration found" } else { "Database configuration missing" }
    }

    # Check required PowerShell modules
    $requiredModules = @('PSScriptAnalyzer')
    $modulesStatus = @{}

    foreach ($module in $requiredModules) {
        $moduleInstalled = Get-Module -Name $module -ListAvailable
        $modulesStatus[$module] = @{
            "Status" = if ($moduleInstalled) { "OK" } else { "WARNING" }
            "Message" = if ($moduleInstalled) { "Module installed" } else { "Module not found" }
        }
    }

    $results["PowerShell Modules"] = $modulesStatus

    # Check MCP environment
    $mcpConfigExists = Test-Path "$env:APPDATA\Claude\claude_desktop_config.json" -ErrorAction SilentlyContinue
    $mcpServerFiles = Test-Path "mcp-servers\*.js" -ErrorAction SilentlyContinue
    $mcpJsonExists = Test-Path "mcp.json" -ErrorAction SilentlyContinue
    $busBuddyAIExists = Test-Path "$PSScriptRoot\..\..\..\BusBuddy.AI\BusBuddy.AI.psm1" -ErrorAction SilentlyContinue

    # Load and test BusBuddy.AI module if not already loaded
    $busBuddyAILoaded = $null -ne (Get-Module -Name "BusBuddy.AI" -ErrorAction SilentlyContinue)
    if (-not $busBuddyAILoaded -and $busBuddyAIExists) {
        try {
            Import-Module "$PSScriptRoot\..\..\..\BusBuddy.AI\BusBuddy.AI.psm1" -ErrorAction Stop
            $busBuddyAILoaded = $true
        } catch {
            $busBuddyAILoaded = $false
        }
    }

    $results["MCP Environment"] = @{
        "Status" = if ($mcpConfigExists -and $mcpServerFiles -and $mcpJsonExists -and $busBuddyAILoaded) { "OK" } else { "WARNING" }
        "Message" = if ($mcpConfigExists -and $mcpServerFiles -and $mcpJsonExists -and $busBuddyAILoaded) {
            "MCP environment properly configured"
        } else {
            "MCP environment incomplete or not properly configured"
        }
        "Details" = @{
            "ConfigFile" = if ($mcpConfigExists) { "OK" } else { "Missing" }
            "ServerFiles" = if ($mcpServerFiles) { "OK" } else { "Missing" }
            "MCPJson" = if ($mcpJsonExists) { "OK" } else { "Missing" }
            "BusBuddy.AI" = if ($busBuddyAILoaded) { "Loaded" } else { if ($busBuddyAIExists) { "Found but not loaded" } else { "Missing" } }
        }
    }

    # Output results
    Write-Host ""
    Write-Host "Health Check Results:" -ForegroundColor Cyan
    Write-Host "=====================" -ForegroundColor Cyan

    $overallStatus = "OK"
    $resultCategories = @{
        "Critical" = @()
        "Warning" = @()
        "OK" = @()
    }

    foreach ($key in $results.Keys) {
        # Skip metadata in display
        if ($key -eq "Metadata") { continue }

        $item = $results[$key]

        # Handle nested hashtables (like PowerShell Modules)
        if ($item -is [hashtable] -and -not $item.ContainsKey("Status")) {
            Write-Host "$($key):" -ForegroundColor Cyan

            foreach ($subKey in $item.Keys) {
                $subItem = $item[$subKey]
                $statusColor = switch ($subItem.Status) {
                    "OK" { "Green" }
                    "WARNING" { "Yellow" }
                    "ERROR" { "Red" }
                    default { "White" }
                }

                Write-Host "  $subKey : " -NoNewline
                Write-Host $subItem.Status -ForegroundColor $statusColor -NoNewline

                if ($subItem.ContainsKey("Version")) {
                    Write-Host " ($($subItem.Version))" -NoNewline
                }

                Write-Host " - $($subItem.Message)"

                # Track overall status
                if ($subItem.Status -eq "ERROR") {
                    $overallStatus = "ERROR"
                    $resultCategories["Critical"] += "$key - $subKey"
                } elseif ($subItem.Status -eq "WARNING" -and $overallStatus -ne "ERROR") {
                    $overallStatus = "WARNING"
                    $resultCategories["Warning"] += "$key - $subKey"
                } else {
                    $resultCategories["OK"] += "$key - $subKey"
                }
            }

            Write-Host ""
        } else {
            $statusColor = switch ($item.Status) {
                "OK" { "Green" }
                "WARNING" { "Yellow" }
                "ERROR" { "Red" }
                default { "White" }
            }

            if ($item.Status -eq "ERROR") {
                $overallStatus = "ERROR"
                $resultCategories["Critical"] += $key
            } elseif ($item.Status -eq "WARNING" -and $overallStatus -ne "ERROR") {
                $overallStatus = "WARNING"
                $resultCategories["Warning"] += $key
            } else {
                $resultCategories["OK"] += $key
            }

            Write-Host "$key : " -NoNewline
            Write-Host $item.Status -ForegroundColor $statusColor -NoNewline

            if ($item.ContainsKey("Version")) {
                Write-Host " ($($item.Version))" -NoNewline
            }

            Write-Host " - $($item.Message)"
        }
    }

    Write-Host ""
    Write-Host "Overall Status: " -NoNewline
    $overallStatusColor = switch ($overallStatus) {
        "OK" { "Green" }
        "WARNING" { "Yellow" }
        "ERROR" { "Red" }
        default { "White" }
    }
    Write-Host $overallStatus -ForegroundColor $overallStatusColor

    # Calculate and display execution time
    $endTime = Get-Date
    $executionTime = ($endTime - $startTime).TotalSeconds
    Write-Host "Health check completed in $executionTime seconds" -ForegroundColor Cyan

    # Add execution metadata to results
    $results["Metadata"] = @{
        "ExecutionTime" = $executionTime
        "Timestamp" = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        "Mode" = if ($Quick) { "Quick" } else { "Full" }
    }

    # Export results if requested
    if ($Export) {
        $results | ConvertTo-Json -Depth 5 | Set-Content -Path $Export
        Write-Host "Results exported to: $Export" -ForegroundColor Cyan
    }

    return $results
}

function global:Get-BusBuddyDiagnostic {
    param (
        [Parameter(Mandatory = $false)]
        [switch]$Full,

        [Parameter(Mandatory = $false)]
        [switch]$CheckDependencies,

        [Parameter(Mandatory = $false)]
        [string]$Export
    )

    Write-Host "🔍 Running BusBuddy diagnostics..." -ForegroundColor Cyan

    $diagnosticResults = @{}

    # Basic diagnostics
    $diagnosticResults["BasicHealth"] = Get-BusBuddyHealth -Quick

    # More detailed diagnostics if requested
    if ($Full) {
        # Check for project structure issues
        Write-Host "Analyzing project structure..." -ForegroundColor Yellow
        $diagnosticResults["ProjectStructure"] = @{
            "MissingFiles" = @()
            "EmptyDirectories" = @()
            "Status" = "OK"
        }

        # Check for essential files
        $essentialFiles = @(
            "BusBuddy.sln",
            "BusBuddy.Core/BusBuddy.Core.csproj",
            "BusBuddy.WPF/BusBuddy.WPF.csproj",
            "BusBuddy.Core/appsettings.json",
            "BusBuddy.WPF/App.xaml",
            "BusBuddy.WPF/App.xaml.cs"
        )

        foreach ($file in $essentialFiles) {
            if (-not (Test-Path $file)) {
                $diagnosticResults["ProjectStructure"]["MissingFiles"] += $file
                $diagnosticResults["ProjectStructure"]["Status"] = "ERROR"
            }
        }

        # Check for empty directories
        $directories = Get-ChildItem -Directory -Recurse | Where-Object {
            $_.FullName -notlike "*\bin\*" -and
            $_.FullName -notlike "*\obj\*" -and
            $_.FullName -notlike "*\.git\*"
        }

        foreach ($dir in $directories) {
            if ((Get-ChildItem $dir.FullName -Force -Recurse | Measure-Object).Count -eq 0) {
                $diagnosticResults["ProjectStructure"]["EmptyDirectories"] += $dir.FullName
            }
        }
    }

    # Check dependencies if requested
    if ($CheckDependencies -or $Full) {
        Write-Host "Checking NuGet dependencies..." -ForegroundColor Yellow

        $projectFiles = Get-ChildItem -Recurse -Filter "*.csproj"
        $diagnosticResults["Dependencies"] = @{
            "Projects" = @{}
            "Status" = "OK"
        }

        foreach ($project in $projectFiles) {
            $projectContent = Get-Content $project.FullName -Raw
            $packageRefs = [regex]::Matches($projectContent, '<PackageReference\s+Include="([^"]+)"\s+Version="([^"]+)"')

            $projectDependencies = @{}
            foreach ($match in $packageRefs) {
                $packageName = $match.Groups[1].Value
                $packageVersion = $match.Groups[2].Value
                $projectDependencies[$packageName] = $packageVersion
            }

            $diagnosticResults["Dependencies"]["Projects"][$project.Name] = $projectDependencies
        }
    }

    # Output results
    Write-Host ""
    Write-Host "Diagnostic Results:" -ForegroundColor Cyan
    Write-Host "===================" -ForegroundColor Cyan

    if ($Full) {
        if ($diagnosticResults["ProjectStructure"]["MissingFiles"].Count -gt 0) {
            Write-Host "Missing Essential Files:" -ForegroundColor Red
            foreach ($file in $diagnosticResults["ProjectStructure"]["MissingFiles"]) {
                Write-Host "  - $file" -ForegroundColor Red
            }
        } else {
            Write-Host "Essential Files: " -NoNewline
            Write-Host "OK" -ForegroundColor Green
        }

        if ($diagnosticResults["ProjectStructure"]["EmptyDirectories"].Count -gt 0) {
            Write-Host "Empty Directories:" -ForegroundColor Yellow
            foreach ($dir in $diagnosticResults["ProjectStructure"]["EmptyDirectories"]) {
                Write-Host "  - $dir" -ForegroundColor Yellow
            }
        }
    }

    # Export results if requested
    if ($Export) {
        $diagnosticResults | ConvertTo-Json -Depth 10 | Set-Content -Path $Export
        Write-Host "Diagnostic results exported to: $Export" -ForegroundColor Cyan
    }

    return $diagnosticResults
}

# Create aliases for backward compatibility
Set-Alias -Name "bb-health" -Value Get-BusBuddyHealth
Set-Alias -Name "bb-diagnostic" -Value Get-BusBuddyDiagnostic

function global:Enable-BusBuddyUIDebug {
    <#
    .SYNOPSIS
        Toggles WPF UI debug mode using Syncfusion SfSkinManager themes
    .DESCRIPTION
        This function enables or disables UI debugging features for BusBuddy WPF application.
        It can toggle the visibility of UI elements, change themes for debugging, and enable
        performance visualization to help diagnose UI-related issues.
    .PARAMETER Mode
        Specifies the UI debug mode to enable:
        - Normal: Standard UI with no debug overlays
        - Layout: Shows layout boundaries and grid lines
        - Performance: Shows performance metrics for UI elements
        - Verbose: Maximum debug information shown
        - Toggle: Cycles through debug modes
    .PARAMETER Theme
        Specifies the Syncfusion theme to use:
        - FluentDark: Default dark theme (default)
        - FluentLight: Light theme for better visibility of some elements
        - Material: Material design theme for contrast testing
        - Office2019: Office theme for comparison
    .PARAMETER OutputToFile
        When specified, outputs debug information to a file
    .PARAMETER EnableLogging
        Enables additional UI-related logging
    .EXAMPLE
        Enable-BusBuddyUIDebug -Mode Layout
        Shows layout boundaries and grid lines for UI debugging
    .EXAMPLE
        Enable-BusBuddyUIDebug -Mode Toggle -Theme FluentLight
        Cycles to next debug mode with FluentLight theme
    .OUTPUTS
        PSObject with current debug settings
    .NOTES
        This function requires BusBuddy WPF application to be running
        This function is aliased to bb-debug-ui for convenience
    #>
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $false)]
        [ValidateSet('Normal', 'Layout', 'Performance', 'Verbose', 'Toggle')]
        [string]$Mode = 'Toggle',

        [Parameter(Mandatory = $false)]
        [ValidateSet('FluentDark', 'FluentLight', 'Material', 'Office2019')]
        [string]$Theme = 'FluentDark',

        [Parameter(Mandatory = $false)]
        [switch]$OutputToFile,

        [Parameter(Mandatory = $false)]
        [switch]$EnableLogging
    )

    Write-Host "🎨 BusBuddy UI Debug Mode" -ForegroundColor Cyan
    Write-Host "════════════════════════" -ForegroundColor DarkGray

    $processName = "BusBuddy.WPF"
    $process = Get-Process -Name $processName -ErrorAction SilentlyContinue

    if (-not $process) {
        Write-Host "❌ BusBuddy WPF application is not running." -ForegroundColor Red
        Write-Host "💡 Start the application first with 'bb-run'" -ForegroundColor Yellow
        return $false
    }

    Write-Host "✅ BusBuddy WPF application detected (PID: $($process.Id))" -ForegroundColor Green

    # Construct debug settings
    $debugSettings = @{
        Mode = $Mode
        Theme = $Theme
        EnableLogging = $EnableLogging.IsPresent
        ProcessId = $process.Id
        Timestamp = Get-Date
    }

    try {
        # Find current directory to place temp file for IPC
        $projectRoot = Get-BusBuddyProjectRoot
        if (-not $projectRoot) {
            $projectRoot = [System.IO.Path]::GetTempPath()
        }

        $debugCommandFile = Join-Path $projectRoot "bb_ui_debug_command.json"
        $debugResponseFile = Join-Path $projectRoot "bb_ui_debug_response.json"

        # Write command file for the application to pick up
        $debugSettings | ConvertTo-Json | Out-File -FilePath $debugCommandFile -Encoding UTF8 -Force

        # Notify the application
        Write-Host "📤 Sending debug command to BusBuddy application..." -ForegroundColor Yellow

        # We need to use the DebugHelper class in the application via command line arguments
        # Start a separate monitoring process to capture the response
        $monitorScript = @"
`$debugResponseFile = '$debugResponseFile'
`$maxWaitTime = 10  # seconds
`$waitCount = 0

while (-not (Test-Path `$debugResponseFile) -and `$waitCount -lt `$maxWaitTime) {
    Start-Sleep -Seconds 1
    `$waitCount++
}

if (Test-Path `$debugResponseFile) {
    `$response = Get-Content `$debugResponseFile -Raw | ConvertFrom-Json
    `$response
    Remove-Item `$debugResponseFile -Force
} else {
    @{ Success = `$false; Error = "Timeout waiting for application response" }
}
"@

        # Create a temporary script file
        $monitorScriptPath = Join-Path $projectRoot "bb_ui_debug_monitor.ps1"
        $monitorScript | Out-File -FilePath $monitorScriptPath -Encoding UTF8 -Force

        # Start the monitor process
        $monitorJob = Start-Job -ScriptBlock {
            param($scriptPath)
            & pwsh -NoProfile -ExecutionPolicy Bypass -File $scriptPath
        } -ArgumentList $monitorScriptPath

        # Wait for the response
        Write-Host "⏳ Waiting for application response..." -ForegroundColor Yellow
        $response = $monitorJob | Wait-Job -Timeout 10 | Receive-Job

        # Clean up
        Remove-Item $monitorScriptPath -Force -ErrorAction SilentlyContinue
        Remove-Item $debugCommandFile -Force -ErrorAction SilentlyContinue

        if ($response -and $response.Success) {
            Write-Host "✅ UI Debug mode successfully set to '$($response.ActiveMode)' with theme '$($response.ActiveTheme)'" -ForegroundColor Green

            if ($OutputToFile) {
                $outputPath = Join-Path $projectRoot "BusBuddy_UI_Debug_$(Get-Date -Format 'yyyyMMdd_HHmmss').json"
                $response | ConvertTo-Json -Depth 3 | Out-File -FilePath $outputPath -Encoding UTF8
                Write-Host "📄 Debug information saved to: $outputPath" -ForegroundColor Blue
            }

            return $response
        } else {
            Write-Host "❌ Failed to set UI Debug mode: $($response.Error)" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Stack Trace: $($_.ScriptStackTrace)" -ForegroundColor DarkGray
        return $false
    }
}

Set-Alias -Name "bb-debug-ui" -Value Enable-BusBuddyUIDebug

# Export functions
Export-ModuleMember -Function Get-BusBuddyHealth, Get-BusBuddyDiagnostic, Enable-BusBuddyUIDebug -Alias bb-health, bb-diagnostic, bb-debug-ui
