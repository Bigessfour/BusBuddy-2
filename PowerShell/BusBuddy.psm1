# 🚌 BusBuddy.psm1 - Lean PowerShell Automation for BusBuddy
# Fast, simple functions for build, run, and development workflow

#Requires -Version 7.5

# Helper: Ensure we're in repo root
function Test-RepoRoot {
    if (-not (Test-Path "BusBuddy.sln")) {
        Write-Error "❌ Not in BusBuddy repo root! Navigate to the directory with BusBuddy.sln."
        return $false
    }
    return $true
}

# Build function with approved verb
function Invoke-BusBuddyBuild {
    <#
    .SYNOPSIS
    Builds the BusBuddy solution
    #>
    [CmdletBinding()]
    param()

    if (-not (Test-RepoRoot)) { return }
    Write-Host "🔨 Building BusBuddy..."
    try {
        dotnet build BusBuddy.sln --configuration Release --no-restore --verbosity minimal
        Write-Host "✅ Build complete!"
    } catch {
        Write-Error "❌ Build failed: $_"
    }
}

# Run function with approved verb
function Start-BusBuddyApp {
    <#
    .SYNOPSIS
    Runs the BusBuddy WPF application
    #>
    [CmdletBinding()]
    param()

    if (-not (Test-RepoRoot)) { return }
    Write-Host "🚀 Running BusBuddy..."
    try {
        dotnet run --project BusBuddy.csproj
    } catch {
        Write-Error "❌ Run failed: $_"
    }
}

# Test function with approved verb
function Invoke-BusBuddyTest {
    <#
    .SYNOPSIS
    Runs tests for the BusBuddy solution
    #>
    [CmdletBinding()]
    param()

    if (-not (Test-RepoRoot)) { return }
    Write-Host "🧪 Testing BusBuddy..."
    try {
        dotnet test BusBuddy.sln --configuration Release --verbosity minimal
        Write-Host "✅ Tests complete!"
    } catch {
        Write-Error "❌ Tests failed: $_"
    }
}

# Health check function with approved verb
function Get-BusBuddyHealth {
    <#
    .SYNOPSIS
    Checks the health of the BusBuddy development environment

    .PARAMETER Quick
    Performs a quick health check with minimal diagnostics
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$false)]
        [switch]$Quick
    )

    Write-Host "🏥 BusBuddy Health Check:"

    # Show quick info if Quick parameter is specified
    if ($Quick) {
        Write-Host "Quick health check mode..." -ForegroundColor Cyan
    }

    # Basic checks
    dotnet --version
    if (Test-Path "BusBuddy.sln") { Write-Host "✅ Solution found" } else { Write-Host "❌ Solution missing" }

    # Additional checks when not in quick mode
    if (-not $Quick) {
        # Add more comprehensive checks here
        Write-Host "Performing comprehensive health check..." -ForegroundColor Yellow
        # Example: Check for project files
        if (Test-Path "BusBuddy.Core\BusBuddy.Core.csproj") {
            Write-Host "✅ Core project found"
        } else {
            Write-Host "❌ Core project missing"
        }

        if (Test-Path "BusBuddy.WPF\BusBuddy.WPF.csproj") {
            Write-Host "✅ WPF project found"
        } else {
            Write-Host "❌ WPF project missing"
        }
    }
}

# Debug start function with approved verb
function Start-BusBuddyDebugFilter {
    <#
    .SYNOPSIS
    Starts the BusBuddy debug filter
    #>
    [CmdletBinding()]
    param([switch]$Force)

    if (-not (Test-RepoRoot)) { return }
    Write-Host "🔍 Starting debug filter..."
    try {
        dotnet run --project BusBuddy.csproj -- --start-debug-filter
        Write-Host "✅ Debug filter started!"
    } catch {
        Write-Error "❌ Debug filter start failed: $_"
    }
}

# Debug export function with approved verb
function Export-BusBuddyDebugData {
    <#
    .SYNOPSIS
    Exports BusBuddy debug data
    #>
    [CmdletBinding()]
    param()

    if (-not (Test-RepoRoot)) { return }
    Write-Host "📊 Exporting debug data..."
    try {
        dotnet run --project BusBuddy.csproj -- --export-debug-json
        Write-Host "✅ Debug data exported!"
    } catch {
        Write-Error "❌ Debug export failed: $_"
    }
}

# Start a BusBuddy task with enhanced monitoring - already has approved verb
function Start-BusBuddyTask {
    <#
    .SYNOPSIS
    Starts a BusBuddy task with enhanced monitoring
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$true)]
        [ValidateSet("Build", "Run", "Health")]
        [string]$TaskType
    )

    if (-not (Test-RepoRoot)) { return }
    if (-not (Test-Path "enhanced-task-monitor-fixed.ps1")) {
        Write-Error "❌ Enhanced task monitor script not found!"
        return
    }

    try {
        switch ($TaskType) {
            "Build" {
                & "enhanced-task-monitor-fixed.ps1" -TaskName "Build" -Command "dotnet" -Arguments "build","BusBuddy.sln" -WaitForCompletion -CaptureOutput -ShowRealTime
            }
            "Run" {
                & "enhanced-task-monitor-fixed.ps1" -TaskName "Run" -Command "dotnet" -Arguments "run","--project","BusBuddy.csproj" -WaitForCompletion -CaptureOutput -ShowRealTime
            }
            "Health" {
                & "enhanced-task-monitor-fixed.ps1" -TaskName "Health Check" -Command "dotnet" -Arguments "--version" -WaitForCompletion -CaptureOutput -ShowRealTime
            }
        }
    } catch {
        Write-Error "❌ Task monitoring failed: $_"
    }
}

# Mentor function with approved verb
function Get-BusBuddyMentor {
    <#
    .SYNOPSIS
    Provides interactive learning guidance for BusBuddy
    #>
    [CmdletBinding()]
    param([string]$Topic = "Getting Started")

    Write-Host "🤖 AI Mentor: Guiding on $Topic"
    switch ($Topic.ToLower()) {
        "powershell" { Write-Host "Learn PowerShell: Start with Get-Help, then modules like this one!" }
        "wpf" { Write-Host "WPF Basics: Use XAML for UI, MVVM pattern for logic." }
        "syncfusion" { Write-Host "Syncfusion: Check https://help.syncfusion.com/wpf/welcome-to-syncfusion-essential-wpf for documentation." }
        "getting started" { Write-Host "New? Clone repo, Import-Module PowerShell\BusBuddy.psm1, run bb-build and bb-run." }
        default { Write-Host "Topic not found—try 'PowerShell', 'WPF', 'Syncfusion', or 'Getting Started'." }
    }
}

# Docs function with approved verb - fixed string interpolation
function Search-BusBuddyDocs {
    <#
    .SYNOPSIS
    Searches BusBuddy documentation
    #>
    [CmdletBinding()]
    param([string]$Technology, [string]$Query)

    Write-Host "📚 Searching docs for $Technology`: $Query"
    # Simple echo; expand to actual search if needed
    Write-Host "Example: For WPF data binding, check Documentation folder."
}

# Happiness function with approved verb
function Get-BusBuddyHappiness {
    <#
    .SYNOPSIS
    Displays motivational quotes for BusBuddy developers
    #>
    [CmdletBinding()]
    param()

    $quotes = @(
        "Great software is built by great teams having great fun!",
        "Keep coding—BusBuddy is rolling!",
        "Solving complex transportation problems, one line of code at a time!",
        "Code clean, run smooth, transport safely!"
    )
    Write-Host "😊 " + ($quotes | Get-Random)
}

# Create aliases for backward compatibility
New-Alias -Name "bb-build" -Value Invoke-BusBuddyBuild
New-Alias -Name "bb-run" -Value Start-BusBuddyApp
New-Alias -Name "bb-test" -Value Invoke-BusBuddyTest
New-Alias -Name "bb-health" -Value Get-BusBuddyHealth
New-Alias -Name "bb-debug-start" -Value Start-BusBuddyDebugFilter
New-Alias -Name "bb-debug-export" -Value Export-BusBuddyDebugData
New-Alias -Name "bb-mentor" -Value Get-BusBuddyMentor
New-Alias -Name "bb-docs" -Value Search-BusBuddyDocs
New-Alias -Name "bb-happiness" -Value Get-BusBuddyHappiness

# Export functions and aliases
Export-ModuleMember -Function Invoke-BusBuddyBuild, Start-BusBuddyApp, Invoke-BusBuddyTest,
                             Get-BusBuddyHealth, Start-BusBuddyDebugFilter, Export-BusBuddyDebugData,
                             Start-BusBuddyTask, Get-BusBuddyMentor, Search-BusBuddyDocs, Get-BusBuddyHappiness -Alias *
