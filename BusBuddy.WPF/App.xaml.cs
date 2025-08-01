using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.IO;
using Serilog;
using Serilog.Events;
using BusBuddy.WPF.ViewModels;
using BusBuddy.WPF.Logging;
using BusBuddy.WPF.Extensions;
using BusBuddy.Core.Extensions;
using BusBuddy.Core;
using BusBuddy.Core.Data;
using BusBuddy.Core.Services;
using BusBuddy.Core.Configuration;

namespace BusBuddy.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// Phase 1: Minimal application startup with basic ViewModels
    /// </summary>
    public partial class App : Application
    {
        private IHost? _host;

        public App()
        {
            // Register Syncfusion license before any UI initialization
            try
            {
                var licenseKey = Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY") ?? "YOUR_LICENSE_KEY";
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(licenseKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Syncfusion license registration failed: {ex.Message}");
                // Continue without license for development
            }

            // Initialize Serilog logger
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/busbuddy-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Phase 2: Enhanced error handling and logging
            try
            {
                // Configure Serilog first
                ConfigureSerilog();

                Log.Information("BusBuddy application starting up");

                // Validate working directory and file paths
                ValidateApplicationPaths();

                // Run async initialization on UI thread
                InitializeApplicationAsync(e).Wait();
            }
            catch (FileNotFoundException ex)
            {
                Log.Error(ex, "File not found during application startup: {FileName}", ex.FileName);
                HandleStartupFileError(ex);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Critical error during application startup");
                MessageBox.Show($"Critical startup error: {ex.Message}", "BusBuddy Startup Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        private void ConfigureSerilog()
        {
            // CRITICAL: Initialize Serilog logging first
            var logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(logsDirectory);
            Log.Logger = new LoggerConfiguration()
                .ConfigureUILogging(logsDirectory)
                .CreateLogger();
        }

        private void ValidateApplicationPaths()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            Log.Information("Current working directory: {Directory}", currentDirectory);

            // Ensure database directory exists
            var databasePath = Path.Combine(currentDirectory, "Data");
            if (!Directory.Exists(databasePath))
            {
                Directory.CreateDirectory(databasePath);
                Log.Information("Created database directory: {Path}", databasePath);
            }

            // Validate that required assemblies are accessible
            var assemblyLocation = typeof(App).Assembly.Location;
            var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);

            if (string.IsNullOrEmpty(assemblyDirectory) || !Directory.Exists(assemblyDirectory))
            {
                Log.Warning("Assembly directory not found or invalid: {Directory}", assemblyDirectory);
            }
            else
            {
                Log.Information("Assembly directory validated: {Directory}", assemblyDirectory);
            }
        }

        private void HandleStartupFileError(FileNotFoundException ex)
        {
            var errorMessage = ex.FileName switch
            {
                var f when f?.Contains("BusBuddy.db") == true =>
                    "Database file not found. A new database will be created.",
                var f when f?.Contains(".dll") == true =>
                    $"Required library missing: {Path.GetFileName(f)}. Please reinstall the application.",
                var f when f?.Contains("appsettings") == true =>
                    "Configuration file missing. Using default settings.",
                _ => $"Required file missing: {ex.FileName ?? "Unknown file"}"
            };

            Log.Warning("Handling file not found: {Message}", errorMessage);

            if (ex.FileName?.Contains(".dll") == true)
            {
                MessageBox.Show(errorMessage, "Missing Component",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                Shutdown(1);
            }
            else
            {
                // Continue with warning for non-critical files
                MessageBox.Show(errorMessage, "File Warning",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async Task InitializeApplicationAsync(StartupEventArgs e)
        {
            Log.Information("🏗️ Building dependency injection host...");
            Console.WriteLine("🏗️ Building dependency injection host...");

            // Phase 1: Basic host builder with database and data seeding
            _host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Load Azure configuration if available
                    var azureConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration", "Production", "appsettings.azure.json");
                    if (File.Exists(azureConfigPath))
                    {
                        config.AddJsonFile(azureConfigPath, optional: true, reloadOnChange: true);
                        Log.Information("📋 Azure configuration loaded from {ConfigPath}", azureConfigPath);
                    }

                    // Add environment variables to resolve ${VAR_NAME} placeholders
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    Log.Information("📦 Registering services...");
                    Console.WriteLine("📦 Registering services...");

                    // Register single database context to avoid conflicts
                    services.AddDbContext<BusBuddy.Core.Data.BusBuddyDbContext>(options =>
                    {
                        // Always use SQLite for MVP Phase 1 to avoid connection issues
                        var connectionString = "Data Source=BusBuddy.db";
                        Log.Information($"Using database connection: {connectionString}");
                        Console.WriteLine($"Using database connection: {connectionString}");
                        options.UseSqlite(connectionString);

                        // Configure for better error reporting
                        options.EnableDetailedErrors();
                        options.EnableSensitiveDataLogging(); // Enable for all builds in MVP Phase 1
                    });

                    // Register as both types for compatibility
                    services.AddScoped<BusBuddy.Core.BusBuddyContext>(sp =>
                    {
                        // Create new instance directly to avoid type conversion issues
                        var dbContext = sp.GetRequiredService<BusBuddy.Core.Data.BusBuddyDbContext>();
                        var optionsBuilder = new DbContextOptionsBuilder<BusBuddy.Core.BusBuddyContext>();
                        optionsBuilder.UseSqlite(dbContext.Database.GetConnectionString());
                        return new BusBuddy.Core.BusBuddyContext(optionsBuilder.Options);
                    });

                    // Phase 1: Register Phase 1 services including data seeding
                    services.AddPhase1Services();
                    // Phase 2: Register enhanced real-world data seeder service
                    services.AddScoped<IPhase2DataSeederService, Phase2DataSeederService>();

                    // Azure Configuration Support
                    var azureConfigService = new AzureConfigurationService(context.Configuration);
                    azureConfigService.RegisterServices(services);

                    // Phase 1: Register only essential ViewModels
                    services.AddTransient<MainWindowViewModel>();
                    services.AddTransient<DashboardViewModel>();
                    services.AddTransient<DriversViewModel>();
                    services.AddTransient<VehiclesViewModel>();
                    services.AddTransient<ActivityScheduleViewModel>();
                    services.AddTransient<BusBuddy.WPF.ViewModels.GoogleEarth.GoogleEarthViewModel>();

                    // Phase 2: Add UI logging services
                    services.AddUILogging();

                    Log.Information("✅ Services registered successfully");
                    Console.WriteLine("✅ Services registered successfully");
                })
                .Build();

            Log.Information("✅ Host built successfully");
            Console.WriteLine("✅ Host built successfully");

            // Ensure database is created and migrations are applied
            try
            {
                Log.Information("🗄️ Ensuring database schema is up to date...");
                Console.WriteLine("🗄️ Ensuring database schema is up to date...");

                using (var scope = _host.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<BusBuddy.Core.BusBuddyContext>();
                    await context.Database.EnsureCreatedAsync();
                    Log.Information("✅ Database schema verified");
                    Console.WriteLine("✅ Database schema verified");
                }
            }
            catch (Exception ex)
            {
                Log.Warning("⚠️ Database schema initialization failed (non-critical): {Message}", ex.Message);
                Console.WriteLine($"⚠️ Database schema initialization failed (continuing): {ex.Message}");
            }

            // For MVP Phase 1, prioritize basic data seeding and handle errors gracefully
            try
            {
                // Phase 1: Initialize data seeding first - CRITICAL
                Log.Information("🗂️ Initializing Phase 1 data...");
                Console.WriteLine("🗂️ Initializing Phase 1 data...");
                await _host.Services.InitializePhase1Async();
                Log.Information("✅ Phase 1 data seeding completed successfully");
                Console.WriteLine("✅ Phase 1 data seeding completed successfully");

                // Phase 2: Only attempt if Phase 1 succeeds
                try
                {
                    Log.Information("🗂️ Initializing Phase 2 enhanced real-world data...");
                    Console.WriteLine("🗂️ Initializing Phase 2 enhanced real-world data...");
                    await _host.Services.GetRequiredService<IPhase2DataSeederService>().SeedAsync();
                    Log.Information("✅ Phase 2 data seeding completed successfully");
                    Console.WriteLine("✅ Phase 2 data seeding completed successfully");
                }
                catch (Exception ex)
                {
                    // Non-critical Phase 2 error - log but continue
                    Log.Warning(ex, "⚠️ Phase 2 data seeding failed (non-critical): {ErrorMessage}", ex.Message);
                    Console.WriteLine($"⚠️ Phase 2 data seeding failed (continuing with Phase 1 data): {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                // Critical Phase 1 error - show message but attempt to continue
                Log.Error(ex, "❌ Critical Phase 1 data seeding failed: {ErrorMessage}", ex.Message);
                Console.WriteLine($"❌ Critical data seeding error: {ex.Message}");
                MessageBox.Show($"Data initialization error: {ex.Message}\n\nThe application will attempt to continue, but some features may not work correctly.",
                               "Data Initialization Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            base.OnStartup(e);

            Log.Information("🪟 Creating main window...");
            Console.WriteLine("🪟 Creating main window...");

            // Phase 1: Simple window creation
            var mainWindow = new Views.Main.MainWindow();

            Log.Information("🔗 Getting ViewModel from DI container...");
            Console.WriteLine("🔗 Getting ViewModel from DI container...");
            var viewModel = _host.Services.GetRequiredService<MainWindowViewModel>();

            Log.Information("🎯 Setting DataContext...");
            Console.WriteLine("🎯 Setting DataContext...");
            mainWindow.DataContext = viewModel;

            Log.Information("🚀 Showing main window...");
            Console.WriteLine("🚀 Showing main window...");
            mainWindow.Show();

            Log.Information("✅ Application startup completed successfully!");
            Console.WriteLine("✅ Application startup completed successfully!");
        }

        private void HandleStartupError(Exception ex)
        {
            // Phase 2: Enhanced error handling with Serilog
            try
            {
                Log.Fatal(ex, "❌ CRITICAL ERROR during application startup");
            }
            catch
            {
                // Fallback if logging fails
            }

            var errorMessage = $"❌ CRITICAL ERROR during application startup:\n\n" +
                              $"Error: {ex.Message}\n\n" +
                              $"Type: {ex.GetType().Name}\n\n" +
                              $"Stack Trace:\n{ex.StackTrace}";

            Console.WriteLine(errorMessage);

            MessageBox.Show(errorMessage, "🚌 BusBuddy - Critical Startup Error",
                           MessageBoxButton.OK, MessageBoxImage.Error);

            // Ensure clean shutdown
            Environment.Exit(1);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                Log.Information("🚌 BusBuddy application shutting down...");
                Log.CloseAndFlush();
            }
            catch
            {
                // Ignore logging errors during shutdown
            }

            _host?.Dispose();
            base.OnExit(e);
        }
    }
}
