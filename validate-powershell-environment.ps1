# ========================================
# PowerShell Environment Validator for BusBuddy
# Diagnose and Fix PowerShell 7.5.2 Configuration Issues
# ========================================

param(
    [switch]$FixVSCodeConfig,
    [switch]$ValidateSyntax,
    [switch]$InstallExtensions,
    [switch]$DetailedReport
)

function Write-StatusMessage {
    param(
        [string]$Message,
        [string]$Status = "INFO",  # INFO, SUCCESS, WARNING, ERROR
        [string]$Details = ""
    )

    $colors = @{
        INFO    = "Cyan"
        SUCCESS = "Green"
        WARNING = "Yellow"
        ERROR   = "Red"
    }

    $icons = @{
        INFO    = "ℹ️"
        SUCCESS = "✅"
        WARNING = "⚠️"
        ERROR   = "❌"
    }

    Write-Host "$($icons[$Status]) $Message" -ForegroundColor $colors[$Status]
    if ($Details) {
        Write-Host "   $Details" -ForegroundColor Gray
    }
}

function Test-PowerShellEnvironment {
    Write-Host "🔍 BusBuddy PowerShell Environment Analysis" -ForegroundColor Cyan
    Write-Host "=================================================" -ForegroundColor Cyan

    $issues = @()
    $recommendations = @()

    # 1. PowerShell Version Check
    Write-StatusMessage "Checking PowerShell Version..."
    $psVersion = $PSVersionTable.PSVersion

    if ($psVersion.Major -ge 7) {
        Write-StatusMessage "PowerShell Version: $psVersion" "SUCCESS"

        if ($psVersion.Major -eq 7 -and $psVersion.Minor -ge 5) {
            Write-StatusMessage "Advanced features available (Ternary, Null coalescing, Pipeline chains)" "SUCCESS"
        }
        elseif ($psVersion.Major -eq 7 -and $psVersion.Minor -ge 0) {
            Write-StatusMessage "Modern features available (some advanced syntax may not work)" "WARNING"
            $recommendations += "Consider upgrading to PowerShell 7.5.2 for full feature support"
        }
    }
    else {
        Write-StatusMessage "PowerShell Version: $psVersion" "ERROR" "PowerShell 7.0+ required"
        $issues += "Using Windows PowerShell 5.1 instead of PowerShell 7+"
        $recommendations += "Install PowerShell 7.5.2 from https://github.com/PowerShell/PowerShell/releases"
    }

    # 2. Execution Context Check
    Write-StatusMessage "Checking Execution Context..."
    Write-StatusMessage "Host: $($Host.Name)" "INFO"
    Write-StatusMessage "Edition: $($PSVersionTable.PSEdition)" "INFO"
    Write-StatusMessage "Executable: $PSHOME" "INFO"

    if ($Host.Name -eq "Visual Studio Code Host") {
        Write-StatusMessage "Running in VS Code PowerShell Extension" "SUCCESS"
    }
    elseif ($Host.Name -eq "ConsoleHost") {
        Write-StatusMessage "Running in PowerShell Console" "SUCCESS"
    }
    else {
        Write-StatusMessage "Running in: $($Host.Name)" "WARNING" "May have compatibility issues"
    }

    # 3. Test Modern PowerShell 7.5.2 Syntax
    Write-StatusMessage "Testing Modern PowerShell Syntax..."

    try {
        # Test ternary operator (PowerShell 7.0+)
        $testExpression = '$true ? "Works" : "Failed"'
        $testValue = Invoke-Expression $testExpression
        Write-StatusMessage "Ternary operator (?:): $testValue" "SUCCESS"
    }
    catch {
        Write-StatusMessage "Ternary operator (?:): Failed" "ERROR" $_.Exception.Message
        $issues += "Ternary operator not supported - indicates PowerShell 5.1 or parser issue"
    }

    try {
        # Test null coalescing (PowerShell 7.0+)
        $testExpression = '$null ?? "Default Value"'
        $testNull = Invoke-Expression $testExpression
        Write-StatusMessage "Null coalescing (??): $testNull" "SUCCESS"
    }
    catch {
        Write-StatusMessage "Null coalescing (??): Failed" "ERROR" $_.Exception.Message
        $issues += "Null coalescing operator not supported"
    }

    try {
        # Test pipeline chain operators (PowerShell 7.0+)
        $testExpression = '$true && "Chain works"'
        $result = Invoke-Expression $testExpression
        Write-StatusMessage "Pipeline chains (&&, ||): $result" "SUCCESS"
    }
    catch {
        Write-StatusMessage "Pipeline chains (&&, ||): Failed" "ERROR" $_.Exception.Message
        $issues += "Pipeline chain operators not supported"
    }

    # 4. VS Code Configuration Check
    if (Test-Path ".vscode/settings.json") {
        Write-StatusMessage "Checking VS Code Configuration..."

        try {
            $vscodeSettings = Get-Content ".vscode/settings.json" | ConvertFrom-Json

            # Check PowerShell default version
            if ($vscodeSettings."powershell.powerShellDefaultVersion") {
                $defaultVersion = $vscodeSettings."powershell.powerShellDefaultVersion"
                if ($defaultVersion -like "*7*") {
                    Write-StatusMessage "VS Code PowerShell Default: $defaultVersion" "SUCCESS"
                }
                else {
                    Write-StatusMessage "VS Code PowerShell Default: $defaultVersion" "WARNING" "Should use PowerShell 7.5.2"
                    $recommendations += "Update VS Code settings to use PowerShell 7.5.2 as default"
                }
            }

            # Check script analysis settings
            if ($vscodeSettings."powershell.scriptAnalysis.enable" -eq $false) {
                Write-StatusMessage "Script Analysis: Disabled" "WARNING" "Consider enabling with PS7 compatible rules"
                $recommendations += "Enable script analysis with PowerShell 7 compatible settings"
            }
            else {
                Write-StatusMessage "Script Analysis: Enabled" "SUCCESS"
            }

        }
        catch {
            Write-StatusMessage "VS Code settings parsing failed" "ERROR" $_.Exception.Message
        }
    }
    else {
        Write-StatusMessage "No VS Code configuration found" "WARNING" "Consider setting up .vscode/settings.json"
    }

    # 5. PSScriptAnalyzer Check
    Write-StatusMessage "Checking PSScriptAnalyzer..."

    $psaModule = Get-Module -Name PSScriptAnalyzer -ListAvailable -ErrorAction SilentlyContinue
    if ($psaModule) {
        Write-StatusMessage "PSScriptAnalyzer: $($psaModule.Version)" "SUCCESS"

        # Test with a simple PowerShell 7 syntax
        $testCode = '$result = $true ? "works" : "failed"'
        try {
            $analysis = Invoke-ScriptAnalyzer -ScriptDefinition $testCode -ErrorAction SilentlyContinue
            if ($analysis | Where-Object { $_.RuleName -eq "ParseError" }) {
                Write-StatusMessage "PSScriptAnalyzer: Flagging PS7 syntax as errors" "WARNING"
                $recommendations += "Update PSScriptAnalyzer settings for PowerShell 7 compatibility"
            }
            else {
                Write-StatusMessage "PSScriptAnalyzer: PowerShell 7 compatible" "SUCCESS"
            }
        }
        catch {
            Write-StatusMessage "PSScriptAnalyzer test failed" "WARNING" $_.Exception.Message
        }
    }
    else {
        Write-StatusMessage "PSScriptAnalyzer: Not installed" "INFO" "Optional but recommended for code quality"
    }

    # 6. Summary and Recommendations
    Write-Host ""
    Write-Host "📊 ANALYSIS SUMMARY" -ForegroundColor Cyan
    Write-Host "===================" -ForegroundColor Cyan

    if ($issues.Count -eq 0) {
        Write-StatusMessage "Environment Status: Excellent" "SUCCESS" "All PowerShell 7.5.2 features available"
    }
    elseif ($issues.Count -le 2) {
        Write-StatusMessage "Environment Status: Good with minor issues" "WARNING" "$($issues.Count) issues found"
    }
    else {
        Write-StatusMessage "Environment Status: Needs attention" "ERROR" "$($issues.Count) critical issues found"
    }

    if ($issues.Count -gt 0) {
        Write-Host ""
        Write-Host "🔧 ISSUES FOUND:" -ForegroundColor Red
        $issues | ForEach-Object { Write-Host "   • $_" -ForegroundColor Yellow }
    }

    if ($recommendations.Count -gt 0) {
        Write-Host ""
        Write-Host "💡 RECOMMENDATIONS:" -ForegroundColor Green
        $recommendations | ForEach-Object { Write-Host "   • $_" -ForegroundColor Cyan }
    }

    return @{
        Issues = $issues
        Recommendations = $recommendations
        PSVersion = $psVersion
        EnvironmentScore = [math]::Max(0, 100 - ($issues.Count * 25))
    }
}

function Fix-VSCodePowerShellConfig {
    Write-Host "🔧 Fixing VS Code PowerShell Configuration..." -ForegroundColor Cyan

    if (-not (Test-Path ".vscode")) {
        New-Item -ItemType Directory -Path ".vscode" -Force | Out-Null
    }

    $settingsPath = ".vscode/settings.json"
    $settings = @{}

    if (Test-Path $settingsPath) {
        try {
            $settings = Get-Content $settingsPath | ConvertFrom-Json -AsHashtable
        }
        catch {
            Write-StatusMessage "Could not parse existing settings, creating new configuration" "WARNING"
            $settings = @{}
        }
    }

    # Find PowerShell 7 installation
    $pwshPaths = @(
        "C:\Program Files\PowerShell\7\pwsh.exe",
        "C:\Program Files\PowerShell\7-preview\pwsh.exe"
    )

    # Try to find pwsh command
    try {
        $pwshCommand = Get-Command pwsh -ErrorAction SilentlyContinue
        if ($pwshCommand) {
            $pwshPaths += $pwshCommand.Source
        }
    }
    catch {
        # Ignore if pwsh command not found
    }

    $pwshPaths = $pwshPaths | Where-Object { $_ -and (Test-Path $_) } | Select-Object -First 1

    if ($pwshPaths) {
        $pwshPath = $pwshPaths
        Write-StatusMessage "Found PowerShell 7 at: $pwshPath" "SUCCESS"

        # Update PowerShell settings
        $settings["powershell.powerShellDefaultVersion"] = "PowerShell 7.5.2"
        $settings["powershell.powerShellExePath"] = $pwshPath
        $settings["powershell.scriptAnalysis.enable"] = $true
        $settings["powershell.scriptAnalysis.settingsPath"] = "PSScriptAnalyzerSettings.psd1"

        # Update terminal profile
        if (-not $settings["terminal.integrated.profiles.windows"]) {
            $settings["terminal.integrated.profiles.windows"] = @{}
        }

        $settings["terminal.integrated.profiles.windows"]["PowerShell 7.5.2"] = @{
            path = $pwshPath
            args = @("-NoProfile")
            icon = "terminal-powershell"
        }

        $settings["terminal.integrated.defaultProfile.windows"] = "PowerShell 7.5.2"

        # Save settings
        $settings | ConvertTo-Json -Depth 10 | Set-Content $settingsPath
        Write-StatusMessage "VS Code configuration updated" "SUCCESS"
    }
    else {
        Write-StatusMessage "PowerShell 7 not found" "ERROR" "Please install PowerShell 7.5.2"
    }
}

function Test-ScriptSyntax {
    Write-Host "🧪 Testing PowerShell 7.5.2 Script Syntax..." -ForegroundColor Cyan

    # Test scripts in the project
    $scriptFiles = Get-ChildItem -Recurse -Include "*.ps1" -ErrorAction SilentlyContinue |
                   Where-Object { $_.FullName -notlike "*bin*" -and $_.FullName -notlike "*obj*" } |
                   Select-Object -First 5

    $syntaxIssues = @()

    foreach ($script in $scriptFiles) {
        Write-Host "Testing: $($script.Name)" -ForegroundColor Gray

        try {
            $tokens = $null
            $errors = $null
            $null = [System.Management.Automation.Language.Parser]::ParseFile(
                $script.FullName, [ref]$tokens, [ref]$errors
            )

            if ($errors.Count -gt 0) {
                $syntaxIssues += @{
                    File = $script.Name
                    Errors = $errors
                }
                Write-StatusMessage "$($script.Name): $($errors.Count) syntax errors" "ERROR"
            }
            else {
                Write-StatusMessage "$($script.Name): Clean" "SUCCESS"
            }
        }
        catch {
            Write-StatusMessage "$($script.Name): Parse failed" "ERROR" $_.Exception.Message
        }
    }

    return $syntaxIssues
}

# Main execution
Write-Host "🚌 BusBuddy PowerShell Environment Validator" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""

$result = Test-PowerShellEnvironment

if ($FixVSCodeConfig) {
    Write-Host ""
    Fix-VSCodePowerShellConfig
}

if ($ValidateSyntax) {
    Write-Host ""
    $syntaxResults = Test-ScriptSyntax

    if ($syntaxResults.Count -gt 0) {
        Write-Host ""
        Write-Host "📋 SYNTAX ISSUES DETAIL:" -ForegroundColor Red
        foreach ($issue in $syntaxResults) {
            Write-Host "File: $($issue.File)" -ForegroundColor Yellow
            foreach ($error in $issue.Errors) {
                Write-Host "   Line $($error.Extent.StartLineNumber): $($error.Message)" -ForegroundColor Red
            }
        }
    }
}

if ($DetailedReport) {
    Write-Host ""
    Write-Host "📊 DETAILED ENVIRONMENT REPORT" -ForegroundColor Cyan
    Write-Host "===============================" -ForegroundColor Cyan
    Write-Host "PowerShell Version: $($PSVersionTable.PSVersion)" -ForegroundColor White
    Write-Host "Host Name: $($Host.Name)" -ForegroundColor White
    Write-Host "Edition: $($PSVersionTable.PSEdition)" -ForegroundColor White
    Write-Host "OS: $($PSVersionTable.OS)" -ForegroundColor White
    Write-Host "Platform: $($PSVersionTable.Platform)" -ForegroundColor White
    Write-Host "Working Directory: $(Get-Location)" -ForegroundColor White
    Write-Host "Environment Score: $($result.EnvironmentScore)/100" -ForegroundColor White
}

Write-Host ""
Write-Host "🎯 QUICK FIXES:" -ForegroundColor Green
Write-Host "• Run in PowerShell 7.5.2: pwsh.exe (not powershell.exe)" -ForegroundColor Cyan
Write-Host "• Fix VS Code config: .\validate-powershell-environment.ps1 -FixVSCodeConfig" -ForegroundColor Cyan
Write-Host "• Test syntax: .\validate-powershell-environment.ps1 -ValidateSyntax" -ForegroundColor Cyan
Write-Host "• Full report: .\validate-powershell-environment.ps1 -DetailedReport" -ForegroundColor Cyan
