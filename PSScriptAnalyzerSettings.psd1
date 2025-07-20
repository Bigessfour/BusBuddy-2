@{
    # PSScriptAnalyzer settings for Bus Buddy PowerShell Development
    # See: https://github.com/PowerShell/PSScriptAnalyzer/blob/development/README.md

    # Enable all rules by default
    IncludeDefaultRules = $true

    # Severity levels to include
    Severity            = @('Error', 'Warning', 'Information')

    # Rules to exclude (if needed)
    ExcludeRules        = @(
        # Allow Write-Host in Bus Buddy scripts for user interaction
        'PSAvoidUsingWriteHost'
        # Temporarily exclude these if they conflict with existing code style
        # 'PSUseShouldProcessForStateChangingFunctions'
    )

    # Custom rule settings
    Rules               = @{
        # Enforce approved PowerShell verbs
        PSUseApprovedVerbs                   = @{
            Enable = $true
        }

        # Consistent indentation (spaces, 4 characters)
        PSUseConsistentIndentation           = @{
            Enable              = $true
            Kind                = 'space'
            IndentationSize     = 4
            PipelineIndentation = 'IncreaseIndentationAfterEveryPipeline'
        }

        # Consistent whitespace usage
        PSUseConsistentWhitespace            = @{
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

        # Enforce CmdletBinding for advanced functions
        PSUseCmdletBindingAttribute          = @{
            Enable = $true
        }

        # Require OutputType attribute for functions
        PSUseOutputTypeCorrectly             = @{
            Enable = $true
        }

        # Process blocks for pipeline functions
        PSUseProcessBlockForPipelineCommand  = @{
            Enable = $true
        }

        # Use declared vars only
        PSUseDeclaredVarsMoreThanAssignments = @{
            Enable = $true
        }

        # Compatible cmdlets - Disabled to avoid profile issues
        # PSUseCompatibleCmdlets               = @{
        #     Enable        = $true
        #     Compatibility = @('Core-6.1.0-Windows', 'Core-6.1.0-Linux', 'Desktop-5.1.14393.206-Windows')
        # }

        # Compatible syntax - Simplified for PowerShell 7.5.2
        # PSUseCompatibleSyntax                = @{
        #     Enable         = $true
        #     TargetVersions = @('7.0', '7.1', '7.2', '7.3', '7.4', '7.5')
        # }

        # Compatible types - Simplified for PowerShell 7.5.2
        # PSUseCompatibleTypes                 = @{
        #     Enable         = $true
        #     TargetVersions = @('7.0', '7.1', '7.2', '7.3', '7.4', '7.5')
        # }

        # Avoid using aliases in scripts - Allow common ones for Bus Buddy
        PSAvoidUsingCmdletAliases            = @{
            Enable    = $true
            Whitelist = @('cd', 'ls', 'cat', 'cp', 'mv', 'rm', 'echo', 'where') # Common aliases for interactive scripts
        }

        # Use literal paths
        PSUseLiteralInitializerForHashtable  = @{
            Enable = $true
        }

        # Avoid using positional parameters
        PSAvoidUsingPositionalParameters     = @{
            Enable = $true
        }

        # Use here strings for multi-line strings
        PSUseToExportFieldsInManifest        = @{
            Enable = $true
        }
    }
}
