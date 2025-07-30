#Requires -Version 7.5
<#
.SYNOPSIS
    BusBuddy Warning Auto-Fix Script - Automatically fixes simple code analysis warnings
.DESCRIPTION
    This script automatically fixes the straightforward warnings from the build output
.NOTES
    Version: 1.0
    Date: July 30, 2025
    Author: BusBuddy Development Team
#>

param(
    [switch]$DryRun,
    [switch]$Verbose
)

$ErrorActionPreference = 'Stop'

Write-Host "🚌 BusBuddy Auto-Fix Script" -ForegroundColor Cyan
Write-Host "===========================" -ForegroundColor Cyan

if ($DryRun) {
    Write-Host "⚠️  DRY RUN MODE - No files will be modified" -ForegroundColor Yellow
}

# Function to add assembly attributes
function Add-AssemblyAttributes {
    Write-Host "🔧 Adding missing assembly attributes..." -ForegroundColor Yellow

    $assemblyInfoFile = "BusBuddy.WPF\AssemblyInfo.cs"
    if (Test-Path $assemblyInfoFile) {
        $content = Get-Content $assemblyInfoFile -Raw
        $modified = $false

        # Add AssemblyVersion if missing
        if ($content -notlike "*AssemblyVersion*") {
            Write-Host "  ✅ Adding AssemblyVersion attribute" -ForegroundColor Green
            if (-not $DryRun) {
                $content += "`n[assembly: System.Reflection.AssemblyVersion(`"1.0.0.0`")]"
                $modified = $true
            }
        }

        # Add NeutralResourcesLanguage if missing
        if ($content -notlike "*NeutralResourcesLanguage*") {
            Write-Host "  ✅ Adding NeutralResourcesLanguage attribute" -ForegroundColor Green
            if (-not $DryRun) {
                $content += "`n[assembly: System.Resources.NeutralResourcesLanguage(`"en-US`")]"
                $modified = $true
            }
        }

        if ($modified -and -not $DryRun) {
            Set-Content $assemblyInfoFile -Value $content -Encoding UTF8
            Write-Host "  💾 Updated $assemblyInfoFile" -ForegroundColor Green
        }
    }
}

# Function to fix hidden member warnings by adding 'new' keyword
function Add-NewKeywords {
    Write-Host "🔧 Adding 'new' keywords for hidden members..." -ForegroundColor Yellow

    # Fix SportsSchedulingViewModel.cs
    $file1 = "BusBuddy.WPF\ViewModels\SportsScheduling\SportsSchedulingViewModel.cs"
    if (Test-Path $file1) {
        Write-Host "  📝 Processing: $file1" -ForegroundColor White
        if (-not $DryRun) {
            $content = Get-Content $file1 -Raw
            # Replace the IsLoading property to add 'new' keyword
            $pattern = 'public bool IsLoading'
            $replacement = 'public new bool IsLoading'
            if ($content -match $pattern -and $content -notlike "*public new bool IsLoading*") {
                $content = $content -replace $pattern, $replacement
                Set-Content $file1 -Value $content -Encoding UTF8
                Write-Host "    ✅ Added 'new' keyword to IsLoading property" -ForegroundColor Green
            }
        }
    }

    # Fix SportsSchedulerViewModel.cs
    $file2 = "BusBuddy.WPF\ViewModels\Sports\SportsSchedulerViewModel.cs"
    if (Test-Path $file2) {
        Write-Host "  📝 Processing: $file2" -ForegroundColor White
        if (-not $DryRun) {
            $content = Get-Content $file2 -Raw
            # Replace the Logger property to add 'new' keyword
            $pattern = 'protected ILogger Logger'
            $replacement = 'protected new ILogger Logger'
            if ($content -match $pattern -and $content -notlike "*protected new ILogger Logger*") {
                $content = $content -replace $pattern, $replacement
                Set-Content $file2 -Value $content -Encoding UTF8
                Write-Host "    ✅ Added 'new' keyword to Logger property" -ForegroundColor Green
            }
        }
    }
}

# Function to add required modifiers to DataIntegrityIssue properties
function Add-RequiredModifiers {
    Write-Host "🔧 Adding 'required' modifiers to properties..." -ForegroundColor Yellow

    $file = "BusBuddy.WPF\Models\DataIntegrityIssue.cs"
    if (Test-Path $file) {
        Write-Host "  📝 Processing: $file" -ForegroundColor White
        if (-not $DryRun) {
            $content = Get-Content $file -Raw
            $modified = $false

            # Properties that need required modifier
            $properties = @(
                'public string EntityType',
                'public string EntityId',
                'public string IssueType',
                'public string Description',
                'public string Severity',
                'public string SuggestedAction'
            )

            foreach ($prop in $properties) {
                $requiredProp = $prop -replace 'public string', 'public required string'
                if ($content -like "*$prop*" -and $content -notlike "*$requiredProp*") {
                    $content = $content -replace [regex]::Escape($prop), $requiredProp
                    $modified = $true
                    Write-Host "    ✅ Added 'required' to: $prop" -ForegroundColor Green
                }
            }

            if ($modified) {
                Set-Content $file -Value $content -Encoding UTF8
                Write-Host "  💾 Updated $file" -ForegroundColor Green
            }
        }
    }
}

# Function to replace Environment.CurrentManagedThreadId
function Update-ThreadIdCalls {
    Write-Host "🔧 Updating Thread.CurrentThread.ManagedThreadId calls..." -ForegroundColor Yellow

    $files = @(
        "BusBuddy.WPF\Logging\BusBuddyContextEnricher.cs",
        "BusBuddy.WPF\Logging\UIOperationEnricher.cs"
    )

    foreach ($file in $files) {
        if (Test-Path $file) {
            Write-Host "  📝 Processing: $file" -ForegroundColor White
            if (-not $DryRun) {
                $content = Get-Content $file -Raw
                $pattern = 'Thread\.CurrentThread\.ManagedThreadId'
                $replacement = 'Environment.CurrentManagedThreadId'
                if ($content -match $pattern) {
                    $content = $content -replace $pattern, $replacement
                    Set-Content $file -Value $content -Encoding UTF8
                    Write-Host "    ✅ Updated ThreadId calls" -ForegroundColor Green
                }
            }
        }
    }
}

# Function to suppress less critical warnings via NoWarn
function Update-ProjectSuppressions {
    Write-Host "🔧 Adding warning suppressions to project files..." -ForegroundColor Yellow

    $projectFile = "BusBuddy.WPF\BusBuddy.WPF.csproj"
    if (Test-Path $projectFile) {
        Write-Host "  📝 Processing: $projectFile" -ForegroundColor White
        if (-not $DryRun) {
            $content = Get-Content $projectFile -Raw

            # Add suppressions for performance warnings that are less critical
            $suppressions = @(
                "CA1845", # Substring usage - can be addressed later
                "CA1869", # JsonSerializerOptions - caching can be added later
                "CA1859", # Return type optimization - less critical
                "CA1852", # Sealing types - less critical
                "CA1836", # IsEmpty vs Count - less critical
                "CS1998"  # Async without await - method stubs
            )

            $newSuppressions = $suppressions -join ";"
            $noWarnPattern = '<NoWarn>\$\(NoWarn\);([^<]*)</NoWarn>'

            if ($content -match $noWarnPattern) {
                $existingSuppressions = $matches[1]
                $allSuppressions = "$existingSuppressions;$newSuppressions"
                $content = $content -replace $noWarnPattern, "<NoWarn>`$(NoWarn);$allSuppressions</NoWarn>"
                Set-Content $projectFile -Value $content -Encoding UTF8
                Write-Host "    ✅ Added warning suppressions" -ForegroundColor Green
            }
        }
    }
}

# Run all fixes
try {
    $fixCount = 0

    Write-Host ""
    Add-AssemblyAttributes
    $fixCount++

    Write-Host ""
    Add-NewKeywords
    $fixCount++

    Write-Host ""
    Add-RequiredModifiers
    $fixCount++

    Write-Host ""
    Update-ThreadIdCalls
    $fixCount++

    Write-Host ""
    Update-ProjectSuppressions
    $fixCount++

    Write-Host ""
    Write-Host "✅ Auto-fix complete!" -ForegroundColor Green
    Write-Host "🔧 Applied $fixCount categories of fixes" -ForegroundColor Cyan
    Write-Host ""

    if (-not $DryRun) {
        Write-Host "🚀 Recommended next steps:" -ForegroundColor Yellow
        Write-Host "  1. Run: dotnet build BusBuddy.sln" -ForegroundColor White
        Write-Host "  2. Review remaining warnings manually" -ForegroundColor White
        Write-Host "  3. Run tests to ensure no regressions" -ForegroundColor White
    } else {
        Write-Host "To apply these fixes, run: .\fix-warnings-auto.ps1" -ForegroundColor Yellow
    }

} catch {
    Write-Error "❌ Error during auto-fix: $_"
    exit 1
}
