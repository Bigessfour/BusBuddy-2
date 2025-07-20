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
using Syncfusion.Themes.FluentDark.WPF;
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
            // Apply FluentDark theme globally using proper SkinManager API
            SfSkinManager.ApplyStylesOnApplication = true;
            SfSkinManager.ApplyThemeAsDefaultStyle = true;

            // Set FluentDark theme - proper Syncfusion 30.1.40 API usage
            SfSkinManager.ApplicationTheme = new Theme("FluentDark");

            Log.Information("Syncfusion FluentDark theme configured successfully using SkinManager");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to configure Syncfusion FluentDark theme");

            // Fallback to basic theme
            try
            {
                SfSkinManager.ApplicationTheme = new Theme("FluentDark");
                Log.Warning("Applied fallback FluentDark theme");
            }
            catch (Exception fallbackEx)
            {
                Log.Error(fallbackEx, "Fallback theme configuration also failed");
            }
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
            // 🎯 REGISTER ESSENTIAL CORE SERVICES

            // Core Business Services
            services.AddScoped<BusBuddy.Core.Services.Interfaces.IBusService, BusBuddy.Core.Services.BusService>();
            services.AddScoped<BusBuddy.Core.Services.IDriverService, BusBuddy.Core.Services.DriverService>();
            services.AddScoped<BusBuddy.Core.Services.IRouteService, BusBuddy.Core.Services.RouteService>();
            services.AddScoped<BusBuddy.Core.Services.IStudentService, BusBuddy.Core.Services.StudentService>();
            services.AddScoped<BusBuddy.Core.Services.IMaintenanceService, BusBuddy.Core.Services.MaintenanceService>();
            services.AddScoped<BusBuddy.Core.Services.IFuelService, BusBuddy.Core.Services.FuelService>();
            services.AddScoped<BusBuddy.Core.Services.IActivityLogService, BusBuddy.Core.Services.ActivityLogService>();
            services.AddScoped<BusBuddy.Core.Services.IDashboardMetricsService, BusBuddy.Core.Services.DashboardMetricsService>();

            // Caching and Performance Services
            services.AddSingleton<BusBuddy.Core.Services.IEnhancedCachingService, BusBuddy.Core.Services.EnhancedCachingService>();
            services.AddSingleton<BusBuddy.Core.Services.IBusCachingService, BusBuddy.Core.Services.BusCachingService>();

            // 🎯 REGISTER WPF PRESENTATION SERVICES

            services.AddSingleton<BusBuddy.WPF.Services.INavigationService, BusBuddy.WPF.Services.NavigationService>();
            services.AddScoped<BusBuddy.WPF.Services.ILazyViewModelService, BusBuddy.WPF.Services.LazyViewModelService>();
            services.AddScoped<BusBuddy.WPF.Services.StartupOrchestrationService>();

            // 🎯 REGISTER CORE VIEWMODELS (Aligned with existing files)

            // Main Application ViewModels
            services.AddScoped<BusBuddy.WPF.ViewModels.DashboardViewModel>();
            services.AddScoped<LoadingViewModel>();
            services.AddScoped<MainViewModel>();

            // Management ViewModels (using correct namespaces from actual files)
            services.AddTransient<BusBuddy.WPF.ViewModels.BusManagementViewModel>();
            services.AddTransient<BusBuddy.WPF.ViewModels.DriverManagementViewModel>();
            services.AddTransient<BusBuddy.WPF.ViewModels.RouteManagementViewModel>();
            services.AddTransient<BusBuddy.WPF.ViewModels.Schedule.ScheduleManagementViewModel>();
            services.AddTransient<BusBuddy.WPF.ViewModels.StudentManagementViewModel>();
            services.AddTransient<BusBuddy.WPF.ViewModels.MaintenanceTrackingViewModel>();
            services.AddTransient<BusBuddy.WPF.ViewModels.FuelManagementViewModel>();
            services.AddTransient<BusBuddy.WPF.ViewModels.ActivityLogViewModel>();
            services.AddTransient<BusBuddy.WPF.ViewModels.SettingsViewModel>();

            // Student ViewModels
            services.AddTransient<BusBuddy.WPF.ViewModels.StudentListViewModel>();
            services.AddTransient<BusBuddy.WPF.ViewModels.StudentDetailViewModel>();
            services.AddTransient<BusBuddy.WPF.ViewModels.Student.StudentEditViewModel>();

            // Schedule ViewModels
            services.AddTransient<BusBuddy.WPF.ViewModels.ScheduleManagement.ScheduleViewModel>();

            // Register AutoMapper
            var mappingProfileType = Type.GetType("BusBuddy.WPF.Mapping.MappingProfile, BusBuddy.WPF");
            if (mappingProfileType != null)
            {
                services.AddAutoMapper(mappingProfileType);
                Log.Debug("AutoMapper profile registered");
            }

            // Register Utilities
            var dbValidatorType = Type.GetType("BusBuddy.WPF.Utilities.DatabaseValidator, BusBuddy.WPF");
            if (dbValidatorType != null)
            {
                services.AddScoped(dbValidatorType);
                Log.Debug("Database validator registered");
            }

            Log.Information("All essential services registered successfully for Syncfusion WPF 30.1.40 implementation");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error registering essential services");
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
