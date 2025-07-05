# Quick Reference: When to Use PowerShell Tools
# Visual Cheat Sheet for Bus Buddy Development

Write-Host '🎯 QUICK REFERENCE: When to Use PowerShell Tools' -ForegroundColor Cyan
Write-Host '=================================================' -ForegroundColor Cyan

Write-Host "`n📊 SYMPTOMS → SOLUTIONS:" -ForegroundColor Yellow

$symptoms = @(
    @{
        Emoji   = '❌'
        Symptom = 'Build fails with errors'
        Visual  = 'Red text flooding terminal'
        Tool    = 'Enhanced-CI-Integration.ps1 -Mode Analysis -AnalyzeBuildErrors'
        Result  = 'Categorized error list → Fix systematically'
    },
    @{
        Emoji   = '⏱️'
        Symptom = 'Tests are slow (>3 minutes)'
        Visual  = 'Long wait times, progress bars'
        Tool    = 'Enhanced-CI-Integration.ps1 -Mode Local -GenerateReports'
        Result  = '5x faster parallel execution'
    },
    @{
        Emoji   = '📉'
        Symptom = 'Low code coverage (<50%)'
        Visual  = 'Codecov showing red percentages'
        Tool    = 'Enhanced-CI-Integration.ps1 -Mode Local -GenerateReports'
        Result  = 'Rich HTML reports showing gaps'
    },
    @{
        Emoji   = '🤖'
        Symptom = 'CI/CD takes forever'
        Visual  = 'GitHub Actions >10 minutes'
        Tool    = 'Enhanced-CI-Integration.ps1 -Mode CI'
        Result  = 'Cut CI time in half'
    },
    @{
        Emoji   = '❓'
        Symptom = "Don't know what's wrong"
        Visual  = 'Generic failure messages'
        Tool    = 'Tool-Decision-Guide.ps1 -Interactive'
        Result  = 'Smart guidance system'
    },
    @{
        Emoji   = '🆕'
        Symptom = 'Starting new development'
        Visual  = 'Fresh workspace setup'
        Tool    = 'Install-DeveloperTools.ps1'
        Result  = 'All tools ready to go'
    },
    @{
        Emoji   = '🔍'
        Symptom = 'Need to debug complex issues'
        Visual  = 'Mysterious behavior'
        Tool    = 'PowerShell7-Integration.ps1'
        Result  = 'Advanced debugging features'
    },
    @{
        Emoji   = '🧪'
        Symptom = 'Writing new tests'
        Visual  = 'Need test framework setup'
        Tool    = 'NUnit-TestingTools.ps1'
        Result  = 'Comprehensive test environment'
    }
)

foreach ($item in $symptoms) {
    Write-Host "`n$($item.Emoji) WHEN YOU SEE: $($item.Visual)" -ForegroundColor White
    Write-Host "   Problem: $($item.Symptom)" -ForegroundColor Gray
    Write-Host "   Use: $($item.Tool)" -ForegroundColor Green
    Write-Host "   Get: $($item.Result)" -ForegroundColor Cyan
}

Write-Host "`n🚀 QUICK START COMMANDS:" -ForegroundColor Yellow
Write-Host '=========================' -ForegroundColor Yellow

Write-Host "`n1️⃣ I don't know what I need:" -ForegroundColor Cyan
Write-Host '   .\Scripts\Tool-Decision-Guide.ps1 -Interactive' -ForegroundColor Yellow

Write-Host "`n2️⃣ My build is broken:" -ForegroundColor Cyan
Write-Host '   .\Scripts\Enhanced-CI-Integration.ps1 -Mode Analysis -AnalyzeBuildErrors' -ForegroundColor Yellow

Write-Host "`n3️⃣ I want faster tests and better coverage:" -ForegroundColor Cyan
Write-Host '   .\Scripts\Enhanced-CI-Integration.ps1 -Mode Local -GenerateReports' -ForegroundColor Yellow

Write-Host "`n4️⃣ Setting up for new development:" -ForegroundColor Cyan
Write-Host '   .\Scripts\Install-DeveloperTools.ps1' -ForegroundColor Yellow

Write-Host "`n5️⃣ See all available tools:" -ForegroundColor Cyan
Write-Host '   .\Scripts\Demo-AllTools.ps1' -ForegroundColor Yellow

Write-Host "`n💡 PRO TIP: When in doubt, start with the Interactive Guide!" -ForegroundColor Green
Write-Host '   .\Scripts\Tool-Decision-Guide.ps1 -Interactive' -ForegroundColor Yellow
