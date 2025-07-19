using BusBuddy.Core.Data;
using BusBuddy.Core.Data.UnitOfWork;
using BusBuddy.WPF.Logging;
using BusBuddy.WPF.Utilities;
using BusBuddy.WPF.ViewModels;
using BusBuddy.WPF.Views.Main;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Core.Enrichers;
using Syncfusion.SfSkinManager;
using Syncfusion.Themes.Windows11Light.WPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media;

namespace BusBuddy.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// OPTIMIZED STARTUP: Theme configuration occurs BEFORE InitializeComponent() to prevent resource corruption
/// </summary>
public partial class App : Application
{
    private IHost _host;
    private StartupPerformanceMonitor? _startupMonitor;
    private Stopwatch? _startupStopwatch;
    private static readonly ILogger Logger = Log.ForContext<App>();

    /// <summary>
    /// Gets the service provider for the application.
    /// </summary>
    public IServiceProvider Services { get; private set; } = default!;

    public App()
    {
        // CRITICAL: Register Syncfusion license FIRST
        RegisterSyncfusionLicense();

        // OPTIMIZATION 1: Configure theme BEFORE InitializeComponent() to prevent corruption
        ConfigureOptimizedSyncfusionTheme();

        // Initialize basic error handling
        InitializeGlobalExceptionHandling();

        // Build the generic host with Serilog configuration
        try
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .UseSerilog((context, services, configuration) =>
                {
                    configuration
                        .ReadFrom.Configuration(context.Configuration)
                        .Enrich.FromLogContext()
                        .Enrich.WithMachineName()
                        .Enrich.WithThreadId()
                        .Enrich.WithProcessId()
                        .Enrich.WithProcessName()
                        .Enrich.WithEnvironmentName()
                        .Enrich.WithEnvironmentUserName()
                        .WriteTo.Console()
                        .WriteTo.File("logs/application-.log",
                            rollingInterval: RollingInterval.Day,
                            shared: true,
                            flushToDiskInterval: TimeSpan.FromSeconds(1));
                })
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services, context.Configuration);
                })
                .Build();
            Services = _host.Services;

            Log.Information("Host built successfully with optimized theme configuration");
        }
        catch (Exception hostEx)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to build host: {hostEx.Message}");
            throw;
        }
    }

    #region Optimized Theme Configuration Methods

    private void RegisterSyncfusionLicense()
    {
        try
        {
            string? envLicenseKey = Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY");
            if (!string.IsNullOrWhiteSpace(envLicenseKey))
            {
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(envLicenseKey);
                System.Diagnostics.Debug.WriteLine("✅ Syncfusion license registered from environment variable");
                return;
            }

            var licenseConfig = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            string? licenseKey = licenseConfig["Syncfusion:LicenseKey"] ?? licenseConfig["SyncfusionLicenseKey"];

            if (!string.IsNullOrWhiteSpace(licenseKey))
            {
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(licenseKey);
                System.Diagnostics.Debug.WriteLine("✅ Syncfusion license registered from appsettings.json");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ WARNING: Syncfusion license key not found");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ ERROR: Failed to register Syncfusion license: {ex.Message}");
        }
    }

    private void ConfigureOptimizedSyncfusionTheme()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🎨 OPTIMIZED: Initializing theme system BEFORE InitializeComponent()");

            // CRITICAL: Set SfSkinManager properties FIRST to prevent resource corruption
            SfSkinManager.ApplyThemeAsDefaultStyle = true;
            SfSkinManager.ApplyStylesOnApplication = true;

            // Apply global theme early — using Windows11Light as preferred theme
            SfSkinManager.ApplicationTheme = new Theme("Windows11Light");

            System.Diagnostics.Debug.WriteLine("✅ OPTIMIZED: Theme configuration completed successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ OPTIMIZED: Theme initialization failed: {ex.Message}");

            // Emergency fallback — try alternative themes
            try
            {
                SfSkinManager.ApplyStylesOnApplication = true;
                SfSkinManager.ApplicationTheme = new Theme("MaterialDark");
                System.Diagnostics.Debug.WriteLine("✅ OPTIMIZED: Emergency MaterialDark theme applied");
            }
            catch (Exception fallbackEx)
            {
                System.Diagnostics.Debug.WriteLine($"❌ OPTIMIZED: Emergency fallback failed: {fallbackEx.Message}");
            }
        }
    }

    private void InitializeGlobalExceptionHandling()
    {
        try
        {
            this.DispatcherUnhandledException += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"Basic UI Exception Handler: {e.Exception.Message}");
                e.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var exception = (Exception)e.ExceptionObject;
                System.Diagnostics.Debug.WriteLine($"Basic Domain Exception Handler: {exception.Message}");
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error setting up basic exception handlers: {ex.Message}");
        }
    }

    #endregion

    protected override void OnStartup(StartupEventArgs e)
    {
        _startupStopwatch = Stopwatch.StartNew();

        Logger.Information("🚀 OPTIMIZED startup initiated — theme already configured in constructor");

        try
        {
            // Start application host
            _host.StartAsync().GetAwaiter().GetResult();

            // Call base.OnStartup after theme configuration
            base.OnStartup(e);

            // Validate that theme was applied correctly
            ValidateThemeConfiguration();

            // Initialize UI
            InitializeUI(e);

            _startupStopwatch.Stop();
            Logger.Information("✅ OPTIMIZED startup completed successfully in {ElapsedMs}ms", _startupStopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _startupStopwatch?.Stop();
            Logger.Fatal(ex, "❌ OPTIMIZED startup failed after {ElapsedMs}ms", _startupStopwatch?.ElapsedMilliseconds ?? 0);

            MessageBox.Show(
                $"A critical error occurred during application startup:\n\n{ex.Message}\n\n" +
                "The application will now close. Please check the logs for details.",
                "Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown();
        }
    }

    private void ValidateThemeConfiguration()
    {
        try
        {
            var currentTheme = SfSkinManager.ApplicationTheme?.ToString() ?? "Unknown";
            var applyStylesOnApp = SfSkinManager.ApplyStylesOnApplication;
            var applyThemeAsDefault = SfSkinManager.ApplyThemeAsDefaultStyle;

            Logger.Information("🎨 Theme validation — Applied: {AppliedTheme}, StylesOnApp: {StylesOnApp}, ThemeAsDefault: {ThemeAsDefault}",
                             currentTheme, applyStylesOnApp, applyThemeAsDefault);

            // Test for XAML parse errors by checking resource dictionaries
            var mergedDictCount = this.Resources?.MergedDictionaries?.Count ?? 0;
            Logger.Information("📦 Resource dictionaries loaded: {Count}", mergedDictCount);

            Logger.Information("✅ Theme validation completed successfully");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "❌ Theme validation failed");
        }
    }

    private void InitializeUI(StartupEventArgs e)
    {
        try
        {
            // Handle command line arguments if present
            if (e.Args.Length > 0)
            {
                Logger.Information("🔧 Processing {Count} command line arguments", e.Args.Length);
                return; // Exit early for command line mode
            }

            // Create and show main window
            var mainWindow = new MainWindow();
            var mainViewModel = Services.GetRequiredService<MainViewModel>();
            mainWindow.DataContext = mainViewModel;
            mainWindow.Show();

            // Navigate to dashboard
            mainViewModel.NavigateToDashboard();

            Logger.Information("✅ UI initialization completed");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "❌ UI initialization failed");
            throw;
        }
    }

    #region Service Configuration

    private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        ConfigureLoggingAndDiagnostics(services);
        ConfigureDatabase(services, configuration);
        ConfigureDataAccess(services);
        ConfigureCoreServices(services, configuration);
        ConfigureWpfServices(services);
        ConfigureUtilities(services);
        ConfigureViewModels(services);
    }

    private void ConfigureLoggingAndDiagnostics(IServiceCollection services)
    {
        services.AddSingleton<BusBuddyContextEnricher>();
        services.AddSingleton<DatabaseOperationEnricher>();
        services.AddSingleton<UIOperationEnricher>();
        services.AddSingleton<LogAggregationEnricher>();
        services.AddSingleton<StartupExceptionEnricher>();
        services.AddSingleton<CondensedLogFormatter>();
        services.AddSingleton<BusBuddy.WPF.Utilities.LogLifecycleManager>();
        services.AddSingleton<BusBuddy.WPF.Utilities.PerformanceMonitor>();
        services.AddSingleton<BusBuddy.WPF.Utilities.StartupPerformanceMonitor>();
    }

    private void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<BusBuddyDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(15);
                sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });

            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            options.EnableThreadSafetyChecks(false);
            options.ConfigureWarnings(warnings =>
                warnings.Log(CoreEventId.MultipleNavigationProperties));

            if (BusBuddy.Core.Utilities.EnvironmentHelper.IsSensitiveDataLoggingEnabled())
            {
                Log.Warning("SECURITY WARNING: Sensitive data logging is enabled. This should NEVER be used in production.");
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        }, ServiceLifetime.Scoped);
    }

    private void ConfigureDataAccess(IServiceCollection services)
    {
        services.AddScoped<IBusBuddyDbContextFactory, BusBuddyDbContextFactory>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<BusBuddy.Core.Data.Repositories.IVehicleRepository, BusBuddy.Core.Data.Repositories.VehicleRepository>();
    }

    private void ConfigureCoreServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddSingleton<BusBuddy.Core.Services.IBusCachingService, BusBuddy.Core.Services.BusCachingService>();
        services.AddSingleton<BusBuddy.Core.Services.IEnhancedCachingService, BusBuddy.Core.Services.EnhancedCachingService>();

        services.AddScoped<BusBuddy.Core.Services.Interfaces.IBusService, BusBuddy.Core.Services.BusService>();
        services.AddScoped<BusBuddy.Core.Services.IRouteService, BusBuddy.Core.Services.RouteService>();
        services.AddScoped<BusBuddy.Core.Services.IDriverService, BusBuddy.Core.Services.DriverService>();
        services.AddScoped<BusBuddy.Core.Services.IFuelService, BusBuddy.Core.Services.FuelService>();
        services.AddScoped<BusBuddy.Core.Services.IMaintenanceService, BusBuddy.Core.Services.MaintenanceService>();
        services.AddScoped<BusBuddy.Core.Services.IActivityLogService, BusBuddy.Core.Services.ActivityLogService>();
        services.AddScoped<BusBuddy.Core.Services.IStudentService, BusBuddy.Core.Services.StudentService>();
        services.AddScoped<BusBuddy.Core.Services.Interfaces.IScheduleService, BusBuddy.Core.Services.ScheduleService>();

        services.AddScoped<BusBuddy.Core.Services.IUserContextService, BusBuddy.Core.Services.UserContextService>();
        services.AddScoped<BusBuddy.Core.Services.IUserSettingsService, BusBuddy.Core.Services.UserSettingsService>();
        services.AddScoped<BusBuddy.Core.Services.IConfigurationService, BusBuddy.Core.Services.ConfigurationService>();

        services.AddScoped<BusBuddy.Core.Services.GoogleEarthEngineService>();
        services.AddScoped<BusBuddy.Core.Services.IDashboardMetricsService, BusBuddy.Core.Services.DashboardMetricsService>();

        services.AddHttpClient<BusBuddy.Core.Services.XAIService>();
        services.AddScoped<BusBuddy.Core.Services.XAIService>();

        services.Configure<BusBuddy.Configuration.XAIDocumentationSettings>(configuration.GetSection("XAI"));
    }

    private void ConfigureWpfServices(IServiceCollection services)
    {
        services.AddAutoMapper(typeof(BusBuddy.WPF.Mapping.MappingProfile));
        services.AddSingleton<BusBuddy.WPF.Services.IMappingService>(provider =>
        {
            var mapper = provider.GetRequiredService<IMapper>();
            var logger = Log.ForContext<BusBuddy.WPF.Services.MappingService>();
            return new BusBuddy.WPF.Services.MappingService(mapper, logger);
        });

        services.AddSingleton<BusBuddy.WPF.Services.OptimizedThemeService>();
        services.AddScoped<BusBuddy.WPF.Services.IDriverAvailabilityService, BusBuddy.WPF.Services.DriverAvailabilityService>();
        services.AddScoped<BusBuddy.WPF.Services.IRoutePopulationScaffold, BusBuddy.WPF.Services.RoutePopulationScaffold>();
        services.AddScoped<BusBuddy.WPF.Services.StartupOptimizationService>();
        services.AddSingleton<BusBuddy.WPF.Services.INavigationService, BusBuddy.WPF.Services.NavigationService>();
        services.AddScoped<BusBuddy.WPF.Services.IGoogleEarthService, BusBuddy.WPF.Services.GoogleEarthService>();
        services.AddScoped<BusBuddy.WPF.Services.StartupValidationService>();
        services.AddScoped<BusBuddy.WPF.Services.StartupOrchestrationService>();
    }

    private void ConfigureUtilities(IServiceCollection services)
    {
        services.AddSingleton<BusBuddy.WPF.Utilities.BackgroundTaskManager>();
        services.AddScoped<BusBuddy.Core.Utilities.DatabaseValidator>();
        services.AddTransient<MainWindow>();
    }

    private void ConfigureViewModels(IServiceCollection services)
    {
        services.AddSingleton<BusBuddy.WPF.Services.ILazyViewModelService, BusBuddy.WPF.Services.LazyViewModelService>();
        services.AddScoped<BusBuddy.WPF.Services.IStartupPreloadService, BusBuddy.WPF.Services.StartupPreloadService>();

        // Essential ViewModels
        services.AddScoped<BusBuddy.WPF.ViewModels.MainViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.DashboardViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.LoadingViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.ActivityLogViewModel>();

        // Management ViewModels
        services.AddScoped<BusBuddy.WPF.ViewModels.BusManagementViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.DriverManagementViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.RouteManagementViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.Schedule.ScheduleManagementViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.ScheduleManagement.ScheduleViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.Schedule.AddEditScheduleViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.StudentManagementViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.MaintenanceTrackingViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.FuelManagementViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.RoutePlanningViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.StudentListViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.GoogleEarthViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.SettingsViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.XaiChatViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.XAI.OptimizedXAIChatViewModel>();

        // Dashboard Tile ViewModels
        services.AddScoped<BusBuddy.WPF.ViewModels.FleetStatusTileViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.MaintenanceAlertsTileViewModel>();
        services.AddScoped<Func<Action<string>?, BusBuddy.WPF.ViewModels.QuickActionsTileViewModel>>(provider =>
            (navigationAction) => new BusBuddy.WPF.ViewModels.QuickActionsTileViewModel(navigationAction));
    }

    #endregion
}
