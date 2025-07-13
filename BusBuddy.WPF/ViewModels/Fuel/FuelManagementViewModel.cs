using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using BusBuddy.Core.Services.Interfaces;
using BusBuddy.WPF.Views.Fuel;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Windows;

// Disable async method without await operator warnings
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace BusBuddy.WPF.ViewModels
{
    public class FuelManagementViewModel : BaseInDevelopmentViewModel
    {
        private readonly IFuelService _fuelService;
        private readonly IBusService _busService;

        private ObservableCollection<Fuel> _fuelRecords = new();
        public ObservableCollection<Fuel> FuelRecords
        {
            get => _fuelRecords;
            set => SetProperty(ref _fuelRecords, value);
        }

        private ObservableCollection<FuelTrendPoint> _fuelTrends = new();
        public ObservableCollection<FuelTrendPoint> FuelTrends
        {
            get => _fuelTrends;
            set => SetProperty(ref _fuelTrends, value);
        }

        private Fuel? _selectedFuelRecord;
        public Fuel? SelectedFuelRecord
        {
            get => _selectedFuelRecord;
            set
            {
                SetProperty(ref _selectedFuelRecord, value);
                OnPropertyChanged(nameof(CanEdit));
                OnPropertyChanged(nameof(CanDelete));
            }
        }

        public bool CanEdit => SelectedFuelRecord != null;
        public bool CanDelete => SelectedFuelRecord != null;

        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand ExportCommand { get; }
        public RelayCommand ReportCommand { get; }
        public RelayCommand ReconciliationCommand { get; }

        public FuelManagementViewModel(IFuelService fuelService, IBusService busService, ILogger<FuelManagementViewModel>? logger = null)
            : base(logger)
        {
            _fuelService = fuelService;
            _busService = busService;

            AddCommand = new RelayCommand(_ => { _ = AddFuelRecordAsync(); });
            EditCommand = new RelayCommand(_ => { _ = EditFuelRecordAsync(); }, _ => CanEdit);
            DeleteCommand = new RelayCommand(_ => { _ = DeleteFuelRecordAsync(); }, _ => CanDelete);
            ExportCommand = new RelayCommand(_ => { _ = ExportFuelDataAsync(); });
            ReportCommand = new RelayCommand(_ => { _ = ShowFuelReportAsync(); });
            ReconciliationCommand = new RelayCommand(_ => { _ = ShowFuelReconciliationAsync(); });

            // Set as ready for development
            IsInDevelopment = false;

            _ = LoadFuelRecordsAsync();
        }

        private async Task LoadFuelRecordsAsync()
        {
            try
            {
                FuelRecords.Clear();
                var records = await _fuelService.GetAllFuelRecordsAsync();
                foreach (var record in records)
                    FuelRecords.Add(record);
                CalculateTrends();

                Logger?.LogInformation("Loaded {RecordCount} fuel records", FuelRecords.Count);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error loading fuel records");
                MessageBox.Show($"Error loading fuel records: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddFuelRecordAsync()
        {
            try
            {
                // Get first bus for default
                var buses = await _busService.GetAllBusesAsync();
                var firstBus = buses.FirstOrDefault();
                if (firstBus == null)
                {
                    MessageBox.Show("No buses available. Please add buses first.", "No Buses", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newFuel = new Fuel
                {
                    FuelDate = DateTime.Now,
                    FuelLocation = "Key Pumps",
                    VehicleFueledId = firstBus.VehicleId,
                    VehicleOdometerReading = 0,
                    FuelType = "Diesel",
                    Gallons = 0,
                    PricePerGallon = 0,
                    TotalCost = 0
                };

                // Show dialog to edit the new record
                var dialog = new FuelDialog(newFuel, _busService, Logger as ILogger<FuelDialog>);

                var result = dialog.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    var created = await _fuelService.CreateFuelRecordAsync(newFuel);
                    FuelRecords.Add(created);
                    CalculateTrends();

                    Logger?.LogInformation("Added new fuel record with ID {FuelId}", created.FuelId);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error adding fuel record");
                MessageBox.Show($"Error adding fuel record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task EditFuelRecordAsync()
        {
            if (SelectedFuelRecord == null) return;

            try
            {
                // Create a copy to edit
                var recordToEdit = new Fuel
                {
                    FuelId = SelectedFuelRecord.FuelId,
                    FuelDate = SelectedFuelRecord.FuelDate,
                    FuelLocation = SelectedFuelRecord.FuelLocation,
                    VehicleFueledId = SelectedFuelRecord.VehicleFueledId,
                    VehicleOdometerReading = SelectedFuelRecord.VehicleOdometerReading,
                    FuelType = SelectedFuelRecord.FuelType,
                    Gallons = SelectedFuelRecord.Gallons,
                    PricePerGallon = SelectedFuelRecord.PricePerGallon,
                    TotalCost = SelectedFuelRecord.TotalCost,
                    Notes = SelectedFuelRecord.Notes
                };

                // Show dialog to edit the record
                var dialog = new FuelDialog(recordToEdit, _busService, Logger as ILogger<FuelDialog>);

                var result = dialog.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    var updated = await _fuelService.UpdateFuelRecordAsync(recordToEdit);

                    // Update the selected record with new values
                    var index = FuelRecords.IndexOf(SelectedFuelRecord);
                    if (index >= 0)
                    {
                        FuelRecords[index] = updated;
                        SelectedFuelRecord = updated;
                    }

                    CalculateTrends();
                    Logger?.LogInformation("Updated fuel record with ID {FuelId}", updated.FuelId);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error updating fuel record {FuelId}", SelectedFuelRecord.FuelId);
                MessageBox.Show($"Error updating fuel record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteFuelRecordAsync()
        {
            if (SelectedFuelRecord == null) return;

            try
            {
                // Confirm deletion
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the fuel record from {SelectedFuelRecord.FuelDate:d} for {SelectedFuelRecord.Gallons} gallons?",
                    "Confirm Deletion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var deleted = await _fuelService.DeleteFuelRecordAsync(SelectedFuelRecord.FuelId);
                    if (deleted)
                    {
                        var fuelId = SelectedFuelRecord.FuelId;
                        FuelRecords.Remove(SelectedFuelRecord);
                        SelectedFuelRecord = null;
                        CalculateTrends();

                        Logger?.LogInformation("Deleted fuel record with ID {FuelId}", fuelId);
                    }
                }
            }
            catch (Exception ex)
            {
                if (SelectedFuelRecord != null)
                {
                    Logger?.LogError(ex, "Error deleting fuel record {FuelId}", SelectedFuelRecord.FuelId);
                    MessageBox.Show($"Error deleting fuel record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task ExportFuelDataAsync()
        {
            try
            {
                // Create a Save File Dialog
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Fuel_Records_{DateTime.Now:yyyyMMdd}",
                    DefaultExt = ".csv",
                    Filter = "CSV Files (*.csv)|*.csv"
                };

                var result = dialog.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    // Export data to CSV
                    using var writer = new System.IO.StreamWriter(dialog.FileName);

                    // Write header
                    writer.WriteLine("Fuel ID,Date,Location,Bus,Odometer,Fuel Type,Gallons,Price/Gallon,Total Cost,Notes");

                    // Write data
                    foreach (var record in FuelRecords)
                    {
                        var busNumber = "Unknown";
                        try
                        {
                            var bus = await _busService.GetBusByIdAsync(record.VehicleFueledId);
                            if (bus != null)
                                busNumber = bus.BusNumber;
                        }
                        catch { /* Ignore errors and use default */ }

                        writer.WriteLine(
                            $"{record.FuelId}," +
                            $"{record.FuelDate:yyyy-MM-dd}," +
                            $"\"{record.FuelLocation}\"," +
                            $"\"{busNumber}\"," +
                            $"{record.VehicleOdometerReading}," +
                            $"\"{record.FuelType}\"," +
                            $"{record.Gallons}," +
                            $"{record.PricePerGallon}," +
                            $"{record.TotalCost}," +
                            $"\"{record.Notes?.Replace("\"", "\"\"")}\"");
                    }

                    MessageBox.Show($"Successfully exported {FuelRecords.Count} fuel records to {dialog.FileName}",
                        "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                    Logger?.LogInformation("Exported {RecordCount} fuel records to CSV", FuelRecords.Count);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error exporting fuel records");
                MessageBox.Show($"Error exporting fuel records: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Task ShowFuelReportAsync()
        {
            MessageBox.Show("Fuel reports will be implemented in the next sprint.",
                "Coming Soon", MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        }

        private async Task ShowFuelReconciliationAsync()
        {
            try
            {
                var dialog = new FuelReconciliationDialog(_fuelService, _busService, Logger as ILogger<FuelReconciliationDialog>);
                _ = dialog.ShowDialog();

                // No need to refresh data as reconciliation doesn't modify records
                Logger?.LogInformation("Opened fuel reconciliation dialog");
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error opening fuel reconciliation dialog");
                MessageBox.Show($"Error opening fuel reconciliation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalculateTrends()
        {
            try
            {
                FuelTrends.Clear();
                // Simple trend: average MPG per month
                var grouped = new System.Linq.EnumerableQuery<Fuel>(FuelRecords)
                    .GroupBy(f => new { f.FuelDate.Year, f.FuelDate.Month })
                    .Select(g => new FuelTrendPoint
                    {
                        Period = new DateTime(g.Key.Year, g.Key.Month, 1),
                        AvgMPG = g.Average(f => f.Gallons.HasValue && f.Gallons > 0 ? (f.VehicleOdometerReading / (double)f.Gallons.Value) : 0)
                    });
                foreach (var pt in grouped)
                    FuelTrends.Add(pt);

                Logger?.LogInformation("Calculated {TrendPointCount} fuel trend points", FuelTrends.Count);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error calculating fuel trends");
            }
        }

        // BaseViewModel already provides PropertyChanged functionality
    }

    public class FuelTrendPoint
    {
        public DateTime Period { get; set; }
        public double AvgMPG { get; set; }
    }
}
