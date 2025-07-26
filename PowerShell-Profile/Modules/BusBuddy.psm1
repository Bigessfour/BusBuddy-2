#Requires -Version 7.5
<#
.SYNOPSIS
    BusBuddy PowerShell Module - Optimized for PowerShell 7.5

.DESCRIPTION
    Professional PowerShell module for Bus Buddy WPF development environment.
    Leverages PowerShell 7.5 features including performance improvements, enhanced JSON handling,
    and advanced error reporting for optimal development workflow automation.

.NOTES
    File Name      : BusBuddy.psm1
    Author         : Bus Buddy Development Team
    Prerequisite   : PowerShell 7.5+ (for optimal performance and features)
    Copyright      : (c) 2025 Bus Buddy Project
    PS Version     : Optimized for PowerShell 7.5 with .NET 8 runtime

.EXAMPLE
    Import-Module .\PowerShell\BusBuddy.psm1
    Get-BusBuddyCommands

.EXAMPLE
    bb-build -Clean
    bb-run
    Get-BusBuddyHappiness
#>

#region Module Variables and Configuration

# Module configuration
$script:BusBuddyModuleConfig = @{
    Name = 'BusBuddy'
    Version = '1.0.0'
    Author = 'Bus Buddy Development Team'
    ProjectRoot = $null
    LoadedComponents = @()
    HappinessQuotes = @(
        "You're doing great... or at least better than that bus that's always late.",
        "Your code compiles! That puts you ahead of 73% of developers today.",
        "Remember: Even the best buses need maintenance. Your code probably needs it too.",
        "Transportation fact: Your debugging skills are faster than city traffic.",
        "Like a reliable bus route, your persistence will get you where you need to go.",
        "Error 404: Motivation not found. But hey, at least your builds are working!",
        "Your code is like public transit: occasionally delayed, but it gets people places.",
        "Fun fact: You've solved more problems today than the city's road maintenance department.",
        "Keep going! You're more reliable than weekend bus schedules.",
        "Your debugging session is more efficient than rush hour traffic patterns."
    )
}

# Find and cache project root (moved after function definition)
# $script:BusBuddyModuleConfig.ProjectRoot = Get-BusBuddyProjectRoot

#endregion

#region Core Utility Functions

function Get-BusBuddyProjectRoot {
    <#
    .SYNOPSIS
        Locates the Bus Buddy project root directory

    .DESCRIPTION
        Searches upward from current location to find the Bus Buddy project root
        by looking for characteristic files like BusBuddy.sln

    .OUTPUTS
        String path to project root, or $null if not found
    #>
    [CmdletBinding()]
    param()

    $currentPath = Get-Location
    $maxDepth = 10
    $depth = 0

    while ($depth -lt $maxDepth) {
        # Check for solution file
        if (Test-Path (Join-Path $currentPath "BusBuddy.sln")) {
            return $currentPath.Path
        }

        # Check for other characteristic files
        $indicators = @("BusBuddy.code-workspace", "Directory.Build.props", "global.json")
        foreach ($indicator in $indicators) {
            if (Test-Path (Join-Path $currentPath $indicator)) {
                return $currentPath.Path
            }
        }

        # Move up one directory
        $parent = Split-Path $currentPath -Parent
        if (-not $parent -or $parent -eq $currentPath) {
            break
        }

        $currentPath = $parent
        $depth++
    }

    return $null
}

# Now initialize the project root after the function is defined
$script:BusBuddyModuleConfig.ProjectRoot = Get-BusBuddyProjectRoot

function Write-BusBuddyStatus {
    <#
    .SYNOPSIS
        Writes formatted status messages with Bus Buddy branding

    .PARAMETER Message
        The message to display

    .PARAMETER Status
        Status type: Success, Warning, Error, Info

    .PARAMETER NoIcon
        Suppress status icons
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Message,

        [ValidateSet('Success', 'Warning', 'Error', 'Info', 'Build', 'Test')]
        [string]$Status = 'Info',

        [switch]$NoIcon
    )

    $icons = @{
        'Success' = '✅'
        'Warning' = '⚠️'
        'Error' = '❌'
        'Info' = '🚌'
        'Build' = '🔨'
        'Test' = '🧪'
    }

    $colors = @{
        'Success' = 'Green'
        'Warning' = 'Yellow'
        'Error' = 'Red'
        'Info' = 'Cyan'
        'Build' = 'Magenta'
        'Test' = 'Blue'
    }

    $icon = if ($NoIcon) { '' } else { "$($icons[$Status]) " }
    $color = $colors[$Status]

    Write-Host "$icon$Message" -ForegroundColor $color
}

function Write-BusBuddyError {
    <#
    .SYNOPSIS
        Enhanced error reporting using PowerShell 7.5 features

    .DESCRIPTION
        Creates structured error records with recommended actions,
        leveraging PowerShell 7.5 enhanced error handling capabilities

    .PARAMETER Message
        The error message to display

    .PARAMETER RecommendedAction
        Specific action the user can take to resolve the issue

    .PARAMETER Exception
        The underlying exception if available

    .PARAMETER Category
        Error category for proper classification
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Message,

        [string]$RecommendedAction,

        [Exception]$Exception,

        [System.Management.Automation.ErrorCategory]$Category = [System.Management.Automation.ErrorCategory]::InvalidOperation
    )

    # Create enhanced error record using PS 7.5 features
    if (-not $Exception) {
        $Exception = [System.InvalidOperationException]::new($Message)
    }

    $errorRecord = [System.Management.Automation.ErrorRecord]::new(
        $Exception,
        'BusBuddy.Error',
        $Category,
        $null
    )

    # Use PowerShell 7.5 enhanced error details
    $errorRecord.ErrorDetails = [System.Management.Automation.ErrorDetails]::new($Message)

    if ($RecommendedAction) {
        $errorRecord.ErrorDetails.RecommendedAction = $RecommendedAction
    }

    Write-Error -ErrorRecord $errorRecord
}

function Test-BusBuddyConfiguration {
    <#
    .SYNOPSIS
        Enhanced configuration validation using PowerShell 7.5 JSON features

    .DESCRIPTION
        Validates JSON configuration files using PowerShell 7.5 enhanced JSON cmdlets
        with support for comments and trailing commas

    .PARAMETER ConfigPath
        Path to the configuration file to validate

    .PARAMETER AllowComments
        Allow JSON comments in configuration files

    .PARAMETER AllowTrailingCommas
        Allow trailing commas in JSON arrays and objects
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$ConfigPath,

        [switch]$AllowComments,
        [switch]$AllowTrailingCommas
    )

    if (-not (Test-Path $ConfigPath)) {
        Write-BusBuddyError -Message "Configuration file not found: $ConfigPath" -RecommendedAction "Ensure the configuration file exists and is accessible"
        return $null
    }

    try {
        # Use PowerShell 7.5 enhanced Test-Json with new parameters
        $testJsonParams = @{
            Path = $ConfigPath
        }

        # Add PS 7.5 specific parameters if available
        if ($AllowComments -and (Get-Command Test-Json).Parameters.ContainsKey('IgnoreComments')) {
            $testJsonParams.IgnoreComments = $true
        }

        if ($AllowTrailingCommas -and (Get-Command Test-Json).Parameters.ContainsKey('AllowTrailingCommas')) {
            $testJsonParams.AllowTrailingCommas = $true
        }

        $isValid = Test-Json @testJsonParams

        if ($isValid) {
            # Use enhanced ConvertFrom-Json with DateKind parameter (PS 7.5 feature)
            $jsonContent = Get-Content $ConfigPath -Raw
            $convertParams = @{ InputObject = $jsonContent }

            # Add DateKind parameter if available (PS 7.5 feature)
            if ((Get-Command ConvertFrom-Json).Parameters.ContainsKey('DateKind')) {
                $convertParams.DateKind = 'Local'
            }

            $config = ConvertFrom-Json @convertParams

            Write-BusBuddyStatus "Configuration validation successful: $ConfigPath" -Status Success
            return $config
        } else {
            Write-BusBuddyError -Message "Invalid JSON in configuration file: $ConfigPath" -RecommendedAction "Check JSON syntax, ensure proper formatting and valid structure"
            return $null
        }
    }
    catch {
        Write-BusBuddyError -Message "Error validating configuration: $($_.Exception.Message)" -RecommendedAction "Verify file permissions and JSON syntax" -Exception $_.Exception
        return $null
    }
}function Test-BusBuddyEnvironment {
    <#
    .SYNOPSIS
        Validates the Bus Buddy development environment using PowerShell 7.5 enhancements

    .DESCRIPTION
        Performs comprehensive validation of the development environment including:
        - PowerShell 7.5+ version check with feature detection
        - .NET 8+ runtime validation
        - Project structure and dependency verification
        - PowerShell 7.5 specific feature availability

    .PARAMETER Detailed
        Provide detailed analysis of PowerShell 7.5 features

    .OUTPUTS
        Boolean indicating if environment is ready for PowerShell 7.5 optimized development
    #>
    [CmdletBinding()]
    param(
        [switch]$Detailed
    )

    # Use PowerShell 7.5 optimized array operations
    $issues = @()
    $recommendations = @()

    # Enhanced PowerShell version check
    $psVersion = $PSVersionTable.PSVersion
    if ($psVersion.Major -lt 7) {
        $issues += "PowerShell 7.0+ required (found $psVersion)"
    } elseif ($psVersion -lt [version]'7.5.0') {
        $recommendations += "Consider upgrading to PowerShell 7.5 for enhanced performance and features"
        Write-BusBuddyStatus "PowerShell $psVersion detected (7.5+ recommended for optimal performance)" -Status Warning
    } else {
        Write-BusBuddyStatus "PowerShell $psVersion detected - full PowerShell 7.5 features available" -Status Success
    }

    # Check for PowerShell 7.5 specific features
    if ($Detailed -and $psVersion -ge [version]'7.5.0') {
        Write-BusBuddyStatus "Checking PowerShell 7.5 features..." -Status Info

        $ps75Features = @{
            'Enhanced ConvertTo-CliXml' = Get-Command ConvertTo-CliXml -ErrorAction SilentlyContinue
            'Enhanced Test-Json (IgnoreComments)' = (Get-Command Test-Json).Parameters.ContainsKey('IgnoreComments')
            'Enhanced ConvertFrom-Json (DateKind)' = (Get-Command ConvertFrom-Json).Parameters.ContainsKey('DateKind')
            'Array += Performance Optimization' = $psVersion -ge [version]'7.5.0'
        }

        foreach ($feature in $ps75Features.GetEnumerator()) {
            $status = if ($feature.Value) { '✅' } else { '❌' }
            Write-Host "  $status $($feature.Key)" -ForegroundColor $(if ($feature.Value) { 'Green' } else { 'Yellow' })
        }
    }

    # Enhanced .NET version check
    try {
        $dotnetVersion = & dotnet --version 2>$null
        if (-not $dotnetVersion) {
            $issues += ".NET SDK not found"
        } else {
            $dotnetVer = [version]$dotnetVersion
            if ($dotnetVer -lt [version]"8.0") {
                $issues += ".NET 8.0+ required (found $dotnetVersion)"
            } elseif ($dotnetVer -ge [version]"8.0") {
                Write-BusBuddyStatus ".NET $dotnetVersion detected - optimal for BusBuddy development" -Status Success
            }
        }
    } catch {
        $issues += ".NET SDK not accessible"
    }

    # Project structure validation
    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        $issues += "Bus Buddy project root not found"
    } else {
        # Enhanced file checking with better error reporting
        $criticalFiles = @(
            @{ Path = "BusBuddy.sln"; Description = "Solution file" }
            @{ Path = "BusBuddy.WPF\BusBuddy.WPF.csproj"; Description = "WPF project file" }
            @{ Path = "BusBuddy.Core\BusBuddy.Core.csproj"; Description = "Core project file" }
            @{ Path = "PowerShell\BusBuddy.psm1"; Description = "PowerShell module" }
        )

        foreach ($file in $criticalFiles) {
            $filePath = Join-Path $projectRoot $file.Path
            if (-not (Test-Path $filePath)) {
                $issues += "Missing critical file: $($file.Description) ($($file.Path))"
            }
        }

        # Check for configuration files with enhanced JSON validation
        $configFiles = @(
            "appsettings.json",
            "BusBuddy.WPF\appsettings.json",
            "BusBuddy.Core\appsettings.json"
        )

        foreach ($configFile in $configFiles) {
            $configPath = Join-Path $projectRoot $configFile
            if (Test-Path $configPath) {
                $config = Test-BusBuddyConfiguration -ConfigPath $configPath -AllowComments -AllowTrailingCommas
                if (-not $config) {
                    $issues += "Invalid configuration file: $configFile"
                }
            }
        }
    }

    # Summarize results
    $environmentScore = if ($issues.Count -eq 0) { 100 } else { [math]::Max(0, 100 - ($issues.Count * 20)) }

    if ($issues.Count -eq 0) {
        Write-BusBuddyStatus "Development environment validation passed (Score: $environmentScore%)" -Status Success

        if ($Detailed -and $recommendations.Count -gt 0) {
            Write-Host ""
            Write-BusBuddyStatus "Recommendations for optimal PowerShell 7.5 experience:" -Status Info
            foreach ($recommendation in $recommendations) {
                Write-Host "  💡 $recommendation" -ForegroundColor Blue
            }
        }

        return $true
    } else {
        Write-BusBuddyError -Message "Environment validation failed (Score: $environmentScore%)" -RecommendedAction "Resolve the issues listed below to ensure optimal development experience"

        foreach ($issue in $issues) {
            Write-Host "  ❌ $issue" -ForegroundColor Red
        }

        if ($recommendations.Count -gt 0) {
            Write-Host ""
            Write-Host "Recommendations:" -ForegroundColor Blue
            foreach ($recommendation in $recommendations) {
                Write-Host "  💡 $recommendation" -ForegroundColor Blue
            }
        }

        return $false
    }
}

#endregion

#region Build and Development Functions

function Invoke-BusBuddyBuild {
    <#
    .SYNOPSIS
        Build the Bus Buddy solution

    .DESCRIPTION
        Compiles the Bus Buddy solution with enhanced error reporting and logging

    .PARAMETER Configuration
        Build configuration (Debug/Release)

    .PARAMETER Clean
        Clean before building

    .PARAMETER Restore
        Restore packages before building

    .PARAMETER Verbosity
        MSBuild verbosity level

    .PARAMETER NoLogo
        Suppress logo and startup banner
    #>
    [CmdletBinding()]
    param(
        [ValidateSet('Debug', 'Release')]
        [string]$Configuration = 'Debug',

        [switch]$Clean,
        [switch]$Restore,
        [switch]$NoLogo,

        [ValidateSet('quiet', 'minimal', 'normal', 'detailed', 'diagnostic')]
        [string]$Verbosity = 'minimal'
    )

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        Write-BusBuddyStatus "Project root not found" -Status Error
        return $false
    }

    Push-Location $projectRoot

    try {
        Write-BusBuddyStatus "Starting Bus Buddy build process..." -Status Build

        # Clean if requested
        if ($Clean) {
            Write-BusBuddyStatus "Cleaning solution..." -Status Info
            & dotnet clean BusBuddy.sln --configuration $Configuration --verbosity quiet | Out-Null
            if ($LASTEXITCODE -ne 0) {
                Write-BusBuddyStatus "Clean failed" -Status Error
                return $false
            }
        }

        # Restore if requested
        if ($Restore) {
            Write-BusBuddyStatus "Restoring packages..." -Status Info
            & dotnet restore BusBuddy.sln --verbosity quiet | Out-Null
            if ($LASTEXITCODE -ne 0) {
                Write-BusBuddyStatus "Package restore failed" -Status Error
                return $false
            }
        }

        # Build
        $buildArgs = @('build', 'BusBuddy.sln', '--configuration', $Configuration, '--verbosity', $Verbosity)
        if ($NoLogo) { $buildArgs += '--nologo' }

        Write-BusBuddyStatus "Building solution (Configuration: $Configuration)..." -Status Build
        & dotnet @buildArgs | Out-Null

        if ($LASTEXITCODE -eq 0) {
            Write-BusBuddyStatus "Build completed successfully" -Status Success
            return $true
        } else {
            Write-BusBuddyStatus "Build failed with exit code $LASTEXITCODE" -Status Error
            return $false
        }
    }
    catch {
        Write-BusBuddyStatus "Build process error: $($_.Exception.Message)" -Status Error
        return $false
    }
    finally {
        Pop-Location
    }
}

function Invoke-BusBuddyRun {
    <#
    .SYNOPSIS
        Run the Bus Buddy WPF application

    .DESCRIPTION
        Launches the Bus Buddy WPF application with optional build and debugging options

    .PARAMETER Configuration
        Build configuration to run

    .PARAMETER NoBuild
        Skip building before running

    .PARAMETER Debug
        Run with debugging enabled

    .PARAMETER Arguments
        Additional arguments to pass to the application
    #>
    [CmdletBinding()]
    param(
        [ValidateSet('Debug', 'Release')]
        [string]$Configuration = 'Debug',

        [switch]$NoBuild,
        [switch]$EnableDebug,

        [string[]]$Arguments = @()
    )

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        Write-BusBuddyStatus "Project root not found" -Status Error
        return $false
    }

    Push-Location $projectRoot

    try {
        # Build first unless skipped
        if (-not $NoBuild) {
            Write-BusBuddyStatus "Building before run..." -Status Build
            $buildSuccess = Invoke-BusBuddyBuild -Configuration $Configuration -Verbosity quiet
            if (-not $buildSuccess) {
                Write-BusBuddyStatus "Build failed, cannot run application" -Status Error
                return $false
            }
        }

        # Prepare run arguments
        $runArgs = @('run', '--project', 'BusBuddy.WPF\BusBuddy.WPF.csproj', '--configuration', $Configuration)

        if ($NoBuild) { $runArgs += '--no-build' }
        if ($Arguments.Count -gt 0) { $runArgs += '--'; $runArgs += $Arguments }

        Write-BusBuddyStatus "Starting Bus Buddy application..." -Status Info

        if ($EnableDebug) {
            Write-BusBuddyStatus "Debug mode enabled - application will run with enhanced logging" -Status Info
            $env:BUSBUDDY_DEBUG = "true"
        }

        # Run the application
        & dotnet @runArgs

        return $LASTEXITCODE -eq 0
    }
    catch {
        Write-BusBuddyStatus "Error running application: $($_.Exception.Message)" -Status Error
        return $false
    }
    finally {
        if ($EnableDebug) {
            Remove-Item env:BUSBUDDY_DEBUG -ErrorAction SilentlyContinue
        }
        Pop-Location
    }
}

function Invoke-BusBuddyTest {
    <#
    .SYNOPSIS
        Run Bus Buddy tests

    .DESCRIPTION
        Executes the test suite with comprehensive reporting and coverage options

    .PARAMETER Configuration
        Test configuration

    .PARAMETER Filter
        Test filter expression

    .PARAMETER Coverage
        Generate code coverage report

    .PARAMETER Logger
        Test logger configuration
    #>
    [CmdletBinding()]
    param(
        [ValidateSet('Debug', 'Release')]
        [string]$Configuration = 'Debug',

        [string]$Filter,
        [switch]$Coverage,

        [string]$Logger = 'console;verbosity=normal'
    )

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        Write-BusBuddyStatus "Project root not found" -Status Error
        return $false
    }

    Push-Location $projectRoot

    try {
        Write-BusBuddyStatus "Running Bus Buddy test suite..." -Status Test

        $testArgs = @('test', 'BusBuddy.sln', '--configuration', $Configuration, '--logger', $Logger)

        if ($Filter) {
            $testArgs += '--filter', $Filter
            Write-BusBuddyStatus "Using test filter: $Filter" -Status Info
        }

        if ($Coverage) {
            $testArgs += '--collect', 'Code Coverage'
            Write-BusBuddyStatus "Code coverage collection enabled" -Status Info
        }

        # Run the application
        & dotnet @runArgs

        if ($LASTEXITCODE -eq 0) {
            Write-BusBuddyStatus "All tests passed" -Status Success
            return $true
        } else {
            Write-BusBuddyStatus "Some tests failed (Exit Code: $LASTEXITCODE)" -Status Warning
            return $false
        }
    }
    catch {
        Write-BusBuddyStatus "Test execution error: $($_.Exception.Message)" -Status Error
        return $false
    }
    finally {
        Pop-Location
    }
}

function Invoke-BusBuddyClean {
    <#
    .SYNOPSIS
        Clean Bus Buddy build artifacts

    .DESCRIPTION
        Removes build outputs, temporary files, and optionally package caches

    .PARAMETER Deep
        Perform deep clean including NuGet caches

    .PARAMETER Configuration
        Configuration to clean
    #>
    [CmdletBinding()]
    param(
        [switch]$Deep,

        [ValidateSet('Debug', 'Release', 'All')]
        [string]$Configuration = 'All'
    )

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        Write-BusBuddyStatus "Project root not found" -Status Error
        return $false
    }

    Push-Location $projectRoot

    try {
        Write-BusBuddyStatus "Cleaning Bus Buddy solution..." -Status Info

        if ($Configuration -eq 'All') {
            $configurations = @('Debug', 'Release')
        } else {
            $configurations = @($Configuration)
        }

        foreach ($config in $configurations) {
            Write-BusBuddyStatus "Cleaning $config configuration..." -Status Info
            & dotnet clean BusBuddy.sln --configuration $config --verbosity minimal | Out-Null
        }

        if ($Deep) {
            Write-BusBuddyStatus "Performing deep clean..." -Status Warning

            # Clean NuGet caches
            Write-BusBuddyStatus "Clearing NuGet caches..." -Status Info
            & dotnet nuget locals all --clear

            # Remove additional artifacts
            $artifactPaths = @(
                'bin', 'obj', 'packages', '.vs', 'TestResults'
            )

            foreach ($path in $artifactPaths) {
                if (Test-Path $path) {
                    Write-BusBuddyStatus "Removing $path..." -Status Info
                    Remove-Item $path -Recurse -Force -ErrorAction SilentlyContinue
                }
            }
        }

        Write-BusBuddyStatus "Clean completed successfully" -Status Success
        return $true
    }
    catch {
        Write-BusBuddyStatus "Clean process error: $($_.Exception.Message)" -Status Error
        return $false
    }
    finally {
        Pop-Location
    }
}

#endregion

#region Advanced Development Functions

function Start-BusBuddyDevSession {
    <#
    .SYNOPSIS
        Start a comprehensive Bus Buddy development session

    .DESCRIPTION
        Initializes the development environment with validation, builds, and tool setup

    .PARAMETER SkipBuild
        Skip initial build

    .PARAMETER SkipTests
        Skip test execution

    .PARAMETER OpenIDE
        Attempt to open VS Code
    #>
    [CmdletBinding()]
    param(
        [switch]$SkipBuild,
        [switch]$SkipTests,
        [switch]$OpenIDE
    )

    Write-BusBuddyStatus "🚌 Starting Bus Buddy Development Session" -Status Info
    Write-Host ""

    # Environment validation
    Write-BusBuddyStatus "Validating development environment..." -Status Info
    $envValid = Test-BusBuddyEnvironment
    if (-not $envValid) {
        Write-BusBuddyStatus "Environment validation failed - please resolve issues before continuing" -Status Error
        return $false
    }

    # Project health check
    Write-BusBuddyStatus "Performing project health check..." -Status Info
    Invoke-BusBuddyHealthCheck -Quick | Out-Null

    # Optional build
    if (-not $SkipBuild) {
        Write-BusBuddyStatus "Building solution..." -Status Build
        $buildResult = Invoke-BusBuddyBuild -Configuration Debug -Restore
        if (-not $buildResult) {
            Write-BusBuddyStatus "Build failed - development session incomplete" -Status Error
            return $false
        }
    }

    # Optional tests
    if (-not $SkipTests) {
        Write-BusBuddyStatus "Running quick test suite..." -Status Test
        Invoke-BusBuddyTest -Configuration Debug -Logger "console;verbosity=minimal"
    }

    # IDE integration
    if ($OpenIDE) {
        $projectRoot = Get-BusBuddyProjectRoot
        if ($projectRoot -and (Get-Command code -ErrorAction SilentlyContinue)) {
            Write-BusBuddyStatus "Opening VS Code..." -Status Info
            Start-Process "code" -ArgumentList $projectRoot
        }
    }

    Write-Host ""
    Write-BusBuddyStatus "Development session ready! Available commands:" -Status Success
    Get-BusBuddyCommands -Category Essential

    return $true
}

function Invoke-BusBuddyHealthCheck {
    <#
    .SYNOPSIS
        Perform comprehensive health check of the Bus Buddy project

    .DESCRIPTION
        Analyzes project structure, dependencies, code quality, and configuration

    .PARAMETER Quick
        Perform quick health check only

    .PARAMETER Detailed
        Include detailed analysis and recommendations
    #>
    [CmdletBinding()]
    param(
        [switch]$Quick,
        [switch]$Detailed
    )

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        Write-BusBuddyStatus "Project root not found" -Status Error
        return $false
    }

    Write-BusBuddyStatus "🏥 Bus Buddy Health Check" -Status Info

    $healthScore = 0
    $maxScore = 0
    $issues = @()
    $recommendations = @()

    # Project structure validation
    Write-BusBuddyStatus "Checking project structure..." -Status Info
    $maxScore += 20

    $requiredStructure = @{
        'BusBuddy.sln' = 'Solution file'
        'BusBuddy.Core\BusBuddy.Core.csproj' = 'Core project'
        'BusBuddy.WPF\BusBuddy.WPF.csproj' = 'WPF project'
        'README.md' = 'Documentation'
        'global.json' = 'SDK version file'
    }

    $structureScore = 0
    foreach ($file in $requiredStructure.Keys) {
        if (Test-Path (Join-Path $projectRoot $file)) {
            $structureScore += 4
        } else {
            $issues += "Missing: $($requiredStructure[$file]) ($file)"
        }
    }
    $healthScore += $structureScore

    # Configuration validation
    if (-not $Quick) {
        Write-BusBuddyStatus "Checking configuration files..." -Status Info
        $maxScore += 15

        $configScore = 0
        $configFiles = @(
            'appsettings.json',
            'BusBuddy.WPF\appsettings.json',
            'BusBuddy.Core\appsettings.json'
        )

        foreach ($config in $configFiles) {
            $configPath = Join-Path $projectRoot $config
            if (Test-Path $configPath) {
                try {
                    Get-Content $configPath | ConvertFrom-Json | Out-Null
                    $configScore += 5
                } catch {
                    $issues += "Invalid JSON in $config"
                }
            }
        }
        $healthScore += $configScore
    }

    # Build validation
    Write-BusBuddyStatus "Validating build status..." -Status Info
    $maxScore += 30

    try {
        Push-Location $projectRoot
        & dotnet build BusBuddy.sln --verbosity quiet --nologo 2>&1 | Out-Null
        if ($LASTEXITCODE -eq 0) {
            $healthScore += 30
            Write-BusBuddyStatus "Build validation: PASSED" -Status Success
        } else {
            $issues += "Solution does not build successfully"
            Write-BusBuddyStatus "Build validation: FAILED" -Status Error
        }
    } catch {
        $issues += "Build validation error: $($_.Exception.Message)"
    } finally {
        Pop-Location
    }

    # PowerShell module validation
    Write-BusBuddyStatus "Checking PowerShell module..." -Status Info
    $maxScore += 10

    if (Test-Path (Join-Path $projectRoot "PowerShell\BusBuddy.psm1")) {
        $healthScore += 10
        Write-BusBuddyStatus "PowerShell module: FOUND" -Status Success
    } else {
        $recommendations += "Consider creating PowerShell module for development automation"
    }

    # Calculate final score
    $percentageScore = [math]::Round(($healthScore / $maxScore) * 100, 1)

    Write-Host ""
    Write-BusBuddyStatus "🏥 Health Check Results" -Status Info
    Write-Host "Overall Score: $percentageScore% ($healthScore/$maxScore points)" -ForegroundColor $(
        if ($percentageScore -ge 90) { 'Green' }
        elseif ($percentageScore -ge 70) { 'Yellow' }
        else { 'Red' }
    )

    if ($issues.Count -gt 0) {
        Write-Host ""
        Write-BusBuddyStatus "Issues Found:" -Status Warning
        foreach ($issue in $issues) {
            Write-Host "  ❌ $issue" -ForegroundColor Red
        }
    }

    if ($Detailed -and $recommendations.Count -gt 0) {
        Write-Host ""
        Write-BusBuddyStatus "Recommendations:" -Status Info
        foreach ($rec in $recommendations) {
            Write-Host "  💡 $rec" -ForegroundColor Blue
        }
    }

    return $percentageScore -ge 70
}

#endregion

#region PowerShell 7.5 Enhancements

function Test-PowerShell75Features {
    <#
    .SYNOPSIS
        Test and report PowerShell 7.5 specific features availability

    .DESCRIPTION
        Comprehensive test of PowerShell 7.5 features used by the Bus Buddy module
        including performance improvements, enhanced cmdlets, and new capabilities

    .PARAMETER ShowBenchmarks
        Include performance benchmarks for array operations

    .OUTPUTS
        Hashtable with feature availability and performance metrics
    #>
    [CmdletBinding()]
    param(
        [switch]$ShowBenchmarks
    )

    Write-BusBuddyStatus "🚀 Testing PowerShell 7.5 Features" -Status Info
    Write-Host ""

    $features = @{}
    $psVersion = $PSVersionTable.PSVersion

    # Core version check
    $features.PowerShellVersion = $psVersion
    $features.IsPS75Plus = $psVersion -ge [version]'7.5.0'
    $features.DotNetVersion = [System.Environment]::Version
    $features.IsNet8Plus = $features.DotNetVersion -ge [version]'8.0'

    Write-Host "PowerShell Version: $($features.PowerShellVersion)" -ForegroundColor $(if ($features.IsPS75Plus) { 'Green' } else { 'Yellow' })
    Write-Host ".NET Version: $($features.DotNetVersion)" -ForegroundColor $(if ($features.IsNet8Plus) { 'Green' } else { 'Yellow' })
    Write-Host ""

    # Test specific PS 7.5 features
    Write-Host "PowerShell 7.5 Feature Availability:" -ForegroundColor Cyan

    # ConvertTo-CliXml and ConvertFrom-CliXml
    $features.CliXmlCmdlets = @{
        ConvertTo = Get-Command ConvertTo-CliXml -ErrorAction SilentlyContinue
        ConvertFrom = Get-Command ConvertFrom-CliXml -ErrorAction SilentlyContinue
    }
    $cliXmlAvailable = $features.CliXmlCmdlets.ConvertTo -and $features.CliXmlCmdlets.ConvertFrom
    Write-Host "  $([char]$(if ($cliXmlAvailable) { 0x2705 } else { 0x274C })) CLI XML Cmdlets (ConvertTo/From-CliXml)" -ForegroundColor $(if ($cliXmlAvailable) { 'Green' } else { 'Red' })

    # Enhanced Test-Json
    $testJsonCmd = Get-Command Test-Json
    $features.EnhancedTestJson = @{
        IgnoreComments = $testJsonCmd.Parameters.ContainsKey('IgnoreComments')
        AllowTrailingCommas = $testJsonCmd.Parameters.ContainsKey('AllowTrailingCommas')
    }
    $enhancedJsonAvailable = $features.EnhancedTestJson.IgnoreComments -and $features.EnhancedTestJson.AllowTrailingCommas
    Write-Host "  $([char]$(if ($enhancedJsonAvailable) { 0x2705 } else { 0x274C })) Enhanced Test-Json (Comments & Trailing Commas)" -ForegroundColor $(if ($enhancedJsonAvailable) { 'Green' } else { 'Red' })

    # Enhanced ConvertFrom-Json
    $convertFromJsonCmd = Get-Command ConvertFrom-Json
    $features.EnhancedConvertFromJson = @{
        DateKind = $convertFromJsonCmd.Parameters.ContainsKey('DateKind')
    }
    Write-Host "  $([char]$(if ($features.EnhancedConvertFromJson.DateKind) { 0x2705 } else { 0x274C })) Enhanced ConvertFrom-Json (DateKind)" -ForegroundColor $(if ($features.EnhancedConvertFromJson.DateKind) { 'Green' } else { 'Red' })

    # Array performance optimization (conceptual test)
    $features.ArrayPerformanceOptimization = $features.IsPS75Plus
    Write-Host "  $([char]$(if ($features.ArrayPerformanceOptimization) { 0x2705 } else { 0x274C })) Array += Performance Optimization" -ForegroundColor $(if ($features.ArrayPerformanceOptimization) { 'Green' } else { 'Red' })

    if ($ShowBenchmarks -and $features.IsPS75Plus) {
        Write-Host ""
        Write-Host "Performance Benchmarks:" -ForegroundColor Yellow

        # Simple array performance test
        $arraySize = 1000
        Write-Host "  Testing array += operation with $arraySize elements..." -ForegroundColor Gray

        $arrayTime = Measure-Command {
            $testArray = @()
            for ($i = 0; $i -lt $arraySize; $i++) {
                $testArray += $i
            }
        }

        $listTime = Measure-Command {
            $testList = [System.Collections.Generic.List[int]]::new()
            for ($i = 0; $i -lt $arraySize; $i++) {
                $testList.Add($i)
            }
        }

        Write-Host "    Array += : $([math]::Round($arrayTime.TotalMilliseconds, 2))ms" -ForegroundColor Cyan
        Write-Host "    List.Add(): $([math]::Round($listTime.TotalMilliseconds, 2))ms" -ForegroundColor Cyan

        $features.PerformanceBenchmark = @{
            ArrayTime = $arrayTime.TotalMilliseconds
            ListTime = $listTime.TotalMilliseconds
            ArrayToListRatio = if ($listTime.TotalMilliseconds -gt 0) { $arrayTime.TotalMilliseconds / $listTime.TotalMilliseconds } else { 0 }
        }
    }

    # Test experimental features if available
    Write-Host ""
    Write-Host "Experimental Features (if enabled):" -ForegroundColor Magenta

    try {
        $experimentalFeatures = Get-ExperimentalFeature -ErrorAction SilentlyContinue
        if ($experimentalFeatures) {
            $relevantFeatures = $experimentalFeatures | Where-Object { $_.Name -like "*PSRedirectToVariable*" -or $_.Name -like "*PSNativeWindowsTildeExpansion*" -or $_.Name -like "*PSSerializeJSONLongEnumAsNumber*" }

            if ($relevantFeatures) {
                foreach ($feature in $relevantFeatures) {
                    $status = if ($feature.Enabled) { '✅ Enabled' } else { '⚠️ Available but not enabled' }
                    Write-Host "  $status $($feature.Name)" -ForegroundColor $(if ($feature.Enabled) { 'Green' } else { 'Yellow' })
                }
            } else {
                Write-Host "  ℹ️ No relevant experimental features found" -ForegroundColor Gray
            }
        } else {
            Write-Host "  ℹ️ Get-ExperimentalFeature not available" -ForegroundColor Gray
        }
    } catch {
        Write-Host "  ⚠️ Unable to check experimental features" -ForegroundColor Yellow
    }

    Write-Host ""

    # Overall compatibility score
    $compatibilityScore = 0
    $maxScore = 4

    if ($features.IsPS75Plus) { $compatibilityScore++ }
    if ($cliXmlAvailable) { $compatibilityScore++ }
    if ($enhancedJsonAvailable) { $compatibilityScore++ }
    if ($features.EnhancedConvertFromJson.DateKind) { $compatibilityScore++ }

    $compatibilityPercentage = [math]::Round(($compatibilityScore / $maxScore) * 100)

    Write-BusBuddyStatus "PowerShell 7.5 Compatibility: $compatibilityPercentage% ($compatibilityScore/$maxScore features)" -Status $(
        if ($compatibilityPercentage -ge 90) { 'Success' }
        elseif ($compatibilityPercentage -ge 70) { 'Warning' }
        else { 'Error' }
    )

    if ($compatibilityPercentage -lt 100) {
        Write-Host ""
        Write-Host "Recommendations:" -ForegroundColor Blue
        if (-not $features.IsPS75Plus) {
            Write-Host "  💡 Upgrade to PowerShell 7.5+ for optimal performance and features" -ForegroundColor Blue
        }
        if (-not $features.IsNet8Plus) {
            Write-Host "  💡 Upgrade to .NET 8+ for optimal BusBuddy development experience" -ForegroundColor Blue
        }
    }

    return $features
}

#endregion

#region Fun and Utility Functions

function Get-BusBuddyHappiness {
    <#
    .SYNOPSIS
        Provides sarcastic motivational quotes for developers

    .DESCRIPTION
        Displays randomly selected humorous and motivational quotes with a transportation theme
        to boost developer morale during long coding sessions

    .PARAMETER Count
        Number of quotes to display (default: 1)

    .PARAMETER All
        Display all available quotes

    .PARAMETER Theme
        Quote theme filter
    #>
    [CmdletBinding()]
    param(
        [int]$Count = 1,
        [switch]$All,

        [ValidateSet('General', 'Debugging', 'Building', 'Testing')]
        [string]$Theme = 'General'
    )

    $quotes = $script:BusBuddyModuleConfig.HappinessQuotes

    if ($All) {
        Write-Host ""
        Write-BusBuddyStatus "🚌 All Bus Buddy Happiness Quotes:" -Status Info
        for ($i = 0; $i -lt $quotes.Count; $i++) {
            Write-Host "  $($i + 1). $($quotes[$i])" -ForegroundColor Yellow
        }
        Write-Host ""
        return
    }

    Write-Host ""
    Write-BusBuddyStatus "🚌 Bus Buddy Developer Happiness Quote:" -Status Info

    for ($i = 0; $i -lt $Count; $i++) {
        $randomQuote = $quotes | Get-Random
        Write-Host "  💬 $randomQuote" -ForegroundColor Yellow
    }
    Write-Host ""
}

function Get-BusBuddyCommands {
    <#
    .SYNOPSIS
        Lists all available Bus Buddy PowerShell commands

    .DESCRIPTION
        Displays organized list of Bus Buddy module functions and aliases

    .PARAMETER Category
        Filter commands by category

    .PARAMETER ShowAliases
        Include command aliases in output
    #>
    [CmdletBinding()]
    param(
        [ValidateSet('All', 'Essential', 'Build', 'Development', 'Analysis', 'Fun')]
        [string]$Category = 'All',

        [switch]$ShowAliases
    )

    $commands = @{
        'Essential' = @(
            @{ Name = 'bb-build'; Description = 'Build the solution'; Function = 'Invoke-BusBuddyBuild' }
            @{ Name = 'bb-run'; Description = 'Run the application'; Function = 'Invoke-BusBuddyRun' }
            @{ Name = 'bb-test'; Description = 'Run tests'; Function = 'Invoke-BusBuddyTest' }
            @{ Name = 'bb-health'; Description = 'Health check'; Function = 'Invoke-BusBuddyHealthCheck' }
        )
        'Build' = @(
            @{ Name = 'bb-clean'; Description = 'Clean build artifacts'; Function = 'Invoke-BusBuddyClean' }
            @{ Name = 'bb-restore'; Description = 'Restore NuGet packages'; Function = 'Invoke-BusBuddyRestore' }
        )
        'Development' = @(
            @{ Name = 'bb-dev-session'; Description = 'Start development session'; Function = 'Start-BusBuddyDevSession' }
            @{ Name = 'bb-env-check'; Description = 'Check environment'; Function = 'Test-BusBuddyEnvironment' }
        )
        'Fun' = @(
            @{ Name = 'bb-happiness'; Description = 'Get motivational quotes'; Function = 'Get-BusBuddyHappiness' }
            @{ Name = 'bb-commands'; Description = 'List all commands'; Function = 'Get-BusBuddyCommands' }
        )
    }

    Write-Host ""
    Write-BusBuddyStatus "🚌 Bus Buddy PowerShell Commands" -Status Info

    if ($Category -eq 'All') {
        foreach ($cat in $commands.Keys | Sort-Object) {
            Write-Host ""
            Write-Host "📂 $cat Commands:" -ForegroundColor Magenta
            foreach ($cmd in $commands[$cat]) {
                $aliasInfo = if ($ShowAliases) { " (→ $($cmd.Function))" } else { "" }
                Write-Host "  $($cmd.Name)$aliasInfo" -ForegroundColor Green -NoNewline
                Write-Host " - $($cmd.Description)" -ForegroundColor Gray
            }
        }
    } else {
        if ($commands.ContainsKey($Category)) {
            Write-Host ""
            Write-Host "📂 $Category Commands:" -ForegroundColor Magenta
            foreach ($cmd in $commands[$Category]) {
                $aliasInfo = if ($ShowAliases) { " (→ $($cmd.Function))" } else { "" }
                Write-Host "  $($cmd.Name)$aliasInfo" -ForegroundColor Green -NoNewline
                Write-Host " - $($cmd.Description)" -ForegroundColor Gray
            }
        }
    }
    Write-Host ""
}

function Get-BusBuddyInfo {
    <#
    .SYNOPSIS
        Display Bus Buddy module information and status

    .DESCRIPTION
        Shows module version, loaded components, project status, and environment info
    #>
    [CmdletBinding()]
    param()

    $config = $script:BusBuddyModuleConfig
    $projectRoot = Get-BusBuddyProjectRoot

    Write-Host ""
    Write-Host "🚌 Bus Buddy PowerShell Module" -ForegroundColor Cyan
    Write-Host "════════════════════════════════════════" -ForegroundColor DarkGray
    Write-Host "Version: $($config.Version)" -ForegroundColor White
    Write-Host "Author: $($config.Author)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Environment:" -ForegroundColor Yellow
    Write-Host "  PowerShell: $($PSVersionTable.PSVersion)" -ForegroundColor Gray
    Write-Host "  Project Root: $(if ($projectRoot) { $projectRoot } else { 'Not Found' })" -ForegroundColor Gray
    Write-Host "  Module Location: $PSScriptRoot" -ForegroundColor Gray
    Write-Host ""

    if ($projectRoot) {
        $envValid = Test-BusBuddyEnvironment
        $status = if ($envValid) { '✅ Ready' } else { '⚠️ Issues Detected' }
        $color = if ($envValid) { 'Green' } else { 'Yellow' }
        Write-Host "Status: $status" -ForegroundColor $color
    } else {
        Write-Host "Status: ❌ Project Not Found" -ForegroundColor Red
    }

    Write-Host ""
    Write-Host "Quick Start:" -ForegroundColor Yellow
    Write-Host "  bb-dev-session    # Start development session" -ForegroundColor Green
    Write-Host "  bb-build          # Build solution" -ForegroundColor Green
    Write-Host "  bb-run            # Run application" -ForegroundColor Green
    Write-Host "  bb-happiness      # Get motivation 😊" -ForegroundColor Green
    Write-Host "  bb-commands       # Show all commands" -ForegroundColor Green
    Write-Host ""
}

#endregion

#region Package Management Functions

function Invoke-BusBuddyRestore {
    <#
    .SYNOPSIS
        Restore NuGet packages for Bus Buddy solution

    .DESCRIPTION
        Restores NuGet packages with enhanced options for force restore and cache management

    .PARAMETER Force
        Force package restore

    .PARAMETER NoCache
        Ignore NuGet cache

    .PARAMETER Sources
        Additional package sources
    #>
    [CmdletBinding()]
    param(
        [switch]$Force,
        [switch]$NoCache,
        [string[]]$Sources
    )

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        Write-BusBuddyStatus "Project root not found" -Status Error
        return $false
    }

    Push-Location $projectRoot

    try {
        Write-BusBuddyStatus "Restoring NuGet packages..." -Status Info

        $restoreArgs = @('restore', 'BusBuddy.sln', '--verbosity', 'minimal')

        if ($Force) {
            $restoreArgs += '--force'
            Write-BusBuddyStatus "Force restore enabled" -Status Warning
        }

        if ($NoCache) {
            $restoreArgs += '--no-cache'
            Write-BusBuddyStatus "Cache ignored" -Status Info
        }

        if ($Sources) {
            foreach ($source in $Sources) {
                $restoreArgs += '--source', $source
            }
            Write-BusBuddyStatus "Additional sources: $($Sources -join ', ')" -Status Info
        }

        & dotnet @restoreArgs

        if ($LASTEXITCODE -eq 0) {
            Write-BusBuddyStatus "Package restore completed successfully" -Status Success
            return $true
        } else {
            Write-BusBuddyStatus "Package restore failed with exit code $LASTEXITCODE" -Status Error
            return $false
        }
    }
    catch {
        Write-BusBuddyStatus "Restore process error: $($_.Exception.Message)" -Status Error
        return $false
    }
    finally {
        Pop-Location
    }
}

#endregion

#region Module Aliases and Exports

# Core development aliases
Set-Alias -Name 'bb-build' -Value 'Invoke-BusBuddyBuild' -Description 'Build Bus Buddy solution'
Set-Alias -Name 'bb-run' -Value 'Invoke-BusBuddyRun' -Description 'Run Bus Buddy application'
Set-Alias -Name 'bb-test' -Value 'Invoke-BusBuddyTest' -Description 'Run Bus Buddy tests'
Set-Alias -Name 'bb-clean' -Value 'Invoke-BusBuddyClean' -Description 'Clean build artifacts'
Set-Alias -Name 'bb-restore' -Value 'Invoke-BusBuddyRestore' -Description 'Restore NuGet packages'

# Advanced development aliases
Set-Alias -Name 'bb-dev-session' -Value 'Start-BusBuddyDevSession' -Description 'Start development session'
Set-Alias -Name 'bb-health' -Value 'Invoke-BusBuddyHealthCheck' -Description 'Project health check'
Set-Alias -Name 'bb-env-check' -Value 'Test-BusBuddyEnvironment' -Description 'Environment validation'
Set-Alias -Name 'bb-validate' -Value 'Test-BusBuddyEnvironment' -Description 'Environment validation (alias)'

# Utility aliases
Set-Alias -Name 'bb-happiness' -Value 'Get-BusBuddyHappiness' -Description 'Motivational quotes'
Set-Alias -Name 'bb-commands' -Value 'Get-BusBuddyCommands' -Description 'List all commands'
Set-Alias -Name 'bb-info' -Value 'Get-BusBuddyInfo' -Description 'Module information'

#endregion

#region Module Initialization

# Display welcome message when module loads
$welcomeMessage = @"

🚌 Bus Buddy PowerShell Module Loaded Successfully!
═══════════════════════════════════════════════════

Quick Commands:
• bb-dev-session  → Start complete development session
• bb-build        → Build the solution
• bb-run          → Run the application
• bb-happiness    → Get developer motivation 😊
• bb-commands     → Show all available commands

Project Status: $(if (Get-BusBuddyProjectRoot) { '✅ Ready' } else { '⚠️ Project not detected' })

"@

Write-Host $welcomeMessage -ForegroundColor Cyan

# Auto-detect and display environment status
if (Get-BusBuddyProjectRoot) {
    $envStatus = Test-BusBuddyEnvironment
    if (-not $envStatus) {
        Write-BusBuddyStatus "Environment validation found issues. Run 'bb-env-check' for details." -Status Warning
    }
}

#endregion

#region AI Mentor System Functions

function Get-BusBuddyMentor {
    <#
    .SYNOPSIS
        AI-powered learning companion for BusBuddy development

    .DESCRIPTION
        Provides contextual learning assistance, documentation links, and interactive tutorials
        for mastering BusBuddy technologies including PowerShell, WPF, Entity Framework, and Azure.

    .PARAMETER Topic
        Technology or concept to learn about (PowerShell, WPF, EntityFramework, Azure, MVVM, etc.)

    .PARAMETER IncludeExamples
        Include practical code examples in the response

    .PARAMETER OpenDocs
        Open official documentation in the default browser

    .PARAMETER BeginnerMode
        Provide simplified explanations suitable for newcomers

    .PARAMETER AdvancedMode
        Show advanced techniques and deep technical details

    .PARAMETER Interactive
        Start an interactive learning session

    .EXAMPLE
        Get-BusBuddyMentor -Topic "PowerShell"

    .EXAMPLE
        Get-BusBuddyMentor -Topic "WPF" -IncludeExamples -OpenDocs

    .EXAMPLE
        Get-BusBuddyMentor -Topic "Entity Framework" -BeginnerMode
    #>
    [CmdletBinding()]
    param(
        [Parameter(Position = 0)]
        [ValidateSet('PowerShell', 'WPF', 'EntityFramework', 'Azure', 'MVVM', 'Debugging', 'Testing', 'Git',
                     'Architecture', 'DataModels', 'Services', 'UIPatterns', 'Syncfusion', 'Getting Started')]
        [string]$Topic = 'Getting Started',

        [switch]$IncludeExamples,
        [switch]$OpenDocs,
        [switch]$BeginnerMode,
        [switch]$AdvancedMode,
        [switch]$Interactive,
        [switch]$Motivate,
        [switch]$Humor
    )

    if ($Motivate) {
        $quotes = @(
            "🚀 Remember: Every expert was once a beginner. Every pro was once an amateur!",
            "🎯 You're not just learning to code, you're learning to solve problems!",
            "✨ Great developers aren't born, they're compiled through experience!",
            "🌟 Each bug you fix makes you stronger. Each feature you build makes you wiser!",
            "🏆 The best time to plant a tree was 20 years ago. The second best time is now. Same with learning!"
        )
        Write-Host "`n$(Get-Random -InputObject $quotes)" -ForegroundColor Green
        return
    }

    if ($Humor) {
        Get-BusBuddyHappiness
        return
    }

    $mentorContent = @{
        'PowerShell' = @{
            'Summary' = 'PowerShell is a powerful shell and scripting language for automation'
            'KeyConcepts' = @('Variables', 'Functions', 'Modules', 'Pipeline', 'Objects')
            'OfficialDocs' = 'https://learn.microsoft.com/en-us/powershell/'
            'Examples' = @(
                '# Variables and basic operations',
                '$name = "BusBuddy"',
                '$version = Get-Content ".\version.txt"',
                '',
                '# Functions with parameters',
                'function Deploy-BusBuddy {',
                '    param([string]$Environment)',
                '    Write-Host "Deploying to $Environment"',
                '}',
                '',
                '# Pipeline operations',
                'Get-ChildItem *.cs | Where-Object {$_.Length -gt 1KB} | Sort-Object Name'
            )
            'BusBuddySpecific' = @(
                'Our PowerShell module provides automation for:',
                '• Building and running the application (bb-build, bb-run)',
                '• Environment validation (bb-health)',
                '• Development session management (bb-dev-session)',
                '• Configuration testing and validation'
            )
        }

        'WPF' = @{
            'Summary' = 'Windows Presentation Foundation for rich desktop UI development'
            'KeyConcepts' = @('XAML', 'Data Binding', 'Controls', 'Styles', 'Resources')
            'OfficialDocs' = 'https://learn.microsoft.com/en-us/dotnet/desktop/wpf/'
            'Examples' = @(
                '<!-- Basic XAML structure -->',
                '<Window x:Class="BusBuddy.MainWindow">',
                '    <Grid>',
                '        <TextBlock Text="{Binding Title}" />',
                '        <Button Content="Click Me" Command="{Binding ClickCommand}" />',
                '    </Grid>',
                '</Window>',
                '',
                '// C# Code-behind with data binding',
                'public partial class MainWindow : Window',
                '{',
                '    public MainWindow()',
                '    {',
                '        InitializeComponent();',
                '        DataContext = new MainViewModel();',
                '    }',
                '}'
            )
            'BusBuddySpecific' = @(
                'BusBuddy WPF implementation features:',
                '• Syncfusion controls for rich UI (DataGrid, Charts, Navigation)',
                '• MVVM pattern with RelayCommand implementation',
                '• FluentDark theme for modern appearance',
                '• Responsive layout for different screen sizes'
            )
        }

        'Getting Started' = @{
            'Summary' = 'Welcome to BusBuddy development! Here is your roadmap.'
            'KeyConcepts' = @('Setup', 'Build', 'Run', 'Debug', 'Learn')
            'OfficialDocs' = 'https://github.com/Bigessfour/BusBuddy-2'
            'Examples' = @(
                '# Quick start commands',
                'bb-build          # Build the solution',
                'bb-run            # Run the application',
                'bb-health         # Check environment',
                '',
                '# Learning commands',
                'bb-mentor PowerShell    # Learn PowerShell',
                'bb-mentor WPF          # Learn WPF development',
                'bb-docs -Technology "EntityFramework"',
                '',
                '# Development workflow',
                'bb-dev-session    # Start full development session'
            )
            'BusBuddySpecific' = @(
                'Your BusBuddy journey starts here:',
                '1. Environment Setup: Ensure PowerShell 7.5+ and .NET 9+',
                '2. Build & Run: Use bb-build and bb-run commands',
                '3. Explore Code: Start with BusBuddy.WPF/Views/MainWindow.xaml',
                '4. Learn Technologies: Use the mentor system for guidance',
                '5. Contribute: Check Docs/Contributing.md for guidelines'
            )
        }
    }

    $content = $mentorContent[$Topic]
    if (-not $content) {
        Write-BusBuddyError -Message "Unknown topic: $Topic" -RecommendedAction "Use tab completion or run 'bb-mentor' to see available topics"
        return
    }

    Write-Host "`n🤖 BusBuddy Mentor: $Topic" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════" -ForegroundColor DarkCyan

    Write-Host "`n📋 Overview:" -ForegroundColor Yellow
    Write-Host $content.Summary -ForegroundColor White

    Write-Host "`n🎯 Key Concepts:" -ForegroundColor Yellow
    $content.KeyConcepts | ForEach-Object { Write-Host "  • $_" -ForegroundColor Green }

    if ($content.BusBuddySpecific) {
        Write-Host "`n🚌 BusBuddy Integration:" -ForegroundColor Yellow
        $content.BusBuddySpecific | ForEach-Object { Write-Host $_ -ForegroundColor Cyan }
    }

    if ($IncludeExamples -and $content.Examples) {
        Write-Host "`n💻 Code Examples:" -ForegroundColor Yellow
        $content.Examples | ForEach-Object { Write-Host $_ -ForegroundColor Gray }
    }

    if ($BeginnerMode) {
        Write-Host "`n🌱 Beginner Tips:" -ForegroundColor Yellow
        $beginnerTips = @{
            'PowerShell' = "Start with basic commands like Get-Help, Get-Command, and Get-Member. Use tab completion!"
            'WPF' = "Begin with simple XAML layouts and data binding. Visual Studio designer can help!"
            'Getting Started' = "Take it one step at a time. Build first, then run, then explore the code!"
        }
        if ($beginnerTips[$Topic]) {
            Write-Host $beginnerTips[$Topic] -ForegroundColor Magenta
        }
    }

    Write-Host "`n📚 Official Documentation:" -ForegroundColor Yellow
    Write-Host $content.OfficialDocs -ForegroundColor Blue

    if ($OpenDocs) {
        Start-Process $content.OfficialDocs
        Write-Host "`n🌐 Opening documentation in browser..." -ForegroundColor Green
    }

    Write-Host "`n💡 Next Steps:" -ForegroundColor Yellow
    Write-Host "  • Use 'bb-mentor $Topic -IncludeExamples' for code examples" -ForegroundColor White
    Write-Host "  • Use 'bb-docs -Technology $Topic' to search official docs" -ForegroundColor White
    Write-Host "  • Use 'bb-ref $Topic' for quick reference" -ForegroundColor White
}

function Search-OfficialDocs {
    <#
    .SYNOPSIS
        Search official Microsoft documentation for specific topics

    .DESCRIPTION
        Provides direct search functionality for official documentation with smart URL construction
        and optional browser integration.

    .PARAMETER Technology
        The technology to search documentation for

    .PARAMETER Query
        The search query or topic

    .PARAMETER OpenInBrowser
        Open the search results in the default browser

    .PARAMETER LinksOnly
        Return only the documentation links without opening browser

    .EXAMPLE
        Search-OfficialDocs -Technology "PowerShell" -Query "modules"

    .EXAMPLE
        Search-OfficialDocs -Technology "WPF" -Query "data binding" -OpenInBrowser
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [ValidateSet('PowerShell', 'WPF', 'EntityFramework', 'Azure', 'DotNet')]
        [string]$Technology,

        [Parameter(Mandatory)]
        [string]$Query,

        [switch]$OpenInBrowser,
        [switch]$LinksOnly
    )

    $docUrls = @{
        'PowerShell' = 'https://learn.microsoft.com/en-us/powershell/'
        'WPF' = 'https://learn.microsoft.com/en-us/dotnet/desktop/wpf/'
        'EntityFramework' = 'https://learn.microsoft.com/en-us/ef/core/'
        'Azure' = 'https://learn.microsoft.com/en-us/azure/'
        'DotNet' = 'https://learn.microsoft.com/en-us/dotnet/'
    }

    $searchUrl = "https://learn.microsoft.com/en-us/search/?terms=$([uri]::EscapeDataString($Query))&category=$Technology"
    $directUrl = $docUrls[$Technology]

    if ($LinksOnly) {
        Write-Host "🔗 Documentation Links for $Technology - '$Query':" -ForegroundColor Cyan
        Write-Host "Direct: $directUrl" -ForegroundColor Blue
        Write-Host "Search: $searchUrl" -ForegroundColor Blue
        return
    }

    Write-Host "🔍 Searching $Technology documentation for: '$Query'" -ForegroundColor Cyan
    Write-Host "📚 Direct documentation: $directUrl" -ForegroundColor Blue
    Write-Host "🔎 Search results: $searchUrl" -ForegroundColor Blue

    if ($OpenInBrowser) {
        Start-Process $searchUrl
        Write-Host "`n🌐 Opening search results in browser..." -ForegroundColor Green
    }
}

function Get-QuickReference {
    <#
    .SYNOPSIS
        Get quick reference sheets for technologies and commands

    .DESCRIPTION
        Provides instant access to cheat sheets, syntax references, and common patterns
        for various technologies used in BusBuddy development.

    .PARAMETER Technology
        The technology to show reference for

    .PARAMETER Type
        The type of reference (Syntax, Commands, Patterns, etc.)

    .EXAMPLE
        Get-QuickReference -Technology "PowerShell"

    .EXAMPLE
        Get-QuickReference -Technology "BusBuddy"
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [ValidateSet('PowerShell', 'WPF', 'BusBuddy')]
        [string]$Technology,

        [ValidateSet('Syntax', 'Commands', 'All')]
        [string]$Type = 'All'
    )

    $references = @{
        'PowerShell' = @{
            'Commands' = @(
                '# Essential PowerShell Commands',
                'Get-Help <command>      # Get help for any command',
                'Get-Command *<term>*    # Find commands containing term',
                'Get-Member              # Show object properties/methods',
                '$_.PropertyName         # Access pipeline object properties',
                'Where-Object {condition}# Filter objects',
                'ForEach-Object {action} # Perform action on each object'
            )
            'Syntax' = @(
                '# PowerShell Syntax Reference',
                '$variable = "value"     # Variable assignment',
                'function Name { }       # Function declaration',
                'param([type]$name)      # Parameter declaration',
                'if ($condition) { }     # Conditional statement',
                'try { } catch { }       # Error handling'
            )
        }
        'BusBuddy' = @{
            'Commands' = @(
                '# BusBuddy PowerShell Commands',
                'bb-build               # Build the solution',
                'bb-run                 # Run the application',
                'bb-test                # Run tests',
                'bb-health              # Check environment health',
                'bb-mentor <topic>      # Get learning assistance',
                'bb-docs <technology>   # Search documentation',
                'bb-happiness           # Get motivational quotes'
            )
        }
    }

    $techRef = $references[$Technology]
    if (-not $techRef) {
        Write-BusBuddyError -Message "No quick reference available for $Technology" -RecommendedAction "Check available technologies with tab completion"
        return
    }

    Write-Host "⚡ $Technology Quick Reference" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════" -ForegroundColor DarkCyan

    if ($Type -eq 'All') {
        foreach ($refType in $techRef.Keys) {
            Write-Host "`n📋 ${refType}:" -ForegroundColor Yellow
            $techRef[$refType] | ForEach-Object { Write-Host $_ -ForegroundColor Gray }
        }
    }
    else {
        if ($techRef.ContainsKey($Type)) {
            Write-Host "`n📋 ${Type}:" -ForegroundColor Yellow
            $techRef[$Type] | ForEach-Object { Write-Host $_ -ForegroundColor Gray }
        }
        else {
            Write-BusBuddyError -Message "Reference type '$Type' not available for $Technology" -RecommendedAction "Use -Type 'All' to see available types"
        }
    }
}

# Create aliases for the mentor system
New-Alias -Name "bb-mentor" -Value "Get-BusBuddyMentor" -Description "AI learning mentor"
New-Alias -Name "bb-docs" -Value "Search-OfficialDocs" -Description "Search official documentation"
New-Alias -Name "bb-ref" -Value "Get-QuickReference" -Description "Quick reference sheets"

#endregion

#region Database Diagnostic Functions

function Invoke-BusBuddyDatabaseDiagnostic {
    <#
    .SYNOPSIS
        Runs comprehensive database diagnostics for BusBuddy

    .DESCRIPTION
        Executes the database diagnostics script with full reporting

    .PARAMETER TestConnection
        Test database connectivity only

    .PARAMETER CheckMigrations
        Check migration status only

    .EXAMPLE
        Invoke-BusBuddyDatabaseDiagnostic
        bb-db-diag
    #>
    [CmdletBinding()]
    param(
        [switch]$TestConnection,
        [switch]$CheckMigrations
    )

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        Write-BusBuddyError "Could not locate BusBuddy project root"
        return
    }

    $scriptPath = Join-Path $projectRoot "Scripts\database-diagnostics.ps1"
    if (-not (Test-Path $scriptPath)) {
        Write-BusBuddyError "Database diagnostics script not found at: $scriptPath"
        return
    }

    Write-BusBuddyStatus "Running database diagnostics..."

    try {
        Push-Location $projectRoot

        if ($TestConnection) {
            & $scriptPath -TestConnection
        } elseif ($CheckMigrations) {
            & $scriptPath -CheckMigrations
        } else {
            & $scriptPath -FullDiagnostic
        }

        if ($LASTEXITCODE -eq 0) {
            Write-BusBuddyStatus "Database diagnostics completed successfully"
        } else {
            Write-BusBuddyError "Database diagnostics completed with issues"
        }
    }
    catch {
        Write-BusBuddyError "Database diagnostics failed: $_"
    }
    finally {
        Pop-Location
    }
}

function Add-BusBuddyMigration {
    <#
    .SYNOPSIS
        Adds a new Entity Framework migration

    .DESCRIPTION
        Creates a new migration with the specified name using EF Core tools

    .PARAMETER Name
        Name of the migration to create

    .EXAMPLE
        Add-BusBuddyMigration -Name "InitialCreate"
        bb-db-add-migration "AddNewFeature"
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        Write-BusBuddyError "Could not locate BusBuddy project root"
        return
    }

    $scriptPath = Join-Path $projectRoot "Scripts\database-diagnostics.ps1"
    if (-not (Test-Path $scriptPath)) {
        Write-BusBuddyError "Database diagnostics script not found at: $scriptPath"
        return
    }

    Write-BusBuddyStatus "Adding migration: $Name"

    try {
        Push-Location $projectRoot
        & $scriptPath -AddMigration -MigrationName $Name

        if ($LASTEXITCODE -eq 0) {
            Write-BusBuddyStatus "Migration '$Name' added successfully"
        } else {
            Write-BusBuddyError "Migration add failed"
        }
    }
    catch {
        Write-BusBuddyError "Migration add failed: $_"
    }
    finally {
        Pop-Location
    }
}

function Update-BusBuddyDatabase {
    <#
    .SYNOPSIS
        Updates the database schema with pending migrations

    .DESCRIPTION
        Applies all pending migrations to update the database schema

    .EXAMPLE
        Update-BusBuddyDatabase
        bb-db-update
    #>
    [CmdletBinding()]
    param()

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        Write-BusBuddyError "Could not locate BusBuddy project root"
        return
    }

    $scriptPath = Join-Path $projectRoot "Scripts\database-diagnostics.ps1"
    if (-not (Test-Path $scriptPath)) {
        Write-BusBuddyError "Database diagnostics script not found at: $scriptPath"
        return
    }

    Write-BusBuddyStatus "Updating database schema..."

    try {
        Push-Location $projectRoot
        & $scriptPath -UpdateDatabase

        if ($LASTEXITCODE -eq 0) {
            Write-BusBuddyStatus "Database updated successfully"
        } else {
            Write-BusBuddyError "Database update failed"
        }
    }
    catch {
        Write-BusBuddyError "Database update failed: $_"
    }
    finally {
        Pop-Location
    }
}

function Test-BusBuddyDatabaseConnection {
    <#
    .SYNOPSIS
        Tests database connectivity

    .DESCRIPTION
        Performs a quick connectivity test to the BusBuddy database

    .EXAMPLE
        Test-BusBuddyDatabaseConnection
        bb-db-test
    #>
    [CmdletBinding()]
    param()

    Invoke-BusBuddyDatabaseDiagnostic -TestConnection
}

function Import-BusBuddyRealWorldData {
    <#
    .SYNOPSIS
        Imports real-world sample data into the database

    .DESCRIPTION
        Seeds the database with realistic sample data for development and testing

    .EXAMPLE
        Import-BusBuddyRealWorldData
        bb-db-seed
    #>
    [CmdletBinding()]
    param()

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        Write-BusBuddyError "Could not locate BusBuddy project root"
        return
    }

    $scriptPath = Join-Path $projectRoot "Scripts\database-diagnostics.ps1"
    if (-not (Test-Path $scriptPath)) {
        Write-BusBuddyError "Database diagnostics script not found at: $scriptPath"
        return
    }

    Write-BusBuddyStatus "Importing real-world sample data..."

    try {
        Push-Location $projectRoot
        & $scriptPath -SeedData

        if ($LASTEXITCODE -eq 0) {
            Write-BusBuddyStatus "Sample data imported successfully"
        } else {
            Write-BusBuddyError "Sample data import failed"
        }
    }
    catch {
        Write-BusBuddyError "Sample data import failed: $_"
    }
    finally {
        Pop-Location
    }
}

# Database diagnostic aliases
New-Alias -Name "bb-db-diag" -Value "Invoke-BusBuddyDatabaseDiagnostic" -Description "Full database diagnostics"
New-Alias -Name "bb-db-test" -Value "Test-BusBuddyDatabaseConnection" -Description "Test database connection"
New-Alias -Name "bb-db-add-migration" -Value "Add-BusBuddyMigration" -Description "Add EF Core migration"
New-Alias -Name "bb-db-update" -Value "Update-BusBuddyDatabase" -Description "Update database schema"
New-Alias -Name "bb-db-seed" -Value "Import-BusBuddyRealWorldData" -Description "Import sample data"

#endregion

#region Export Definitions

# Export all functions that should be available publicly
Export-ModuleMember -Function @(
    # Core utility functions
    'Get-BusBuddyProjectRoot',
    'Write-BusBuddyStatus',
    'Write-BusBuddyError',
    'Test-BusBuddyConfiguration',
    'Test-BusBuddyEnvironment',

    # PowerShell 7.5 enhancements
    'Test-PowerShell75Features',

    # Build and development functions
    'Invoke-BusBuddyBuild',
    'Invoke-BusBuddyRun',
    'Invoke-BusBuddyTest',
    'Invoke-BusBuddyClean',
    'Invoke-BusBuddyRestore',

    # Advanced development functions
    'Start-BusBuddyDevSession',
    'Invoke-BusBuddyHealthCheck',

    # Fun and utility functions
    'Get-BusBuddyHappiness',
    'Get-BusBuddyCommands',
    'Get-BusBuddyInfo',

    # AI Mentor System functions
    'Get-BusBuddyMentor',
    'Search-OfficialDocs',
    'Get-QuickReference',

    # Database diagnostic functions
    'Invoke-BusBuddyDatabaseDiagnostic',
    'Add-BusBuddyMigration',
    'Update-BusBuddyDatabase',
    'Test-BusBuddyDatabaseConnection',
    'Import-BusBuddyRealWorldData'
)

# Export all aliases
Export-ModuleMember -Alias @(
    'bb-build', 'bb-run', 'bb-test', 'bb-clean', 'bb-restore',
    'bb-dev-session', 'bb-health', 'bb-env-check',
    'bb-happiness', 'bb-commands', 'bb-info',
    'bb-mentor', 'bb-docs', 'bb-ref',
    'bb-db-diag', 'bb-db-test', 'bb-db-add-migration', 'bb-db-update', 'bb-db-seed'
)

#endregion
