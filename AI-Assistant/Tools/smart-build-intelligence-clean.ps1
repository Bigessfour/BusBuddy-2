# ========================================
# BusBuddy Smart Build Intelligence Script - Phase 1 Enhanced
# ========================================

$ErrorActionPreference = 'Continue'
$startTime = Get-Date

Write-Host '🚀 BusBuddy Build Intelligence Report - Phase 1 Focus' -ForegroundColor Cyan
Write-Host ('=' * 70) -ForegroundColor Gray
Write-Host ('📅 Build Started: ' + $startTime.ToString('yyyy-MM-dd HH:mm:ss')) -ForegroundColor Yellow
Write-Host ''

# Phase 1 Focus Areas Check
Write-Host '🎯 Phase 1 Core Components:' -ForegroundColor Magenta
$phase1Components = @(
    @{ Name = "Database Models"; Path = "BusBuddy.Core\Models"; Check = "Driver.cs,Vehicle.cs,Activity.cs" },
    @{ Name = "EF DbContext"; Path = "BusBuddy.Core\Data"; Check = "BusBuddyDbContext.cs" },
    @{ Name = "Core Services"; Path = "BusBuddy.Core\Services"; Check = "DriverService.cs,VehicleService.cs,ActivityService.cs" },
    @{ Name = "WPF ViewModels"; Path = "BusBuddy.WPF\ViewModels"; Check = "MainViewModel.cs,DashboardViewModel.cs" },
    @{ Name = "WPF Views"; Path = "BusBuddy.WPF\Views"; Check = "MainWindow.xaml,DashboardView.xaml" }
)

foreach ($component in $phase1Components) {
    $basePath = $component.Path
    if (Test-Path $basePath) {
        $checkFiles = $component.Check -split ','
        $foundFiles = 0
        foreach ($file in $checkFiles) {
            if (Test-Path (Join-Path $basePath $file)) { $foundFiles++ }
        }
        $status = if ($foundFiles -eq $checkFiles.Count) { "✅" } else { "⚠️ " }
        Write-Host ("  $status " + $component.Name + " ($foundFiles/$($checkFiles.Count) files)") -ForegroundColor $(if ($foundFiles -eq $checkFiles.Count) { 'Green' } else { 'Yellow' })
    } else {
        Write-Host ("  ❌ " + $component.Name + " (Path missing)") -ForegroundColor Red
    }
}

# Pre-Build Analysis
Write-Host '🔍 Pre-Build Analysis:' -ForegroundColor Green
$projects = @('BusBuddy.Core', 'BusBuddy.WPF', 'BusBuddy.Tests')
$projects | ForEach-Object {
    $path = $_ + '\' + $_ + '.csproj'
    if (Test-Path $path) {
        Write-Host ('  ✅ ' + $_) -ForegroundColor Green
    }
    else {
        Write-Host ('  ❌ ' + $_ + ' (Missing)') -ForegroundColor Red
    }
}

Write-Host ''
Write-Host '🔨 Building Solution...' -ForegroundColor Cyan

# Execute build and capture output
$buildOutput = dotnet build BusBuddy.sln --verbosity normal --nologo 2>&1
$buildExitCode = $LASTEXITCODE
$endTime = Get-Date
$duration = $endTime - $startTime

# Post-Build EF Core Migration Check
Write-Host ''
Write-Host '🗄️  EF Core Database Status:' -ForegroundColor Cyan
try {
    $migrationsList = dotnet ef migrations list --project BusBuddy.Core --startup-project BusBuddy.WPF --no-build 2>&1
    if ($LASTEXITCODE -eq 0) {
        $migrations = $migrationsList | Where-Object { $_ -match '^\s*\d+' }
        Write-Host ("  📋 Migrations: " + $migrations.Count + " found") -ForegroundColor Green
        if ($migrations.Count -gt 0) {
            Write-Host ("  🔄 Latest: " + ($migrations | Select-Object -Last 1).Trim()) -ForegroundColor Yellow
        }

        # Check if database needs update
        $pendingMigrations = dotnet ef migrations has-pending-model-changes --project BusBuddy.Core --startup-project BusBuddy.WPF --no-build 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✅ Database schema up to date" -ForegroundColor Green
        } else {
            Write-Host "  ⚠️  Pending model changes detected" -ForegroundColor Yellow
            Write-Host "  💡 Run: dotnet ef migrations add <MigrationName>" -ForegroundColor Cyan
        }
    } else {
        Write-Host "  ❌ EF Core tools not available or project issues" -ForegroundColor Red
        Write-Host "  💡 Install: dotnet tool install --global dotnet-ef" -ForegroundColor Cyan
    }
} catch {
    Write-Host "  ⚠️  EF Core check failed: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host ''
Write-Host '📊 Build Results:' -ForegroundColor Magenta

if ($buildExitCode -eq 0) {
    Write-Host '  🎉 BUILD SUCCESSFUL' -ForegroundColor Green
}
else {
    Write-Host '  💥 BUILD FAILED' -ForegroundColor Red
}

Write-Host ('  ⏱️  Duration: ' + $duration.TotalSeconds.ToString('F1') + 's') -ForegroundColor Yellow

# Count warnings and errors
$warnings = ($buildOutput | Select-String 'warning').Count
$errors = ($buildOutput | Select-String 'error').Count

Write-Host ('  ⚠️  Warnings: ' + $warnings) -ForegroundColor $(if ($warnings -eq 0) { 'Green' } else { 'Yellow' })
Write-Host ('  🚨 Errors: ' + $errors) -ForegroundColor $(if ($errors -eq 0) { 'Green' } else { 'Red' })

Write-Host ''

# Show key warnings if any
if ($warnings -gt 0) {
    Write-Host '⚠️  Key Warnings (Phase 1 Focus):' -ForegroundColor Yellow

    # Prioritize Phase 1 related warnings
    $phase1Warnings = $buildOutput | Select-String 'warning' | Where-Object {
        $_.Line -match 'DbContext|Driver|Vehicle|Activity|MainWindow|Dashboard'
    }

    if ($phase1Warnings.Count -gt 0) {
        Write-Host '  🎯 Phase 1 Component Warnings:' -ForegroundColor Red
        $phase1Warnings | Select-Object -First 3 | ForEach-Object {
            $line = $_.Line
            if ($line -match 'CA\d+') {
                $codeMatch = $matches[0]
                Write-Host ('    📝 ' + $codeMatch + ' in Phase 1 code') -ForegroundColor Red
            }
        }
    }

    # Show other warnings
    $buildOutput | Select-String 'warning' | Select-Object -First 3 | ForEach-Object {
        $line = $_.Line
        if ($line -match 'CA2000.*DbContext') {
            Write-Host ('  🗄️  Database: CA2000 - Dispose DbContext properly') -ForegroundColor Yellow
        }
        elseif ($line -match 'CA\d+') {
            $codeMatch = $matches[0]
            Write-Host ('  📝 ' + $codeMatch + ' detected') -ForegroundColor Yellow
        }
        else {
            Write-Host ('  📝 ' + $line.Substring(0, [Math]::Min(60, $line.Length))) -ForegroundColor Yellow
        }
    }
}

# Show critical errors if any
if ($errors -gt 0) {
    Write-Host '🚨 Critical Errors:' -ForegroundColor Red
    $buildOutput | Select-String 'error' | Select-Object -First 3 | ForEach-Object {
        Write-Host ('  💀 ' + $_.Line.Substring(0, [Math]::Min(80, $_.Line.Length))) -ForegroundColor Red
    }
}

# Problems Panel Analysis
Write-Host ''
Write-Host '🔍 VS Code Problems Panel Analysis:' -ForegroundColor Magenta

# Advanced diagnostic detection
$diagnosticSources = @(
    @{ Pattern = "CS\d+"; Name = "C# Compiler"; Icon = "🔶" },
    @{ Pattern = "CA\d+"; Name = "Code Analysis"; Icon = "📊" },
    @{ Pattern = "IDE\d+"; Name = "IntelliSense"; Icon = "🧠" },
    @{ Pattern = "XC\d+|XLS\d+"; Name = "XAML"; Icon = "🎨" },
    @{ Pattern = "MSB\d+"; Name = "MSBuild"; Icon = "🔨" },
    @{ Pattern = "NU\d+"; Name = "NuGet"; Icon = "📦" }
)

$totalProblems = 0
$detectedIssues = @()

foreach ($source in $diagnosticSources) {
    $matchResults = $buildOutput | Select-String $source.Pattern
    if ($matchResults.Count -gt 0) {
        $detectedIssues += [PSCustomObject]@{
            Source = $source.Name
            Icon   = $source.Icon
            Count  = $matchResults.Count
        }
        $totalProblems += $matchResults.Count
    }
}

# File summary
$files = Get-ChildItem -Recurse -Include "*.cs", "*.xaml", "*.csproj" -ErrorAction SilentlyContinue
$csFiles = ($files | Where-Object { $_.Extension -eq '.cs' }).Count
$xamlFiles = ($files | Where-Object { $_.Extension -eq '.xaml' }).Count
Write-Host ("  📁 Files: " + $files.Count + " total (C#: $csFiles, XAML: $xamlFiles)") -ForegroundColor Gray

# Diagnostic summary
if ($detectedIssues.Count -gt 0) {
    Write-Host "  🚨 Build Issues:" -ForegroundColor Yellow
    foreach ($issue in $detectedIssues) {
        Write-Host ("    " + $issue.Icon + " " + $issue.Source + ": " + $issue.Count) -ForegroundColor Yellow
    }
    Write-Host ("  📊 Total: $totalProblems + VS Code diagnostics") -ForegroundColor Yellow
}
else {
    Write-Host "  ⚠️  Build clean, but Problems Panel shows 100+ issues" -ForegroundColor Yellow
    Write-Host "  🔍 Likely IntelliSense/Code Analysis/XAML Designer issues" -ForegroundColor Cyan
}

Write-Host "  💡 Full diagnostics: Problems Panel (Ctrl+Shift+M)" -ForegroundColor Cyan

Write-Host ''
Write-Host '🎯 Phase 1 Next Actions:' -ForegroundColor Cyan

if ($buildExitCode -eq 0) {
    Write-Host '  ✅ Build succeeded - Phase 1 components ready' -ForegroundColor Green
    Write-Host '  🗄️  Database: Run migration if needed: dotnet ef database update' -ForegroundColor Yellow
    Write-Host '  🔍 Problems Panel: Ctrl+Shift+M (100+ issues = design-time diagnostics)' -ForegroundColor Yellow

    if ($warnings -gt 0) {
        Write-Host '  🛠️  Quality: Focus on DbContext CA2000 warnings first' -ForegroundColor Gray
    }

    Write-Host '  🚀 Launch: dotnet run --project BusBuddy.WPF/BusBuddy.WPF.csproj' -ForegroundColor Green
    Write-Host '  📊 Monitor: Use Smart Runtime Intelligence for detailed monitoring' -ForegroundColor Cyan
    Write-Host '  🎯 Test: Navigate to Dashboard → Drivers/Vehicles/Activities' -ForegroundColor Magenta
    Write-Host '  ⚠️  If DateTime errors occur, check appsettings.json connection string' -ForegroundColor Yellow
}
else {
    Write-Host '  🔧 Fix build errors in Phase 1 components first' -ForegroundColor Red
    Write-Host '  💡 Try: dotnet clean && dotnet restore' -ForegroundColor Cyan
    Write-Host '  🗄️  Check: EF Core tools and migrations' -ForegroundColor Yellow
}

Write-Host ''
Write-Host ('=' * 60) -ForegroundColor Gray
Write-Host ('📈 Report Complete: ' + (Get-Date -Format 'HH:mm:ss')) -ForegroundColor Cyan
