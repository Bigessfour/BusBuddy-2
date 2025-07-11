using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using BusBuddy.Core.Services.Interfaces;
using BusBuddy.WPF.Views.Maintenance;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

// Disable async method without await operator warnings
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

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
        public BusBuddy.WPF.RelayCommand AlertsCommand { get; }
        public BusBuddy.WPF.RelayCommand ReportCommand { get; }

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
            EditCommand = new BusBuddy.WPF.RelayCommand(_ => { _ = EditRecordAsync(); }, _ => SelectedRecord != null);
            DeleteCommand = new BusBuddy.WPF.RelayCommand(_ => { _ = DeleteRecordAsync(); }, _ => SelectedRecord != null);
            AlertsCommand = new BusBuddy.WPF.RelayCommand(_ => { _ = ShowMaintenanceAlertsAsync(); });
            ReportCommand = new BusBuddy.WPF.RelayCommand(_ => MessageBox.Show("Maintenance reports will be implemented in the next sprint.",
                "Coming Soon", MessageBoxButton.OK, MessageBoxImage.Information));

            // Set as ready for development
            IsInDevelopment = false;

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
                var buses = await _busService.GetAllBusesAsync();
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
                    Status = "Scheduled",
                    Priority = "Normal"
                };

                // Show dialog to edit the new record
                var dialog = new MaintenanceDialog(newRecord, _busService, Logger as ILogger<MaintenanceDialog>);

                var result = dialog.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    var created = await _maintenanceService.CreateMaintenanceRecordAsync(newRecord);
                    MaintenanceRecords.Add(created);

                    Logger?.LogInformation("Added new maintenance record with ID {MaintenanceId}", created.MaintenanceId);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error adding maintenance record");
                MessageBox.Show($"Error adding maintenance record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task EditRecordAsync()
        {
            if (SelectedRecord == null) return;

            try
            {
                // Create a copy to edit
                var recordToEdit = new Maintenance
                {
                    MaintenanceId = SelectedRecord.MaintenanceId,
                    VehicleId = SelectedRecord.VehicleId,
                    Date = SelectedRecord.Date,
                    OdometerReading = SelectedRecord.OdometerReading,
                    MaintenanceCompleted = SelectedRecord.MaintenanceCompleted,
                    Vendor = SelectedRecord.Vendor,
                    RepairCost = SelectedRecord.RepairCost,
                    Status = SelectedRecord.Status,
                    Priority = SelectedRecord.Priority,
                    Description = SelectedRecord.Description,
                    PerformedBy = SelectedRecord.PerformedBy,
                    NextServiceDue = SelectedRecord.NextServiceDue,
                    NextServiceOdometer = SelectedRecord.NextServiceOdometer,
                    Notes = SelectedRecord.Notes,
                    WorkOrderNumber = SelectedRecord.WorkOrderNumber,
                    Warranty = SelectedRecord.Warranty,
                    WarrantyExpiry = SelectedRecord.WarrantyExpiry,
                    PartsUsed = SelectedRecord.PartsUsed,
                    LaborHours = SelectedRecord.LaborHours,
                    LaborCost = SelectedRecord.LaborCost,
                    PartsCost = SelectedRecord.PartsCost,
                    CreatedDate = SelectedRecord.CreatedDate,
                    CreatedBy = SelectedRecord.CreatedBy
                };

                // Show dialog to edit the record
                var dialog = new MaintenanceDialog(recordToEdit, _busService, Logger as ILogger<MaintenanceDialog>);

                var result = dialog.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    var updated = await _maintenanceService.UpdateMaintenanceRecordAsync(recordToEdit);

                    // Update the selected record with new values
                    var index = MaintenanceRecords.IndexOf(SelectedRecord);
                    if (index >= 0)
                    {
                        MaintenanceRecords[index] = updated;
                        SelectedRecord = updated;
                    }

                    Logger?.LogInformation("Updated maintenance record with ID {MaintenanceId}", updated.MaintenanceId);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error updating maintenance record {MaintenanceId}", SelectedRecord.MaintenanceId);
                MessageBox.Show($"Error updating maintenance record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task DeleteRecordAsync()
        {
            if (SelectedRecord == null) return;

            try
            {
                // Store the ID for logging in case SelectedRecord gets set to null
                var maintenanceId = SelectedRecord.MaintenanceId;

                // Confirm deletion
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the maintenance record from {SelectedRecord.Date:d} for {SelectedRecord.MaintenanceCompleted}?",
                    "Confirm Deletion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var deleted = await _maintenanceService.DeleteMaintenanceRecordAsync(maintenanceId);
                    if (deleted)
                    {
                        MaintenanceRecords.Remove(SelectedRecord);
                        SelectedRecord = null;
                        Logger?.LogInformation("Deleted maintenance record with ID {MaintenanceId}", maintenanceId);
                    }
                }
            }
            catch (Exception ex)
            {
                var id = SelectedRecord?.MaintenanceId ?? 0;
                Logger?.LogError(ex, "Error deleting maintenance record {MaintenanceId}", id);
                MessageBox.Show($"Error deleting maintenance record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ShowMaintenanceAlertsAsync()
        {
            try
            {
                var alertsDialog = new Views.Maintenance.MaintenanceAlertsDialog(_maintenanceService, _busService, Logger as ILogger<Views.Maintenance.MaintenanceAlertsDialog>);
                var result = alertsDialog.ShowDialog();

                if (result.HasValue && result.Value)
                {
                    // User clicked "Create New Maintenance" in the alerts dialog
                    _ = AddRecordAsync();
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error showing maintenance alerts");
                MessageBox.Show($"Error showing maintenance alerts: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
