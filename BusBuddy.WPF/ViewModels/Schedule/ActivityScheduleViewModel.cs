using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using CoreInterfaces = BusBuddy.Core.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

// Disable async method without await operator warnings
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace BusBuddy.WPF.ViewModels.ScheduleManagement
{
    public class ActivityScheduleViewModel : ObservableObject
    {
        private readonly CoreInterfaces.IActivityScheduleService _activityScheduleService;
        private readonly IDriverService _driverService;
        private readonly CoreInterfaces.IBusService _busService;

        public ObservableCollection<ActivitySchedule> ActivitySchedules { get; } = new();

        private ActivitySchedule? _selectedActivitySchedule;
        public ActivitySchedule? SelectedActivitySchedule
        {
            get => _selectedActivitySchedule;
            set => SetProperty(ref _selectedActivitySchedule, value);
        }

        private DateTime _startDate = DateTime.Today.AddDays(-7);
        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        private DateTime _endDate = DateTime.Today.AddDays(30);
        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        private string _filterTripType = string.Empty;
        public string FilterTripType
        {
            get => _filterTripType;
            set => SetProperty(ref _filterTripType, value);
        }

        private string _filterStatus = string.Empty;
        public string FilterStatus
        {
            get => _filterStatus;
            set => SetProperty(ref _filterStatus, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        // Commands
        public ICommand LoadActivitySchedulesCommand { get; }
        public ICommand AddActivityScheduleCommand { get; }
        public ICommand EditActivityScheduleCommand { get; }
        public ICommand DeleteActivityScheduleCommand { get; }
        public ICommand ConfirmActivityScheduleCommand { get; }
        public ICommand CancelActivityScheduleCommand { get; }
        public ICommand CompleteActivityScheduleCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand ExportToCsvCommand { get; }
        public ICommand ShowReportsCommand { get; }

        public ActivityScheduleViewModel(
            CoreInterfaces.IActivityScheduleService activityScheduleService,
            IDriverService driverService,
            CoreInterfaces.IBusService busService)
        {
            _activityScheduleService = activityScheduleService;
            _driverService = driverService;
            _busService = busService;

            LoadActivitySchedulesCommand = new AsyncRelayCommand(LoadActivitySchedulesAsync);
            AddActivityScheduleCommand = new AsyncRelayCommand(AddActivityScheduleAsync);
            EditActivityScheduleCommand = new AsyncRelayCommand(EditActivityScheduleAsync, CanEditOrDeleteActivitySchedule);
            DeleteActivityScheduleCommand = new AsyncRelayCommand(DeleteActivityScheduleAsync, CanEditOrDeleteActivitySchedule);
            ConfirmActivityScheduleCommand = new AsyncRelayCommand(ConfirmActivityScheduleAsync, CanConfirmActivitySchedule);
            CancelActivityScheduleCommand = new AsyncRelayCommand(CancelActivityScheduleAsync, CanCancelOrCompleteActivitySchedule);
            CompleteActivityScheduleCommand = new AsyncRelayCommand(CompleteActivityScheduleAsync, CanCancelOrCompleteActivitySchedule);
            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            FilterCommand = new AsyncRelayCommand(ApplyFiltersAsync);
            ExportToCsvCommand = new AsyncRelayCommand(ExportToCsvAsync);
            ShowReportsCommand = new RelayCommand(ExecuteShowReportsDialog);

            // Load schedules when view model is created
            _ = LoadActivitySchedulesAsync();
        }

        private async Task LoadActivitySchedulesAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading activity schedules...";

                ActivitySchedules.Clear();
                var schedules = await _activityScheduleService.GetActivitySchedulesByDateRangeAsync(StartDate, EndDate);

                // Apply additional filters if they are set
                if (!string.IsNullOrEmpty(FilterTripType))
                {
                    schedules = schedules.Where(s => s.TripType == FilterTripType);
                }

                if (!string.IsNullOrEmpty(FilterStatus))
                {
                    schedules = schedules.Where(s => s.Status == FilterStatus);
                }

                foreach (var schedule in schedules.OrderBy(s => s.ScheduledDate).ThenBy(s => s.ScheduledLeaveTime))
                {
                    ActivitySchedules.Add(schedule);
                }

                StatusMessage = $"Loaded {ActivitySchedules.Count} activity schedules.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading activity schedules: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AddActivityScheduleAsync()
        {
            try
            {
                // Show dialog to collect information for the new activity schedule
                var dialog = new Views.Schedule.ActivityScheduleDialog(
                    _activityScheduleService,
                    _driverService,
                    _busService);

                var result = dialog.ShowDialog();

                if (result == true)
                {
                    var createdSchedule = dialog.Schedule;
                    ActivitySchedules.Add(createdSchedule);
                    SelectedActivitySchedule = createdSchedule;
                    StatusMessage = $"Created new activity schedule for {createdSchedule.ScheduledDate:d}.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error adding activity schedule: {ex.Message}";
            }
        }

        private async Task EditActivityScheduleAsync()
        {
            if (SelectedActivitySchedule == null)
                return;

            try
            {
                // Show dialog to edit the selected schedule
                var dialog = new Views.Schedule.ActivityScheduleDialog(
                    _activityScheduleService,
                    _driverService,
                    _busService,
                    SelectedActivitySchedule);

                var result = dialog.ShowDialog();

                if (result == true)
                {
                    var updatedSchedule = dialog.Schedule;

                    // Replace the item in the collection to refresh the UI
                    var index = ActivitySchedules.IndexOf(SelectedActivitySchedule);
                    if (index >= 0)
                    {
                        ActivitySchedules[index] = updatedSchedule;
                        SelectedActivitySchedule = updatedSchedule;
                    }

                    StatusMessage = $"Updated activity schedule for {updatedSchedule.ScheduledDate:d}.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating activity schedule: {ex.Message}";
            }
        }

        private async Task DeleteActivityScheduleAsync()
        {
            if (SelectedActivitySchedule == null)
                return;

            try
            {
                var scheduleToDelete = SelectedActivitySchedule;

                // Confirm deletion with the user
                var result = System.Windows.MessageBox.Show(
                    $"Are you sure you want to delete the activity schedule for {scheduleToDelete.ScheduledDate:d} to {scheduleToDelete.ScheduledDestination}?",
                    "Confirm Delete",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    var deleted = await _activityScheduleService.DeleteActivityScheduleAsync(scheduleToDelete.ActivityScheduleId);

                    if (deleted)
                    {
                        ActivitySchedules.Remove(scheduleToDelete);
                        SelectedActivitySchedule = null;
                        StatusMessage = "Activity schedule deleted successfully.";
                    }
                    else
                    {
                        StatusMessage = "Could not delete activity schedule.";
                    }
                }
                else
                {
                    StatusMessage = "Delete operation cancelled.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting activity schedule: {ex.Message}";
            }
        }

        private async Task ConfirmActivityScheduleAsync()
        {
            if (SelectedActivitySchedule == null)
                return;

            try
            {
                var confirmed = await _activityScheduleService.ConfirmActivityScheduleAsync(SelectedActivitySchedule.ActivityScheduleId);

                if (confirmed)
                {
                    // Refresh the item in the collection
                    await RefreshSelectedItemAsync();
                    StatusMessage = "Activity schedule confirmed successfully.";
                }
                else
                {
                    StatusMessage = "Could not confirm activity schedule.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error confirming activity schedule: {ex.Message}";
            }
        }

        private async Task CancelActivityScheduleAsync()
        {
            if (SelectedActivitySchedule == null)
                return;

            try
            {
                // Prompt for a reason
                var reasonDialog = new Views.Schedule.CancellationReasonDialog();
                var dialogResult = reasonDialog.ShowDialog();

                if (dialogResult != true)
                {
                    StatusMessage = "Cancellation aborted.";
                    return;
                }

                string reason = reasonDialog.CancellationReason;

                var cancelled = await _activityScheduleService.CancelActivityScheduleAsync(SelectedActivitySchedule.ActivityScheduleId, reason);

                if (cancelled)
                {
                    // Refresh the item in the collection
                    await RefreshSelectedItemAsync();
                    StatusMessage = "Activity schedule cancelled successfully.";
                }
                else
                {
                    StatusMessage = "Could not cancel activity schedule.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error cancelling activity schedule: {ex.Message}";
            }
        }

        private async Task CompleteActivityScheduleAsync()
        {
            if (SelectedActivitySchedule == null)
                return;

            try
            {
                var completed = await _activityScheduleService.CompleteActivityScheduleAsync(SelectedActivitySchedule.ActivityScheduleId);

                if (completed)
                {
                    // Refresh the item in the collection
                    await RefreshSelectedItemAsync();
                    StatusMessage = "Activity schedule marked as completed.";
                }
                else
                {
                    StatusMessage = "Could not complete activity schedule.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error completing activity schedule: {ex.Message}";
            }
        }

        private async Task RefreshAsync()
        {
            await LoadActivitySchedulesAsync();
        }

        private async Task ApplyFiltersAsync()
        {
            await LoadActivitySchedulesAsync();
        }

        private async Task ExportToCsvAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Exporting activity schedules to CSV...";

                var csvContent = await _activityScheduleService.ExportActivitySchedulesToCsvAsync(StartDate, EndDate);

                // Create a save file dialog
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv",
                    DefaultExt = ".csv",
                    FileName = $"ActivitySchedules_{StartDate:yyyyMMdd}-{EndDate:yyyyMMdd}.csv"
                };

                if (dialog.ShowDialog() == true)
                {
                    // Save the CSV content to the selected file
                    await System.IO.File.WriteAllTextAsync(dialog.FileName, csvContent);
                    StatusMessage = $"Exported {csvContent.Split('\n').Length - 1} activity schedules to {dialog.FileName}.";
                }
                else
                {
                    StatusMessage = "Export cancelled.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error exporting to CSV: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshSelectedItemAsync()
        {
            if (SelectedActivitySchedule != null)
            {
                var refreshedItem = await _activityScheduleService.GetActivityScheduleByIdAsync(SelectedActivitySchedule.ActivityScheduleId);
                if (refreshedItem != null)
                {
                    var index = ActivitySchedules.IndexOf(SelectedActivitySchedule);
                    if (index >= 0)
                    {
                        ActivitySchedules[index] = refreshedItem;
                        SelectedActivitySchedule = refreshedItem;
                    }
                }
            }
        }

        private bool CanEditOrDeleteActivitySchedule()
        {
            return SelectedActivitySchedule != null;
        }

        private bool CanConfirmActivitySchedule()
        {
            return SelectedActivitySchedule != null &&
                   SelectedActivitySchedule.Status == "Scheduled";
        }

        private bool CanCancelOrCompleteActivitySchedule()
        {
            return SelectedActivitySchedule != null &&
                   (SelectedActivitySchedule.Status == "Scheduled" ||
                    SelectedActivitySchedule.Status == "Confirmed");
        }

        private void ShowReportsDialog()
        {
            try
            {
                var reportDialog = new Views.Schedule.ActivityScheduleReportDialog(_activityScheduleService);
                reportDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error showing reports: {ex.Message}";
            }
        }

        private void ExecuteShowReportsDialog(object? parameter)
        {
            ShowReportsDialog();
        }
    }
}
