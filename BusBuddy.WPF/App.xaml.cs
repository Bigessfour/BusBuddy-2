using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using BusBuddy.WPF.ViewModels;
using BusBuddy.WPF.Logging;
using BusBuddy.Core.Extensions;
using BusBuddy.Core;
using BusBuddy.Core.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.IO;

namespace BusBuddy.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// Phase 1: Minimal application startup with basic ViewModels
    /// </summary>
    public partial class App : Application
    {
        private IHost? _host;

        protected override void OnStartup(StartupEventArgs e)
        {
            // Phase 2: Enhanced error handling and logging
            try
            {
                // Run async initialization
                Task.Run(async () => await InitializeApplicationAsync(e)).Wait();
            }
            catch (Exception ex)
            {
                HandleStartupError(ex);
            }
        }

        private async Task InitializeApplicationAsync(StartupEventArgs e)
        {
            // CRITICAL: Initialize Serilog logging first
            var logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(logsDirectory);
            Log.Logger = new LoggerConfiguration()
                .ConfigureUILogging(logsDirectory)
                .CreateLogger();

            Log.Information("🚌 BusBuddy application starting up...");

            // Register Syncfusion license (Phase 1 requirement)
            try
            {
                var licenseKey = Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY");
                if (!string.IsNullOrEmpty(licenseKey))
                {
                    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(licenseKey);
                    Log.Information("✅ Syncfusion license registered successfully");
                    Console.WriteLine("✅ Syncfusion license registered successfully");
                }
                else
                {
                    Log.Warning("⚠️ No Syncfusion license key found in environment variables");
                    Console.WriteLine("⚠️ No Syncfusion license key found in environment variables");
                }
            }
            catch (Exception ex)
            {
                // Phase 1: Simple error handling
                Log.Error(ex, "❌ Syncfusion license error: {ErrorMessage}", ex.Message);
                Console.WriteLine($"❌ Syncfusion license error: {ex.Message}");
                MessageBox.Show($"Syncfusion license error: {ex.Message}", "Warning");
            }

            Log.Information("🏗️ Building dependency injection host...");
            Console.WriteLine("🏗️ Building dependency injection host...");

            // Phase 1: Basic host builder with database and data seeding
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    Log.Information("📦 Registering services...");
                    Console.WriteLine("📦 Registering services...");

                    // Phase 1: Register database context
                    services.AddDbContext<BusBuddyDbContext>(options =>
                        options.UseSqlServer("Data Source=BusBuddy.db"));

                    // Phase 1: Register Phase 1 services including data seeding
                    services.AddPhase1Services();

                    // Phase 1: Register only essential ViewModels
                    services.AddTransient<MainWindowViewModel>();
                    services.AddTransient<DashboardViewModel>();
                    services.AddTransient<DriversViewModel>();
                    services.AddTransient<VehiclesViewModel>();
                    services.AddTransient<ActivityScheduleViewModel>();

                    // Phase 2: Add UI logging services
                    services.AddUILogging();

                    Log.Information("✅ Services registered successfully");
                    Console.WriteLine("✅ Services registered successfully");
                })
                .Build();

            Log.Information("✅ Host built successfully");
            Console.WriteLine("✅ Host built successfully");

            // Phase 1: Initialize data seeding
            Log.Information("🗂️ Initializing Phase 1 data...");
            Console.WriteLine("🗂️ Initializing Phase 1 data...");
            await _host.Services.InitializePhase1Async();

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
