#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Examples of PowerShell verb suppression techniques for BusBuddy project
.DESCRIPTION
    Demonstrates how to suppress PSUseApprovedVerbs warnings when needed
#>

# Example 1: Function-level suppression for legacy functions
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseApprovedVerbs', '')]
function Fix-LegacyIssue {
    param([string]$IssueDescription)
    Write-Host "Legacy function that can't be renamed due to external dependencies"
}

# Example 2: Multiple suppressions
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseApprovedVerbs', '')]
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSAvoidUsingWriteHost', '')]
function Fix-MultipleWarnings {
    Write-Host "Function with multiple suppressions"
}

# Example 3: Suppression with specific scope
[System.Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseApprovedVerbs', '', Scope='Function', Target='Fix-SpecificFunction')]
function Fix-SpecificFunction {
    param([string]$InputData)
    return $InputData
}

# Example 4: Best practice - use approved verbs instead
function Repair-ModernIssue {
    param([string]$Issue)
    Write-Host "Modern function using approved 'Repair' verb"
}

Write-Host "PowerShell verb suppression examples loaded" -ForegroundColor Green
