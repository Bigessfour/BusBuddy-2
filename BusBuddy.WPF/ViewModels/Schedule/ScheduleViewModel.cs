using BusBuddy.Core.Models;
using BusBuddy.Core.Services.Interfaces;
using BusBuddy.Core.Services;
using BusBuddy.WPF.Views.Schedule;
using BusBuddy.WPF.Models;
using BusBuddy.WPF.ViewModels.Schedule;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Serilog;
using Serilog.Context;
using System.Windows;

namespace BusBuddy.WPF.ViewModels.ScheduleManagement
{
    public class ScheduleViewModel : ObservableObject
    {
        private static readonly ILogger Logger = Log.ForContext<ScheduleViewModel>();
        private readonly IScheduleService _scheduleService;
        private readonly IRouteService _routeService;
        private readonly IBusService _busService;
        private readonly IDriverService _driverService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        // Collections
        public ObservableCollection<BusBuddy.Core.Models.Schedule> Schedules { get; } = new();
        public ObservableCollection<BusBuddy.Core.Models.Schedule> FilteredSchedules { get; } = new();
        private ObservableCollection<BusBuddy.Core.Models.Schedule> _allSchedules = new();

        // Sports Categories for filtering
        public ObservableCollection<string> SportsCategories { get; } = new()
        {
            "All",
            "Volleyball",
            "Junior High Volleyball",
            "Football",
            "Junior High Football",
            "Softball",
            "Cheer",
            "Activity",
            "Sports", // Special category for all sports
            "Routes"  // Special category for regular routes
        };

        private string _selectedCategory = "All";
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    using (LogContext.PushProperty("SelectedCategory", value))
                    {
                        Logger.Information("Sports category filter changed to: {Category}", value);
                        _ = FilterSchedulesByCategoryAsync();
                    }
                }
            }
        }

        private BusBuddy.Core.Models.Schedule? _selectedSchedule;
        public BusBuddy.Core.Models.Schedule? SelectedSchedule
        {
            get => _selectedSchedule;
            set
            {
                if (SetProperty(ref _selectedSchedule, value))
                {
                    using (LogContext.PushProperty("ScheduleId", value?.ScheduleId))
                    {
                        Logger.Debug("Schedule selection changed. SportsCategory: {SportsCategory}, Opponent: {Opponent}",
                            value?.SportsCategory, value?.Opponent);
                        OnPropertyChanged(nameof(CanEditSchedule));
                        OnPropertyChanged(nameof(CanDeleteSchedule));
                    }
                }
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private bool _isFiltering;
        public bool IsFiltering
        {
            get => _isFiltering;
            set => SetProperty(ref _isFiltering, value);
        }

        // Statistics for dashboard display
        private int _totalSchedules;
        public int TotalSchedules
        {
            get => _totalSchedules;
            set => SetProperty(ref _totalSchedules, value);
        }

        private int _sportsTrips;
        public int SportsTrips
        {
            get => _sportsTrips;
            set => SetProperty(ref _sportsTrips, value);
        }

        private int _regularRoutes;
        public int RegularRoutes
        {
            get => _regularRoutes;
            set => SetProperty(ref _regularRoutes, value);
        }

        public bool CanEditSchedule => SelectedSchedule != null;
        public bool CanDeleteSchedule => SelectedSchedule != null;

        public ICommand LoadSchedulesCommand { get; }
        public ICommand AddScheduleCommand { get; }
        public ICommand EditScheduleCommand { get; }
        public ICommand DeleteScheduleCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand FilterCommand { get; }

        public ScheduleViewModel(
            IScheduleService scheduleService,
            IRouteService routeService,
            IBusService busService,
            IDriverService driverService,
            IServiceProvider serviceProvider)
        {
            _scheduleService = scheduleService;
            _routeService = routeService;
            _busService = busService;
            _driverService = driverService;
            _serviceProvider = serviceProvider;
            _logger = Log.ForContext<ScheduleViewModel>()
                .ForContext("Module", "ScheduleManagement")
                .ForContext("Component", "ScheduleViewModel");

            LoadSchedulesCommand = new AsyncRelayCommand(LoadSchedulesAsync);
            AddScheduleCommand = new AsyncRelayCommand(AddScheduleAsync);
            EditScheduleCommand = new AsyncRelayCommand(EditScheduleAsync, () => CanEditSchedule);
            DeleteScheduleCommand = new AsyncRelayCommand(DeleteScheduleAsync, () => CanDeleteSchedule);
            RefreshCommand = new AsyncRelayCommand(LoadSchedulesAsync);
            FilterCommand = new AsyncRelayCommand(FilterSchedulesByCategoryAsync);

            Logger.Information("ScheduleViewModel initialized with sports category filtering support");
        }
        private async Task LoadSchedulesAsync()
        {
            using (LogContext.PushProperty("Operation", "LoadSchedulesAsync"))
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                Logger.Information("Starting to load schedules");

                try
                {
                    IsLoading = true;
                    ErrorMessage = string.Empty;

                    Logger.Information("Loading schedules from service...");

                    Schedules.Clear();
                    FilteredSchedules.Clear();
                    _allSchedules.Clear();

                    var schedules = await _scheduleService.GetSchedulesAsync();
                    var scheduleList = schedules.ToList();

                    foreach (var schedule in scheduleList)
                    {
                        Schedules.Add(schedule);
                        _allSchedules.Add(schedule);
                    }

                    UpdateStatistics(scheduleList);

                    // Apply current filter
                    await FilterSchedulesByCategoryAsync();

                    stopwatch.Stop();
                    Logger.Information("Successfully loaded {ScheduleCount} schedules in {ElapsedMs}ms. Sports trips: {SportsTrips}, Regular routes: {RegularRoutes}",
                        scheduleList.Count, stopwatch.ElapsedMilliseconds, SportsTrips, RegularRoutes);
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    Logger.Error(ex, "Failed to load schedules after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                    ErrorMessage = "Failed to load schedules. Please try again.";
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async Task AddScheduleAsync()
        {
            _logger.Information("Starting to add new schedule");

            try
            {
                _logger.Information("Opening add schedule dialog");

                var viewModel = new AddEditScheduleViewModel(
                    _scheduleService,
                    _routeService,
                    _busService,
                    _driverService);

                var dialog = new AddEditScheduleDialog
                {
                    DataContext = viewModel,
                    Owner = Application.Current.MainWindow
                };

                // Load data first
                if (viewModel.LoadDataCommand is AsyncRelayCommand loadCommand)
                {
                    await loadCommand.ExecuteAsync(null);
                }

                var result = dialog.ShowDialog();

                if (result == true)
                {
                    _logger.Information("Schedule added successfully, refreshing list");
                    await LoadSchedulesAsync();
                }
                else
                {
                    _logger.Debug("Add schedule dialog cancelled");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to add schedule");
                ErrorMessage = "Failed to add schedule. Please try again.";
            }
        }

        private async Task EditScheduleAsync()
        {
            if (SelectedSchedule == null)
            {
                _logger.Warning("Edit schedule attempted with no schedule selected");
                return;
            }

            _logger.Information("Opening edit schedule dialog for schedule {ScheduleId}", SelectedSchedule.ScheduleId);

            try
            {
                _logger.Information("Opening edit schedule dialog for schedule {ScheduleId}", SelectedSchedule.ScheduleId);

                var viewModel = new AddEditScheduleViewModel(
                    _scheduleService,
                    _routeService,
                    _busService,
                    _driverService,
                    SelectedSchedule);

                var dialog = new AddEditScheduleDialog
                {
                    DataContext = viewModel,
                    Owner = Application.Current.MainWindow
                };

                // Load data first
                if (viewModel.LoadDataCommand is AsyncRelayCommand loadCommand)
                {
                    await loadCommand.ExecuteAsync(null);
                }

                var result = dialog.ShowDialog();

                if (result == true)
                {
                    _logger.Information("Schedule {ScheduleId} updated successfully, refreshing list", SelectedSchedule.ScheduleId);
                    await LoadSchedulesAsync();
                }
                else
                {
                    _logger.Debug("Edit schedule dialog cancelled for schedule {ScheduleId}", SelectedSchedule.ScheduleId);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to edit schedule {ScheduleId}", SelectedSchedule.ScheduleId);
                ErrorMessage = "Failed to edit schedule. Please try again.";
            }
        }

        private async Task DeleteScheduleAsync()
        {
            if (SelectedSchedule == null)
            {
                Logger.Warning("Delete schedule attempted with no schedule selected");
                return;
            }

            using (LogContext.PushProperty("Operation", "DeleteScheduleAsync"))
            using (LogContext.PushProperty("ScheduleId", SelectedSchedule.ScheduleId))
            using (LogContext.PushProperty("SportsCategory", SelectedSchedule.SportsCategory))
            {
                Logger.Information("Deleting schedule");

                try
                {
                    var result = MessageBox.Show(
                        $"Are you sure you want to delete this schedule?\n\nRoute: {SelectedSchedule.Route?.RouteName}\nDate: {SelectedSchedule.ScheduleDate:yyyy-MM-dd}\nSports Category: {SelectedSchedule.SportsCategory ?? "None"}\nOpponent: {SelectedSchedule.Opponent ?? "N/A"}",
                        "Confirm Delete",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        Logger.Information("User confirmed deletion, proceeding...");

                        await _scheduleService.DeleteScheduleAsync(SelectedSchedule.ScheduleId);

                        // Remove from all collections
                        Schedules.Remove(SelectedSchedule);
                        FilteredSchedules.Remove(SelectedSchedule);
                        _allSchedules.Remove(SelectedSchedule);

                        // Update statistics
                        UpdateStatistics(_allSchedules.ToList());

                        Logger.Information("Successfully deleted schedule");
                    }
                    else
                    {
                        Logger.Debug("Delete schedule cancelled by user");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to delete schedule");
                    ErrorMessage = "Failed to delete schedule. Please try again.";
                }
            }
        }

        /// <summary>
        /// Filters schedules by the selected sports category
        /// </summary>
        private async Task FilterSchedulesByCategoryAsync()
        {
            if (SelectedCategory == null)
            {
                Logger.Warning("FilterSchedulesByCategoryAsync called with null SelectedCategory");
                return;
            }

            using (LogContext.PushProperty("Operation", "FilterSchedulesByCategoryAsync"))
            using (LogContext.PushProperty("SelectedCategory", SelectedCategory))
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                Logger.Information("Starting to filter schedules by category");

                try
                {
                    IsFiltering = true;
                    FilteredSchedules.Clear();

                    IEnumerable<BusBuddy.Core.Models.Schedule> filteredSchedules;

                    if (SelectedCategory == "All")
                    {
                        filteredSchedules = _allSchedules;
                        Logger.Debug("Showing all schedules");
                    }
                    else
                    {
                        // Use the service method for filtering
                        filteredSchedules = await _scheduleService.GetSchedulesByCategoryAsync(SelectedCategory);
                        Logger.Debug("Filtered schedules using service method");
                    }

                    var scheduleList = filteredSchedules.ToList();

                    foreach (var schedule in scheduleList)
                    {
                        FilteredSchedules.Add(schedule);
                    }

                    stopwatch.Stop();
                    Logger.Information("Successfully filtered {FilteredCount} schedules in {ElapsedMs}ms",
                        scheduleList.Count, stopwatch.ElapsedMilliseconds);

                    // Log category breakdown for diagnostics
                    if (scheduleList.Any())
                    {
                        var categoryBreakdown = scheduleList.GroupBy(s => s.SportsCategory ?? "None")
                            .ToDictionary(g => g.Key, g => g.Count());
                        Logger.Debug("Filtered results breakdown: {@CategoryBreakdown}", categoryBreakdown);
                    }
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    Logger.Error(ex, "Failed to filter schedules by category after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                    ErrorMessage = "Failed to filter schedules. Please try again.";
                }
                finally
                {
                    IsFiltering = false;
                }
            }
        }

        /// <summary>
        /// Updates statistics for dashboard display
        /// </summary>
        private void UpdateStatistics(List<BusBuddy.Core.Models.Schedule> schedules)
        {
            using (LogContext.PushProperty("Operation", "UpdateStatistics"))
            {
                Logger.Debug("Updating schedule statistics");

                TotalSchedules = schedules.Count;
                SportsTrips = schedules.Count(s => s.IsSportsTrip);
                RegularRoutes = schedules.Count(s => !s.IsSportsTrip);

                Logger.Information("Statistics updated: Total={TotalSchedules}, Sports={SportsTrips}, Routes={RegularRoutes}",
                    TotalSchedules, SportsTrips, RegularRoutes);
            }
        }
    }
}
