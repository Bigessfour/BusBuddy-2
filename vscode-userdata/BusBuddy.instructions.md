# 🚌 BusBuddy CS0103 Error Prevention Guide

## 🚨 **Critical CS0103 Error Prevention Standards**

This file contains **MANDATORY** coding standards to prevent CS0103 "The name 'X' does not exist in the current context" errors in the BusBuddy WPF application.

### **📋 Quick Reference: CS0103 Root Causes**

| **Error Pattern** | **Root Cause** | **Immediate Fix** | **Prevention** |
|---|---|---|---|
| `InitializeComponent()` not found | Missing `partial` keyword | Add `partial` to class | Always use `partial class` for WPF |
| XAML element names not found | Designer files not generated | Clean + Rebuild | Regular clean builds |
| Syncfusion controls not found | Missing package references | Check project packages | Verify Directory.Build.props |
| Extension methods not found | Missing using directives | Add proper `using` statements | Follow using standards |

### **🎯 Emergency CS0103 Resolution Protocol**

```powershell
# STEP 1: Quick Fix (90% success rate)
dotnet clean BusBuddy.WPF/BusBuddy.WPF.csproj
dotnet build BusBuddy.WPF/BusBuddy.WPF.csproj

# STEP 2: Force Package Restore (95% success rate)
dotnet restore --force --no-cache
dotnet clean
dotnet build --verbosity minimal

# STEP 3: Nuclear Option (99% success rate)
Remove-Item -Recurse -Force BusBuddy.WPF/bin/, BusBuddy.WPF/obj/
dotnet restore
dotnet build
```

### **✅ MANDATORY Coding Standards**

#### **1. WPF Class Declaration Standards**

```csharp
// ✅ CORRECT: Always use partial class for WPF
namespace BusBuddy.WPF.Views
{
    public partial class DashboardView : UserControl  // 'partial' is REQUIRED
    {
        public DashboardView()
        {
            InitializeComponent(); // Auto-generated, requires 'partial'
        }
    }
}

// ❌ WRONG: Missing partial keyword
public class DashboardView : UserControl  // Will cause CS0103 for InitializeComponent()
```

#### **2. Using Directive Standards**

```csharp
// ✅ MANDATORY using statements for BusBuddy WPF files
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Serilog;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using BusBuddy.WPF.ViewModels;

// ✅ SYNCFUSION using statements (when using Syncfusion controls)
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.Licensing;
using Syncfusion.Themes.FluentDark.WPF;
```

#### **3. XAML-CodeBehind Synchronization**

```xml
<!-- ✅ CORRECT XAML Declaration -->
<UserControl x:Class="BusBuddy.WPF.Views.DashboardView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <!-- x:Name creates auto-generated properties -->
    <Button x:Name="SaveButton" Click="SaveButton_Click" />
    <DataGrid x:Name="DriversDataGrid" />
</UserControl>
```

```csharp
// ✅ MATCHING Code-Behind (namespace and class name must match XAML)
namespace BusBuddy.WPF.Views  // Must match XAML namespace
{
    public partial class DashboardView : UserControl  // Must match XAML class
    {
        public DashboardView()
        {
            InitializeComponent();
            
            // SaveButton and DriversDataGrid are auto-generated from x:Name
            SaveButton.Click += SaveButton_Click;
        }
        
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Event handler implementation
        }
    }
}
```

#### **4. Package Reference Validation**

```xml
<!-- ✅ REQUIRED packages in BusBuddy.WPF.csproj -->
<PackageReference Include="Microsoft.Extensions.Hosting" Version="$(EntityFrameworkVersion)" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="$(EntityFrameworkVersion)" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="$(EntityFrameworkVersion)" />
<PackageReference Include="Serilog" Version="$(SerilogVersion)" />

<!-- ✅ Syncfusion packages (inherited from Directory.Build.props) -->
<PackageReference Include="Syncfusion.SfGrid.WPF" Version="$(SyncfusionVersion)" />
<PackageReference Include="Syncfusion.Themes.FluentDark.WPF" Version="$(SyncfusionVersion)" />
```

### **🔧 Build Validation Standards**

#### **Pre-Commit Validation (MANDATORY)**

```powershell
# ✅ REQUIRED before every git commit
dotnet clean BusBuddy.WPF/BusBuddy.WPF.csproj
dotnet build BusBuddy.WPF/BusBuddy.WPF.csproj --verbosity minimal

# ✅ Validate build success
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build successful - ready to commit" -ForegroundColor Green
    git add .
    git commit -m "fix: resolve CS0103 errors in [ComponentName]"
} else {
    Write-Host "❌ Build failed - fix CS0103 errors before committing" -ForegroundColor Red
    # DO NOT COMMIT
}
```

#### **IntelliSense vs Build Reality**

**CRITICAL UNDERSTANDING**: Due to OmniSharp deprecation in this project:

- ✅ **Trust the build output** - If `dotnet build` succeeds, code is correct
- ⚠️ **Ignore IntelliSense red squiggles** - May show false CS0103 errors
- 🎯 **Focus on actual compilation** - Only fix real build failures
- 📋 **Use build verbosity** - `--verbosity detailed` for true error details

### **🚫 Common CS0103 Mistakes to Avoid**

| **Mistake** | **Consequence** | **Fix** |
|---|---|---|
| Forgetting `partial` keyword | `InitializeComponent()` CS0103 | Add `partial` to class declaration |
| Wrong namespace in XAML | Designer files not generated | Match XAML and code-behind namespaces |
| Missing using directives | Type not found errors | Add complete using statements |
| Manual deletion of designer files | Loss of auto-generated code | Rebuild project to regenerate |
| Editing .g.cs files manually | Build conflicts | Never edit auto-generated files |

### **📝 File Organization Standards**

```
BusBuddy.WPF/
├── Views/
│   ├── Dashboard/
│   │   ├── DashboardView.xaml           # x:Class must match code-behind
│   │   └── DashboardView.xaml.cs        # Must use partial class
│   ├── Driver/
│   │   ├── DriversView.xaml
│   │   └── DriversView.xaml.cs          # Namespace must match XAML
│   └── Vehicle/
│       ├── VehiclesView.xaml
│       └── VehiclesView.xaml.cs
├── ViewModels/                          # No XAML = no partial needed
├── Services/                            # No XAML = no partial needed
└── obj/Debug/                           # Contains auto-generated .g.cs files
    └── Views/
        ├── DashboardView.g.cs           # AUTO-GENERATED - never edit
        ├── DriversView.g.cs             # Contains InitializeComponent()
        └── VehiclesView.g.cs            # Regenerated on build
```

### **🎯 Success Metrics**

**Zero CS0103 Errors Achieved When:**
- ✅ All WPF code-behind classes use `partial` keyword
- ✅ XAML and code-behind namespaces match exactly
- ✅ All required using directives included
- ✅ Build succeeds with `--verbosity minimal`
- ✅ No manual editing of auto-generated files
- ✅ Regular clean builds performed

**Build Command Success Pattern:**
```
dotnet build BusBuddy.WPF/BusBuddy.WPF.csproj
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

## 📚 **Additional Resources**

- **WPF Partial Classes**: [Microsoft WPF Documentation](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
- **XAML Compilation**: [XAML in WPF](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/xaml/)
- **Syncfusion WPF**: [Syncfusion Documentation](https://help.syncfusion.com/wpf/)
- **Build Troubleshooting**: See `.vscode/instructions.md` for complete debugging guide

---

**Last Updated**: August 2, 2025 - CS0103 Error Prevention Standards v1.0
