
using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Licensing;
using Syncfusion.SfSkinManager;
using BusBuddy.WPF.ViewModels;
using BusBuddy.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace BusBuddy.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public IServiceProvider? Services { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        // Register your Syncfusion license key
        var licenseKey = System.Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY");
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(licenseKey);

        // Setup DI
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        var window = new MainWindow();
        SfSkinManager.SetTheme(window, new Theme("Office2019Colorful"));
        window.Show();

        base.OnStartup(e);
    }

    private void ConfigureServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services)
    {
        // Register your view models and services here
        services.AddScoped<DashboardViewModel>();
        services.AddScoped<ActivityLogViewModel>();
        // Register DbContext (update connection string as needed)
        services.AddDbContext<BusBuddyDbContext>(options =>
            options.UseSqlite("Data Source=busbuddy.db"));
        // Register IScheduleService implementation
        services.AddScoped<IScheduleService, ScheduleService>();
        // Register ActivityLogService for logging
        services.AddScoped<BusBuddy.Core.Services.IActivityLogService, BusBuddy.Core.Services.ActivityLogService>();
        // Add other services as needed
    }
}

