using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BusBuddy.WPF.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly IScheduleService _scheduleService;
        private readonly IBusService _busService;

        [ObservableProperty]
        private string _dashboardTitle = "Bus Buddy Dashboard";

        [ObservableProperty]
        private ObservableCollection<RidershipDataPoint> _ridershipData;

        [ObservableProperty]
        private ObservableCollection<Activity> _busSchedules = new();

        [ObservableProperty]
        private int _totalActiveBuses;

        [ObservableProperty]
        private int _totalInactiveBuses;

        [ObservableProperty]
        private int _busesWithMaintenanceDue;

        [ObservableProperty]
        private bool _isSidebarVisible = true;

        public ICommand ToggleSidebarCommand { get; }
        public ICommand NavigateToBusManagementCommand { get; }
        public ICommand NavigateToDriverManagementCommand { get; }
        public ICommand NavigateToRouteManagementCommand { get; }
        public ICommand NavigateToScheduleManagementCommand { get; }
        public ICommand NavigateToStudentManagementCommand { get; }
        public ICommand NavigateToMaintenanceTrackingCommand { get; }
        public ICommand NavigateToFuelManagementCommand { get; }
        public ICommand NavigateToActivityLoggingCommand { get; }
        public ICommand NavigateToTicketManagementCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }

        public event Action<string>? NavigateToModule;

        public DashboardViewModel(IScheduleService scheduleService, IBusService busService)
        {
            _scheduleService = scheduleService;
            _busService = busService;

            _ridershipData = new ObservableCollection<RidershipDataPoint>
            {
                new() { Date = DateTime.Now.AddDays(-6), PassengerCount = 120 },
                new() { Date = DateTime.Now.AddDays(-5), PassengerCount = 135 },
                new() { Date = DateTime.Now.AddDays(-4), PassengerCount = 140 },
                new() { Date = DateTime.Now.AddDays(-3), PassengerCount = 160 },
                new() { Date = DateTime.Now.AddDays(-2), PassengerCount = 155 },
                new() { Date = DateTime.Now.AddDays(-1), PassengerCount = 170 },
                new() { Date = DateTime.Now, PassengerCount = 180 }
            };

            ToggleSidebarCommand = new RelayCommand(_ => IsSidebarVisible = !IsSidebarVisible);
            NavigateToBusManagementCommand = new RelayCommand(_ => NavigateToModule?.Invoke("BusManagement"));
            NavigateToDriverManagementCommand = new RelayCommand(_ => NavigateToModule?.Invoke("DriverManagement"));
            NavigateToRouteManagementCommand = new RelayCommand(_ => NavigateToModule?.Invoke("RouteManagement"));
            NavigateToScheduleManagementCommand = new RelayCommand(_ => NavigateToModule?.Invoke("ScheduleManagement"));
            NavigateToStudentManagementCommand = new RelayCommand(_ => NavigateToModule?.Invoke("StudentManagement"));
            NavigateToMaintenanceTrackingCommand = new RelayCommand(_ => NavigateToModule?.Invoke("MaintenanceTracking"));
            NavigateToFuelManagementCommand = new RelayCommand(_ => NavigateToModule?.Invoke("FuelManagement"));
            NavigateToActivityLoggingCommand = new RelayCommand(_ => NavigateToModule?.Invoke("ActivityLogging"));
            NavigateToTicketManagementCommand = new RelayCommand(_ => NavigateToModule?.Invoke("TicketManagement"));
            NavigateToSettingsCommand = new RelayCommand(_ => NavigateToModule?.Invoke("Settings"));

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            await LoadBusSchedulesAsync();
            await LoadFleetStatusSummaryAsync();
        }

        private async Task LoadFleetStatusSummaryAsync()
        {
            try
            {
                var buses = await _busService.GetAllBusEntitiesAsync();
                TotalActiveBuses = buses.Count(b => b.Status == "Active");
                TotalInactiveBuses = buses.Count(b => b.Status == "Inactive");
                BusesWithMaintenanceDue = buses.Count(b => b.NextMaintenanceDue.HasValue && b.NextMaintenanceDue.Value <= DateTime.Now);
            }
            catch
            {
                TotalActiveBuses = 0;
                TotalInactiveBuses = 0;
                BusesWithMaintenanceDue = 0;
            }
        }

        private async Task LoadBusSchedulesAsync()
        {
            var activities = await _scheduleService.GetAllSchedulesAsync();
            BusSchedules = new ObservableCollection<Activity>(activities);
        }
    }

    public class RidershipDataPoint
    {
        public DateTime Date { get; set; }
        public int PassengerCount { get; set; }
    }
}
