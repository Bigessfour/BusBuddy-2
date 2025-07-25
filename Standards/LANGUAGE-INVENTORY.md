# 🌍 BusBuddy Complete Language & Technology Inventory

## **Discovered Languages & Technologies**

### 🎯 **Primary Languages**

#### 1. **C# 12.0** ⭐ **PRIMARY**
- **Version**: 12.0 (auto-detected from .NET 8.0)
- **Framework**: .NET 8.0.412 (net8.0-windows)
- **Official Documentation**: [Microsoft Learn C# 12.0](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12)
- **Usage**: Core application logic, WPF UI layer, business models
- **Key Features Used**:
  - Primary Constructors
  - Collection Expressions `[1, 2, 3]`
  - Ref Readonly Parameters
  - Using Aliases for Any Type
  - Nullable Reference Types (enabled)

#### 2. **XAML** ⭐ **PRIMARY UI**
- **Framework**: WPF for .NET 8.0
- **Official Documentation**: [WPF XAML Overview](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/xaml/)
- **Usage**: WPF UI definitions, data binding, styles, resources
- **Standards Source**: [WPF Data Binding](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/data/)

#### 3. **PowerShell 7.5.2** ⭐ **AUTOMATION**
- **Version**: 7.5.2 (PowerShell Core)
- **Official Documentation**: [PowerShell 7.5](https://learn.microsoft.com/en-us/powershell/scripting/whats-new/what-s-new-in-powershell-75)
- **Usage**: Build automation, development workflows, task management
- **Configuration**: `PSScriptAnalyzerSettings.psd1`

### 🔧 **Configuration Languages**

#### 4. **JSON** ⭐ **CONFIGURATION**
- **Standard**: JSON Schema Draft 2020-12
- **Official Specification**: [JSON.org](https://www.json.org/) | [RFC 8259](https://tools.ietf.org/html/rfc8259)
- **Usage**: Application settings, package management, build configuration
- **Files**: `appsettings.json`, `global.json`, `packages.lock.json`

#### 5. **XML** ⭐ **BUILD SYSTEM**
- **Version**: XML 1.0 (Fifth Edition)
- **Official Specification**: [W3C XML](https://www.w3.org/TR/xml/) | [W3C XML Schema](https://www.w3.org/TR/xmlschema-1/)
- **Usage**: MSBuild files, project configuration, test settings
- **Files**: `*.csproj`, `Directory.Build.props`, `testsettings.runsettings.xml`

#### 6. **YAML 1.2** ⭐ **CI/CD**
- **Version**: YAML 1.2
- **Official Specification**: [YAML.org](https://yaml.org/spec/1.2.2/) | [GitHub Actions YAML](https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions)
- **Usage**: GitHub Actions workflows, CodeCov configuration
- **Files**: `.github/workflows/build-and-test.yml`, `codecov.yml`

### 🏗️ **Framework Technologies**

#### 7. **WPF (Windows Presentation Foundation)**
- **Version**: .NET 8.0 WPF
- **Official Documentation**: [WPF Documentation](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)
- **Usage**: Desktop UI framework
- **Key Components**: Data binding, MVVM, styles, templates

#### 8. **Entity Framework Core 9.0.7**
- **Version**: 9.0.7
- **Official Documentation**: [EF Core 9.0](https://learn.microsoft.com/en-us/ef/core/)
- **Usage**: Database ORM, data access layer
- **Provider**: SQL Server

#### 9. **Syncfusion WPF 30.1.40**
- **Version**: 30.1.40
- **Official Documentation**: [Syncfusion WPF](https://help.syncfusion.com/wpf/welcome-to-syncfusion-essential-wpf)
- **Usage**: Advanced WPF controls, themes, charts
- **License**: Commercial license required

### 📊 **Data & Database**

#### 10. **SQL Server**
- **Provider**: Microsoft SQL Server (LocalDB/Express)
- **Connection**: `Server=localhost\\SQLEXPRESS`
- **Official Documentation**: [SQL Server Documentation](https://learn.microsoft.com/en-us/sql/sql-server/)
- **Usage**: Primary database for BusBuddy data

#### 11. **T-SQL**
- **Version**: SQL Server T-SQL
- **Official Documentation**: [T-SQL Reference](https://learn.microsoft.com/en-us/sql/t-sql/)
- **Usage**: Database queries, stored procedures, migrations

### 🛠️ **Tool Configuration Languages**

#### 12. **EditorConfig**
- **Version**: EditorConfig Core 0.12.4
- **Official Documentation**: [EditorConfig.org](https://editorconfig.org/)
- **Usage**: Cross-editor coding style enforcement
- **File**: `.editorconfig`

#### 13. **MSBuild**
- **Version**: 17.11.31 (from .NET SDK 8.0.412)
- **Official Documentation**: [MSBuild Reference](https://learn.microsoft.com/en-us/visualstudio/msbuild/)
- **Usage**: Build system, project configuration
- **Files**: `*.csproj`, `Directory.Build.props`, `*.targets`

#### 14. **PowerShell Data File (.psd1)**
- **Version**: PowerShell 7.5.2 compatible
- **Official Documentation**: [about_PowerShell_Data_Files](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_powershell_data_files)
- **Usage**: PSScriptAnalyzer configuration
- **File**: `PSScriptAnalyzerSettings.psd1`

### 🌐 **Web Technologies**

#### 15. **WebView2**
- **Version**: 1.0.3351.48
- **Official Documentation**: [WebView2 Documentation](https://learn.microsoft.com/en-us/microsoft-edge/webview2/)
- **Usage**: Embedded web browser for Google Earth integration

### 📦 **Package Management**

#### 16. **NuGet**
- **Version**: Integrated with .NET SDK 8.0.412
- **Official Documentation**: [NuGet Documentation](https://learn.microsoft.com/en-us/nuget/)
- **Usage**: .NET package management
- **Files**: `packages.lock.json`, package references in `.csproj`

#### 17. **Google APIs**
- **Version**: 1.70.0
- **Official Documentation**: [Google APIs .NET Client](https://developers.google.com/api-client-library/dotnet/)
- **Usage**: Google Earth Engine integration

## **Version Summary**

| Technology | Version | Status | Documentation |
|------------|---------|--------|---------------|
| **C#** | 12.0 | ✅ Current | [Microsoft Learn C# 12.0](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12) |
| **.NET** | 8.0.412 | ✅ LTS | [.NET 8.0 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8) |
| **PowerShell** | 7.5.2 | ✅ Current | [PowerShell 7.5](https://learn.microsoft.com/en-us/powershell/scripting/whats-new/what-s-new-in-powershell-75) |
| **WPF** | .NET 8.0 | ✅ Current | [WPF Documentation](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/) |
| **EF Core** | 9.0.7 | ✅ Current | [EF Core 9.0](https://learn.microsoft.com/en-us/ef/core/) |
| **Syncfusion** | 30.1.40 | ✅ Current | [Syncfusion WPF](https://help.syncfusion.com/wpf/welcome-to-syncfusion-essential-wpf) |
| **JSON** | RFC 8259 | ✅ Standard | [JSON.org](https://www.json.org/) |
| **XML** | 1.0 (5th Ed) | ✅ Standard | [W3C XML](https://www.w3.org/TR/xml/) |
| **YAML** | 1.2 | ✅ Standard | [YAML.org](https://yaml.org/spec/1.2.2/) |
| **MSBuild** | 17.11.31 | ✅ Current | [MSBuild Reference](https://learn.microsoft.com/en-us/visualstudio/msbuild/) |

---
**Generated**: July 25, 2025
**Total Languages**: 17
**Primary Languages**: C# 12.0, XAML, PowerShell 7.5.2
**Configuration Languages**: JSON, XML, YAML, EditorConfig
**Framework Technologies**: WPF, Entity Framework Core, Syncfusion
