# Comprehensive PowerShell 7.x Integration for Bus Buddy
# Combines all PowerShell 7.x features and tools

param(
    [string]$Action = "Demo",
    [switch]$Parallel,
    [switch]$Interactive
)

# Import PowerShell 7.x modules
Import-Module Terminal-Icons -ErrorAction SilentlyContinue
Import-Module Microsoft.PowerShell.ConsoleGuiTools -ErrorAction SilentlyContinue
Import-Module powershell-yaml -ErrorAction SilentlyContinue

function Show-BusBuddyDashboard {
    Write-Host "🚌 BUS BUDDY - PowerShell 7.x Development Dashboard" -ForegroundColor Magenta
    Write-Host "=" * 60 -ForegroundColor Cyan

    # System Info using PowerShell 7.x features
    $sysInfo = @{
        PowerShellVersion = $PSVersionTable.PSVersion
        Edition = $PSVersionTable.PSEdition
        Platform = $IsWindows ? "Windows" : ($IsLinux ? "Linux" : "macOS")
        WorkingDirectory = Get-Location
    }

    Write-Host "`n📊 SYSTEM INFORMATION:" -ForegroundColor Green
    $sysInfo.GetEnumerator() | ForEach-Object {
        Write-Host "   $($_.Key): $($_.Value)" -ForegroundColor White
    }

    # Project stats with parallel processing
    Write-Host "`n📁 PROJECT STATISTICS:" -ForegroundColor Green
    $stats = Get-ProjectStats -Parallel:$Parallel
    $stats.GetEnumerator() | ForEach-Object {
        Write-Host "   $($_.Key): $($_.Value)" -ForegroundColor White
    }
}

function Get-ProjectStats {
    param([switch]$Parallel)

    if ($Parallel -and $PSVersionTable.PSVersion.Major -ge 7) {
        # PowerShell 7.x parallel processing
        $files = Get-ChildItem -Recurse -File

        $results = $files | ForEach-Object -Parallel {
            $file = $_
            $ext = $file.Extension.ToLower()

            switch ($ext) {
                '.cs' { @{ Type = 'CSharp'; Size = $file.Length } }
                '.ps1' { @{ Type = 'PowerShell'; Size = $file.Length } }
                '.json' { @{ Type = 'JSON'; Size = $file.Length } }
                '.md' { @{ Type = 'Markdown'; Size = $file.Length } }
                default { @{ Type = 'Other'; Size = $file.Length } }
            }
        } -ThrottleLimit 10

        $grouped = $results | Group-Object Type
        $stats = @{}

        foreach ($group in $grouped) {
            $totalSize = ($group.Group | Measure-Object -Property Size -Sum).Sum
            $stats["$($group.Name) Files"] = "$($group.Count) files ($([math]::Round($totalSize/1KB, 2)) KB)"
        }
    } else {
        # Sequential processing fallback
        $csFiles = Get-ChildItem -Recurse -Filter "*.cs" | Measure-Object
        $ps1Files = Get-ChildItem -Recurse -Filter "*.ps1" | Measure-Object
        $jsonFiles = Get-ChildItem -Recurse -Filter "*.json" | Measure-Object

        $stats = @{
            "C# Files" = $csFiles.Count
            "PowerShell Files" = $ps1Files.Count
            "JSON Files" = $jsonFiles.Count
        }
    }

    return $stats
}

function Invoke-DevWorkflow {
    Write-Host "`n🔧 DEVELOPMENT WORKFLOW:" -ForegroundColor Green

    # 1. Run dependency analysis
    Write-Host "   1. Analyzing dependencies..." -ForegroundColor Yellow
    & ".\Scripts\Analyze-Dependencies.ps1" | Out-Null
    Write-Host "   ✓ Dependencies analyzed" -ForegroundColor Green

    # 2. Categorize build errors with PowerShell 7.x features
    Write-Host "   2. Categorizing build errors..." -ForegroundColor Yellow
    $buildResults = & ".\Scripts\Categorize-BuildErrors.ps1"
    $errorCount = ($buildResults | Select-String "Total Errors:" | ForEach-Object { $_ -replace ".*Total Errors: (\d+).*", '$1' })
    Write-Host "   ✓ Found $errorCount build errors" -ForegroundColor Green

    # 3. Run tests with enhanced reporting
    Write-Host "   3. Running PowerShell tests..." -ForegroundColor Yellow
    $testResults = Invoke-Pester -Path ".\Tests\BusBuddy.PowerShell.Tests.ps1" -PassThru
    $passRate = [math]::Round(($testResults.PassedCount / $testResults.TotalCount) * 100, 1)
    Write-Host "   ✓ Tests completed: $passRate% pass rate" -ForegroundColor Green
}

function Show-InteractiveMenu {
    if (Get-Command Out-ConsoleGridView -ErrorAction SilentlyContinue) {
        Write-Host "`n🎯 INTERACTIVE TOOLS:" -ForegroundColor Green

        $menuItems = @(
            @{ Tool = "File Explorer"; Command = "Get-ChildItem -Recurse | Out-ConsoleGridView" }
            @{ Tool = "Process Monitor"; Command = "Get-Process | Out-ConsoleGridView" }
            @{ Tool = "Build Errors"; Command = ".\Scripts\Categorize-BuildErrors.ps1 | Out-ConsoleGridView" }
            @{ Tool = "Test Results"; Command = "Invoke-Pester .\Tests\ -PassThru | Out-ConsoleGridView" }
        )

        $selection = $menuItems | Out-ConsoleGridView -Title "Select a tool to run"

        if ($selection) {
            Write-Host "Running: $($selection.Tool)" -ForegroundColor Cyan
            Invoke-Expression $selection.Command
        }
    } else {
        Write-Warning "Interactive tools require Microsoft.PowerShell.ConsoleGuiTools module"
    }
}

function Test-PowerShell7Features {
    Write-Host "`n🧪 POWERSHELL 7.X FEATURES TEST:" -ForegroundColor Green

    # 1. Null conditional operators
    Write-Host "   Testing null conditional operators..." -ForegroundColor Yellow
    $testObj = $null
    $result = $testObj?.Property ?? "Default Value"
    $success = $result -eq "Default Value"
    Write-Host "   ✓ Null conditionals: $($success ? 'PASS' : 'FAIL')" -ForegroundColor ($success ? 'Green' : 'Red')

    # 2. Ternary operator
    Write-Host "   Testing ternary operator..." -ForegroundColor Yellow
    $condition = $true
    $ternaryResult = $condition ? "Success" : "Failure"
    $success = $ternaryResult -eq "Success"
    Write-Host "   ✓ Ternary operator: $($success ? 'PASS' : 'FAIL')" -ForegroundColor ($success ? 'Green' : 'Red')

    # 3. Enhanced JSON support
    Write-Host "   Testing enhanced JSON support..." -ForegroundColor Yellow
    $jsonString = '{"name": "Bus Buddy", "version": "1.0"}'
    $hashtable = $jsonString | ConvertFrom-Json -AsHashtable
    $success = $hashtable -is [hashtable] -and $hashtable.name -eq "Bus Buddy"
    Write-Host "   ✓ JSON as hashtable: $($success ? 'PASS' : 'FAIL')" -ForegroundColor ($success ? 'Green' : 'Red')

    # 4. Parallel processing (if enabled)
    if ($Parallel) {
        Write-Host "   Testing parallel processing..." -ForegroundColor Yellow
        $files = Get-ChildItem -Filter "*.cs" | Select-Object -First 5
        $startTime = Get-Date

        $results = $files | ForEach-Object -Parallel {
            Start-Sleep -Milliseconds 100  # Simulate work
            $_.Name
        } -ThrottleLimit 3

        $duration = (Get-Date) - $startTime
        $success = $results.Count -eq $files.Count -and $duration.TotalSeconds -lt 1
        Write-Host "   ✓ Parallel processing: $($success ? 'PASS' : 'FAIL') ($($duration.TotalSeconds)s)" -ForegroundColor ($success ? 'Green' : 'Red')
    }
}

function Export-BusBuddyConfig {
    Write-Host "`n💾 EXPORTING CONFIGURATION:" -ForegroundColor Green

    $config = @{
        Project = "Bus Buddy"
        PowerShellVersion = $PSVersionTable.PSVersion.ToString()
        Platform = $IsWindows ? "Windows" : ($IsLinux ? "Linux" : "macOS")
        Tools = @{
            PSScriptAnalyzer = (Get-Module PSScriptAnalyzer -ListAvailable) ? $true : $false
            Pester = (Get-Module Pester -ListAvailable) ? $true : $false
            TerminalIcons = (Get-Module Terminal-Icons -ListAvailable) ? $true : $false
            ConsoleGuiTools = (Get-Module Microsoft.PowerShell.ConsoleGuiTools -ListAvailable) ? $true : $false
            PowerShellYaml = (Get-Module powershell-yaml -ListAvailable) ? $true : $false
        }
        Features = @{
            NullConditionals = $PSVersionTable.PSVersion.Major -ge 7
            TernaryOperator = $PSVersionTable.PSVersion.Major -ge 7
            ParallelProcessing = $PSVersionTable.PSVersion.Major -ge 7
            EnhancedJSON = $PSVersionTable.PSVersion.Major -ge 7
        }
        Timestamp = Get-Date
    }

    # Export as YAML if module is available
    if (Get-Module powershell-yaml -ListAvailable) {
        $config | ConvertTo-Yaml | Out-File "BusBuddy-PS7Config.yml" -Encoding UTF8
        Write-Host "   ✓ Configuration exported to BusBuddy-PS7Config.yml" -ForegroundColor Green
    }

    # Also export as JSON
    $config | ConvertTo-Json -Depth 3 | Out-File "BusBuddy-PS7Config.json" -Encoding UTF8
    Write-Host "   ✓ Configuration exported to BusBuddy-PS7Config.json" -ForegroundColor Green
}

# Main execution
switch ($Action.ToLower()) {
    "demo" {
        Show-BusBuddyDashboard
        Test-PowerShell7Features
        if ($Interactive) { Show-InteractiveMenu }
        Export-BusBuddyConfig
    }
    "workflow" {
        Show-BusBuddyDashboard
        Invoke-DevWorkflow
    }
    "test" {
        Test-PowerShell7Features
    }
    "interactive" {
        Show-InteractiveMenu
    }
    "export" {
        Export-BusBuddyConfig
    }
    default {
        Write-Host "PowerShell 7.x Integration for Bus Buddy" -ForegroundColor Magenta
        Write-Host "Available actions:" -ForegroundColor White
        Write-Host "  Demo        - Full demonstration" -ForegroundColor Gray
        Write-Host "  Workflow    - Run development workflow" -ForegroundColor Gray
        Write-Host "  Test        - Test PowerShell 7.x features" -ForegroundColor Gray
        Write-Host "  Interactive - Show interactive menu" -ForegroundColor Gray
        Write-Host "  Export      - Export configuration" -ForegroundColor Gray
        Write-Host "`nExample: .\PowerShell7-Integration.ps1 -Action Demo -Parallel -Interactive" -ForegroundColor Cyan
    }
}
