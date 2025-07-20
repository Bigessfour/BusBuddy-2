using System;
using System.Windows;
using BusBuddy.WPF.ViewModels.Schedule;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using BusBuddy.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BusBuddy.WPF.Views.Schedule
{
    /// <summary>
    /// Interaction logic for ScheduleDetailsDialog.xaml
    /// Displays detailed information about a selected schedule with professional BusBuddy theming
    /// </summary>
    public partial class ScheduleDetailsDialog : Window, IDisposable
    {
        private readonly ScheduleDetailsViewModel _viewModel;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the ScheduleDetailsDialog class
        /// </summary>
        /// <param name="schedule">The schedule to display details for</param>
        /// <param name="serviceProvider">The service provider for dependency injection</param>
        public ScheduleDetailsDialog(Core.Models.Schedule schedule, IServiceProvider serviceProvider)
        {
            InitializeComponent();

            // Get required services from the service provider
            var scheduleService = serviceProvider.GetRequiredService<IScheduleService>();
            var busService = serviceProvider.GetRequiredService<IBusService>();
            var driverService = serviceProvider.GetRequiredService<IDriverService>();
            var logger = serviceProvider.GetRequiredService<ILogger<ScheduleDetailsViewModel>>();

            // Create and set the view model
            _viewModel = new ScheduleDetailsViewModel(schedule, scheduleService, busService, driverService, serviceProvider, logger);
            DataContext = _viewModel;

            // Set the window title to include the schedule route name
            Title = $"Schedule Details - {schedule.Route?.RouteName ?? "Unknown Route"}";
        }

        /// <summary>
        /// Handles the Close button click event
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The event arguments</param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Override to handle window closing and cleanup
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnClosed(EventArgs e)
        {
            // Clean up resources
            Dispose();
            base.OnClosed(e);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern.
        /// </summary>
        /// <param name="disposing">true if disposing managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _viewModel?.Dispose();
                _disposed = true;
            }
        }
    }
}
