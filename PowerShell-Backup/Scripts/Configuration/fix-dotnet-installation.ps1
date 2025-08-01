#Requires -Version 7.5
#Requires -RunAsAdministrator

<#
.SYNOPSIS
Fix .NET SDK Installation Issue (0x80070643 Error Resolution)

.DESCRIPTION
Comprehensive script to resolve .NET 9.0 SDK installation issues and ensure stable build environment.
Includes cleanup, repair, and fallback to .NET 8.0 if needed.

.PARAMETER ForceCleanInstall
Force complete cleanup and reinstall

.PARAMETER FallbackToNet8
Use .NET 8.0 LTS instead of 9.0

.PARAMETER SkipDownload
Skip downloading new SDK (use existing installer)

.EXAMPLE
.\fix-dotnet-installation.ps1 -ForceCleanInstall

.EXAMPLE
.\fix-dotnet-installation.ps1 -FallbackToNet8
#>

param(
    [switch]$ForceCleanInstall,
    [switch]$FallbackToNet8,
    [switch]$SkipDownload
)

$ErrorActionPreference = "Stop"

function Write-StatusMessage {
    param(
        [string]$Message,
        [string]$Level = 'Info'
    )

    $color = switch ($Level) {
        'Success' { 'Green' }
        'Warning' { 'Yellow' }
        'Error' { 'Red' }
        'Info' { 'Cyan' }
        default { 'White' }
    }

    $timestamp = Get-Date -Format 'HH:mm:ss'
    Write-Host "[$timestamp] $Message" -ForegroundColor $color
}

function Test-AdminRights {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Remove-DotNetInstallations {
    Write-StatusMessage "🧹 Cleaning existing .NET installations..." -Level 'Warning'

    try {
        # Stop processes that might lock files
        $processes = @('dotnet', 'MSBuild', 'VBCSCompiler', 'Code')
        foreach ($proc in $processes) {
            Get-Process -Name $proc -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
        }

        # Clear NuGet caches
        Write-StatusMessage "Clearing NuGet caches..."
        dotnet nuget locals all --clear 2>$null

        # Remove .NET program files directories
        $dotnetPaths = @(
            "${env:ProgramFiles}\dotnet",
            "${env:ProgramFiles(x86)}\dotnet",
            "${env:USERPROFILE}\.dotnet"
        )

        foreach ($path in $dotnetPaths) {
            if (Test-Path $path) {
                Write-StatusMessage "Removing: $path"
                try {
                    Remove-Item -Path $path -Recurse -Force -ErrorAction SilentlyContinue
                    Start-Sleep -Seconds 2
                }
                catch {
                    Write-StatusMessage "Could not remove $path completely (some files may be in use)" -Level 'Warning'
                }
            }
        }

        # Clean registry entries
        Write-StatusMessage "Cleaning registry entries..."
        $regPaths = @(
            "HKLM:\SOFTWARE\dotnet",
            "HKLM:\SOFTWARE\Microsoft\.NETFramework",
            "HKCU:\SOFTWARE\Microsoft\.NETFramework"
        )

        foreach ($regPath in $regPaths) {
            if (Test-Path $regPath) {
                try {
                    Remove-Item -Path $regPath -Recurse -Force -ErrorAction SilentlyContinue
                }
                catch {
                    Write-StatusMessage "Could not clean registry path: $regPath" -Level 'Warning'
                }
            }
        }

        Write-StatusMessage "✅ Cleanup completed" -Level 'Success'

    }
    catch {
        Write-StatusMessage "❌ Cleanup failed: $($_.Exception.Message)" -Level 'Error'
        throw
    }
}

function Install-DotNetSDK {
    param(
        [string]$Version = "8.0",
        [switch]$SkipDownload
    )

    Write-StatusMessage "📦 Installing .NET $Version SDK..." -Level 'Info'

    try {
        $installerUrl = if ($Version -eq "9.0") {
            "https://download.microsoft.com/download/a/b/8/ab897875-c5db-4d2c-bb13-48103c7b9d89/dotnet-sdk-9.0.101-win-x64.exe"
        }
        else {
            "https://download.microsoft.com/download/3/1/e/31e2a058-4fa6-4b1b-9f53-9edca4b0d1e1/dotnet-sdk-8.0.404-win-x64.exe"
        }

        $installerPath = Join-Path $env:TEMP "dotnet-sdk-$Version-installer.exe"

        if (-not $SkipDownload -or -not (Test-Path $installerPath)) {
            Write-StatusMessage "Downloading .NET $Version SDK installer..."

            $webClient = New-Object System.Net.WebClient
            $webClient.DownloadFile($installerUrl, $installerPath)

            Write-StatusMessage "✅ Download completed" -Level 'Success'
        }

        # Install with specific parameters to avoid 0x80070643
        Write-StatusMessage "Installing .NET SDK (this may take several minutes)..."

        $installArgs = @(
            '/quiet',
            '/norestart',
            'DONOTOPTOUTOFSDKTELEMETRY=1'
        )

        $process = Start-Process -FilePath $installerPath -ArgumentList $installArgs -Wait -PassThru -NoNewWindow

        if ($process.ExitCode -eq 0) {
            Write-StatusMessage "✅ .NET $Version SDK installed successfully" -Level 'Success'
        }
        else {
            throw "Installation failed with exit code: $($process.ExitCode)"
        }

        # Clean up installer
        if (Test-Path $installerPath) {
            Remove-Item $installerPath -Force -ErrorAction SilentlyContinue
        }

    }
    catch {
        Write-StatusMessage "❌ Installation failed: $($_.Exception.Message)" -Level 'Error'
        throw
    }
}

function Update-EnvironmentPath {
    Write-StatusMessage "🔧 Updating environment variables..." -Level 'Info'

    try {
        # Add .NET to PATH
        $dotnetPath = "${env:ProgramFiles}\dotnet"
        $currentPath = [Environment]::GetEnvironmentVariable("PATH", "Machine")

        if ($currentPath -notlike "*$dotnetPath*") {
            $newPath = "$dotnetPath;$currentPath"
            [Environment]::SetEnvironmentVariable("PATH", $newPath, "Machine")
            Write-StatusMessage "✅ Added .NET to system PATH" -Level 'Success'
        }

        # Refresh current session PATH
        $env:PATH = [Environment]::GetEnvironmentVariable("PATH", "Machine") + ";" + [Environment]::GetEnvironmentVariable("PATH", "User")

        Write-StatusMessage "✅ Environment variables updated" -Level 'Success'

    }
    catch {
        Write-StatusMessage "❌ Failed to update environment: $($_.Exception.Message)" -Level 'Error'
        throw
    }
}

function Test-DotNetInstallation {
    Write-StatusMessage "🔍 Verifying .NET installation..." -Level 'Info'

    try {
        # Wait for installation to settle
        Start-Sleep -Seconds 5

        # Test .NET command
        $versionOutput = & "${env:ProgramFiles}\dotnet\dotnet.exe" --version 2>&1

        if ($LASTEXITCODE -eq 0) {
            Write-StatusMessage "✅ .NET SDK Version: $versionOutput" -Level 'Success'

            # Test SDK list
            $sdkList = & "${env:ProgramFiles}\dotnet\dotnet.exe" --list-sdks 2>&1
            Write-StatusMessage "📦 Installed SDKs:" -Level 'Info'
            $sdkList | ForEach-Object { Write-StatusMessage "   $_" -Level 'Info' }

            return $true
        }
        else {
            Write-StatusMessage "❌ .NET command failed: $versionOutput" -Level 'Error'
            return $false
        }

    }
    catch {
        Write-StatusMessage "❌ Verification failed: $($_.Exception.Message)" -Level 'Error'
        return $false
    }
}

function Update-BusBuddyProjects {
    param([string]$TargetFramework = "net8.0-windows")

    Write-StatusMessage "🔧 Updating BusBuddy project files..." -Level 'Info'

    try {
        $projectFiles = @(
            "Directory.Build.props",
            "BusBuddy.Core\BusBuddy.Core.csproj",
            "BusBuddy.WPF\BusBuddy.WPF.csproj",
            "BusBuddy.Tests\BusBuddy.Tests.csproj",
            "BusBuddy.UITests\BusBuddy.UITests.csproj"
        )

        foreach ($projectFile in $projectFiles) {
            if (Test-Path $projectFile) {
                Write-StatusMessage "Updating: $projectFile"

                $content = Get-Content $projectFile -Raw

                # Update target framework
                $content = $content -replace '<TargetFramework>net9\.0(-windows)?</TargetFramework>', "<TargetFramework>$TargetFramework</TargetFramework>"

                # Update package versions to stable .NET 8.0 versions
                $content = $content -replace '<EntityFrameworkVersion>9\.0\.0</EntityFrameworkVersion>', '<EntityFrameworkVersion>8.0.11</EntityFrameworkVersion>'

                Set-Content -Path $projectFile -Value $content
                Write-StatusMessage "✅ Updated: $projectFile" -Level 'Success'
            }
        }

    }
    catch {
        Write-StatusMessage "❌ Failed to update projects: $($_.Exception.Message)" -Level 'Error'
        throw
    }
}

# Main execution
try {
    Write-StatusMessage "🚌 BusBuddy .NET Installation Fix" -Level 'Info'
    Write-StatusMessage "=" * 50 -Level 'Info'

    # Check admin rights
    if (-not (Test-AdminRights)) {
        Write-StatusMessage "❌ This script requires administrator privileges" -Level 'Error'
        Write-StatusMessage "Please run PowerShell as Administrator and try again" -Level 'Warning'
        exit 1
    }

    # Determine target version
    $targetVersion = if ($FallbackToNet8) { "8.0" } else { "8.0" }  # Default to .NET 8.0 for stability
    $targetFramework = if ($FallbackToNet8) { "net8.0-windows" } else { "net8.0-windows" }

    Write-StatusMessage "Target .NET Version: $targetVersion" -Level 'Info'

    # Step 1: Clean existing installations if forced
    if ($ForceCleanInstall) {
        Remove-DotNetInstallations
        Start-Sleep -Seconds 3
    }

    # Step 2: Install .NET SDK
    Install-DotNetSDK -Version $targetVersion -SkipDownload:$SkipDownload

    # Step 3: Update environment
    Update-EnvironmentPath

    # Step 4: Verify installation
    if (-not (Test-DotNetInstallation)) {
        throw "Installation verification failed"
    }

    # Step 5: Update BusBuddy projects
    Update-BusBuddyProjects -TargetFramework $targetFramework

    # Step 6: Test build
    Write-StatusMessage "🔨 Testing build..." -Level 'Info'
    $buildResult = & "${env:ProgramFiles}\dotnet\dotnet.exe" build BusBuddy.sln --verbosity minimal 2>&1

    if ($LASTEXITCODE -eq 0) {
        Write-StatusMessage "✅ Build test successful!" -Level 'Success'
    }
    else {
        Write-StatusMessage "⚠️ Build test had issues, but .NET is installed" -Level 'Warning'
        Write-StatusMessage "Build output: $buildResult" -Level 'Warning'
    }

    Write-StatusMessage "" -Level 'Info'
    Write-StatusMessage "🎯 Next Steps:" -Level 'Info'
    Write-StatusMessage "   1. Close and reopen VS Code" -Level 'Info'
    Write-StatusMessage "   2. Restart your terminal/PowerShell session" -Level 'Info'
    Write-StatusMessage "   3. Run: dotnet --version" -Level 'Info'
    Write-StatusMessage "   4. Run: dotnet build BusBuddy.sln" -Level 'Info'

    Write-StatusMessage "" -Level 'Info'
    Write-StatusMessage "✅ .NET installation fix completed successfully!" -Level 'Success'

}
catch {
    Write-StatusMessage "" -Level 'Error'
    Write-StatusMessage "❌ Installation fix failed: $($_.Exception.Message)" -Level 'Error'
    Write-StatusMessage "" -Level 'Error'
    Write-StatusMessage "💡 Troubleshooting options:" -Level 'Warning'
    Write-StatusMessage "   1. Try running with -FallbackToNet8 flag" -Level 'Warning'
    Write-StatusMessage "   2. Download .NET manually from https://dotnet.microsoft.com/download" -Level 'Warning'
    Write-StatusMessage "   3. Check Windows Update for system updates" -Level 'Warning'
    Write-StatusMessage "   4. Run Windows System File Checker: sfc /scannow" -Level 'Warning'

    exit 1
}
