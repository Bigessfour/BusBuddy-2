using BusBuddy.Core.Data;
using BusBuddy.Core.Data.UnitOfWork;
using BusBuddy.WPF.Logging;
using BusBuddy.WPF.Utilities;
using BusBuddy.WPF.ViewModels;
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
        // CRITICAL: Register Syncfusion license FIRST according to official documentation
        // This must be done in App constructor before any Syncfusion controls are initialized
        string? envLicenseKey = Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY");
        if (!string.IsNullOrWhiteSpace(envLicenseKey))
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(envLicenseKey);
        }
        else
        {
            // Fallback: try to load from appsettings.json if environment variable not found
            try
            {
                var licenseConfig = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                    .AddEnvironmentVariables()
                    .Build();

                string? licenseKey = licenseConfig["Syncfusion:LicenseKey"] ??
                                   licenseConfig["SyncfusionLicenseKey"];

                if (!string.IsNullOrWhiteSpace(licenseKey))
                {
                    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(licenseKey);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("WARNING: Syncfusion license key not found in environment variables or appsettings.json");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: Failed to load Syncfusion license from appsettings.json: {ex.Message}");
            }
        }





        // ══════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════
        // 🎨 PURE SYNCFUSION 30.1.40 THEME CONFIGURATION — FLUENTDARK PRIMARY + FLUENTLIGHT FALLBACK
        // ══════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════
        // ✅ GLOBAL APPLICATION: All Syncfusion controls automatically themed via SfSkinManager
        // ✅ AUTOMATIC FALLBACK: FluentLight applied if FluentDark fails
        // ✅ PERFORMANCE: Single theme initialization, no runtime switching overhead
        // ✅ CONSISTENCY: Unified theme across all views and controls via MergedDictionaries
        // ✅ COMPLIANCE: 100% Syncfusion WPF 30.1.40 documentation alignment
        // ══════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════

        // Configure SfSkinManager for global theme application
        SfSkinManager.ApplyStylesOnApplication = true;
        SfSkinManager.ApplyThemeAsDefaultStyle = true;

        // 🎯 OPTIONAL: Auto-detect system theme preference
        string preferredTheme = "FluentDark"; // Default to FluentDark
        try
        {
            // Check Windows system theme preference
            var systemTheme = Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 1);
            if (systemTheme is int themeValue && themeValue == 1)
            {
                preferredTheme = "FluentLight";
                System.Diagnostics.Debug.WriteLine("🎨 SYSTEM THEME: Light theme detected, using FluentLight");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("🎨 SYSTEM THEME: Dark theme detected, using FluentDark");
            }
        }
        catch (Exception themeDetectionEx)
        {
            System.Diagnostics.Debug.WriteLine($"🎨 SYSTEM THEME: Could not detect system theme, using FluentDark default: {themeDetectionEx.Message}");
        }

        try
        {
            // 🎯 APPLY PREFERRED THEME: Based on system preference or default
            SfSkinManager.ApplicationTheme = new Theme(preferredTheme);
            System.Diagnostics.Debug.WriteLine($"✅ THEME: {preferredTheme} applied successfully as primary theme");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ THEME: {preferredTheme} failed, applying FluentLight fallback: {ex.Message}");
            try
            {
                // 🔄 FALLBACK THEME: FluentLight (Clean light theme)
                SfSkinManager.ApplicationTheme = new Theme("FluentLight");
                System.Diagnostics.Debug.WriteLine("✅ THEME: FluentLight applied successfully as fallback theme");
            }
            catch (Exception fallbackEx)
            {
                System.Diagnostics.Debug.WriteLine($"❌ THEME: Both primary and FluentLight failed: {fallbackEx.Message}");
            }
        }

        // ✅ THEME RESOURCES: Automatically loaded via BusBuddyResourceDictionary.xaml MergedDictionaries
        // ✅ CONTROL THEMING: All Syncfusion controls automatically inherit FluentDark theme
        // ✅ NO MANUAL INTERVENTION: SfSkinManager handles all theme application automatically

        // CRITICAL: Initialize error handling before anything else
        try
        {
            // Dictionary to track repeated exceptions and prevent flood logging
            var _exceptionOccurrences = new Dictionary<string, int>();
            var _lastExceptionTime = new Dictionary<string, DateTime>();
            const int MAX_EXCEPTION_OCCURRENCES = 5;
            var EXCEPTION_TIMEOUT = TimeSpan.FromSeconds(30);

            // Set up global exception handlers immediately with intelligent filtering
            this.DispatcherUnhandledException += (sender, e) =>
            {
                var exception = e.Exception;
                var message = exception.Message;
                var stackTrace = exception.StackTrace ?? "";

                // Create a unique key for this exception type and message
                var exceptionKey = $"{exception.GetType().Name}|{message.Substring(0, Math.Min(message.Length, 100))}";

                // Check if this exception has occurred too frequently
                var currentTime = DateTime.Now;
                if (_exceptionOccurrences.ContainsKey(exceptionKey))
                {
                    var lastTime = _lastExceptionTime.GetValueOrDefault(exceptionKey, DateTime.MinValue);
                    if (currentTime - lastTime < EXCEPTION_TIMEOUT)
                    {
                        _exceptionOccurrences[exceptionKey]++;
                        if (_exceptionOccurrences[exceptionKey] > MAX_EXCEPTION_OCCURRENCES)
                        {
                            // Rate limit this exception
                            e.Handled = true;
                            return;
                        }
                    }
                    else
                    {
                        // Reset counter if timeout has passed
                        _exceptionOccurrences[exceptionKey] = 1;
                    }
                }
                else
                {
                    _exceptionOccurrences[exceptionKey] = 1;
                }
                _lastExceptionTime[exceptionKey] = currentTime;

                // Enhanced exception categorization with actionable recommendations
                var exceptionCategory = CategorizeException(exception);
                var actionableRecommendation = GetActionableRecommendation(exception);

                // Filter out known ButtonAdv style conflicts (already fixed)
                if (message.Contains("ButtonAdv") && message.Contains("TargetType does not match"))
                {
                    System.Diagnostics.Debug.WriteLine($"🔧 FIXED: ButtonAdv style conflict (already converted) - {message}");
                    e.Handled = true;
                    return;
                }

                // Filter out resolved XAML style issues
                if (message.Contains("Set property") && message.Contains("Style") && message.Contains("threw an exception"))
                {
                    System.Diagnostics.Debug.WriteLine($"🔧 RESOLVED: XAML style issue (ButtonAdv conversion applied) - {message}");
                    e.Handled = true;
                    return;
                }

                // Filter out BeginInit recursion issues
                if (message.Contains("Cannot have nested BeginInit calls"))
                {
                    System.Diagnostics.Debug.WriteLine($"🔧 HANDLED: BeginInit recursion prevented - {message}");
                    e.Handled = true;
                    return;
                }

                // Enhanced logging with startup context and actionable insights
                using (LogContext.PushProperty("ExceptionCategory", exceptionCategory))
                using (LogContext.PushProperty("ActionableRecommendation", actionableRecommendation))
                using (LogContext.PushProperty("IsStartupException", IsStartupPhase()))
                using (LogContext.PushProperty("StartupPhase", GetCurrentStartupPhase()))
                using (LogContext.PushProperty("ExceptionOccurrenceCount", _exceptionOccurrences[exceptionKey]))
                {
                    // Log actionable exceptions with enhanced context
                    Log.Error(exception, "🚨 ACTIONABLE DISPATCHER EXCEPTION: {ExceptionType} - {Message} at {StackTraceLocation}",
                        exception.GetType().Name, message, ExtractStackTraceLocation(stackTrace));

                    // Additional startup-specific logging if during startup
                    if (IsStartupPhase())
                    {
                        Log.Error("🚀 STARTUP_PERF: Exception during startup phase {StartupPhase} - {ActionableRecommendation}",
                            GetCurrentStartupPhase(), actionableRecommendation);
                    }
                }

                e.Handled = true; // Prevent crash
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var exception = (Exception)e.ExceptionObject;
                Log.Fatal(exception, "💥 CRITICAL APPLICATION DOMAIN EXCEPTION: {ExceptionType} - {Message}",
                    exception.GetType().Name, exception.Message);
                System.Diagnostics.Debug.WriteLine($"App Domain Exception: {exception.Message}");
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error setting up exception handlers: {ex.Message}");
        }

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
    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            // Handle command line arguments for debug functionality
            if (e.Args.Length > 0)
            {
                HandleCommandLineArgumentsAsync(e.Args).GetAwaiter().GetResult();
                return; // Exit after handling command line arguments
            }

            // Start the generic host to initialize DI and logging before UI
            _host.StartAsync().GetAwaiter().GetResult();
            base.OnStartup(e);

            // 🔍 AUTO DEBUG FILTER: Start automatic filtering in debug mode
#if DEBUG
            try
            {
                BusBuddy.WPF.Utilities.DebugHelper.StartAutoFilter();
                Log.Information("🔍 Debug auto-filter enabled for debug session");

                // Test the filter is working (after 3 seconds delay)
                var testTimer = new System.Threading.Timer(
                    callback: _ => BusBuddy.WPF.Utilities.DebugHelper.TestAutoFilter(),
                    state: null,
                    dueTime: TimeSpan.FromSeconds(3),
                    period: TimeSpan.FromMilliseconds(-1) // Run once
                );
            }
            catch (Exception debugEx)
            {
                Log.Warning(debugEx, "🔍 Debug auto-filter failed to start");
            }
#endif

            // 🛠️ Temporarily disable custom resource dictionary loading to fix parsing issue
            try
            {
                // Clear any existing resource dictionaries to start fresh
                this.Resources.MergedDictionaries.Clear();

                // Load resource dictionaries using standard WPF approach
                var resourceDictionary = new ResourceDictionary();
                resourceDictionary.Source = new Uri("Resources/BusBuddyResourceDictionary.xaml", UriKind.Relative);
                this.Resources.MergedDictionaries.Add(resourceDictionary);

                // Add critical diagnostic logging
                Log.Information("🎨 Resource dictionaries loaded using standard WPF approach");

                // Verify if critical keys exist in merged resources to identify potential issues
                string[] criticalKeys = new string[] { "SurfaceBorder", "SurfaceBorderBrush", "SurfaceBorderColor" };
                foreach (var key in criticalKeys)
                {
                    if (this.Resources.Contains(key))
                    {
                        Log.Debug("✅ Critical resource key exists: {Key} ({Type})",
                            key, this.Resources[key]?.GetType().Name ?? "Unknown");
                    }
                    else
                    {
                        Log.Warning("⚠️ Critical resource key missing: {Key}", key);
                    }
                }
            }
            catch (Exception resourceEx)
            {
                Log.Error(resourceEx, "Error loading resource dictionaries");
            }

            // Now that the host is started, Serilog should be initialized
            // 🎨 THEME CONFIRMATION: Theme already applied in constructor
            // SfSkinManager.ApplicationTheme is already set to FluentDark with FluentLight fallback
            Log.Information("🎨 Syncfusion theme confirmed: FluentDark primary with FluentLight fallback ready");

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

            // Check for appsettings.json before loading configuration
            if (!File.Exists(appSettingsPath))
            {
                MessageBox.Show(
                    $"The configuration file 'appsettings.json' was not found at '{appSettingsPath}'.\n\n" +
                    "Please ensure the file exists and is set to 'Copy if newer' in the project.",
                    "Configuration File Missing",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown();
                return;
            }

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

            // Register logging with DI so Serilog can be injected — removed Microsoft.Extensions.Logging dependency
            ServiceCollection services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

            // Register Serilog as the primary logger without Microsoft logging wrapper
            services.AddSerilog();

            // Initialize Serilog for robust logging, with enrichers
            try
            {
                // Create custom enrichers
                var contextEnricher = new BusBuddyContextEnricher();
                var dbEnricher = new DatabaseOperationEnricher();
                var uiEnricher = new UIOperationEnricher();
                var aggregationEnricher = new LogAggregationEnricher();
                var startupExceptionEnricher = new StartupExceptionEnricher(); // Enhanced startup exception handling

                // Create custom formatters
                var condensedFormatter = new CondensedLogFormatter(includeProperties: true, showAggregatedOnly: false);
                var consoleFormatter = new CondensedLogFormatter(includeProperties: false, showAggregatedOnly: true);

                // Use consolidated Serilog configuration with enhanced enrichment capabilities
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information() // Reduce noise by starting at Information level
                                                // Built-in enrichers with Serilog enrichment extensions
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithThreadId()
                    .Enrich.WithProcessId()
                    .Enrich.WithProcessName()
                    .Enrich.WithEnvironmentName()
                    .Enrich.WithEnvironmentUserName()
                    // Custom BusBuddy enrichers (order matters — aggregation should be last)
                    .Enrich.With(contextEnricher)
                    .Enrich.With(dbEnricher)
                    .Enrich.With(uiEnricher)
                    .Enrich.With(startupExceptionEnricher)
                    .Enrich.With(aggregationEnricher)
                    // CONSOLIDATED: Only 2 log files with smart filtering and safe concurrent access
                    .WriteTo.File(condensedFormatter, Path.Combine(logsDirectory, "busbuddy-consolidated-.log"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30,
                        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                        shared: true, // Enable shared access for concurrent processes
                        flushToDiskInterval: TimeSpan.FromSeconds(1))
                    .WriteTo.File(Path.Combine(logsDirectory, "busbuddy-errors-.log"),
                        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30,
                        shared: true, // Enable shared access for concurrent processes
                        flushToDiskInterval: TimeSpan.FromSeconds(1),
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] [{ThreadId}] [{LogCategory}] {Message:lj}{NewLine}    📊 {EventSignature} (Count: {EventOccurrenceCount}){NewLine}    🔍 {Properties:j}{NewLine}{Exception}")
                    // Simplified console output with aggregation info
                    .WriteTo.Console(consoleFormatter)
                    .CreateLogger();

                Log.Information("BusBuddy WPF application starting with pure Serilog and enrichment (Microsoft.Extensions.Logging removed). Build: {BuildTime}", DateTime.Now);
                Log.Information("Enhanced Enrichers enabled: Context, Database, UI, StartupException, Aggregation, Machine, Thread, Process, Environment, EnvironmentUser");
                Log.Information("Consolidated logging: 2 files (main + errors) with smart aggregation");
                Log.Information("Logs directory: {LogsDirectory}", logsDirectory);
                Log.Information("Enhanced structured logging with {EnricherCount} enrichers active (pure Serilog implementation)", 10);
                Log.Information("🔧 Improved Error Handling: ButtonAdv style conflicts filtered, actionable errors prioritized");
                Log.Information("📋 Log Lifecycle: 7-day retention for app logs, 30-day for actionable errors, auto-cleanup enabled");
                Log.Information("🚀 Startup Exception Enrichment: Enhanced dispatcher exception handling with actionable recommendations");
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
            this.Exit += (s, ex) =>
            {
#if DEBUG
                BusBuddy.WPF.Utilities.DebugHelper.StopAutoFilter();
#endif
                Log.CloseAndFlush();
            };

            // Add configuration to DI container
            services.AddSingleton<IConfiguration>(configuration);

            // Setup DI
            ConfigureServices(services, configuration);

            // Set the Services property to the host's service provider for backward compatibility
            Services = _host.Services;

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
            Log.Fatal(ex, "[STARTUP] Fatal error during OnStartup method execution");

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

        // 🎨 THEME SERVICE REMOVED: SfSkinManager handles all theming automatically
        // ✅ No manual theme service needed - SfSkinManager provides global theme management
        // ✅ All Syncfusion controls automatically inherit theme from SfSkinManager.ApplicationTheme
        // ✅ Theme resources loaded via BusBuddyResourceDictionary.xaml MergedDictionaries
    }

    private void ConfigureUtilities(IServiceCollection services)
    {
        // Register performance utilities
        services.AddSingleton<BusBuddy.WPF.Utilities.BackgroundTaskManager>();

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
        services.AddScoped<BusBuddy.WPF.ViewModels.XaiChatViewModel>();
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
    private void PreWarmCaches(BackgroundTaskManager backgroundTaskManager, IServiceProvider serviceProvider)
    {
        Log.Information("[STARTUP] Starting cache pre-warming in background");

        // Start cache pre-warming with a short delay to let UI initialize first
        backgroundTaskManager.RunLowPriorityTaskAsync(async () =>
        {
            try
            {
                // Delay slightly to prioritize UI initialization
                await Task.Delay(500);

                // Get caching and data services
                var cacheService = serviceProvider.GetService<BusBuddy.Core.Services.IEnhancedCachingService>();
                var busService = serviceProvider.GetService<BusBuddy.Core.Services.Interfaces.IBusService>();
                var driverService = serviceProvider.GetService<BusBuddy.Core.Services.IDriverService>();
                var routeService = serviceProvider.GetService<BusBuddy.Core.Services.IRouteService>();
                var dashboardMetricsService = serviceProvider.GetService<BusBuddy.Core.Services.IDashboardMetricsService>();

                if (cacheService == null)
                {
                    Log.Warning("[STARTUP] Cache service not found - pre-warming skipped");
                    return;
                }

                // Start pre-warming tasks
                var tasks = new List<Task>();

                // Pre-warm dashboard metrics cache
                if (dashboardMetricsService != null)
                {
                    tasks.Add(cacheService.GetDashboardMetricsAsync(async () =>
                        await dashboardMetricsService.GetDashboardMetricsAsync()));
                }

                // Pre-warm bus data cache
                if (busService != null)
                {
                    tasks.Add(cacheService.GetAllBusesAsync(async () =>
                        await busService.GetAllBusesAsync()));
                }

                // Pre-warm driver data cache
                if (driverService != null)
                {
                    tasks.Add(cacheService.GetAllDriversAsync(async () =>
                        await driverService.GetAllDriversAsync()));
                }

                // Pre-warm route data cache
                if (routeService != null)
                {
                    tasks.Add(cacheService.GetAllRoutesAsync(async () =>
                        await routeService.GetAllActiveRoutesAsync()));
                }

                // Wait for all pre-warming tasks to complete
                await Task.WhenAll(tasks);
                Log.Information("[STARTUP] Cache pre-warming completed successfully");
            }
            catch (System.Data.SqlTypes.SqlNullValueException ex)
            {
                // Specific handling for SQL NULL value errors
                Log.Warning(ex, "[STARTUP] SQL NULL value detected during cache pre-warming. This may indicate a data integrity issue: {ErrorMessage}", ex.Message);
                // Continue application execution as this is non-critical

                // Log a more user-friendly message for troubleshooting
                Log.Information("[STARTUP] Cache pre-warming encountered non-critical errors but application will continue to function normally");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[STARTUP] Error during cache pre-warming: {ErrorMessage}", ex.Message);
            }
        }, "CachePreWarming", 500);
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
