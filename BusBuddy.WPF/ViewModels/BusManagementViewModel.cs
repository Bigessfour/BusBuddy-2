using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System;

namespace BusBuddy.WPF.ViewModels
{
    using BusBuddy.Core.Services;
    using BusBuddy.Core.Models;
    using System.Threading.Tasks;

    public class BusManagementViewModel : INotifyPropertyChanged
    {
        private readonly IBusService _service;
        public ObservableCollection<Bus> Buses { get; } = new();
        public ICommand AddBusCommand { get; }
        public ICommand EditBusCommand { get; }
        public ICommand DeleteBusCommand { get; }
        private Bus? _selectedBus;
        public Bus? SelectedBus
        {
            get => _selectedBus;
            set { _selectedBus = value; OnPropertyChanged(); }
        }
        public BusManagementViewModel(IBusService service)
        {
            _service = service;
            LoadBusesAsync();
            AddBusCommand = new BusBuddy.WPF.RelayCommand(_ => AddBusAsyncWrapper());
            EditBusCommand = new BusBuddy.WPF.RelayCommand(_ => EditBusAsyncWrapper());
            DeleteBusCommand = new BusBuddy.WPF.RelayCommand(_ => DeleteBusAsyncWrapper());
        }

        private async void LoadBusesAsync()
        {
            Buses.Clear();
            var buses = await _service.GetAllBusEntitiesAsync();
            foreach (var b in buses)
                Buses.Add(b);
        }

        private async void AddBusAsyncWrapper() => await AddBusAsync();
        private async void EditBusAsyncWrapper() => await EditBusAsync();
        private async void DeleteBusAsyncWrapper() => await DeleteBusAsync();

        private async Task AddBusAsync()
        {
            var newBus = new Bus
            {
                BusNumber = "New Bus",
                Model = "Model X",
                SeatingCapacity = 50,
                Status = "Active"
            };
            var created = await _service.AddBusEntityAsync(newBus);
            Buses.Add(created);
        }

        private async Task EditBusAsync()
        {
            if (SelectedBus != null)
            {
                await _service.UpdateBusEntityAsync(SelectedBus);
                // Optionally reload buses
            }
        }

        private async Task DeleteBusAsync()
        {
            if (SelectedBus != null)
            {
                await _service.DeleteBusEntityAsync(SelectedBus.VehicleId);
                Buses.Remove(SelectedBus);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // ...existing code...
}
