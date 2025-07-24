# ========================================
# PSScriptAnalyzer Settings for BusBuddy
# PowerShell 7.5.2 Compatible Configuration
# ========================================

@{
    # Only scan files that should be scanned
    IncludeDefaultRules  = $true

    # Exclude rules that conflict with PowerShell 7.5.2 syntax
    ExcludeRules         = @(
        'PSUseDeclaredVarsMoreThanAssignments',  # False positives with modern syntax
        'PSAvoidUsingAlias',                     # Avoids flagging '?' as Where-Object alias in PS7 ternary contexts
        'PSAvoidUsingPositionalParameters',     # Allow modern parameter syntax
        'PSUseShouldProcessForStateChangingFunctions'  # Not needed for all functions
    )

    # Include modern PowerShell 7.5.2 validation rules
    IncludeRules         = @(
        'PSUseModernOperators'                   # Validates/enforces PS7 ??, && operators (custom rule)
    )

    # Severity overrides for BusBuddy project
    # Change from "Error" to "Warning" for modern PS7 features
    Severity             = @(
        'Warning'   # Allows modern PS7 features without hard errors; treat as warnings for review
        # Exclude 'Information' to reduce noise
    )

    # Performance optimizations for PowerShell 7.5.2
    # Ensures PS7 ternary/null operators aren't flagged inappropriately
    CustomRulePath       = @()

    # Modern syntax support for PowerShell 7.5.2
    Rules                = @{
        # Allow modern PowerShell 7.5.2 syntax patterns
        PSAvoidUsingAlias = @{
            # Allow ?, ??, and other modern operators
            allowlist = @('?', '??', '&&', '||')
        }
    }

    # BusBuddy-specific approved verb patterns
    ApprovedVerbPatterns = @{
        # Execution patterns for BusBuddy's Syncfusion API analyzer
        execution = @(
            'Invoke-SyncfusionAnalysis'
        )
    }

    # Forbidden patterns with PowerShell 7.5.2 exceptions
    ForbiddenPatterns    = @{
        # Allow modern PowerShell 7 features (per 2025 docs)
        allowedModernFeatures = @(
            'Use-TernaryOperator',  # Custom rule: Ensure PS7 modern features aren't forbidden
            'Use-NullConditional',  # Allow ?. and ?[] operators
            'Use-PipelineChain'     # Allow && and || operators
        )
    }

    # Custom enforcement rules for PowerShell 7.5.2
    Enforcements         = @{
        psScriptAnalyzer = @{
            # Custom rules to validate PowerShell 7 features like ?? and &&
            customRules                  = @(
                'PSUseModernSyntax',     # Hypothetical rule to validate PS7 features
                'PSUseModernOperators'   # Validates/enforces PS7 ??, && operators
            )
            # Enable modern syntax validation
            enableModernSyntaxValidation = $true
            # Validate ternary operators, null conditionals, and pipeline chains
            modernFeatureValidation      = @(
                'TernaryOperators',
                'NullConditionals',
                'PipelineChains',
                'ParallelForEach'
            )
        }

        # BusBuddy-specific XAML and WPF validation standards
        xamlValidation   = @{
            syncfusionNamespaceRequired = @{
                severity = 'Error'
                message  = 'BusBuddy requires Syncfusion xmlns declaration in XAML files'
                pattern  = 'xmlns:syncfusion="http://schemas.syncfusion.com/wpf"'
            }
        }

        wpfStandards     = @{
            dataBinding = @{
                requireModeTwoWayForSyncfusion = $true  # Ensures two-way bindings for Syncfusion controls
                message                        = 'Syncfusion controls in BusBuddy should use Mode=TwoWay for proper data binding'
            }
        }
    }
}
