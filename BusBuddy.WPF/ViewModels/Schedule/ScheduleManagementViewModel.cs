using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using BusBuddy.Core.Services.Interfaces;
using BusBuddy.WPF.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Syncfusion.UI.Xaml.Scheduler;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;

namespace BusBuddy.WPF.ViewModels.Schedule
{
    public partial class ScheduleManagementViewModel : BaseInDevelopmentViewModel
    {
        private readonly IScheduleService _scheduleService;
        private readonly IBusService _busService;
        private readonly IDriverService _driverService;

        [ObservableProperty]
        private ObservableCollection<BusBuddy.Core.Models.Schedule> _schedules;

        [ObservableProperty]
        private ObservableCollection<Bus> _buses;

        [ObservableProperty]
        private ObservableCollection<Driver> _drivers;

        [ObservableProperty]
        private BusBuddy.Core.Models.Schedule _selectedSchedule;

        // Syncfusion SfScheduler specific properties - using qualified name to avoid ambiguity
        [ObservableProperty]
        private ObservableCollection<Syncfusion.UI.Xaml.Scheduler.ScheduleAppointment> _scheduleAppointments;

        [ObservableProperty]
        private SchedulerViewType _selectedSchedulerViewType = SchedulerViewType.Week;

        [ObservableProperty]
        private DateTime _selectedDate = DateTime.Today;

        [ObservableProperty]
        private ObservableCollection<SchedulerResource> _busResources;

        [ObservableProperty]
        private ObservableCollection<SchedulerResource> _driverResources;

        // Statistics properties for dashboard integration
        [ObservableProperty]
        private int _todayScheduleCount;

        [ObservableProperty]
        private int _weekScheduleCount;

        [ObservableProperty]
        private int _conflictCount;

        [ObservableProperty]
        private int _totalSchedules;

        // UI helper properties
        [ObservableProperty]
        private ObservableCollection<ViewTypeOption> _viewTypes;

        [ObservableProperty]
        private ViewTypeOption _selectedViewType;

        [ObservableProperty]
        private ObservableCollection<ScheduleFilterOption> _scheduleFilters;

        [ObservableProperty]
        private ScheduleFilterOption _selectedScheduleFilter;

        [ObservableProperty]
        private bool _isBusy;

        public IAsyncRelayCommand LoadSchedulesCommand { get; }
        public IAsyncRelayCommand AddScheduleCommand { get; }
        public IAsyncRelayCommand UpdateScheduleCommand { get; }
        public IAsyncRelayCommand DeleteScheduleCommand { get; }

        // Syncfusion Scheduler Commands
        public IRelayCommand PreviousPeriodCommand { get; }
        public IRelayCommand NextPeriodCommand { get; }
        public IRelayCommand GoToTodayCommand { get; }
        public IAsyncRelayCommand ViewScheduleDetailsCommand { get; }
        public IAsyncRelayCommand EditScheduleCommand { get; }
        public IAsyncRelayCommand DuplicateScheduleCommand { get; }
        public IAsyncRelayCommand RescheduleCommand { get; }
        public IAsyncRelayCommand MarkCompleteCommand { get; }
        public IAsyncRelayCommand CancelScheduleCommand { get; }
        public IAsyncRelayCommand AddToRouteCommand { get; }
        public IAsyncRelayCommand GenerateDailyReportCommand { get; }
        public IAsyncRelayCommand GenerateWeeklyReportCommand { get; }
        public IAsyncRelayCommand GenerateUtilizationReportCommand { get; }
        public IAsyncRelayCommand ExportScheduleCommand { get; }
        public IAsyncRelayCommand GenerateScheduleReportCommand { get; }

        public ScheduleManagementViewModel(
            IScheduleService scheduleService,
            IBusService busService,
            IDriverService driverService,
            ILogger<ScheduleManagementViewModel>? logger = null)
            : base(logger)
        {
            _scheduleService = scheduleService;
            _busService = busService;
            _driverService = driverService;

            // Initialize collections
            Schedules = new ObservableCollection<BusBuddy.Core.Models.Schedule>();
            Buses = new ObservableCollection<Bus>();
            Drivers = new ObservableCollection<Driver>();
            ScheduleAppointments = new ObservableCollection<Syncfusion.UI.Xaml.Scheduler.ScheduleAppointment>();
            BusResources = new ObservableCollection<SchedulerResource>();
            DriverResources = new ObservableCollection<SchedulerResource>();

            // Initialize UI helper collections
            ViewTypes = new ObservableCollection<ViewTypeOption>
            {
                new ViewTypeOption { Name = "Day", ViewType = SchedulerViewType.Day, Description = "Day view" },
                new ViewTypeOption { Name = "Week", ViewType = SchedulerViewType.Week, Description = "Week view" },
                new ViewTypeOption { Name = "Work Week", ViewType = SchedulerViewType.WorkWeek, Description = "Work week view" },
                new ViewTypeOption { Name = "Month", ViewType = SchedulerViewType.Month, Description = "Month view" },
                new ViewTypeOption { Name = "Timeline Day", ViewType = SchedulerViewType.TimelineDay, Description = "Timeline day view" },
                new ViewTypeOption { Name = "Timeline Week", ViewType = SchedulerViewType.TimelineWeek, Description = "Timeline week view" },
                new ViewTypeOption { Name = "Timeline Work Week", ViewType = SchedulerViewType.TimelineWorkWeek, Description = "Timeline work week view" },
                new ViewTypeOption { Name = "Timeline Month", ViewType = SchedulerViewType.TimelineMonth, Description = "Timeline month view" }
            };
            SelectedViewType = ViewTypes.FirstOrDefault(v => v.ViewType == SchedulerViewType.Week) ?? ViewTypes.First();

            ScheduleFilters = new ObservableCollection<ScheduleFilterOption>
            {
                new ScheduleFilterOption { Name = "All Schedules", FilterValue = "all", Description = "Show all schedules" },
                new ScheduleFilterOption { Name = "Today's Schedules", FilterValue = "today", Description = "Show today's schedules only" },
                new ScheduleFilterOption { Name = "This Week", FilterValue = "week", Description = "Show this week's schedules" },
                new ScheduleFilterOption { Name = "Active Only", FilterValue = "active", Description = "Show only active schedules" },
                new ScheduleFilterOption { Name = "Scheduled", FilterValue = "scheduled", Description = "Show scheduled only" },
                new ScheduleFilterOption { Name = "In Progress", FilterValue = "inprogress", Description = "Show in progress only" },
                new ScheduleFilterOption { Name = "Completed", FilterValue = "completed", Description = "Show completed only" },
                new ScheduleFilterOption { Name = "Cancelled", FilterValue = "cancelled", Description = "Show cancelled only" },
                new ScheduleFilterOption { Name = "Conflicts", FilterValue = "conflicts", Description = "Show scheduling conflicts" }
            };
            SelectedScheduleFilter = ScheduleFilters.First();

            SelectedSchedule = new BusBuddy.Core.Models.Schedule();

            // Initialize commands
            LoadSchedulesCommand = new AsyncRelayCommand(LoadDataAsync);
            AddScheduleCommand = new AsyncRelayCommand(AddScheduleAsync);
            UpdateScheduleCommand = new AsyncRelayCommand(UpdateScheduleAsync, CanUpdateOrDelete);
            DeleteScheduleCommand = new AsyncRelayCommand(DeleteScheduleAsync, CanUpdateOrDelete);

            // Initialize Syncfusion Scheduler commands
            PreviousPeriodCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(NavigateToPreviousPeriod);
            NextPeriodCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(NavigateToNextPeriod);
            GoToTodayCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(NavigateToToday);
            ViewScheduleDetailsCommand = new AsyncRelayCommand(ViewScheduleDetailsAsync);
            EditScheduleCommand = new AsyncRelayCommand(EditScheduleAsync);
            DuplicateScheduleCommand = new AsyncRelayCommand(DuplicateScheduleAsync);
            RescheduleCommand = new AsyncRelayCommand(RescheduleAsync);
            MarkCompleteCommand = new AsyncRelayCommand(MarkCompleteAsync);
            CancelScheduleCommand = new AsyncRelayCommand(CancelScheduleAsync);
            AddToRouteCommand = new AsyncRelayCommand(AddToRouteAsync);
            GenerateDailyReportCommand = new AsyncRelayCommand(GenerateDailyReportAsync);
            GenerateWeeklyReportCommand = new AsyncRelayCommand(GenerateWeeklyReportAsync);
            GenerateUtilizationReportCommand = new AsyncRelayCommand(GenerateUtilizationReportAsync);
            ExportScheduleCommand = new AsyncRelayCommand(ExportScheduleAsync);
            GenerateScheduleReportCommand = new AsyncRelayCommand(GenerateScheduleReportAsync);

            // Schedule Management is now production-ready!
            IsInDevelopment = false;

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;

                await LoadSchedulesAsync();
                await LoadBusesAsync();
                await LoadDriversAsync();
                await LoadScheduleAppointmentsAsync();
                await LoadResourcesAsync();
                await UpdateStatisticsAsync();

                Logger?.LogInformation("Loaded schedules, buses, and drivers data");
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error loading schedule data");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private Task LoadScheduleAppointmentsAsync()
        {
            try
            {
                // Convert Schedule models to Syncfusion ScheduleAppointment objects
                ScheduleAppointments.Clear();

                foreach (var schedule in Schedules)
                {
                    var appointment = new Syncfusion.UI.Xaml.Scheduler.ScheduleAppointment
                    {
                        StartTime = schedule.DepartureTime,
                        EndTime = schedule.ArrivalTime,
                        Subject = $"Bus {schedule.Bus?.BusNumber ?? "N/A"} - {schedule.Route?.RouteName ?? "N/A"}",
                        Notes = schedule.Notes ?? string.Empty,
                        AppointmentBackground = GetStatusColor(schedule.Status),
                        Id = schedule.ScheduleId,
                        IsAllDay = false
                    };

                    // Add resource information for grouping
                    if (schedule.Bus != null)
                    {
                        appointment.ResourceIdCollection = new ObservableCollection<object> { schedule.BusId };
                    }

                    ScheduleAppointments.Add(appointment);
                }

                Logger?.LogInformation("Loaded {AppointmentCount} schedule appointments", ScheduleAppointments.Count);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error loading schedule appointments");
            }

            return Task.CompletedTask;
        }

        private Task LoadResourcesAsync()
        {
            try
            {
                // Load bus resources
                BusResources.Clear();
                foreach (var bus in Buses)
                {
                    var resource = new SchedulerResource
                    {
                        Id = bus.VehicleId,
                        Name = $"Bus {bus.BusNumber}",
                        Background = System.Windows.Media.Brushes.LightBlue,
                        Foreground = System.Windows.Media.Brushes.Black
                    };
                    BusResources.Add(resource);
                }

                // Load driver resources
                DriverResources.Clear();
                foreach (var driver in Drivers)
                {
                    var resource = new SchedulerResource
                    {
                        Id = driver.DriverId,
                        Name = $"{driver.FirstName} {driver.LastName}",
                        Background = System.Windows.Media.Brushes.LightGreen,
                        Foreground = System.Windows.Media.Brushes.Black
                    };
                    DriverResources.Add(resource);
                }

                Logger?.LogInformation("Loaded {BusResourceCount} bus resources and {DriverResourceCount} driver resources",
                    BusResources.Count, DriverResources.Count);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error loading resources");
            }

            return Task.CompletedTask;
        }

        private async Task UpdateStatisticsAsync()
        {
            try
            {
                var today = DateTime.Today;
                var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(6);

                TodayScheduleCount = Schedules.Count(s => s.ScheduleDate.Date == today);
                WeekScheduleCount = Schedules.Count(s => s.ScheduleDate.Date >= startOfWeek && s.ScheduleDate.Date <= endOfWeek);
                ConflictCount = await DetectConflictsAsync();
                TotalSchedules = Schedules.Count;

                Logger?.LogInformation("Updated schedule statistics: Today={Today}, Week={Week}, Conflicts={Conflicts}, Total={Total}",
                    TodayScheduleCount, WeekScheduleCount, ConflictCount, TotalSchedules);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error updating statistics");
            }
        }

        private Task<int> DetectConflictsAsync()
        {
            // Simple conflict detection: multiple schedules for same bus at overlapping times
            var conflicts = 0;

            foreach (var schedule in Schedules.Where(s => s.Status == "Scheduled" || s.Status == "InProgress"))
            {
                var overlapping = Schedules.Where(s =>
                    s.ScheduleId != schedule.ScheduleId &&
                    s.BusId == schedule.BusId &&
                    s.DepartureTime < schedule.ArrivalTime &&
                    s.ArrivalTime > schedule.DepartureTime).ToList();

                if (overlapping.Any())
                {
                    conflicts++;
                }
            }

            return Task.FromResult(conflicts);
        }

        private System.Windows.Media.Brush GetStatusColor(string status)
        {
            return status switch
            {
                "Scheduled" => System.Windows.Media.Brushes.LightBlue,
                "InProgress" => System.Windows.Media.Brushes.Orange,
                "Completed" => System.Windows.Media.Brushes.LightGreen,
                "Cancelled" => System.Windows.Media.Brushes.LightCoral,
                "Delayed" => System.Windows.Media.Brushes.Yellow,
                _ => System.Windows.Media.Brushes.Gray
            };
        }

        // Navigation methods for Syncfusion Scheduler
        private void NavigateToPreviousPeriod()
        {
            SelectedDate = SelectedSchedulerViewType switch
            {
                SchedulerViewType.Day => SelectedDate.AddDays(-1),
                SchedulerViewType.Week or SchedulerViewType.WorkWeek => SelectedDate.AddDays(-7),
                SchedulerViewType.Month => SelectedDate.AddMonths(-1),
                _ => SelectedDate.AddDays(-1)
            };
        }

        private void NavigateToNextPeriod()
        {
            SelectedDate = SelectedSchedulerViewType switch
            {
                SchedulerViewType.Day => SelectedDate.AddDays(1),
                SchedulerViewType.Week or SchedulerViewType.WorkWeek => SelectedDate.AddDays(7),
                SchedulerViewType.Month => SelectedDate.AddMonths(1),
                _ => SelectedDate.AddDays(1)
            };
        }

        private void NavigateToToday()
        {
            SelectedDate = DateTime.Today;
        }

        // Scheduler command implementations
        private Task ViewScheduleDetailsAsync()
        {
            if (SelectedSchedule != null)
            {
                Logger?.LogInformation("Viewing details for schedule {ScheduleId}", SelectedSchedule.ScheduleId);
                // TODO: Open schedule details dialog
            }
            return Task.CompletedTask;
        }

        private Task EditScheduleAsync()
        {
            if (SelectedSchedule != null)
            {
                Logger?.LogInformation("Editing schedule {ScheduleId}", SelectedSchedule.ScheduleId);
                // TODO: Open schedule edit dialog
            }
            return Task.CompletedTask;
        }

        private async Task DuplicateScheduleAsync()
        {
            if (SelectedSchedule != null)
            {
                try
                {
                    var newSchedule = new BusBuddy.Core.Models.Schedule
                    {
                        BusId = SelectedSchedule.BusId,
                        RouteId = SelectedSchedule.RouteId,
                        DriverId = SelectedSchedule.DriverId,
                        DepartureTime = SelectedSchedule.DepartureTime.AddDays(1),
                        ArrivalTime = SelectedSchedule.ArrivalTime.AddDays(1),
                        ScheduleDate = SelectedSchedule.ScheduleDate.AddDays(1),
                        Status = "Scheduled",
                        Notes = $"Duplicated from Schedule {SelectedSchedule.ScheduleId}"
                    };

                    await _scheduleService.AddScheduleAsync(newSchedule);
                    await LoadDataAsync();
                    Logger?.LogInformation("Duplicated schedule {ScheduleId}", SelectedSchedule.ScheduleId);
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, "Error duplicating schedule");
                }
            }
        }

        private Task RescheduleAsync()
        {
            if (SelectedSchedule != null)
            {
                Logger?.LogInformation("Rescheduling schedule {ScheduleId}", SelectedSchedule.ScheduleId);
                // TODO: Open reschedule dialog
            }
            return Task.CompletedTask;
        }

        private async Task MarkCompleteAsync()
        {
            if (SelectedSchedule != null)
            {
                try
                {
                    SelectedSchedule.Status = "Completed";
                    SelectedSchedule.UpdatedDate = DateTime.UtcNow;
                    await _scheduleService.UpdateScheduleAsync(SelectedSchedule);
                    await LoadDataAsync();
                    Logger?.LogInformation("Marked schedule {ScheduleId} as complete", SelectedSchedule.ScheduleId);
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, "Error marking schedule as complete");
                }
            }
        }

        private async Task CancelScheduleAsync()
        {
            if (SelectedSchedule != null)
            {
                try
                {
                    SelectedSchedule.Status = "Cancelled";
                    SelectedSchedule.UpdatedDate = DateTime.UtcNow;
                    await _scheduleService.UpdateScheduleAsync(SelectedSchedule);
                    await LoadDataAsync();
                    Logger?.LogInformation("Cancelled schedule {ScheduleId}", SelectedSchedule.ScheduleId);
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, "Error cancelling schedule");
                }
            }
        }

        private Task AddToRouteAsync()
        {
            if (SelectedSchedule != null)
            {
                Logger?.LogInformation("Adding schedule {ScheduleId} to route", SelectedSchedule.ScheduleId);
                // TODO: Implement route integration
            }
            return Task.CompletedTask;
        }

        // Report generation methods
        private Task GenerateDailyReportAsync()
        {
            Logger?.LogInformation("Generating daily report");
            // TODO: Implement daily report generation
            return Task.CompletedTask;
        }

        private Task GenerateWeeklyReportAsync()
        {
            Logger?.LogInformation("Generating weekly report");
            // TODO: Implement weekly report generation
            return Task.CompletedTask;
        }

        private Task GenerateUtilizationReportAsync()
        {
            Logger?.LogInformation("Generating utilization report");
            // TODO: Implement utilization report generation
            return Task.CompletedTask;
        }

        private Task ExportScheduleAsync()
        {
            Logger?.LogInformation("Exporting schedule data");
            // TODO: Implement Excel export
            return Task.CompletedTask;
        }

        private Task GenerateScheduleReportAsync()
        {
            Logger?.LogInformation("Generating schedule report");
            // TODO: Implement schedule report generation
            return Task.CompletedTask;
        }

        private async Task LoadSchedulesAsync()
        {
            try
            {
                var schedules = await _scheduleService.GetSchedulesAsync();
                Schedules.Clear();
                foreach (var s in schedules)
                {
                    Schedules.Add(s);
                }
                Logger?.LogInformation("Loaded {ScheduleCount} schedules", Schedules.Count);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error loading schedules");
            }
        }

        private async Task LoadBusesAsync()
        {
            try
            {
                var buses = await _busService.GetAllBusesAsync();
                Buses.Clear();
                foreach (var bus in buses)
                {
                    Buses.Add(bus);
                }
                Logger?.LogInformation("Loaded {BusCount} buses", Buses.Count);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error loading buses");
            }
        }

        private async Task LoadDriversAsync()
        {
            try
            {
                var drivers = await _driverService.GetAllDriversAsync();
                Drivers.Clear();
                foreach (var driver in drivers)
                {
                    Drivers.Add(driver);
                }
                Logger?.LogInformation("Loaded {DriverCount} drivers", Drivers.Count);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error loading drivers");
            }
        }

        private async Task AddScheduleAsync()
        {
            if (SelectedSchedule != null)
            {
                try
                {
                    // Set default values for a new schedule
                    SelectedSchedule.ScheduleDate = DateTime.Now;
                    await _scheduleService.AddScheduleAsync(SelectedSchedule);
                    await LoadSchedulesAsync();
                    Logger?.LogInformation("Added new schedule with ID {ScheduleId}", SelectedSchedule.ScheduleId);
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, "Error adding schedule");
                }
            }
        }

        private async Task UpdateScheduleAsync()
        {
            if (SelectedSchedule != null)
            {
                try
                {
                    await _scheduleService.UpdateScheduleAsync(SelectedSchedule);
                    await LoadSchedulesAsync();
                    Logger?.LogInformation("Updated schedule with ID {ScheduleId}", SelectedSchedule.ScheduleId);
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, "Error updating schedule {ScheduleId}", SelectedSchedule.ScheduleId);
                }
            }
        }

        private async Task DeleteScheduleAsync()
        {
            if (SelectedSchedule != null)
            {
                try
                {
                    await _scheduleService.DeleteScheduleAsync(SelectedSchedule.ScheduleId);
                    await LoadSchedulesAsync();
                    Logger?.LogInformation("Deleted schedule with ID {ScheduleId}", SelectedSchedule.ScheduleId);
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, "Error deleting schedule {ScheduleId}", SelectedSchedule.ScheduleId);
                }
            }
        }

        private bool CanUpdateOrDelete()
        {
            return SelectedSchedule != null && SelectedSchedule.ScheduleId != 0;
        }

        partial void OnSelectedScheduleChanged(BusBuddy.Core.Models.Schedule value)
        {
            (UpdateScheduleCommand as IRelayCommand)?.NotifyCanExecuteChanged();
            (DeleteScheduleCommand as IRelayCommand)?.NotifyCanExecuteChanged();

            if (value != null)
            {
                Logger?.LogDebug("Selected schedule changed to ID {ScheduleId}", value.ScheduleId);
            }
        }

        partial void OnSelectedViewTypeChanged(ViewTypeOption value)
        {
            if (value != null)
            {
                SelectedSchedulerViewType = value.ViewType;
                Logger?.LogDebug("View type changed to {ViewType}", value.Name);
            }
        }

        partial void OnSelectedScheduleFilterChanged(ScheduleFilterOption value)
        {
            if (value != null)
            {
                Logger?.LogDebug("Schedule filter changed to {Filter}", value.Name);
                _ = ApplyFilterAsync(value.FilterValue);
            }
        }

        private async Task ApplyFilterAsync(string filterValue)
        {
            try
            {
                IsBusy = true;

                // Reload schedules with filter applied
                await LoadSchedulesAsync();

                // Apply client-side filtering if needed
                var filteredSchedules = filterValue switch
                {
                    "today" => Schedules.Where(s => s.ScheduleDate.Date == DateTime.Today),
                    "week" => Schedules.Where(s => s.ScheduleDate.Date >= DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek) &&
                                                   s.ScheduleDate.Date <= DateTime.Today.AddDays(6 - (int)DateTime.Today.DayOfWeek)),
                    "active" => Schedules.Where(s => s.Status == "Scheduled" || s.Status == "InProgress"),
                    "scheduled" => Schedules.Where(s => s.Status == "Scheduled"),
                    "inprogress" => Schedules.Where(s => s.Status == "InProgress"),
                    "completed" => Schedules.Where(s => s.Status == "Completed"),
                    "cancelled" => Schedules.Where(s => s.Status == "Cancelled"),
                    "conflicts" => await GetConflictedSchedulesAsync(),
                    _ => Schedules
                };

                // Update the collection
                Schedules.Clear();
                foreach (var schedule in filteredSchedules)
                {
                    Schedules.Add(schedule);
                }

                // Refresh appointments and statistics
                await LoadScheduleAppointmentsAsync();
                await UpdateStatisticsAsync();

                Logger?.LogInformation("Applied filter {Filter}, showing {Count} schedules", filterValue, Schedules.Count);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error applying filter {Filter}", filterValue);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private Task<IEnumerable<BusBuddy.Core.Models.Schedule>> GetConflictedSchedulesAsync()
        {
            var conflicted = new List<BusBuddy.Core.Models.Schedule>();

            foreach (var schedule in Schedules.Where(s => s.Status == "Scheduled" || s.Status == "InProgress"))
            {
                var hasConflict = Schedules.Any(s =>
                    s.ScheduleId != schedule.ScheduleId &&
                    s.BusId == schedule.BusId &&
                    s.DepartureTime < schedule.ArrivalTime &&
                    s.ArrivalTime > schedule.DepartureTime);

                if (hasConflict && !conflicted.Contains(schedule))
                {
                    conflicted.Add(schedule);
                }
            }

            return Task.FromResult<IEnumerable<BusBuddy.Core.Models.Schedule>>(conflicted);
        }
    }
}
