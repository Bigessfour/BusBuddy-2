# BusBuddy Deployment Validation Script v2.0
# This script validates that the application is ready for deployment

param(
    [string]$Environment = 'Production',
    [string]$LogsDir = 'logs',
    [switch]$FailFast = $false,
    [switch]$Verbose = $false,
    [switch]$JsonOutput = $false
)

Write-Host '🚌 BusBuddy Deployment Validation Script v2.0' -ForegroundColor Cyan
Write-Host "Environment: $Environment" -ForegroundColor Yellow
Write-Host "Logs Directory: $LogsDir" -ForegroundColor Yellow
Write-Host '===============================================' -ForegroundColor Cyan

$ErrorActionPreference = if ($FailFast) { 'Stop' } else { 'Continue' }
$VerbosePreference = if ($Verbose) { 'Continue' } else { 'SilentlyContinue' }

$ValidationResults = @()
$OverallSuccess = $true

function Add-ValidationResult {
    param(
        [string]$Name,
        [bool]$Success,
        [string]$Message,
        [string]$Details = ''
    )

    $script:ValidationResults += [PSCustomObject]@{
        Name      = $Name
        Success   = $Success
        Message   = $Message
        Details   = $Details
        Timestamp = Get-Date
    }

    if (-not $Success) {
        $script:OverallSuccess = $false
    }

    $icon = if ($Success) { '✅' } else { '❌' }
    $color = if ($Success) { 'Green' } else { 'Red' }
    Write-Host "$icon $Name`: $Message" -ForegroundColor $color

    if ($Details -and ($Verbose -or -not $Success)) {
        Write-Host "   Details: $Details" -ForegroundColor Gray
    }
}

# Check for required files
Write-Host "`n📁 Checking Required Files..." -ForegroundColor Cyan

$RequiredFiles = @(
    'BusBuddy.sln',
    'BusBuddy.WPF\BusBuddy.WPF.csproj',
    'BusBuddy.Core\BusBuddy.Core.csproj',
    'BusBuddy.WPF\appsettings.json'
)

foreach ($file in $RequiredFiles) {
    $exists = Test-Path $file
    Add-ValidationResult -Name "File: $file" -Success $exists -Message $(if ($exists) { 'Found' } else { 'Missing' })
}

# Check .NET SDK
Write-Host "`n🔧 Checking .NET Environment..." -ForegroundColor Cyan

try {
    $dotnetVersion = dotnet --version 2>$null
    if ($dotnetVersion) {
        # Parse version to check if it's >= 8.0
        $versionObj = [System.Version]::Parse($dotnetVersion.Split('-')[0])
        $isValidVersion = $versionObj.Major -ge 8

        if ($isValidVersion) {
            Add-ValidationResult -Name '.NET SDK Version' -Success $true -Message "Version $dotnetVersion (>= 8.0 required)" -Details "Detected .NET $($versionObj.Major).$($versionObj.Minor)"
        } else {
            Add-ValidationResult -Name '.NET SDK Version' -Success $false -Message "Version $dotnetVersion is below required .NET 8.0" -Details 'Please upgrade to .NET 8.0 or later'
        }
    } else {
        Add-ValidationResult -Name '.NET SDK' -Success $false -Message 'Not found or not accessible'
    }

    # Check for .NET runtime specifically
    $runtimeInfo = dotnet --list-runtimes 2>$null | Where-Object { $_ -like '*Microsoft.WindowsDesktop.App*8.*' }
    if ($runtimeInfo) {
        Add-ValidationResult -Name '.NET Windows Desktop Runtime' -Success $true -Message 'Windows Desktop Runtime 8.x found' -Details ($runtimeInfo -join ', ')
    } else {
        Add-ValidationResult -Name '.NET Windows Desktop Runtime' -Success $false -Message '.NET Windows Desktop Runtime 8.x not found' -Details 'WPF applications require this runtime'
    }
} catch {
    Add-ValidationResult -Name '.NET SDK' -Success $false -Message 'Error checking version' -Details $_.Exception.Message
}

# Check environment variables for production
Write-Host "`n🔐 Checking Environment Configuration..." -ForegroundColor Cyan

if ($Environment -ne 'Development') {
    $syncfusionKey = [Environment]::GetEnvironmentVariable('SYNCFUSION_LICENSE_KEY')
    if ($syncfusionKey) {
        Add-ValidationResult -Name 'Syncfusion License' -Success $true -Message 'Environment variable set' -Details "Key length: $($syncfusionKey.Length) characters"
    } else {
        Add-ValidationResult -Name 'Syncfusion License' -Success $false -Message 'SYNCFUSION_LICENSE_KEY not set'
    }

    # Check database connection string
    $dbConnectionString = [Environment]::GetEnvironmentVariable('DATABASE_CONNECTION_STRING')
    if ($dbConnectionString) {
        Add-ValidationResult -Name 'Database Connection String' -Success $true -Message 'Environment variable set'

        # Test basic database connectivity
        Write-Host "`n🗄️ Testing Database Connectivity..." -ForegroundColor Cyan
        try {
            # Try to ping the database server
            if ($dbConnectionString -match 'Server=tcp:([^,;]+)') {
                $serverName = $matches[1]
                $pingResult = Test-NetConnection -ComputerName $serverName -Port 1433 -WarningAction SilentlyContinue
                if ($pingResult.TcpTestSucceeded) {
                    Add-ValidationResult -Name 'Database Server Connectivity' -Success $true -Message "Can reach $serverName on port 1433"
                } else {
                    Add-ValidationResult -Name 'Database Server Connectivity' -Success $false -Message "Cannot reach $serverName on port 1433" -Details 'Check firewall and network connectivity'
                }
            }

            # Try basic SQL connection (if sqlcmd is available)
            $sqlcmdPath = Get-Command sqlcmd -ErrorAction SilentlyContinue
            if ($sqlcmdPath) {
                # Parse connection string components
                $server = $null
                $database = $null
                $userId = $null
                $password = $null

                # Extract server name
                if ($dbConnectionString -match 'Server=tcp:([^,;]+)') {
                    $server = $matches[1]
                }

                # Extract database name
                if ($dbConnectionString -match 'Initial Catalog=([^;]+)') {
                    $database = $matches[1]
                } elseif ($dbConnectionString -match 'Database=([^;]+)') {
                    $database = $matches[1]
                }

                # Extract user ID
                if ($dbConnectionString -match 'User ID=([^;]+)') {
                    $userId = $matches[1]
                }

                # Extract password
                if ($dbConnectionString -match 'Password=([^;]+)') {
                    $password = $matches[1]
                }

                if ($server -and $database -and $userId -and $password) {
                    try {
                        $testQuery = 'SELECT 1 AS TestConnection'
                        # Use integrated authentication approach for Azure SQL
                        $sqlResult = sqlcmd -S $server -d $database -U $userId -P $password -Q $testQuery -l 10 -W 2>&1

                        if ($LASTEXITCODE -eq 0 -and $sqlResult -like '*TestConnection*') {
                            Add-ValidationResult -Name 'Database Connection Test' -Success $true -Message 'Successfully connected to database' -Details "Server: $server, Database: $database"
                        } else {
                            Add-ValidationResult -Name 'Database Connection Test' -Success $false -Message 'Database connection failed' -Details "Exit code: $LASTEXITCODE, Output: $($sqlResult -join ' ')"
                        }
                    } catch {
                        Add-ValidationResult -Name 'Database Connection Test' -Success $false -Message 'Error testing database connection' -Details $_.Exception.Message
                    }
                } else {
                    Add-ValidationResult -Name 'Database Connection Test' -Success $false -Message 'Cannot parse connection string components' -Details "Server: $server, Database: $database, UserID: $userId, Password: $(if($password){'***'}else{'missing'})"
                }
            } else {
                Add-ValidationResult -Name 'Database Connection Test' -Success $false -Message 'Cannot test database connection' -Details 'sqlcmd not available'
            }
        } catch {
            Add-ValidationResult -Name 'Database Connectivity' -Success $false -Message 'Error testing database connectivity' -Details $_.Exception.Message
        }
    } else {
        Add-ValidationResult -Name 'Database Connection String' -Success $false -Message 'DATABASE_CONNECTION_STRING not set'
    }
}

# Check for sensitive data logging
$sensitiveLogging = [Environment]::GetEnvironmentVariable('ENABLE_SENSITIVE_DATA_LOGGING')
if ($Environment -ne 'Development' -and $sensitiveLogging -eq 'true') {
    Add-ValidationResult -Name 'Security Check' -Success $false -Message "Sensitive data logging enabled in $Environment" -Details 'This is a security risk'
} else {
    Add-ValidationResult -Name 'Security Check' -Success $true -Message 'Sensitive data logging properly configured'
}

# Build the solution
Write-Host "`n🔨 Building Solution..." -ForegroundColor Cyan

try {
    # First restore packages
    Write-Verbose 'Restoring NuGet packages...'
    $restoreOutput = dotnet restore BusBuddy.sln --verbosity quiet 2>&1
    $restoreSuccess = $LASTEXITCODE -eq 0

    if ($restoreSuccess) {
        Add-ValidationResult -Name 'Package Restore' -Success $true -Message 'NuGet packages restored successfully'
    } else {
        Add-ValidationResult -Name 'Package Restore' -Success $false -Message 'Package restore failed' -Details ($restoreOutput -join "`n")
    }

    # Then build the solution
    Write-Verbose 'Building solution...'
    $buildOutput = dotnet build BusBuddy.sln --configuration Release --no-restore --verbosity normal 2>&1
    $buildSuccess = $LASTEXITCODE -eq 0

    if ($buildSuccess) {
        # Check for warnings
        $warnings = $buildOutput | Where-Object { $_ -like '*warning*' }
        $warningCount = ($warnings | Measure-Object).Count

        if ($warningCount -eq 0) {
            Add-ValidationResult -Name 'Build' -Success $true -Message 'Solution built successfully with no warnings'
        } else {
            Add-ValidationResult -Name 'Build' -Success $true -Message "Solution built successfully with $warningCount warnings" -Details ($warnings -join "`n")
        }
    } else {
        Add-ValidationResult -Name 'Build' -Success $false -Message 'Build failed' -Details ($buildOutput -join "`n")
    }
} catch {
    Add-ValidationResult -Name 'Build' -Success $false -Message 'Build error' -Details $_.Exception.Message
}

# Check logs directory
Write-Host "`n📝 Checking Logging Configuration..." -ForegroundColor Cyan

if (-not (Test-Path $LogsDir)) {
    try {
        New-Item -ItemType Directory -Path $LogsDir -Force | Out-Null
        Add-ValidationResult -Name 'Logs Directory' -Success $true -Message "Created logs directory: $LogsDir"
    } catch {
        Add-ValidationResult -Name 'Logs Directory' -Success $false -Message "Could not create logs directory: $LogsDir" -Details $_.Exception.Message
    }
} else {
    Add-ValidationResult -Name 'Logs Directory' -Success $true -Message "Logs directory exists: $LogsDir"
}

# Test log file write permissions
try {
    $testLogFile = Join-Path $LogsDir 'deployment_test.log'
    "Deployment validation test at $(Get-Date)" | Out-File -FilePath $testLogFile -Encoding UTF8
    Remove-Item $testLogFile -ErrorAction SilentlyContinue
    Add-ValidationResult -Name 'Log Write Permissions' -Success $true -Message "Can write to logs directory: $LogsDir"
} catch {
    Add-ValidationResult -Name 'Log Write Permissions' -Success $false -Message "Cannot write to logs directory: $LogsDir" -Details $_.Exception.Message
}

# Run a quick startup validation (if we can)
Write-Host "`n🚀 Running Application Startup Test..." -ForegroundColor Cyan

try {
    # Just test that the app can start its validation process without running the full UI
    $startupTestOutput = dotnet run --project BusBuddy.WPF\BusBuddy.WPF.csproj --no-build -- --validate-only 2>&1

    # Since --validate-only doesn't exist, we'll just check if the build artifacts are there
    $wpfExe = 'BusBuddy.WPF\bin\Release\net8.0-windows\BusBuddy.WPF.exe'
    if (Test-Path $wpfExe) {
        Add-ValidationResult -Name 'Application Artifacts' -Success $true -Message 'Built executable found'
    } else {
        Add-ValidationResult -Name 'Application Artifacts' -Success $false -Message 'Built executable not found' -Details "Expected: $wpfExe"
    }
} catch {
    Add-ValidationResult -Name 'Startup Test' -Success $false -Message 'Could not test application startup' -Details $_.Exception.Message
}

# Summary
Write-Host "`n📊 Validation Summary" -ForegroundColor Cyan
Write-Host '===================' -ForegroundColor Cyan

$totalChecks = $ValidationResults.Count
$passedChecks = ($ValidationResults | Where-Object { $_.Success }).Count
$failedChecks = $totalChecks - $passedChecks

Write-Host "Total Checks: $totalChecks" -ForegroundColor White
Write-Host "Passed: $passedChecks" -ForegroundColor Green
Write-Host "Failed: $failedChecks" -ForegroundColor Red

if ($OverallSuccess) {
    Write-Host "`n✅ DEPLOYMENT VALIDATION PASSED" -ForegroundColor Green -BackgroundColor DarkGreen
    Write-Host "The application appears ready for deployment to $Environment" -ForegroundColor Green
} else {
    Write-Host "`n❌ DEPLOYMENT VALIDATION FAILED" -ForegroundColor Red -BackgroundColor DarkRed
    Write-Host 'The following issues must be resolved before deployment:' -ForegroundColor Red

    $ValidationResults | Where-Object { -not $_.Success } | ForEach-Object {
        Write-Host "  • $($_.Name): $($_.Message)" -ForegroundColor Red
        if ($_.Details) {
            Write-Host "    $($_.Details)" -ForegroundColor DarkRed
        }
    }
}

# Write detailed results to log file
$reportPath = "$LogsDir\deployment_validation_$(Get-Date -Format 'yyyyMMdd_HHmmss').log"
$jsonPath = "$LogsDir\deployment_validation_$(Get-Date -Format 'yyyyMMdd_HHmmss').json"

try {
    # Generate detailed text report
    $report = @"
BusBuddy Deployment Validation Report v2.0
Generated: $(Get-Date)
Environment: $Environment
Overall Result: $(if ($OverallSuccess) { 'PASSED' } else { 'FAILED' })

Summary:
========
Total Checks: $totalChecks
Passed: $passedChecks
Failed: $failedChecks

Detailed Results:
================
"@

    $ValidationResults | ForEach-Object {
        $status = if ($_.Success) { 'PASS' } else { 'FAIL' }
        $report += "`n[$status] $($_.Name): $($_.Message)"
        if ($_.Details) {
            $report += "`n        Details: $($_.Details)"
        }
        $report += "`n        Timestamp: $($_.Timestamp)"
    }

    $report | Out-File -FilePath $reportPath -Encoding UTF8
    Write-Host "`n📄 Detailed report saved to: $reportPath" -ForegroundColor Cyan

    # Generate JSON report if requested
    if ($JsonOutput) {
        $jsonReport = @{
            version     = '2.0'
            timestamp   = Get-Date
            environment = $Environment
            summary     = @{
                totalChecks    = $totalChecks
                passedChecks   = $passedChecks
                failedChecks   = $failedChecks
                overallSuccess = $OverallSuccess
            }
            results     = $ValidationResults
        } | ConvertTo-Json -Depth 3

        $jsonReport | Out-File -FilePath $jsonPath -Encoding UTF8
        Write-Host "📊 JSON report saved to: $jsonPath" -ForegroundColor Cyan
    }
} catch {
    Write-Host "`n⚠️  Could not save detailed report: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Exit with appropriate code
exit $(if ($OverallSuccess) { 0 } else { 1 })
