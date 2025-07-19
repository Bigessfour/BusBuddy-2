# Syncfusion WPF Implementation Validator
# READ-ONLY analysis tool to ensure correct Syncfusion implementation according to official documentation

#region Syncfusion Standards Analysis

function Test-SyncfusionImplementation {
    <#
    .SYNOPSIS
    Validates Syncfusion WPF implementation against official documentation standards
    .DESCRIPTION
    READ-ONLY: Analyzes files for proper Syncfusion theme implementation, namespace usage, and control configuration
    #>
    param(
        [Parameter(Mandatory)]
        [string]$ProjectPath
    )

    $analysis = @{
        IsValid = $true
        Errors = @()
        Warnings = @()
        ThemeImplementation = @{
            HasSkinManager = $false
            ThemeAssemblies = @()
            GlobalThemeSet = $false
            ApplyThemeAsDefaultStyle = $false
            SupportedThemes = @('FluentLight', 'FluentDark', 'Windows11Light', 'Windows11Dark', 'Material3Light', 'Material3Dark')
            CurrentTheme = $null
        }
        NamespaceUsage = @{
            CoreNamespace = $false  # xmlns:syncfusion="http://schemas.syncfusion.com/wpf"
            SkinManagerNamespace = $false  # xmlns:syncfusionskin="clr-namespace:Syncfusion.SfSkinManager;assembly=Syncfusion.SfSkinManager.WPF"
        }
        ControlsFound = @()
        ResourceKeys = @()
        BestPractices = @{
            LicenseRegistered = $false
            ThemeSettingsRegistered = $false
            ProperDisposal = $false
        }
    }

    # Analyze all XAML files
    $xamlFiles = Get-ChildItem -Path $ProjectPath -Filter "*.xaml" -Recurse
    foreach ($file in $xamlFiles) {
        $xamlAnalysis = Test-XamlSyncfusionImplementation -FilePath $file.FullName

        # Merge analysis results
        if ($xamlAnalysis.NamespaceUsage.CoreNamespace) {
            $analysis.NamespaceUsage.CoreNamespace = $true
        }
        if ($xamlAnalysis.NamespaceUsage.SkinManagerNamespace) {
            $analysis.NamespaceUsage.SkinManagerNamespace = $true
        }

        $analysis.ControlsFound += $xamlAnalysis.ControlsFound
        $analysis.ResourceKeys += $xamlAnalysis.ResourceKeys
        $analysis.Errors += $xamlAnalysis.Errors
        $analysis.Warnings += $xamlAnalysis.Warnings
    }

    # Analyze C# files for theme setup
    $csFiles = Get-ChildItem -Path $ProjectPath -Filter "*.cs" -Recurse
    foreach ($file in $csFiles) {
        $csAnalysis = Test-CSharpSyncfusionImplementation -FilePath $file.FullName

        if ($csAnalysis.ThemeImplementation.GlobalThemeSet) {
            $analysis.ThemeImplementation.GlobalThemeSet = $true
        }
        if ($csAnalysis.ThemeImplementation.ApplyThemeAsDefaultStyle) {
            $analysis.ThemeImplementation.ApplyThemeAsDefaultStyle = $true
        }
        if ($csAnalysis.BestPractices.LicenseRegistered) {
            $analysis.BestPractices.LicenseRegistered = $true
        }

        $analysis.Errors += $csAnalysis.Errors
        $analysis.Warnings += $csAnalysis.Warnings
    }

    # Validate overall implementation
    $analysis = Test-SyncfusionBestPractices -Analysis $analysis

    return $analysis
}

function Test-XamlSyncfusionImplementation {
    <#
    .SYNOPSIS
    Analyzes XAML files for proper Syncfusion implementation
    #>
    param(
        [Parameter(Mandatory)]
        [string]$FilePath
    )

    $analysis = @{
        Errors = @()
        Warnings = @()
        NamespaceUsage = @{
            CoreNamespace = $false
            SkinManagerNamespace = $false
        }
        ControlsFound = @()
        ResourceKeys = @()
        ThemeUsage = @()
    }

    try {
        $content = Get-Content $FilePath -Raw
        [xml]$xaml = $content

        # Check namespaces
        foreach ($attr in $xaml.DocumentElement.Attributes) {
            if ($attr.Value -eq "http://schemas.syncfusion.com/wpf") {
                $analysis.NamespaceUsage.CoreNamespace = $true
            }
            if ($attr.Value -eq "clr-namespace:Syncfusion.SfSkinManager;assembly=Syncfusion.SfSkinManager.WPF") {
                $analysis.NamespaceUsage.SkinManagerNamespace = $true
            }
        }

        # Find Syncfusion controls
        $syncfusionControls = @(
            'SfDataGrid', 'SfChart', 'SfDiagram', 'SfNavigationDrawer', 'SfBusyIndicator',
            'SfTextBoxExt', 'SfDatePicker', 'SfTimePicker', 'SfScheduler', 'SfRichTextBoxAdv',
            'DockingManager', 'SfAccordion', 'SfTreeView', 'SfTreeGrid', 'SfTabControl'
        )

        foreach ($control in $syncfusionControls) {
            $controlMatches = [regex]::Matches($content, "<syncfusion:$control")
            foreach ($match in $controlMatches) {
                $lineNumber = ($content.Substring(0, $match.Index) -split "`n").Count
                $analysis.ControlsFound += @{
                    Control = $control
                    Line = $lineNumber
                    File = $FilePath
                }
            }
        }

        # Check for theme usage
        $themeMatches = [regex]::Matches($content, 'syncfusionskin:SfSkinManager\.Theme="{syncfusionskin:SkinManagerExtension ThemeName=([^}]+)}"')
        foreach ($match in $themeMatches) {
            $themeName = $match.Groups[1].Value
            $lineNumber = ($content.Substring(0, $match.Index) -split "`n").Count
            $analysis.ThemeUsage += @{
                Theme = $themeName
                Line = $lineNumber
                File = $FilePath
            }
        }

        # Check for resource key usage
        $resourceKeyMatches = [regex]::Matches($content, '{sfskin:ThemeResource ThemeKey={sfskin:ThemeKey Key=([^}]+)}}')
        foreach ($match in $resourceKeyMatches) {
            $resourceKey = $match.Groups[1].Value
            $lineNumber = ($content.Substring(0, $match.Index) -split "`n").Count
            $analysis.ResourceKeys += @{
                Key = $resourceKey
                Line = $lineNumber
                File = $FilePath
            }
        }

        # Validate implementation
        if ($analysis.ControlsFound.Count -gt 0 -and -not $analysis.NamespaceUsage.CoreNamespace) {
            $analysis.Errors += "Syncfusion controls found but core namespace 'http://schemas.syncfusion.com/wpf' is missing"
        }

        if ($analysis.ThemeUsage.Count -gt 0 -and -not $analysis.NamespaceUsage.SkinManagerNamespace) {
            $analysis.Errors += "Theme usage found but SkinManager namespace is missing"
        }

    } catch {
        $analysis.Errors += "XAML parsing error: $($_.Exception.Message)"
    }

    return $analysis
}

function Test-CSharpSyncfusionImplementation {
    <#
    .SYNOPSIS
    Analyzes C# files for proper Syncfusion theme and license implementation
    #>
    param(
        [Parameter(Mandatory)]
        [string]$FilePath
    )

    $analysis = @{
        Errors = @()
        Warnings = @()
        ThemeImplementation = @{
            GlobalThemeSet = $false
            ApplyThemeAsDefaultStyle = $false
            ThemeSetBeforeInitialize = $false
        }
        BestPractices = @{
            LicenseRegistered = $false
            ThemeSettingsRegistered = $false
            ProperDisposal = $false
        }
    }

    try {
        $content = Get-Content $FilePath -Raw
        $lines = $content -split "`n"

        # Check for license registration
        if ($content -match 'Syncfusion\.Licensing\.SyncfusionLicenseProvider\.RegisterLicense') {
            $analysis.BestPractices.LicenseRegistered = $true
        }

        # Check for global theme setting
        if ($content -match 'SfSkinManager\.ApplicationTheme\s*=\s*new\s+Theme\s*\(') {
            $analysis.ThemeImplementation.GlobalThemeSet = $true
        }

        # Check for ApplyThemeAsDefaultStyle
        if ($content -match 'SfSkinManager\.ApplyThemeAsDefaultStyle\s*=\s*true') {
            $analysis.ThemeImplementation.ApplyThemeAsDefaultStyle = $true
        }

        # Check for theme settings registration
        if ($content -match 'SfSkinManager\.RegisterThemeSettings') {
            $analysis.BestPractices.ThemeSettingsRegistered = $true
        }

        # Check for proper disposal
        if ($content -match 'SfSkinManager\.Dispose\s*\(') {
            $analysis.BestPractices.ProperDisposal = $true
        }

        # Validate order of operations
        $applyThemeLine = -1
        $initComponentLine = -1

        for ($i = 0; $i -lt $lines.Count; $i++) {
            if ($lines[$i] -match 'SfSkinManager\.ApplyThemeAsDefaultStyle\s*=\s*true') {
                $applyThemeLine = $i
            }
            if ($lines[$i] -match 'InitializeComponent\s*\(\s*\)') {
                $initComponentLine = $i
            }
        }

        if ($applyThemeLine -ne -1 -and $initComponentLine -ne -1) {
            if ($applyThemeLine -lt $initComponentLine) {
                $analysis.ThemeImplementation.ThemeSetBeforeInitialize = $true
            } else {
                $analysis.Errors += "SfSkinManager.ApplyThemeAsDefaultStyle must be set BEFORE InitializeComponent() call"
            }
        }

    } catch {
        $analysis.Errors += "C# analysis error: $($_.Exception.Message)"
    }

    return $analysis
}

function Test-SyncfusionBestPractices {
    <#
    .SYNOPSIS
    Validates overall Syncfusion implementation against best practices
    #>
    param($Analysis)

    # Add warnings for missing best practices
    if (-not $Analysis.BestPractices.LicenseRegistered) {
        $Analysis.Warnings += "Syncfusion license not registered - add Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense() call"
    }

    if (-not $Analysis.ThemeImplementation.ApplyThemeAsDefaultStyle) {
        $Analysis.Warnings += "SfSkinManager.ApplyThemeAsDefaultStyle not set to true"
    }

    if ($Analysis.ControlsFound.Count -gt 0 -and -not $Analysis.ThemeImplementation.GlobalThemeSet) {
        $Analysis.Warnings += "Syncfusion controls found but no global theme set via SfSkinManager.ApplicationTheme"
    }

    if (-not $Analysis.BestPractices.ProperDisposal) {
        $Analysis.Warnings += "No SfSkinManager.Dispose() call found - should be called in Window_Closed event"
    }

    return $Analysis
}

#endregion

#region Syncfusion Theme and Control Validation

function Get-SyncfusionThemeInfo {
    <#
    .SYNOPSIS
    Returns official Syncfusion theme information and validation rules
    #>

    return @{
        SupportedThemes = @{
            'FluentLight' = @{
                Assembly = 'Syncfusion.Themes.FluentLight.Wpf.dll'
                NuGet = 'Syncfusion.Themes.FluentLight.WPF'
                ThemeSettings = 'FluentLightThemeSettings'
                Palette = 'FluentPalette'
            }
            'FluentDark' = @{
                Assembly = 'Syncfusion.Themes.FluentDark.Wpf.dll'
                NuGet = 'Syncfusion.Themes.FluentDark.WPF'
                ThemeSettings = 'FluentDarkThemeSettings'
                Palette = 'FluentPalette'
            }
            'Windows11Light' = @{
                Assembly = 'Syncfusion.Themes.Windows11Light.Wpf.dll'
                NuGet = 'Syncfusion.Themes.Windows11Light.WPF'
                ThemeSettings = 'Windows11LightThemeSettings'
                Palette = 'Windows11Palette'
            }
            'Windows11Dark' = @{
                Assembly = 'Syncfusion.Themes.Windows11Dark.Wpf.dll'
                NuGet = 'Syncfusion.Themes.Windows11Dark.WPF'
                ThemeSettings = 'Windows11DarkThemeSettings'
                Palette = 'Windows11Palette'
            }
        }

        FrameworkResourceKeys = @{
            'Button' = 'WPFButtonStyle'
            'PrimaryButton' = 'WPFPrimaryButtonStyle'
            'FlatButton' = 'WPFFlatButtonStyle'
            'TextBox' = 'WPFTextBoxStyle'
            'ComboBox' = 'WPFComboBoxStyle'
            'DataGrid' = 'WPFDataGridStyle'
            'TabControl' = 'WPFTabControlStyle'
        }

        SyncfusionResourceKeys = @{
            'SfDataGrid' = 'SyncfusionSfDataGridStyle'
            'SfNavigationDrawer' = 'SyncfusionSfNavigationDrawerStyle'
            'DockingManager' = 'SyncfusionDockingManagerStyle'
            'SfChart' = 'SyncfusionSfChartStyle'
            'SfBusyIndicator' = 'SyncfusionSfBusyIndicatorStyle'
            'SfTextBoxExt' = 'SyncfusionSfTextBoxExtStyle'
            'SfScheduler' = 'SyncfusionSfSchedulerStyle'
        }

        RequiredNamespaces = @{
            'Core' = 'xmlns:syncfusion="http://schemas.syncfusion.com/wpf"'
            'SkinManager' = 'xmlns:syncfusionskin="clr-namespace:Syncfusion.SfSkinManager;assembly=Syncfusion.SfSkinManager.WPF"'
        }

        ImplementationTemplate = @{
            'XamlNamespaces' = @'
xmlns:syncfusion="http://schemas.syncfusion.com/wpf"
xmlns:syncfusionskin="clr-namespace:Syncfusion.SfSkinManager;assembly=Syncfusion.SfSkinManager.WPF"
'@

            'WindowTheme' = @'
syncfusionskin:SfSkinManager.Theme="{syncfusionskin:SkinManagerExtension ThemeName=FluentDark}"
'@

            'CSharpSetup' = @'
// In App.xaml.cs constructor or MainWindow constructor
SfSkinManager.ApplyThemeAsDefaultStyle = true;
SfSkinManager.ApplicationTheme = new Theme("FluentDark");
'@

            'LicenseRegistration' = @'
// In App.xaml.cs constructor, before any Syncfusion control usage
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY");
'@
        }
    }
}

function Test-SyncfusionProjectStructure {
    <#
    .SYNOPSIS
    Validates project structure for proper Syncfusion implementation
    #>
    param(
        [Parameter(Mandatory)]
        [string]$ProjectPath
    )

    $structure = @{
        IsValid = $true
        Issues = @()
        Recommendations = @()
        ProjectFiles = @{
            HasCsprojReferences = $false
            HasPackagesConfig = $false
            SyncfusionReferences = @()
        }
        ResourceStructure = @{
            HasResourceDictionaries = $false
            ThemeResources = @()
            CustomStyles = @()
        }
    }

    # Check project files
    $csprojFiles = Get-ChildItem -Path $ProjectPath -Filter "*.csproj" -Recurse
    foreach ($csproj in $csprojFiles) {
        $content = Get-Content $csproj.FullName -Raw

        # Check for Syncfusion NuGet references
        $syncfusionRefs = [regex]::Matches($content, '<PackageReference Include="(Syncfusion[^"]+)"')
        foreach ($ref in $syncfusionRefs) {
            $structure.ProjectFiles.SyncfusionReferences += $ref.Groups[1].Value
        }

        if ($syncfusionRefs.Count -gt 0) {
            $structure.ProjectFiles.HasCsprojReferences = $true
        }
    }

    # Check for resource dictionaries
    $resourceFiles = Get-ChildItem -Path $ProjectPath -Filter "*Resource*.xaml" -Recurse
    if ($resourceFiles.Count -gt 0) {
        $structure.ResourceStructure.HasResourceDictionaries = $true
    }

    # Validate minimum required references
    $requiredRefs = @(
        'Syncfusion.SfSkinManager.WPF',
        'Syncfusion.Themes.FluentDark.WPF'
    )

    foreach ($required in $requiredRefs) {
        if ($structure.ProjectFiles.SyncfusionReferences -notcontains $required) {
            $structure.Issues += "Missing required Syncfusion reference: $required"
        }
    }

    if ($structure.Issues.Count -gt 0) {
        $structure.IsValid = $false
    }

    return $structure
}

#endregion

# Export all functions
# Note: Functions are available when script is dot-sourced
# Available functions: Test-SyncfusionImplementation, Test-XamlSyncfusionImplementation, Test-CSharpSyncfusionImplementation, Test-SyncfusionBestPractices, Get-SyncfusionThemeInfo, Test-SyncfusionProjectStructure
