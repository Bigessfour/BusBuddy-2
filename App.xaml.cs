using System.Windows;
using Serilog;
using Serilog.Core.Enrichers;

namespace BusBuddy.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml — Main application entry point
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize Serilog with enrichment for application-level logging
            Log.Logger = new LoggerConfiguration()
                .Enrich.With(new MachineNameEnricher())
                .Enrich.With(new ThreadIdEnricher())
                .Enrich.With(new ProcessIdEnricher())
                .WriteTo.Console()
                .CreateLogger();

            Log.Information("Application starting with Serilog and enrichment");

            // Initialize application-level resources and configurations
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Application shutting down");
            Log.CloseAndFlush();

            // Cleanup application resources
            base.OnExit(e);
        }
    }
}
