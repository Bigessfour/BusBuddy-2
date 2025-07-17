using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Serilog;
using BusBuddy.WPF.Services;
using BusBuddy.WPF.Models;

namespace BusBuddy.WPF.ViewModels
{
    /// <summary>
    /// ViewModel for Google Earth integration
    /// Manages geospatial mapping and route visualization for transportation management
    /// </summary>
    public class GoogleEarthViewModel : INotifyPropertyChanged
    {
        private static readonly ILogger Logger = Log.ForContext<GoogleEarthViewModel>();

        private readonly IGoogleEarthService _googleEarthService;
        private bool _isMapLoading = false;
        private bool _isLiveTrackingEnabled = true;
        private object? _selectedBus;
        private string _currentMapLayer = "Satellite";

        public GoogleEarthViewModel(IGoogleEarthService googleEarthService)
        {
            _googleEarthService = googleEarthService ?? throw new ArgumentNullException(nameof(googleEarthService));

            ActiveBuses = new ObservableCollection<BusLocation>();

            // Initialize commands
            InitializeMapCommand = new RelayCommand(async _ => await InitializeMapAsync());
            CenterOnFleetCommand = new RelayCommand(async _ => await CenterOnFleetAsync());
            ShowAllBusesCommand = new RelayCommand(async _ => await ShowAllBusesAsync());
            ShowRoutesCommand = new RelayCommand(async _ => await ShowRoutesAsync());
            ShowSchoolsCommand = new RelayCommand(async _ => await ShowSchoolsAsync());
            TrackSelectedBusCommand = new RelayCommand(async _ => await TrackSelectedBusAsync(), _ => SelectedBus != null);
            ZoomInCommand = new RelayCommand(_ => ZoomIn());
            ZoomOutCommand = new RelayCommand(_ => ZoomOut());
            ResetViewCommand = new RelayCommand(async _ => await ResetViewAsync());

            // Initialize data
            LoadSampleData();

            Logger.Information("Google Earth ViewModel initialized");
        }

        #region Properties

        /// <summary>
        /// Collection of active bus locations
        /// </summary>
        public ObservableCollection<BusLocation> ActiveBuses { get; }

        /// <summary>
        /// Whether the map is currently loading
        /// </summary>
        public bool IsMapLoading
        {
            get => _isMapLoading;
            set
            {
                _isMapLoading = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether live tracking is enabled
        /// </summary>
        public bool IsLiveTrackingEnabled
        {
            get => _isLiveTrackingEnabled;
            set
            {
                _isLiveTrackingEnabled = value;
                OnPropertyChanged();
                if (value)
                {
                    StartLiveTracking();
                }
                else
                {
                    StopLiveTracking();
                }
            }
        }

        /// <summary>
        /// Currently selected bus
        /// </summary>
        public object? SelectedBus
        {
            get => _selectedBus;
            set
            {
                _selectedBus = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Current map layer type
        /// </summary>
        public string CurrentMapLayer
        {
            get => _currentMapLayer;
            set
            {
                _currentMapLayer = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand InitializeMapCommand { get; }
        public ICommand CenterOnFleetCommand { get; }
        public ICommand ShowAllBusesCommand { get; }
        public ICommand ShowRoutesCommand { get; }
        public ICommand ShowSchoolsCommand { get; }
        public ICommand TrackSelectedBusCommand { get; }
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        public ICommand ResetViewCommand { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize the Google Earth map
        /// </summary>
        private async Task InitializeMapAsync()
        {
            try
            {
                IsMapLoading = true;

                await _googleEarthService.InitializeAsync();
                await LoadBusLocationsAsync();

                Logger.Information("Google Earth map initialized successfully");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to initialize Google Earth map");
            }
            finally
            {
                IsMapLoading = false;
            }
        }

        /// <summary>
        /// Center the map on the fleet
        /// </summary>
        private async Task CenterOnFleetAsync()
        {
            try
            {
                await _googleEarthService.CenterOnFleetAsync();
                Logger.Information("Map centered on fleet");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to center map on fleet");
            }
        }

        /// <summary>
        /// Show all buses on the map
        /// </summary>
        private async Task ShowAllBusesAsync()
        {
            try
            {
                await _googleEarthService.ShowAllBusesAsync();
                Logger.Information("All buses displayed on map");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to show all buses on map");
            }
        }

        /// <summary>
        /// Show routes on the map
        /// </summary>
        private async Task ShowRoutesAsync()
        {
            try
            {
                await _googleEarthService.ShowRoutesAsync();
                Logger.Information("Routes displayed on map");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to show routes on map");
            }
        }

        /// <summary>
        /// Show schools on the map
        /// </summary>
        private async Task ShowSchoolsAsync()
        {
            try
            {
                await _googleEarthService.ShowSchoolsAsync();
                Logger.Information("Schools displayed on map");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to show schools on map");
            }
        }

        /// <summary>
        /// Track the selected bus
        /// </summary>
        private async Task TrackSelectedBusAsync()
        {
            if (SelectedBus is BusLocation bus)
            {
                try
                {
                    await _googleEarthService.TrackBusAsync(bus.BusNumber);
                    Logger.Information("Tracking bus: {BusNumber}", bus.BusNumber);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to track bus: {BusNumber}", bus.BusNumber);
                }
            }
        }

        /// <summary>
        /// Zoom in on the map
        /// </summary>
        private void ZoomIn()
        {
            try
            {
                _googleEarthService.ZoomIn();
                Logger.Debug("Map zoomed in");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to zoom in");
            }
        }

        /// <summary>
        /// Zoom out on the map
        /// </summary>
        private void ZoomOut()
        {
            try
            {
                _googleEarthService.ZoomOut();
                Logger.Debug("Map zoomed out");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to zoom out");
            }
        }

        /// <summary>
        /// Reset the map view
        /// </summary>
        private async Task ResetViewAsync()
        {
            try
            {
                await _googleEarthService.ResetViewAsync();
                Logger.Information("Map view reset");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to reset map view");
            }
        }

        /// <summary>
        /// Change the map layer
        /// </summary>
        public void ChangeMapLayer(string layerType)
        {
            try
            {
                CurrentMapLayer = layerType;
                _googleEarthService.ChangeMapLayer(layerType);
                Logger.Information("Map layer changed to: {LayerType}", layerType);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to change map layer to: {LayerType}", layerType);
            }
        }

        /// <summary>
        /// Start live tracking
        /// </summary>
        private void StartLiveTracking()
        {
            try
            {
                _googleEarthService.StartLiveTracking();
                Logger.Information("Live tracking started");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to start live tracking");
            }
        }

        /// <summary>
        /// Stop live tracking
        /// </summary>
        private void StopLiveTracking()
        {
            try
            {
                _googleEarthService.StopLiveTracking();
                Logger.Information("Live tracking stopped");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to stop live tracking");
            }
        }

        /// <summary>
        /// Load bus locations from service
        /// </summary>
        private async Task LoadBusLocationsAsync()
        {
            try
            {
                var locations = await _googleEarthService.GetBusLocationsAsync();
                ActiveBuses.Clear();
                foreach (var location in locations)
                {
                    ActiveBuses.Add(location);
                }
                Logger.Information("Bus locations loaded: {Count}", locations.Count);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load bus locations");
            }
        }

        /// <summary>
        /// Load sample data for demonstration
        /// </summary>
        private void LoadSampleData()
        {
            ActiveBuses.Add(new BusLocation { BusNumber = "101", RouteNumber = "Route 1", Status = "Active", Latitude = 40.7128, Longitude = -74.0060 });
            ActiveBuses.Add(new BusLocation { BusNumber = "102", RouteNumber = "Route 2", Status = "Active", Latitude = 40.7589, Longitude = -73.9851 });
            ActiveBuses.Add(new BusLocation { BusNumber = "103", RouteNumber = "Route 3", Status = "Maintenance", Latitude = 40.7282, Longitude = -73.7949 });
            ActiveBuses.Add(new BusLocation { BusNumber = "104", RouteNumber = "Route 1", Status = "Active", Latitude = 40.6892, Longitude = -74.0445 });
            ActiveBuses.Add(new BusLocation { BusNumber = "105", RouteNumber = "Route 4", Status = "Active", Latitude = 40.7505, Longitude = -73.9934 });
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
