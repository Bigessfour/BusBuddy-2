#Requires -Version 7.5

[CmdletBinding()]
param (
    [string]$ProjectPath,
    [switch]$CleanBuild = $false,
    [switch]$CaptureDependencies = $true,
    [string]$LogDir = "logs",
    [int]$Timeout = 120,
    [switch]$EnableBreakpoints,
    [switch]$GitHubUpload,
    [switch]$AutoDetectProject = $true
)

# Auto-detect project path if not specified
if ([string]::IsNullOrEmpty($ProjectPath) -and $AutoDetectProject) {
    Write-Host "Project path not specified. Auto-detecting main project..." -ForegroundColor Cyan

    # Priority order for project detection
    $projectCandidates = @(
        "BusBuddy.WPF/BusBuddy.WPF.csproj",
        "BusBuddy.WPF\BusBuddy.WPF.csproj",
        (Get-ChildItem -Recurse -Filter "*.WPF.csproj" | Select-Object -First 1 -ExpandProperty FullName),
        (Get-ChildItem -Path "." -Filter "*.csproj" | Where-Object { $_.Name -match "BusBuddy" } | Select-Object -First 1 -ExpandProperty FullName)
    )

    foreach ($candidate in $projectCandidates) {
        if (-not [string]::IsNullOrEmpty($candidate) -and (Test-Path $candidate)) {
            $ProjectPath = $candidate
            Write-Host "Found project: $ProjectPath" -ForegroundColor Green
            break
        }
    }

    if ([string]::IsNullOrEmpty($ProjectPath)) {
        Write-Host "Could not auto-detect project file. Please specify using -ProjectPath." -ForegroundColor Red
        exit 1
    }
}

# Create timestamp for log files
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$logFile = Join-Path $LogDir "busbuddy-run-$timestamp.log"

# Ensure log directory exists
if (-not (Test-Path $LogDir)) {
    New-Item -Path $LogDir -ItemType Directory -Force | Out-Null
}

# ANSI colors (fallback if not supported)
$colors = @{
    Red = if ($Host.UI.SupportsVirtualTerminal) { "`e[31m" } else { "" }
    Yellow = if ($Host.UI.SupportsVirtualTerminal) { "`e[33m" } else { "" }
    Green = if ($Host.UI.SupportsVirtualTerminal) { "`e[32m" } else { "" }
    Cyan = if ($Host.UI.SupportsVirtualTerminal) { "`e[36m" } else { "" }
    Magenta = if ($Host.UI.SupportsVirtualTerminal) { "`e[35m" } else { "" }
    Reset = if ($Host.UI.SupportsVirtualTerminal) { "`e[0m" } else { "" }
}

function Write-LogEntry {
    param (
        [string]$Message,
        [string]$Level = "INFO"
    )

    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logEntry = "[$timestamp] [$Level] $Message"

    # Write to console with appropriate color
    switch ($Level) {
        "ERROR"   { Write-Host $logEntry -ForegroundColor Red }
        "WARNING" { Write-Host $logEntry -ForegroundColor Yellow }
        "SUCCESS" { Write-Host $logEntry -ForegroundColor Green }
        default   { Write-Host $logEntry }
    }

    # Write to log file
    Add-Content -Path $logFile -Value $logEntry
}

function Validate-SerilogReferences {
    Write-LogEntry "Validating Serilog references..." "INFO"

    # Track seen packages to detect duplicates
    $seenPackages = @{}

    # Check Directory.Build.props
    if (Test-Path "Directory.Build.props") {
        $buildProps = Get-Content "Directory.Build.props" -Raw
        if ($buildProps -match 'SerilogVersion>([^<]+)<') {
            $centralVersion = $matches[1]
            Write-LogEntry "Found centralized Serilog version: $centralVersion" "INFO"
        }
    }

    # Check .csproj files for Serilog references
    $projectFiles = Get-ChildItem -Recurse -Filter "*.csproj"
    $inconsistentVersions = @()

    foreach ($projectFile in $projectFiles) {
        $content = Get-Content $projectFile -Raw
        if ($content -match 'Include="Serilog"[^>]*Version="([^"]+)"') {
            $projectVersion = $matches[1]
            if ($centralVersion -and ($projectVersion -ne $centralVersion)) {
                $inconsistentVersions += @{
                    Project = $projectFile.Name
                    Version = $projectVersion
                    Expected = $centralVersion
                }
            }

            # Track this package version
            $key = "Serilog-$projectVersion"
            if ($seenPackages.ContainsKey($key)) {
                $seenPackages[$key] += 1
            } else {
                $seenPackages[$key] = 1
            }

            Write-LogEntry "Project $($projectFile.Name) references Serilog v$projectVersion" "INFO"
        }
    }

    # Check for assembly binding redirects
    $configFiles = Get-ChildItem -Recurse -Filter "*.config" | Where-Object { $_.Name -match '(app|web)\.config$' }
    $hasBindingRedirects = $false

    foreach ($configFile in $configFiles) {
        $content = Get-Content $configFile -Raw
        if ($content -match '<assemblyIdentity name="Serilog"' -and $content -match '<bindingRedirect') {
            $hasBindingRedirects = $true
            Write-LogEntry "Found Serilog binding redirect in $($configFile.Name)" "INFO"

            # Extract redirect version
            if ($content -match 'newVersion="([^"]+)"') {
                $redirectVersion = $matches[1]
                Write-LogEntry "  - Redirecting to version: $redirectVersion" "INFO"
            }
        }
    }

    # Check for version conflicts
    if ($seenPackages.Count -gt 1) {
        Write-LogEntry "WARNING: Multiple Serilog versions detected:" "WARNING"
        foreach ($key in $seenPackages.Keys) {
            $version = $key.Split('-')[1]
            Write-LogEntry "  - v$version in $($seenPackages[$key]) project(s)" "WARNING"
        }

        if (-not $hasBindingRedirects) {
            Write-LogEntry "WARNING: No binding redirects found for Serilog version conflicts" "WARNING"
        }
    }

    if ($inconsistentVersions.Count -gt 0) {
        Write-LogEntry "WARNING: Found inconsistent Serilog versions:" "WARNING"
        foreach ($item in $inconsistentVersions) {
            Write-LogEntry "  - $($item.Project): v$($item.Version) (expected v$($item.Expected))" "WARNING"
        }
        return $false
    }

    return $true
}

function Build-Project {
    param (
        [switch]$Clean
    )

    try {
        # Optional clean
        if ($Clean) {
            Write-LogEntry "Cleaning project..." "INFO"
            $cleanOutput = dotnet clean $ProjectPath 2>&1
            Write-LogEntry "Clean completed: $cleanOutput" "INFO"
        }

        # Build with timing
        Write-LogEntry "Building project $ProjectPath..." "INFO"
        $startTime = Get-Date
        $buildOutput = dotnet build $ProjectPath --configuration Debug 2>&1
        $endTime = Get-Date
        $buildTime = ($endTime - $startTime).TotalSeconds

        # Check for build errors
        $buildSuccess = $LASTEXITCODE -eq 0

        # Count warnings and errors
        $warningCount = ($buildOutput | Select-String -Pattern "warning" -SimpleMatch).Count
        $errorCount = ($buildOutput | Select-String -Pattern "error" -SimpleMatch).Count

        if ($buildSuccess) {
            Write-LogEntry "Build succeeded in $buildTime seconds ($warningCount warnings, $errorCount errors)" "SUCCESS"
        } else {
            Write-LogEntry "Build failed with exit code $LASTEXITCODE" "ERROR"
            foreach ($line in $buildOutput) {
                if ($line -match "error") {
                    Write-LogEntry "  $line" "ERROR"
                }
            }
        }

        # Log build output to file
        $buildOutput | Out-File -Append -FilePath $logFile

        return $buildSuccess
    }
    catch {
        Write-LogEntry "Exception during build: $_" "ERROR"
        Write-LogEntry $_.Exception.StackTrace "ERROR"
        return $false
    }
}

function Run-Project {
    try {
        Write-LogEntry "Running project $ProjectPath..." "INFO"

        # Ensure output directories exist
        $stdoutPath = "$LogDir\stdout-$timestamp.log"
        $stderrPath = "$LogDir\stderr-$timestamp.log"
        $mergedOutputPath = "$LogDir\output-$timestamp.log"

        # Create new Process with more detailed output capture
        $psi = New-Object System.Diagnostics.ProcessStartInfo
        $psi.FileName = "dotnet"
        $psi.Arguments = "run --project `"$ProjectPath`""
        $psi.UseShellExecute = $false
        $psi.RedirectStandardOutput = $true
        $psi.RedirectStandardError = $true
        $psi.CreateNoWindow = $true

        $process = New-Object System.Diagnostics.Process
        $process.StartInfo = $psi

        # Set up output handlers with more detailed logging
        $stdoutBuilder = New-Object System.Text.StringBuilder
        $stderrBuilder = New-Object System.Text.StringBuilder

        $stdoutHandler = [System.EventHandler[System.Diagnostics.DataReceivedEventArgs]]{
            param($sender, $e)
            if (-not [string]::IsNullOrEmpty($e.Data)) {
                $stdoutBuilder.AppendLine($e.Data) | Out-Null
                $timestamp = Get-Date -Format "HH:mm:ss.fff"
                "[$timestamp] [STDOUT] $($e.Data)" | Out-File -FilePath $mergedOutputPath -Append

                # Real-time console output for important messages
                if ($e.Data -match "(error|exception|fail|crash)") {
                    Write-Host "[$timestamp] [STDOUT] $($e.Data)" -ForegroundColor Yellow
                }
            }
        }

        $stderrHandler = [System.EventHandler[System.Diagnostics.DataReceivedEventArgs]]{
            param($sender, $e)
            if (-not [string]::IsNullOrEmpty($e.Data)) {
                $stderrBuilder.AppendLine($e.Data) | Out-Null
                $timestamp = Get-Date -Format "HH:mm:ss.fff"
                "[$timestamp] [STDERR] $($e.Data)" | Out-File -FilePath $mergedOutputPath -Append

                # Always show stderr in real-time
                Write-Host "[$timestamp] [STDERR] $($e.Data)" -ForegroundColor Red
            }
        }

        $process.OutputDataReceived += $stdoutHandler
        $process.ErrorDataReceived += $stderrHandler

        # Start and begin capturing
        $process.Start() | Out-Null
        $process.BeginOutputReadLine()
        $process.BeginErrorReadLine()

        # Store process for cleanup
        $global:wpfProcess = $process
        $global:appProcesses += $process

        # Wait for completion or timeout
        $completed = $process.WaitForExit($Timeout * 1000)

        if (-not $completed) {
            Write-LogEntry "Process timed out after $Timeout seconds" "WARNING"
            try {
                $process.Kill()
                Write-LogEntry "Process was terminated" "WARNING"
            }
            catch {
                Write-LogEntry "Failed to terminate process: $_" "ERROR"
            }
        }
        else {
            $exitCode = $process.ExitCode
            if ($exitCode -eq 0) {
                Write-LogEntry "Process completed successfully with exit code 0" "SUCCESS"
            }
            else {
                Write-LogEntry "Process failed with exit code $exitCode" "ERROR"
            }
        }

        # Save the captured output
        $stdoutBuilder.ToString() | Out-File -FilePath $stdoutPath
        $stderrBuilder.ToString() | Out-File -FilePath $stderrPath

        # Clean up event handlers to prevent memory leaks
        $process.OutputDataReceived -= $stdoutHandler
        $process.ErrorDataReceived -= $stderrHandler

        # Analyze the output
        $stdout = Get-Content $stdoutPath -ErrorAction SilentlyContinue
        $stderr = Get-Content $stderrPath -ErrorAction SilentlyContinue

        # Summary logging
        $stdoutLines = if ($stdout) { $stdout.Count } else { 0 }
        $stderrLines = if ($stderr) { $stderr.Count } else { 0 }

        Write-LogEntry "Process output: $stdoutLines lines to stdout, $stderrLines lines to stderr" "INFO"

        # Process stderr with improved error categorization
        if ($stderr -and $stderr.Count -gt 0) {
            Write-LogEntry "STDERR ANALYSIS:" "ERROR"

            # Categorize errors
            $serilogErrors = $stderr | Where-Object { $_ -match "Serilog" }
            $assemblyErrors = $stderr | Where-Object { $_ -match "System\.IO\.FileNotFoundException|Could not load file or assembly" }
            $typeErrors = $stderr | Where-Object { $_ -match "TypeLoadException|MissingMethodException" }
            $otherErrors = $stderr | Where-Object {
                $_ -match "Exception|Error" -and
                $_ -notmatch "Serilog" -and
                $_ -notmatch "System\.IO\.FileNotFoundException|Could not load file or assembly" -and
                $_ -notmatch "TypeLoadException|MissingMethodException"
            }

            # Log categorized errors
            if ($serilogErrors) {
                Write-LogEntry "SERILOG ERRORS DETECTED:" "ERROR"
                $serilogErrors | ForEach-Object { Write-LogEntry "  $_" "ERROR" }
            }

            if ($assemblyErrors) {
                Write-LogEntry "ASSEMBLY LOADING ERRORS DETECTED:" "ERROR"
                $assemblyErrors | ForEach-Object { Write-LogEntry "  $_" "ERROR" }
            }

            if ($typeErrors) {
                Write-LogEntry "TYPE ERRORS DETECTED:" "ERROR"
                $typeErrors | ForEach-Object { Write-LogEntry "  $_" "ERROR" }
            }

            if ($otherErrors) {
                Write-LogEntry "OTHER ERRORS DETECTED:" "ERROR"
                $otherErrors | ForEach-Object { Write-LogEntry "  $_" "ERROR" }
            }

            # Check for common exceptions
            $knownExceptions = @(
                @{Pattern = "System\.IO\.FileNotFoundException.*Serilog"; Fix = "Add binding redirect for Serilog in app.config" },
                @{Pattern = "TypeLoadException"; Fix = "Check for conflicting type definitions or missing dependencies" },
                @{Pattern = "MissingMethodException"; Fix = "Check for method signature mismatches or framework version issues" }
            )

            foreach ($line in $stderr) {
                foreach ($exception in $knownExceptions) {
                    if ($line -match $exception.Pattern) {
                        Write-LogEntry "DETECTED KNOWN ISSUE: $($exception.Fix)" "WARNING"
                        break
                    }
                }
            }
        }

        if ($CaptureDependencies) {
            Analyze-Dependencies
        }

        return ($exitCode -eq 0)
    }
    catch {
        Write-LogEntry "Exception during run: $_" "ERROR"
        Write-LogEntry $_.Exception.StackTrace "ERROR"
        return $false
    }
}

function Analyze-Dependencies {
    Write-LogEntry "Analyzing project dependencies..." "INFO"

    try {
        $depsOutput = dotnet list $ProjectPath package --include-transitive 2>&1

        # Log filtered dependency output - focus on Serilog and key packages
        Write-LogEntry "DEPENDENCY ANALYSIS (FILTERED):" "INFO"

        # Store Serilog versions
        $serilogPackages = @()
        $serilogMainVersion = $null
        $otherKeyPackages = @()

        foreach ($line in $depsOutput) {
            # Capture Serilog packages
            if ($line -match "Serilog") {
                $serilogPackages += $line
                # Extract the main Serilog version if found
                if ($line -match "^\s+Serilog\s+(\d+\.\d+\.\d+)") {
                    $serilogMainVersion = $matches[1]
                    Write-LogEntry "  DETECTED: Main Serilog version $serilogMainVersion" "INFO"
                }
            }
            # Capture other significant packages (sync, UI, critical)
            elseif ($line -match "(Syncfusion|Microsoft\.Extensions|Microsoft\.UI|Microsoft\.NET|Microsoft\.EntityFrameworkCore|NewtonSoft\.Json)") {
                $otherKeyPackages += $line
            }
        }

        # Log Serilog packages first (these are critical)
        if ($serilogPackages.Count -gt 0) {
            Write-LogEntry "Serilog packages (critical for application):" "INFO"
            $serilogPackages | ForEach-Object { Write-LogEntry "  $_" "INFO" }

            # Check for version conflicts
            $versions = @{}
            foreach ($pkg in $serilogPackages) {
                if ($pkg -match "(Serilog(?:\.[\w\.]+)?)\s+(\d+\.\d+\.\d+)") {
                    $name = $matches[1]
                    $version = $matches[2]
                    $versions[$name] = $version
                }
            }

            # Log a more targeted summary
            Write-LogEntry "Serilog package analysis:" "INFO"
            foreach ($key in $versions.Keys) {
                Write-LogEntry "  - $key : $($versions[$key])" "INFO"
            }

            # Generate appropriate binding redirect based on actual installed version
            if ($serilogMainVersion) {
                Write-LogEntry "BINDING REDIRECT ANALYSIS:" "WARNING"
                Write-LogEntry "  Detected Serilog $serilogMainVersion installed" "WARNING"
                Write-LogEntry "  Application likely requires assembly redirect for version conflicts" "WARNING"
                Write-LogEntry "  RECOMMENDATION: Add binding redirect to $serilogMainVersion in app.config" "WARNING"

                # Store for use in final recommendations
                $script:detectedSerilogVersion = $serilogMainVersion
            }
        }
        else {
            Write-LogEntry "No Serilog packages found - potential configuration issue" "WARNING"
        }

        # Log other key packages
        if ($otherKeyPackages.Count -gt 0) {
            Write-LogEntry "Other key packages:" "INFO"
            $otherKeyPackages | ForEach-Object { Write-LogEntry "  $_" "INFO" }
        }

        # Always log the full output to the file for reference
        "FULL DEPENDENCY LIST:" | Out-File -FilePath "$LogDir\dependencies-$timestamp.log"
        $depsOutput | Out-File -FilePath "$LogDir\dependencies-$timestamp.log" -Append
        Write-LogEntry "Full dependency list saved to: $LogDir\dependencies-$timestamp.log" "INFO"
    }
    catch {
        Write-LogEntry "Error analyzing dependencies: $_" "ERROR"
    }
}

function Check-EventHandlerLeaks {
    param (
        [string]$LogFile
    )

    $timestamp = Get-Date -Format 'HH:mm:ss.fff'

    try {
        # Get registered events
        $events = Get-EventSubscriber -ErrorAction SilentlyContinue
        if ($events -and $events.Count -gt 0) {
            Write-LogEntry "WARNING: Found $($events.Count) registered event handlers that might cause memory leaks if not unregistered:" "WARNING"
            "[$timestamp] WARNING: Found $($events.Count) registered event handlers:" | Out-File -FilePath $LogFile -Append

            foreach ($event in $events) {
                Write-LogEntry "  - $($event.SourceIdentifier) (SubscriptionId: $($event.SubscriptionId))" "WARNING"
                "[$timestamp] EVENT HANDLER: $($event.SourceIdentifier) (SubscriptionId: $($event.SubscriptionId))" | Out-File -FilePath $LogFile -Append
            }

            Write-LogEntry "TIP: Ensure all event handlers are properly unregistered when no longer needed." "WARNING"
            "[$timestamp] TIP: Ensure all event handlers are properly unregistered when no longer needed." | Out-File -FilePath $LogFile -Append
        }
    } catch {
        Write-LogEntry "Error checking for event handler leaks: $_" "ERROR"
        "[$timestamp] ERROR: Failed to check for event handler leaks: $_" | Out-File -FilePath $LogFile -Append
    }
}

# Function to push logs to GitHub Actions
function Push-LogToActions {
    param (
        [string]$LogFile,
        [string]$Repo = 'Bigessfour/BusBuddy-2'
    )

    if (-not (Test-Path $LogFile)) {
        Write-LogEntry "Log file not found: $LogFile" "ERROR"
        return
    }

    try {
        if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
            Write-LogEntry "GitHub CLI not found. Cannot upload logs." "WARNING"
            return
        }

        $runJson = gh run list --limit 1 --repo $Repo --json databaseId | ConvertFrom-Json
        $runId = $runJson.databaseId[0]
        if ($runId) {
            gh artifact upload --repo $Repo --run-id $runId --name "busbuddy-run-log" $LogFile
            Write-LogEntry "Uploaded log to run $runId" "SUCCESS"
        } else {
            Write-LogEntry "No active run found. Log saved locally: $LogFile" "WARNING"
        }
    } catch {
        Write-LogEntry "Failed to upload: $_" "ERROR"
    }
}

# Main execution
Write-LogEntry "=== Starting BusBuddy build and run process (PowerShell $($PSVersionTable.PSVersion)) ===" "INFO"

# Create script-level variable to store detected Serilog version
$script:detectedSerilogVersion = $null

# Validate Serilog references
$serilogValid = Validate-SerilogReferences
if (-not $serilogValid) {
    Write-LogEntry "Serilog version mismatch detected - application may fail at runtime" "WARNING"
    Write-LogEntry "Will provide binding redirect solution after analyzing installed packages" "WARNING"
}

# Build the project
$buildResult = Build-Project -Clean:$CleanBuild
if (-not $buildResult) {
    Write-LogEntry "Build failed - skipping run phase" "ERROR"
    exit 1
}

# Run the project
$runResult = Run-Project
if (-not $runResult) {
    Write-LogEntry "Run failed - check logs for details" "ERROR"
    # Continue to provide analysis and recommendations
}

# Check for event handler leaks
Check-EventHandlerLeaks -LogFile $logFile

# Check for specific errors in the log file
$logContent = Get-Content -Path $logFile -ErrorAction SilentlyContinue
$serilogIssueDetected = $logContent -match 'Serilog.*FileNotFoundException'

if ($serilogIssueDetected) {
    # Use the detected version or fall back to a default if analysis failed
    $redirectVersion = if ($script:detectedSerilogVersion) { $script:detectedSerilogVersion } else { "4.0.2" }

    Write-LogEntry "═════════════════════════════════════════════════════" "WARNING"
    Write-LogEntry "DEPENDENCY ISSUE DETECTED:" "WARNING"
    Write-LogEntry "   The application requires Serilog assembly version 4.0.0.0, but installed package is $redirectVersion" "WARNING"
    Write-LogEntry "   This is a common issue with Serilog 4.0+ where assembly version may not match package version" "WARNING"
    Write-LogEntry "RECOMMENDED FIX:" "SUCCESS"
    Write-LogEntry "   1. Create or update app.config in the project root:" "SUCCESS"
    Write-LogEntry "      <?xml version=\"1.0\" encoding=\"utf-8\"?>" "SUCCESS"
    Write-LogEntry "      <configuration>" "SUCCESS"
    Write-LogEntry "        <runtime>" "SUCCESS"
    Write-LogEntry "          <assemblyBinding xmlns=\"urn:schemas-microsoft-com:asm.v1\">" "SUCCESS"
    Write-LogEntry "            <dependentAssembly>" "SUCCESS"
    Write-LogEntry "              <assemblyIdentity name=\"Serilog\" publicKeyToken=\"24c2f752a8e58a10\" culture=\"neutral\" />" "SUCCESS"
    Write-LogEntry "              <bindingRedirect oldVersion=\"0.0.0.0-$redirectVersion.0\" newVersion=\"$redirectVersion.0\" />" "SUCCESS"
    Write-LogEntry "            </dependentAssembly>" "SUCCESS"
    Write-LogEntry "          </assemblyBinding>" "SUCCESS"
    Write-LogEntry "        </runtime>" "SUCCESS"
    Write-LogEntry "      </configuration>" "SUCCESS"
    Write-LogEntry "   2. Create same file in output directory: $((Get-Item $ProjectPath).Directory.FullName)/bin/Debug/net9.0-windows/app.config" "SUCCESS"
    Write-LogEntry "   3. Clean solution: dotnet clean" "SUCCESS"
    Write-LogEntry "   4. Restore packages: dotnet restore --force" "SUCCESS"
    Write-LogEntry "   5. Rebuild solution: dotnet build" "SUCCESS"
    Write-LogEntry "   6. Verify: dotnet list package --include-transitive | Select-String Serilog" "SUCCESS"
    Write-LogEntry "═════════════════════════════════════════════════════" "WARNING"

    # Would you like to automatically generate these config files?
    Write-Host ""
    Write-Host "Would you like to automatically generate the binding redirect config files? (y/n)" -ForegroundColor Cyan
    $response = Read-Host

    if ($response -eq "y") {
        try {
            # Create app.config in project root
            $projectDir = (Get-Item $ProjectPath).Directory.FullName
            $appConfigPath = Join-Path $projectDir "app.config"

            $configContent = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="Serilog" publicKeyToken="24c2f752a8e58a10" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-$redirectVersion.0" newVersion="$redirectVersion.0" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>
"@
            Set-Content -Path $appConfigPath -Value $configContent
            Write-Host "Created binding redirect at: $appConfigPath" -ForegroundColor Green

            # Create same file in output directory if it exists
            $outputDir = Join-Path $projectDir "bin\Debug\net9.0-windows"
            if (Test-Path $outputDir) {
                $outputConfigPath = Join-Path $outputDir "app.config"
                Set-Content -Path $outputConfigPath -Value $configContent
                Write-Host "Created binding redirect at: $outputConfigPath" -ForegroundColor Green
            }

            Write-Host "Binding redirects created successfully. Please clean and rebuild the solution." -ForegroundColor Green
        }
        catch {
            Write-Host "Error creating binding redirects: $_" -ForegroundColor Red
        }
    }
}

# Upload log to GitHub Actions if requested
if ($GitHubUpload) {
    Push-LogToActions -LogFile $logFile
}

# Make process variables global so they can be accessed by the cleanup script
$global:wpfProcess = $null  # This would be set in your Run-Project function if needed
$global:appProcesses = @()  # This would be populated in your Run-Project function if needed

# Register script termination handler with improved error handling
$scriptCleanup = [scriptblock]::Create(@"
    # Ensure any lingering processes are cleaned up when script exits
    try {
        if (`$global:wpfProcess -and -not `$global:wpfProcess.HasExited) {
            Stop-Process -Id `$global:wpfProcess.Id -Force -ErrorAction SilentlyContinue
        }

        if (`$global:appProcesses) {
            foreach (`$proc in `$global:appProcesses) {
                if (`$proc -and -not `$proc.HasExited) {
                    Stop-Process -Id `$proc.Id -Force -ErrorAction SilentlyContinue
                }
            }
        }
    } catch {
        # Silent error handling for cleanup
    }
"@)

# Register script termination handler with proper error handling
try {
    Register-EngineEvent -SourceIdentifier PowerShell.Exiting -Action $scriptCleanup -SupportEvent -ErrorAction Stop
    $cleanupRegistered = $true
} catch {
    Write-LogEntry "Could not register cleanup event: $($_.Exception.Message)" "WARNING"
    $cleanupRegistered = $false
}

Write-LogEntry "=== Process completed successfully ===" "SUCCESS"

Write-Host "───────────────────────────────────────────────────────────"
Write-Host "Press any key to continue..." -NoNewline
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# Clean up the event registration before exiting
if ($cleanupRegistered) {
    try {
        Unregister-Event -SourceIdentifier PowerShell.Exiting -Force -ErrorAction SilentlyContinue
    } catch {
        # Silently continue if unregistration fails - this is just cleanup
    }
}

# Check for potential event handler leaks only if there were any registered
$events = Get-EventSubscriber -ErrorAction SilentlyContinue
if ($events -and $events.Count -gt 0) {
    Check-EventHandlerLeaks -LogFile $logFile
}

# Final process cleanup to ensure nothing remains after script completion
try {
    # One more check for any lingering processes that might have been missed
    if ($global:wpfProcess -and -not $global:wpfProcess.HasExited) {
        Stop-Process -Id $global:wpfProcess.Id -Force -ErrorAction SilentlyContinue
    }

    # Clean up any other processes that might be lingering
    $lingeringProcesses = 0
    if ($global:appProcesses) {
        foreach ($proc in $global:appProcesses) {
            if ($proc -and -not $proc.HasExited) {
                $lingeringProcesses++
                Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
            }
        }
    }

    if ($lingeringProcesses -gt 0 -and $logFile -and (Test-Path $logFile)) {
        "[$(Get-Date -Format 'HH:mm:ss.fff')] CLEANUP: Terminated $lingeringProcesses lingering processes" |
            Out-File -FilePath $logFile -Append -ErrorAction SilentlyContinue
    }
} catch {
    # Final cleanup, ignore errors
}

# Log script completion
if ($logFile -and (Test-Path $logFile)) {
    "[$(Get-Date -Format 'HH:mm:ss.fff')] SCRIPT-COMPLETED: Run with exception capture completed successfully" |
        Out-File -FilePath $logFile -Append -ErrorAction SilentlyContinue
}

exit 0
# Final process cleanup to ensure nothing remains after script completion
try {
    # One more check for any lingering processes that might have been missed
    if ($global:wpfProcess -and -not $global:wpfProcess.HasExited) {
        Stop-Process -Id $global:wpfProcess.Id -Force -ErrorAction SilentlyContinue
    }

    # Clean up any other processes that might be lingering
    $lingeringProcesses = 0
    if ($global:appProcesses) {
        foreach ($proc in $global:appProcesses) {
            if ($proc -and -not $proc.HasExited) {
                $lingeringProcesses++
                Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
            }
        }
    }

    if ($lingeringProcesses -gt 0 -and $logFile -and (Test-Path $logFile)) {
        "[$(Get-Date -Format 'HH:mm:ss.fff')] CLEANUP: Terminated $lingeringProcesses lingering processes" |
            Out-File -FilePath $logFile -Append -ErrorAction SilentlyContinue
    }
} catch {
    # Final cleanup, ignore errors
}

# Log script completion
if ($logFile -and (Test-Path $logFile)) {
    "[$(Get-Date -Format 'HH:mm:ss.fff')] SCRIPT-COMPLETED: Run with exception capture completed successfully" |
        Out-File -FilePath $logFile -Append -ErrorAction SilentlyContinue
}
