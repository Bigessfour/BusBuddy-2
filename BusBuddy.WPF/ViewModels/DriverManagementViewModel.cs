using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using BusBuddy.WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BusBuddy.WPF.ViewModels
{
    public partial class DriverManagementViewModel : ObservableObject
    {
        private readonly IDriverService _driverService;
        private readonly IDriverAvailabilityService _availabilityService;
        private readonly IActivityLogService _activityLogService;

        [ObservableProperty]
        private ObservableCollection<Driver> _drivers;

        [ObservableProperty]
        private Driver _selectedDriver;

        [ObservableProperty]
        private ObservableCollection<DriverAvailability> _driverAvailabilities;

        [ObservableProperty]
        private ObservableCollection<DateTime> _selectedAvailabilityDates = new();

        [ObservableProperty]
        private ObservableCollection<DriverLicenseStatus> _licenseStatusReport = new();

        public ICommand LoadDriversCommand { get; }
        public ICommand AddDriverCommand { get; }
        public ICommand UpdateDriverCommand { get; }
        public ICommand DeleteDriverCommand { get; }
        public ICommand GenerateLicenseStatusReportCommand { get; }

        public DriverManagementViewModel(IDriverService driverService, IDriverAvailabilityService availabilityService, IActivityLogService activityLogService)
        {
            _driverService = driverService;
            _availabilityService = availabilityService;
            _activityLogService = activityLogService;

            Drivers = new ObservableCollection<Driver>();
            SelectedDriver = new Driver();
            DriverAvailabilities = new ObservableCollection<DriverAvailability>();

            LoadDriversCommand = new AsyncRelayCommand(LoadDriversAsync);
            AddDriverCommand = new AsyncRelayCommand(AddDriverAsync);
            UpdateDriverCommand = new AsyncRelayCommand(UpdateDriverAsync, CanUpdateOrDelete);
            DeleteDriverCommand = new AsyncRelayCommand(DeleteDriverAsync, CanUpdateOrDelete);
            GenerateLicenseStatusReportCommand = new RelayCommand(_ => GenerateLicenseStatusReport());

            _ = LoadDriversAsync();
            _ = LoadDriverAvailabilitiesAsync();
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
                await _activityLogService.LogAsync("Loaded drivers", "System");
            }
            catch (Exception ex)
            {
                await _activityLogService.LogAsync("Error loading drivers", "System", ex.ToString());
            }
        }

        private async Task AddDriverAsync()
        {
            if (SelectedDriver != null)
            {
                await _driverService.AddDriverAsync(SelectedDriver);
                await LoadDriversAsync();
            }
        }

        private async Task UpdateDriverAsync()
        {
            if (SelectedDriver != null)
            {
                await _driverService.UpdateDriverAsync(SelectedDriver);
                await LoadDriversAsync();
            }
        }

        private async Task DeleteDriverAsync()
        {
            if (SelectedDriver != null)
            {
                await _driverService.DeleteDriverAsync(SelectedDriver.DriverId);
                await LoadDriversAsync();
            }
        }

        private bool CanUpdateOrDelete()
        {
            return SelectedDriver != null && SelectedDriver.DriverId != 0;
        }

        partial void OnSelectedDriverChanged(Driver value)
        {
            (UpdateDriverCommand as IRelayCommand)?.NotifyCanExecuteChanged();
            (DeleteDriverCommand as IRelayCommand)?.NotifyCanExecuteChanged();
        }

        private async Task LoadDriverAvailabilitiesAsync()
        {
            try
            {
                var availabilities = await _availabilityService.GetDriverAvailabilitiesAsync();
                DriverAvailabilities.Clear();
                foreach (var info in availabilities)
                {
                    DriverAvailabilities.Add(new DriverAvailability
                    {
                        DriverId = info.DriverId,
                        DriverName = info.DriverName,
                        AvailableDates = info.AvailableDates
                    });
                }
                if (DriverAvailabilities.Count > 0)
                    SelectedAvailabilityDates = new ObservableCollection<DateTime>(DriverAvailabilities[0].AvailableDates);
                await _activityLogService.LogAsync("Loaded driver availabilities", "System");
            }
            catch (Exception ex)
            {
                await _activityLogService.LogAsync("Error loading driver availabilities", "System", ex.ToString());
            }
        }

        private void GenerateLicenseStatusReport()
        {
            LicenseStatusReport.Clear();
            foreach (var driver in Drivers)
            {
                var status = "Current";
                if (driver.LicenseExpiryDate.HasValue)
                {
                    var days = (driver.LicenseExpiryDate.Value - DateTime.Today).Days;
                    if (days < 0) status = "Expired";
                    else if (days < 30) status = "Expiring Soon";
                }
                else
                {
                    status = "Unknown";
                }
                LicenseStatusReport.Add(new DriverLicenseStatus
                {
                    DriverName = driver.FullName,
                    LicenseStatus = status,
                    LicenseExpiry = driver.LicenseExpiryDate
                });
            }
        }
    }

    public class DriverLicenseStatus
    {
        public string DriverName { get; set; } = string.Empty;
        public string LicenseStatus { get; set; } = string.Empty;
        public DateTime? LicenseExpiry { get; set; }
    }

    public class DriverAvailability
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public System.Collections.Generic.List<DateTime> AvailableDates { get; set; } = new();
    }
}
