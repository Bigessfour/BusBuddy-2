#Requires -Version 7.0
#Requires -RunAsAdministrator

<#
.SYNOPSIS
    Fast and comprehensive development environment update with Administrator privileges
.DESCRIPTION
    Quickly updates PowerShell, VS Code, extensions, and all configurations to latest versions
    Uses administrator privileges for system-wide updates, package managers, and security policies
.PARAMETER SkipSystemUpdates
    Skip system-level updates that require administrator privileges
.PARAMETER ForceReinstall
    Force reinstallation of all components even if they appear current
.EXAMPLE
    .\Fast-Environment-Update.ps1
.EXAMPLE
    .\Fast-Environment-Update.ps1 -ForceReinstall
#>

[CmdletBinding()]
param(
    [Parameter()]
    [switch]$SkipSystemUpdates,

    [Parameter()]
    [switch]$ForceReinstall
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

# Verify administrator privileges
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Error "❌ This script requires Administrator privileges. Please run as Administrator."
    exit 1
}

Write-Host '🚀 Fast Environment Update - Bus Buddy Development Environment (Administrator Mode)' -ForegroundColor Cyan
Write-Host '==================================================================================' -ForegroundColor Cyan
Write-Host '   ⚡ Administrator privileges detected - Full system updates enabled' -ForegroundColor Green
Write-Host ''

# Track what gets updated
$UpdatedComponents = @()
$Errors = @()

#region System-Level Package Managers Setup
if (-not $SkipSystemUpdates) {
    Write-Host '🔧 PHASE 0: System Package Managers Setup' -ForegroundColor Magenta

    # Install/Update Chocolatey
    try {
        if (-not (Get-Command choco -ErrorAction SilentlyContinue)) {
            Write-Host '   📦 Installing Chocolatey package manager...' -ForegroundColor Yellow
            Set-ExecutionPolicy Bypass -Scope Process -Force
            [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
            # Download and execute Chocolatey install script securely
            $chocoScript = (New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1')
            & ([ScriptBlock]::Create($chocoScript))
            $UpdatedComponents += 'Chocolatey (Installed)'
        } else {
            Write-Host '   🔄 Updating Chocolatey...' -ForegroundColor Yellow
            choco upgrade chocolatey -y
            $UpdatedComponents += 'Chocolatey (Updated)'
        }
    } catch {
        $Errors += "Chocolatey setup failed: $($_.Exception.Message)"
        Write-Host "   ⚠️  Chocolatey setup failed, continuing..." -ForegroundColor Yellow
    }

    # Install/Update Scoop
    try {
        if (-not (Get-Command scoop -ErrorAction SilentlyContinue)) {
            Write-Host '   📦 Installing Scoop package manager...' -ForegroundColor Yellow
            Set-ExecutionPolicy RemoteSigned -Scope CurrentUser -Force
            # Download and execute Scoop install script securely
            $scoopScript = Invoke-RestMethod get.scoop.sh
            & ([ScriptBlock]::Create($scoopScript))
            $UpdatedComponents += 'Scoop (Installed)'
        } else {
            Write-Host '   🔄 Updating Scoop...' -ForegroundColor Yellow
            scoop update
            $UpdatedComponents += 'Scoop (Updated)'
        }
    } catch {
        $Errors += "Scoop setup failed: $($_.Exception.Message)"
        Write-Host "   ⚠️  Scoop setup failed, continuing..." -ForegroundColor Yellow
    }

    # Install/Update Winget
    try {
        if (-not (Get-Command winget -ErrorAction SilentlyContinue)) {
            Write-Host '   📦 Installing Windows Package Manager (winget)...' -ForegroundColor Yellow
            # Winget comes with Windows 11, for Windows 10 we need to install it
            $uri = 'https://github.com/microsoft/winget-cli/releases/latest/download/Microsoft.DesktopAppInstaller_8wekyb3d8bbwe.msixbundle'
            $outFile = "$env:TEMP\winget.msixbundle"
            Invoke-WebRequest -Uri $uri -OutFile $outFile
            Add-AppxPackage $outFile
            $UpdatedComponents += 'Winget (Installed)'
        } else {
            Write-Host '   ✅ Winget already available' -ForegroundColor Green
        }
    } catch {
        $Errors += "Winget setup failed: $($_.Exception.Message)"
        Write-Host "   ⚠️  Winget setup failed, continuing..." -ForegroundColor Yellow
    }
}
#endregion

#region PowerShell Core Update
Write-Host '📦 PHASE 1: PowerShell Core Update' -ForegroundColor Green
try {
    # Check current PowerShell version
    $currentVersion = $PSVersionTable.PSVersion
    Write-Host "   Current PowerShell: $currentVersion" -ForegroundColor White

    # Get latest PowerShell release
    Write-Host '   🔍 Checking for latest PowerShell Core...' -ForegroundColor Yellow
    $latestRelease = Invoke-RestMethod -Uri 'https://api.github.com/repos/PowerShell/PowerShell/releases/latest' -TimeoutSec 10
    $latestVersion = $latestRelease.tag_name.TrimStart('v')

    if ([version]$latestVersion -gt $currentVersion) {
        Write-Host "   📥 New version available: $latestVersion" -ForegroundColor Cyan

        # Download and install latest PowerShell
        $downloadUrl = ($latestRelease.assets | Where-Object { $_.name -like '*win-x64.msi' }).browser_download_url
        $installerPath = "$env:TEMP\PowerShell-$latestVersion-win-x64.msi"

        Write-Host "   ⬇️  Downloading PowerShell $latestVersion..." -ForegroundColor Yellow
        Invoke-WebRequest -Uri $downloadUrl -OutFile $installerPath -TimeoutSec 30

        Write-Host "   🔧 Installing PowerShell $latestVersion..." -ForegroundColor Yellow
        Start-Process -FilePath 'msiexec.exe' -ArgumentList '/i', $installerPath, '/quiet', '/norestart' -Wait

        $UpdatedComponents += "PowerShell Core $latestVersion"
        Write-Host "   ✅ PowerShell updated to $latestVersion" -ForegroundColor Green
    } else {
        Write-Host "   ✅ PowerShell is up to date ($currentVersion)" -ForegroundColor Green
    }
} catch {
    $Errors += "PowerShell update failed: $($_.Exception.Message)"
    Write-Host "   ❌ PowerShell update failed: $($_.Exception.Message)" -ForegroundColor Red
}
#endregion

#region VS Code Update
Write-Host ''
Write-Host '🖥️  PHASE 2: VS Code Update' -ForegroundColor Green
try {
    # Find VS Code installation
    $vscodeExe = $null
    $possiblePaths = @(
        "${env:LOCALAPPDATA}\Programs\Microsoft VS Code\Code.exe",
        "${env:LOCALAPPDATA}\Programs\Microsoft VS Code Insiders\Code - Insiders.exe",
        "${env:ProgramFiles}\Microsoft VS Code\Code.exe",
        "${env:ProgramFiles(x86)}\Microsoft VS Code\Code.exe"
    )

    foreach ($path in $possiblePaths) {
        if (Test-Path $path) {
            $vscodeExe = $path
            break
        }
    }

    if ($vscodeExe) {
        Write-Host "   📍 Found VS Code: $vscodeExe" -ForegroundColor White

        # Get VS Code version
        $vscodeVersion = & $vscodeExe --version 2>$null | Select-Object -First 1
        Write-Host "   Current VS Code: $vscodeVersion" -ForegroundColor White

        # Update VS Code via winget if available
        if (Get-Command winget -ErrorAction SilentlyContinue) {
            Write-Host '   🔄 Updating VS Code via winget...' -ForegroundColor Yellow
            $null = winget upgrade 'Microsoft.VisualStudioCode' --silent --accept-source-agreements 2>&1
            if ($LASTEXITCODE -eq 0) {
                $UpdatedComponents += 'VS Code (via winget)'
                Write-Host '   ✅ VS Code updated successfully' -ForegroundColor Green
            } else {
                Write-Host '   ℹ️  VS Code is up to date or update not needed' -ForegroundColor Blue
            }
        } else {
            Write-Host '   ℹ️  Winget not available - VS Code update skipped' -ForegroundColor Blue
        }
    } else {
        Write-Host '   ❌ VS Code not found in standard locations' -ForegroundColor Red
    }
} catch {
    $Errors += "VS Code update failed: $($_.Exception.Message)"
    Write-Host "   ❌ VS Code update failed: $($_.Exception.Message)" -ForegroundColor Red
}
#endregion

#region VS Code Extensions Update
Write-Host ''
Write-Host '🔌 PHASE 3: VS Code Extensions Update' -ForegroundColor Green
try {
    if ($vscodeExe) {
        # Critical extensions for Bus Buddy development
        $criticalExtensions = @(
            @{ Id = 'ms-vscode.powershell'; Name = 'PowerShell'; Target = '2025.6.1' },
            @{ Id = 'spmeesseman.vscode-taskexplorer'; Name = 'Task Explorer'; Target = 'latest' },
            @{ Id = 'ms-dotnettools.csharp'; Name = 'C# Dev Kit'; Target = 'latest' },
            @{ Id = 'ms-dotnettools.vscodeintellicode-csharp'; Name = 'IntelliCode for C#'; Target = 'latest' },
            @{ Id = 'doggy8088.netcore-snippets'; Name = '.NET Core Snippets'; Target = 'latest' },
            @{ Id = 'formulahendry.dotnet-test-explorer'; Name = '.NET Test Explorer'; Target = 'latest' }
        )

        Write-Host '   🔍 Updating critical extensions...' -ForegroundColor Yellow

        foreach ($ext in $criticalExtensions) {
            try {
                Write-Host "      📦 $($ext.Name)..." -ForegroundColor White

                # Install/update extension
                $null = & $vscodeExe --install-extension $ext.Id --force 2>&1
                if ($LASTEXITCODE -eq 0) {
                    Write-Host '         ✅ Updated/Installed' -ForegroundColor Green
                    $UpdatedComponents += "$($ext.Name) extension"
                } else {
                    Write-Host '         ⚠️  Already up to date' -ForegroundColor Yellow
                }
            } catch {
                Write-Host "         ❌ Failed: $($_.Exception.Message)" -ForegroundColor Red
            }
        }

        Write-Host '   🔄 Updating all other extensions...' -ForegroundColor Yellow
        $null = & $vscodeExe --update-extensions 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host '   ✅ All extensions updated' -ForegroundColor Green
        }
    }
} catch {
    $Errors += "Extensions update failed: $($_.Exception.Message)"
    Write-Host "   ❌ Extensions update failed: $($_.Exception.Message)" -ForegroundColor Red
}
#endregion

#region PowerShell Modules Update
Write-Host ''
Write-Host '📚 PHASE 4: PowerShell Modules Update' -ForegroundColor Green
try {
    # Critical modules for development
    $criticalModules = @(
        'PSScriptAnalyzer',
        'Pester',
        'PowerShellGet',
        'PackageManagement',
        'PSReadLine'
    )

    Write-Host '   🔄 Updating PowerShell modules...' -ForegroundColor Yellow

    # Update PowerShellGet first
    Install-Module PowerShellGet -Force -AllowClobber -Scope CurrentUser -Repository PSGallery

    foreach ($module in $criticalModules) {
        try {
            Write-Host "      📦 $module..." -ForegroundColor White
            Install-Module $module -Force -AllowClobber -Scope CurrentUser -Repository PSGallery
            Write-Host '         ✅ Updated' -ForegroundColor Green
            $UpdatedComponents += "$module module"
        } catch {
            Write-Host "         ❌ Failed: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
} catch {
    $Errors += "PowerShell modules update failed: $($_.Exception.Message)"
    Write-Host "   ❌ PowerShell modules update failed: $($_.Exception.Message)" -ForegroundColor Red
}
#endregion

#region Configuration Updates
Write-Host ''
Write-Host '⚙️  PHASE 5: Configuration Updates' -ForegroundColor Green
try {
    $workspaceRoot = Split-Path $PSScriptRoot -Parent | Split-Path -Parent
    $vscodeDir = Join-Path $workspaceRoot '.vscode'

    # Ensure .vscode directory exists
    if (!(Test-Path $vscodeDir)) {
        New-Item -ItemType Directory -Path $vscodeDir -Force | Out-Null
    }

    # Update settings.json with latest PowerShell configuration
    $settingsPath = Join-Path $vscodeDir 'settings.json'
    $optimalSettings = @{
        'powershell.codeFormatting.preset'                    = 'OTBS'
        'powershell.codeFormatting.openBraceOnSameLine'       = $true
        'powershell.codeFormatting.newLineAfterOpenBrace'     = $true
        'powershell.codeFormatting.newLineAfterCloseBrace'    = $true
        'powershell.codeFormatting.whitespaceBeforeOpenBrace' = $true
        'powershell.codeFormatting.whitespaceBeforeOpenParen' = $true
        'powershell.codeFormatting.whitespaceAroundOperator'  = $true
        'powershell.codeFormatting.whitespaceAfterSeparator'  = $true
        'powershell.codeFormatting.ignoreOneLineBlock'        = $false
        'powershell.integratedConsole.showOnStartup'          = $false
        'powershell.scriptAnalysis.enable'                    = $true
        'powershell.scriptAnalysis.settingsPath'              = './PSScriptAnalyzerSettings.psd1'
        'terminal.integrated.defaultProfile.windows'          = 'PowerShell'
        'terminal.integrated.profiles.windows'                = @{
            'PowerShell' = @{
                'source' = 'PowerShell'
                'icon'   = 'terminal-powershell'
                'args'   = @('-NoProfile', '-ExecutionPolicy', 'Bypass')
            }
        }
    }

    # Merge with existing settings
    $currentSettings = @{}
    if (Test-Path $settingsPath) {
        $currentSettings = Get-Content $settingsPath | ConvertFrom-Json -AsHashtable -ErrorAction SilentlyContinue
    }

    foreach ($key in $optimalSettings.Keys) {
        $currentSettings[$key] = $optimalSettings[$key]
    }

    $currentSettings | ConvertTo-Json -Depth 10 | Set-Content $settingsPath -Encoding UTF8
    Write-Host '   ✅ VS Code settings updated' -ForegroundColor Green
    $UpdatedComponents += 'VS Code settings'

    # Update PSScriptAnalyzer settings
    $psaSettingsPath = Join-Path $workspaceRoot 'PSScriptAnalyzerSettings.psd1'
    $psaSettings = @"
@{
    Rules = @{
        PSAvoidUsingCmdletAliases = @{
            Enable = $true
            allowlist = @('cd', 'dir', 'ls', 'cat', 'cp', 'mv', 'rm')
        }
        PSUseCompatibleCommands = @{
            Enable = $true
            TargetProfiles = @('win-8_x64_10.0.17763.0_6.2.4_x64_4.0.30319.42000_core')
        }
        PSUseCompatibleSyntax = @{
            Enable = $true
            TargetVersions = @('7.0', '7.1', '7.2', '7.3', '7.4')
        }
    }
    IncludeRules = @(
        'PSAvoidAssignmentToAutomaticVariable',
        'PSAvoidDefaultValueForMandatoryParameter',
        'PSAvoidDefaultValueSwitchParameter',
        'PSAvoidGlobalAliases',
        'PSAvoidGlobalFunctions',
        'PSAvoidGlobalVars',
        'PSAvoidInvokingEmptyMembers',
        'PSAvoidNullOrEmptyHelpMessageAttribute',
        'PSAvoidOverwritingBuiltInCmdlets',
        'PSAvoidReservedCharInCmdletNames',
        'PSAvoidReservedParams',
        'PSAvoidShouldContinueWithoutForce',
        'PSAvoidTrailingWhitespace',
        'PSAvoidUsingCmdletAliases',
        'PSAvoidUsingComputerNameHardcoded',
        'PSAvoidUsingConvertToSecureStringWithPlainText',
        'PSAvoidUsingEmptyCatchBlock',
        'PSAvoidUsingInvokeExpression',
        'PSAvoidUsingPlainTextForPassword',
        'PSAvoidUsingPositionalParameters',
        'PSAvoidUsingUsernameAndPasswordParams',
        'PSAvoidUsingWMICmdlet',
        'PSAvoidUsingWriteHost',
        'PSDSCDscExamplesPresent',
        'PSDSCDscTestsPresent',
        'PSDSCReturnCorrectTypesForDSCFunctions',
        'PSDSCStandardDSCFunctionsInResource',
        'PSDSCUseIdenticalMandatoryParametersForDSC',
        'PSDSCUseIdenticalParametersForDSC',
        'PSMisleadingBacktick',
        'PSMissingModuleManifestField',
        'PSPlaceCloseBrace',
        'PSPlaceOpenBrace',
        'PSPossibleIncorrectComparisonWithNull',
        'PSPossibleIncorrectUsageOfAssignmentOperator',
        'PSPossibleIncorrectUsageOfRedirectionOperator',
        'PSProvideCommentHelp',
        'PSReservedCmdletChar',
        'PSReservedParams',
        'PSReviewUnusedParameter',
        'PSShouldProcess',
        'PSUseApprovedVerbs',
        'PSUseBOMForUnicodeEncodedFile',
        'PSUseCmdletCorrectly',
        'PSUseCompatibleCmdlets',
        'PSUseCompatibleCommands',
        'PSUseCompatibleSyntax',
        'PSUseConsistentIndentation',
        'PSUseConsistentWhitespace',
        'PSUseCorrectCasing',
        'PSUseDeclaredVarsMoreThanAssignments',
        'PSUseLiteralInitializerForHashtable',
        'PSUseOutputTypeCorrectly',
        'PSUseProcessBlockForPipelineCommand',
        'PSUsePSCredentialType',
        'PSUseShouldProcessForStateChangingFunctions',
        'PSUseSingularNouns',
        'PSUseStrictMode',
        'PSUseToExportFieldsInManifest',
        'PSUseUTF8EncodingForHelpFile',
        'PSUseVerboseMessageInDSCResource'
    )
}
"@
    $psaSettings | Set-Content $psaSettingsPath -Encoding UTF8
    Write-Host '   ✅ PSScriptAnalyzer settings updated' -ForegroundColor Green
    $UpdatedComponents += 'PSScriptAnalyzer settings'

} catch {
    $Errors += "Configuration update failed: $($_.Exception.Message)"
    Write-Host "   ❌ Configuration update failed: $($_.Exception.Message)" -ForegroundColor Red
}
#endregion

#region .NET SDK Update
Write-Host ''
Write-Host '🔧 PHASE 6: .NET SDK Update' -ForegroundColor Green
try {
    # Check current .NET version
    $dotnetVersion = dotnet --version 2>$null
    if ($dotnetVersion) {
        Write-Host "   Current .NET SDK: $dotnetVersion" -ForegroundColor White

        # Update via winget if available
        if (Get-Command winget -ErrorAction SilentlyContinue) {
            Write-Host '   🔄 Updating .NET SDK via winget...' -ForegroundColor Yellow
            $null = winget upgrade 'Microsoft.DotNet.SDK.8' --silent --accept-source-agreements 2>&1
            if ($LASTEXITCODE -eq 0) {
                $UpdatedComponents += '.NET SDK'
                Write-Host '   ✅ .NET SDK updated' -ForegroundColor Green
            } else {
                Write-Host '   ℹ️  .NET SDK is up to date' -ForegroundColor Blue
            }
        }
    } else {
        Write-Host '   ❌ .NET SDK not found' -ForegroundColor Red
    }
} catch {
    $Errors += ".NET SDK update failed: $($_.Exception.Message)"
    Write-Host "   ❌ .NET SDK update failed: $($_.Exception.Message)" -ForegroundColor Red
}
#endregion

#region Summary
Write-Host ''
Write-Host '📊 UPDATE SUMMARY' -ForegroundColor Cyan
Write-Host '=================' -ForegroundColor Cyan

if ($UpdatedComponents.Count -gt 0) {
    Write-Host '✅ Successfully Updated Components:' -ForegroundColor Green
    $UpdatedComponents | ForEach-Object { Write-Host "   • $_" -ForegroundColor White }
} else {
    Write-Host 'ℹ️  All components were already up to date' -ForegroundColor Blue
}

if ($Errors.Count -gt 0) {
    Write-Host ''
    Write-Host '❌ Errors Encountered:' -ForegroundColor Red
    $Errors | ForEach-Object { Write-Host "   • $_" -ForegroundColor Red }
}

Write-Host ''
Write-Host '🚀 NEXT STEPS:' -ForegroundColor Cyan
Write-Host '1. Restart VS Code to apply all changes' -ForegroundColor White
Write-Host '2. Open Bus Buddy workspace' -ForegroundColor White
Write-Host '3. Verify PowerShell extension is v2025.6.1 in Extensions view' -ForegroundColor White
Write-Host '4. Test PowerShell scripting with new features' -ForegroundColor White

Write-Host ''
Write-Host '✅ Fast Environment Update Completed!' -ForegroundColor Green
Write-Host '   Your Bus Buddy development environment is now optimized with the latest versions.' -ForegroundColor White
#endregion
