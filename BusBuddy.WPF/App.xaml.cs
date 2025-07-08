
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
        // Register GoogleEarthEngineService and RouteManagementViewModel for DI
        services.AddScoped<BusBuddy.Core.Services.GoogleEarthEngineService>();
        services.AddScoped<BusBuddy.WPF.ViewModels.RouteManagementViewModel>();
        // Register DriverManagementViewModel for DI
        services.AddScoped<BusBuddy.WPF.ViewModels.DriverManagementViewModel>();
        // Register your view models and services here
        services.AddScoped<DashboardViewModel>();
        services.AddScoped<ActivityLogViewModel>();
        // Register RoutePopulationScaffold and proxy for DI
        services.AddScoped<BusBuddy.Core.Services.IBusService, BusBuddy.Core.Services.BusService>();
        services.AddScoped<BusBuddy.Core.Services.RoutePopulationScaffold>();
        services.AddScoped<BusBuddy.WPF.Services.IRoutePopulationScaffold, BusBuddy.WPF.Services.RoutePopulationScaffoldProxy>(sp =>
            new BusBuddy.WPF.Services.RoutePopulationScaffoldProxy(sp.GetRequiredService<BusBuddy.Core.Services.RoutePopulationScaffold>()));
        services.AddScoped<BusBuddy.WPF.ViewModels.RoutePlanningViewModel>();
        // Register DbContext (update connection string as needed)
        services.AddDbContext<BusBuddyDbContext>(options =>
            options.UseSqlite("Data Source=busbuddy.db"));
        // Register IScheduleService with real implementation
        services.AddScoped<BusBuddy.Core.Services.IScheduleService, BusBuddy.Core.Services.ScheduleService>();
        // Register IStudentService with real implementation
        services.AddScoped<BusBuddy.Core.Services.IStudentService, BusBuddy.Core.Services.StudentService>();
        // Register ActivityLogService for logging (if needed for other logging)
        services.AddScoped<BusBuddy.Core.Services.IActivityLogService, BusBuddy.Core.Services.ActivityLogService>();
        // Register Fuel service and view model
        services.AddScoped<BusBuddy.Core.Services.IFuelService, BusBuddy.Core.Services.FuelService>();
        services.AddScoped<BusBuddy.WPF.ViewModels.FuelManagementViewModel>();
        // Register Settings view model
        services.AddScoped<BusBuddy.WPF.ViewModels.SettingsViewModel>();
        // Add other services as needed
        services.AddScoped<BusBuddy.WPF.Services.IDriverAvailabilityService, BusBuddy.WPF.Services.DriverAvailabilityService>();
        // Register Maintenance service and view model
        services.AddScoped<BusBuddy.Core.Services.IMaintenanceService, BusBuddy.Core.Services.MaintenanceService>();
        services.AddScoped<BusBuddy.WPF.ViewModels.MaintenanceTrackingViewModel>();
    }
}

