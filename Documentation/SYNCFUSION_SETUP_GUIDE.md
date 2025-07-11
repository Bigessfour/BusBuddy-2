# Syncfusion Setup Guide for BusBuddy

**Date:** July 10, 2025  
**Version:** 1.0  
**Target:** Development Team & System Administrators

## 📋 Overview

This guide provides step-by-step instructions for setting up Syncfusion components in the BusBuddy application, including license configuration, package installation, and troubleshooting common issues.

## 🎯 Prerequisites

- Visual Studio 2022 or later
- .NET 8.0 SDK
- NuGet Package Manager
- Syncfusion account (for license key)

## 📦 Package Installation

### 1. Required Syncfusion Packages

Add the following NuGet packages to your projects:

**BusBuddy.WPF Project:**
```xml
<PackageReference Include="Syncfusion.SfGrid.WPF" Version="26.1.35" />
<PackageReference Include="Syncfusion.SfChart.WPF" Version="26.1.35" />
<PackageReference Include="Syncfusion.Tools.WPF" Version="26.1.35" />
<PackageReference Include="Syncfusion.SfRibbon.WPF" Version="26.1.35" />
<PackageReference Include="Syncfusion.SfDocking.WPF" Version="26.1.35" />
<PackageReference Include="Syncfusion.SfNavigationDrawer.WPF" Version="26.1.35" />
<PackageReference Include="Syncfusion.Themes.MaterialLight.WPF" Version="26.1.35" />
```

### 2. Installation Commands

**Via Package Manager Console:**
```powershell
Install-Package Syncfusion.SfGrid.WPF -Version 26.1.35
Install-Package Syncfusion.SfChart.WPF -Version 26.1.35
Install-Package Syncfusion.Tools.WPF -Version 26.1.35
Install-Package Syncfusion.SfRibbon.WPF -Version 26.1.35
Install-Package Syncfusion.SfDocking.WPF -Version 26.1.35
Install-Package Syncfusion.SfNavigationDrawer.WPF -Version 26.1.35
Install-Package Syncfusion.Themes.MaterialLight.WPF -Version 26.1.35
```

**Via .NET CLI:**
```bash
dotnet add package Syncfusion.SfGrid.WPF --version 26.1.35
dotnet add package Syncfusion.SfChart.WPF --version 26.1.35
dotnet add package Syncfusion.Tools.WPF --version 26.1.35
```

## 🔑 License Configuration

### 1. Obtaining License Key

1. **Community License (Free):**
   - Register at [Syncfusion Community License](https://www.syncfusion.com/products/communitylicense)
   - Download license key from your account dashboard

2. **Commercial License:**
   - Purchase from [Syncfusion Store](https://www.syncfusion.com/sales/products)
   - Access license key in customer portal

### 2. License Registration

**Method 1: Application Configuration (Recommended)**

Add to `appsettings.json`:
```json
{
  "Syncfusion": {
    "LicenseKey": "YOUR_LICENSE_KEY_HERE"
  }
}
```

Register in `App.xaml.cs`:
```csharp
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Register Syncfusion license
        var configuration = BuildConfiguration();
        var licenseKey = configuration["Syncfusion:LicenseKey"];
        
        if (!string.IsNullOrEmpty(licenseKey))
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(licenseKey);
        }
        else
        {
            throw new InvalidOperationException("Syncfusion license key not found in configuration");
        }
        
        base.OnStartup(e);
    }
}
```

**Method 2: Direct Registration**
```csharp
// In App.xaml.cs or MainWindow constructor
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY_HERE");
```

### 3. Environment-Specific Configuration

**Development Environment:**
```json
// appsettings.Development.json
{
  "Syncfusion": {
    "LicenseKey": "DEVELOPMENT_LICENSE_KEY"
  }
}
```

**Production Environment:**
```json
// appsettings.Production.json
{
  "Syncfusion": {
    "LicenseKey": "PRODUCTION_LICENSE_KEY"
  }
}
```

## 🎨 Theme Configuration

### 1. Material Light Theme Setup

Add to `App.xaml`:
```xml
<Application x:Class="BusBuddy.WPF.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:syncfusionThemes="clr-namespace:Syncfusion.Themes.MaterialLight.WPF;assembly=Syncfusion.Themes.MaterialLight.WPF">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <syncfusionThemes:MaterialLightThemeSettings x:Key="MaterialLight" 
                                                           PrimaryBackground="#FF2196F3"
                                                           PrimaryForeground="#FFFFFFFF"/>
                <ResourceDictionary Source="/Syncfusion.Themes.MaterialLight.WPF;component/MSControl/Button.xaml"/>
                <ResourceDictionary Source="/Syncfusion.Themes.MaterialLight.WPF;component/SfGrid/SfDataGrid.xaml"/>
                <ResourceDictionary Source="/Syncfusion.Themes.MaterialLight.WPF;component/SfChart/SfChart.xaml"/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

### 2. Apply Theme to Syncfusion Controls

```csharp
// In MainWindow or UserControl constructor
SfSkinManager.SetTheme(this, new MaterialLightTheme());
```

## 🔧 Component Configuration

### 1. SfDataGrid Setup

```xml
<syncfusion:SfDataGrid x:Name="BusDataGrid"
                       ItemsSource="{Binding Buses}"
                       AutoGenerateColumns="False"
                       AllowEditing="True"
                       AllowFiltering="True"
                       AllowSorting="True">
    <syncfusion:SfDataGrid.Columns>
        <syncfusion:GridTextColumn MappingName="BusNumber" HeaderText="Bus #"/>
        <syncfusion:GridTextColumn MappingName="Make" HeaderText="Make"/>
        <syncfusion:GridTextColumn MappingName="Model" HeaderText="Model"/>
    </syncfusion:SfDataGrid.Columns>
</syncfusion:SfDataGrid>
```

### 2. SfChart Configuration

```xml
<syncfusion:SfChart x:Name="FuelChart">
    <syncfusion:SfChart.PrimaryAxis>
        <syncfusion:CategoryAxis Header="Month"/>
    </syncfusion:SfChart.PrimaryAxis>
    <syncfusion:SfChart.SecondaryAxis>
        <syncfusion:NumericalAxis Header="Gallons"/>
    </syncfusion:SfChart.SecondaryAxis>
    <syncfusion:ColumnSeries ItemsSource="{Binding FuelData}"
                            XBindingPath="Month"
                            YBindingPath="Gallons"/>
</syncfusion:SfChart>
```

### 3. SfRibbon Setup

```xml
<syncfusion:SfRibbon x:Name="MainRibbon">
    <syncfusion:SfRibbonTab Caption="HOME">
        <syncfusion:SfRibbonBar Header="Dashboard">
            <syncfusion:SfRibbonButton Label="Overview" 
                                      LargeIcon="Images/overview.png"
                                      Command="{Binding ShowOverviewCommand}"/>
        </syncfusion:SfRibbonBar>
    </syncfusion:SfRibbonTab>
</syncfusion:SfRibbon>
```

## 🚨 Troubleshooting

### Common Issues

**1. License Validation Error**
```
Error: "The included Syncfusion license is invalid"
```
**Solution:**
- Verify license key is correct
- Check license key hasn't expired
- Ensure correct license type (Community vs Commercial)
- Register license before using any Syncfusion controls

**2. Assembly Loading Issues**
```
Error: "Could not load file or assembly 'Syncfusion.SfGrid.WPF'"
```
**Solution:**
- Check NuGet packages are properly installed
- Verify all dependencies are included
- Clean and rebuild solution
- Check target framework compatibility

**3. Theme Not Applied**
```
Issue: Controls don't show Material Light theme
```
**Solution:**
- Ensure theme resource dictionaries are merged in App.xaml
- Apply theme using SfSkinManager.SetTheme()
- Check theme assembly references

**4. Control Not Rendering**
```
Issue: Syncfusion controls appear blank or with errors
```
**Solution:**
- Verify license registration occurs before control initialization
- Check XAML namespace declarations
- Ensure proper data binding syntax

### Debug Configuration

Add debug logging for Syncfusion initialization:

```csharp
private void InitializeSyncfusion()
{
    try
    {
        var licenseKey = _configuration["Syncfusion:LicenseKey"];
        _logger.LogDebug("[DEBUG] Syncfusion Initialization: License key length: {Length}", 
            licenseKey?.Length ?? 0);
        
        if (string.IsNullOrEmpty(licenseKey))
        {
            _logger.LogError("[DEBUG] Syncfusion Initialization: License key is missing");
            throw new InvalidOperationException("Syncfusion license key not configured");
        }
        
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(licenseKey);
        _logger.LogDebug("[DEBUG] Syncfusion Initialization: License registered successfully");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "[DEBUG] Syncfusion Initialization: Failed to register license");
        throw;
    }
}
```

## 📚 Additional Resources

- [Syncfusion WPF Documentation](https://help.syncfusion.com/wpf/overview)
- [Syncfusion License Registration Guide](https://help.syncfusion.com/common/essential-studio/licensing/license-key)
- [Syncfusion Theme Studio](https://help.syncfusion.com/wpf/themes/theme-studio)
- [Community Support](https://www.syncfusion.com/forums/wpf)

## 🔄 Version Compatibility

| BusBuddy Version | Syncfusion Version | .NET Version | Status |
|------------------|-------------------|--------------|--------|
| 1.0.0           | 26.1.35           | .NET 8.0     | ✅ Current |
| 1.0.0           | 25.x.x            | .NET 8.0     | ⚠️ Compatible |
| 1.0.0           | 24.x.x            | .NET 8.0     | ❌ Not Tested |

## 📞 Support

For Syncfusion-specific issues:
- Check official documentation first
- Contact BusBuddy development team for application-specific issues
- Use Syncfusion support forums for component-specific problems
- Maintain current license for priority support
