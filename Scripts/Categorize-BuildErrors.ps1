# Bus Buddy Build Error Categorizer
# Automatically categorizes build errors into logical groups for easier fixing

param(
    [string]$BuildLogPath,
    [switch]$RunBuild,
    [switch]$ExportReport
)

function Get-BusBuddyBuildErrors {
    param([string]$LogPath)

    if ($RunBuild) {
        Write-Host "Running build to capture errors..." -ForegroundColor Yellow
        $buildOutput = dotnet build 2>&1
        $errors = $buildOutput | Where-Object { $_ -like "*error*" }
    } elseif ($LogPath -and (Test-Path $LogPath)) {
        $errors = Get-Content $LogPath | Where-Object { $_ -like "*error*" }
    } else {
        Write-Host "Running build to capture current errors..." -ForegroundColor Yellow
        $buildOutput = dotnet build 2>&1
        $errors = $buildOutput | Where-Object { $_ -like "*error*" }
    }

    # Categorize errors
    $categories = @{
        'Missing Properties' = @()
        'Namespace Issues' = @()
        'Constructor Problems' = @()
        'Repository Issues' = @()
        'Service Configuration' = @()
        'Syncfusion Control Issues' = @()
        'Designer File Issues' = @()
        'Other' = @()
    }

    foreach ($error in $errors) {
        switch -Regex ($error) {
            "does not contain a definition for" {
                $categories['Missing Properties'] += $error
            }
            "namespace|using directive" {
                $categories['Namespace Issues'] += $error
            }
            "constructor|parameter" {
                $categories['Constructor Problems'] += $error
            }
            "Repository|IRepository" {
                $categories['Repository Issues'] += $error
            }
            "Service|DI|dependency injection" {
                $categories['Service Configuration'] += $error
            }
            "Syncfusion|Control|Grid|Button" {
                $categories['Syncfusion Control Issues'] += $error
            }
            "Designer|InitializeComponent" {
                $categories['Designer File Issues'] += $error
            }
            default {
                $categories['Other'] += $error
            }
        }
    }

    return $categories
}

function Show-ErrorSummary {
    param($Categories)

    Write-Host "`nBUS BUDDY BUILD ERROR SUMMARY" -ForegroundColor Magenta
    Write-Host "==============================" -ForegroundColor Magenta

    $totalErrors = 0
    foreach ($category in $Categories.Keys) {
        $count = $Categories[$category].Count
        $totalErrors += $count

        if ($count -gt 0) {
            Write-Host "$category`: $count errors" -ForegroundColor $(
                switch ($count) {
                    {$_ -gt 50} { 'Red' }
                    {$_ -gt 20} { 'Yellow' }
                    default { 'Green' }
                }
            )
        }
    }

    Write-Host "`nTotal Errors: $totalErrors" -ForegroundColor Cyan

    # Show top priority fixes
    Write-Host "`nRECOMMENDED FIX ORDER:" -ForegroundColor Green
    $priorityOrder = @(
        'Designer File Issues',
        'Namespace Issues',
        'Missing Properties',
        'Repository Issues',
        'Constructor Problems',
        'Service Configuration',
        'Syncfusion Control Issues'
    )

    $priority = 1
    foreach ($category in $priorityOrder) {
        if ($Categories[$category].Count -gt 0) {
            Write-Host "$priority. Fix $category ($($Categories[$category].Count) errors)" -ForegroundColor White
            $priority++
        }
    }
}

# Run the categorization
Write-Host "Categorizing Bus Buddy build errors..." -ForegroundColor Green

$errorCategories = Get-BusBuddyBuildErrors -LogPath $BuildLogPath
Show-ErrorSummary -Categories $errorCategories

if ($ExportReport) {
    $reportPath = "BuildErrorReport_$(Get-Date -Format 'yyyyMMdd_HHmmss').txt"

    $report = @"
Bus Buddy Build Error Analysis Report
Generated: $(Get-Date)

SUMMARY:
$(foreach ($cat in $errorCategories.Keys) { if ($errorCategories[$cat].Count -gt 0) { "$cat`: $($errorCategories[$cat].Count) errors" } })

DETAILED ERRORS BY CATEGORY:
$(foreach ($cat in $errorCategories.Keys) {
    if ($errorCategories[$cat].Count -gt 0) {
        "`n=== $cat ===`n$($errorCategories[$cat] | Out-String)"
    }
})
"@

    $report | Out-File -FilePath $reportPath -Encoding UTF8
    Write-Host "`nDetailed report exported to: $reportPath" -ForegroundColor Green
}
