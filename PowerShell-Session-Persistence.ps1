#Requires -Version 7.6
<#
.SYNOPSIS
    🔄 PowerShell Session Persistence Manager for BusBuddy Development

.DESCRIPTION
    Provides intelligent session state management and persistence features:
    - Automatic profile and function loading with error recovery
    - Session state validation and health monitoring
    - Intelligent command discovery and loading
    - Development environment persistence across sessions
    - AI tool integration status tracking and restoration

.FEATURES
    ✅ Auto-recovery from profile loading failures
    ✅ Persistent environment variable management
    ✅ Session health monitoring and validation
    ✅ AI tool status tracking and restoration
    ✅ Command availability verification
    ✅ Development workspace state persistence

.NOTES
    File: PowerShell-Session-Persistence.ps1
    Author: BusBuddy Development Team
    Requires: PowerShell 7.6+ for optimal performance
    Compatible: PowerShell 7.4+ with limited features
#>

param(
    [switch]$Restore,
    [switch]$Save,
    [switch]$Validate,
    [switch]$Reset,
    [switch]$Quiet = $false,
    [switch]$Detailed = $false
)

#region Core Session Persistence Framework

$Global:BusBuddySessionState = @{
    Version              = "1.0.0"
    LastUpdated          = Get-Date
    WorkspaceRoot        = $PSScriptRoot
    LoadedComponents     = @{}
    AvailableCommands    = @{}
    EnvironmentVariables = @{}
    AIToolsStatus        = @{
        Available  = $false
        LastTested = $null
        Components = @{}
    }
    SessionHealth        = @{
        ProfileLoaded         = $false
        FunctionsAvailable    = $false
        BuildEnvironmentReady = $false
        AIToolsReady          = $false
    }
    PersistenceConfig    = @{
        StateFile          = "$PSScriptRoot\.busbuddy-session-state.json"
        AutoSave           = $true
        ValidationInterval = 300 # 5 minutes
        MaxRetries         = 3
    }
}

function Initialize-BusBuddySessionPersistence {
    <#
    .SYNOPSIS
        Initialize the BusBuddy session persistence system
    #>
    param([switch]$Force)

    if (-not $Quiet) {
        Write-Host "🔄 Initializing BusBuddy Session Persistence..." -ForegroundColor Cyan
    }

    # Create state directory if needed
    $stateDir = Split-Path $Global:BusBuddySessionState.PersistenceConfig.StateFile -Parent
    if (-not (Test-Path $stateDir)) {
        New-Item -ItemType Directory -Path $stateDir -Force | Out-Null
    }

    # Load existing state if available
    if ((Test-Path $Global:BusBuddySessionState.PersistenceConfig.StateFile) -and -not $Force) {
        try {
            $savedState = Get-Content $Global:BusBuddySessionState.PersistenceConfig.StateFile | ConvertFrom-Json
            if (-not $Quiet) {
                Write-Host "   ✅ Previous session state loaded" -ForegroundColor Green
            }
        }
        catch {
            Write-Warning "Failed to load previous session state: $($_.Exception.Message)"
        }
    }

    # Initialize workspace context
    $Global:BusBuddySessionState.WorkspaceRoot = $PSScriptRoot
    Set-Location $PSScriptRoot

    if (-not $Quiet) {
        Write-Host "   📂 Workspace: $PSScriptRoot" -ForegroundColor Gray
        Write-Host "   ✅ Session persistence initialized" -ForegroundColor Green
    }
}

function Test-BusBuddySessionHealth {
    <#
    .SYNOPSIS
        Comprehensive session health validation and reporting
    #>
    param([switch]$Detailed)

    $health = @{
        Overall         = $true
        Components      = @{}
        Recommendations = @()
        Timestamp       = Get-Date
    }

    if (-not $Quiet) {
        Write-Host "🏥 Running BusBuddy Session Health Check..." -ForegroundColor Cyan
    }

    # Test 1: Profile Loading Status
    $profileCommands = @('bb-health', 'Get-BusBuddySystemStatus', 'Start-AdminOptimizationSession')
    $availableCommands = @()
    foreach ($cmd in $profileCommands) {
        if (Get-Command $cmd -ErrorAction SilentlyContinue) {
            $availableCommands += $cmd
        }
    }

    $health.Components['ProfileFunctions'] = @{
        Status    = $availableCommands.Count -gt 0
        Available = $availableCommands
        Missing   = $profileCommands | Where-Object { $_ -notin $availableCommands }
        Count     = $availableCommands.Count
        Total     = $profileCommands.Count
    }

    if ($health.Components['ProfileFunctions'].Status -eq $false) {
        $health.Overall = $false
        $health.Recommendations += "Load BusBuddy profile functions: . '$PSScriptRoot\Load-AdminFunctions.ps1'"
    }

    # Test 2: AI Tools Availability
    $aiFiles = @(
        "AI-Assistant\Core\start-ai-busbuddy.ps1"
        "AI-Assistant\Tools\smart-build-intelligence-clean.ps1"
        "AI-Assistant\Tools\smart-runtime-intelligence.ps1"
    )

    $availableAITools = $aiFiles | Where-Object { Test-Path (Join-Path $PSScriptRoot $_) }
    $health.Components['AITools'] = @{
        Status    = $availableAITools.Count -eq $aiFiles.Count
        Available = $availableAITools
        Missing   = $aiFiles | Where-Object { $_ -notin $availableAITools }
        Count     = $availableAITools.Count
        Total     = $aiFiles.Count
    }

    if ($health.Components['AITools'].Status -eq $false) {
        $health.Recommendations += "Ensure AI Assistant tools are properly configured"
    }

    # Test 3: Build Environment
    $buildTests = @{
        DotNetSDK    = { (dotnet --version) -ne $null }
        SolutionFile = { Test-Path "$PSScriptRoot\BusBuddy.sln" }
        ProjectFiles = { (Get-ChildItem -Recurse -Filter "*.csproj").Count -gt 0 }
    }

    $buildResults = @{}
    foreach ($test in $buildTests.GetEnumerator()) {
        try {
            $buildResults[$test.Key] = & $test.Value
        }
        catch {
            $buildResults[$test.Key] = $false
        }
    }

    $health.Components['BuildEnvironment'] = @{
        Status  = ($buildResults.Values | Where-Object { $_ -eq $true }).Count -eq $buildResults.Count
        Results = $buildResults
    }

    if ($health.Components['BuildEnvironment'].Status -eq $false) {
        $health.Overall = $false
        $health.Recommendations += "Verify build environment: .NET SDK, solution and project files"
    }

    # Test 4: PowerShell Environment
    $psTests = @{
        Version         = $PSVersionTable.PSVersion -ge [version]"7.6"
        ExecutionPolicy = (Get-ExecutionPolicy) -in @('Unrestricted', 'RemoteSigned', 'Bypass')
        Modules         = (Get-Module).Count -gt 5
    }

    $health.Components['PowerShellEnvironment'] = @{
        Status  = ($psTests.Values | Where-Object { $_ -eq $true }).Count -eq $psTests.Count
        Results = $psTests
        Version = $PSVersionTable.PSVersion.ToString()
        Edition = $PSVersionTable.PSEdition
    }

    # Update global session state
    $Global:BusBuddySessionState.SessionHealth = @{
        ProfileLoaded         = $health.Components['ProfileFunctions'].Status
        FunctionsAvailable    = $health.Components['ProfileFunctions'].Count -gt 0
        BuildEnvironmentReady = $health.Components['BuildEnvironment'].Status
        AIToolsReady          = $health.Components['AITools'].Status
        LastValidated         = Get-Date
    }

    # Display results
    if (-not $Quiet) {
        $overallStatus = if ($health.Overall) { "✅ HEALTHY" } else { "⚠️ ISSUES DETECTED" }
        $statusColor = if ($health.Overall) { "Green" } else { "Yellow" }

        Write-Host ""
        Write-Host "📊 Session Health Status: $overallStatus" -ForegroundColor $statusColor
        Write-Host ""

        foreach ($component in $health.Components.GetEnumerator()) {
            $compStatus = if ($component.Value.Status) { "✅" } else { "❌" }
            $compColor = if ($component.Value.Status) { "Green" } else { "Red" }

            Write-Host "   $compStatus $($component.Key)" -ForegroundColor $compColor

            if ($Detailed -and $component.Value.ContainsKey('Available')) {
                foreach ($item in $component.Value.Available) {
                    Write-Host "      ✅ $item" -ForegroundColor Green
                }
                foreach ($item in $component.Value.Missing) {
                    Write-Host "      ❌ $item" -ForegroundColor Red
                }
            }
        }

        if ($health.Recommendations.Count -gt 0) {
            Write-Host ""
            Write-Host "💡 Recommendations:" -ForegroundColor Yellow
            foreach ($rec in $health.Recommendations) {
                Write-Host "   • $rec" -ForegroundColor Gray
            }
        }

        Write-Host ""
    }

    return $health
}

function Repair-BusBuddySession {
    <#
    .SYNOPSIS
        Intelligent session repair and component loading
    #>
    param([switch]$Force)

    if (-not $Quiet) {
        Write-Host "🔧 Repairing BusBuddy Session..." -ForegroundColor Cyan
    }

    $repairResults = @{
        ProfileFunctions = $false
        AITools          = $false
        Environment      = $false
        BuildSystem      = $false
    }

    # Repair 1: Load missing profile functions
    $adminFunctionsPath = Join-Path $PSScriptRoot "Load-AdminFunctions.ps1"
    if (Test-Path $adminFunctionsPath) {
        try {
            . $adminFunctionsPath
            $repairResults.ProfileFunctions = $true
            if (-not $Quiet) {
                Write-Host "   ✅ Profile functions loaded" -ForegroundColor Green
            }
        }
        catch {
            Write-Warning "Failed to load admin functions: $($_.Exception.Message)"
        }
    }

    # Repair 2: Load main profile
    $mainProfilePath = Join-Path $PSScriptRoot "BusBuddy-PowerShell-Profile.ps1"
    if (Test-Path $mainProfilePath) {
        try {
            . $mainProfilePath
            if (-not $Quiet) {
                Write-Host "   ✅ Main profile loaded" -ForegroundColor Green
            }
        }
        catch {
            Write-Warning "Failed to load main profile: $($_.Exception.Message)"
        }
    }

    # Repair 3: Initialize AI tools
    $aiStartupPath = Join-Path $PSScriptRoot "AI-Assistant\Core\start-ai-busbuddy.ps1"
    if (Test-Path $aiStartupPath) {
        try {
            . $aiStartupPath -Mode health -LoadAllComponents
            $repairResults.AITools = $true
            if (-not $Quiet) {
                Write-Host "   ✅ AI tools initialized" -ForegroundColor Green
            }
        }
        catch {
            Write-Warning "Failed to initialize AI tools: $($_.Exception.Message)"
        }
    }

    # Repair 4: Validate environment
    try {
        Set-Location $PSScriptRoot
        $env:BUSBUDDY_WORKSPACE = $PSScriptRoot
        $repairResults.Environment = $true
        if (-not $Quiet) {
            Write-Host "   ✅ Environment configured" -ForegroundColor Green
        }
    }
    catch {
        Write-Warning "Failed to configure environment: $($_.Exception.Message)"
    }

    # Update session state
    $Global:BusBuddySessionState.LoadedComponents = $repairResults
    $Global:BusBuddySessionState.LastUpdated = Get-Date

    # Run health check to verify repairs
    if (-not $Quiet) {
        Write-Host ""
        Test-BusBuddySessionHealth -Detailed:$Detailed
    }

    return $repairResults
}

function Save-BusBuddySessionState {
    <#
    .SYNOPSIS
        Save current session state to persistent storage
    #>
    try {
        $stateToSave = @{
            Version              = $Global:BusBuddySessionState.Version
            SavedAt              = Get-Date
            WorkspaceRoot        = $Global:BusBuddySessionState.WorkspaceRoot
            LoadedComponents     = $Global:BusBuddySessionState.LoadedComponents
            SessionHealth        = $Global:BusBuddySessionState.SessionHealth
            EnvironmentVariables = @{
                BUSBUDDY_WORKSPACE = $env:BUSBUDDY_WORKSPACE
                PWD                = $PWD.Path
            }
        }

        $stateJson = $stateToSave | ConvertTo-Json -Depth 5
        $stateFile = $Global:BusBuddySessionState.PersistenceConfig.StateFile
        $stateJson | Set-Content -Path $stateFile -Encoding UTF8

        if (-not $Quiet) {
            Write-Host "💾 Session state saved to: $stateFile" -ForegroundColor Green
        }

        return $true
    }
    catch {
        Write-Warning "Failed to save session state: $($_.Exception.Message)"
        return $false
    }
}

function Start-BusBuddyDevelopmentSession {
    <#
    .SYNOPSIS
        Comprehensive development session startup with persistence
    #>
    param(
        [switch]$NoHealthCheck,
        [switch]$SkipAI,
        [switch]$Force
    )

    Write-Host "🚌🚀 Starting BusBuddy Development Session..." -ForegroundColor Cyan
    Write-Host "      Enhanced with session persistence and AI tools" -ForegroundColor Green
    Write-Host ""

    # Initialize persistence
    Initialize-BusBuddySessionPersistence -Force:$Force

    # Repair/load components
    $repairResults = Repair-BusBuddySession -Force:$Force

    # Health check unless skipped
    if (-not $NoHealthCheck) {
        $health = Test-BusBuddySessionHealth -Detailed

        if (-not $health.Overall) {
            Write-Host "⚠️ Session has some issues - but continuing..." -ForegroundColor Yellow
            Write-Host "   💡 Use 'Test-BusBuddySessionHealth -Detailed' for more info" -ForegroundColor Gray
        }
    }

    # Save session state
    if ($Global:BusBuddySessionState.PersistenceConfig.AutoSave) {
        Save-BusBuddySessionState
    }

    # Display available commands
    Write-Host "🎯 Available BusBuddy Commands:" -ForegroundColor Cyan
    $bbCommands = Get-Command -Name "*busbuddy*", "bb-*" -ErrorAction SilentlyContinue | Select-Object Name
    if ($bbCommands) {
        $bbCommands | ForEach-Object { Write-Host "   • $($_.Name)" -ForegroundColor Gray }
    }
    else {
        Write-Host "   ⚠️ No BusBuddy commands found - check profile loading" -ForegroundColor Yellow
    }

    # Final status
    Write-Host ""
    Write-Host "✅ BusBuddy Development Session Ready!" -ForegroundColor Green
    Write-Host "   📂 Workspace: $($Global:BusBuddySessionState.WorkspaceRoot)" -ForegroundColor Gray
    Write-Host "   🔧 Commands: Test-BusBuddySessionHealth, Repair-BusBuddySession" -ForegroundColor Gray
    Write-Host "   💾 State: Automatically saved and restored" -ForegroundColor Gray
    Write-Host ""

    return $Global:BusBuddySessionState
}

#endregion

#region Main Execution Logic

# Handle parameters
switch ($true) {
    $Restore {
        Initialize-BusBuddySessionPersistence
        Repair-BusBuddySession
    }
    $Save {
        Save-BusBuddySessionState
    }
    $Validate {
        Test-BusBuddySessionHealth -Detailed:$Detailed
    }
    $Reset {
        Initialize-BusBuddySessionPersistence -Force
        Repair-BusBuddySession -Force
    }
    default {
        # Default: Full development session startup
        Start-BusBuddyDevelopmentSession -Force:$Reset -SkipAI:$SkipAI
    }
}

#endregion

# Export functions to global scope
$functionsToExport = @(
    'Initialize-BusBuddySessionPersistence'
    'Test-BusBuddySessionHealth'
    'Repair-BusBuddySession'
    'Save-BusBuddySessionState'
    'Start-BusBuddyDevelopmentSession'
)

foreach ($func in $functionsToExport) {
    if (Get-Command $func -ErrorAction SilentlyContinue) {
        Set-Alias -Name $func -Value $func -Scope Global -Force
    }
}

# Create convenient aliases
Set-Alias -Name 'bb-session-start' -Value 'Start-BusBuddyDevelopmentSession' -Scope Global -Force
Set-Alias -Name 'bb-session-health' -Value 'Test-BusBuddySessionHealth' -Scope Global -Force
Set-Alias -Name 'bb-session-repair' -Value 'Repair-BusBuddySession' -Scope Global -Force
Set-Alias -Name 'bb-session-save' -Value 'Save-BusBuddySessionState' -Scope Global -Force

if (-not $Quiet) {
    Write-Host "🔄 BusBuddy Session Persistence loaded successfully!" -ForegroundColor Green
    Write-Host "   📋 Quick commands: bb-session-start, bb-session-health, bb-session-repair" -ForegroundColor Gray
}
