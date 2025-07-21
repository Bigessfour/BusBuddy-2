# Direct XAI Test - No AI Assistant Integration
# Minimal test to verify API key only

Write-Host "🔑 Testing XAI API Key..." -ForegroundColor Cyan

if ($env:XAI_API_KEY) {
    $keyLength = $env:XAI_API_KEY.Length
    Write-Host "✅ Found XAI_API_KEY ($keyLength chars)" -ForegroundColor Green

    try {
        $headers = @{
            'Authorization' = "Bearer $env:XAI_API_KEY"
            'Content-Type'  = 'application/json'
        }

        $body = @{
            model      = "grok-4-latest"
            messages   = @(@{
                    role    = "user"
                    content = "Hello from BusBuddy! Please confirm XAI connection works."
                })
            max_tokens = 30
        } | ConvertTo-Json -Depth 5

        Write-Host "🧪 Calling XAI API..." -ForegroundColor Yellow
        $response = Invoke-RestMethod -Uri "https://api.x.ai/v1/chat/completions" -Method POST -Headers $headers -Body $body -TimeoutSec 10

        Write-Host "✅ SUCCESS!" -ForegroundColor Green
        Write-Host "Response: $($response.choices[0].message.content)" -ForegroundColor White

    }
    catch {
        Write-Host "❌ FAILED: $($_.Exception.Message)" -ForegroundColor Red

        if ($_.Exception.Message -match "403") {
            Write-Host "" -ForegroundColor Yellow
            Write-Host "🔍 API Key Issue Detected:" -ForegroundColor Yellow
            Write-Host "  • Key might be expired or invalid" -ForegroundColor Gray
            Write-Host "  • Check your XAI Console for key status" -ForegroundColor Gray
            Write-Host "  • Try regenerating the API key" -ForegroundColor Gray
        }
    }
}
else {
    Write-Host "❌ No XAI_API_KEY found" -ForegroundColor Red
}
