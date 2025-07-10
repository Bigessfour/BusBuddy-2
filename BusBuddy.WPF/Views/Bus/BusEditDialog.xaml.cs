using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BusBuddy.Core.Models;
using BusBuddy.WPF.ViewModels;

namespace BusBuddy.WPF.Views.Bus
{
    public partial class BusEditDialog : Window, INotifyPropertyChanged
    {
        private BusBuddy.Core.Models.Bus _bus = null!;
        private string _dialogTitle = string.Empty;
        private bool _hasValidationErrors;
        private string _validationMessage = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public BusBuddy.Core.Models.Bus Bus
        {
            get => _bus;
            set
            {
                _bus = value;
                OnPropertyChanged(nameof(Bus));
            }
        }

        public string DialogTitle
        {
            get => _dialogTitle;
            set
            {
                _dialogTitle = value;
                OnPropertyChanged(nameof(DialogTitle));
            }
        }

        public bool HasValidationErrors
        {
            get => _hasValidationErrors;
            set
            {
                _hasValidationErrors = value;
                OnPropertyChanged(nameof(HasValidationErrors));
            }
        }

        public string ValidationMessage
        {
            get => _validationMessage;
            set
            {
                _validationMessage = value;
                OnPropertyChanged(nameof(ValidationMessage));
            }
        }

        public BusEditDialog(BusBuddy.Core.Models.Bus bus, bool isNewBus)
        {
            InitializeComponent();
            DataContext = this;

            // Create a clone of the bus to avoid modifying the original until save
            Bus = CloneBus(bus);

            // Set title based on whether we're adding or editing
            DialogTitle = isNewBus ? "Add New Bus" : "Edit Bus Details";

            // Set up validation
            Bus.PropertyChanged += Bus_PropertyChanged;
        }

        private void Bus_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Reset validation when properties change
            HasValidationErrors = false;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateBus())
            {
                DialogResult = true;
                Close();
            }
        }

        private bool ValidateBus()
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(Bus.BusNumber))
            {
                ShowValidationError("Bus Number is required.");
                return false;
            }

            if (Bus.Year < 1900 || Bus.Year > DateTime.Now.Year + 5)
            {
                ShowValidationError("Year must be between 1900 and " + (DateTime.Now.Year + 5) + ".");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Bus.Make))
            {
                ShowValidationError("Make is required.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Bus.Model))
            {
                ShowValidationError("Model is required.");
                return false;
            }

            if (Bus.SeatingCapacity < 1)
            {
                ShowValidationError("Seating Capacity must be at least 1.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Bus.VINNumber))
            {
                ShowValidationError("VIN Number is required.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Bus.LicenseNumber))
            {
                ShowValidationError("License Number is required.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Bus.Status))
            {
                ShowValidationError("Status is required.");
                return false;
            }

            // All validations passed
            return true;
        }

        private void ShowValidationError(string message)
        {
            ValidationMessage = message;
            HasValidationErrors = true;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Create a deep copy of the bus to avoid modifying the original until save
        private BusBuddy.Core.Models.Bus CloneBus(BusBuddy.Core.Models.Bus original)
        {
            if (original == null)
            {
                return new BusBuddy.Core.Models.Bus
                {
                    Status = "Active", // Default status for new buses
                    Year = DateTime.Now.Year, // Default to current year
                };
            }

            return new BusBuddy.Core.Models.Bus
            {
                VehicleId = original.VehicleId,
                BusNumber = original.BusNumber,
                Year = original.Year,
                Make = original.Make,
                Model = original.Model,
                SeatingCapacity = original.SeatingCapacity,
                VINNumber = original.VINNumber,
                LicenseNumber = original.LicenseNumber,
                DateLastInspection = original.DateLastInspection,
                CurrentOdometer = original.CurrentOdometer,
                Status = original.Status,
                PurchaseDate = original.PurchaseDate,
                PurchasePrice = original.PurchasePrice,
                InsurancePolicyNumber = original.InsurancePolicyNumber,
                InsuranceExpiryDate = original.InsuranceExpiryDate,
                CreatedDate = original.CreatedDate,
                UpdatedDate = DateTime.UtcNow,
                CreatedBy = original.CreatedBy,
                UpdatedBy = original.UpdatedBy,
                Department = original.Department,
                FleetType = original.FleetType,
                FuelCapacity = original.FuelCapacity,
                FuelType = original.FuelType,
                MilesPerGallon = original.MilesPerGallon,
                NextMaintenanceDue = original.NextMaintenanceDue,
                NextMaintenanceMileage = original.NextMaintenanceMileage,
                LastServiceDate = original.LastServiceDate,
                SpecialEquipment = original.SpecialEquipment,
                GPSTracking = original.GPSTracking,
                GPSDeviceId = original.GPSDeviceId,
                Notes = original.Notes
            };
        }
    }
}
