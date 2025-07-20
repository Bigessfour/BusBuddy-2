# Syncfusion Namespace Validation Script for VS Code Task Integration
# This script is called by the "🔍 Full Project Validation Suite" task

param(
    [string]$ProjectPath = $PWD
)

Write-Host '🔍 SYNCFUSION VALIDATION: Checking XAML files for missing namespaces...' -ForegroundColor Cyan

# Import the comprehensive Syncfusion validator
$syncfusionValidatorPath = Join-Path $ProjectPath 'Tools\Scripts\Syncfusion-Implementation-Validator.ps1'
if (Test-Path $syncfusionValidatorPath) {
    . $syncfusionValidatorPath

    try {
        $analysis = Test-SyncfusionImplementation -ProjectPath $ProjectPath

        if ($analysis.Errors.Count -eq 0) {
            Write-Host '✅ All Syncfusion namespaces and implementations are valid!' -ForegroundColor Green
        } else {
            Write-Host "❌ Found $($analysis.Errors.Count) Syncfusion implementation errors:" -ForegroundColor Red
            $analysis.Errors | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
        }

        if ($analysis.Warnings.Count -gt 0) {
            Write-Host "⚠️ Found $($analysis.Warnings.Count) Syncfusion warnings:" -ForegroundColor Yellow
            $analysis.Warnings | ForEach-Object { Write-Host "  - $_" -ForegroundColor Yellow }
        }

        # Output summary for VS Code Problems panel
        $totalIssues = $analysis.Errors.Count + $analysis.Warnings.Count
        if ($totalIssues -eq 0) {
            Write-Host '🎉 Syncfusion implementation validation passed!' -ForegroundColor Green
        } else {
            Write-Host "📊 Syncfusion validation summary: $($analysis.Errors.Count) errors, $($analysis.Warnings.Count) warnings" -ForegroundColor Cyan
        }

    } catch {
        Write-Host "❌ Syncfusion validation failed with error: $_" -ForegroundColor Red
        exit 1
    }

} else {
    # Fallback to basic namespace check if comprehensive validator not available
    Write-Host '⚠️ Comprehensive validator not found, using basic namespace check...' -ForegroundColor Yellow

    $missingNamespaces = @()
    $xamlFiles = Get-ChildItem -Path "$ProjectPath\BusBuddy.WPF\Views" -Filter '*.xaml' -Recurse -ErrorAction SilentlyContinue

    if ($xamlFiles.Count -eq 0) {
        $xamlFiles = Get-ChildItem -Path $ProjectPath -Filter '*.xaml' -Recurse -ErrorAction SilentlyContinue
    }

    ForEach-Object ($file in $xamlFiles) {
        try {
            $content = Get-Content $file.FullName -Raw
            if ($content -notmatch 'xmlns:syncfusion=' -and ($content -match 'sf:' -or $content -match 'syncfusion:')) {
                $missingNamespaces += $file.Name
                Write-Host "❌ Missing Syncfusion namespace: $($file.Name)" -ForegroundColor Red
            } else {
                Write-Host "✅ Valid: $($file.Name)" -ForegroundColor Green
            }
        } catch {
            Write-Host "⚠️ Could not analyze $($file.Name): $_" -ForegroundColor Yellow
        }
    }

    if ($missingNamespaces.Count -eq 0) {
        Write-Host '🎉 All XAML files have proper Syncfusion namespaces!' -ForegroundColor Green
    } else {
        Write-Host "⚠️ Found $($missingNamespaces.Count) files with missing Syncfusion namespaces" -ForegroundColor Yellow
        exit 1
    }
}

exit 0
