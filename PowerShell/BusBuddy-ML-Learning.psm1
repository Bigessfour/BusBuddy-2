#Requires -Version 7.5
<#
.SYNOPSIS
    BusBuddy Machine Learning & AI Learning Module

.DESCRIPTION
    A PowerShell module that helps developers learn about ML concepts specifically
    in the context of transportation systems and bus fleet management. Uses the
    BusBuddy Grok-4 integration for interactive learning.

.NOTES
    Features:
    - Interactive ML concept explanations
    - Transportation-specific use cases
    - Hands-on examples with BusBuddy data
    - Integration with Grok-4 AI for detailed explanations
    - Progressive learning paths
    - Practical exercises
#>

# ML Learning Module Configuration
$script:MLConfig = @{
    LearningSessions   = @{}
    CurrentLevel       = 'Beginner'
    ProgressTracking   = @{}
    InteractiveMode    = $true
    UseGrokIntegration = $true
}

# ML Concepts specifically for Transportation/Bus Systems
$script:TransportationMLConcepts = @{
    'Beginner'     = @{
        'Data Collection'     = @{
            Description     = 'Understanding what data we collect from buses and routes'
            Examples        = @(
                'GPS coordinates and timestamps',
                'Fuel consumption readings',
                'Engine temperature and diagnostics',
                'Student ridership patterns',
                'Driver behavior metrics'
            )
            BusBuddyContext = 'How BusBuddy collects data from vehicles, drivers, and routes'
            Exercise        = 'Analyze sample route data to identify patterns'
        }

        'Pattern Recognition' = @{
            Description     = 'Finding recurring patterns in transportation data'
            Examples        = @(
                'Peak ridership times',
                'Optimal route timing',
                'Maintenance needs prediction',
                'Traffic pattern analysis'
            )
            BusBuddyContext = 'Using BusBuddy data to identify operational patterns'
            Exercise        = 'Identify patterns in provided bus schedule data'
        }

        'Predictive Modeling' = @{
            Description     = 'Using historical data to predict future events'
            Examples        = @(
                'When a bus will need maintenance',
                'Expected fuel consumption for routes',
                'Optimal scheduling for efficiency',
                'Student pickup time predictions'
            )
            BusBuddyContext = 'How BusBuddy can predict operational needs'
            Exercise        = 'Create a simple maintenance prediction model'
        }
    }

    'Intermediate' = @{
        'Route Optimization'  = @{
            Description     = 'Using ML algorithms to optimize bus routes'
            Examples        = @(
                'Traveling Salesman Problem for routes',
                'Genetic algorithms for scheduling',
                'A* pathfinding for navigation',
                'Multi-objective optimization'
            )
            BusBuddyContext = 'Advanced route planning features in BusBuddy'
            Exercise        = 'Optimize a sample route using basic algorithms'
        }

        'Real-time Analytics' = @{
            Description     = 'Processing streaming data for immediate insights'
            Examples        = @(
                'Live traffic condition analysis',
                'Real-time vehicle health monitoring',
                'Dynamic route adjustments',
                'Emergency response optimization'
            )
            BusBuddyContext = 'Real-time features in BusBuddy dashboard'
            Exercise        = 'Build a real-time data processing pipeline'
        }
    }

    'Advanced'     = @{
        'Computer Vision' = @{
            Description     = 'Using Grok-4 vision capabilities for transportation'
            Examples        = @(
                'Analyzing driver behavior from camera feeds',
                'Detecting maintenance issues from images',
                'Student safety monitoring',
                'Vehicle condition assessment'
            )
            BusBuddyContext = 'Vision-based safety and monitoring features'
            Exercise        = 'Implement image analysis for vehicle inspection'
        }

        'Multi-modal AI'  = @{
            Description     = 'Combining text, images, and sensor data'
            Examples        = @(
                'Comprehensive vehicle diagnostics',
                'Integrated safety systems',
                'Advanced predictive maintenance',
                'Smart fleet management'
            )
            BusBuddyContext = 'Grok-4 integration for comprehensive AI features'
            Exercise        = 'Design a multi-modal safety system'
        }
    }
}

function Start-MLLearningSession {
    [CmdletBinding()]
    param(
        [ValidateSet('Beginner', 'Intermediate', 'Advanced')]
        [string]$Level = 'Beginner',

        [string]$Concept,

        [switch]$Interactive,

        [switch]$UseGrok
    )

    Write-Host "🤖 Starting BusBuddy ML Learning Session" -ForegroundColor Cyan
    Write-Host "=======================================" -ForegroundColor Cyan
    Write-Host "📚 Level: $Level" -ForegroundColor Green

    if ($Concept) {
        Show-MLConcept -Level $Level -Concept $Concept -Interactive:$Interactive -UseGrok:$UseGrok
    }
    else {
        Show-LevelOverview -Level $Level
    }
}

function Show-MLConcept {
    [CmdletBinding()]
    param(
        [string]$Level,
        [string]$Concept,
        [switch]$Interactive,
        [switch]$UseGrok
    )

    if (-not $script:TransportationMLConcepts[$Level][$Concept]) {
        Write-Host "❌ Concept '$Concept' not found at level '$Level'" -ForegroundColor Red
        return
    }

    $conceptData = $script:TransportationMLConcepts[$Level][$Concept]

    Write-Host "`n🎯 Learning: $Concept" -ForegroundColor Yellow
    Write-Host "=" * (10 + $Concept.Length) -ForegroundColor Yellow

    Write-Host "`n📖 Description:" -ForegroundColor Cyan
    Write-Host "   $($conceptData.Description)" -ForegroundColor White

    Write-Host "`n🚌 BusBuddy Context:" -ForegroundColor Cyan
    Write-Host "   $($conceptData.BusBuddyContext)" -ForegroundColor White

    Write-Host "`n💡 Examples:" -ForegroundColor Cyan
    foreach ($example in $conceptData.Examples) {
        Write-Host "   • $example" -ForegroundColor Gray
    }

    Write-Host "`n🏋️ Practical Exercise:" -ForegroundColor Cyan
    Write-Host "   $($conceptData.Exercise)" -ForegroundColor White

    if ($UseGrok) {
        Write-Host "`n🤖 Ask Grok for Details:" -ForegroundColor Magenta
        Write-Host "   bb-ask-grok `"Explain $Concept in transportation context`"" -ForegroundColor Gray
    }

    if ($Interactive) {
        Start-InteractiveLearning -Level $Level -Concept $Concept
    }
}

function Show-LevelOverview {
    [CmdletBinding()]
    param([string]$Level)

    Write-Host "`n📚 $Level Level Concepts:" -ForegroundColor Yellow
    Write-Host "=" * (15 + $Level.Length) -ForegroundColor Yellow

    $concepts = $script:TransportationMLConcepts[$Level]
    $index = 1

    foreach ($concept in $concepts.Keys) {
        Write-Host "`n$index. 🎯 $concept" -ForegroundColor Cyan
        Write-Host "   $($concepts[$concept].Description)" -ForegroundColor Gray
        $index++
    }

    Write-Host "`n🚀 Next Steps:" -ForegroundColor Green
    Write-Host "   • bb-learn-ml -Level $Level -Concept '<concept-name>' -Interactive" -ForegroundColor White
    Write-Host "   • bb-learn-ml -Level $Level -Concept '<concept-name>' -UseGrok" -ForegroundColor White
}

function Start-InteractiveLearning {
    [CmdletBinding()]
    param(
        [string]$Level,
        [string]$Concept
    )

    Write-Host "`n🎮 Interactive Learning Mode" -ForegroundColor Magenta
    Write-Host "============================" -ForegroundColor Magenta

    $conceptData = $script:TransportationMLConcepts[$Level][$Concept]

    # Create a sample dataset based on the concept
    switch ($Concept) {
        'Data Collection' { Show-DataCollectionExercise }
        'Pattern Recognition' { Show-PatternRecognitionExercise }
        'Predictive Modeling' { Show-PredictiveModelingExercise }
        'Route Optimization' { Show-RouteOptimizationExercise }
        'Real-time Analytics' { Show-RealTimeAnalyticsExercise }
        'Computer Vision' { Show-ComputerVisionExercise }
        'Multi-modal AI' { Show-MultiModalExercise }
        default {
            Write-Host "   Interactive exercise for '$Concept' coming soon!" -ForegroundColor Yellow
        }
    }
}

function Show-DataCollectionExercise {
    Write-Host "`n📊 Data Collection Exercise:" -ForegroundColor Yellow
    Write-Host "Here's sample BusBuddy data. What patterns do you see?" -ForegroundColor White

    # Generate sample data
    $sampleData = @"
🚌 Sample Bus Data (Bus #42, Route: Elementary School Loop)
Time        | Speed | Fuel  | Students | Location
08:15:00   | 25mph | 78%   | 0        | Depot
08:18:30   | 35mph | 77%   | 0        | Main St & Oak
08:22:15   | 15mph | 76%   | 12       | Elmwood Pickup
08:26:45   | 30mph | 75%   | 12       | Pine St
08:29:20   | 10mph | 75%   | 23       | Cedar Ave Pickup
08:35:10   | 45mph | 73%   | 23       | Highway 34
08:42:00   | 5mph  | 72%   | 23       | School Arrival

💡 Questions to consider:
   • What affects fuel consumption most?
   • When do speed patterns change?
   • How does student count impact timing?
"@

    Write-Host $sampleData -ForegroundColor Gray

    $response = Read-Host "`n🤔 What patterns do you notice? (Press Enter to continue)"
    if ($response) {
        Write-Host "✅ Great observation: $response" -ForegroundColor Green
    }
}

function Show-PatternRecognitionExercise {
    Write-Host "`n🔍 Pattern Recognition Exercise:" -ForegroundColor Yellow
    Write-Host "Analyze this week's ridership data:" -ForegroundColor White

    $patternData = @"
📈 Weekly Ridership Patterns
Monday:    [████████████████████] 87 students
Tuesday:   [██████████████████  ] 79 students
Wednesday: [████████████████████] 89 students
Thursday:  [███████████████████ ] 83 students
Friday:    [██████████████████  ] 76 students

🕐 Peak Pickup Times:
   7:45 AM - 8:15 AM: Heavy ridership
   3:15 PM - 3:45 PM: Heavy ridership

💡 What patterns suggest optimization opportunities?
"@

    Write-Host $patternData -ForegroundColor Gray
}

function Get-MLLearningProgress {
    [CmdletBinding()]
    param()

    Write-Host "📊 Your ML Learning Progress" -ForegroundColor Cyan
    Write-Host "============================" -ForegroundColor Cyan

    foreach ($level in @('Beginner', 'Intermediate', 'Advanced')) {
        $concepts = $script:TransportationMLConcepts[$level].Keys
        $completed = $script:MLConfig.ProgressTracking[$level] ?? @()
        $progress = if ($concepts.Count -gt 0) { ($completed.Count / $concepts.Count) * 100 } else { 0 }

        $progressBar = "█" * [Math]::Floor($progress / 5) + "░" * (20 - [Math]::Floor($progress / 5))

        Write-Host "`n$level Level: [$progressBar] $([Math]::Round($progress, 1))%" -ForegroundColor Green

        foreach ($concept in $concepts) {
            $status = if ($concept -in $completed) { "✅" } else { "⏳" }
            Write-Host "   $status $concept" -ForegroundColor Gray
        }
    }
}

function Get-GrokMLExample {
    [CmdletBinding()]
    param(
        [string]$Topic,
        [string]$Context = "transportation and bus fleet management"
    )

    Write-Host "🤖 Grok-4 ML Example Request" -ForegroundColor Magenta
    Write-Host "Topic: $Topic" -ForegroundColor White
    Write-Host "Context: $Context" -ForegroundColor White

    $prompt = @"
Explain $Topic in the context of $Context. Include:
1. A clear, practical explanation
2. A specific example using bus/transportation data
3. How this would be implemented in a WPF application like BusBuddy
4. Code snippet or algorithm outline
5. Benefits for fleet management

Keep it beginner-friendly but technically accurate.
"@

    Write-Host "`n📝 Grok Prompt Generated:" -ForegroundColor Yellow
    Write-Host $prompt -ForegroundColor Gray

    Write-Host "`n🚀 To get Grok's response, use:" -ForegroundColor Cyan
    Write-Host "bb-ask-grok `"$($prompt.Replace('"', '\"'))`"" -ForegroundColor Green
}

# Convenient aliases and exports
Set-Alias -Name 'bb-learn-ml' -Value 'Start-MLLearningSession' -Force
Set-Alias -Name 'bb-ml-progress' -Value 'Get-MLLearningProgress' -Force
Set-Alias -Name 'bb-grok-ml' -Value 'Get-GrokMLExample' -Force

Export-ModuleMember -Function @(
    'Start-MLLearningSession',
    'Get-MLLearningProgress',
    'Get-GrokMLExample'
) -Alias @(
    'bb-learn-ml',
    'bb-ml-progress',
    'bb-grok-ml'
)

Write-Host "🤖 BusBuddy ML Learning Module Loaded!" -ForegroundColor Green
Write-Host "📚 Quick Start:" -ForegroundColor Cyan
Write-Host "   bb-learn-ml                    # Overview of beginner concepts" -ForegroundColor White
Write-Host "   bb-learn-ml -Interactive       # Interactive learning mode" -ForegroundColor White
Write-Host "   bb-ml-progress                 # Check your learning progress" -ForegroundColor White
Write-Host "   bb-grok-ml -Topic 'AI Safety'  # Get Grok explanation" -ForegroundColor White
