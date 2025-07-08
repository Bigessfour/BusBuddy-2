using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BusBuddy.WPF.ViewModels
{
    public partial class BusManagementViewModel : ObservableObject
    {
        private readonly IBusService _busService;

        [ObservableProperty]
        private ObservableCollection<Bus> _buses;

        [ObservableProperty]
        private Bus _selectedBus;

        public IAsyncRelayCommand LoadBusesCommand { get; }
        public IAsyncRelayCommand AddBusCommand { get; }
        public IAsyncRelayCommand UpdateBusCommand { get; }
        public IAsyncRelayCommand DeleteBusCommand { get; }

        public BusManagementViewModel(IBusService busService)
        {
            _busService = busService;
            _buses = new ObservableCollection<Bus>();
            _selectedBus = new Bus();

            LoadBusesCommand = new AsyncRelayCommand(LoadBusesAsync);
            AddBusCommand = new AsyncRelayCommand(AddBusAsync);
            UpdateBusCommand = new AsyncRelayCommand(UpdateBusAsync, CanUpdateOrDelete);
            DeleteBusCommand = new AsyncRelayCommand(DeleteBusAsync, CanUpdateOrDelete);

            _ = LoadBusesAsync();
        }

        private async Task LoadBusesAsync()
        {
            var buses = await _busService.GetAllBusEntitiesAsync();
            Buses.Clear();
            foreach (var b in buses)
                Buses.Add(b);
        }

        private async Task AddBusAsync()
        {
            if (SelectedBus != null)
            {
                await _busService.AddBusEntityAsync(SelectedBus);
                await LoadBusesAsync();
            }
        }

        private async Task UpdateBusAsync()
        {
            if (SelectedBus != null)
            {
                await _busService.UpdateBusEntityAsync(SelectedBus);
                await LoadBusesAsync();
            }
        }

        private async Task DeleteBusAsync()
        {
            if (SelectedBus != null)
            {
                await _busService.DeleteBusEntityAsync(SelectedBus.VehicleId);
                await LoadBusesAsync();
            }
        }

        private bool CanUpdateOrDelete()
        {
            return SelectedBus != null && SelectedBus.VehicleId != 0;
        }

        partial void OnSelectedBusChanged(Bus value)
        {
            (UpdateBusCommand as IRelayCommand)?.NotifyCanExecuteChanged();
            (DeleteBusCommand as IRelayCommand)?.NotifyCanExecuteChanged();
        }
    }
}
