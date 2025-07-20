#Requires -Version 7.0
<#
.SYNOPSIS
    Verifies administrator privileges requirements for Bus Buddy PowerShell workflows

.DESCRIPTION
    This script checks which Bus Buddy scripts and workflows require administrator privileges
    and documents the requirements in the project documentation.

.NOTES
    Version: 1.0
    Date: July 19, 2025
    Author: Bus Buddy Team

.EXAMPLE
    # Verify admin privileges requirements
    .\verify-admin-requirements.ps1
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [switch]$GenerateDocumentation
)

Write-Host "🔍 Verifying Administrator Privileges Requirements..." -ForegroundColor Cyan

# Function to check if a file requires admin privileges
function Test-AdminRequirement {
    param(
        [string]$FilePath
    )

    if (-not (Test-Path $FilePath)) {
        return @{
            Path       = $FilePath
            Required   = $false
            Explicit   = $false
            Reason     = "File not found"
            Operations = @()
        }
    }

    $content = Get-Content -Path $FilePath -Raw

    # Check for explicit #Requires -RunAsAdministrator directive
    $explicitRequirement = $content -match "#Requires\s+-RunAsAdministrator"

    # Check for operations that typically need admin privileges
    $adminOperations = @()

    # Registry operations
    if ($content -match "New-ItemProperty.*HKLM:|Set-ItemProperty.*HKLM:|New-Item.*HKLM:|Remove-Item.*HKLM:") {
        $adminOperations += "HKLM registry access"
    }

    # Service management
    if ($content -match "New-Service|Set-Service|Remove-Service|Restart-Service") {
        $adminOperations += "Service management"
    }

    # System-wide environment variables
    if (($content -match [regex]::Escape('[Environment]::SetEnvironmentVariable(')) -and ($content -match "Machine")) {
        $adminOperations += "Machine-level environment variables"
    }

    # Network configuration
    if ($content -match "New-NetFirewallRule|Set-NetFirewallRule|Remove-NetFirewallRule") {
        $adminOperations += "Firewall configuration"
    }

    # System file operations in protected directories
    if ($content -match "C:\\Windows|C:\\Program Files") {
        $adminOperations += "System directory operations"
    }

    # Process management with elevated privileges
    if ($content -match "Start-Process.*-Verb\s+RunAs") {
        $adminOperations += "Elevated process execution"
    }

    # Scheduled task creation
    if ($content -match "Register-ScheduledTask|New-ScheduledTask") {
        $adminOperations += "Scheduled task management"
    }

    # User account management
    if ($content -match "New-LocalUser|Remove-LocalUser|Add-LocalGroupMember") {
        $adminOperations += "User account management"
    }

    # Determine if admin is required based on operations
    $requiresAdmin = $explicitRequirement -or ($adminOperations.Count -gt 0)

    return @{
        Path       = $FilePath
        Required   = $requiresAdmin
        Explicit   = $explicitRequirement
        Reason     = if ($explicitRequirement) { "Explicit #Requires directive" } else { if ($adminOperations.Count -gt 0) { "Administrative operations" } else { "No admin requirements" } }
        Operations = $adminOperations
    }
}

# Find all PowerShell scripts in the project
$projectRoot = $PSScriptRoot
$psScripts = Get-ChildItem -Path $projectRoot -Recurse -Filter "*.ps1" | Select-Object -ExpandProperty FullName

# Check each script for admin requirements
$results = @()
ForEach-Object ($script in $psScripts) {
    $relativePath = $script.Replace("$projectRoot\", "")
    Write-Host "  Checking $relativePath..." -ForegroundColor Gray
    $results += Test-AdminRequirement -FilePath $script
}

# Display summary
$adminRequired = $results | Where-Object { $_.Required }
$explicitDirective = $results | Where-Object { $_.Explicit }
$implicitRequired = $results | Where-Object { $_.Required -and -not $_.Explicit }

Write-Host "`n📊 Administrator Privileges Summary:" -ForegroundColor Cyan
Write-Host "  • Total scripts analyzed: $($results.Count)" -ForegroundColor White
Write-Host "  • Scripts requiring admin privileges: $($adminRequired.Count)" -ForegroundColor White
Write-Host "  • Scripts with explicit #Requires directive: $($explicitDirective.Count)" -ForegroundColor White
Write-Host "  • Scripts with implicit admin requirements: $($implicitRequired.Count)" -ForegroundColor White

# Display scripts requiring admin privileges
if ($adminRequired.Count -gt 0) {
    Write-Host "`n📋 Scripts Requiring Administrator Privileges:" -ForegroundColor Green
    ForEach-Object ($script in $adminRequired) {
        $relativePath = $script.Path.Replace("$projectRoot\", "")
        Write-Host "`n  📄 $relativePath" -ForegroundColor Yellow

        if ($script.Explicit) {
            Write-Host "    ✅ Has explicit #Requires -RunAsAdministrator directive" -ForegroundColor Green
        } else {
            Write-Host "    ⚠️ Needs explicit #Requires -RunAsAdministrator directive" -ForegroundColor Yellow
        }

        if ($script.Operations.Count -gt 0) {
            Write-Host "    🔐 Administrative operations:" -ForegroundColor Cyan
            ForEach-Object ($op in $script.Operations) {
                Write-Host "      • $op" -ForegroundColor White
            }
        }
    }
}

# Generate documentation if requested
if ($GenerateDocumentation) {
    $docPath = Join-Path $projectRoot "ADMIN-PRIVILEGES-REQUIREMENTS.md"

    # Create initial document content
    $docContent = "# Bus Buddy Administrator Privileges Requirements`n`n"
    $docContent += "This document outlines which scripts and workflows in the Bus Buddy project require administrator privileges.`n`n"
    $docContent += "## Summary`n`n"
    $docContent += "- **Total scripts analyzed:** $($results.Count)`n"
    $docContent += "- **Scripts requiring admin privileges:** $($adminRequired.Count)`n"
    $docContent += "- **Scripts with explicit #Requires directive:** $($explicitDirective.Count)`n"
    $docContent += "- **Scripts with implicit admin requirements:** $($implicitRequired.Count)`n`n"
    $docContent += "## Scripts Requiring Administrator Privileges`n"

    # Add each script's details
    ForEach-Object ($script in $adminRequired) {
        $relativePath = $script.Path.Replace("$projectRoot\", "")

        $docContent += "`n### $relativePath`n`n"
        $docContent += "- **Status:** $(if ($script.Explicit) { "✅ Has explicit #Requires directive" } else { "⚠️ Needs explicit #Requires directive" })`n"
        $docContent += "- **Reason:** $($script.Reason)`n"

        if ($script.Operations.Count -gt 0) {
            $docContent += "`n#### Administrative Operations:`n`n"
            ForEach-Object ($op in $script.Operations) {
                $docContent += "- $op`n"
            }
        }
    }

    # Add documentation footer
    $docContent += @'

## How to Run with Administrator Privileges

To run PowerShell scripts with administrator privileges:

1. **Right-click PowerShell or VS Code** and Select-Object "Run as administrator"
2. **From an existing PowerShell session**, use:
   ```powershell
   Start-Process pwsh -ArgumentList "-File", "path\to\script.ps1" -Verb RunAs
   ```
3. **In VS Code**, configure your terminal to run as administrator in settings.json:
   ```json
   "terminal.integrated.profiles.windows": {
     "PowerShell Admin": {
       "path": "pwsh.exe",
       "args": ["-Command", "Start-Process pwsh -Verb RunAs"]
     }
   }
   ```

## Requirements for Adding New Scripts

When adding new scripts to the Bus Buddy project:

1. If your script performs administrative operations, add `#Requires -RunAsAdministrator` at the top
2. Document the specific administrative operations your script performs
3. Use `Test-AdminPrivileges` function to check for admin rights at runtime
4. Always provide proper error handling for cases when admin rights are not available

'@

    $docContent += "This document was automatically generated on $(Get-Date -Format "yyyy-MM-dd") by verify-admin-requirements.ps1."

    try {
        Set-Content -Path $docPath -Value $docContent -Force
        Write-Host "`n✅ Generated documentation: $docPath" -ForegroundColor Green
    } catch {
        Write-Host "`n❌ Failed to generate documentation: $_" -ForegroundColor Red
    }
}

# Print scripts that need to be fixed
if ($implicitRequired.Count -gt 0) {
    Write-Host "`n⚠️ Scripts Needing #Requires -RunAsAdministrator Directive:" -ForegroundColor Yellow
    ForEach-Object ($script in $implicitRequired) {
        $relativePath = $script.Path.Replace("$projectRoot\", "")
        Write-Host "  • $relativePath" -ForegroundColor White
    }

    Write-Host "`n💡 Run the script with -GenerateDocumentation to create a full report" -ForegroundColor Cyan
}

Write-Host "`n✅ Administrator privileges verification complete!" -ForegroundColor Green
