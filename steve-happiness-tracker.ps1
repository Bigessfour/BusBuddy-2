# Steve Happiness Tracker for BusBuddy
# Quick tool to track progress toward making Steve happy with a working UI

param(
    [Parameter(Mandatory = $false)]
    [ValidateRange(0, 100)]
    [int]$HappinessLevel,

    [Parameter(Mandatory = $false)]
    [string]$Achievement = "",

    [Parameter(Mandatory = $false)]
    [switch]$TestUI,

    [Parameter(Mandatory = $false)]
    [switch]$ShowStatus,

    [Parameter(Mandatory = $false)]
    [switch]$Help
)

function Show-Help {
    Write-Host "😊 Steve Happiness Tracker for BusBuddy" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "📊 Show current Steve happiness status:" -ForegroundColor Yellow
    Write-Host "  .\steve-happiness-tracker.ps1 -ShowStatus" -ForegroundColor Green
    Write-Host ""
    Write-Host "🎯 Update Steve's happiness level:" -ForegroundColor Yellow
    Write-Host "  .\steve-happiness-tracker.ps1 -HappinessLevel 30 -Achievement 'Got MainWindow working!'" -ForegroundColor Green
    Write-Host ""
    Write-Host "🧪 Test UI functionality:" -ForegroundColor Yellow
    Write-Host "  .\steve-happiness-tracker.ps1 -TestUI" -ForegroundColor Green
}

function Show-SteveStatus {
    Write-Host "😊 STEVE HAPPINESS STATUS REPORT" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
    Write-Host ""

    # Read current happiness from README if available
    $readmePath = "README.md"
    $currentHappiness = 0

    if (Test-Path $readmePath) {
        $content = Get-Content $readmePath -Raw
        if ($content -match 'Can Steve USE BusBuddy\? \[.*\] (\d+)%') {
            $currentHappiness = [int]$matches[1]
        }
    }

    # Generate happiness bar
    $barLength = 32
    $filled = [math]::Floor($currentHappiness * $barLength / 100)
    $empty = $barLength - $filled
    $happinessBar = ("█" * $filled) + ("░" * $empty)

    Write-Host "🚌 Can Steve USE BusBuddy?: [$happinessBar] $currentHappiness%" -ForegroundColor White
    Write-Host ""

    # Determine Steve's mood
    $mood = switch ($currentHappiness) {
        { $_ -eq 0 } { "😐 Waiting patiently... (Foundation looks good though!)" }
        { $_ -le 25 } { "🙂 Getting excited! (Something is working!)" }
        { $_ -le 50 } { "😊 Happy! (UI is coming together!)" }
        { $_ -le 75 } { "😃 Very Happy! (This is looking great!)" }
        { $_ -lt 100 } { "🤩 Thrilled! (Almost perfect!)" }
        default { "🥳 ECSTATIC! (BusBuddy is AMAZING!)" }
    }

    Write-Host "😊 Steve's Current Mood: $mood" -ForegroundColor Yellow
    Write-Host ""

    # Show next milestone
    $nextMilestone = switch ($currentHappiness) {
        { $_ -lt 10 } { "🎯 Next Goal: Get the application to launch and show MainWindow" }
        { $_ -lt 25 } { "🎯 Next Goal: Navigate from MainWindow to Dashboard" }
        { $_ -lt 50 } { "🎯 Next Goal: Show all 3 core views (Drivers, Vehicles, Activities)" }
        { $_ -lt 75 } { "🎯 Next Goal: Display sample data in the views" }
        { $_ -lt 100 } { "🎯 Next Goal: Perfect the UI polish and performance" }
        default { "🏆 Goal: Steve is FULLY HAPPY! Mission accomplished!" }
    }

    Write-Host $nextMilestone -ForegroundColor Green
    Write-Host ""

    # Show quick wins
    if ($currentHappiness -lt 50) {
        Write-Host "💡 Quick Wins to Boost Steve's Happiness:" -ForegroundColor Magenta
        Write-Host "   • Get MainWindow to display properly" -ForegroundColor Gray
        Write-Host "   • Add navigation between views" -ForegroundColor Gray
        Write-Host "   • Show ANY data in ANY view" -ForegroundColor Gray
        Write-Host "   • Fix any UI crashes or errors" -ForegroundColor Gray
        Write-Host ""
    }

    Write-Host "📈 Steve Happiness History:" -ForegroundColor Yellow
    Write-Host "   • Foundation Building: 😐 → 😊 (Build system working)" -ForegroundColor Gray
    Write-Host "   • UI Development: 😊 → 😃 (Views working)" -ForegroundColor Gray
    Write-Host "   • Data Integration: 😃 → 🤩 (Data flowing)" -ForegroundColor Gray
    Write-Host "   • Final Polish: 🤩 → 🥳 (Production ready)" -ForegroundColor Gray
}

function Test-UIFunctionality {
    Write-Host "🧪 TESTING UI FUNCTIONALITY FOR STEVE" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
    Write-Host ""

    # Test 1: Can we build?
    Write-Host "🔨 Test 1: Build System" -ForegroundColor Yellow
    $buildTime = Measure-Command {
        $result = dotnet build BusBuddy.sln --verbosity quiet --nologo 2>&1
        $buildSuccess = $LASTEXITCODE -eq 0
    }

    if ($buildSuccess) {
        Write-Host "   ✅ Build successful in $($buildTime.TotalSeconds.ToString('F2'))s" -ForegroundColor Green
        $stevePoints = 10
    }
    else {
        Write-Host "   ❌ Build failed - Steve can't even start the app!" -ForegroundColor Red
        Write-Host "   💡 Fix the build first to make Steve happy" -ForegroundColor Yellow
        return
    }

    # Test 2: Can we launch the app?
    Write-Host ""
    Write-Host "🚀 Test 2: Application Launch" -ForegroundColor Yellow
    Write-Host "   💡 Manual test needed - run: dotnet run --project BusBuddy.WPF\\BusBuddy.WPF.csproj" -ForegroundColor Gray
    Write-Host "   📋 Check if MainWindow appears without crashing" -ForegroundColor Gray

    # Test 3: Project structure
    Write-Host ""
    Write-Host "🗂️ Test 3: Project Structure for Steve's Happiness" -ForegroundColor Yellow

    $coreViews = @(
        "BusBuddy.WPF\Views\Dashboard",
        "BusBuddy.WPF\Views\Driver",
        "BusBuddy.WPF\Views\Vehicle",
        "BusBuddy.WPF\Views\Activity"
    )

    $viewsFound = 0
    foreach ($view in $coreViews) {
        if (Test-Path $view) {
            Write-Host "   ✅ $view folder exists" -ForegroundColor Green
            $viewsFound++
        }
        else {
            Write-Host "   ⚠️ $view folder missing" -ForegroundColor Yellow
        }
    }

    $stevePoints += ($viewsFound * 5)

    Write-Host ""
    Write-Host "📊 STEVE HAPPINESS ASSESSMENT:" -ForegroundColor Cyan
    Write-Host "   Current estimated happiness: $stevePoints% of 30% (Phase 1 goal)" -ForegroundColor White

    if ($stevePoints -ge 25) {
        Write-Host "   😊 Steve Status: Getting Happy!" -ForegroundColor Green
    }
    elseif ($stevePoints -ge 15) {
        Write-Host "   🙂 Steve Status: Cautiously Optimistic" -ForegroundColor Yellow
    }
    else {
        Write-Host "   😐 Steve Status: Still Waiting..." -ForegroundColor Gray
    }

    Write-Host ""
    Write-Host "🎯 Next Steps to Make Steve Happier:" -ForegroundColor Magenta
    Write-Host "   1. Run the app and make sure MainWindow shows up" -ForegroundColor Gray
    Write-Host "   2. Click around - can you navigate between views?" -ForegroundColor Gray
    Write-Host "   3. Look for any crashes or errors that would frustrate Steve" -ForegroundColor Gray
    Write-Host "   4. Update happiness level: .\steve-happiness-tracker.ps1 -HappinessLevel XX" -ForegroundColor Gray
}

function Update-SteveHappiness {
    param([int]$Level, [string]$Achievement)

    Write-Host "📈 UPDATING STEVE'S HAPPINESS LEVEL" -ForegroundColor Cyan
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "🎯 New Happiness Level: $Level%" -ForegroundColor Green
    Write-Host "🏆 Achievement: $Achievement" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "💡 Don't forget to update the README.md Steve Happiness Dashboard!" -ForegroundColor Magenta
    Write-Host "   Look for the '😊 Steve Happiness Dashboard' section" -ForegroundColor Gray
    Write-Host ""

    # Determine celebration level
    if ($Level -ge 30) {
        Write-Host "🎉 MILESTONE REACHED! Steve is getting genuinely happy!" -ForegroundColor Green
    }
    elseif ($Level -ge 15) {
        Write-Host "🎊 Great progress! Steve is starting to smile!" -ForegroundColor Yellow
    }
    else {
        Write-Host "🚀 Keep going! Every step makes Steve a little happier!" -ForegroundColor Cyan
    }
}

# Main execution
if ($Help) {
    Show-Help
    return
}

if ($TestUI) {
    Test-UIFunctionality
    return
}

if ($PSBoundParameters.ContainsKey('HappinessLevel')) {
    Update-SteveHappiness -Level $HappinessLevel -Achievement $Achievement
    return
}

# Default: show status
Show-SteveStatus
