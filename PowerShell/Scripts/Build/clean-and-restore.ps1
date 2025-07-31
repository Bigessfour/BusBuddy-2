#Requires -Version 7.5

<#
.SYNOPSIS
BusBuddy Clean and Restore Script

.DESCRIPTION
Performs comprehensive clean and restore operations for the BusBuddy project.
Includes cleanup of build artifacts, temporary files, and restoration of NuGet packages.

.PARAMETER Force
Skip confirmation prompts

.PARAMETER Verbose
Enable verbose output

.PARAMETER SkipProfiles
Skip loading PowerShell profiles

.EXAMPLE
.\clean-and-restore.ps1

.EXAMPLE
.\clean-and-restore.ps1 -Force -Verbose
#>

param(
    [switch]$Force,
    [switch]$Verbose,
    [switch]$SkipProfiles
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

function Test-BusBuddyEnvironment {
    Write-StatusMessage "🔍 Validating BusBuddy environment..." -Level 'Info'

    # Check for solution file
    if (-not (Test-Path 'BusBuddy.sln')) {
        throw "BusBuddy.sln not found. Please run this script from the BusBuddy root directory."
    }

    # Check .NET SDK
    try {
        $dotnetVersion = dotnet --version 2>$null
        if ($LASTEXITCODE -ne 0) {
            throw ".NET SDK not found or not working"
        }
        Write-StatusMessage "✅ .NET SDK Version: $dotnetVersion" -Level 'Success'
    } catch {
        throw ".NET SDK validation failed: $($_.Exception.Message)"
    }

    # Check for essential project files
    $projectFiles = @('BusBuddy.Core\BusBuddy.Core.csproj', 'BusBuddy.WPF\BusBuddy.WPF.csproj')
    foreach ($file in $projectFiles) {
        if (-not (Test-Path $file)) {
            throw "Required project file not found: $file"
        }
    }

    Write-StatusMessage "✅ Environment validation passed" -Level 'Success'
}

function Invoke-ProjectClean {
    Write-StatusMessage "🧹 Starting project clean operation..." -Level 'Info'

    try {
        # Clean solution
        Write-StatusMessage "Cleaning solution..." -Level 'Info'
        dotnet clean BusBuddy.sln --verbosity $(if ($Verbose) { 'normal' } else { 'minimal' })

        if ($LASTEXITCODE -ne 0) {
            throw "dotnet clean failed with exit code: $LASTEXITCODE"
        }

        # Remove additional build artifacts
        $artifactPaths = @(
            'BusBuddy.Core\bin',
            'BusBuddy.Core\obj',
            'BusBuddy.WPF\bin',
            'BusBuddy.WPF\obj',
            'TestResults',
            'packages'
        )

        foreach ($path in $artifactPaths) {
            if (Test-Path $path) {
                Write-StatusMessage "Removing build artifacts: $path" -Level 'Info'
                Remove-Item $path -Recurse -Force -ErrorAction SilentlyContinue
            }
        }

        # Clean temporary files
        $tempFiles = Get-ChildItem -Recurse -Force | Where-Object {
            $_.Name -match '\.(tmp|temp|cache|log)$' -or
            $_.Name -match '_wpftmp' -or
            $_.Name -match '\.backup$'
        }

        if ($tempFiles) {
            Write-StatusMessage "Removing $($tempFiles.Count) temporary files..." -Level 'Info'
            $tempFiles | Remove-Item -Force -ErrorAction SilentlyContinue
        }

        Write-StatusMessage "✅ Clean operation completed successfully" -Level 'Success'

    } catch {
        Write-StatusMessage "❌ Clean operation failed: $($_.Exception.Message)" -Level 'Error'
        throw
    }
}

function Invoke-PackageRestore {
    Write-StatusMessage "📦 Starting package restore operation..." -Level 'Info'

    try {
        # Clear NuGet cache first
        Write-StatusMessage "Clearing NuGet cache..." -Level 'Info'
        dotnet nuget locals all --clear

        # Restore packages
        Write-StatusMessage "Restoring packages..." -Level 'Info'
        dotnet restore BusBuddy.sln --force --no-cache --verbosity $(if ($Verbose) { 'normal' } else { 'minimal' })

        if ($LASTEXITCODE -ne 0) {
            throw "dotnet restore failed with exit code: $LASTEXITCODE"
        }

        Write-StatusMessage "✅ Package restore completed successfully" -Level 'Success'

    } catch {
        Write-StatusMessage "❌ Package restore failed: $($_.Exception.Message)" -Level 'Error'
        throw
    }
}

function Test-ProjectIntegrity {
    Write-StatusMessage "🔍 Testing project integrity..." -Level 'Info'

    try {
        # Test build without actually building
        dotnet build BusBuddy.sln --verbosity quiet --no-restore --dry-run 2>$null

        if ($LASTEXITCODE -eq 0) {
            Write-StatusMessage "✅ Project integrity check passed" -Level 'Success'
        } else {
            Write-StatusMessage "⚠️ Project integrity check indicated potential issues" -Level 'Warning'
        }

    } catch {
        Write-StatusMessage "⚠️ Could not perform integrity check: $($_.Exception.Message)" -Level 'Warning'
    }
}

function Load-BusBuddyProfiles {
    if ($SkipProfiles) {
        Write-StatusMessage "Skipping PowerShell profile loading (SkipProfiles flag set)" -Level 'Info'
        return $false
    }

    Write-StatusMessage "🔧 Checking for BusBuddy PowerShell profiles..." -Level 'Info'

    if (Test-Path '.\load-bus-buddy-profiles.ps1') {
        try {
            Write-StatusMessage "Loading BusBuddy profiles..." -Level 'Info'
            & '.\load-bus-buddy-profiles.ps1' -ErrorAction Stop

            # Check if bb-* commands are available
            $bbCommands = Get-Command bb-* -ErrorAction SilentlyContinue
            if ($bbCommands) {
                Write-StatusMessage "✅ BusBuddy profiles loaded successfully ($($bbCommands.Count) commands available)" -Level 'Success'
                return $true
            } else {
                Write-StatusMessage "⚠️ Profiles loaded but bb-* commands not available" -Level 'Warning'
                return $false
            }

        } catch {
            Write-StatusMessage "❌ Error loading profiles: $($_.Exception.Message)" -Level 'Error'
            Write-StatusMessage "Continuing with standard operations..." -Level 'Info'
            return $false
        }
    } else {
        Write-StatusMessage "⚠️ load-bus-buddy-profiles.ps1 not found" -Level 'Warning'
        return $false
    }
}

# Main execution
try {
    Write-StatusMessage "🚌 BusBuddy Clean and Restore Script" -Level 'Info'
    Write-StatusMessage "PowerShell Version: $($PSVersionTable.PSVersion)" -Level 'Info'
    Write-StatusMessage "Working Directory: $PWD" -Level 'Info'
    Write-StatusMessage "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -Level 'Info'
    Write-StatusMessage "=" * 60 -Level 'Info'

    # Load profiles first (optional)
    $profilesLoaded = Load-BusBuddyProfiles

    # Validate environment
    Test-BusBuddyEnvironment

    # Confirm operation if not forced
    if (-not $Force) {
        Write-StatusMessage "`n⚠️ This will clean build artifacts and restore packages" -Level 'Warning'
        $confirm = Read-Host "Continue? (y/N)"
        if ($confirm -ne 'y' -and $confirm -ne 'Y') {
            Write-StatusMessage "❌ Operation cancelled by user" -Level 'Warning'
            exit 0
        }
    }

    # Perform clean operation
    Invoke-ProjectClean

    # Perform restore operation
    Invoke-PackageRestore

    # Test project integrity
    Test-ProjectIntegrity

    Write-StatusMessage "`n=" * 60 -Level 'Info'
    Write-StatusMessage "✅ Clean and restore operations completed successfully!" -Level 'Success'

    # Show next steps
    Write-StatusMessage "`n💡 Recommended next steps:" -Level 'Info'
    if ($profilesLoaded) {
        Write-StatusMessage "   bb-build                    # Build using BusBuddy profile" -Level 'Info'
        Write-StatusMessage "   bb-run                      # Run application" -Level 'Info'
    } else {
        Write-StatusMessage "   dotnet build BusBuddy.sln   # Build solution" -Level 'Info'
        Write-StatusMessage "   dotnet run --project BusBuddy.WPF\\BusBuddy.WPF.csproj   # Run application" -Level 'Info'
    }

} catch {
    Write-StatusMessage "`n❌ Script failed: $($_.Exception.Message)" -Level 'Error'
    Write-StatusMessage "PowerShell Version: $($PSVersionTable.PSVersion)" -Level 'Error'
    Write-StatusMessage "Working Directory: $PWD" -Level 'Error'
    exit 1
}
