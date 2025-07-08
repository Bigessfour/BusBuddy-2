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
    public class DemoDriver : Driver
    {
        public DateTime? LicenseExpiry { get; set; }
    }

    public class DriverAvailability
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public List<DateTime> AvailableDates { get; set; } = new();
    }

    public class DriverManagementViewModel : INotifyPropertyChanged
    {

        public ObservableCollection<DemoDriver> Drivers { get; set; } = new();
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

        public DriverManagementViewModel()
        {
            // Placeholder: Load drivers and assignments from service
            LoadDrivers();
            LoadDriverAvailabilities();
            GenerateLicenseStatusReportCommand = new RelayCommand(GenerateLicenseStatusReport);
        }

        private void LoadDrivers()
        {
            // TODO: Replace with real data service
            Drivers.Clear();
            Drivers.Add(new DemoDriver { DriverId = 1, DriverName = "Alice Smith", Status = "Active", LicenseExpiry = DateTime.Today.AddDays(10) });
            Drivers.Add(new DemoDriver { DriverId = 2, DriverName = "Bob Jones", Status = "Active", LicenseExpiry = DateTime.Today.AddDays(-5) });
            Drivers.Add(new DemoDriver { DriverId = 3, DriverName = "Carol Lee", Status = "Inactive", LicenseExpiry = DateTime.Today.AddDays(40) });
        }

        private void LoadDriverAvailabilities()
        {
            // Placeholder: Simulate driver availability
            DriverAvailabilities.Clear();
            DriverAvailabilities.Add(new DriverAvailability { DriverId = 1, DriverName = "Alice Smith", AvailableDates = new List<DateTime> { DateTime.Today, DateTime.Today.AddDays(1), DateTime.Today.AddDays(2) } });
            DriverAvailabilities.Add(new DriverAvailability { DriverId = 2, DriverName = "Bob Jones", AvailableDates = new List<DateTime> { DateTime.Today.AddDays(3), DateTime.Today.AddDays(4) } });
            DriverAvailabilities.Add(new DriverAvailability { DriverId = 3, DriverName = "Carol Lee", AvailableDates = new List<DateTime> { DateTime.Today.AddDays(5) } });
            // For demo, just show Alice's dates in the calendar
            SelectedAvailabilityDates = new ObservableCollection<DateTime>(DriverAvailabilities[0].AvailableDates);
        }

        private void GenerateLicenseStatusReport()
        {
            LicenseStatusReport.Clear();
            foreach (var driver in Drivers)
            {
                var status = "Current";
                if (driver.LicenseExpiry.HasValue)
                {
                    var days = (driver.LicenseExpiry.Value - DateTime.Today).Days;
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
                    LicenseExpiry = driver.LicenseExpiry
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
