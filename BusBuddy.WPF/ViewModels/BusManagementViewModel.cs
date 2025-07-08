using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System;

namespace BusBuddy.WPF.ViewModels
{
    public class Bus : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string? Number { get; set; }
        public string? Model { get; set; }
        public int Capacity { get; set; }
        public string? Status { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public interface IBusService
    {
        ObservableCollection<Bus> GetBuses();
        void AddBus(Bus bus);
        void UpdateBus(Bus bus);
        void DeleteBus(Bus bus);
    }

    public class BusService : IBusService
    {
        private ObservableCollection<Bus> _buses = new();
        public ObservableCollection<Bus> GetBuses() => _buses;
        public void AddBus(Bus bus) => _buses.Add(bus);
        public void UpdateBus(Bus bus)
        {
            var existing = _buses.FirstOrDefault(b => b.Id == bus.Id);
            if (existing != null)
            {
                existing.Number = bus.Number;
                existing.Model = bus.Model;
                existing.Capacity = bus.Capacity;
                existing.Status = bus.Status;
            }
        }
        public void DeleteBus(Bus bus) => _buses.Remove(bus);
    }

    public class BusManagementViewModel : INotifyPropertyChanged
    {
        private readonly IBusService _service;
        public ObservableCollection<Bus> Buses { get; }
        public ICommand AddBusCommand { get; }
        public ICommand EditBusCommand { get; }
        public ICommand DeleteBusCommand { get; }
        private Bus? _selectedBus;
        public Bus? SelectedBus
        {
            get => _selectedBus;
            set { _selectedBus = value; OnPropertyChanged(); }
        }

        // Multi-selection support
        private ObservableCollection<Bus> _selectedBuses = new();
        public ObservableCollection<Bus> SelectedBuses
        {
            get => _selectedBuses;
            set { _selectedBuses = value; OnPropertyChanged(); }
        }

        public ICommand BulkUpdateStatusCommand { get; }
        public BusManagementViewModel(IBusService service)
        {
            _service = service;
            Buses = _service.GetBuses();
            AddBusCommand = new RelayCommand(AddBus);
            EditBusCommand = new RelayCommand(EditBus);
            DeleteBusCommand = new RelayCommand(DeleteBus);
            BulkUpdateStatusCommand = new RelayCommand(BulkUpdateStatus);
        }

        private void BulkUpdateStatus()
        {
            if (SelectedBuses == null || SelectedBuses.Count == 0)
                return;

            // Example: Mark all selected as Inactive
            foreach (var bus in SelectedBuses)
            {
                bus.Status = "Inactive";
                _service.UpdateBus(bus);
            }
            // Notify UI
            OnPropertyChanged(nameof(Buses));
        }
        private void AddBus()
        {
            var newBus = new Bus
            {
                Id = Buses.Count + 1,
                Number = "New Bus",
                Model = "Model X",
                Capacity = 50,
                Status = "Active"
            };
            _service.AddBus(newBus);
        }
        private void EditBus()
        {
            if (SelectedBus != null)
                _service.UpdateBus(SelectedBus);
        }
        private void DeleteBus()
        {
            if (SelectedBus != null)
                _service.DeleteBus(SelectedBus);
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
