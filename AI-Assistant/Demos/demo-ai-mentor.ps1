# AI Mentor Demonstration Script
# Give the XAI AI Mentor real BusBuddy tasks to showcase capabilities

Write-Host "🧠 AI MENTOR DEMONSTRATION - Real BusBuddy Tasks" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Gray

# Set up environment - API key should be set externally
if (-not $env:XAI_API_KEY) {
    Write-Host "⚠️ XAI_API_KEY environment variable not set" -ForegroundColor Yellow
    Write-Host "💡 Set it with: & '.\AI-Assistant\XAI-Integration\activate-xai-key.ps1'" -ForegroundColor Cyan
    exit 1
}

# Load AI assistant quietly
. ".\ai-development-assistant.ps1" -AIMode $true -InteractiveMode $false -AutoFix $false

Write-Host ""
Write-Host "🎯 TASK 1: Code Review - MainWindow.xaml.cs" -ForegroundColor Yellow
Write-Host "============================================" -ForegroundColor Gray

# Read some actual code for the mentor to analyze
$mainWindowCode = ""
if (Test-Path "BusBuddy.WPF\Views\MainWindow.xaml.cs") {
    $mainWindowCode = Get-Content "BusBuddy.WPF\Views\MainWindow.xaml.cs" -Raw -ErrorAction SilentlyContinue
    $codeSnippet = $mainWindowCode.Substring(0, [math]::Min(500, $mainWindowCode.Length))
}
else {
    $codeSnippet = "MainWindow code not found at expected location"
}

$mentorResponse1 = Invoke-XAICodeMentor -Question "Review this MainWindow code and suggest improvements for MVVM pattern compliance and performance" -CodeContext $codeSnippet -TaskType "debug"

Write-Host ""
Write-Host "🎯 TASK 2: Database Performance Analysis" -ForegroundColor Yellow
Write-Host "=======================================" -ForegroundColor Gray

$mentorResponse2 = Invoke-XAICodeMentor -Question "How should I optimize Entity Framework queries in BusBuddy for better performance when loading driver and vehicle data?" -TaskType "enhance"

Write-Host ""
Write-Host "🎯 TASK 3: Syncfusion Control Integration" -ForegroundColor Yellow
Write-Host "=========================================" -ForegroundColor Gray

$mentorResponse3 = Invoke-XAICodeMentor -Question "What's the best way to implement a Syncfusion DataGrid for displaying bus route information with sorting, filtering, and pagination?" -TaskType "enhance"

Write-Host ""
Write-Host "🎯 TASK 4: AI Maintenance Assistant Demo" -ForegroundColor Yellow
Write-Host "=======================================" -ForegroundColor Gray

$assistantResponse = Invoke-AIMaintenanceAssistant -NaturalLanguageRequest "I want to add a new feature to track fuel consumption for each bus. What files do I need to modify and what's the best approach?"

Write-Host ""
Write-Host "🎯 TASK 5: Issue Report Generation" -ForegroundColor Yellow
Write-Host "=================================" -ForegroundColor Gray

$issueReport = New-AIIssueReport -IssueDescription "Application crashes when loading large datasets" -ExpectedBehavior "Should load smoothly with progress indication" -ActualBehavior "UI freezes and app becomes unresponsive" -ErrorMessages "OutOfMemoryException in DataGrid binding"

Write-Host ""
Write-Host "✅ AI MENTOR DEMONSTRATION COMPLETE!" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Gray
Write-Host "🧠 XAI Code Mentor: Provided expert guidance on 3 development tasks" -ForegroundColor Green
Write-Host "🤖 AI Maintenance Assistant: Analyzed feature request with actionable plan" -ForegroundColor Green
Write-Host "📝 AI Issue Reporter: Generated comprehensive issue documentation" -ForegroundColor Green
Write-Host ""
Write-Host "🚀 Your AI-Native BusBuddy development environment is ACTIVE!" -ForegroundColor Cyan
Write-Host "   Ready to assist with all aspects of BusBuddy development and maintenance" -ForegroundColor White
