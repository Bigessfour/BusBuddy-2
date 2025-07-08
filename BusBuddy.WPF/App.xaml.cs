using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Licensing;
using Syncfusion.SfSkinManager;
using BusBuddy.WPF.ViewModels;
using BusBuddy.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;

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

        // Build configuration for Serilog and DI
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Initialize Serilog for robust logging
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.File("BusBuddy.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14, shared: true)
            .WriteTo.Console()
            .CreateLogger();

        // Global exception handlers for robust error capture
        AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
        {
            Log.Fatal(ex.ExceptionObject as Exception, "[FATAL] Unhandled exception");
            Log.CloseAndFlush();
        };
        this.DispatcherUnhandledException += (s, ex) =>
        {
            Log.Error(ex.Exception, "[ERROR] Dispatcher unhandled exception");
            Log.CloseAndFlush();
        };
        this.Exit += (s, ex) => Log.CloseAndFlush();

        // Setup DI
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        ConfigureServices(services, configuration);
        Services = services.BuildServiceProvider();

        var window = new MainWindow();
        SfSkinManager.SetTheme(window, new Theme("Office2019Colorful"));
        window.Show();

        base.OnStartup(e);
    }

    private void ConfigureServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services, IConfiguration configuration)
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
        // Register DbContext using SQL Server and connection string from appsettings.json
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<BusBuddyDbContext>(options =>
            options.UseSqlServer(connectionString));
        // Register IScheduleService with real implementation
        services.AddScoped<BusBuddy.Core.Services.IScheduleService, BusBuddy.Core.Services.ScheduleService>();
        // Register IStudentService with real implementation
        services.AddScoped<BusBuddy.Core.Services.IStudentService, BusBuddy.Core.Services.StudentService>();
        // Register IDriverService with real implementation
        services.AddScoped<BusBuddy.Core.Services.IDriverService, BusBuddy.Core.Services.DriverService>();
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
        // Register Route service
        services.AddScoped<BusBuddy.Core.Services.IRouteService, BusBuddy.Core.Services.RouteService>();
        // Register Driver service
        services.AddScoped<BusBuddy.Core.Services.IDriverService, BusBuddy.Core.Services.DriverService>();
        // Register ScheduleManagementViewModel for DI
        services.AddScoped<BusBuddy.WPF.ViewModels.ScheduleManagementViewModel>();
    }
}

