# BusBuddy.XAML PowerShell Module
# Reusable XAML processing functions for Bus Buddy development tools

@{
    ModuleVersion = '1.0.0'
    Description = 'Reusable XAML processing functions for Bus Buddy development'
    Author = 'Bus Buddy Development Team'
    PowerShellVersion = '7.0'
    FunctionsToExport = @(
        'Test-XamlSyntax',
        'Backup-XamlFiles',
        'Safe-BulkXamlUpdate',
        'Remove-XmlAttribute-Safe',
        'Format-XamlFile',
        'Get-XamlFileInfo'
    )
    AliasesToExport = @(
        'xaml-test',
        'xaml-backup',
        'xaml-format'
    )
}
