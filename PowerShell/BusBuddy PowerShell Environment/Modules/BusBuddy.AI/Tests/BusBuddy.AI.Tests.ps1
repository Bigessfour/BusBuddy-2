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
                'Initialize-BusBuddyAI',
                'Invoke-BusBuddyAICodeGeneration',
                'Invoke-BusBuddyAICodeReview',
                'Invoke-BusBuddyAIArchitectureAnalysis'
            )

            foreach ($Function in $ExpectedFunctions) {
                Get-Command $Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty -Because "Function $Function should be available"
            }
        }

        It "Should export expected aliases" {
            $ExpectedAliases = @(
                'bb-ai-init',
                'bb-ai-generate',
                'bb-ai-review',
                'bb-ai-architect'
            )

            foreach ($Alias in $ExpectedAliases) {
                Get-Alias $Alias -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty -Because "Alias $Alias should be available"
            }
        }
    }

    Context "Function Parameters" {
        It "Invoke-BusBuddyAICodeGeneration should have required parameters" {
            $Command = Get-Command Invoke-BusBuddyAICodeGeneration
            $Command.Parameters.Keys | Should -Contain 'ComponentType'
            $Command.Parameters.Keys | Should -Contain 'Requirements'
            $Command.Parameters.Keys | Should -Contain 'OutputPath'

            # Check parameter validation
            $ComponentTypeParam = $Command.Parameters['ComponentType']
            $ComponentTypeParam.Attributes.ValidValues | Should -Contain 'ViewModel'
            $ComponentTypeParam.Attributes.ValidValues | Should -Contain 'View'
            $ComponentTypeParam.Attributes.ValidValues | Should -Contain 'Service'
            $ComponentTypeParam.Attributes.ValidValues | Should -Contain 'Model'
            $ComponentTypeParam.Attributes.ValidValues | Should -Contain 'Test'
        }

        It "Invoke-BusBuddyAICodeReview should support both file and snippet parameters" {
            $Command = Get-Command Invoke-BusBuddyAICodeReview
            $Command.Parameters.Keys | Should -Contain 'FilePath'
            $Command.Parameters.Keys | Should -Contain 'CodeSnippet'
        }

        It "Invoke-BusBuddyAIArchitectureAnalysis should have AnalysisType parameter" {
            $Command = Get-Command Invoke-BusBuddyAIArchitectureAnalysis
            $Command.Parameters.Keys | Should -Contain 'AnalysisType'

            $AnalysisTypeParam = $Command.Parameters['AnalysisType']
            $AnalysisTypeParam.Attributes.ValidValues | Should -Contain 'Overall'
            $AnalysisTypeParam.Attributes.ValidValues | Should -Contain 'Performance'
            $AnalysisTypeParam.Attributes.ValidValues | Should -Contain 'Security'
            $AnalysisTypeParam.Attributes.ValidValues | Should -Contain 'Scalability'
            $AnalysisTypeParam.Attributes.ValidValues | Should -Contain 'Maintainability'
        }
    }

    Context "Parameter Validation" {
        It "Should reject null or empty Requirements parameter" {
            { Invoke-BusBuddyAICodeGeneration -ComponentType 'Model' -Requirements '' } |
            Should -Throw -Because "Empty requirements should be rejected"

            { Invoke-BusBuddyAICodeGeneration -ComponentType 'Model' -Requirements $null } |
            Should -Throw -Because "Null requirements should be rejected"
        }

        It "Should reject invalid ComponentType" {
            { Invoke-BusBuddyAICodeGeneration -ComponentType 'InvalidType' -Requirements 'Test requirement' } |
            Should -Throw -Because "Invalid component type should be rejected"
        }

        It "Should reject null or empty CodeSnippet for code review" {
            { Invoke-BusBuddyAICodeReview -CodeSnippet '' } |
            Should -Throw -Because "Empty code snippet should be rejected"

            { Invoke-BusBuddyAICodeReview -CodeSnippet $null } |
            Should -Throw -Because "Null code snippet should be rejected"
        }
    }

    Context "AI Connectivity" -Tag 'Integration' {
        It "Should handle missing API key gracefully" {
            # Temporarily remove API key
            $OriginalKey = $env:XAI_API_KEY
            $env:XAI_API_KEY = $null

            try {
                $Result = Initialize-BusBuddyAI
                $Result | Should -Be $false -Because "Should return false when API key is missing"
            }
            finally {
                $env:XAI_API_KEY = $OriginalKey
            }
        }

        It "Should initialize successfully with valid API key" -Skip:(-not $env:XAI_API_KEY) {
            $Result = Initialize-BusBuddyAI
            $Result | Should -Be $true -Because "Should initialize successfully with valid API key"
        }
    }

    Context "Error Handling" {
        It "Should throw meaningful errors for invalid file paths" {
            { Invoke-BusBuddyAICodeReview -FilePath 'C:\NonExistent\File.cs' } |
            Should -Throw -Because "Should handle non-existent files gracefully"
        }
    }
}

AfterAll {
    # Clean up
    Remove-Module BusBuddy.AI -Force -ErrorAction SilentlyContinue
}
