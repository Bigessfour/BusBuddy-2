using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BusBuddy.WPF.ViewModels
{
    public partial class RouteManagementViewModel : ObservableObject
    {
        private readonly IRouteService _routeService;
        private readonly IBusService _busService;
        private readonly IDriverService _driverService;

        [ObservableProperty]
        private ObservableCollection<Route> _routes;

        [ObservableProperty]
        private ObservableCollection<Bus> _buses;

        [ObservableProperty]
        private ObservableCollection<Driver> _drivers;

        [ObservableProperty]
        private Route _selectedRoute;

        public ICommand LoadRoutesCommand { get; }
        public ICommand AddRouteCommand { get; }
        public ICommand UpdateRouteCommand { get; }
        public ICommand DeleteRouteCommand { get; }

        public RouteManagementViewModel(IRouteService routeService, IBusService busService, IDriverService driverService)
        {
            _routeService = routeService;
            _busService = busService;
            _driverService = driverService;

            Routes = new ObservableCollection<Route>();
            Buses = new ObservableCollection<Bus>();
            Drivers = new ObservableCollection<Driver>();
            SelectedRoute = new Route();

            LoadRoutesCommand = new AsyncRelayCommand(LoadDataAsync);
            AddRouteCommand = new AsyncRelayCommand(AddRouteAsync);
            UpdateRouteCommand = new AsyncRelayCommand(UpdateRouteAsync, CanUpdateOrDelete);
            DeleteRouteCommand = new AsyncRelayCommand(DeleteRouteAsync, CanUpdateOrDelete);
        }

        private async Task LoadDataAsync()
        {
            await LoadRoutesAsync();
            await LoadBusesAsync();
            await LoadDriversAsync();
        }

        private async Task LoadRoutesAsync()
        {
            var routes = await _routeService.GetAllActiveRoutesAsync();
            Routes.Clear();
            foreach (var route in routes)
            {
                Routes.Add(route);
            }
        }

        private async Task LoadBusesAsync()
        {
            var buses = await _busService.GetAllBusEntitiesAsync();
            Buses.Clear();
            foreach (var bus in buses)
            {
                Buses.Add(bus);
            }
        }

        private async Task LoadDriversAsync()
        {
            var drivers = await _driverService.GetAllDriversAsync();
            Drivers.Clear();
            foreach (var driver in drivers)
            {
                Drivers.Add(driver);
            }
        }

        private async Task AddRouteAsync()
        {
            if (SelectedRoute != null)
            {
                await _routeService.CreateRouteAsync(SelectedRoute);
                await LoadRoutesAsync();
            }
        }

        private async Task UpdateRouteAsync()
        {
            if (SelectedRoute != null)
            {
                await _routeService.UpdateRouteAsync(SelectedRoute);
                await LoadRoutesAsync();
            }
        }

        private async Task DeleteRouteAsync()
        {
            if (SelectedRoute != null)
            {
                await _routeService.DeleteRouteAsync(SelectedRoute.RouteId);
                await LoadRoutesAsync();
            }
        }

        private bool CanUpdateOrDelete()
        {
            return SelectedRoute != null && SelectedRoute.RouteId != 0;
        }

        partial void OnSelectedRouteChanged(Route value)
        {
            (UpdateRouteCommand as IRelayCommand)?.NotifyCanExecuteChanged();
            (DeleteRouteCommand as IRelayCommand)?.NotifyCanExecuteChanged();
        }
    }
}
