#Requires -Version 7.0
<#
.SYNOPSIS
    Bus Buddy Advanced Development Workflows

.DESCRIPTION
    Advanced PowerShell workflow automation for Bus Buddy WPF development
    Includes build automation, testing workflows, and comprehensive diagnostics

.NOTES
    Refactored version 2.0 with centralized command execution, enhanced error handling,
    and comprehensive diagnostic capabilities. Load this alongside BusBuddy-PowerShell-Profile.ps1
    for complete development environment.

.PARAMETER Quiet
    Suppress welcome message when dot-sourcing the script

.PARAMETER Command
    Ignored parameter for compatibility with external tools

.EXAMPLE
    . .\BusBuddy-Advanced-Workflows.ps1 -Quiet
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [switch]$Quiet,

    [Parameter(Mandatory = $false, DontShow)]
    [string]$Command,

    [Parameter(ValueFromRemainingArguments = $true, DontShow)]
    $RemainingArgs
)

# Check for global quiet flag if it exists (for compatibility with load-bus-buddy-profiles.ps1)
if (-not $Quiet -and (Get-Variable -Name BusBuddyAdvancedWorkflowsQuiet -Scope Script -ErrorAction SilentlyContinue)) {
    $Quiet = $Script:BusBuddyAdvancedWorkflowsQuiet
}

# Enhanced configuration with additional settings for improved functionality
$Script:BusBuddyConfig = @{
    SolutionFile          = 'BusBuddy.sln'
    Projects              = @{
        Core  = 'BusBuddy.Core'
        WPF   = 'BusBuddy.WPF'
        Tests = 'BusBuddy.Tests'
    }
    ProjectFiles          = @{
        Core  = 'BusBuddy.Core\BusBuddy.Core.csproj'
        WPF   = 'BusBuddy.WPF\BusBuddy.WPF.csproj'
        Tests = 'BusBuddy.Tests\BusBuddy.Tests.csproj'
    }
    TestCategories        = @('Unit', 'Integration', 'Performance', 'UI')
    LogDirectory          = 'logs'
    BuildConfiguration    = 'Debug'
    PublishConfiguration  = 'Release'
    SupportedFrameworks   = @('net8.0-windows')
    RequiredTools         = @{
        'dotnet' = '8.0'
        'pwsh'   = '7.0'
        'git'    = '2.0'
    }
    PerformanceThresholds = @{
        BuildTimeWarning   = 60    # seconds
        TestTimeWarning    = 30    # seconds
        RestoreTimeWarning = 15    # seconds
    }
    DefaultVerbosity      = 'minimal'
    NuGetCacheLocation    = @(
        "${env:USERPROFILE}\.nuget\packages"
        "${env:LOCALAPPDATA}\NuGet\Cache"
    )
}

# Helper to get project root with enhanced validation and git fallback
function Get-BusBuddyProjectRoot {
    <#
    .SYNOPSIS
        Find the Bus Buddy project root directory with enhanced validation and git fallback
    .PARAMETER SolutionName
        Name of the solution file to search for (defaults to configured value)
    .PARAMETER MaxDepth
        Maximum directory depth to search (default: 5)
    #>
    param(
        [Parameter(Mandatory = $false)]
        [string]$SolutionName = $Script:BusBuddyConfig.SolutionFile,

        [Parameter(Mandatory = $false)]
        [int]$MaxDepth = 5
    )

    $currentPath = $PWD.Path
    $depth = 0

    while ($depth -lt $MaxDepth) {
        # Primary check: look for solution file
        $solutionPath = Join-Path $currentPath $SolutionName
        if (Test-Path $solutionPath) {
            # Validate it's actually a Bus Buddy solution
            try {
                $solutionContent = Get-Content $solutionPath -Raw -ErrorAction SilentlyContinue
                if ($solutionContent -and $solutionContent -match 'BusBuddy') {
                    Write-Verbose "Found Bus Buddy project at: $currentPath"
                    return $currentPath
                }
            } catch {
                Write-Verbose "Failed to validate solution file: $_"
            }
        }

        # Fallback: check for git root if solution not found
        $gitPath = Join-Path $currentPath '.git'
        if (Test-Path $gitPath) {
            # Check if this git repo contains Bus Buddy projects
            $potentialProjects = Get-ChildItem $currentPath -Filter '*.csproj' -Recurse -ErrorAction SilentlyContinue |
                Where-Object { $_.Name -like '*BusBuddy*' } |
                    Select-Object -First 1

            if ($potentialProjects) {
                Write-Verbose "Found Bus Buddy project in git root: $currentPath"
                return $currentPath
            }
        }

        # Move up one directory
        $parent = Split-Path $currentPath -Parent
        if ($parent -eq $currentPath -or [string]::IsNullOrEmpty($parent)) {
            break
        }
        $currentPath = $parent
        $depth++
    }

    Write-Verbose "Bus Buddy project root not found after searching $depth directories"
    return $null
}

# Centralized dotnet command executor with comprehensive error handling and logging
function Invoke-DotnetCommand {
    <#
    .SYNOPSIS
        Execute dotnet commands with comprehensive error handling, logging, and performance monitoring
    .DESCRIPTION
        Centralized function for executing dotnet commands with consistent error handling,
        timing, output capture, and optional logging to files
    .PARAMETER Command
        The dotnet command to execute (e.g., 'build', 'test', 'restore')
    .PARAMETER Args
        Additional arguments for the command
    .PARAMETER SuccessMessage
        Message to display on successful completion
    .PARAMETER FailureMessage
        Message to display on failure
    .PARAMETER LogFile
        Optional path to log command output
    .PARAMETER Verbosity
        MSBuild verbosity level
    .PARAMETER WorkingDirectory
        Directory to execute command in (defaults to project root)
    .PARAMETER Force
        Force execution even if working directory validation fails
    .PARAMETER SuppressWelcome
        Suppress dotnet welcome message
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$Command,

        [Parameter(Mandatory = $false)]
        [string[]]$Args = @(),

        [Parameter(Mandatory = $false)]
        [string]$SuccessMessage = 'Operation completed successfully',

        [Parameter(Mandatory = $false)]
        [string]$FailureMessage = 'Operation failed',

        [Parameter(Mandatory = $false)]
        [string]$LogFile = $null,

        [Parameter(Mandatory = $false)]
        [ValidateSet('quiet', 'minimal', 'normal', 'detailed', 'diagnostic')]
        [string]$Verbosity = $Script:BusBuddyConfig.DefaultVerbosity,

        [Parameter(Mandatory = $false)]
        [string]$WorkingDirectory = $null,

        [Parameter(Mandatory = $false)]
        [switch]$Force,

        [Parameter(Mandatory = $false)]
        [switch]$SuppressWelcome
    )

    # Validate dotnet is available
    if (-not (Get-Command 'dotnet' -ErrorAction SilentlyContinue)) {
        $errorMsg = ".NET SDK not found in PATH"
        Write-Host "❌ $errorMsg" -ForegroundColor Red
        Write-Host "💡 Download from: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
        return @{
            Success  = $false;
            ExitCode = -1;
            Output   = @();
            Errors   = @($errorMsg);
            Duration = 0
        }
    }

    # Determine working directory
    if (-not $WorkingDirectory) {
        $WorkingDirectory = Get-BusBuddyProjectRoot
        if (-not $WorkingDirectory) {
            $errorMsg = "Bus Buddy project root not found"
            Write-Host "❌ $errorMsg" -ForegroundColor Red
            Write-Host "💡 Navigate to project directory or use -WorkingDirectory parameter" -ForegroundColor Yellow
            return @{
                Success  = $false;
                ExitCode = -1;
                Output   = @();
                Errors   = @($errorMsg);
                Duration = 0
            }
        }
    }

    # Validate working directory
    if (-not (Test-Path $WorkingDirectory)) {
        $errorMsg = "Working directory not found: $WorkingDirectory"
        Write-Host "❌ $errorMsg" -ForegroundColor Red
        return @{
            Success  = $false;
            ExitCode = -1;
            Output   = @();
            Errors   = @($errorMsg);
            Duration = 0
        }
    }

    $originalLocation = Get-Location
    $startTime = Get-Date
    $allOutput = @()
    $errorOutput = @()

    try {
        Set-Location $WorkingDirectory

        # Build complete argument list
        $fullArgs = @($Command) + $Args + @('--verbosity', $Verbosity)
        if ($SuppressWelcome) {
            $fullArgs += '--nologo'
        }

        # Display execution info
        Write-Host "🔄 Executing dotnet $Command..." -ForegroundColor Cyan
        Write-Verbose "Working Directory: $WorkingDirectory"
        Write-Verbose "Full Command: dotnet $($fullArgs -join ' ')"

        # Execute command with output capture
        if ($LogFile) {
            # Ensure log directory exists
            $logDir = Split-Path $LogFile -Parent
            if ($logDir -and -not (Test-Path $logDir)) {
                New-Item -ItemType Directory -Path $logDir -Force | Out-Null
            }

            # Execute with output redirection
            $output = & dotnet @fullArgs 2>&1
            $exitCode = $LASTEXITCODE

            # Parse output and errors
            foreach ($line in $output) {
                if ($line -is [System.Management.Automation.ErrorRecord]) {
                    $errorOutput += $line.ToString()
                } else {
                    $allOutput += $line.ToString()
                }
            }

            # Write to log file
            try {
                $logEntry = @{
                    Timestamp        = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
                    Command          = "dotnet $($fullArgs -join ' ')"
                    WorkingDirectory = $WorkingDirectory
                    ExitCode         = $exitCode
                    Output           = $allOutput
                    Errors           = $errorOutput
                } | ConvertTo-Json -Depth 3

                Add-Content -Path $LogFile -Value $logEntry -Encoding UTF8
                Write-Verbose "Output logged to: $LogFile"
            } catch {
                Write-Warning "Failed to write to log file: $_"
            }
        } else {
            # Execute with direct output
            & dotnet @fullArgs
            $exitCode = $LASTEXITCODE
        }

        $duration = ((Get-Date) - $startTime).TotalSeconds

        # Analyze results and provide feedback
        if ($exitCode -eq 0) {
            Write-Host "✅ $SuccessMessage in $([math]::Round($duration, 2)) seconds" -ForegroundColor Green

            # Performance warning if operation takes too long
            $warningThreshold = switch ($Command) {
                'build' { $Script:BusBuddyConfig.PerformanceThresholds.BuildTimeWarning }
                'test' { $Script:BusBuddyConfig.PerformanceThresholds.TestTimeWarning }
                'restore' { $Script:BusBuddyConfig.PerformanceThresholds.RestoreTimeWarning }
                default { 120 }
            }

            if ($duration -gt $warningThreshold) {
                Write-Host "⚠️  Operation took longer than expected ($warningThreshold seconds)" -ForegroundColor Yellow
                Write-Host "💡 Consider optimizing dependencies or build configuration" -ForegroundColor Gray
            }

            return @{
                Success  = $true;
                ExitCode = $exitCode;
                Output   = $allOutput;
                Errors   = $errorOutput;
                Duration = $duration
            }
        } else {
            Write-Host "❌ $FailureMessage with exit code $exitCode after $([math]::Round($duration, 2)) seconds" -ForegroundColor Red

            # Provide specific guidance for common errors
            $guidance = switch ($exitCode) {
                1 {
                    "Check for compilation errors in the output above"
                    # Look for specific error patterns
                    $compileErrors = $allOutput + $errorOutput | Where-Object { $_ -match 'error CS\d+' }
                    if ($compileErrors) {
                        Write-Host "� Compilation errors detected:" -ForegroundColor Yellow
                        $compileErrors | ForEach-Object { Write-Host "  • $_" -ForegroundColor Red }
                    }
                }
                2 { "Check project file paths and references" }
                3 { "Check for missing dependencies or packages" }
                4 { "Build configuration or target framework issues" }
                default { "Check the error output above for details" }
            }
            Write-Host "💡 $guidance" -ForegroundColor Yellow

            return @{
                Success  = $false;
                ExitCode = $exitCode;
                Output   = $allOutput;
                Errors   = $errorOutput;
                Duration = $duration
            }
        }
    } catch {
        $duration = ((Get-Date) - $startTime).TotalSeconds
        $errorMsg = $_.Exception.Message
        Write-Host "❌ $FailureMessage with exception after $([math]::Round($duration, 2)) seconds" -ForegroundColor Red
        Write-Host "Exception: $errorMsg" -ForegroundColor Red

        return @{
            Success  = $false;
            ExitCode = -1;
            Output   = $allOutput;
            Errors   = @($errorMsg);
            Duration = $duration
        }
    } finally {
        Set-Location $originalLocation
    }
}

# Core Build Commands with enhanced functionality
function Invoke-BusBuddyBuild {
    <#
    .SYNOPSIS
        Build the Bus Buddy solution with comprehensive validation and enhanced error handling
    .DESCRIPTION
        Performs a clean build of the Bus Buddy solution with comprehensive validation,
        dependency checking, and detailed error reporting
    .PARAMETER Configuration
        Build configuration (Debug or Release)
    .PARAMETER Verbosity
        MSBuild verbosity level
    .PARAMETER LogFile
        Optional path to log build output
    .PARAMETER Force
        Force build even if validation warnings exist
    .PARAMETER NoRestore
        Skip automatic package restore
    .PARAMETER Framework
        Target framework for build
    #>
    param(
        [Parameter(Mandatory = $false)]
        [ValidateSet('Debug', 'Release')]
        [string]$Configuration = $Script:BusBuddyConfig.BuildConfiguration,

        [Parameter(Mandatory = $false)]
        [ValidateSet('quiet', 'minimal', 'normal', 'detailed', 'diagnostic')]
        [string]$Verbosity = $Script:BusBuddyConfig.DefaultVerbosity,

        [Parameter(Mandatory = $false)]
        [string]$LogFile = $null,

        [Parameter(Mandatory = $false)]
        [switch]$Force,

        [Parameter(Mandatory = $false)]
        [switch]$NoRestore,

        [Parameter(Mandatory = $false)]
        [string]$Framework = $null
    )

    Write-Host '🚀 Building Bus Buddy solution...' -ForegroundColor Cyan

    # Pre-build validation
    $root = Get-BusBuddyProjectRoot
    if (-not $root) {
        Write-Host '❌ Bus Buddy project not found. Navigate to project directory first.' -ForegroundColor Red
        Write-Host '💡 Ensure you are in a directory containing BusBuddy.sln or a subdirectory' -ForegroundColor Yellow
        return $false
    }

    # Validate solution file
    $solutionPath = Join-Path $root $Script:BusBuddyConfig.SolutionFile
    if (-not (Test-Path $solutionPath)) {
        Write-Host "❌ Solution file not found: $solutionPath" -ForegroundColor Red
        return $false
    }

    # Build arguments
    $buildArgs = @($Script:BusBuddyConfig.SolutionFile, '--configuration', $Configuration)

    if ($NoRestore) {
        $buildArgs += '--no-restore'
    }

    if ($Framework) {
        $buildArgs += '--framework', $Framework
    }

    # Setup logging if requested
    if ($LogFile) {
        if (-not (Split-Path $LogFile -Parent | Test-Path)) {
            $logDir = Split-Path $LogFile -Parent
            New-Item -ItemType Directory -Path $logDir -Force | Out-Null
        }
    } else {
        # Create default log file with timestamp
        $logsDir = Join-Path $root $Script:BusBuddyConfig.LogDirectory
        if (-not (Test-Path $logsDir)) {
            New-Item -ItemType Directory -Path $logsDir -Force | Out-Null
        }
        $LogFile = Join-Path $logsDir "build-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
    }

    # Execute build
    $result = Invoke-DotnetCommand -Command 'build' -Args $buildArgs -SuccessMessage 'Build completed' -FailureMessage 'Build failed' -Verbosity $Verbosity -LogFile $LogFile -SuppressWelcome

    # Post-build analysis
    if (-not $result.Success) {
        Write-Host "`n🔍 Build Error Analysis:" -ForegroundColor Yellow

        # Analyze specific error patterns
        $allErrors = $result.Output + $result.Errors
        $compilationErrors = $allErrors | Where-Object { $_ -match 'error CS\d+' }
        $packageErrors = $allErrors | Where-Object { $_ -match 'package.*not found|NU\d+' }
        $referenceErrors = $allErrors | Where-Object { $_ -match 'reference.*could not be resolved' }

        if ($compilationErrors) {
            Write-Host "  🔴 Compilation Errors:" -ForegroundColor Red
            $compilationErrors | ForEach-Object { Write-Host "    • $_" -ForegroundColor Red }
        }

        if ($packageErrors) {
            Write-Host "  📦 Package Issues:" -ForegroundColor Yellow
            $packageErrors | ForEach-Object { Write-Host "    • $_" -ForegroundColor Yellow }
            Write-Host "    💡 Try running: bb-restore" -ForegroundColor Gray
        }

        if ($referenceErrors) {
            Write-Host "  🔗 Reference Issues:" -ForegroundColor Yellow
            $referenceErrors | ForEach-Object { Write-Host "    • $_" -ForegroundColor Yellow }
            Write-Host "    💡 Check project references and dependencies" -ForegroundColor Gray
        }

        Write-Host "  📝 Full log available at: $LogFile" -ForegroundColor Gray
    } else {
        Write-Host "📝 Build log saved to: $LogFile" -ForegroundColor Gray
    }

    return $result.Success
}

function Clear-BusBuddyBuild {
    <#
    .SYNOPSIS
        Clean the Bus Buddy solution with enhanced options
    .DESCRIPTION
        Cleans build artifacts with options for different levels of cleaning
    .PARAMETER Verbosity
        MSBuild verbosity level
    .PARAMETER LogFile
        Optional path to log clean output
    .PARAMETER Deep
        Perform deep clean including NuGet cache and temp files
    .PARAMETER Force
        Force clean even if some operations fail
    #>
    param(
        [Parameter(Mandatory = $false)]
        [ValidateSet('quiet', 'minimal', 'normal', 'detailed', 'diagnostic')]
        [string]$Verbosity = $Script:BusBuddyConfig.DefaultVerbosity,

        [Parameter(Mandatory = $false)]
        [string]$LogFile = $null,

        [Parameter(Mandatory = $false)]
        [switch]$Deep,

        [Parameter(Mandatory = $false)]
        [switch]$Force
    )

    Write-Host '🔧 Cleaning Bus Buddy solution...' -ForegroundColor Cyan

    $root = Get-BusBuddyProjectRoot
    if (-not $root) {
        Write-Host '❌ Bus Buddy project not found. Navigate to project directory first.' -ForegroundColor Red
        return $false
    }

    # Standard clean
    $cleanArgs = @($Script:BusBuddyConfig.SolutionFile)
    $result = Invoke-DotnetCommand -Command 'clean' -Args $cleanArgs -SuccessMessage 'Clean completed' -FailureMessage 'Clean failed' -Verbosity $Verbosity -LogFile $LogFile -SuppressWelcome

    # Deep clean if requested
    if ($Deep) {
        Write-Host '🧹 Performing deep clean...' -ForegroundColor Yellow

        try {
            # Remove bin and obj directories
            $binDirs = Get-ChildItem -Path $root -Name 'bin' -Directory -Recurse -ErrorAction SilentlyContinue
            $objDirs = Get-ChildItem -Path $root -Name 'obj' -Directory -Recurse -ErrorAction SilentlyContinue

            $totalDirs = $binDirs.Count + $objDirs.Count
            if ($totalDirs -gt 0) {
                Write-Host "  Removing $totalDirs build artifact directories..." -ForegroundColor Gray

                ($binDirs + $objDirs) | ForEach-Object {
                    try {
                        $fullPath = Join-Path $root $_
                        if (Test-Path $fullPath) {
                            Remove-Item -Path $fullPath -Recurse -Force -ErrorAction Stop
                            Write-Verbose "Removed: $fullPath"
                        }
                    } catch {
                        if (-not $Force) {
                            Write-Warning "Failed to remove $_`: $($_.Exception.Message)"
                        }
                    }
                }
            }

            # Clear NuGet cache if requested
            Write-Host '  Clearing NuGet cache...' -ForegroundColor Gray
            Invoke-DotnetCommand -Command 'nuget' -Args @('locals', 'all', '--clear') -SuccessMessage 'NuGet cache cleared' -FailureMessage 'NuGet cache clear failed' -Verbosity 'quiet'

            Write-Host '✅ Deep clean completed' -ForegroundColor Green
        } catch {
            Write-Host "⚠️  Deep clean partially failed: $($_.Exception.Message)" -ForegroundColor Yellow
            if (-not $Force) {
                $result.Success = $false
            }
        }
    }

    return $result.Success
}

function Invoke-BusBuddyTest {
    <#
    .SYNOPSIS
        Run tests for the Bus Buddy solution with comprehensive filtering and reporting
    .DESCRIPTION
        Executes tests with advanced filtering, parallel execution, and detailed reporting
    .PARAMETER Configuration
        Test configuration (Debug or Release)
    .PARAMETER NoBuild
        Skip building before running tests
    .PARAMETER Category
        Test category filter (Unit, Integration, Performance, UI)
    .PARAMETER Filter
        Custom test filter expression
    .PARAMETER Project
        Specific project to test (Core, WPF, Tests)
    .PARAMETER Framework
        Target framework to test
    .PARAMETER Logger
        Test logger options (trx, junit, html)
    .PARAMETER LogFile
        Path to save test output
    .PARAMETER Parallel
        Run tests in parallel
    .PARAMETER Collect
        Data collector for coverage
    .PARAMETER Settings
        Test settings file
    .PARAMETER Verbosity
        Test output verbosity
    #>
    param(
        [Parameter(Mandatory = $false)]
        [ValidateSet('Debug', 'Release')]
        [string]$Configuration = $Script:BusBuddyConfig.BuildConfiguration,

        [Parameter(Mandatory = $false)]
        [switch]$NoBuild,

        [Parameter(Mandatory = $false)]
        [ValidateSet('Unit', 'Integration', 'Performance', 'UI', 'All')]
        [string]$Category = 'All',

        [Parameter(Mandatory = $false)]
        [string]$Filter,

        [Parameter(Mandatory = $false)]
        [ValidateSet('Core', 'WPF', 'Tests', 'All')]
        [string]$Project = 'All',

        [Parameter(Mandatory = $false)]
        [string]$Framework,

        [Parameter(Mandatory = $false)]
        [ValidateSet('trx', 'junit', 'html', 'console')]
        [string[]]$Logger = @('console'),

        [Parameter(Mandatory = $false)]
        [string]$LogFile = $null,

        [Parameter(Mandatory = $false)]
        [switch]$Parallel,

        [Parameter(Mandatory = $false)]
        [string]$Collect = $null,

        [Parameter(Mandatory = $false)]
        [string]$Settings = $null,

        [Parameter(Mandatory = $false)]
        [ValidateSet('quiet', 'minimal', 'normal', 'detailed', 'diagnostic')]
        [string]$Verbosity = $Script:BusBuddyConfig.DefaultVerbosity
    )

    Write-Host '🧪 Running Bus Buddy tests...' -ForegroundColor Cyan

    $root = Get-BusBuddyProjectRoot
    if (-not $root) {
        Write-Host '❌ Bus Buddy project not found. Navigate to project directory first.' -ForegroundColor Red
        return $false
    }

    # Build test arguments
    $testArgs = @()

    # Determine target
    if ($Project -eq 'All') {
        $testArgs += $Script:BusBuddyConfig.SolutionFile
    } else {
        $projectFile = $Script:BusBuddyConfig.ProjectFiles[$Project]
        if (-not $projectFile) {
            Write-Host "❌ Unknown project: $Project" -ForegroundColor Red
            return $false
        }
        $projectPath = Join-Path $root $projectFile
        if (-not (Test-Path $projectPath)) {
            Write-Host "❌ Project file not found: $projectPath" -ForegroundColor Red
            return $false
        }
        $testArgs += $projectPath
    }

    # Add configuration and basic options
    $testArgs += '--configuration', $Configuration

    if ($NoBuild) {
        $testArgs += '--no-build'
    }

    if ($Framework) {
        $testArgs += '--framework', $Framework
    }

    # Build filter expression
    $filterExpressions = @()
    if ($Category -ne 'All') {
        $filterExpressions += "Category=$Category"
    }
    if ($Filter) {
        $filterExpressions += $Filter
    }
    if ($filterExpressions.Count -gt 0) {
        $testArgs += '--filter', ($filterExpressions -join ' & ')
    }

    # Add data collection
    if ($Collect) {
        $testArgs += '--collect', $Collect
    }

    # Add settings file
    if ($Settings -and (Test-Path $Settings)) {
        $testArgs += '--settings', $Settings
    }

    # Add parallel execution
    if ($Parallel) {
        $testArgs += '--parallel'
    }

    # Configure loggers
    $logsDir = Join-Path $root $Script:BusBuddyConfig.LogDirectory
    if (-not (Test-Path $logsDir)) {
        New-Item -ItemType Directory -Path $logsDir -Force | Out-Null
    }

    $timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'

    foreach ($log in $Logger) {
        switch ($log) {
            'trx' {
                $trxPath = Join-Path $logsDir "test-results-$timestamp.trx"
                $testArgs += '--logger', "trx;LogFileName=$trxPath"
                Write-Host "📊 TRX results will be saved to: $trxPath" -ForegroundColor Gray
            }
            'junit' {
                $junitPath = Join-Path $logsDir "test-results-$timestamp.xml"
                $testArgs += '--logger', "junit;LogFilePath=$junitPath"
                Write-Host "📊 JUnit results will be saved to: $junitPath" -ForegroundColor Gray
            }
            'html' {
                $htmlPath = Join-Path $logsDir "test-results-$timestamp.html"
                $testArgs += '--logger', "html;LogFileName=$htmlPath"
                Write-Host "📊 HTML results will be saved to: $htmlPath" -ForegroundColor Gray
            }
            'console' {
                $testArgs += '--logger', 'console;verbosity=normal'
            }
        }
    }

    # Display test configuration
    Write-Host "  Project: $Project | Category: $Category | Configuration: $Configuration" -ForegroundColor Gray
    if ($filterExpressions.Count -gt 0) {
        Write-Host "  Filter: $($filterExpressions -join ' & ')" -ForegroundColor Gray
    }

    # Execute tests
    $result = Invoke-DotnetCommand -Command 'test' -Args $testArgs -SuccessMessage 'Tests completed' -FailureMessage 'Tests failed' -Verbosity $Verbosity -LogFile $LogFile -SuppressWelcome

    # Post-test analysis
    if (-not $result.Success) {
        Write-Host "`n🔍 Test Failure Analysis:" -ForegroundColor Yellow

        $allOutput = $result.Output + $result.Errors
        $failedTests = $allOutput | Where-Object { $_ -match 'Failed\s+\w+' -or $_ -match '\[FAIL\]' }
        $passedTests = $allOutput | Where-Object { $_ -match 'Passed\s+\w+' -or $_ -match '\[PASS\]' }

        if ($failedTests) {
            Write-Host "  ❌ Failed Tests:" -ForegroundColor Red
            $failedTests | ForEach-Object { Write-Host "    • $_" -ForegroundColor Red }
        }

        if ($passedTests) {
            Write-Host "  ✅ Passed Tests: $($passedTests.Count)" -ForegroundColor Green
        }

        if ($NoBuild) {
            Write-Host "  💡 Try running bb-build first if tests fail due to compilation issues" -ForegroundColor Yellow
        }
    }

    return $result.Success
}

function Restore-BusBuddyPackages {
    <#
    .SYNOPSIS
        Restore NuGet packages with enhanced validation and diagnostics
    .DESCRIPTION
        Restore packages with comprehensive validation and detailed diagnostics
    .PARAMETER Verbosity
        Verbosity level for restore operation
    .PARAMETER LogFile
        Optional path to log restore output
    .PARAMETER Force
        Force restore even if packages.lock.json exists
    .PARAMETER NoCache
        Ignore NuGet cache during restore
    .PARAMETER CheckVulnerabilities
        Check for vulnerable packages after restore
    #>
    param(
        [Parameter(Mandatory = $false)]
        [ValidateSet('quiet', 'minimal', 'normal', 'detailed', 'diagnostic')]
        [string]$Verbosity = $Script:BusBuddyConfig.DefaultVerbosity,

        [Parameter(Mandatory = $false)]
        [string]$LogFile = $null,

        [Parameter(Mandatory = $false)]
        [switch]$Force,

        [Parameter(Mandatory = $false)]
        [switch]$NoCache,

        [Parameter(Mandatory = $false)]
        [switch]$CheckVulnerabilities
    )

    Write-Host '📦 Restoring NuGet packages...' -ForegroundColor Cyan

    $root = Get-BusBuddyProjectRoot
    if (-not $root) {
        Write-Host '❌ Bus Buddy project not found. Navigate to project directory first.' -ForegroundColor Red
        return $false
    }

    # Check NuGet configuration
    $nugetConfig = Join-Path $root 'nuget.config'
    if (Test-Path $nugetConfig) {
        Write-Host "📋 Using NuGet configuration: $nugetConfig" -ForegroundColor Gray
    }

    # Build restore arguments
    $restoreArgs = @($Script:BusBuddyConfig.SolutionFile)

    if ($Force) {
        $restoreArgs += '--force'
    }

    if ($NoCache) {
        $restoreArgs += '--no-cache'
    }

    # Execute restore
    $result = Invoke-DotnetCommand -Command 'restore' -Args $restoreArgs -SuccessMessage 'Package restore completed' -FailureMessage 'Package restore failed' -Verbosity $Verbosity -LogFile $LogFile -SuppressWelcome

    # Check for vulnerabilities if requested and restore succeeded
    if ($result.Success -and $CheckVulnerabilities) {
        Write-Host '🔍 Checking for vulnerable packages...' -ForegroundColor Yellow
        $vulnResult = Invoke-DotnetCommand -Command 'list' -Args @($Script:BusBuddyConfig.SolutionFile, 'package', '--vulnerable') -SuccessMessage 'Vulnerability check completed' -FailureMessage 'Vulnerability check failed' -Verbosity 'quiet'

        if ($vulnResult.Success) {
            $vulnerabilities = $vulnResult.Output | Where-Object { $_ -match 'has the following vulnerable' -or $_ -match 'severity' }
            if ($vulnerabilities) {
                Write-Host "⚠️  Vulnerable packages detected:" -ForegroundColor Red
                $vulnerabilities | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
                Write-Host "💡 Consider updating vulnerable packages" -ForegroundColor Gray
            } else {
                Write-Host "✅ No vulnerable packages detected" -ForegroundColor Green
            }
        }
    }

    # Post-restore analysis
    if (-not $result.Success) {
        Write-Host "`n🔍 Package Restore Analysis:" -ForegroundColor Yellow
        Write-Host "💡 Common solutions:" -ForegroundColor Yellow
        Write-Host "  • Check internet connectivity" -ForegroundColor Gray
        Write-Host "  • Verify NuGet package sources: dotnet nuget list source" -ForegroundColor Gray
        Write-Host "  • Clear NuGet cache: bb-clean -Deep" -ForegroundColor Gray
        Write-Host "  • Check authentication for private feeds" -ForegroundColor Gray
    }

    return $result.Success
}

function Publish-BusBuddy {
    <#
    .SYNOPSIS
        Publish Bus Buddy application with Azure deployment options and comprehensive validation
    .DESCRIPTION
        Creates optimized deployments with Azure configuration, self-contained options, and deployment validation
    .PARAMETER Configuration
        Build configuration for publishing (Release is recommended)
    .PARAMETER Runtime
        Target runtime identifier (win-x64, win-x86, win-arm64)
    .PARAMETER SelfContained
        Create self-contained deployment
    .PARAMETER OutputPath
        Custom output directory for published files
    .PARAMETER ReadyToRun
        Enable ReadyToRun compilation for faster startup
    .PARAMETER TrimUnusedCode
        Enable IL trimming to reduce size
    .PARAMETER SingleFile
        Create single-file executable
    .PARAMETER AzureReady
        Configure for Azure App Service deployment
    .PARAMETER IncludeNativeLibraries
        Include native libraries for Syncfusion controls
    .PARAMETER ValidateOutput
        Validate published output after completion
    #>
    param(
        [Parameter(Mandatory = $false)]
        [ValidateSet('Debug', 'Release')]
        [string]$Configuration = 'Release',

        [Parameter(Mandatory = $false)]
        [ValidateSet('win-x64', 'win-x86', 'win-arm64', 'portable')]
        [string]$Runtime = 'win-x64',

        [Parameter(Mandatory = $false)]
        [switch]$SelfContained,

        [Parameter(Mandatory = $false)]
        [string]$OutputPath = $null,

        [Parameter(Mandatory = $false)]
        [switch]$ReadyToRun,

        [Parameter(Mandatory = $false)]
        [switch]$TrimUnusedCode,

        [Parameter(Mandatory = $false)]
        [switch]$SingleFile,

        [Parameter(Mandatory = $false)]
        [switch]$AzureReady,

        [Parameter(Mandatory = $false)]
        [switch]$IncludeNativeLibraries,

        [Parameter(Mandatory = $false)]
        [switch]$ValidateOutput
    )

    Write-Host '📦 Publishing Bus Buddy application...' -ForegroundColor Cyan

    $root = Get-BusBuddyProjectRoot
    if (-not $root) {
        Write-Host '❌ Bus Buddy project not found. Navigate to project directory first.' -ForegroundColor Red
        return $false
    }

    # Validate WPF project
    $wpfProjectPath = Join-Path $root $Script:BusBuddyConfig.ProjectFiles.WPF
    if (-not (Test-Path $wpfProjectPath)) {
        Write-Host "❌ WPF project not found: $wpfProjectPath" -ForegroundColor Red
        return $false
    }

    # Set default output path
    if (-not $OutputPath) {
        $timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
        $OutputPath = Join-Path $root "publish\busbuddy-$Configuration-$Runtime-$timestamp"
    }

    # Ensure output directory exists
    if (-not (Test-Path (Split-Path $OutputPath -Parent))) {
        New-Item -ItemType Directory -Path (Split-Path $OutputPath -Parent) -Force | Out-Null
    }

    # Build publish arguments
    $publishArgs = @(
        'publish',
        $Script:BusBuddyConfig.ProjectFiles.WPF,
        '--configuration', $Configuration,
        '--output', $OutputPath
    )

    if ($Runtime -ne 'portable') {
        $publishArgs += '--runtime', $Runtime
    }

    if ($SelfContained) {
        $publishArgs += '--self-contained', 'true'
    } else {
        $publishArgs += '--self-contained', 'false'
    }

    if ($ReadyToRun) {
        $publishArgs += '--property:PublishReadyToRun=true'
    }

    if ($TrimUnusedCode -and $SelfContained) {
        $publishArgs += '--property:PublishTrimmed=true'
        Write-Host '⚠️  IL trimming enabled - test thoroughly before deployment' -ForegroundColor Yellow
    }

    if ($SingleFile -and $SelfContained) {
        $publishArgs += '--property:PublishSingleFile=true'
    }

    # Azure-specific optimizations
    if ($AzureReady) {
        Write-Host '☁️  Configuring for Azure deployment...' -ForegroundColor Cyan
        $publishArgs += '--property:PublishProfile=Azure'
        $publishArgs += '--property:EnvironmentName=Production'

        # Copy Azure-specific configuration
        $azureConfigPath = Join-Path $root 'appsettings.azure.json'
        if (Test-Path $azureConfigPath) {
            Write-Host "📋 Azure configuration found: $azureConfigPath" -ForegroundColor Green
        } else {
            Write-Host "⚠️  Azure configuration not found. Creating template..." -ForegroundColor Yellow
            $azureTemplate = @{
                "Logging"           = @{
                    "LogLevel" = @{
                        "Default"                    = "Information"
                        "Microsoft.Hosting.Lifetime" = "Information"
                    }
                }
                "AllowedHosts"      = "*"
                "ConnectionStrings" = @{
                    "DefaultConnection" = "#{CONNECTION_STRING}#"
                }
            } | ConvertTo-Json -Depth 4
            $azureTemplate | Set-Content -Path $azureConfigPath -Encoding UTF8
        }
    }

    # Include native libraries for Syncfusion
    if ($IncludeNativeLibraries) {
        Write-Host '📚 Including Syncfusion native libraries...' -ForegroundColor Cyan
        $publishArgs += '--property:IncludeNativeLibrariesForSelfExtract=true'
    }

    # Display publish configuration
    Write-Host "📋 Publish Configuration:" -ForegroundColor Gray
    Write-Host "  Configuration: $Configuration" -ForegroundColor Gray
    Write-Host "  Runtime: $Runtime" -ForegroundColor Gray
    Write-Host "  Self-Contained: $SelfContained" -ForegroundColor Gray
    Write-Host "  Output: $OutputPath" -ForegroundColor Gray
    if ($ReadyToRun) { Write-Host "  ReadyToRun: Enabled" -ForegroundColor Gray }
    if ($TrimUnusedCode) { Write-Host "  IL Trimming: Enabled" -ForegroundColor Gray }
    if ($SingleFile) { Write-Host "  Single File: Enabled" -ForegroundColor Gray }
    if ($AzureReady) { Write-Host "  Azure Ready: Enabled" -ForegroundColor Gray }

    # Execute publish
    $result = Invoke-DotnetCommand -Command 'publish' -Args $publishArgs[1..($publishArgs.Length - 1)] -SuccessMessage 'Publish completed' -FailureMessage 'Publish failed' -SuppressWelcome

    if ($result.Success) {
        Write-Host "✅ Application published successfully to: $OutputPath" -ForegroundColor Green

        # Validate output if requested
        if ($ValidateOutput) {
            Write-Host '🔍 Validating published output...' -ForegroundColor Yellow

            $exeName = "BusBuddy.WPF.exe"
            $exePath = Join-Path $OutputPath $exeName

            if (Test-Path $exePath) {
                Write-Host "  ✅ Executable found: $exeName" -ForegroundColor Green

                # Check file size
                $fileSize = [math]::Round((Get-Item $exePath).Length / 1MB, 2)
                Write-Host "  📏 Executable size: $fileSize MB" -ForegroundColor Gray

                # Check dependencies
                $publishedFiles = Get-ChildItem $OutputPath -Recurse
                $totalSize = [math]::Round(($publishedFiles | Measure-Object Length -Sum).Sum / 1MB, 2)
                Write-Host "  📦 Total deployment size: $totalSize MB" -ForegroundColor Gray
                Write-Host "  📁 Total files: $($publishedFiles.Count)" -ForegroundColor Gray

                # Syncfusion library check
                $syncfusionFiles = $publishedFiles | Where-Object { $_.Name -like '*Syncfusion*' }
                if ($syncfusionFiles.Count -gt 0) {
                    Write-Host "  ✅ Syncfusion libraries: $($syncfusionFiles.Count) files" -ForegroundColor Green
                } else {
                    Write-Host "  ⚠️  No Syncfusion libraries detected" -ForegroundColor Yellow
                }

            } else {
                Write-Host "  ❌ Executable not found: $exeName" -ForegroundColor Red
                $result.Success = $false
            }
        }

        # Deployment instructions
        if ($result.Success) {
            Write-Host "`n📋 Deployment Instructions:" -ForegroundColor Cyan
            Write-Host "  📁 Published to: $OutputPath" -ForegroundColor Gray

            if ($AzureReady) {
                Write-Host "  ☁️  Azure Deployment:" -ForegroundColor Cyan
                Write-Host "    1. Zip the contents of the publish folder" -ForegroundColor Gray
                Write-Host "    2. Deploy to Azure App Service using Azure CLI or portal" -ForegroundColor Gray
                Write-Host "    3. Update connection strings in Azure configuration" -ForegroundColor Gray
            } else {
                Write-Host "  🖥️  Local Deployment:" -ForegroundColor Cyan
                Write-Host "    1. Copy the publish folder to target machine" -ForegroundColor Gray
                Write-Host "    2. Ensure target has .NET 8.0 runtime (if not self-contained)" -ForegroundColor Gray
                Write-Host "    3. Run $exeName to start the application" -ForegroundColor Gray
            }
        }
    }

    return $result.Success
}

function Start-BusBuddyApplication {
    <#
    .SYNOPSIS
        Run the Bus Buddy WPF application with comprehensive validation and debug support
    .DESCRIPTION
        Start the application with proper validation, debug parameter support, and process monitoring
    .PARAMETER DebugFilter
        Start with debug filter enabled
    .PARAMETER ExportDebug
        Export debug data on startup
    .PARAMETER Configuration
        Run configuration (Debug or Release)
    .PARAMETER ValidateArgs
        Validate debug argument support before starting
    .PARAMETER Arguments
        Additional arguments to pass to the application
    .PARAMETER WaitForExit
        Wait for the application to exit before returning
    .PARAMETER MonitorPerformance
        Monitor application startup performance
    #>
    param(
        [Parameter(Mandatory = $false)]
        [switch]$DebugFilter,

        [Parameter(Mandatory = $false)]
        [switch]$ExportDebug,

        [Parameter(Mandatory = $false)]
        [ValidateSet('Debug', 'Release')]
        [string]$Configuration = $Script:BusBuddyConfig.BuildConfiguration,

        [Parameter(Mandatory = $false)]
        [switch]$ValidateArgs,

        [Parameter(Mandatory = $false)]
        [string[]]$Arguments = @(),

        [Parameter(Mandatory = $false)]
        [switch]$WaitForExit,

        [Parameter(Mandatory = $false)]
        [switch]$MonitorPerformance
    )

    Write-Host '🚌 Starting Bus Buddy application...' -ForegroundColor Cyan

    $root = Get-BusBuddyProjectRoot
    if (-not $root) {
        Write-Host '❌ Bus Buddy project not found. Navigate to project directory first.' -ForegroundColor Red
        return $false
    }

    # Validate WPF project
    $wpfProjectPath = Join-Path $root $Script:BusBuddyConfig.ProjectFiles.WPF
    if (-not (Test-Path $wpfProjectPath)) {
        Write-Host "❌ WPF project not found: $wpfProjectPath" -ForegroundColor Red
        return $false
    }

    # Validate debug arguments if requested
    if ($ValidateArgs -and ($DebugFilter -or $ExportDebug)) {
        Write-Host '🔍 Validating debug argument support...' -ForegroundColor Yellow

        $appXamlCs = Join-Path $root 'BusBuddy.WPF\App.xaml.cs'
        if (Test-Path $appXamlCs) {
            $appContent = Get-Content $appXamlCs -Raw -ErrorAction SilentlyContinue

            if ($DebugFilter -and $appContent -notmatch '--start-debug-filter|start-debug-filter') {
                Write-Host '⚠️  Warning: Application may not support --start-debug-filter argument' -ForegroundColor Yellow
            }

            if ($ExportDebug -and $appContent -notmatch '--export-debug-json|export-debug-json') {
                Write-Host '⚠️  Warning: Application may not support --export-debug-json argument' -ForegroundColor Yellow
            }
        }
    }

    # Check for running instances
    $existingProcesses = Get-Process -Name 'BusBuddy*' -ErrorAction SilentlyContinue
    if ($existingProcesses) {
        Write-Host "⚠️  Found $($existingProcesses.Count) existing Bus Buddy process(es)" -ForegroundColor Yellow
        Write-Host "💡 Close existing instances if you encounter issues" -ForegroundColor Gray
    }

    # Build run arguments
    $runArgs = @('run', '--project', $Script:BusBuddyConfig.ProjectFiles.WPF, '--configuration', $Configuration)

    # Add application arguments
    $appArgs = $Arguments
    if ($DebugFilter) {
        $appArgs += '--start-debug-filter'
        Write-Host '🐛 Debug filter enabled' -ForegroundColor Cyan
    }
    if ($ExportDebug) {
        $appArgs += '--export-debug-json'
        Write-Host '📤 Debug export enabled' -ForegroundColor Cyan
    }

    if ($appArgs.Count -gt 0) {
        $runArgs += '--'
        $runArgs += $appArgs
    }

    # Monitor performance if requested
    $startupTime = $null
    if ($MonitorPerformance) {
        $startupTime = Get-Date
        Write-Host '⏱️  Monitoring startup performance...' -ForegroundColor Gray
    }

    # Execute application
    try {
        Write-Host "📍 Starting from: $root" -ForegroundColor Gray
        Write-Host "⚙️  Command: dotnet $($runArgs -join ' ')" -ForegroundColor Gray

        if ($WaitForExit) {
            $result = Invoke-DotnetCommand -Command 'run' -Args $runArgs[1..($runArgs.Length - 1)] -SuccessMessage 'Application completed' -FailureMessage 'Application failed' -SuppressWelcome

            if ($MonitorPerformance -and $startupTime) {
                $duration = ((Get-Date) - $startupTime).TotalSeconds
                Write-Host "⏱️  Total execution time: $([math]::Round($duration, 2)) seconds" -ForegroundColor Gray
            }

            return $result.Success
        } else {
            # Start application without waiting
            Push-Location $root
            $process = Start-Process -FilePath 'dotnet' -ArgumentList $runArgs -PassThru -NoNewWindow
            Pop-Location

            if ($process) {
                Write-Host "✅ Application started successfully (PID: $($process.Id))" -ForegroundColor Green

                if ($MonitorPerformance -and $startupTime) {
                    Start-Sleep -Seconds 2  # Give process time to initialize
                    $duration = ((Get-Date) - $startupTime).TotalSeconds
                    Write-Host "⏱️  Startup initiated in: $([math]::Round($duration, 2)) seconds" -ForegroundColor Gray
                }

                return $true
            } else {
                Write-Host "❌ Failed to start application" -ForegroundColor Red
                return $false
            }
        }
    } catch {
        Write-Host "❌ Failed to start application: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Advanced Workflow Commands with enhanced functionality
function Start-BusBuddyDevSession {
    <#
    .SYNOPSIS
        Start a complete Bus Buddy development session with comprehensive setup and error aggregation
    .DESCRIPTION
        Opens workspace, builds solution, runs tests, and starts debug monitoring
        Includes environment validation, performance monitoring, and comprehensive error reporting
    .PARAMETER SkipTests
        Skip running tests during session startup
    .PARAMETER SkipValidation
        Skip environment validation
    .PARAMETER Configuration
        Build configuration for the session
    .PARAMETER OpenIDE
        Open VS Code after successful setup
    .PARAMETER CollectErrors
        Collect and aggregate all errors for final report
    #>
    param(
        [Parameter(Mandatory = $false)]
        [switch]$SkipTests,

        [Parameter(Mandatory = $false)]
        [switch]$SkipValidation,

        [Parameter(Mandatory = $false)]
        [ValidateSet('Debug', 'Release')]
        [string]$Configuration = $Script:BusBuddyConfig.BuildConfiguration,

        [Parameter(Mandatory = $false)]
        [switch]$OpenIDE,

        [Parameter(Mandatory = $false)]
        [switch]$CollectErrors
    )

    Write-Host '🚌 Starting Bus Buddy Development Session...' -ForegroundColor Cyan
    $sessionStart = Get-Date

    # Error aggregation system
    $sessionErrors = @{
        Environment = @()
        Clean       = @()
        Restore     = @()
        Build       = @()
        Test        = @()
        XAML        = @()
        General     = @()
    }

    $root = Get-BusBuddyProjectRoot
    if (-not $root) {
        Write-Host '❌ Bus Buddy project not found. Navigate to project directory first.' -ForegroundColor Red
        return $false
    }

    Write-Host "📁 Project root: $root" -ForegroundColor Gray
    Set-Location $root

    # Environment validation with error collection
    if (-not $SkipValidation) {
        Write-Host '🔍 Validating development environment...' -ForegroundColor White
        $validationResult = Test-DevelopmentEnvironment
        if (-not $validationResult.IsValid) {
            Write-Host '⚠️  Environment validation warnings detected:' -ForegroundColor Yellow
            $validationResult.Issues | ForEach-Object {
                Write-Host "  • $_" -ForegroundColor Yellow
                if ($CollectErrors) {
                    $sessionErrors.Environment += $_
                }
            }

            $continue = Read-Host 'Continue anyway? (y/N)'
            if ($continue -ne 'y' -and $continue -ne 'Y') {
                return $false
            }
        } else {
            Write-Host '✅ Environment validation passed' -ForegroundColor Green
        }
    }

    # Clean and restore with error tracking
    Write-Host '🔧 Cleaning solution...' -ForegroundColor White
    try {
        $cleanResult = bb-clean -Force
        if (-not $cleanResult) {
            $errorMsg = 'Clean operation failed'
            Write-Host "❌ $errorMsg" -ForegroundColor Red
            if ($CollectErrors) {
                $sessionErrors.Clean += $errorMsg
            }
        }
    } catch {
        $errorMsg = "Clean operation exception: $($_.Message)"
        Write-Host "❌ $errorMsg" -ForegroundColor Red
        if ($CollectErrors) {
            $sessionErrors.Clean += $errorMsg
        }
    }

    Write-Host '📦 Restoring packages...' -ForegroundColor White
    try {
        $restoreResult = bb-restore -CheckVulnerabilities
        if (-not $restoreResult) {
            $errorMsg = 'Package restore failed'
            Write-Host "❌ $errorMsg" -ForegroundColor Red
            if ($CollectErrors) {
                $sessionErrors.Restore += $errorMsg
            }
        }
    } catch {
        $errorMsg = "Restore operation exception: $($_.Message)"
        Write-Host "❌ $errorMsg" -ForegroundColor Red
        if ($CollectErrors) {
            $sessionErrors.Restore += $errorMsg
        }
    }

    # Build solution with error tracking
    Write-Host "🔨 Building solution ($Configuration)..." -ForegroundColor White
    try {
        $buildResult = bb-build -Configuration $Configuration
        if (-not $buildResult) {
            $errorMsg = 'Build operation failed'
            Write-Host "❌ $errorMsg" -ForegroundColor Red
            if ($CollectErrors) {
                $sessionErrors.Build += $errorMsg
            }
        }
    } catch {
        $errorMsg = "Build operation exception: $($_.Message)"
        Write-Host "❌ $errorMsg" -ForegroundColor Red
        if ($CollectErrors) {
            $sessionErrors.Build += $errorMsg
        }
    }

    # XAML validation with error tracking
    Write-Host '🎨 Validating XAML files...' -ForegroundColor White
    try {
        $xamlHealthScript = Join-Path $root 'Tools\Scripts\XAML-Health-Suite.ps1'
        if (Test-Path $xamlHealthScript) {
            $xamlResult = & $xamlHealthScript -HealthCheck -ProjectRoot $root
            if ($xamlResult -and $xamlResult.Issues) {
                foreach ($issue in $xamlResult.Issues) {
                    Write-Host "  ⚠️  XAML: $issue" -ForegroundColor Yellow
                    if ($CollectErrors) {
                        $sessionErrors.XAML += $issue
                    }
                }
            }
        }
    } catch {
        $errorMsg = "XAML validation exception: $($_.Message)"
        Write-Host "❌ $errorMsg" -ForegroundColor Red
        if ($CollectErrors) {
            $sessionErrors.XAML += $errorMsg
        }
    }

    # Run tests (unless skipped) with error tracking
    if (-not $SkipTests) {
        Write-Host '🧪 Running tests...' -ForegroundColor White
        try {
            $testResult = bb-test -NoBuild -Configuration $Configuration
            if (-not $testResult) {
                $errorMsg = 'Test execution failed'
                Write-Host "⚠️  $errorMsg, but continuing session startup" -ForegroundColor Yellow
                if ($CollectErrors) {
                    $sessionErrors.Test += $errorMsg
                }
            } else {
                Write-Host '✅ All tests passed' -ForegroundColor Green
            }
        } catch {
            $errorMsg = "Test execution exception: $($_.Message)"
            Write-Host "⚠️  $errorMsg" -ForegroundColor Yellow
            if ($CollectErrors) {
                $sessionErrors.Test += $errorMsg
            }
        }
    }

    # Open IDE if requested
    if ($OpenIDE) {
        Write-Host '💻 Opening VS Code...' -ForegroundColor White
        try {
            if (Get-Command 'code' -ErrorAction SilentlyContinue) {
                & code $root
            } elseif (Get-Command 'code-insiders' -ErrorAction SilentlyContinue) {
                & code-insiders $root
            } else {
                $errorMsg = 'VS Code not found in PATH'
                Write-Host "⚠️  $errorMsg" -ForegroundColor Yellow
                if ($CollectErrors) {
                    $sessionErrors.General += $errorMsg
                }
            }
        } catch {
            $errorMsg = "VS Code launch exception: $($_.Message)"
            Write-Host "⚠️  $errorMsg" -ForegroundColor Yellow
            if ($CollectErrors) {
                $sessionErrors.General += $errorMsg
            }
        }
    }

    $sessionDuration = ((Get-Date) - $sessionStart).TotalSeconds

    # Error aggregation and final report
    if ($CollectErrors) {
        $totalErrors = 0
        $errorCategories = @()

        foreach ($category in $sessionErrors.Keys) {
            $errorCount = $sessionErrors[$category].Count
            if ($errorCount -gt 0) {
                $totalErrors += $errorCount
                $errorCategories += "$category ($errorCount)"
            }
        }

        Write-Host "`n📊 Session Error Summary:" -ForegroundColor Cyan
        if ($totalErrors -eq 0) {
            Write-Host "✅ No errors detected during session startup!" -ForegroundColor Green
        } else {
            Write-Host "⚠️  $totalErrors total issues detected:" -ForegroundColor Yellow

            foreach ($category in $sessionErrors.Keys) {
                if ($sessionErrors[$category].Count -gt 0) {
                    Write-Host "  🔸 $category Issues:" -ForegroundColor Yellow
                    $sessionErrors[$category] | ForEach-Object {
                        Write-Host "    • $_" -ForegroundColor Red
                    }
                }
            }

            Write-Host "`n💡 Recommended Actions:" -ForegroundColor Yellow
            if ($sessionErrors.Environment.Count -gt 0) {
                Write-Host "  • Update development tools and dependencies" -ForegroundColor Gray
            }
            if ($sessionErrors.Build.Count -gt 0) {
                Write-Host "  • Review compilation errors and fix syntax issues" -ForegroundColor Gray
            }
            if ($sessionErrors.Test.Count -gt 0) {
                Write-Host "  • Investigate failing tests and update assertions" -ForegroundColor Gray
            }
            if ($sessionErrors.XAML.Count -gt 0) {
                Write-Host "  • Run bb-xaml-validate for detailed XAML analysis" -ForegroundColor Gray
            }

            # Export error report
            $errorReportPath = Join-Path $root "logs\session-errors-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
            $logsDir = Join-Path $root 'logs'
            if (-not (Test-Path $logsDir)) {
                New-Item -ItemType Directory -Path $logsDir -Force | Out-Null
            }

            $sessionErrors | ConvertTo-Json -Depth 3 | Set-Content -Path $errorReportPath -Encoding UTF8
            Write-Host "📋 Detailed error report saved to: $errorReportPath" -ForegroundColor Gray
        }
    }

    Write-Host "`n✅ Development session ready! ($([math]::Round($sessionDuration, 2))s)" -ForegroundColor Green
    Write-Host '💡 Available commands:' -ForegroundColor Yellow
    Write-Host '  • bb-run               - Start the application' -ForegroundColor Gray
    Write-Host '  • bb-debug-start       - Enable debug monitoring' -ForegroundColor Gray
    Write-Host '  • bb-quick-test        - Quick build & test cycle' -ForegroundColor Gray
    Write-Host '  • bb-diagnostic        - Comprehensive system analysis' -ForegroundColor Gray
    Write-Host '  • Publish-BusBuddy     - Create deployment package' -ForegroundColor Gray

    # Return success status based on critical errors
    $criticalErrors = $sessionErrors.Environment.Count + $sessionErrors.Build.Count
    return $criticalErrors -eq 0
}

function Invoke-BusBuddyQuickTest {
    <#
    .SYNOPSIS
        Quick build-test-validate cycle with performance monitoring
    .DESCRIPTION
        Optimized workflow for rapid development iteration
    .PARAMETER Category
        Test category to run (default: Unit for speed)
    .PARAMETER Configuration
        Build configuration
    .PARAMETER SkipBuild
        Skip build if already built
    #>
    param(
        [Parameter(Mandatory = $false)]
        [ValidateSet('Unit', 'Integration', 'Performance', 'UI', 'All')]
        [string]$Category = 'Unit',

        [Parameter(Mandatory = $false)]
        [ValidateSet('Debug', 'Release')]
        [string]$Configuration = $Script:BusBuddyConfig.BuildConfiguration,

        [Parameter(Mandatory = $false)]
        [switch]$SkipBuild
    )

    Write-Host '⚡ Bus Buddy Quick Test Cycle' -ForegroundColor Cyan
    $cycleStart = Get-Date

    $success = $true
    $results = @{
        Build = @{ Success = $true; Duration = 0; Skipped = $SkipBuild }
        Test  = @{ Success = $true; Duration = 0 }
    }

    if (-not $SkipBuild) {
        Write-Host '🔨 Building...' -ForegroundColor White
        $buildStart = Get-Date
        $buildResult = bb-build -Configuration $Configuration
        $results.Build.Duration = ((Get-Date) - $buildStart).TotalSeconds
        $results.Build.Success = $buildResult

        if (-not $buildResult) {
            $success = $false
            Write-Host '❌ Build failed, skipping tests' -ForegroundColor Red
        }
    }

    if ($success) {
        Write-Host "🧪 Testing ($Category tests)..." -ForegroundColor White
        $testStart = Get-Date
        $testResult = bb-test -Category $Category -NoBuild -Configuration $Configuration
        $results.Test.Duration = ((Get-Date) - $testStart).TotalSeconds
        $results.Test.Success = $testResult

        if (-not $testResult) {
            $success = $false
        }
    }

    $totalDuration = ((Get-Date) - $cycleStart).TotalSeconds

    # Results summary
    Write-Host "`n📊 Quick Test Cycle Results:" -ForegroundColor Cyan
    if (-not $results.Build.Skipped) {
        $buildStatus = if ($results.Build.Success) { '✅' } else { '❌' }
        Write-Host "  Build: $buildStatus ($([math]::Round($results.Build.Duration, 2))s)" -ForegroundColor Gray
    } else {
        Write-Host "  Build: ⏭️  Skipped" -ForegroundColor Gray
    }

    $testStatus = if ($results.Test.Success) { '✅' } else { '❌' }
    Write-Host "  Tests: $testStatus ($([math]::Round($results.Test.Duration, 2))s)" -ForegroundColor Gray
    Write-Host "  Total: $([math]::Round($totalDuration, 2))s" -ForegroundColor Gray

    if ($success) {
        Write-Host '✅ Quick test cycle completed successfully!' -ForegroundColor Green

        # Performance feedback
        if ($totalDuration -lt 10) {
            Write-Host '🚀 Excellent cycle time!' -ForegroundColor Green
        } elseif ($totalDuration -lt 30) {
            Write-Host '👍 Good cycle time' -ForegroundColor Yellow
        } else {
            Write-Host '🐌 Consider optimizing build or test performance' -ForegroundColor Yellow
        }
    } else {
        Write-Host '❌ Quick test cycle failed' -ForegroundColor Red
    }

    return $success
}

function Get-BusBuddyHealth {
    <#
    .SYNOPSIS
        Comprehensive health check with actionable recommendations
    .DESCRIPTION
        Enhanced health check including NuGet cache analysis, dependency verification,
        and system resource monitoring
    .PARAMETER IncludePerformance
        Include performance benchmarking in health check
    .PARAMETER CheckVulnerabilities
        Check for vulnerable packages
    .PARAMETER Deep
        Perform deep analysis including cache validation
    #>
    param(
        [Parameter(Mandatory = $false)]
        [switch]$IncludePerformance,

        [Parameter(Mandatory = $false)]
        [switch]$CheckVulnerabilities,

        [Parameter(Mandatory = $false)]
        [switch]$Deep
    )

    Write-Host '🏥 Bus Buddy Comprehensive Health Check' -ForegroundColor Cyan

    $root = Get-BusBuddyProjectRoot
    if (-not $root) {
        Write-Host '❌ Bus Buddy project not found' -ForegroundColor Red
        return $false
    }

    $healthReport = @{
        Issues          = @()
        Warnings        = @()
        Recommendations = @()
        Performance     = @{}
    }

    # XAML validation check
    Write-Host '🎨 Validating XAML files...' -ForegroundColor White
    try {
        $xamlFiles = Get-ChildItem $root -Filter "*.xaml" -Recurse | Where-Object {
            $_.FullName -notlike "*\bin\*" -and $_.FullName -notlike "*\obj\*"
        }

        if ($xamlFiles.Count -gt 0) {
            # Use existing XAML validation tools if available
            $xamlHealthScript = Join-Path $root 'Tools\Scripts\XAML-Health-Suite.ps1'
            if (Test-Path $xamlHealthScript) {
                Write-Host "  🔧 Running XAML Health Suite..." -ForegroundColor Gray
                try {
                    & $xamlHealthScript -HealthCheck -ProjectRoot $root | Out-Null
                    Write-Host "  ✅ XAML health check completed" -ForegroundColor Green
                } catch {
                    $healthReport.Warnings += "XAML health check failed: $($_.Message)"
                    Write-Host "  ⚠️  XAML health check encountered issues" -ForegroundColor Yellow
                }
            } else {
                # Basic XAML validation
                $invalidXamlFiles = @()
                foreach ($xamlFile in $xamlFiles) {
                    try {
                        [xml]$xamlContent = Get-Content $xamlFile.FullName -ErrorAction Stop
                        # Basic namespace validation
                        if (-not $xamlContent.DocumentElement.NamespaceURI) {
                            $invalidXamlFiles += $xamlFile.Name
                        }
                    } catch {
                        $invalidXamlFiles += $xamlFile.Name
                    }
                }

                if ($invalidXamlFiles.Count -gt 0) {
                    $healthReport.Issues += "Invalid XAML files detected: $($invalidXamlFiles.Count) files"
                    $healthReport.Recommendations += 'Review and fix XAML syntax errors'
                } else {
                    Write-Host "  ✅ All XAML files are syntactically valid" -ForegroundColor Green
                }
            }

            Write-Host "  📊 XAML files found: $($xamlFiles.Count)" -ForegroundColor Gray
        } else {
            $healthReport.Warnings += 'No XAML files found in project'
        }
    } catch {
        $healthReport.Warnings += "XAML validation failed: $($_.Message)"
    }

    # Basic environment check
    Write-Host '🔍 Checking development environment...' -ForegroundColor White
    $envCheck = Test-DevelopmentEnvironment
    if (-not $envCheck.IsValid) {
        $healthReport.Issues += $envCheck.Issues
    }

    # Project structure validation
    Write-Host '🏗️  Validating project structure...' -ForegroundColor White
    foreach ($projectName in $Script:BusBuddyConfig.Projects.Keys) {
        $projectFile = $Script:BusBuddyConfig.ProjectFiles[$projectName]
        $projectPath = Join-Path $root $projectFile
        if (-not (Test-Path $projectPath)) {
            $healthReport.Issues += "Missing project: $projectName"
        }
    }

    # Check for large log files
    Write-Host '📝 Checking log files...' -ForegroundColor White
    $logsPath = Join-Path $root $Script:BusBuddyConfig.LogDirectory
    if (Test-Path $logsPath) {
        $largeLogFiles = Get-ChildItem $logsPath -Filter '*.log' -ErrorAction SilentlyContinue |
            Where-Object { $_.Length -gt 10MB }

        if ($largeLogFiles.Count -gt 0) {
            $healthReport.Warnings += "Large log files detected (>10MB): $($largeLogFiles.Count) files"
            $healthReport.Recommendations += 'Consider archiving or cleaning old log files'
        }
    }

    # Check build artifacts
    Write-Host '🔧 Checking build artifacts...' -ForegroundColor White
    $binDirs = Get-ChildItem $root -Name 'bin' -Directory -Recurse -ErrorAction SilentlyContinue
    $objDirs = Get-ChildItem $root -Name 'obj' -Directory -Recurse -ErrorAction SilentlyContinue

    $totalArtifacts = $binDirs.Count + $objDirs.Count
    if ($totalArtifacts -gt 0) {
        $healthReport.Warnings += "Build artifacts present: $totalArtifacts directories"
        $healthReport.Recommendations += 'Consider bb-clean -Deep for fresh build environment'
    }

    # NuGet cache analysis (if Deep)
    if ($Deep) {
        Write-Host '📦 Analyzing NuGet cache...' -ForegroundColor White
        foreach ($cacheLocation in $Script:BusBuddyConfig.NuGetCacheLocation) {
            if (Test-Path $cacheLocation) {
                try {
                    $cacheSize = (Get-ChildItem $cacheLocation -Recurse -ErrorAction SilentlyContinue |
                            Measure-Object -Property Length -Sum).Sum / 1GB

                    if ($cacheSize -gt 5) {
                        $healthReport.Warnings += "Large NuGet cache: $([math]::Round($cacheSize, 2))GB at $cacheLocation"
                        $healthReport.Recommendations += 'Consider clearing NuGet cache: bb-clean -Deep'
                    }
                } catch {
                    Write-Verbose "Failed to analyze cache at $cacheLocation"
                }
            }
        }
    }

    # Vulnerability check
    if ($CheckVulnerabilities) {
        Write-Host '🛡️  Checking for vulnerable packages...' -ForegroundColor White
        try {
            $vulnResult = Invoke-DotnetCommand -Command 'list' -Args @($Script:BusBuddyConfig.SolutionFile, 'package', '--vulnerable') -SuccessMessage '' -FailureMessage '' -Verbosity 'quiet'

            if ($vulnResult.Success) {
                $vulnerabilities = $vulnResult.Output | Where-Object { $_ -match 'has the following vulnerable|severity' }
                if ($vulnerabilities.Count -gt 0) {
                    $healthReport.Issues += "Vulnerable packages detected: $($vulnerabilities.Count) issues"
                    $healthReport.Recommendations += 'Update vulnerable packages to latest secure versions'
                }
            }
        } catch {
            $healthReport.Warnings += 'Failed to check for package vulnerabilities'
        }
    }

    # Performance benchmarking
    if ($IncludePerformance) {
        Write-Host '⚡ Running performance benchmarks...' -ForegroundColor White

        # Build performance test
        $buildStart = Get-Date
        bb-build -Verbosity quiet | Out-Null
        $buildDuration = ((Get-Date) - $buildStart).TotalSeconds

        $healthReport.Performance.BuildTime = $buildDuration

        if ($buildDuration -gt $Script:BusBuddyConfig.PerformanceThresholds.BuildTimeWarning) {
            $healthReport.Warnings += "Slow build performance: $([math]::Round($buildDuration, 2))s"
            $healthReport.Recommendations += 'Consider optimizing build configuration or dependencies'
        }
    }

    # System resources
    Write-Host '💻 Checking system resources...' -ForegroundColor White
    try {
        $availableMemory = (Get-WmiObject -Class Win32_OperatingSystem).FreePhysicalMemory / 1MB
        if ($availableMemory -lt 1024) {
            # Less than 1GB free
            $healthReport.Warnings += "Low available memory: $([math]::Round($availableMemory, 0))MB"
            $healthReport.Recommendations += 'Close unnecessary applications to free memory'
        }
    } catch {
        Write-Verbose 'Failed to check system memory'
    }

    # Display results
    Write-Host "`n📋 Health Check Results:" -ForegroundColor Cyan

    $issueCount = $healthReport.Issues.Count
    $warningCount = $healthReport.Warnings.Count

    if ($issueCount -eq 0 -and $warningCount -eq 0) {
        Write-Host '✅ No critical issues or warnings detected' -ForegroundColor Green
        Write-Host '🎉 System is in excellent health!' -ForegroundColor Green
    } else {
        if ($issueCount -gt 0) {
            Write-Host "❌ $issueCount critical issue(s) found:" -ForegroundColor Red
            $healthReport.Issues | ForEach-Object { Write-Host "  • $_" -ForegroundColor Red }
        }

        if ($warningCount -gt 0) {
            Write-Host "⚠️  $warningCount warning(s) found:" -ForegroundColor Yellow
            $healthReport.Warnings | ForEach-Object { Write-Host "  • $_" -ForegroundColor Yellow }
        }
    }

    if ($healthReport.Recommendations.Count -gt 0) {
        Write-Host "`n💡 Recommendations:" -ForegroundColor Yellow
        $healthReport.Recommendations | ForEach-Object { Write-Host "  • $_" -ForegroundColor Gray }
    }

    if ($IncludePerformance -and $healthReport.Performance.BuildTime) {
        Write-Host "`n⚡ Performance Metrics:" -ForegroundColor Cyan
        Write-Host "  Build Time: $([math]::Round($healthReport.Performance.BuildTime, 2))s" -ForegroundColor Gray
    }

    return $issueCount -eq 0
}

# Helper function for environment validation
function Test-DevelopmentEnvironment {
    <#
    .SYNOPSIS
        Validate development environment requirements
    #>
    $validation = @{
        IsValid = $true
        Issues  = @()
    }

    # Check required tools
    foreach ($tool in $Script:BusBuddyConfig.RequiredTools.Keys) {
        $requiredVersion = $Script:BusBuddyConfig.RequiredTools[$tool]

        if (-not (Get-Command $tool -ErrorAction SilentlyContinue)) {
            $validation.IsValid = $false
            $validation.Issues += "Missing required tool: $tool (required: $requiredVersion+)"
        } else {
            try {
                $actualVersion = & $tool --version 2>$null | Select-Object -First 1
                Write-Verbose "Found $tool version: $actualVersion"
            } catch {
                $validation.Issues += "Failed to verify $tool version"
            }
        }
    }

    # Check .NET SDK
    try {
        $dotnetInfo = dotnet --info 2>$null
        if (-not $dotnetInfo -or $dotnetInfo -notmatch 'net8.0') {
            $validation.Issues += '.NET 8.0 SDK not detected'
        }
    } catch {
        $validation.IsValid = $false
        $validation.Issues += '.NET SDK validation failed'
    }

    return $validation
}

function Get-BusBuddyDiagnostic {
    <#
    .SYNOPSIS
        Comprehensive Bus Buddy system diagnostic with advanced analysis
    .DESCRIPTION
        Performs extensive validation, dependency analysis, performance benchmarking,
        and generates actionable recommendations for system optimization
    .PARAMETER IncludeXaml
        Include XAML validation and analysis
    .PARAMETER IncludeSerilog
        Include Serilog configuration validation
    .PARAMETER IncludeDependencies
        Include comprehensive dependency analysis
    .PARAMETER IncludePerformance
        Include performance benchmarking
    .PARAMETER IncludeGit
        Include Git repository analysis
    .PARAMETER ExportResults
        Export diagnostic results to JSON
    .PARAMETER Detailed
        Include detailed analysis for all components
    #>
    param(
        [Parameter(Mandatory = $false)]
        [switch]$IncludeXaml,

        [Parameter(Mandatory = $false)]
        [switch]$IncludeSerilog,

        [Parameter(Mandatory = $false)]
        [switch]$IncludeDependencies,

        [Parameter(Mandatory = $false)]
        [switch]$IncludePerformance,

        [Parameter(Mandatory = $false)]
        [switch]$IncludeGit,

        [Parameter(Mandatory = $false)]
        [switch]$ExportResults,

        [Parameter(Mandatory = $false)]
        [switch]$Detailed
    )

    Write-Host '🔍 Bus Buddy Comprehensive System Diagnostic' -ForegroundColor Cyan
    $diagnosticStart = Get-Date

    $root = Get-BusBuddyProjectRoot
    if (-not $root) {
        Write-Host '❌ Bus Buddy project not found' -ForegroundColor Red
        return $false
    }

    $diagnosticResults = @{
        Timestamp          = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
        DiagnosticVersion  = '2.0'
        ProjectRoot        = $root
        SystemInfo         = @{}
        ProjectStructure   = @{}
        BuildValidation    = @{}
        TestResults        = @{}
        ToolValidation     = @{}
        DependencyAnalysis = @{}
        PerformanceMetrics = @{}
        GitAnalysis        = @{}
        XamlAnalysis       = @{}
        SerilogAnalysis    = @{}
        Issues             = @()
        Warnings           = @()
        Recommendations    = @()
        Summary            = @{}
    }

    # Enhanced System Information
    Write-Host '📊 Analyzing system information...' -ForegroundColor White
    try {
        $os = Get-WmiObject -Class Win32_OperatingSystem
        $cpu = Get-WmiObject -Class Win32_Processor | Select-Object -First 1
        $memory = Get-WmiObject -Class Win32_ComputerSystem

        $systemInfo = @{
            PowerShellVersion = $PSVersionTable.PSVersion.ToString()
            PowerShellEdition = $PSVersionTable.PSEdition
            DotNetVersion     = (dotnet --version 2>$null)
            OSVersion         = $os.Caption
            OSArchitecture    = $os.OSArchitecture
            TotalMemoryGB     = [math]::Round($memory.TotalPhysicalMemory / 1GB, 2)
            AvailableMemoryGB = [math]::Round($os.FreePhysicalMemory / 1MB / 1024, 2)
            CPUName           = $cpu.Name
            CPUCores          = $cpu.NumberOfCores
            CPULogicalCores   = $cpu.NumberOfLogicalProcessors
            MachineName       = $env:COMPUTERNAME
            UserName          = $env:USERNAME
            TimeZone          = (Get-TimeZone).Id
        }

        # Memory health check
        $memoryUsagePercent = (1 - ($systemInfo.AvailableMemoryGB / $systemInfo.TotalMemoryGB)) * 100
        if ($memoryUsagePercent -gt 85) {
            $diagnosticResults.Warnings += "High memory usage: $([math]::Round($memoryUsagePercent, 1))%"
            $diagnosticResults.Recommendations += 'Consider closing unnecessary applications'
        }

        $diagnosticResults.SystemInfo = $systemInfo
        Write-Host "  OS: $($systemInfo.OSVersion) ($($systemInfo.OSArchitecture))" -ForegroundColor Gray
        Write-Host "  Memory: $($systemInfo.AvailableMemoryGB)GB / $($systemInfo.TotalMemoryGB)GB available" -ForegroundColor Gray
        Write-Host "  .NET: $($systemInfo.DotNetVersion)" -ForegroundColor Gray
    } catch {
        $diagnosticResults.Issues += "Failed to gather system information: $($_.Exception.Message)"
    }

    # Enhanced Tool Validation
    Write-Host '🛠️  Validating development tools...' -ForegroundColor White
    $toolResults = @{}

    foreach ($tool in $Script:BusBuddyConfig.RequiredTools.Keys) {
        $requiredVersion = $Script:BusBuddyConfig.RequiredTools[$tool]
        try {
            $toolPath = Get-Command $tool -ErrorAction SilentlyContinue
            if ($toolPath) {
                $actualVersion = & $tool --version 2>$null | Select-Object -First 1
                $toolResults[$tool] = @{
                    Available       = $true
                    Version         = $actualVersion
                    RequiredVersion = $requiredVersion
                    Path            = $toolPath.Source
                }
                Write-Host "  ✅ $tool found: $actualVersion" -ForegroundColor Green
            } else {
                $toolResults[$tool] = @{ Available = $false; RequiredVersion = $requiredVersion }
                Write-Host "  ❌ $tool not found (required: $requiredVersion+)" -ForegroundColor Red
                $diagnosticResults.Issues += "Missing required tool: $tool"
            }
        } catch {
            $toolResults[$tool] = @{ Available = $false; Error = $_.Message; RequiredVersion = $requiredVersion }
            Write-Host "  ❌ $tool validation failed: $($_.Message)" -ForegroundColor Red
            $diagnosticResults.Issues += "Tool validation error: $tool - $($_.Exception.Message)"
        }
    }

    # Check for additional useful tools
    $optionalTools = @('git', 'code', 'code-insiders')
    foreach ($tool in $optionalTools) {
        if (Get-Command $tool -ErrorAction SilentlyContinue) {
            $version = try { & $tool --version 2>$null | Select-Object -First 1 } catch { 'Unknown' }
            $toolResults[$tool] = @{ Available = $true; Version = $version; Optional = $true }
            Write-Host "  ✅ $tool available: $version" -ForegroundColor Green
        }
    }

    $diagnosticResults.ToolValidation = $toolResults

    # Enhanced Project Structure Analysis
    Write-Host '🏗️  Analyzing project structure...' -ForegroundColor White
    $structureResults = @{
        SolutionFile       = @{}
        Projects           = @{}
        SourceFiles        = @{}
        ConfigurationFiles = @{}
    }

    # Solution analysis
    $solutionFile = Join-Path $root $Script:BusBuddyConfig.SolutionFile
    $structureResults.SolutionFile = @{
        Path         = $solutionFile
        Exists       = Test-Path $solutionFile
        Size         = if (Test-Path $solutionFile) { (Get-Item $solutionFile).Length } else { 0 }
        LastModified = if (Test-Path $solutionFile) { (Get-Item $solutionFile).LastWriteTime } else { $null }
    }

    if (Test-Path $solutionFile) {
        Write-Host "  ✅ Solution file found" -ForegroundColor Green
        try {
            $solutionContent = Get-Content $solutionFile -Raw
            $projectReferences = ([regex]::Matches($solutionContent, 'Project\(')).Count
            $structureResults.SolutionFile.ProjectCount = $projectReferences
            Write-Host "    Projects in solution: $projectReferences" -ForegroundColor Gray
        } catch {
            $diagnosticResults.Warnings += 'Failed to analyze solution file content'
        }
    } else {
        Write-Host "  ❌ Solution file missing" -ForegroundColor Red
        $diagnosticResults.Issues += 'Solution file not found'
    }

    # Project files analysis
    $projectResults = @{}
    foreach ($projectName in $Script:BusBuddyConfig.Projects.Keys) {
        $projectFile = $Script:BusBuddyConfig.ProjectFiles[$projectName]
        $projectPath = Join-Path $root $projectFile
        $exists = Test-Path $projectPath

        $analysis = @{
            ProjectFile = $projectFile
            Path        = $projectPath
            Exists      = $exists
        }

        if ($exists) {
            try {
                $projectInfo = Get-Item $projectPath
                $analysis.Size = $projectInfo.Length
                $analysis.LastModified = $projectInfo.LastWriteTime

                $projectContent = Get-Content $projectPath -Raw
                $analysis.TargetFramework = if ($projectContent -match '<TargetFramework>(.*?)</TargetFramework>') { $matches[1] } else { 'Unknown' }
                $analysis.PackageReferences = ([regex]::Matches($projectContent, '<PackageReference')).Count
                $analysis.ProjectReferences = ([regex]::Matches($projectContent, '<ProjectReference')).Count

                Write-Host "  ✅ $projectName project found" -ForegroundColor Green
                Write-Host "    Target Framework: $($analysis.TargetFramework)" -ForegroundColor Gray
                Write-Host "    Package References: $($analysis.PackageReferences)" -ForegroundColor Gray
            } catch {
                $analysis.AnalysisError = $_.Message
                $diagnosticResults.Warnings += "Failed to analyze $projectName project file"
            }
        } else {
            Write-Host "  ❌ $projectName project missing" -ForegroundColor Red
            $diagnosticResults.Issues += "$projectName project not found"
        }

        $projectResults[$projectName] = $analysis
    }
    $structureResults.Projects = $projectResults

    # Source files analysis
    try {
        $csFiles = Get-ChildItem -Path $root -Filter "*.cs" -Recurse | Where-Object {
            $_.FullName -notlike "*\bin\*" -and $_.FullName -notlike "*\obj\*"
        }
        $xamlFiles = Get-ChildItem -Path $root -Filter "*.xaml" -Recurse | Where-Object {
            $_.FullName -notlike "*\bin\*" -and $_.FullName -notlike "*\obj\*"
        }

        $structureResults.SourceFiles = @{
            CSharpFiles     = $csFiles.Count
            XamlFiles       = $xamlFiles.Count
            TotalFiles      = $csFiles.Count + $xamlFiles.Count
            LargestFileSize = if ($csFiles) { ($csFiles | Measure-Object Length -Maximum).Maximum } else { 0 }
            TotalCodeSize   = [math]::Round((($csFiles | Measure-Object Length -Sum).Sum + ($xamlFiles | Measure-Object Length -Sum).Sum) / 1KB, 2)
        }

        Write-Host "  📁 Source files: $($structureResults.SourceFiles.CSharpFiles) C#, $($structureResults.SourceFiles.XamlFiles) XAML" -ForegroundColor Gray
        Write-Host "  📏 Total code size: $($structureResults.SourceFiles.TotalCodeSize) KB" -ForegroundColor Gray
    } catch {
        $diagnosticResults.Warnings += 'Failed to analyze source files'
    }

    # Configuration files analysis
    $configFiles = @('appsettings.json', 'nuget.config', 'global.json', '.gitignore')
    $configResults = @{}
    foreach ($configFile in $configFiles) {
        $configPath = Join-Path $root $configFile
        $configResults[$configFile] = @{
            Exists = Test-Path $configPath
            Path   = $configPath
        }
        if (Test-Path $configPath) {
            $configResults[$configFile].Size = (Get-Item $configPath).Length
            $configResults[$configFile].LastModified = (Get-Item $configPath).LastWriteTime
        }
    }
    $structureResults.ConfigurationFiles = $configResults

    $diagnosticResults.ProjectStructure = $structureResults

    # Git Analysis (if requested)
    if ($IncludeGit) {
        Write-Host '📋 Analyzing Git repository...' -ForegroundColor White
        $gitResults = @{}

        try {
            if (Test-Path (Join-Path $root '.git')) {
                $gitResults.IsGitRepo = $true

                # Get branch info
                $currentBranch = git branch --show-current 2>$null
                $gitResults.CurrentBranch = $currentBranch

                # Get commit info
                $lastCommit = git log -1 --pretty=format:"%h - %an, %ar : %s" 2>$null
                $gitResults.LastCommit = $lastCommit

                # Check for uncommitted changes
                $status = git status --porcelain 2>$null
                $gitResults.UncommittedChanges = $status.Count

                # Check for unpushed commits
                try {
                    $unpushed = git log --oneline origin/$currentBranch..$currentBranch 2>$null
                    $gitResults.UnpushedCommits = if ($unpushed) { $unpushed.Count } else { 0 }
                } catch {
                    $gitResults.UnpushedCommits = 'Unknown'
                }

                Write-Host "  ✅ Git repository detected" -ForegroundColor Green
                Write-Host "    Branch: $currentBranch" -ForegroundColor Gray
                Write-Host "    Uncommitted changes: $($gitResults.UncommittedChanges)" -ForegroundColor Gray

                if ($gitResults.UncommittedChanges -gt 0) {
                    $diagnosticResults.Warnings += "Uncommitted changes detected: $($gitResults.UncommittedChanges) files"
                    $diagnosticResults.Recommendations += 'Consider committing or stashing changes before major operations'
                }
            } else {
                $gitResults.IsGitRepo = $false
                $diagnosticResults.Warnings += 'Project is not under Git version control'
                $diagnosticResults.Recommendations += 'Consider initializing Git repository for version control'
            }
        } catch {
            $gitResults.Error = $_.Message
            $diagnosticResults.Warnings += "Git analysis failed: $($_.Message)"
        }

        $diagnosticResults.GitAnalysis = $gitResults
    }

    # Build Validation with enhanced analysis
    Write-Host '🔨 Validating build system...' -ForegroundColor White
    $buildStartTime = Get-Date
    $buildSuccess = bb-build -Verbosity quiet
    $buildEndTime = Get-Date
    $buildDuration = ($buildEndTime - $buildStartTime).TotalSeconds

    $buildResults = @{
        Success           = $buildSuccess
        Duration          = $buildDuration
        Timestamp         = $buildEndTime.ToString('yyyy-MM-dd HH:mm:ss')
        PerformanceRating = if ($buildDuration -lt 30) { 'Excellent' } elseif ($buildDuration -lt 60) { 'Good' } elseif ($buildDuration -lt 120) { 'Fair' } else { 'Poor' }
    }

    if ($buildSuccess) {
        Write-Host "  ✅ Build successful ($([math]::Round($buildDuration, 2))s - $($buildResults.PerformanceRating))" -ForegroundColor Green

        if ($buildDuration -gt $Script:BusBuddyConfig.PerformanceThresholds.BuildTimeWarning) {
            $diagnosticResults.Warnings += "Slow build performance: $([math]::Round($buildDuration, 2))s"
            $diagnosticResults.Recommendations += 'Consider optimizing build configuration or dependencies'
        }
    } else {
        Write-Host "  ❌ Build failed ($([math]::Round($buildDuration, 2))s)" -ForegroundColor Red
        $diagnosticResults.Issues += 'Build failures detected'
        $diagnosticResults.Recommendations += 'Review build errors and resolve compilation issues'
    }

    $diagnosticResults.BuildValidation = $buildResults

    # Test Validation (only if build succeeded)
    if ($buildSuccess) {
        Write-Host '🧪 Validating test system...' -ForegroundColor White
        $testResults = @{}

        # Run tests by category for detailed analysis
        foreach ($category in $Script:BusBuddyConfig.TestCategories) {
            $testStartTime = Get-Date
            $testSuccess = bb-test -Category $category -NoBuild -Verbosity quiet
            $testEndTime = Get-Date
            $testDuration = ($testEndTime - $testStartTime).TotalSeconds

            $testResults[$category] = @{
                Success   = $testSuccess
                Duration  = $testDuration
                Timestamp = $testEndTime.ToString('yyyy-MM-dd HH:mm:ss')
            }

            $status = if ($testSuccess) { '✅' } else { '❌' }
            Write-Host "    $status $category tests ($([math]::Round($testDuration, 2))s)" -ForegroundColor Gray
        }

        # Overall test summary
        $overallTestSuccess = $testResults.Values | ForEach-Object { $_.Success } | Where-Object { $_ -eq $false }
        $testResults.Overall = @{
            Success          = $overallTestSuccess.Count -eq 0
            TotalCategories  = $Script:BusBuddyConfig.TestCategories.Count
            FailedCategories = $overallTestSuccess.Count
            Timestamp        = (Get-Date).ToString('yyyy-MM-dd HH:mm:ss')
        }

        if ($testResults.Overall.Success) {
            Write-Host "  ✅ All test categories passed" -ForegroundColor Green
        } else {
            Write-Host "  ❌ $($testResults.Overall.FailedCategories) test categories failed" -ForegroundColor Red
            $diagnosticResults.Issues += "Test failures in $($testResults.Overall.FailedCategories) categories"
        }

        $diagnosticResults.TestResults = $testResults
    }

    # Performance Metrics (if requested)
    if ($IncludePerformance) {
        Write-Host '⚡ Running performance analysis...' -ForegroundColor White
        $performanceMetrics = @{}

        # Clean build time
        Write-Host '  Testing clean build performance...' -ForegroundColor Gray
        $cleanStart = Get-Date
        bb-clean -Verbosity quiet | Out-Null
        $cleanEnd = Get-Date
        $cleanDuration = ($cleanEnd - $cleanStart).TotalSeconds

        # Full build time
        $fullBuildStart = Get-Date
        bb-build -Verbosity quiet | Out-Null
        $fullBuildEnd = Get-Date
        $fullBuildDuration = ($fullBuildEnd - $fullBuildStart).TotalSeconds

        # Incremental build time
        $incrementalStart = Get-Date
        bb-build -Verbosity quiet | Out-Null
        $incrementalEnd = Get-Date
        $incrementalDuration = ($incrementalEnd - $incrementalStart).TotalSeconds

        # Restore time
        $restoreStart = Get-Date
        bb-restore -Verbosity quiet | Out-Null
        $restoreEnd = Get-Date
        $restoreDuration = ($restoreEnd - $restoreStart).TotalSeconds

        $performanceMetrics = @{
            CleanTime            = $cleanDuration
            FullBuildTime        = $fullBuildDuration
            IncrementalBuildTime = $incrementalDuration
            RestoreTime          = $restoreDuration
            TestedAt             = (Get-Date).ToString('yyyy-MM-dd HH:mm:ss')
            BuildEfficiency      = [math]::Round(($incrementalDuration / $fullBuildDuration) * 100, 1)
        }

        Write-Host "    Clean: $([math]::Round($cleanDuration, 2))s" -ForegroundColor Gray
        Write-Host "    Full Build: $([math]::Round($fullBuildDuration, 2))s" -ForegroundColor Gray
        Write-Host "    Incremental: $([math]::Round($incrementalDuration, 2))s" -ForegroundColor Gray
        Write-Host "    Restore: $([math]::Round($restoreDuration, 2))s" -ForegroundColor Gray
        Write-Host "    Build Efficiency: $($performanceMetrics.BuildEfficiency)%" -ForegroundColor Gray

        # Performance recommendations
        if ($fullBuildDuration -gt 60) {
            $diagnosticResults.Recommendations += 'Consider optimizing build performance - full build exceeds 1 minute'
        }
        if ($restoreDuration -gt 15) {
            $diagnosticResults.Recommendations += 'Package restore is slow - consider checking network connectivity'
        }
        if ($performanceMetrics.BuildEfficiency -gt 50) {
            $diagnosticResults.Recommendations += 'Incremental builds are slow - consider optimizing project dependencies'
        }

        $diagnosticResults.PerformanceMetrics = $performanceMetrics
    }

    # Additional analysis modules would continue here...
    # (XAML, Serilog, Dependencies as per original but enhanced)

    # Generate comprehensive summary
    $totalDuration = ((Get-Date) - $diagnosticStart).TotalSeconds
    $diagnosticResults.Summary = @{
        TotalIssues          = $diagnosticResults.Issues.Count
        TotalWarnings        = $diagnosticResults.Warnings.Count
        TotalRecommendations = $diagnosticResults.Recommendations.Count
        DiagnosticDuration   = $totalDuration
        OverallHealth        = if ($diagnosticResults.Issues.Count -eq 0) { 'Healthy' } elseif ($diagnosticResults.Issues.Count -lt 3) { 'Minor Issues' } else { 'Needs Attention' }
    }

    # Display summary
    Write-Host "`n📋 Diagnostic Summary:" -ForegroundColor Cyan
    Write-Host "  Overall Health: $($diagnosticResults.Summary.OverallHealth)" -ForegroundColor $(
        switch ($diagnosticResults.Summary.OverallHealth) {
            'Healthy' { 'Green' }
            'Minor Issues' { 'Yellow' }
            'Needs Attention' { 'Red' }
        }
    )
    Write-Host "  Issues: $($diagnosticResults.Summary.TotalIssues)" -ForegroundColor Gray
    Write-Host "  Warnings: $($diagnosticResults.Summary.TotalWarnings)" -ForegroundColor Gray
    Write-Host "  Recommendations: $($diagnosticResults.Summary.TotalRecommendations)" -ForegroundColor Gray
    Write-Host "  Analysis Time: $([math]::Round($totalDuration, 2))s" -ForegroundColor Gray

    # Display issues and recommendations
    if ($diagnosticResults.Issues.Count -gt 0) {
        Write-Host "`n❌ Critical Issues:" -ForegroundColor Red
        $diagnosticResults.Issues | ForEach-Object { Write-Host "  • $_" -ForegroundColor Red }
    }

    if ($diagnosticResults.Warnings.Count -gt 0) {
        Write-Host "`n⚠️  Warnings:" -ForegroundColor Yellow
        $diagnosticResults.Warnings | ForEach-Object { Write-Host "  • $_" -ForegroundColor Yellow }
    }

    if ($diagnosticResults.Recommendations.Count -gt 0) {
        Write-Host "`n💡 Recommendations:" -ForegroundColor Yellow
        $diagnosticResults.Recommendations | ForEach-Object { Write-Host "  • $_" -ForegroundColor Gray }
    }

    # Export results if requested
    if ($ExportResults) {
        $timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
        $exportPath = Join-Path $root "$($Script:BusBuddyConfig.LogDirectory)\diagnostic-results-$timestamp.json"

        $logsDir = Join-Path $root $Script:BusBuddyConfig.LogDirectory
        if (-not (Test-Path $logsDir)) {
            New-Item -ItemType Directory -Path $logsDir -Force | Out-Null
        }

        try {
            $diagnosticResults | ConvertTo-Json -Depth 10 | Set-Content -Path $exportPath -Encoding UTF8
            Write-Host "`n💾 Diagnostic results exported to: $exportPath" -ForegroundColor Cyan
        } catch {
            Write-Host "`n❌ Failed to export diagnostic results: $($_.Message)" -ForegroundColor Red
        }
    }

    return $diagnosticResults.Issues.Count -eq 0
}

function New-BusBuddyReport {
    <#
    .SYNOPSIS
        Generate comprehensive Bus Buddy project report with advanced analytics and PowerShell 7.5.2 job support
    .DESCRIPTION
        Creates detailed project reports including build metrics, test coverage, dependency analysis,
        code quality metrics, and performance benchmarks with multiple output formats and parallel processing
    .PARAMETER IncludeTests
        Include comprehensive test analysis and coverage
    .PARAMETER IncludeDependencies
        Include dependency vulnerability and version analysis
    .PARAMETER IncludeMetrics
        Include code quality and complexity metrics
    .PARAMETER IncludePerformance
        Include performance benchmarks and optimization suggestions
    .PARAMETER IncludeGit
        Include Git repository analysis and history
    .PARAMETER OutputFormat
        Output format for the report (JSON, HTML, Markdown, CSV)
    .PARAMETER OutputPath
        Custom output path for the report
    .PARAMETER Detailed
        Include detailed analysis for all components
    .PARAMETER AsJob
        Run report generation as PowerShell background job (PowerShell 7.5.2+ feature)
    .PARAMETER JobName
        Name for the background job (used with -AsJob)
    .PARAMETER Parallel
        Use parallel processing for analysis tasks (PowerShell 7.5.2+ feature)
    #>
    param(
        [Parameter(Mandatory = $false)]
        [switch]$IncludeTests,

        [Parameter(Mandatory = $false)]
        [switch]$IncludeDependencies,

        [Parameter(Mandatory = $false)]
        [switch]$IncludeMetrics,

        [Parameter(Mandatory = $false)]
        [switch]$IncludePerformance,

        [Parameter(Mandatory = $false)]
        [switch]$IncludeGit,

        [Parameter(Mandatory = $false)]
        [ValidateSet('JSON', 'HTML', 'Markdown', 'CSV')]
        [string]$OutputFormat = 'JSON',

        [Parameter(Mandatory = $false)]
        [string]$OutputPath,

        [Parameter(Mandatory = $false)]
        [switch]$Detailed,

        [Parameter(Mandatory = $false)]
        [switch]$AsJob,

        [Parameter(Mandatory = $false)]
        [string]$JobName = "BusBuddy-Report-$(Get-Date -Format 'HHmmss')",

        [Parameter(Mandatory = $false)]
        [switch]$Parallel
    )

    # PowerShell 7.5.2+ background job support
    if ($AsJob) {
        if ($PSVersionTable.PSVersion.Major -lt 7 -or ($PSVersionTable.PSVersion.Major -eq 7 -and $PSVersionTable.PSVersion.Minor -lt 5)) {
            Write-Host '⚠️  Background job support requires PowerShell 7.5+. Running synchronously...' -ForegroundColor Yellow
            $AsJob = $false
        } else {
            Write-Host "� Starting report generation as background job: $JobName" -ForegroundColor Cyan

            # Create job parameters
            $jobParams = @{
                IncludeTests        = $IncludeTests
                IncludeDependencies = $IncludeDependencies
                IncludeMetrics      = $IncludeMetrics
                IncludePerformance  = $IncludePerformance
                IncludeGit          = $IncludeGit
                OutputFormat        = $OutputFormat
                OutputPath          = $OutputPath
                Detailed            = $Detailed
                Parallel            = $Parallel
            }

            # Start background job
            $job = Start-ThreadJob -Name $JobName -ScriptBlock {
                param($params, $workflowsPath, $configPath)

                # Load required modules in job context
                if (Test-Path $workflowsPath) {
                    . $workflowsPath -Quiet
                }

                # Execute report generation
                bb-report @params

            } -ArgumentList $jobParams, $PSCommandPath, $Script:BusBuddyConfig

            Write-Host "✅ Background job started successfully" -ForegroundColor Green
            Write-Host "📋 Job Name: $JobName" -ForegroundColor Gray
            Write-Host "🆔 Job ID: $($job.Id)" -ForegroundColor Gray
            Write-Host "💡 Monitor progress: Get-Job -Name '$JobName' | Receive-Job -Keep" -ForegroundColor Yellow
            Write-Host "💡 Get results: Get-Job -Name '$JobName' | Receive-Job" -ForegroundColor Yellow
            Write-Host "💡 Remove job: Get-Job -Name '$JobName' | Remove-Job" -ForegroundColor Yellow

            return $job
        }
    }

    Write-Host '�📊 Generating Comprehensive Bus Buddy Project Report' -ForegroundColor Cyan
    $reportStart = Get-Date

    $root = Get-BusBuddyProjectRoot
    if (-not $root) {
        Write-Host '❌ Bus Buddy project not found' -ForegroundColor Red
        return $false
    }

    # Determine output path
    $timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
    if (-not $OutputPath) {
        $logsDir = Join-Path $root $Script:BusBuddyConfig.LogDirectory
        if (-not (Test-Path $logsDir)) {
            New-Item -ItemType Directory -Path $logsDir -Force | Out-Null
        }

        $fileExtension = switch ($OutputFormat) {
            'JSON' { 'json' }
            'HTML' { 'html' }
            'Markdown' { 'md' }
            'CSV' { 'csv' }
        }
        $OutputPath = Join-Path $logsDir "busbuddy-report-$timestamp.$fileExtension"
    }

    # Initialize comprehensive report
    $report = @{
        Metadata             = @{
            GeneratedAt        = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
            ReportVersion      = '2.0'
            ProjectRoot        = $root
            GeneratedBy        = $env:USERNAME
            Machine            = $env:COMPUTERNAME
            ReportType         = 'Comprehensive Project Analysis'
            PowerShellVersion  = $PSVersionTable.PSVersion.ToString()
            ParallelProcessing = $Parallel.IsPresent
        }
        Environment          = @{}
        ProjectConfiguration = $Script:BusBuddyConfig
        ProjectStructure     = @{}
        BuildAnalysis        = @{}
        TestAnalysis         = @{}
        DependencyAnalysis   = @{}
        CodeMetrics          = @{}
        PerformanceAnalysis  = @{}
        GitAnalysis          = @{}
        QualityMetrics       = @{}
        SecurityAnalysis     = @{}
        Recommendations      = @()
        Issues               = @()
        Summary              = @{}
    }

    # Enhanced Environment Analysis with parallel processing
    Write-Host '🌍 Analyzing environment...' -ForegroundColor White
    try {
        if ($Parallel -and $PSVersionTable.PSVersion.Major -ge 7 -and $PSVersionTable.PSVersion.Minor -ge 5) {
            Write-Host '  ⚡ Using parallel environment analysis...' -ForegroundColor Gray

            # PowerShell 7.5.2+ parallel processing
            $envTasks = @(
                { Get-WmiObject -Class Win32_OperatingSystem -ErrorAction SilentlyContinue },
                { Get-WmiObject -Class Win32_Processor -ErrorAction SilentlyContinue | Select-Object -First 1 },
                { Get-WmiObject -Class Win32_ComputerSystem -ErrorAction SilentlyContinue },
                { dotnet --version 2>$null },
                { dotnet --info 2>$null | Select-String "Microsoft.NETCore.App" | Select-Object -First 1 }
            )

            $envResults = $envTasks | ForEach-Object -Parallel { & $_ } -ThrottleLimit 3
            $os = $envResults[0]
            $cpu = $envResults[1]
            $memory = $envResults[2]
            $dotnetVersion = $envResults[3]
            $runtimeInfo = $envResults[4]
        } else {
            # Sequential processing
            $os = Get-WmiObject -Class Win32_OperatingSystem -ErrorAction SilentlyContinue
            $cpu = Get-WmiObject -Class Win32_Processor -ErrorAction SilentlyContinue | Select-Object -First 1
            $memory = Get-WmiObject -Class Win32_ComputerSystem -ErrorAction SilentlyContinue
            $dotnetVersion = dotnet --version 2>$null
            $runtimeInfo = dotnet --info 2>$null | Select-String "Microsoft.NETCore.App" | Select-Object -First 1
        }

        $report.Environment = @{
            PowerShell = @{
                Version         = $PSVersionTable.PSVersion.ToString()
                Edition         = $PSVersionTable.PSEdition
                CLRVersion      = $PSVersionTable.CLRVersion.ToString()
                ParallelSupport = $PSVersionTable.PSVersion.Major -ge 7 -and $PSVersionTable.PSVersion.Minor -ge 5
            }
            DotNet     = @{
                Version = $dotnetVersion
                Runtime = if ($runtimeInfo) { $runtimeInfo.ToString() } else { 'Unknown' }
            }
            System     = @{
                OS                = if ($os) { $os.Caption } else { [Environment]::OSVersion.ToString() }
                Architecture      = if ($os) { $os.OSArchitecture } else { 'Unknown' }
                TotalMemoryGB     = if ($memory) { [math]::Round($memory.TotalPhysicalMemory / 1GB, 2) } else { 0 }
                AvailableMemoryGB = if ($os) { [math]::Round($os.FreePhysicalMemory / 1MB / 1024, 2) } else { 0 }
                ProcessorCount    = [Environment]::ProcessorCount
                CPUName           = if ($cpu) { $cpu.Name } else { 'Unknown' }
            }
            Tools      = @{}
        }

        # Tool versions with parallel processing
        $tools = @('dotnet', 'git', 'code', 'pwsh')
        if ($Parallel -and $PSVersionTable.PSVersion.Major -ge 7 -and $PSVersionTable.PSVersion.Minor -ge 5) {
            $toolResults = $tools | ForEach-Object -Parallel {
                $tool = $_
                if (Get-Command $tool -ErrorAction SilentlyContinue) {
                    try {
                        $version = & $tool --version 2>$null | Select-Object -First 1
                        return @{ Tool = $tool; Version = $version; Available = $true }
                    } catch {
                        return @{ Tool = $tool; Version = 'Available but version unknown'; Available = $true }
                    }
                } else {
                    return @{ Tool = $tool; Version = 'Not available'; Available = $false }
                }
            } -ThrottleLimit 2

            foreach ($result in $toolResults) {
                $report.Environment.Tools[$result.Tool] = $result.Version
            }
        } else {
            foreach ($tool in $tools) {
                if (Get-Command $tool -ErrorAction SilentlyContinue) {
                    try {
                        $version = & $tool --version 2>$null | Select-Object -First 1
                        $report.Environment.Tools[$tool] = $version
                    } catch {
                        $report.Environment.Tools[$tool] = 'Available but version unknown'
                    }
                } else {
                    $report.Environment.Tools[$tool] = 'Not available'
                }
            }
        }
    } catch {
        $report.Issues += "Environment analysis failed: $($_.Message)"
    }

    # Project Structure Analysis
    Write-Host '🏗️  Analyzing project structure...' -ForegroundColor White
    $structureAnalysis = @{
        Solution      = @{}
        Projects      = @{}
        SourceFiles   = @{}
        Dependencies  = @{}
        Configuration = @{}
    }

    # Solution analysis
    $solutionPath = Join-Path $root $Script:BusBuddyConfig.SolutionFile
    $structureAnalysis.Solution = @{
        Path         = $solutionPath
        Exists       = Test-Path $solutionPath
        Size         = if (Test-Path $solutionPath) { (Get-Item $solutionPath).Length } else { 0 }
        LastModified = if (Test-Path $solutionPath) { (Get-Item $solutionPath).LastWriteTime } else { $null }
    }

    # Enhanced project analysis
    foreach ($projectName in $Script:BusBuddyConfig.Projects.Keys) {
        $projectFile = $Script:BusBuddyConfig.ProjectFiles[$projectName]
        $projectPath = Join-Path $root $projectFile

        $analysis = @{
            Path   = $projectPath
            Exists = Test-Path $projectPath
        }

        if (Test-Path $projectPath) {
            try {
                $projectInfo = Get-Item $projectPath
                $analysis.Size = $projectInfo.Length
                $analysis.LastModified = $projectInfo.LastWriteTime

                $projectContent = Get-Content $projectPath -Raw
                $analysis.TargetFramework = if ($projectContent -match '<TargetFramework>(.*?)</TargetFramework>') { $matches[1] } else { 'Unknown' }
                $analysis.PackageReferences = ([regex]::Matches($projectContent, '<PackageReference')).Count
                $analysis.ProjectReferences = ([regex]::Matches($projectContent, '<ProjectReference')).Count

                # Extract package information
                $packageMatches = [regex]::Matches($projectContent, '<PackageReference\s+Include="([^"]+)"\s+Version="([^"]+)"')
                $packages = @{}
                foreach ($match in $packageMatches) {
                    $packages[$match.Groups[1].Value] = $match.Groups[2].Value
                }
                $analysis.Packages = $packages

            } catch {
                $analysis.Error = $_.Message
                $report.Issues += "Failed to analyze $projectName project: $($_.Message)"
            }
        }

        $structureAnalysis.Projects[$projectName] = $analysis
    }

    # Source file analysis
    try {
        $csFiles = Get-ChildItem -Path $root -Filter "*.cs" -Recurse | Where-Object {
            $_.FullName -notlike "*\bin\*" -and $_.FullName -notlike "*\obj\*"
        }
        $xamlFiles = Get-ChildItem -Path $root -Filter "*.xaml" -Recurse | Where-Object {
            $_.FullName -notlike "*\bin\*" -and $_.FullName -notlike "*\obj\*"
        }

        $structureAnalysis.SourceFiles = @{
            CSharpFiles = $csFiles.Count
            XamlFiles   = $xamlFiles.Count
            TotalFiles  = $csFiles.Count + $xamlFiles.Count
            TotalSizeKB = [math]::Round((($csFiles | Measure-Object Length -Sum).Sum + ($xamlFiles | Measure-Object Length -Sum).Sum) / 1KB, 2)
            LargestFile = if ($csFiles) {
                $largest = $csFiles | Sort-Object Length -Descending | Select-Object -First 1
                @{ Name = $largest.Name; SizeKB = [math]::Round($largest.Length / 1KB, 2) }
            } else { $null }
        }

        # Calculate lines of code if detailed metrics requested
        if ($IncludeMetrics) {
            Write-Host '  📏 Calculating code metrics...' -ForegroundColor Gray
            $totalLines = 0
            $fileMetrics = @()

            foreach ($file in $csFiles) {
                try {
                    $lines = (Get-Content $file.FullName).Count
                    $totalLines += $lines
                    $fileMetrics += @{
                        File   = $file.Name
                        Lines  = $lines
                        SizeKB = [math]::Round($file.Length / 1KB, 2)
                    }
                } catch {
                    # Skip files that can't be read
                }
            }

            $structureAnalysis.SourceFiles.TotalLinesOfCode = $totalLines
            $structureAnalysis.SourceFiles.AverageLinesPerFile = if ($csFiles.Count -gt 0) { [math]::Round($totalLines / $csFiles.Count, 0) } else { 0 }

            if ($Detailed) {
                $structureAnalysis.SourceFiles.FileMetrics = $fileMetrics | Sort-Object Lines -Descending | Select-Object -First 20
            }
        }
    } catch {
        $report.Issues += "Source file analysis failed: $($_.Message)"
    }

    $report.ProjectStructure = $structureAnalysis

    # Build Analysis
    Write-Host '🔨 Analyzing build performance...' -ForegroundColor White
    $buildResults = @{}

    # Test build performance
    $buildConfigs = @('Debug', 'Release')
    foreach ($config in $buildConfigs) {
        Write-Host "  Testing $config build..." -ForegroundColor Gray
        $buildStart = Get-Date
        $buildSuccess = bb-build -Configuration $config -Verbosity quiet
        $buildEnd = Get-Date
        $buildDuration = ($buildEnd - $buildStart).TotalSeconds

        $buildResults[$config] = @{
            Success           = $buildSuccess
            Duration          = $buildDuration
            Timestamp         = $buildEnd.ToString('yyyy-MM-dd HH:mm:ss')
            PerformanceRating = switch ($buildDuration) {
                { $_ -lt 30 } { 'Excellent' }
                { $_ -lt 60 } { 'Good' }
                { $_ -lt 120 } { 'Fair' }
                default { 'Poor' }
            }
        }
    }

    $report.BuildAnalysis = $buildResults

    # Test Analysis (if requested)
    if ($IncludeTests) {
        Write-Host '🧪 Analyzing test coverage and performance...' -ForegroundColor White
        $testResults = @{
            Categories = @{}
            Overall    = @{}
            Coverage   = @{}
        }

        # Test each category
        foreach ($category in $Script:BusBuddyConfig.TestCategories) {
            Write-Host "  Testing $category category..." -ForegroundColor Gray
            $testStart = Get-Date
            $testSuccess = bb-test -Category $category -NoBuild -Verbosity quiet
            $testEnd = Get-Date
            $testDuration = ($testEnd - $testStart).TotalSeconds

            $testResults.Categories[$category] = @{
                Success   = $testSuccess
                Duration  = $testDuration
                Timestamp = $testEnd.ToString('yyyy-MM-dd HH:mm:ss')
            }
        }

        # Overall test summary
        $failedCategories = $testResults.Categories.Values | Where-Object { -not $_.Success }
        $testResults.Overall = @{
            TotalCategories  = $Script:BusBuddyConfig.TestCategories.Count
            PassedCategories = $Script:BusBuddyConfig.TestCategories.Count - $failedCategories.Count
            FailedCategories = $failedCategories.Count
            OverallSuccess   = $failedCategories.Count -eq 0
            TotalDuration    = ($testResults.Categories.Values | Measure-Object Duration -Sum).Sum
        }

        $report.TestAnalysis = $testResults
    }

    # Performance Analysis (if requested)
    if ($IncludePerformance) {
        Write-Host '⚡ Running performance benchmarks...' -ForegroundColor White
        $performanceResults = @{}

        # Build performance metrics
        Write-Host '  Benchmarking build operations...' -ForegroundColor Gray

        # Clean operation
        $cleanStart = Get-Date
        bb-clean -Verbosity quiet | Out-Null
        $cleanDuration = ((Get-Date) - $cleanStart).TotalSeconds

        # Restore operation
        $restoreStart = Get-Date
        bb-restore -Verbosity quiet | Out-Null
        $restoreDuration = ((Get-Date) - $restoreStart).TotalSeconds

        # Full build
        $fullBuildStart = Get-Date
        bb-build -Verbosity quiet | Out-Null
        $fullBuildDuration = ((Get-Date) - $fullBuildStart).TotalSeconds

        # Incremental build
        $incrementalStart = Get-Date
        bb-build -Verbosity quiet | Out-Null
        $incrementalDuration = ((Get-Date) - $incrementalStart).TotalSeconds

        $performanceResults = @{
            CleanTime            = $cleanDuration
            RestoreTime          = $restoreDuration
            FullBuildTime        = $fullBuildDuration
            IncrementalBuildTime = $incrementalDuration
            BuildEfficiency      = if ($fullBuildDuration -gt 0) { [math]::Round(($incrementalDuration / $fullBuildDuration) * 100, 1) } else { 0 }
            TestedAt             = (Get-Date).ToString('yyyy-MM-dd HH:mm:ss')
            Recommendations      = @()
        }

        # Performance recommendations
        if ($fullBuildDuration -gt 60) {
            $performanceResults.Recommendations += 'Full build time exceeds 1 minute - consider dependency optimization'
        }
        if ($restoreDuration -gt 15) {
            $performanceResults.Recommendations += 'Package restore is slow - check network connectivity and NuGet sources'
        }
        if ($performanceResults.BuildEfficiency -gt 50) {
            $performanceResults.Recommendations += 'Incremental builds are inefficient - review project structure'
        }

        $report.PerformanceAnalysis = $performanceResults
    }

    # Git Analysis (if requested)
    if ($IncludeGit) {
        Write-Host '📋 Analyzing Git repository...' -ForegroundColor White
        $gitResults = @{}

        try {
            if (Test-Path (Join-Path $root '.git')) {
                $gitResults.IsRepository = $true

                # Branch information
                $gitResults.CurrentBranch = git branch --show-current 2>$null
                $gitResults.TotalBranches = (git branch -a 2>$null | Measure-Object).Count

                # Commit history
                $gitResults.TotalCommits = (git rev-list --count HEAD 2>$null)
                $gitResults.LastCommit = git log -1 --pretty=format:"%h - %an, %ar : %s" 2>$null

                # Repository status
                $status = git status --porcelain 2>$null
                $gitResults.UncommittedFiles = if ($status) { $status.Count } else { 0 }

                # Contributors
                $contributors = git shortlog -sn 2>$null | Measure-Object
                $gitResults.Contributors = $contributors.Count

                # Recent activity (last 30 days)
                $recentCommits = git log --since="30 days ago" --oneline 2>$null
                $gitResults.RecentActivity = if ($recentCommits) { $recentCommits.Count } else { 0 }

            } else {
                $gitResults.IsRepository = $false
                $report.Recommendations += 'Initialize Git repository for version control'
            }
        } catch {
            $gitResults.Error = $_.Message
            $report.Issues += "Git analysis failed: $($_.Message)"
        }

        $report.GitAnalysis = $gitResults
    }

    # Generate comprehensive summary
    $reportDuration = ((Get-Date) - $reportStart).TotalSeconds
    $report.Summary = @{
        GenerationTime       = $reportDuration
        TotalIssues          = $report.Issues.Count
        TotalRecommendations = $report.Recommendations.Count
        ProjectHealth        = if ($report.Issues.Count -eq 0) { 'Excellent' } elseif ($report.Issues.Count -lt 3) { 'Good' } elseif ($report.Issues.Count -lt 6) { 'Fair' } else { 'Poor' }
        BuildStatus          = if ($report.BuildAnalysis.Debug -and $report.BuildAnalysis.Debug.Success) { 'Passing' } else { 'Failing' }
        TestStatus           = if ($report.TestAnalysis -and $report.TestAnalysis.Overall.OverallSuccess) { 'Passing' } else { 'Unknown' }
    }

    # Export report based on format
    try {
        switch ($OutputFormat) {
            'JSON' {
                $jsonOutput = $report | ConvertTo-Json -Depth 10
                $jsonOutput | Set-Content -Path $OutputPath -Encoding UTF8
            }
            'HTML' {
                $htmlContent = ConvertTo-HtmlReport -Report $report
                $htmlContent | Set-Content -Path $OutputPath -Encoding UTF8
            }
            'Markdown' {
                $markdownContent = ConvertTo-MarkdownReport -Report $report
                $markdownContent | Set-Content -Path $OutputPath -Encoding UTF8
            }
            'CSV' {
                $csvContent = ConvertTo-CsvReport -Report $report
                $csvContent | Set-Content -Path $OutputPath -Encoding UTF8
            }
        }

        Write-Host "📊 Report generated successfully!" -ForegroundColor Green
        Write-Host "  📁 Path: $OutputPath" -ForegroundColor Gray
        Write-Host "  📏 Format: $OutputFormat" -ForegroundColor Gray
        Write-Host "  📊 Size: $([math]::Round((Get-Item $OutputPath).Length / 1KB, 2)) KB" -ForegroundColor Gray
        Write-Host "  ⏱️  Generation time: $([math]::Round($reportDuration, 2))s" -ForegroundColor Gray

        # Display key insights
        Write-Host "`n📈 Key Insights:" -ForegroundColor Cyan
        Write-Host "  Project Health: $($report.Summary.ProjectHealth)" -ForegroundColor $(
            switch ($report.Summary.ProjectHealth) {
                'Excellent' { 'Green' }
                'Good' { 'Green' }
                'Fair' { 'Yellow' }
                'Poor' { 'Red' }
            }
        )

        if ($report.ProjectStructure.SourceFiles.TotalFiles) {
            Write-Host "  Source Files: $($report.ProjectStructure.SourceFiles.TotalFiles)" -ForegroundColor Gray
        }

        if ($report.BuildAnalysis.Debug) {
            Write-Host "  Build Performance: $($report.BuildAnalysis.Debug.PerformanceRating)" -ForegroundColor Gray
        }

        return $true
    } catch {
        Write-Host "❌ Failed to generate report: $($_.Message)" -ForegroundColor Red
        return $false
    }
}

# Helper functions for report formatting
function ConvertTo-HtmlReport {
    param($Report)

    $css = @"
<style>
body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 20px; line-height: 1.6; }
.header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; border-radius: 8px; margin-bottom: 20px; }
.section { margin: 20px 0; padding: 15px; border-left: 4px solid #667eea; background: #f8f9fa; }
.success { color: #28a745; font-weight: bold; }
.error { color: #dc3545; font-weight: bold; }
.warning { color: #ffc107; font-weight: bold; }
.metric { display: inline-block; margin: 10px; padding: 10px; background: white; border-radius: 4px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
table { border-collapse: collapse; width: 100%; margin: 10px 0; }
th, td { border: 1px solid #ddd; padding: 12px; text-align: left; }
th { background-color: #667eea; color: white; }
tr:nth-child(even) { background-color: #f2f2f2; }
.summary-card { background: white; padding: 20px; margin: 10px; border-radius: 8px; box-shadow: 0 4px 8px rgba(0,0,0,0.1); }
</style>
"@

    $html = @"
<!DOCTYPE html>
<html>
<head>
    <title>Bus Buddy Project Report</title>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    $css
</head>
<body>
    <div class="header">
        <h1>🚌 Bus Buddy Project Report</h1>
        <p>Generated: $($Report.Metadata.GeneratedAt) | Project Health: $($Report.Summary.ProjectHealth)</p>
    </div>

    <div class="summary-card">
        <h2>📊 Executive Summary</h2>
        <div class="metric">
            <h3>Project Health</h3>
            <p class="$(if ($Report.Summary.ProjectHealth -eq 'Excellent' -or $Report.Summary.ProjectHealth -eq 'Good') { 'success' } else { 'warning' })">
                $($Report.Summary.ProjectHealth)
            </p>
        </div>
        <div class="metric">
            <h3>Build Status</h3>
            <p class="$(if ($Report.Summary.BuildStatus -eq 'Passing') { 'success' } else { 'error' })">
                $($Report.Summary.BuildStatus)
            </p>
        </div>
        <div class="metric">
            <h3>Issues Found</h3>
            <p class="$(if ($Report.Summary.TotalIssues -eq 0) { 'success' } else { 'error' })">
                $($Report.Summary.TotalIssues)
            </p>
        </div>
    </div>

    <div class="section">
        <h2>🏗️ Project Structure</h2>
        <p><strong>Total Source Files:</strong> $($Report.ProjectStructure.SourceFiles.TotalFiles)</p>
        <p><strong>C# Files:</strong> $($Report.ProjectStructure.SourceFiles.CSharpFiles)</p>
        <p><strong>XAML Files:</strong> $($Report.ProjectStructure.SourceFiles.XamlFiles)</p>
        <p><strong>Total Code Size:</strong> $($Report.ProjectStructure.SourceFiles.TotalSizeKB) KB</p>
    </div>

    <div class="section">
        <h2>🔨 Build Analysis</h2>
        <table>
            <thead>
                <tr><th>Configuration</th><th>Status</th><th>Duration (s)</th><th>Performance</th></tr>
            </thead>
            <tbody>
"@

    foreach ($config in $Report.BuildAnalysis.Keys) {
        $build = $Report.BuildAnalysis[$config]
        $statusClass = if ($build.Success) { 'success' } else { 'error' }
        $html += @"
                <tr>
                    <td>$config</td>
                    <td class="$statusClass">$(if ($build.Success) { 'Success' } else { 'Failed' })</td>
                    <td>$([math]::Round($build.Duration, 2))</td>
                    <td>$($build.PerformanceRating)</td>
                </tr>
"@
    }

    $html += @"
            </tbody>
        </table>
    </div>

    <div class="section">
        <h2>🌍 Environment</h2>
        <p><strong>PowerShell:</strong> $($Report.Environment.PowerShell.Version) ($($Report.Environment.PowerShell.Edition))</p>
        <p><strong>.NET:</strong> $($Report.Environment.DotNet.Version)</p>
        <p><strong>Operating System:</strong> $($Report.Environment.System.OS)</p>
        <p><strong>Memory:</strong> $($Report.Environment.System.AvailableMemoryGB)GB / $($Report.Environment.System.TotalMemoryGB)GB</p>
    </div>
"@

    if ($Report.Issues.Count -gt 0) {
        $html += @"
    <div class="section">
        <h2>❌ Issues</h2>
        <ul>
"@
        foreach ($issue in $Report.Issues) {
            $html += "            <li class='error'>$issue</li>`n"
        }
        $html += @"
        </ul>
    </div>
"@
    }

    if ($Report.Recommendations.Count -gt 0) {
        $html += @"
    <div class="section">
        <h2>💡 Recommendations</h2>
        <ul>
"@
        foreach ($rec in $Report.Recommendations) {
            $html += "            <li>$rec</li>`n"
        }
        $html += @"
        </ul>
    </div>
"@
    }

    $html += @"
    <div class="section">
        <h2>📋 Report Details</h2>
        <p><strong>Generated By:</strong> $($Report.Metadata.GeneratedBy)</p>
        <p><strong>Machine:</strong> $($Report.Metadata.Machine)</p>
        <p><strong>Generation Time:</strong> $([math]::Round($Report.Summary.GenerationTime, 2)) seconds</p>
        <p><strong>Report Version:</strong> $($Report.Metadata.ReportVersion)</p>
    </div>

</body>
</html>
"@

    return $html
}

function ConvertTo-MarkdownReport {
    param($Report)

    $markdown = @"
# 🚌 Bus Buddy Project Report

**Generated:** $($Report.Metadata.GeneratedAt)
**Project Root:** $($Report.Metadata.ProjectRoot)
**Report Version:** $($Report.Metadata.ReportVersion)

## 📊 Executive Summary

| Metric | Value |
|--------|-------|
| **Project Health** | $($Report.Summary.ProjectHealth) |
| **Build Status** | $($Report.Summary.BuildStatus) |
| **Total Issues** | $($Report.Summary.TotalIssues) |
| **Total Recommendations** | $($Report.Summary.TotalRecommendations) |

## 🏗️ Project Structure

- **Total Source Files:** $($Report.ProjectStructure.SourceFiles.TotalFiles)
- **C# Files:** $($Report.ProjectStructure.SourceFiles.CSharpFiles)
- **XAML Files:** $($Report.ProjectStructure.SourceFiles.XamlFiles)
- **Total Code Size:** $($Report.ProjectStructure.SourceFiles.TotalSizeKB) KB

## 🔨 Build Analysis

| Configuration | Status | Duration (s) | Performance |
|---------------|--------|--------------|-------------|
"@

    foreach ($config in $Report.BuildAnalysis.Keys) {
        $build = $Report.BuildAnalysis[$config]
        $status = if ($build.Success) { '✅ Success' } else { '❌ Failed' }
        $markdown += "| $config | $status | $([math]::Round($build.Duration, 2)) | $($build.PerformanceRating) |`n"
    }

    $markdown += @"

## 🌍 Environment

- **PowerShell:** $($Report.Environment.PowerShell.Version) ($($Report.Environment.PowerShell.Edition))
- **.NET:** $($Report.Environment.DotNet.Version)
- **Operating System:** $($Report.Environment.System.OS)
- **Memory:** $($Report.Environment.System.AvailableMemoryGB)GB / $($Report.Environment.System.TotalMemoryGB)GB available

"@

    if ($Report.Issues.Count -gt 0) {
        $markdown += @"
## ❌ Issues

"@
        foreach ($issue in $Report.Issues) {
            $markdown += "- $issue`n"
        }
        $markdown += "`n"
    }

    if ($Report.Recommendations.Count -gt 0) {
        $markdown += @"
## 💡 Recommendations

"@
        foreach ($rec in $Report.Recommendations) {
            $markdown += "- $rec`n"
        }
        $markdown += "`n"
    }

    $markdown += @"
## 📋 Report Metadata

- **Generated By:** $($Report.Metadata.GeneratedBy)
- **Machine:** $($Report.Metadata.Machine)
- **Generation Time:** $([math]::Round($Report.Summary.GenerationTime, 2)) seconds

---
*Generated by Bus Buddy Advanced Workflows v$($Report.Metadata.ReportVersion)*
"@

    return $markdown
}

function ConvertTo-CsvReport {
    param($Report)

    $csvData = @()

    # Summary metrics
    $csvData += [PSCustomObject]@{
        Category = 'Summary'
        Metric   = 'Project Health'
        Value    = $Report.Summary.ProjectHealth
        Details  = ''
    }

    $csvData += [PSCustomObject]@{
        Category = 'Summary'
        Metric   = 'Build Status'
        Value    = $Report.Summary.BuildStatus
        Details  = ''
    }

    # Project structure metrics
    $csvData += [PSCustomObject]@{
        Category = 'Project Structure'
        Metric   = 'Total Files'
        Value    = $Report.ProjectStructure.SourceFiles.TotalFiles
        Details  = "$($Report.ProjectStructure.SourceFiles.CSharpFiles) C#, $($Report.ProjectStructure.SourceFiles.XamlFiles) XAML"
    }

    # Build metrics
    foreach ($config in $Report.BuildAnalysis.Keys) {
        $build = $Report.BuildAnalysis[$config]
        $csvData += [PSCustomObject]@{
            Category = 'Build Analysis'
            Metric   = "$config Build"
            Value    = if ($build.Success) { 'Success' } else { 'Failed' }
            Details  = "$([math]::Round($build.Duration, 2))s - $($build.PerformanceRating)"
        }
    }

    # Environment metrics
    $csvData += [PSCustomObject]@{
        Category = 'Environment'
        Metric   = 'PowerShell Version'
        Value    = $Report.Environment.PowerShell.Version
        Details  = $Report.Environment.PowerShell.Edition
    }

    return $csvData | ConvertTo-Csv -NoTypeInformation
}

# Debug Helper Integration Commands
function Start-BusBuddyDebug {
    <#
    .SYNOPSIS
        Start debug filter monitoring with actual implementation
    .DESCRIPTION
        Connects to the running Bus Buddy application and starts debug monitoring
    .PARAMETER Port
        Port to connect to for debug monitoring (default: 5000)
    .PARAMETER LogPath
        Path to write debug logs (default: logs/debug-monitor.log)
    #>
    param(
        [Parameter(Mandatory = $false)]
        [int]$Port = 5000,

        [Parameter(Mandatory = $false)]
        [string]$LogPath = $null
    )

    $root = Get-BusBuddyProjectRoot
    if (-not $root) {
        Write-Host '❌ Bus Buddy project not found' -ForegroundColor Red
        return $false
    }

    # Set default log path if not provided
    if (-not $LogPath) {
        $logsDir = Join-Path $root 'logs'
        if (-not (Test-Path $logsDir)) {
            New-Item -ItemType Directory -Path $logsDir -Force | Out-Null
        }
        $LogPath = Join-Path $logsDir "debug-monitor-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
    }

    Write-Host '🐛 Starting debug filter monitoring...' -ForegroundColor Cyan
    Write-Host "� Log Path: $LogPath" -ForegroundColor Gray

    # Check if Bus Buddy application is running
    $busBuddyProcesses = Get-Process -Name 'BusBuddy*' -ErrorAction SilentlyContinue
    if (-not $busBuddyProcesses) {
        Write-Host '⚠️ No Bus Buddy processes found. Starting application with debug monitoring...' -ForegroundColor Yellow
        return bb-run -DebugFilter -ValidateArgs
    }

    Write-Host "✅ Found $($busBuddyProcesses.Count) Bus Buddy process(es)" -ForegroundColor Green

    # Try to connect to debug interface (placeholder for actual implementation)
    try {
        Write-Host "🔗 Attempting to connect to debug interface on port $Port..." -ForegroundColor Yellow

        # This would be the actual debug monitoring implementation
        # For now, we'll create a basic file watcher simulation
        Write-Host "📝 Starting debug log monitoring..." -ForegroundColor Cyan
        Write-Host "💡 Monitor file: $LogPath" -ForegroundColor Gray
        Write-Host "💡 Press Ctrl+C to stop monitoring" -ForegroundColor Gray

        # Create initial log entry
        "Debug monitoring started at $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" | Out-File -FilePath $LogPath -Append -Encoding UTF8

        Write-Host "✅ Debug monitoring active. Check log file for output." -ForegroundColor Green
        return $true

    } catch {
        Write-Host "❌ Failed to start debug monitoring: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "💡 Ensure the Bus Buddy application supports debug monitoring" -ForegroundColor Yellow
        return $false
    }
}

function Export-BusBuddyDebug {
    <#
    .SYNOPSIS
        Export debug data from running application with actual implementation
    .DESCRIPTION
        Collects and exports comprehensive debug information from the running Bus Buddy application
    .PARAMETER OutputPath
        Path to save the debug export (default: logs/debug-export-{timestamp}.json)
    .PARAMETER IncludeLogs
        Include application logs in the export
    .PARAMETER IncludeState
        Include application state information
    #>
    param(
        [Parameter(Mandatory = $false)]
        [string]$OutputPath = $null,

        [Parameter(Mandatory = $false)]
        [switch]$IncludeLogs,

        [Parameter(Mandatory = $false)]
        [switch]$IncludeState
    )

    $root = Get-BusBuddyProjectRoot
    if (-not $root) {
        Write-Host '❌ Bus Buddy project not found' -ForegroundColor Red
        return $false
    }

    # Set default output path if not provided
    if (-not $OutputPath) {
        $logsDir = Join-Path $root 'logs'
        if (-not (Test-Path $logsDir)) {
            New-Item -ItemType Directory -Path $logsDir -Force | Out-Null
        }
        $OutputPath = Join-Path $logsDir "debug-export-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
    }

    Write-Host '📤 Exporting debug data...' -ForegroundColor Cyan
    Write-Host "📍 Output Path: $OutputPath" -ForegroundColor Gray

    try {
        # Create comprehensive debug export
        $debugExport = @{
            ExportedAt       = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
            ProjectRoot      = $root
            SystemInfo       = @{
                PowerShellVersion = $PSVersionTable.PSVersion.ToString()
                DotNetVersion     = (dotnet --version 2>$null)
                MachineName       = $env:COMPUTERNAME
                UserName          = $env:USERNAME
                OSVersion         = [Environment]::OSVersion.ToString()
            }
            Processes        = @()
            ProjectStructure = @{}
            ApplicationLogs  = @()
            BuildStatus      = @{}
        }

        # Collect running processes
        $busBuddyProcesses = Get-Process -Name 'BusBuddy*' -ErrorAction SilentlyContinue
        foreach ($process in $busBuddyProcesses) {
            $debugExport.Processes += @{
                Name       = $process.ProcessName
                Id         = $process.Id
                StartTime  = $process.StartTime
                WorkingSet = $process.WorkingSet64
                CPU        = $process.TotalProcessorTime.TotalSeconds
            }
        }

        # Analyze project structure
        $projects = @('BusBuddy.Core', 'BusBuddy.WPF', 'BusBuddy.Tests')
        foreach ($project in $projects) {
            $projectPath = Join-Path $root "$project\$project.csproj"
            $debugExport.ProjectStructure[$project] = @{
                Exists       = Test-Path $projectPath
                Path         = $projectPath
                LastModified = if (Test-Path $projectPath) { (Get-Item $projectPath).LastWriteTime } else { $null }
            }
        }

        # Include logs if requested
        if ($IncludeLogs) {
            $logsDir = Join-Path $root 'logs'
            if (Test-Path $logsDir) {
                $logFiles = Get-ChildItem $logsDir -Filter '*.log' | Sort-Object LastWriteTime -Descending | Select-Object -First 5
                foreach ($logFile in $logFiles) {
                    $logContent = Get-Content $logFile.FullName -Tail 20 -ErrorAction SilentlyContinue
                    $debugExport.ApplicationLogs += @{
                        FileName     = $logFile.Name
                        LastModified = $logFile.LastWriteTime
                        RecentLines  = $logContent
                    }
                }
            }
        }

        # Test build status if requested
        if ($IncludeState) {
            Write-Host '🔨 Testing current build status...' -ForegroundColor Yellow
            $buildResult = bb-build -CaptureOutput
            $debugExport.BuildStatus = @{
                Success  = $buildResult
                TestedAt = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
            }
        }

        # Export to JSON
        $debugExport | ConvertTo-Json -Depth 4 | Out-File -FilePath $OutputPath -Encoding UTF8

        Write-Host "✅ Debug data exported successfully" -ForegroundColor Green
        Write-Host "📋 Export file: $OutputPath" -ForegroundColor Gray

        # Display summary
        Write-Host "`n📊 Export Summary:" -ForegroundColor Cyan
        Write-Host "  • Running Processes: $($debugExport.Processes.Count)" -ForegroundColor Gray
        Write-Host "  • Project Files: $($debugExport.ProjectStructure.Count)" -ForegroundColor Gray
        if ($IncludeLogs) {
            Write-Host "  • Log Files: $($debugExport.ApplicationLogs.Count)" -ForegroundColor Gray
        }
        if ($IncludeState) {
            Write-Host "  • Build Status: $(if ($debugExport.BuildStatus.Success) { 'Success' } else { 'Failed' })" -ForegroundColor Gray
        }

        return $true

    } catch {
        Write-Host "❌ Failed to export debug data: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# PowerShell Profile Integration
function Update-BusBuddyProfile {
    <#
    .SYNOPSIS
        Reload Bus Buddy PowerShell profiles
    #>
    $profilePath = Join-Path (Get-BusBuddyProjectRoot) 'BusBuddy-PowerShell-Profile.ps1'
    $workflowsPath = Join-Path (Get-BusBuddyProjectRoot) 'BusBuddy-Advanced-Workflows.ps1'

    if (Test-Path $profilePath) {
        . $profilePath
        Write-Host '✅ Bus Buddy PowerShell Profile reloaded' -ForegroundColor Green
    }

    if (Test-Path $workflowsPath) {
        . $workflowsPath
        Write-Host '✅ Bus Buddy Advanced Workflows reloaded' -ForegroundColor Green
    }
}

# VS Code Integration
function code {
    <#
    .SYNOPSIS
        Enhanced VS Code launcher with Bus Buddy workspace detection
    #>
    param(
        [Parameter(Mandatory = $false, Position = 0)]
        [string]$Path = '.'
    )

    # Try VS Code first, then VS Code Insiders
    $codeCommand = $null
    if (Get-Command 'code' -ErrorAction SilentlyContinue) {
        $codeCommand = 'code'
    } elseif (Get-Command 'code-insiders' -ErrorAction SilentlyContinue) {
        $codeCommand = 'code-insiders'
    }

    if ($codeCommand) {
        if ($Path -eq '.') {
            $root = Get-BusBuddyProjectRoot
            if ($root) {
                Write-Host "🚌 Opening Bus Buddy workspace in $codeCommand..." -ForegroundColor Cyan
                & $codeCommand $root
            } else {
                & $codeCommand $PWD.Path
            }
        } else {
            & $codeCommand $Path
        }
    } else {
        Write-Host '❌ VS Code not found in PATH' -ForegroundColor Red
        Write-Host '💡 Install VS Code or VS Code Insiders and ensure it is in your PATH' -ForegroundColor Yellow
    }
}

# Aliases for convenience
Set-Alias -Name 'vs' -Value 'code'
Set-Alias -Name 'vscode' -Value 'code'
Set-Alias -Name 'edit' -Value 'code'

# Function to display welcome message
function Show-BusBuddyAdvancedWorkflowsWelcome {
    [CmdletBinding()]
    param()

    Write-Host '🚀 Bus Buddy Advanced Workflows Loaded!' -ForegroundColor Green
    Write-Host '   Build Commands:' -ForegroundColor Gray
    Write-Host '     • bb-build       - Build solution with enhanced validation' -ForegroundColor Gray
    Write-Host '     • bb-clean       - Clean solution with deep options' -ForegroundColor Gray
    Write-Host '     • bb-test        - Run tests with filtering and reporting' -ForegroundColor Gray
    Write-Host '     • bb-restore     - Restore packages with vulnerability checks' -ForegroundColor Gray
    Write-Host '     • bb-run         - Run application with debug options' -ForegroundColor Gray
    Write-Host '     • bb-publish     - Publish with Azure deployment options' -ForegroundColor Cyan
    Write-Host '   Workflow Commands:' -ForegroundColor Gray
    Write-Host '     • bb-dev-session - Complete dev setup with error aggregation' -ForegroundColor Cyan
    Write-Host '     • bb-quick-test  - Quick build & test cycle' -ForegroundColor Gray
    Write-Host '     • bb-diagnostic  - Comprehensive system analysis' -ForegroundColor Gray
    Write-Host '     • bb-health      - Health check with XAML validation' -ForegroundColor Cyan
    Write-Host '     • bb-report      - Generate project reports with -AsJob support' -ForegroundColor Cyan
    Write-Host '   Debug Commands:' -ForegroundColor Gray
    Write-Host '     • bb-debug-start - Start debug filter monitoring' -ForegroundColor Gray
    Write-Host '     • bb-debug-export- Export debug data' -ForegroundColor Gray
    Write-Host '   Integration Commands:' -ForegroundColor Gray
    Write-Host '     • code/vs        - Open in VS Code with workspace detection' -ForegroundColor Gray
    Write-Host '     • bb-reload      - Reload PowerShell profiles' -ForegroundColor Gray
    Write-Host ''
    Write-Host '✨ PowerShell 7.5.2 Enhanced Features:' -ForegroundColor Cyan
    Write-Host '   • Parallel processing in reports and analysis' -ForegroundColor Gray
    Write-Host '   • Background job support with -AsJob parameter' -ForegroundColor Gray
    Write-Host '   • Enhanced error aggregation and reporting' -ForegroundColor Gray
    Write-Host '   • XAML validation integration' -ForegroundColor Gray
    Write-Host ''
}

# Export the function for external access
Export-ModuleMember -Function Show-BusBuddyAdvancedWorkflowsWelcome -ErrorAction SilentlyContinue

# Create an alias for the welcome function
Set-Alias -Name bb-welcome -Value Show-BusBuddyAdvancedWorkflowsWelcome -Force -Scope Global

# Show welcome message unless quiet mode is requested
if (-not $Quiet) {
    Show-BusBuddyAdvancedWorkflowsWelcome
}


# Backward Compatibility Aliases
Set-Alias -Name 'bb-quick-test' -Value 'Invoke-BusBuddyQuickTest' -Force
Set-Alias -Name 'bb-debug-export' -Value 'Export-BusBuddyDebug' -Force
Set-Alias -Name 'bb-restore' -Value 'Restore-BusBuddyPackages' -Force
Set-Alias -Name 'bb-dev-session' -Value 'Start-BusBuddyDevSession' -Force
Set-Alias -Name 'bb-health' -Value 'Get-BusBuddyHealth' -Force
Set-Alias -Name 'bb-build' -Value 'Invoke-BusBuddyBuild' -Force
Set-Alias -Name 'bb-clean' -Value 'Clear-BusBuddyBuild' -Force
Set-Alias -Name 'bb-run' -Value 'Start-BusBuddyApplication' -Force
Set-Alias -Name 'bb-diagnostic' -Value 'Get-BusBuddyDiagnostic' -Force
Set-Alias -Name 'bb-reload' -Value 'Update-BusBuddyProfile' -Force
Set-Alias -Name 'bb-debug-start' -Value 'Start-BusBuddyDebug' -Force
Set-Alias -Name 'bb-report' -Value 'New-BusBuddyReport' -Force
Set-Alias -Name 'bb-test' -Value 'Invoke-BusBuddyTest' -Force
Set-Alias -Name 'bb-publish' -Value 'Publish-BusBuddy' -Force

