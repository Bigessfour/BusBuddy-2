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
        public ObservableCollection<Schedule> BusSchedules { get; }

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

        public DashboardViewModel()
        {
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
            BusSchedules = new ObservableCollection<Schedule>
            {
                new Schedule { BusId = 1, RouteId = 101, DriverId = 1, DepartureTime = DateTime.Now.AddHours(1), ArrivalTime = DateTime.Now.AddHours(2), Status = "Scheduled" },
                new Schedule { BusId = 2, RouteId = 102, DriverId = 2, DepartureTime = DateTime.Now.AddHours(1.5), ArrivalTime = DateTime.Now.AddHours(2.5), Status = "Scheduled" },
                new Schedule { BusId = 3, RouteId = 103, DriverId = 3, DepartureTime = DateTime.Now.AddHours(2), ArrivalTime = DateTime.Now.AddHours(3), Status = "Delayed" },
                new Schedule { BusId = 4, RouteId = 104, DriverId = 4, DepartureTime = DateTime.Now.AddHours(2.5), ArrivalTime = DateTime.Now.AddHours(3.5), Status = "Scheduled" },
                new Schedule { BusId = 5, RouteId = 105, DriverId = 5, DepartureTime = DateTime.Now.AddHours(3), ArrivalTime = DateTime.Now.AddHours(4), Status = "Cancelled" }
            };

            // Initialize commands
            NavigateToBusManagementCommand = new RelayCommand(() => NavigateToModule?.Invoke("BusManagement"));
            NavigateToDriverManagementCommand = new RelayCommand(() => NavigateToModule?.Invoke("DriverManagement"));
            NavigateToRouteManagementCommand = new RelayCommand(() => NavigateToModule?.Invoke("RouteManagement"));
            NavigateToScheduleManagementCommand = new RelayCommand(() => NavigateToModule?.Invoke("ScheduleManagement"));
            NavigateToStudentManagementCommand = new RelayCommand(() => NavigateToModule?.Invoke("StudentManagement"));
            NavigateToMaintenanceTrackingCommand = new RelayCommand(() => NavigateToModule?.Invoke("MaintenanceTracking"));
            NavigateToFuelManagementCommand = new RelayCommand(() => NavigateToModule?.Invoke("FuelManagement"));
            NavigateToActivityLoggingCommand = new RelayCommand(() => NavigateToModule?.Invoke("ActivityLogging"));
            NavigateToTicketManagementCommand = new RelayCommand(() => NavigateToModule?.Invoke("TicketManagement"));
            NavigateToSettingsCommand = new RelayCommand(() => NavigateToModule?.Invoke("Settings"));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Simple RelayCommand implementation for ICommand
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        public RelayCommand(Action execute) => _execute = execute;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute();
        public event EventHandler? CanExecuteChanged { add { } remove { } }
    }
}
