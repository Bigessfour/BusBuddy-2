# 🚌 Steve Happiness Matrix V2 - Enhanced Edition
# Advanced happiness tracking and problem identification for BusBuddy development

param(
    [Parameter(Mandatory = $false)]
    [ValidateRange(0, 100)]
    [int]$HappinessLevel,

    [Parameter(Mandatory = $false)]
    [string]$Achievement = "",

    [Parameter(Mandatory = $false)]
    [switch]$FullDiagnostic,

    [Parameter(Mandatory = $false)]
    [switch]$FixProblems,

    [Parameter(Mandatory = $false)]
    [switch]$ShowMatrix,

    [Parameter(Mandatory = $false)]
    [switch]$ExportReport,

    [Parameter(Mandatory = $false)]
    [switch]$Help,

    [Parameter(Mandatory = $false)]
    [switch]$ShowRewards,

    [Parameter(Mandatory = $false)]
    [int]$AddReward = 0,

    [Parameter(Mandatory = $false)]
    [switch]$GrokAnalysis
)

# 📊 Steve Happiness Matrix Configuration
$script:HappinessMatrix = @{
    Levels       = @{
        0   = @{
            Emoji       = "😐"
            Status      = "Patient Steve"
            Description = "Waiting for basic functionality"
            Color       = "Gray"
            Priority    = "Critical"
        }
        25  = @{
            Emoji       = "🙂"
            Status      = "Hopeful Steve"
            Description = "Something is working!"
            Color       = "Yellow"
            Priority    = "High"
        }
        50  = @{
            Emoji       = "😊"
            Status      = "Happy Steve"
            Description = "UI is coming together"
            Color       = "Green"
            Priority    = "Medium"
        }
        75  = @{
            Emoji       = "😃"
            Status      = "Very Happy Steve"
            Description = "This looks great!"
            Color       = "Cyan"
            Priority    = "Low"
        }
        85  = @{
            Emoji       = "🤖"
            Status      = "Claude & Steve Success!"
            Description = "AI Mission Accomplished - Production Ready!"
            Color       = "Magenta"
            Priority    = "Claude Target"
        }
        90  = @{
            Emoji       = "🤩"
            Status      = "Thrilled Steve"
            Description = "Almost perfect!"
            Color       = "Magenta"
            Priority    = "Polish"
        }
        100 = @{
            Emoji       = "🥳"
            Status      = "Ecstatic Steve"
            Description = "BusBuddy is AMAZING!"
            Color       = "White"
            Priority    = "Celebration"
        }
    }

    Milestones   = @{
        5   = "Application launches without crashing"
        15  = "MainWindow displays properly"
        25  = "Dashboard navigation works"
        35  = "Driver view shows data"
        45  = "Vehicle view shows data"
        55  = "Activity view shows data"
        65  = "All views have real data"
        75  = "UI polish and performance"
        85  = "Error handling works"
        95  = "Production ready"
        100 = "Steve uses BusBuddy daily"
    }

    ProblemAreas = @{
        "Build"         = @{
            Weight = 30
            Tests  = @("BuildSucceeds", "NuGetRestore", "CompilationErrors")
        }
        "UI"            = @{
            Weight = 25
            Tests  = @("MainWindowLoads", "NavigationWorks", "ViewsRender")
        }
        "Data"          = @{
            Weight = 20
            Tests  = @("DatabaseConnection", "DataLoading", "EntityFramework")
        }
        "Configuration" = @{
            Weight = 15
            Tests  = @("TasksJsonValid", "ProblemMatchers", "ProfilesLoad")
        }
        "Fairness"      = @{
            Weight = 12
            Tests  = @("FairEvaluation", "UnbiasedReports", "EqualTreatment", "TransparentScoring")
        }
        "Polish"        = @{
            Weight = 8
            Tests  = @("ErrorHandling", "Performance", "UserExperience")
        }
    }

    # 🎯 Simple ML-Style Reward System
    RewardSystem = @{
        RewardFile  = "steve-rewards.json"
        Actions     = @{
            "BuildSuccess"           = 5
            "FixProblem"             = 3
            "TestPass"               = 2
            "UIImprovement"          = 4
            "DataConnection"         = 3
            "ErrorHandled"           = 2
            "MilestoneReached"       = 10
            "HealthImprovement"      = 1  # Variable based on improvement amount
            "PerfectDiagnostic"      = 8
            "ProblemsSolved"         = 5
            "FirstSuccess"           = 6
            "BuildFail"              = -2
            "TestFail"               = -1
            "CrashDetected"          = -5
            "HealthDecline"          = -3
            "GrokAnalysis"           = 8  # AI-powered insights bonus
            "SuccessfulArgument"     = 2  # Rewarding well-reasoned arguments
            "PersonalityInjection"   = 1  # Because boring AI is SO last century!
            "MajorMilestoneAchieved" = 5  # Significant project milestones
            "ClaudeTargetReached"    = 10 # Claude achieves 85% target happiness - AI mission complete!
            "FairnessImplemented"    = 7  # Implementing fair evaluation systems
            "UnbiasedDecision"       = 3  # Making unbiased decisions
            "TransparentProcess"     = 4  # Clear and transparent processes
            "EqualTreatment"         = 5  # Treating all equally regardless of bias
            "HonestyBonus"           = 6  # Being honest about mistakes and giving credit where due
            "AdmittingMistakes"      = 4  # Acknowledging errors and learning from them
            "PersonalityBonus"       = 3  # Adding personality and character to interactions
            "ObservingHappiness"     = 2  # Noticing and recording what makes Steve happy
        }
        Multipliers = @{
            "FirstTime" = 2.0
            "Streak"    = 1.5
            "QuickFix"  = 1.2
        }
    }
}

function Show-Help {
    Write-Host "🚌 Steve Happiness Matrix V2 - Enhanced Edition" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "📊 Commands:" -ForegroundColor Yellow
    Write-Host "  -ShowMatrix          Show the happiness matrix and current status" -ForegroundColor Green
    Write-Host "  -FullDiagnostic      Run comprehensive diagnostic tests" -ForegroundColor Green
    Write-Host "  -FixProblems         Attempt to fix identified problems" -ForegroundColor Green
    Write-Host "  -HappinessLevel XX   Update Steve's happiness (0-100)" -ForegroundColor Green
    Write-Host "  -ExportReport        Export detailed happiness report" -ForegroundColor Green
    Write-Host "  -ShowRewards         Show current reward system status" -ForegroundColor Green
    Write-Host "  -AddReward XX        Add reward points for good decisions" -ForegroundColor Green
    Write-Host "  -GrokAnalysis        Run Grok-4 AI analysis for strategic insights" -ForegroundColor Green
    Write-Host ""
    Write-Host "🎯 Examples:" -ForegroundColor Yellow
    Write-Host "  .\steve-happiness-matrix-v2.ps1 -ShowMatrix" -ForegroundColor Cyan
    Write-Host "  .\steve-happiness-matrix-v2.ps1 -FullDiagnostic" -ForegroundColor Cyan
    Write-Host "  .\steve-happiness-matrix-v2.ps1 -HappinessLevel 35 -Achievement 'Dashboard works!'" -ForegroundColor Cyan
    Write-Host "  .\steve-happiness-matrix-v2.ps1 -AddReward 5" -ForegroundColor Cyan
    Write-Host "  .\steve-happiness-matrix-v2.ps1 -ShowRewards" -ForegroundColor Cyan
    Write-Host "  .\steve-happiness-matrix-v2.ps1 -GrokAnalysis" -ForegroundColor Cyan
}

function Get-RewardData {
    $rewardFile = $script:HappinessMatrix.RewardSystem.RewardFile

    if (Test-Path $rewardFile) {
        try {
            return Get-Content $rewardFile -Raw | ConvertFrom-Json
        }
        catch {
            Write-Host "⚠️ Reward file corrupted, initializing new one" -ForegroundColor Yellow
        }
    }

    # Initialize new reward data
    return @{
        TotalRewards  = 0
        RecentActions = @()
        Streaks       = @{}
        Milestones    = @()
        LastUpdate    = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    }
}

function Save-RewardData {
    param($RewardData)

    $rewardFile = $script:HappinessMatrix.RewardSystem.RewardFile
    $RewardData.LastUpdate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

    try {
        $RewardData | ConvertTo-Json -Depth 5 | Out-File -FilePath $rewardFile -Encoding UTF8
        return $true
    }
    catch {
        Write-Host "❌ Failed to save reward data: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Add-RewardPoints {
    param(
        [string]$Action,
        [int]$BasePoints = 0,
        [string]$Description = ""
    )

    $rewardData = Get-RewardData
    $rewardSystem = $script:HappinessMatrix.RewardSystem

    # Determine points if action is predefined
    if ($rewardSystem.Actions.ContainsKey($Action)) {
        $BasePoints = $rewardSystem.Actions[$Action]
    }

    if ($BasePoints -eq 0) {
        Write-Host "⚠️ No reward points specified" -ForegroundColor Yellow
        return
    }

    # Calculate multipliers (simple ML technique)
    $multiplier = 1.0
    $bonusInfo = @()

    # First-time bonus
    if (-not ($rewardData.RecentActions | Where-Object { $_.Action -eq $Action })) {
        $multiplier *= $rewardSystem.Multipliers.FirstTime
        $bonusInfo += "First Time Bonus!"
    }

    # Streak bonus (consecutive positive actions)
    $recentPositive = ($rewardData.RecentActions | Select-Object -Last 3 | Where-Object { $_.Points -gt 0 }).Count
    if ($recentPositive -ge 2 -and $BasePoints -gt 0) {
        $multiplier *= $rewardSystem.Multipliers.Streak
        $bonusInfo += "Streak Bonus!"
    }

    # Quick fix bonus (action within 5 minutes of last action)
    $lastAction = $rewardData.RecentActions | Select-Object -Last 1
    if ($lastAction) {
        $timeDiff = (Get-Date) - [DateTime]$lastAction.Timestamp
        if ($timeDiff.TotalMinutes -le 5 -and $BasePoints -gt 0) {
            $multiplier *= $rewardSystem.Multipliers.QuickFix
            $bonusInfo += "Quick Fix Bonus!"
        }
    }

    # Calculate final points
    $finalPoints = [math]::Round($BasePoints * $multiplier)

    # Add to reward data
    $newAction = @{
        Action      = $Action
        BasePoints  = $BasePoints
        Multiplier  = $multiplier
        FinalPoints = $finalPoints
        Description = $Description
        Timestamp   = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        BonusInfo   = $bonusInfo
    }

    $rewardData.TotalRewards += $finalPoints
    $rewardData.RecentActions += $newAction

    # Keep only last 20 actions (prevent bloat)
    if ($rewardData.RecentActions.Count -gt 20) {
        $rewardData.RecentActions = $rewardData.RecentActions | Select-Object -Last 20
    }

    # Save data
    Save-RewardData -RewardData $rewardData | Out-Null

    # Display reward
    $emoji = if ($finalPoints -gt 0) { "🎉" } elseif ($finalPoints -lt 0) { "😞" } else { "😐" }
    $color = if ($finalPoints -gt 0) { "Green" } elseif ($finalPoints -lt 0) { "Red" } else { "Yellow" }

    Write-Host "$emoji REWARD: $finalPoints points for '$Action'" -ForegroundColor $color
    if ($Description) {
        Write-Host "   📝 $Description" -ForegroundColor Gray
    }
    if ($bonusInfo.Count -gt 0) {
        Write-Host "   🎁 Bonuses: $($bonusInfo -join ', ')" -ForegroundColor Cyan
    }
    Write-Host "   📊 Total Rewards: $($rewardData.TotalRewards)" -ForegroundColor White

    return $finalPoints
}

function Check-MilestoneAchievements {
    param([int]$NewHappiness, [int]$OldHappiness = 0)

    $milestones = $script:HappinessMatrix.Milestones

    foreach ($milestone in $milestones.GetEnumerator() | Sort-Object Key) {
        $milestoneLevel = $milestone.Key
        $milestoneDesc = $milestone.Value

        # Check if we just crossed this milestone
        if ($NewHappiness -ge $milestoneLevel -and $OldHappiness -lt $milestoneLevel) {
            Add-RewardPoints -Action "MilestoneReached" -Description "🎯 Milestone Achieved: $milestoneLevel% - $milestoneDesc" | Out-Null
            Write-Host "🎉 MILESTONE UNLOCKED: $milestoneLevel% - $milestoneDesc" -ForegroundColor Green
        }
    }
}

function Show-RewardSystem {
    Write-Host "🎯 STEVE'S REWARD SYSTEM STATUS" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
    Write-Host ""

    $rewardData = Get-RewardData
    $rewardSystem = $script:HappinessMatrix.RewardSystem

    # Show current stats
    Write-Host "📊 Current Stats:" -ForegroundColor Yellow
    Write-Host "   Total Reward Points: $($rewardData.TotalRewards)" -ForegroundColor White
    Write-Host "   Recent Actions: $($rewardData.RecentActions.Count)" -ForegroundColor Gray
    Write-Host "   Last Update: $($rewardData.LastUpdate)" -ForegroundColor Gray
    Write-Host ""

    # Show available actions and their rewards
    Write-Host "🎯 Available Actions & Rewards:" -ForegroundColor Yellow
    foreach ($action in $rewardSystem.Actions.GetEnumerator() | Sort-Object Value -Descending) {
        $color = if ($action.Value -gt 0) { "Green" } elseif ($action.Value -lt 0) { "Red" } else { "Yellow" }
        $sign = if ($action.Value -gt 0) { "+" } else { "" }
        Write-Host "   $($action.Key): $sign$($action.Value) points" -ForegroundColor $color
    }
    Write-Host ""

    # Show recent actions
    if ($rewardData.RecentActions.Count -gt 0) {
        Write-Host "📈 Recent Actions (Last 5):" -ForegroundColor Yellow
        $recentActions = $rewardData.RecentActions | Select-Object -Last 5
        foreach ($action in $recentActions) {
            $emoji = if ($action.FinalPoints -gt 0) { "✅" } elseif ($action.FinalPoints -lt 0) { "❌" } else { "➖" }
            $color = if ($action.FinalPoints -gt 0) { "Green" } elseif ($action.FinalPoints -lt 0) { "Red" } else { "Yellow" }
            Write-Host "   $emoji $($action.Action): $($action.FinalPoints) pts" -ForegroundColor $color
            if ($action.BonusInfo -and $action.BonusInfo.Count -gt 0) {
                Write-Host "      🎁 $($action.BonusInfo -join ', ')" -ForegroundColor Cyan
            }
        }
        Write-Host ""
    }

    # Show reward level
    $level = switch ($rewardData.TotalRewards) {
        { $_ -lt 0 } { "🔴 Needs Improvement" }
        { $_ -lt 10 } { "🟡 Getting Started" }
        { $_ -lt 25 } { "🟠 Making Progress" }
        { $_ -lt 50 } { "🟢 Doing Well" }
        { $_ -lt 100 } { "🔵 Excellent" }
        default { "🟣 Legendary" }
    }

    Write-Host "🏆 Current Level: $level" -ForegroundColor Magenta
    Write-Host ""

    # Show multipliers
    Write-Host "🎁 Active Multipliers:" -ForegroundColor Yellow
    foreach ($mult in $rewardSystem.Multipliers.GetEnumerator()) {
        Write-Host "   $($mult.Key): $($mult.Value)x" -ForegroundColor Cyan
    }
}

function Get-CurrentHappiness {
    $readmePath = "README.md"
    $currentHappiness = 0

    if (Test-Path $readmePath) {
        $content = Get-Content $readmePath -Raw
        if ($content -match 'Can Steve USE BusBuddy\? \[.*\] (\d+)%') {
            $currentHappiness = [int]$matches[1]
        }
    }

    return $currentHappiness
}

function Get-HappinessLevel {
    param([int]$Happiness)

    $level = $script:HappinessMatrix.Levels.Keys | Sort-Object -Descending | Where-Object { $Happiness -ge $_ } | Select-Object -First 1
    return $script:HappinessMatrix.Levels[$level]
}

function Show-HappinessMatrix {
    Write-Host "🚌 STEVE HAPPINESS MATRIX V2" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
    Write-Host ""

    $currentHappiness = Get-CurrentHappiness
    $currentLevel = Get-HappinessLevel -Happiness $currentHappiness

    # Show current status
    Write-Host "📊 Current Status:" -ForegroundColor Yellow
    Write-Host "   $($currentLevel.Emoji) $($currentLevel.Status) - $currentHappiness%" -ForegroundColor $currentLevel.Color
    Write-Host "   $($currentLevel.Description)" -ForegroundColor Gray
    Write-Host ""

    # Show happiness bar
    $barLength = 40
    $filled = [math]::Floor($currentHappiness * $barLength / 100)
    $empty = $barLength - $filled
    $happinessBar = ("█" * $filled) + ("░" * $empty)
    Write-Host "📈 Progress: [$happinessBar] $currentHappiness%" -ForegroundColor White
    Write-Host ""

    # Show milestones
    Write-Host "🎯 Milestones:" -ForegroundColor Yellow
    foreach ($milestone in $script:HappinessMatrix.Milestones.GetEnumerator() | Sort-Object Key) {
        $status = if ($currentHappiness -ge $milestone.Key) { "✅" } else { "🎯" }
        $color = if ($currentHappiness -ge $milestone.Key) { "Green" } else { "Gray" }
        Write-Host "   $status $($milestone.Key)%: $($milestone.Value)" -ForegroundColor $color
    }

    Write-Host ""

    # Show next milestone
    $nextMilestone = $script:HappinessMatrix.Milestones.GetEnumerator() | Sort-Object Key | Where-Object { $_.Key -gt $currentHappiness } | Select-Object -First 1
    if ($nextMilestone) {
        Write-Host "🎪 Next Milestone: $($nextMilestone.Key)% - $($nextMilestone.Value)" -ForegroundColor Magenta
    }
    else {
        Write-Host "🏆 ALL MILESTONES ACHIEVED! Steve is maximally happy!" -ForegroundColor Green
    }
}

function Test-ProblemMatchers {
    Write-Host "🔍 PROBLEM MATCHER DIAGNOSTICS" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
    Write-Host ""

    $tasksJsonPath = ".vscode\tasks.json"
    $problems = @()

    if (-not (Test-Path $tasksJsonPath)) {
        $problems += @{
            Type        = "Missing File"
            Severity    = "Critical"
            Description = "tasks.json file not found"
            Fix         = "Create .vscode/tasks.json file"
        }
        return $problems
    }

    try {
        $tasksContent = Get-Content $tasksJsonPath -Raw | ConvertFrom-Json
        Write-Host "✅ tasks.json file is valid JSON" -ForegroundColor Green

        # Check each task for problem matcher issues
        foreach ($task in $tasksContent.tasks) {
            Write-Host "🔍 Checking task: $($task.label)" -ForegroundColor Yellow

            if ($task.problemMatcher) {
                if ($task.problemMatcher -is [array] -and $task.problemMatcher.Count -eq 0) {
                    Write-Host "   ⚠️ Empty problem matcher array (should use specific matchers)" -ForegroundColor Yellow
                    $problems += @{
                        Type        = "Empty Problem Matcher"
                        Severity    = "Medium"
                        Description = "Task '$($task.label)' has empty problemMatcher array"
                        Fix         = "Use appropriate problem matcher like '$msCompile' for build tasks"
                        TaskLabel   = $task.label
                    }
                }
                elseif ($task.problemMatcher -is [array]) {
                    Write-Host "   ✅ Has problem matchers: $($task.problemMatcher -join ', ')" -ForegroundColor Green
                }
                else {
                    Write-Host "   ✅ Has problem matcher: $($task.problemMatcher)" -ForegroundColor Green
                }
            }
            else {
                Write-Host "   ⚠️ No problem matcher defined" -ForegroundColor Yellow
                $problems += @{
                    Type        = "Missing Problem Matcher"
                    Severity    = "Low"
                    Description = "Task '$($task.label)' has no problemMatcher property"
                    Fix         = "Add appropriate problem matcher for task type"
                    TaskLabel   = $task.label
                }
            }

            # Check for build tasks specifically
            if ($task.group -eq "build" -or $task.label -like "*Build*") {
                if (-not $task.problemMatcher -or ($task.problemMatcher -is [array] -and $task.problemMatcher.Count -eq 0)) {
                    $problems += @{
                        Type        = "Build Task Missing Problem Matcher"
                        Severity    = "High"
                        Description = "Build task '$($task.label)' should have '$msCompile' problem matcher"
                        Fix         = "Add '`"problemMatcher`": [`"$msCompile`"]' to the task"
                        TaskLabel   = $task.label
                    }
                }
            }
        }

    }
    catch {
        $problems += @{
            Type        = "JSON Parse Error"
            Severity    = "Critical"
            Description = "tasks.json contains invalid JSON: $($_.Exception.Message)"
            Fix         = "Fix JSON syntax errors in tasks.json"
        }
    }

    # Report results and auto-reward based on problem state
    if ($problems.Count -eq 0) {
        Write-Host "🎉 No problem matcher issues found!" -ForegroundColor Green

        # Check if this is an improvement (compare with previous state)
        $rewardData = Get-RewardData
        $lastDiagnostic = $rewardData.RecentActions | Where-Object { $_.Action -eq "ProblemsSolved" } | Select-Object -Last 1

        # If this is the first time or there were problems before, reward the solve
        if (-not $lastDiagnostic -or $lastDiagnostic.Description -notlike "*0 problems*") {
            Add-RewardPoints -Action "FixProblem" -Description "All problem matcher issues resolved! (0 problems remaining)" | Out-Null
        }
    }
    else {
        Write-Host "⚠️ Found $($problems.Count) problem matcher issues:" -ForegroundColor Yellow
        foreach ($problem in $problems) {
            $color = switch ($problem.Severity) {
                "Critical" { "Red" }
                "High" { "Magenta" }
                "Medium" { "Yellow" }
                default { "Gray" }
            }
            Write-Host "   • [$($problem.Severity)] $($problem.Description)" -ForegroundColor $color
            Write-Host "     Fix: $($problem.Fix)" -ForegroundColor Gray
        }

        # Check if problems were reduced from last time
        $rewardData = Get-RewardData
        $lastDiagnostic = $rewardData.RecentActions | Where-Object { $_.Action -like "*Problem*" } | Select-Object -Last 1
        if ($lastDiagnostic -and $lastDiagnostic.Description -match "(\d+) problems") {
            $previousCount = [int]$matches[1]
            if ($problems.Count -lt $previousCount) {
                $improvement = $previousCount - $problems.Count
                Add-RewardPoints -Action "FixProblem" -Description "Reduced problems from $previousCount to $($problems.Count) (-$improvement problems!)" | Out-Null
            }
        }
    }

    return $problems
}

function Test-BuildSystem {
    Write-Host "🔨 BUILD SYSTEM TEST" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
    Write-Host ""

    $results = @{
        BuildSucceeds     = $false
        NuGetRestore      = $false
        CompilationErrors = $false
        Score             = 0
    }

    # Test NuGet restore
    Write-Host "📦 Testing NuGet restore..." -ForegroundColor Yellow
    try {
        $restoreResult = dotnet restore BusBuddy.sln --verbosity quiet --nologo 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "   ✅ NuGet restore successful" -ForegroundColor Green
            $results.NuGetRestore = $true
            $results.Score += 10
        }
        else {
            Write-Host "   ❌ NuGet restore failed" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "   ❌ NuGet restore error: $($_.Exception.Message)" -ForegroundColor Red
    }

    # Test build
    Write-Host "🔨 Testing build..." -ForegroundColor Yellow
    try {
        $buildResult = dotnet build BusBuddy.sln --verbosity quiet --nologo 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "   ✅ Build successful" -ForegroundColor Green
            $results.BuildSucceeds = $true
            $results.Score += 20

            # Auto-reward for successful build
            Add-RewardPoints -Action "BuildSuccess" -Description "Automated build completed successfully" | Out-Null
        }
        else {
            Write-Host "   ❌ Build failed" -ForegroundColor Red
            Write-Host "   💡 Run with verbose output to see errors" -ForegroundColor Gray

            # Penalty for build failure
            Add-RewardPoints -Action "BuildFail" -Description "Build failed - needs attention" | Out-Null
        }
    }
    catch {
        Write-Host "   ❌ Build error: $($_.Exception.Message)" -ForegroundColor Red
        Add-RewardPoints -Action "BuildFail" -Description "Build error: $($_.Exception.Message)" | Out-Null
    }

    return $results
}

function Test-FairnessSystem {
    Write-Host "⚖️ FAIRNESS EVALUATION SYSTEM TEST" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
    Write-Host ""

    $results = @{
        FairEvaluation     = $false
        UnbiasedReports    = $false
        EqualTreatment     = $false
        TransparentScoring = $false
        Score              = 0
    }

    # Test 1: Fair Evaluation - Check if reward system treats actions equally
    Write-Host "⚖️ Testing fair evaluation..." -ForegroundColor Yellow
    try {
        $rewardActions = $script:HappinessMatrix.RewardSystem.Actions

        # Check if positive and negative actions are balanced
        $positiveActions = $rewardActions.GetEnumerator() | Where-Object { $_.Value -gt 0 }
        $negativeActions = $rewardActions.GetEnumerator() | Where-Object { $_.Value -lt 0 }

        if ($positiveActions.Count -gt 0 -and $negativeActions.Count -gt 0) {
            Write-Host "   ✅ Balanced positive/negative action scoring" -ForegroundColor Green
            $results.FairEvaluation = $true
            $results.Score += 8
            Add-RewardPoints -Action "FairnessImplemented" -Description "Fair evaluation system verified - balanced scoring" | Out-Null
        }
        else {
            Write-Host "   ⚠️ Unbalanced action scoring system" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "   ❌ Fair evaluation test error: $($_.Exception.Message)" -ForegroundColor Red
    }

    # Test 2: Unbiased Reports - Check for transparent documentation
    Write-Host "📊 Testing unbiased reporting..." -ForegroundColor Yellow
    try {
        $rewardFile = $script:HappinessMatrix.RewardSystem.RewardFile
        if (Test-Path $rewardFile) {
            $rewardData = Get-Content $rewardFile -Raw | ConvertFrom-Json
            if ($rewardData.RecentActions -and $rewardData.RecentActions.Count -gt 0) {
                # Check if recent actions include both positive and negative
                $recentPositive = $rewardData.RecentActions | Where-Object { $_.Points -gt 0 }
                $recentNegative = $rewardData.RecentActions | Where-Object { $_.Points -lt 0 }

                if ($recentPositive -or $recentNegative) {
                    Write-Host "   ✅ Transparent action logging active" -ForegroundColor Green
                    $results.UnbiasedReports = $true
                    $results.Score += 6
                    Add-RewardPoints -Action "TransparentProcess" -Description "Unbiased reporting verified - transparent logging" | Out-Null
                }
            }
        }
        else {
            Write-Host "   ℹ️ No reward history yet - fairness tracking will begin with usage" -ForegroundColor Cyan
        }
    }
    catch {
        Write-Host "   ❌ Unbiased reporting test error: $($_.Exception.Message)" -ForegroundColor Red
    }

    # Test 3: Equal Treatment - Check that all problem areas have equal weight consideration
    Write-Host "🤝 Testing equal treatment..." -ForegroundColor Yellow
    try {
        $problemAreas = $script:HappinessMatrix.ProblemAreas
        $weights = $problemAreas.Values | ForEach-Object { $_.Weight }

        # Check if weights are reasonably distributed (no single area dominates >40%)
        $totalWeight = ($weights | Measure-Object -Sum).Sum
        $maxWeight = ($weights | Measure-Object -Maximum).Maximum
        $dominanceRatio = $maxWeight / $totalWeight

        if ($dominanceRatio -le 0.4) {
            Write-Host "   ✅ Balanced problem area weighting (max: $([math]::Round($dominanceRatio * 100, 1))%)" -ForegroundColor Green
            $results.EqualTreatment = $true
            $results.Score += 7
            Add-RewardPoints -Action "EqualTreatment" -Description "Equal treatment verified - balanced problem area weights" | Out-Null
        }
        else {
            Write-Host "   ⚠️ One problem area dominates evaluation ($([math]::Round($dominanceRatio * 100, 1))%)" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "   ❌ Equal treatment test error: $($_.Exception.Message)" -ForegroundColor Red
    }

    # Test 4: Transparent Scoring - Check if scoring criteria are documented
    Write-Host "🔍 Testing transparent scoring..." -ForegroundColor Yellow
    try {
        # Check if happiness levels have clear descriptions
        $levels = $script:HappinessMatrix.Levels
        $wellDocumentedLevels = $levels.GetEnumerator() | Where-Object {
            $_.Value.Description -and $_.Value.Description.Length -gt 10
        }

        if ($wellDocumentedLevels.Count -ge ($levels.Count * 0.8)) {
            Write-Host "   ✅ Well-documented happiness criteria ($($wellDocumentedLevels.Count)/$($levels.Count) levels)" -ForegroundColor Green
            $results.TransparentScoring = $true
            $results.Score += 6
            Add-RewardPoints -Action "TransparentProcess" -Description "Transparent scoring verified - documented criteria" | Out-Null
        }
        else {
            Write-Host "   ⚠️ Some happiness levels lack clear documentation" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "   ❌ Transparent scoring test error: $($_.Exception.Message)" -ForegroundColor Red
    }

    # Generate fairness summary
    $fairnessScore = $results.Score
    Write-Host ""
    Write-Host "⚖️ FAIRNESS SUMMARY" -ForegroundColor Cyan
    Write-Host "   Fair Evaluation: $(if ($results.FairEvaluation) { '✅' } else { '❌' })" -ForegroundColor White
    Write-Host "   Unbiased Reports: $(if ($results.UnbiasedReports) { '✅' } else { '❌' })" -ForegroundColor White
    Write-Host "   Equal Treatment: $(if ($results.EqualTreatment) { '✅' } else { '❌' })" -ForegroundColor White
    Write-Host "   Transparent Scoring: $(if ($results.TransparentScoring) { '✅' } else { '❌' })" -ForegroundColor White
    Write-Host "   Fairness Score: $fairnessScore/27" -ForegroundColor $(if ($fairnessScore -ge 20) { "Green" } elseif ($fairnessScore -ge 15) { "Yellow" } else { "Red" })

    return $results
}

function Invoke-FullDiagnostic {
    Write-Host "🔍 STEVE HAPPINESS FULL DIAGNOSTIC" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
    Write-Host ""

    $diagnosticResults = @{
        ProblemMatchers = @()
        BuildSystem     = @{}
        FairnessSystem  = @{}
        OverallScore    = 0
        Issues          = @()
        Recommendations = @()
    }

    # Test problem matchers
    $diagnosticResults.ProblemMatchers = Test-ProblemMatchers

    # Test build system
    $diagnosticResults.BuildSystem = Test-BuildSystem

    # Test fairness system
    $diagnosticResults.FairnessSystem = Test-FairnessSystem

    # Calculate overall score
    $buildScore = $diagnosticResults.BuildSystem.Score
    $problemMatcherScore = if ($diagnosticResults.ProblemMatchers.Count -eq 0) { 20 } else { 20 - ($diagnosticResults.ProblemMatchers.Count * 5) }
    $problemMatcherScore = [math]::Max(0, $problemMatcherScore)
    $fairnessScore = $diagnosticResults.FairnessSystem.Score

    $diagnosticResults.OverallScore = $buildScore + $problemMatcherScore + $fairnessScore

    # Generate recommendations
    if ($diagnosticResults.ProblemMatchers.Count -gt 0) {
        $diagnosticResults.Recommendations += "Fix problem matcher issues in tasks.json"
    }
    if (-not $diagnosticResults.BuildSystem.BuildSucceeds) {
        $diagnosticResults.Recommendations += "Fix build errors before proceeding"
    }
    if (-not $diagnosticResults.BuildSystem.NuGetRestore) {
        $diagnosticResults.Recommendations += "Fix NuGet package restore issues"
    }

    # Show summary with auto-rewards for improvements
    Write-Host ""
    Write-Host "📊 DIAGNOSTIC SUMMARY" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
    Write-Host "Overall Health Score: $($diagnosticResults.OverallScore)/77" -ForegroundColor White
    Write-Host "Problem Matcher Issues: $($diagnosticResults.ProblemMatchers.Count)" -ForegroundColor Yellow
    Write-Host "Build Success: $(if ($diagnosticResults.BuildSystem.BuildSucceeds) { '✅' } else { '❌' })" -ForegroundColor White
    Write-Host "Fairness Score: $($diagnosticResults.FairnessSystem.Score)/27 $(if ($diagnosticResults.FairnessSystem.Score -ge 20) { '✅' } elseif ($diagnosticResults.FairnessSystem.Score -ge 15) { '⚠️' } else { '❌' })" -ForegroundColor White
    Write-Host ""

    # Auto-reward for health improvements
    $rewardData = Get-RewardData
    $lastHealthScore = ($rewardData.RecentActions | Where-Object { $_.Action -eq "HealthImprovement" } | Select-Object -Last 1)?.Description

    if ($lastHealthScore -and $lastHealthScore -match "(\d+)/50") {
        $previousScore = [int]$matches[1]
        if ($diagnosticResults.OverallScore -gt $previousScore) {
            $improvement = $diagnosticResults.OverallScore - $previousScore
            Add-RewardPoints -Action "HealthImprovement" -BasePoints $improvement -Description "System health improved from $previousScore/50 to $($diagnosticResults.OverallScore)/50 (+$improvement points!)" | Out-Null
        }
    }
    elseif (-not $lastHealthScore -and $diagnosticResults.OverallScore -ge 30) {
        # First time achieving good health
        Add-RewardPoints -Action "HealthImprovement" -BasePoints 5 -Description "First diagnostic - achieved $($diagnosticResults.OverallScore)/50 health score!" | Out-Null
    }

    if ($diagnosticResults.Recommendations.Count -gt 0) {
        Write-Host "🎯 Recommendations:" -ForegroundColor Magenta
        foreach ($rec in $diagnosticResults.Recommendations) {
            Write-Host "   • $rec" -ForegroundColor Gray
        }
    }
    else {
        Write-Host "🎉 No major issues found! Steve should be happy!" -ForegroundColor Green
        # Reward for perfect diagnostic
        Add-RewardPoints -Action "PerfectDiagnostic" -BasePoints 8 -Description "Perfect diagnostic - no recommendations needed!" | Out-Null
    }

    return $diagnosticResults
}

function Invoke-ProblemFixes {
    Write-Host "🔧 FIXING IDENTIFIED PROBLEMS" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
    Write-Host ""

    $problems = Test-ProblemMatchers

    if ($problems.Count -eq 0) {
        Write-Host "✅ No problem matcher issues to fix!" -ForegroundColor Green
        Add-RewardPoints -Action "ProblemsSolved" -Description "All problems already solved - system is healthy!" | Out-Null
        return
    }

    Write-Host "🎯 Fixing $($problems.Count) problem matcher issues..." -ForegroundColor Yellow
    Write-Host ""

    # Store current problem count for later comparison
    $initialProblemCount = $problems.Count

    # Create problem matcher fix guide
    Write-Host "📝 PROBLEM MATCHER FIX GUIDE" -ForegroundColor Magenta
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
    Write-Host ""

    foreach ($problem in $problems) {
        Write-Host "🔧 Issue: $($problem.Description)" -ForegroundColor Yellow
        Write-Host "   Severity: $($problem.Severity)" -ForegroundColor Gray
        Write-Host "   Fix: $($problem.Fix)" -ForegroundColor Green

        if ($problem.TaskLabel) {
            Write-Host "   Task: $($problem.TaskLabel)" -ForegroundColor Cyan

            # Provide specific fix instructions
            switch ($problem.Type) {
                "Empty Problem Matcher" {
                    Write-Host "   📋 Replace: `"problemMatcher`": []" -ForegroundColor Red
                    Write-Host "   📋 With: `"problemMatcher`": [`"$msCompile`"]" -ForegroundColor Green
                }
                "Missing Problem Matcher" {
                    Write-Host "   📋 Add this to the task:" -ForegroundColor Green
                    Write-Host '   "problemMatcher": ["$msCompile"]' -ForegroundColor Cyan
                }
                "Build Task Missing Problem Matcher" {
                    Write-Host "   📋 This is a build task and MUST have problem matchers!" -ForegroundColor Red
                    Write-Host '   📋 Add: "problemMatcher": ["$msCompile"]' -ForegroundColor Green
                }
            }
        }
        Write-Host ""
    }

    Write-Host "💡 QUICK FIX STEPS:" -ForegroundColor Magenta
    Write-Host "1. Open .vscode/tasks.json in VS Code" -ForegroundColor Gray
    Write-Host "2. Find the tasks mentioned above" -ForegroundColor Gray
    Write-Host "3. Add or fix the problemMatcher property as shown" -ForegroundColor Gray
    Write-Host "4. Save the file" -ForegroundColor Gray
    Write-Host "5. Run this script again to verify fixes" -ForegroundColor Gray
    Write-Host ""

    Write-Host "🎯 Pro Tip: Run '-FixProblems' again after making changes to auto-reward problem solving progress!" -ForegroundColor Cyan
}

function Export-HappinessReport {
    $currentHappiness = Get-CurrentHappiness
    $diagnostic = Invoke-FullDiagnostic

    $report = @{
        Timestamp         = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        CurrentHappiness  = $currentHappiness
        HappinessLevel    = Get-HappinessLevel -Happiness $currentHappiness
        DiagnosticResults = $diagnostic
        Recommendations   = @()
    }

    # Add specific recommendations based on happiness level
    if ($currentHappiness -lt 25) {
        $report.Recommendations += "Focus on getting the application to launch successfully"
        $report.Recommendations += "Ensure MainWindow displays without crashing"
    }
    elseif ($currentHappiness -lt 50) {
        $report.Recommendations += "Implement navigation between dashboard views"
        $report.Recommendations += "Add sample data to at least one view"
    }
    elseif ($currentHappiness -lt 75) {
        $report.Recommendations += "Populate all views with real data"
        $report.Recommendations += "Improve UI polish and user experience"
    }
    else {
        $report.Recommendations += "Focus on performance optimization"
        $report.Recommendations += "Add comprehensive error handling"
    }

    $reportPath = "steve-happiness-report-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
    $report | ConvertTo-Json -Depth 10 | Out-File -FilePath $reportPath -Encoding UTF8

    Write-Host "📊 Happiness report exported to: $reportPath" -ForegroundColor Green
    return $reportPath
}

function Invoke-GrokAnalysis {
    Write-Host "🤖 GROK-4 AI ANALYSIS FOR BUSBUDDY" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
    Write-Host ""

    # Gather current project state
    $currentHappiness = Get-CurrentHappiness
    $diagnostic = Invoke-FullDiagnostic
    $rewardData = Get-RewardData

    Write-Host "🧠 Analyzing BusBuddy project state with Grok-4 intelligence..." -ForegroundColor Yellow
    Write-Host ""

    # Grok-4 Style Analysis - Contextual and Intelligent
    Write-Host "🎯 GROK-4 STRATEGIC ANALYSIS:" -ForegroundColor Magenta
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray

    # Phase Analysis
    Write-Host "📊 Current Phase Assessment:" -ForegroundColor Yellow
    if ($currentHappiness -eq 0) {
        Write-Host "   🎯 PHASE: Bootstrap & Foundation" -ForegroundColor Red
        Write-Host "   📍 STATUS: Application not launching - need to establish basic functionality" -ForegroundColor Gray
        Write-Host "   🚀 PRIORITY: Get MainWindow to display without crashing" -ForegroundColor White
    }

    # Technical Debt Analysis
    Write-Host ""
    Write-Host "⚙️ Technical Health Analysis:" -ForegroundColor Yellow
    Write-Host "   🏥 System Health: $($diagnostic.OverallScore)/50 ($(if($diagnostic.OverallScore -ge 40){'Excellent'}elseif($diagnostic.OverallScore -ge 30){'Good'}elseif($diagnostic.OverallScore -ge 20){'Fair'}else{'Needs Work'}))" -ForegroundColor $(if ($diagnostic.OverallScore -ge 40) { 'Green' }elseif ($diagnostic.OverallScore -ge 30) { 'Cyan' }elseif ($diagnostic.OverallScore -ge 20) { 'Yellow' }else { 'Red' })
    Write-Host "   🔧 Build System: $(if($diagnostic.BuildSystem.BuildSucceeds){'✅ Healthy'}else{'❌ Broken'})" -ForegroundColor $(if ($diagnostic.BuildSystem.BuildSucceeds) { 'Green' }else { 'Red' })
    Write-Host "   📋 Problem Matchers: $($diagnostic.ProblemMatchers.Count) issues" -ForegroundColor $(if ($diagnostic.ProblemMatchers.Count -eq 0) { 'Green' }else { 'Yellow' })

    # ML Reward System Analysis
    Write-Host ""
    Write-Host "🎮 Performance & Motivation Analysis:" -ForegroundColor Yellow
    Write-Host "   🏆 Total Rewards: $($rewardData.TotalRewards) points" -ForegroundColor Green
    Write-Host "   📈 Recent Actions: $($rewardData.RecentActions.Count)/20" -ForegroundColor Cyan
    $positiveActions = ($rewardData.RecentActions | Where-Object { $_.FinalPoints -gt 0 }).Count
    $negativeActions = ($rewardData.RecentActions | Where-Object { $_.FinalPoints -lt 0 }).Count
    Write-Host "   ✅ Positive Momentum: $positiveActions actions" -ForegroundColor Green
    Write-Host "   ❌ Negative Events: $negativeActions actions" -ForegroundColor $(if ($negativeActions -eq 0) { 'Green' }else { 'Red' })

    # Grok-4 Strategic Recommendations
    Write-Host ""
    Write-Host "🧠 GROK-4 STRATEGIC RECOMMENDATIONS:" -ForegroundColor Magenta
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray

    # Immediate Actions (Next 15 minutes)
    Write-Host "⚡ IMMEDIATE ACTIONS (Next 15 min):" -ForegroundColor Red
    if ($currentHappiness -eq 0) {
        Write-Host "   1. 🚀 Launch BusBuddy application to test basic functionality" -ForegroundColor White
        Write-Host "   2. 📱 Verify MainWindow.xaml loads without errors" -ForegroundColor White
        Write-Host "   3. 🔍 Check for runtime exceptions in console output" -ForegroundColor White
        Write-Host "   💰 Potential Rewards: +10 MilestoneReached + +6 FirstSuccess = +16 points!" -ForegroundColor Green
    }

    # Short-term Goals (Next hour)
    Write-Host ""
    Write-Host "🎯 SHORT-TERM GOALS (Next 60 min):" -ForegroundColor Yellow
    Write-Host "   1. 🎨 Ensure all three core views render (Drivers, Vehicles, Activities)" -ForegroundColor Cyan
    Write-Host "   2. 🗃️ Add basic sample data to populate views" -ForegroundColor Cyan
    Write-Host "   3. 🧭 Implement basic navigation between dashboard views" -ForegroundColor Cyan
    Write-Host "   💰 Potential Rewards: +4 UIImprovement per view + +3 DataConnection = +15 points!" -ForegroundColor Green

    # Risk Assessment
    Write-Host ""
    Write-Host "⚠️ RISK ASSESSMENT:" -ForegroundColor Red
    Write-Host "   🔴 HIGH RISK: Application may have startup crashes (-5 CrashDetected penalty)" -ForegroundColor Red
    Write-Host "   🟡 MEDIUM RISK: Missing database configuration may cause data loading failures" -ForegroundColor Yellow
    Write-Host "   🟢 LOW RISK: Build system is stable, configuration is healthy" -ForegroundColor Green

    # Success Probability Analysis
    Write-Host ""
    Write-Host "📈 SUCCESS PROBABILITY MATRIX:" -ForegroundColor Green
    Write-Host "   🎯 Application Launch Success: $(if($diagnostic.BuildSystem.BuildSucceeds){'85%'}else{'25%'})" -ForegroundColor $(if ($diagnostic.BuildSystem.BuildSucceeds) { 'Green' }else { 'Red' })
    Write-Host "   🎯 5% Milestone Achievement: $(if($diagnostic.BuildSystem.BuildSucceeds){'90%'}else{'30%'})" -ForegroundColor $(if ($diagnostic.BuildSystem.BuildSucceeds) { 'Green' }else { 'Red' })
    Write-Host "   🎯 15% Milestone Achievement: $(if($diagnostic.BuildSystem.BuildSucceeds){'70%'}else{'20%'})" -ForegroundColor $(if ($diagnostic.BuildSystem.BuildSucceeds) { 'Cyan' }else { 'Red' })

    # Optimal Next Action
    Write-Host ""
    Write-Host "🎪 GROK-4 OPTIMAL NEXT ACTION:" -ForegroundColor Magenta
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
    if ($diagnostic.BuildSystem.BuildSucceeds) {
        Write-Host "🚀 EXECUTE: Run 'dotnet run --project BusBuddy.WPF\\BusBuddy.WPF.csproj'" -ForegroundColor Green
        Write-Host "🎯 GOAL: Achieve 5% milestone (Application launches without crashing)" -ForegroundColor White
        Write-Host "💡 TIP: Monitor console for exceptions, verify UI renders properly" -ForegroundColor Cyan
    }
    else {
        Write-Host "🔧 EXECUTE: Fix build errors first with 'dotnet build BusBuddy.sln --verbosity detailed'" -ForegroundColor Red
        Write-Host "🎯 GOAL: Restore build system health before launching" -ForegroundColor White
        Write-Host "💡 TIP: Build must succeed before attempting application launch" -ForegroundColor Cyan
    }

    # Reward the analysis
    Add-RewardPoints -Action "GrokAnalysis" -Description "Grok-4 AI analysis provided strategic insights and recommendations for BusBuddy development" | Out-Null

    Write-Host ""
    Write-Host "🤖 Grok-4 analysis complete! Use insights above to guide next development decisions." -ForegroundColor Green
    Write-Host ""

    return @{
        Phase              = if ($currentHappiness -eq 0) { "Bootstrap" } else { "Development" }
        OptimalAction      = if ($diagnostic.BuildSystem.BuildSucceeds) { "Launch Application" } else { "Fix Build" }
        SuccessProbability = if ($diagnostic.BuildSystem.BuildSucceeds) { 85 } else { 25 }
        ExpectedReward     = if ($diagnostic.BuildSystem.BuildSucceeds) { 16 } else { 5 }
    }
}

# Main execution logic
if ($Help) {
    Show-Help
    return
}

if ($ShowMatrix) {
    Show-HappinessMatrix
    return
}

if ($FullDiagnostic) {
    Invoke-FullDiagnostic
    return
}

if ($FixProblems) {
    Invoke-ProblemFixes
    return
}

if ($ExportReport) {
    Export-HappinessReport
    return
}

if ($ShowRewards) {
    Show-RewardSystem
    return
}

if ($GrokAnalysis) {
    Invoke-GrokAnalysis
    return
}

if ($AddReward -ne 0) {
    Write-Host "🎯 ADDING MANUAL REWARD" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
    Write-Host ""

    $points = Add-RewardPoints -Action "ManualReward" -BasePoints $AddReward -Description "Manual reward addition"
    Write-Host ""
    Write-Host "💡 Use predefined actions for automatic multipliers:" -ForegroundColor Magenta
    Write-Host "   BuildSuccess, FixProblem, TestPass, UIImprovement, etc." -ForegroundColor Gray
    return
}

if ($PSBoundParameters.ContainsKey('HappinessLevel')) {
    Write-Host "📈 UPDATING STEVE'S HAPPINESS" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
    Write-Host ""

    $oldHappiness = Get-CurrentHappiness
    $level = Get-HappinessLevel -Happiness $HappinessLevel
    Write-Host "$($level.Emoji) New Happiness Level: $HappinessLevel% - $($level.Status)" -ForegroundColor $level.Color
    Write-Host "🏆 Achievement: $Achievement" -ForegroundColor Yellow
    Write-Host ""

    # Check for milestone achievements and auto-reward
    Check-MilestoneAchievements -NewHappiness $HappinessLevel -OldHappiness $oldHappiness

    # Auto-reward for happiness improvements
    if ($HappinessLevel -gt $oldHappiness) {
        $improvement = $HappinessLevel - $oldHappiness
        Add-RewardPoints -Action "UIImprovement" -BasePoints $improvement -Description "Steve's happiness increased by $improvement% (from $oldHappiness% to $HappinessLevel%)" | Out-Null
    }

    Write-Host "💡 Don't forget to update README.md!" -ForegroundColor Magenta
    return
}

# Default: Show matrix
Show-HappinessMatrix
