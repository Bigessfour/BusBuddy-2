# Bus Buddy Development Automation Hub
# Central script to run all development tools

param(
    [ValidateSet('Install', 'Analyze', 'Build', 'Test', 'Refactor', 'All')]
    [string]$Action = 'All',
    [switch]$Detailed,
    [switch]$ExportReports
)

Write-Host "Bus Buddy Development Automation Hub" -ForegroundColor Magenta
Write-Host "====================================" -ForegroundColor Magenta

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

switch ($Action) {
    'Install' {
        Write-Host "Installing developer tools..." -ForegroundColor Green
        & "$scriptDir\Install-DeveloperTools.ps1"
    }

    'Analyze' {
        Write-Host "Running dependency analysis..." -ForegroundColor Green
        & "$scriptDir\Analyze-Dependencies.ps1" -ShowDetails:$Detailed -ExportReport:$ExportReports
    }

    'Build' {
        Write-Host "Analyzing build errors..." -ForegroundColor Green
        & "$scriptDir\Categorize-BuildErrors.ps1" -RunBuild -ExportReport:$ExportReports
    }

    'Test' {
        Write-Host "Running NUnit and PowerShell tests..." -ForegroundColor Green

        # Run NUnit tests
        if (Get-Command dotnet -ErrorAction SilentlyContinue) {
            Write-Host "Running .NET/NUnit tests..." -ForegroundColor Yellow
            & "$scriptDir\NUnit-TestingTools.ps1"
            Invoke-BusBuddyTests -Detailed:$Detailed
        }

        # Run PowerShell tests
        if (Get-Module Pester -ListAvailable) {
            Write-Host "Running PowerShell tests..." -ForegroundColor Yellow
            Invoke-Pester "$scriptDir\..\Tests\BusBuddy.PowerShell.Tests.ps1" -Detailed:$Detailed
        } else {
            Write-Warning "Pester not installed. Run with -Action Install first."
        }
    }

    'Refactor' {
        Write-Host "Running refactoring tools..." -ForegroundColor Green
        Import-Module "$scriptDir\BusBuddy-RefactoringTools.psm1" -Force
        Test-SyncfusionReferences
    }

    'All' {
        Write-Host "Running complete development workflow..." -ForegroundColor Green

        # Check if tools are installed
        $toolsNeeded = @('PSScriptAnalyzer', 'Pester', 'InvokeBuild')
        $missingTools = @()

        foreach ($tool in $toolsNeeded) {
            if (-not (Get-Module $tool -ListAvailable)) {
                $missingTools += $tool
            }
        }

        if ($missingTools.Count -gt 0) {
            Write-Warning "Missing tools: $($missingTools -join ', ')"
            Write-Host "Installing missing tools..." -ForegroundColor Yellow
            & "$scriptDir\Install-DeveloperTools.ps1"
        }

        # Run full analysis workflow
        Write-Host "`n1. Dependency Analysis" -ForegroundColor Cyan
        & "$scriptDir\Analyze-Dependencies.ps1" -ExportReport:$ExportReports

        Write-Host "`n2. Build Error Analysis" -ForegroundColor Cyan
        & "$scriptDir\Categorize-BuildErrors.ps1" -RunBuild -ExportReport:$ExportReports

        Write-Host "`n3. Syncfusion Reference Check" -ForegroundColor Cyan
        Import-Module "$scriptDir\BusBuddy-RefactoringTools.psm1" -Force
        Test-SyncfusionReferences

        Write-Host "`n4. PowerShell Tests" -ForegroundColor Cyan
        if (Get-Module Pester -ListAvailable) {
            Invoke-Pester "$scriptDir\..\Tests\BusBuddy.PowerShell.Tests.ps1"
        }

        Write-Host "`nDevelopment workflow complete!" -ForegroundColor Green
    }
}

Write-Host "`nUse -Action parameter to run specific tools:" -ForegroundColor White
Write-Host "  Install  - Install all developer tools" -ForegroundColor Gray
Write-Host "  Analyze  - Run dependency analysis" -ForegroundColor Gray
Write-Host "  Build    - Categorize build errors" -ForegroundColor Gray
Write-Host "  Test     - Run PowerShell tests" -ForegroundColor Gray
Write-Host "  Refactor - Run refactoring tools" -ForegroundColor Gray
Write-Host "  All      - Run complete workflow" -ForegroundColor Gray
