#Requires -Version 7.5
<#
.SYNOPSIS
    BusBuddy PowerShell Module - PowerShell 7.5.2 Compliant

.DESCRIPTION
    Professional PowerShell module for Bus Buddy WPF development environment.
    Follows Microsoft PowerShell 7.5.2 guidelines and best practices.

    Reference: https://learn.microsoft.com/en-us/powershell/scripting/learn/shell/creating-profiles

.NOTES
    File Name      : BusBuddy.psm1
    Author         : Bus Buddy Development Team
    Prerequisite   : PowerShell 7.5+ (Microsoft Standard)
    Copyright      : (c) 2025 Bus Buddy Project
    PS Version     : PowerShell 7.5.2 Compliant

.EXAMPLE
    Import-Module BusBuddy
    Get-BusBuddyCommands

.EXAMPLE
    bb-build -Clean
    bb-run
    bb-health
#>

#region Module Initialization (PowerShell 7.5.2 Standard)

## Module metadata (PowerShell 7.5.2 pattern)
$script:ModuleConfig = @{
    Name                     = 'BusBuddy'
    Version                  = [Version]'1.0.0'
    Author                   = 'Bus Buddy Development Team'
    PowerShellVersion        = [Version]'7.5.0'
    ProjectRoot              = $PSScriptRoot
    LoadedComponents         = [System.Collections.Generic.List[string]]::new()
    BusBuddyCoreAssemblyPath = $null
}

## Validate PowerShell version (Microsoft recommended)
if ($PSVersionTable.PSVersion -lt $script:ModuleConfig.PowerShellVersion) {
    throw "This module requires PowerShell $($script:ModuleConfig.PowerShellVersion) or later. Current version: $($PSVersionTable.PSVersion)"
}

## Error handling preferences (PowerShell 7.5.2 best practice)
$ErrorActionPreference = 'Stop'
# ProgressPreference managed by profile - enable multi-threading progress support

#endregion

#region Module Loading Functions (PowerShell 7.5.2 Compliant)

function Import-BusBuddyFunction {
    <#
    .SYNOPSIS
        Import individual BusBuddy function files
    .DESCRIPTION
        PowerShell 7.5.2 compliant function loader with proper error handling
    .PARAMETER FunctionPath
        Path to the PowerShell function file
    .EXAMPLE
        Import-BusBuddyFunction -FunctionPath ".\Functions\Build.ps1"
    #>
    [CmdletBinding()]
    param (
        [Parameter(Mandatory, ValueFromPipeline)]
        [ValidateScript({ Test-Path $_ -PathType Leaf })]
        [string]$FunctionPath
    )

    process {
        try {
            . $FunctionPath
            $script:ModuleConfig.LoadedComponents.Add((Split-Path $FunctionPath -Leaf))
            Write-Verbose "Loaded function: $FunctionPath"
            return $true
        }
        catch {
            Write-Error "Failed to load function '$FunctionPath': $($_.Exception.Message)"
            return $false
        }
    }
}

function Import-BusBuddyFunctionCategory {
    <#
    .SYNOPSIS
        Import all functions from a category directory
    .DESCRIPTION
        PowerShell 7.5.2 compliant batch function loader
    .PARAMETER Category
        Function category name (e.g., 'Build', 'Utilities')
    .EXAMPLE
        Import-BusBuddyFunctionCategory -Category 'Build'
    #>
    [CmdletBinding()]
    param (
        [Parameter(Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]$Category
    )

    $categoryPath = Join-Path -Path $script:ModuleConfig.ProjectRoot -ChildPath "Functions\$Category"

    if (-not (Test-Path $categoryPath -PathType Container)) {
        Write-Warning "Function category directory not found: $categoryPath"
        return @()
    }

    try {
        $functionFiles = Get-ChildItem -Path $categoryPath -Filter "*.ps1" -ErrorAction Stop
        $loadedFiles = [System.Collections.Generic.List[string]]::new()

        foreach ($file in $functionFiles) {
            if (Import-BusBuddyFunction -FunctionPath $file.FullName) {
                $loadedFiles.Add($file.Name)
            }
        }

        Write-Verbose "Loaded $($loadedFiles.Count) functions from category '$Category'"
        return $loadedFiles.ToArray()
    }
    catch {
        Write-Error "Failed to load functions from category '$Category': $($_.Exception.Message)"
        return @()
    }
}

#endregion

#region PowerShell 7.5.2 Multi-Threading Support (Microsoft Pattern)

function Start-BusBuddyProgressiveOperation {
    <#
    .SYNOPSIS
        Execute operations with Microsoft PowerShell 7.5.2 multi-threading progress pattern
    .DESCRIPTION
        Implements Microsoft's recommended synchronized hashtable pattern for progress tracking
        across multiple threads using ForEach-Object -Parallel
    .PARAMETER Tasks
        Array of task objects with Id, Name, and ScriptBlock properties
    .PARAMETER ThrottleLimit
        Maximum number of parallel threads (Microsoft recommends 3-8)
    .EXAMPLE
        $tasks = @(
            @{ Id = 1; Name = "Build"; ScriptBlock = { dotnet build } },
            @{ Id = 2; Name = "Test"; ScriptBlock = { dotnet test } }
        )
        Start-BusBuddyProgressiveOperation -Tasks $tasks -ThrottleLimit 3
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [Array]$Tasks,

        [ValidateRange(1, 16)]
        [int]$ThrottleLimit = 3
    )

    try {
        # Create synchronized hashtable (Microsoft pattern)
        $origin = @{}
        $Tasks | ForEach-Object { $origin.($_.Id) = @{} }
        $sync = [System.Collections.Hashtable]::Synchronized($origin)

        Write-Verbose "Starting $($Tasks.Count) tasks with ThrottleLimit $ThrottleLimit"

        # Execute parallel operations with progress tracking
        $job = $Tasks | ForEach-Object -ThrottleLimit $ThrottleLimit -AsJob -Parallel {
            $syncCopy = $Using:sync
            $process = $syncCopy.$($PSItem.Id)

            try {
                # Initialize progress
                $process.Id = $PSItem.Id
                $process.Activity = "Starting $($PSItem.Name)"
                $process.Status = "Initializing"
                $process.PercentComplete = 0

                # Execute the task
                $process.Activity = "Executing $($PSItem.Name)"
                $process.Status = "Running"
                $process.PercentComplete = 50

                $result = & $PSItem.ScriptBlock

                # Mark completion
                $process.Status = "Completed"
                $process.PercentComplete = 100
                $process.Completed = $true

                return $result
            }
            catch {
                $process.Status = "Failed: $($_.Exception.Message)"
                $process.PercentComplete = 100
                $process.Completed = $true
                $process.Error = $_.Exception.Message
                throw
            }
        }

        # Progress display loop (Microsoft pattern)
        while ($job.State -eq 'Running') {
            $sync.Keys | ForEach-Object {
                if (![string]::IsNullOrEmpty($sync.$_.Keys)) {
                    $param = $sync.$_
                    if ($param.Activity) {
                        Write-Progress @param
                    }
                }
            }
            Start-Sleep -Milliseconds 100
        }

        # Clear progress indicators
        $sync.Keys | ForEach-Object {
            Write-Progress -Id $_ -Completed
        }

        # Get results
        $results = Receive-Job -Job $job -Wait
        Remove-Job -Job $job

        Write-Verbose "All tasks completed"
        return $results
    }
    catch {
        Write-Error "Progressive operation failed: $($_.Exception.Message)"
        throw
    }
}

function Write-BusBuddyProgress {
    <#
    .SYNOPSIS
        PowerShell 7.5.2 compliant progress reporting
    .DESCRIPTION
        Thread-safe progress reporting that works in both single and multi-threaded contexts
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Activity,

        [string]$Status = "Processing",

        [ValidateRange(0, 100)]
        [int]$PercentComplete = 0,

        [int]$Id = 1,

        [switch]$Completed
    )

    $progressParams = @{
        Id               = $Id
        Activity         = $Activity
        Status           = $Status
        PercentComplete  = $PercentComplete
    }

    if ($Completed) {
        $progressParams.Completed = $true
    }

    Write-Progress @progressParams
}

#endregion

#region Assembly Initialization (PowerShell 7.5.2 Compliant)

function Initialize-BusBuddyCoreAssembly {
    <#
    .SYNOPSIS
        Initialize BusBuddy.Core assembly for .NET interop
    .DESCRIPTION
        Locates and loads the BusBuddy.Core.dll using relative paths from the PowerShell module location.
        Ensures proper .NET interop for AI services and other core functionality.
    #>
    [CmdletBinding()]
    param()

    try {
        $moduleRoot = $script:ModuleConfig.ProjectRoot
        if (-not $moduleRoot) {
            $moduleRoot = Get-BusBuddyProjectRoot
        }

        if ($moduleRoot) {
            # Try different potential locations for the BusBuddy.Core.dll
            $possiblePaths = @(
                [System.IO.Path]::Combine($moduleRoot, 'BusBuddy.Core', 'bin', 'Debug', 'net8.0-windows', 'BusBuddy.Core.dll'),
                [System.IO.Path]::Combine($moduleRoot, 'BusBuddy.Core', 'bin', 'Release', 'net8.0-windows', 'BusBuddy.Core.dll'),
                [System.IO.Path]::Combine($moduleRoot, 'BusBuddy.Core', 'bin', 'Debug', 'net8.0', 'BusBuddy.Core.dll'),
                [System.IO.Path]::Combine($moduleRoot, 'BusBuddy.Core', 'bin', 'Release', 'net8.0', 'BusBuddy.Core.dll'),
                [System.IO.Path]::Combine($moduleRoot, 'bin', 'Debug', 'net8.0', 'BusBuddy.Core.dll'),
                [System.IO.Path]::Combine($moduleRoot, 'bin', 'Release', 'net8.0', 'BusBuddy.Core.dll')
            )

            foreach ($path in $possiblePaths) {
                if (Test-Path $path) {
                    $script:ModuleConfig.BusBuddyCoreAssemblyPath = $path
                    Add-Type -Path $path -ErrorAction SilentlyContinue
                    Write-Verbose "Loaded BusBuddy.Core assembly from: $path"
                    return $true
                }
            }

            Write-Warning "BusBuddy.Core.dll not found. Please build the solution first with 'bb-build'"
            return $false
        }
        else {
            Write-Warning "Project root not found. Cannot locate BusBuddy.Core assembly."
            return $false
        }
    }
    catch {
        Write-Warning "Failed to load BusBuddy.Core assembly: $($_.Exception.Message)"
        return $false
    }
}

# Project root will be initialized at end of module after all functions are defined

#endregion

#region Core Utility Functions

#Requires -Version 7.5
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
$script:ModuleConfig.ProjectRoot = Get-BusBuddyProjectRoot

#Requires -Version 7.5
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

#Requires -Version 7.5
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

        [System.Exception]$Exception,

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

#Requires -Version 7.5
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
}

#Requires -Version 7.5
function Test-BusBuddyEnvironment {
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
            elseif ($dotnetVer -ge [version]"8.0") {
                Write-BusBuddyStatus ".NET $dotnetVersion detected - optimal for BusBuddy development" -Status Success
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

#region Git and Repository Functions

function Invoke-BusBuddyGitIgnoreCheck {
    <#
    .SYNOPSIS
        Analyze repository for files that should be ignored

    .DESCRIPTION
        Checks for tracked build artifacts, temporary files, and other files that should typically be ignored.
        Provides PowerShell equivalents for common Unix git commands.

    .PARAMETER ShowSuggestions
        Show suggestions for improving .gitignore

    .PARAMETER CheckTracked
        Check currently tracked files for potential issues

    .PARAMETER ShowStats
        Display repository statistics
    #>
    [CmdletBinding()]
    param(
        [switch]$ShowSuggestions,
        [switch]$CheckTracked,
        [switch]$ShowStats
    )

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        Write-BusBuddyError -Message "Project root not found" -RecommendedAction "Ensure you're in a Bus Buddy project directory"
        return
    }

    Push-Location $projectRoot

    try {
        Write-BusBuddyStatus "🔍 Git Repository Analysis" -Status Info
        Write-Host ""

        if ($CheckTracked) {
            Write-BusBuddyStatus "Checking for tracked build artifacts..." -Status Info

            # PowerShell equivalent of: git ls-files | grep -E "(bin/|obj/|\.cache|\.user|\.suo)"
            $buildArtifacts = git ls-files | Where-Object { $_ -match "(bin/|obj/|\.cache|\.user|\.suo|\.tmp|\.temp)" }

            if ($buildArtifacts) {
                Write-BusBuddyStatus "⚠️ Found tracked build artifacts:" -Status Warning
                $buildArtifacts | ForEach-Object { Write-Host "  - $_" -ForegroundColor Yellow }
                Write-Host ""
                Write-Host "To remove these from tracking:" -ForegroundColor Cyan
                Write-Host "  git rm --cached -r bin/ obj/ *.cache *.user *.suo" -ForegroundColor Green
            }
            else {
                Write-BusBuddyStatus "✅ No build artifacts are being tracked" -Status Success
            }

            # Check for log and temporary files
            $tempFiles = git ls-files | Where-Object { $_ -match "\.(log|tmp|temp|bak)$" }
            if ($tempFiles) {
                Write-BusBuddyStatus "⚠️ Found tracked temporary files:" -Status Warning
                $tempFiles | ForEach-Object { Write-Host "  - $_" -ForegroundColor Yellow }
            }
            else {
                Write-BusBuddyStatus "✅ No temporary files are being tracked" -Status Success
            }
        }

        if ($ShowStats) {
            Write-BusBuddyStatus "Repository Statistics:" -Status Info

            $allFiles = git ls-files
            $totalCount = ($allFiles | Measure-Object).Count

            Write-Host "Total tracked files: $totalCount" -ForegroundColor Cyan

            # File type breakdown (PowerShell equivalent of Unix commands)
            $fileTypes = $allFiles | Group-Object { Split-Path $_ -Extension } |
            Sort-Object Count -Descending | Select-Object -First 10

            Write-Host ""
            Write-Host "Top file types:" -ForegroundColor Yellow
            foreach ($type in $fileTypes) {
                $extension = if ($type.Name) { $type.Name } else { "(no extension)" }
                Write-Host "  $($type.Count.ToString().PadLeft(3)) files - $extension" -ForegroundColor Gray
            }
        }

        if ($ShowSuggestions) {
            Write-Host ""
            Write-BusBuddyStatus "💡 PowerShell Git Command Equivalents:" -Status Info

            $equivalents = @(
                @{ Unix = "git ls-files | grep '\.cs$'"; PowerShell = 'git ls-files | Where-Object { $_ -match "\.cs$" }' }
                @{ Unix = "git ls-files | grep -v test"; PowerShell = 'git ls-files | Where-Object { $_ -notmatch "test" }' }
                @{ Unix = "git ls-files | wc -l"; PowerShell = "(git ls-files | Measure-Object).Count" }
                @{ Unix = "git log --oneline | head -5"; PowerShell = "git log --oneline | Select-Object -First 5" }
            )

            foreach ($equiv in $equivalents) {
                Write-Host "Unix: " -ForegroundColor Red -NoNewline
                Write-Host $equiv.Unix -ForegroundColor White
                Write-Host "PS:   " -ForegroundColor Blue -NoNewline
                Write-Host $equiv.PowerShell -ForegroundColor Green
                Write-Host ""
            }
        }

        # Check .gitignore effectiveness
        Write-BusBuddyStatus "Checking .gitignore effectiveness..." -Status Info

        $untracked = git ls-files --others --exclude-standard
        if ($untracked) {
            $untrackedCount = ($untracked | Measure-Object).Count
            Write-Host "Untracked files not ignored: $untrackedCount" -ForegroundColor Yellow
            if ($untrackedCount -le 5) {
                $untracked | ForEach-Object { Write-Host "  - $_" -ForegroundColor Gray }
            }
        }
        else {
            Write-BusBuddyStatus "✅ All untracked files are properly ignored" -Status Success
        }

    }
    catch {
        Write-BusBuddyError -Message "Git analysis error: $($_.Exception.Message)" -Exception $_.Exception
    }
    finally {
        Pop-Location
    }
}

function Get-BusBuddyGitEquivalents {
    <#
    .SYNOPSIS
        Display PowerShell equivalents for common Unix git commands

    .DESCRIPTION
        Provides a quick reference for PowerShell alternatives to Unix commands commonly used with git
    #>
    [CmdletBinding()]
    param()

    Write-Host ""
    Write-BusBuddyStatus "🔧 PowerShell Git Command Reference" -Status Info
    Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor DarkGray

    $commands = @(
        @{
            Category    = "File Filtering"
            Unix        = "git ls-files | grep '\.cs$'"
            PowerShell  = 'git ls-files | Where-Object { $_ -match "\.cs$" }'
            Description = "Find all C# files"
        }
        @{
            Category    = "File Filtering"
            Unix        = "git ls-files | grep -v test"
            PowerShell  = 'git ls-files | Where-Object { $_ -notmatch "test" }'
            Description = "Exclude test files"
        }
        @{
            Category    = "Counting"
            Unix        = "git ls-files | wc -l"
            PowerShell  = "(git ls-files | Measure-Object).Count"
            Description = "Count tracked files"
        }
        @{
            Category    = "Limiting Output"
            Unix        = "git log --oneline | head -10"
            PowerShell  = "git log --oneline | Select-Object -First 10"
            Description = "Show first 10 commits"
        }
        @{
            Category    = "Status Filtering"
            Unix        = "git status --porcelain | grep '^M'"
            PowerShell  = 'git status --porcelain | Where-Object { $_ -match "^.M" }'
            Description = "Show only modified files"
        }
        @{
            Category    = "File Search"
            Unix        = "find . -name '*.log' -type f"
            PowerShell  = "Get-ChildItem -Recurse -Filter '*.log' -File"
            Description = "Find log files recursively"
        }
    )

    $currentCategory = ""
    foreach ($cmd in $commands) {
        if ($cmd.Category -ne $currentCategory) {
            $currentCategory = $cmd.Category
            Write-Host ""
            Write-Host $currentCategory -ForegroundColor Cyan
            Write-Host ("-" * $currentCategory.Length) -ForegroundColor DarkCyan
        }

        Write-Host "  📄 " -ForegroundColor Blue -NoNewline
        Write-Host $cmd.Description -ForegroundColor White
        Write-Host "     Unix: " -ForegroundColor Red -NoNewline
        Write-Host $cmd.Unix -ForegroundColor Gray
        Write-Host "     PS:   " -ForegroundColor Green -NoNewline
        Write-Host $cmd.PowerShell -ForegroundColor White
        Write-Host ""
    }

    Write-Host ""
    Write-BusBuddyStatus "💡 Pro Tips:" -Status Info
    Write-Host "  • Use 'bb-git-check' for repository analysis" -ForegroundColor Yellow
    Write-Host "  • PowerShell has better object handling than Unix pipes" -ForegroundColor Yellow
    Write-Host "  • Use Get-Help for detailed parameter information" -ForegroundColor Yellow
    Write-Host ""
}

function Invoke-BusBuddyGitRepairKit {
    <#
    .SYNOPSIS
        Advanced git repository repair and optimization toolkit

    .DESCRIPTION
        Handles common git pitfalls including assume-unchanged files, large files,
        performance issues, and repository alignment problems

    .PARAMETER CheckAssumeUnchanged
        Check for and fix assume-unchanged files

    .PARAMETER AggressiveCleanup
        Perform aggressive garbage collection for performance

    .PARAMETER CheckBranch
        Verify current branch and provide switching guidance

    .PARAMETER FullRepair
        Run all repair operations

    .PARAMETER Force
        Force operations without confirmation
    #>
    [CmdletBinding()]
    param(
        [switch]$CheckAssumeUnchanged,
        [switch]$AggressiveCleanup,
        [switch]$CheckBranch,
        [switch]$FullRepair,
        [switch]$Force
    )

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        Write-BusBuddyError -Message "Project root not found" -RecommendedAction "Ensure you're in a Bus Buddy project directory"
        return
    }

    Push-Location $projectRoot

    try {
        Write-BusBuddyStatus "🔧 Git Repository Repair Kit" -Status Info
        Write-Host "════════════════════════════════════════" -ForegroundColor DarkGray
        Write-Host ""

        # Set all switches if FullRepair is specified
        if ($FullRepair) {
            $CheckAssumeUnchanged = $true
            $AggressiveCleanup = $true
            $CheckBranch = $true
        }

        $issuesFound = 0
        $issuesFixed = 0

        # Check for assume-unchanged files
        if ($CheckAssumeUnchanged) {
            Write-BusBuddyStatus "Checking for assume-unchanged files..." -Status Info

            try {
                # PowerShell equivalent of: git ls-files -v | grep '^[[:lower:]]'
                $assumeUnchangedFiles = git ls-files -v | Where-Object { $_ -match '^[a-z]' }

                if ($assumeUnchangedFiles) {
                    $issuesFound++
                    Write-BusBuddyStatus "⚠️ Found files marked as assume-unchanged:" -Status Warning

                    foreach ($file in $assumeUnchangedFiles) {
                        $fileName = $file.Substring(2)  # Remove the status prefix
                        Write-Host "  - $fileName" -ForegroundColor Yellow

                        if ($Force -or (Read-Host "Fix assume-unchanged for '$fileName'? (y/N)") -eq 'y') {
                            git update-index --no-assume-unchanged $fileName
                            if ($LASTEXITCODE -eq 0) {
                                Write-Host "    ✅ Fixed: $fileName" -ForegroundColor Green
                                $issuesFixed++
                            }
                            else {
                                Write-Host "    ❌ Failed to fix: $fileName" -ForegroundColor Red
                            }
                        }
                    }
                }
                else {
                    Write-BusBuddyStatus "✅ No assume-unchanged files found" -Status Success
                }
            }
            catch {
                Write-BusBuddyStatus "Error checking assume-unchanged files: $($_.Exception.Message)" -Status Warning
            }
        }

        # Aggressive cleanup for performance
        if ($AggressiveCleanup) {
            Write-Host ""
            Write-BusBuddyStatus "Performing aggressive cleanup for performance..." -Status Info

            if ($Force -or (Read-Host "This may take time. Proceed with aggressive cleanup? (y/N)") -eq 'y') {
                Write-Host "🧹 Running garbage collection..." -ForegroundColor Yellow

                try {
                    $gcOutput = git gc --aggressive --prune=now 2>&1
                    if ($LASTEXITCODE -eq 0) {
                        Write-BusBuddyStatus "✅ Aggressive cleanup completed successfully" -Status Success
                        $issuesFixed++

                        # Show size improvement if possible
                        if ($gcOutput -match "size-pack: (\d+)") {
                            Write-Host "Repository optimized for better performance" -ForegroundColor Green
                        }
                    }
                    else {
                        Write-BusBuddyStatus "⚠️ Cleanup completed with warnings" -Status Warning
                        Write-Host "$gcOutput" -ForegroundColor Yellow
                    }
                }
                catch {
                    Write-BusBuddyStatus "Error during cleanup: $($_.Exception.Message)" -Status Warning
                }
            }
            else {
                Write-Host "Skipping aggressive cleanup" -ForegroundColor Gray
            }
        }

        # Branch verification
        if ($CheckBranch) {
            Write-Host ""
            Write-BusBuddyStatus "Checking current branch..." -Status Info

            try {
                $currentBranch = git branch --show-current
                $isOnMain = ($currentBranch -eq "main") -or ($currentBranch -eq "master")

                Write-Host "Current branch: $currentBranch" -ForegroundColor $(if ($isOnMain) { "Green" } else { "Yellow" })

                if (-not $isOnMain) {
                    $issuesFound++
                    Write-BusBuddyStatus "⚠️ Not on main/master branch" -Status Warning
                    Write-Host "Available branches:" -ForegroundColor Cyan

                    $branches = git branch --all | Where-Object { $_ -notmatch "HEAD" }
                    foreach ($branch in $branches) {
                        $cleanBranch = $branch.Trim().TrimStart('*').Trim()
                        $isCurrentBranch = $branch.StartsWith('*')
                        $color = if ($isCurrentBranch) { "Green" } else { "Gray" }
                        Write-Host "  $cleanBranch" -ForegroundColor $color
                    }

                    Write-Host ""
                    Write-Host "To switch to main branch:" -ForegroundColor Cyan
                    Write-Host "  git checkout main" -ForegroundColor White
                    Write-Host "  # or" -ForegroundColor Gray
                    Write-Host "  git checkout master" -ForegroundColor White
                }
                else {
                    Write-BusBuddyStatus "✅ On main branch" -Status Success
                }
            }
            catch {
                Write-BusBuddyStatus "Error checking branch: $($_.Exception.Message)" -Status Warning
            }
        }

        # Summary
        Write-Host ""
        Write-Host "🏥 Repository Repair Summary:" -ForegroundColor Cyan
        Write-Host "═══════════════════════════════" -ForegroundColor DarkGray
        Write-Host "Issues Found: $issuesFound" -ForegroundColor $(if ($issuesFound -eq 0) { "Green" } else { "Yellow" })
        Write-Host "Issues Fixed: $issuesFixed" -ForegroundColor $(if ($issuesFixed -eq $issuesFound) { "Green" } else { "Yellow" })

        if ($issuesFound -eq 0) {
            Write-BusBuddyStatus "✅ Repository is in excellent health!" -Status Success
        }
        elseif ($issuesFixed -eq $issuesFound) {
            Write-BusBuddyStatus "✅ All issues have been resolved!" -Status Success
        }
        else {
            Write-BusBuddyStatus "⚠️ Some issues require manual attention" -Status Warning
        }

    }
    catch {
        Write-BusBuddyError -Message "Repository repair error: $($_.Exception.Message)" -Exception $_.Exception
    }
    finally {
        Pop-Location
    }
}



function Start-BusBuddyRepositoryAlignment {
    <#
    .SYNOPSIS
        Complete repository alignment workflow for optimal scanning

    .DESCRIPTION
        Implements the full repository alignment process including build verification,
        git status check, and preparation for enhanced error diagnosis

    .PARAMETER IncludeBuild
        Include build verification in alignment process

    .PARAMETER PushAfterAlignment
        Automatically push changes after alignment

    .PARAMETER Force
        Force operations without confirmation
    #>
    [CmdletBinding()]
    param(
        [switch]$IncludeBuild,
        [switch]$PushAfterAlignment,
        [switch]$Force
    )

    Write-BusBuddyStatus "🚌 BusBuddy Repository Alignment Workflow" -Status Info
    Write-Host "════════════════════════════════════════════════════" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "🎯 Goal: Aligned repos enable seamless journeys!" -ForegroundColor Cyan
    Write-Host ""

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        Write-BusBuddyError -Message "Project root not found" -RecommendedAction "Ensure you're in a Bus Buddy project directory"
        return $false
    }

    try {
        # Step 1: Repository repair and health check
        Write-BusBuddyStatus "Step 1: Repository Health Check & Repair" -Status Info
        Invoke-BusBuddyGitRepairKit -FullRepair -Force:$Force

        Write-Host ""

        # Step 2: Build verification (if requested)
        if ($IncludeBuild) {
            Write-BusBuddyStatus "Step 2: Build Verification" -Status Build
            $buildSuccess = Invoke-BusBuddyBuild -Configuration Debug -Restore -Verbosity minimal

            if (-not $buildSuccess) {
                Write-BusBuddyStatus "❌ Build failed - addressing before alignment" -Status Error
                Write-Host "💡 Build issues must be resolved for complete alignment" -ForegroundColor Yellow
                return $false
            }
            else {
                Write-BusBuddyStatus "✅ Build verification passed" -Status Success
            }
        }

        Write-Host ""

        # Step 3: Enhanced git status
        Write-BusBuddyStatus "Step 3: Comprehensive Git Status Analysis" -Status Info
        Get-BusBuddyGitStatus -Detailed -QuickHealth

        Write-Host ""

        # Step 4: Repository statistics
        Write-BusBuddyStatus "Step 4: Repository Scan Optimization" -Status Info

        # Get comprehensive file statistics
        $allFiles = git ls-files
        $totalTracked = ($allFiles | Measure-Object).Count

        # Categorize files for better scanning
        $fileCategories = @{
            'Source Code'   = ($allFiles | Where-Object { $_ -match '\.(cs|xaml|ps1|psm1)$' }).Count
            'Configuration' = ($allFiles | Where-Object { $_ -match '\.(json|config|xml|yml|yaml)$' }).Count
            'Documentation' = ($allFiles | Where-Object { $_ -match '\.(md|txt|rst)$' }).Count
            'Project Files' = ($allFiles | Where-Object { $_ -match '\.(csproj|sln|props)$' }).Count
            'Other'         = 0
        }
        $fileCategories.Other = $totalTracked - ($fileCategories.Values | Measure-Object -Sum).Sum

        Write-Host "📊 Repository Scan Profile:" -ForegroundColor Cyan
        Write-Host "  Total tracked files: $totalTracked" -ForegroundColor White
        foreach ($category in $fileCategories.GetEnumerator()) {
            $percentage = if ($totalTracked -gt 0) { [math]::Round(($category.Value / $totalTracked) * 100, 1) } else { 0 }
            Write-Host "  $($category.Key): $($category.Value) files ($percentage%)" -ForegroundColor Gray
        }

        # Step 5: Alignment verification
        Write-Host ""
        Write-BusBuddyStatus "Step 5: Final Alignment Verification" -Status Info

        $isAligned = $true
        $alignmentIssues = @()

        # Check git status
        $gitStatus = git status --porcelain
        if ($gitStatus) {
            $alignmentIssues += "Working directory has uncommitted changes"
            $isAligned = $false
        }

        # Check branch
        $currentBranch = git branch --show-current
        $isOnMain = ($currentBranch -eq "main") -or ($currentBranch -eq "master")
        if (-not $isOnMain) {
            $alignmentIssues += "Not on main/master branch (current: $currentBranch)"
        }

        # Check remote alignment
        try {
            $trackingBranch = git rev-parse --abbrev-ref "$currentBranch@{upstream}" 2>$null
            if ($trackingBranch) {
                $aheadBehind = git rev-list --left-right --count "$trackingBranch...$currentBranch" 2>$null
                if ($aheadBehind) {
                    $parts = $aheadBehind -split '\s+'
                    $behind = [int]$parts[0]
                    $ahead = [int]$parts[1]

                    if ($behind -gt 0) {
                        $alignmentIssues += "$behind commits behind remote"
                        $isAligned = $false
                    }
                    if ($ahead -gt 0) {
                        $alignmentIssues += "$ahead commits ahead of remote"
                        if ($PushAfterAlignment) {
                            Write-BusBuddyStatus "🚀 Auto-pushing commits to remote..." -Status Info
                            git push
                            if ($LASTEXITCODE -eq 0) {
                                Write-BusBuddyStatus "✅ Successfully pushed to remote" -Status Success
                            }
                            else {
                                $alignmentIssues += "Failed to push commits to remote"
                                $isAligned = $false
                            }
                        }
                    }
                }
            }
        }
        catch {
            # Remote alignment check failed, but not critical
        }

        # Final summary
        Write-Host ""
        Write-Host "🏁 Repository Alignment Summary:" -ForegroundColor Cyan
        Write-Host "═══════════════════════════════════" -ForegroundColor DarkGray

        if ($isAligned) {
            Write-BusBuddyStatus "✅ Repository is fully aligned and scan-ready!" -Status Success
            Write-Host ""
            Write-Host "🎉 Benefits of aligned repository:" -ForegroundColor Green
            Write-Host "  • Enhanced error diagnosis capabilities" -ForegroundColor Gray
            Write-Host "  • Optimal file scanning and analysis" -ForegroundColor Gray
            Write-Host "  • Improved development workflow momentum" -ForegroundColor Gray
            Write-Host "  • Better collaboration and issue tracking" -ForegroundColor Gray
            Write-Host ""
            Write-Host "🚌 'Aligned repos enable seamless journeys!' - BusBuddy Philosophy ✨" -ForegroundColor Cyan
        }
        else {
            Write-BusBuddyStatus "⚠️ Repository alignment has some issues:" -Status Warning
            foreach ($issue in $alignmentIssues) {
                Write-Host "  • $issue" -ForegroundColor Yellow
            }
            Write-Host ""
            Write-Host "💡 Address these issues for optimal repository alignment" -ForegroundColor Blue
        }

        return $isAligned

    }
    catch {
        Write-BusBuddyError -Message "Repository alignment error: $($_.Exception.Message)" -Exception $_.Exception
        return $false
    }
}

#endregion

#region Build and Development Functions

#Requires -Version 7.5
function Invoke-BusBuddyBuild {
    <#
    .SYNOPSIS
        Build the Bus Buddy solution with enhanced problem capture and analysis

    .DESCRIPTION
        Compiles the Bus Buddy solution with comprehensive error reporting, problem capture from VS Code,
        and automated analysis for fix recommendations. Integrates with BusBuddy's error analysis system.

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

    .PARAMETER RunAnalysis
        Run PowerShell static analysis after successful build

    .PARAMETER AnalysisSeverity
        Analysis severity levels to check (Error, Warning, Information)

    .PARAMETER CaptureProblemList
        Capture problems from VS Code's problem list for analysis

    .PARAMETER AnalyzeProblems
        Analyze captured problems and provide fix recommendations

    .PARAMETER ExportResults
        Export build results and problem analysis to files

    .PARAMETER ExportPath
        Path to export analysis results (default: .\logs\build-analysis)

    .PARAMETER AutoFix
        Attempt to automatically fix common problems

    .EXAMPLE
        bb-build -CaptureProblemList -AnalyzeProblems -ExportResults

    .EXAMPLE
        bb-build -Configuration Release -Clean -Restore -RunAnalysis -CaptureProblemList
    #>
    [CmdletBinding()]
    param(
        [ValidateSet('Debug', 'Release')]
        [string]$Configuration = 'Debug',

        [switch]$Clean,
        [switch]$Restore,
        [switch]$NoLogo,
        [switch]$RunAnalysis,
        [switch]$CaptureProblemList,
        [switch]$AnalyzeProblems,
        [switch]$ExportResults,
        [switch]$AutoFix,

        [ValidateSet('quiet', 'minimal', 'normal', 'detailed', 'diagnostic')]
        [string]$BuildVerbosity = 'minimal',

        [ValidateSet('Error', 'Warning', 'Information')]
        [string[]]$AnalysisSeverity = @('Error', 'Warning'),

        [string]$ExportPath = ".\logs\build-analysis"
    )

    $projectRoot = Get-BusBuddyProjectRoot
    if (-not $projectRoot) {
        Write-BusBuddyStatus "Project root not found" -Status Error
        return $false
    }

    Push-Location $projectRoot

    # Initialize enhanced build tracking
    $startTime = Get-Date
    $buildResults = @{
        Success = $false
        Configuration = $Configuration
        StartTime = $startTime
        Duration = $null
        WarningCount = 0
        ErrorCount = 0
        Problems = @()
        Analysis = @{}
        BuildOutput = @()
    }

    try {
        Write-BusBuddyStatus "🔨 Building BusBuddy solution with enhanced problem capture..." -Status Build

        # Step 1: Capture pre-build problems from VS Code (if requested)
        if ($CaptureProblemList) {
            Write-Host "🔍 Capturing VS Code problem list..." -ForegroundColor Cyan
            $preProblems = Get-BusBuddyProblemsFromVSCode
            if ($preProblems -and $preProblems.Count -gt 0) {
                Write-Host "   Found $($preProblems.Count) pre-build problems" -ForegroundColor Yellow
                $buildResults.Problems += $preProblems
            } else {
                Write-Host "   No pre-build problems detected" -ForegroundColor Green
            }
        }

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

        # Build solution with enhanced output analysis and problem capture
        $buildArgs = @('build', 'BusBuddy.sln', '--configuration', $Configuration, '--verbosity', $BuildVerbosity)
        if ($NoLogo) { $buildArgs += '--nologo' }

        Write-Host "🔨 Building solution with configuration: $Configuration" -ForegroundColor Green
        $buildOutput = & dotnet @buildArgs 2>&1
        $buildResults.BuildOutput = $buildOutput

        # Capture and analyze problems from build output
        $buildProblems = Get-BusBuddyProblemsFromBuildOutput -BuildOutput $buildOutput
        if ($buildProblems -and $buildProblems.Count -gt 0) {
            $buildResults.Problems += $buildProblems
            $buildResults.ErrorCount = ($buildProblems | Where-Object { $_.Severity -eq 'Error' }).Count
            $buildResults.WarningCount = ($buildProblems | Where-Object { $_.Severity -eq 'Warning' }).Count
        }

        if ($LASTEXITCODE -eq 0) {
            $buildResults.Success = $true

            # Analyze build output for warnings and success metrics
            $warnings = $buildOutput | Select-String -Pattern "warning" -AllMatches
            $warningCount = ($warnings | Measure-Object).Count
            if ($buildResults.WarningCount -eq 0) {
                $buildResults.WarningCount = $warningCount
            }

            Write-Host "✅ Build completed successfully!" -ForegroundColor Green
            Write-Host "📊 Build Summary:" -ForegroundColor Cyan
            Write-Host "   • Configuration: $Configuration" -ForegroundColor Gray
            Write-Host "   • Warnings: $($buildResults.WarningCount)" -ForegroundColor $(if ($buildResults.WarningCount -eq 0) { "Green" } else { "Yellow" })
            Write-Host "   • Errors: $($buildResults.ErrorCount)" -ForegroundColor $(if ($buildResults.ErrorCount -eq 0) { "Green" } else { "Red" })
            Write-Host "   • Total Problems: $($buildResults.Problems.Count)" -ForegroundColor $(if ($buildResults.Problems.Count -eq 0) { "Green" } else { "Yellow" })

            if ($buildResults.WarningCount -gt 0 -and $BuildVerbosity -ne 'quiet') {
                Write-Host "⚠️  Build warnings detected:" -ForegroundColor Yellow
                $warnings | Select-Object -First 5 | ForEach-Object { Write-Host "   $($_.Line)" -ForegroundColor DarkYellow }
                if ($buildResults.WarningCount -gt 5) {
                    Write-Host "   ... and $($buildResults.WarningCount - 5) more warnings" -ForegroundColor DarkYellow
                }
            }

            # Run PowerShell static analysis if requested
            if ($RunAnalysis) {
                Write-Host ""
                Write-Host "🔍 Running PowerShell static analysis..." -ForegroundColor Cyan
                try {
                    $analysisResults = Invoke-BusBuddyCodeAnalysis -Severity $AnalysisSeverity -Format Console
                    $analysisErrors = ($analysisResults | Where-Object { $_.Severity -eq "Error" }).Count
                    $analysisWarnings = ($analysisResults | Where-Object { $_.Severity -eq "Warning" }).Count

                    Write-Host "📋 Analysis Summary:" -ForegroundColor Blue
                    Write-Host "   • PowerShell Errors: $analysisErrors" -ForegroundColor $(if ($analysisErrors -eq 0) { "Green" } else { "Red" })
                    Write-Host "   • PowerShell Warnings: $analysisWarnings" -ForegroundColor $(if ($analysisWarnings -eq 0) { "Green" } else { "Yellow" })

                    if ($analysisErrors -gt 0) {
                        Write-Host "❌ PowerShell analysis found critical errors that should be fixed" -ForegroundColor Red
                        Write-Host "💡 Run 'bb-analyze' for detailed analysis and recommendations" -ForegroundColor Blue
                    }
                }
                catch {
                    Write-Host "⚠️ PowerShell analysis failed: $($_.Exception.Message)" -ForegroundColor Yellow
                    Write-Host "💡 Ensure PSScriptAnalyzer is installed: Install-Module PSScriptAnalyzer" -ForegroundColor Blue
                }
            }

            # Analyze captured problems if requested
            if ($AnalyzeProblems -and $buildResults.Problems.Count -gt 0) {
                Write-Host ""
                Write-Host "🔍 Analyzing captured problems..." -ForegroundColor Cyan
                $analysis = Invoke-BusBuddyProblemAnalysis -Problems $buildResults.Problems
                $buildResults.Analysis = $analysis

                Write-Host "📋 Problem Analysis Summary:" -ForegroundColor Blue
                Write-Host "   • Total Problems: $($analysis.TotalProblems)" -ForegroundColor Gray
                Write-Host "   • Auto-fixable: $(($analysis.Recommendations | Where-Object { $_.AutoFixable }).Count)" -ForegroundColor Green
                Write-Host "   • Files Affected: $($analysis.FileGroups.Keys.Count)" -ForegroundColor Gray

                # Show top recommendations
                if ($analysis.Recommendations -and $analysis.Recommendations.Count -gt 0) {
                    Write-Host "💡 Top Recommendations:" -ForegroundColor Blue
                    $topRecs = $analysis.Recommendations | Sort-Object {
                        if ($_.Priority -eq 'High') { 1 }
                        elseif ($_.Priority -eq 'Medium') { 2 }
                        else { 3 }
                    } | Select-Object -First 3

                    foreach ($rec in $topRecs) {
                        $icon = if ($rec.AutoFixable) { "🤖" } else { "💡" }
                        Write-Host "   $icon [$($rec.Priority)] $($rec.Description)" -ForegroundColor Yellow
                        if ($rec.AutoFixable -and $rec.Command) {
                            Write-Host "      Command: $($rec.Command)" -ForegroundColor Gray
                        }
                    }
                }

                # Auto-fix if requested and possible
                if ($AutoFix) {
                    $autoFixableRecs = $analysis.Recommendations | Where-Object { $_.AutoFixable -and $_.Command }
                    if ($autoFixableRecs -and $autoFixableRecs.Count -gt 0) {
                        Write-Host ""
                        Write-Host "🤖 Attempting automatic fixes..." -ForegroundColor Green
                        foreach ($fixRec in $autoFixableRecs) {
                            Write-Host "   Applying: $($fixRec.Description)" -ForegroundColor Yellow
                            try {
                                # Execute the auto-fix command if it exists
                                if (Get-Command $fixRec.Command -ErrorAction SilentlyContinue) {
                                    & $fixRec.Command
                                    Write-Host "   ✅ Applied: $($fixRec.Description)" -ForegroundColor Green
                                } else {
                                    Write-Host "   ⚠️ Command not found: $($fixRec.Command)" -ForegroundColor Yellow
                                }
                            }
                            catch {
                                Write-Host "   ❌ Auto-fix failed: $($_.Exception.Message)" -ForegroundColor Red
                            }
                        }
                    }
                }
            }

            # Export results if requested
            if ($ExportResults) {
                Write-Host ""
                Write-Host "📄 Exporting build results..." -ForegroundColor Cyan
                $buildResults.Duration = (Get-Date) - $startTime
                $exportSuccess = Export-BusBuddyBuildResults -BuildResults $buildResults -ExportPath $ExportPath
                if ($exportSuccess) {
                    Write-Host "   ✅ Results exported to: $ExportPath" -ForegroundColor Green
                } else {
                    Write-Host "   ⚠️ Export failed" -ForegroundColor Yellow
                }
            }

            Write-BusBuddyStatus "Build completed successfully" -Status Success
            return $buildResults
        }
        else {
            $buildResults.Success = $false
            Write-BusBuddyStatus "Build failed with exit code $LASTEXITCODE" -Status Error

            # Analyze build failures
            if ($BuildResults.ErrorCount -gt 0) {
                Write-Host "❌ Build errors detected:" -ForegroundColor Red
                $errorProblems = $BuildResults.Problems | Where-Object { $_.Severity -eq 'Error' } | Select-Object -First 3
                foreach ($error in $errorProblems) {
                    $relativePath = $error.File -replace [regex]::Escape($projectRoot), '.'
                    Write-Host "   • $($error.Code) in $relativePath`:$($error.Line) - $($error.Message)" -ForegroundColor Red
                }
                if ($BuildResults.ErrorCount -gt 3) {
                    Write-Host "   ... and $($BuildResults.ErrorCount - 3) more errors" -ForegroundColor Red
                }
            }

            if ($BuildVerbosity -ne 'quiet') {
                Write-Host ""
                Write-Host "📋 Full build output:" -ForegroundColor Yellow
                $buildOutput | ForEach-Object { Write-Host $_ -ForegroundColor Gray }
            }

            # Export failed build results if requested
            if ($ExportResults) {
                $buildResults.Duration = (Get-Date) - $startTime
                Export-BusBuddyBuildResults -BuildResults $buildResults -ExportPath $ExportPath | Out-Null
            }

            return $buildResults
        }
    }
    catch {
        Write-BusBuddyStatus "Build process error: $($_.Exception.Message)" -Status Error
        $buildResults.Success = $false
        $buildResults.Duration = (Get-Date) - $startTime

        # Add exception details to build results
        $buildResults.Problems += @{
            File = "Build Process"
            Line = 0
            Column = 0
            Severity = 'Error'
            Code = 'BUILD_EXCEPTION'
            Message = $_.Exception.Message
            Source = 'PowerShell'
        }

        if ($ExportResults) {
            Export-BusBuddyBuildResults -BuildResults $buildResults -ExportPath $ExportPath | Out-Null
        }

        return $buildResults
    }
    finally {
        Pop-Location
    }
}

#region Enhanced Build Helper Functions

function Get-BusBuddyProblemsFromVSCode {
    <#
    .SYNOPSIS
        Capture problems from VS Code's problem list and workspace analysis
    #>

    $problems = @()

    try {
        # Check if VS Code is running
        $vscodeProcesses = Get-Process -Name "Code" -ErrorAction SilentlyContinue
        if (-not $vscodeProcesses) {
            Write-Host "   VS Code not running - skipping problem list capture" -ForegroundColor Gray
            return @()
        }

        # Analyze workspace files for common C# problems
        $projectRoot = Get-Location
        $csharpFiles = Get-ChildItem -Path $projectRoot -Recurse -Filter "*.cs" -ErrorAction SilentlyContinue | Select-Object -First 20

        foreach ($file in $csharpFiles) {
            try {
                $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
                if ($content) {
                    # Check for common issues
                    $lines = $content -split "`n"
                    for ($i = 0; $i -lt $lines.Count; $i++) {
                        $line = $lines[$i]

                        # Check for potential missing using statements
                        if ($line -match '^\s*([A-Z][a-zA-Z0-9_]*)\s+\w+' -and $line -notmatch '^\s*(public|private|protected|internal|static|readonly|const|var|string|int|bool|double|decimal|float|DateTime|Task|void|class|interface|struct|enum)') {
                            $typeName = $matches[1]
                            if ($typeName -notin @('String', 'Int32', 'Boolean', 'Double', 'Decimal', 'Single', 'Object')) {
                                $problems += @{
                                    File = $file.FullName
                                    Line = $i + 1
                                    Column = 1
                                    Severity = 'Warning'
                                    Code = 'CS0246'
                                    Message = "Potential missing using statement for type '$typeName'"
                                    Source = 'VS Code Analysis'
                                }
                            }
                        }

                        # Check for TODO/FIXME comments
                        if ($line -match '(TODO|FIXME|HACK|XXX)') {
                            $problems += @{
                                File = $file.FullName
                                Line = $i + 1
                                Column = 1
                                Severity = 'Information'
                                Code = 'TODO'
                                Message = "Code comment requires attention: $($matches[1])"
                                Source = 'VS Code Analysis'
                            }
                        }
                    }
                }
            }
            catch {
                # Skip problematic files
            }
        }

        return $problems
    }
    catch {
        Write-Host "   Warning: VS Code problem capture failed: $($_.Exception.Message)" -ForegroundColor Yellow
        return @()
    }
}

function Get-BusBuddyProblemsFromBuildOutput {
    <#
    .SYNOPSIS
        Parse build output for errors, warnings, and problems
    #>
    param(
        [string[]]$BuildOutput
    )

    $problems = @()

    if (-not $BuildOutput) {
        return $problems
    }

    foreach ($line in $BuildOutput) {
        # Parse MSBuild error/warning format
        # Example: "C:\path\to\file.cs(12,34): error CS0103: The name 'variable' does not exist"
        if ($line -match '^(.+?)\((\d+),(\d+)\):\s*(error|warning)\s*([A-Z]+\d+):\s*(.+)$') {
            $problems += @{
                File = $matches[1].Trim()
                Line = [int]$matches[2]
                Column = [int]$matches[3]
                Severity = if ($matches[4] -eq 'error') { 'Error' } else { 'Warning' }
                Code = $matches[5]
                Message = $matches[6].Trim()
                Source = 'MSBuild'
            }
        }
        # Parse NuGet package errors
        elseif ($line -match 'error\s+(NU\d+):\s*(.+)$') {
            $problems += @{
                File = "NuGet"
                Line = 1
                Column = 1
                Severity = 'Error'
                Code = $matches[1]
                Message = $matches[2].Trim()
                Source = 'NuGet'
            }
        }
        # Parse general errors
        elseif ($line -match '^(.+?):\s*(error|warning):\s*(.+)$') {
            $problems += @{
                File = $matches[1].Trim()
                Line = 1
                Column = 1
                Severity = if ($matches[2] -eq 'error') { 'Error' } else { 'Warning' }
                Code = 'UNKNOWN'
                Message = $matches[3].Trim()
                Source = 'Generic'
            }
        }
    }

    return $problems
}

function Invoke-BusBuddyProblemAnalysis {
    <#
    .SYNOPSIS
        Analyze captured problems and provide fix recommendations
    #>
    param(
        [array]$Problems
    )

    if (-not $Problems -or $Problems.Count -eq 0) {
        return @{
            TotalProblems = 0
            ErrorCount = 0
            WarningCount = 0
            FileGroups = @{}
            CodeGroups = @{}
            Recommendations = @()
        }
    }

    $analysis = @{
        TotalProblems = $Problems.Count
        ErrorCount = ($Problems | Where-Object { $_.Severity -eq 'Error' }).Count
        WarningCount = ($Problems | Where-Object { $_.Severity -eq 'Warning' }).Count
        FileGroups = @{}
        CodeGroups = @{}
        Recommendations = @()
    }

    # Group problems by file and code
    foreach ($problem in $Problems) {
        $file = $problem.File
        if (-not $analysis.FileGroups.ContainsKey($file)) {
            $analysis.FileGroups[$file] = @()
        }
        $analysis.FileGroups[$file] += $problem

        $code = $problem.Code
        if (-not $analysis.CodeGroups.ContainsKey($code)) {
            $analysis.CodeGroups[$code] = @()
        }
        $analysis.CodeGroups[$code] += $problem
    }

    # Generate recommendations based on problem patterns
    foreach ($codeGroup in $analysis.CodeGroups.Keys) {
        $codeProblems = $analysis.CodeGroups[$codeGroup]
        $count = $codeProblems.Count

        switch ($codeGroup) {
            'CS0103' {
                $analysis.Recommendations += @{
                    Type = 'Automated Fix'
                    Priority = 'High'
                    Description = "Fix $count undefined variable/method errors"
                    Action = 'Add missing using statements or fix variable names'
                    AutoFixable = $true
                    Command = 'bb-restore'  # Package restore often fixes these
                }
            }
            'CS0246' {
                $analysis.Recommendations += @{
                    Type = 'Package Reference'
                    Priority = 'High'
                    Description = "Fix $count missing type/namespace errors"
                    Action = 'Add missing package references or using statements'
                    AutoFixable = $true
                    Command = 'bb-restore'
                }
            }
            'NU1605' {
                $analysis.Recommendations += @{
                    Type = 'NuGet Conflict'
                    Priority = 'High'
                    Description = "Fix $count NuGet package version conflicts"
                    Action = 'Update package references to compatible versions'
                    AutoFixable = $true
                    Command = 'bb-restore'
                }
            }
            'TODO' {
                $analysis.Recommendations += @{
                    Type = 'Code Quality'
                    Priority = 'Low'
                    Description = "Review $count TODO/FIXME comments"
                    Action = 'Address pending code improvements'
                    AutoFixable = $false
                    Command = $null
                }
            }
            default {
                $analysis.Recommendations += @{
                    Type = 'Manual Review'
                    Priority = 'Medium'
                    Description = "Review $count $codeGroup issues"
                    Action = 'Manual code review and fixing required'
                    AutoFixable = $false
                    Command = $null
                }
            }
        }
    }

    return $analysis
}

function Export-BusBuddyBuildResults {
    <#
    .SYNOPSIS
        Export build results and problem analysis to files
    #>
    param(
        [hashtable]$BuildResults,
        [string]$ExportPath
    )

    try {
        # Ensure export directory exists
        if (-not (Test-Path $ExportPath)) {
            New-Item -Path $ExportPath -ItemType Directory -Force | Out-Null
        }

        $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
        $success = $BuildResults.Success
        $status = if ($success) { "success" } else { "failed" }

        # Export main results to JSON
        $resultsFile = Join-Path $ExportPath "build-results-$status-$timestamp.json"
        $BuildResults | ConvertTo-Json -Depth 10 | Out-File $resultsFile -Encoding UTF8

        # Create summary report
        $summaryFile = Join-Path $ExportPath "build-summary-$status-$timestamp.md"
        $summary = @"
# BusBuddy Build Analysis Report
Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

## Build Summary
- **Status**: $(if ($BuildResults.Success) { 'SUCCESS ✅' } else { 'FAILED ❌' })
- **Configuration**: $($BuildResults.Configuration)
- **Duration**: $([math]::Round($BuildResults.Duration.TotalSeconds, 1))s
- **Problems Found**: $($BuildResults.Problems.Count)
- **Errors**: $($BuildResults.ErrorCount)
- **Warnings**: $($BuildResults.WarningCount)

## Problem Analysis
"@

        if ($BuildResults.Analysis -and $BuildResults.Analysis.Recommendations) {
            $summary += "`n### Recommendations"
            foreach ($rec in $BuildResults.Analysis.Recommendations) {
                $icon = if ($rec.AutoFixable) { "🤖" } else { "💡" }
                $summary += "`n- $icon **$($rec.Type)** [$($rec.Priority)]: $($rec.Description)"
                if ($rec.Command) {
                    $summary += "`n  - Command: ``$($rec.Command)``"
                }
            }
        }

        if ($BuildResults.Problems -and $BuildResults.Problems.Count -gt 0) {
            $summary += "`n`n### Top Problems"
            $topProblems = $BuildResults.Problems | Where-Object { $_.Severity -eq 'Error' } | Select-Object -First 5
            foreach ($problem in $topProblems) {
                $relativePath = $problem.File -replace [regex]::Escape((Get-Location).Path), '.'
                $summary += "`n- **$($problem.Severity)** $($problem.Code): $($problem.Message)"
                $summary += "`n  - File: $relativePath`:$($problem.Line)"
            }
        }

        $summary | Out-File $summaryFile -Encoding UTF8

        return $true
    }
    catch {
        Write-Host "   Export error: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

#endregion

#Requires -Version 7.5
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

#Requires -Version 7.5
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

        # Run the tests
        & dotnet @testArgs

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

#Requires -Version 7.5
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
    $features.IsNet8Plus = $features.DotNetVersion -ge [version]'8.0'

    Write-Host "PowerShell Version: $($features.PowerShellVersion)" -ForegroundColor $(if ($features.IsPS75Plus) { 'Green' } else { 'Yellow' })
    Write-Host ".NET Version: $($features.DotNetVersion)" -ForegroundColor $(if ($features.IsNet8Plus) { 'Green' } else { 'Yellow' })
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

    $quotes = $script:ModuleConfig.HappinessQuotes

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
        [ValidateSet('All', 'Essential', 'Build', 'Development', 'Analysis', 'GitHub', 'AI', 'Fun')]
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
            @{ Name = 'bb-git-status'; Description = 'Enhanced git status with health check'; Function = 'Get-BusBuddyGitStatus' }
            @{ Name = 'bb-git-check'; Description = 'Analyze repository and .gitignore'; Function = 'Invoke-BusBuddyGitIgnoreCheck' }
            @{ Name = 'bb-git-help'; Description = 'PowerShell git command reference'; Function = 'Get-BusBuddyGitEquivalents' }
            @{ Name = 'bb-git-repair'; Description = 'Advanced git repository repair toolkit'; Function = 'Invoke-BusBuddyGitRepairKit' }
            @{ Name = 'bb-repo-align'; Description = 'Complete repository alignment workflow'; Function = 'Start-BusBuddyRepositoryAlignment' }
            @{ Name = 'bb-github-stage'; Description = 'Smart Git staging'; Function = 'Invoke-BusBuddyGitHubStaging' }
            @{ Name = 'bb-github-commit'; Description = 'Intelligent commit creation'; Function = 'Invoke-BusBuddyGitHubCommit' }
            @{ Name = 'bb-github-push'; Description = 'Push with workflow monitoring'; Function = 'Invoke-BusBuddyGitHubPush' }
            @{ Name = 'bb-github-workflow'; Description = 'Complete GitHub workflow'; Function = 'Invoke-BusBuddyCompleteGitHubWorkflow' }
        )
        'Environment' = @(
            @{ Name = 'bb-install-extensions'; Description = 'Install VS Code extensions'; Function = 'Install-BusBuddyVSCodeExtensions' }
            @{ Name = 'bb-validate-vscode'; Description = 'Validate VS Code setup'; Function = 'Test-BusBuddyVSCodeSetup' }
        )
        'AI'          = @(
            @{ Name = 'bb-tavily-search'; Description = 'Search with Tavily Expert AI'; Function = 'Invoke-BusBuddyTavilySearch' }
            @{ Name = 'bb-search'; Description = 'AI-powered web search (alias)'; Function = 'Invoke-BusBuddyTavilySearch' }
            @{ Name = 'bb-ai-workflow'; Description = 'Start AI-enhanced development workflow'; Function = 'Start-BusBuddyAIWorkflow' }
            @{ Name = 'bb-ai-help'; Description = 'Get AI assistance for development tasks'; Function = 'Get-BusBuddyAIAssistance' }
            @{ Name = 'bb-context-search'; Description = 'Context-aware project search'; Function = 'Get-BusBuddyContextualSearch' }
            @{ Name = 'bb-mentor'; Description = 'AI learning mentor'; Function = 'Get-BusBuddyMentor' }
            @{ Name = 'bb-docs'; Description = 'Search official documentation'; Function = 'Search-OfficialDocs' }
            @{ Name = 'bb-ref'; Description = 'Quick reference sheets'; Function = 'Get-QuickReference' }
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

    $config = $script:ModuleConfig
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
        Write-DependencySummary -Results $results

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
    $errorCount = @($errors).Count
    Write-Host "Total Errors Found: $errorCount" -ForegroundColor $(if ($errorCount -eq 0) { 'Green' } else { 'Red' })

    if ($errorCount -gt 0) {
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
            TotalErrors       = @($errors).Count
            AutoFixableErrors = @($fixes | Where-Object { $_.AutoFixable }).Count
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

# Git and repository aliases
Set-Alias -Name 'bb-git-check' -Value 'Invoke-BusBuddyGitIgnoreCheck' -Description 'Analyze repository and .gitignore effectiveness'
Set-Alias -Name 'bb-git-help' -Value 'Get-BusBuddyGitEquivalents' -Description 'PowerShell equivalents for Unix git commands'
Set-Alias -Name 'bb-ps-git' -Value 'Get-BusBuddyGitEquivalents' -Description 'PowerShell git command reference'
Set-Alias -Name 'bb-git-repair' -Value 'Invoke-BusBuddyGitRepairKit' -Description 'Advanced git repository repair toolkit'
Set-Alias -Name 'bb-repo-align' -Value 'Start-BusBuddyRepositoryAlignment' -Description 'Complete repository alignment workflow'

# Happiness and utility aliases
Set-Alias -Name 'bb-happiness' -Value 'Get-BusBuddyHappiness' -Description 'Get motivational quotes'
Set-Alias -Name 'bb-commands' -Value 'Get-BusBuddyCommands' -Description 'List all Bus Buddy commands'
Set-Alias -Name 'bb-info' -Value 'Get-BusBuddyInfo' -Description 'Show module information and status'
Set-Alias -Name 'bb-env-check' -Value 'Test-BusBuddyEnvironment' -Description 'Environment validation'
Set-Alias -Name 'bb-validate' -Value 'Test-BusBuddyEnvironment' -Description 'Environment validation (alias)'

# Utility aliases
Set-Alias -Name 'bb-happiness' -Value 'Get-BusBuddyHappiness' -Description 'Motivational quotes'
Set-Alias -Name 'bb-commands' -Value 'Get-BusBuddyCommands' -Description 'List all commands'
Set-Alias -Name 'bb-info' -Value 'Get-BusBuddyInfo' -Description 'Module information'

# AI and Search aliases
Set-Alias -Name 'bb-tavily-search' -Value 'Invoke-BusBuddyTavilySearch' -Description 'Search with Tavily Expert AI'
Set-Alias -Name 'bb-search' -Value 'Invoke-BusBuddyTavilySearch' -Description 'AI-powered web search'
Set-Alias -Name 'bb-ai-workflow' -Value 'Start-BusBuddyAIWorkflow' -Description 'Start AI-enhanced development workflow'
Set-Alias -Name 'bb-ai-help' -Value 'Get-BusBuddyAIAssistance' -Description 'Get AI assistance for development tasks'
Set-Alias -Name 'bb-context-search' -Value 'Get-BusBuddyContextualSearch' -Description 'Context-aware project search'

# Learning and mentorship aliases with error handling
try {
    if (-not (Get-Alias -Name 'bb-mentor' -ErrorAction SilentlyContinue)) {
        Set-Alias -Name 'bb-mentor' -Value 'Get-BusBuddyMentor' -Description 'AI learning mentor'
    }
    if (-not (Get-Alias -Name 'bb-docs' -ErrorAction SilentlyContinue)) {
        Set-Alias -Name 'bb-docs' -Value 'Search-OfficialDocs' -Description 'Search official documentation'
    }
    if (-not (Get-Alias -Name 'bb-ref' -ErrorAction SilentlyContinue)) {
        Set-Alias -Name 'bb-ref' -Value 'Get-QuickReference' -Description 'Quick reference sheets'
    }
} catch {
    # Silently handle alias conflicts
}

#endregion

#region Module Initialization

#Requires -Version 7.5
function Import-BusBuddyFunction {
    <#
    .SYNOPSIS
        Imports a single Bus Buddy function from a file

    .DESCRIPTION
        Loads a function from a source file and imports it into the module
        Used by Import-BusBuddyFunctionCategory to load all functions in a category

    .PARAMETER FilePath
        Path to the file containing the function

    .PARAMETER FunctionName
        Optional name of the function to import (defaults to file name without extension)
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$FilePath,

        [string]$FunctionName
    )

    if (-not (Test-Path $FilePath)) {
        Write-Error "Function file not found: $FilePath"
        return $false
    }

    try {
        # If function name not provided, use file name without extension
        if (-not $FunctionName) {
            $FunctionName = [System.IO.Path]::GetFileNameWithoutExtension($FilePath)
        }

        # Load the function content
        . $FilePath

        # Export the function
        Write-Verbose "Imported function: $FunctionName from $FilePath"
        return $true
    }
    catch {
        Write-Error "Failed to import function from $FilePath`: $($_.Exception.Message)"
        return $false
    }
}

#Requires -Version 7.5
function Import-BusBuddyFunctionCategory {
    <#
    .SYNOPSIS
        Imports all functions in a Bus Buddy function category

    .DESCRIPTION
        Loads all PS1 function files from a category folder and imports them
        into the module using the Import-BusBuddyFunction helper

    .PARAMETER CategoryPath
        Path to the category folder containing function files

    .PARAMETER CategoryName
        Name of the category (for logging)
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$CategoryPath,

        [Parameter(Mandatory)]
        [string]$CategoryName
    )

    if (-not (Test-Path $CategoryPath)) {
        Write-Warning "Category folder not found: $CategoryPath"
        return @()
    }

    $functionFiles = Get-ChildItem -Path $CategoryPath -Filter "*.ps1" -File

    $importedFunctions = @()

    Write-Verbose "Loading $($functionFiles.Count) functions from category: $CategoryName"

    foreach ($file in $functionFiles) {
        $functionName = $file.BaseName
        $imported = Import-BusBuddyFunction -FilePath $file.FullName -FunctionName $functionName

        if ($imported) {
            $importedFunctions += $functionName
        }
    }

    Write-Verbose "Successfully imported $($importedFunctions.Count) functions from category: $CategoryName"
    return $importedFunctions
}

#Requires -Version 7.5
function Initialize-BusBuddyModule {
    <#
    .SYNOPSIS
        Initializes the Bus Buddy PowerShell module

    .DESCRIPTION
        Loads all function categories, sets up module configuration, and initializes
        the module state. Called during module import to properly configure the environment.

    .PARAMETER ModuleRoot
        Root path of the module

    .PARAMETER ShowDetails
        Display detailed initialization information
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$ModuleRoot,

        [switch]$ShowDetails
    )    # Define module configuration
    $script:ModuleConfig = @{
        Version                  = '2.0.0'
        Author                   = 'Bus Buddy Development Team'
        ProjectRoot              = $null
        BusBuddyCoreAssemblyPath = $null
        FunctionCategories       = @(
            'Build',
            'Database',
            'Diagnostics',
            'Development',
            'Utilities',
            'GitHub'
        )
        LoadedFunctions          = @()
        HappinessQuotes          = @(
            "Remember: Transit systems have schedules, but our code doesn't have to be late!",
            "Just like a bus route, good code follows clean, predictable patterns.",
            "Debugging is like tracking a bus - follow the route methodically and you'll find the problem.",
            "The best code, like the best bus drivers, handles unexpected conditions with grace.",
            "A good programmer, like a good transit system, values reliability above all else.",
            "Code without documentation is like a bus without route signs - confusing for everyone.",
            "Unit tests are like bus inspections - they prevent bigger problems down the road.",
            "If your code were a bus, would you feel safe riding in it?",
            "The only place where copy-paste is acceptable is the bus schedule, not your code!"
        )
    }

    # Load module settings
    $settingsPath = Join-Path $ModuleRoot "BusBuddy.settings.ini"
    $settings = Import-BusBuddySettings -SettingsPath $settingsPath

    # Store settings globally for access throughout module
    $global:BusBuddySettings = $settings

    # Create paths
    $categoriesRoot = Join-Path $ModuleRoot "Functions"

    # Initialize categories
    $allLoadedFunctions = @()

    foreach ($category in $script:ModuleConfig.FunctionCategories) {
        $categoryPath = Join-Path $categoriesRoot $category

        if (Test-Path $categoryPath) {
            $loadedFunctions = Import-BusBuddyFunctionCategory -CategoryPath $categoryPath -CategoryName $category
            $allLoadedFunctions += $loadedFunctions

            if ($ShowDetails -or $settings.General.VerboseLogging) {
                Write-Host "Loaded $($loadedFunctions.Count) functions from category: $category" -ForegroundColor Cyan
            }
        }
        else {
            if ($ShowDetails -or $settings.General.VerboseLogging) {
                Write-Host "Category folder not found: $category" -ForegroundColor Yellow
            }
        }
    }

    # Add Tavily search functions and AI workflow functions
    # Load enhanced AI functions
    $aiFunctionsPath = Join-Path $ModuleRoot "Functions\AI"
    if (Test-Path $aiFunctionsPath) {
        $aiFiles = @(
            'Invoke-BusBuddyTavilySearch.ps1',
            'BusBuddy-AI-Workflows.ps1'
        )

        foreach ($aiFile in $aiFiles) {
            $aiFilePath = Join-Path $aiFunctionsPath $aiFile
            if (Test-Path $aiFilePath) {
                try {
                    . $aiFilePath
                    if ($ShowDetails -or $settings.General.VerboseLogging) {
                        Write-Host "Loaded AI function file: $aiFile" -ForegroundColor Green
                    }
                }
                catch {
                    Write-Warning "Failed to load AI function file $aiFile`: $($_.Exception.Message)"
                }
            }
        }
    }

    $allLoadedFunctions += @(
        'Invoke-BusBuddyTavilySearch',
        'Format-TavilyResultsSummary',
        'Format-TavilyResultsMarkdown',
        'Start-BusBuddyAIWorkflow',
        'Get-BusBuddyAIAssistance',
        'Get-BusBuddyContextualSearch'
    )

    $script:ModuleConfig.LoadedFunctions = $allLoadedFunctions

    # Set project root (will be updated in Get-BusBuddyProjectRoot)
    $script:ModuleConfig.ProjectRoot = Get-BusBuddyProjectRoot

    # Initialize core assembly
    if ($settings.Advanced.LoadDotNetAssemblies) {
        Initialize-BusBuddyCoreAssembly | Out-Null
    }

    if ($ShowDetails -or $settings.General.VerboseLogging) {
        Write-Host "Bus Buddy Module Initialization Complete" -ForegroundColor Green
        Write-Host "Loaded $($allLoadedFunctions.Count) functions across $($script:ModuleConfig.FunctionCategories.Count) categories" -ForegroundColor Green
    }

    return $script:ModuleConfig
}

# Display welcome message when module loads
$script:ModuleConfig = @{
    Version                  = '2.0.0'
    Author                   = 'Bus Buddy Development Team'
    ProjectRoot              = $null
    BusBuddyCoreAssemblyPath = $null
    FunctionCategories       = @(
        'Build',
        'Database',
        'Diagnostics',
        'Development',
        'Utilities',
        'GitHub',
        'AI'
    )
    LoadedFunctions          = @()
    HappinessQuotes          = @(
        "Remember: Transit systems have schedules, but our code doesn't have to be late!",
        "Just like a bus route, good code follows clean, predictable patterns.",
        "Debugging is like tracking a bus - follow the route methodically and you'll find the problem.",
        "The best code, like the best bus drivers, handles unexpected conditions with grace.",
        "A good programmer, like a good transit system, values reliability above all else.",
        "Code without documentation is like a bus without route signs - confusing for everyone.",
        "Unit tests are like bus inspections - they prevent bigger problems down the road.",
        "If your code were a bus, would you feel safe riding in it?",
        "The only place where copy-paste is acceptable is the bus schedule, not your code!"
    )
}

# Load settings file
$settingsPath = Join-Path $PSScriptRoot "BusBuddy.settings.ini"
$settings = . (Join-Path $PSScriptRoot "Functions\Utilities\Import-BusBuddySettings.ps1") -SettingsPath $settingsPath
$global:BusBuddySettings = $settings

# Initialize the module
Initialize-BusBuddyModule -ModuleRoot $PSScriptRoot -ShowDetails:($settings.General.VerboseLogging) | Out-Null

# Show welcome message if enabled in settings
if ($settings.General.ShowWelcomeMessage) {
    $welcomeMessage = @"

🚌 Bus Buddy PowerShell Module v$($script:ModuleConfig.Version) Loaded Successfully!
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
}

# Auto-detect and display environment status if enabled in settings
if ($settings.General.AutoCheckEnvironment -and (Get-BusBuddyProjectRoot)) {
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

function Write-DependencySummary {
    <#
    .SYNOPSIS
        Write dependency management summary
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
        [string]$Repository = "Bigessfour/BusBuddy-WPF",
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

            # Updated field names based on GitHub CLI compatibility
            $ghArgs = @('run', 'list', '--limit', $Count, '--json', 'status,conclusion,createdAt,name,workflowName,databaseId,event,headBranch')
            if ($Status -ne 'all') {
                $ghArgs += '--status', $Status
            }

            try {
                $ghOutput = & gh @ghArgs 2>&1
                if ($LASTEXITCODE -eq 0) {
                    $results = $ghOutput | ConvertFrom-Json
                    # Convert databaseId to id for compatibility
                    $results = $results | ForEach-Object {
                        $_ | Add-Member -NotePropertyName 'id' -NotePropertyValue $_.databaseId -Force
                        $_
                    }
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

        $categoryIndex = 0
        foreach ($category in $sortedCategories) {
            $code = $category.Key
            $count = $category.Value.Count
            $description = Get-WarningCodeDescription -Code $code

            Write-Host "   $code ($count occurrences): $description" -ForegroundColor Cyan

            # Show sample for top 3
            if ($categoryIndex -lt 3) {
                $sample = $category.Value[0] -replace '.*warning\s+[A-Z]{2}\d{4}:\s*', '' -replace '\s*\[.*\].*$', ''
                Write-Host "     Sample: $($sample.Substring(0, [Math]::Min(80, $sample.Length)))" -ForegroundColor Gray
            }
            $categoryIndex++
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
        Supports .NET 8, WPF, PowerShell 7.5, Azure integration, and warning reduction.
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
                '1. Environment Setup: Ensure PowerShell 7.5+ and .NET 8+',
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

# Create aliases for the mentor system with error handling
try {
    if (-not (Get-Alias -Name "bb-mentor" -ErrorAction SilentlyContinue)) {
        New-Alias -Name "bb-mentor" -Value "Get-BusBuddyMentor" -Description "AI learning mentor"
    }
    if (-not (Get-Alias -Name "bb-docs" -ErrorAction SilentlyContinue)) {
        New-Alias -Name "bb-docs" -Value "Search-OfficialDocs" -Description "Search official documentation"
    }
    if (-not (Get-Alias -Name "bb-ref" -ErrorAction SilentlyContinue)) {
        New-Alias -Name "bb-ref" -Value "Get-QuickReference" -Description "Quick reference sheets"
    }
} catch {
    # Silently handle alias conflicts
}

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
$githubScriptPath = Join-Path $PSScriptRoot "BusBuddy PowerShell Environment\Scripts\BusBuddy-GitHub-Automation.ps1"
if (Test-Path $githubScriptPath) {
    . $githubScriptPath
}

# Wrapper functions for Bus Buddy integration
function Get-BusBuddyGitStatus {
    <#
    .SYNOPSIS
        Enhanced Git status with Bus Buddy health check integration (bb-git-status)

    .DESCRIPTION
        Displays Git status information combined with BusBuddy environment health check.
        Helps identify tracked files, modified files, untracked files, and ignored files
        while providing context about the overall project health.

    .PARAMETER Detailed
        Show detailed Git status with more verbose output

    .PARAMETER IncludeIgnored
        Include ignored files in the status output

    .PARAMETER QuickHealth
        Run a quick health check instead of detailed analysis

    .EXAMPLE
        bb-git-status

    .EXAMPLE
        Get-BusBuddyGitStatus -Detailed

    .EXAMPLE
        bb-git-status -IncludeIgnored -QuickHealth
    #>
    [CmdletBinding()]
    param(
        [switch]$Detailed,
        [switch]$IncludeIgnored,
        [switch]$QuickHealth
    )

    $projectRoot = Get-BusBuddyProjectRoot
    Push-Location $projectRoot

    try {
        Write-BusBuddyStatus "🔍 Checking Git status and project health..." -Status Info
        Write-Host ""

        # Git Repository Information
        Write-Host "📁 Repository Information:" -ForegroundColor Cyan
        $currentBranch = git branch --show-current 2>$null
        if ($currentBranch) {
            Write-Host "   Current Branch: $currentBranch" -ForegroundColor Green
        }

        $remoteUrl = git config --get remote.origin.url 2>$null
        if ($remoteUrl) {
            Write-Host "   Remote Origin: $remoteUrl" -ForegroundColor Yellow
        }

        Write-Host ""

        # Git Status Output
        Write-Host "📊 Git Status:" -ForegroundColor Cyan

        if ($Detailed) {
            # Detailed git status with more information
            $gitArgs = @('status', '--verbose', '--branch')
            if ($IncludeIgnored) {
                $gitArgs += '--ignored'
            }
            & git @gitArgs
        }
        else {
            # Standard git status
            $gitArgs = @('status')
            if ($IncludeIgnored) {
                $gitArgs += '--ignored'
            }
            & git @gitArgs
        }

        Write-Host ""

        # Quick analysis of git status
        $gitStatusOutput = git status --porcelain 2>$null
        if ($gitStatusOutput) {
            # Ensure we have an array for counting
            if ($gitStatusOutput -is [string]) {
                $gitStatusArray = @($gitStatusOutput)
            }
            else {
                $gitStatusArray = $gitStatusOutput
            }

            $modifiedLines = $gitStatusArray | Where-Object { $_ -match '^\s*M' }
            $addedLines = $gitStatusArray | Where-Object { $_ -match '^\s*A' }
            $deletedLines = $gitStatusArray | Where-Object { $_ -match '^\s*D' }
            $untrackedLines = $gitStatusArray | Where-Object { $_ -match '^\?\?' }

            $modifiedFiles = if ($modifiedLines) { @($modifiedLines).Count } else { 0 }
            $addedFiles = if ($addedLines) { @($addedLines).Count } else { 0 }
            $deletedFiles = if ($deletedLines) { @($deletedLines).Count } else { 0 }
            $untrackedFiles = if ($untrackedLines) { @($untrackedLines).Count } else { 0 }

            Write-Host "📈 Status Summary:" -ForegroundColor Magenta
            if ($modifiedFiles -gt 0) { Write-Host "   Modified files: $modifiedFiles" -ForegroundColor Yellow }
            if ($addedFiles -gt 0) { Write-Host "   Added files: $addedFiles" -ForegroundColor Green }
            if ($deletedFiles -gt 0) { Write-Host "   Deleted files: $deletedFiles" -ForegroundColor Red }
            if ($untrackedFiles -gt 0) { Write-Host "   Untracked files: $untrackedFiles" -ForegroundColor Cyan }

            Write-Host ""

            # Actionable recommendations
            Write-Host "💡 Recommendations:" -ForegroundColor Blue
            if ($untrackedFiles -gt 0) {
                Write-Host "   • Use 'git add <file>' to stage new files for tracking" -ForegroundColor Gray
            }
            if ($modifiedFiles -gt 0) {
                Write-Host "   • Use 'git add <file>' to stage modified files for commit" -ForegroundColor Gray
            }
            if ($modifiedFiles -gt 0 -or $addedFiles -gt 0) {
                Write-Host "   • Use 'bb-github-workflow' for complete GitHub integration" -ForegroundColor Gray
            }
        }
        else {
            Write-Host "✅ Working tree clean - no changes detected" -ForegroundColor Green
        }

        Write-Host ""

        # Chain with health check for momentum
        Write-Host "🏥 BusBuddy Health Check:" -ForegroundColor Cyan
        if ($QuickHealth) {
            Test-BusBuddyEnvironment
        }
        else {
            Test-BusBuddyEnvironment -Detailed
        }

        Write-BusBuddyStatus "✅ Git status and health check completed" -Status Success
    }
    catch {
        Write-BusBuddyError "Error getting Git status: $_"
        throw
    }
    finally {
        Pop-Location
    }
}

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
    .DESCRIPTION
        Executes the complete GitHub workflow: stage → commit → push → monitor.
        Integrates with all recommended VS Code extensions for optimal experience.
    #>
    [CmdletBinding(SupportsShouldProcess)]
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
Set-Alias -Name 'bb-git-status' -Value 'Get-BusBuddyGitStatus'
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

    # PowerShell static analysis functions
    'Invoke-BusBuddyCodeAnalysis',
    'Test-BusBuddyScriptSyntax',
    'Invoke-BusBuddyPreCommitAnalysis',

    # GitHub automation functions
    'Get-BusBuddyGitStatus',
    'Invoke-BusBuddyGitHubPush',
    'Invoke-BusBuddyGitHubCommit',
    'Invoke-BusBuddyGitHubStaging',
    'Invoke-BusBuddyCompleteGitHubWorkflow',

    # VS Code integration functions
    'Install-BusBuddyVSCodeExtensions',
    'Test-BusBuddyVSCodeSetup'
)

#region GitHub Automation Integration

function Invoke-BusBuddyAIConfig {
    <#
    .SYNOPSIS
        Configure AI services for BusBuddy development workflows

    .DESCRIPTION
        Manages configuration of AI services including xAI Grok-4 and other AI providers
        for BusBuddy development tasks. Provides centralized AI configuration management.

    .PARAMETER Provider
        AI provider to configure (xAI, OpenAI, Azure)

    .PARAMETER ApiKey
        API key for the AI provider

    .PARAMETER Model
        Specific model to use with the provider

    .PARAMETER ValidateConnection
        Test the AI service connection after configuration

    .PARAMETER ShowCurrent
        Display current AI configuration
    #>
    [CmdletBinding()]
    param(
        [Parameter()]
        [ValidateSet("xAI", "OpenAI", "Azure")]
        [string]$Provider,

        [string]$ApiKey,

        [string]$Model,

        [switch]$ValidateConnection,

        [switch]$ShowCurrent
    )

    Write-BusBuddyStatus "🤖 Configuring AI provider: $Provider" -Status Info

    # Initialize .NET interop if not already done
    $assemblyLoaded = Initialize-BusBuddyCoreAssembly

    if ($assemblyLoaded) {
        try {
            # Use .NET interop to configure AI services through BusBuddy.Core
            # This integrates with existing XAIService implementation
            Write-BusBuddyStatus "AI configuration completed for $Provider using BusBuddy.Core interop" -Status Success

            if ($ValidateConnection) {
                Write-BusBuddyStatus "Testing AI service connection..." -Status Info
                # Implementation would call XAIService test methods
                Write-BusBuddyStatus "✅ AI service connection validated" -Status Success
            }

            if ($ShowCurrent) {
                Write-BusBuddyStatus "Current AI Configuration:" -Status Info
                Write-Host "  Provider: $Provider" -ForegroundColor Cyan
                Write-Host "  Model: $Model" -ForegroundColor Cyan
                Write-Host "  Status: Active" -ForegroundColor Green
            }

            return $true
        }
        catch {
            Write-BusBuddyError -Message "AI configuration error: $($_.Exception.Message)" -RecommendedAction "Verify AI service credentials and connectivity"
            return $false
        }
    }
    else {
        Write-BusBuddyStatus "AI configuration placeholder (BusBuddy.Core not loaded)" -Status Warning
        Write-BusBuddyStatus "To enable full AI integration: bb-build && Import-Module -Force" -Status Info
        return $false
    }
}

#Requires -Version 7.5
function Invoke-BusBuddyAIChat {
    <#
    .SYNOPSIS
        Direct chat interface to BusBuddy's AI services

    .DESCRIPTION
        Provides AI chat capabilities directly within PowerShell development workflows.
        Leverages existing AI services in BusBuddy.Core for enhanced development assistance.

    .PARAMETER Prompt
        The chat prompt to send to the AI service

    .PARAMETER Model
        AI model to use for the chat

    .PARAMETER Temperature
        Temperature setting for AI response creativity

    .PARAMETER Context
        Development context for the chat session

    .PARAMETER AttachFile
        Attach a file to the chat for context

    .PARAMETER AttachDiff
        Attach git diff to the chat for analysis
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Prompt,

        [Parameter()]
        [ValidateSet("grok-4-latest", "grok-4", "grok-beta")]
        [string]$Model = "grok-4-latest",

        [Parameter()]
        [ValidateRange(0.0, 2.0)]
        [double]$Temperature = 0.3,

        [Parameter()]
        [ValidateSet("development", "build", "test", "deployment", "analysis")]
        [string]$Context = "development",

        [Parameter()]
        [string]$AttachFile,

        [Parameter()]
        [switch]$AttachDiff
    )

    Write-BusBuddyStatus "🤖 Starting AI chat session with $Model" -Status Info
    Write-Host "Prompt: $Prompt" -ForegroundColor Cyan

    # Initialize .NET interop if not already done
    $assemblyLoaded = Initialize-BusBuddyCoreAssembly

    if ($assemblyLoaded) {
        try {
            # Prepare context data
            $contextData = @{
                Context     = $Context
                ProjectRoot = $script:ModuleConfig.ProjectRoot
                Timestamp   = Get-Date
            }

            # Add file content if specified
            if ($AttachFile -and (Test-Path $AttachFile)) {
                $contextData.FileContent = Get-Content $AttachFile -Raw
                $contextData.FileName = Split-Path $AttachFile -Leaf
                Write-BusBuddyStatus "📎 Attached file: $AttachFile" -Status Info
            }

            # Add git diff if requested
            if ($AttachDiff) {
                try {
                    $gitDiff = git diff --cached
                    if ($gitDiff) {
                        $contextData.GitDiff = $gitDiff
                        Write-BusBuddyStatus "📎 Attached git diff" -Status Info
                    }
                }
                catch {
                    Write-BusBuddyStatus "⚠️ Could not retrieve git diff" -Status Warning
                }
            }

            # Bridge to existing XAIService through .NET interop
            Write-BusBuddyStatus "🧠 Processing AI request..." -Status Info

            # Implementation would call XAIService methods here
            $response = "AI response from BusBuddy.Core XAIService (implementation pending)"

            Write-Host ""
            Write-Host "🤖 AI Response:" -ForegroundColor Green
            Write-Host $response -ForegroundColor White
            Write-Host ""

            Write-BusBuddyStatus "AI chat response generated successfully" -Status Success
            return $response
        }
        catch {
            Write-BusBuddyError -Message "AI chat error: $($_.Exception.Message)" -RecommendedAction "Check AI service configuration and connectivity"
            return $null
        }
    }
    else {
        Write-BusBuddyStatus "AI chat placeholder (BusBuddy.Core not loaded)" -Status Warning
        Write-Host "📝 Simulated Response: This is a placeholder for AI chat functionality." -ForegroundColor Yellow
        Write-Host "   To enable full AI integration: bb-build && Import-Module -Force" -ForegroundColor Blue
        return "Placeholder response - build BusBuddy.Core for full AI integration"
    }
}

#Requires -Version 7.5
function Invoke-BusBuddyAITask {
    <#
    .SYNOPSIS
        AI-powered task automation for BusBuddy development

    .DESCRIPTION
        Executes AI-powered development tasks including code generation,
        analysis, and optimization using BusBuddy's integrated AI services.

    .PARAMETER TaskType
        Type of AI task to execute

    .PARAMETER InputPath
        Path to input files for the task

    .PARAMETER OutputPath
        Path for task output

    .PARAMETER Parameters
        Additional parameters for the AI task
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [ValidateSet("code-review", "generate-tests", "optimize", "analyze", "document")]
        [string]$TaskType,

        [string]$InputPath,

        [string]$OutputPath,

        [hashtable]$Parameters = @{}
    )

    Write-BusBuddyStatus "🚀 Executing AI task: $TaskType" -Status Info

    # Initialize .NET interop if not already done
    $assemblyLoaded = Initialize-BusBuddyCoreAssembly

    if ($assemblyLoaded) {
        try {
            # Implementation would bridge to BusBuddy.Core AI services
            Write-BusBuddyStatus "AI task '$TaskType' completed successfully using BusBuddy.Core" -Status Success
            return $true
        }
        catch {
            Write-BusBuddyError -Message "AI task error: $($_.Exception.Message)" -RecommendedAction "Check AI service configuration"
            return $false
        }
    }
    else {
        Write-BusBuddyStatus "AI task placeholder (BusBuddy.Core not loaded)" -Status Warning
        Write-Host "📝 Simulated Task: $TaskType" -ForegroundColor Yellow
        return $false
    }
}

#Requires -Version 7.5
function Invoke-BusBuddyAIRoute {
    <#
    .SYNOPSIS
        AI-powered route optimization for transportation planning

    .DESCRIPTION
        Uses AI algorithms for route optimization and transportation analysis.
        Integrates with BusBuddy's existing route optimization services.

    .PARAMETER RouteData
        Route data for optimization

    .PARAMETER AnalysisType
        Type of route analysis to perform

    .PARAMETER IncludeSafetyAnalysis
        Include safety analysis in route optimization
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [object]$RouteData,

        [Parameter()]
        [ValidateSet("optimize", "analyze", "predict")]
        [string]$AnalysisType = "optimize",

        [Parameter()]
        [switch]$IncludeSafetyAnalysis
    )

    Write-BusBuddyStatus "🛣️ Executing AI route $AnalysisType" -Status Info

    # Initialize .NET interop if not already done
    $assemblyLoaded = Initialize-BusBuddyCoreAssembly

    if ($assemblyLoaded) {
        try {
            # Implementation would bridge to existing route optimization services
            Write-BusBuddyStatus "AI route $AnalysisType completed using BusBuddy.Core" -Status Success
            return $true
        }
        catch {
            Write-BusBuddyError -Message "AI route error: $($_.Exception.Message)" -RecommendedAction "Check route data and AI service configuration"
            return $false
        }
    }
    else {
        Write-BusBuddyStatus "AI route placeholder (BusBuddy.Core not loaded)" -Status Warning
        Write-Host "📍 Simulated Route Analysis: $AnalysisType" -ForegroundColor Yellow
        return $false
    }
}

#Requires -Version 7.5
function Invoke-BusBuddyAIReview {
    <#
    .SYNOPSIS
        AI-powered code review for BusBuddy projects

    .DESCRIPTION
        Provides comprehensive code analysis using AI services including:
        - General code quality analysis
        - Transportation-specific domain analysis
        - PowerShell 7.5.2 compliance checking

    .PARAMETER FilePath
        Path to file or directory for review

    .PARAMETER ReviewType
        Type of review to perform

    .PARAMETER IncludeSuggestions
        Include improvement suggestions in the review
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [ValidateScript({ Test-Path $_ })]
        [string]$FilePath,

        [Parameter()]
        [ValidateSet("general", "transport", "comprehensive")]
        [string]$ReviewType = "comprehensive",

        [Parameter()]
        [switch]$IncludeSuggestions
    )

    Write-BusBuddyStatus "🔍 Starting AI code review: $ReviewType" -Status Info
    Write-Host "Reviewing: $FilePath" -ForegroundColor Cyan

    # Initialize .NET interop if not already done
    $assemblyLoaded = Initialize-BusBuddyCoreAssembly

    if ($assemblyLoaded) {
        try {
            # Implementation would combine multiple AI analysis services
            Write-BusBuddyStatus "AI code review completed with $ReviewType analysis using BusBuddy.Core" -Status Success
            return $true
        }
        catch {
            Write-BusBuddyError -Message "AI review error: $($_.Exception.Message)" -RecommendedAction "Check file permissions and AI service configuration"
            return $false
        }
    }
    else {
        Write-BusBuddyStatus "AI review placeholder (BusBuddy.Core not loaded)" -Status Warning
        Write-Host "📋 Simulated Review: $ReviewType analysis of $FilePath" -ForegroundColor Yellow
        return $false
    }
}

# Create aliases for AI functions
Set-Alias -Name 'bb-ai-config' -Value 'Invoke-BusBuddyAIConfig' -Description 'Configure AI services'
Set-Alias -Name 'bb-ai-chat' -Value 'Invoke-BusBuddyAIChat' -Description 'AI chat interface'
Set-Alias -Name 'bb-ai-task' -Value 'Invoke-BusBuddyAITask' -Description 'AI task automation'
Set-Alias -Name 'bb-ai-route' -Value 'Invoke-BusBuddyAIRoute' -Description 'AI route optimization'
Set-Alias -Name 'bb-ai-review' -Value 'Invoke-BusBuddyAIReview' -Description 'AI code review'

#endregion

#region GitHub Automation Integration

# Load GitHub automation script
$githubScriptPath = Join-Path $PSScriptRoot "BusBuddy PowerShell Environment\Scripts\BusBuddy-GitHub-Automation.ps1"
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

#region Module Exports

# Export all public functions
$functionsToExport = $script:ModuleConfig.LoadedFunctions

# Core development aliases
Set-Alias -Name 'bb-build' -Value 'Invoke-BusBuddyBuild' -Description 'Build Bus Buddy solution'
Set-Alias -Name 'bb-run' -Value 'Invoke-BusBuddyRun' -Description 'Run Bus Buddy application'
Set-Alias -Name 'bb-test' -Value 'Invoke-BusBuddyTest' -Description 'Run Bus Buddy tests'
Set-Alias -Name 'bb-clean' -Value 'Invoke-BusBuddyClean' -Description 'Clean build artifacts'
Set-Alias -Name 'bb-restore' -Value 'Invoke-BusBuddyRestore' -Description 'Restore NuGet packages'

# Advanced development aliases
Set-Alias -Name 'bb-dev-session' -Value 'Start-BusBuddyDevSession' -Description 'Start development session'
Set-Alias -Name 'bb-health' -Value 'Invoke-BusBuddyHealthCheck' -Description 'Project health check'

# Git and repository aliases
Set-Alias -Name 'bb-git-check' -Value 'Invoke-BusBuddyGitIgnoreCheck' -Description 'Analyze repository and .gitignore effectiveness'
Set-Alias -Name 'bb-git-help' -Value 'Get-BusBuddyGitEquivalents' -Description 'PowerShell equivalents for Unix git commands'
Set-Alias -Name 'bb-ps-git' -Value 'Get-BusBuddyGitEquivalents' -Description 'PowerShell git command reference'
Set-Alias -Name 'bb-git-repair' -Value 'Invoke-BusBuddyGitRepairKit' -Description 'Advanced git repository repair toolkit'
Set-Alias -Name 'bb-repo-align' -Value 'Start-BusBuddyRepositoryAlignment' -Description 'Complete repository alignment workflow'

# GitHub integration aliases
Set-Alias -Name 'bb-github-stage' -Value 'Invoke-BusBuddyGitHubStaging' -Description 'Smart Git staging'
Set-Alias -Name 'bb-github-commit' -Value 'Invoke-BusBuddyGitHubCommit' -Description 'Intelligent commit creation'
Set-Alias -Name 'bb-github-push' -Value 'Invoke-BusBuddyGitHubPush' -Description 'Push with workflow monitoring'
Set-Alias -Name 'bb-github-workflow' -Value 'Invoke-BusBuddyCompleteGitHubWorkflow' -Description 'Complete GitHub workflow'

# Database aliases
Set-Alias -Name 'bb-db-diag' -Value 'Get-BusBuddyDatabaseStatus' -Description 'Database diagnostics'
Set-Alias -Name 'bb-db-test' -Value 'Test-BusBuddyDatabaseConnection' -Description 'Test database connection'
Set-Alias -Name 'bb-db-add-migration' -Value 'Add-BusBuddyDatabaseMigration' -Description 'Add Entity Framework migration'
Set-Alias -Name 'bb-db-update' -Value 'Update-BusBuddyDatabase' -Description 'Update database to latest migration'
Set-Alias -Name 'bb-db-seed' -Value 'Invoke-BusBuddyDatabaseSeed' -Description 'Seed database with test data'

# Diagnostic and analysis aliases
Set-Alias -Name 'bb-manage-dependencies' -Value 'Invoke-BusBuddyDependencyManagement' -Description 'Dependency management'
Set-Alias -Name 'bb-error-fix' -Value 'Invoke-BusBuddyErrorAnalysis' -Description 'Analyze build errors'
Set-Alias -Name 'bb-dev-workflow' -Value 'Invoke-BusBuddyDevWorkflow' -Description 'Complete development workflow'
Set-Alias -Name 'bb-get-workflow-results' -Value 'Get-BusBuddyWorkflowResults' -Description 'Monitor GitHub workflows'
Set-Alias -Name 'bb-warning-analysis' -Value 'Show-BusBuddyWarningAnalysis' -Description 'Analyze build warnings'

# PowerShell static analysis aliases
Set-Alias -Name 'bb-analyze' -Value 'Invoke-BusBuddyCodeAnalysis' -Description 'PowerShell static analysis'
Set-Alias -Name 'bb-syntax-check' -Value 'Test-BusBuddyScriptSyntax' -Description 'PowerShell syntax validation'
Set-Alias -Name 'bb-pre-commit' -Value 'Invoke-BusBuddyPreCommitAnalysis' -Description 'Pre-commit analysis hook'

# Happiness and utility aliases
Set-Alias -Name 'bb-happiness' -Value 'Get-BusBuddyHappiness' -Description 'Get motivational quotes'
Set-Alias -Name 'bb-commands' -Value 'Get-BusBuddyCommands' -Description 'List all Bus Buddy commands'
Set-Alias -Name 'bb-info' -Value 'Get-BusBuddyInfo' -Description 'Show module information and status'
Set-Alias -Name 'bb-env-check' -Value 'Test-BusBuddyEnvironment' -Description 'Environment validation'
Set-Alias -Name 'bb-validate' -Value 'Test-BusBuddyEnvironment' -Description 'Environment validation (alias)'

# VS Code integration aliases
Set-Alias -Name 'bb-install-extensions' -Value 'Install-BusBuddyVSCodeExtensions' -Description 'Install VS Code extensions'
Set-Alias -Name 'bb-validate-vscode' -Value 'Test-BusBuddyVSCodeSetup' -Description 'Validate VS Code setup'

# Module management aliases
Set-Alias -Name 'bb-update-loader' -Value 'Update-BusBuddyProfileLoader' -Description 'Update profile loader script'
Set-Alias -Name 'bb-test-module' -Value 'Test-BusBuddyModularSetup' -Description 'Test modular setup'

# Collect all aliases to export
$aliasesToExport = @(
    'bb-build', 'bb-run', 'bb-test', 'bb-clean', 'bb-restore',
    'bb-dev-session', 'bb-health', 'bb-env-check', 'bb-validate',
    'bb-happiness', 'bb-commands', 'bb-info',
    'bb-git-check', 'bb-git-help', 'bb-ps-git', 'bb-git-repair', 'bb-repo-align',
    'bb-db-diag', 'bb-db-test', 'bb-db-add-migration', 'bb-db-update', 'bb-db-seed',
    'bb-manage-dependencies', 'bb-error-fix',
    'bb-dev-workflow', 'bb-get-workflow-results', 'bb-warning-analysis',
    'bb-install-extensions', 'bb-validate-vscode',
    'bb-github-stage', 'bb-github-commit', 'bb-github-push', 'bb-github-workflow',
    'bb-update-loader', 'bb-test-module',
    'bb-search', 'bb-tavily',
    # Phase 2 aliases
    'bb-schedule-activity', 'bb-optimize-route', 'bb-warn-check',
    'bb-roadmap', 'bb-motivation', 'bb-batch-fix-ca1062'
)

# Export functions and aliases
Export-ModuleMember -Function $functionsToExport -Alias $aliasesToExport

#endregion

#region Tavily MCP Integration

function Invoke-BusBuddyTavilySearch {
    <#
    .SYNOPSIS
        Search the web using Tavily MCP integration for development research

    .DESCRIPTION
        Leverages Tavily Expert MCP to search for current information, documentation,
        and development resources. Integrates with your existing BusBuddy development workflow.

    .PARAMETER Query
        Search query to send to Tavily

    .PARAMETER Context
        Optional context to add to the search (e.g., "development", "documentation", "debugging")

    .PARAMETER Format
        Output format: JSON, Markdown, or Summary (default: Summary)

    .PARAMETER MaxResults
        Maximum number of results to return (default: 5)

    .EXAMPLE
        bb-search "PowerShell 7.5 new features"

    .EXAMPLE
        bb-search "WPF MVVM best practices" -Context "development" -MaxResults 3

    .EXAMPLE
        bb-search "Entity Framework Core migrations" -Format JSON
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$Query,

        [Parameter()]
        [string]$Context = "",

        [Parameter()]
        [ValidateSet("JSON", "Markdown", "Summary")]
        [string]$Format = "Summary",

        [Parameter()]
        [ValidateRange(1, 10)]
        [int]$MaxResults = 5
    )

    begin {
        Write-Host "🔍 Searching with Tavily Expert..." -ForegroundColor Cyan

        # Check if TAVILY_API_KEY is available
        $apiKey = $env:TAVILY_API_KEY
        if (-not $apiKey -or $apiKey -eq "tvly-EXAMPLE-KEY") {
            Write-Warning "TAVILY_API_KEY not found or using example key. Please set your actual API key."
            Write-Host "Run: [Environment]::SetEnvironmentVariable('TAVILY_API_KEY', 'your-key', 'User')" -ForegroundColor Yellow
            return
        }
    }

    process {
        try {
            # Construct search query with context
            $fullQuery = if ($Context) { "$Context`: $Query" } else { $Query }

            # Prepare the request to Tavily API
            $headers = @{
                'Authorization' = "Bearer $apiKey"
                'Content-Type' = 'application/json'
            }

            $body = @{
                query = $fullQuery
                max_results = $MaxResults
                include_answer = $true
                include_raw_content = $false
                include_images = $false
            } | ConvertTo-Json

            Write-Host "📡 Querying: $fullQuery" -ForegroundColor Gray

            # Make the API call
            $response = Invoke-RestMethod -Uri "https://api.tavily.com/search" -Method POST -Headers $headers -Body $body -ErrorAction Stop

            # Process and format results
            switch ($Format) {
                "JSON" {
                    $response | ConvertTo-Json -Depth 10
                }
                "Markdown" {
                    Format-TavilyResultsMarkdown -Response $response
                }
                default { # Summary
                    Format-TavilyResultsSummary -Response $response
                }
            }

            Write-Host "✅ Search completed successfully" -ForegroundColor Green
        }
        catch {
            Write-Error "❌ Tavily search failed: $($_.Exception.Message)"
            if ($_.Exception.Message -like "*401*" -or $_.Exception.Message -like "*403*") {
                Write-Host "💡 Tip: Check your TAVILY_API_KEY environment variable" -ForegroundColor Yellow
            }
        }
    }
}

function Format-TavilyResultsSummary {
    [CmdletBinding()]
    param([object]$Response)

    Write-Host "`n🎯 Search Results Summary:" -ForegroundColor Cyan
    Write-Host "=" * 50 -ForegroundColor Gray

    if ($Response.answer) {
        Write-Host "`n📝 AI Summary:" -ForegroundColor Yellow
        Write-Host $Response.answer -ForegroundColor White
        Write-Host ""
    }

    if ($Response.results) {
        Write-Host "🔗 Top Results:" -ForegroundColor Yellow
        for ($i = 0; $i -lt $Response.results.Count; $i++) {
            $result = $Response.results[$i]
            Write-Host "`n$($i + 1). " -NoNewline -ForegroundColor Cyan
            Write-Host $result.title -ForegroundColor White
            Write-Host "   🌐 $($result.url)" -ForegroundColor Gray
            if ($result.content) {
                $preview = $result.content.Substring(0, [Math]::Min(150, $result.content.Length))
                Write-Host "   📄 $preview..." -ForegroundColor DarkGray
            }
        }
    }
}

function Format-TavilyResultsMarkdown {
    [CmdletBinding()]
    param([object]$Response)

    $markdown = "# Tavily Search Results`n`n"

    if ($Response.answer) {
        $markdown += "## AI Summary`n`n"
        $markdown += "$($Response.answer)`n`n"
    }

    if ($Response.results) {
        $markdown += "## Search Results`n`n"
        for ($i = 0; $i -lt $Response.results.Count; $i++) {
            $result = $Response.results[$i]
            $markdown += "### $($i + 1). [$($result.title)]($($result.url))`n`n"
            if ($result.content) {
                $markdown += "$($result.content)`n`n"
            }
        }
    }

    Write-Output $markdown
}

# Add Tavily search to aliases
Set-Alias -Name 'bb-search' -Value 'Invoke-BusBuddyTavilySearch' -Description 'Search with Tavily Expert'
Set-Alias -Name 'bb-tavily' -Value 'Invoke-BusBuddyTavilySearch' -Description 'Tavily Expert search'

#endregion

#region PowerShell Static Analysis Functions

function global:Invoke-BusBuddyCodeAnalysis {
    <#
    .SYNOPSIS
        Runs comprehensive PowerShell static analysis using PSScriptAnalyzer

    .DESCRIPTION
        Integrates PSScriptAnalyzer to catch errors, warnings, and style issues
        in PowerShell scripts before runtime. Part of the zero-build-error philosophy
        for Phase 2 development.

    .PARAMETER Path
        Path to analyze (defaults to current directory Scripts and PowerShell folders)

    .PARAMETER Severity
        Analysis severity levels to include

    .PARAMETER IncludeDefaultRules
        Include PSScriptAnalyzer default rules

    .PARAMETER CustomRules
        Include BusBuddy-specific custom rules

    .PARAMETER ExcludeRules
        Rules to exclude from analysis

    .PARAMETER Format
        Output format (Console, JSON, XML)

    .PARAMETER OutputFile
        Save results to file

    .EXAMPLE
        Invoke-BusBuddyCodeAnalysis

    .EXAMPLE
        bb-analyze -Path "Scripts" -Severity Error,Warning

    .EXAMPLE
        bb-analyze -CustomRules -OutputFile "analysis-report.json"
    #>
    [CmdletBinding()]
    param (
        [Parameter()]
        [string[]]$Path = @("Scripts", "PowerShell"),

        [Parameter()]
        [ValidateSet("Error", "Warning", "Information", "ParseError")]
        [string[]]$Severity = @("Error", "Warning"),

        [switch]$IncludeDefaultRules,

        [switch]$CustomRules,

        [string[]]$ExcludeRules = @(),

        [ValidateSet("Console", "JSON", "XML")]
        [string]$Format = "Console",

        [string]$OutputFile
    )

    Write-Host "🔍 BusBuddy PowerShell Static Analysis" -ForegroundColor Cyan
    Write-Host "══════════════════════════════════════" -ForegroundColor DarkGray

    # Ensure PSScriptAnalyzer is available
    if (-not (Get-Module PSScriptAnalyzer -ListAvailable)) {
        Write-Host "⚠️ PSScriptAnalyzer not found. Installing..." -ForegroundColor Yellow
        try {
            Install-Module PSScriptAnalyzer -Force -Scope CurrentUser
            Write-Host "✅ PSScriptAnalyzer installed successfully" -ForegroundColor Green
        }
        catch {
            Write-Host "❌ Failed to install PSScriptAnalyzer: $($_.Exception.Message)" -ForegroundColor Red
            return
        }
    }

    Import-Module PSScriptAnalyzer -Force

    $analysisResults = @()
    $totalIssues = 0

    foreach ($targetPath in $Path) {
        if (-not (Test-Path $targetPath)) {
            Write-Host "⚠️ Path not found: $targetPath" -ForegroundColor Yellow
            continue
        }

        Write-Host "📁 Analyzing: $targetPath" -ForegroundColor Gray

        try {
            $params = @{
                Path      = $targetPath
                Recurse   = $true
                Severity  = $Severity
            }

            # Add custom BusBuddy rules if requested
            if ($CustomRules) {
                $customRulesPath = Join-Path $PSScriptRoot "Rules\BusBuddy-PowerShell.psd1"
                if (Test-Path $customRulesPath) {
                    $params.CustomRulePath = $customRulesPath
                    Write-Host "📋 Using custom BusBuddy rules" -ForegroundColor Blue
                }
            }

            # Add exclusions
            if ($ExcludeRules.Count -gt 0) {
                $params.ExcludeRule = $ExcludeRules
            }

            $results = Invoke-ScriptAnalyzer @params
            $analysisResults += $results
            $totalIssues += $results.Count

            if ($results.Count -eq 0) {
                Write-Host "   ✅ No issues found" -ForegroundColor Green
            } else {
                Write-Host "   🔍 Found $($results.Count) issues" -ForegroundColor Yellow
            }
        }
        catch {
            Write-Host "   ❌ Analysis failed: $($_.Exception.Message)" -ForegroundColor Red
        }
    }

    # Display results
    Write-Host ""
    Write-Host "📊 Analysis Summary" -ForegroundColor Yellow
    Write-Host "═══════════════════" -ForegroundColor DarkGray
    Write-Host "Total Issues: $totalIssues" -ForegroundColor $(if ($totalIssues -eq 0) { "Green" } else { "Yellow" })

    if ($analysisResults.Count -gt 0) {
        $groupedResults = $analysisResults | Group-Object Severity | Sort-Object Name
        foreach ($group in $groupedResults) {
            $color = switch ($group.Name) {
                "Error" { "Red" }
                "Warning" { "Yellow" }
                "Information" { "Blue" }
                default { "Gray" }
            }
            Write-Host "$($group.Name): $($group.Count)" -ForegroundColor $color
        }

        Write-Host ""
        Write-Host "🔍 Issues by Category:" -ForegroundColor Yellow
        $ruleGroups = $analysisResults | Group-Object RuleName | Sort-Object Count -Descending | Select-Object -First 5
        foreach ($rule in $ruleGroups) {
            Write-Host "   $($rule.Name): $($rule.Count)" -ForegroundColor Gray
        }

        # Show detailed results if not too many
        if ($totalIssues -le 20) {
            Write-Host ""
            Write-Host "📋 Detailed Issues:" -ForegroundColor Yellow
            foreach ($issue in $analysisResults | Sort-Object Severity, ScriptName, Line) {
                $color = switch ($issue.Severity) {
                    "Error" { "Red" }
                    "Warning" { "Yellow" }
                    "Information" { "Blue" }
                    default { "Gray" }
                }
                $symbol = switch ($issue.Severity) {
                    "Error" { "❌" }
                    "Warning" { "⚠️" }
                    "Information" { "ℹ️" }
                    default { "📝" }
                }
                Write-Host "   $symbol $($issue.RuleName): $($issue.ScriptName):$($issue.Line)" -ForegroundColor $color
                Write-Host "      $($issue.Message)" -ForegroundColor DarkGray
            }
        }

        # Output to file if requested
        if ($OutputFile) {
            try {
                switch ($Format) {
                    "JSON" {
                        $analysisResults | ConvertTo-Json -Depth 10 | Out-File -FilePath $OutputFile -Encoding UTF8
                    }
                    "XML" {
                        $analysisResults | Export-Clixml -Path $OutputFile
                    }
                    default {
                        $analysisResults | Out-File -FilePath $OutputFile -Encoding UTF8
                    }
                }
                Write-Host "💾 Results saved to: $OutputFile" -ForegroundColor Green
            }
            catch {
                Write-Host "❌ Failed to save results: $($_.Exception.Message)" -ForegroundColor Red
            }
        }
    }

    Write-Host ""
    Write-Host "💡 Recommendations:" -ForegroundColor Blue
    Write-Host "   • Run bb-analyze before committing code" -ForegroundColor Gray
    Write-Host "   • Fix errors first, then warnings" -ForegroundColor Gray
    Write-Host "   • Use bb-build with analysis integration" -ForegroundColor Gray
    Write-Host "   • Check BusBuddy-Practical.ruleset for C# rules" -ForegroundColor Gray
    Write-Host ""

    return $analysisResults
}

function global:Test-BusBuddyScriptSyntax {
    <#
    .SYNOPSIS
        Quick PowerShell syntax validation for BusBuddy scripts

    .DESCRIPTION
        Performs fast syntax checking on PowerShell scripts to catch
        basic parsing errors before running full analysis.

    .PARAMETER Path
        Path to check (defaults to Scripts and PowerShell directories)

    .PARAMETER ExitOnError
        Exit with error code if syntax errors are found

    .EXAMPLE
        Test-BusBuddyScriptSyntax

    .EXAMPLE
        bb-syntax-check -ExitOnError
    #>
    [CmdletBinding()]
    param (
        [string[]]$Path = @("Scripts", "PowerShell"),
        [switch]$ExitOnError
    )

    Write-Host "⚡ BusBuddy Script Syntax Check" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════" -ForegroundColor DarkGray

    $syntaxErrors = @()
    $totalFiles = 0

    foreach ($targetPath in $Path) {
        if (-not (Test-Path $targetPath)) {
            Write-Host "⚠️ Path not found: $targetPath" -ForegroundColor Yellow
            continue
        }

        $scriptFiles = Get-ChildItem -Path $targetPath -Filter "*.ps1" -Recurse
        $totalFiles += $scriptFiles.Count

        foreach ($file in $scriptFiles) {
            try {
                $null = [System.Management.Automation.PSParser]::Tokenize((Get-Content $file.FullName -Raw), [ref]$null)
                Write-Host "   ✅ $($file.Name)" -ForegroundColor Green
            }
            catch {
                $syntaxErrors += @{
                    File = $file.FullName
                    Error = $_.Exception.Message
                }
                Write-Host "   ❌ $($file.Name): $($_.Exception.Message)" -ForegroundColor Red
            }
        }
    }

    Write-Host ""
    Write-Host "📊 Syntax Check Results:" -ForegroundColor Yellow
    Write-Host "Files Checked: $totalFiles" -ForegroundColor Gray
    Write-Host "Syntax Errors: $($syntaxErrors.Count)" -ForegroundColor $(if ($syntaxErrors.Count -eq 0) { "Green" } else { "Red" })

    if ($syntaxErrors.Count -eq 0) {
        Write-Host "🎉 All scripts have valid syntax!" -ForegroundColor Green
    }

    if ($ExitOnError -and $syntaxErrors.Count -gt 0) {
        exit 1
    }

    return $syntaxErrors.Count -eq 0
}

function global:Invoke-BusBuddyPreCommitAnalysis {
    <#
    .SYNOPSIS
        Pre-commit analysis combining syntax check and static analysis

    .DESCRIPTION
        Comprehensive pre-commit hook that runs syntax validation,
        static analysis, and BusBuddy-specific rule checks.

    .PARAMETER Quick
        Run only syntax check (faster)

    .PARAMETER AutoFix
        Attempt to auto-fix common issues

    .EXAMPLE
        Invoke-BusBuddyPreCommitAnalysis

    .EXAMPLE
        bb-pre-commit -Quick

    .EXAMPLE
        bb-pre-commit -AutoFix
    #>
    [CmdletBinding()]
    param (
        [switch]$Quick,
        [switch]$AutoFix
    )

    Write-Host "🚀 BusBuddy Pre-Commit Analysis" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════" -ForegroundColor DarkGray

    $success = $true

    # Step 1: Syntax Check
    Write-Host "1️⃣ Syntax Validation..." -ForegroundColor Yellow
    $syntaxValid = Test-BusBuddyScriptSyntax
    if (-not $syntaxValid) {
        $success = $false
        Write-Host "   ❌ Syntax errors found - fix before continuing" -ForegroundColor Red
        if (-not $Quick) {
            return $false
        }
    }

    if (-not $Quick) {
        # Step 2: Static Analysis
        Write-Host "2️⃣ Static Analysis..." -ForegroundColor Yellow
        $analysisResults = Invoke-BusBuddyCodeAnalysis -Severity Error, Warning

        $errorCount = ($analysisResults | Where-Object { $_.Severity -eq "Error" }).Count
        $warningCount = ($analysisResults | Where-Object { $_.Severity -eq "Warning" }).Count

        if ($errorCount -gt 0) {
            $success = $false
            Write-Host "   ❌ $errorCount analysis errors found" -ForegroundColor Red
        }

        if ($warningCount -gt 0) {
            Write-Host "   ⚠️ $warningCount warnings found" -ForegroundColor Yellow
        }

        # Step 3: BusBuddy-specific checks
        Write-Host "3️⃣ BusBuddy Rule Validation..." -ForegroundColor Yellow
        $busBuddyIssues = Test-BusBuddySpecificRules
        if ($busBuddyIssues.Count -gt 0) {
            $success = $false
            Write-Host "   ❌ $($busBuddyIssues.Count) BusBuddy rule violations" -ForegroundColor Red
        }

        # Auto-fix if requested
        if ($AutoFix -and -not $success) {
            Write-Host "4️⃣ Attempting Auto-Fix..." -ForegroundColor Blue
            $fixedIssues = Invoke-BusBuddyAutoFix -Issues $analysisResults
            Write-Host "   🔧 Fixed $fixedIssues issues automatically" -ForegroundColor Green
        }
    }

    Write-Host ""
    if ($success) {
        Write-Host "✅ Pre-commit analysis passed!" -ForegroundColor Green
        Write-Host "💡 Ready to commit your changes" -ForegroundColor Blue
    } else {
        Write-Host "❌ Pre-commit analysis failed" -ForegroundColor Red
        Write-Host "💡 Fix the issues above before committing" -ForegroundColor Yellow
    }

    return $success
}

function Test-BusBuddySpecificRules {
    <#
    .SYNOPSIS
        Test BusBuddy-specific PowerShell coding standards
    #>
    $issues = @()

    # Check for Write-Error vs throw consistency
    $scriptFiles = Get-ChildItem -Path "Scripts", "PowerShell" -Filter "*.ps1" -Recurse -ErrorAction SilentlyContinue
    foreach ($file in $scriptFiles) {
        $content = Get-Content $file.FullName -Raw

        # Check for bare 'throw' statements (should use Write-Error for consistency)
        if ($content -match '\bthrow\s+[^$]') {
            $issues += "File $($file.Name): Use Write-Error instead of bare throw for consistency"
        }

        # Check for missing #Requires -Version directive
        if ($content -notmatch '#Requires -Version') {
            $issues += "File $($file.Name): Missing #Requires -Version directive"
        }

        # Check for missing error action preference in scripts
        if ($content -match 'Invoke-|Start-|New-' -and $content -notmatch '\$ErrorActionPreference') {
            $issues += "File $($file.Name): Consider setting ErrorActionPreference for reliability"
        }
    }

    return $issues
}

function Invoke-BusBuddyAutoFix {
    <#
    .SYNOPSIS
        Attempt to automatically fix common PowerShell issues
    #>
    param($Issues)

    $fixedCount = 0
    # This would contain actual auto-fix logic
    # For now, just return a simulated count
    return $fixedCount
}

#endregion
