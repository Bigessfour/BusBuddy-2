using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using BusBuddy.Core.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BusBuddy.WPF.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly BusBuddy.Core.Services.Interfaces.IScheduleService _scheduleService;
        private readonly IBusService _busService;

        [ObservableProperty]
        private object? _currentModuleView;

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


        public DashboardViewModel(BusBuddy.Core.Services.Interfaces.IScheduleService scheduleService, IBusService busService)
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
            NavigateToBusManagementCommand = new RelayCommand(_ => ShowModule("BusManagement"));
            NavigateToDriverManagementCommand = new RelayCommand(_ => ShowModule("DriverManagement"));
            NavigateToRouteManagementCommand = new RelayCommand(_ => ShowModule("RouteManagement"));
            NavigateToScheduleManagementCommand = new RelayCommand(_ => ShowModule("ScheduleManagement"));
            NavigateToStudentManagementCommand = new RelayCommand(_ => ShowModule("StudentManagement"));
            NavigateToMaintenanceTrackingCommand = new RelayCommand(_ => ShowModule("MaintenanceTracking"));
            NavigateToFuelManagementCommand = new RelayCommand(_ => ShowModule("FuelManagement"));
            NavigateToActivityLoggingCommand = new RelayCommand(_ => ShowModule("ActivityLogging"));
            NavigateToTicketManagementCommand = new RelayCommand(_ => ShowModule("TicketManagement"));
            NavigateToSettingsCommand = new RelayCommand(_ => ShowModule("Settings"));

            _ = LoadDataAsync();
        }

        private void ShowModule(string moduleName)
        {
            // Use DI to resolve the view for the module
            var app = System.Windows.Application.Current as App;
            if (app?.Services == null) return;
            switch (moduleName)
            {
                case "BusManagement":
                    CurrentModuleView = new BusBuddy.WPF.Views.BusManagementView();
                    break;
                case "DriverManagement":
                    CurrentModuleView = new BusBuddy.WPF.Views.DriverManagementView();
                    break;
                case "RouteManagement":
                    CurrentModuleView = new BusBuddy.WPF.Views.RouteManagementView();
                    break;
                case "ScheduleManagement":
                    CurrentModuleView = new BusBuddy.WPF.Views.ScheduleManagementView();
                    break;
                case "StudentManagement":
                    CurrentModuleView = new BusBuddy.WPF.Views.StudentManagementView();
                    break;
                case "MaintenanceTracking":
                    CurrentModuleView = new BusBuddy.WPF.Views.MaintenanceTrackingView();
                    break;
                case "FuelManagement":
                    CurrentModuleView = new BusBuddy.WPF.Views.FuelManagementView();
                    break;
                case "ActivityLogging":
                    CurrentModuleView = new BusBuddy.WPF.Views.ActivityLoggingView();
                    break;
                case "TicketManagement":
                    CurrentModuleView = new BusBuddy.WPF.Views.TicketManagementView();
                    break;
                case "Settings":
                    CurrentModuleView = new BusBuddy.WPF.Views.SettingsView();
                    break;
                default:
                    CurrentModuleView = null;
                    break;
            }
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
            var schedules = await _scheduleService.GetSchedulesAsync();
            // Map or convert Schedule to Activity if needed, or update the UI to use Schedule directly
            // For now, just clear the collection
            BusSchedules.Clear();
            // If you want to display schedules, you may need to create a new ObservableCollection<Schedule>
        }
    }

    public class RidershipDataPoint
    {
        public DateTime Date { get; set; }
        public int PassengerCount { get; set; }
    }
}
