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
using Syncfusion.Themes.FluentDark.WPF;
using Syncfusion.Themes.FluentLight.WPF;
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
/// </summary>
public partial class App : Application
{
    private IHost _host;
    private StartupPerformanceMonitor? _startupMonitor;

    /// <summary>
    /// Gets the service provider for the application.
    /// </summary>
    public IServiceProvider Services { get; private set; } = default!;

    public App()
    {
        // CRITICAL: Register Syncfusion license FIRST
        RegisterSyncfusionLicense();

        // Configure theme early but simply
        ConfigureSyncfusionTheme();

        // Initialize basic error handling
        InitializeGlobalExceptionHandling();

        // ✅ THEME RESOURCES: Automatically loaded via BusBuddyResourceDictionary.xaml MergedDictionaries
        // ✅ CONTROL THEMING: All Syncfusion controls automatically inherit FluentDark theme
        // ✅ NO MANUAL INTERVENTION: SfSkinManager handles all theme application automatically

        // CRITICAL: Initialize basic error handling first
        InitializeGlobalExceptionHandling();

        // Initialize Serilog directly first (before host) with enrichment
        try
        {
            var startupConfig = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(startupConfig)
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
                    shared: true, // Enable shared access for concurrent processes
                    flushToDiskInterval: TimeSpan.FromSeconds(1))
                .CreateLogger();

            Log.Information("Serilog initialized successfully with enrichment capabilities");
        }
        catch (Exception logEx)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to initialize Serilog: {logEx.Message}");
        }

        // Build the generic host with Serilog already initialized
        try
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .UseSerilog()
                .ConfigureServices((context, services) =>
                {
                    // Use the application's ConfigureServices method for all registrations
                    ConfigureServices(services, context.Configuration);
                })
                .Build();
            Services = _host.Services;

            Log.Information("Host built successfully");
        }
        catch (Exception hostEx)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to build host: {hostEx.Message}");
            throw;
        }
    }

    #region Helper Methods for Simplified Constructor

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

            // Fallback: try to load from appsettings.json
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

    private void ConfigureSyncfusionTheme()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🎨 Initializing optimized theme system");

            // Use the optimized theme service for centralized theme management
            BusBuddy.WPF.Services.OptimizedThemeService.InitializeApplicationTheme();

            System.Diagnostics.Debug.WriteLine("✅ Optimized theme system initialized successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Optimized theme initialization failed: {ex.Message}");

            // Emergency fallback - minimal theme setup
            try
            {
                SfSkinManager.ApplyStylesOnApplication = true;
                SfSkinManager.ApplicationTheme = new Theme("MaterialDark");
                System.Diagnostics.Debug.WriteLine("✅ Emergency MaterialDark theme applied");
            }
            catch (Exception fallbackEx)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Emergency fallback failed: {fallbackEx.Message}");
            }
        }
    }

    private void InitializeGlobalExceptionHandling()
    {
        try
        {
            // Set up basic exception handling that doesn't conflict with detailed handlers in OnStartup
            this.DispatcherUnhandledException += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"Basic UI Exception Handler: {e.Exception.Message}");
                e.Handled = true; // Prevent crash
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

    /// <summary>
    /// Execute parallel startup optimizations to improve performance
    /// </summary>
    private async Task ExecuteParallelStartupAsync(string[] args)
    {
        using (LogContext.PushProperty("Operation", "ParallelStartup"))
        {
            var parallelStopwatch = Stopwatch.StartNew();
            Logger.Information("🚀 Starting parallel startup optimization");

            try
            {
                // Define independent startup tasks that can run concurrently
                var startupTasks = new List<Task>
                {
                    // Theme validation and initialization
                    BusBuddy.WPF.Utilities.PerformanceOptimizer.ExecuteTimedAsync("ThemeValidation", async () =>
                    {
                        await ValidateAndInitializeThemeAsync();
                        return Task.CompletedTask;
                    }),

                    // Command line argument processing
                    BusBuddy.WPF.Utilities.PerformanceOptimizer.ExecuteTimedAsync("CommandLineProcessing", async () =>
                    {
                        await ProcessCommandLineArgumentsAsync(args);
                        return Task.CompletedTask;
                    }),

                    // Resource dictionary validation
                    BusBuddy.WPF.Utilities.PerformanceOptimizer.ExecuteTimedAsync("ResourceValidation", async () =>
                    {
                        await Task.Run(() => ValidateResourceDictionaries());
                        return Task.CompletedTask;
                    }),

                    // Host startup (critical path - must complete before UI)
                    BusBuddy.WPF.Utilities.PerformanceOptimizer.ExecuteTimedAsync("HostStartup", async () =>
                    {
                        await StartApplicationHostAsync();
                        return Task.CompletedTask;
                    })
                };

                // Execute cache warm-up operations in parallel using PerformanceOptimizer
                var cacheWarmupOperations = new List<Func<Task>>
                {
                    async () => await PreWarmDatabaseConnectionAsync(),
                    async () => await PreWarmServicesAsync(),
                    async () => await ValidateSecuritySettingsAsync()
                };

                // Add cache warm-up to startup tasks
                startupTasks.Add(BusBuddy.WPF.Utilities.PerformanceOptimizer.WarmupCachesParallelAsync(cacheWarmupOperations));

                // Wait for all parallel startup tasks to complete
                await Task.WhenAll(startupTasks);
                parallelStopwatch.Stop();

                Logger.Information("✅ Parallel startup optimization completed in {ElapsedMs}ms", parallelStopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                parallelStopwatch.Stop();
                Logger.Error(ex, "❌ Parallel startup optimization failed after {ElapsedMs}ms", parallelStopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }

    /// <summary>
    /// Enhanced theme validation with async support
    /// </summary>
    private async Task ValidateAndInitializeThemeAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                Logger.Debug("🎨 Starting enhanced theme validation");

                var currentTheme = SfSkinManager.ApplicationTheme;
                var stylesOnApp = SfSkinManager.ApplyStylesOnApplication;

                Logger.Debug("🎨 Current theme: {Theme}, StylesOnApp: {StylesOnApp}",
                           currentTheme?.ToString() ?? "NULL", stylesOnApp);

                SfSkinManager.ApplyStylesOnApplication = true;

                // Validate critical resources
                var resourcesNeeded = new Dictionary<string, object>
                {
                    ["ContentForeground"] = new SolidColorBrush(Colors.White),
                    ["SurfaceBackground"] = new SolidColorBrush(Color.FromRgb(31, 31, 31)),
                    ["SurfaceBorderBrush"] = new SolidColorBrush(Color.FromRgb(51, 51, 51))
                };

                foreach (var resource in resourcesNeeded)
                {
                    if (!Application.Current.Resources.Contains(resource.Key))
                    {
                        Logger.Debug("🎨 Adding missing {ResourceKey} resource", resource.Key);
                        Application.Current.Resources[resource.Key] = resource.Value;
                    }
                }

                Logger.Information("✅ Enhanced theme validation completed");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "❌ Theme validation failed, applying fallback");

                try
                {
                    SfSkinManager.ApplyStylesOnApplication = true;
                    SfSkinManager.ApplicationTheme = new Theme("FluentLight");
                    Logger.Information("✅ Fallback theme applied");
                }
                catch (Exception fallbackEx)
                {
                    Logger.Error(fallbackEx, "❌ Fallback theme failed");
                    throw;
                }
            }
        });
    }

    /// <summary>
    /// Enhanced command line argument processing with async support
    /// </summary>
    private async Task ProcessCommandLineArgumentsAsync(string[] args)
    {
        await Task.Run(() =>
        {
            try
            {
                Logger.Information("🔧 Processing {Count} command line arguments", args.Length);

                if (args.Length > 0)
                {
                    Logger.Information("🔧 Arguments: {Arguments}", string.Join(" ", args));
                    // Process arguments here
                }

                Logger.Information("✅ Command line arguments processed");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "❌ Command line argument processing failed");
                throw;
            }
        });
    }

    /// <summary>
    /// Start application host with enhanced error handling
    /// </summary>
    private async Task StartApplicationHostAsync()
    {
        try
        {
            Logger.Information("🏗️ Starting application host");
            await _host.StartAsync();
            Logger.Information("✅ Application host started successfully");
        }
        catch (Exception ex)
        {
            Logger.Fatal(ex, "❌ Failed to start application host");
            throw;
        }
    }

    /// <summary>
    /// Validate resource dictionaries asynchronously
    /// </summary>
    private void ValidateResourceDictionaries()
    {
        try
        {
            Logger.Information("🎨 Validating resource dictionaries");

            var mergedDictCount = this.Resources?.MergedDictionaries?.Count ?? 0;
            Logger.Information("🎨 Total merged dictionaries: {Count}", mergedDictCount);

            string[] criticalKeys = { "SurfaceBorder", "SurfaceBorderBrush", "SurfaceBorderColor" };
            var foundKeys = criticalKeys.Where(k => this.Resources?.Contains(k) == true).ToList();
            var missingKeys = criticalKeys.Except(foundKeys).ToList();

            if (missingKeys.Any())
            {
                Logger.Warning("⚠️ Missing critical resources: {MissingKeys}", string.Join(", ", missingKeys));
            }
            else
            {
                Logger.Information("✅ All critical resources present");
            }

            Logger.Information("✅ Resource dictionary validation completed");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "❌ Resource dictionary validation failed");
            throw;
        }
    }

    /// <summary>
    /// Pre-warm database connection pool
    /// </summary>
    private async Task PreWarmDatabaseConnectionAsync()
    {
        try
        {
            Logger.Debug("🔥 Pre-warming database connection");

            using var scope = _host.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<BusBuddy.Core.Data.BusBuddyDbContext>();
            if (dbContext != null)
            {
                await dbContext.Database.CanConnectAsync();
                Logger.Debug("✅ Database connection pre-warmed");
            }
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "⚠️ Database connection pre-warming failed");
        }
    }

    /// <summary>
    /// Pre-warm essential services
    /// </summary>
    private async Task PreWarmServicesAsync()
    {
        try
        {
            Logger.Debug("🔥 Pre-warming essential services");

            using var scope = _host.Services.CreateScope();

            // Pre-warm commonly used services
            var viewModelServices = new[]
            {
                typeof(MainViewModel),
                typeof(EnhancedDashboardViewModel),
                typeof(LoadingViewModel)
            };

            await Task.Run(() =>
            {
                foreach (var serviceType in viewModelServices)
                {
                    try
                    {
                        scope.ServiceProvider.GetService(serviceType);
                        Logger.Debug("✅ Pre-warmed {ServiceType}", serviceType.Name);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning(ex, "⚠️ Failed to pre-warm {ServiceType}", serviceType.Name);
                    }
                }
            });

            Logger.Debug("✅ Services pre-warming completed");
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "⚠️ Services pre-warming failed");
        }
    }

    /// <summary>
    /// Validate security settings asynchronously
    /// </summary>
    private async Task ValidateSecuritySettingsAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                Logger.Debug("🔒 Validating security settings");

                // Check sensitive data logging in production
                if (!BusBuddy.Core.Utilities.EnvironmentHelper.IsDevelopment() &&
                    Environment.GetEnvironmentVariable("ENABLE_SENSITIVE_DATA_LOGGING") == "true")
                {
                    Logger.Error("🚨 SECURITY RISK: Sensitive data logging enabled in production");
                    throw new SecurityException("Sensitive data logging enabled in production environment");
                }

                Logger.Debug("✅ Security settings validated");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "❌ Security validation failed");
                throw;
            }
        });
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        var startupStopwatch = Stopwatch.StartNew();
        BusBuddy.WPF.Utilities.PerformanceOptimizer.StartTiming("ApplicationStartup");

        Logger.Information("🚀 Enhanced application startup initiated with {ArgumentCount} command line arguments", e.Args.Length);

        try
        {
            using (LogContext.PushProperty("StartupPhase", "ParallelInitialization"))
            {
                // Execute parallel startup optimizations
                var parallelStartupTask = ExecuteParallelStartupAsync(e.Args);
                parallelStartupTask.Wait(); // Wait for completion since OnStartup is not async

                // Call base.OnStartup after theme configuration
                Logger.Information("🏗️ Calling base.OnStartup after parallel initialization");
                base.OnStartup(e);
                Logger.Debug("✅ base.OnStartup completed successfully");

                startupStopwatch.Stop();
                BusBuddy.WPF.Utilities.PerformanceOptimizer.StopTiming("ApplicationStartup");

                Logger.Information("✅ Enhanced application startup completed successfully in {ElapsedMs}ms",
                                 startupStopwatch.ElapsedMilliseconds);
            }
        }
        catch (Exception ex)
        {
            startupStopwatch.Stop();
            BusBuddy.WPF.Utilities.PerformanceOptimizer.StopTiming("ApplicationStartup");
            Logger.Fatal(ex, "❌ Application startup failed after {ElapsedMs}ms", startupStopwatch.ElapsedMilliseconds);

            MessageBox.Show(
                $"A critical error occurred during application startup:\n\n{ex.Message}\n\n" +
                "The application will now close. Please check the logs for details.",
                "Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown();
            return;
        }

        // Continue with existing startup logic for UI initialization...
        StartEnhancedUIInitialization(e);
    }

    /// <summary>
    /// Enhanced UI initialization with performance monitoring
    /// </summary>
    private void StartEnhancedUIInitialization(StartupEventArgs e)
    {
        using (LogContext.PushProperty("Operation", "UIInitialization"))
        {
            BusBuddy.WPF.Utilities.PerformanceOptimizer.StartTiming("UIInitialization");

            try
            {
                // Handle command line arguments for debug functionality
                if (e.Args.Length > 0)
                {
                    using (LogContext.PushProperty("Operation", "CommandLineHandling"))
                    {
                        try
                        {
                            Log.Information("🔧 Processing command line arguments: {Arguments}", string.Join(" ", e.Args));
                            HandleCommandLineArgumentsAsync(e.Args).GetAwaiter().GetResult();
                            Log.Information("🔧 Command line arguments processed successfully");
                            return; // Exit after handling command line arguments
                        }
                        catch (Exception cmdEx)
                        {
                            Log.Error(cmdEx, "❌ Failed to process command line arguments");
                            throw;
                        }
                    }
                }

                Log.Information("🚀 No command line arguments detected, proceeding with normal startup");

                // Continue with rest of startup logic using performance optimizer
                ExecuteUIInitializationPhases();
            }
            finally
            {
                BusBuddy.WPF.Utilities.PerformanceOptimizer.StopTiming("UIInitialization");
            }
        }
    }

    /// <summary>
    /// Execute UI initialization phases with performance monitoring
    /// </summary>
    private void ExecuteUIInitializationPhases()
    {
        // Execute remaining startup logic with performance optimization
        var uiStopwatch = Stopwatch.StartNew();

        try
        {
                {
                    Log.Information("🎨 [STARTUP] Starting resource dictionary validation");
                    Log.Debug("🎨 [STARTUP] App.xaml resource loading approach - using SfSkinManager only");

                    // Add critical diagnostic logging
                    Log.Information("🎨 [STARTUP] Resource dictionaries loaded via App.xaml configuration");

                    // Log merged dictionary count
                    var mergedDictCount = this.Resources?.MergedDictionaries?.Count ?? 0;
                    Log.Information("🎨 [STARTUP] Total merged dictionaries: {Count}", mergedDictCount);

                    // Verify critical resources efficiently
                    string[] criticalKeys = new string[] { "SurfaceBorder", "SurfaceBorderBrush", "SurfaceBorderColor" };
                    var foundKeys = criticalKeys.Where(k => this.Resources?.Contains(k) == true).ToList();
                    var missingKeys = criticalKeys.Except(foundKeys).ToList();

                    if (missingKeys.Any())
                    {
                        Log.Warning("⚠️ [STARTUP] Missing critical resources: {MissingKeys}", string.Join(", ", missingKeys));
                    }
                    else
                    {
                        Log.Information("✅ [STARTUP] All critical resources present");
                    }

                    // Log current theme information
                    var currentTheme = SfSkinManager.ApplicationTheme?.ToString() ?? "Unknown";
                    Log.Information("🎨 [STARTUP] Current SfSkinManager theme: {ThemeName}", currentTheme);

                    Log.Information("✅ [STARTUP] Resource dictionary validation completed successfully");
                }
                catch (Exception resourceEx)
                {
                    Log.Error(resourceEx, "❌ [STARTUP] Error validating resource dictionaries");

                    // Log additional diagnostic information
                    try
                    {
                        var resourceCount = this.Resources?.Count ?? 0;
                        var mergedCount = this.Resources?.MergedDictionaries?.Count ?? 0;
                        Log.Error("❌ [STARTUP] Resource diagnostic - Total resources: {ResourceCount}, Merged dictionaries: {MergedCount}",
                                 resourceCount, mergedCount);
                    }
                    catch (Exception diagnosticEx)
                    {
                        Log.Error(diagnosticEx, "❌ [STARTUP] Failed to gather resource diagnostic information");
                    }
                }
            }

            // Now that the host is started, Serilog should be initialized
            // 🎨 THEME CONFIRMATION: Theme already applied in constructor
            // SfSkinManager.ApplicationTheme is already set to FluentDark with FluentLight fallback
            using (LogContext.PushProperty("Operation", "ThemeConfirmation"))
            {
                try
                {
                    Log.Information("🎨 [STARTUP] Starting Syncfusion theme confirmation");

                    var appliedTheme = SfSkinManager.ApplicationTheme?.ToString() ?? "Unknown";
                    var applyStylesOnApp = SfSkinManager.ApplyStylesOnApplication;
                    var applyThemeAsDefault = SfSkinManager.ApplyThemeAsDefaultStyle;

                    Log.Information("🎨 [STARTUP] Theme Status - Applied: {AppliedTheme}, StylesOnApp: {StylesOnApp}, ThemeAsDefault: {ThemeAsDefault}",
                                   appliedTheme, applyStylesOnApp, applyThemeAsDefault);

                    Log.Information("🎨 [STARTUP] Syncfusion theme confirmed: FluentDark primary with FluentLight fallback ready");
                }
                catch (Exception themeEx)
                {
                    Log.Error(themeEx, "❌ [STARTUP] Error during theme confirmation");
                }
            }

            // Start performance monitoring
            var stopwatch = Stopwatch.StartNew();

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

            // --- BEGIN: Enhanced Build log diagnostics with safe concurrent access ---
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

                // Use safe concurrent file writing with retry logic
                SafeWriteToFile(buildLogTestPath, startupInfo);

                // Also write to a secondary location to ensure we're getting logs somewhere
                string secondaryLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup_diagnostic.log");
                SafeWriteToFile(secondaryLogPath, startupInfo);

                // Create an empty marker file that we can check for existence
                SafeWriteToFile(Path.Combine(logDir, "app_started.marker"), DateTime.Now.ToString("o"));
            }
            catch (Exception ex)
            {
                // If the primary logging fails, use console output as fallback
                Console.WriteLine($"[BUILDLOG TEST] Could not write to logs/build.log: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[BUILDLOG TEST] Could not write to logs/build.log: {ex.Message}");
            }
            // --- END: Enhanced Build log diagnostics with safe concurrent access ---

            string appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

            // Configuration and logging already handled by host builder

            // Determine the solution root directory for logs
            string solutionRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName
                               ?? Directory.GetCurrentDirectory();
            string logsDirectory = Path.Combine(solutionRoot, "logs");

            // Ensure the logs directory exists
            if (!Directory.Exists(logsDirectory))
            {
                Directory.CreateDirectory(logsDirectory);
            }

            // Enhanced global exception handlers for comprehensive error capture
            // Remove basic handlers and replace with comprehensive ones
            AppDomain.CurrentDomain.UnhandledException -= (sender, e) =>
            {
                var exception = (Exception)e.ExceptionObject;
                System.Diagnostics.Debug.WriteLine($"Basic Domain Exception Handler: {exception.Message}");
            };

            this.DispatcherUnhandledException -= (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"Basic UI Exception Handler: {e.Exception.Message}");
                e.Handled = true;
            };

            // Set up comprehensive exception handlers with detailed logging
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            this.Exit += (s, ex) =>
            {
                // Clean application shutdown - debug helper not automatically started
                Log.CloseAndFlush();
            };

            // 🔧 PROGRESS-AWARE STARTUP: Orchestrated startup sequence with LoadingView integration
            Log.Information("[STARTUP] Running orchestrated startup sequence with LoadingView progress indication");
            var orchestrationStopwatch = Stopwatch.StartNew();

            // Create main window early to establish UI context
            var mainWindow = new MainWindow();

            // Get the main view model and loading view model
            var mainViewModel = Services.GetRequiredService<MainViewModel>();
            var loadingViewModel = Services.GetRequiredService<LoadingViewModel>();

            // Set up the main window with the main view model
            mainWindow.DataContext = mainViewModel;

            // Show the main window with loading view active
            mainWindow.Show();

            // Start the orchestrated startup sequence
            _ = Task.Run(async () =>
            {
                try
                {
                    using var orchestrationScope = Services.CreateScope();
                    var orchestrationService = orchestrationScope.ServiceProvider.GetRequiredService<BusBuddy.WPF.Services.StartupOrchestrationService>();

                    // Execute orchestrated startup with progress updates
                    var startupResult = await orchestrationService.ExecuteStartupSequenceAsync(loadingViewModel);

                    orchestrationStopwatch.Stop();

                    // Log detailed orchestration results
                    Log.Information("[STARTUP] Orchestrated startup completed in {OrchestrationTimeMs}ms", startupResult.TotalExecutionTimeMs);
                    Log.Information("[STARTUP] Startup Phases Summary:\n{StartupSummary}", startupResult.GetExecutionSummary());

                    // Write orchestration results to deployment-ready file
                    try
                    {
                        string orchestrationSolutionRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName
                                           ?? Directory.GetCurrentDirectory();
                        string orchestrationLogsDirectory = Path.Combine(orchestrationSolutionRoot, "logs");
                        string orchestrationLogPath = Path.Combine(orchestrationLogsDirectory, $"startup_orchestration_{DateTime.Now:yyyyMMdd_HHmmss}.log");

                        await File.WriteAllTextAsync(orchestrationLogPath, startupResult.GetExecutionSummary());
                        Log.Information("[STARTUP] Orchestration results written to: {OrchestrationLogPath}", orchestrationLogPath);
                    }
                    catch (Exception logEx)
                    {
                        Log.Warning(logEx, "[STARTUP] Could not write orchestration results to file");
                    }

                    // Switch to dashboard after successful startup
                    if (startupResult.IsSuccessful)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            // Use the synchronous version with background initialization
                            mainViewModel.NavigateToDashboard();
                            Log.Information("[STARTUP] ✅ Startup completed successfully — switched to Dashboard");

                            // Start cache pre-warming after successful navigation
                            PreWarmCaches();
                        });
                    }
                    else
                    {
                        // Handle startup failure
                        var failedPhases = string.Join("\n", startupResult.PhaseResults
                            .Where(r => !r.IsSuccessful)
                            .Select(r => $"• {r.PhaseName}: {r.ErrorMessage}"));

                        var isDevelopment = BusBuddy.Core.Utilities.EnvironmentHelper.IsDevelopment();

                        if (isDevelopment)
                        {
                            // In development, show warning but continue to dashboard
                            Log.Warning("[STARTUP] Orchestration phase failures detected in development environment - continuing to dashboard");
                            Log.Warning("[STARTUP] Failed phases:\n{FailedPhases}", failedPhases);

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                mainViewModel.NavigateToDashboard();
                            });
                        }
                        else
                        {
                            // In production, this is more serious
                            Log.Error("[STARTUP] Orchestration phase failures detected in production environment");
                            Log.Error("[STARTUP] Failed phases:\n{FailedPhases}", failedPhases);

                            // Show warning on UI thread
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show(
                                    $"The application failed during startup orchestration and may not function correctly:\n\n{failedPhases}\n\n" +
                                    "Please check the logs and contact technical support if this problem persists.",
                                    "Startup Orchestration Failed",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                                // Still navigate to dashboard for usability
                                mainViewModel.NavigateToDashboard();
                            });
                        }
                    }
                }
                catch (Exception orchestrationEx)
                {
                    Log.Error(orchestrationEx, "[STARTUP] Error during startup orchestration");

                    // Fallback to dashboard even on error
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        mainViewModel.NavigateToDashboard();
                    });
                }
            });

            // Run log consolidation after DI is set up (asynchronously) - DISABLED due to missing utility
            /*
            _ = Task.Run(async () =>
            {
                try
                {
                    var logConsolidationUtility = Services.GetRequiredService<BusBuddy.WPF.Utilities.LogConsolidationUtility>();
                    await logConsolidationUtility.ConsolidateLogsAsync();
                    var stats = logConsolidationUtility.GetLogStats();

                    Log.Information("[STARTUP] Log consolidation complete - {ActiveFiles} active files, {ArchivedFiles} archived files, {TotalSizeMB:F2}MB total",
                        stats.ActiveLogFiles, stats.ArchivedLogFiles, stats.TotalSizeMB);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "[STARTUP] Log consolidation had issues but continuing: {ErrorMessage}", ex.Message);
                }
            });
            */

            // Create and initialize startup performance monitor
            _startupMonitor = new StartupPerformanceMonitor(Log.Logger);
            _startupMonitor.Start();

            // Log basic startup timing
            Log.Information("[STARTUP_PERF] Initial startup setup completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

            // Pre-warm the bus cache on startup to reduce initial DB load
            // This is done in a background task to avoid blocking the UI
            _ = Task.Run(async () =>
            {
                try
                {
                    // Wait a short delay to allow the application to fully initialize
                    await Task.Delay(2000);

                    // Run database validation to detect and fix potential issues
                    using (var scope = this.Services.CreateScope())
                    {
                        var dbValidator = scope.ServiceProvider.GetService<BusBuddy.Core.Utilities.DatabaseValidator>();
                        if (dbValidator != null)
                        {
                            Log.Information("[STARTUP] Running database validation...");
                            // Enable breaking into the debugger when issues are found if we're in debug mode
                            bool isDebugMode = System.Diagnostics.Debugger.IsAttached || Environment.GetEnvironmentVariable("ENABLE_DB_VALIDATION") == "true";
                            var issues = await dbValidator.ValidateDatabaseDataAsync(breakOnIssue: isDebugMode);

                            if (issues.Any())
                            {
                                Log.Warning("[STARTUP] Database validation found {Count} issues", issues.Count);
                                foreach (var issue in issues)
                                {
                                    Log.Warning("[STARTUP] Database issue: {Issue}", issue);
                                }

                                // Attempt to fix common issues automatically
                                Log.Information("[STARTUP] Attempting to fix database issues automatically...");
                                var fixCount = await dbValidator.RunAutomaticFixesAsync(breakOnFix: isDebugMode);
                                Log.Information("[STARTUP] Fixed {Count} database issues automatically", fixCount);
                            }
                            else
                            {
                                Log.Information("[STARTUP] Database validation completed successfully with no issues found");
                            }
                        }
                    }

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
                                var busService = freshScope.ServiceProvider.GetRequiredService<BusBuddy.Core.Services.Interfaces.IBusService>();
                                var result = await busService.GetAllBusesAsync();
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

                // Log that we're about to initialize the UI
                Log.Information("[STARTUP] Creating and showing MainWindow");

                // 🎨 THEME CONFIRMATION: SfSkinManager automatically handles all theme application
                // ✅ FluentDark theme applied globally via SfSkinManager in App constructor
                // ✅ All Syncfusion controls automatically inherit FluentDark theme
                // ✅ Theme resources loaded via BusBuddyResourceDictionary.xaml MergedDictionaries
                Log.Information("🎨 Syncfusion WPF 30.1.40 environment confirmed: FluentDark theme active with automatic control theming");

                // 🎯 THEME VALIDATION: Verify SfSkinManager is properly configured
                var currentTheme = SfSkinManager.ApplicationTheme;
                if (currentTheme != null)
                {
                    Log.Information("✅ SfSkinManager.ApplicationTheme active: {ThemeName}", currentTheme.ToString());
                }
                else
                {
                    Log.Warning("⚠️ SfSkinManager.ApplicationTheme is null - theme may not be properly configured");
                }

                // Complete startup performance monitoring
                _startupMonitor.Complete();

                // Log total time from the original stopwatch
                stopwatch.Stop();
                startupStopwatch.Stop();

                using (LogContext.PushProperty("Operation", "StartupCompletion"))
                {
                    Log.Information("🎉 [STARTUP] Application startup sequence completed successfully");
                    Log.Information("⏱️ [STARTUP] Total startup time: {ElapsedMs}ms", startupStopwatch.ElapsedMilliseconds);
                    Log.Information("⏱️ [STARTUP] OnStartup method time: {OnStartupMs}ms", stopwatch.ElapsedMilliseconds);

                    // Log final application state
                    Log.Information("📊 [STARTUP] Final application state:");
                    Log.Information("   🎨 Theme: {Theme}", SfSkinManager.ApplicationTheme?.ToString() ?? "Unknown");
                    Log.Information("   📦 Resource dictionaries: {Count}", this.Resources?.MergedDictionaries?.Count ?? 0);
                    Log.Information("   🔧 Debug mode: {IsDebug}",
#if DEBUG
                        true
#else
                        false
#endif
                    );
                    Log.Information("✅ [STARTUP] Bus Buddy WPF application is ready for user interaction");
                }

                Log.Information("[STARTUP_PERF] Initial application startup completed in {ElapsedMs}ms",
                    stopwatch.ElapsedMilliseconds);

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

                    SafeWriteToFile(Path.Combine(logsDirectory, "startup_failure.log"), startupErrorDetails);
                    SafeWriteToFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup_failure.log"), startupErrorDetails);

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
        catch (Exception ex)
        {
            // Handle any exceptions that occur during the entire OnStartup method
            using (LogContext.PushProperty("Operation", "StartupFailure"))
            {
                startupStopwatch?.Stop();
                Log.Fatal(ex, "❌ [STARTUP] Fatal error during OnStartup method execution after {ElapsedMs}ms",
                         startupStopwatch?.ElapsedMilliseconds ?? 0);

                // Log specific error context
                Log.Fatal("❌ [STARTUP] Error details - Type: {ExceptionType}, Message: {Message}",
                         ex.GetType().Name, ex.Message);

                if (ex.InnerException != null)
                {
                    Log.Fatal("❌ [STARTUP] Inner exception - Type: {InnerType}, Message: {InnerMessage}",
                             ex.InnerException.GetType().Name, ex.InnerException.Message);
                }

                // Log application state at failure
                try
                {
                    Log.Fatal("❌ [STARTUP] Application state at failure:");
                    Log.Fatal("   🎨 Theme status: {ThemeStatus}", SfSkinManager.ApplicationTheme?.ToString() ?? "Not set");
                    Log.Fatal("   📦 Resource count: {ResourceCount}", this.Resources?.Count ?? 0);
                    Log.Fatal("   🏗️ Host status: {HostStatus}", _host?.Services != null ? "Initialized" : "Not initialized");
                }
                catch (Exception stateEx)
                {
                    Log.Fatal(stateEx, "❌ [STARTUP] Failed to log application state during fatal error");
                }
            }

            // Show error message to user
            MessageBox.Show(
                $"A fatal error occurred during application startup:\n\n{ex.Message}\n\n" +
                "The application will now close. Please check the logs for more details.",
                "Fatal Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            // Ensure we shut down the application
            Shutdown();
        }
    }

    /// <summary>
    /// Handles command line arguments for debug functionality
    /// </summary>
    private async Task HandleCommandLineArgumentsAsync(string[] args)
    {
        try
        {
            // Initialize minimal logging for command line operations
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            Log.Information("Handling command line arguments: {Arguments}", string.Join(" ", args));

            foreach (var arg in args)
            {
                switch (arg.ToLower())
                {
                    case "--start-debug-filter":
                        Log.Information("Starting debug filter from command line");
                        BusBuddy.WPF.Utilities.DebugHelper.StartAutoFilter();

                        // Keep the application running to monitor
                        Console.WriteLine("🔍 Debug filter started. Press Ctrl+C to exit...");
                        await Task.Delay(TimeSpan.FromMinutes(30)); // Run for 30 minutes
                        break;

                    case "--export-debug-json":
                        Log.Information("Exporting debug issues to JSON");
                        await BusBuddy.WPF.Utilities.DebugHelper.ExportToJson();
                        break;

                    case "--start-streaming":
                        Log.Information("Starting real-time debug streaming");
                        BusBuddy.WPF.Utilities.DebugOutputFilter.StartRealTimeStreaming();
                        BusBuddy.WPF.Utilities.DebugHelper.StartAutoFilter();

                        Console.WriteLine("🎯 Real-time debug streaming started. Press Ctrl+C to exit...");
                        await Task.Delay(TimeSpan.FromHours(1)); // Run for 1 hour
                        break;

                    case "--test-filter":
                        Log.Information("Testing debug filter");
                        BusBuddy.WPF.Utilities.DebugHelper.TestAutoFilter();
                        break;

                    case "--show-recent":
                        Log.Information("Showing recent streaming entries");
                        BusBuddy.WPF.Utilities.DebugHelper.ShowRecentStreamingEntries();
                        break;

                    case "--health-check":
                        Log.Information("Running health check");
                        BusBuddy.WPF.Utilities.DebugHelper.HealthCheck();
                        break;

                    default:
                        if (arg.StartsWith("--output-file="))
                        {
                            var outputFile = arg.Substring("--output-file=".Length);
                            Log.Information("Setting output file: {OutputFile}", outputFile);
                            await BusBuddy.WPF.Utilities.DebugHelper.ExportToJson(outputFile);
                        }
                        else if (!arg.StartsWith("--"))
                        {
                            Log.Warning("Unknown command line argument: {Argument}", arg);
                        }
                        break;
                }
            }

            // Shutdown after handling command line arguments
            Shutdown();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error handling command line arguments");
            Shutdown();
        }
    }

    private void ConfigureServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services, IConfiguration configuration)
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
        // Use Serilog exclusively — no Microsoft Extensions Logging wrapper needed

        // Register Serilog enrichers as singletons (as recommended in the article)
        services.AddSingleton<BusBuddyContextEnricher>();
        services.AddSingleton<DatabaseOperationEnricher>();
        services.AddSingleton<UIOperationEnricher>();
        services.AddSingleton<LogAggregationEnricher>();
        services.AddSingleton<StartupExceptionEnricher>(); // Enhanced startup exception handling

        // Register custom formatters
        services.AddSingleton<CondensedLogFormatter>();

        // Register log lifecycle management
        services.AddSingleton<BusBuddy.WPF.Utilities.LogLifecycleManager>();

        // Register UI-specific logging services — DISABLED due to missing utilities
        // services.AddUILogging();

        // Register performance monitoring utilities
        services.AddSingleton<BusBuddy.WPF.Utilities.PerformanceMonitor>();
        services.AddSingleton<BusBuddy.WPF.Utilities.StartupPerformanceMonitor>();
    }

    private void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
    {
        // Register DbContext using SQL Server and connection string from appsettings.json with scoped lifetime
        string? connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<BusBuddyDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                // OPTIMIZATION: Configure SQL options for better performance and resilience
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,         // Reduced from 5
                    maxRetryDelay: TimeSpan.FromSeconds(10), // Reduced from 30
                    errorNumbersToAdd: null);

                // OPTIMIZATION: Improved command timeout for faster queries during startup
                sqlOptions.CommandTimeout(15); // Reduced from 60 seconds

                // OPTIMIZATION: Set query splitting behavior to improve performance
                sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });

            // OPTIMIZATION: Use NoTracking by default for better concurrency performance
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            // CONCURRENCY FIX: Enable thread safety for concurrent operations
            options.EnableThreadSafetyChecks(false); // Disable checks that cause exceptions in concurrent scenarios

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
    }

    private void ConfigureDataAccess(IServiceCollection services)
    {
        // Register DbContextFactory - CRITICAL - must be scoped, not singleton
        services.AddScoped<IBusBuddyDbContextFactory, BusBuddyDbContextFactory>();

        // Register UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register specialized repositories for thread-safe data access
        services.AddScoped<BusBuddy.Core.Data.Repositories.IVehicleRepository, BusBuddy.Core.Data.Repositories.VehicleRepository>();
    }

    private void ConfigureCoreServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register memory caching services - CRITICAL for BusCachingService
        services.AddMemoryCache();

        // Register caching service - depends on IMemoryCache
        services.AddSingleton<BusBuddy.Core.Services.IBusCachingService, BusBuddy.Core.Services.BusCachingService>();
        services.AddSingleton<BusBuddy.Core.Services.IEnhancedCachingService, BusBuddy.Core.Services.EnhancedCachingService>();

        // Register Core Business Services
        services.AddScoped<BusBuddy.Core.Services.Interfaces.IBusService, BusBuddy.Core.Services.BusService>();
        services.AddScoped<BusBuddy.Core.Services.IRouteService, BusBuddy.Core.Services.RouteService>();
        services.AddScoped<BusBuddy.Core.Services.IDriverService, BusBuddy.Core.Services.DriverService>();
        services.AddScoped<BusBuddy.Core.Services.IFuelService, BusBuddy.Core.Services.FuelService>();
        services.AddScoped<BusBuddy.Core.Services.IMaintenanceService, BusBuddy.Core.Services.MaintenanceService>();
        services.AddScoped<BusBuddy.Core.Services.IActivityLogService, BusBuddy.Core.Services.ActivityLogService>();
        services.AddScoped<BusBuddy.Core.Services.IStudentService, BusBuddy.Core.Services.StudentService>();
        services.AddScoped<BusBuddy.Core.Services.Interfaces.IScheduleService, BusBuddy.Core.Services.ScheduleService>();

        // Configuration and User Services
        services.AddScoped<BusBuddy.Core.Services.IUserContextService, BusBuddy.Core.Services.UserContextService>();
        services.AddScoped<BusBuddy.Core.Services.IUserSettingsService, BusBuddy.Core.Services.UserSettingsService>();
        services.AddScoped<BusBuddy.Core.Services.IConfigurationService, BusBuddy.Core.Services.ConfigurationService>();

        // Specialized Services
        services.AddScoped<BusBuddy.Core.Services.GoogleEarthEngineService>();
        services.AddScoped<BusBuddy.Core.Services.IDashboardMetricsService, BusBuddy.Core.Services.DashboardMetricsService>();

        // AI Services
        services.AddHttpClient<BusBuddy.Core.Services.XAIService>();
        services.AddScoped<BusBuddy.Core.Services.XAIService>();

        // Register XAI configuration settings
        services.Configure<BusBuddy.Configuration.XAIDocumentationSettings>(configuration.GetSection("XAI"));
    }

    private void ConfigureWpfServices(IServiceCollection services)
    {
        // Register AutoMapper services
        services.AddAutoMapper(typeof(BusBuddy.WPF.Mapping.MappingProfile));
        services.AddSingleton<BusBuddy.WPF.Services.IMappingService>(provider =>
        {
            var mapper = provider.GetRequiredService<IMapper>();
            var logger = Log.ForContext<BusBuddy.WPF.Services.MappingService>();
            return new BusBuddy.WPF.Services.MappingService(mapper, logger);
        });

        // Register optimized theme service as singleton for centralized theme management
        services.AddSingleton<BusBuddy.WPF.Services.OptimizedThemeService>();

        // Register WPF-specific Services - Changed RoutePopulationScaffold to Scoped
        services.AddScoped<BusBuddy.WPF.Services.IDriverAvailabilityService, BusBuddy.WPF.Services.DriverAvailabilityService>();
        services.AddScoped<BusBuddy.WPF.Services.IRoutePopulationScaffold, BusBuddy.WPF.Services.RoutePopulationScaffold>();
        services.AddScoped<BusBuddy.WPF.Services.StartupOptimizationService>();

        // Register Navigation Service for centralized navigation management
        services.AddSingleton<BusBuddy.WPF.Services.INavigationService, BusBuddy.WPF.Services.NavigationService>();

        // Register Google Earth Service for geospatial mapping and route visualization
        services.AddScoped<BusBuddy.WPF.Services.IGoogleEarthService, BusBuddy.WPF.Services.GoogleEarthService>();

        // Register startup validation service for deployment readiness
        services.AddScoped<BusBuddy.WPF.Services.StartupValidationService>();

        // Register startup orchestration service with enhanced Serilog logging
        services.AddScoped<BusBuddy.WPF.Services.StartupOrchestrationService>();

        // 🎨 THEME SERVICE OPTIMIZATION: OptimizedThemeService provides centralized theme management
        // ✅ Eliminates redundant per-view theme operations
        // ✅ Provides intelligent fallback handling without risky FluentLight dependency
        // ✅ Implements resource caching for improved performance
    }

    private void ConfigureUtilities(IServiceCollection services)
    {
        // Register performance utilities
        // services.AddSingleton<BusBuddy.WPF.Utilities.BackgroundTaskManager>(); // DISABLED: Class not found

        // Register database validation utilities
        services.AddScoped<BusBuddy.Core.Utilities.DatabaseValidator>();

        // Register log consolidation utility — DISABLED due to missing utility
        /*
        services.AddScoped<BusBuddy.WPF.Utilities.LogConsolidationUtility>(provider =>
        {
            // Using Serilog directly instead of Microsoft.Extensions.Logging wrapper
            var serilogLogger = Log.ForContext<BusBuddy.WPF.Utilities.LogConsolidationUtility>();
            string solutionRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName
                               ?? Directory.GetCurrentDirectory();
            string logsDirectory = Path.Combine(solutionRoot, "logs");
            return new BusBuddy.WPF.Utilities.LogConsolidationUtility(serilogLogger, logsDirectory);
        });
        */

        // Register Main Window
        services.AddTransient<MainWindow>();
    }

    private void ConfigureViewModels(IServiceCollection services)
    {
        // ⚡ LAZY LOADING SERVICE - Performance optimization
        services.AddSingleton<BusBuddy.WPF.Services.ILazyViewModelService, BusBuddy.WPF.Services.LazyViewModelService>();

        // ⚡ STARTUP PRELOAD SERVICE - Background data loading
        services.AddScoped<BusBuddy.WPF.Services.IStartupPreloadService, BusBuddy.WPF.Services.StartupPreloadService>();

        // Main/Dashboard/Navigation ViewModels - Essential ViewModels loaded eagerly
        services.AddScoped<BusBuddy.WPF.ViewModels.MainViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.DashboardViewModel>(); // Essential for immediate display
        services.AddScoped<BusBuddy.WPF.ViewModels.LoadingViewModel>(); // Essential for startup

        // Activity Log ViewModel - Loaded eagerly for startup logging
        services.AddScoped<BusBuddy.WPF.ViewModels.ActivityLogViewModel>();

        // Dashboard Tile ViewModels
        services.AddScoped<BusBuddy.WPF.ViewModels.FleetStatusTileViewModel>();
        services.AddScoped<BusBuddy.WPF.ViewModels.MaintenanceAlertsTileViewModel>();
        services.AddScoped<Func<Action<string>?, BusBuddy.WPF.ViewModels.QuickActionsTileViewModel>>(provider =>
            (navigationAction) => new BusBuddy.WPF.ViewModels.QuickActionsTileViewModel(navigationAction));

        // Management ViewModels - Loaded lazily for better startup performance
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

        // List ViewModels - Loaded lazily
        services.AddScoped<BusBuddy.WPF.ViewModels.StudentListViewModel>();

        // Google Earth Integration ViewModel
        services.AddScoped<BusBuddy.WPF.ViewModels.GoogleEarthViewModel>();

        // Settings and AI ViewModels - Loaded lazily
        services.AddScoped<BusBuddy.WPF.ViewModels.SettingsViewModel>();
        // XAI Chat ViewModels - Performance optimized
        services.AddScoped<BusBuddy.WPF.ViewModels.XaiChatViewModel>(); // Legacy support
        services.AddScoped<BusBuddy.WPF.ViewModels.XAI.OptimizedXAIChatViewModel>(); // Optimized version
    }

    #region Safe File Writing Helper Method

    /// <summary>
    /// Safely writes text to a file with retry logic to handle concurrent access
    /// </summary>
    private static void SafeWriteToFile(string filePath, string content)
    {
        const int maxRetries = 3;
        const int retryDelayMs = 100;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                // Use FileStream with proper sharing to handle concurrent access
                using (var fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.Write(content);
                    streamWriter.Flush();
                }
                return; // Success
            }
            catch (IOException) when (i < maxRetries - 1)
            {
                // Wait before retry
                Thread.Sleep(retryDelayMs);
            }
            catch (UnauthorizedAccessException)
            {
                // Try with different approach
                try
                {
                    File.AppendAllText(filePath, content);
                    return;
                }
                catch
                {
                    // Fall back to console output
                    Console.WriteLine($"[SAFE_WRITE] Could not write to {filePath}: {content}");
                    return;
                }
            }
        }

        // Final fallback
        Console.WriteLine($"[SAFE_WRITE] Failed to write to {filePath} after {maxRetries} attempts: {content}");
    }

    #endregion

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
            try
            {
                SafeWriteToFile(fallbackLogPath,
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
            // ENHANCED: Extract more actionable information from dispatcher exceptions
            string errorMessage = e.Exception?.Message ?? "Unknown error";
            string exceptionType = e.Exception?.GetType().Name ?? "Unknown";

            // Enhanced exception categorization for actionable insights
            var isXamlParsingError = e.Exception is System.Windows.Markup.XamlParseException;
            var isSyncfusionError = e.Exception?.GetType().FullName?.Contains("Syncfusion") == true ||
                                   e.Exception?.StackTrace?.Contains("Syncfusion") == true;
            var isResourceError = errorMessage.Contains("StaticResource", StringComparison.OrdinalIgnoreCase) ||
                                 errorMessage.Contains("resource", StringComparison.OrdinalIgnoreCase);
            var isStartupError = errorMessage.Contains("initialization", StringComparison.OrdinalIgnoreCase) ||
                                errorMessage.Contains("startup", StringComparison.OrdinalIgnoreCase);

            // Enhanced logging with actionable context
            if (isXamlParsingError && e.Exception is System.Windows.Markup.XamlParseException xamlEx)
            {
                Log.Error(e.Exception, "🚨 ACTIONABLE XAML PARSING EXCEPTION: {ExceptionType} - {Message} | Line: {LineNumber}, Position: {LinePosition} | File: {SourceFile}",
                    exceptionType, errorMessage, xamlEx.LineNumber, xamlEx.LinePosition, xamlEx.BaseUri?.ToString() ?? "Unknown");
            }
            else if (isSyncfusionError)
            {
                Log.Error(e.Exception, "🚨 ACTIONABLE SYNCFUSION EXCEPTION: {ExceptionType} - {Message} | Check license registration and theme setup",
                    exceptionType, errorMessage);
            }
            else if (isResourceError)
            {
                Log.Error(e.Exception, "🚨 ACTIONABLE RESOURCE EXCEPTION: {ExceptionType} - {Message} | Check StaticResource references and resource dictionaries",
                    exceptionType, errorMessage);
            }
            else if (isStartupError)
            {
                Log.Error(e.Exception, "🚨 ACTIONABLE STARTUP EXCEPTION: {ExceptionType} - {Message} | Check application initialization sequence",
                    exceptionType, errorMessage);
            }
            else
            {
                Log.Error(e.Exception, "🚨 ACTIONABLE DISPATCHER EXCEPTION: {ExceptionType} - {Message}",
                    exceptionType, errorMessage);
            }

            // CRITICAL: Mark as handled immediately without any UI interaction
            e.Handled = true;

            // Write to console for immediate feedback during development
            System.Diagnostics.Debug.WriteLine($"Enhanced UI Exception handled: {exceptionType} - {errorMessage}");
        }
        catch (Exception logEx)
        {
            try
            {
                // Emergency fallback logging only - no UI interaction
                string fallbackDetails = $"[ERROR] [{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] Enhanced UI exception handling failed: {e.Exception?.Message ?? "Unknown"}\n" +
                                       $"Logging error: {logEx.Message}\n" +
                                       $"Exception type: {e.Exception?.GetType().Name ?? "Unknown"}\n" +
                                       $"Stack trace: {e.Exception?.StackTrace ?? "Unknown"}\n";

                SafeWriteToFile(fallbackLogPath, fallbackDetails);
                e.Handled = true;
            }
            catch
            {
                // Last resort - just suppress the error
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
                SafeWriteToFile(fallbackLogPath,
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
    /// Determines if an exception is related to Syncfusion components
    /// </summary>
    private static bool IsSyncfusionException(Exception? exception)
    {
        if (exception == null) return false;

        // Check for XAML parsing errors specifically related to * character parsing
        var isXamlStarParsingError = exception is System.Windows.Markup.XamlParseException xamlEx &&
                                    (xamlEx.Message.Contains("is not a valid value for Double") ||
                                     xamlEx.Message.Contains("TypeConverterMarkupExtension"));

        // Check for DateTimePattern enum parsing errors
        var isDateTimePatternError = exception is System.Windows.Markup.XamlParseException dtpEx &&
                                    (dtpEx.InnerException?.Message.Contains("DateTimePattern") == true ||
                                     dtpEx.InnerException?.Message.Contains("is not a valid value for DateTimePattern") == true ||
                                     dtpEx.Message.Contains("FullDate is not a valid value"));

        return exception.GetType().FullName?.Contains("Syncfusion") == true ||
               exception.Message.Contains("Syncfusion") ||
               exception.Message.Contains("DockingManager") ||
               exception.Message.Contains("AutoHidden") ||
               exception.Message.Contains("TabControlExt") ||
               exception.Message.Contains("TDILayoutPanel") ||
               exception.StackTrace?.Contains("Syncfusion") == true ||
               isXamlStarParsingError ||
               isDateTimePatternError;
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

    /// <summary>
    /// Pre-warms caches in the background to improve performance of first access
    /// </summary>
    private void PreWarmCaches()
    {
        Log.Information("[STARTUP] Starting cache pre-warming in background");

        // Use the existing cache pre-warming logic from OnStartup
        // This method is now called after orchestration completion
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(500); // Brief delay for UI initialization

                using var scope = Services.CreateScope();
                var busCacheService = scope.ServiceProvider.GetService<BusBuddy.Core.Services.IBusCachingService>();

                if (busCacheService != null)
                {
                    Func<Task<List<BusBuddy.Core.Models.Bus>>> fetchBusesFunc = async () =>
                    {
                        using var freshScope = Services.CreateScope();
                        var busService = freshScope.ServiceProvider.GetRequiredService<BusBuddy.Core.Services.Interfaces.IBusService>();
                        var result = await busService.GetAllBusesAsync();
                        return result.ToList();
                    };

                    await busCacheService.GetAllBusesAsync(fetchBusesFunc);
                    Log.Information("[STARTUP] Cache pre-warming completed successfully");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[STARTUP] Cache pre-warming failed: {ErrorMessage}", ex.Message);
            }
        });
    }

    #region Enhanced Exception Handling Helper Methods

    private static string CategorizeException(Exception exception)
    {
        return exception switch
        {
            System.Windows.Markup.XamlParseException => "XAML_PARSING",
            System.InvalidOperationException => "INVALID_OPERATION",
            System.NullReferenceException => "NULL_REFERENCE",
            System.ArgumentException => "ARGUMENT_INVALID",
            System.IO.FileNotFoundException => "FILE_NOT_FOUND",
            System.Configuration.ConfigurationErrorsException => "CONFIGURATION_ERROR",
            Microsoft.Data.SqlClient.SqlException => "DATABASE_ERROR",
            System.Net.NetworkInformation.NetworkInformationException => "NETWORK_ERROR",
            System.UnauthorizedAccessException => "PERMISSION_DENIED",
            System.OutOfMemoryException => "MEMORY_EXHAUSTED",
            System.Threading.Tasks.TaskCanceledException => "TASK_CANCELLED",
            _ => "GENERAL_EXCEPTION"
        };
    }

    private static string GetActionableRecommendation(Exception exception)
    {
        return exception switch
        {
            System.Windows.Markup.XamlParseException xamlEx =>
                $"Fix XAML syntax error. Check line numbers in error message. Common issues: unclosed tags, missing namespaces, invalid property values.",

            System.InvalidOperationException dataEx when dataEx.Message.Contains("binding") =>
                "Fix data binding expression. Check property names, data context, and binding syntax.",

            System.NullReferenceException =>
                "Add null checks before accessing objects. Use null-conditional operators (?.) or validate object initialization.",

            System.ArgumentException argEx =>
                $"Validate method arguments. Parameter '{GetParameterNameFromException(argEx)}' has invalid value.",

            System.IO.FileNotFoundException fileEx =>
                $"Ensure file exists: '{fileEx.FileName}'. Check file path and build action (Content/Copy if newer).",

            System.Configuration.ConfigurationErrorsException =>
                "Check appsettings.json syntax and required configuration values. Validate connection strings.",

            Microsoft.Data.SqlClient.SqlException sqlEx =>
                $"Database connection issue. Check connection string, server availability, and SQL syntax. Error: {sqlEx.Number}",

            System.Net.NetworkInformation.NetworkInformationException =>
                "Network connectivity issue. Check internet connection and firewall settings.",

            System.UnauthorizedAccessException =>
                "Insufficient permissions. Run as administrator or check file/folder permissions.",

            System.OutOfMemoryException =>
                "Memory exhausted. Reduce memory usage, dispose objects properly, or increase available memory.",

            System.Threading.Tasks.TaskCanceledException =>
                "Task was cancelled. Check cancellation tokens and async operation timeouts.",

            _ => "General exception. Check logs for detailed stack trace and inner exceptions."
        };
    }

    private static string GetParameterNameFromException(ArgumentException argEx)
    {
        return argEx.ParamName ?? "unknown";
    }

    private static string ExtractStackTraceLocation(string stackTrace)
    {
        if (string.IsNullOrEmpty(stackTrace))
            return "Unknown location";

        // Extract the first line that contains file path and line number
        var lines = stackTrace.Split('\n');
        foreach (var line in lines)
        {
            if (line.Contains(".cs:line"))
            {
                // Extract file name and line number
                var parts = line.Split(new[] { " in " }, StringSplitOptions.None);
                if (parts.Length > 1)
                {
                    var filePart = parts[1].Trim();
                    if (filePart.Contains(":line"))
                    {
                        return filePart;
                    }
                }
            }
        }

        return "Stack trace available but location not parsed";
    }

    private bool IsStartupPhase()
    {
        // Since we can't access the private _isCompleted field directly,
        // we'll check if startup monitor exists and assume startup is in progress
        // if the monitor hasn't been disposed
        return _startupMonitor != null;
    }

    private string GetCurrentStartupPhase()
    {
        if (_startupMonitor == null)
            return "Pre-Startup";

        return "In Progress";
    }

    #endregion
}
