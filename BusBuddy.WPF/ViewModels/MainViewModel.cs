using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using BusBuddy.WPF.ViewModels;
using BusBuddy.WPF.ViewModels.Schedule;
using Microsoft.Extensions.Logging;

namespace BusBuddy.WPF.ViewModels
{
    public class NavigationItem
    {
        public required string Name { get; set; }
        public required string ViewModelName { get; set; }

        /// <summary>
        /// Gets or sets whether this navigation item is for a deprecated module.
        /// </summary>
        public bool IsDeprecated { get; set; }

        /// <summary>
        /// Gets or sets a tooltip to display for this navigation item.
        /// </summary>
        public string? Tooltip { get; set; }
    }

    public partial class MainViewModel : BaseViewModel
    {
        [ObservableProperty]
        private object? _currentViewModel;

        private readonly ILogger<MainViewModel>? _logger;
        private readonly DashboardViewModel _dashboardViewModel;
        private readonly BusManagementViewModel _busManagementViewModel;
        private readonly DriverManagementViewModel _driverManagementViewModel;
        private readonly RouteManagementViewModel _routeManagementViewModel;
        private readonly ScheduleManagementViewModel _scheduleManagementViewModel;
        private readonly StudentManagementViewModel _studentManagementViewModel;
        private readonly MaintenanceTrackingViewModel _maintenanceTrackingViewModel;
        private readonly FuelManagementViewModel _fuelManagementViewModel;
        private readonly ActivityLogViewModel _activityLogViewModel;
        private readonly SettingsViewModel _settingsViewModel;
        private readonly StudentListViewModel _studentListViewModel;
        private readonly LoadingViewModel _loadingViewModel;

        public ObservableCollection<NavigationItem> NavigationItems { get; }

        public MainViewModel(
            DashboardViewModel dashboardViewModel,
            BusManagementViewModel busManagementViewModel,
            DriverManagementViewModel driverManagementViewModel,
            RouteManagementViewModel routeManagementViewModel,
            ScheduleManagementViewModel scheduleManagementViewModel,
            StudentManagementViewModel studentManagementViewModel,
            MaintenanceTrackingViewModel maintenanceTrackingViewModel,
            FuelManagementViewModel fuelManagementViewModel,
            ActivityLogViewModel activityLogViewModel,
            SettingsViewModel settingsViewModel,
            StudentListViewModel studentListViewModel,
            LoadingViewModel loadingViewModel,
            ILogger<MainViewModel>? logger = null)
        {
            _dashboardViewModel = dashboardViewModel;
            _busManagementViewModel = busManagementViewModel;
            _driverManagementViewModel = driverManagementViewModel;
            _routeManagementViewModel = routeManagementViewModel;
            _scheduleManagementViewModel = scheduleManagementViewModel;
            _studentManagementViewModel = studentManagementViewModel;
            _maintenanceTrackingViewModel = maintenanceTrackingViewModel;
            _fuelManagementViewModel = fuelManagementViewModel;
            _activityLogViewModel = activityLogViewModel;
            _settingsViewModel = settingsViewModel;
            _studentListViewModel = studentListViewModel;
            _loadingViewModel = loadingViewModel;
            _logger = logger;

            NavigationItems = new ObservableCollection<NavigationItem>
            {
                new NavigationItem { Name = "Dashboard", ViewModelName = "Dashboard" },
                new NavigationItem { Name = "Buses", ViewModelName = "Buses" },
                new NavigationItem { Name = "Drivers", ViewModelName = "Drivers" },
                new NavigationItem { Name = "Routes", ViewModelName = "Routes" },
                new NavigationItem { Name = "Schedule", ViewModelName = "Schedule" },
                new NavigationItem { Name = "Students", ViewModelName = "Students" },
                new NavigationItem { Name = "Maintenance", ViewModelName = "Maintenance" },
                new NavigationItem { Name = "Fuel", ViewModelName = "Fuel" },
                new NavigationItem { Name = "Activity", ViewModelName = "Activity" },
                new NavigationItem { Name = "Student List", ViewModelName = "StudentList" },
                new NavigationItem { Name = "Settings", ViewModelName = "Settings" }
            };

            // Start with loading view for smooth startup - no data loading yet
            CurrentViewModel = _loadingViewModel;

            _logger?.LogInformation("MainViewModel initialized with {Count} navigation items", NavigationItems.Count);
        }

        partial void OnCurrentViewModelChanged(object? value)
        {
            var viewName = value?.GetType().Name?.Replace("ViewModel", "") ?? "Unknown";
            _logger?.LogInformation("UI View model changed - CurrentView is now {ViewName} ({ViewModelType})",
                viewName, value?.GetType().Name ?? "null");
        }

        [RelayCommand]
        private void NavigateTo(string viewModelName)
        {
            _logger?.LogInformation("UI Navigation initiated to {ViewModelName} via button click", viewModelName);

            object? previousViewModel = CurrentViewModel;
            var startTime = System.Diagnostics.Stopwatch.StartNew();

            CurrentViewModel = viewModelName switch
            {
                "Dashboard" => _dashboardViewModel,
                "Buses" => _busManagementViewModel,
                "Drivers" => _driverManagementViewModel,
                "Routes" => _routeManagementViewModel,
                "Schedule" => _scheduleManagementViewModel,
                "Students" => _studentManagementViewModel,
                "Maintenance" => _maintenanceTrackingViewModel,
                "Fuel" => _fuelManagementViewModel,
                "Activity" => _activityLogViewModel,
                "Settings" => _settingsViewModel,
                "StudentList" => _studentListViewModel,
                "Loading" => _loadingViewModel,
                "Error" => _loadingViewModel, // Use loading view for errors too
                _ => _dashboardViewModel
            };

            startTime.Stop();
            _logger?.LogInformation("UI View transition completed to {ViewModel} from {PreviousViewModel} in {ElapsedMs}ms",
                CurrentViewModel?.GetType().Name,
                previousViewModel?.GetType().Name ?? "null",
                startTime.ElapsedMilliseconds);
        }
    }
}
