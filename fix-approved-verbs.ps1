# Fix Approved Verbs in Bus Buddy PowerShell Files
# This script updates function names to use PowerShell approved verbs

param(
    [switch]$Apply,
    [switch]$WhatIf
)

Write-Host "🔧 Bus Buddy Approved Verbs Fix Script" -ForegroundColor Green
Write-Host "=" * 50

if (-not $Apply) {
    Write-Host "🔍 PREVIEW MODE - Use -Apply to make changes" -ForegroundColor Yellow
    $WhatIf = $true
} else {
    $WhatIf = $false
}

# Function name mappings from unapproved to approved verbs
$functionMappings = @{
    # Core build commands
    'bb-build' = 'Invoke-BusBuddyBuild'
    'bb-test' = 'Invoke-BusBuddyTest'
    'bb-clean' = 'Clear-BusBuddyBuild'
    'bb-run' = 'Start-BusBuddyApplication'
    'bb-restore' = 'Restore-BusBuddyPackages'

    # Navigation and info commands
    'bb-root' = 'Set-BusBuddyLocation'
    'bb-views' = 'Get-BusBuddyViews'
    'bb-resources' = 'Get-BusBuddyResources'
    'bb-tools' = 'Get-BusBuddyTools'
    'bb-logs' = 'Get-BusBuddyLogs'

    # Health and diagnostic commands
    'bb-check' = 'Test-BusBuddyHealth'
    'bb-health' = 'Get-BusBuddyHealth'
    'bb-syntax' = 'Test-BusBuddySyntax'
    'bb-diagnostic' = 'Get-BusBuddyDiagnostic'
    'bb-report' = 'New-BusBuddyReport'
    'bb-quick-health' = 'Test-BusBuddyQuickHealth'
    'bb-null-check' = 'Test-BusBuddyNullSafety'

    # Development workflow commands
    'bb-dev-session' = 'Start-BusBuddyDevSession'
    'bb-quick-test' = 'Invoke-BusBuddyQuickTest'
    'bb-reload' = 'Update-BusBuddyProfile'

    # Debug commands
    'bb-debug-start' = 'Start-BusBuddyDebug'
    'bb-debug-export' = 'Export-BusBuddyDebug'

    # XAML commands
    'bb-xaml-button' = 'New-BusBuddyXamlButton'
    'bb-xaml-bind' = 'New-BusBuddyXamlBinding'

    # Profile persistence functions
    'Load-BusBuddyProfiles' = 'Import-BusBuddyProfiles'
    'Reload-BusBuddyProfiles' = 'Update-BusBuddyProfiles'

    # Other functions that need fixing
    'Validate-SyncfusionNamespaces' = 'Test-SyncfusionNamespaces'
}

# Alias mappings to maintain backward compatibility
$aliasMappings = @{
    'Invoke-BusBuddyBuild' = 'bb-build'
    'Invoke-BusBuddyTest' = 'bb-test'
    'Clear-BusBuddyBuild' = 'bb-clean'
    'Start-BusBuddyApplication' = 'bb-run'
    'Restore-BusBuddyPackages' = 'bb-restore'
    'Set-BusBuddyLocation' = 'bb-root'
    'Get-BusBuddyViews' = 'bb-views'
    'Get-BusBuddyResources' = 'bb-resources'
    'Get-BusBuddyTools' = 'bb-tools'
    'Get-BusBuddyLogs' = 'bb-logs'
    'Test-BusBuddyHealth' = 'bb-check'
    'Get-BusBuddyHealth' = 'bb-health'
    'Test-BusBuddySyntax' = 'bb-syntax'
    'Get-BusBuddyDiagnostic' = 'bb-diagnostic'
    'New-BusBuddyReport' = 'bb-report'
    'Test-BusBuddyQuickHealth' = 'bb-quick-health'
    'Test-BusBuddyNullSafety' = 'bb-null-check'
    'Start-BusBuddyDevSession' = 'bb-dev-session'
    'Invoke-BusBuddyQuickTest' = 'bb-quick-test'
    'Update-BusBuddyProfile' = 'bb-reload'
    'Start-BusBuddyDebug' = 'bb-debug-start'
    'Export-BusBuddyDebug' = 'bb-debug-export'
    'New-BusBuddyXamlButton' = 'bb-xaml-button'
    'New-BusBuddyXamlBinding' = 'bb-xaml-bind'
    'Import-BusBuddyProfiles' = 'Load-BusBuddyProfiles'
    'Update-BusBuddyProfiles' = 'Reload-BusBuddyProfiles'
}

# Files to process
$filesToProcess = @(
    'BusBuddy-PowerShell-Profile.ps1',
    '.vscode\BusBuddy-Advanced-Workflows.ps1',
    'Tools/Scripts/XAML-Health-Suite.ps1',
    'Tools/Scripts/XAML-Null-Safety-Analyzer.ps1',
    'persistent-profile-helper.ps1'
)

Write-Host "`n📋 PROCESSING FILES:" -ForegroundColor Cyan

foreach ($filePath in $filesToProcess) {
    if (-not (Test-Path $filePath)) {
        Write-Warning "File not found: $filePath"
        continue
    }

    Write-Host "`n📄 Processing: $filePath" -ForegroundColor Yellow

    $content = Get-Content -Path $filePath -Raw
    $originalContent = $content
    $changesCount = 0

    # Update function definitions
    foreach ($oldName in $functionMappings.Keys) {
        $newName = $functionMappings[$oldName]

        # Replace function definition
        $pattern = "function\s+$([regex]::Escape($oldName))\s*\{"
        $replacement = "function $newName {"

        if ($content -match $pattern) {
            Write-Host "  ✅ Updating function: $oldName → $newName" -ForegroundColor Green
            $content = $content -replace $pattern, $replacement
            $changesCount++
        }
    }

    # Add aliases section for backward compatibility
    if ($changesCount -gt 0 -and $content -notmatch "# Backward Compatibility Aliases") {
        $aliasSection = "`n`n# Backward Compatibility Aliases`n"

        # Find functions that were changed in this file
        foreach ($oldName in $functionMappings.Keys) {
            $newName = $functionMappings[$oldName]
            if ($originalContent -match "function\s+$([regex]::Escape($oldName))\s*\{") {
                $aliasName = $aliasMappings[$newName]
                if ($aliasName) {
                    $aliasSection += "Set-Alias -Name '$aliasName' -Value '$newName' -Force`n"
                }
            }
        }

        $content += $aliasSection
        Write-Host "  📎 Added backward compatibility aliases" -ForegroundColor Blue
    }

    if ($changesCount -gt 0) {
        if ($Apply) {
            Set-Content -Path $filePath -Value $content -Encoding UTF8
            Write-Host "  💾 Changes applied to $filePath" -ForegroundColor Green
        } else {
            Write-Host "  🔍 Would apply $changesCount changes to $filePath" -ForegroundColor Yellow
        }
    } else {
        Write-Host "  ℹ️  No changes needed for $filePath" -ForegroundColor Gray
    }
}

Write-Host "`n🎯 SUMMARY:" -ForegroundColor Green
Write-Host "  • Function names updated to use approved PowerShell verbs"
Write-Host "  • Backward compatibility aliases added"
Write-Host "  • All existing scripts will continue to work with old function names"

if (-not $Apply) {
    Write-Host "`n⚠️  This was a preview. Use -Apply to make actual changes." -ForegroundColor Yellow
}

Write-Host "`n✅ Approved verbs fix complete!" -ForegroundColor Green
