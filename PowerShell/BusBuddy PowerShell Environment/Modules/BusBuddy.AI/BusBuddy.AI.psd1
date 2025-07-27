@{
    # Module manifest for BusBuddy.AI
    RootModule             = 'BusBuddy.AI.psm1'
    ModuleVersion          = '1.0.0'
    GUID                   = 'a8c1f8a9-5b2c-4d3e-9f8a-7b6c5d4e3f2a'
    Author                 = 'BusBuddy Development Team'
    CompanyName            = 'BusBuddy Project'
    Copyright              = '(c) 2025 BusBuddy Project. All rights reserved.'
    Description            = 'BusBuddy AI Integration Module - Mandatory AI Tools for Development using xAI Grok-4-0709'

    # Minimum version of the PowerShell engine required by this module
    PowerShellVersion      = '7.5'

    # Minimum version of the .NET Framework required by this module
    DotNetFrameworkVersion = '8.0'

    # Modules that must be imported into the global environment prior to importing this module
    RequiredModules        = @('PSAISuite')

    # Functions to export from this module
    FunctionsToExport      = @(
        'Initialize-BusBuddyAI',
        'Invoke-BusBuddyAICodeGeneration',
        'Invoke-BusBuddyAICodeReview',
        'Invoke-BusBuddyAIArchitectureAnalysis'
    )

    # Cmdlets to export from this module
    CmdletsToExport        = @()

    # Variables to export from this module
    VariablesToExport      = @()

    # Aliases to export from this module
    AliasesToExport        = @(
        'bb-ai-init',
        'bb-ai-generate',
        'bb-ai-review',
        'bb-ai-architect'
    )

    # DSC resources to export from this module
    DscResourcesToExport   = @()

    # List of all modules packaged with this module
    ModuleList             = @()

    # List of all files packaged with this module
    FileList               = @('BusBuddy.AI.psm1', 'BusBuddy.AI.psd1')

    # Private data to pass to the module specified in RootModule/ModuleToProcess
    PrivateData            = @{
        PSData = @{
            # Tags applied to this module. These help with module discovery in online galleries
            Tags                       = @('BusBuddy', 'AI', 'xAI', 'Grok', 'CodeGeneration', 'Development', 'Automation')

            # A URL to the license for this module
            LicenseUri                 = ''

            # A URL to the main website for this project
            ProjectUri                 = 'https://github.com/Bigessfour/BusBuddy-2'

            # A URL to an icon representing this module
            IconUri                    = ''

            # Release notes of this module
            ReleaseNotes               = 'Initial release of BusBuddy AI Integration Module with xAI Grok-4-0709 support'

            # Prerelease string of this module
            Prerelease                 = ''

            # Flag to indicate whether the module requires explicit user acceptance for install/update/save
            RequireLicenseAcceptance   = $false

            # External dependent modules of this module
            ExternalModuleDependencies = @('PSAISuite')
        }
    }

    # HelpInfo URI of this module
    HelpInfoURI            = ''

    # Default prefix for commands exported from this module. Override the default prefix using Import-Module -Prefix
    DefaultCommandPrefix   = ''
}
