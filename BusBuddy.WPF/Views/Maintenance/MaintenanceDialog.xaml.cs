using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusBuddy.WPF.Views.Maintenance
{
    public partial class MaintenanceDialog : Window, INotifyPropertyChanged
    {
        private readonly IBusService _busService;
        private readonly ILogger<MaintenanceDialog>? _logger;
        private BusBuddy.Core.Models.Maintenance _maintenance;
        private BusBuddy.Core.Models.Bus? _selectedBus;
        private bool _isValid = false;

        // Observable collections for ComboBox options
        public ObservableCollection<BusBuddy.Core.Models.Bus> AvailableBuses { get; } = new ObservableCollection<BusBuddy.Core.Models.Bus>();
        public ObservableCollection<string> MaintenanceTypes { get; } = new ObservableCollection<string>
        {
            "Tires", "Windshield", "Alignment", "Mechanical", "Car Wash", "Cleaning", "Accessory Install", "Oil Change", "Brakes", "Transmission", "Engine", "Electrical", "Other"
        };
        public ObservableCollection<string> StatusOptions { get; } = new ObservableCollection<string>
        {
            "Scheduled", "In Progress", "Completed", "Cancelled", "Deferred"
        };
        public ObservableCollection<string> PriorityOptions { get; } = new ObservableCollection<string>
        {
            "Low", "Normal", "High", "Emergency"
        };

        public string DialogTitle { get; private set; }

        public BusBuddy.Core.Models.Maintenance Maintenance
        {
            get => _maintenance;
            set
            {
                _maintenance = value;
                OnPropertyChanged();
                ValidateForm();
            }
        }

        public BusBuddy.Core.Models.Bus? SelectedBus
        {
            get => _selectedBus;
            set
            {
                _selectedBus = value;
                if (_selectedBus != null)
                {
                    Maintenance.VehicleId = _selectedBus.VehicleId;
                }
                OnPropertyChanged();
                ValidateForm();
            }
        }

        public bool IsValid
        {
            get => _isValid;
            set
            {
                _isValid = value;
                OnPropertyChanged();
            }
        }

        public MaintenanceDialog(BusBuddy.Core.Models.Maintenance maintenance, IBusService busService, ILogger<MaintenanceDialog>? logger = null)
        {
            _maintenance = maintenance ?? throw new ArgumentNullException(nameof(maintenance));
            _busService = busService ?? throw new ArgumentNullException(nameof(busService));
            _logger = logger;

            DialogTitle = maintenance.MaintenanceId == 0 ? "Add Maintenance Record" : "Edit Maintenance Record";

            InitializeComponent();
            DataContext = this;

            // Ensure the model has default values if it's new
            if (maintenance.MaintenanceId == 0)
            {
                maintenance.Date = DateTime.Now;
                maintenance.Status = "Scheduled";
                maintenance.Priority = "Normal";
            }

            LoadBusesAsync();
        }

        private async void LoadBusesAsync()
        {
            try
            {
                AvailableBuses.Clear();
                var buses = await _busService.GetAllBusesAsync();
                foreach (var bus in buses)
                {
                    AvailableBuses.Add(bus);
                }

                // Select the bus that matches the maintenance record
                _selectedBus = AvailableBuses.FirstOrDefault(b => b.VehicleId == Maintenance.VehicleId);
                OnPropertyChanged(nameof(SelectedBus));

                _logger?.LogInformation("Loaded {BusCount} buses for maintenance dialog", AvailableBuses.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading buses for maintenance dialog");
                MessageBox.Show($"Error loading buses: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ValidateForm()
        {
            IsValid =
                SelectedBus != null &&
                !string.IsNullOrWhiteSpace(Maintenance.MaintenanceCompleted) &&
                !string.IsNullOrWhiteSpace(Maintenance.Vendor) &&
                !string.IsNullOrWhiteSpace(Maintenance.Status);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Update properties for timestamp tracking
            Maintenance.UpdatedDate = DateTime.UtcNow;
            if (Maintenance.MaintenanceId == 0)
            {
                Maintenance.CreatedDate = DateTime.UtcNow;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
