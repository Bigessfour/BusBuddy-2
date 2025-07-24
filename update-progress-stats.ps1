# BusBuddy Progress Statistics Updater
# Updates README.md progress tracking section with current session data

param(
    [Parameter(Mandatory = $false)]
    [string]$SessionNumber = "",

    [Parameter(Mandatory = $false)]
    [string]$SessionFocus = "",

    [Parameter(Mandatory = $false)]
    [string]$TimeSpent = "",

    [Parameter(Mandatory = $false)]
    [string[]]$Achievements = @(),

    [Parameter(Mandatory = $false)]
    [string[]]$InProgress = @(),

    [Parameter(Mandatory = $false)]
    [string[]]$Issues = @(),

    [Parameter(Mandatory = $false)]
    [string[]]$NextGoals = @(),

    [Parameter(Mandatory = $false)]
    [switch]$UpdateComponentStatus,

    [Parameter(Mandatory = $false)]
    [string]$ComponentName = "",

    [Parameter(Mandatory = $false)]
    [ValidateSet("COMPLETE", "IN PROGRESS", "PENDING", "BLOCKED", "DEFERRED")]
    [string]$ComponentStatus = "",

    [Parameter(Mandatory = $false)]
    [ValidateRange(0, 100)]
    [int]$ComponentProgress = 0,

    [Parameter(Mandatory = $false)]
    [switch]$ShowCurrentStats,

    [Parameter(Mandatory = $false)]
    [switch]$Help
)

function Show-Help {
    Write-Host "🚌 BusBuddy Progress Statistics Updater" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "📊 Update session progress:" -ForegroundColor Yellow
    Write-Host "  .\update-progress-stats.ps1 -SessionNumber 2 -SessionFocus 'UI Testing' -TimeSpent '2 hours'" -ForegroundColor Green
    Write-Host ""
    Write-Host "🔧 Update component status:" -ForegroundColor Yellow
    Write-Host "  .\update-progress-stats.ps1 -UpdateComponentStatus -ComponentName 'Dashboard View' -ComponentStatus 'IN PROGRESS' -ComponentProgress 50" -ForegroundColor Green
    Write-Host ""
    Write-Host "📈 Show current statistics:" -ForegroundColor Yellow
    Write-Host "  .\update-progress-stats.ps1 -ShowCurrentStats" -ForegroundColor Green
    Write-Host ""
    Write-Host "📋 Add achievements:" -ForegroundColor Yellow
    Write-Host "  .\update-progress-stats.ps1 -SessionNumber 2 -Achievements @('Fixed navigation', 'Added dashboard')" -ForegroundColor Green
}

function Show-CurrentStats {
    Write-Host "📊 Current BusBuddy Development Statistics" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray

    # Read current stats from README
    $readmePath = "README.md"
    if (Test-Path $readmePath) {
        $content = Get-Content $readmePath -Raw

        # Extract progress percentages using regex
        if ($content -match 'FOUNDATION: [█░]*\s*(\d+)%') {
            $foundation = $matches[1]
            Write-Host "🏗️  Foundation: $foundation%" -ForegroundColor Green
        }

        if ($content -match 'CORE SYSTEMS: [█░]*\s*(\d+)%') {
            $coreSystems = $matches[1]
            Write-Host "🔧 Core Systems: $coreSystems%" -ForegroundColor Yellow
        }

        if ($content -match 'UI TESTING: [█░]*\s*(\d+)%') {
            $uiTesting = $matches[1]
            Write-Host "🖥️  UI Testing: $uiTesting%" -ForegroundColor Magenta
        }

        if ($content -match 'INTEGRATION: [█░]*\s*(\d+)%') {
            $integration = $matches[1]
            Write-Host "✅ Integration: $integration%" -ForegroundColor Blue
        }

        Write-Host ""
        Write-Host "📅 Last Updated: $(Get-Date -Format 'yyyy-MM-dd HH:mm')" -ForegroundColor Gray
    }
    else {
        Write-Host "❌ README.md not found in current directory" -ForegroundColor Red
    }
}

function Update-ProgressBar {
    param([int]$Progress, [int]$Width = 32)

    $filled = [math]::Floor($Progress * $Width / 100)
    $empty = $Width - $filled

    return ("█" * $filled) + ("░" * $empty)
}

function Add-SessionUpdate {
    $readmePath = "README.md"
    if (-not (Test-Path $readmePath)) {
        Write-Host "❌ README.md not found in current directory" -ForegroundColor Red
        return
    }

    $currentDate = Get-Date -Format "yyyy-MM-dd"

    # Build session update block
    $sessionUpdate = @"

### Session $SessionNumber Update - $currentDate
**Time Spent**: $TimeSpent | **Focus**: $SessionFocus
**Achievements**:
"@

    foreach ($achievement in $Achievements) {
        $sessionUpdate += "`n- ✅ $achievement"
    }

    if ($InProgress.Count -gt 0) {
        $sessionUpdate += "`n**In Progress**:"
        foreach ($item in $InProgress) {
            $sessionUpdate += "`n- 🔄 $item"
        }
    }

    if ($Issues.Count -gt 0) {
        $sessionUpdate += "`n**Blocked/Issues**:"
        foreach ($issue in $Issues) {
            $sessionUpdate += "`n- ⚠️ $issue"
        }
    }

    if ($NextGoals.Count -gt 0) {
        $sessionUpdate += "`n**Next Session Goals**:"
        foreach ($goal in $NextGoals) {
            $sessionUpdate += "`n- 🎯 $goal"
        }
    }

    # Find insertion point in README (after the template section)
    $content = Get-Content $readmePath -Raw
    $insertionPoint = $content.IndexOf("#### **📈 Progress Visualization Legend**")

    if ($insertionPoint -gt 0) {
        $beforeInsert = $content.Substring(0, $insertionPoint)
        $afterInsert = $content.Substring($insertionPoint)

        $newContent = $beforeInsert + $sessionUpdate + "`n`n" + $afterInsert
        Set-Content $readmePath $newContent -NoNewline

        Write-Host "✅ Session update added to README.md" -ForegroundColor Green
    }
    else {
        Write-Host "❌ Could not find insertion point in README.md" -ForegroundColor Red
    }
}

function Update-Component {
    $readmePath = "README.md"
    if (-not (Test-Path $readmePath)) {
        Write-Host "❌ README.md not found in current directory" -ForegroundColor Red
        return
    }

    $content = Get-Content $readmePath -Raw
    $currentDate = Get-Date -Format "yyyy-MM-dd HH:mm"

    # Status emoji mapping
    $statusEmoji = @{
        "COMPLETE"    = "✅ **COMPLETE**"
        "IN PROGRESS" = "🔄 **IN PROGRESS**"
        "PENDING"     = "⏳ **PENDING**"
        "BLOCKED"     = "⚠️ **BLOCKED**"
        "DEFERRED"    = "🚫 **DEFERRED**"
    }

    # Find and update component status
    $pattern = "\| \*\*.*$ComponentName.*\*\* \| .* \| \d+% \| .* \|"
    if ($content -match $pattern) {
        $statusText = $statusEmoji[$ComponentStatus]
        $replacement = "| **$ComponentName** | $statusText | $ComponentProgress% | $currentDate | Maintenance |"

        $newContent = $content -replace [regex]::Escape($matches[0]), $replacement
        Set-Content $readmePath $newContent -NoNewline

        Write-Host "✅ Component '$ComponentName' updated to $ComponentStatus ($ComponentProgress%)" -ForegroundColor Green
    }
    else {
        Write-Host "❌ Component '$ComponentName' not found in progress table" -ForegroundColor Red
    }
}

# Main execution
if ($Help) {
    Show-Help
    return
}

if ($ShowCurrentStats) {
    Show-CurrentStats
    return
}

if ($UpdateComponentStatus) {
    if (-not $ComponentName -or -not $ComponentStatus) {
        Write-Host "❌ Component name and status are required for component updates" -ForegroundColor Red
        Show-Help
        return
    }
    Update-Component
    return
}

if ($SessionNumber) {
    if (-not $SessionFocus) {
        Write-Host "❌ Session focus is required for session updates" -ForegroundColor Red
        Show-Help
        return
    }
    Add-SessionUpdate
    return
}

# Default: show current stats
Show-CurrentStats
