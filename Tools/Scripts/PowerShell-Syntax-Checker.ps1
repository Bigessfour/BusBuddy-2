# PowerShell Syntax Checker and Code Generator
# Validates PowerShell syntax before execution to prevent iterative fixes

param(
    [Parameter(Mandatory = $false)]
    [string]$FilePath,

    [Parameter(Mandatory = $false)]
    [string]$ScriptContent,

    [Parameter(Mandatory = $false)]
    [switch]$Fix,

    [Parameter(Mandatory = $false)]
    [switch]$GenerateTemplate
)

# Import XAML-Tools for PowerShell functions
Import-Module "$PSScriptRoot\XAML-Tools.ps1" -Force

function Invoke-PowerShellSyntaxCheck {
    param(
        [string]$Path,
        [string]$Content
    )

    Write-Host '🔍 PowerShell Syntax Validation' -ForegroundColor Cyan
    Write-Host '================================' -ForegroundColor Cyan

    $validation = Test-PowerShellSyntax -FilePath $Path -ScriptContent $Content

    if ($validation.IsValid) {
        Write-Host '✅ Syntax is valid' -ForegroundColor Green
    } else {
        Write-Host '❌ Syntax errors found:' -ForegroundColor Red
        foreach ($syntaxError in $validation.Errors) {
            Write-Host "  Line $($syntaxError.Extent.StartLineNumber): $($syntaxError.Message)" -ForegroundColor Red
        }
    }

    if ($validation.Warnings.Count -gt 0) {
        Write-Host '⚠️  Best practice warnings:' -ForegroundColor Yellow
        foreach ($warning in $validation.Warnings) {
            Write-Host "  $warning" -ForegroundColor Yellow
        }
    }

    return $validation
}

function New-PowerShellScriptTemplate {
    param(
        [string]$ScriptName,
        [string]$Description
    )

    $template = @"
<#
.SYNOPSIS
    $Description

.DESCRIPTION
    Long description of what this script does

.PARAMETER ParameterName
    Description of parameter

.EXAMPLE
    .\`$ScriptName.ps1 -ParameterName Value

.NOTES
    Author: BusBuddy Development Team
    Date: $(Get-Date -Format 'yyyy-MM-dd')
    Version: 1.0
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=`$false, Position=0)]
    [ValidateNotNullOrEmpty()]
    [string]`$ParameterName = "DefaultValue"
)

# Error handling
`$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

try {
    Write-Verbose "Starting $ScriptName"

    # Main script logic here

    Write-Host "✅ $ScriptName completed successfully" -ForegroundColor Green
}
catch {
    Write-Error "❌ Error in `$ScriptName: `$(`$_.Exception.Message)"
    exit 1
}
finally {
    Write-Verbose "Finished $ScriptName"
}
"@

    return $template
}

function Test-PowerShellFunction {
    param(
        [string]$FunctionCode
    )

    # Extract function name
    $functionName = ($FunctionCode | Select-String -Pattern 'function\s+(\w+)').Matches[0].Groups[1].Value

    Write-Host "🧪 Testing function: $functionName" -ForegroundColor Cyan

    # Test syntax
    $syntaxTest = Test-PowerShellSyntax -ScriptContent $FunctionCode

    if (-not $syntaxTest.IsValid) {
        Write-Host '❌ Syntax errors in function' -ForegroundColor Red
        return $false
    }

    # Test parameter block
    $paramMatch = $FunctionCode | Select-String -Pattern 'param\s*\((.*?)\)' -AllMatches
    if ($paramMatch) {
        $paramTest = Test-PowerShellParameterSyntax -ParameterBlock $paramMatch.Matches[0].Value
        if (-not $paramTest.IsValid) {
            Write-Host '⚠️  Parameter issues:' -ForegroundColor Yellow
            foreach ($issue in $paramTest.Issues) {
                Write-Host "  $issue" -ForegroundColor Yellow
            }
        }
    }

    Write-Host '✅ Function validation complete' -ForegroundColor Green
    return $true
}

# Main execution
if ($GenerateTemplate) {
    $scriptName = Read-Host 'Enter script name'
    $description = Read-Host 'Enter script description'
    $template = New-PowerShellScriptTemplate -ScriptName $scriptName -Description $description

    $outputPath = "$PSScriptRoot\Generated-$scriptName.ps1"
    $template | Out-File -FilePath $outputPath -Encoding UTF8
    Write-Host "✅ Template generated: $outputPath" -ForegroundColor Green
    exit 0
}

if ($FilePath) {
    if (-not (Test-Path $FilePath)) {
        Write-Error "File not found: $FilePath"
        exit 1
    }

    $validation = Invoke-PowerShellSyntaxCheck -Path $FilePath

    if ($Fix -and -not $validation.IsValid) {
        Write-Host '🔧 Attempting to format code...' -ForegroundColor Yellow
        $formatted = Format-PowerShellCode -FilePath $FilePath
        $formatted | Out-File -FilePath $FilePath -Encoding UTF8
        Write-Host '✅ Code formatted and saved' -ForegroundColor Green
    }
} elseif ($ScriptContent) {
    Invoke-PowerShellSyntaxCheck -Content $ScriptContent
} else {
    Write-Host 'Usage: PowerShell-Syntax-Checker.ps1 -FilePath <path> [-Fix]' -ForegroundColor Yellow
    Write-Host '   or: PowerShell-Syntax-Checker.ps1 -ScriptContent <content>' -ForegroundColor Yellow
    Write-Host '   or: PowerShell-Syntax-Checker.ps1 -GenerateTemplate' -ForegroundColor Yellow
}
