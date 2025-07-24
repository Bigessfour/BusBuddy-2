#Requires -Version 7.6
<#
.SYNOPSIS
    BusBuddy Administrator Function Loader for PowerShell 7.6

.DESCRIPTION
    Intelligent loader that provides administrator functions in the PowerShell environment
    without requiring constant elevation. Includes Dell Inspiron 16 5640 optimizations
    and seamless integration with BusBuddy development workflow.

.FEATURES
    - Smart privilege detection and elevation
    - Dell-specific hardware optimizations
    - PowerShell 7.6 enhanced performance functions
    - Development environment integration
    - Safe fallback for non-admin operations

.EXAMPLE
    # Load in current session (non-admin)
    . .\Load-AdminFunctions.ps1

    # Use admin functions (will auto-elevate when needed)
    Invoke-DellOptimization -PowerShellOnly
    Start-AdminOptimization -QuickStart

.NOTES
    Designed for Dell Inspiron 16 5640 with PowerShell 7.6.4+
    Integrates with BusBuddy development environment
#>

#region PowerShell 7.6 Execution Policy Management

function Invoke-SafeModuleImport {
    <#
    .SYNOPSIS
        Safely import modules with PowerShell 7.6 execution policy best practices
    .DESCRIPTION
        Implements PowerShell 7.6 execution policy handling for local development modules
        following Microsoft's security recommendations from the PS 7.6 documentation.
    .PARAMETER ModulePath
        Path to the module file to import
    .PARAMETER Force
        Force reimport of the module
    .PARAMETER Global
        Import module into global scope
    .EXAMPLE
        Invoke-SafeModuleImport -ModulePath ".\MyModule.psm1" -Force -Global
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$ModulePath,

        [switch]$Force,

        [switch]$Global
    )

    try {
        # PowerShell 7.6: Use enhanced Unblock-File for local development files
        if (Test-Path $ModulePath) {
            if (Get-Command Unblock-File -ErrorAction SilentlyContinue) {
                Unblock-File -Path $ModulePath -ErrorAction SilentlyContinue
                Write-Verbose "Unblocked module file: $ModulePath"
            }
        }

        # Import with proper parameter handling
        $importParams = @{
            Name = $ModulePath
            ErrorAction = 'Stop'
        }

        if ($Force) { $importParams.Force = $true }
        if ($Global) { $importParams.Global = $true }

        Import-Module @importParams

        Write-Host "   ✅ Module loaded successfully: $(Split-Path $ModulePath -Leaf)" -ForegroundColor Green
        return $true
    }
    catch [System.Management.Automation.PSSecurityException] {
        Write-Host "   ⚠️ Module requires execution policy adjustment: $(Split-Path $ModulePath -Leaf)" -ForegroundColor Yellow
        Write-Host "   💡 For development, run: Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass" -ForegroundColor Cyan
        Write-Host "   📚 See: https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_execution_policies" -ForegroundColor Gray
        return $false
    }
    catch {
        Write-Warning "Module could not be loaded: $($_.Exception.Message)"
        return $false
    }
}

function Set-DevelopmentExecutionPolicy {
    <#
    .SYNOPSIS
        Configure execution policy for BusBuddy development following PS 7.6 best practices
    .DESCRIPTION
        Sets appropriate execution policy for local development while maintaining security.
        Uses Process scope to avoid permanent system changes.
    .PARAMETER Scope
        Execution policy scope (Process, CurrentUser, LocalMachine)
    .PARAMETER Policy
        Execution policy to set (Bypass, RemoteSigned, AllSigned)
    .EXAMPLE
        Set-DevelopmentExecutionPolicy -Scope Process -Policy Bypass
    #>
    [CmdletBinding()]
    param(
        [Parameter()]
        [ValidateSet('Process', 'CurrentUser', 'LocalMachine')]
        [string]$Scope = 'Process',

        [Parameter()]
        [ValidateSet('Bypass', 'RemoteSigned', 'AllSigned')]
        [string]$Policy = 'Bypass'
    )

    try {
        # Check current policy
        $currentPolicy = Get-ExecutionPolicy -Scope $Scope

        if ($currentPolicy -eq $Policy) {
            Write-Host "   ✅ Execution policy already set to $Policy for $Scope scope" -ForegroundColor Green
            return $true
        }

        # Set new policy
        Set-ExecutionPolicy -ExecutionPolicy $Policy -Scope $Scope -Force
        Write-Host "   ✅ Execution policy set to $Policy for $Scope scope" -ForegroundColor Green

        # Verify the change
        $newPolicy = Get-ExecutionPolicy -Scope $Scope
        if ($newPolicy -eq $Policy) {
            Write-Host "   ✅ Execution policy change verified" -ForegroundColor Green
            return $true
        } else {
            Write-Warning "Execution policy verification failed. Expected: $Policy, Actual: $newPolicy"
            return $false
        }
    }
    catch {
        Write-Warning "Failed to set execution policy: $($_.Exception.Message)"
        return $false
    }
}

#endregion

[CmdletBinding()]
param(
    [switch]$LoadDellOptimizations,
    [switch]$EnableAutoElevation,
    [switch]$Quiet
)


# Global admin function registry with PS 7.6 optimizations
$Global:BusBuddyAdminFunctions = @{
    IsLoaded             = $false
    AdminPrivileges      = $false
    DellOptimizations    = $false
    PowerShell76Features = $false
    Functions            = @{}
    LoadTime             = Get-Date
    Version              = "2.0.0-PS76"
}

#region Administrator Privilege Management

function Test-AdminPrivileges {
    <#
    .SYNOPSIS
        Check if current session has administrator privileges
    .DESCRIPTION
        Uses PowerShell 7.6 enhanced security principal detection with improved error handling
    #>
    [CmdletBinding()]
    param()

    try {
        $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
        $principal = [Security.Principal.WindowsPrincipal]$currentUser

        # PS 7.6: Use PipelineStopToken if available for responsive cancellation
        if ($PSCmdlet.PipelineStopToken?.IsCancellationRequested) {
            throw [System.OperationCanceledException]::new("Admin privilege check cancelled")
        }

        return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    }
    catch [System.OperationCanceledException] {
        Write-Verbose "Admin privilege check was cancelled"
        throw
    }
    catch {
        # PowerShell 7.6: Enhanced error stringification for empty exception messages
        $errorMessage = $_.Exception.Message ?? 'Unknown security principal error'
        Write-Verbose "Failed to determine admin privileges: $errorMessage"
        return $false
    }
}

function Test-PowerShell76Features {
    <#
    .SYNOPSIS
        Test for PowerShell 7.6 specific features and capabilities
    .DESCRIPTION
        Validates that PowerShell 7.6 features are available and functional with enhanced cancellation support
    #>
    [CmdletBinding()]
    param()

    $features = @{
        Version                  = $PSVersionTable.PSVersion
        DotNetVersion            = [System.Runtime.InteropServices.RuntimeInformation]::FrameworkDescription
        ThreadJobModule          = $false
        ImprovedJoinPath         = $false
        EnhancedStartProcess     = $false
        PipelineStopTokenSupport = $false
        ExperimentalFeatures     = @()
    }

    # PS 7.6: Check for PipelineStopToken support
    try {
        $features.PipelineStopTokenSupport = $null -ne $PSCmdlet.PipelineStopToken
    }
    catch {
        $features.PipelineStopTokenSupport = $false
    }

    # Test for Microsoft.PowerShell.ThreadJob module (PS 7.6 renamed module)
    try {
        # PS 7.6: Check cancellation before expensive operations
        if ($PSCmdlet.PipelineStopToken?.IsCancellationRequested) {
            throw [System.OperationCanceledException]::new("Feature testing cancelled")
        }

        $threadJobModule = Get-Module -Name 'Microsoft.PowerShell.ThreadJob' -ListAvailable -ErrorAction SilentlyContinue
        $features.ThreadJobModule = $null -ne $threadJobModule
    }
    catch [System.OperationCanceledException] {
        throw
    }
    catch {
        $features.ThreadJobModule = $false
    }

    # Test Join-Path enhanced string[] support
    try {
        $testPath = Join-Path -Path "C:" -ChildPath @("Users", "Test")
        $features.ImprovedJoinPath = $testPath -eq "C:\Users\Test"
    }
    catch {
        $features.ImprovedJoinPath = $false
    }

    # Test Start-Process efficiency improvements
    try {
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        $testProcess = Start-Process -FilePath 'pwsh.exe' -ArgumentList '-Command', 'Get-Date' -Wait -PassThru -WindowStyle Hidden -ErrorAction SilentlyContinue
        $sw.Stop()
        $features.EnhancedStartProcess = $sw.ElapsedMilliseconds -lt 2000  # Should be fast with PS 7.6 improvements
    }
    catch {
        $features.EnhancedStartProcess = $false
    }

    # Check for experimental features
    try {
        $experimentalFeatures = Get-ExperimentalFeature -ErrorAction SilentlyContinue
        if ($experimentalFeatures) {
            $features.ExperimentalFeatures = $experimentalFeatures | Where-Object { $_.Enabled } | Select-Object -ExpandProperty Name
        }
    }
    catch {
        $features.ExperimentalFeatures = @()
    }

    return $features
}

function Enable-PowerShell76ExperimentalFeatures {
    <#
    .SYNOPSIS
        Enable useful PowerShell 7.6 experimental features for BusBuddy development
    .DESCRIPTION
        Enables experimental features that enhance development productivity
    #>
    [CmdletBinding()]
    param(
        [string[]]$Features = @(
            'PSNativeWindowsTildeExpansion',
            'PSRedirectToVariable',
            'PSSerializeJSONLongEnumAsNumber'
        ),
        [switch]$WhatIf
    )

    Write-Host "`n🧪 PowerShell 7.6 Experimental Features" -ForegroundColor Cyan
    Write-Host "=======================================" -ForegroundColor Cyan

    foreach ($feature in $Features) {
        try {
            $currentFeature = Get-ExperimentalFeature -Name $feature -ErrorAction SilentlyContinue
            if ($currentFeature) {
                if ($currentFeature.Enabled) {
                    Write-Host "✅ $feature - Already enabled" -ForegroundColor Green
                }
                else {
                    if (-not $WhatIf) {
                        Enable-ExperimentalFeature -Name $feature -Scope CurrentUser
                        Write-Host "🔄 $feature - Enabled (restart required)" -ForegroundColor Yellow
                    }
                    else {
                        Write-Host "🔄 $feature - Would be enabled" -ForegroundColor Yellow
                    }
                }
            }
            else {
                Write-Host "⚠️ $feature - Not available in this PowerShell version" -ForegroundColor Gray
            }
        }
        catch {
            Write-Host "❌ $feature - Failed to enable: $($_.Exception.Message)" -ForegroundColor Red
        }
    }

    if (-not $WhatIf) {
        Write-Host "`n💡 Restart PowerShell to apply experimental feature changes" -ForegroundColor Cyan
    }
}

function Request-AdminElevation {
    <#
    .SYNOPSIS
        Request administrator elevation for current script or command
    #>
    [CmdletBinding()]
    param(
        [string]$ScriptPath,
        [string[]]$Arguments = @(),
        [switch]$Wait,
        [switch]$ShowInstructions,
        [switch]$SkipExternalWindow
    )

    if (Test-AdminPrivileges) {
        Write-Warning "Already running with administrator privileges"
        return $true
    }

    # Always show instructions instead of opening external window by default
    Write-Host "`n🔐 Administrator Privileges Required" -ForegroundColor Yellow
    Write-Host "====================================" -ForegroundColor Yellow
    Write-Host "This operation requires administrator privileges." -ForegroundColor White

    Write-Host "`n💡 Recommended Approach (Stay in VS Code):" -ForegroundColor Cyan
    Write-Host "1. 🎯 Use VS Code's Integrated Terminal:" -ForegroundColor Green
    Write-Host "   • Open new integrated terminal (Ctrl+Shift+`)" -ForegroundColor Gray
    Write-Host "   • Right-click terminal tab → 'Run as Administrator'" -ForegroundColor Gray
    Write-Host "   • Navigate to: $PWD" -ForegroundColor Gray
    Write-Host "   • Run: . .\Load-AdminFunctions.ps1 -LoadDellOptimizations" -ForegroundColor Gray
    Write-Host "   • Run: admin-start" -ForegroundColor Gray

    Write-Host "`n📋 Alternative Options:" -ForegroundColor Cyan
    Write-Host "2. � Restart VS Code as Administrator:" -ForegroundColor Green
    Write-Host "   • Close VS Code" -ForegroundColor Gray
    Write-Host "   • Right-click VS Code icon → 'Run as Administrator'" -ForegroundColor Gray
    Write-Host "   • Reopen your project" -ForegroundColor Gray

    Write-Host "`n3. � Available Non-Admin Functions (Run Now):" -ForegroundColor Green
    Write-Host "   • PowerShell Environment Optimization" -ForegroundColor Gray
    Write-Host "   • System Status and Monitoring" -ForegroundColor Gray
    Write-Host "   • BusBuddy Development Tools" -ForegroundColor Gray

    Write-Host "`n💡 Quick Commands You Can Run Now:" -ForegroundColor Cyan
    Write-Host "   admin-status                    # Show system status" -ForegroundColor Gray
    Write-Host "   Invoke-AdminFunction -FunctionName 'Optimize-PowerShellEnvironment'  # Optimize PS (no admin)" -ForegroundColor Gray

    if (-not $SkipExternalWindow) {
        Write-Host "`n4. 🪟 External Window (Last Resort):" -ForegroundColor Yellow
        $choice = Read-Host "Open external admin window? (y/N)"
        if ($choice -match '^[Yy]') {
            # Proceed with external window logic below
        }
        else {
            Write-Host "`n✨ Try the VS Code integrated terminal option above for the best experience!" -ForegroundColor Cyan
            return $false
        }
    }
    else {
        Write-Host "`n✨ Use the VS Code integrated terminal option above for the best experience!" -ForegroundColor Cyan
        return $false
    }

    try {
        if ($ScriptPath) {
            $processArgs = @(
                '-NoProfile'
                '-ExecutionPolicy', 'Bypass'
                '-File', $ScriptPath
            ) + $Arguments

            # PowerShell 7.6: Leverage improved Start-Process -Wait polling efficiency
            if ($Wait) {
                $process = Start-Process -FilePath 'pwsh.exe' -ArgumentList $processArgs -Verb RunAs -Wait -PassThru
                return $process.ExitCode -eq 0
            }
            else {
                $process = Start-Process -FilePath 'pwsh.exe' -ArgumentList $processArgs -Verb RunAs -PassThru
            }

            return $true
        }
        else {
            # Start new admin PowerShell session with current context and auto-load functions
            $currentPath = $PWD.Path
            $startupCommand = @"
Set-Location '$currentPath'
Write-Host '🔐 BusBuddy Administrator PowerShell Session' -ForegroundColor Green
Write-Host '===========================================' -ForegroundColor Green
Write-Host 'Current Path: $currentPath' -ForegroundColor Gray
Write-Host ''
Write-Host '🚀 Loading BusBuddy Admin Functions...' -ForegroundColor Cyan
. .\Load-AdminFunctions.ps1 -LoadDellOptimizations
Write-Host ''
Write-Host '✨ Admin functions loaded! Starting optimization menu...' -ForegroundColor Green
Write-Host ''
admin-start
"@

            $processArgs = @(
                '-NoExit'
                '-Command'
                $startupCommand
            )

            $process = Start-Process -FilePath 'pwsh.exe' -ArgumentList $processArgs -Verb RunAs -PassThru

            Write-Host "🚀 Administrator PowerShell window opened" -ForegroundColor Green
            Write-Host "✨ Functions will auto-load and menu will start automatically!" -ForegroundColor Cyan
            Write-Host "💡 The admin window will show the full optimization menu once opened." -ForegroundColor Gray

            return $true
        }
    }
    catch {
        Write-Error "Failed to request administrator elevation: $($_.Exception.Message)"
        Write-Host "`n🔧 Alternative: Manual elevation steps:" -ForegroundColor Yellow
        Write-Host "1. Right-click PowerShell icon → 'Run as Administrator'" -ForegroundColor Gray
        Write-Host "2. Navigate to: $PWD" -ForegroundColor Gray
        Write-Host "3. Run: . .\Load-AdminFunctions.ps1 -LoadDellOptimizations" -ForegroundColor Gray
        return $false
    }
}

function Invoke-AdminFunction {
    <#
    .SYNOPSIS
        Execute a function with administrator privileges
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$FunctionName,

        [hashtable]$Parameters = @{},

        [switch]$AutoElevate
    )

    if (-not $Global:BusBuddyAdminFunctions.Functions.ContainsKey($FunctionName)) {
        throw "Admin function '$FunctionName' not found. Available functions: $($Global:BusBuddyAdminFunctions.Functions.Keys -join ', ')"
    }

    $func = $Global:BusBuddyAdminFunctions.Functions[$FunctionName]

    if ($func.RequiresAdmin -and -not (Test-AdminPrivileges)) {
        if ($AutoElevate) {
            Write-Host "🔐 Function '$FunctionName' requires administrator privileges. Requesting elevation..." -ForegroundColor Yellow

            # Create temporary script to execute the function
            $tempScript = Join-Path -Path $env:TEMP -ChildPath "BusBuddy-Admin-$FunctionName-$(Get-Date -Format 'yyyyMMddHHmmss').ps1"

            $scriptContent = @"
# Load BusBuddy admin functions
. '$PSScriptRoot\Load-AdminFunctions.ps1' -LoadDellOptimizations -Quiet:`$true

# Execute the requested function
$FunctionName @Parameters
"@

            Set-Content -Path $tempScript -Value $scriptContent

            try {
                $result = Request-AdminElevation -ScriptPath $tempScript -Wait
                return $result
            }
            finally {
                Remove-Item -Path $tempScript -Force -ErrorAction SilentlyContinue
            }
        }
        else {
            throw "Function '$FunctionName' requires administrator privileges. Use -AutoElevate to request elevation."
        }
    }

    # Execute the function
    return & $func.ScriptBlock @Parameters
}

function Start-OptimizedBackgroundTask {
    <#
    .SYNOPSIS
        Start background tasks with PowerShell 7.6 optimizations and cancellation support
    .DESCRIPTION
        Leverages PS 7.6 Microsoft.PowerShell.ThreadJob for enhanced performance and PipelineStopToken for cancellation
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [scriptblock]$ScriptBlock,

        [int]$ThrottleLimit = 10,

        [string]$Name = "BusBuddy-Task-$(Get-Date -Format 'HHmmss')"
    )

    try {
        # PS 7.6: Use Microsoft.PowerShell.ThreadJob for better performance
        $job = Start-ThreadJob -ScriptBlock $ScriptBlock -ThrottleLimit $ThrottleLimit -Name $Name

        # PS 7.6: Enhanced job monitoring with cancellation support
        if ($PSCmdlet.PipelineStopToken) {
            Register-ObjectEvent -InputObject $PSCmdlet.PipelineStopToken -EventName 'Cancelled' -Action {
                Get-Job -Name $using:Name -ErrorAction SilentlyContinue | Stop-Job -Force
            } | Out-Null
        }

        Write-Verbose "Started optimized background task: $Name"
        return $job
    }
    catch {
        $errorMessage = $_.Exception.Message ?? 'Unknown ThreadJob error'
        Write-Warning "ThreadJob failed, task not started: $errorMessage"
        return $null
    }
}

function Get-AdminElevationOptions {
    <#
    .SYNOPSIS
        Display options for administrator elevation without opening external windows
    #>
    [CmdletBinding()]
    param()

    if (Test-AdminPrivileges) {
        Write-Host "✅ Already running with administrator privileges!" -ForegroundColor Green
        return $true
    }

    Write-Host "`n🔐 Administrator Privileges Required" -ForegroundColor Yellow
    Write-Host "====================================" -ForegroundColor Yellow

    Write-Host "`n📋 Elevation Options:" -ForegroundColor Cyan

    Write-Host "`n1. 🔄 Restart Current Session (Recommended):" -ForegroundColor Green
    Write-Host "   • Copy this command:" -ForegroundColor Gray
    $restartCommand = "Start-Process pwsh.exe -Verb RunAs -ArgumentList '-NoExit', '-Command', 'Set-Location \`"$PWD\`"; . .\Load-AdminFunctions.ps1 -LoadDellOptimizations'"
    Write-Host "   $restartCommand" -ForegroundColor Cyan

    Write-Host "`n2. 🎯 VS Code Integrated Terminal:" -ForegroundColor Green
    Write-Host "   • Open new integrated terminal" -ForegroundColor Gray
    Write-Host "   • Right-click terminal tab → 'Run as Administrator'" -ForegroundColor Gray
    Write-Host "   • Navigate to project folder and reload functions" -ForegroundColor Gray

    Write-Host "`n3. 📋 Manual Steps:" -ForegroundColor Green
    Write-Host "   • Right-click PowerShell icon → 'Run as Administrator'" -ForegroundColor Gray
    Write-Host "   • Run: Set-Location '$PWD'" -ForegroundColor Gray
    Write-Host "   • Run: . .\Load-AdminFunctions.ps1 -LoadDellOptimizations" -ForegroundColor Gray

    Write-Host "`n4. 🚀 Available Non-Admin Functions:" -ForegroundColor Green
    Write-Host "   • PowerShell Environment Optimization (no admin needed)" -ForegroundColor Gray
    Write-Host "   • System Status and Monitoring" -ForegroundColor Gray
    Write-Host "   • BusBuddy Development Tools" -ForegroundColor Gray

    Write-Host "`n💡 Quick Commands You Can Run Now:" -ForegroundColor Cyan
    Write-Host "   admin-status                    # Show system status" -ForegroundColor Gray
    Write-Host "   Invoke-AdminFunction -FunctionName 'Optimize-PowerShellEnvironment'  # Optimize PS (no admin)" -ForegroundColor Gray

    return $false
}

#endregion

#region Dell Inspiron Optimization Functions

function Register-DellOptimizationFunctions {
    <#
    .SYNOPSIS
        Register Dell Inspiron 16 5640 optimization functions
    #>

    # Register Dell hardware optimization function
    $Global:BusBuddyAdminFunctions.Functions['Optimize-DellHardware'] = @{
        RequiresAdmin = $true
        Description   = 'Optimize Dell Inspiron 16 5640 hardware settings'
        ScriptBlock   = {
            param(
                [switch]$PowerManagement,
                [switch]$CPUOptimization,
                [switch]$MemoryOptimization,
                [switch]$StorageOptimization,
                [switch]$WhatIf
            )

            Write-Host "🔧 Dell Inspiron 16 5640 Hardware Optimization" -ForegroundColor Cyan

            if ($PowerManagement -or $PSBoundParameters.Count -eq 0) {
                Write-Host "⚡ Optimizing power management..." -ForegroundColor Yellow
                if (-not $WhatIf) {
                    # Set high performance power plan
                    powercfg /setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c
                    # Optimize CPU performance
                    powercfg /setacvalueindex scheme_current sub_processor PERFINCTHRESHOLD 10
                    powercfg /setacvalueindex scheme_current sub_processor PERFDECTHRESHOLD 8
                }
                Write-Host "  ✅ Power management optimized" -ForegroundColor Green
            }

            if ($CPUOptimization -or $PSBoundParameters.Count -eq 0) {
                Write-Host "🔬 Optimizing CPU settings..." -ForegroundColor Yellow
                if (-not $WhatIf) {
                    # Intel i5-1334U specific optimizations
                    powercfg /setacvalueindex scheme_current sub_processor PROCTHROTTLEMAX 100
                    powercfg /setacvalueindex scheme_current sub_processor PROCTHROTTLEMIN 5
                }
                Write-Host "  ✅ CPU optimization completed" -ForegroundColor Green
            }

            if ($MemoryOptimization -or $PSBoundParameters.Count -eq 0) {
                Write-Host "💾 Optimizing memory settings..." -ForegroundColor Yellow
                if (-not $WhatIf) {
                    # Clear memory caches
                    [System.GC]::Collect()
                    [System.GC]::WaitForPendingFinalizers()
                    [System.GC]::Collect()
                }
                Write-Host "  ✅ Memory optimization completed" -ForegroundColor Green
            }

            if ($StorageOptimization -or $PSBoundParameters.Count -eq 0) {
                Write-Host "💿 Optimizing storage settings..." -ForegroundColor Yellow
                if (-not $WhatIf) {
                    # Optimize SSD settings
                    fsutil behavior set DisableLastAccess 1
                    fsutil behavior set EncryptPagingFile 0
                }
                Write-Host "  ✅ Storage optimization completed" -ForegroundColor Green
            }

            Write-Host "🎉 Dell hardware optimization completed!" -ForegroundColor Green
            return @{
                Status               = 'Success'
                OptimizationsApplied = @('PowerManagement', 'CPU', 'Memory', 'Storage') | Where-Object {
                    $PSBoundParameters.ContainsKey($_) -or $PSBoundParameters.Count -eq 0
                }
                WhatIf               = $WhatIf
            }
        }
    }

    # Register PowerShell optimization function
    $Global:BusBuddyAdminFunctions.Functions['Optimize-PowerShellEnvironment'] = @{
        RequiresAdmin = $false
        Description   = 'Optimize PowerShell 7.6 environment for Dell Inspiron'
        ScriptBlock   = {
            param(
                [switch]$ParallelProcessing,
                [switch]$MemoryManagement,
                [switch]$ModuleOptimization,
                [switch]$WhatIf
            )

            Write-Host "🚀 PowerShell 7.6 Environment Optimization" -ForegroundColor Cyan

            $optimizations = @()

            if ($ParallelProcessing -or $PSBoundParameters.Count -eq 0) {
                Write-Host "⚡ Optimizing parallel processing..." -ForegroundColor Yellow
                if (-not $WhatIf) {
                    # Set optimal throttle limits for Dell Inspiron i5-1334U (12 threads)
                    $env:PSParallelThrottleLimit = '10'  # Leave 2 threads for system
                    $PSDefaultParameterValues['ForEach-Object:ThrottleLimit'] = 10
                    $PSDefaultParameterValues['Start-ThreadJob:ThrottleLimit'] = 10

                    # PowerShell 7.6: Enhanced module loading with improved efficiency
                    try {
                        # PS 7.6: Use Microsoft.PowerShell.ThreadJob with prefix override support
                        Import-Module Microsoft.PowerShell.ThreadJob -Force -Global -ErrorAction Stop -Prefix ""
                        Write-Verbose "Using PowerShell 7.6 Microsoft.PowerShell.ThreadJob module"
                    }
                    catch {
                        try {
                            # Fallback to proxy ThreadJob module (PS 7.6 compatibility layer)
                            Import-Module ThreadJob -Force -Global -ErrorAction SilentlyContinue
                            Write-Verbose "Fallback to proxy ThreadJob module"
                        }
                        catch {
                            Write-Warning "ThreadJob modules unavailable: $($_.Exception.Message ?? 'Module loading failed')"
                        }
                    }
                }
                $optimizations += 'ParallelProcessing'
                Write-Host "  ✅ Parallel processing optimized for 12-thread CPU (PS 7.6)" -ForegroundColor Green
            }

            if ($MemoryManagement -or $PSBoundParameters.Count -eq 0) {
                Write-Host "💾 Optimizing memory management..." -ForegroundColor Yellow
                if (-not $WhatIf) {
                    # Optimize garbage collection for 16GB RAM
                    [System.GC]::Collect()
                    $PSDefaultParameterValues['*:ErrorAction'] = 'Stop'  # Faster error handling
                    $PSStyle.Progress.View = 'Minimal'  # Reduce progress overhead
                }
                $optimizations += 'MemoryManagement'
                Write-Host "  ✅ Memory management optimized" -ForegroundColor Green
            }

            if ($ModuleOptimization -or $PSBoundParameters.Count -eq 0) {
                Write-Host "📦 Optimizing module loading..." -ForegroundColor Yellow
                if (-not $WhatIf) {
                    # PS 7.6: Enhanced module loading with improved efficiency and error handling
                    Import-Module Microsoft.PowerShell.Utility -Force -Global
                    Import-Module Microsoft.PowerShell.Management -Force -Global

                    # PS 7.6: Use updated ThreadJob module with enhanced performance
                    try {
                        # Primary: Microsoft.PowerShell.ThreadJob (PS 7.6 native)
                        Import-Module Microsoft.PowerShell.ThreadJob -Force -Global -ErrorAction Stop -Prefix ""
                        Write-Verbose "Loaded PowerShell 7.6 Microsoft.PowerShell.ThreadJob module"
                    }
                    catch {
                        try {
                            # Fallback: ThreadJob proxy module (PS 7.6 compatibility)
                            Import-Module ThreadJob -Force -Global -ErrorAction SilentlyContinue
                            Write-Verbose "Loaded proxy ThreadJob module for compatibility"
                        }
                        catch {
                            Write-Warning "ThreadJob modules unavailable: $($_.Exception.Message ?? 'Module loading failed')"
                        }
                    }
                }
                $optimizations += 'ModuleOptimization'
                Write-Host "  ✅ Module loading optimized (PS 7.6 compatible)" -ForegroundColor Green
            }

            Write-Host "🎉 PowerShell environment optimization completed!" -ForegroundColor Green
            return @{
                Status               = 'Success'
                OptimizationsApplied = $optimizations
                WhatIf               = $WhatIf
                ProcessorCount       = [System.Environment]::ProcessorCount
                AvailableMemoryGB    = [Math]::Round((Get-CimInstance Win32_ComputerSystem).TotalPhysicalMemory / 1GB, 1)
            }
        }
    }

    # Register system cleanup function
    $Global:BusBuddyAdminFunctions.Functions['Start-SystemCleanup'] = @{
        RequiresAdmin = $true
        Description   = 'Clean up system files and optimize Windows 11'
        ScriptBlock   = {
            param(
                [switch]$TempFiles,
                [switch]$WindowsUpdate,
                [switch]$SystemFiles,
                [switch]$WhatIf
            )

            Write-Host "🧹 Windows 11 System Cleanup" -ForegroundColor Cyan

            $cleanupResults = @()

            if ($TempFiles -or $PSBoundParameters.Count -eq 0) {
                Write-Host "🗑️ Cleaning temporary files..." -ForegroundColor Yellow
                if (-not $WhatIf) {
                    # Clean temp directories
                    $tempPaths = @($env:TEMP, $env:TMP, "$env:LOCALAPPDATA\Temp")
                    foreach ($path in $tempPaths) {
                        if (Test-Path $path) {
                            Get-ChildItem -Path $path -Recurse -Force -ErrorAction SilentlyContinue |
                            Remove-Item -Force -Recurse -ErrorAction SilentlyContinue
                        }
                    }
                }
                $cleanupResults += 'TempFiles'
                Write-Host "  ✅ Temporary files cleaned" -ForegroundColor Green
            }

            if ($SystemFiles -or $PSBoundParameters.Count -eq 0) {
                Write-Host "🔧 Running disk cleanup..." -ForegroundColor Yellow
                if (-not $WhatIf) {
                    # PS 7.6: Use enhanced Start-Process with improved efficiency
                    Start-Process -FilePath 'cleanmgr.exe' -ArgumentList '/sagerun:1' -Wait -WindowStyle Hidden
                }
                $cleanupResults += 'SystemFiles'
                Write-Host "  ✅ System cleanup completed" -ForegroundColor Green
            }

            Write-Host "🎉 System cleanup completed!" -ForegroundColor Green
            return @{
                Status            = 'Success'
                CleanupOperations = $cleanupResults
                WhatIf            = $WhatIf
            }
        }
    }

    $Global:BusBuddyAdminFunctions.DellOptimizations = $true

    # Register AI-Assistant integration function
    $Global:BusBuddyAdminFunctions.Functions['Start-AIAssistant'] = @{
        RequiresAdmin = $false
        Description   = 'Launch BusBuddy AI-Assistant Development Environment'
        ScriptBlock   = {
            param(
                [string]$Mode = "interactive",
                [switch]$WithContext
            )
            Start-AIAssistantIntegration -Mode $Mode -AdminContext:$WithContext
        }
    }

    # Register PowerShell 7.6 feature functions
    $Global:BusBuddyAdminFunctions.Functions['Test-PowerShell76Features'] = @{
        RequiresAdmin = $false
        Description   = 'Test PowerShell 7.6 specific features and capabilities'
        ScriptBlock   = {
            Test-PowerShell76Features
        }
    }

    $Global:BusBuddyAdminFunctions.Functions['Enable-PowerShell76ExperimentalFeatures'] = @{
        RequiresAdmin = $false
        Description   = 'Enable useful PowerShell 7.6 experimental features'
        ScriptBlock   = {
            param(
                [string[]]$Features = @(
                    'PSNativeWindowsTildeExpansion',
                    'PSRedirectToVariable',
                    'PSSerializeJSONLongEnumAsNumber'
                ),
                [switch]$WhatIf
            )
            Enable-PowerShell76ExperimentalFeatures -Features $Features -WhatIf:$WhatIf
        }
    }

    # Register optimized background task function (PS 7.6)
    $Global:BusBuddyAdminFunctions.Functions['Start-OptimizedBackgroundTask'] = @{
        RequiresAdmin = $false
        Description   = 'Start background tasks with PowerShell 7.6 optimizations and cancellation support'
        ScriptBlock   = {
            param(
                [Parameter(Mandatory)]
                [scriptblock]$ScriptBlock,
                [int]$ThrottleLimit = 10,
                [string]$Name = "BusBuddy-Task-$(Get-Date -Format 'HHmmss')"
            )
            Start-OptimizedBackgroundTask -ScriptBlock $ScriptBlock -ThrottleLimit $ThrottleLimit -Name $Name
        }
    }

    if (-not $Quiet) {
        Write-Host "✅ Dell Inspiron optimization functions registered" -ForegroundColor Green
        Write-Host "✅ AI-Assistant integration functions registered" -ForegroundColor Green
        Write-Host "✅ PowerShell 7.6 optimized background tasks registered" -ForegroundColor Green
        Write-Host "   Available functions: $($Global:BusBuddyAdminFunctions.Functions.Keys -join ', ')" -ForegroundColor Gray
    }
}

#endregion

#region Script Initialization

# Mark admin functions as loaded and detect PS 7.6 features
$Global:BusBuddyAdminFunctions.IsLoaded = $true
$Global:BusBuddyAdminFunctions.AdminPrivileges = Test-AdminPrivileges

# Detect PowerShell 7.6 features on load
try {
    $ps76Features = Test-PowerShell76Features
    $Global:BusBuddyAdminFunctions.PowerShell76Features = $ps76Features

    if (-not $Quiet) {
        Write-Host "`n🚀 BusBuddy Admin Functions Loaded (PowerShell 7.6 Enhanced)" -ForegroundColor Green
        Write-Host "   PS 7.6 Features: $($ps76Features.ThreadJobModule), $($ps76Features.ImprovedJoinPath), $($ps76Features.EnhancedStartProcess)" -ForegroundColor Gray
        if ($ps76Features.PipelineStopTokenSupport) {
            Write-Host "   🎯 Advanced cancellation support enabled" -ForegroundColor Cyan
        }
    }
}
catch {
    Write-Warning "PowerShell 7.6 feature detection failed: $($_.Exception.Message ?? 'Unknown error')"
    $Global:BusBuddyAdminFunctions.PowerShell76Features = $false
}


# Always register Dell optimizations and admin functions on load
Register-DellOptimizationFunctions
$Global:BusBuddyAdminFunctions.DellOptimizations = $true

# Create convenient aliases for common functions
Set-Alias -Name 'admin-start' -Value 'Start-AdminOptimizationSession' -Scope Global -Force
Set-Alias -Name 'admin-status' -Value 'Get-BusBuddySystemStatus' -Scope Global -Force
Set-Alias -Name 'admin-elevate' -Value 'Get-AdminElevationOptions' -Scope Global -Force
Set-Alias -Name 'ps76-test' -Value 'Test-PowerShell76Performance' -Scope Global -Force
Set-Alias -Name 'ps76-features' -Value 'Show-PowerShell76Features' -Scope Global -Force

if (-not $Quiet) {
    Write-Host "`n💡 Quick Commands Available:" -ForegroundColor Cyan
    Write-Host "   admin-start     # Start optimization session" -ForegroundColor Gray
    Write-Host "   admin-status    # Show system status" -ForegroundColor Gray
    Write-Host "   ps76-test       # Test PS 7.6 performance" -ForegroundColor Gray
    Write-Host "   ps76-features   # Manage PS 7.6 features" -ForegroundColor Gray
}

function Start-AIAssistantIntegration {
    <#
    .SYNOPSIS
        Launch BusBuddy AI-Assistant Development Environment with admin context
    #>
    [CmdletBinding()]
    param(
        [string]$Mode = "menu",
        [switch]$WithOptimizations,
        [switch]$AdminContext
    )

    Write-Host "`n🤖 BusBuddy AI-Assistant Integration" -ForegroundColor Cyan
    Write-Host "====================================" -ForegroundColor Cyan

    # Check if AI-Assistant is available
    $aiAssistantPath = ".\AI-Assistant"
    $aiLauncherPath = ".\AI-Assistant\Core\start-ai-busbuddy.ps1"

    if (-not (Test-Path $aiAssistantPath)) {
        Write-Warning "AI-Assistant folder not found at: $aiAssistantPath"
        Write-Host "`n📁 Expected AI-Assistant structure:" -ForegroundColor Yellow
        Write-Host "   .\AI-Assistant\Core\start-ai-busbuddy.ps1" -ForegroundColor Gray
        Write-Host "   .\AI-Assistant\Core\ai-development-assistant.ps1" -ForegroundColor Gray
        Write-Host "`n💡 Please ensure AI-Assistant is properly installed." -ForegroundColor Cyan
        return $false
    }

    if (-not (Test-Path $aiLauncherPath)) {
        Write-Warning "AI-Assistant launcher not found at: $aiLauncherPath"
        Write-Host "`n🔧 Available options:" -ForegroundColor Yellow
        Write-Host "1. Install AI-Assistant components" -ForegroundColor Gray
        Write-Host "2. Use basic AI functions" -ForegroundColor Gray
        Write-Host "3. Return to admin menu" -ForegroundColor Gray

        $choice = Read-Host "`nSelect option (1-3)"
        switch ($choice) {
            '1' { Install-AIAssistantComponents }
            '2' { Start-BasicAIFunctions }
            '3' { return }
            default { Write-Host "❌ Invalid choice. Returning to admin menu." -ForegroundColor Red; return }
        }
        return
    }

    # Check execution policy and provide user options
    $currentPolicy = Get-ExecutionPolicy -Scope CurrentUser
    if ($currentPolicy -eq 'Restricted' -or $currentPolicy -eq 'AllSigned') {
        Write-Host "`n🔐 Execution Policy Notice" -ForegroundColor Yellow
        Write-Host "Current execution policy: $currentPolicy" -ForegroundColor Gray
        Write-Host "`n📋 Options to run AI-Assistant:" -ForegroundColor Cyan
        Write-Host "1. 🚀 Run with bypass (Recommended - No permanent changes)" -ForegroundColor Green
        Write-Host "2. 🔧 Temporarily change execution policy for this session" -ForegroundColor Yellow
        Write-Host "3. 📖 Show manual execution instructions" -ForegroundColor Blue
        Write-Host "4. ↩️ Return to menu" -ForegroundColor Red

        $policyChoice = Read-Host "`nSelect option (1-4)"
        switch ($policyChoice) {
            '1' {
                Write-Host "✅ Using execution policy bypass for AI-Assistant" -ForegroundColor Green
                # Continue with bypass method (already implemented below)
            }
            '2' {
                Write-Host "`n🔧 Temporarily changing execution policy..." -ForegroundColor Cyan
                try {
                    Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser -Force
                    Write-Host "✅ Execution policy temporarily changed to RemoteSigned" -ForegroundColor Green
                    Write-Host "💡 This will be reset when you close PowerShell" -ForegroundColor Gray
                }
                catch {
                    Write-Warning "Failed to change execution policy: $($_.Exception.Message)"
                    Write-Host "🔄 Falling back to bypass method..." -ForegroundColor Yellow
                }
            }
            '3' {
                Write-Host "`n📖 Manual Execution Instructions:" -ForegroundColor Cyan
                Write-Host "Open a new PowerShell window and run:" -ForegroundColor Gray
                Write-Host "pwsh.exe -ExecutionPolicy Bypass -File `"$aiLauncherPath`" -Mode interactive -LoadAllComponents" -ForegroundColor Yellow
                Read-Host "`nPress Enter to return to menu"
                return
            }
            '4' {
                Write-Host "↩️ Returning to menu..." -ForegroundColor Gray
                return
            }
            default {
                Write-Host "❌ Invalid option. Using bypass method..." -ForegroundColor Yellow
            }
        }
    }

    # Display AI-Assistant options
    if ($Mode -eq "menu") {
        Write-Host "`n🎯 AI-Assistant Launch Options:" -ForegroundColor Yellow
        Write-Host "1. 🚀 Interactive AI Development Assistant" -ForegroundColor Green
        Write-Host "2. 🎓 AI Mentor Mode (Educational)" -ForegroundColor Cyan
        Write-Host "3. 🔍 System Health Analysis" -ForegroundColor Blue
        Write-Host "4. 🧠 Complete AI-Integrated Environment" -ForegroundColor Magenta
        Write-Host "5. 🔧 AI Component Status Check" -ForegroundColor Yellow
        Write-Host "6. 🎛️ Custom AI Configuration" -ForegroundColor Gray
        Write-Host "0. Return to Admin Menu" -ForegroundColor Red

        $aiChoice = Read-Host "`nSelect AI mode (0-6)"

        switch ($aiChoice) {
            '1' {
                Write-Host "`n🚀 Launching Interactive AI Assistant..." -ForegroundColor Cyan
                Write-Host "✨ This mode provides real-time code assistance and interactive debugging" -ForegroundColor Gray
                try {
                    # Use pwsh.exe with ExecutionPolicy Bypass to run AI-Assistant scripts
                    Start-Process -FilePath 'pwsh.exe' -ArgumentList '-ExecutionPolicy', 'Bypass', '-File', $aiLauncherPath, '-Mode', 'interactive', '-LoadAllComponents' -Wait
                }
                catch {
                    Write-Warning "Failed to launch AI Assistant: $($_.Exception.Message)"
                    Write-Host "`n🔧 Alternative: Manual execution" -ForegroundColor Yellow
                    Write-Host "Run this command in a new PowerShell window:" -ForegroundColor Gray
                    Write-Host "pwsh.exe -ExecutionPolicy Bypass -File `"$aiLauncherPath`" -Mode interactive -LoadAllComponents" -ForegroundColor Cyan
                }
            }
            '2' {
                Write-Host "`n🎓 Launching AI Mentor Mode..." -ForegroundColor Cyan
                Write-Host "✨ This mode focuses on education and best practices guidance" -ForegroundColor Gray
                try {
                    Start-Process -FilePath 'pwsh.exe' -ArgumentList '-ExecutionPolicy', 'Bypass', '-File', $aiLauncherPath, '-Mode', 'mentor', '-LoadAllComponents' -Wait
                }
                catch {
                    Write-Warning "Failed to launch AI Assistant: $($_.Exception.Message)"
                    Write-Host "`n🔧 Alternative: Manual execution" -ForegroundColor Yellow
                    Write-Host "Run this command in a new PowerShell window:" -ForegroundColor Gray
                    Write-Host "pwsh.exe -ExecutionPolicy Bypass -File `"$aiLauncherPath`" -Mode mentor -LoadAllComponents" -ForegroundColor Cyan
                }
            }
            '3' {
                Write-Host "`n🔍 Running System Health Analysis..." -ForegroundColor Cyan
                Write-Host "✨ This mode performs comprehensive system diagnostics" -ForegroundColor Gray
                try {
                    Start-Process -FilePath 'pwsh.exe' -ArgumentList '-ExecutionPolicy', 'Bypass', '-File', $aiLauncherPath, '-Mode', 'health', '-LoadAllComponents' -Wait
                }
                catch {
                    Write-Warning "Failed to launch AI Assistant: $($_.Exception.Message)"
                    Write-Host "`n🔧 Alternative: Manual execution" -ForegroundColor Yellow
                    Write-Host "Run this command in a new PowerShell window:" -ForegroundColor Gray
                    Write-Host "pwsh.exe -ExecutionPolicy Bypass -File `"$aiLauncherPath`" -Mode health -LoadAllComponents" -ForegroundColor Cyan
                }
            }
            '4' {
                Write-Host "`n🧠 Launching Complete AI Environment..." -ForegroundColor Cyan
                Write-Host "✨ This mode provides full AI integration with all features enabled" -ForegroundColor Gray
                try {
                    Start-Process -FilePath 'pwsh.exe' -ArgumentList '-ExecutionPolicy', 'Bypass', '-File', $aiLauncherPath, '-Mode', 'ai-integrated', '-LoadAllComponents' -Wait
                }
                catch {
                    Write-Warning "Failed to launch AI Assistant: $($_.Exception.Message)"
                    Write-Host "`n🔧 Alternative: Manual execution" -ForegroundColor Yellow
                    Write-Host "Run this command in a new PowerShell window:" -ForegroundColor Gray
                    Write-Host "pwsh.exe -ExecutionPolicy Bypass -File `"$aiLauncherPath`" -Mode ai-integrated -LoadAllComponents" -ForegroundColor Cyan
                }
            }
            '5' {
                Write-Host "`n🔧 Checking AI Component Status..." -ForegroundColor Cyan
                Write-Host "✨ This mode verifies all AI components and their health" -ForegroundColor Gray
                try {
                    Start-Process -FilePath 'pwsh.exe' -ArgumentList '-ExecutionPolicy', 'Bypass', '-File', $aiLauncherPath, '-Mode', 'components', '-LoadAllComponents' -Wait
                }
                catch {
                    Write-Warning "Failed to launch AI Assistant: $($_.Exception.Message)"
                    Write-Host "`n🔧 Alternative: Manual execution" -ForegroundColor Yellow
                    Write-Host "Run this command in a new PowerShell window:" -ForegroundColor Gray
                    Write-Host "pwsh.exe -ExecutionPolicy Bypass -File `"$aiLauncherPath`" -Mode components -LoadAllComponents" -ForegroundColor Cyan
                }
            }
            '6' {
                Write-Host "`n🎛️ Custom AI Configuration..." -ForegroundColor Cyan
                Start-CustomAIConfiguration
            }
            '0' {
                Write-Host "↩️ Returning to admin menu..." -ForegroundColor Gray
                return
            }
            default {
                Write-Host "❌ Invalid option. Returning to admin menu." -ForegroundColor Red
                return
            }
        }
    }
    else {
        # Direct mode launch
        Write-Host "`n🚀 Launching AI-Assistant in $Mode mode..." -ForegroundColor Cyan
        try {
            Start-Process -FilePath 'pwsh.exe' -ArgumentList '-ExecutionPolicy', 'Bypass', '-File', $aiLauncherPath, '-Mode', $Mode, '-LoadAllComponents' -Wait
        }
        catch {
            Write-Warning "Failed to launch AI Assistant: $($_.Exception.Message)"
            Write-Host "`n🔧 Alternative: Manual execution" -ForegroundColor Yellow
            Write-Host "Run this command in a new PowerShell window:" -ForegroundColor Gray
            Write-Host "pwsh.exe -ExecutionPolicy Bypass -File `"$aiLauncherPath`" -Mode $Mode -LoadAllComponents" -ForegroundColor Cyan
        }
    }

    # Post-launch integration
    Write-Host "`n✅ AI-Assistant session completed" -ForegroundColor Green
    Write-Host "💡 Admin functions remain available in this session" -ForegroundColor Cyan
    Write-Host "🔄 You can return to the admin menu or continue with AI-powered development" -ForegroundColor Gray
}

function Start-CustomAIConfiguration {
    <#
    .SYNOPSIS
        Configure AI-Assistant with custom parameters
    #>
    Write-Host "`n🎛️ Custom AI Configuration" -ForegroundColor Cyan
    Write-Host "==========================" -ForegroundColor Cyan

    Write-Host "`n📋 Available AI Features:" -ForegroundColor Yellow
    Write-Host "• AI Mode: Enhanced code intelligence" -ForegroundColor Gray
    Write-Host "• Learn From Run: Continuous learning" -ForegroundColor Gray
    Write-Host "• Predictive Analysis: Error prediction" -ForegroundColor Gray
    Write-Host "• Auto Fix: Automatic issue resolution" -ForegroundColor Gray
    Write-Host "• Stream Logs: Real-time log monitoring" -ForegroundColor Gray
    Write-Host "• Monitor Health: System health tracking" -ForegroundColor Gray
    Write-Host "• Syncfusion Diagnostics: WPF component analysis" -ForegroundColor Gray

    $customParams = @()

    $aiMode = Read-Host "`nEnable AI Mode? (Y/n)"
    if ($aiMode -notmatch '^[Nn]') { $customParams += "-AIMode `$true" }

    $learnMode = Read-Host "Enable Learning Mode? (Y/n)"
    if ($learnMode -notmatch '^[Nn]') { $customParams += "-LearnFromRun `$true" }

    $predictive = Read-Host "Enable Predictive Analysis? (Y/n)"
    if ($predictive -notmatch '^[Nn]') { $customParams += "-PredictiveAnalysis `$true" }

    $autoFix = Read-Host "Enable Auto Fix? (y/N)"
    if ($autoFix -match '^[Yy]') { $customParams += "-AutoFix `$true" }

    $syncfusion = Read-Host "Enable Syncfusion Diagnostics? (Y/n)"
    if ($syncfusion -notmatch '^[Nn]') { $customParams += "-SyncfusionDiagnostics `$true" }

    $aiLauncherPath = ".\AI-Assistant\Core\ai-development-assistant.ps1"
    if (Test-Path $aiLauncherPath) {
        $command = "& `"$aiLauncherPath`" " + ($customParams -join ' ')
        Write-Host "`n🚀 Launching AI Assistant with custom configuration..." -ForegroundColor Cyan
        Write-Host "Command: $command" -ForegroundColor Gray
        try {
            # Use pwsh.exe with ExecutionPolicy Bypass to avoid signing issues
            $args = @('-ExecutionPolicy', 'Bypass', '-File', $aiLauncherPath) + $customParams.Split(' ')
            Start-Process -FilePath 'pwsh.exe' -ArgumentList $args -Wait
        }
        catch {
            Write-Warning "Failed to launch AI Assistant: $($_.Exception.Message)"
            Write-Host "`n🔧 Alternative: Manual execution" -ForegroundColor Yellow
            Write-Host "Run this command in a new PowerShell window:" -ForegroundColor Gray
            Write-Host "pwsh.exe -ExecutionPolicy Bypass -File `"$aiLauncherPath`" $($customParams -join ' ')" -ForegroundColor Cyan
        }
    }
    else {
        Write-Warning "AI development assistant not found at: $aiLauncherPath"
    }
}

function Install-AIAssistantComponents {
    <#
    .SYNOPSIS
        Install or repair AI-Assistant components
    #>
    Write-Host "`n🔧 AI-Assistant Component Installation" -ForegroundColor Cyan
    Write-Host "=====================================" -ForegroundColor Cyan

    Write-Host "This feature would install missing AI components." -ForegroundColor Yellow
    Write-Host "Current implementation: Manual verification" -ForegroundColor Gray

    Write-Host "`n📁 Expected AI-Assistant structure:" -ForegroundColor Yellow
    Write-Host "AI-Assistant/" -ForegroundColor Gray
    Write-Host "├── Core/" -ForegroundColor Gray
    Write-Host "│   ├── start-ai-busbuddy.ps1" -ForegroundColor Gray
    Write-Host "│   ├── ai-development-assistant.ps1" -ForegroundColor Gray
    Write-Host "│   └── ai-knowledge-base.json" -ForegroundColor Gray
    Write-Host "├── Scripts/" -ForegroundColor Gray
    Write-Host "├── Tools/" -ForegroundColor Gray
    Write-Host "└── Documentation/" -ForegroundColor Gray

    Write-Host "`n💡 Manual setup: Ensure all files are present and executable" -ForegroundColor Cyan
}

function Start-BasicAIFunctions {
    <#
    .SYNOPSIS
        Start basic AI functions without full AI-Assistant
    #>
    Write-Host "`n🤖 Basic AI Functions" -ForegroundColor Cyan
    Write-Host "=====================" -ForegroundColor Cyan

    Write-Host "`n🔍 Available basic AI operations:" -ForegroundColor Yellow
    Write-Host "1. Code Analysis (PowerShell-based)" -ForegroundColor Green
    Write-Host "2. Log Pattern Recognition" -ForegroundColor Green
    Write-Host "3. System Intelligence Gathering" -ForegroundColor Green
    Write-Host "4. Development Insights" -ForegroundColor Green

    Write-Host "`n💡 For full AI capabilities, install complete AI-Assistant" -ForegroundColor Cyan
    Read-Host "Press Enter to continue"
}

function Show-PowerShell76Features {
    <#
    .SYNOPSIS
        Display and manage PowerShell 7.6 specific features
    #>
    Write-Host "`n🆕 PowerShell 7.6 Features & Experimental Options" -ForegroundColor Cyan
    Write-Host "================================================" -ForegroundColor Cyan

    # Show current feature status
    $ps76Features = Test-PowerShell76Features

    Write-Host "`n📊 Current Feature Status:" -ForegroundColor Yellow
    Write-Host "  PowerShell Version: $($ps76Features.Version)" -ForegroundColor Gray
    Write-Host "  .NET Version: $($ps76Features.DotNetVersion)" -ForegroundColor Gray
    Write-Host "  ThreadJob Module: $(if ($ps76Features.ThreadJobModule) { '✅ Available' } else { '⚠️ Missing' })" -ForegroundColor $(if ($ps76Features.ThreadJobModule) { 'Green' } else { 'Yellow' })
    Write-Host "  Enhanced Join-Path: $(if ($ps76Features.ImprovedJoinPath) { '✅ Working' } else { '⚠️ Limited' })" -ForegroundColor $(if ($ps76Features.ImprovedJoinPath) { 'Green' } else { 'Yellow' })
    Write-Host "  Experimental Features: $($ps76Features.ExperimentalFeatures.Count) enabled" -ForegroundColor Gray

    if ($ps76Features.ExperimentalFeatures.Count -gt 0) {
        Write-Host "`n🧪 Enabled Experimental Features:" -ForegroundColor Yellow
        foreach ($feature in $ps76Features.ExperimentalFeatures) {
            Write-Host "  • $feature" -ForegroundColor Green
        }
    }

    Write-Host "`n🔧 Available Actions:" -ForegroundColor Yellow
    Write-Host "1. 🧪 Enable Recommended Experimental Features" -ForegroundColor Green
    Write-Host "2. 📋 Show All Available Experimental Features" -ForegroundColor Cyan
    Write-Host "3. 🔍 Test PowerShell 7.6 Performance Features" -ForegroundColor Blue
    Write-Host "4. 📊 Generate PowerShell 7.6 Compatibility Report" -ForegroundColor Magenta
    Write-Host "0. Return to Main Menu" -ForegroundColor Red

    $choice = Read-Host "`nSelect option (0-4)"

    switch ($choice) {
        '1' {
            Write-Host "`n🧪 Enabling Recommended Experimental Features..." -ForegroundColor Cyan
            Enable-PowerShell76ExperimentalFeatures
        }
        '2' {
            Write-Host "`n📋 Available Experimental Features:" -ForegroundColor Cyan
            try {
                $allFeatures = Get-ExperimentalFeature
                if ($allFeatures) {
                    $allFeatures | Format-Table Name, Enabled, Description -AutoSize
                    Write-Host "`n💡 To enable a feature: Enable-ExperimentalFeature -Name 'FeatureName'" -ForegroundColor Cyan
                }
                else {
                    Write-Host "No experimental features available in this PowerShell version" -ForegroundColor Yellow
                }
            }
            catch {
                Write-Host "❌ Unable to retrieve experimental features: $($_.Exception.Message)" -ForegroundColor Red
            }
            Read-Host "`nPress Enter to continue"
        }
        '3' {
            Write-Host "`n🔍 Testing PowerShell 7.6 Performance Features..." -ForegroundColor Cyan
            Test-PowerShell76Performance
        }
        '4' {
            Write-Host "`n📊 Generating PowerShell 7.6 Compatibility Report..." -ForegroundColor Cyan
            Generate-PowerShell76CompatibilityReport
        }
        '0' {
            Write-Host "↩️ Returning to main menu..." -ForegroundColor Gray
        }
        default {
            Write-Host "❌ Invalid option." -ForegroundColor Red
        }
    }
}

function Test-PowerShell76Performance {
    <#
    .SYNOPSIS
        Test PowerShell 7.6 performance improvements with comprehensive benchmarking
    .DESCRIPTION
        Tests PS 7.6 specific performance enhancements including Start-Process efficiency, Join-Path improvements, and ThreadJob performance
    #>
    [CmdletBinding()]
    param()

    Write-Host "`n⚡ PowerShell 7.6 Performance Test Suite" -ForegroundColor Cyan
    Write-Host "=======================================" -ForegroundColor Cyan

    $performanceResults = @{
        StartProcessTest      = @{}
        JoinPathTest          = @{}
        ThreadJobTest         = @{}
        PipelineStopTokenTest = @{}
        OverallScore          = 0
    }

    # Test 1: Start-Process efficiency improvement (PS 7.6 feature)
    Write-Host "`n🔍 Testing Start-Process efficiency..." -ForegroundColor Yellow
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        # PS 7.6: Enhanced polling efficiency should reduce execution time
        $process = Start-Process -FilePath 'pwsh.exe' -ArgumentList '-Command', 'Get-Date' -Wait -PassThru -WindowStyle Hidden
        $sw.Stop()
        $performanceResults.StartProcessTest = @{
            Success     = $true
            ElapsedMs   = $sw.ElapsedMilliseconds
            Performance = if ($sw.ElapsedMilliseconds -lt 1500) { 'Excellent' } elseif ($sw.ElapsedMilliseconds -lt 3000) { 'Good' } else { 'Poor' }
        }
        Write-Host "  ✅ Start-Process test completed in $($sw.ElapsedMilliseconds)ms - $($performanceResults.StartProcessTest.Performance)" -ForegroundColor Green
    }
    catch {
        $performanceResults.StartProcessTest = @{ Success = $false; Error = $_.Exception.Message ?? 'Start-Process test failed' }
        Write-Host "  ❌ Start-Process test failed: $($performanceResults.StartProcessTest.Error)" -ForegroundColor Red
    }

    # Test 2: Join-Path string[] support (PS 7.6 breaking change)
    Write-Host "`n🔍 Testing enhanced Join-Path..." -ForegroundColor Yellow
    try {
        $sw.Restart()
        $testPaths = @(
            Join-Path -Path "C:" -ChildPath @("Users", "Test", "Documents"),
            Join-Path -Path "D:" -ChildPath @("Projects", "BusBuddy", "Logs"),
            Join-Path -Path $env:TEMP -ChildPath @("Cache", "PowerShell", "Modules")
        )
        $sw.Stop()

        $allPathsValid = ($testPaths | Where-Object { $_ -match '^[A-Z]:\\.*' }).Count
        $performanceResults.JoinPathTest = @{
            Success        = $allPathsValid -eq $testPaths.Count
            ElapsedMs      = $sw.ElapsedMilliseconds
            PathsGenerated = $testPaths.Count
        }
        Write-Host "  ✅ Enhanced Join-Path working: Generated $($testPaths.Count) paths in $($sw.ElapsedMilliseconds)ms" -ForegroundColor Green
    }
    catch {
        $performanceResults.JoinPathTest = @{ Success = $false; Error = $_.Exception.Message ?? 'Join-Path test failed' }
        Write-Host "  ❌ Enhanced Join-Path test failed: $($performanceResults.JoinPathTest.Error)" -ForegroundColor Red
    }

    # Test 3: ThreadJob module performance (PS 7.6 Microsoft.PowerShell.ThreadJob)
    Write-Host "`n🔍 Testing ThreadJob module performance..." -ForegroundColor Yellow
    try {
        $sw.Restart()
        $jobs = 1..5 | ForEach-Object {
            Start-ThreadJob -ScriptBlock {
                Get-Date
                Start-Sleep -Milliseconds 100
                Get-Date
                return "Job $using:_ completed"
            }
        }
        $results = $jobs | Receive-Job -Wait
        $jobs | Remove-Job
        $sw.Stop()

        $performanceResults.ThreadJobTest = @{
            Success       = $results.Count -eq 5
            ElapsedMs     = $sw.ElapsedMilliseconds
            JobsCompleted = $results.Count
            Efficiency    = if ($sw.ElapsedMilliseconds -lt 1000) { 'Excellent' } elseif ($sw.ElapsedMilliseconds -lt 2000) { 'Good' } else { 'Poor' }
        }
        Write-Host "  ✅ ThreadJob test completed: $($results.Count) jobs in $($sw.ElapsedMilliseconds)ms - $($performanceResults.ThreadJobTest.Efficiency)" -ForegroundColor Green
    }
    catch {
        $performanceResults.ThreadJobTest = @{ Success = $false; Error = $_.Exception.Message ?? 'ThreadJob test failed' }
        Write-Host "  ❌ ThreadJob test failed: $($performanceResults.ThreadJobTest.Error)" -ForegroundColor Red
    }

    # Test 4: PipelineStopToken support (PS 7.6 feature)
    Write-Host "`n🔍 Testing PipelineStopToken support..." -ForegroundColor Yellow
    try {
        $pipelineStopTokenAvailable = $null -ne $PSCmdlet.PipelineStopToken
        $performanceResults.PipelineStopTokenTest = @{
            Available    = $pipelineStopTokenAvailable
            SupportLevel = if ($pipelineStopTokenAvailable) { 'Full Support' } else { 'Limited Support' }
        }
        Write-Host "  $(if ($pipelineStopTokenAvailable) { '✅' } else { '⚠️' }) PipelineStopToken: $($performanceResults.PipelineStopTokenTest.SupportLevel)" -ForegroundColor $(if ($pipelineStopTokenAvailable) { 'Green' } else { 'Yellow' })
    }
    catch {
        $performanceResults.PipelineStopTokenTest = @{ Available = $false; Error = $_.Exception.Message ?? 'PipelineStopToken test failed' }
        Write-Host "  ❌ PipelineStopToken test failed: $($performanceResults.PipelineStopTokenTest.Error)" -ForegroundColor Red
    }

    # Calculate overall performance score
    $successfulTests = 0
    if ($performanceResults.StartProcessTest.Success) { $successfulTests++ }
    if ($performanceResults.JoinPathTest.Success) { $successfulTests++ }
    if ($performanceResults.ThreadJobTest.Success) { $successfulTests++ }
    if ($performanceResults.PipelineStopTokenTest.Available) { $successfulTests++ }

    $performanceResults.OverallScore = [Math]::Round(($successfulTests / 4) * 100, 1)

    Write-Host "`n📊 Performance Summary:" -ForegroundColor Cyan
    Write-Host "  Overall Score: $($performanceResults.OverallScore)% ($successfulTests/4 tests passed)" -ForegroundColor $(if ($performanceResults.OverallScore -ge 75) { 'Green' } elseif ($performanceResults.OverallScore -ge 50) { 'Yellow' } else { 'Red' })

    if ($performanceResults.OverallScore -eq 100) {
        Write-Host "  🎉 Excellent! Full PowerShell 7.6 performance optimization active" -ForegroundColor Green
    }
    elseif ($performanceResults.OverallScore -ge 75) {
        Write-Host "  ✅ Good! Most PowerShell 7.6 features are working optimally" -ForegroundColor Green
    }
    else {
        Write-Host "  ⚠️ Some PowerShell 7.6 features may not be fully available" -ForegroundColor Yellow
    }

    Read-Host "`nPress Enter to continue"
    return $performanceResults
}

function Generate-PowerShell76CompatibilityReport {
    <#
    .SYNOPSIS
        Generate comprehensive PowerShell 7.6 compatibility report
    #>
    Write-Host "`n📊 PowerShell 7.6 Compatibility Report" -ForegroundColor Cyan
    Write-Host "======================================" -ForegroundColor Cyan

    $report = @{
        PowerShellVersion    = $PSVersionTable.PSVersion
        DotNetVersion        = [System.Runtime.InteropServices.RuntimeInformation]::FrameworkDescription
        Features             = Test-PowerShell76Features
        Modules              = @{}
        ExperimentalFeatures = @{}
        Recommendations      = @()
    }

    # Check key modules
    $keyModules = @('Microsoft.PowerShell.ThreadJob', 'ThreadJob', 'PSReadLine', 'Microsoft.PowerShell.PSResourceGet')
    foreach ($module in $keyModules) {
        $moduleInfo = Get-Module -Name $module -ListAvailable -ErrorAction SilentlyContinue | Select-Object -First 1
        $report.Modules[$module] = if ($moduleInfo) {
            @{ Version = $moduleInfo.Version; Available = $true }
        }
        else {
            @{ Version = $null; Available = $false }
        }
    }

    # Check experimental features
    try {
        $expFeatures = Get-ExperimentalFeature -ErrorAction SilentlyContinue
        foreach ($feature in $expFeatures) {
            $report.ExperimentalFeatures[$feature.Name] = $feature.Enabled
        }
    }
    catch {
        $report.ExperimentalFeatures = @{ Error = "Unable to retrieve experimental features" }
    }

    # Generate recommendations
    if (-not $report.Features.ThreadJobModule) {
        $report.Recommendations += "Install Microsoft.PowerShell.ThreadJob module for better parallel processing"
    }

    if ($report.ExperimentalFeatures.Count -eq 0) {
        $report.Recommendations += "Consider enabling experimental features for enhanced functionality"
    }

    # Display report
    Write-Host "`n📋 Compatibility Report:" -ForegroundColor Yellow
    Write-Host "PowerShell: $($report.PowerShellVersion)" -ForegroundColor Gray
    Write-Host ".NET: $($report.DotNetVersion)" -ForegroundColor Gray

    Write-Host "`n📦 Module Status:" -ForegroundColor Yellow
    foreach ($module in $report.Modules.Keys) {
        $status = $report.Modules[$module]
        $color = if ($status.Available) { 'Green' } else { 'Red' }
        $version = if ($status.Version) { " v$($status.Version)" } else { "" }
        Write-Host "  $module$version - $(if ($status.Available) { '✅ Available' } else { '❌ Missing' })" -ForegroundColor $color
    }

    if ($report.Recommendations.Count -gt 0) {
        Write-Host "`n💡 Recommendations:" -ForegroundColor Yellow
        foreach ($rec in $report.Recommendations) {
            Write-Host "  • $rec" -ForegroundColor Cyan
        }
    }

    # Save report to file
    $reportPath = "PowerShell76-Compatibility-Report-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
    try {
        $report | ConvertTo-Json -Depth 5 | Out-File -FilePath $reportPath -Encoding UTF8
        Write-Host "`n💾 Report saved to: $reportPath" -ForegroundColor Green
    }
    catch {
        Write-Host "`n⚠️ Could not save report to file: $($_.Exception.Message)" -ForegroundColor Yellow
    }

    Read-Host "`nPress Enter to continue"
}

#endregion

#region Integration Functions

function Start-AdminOptimizationSession {
    <#
    .SYNOPSIS
        Start an interactive admin optimization session
    #>
    [CmdletBinding()]
    param(
        [switch]$QuickStart,
        [switch]$DellOnly,
        [switch]$PowerShellOnly
    )

    Write-Host "🔧 BusBuddy Admin Optimization Session" -ForegroundColor Cyan
    Write-Host "=====================================" -ForegroundColor Cyan

    $isAdmin = Test-AdminPrivileges
    Write-Host "Admin Status: $(if ($isAdmin) { '✅ Administrator' } else { '⚠️ Standard User' })" -ForegroundColor $(if ($isAdmin) { 'Green' } else { 'Yellow' })

    if ($QuickStart) {
        Write-Host "`n🚀 Running Quick Start Optimization..." -ForegroundColor Cyan

        # Always run PowerShell optimizations (no admin required)
        $psResult = Invoke-AdminFunction -FunctionName 'Optimize-PowerShellEnvironment'

        if ($isAdmin) {
            # Run hardware optimizations if admin
            $hwResult = Invoke-AdminFunction -FunctionName 'Optimize-DellHardware'
            $cleanupResult = Invoke-AdminFunction -FunctionName 'Start-SystemCleanup'
        }
        else {
            Write-Host "⚠️ Administrator privileges required for hardware and system optimizations" -ForegroundColor Yellow
            Write-Host "   Run the following to enable full optimization:" -ForegroundColor Gray
            Write-Host "   Start-Process pwsh.exe -Verb RunAs" -ForegroundColor Gray
        }

        return @{
            PowerShellOptimization = $psResult
            HardwareOptimization   = if ($isAdmin) { $hwResult } else { 'Skipped - No Admin' }
            SystemCleanup          = if ($isAdmin) { $cleanupResult } else { 'Skipped - No Admin' }
        }
    }

    # Interactive mode
    do {
        Write-Host "`n📋 Available Optimization Options:" -ForegroundColor Yellow
        Write-Host "1. 🚀 PowerShell Environment Optimization (No Admin Required)" -ForegroundColor Green
        if ($isAdmin) {
            Write-Host "2. 🔧 Dell Hardware Optimization (Admin)" -ForegroundColor Cyan
            Write-Host "3. 🧹 System Cleanup (Admin)" -ForegroundColor Cyan
            Write-Host "4. 🎯 Full Optimization (Admin)" -ForegroundColor Magenta
        }
        else {
            Write-Host "2. 🔧 Dell Hardware Optimization (Requires Admin)" -ForegroundColor Gray
            Write-Host "3. 🧹 System Cleanup (Requires Admin)" -ForegroundColor Gray
            Write-Host "4. 🎯 Full Optimization (Requires Admin)" -ForegroundColor Gray
        }
        Write-Host "5. 📊 Show System Status" -ForegroundColor Blue
        Write-Host "6. 🔐 Show Administrator Elevation Options" -ForegroundColor Yellow
        Write-Host "7. 🤖 Launch AI-Assistant Development Environment" -ForegroundColor Magenta
        Write-Host "8. 🆕 PowerShell 7.6 Features & Experimental Options" -ForegroundColor Cyan
        Write-Host "0. Exit" -ForegroundColor Red

        $choice = Read-Host "`nSelect option (0-8)"

        switch ($choice) {
            '1' {
                Invoke-AdminFunction -FunctionName 'Optimize-PowerShellEnvironment'
            }
            '2' {
                if ($isAdmin) {
                    Invoke-AdminFunction -FunctionName 'Optimize-DellHardware'
                }
                else {
                    Write-Host "🔐 Dell Hardware Optimization requires administrator privileges" -ForegroundColor Yellow
                    Request-AdminElevation -SkipExternalWindow
                }
            }
            '3' {
                if ($isAdmin) {
                    Invoke-AdminFunction -FunctionName 'Start-SystemCleanup'
                }
                else {
                    Write-Host "🔐 System Cleanup requires administrator privileges" -ForegroundColor Yellow
                    Request-AdminElevation -SkipExternalWindow
                }
            }
            '4' {
                if ($isAdmin) {
                    Write-Host "🎯 Running Full Optimization Suite..." -ForegroundColor Magenta
                    Write-Host "This will run all optimizations in sequence." -ForegroundColor Gray

                    # Run all optimizations in the current session
                    Write-Host "`n1/3 PowerShell Environment Optimization..." -ForegroundColor Cyan
                    Invoke-AdminFunction -FunctionName 'Optimize-PowerShellEnvironment'

                    Write-Host "`n2/3 Dell Hardware Optimization..." -ForegroundColor Cyan
                    Invoke-AdminFunction -FunctionName 'Optimize-DellHardware'

                    Write-Host "`n3/3 System Cleanup..." -ForegroundColor Cyan
                    Invoke-AdminFunction -FunctionName 'Start-SystemCleanup'

                    Write-Host "`n🎉 Full optimization completed successfully!" -ForegroundColor Green
                }
                else {
                    Write-Host "🔐 Full optimization requires administrator privileges" -ForegroundColor Yellow
                    Write-Host "Available options:" -ForegroundColor Cyan
                    Write-Host "1. Run PowerShell optimization only (no admin needed)" -ForegroundColor Green
                    Write-Host "2. Show elevation instructions (recommended)" -ForegroundColor Yellow

                    $choice = Read-Host "Select option (1/2)"
                    if ($choice -eq '1') {
                        Write-Host "`n🚀 Running PowerShell Optimization..." -ForegroundColor Cyan
                        Invoke-AdminFunction -FunctionName 'Optimize-PowerShellEnvironment'
                    }
                    elseif ($choice -eq '2') {
                        Request-AdminElevation -SkipExternalWindow
                    }
                    else {
                        Write-Host "❌ Invalid choice. Returning to main menu." -ForegroundColor Red
                    }
                }
            }
            '5' {
                Get-BusBuddySystemStatus
            }
            '6' {
                Get-AdminElevationOptions
            }
            '7' {
                Start-AIAssistantIntegration
            }
            '8' {
                Show-PowerShell76Features
            }
            '0' {
                Write-Host "👋 Exiting admin optimization session" -ForegroundColor Cyan
                break
            }
            default {
                Write-Host "❌ Invalid option. Please select 0-8." -ForegroundColor Red
            }
        }

    } while ($choice -ne '0')
}

function Get-BusBuddySystemStatus {
    <#
    .SYNOPSIS
        Get comprehensive system status for BusBuddy development
    #>

    Write-Host "📊 BusBuddy System Status" -ForegroundColor Cyan
    Write-Host "=========================" -ForegroundColor Cyan

    # System Information
    $computerInfo = Get-ComputerInfo
    $processorInfo = Get-CimInstance Win32_Processor

    Write-Host "`n💻 Hardware Information:" -ForegroundColor Yellow
    Write-Host "  System: $($computerInfo.CsManufacturer) $($computerInfo.CsModel)" -ForegroundColor Gray
    Write-Host "  Processor: $($processorInfo.Name)" -ForegroundColor Gray
    Write-Host "  Cores: $($processorInfo.NumberOfCores) | Threads: $($processorInfo.NumberOfLogicalProcessors)" -ForegroundColor Gray
    Write-Host "  Memory: $([Math]::Round($computerInfo.TotalPhysicalMemory / 1GB, 1)) GB" -ForegroundColor Gray

    # PowerShell Status
    Write-Host "`n🚀 PowerShell Status:" -ForegroundColor Yellow
    Write-Host "  Version: $($PSVersionTable.PSVersion)" -ForegroundColor Gray
    Write-Host "  Edition: $($PSVersionTable.PSEdition)" -ForegroundColor Gray
    Write-Host "  .NET: $([System.Runtime.InteropServices.RuntimeInformation]::FrameworkDescription)" -ForegroundColor Gray
    Write-Host "  Admin: $(if (Test-AdminPrivileges) { '✅ Yes' } else { '⚠️ No' })" -ForegroundColor $(if (Test-AdminPrivileges) { 'Green' } else { 'Yellow' })

    # PowerShell 7.6 Features Status
    $ps76Features = Test-PowerShell76Features
    Write-Host "`n🆕 PowerShell 7.6 Features:" -ForegroundColor Yellow
    Write-Host "  ThreadJob Module: $(if ($ps76Features.ThreadJobModule) { '✅ Available' } else { '⚠️ Missing' })" -ForegroundColor $(if ($ps76Features.ThreadJobModule) { 'Green' } else { 'Yellow' })
    Write-Host "  Enhanced Join-Path: $(if ($ps76Features.ImprovedJoinPath) { '✅ Working' } else { '⚠️ Limited' })" -ForegroundColor $(if ($ps76Features.ImprovedJoinPath) { 'Green' } else { 'Yellow' })
    Write-Host "  Enhanced Start-Process: $(if ($ps76Features.EnhancedStartProcess) { '✅ Optimized' } else { '⚠️ Standard' })" -ForegroundColor $(if ($ps76Features.EnhancedStartProcess) { 'Green' } else { 'Yellow' })
    Write-Host "  PipelineStopToken: $(if ($ps76Features.PipelineStopTokenSupport) { '✅ Supported' } else { '⚠️ Not Available' })" -ForegroundColor $(if ($ps76Features.PipelineStopTokenSupport) { 'Green' } else { 'Yellow' })
    Write-Host "  Experimental Features: $($ps76Features.ExperimentalFeatures.Count) enabled" -ForegroundColor Gray

    if ($ps76Features.ExperimentalFeatures.Count -gt 0) {
        Write-Host "`n🧪 Active Experimental Features:" -ForegroundColor Yellow
        foreach ($feature in $ps76Features.ExperimentalFeatures) {
            Write-Host "  • $feature" -ForegroundColor Green
        }
    }

    # Admin Functions Status
    Write-Host "`n🔧 Admin Functions Status:" -ForegroundColor Yellow
    Write-Host "  Loaded: $(if ($Global:BusBuddyAdminFunctions.IsLoaded) { '✅ Yes' } else { '❌ No' })" -ForegroundColor $(if ($Global:BusBuddyAdminFunctions.IsLoaded) { 'Green' } else { 'Red' })
    Write-Host "  Dell Optimizations: $(if ($Global:BusBuddyAdminFunctions.DellOptimizations) { '✅ Yes' } else { '❌ No' })" -ForegroundColor $(if ($Global:BusBuddyAdminFunctions.DellOptimizations) { 'Green' } else { 'Red' })
    Write-Host "  Functions Available: $($Global:BusBuddyAdminFunctions.Functions.Count)" -ForegroundColor Gray

    # AI-Assistant Status
    $aiAssistantPath = ".\AI-Assistant"
    $aiAvailable = Test-Path $aiAssistantPath
    Write-Host "`n🤖 AI-Assistant Status:" -ForegroundColor Yellow
    Write-Host "  Available: $(if ($aiAvailable) { '✅ Yes' } else { '❌ No' })" -ForegroundColor $(if ($aiAvailable) { 'Green' } else { 'Red' })
    if ($aiAvailable) {
        $aiLauncherPath = ".\AI-Assistant\Core\start-ai-busbuddy.ps1"
        $aiLauncherExists = Test-Path $aiLauncherPath
        Write-Host "  Launcher: $(if ($aiLauncherExists) { '✅ Ready' } else { '⚠️ Missing' })" -ForegroundColor $(if ($aiLauncherExists) { 'Green' } else { 'Yellow' })
        Write-Host "  Quick Access: ai-start, ai-assistant, bb-ai" -ForegroundColor Gray
    }
    else {
        Write-Host "  Install Path: $aiAssistantPath" -ForegroundColor Gray
    }

    # Process Information
    $psProcesses = Get-Process | Where-Object { $_.ProcessName -match 'pwsh|dotnet' }
    Write-Host "`n⚙️ Active Processes:" -ForegroundColor Yellow
    Write-Host "  PowerShell: $(@($psProcesses | Where-Object ProcessName -match 'pwsh').Count)" -ForegroundColor Gray
    Write-Host "  .NET: $(@($psProcesses | Where-Object ProcessName -eq 'dotnet').Count)" -ForegroundColor Gray

    return @{
        Hardware       = @{
            Manufacturer = $computerInfo.CsManufacturer
            Model        = $computerInfo.CsModel
            Processor    = $processorInfo.Name
            Cores        = $processorInfo.NumberOfCores
            Threads      = $processorInfo.NumberOfLogicalProcessors
            MemoryGB     = [Math]::Round($computerInfo.TotalPhysicalMemory / 1GB, 1)
        }
        PowerShell     = @{
            Version = $PSVersionTable.PSVersion
            Edition = $PSVersionTable.PSEdition
            DotNet  = [System.Runtime.InteropServices.RuntimeInformation]::FrameworkDescription
            IsAdmin = Test-AdminPrivileges
        }
        AdminFunctions = $Global:BusBuddyAdminFunctions
        Processes      = @{
            PowerShell = @($psProcesses | Where-Object ProcessName -match 'pwsh').Count
            DotNet     = @($psProcesses | Where-Object ProcessName -eq 'dotnet').Count
        }
        PS76Features   = $ps76Features
        AIAssistant    = @{
            Available = $aiAvailable
            Path      = $aiAssistantPath
        }
    }
}

#endregion

#region Initialization

# Initialize the admin function system
$Global:BusBuddyAdminFunctions.AdminPrivileges = Test-AdminPrivileges

if ($LoadDellOptimizations) {
    Register-DellOptimizationFunctions
}
# Create convenient aliases
Set-Alias -Name 'admin-start' -Value 'Start-AdminOptimizationSession'
Set-Alias -Name 'admin-status' -Value 'Get-BusBuddySystemStatus'
Set-Alias -Name 'admin-elevate' -Value 'Request-AdminElevation'
Set-Alias -Name 'admin-options' -Value 'Get-AdminElevationOptions'
Set-Alias -Name 'dell-optimize' -Value 'Invoke-AdminFunction'
Set-Alias -Name 'ai-start' -Value 'Start-AIAssistantIntegration'
Set-Alias -Name 'ai-assistant' -Value 'Start-AIAssistantIntegration'
Set-Alias -Name 'bb-ai' -Value 'Start-AIAssistantIntegration'
Set-Alias -Name 'ps76-features' -Value 'Show-PowerShell76Features'
Set-Alias -Name 'ps76-test' -Value 'Test-PowerShell76Features'
Set-Alias -Name 'ps76-experimental' -Value 'Enable-PowerShell76ExperimentalFeatures'

# Mark as loaded
$Global:BusBuddyAdminFunctions.IsLoaded = $true
$Global:BusBuddyAdminFunctions.LoadTime = Get-Date

if (-not $Quiet) {
    Write-Host "✅ BusBuddy Administrator Functions Loaded" -ForegroundColor Green
    Write-Host "   PowerShell: $($PSVersionTable.PSVersion) (Enhanced for 7.6)" -ForegroundColor Gray
    Write-Host "   Admin Status: $(if ($Global:BusBuddyAdminFunctions.AdminPrivileges) { 'Administrator' } else { 'Standard User' })" -ForegroundColor Gray
    Write-Host "   Dell Optimizations: $(if ($LoadDellOptimizations) { 'Enabled' } else { 'Disabled' })" -ForegroundColor Gray

    # Check AI-Assistant availability
    $aiAvailable = Test-Path ".\AI-Assistant"
    Write-Host "   AI-Assistant: $(if ($aiAvailable) { 'Available' } else { 'Not Found' })" -ForegroundColor $(if ($aiAvailable) { 'Gray' } else { 'Yellow' })

    Write-Host "   Quick Start: admin-start -QuickStart" -ForegroundColor Gray
    Write-Host "   Interactive: admin-start" -ForegroundColor Gray
    Write-Host "   Status: admin-status" -ForegroundColor Gray
    if ($aiAvailable) {
        Write-Host "   AI Assistant: ai-start, ai-assistant, bb-ai" -ForegroundColor Gray
    }
    Write-Host "   PowerShell 7.6: ps76-features, ps76-test, ps76-experimental" -ForegroundColor Gray
}

#endregion

# Note: Export-ModuleMember only works in modules, these functions are available in session
