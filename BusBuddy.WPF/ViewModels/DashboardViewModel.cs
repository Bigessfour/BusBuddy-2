using BusBuddy.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BusBuddy.Core.Models;

namespace BusBuddy.WPF.ViewModels
{
    public class RidershipDataPoint
    {
        public DateTime Date { get; set; }
        public int PassengerCount { get; set; }
    }

    public class DashboardViewModel : INotifyPropertyChanged
    {
        private string _dashboardTitle = "Bus Buddy Dashboard";
        public string DashboardTitle
        {
            get => _dashboardTitle;
            set { _dashboardTitle = value; OnPropertyChanged(); }
        }

        public ObservableCollection<RidershipDataPoint> RidershipData { get; }
        public ObservableCollection<Activity> BusSchedules { get; set; } = new ObservableCollection<Activity>();

        // Navigation commands for the 10 modules
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

        // Event to notify view of navigation
        public event Action<string>? NavigateToModule;

        private readonly IScheduleService _scheduleService;
        private readonly BusBuddy.Core.Services.IBusService _busService;

        // Fleet status summary properties
        private int _totalActiveBuses;
        public int TotalActiveBuses
        {
            get => _totalActiveBuses;
            set { _totalActiveBuses = value; OnPropertyChanged(); }
        }

        private int _totalInactiveBuses;
        public int TotalInactiveBuses
        {
            get => _totalInactiveBuses;
            set { _totalInactiveBuses = value; OnPropertyChanged(); }
        }

        private int _busesWithMaintenanceDue;
        public int BusesWithMaintenanceDue
        {
            get => _busesWithMaintenanceDue;
            set { _busesWithMaintenanceDue = value; OnPropertyChanged(); }
        }

        public DashboardViewModel(IScheduleService scheduleService, BusBuddy.Core.Services.IBusService busService)
        {
            _scheduleService = scheduleService;
            _busService = busService;
            RidershipData = new ObservableCollection<RidershipDataPoint>
            {
                new RidershipDataPoint { Date = DateTime.Now.AddDays(-6), PassengerCount = 120 },
                new RidershipDataPoint { Date = DateTime.Now.AddDays(-5), PassengerCount = 135 },
                new RidershipDataPoint { Date = DateTime.Now.AddDays(-4), PassengerCount = 140 },
                new RidershipDataPoint { Date = DateTime.Now.AddDays(-3), PassengerCount = 160 },
                new RidershipDataPoint { Date = DateTime.Now.AddDays(-2), PassengerCount = 155 },
                new RidershipDataPoint { Date = DateTime.Now.AddDays(-1), PassengerCount = 170 },
                new RidershipDataPoint { Date = DateTime.Now, PassengerCount = 180 }
            };
            LoadData();
            LoadFleetStatusSummary();

            // Initialize commands
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
        }

        private async void LoadFleetStatusSummary()
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
                // Handle/log error as needed
                TotalActiveBuses = 0;
                TotalInactiveBuses = 0;
                BusesWithMaintenanceDue = 0;
            }
        }

        private void LoadData()
        {
            _ = LoadBusSchedulesAsync();
        }

        private async Task LoadBusSchedulesAsync()
        {
            var activities = await _scheduleService.GetAllSchedulesAsync();
            BusSchedules = new ObservableCollection<Activity>(activities);
            OnPropertyChanged(nameof(BusSchedules));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // ...existing code...
}
