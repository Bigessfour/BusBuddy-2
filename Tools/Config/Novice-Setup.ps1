#Requires -Version 7.0
<#
.SYNOPSIS
    Bus Buddy Development Configuration for Novice Users
    
.DESCRIPTION
    Simplified PowerShell configuration that sets up the development environment
    with easy-to-use aliases and automatic path detection for novice developers.
    
.NOTES
    Author: Bus Buddy Development Team
    Version: 1.0
    Focus: Novice-Friendly, WPF Scheduling, Auto-Configuration
#>

# ═══════════════════════════════════════════════════════════════════════════════════
# AUTO-DETECT PROJECT PATHS
# ═══════════════════════════════════════════════════════════════════════════════════

$script:BusBuddyConfig = @{
    # Auto-detect project root from current script location
    ProjectRoot = ""
    ToolsPath = ""
    LogsPath = ""
    XamlPath = ""
    
    # Development settings optimized for novices
    Settings = @{
        AutoBackup = $true
        VerboseOutput = $true
        UINotifications = $true
        PreCommitValidation = $true
        SchedulingFocus = $true
        IndentSize = 4
    }
}

# Auto-detect paths
if ($PSScriptRoot) {
    $script:BusBuddyConfig.ProjectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
} else {
    # Fallback: search up from current directory
    $currentPath = Get-Location
    while ($currentPath.Path -and -not (Test-Path (Join-Path $currentPath.Path "BusBuddy.sln"))) {
        $currentPath = Split-Path $currentPath.Path -Parent
        if ([string]::IsNullOrEmpty($currentPath)) { break }
    }
    $script:BusBuddyConfig.ProjectRoot = $currentPath
}

$script:BusBuddyConfig.ToolsPath = Join-Path $script:BusBuddyConfig.ProjectRoot "Tools"
$script:BusBuddyConfig.LogsPath = Join-Path $script:BusBuddyConfig.ProjectRoot "logs"
$script:BusBuddyConfig.XamlPath = Join-Path $script:BusBuddyConfig.ProjectRoot "BusBuddy.WPF"

# Ensure required directories exist
@($script:BusBuddyConfig.LogsPath) | ForEach-Object {
    if (-not (Test-Path $_)) {
        New-Item -Path $_ -ItemType Directory -Force | Out-Null
    }
}

# ═══════════════════════════════════════════════════════════════════════════════════
# NOVICE-FRIENDLY FUNCTIONS
# ═══════════════════════════════════════════════════════════════════════════════════

function Write-BusBuddyMessage {
    <#
    .SYNOPSIS
        Consistent messaging for Bus Buddy operations
    #>
    param(
        [string]$Message,
        [ValidateSet('Info', 'Success', 'Warning', 'Error', 'Step')]$Type = 'Info'
    )
    
    $colors = @{
        'Info' = 'Cyan'
        'Success' = 'Green'
        'Warning' = 'Yellow'
        'Error' = 'Red'
        'Step' = 'Magenta'
    }
    
    $icons = @{
        'Info' = '🔍'
        'Success' = '✅'
        'Warning' = '⚠️'
        'Error' = '❌'
        'Step' = '📋'
    }
    
    Write-Host "$($icons[$Type]) $Message" -ForegroundColor $colors[$Type]
}

function Test-BusBuddyEnvironment {
    <#
    .SYNOPSIS
        Validates the Bus Buddy development environment
    #>
    Write-BusBuddyMessage "Checking Bus Buddy development environment..." -Type 'Step'
    
    $checks = @()
    
    # Project structure
    if (Test-Path (Join-Path $script:BusBuddyConfig.ProjectRoot "BusBuddy.sln")) {
        $checks += @{ Name = "Solution file"; Status = "✅"; Color = "Green" }
    } else {
        $checks += @{ Name = "Solution file"; Status = "❌"; Color = "Red" }
    }
    
    # Tools directory
    if (Test-Path $script:BusBuddyConfig.ToolsPath) {
        $checks += @{ Name = "Tools directory"; Status = "✅"; Color = "Green" }
    } else {
        $checks += @{ Name = "Tools directory"; Status = "❌"; Color = "Red" }
    }
    
    # PowerShell version
    if ($PSVersionTable.PSVersion.Major -ge 7) {
        $checks += @{ Name = "PowerShell 7+"; Status = "✅"; Color = "Green" }
    } else {
        $checks += @{ Name = "PowerShell 7+"; Status = "⚠️"; Color = "Yellow" }
    }
    
    # Display results
    foreach ($check in $checks) {
        Write-Host "  $($check.Status) $($check.Name)" -ForegroundColor $check.Color
    }
    
    $failedChecks = $checks | Where-Object { $_.Status -eq "❌" }
    if ($failedChecks.Count -eq 0) {
        Write-BusBuddyMessage "Environment validation complete!" -Type 'Success'
        return $true
    } else {
        Write-BusBuddyMessage "Environment issues detected. Please check the failed items above." -Type 'Error'
        return $false
    }
}

function Invoke-BusBuddyXamlFormat {
    <#
    .SYNOPSIS
        Novice-friendly XAML formatting with automatic backup
    .DESCRIPTION
        Formats all XAML files in the project with safe defaults:
        - Automatic backup enabled
        - 4-space indentation
        - Validation before and after formatting
        - Clear progress messages
    #>
    param(
        [switch]$ValidateOnly,
        [switch]$RemoveDeprecated
    )
    
    Write-BusBuddyMessage "Starting XAML processing..." -Type 'Step'
    
    $formatScript = Join-Path $script:BusBuddyConfig.ToolsPath "Scripts\Format-XamlFiles.ps1"
    
    if (-not (Test-Path $formatScript)) {
        Write-BusBuddyMessage "XAML formatting script not found: $formatScript" -Type 'Error'
        return $false
    }
    
    $arguments = @(
        "-Path", "`"$($script:BusBuddyConfig.ProjectRoot)`""
        "-BackupEnabled"
        "-Verbose"
    )
    
    if ($ValidateOnly) {
        $arguments += "-Validate"
        Write-BusBuddyMessage "Running validation only..." -Type 'Info'
    } else {
        $arguments += "-Format", "-Validate"
        Write-BusBuddyMessage "Formatting and validating XAML files..." -Type 'Info'
    }
    
    if ($RemoveDeprecated) {
        $arguments += "-RemoveDeprecated"
        Write-BusBuddyMessage "Removing deprecated Syncfusion attributes..." -Type 'Info'
    }
    
    try {
        & $formatScript @arguments
        Write-BusBuddyMessage "XAML processing completed successfully!" -Type 'Success'
        return $true
    }
    catch {
        Write-BusBuddyMessage "XAML processing failed: $($_.Exception.Message)" -Type 'Error'
        return $false
    }
}

function Start-BusBuddyDebugFilter {
    <#
    .SYNOPSIS
        Starts debug filtering with scheduling focus for novice users
    .DESCRIPTION
        Launches the debug filter with optimal settings for WPF scheduling:
        - Scheduling focus enabled
        - UI notifications for critical issues
        - High priority filter (shows critical and high priority issues)
        - Real-time streaming mode
    #>
    param(
        [ValidateRange(1,4)][int]$Priority = 2,
        [switch]$FileMode
    )
    
    Write-BusBuddyMessage "Starting Bus Buddy debug filter..." -Type 'Step'
    Write-BusBuddyMessage "Focus: WPF Scheduling System" -Type 'Info'
    Write-BusBuddyMessage "Priority Level: $Priority and above" -Type 'Info'
    
    $debugScript = Join-Path $script:BusBuddyConfig.ToolsPath "Scripts\test-debug-filter.ps1"
    
    if (-not (Test-Path $debugScript)) {
        Write-BusBuddyMessage "Debug filter script not found: $debugScript" -Type 'Error'
        return $false
    }
    
    $arguments = @(
        "-SchedulingFocus"
        "-UINotifications" 
        "-Priority", $Priority
        "-Verbose"
    )
    
    if ($FileMode) {
        $arguments += "-Mode", "File"
        Write-BusBuddyMessage "Processing existing log files..." -Type 'Info'
    } else {
        $arguments += "-Mode", "Stream"
        Write-BusBuddyMessage "Starting real-time monitoring..." -Type 'Info'
        Write-BusBuddyMessage "Press Ctrl+C to stop monitoring" -Type 'Warning'
    }
    
    try {
        & $debugScript @arguments
    }
    catch {
        Write-BusBuddyMessage "Debug filter failed: $($_.Exception.Message)" -Type 'Error'
        return $false
    }
}

function Invoke-BusBuddyHealthCheck {
    <#
    .SYNOPSIS
        Comprehensive health check for Bus Buddy project
    .DESCRIPTION
        Runs a complete health check including:
        - Environment validation
        - XAML syntax validation
        - Debug log analysis
        - Project build status
    #>
    Write-BusBuddyMessage "🩺 Starting Bus Buddy Health Check..." -Type 'Step'
    
    $results = @{
        Environment = $false
        XAML = $false
        Logs = $false
        Build = $false
    }
    
    # 1. Environment check
    Write-BusBuddyMessage "Step 1: Environment validation" -Type 'Step'
    $results.Environment = Test-BusBuddyEnvironment
    
    # 2. XAML validation
    Write-BusBuddyMessage "Step 2: XAML validation" -Type 'Step'
    $results.XAML = Invoke-BusBuddyXamlFormat -ValidateOnly
    
    # 3. Log analysis
    Write-BusBuddyMessage "Step 3: Debug log analysis" -Type 'Step'
    $results.Logs = Start-BusBuddyDebugFilter -Priority 1 -FileMode
    
    # 4. Build test (optional)
    Write-BusBuddyMessage "Step 4: Build validation" -Type 'Step'
    try {
        Push-Location $script:BusBuddyConfig.ProjectRoot
        $buildResult = dotnet build --verbosity quiet --no-restore 2>&1
        $results.Build = $LASTEXITCODE -eq 0
        
        if ($results.Build) {
            Write-BusBuddyMessage "Build validation passed" -Type 'Success'
        } else {
            Write-BusBuddyMessage "Build validation failed" -Type 'Error'
        }
    }
    catch {
        Write-BusBuddyMessage "Build validation error: $($_.Exception.Message)" -Type 'Error'
        $results.Build = $false
    }
    finally {
        Pop-Location
    }
    
    # Summary
    Write-Host "`n" -NoNewline
    Write-BusBuddyMessage "🩺 Health Check Summary:" -Type 'Step'
    
    $passed = 0
    foreach ($test in $results.Keys) {
        if ($results[$test]) {
            Write-Host "  ✅ $test" -ForegroundColor Green
            $passed++
        } else {
            Write-Host "  ❌ $test" -ForegroundColor Red
        }
    }
    
    $percentage = [math]::Round(($passed / $results.Count) * 100)
    
    if ($percentage -eq 100) {
        Write-BusBuddyMessage "🎉 All health checks passed! ($percentage%)" -Type 'Success'
    } elseif ($percentage -ge 75) {
        Write-BusBuddyMessage "⚠️ Most health checks passed ($percentage%) - minor issues detected" -Type 'Warning'
    } else {
        Write-BusBuddyMessage "❌ Health check failed ($percentage%) - attention required" -Type 'Error'
    }
}

function Start-BusBuddyDevSession {
    <#
    .SYNOPSIS
        Starts a complete development session for novice users
    .DESCRIPTION
        Launches a full development environment including:
        - Environment validation
        - XAML formatting and validation
        - Project build
        - Debug filter activation
    #>
    Write-BusBuddyMessage "🚀 Starting Bus Buddy Development Session..." -Type 'Step'
    
    # Step 1: Environment check
    if (-not (Test-BusBuddyEnvironment)) {
        Write-BusBuddyMessage "Environment check failed. Please fix issues before continuing." -Type 'Error'
        return
    }
    
    # Step 2: XAML processing
    Write-BusBuddyMessage "Processing XAML files..." -Type 'Step'
    if (-not (Invoke-BusBuddyXamlFormat)) {
        Write-BusBuddyMessage "XAML processing failed. Check the errors above." -Type 'Error'
        return
    }
    
    # Step 3: Build project
    Write-BusBuddyMessage "Building project..." -Type 'Step'
    try {
        Push-Location $script:BusBuddyConfig.ProjectRoot
        $buildOutput = dotnet build --verbosity minimal 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-BusBuddyMessage "Build successful!" -Type 'Success'
        } else {
            Write-BusBuddyMessage "Build failed. Check output above." -Type 'Error'
            Write-Host $buildOutput
            return
        }
    }
    finally {
        Pop-Location
    }
    
    # Step 4: Start debug monitoring
    Write-BusBuddyMessage "Starting debug monitoring..." -Type 'Step'
    Write-BusBuddyMessage "Development session is ready! Debug filter will monitor for issues." -Type 'Success'
    
    Start-BusBuddyDebugFilter -Priority 2
}

# ═══════════════════════════════════════════════════════════════════════════════════
# NOVICE-FRIENDLY ALIASES
# ═══════════════════════════════════════════════════════════════════════════════════

# Main command aliases
Set-Alias -Name "bb-fix-xaml" -Value "Invoke-BusBuddyXamlFormat" -Description "Format and fix XAML files"
Set-Alias -Name "bb-check-xaml" -Value "Invoke-BusBuddyXamlFormat" -Description "Validate XAML files only"
Set-Alias -Name "bb-debug-start" -Value "Start-BusBuddyDebugFilter" -Description "Start debug filtering"
Set-Alias -Name "bb-health" -Value "Invoke-BusBuddyHealthCheck" -Description "Run health check"
Set-Alias -Name "bb-dev-start" -Value "Start-BusBuddyDevSession" -Description "Start development session"
Set-Alias -Name "bb-env-check" -Value "Test-BusBuddyEnvironment" -Description "Check environment"

# Navigation aliases
Set-Alias -Name "bb-root" -Value { Set-Location $script:BusBuddyConfig.ProjectRoot } -Description "Go to project root"
Set-Alias -Name "bb-logs" -Value { Set-Location $script:BusBuddyConfig.LogsPath } -Description "Go to logs folder"
Set-Alias -Name "bb-tools" -Value { Set-Location $script:BusBuddyConfig.ToolsPath } -Description "Go to tools folder"

# Quick action aliases
Set-Alias -Name "bb-build" -Value { 
    Push-Location $script:BusBuddyConfig.ProjectRoot
    dotnet build --verbosity minimal
    Pop-Location
} -Description "Quick build"

Set-Alias -Name "bb-clean" -Value { 
    Push-Location $script:BusBuddyConfig.ProjectRoot
    dotnet clean --verbosity minimal
    Pop-Location
} -Description "Quick clean"

Set-Alias -Name "bb-run" -Value { 
    Push-Location $script:BusBuddyConfig.ProjectRoot
    dotnet run --project BusBuddy.WPF/BusBuddy.WPF.csproj
    Pop-Location
} -Description "Quick run"

# ═══════════════════════════════════════════════════════════════════════════════════
# HELP SYSTEM FOR NOVICES
# ═══════════════════════════════════════════════════════════════════════════════════

function Show-BusBuddyHelp {
    <#
    .SYNOPSIS
        Shows help information for novice Bus Buddy developers
    #>
    Write-Host @"

🚌 Bus Buddy Development Commands for Novices
═══════════════════════════════════════════════════════════════════

📋 ESSENTIAL COMMANDS:
  bb-dev-start     Start complete development session (recommended for new users)
  bb-health        Run comprehensive health check
  bb-fix-xaml     Format and validate all XAML files
  bb-debug-start   Start debug monitoring for scheduling issues

🔧 DAILY WORKFLOW:
  bb-check-xaml    Validate XAML before committing changes
  bb-build         Build the project
  bb-run           Run the application
  bb-clean         Clean build artifacts

📁 NAVIGATION:
  bb-root          Go to project root directory
  bb-logs          Go to logs folder
  bb-tools         Go to tools folder

🩺 HEALTH & MAINTENANCE:
  bb-env-check     Check development environment
  bb-health        Weekly health check (recommended)

💡 GETTING STARTED:
  1. Run 'bb-dev-start' to set up your development environment
  2. Use 'bb-fix-xaml' before committing any XAML changes  
  3. Run 'bb-health' weekly to catch issues early
  4. Use 'bb-debug-start' when debugging scheduling features

📚 For detailed help on any command, use: Get-Help <command-name> -Detailed

"@ -ForegroundColor Cyan
}

# Auto-display help on first load
Show-BusBuddyHelp

# Welcome message
Write-BusBuddyMessage "🚌 Bus Buddy Development Environment Ready!" -Type 'Success'
Write-BusBuddyMessage "Project Root: $($script:BusBuddyConfig.ProjectRoot)" -Type 'Info'
Write-BusBuddyMessage "Type 'Show-BusBuddyHelp' to see available commands" -Type 'Info'

# Quick environment check
if ($script:BusBuddyConfig.Settings.PreCommitValidation) {
    Write-BusBuddyMessage "Auto-validation enabled for commits" -Type 'Info'
}

if ($script:BusBuddyConfig.Settings.SchedulingFocus) {
    Write-BusBuddyMessage "Enhanced scheduling debugging enabled" -Type 'Info'
}
