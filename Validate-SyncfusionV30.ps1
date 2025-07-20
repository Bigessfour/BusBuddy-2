# Syncfusion v30.1.40 Property Validation Script
# This script validates all Syncfusion control properties for v30.1.40 compatibility

param(
    [switch]$Fix,
    [switch]$Report,
    [string]$OutputPath = ".\syncfusion-validation-report.json"
)

# Known unsupported properties in v30.1.40
$UnsupportedProperties = @{
    'SfLinearProgressBar' = @('CornerRadius')
    'DockingManager'      = @('BorderBrush')
    'SfNavigationDrawer'  = @('BorderBrush', 'BorderThickness')
    'ButtonAdv'           = @('CornerRadius')  # v30.1.40 changed corner radius handling
    'SfBusyIndicator'     = @('CornerRadius')
}

# Property replacements for v30.1.40
$PropertyReplacements = @{
    'SfLinearProgressBar.CornerRadius' = 'Remove - Use container with CornerRadius instead'
    'DockingManager.BorderBrush'       = 'Remove - Use container Border control instead'
    'SfNavigationDrawer.BorderBrush'   = 'Remove - Use container Border control instead'
    'ButtonAdv.CornerRadius'           = 'Remove - Use Style Template override instead'
}

function Test-SyncfusionPropertyCompatibility {
    param(
        [string]$ProjectPath,
        [switch]$FixIssues
    )

    Write-Host "🔍 Scanning Syncfusion v30.1.40 property compatibility..." -ForegroundColor Cyan

    $xamlFiles = Get-ChildItem -Path $ProjectPath -Filter "*.xaml" -Recurse
    $issues = @()
    $fixedIssues = @()

    foreach ($file in $xamlFiles) {
        Write-Host "📄 Checking: $($file.Name)" -ForegroundColor Yellow

        $content = Get-Content -Path $file.FullName -Raw
        $lineNumber = 0

        $lines = Get-Content -Path $file.FullName

        foreach ($line in $lines) {
            $lineNumber++

            foreach ($controlType in $UnsupportedProperties.Keys) {
                foreach ($property in $UnsupportedProperties[$controlType]) {
                    if ($line -match "$property\s*=" -and $line -match $controlType) {
                        $issue = @{
                            File        = $file.FullName
                            Line        = $lineNumber
                            ControlType = $controlType
                            Property    = $property
                            Content     = $line.Trim()
                            Replacement = $PropertyReplacements["$controlType.$property"]
                        }
                        $issues += $issue

                        Write-Host "  ❌ Line $lineNumber`: Unsupported property '$property' in $controlType" -ForegroundColor Red
                        Write-Host "     Content: $($line.Trim())" -ForegroundColor Gray
                        Write-Host "     Solution: $($PropertyReplacements["$controlType.$property"])" -ForegroundColor Green

                        if ($FixIssues) {
                            # Remove the problematic property line
                            $newContent = $content -replace [regex]::Escape($line), "<!-- REMOVED: $($line.Trim()) - Not supported in v30.1.40 -->"
                            Set-Content -Path $file.FullName -Value $newContent
                            $fixedIssues += $issue
                            Write-Host "     ✅ FIXED: Property removed and commented" -ForegroundColor Green
                        }
                    }
                }
            }
        }
    }

    return @{
        Issues      = $issues
        FixedIssues = $fixedIssues
        TotalFiles  = $xamlFiles.Count
    }
}

function Get-SyncfusionControlUsage {
    param([string]$ProjectPath)

    Write-Host "📊 Analyzing Syncfusion control usage..." -ForegroundColor Cyan

    $xamlFiles = Get-ChildItem -Path $ProjectPath -Filter "*.xaml" -Recurse
    $controlUsage = @{}

    foreach ($file in $xamlFiles) {
        $content = Get-Content -Path $file.FullName -Raw

        # Find all syncfusion controls
        $matches = [regex]::Matches($content, 'syncfusion:([A-Za-z]+)')

        foreach ($match in $matches) {
            $controlName = $match.Groups[1].Value

            if (-not $controlUsage.ContainsKey($controlName)) {
                $controlUsage[$controlName] = @{
                    Count      = 0
                    Files      = @()
                    Properties = @()
                }
            }

            $controlUsage[$controlName].Count++
            if ($controlUsage[$controlName].Files -notcontains $file.Name) {
                $controlUsage[$controlName].Files += $file.Name
            }
        }
    }

    return $controlUsage
}

function Export-ValidationReport {
    param(
        [hashtable]$ValidationResult,
        [hashtable]$ControlUsage,
        [string]$OutputPath
    )

    $report = @{
        Timestamp         = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        SyncfusionVersion = "30.1.40"
        Summary           = @{
            TotalFiles    = $ValidationResult.TotalFiles
            TotalIssues   = $ValidationResult.Issues.Count
            FixedIssues   = $ValidationResult.FixedIssues.Count
            ControlsFound = $ControlUsage.Keys.Count
        }
        Issues            = $ValidationResult.Issues
        FixedIssues       = $ValidationResult.FixedIssues
        ControlUsage      = $ControlUsage
        Recommendations   = @(
            "Update resource dictionary to use only supported v30.1.40 properties",
            "Replace unsupported BorderBrush with container Border controls",
            "Remove CornerRadius from SfLinearProgressBar and ButtonAdv",
            "Use SfSkinManager for automatic theme application",
            "Test all controls after property updates"
        )
    }

    $jsonReport = $report | ConvertTo-Json -Depth 10
    Set-Content -Path $OutputPath -Value $jsonReport

    Write-Host "📋 Validation report exported to: $OutputPath" -ForegroundColor Green
}

# Main execution
$projectPath = Get-Location
$validationResult = Test-SyncfusionPropertyCompatibility -ProjectPath $projectPath -FixIssues:$Fix
$controlUsage = Get-SyncfusionControlUsage -ProjectPath $projectPath

Write-Host "`n📊 VALIDATION SUMMARY" -ForegroundColor Cyan
Write-Host "═══════════════════════" -ForegroundColor Cyan
Write-Host "Total XAML files scanned: $($validationResult.TotalFiles)" -ForegroundColor White
Write-Host "Syncfusion controls found: $($controlUsage.Keys.Count)" -ForegroundColor White
Write-Host "Compatibility issues: $($validationResult.Issues.Count)" -ForegroundColor $(if ($validationResult.Issues.Count -gt 0) { 'Red' } else { 'Green' })

if ($Fix) {
    Write-Host "Issues fixed: $($validationResult.FixedIssues.Count)" -ForegroundColor Green
}

Write-Host "`n🎛️  CONTROLS FOUND:" -ForegroundColor Yellow
foreach ($control in $controlUsage.Keys | Sort-Object) {
    $usage = $controlUsage[$control]
    $status = if ($UnsupportedProperties.ContainsKey($control)) { "⚠️  HAS KNOWN ISSUES" } else { "✅ OK" }
    Write-Host "  $control`: $($usage.Count) uses in $($usage.Files.Count) files - $status" -ForegroundColor White
}

if ($Report) {
    Export-ValidationReport -ValidationResult $validationResult -ControlUsage $controlUsage -OutputPath $OutputPath
}

if ($validationResult.Issues.Count -gt 0 -and -not $Fix) {
    Write-Host "`n💡 Run with -Fix parameter to automatically fix issues" -ForegroundColor Yellow
    Write-Host "💡 Run with -Report parameter to export detailed JSON report" -ForegroundColor Yellow
}

Write-Host "`n🎯 NEXT STEPS:" -ForegroundColor Cyan
Write-Host "1. Replace current resource dictionary with validated version" -ForegroundColor White
Write-Host "2. Update App.xaml to reference new dictionary" -ForegroundColor White
Write-Host "3. Build and test application" -ForegroundColor White
Write-Host "4. Verify all Syncfusion controls render correctly" -ForegroundColor White
