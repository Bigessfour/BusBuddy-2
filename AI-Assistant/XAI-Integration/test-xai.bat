@echo off
echo Testing XAI API connection...
if "%XAI_API_KEY%"=="" (
    echo ERROR: XAI_API_KEY environment variable not set
    echo Please set it with: set XAI_API_KEY=your-key-here
    pause
    exit /b 1
)
echo API Key configured from environment variable
echo Testing XAI Connection with Grok-4...
powershell -NoProfile -NoLogo -ExecutionPolicy Bypass -Command "& { if (-not $env:XAI_API_KEY) { Write-Host 'ERROR: XAI_API_KEY environment variable not set' -ForegroundColor Red; exit 1 }; Write-Host 'API Key Length:' $env:XAI_API_KEY.Length; try { $h = @{'Authorization'='Bearer '+$env:XAI_API_KEY; 'Content-Type'='application/json'}; $b = '{\"model\":\"grok-4-latest\",\"messages\":[{\"role\":\"user\",\"content\":\"Hello from BusBuddy!\"}],\"max_tokens\":20}'; Write-Host 'Making API call to Grok-4...'; $r = Invoke-RestMethod -Uri 'https://api.x.ai/v1/chat/completions' -Method POST -Headers $h -Body $b -TimeoutSec 15; Write-Host 'SUCCESS! Grok-4 Response:' $r.choices[0].message.content -ForegroundColor Green } catch { Write-Host 'FAILED:' $_.Exception.Message -ForegroundColor Red; if ($_.Exception.Message -match '403') { Write-Host 'Key appears invalid or expired' -ForegroundColor Yellow } } }"
pause
