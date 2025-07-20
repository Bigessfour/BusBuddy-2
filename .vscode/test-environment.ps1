#!/usr/bin/env pwsh
# Quick test script to verify environment variables are properly configured

Write-Host '🔍 Environment Variable Configuration Test' -ForegroundColor Green
Write-Host '==========================================' -ForegroundColor Green

# Test 1: Check if environment variables are set
Write-Host "`n✅ Testing Environment Variables:" -ForegroundColor Yellow

$envVars = @{
    'ASPNETCORE_ENVIRONMENT'     = $env:ASPNETCORE_ENVIRONMENT
    'SYNCFUSION_LICENSE_KEY'     = $env:SYNCFUSION_LICENSE_KEY
    'DATABASE_CONNECTION_STRING' = $env:DATABASE_CONNECTION_STRING
}

ForEach-Object ($var in $envVars.GetEnumerator()) {
    if ($var.Value) {
        if ($var.Key -eq 'DATABASE_CONNECTION_STRING') {
            Write-Host "   ✓ $($var.Key): SET (length: $($var.Value.Length) chars)" -ForegroundColor Green
        } elseif ($var.Key -eq 'SYNCFUSION_LICENSE_KEY') {
            Write-Host "   ✓ $($var.Key): SET (length: $($var.Value.Length) chars)" -ForegroundColor Green
        } else {
            Write-Host "   ✓ $($var.Key): $($var.Value)" -ForegroundColor Green
        }
    } else {
        Write-Host "   ❌ $($var.Key): NOT SET" -ForegroundColor Red
    }
}

# Test 2: Test configuration loading
Write-Host "`n✅ Testing Configuration File Loading:" -ForegroundColor Yellow

$appsettingsPath = 'BusBuddy.WPF\appsettings.json'
if (Test-Path $appsettingsPath) {
    Write-Host '   ✓ appsettings.json found' -ForegroundColor Green

    try {
        $config = Get-Content $appsettingsPath | ConvertFrom-Json

        # Check if it uses environment variables
        if ($config.ConnectionStrings.DefaultConnection -eq '${DATABASE_CONNECTION_STRING}') {
            Write-Host '   ✓ DefaultConnection uses environment variable' -ForegroundColor Green
        } else {
            Write-Host "   ⚠️  DefaultConnection: $($config.ConnectionStrings.DefaultConnection)" -ForegroundColor Yellow
        }

        if ($config.SyncfusionLicenseKey -eq '${SYNCFUSION_LICENSE_KEY}') {
            Write-Host '   ✓ SyncfusionLicenseKey uses environment variable' -ForegroundColor Green
        } else {
            Write-Host "   ⚠️  SyncfusionLicenseKey: $($config.SyncfusionLicenseKey)" -ForegroundColor Yellow
        }

        Write-Host "   ✓ DatabaseProvider: $($config.DatabaseProvider)" -ForegroundColor Green

    } catch {
        Write-Host "   ❌ Error reading appsettings.json: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "   ❌ appsettings.json not found at $appsettingsPath" -ForegroundColor Red
}

# Test 3: Connection string validation
Write-Host "`n✅ Testing Database Connection String:" -ForegroundColor Yellow

if ($env:DATABASE_CONNECTION_STRING) {
    $connString = $env:DATABASE_CONNECTION_STRING

    # Basic validation
    if ($connString -match 'Server=tcp:busbuddy-server-sm2\.database\.windows\.net') {
        Write-Host '   ✓ Azure SQL Server endpoint detected' -ForegroundColor Green
    }

    if ($connString -match 'Initial Catalog=BusBuddyDB') {
        Write-Host '   ✓ Database name: BusBuddyDB' -ForegroundColor Green
    }

    if ($connString -match 'Encrypt=true') {
        Write-Host '   ✓ Encryption enabled' -ForegroundColor Green
    }

    if ($connString -match 'User ID=busbuddy_admin') {
        Write-Host '   ✓ User ID: busbuddy_admin' -ForegroundColor Green
    }

} else {
    Write-Host '   ❌ DATABASE_CONNECTION_STRING not available for testing' -ForegroundColor Red
}

Write-Host "`n🎉 Environment test completed!" -ForegroundColor Green
Write-Host '📝 Next steps:' -ForegroundColor Cyan
Write-Host '   1. If all tests passed, you can run the application' -ForegroundColor White
Write-Host '   2. If any tests failed, check environment variable setup' -ForegroundColor White
Write-Host '   3. Restart VS Code to ensure it picks up environment variables' -ForegroundColor White
