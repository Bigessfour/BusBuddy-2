using System.Windows;
using BusBuddy.Core.Services;
using BusBuddy.Core.Services.Interfaces;
using BusBuddy.WPF.ViewModels.Activity;
using Microsoft.Extensions.DependencyInjection;

namespace BusBuddy.WPF.Services
{
    public class DialogService
    {
        public bool? ShowActivityScheduleDialog(ActivityScheduleViewModel viewModel)
        {
            try
            {
                // Get required services from the application's service provider
                var serviceProvider = ((App)Application.Current).Services;
                var activityScheduleService = serviceProvider.GetRequiredService<IActivityScheduleService>();
                var driverService = serviceProvider.GetRequiredService<IDriverService>();
                var busService = serviceProvider.GetRequiredService<IBusService>();

                // Create the dialog with proper constructor parameters
                var dialog = new Views.Schedule.ActivityScheduleDialog(
                    activityScheduleService,
                    driverService,
                    busService,
                    viewModel.SelectedSchedule)
                {
                    Owner = Application.Current?.MainWindow
                };

                return dialog.ShowDialog();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error opening activity schedule dialog: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}
