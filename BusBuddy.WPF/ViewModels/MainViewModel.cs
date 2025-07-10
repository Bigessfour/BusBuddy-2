using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using BusBuddy.WPF.ViewModels;

namespace BusBuddy.WPF.ViewModels
{
    public class NavigationItem
    {
        public required string Name { get; set; }
        public required string ViewModelName { get; set; }
    }

    public partial class MainViewModel : BaseViewModel
    {
        [ObservableProperty]
        private object? _currentViewModel;

        private readonly DashboardViewModel _dashboardViewModel;
        private readonly BusManagementViewModel _busManagementViewModel;
        private readonly DriverManagementViewModel _driverManagementViewModel;
        private readonly RouteManagementViewModel _routeManagementViewModel;
        private readonly ScheduleManagementViewModel _scheduleManagementViewModel;
        private readonly StudentManagementViewModel _studentManagementViewModel;
        private readonly MaintenanceTrackingViewModel _maintenanceTrackingViewModel;
        private readonly FuelManagementViewModel _fuelManagementViewModel;
        private readonly ActivityLogViewModel _activityLoggingViewModel;
        private readonly SettingsViewModel _settingsViewModel;
        private readonly StudentListViewModel _studentListViewModel;
        private readonly TicketManagementViewModel _ticketManagementViewModel;

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
            ActivityLogViewModel activityLoggingViewModel,
            SettingsViewModel settingsViewModel,
            StudentListViewModel studentListViewModel,
            TicketManagementViewModel ticketManagementViewModel)
        {
            _dashboardViewModel = dashboardViewModel;
            _busManagementViewModel = busManagementViewModel;
            _driverManagementViewModel = driverManagementViewModel;
            _routeManagementViewModel = routeManagementViewModel;
            _scheduleManagementViewModel = scheduleManagementViewModel;
            _studentManagementViewModel = studentManagementViewModel;
            _maintenanceTrackingViewModel = maintenanceTrackingViewModel;
            _fuelManagementViewModel = fuelManagementViewModel;
            _activityLoggingViewModel = activityLoggingViewModel;
            _settingsViewModel = settingsViewModel;
            _studentListViewModel = studentListViewModel;
            _ticketManagementViewModel = ticketManagementViewModel;

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
                new NavigationItem { Name = "Tickets", ViewModelName = "Tickets" },
                new NavigationItem { Name = "Settings", ViewModelName = "Settings" }
            };

            // Set the default view
            CurrentViewModel = _dashboardViewModel;

            // Log initialization for debugging
            System.Diagnostics.Debug.WriteLine($"MainViewModel initialized with current view: {CurrentViewModel?.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"Navigation items count: {NavigationItems.Count}");
        }

        partial void OnCurrentViewModelChanged(object? value)
        {
            System.Diagnostics.Debug.WriteLine($"CurrentViewModel changed to: {value?.GetType().Name ?? "null"}");
        }

        [RelayCommand]
        private void NavigateTo(string viewModelName)
        {
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
                "Activity" => _activityLoggingViewModel,
                "Settings" => _settingsViewModel,
                "StudentList" => _studentListViewModel,
                "Tickets" => _ticketManagementViewModel,
                _ => _dashboardViewModel
            };
        }
    }
}
