# PSScriptAnalyzer Custom Rules for Bus Buddy Project
# Place this file in the project root to enable custom analysis rules

@{
    # Include default rules
    IncludeDefaultRules = $true

    # Severity levels to include
    Severity = @('Error', 'Warning', 'Information')

    # Custom rules for Bus Buddy project
    Rules = @{
        # Enforce proper namespace usage
        PSUseConsistentWhitespace = @{
            Enable = $true
            CheckInnerBrace = $true
            CheckOpenBrace = $true
            CheckOperator = $true
            CheckPipe = $true
            CheckSeparator = $true
        }

        # Enforce consistent indentation
        PSUseConsistentIndentation = @{
            Enable = $true
            IndentationSize = 4
            PipelineIndentation = 'IncreaseIndentationForFirstPipeline'
            Kind = 'space'
        }

        # Check for approved verbs
        PSUseApprovedVerbs = @{
            Enable = $true
        }

        # Enforce ShouldProcess for cmdlets that make changes
        PSUseShouldProcessForStateChangingFunctions = @{
            Enable = $true
        }

        # Check for proper error handling
        PSAvoidUsingPlainTextForPassword = @{
            Enable = $true
        }
    }

    # Exclude certain files from analysis
    ExcludeRules = @()

    # Include specific file patterns
    IncludeRules = @(
        'PSAvoidGlobalVars',
        'PSAvoidUsingCmdletAliases',
        'PSReservedCmdletChar',
        'PSReservedParams',
        'PSShouldProcess',
        'PSMissingModuleManifestField',
        'PSAvoidDefaultValueSwitchParameter'
    )
}
