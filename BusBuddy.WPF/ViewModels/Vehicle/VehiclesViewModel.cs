using BusBuddy.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace BusBuddy.WPF.ViewModels.Vehicle
{
    /// <summary>
    /// Phase 1 ViewModel for Vehicle Management - Simple and functional
    /// </summary>
    public class VehiclesViewModel : BaseViewModel
    {
        #region Fields
        private ObservableCollection<Bus> _vehicles = new();
        private Bus? _selectedVehicle;
        #endregion

        #region Properties
        public ObservableCollection<Bus> Vehicles
        {
            get => _vehicles;
            set => SetProperty(ref _vehicles, value);
        }

        public Bus? SelectedVehicle
        {
            get => _selectedVehicle;
            set => SetProperty(ref _selectedVehicle, value);
        }
        #endregion

        #region Commands
        public ICommand AddVehicleCommand { get; }
        public ICommand EditVehicleCommand { get; }
        public ICommand DeleteVehicleCommand { get; }
        #endregion

        #region Constructor
        public VehiclesViewModel()
        {
            // Initialize commands - Phase 1 simple approach
            AddVehicleCommand = new RelayCommand(AddVehicle);
            EditVehicleCommand = new RelayCommand(EditVehicle, CanEditVehicle);
            DeleteVehicleCommand = new RelayCommand(DeleteVehicle, CanDeleteVehicle);

            // Load sample data
            LoadVehicleData();
        }
        #endregion

        #region Command Methods
        private void AddVehicle()
        {
            try
            {
                // Phase 1: Simple add functionality
                var newVehicle = new VehicleRecord
                {
                    Id = Vehicles.Count + 1,
                    BusNumber = "NEW",
                    LicensePlate = "NEW123",
                    Make = "New",
                    Model = "Bus",
                    Year = DateTime.Now.Year,
                    Capacity = 72,
                    Status = "Active",
                    Mileage = 0
                };

                Vehicles.Add(newVehicle);
                SelectedVehicle = newVehicle;

                Console.WriteLine($"Added new vehicle: {newVehicle.BusNumber}");
            }
            catch (Exception ex)
            {
                // Phase 1 error handling
                Console.WriteLine($"Error adding vehicle: {ex.Message}");
                MessageBox.Show($"Error adding vehicle: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditVehicle()
        {
            try
            {
                if (SelectedVehicle != null)
                {
                    // Phase 1: Simple edit notification
                    Console.WriteLine($"Edit requested for: {SelectedVehicle.BusNumber}");
                    MessageBox.Show($"Edit functionality for {SelectedVehicle.BusNumber} - {SelectedVehicle.Make} {SelectedVehicle.Model}\n\n(Phase 2: Full edit dialog)",
                                  "Edit Vehicle", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error editing vehicle: {ex.Message}");
                MessageBox.Show($"Error editing vehicle: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteVehicle()
        {
            try
            {
                if (SelectedVehicle != null)
                {
                    var result = MessageBox.Show($"Delete vehicle {SelectedVehicle.BusNumber}?",
                                               "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        Console.WriteLine($"Deleting vehicle: {SelectedVehicle.BusNumber}");
                        Vehicles.Remove(SelectedVehicle);
                        SelectedVehicle = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting vehicle: {ex.Message}");
                MessageBox.Show($"Error deleting vehicle: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanEditVehicle() => SelectedVehicle != null;
        private bool CanDeleteVehicle() => SelectedVehicle != null;
        #endregion

        #region Data Loading - Phase 1
        private void LoadVehicleData()
        {
            try
            {
                // Phase 1: Sample vehicle data for demonstration
                var sampleData = new[]
                {
                    new VehicleRecord { Id = 1, BusNumber = "Bus-001", LicensePlate = "SCH-001", Make = "Blue Bird", Model = "Vision", Year = 2019, Capacity = 72, Status = "Active", Mileage = 45230 },
                    new VehicleRecord { Id = 2, BusNumber = "Bus-002", LicensePlate = "SCH-002", Make = "Thomas Built", Model = "Saf-T-Liner C2", Year = 2020, Capacity = 71, Status = "Active", Mileage = 38750 },
                    new VehicleRecord { Id = 3, BusNumber = "Bus-003", LicensePlate = "SCH-003", Make = "IC Bus", Model = "CE Series", Year = 2018, Capacity = 77, Status = "Maintenance", Mileage = 52100 },
                    new VehicleRecord { Id = 4, BusNumber = "Bus-004", LicensePlate = "SCH-004", Make = "Blue Bird", Model = "All American", Year = 2021, Capacity = 72, Status = "Active", Mileage = 28900 },
                    new VehicleRecord { Id = 5, BusNumber = "Bus-005", LicensePlate = "SCH-005", Make = "Thomas Built", Model = "Saf-T-Liner HDX", Year = 2017, Capacity = 78, Status = "Active", Mileage = 67500 },
                    new VehicleRecord { Id = 6, BusNumber = "Bus-006", LicensePlate = "SCH-006", Make = "IC Bus", Model = "RE Series", Year = 2022, Capacity = 77, Status = "Active", Mileage = 15600 },
                    new VehicleRecord { Id = 7, BusNumber = "Bus-007", LicensePlate = "SCH-007", Make = "Blue Bird", Model = "Vision", Year = 2019, Capacity = 72, Status = "Active", Mileage = 41200 },
                    new VehicleRecord { Id = 8, BusNumber = "Bus-008", LicensePlate = "SCH-008", Make = "Thomas Built", Model = "Saf-T-Liner C2", Year = 2020, Capacity = 71, Status = "Out of Service", Mileage = 43800 },
                    new VehicleRecord { Id = 9, BusNumber = "Bus-009", LicensePlate = "SCH-009", Make = "IC Bus", Model = "CE Series", Year = 2018, Capacity = 77, Status = "Active", Mileage = 49300 },
                    new VehicleRecord { Id = 10, BusNumber = "Bus-010", LicensePlate = "SCH-010", Make = "Blue Bird", Model = "All American", Year = 2021, Capacity = 72, Status = "Active", Mileage = 31750 },
                    new VehicleRecord { Id = 11, BusNumber = "Bus-011", LicensePlate = "SCH-011", Make = "Thomas Built", Model = "Saf-T-Liner HDX", Year = 2016, Capacity = 78, Status = "Active", Mileage = 78200 },
                    new VehicleRecord { Id = 12, BusNumber = "Bus-012", LicensePlate = "SCH-012", Make = "IC Bus", Model = "RE Series", Year = 2022, Capacity = 77, Status = "Active", Mileage = 12400 },
                    new VehicleRecord { Id = 13, BusNumber = "Bus-013", LicensePlate = "SCH-013", Make = "Blue Bird", Model = "Vision", Year = 2017, Capacity = 72, Status = "Maintenance", Mileage = 58900 },
                    new VehicleRecord { Id = 14, BusNumber = "Bus-014", LicensePlate = "SCH-014", Make = "Thomas Built", Model = "Saf-T-Liner C2", Year = 2019, Capacity = 71, Status = "Active", Mileage = 36700 },
                    new VehicleRecord { Id = 15, BusNumber = "Bus-015", LicensePlate = "SCH-015", Make = "IC Bus", Model = "CE Series", Year = 2023, Capacity = 77, Status = "Active", Mileage = 8500 }
                };

                Vehicles.Clear();
                foreach (var vehicle in sampleData)
                {
                    Vehicles.Add(vehicle);
                }

                Console.WriteLine($"Loaded {Vehicles.Count} vehicles for Phase 1");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading vehicle data: {ex.Message}");
                MessageBox.Show($"Error loading vehicle data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }

    #region Data Models - Phase 1
    public class VehicleRecord
    {
        public int Id { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Capacity { get; set; }
        public string Status { get; set; } = string.Empty;
        public int Mileage { get; set; }

        public string DisplayName => $"{BusNumber} - {Make} {Model}";
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _execute();
    }
    #endregion
}
