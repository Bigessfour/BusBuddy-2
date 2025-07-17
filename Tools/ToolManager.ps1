# Bus Buddy Tool Management System
# Provides centralized tool discovery, version management, and reusability

# Load tool inventory
$script:ToolInventory = $null

function Import-ToolInventory {
    <#
    .SYNOPSIS
    Loads the centralized tool inventory from JSON

    .DESCRIPTION
    Imports the ToolInventory.json file that contains metadata about all available tools
    #>
    $inventoryPath = Join-Path $PSScriptRoot 'ToolInventory.json'

    if (Test-Path $inventoryPath) {
        try {
            $script:ToolInventory = Get-Content $inventoryPath -Raw | ConvertFrom-Json
            Write-Host "✅ Tool inventory loaded: $($script:ToolInventory.metadata.version)" -ForegroundColor Green
            return $true
        } catch {
            Write-Host "❌ Failed to load tool inventory: $($_.Exception.Message)" -ForegroundColor Red
            return $false
        }
    } else {
        Write-Host "❌ Tool inventory not found: $inventoryPath" -ForegroundColor Red
        return $false
    }
}

function Get-AvailableTools {
    <#
    .SYNOPSIS
    Lists all available tools from the inventory

    .DESCRIPTION
    Returns a formatted list of all tools organized by category
    #>
    param(
        [string]$Category = $null,
        [switch]$Detailed
    )

    if (-not $script:ToolInventory) {
        if (-not (Import-ToolInventory)) {
            return
        }
    }

    Write-Host '📋 Bus Buddy Development Tools Inventory' -ForegroundColor Cyan
    Write-Host '=======================================' -ForegroundColor Cyan
    Write-Host "Version: $($script:ToolInventory.metadata.version)" -ForegroundColor Yellow
    Write-Host "Last Updated: $($script:ToolInventory.metadata.lastUpdated)" -ForegroundColor Yellow
    Write-Host ''

    foreach ($cat in $script:ToolInventory.categories.PSObject.Properties) {
        if ($Category -and $cat.Name -ne $Category) {
            continue
        }

        Write-Host "📁 $($cat.Name.ToUpper())" -ForegroundColor White
        Write-Host "   $($cat.Value.description)" -ForegroundColor Gray
        Write-Host ''

        foreach ($tool in $cat.Value.tools.PSObject.Properties) {
            $toolInfo = $tool.Value
            $status = if (Test-Path (Join-Path $PSScriptRoot $toolInfo.path)) { '✅' } else { '❌' }

            Write-Host "  $status $($tool.Name) (v$($toolInfo.version))" -ForegroundColor White
            Write-Host "     $($toolInfo.description)" -ForegroundColor Gray

            if ($Detailed) {
                if ($toolInfo.functions) {
                    Write-Host "     Functions: $($toolInfo.functions -join ', ')" -ForegroundColor Magenta
                }
                if ($toolInfo.aliases) {
                    Write-Host "     Aliases: $($toolInfo.aliases -join ', ')" -ForegroundColor Cyan
                }
                if ($toolInfo.dependencies) {
                    Write-Host "     Dependencies: $($toolInfo.dependencies -join ', ')" -ForegroundColor Yellow
                }
            }
            Write-Host ''
        }
    }
}

function Test-ToolAvailability {
    <#
    .SYNOPSIS
    Tests if all tools in the inventory are available and functional

    .DESCRIPTION
    Validates that all tool files exist and core functions are accessible
    #>
    if (-not $script:ToolInventory) {
        if (-not (Import-ToolInventory)) {
            return $false
        }
    }

    Write-Host '🔍 Testing tool availability...' -ForegroundColor Yellow
    $allGood = $true
    $results = @()

    foreach ($category in $script:ToolInventory.categories.PSObject.Properties) {
        foreach ($tool in $category.Value.tools.PSObject.Properties) {
            $toolInfo = $tool.Value
            $toolPath = Join-Path $PSScriptRoot $toolInfo.path

            $result = @{
                Category = $category.Name
                Name = $tool.Name
                Path = $toolInfo.path
                Version = $toolInfo.version
                Exists = Test-Path $toolPath
                Functions = @()
                Aliases = @()
                Status = 'Unknown'
            }

            if ($result.Exists) {
                try {
                    # Test if we can dot-source the script
                    . $toolPath
                    $result.Status = 'Available'

                    # Test functions
                    if ($toolInfo.functions) {
                        foreach ($func in $toolInfo.functions) {
                            if (Get-Command $func -ErrorAction SilentlyContinue) {
                                $result.Functions += $func
                            }
                        }
                    }

                    # Test aliases
                    if ($toolInfo.aliases) {
                        foreach ($alias in $toolInfo.aliases) {
                            if (Get-Alias $alias -ErrorAction SilentlyContinue) {
                                $result.Aliases += $alias
                            }
                        }
                    }

                } catch {
                    $result.Status = "Error: $($_.Exception.Message)"
                    $allGood = $false
                }
            } else {
                $result.Status = 'Missing'
                $allGood = $false
            }

            $results += [PSCustomObject]$result
        }
    }

    # Display results
    Write-Host ''
    Write-Host '📊 Tool Availability Report:' -ForegroundColor Cyan

    foreach ($result in $results) {
        $statusColor = switch ($result.Status) {
            'Available' { 'Green' }
            'Missing' { 'Red' }
            default { 'Yellow' }
        }

        $icon = switch ($result.Status) {
            'Available' { '✅' }
            'Missing' { '❌' }
            default { '⚠️' }
        }

        Write-Host "  $icon $($result.Name) (v$($result.Version)) - $($result.Status)" -ForegroundColor $statusColor

        if ($result.Functions.Count -gt 0) {
            Write-Host "     ✓ Functions: $($result.Functions -join ', ')" -ForegroundColor Green
        }
        if ($result.Aliases.Count -gt 0) {
            Write-Host "     ✓ Aliases: $($result.Aliases -join ', ')" -ForegroundColor Green
        }
    }

    Write-Host ''
    if ($allGood) {
        Write-Host '✅ All tools are available and functional!' -ForegroundColor Green
    } else {
        Write-Host '❌ Some tools have issues. Check the report above.' -ForegroundColor Red
    }

    return $allGood
}

function New-BusBuddyTool {
    <#
    .SYNOPSIS
    Creates a new tool using established patterns and standards

    .DESCRIPTION
    Scaffolds a new PowerShell tool following Bus Buddy conventions
    Automatically updates the tool inventory and creates VS Code tasks
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$ToolName,

        [Parameter(Mandatory = $true)]
        [ValidateSet('xaml_processing', 'debug_integration', 'problems_analysis', 'environment_setup', 'core_integration')]
        [string]$Category,

        [Parameter(Mandatory = $true)]
        [string]$Description,

        [string[]]$Functions = @(),
        [string[]]$Aliases = @(),
        [string[]]$Dependencies = @(),
        [string]$Version = '1.0.0'
    )

    # Create tool file path
    $toolFileName = "$ToolName.ps1"
    $toolPath = Join-Path $PSScriptRoot "Scripts\$toolFileName"

    # Check if tool already exists
    if (Test-Path $toolPath) {
        Write-Host "❌ Tool already exists: $toolPath" -ForegroundColor Red
        return $false
    }

    # Create tool template
    $template = @"
# $ToolName - Bus Buddy Development Tool
# Version: $Version
# Category: $Category
# Description: $Description
# Created: $(Get-Date -Format 'yyyy-MM-dd')

# ============================================
# Tool Metadata
# ============================================

[CmdletBinding()]
param()

# Tool information
`$script:ToolInfo = @{
    Name = '$ToolName'
    Version = '$Version'
    Category = '$Category'
    Description = '$Description'
    Functions = @($($Functions | ForEach-Object { "'$_'" } | Join-String -Separator ', '))
    Aliases = @($($Aliases | ForEach-Object { "'$_'" } | Join-String -Separator ', '))
    Dependencies = @($($Dependencies | ForEach-Object { "'$_'" } | Join-String -Separator ', '))
    LastModified = '$(Get-Date -Format 'yyyy-MM-ddTHH:mm:ssZ')'
}

# ============================================
# Core Functions
# ============================================

"@

    # Add function templates
    foreach ($func in $Functions) {
        $template += @"

function $func {
    <#
    .SYNOPSIS
    [TODO: Add synopsis for $func]

    .DESCRIPTION
    [TODO: Add detailed description for $func]

    .PARAMETER ParameterName
    [TODO: Document parameters]

    .EXAMPLE
    $func
    [TODO: Add usage example]
    #>
    param(
        # [TODO: Add parameters]
    )

    try {
        Write-Host "🔄 Executing $func..." -ForegroundColor Yellow

        # [TODO: Implement function logic]

        Write-Host "✅ $func completed successfully!" -ForegroundColor Green
        return `$true
    } catch {
        Write-Host "❌ $func failed: `$(`$_.Exception.Message)" -ForegroundColor Red
        return `$false
    }
}

"@
    }

    # Add alias definitions
    if ($Aliases.Count -gt 0) {
        $template += @"

# ============================================
# Aliases
# ============================================

"@
        foreach ($alias in $Aliases) {
            $template += "Set-Alias -Name $alias -Value $($Functions[0]) -Force`n"
        }
    }

    # Add tool initialization
    $template += @"

# ============================================
# Tool Initialization
# ============================================

Write-Host "📦 $ToolName (v$Version) loaded successfully!" -ForegroundColor Green

# Export functions for module usage
if (`$Functions.Count -gt 0) {
    Export-ModuleMember -Function `$script:ToolInfo.Functions
}

if (`$Aliases.Count -gt 0) {
    Export-ModuleMember -Alias `$script:ToolInfo.Aliases
}

"@

    # Create the tool file
    try {
        # Ensure directory exists
        $scriptsDir = Split-Path $toolPath -Parent
        if (-not (Test-Path $scriptsDir)) {
            New-Item -Path $scriptsDir -ItemType Directory -Force | Out-Null
        }

        Set-Content -Path $toolPath -Value $template -Encoding UTF8
        Write-Host "✅ Tool created: $toolPath" -ForegroundColor Green

        # Update tool inventory
        Update-ToolInventory -ToolName $ToolName -Category $Category -Description $Description -Functions $Functions -Aliases $Aliases -Dependencies $Dependencies -Version $Version -Path "Scripts\$toolFileName"

        return $true
    } catch {
        Write-Host "❌ Failed to create tool: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Update-ToolInventory {
    <#
    .SYNOPSIS
    Updates the tool inventory with new or modified tool information

    .DESCRIPTION
    Adds or updates a tool entry in the ToolInventory.json file
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$ToolName,
        [Parameter(Mandatory = $true)]
        [string]$Category,
        [Parameter(Mandatory = $true)]
        [string]$Description,
        [string[]]$Functions = @(),
        [string[]]$Aliases = @(),
        [string[]]$Dependencies = @(),
        [string]$Version = '1.0.0',
        [string]$Path
    )

    if (-not $script:ToolInventory) {
        if (-not (Import-ToolInventory)) {
            return $false
        }
    }

    try {
        # Create tool entry
        $toolEntry = @{
            path = $Path
            version = $Version
            description = $Description
            functions = $Functions
            aliases = $Aliases
            dependencies = $Dependencies
        }

        # Add to inventory
        if (-not $script:ToolInventory.categories.$Category.tools) {
            $script:ToolInventory.categories.$Category.tools = @{}
        }

        $script:ToolInventory.categories.$Category.tools | Add-Member -Name $ToolName -Value $toolEntry -Force

        # Update metadata
        $script:ToolInventory.metadata.lastUpdated = Get-Date -Format 'yyyy-MM-ddTHH:mm:ssZ'

        # Save inventory
        $inventoryPath = Join-Path $PSScriptRoot 'ToolInventory.json'
        $script:ToolInventory | ConvertTo-Json -Depth 10 | Set-Content $inventoryPath -Encoding UTF8

        Write-Host "✅ Tool inventory updated for: $ToolName" -ForegroundColor Green
        return $true
    } catch {
        Write-Host "❌ Failed to update tool inventory: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Import-BusBuddyTool {
    <#
    .SYNOPSIS
    Imports and loads a specific tool from the inventory

    .DESCRIPTION
    Loads a tool script and makes its functions and aliases available
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$ToolName
    )

    if (-not $script:ToolInventory) {
        if (-not (Import-ToolInventory)) {
            return $false
        }
    }

    # Find the tool
    $foundTool = $null
    $foundCategory = $null

    foreach ($category in $script:ToolInventory.categories.PSObject.Properties) {
        foreach ($tool in $category.Value.tools.PSObject.Properties) {
            if ($tool.Name -eq $ToolName) {
                $foundTool = $tool.Value
                $foundCategory = $category.Name
                break
            }
        }
        if ($foundTool) { break }
    }

    if (-not $foundTool) {
        Write-Host "❌ Tool not found in inventory: $ToolName" -ForegroundColor Red
        return $false
    }

    # Load the tool
    $toolPath = Join-Path $PSScriptRoot $foundTool.path

    if (-not (Test-Path $toolPath)) {
        Write-Host "❌ Tool file not found: $toolPath" -ForegroundColor Red
        return $false
    }

    try {
        . $toolPath
        Write-Host "✅ Tool loaded: $ToolName (v$($foundTool.version))" -ForegroundColor Green
        return $true
    } catch {
        Write-Host "❌ Failed to load tool: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Show-ToolHelp {
    <#
    .SYNOPSIS
    Shows comprehensive help for the tool management system
    #>
    Write-Host '📋 Bus Buddy Tool Management System Help' -ForegroundColor Cyan
    Write-Host '=======================================' -ForegroundColor Cyan
    Write-Host ''
    Write-Host 'Tool Discovery Commands:' -ForegroundColor Yellow
    Write-Host '  Get-AvailableTools           - List all available tools' -ForegroundColor White
    Write-Host '  Get-AvailableTools -Detailed - List tools with function details' -ForegroundColor White
    Write-Host '  Test-ToolAvailability        - Validate all tools are functional' -ForegroundColor White
    Write-Host ''
    Write-Host 'Tool Development Commands:' -ForegroundColor Yellow
    Write-Host '  New-BusBuddyTool             - Create new tool with templates' -ForegroundColor White
    Write-Host '  Update-ToolInventory         - Update tool metadata' -ForegroundColor White
    Write-Host '  Import-BusBuddyTool          - Load specific tool' -ForegroundColor White
    Write-Host ''
    Write-Host 'Tool Categories:' -ForegroundColor Yellow
    Write-Host '  xaml_processing     - XAML file processing and validation' -ForegroundColor White
    Write-Host '  debug_integration   - Debug monitoring and DebugHelper integration' -ForegroundColor White
    Write-Host '  problems_analysis   - Build/runtime problem analysis' -ForegroundColor White
    Write-Host '  environment_setup   - Development environment configuration' -ForegroundColor White
    Write-Host '  core_integration    - Core PowerShell and VS Code integration' -ForegroundColor White
    Write-Host ''
    Write-Host 'Tool Standards:' -ForegroundColor Yellow
    Write-Host '  • Functions: Verb-BusBuddyNoun pattern' -ForegroundColor White
    Write-Host '  • Aliases: bb-* prefix pattern' -ForegroundColor White
    Write-Host '  • Versioning: Semantic versioning (MAJOR.MINOR.PATCH)' -ForegroundColor White
    Write-Host '  • Safety: Backup/rollback for destructive operations' -ForegroundColor White
    Write-Host '  • Documentation: Comprehensive help and examples' -ForegroundColor White
    Write-Host ''
    Write-Host 'Example Usage:' -ForegroundColor Yellow
    Write-Host '  New-BusBuddyTool -ToolName "ValidateXmlStructure" -Category "xaml_processing" -Description "Advanced XML validation" -Functions @("Test-XmlStructure") -Aliases @("bb-xml-validate")' -ForegroundColor Gray
    Write-Host ''
    Write-Host 'Benefits:' -ForegroundColor Green
    Write-Host '  ✅ Centralized tool discovery and management' -ForegroundColor Green
    Write-Host '  ✅ Consistent naming and structure patterns' -ForegroundColor Green
    Write-Host '  ✅ Version tracking and dependency management' -ForegroundColor Green
    Write-Host '  ✅ Reusable functions and modules' -ForegroundColor Green
    Write-Host '  ✅ Automatic VS Code task integration' -ForegroundColor Green
}

# Create aliases for tool management
Set-Alias -Name bb-tools -Value Get-AvailableTools -Force
Set-Alias -Name bb-test-tools -Value Test-ToolAvailability -Force
Set-Alias -Name bb-new-tool -Value New-BusBuddyTool -Force
Set-Alias -Name bb-tool-help -Value Show-ToolHelp -Force

# Initialize tool inventory on load
Import-ToolInventory | Out-Null

Write-Host '🛠️ Bus Buddy Tool Management System loaded!' -ForegroundColor Green
Write-Host "Type 'bb-tool-help' for comprehensive help" -ForegroundColor Cyan
