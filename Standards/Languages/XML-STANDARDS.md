# 🔧 XML Standards for BusBuddy

## 📋 **Overview**
This document defines the XML standards for the BusBuddy project, covering MSBuild project files, configuration files, and XML documentation standards.

## 🎯 **General XML Standards**

### **Formatting**
- **Indentation**: 2 spaces (no tabs)
- **Line Endings**: LF (Unix-style)
- **Encoding**: UTF-8 with BOM for MSBuild files
- **File Extension**: `.xml`, `.config`, `.csproj`, `.props`

### **Structure**
- **XML Declaration**: Include for standalone XML files
- **Root Element**: Single root element required
- **Namespace Declaration**: Include appropriate namespaces
- **Comments**: Use `<!-- -->` for documentation

## 📁 **File-Specific Standards**

### **MSBuild Project Files (.csproj)**

#### **BusBuddy.WPF.csproj Example**
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net9.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <Nullable>enable</Nullable>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <AssemblyTitle>BusBuddy WPF Application</AssemblyTitle>
    <AssemblyDescription>School Transportation Management System</AssemblyDescription>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.7" />
    <PackageReference Include="Serilog.AspNetCore" Version="4.0.2" />
    <PackageReference Include="Syncfusion.SfGrid.WPF" Version="30.1.40" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\BusBuddy.Core\BusBuddy.Core.csproj" />
  </ItemGroup>

</Project>
```

#### **Directory.Build.props Example**
```xml
<Project>

  <PropertyGroup>
    <LangVersion>13.0</LangVersion>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <AnalysisLevel>latest</AnalysisLevel>
    <CodeAnalysisRuleSet>$(MSBuildThisFileDirectory)BusBuddy-Practical.ruleset</CodeAnalysisRuleSet>
  </PropertyGroup>

  <PropertyGroup>
    <Company>BusBuddy Transportation Solutions</Company>
    <Product>BusBuddy</Product>
    <Copyright>Copyright © 2025 BusBuddy Transportation Solutions</Copyright>
    <AssemblyVersion>1.0.0.0</AssemblyVersion>
    <FileVersion>1.0.0.0</FileVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="3.3.4" PrivateAssets="all" />
    <PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="8.0.0" PrivateAssets="all" />
  </ItemGroup>

</Project>
```

### **Configuration Files (app.config)**

#### **WPF App.config Example**
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <startup>
    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8" />
  </startup>
  
  <connectionStrings>
    <add name="DefaultConnection" 
         connectionString="Data Source=BusBuddy.db" 
         providerName="Microsoft.Data.Sqlite" />
  </connectionStrings>
  
  <appSettings>
    <add key="Environment" value="Development" />
    <add key="EnableLogging" value="true" />
    <add key="LogLevel" value="Information" />
  </appSettings>
  
</configuration>
```

### **Code Analysis Rules (.ruleset)**

#### **BusBuddy-Practical.ruleset Example**
```xml
<?xml version="1.0" encoding="utf-8"?>
<RuleSet Name="BusBuddy Practical Rules" 
         Description="Practical code analysis rules for BusBuddy development" 
         ToolsVersion="16.0">
  
  <Rules AnalyzerId="Microsoft.CodeAnalysis.CSharp" RuleNamespace="Microsoft.CodeAnalysis.CSharp">
    <Rule Id="CS8618" Action="Warning" />
    <Rule Id="CS8625" Action="Warning" />
    <Rule Id="CS8629" Action="Warning" />
  </Rules>
  
  <Rules AnalyzerId="Microsoft.CodeAnalysis.NetAnalyzers" RuleNamespace="Microsoft.CodeAnalysis.NetAnalyzers">
    <Rule Id="CA1001" Action="Warning" />
    <Rule Id="CA1031" Action="Info" />
    <Rule Id="CA1062" Action="Info" />
  </Rules>
  
</RuleSet>
```

## 🔧 **XML Documentation Standards**

### **C# XML Documentation**
```xml
/// <summary>
/// Represents a school bus driver with associated information and qualifications.
/// </summary>
/// <remarks>
/// This class contains all necessary information for driver management including
/// licensing, certifications, and employment history.
/// </remarks>
public class Driver
{
    /// <summary>
    /// Gets or sets the unique identifier for the driver.
    /// </summary>
    /// <value>
    /// A positive integer that uniquely identifies the driver in the system.
    /// </value>
    public int DriverId { get; set; }

    /// <summary>
    /// Gets or sets the driver's full name.
    /// </summary>
    /// <value>
    /// The complete name of the driver, including first and last name.
    /// </value>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the name is null or empty.
    /// </exception>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Validates the driver's license information.
    /// </summary>
    /// <param name="licenseNumber">The license number to validate.</param>
    /// <param name="expirationDate">The license expiration date.</param>
    /// <returns>
    /// <c>true</c> if the license is valid; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the license number format is invalid.
    /// </exception>
    public bool ValidateLicense(string licenseNumber, DateTime expirationDate)
    {
        // Implementation
    }
}
```

### **XML Documentation Tags**
- **`<summary>`**: Brief description of the element
- **`<remarks>`**: Additional detailed information
- **`<param>`**: Parameter description
- **`<returns>`**: Return value description
- **`<exception>`**: Exceptions that may be thrown
- **`<example>`**: Usage examples
- **`<see>`**: Cross-references to other elements
- **`<value>`**: Property value description

## 🛡️ **Security Standards**

### **Sensitive Data in XML**
```xml
<!-- ❌ NEVER DO THIS -->
<connectionStrings>
  <add name="Production" 
       connectionString="Server=prod;Database=BusBuddy;User=admin;Password=secret123;" />
</connectionStrings>

<!-- ✅ DO THIS INSTEAD -->
<connectionStrings>
  <add name="Production" 
       connectionString="Server=${DB_SERVER};Database=${DB_NAME};Integrated Security=true;" />
</connectionStrings>
```

### **Configuration Transformation**
```xml
<!-- appsettings.Production.config -->
<?xml version="1.0" encoding="utf-8"?>
<configuration xmlns:xdt="http://schemas.microsoft.com/XML-Document-Transform">
  
  <connectionStrings>
    <add name="DefaultConnection" 
         connectionString="${AZURE_SQL_CONNECTION_STRING}"
         xdt:Transform="SetAttributes" 
         xdt:Locator="Match(name)" />
  </connectionStrings>
  
  <appSettings>
    <add key="Environment" value="Production" xdt:Transform="SetAttributes" xdt:Locator="Match(key)" />
  </appSettings>
  
</configuration>
```

## 📊 **Validation Standards**

### **Schema Validation**
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <!-- Content with proper namespace -->
</Project>
```

### **Well-Formed XML Requirements**
- **Single Root Element**: Only one root element
- **Proper Nesting**: All elements properly nested
- **Attribute Quotes**: All attribute values quoted
- **Case Sensitivity**: Consistent element and attribute casing
- **End Tags**: All elements properly closed

## 🔍 **Formatting Standards**

### **Indentation Rules**
```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net9.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="Serilog" Version="4.0.2" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.7" />
  </ItemGroup>
</Project>
```

### **Attribute Formatting**
```xml
<!-- Short attributes on same line -->
<PackageReference Include="Serilog" Version="4.0.2" />

<!-- Long attributes on separate lines -->
<PackageReference 
    Include="Microsoft.EntityFrameworkCore.SqlServer" 
    Version="9.0.7" 
    PrivateAssets="all" />
```

## 📁 **File Organization**

### **Project Structure**
```
BusBuddy/
├── Directory.Build.props           # Global MSBuild properties
├── BusBuddy-Practical.ruleset     # Code analysis rules
├── BusBuddy.sln                   # Solution file
├── BusBuddy.Core/
│   ├── BusBuddy.Core.csproj       # Core project file
│   └── app.config                 # Core configuration
├── BusBuddy.WPF/
│   ├── BusBuddy.WPF.csproj        # WPF project file
│   └── app.config                 # WPF configuration
└── BusBuddy.Tests/
    └── BusBuddy.Tests.csproj      # Test project file
```

### **Naming Conventions**
- **Project Files**: `{ProjectName}.csproj`
- **Configuration**: `app.config`, `web.config`
- **Properties**: `Directory.Build.props`, `Directory.Build.targets`
- **Rules**: `{ProjectName}.ruleset`

## ✅ **Quality Checklist**

### **Before Committing XML Files**
- [ ] Valid XML syntax (well-formed)
- [ ] Consistent 2-space indentation
- [ ] Proper namespace declarations
- [ ] No sensitive data hardcoded
- [ ] Comments for complex configurations
- [ ] Schema validation passes
- [ ] Appropriate file encoding (UTF-8)

### **MSBuild Specific Checklist**
- [ ] SDK-style project format used
- [ ] Target framework explicitly specified
- [ ] Package versions pinned
- [ ] No unnecessary ItemGroups or PropertyGroups
- [ ] Project references use relative paths
- [ ] Documentation generation enabled

## 🛠️ **Tools and Integration**

### **Recommended Tools**
- **VS Code**: XML extension for validation and IntelliSense
- **Visual Studio**: Built-in MSBuild support
- **XMLSpy**: Advanced XML editing and validation
- **MSBuild**: Command-line build tool

### **Build Integration**
- **Schema Validation**: Automatic XML schema validation
- **Format Checking**: Automated formatting verification
- **Security Scanning**: Check for hardcoded secrets

---

**Document Version**: 1.0  
**Last Updated**: July 30, 2025  
**Applies To**: All XML files in BusBuddy project  
**Next Review**: Monthly standards review
