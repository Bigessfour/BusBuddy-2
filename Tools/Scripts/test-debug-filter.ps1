#Requires -Version 7.0
<#
.SYNOPSIS
    Bus Buddy Debug Filter - WPF Scheduling Focus
    
.DESCRIPTION
    Enhanced debug filtering tool specifically designed for Bus Buddy WPF scheduling system.
    Filters and categorizes debug output with special attention to:
    - Syncfusion scheduler binding issues
    - DateTime validation errors  
    - Data input validation for scheduling
    - UI thread synchronization issues
    
.PARAMETER LogPath
    Path to log file or directory to monitor (defaults to project logs folder)
    
.PARAMETER OutputPath  
    Path for filtered output file (defaults to logs/debug-filtered.log)
    
.PARAMETER Mode
    Filter mode: Stream (real-time), File (process existing), or Both
    
.PARAMETER Priority
    Minimum priority level to display (1=Critical, 2=High, 3=Medium, 4=Low)
    
.PARAMETER SchedulingFocus
    Enable enhanced filtering for scheduling-specific issues
    
.PARAMETER UINotifications
    Show desktop notifications for critical issues
    
.EXAMPLE
    .\test-debug-filter.ps1 -Mode Stream -SchedulingFocus -UINotifications
    
.EXAMPLE
    .\test-debug-filter.ps1 -LogPath "logs\busbuddy-*.log" -Priority 2
    
.NOTES
    Author: Bus Buddy Development Team
    Version: 2.0
    Focus: WPF Scheduling, Syncfusion DateTime Controls, Data Validation
#>

param(
    [string]$LogPath = "",
    [string]$OutputPath = "",
    [ValidateSet('Stream', 'File', 'Both')]$Mode = 'Stream',
    [ValidateRange(1,4)][int]$Priority = 3,
    [switch]$SchedulingFocus,
    [switch]$UINotifications,
    [switch]$Verbose
)

# ═══════════════════════════════════════════════════════════════════════════════════
# CONFIGURATION FOR BUS BUDDY WPF SCHEDULING
# ═══════════════════════════════════════════════════════════════════════════════════

$script:Config = @{
    # Auto-detect project paths
    ProjectRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
    LogsFolder = ""
    OutputFolder = ""
    
    # Priority levels for scheduling system
    PriorityLevels = @{
        1 = @{ Name = 'CRITICAL'; Color = 'Red'; Icon = '🔥' }
        2 = @{ Name = 'HIGH'; Color = 'Yellow'; Icon = '⚠️' }
        3 = @{ Name = 'MEDIUM'; Color = 'Cyan'; Icon = 'ℹ️' }
        4 = @{ Name = 'LOW'; Color = 'Gray'; Icon = '🔍' }
    }
    
    # WPF Scheduling-Specific Error Patterns
    SchedulingPatterns = @{
        # Priority 1 - Critical Issues
        Critical = @{
            'DateTimeBinding' = @{
                Pattern = '(DateTime.*binding.*failed|Invalid.*DateTime.*format|DateTimePattern.*error)'
                Description = 'DateTime binding failure in scheduler controls'
                Solution = 'Check DateTimePattern property and binding path'
            }
            'SchedulerCrash' = @{
                Pattern = '(SfScheduler.*exception|Scheduler.*null.*reference|AppointmentMapping.*error)'
                Description = 'Syncfusion Scheduler critical error'
                Solution = 'Verify AppointmentMapping and data source configuration'
            }
            'DataValidation' = @{
                Pattern = '(Validation.*failed.*schedule|Invalid.*schedule.*data|Schedule.*constraint.*violation)'
                Description = 'Schedule data validation failure'
                Solution = 'Check schedule data integrity and business rules'
            }
            'UIThread' = @{
                Pattern = '(Cross.*thread.*operation|UI.*thread.*violation|Dispatcher.*invoke.*error)'
                Description = 'UI thread synchronization error'
                Solution = 'Use Dispatcher.Invoke for UI updates from background threads'
            }
        }
        
        # Priority 2 - High Issues  
        High = @{
            'SyncfusionBinding' = @{
                Pattern = '(Syncfusion.*binding.*error|SfDatePicker.*binding|SfTimePicker.*binding)'
                Description = 'Syncfusion control binding issue'
                Solution = 'Verify binding paths and FallbackValues'
            }
            'ScheduleConflict' = @{
                Pattern = '(Schedule.*conflict|Appointment.*overlap|Route.*assignment.*conflict)'
                Description = 'Scheduling conflict detected'
                Solution = 'Check schedule validation rules and conflict resolution'
            }
            'DataInput' = @{
                Pattern = '(Invalid.*input.*format|Input.*validation.*failed|Format.*exception.*input)'
                Description = 'Data input validation failure'
                Solution = 'Implement proper input validation and format conversion'
            }
            'ResourceLoad' = @{
                Pattern = '(Resource.*not.*found|XAML.*load.*error|Style.*not.*found)'
                Description = 'Resource loading issue'
                Solution = 'Check resource dictionary references and XAML syntax'
            }
        }
        
        # Priority 3 - Medium Issues
        Medium = @{
            'PerformanceWarning' = @{
                Pattern = '(Performance.*warning|Slow.*query|UI.*freeze|Long.*running.*operation)'
                Description = 'Performance-related warning'
                Solution = 'Consider asynchronous operations and UI virtualization'
            }
            'BindingWarning' = @{
                Pattern = '(Binding.*warning|Property.*not.*found|Converter.*warning)'
                Description = 'Data binding warning'
                Solution = 'Check binding paths and add FallbackValues'
            }
            'ConfigurationIssue' = @{
                Pattern = '(Configuration.*warning|Setting.*not.*found|Default.*value.*used)'
                Description = 'Configuration or settings issue'
                Solution = 'Verify application configuration files'
            }
        }
        
        # Priority 4 - Low Issues  
        Low = @{
            'DebugInfo' = @{
                Pattern = '(Debug:|Verbose:|Information:)'
                Description = 'Debug information'
                Solution = 'Informational only'
            }
            'TraceInfo' = @{
                Pattern = '(Trace:|Navigation:|State.*change)'
                Description = 'Application trace information'
                Solution = 'Informational only'
            }
        }
    }
    
    # Enhanced patterns when SchedulingFocus is enabled
    EnhancedSchedulingPatterns = @{
        'InvalidDate' = 'Invalid.*Date.*|Date.*out.*of.*range|DateTime.*parse.*error'
        'SyncfusionScheduler' = 'SfScheduler.*|Appointment.*|Schedule.*View.*|Calendar.*'
        'RouteAssignment' = 'Route.*assignment|Bus.*route|Student.*route|Route.*conflict'
        'TimeValidation' = 'Time.*validation|Invalid.*time|Time.*format.*error|AM.*PM.*error'
        'RecurrenceError' = 'Recurrence.*pattern|Recurring.*appointment|Schedule.*repeat'
        'ViewModelBinding' = 'ViewModel.*binding|ObservableCollection.*|PropertyChanged.*'
    }
}

# Initialize paths
if ([string]::IsNullOrEmpty($LogPath)) {
    $script:Config.LogsFolder = Join-Path $script:Config.ProjectRoot "logs"
    $LogPath = Join-Path $script:Config.LogsFolder "*.log"
}

if ([string]::IsNullOrEmpty($OutputPath)) {
    $script:Config.OutputFolder = Join-Path $script:Config.ProjectRoot "logs"
    $OutputPath = Join-Path $script:Config.OutputFolder "debug-filtered-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
}

# ═══════════════════════════════════════════════════════════════════════════════════
# CORE FUNCTIONS
# ═══════════════════════════════════════════════════════════════════════════════════

function Write-FilteredMessage {
    param(
        [string]$Message,
        [int]$Priority,
        [string]$Category = '',
        [string]$Source = ''
    )
    
    $priorityInfo = $script:Config.PriorityLevels[$Priority]
    $timestamp = Get-Date -Format 'HH:mm:ss.fff'
    
    $formattedMessage = "[$timestamp] $($priorityInfo.Icon) [$($priorityInfo.Name)]"
    if ($Category) { $formattedMessage += " [$Category]" }
    if ($Source) { $formattedMessage += " ($Source)" }
    $formattedMessage += " $Message"
    
    # Console output with color
    Write-Host $formattedMessage -ForegroundColor $priorityInfo.Color
    
    # File output
    $formattedMessage | Add-Content -Path $OutputPath -Encoding UTF8
    
    # UI Notifications for critical issues
    if ($UINotifications -and $Priority -eq 1) {
        Show-DesktopNotification -Title "Bus Buddy Critical Error" -Message $Message
    }
}

function Show-DesktopNotification {
    param(
        [string]$Title,
        [string]$Message
    )
    
    try {
        # Windows 10/11 toast notification
        $null = [Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime]
        $null = [Windows.Data.Xml.Dom.XmlDocument, Windows.Data.Xml.Dom.XmlDocument, ContentType = WindowsRuntime]
        
        $toastXml = [Windows.Data.Xml.Dom.XmlDocument]::new()
        $toastXml.LoadXml(@"
<toast>
    <visual>
        <binding template="ToastText02">
            <text id="1">$Title</text>
            <text id="2">$Message</text>
        </binding>
    </visual>
</toast>
"@)
        
        $toast = [Windows.UI.Notifications.ToastNotification]::new($toastXml)
        [Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier("Bus Buddy WPF").Show($toast)
    }
    catch {
        # Fallback to balloon tip
        Add-Type -AssemblyName System.Windows.Forms
        $balloon = New-Object System.Windows.Forms.NotifyIcon
        $balloon.Icon = [System.Drawing.SystemIcons]::Information
        $balloon.BalloonTipIcon = 'Error'
        $balloon.BalloonTipText = $Message
        $balloon.BalloonTipTitle = $Title
        $balloon.Visible = $true
        $balloon.ShowBalloonTip(5000)
    }
}

function Test-LogPattern {
    param(
        [string]$LogLine,
        [hashtable]$PatternSet,
        [int]$Priority
    )
    
    foreach ($category in $PatternSet.Keys) {
        $patternInfo = $PatternSet[$category]
        
        if ($LogLine -match $patternInfo.Pattern) {
            return @{
                Matched = $true
                Category = $category
                Priority = $Priority
                Description = $patternInfo.Description
                Solution = $patternInfo.Solution
                Pattern = $patternInfo.Pattern
            }
        }
    }
    
    return @{ Matched = $false }
}

function Get-EnhancedSchedulingMatch {
    param([string]$LogLine)
    
    if (-not $SchedulingFocus) { return $null }
    
    foreach ($category in $script:Config.EnhancedSchedulingPatterns.Keys) {
        $pattern = $script:Config.EnhancedSchedulingPatterns[$category]
        if ($LogLine -match $pattern) {
            return @{
                Category = "Enhanced_$category"
                Priority = 2  # High priority for scheduling issues
                Description = "Enhanced scheduling pattern detected: $category"
                Solution = "Review scheduling logic and data flow"
            }
        }
    }
    
    return $null
}

function Process-LogLine {
    param(
        [string]$LogLine,
        [string]$Source = ''
    )
    
    if ([string]::IsNullOrWhiteSpace($LogLine)) { return }
    
    # Enhanced scheduling pattern check first
    $enhancedMatch = Get-EnhancedSchedulingMatch -LogLine $LogLine
    if ($enhancedMatch) {
        if ($enhancedMatch.Priority -le $Priority) {
            Write-FilteredMessage -Message $LogLine -Priority $enhancedMatch.Priority -Category $enhancedMatch.Category -Source $Source
        }
        return
    }
    
    # Standard pattern matching
    foreach ($priorityLevel in $script:Config.SchedulingPatterns.Keys) {
        $priorityNum = switch ($priorityLevel) {
            'Critical' { 1 }
            'High' { 2 }
            'Medium' { 3 }
            'Low' { 4 }
        }
        
        if ($priorityNum -le $Priority) {
            $match = Test-LogPattern -LogLine $LogLine -PatternSet $script:Config.SchedulingPatterns[$priorityLevel] -Priority $priorityNum
            
            if ($match.Matched) {
                $message = $LogLine
                if ($Verbose) {
                    $message += "`n    📋 $($match.Description)`n    💡 $($match.Solution)"
                }
                
                Write-FilteredMessage -Message $message -Priority $match.Priority -Category $match.Category -Source $Source
                return  # Only match the first (highest priority) pattern
            }
        }
    }
}

function Start-FileProcessing {
    param([string]$FilePath)
    
    Write-Host "🔍 Processing log file: $FilePath" -ForegroundColor Cyan
    
    if (-not (Test-Path $FilePath)) {
        Write-Host "❌ File not found: $FilePath" -ForegroundColor Red
        return
    }
    
    $content = Get-Content $FilePath
    $lineCount = 0
    $matchCount = 0
    
    foreach ($line in $content) {
        $lineCount++
        $initialMatches = (Get-Content $OutputPath -ErrorAction SilentlyContinue | Measure-Object).Count
        
        Process-LogLine -LogLine $line -Source (Split-Path $FilePath -Leaf)
        
        $newMatches = (Get-Content $OutputPath -ErrorAction SilentlyContinue | Measure-Object).Count
        if ($newMatches -gt $initialMatches) {
            $matchCount++
        }
    }
    
    Write-Host "📊 Processed $lineCount lines, found $matchCount matches" -ForegroundColor Green
}

function Start-StreamProcessing {
    Write-Host "🔄 Starting real-time log monitoring..." -ForegroundColor Cyan
    Write-Host "📁 Monitoring: $LogPath" -ForegroundColor Cyan
    Write-Host "📝 Output: $OutputPath" -ForegroundColor Cyan
    Write-Host "🎯 Priority: $Priority and above" -ForegroundColor Cyan
    Write-Host "Press Ctrl+C to stop monitoring`n" -ForegroundColor Yellow
    
    # Get list of log files to monitor
    $logFiles = Get-ChildItem -Path $LogPath | Sort-Object LastWriteTime -Descending
    
    if ($logFiles.Count -eq 0) {
        Write-Host "❌ No log files found matching: $LogPath" -ForegroundColor Red
        return
    }
    
    $lastPositions = @{}
    
    # Initialize positions for each file
    foreach ($file in $logFiles) {
        $lastPositions[$file.FullName] = (Get-Item $file.FullName).Length
    }
    
    try {
        while ($true) {
            foreach ($file in $logFiles) {
                if (Test-Path $file.FullName) {
                    $currentLength = (Get-Item $file.FullName).Length
                    $lastPosition = $lastPositions[$file.FullName]
                    
                    if ($currentLength -gt $lastPosition) {
                        # Read new content
                        $stream = [System.IO.File]::OpenRead($file.FullName)
                        $stream.Seek($lastPosition, [System.IO.SeekOrigin]::Begin) | Out-Null
                        $reader = New-Object System.IO.StreamReader($stream)
                        
                        while (-not $reader.EndOfStream) {
                            $line = $reader.ReadLine()
                            if ($line) {
                                Process-LogLine -LogLine $line -Source $file.Name
                            }
                        }
                        
                        $lastPositions[$file.FullName] = $stream.Position
                        $reader.Close()
                        $stream.Close()
                    }
                }
            }
            
            Start-Sleep -Milliseconds 500
        }
    }
    catch {
        Write-Host "`n🔴 Monitoring stopped: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# ═══════════════════════════════════════════════════════════════════════════════════
# MAIN EXECUTION
# ═══════════════════════════════════════════════════════════════════════════════════

function Main {
    Write-Host "🚌 Bus Buddy Debug Filter v2.0" -ForegroundColor Cyan
    Write-Host "🎯 Focus: WPF Scheduling System, Syncfusion Controls" -ForegroundColor Cyan
    
    # Create output directory if needed
    $outputDir = Split-Path $OutputPath -Parent
    if (-not (Test-Path $outputDir)) {
        New-Item -Path $outputDir -ItemType Directory -Force | Out-Null
    }
    
    # Initialize output file
    "# Bus Buddy Debug Filter Output - $(Get-Date)" | Set-Content -Path $OutputPath -Encoding UTF8
    "# Priority Level: $Priority and above" | Add-Content -Path $OutputPath -Encoding UTF8
    "# Scheduling Focus: $SchedulingFocus" | Add-Content -Path $OutputPath -Encoding UTF8
    "" | Add-Content -Path $OutputPath -Encoding UTF8
    
    switch ($Mode) {
        'File' {
            $files = Get-ChildItem -Path $LogPath
            foreach ($file in $files) {
                Start-FileProcessing -FilePath $file.FullName
            }
        }
        'Stream' {
            Start-StreamProcessing
        }
        'Both' {
            # Process existing files first
            $files = Get-ChildItem -Path $LogPath
            foreach ($file in $files) {
                Start-FileProcessing -FilePath $file.FullName
            }
            
            Write-Host "`n🔄 Switching to real-time monitoring..." -ForegroundColor Cyan
            Start-StreamProcessing
        }
    }
    
    Write-Host "`n✅ Debug filtering complete. Output saved to: $OutputPath" -ForegroundColor Green
}

# Execute main function
Main
