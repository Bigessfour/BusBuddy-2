using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.IO;
using System.Diagnostics;
using System.Linq;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using BusBuddy.Core.Services.Interfaces;
using BusBuddy.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using Microsoft.Win32;

namespace BusBuddy.WPF.ViewModels.Schedule
{
    /// <summary>
    /// ViewModel for the Schedule Details Dialog
    /// Provides detailed information about a selected schedule with PDF report generation
    /// </summary>
    public partial class ScheduleDetailsViewModel : ObservableObject, IDisposable
    {
        private readonly IScheduleService _scheduleService;
        private readonly IBusService _busService;
        private readonly IDriverService _driverService;
        private readonly ILogger<ScheduleDetailsViewModel> _logger;
        private readonly IActivityService? _activityService;
        private readonly PdfReportService _pdfReportService;
        private readonly IServiceProvider _serviceProvider;
        private bool _disposed = false;

        /// <summary>
        /// The schedule being displayed
        /// </summary>
        [ObservableProperty]
        private Core.Models.Schedule _schedule;

        /// <summary>
        /// Collection of recent activities related to this schedule
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<Core.Models.Activity> _recentActivities;

        /// <summary>
        /// Indicates if the view model is currently loading data
        /// </summary>
        [ObservableProperty]
        private bool _isLoading;

        /// <summary>
        /// Command to edit the schedule
        /// </summary>
        public ICommand EditScheduleCommand { get; }

        /// <summary>
        /// Command to generate a PDF report for this schedule
        /// </summary>
        public ICommand GenerateReportCommand { get; }

        /// <summary>
        /// Initializes a new instance of the ScheduleDetailsViewModel class
        /// </summary>
        /// <param name="schedule">The schedule to display details for</param>
        /// <param name="scheduleService">The schedule service</param>
        /// <param name="busService">The bus service</param>
        /// <param name="driverService">The driver service</param>
        /// <param name="serviceProvider">The service provider for dependency injection</param>
        /// <param name="logger">The logger instance</param>
        public ScheduleDetailsViewModel(Core.Models.Schedule schedule,
                                       IScheduleService scheduleService,
                                       IBusService busService,
                                       IDriverService driverService,
                                       IServiceProvider serviceProvider,
                                       ILogger<ScheduleDetailsViewModel> logger)
        {
            _scheduleService = scheduleService ?? throw new ArgumentNullException(nameof(scheduleService));
            _busService = busService ?? throw new ArgumentNullException(nameof(busService));
            _driverService = driverService ?? throw new ArgumentNullException(nameof(driverService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Initialize PDF report service
            _pdfReportService = new PdfReportService();

            // Try to get activity service, make it optional
            _activityService = serviceProvider.GetService<IActivityService>();

            Schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
            RecentActivities = new ObservableCollection<Core.Models.Activity>();

            // Initialize commands
            EditScheduleCommand = new AsyncRelayCommand(EditScheduleAsync);
            GenerateReportCommand = new AsyncRelayCommand(GenerateReportAsync);

            // Load initial data
            _ = LoadRecentActivitiesAsync();

            _logger.LogInformation("ScheduleDetailsViewModel initialized for schedule {ScheduleId}", schedule.ScheduleId);
        }        /// <summary>
                 /// Loads recent activities related to this schedule
                 /// </summary>
                 /// <returns>A task representing the asynchronous operation</returns>
        private async Task LoadRecentActivitiesAsync()
        {
            try
            {
                IsLoading = true;
                _logger.LogInformation("Loading recent activities for schedule {ScheduleId}", Schedule.ScheduleId);

                // If activity service is not available, skip loading activities
                if (_activityService == null)
                {
                    _logger.LogWarning("Activity service not available, skipping recent activities load");
                    return;
                }

                // Get all activities and filter by schedule ID if possible
                var allActivities = await _activityService.GetAllActivitiesAsync();
                var recentActivities = allActivities
                    .Where(a => a.Date >= DateTime.Now.AddDays(-30))
                    .OrderByDescending(a => a.Date)
                    .Take(10)
                    .ToList();

                RecentActivities.Clear();
                foreach (var activity in recentActivities)
                {
                    RecentActivities.Add(activity);
                }

                _logger.LogInformation("Loaded {Count} recent activities for schedule {ScheduleId}",
                    RecentActivities.Count, Schedule.ScheduleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading recent activities for schedule {ScheduleId}", Schedule.ScheduleId);
                // Keep the UI functional even if activities can't be loaded
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Handles the edit schedule command
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task EditScheduleAsync()
        {
            try
            {
                _logger.LogInformation("Edit schedule functionality would be implemented here for schedule {ScheduleId}", Schedule.ScheduleId);

                // For now, show a message that this feature is not yet implemented
                MessageBox.Show("Edit schedule functionality is not yet implemented in this demo.",
                              "Feature Coming Soon",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing schedule {ScheduleId}", Schedule.ScheduleId);
                MessageBox.Show($"Error editing schedule: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the generate PDF report command using existing PdfReportService
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task GenerateReportAsync()
        {
            try
            {
                _logger.LogInformation("Generating PDF report for schedule {ScheduleId}", Schedule.ScheduleId);

                // Create an activity representation of the schedule for PDF generation
                var scheduleActivity = new Core.Models.Activity
                {
                    ActivityId = Schedule.ScheduleId,
                    ActivityType = "Regular Route",
                    Description = $"Schedule for {Schedule.Route?.RouteName ?? "Unknown Route"}",
                    Date = Schedule.ScheduleDate,
                    LeaveTime = Schedule.DepartureTime.TimeOfDay,
                    ReturnTime = Schedule.ArrivalTime.TimeOfDay,
                    Status = Schedule.Status,
                    Notes = Schedule.Notes,
                    CreatedDate = Schedule.CreatedDate,
                    UpdatedDate = Schedule.UpdatedDate,
                    Driver = Schedule.Driver,
                    AssignedVehicle = Schedule.Bus,
                    Route = Schedule.Route
                };

                // Generate PDF report using existing service
                var pdfData = await Task.Run(() => _pdfReportService.GenerateActivityReport(scheduleActivity));

                // Save the PDF file
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    DefaultExt = "pdf",
                    FileName = $"Schedule_Report_{Schedule.ScheduleId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    await File.WriteAllBytesAsync(saveFileDialog.FileName, pdfData);
                    _logger.LogInformation("PDF report saved to {FilePath}", saveFileDialog.FileName);

                    // Ask user if they want to open the file
                    var result = MessageBox.Show($"PDF report generated successfully!\n\nWould you like to open the file now?",
                                               "Report Generated",
                                               MessageBoxButton.YesNo,
                                               MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = saveFileDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF report for schedule {ScheduleId}", Schedule.ScheduleId);
                MessageBox.Show($"Error generating PDF report: {ex.Message}",
                              "Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Disposes of the view model resources
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _logger.LogInformation("Disposing ScheduleDetailsViewModel for schedule {ScheduleId}", Schedule?.ScheduleId);

                // Clear collections
                RecentActivities?.Clear();

                // Mark as disposed
                _disposed = true;
            }
        }
    }
}
