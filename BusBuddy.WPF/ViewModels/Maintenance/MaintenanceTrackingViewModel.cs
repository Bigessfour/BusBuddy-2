using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace BusBuddy.WPF.ViewModels
{
    public class MaintenanceTrackingViewModel : BaseInDevelopmentViewModel
    {
        private readonly IMaintenanceService _maintenanceService;
        private readonly IBusService _busService;

        public ObservableCollection<Maintenance> MaintenanceRecords { get; } = new();
        public ObservableCollection<Bus> AvailableBuses { get; } = new();

        public BusBuddy.WPF.RelayCommand AddCommand { get; }
        public BusBuddy.WPF.RelayCommand EditCommand { get; }
        public BusBuddy.WPF.RelayCommand DeleteCommand { get; }

        private Maintenance? _selectedRecord;
        public Maintenance? SelectedRecord
        {
            get => _selectedRecord;
            set { _selectedRecord = value; OnPropertyChanged(); }
        }

        public MaintenanceTrackingViewModel(IMaintenanceService maintenanceService, IBusService busService, ILogger<MaintenanceTrackingViewModel>? logger = null)
            : base(logger)
        {
            _maintenanceService = maintenanceService;
            _busService = busService;

            AddCommand = new BusBuddy.WPF.RelayCommand(_ => { _ = AddRecordAsync(); });
            EditCommand = new BusBuddy.WPF.RelayCommand(_ => { _ = EditRecordAsync(); });
            DeleteCommand = new BusBuddy.WPF.RelayCommand(_ => { _ = DeleteRecordAsync(); });

            // Set as in-development
            IsInDevelopment = true;

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                MaintenanceRecords.Clear();
                var records = await _maintenanceService.GetAllMaintenanceRecordsAsync();
                foreach (var record in records)
                    MaintenanceRecords.Add(record);

                AvailableBuses.Clear();
                var buses = await _busService.GetAllBusEntitiesAsync();
                foreach (var bus in buses)
                    AvailableBuses.Add(bus);

                Logger?.LogInformation("Loaded {RecordCount} maintenance records and {BusCount} buses",
                    MaintenanceRecords.Count, AvailableBuses.Count);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error loading maintenance data");
            }
        }

        private async Task AddRecordAsync()
        {
            try
            {
                var firstBus = AvailableBuses.Count > 0 ? AvailableBuses[0] : null;
                if (firstBus == null) return;

                var newRecord = new Maintenance
                {
                    VehicleId = firstBus.VehicleId,
                    Date = DateTime.Now,
                    OdometerReading = 0,
                    MaintenanceCompleted = "New Maintenance",
                    Vendor = "",
                    RepairCost = 0,
                    Status = "Scheduled"
                };

                var created = await _maintenanceService.CreateMaintenanceRecordAsync(newRecord);
                MaintenanceRecords.Add(created);

                Logger?.LogInformation("Added new maintenance record with ID {MaintenanceId}", created.MaintenanceId);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error adding maintenance record");
            }
        }

        private async Task EditRecordAsync()
        {
            if (SelectedRecord == null) return;

            try
            {
                var updated = await _maintenanceService.UpdateMaintenanceRecordAsync(SelectedRecord);
                Logger?.LogInformation("Updated maintenance record with ID {MaintenanceId}", SelectedRecord.MaintenanceId);
                // Optionally refresh the list or update the item in-place
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error updating maintenance record {MaintenanceId}", SelectedRecord.MaintenanceId);
            }
        }

        private async Task DeleteRecordAsync()
        {
            if (SelectedRecord == null) return;

            try
            {
                var deleted = await _maintenanceService.DeleteMaintenanceRecordAsync(SelectedRecord.MaintenanceId);
                if (deleted)
                {
                    MaintenanceRecords.Remove(SelectedRecord);
                    Logger?.LogInformation("Deleted maintenance record with ID {MaintenanceId}", SelectedRecord.MaintenanceId);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error deleting maintenance record {MaintenanceId}", SelectedRecord.MaintenanceId);
            }
        }
    }
}
