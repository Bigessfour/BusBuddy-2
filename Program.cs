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
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JEaF5cXmRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXhec3RSRGRYU0R2WUBWYEk=");

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