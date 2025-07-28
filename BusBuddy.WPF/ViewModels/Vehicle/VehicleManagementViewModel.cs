using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BusBuddy.WPF.ViewModels.Vehicle
{
    /// <summary>
    /// ViewModel for managing vehicles/buses in Phase 1
    /// Provides CRUD operations and fleet status monitoring
    /// </summary>
    public partial class VehicleManagementViewModel : ObservableObject
    {
        private readonly IBusService _busService;

        [ObservableProperty]
        private ObservableCollection<Bus> _vehicles = new();

        [ObservableProperty]
        private Bus? _selectedVehicle;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        public ICommand LoadVehiclesCommand { get; }
        public ICommand AddVehicleCommand { get; }
        public ICommand UpdateVehicleCommand { get; }
        public ICommand DeleteVehicleCommand { get; }
        public ICommand SearchVehiclesCommand { get; }
        public ICommand RefreshCommand { get; }

        public VehicleManagementViewModel(IBusService busService)
        {
            _busService = busService ?? throw new ArgumentNullException(nameof(busService));

            LoadVehiclesCommand = new AsyncRelayCommand(LoadVehiclesAsync);
            AddVehicleCommand = new AsyncRelayCommand(AddVehicleAsync, CanAddVehicle);
            UpdateVehicleCommand = new AsyncRelayCommand(UpdateVehicleAsync, CanUpdateVehicle);
            DeleteVehicleCommand = new AsyncRelayCommand(DeleteVehicleAsync, CanDeleteVehicle);
            SearchVehiclesCommand = new AsyncRelayCommand(SearchVehiclesAsync);
            RefreshCommand = new AsyncRelayCommand(RefreshAsync);

            // Initialize with loading
            _ = LoadVehiclesAsync();
        }

        /// <summary>
        /// Load all vehicles from the service
        /// </summary>
        private async Task LoadVehiclesAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading vehicles...";

                var vehicles = await _busService.GetAllBusesAsync();

                Vehicles.Clear();
                foreach (var vehicle in vehicles)
                {
                    Vehicles.Add(vehicle);
                }

                StatusMessage = $"Loaded {Vehicles.Count} vehicles";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading vehicles: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Add a new vehicle
        /// </summary>
        private async Task AddVehicleAsync()
        {
            if (SelectedVehicle == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = "Adding vehicle...";

                var addedVehicle = await _busService.AddBusAsync(SelectedVehicle);
                Vehicles.Add(addedVehicle);

                SelectedVehicle = new Bus(); // Reset for next entry
                StatusMessage = "Vehicle added successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error adding vehicle: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Update selected vehicle
        /// </summary>
        private async Task UpdateVehicleAsync()
        {
            if (SelectedVehicle == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = "Updating vehicle...";

                var updateSucceeded = await _busService.UpdateBusAsync(SelectedVehicle);

                if (updateSucceeded)
                {
                    // Update in collection - find and replace the vehicle
                    var index = Vehicles.ToList().FindIndex(v => v.VehicleId == SelectedVehicle.VehicleId);
                    if (index >= 0)
                    {
                        Vehicles[index] = SelectedVehicle;
                    }
                    StatusMessage = "Vehicle updated successfully";
                }
                else
                {
                    StatusMessage = "Vehicle update failed - no changes detected";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating vehicle: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Delete selected vehicle
        /// </summary>
        private async Task DeleteVehicleAsync()
        {
            if (SelectedVehicle == null) return;

            try
            {
                IsLoading = true;
                StatusMessage = "Deleting vehicle...";

                var deleteSucceeded = await _busService.DeleteBusAsync(SelectedVehicle.VehicleId);
                if (deleteSucceeded)
                {
                    Vehicles.Remove(SelectedVehicle);
                    SelectedVehicle = null;
                    StatusMessage = "Vehicle deleted successfully";
                }
                else
                {
                    StatusMessage = "Vehicle deletion failed - vehicle may not exist";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting vehicle: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Search vehicles by text
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
                IsLoading = true;
                StatusMessage = "Searching vehicles...";

                var allVehicles = await _busService.GetAllBusesAsync();
                var filteredVehicles = allVehicles.Where(v =>
                    v.BusNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    v.Make.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    v.Model.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    v.LicenseNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                ).ToList();

                Vehicles.Clear();
                foreach (var vehicle in filteredVehicles)
                {
                    Vehicles.Add(vehicle);
                }

                StatusMessage = $"Found {Vehicles.Count} vehicles matching '{SearchText}'";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error searching vehicles: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Refresh the vehicle list
        /// </summary>
        private async Task RefreshAsync()
        {
            SearchText = string.Empty;
            await LoadVehiclesAsync();
        }

        /// <summary>
        /// Check if a vehicle can be added
        /// </summary>
        private bool CanAddVehicle()
        {
            return SelectedVehicle != null &&
                   !string.IsNullOrWhiteSpace(SelectedVehicle.BusNumber) &&
                   !IsLoading;
        }

        /// <summary>
        /// Check if a vehicle can be updated
        /// </summary>
        private bool CanUpdateVehicle()
        {
            return SelectedVehicle != null &&
                   SelectedVehicle.VehicleId > 0 &&
                   !IsLoading;
        }

        /// <summary>
        /// Check if a vehicle can be deleted
        /// </summary>
        private bool CanDeleteVehicle()
        {
            return SelectedVehicle != null &&
                   SelectedVehicle.VehicleId > 0 &&
                   !IsLoading;
        }

        /// <summary>
        /// Property change notification for selected vehicle
        /// </summary>
        partial void OnSelectedVehicleChanged(Bus? value)
        {
            // Refresh command states
            ((AsyncRelayCommand)AddVehicleCommand).NotifyCanExecuteChanged();
            ((AsyncRelayCommand)UpdateVehicleCommand).NotifyCanExecuteChanged();
            ((AsyncRelayCommand)DeleteVehicleCommand).NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Get vehicle fleet summary for dashboard
        /// </summary>
        public VehicleFleetSummary GetFleetSummary()
        {
            return new VehicleFleetSummary
            {
                TotalVehicles = Vehicles.Count,
                ActiveVehicles = Vehicles.Count(v => v.Status == "Active"),
                InactiveVehicles = Vehicles.Count(v => v.Status != "Active"),
                VehiclesNeedingInspection = Vehicles.Count(v => v.InspectionStatus != "Current"),
                AverageFleetAge = Vehicles.Any() ? (int)Vehicles.Average(v => DateTime.Now.Year - v.Year) : 0
            };
        }
    }

    /// <summary>
    /// Summary data for vehicle fleet dashboard
    /// </summary>
    public class VehicleFleetSummary
    {
        public int TotalVehicles { get; set; }
        public int ActiveVehicles { get; set; }
        public int InactiveVehicles { get; set; }
        public int VehiclesNeedingInspection { get; set; }
        public int AverageFleetAge { get; set; }
    }
}
