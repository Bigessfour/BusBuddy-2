using System.Configuration;
using System.Data;
using System.Windows;
using Syncfusion.Licensing;

namespace BusBuddy.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        var licenseKey = System.Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY");
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(licenseKey);
    }
}

