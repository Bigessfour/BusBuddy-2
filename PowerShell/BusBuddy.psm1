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
    PS Version     : Optimized for PowerShell 7.5 with .NET 9 features

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
    Name             = 'BusBuddy'
    Version          = '1.0.0'
    Author           = 'Bus Buddy Development Team'
    ProjectRoot      = $null
    LoadedComponents = @()
    HappinessQuotes  = @(
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
        'Error'   = '❌'
        'Info'    = '🚌'
        'Build'   = '🔨'
        'Test'    = '🧪'
    }

    $colors = @{
        'Success' = 'Green'
        'Warning' = 'Yellow'
        'Error'   = 'Red'
        'Info'    = 'Cyan'
        'Build'   = 'Magenta'
        'Test'    = 'Blue'
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
        }
        else {
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
        - .NET 9+ runtime validation
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
    }
    elseif ($psVersion -lt [version]'7.5.0') {
        $recommendations += "Consider upgrading to PowerShell 7.5 for enhanced performance and features"
        Write-BusBuddyStatus "PowerShell $psVersion detected (7.5+ recommended for optimal performance)" -Status Warning
    }
    else {
        Write-BusBuddyStatus "PowerShell $psVersion detected - full PowerShell 7.5 features available" -Status Success
    }

    # Check for PowerShell 7.5 specific features
    if ($Detailed -and $psVersion -ge [version]'7.5.0') {
        Write-BusBuddyStatus "Checking PowerShell 7.5 features..." -Status Info

        $ps75Features = @{
            'Enhanced ConvertTo-CliXml'            = Get-Command ConvertTo-CliXml -ErrorAction SilentlyContinue
            'Enhanced Test-Json (IgnoreComments)'  = (Get-Command Test-Json).Parameters.ContainsKey('IgnoreComments')
            'Enhanced ConvertFrom-Json (DateKind)' = (Get-Command ConvertFrom-Json).Parameters.ContainsKey('DateKind')
            'Array += Performance Optimization'    = $psVersion -ge [version]'7.5.0'
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
        }
        else {
            $dotnetVer = [version]$dotnetVersion
            if ($dotnetVer -lt [version]"8.0") {
                $issues += ".NET 8.0+ required (found $dotnetVersion)"
            }
            elseif ($dotnetVer -lt [version]"9.0") {
                $recommendations += "Consider upgrading to .NET 9 for PowerShell 7.5 optimal performance"
                Write-BusBuddyStatus ".NET $dotnetVersion detected (.NET 9+ recommended)" -Status Warning
            }
            else {
                Write-BusBuddyStatus ".NET $dotnetVersion detected - optimal for PowerShell 7.5" -Status Success
            }
        }
    }
    catch {
        $issues += ".NET SDK not accessible"
    }

    # Project structure validation
    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        $issues += "Bus Buddy project root not found"
    }
    else {
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
    }
    else {
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
        Write-BusBuddyStatus "🔨 Building BusBuddy solution..." -Status Build

        # Clean if requested
        if ($Clean) {
            Write-Host "🧹 Cleaning previous build artifacts..." -ForegroundColor Yellow
            dotnet clean BusBuddy.sln --configuration $Configuration --verbosity quiet | Out-Null
            if ($LASTEXITCODE -ne 0) {
                Write-BusBuddyStatus "Clean operation failed" -Status Error
                return $false
            }
        }

        # Restore if requested
        if ($Restore) {
            Write-Host "📦 Restoring NuGet packages..." -ForegroundColor Yellow
            dotnet restore BusBuddy.sln --verbosity quiet | Out-Null
            if ($LASTEXITCODE -ne 0) {
                Write-BusBuddyStatus "Package restore failed" -Status Error
                return $false
            }
        }

        # Build solution with enhanced output analysis
        $buildArgs = @('build', 'BusBuddy.sln', '--configuration', $Configuration, '--verbosity', $Verbosity)
        if ($NoLogo) { $buildArgs += '--nologo' }

        Write-Host "🔨 Building solution with configuration: $Configuration" -ForegroundColor Green
        $buildOutput = & dotnet @buildArgs 2>&1

        if ($LASTEXITCODE -eq 0) {
            # Analyze build output for warnings and success metrics
            $warnings = $buildOutput | Select-String -Pattern "warning" -AllMatches
            $warningCount = ($warnings | Measure-Object).Count

            Write-Host "✅ Build completed successfully!" -ForegroundColor Green
            Write-Host "📊 Build Summary:" -ForegroundColor Cyan
            Write-Host "   • Configuration: $Configuration" -ForegroundColor Gray
            Write-Host "   • Warnings: $warningCount" -ForegroundColor $(if ($warningCount -eq 0) { "Green" } else { "Yellow" })

            if ($warningCount -gt 0 -and $Verbosity -ne 'quiet') {
                Write-Host "⚠️  Build warnings detected:" -ForegroundColor Yellow
                $warnings | Select-Object -First 5 | ForEach-Object { Write-Host "   $($_.Line)" -ForegroundColor DarkYellow }
                if ($warningCount -gt 5) {
                    Write-Host "   ... and $($warningCount - 5) more warnings" -ForegroundColor DarkYellow
                }
            }

            Write-BusBuddyStatus "Build completed successfully" -Status Success
            return $true
        }
        else {
            Write-BusBuddyStatus "Build failed with exit code $LASTEXITCODE" -Status Error
            if ($Verbosity -ne 'quiet') {
                $buildOutput | ForEach-Object { Write-Host $_ -ForegroundColor Red }
            }
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
        Write-BusBuddyStatus "🚌 Starting BusBuddy application..." -Status Info

        # Verify WPF project exists
        $wpfProject = "BusBuddy.WPF\BusBuddy.WPF.csproj"
        if (-not (Test-Path $wpfProject)) {
            Write-BusBuddyStatus "WPF project not found: $wpfProject" -Status Error
            return $false
        }

        # Build first unless skipped
        if (-not $NoBuild) {
            Write-Host "🔨 Building before launch..." -ForegroundColor Yellow
            $buildSuccess = Invoke-BusBuddyBuild -Configuration $Configuration -Verbosity quiet
            if (-not $buildSuccess) {
                Write-BusBuddyStatus "Build failed, cannot run application" -Status Error
                return $false
            }
        }

        # Prepare run arguments
        $runArgs = @('run', '--project', $wpfProject, '--configuration', $Configuration)

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
        }
        else {
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
        }
        else {
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
        'BusBuddy.sln'                       = 'Solution file'
        'BusBuddy.Core\BusBuddy.Core.csproj' = 'Core project'
        'BusBuddy.WPF\BusBuddy.WPF.csproj'   = 'WPF project'
        'README.md'                          = 'Documentation'
        'global.json'                        = 'SDK version file'
    }

    $structureScore = 0
    foreach ($file in $requiredStructure.Keys) {
        if (Test-Path (Join-Path $projectRoot $file)) {
            $structureScore += 4
        }
        else {
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
                }
                catch {
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
        }
        else {
            $issues += "Solution does not build successfully"
            Write-BusBuddyStatus "Build validation: FAILED" -Status Error
        }
    }
    catch {
        $issues += "Build validation error: $($_.Exception.Message)"
    }
    finally {
        Pop-Location
    }

    # PowerShell module validation
    Write-BusBuddyStatus "Checking PowerShell module..." -Status Info
    $maxScore += 10

    if (Test-Path (Join-Path $projectRoot "PowerShell\BusBuddy.psm1")) {
        $healthScore += 10
        Write-BusBuddyStatus "PowerShell module: FOUND" -Status Success
    }
    else {
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
    $features.IsNet9Plus = $features.DotNetVersion -ge [version]'9.0'

    Write-Host "PowerShell Version: $($features.PowerShellVersion)" -ForegroundColor $(if ($features.IsPS75Plus) { 'Green' } else { 'Yellow' })
    Write-Host ".NET Version: $($features.DotNetVersion)" -ForegroundColor $(if ($features.IsNet9Plus) { 'Green' } else { 'Yellow' })
    Write-Host ""

    # Test specific PS 7.5 features
    Write-Host "PowerShell 7.5 Feature Availability:" -ForegroundColor Cyan

    # ConvertTo-CliXml and ConvertFrom-CliXml
    $features.CliXmlCmdlets = @{
        ConvertTo   = Get-Command ConvertTo-CliXml -ErrorAction SilentlyContinue
        ConvertFrom = Get-Command ConvertFrom-CliXml -ErrorAction SilentlyContinue
    }
    $cliXmlAvailable = $features.CliXmlCmdlets.ConvertTo -and $features.CliXmlCmdlets.ConvertFrom
    Write-Host "  $([char]$(if ($cliXmlAvailable) { 0x2705 } else { 0x274C })) CLI XML Cmdlets (ConvertTo/From-CliXml)" -ForegroundColor $(if ($cliXmlAvailable) { 'Green' } else { 'Red' })

    # Enhanced Test-Json
    $testJsonCmd = Get-Command Test-Json
    $features.EnhancedTestJson = @{
        IgnoreComments      = $testJsonCmd.Parameters.ContainsKey('IgnoreComments')
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
            ArrayTime        = $arrayTime.TotalMilliseconds
            ListTime         = $listTime.TotalMilliseconds
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
            }
            else {
                Write-Host "  ℹ️ No relevant experimental features found" -ForegroundColor Gray
            }
        }
        else {
            Write-Host "  ℹ️ Get-ExperimentalFeature not available" -ForegroundColor Gray
        }
    }
    catch {
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
        if (-not $features.IsNet9Plus) {
            Write-Host "  💡 Upgrade to .NET 9+ for best PowerShell 7.5 experience" -ForegroundColor Blue
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

    # Add theme-specific quotes
    $themeQuotes = switch ($Theme) {
        'Debugging' {
            @(
                "🐛 Debugging: It's like being a detective in a crime novel where you're also the murderer.",
                "🔍 Remember: There are only two types of code - code that works and code that doesn't work yet.",
                "🎯 Every bug is just an undocumented feature waiting to be discovered!"
            )
        }
        'Building' {
            @(
                "🔨 A successful build is like a perfectly timed bus route - everything just clicks!",
                "⚙️ Compilation errors are just the computer's way of asking for clarification.",
                "🚀 Clean builds are like empty buses - rare but beautiful when they happen!"
            )
        }
        'Testing' {
            @(
                "🧪 Writing tests is like checking if the bus actually goes where the sign says it does.",
                "✅ Green tests are the developer's equivalent of hitting all green lights.",
                "🎪 Testing in production is like using your customers as crash test dummies!"
            )
        }
        default { $quotes }
    }

    if ($All) {
        Write-Host ""
        Write-Host "🚌 All Bus Buddy Happiness Quotes ($Theme theme):" -ForegroundColor Cyan
        for ($i = 0; $i -lt $themeQuotes.Count; $i++) {
            Write-Host "  $($i + 1). $($themeQuotes[$i])" -ForegroundColor Yellow
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
        [ValidateSet('All', 'Essential', 'Build', 'Development', 'Analysis', 'GitHub', 'Fun')]
        [string]$Category = 'All',

        [switch]$ShowAliases
    )

    $commands = @{
        'Essential'   = @(
            @{ Name = 'bb-build'; Description = 'Build the solution'; Function = 'Invoke-BusBuddyBuild' }
            @{ Name = 'bb-run'; Description = 'Run the application'; Function = 'Invoke-BusBuddyRun' }
            @{ Name = 'bb-test'; Description = 'Run tests'; Function = 'Invoke-BusBuddyTest' }
            @{ Name = 'bb-health'; Description = 'Health check'; Function = 'Invoke-BusBuddyHealthCheck' }
        )
        'Build'       = @(
            @{ Name = 'bb-clean'; Description = 'Clean build artifacts'; Function = 'Invoke-BusBuddyClean' }
            @{ Name = 'bb-restore'; Description = 'Restore NuGet packages'; Function = 'Invoke-BusBuddyRestore' }
        )
        'Development' = @(
            @{ Name = 'bb-dev-session'; Description = 'Start development session'; Function = 'Start-BusBuddyDevSession' }
            @{ Name = 'bb-env-check'; Description = 'Check environment'; Function = 'Test-BusBuddyEnvironment' }
            @{ Name = 'bb-dev-workflow'; Description = 'Complete development workflow'; Function = 'Invoke-BusBuddyDevWorkflow' }
        )
        'Analysis'    = @(
            @{ Name = 'bb-manage-dependencies'; Description = 'Dependency management'; Function = 'Invoke-BusBuddyDependencyManagement' }
            @{ Name = 'bb-error-fix'; Description = 'Analyze build errors'; Function = 'Invoke-BusBuddyErrorAnalysis' }
            @{ Name = 'bb-warning-analysis'; Description = 'Analyze build warnings'; Function = 'Show-BusBuddyWarningAnalysis' }
            @{ Name = 'bb-get-workflow-results'; Description = 'Monitor GitHub workflows'; Function = 'Get-BusBuddyWorkflowResults' }
        )
        'GitHub'      = @(
            @{ Name = 'bb-github-stage'; Description = 'Smart Git staging'; Function = 'Invoke-BusBuddyGitHubStaging' }
            @{ Name = 'bb-github-commit'; Description = 'Intelligent commit creation'; Function = 'Invoke-BusBuddyGitHubCommit' }
            @{ Name = 'bb-github-push'; Description = 'Push with workflow monitoring'; Function = 'Invoke-BusBuddyGitHubPush' }
            @{ Name = 'bb-github-workflow'; Description = 'Complete GitHub workflow'; Function = 'Invoke-BusBuddyCompleteGitHubWorkflow' }
        )
        'Environment' = @(
            @{ Name = 'bb-install-extensions'; Description = 'Install VS Code extensions'; Function = 'Install-BusBuddyVSCodeExtensions' }
            @{ Name = 'bb-validate-vscode'; Description = 'Validate VS Code setup'; Function = 'Test-BusBuddyVSCodeSetup' }
        )
        'Fun'         = @(
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
    }
    else {
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
    }
    else {
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
        }
        else {
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

function Invoke-BusBuddyDependencyManagement {
    <#
    .SYNOPSIS
        Comprehensive dependency management for Bus Buddy (bb-manage-dependencies)

    .DESCRIPTION
        Manages NuGet dependencies, vulnerability scanning, version validation,
        and dependency health monitoring. Incorporates functionality from the
        original dependency-management.ps1 script.

    .PARAMETER ScanVulnerabilities
        Scan for vulnerable packages

    .PARAMETER ValidateVersions
        Validate version pinning across projects

    .PARAMETER GenerateReport
        Generate dependency report

    .PARAMETER UpdatePackages
        Update packages to latest versions

    .PARAMETER RestoreOnly
        Restore packages only

    .PARAMETER Full
        Run all checks
    #>
    [CmdletBinding()]
    param(
        [switch]$ScanVulnerabilities,
        [switch]$ValidateVersions,
        [switch]$GenerateReport,
        [switch]$UpdatePackages,
        [switch]$RestoreOnly,
        [switch]$Full
    )

    # Set all switches if Full is specified
    if ($Full) {
        $ScanVulnerabilities = $true
        $ValidateVersions = $true
        $GenerateReport = $true
        $RestoreOnly = $true
    }

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        Write-BusBuddyStatus "Project root not found" -Status Error
        return $false
    }

    Push-Location $projectRoot

    try {
        Write-BusBuddyStatus "🚌 BusBuddy Dependency Management & Security Scanner" -Status Info
        Write-Host "📅 $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
        Write-Host ""

        $results = @{
            RestoreSuccessful    = $false
            VulnerabilitiesFound = 0
            VersionIssues        = 0
            ReportGenerated      = $false
            UpdatesAvailable     = 0
        }

        # Step 1: Package Restore
        if ($RestoreOnly -or $Full) {
            Write-Host "📦 Step 1: Package Restoration" -ForegroundColor Cyan
            Write-Host "=============================" -ForegroundColor Cyan

            $restoreResult = Invoke-BusBuddyRestore
            $results.RestoreSuccessful = $restoreResult

            if (-not $restoreResult) {
                Write-BusBuddyStatus "Package restore failed - stopping dependency management" -Status Error
                return $results
            }
        }

        # Step 2: Vulnerability Scanning
        if ($ScanVulnerabilities) {
            Write-Host ""
            Write-Host "🛡️ Step 2: Vulnerability Scanning" -ForegroundColor Cyan
            Write-Host "==================================" -ForegroundColor Cyan

            $vulnResults = Invoke-VulnerabilityScanning
            $results.VulnerabilitiesFound = $vulnResults.Count
        }

        # Step 3: Version Validation
        if ($ValidateVersions) {
            Write-Host ""
            Write-Host "📌 Step 3: Version Pinning Validation" -ForegroundColor Cyan
            Write-Host "=====================================" -ForegroundColor Cyan

            $versionResults = Test-VersionPinning
            $results.VersionIssues = $versionResults.Issues.Count
        }

        # Step 4: Report Generation
        if ($GenerateReport) {
            Write-Host ""
            Write-Host "📄 Step 4: Dependency Report Generation" -ForegroundColor Cyan
            Write-Host "=======================================" -ForegroundColor Cyan

            $reportResult = New-DependencyReport
            $results.ReportGenerated = $reportResult
        }

        # Step 5: Package Updates
        if ($UpdatePackages) {
            Write-Host ""
            Write-Host "⬆️ Step 5: Package Updates" -ForegroundColor Cyan
            Write-Host "==========================" -ForegroundColor Cyan

            $updateResults = Update-Packages
            $results.UpdatesAvailable = $updateResults.Count
        }

        # Summary
        Write-Host ""
        Show-DependencySummary -Results $results

        return $results
    }
    catch {
        Write-BusBuddyStatus "Dependency management error: $($_.Exception.Message)" -Status Error
        return $results
    }
    finally {
        Pop-Location
    }
}

function Invoke-BusBuddyErrorAnalysis {
    <#
    .SYNOPSIS
        Analyze and fix Bus Buddy build errors (bb-error-fix)

    .DESCRIPTION
        Analyzes build errors and provides automated fix recommendations.
        Incorporates functionality from the original bb-error-fix.ps1 script.

    .PARAMETER BuildOutput
        Build output text to analyze

    .PARAMETER AutoFix
        Automatically apply fixes where possible

    .PARAMETER Detailed
        Provide detailed error analysis
    #>
    [CmdletBinding()]
    param(
        [string]$BuildOutput,
        [switch]$AutoFix,
        [switch]$Detailed
    )

    Write-BusBuddyStatus "🔍 Bus Buddy Error Analysis Tool" -Status Info
    Write-Host "=================================" -ForegroundColor Cyan

    $errors = @()
    $fixes = @()

    if (-not $BuildOutput) {
        # Get build output by running a build
        Write-Host "🔨 Running build to capture errors..." -ForegroundColor Yellow
        $projectRoot = Get-BusBuddyProjectRoot
        if ($projectRoot) {
            Push-Location $projectRoot
            try {
                $BuildOutput = dotnet build BusBuddy.sln --verbosity normal 2>&1 | Out-String
            }
            finally {
                Pop-Location
            }
        }
    }

    if (-not $BuildOutput) {
        Write-BusBuddyStatus "No build output to analyze" -Status Warning
        return
    }

    # Extract specific errors
    $buildLines = $BuildOutput -split "`n"

    foreach ($line in $buildLines) {
        # XAML namespace errors
        if ($line -match "error MC3074.*'(.+?)' does not exist.*Line (\d+)") {
            $tagName = $matches[1]
            $lineNumber = $matches[2]
            $filePath = if ($line -match "([^\\]+\.xaml)") { $matches[1] } else { "Unknown" }

            $errors += [PSCustomObject]@{
                Type     = "XAML Namespace"
                File     = $filePath
                Line     = $lineNumber
                Element  = $tagName
                Message  = "Tag '$tagName' does not exist"
                Severity = "Error"
            }

            # Common XAML fixes
            $suggestedFix = switch -Regex ($tagName) {
                "^sf:" { "Add Syncfusion namespace: xmlns:sf=""http://schemas.syncfusion.com/wpf""" }
                "^syncfusion:" { "Add Syncfusion namespace: xmlns:syncfusion=""http://schemas.syncfusion.com/wpf""" }
                "^local:" { "Add local namespace: xmlns:local=""clr-namespace:YourNamespace""" }
                default { "Check namespace declaration for '$tagName'" }
            }

            $fixes += [PSCustomObject]@{
                ErrorType   = "XAML Namespace"
                File        = $filePath
                Line        = $lineNumber
                Suggestion  = $suggestedFix
                AutoFixable = $false
            }
        }

        # C# compilation errors
        if ($line -match "error CS\d+.*") {
            $errors += [PSCustomObject]@{
                Type     = "C# Compilation"
                Message  = $line.Trim()
                Severity = "Error"
            }
        }

        # Package reference errors
        if ($line -match "error NU\d+.*") {
            $errors += [PSCustomObject]@{
                Type     = "NuGet Package"
                Message  = $line.Trim()
                Severity = "Error"
            }

            $fixes += [PSCustomObject]@{
                ErrorType   = "Package Reference"
                Suggestion  = "Run 'dotnet restore' or check package.lock.json"
                AutoFixable = $true
            }
        }
    }

    # Display results
    Write-Host ""
    Write-Host "📊 Error Analysis Results:" -ForegroundColor Cyan
    Write-Host "Total Errors Found: $($errors.Count)" -ForegroundColor $(if ($errors.Count -eq 0) { 'Green' } else { 'Red' })

    if ($errors.Count -gt 0) {
        Write-Host ""
        Write-Host "❌ Errors Detected:" -ForegroundColor Red
        $errors | Group-Object Type | ForEach-Object {
            Write-Host "  $($_.Name): $($_.Count) errors" -ForegroundColor Yellow
            if ($Detailed) {
                $_.Group | ForEach-Object {
                    Write-Host "    • $($_.Message)" -ForegroundColor Gray
                }
            }
        }

        Write-Host ""
        Write-Host "🔧 Suggested Fixes:" -ForegroundColor Yellow
        $fixes | ForEach-Object {
            $fixIcon = if ($_.AutoFixable) { "🤖" } else { "💡" }
            Write-Host "  $fixIcon $($_.Suggestion)" -ForegroundColor Cyan
        }

        if ($AutoFix) {
            Write-Host ""
            Write-Host "🤖 Applying automatic fixes..." -ForegroundColor Green

            $autoFixableFixes = $fixes | Where-Object { $_.AutoFixable }
            foreach ($fix in $autoFixableFixes) {
                switch ($fix.ErrorType) {
                    "Package Reference" {
                        Write-Host "   Restoring packages..." -ForegroundColor Yellow
                        Invoke-BusBuddyRestore
                    }
                }
            }
        }
    }

    return @{
        Errors  = $errors
        Fixes   = $fixes
        Summary = @{
            TotalErrors       = $errors.Count
            AutoFixableErrors = ($fixes | Where-Object { $_.AutoFixable }).Count
        }
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

#region Development Workflow Functions

# Helper functions for dependency management
function Invoke-VulnerabilityScanning {
    <#
    .SYNOPSIS
        Scan for vulnerable NuGet packages
    #>
    [CmdletBinding()]
    param()

    Write-Host "🔍 Scanning for vulnerable packages..." -ForegroundColor Yellow

    try {
        # Use dotnet list package --vulnerable if available
        $vulnerableOutput = & dotnet list package --vulnerable 2>&1

        if ($LASTEXITCODE -eq 0) {
            $vulnerableLines = $vulnerableOutput | Where-Object { $_ -match ">" -and $_ -notmatch "The following sources were used" }

            if ($vulnerableLines.Count -gt 0) {
                Write-Host "⚠️ Found $($vulnerableLines.Count) vulnerable packages:" -ForegroundColor Red
                foreach ($line in $vulnerableLines) {
                    Write-Host "  • $line" -ForegroundColor Red
                }
                return $vulnerableLines
            }
            else {
                Write-Host "✅ No vulnerable packages detected" -ForegroundColor Green
                return @()
            }
        }
        else {
            Write-Host "⚠️ Vulnerability scanning not available in this .NET version" -ForegroundColor Yellow
            return @()
        }
    }
    catch {
        Write-Host "❌ Error during vulnerability scanning: $($_.Exception.Message)" -ForegroundColor Red
        return @()
    }
}

function Test-VersionPinning {
    <#
    .SYNOPSIS
        Validate package version pinning across projects
    #>
    [CmdletBinding()]
    param()

    Write-Host "📌 Validating version pinning..." -ForegroundColor Yellow

    $issues = @()

    try {
        # Find all project files
        $projectFiles = Get-ChildItem -Recurse -Name "*.csproj"

        foreach ($projectFile in $projectFiles) {
            $content = Get-Content $projectFile -Raw

            # Check for floating version references (e.g., 1.0.* or 1.0.+)
            if ($content -match 'Version="[^"]*[\*\+][^"]*"') {
                $issues += "Floating version detected in $projectFile"
            }

            # Check for missing version attributes
            $packageRefs = $content | Select-String -Pattern '<PackageReference\s+Include="([^"]*)"(?:\s+Version="([^"]*)")?'
            foreach ($match in $packageRefs.Matches) {
                if (-not $match.Groups[2].Value) {
                    $issues += "Missing version for package $($match.Groups[1].Value) in $projectFile"
                }
            }
        }

        if ($issues.Count -eq 0) {
            Write-Host "✅ All packages have proper version pinning" -ForegroundColor Green
        }
        else {
            Write-Host "⚠️ Found $($issues.Count) version pinning issues:" -ForegroundColor Yellow
            foreach ($issue in $issues) {
                Write-Host "  • $issue" -ForegroundColor Red
            }
        }

        return @{ Issues = $issues }
    }
    catch {
        Write-Host "❌ Error validating version pinning: $($_.Exception.Message)" -ForegroundColor Red
        return @{ Issues = @("Version validation failed: $($_.Exception.Message)") }
    }
}

function New-DependencyReport {
    <#
    .SYNOPSIS
        Generate dependency report
    #>
    [CmdletBinding()]
    param()

    Write-Host "📄 Generating dependency report..." -ForegroundColor Yellow

    try {
        $reportPath = "dependency-report-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"

        # Get package list
        $packageOutput = & dotnet list package --include-transitive 2>&1

        $report = @{
            Timestamp    = Get-Date
            Packages     = $packageOutput | Where-Object { $_ -match ">" }
            ProjectFiles = (Get-ChildItem -Recurse -Name "*.csproj")
            Summary      = @{
                TotalProjects = (Get-ChildItem -Recurse -Name "*.csproj").Count
                PackageCount  = ($packageOutput | Where-Object { $_ -match ">" }).Count
            }
        }

        $report | ConvertTo-Json -Depth 3 | Out-File $reportPath -Encoding UTF8
        Write-Host "✅ Report generated: $reportPath" -ForegroundColor Green

        return $true
    }
    catch {
        Write-Host "❌ Error generating report: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Update-Packages {
    <#
    .SYNOPSIS
        Update packages to latest versions
    #>
    [CmdletBinding()]
    param()

    Write-Host "⬆️ Checking for package updates..." -ForegroundColor Yellow

    try {
        # Check for outdated packages
        $outdatedOutput = & dotnet list package --outdated 2>&1
        $outdatedPackages = $outdatedOutput | Where-Object { $_ -match ">" -and $_ -notmatch "Project" }

        if ($outdatedPackages.Count -gt 0) {
            Write-Host "📦 Found $($outdatedPackages.Count) outdated packages:" -ForegroundColor Yellow
            foreach ($package in $outdatedPackages) {
                Write-Host "  • $package" -ForegroundColor Cyan
            }
        }
        else {
            Write-Host "✅ All packages are up to date" -ForegroundColor Green
        }

        return $outdatedPackages
    }
    catch {
        Write-Host "❌ Error checking for updates: $($_.Exception.Message)" -ForegroundColor Red
        return @()
    }
}

function Show-DependencySummary {
    <#
    .SYNOPSIS
        Show dependency management summary
    #>
    [CmdletBinding()]
    param(
        [hashtable]$Results
    )

    Write-Host ""
    Write-Host "📊 Dependency Management Summary:" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor DarkGray
    Write-Host "✅ Package Restore: $(if ($Results.RestoreSuccessful) { 'Success' } else { 'Failed' })" -ForegroundColor $(if ($Results.RestoreSuccessful) { 'Green' } else { 'Red' })
    Write-Host "🛡️ Vulnerabilities: $($Results.VulnerabilitiesFound)" -ForegroundColor $(if ($Results.VulnerabilitiesFound -eq 0) { 'Green' } else { 'Red' })
    Write-Host "📌 Version Issues: $($Results.VersionIssues)" -ForegroundColor $(if ($Results.VersionIssues -eq 0) { 'Green' } else { 'Yellow' })
    Write-Host "📄 Report Generated: $(if ($Results.ReportGenerated) { 'Yes' } else { 'No' })" -ForegroundColor $(if ($Results.ReportGenerated) { 'Green' } else { 'Gray' })

    if ($Results.UpdatesAvailable -gt 0) {
        Write-Host "⬆️ Updates Available: $($Results.UpdatesAvailable)" -ForegroundColor Yellow
    }

    Write-Host ""
    $overallScore = if ($Results.RestoreSuccessful -and $Results.VulnerabilitiesFound -eq 0 -and $Results.VersionIssues -eq 0) { 'Excellent' } elseif ($Results.RestoreSuccessful) { 'Good' } else { 'Needs Attention' }
    Write-Host "Overall Status: $overallScore" -ForegroundColor $(
        switch ($overallScore) {
            'Excellent' { 'Green' }
            'Good' { 'Yellow' }
            default { 'Red' }
        }
    )
}

function Invoke-BusBuddyDevWorkflow {
    <#
    .SYNOPSIS
        Comprehensive development workflow for Bus Buddy (bb-dev-workflow)

    .DESCRIPTION
        Orchestrates a complete development cycle: build → validate → test → identify next Phase 2 target.
        Handles warnings from analyzers, validates against rulesets, and provides structured development guidance.

    .PARAMETER FullCycle
        Run complete cycle including application launch

    .PARAMETER SkipValidation
        Skip deep validation and warning analysis

    .PARAMETER SkipTests
        Skip test execution

    .PARAMETER Phase2Target
        Override automatic Phase 2 target selection

    .PARAMETER AnalysisMode
        Enable comprehensive analysis mode for warnings
    #>
    [CmdletBinding()]
    param(
        [switch]$FullCycle,
        [switch]$SkipValidation,
        [switch]$SkipTests,
        [string]$Phase2Target,
        [switch]$AnalysisMode
    )

    $startTime = Get-Date
    Write-BusBuddyStatus "🚌 Starting BusBuddy Development Workflow" -Status Info
    Write-Host "Started at: $($startTime.ToString('yyyy-MM-dd HH:mm:ss'))" -ForegroundColor Gray
    Write-Host ""

    $workflowResults = @{
        BuildSuccess     = $false
        ValidationPassed = $false
        TestsPassed      = $false
        WarningCount     = 0
        NextPhase2Target = $null
        Duration         = $null
        Issues           = @()
        Recommendations  = @()
    }

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        Write-BusBuddyStatus "Project root not found - workflow cannot continue" -Status Error
        return $workflowResults
    }

    Push-Location $projectRoot

    try {
        # =====================================
        # Step 1: Clean Build with Analysis
        # =====================================
        Write-BusBuddyStatus "Step 1: Building solution..." -Status Build
        Write-Host "🔨 Building with clean state and analyzer validation" -ForegroundColor Cyan

        # Clean build to ensure fresh state
        $cleanResult = Invoke-BusBuddyClean -Configuration Debug
        if (-not $cleanResult) {
            $workflowResults.Issues += "Clean operation failed"
        }

        # Build with enhanced analysis
        $buildResult = Invoke-BusBuddyBuild -Configuration Debug -Restore -Verbosity normal
        $workflowResults.BuildSuccess = $buildResult

        if (-not $buildResult) {
            Write-BusBuddyStatus "❌ Build failed - workflow terminated" -Status Error
            $workflowResults.Issues += "Build compilation failed"
            return $workflowResults
        }

        Write-BusBuddyStatus "✅ Build completed successfully" -Status Success

        # =====================================
        # Step 2: Deep Validation & Analysis
        # =====================================
        if (-not $SkipValidation) {
            Write-Host ""
            Write-BusBuddyStatus "Step 2: Validation and Analysis..." -Status Info
            Write-Host "🔍 Running comprehensive validation checks" -ForegroundColor Cyan

            # Enhanced build with analyzer reporting
            if ($AnalysisMode) {
                Write-Host "🔬 Running enhanced analyzer validation..." -ForegroundColor Yellow
                $analyzerOutput = & dotnet build BusBuddy.sln --no-incremental /p:EnableNETAnalyzers=true /p:AnalysisMode=All /p:ReportAnalyzer=true --verbosity normal 2>&1

                if ($analyzerOutput) {
                    $warnings = $analyzerOutput | Select-String -Pattern "warning"
                    $workflowResults.WarningCount = ($warnings | Measure-Object).Count

                    Write-Host "📊 Analyzer Results:" -ForegroundColor Yellow
                    Write-Host "   • Total Warnings: $($workflowResults.WarningCount)" -ForegroundColor $(if ($workflowResults.WarningCount -lt 100) { 'Green' } elseif ($workflowResults.WarningCount -lt 300) { 'Yellow' } else { 'Red' })

                    # Focus on null safety warnings (CS860x)
                    $nullSafetyWarnings = $warnings | Where-Object { $_.Line -match "CS860\d" }
                    if ($nullSafetyWarnings) {
                        Write-Host "   • Null Safety Warnings (CS860x): $($nullSafetyWarnings.Count)" -ForegroundColor Red
                        $workflowResults.Recommendations += "Address null safety warnings (CS860x) - these are configured as errors in BusBuddy-Practical.ruleset"
                    }
                }
            }

            # Health check validation
            Write-Host "🏥 Running health check..." -ForegroundColor Yellow
            $healthResult = Invoke-BusBuddyHealthCheck -Quick
            $workflowResults.ValidationPassed = $healthResult

            # Dependency vulnerability scanning
            Write-Host "🛡️ Scanning dependencies for vulnerabilities..." -ForegroundColor Yellow
            try {
                $depResults = Invoke-BusBuddyDependencyManagement -ScanVulnerabilities -ValidateVersions
                if ($depResults.VulnerabilitiesFound -gt 0) {
                    $workflowResults.Issues += "$($depResults.VulnerabilitiesFound) security vulnerabilities found in dependencies"
                }
                if ($depResults.VersionIssues -gt 0) {
                    $workflowResults.Issues += "$($depResults.VersionIssues) version pinning issues detected"
                }
            }
            catch {
                Write-BusBuddyStatus "⚠️ Dependency analysis failed: $($_.Exception.Message)" -Status Warning
                $workflowResults.Issues += "Dependency analysis incomplete"
            }

            Write-BusBuddyStatus "$(if ($workflowResults.ValidationPassed) { '✅' } else { '⚠️' }) Validation completed" -Status $(if ($workflowResults.ValidationPassed) { 'Success' } else { 'Warning' })
        }

        # =====================================
        # Step 3: Test Execution
        # =====================================
        if (-not $SkipTests) {
            Write-Host ""
            Write-BusBuddyStatus "Step 3: Running test suite..." -Status Test
            Write-Host "🧪 Executing comprehensive test validation" -ForegroundColor Cyan

            $testResult = Invoke-BusBuddyTest -Configuration Debug -Logger "console;verbosity=minimal"
            $workflowResults.TestsPassed = $testResult

            if ($testResult) {
                Write-BusBuddyStatus "✅ All tests passed" -Status Success
            }
            else {
                Write-BusBuddyStatus "⚠️ Some tests failed or had issues" -Status Warning
                $workflowResults.Issues += "Test execution encountered failures"
            }
        }

        # =====================================
        # Step 4: Phase 2 Target Identification
        # =====================================
        Write-Host ""
        Write-BusBuddyStatus "Step 4: Identifying next Phase 2 target..." -Status Info
        Write-Host "🎯 Analyzing project state for Phase 2 priorities" -ForegroundColor Cyan

        if ($Phase2Target) {
            $workflowResults.NextPhase2Target = $Phase2Target
            Write-Host "🎯 Using specified target: $Phase2Target" -ForegroundColor Green
        }
        else {
            # Intelligent Phase 2 target selection based on project state
            $phase2Targets = @(
                @{
                    Target      = "Implement comprehensive UI testing with Syncfusion controls"
                    Priority    = if ($workflowResults.TestsPassed) { 3 } else { 1 }
                    Description = "Automated UI testing for dashboard, driver management, and vehicle tracking"
                }
                @{
                    Target      = "Integrate real-world data seeding and Azure SQL connectivity"
                    Priority    = if ($workflowResults.ValidationPassed) { 2 } else { 3 }
                    Description = "Production-ready data integration with sample transportation data"
                }
                @{
                    Target      = "Enhance Dashboard with advanced analytics and reporting"
                    Priority    = if ($workflowResults.BuildSuccess) { 2 } else { 4 }
                    Description = "Business intelligence dashboard with charts and KPI monitoring"
                }
                @{
                    Target      = "Optimize routing algorithms with xAI Grok API integration"
                    Priority    = 4
                    Description = "AI-powered route optimization and predictive maintenance"
                }
                @{
                    Target      = "Reduce analyzer warnings and improve code quality (Focus: CS860x null safety)"
                    Priority    = if ($workflowResults.WarningCount -gt 200) { 1 } else { 3 }
                    Description = "Address null safety warnings and improve overall code quality metrics"
                }
                @{
                    Target      = "Implement advanced MVVM patterns and dependency injection"
                    Priority    = if ($workflowResults.ValidationPassed) { 3 } else { 2 }
                    Description = "Upgrade to sophisticated MVVM with proper IoC container integration"
                }
            )

            # Select target based on priority (lower number = higher priority)
            $selectedTarget = $phase2Targets | Sort-Object Priority | Select-Object -First 1
            $workflowResults.NextPhase2Target = $selectedTarget.Target

            Write-Host "🎯 Recommended Phase 2 Target:" -ForegroundColor Green
            Write-Host "   Target: $($selectedTarget.Target)" -ForegroundColor White
            Write-Host "   Priority: $($selectedTarget.Priority)" -ForegroundColor Gray
            Write-Host "   Description: $($selectedTarget.Description)" -ForegroundColor Gray
        }

        # =====================================
        # Step 5: Full Cycle (Optional)
        # =====================================
        if ($FullCycle) {
            Write-Host ""
            Write-BusBuddyStatus "Step 5: Launching application..." -Status Info
            Write-Host "🚀 Starting Bus Buddy application for validation" -ForegroundColor Cyan

            try {
                $runResult = Invoke-BusBuddyRun -Configuration Debug -NoBuild
                if ($runResult) {
                    Write-BusBuddyStatus "✅ Application launched successfully" -Status Success
                }
                else {
                    Write-BusBuddyStatus "⚠️ Application launch had issues" -Status Warning
                    $workflowResults.Issues += "Application runtime issues detected"
                }
            }
            catch {
                Write-BusBuddyStatus "❌ Application launch failed: $($_.Exception.Message)" -Status Error
                $workflowResults.Issues += "Application launch failed"
            }
        }

        # =====================================
        # Workflow Summary
        # =====================================
        $endTime = Get-Date
        $workflowResults.Duration = $endTime - $startTime

        Write-Host ""
        Write-BusBuddyStatus "🏁 Development Workflow Complete" -Status Success
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor DarkGray
        Write-Host "📊 Workflow Summary:" -ForegroundColor Cyan
        Write-Host "   • Duration: $([math]::Round($workflowResults.Duration.TotalMinutes, 1)) minutes" -ForegroundColor Gray
        Write-Host "   • Build: $(if ($workflowResults.BuildSuccess) { '✅ Success' } else { '❌ Failed' })" -ForegroundColor $(if ($workflowResults.BuildSuccess) { 'Green' } else { 'Red' })
        Write-Host "   • Validation: $(if ($workflowResults.ValidationPassed) { '✅ Passed' } else { '⚠️ Issues' })" -ForegroundColor $(if ($workflowResults.ValidationPassed) { 'Green' } else { 'Yellow' })
        Write-Host "   • Tests: $(if ($workflowResults.TestsPassed) { '✅ Passed' } else { '⚠️ Issues' })" -ForegroundColor $(if ($workflowResults.TestsPassed) { 'Green' } else { 'Yellow' })
        Write-Host "   • Warnings: $($workflowResults.WarningCount)" -ForegroundColor $(if ($workflowResults.WarningCount -lt 100) { 'Green' } elseif ($workflowResults.WarningCount -lt 300) { 'Yellow' } else { 'Red' })

        if ($workflowResults.Issues.Count -gt 0) {
            Write-Host ""
            Write-Host "⚠️ Issues Identified:" -ForegroundColor Yellow
            foreach ($issue in $workflowResults.Issues) {
                Write-Host "   • $issue" -ForegroundColor Red
            }
        }

        if ($workflowResults.Recommendations.Count -gt 0) {
            Write-Host ""
            Write-Host "💡 Recommendations:" -ForegroundColor Blue
            foreach ($rec in $workflowResults.Recommendations) {
                Write-Host "   • $rec" -ForegroundColor Cyan
            }
        }

        Write-Host ""
        Write-Host "🎯 Next Phase 2 Focus:" -ForegroundColor Magenta
        Write-Host "   $($workflowResults.NextPhase2Target)" -ForegroundColor White

        return $workflowResults
    }
    catch {
        Write-BusBuddyStatus "❌ Workflow error: $($_.Exception.Message)" -Status Error
        $workflowResults.Issues += "Workflow execution error: $($_.Exception.Message)"
        return $workflowResults
    }
    finally {
        Pop-Location
    }
}

function Get-BusBuddyWorkflowResults {
    <#
    .SYNOPSIS
        Monitor GitHub Actions workflow results (bb-get-workflow-results)

    .DESCRIPTION
        Retrieves and displays GitHub Actions workflow run status and results.
        Supports both GitHub CLI and REST API methods for comprehensive monitoring.

    .PARAMETER Count
        Number of recent workflow runs to retrieve

    .PARAMETER Repository
        Repository name (defaults to current repository)

    .PARAMETER Status
        Filter by workflow status (completed, in_progress, queued)

    .PARAMETER Detailed
        Show detailed workflow information including logs
    #>
    [CmdletBinding()]
    param(
        [int]$Count = 5,
        [string]$Repository = "Bigessfour/BusBuddy-2",
        [ValidateSet('completed', 'in_progress', 'queued', 'all')]
        [string]$Status = 'all',
        [switch]$Detailed
    )

    Write-BusBuddyStatus "📡 Retrieving GitHub Actions workflow results..." -Status Info

    $results = @()

    try {
        # Method 1: Try GitHub CLI first (preferred)
        if (Get-Command gh -ErrorAction SilentlyContinue) {
            Write-Host "🔗 Using GitHub CLI for workflow data..." -ForegroundColor Yellow

            $ghArgs = @('run', 'list', '--limit', $Count, '--json', 'status,conclusion,createdAt,name,workflowName,id,event,headBranch')
            if ($Status -ne 'all') {
                $ghArgs += '--status', $Status
            }

            try {
                $ghOutput = & gh @ghArgs 2>&1
                if ($LASTEXITCODE -eq 0) {
                    $results = $ghOutput | ConvertFrom-Json
                    Write-BusBuddyStatus "✅ Retrieved $($results.Count) workflow runs via GitHub CLI" -Status Success
                }
                else {
                    Write-BusBuddyStatus "⚠️ GitHub CLI failed: $ghOutput" -Status Warning
                    throw "GitHub CLI failed"
                }
            }
            catch {
                Write-BusBuddyStatus "⚠️ GitHub CLI error, falling back to REST API..." -Status Warning
                $results = @()  # Clear results to trigger fallback
            }
        }

        # Method 2: Fallback to REST API
        if ($results.Count -eq 0) {
            Write-Host "🌐 Using GitHub REST API for workflow data..." -ForegroundColor Yellow

            # Check for GitHub token
            if (-not $env:GITHUB_TOKEN) {
                Write-BusBuddyStatus "❌ GITHUB_TOKEN environment variable not set" -Status Error
                Write-Host "💡 Set your GitHub Personal Access Token:" -ForegroundColor Blue
                Write-Host "   `$env:GITHUB_TOKEN = 'your_token_here'" -ForegroundColor Gray
                Write-Host "   Or: gh auth login" -ForegroundColor Gray
                return $null
            }

            $headers = @{
                Authorization = "token $env:GITHUB_TOKEN"
                Accept        = "application/vnd.github.v3+json"
            }

            $queryParams = @("per_page=$Count")
            if ($Status -ne 'all') { $queryParams += "status=$Status" }
            $fullUrl = "https://api.github.com/repos/$Repository/actions/runs?$($queryParams -join '&')"

            try {
                $response = Invoke-RestMethod -Uri $fullUrl -Headers $headers -Method Get
                $results = $response.workflow_runs | Select-Object -First $Count

                # Convert to consistent format
                $results = $results | ForEach-Object {
                    [PSCustomObject]@{
                        id           = $_.id
                        name         = $_.name
                        workflowName = $_.name
                        status       = $_.status
                        conclusion   = $_.conclusion
                        createdAt    = $_.created_at
                        event        = $_.event
                        headBranch   = $_.head_branch
                    }
                }

                Write-BusBuddyStatus "✅ Retrieved $($results.Count) workflow runs via REST API" -Status Success
            }
            catch {
                Write-BusBuddyStatus "❌ REST API error: $($_.Exception.Message)" -Status Error
                return $null
            }
        }

        # Display results
        if ($results.Count -eq 0) {
            Write-BusBuddyStatus "ℹ️ No workflow runs found" -Status Info
            return $null
        }

        Write-Host ""
        Write-Host "📊 GitHub Actions Workflow Results:" -ForegroundColor Cyan
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor DarkGray

        if ($Detailed) {
            foreach ($run in $results) {
                $statusIcon = switch ($run.status) {
                    'completed' { if ($run.conclusion -eq 'success') { '✅' } elseif ($run.conclusion -eq 'failure') { '❌' } else { '⚠️' } }
                    'in_progress' { '🔄' }
                    'queued' { '⏳' }
                    default { '❓' }
                }

                Write-Host ""
                Write-Host "$statusIcon Workflow: $($run.workflowName)" -ForegroundColor White
                Write-Host "   • ID: $($run.id)" -ForegroundColor Gray
                Write-Host "   • Status: $($run.status)" -ForegroundColor $(if ($run.status -eq 'completed') { 'Green' } elseif ($run.status -eq 'in_progress') { 'Yellow' } else { 'Gray' })
                Write-Host "   • Conclusion: $($run.conclusion)" -ForegroundColor $(if ($run.conclusion -eq 'success') { 'Green' } elseif ($run.conclusion -eq 'failure') { 'Red' } else { 'Yellow' })
                Write-Host "   • Created: $([DateTime]::Parse($run.createdAt).ToString('yyyy-MM-dd HH:mm:ss'))" -ForegroundColor Gray
                Write-Host "   • Event: $($run.event)" -ForegroundColor Gray
                Write-Host "   • Branch: $($run.headBranch)" -ForegroundColor Gray
            }
        }
        else {
            # Compact table format
            $tableData = $results | ForEach-Object {
                $statusIcon = switch ($_.status) {
                    'completed' { if ($_.conclusion -eq 'success') { '✅' } elseif ($_.conclusion -eq 'failure') { '❌' } else { '⚠️' } }
                    'in_progress' { '🔄' }
                    'queued' { '⏳' }
                    default { '❓' }
                }

                [PSCustomObject]@{
                    Status     = "$statusIcon $($_.status)"
                    Workflow   = $_.workflowName
                    Conclusion = $_.conclusion
                    Created    = [DateTime]::Parse($_.createdAt).ToString('MM-dd HH:mm')
                    Branch     = $_.headBranch
                    Event      = $_.event
                }
            }

            $tableData | Format-Table -AutoSize
        }

        # Summary statistics
        $statusCounts = $results | Group-Object status
        $conclusionCounts = $results | Where-Object { $_.conclusion } | Group-Object conclusion

        Write-Host ""
        Write-Host "📈 Summary Statistics:" -ForegroundColor Cyan
        foreach ($statusGroup in $statusCounts) {
            Write-Host "   • $($statusGroup.Name): $($statusGroup.Count)" -ForegroundColor Gray
        }
        if ($conclusionCounts) {
            Write-Host "   Recent Conclusions:" -ForegroundColor Gray
            foreach ($conclusionGroup in $conclusionCounts) {
                $color = switch ($conclusionGroup.Name) {
                    'success' { 'Green' }
                    'failure' { 'Red' }
                    default { 'Yellow' }
                }
                Write-Host "     - $($conclusionGroup.Name): $($conclusionGroup.Count)" -ForegroundColor $color
            }
        }

        return $results
    }
    catch {
        Write-BusBuddyStatus "❌ Error retrieving workflow results: $($_.Exception.Message)" -Status Error
        return $null
    }
}

function Show-BusBuddyWarningAnalysis {
    <#
    .SYNOPSIS
        Analyze and categorize build warnings (bb-warning-analysis)

    .DESCRIPTION
        Provides detailed analysis of build warnings with focus on null safety (CS860x)
        and actionable recommendations for reducing warning count below 100.

    .PARAMETER ShowTop
        Number of top warning types to display

    .PARAMETER FocusNullSafety
        Focus analysis on null safety warnings (CS860x series)

    .PARAMETER GenerateReport
        Generate detailed warning report file
    #>
    [CmdletBinding()]
    param(
        [int]$ShowTop = 10,
        [switch]$FocusNullSafety,
        [switch]$GenerateReport
    )

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        Write-BusBuddyStatus "Project root not found" -Status Error
        return
    }

    Push-Location $projectRoot

    try {
        Write-BusBuddyStatus "🔍 Analyzing build warnings..." -Status Info

        # Get detailed build output with warnings
        $buildOutput = & dotnet build BusBuddy.sln --verbosity normal /p:EnableNETAnalyzers=true 2>&1
        $warnings = $buildOutput | Select-String -Pattern "warning"

        if (-not $warnings) {
            Write-BusBuddyStatus "🎉 No warnings found!" -Status Success
            return
        }

        Write-Host ""
        Write-Host "📊 Warning Analysis Results:" -ForegroundColor Cyan
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor DarkGray
        Write-Host "Total Warnings: $($warnings.Count)" -ForegroundColor $(if ($warnings.Count -lt 100) { 'Green' } elseif ($warnings.Count -lt 300) { 'Yellow' } else { 'Red' })

        # Categorize warnings
        $warningCategories = @{}
        $nullSafetyWarnings = @()

        foreach ($warning in $warnings) {
            $warningLine = $warning.Line

            # Extract warning code (e.g., CS8600, CA1234, etc.)
            if ($warningLine -match "warning\s+([A-Z]{2}\d{4})") {
                $warningCode = $matches[1]

                if (-not $warningCategories.ContainsKey($warningCode)) {
                    $warningCategories[$warningCode] = @()
                }
                $warningCategories[$warningCode] += $warningLine

                # Track null safety warnings specifically
                if ($warningCode -match "CS860\d") {
                    $nullSafetyWarnings += $warningLine
                }
            }
        }

        # Display top warning categories
        Write-Host ""
        Write-Host "🏆 Top $ShowTop Warning Categories:" -ForegroundColor Yellow
        $sortedCategories = $warningCategories.GetEnumerator() | Sort-Object { $_.Value.Count } -Descending | Select-Object -First $ShowTop

        foreach ($category in $sortedCategories) {
            $code = $category.Key
            $count = $category.Value.Count
            $description = Get-WarningCodeDescription -Code $code

            Write-Host "   $code ($count occurrences): $description" -ForegroundColor Cyan

            # Show sample for top 3
            if ($sortedCategories.IndexOf($category) -lt 3) {
                $sample = $category.Value[0] -replace '.*warning\s+[A-Z]{2}\d{4}:\s*', '' -replace '\s*\[.*\].*$', ''
                Write-Host "     Sample: $($sample.Substring(0, [Math]::Min(80, $sample.Length)))" -ForegroundColor Gray
            }
        }

        # Null safety analysis
        if ($FocusNullSafety -or $nullSafetyWarnings.Count -gt 0) {
            Write-Host ""
            Write-Host "🛡️ Null Safety Analysis (CS860x series):" -ForegroundColor Magenta
            Write-Host "Null Safety Warnings: $($nullSafetyWarnings.Count)" -ForegroundColor $(if ($nullSafetyWarnings.Count -eq 0) { 'Green' } else { 'Red' })

            if ($nullSafetyWarnings.Count -gt 0) {
                $nullSafetyByCode = $nullSafetyWarnings | ForEach-Object {
                    if ($_ -match "warning\s+(CS860\d)") { $matches[1] }
                } | Group-Object | Sort-Object Count -Descending

                foreach ($group in $nullSafetyByCode) {
                    Write-Host "   $($group.Name): $($group.Count) occurrences" -ForegroundColor Yellow
                }

                Write-Host ""
                Write-Host "💡 Null Safety Recommendations:" -ForegroundColor Blue
                Write-Host "   • Enable nullable reference types: <Nullable>enable</Nullable>" -ForegroundColor Cyan
                Write-Host "   • Add null checks: ArgumentNullException.ThrowIfNull(parameter)" -ForegroundColor Cyan
                Write-Host "   • Use null-conditional operators: object?.Property" -ForegroundColor Cyan
                Write-Host "   • Initialize collections in constructors to avoid null references" -ForegroundColor Cyan
            }
        }

        # Generate report if requested
        if ($GenerateReport) {
            $reportPath = Join-Path $projectRoot "warning-analysis-report.json"
            $reportData = @{
                Timestamp       = Get-Date
                TotalWarnings   = $warnings.Count
                Categories      = $warningCategories
                NullSafetyCount = $nullSafetyWarnings.Count
                TopCategories   = $sortedCategories | ForEach-Object { @{ Code = $_.Key; Count = $_.Value.Count } }
                Recommendations = @(
                    "Focus on CS860x null safety warnings first",
                    "Configure ruleset to suppress non-critical analyzer warnings",
                    "Aim for <100 total warnings by Phase 2 completion",
                    "Use dotnet build /p:ReportAnalyzer=true for detailed analysis"
                )
            }

            $reportData | ConvertTo-Json -Depth 3 | Out-File $reportPath -Encoding UTF8
            Write-BusBuddyStatus "📄 Detailed report saved to: $reportPath" -Status Info
        }

        # Final recommendations
        Write-Host ""
        Write-Host "🎯 Action Plan:" -ForegroundColor Green
        if ($warnings.Count -gt 300) {
            Write-Host "   1. HIGH PRIORITY: Reduce warnings from $($warnings.Count) to <300" -ForegroundColor Red
            Write-Host "   2. Configure BusBuddy-Practical.ruleset to suppress non-critical warnings" -ForegroundColor Yellow
            Write-Host "   3. Focus on null safety (CS860x) warnings first" -ForegroundColor Yellow
        }
        elseif ($warnings.Count -gt 100) {
            Write-Host "   1. MEDIUM PRIORITY: Reduce warnings from $($warnings.Count) to <100" -ForegroundColor Yellow
            Write-Host "   2. Address top 5 warning categories systematically" -ForegroundColor Cyan
        }
        else {
            Write-Host "   🎉 EXCELLENT: Warning count is under control!" -ForegroundColor Green
            Write-Host "   Focus on code quality improvements and Phase 2 features" -ForegroundColor Cyan
        }

    }
    catch {
        Write-BusBuddyStatus "❌ Error analyzing warnings: $($_.Exception.Message)" -Status Error
    }
    finally {
        Pop-Location
    }
}

function Get-WarningCodeDescription {
    <#
    .SYNOPSIS
        Get description for common warning codes
    #>
    param([string]$Code)

    $descriptions = @{
        'CS8600'  = 'Converting null literal or possible null value to non-nullable type'
        'CS8601'  = 'Possible null reference assignment'
        'CS8602'  = 'Dereference of a possibly null reference'
        'CS8603'  = 'Possible null reference return'
        'CS8604'  = 'Possible null reference argument'
        'CS8618'  = 'Non-nullable field must contain a non-null value when exiting constructor'
        'CS8625'  = 'Cannot convert null literal to non-nullable reference type'
        'CA1031'  = 'Do not catch general exception types'
        'CA1303'  = 'Do not pass literals as localized parameters'
        'CA1848'  = 'Use the LoggerMessage delegates'
        'CA2007'  = 'Consider calling ConfigureAwait on the awaited task'
        'CS1591'  = 'Missing XML comment for publicly visible type or member'
        'CA1822'  = 'Mark members as static'
        'CA1805'  = 'Do not initialize unnecessarily'
        'CA1812'  = 'Avoid uninstantiated internal classes'
        'IDE0051' = 'Remove unused private members'
        'IDE0052' = 'Remove unread private members'
    }

    return $descriptions[$Code] ?? "Unknown warning type"
}

function Install-BusBuddyVSCodeExtensions {
    <#
    .SYNOPSIS
        Install recommended VS Code extensions for Bus Buddy development (bb-install-extensions)

    .DESCRIPTION
        Installs the curated set of VS Code extensions for optimal Bus Buddy development.
        Supports .NET 9, WPF, PowerShell 7.5, Azure integration, and warning reduction.
        Extensions are defined in .vscode/extensions.json for team consistency.

    .PARAMETER All
        Install all recommended extensions (default behavior)

    .PARAMETER Essential
        Install only essential extensions for basic development

    .PARAMETER Advanced
        Install advanced extensions including AI tools and productivity enhancements

    .PARAMETER ListOnly
        List available extensions without installing

    .PARAMETER Force
        Force reinstall extensions even if already installed
    #>
    [CmdletBinding()]
    param(
        [switch]$All,
        [switch]$Essential,
        [switch]$Advanced,
        [switch]$ListOnly,
        [switch]$Force
    )

    Write-BusBuddyStatus "🔌 VS Code Extensions Manager for Bus Buddy" -Status Info
    Write-Host "══════════════════════════════════════════════════════════" -ForegroundColor DarkGray

    # Check if VS Code CLI is available
    $codeCommand = $null
    $codeCommands = @('code', 'code-insiders', 'codium')

    foreach ($cmd in $codeCommands) {
        if (Get-Command $cmd -ErrorAction SilentlyContinue) {
            $codeCommand = $cmd
            break
        }
    }

    if (-not $codeCommand) {
        Write-BusBuddyStatus "❌ VS Code CLI not found in PATH" -Status Error
        Write-Host "💡 Solutions:" -ForegroundColor Blue
        Write-Host "   • Install VS Code and add to PATH" -ForegroundColor Gray
        Write-Host "   • Run from VS Code integrated terminal" -ForegroundColor Gray
        Write-Host "   • Use 'code --help' to verify CLI availability" -ForegroundColor Gray
        return $false
    }

    Write-BusBuddyStatus "✅ Found VS Code CLI: $codeCommand" -Status Success

    # Define extension categories
    $extensionCategories = @{
        'Essential'         = @{
            'ms-dotnettools.csharp'           = '.NET C# Support - Core IntelliSense and debugging'
            'ms-vscode.powershell'            = 'PowerShell 7.5 support with advanced features'
            'ms-dotnettools.csdevkit'         = '.NET development kit with build/test integration'
            'eamodio.gitlens'                 = 'Git supercharged - history, blame, and GitHub integration'
            'spmeesseman.vscode-taskexplorer' = 'Task Explorer (EXCLUSIVE task management method)'
        }
        'Core Development'  = @{
            'ms-dotnettools.vscode-dotnet-runtime' = '.NET runtime management'
            'ms-dotnettools.xaml'                  = 'XAML language support for WPF'
            'josefpihrt-vscode.roslynator'         = 'Advanced C# code analysis and refactoring'
            'esbenp.prettier-vscode'               = 'Code formatter for consistency'
            'editorconfig.editorconfig'            = 'EditorConfig support for team consistency'
        }
        'Testing & Quality' = @{
            'ms-vscode.test-adapter-converter'      = 'Test adapter converter'
            'streetsidesoftware.code-spell-checker' = 'Spell checker for documentation'
            'aaron-bond.better-comments'            = 'Enhanced comment highlighting'
        }
        'AI & Productivity' = @{
            'github.copilot'               = 'GitHub Copilot AI assistance'
            'github.copilot-chat'          = 'GitHub Copilot Chat interface'
            'ms-vscode.powershell-preview' = 'PowerShell preview with PS 7.5.2 features'
        }
        'Azure & Cloud'     = @{
            'ms-vscode.azure-account'                  = 'Azure account management'
            'ms-azuretools.vscode-azureresourcegroups' = 'Azure resource management'
        }
        'Advanced Tools'    = @{
            'syncfusioninc.maui-vscode-extensions' = 'Syncfusion development support'
            'ms-vscode.hexeditor'                  = 'Hex editor for binary files'
            'ms-vscode-remote.remote-ssh'          = 'Remote development via SSH'
        }
    }

    # Determine which extensions to install
    $extensionsToInstall = @()

    if ($Essential) {
        $extensionsToInstall = $extensionCategories['Essential'].Keys
        Write-Host "📦 Installing Essential Extensions Only" -ForegroundColor Yellow
    }
    elseif ($Advanced) {
        $extensionsToInstall = $extensionCategories.Values | ForEach-Object { $_.Keys } | Sort-Object -Unique
        Write-Host "🚀 Installing All Advanced Extensions" -ForegroundColor Cyan
    }
    else {
        # Default: Core Development + Essential
        $extensionsToInstall = $extensionCategories['Essential'].Keys + $extensionCategories['Core Development'].Keys
        Write-Host "🔧 Installing Recommended Extensions (Essential + Core Development)" -ForegroundColor Green
    }

    if ($ListOnly) {
        Write-Host ""
        Write-Host "📋 Available Extension Categories:" -ForegroundColor Cyan

        foreach ($category in $extensionCategories.Keys) {
            Write-Host ""
            Write-Host "📂 $category" -ForegroundColor Magenta
            foreach ($ext in $extensionCategories[$category].GetEnumerator()) {
                $installed = & $codeCommand --list-extensions | Where-Object { $_ -eq $ext.Key }
                $status = if ($installed) { "✅" } else { "⬜" }
                Write-Host "   $status $($ext.Key)" -ForegroundColor White
                Write-Host "      $($ext.Value)" -ForegroundColor Gray
            }
        }
        return $true
    }

    # Install extensions
    Write-Host ""
    Write-Host "🔌 Installing VS Code Extensions..." -ForegroundColor Cyan
    Write-Host "Total extensions to process: $($extensionsToInstall.Count)" -ForegroundColor Gray

    $installResults = @{
        Installed        = @()
        Skipped          = @()
        Failed           = @()
        AlreadyInstalled = @()
    }

    foreach ($extensionId in $extensionsToInstall) {
        try {
            # Check if already installed
            $installed = & $codeCommand --list-extensions | Where-Object { $_ -eq $extensionId }

            if ($installed -and -not $Force) {
                Write-Host "⏭️  $extensionId (already installed)" -ForegroundColor DarkGray
                $installResults.AlreadyInstalled += $extensionId
                continue
            }

            Write-Host "📦 Installing $extensionId..." -ForegroundColor Yellow -NoNewline

            $installOutput = & $codeCommand --install-extension $extensionId --force 2>&1

            if ($LASTEXITCODE -eq 0) {
                Write-Host " ✅" -ForegroundColor Green
                $installResults.Installed += $extensionId
            }
            else {
                Write-Host " ❌" -ForegroundColor Red
                Write-Host "   Error: $installOutput" -ForegroundColor Red
                $installResults.Failed += @{ Extension = $extensionId; Error = $installOutput }
            }
        }
        catch {
            Write-Host " ❌ Exception" -ForegroundColor Red
            Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
            $installResults.Failed += @{ Extension = $extensionId; Error = $_.Exception.Message }
        }
    }

    # Display summary
    Write-Host ""
    Write-Host "📊 Installation Summary:" -ForegroundColor Cyan
    Write-Host "══════════════════════════════════════════════════════════" -ForegroundColor DarkGray
    Write-Host "✅ Newly Installed: $($installResults.Installed.Count)" -ForegroundColor Green
    Write-Host "⏭️  Already Installed: $($installResults.AlreadyInstalled.Count)" -ForegroundColor Gray
    Write-Host "❌ Failed: $($installResults.Failed.Count)" -ForegroundColor Red

    if ($installResults.Failed.Count -gt 0) {
        Write-Host ""
        Write-Host "❌ Failed Extensions:" -ForegroundColor Red
        foreach ($failure in $installResults.Failed) {
            Write-Host "   • $($failure.Extension): $($failure.Error)" -ForegroundColor Red
        }
    }

    if ($installResults.Installed.Count -gt 0) {
        Write-Host ""
        Write-Host "🔄 Restart VS Code to activate new extensions" -ForegroundColor Yellow
        Write-Host "💡 Run 'bb-health' after restart to validate environment" -ForegroundColor Blue
    }

    # Generate extensions report
    $projectRoot = Get-BusBuddyProjectRoot
    if ($projectRoot) {
        $reportPath = Join-Path $projectRoot "vscode-extensions-install-report.json"
        $report = @{
            Timestamp        = Get-Date
            VSCodeCommand    = $codeCommand
            InstallationType = if ($Essential) { 'Essential' } elseif ($Advanced) { 'Advanced' } else { 'Recommended' }
            Results          = $installResults
            NextSteps        = @(
                "Restart VS Code to activate extensions",
                "Run bb-health to validate development environment",
                "Check .vscode/extensions.json for team recommendations",
                "Use bb-dev-workflow to test integrated development cycle"
            )
        }

        try {
            $report | ConvertTo-Json -Depth 3 | Out-File $reportPath -Encoding UTF8
            Write-Host "📄 Installation report saved to: $reportPath" -ForegroundColor Blue
        }
        catch {
            Write-BusBuddyStatus "⚠️ Could not save report: $($_.Exception.Message)" -Status Warning
        }
    }

    $successRate = if ($extensionsToInstall.Count -gt 0) {
        [math]::Round((($installResults.Installed.Count + $installResults.AlreadyInstalled.Count) / $extensionsToInstall.Count) * 100, 1)
    }
    else { 100 }

    Write-Host ""
    if ($successRate -ge 90) {
        Write-BusBuddyStatus "🎉 Extensions setup completed successfully ($successRate% success rate)" -Status Success
    }
    elseif ($successRate -ge 70) {
        Write-BusBuddyStatus "⚠️ Extensions setup completed with some issues ($successRate% success rate)" -Status Warning
    }
    else {
        Write-BusBuddyStatus "❌ Extensions setup had significant issues ($successRate% success rate)" -Status Error
    }

    return $installResults
}

function Test-BusBuddyVSCodeSetup {
    <#
    .SYNOPSIS
        Validate VS Code setup for Bus Buddy development (bb-validate-vscode)

    .DESCRIPTION
        Comprehensive validation of VS Code environment including extensions,
        settings, and integration with Bus Buddy development workflow.

    .PARAMETER ShowDetails
        Show detailed information about each extension and setting

    .PARAMETER CheckTasks
        Validate VS Code task configuration

    .PARAMETER CheckSettings
        Validate VS Code settings configuration
    #>
    [CmdletBinding()]
    param(
        [switch]$ShowDetails,
        [switch]$CheckTasks,
        [switch]$CheckSettings
    )

    Write-BusBuddyStatus "🔍 VS Code Setup Validation for Bus Buddy" -Status Info
    Write-Host "══════════════════════════════════════════════════════════" -ForegroundColor DarkGray

    $validationResults = @{
        VSCodeAvailable     = $false
        ExtensionsInstalled = @()
        ExtensionsMissing   = @()
        TasksConfigured     = $false
        SettingsValid       = $false
        OverallScore        = 0
        Recommendations     = @()
    }

    # Check VS Code availability
    $codeCommand = $null
    $codeCommands = @('code', 'code-insiders', 'codium')

    foreach ($cmd in $codeCommands) {
        if (Get-Command $cmd -ErrorAction SilentlyContinue) {
            $codeCommand = $cmd
            $validationResults.VSCodeAvailable = $true
            break
        }
    }

    if ($validationResults.VSCodeAvailable) {
        Write-Host "✅ VS Code CLI Available: $codeCommand" -ForegroundColor Green
    }
    else {
        Write-Host "❌ VS Code CLI Not Found" -ForegroundColor Red
        $validationResults.Recommendations += "Install VS Code and ensure CLI is in PATH"
        return $validationResults
    }

    # Check essential extensions
    Write-Host ""
    Write-Host "🔌 Checking Essential Extensions..." -ForegroundColor Cyan

    $essentialExtensions = @{
        'ms-dotnettools.csharp'           = 'C# Development'
        'ms-vscode.powershell'            = 'PowerShell Support'
        'spmeesseman.vscode-taskexplorer' = 'Task Explorer (Required)'
        'eamodio.gitlens'                 = 'Git Integration'
        'ms-dotnettools.csdevkit'         = '.NET Development Kit'
    }

    try {
        $installedExtensions = & $codeCommand --list-extensions 2>&1

        foreach ($ext in $essentialExtensions.GetEnumerator()) {
            if ($installedExtensions -contains $ext.Key) {
                Write-Host "   ✅ $($ext.Value) ($($ext.Key))" -ForegroundColor Green
                $validationResults.ExtensionsInstalled += $ext.Key
            }
            else {
                Write-Host "   ❌ $($ext.Value) ($($ext.Key))" -ForegroundColor Red
                $validationResults.ExtensionsMissing += $ext.Key
                $validationResults.Recommendations += "Install missing extension: $($ext.Key)"
            }
        }
    }
    catch {
        Write-Host "   ⚠️ Could not check extensions: $($_.Exception.Message)" -ForegroundColor Yellow
        $validationResults.Recommendations += "Manually verify extension installation"
    }

    # Check .vscode configuration
    $projectRoot = Get-BusBuddyProjectRoot
    if ($projectRoot) {
        Write-Host ""
        Write-Host "⚙️ Checking VS Code Configuration..." -ForegroundColor Cyan

        # Check extensions.json
        $extensionsJsonPath = Join-Path $projectRoot ".vscode\extensions.json"
        if (Test-Path $extensionsJsonPath) {
            Write-Host "   ✅ .vscode/extensions.json exists" -ForegroundColor Green

            if ($ShowDetails) {
                try {
                    $extensionsConfig = Get-Content $extensionsJsonPath | ConvertFrom-Json
                    $recommendedCount = $extensionsConfig.recommendations.Count
                    Write-Host "      • Recommended extensions: $recommendedCount" -ForegroundColor Gray
                }
                catch {
                    Write-Host "      ⚠️ Could not parse extensions.json" -ForegroundColor Yellow
                }
            }
        }
        else {
            Write-Host "   ❌ .vscode/extensions.json missing" -ForegroundColor Red
            $validationResults.Recommendations += "Create .vscode/extensions.json for team consistency"
        }

        # Check tasks.json
        if ($CheckTasks) {
            $tasksJsonPath = Join-Path $projectRoot ".vscode\tasks.json"
            if (Test-Path $tasksJsonPath) {
                Write-Host "   ✅ .vscode/tasks.json exists" -ForegroundColor Green
                $validationResults.TasksConfigured = $true

                if ($ShowDetails) {
                    try {
                        $tasksConfig = Get-Content $tasksJsonPath | ConvertFrom-Json
                        $taskCount = $tasksConfig.tasks.Count
                        Write-Host "      • Configured tasks: $taskCount" -ForegroundColor Gray
                    }
                    catch {
                        Write-Host "      ⚠️ Could not parse tasks.json" -ForegroundColor Yellow
                    }
                }
            }
            else {
                Write-Host "   ⚠️ .vscode/tasks.json missing (optional)" -ForegroundColor Yellow
            }
        }

        # Check settings.json
        if ($CheckSettings) {
            $settingsJsonPath = Join-Path $projectRoot ".vscode\settings.json"
            if (Test-Path $settingsJsonPath) {
                Write-Host "   ✅ .vscode/settings.json exists" -ForegroundColor Green
                $validationResults.SettingsValid = $true

                if ($ShowDetails) {
                    try {
                        $settingsConfig = Get-Content $settingsJsonPath | ConvertFrom-Json
                        $settingCount = ($settingsConfig | Get-Member -MemberType NoteProperty).Count
                        Write-Host "      • Custom settings: $settingCount" -ForegroundColor Gray
                    }
                    catch {
                        Write-Host "      ⚠️ Could not parse settings.json" -ForegroundColor Yellow
                    }
                }
            }
            else {
                Write-Host "   ⚠️ .vscode/settings.json missing (optional)" -ForegroundColor Yellow
            }
        }
    }

    # Calculate overall score
    $maxScore = 100
    $currentScore = 0

    if ($validationResults.VSCodeAvailable) { $currentScore += 20 }
    $extensionScore = ($validationResults.ExtensionsInstalled.Count / $essentialExtensions.Count) * 50
    $currentScore += $extensionScore
    if ($validationResults.TasksConfigured) { $currentScore += 15 }
    if ($validationResults.SettingsValid) { $currentScore += 15 }

    $validationResults.OverallScore = [math]::Round(($currentScore / $maxScore) * 100, 1)

    # Display summary
    Write-Host ""
    Write-Host "📊 VS Code Setup Summary:" -ForegroundColor Cyan
    Write-Host "══════════════════════════════════════════════════════════" -ForegroundColor DarkGray
    Write-Host "Overall Score: $($validationResults.OverallScore)%" -ForegroundColor $(
        if ($validationResults.OverallScore -ge 80) { 'Green' }
        elseif ($validationResults.OverallScore -ge 60) { 'Yellow' }
        else { 'Red' }
    )
    Write-Host "Essential Extensions: $($validationResults.ExtensionsInstalled.Count)/$($essentialExtensions.Count) installed" -ForegroundColor $(
        if ($validationResults.ExtensionsMissing.Count -eq 0) { 'Green' } else { 'Yellow' }
    )

    if ($validationResults.Recommendations.Count -gt 0) {
        Write-Host ""
        Write-Host "💡 Recommendations:" -ForegroundColor Blue
        foreach ($rec in $validationResults.Recommendations) {
            Write-Host "   • $rec" -ForegroundColor Cyan
        }

        Write-Host ""
        Write-Host "🚀 Quick Fixes:" -ForegroundColor Yellow
        Write-Host "   bb-install-extensions          # Install missing extensions" -ForegroundColor Green
        Write-Host "   bb-install-extensions -ListOnly # See all available extensions" -ForegroundColor Green
        Write-Host "   bb-dev-workflow                # Test complete development cycle" -ForegroundColor Green
    }

    return $validationResults
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

    Write-Host "🤖 BusBuddy AI Mentor - $Topic Learning Assistant" -ForegroundColor Cyan
    Write-Host "=======================================" -ForegroundColor Cyan
    Write-Host ""

    # Enhanced topic routing with better documentation links
    switch ($Topic) {
        "PowerShell" {
            Write-Host "📘 PowerShell 7.5+ Learning Path:" -ForegroundColor Green
            Write-Host "   • Modern syntax improvements and performance enhancements"
            Write-Host "   • Enhanced JSON handling and REST API interactions"
            Write-Host "   • Cross-platform compatibility and containerization"
            if ($OpenDocs) {
                Start-Process "https://learn.microsoft.com/en-us/powershell/scripting/whats-new/what-s-new-in-powershell-75"
            }
            if ($IncludeExamples) {
                Write-Host "`n📝 Quick Example:" -ForegroundColor Yellow
                Write-Host "   # Modern PowerShell 7.5 features"
                Write-Host "   \$data = @{ Name='BusBuddy'; Version='2.0' } | ConvertTo-Json"
                Write-Host "   Get-Process | Where-Object CPU -gt 100 | Select-Object -First 5"
            }
        }
        "WPF" {
            Write-Host "🖥️ WPF & XAML Mastery:" -ForegroundColor Green
            Write-Host "   • MVVM pattern implementation and data binding"
            Write-Host "   • Custom controls and styling with modern UI patterns"
            Write-Host "   • Syncfusion component integration and theming"
            if ($OpenDocs) {
                Start-Process "https://learn.microsoft.com/en-us/dotnet/desktop/wpf/"
            }
            if (Test-Path "./Docs/Learning/WPF-Learning-Path.md") {
                Write-Host "   📚 Local guide: ./Docs/Learning/WPF-Learning-Path.md"
            }
        }
        "Getting Started" {
            Write-Host "🌟 Welcome to BusBuddy Development!" -ForegroundColor Green
            Write-Host "   1. Environment Setup: bb-health (check your development environment)"
            Write-Host "   2. First Build: bb-build (compile the solution)"
            Write-Host "   3. Run Application: bb-run (launch BusBuddy WPF)"
            Write-Host "   4. Testing: bb-test (run the test suite)"
            Write-Host "   5. Explore: bb-commands (see all available commands)"
            Write-Host ""
            Write-Host "📚 Learning Resources:" -ForegroundColor Cyan
            Write-Host "   • PowerShell: bb-mentor -Topic PowerShell"
            Write-Host "   • WPF Development: bb-mentor -Topic WPF"
            Write-Host "   • Architecture: bb-mentor -Topic MVVM"
        }
        default {
            Write-Host "📋 Available Topics:" -ForegroundColor Yellow
            Write-Host "   PowerShell, WPF, EntityFramework, Azure, MVVM, Testing, Git"
            Write-Host "`n💡 Usage: bb-mentor -Topic <TopicName> [-OpenDocs] [-IncludeExamples]"
            if (Test-Path "./Docs/Learning/$Topic.md") {
                Write-Host "📖 Found local guide: ./Docs/Learning/$Topic.md"
                if ($OpenDocs) { Start-Process "./Docs/Learning/$Topic.md" }
            }
        }
    }

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
        'PowerShell'      = @{
            'Summary'          = 'PowerShell is a powerful shell and scripting language for automation'
            'KeyConcepts'      = @('Variables', 'Functions', 'Modules', 'Pipeline', 'Objects')
            'OfficialDocs'     = 'https://learn.microsoft.com/en-us/powershell/'
            'Examples'         = @(
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

        'WPF'             = @{
            'Summary'          = 'Windows Presentation Foundation for rich desktop UI development'
            'KeyConcepts'      = @('XAML', 'Data Binding', 'Controls', 'Styles', 'Resources')
            'OfficialDocs'     = 'https://learn.microsoft.com/en-us/dotnet/desktop/wpf/'
            'Examples'         = @(
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
            'Summary'          = 'Welcome to BusBuddy development! Here is your roadmap.'
            'KeyConcepts'      = @('Setup', 'Build', 'Run', 'Debug', 'Learn')
            'OfficialDocs'     = 'https://github.com/Bigessfour/BusBuddy-2'
            'Examples'         = @(
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
            'PowerShell'      = "Start with basic commands like Get-Help, Get-Command, and Get-Member. Use tab completion!"
            'WPF'             = "Begin with simple XAML layouts and data binding. Visual Studio designer can help!"
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
        'PowerShell'      = 'https://learn.microsoft.com/en-us/powershell/'
        'WPF'             = 'https://learn.microsoft.com/en-us/dotnet/desktop/wpf/'
        'EntityFramework' = 'https://learn.microsoft.com/en-us/ef/core/'
        'Azure'           = 'https://learn.microsoft.com/en-us/azure/'
        'DotNet'          = 'https://learn.microsoft.com/en-us/dotnet/'
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
            'Syntax'   = @(
                '# PowerShell Syntax Reference',
                '$variable = "value"     # Variable assignment',
                'function Name { }       # Function declaration',
                'param([type]$name)      # Parameter declaration',
                'if ($condition) { }     # Conditional statement',
                'try { } catch { }       # Error handling'
            )
        }
        'BusBuddy'   = @{
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
        }
        elseif ($CheckMigrations) {
            & $scriptPath -CheckMigrations
        }
        else {
            & $scriptPath -FullDiagnostic
        }

        if ($LASTEXITCODE -eq 0) {
            Write-BusBuddyStatus "Database diagnostics completed successfully"
        }
        else {
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
        }
        else {
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
        }
        else {
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
        }
        else {
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

# Package management aliases
New-Alias -Name "bb-manage-dependencies" -Value "Invoke-BusBuddyDependencyManagement" -Description "Comprehensive dependency management"
New-Alias -Name "bb-error-fix" -Value "Invoke-BusBuddyErrorAnalysis" -Description "Analyze and fix build errors"

# Development workflow aliases
New-Alias -Name "bb-dev-workflow" -Value "Invoke-BusBuddyDevWorkflow" -Description "Comprehensive development workflow"
New-Alias -Name "bb-get-workflow-results" -Value "Get-BusBuddyWorkflowResults" -Description "Monitor GitHub Actions workflows"
New-Alias -Name "bb-warning-analysis" -Value "Show-BusBuddyWarningAnalysis" -Description "Analyze build warnings"

# VS Code integration aliases
New-Alias -Name "bb-install-extensions" -Value "Install-BusBuddyVSCodeExtensions" -Description "Install VS Code extensions"
New-Alias -Name "bb-validate-vscode" -Value "Test-BusBuddyVSCodeSetup" -Description "Validate VS Code setup"

#endregion

#region GitHub Automation Integration

# Load GitHub automation script
$githubScriptPath = Join-Path $PSScriptRoot "..\Tools\Scripts\GitHub\BusBuddy-GitHub-Automation.ps1"
if (Test-Path $githubScriptPath) {
    . $githubScriptPath
}

# Wrapper functions for Bus Buddy integration
function Invoke-BusBuddyGitHubStaging {
    <#
    .SYNOPSIS
        Smart Git staging with Bus Buddy integration (bb-github-stage)
    #>
    [CmdletBinding()]
    param([switch]$InteractiveMode, [switch]$UseExtensions)

    $projectRoot = Get-BusBuddyProjectRoot
    Push-Location $projectRoot
    try {
        if ($UseExtensions -and (Test-VSCodeExtension -ExtensionId "eamodio.gitlens")) {
            Write-BusBuddyStatus "🔗 GitLens extension detected" -Status Info
        }
        return Invoke-SmartGitStaging -InteractiveMode:$InteractiveMode
    }
    finally { Pop-Location }
}

function Invoke-BusBuddyGitHubCommit {
    <#
    .SYNOPSIS
        Intelligent commit creation with Bus Buddy standards (bb-github-commit)
    #>
    [CmdletBinding()]
    param([string[]]$StagedFiles, [switch]$GenerateMessage, [string]$CustomMessage, [switch]$UseExtensions)

    $projectRoot = Get-BusBuddyProjectRoot
    Push-Location $projectRoot
    try {
        return New-IntelligentCommit -StagedFiles $StagedFiles -GenerateMessage:$GenerateMessage -CustomMessage $CustomMessage
    }
    finally { Pop-Location }
}

function Invoke-BusBuddyGitHubPush {
    <#
    .SYNOPSIS
        Push and workflow monitoring with Bus Buddy integration (bb-github-push)
    #>
    [CmdletBinding()]
    param([switch]$WaitForCompletion, [switch]$UseExtensions, [int]$TimeoutMinutes = 30)

    $projectRoot = Get-BusBuddyProjectRoot
    Push-Location $projectRoot
    try {
        return Start-WorkflowRun -WaitForCompletion:$WaitForCompletion -TimeoutMinutes $TimeoutMinutes
    }
    finally { Pop-Location }
}

function Invoke-BusBuddyCompleteGitHubWorkflow {
    <#
    .SYNOPSIS
        Complete GitHub workflow automation (bb-github-workflow)
    #>
    [CmdletBinding()]
    param([switch]$GenerateCommitMessage, [switch]$WaitForCompletion, [switch]$AnalyzeResults, [switch]$AutoFix, [switch]$InteractiveMode, [string]$CommitMessage, [bool]$UseExtensions = $true)

    $projectRoot = Get-BusBuddyProjectRoot
    Push-Location $projectRoot
    try {
        # Check VS Code extensions from extensions.json
        if ($UseExtensions) {
            $extensions = @("eamodio.gitlens", "github.vscode-pull-request-github", "spmeesseman.vscode-taskexplorer", "github.copilot", "ms-vscode.powershell", "ms-dotnettools.csdevkit")
            foreach ($ext in $extensions) {
                if (Test-VSCodeExtension -ExtensionId $ext) {
                    Write-BusBuddyStatus "✅ Extension $ext active" -Status Success
                }
                else {
                    Write-BusBuddyStatus "⚠️ Extension $ext not found" -Status Warning
                }
            }
        }
        return Invoke-CompleteGitHubWorkflow -GenerateCommitMessage:$GenerateCommitMessage -WaitForCompletion:$WaitForCompletion -AnalyzeResults:$AnalyzeResults -AutoFix:$AutoFix -CommitMessage $CommitMessage
    }
    finally { Pop-Location }
}

function Test-VSCodeExtension {
    param([string]$ExtensionId)
    try {
        if (Get-Command code -ErrorAction SilentlyContinue) {
            $installedExtensions = & code --list-extensions 2>$null
            return ($installedExtensions -contains $ExtensionId)
        }
        return $false
    }
    catch { return $false }
}

# Create GitHub aliases
Set-Alias -Name 'bb-github-stage' -Value 'Invoke-BusBuddyGitHubStaging'
Set-Alias -Name 'bb-github-commit' -Value 'Invoke-BusBuddyGitHubCommit'
Set-Alias -Name 'bb-github-push' -Value 'Invoke-BusBuddyGitHubPush'
Set-Alias -Name 'bb-github-workflow' -Value 'Invoke-BusBuddyCompleteGitHubWorkflow'

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
    'Import-BusBuddyRealWorldData',

    # Package management functions
    'Invoke-BusBuddyDependencyManagement',
    'Invoke-BusBuddyErrorAnalysis',

    # Development workflow functions
    'Invoke-BusBuddyDevWorkflow',
    'Get-BusBuddyWorkflowResults',
    'Show-BusBuddyWarningAnalysis',

    # GitHub automation functions
    'Invoke-BusBuddyGitHubPush',
    'Invoke-BusBuddyGitHubCommit',
    'Invoke-BusBuddyGitHubStaging',
    'Invoke-BusBuddyCompleteGitHubWorkflow',

    # VS Code integration functions
    'Install-BusBuddyVSCodeExtensions',
    'Test-BusBuddyVSCodeSetup'
)

#region GitHub Automation Integration

# Load GitHub automation script
$githubScriptPath = Join-Path $PSScriptRoot "..\Tools\Scripts\GitHub\BusBuddy-GitHub-Automation.ps1"
if (Test-Path $githubScriptPath) {
    . $githubScriptPath
}

# Wrapper functions for Bus Buddy integration
function Invoke-BusBuddyGitHubStaging {
    <#
    .SYNOPSIS
        Smart Git staging with Bus Buddy integration (bb-github-stage)
    .DESCRIPTION
        Intelligently stages files for commit with interactive mode support.
        Integrates with VS Code extensions for enhanced workflow.
    #>
    [CmdletBinding()]
    param(
        [switch]$InteractiveMode,
        [switch]$UseExtensions
    )

    $projectRoot = Get-BusBuddyProjectRoot
    Push-Location $projectRoot
    try {
        Write-BusBuddyStatus "🎯 Starting smart Git staging..." -Status Info

        # Check for GitLens extension integration
        if ($UseExtensions -and (Test-VSCodeExtension -ExtensionId "eamodio.gitlens")) {
            Write-BusBuddyStatus "🔗 GitLens extension detected - enhanced Git integration available" -Status Info
        }

        $result = Invoke-SmartGitStaging -InteractiveMode:$InteractiveMode

        if ($result.Count -gt 0) {
            Write-BusBuddyStatus "✅ Successfully staged $($result.Count) files" -Status Success
        }

        return $result
    }
    finally {
        Pop-Location
    }
}

function Invoke-BusBuddyGitHubCommit {
    <#
    .SYNOPSIS
        Intelligent commit creation with Bus Buddy standards (bb-github-commit)
    .DESCRIPTION
        Creates intelligent commits following Bus Buddy conventions.
        Supports automatic message generation and VS Code extension integration.
    #>
    [CmdletBinding()]
    param(
        [string[]]$StagedFiles,
        [switch]$GenerateMessage,
        [string]$CustomMessage,
        [switch]$UseExtensions
    )

    $projectRoot = Get-BusBuddyProjectRoot
    Push-Location $projectRoot
    try {
        Write-BusBuddyStatus "💾 Creating intelligent commit..." -Status Info

        # Check for GitHub Pull Request extension
        if ($UseExtensions -and (Test-VSCodeExtension -ExtensionId "github.vscode-pull-request-github")) {
            Write-BusBuddyStatus "🔗 GitHub PR extension detected - enhanced GitHub integration available" -Status Info
        }

        $result = New-IntelligentCommit -StagedFiles $StagedFiles -GenerateMessage:$GenerateMessage -CustomMessage $CustomMessage

        if ($result) {
            Write-BusBuddyStatus "✅ Commit created: $($result.Hash)" -Status Success
        }

        return $result
    }
    finally {
        Pop-Location
    }
}

function Invoke-BusBuddyGitHubPush {
    <#
    .SYNOPSIS
        Push and workflow monitoring with Bus Buddy integration (bb-github-push)
    .DESCRIPTION
        Pushes changes to GitHub and monitors workflow execution.
        Integrates with Task Explorer for enhanced monitoring.
    #>
    [CmdletBinding()]
    param(
        [switch]$WaitForCompletion,
        [switch]$UseExtensions,
        [int]$TimeoutMinutes = 30
    )

    $projectRoot = Get-BusBuddyProjectRoot
    Push-Location $projectRoot
    try {
        Write-BusBuddyStatus "🚀 Pushing to GitHub with workflow monitoring..." -Status Info

        # Check for Task Explorer integration
        if ($UseExtensions -and (Test-VSCodeExtension -ExtensionId "spmeesseman.vscode-taskexplorer")) {
            Write-BusBuddyStatus "🔗 Task Explorer detected - can monitor via VS Code tasks" -Status Info
        }

        $result = Start-WorkflowRun -WaitForCompletion:$WaitForCompletion -TimeoutMinutes $TimeoutMinutes

        return $result
    }
    finally {
        Pop-Location
    }
}

function Invoke-BusBuddyCompleteGitHubWorkflow {
    <#
    .SYNOPSIS
        Complete GitHub workflow automation (bb-github-workflow)
    .DESCRIPTION
        Executes the complete GitHub workflow: stage → commit → push → monitor.
        Integrates with all recommended VS Code extensions for optimal experience.
    #>
    [CmdletBinding()]
    param(
        [switch]$GenerateCommitMessage,
        [switch]$WaitForCompletion,
        [switch]$AnalyzeResults,
        [switch]$AutoFix,
        [switch]$InteractiveMode,
        [string]$CommitMessage,
        [bool]$UseExtensions = $true
    )

    $projectRoot = Get-BusBuddyProjectRoot
    Push-Location $projectRoot
    try {
        Write-BusBuddyStatus "🎯 Starting complete GitHub workflow automation..." -Status Info

        # Check and report extension status for key extensions from extensions.json
        if ($UseExtensions) {
            $extensions = @(
                @{ Name = "GitLens"; Id = "eamodio.gitlens" },
                @{ Name = "GitHub Pull Requests"; Id = "github.vscode-pull-request-github" },
                @{ Name = "Task Explorer"; Id = "spmeesseman.vscode-taskexplorer" },
                @{ Name = "GitHub Copilot"; Id = "github.copilot" },
                @{ Name = "PowerShell"; Id = "ms-vscode.powershell" },
                @{ Name = "C# DevKit"; Id = "ms-dotnettools.csdevkit" },
                @{ Name = "Roslynator"; Id = "josefpihrt-vscode.roslynator" }
            )

            foreach ($ext in $extensions) {
                if (Test-VSCodeExtension -ExtensionId $ext.Id) {
                    Write-BusBuddyStatus "✅ $($ext.Name) extension active" -Status Success
                }
                else {
                    Write-BusBuddyStatus "⚠️ $($ext.Name) extension not found - install for enhanced workflow" -Status Warning
                }
            }
        }

        $result = Invoke-CompleteGitHubWorkflow -GenerateCommitMessage:$GenerateCommitMessage -WaitForCompletion:$WaitForCompletion -AnalyzeResults:$AnalyzeResults -AutoFix:$AutoFix -CommitMessage $CommitMessage

        Write-BusBuddyStatus "🎉 Complete GitHub workflow finished!" -Status Success
        return $result
    }
    finally {
        Pop-Location
    }
}

# Helper function to test VS Code extensions
function Test-VSCodeExtension {
    param([string]$ExtensionId)

    try {
        if (Get-Command code -ErrorAction SilentlyContinue) {
            $installedExtensions = & code --list-extensions 2>$null
            return ($installedExtensions -contains $ExtensionId)
        }
        return $false
    }
    catch {
        return $false
    }
}

# Create aliases for GitHub functions
Set-Alias -Name 'bb-github-stage' -Value 'Invoke-BusBuddyGitHubStaging'
Set-Alias -Name 'bb-github-commit' -Value 'Invoke-BusBuddyGitHubCommit'
Set-Alias -Name 'bb-github-push' -Value 'Invoke-BusBuddyGitHubPush'
Set-Alias -Name 'bb-github-workflow' -Value 'Invoke-BusBuddyCompleteGitHubWorkflow'

#endregion

# Export all aliases
Export-ModuleMember -Alias @(
    'bb-build', 'bb-run', 'bb-test', 'bb-clean', 'bb-restore',
    'bb-dev-session', 'bb-health', 'bb-env-check',
    'bb-happiness', 'bb-commands', 'bb-info',
    'bb-mentor', 'bb-docs', 'bb-ref',
    'bb-db-diag', 'bb-db-test', 'bb-db-add-migration', 'bb-db-update', 'bb-db-seed',
    'bb-manage-dependencies', 'bb-error-fix',
    'bb-dev-workflow', 'bb-get-workflow-results', 'bb-warning-analysis',
    'bb-install-extensions', 'bb-validate-vscode',
    'bb-github-push', 'bb-github-commit', 'bb-github-stage', 'bb-github-workflow'
)

#endregion
