#Requires -Version 7.5

<#
.SYNOPSIS
BusBuddy Comprehensive Build Pipeline with Mandatory PowerShell 7.5.2 Syntax Enforcement

.DESCRIPTION
Industry-standard build pipeline that:
1. MANDATORY: Validates all PowerShell scripts against 7.5.2 requirements
2. Runs comprehensive build process
3. Validates application functionality
4. Provides detailed logging and error analysis

.NOTES
PowerShell 7.5.2 syntax validation is MANDATORY and will block builds if failed
#>

[CmdletBinding()]
param(
    [Parameter()]
    [switch]$SkipSyntaxCheck,  # Only for emergency builds - NOT recommended

    [Parameter()]
    [switch]$StrictMode,       # Enable strict syntax validation

    [Parameter()]
    [switch]$Verbose
)

# Pipeline configuration
$script:PipelineStart = Get-Date
$script:LogFile = "logs\build-pipeline-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
$script:ErrorLog = "logs\build-errors-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"

function Write-PipelineLog {
    param([string]$Message, [string]$Level = "Info")

    $timestamp = Get-Date -Format "HH:mm:ss.fff"
    $logMessage = "[$timestamp] [$Level] $Message"

    # Create logs directory if it doesn't exist
    $logsDir = Split-Path $script:LogFile -Parent
    if (-not (Test-Path $logsDir)) {
        New-Item -ItemType Directory -Path $logsDir -Force | Out-Null
    }

    # Write to log file
    $logMessage | Out-File -FilePath $script:LogFile -Append -Encoding UTF8

    # Write to console with colors
    $color = switch ($Level) {
        "Error" { "Red" }
        "Warning" { "Yellow" }
        "Success" { "Green" }
        "Info" { "Cyan" }
        default { "White" }
    }

    Write-Host $logMessage -ForegroundColor $color
}

function Step-MandatorySyntaxValidation {
    Write-PipelineLog "🔒 STEP 1: MANDATORY PowerShell 7.5.2 Syntax Validation" "Info"

    if ($SkipSyntaxCheck) {
        Write-PipelineLog "⚠️  WARNING: Syntax check skipped - NOT RECOMMENDED!" "Warning"
        return $true
    }

    try {
        # Load the syntax enforcer
        $enforcerPath = "Tools\Scripts\PowerShell-7.5.2-Syntax-Enforcer.ps1"
        if (-not (Test-Path $enforcerPath)) {
            Write-PipelineLog "❌ Syntax enforcer not found: $enforcerPath" "Error"
            return $false
        }

        . $enforcerPath

        # Run mandatory syntax check
        $syntaxResult = Invoke-MandatorySyntaxCheck -Path "." -StrictMode:$StrictMode

        if ($syntaxResult) {
            Write-PipelineLog "✅ All PowerShell scripts pass 7.5.2 syntax validation" "Success"
            return $true
        } else {
            Write-PipelineLog "❌ PowerShell 7.5.2 syntax validation FAILED - BUILD BLOCKED" "Error"
            Write-PipelineLog "Fix all syntax issues before proceeding with build" "Error"
            return $false
        }

    } catch {
        Write-PipelineLog "❌ Syntax validation error: $($_.Exception.Message)" "Error"
        return $false
    }
}

function Step-EnvironmentValidation {
    Write-PipelineLog "🔍 STEP 2: Environment Validation" "Info"

    try {
        # Check PowerShell version
        $psVersion = $PSVersionTable.PSVersion
        Write-PipelineLog "PowerShell Version: $psVersion" "Info"

        if ($psVersion -lt [version]"7.5.0") {
            Write-PipelineLog "❌ PowerShell 7.5+ required for BusBuddy development" "Error"
            return $false
        }

        # Check .NET version
        $dotnetVersion = dotnet --version 2>$null
        Write-PipelineLog ".NET Version: $dotnetVersion" "Info"

        # Check workspace structure
        $requiredPaths = @(
            "BusBuddy.sln",
            "BusBuddy.Core",
            "BusBuddy.WPF",
            "Tools\Scripts\PowerShell-7.5.2-Syntax-Enforcer.ps1"
        )

        foreach ($path in $requiredPaths) {
            if (-not (Test-Path $path)) {
                Write-PipelineLog "❌ Required path missing: $path" "Error"
                return $false
            }
        }

        Write-PipelineLog "✅ Environment validation passed" "Success"
        return $true

    } catch {
        Write-PipelineLog "❌ Environment validation error: $($_.Exception.Message)" "Error"
        return $false
    }
}

function Step-BuildSolution {
    Write-PipelineLog "🔨 STEP 3: Solution Build" "Info"

    try {
        # Clean first
        Write-PipelineLog "Cleaning solution..." "Info"
        $cleanResult = dotnet clean BusBuddy.sln --verbosity minimal 2>&1

        if ($LASTEXITCODE -ne 0) {
            Write-PipelineLog "❌ Clean failed: $cleanResult" "Error"
            return $false
        }

        # Restore packages
        Write-PipelineLog "Restoring packages..." "Info"
        $restoreResult = dotnet restore BusBuddy.sln --verbosity minimal 2>&1

        if ($LASTEXITCODE -ne 0) {
            Write-PipelineLog "❌ Package restore failed: $restoreResult" "Error"
            return $false
        }

        # Build solution
        Write-PipelineLog "Building solution..." "Info"
        $buildResult = dotnet build BusBuddy.sln --configuration Release --verbosity minimal --no-restore 2>&1

        if ($LASTEXITCODE -ne 0) {
            Write-PipelineLog "❌ Build failed: $buildResult" "Error"
            $buildResult | Out-File -FilePath $script:ErrorLog -Append -Encoding UTF8
            return $false
        }

        Write-PipelineLog "✅ Solution build completed successfully" "Success"
        return $true

    } catch {
        Write-PipelineLog "❌ Build error: $($_.Exception.Message)" "Error"
        return $false
    }
}

function Step-RunTests {
    Write-PipelineLog "🧪 STEP 4: Test Execution" "Info"

    try {
        # Check if test project exists
        if (-not (Test-Path "BusBuddy.Tests\BusBuddy.Tests.csproj")) {
            Write-PipelineLog "⚠️  No test project found - skipping tests" "Warning"
            return $true
        }

        # Run tests
        Write-PipelineLog "Running unit tests..." "Info"
        $testResult = dotnet test BusBuddy.Tests\BusBuddy.Tests.csproj --configuration Release --verbosity minimal --no-build 2>&1

        if ($LASTEXITCODE -ne 0) {
            Write-PipelineLog "❌ Tests failed: $testResult" "Error"
            $testResult | Out-File -FilePath $script:ErrorLog -Append -Encoding UTF8
            return $false
        }

        Write-PipelineLog "✅ All tests passed" "Success"
        return $true

    } catch {
        Write-PipelineLog "❌ Test execution error: $($_.Exception.Message)" "Error"
        return $false
    }
}

function Step-ApplicationValidation {
    Write-PipelineLog "🚀 STEP 5: Application Validation" "Info"

    try {
        # Quick application startup test
        Write-PipelineLog "Testing application startup..." "Info"

        # Start application in background for quick validation
        $appProcess = Start-Process -FilePath "dotnet" -ArgumentList "run", "--project", "BusBuddy.WPF\BusBuddy.WPF.csproj", "--no-build" -PassThru -WindowStyle Hidden

        # Give it a few seconds to start
        Start-Sleep -Seconds 5

        # Check if process is still running (indicates successful startup)
        if ($appProcess.HasExited) {
            $exitCode = $appProcess.ExitCode
            Write-PipelineLog "❌ Application exited during startup with code: $exitCode" "Error"
            return $false
        } else {
            # Application started successfully, terminate it
            $appProcess.Kill()
            $appProcess.WaitForExit(5000)  # Wait up to 5 seconds for graceful shutdown
            Write-PipelineLog "✅ Application startup validation passed" "Success"
            return $true
        }

    } catch {
        Write-PipelineLog "❌ Application validation error: $($_.Exception.Message)" "Error"
        return $false
    }
}

function Write-PipelineSummary {
    param([bool]$Success, [timespan]$Duration)

    Write-PipelineLog "📊 BUILD PIPELINE SUMMARY" "Info"
    Write-PipelineLog "Duration: $($Duration.ToString('mm\:ss\.fff'))" "Info"
    Write-PipelineLog "Log File: $script:LogFile" "Info"

    if ($Success) {
        Write-PipelineLog "🎉 BUILD PIPELINE COMPLETED SUCCESSFULLY!" "Success"
        Write-PipelineLog "✅ All PowerShell scripts validated against 7.5.2 requirements" "Success"
        Write-PipelineLog "✅ Solution built and tested successfully" "Success"
        Write-PipelineLog "✅ Application validated and ready for deployment" "Success"
    } else {
        Write-PipelineLog "💥 BUILD PIPELINE FAILED!" "Error"
        Write-PipelineLog "Error Log: $script:ErrorLog" "Error"
        Write-PipelineLog "Review logs and fix issues before retry" "Error"
    }
}

# Main Pipeline Execution
try {
    Write-PipelineLog "🚀 Starting BusBuddy Comprehensive Build Pipeline" "Info"
    Write-PipelineLog "Workspace: $PWD" "Info"
    Write-PipelineLog "PowerShell: $($PSVersionTable.PSVersion)" "Info"

    $pipelineSuccess = $true

    # Execute pipeline steps
    $steps = @(
        @{ Name = "Syntax Validation"; Action = { Step-MandatorySyntaxValidation } },
        @{ Name = "Environment Validation"; Action = { Step-EnvironmentValidation } },
        @{ Name = "Solution Build"; Action = { Step-BuildSolution } },
        @{ Name = "Test Execution"; Action = { Step-RunTests } },
        @{ Name = "Application Validation"; Action = { Step-ApplicationValidation } }
    )

    foreach ($step in $steps) {
        $stepResult = & $step.Action
        if (-not $stepResult) {
            Write-PipelineLog "❌ Pipeline failed at step: $($step.Name)" "Error"
            $pipelineSuccess = $false
            break
        }
    }

    # Calculate duration and write summary
    $duration = (Get-Date) - $script:PipelineStart
    Write-PipelineSummary -Success $pipelineSuccess -Duration $duration

    # Exit with appropriate code
    if ($pipelineSuccess) {
        exit 0
    } else {
        exit 1
    }

} catch {
    Write-PipelineLog "💥 CRITICAL PIPELINE ERROR: $($_.Exception.Message)" "Error"
    Write-PipelineLog $_.Exception.StackTrace "Error"
    exit 1
}
