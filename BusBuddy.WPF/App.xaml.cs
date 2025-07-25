using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using BusBuddy.WPF.ViewModels;

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
                // Register Syncfusion license (Phase 1 requirement)
                try
                {
                    var licenseKey = Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY");
                    if (!string.IsNullOrEmpty(licenseKey))
                    {
                        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(licenseKey);
                        Console.WriteLine("✅ Syncfusion license registered successfully");
                    }
                    else
                    {
                        Console.WriteLine("⚠️ No Syncfusion license key found in environment variables");
                    }
                }
                catch (Exception ex)
                {
                    // Phase 1: Simple error handling
                    Console.WriteLine($"❌ Syncfusion license error: {ex.Message}");
                    MessageBox.Show($"Syncfusion license error: {ex.Message}", "Warning");
                }

                Console.WriteLine("🏗️ Building dependency injection host...");

                // Phase 1: Basic host builder
                _host = Host.CreateDefaultBuilder()
                    .ConfigureServices((context, services) =>
                    {
                        Console.WriteLine("📦 Registering services...");

                        // Phase 1: Register only essential ViewModels
                        services.AddTransient<MainWindowViewModel>();
                        services.AddTransient<DashboardViewModel>();
                        services.AddTransient<DriversViewModel>();
                        services.AddTransient<VehiclesViewModel>();
                        services.AddTransient<ActivityScheduleViewModel>();

                        Console.WriteLine("✅ Services registered successfully");
                    })
                    .Build();

                Console.WriteLine("✅ Host built successfully");

                base.OnStartup(e);

                Console.WriteLine("🪟 Creating main window...");

                // Phase 1: Simple window creation
                var mainWindow = new Views.Main.MainWindow();

                Console.WriteLine("🔗 Getting ViewModel from DI container...");
                var viewModel = _host.Services.GetRequiredService<MainWindowViewModel>();

                Console.WriteLine("🎯 Setting DataContext...");
                mainWindow.DataContext = viewModel;

                Console.WriteLine("🚀 Showing main window...");
                mainWindow.Show();

                Console.WriteLine("✅ Application startup completed successfully!");
            }
            catch (Exception ex)
            {
                // Phase 2: Enhanced error reporting
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
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _host?.Dispose();
            base.OnExit(e);
        }
    }
}
