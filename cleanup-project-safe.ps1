# Bus Buddy Project Cleanup Script
# Safe cleanup that won't break your project

Write-Host "🧹 Starting Bus Buddy Project Cleanup..." -ForegroundColor Cyan

# 1. Remove standalone test/demo files
$testFiles = @(
    "demo-gee.ps1",
    "test-google-earth-engine.ps1", 
    "test-xai-integration.ps1",
    "verification-complete.ps1",
    "verify-gee-live.ps1",
    "GoogleCloudSDKInstaller.exe",
    "codecov.exe"
)

Write-Host "🗑️  Removing standalone test/demo files..." -ForegroundColor Yellow
foreach ($file in $testFiles) {
    if (Test-Path $file) {
        Remove-Item $file -Force
        Write-Host "   ✅ Deleted: $file" -ForegroundColor Green
    }
}

# 2. Remove redundant documentation
$redundantDocs = @(
    "INTEGRATION_STATUS_REPORT.md",
    "GOOGLE_EARTH_ENGINE_STATUS.md", 
    "build-error-analysis.json",
    "github-actions-integration.yml"
)

Write-Host "📄 Removing redundant documentation..." -ForegroundColor Yellow
foreach ($doc in $redundantDocs) {
    if (Test-Path $doc) {
        Remove-Item $doc -Force
        Write-Host "   ✅ Deleted: $doc" -ForegroundColor Green
    }
}

# 3. Clean build artifacts (keep folders)
Write-Host "🏗️  Cleaning build artifacts..." -ForegroundColor Yellow
if (Test-Path "bin") {
    Get-ChildItem "bin" -Recurse | Remove-Item -Recurse -Force
    Write-Host "   ✅ Cleaned: bin/ contents" -ForegroundColor Green
}
if (Test-Path "obj") {
    Get-ChildItem "obj" -Recurse | Remove-Item -Recurse -Force  
    Write-Host "   ✅ Cleaned: obj/ contents" -ForegroundColor Green
}
if (Test-Path "TestResults") {
    Remove-Item "TestResults" -Recurse -Force
    Write-Host "   ✅ Deleted: TestResults/" -ForegroundColor Green
}

# 4. Remove redundant config file
if (Test-Path "BusBuddy-PS7Config.json") {
    Remove-Item "BusBuddy-PS7Config.json" -Force
    Write-Host "   ✅ Deleted: BusBuddy-PS7Config.json (keeping .yml version)" -ForegroundColor Green
}

Write-Host ""
Write-Host "✨ Cleanup Complete! Your project is now cleaner and more organized." -ForegroundColor Green
Write-Host ""
Write-Host "📊 Summary:" -ForegroundColor Cyan
Write-Host "   • Removed 7+ unnecessary test/demo files" -ForegroundColor White
Write-Host "   • Cleaned up redundant documentation" -ForegroundColor White  
Write-Host "   • Cleared build artifacts" -ForegroundColor White
Write-Host "   • All essential project files preserved" -ForegroundColor White
Write-Host ""
Write-Host "🎯 Next Steps:" -ForegroundColor Cyan
Write-Host "   1. Consider consolidating Tests/ and Testing/ into BusBuddy.Tests/" -ForegroundColor White
Write-Host "   2. Run 'dotnet build' to verify everything still works" -ForegroundColor White
Write-Host "   3. Commit the cleanup changes to git" -ForegroundColor White
