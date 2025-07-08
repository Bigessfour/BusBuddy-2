using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BusBuddy.Core.Models;
using System.Collections.Generic;

namespace BusBuddy.WPF.ViewModels
{

    // Demo/mock driver class for ViewModel use


    public class DriverAvailability
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public List<DateTime> AvailableDates { get; set; } = new();
    }

    public class DriverManagementViewModel : INotifyPropertyChanged
    {

        private readonly BusBuddy.Core.Services.IBusService _busService;
        private readonly BusBuddy.WPF.Services.IDriverAvailabilityService _availabilityService;

        public ObservableCollection<Driver> Drivers { get; set; } = new();
        public ObservableCollection<DriverAvailability> DriverAvailabilities { get; set; } = new();
        public ICommand GenerateLicenseStatusReportCommand { get; }
        public ObservableCollection<DriverLicenseStatus> LicenseStatusReport { get; set; } = new();

        // Calendar properties
        public DateTime CalendarStartDate { get; set; } = DateTime.Today.AddDays(-7);
        public DateTime CalendarEndDate { get; set; } = DateTime.Today.AddDays(30);
        private ObservableCollection<DateTime> _selectedAvailabilityDates = new();
        public ObservableCollection<DateTime> SelectedAvailabilityDates
        {
            get => _selectedAvailabilityDates;
            set { _selectedAvailabilityDates = value; OnPropertyChanged(); }
        }



        private readonly BusBuddy.Core.Services.IActivityLogService _activityLogService;

        public DriverManagementViewModel(BusBuddy.Core.Services.IBusService busService, BusBuddy.WPF.Services.IDriverAvailabilityService availabilityService, BusBuddy.Core.Services.IActivityLogService activityLogService)
        {
            _busService = busService;
            _availabilityService = availabilityService;
            _activityLogService = activityLogService;
            _ = LoadDriversAsync();
            _ = LoadDriverAvailabilitiesAsync();
            GenerateLicenseStatusReportCommand = new RelayCommand(_ => GenerateLicenseStatusReport());
        }


        private async Task LoadDriversAsync()
        {
            try
            {
                Drivers.Clear();
                var drivers = await _busService.GetAllDriversAsync();
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


        private async Task LoadDriverAvailabilitiesAsync()
        {
            try
            {
                DriverAvailabilities.Clear();
                var availabilities = await _availabilityService.GetDriverAvailabilitiesAsync();
                foreach (var info in availabilities)
                {
                    DriverAvailabilities.Add(new DriverAvailability
                    {
                        DriverId = info.DriverId,
                        DriverName = info.DriverName,
                        AvailableDates = info.AvailableDates
                    });
                }
                // Show the first driver's dates in the calendar if available
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
                    DriverName = driver.DriverName,
                    LicenseStatus = status,
                    LicenseExpiry = driver.LicenseExpiryDate
                });
            }
            OnPropertyChanged(nameof(LicenseStatusReport));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DriverLicenseStatus
    {
        public string DriverName { get; set; } = string.Empty;
        public string LicenseStatus { get; set; } = string.Empty;
        public DateTime? LicenseExpiry { get; set; }
    }

    // Use existing RelayCommand from shared ViewModel base
}
