#Requires -Version 7.5
<#
.SYNOPSIS
    Verify BusBuddy terminal setup and profile loading

.DESCRIPTION
    Quick verification script to test that BusBuddy PowerShell profiles load correctly
    and all essential commands are available in VS Code terminal.

.EXAMPLE
    .\verify-terminal-setup.ps1
#>

Write-Host "🚌 BusBuddy Terminal Setup Verification" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════" -ForegroundColor DarkGray
Write-Host ""

# Check PowerShell version
Write-Host "📋 PowerShell Version: $($PSVersionTable.PSVersion)" -ForegroundColor Yellow
if ($PSVersionTable.PSVersion -ge [version]'7.5.0') {
    Write-Host "✅ PowerShell 7.5+ detected" -ForegroundColor Green
} else {
    Write-Host "❌ PowerShell 7.5+ required" -ForegroundColor Red
    exit 1
}

# Check working directory
$currentPath = Get-Location
Write-Host "📁 Current Directory: $currentPath" -ForegroundColor Yellow
if ($currentPath.Path -like "*BusBuddy*") {
    Write-Host "✅ In BusBuddy project directory" -ForegroundColor Green
} else {
    Write-Host "⚠️ Not in BusBuddy project directory" -ForegroundColor Yellow
}

# Check profile loading
Write-Host ""
Write-Host "🔧 Testing Profile Loading..." -ForegroundColor Cyan
if (Test-Path ".\load-bus-buddy-profiles.ps1") {
    Write-Host "✅ Profile script found" -ForegroundColor Green

    try {
        & ".\load-bus-buddy-profiles.ps1" -Quiet
        Write-Host "✅ Profiles loaded successfully" -ForegroundColor Green
    } catch {
        Write-Host "❌ Profile loading failed: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "❌ Profile script not found" -ForegroundColor Red
    exit 1
}

# Check essential commands
Write-Host ""
Write-Host "🎯 Testing Essential Commands..." -ForegroundColor Cyan

$essentialCommands = @('bb-health', 'bb-build', 'bb-run', 'bb-clean', 'bb-test')
$commandsAvailable = 0

foreach ($cmd in $essentialCommands) {
    if (Get-Command $cmd -ErrorAction SilentlyContinue) {
        Write-Host "✅ $cmd" -ForegroundColor Green
        $commandsAvailable++
    } else {
        Write-Host "❌ $cmd" -ForegroundColor Red
    }
}

# Check build capability
Write-Host ""
Write-Host "🔨 Testing Build Capability..." -ForegroundColor Cyan
try {
    $buildOutput = dotnet --version 2>$null
    if ($buildOutput) {
        Write-Host "✅ .NET SDK: $buildOutput" -ForegroundColor Green
    } else {
        Write-Host "❌ .NET SDK not found" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ .NET SDK not accessible" -ForegroundColor Red
}

# Summary
Write-Host ""
Write-Host "📊 Summary:" -ForegroundColor Cyan
Write-Host "═══════════" -ForegroundColor DarkGray
Write-Host "Commands Available: $commandsAvailable/$($essentialCommands.Count)" -ForegroundColor $(if ($commandsAvailable -eq $essentialCommands.Count) { "Green" } else { "Yellow" })

if ($commandsAvailable -eq $essentialCommands.Count) {
    Write-Host ""
    Write-Host "🎉 Terminal setup is COMPLETE!" -ForegroundColor Green
    Write-Host "Ready for BusBuddy development. Try:" -ForegroundColor Cyan
    Write-Host "  • bb-health    - Check project health" -ForegroundColor Gray
    Write-Host "  • bb-build     - Build the solution" -ForegroundColor Gray
    Write-Host "  • bb-run       - Run the application" -ForegroundColor Gray
    Write-Host "  • bb-clean     - Clean build artifacts" -ForegroundColor Gray
    Write-Host ""
    Write-Host "🚌 Happy coding with BusBuddy! 🚌" -ForegroundColor Cyan
} else {
    Write-Host ""
    Write-Host "⚠️ Some commands are missing. Profile loading may need troubleshooting." -ForegroundColor Yellow
    Write-Host "Try reloading VS Code or running manually:" -ForegroundColor Cyan
    Write-Host "  Import-Module .\PowerShell\Modules\BusBuddy\BusBuddy.psm1" -ForegroundColor Gray
}
