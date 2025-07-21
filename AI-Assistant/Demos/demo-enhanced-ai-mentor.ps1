# ========================================
# Enhanced AI Mentor Demo for BusBuddy
# ========================================

Write-Host "🎓 Enhanced AI Mentor Demo - Interactive AI Development Assistant" -ForegroundColor Cyan
Write-Host "=" * 70 -ForegroundColor Gray
Write-Host ""

# Check prerequisites
if (-not $env:XAI_API_KEY) {
    Write-Host "❌ XAI API Key not configured!" -ForegroundColor Red
    Write-Host "   Please set your API key: `$env:XAI_API_KEY = 'your-key'" -ForegroundColor Yellow
    Write-Host "   Then run this demo again." -ForegroundColor Yellow
    exit
}

Write-Host "✅ XAI API Key configured" -ForegroundColor Green

# Load the enhanced AI assistant
Write-Host "📁 Loading enhanced AI development assistant..." -ForegroundColor Cyan
. ".\ai-development-assistant.ps1" -AIMode $false -SkipAIEnhancement

Write-Host ""
Write-Host "🚀 Enhanced AI Mentor Features Demo:" -ForegroundColor Magenta
Write-Host ""
Write-Host "1. Quick Mentor Questions" -ForegroundColor Green
Write-Host "2. Interactive Mentor Session" -ForegroundColor Green
Write-Host "3. Debug with Mentor" -ForegroundColor Green
Write-Host "4. Architecture Review" -ForegroundColor Green
Write-Host "5. View Recent Sessions" -ForegroundColor Green
Write-Host "6. Export Knowledge" -ForegroundColor Green
Write-Host "7. Full Interactive AI Session" -ForegroundColor Green
Write-Host ""

$choice = Read-Host "Select demo option (1-7, or 'exit')"

switch ($choice) {
    "1" {
        Write-Host ""
        Write-Host "🤖 Demo: Quick Mentor Question" -ForegroundColor Cyan
        Write-Host "This shows a one-shot question without interactive follow-up" -ForegroundColor Gray
        Write-Host ""

        Ask-BusBuddyMentor -Question "How do I add a new WPF view to BusBuddy following MVVM patterns?" -TaskType "explain"
    }

    "2" {
        Write-Host ""
        Write-Host "🎓 Demo: Interactive Mentor Session" -ForegroundColor Cyan
        Write-Host "This starts a full conversation with Grok where you can ask follow-up questions" -ForegroundColor Gray
        Write-Host ""

        Start-BusBuddyMentorSession -InitialQuestion "I want to understand how the BusBuddy MVVM architecture works. Can you explain it step by step?"
    }

    "3" {
        Write-Host ""
        Write-Host "🐛 Demo: Debug with Mentor" -ForegroundColor Cyan
        Write-Host "This helps debug specific issues with context" -ForegroundColor Gray
        Write-Host ""

        Debug-WithMentor -ErrorMessage "System.ArgumentNullException: Value cannot be null" -FilePath "BusBuddy.WPF/ViewModels/DriverViewModel.cs" -CodeSnippet "var driver = new Driver { Name = null };"
    }

    "4" {
        Write-Host ""
        Write-Host "🏗️ Demo: Architecture Review" -ForegroundColor Cyan
        Write-Host "This reviews architecture and best practices" -ForegroundColor Gray
        Write-Host ""

        Review-ArchitectureWithMentor -Component "dependency injection setup"
    }

    "5" {
        Write-Host ""
        Write-Host "📚 Demo: View Recent Sessions" -ForegroundColor Cyan
        Write-Host "This shows your conversation history" -ForegroundColor Gray
        Write-Host ""

        Show-RecentMentorSessions -Count 3
    }

    "6" {
        Write-Host ""
        Write-Host "📁 Demo: Export Knowledge" -ForegroundColor Cyan
        Write-Host "This exports your mentor sessions for review" -ForegroundColor Gray
        Write-Host ""

        Export-MentorKnowledge -OutputPath "demo-mentor-export.json"
    }

    "7" {
        Write-Host ""
        Write-Host "🎯 Demo: Full Interactive AI Session" -ForegroundColor Cyan
        Write-Host "This starts the complete AI development environment" -ForegroundColor Gray
        Write-Host ""

        $aiKnowledge = Get-AIKnowledgeBase
        Start-InteractiveAISession -AIKnowledge $aiKnowledge
    }

    "exit" {
        Write-Host "👋 Demo ended!" -ForegroundColor Green
        exit
    }

    default {
        Write-Host "❌ Invalid choice. Please run the demo again." -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "✨ Demo completed! The enhanced AI mentor is now ready for your development workflow." -ForegroundColor Green
Write-Host ""
Write-Host "💡 Quick Access Functions:" -ForegroundColor Cyan
Write-Host "   Ask-BusBuddyMentor 'your question'" -ForegroundColor White
Write-Host "   Start-BusBuddyMentorSession" -ForegroundColor White
Write-Host "   Debug-WithMentor -ErrorMessage 'error details'" -ForegroundColor White
Write-Host "   Review-ArchitectureWithMentor" -ForegroundColor White
Write-Host "   Show-RecentMentorSessions" -ForegroundColor White
Write-Host ""
