using BusBuddy.Core.Data;
using BusBuddy.Core.Data.UnitOfWork;
using BusBuddy.WPF.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Syncfusion.SfSkinManager;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace BusBuddy.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IHost _host;

    /// <summary>
    /// Gets the service provider for the application.
    /// </summary>
    public IServiceProvider Services { get; private set; } = default!;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                // Use the application's ConfigureServices method for all registrations
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();
                ConfigureServices(services, configuration);
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        // Security check: Ensure sensitive data logging is not enabled in production
        if (!BusBuddy.Core.Utilities.EnvironmentHelper.IsDevelopment() &&
            Environment.GetEnvironmentVariable("ENABLE_SENSITIVE_DATA_LOGGING") == "true")
        {
            MessageBox.Show(
                "SECURITY RISK: Sensitive data logging is enabled in a non-development environment. " +
                "This configuration is insecure and should only be used in development.\n\n" +
                "The application will now exit for security reasons.",
                "Security Risk Detected",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
            return;
        }

        // --- BEGIN: Enhanced Build log diagnostics ---
        string buildLogTestPath = Path.Combine(
            Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName ?? Directory.GetCurrentDirectory(),
            "logs", "build.log");
        try
        {
            // Ensure the directory exists
            var logDir = Path.GetDirectoryName(buildLogTestPath)!;
            Directory.CreateDirectory(logDir);

            // Log detailed startup information
            string startupInfo = $"[BUILDLOG TEST] OnStartup entered at {DateTime.Now:O}\n" +
                                 $"[BUILDLOG TEST] AppDomain.CurrentDomain.BaseDirectory: {AppDomain.CurrentDomain.BaseDirectory}\n" +
                                 $"[BUILDLOG TEST] Working Directory: {Environment.CurrentDirectory}\n" +
                                 $"[BUILDLOG TEST] OS Version: {Environment.OSVersion}\n" +
                                 $"[BUILDLOG TEST] .NET Version: {Environment.Version}\n" +
                                 $"[BUILDLOG TEST] App Version: {typeof(App).Assembly.GetName().Version}\n" +
                                 $"[BUILDLOG TEST] Process ID: {Environment.ProcessId}\n" +
                                 $"[BUILDLOG TEST] Log Directory: {logDir}\n";

            File.AppendAllText(buildLogTestPath, startupInfo);

            // Also write to a secondary location to ensure we're getting logs somewhere
            string secondaryLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup_diagnostic.log");
            File.AppendAllText(secondaryLogPath, startupInfo);

            // Create an empty marker file that we can check for existence
            File.WriteAllText(Path.Combine(logDir, "app_started.marker"), DateTime.Now.ToString("o"));
        }
        catch (Exception ex)
        {
            // If the primary logging fails, use multiple fallbacks
            try
            {
                // Fallback 1: app base directory
                string fallback1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "buildlog_fallback.txt");
                File.AppendAllText(fallback1, $"[BUILDLOG TEST] Could not write to logs/build.log: {ex}\n" +
                                            $"[BUILDLOG TEST] Exception details: {ex.ToString()}\n" +
                                            $"[BUILDLOG TEST] Inner exception: {ex.InnerException?.ToString()}\n");

                // Fallback 2: current directory
                string fallback2 = Path.Combine(Environment.CurrentDirectory, "buildlog_fallback_current.txt");
                File.AppendAllText(fallback2, $"[BUILDLOG TEST] Could not write to logs/build.log: {ex}\n");

                // Fallback 3: temp directory
                string fallback3 = Path.Combine(Path.GetTempPath(), "BusBuddy_buildlog_fallback.txt");
                File.AppendAllText(fallback3, $"[BUILDLOG TEST] Could not write to logs/build.log: {ex}\n");
            }
            catch (Exception fallbackEx)
            {
                // Last resort - try to output to console
                Console.WriteLine($"CRITICAL: All logging fallbacks failed. Initial error: {ex.Message}, Fallback error: {fallbackEx.Message}");
            }
        }
        // --- END: Enhanced Build log diagnostics ---

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
                .Enrich.WithProperty("BuildTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .WriteTo.File(buildLogPath,
                    rollingInterval: RollingInterval.Infinite,
                    shared: true,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{MachineName}] [{ThreadId}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(runtimeLogPath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    shared: true,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{MachineName}] [{ThreadId}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information("[STARTUP] BusBuddy WPF application is starting. Build completed at {BuildTime}", DateTime.Now);
            Log.Information("[STARTUP] Logs directory: {LogsDirectory}", logsDirectory);
            Log.Information("[STARTUP] Build log: {BuildLogPath}", buildLogPath);
            Log.Information("[STARTUP] Runtime log: {RuntimeLogPath}", runtimeLogPath);
        }
        catch (Exception serilogEx)
        {
            // Fallback: write to a basic log file if Serilog config fails
            string fallbackMessage = $"[FATAL] [{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] Serilog init failed: {serilogEx}\n";
            File.AppendAllText(fallbackLogPath, fallbackMessage);

            // Also try to write to console for immediate feedback
            Console.WriteLine($"SERILOG INIT FAILED: {serilogEx.Message}");
        }

        // Enhanced global exception handlers for comprehensive error capture
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        this.DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        this.Exit += (s, ex) => Log.CloseAndFlush();

        // Add configuration to DI container
        services.AddSingleton<IConfiguration>(configuration);

        // Setup DI
        ConfigureServices(services, configuration);

        // Set the Services property to the host's service provider for backward compatibility
        Services = _host.Services;

        // Pre-warm the bus cache on startup to reduce initial DB load
        // This is done in a background task to avoid blocking the UI
        _ = Task.Run(async () =>
        {
            try
            {
                // Wait a short delay to allow the application to fully initialize
                await Task.Delay(2000);

                // Get the caching service (which should be a singleton)
                var busCacheService = this.Services.GetService<BusBuddy.Core.Services.IBusCachingService>();
                if (busCacheService != null)
                {
                    // Define a data fetching function that creates a fresh scope for each call
                    Func<Task<List<BusBuddy.Core.Models.Bus>>> fetchBusesFunc = async () =>
                    {
                        // Create a new scope for each invocation to avoid DbContext disposal issues
                        using var freshScope = this.Services.CreateScope();
                        try
                        {
                            var busService = freshScope.ServiceProvider.GetRequiredService<BusBuddy.Core.Services.IBusService>();
                            var result = await busService.GetAllBusEntitiesAsync();
                            return result.ToList();
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Error fetching buses in cache pre-warming: {ErrorMessage}", ex.Message);
                            throw;
                        }
                    };

                    // Call the caching service with our data fetching function
                    await busCacheService.GetAllBusesAsync(fetchBusesFunc);
                    Log.Information("Bus cache pre-warming completed successfully");
                }
                else
                {
                    Log.Warning("Bus caching service not found - cache pre-warming skipped");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Bus cache pre-warming failed: {ErrorMessage}", ex.Message);
                // Don't throw - this is a background operation that shouldn't crash the app
            }
        });

        var serviceProvider = _host.Services;

        try
        {
            // Create marker file to indicate we reached this point
            File.WriteAllText(Path.Combine(logsDirectory, "pre_dashboard_init.marker"), DateTime.Now.ToString("o"));

            // Log that we're about to resolve the DashboardViewModel
            Log.Information("[STARTUP] Resolving DashboardViewModel from service provider");

            var dashboardViewModel = serviceProvider.GetRequiredService<DashboardViewModel>();

            // Log successful resolution
            Log.Information("[STARTUP] Successfully resolved DashboardViewModel");

            // Create another marker file
            File.WriteAllText(Path.Combine(logsDirectory, "post_dashboard_resolve.marker"), DateTime.Now.ToString("o"));

            var mainWindow = new MainWindow
            {
                DataContext = dashboardViewModel
            };

            // Log window creation
            Log.Information("[STARTUP] Created MainWindow with DashboardViewModel");

            mainWindow.Show();

            // Log window display
            Log.Information("[STARTUP] MainWindow.Show() called");

            // Create another marker file
            File.WriteAllText(Path.Combine(logsDirectory, "pre_initialize_async.marker"), DateTime.Now.ToString("o"));

            // Log that we're about to initialize the dashboard
            Log.Information("[STARTUP] Calling DashboardViewModel.InitializeAsync()");

            await dashboardViewModel.InitializeAsync();

            // Log successful initialization
            Log.Information("[STARTUP] DashboardViewModel initialization completed successfully");

            // Final marker file for successful startup
            File.WriteAllText(Path.Combine(logsDirectory, "application_started_successfully.marker"), DateTime.Now.ToString("o"));
        }
        catch (Exception startupEx)
        {
            // Log the detailed exception
            Log.Fatal(startupEx, "[STARTUP] Critical error during application startup");

            // Also write to fallback locations
            try
            {
                string startupErrorDetails = $"[FATAL ERROR] {DateTime.Now:o} - Startup failed: {startupEx}\n" +
                                            $"Stack trace: {startupEx.StackTrace}\n" +
                                            $"Inner exception: {startupEx.InnerException?.ToString() ?? "None"}\n";

                File.WriteAllText(Path.Combine(logsDirectory, "startup_failure.log"), startupErrorDetails);
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup_failure.log"), startupErrorDetails);

                MessageBox.Show(
                    $"A critical error occurred during application startup:\n\n{startupEx.Message}\n\n" +
                    "Please check the log files for details and contact technical support.",
                    "Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception logEx)
            {
                // Last resort console output
                Console.WriteLine($"CRITICAL: Application startup failed. Error: {startupEx.Message}, Logging error: {logEx.Message}");
            }

            // Ensure we shut down the application
            Shutdown();
        }
    }

    private void ConfigureServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services, IConfiguration configuration)
    {
        // Register logging so ILogger<T> can be injected into services
        services.AddLogging();

        // Register memory caching services - CRITICAL for BusCachingService
        services.AddMemoryCache();

        // Register AutoMapper services
        services.AddAutoMapper(typeof(BusBuddy.WPF.Mapping.MappingProfile));
        services.AddSingleton<BusBuddy.WPF.Services.IMappingService, BusBuddy.WPF.Services.MappingService>();

        // Register performance monitoring utility
        services.AddSingleton<BusBuddy.WPF.Utilities.PerformanceMonitor>();

        // Register DbContext using SQL Server and connection string from appsettings.json with scoped lifetime
        string? connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<BusBuddyDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                // Configure SQL options for better performance and resilience
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);

                // Improve command timeout for complex queries
                sqlOptions.CommandTimeout(60);
                // Set query splitting behavior to avoid EF Core warnings and improve performance
                sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });

            // Set query tracking behavior for better performance in read-only scenarios
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll); // Changed from NoTracking to allow updates

            // Configure warnings
            options.ConfigureWarnings(warnings =>
                warnings.Log(CoreEventId.MultipleNavigationProperties));

            // Disable sensitive data logging by default for security
            // Only enable it when explicitly running in Development mode with proper controls
            if (BusBuddy.Core.Utilities.EnvironmentHelper.IsSensitiveDataLoggingEnabled())
            {
                Log.Warning("SECURITY WARNING: Sensitive data logging is enabled. This should NEVER be used in production.");
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        }, ServiceLifetime.Scoped); // Explicitly specify scoped lifetime

        // Register DbContextFactory - CRITICAL - must be scoped, not singleton
        services.AddScoped<IBusBuddyDbContextFactory, BusBuddyDbContextFactory>();

        // Register UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register specialized repositories for thread-safe data access
        services.AddScoped<BusBuddy.Core.Data.Repositories.IVehicleRepository, BusBuddy.Core.Data.Repositories.VehicleRepository>();

        // Register caching service - depends on IMemoryCache
        services.AddSingleton<BusBuddy.Core.Services.IBusCachingService, BusBuddy.Core.Services.BusCachingService>();

        // Register Core Services
        services.AddScoped<BusBuddy.Core.Services.IBusService, BusBuddy.Core.Services.BusService>();
        services.AddScoped<BusBuddy.Core.Services.IRouteService, BusBuddy.Core.Services.RouteService>();
        services.AddScoped<BusBuddy.Core.Services.IDriverService, BusBuddy.Core.Services.DriverService>();
        services.AddScoped<BusBuddy.Core.Services.ITicketService, BusBuddy.Core.Services.TicketService>();
        services.AddScoped<BusBuddy.Core.Services.IFuelService, BusBuddy.Core.Services.FuelService>();
        services.AddScoped<BusBuddy.Core.Services.IMaintenanceService, BusBuddy.Core.Services.MaintenanceService>();
        services.AddScoped<BusBuddy.Core.Services.IUserContextService, BusBuddy.Core.Services.UserContextService>();
        services.AddScoped<BusBuddy.Core.Services.IActivityLogService, BusBuddy.Core.Services.ActivityLogService>();
        services.AddScoped<BusBuddy.Core.Services.IStudentService, BusBuddy.Core.Services.StudentService>();
        services.AddScoped<BusBuddy.Core.Services.Interfaces.IScheduleService, BusBuddy.Core.Services.ScheduleService>();
        services.AddScoped<BusBuddy.Core.Services.IConfigurationService, BusBuddy.Core.Services.ConfigurationService>();
        services.AddScoped<BusBuddy.Core.Services.GoogleEarthEngineService>();
        services.AddScoped<BusBuddy.Core.Services.RoutePopulationScaffold>();

        // Register WPF Services
        services.AddScoped<BusBuddy.WPF.Services.IDriverAvailabilityService, BusBuddy.WPF.Services.DriverAvailabilityService>();
        services.AddScoped<BusBuddy.WPF.Services.IRoutePopulationScaffold, BusBuddy.WPF.Services.RoutePopulationScaffold>();

        // Register Main Window
        services.AddTransient<MainWindow>();

        // Register ViewModels with consistent lifetimes
        // Main/Dashboard/Navigation ViewModels
        services.AddTransient<BusBuddy.WPF.ViewModels.MainViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.DashboardViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.ActivityLogViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.SettingsViewModel>();

        // Management ViewModels
        services.AddScoped<BusBuddy.WPF.ViewModels.BusManagementViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.DriverManagementViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.RouteManagementViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.ScheduleManagementViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.StudentManagementViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.MaintenanceTrackingViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.FuelManagementViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.TicketManagementViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.RoutePlanningViewModel>();

        // List ViewModels
        services.AddScoped<BusBuddy.WPF.ViewModels.StudentListViewModel>();
    }

    #region Global Exception Handling

    /// <summary>
    /// Handles unhandled exceptions in the application domain (non-UI thread exceptions)
    /// </summary>
    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var fallbackLogPath = GetFallbackLogPath();

        try
        {
            var exception = e.ExceptionObject as Exception;
            Log.Fatal(exception, "[FATAL] Unhandled domain exception - IsTerminating: {IsTerminating}", e.IsTerminating);

            if (e.IsTerminating)
            {
                Log.Fatal("Application is terminating due to unhandled exception");
                Log.CloseAndFlush();

                // Show critical error message
                MessageBox.Show(
                    "A critical error has occurred and the application must close.\n\n" +
                    "Please check the log files for details and contact support if the problem persists.",
                    "Critical Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        catch (Exception logEx)
        {
            // Ensure fallback logging goes to the logs directory
            try
            {
                File.AppendAllText(fallbackLogPath,
                    $"[FATAL] [{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] Unhandled domain exception: {e.ExceptionObject}\n" +
                    $"[LOGGING ERROR]: {logEx}\n");
            }
            catch
            {
                // Last resort - do nothing to prevent infinite recursion
            }
        }
    }

    /// <summary>
    /// Handles unhandled exceptions on the UI thread
    /// </summary>
    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var fallbackLogPath = GetFallbackLogPath();

        try
        {
            // Categorize the exception for better user messaging
            var isDbException = IsDbContextException(e.Exception);
            var isCriticalException = IsCriticalException(e.Exception);

            if (isCriticalException)
            {
                Log.Fatal(e.Exception, "[FATAL] Critical dispatcher unhandled exception");
                e.Handled = true;

                MessageBox.Show(
                    "A critical error has occurred. The application will attempt to continue, but some features may not work correctly.\n\n" +
                    "Please save your work and restart the application.",
                    "Critical Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            else if (isDbException)
            {
                Log.Error(e.Exception, "[ERROR] Database-related dispatcher exception");
                e.Handled = true;

                MessageBox.Show(
                    "A database error occurred. Please check your connection and try again.\n\n" +
                    "If the problem persists, contact your system administrator.",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            else
            {
                Log.Error(e.Exception, "[ERROR] Dispatcher unhandled exception");
                e.Handled = true;

                MessageBox.Show(
                    "An unexpected error occurred. Please try again.\n\n" +
                    "If the problem persists, please contact support.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            Log.CloseAndFlush();
        }
        catch (Exception logEx)
        {
            try
            {
                // Ensure fallback logging
                File.AppendAllText(fallbackLogPath,
                    $"[ERROR] [{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] Dispatcher unhandled exception: {e.Exception}\n" +
                    $"[LOGGING ERROR]: {logEx}\n");

                e.Handled = true;
                MessageBox.Show(
                    "A fatal error occurred. Please contact support.",
                    "Fatal Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch
            {
                // Last resort - prevent infinite recursion
                e.Handled = true;
            }
        }
    }

    /// <summary>
    /// Handles unobserved task exceptions (async operations that don't properly handle exceptions)
    /// </summary>
    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        var fallbackLogPath = GetFallbackLogPath();

        try
        {
            // Log all inner exceptions for comprehensive debugging
            foreach (var ex in e.Exception.InnerExceptions)
            {
                Log.Error(ex, "[ERROR] Unobserved task exception");
            }

            // Mark as observed to prevent application termination
            e.SetObserved();

            Log.Warning("Unobserved task exception was handled and marked as observed");
        }
        catch (Exception logEx)
        {
            try
            {
                File.AppendAllText(fallbackLogPath,
                    $"[ERROR] [{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] Unobserved task exception: {e.Exception}\n" +
                    $"[LOGGING ERROR]: {logEx}\n");

                e.SetObserved();
            }
            catch
            {
                // Last resort
                e.SetObserved();
            }
        }
    }

    /// <summary>
    /// Determines if an exception is related to database context issues
    /// </summary>
    private static bool IsDbContextException(Exception exception)
    {
        return exception is DbUpdateException ||
               exception is InvalidOperationException &&
               (exception.Message.Contains("DbContext") ||
                exception.Message.Contains("database") ||
                exception.Message.Contains("connection"));
    }

    /// <summary>
    /// Determines if an exception is critical and requires special handling
    /// </summary>
    private static bool IsCriticalException(Exception exception)
    {
        return exception is OutOfMemoryException ||
               exception is StackOverflowException ||
               exception is AccessViolationException ||
               exception is AppDomainUnloadedException ||
               exception is BadImageFormatException ||
               exception is CannotUnloadAppDomainException ||
               exception is InvalidProgramException;
    }

    /// <summary>
    /// Gets the fallback log path for emergency logging
    /// </summary>
    private static string GetFallbackLogPath()
    {
        try
        {
            string solutionRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName
                               ?? Directory.GetCurrentDirectory();
            string logsDirectory = Path.Combine(solutionRoot, "logs");
            Directory.CreateDirectory(logsDirectory);
            return Path.Combine(logsDirectory, "BusBuddy_emergency.log");
        }
        catch
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BusBuddy_emergency.log");
        }
    }

    #endregion
}


