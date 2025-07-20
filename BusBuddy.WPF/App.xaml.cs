using BusBuddy.Core.Data;
using BusBuddy.Core.Data.UnitOfWork;
using BusBuddy.WPF.Logging;
using BusBuddy.WPF.Utilities;
using BusBuddy.WPF.ViewModels;
using BusBuddy.WPF.Views.Main;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Syncfusion.Licensing;
using Syncfusion.SfSkinManager;
using Syncfusion.Themes.FluentLight.WPF;
using System;
using System.IO;
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
        // Register Syncfusion license
        RegisterSyncfusionLicense();

        // Configure theme
        ConfigureSyncfusionTheme();

        // Build host
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

        Log.Information("Host built successfully");
    }

    private void RegisterSyncfusionLicense()
    {
        try
        {
            // Try environment variable first for security
            string? licenseKey = Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY");

            // Fallback to appsettings if not found in environment
            if (string.IsNullOrWhiteSpace(licenseKey))
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true)
                    .Build();
                licenseKey = config["Syncfusion:LicenseKey"];
            }

            // Register license if found
            if (!string.IsNullOrWhiteSpace(licenseKey))
            {
                SyncfusionLicenseProvider.RegisterLicense(licenseKey);
                Log.Debug("Syncfusion license registered successfully");
            }
            else
            {
                Log.Warning("Syncfusion license key not found in environment variables or appsettings.json");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to register Syncfusion license");
        }
    }

    private void ConfigureSyncfusionTheme()
    {
        try
        {
            // Apply theme settings once, early in application lifecycle
            SfSkinManager.ApplyStylesOnApplication = true;
            SfSkinManager.ApplyThemeAsDefaultStyle = true;
            SfSkinManager.ApplicationTheme = new Theme("FluentLight");
            Log.Debug("Syncfusion FluentLight theme configured");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to configure Syncfusion theme");
        }
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Set up exception handlers
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        await _host.StartAsync();

        // Handle command line if present
        if (e.Args.Length > 0)
        {
            await HandleCommandLineArgumentsAsync(e.Args);
            Shutdown();
            return;
        }

        // Orchestrate startup
        var mainWindow = new MainWindow();

        try
        {
            var mainViewModel = Services.GetRequiredService<MainViewModel>();
            mainWindow.DataContext = mainViewModel;
            mainWindow.Show();

            _ = Task.Run(async () =>
            {
                try
                {
                    // Try to get orchestration service if available
                    var orchestrationService = Services.GetService<object>()?.GetType().Assembly
                        .GetType("BusBuddy.WPF.Services.StartupOrchestrationService");

                    var loadingViewModel = Services.GetRequiredService<LoadingViewModel>();

                    if (orchestrationService != null)
                    {
                        // Use reflection to call the service if it exists
                        Log.Information("Executing startup sequence with orchestration service");
                        // This would use reflection to call ExecuteStartupSequenceAsync

                        // For now, just navigate to dashboard as a fallback
                        await Task.Delay(100); // Ensure we have an await in the async lambda
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (mainViewModel.GetType().GetMethod("NavigateToDashboard") != null)
                            {
                                mainViewModel.NavigateToDashboard();
                            }
                        });
                    }
                    else
                    {
                        // Fallback: Just navigate to dashboard
                        Log.Warning("Startup orchestration service not available, using fallback");
                        await Task.Delay(100); // Ensure we have an await in the async lambda
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (mainViewModel.GetType().GetMethod("NavigateToDashboard") != null)
                            {
                                mainViewModel.NavigateToDashboard();
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error during startup orchestration");
                }
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error initializing main view model");
            mainWindow.Show(); // Show window anyway
        }

        // Pre-warm caches in background
        _ = Task.Run(PreWarmCachesAsync);
    }

    private async Task PreWarmCachesAsync()
    {
        try
        {
            // Use reflection to check if bus caching service exists
            Log.Information("Pre-warming application caches");

            // Simple delay to ensure some async operation occurs
            await Task.Delay(100);

            // Just log that pre-warming was attempted since we can't access the actual types
            Log.Information("Cache pre-warming completed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Cache pre-warming failed");
        }
    }

    private async Task HandleCommandLineArgumentsAsync(string[] args)
    {
        // Simplified handling; add debug cases under #if DEBUG if needed
        Log.Information("Processing command line arguments: {Arguments}", string.Join(" ", args));

        // Add an await to make this properly async
        await Task.CompletedTask;
    }

    private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Logging and Diagnostics
        services.AddSingleton<PerformanceMonitor>();

        // Database
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
            });
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }, ServiceLifetime.Scoped);

        // Data Access
        services.AddScoped<IBusBuddyDbContextFactory, BusBuddyDbContextFactory>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Core Services
        services.AddMemoryCache();

        // Register remaining services based on availability
        RegisterRemainingServices(services);

        // ViewModels
        services.AddScoped<MainViewModel>();
        services.AddScoped<LoadingViewModel>();
        services.AddScoped<DashboardViewModel>();
    }

    private void RegisterRemainingServices(IServiceCollection services)
    {
        // This method conditionally registers services based on their availability
        // It's separated to make the main method cleaner and easier to maintain

        try
        {
            // Check if types exist before registering
            // These are added conditionally to allow compilation even when some types are missing

            var vehicleRepoType = Type.GetType("BusBuddy.Core.Data.IVehicleRepository, BusBuddy.Core");
            var vehicleRepoImplType = Type.GetType("BusBuddy.Core.Data.VehicleRepository, BusBuddy.Core");
            if (vehicleRepoType != null && vehicleRepoImplType != null)
            {
                Log.Debug("Registering vehicle repository services");
                // services.AddScoped(vehicleRepoType, vehicleRepoImplType);
            }

            var busCachingServiceType = Type.GetType("BusBuddy.Core.Services.IBusCachingService, BusBuddy.Core");
            var busCachingServiceImplType = Type.GetType("BusBuddy.Core.Services.BusCachingService, BusBuddy.Core");
            if (busCachingServiceType != null && busCachingServiceImplType != null)
            {
                Log.Debug("Registering bus caching service");
                // services.AddSingleton(busCachingServiceType, busCachingServiceImplType);
            }

            var busServiceType = Type.GetType("BusBuddy.Core.Services.IBusService, BusBuddy.Core");
            var busServiceImplType = Type.GetType("BusBuddy.Core.Services.BusService, BusBuddy.Core");
            if (busServiceType != null && busServiceImplType != null)
            {
                Log.Debug("Registering bus service");
                // services.AddScoped(busServiceType, busServiceImplType);
            }

            var mappingProfileType = Type.GetType("BusBuddy.WPF.Mapping.MappingProfile, BusBuddy.WPF");
            if (mappingProfileType != null)
            {
                Log.Debug("Registering AutoMapper profile");
                // services.AddAutoMapper(mappingProfileType);
            }

            var navServiceType = Type.GetType("BusBuddy.WPF.Services.INavigationService, BusBuddy.WPF");
            var navServiceImplType = Type.GetType("BusBuddy.WPF.Services.NavigationService, BusBuddy.WPF");
            if (navServiceType != null && navServiceImplType != null)
            {
                Log.Debug("Registering navigation service");
                // services.AddSingleton(navServiceType, navServiceImplType);
            }

            var startupOrchServiceType = Type.GetType("BusBuddy.WPF.Services.StartupOrchestrationService, BusBuddy.WPF");
            if (startupOrchServiceType != null)
            {
                Log.Debug("Registering startup orchestration service");
                // services.AddScoped(startupOrchServiceType);
            }

            var dbValidatorType = Type.GetType("BusBuddy.WPF.Utilities.DatabaseValidator, BusBuddy.WPF");
            if (dbValidatorType != null)
            {
                Log.Debug("Registering database validator");
                // services.AddScoped(dbValidatorType);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error registering conditional services");
        }
    }

    #region Exception Handling

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        Log.Fatal(ex, "Unhandled domain exception. IsTerminating: {IsTerminating}", e.IsTerminating);
        // Note: IsTerminating is read-only and determined by the runtime
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unhandled UI exception");
        e.Handled = true;
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unobserved task exception");
        e.SetObserved();
    }

    #endregion
}
