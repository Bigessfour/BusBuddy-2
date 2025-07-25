# ========================================
# PSScriptAnalyzer Settings for BusBuddy
# PowerShell 7.5.2 Compatible Configuration
# VALIDATED against Microsoft official documentation
# https://learn.microsoft.com/en-us/powershell/utility-modules/psscriptanalyzer/using-scriptanalyzer
# ========================================

@{
    # Include default rules (required for baseline functionality)
    IncludeDefaultRules   = $true

    # Only use valid severity levels as per Microsoft docs
    Severity              = @('Error', 'Warning', 'Information')

    # Exclude rules that conflict with PowerShell 7.5 modern syntax
    ExcludeRules          = @(
        'PSUseDeclaredVarsMoreThanAssignments',  # False positives with modern syntax
        'PSAvoidUsingPositionalParameters',     # Allow modern parameter syntax in PS 7.5
        'PSUseShouldProcessForStateChangingFunctions',  # Not needed for all utility functions
        'PSAvoidGlobalVars',                     # Allow global vars for configuration
        'PSAvoidUsingWriteHost'                  # Allow Write-Host for user feedback in PS 7.5
    )

    # Include specific rules for PowerShell 7.5 best practices
    IncludeRules          = @(
        'PSUseCompatibleSyntax',                 # Ensure syntax compatibility
        'PSUseCompatibleCommands',               # Ensure command compatibility
        'PSReviewUnusedParameter',               # Code quality
        'PSUsePSCredentialType'                  # Security best practices
    )

    # Custom rule paths (empty for now, as per Microsoft docs format)
    CustomRulePath        = @()

    # Do not recurse into custom rule directories
    RecurseCustomRulePath = $false
}
