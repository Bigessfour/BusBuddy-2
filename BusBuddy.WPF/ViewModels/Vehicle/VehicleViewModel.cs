using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace BusBuddy.WPF.ViewModels.Vehicle
{
    /// <summary>
    /// VehicleViewModel - Core service integration following Step 3 workflow
    /// Exposes properties/commands bound to Core services (BusService.cs)
    /// </summary>
    public partial class VehicleViewModel : BaseViewModel
    {
        private readonly IBusService _busService;
        private readonly ILogger<VehicleViewModel> _logger;

        #region Properties - MVVM Binding Ready

        private Bus? _selectedVehicle;
        private IEnumerable<Bus> _vehicles = new List<Bus>();

        public Bus? SelectedVehicle
        {
            get => _selectedVehicle;
            set => SetProperty(ref _selectedVehicle, value);
        }

        public IEnumerable<Bus> Vehicles
        {
            get => _vehicles;
            set => SetProperty(ref _vehicles, value);
        }

        [ObservableProperty]
        private Bus _currentVehicle = new();

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _statusMessage = "Ready";

        [ObservableProperty]
        private string _searchText = string.Empty;

        // Fleet statistics for mini-dashboard
        [ObservableProperty]
        private int _totalVehicles;

        [ObservableProperty]
        private int _activeVehicles;

        [ObservableProperty]
        private int _vehiclesInMaintenance;

        [ObservableProperty]
        private double _fleetUtilization;

        #endregion

        #region Commands - Core Service Integration

        public ICommand LoadVehiclesCommand { get; }
        public ICommand SaveVehicleCommand { get; }
        public ICommand DeleteVehicleCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand NewVehicleCommand { get; }

        #endregion

        public VehicleViewModel(IBusService busService, ILogger<VehicleViewModel> logger)
        {
            _busService = busService ?? throw new ArgumentNullException(nameof(busService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Initialize commands with Core service integration
            LoadVehiclesCommand = new AsyncRelayCommand(LoadVehiclesAsync);
            SaveVehicleCommand = new AsyncRelayCommand(SaveVehicleAsync, CanSaveVehicle);
            DeleteVehicleCommand = new AsyncRelayCommand(DeleteVehicleAsync, CanDeleteVehicle);
            RefreshCommand = new AsyncRelayCommand(RefreshDataAsync);
            SearchCommand = new AsyncRelayCommand(SearchVehiclesAsync);
            NewVehicleCommand = new RelayCommand(CreateNewVehicle);

            // Auto-load data on initialization
            _ = LoadVehiclesAsync();
        }

        #region Core Service Integration Methods

        /// <summary>
        /// Core service integration: await _busService.GetAllAsync() in VM
        /// </summary>
        private async Task LoadVehiclesAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Loading vehicles from Core service...";
                _logger.LogInformation("Loading vehicles via BusService");

                // Core service integration - as specified in Step 3
                var vehicles = await _busService.GetAllBusesAsync();

                Vehicles.Clear();
                foreach (var vehicle in vehicles)
                {
                    Vehicles.Add(vehicle);
                }

                // Update fleet statistics for mini-dashboard
                UpdateFleetStatistics();

                StatusMessage = $"Loaded {Vehicles.Count} vehicles successfully";
                _logger.LogInformation("Successfully loaded {VehicleCount} vehicles", Vehicles.Count);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading vehicles: {ex.Message}";
                _logger.LogError(ex, "Failed to load vehicles from BusService");

                // Fallback: Load sample data for development
                LoadSampleData();
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Core service integration: Save via _busService
        /// </summary>
        private async Task SaveVehicleAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Saving vehicle...";
                _logger.LogInformation("Saving vehicle: {VehicleNumber}", CurrentVehicle.BusNumber);

                Bus savedVehicle;
                if (CurrentVehicle.VehicleId == 0)
                {
                    // New vehicle - Core service integration
                    savedVehicle = await _busService.AddBusAsync(CurrentVehicle);
                    Vehicles.Add(savedVehicle);
                    StatusMessage = $"Vehicle {savedVehicle.BusNumber} added successfully";
                }
                else
                {
                    // Update existing - Core service integration
                    var success = await _busService.UpdateBusAsync(CurrentVehicle);
                    if (success)
                    {
                        // Update in collection
                        var index = Vehicles.ToList().FindIndex(v => v.VehicleId == CurrentVehicle.VehicleId);
                        if (index >= 0)
                        {
                            Vehicles[index] = CurrentVehicle;
                        }
                        StatusMessage = $"Vehicle {CurrentVehicle.BusNumber} updated successfully";
                    }
                    savedVehicle = CurrentVehicle;
                }

                SelectedVehicle = savedVehicle;
                UpdateFleetStatistics();

                _logger.LogInformation("Successfully saved vehicle: {VehicleNumber}", savedVehicle.BusNumber);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error saving vehicle: {ex.Message}";
                _logger.LogError(ex, "Failed to save vehicle: {VehicleNumber}", CurrentVehicle.BusNumber);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Core service integration: Delete via _busService
        /// </summary>
        private async Task DeleteVehicleAsync()
        {
            if (SelectedVehicle == null)
            {
                return;
            }

            try
            {
                IsBusy = true;
                StatusMessage = $"Deleting vehicle {SelectedVehicle.BusNumber}...";
                _logger.LogInformation("Deleting vehicle: {VehicleNumber}", SelectedVehicle.BusNumber);

                // Core service integration
                var success = await _busService.DeleteBusAsync(SelectedVehicle.VehicleId);
                if (success)
                {
                    Vehicles.Remove(SelectedVehicle);
                    SelectedVehicle = null;
                    CurrentVehicle = new Bus();
                    UpdateFleetStatistics();
                    StatusMessage = "Vehicle deleted successfully";
                    _logger.LogInformation("Successfully deleted vehicle");
                }
                else
                {
                    StatusMessage = "Failed to delete vehicle";
                    _logger.LogWarning("Vehicle deletion failed - service returned false");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting vehicle: {ex.Message}";
                _logger.LogError(ex, "Failed to delete vehicle: {VehicleNumber}", SelectedVehicle.BusNumber);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Search vehicles with Core service integration
        /// </summary>
        private async Task SearchVehiclesAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadVehiclesAsync();
                return;
            }

            try
            {
                IsBusy = true;
                StatusMessage = $"Searching for '{SearchText}'...";
                _logger.LogInformation("Searching vehicles: {SearchText}", SearchText);

                // For MVP: Filter loaded vehicles. Phase 2: Server-side search
                var filtered = Vehicles.Where(v =>
                    v.BusNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    v.Make.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    v.Model.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    v.LicenseNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                ).ToList();

                Vehicles.Clear();
                foreach (var vehicle in filtered)
                {
                    Vehicles.Add(vehicle);
                }

                StatusMessage = $"Found {Vehicles.Count} vehicles matching '{SearchText}'";
                UpdateFleetStatistics();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Search error: {ex.Message}";
                _logger.LogError(ex, "Failed to search vehicles");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RefreshDataAsync()
        {
            SearchText = string.Empty;
            await LoadVehiclesAsync();
        }

        #endregion

        #region Command Validation

        private bool CanSaveVehicle()
        {
            return CurrentVehicle != null &&
                   !string.IsNullOrWhiteSpace(CurrentVehicle.BusNumber) &&
                   !IsBusy;
        }

        private bool CanDeleteVehicle()
        {
            return SelectedVehicle != null &&
                   SelectedVehicle.VehicleId > 0 &&
                   !IsBusy;
        }

        #endregion

        #region Helper Methods

        private void CreateNewVehicle()
        {
            CurrentVehicle = new Bus
            {
                BusNumber = $"BUS{Vehicles.Count + 1:000}",
                Make = "",
                Model = "",
                Year = DateTime.Now.Year,
                Capacity = 40,
                Status = "Active"
            };

            StatusMessage = "Ready to create new vehicle";
            _logger.LogInformation("Created new vehicle template");
        }

        private void UpdateFleetStatistics()
        {
            TotalVehicles = Vehicles.Count;
            ActiveVehicles = Vehicles.Count(v => v.Status == "Active");
            VehiclesInMaintenance = Vehicles.Count(v => v.Status == "Maintenance");
            FleetUtilization = TotalVehicles > 0 ? (double)ActiveVehicles / TotalVehicles * 100 : 0;
        }

        private void LoadSampleData()
        {
            var sampleVehicles = new List<Bus>
            {
                new() { VehicleId = 1, BusNumber = "BUS001", Make = "Ford", Model = "Transit", Year = 2022, Capacity = 40, Status = "Active", LicenseNumber = "ABC-123" },
                new() { VehicleId = 2, BusNumber = "BUS002", Make = "Chevrolet", Model = "Express", Year = 2021, Capacity = 35, Status = "Active", LicenseNumber = "DEF-456" },
                new() { VehicleId = 3, BusNumber = "BUS003", Make = "Mercedes", Model = "Sprinter", Year = 2023, Capacity = 20, Status = "Maintenance", LicenseNumber = "GHI-789" },
                new() { VehicleId = 4, BusNumber = "BUS004", Make = "Ford", Model = "E-Series", Year = 2020, Capacity = 45, Status = "Active", LicenseNumber = "JKL-012" },
                new() { VehicleId = 5, BusNumber = "BUS005", Make = "Isuzu", Model = "NPR", Year = 2022, Capacity = 30, Status = "OutOfService", LicenseNumber = "MNO-345" }
            };

            Vehicles.Clear();
            foreach (var vehicle in sampleVehicles)
            {
                Vehicles.Add(vehicle);
            }

            UpdateFleetStatistics();
            StatusMessage = "Sample data loaded (Core service unavailable)";
            _logger.LogInformation("Loaded sample data as fallback");
        }

        #endregion

        #region Property Change Handlers

        partial void OnSelectedVehicleChanged(Bus? value)
        {
            if (value != null)
            {
                // Clone for editing to avoid direct binding issues
                CurrentVehicle = new Bus
                {
                    VehicleId = value.VehicleId,
                    BusNumber = value.BusNumber,
                    Make = value.Make,
                    Model = value.Model,
                    Year = value.Year,
                    Capacity = value.FuelCapacity,
                    Status = value.Status,
                    LicenseNumber = value.LicenseNumber
                };
            }

            // Update command states
            ((AsyncRelayCommand)SaveVehicleCommand).NotifyCanExecuteChanged();
            ((AsyncRelayCommand)DeleteVehicleCommand).NotifyCanExecuteChanged();
        }

        partial void OnCurrentVehicleChanged(Bus value)
        {
            ((AsyncRelayCommand)SaveVehicleCommand).NotifyCanExecuteChanged();
        }

        partial void OnIsBusyChanged(bool value)
        {
            // Update all command states when busy state changes
            ((AsyncRelayCommand)SaveVehicleCommand).NotifyCanExecuteChanged();
            ((AsyncRelayCommand)DeleteVehicleCommand).NotifyCanExecuteChanged();
        }

        #endregion
    }
}
