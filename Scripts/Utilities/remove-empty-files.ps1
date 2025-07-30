#Requires -Version 7.5

<#
.SYNOPSIS
Remove empty PowerShell files from BusBuddy project

.DESCRIPTION
Identifies and removes empty PowerShell script files to clean up the repository
#>

param([switch]$WhatIf) # Default is $false for switch parameters

$emptyFiles = @(
    'Tools\Scripts\BusBuddy-Terminal-Flow-Monitor.ps1',
    'switch-database-provider.ps1',
    'setup-localdb.ps1',
    'Scripts\temp_script.ps1',
    'Scripts\tavily-tool.ps1',
    'Scripts\register-tavily-commands.ps1',
    'Scripts\PowerShell-Error-Diagnostic.ps1',
    'Scripts\Phase2-Code-Quality-Fix.ps1',
    'Scripts\Maintenance\Self-Resolving-Build.ps1',
    'Scripts\Maintenance\Resource-Cleanup-Manager.ps1',
    'Scripts\Maintenance\Phase2-Performance-Optimizer.ps1',
    'Scripts\Maintenance\MSB3027-File-Lock-Solution.ps1',
    'Scripts\Maintenance\MSB3027-File-Lock-Solution-Fixed.ps1',
    'Scripts\Maintenance\Master-Issue-Resolution.ps1',
    'Scripts\Maintenance\Fix-File-Locks.ps1',
    'Scripts\Maintenance\Enhanced-Build-Workflow.ps1',
    'Scripts\Interactive-Runtime-Error-Capture.ps1',
    'Scripts\fix-tavily-scripts.ps1',
    'Scripts\Fix-PowerShell-Paging.ps1',
    'Scripts\Fix-GitHub-Workflows.ps1',
    'Scripts\Fix-GitHub-Workflows-UTF8.ps1',
    'Scripts\fix-file-encodings.ps1',
    'Scripts\Cross-Platform-Compatibility-Fix.ps1',
    'Scripts\BusBuddy-Terminal-Flow-Monitor.ps1',
    'Scripts\bb-help.ps1',
    'PowerShell\Load-BusBuddyModules.ps1'
)

Write-Host "🧹 Removing Empty PowerShell Files" -ForegroundColor Cyan
Write-Host "WhatIf Mode: $WhatIf" -ForegroundColor Yellow
Write-Host ""

$removedCount = 0
foreach ($file in $emptyFiles) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw -ErrorAction SilentlyContinue
        if ([string]::IsNullOrWhiteSpace($content)) {
            if ($WhatIf) {
                Write-Host "Would remove: $file" -ForegroundColor Yellow
            }
            else {
                Remove-Item $file -Force
                Write-Host "Removed: $file" -ForegroundColor Red
            }
            $removedCount++
        }
    }
}

Write-Host ""
if ($WhatIf) {
    Write-Host "Would remove $removedCount empty files" -ForegroundColor Yellow
    Write-Host "Run with -WhatIf:`$false to actually remove files" -ForegroundColor Green
}
else {
    Write-Host "Removed $removedCount empty files" -ForegroundColor Green
}

# cSpell:words localdb tavily
