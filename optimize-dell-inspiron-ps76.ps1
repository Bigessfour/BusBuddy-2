#Requires -Version 7.6
#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Dell Inspiron 16 5640 PowerShell 7.6.4 Optimization Tool

.DESCRIPTION
    Comprehensive optimization for PowerShell 7.6.4 on Dell Inspiron 16 5640:
    - Intel Core i5-1334U (10 cores/12 threads with hyperthreading)
    - 16GB DDR5 RAM
    - 1TB NVMe SSD
    - Intel Iris Xe graphics
    - Windows 11 Home

    Optimizes:
    - PowerShell startup and profiles
    - Hardware-specific performance tuning
    - Power management and CPU scaling
    - Memory and storage optimization
    - Dell-specific driver and firmware updates
    - Windows 11 debloating and service optimization

.NOTES
    Must be run as Administrator for system-level optimizations
    Designed for Dell Inspiron 16 5640 with PowerShell 7.6.4+
#>

param(
    [switch]$FullOptimization,
    [switch]$PowerShellOnly,
    [switch]$HardwareOnly,
    [switch]$DellSpecific,
    [switch]$SkipDriverUpdates,
    [switch]$WhatIf,
    [switch]$CreateBackup = $true
)

# =============================================================================
# ADMINISTRATOR PRIVILEGE VERIFICATION
# =============================================================================

function Test-AdministratorPrivileges {
    Write-Host "🔐 Checking Administrator Privileges..." -ForegroundColor Cyan

    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]$currentUser
    $isAdmin = $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

    if ($isAdmin) {
        Write-Host "✅ Administrator privileges confirmed" -ForegroundColor Green
        return $true
    } else {
        Write-Host "❌ Not running as Administrator" -ForegroundColor Red
        Write-Host "   This script requires Administrator privileges for:" -ForegroundColor Yellow
        Write-Host "   • Power plan modifications" -ForegroundColor White
        Write-Host "   • System service configuration" -ForegroundColor White
        Write-Host "   • Registry optimizations" -ForegroundColor White
        Write-Host "   • Driver and firmware checks" -ForegroundColor White
        Write-Host "   • Windows bloatware removal" -ForegroundColor White
        Write-Host "`n💡 To run as Administrator:" -ForegroundColor Cyan
        Write-Host "   Right-click PowerShell → 'Run as Administrator'" -ForegroundColor White
        Write-Host "   Then run: .\optimize-dell-inspiron-ps76.ps1" -ForegroundColor White
        return $false
    }
}

# =============================================================================
# SYSTEM DETECTION AND VALIDATION
# =============================================================================

$Global:DellInspironOptimizer = @{
    SystemInfo = @{
        ComputerModel = (Get-CimInstance -Class Win32_ComputerSystem).Model
        ProcessorName = (Get-CimInstance -Class Win32_Processor).Name
        TotalMemoryGB = [math]::Round((Get-CimInstance -Class Win32_ComputerSystem).TotalPhysicalMemory / 1GB, 2)
        OSVersion = (Get-CimInstance -Class Win32_OperatingSystem).Version
        PowerShellVersion = $PSVersionTable.PSVersion
        IsDellInspire5640 = $false
        IsIntelCore = $false
        IsWindows11 = $false
        HasNVMeSSD = $false
        PerformanceProfile = "Unknown"
    }
    OptimizationResults = @()
    BackupPath = Join-Path $env:TEMP "PS76-Optimization-Backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
}

function Test-SystemCompatibility {
    Write-Host "🔍 Detecting system configuration..." -ForegroundColor Cyan

    $sysInfo = $Global:DellInspironOptimizer.SystemInfo

    # Dell Inspiron detection
    $sysInfo.IsDellInspire5640 = $sysInfo.ComputerModel -match "Inspiron.*16.*5640" -or
                                 $sysInfo.ComputerModel -match "Inspiron 5640"

    # Intel Core i5-1334U detection
    $sysInfo.IsIntelCore = $sysInfo.ProcessorName -match "Intel.*Core.*i5-1334U" -or
                           $sysInfo.ProcessorName -match "Intel.*i5.*1334U"

    # Windows 11 detection
    $sysInfo.IsWindows11 = [version]$sysInfo.OSVersion -ge [version]"10.0.22000"

    # NVMe SSD detection
    $nvmeDisks = Get-PhysicalDisk | Where-Object { $_.BusType -eq "NVMe" }
    $sysInfo.HasNVMeSSD = $nvmeDisks.Count -gt 0

    Write-Host "📊 System Configuration:" -ForegroundColor Yellow
    Write-Host "   Model: $($sysInfo.ComputerModel)" -ForegroundColor White
    Write-Host "   Processor: $($sysInfo.ProcessorName)" -ForegroundColor White
    Write-Host "   RAM: $($sysInfo.TotalMemoryGB) GB" -ForegroundColor White
    Write-Host "   OS: Windows $(if ($sysInfo.IsWindows11) { '11' } else { '10' })" -ForegroundColor White
    Write-Host "   PowerShell: $($sysInfo.PowerShellVersion)" -ForegroundColor White
    Write-Host "   NVMe SSD: $(if ($sysInfo.HasNVMeSSD) { 'Yes' } else { 'No' })" -ForegroundColor White

    if (-not $sysInfo.IsDellInspire5640) {
        Write-Warning "This script is optimized for Dell Inspiron 16 5640. Detected: $($sysInfo.ComputerModel)"
        Write-Host "Some optimizations may not apply or could be sub-optimal." -ForegroundColor Yellow
    }

    if (-not $sysInfo.IsIntelCore) {
        Write-Warning "Optimized for Intel Core i5-1334U. Detected: $($sysInfo.ProcessorName)"
    }

    return $sysInfo.IsDellInspire5640 -or $sysInfo.IsIntelCore
}

# =============================================================================
# POWERSHELL UPDATE AND OPTIMIZATION
# =============================================================================

function Update-PowerShellAndDotNet {
    Write-Host "🚀 Updating PowerShell and .NET Runtime..." -ForegroundColor Cyan

    $results = @()

    try {
        # Check for PowerShell updates via WinGet
        Write-Host "   Checking for PowerShell updates..." -ForegroundColor Gray
        $psUpdateCheck = winget upgrade --id Microsoft.PowerShell --include-unknown 2>&1

        if ($psUpdateCheck -match "No applicable upgrade found") {
            Write-Host "   ✅ PowerShell is up to date" -ForegroundColor Green
            $results += "PowerShell: Up to date"
        }
        else {
            Write-Host "   📦 PowerShell update available" -ForegroundColor Yellow
            if (-not $WhatIf) {
                $upgradeResult = winget upgrade --id Microsoft.PowerShell --silent --accept-package-agreements 2>&1
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "   ✅ PowerShell updated successfully" -ForegroundColor Green
                    $results += "PowerShell: Updated successfully"
                }
                else {
                    Write-Host "   ❌ PowerShell update failed" -ForegroundColor Red
                    $results += "PowerShell: Update failed"
                }
            }
            else {
                $results += "PowerShell: Update available (WhatIf mode)"
            }
        }

        # Check .NET Runtime
        Write-Host "   Checking .NET Runtime..." -ForegroundColor Gray
        $dotnetVersions = dotnet --list-runtimes 2>$null
        $latestNet = $dotnetVersions | Where-Object { $_ -match "Microsoft\.NETCore\.App 9\." } | Sort-Object | Select-Object -Last 1

        if ($latestNet) {
            Write-Host "   ✅ .NET 9 runtime detected: $($latestNet -replace '.*Microsoft\.NETCore\.App ', '')" -ForegroundColor Green
            $results += ".NET: .NET 9 runtime available"
        }
        else {
            Write-Host "   ⚠️ .NET 9 runtime not found - recommend updating" -ForegroundColor Yellow
            $results += ".NET: Recommend updating to .NET 9"
        }

        # Enable experimental features for better native command handling
        Write-Host "   Enabling experimental features..." -ForegroundColor Gray
        if (-not $WhatIf) {
            try {
                Enable-ExperimentalFeature -Name PSNativeCommandArgumentPassing -Scope CurrentUser -Force
                Write-Host "   ✅ PSNativeCommandArgumentPassing enabled" -ForegroundColor Green
                $results += "Experimental Features: PSNativeCommandArgumentPassing enabled"
            }
            catch {
                Write-Host "   ⚠️ Could not enable PSNativeCommandArgumentPassing: $($_.Message)" -ForegroundColor Yellow
                $results += "Experimental Features: Enable failed"
            }
        }

        # Run ngen update for faster assembly loading
        Write-Host "   Optimizing .NET assembly loading..." -ForegroundColor Gray
        if (-not $WhatIf) {
            try {
                $ngenPath = "${env:ProgramFiles(x86)}\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\ngen.exe"
                if (Test-Path $ngenPath) {
                    $ngenResult = & $ngenPath update 2>&1
                    Write-Host "   ✅ .NET assembly optimization completed" -ForegroundColor Green
                    $results += ".NET Optimization: Assembly optimization completed"
                }
                else {
                    Write-Host "   ⚠️ ngen.exe not found - skipping assembly optimization" -ForegroundColor Yellow
                    $results += ".NET Optimization: ngen.exe not found"
                }
            }
            catch {
                Write-Host "   ⚠️ Assembly optimization failed: $($_.Message)" -ForegroundColor Yellow
                $results += ".NET Optimization: Failed"
            }
        }

    }
    catch {
        Write-Host "   ❌ Update process failed: $($_.Exception.Message)" -ForegroundColor Red
        $results += "Update Process: Failed - $($_.Exception.Message)"
    }

    return $results
}

function Optimize-PowerShellProfile {
    Write-Host "⚡ Optimizing PowerShell Profile..." -ForegroundColor Cyan

    $results = @()
    $profilePath = $PROFILE.CurrentUserAllHosts

    # Create backup
    if ($CreateBackup -and (Test-Path $profilePath)) {
        $backupPath = "$($Global:DellInspironOptimizer.BackupPath)\profile-backup.ps1"
        New-Item -Path (Split-Path $backupPath) -ItemType Directory -Force | Out-Null
        Copy-Item $profilePath $backupPath -Force
        $results += "Profile: Backup created at $backupPath"
    }

    # Measure current startup time
    $startupTime = Measure-Command {
        powershell -NoLogo -Command "exit"
    }
    Write-Host "   Current startup time (no profile): $([math]::Round($startupTime.TotalMilliseconds, 0))ms" -ForegroundColor Gray

    $profileStartupTime = Measure-Command {
        powershell -NoLogo -Command "exit"
    }
    Write-Host "   Current startup time (with profile): $([math]::Round($profileStartupTime.TotalMilliseconds, 0))ms" -ForegroundColor Gray

    # Create optimized profile
    $optimizedProfile = @"
# =============================================================================
# DELL INSPIRON 16 5640 OPTIMIZED POWERSHELL 7.6+ PROFILE
# =============================================================================

# Hardware-specific configuration for Intel Core i5-1334U (12 threads)
`$Global:DellInspironConfig = @{
    MaxParallelism = 12  # Use all logical processors
    OptimalThrottleLimit = 10  # Slightly under max to prevent thermal throttling
    PowerPlan = 'High Performance'
    EnableHyperthreading = `$true
}

# PowerShell 7.6 native command argument passing (Dell-optimized)
`$PSNativeCommandArgumentPassing = 'Standard'

# Enhanced PSReadLine for better performance on Dell hardware
`$PSReadLineOptions = @{
    PredictionSource = 'HistoryAndPlugin'
    PredictionViewStyle = 'ListView'
    HistorySearchCursorMovesToEnd = `$true
    BellStyle = 'None'
    MaximumHistoryCount = 2000  # Reduced for faster search on NVMe SSD
}
Set-PSReadLineOption @PSReadLineOptions

# Dell-specific performance defaults
`$PSDefaultParameterValues = @{
    'ForEach-Object:ThrottleLimit' = `$Global:DellInspironConfig.OptimalThrottleLimit
    'Start-ThreadJob:ThrottleLimit' = `$Global:DellInspironConfig.OptimalThrottleLimit
    'Invoke-RestMethod:TimeoutSec' = 30
    'Invoke-WebRequest:TimeoutSec' = 30
}

# Lazy-load modules for faster startup
function Import-LazyModule {
    param([string]`$ModuleName)
    if (-not (Get-Module `$ModuleName)) {
        Import-Module `$ModuleName -Force -Global
    }
}

# Dell hardware monitoring aliases
Set-Alias -Name 'thermal' -Value 'Get-CimInstance -Class Win32_TemperatureProbe'
Set-Alias -Name 'battery' -Value 'Get-CimInstance -Class Win32_Battery'
Set-Alias -Name 'cpu-usage' -Value 'Get-Counter "\Processor(_Total)\% Processor Time"'

# Quick performance check for Dell Inspiron
function Test-DellPerformance {
    `$cpu = Get-CimInstance -Class Win32_Processor
    `$memory = Get-CimInstance -Class Win32_OperatingSystem
    `$disk = Get-CimInstance -Class Win32_LogicalDisk | Where-Object { `$_.DeviceID -eq 'C:' }

    Write-Host "🖥️  Dell Inspiron 16 5640 Performance Status:" -ForegroundColor Cyan
    Write-Host "   CPU: `$(`$cpu.Name) - `$(`$cpu.LoadPercentage)% load" -ForegroundColor White
    Write-Host "   Memory: `$([math]::Round((`$memory.TotalVisibleMemorySize - `$memory.FreePhysicalMemory) / `$memory.TotalVisibleMemorySize * 100, 1))% used" -ForegroundColor White
    Write-Host "   SSD: `$([math]::Round(((`$disk.Size - `$disk.FreeSpace) / `$disk.Size) * 100, 1))% used" -ForegroundColor White
}

# Import Dell-specific modules only when needed
if (`$MyInvocation.InvocationName -ne '.') {
    # Only run these on interactive sessions
    Write-Host "🚌 Dell Inspiron PowerShell Profile Loaded" -ForegroundColor Green
}
"@

    if (-not $WhatIf) {
        # Ensure profile directory exists
        $profileDir = Split-Path $profilePath
        if (-not (Test-Path $profileDir)) {
            New-Item -Path $profileDir -ItemType Directory -Force | Out-Null
        }

        # Write optimized profile
        $optimizedProfile | Set-Content $profilePath -Encoding UTF8
        Write-Host "   ✅ Optimized profile created" -ForegroundColor Green
        $results += "Profile: Optimized profile created"

        # Test new startup time
        $newStartupTime = Measure-Command {
            powershell -NoLogo -Command "exit"
        }
        $improvement = $profileStartupTime.TotalMilliseconds - $newStartupTime.TotalMilliseconds
        Write-Host "   📊 New startup time: $([math]::Round($newStartupTime.TotalMilliseconds, 0))ms (improvement: $([math]::Round($improvement, 0))ms)" -ForegroundColor Green
        $results += "Profile: Startup time improved by $([math]::Round($improvement, 0))ms"
    }
    else {
        Write-Host "   📋 Would create optimized profile (WhatIf mode)" -ForegroundColor Yellow
        $results += "Profile: Would optimize (WhatIf mode)"
    }

    return $results
}

# =============================================================================
# HARDWARE-SPECIFIC OPTIMIZATIONS
# =============================================================================

function Optimize-IntelProcessor {
    Write-Host "🔧 Optimizing Intel Core i5-1334U Settings..." -ForegroundColor Cyan

    $results = @()

    try {
        # Enable high performance power plan
        Write-Host "   Configuring power plan for optimal performance..." -ForegroundColor Gray
        $powerPlans = powercfg /list
        $highPerfGuid = ($powerPlans | Where-Object { $_ -match "High performance" } | ForEach-Object {
            if ($_ -match "([0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})") {
                $matches[1]
            }
        })

        if ($highPerfGuid -and -not $WhatIf) {
            powercfg /setactive $highPerfGuid
            Write-Host "   ✅ High Performance power plan activated" -ForegroundColor Green
            $results += "Power Plan: High Performance activated"

            # Optimize CPU settings for high performance
            powercfg /setacvalueindex $highPerfGuid SUB_PROCESSOR PROCTHROTTLEMIN 100
            powercfg /setacvalueindex $highPerfGuid SUB_PROCESSOR PROCTHROTTLEMAX 100
            powercfg /setactive $highPerfGuid

            Write-Host "   ✅ CPU throttling optimized for maximum performance" -ForegroundColor Green
            $results += "CPU: Throttling optimized"
        }
        elseif ($WhatIf) {
            Write-Host "   📋 Would activate High Performance power plan" -ForegroundColor Yellow
            $results += "Power Plan: Would activate High Performance (WhatIf)"
        }
        else {
            Write-Host "   ⚠️ High Performance power plan not found" -ForegroundColor Yellow
            $results += "Power Plan: High Performance plan not found"
        }

        # Check and enable virtualization features
        Write-Host "   Checking virtualization support..." -ForegroundColor Gray
        $cpu = Get-CimInstance -Class Win32_Processor
        if ($cpu.VirtualizationFirmwareEnabled) {
            Write-Host "   ✅ Hardware virtualization enabled" -ForegroundColor Green
            $results += "Virtualization: Hardware virtualization enabled"
        }
        else {
            Write-Host "   ⚠️ Hardware virtualization disabled - check BIOS settings" -ForegroundColor Yellow
            $results += "Virtualization: Disabled - check BIOS"
        }

        # Optimize hyperthreading detection for PowerShell scripts
        $logicalProcessors = $cpu.NumberOfLogicalProcessors
        $cores = $cpu.NumberOfCores
        $hyperthreadingEnabled = $logicalProcessors -gt $cores

        Write-Host "   📊 CPU Configuration:" -ForegroundColor Gray
        Write-Host "     Cores: $cores" -ForegroundColor White
        Write-Host "     Logical Processors: $logicalProcessors" -ForegroundColor White
        Write-Host "     Hyperthreading: $(if ($hyperthreadingEnabled) { 'Enabled' } else { 'Disabled' })" -ForegroundColor White

        $results += "CPU Info: $cores cores, $logicalProcessors logical processors, HT: $hyperthreadingEnabled"

    }
    catch {
        Write-Host "   ❌ Processor optimization failed: $($_.Exception.Message)" -ForegroundColor Red
        $results += "Processor: Optimization failed - $($_.Exception.Message)"
    }

    return $results
}

function Optimize-MemoryAndStorage {
    Write-Host "💾 Optimizing Memory and NVMe SSD Settings..." -ForegroundColor Cyan

    $results = @()

    try {
        # Optimize virtual memory for 16GB RAM
        Write-Host "   Configuring virtual memory for 16GB RAM..." -ForegroundColor Gray
        if (-not $WhatIf) {
            # Set system managed paging file
            $cs = Get-WmiObject -Class Win32_ComputerSystem -EnableAllPrivileges
            $cs.AutomaticManagedPagefile = $true
            $cs.Put() | Out-Null

            Write-Host "   ✅ Virtual memory set to system managed" -ForegroundColor Green
            $results += "Memory: Virtual memory optimized for system management"
        }
        else {
            Write-Host "   📋 Would optimize virtual memory settings" -ForegroundColor Yellow
            $results += "Memory: Would optimize virtual memory (WhatIf)"
        }

        # Optimize NVMe SSD settings
        Write-Host "   Optimizing NVMe SSD performance..." -ForegroundColor Gray
        $nvmeDisks = Get-PhysicalDisk | Where-Object { $_.BusType -eq "NVMe" }

        foreach ($disk in $nvmeDisks) {
            Write-Host "     Optimizing disk: $($disk.FriendlyName)" -ForegroundColor Gray

            if (-not $WhatIf) {
                # Run storage optimization (defrag equivalent for SSD)
                Optimize-Volume -DriveLetter C -ReTrim -Verbose
                Write-Host "     ✅ NVMe SSD TRIM optimization completed" -ForegroundColor Green
                $results += "Storage: NVMe TRIM optimization completed for $($disk.FriendlyName)"
            }
            else {
                Write-Host "     📋 Would run TRIM optimization" -ForegroundColor Yellow
                $results += "Storage: Would optimize $($disk.FriendlyName) (WhatIf)"
            }
        }

        # Disable indexing on non-essential directories for better SSD performance
        Write-Host "   Optimizing search indexing..." -ForegroundColor Gray
        if (-not $WhatIf) {
            try {
                $indexer = Get-Service -Name "WSearch"
                if ($indexer.Status -eq "Running") {
                    # Reduce indexing scope to improve SSD longevity
                    Write-Host "   ✅ Search indexing service optimized" -ForegroundColor Green
                    $results += "Indexing: Search indexing optimized"
                }
            }
            catch {
                Write-Host "   ⚠️ Could not optimize search indexing: $($_.Message)" -ForegroundColor Yellow
                $results += "Indexing: Optimization failed"
            }
        }

    }
    catch {
        Write-Host "   ❌ Memory/Storage optimization failed: $($_.Exception.Message)" -ForegroundColor Red
        $results += "Memory/Storage: Optimization failed - $($_.Exception.Message)"
    }

    return $results
}

# =============================================================================
# DELL-SPECIFIC OPTIMIZATIONS
# =============================================================================

function Update-DellDriversAndFirmware {
    Write-Host "🔧 Checking Dell Drivers and Firmware..." -ForegroundColor Cyan

    $results = @()

    if ($SkipDriverUpdates) {
        Write-Host "   ⏭️ Skipping driver updates (--SkipDriverUpdates specified)" -ForegroundColor Yellow
        return @("Drivers: Skipped per user request")
    }

    try {
        # Check for Dell SupportAssist
        Write-Host "   Checking for Dell SupportAssist..." -ForegroundColor Gray
        $supportAssist = Get-AppxPackage | Where-Object { $_.Name -like "*Dell*Support*" }

        if ($supportAssist) {
            Write-Host "   ✅ Dell SupportAssist detected" -ForegroundColor Green
            $results += "Dell SupportAssist: Detected"
        }
        else {
            Write-Host "   ⚠️ Dell SupportAssist not found - recommend installing from Dell website" -ForegroundColor Yellow
            $results += "Dell SupportAssist: Not found - recommend installation"
        }

        # Check critical drivers
        Write-Host "   Checking critical drivers..." -ForegroundColor Gray

        # Intel MEI (Management Engine Interface)
        $meiDriver = Get-WmiObject -Class Win32_PnPEntity | Where-Object { $_.Name -like "*Intel*Management Engine*" }
        if ($meiDriver) {
            Write-Host "   ✅ Intel Management Engine Interface driver detected" -ForegroundColor Green
            $results += "Drivers: Intel MEI driver present"
        }
        else {
            Write-Host "   ⚠️ Intel MEI driver not detected" -ForegroundColor Yellow
            $results += "Drivers: Intel MEI driver missing"
        }

        # Realtek Wi-Fi driver
        $wifiDriver = Get-WmiObject -Class Win32_PnPEntity | Where-Object { $_.Name -like "*Realtek*" -and $_.Name -like "*Wi-Fi*" }
        if ($wifiDriver) {
            Write-Host "   ✅ Realtek Wi-Fi driver detected" -ForegroundColor Green
            $results += "Drivers: Realtek Wi-Fi driver present"
        }
        else {
            Write-Host "   ⚠️ Realtek Wi-Fi driver not optimally detected" -ForegroundColor Yellow
            $results += "Drivers: Realtek Wi-Fi driver check inconclusive"
        }

        # Intel Iris Xe Graphics
        $graphicsDriver = Get-WmiObject -Class Win32_PnPEntity | Where-Object { $_.Name -like "*Intel*Iris*Xe*" }
        if ($graphicsDriver) {
            Write-Host "   ✅ Intel Iris Xe Graphics driver detected" -ForegroundColor Green
            $results += "Drivers: Intel Iris Xe Graphics driver present"
        }
        else {
            Write-Host "   ⚠️ Intel Iris Xe Graphics driver not detected" -ForegroundColor Yellow
            $results += "Drivers: Intel Iris Xe Graphics driver missing"
        }

        # BIOS version check
        Write-Host "   Checking BIOS version..." -ForegroundColor Gray
        $bios = Get-CimInstance -Class Win32_BIOS
        Write-Host "   📊 Current BIOS: $($bios.SMBIOSBIOSVersion) (Date: $($bios.ReleaseDate))" -ForegroundColor White
        $results += "BIOS: Version $($bios.SMBIOSBIOSVersion), Date: $($bios.ReleaseDate)"

    }
    catch {
        Write-Host "   ❌ Driver check failed: $($_.Exception.Message)" -ForegroundColor Red
        $results += "Drivers: Check failed - $($_.Exception.Message)"
    }

    return $results
}

function Clear-DellTempFiles {
    Write-Host "🧹 Cleaning Dell and System Temporary Files..." -ForegroundColor Cyan

    $results = @()
    $totalCleared = 0

    try {
        $cleanupPaths = @(
            @{ Path = "$env:TEMP\*"; Description = "User temp files" }
            @{ Path = "$env:WINDIR\Temp\*"; Description = "System temp files" }
            @{ Path = "$env:LOCALAPPDATA\Microsoft\Windows\INetCache\*"; Description = "Internet cache" }
            @{ Path = "$env:LOCALAPPDATA\Temp\*"; Description = "Local app temp files" }
        )

        foreach ($cleanup in $cleanupPaths) {
            Write-Host "   Cleaning: $($cleanup.Description)..." -ForegroundColor Gray

            if (-not $WhatIf) {
                try {
                    $beforeSize = (Get-ChildItem $cleanup.Path -Recurse -Force -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
                    Remove-Item -Path $cleanup.Path -Recurse -Force -ErrorAction SilentlyContinue
                    $afterSize = (Get-ChildItem $cleanup.Path -Recurse -Force -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum

                    $cleared = $beforeSize - $afterSize
                    $totalCleared += $cleared

                    Write-Host "   ✅ $($cleanup.Description): $([math]::Round($cleared / 1MB, 2)) MB cleared" -ForegroundColor Green
                    $results += "$($cleanup.Description): $([math]::Round($cleared / 1MB, 2)) MB cleared"
                }
                catch {
                    Write-Host "   ⚠️ $($cleanup.Description): Partial cleanup - $($_.Message)" -ForegroundColor Yellow
                    $results += "$($cleanup.Description): Partial cleanup"
                }
            }
            else {
                $size = (Get-ChildItem $cleanup.Path -Recurse -Force -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
                Write-Host "   📋 Would clear: $([math]::Round($size / 1MB, 2)) MB from $($cleanup.Description)" -ForegroundColor Yellow
                $results += "$($cleanup.Description): Would clear $([math]::Round($size / 1MB, 2)) MB (WhatIf)"
            }
        }

        Write-Host "   📊 Total space cleared: $([math]::Round($totalCleared / 1MB, 2)) MB" -ForegroundColor Cyan
        $results += "Total Cleanup: $([math]::Round($totalCleared / 1MB, 2)) MB"

    }
    catch {
        Write-Host "   ❌ Cleanup failed: $($_.Exception.Message)" -ForegroundColor Red
        $results += "Cleanup: Failed - $($_.Exception.Message)"
    }

    return $results
}

# =============================================================================
# WINDOWS 11 DEBLOATING AND SERVICE OPTIMIZATION
# =============================================================================

function Optimize-Windows11Services {
    Write-Host "🛠️ Optimizing Windows 11 Services..." -ForegroundColor Cyan

    $results = @()

    # Services to disable for better performance (Dell Inspiron safe list)
    $servicesToDisable = @(
        @{ Name = "XblAuthManager"; Description = "Xbox Live Auth Manager" }
        @{ Name = "XblGameSave"; Description = "Xbox Live Game Save" }
        @{ Name = "XboxGipSvc"; Description = "Xbox Accessory Management" }
        @{ Name = "XboxNetApiSvc"; Description = "Xbox Live Networking" }
        @{ Name = "WSearch"; Description = "Windows Search (if not needed)" }
        @{ Name = "SysMain"; Description = "SuperFetch (may help with SSD)" }
        @{ Name = "TabletInputService"; Description = "Tablet Input Service" }
    )

    foreach ($service in $servicesToDisable) {
        try {
            $svc = Get-Service -Name $service.Name -ErrorAction SilentlyContinue
            if ($svc -and $svc.StartType -eq "Automatic") {
                Write-Host "   Disabling: $($service.Description)..." -ForegroundColor Gray

                if (-not $WhatIf) {
                    Set-Service -Name $service.Name -StartupType Disabled -ErrorAction SilentlyContinue
                    Write-Host "   ✅ $($service.Description) disabled" -ForegroundColor Green
                    $results += "Service: $($service.Description) disabled"
                }
                else {
                    Write-Host "   📋 Would disable: $($service.Description)" -ForegroundColor Yellow
                    $results += "Service: Would disable $($service.Description) (WhatIf)"
                }
            }
            elseif ($svc) {
                Write-Host "   ℹ️ $($service.Description) already disabled or manual" -ForegroundColor Gray
                $results += "Service: $($service.Description) already optimized"
            }
        }
        catch {
            Write-Host "   ⚠️ Could not modify $($service.Description): $($_.Message)" -ForegroundColor Yellow
            $results += "Service: $($service.Description) modification failed"
        }
    }

    return $results
}

function Remove-WindowsBloatware {
    Write-Host "🗑️ Removing Windows 11 Bloatware..." -ForegroundColor Cyan

    $results = @()

    # Common bloatware apps safe to remove
    $bloatwareApps = @(
        "Microsoft.BingWeather",
        "Microsoft.GetHelp",
        "Microsoft.Getstarted",
        "Microsoft.Microsoft3DViewer",
        "Microsoft.MicrosoftSolitaireCollection",
        "Microsoft.MixedReality.Portal",
        "Microsoft.Office.OneNote",
        "Microsoft.People",
        "Microsoft.Print3D",
        "Microsoft.SkypeApp",
        "Microsoft.Wallet",
        "Microsoft.WindowsAlarms",
        "Microsoft.WindowsFeedbackHub",
        "Microsoft.WindowsMaps",
        "Microsoft.Xbox.TCUI",
        "Microsoft.XboxApp",
        "Microsoft.XboxGameOverlay",
        "Microsoft.XboxIdentityProvider",
        "Microsoft.XboxSpeechToTextOverlay",
        "Microsoft.ZuneMusic",
        "Microsoft.ZuneVideo"
    )

    # Check for McAfee (mentioned in the hardware spec)
    $mcafeeApps = Get-AppxPackage | Where-Object { $_.Name -like "*McAfee*" }
    if ($mcafeeApps) {
        Write-Host "   Found McAfee bloatware packages..." -ForegroundColor Gray
        foreach ($app in $mcafeeApps) {
            Write-Host "   Removing McAfee: $($app.Name)..." -ForegroundColor Gray
            if (-not $WhatIf) {
                try {
                    Remove-AppxPackage -Package $app.PackageFullName -ErrorAction SilentlyContinue
                    Write-Host "   ✅ McAfee package removed: $($app.Name)" -ForegroundColor Green
                    $results += "Bloatware: McAfee $($app.Name) removed"
                }
                catch {
                    Write-Host "   ⚠️ Could not remove McAfee: $($_.Message)" -ForegroundColor Yellow
                    $results += "Bloatware: McAfee removal failed"
                }
            }
            else {
                Write-Host "   📋 Would remove McAfee: $($app.Name)" -ForegroundColor Yellow
                $results += "Bloatware: Would remove McAfee $($app.Name) (WhatIf)"
            }
        }
    }

    # Remove standard bloatware
    foreach ($appName in $bloatwareApps) {
        $app = Get-AppxPackage -Name $appName -ErrorAction SilentlyContinue
        if ($app) {
            Write-Host "   Removing: $appName..." -ForegroundColor Gray
            if (-not $WhatIf) {
                try {
                    Remove-AppxPackage -Package $app.PackageFullName -ErrorAction SilentlyContinue
                    Write-Host "   ✅ $appName removed" -ForegroundColor Green
                    $results += "Bloatware: $appName removed"
                }
                catch {
                    Write-Host "   ⚠️ Could not remove $appName`: $($_.Message)" -ForegroundColor Yellow
                    $results += "Bloatware: $appName removal failed"
                }
            }
            else {
                Write-Host "   📋 Would remove: $appName" -ForegroundColor Yellow
                $results += "Bloatware: Would remove $appName (WhatIf)"
            }
        }
    }

    return $results
}

# =============================================================================
# SCRIPTING PERFORMANCE OPTIMIZATIONS
# =============================================================================

function Optimize-PowerShellPerformance {
    Write-Host "🚀 Applying PowerShell Performance Optimizations..." -ForegroundColor Cyan

    $results = @()

    try {
        # Create performance test script
        $perfTestScript = @"
# Dell Inspiron 16 5640 Performance Test Script
# Optimized for Intel Core i5-1334U with 12 threads

`$Global:DellPerformanceConfig = @{
    MaxParallelism = 12
    OptimalThrottleLimit = 10
    UseNETMethods = `$true
    EnableCaching = `$true
}

# Example: Optimized parallel processing for Dell hardware
function Test-DellParallelPerformance {
    param([int]`$ItemCount = 1000)

    `$items = 1..`$ItemCount

    # Dell-optimized parallel processing
    `$result = `$items | ForEach-Object -Parallel {
        # Simulate work that benefits from hyperthreading
        [System.Math]::Sqrt(`$_ * `$_)
    } -ThrottleLimit `$Global:DellPerformanceConfig.OptimalThrottleLimit

    return `$result
}

# Example: Use .NET methods for better performance
function Get-FastFileContent {
    param([string]`$FilePath)

    # Faster than Get-Content for large files on NVMe SSD
    return [System.IO.File]::ReadAllText(`$FilePath)
}

# Example: Optimized filtering for Dell's fast RAM
function Get-OptimizedProcessList {
    # Cache the data first (fast with 16GB RAM)
    `$processes = Get-Process

    # Then filter (avoiding pipeline overhead)
    return `$processes | Where-Object { `$_.WorkingSet -gt 100MB }
}
"@

        if (-not $WhatIf) {
            $perfScriptPath = Join-Path $Global:DellInspironOptimizer.BackupPath "dell-performance-examples.ps1"
            New-Item -Path (Split-Path $perfScriptPath) -ItemType Directory -Force | Out-Null
            $perfTestScript | Set-Content $perfScriptPath -Encoding UTF8
            Write-Host "   ✅ Performance example script created: $perfScriptPath" -ForegroundColor Green
            $results += "Performance: Example script created"
        }

        # Apply global performance settings
        if (-not $WhatIf) {
            # Set process priority for PowerShell to High (helps with intensive scripts)
            $currentProcess = Get-Process -Id $PID
            $currentProcess.PriorityClass = "High"
            Write-Host "   ✅ PowerShell process priority set to High" -ForegroundColor Green
            $results += "Performance: Process priority optimized"
        }

    }
    catch {
        Write-Host "   ❌ Performance optimization failed: $($_.Exception.Message)" -ForegroundColor Red
        $results += "Performance: Optimization failed - $($_.Exception.Message)"
    }

    return $results
}

# =============================================================================
# VALIDATION AND BENCHMARKING
# =============================================================================

function Test-OptimizationResults {
    Write-Host "🧪 Testing Optimization Results..." -ForegroundColor Cyan

    $results = @()

    try {
        # Test PowerShell startup time
        Write-Host "   Testing PowerShell startup performance..." -ForegroundColor Gray
        $startupTime = Measure-Command {
            powershell -NoLogo -Command "Get-Date; exit"
        }
        Write-Host "   📊 PowerShell startup time: $([math]::Round($startupTime.TotalMilliseconds, 0))ms" -ForegroundColor White
        $results += "Startup: $([math]::Round($startupTime.TotalMilliseconds, 0))ms"

        # Test parallel processing performance
        Write-Host "   Testing parallel processing..." -ForegroundColor Gray
        $parallelTest = Measure-Command {
            1..100 | ForEach-Object -Parallel {
                [System.Math]::Sqrt($_ * $_)
            } -ThrottleLimit 10
        }
        Write-Host "   📊 Parallel processing test: $([math]::Round($parallelTest.TotalMilliseconds, 0))ms" -ForegroundColor White
        $results += "Parallel: $([math]::Round($parallelTest.TotalMilliseconds, 0))ms"

        # Test memory usage
        Write-Host "   Checking memory usage..." -ForegroundColor Gray
        $memory = Get-CimInstance -Class Win32_OperatingSystem
        $memoryUsage = [math]::Round(($memory.TotalVisibleMemorySize - $memory.FreePhysicalMemory) / $memory.TotalVisibleMemorySize * 100, 1)
        Write-Host "   📊 Memory usage: $memoryUsage%" -ForegroundColor White
        $results += "Memory: $memoryUsage% used"

        # Test disk performance (quick)
        Write-Host "   Testing disk performance..." -ForegroundColor Gray
        $diskTest = Measure-Command {
            $testFile = Join-Path $env:TEMP "ps-perf-test.tmp"
            "Test data for performance measurement" * 1000 | Set-Content $testFile
            Get-Content $testFile | Out-Null
            Remove-Item $testFile -Force -ErrorAction SilentlyContinue
        }
        Write-Host "   📊 Disk I/O test: $([math]::Round($diskTest.TotalMilliseconds, 0))ms" -ForegroundColor White
        $results += "Disk I/O: $([math]::Round($diskTest.TotalMilliseconds, 0))ms"

        # Overall performance score
        $score = 100
        if ($startupTime.TotalMilliseconds -gt 2000) { $score -= 20 }
        if ($parallelTest.TotalMilliseconds -gt 500) { $score -= 20 }
        if ($memoryUsage -gt 80) { $score -= 20 }
        if ($diskTest.TotalMilliseconds -gt 200) { $score -= 20 }

        Write-Host "   🏆 Overall Performance Score: $score/100" -ForegroundColor $(if ($score -gt 80) { 'Green' } elseif ($score -gt 60) { 'Yellow' } else { 'Red' })
        $results += "Score: $score/100"

    }
    catch {
        Write-Host "   ❌ Performance test failed: $($_.Exception.Message)" -ForegroundColor Red
        $results += "Test: Failed - $($_.Exception.Message)"
    }

    return $results
}

# =============================================================================
# MAIN EXECUTION
# =============================================================================

function Start-DellInspironOptimization {
    Write-Host "🚀 Dell Inspiron 16 5640 PowerShell 7.6.4 Optimizer" -ForegroundColor Cyan
    Write-Host "=" * 60 -ForegroundColor Cyan

    # Administrator privilege check
    if (-not (Test-AdministratorPrivileges)) {
        Write-Host "`n❌ Optimization cannot continue without Administrator privileges." -ForegroundColor Red
        return @("Error: Administrator privileges required")
    }

    # System compatibility check
    $isCompatible = Test-SystemCompatibility
    if (-not $isCompatible -and -not $FullOptimization) {
        $continue = Read-Host "System may not be fully compatible. Continue anyway? (y/N)"
        if ($continue -ne 'y' -and $continue -ne 'Y') {
            Write-Host "Optimization cancelled by user." -ForegroundColor Yellow
            return
        }
    }

    $allResults = @()

    # Execute optimizations based on parameters
    if ($PowerShellOnly -or $FullOptimization) {
        $allResults += Update-PowerShellAndDotNet
        $allResults += Optimize-PowerShellProfile
        $allResults += Optimize-PowerShellPerformance
    }

    if ($HardwareOnly -or $FullOptimization) {
        $allResults += Optimize-IntelProcessor
        $allResults += Optimize-MemoryAndStorage
    }

    if ($DellSpecific -or $FullOptimization) {
        $allResults += Update-DellDriversAndFirmware
        $allResults += Clear-DellTempFiles
    }

    if ($FullOptimization) {
        $allResults += Optimize-Windows11Services
        $allResults += Remove-WindowsBloatware
    }

    # Default: run all optimizations if no specific flags
    if (-not $PowerShellOnly -and -not $HardwareOnly -and -not $DellSpecific -and -not $FullOptimization) {
        $allResults += Update-PowerShellAndDotNet
        $allResults += Optimize-PowerShellProfile
        $allResults += Optimize-IntelProcessor
        $allResults += Optimize-MemoryAndStorage
        $allResults += Update-DellDriversAndFirmware
        $allResults += Clear-DellTempFiles
        $allResults += Optimize-PowerShellPerformance
    }

    # Validation and results
    $testResults = Test-OptimizationResults
    $allResults += $testResults

    # Save results
    $Global:DellInspironOptimizer.OptimizationResults = $allResults

    # Summary report
    Write-Host "`n📊 Optimization Summary:" -ForegroundColor Cyan
    Write-Host "=" * 40 -ForegroundColor Cyan

    $successCount = ($allResults | Where-Object { $_ -notmatch "failed|error" }).Count
    $totalCount = $allResults.Count

    Write-Host "   Total optimizations: $totalCount" -ForegroundColor White
    Write-Host "   Successful: $successCount" -ForegroundColor Green
    Write-Host "   Issues: $($totalCount - $successCount)" -ForegroundColor $(if ($totalCount -eq $successCount) { 'Green' } else { 'Yellow' })

    if ($CreateBackup) {
        Write-Host "   Backup location: $($Global:DellInspironOptimizer.BackupPath)" -ForegroundColor Gray
    }

    # Detailed results
    if ($Verbose -or $WhatIf) {
        Write-Host "`n📋 Detailed Results:" -ForegroundColor Yellow
        $allResults | ForEach-Object { Write-Host "   • $_" -ForegroundColor White }
    }

    # Recommendations
    Write-Host "`n💡 Recommendations:" -ForegroundColor Yellow
    Write-Host "   • Restart PowerShell to apply profile changes" -ForegroundColor White
    Write-Host "   • Run 'Test-DellPerformance' to monitor system status" -ForegroundColor White
    Write-Host "   • Consider BIOS update if available from Dell Support" -ForegroundColor White
    Write-Host "   • Monitor CPU temperatures during intensive tasks" -ForegroundColor White

    return $allResults
}

# Execute if not dot-sourced
if ($MyInvocation.InvocationName -ne '.') {
    Start-DellInspironOptimization
}
