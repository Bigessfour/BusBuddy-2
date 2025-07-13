# PowerShell script to analyze all StaticResource references
Write-Host "=== BusBuddy Static Resource Analysis ===" -ForegroundColor Cyan

# Get all XAML files
$xamlFiles = Get-ChildItem -Path "." -Recurse -Filter "*.xaml" | Where-Object { $_.Name -notlike "*bin*" -and $_.Name -notlike "*obj*" }

Write-Host "`nFound $($xamlFiles.Count) XAML files" -ForegroundColor Green

# Extract all StaticResource references
$allReferences = @()
$allDefinitions = @()

foreach ($file in $xamlFiles) {
    $content = Get-Content $file.FullName -Raw

    # Find StaticResource references
    $references = [regex]::Matches($content, '\{StaticResource\s+([^}]+)\}')
    foreach ($match in $references) {
        $resourceName = $match.Groups[1].Value.Trim()
        $allReferences += [PSCustomObject]@{
            File = $file.Name
            ResourceName = $resourceName
            Line = ($content.Substring(0, $match.Index) -split "`n").Count
        }
    }

    # Find resource definitions
    $definitions = [regex]::Matches($content, 'x:Key\s*=\s*"([^"]+)"')
    foreach ($match in $definitions) {
        $resourceName = $match.Groups[1].Value.Trim()
        $allDefinitions += [PSCustomObject]@{
            File = $file.Name
            ResourceName = $resourceName
            Line = ($content.Substring(0, $match.Index) -split "`n").Count
        }
    }
}

# Get unique resource names
$uniqueReferences = $allReferences | Select-Object ResourceName -Unique | Sort-Object ResourceName
$uniqueDefinitions = $allDefinitions | Select-Object ResourceName -Unique | Sort-Object ResourceName

Write-Host "`n=== SUMMARY ===" -ForegroundColor Yellow
Write-Host "Total StaticResource references: $($allReferences.Count)" -ForegroundColor White
Write-Host "Unique resources referenced: $($uniqueReferences.Count)" -ForegroundColor White
Write-Host "Unique resources defined: $($uniqueDefinitions.Count)" -ForegroundColor White

# Find missing resources
$missingResources = @()
foreach ($ref in $uniqueReferences) {
    $isDefined = $uniqueDefinitions | Where-Object { $_.ResourceName -eq $ref.ResourceName }
    if (-not $isDefined) {
        $missingResources += $ref.ResourceName
        $usageFiles = $allReferences | Where-Object { $_.ResourceName -eq $ref.ResourceName } | Select-Object File, Line
        Write-Host "`n❌ MISSING: $($ref.ResourceName)" -ForegroundColor Red
        foreach ($usage in $usageFiles) {
            Write-Host "   Used in: $($usage.File):$($usage.Line)" -ForegroundColor Gray
        }
    }
}

if ($missingResources.Count -eq 0) {
    Write-Host "`n✅ All StaticResource references have definitions!" -ForegroundColor Green
} else {
    Write-Host "`n⚠️  Found $($missingResources.Count) missing resource definitions" -ForegroundColor Red
    Write-Host "`nMissing Resources:" -ForegroundColor Yellow
    $missingResources | Sort-Object | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
}

# Show most commonly used resources
Write-Host "`n=== MOST USED RESOURCES ===" -ForegroundColor Yellow
$resourceUsage = $allReferences | Group-Object ResourceName | Sort-Object Count -Descending | Select-Object -First 10
foreach ($usage in $resourceUsage) {
    Write-Host "$($usage.Count)x - $($usage.Name)" -ForegroundColor Cyan
}
