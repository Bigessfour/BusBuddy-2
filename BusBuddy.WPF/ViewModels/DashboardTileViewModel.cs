using System.ComponentModel;
using System.Windows.Input;
using BusBuddy.WPF.ViewModels.Panels;
using Syncfusion.Windows.Tools.Controls;
using System.Runtime.CompilerServices;
using BusBuddy.Core.Data.UnitOfWork;

namespace BusBuddy.WPF.ViewModels
{
    /// <summary>
    /// Represents a tile in the modernized dashboard using SfTileView
    /// </summary>
    public class DashboardTileViewModel : PanelViewModel
    {
        private string? _title;
        private object? _content;
        private string _state = "Normal";
        private int _priority = 0;
        private bool _isRefreshing = false;
        private DateTime _lastUpdated = DateTime.Now;

        public string? Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        public object? Content
        {
            get => _content;
            set { _content = value; OnPropertyChanged(); }
        }

        public string State
        {
            get => _state;
            set { _state = value; OnPropertyChanged(); }
        }

        public int Priority
        {
            get => _priority;
            set { _priority = value; OnPropertyChanged(); }
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set { _isRefreshing = value; OnPropertyChanged(); }
        }

        public DateTime LastUpdated
        {
            get => _lastUpdated;
            set { _lastUpdated = value; OnPropertyChanged(); }
        }

        public string LastUpdatedFormatted => LastUpdated.ToString("HH:mm:ss");

        // Commands for tile interactions
        public ICommand? RefreshCommand { get; set; }
        public ICommand? MaximizeCommand { get; set; }
        public ICommand? MinimizeCommand { get; set; }
        public ICommand? CloseCommand { get; set; }

        public DashboardTileViewModel()
        {
            RefreshCommand = new RelayCommand(async _ => await RefreshTileAsync());
            MaximizeCommand = new RelayCommand(_ => State = "Maximized");
            MinimizeCommand = new RelayCommand(_ => State = "Minimized");
        }

        public virtual async Task RefreshTileAsync()
        {
            IsRefreshing = true;
            try
            {
                // Override in derived classes for specific refresh logic
                await Task.Delay(1000); // Simulate async operation
                LastUpdated = DateTime.Now;
            }
            finally
            {
                IsRefreshing = false;
            }
        }
    }

    /// <summary>
    /// Specific tile for fleet status display
    /// </summary>
    public class FleetStatusTileViewModel : DashboardTileViewModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private int _activeBusCount;
        private int _maintenanceBusCount;
        private int _outOfServiceCount;

        public int ActiveBusCount
        {
            get => _activeBusCount;
            set { _activeBusCount = value; OnPropertyChanged(); }
        }

        public int MaintenanceBusCount
        {
            get => _maintenanceBusCount;
            set { _maintenanceBusCount = value; OnPropertyChanged(); }
        }

        public int OutOfServiceCount
        {
            get => _outOfServiceCount;
            set { _outOfServiceCount = value; OnPropertyChanged(); }
        }

        public int TotalBusCount => ActiveBusCount + MaintenanceBusCount + OutOfServiceCount;

        public FleetStatusTileViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            Title = "Fleet Status";
            Priority = 1;
        }

        public override async Task RefreshTileAsync()
        {
            IsRefreshing = true;
            try
            {
                // Get real fleet data from repository
                var vehicleCountByStatus = await _unitOfWork.Buses.GetVehicleCountByStatusAsync();

                // Map status values to our display properties
                ActiveBusCount = vehicleCountByStatus.GetValueOrDefault("Active", 0);
                MaintenanceBusCount = vehicleCountByStatus.GetValueOrDefault("Maintenance", 0) +
                                     vehicleCountByStatus.GetValueOrDefault("In Maintenance", 0);
                OutOfServiceCount = vehicleCountByStatus.GetValueOrDefault("Out of Service", 0) +
                                   vehicleCountByStatus.GetValueOrDefault("Inactive", 0);

                LastUpdated = DateTime.Now;
            }
            catch (Exception ex)
            {
                // Log error and fallback to default values
                System.Diagnostics.Debug.WriteLine($"Error refreshing fleet status: {ex.Message}");

                // Fallback values if database is unavailable
                ActiveBusCount = 0;
                MaintenanceBusCount = 0;
                OutOfServiceCount = 0;
            }
            finally
            {
                IsRefreshing = false;
            }
        }
    }

    /// <summary>
    /// Specific tile for maintenance alerts
    /// </summary>
    public class MaintenanceAlertsTileViewModel : DashboardTileViewModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private int _criticalMaintenanceCount;
        private int _upcomingMaintenanceCount;
        private int _overdueMaintenanceCount;

        public int CriticalMaintenanceCount
        {
            get => _criticalMaintenanceCount;
            set { _criticalMaintenanceCount = value; OnPropertyChanged(); }
        }

        public int UpcomingMaintenanceCount
        {
            get => _upcomingMaintenanceCount;
            set { _upcomingMaintenanceCount = value; OnPropertyChanged(); }
        }

        public int OverdueMaintenanceCount
        {
            get => _overdueMaintenanceCount;
            set { _overdueMaintenanceCount = value; OnPropertyChanged(); }
        }

        public MaintenanceAlertsTileViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            Title = "Maintenance Alerts";
            Priority = 2;
        }

        public override async Task RefreshTileAsync()
        {
            IsRefreshing = true;
            try
            {
                // Get real maintenance data from repository
                var upcomingMaintenance = await _unitOfWork.MaintenanceRecords.GetUpcomingMaintenanceAsync(30);
                var overdueMaintenance = await _unitOfWork.MaintenanceRecords.GetOverdueMaintenanceAsync();
                var vehiclesDueForInspection = await _unitOfWork.Buses.GetVehiclesWithExpiredInspectionAsync();

                // Calculate counts based on real data
                UpcomingMaintenanceCount = upcomingMaintenance.Count();
                OverdueMaintenanceCount = overdueMaintenance.Count();
                CriticalMaintenanceCount = vehiclesDueForInspection.Count();

                LastUpdated = DateTime.Now;
            }
            catch (Exception ex)
            {
                // Log error and fallback to default values
                System.Diagnostics.Debug.WriteLine($"Error refreshing maintenance alerts: {ex.Message}");

                // Fallback values if database is unavailable
                CriticalMaintenanceCount = 0;
                UpcomingMaintenanceCount = 0;
                OverdueMaintenanceCount = 0;
            }
            finally
            {
                IsRefreshing = false;
            }
        }
    }

    /// <summary>
    /// Quick actions tile for common operations
    /// </summary>
    public class QuickActionsTileViewModel : DashboardTileViewModel
    {
        private readonly Action<string>? _navigationAction;

        public ICommand QuickAddStudentCommand { get; set; }
        public ICommand QuickAddBusCommand { get; set; }
        public ICommand QuickScheduleTripCommand { get; set; }
        public ICommand QuickMaintenanceCommand { get; set; }
        public ICommand QuickFuelEntryCommand { get; set; }
        public ICommand QuickReportCommand { get; set; }

        public QuickActionsTileViewModel(Action<string>? navigationAction = null)
        {
            _navigationAction = navigationAction;
            Title = "Quick Actions";
            Priority = 5;
            State = "Minimized";

            // Initialize commands with actual navigation
            QuickAddStudentCommand = new RelayCommand(_ => _navigationAction?.Invoke("Students"));
            QuickAddBusCommand = new RelayCommand(_ => _navigationAction?.Invoke("Buses"));
            QuickScheduleTripCommand = new RelayCommand(_ => _navigationAction?.Invoke("Schedule"));
            QuickMaintenanceCommand = new RelayCommand(_ => _navigationAction?.Invoke("Maintenance"));
            QuickFuelEntryCommand = new RelayCommand(_ => _navigationAction?.Invoke("Fuel"));
            QuickReportCommand = new RelayCommand(_ => _navigationAction?.Invoke("Activity"));
        }
    }
}
