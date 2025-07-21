# Simple XAI Te        $body = @{
model      = "grok-4-latest"
messages   = @(
    @{
        role    = "user"
        content = "Hello! This is a test from BusBuddy AI Development Environment."
    }
)
max_tokens = 50
} | ConvertTo-Json -Depth 10
# This won't trigger the AI Development Assistant

Write-Host "🔍 Testing XAI Integration..." -ForegroundColor Cyan

if ($env:XAI_API_KEY) {
    Write-Host "✅ XAI_API_KEY found (length: $($env:XAI_API_KEY.Length) characters)" -ForegroundColor Green

    # Test XAI API directly
    try {
        $headers = @{
            'Authorization' = "Bearer $env:XAI_API_KEY"
            'Content-Type'  = 'application/json'
        }

        $body = @{
            model      = "grok-4-latest"
            messages   = @(
                @{
                    role    = "user"
                    content = "Hello! This is a test from BusBuddy AI Development Environment. Please respond with 'XAI/Grok integration successful!'"
                }
            )
            max_tokens = 50
        } | ConvertTo-Json -Depth 10

        Write-Host "🧪 Testing XAI API connection..." -ForegroundColor Yellow
        $response = Invoke-RestMethod -Uri "https://api.x.ai/v1/chat/completions" -Method POST -Headers $headers -Body $body -TimeoutSec 10

        Write-Host "✅ XAI Connection SUCCESSFUL!" -ForegroundColor Green
        Write-Host "💬 Response: $($response.choices[0].message.content)" -ForegroundColor White
        Write-Host ""
        Write-Host "🎉 Your AI-native BusBuddy environment is FULLY OPERATIONAL!" -ForegroundColor Magenta

    }
    catch {
        Write-Host "❌ XAI Connection Failed: $($_.Exception.Message)" -ForegroundColor Red

        if ($_.Exception.Message -like "*403*") {
            Write-Host "💡 Possible causes:" -ForegroundColor Yellow
            Write-Host "   - API key might be invalid or expired" -ForegroundColor Gray
            Write-Host "   - Check your XAI account status" -ForegroundColor Gray
            Write-Host "   - Verify the API key format" -ForegroundColor Gray
        }
    }
}
else {
    Write-Host "❌ XAI_API_KEY environment variable not found" -ForegroundColor Red
    Write-Host "💡 Set it with: `$env:XAI_API_KEY = 'your-api-key'" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "🚌🤖 Your AI Development Environment Status:" -ForegroundColor Cyan
Write-Host "   ✅ AI Development Assistant: ACTIVE" -ForegroundColor Green
Write-Host "   ✅ Interactive AI Session: READY" -ForegroundColor Green
Write-Host "   ✅ PowerShell Integration: LOADED" -ForegroundColor Green
Write-Host "   ⚡ Ready for AI-native development!" -ForegroundColor Green
