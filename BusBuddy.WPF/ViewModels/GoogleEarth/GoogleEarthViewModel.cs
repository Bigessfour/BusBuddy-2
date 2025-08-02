using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using BusBuddy.Core.Services;
using BusBuddy.WPF.ViewModels.Base;
using BusBuddy.Core.Models;
using RouteModel = BusBuddy.Core.Models.Route;

namespace BusBuddy.WPF.ViewModels.GoogleEarth
{
    /// <summary>
    /// ViewModel for Google Earth integration view
    /// Manages map layers, route visualization, and geographic data
    /// </summary>
    public class GoogleEarthViewModel : BaseViewModel
    {
        private readonly IGeoDataService _geoDataService;
        private readonly ILogger<GoogleEarthViewModel> _logger;

        private ObservableCollection<RouteModel> _routes = new();
        private RouteModel? _selectedRoute;
        private string _selectedMapLayer = "Satellite";
        private bool _isMapLoading;
        private string _statusMessage = "Ready";

        public GoogleEarthViewModel(IGeoDataService geoDataService, ILogger<GoogleEarthViewModel> logger)
        {
            _geoDataService = geoDataService ?? throw new ArgumentNullException(nameof(geoDataService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            LoadRoutesCommand = new RelayCommand(async () => await LoadRoutesAsync());
            RefreshMapCommand = new RelayCommand(async () => await RefreshMapAsync());
            ExportRouteDataCommand = new RelayCommand(async () => await ExportRouteDataAsync(), () => SelectedRoute != null);
        }

        #region Properties

        /// <summary>
        /// Indicates if the map is currently loading
        /// </summary>
        public bool IsMapLoading
        {
            get => _isMapLoading;
            set => SetProperty(ref _isMapLoading, value);
        }

        /// <summary>
        /// Currently selected map layer (Satellite, Terrain, etc.)
        /// </summary>
        public string SelectedMapLayer
        {
            get => _selectedMapLayer;
            set
            {
                if (SetProperty(ref _selectedMapLayer, value))
                {
                    OnMapLayerChanged();
                }
            }
        }

        /// <summary>
        /// Current status of the map system
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// Collection of routes to display on the map
        /// </summary>
        public ObservableCollection<RouteModel> Routes
        {
            get => _routes;
            set => SetProperty(ref _routes, value);
        }

        /// <summary>
        /// Currently selected route for detailed view
        /// </summary>
        public RouteModel? SelectedRoute
        {
            get => _selectedRoute;
            set
            {
                if (SetProperty(ref _selectedRoute, value))
                {
                    ExportRouteDataCommand.NotifyCanExecuteChanged();
                    OnSelectedRouteChanged();
                }
            }
        }

        /// <summary>
        /// Available map layer options
        /// </summary>
        public ObservableCollection<string> MapLayers { get; } = new()
        {
            "Satellite",
            "Terrain",
            "Roadmap",
            "Hybrid"
        };

        #endregion

        #region Commands

        public ICommand LoadRoutesCommand { get; private set; } = null!;
        public ICommand RefreshMapCommand { get; private set; } = null!;
        public ICommand ExportRouteDataCommand { get; private set; } = null!;

        #endregion

        #region Private Methods

        private async Task LoadRoutesAsync()
        {
            try
            {
                IsMapLoading = true;
                StatusMessage = "Loading routes...";

                _logger.LogInformation("Loading routes for Google Earth integration");

                var routes = await _geoDataService.GetRoutesWithGeoDataAsync();

                Routes.Clear();
                foreach (var route in routes)
                {
                    Routes.Add(route);
                }

                StatusMessage = $"Loaded {routes.Count} routes";
                _logger.LogInformation("Successfully loaded {Count} routes", routes.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading routes for Google Earth");
                StatusMessage = "Error loading routes";
                ShowError("Failed to load routes for Google Earth integration");
            }
            finally
            {
                IsMapLoading = false;
            }
        }

        private void OnMapLayerChanged()
        {
            _logger.LogInformation("Map layer changed to: {Layer}", SelectedMapLayer);
            StatusMessage = $"Switched to {SelectedMapLayer} view";
        }

        private void OnSelectedRouteChanged()
        {
            if (SelectedRoute is null)
            {
                return;
            }

            try
            {
                _logger.LogInformation("Selected route changed to: {RouteName}", SelectedRoute.RouteName ?? "Unknown");
                StatusMessage = $"Selected: {SelectedRoute.RouteName ?? "Unknown Route"}";

                // Trigger map update for selected route
                _ = Task.Run(async () => await UpdateMapForRouteAsync(SelectedRoute.RouteName ?? "Unknown"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling route selection change");
            }
        }

        private async Task RefreshMapAsync()
        {
            try
            {
                IsMapLoading = true;
                StatusMessage = "Refreshing map...";

                if (SelectedRoute is not null)
                {
                    _logger.LogInformation("Refreshing map for route: {RouteName}", SelectedRoute.RouteName ?? "Unknown");
                    await UpdateMapForRouteAsync(SelectedRoute.RouteName ?? "Unknown");
                }
                else
                {
                    await LoadAllRoutesOnMapAsync();
                }

                StatusMessage = "Map refreshed";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing map");
                StatusMessage = "Error refreshing map";
                ShowError("Failed to refresh map display");
            }
            finally
            {
                IsMapLoading = false;
            }
        }

        private async Task LoadAllRoutesOnMapAsync()
        {
            // Create sample route data for demonstration
            var sampleRoute = new RouteModel
            {
                Id = 1,
                RouteName = "Sample Route 1",
                RouteDate = DateTime.Today,
                Status = "Active"
            };

            await UpdateMapForRouteAsync(sampleRoute.RouteName);
        }

        private async Task UpdateMapForRouteAsync(string routeName)
        {
            // Create sample route data for demonstration
            var sampleRoute = new RouteModel
            {
                Id = 1,
                RouteName = routeName,
                RouteDate = DateTime.Today,
                Status = "Active"
            };

            await Task.Delay(500); // Simulate map update
            _logger.LogInformation("Map updated for route: {RouteName}", routeName);
        }

        private async Task ExportRouteDataAsync()
        {
            // Create sample route data for demonstration
            var sampleRoute = new RouteModel
            {
                Id = 1,
                RouteName = "Sample Export Route",
                RouteDate = DateTime.Today,
                Status = "Active"
            };

            await Task.Delay(200); // Simulate export
            _logger.LogInformation("Route data exported for: {RouteName}", sampleRoute.RouteName);
        }

        private void ShowError(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            // Simple error display for now
            StatusMessage = $"Error: {message}";
        }
    }
}
                    RouteId = 3,
                    RouteName = "Route 3 - High School",
                    Description = "High school morning route",
                    StartTime = new TimeSpan(7, 15, 0),
                    EndTime = new TimeSpan(8, 30, 0),
                    Status = RouteStatus.Active,
                    School = "Central High School"
                }
            };
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }

    /// <summary>
    /// Simple relay command implementation for MVVM
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _execute();
    }
}
