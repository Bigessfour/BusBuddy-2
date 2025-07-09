using BusBuddy.Core.Data;
using BusBuddy.Core.Data.UnitOfWork;
using BusBuddy.WPF.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Syncfusion.SfSkinManager;
using System.IO;
using System.Windows;

namespace BusBuddy.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        string? envLicenseKey = Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY");
        string appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

        // Check for appsettings.json before loading configuration
        if (!File.Exists(appSettingsPath) && string.IsNullOrWhiteSpace(envLicenseKey))
        {
            MessageBox.Show(
                $"The configuration file 'appsettings.json' was not found at '{appSettingsPath}'.\n\n" +
                "Please ensure the file exists and is set to 'Copy if newer' in the project, or set the SYNCFUSION_LICENSE_KEY environment variable.",
                "Configuration File Missing",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
            return;
        }

        string? licenseKey = !string.IsNullOrWhiteSpace(envLicenseKey)
            ? envLicenseKey
            : new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build()["Syncfusion:LicenseKey"]
                ?? new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build()["SyncfusionLicenseKey"];

        if (string.IsNullOrWhiteSpace(licenseKey))
        {
            MessageBox.Show(
                "Syncfusion license key is missing. Please set the SYNCFUSION_LICENSE_KEY environment variable or add it to appsettings.json.",
                "License Key Missing",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
            return;
        }
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(licenseKey);

        // Build configuration for Serilog and DI
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Determine the solution root directory
        string solutionRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName
                           ?? Directory.GetCurrentDirectory();

        // Centralize all logs in the logs directory
        string logsDirectory = Path.Combine(solutionRoot, "logs");
        string buildLogPath = Path.Combine(logsDirectory, "build.log");
        string runtimeLogPath = Path.Combine(logsDirectory, "busbuddy-.log");
        string fallbackLogPath = Path.Combine(logsDirectory, "BusBuddy_fallback.log");

        // Ensure the logs directory exists
        if (!Directory.Exists(logsDirectory))
        {
            Directory.CreateDirectory(logsDirectory);
        }

        // Register logging with DI so ILogger<T> can be injected
        ServiceCollection services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddSerilog();
        });

        // Initialize Serilog for robust logging, with fallback
        try
        {
            // Use the Serilog configuration from appsettings.json
            // but override the file paths to ensure they're in the logs directory
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ApplicationVersion", typeof(App).Assembly.GetName().Version?.ToString() ?? "Unknown")
                .Enrich.WithProperty("MachineName", Environment.MachineName)
                .Enrich.WithProperty("ProcessId", Environment.ProcessId)
                .Enrich.With(new BusBuddy.Core.Logging.QueryTrackingEnricher()) // Register custom enricher
                .WriteTo.File(buildLogPath, rollingInterval: RollingInterval.Infinite, shared: true)
                .WriteTo.File(runtimeLogPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7, shared: true)
                .WriteTo.Console()
                .CreateLogger();
            Log.Information("[STARTUP] BusBuddy WPF application is starting. Debugging log attached.");
        }
        catch (Exception serilogEx)
        {
            // Fallback: write to a basic log file if Serilog config fails
            System.IO.File.AppendAllText(fallbackLogPath, $"[FATAL] Serilog init failed: {serilogEx}\n");
        }

        // Global exception handlers for robust error capture
        AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
        {
            try
            {
                Log.Fatal(ex.ExceptionObject as Exception, "[FATAL] Unhandled exception");
                Log.CloseAndFlush();
            }
            catch (Exception logEx)
            {
                // Ensure fallback logging goes to the logs directory
                System.IO.File.AppendAllText(fallbackLogPath, $"[FATAL] Unhandled exception: {ex.ExceptionObject}\n[LOGGING ERROR]: {logEx}\n");
            }
        };
        this.DispatcherUnhandledException += (s, ex) =>
        {
            try
            {
                Log.Error(ex.Exception, "[ERROR] Dispatcher unhandled exception");
                Log.CloseAndFlush();
            }
            catch (Exception logEx)
            {
                // Ensure fallback logging goes to the logs directory
                System.IO.File.AppendAllText(fallbackLogPath, $"[ERROR] Dispatcher unhandled exception: {ex.Exception}\n[LOGGING ERROR]: {logEx}\n");
            }
        };
        this.Exit += (s, ex) => Log.CloseAndFlush();

        // Add configuration to DI container
        services.AddSingleton<IConfiguration>(configuration);

        // Setup DI
        ConfigureServices(services, configuration);
        Services = services.BuildServiceProvider();

        MainWindow window = Services.GetRequiredService<MainWindow>();
        SfSkinManager.SetTheme(window, new Theme("Office2019Colorful"));
        window.Show();

        base.OnStartup(e);
    }

    private void ConfigureServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services, IConfiguration configuration)
    {
        // Register logging so ILogger<T> can be injected into services
        services.AddLogging();

        // Register memory caching services
        services.AddMemoryCache();

        // Register caching service
        services.AddSingleton<BusBuddy.Core.Services.IBusCachingService, BusBuddy.Core.Services.BusCachingService>();

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
        services.AddScoped<BusBuddy.Core.Services.IRouteService, BusBuddy.Core.Services.RouteService>();
        services.AddScoped<BusBuddy.Core.Services.RoutePopulationScaffold>();
        services.AddScoped<BusBuddy.WPF.Services.IRoutePopulationScaffold, BusBuddy.WPF.Services.RoutePopulationScaffoldProxy>(sp =>
            new BusBuddy.WPF.Services.RoutePopulationScaffoldProxy(sp.GetRequiredService<BusBuddy.Core.Services.RoutePopulationScaffold>()));
        services.AddScoped<BusBuddy.WPF.ViewModels.RoutePlanningViewModel>();
        // Register DbContext using SQL Server and connection string from appsettings.json
        string? connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<BusBuddyDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Student related ViewModels
        services.AddTransient<StudentListViewModel>();


        services.AddTransient<BusBuddyDbContext>(provider =>
        {
            DbContextOptionsBuilder<BusBuddyDbContext> optionsBuilder = new DbContextOptionsBuilder<BusBuddyDbContext>();

            // Configure for SQL Server (in production)
            optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
            {
                // Configure SQL options for better performance and resilience
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);

                // Improve command timeout for complex queries
                sqlOptions.CommandTimeout(60);
            });

            // Set query tracking behavior for better performance
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            // Configure warnings
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Log(CoreEventId.MultipleNavigationProperties));

            // Enable sensitive data logging in development
#if DEBUG
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
#endif

            return new BusBuddyDbContext(optionsBuilder.Options);
        });

        // Register DbContextFactory for proper context creation in async scenarios
        services.AddSingleton<IBusBuddyDbContextFactory, BusBuddyDbContextFactory>();

        // Register specialized repositories for thread-safe data access
        services.AddScoped<BusBuddy.Core.Data.Repositories.IVehicleRepository, BusBuddy.Core.Data.Repositories.VehicleRepository>();
        services.AddScoped<BusBuddy.Core.Services.IUserContextService, BusBuddy.Core.Services.UserContextService>();

        // Register IScheduleService with real implementation
        services.AddScoped<BusBuddy.Core.Services.Interfaces.IScheduleService, BusBuddy.Core.Services.ScheduleService>();
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
        // Register Driver service 
        services.AddScoped<BusBuddy.Core.Services.IDriverService, BusBuddy.Core.Services.DriverService>();
        // Register Configuration service
        services.AddScoped<BusBuddy.Core.Services.IConfigurationService, BusBuddy.Core.Services.ConfigurationService>();
        // Register ScheduleManagementViewModel for DI
        services.AddScoped<BusBuddy.WPF.ViewModels.ScheduleManagementViewModel>();
        services.AddTransient<MainWindow>();
    }
}

