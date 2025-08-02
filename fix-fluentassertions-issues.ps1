# PowerShell script to fix FluentAssertions HaveLength issues
# This addresses the most common type of error in the 335 problems

$workspace = "c:\Users\steve.mckitrick\Desktop\BusBuddy"
$patterns = @(
    @{
        # Pattern: .Should().HaveLength(c => c <= X)
        Find = '\.Should\(\)\.HaveLength\(c => c <= (\d+),'
        Replace = '.Length.Should().BeLessOrEqualTo($1,'
    },
    @{
        # Pattern: .Should().HaveLength(c => c >= X)
        Find = '\.Should\(\)\.HaveLength\(c => c >= (\d+),'
        Replace = '.Length.Should().BeGreaterOrEqualTo($1,'
    },
    @{
        # Pattern: .Should().HaveLength(c => c < X)
        Find = '\.Should\(\)\.HaveLength\(c => c < (\d+),'
        Replace = '.Length.Should().BeLessThan($1,'
    },
    @{
        # Pattern: .Should().HaveLength(c => c > X)
        Find = '\.Should\(\)\.HaveLength\(c => c > (\d+),'
        Replace = '.Length.Should().BeGreaterThan($1,'
    },
    @{
        # Pattern: .Should().HaveLength(c => c == X)
        Find = '\.Should\(\)\.HaveLength\(c => c == (\d+),'
        Replace = '.Length.Should().Be($1,'
    }
)

Write-Host "🔧 Starting systematic fix of FluentAssertions HaveLength issues..." -ForegroundColor Cyan

$totalFixed = 0
$files = Get-ChildItem -Path "$workspace\BusBuddy.UITests" -Filter "*.cs" -Recurse

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    $fileFixed = 0

    foreach ($pattern in $patterns) {
        $regex = [regex]::new($pattern.Find)
        $matches = $regex.Matches($content)

        if ($matches.Count -gt 0) {
            $content = $regex.Replace($content, $pattern.Replace)
            $fileFixed += $matches.Count
        }
    }

    if ($fileFixed -gt 0) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        Write-Host "  ✅ Fixed $fileFixed issues in $($file.Name)" -ForegroundColor Green
        $totalFixed += $fileFixed
    }
}

Write-Host "🎯 Fixed $totalFixed FluentAssertions HaveLength issues total" -ForegroundColor Yellow

# Now let's build to see remaining issues
Write-Host "🏗️ Building UITests to check remaining issues..." -ForegroundColor Cyan
Set-Location $workspace
$buildResult = dotnet build BusBuddy.UITests\BusBuddy.UITests.csproj --verbosity minimal 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ UITests build successful!" -ForegroundColor Green
} else {
    Write-Host "❌ Still have build errors. Next phase needed." -ForegroundColor Red
    $buildResult | Select-String "error" | ForEach-Object {
        Write-Host "  $($_)" -ForegroundColor Yellow
    }
}
