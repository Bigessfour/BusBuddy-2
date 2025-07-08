using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services;

namespace BusBuddy.WPF.ViewModels
{
    public class MaintenanceTrackingViewModel : INotifyPropertyChanged
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

        public MaintenanceTrackingViewModel(IMaintenanceService maintenanceService, IBusService busService)
        {
            _maintenanceService = maintenanceService;
            _busService = busService;
            AddCommand = new BusBuddy.WPF.RelayCommand(_ => { _ = AddRecordAsync(); });
            EditCommand = new BusBuddy.WPF.RelayCommand(_ => { _ = EditRecordAsync(); });
            DeleteCommand = new BusBuddy.WPF.RelayCommand(_ => { _ = DeleteRecordAsync(); });
            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            MaintenanceRecords.Clear();
            var records = await _maintenanceService.GetAllMaintenanceRecordsAsync();
            foreach (var record in records)
                MaintenanceRecords.Add(record);

            AvailableBuses.Clear();
            var buses = await _busService.GetAllBusEntitiesAsync();
            foreach (var bus in buses)
                AvailableBuses.Add(bus);
        }

        private async Task AddRecordAsync()
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
        }

        private async Task EditRecordAsync()
        {
            if (SelectedRecord == null) return;
            var updated = await _maintenanceService.UpdateMaintenanceRecordAsync(SelectedRecord);
            // Optionally refresh the list or update the item in-place
        }

        private async Task DeleteRecordAsync()
        {
            if (SelectedRecord == null) return;
            var deleted = await _maintenanceService.DeleteMaintenanceRecordAsync(SelectedRecord.MaintenanceId);
            if (deleted)
                MaintenanceRecords.Remove(SelectedRecord);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
