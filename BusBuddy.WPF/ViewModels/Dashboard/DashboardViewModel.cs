using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using BusBuddy.Core.Services.Interfaces;
using BusBuddy.WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BusBuddy.WPF.ViewModels
{
    public partial class DashboardViewModel : BaseViewModel
    {
        private readonly IBusService _busService;
        private readonly IScheduleService _scheduleService;
        private readonly IRoutePopulationScaffold _routePopulationScaffold;

        [ObservableProperty]
        private int _totalActiveBuses;
        [ObservableProperty]
        private int _totalInactiveBuses;
        [ObservableProperty]
        private int _busesWithMaintenanceDue;
        [ObservableProperty]
        private ObservableCollection<Activity> _busSchedules = new();
        [ObservableProperty]
        private string _selectedModule = "Bus Management";
        [ObservableProperty]
        private string _dashboardTitle;
        [ObservableProperty]
        private bool _isSidebarVisible = true;
        [ObservableProperty]
        private int _selectedTabIndex = 0;

        public ObservableCollection<NavigationItemViewModel> NavigationItems { get; }

        public DashboardViewModel(IScheduleService scheduleService, IBusService busService, IRoutePopulationScaffold routePopulationScaffold)
        {
            _scheduleService = scheduleService;
            _busService = busService;
            _routePopulationScaffold = routePopulationScaffold;

            // Set initial values
            SelectedModule = "Bus Management";
            DashboardTitle = "Bus Buddy - Bus Management";
            IsSidebarVisible = true;

            NavigationItems = new ObservableCollection<NavigationItemViewModel>
            {
                new NavigationItemViewModel { Content = "Bus Management", Command = new RelayCommand(_ => NavigateTo("Bus Management")), IsSelected = true },
                new NavigationItemViewModel { Content = "Driver Management", Command = new RelayCommand(_ => NavigateTo("Driver Management")) },
                new NavigationItemViewModel { Content = "Route Management", Command = new RelayCommand(_ => NavigateTo("Route Management")) },
                new NavigationItemViewModel { Content = "Schedule Management", Command = new RelayCommand(_ => NavigateTo("Schedule Management")) },
                new NavigationItemViewModel { Content = "Student Management", Command = new RelayCommand(_ => NavigateTo("Student Management")) },
                new NavigationItemViewModel { Content = "Maintenance Tracking", Command = new RelayCommand(_ => NavigateTo("Maintenance Tracking")) },
                new NavigationItemViewModel { Content = "Fuel Management", Command = new RelayCommand(_ => NavigateTo("Fuel Management")) },
                new NavigationItemViewModel { Content = "Activity Logging", Command = new RelayCommand(_ => NavigateTo("Activity Logging")) },
                new NavigationItemViewModel { Content = "Ticket Management", Command = new RelayCommand(_ => NavigateTo("Ticket Management")) },
                new NavigationItemViewModel { Content = "Settings", Command = new RelayCommand(_ => NavigateTo("Settings")) }
            };

            LoadDashboardDataCommand.Execute(null);
        }

        [RelayCommand]
        private void ToggleSidebar()
        {
            IsSidebarVisible = !IsSidebarVisible;
        }
        [RelayCommand]
        private void NavigateTo(string moduleName)
        {
            SelectedModule = moduleName;
            DashboardTitle = $"Bus Buddy - {moduleName}";

            // Update selected status of navigation items
            foreach (var item in NavigationItems)
            {
                item.IsSelected = item.Content == moduleName;
            }

            // Update SelectedTabIndex based on the module name
            SelectedTabIndex = moduleName switch
            {
                "Bus Management" => 0,
                "Driver Management" => 1,
                "Route Management" => 2,
                "Schedule Management" => 3,
                "Student Management" => 4,
                "Maintenance Tracking" => 5,
                "Fuel Management" => 6,
                "Activity Logging" => 7,
                "Ticket Management" => 8,
                "Settings" => 9,
                _ => 0
            };

            // Refresh data when switching to certain modules
            if (moduleName == "Bus Management" || moduleName == "Fuel Management")
            {
                LoadDashboardDataCommand.Execute(null);
            }
        }

        [RelayCommand]
        private async Task LoadDashboardData()
        {
            try
            {
                var buses = await _busService.GetAllBusesAsync();
                TotalActiveBuses = buses.Count(b => b.Status == "Active");
                TotalInactiveBuses = buses.Count(b => b.Status == "Inactive");
                BusesWithMaintenanceDue = buses.Count(b => b.Status == "Maintenance");
            }
            catch (Exception)
            {
                TotalActiveBuses = 0;
                TotalInactiveBuses = 0;
                BusesWithMaintenanceDue = 0;
            }

            try
            {
                var schedules = await _scheduleService.GetSchedulesAsync();
                var todaySchedules = schedules.Where(s => s.ScheduleDate.Date == DateTime.Today).ToList();

                // Convert schedules to activities using the helper method
                var activities = todaySchedules.Select(ConvertScheduleToActivity).ToList();

                BusSchedules = new ObservableCollection<Activity>(activities);
            }
            catch (Exception)
            {
                BusSchedules.Clear();
            }
        }

        // Helper method to convert Schedule objects to Activity objects
        private Activity ConvertScheduleToActivity(Schedule schedule)
        {
            return new Activity
            {
                ActivityId = schedule.ScheduleId,
                AssignedVehicleId = schedule.BusId,
                DriverId = schedule.DriverId,
                Date = schedule.ScheduleDate,
                LeaveTime = schedule.DepartureTime.TimeOfDay,
                EventTime = schedule.ArrivalTime.TimeOfDay,
                ActivityType = "Route Run",
                Status = schedule.Status,
                RouteId = schedule.RouteId,
                Destination = "Route Destination",
                RequestedBy = "System" // Default value for required field
            };
        }
    }

    public class RidershipDataPoint
    {
        public DateTime Date { get; set; }
        public int PassengerCount { get; set; }
    }

    public class NavigationItemViewModel : ObservableObject
    {
        private string _content = string.Empty;
        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        private ICommand _command = null!;
        public ICommand Command
        {
            get => _command;
            set => SetProperty(ref _command, value);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
