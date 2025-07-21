# ========================================
# BusBuddy AI-Native Development Launcher
# Your Vision: AI-Powered Maintenance Environment
# ========================================

param(
    [string]$Mode = "interactive", # interactive, mentor, health, quick-fix
    [switch]$SetupXAI,
    [switch]$TestConnection
)

Write-Host ""
Write-Host "🚌🤖 BusBuddy AI-Native Development Environment Launcher" -ForegroundColor Cyan
Write-Host "      Making you a supercharged BusBuddy developer!" -ForegroundColor Green
Write-Host ""

# Setup XAI if requested
if ($SetupXAI) {
    Write-Host "🔑 XAI API Key Setup:" -ForegroundColor Yellow
    Write-Host "1. Go to https://x.ai/ and get your API key" -ForegroundColor Gray
    Write-Host "2. Set environment variable:" -ForegroundColor Gray
    Write-Host '   $env:XAI_API_KEY = "your-api-key-here"' -ForegroundColor Cyan
    Write-Host ""

    $apiKey = Read-Host "Enter your XAI API key (or press Enter to skip)"
    if ($apiKey -and $apiKey.Length -gt 0) {
        $env:XAI_API_KEY = $apiKey
        Write-Host "✅ XAI API key configured for this session!" -ForegroundColor Green
        Write-Host "💡 To make it permanent, add to your PowerShell profile or system environment" -ForegroundColor Yellow
    }
    Write-Host ""
}

# Test XAI connection
if ($TestConnection) {
    Write-Host "🧪 Testing XAI connection..." -ForegroundColor Cyan
    if ($env:XAI_API_KEY) {
        try {
            # Simple test request
            $headers = @{
                'Authorization' = "Bearer $env:XAI_API_KEY"
                'Content-Type' = 'application/json'
            }

            $body = @{
                model = "grok-3-latest"
                messages = @(@{
                    role = "user"
                    content = "Hello! Just testing connection."
                })
                max_tokens = 10
            } | ConvertTo-Json -Depth 10

            $response = Invoke-RestMethod -Uri "https://api.x.ai/v1/chat/completions" -Method POST -Headers $headers -Body $body -TimeoutSec 10
            Write-Host "✅ XAI connection successful!" -ForegroundColor Green
            Write-Host "   Response: $($response.choices[0].message.content)" -ForegroundColor Gray
        }
        catch {
            Write-Host "❌ XAI connection failed: $($_.Exception.Message)" -ForegroundColor Red
            Write-Host "💡 Check your API key and internet connection" -ForegroundColor Yellow
        }
    } else {
        Write-Host "⚠️  XAI API key not configured" -ForegroundColor Yellow
        Write-Host "Run with -SetupXAI to configure your API key" -ForegroundColor Gray
    }
    Write-Host ""
}

# Launch the appropriate mode
switch ($Mode.ToLower()) {
    "interactive" {
        Write-Host "🎯 Starting Interactive AI Development Session..." -ForegroundColor Cyan
        & ".\ai-development-assistant.ps1" -AIMode $true -InteractiveMode $true
    }
    "mentor" {
        Write-Host "👨‍🏫 Starting AI Mentor Session..." -ForegroundColor Cyan
        Write-Host "Ask me anything about BusBuddy development!" -ForegroundColor Green
        & ".\ai-development-assistant.ps1" -AIMode $true -InteractiveMode $true
    }
    "health" {
        Write-Host "🏥 Running AI Health Diagnostics..." -ForegroundColor Green
        & ".\ai-development-assistant.ps1" -AIMode $true -MonitorHealth $true
    }
    "quick-fix" {
        Write-Host "🚀 Quick AI Analysis and Fix..." -ForegroundColor Green
        & ".\ai-development-assistant.ps1" -AIMode $true -AutoFix $true -SemanticAnalysis $true
    }
    default {
        Write-Host "🤖 Available modes:" -ForegroundColor Yellow
        Write-Host "  interactive  - Full AI development environment" -ForegroundColor Green
        Write-Host "  mentor      - AI code mentor and tutor" -ForegroundColor Green
        Write-Host "  health      - AI system health check" -ForegroundColor Green
        Write-Host "  quick-fix   - Quick analysis and auto-fix" -ForegroundColor Green
        Write-Host ""
        Write-Host "Example: .\start-ai-busbuddy.ps1 -Mode interactive" -ForegroundColor Cyan
        Write-Host "Setup:   .\start-ai-busbuddy.ps1 -SetupXAI" -ForegroundColor Cyan
    }
}

# Show your vision
Write-Host ""
Write-Host "🎯 Your Vision:" -ForegroundColor Magenta
Write-Host "   AI-native BusBuddy where XAI helps you maintain and enhance" -ForegroundColor Gray
Write-Host "   the production system with confidence and intelligence!" -ForegroundColor Gray
Write-Host ""
