# Fix Write-Output - Host usage in BusBuddy - Advanced - Workflows.ps1

$filePath = "c:\Users\steve.mckitrick\Desktop\Bus Buddy\.vscode\BusBuddy - Advanced - Workflows.ps1"
$content = Get - Content $filePath -Raw

Write-Output - Host "Processing file: $filePath" -ForegroundColor Cyan

# Replace various Write-Output - Host patterns with Write-Output - BusBuddyWorkflowMessage equivalents
$replacements = @(
    @{ Pattern = 'Write-Output - Host "❌ ([^"]+)" -ForegroundColor Red'; Replacement = 'Write-Output - BusBuddyWorkflowMessage -Message "$1" -Type Error' },
    @{ Pattern = 'Write-Output - Host "✅ ([^"]+)" -ForegroundColor Green'; Replacement = 'Write-Output - BusBuddyWorkflowMessage -Message "$1" -Type Success' },
    @{ Pattern = 'Write-Output - Host "🔄 ([^"]+)" -ForegroundColor Cyan'; Replacement = 'Write-Output - BusBuddyWorkflowMessage -Message "$1" -Type Info' },
    @{ Pattern = 'Write-Output - Host "⚠️([^"]+)" -ForegroundColor Yellow'; Replacement = 'Write-Output - BusBuddyWorkflowMessage -Message "$1" -Type Warning' },
    @{ Pattern = 'Write-Output - Host "💡 ([^"]+)" -ForegroundColor Yellow'; Replacement = 'Write-Output - BusBuddyWorkflowMessage -Message "$1" -Type Warning' },
    @{ Pattern = 'Write-Output - Host "💡 ([^"]+)" -ForegroundColor Gray'; Replacement = 'Write-Output - BusBuddyWorkflowMessage -Message "$1" -Type Verbose' },
    @{ Pattern = 'Write-Output - Host "🔍 ([^"]+)" -ForegroundColor Cyan'; Replacement = 'Write-Output - BusBuddyWorkflowMessage -Message "$1" -Type Debug' },
    @{ Pattern = 'Write-Output - Host "🚀 ([^"]+)" -ForegroundColor Cyan'; Replacement = 'Write-Output - BusBuddyWorkflowMessage -Message "$1" -Type Info' },
    @{ Pattern = 'Write-Output - Host "📊 ([^"]+)" -ForegroundColor ([A - Za - z]+)'; Replacement = 'Write-Output - BusBuddyWorkflowMessage -Message "$1" -Type Info' },
    @{ Pattern = 'Write-Output - Host "`n🔍 ([^"]+)" -ForegroundColor Yellow'; Replacement = 'Write-Output - BusBuddyWorkflowMessage -Message "`n$1" -Type Warning' },
    @{ Pattern = 'Write-Output - Host "  🔴 ([^"]+)" -ForegroundColor Red'; Replacement = 'Write-Output - BusBuddyWorkflowMessage -Message "  $1" -Type Error' },
    @{ Pattern = '\$compileErrors \| ForEach-Object - Object \{ Write-Output - Host "  • \$_" -ForegroundColor Red \}'; Replacement = '$compileErrors | ForEach-Object - Object { Write-Output - BusBuddyWorkflowMessage -Message "  • $_" -Type Error }' }
)

$changeCount = 0
ForEach-Object ($replacement in $replacements) {
    $oldContent = $content
    $content = $content -replace $replacement.Pattern, $replacement.Replacement
    if ($content -ne $oldContent) {
        $matches = [regex]::Matches($oldContent, $replacement.Pattern)
        $changeCount += $matches.Count
        Write-Output - Host "  Replaced $($matches.Count) instances of pattern: $($replacement.Pattern.Substring(0, [Math]::Min(50, $replacement.Pattern.Length)))" -ForegroundColor Green
    }
}

# Handle some special cases manually
$specialReplacements = @(
    @{ Old = 'Write-Output - Host ''🚀 Building Bus Buddy solution...'' -ForegroundColor Cyan'; New = 'Write-Output - BusBuddyWorkflowMessage -Message "Building Bus Buddy solution..." -Type Info' },
    @{ Old = 'Write-Output - Host ''❌ Bus Buddy project not found. Navigate to project directory first.'' -ForegroundColor Red'; New = 'Write-Output - BusBuddyWorkflowMessage -Message "Bus Buddy project not found. Navigate to project directory first." -Type Error' },
    @{ Old = 'Write-Output - Host ''💡 Ensure you are in a directory containing BusBuddy.sln or a subdirectory'' -ForegroundColor Yellow'; New = 'Write-Output - BusBuddyWorkflowMessage -Message "Ensure you are in a directory containing BusBuddy.sln or a subdirectory" -Type Warning' }
)

ForEach-Object ($special in $specialReplacements) {
    if ($content.Contains($special.Old)) {
        $content = $content.Replace($special.Old, $special.New)
        $changeCount++
        Write-Output - Host "  Fixed special case: $($special.Old.Substring(0, [Math]::Min(50, $special.Old.Length)))" -ForegroundColor Green
    }
}

# Save the updated content
$content | Set - Content $filePath -Encoding UTF8
Write-Output - Host "Completed! Made $changeCount replacements in BusBuddy - Advanced - Workflows.ps1" -ForegroundColor Green
