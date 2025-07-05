# Demo script to showcase all the visual decorations
# Run this to see the full spectacle!

# Load the decorated functions
. '.\Scripts\Tool-Decision-Guide.ps1'

Write-Host '🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪' -ForegroundColor Magenta
Write-Host '🎪                     🎨 DECORATION SHOWCASE 🎨                           🎪' -ForegroundColor Magenta
Write-Host '🎪                   See All the Visual Effects!                          🎪' -ForegroundColor Magenta
Write-Host '🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪🎪' -ForegroundColor Magenta
Write-Host ''

# Demo 1: Tool Animation
Write-Host '🎬 Demo 1: Tool Launch Animation' -ForegroundColor Yellow
Start-ToolAnimation -ToolName 'Enhanced PowerShell Toolkit' -Action 'Demonstrating'

# Demo 2: Progress Spinner
Write-Host '🎬 Demo 2: Progress Spinner' -ForegroundColor Yellow
Show-ProgressSpinner -Message 'Processing awesome decorations' -Seconds 3

# Demo 3: Success Banner
Write-Host '🎬 Demo 3: Success Banner' -ForegroundColor Yellow
Show-SuccessBanner -Message 'DECORATIONS WORKING PERFECTLY'

# Demo 4: Error Banner
Write-Host '🎬 Demo 4: Error Banner (fake error)' -ForegroundColor Yellow
Show-ErrorBanner -Message 'DEMO ERROR - NOT REAL'

# Demo 5: Info Box
Write-Host '🎬 Demo 5: Info Box' -ForegroundColor Yellow
Show-InfoBox -Title 'INFORMATION DISPLAY' -Content @(
    '🎨 This is how information looks',
    '✨ Pretty neat with borders and colors',
    '🔧 Perfect for tool recommendations',
    '🎯 Easy to read and understand'
) -Color 'Cyan'

# Demo 6: Command Preview
Write-Host '🎬 Demo 6: Command Preview' -ForegroundColor Yellow
Show-CommandPreview -Command '.\Scripts\Amazing-Tool.ps1 -Awesome' -Description 'Demonstrates how commands look before execution'

# Demo 7: Tool Menu
Write-Host '🎬 Demo 7: Tool Selection Menu' -ForegroundColor Yellow
$demoTools = @(
    @{ Name = 'Super Builder'; Description = 'Builds everything amazingly fast' },
    @{ Name = 'Test Rocket'; Description = 'Launches tests at light speed' },
    @{ Name = 'Coverage Master'; Description = 'Generates beautiful coverage reports' }
)
Show-ToolMenu -Tools $demoTools

Write-Host ''
Write-Host '🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊' -ForegroundColor Green
Write-Host '🎊                     🎉 DECORATION DEMO COMPLETE! 🎉                     🎊' -ForegroundColor Green
Write-Host "🎊              Now when you run tools, they'll look AMAZING!              🎊" -ForegroundColor Green
Write-Host '🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊🎊' -ForegroundColor Green
Write-Host ''

Write-Host '🚀 To see the full interactive experience with decorations:' -ForegroundColor Cyan
Write-Host '   .\Scripts\Tool-Decision-Guide.ps1 -Interactive' -ForegroundColor Yellow
Write-Host ''
Write-Host "🎮 Every time you 'poke a button', you'll get:" -ForegroundColor White
Write-Host '   ✨ Animated progress bars' -ForegroundColor Gray
Write-Host '   🎆 Colorful banners and fireworks' -ForegroundColor Gray
Write-Host '   📊 Beautiful info boxes' -ForegroundColor Gray
Write-Host '   🎯 Epic menu selections' -ForegroundColor Gray
Write-Host '   🚀 Launch countdowns' -ForegroundColor Gray
Write-Host '   🎉 Success celebrations' -ForegroundColor Gray
Write-Host ''
