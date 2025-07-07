using Microsoft.Extensions.Logging;
using Syncfusion.Windows.Forms;
using Syncfusion.Licensing;
using System;
using System.IO;
using System.Windows.Forms;

namespace Bus_Buddy;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        try
        {
            var licenseKey = Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY");
            if (!string.IsNullOrEmpty(licenseKey))
            {
                SyncfusionLicenseProvider.RegisterLicense(licenseKey);
            }

            // Initialize the service container
            ServiceContainer.Initialize();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Resolve the Dashboard from the service container
            var dashboard = ServiceContainer.GetService<Dashboard>();
            Application.Run(dashboard);
        }
        catch (Exception ex)
        {
            // Log the exception to a file for detailed analysis
            File.WriteAllText("startup-error.log", ex.ToString());
            // Display a simple message to the user
            MessageBox.Show($"An unexpected error occurred during startup. Please check the startup-error.log file for details.", "Application Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}