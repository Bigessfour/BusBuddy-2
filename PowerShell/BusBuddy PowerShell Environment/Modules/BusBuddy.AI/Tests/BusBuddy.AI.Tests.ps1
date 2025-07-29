#Requires -Version 7.5
#Requires -Modules Pester

<#
.SYNOPSIS
Pester tests for BusBuddy.AI module

.DESCRIPTION
Tests for the BusBuddy AI Integration module functionality including:
- Module loading and initialization
- Function availability
- Parameter validation
- Basic AI connectivity (when API key available)

.NOTES
Author: BusBuddy Development Team
Created: July 26, 2025
Test Framework: Pester 5.x
#>

BeforeAll {
    # Import the module
    $ModulePath = Join-Path $PSScriptRoot '..' 'BusBuddy.AI.psd1'
    Import-Module $ModulePath -Force
}

Describe "BusBuddy.AI Module" {

    Context "Module Loading" {
        It "Should load the module successfully" {
            Get-Module BusBuddy.AI | Should -Not -BeNullOrEmpty
        }

        It "Should export expected functions" {
            $ExpectedFunctions = @(
                "Start-BusBuddyAIWorkflow",
                "Get-BusBuddyAIAssistance",
                "Get-BusBuddyContextualSearch"
            )

            foreach ($Function in $ExpectedFunctions) {
                Get-Command $Function -Module BusBuddy.AI -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
            }
        }

        It "Should export expected aliases" {
            $ExpectedAliases = @(
                "bb-ai-start",
                "bb-ai-help",
                "bb-ai-search"
            )

            foreach ($Alias in $ExpectedAliases) {
                Get-Alias $Alias -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
            }
        }
    }

    Context "Function Parameters" {
        It "Start-BusBuddyAIWorkflow should have required parameters" {
            $Command = Get-Command Start-BusBuddyAIWorkflow -ErrorAction SilentlyContinue
            if ($Command) {
                $Command.Parameters.Keys | Should -Contain 'WorkflowType'

                # Check parameter validation
                $WorkflowTypeParam = $Command.Parameters['WorkflowType']
                $WorkflowTypeParam.Attributes.ValidValues | Should -Contain 'CodeReview'
                $WorkflowTypeParam.Attributes.ValidValues | Should -Contain 'Documentation'
                $WorkflowTypeParam.Attributes.ValidValues | Should -Contain 'Testing'
                $WorkflowTypeParam.Attributes.ValidValues | Should -Contain 'Optimization'
                $WorkflowTypeParam.Attributes.ValidValues | Should -Contain 'Architecture'
            }
        }

        It "Get-BusBuddyAIAssistance should support both task and context parameters" {
            $Command = Get-Command Get-BusBuddyAIAssistance -ErrorAction SilentlyContinue
            if ($Command) {
                $Command.Parameters.Keys | Should -Contain 'Task'
                $Command.Parameters.Keys | Should -Contain 'Context'
            }
        }

        It "Get-BusBuddyContextualSearch should have Query and Scope parameters" {
            $Command = Get-Command Get-BusBuddyContextualSearch -ErrorAction SilentlyContinue
            if ($Command) {
                $Command.Parameters.Keys | Should -Contain 'Query'
                $Command.Parameters.Keys | Should -Contain 'Scope'

                $ScopeParam = $Command.Parameters['Scope']
                $ScopeParam.Attributes.ValidValues | Should -Contain 'Code'
                $ScopeParam.Attributes.ValidValues | Should -Contain 'Documentation'
                $ScopeParam.Attributes.ValidValues | Should -Contain 'Configuration'
                $ScopeParam.Attributes.ValidValues | Should -Contain 'Scripts'
                $ScopeParam.Attributes.ValidValues | Should -Contain 'All'
            }
        }
    }

    Context "Parameter Validation" {
        It "Should reject null or empty Task parameter for AI assistance" {
            { Get-BusBuddyAIAssistance -Task '' } |
            Should -Throw -Because "Empty task should be rejected"

            { Get-BusBuddyAIAssistance -Task $null } |
            Should -Throw -Because "Null task should be rejected"
        }

        It "Should reject invalid WorkflowType" {
            { Start-BusBuddyAIWorkflow -WorkflowType 'InvalidType' } |
            Should -Throw -Because "Invalid workflow type should be rejected"
        }

        It "Should reject null or empty Query for contextual search" {
            { Get-BusBuddyContextualSearch -Query '' } |
            Should -Throw -Because "Empty query should be rejected"
        }
    }

    Context "AI Connectivity" -Tag 'Integration' {
        It "Should handle missing API key gracefully" {
            # Test without API key set
            $originalKey = $env:XAI_API_KEY
            try {
                $env:XAI_API_KEY = $null
                { Get-BusBuddyAIAssistance -Task "Test task" } | Should -Not -Throw
            } finally {
                $env:XAI_API_KEY = $originalKey
            }
        }

        It "Should initialize successfully with valid API key" -Skip:(-not $env:XAI_API_KEY) {
            { Get-BusBuddyAIAssistance -Task "Test connectivity" } | Should -Not -Throw
        }
    }

    Context "Error Handling" {
        It "Should throw meaningful errors for invalid file paths" {
            { Get-BusBuddyContextualSearch -Query "test" -Scope "InvalidScope" } |
            Should -Throw -Because "Invalid scope should be rejected"
        }

        It "Should handle workflow errors gracefully" {
            { Start-BusBuddyAIWorkflow -WorkflowType "CodeReview" } | Should -Not -Throw
        }
    }
}

AfterAll {
    # Clean up
    Remove-Module BusBuddy.AI -Force -ErrorAction SilentlyContinue
}
