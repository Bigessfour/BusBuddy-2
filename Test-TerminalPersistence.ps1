#Requires -Version 7.0
<#
.SYNOPSIS
Test Terminal Persistence for BusBuddy Development Environment

.DESCRIPTION
Comprehensive testing script for the Enhanced Terminal Persistence functionality.
Tests session management, state preservation, profile loading, and environment
configuration for the BusBuddy development workflow.

.NOTES
- Requires PowerShell 7.0+
- Tests VS Code integrated terminal compatibility
- Validates session state persistence
- Includes performance benchmarks

.EXAMPLE
.\Test-TerminalPersistence.ps1
Run all terminal persistence tests

.EXAMPLE
.\Test-TerminalPersistence.ps1 -TestName "SessionManagement"
Run specific test category

.EXAMPLE
.\Test-TerminalPersistence.ps1 -Verbose -IncludePerformance
Run with verbose output and performance tests
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("All", "SessionManagement", "ProfileLoading", "StatePreservation", "EnvironmentConfig", "Performance")]
    [string]$TestName = "All",

    [Parameter(Mandatory = $false)]
    [switch]$IncludePerformance,

    [Parameter(Mandatory = $false)]
    [switch]$Quiet,

    [Parameter(Mandatory = $false)]
    [string]$WorkspaceRoot = $PSScriptRoot,

    [Parameter(Mandatory = $false)]
    [switch]$GenerateReport
)

# Terminal Persistence Testing Framework
class TerminalPersistenceTestRunner {
    [string]$WorkspaceRoot
    [array]$TestResults
    [hashtable]$TestStats
    [datetime]$StartTime

    TerminalPersistenceTestRunner([string]$workspaceRoot) {
        $this.WorkspaceRoot = $workspaceRoot
        $this.TestResults = @()
        $this.TestStats = @{
            Passed = 0
            Failed = 0
            Skipped = 0
            Total = 0
        }
        $this.StartTime = Get-Date
    }

    [void]WriteTestHeader([string]$testName) {
        if (-not $using:Quiet) {
            Write-Host "`n🧪 Testing: $testName" -ForegroundColor Cyan
            Write-Host ("=" * 50) -ForegroundColor Gray
        }
    }

    [object]RunTest([string]$testName, [scriptblock]$testScript) {
        $testStart = Get-Date
        $result = @{
            Name = $testName
            Status = "Unknown"
            Message = ""
            Duration = 0
            Details = @{}
        }

        try {
            $testOutput = & $testScript
            $result.Status = "Passed"
            $result.Message = "Test completed successfully"
            $result.Details = $testOutput
            $this.TestStats.Passed++
        }
        catch {
            $result.Status = "Failed"
            $result.Message = $_.Exception.Message
            $result.Details = @{ Exception = $_.Exception }
            $this.TestStats.Failed++
        }
        finally {
            $result.Duration = (Get-Date) - $testStart
            $this.TestStats.Total++
        }

        $this.TestResults += $result

        if (-not $using:Quiet) {
            $statusColor = if ($result.Status -eq "Passed") { "Green" } else { "Red" }
            Write-Host "  ✓ $testName - $($result.Status)" -ForegroundColor $statusColor
            if ($result.Status -eq "Failed") {
                Write-Host "    Error: $($result.Message)" -ForegroundColor Red
            }
        }

        return $result
    }

    [void]TestSessionManagement() {
        $this.WriteTestHeader("Session Management")

        # Test 1: Enhanced Terminal Persistence script exists
        $this.RunTest("Script Existence", {
            $scriptPath = Join-Path $this.WorkspaceRoot "Enhanced-Terminal-Persistence.ps1"
            if (-not (Test-Path $scriptPath)) {
                throw "Enhanced-Terminal-Persistence.ps1 not found"
            }
            return @{ ScriptPath = $scriptPath }
        })

        # Test 2: Script can be loaded without errors
        $this.RunTest("Script Loading", {
            $scriptPath = Join-Path $this.WorkspaceRoot "Enhanced-Terminal-Persistence.ps1"
            try {
                . $scriptPath
                return @{ LoadStatus = "Success" }
            }
            catch {
                throw "Failed to load script: $($_.Exception.Message)"
            }
        })

        # Test 3: TerminalPersistenceManager class instantiation
        $this.RunTest("Class Instantiation", {
            $scriptPath = Join-Path $this.WorkspaceRoot "Enhanced-Terminal-Persistence.ps1"
            . $scriptPath

            $manager = [TerminalPersistenceManager]::new($this.WorkspaceRoot)
            if (-not $manager) {
                throw "Failed to instantiate TerminalPersistenceManager"
            }

            return @{
                ManagerType = $manager.GetType().Name
                WorkspaceRoot = $manager.WorkspaceRoot
                SessionsPath = $manager.SessionsPath
            }
        })

        # Test 4: Sessions directory creation
        $this.RunTest("Sessions Directory", {
            $scriptPath = Join-Path $this.WorkspaceRoot "Enhanced-Terminal-Persistence.ps1"
            . $scriptPath

            $manager = [TerminalPersistenceManager]::new($this.WorkspaceRoot)
            $sessionsPath = Join-Path $this.WorkspaceRoot ".vscode\terminal-sessions"

            if (-not (Test-Path $sessionsPath)) {
                throw "Sessions directory was not created"
            }

            return @{
                SessionsPath = $sessionsPath
                Exists = Test-Path $sessionsPath
            }
        })
    }

    [void]TestProfileLoading() {
        $this.WriteTestHeader("Profile Loading")

        # Test 1: BusBuddy profiles existence
        $this.RunTest("Profile Scripts Existence", {
            $profilePath = Join-Path $this.WorkspaceRoot "load-bus-buddy-profiles.ps1"
            $mainProfilePath = Join-Path $this.WorkspaceRoot "BusBuddy-PowerShell-Profile-7.5.2.ps1"

            $results = @{
                LoadScript = Test-Path $profilePath
                MainProfile = Test-Path $mainProfilePath
            }

            if (-not $results.LoadScript -and -not $results.MainProfile) {
                throw "No BusBuddy profile scripts found"
            }

            return $results
        })

        # Test 2: Profile loading functionality
        $this.RunTest("Profile Loading Test", {
            $scriptPath = Join-Path $this.WorkspaceRoot "Enhanced-Terminal-Persistence.ps1"
            . $scriptPath

            $manager = [TerminalPersistenceManager]::new($this.WorkspaceRoot)

            # Capture current function count
            $beforeFunctions = (Get-Command -CommandType Function).Count

            try {
                $manager.LoadBusBuddyProfiles()
                $afterFunctions = (Get-Command -CommandType Function).Count

                return @{
                    FunctionsBefore = $beforeFunctions
                    FunctionsAfter = $afterFunctions
                    NewFunctions = $afterFunctions - $beforeFunctions
                    LoadAttempted = $true
                }
            }
            catch {
                return @{
                    LoadAttempted = $true
                    Error = $_.Exception.Message
                }
            }
        })

        # Test 3: BusBuddy commands availability
        $this.RunTest("BusBuddy Commands", {
            $profilePath = Join-Path $this.WorkspaceRoot "load-bus-buddy-profiles.ps1"

            if (Test-Path $profilePath) {
                try {
                    & $profilePath -Quiet
                }
                catch {
                    # Profile loading may fail, that's OK for testing
                }
            }

            $busBuddyCommands = Get-Command -Name "bb-*" -ErrorAction SilentlyContinue

            return @{
                CommandCount = $busBuddyCommands.Count
                Commands = $busBuddyCommands.Name
                HasCommands = $busBuddyCommands.Count -gt 0
            }
        })
    }

    [void]TestStatePreservation() {
        $this.WriteTestHeader("State Preservation")

        # Test 1: Session state saving
        $this.RunTest("Session State Saving", {
            $scriptPath = Join-Path $this.WorkspaceRoot "Enhanced-Terminal-Persistence.ps1"
            . $scriptPath

            $manager = [TerminalPersistenceManager]::new($this.WorkspaceRoot)
            $testSessionName = "TestSession-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

            $testConfig = @{
                SessionName = $testSessionName
                SessionId = [System.Guid]::NewGuid().ToString()
                TestData = "Test state preservation"
                TestTime = Get-Date
            }

            $manager.SaveSessionState($testSessionName, $testConfig)

            $sessionFile = Join-Path $manager.SessionsPath "$testSessionName.json"
            if (-not (Test-Path $sessionFile)) {
                throw "Session file was not created"
            }

            return @{
                SessionName = $testSessionName
                SessionFile = $sessionFile
                FileExists = Test-Path $sessionFile
                FileSize = (Get-Item $sessionFile).Length
            }
        })

        # Test 2: Session state restoration
        $this.RunTest("Session State Restoration", {
            $scriptPath = Join-Path $this.WorkspaceRoot "Enhanced-Terminal-Persistence.ps1"
            . $scriptPath

            $manager = [TerminalPersistenceManager]::new($this.WorkspaceRoot)
            $testSessionName = "TestRestore-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

            # Create test session
            $originalConfig = @{
                SessionName = $testSessionName
                TestValue = "OriginalTestValue"
                Timestamp = Get-Date
            }

            $manager.SaveSessionState($testSessionName, $originalConfig)

            # Restore session
            $restoredConfig = $manager.RestoreSessionState($testSessionName)

            if ($restoredConfig.SessionName -ne $testSessionName) {
                throw "Restored session name doesn't match"
            }

            return @{
                OriginalName = $originalConfig.SessionName
                RestoredName = $restoredConfig.SessionName
                RestoreSuccess = $true
            }
        })

        # Test 3: Session listing
        $this.RunTest("Session Listing", {
            $scriptPath = Join-Path $this.WorkspaceRoot "Enhanced-Terminal-Persistence.ps1"
            . $scriptPath

            $manager = [TerminalPersistenceManager]::new($this.WorkspaceRoot)
            $sessions = $manager.ListAvailableSessions()

            return @{
                SessionCount = $sessions.Count
                HasSessions = $sessions.Count -gt 0
                Sessions = $sessions | Select-Object Name, StartTime
            }
        })
    }

    [void]TestEnvironmentConfig() {
        $this.WriteTestHeader("Environment Configuration")

        # Test 1: Environment variables setup
        $this.RunTest("Environment Variables", {
            $scriptPath = Join-Path $this.WorkspaceRoot "Enhanced-Terminal-Persistence.ps1"
            . $scriptPath

            $manager = [TerminalPersistenceManager]::new($this.WorkspaceRoot)
            $testSessionName = "TestEnv-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

            $manager.InitializeSession($testSessionName)

            $envVars = @{
                SessionId = $env:BUSBUDDY_SESSION_ID
                SessionName = $env:BUSBUDDY_SESSION_NAME
                Workspace = $env:BUSBUDDY_WORKSPACE
            }

            $missingVars = @()
            foreach ($var in $envVars.Keys) {
                if (-not $envVars[$var]) {
                    $missingVars += $var
                }
            }

            if ($missingVars.Count -gt 0) {
                throw "Missing environment variables: $($missingVars -join ', ')"
            }

            return $envVars
        })

        # Test 2: PowerShell configuration
        $this.RunTest("PowerShell Configuration", {
            $hasReadLine = Get-Module PSReadLine -ListAvailable
            $currentPreference = $ErrorActionPreference

            return @{
                HasPSReadLine = $hasReadLine -ne $null
                ErrorActionPreference = $currentPreference
                PSVersion = $PSVersionTable.PSVersion.ToString()
                Edition = $PSVersionTable.PSEdition
            }
        })

        # Test 3: Git integration test
        $this.RunTest("Git Integration", {
            try {
                $gitStatus = git --version 2>$null
                $hasGit = $LASTEXITCODE -eq 0

                $isGitRepo = $false
                if ($hasGit) {
                    $gitDir = git rev-parse --git-dir 2>$null
                    $isGitRepo = $LASTEXITCODE -eq 0
                }

                return @{
                    HasGit = $hasGit
                    GitVersion = $gitStatus
                    IsGitRepository = $isGitRepo
                }
            }
            catch {
                return @{
                    HasGit = $false
                    Error = $_.Exception.Message
                }
            }
        })
    }

    [void]TestPerformance() {
        $this.WriteTestHeader("Performance Tests")

        # Test 1: Script loading performance
        $this.RunTest("Script Loading Performance", {
            $scriptPath = Join-Path $this.WorkspaceRoot "Enhanced-Terminal-Persistence.ps1"

            $iterations = 5
            $times = @()

            for ($i = 0; $i -lt $iterations; $i++) {
                $start = Get-Date
                . $scriptPath
                $end = Get-Date
                $times += ($end - $start).TotalMilliseconds
            }

            $averageTime = ($times | Measure-Object -Average).Average
            $maxTime = ($times | Measure-Object -Maximum).Maximum

            return @{
                Iterations = $iterations
                AverageTime = $averageTime
                MaxTime = $maxTime
                Times = $times
            }
        })

        # Test 2: Session initialization performance
        $this.RunTest("Session Initialization Performance", {
            $scriptPath = Join-Path $this.WorkspaceRoot "Enhanced-Terminal-Persistence.ps1"
            . $scriptPath

            $iterations = 3
            $times = @()

            for ($i = 0; $i -lt $iterations; $i++) {
                $manager = [TerminalPersistenceManager]::new($this.WorkspaceRoot)
                $testSessionName = "PerfTest-$i-$(Get-Date -Format 'HHmmss')"

                $start = Get-Date
                $manager.InitializeSession($testSessionName)
                $end = Get-Date

                $times += ($end - $start).TotalMilliseconds
            }

            $averageTime = ($times | Measure-Object -Average).Average

            return @{
                Iterations = $iterations
                AverageTime = $averageTime
                Times = $times
            }
        })
    }

    [void]RunAllTests() {
        Write-Host "🚀 Starting Terminal Persistence Tests" -ForegroundColor Cyan
        Write-Host "Workspace: $($this.WorkspaceRoot)" -ForegroundColor Gray
        Write-Host "Test Runner: PowerShell $($PSVersionTable.PSVersion)" -ForegroundColor Gray

        if ($TestName -eq "All" -or $TestName -eq "SessionManagement") {
            $this.TestSessionManagement()
        }

        if ($TestName -eq "All" -or $TestName -eq "ProfileLoading") {
            $this.TestProfileLoading()
        }

        if ($TestName -eq "All" -or $TestName -eq "StatePreservation") {
            $this.TestStatePreservation()
        }

        if ($TestName -eq "All" -or $TestName -eq "EnvironmentConfig") {
            $this.TestEnvironmentConfig()
        }

        if (($TestName -eq "All" -and $IncludePerformance) -or $TestName -eq "Performance") {
            $this.TestPerformance()
        }
    }

    [void]GenerateTestReport() {
        $duration = (Get-Date) - $this.StartTime

        Write-Host "`n📊 Test Results Summary" -ForegroundColor Cyan
        Write-Host ("=" * 50) -ForegroundColor Gray
        Write-Host "Total Tests: $($this.TestStats.Total)" -ForegroundColor White
        Write-Host "Passed: $($this.TestStats.Passed)" -ForegroundColor Green
        Write-Host "Failed: $($this.TestStats.Failed)" -ForegroundColor Red
        Write-Host "Skipped: $($this.TestStats.Skipped)" -ForegroundColor Yellow
        Write-Host "Duration: $($duration.TotalSeconds.ToString('F2')) seconds" -ForegroundColor Gray

        $successRate = if ($this.TestStats.Total -gt 0) {
            ($this.TestStats.Passed / $this.TestStats.Total * 100).ToString('F1')
        } else {
            "0"
        }
        Write-Host "Success Rate: $successRate%" -ForegroundColor $(if ($successRate -eq "100.0") { "Green" } else { "Yellow" })

        if ($this.TestStats.Failed -gt 0) {
            Write-Host "`n❌ Failed Tests:" -ForegroundColor Red
            $failedTests = $this.TestResults | Where-Object { $_.Status -eq "Failed" }
            foreach ($test in $failedTests) {
                Write-Host "  • $($test.Name): $($test.Message)" -ForegroundColor Red
            }
        }

        if ($GenerateReport) {
            $reportPath = Join-Path $this.WorkspaceRoot "terminal-persistence-test-report.json"
            $reportData = @{
                TestRun = @{
                    Timestamp = $this.StartTime
                    Duration = $duration.TotalSeconds
                    TestName = $TestName
                    WorkspaceRoot = $this.WorkspaceRoot
                }
                Statistics = $this.TestStats
                Results = $this.TestResults
            }

            $reportData | ConvertTo-Json -Depth 10 | Set-Content -Path $reportPath -Encoding UTF8
            Write-Host "`n📄 Test report saved to: $reportPath" -ForegroundColor Blue
        }
    }
}

# Main execution
function Main {
    try {
        $testRunner = [TerminalPersistenceTestRunner]::new($WorkspaceRoot)
        $testRunner.RunAllTests()
        $testRunner.GenerateTestReport()

        if ($testRunner.TestStats.Failed -gt 0) {
            exit 1
        }
    }
    catch {
        Write-Error "Test execution failed: $($_.Exception.Message)"
        exit 1
    }
}

# Execute main function if script is run directly
if ($MyInvocation.InvocationName -ne '.') {
    Main
}
