# Utilities.ps1 - Utility functions for BusBuddy
# Microsoft PowerShell dot-source script

function Get-BusBuddyInfo {
    <#
    .SYNOPSIS
    Get BusBuddy project information
    .DESCRIPTION
    Displays comprehensive information about the BusBuddy project
    .EXAMPLE
    Get-BusBuddyInfo
    #>
    [CmdletBinding()]
    param()

    try {
        Write-Host "ℹ️ BusBuddy Project Information" -ForegroundColor Cyan
        Write-Host "==============================" -ForegroundColor Cyan

        Write-Host "📁 Workspace: $BusBuddyWorkspace" -ForegroundColor White
        Write-Host "🔧 PowerShell: $($PSVersionTable.PSVersion)" -ForegroundColor White
        Write-Host "🛠️ .NET SDK: $(dotnet --version)" -ForegroundColor White

        if ($global:BusBuddySettings) {
            Write-Host "⚙️ Settings:" -ForegroundColor White
            $global:BusBuddySettings.GetEnumerator() | ForEach-Object {
                Write-Host "   $($_.Key): $($_.Value)" -ForegroundColor Gray
            }
        }

        # Project sizes
        $projects = @("BusBuddy.Core", "BusBuddy.WPF", "BusBuddy.Tests")
        Write-Host "📊 Project Sizes:" -ForegroundColor White
        foreach ($project in $projects) {
            $projectPath = "$BusBuddyWorkspace\$project"
            if (Test-Path $projectPath) {
                $files = Get-ChildItem -Path $projectPath -Recurse -File -Include "*.cs", "*.xaml" | Measure-Object -Property Length -Sum
                Write-Host "   ${project}: $($files.Count) files, $([math]::Round($files.Sum / 1KB, 1)) KB" -ForegroundColor Gray
            }
        }
    }
    catch {
        Write-Error "Info error: $($_.Exception.Message)"
    }
}

function Set-BusBuddyWorkspace {
    <#
    .SYNOPSIS
    Set the BusBuddy workspace location
    .DESCRIPTION
    Changes the current BusBuddy workspace path
    .PARAMETER Path
    The path to the BusBuddy workspace
    .EXAMPLE
    Set-BusBuddyWorkspace -Path "C:\Projects\BusBuddy"
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    try {
        if (Test-Path "$Path\BusBuddy.sln") {
            $global:BusBuddyWorkspace = $Path
            Set-Location $Path
            Write-Host "✅ Workspace set to: $Path" -ForegroundColor Green
        } else {
            Write-Error "Invalid workspace: BusBuddy.sln not found in $Path"
        }
    }
    catch {
        Write-Error "Workspace set error: $($_.Exception.Message)"
    }
}

function Get-BusBuddyCommands {
    <#
    .SYNOPSIS
    List all BusBuddy commands
    .DESCRIPTION
    Shows all available BusBuddy functions and aliases
    .EXAMPLE
    Get-BusBuddyCommands
    #>
    [CmdletBinding()]
    param()

    try {
        Write-Host "🚌 BusBuddy Commands" -ForegroundColor Cyan
        Write-Host "===================" -ForegroundColor Cyan

        # Functions
        Write-Host "📝 Functions:" -ForegroundColor Yellow
        Get-Command -Name "*BusBuddy*" -CommandType Function |
            Sort-Object Name |
            ForEach-Object { Write-Host "   $($_.Name)" -ForegroundColor White }

        # Aliases
        Write-Host "🔗 Aliases:" -ForegroundColor Yellow
        Get-Alias -Name "bb-*" -ErrorAction SilentlyContinue |
            Sort-Object Name |
            ForEach-Object { Write-Host "   $($_.Name) -> $($_.Definition)" -ForegroundColor White }
    }
    catch {
        Write-Error "Commands list error: $($_.Exception.Message)"
    }
}

function Export-BusBuddySettings {
    <#
    .SYNOPSIS
    Export current BusBuddy settings
    .DESCRIPTION
    Exports the current settings to a backup file
    .PARAMETER Path
    The path where to export settings
    .EXAMPLE
    Export-BusBuddySettings -Path "backup.ini"
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    try {
        if ($global:BusBuddySettings) {
            $content = $global:BusBuddySettings.GetEnumerator() |
                ForEach-Object { "$($_.Key)=$($_.Value)" }
            $content | Out-File -FilePath $Path -Encoding UTF8
            Write-Host "✅ Settings exported to: $Path" -ForegroundColor Green
        } else {
            Write-Warning "No settings loaded to export"
        }
    }
    catch {
        Write-Error "Export error: $($_.Exception.Message)"
    }
}

Write-Verbose "Utility functions loaded: Get-BusBuddyInfo, Set-BusBuddyWorkspace, Get-BusBuddyCommands, Export-BusBuddySettings"
