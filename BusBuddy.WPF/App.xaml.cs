using System.Configuration;
using System.Data;
using System.Windows;
using Syncfusion.Licensing;
using Syncfusion.SfSkinManager;

namespace BusBuddy.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Register your Syncfusion license key
        var licenseKey = System.Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY");
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(licenseKey);

        var window = new MainWindow();
        // Set the application theme. You can easily change this to other themes like:
        // "FluentLight", "Metro", "Office2019Colorful", "MaterialDark", etc.
        SfSkinManager.SetTheme(window, new Theme("Office2019Colorful"));
        window.Show();

        base.OnStartup(e);
    }
}

