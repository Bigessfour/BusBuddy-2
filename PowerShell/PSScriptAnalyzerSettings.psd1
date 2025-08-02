@{
    # PSScriptAnalyzer configuration for BusBuddy PowerShell modules
    # This configuration enforces BusBuddy coding standards and best practices

    # Include all default rules
    IncludeDefaultRules = $true

    # Severity levels to include
    Severity            = @('Error', 'Warning', 'Information')

    # Rules to exclude (if any become problematic)
    ExcludeRules        = @(
        # Temporarily exclude if needed
    )

    # Custom rules configuration
    Rules               = @{
        # Enforce proper function naming
        PSUseSingularNouns                             = @{
            Enable = $true
        }

        # Ensure approved verbs are used
        PSUseApprovedVerbs                             = @{
            Enable = $true
        }

        # Enforce parameter validation - ESCALATED TO ERROR for unused variables
        PSUseDeclaredVarsMoreThanAssignments           = @{
            Enable   = $true
            Severity = 'Error'  # Escalate to block on analysis
        }

        # Prevent automatic variable assignments - ESCALATED TO ERROR
        PSAvoidAssignmentToAutomaticVariable           = @{
            Enable   = $true
            Severity = 'Error'  # Escalate to error for $matches, etc.
        }

        # Additional: Enforce no global vars to avoid side effects
        PSAvoidGlobalVars                              = @{
            Enable   = $true
            Severity = 'Warning'
        }

        # Security rules
        PSAvoidUsingPlainTextForPassword               = @{
            Enable = $true
        }

        PSAvoidUsingConvertToSecureStringWithPlainText = @{
            Enable = $true
        }

        # Performance rules
        PSUseShouldProcessForStateChangingFunctions    = @{
            Enable = $true
        }

        # Code style rules for BusBuddy
        PSPlaceOpenBrace                               = @{
            Enable             = $true
            OnSameLine         = $true
            NewLineAfter       = $true
            IgnoreOneLineBlock = $true
        }

        PSPlaceCloseBrace                              = @{
            Enable             = $true
            NewLineAfter       = $false
            IgnoreOneLineBlock = $true
            NoEmptyLineBefore  = $false
        }

        PSUseConsistentIndentation                     = @{
            Enable              = $true
            Kind                = 'space'
            PipelineIndentation = 'IncreaseIndentationForFirstPipeline'
            IndentationSize     = 4
        }

        PSUseConsistentWhitespace                      = @{
            Enable                          = $true
            CheckInnerBrace                 = $true
            CheckOpenBrace                  = $true
            CheckOpenParen                  = $true
            CheckOperator                   = $true
            CheckPipe                       = $true
            CheckPipeForRedundantWhitespace = $false
            CheckSeparator                  = $true
            CheckParameter                  = $false
        }

        # BusBuddy specific: Enforce Serilog logging
        PSAvoidUsingWriteHost                          = @{
            Enable = $true
        }

        # Comment-based help
        PSProvideCommentHelp                           = @{
            Enable                  = $true
            ExportedOnly            = $false
            BlockComment            = $true
            VSCodeSnippetCorrection = $false
            Placement               = "begin"
        }
    }
}
