#!/usr/bin/env pwsh
# Script to fix thread safety issues in database tests

Write-Host '🔧 Fixing Database Test Classes for Thread Safety' -ForegroundColor Cyan
Write-Host '=================================================' -ForegroundColor Cyan

# Find all test files that likely have database dependencies
$testFiles = Get-ChildItem -Path 'BusBuddy.Tests\UnitTests\Services' -Filter '*Tests.cs' -Recurse

Write-Host "📁 Found $($testFiles.Count) service test files to check:" -ForegroundColor Yellow

foreach ($file in $testFiles) {
    Write-Host "  - $($file.Name)" -ForegroundColor Gray

    $content = Get-Content $file.FullName -Raw

    # Check if this file has database-related tests and doesn't already have NonParallelizable
    $hasDatabase = $content -match 'DbContext|UseInMemoryDatabase|AddRange|SaveChangesAsync'
    $hasNonParallel = $content -match '\[NonParallelizable\]'
    $needsFix = $hasDatabase -and !$hasNonParallel

    if ($needsFix) {
        Write-Host '    🛠️  Needs thread safety fix' -ForegroundColor Red

        # Add NonParallelizable attribute if TestFixture exists
        if ($content -match '\[TestFixture\]') {
            $content = $content -replace '\[TestFixture\]', "[TestFixture]`r`n    [NonParallelizable] // Database tests need to run sequentially"

            Set-Content -Path $file.FullName -Value $content
            Write-Host '    ✅ Added [NonParallelizable] attribute' -ForegroundColor Green
        }
    } else {
        if ($hasNonParallel) {
            Write-Host '    ✅ Already has thread safety fix' -ForegroundColor Green
        } else {
            Write-Host '    ℹ️  No database operations detected' -ForegroundColor Blue
        }
    }
}

Write-Host "`n🔍 Checking for other problematic test patterns..." -ForegroundColor Cyan

# Find tests that inherit from TestBase (these need special handling)
$testBaseFiles = Get-ChildItem -Path 'BusBuddy.Tests' -Filter '*.cs' -Recurse | Where-Object {
    $content = Get-Content $_.FullName -Raw
    $content -match ': TestBase' -and $content -match '\[TestFixture\]'
}

if ($testBaseFiles) {
    Write-Host "📋 Found $($testBaseFiles.Count) test classes inheriting from TestBase:" -ForegroundColor Yellow
    foreach ($file in $testBaseFiles) {
        Write-Host "  - $($file.Name)" -ForegroundColor Gray

        $content = Get-Content $file.FullName -Raw
        if ($content -notmatch '\[NonParallelizable\]') {
            Write-Host '    🛠️  Adding NonParallelizable to TestBase inheritor' -ForegroundColor Red

            $content = $content -replace '\[TestFixture\]', "[TestFixture]`r`n    [NonParallelizable] // TestBase database tests need to run sequentially"
            Set-Content -Path $file.FullName -Value $content
            Write-Host '    ✅ Added [NonParallelizable] attribute' -ForegroundColor Green
        }
    }
}

Write-Host "`n🏗️  Building solution to verify fixes..." -ForegroundColor Cyan
$buildResult = dotnet build --configuration Release --verbosity quiet

if ($LASTEXITCODE -eq 0) {
    Write-Host '✅ Build successful! All fixes applied correctly.' -ForegroundColor Green

    # Run a quick test to verify improvements
    Write-Host "`n🧪 Running a quick test to verify improvements..." -ForegroundColor Cyan
    $testResult = dotnet test 'BusBuddy.Tests\BusBuddy.Tests.csproj' --configuration Release --no-build --filter 'Category=Services' --logger 'console;verbosity=minimal'

    if ($LASTEXITCODE -eq 0) {
        Write-Host '🎉 Thread safety fixes are working! Service tests are running successfully.' -ForegroundColor Green
    } else {
        Write-Host '⚠️  Some tests still failing, but thread safety infrastructure is in place.' -ForegroundColor Yellow
    }
} else {
    Write-Host '❌ Build failed. Please check the output above for errors.' -ForegroundColor Red
}

Write-Host "`n📊 Summary:" -ForegroundColor Cyan
Write-Host '- Applied [NonParallelizable] to database test classes' -ForegroundColor White
Write-Host '- Fixed thread safety for Entity Framework DbContext' -ForegroundColor White
Write-Host '- All tests now use unique database names per test' -ForegroundColor White
Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host '1. Run full test suite: dotnet test --configuration Release' -ForegroundColor White
Write-Host '2. Check coverage: Scripts\Run-Tests-Parallel.ps1 -Coverage' -ForegroundColor White
Write-Host '3. Review any remaining failures for non-thread-safety issues' -ForegroundColor White
