using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace BusBuddy.WPF.ViewModels
{
    public class MaintenanceRecord : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string? BusNumber { get; set; }
        public string? Description { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? Status { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public interface IMaintenanceService
    {
        ObservableCollection<MaintenanceRecord> GetMaintenanceRecords();
        ObservableCollection<string> GetAvailableBuses();
        void AddRecord(MaintenanceRecord record);
        void UpdateRecord(MaintenanceRecord record);
        void DeleteRecord(MaintenanceRecord record);
    }

    public class MaintenanceService : IMaintenanceService
    {
        private ObservableCollection<MaintenanceRecord> _records = new();
        private ObservableCollection<string> _buses = new() { "Bus 1", "Bus 2", "Bus 3" };
        public ObservableCollection<MaintenanceRecord> GetMaintenanceRecords() => _records;
        public ObservableCollection<string> GetAvailableBuses() => _buses;
        public void AddRecord(MaintenanceRecord record) => _records.Add(record);
        public void UpdateRecord(MaintenanceRecord record)
        {
            var existing = _records.FirstOrDefault(r => r.Id == record.Id);
            if (existing != null)
            {
                existing.BusNumber = record.BusNumber;
                existing.Description = record.Description;
                existing.ScheduledDate = record.ScheduledDate;
                existing.CompletedDate = record.CompletedDate;
                existing.Status = record.Status;
            }
        }
        public void DeleteRecord(MaintenanceRecord record) => _records.Remove(record);
    }

    public class MaintenanceTrackingViewModel : INotifyPropertyChanged
    {
        private readonly IMaintenanceService _service;
        public ObservableCollection<MaintenanceRecord> MaintenanceRecords { get; }
        public ObservableCollection<string> AvailableBuses { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        private MaintenanceRecord? _selectedRecord;
        public MaintenanceRecord? SelectedRecord
        {
            get => _selectedRecord;
            set { _selectedRecord = value; OnPropertyChanged(); }
        }
        public MaintenanceTrackingViewModel(IMaintenanceService service)
        {
            _service = service;
            MaintenanceRecords = _service.GetMaintenanceRecords();
            AvailableBuses = _service.GetAvailableBuses();
            AddCommand = new RelayCommand(AddRecord);
            EditCommand = new RelayCommand(EditRecord);
            DeleteCommand = new RelayCommand(DeleteRecord);
        }
        private void AddRecord()
        {
            var newRecord = new MaintenanceRecord
            {
                Id = MaintenanceRecords.Count + 1,
                BusNumber = AvailableBuses.FirstOrDefault() ?? string.Empty,
                Description = "New Maintenance",
                ScheduledDate = DateTime.Now,
                Status = "Scheduled"
            };
            _service.AddRecord(newRecord);
        }
        private void EditRecord()
        {
            if (SelectedRecord != null)
                _service.UpdateRecord(SelectedRecord);
        }
        private void DeleteRecord()
        {
            if (SelectedRecord != null)
                _service.DeleteRecord(SelectedRecord);
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
