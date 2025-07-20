using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Serilog;
using Serilog.Context;
using BusBuddy.WPF.Services;
using BusBuddy.WPF.Models;
using CommunityToolkit.Mvvm.Input;

namespace BusBuddy.WPF.ViewModels
{
    /// <summary>
    /// ViewModel for Google Earth integration
    /// Manages geospatial mapping and route visualization for transportation management
    /// Enhanced with BaseViewModel patterns and improved async handling
    /// </summary>
    public partial class GoogleEarthViewModel : BaseViewModel
    {
        private readonly IGoogleEarthService _googleEarthService;
        private bool _isLiveTrackingEnabled = true;
        private object? _selectedBus;
        private string _currentMapLayer = "Satellite";

        public GoogleEarthViewModel(IGoogleEarthService googleEarthService)
        {
            _googleEarthService = googleEarthService ?? throw new ArgumentNullException(nameof(googleEarthService));

            using (LogContext.PushProperty("ViewModelInitialization", "GoogleEarthViewModel"))
            {
                ActiveBuses = new ObservableCollection<BusLocation>();

                // Initialize commands using CommunityToolkit.Mvvm patterns
                InitializeMapCommand = new AsyncRelayCommand(InitializeMapAsync);
                CenterOnFleetCommand = new AsyncRelayCommand(CenterOnFleetAsync);
                ShowAllBusesCommand = new AsyncRelayCommand(ShowAllBusesAsync);
                ShowRoutesCommand = new AsyncRelayCommand(ShowRoutesAsync);
                ShowSchoolsCommand = new AsyncRelayCommand(ShowSchoolsAsync);
                TrackSelectedBusCommand = new AsyncRelayCommand(TrackSelectedBusAsync, () => SelectedBus != null);
                ZoomInCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(ZoomIn);
                ZoomOutCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(ZoomOut);
                ResetViewCommand = new AsyncRelayCommand(ResetViewAsync);

                // Initialize data
                LoadSampleData();

                Logger.Information("Google Earth ViewModel initialized with enhanced BaseViewModel patterns");
            }
        }

        #region Properties

        /// <summary>
        /// Collection of active bus locations
        /// </summary>
        public ObservableCollection<BusLocation> ActiveBuses { get; }

        /// <summary>
        /// Whether live tracking is enabled
        /// </summary>
        public bool IsLiveTrackingEnabled
        {
            get => _isLiveTrackingEnabled;
            set
            {
                if (SetProperty(ref _isLiveTrackingEnabled, value))
                {
                    LogUserInteraction($"LiveTracking{(value ? "Enabled" : "Disabled")}");

                    if (value)
                    {
                        _ = Task.Run(StartLiveTrackingAsync);
                    }
                    else
                    {
                        _ = Task.Run(StopLiveTrackingAsync);
                    }
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
                if (SetProperty(ref _selectedBus, value))
                {
                    // Update command can execute state
                    ((AsyncRelayCommand)TrackSelectedBusCommand).NotifyCanExecuteChanged();

                    if (value is BusLocation bus)
                    {
                        LogUserInteraction("BusSelected", new { BusNumber = bus.BusNumber, Route = bus.RouteNumber });
                    }
                }
            }
        }

        /// <summary>
        /// Current map layer type
        /// </summary>
        public string CurrentMapLayer
        {
            get => _currentMapLayer;
            set => SetProperty(ref _currentMapLayer, value);
        }

        #endregion

        #region Commands

        public IAsyncRelayCommand InitializeMapCommand { get; }
        public IAsyncRelayCommand CenterOnFleetCommand { get; }
        public IAsyncRelayCommand ShowAllBusesCommand { get; }
        public IAsyncRelayCommand ShowRoutesCommand { get; }
        public IAsyncRelayCommand ShowSchoolsCommand { get; }
        public IAsyncRelayCommand TrackSelectedBusCommand { get; }
        public IRelayCommand ZoomInCommand { get; }
        public IRelayCommand ZoomOutCommand { get; }
        public IAsyncRelayCommand ResetViewCommand { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize the Google Earth map with enhanced error handling and performance tracking
        /// </summary>
        private async Task InitializeMapAsync()
        {
            await LoadDataAsync(async () =>
            {
                using (LogContext.PushProperty("GeospatialOperation", "MapInitialization"))
                {
                    await _googleEarthService.InitializeAsync();
                    await LoadBusLocationsAsync();

                    Logger.Information("Google Earth map initialized successfully");
                }
            });
        }

        /// <summary>
        /// Center the map on the fleet with background processing
        /// </summary>
        private async Task CenterOnFleetAsync()
        {
            await ExecuteCommandAsync(async () =>
            {
                using (LogContext.PushProperty("GeospatialOperation", "CenterOnFleet"))
                {
                    await _googleEarthService.CenterOnFleetAsync();
                    LogUserInteraction("CenterOnFleet");
                }
            }, "CenterOnFleet");
        }

        /// <summary>
        /// Show all buses on the map with performance optimization
        /// </summary>
        private async Task ShowAllBusesAsync()
        {
            await ExecuteCommandAsync(async () =>
            {
                using (LogContext.PushProperty("GeospatialOperation", "ShowAllBuses"))
                {
                    await _googleEarthService.ShowAllBusesAsync();
                    LogUserInteraction("ShowAllBuses");
                }
            }, "ShowAllBuses");
        }

        /// <summary>
        /// Show routes on the map
        /// </summary>
        private async Task ShowRoutesAsync()
        {
            await ExecuteCommandAsync(async () =>
            {
                using (LogContext.PushProperty("GeospatialOperation", "ShowRoutes"))
                {
                    await _googleEarthService.ShowRoutesAsync();
                    LogUserInteraction("ShowRoutes");
                }
            }, "ShowRoutes");
        }

        /// <summary>
        /// Show schools on the map
        /// </summary>
        private async Task ShowSchoolsAsync()
        {
            await ExecuteCommandAsync(async () =>
            {
                using (LogContext.PushProperty("GeospatialOperation", "ShowSchools"))
                {
                    await _googleEarthService.ShowSchoolsAsync();
                    LogUserInteraction("ShowSchools");
                }
            }, "ShowSchools");
        }

        /// <summary>
        /// Track the selected bus with enhanced validation
        /// </summary>
        private async Task TrackSelectedBusAsync()
        {
            if (SelectedBus is not BusLocation bus)
            {
                Logger.Warning("No bus selected for tracking operation");
                return;
            }

            await ExecuteCommandAsync(async () =>
            {
                using (LogContext.PushProperty("GeospatialOperation", "TrackBus"))
                using (LogContext.PushProperty("BusNumber", bus.BusNumber))
                {
                    await _googleEarthService.TrackBusAsync(bus.BusNumber);
                    LogUserInteraction("TrackBus", new { BusNumber = bus.BusNumber, Route = bus.RouteNumber });
                }
            }, "TrackBus");
        }

        /// <summary>
        /// Zoom in on the map with error handling
        /// </summary>
        private void ZoomIn()
        {
            try
            {
                using (LogContext.PushProperty("GeospatialOperation", "ZoomIn"))
                {
                    _googleEarthService.ZoomIn();
                    LogUserInteraction("ZoomIn");
                    Logger.Debug("Map zoomed in successfully");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to zoom in");
                ErrorMessage = "Failed to zoom in. Please try again.";
            }
        }

        /// <summary>
        /// Zoom out on the map with error handling
        /// </summary>
        private void ZoomOut()
        {
            try
            {
                using (LogContext.PushProperty("GeospatialOperation", "ZoomOut"))
                {
                    _googleEarthService.ZoomOut();
                    LogUserInteraction("ZoomOut");
                    Logger.Debug("Map zoomed out successfully");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to zoom out");
                ErrorMessage = "Failed to zoom out. Please try again.";
            }
        }

        /// <summary>
        /// Reset the map view
        /// </summary>
        private async Task ResetViewAsync()
        {
            await ExecuteCommandAsync(async () =>
            {
                using (LogContext.PushProperty("GeospatialOperation", "ResetView"))
                {
                    await _googleEarthService.ResetViewAsync();
                    LogUserInteraction("ResetView");
                }
            }, "ResetView");
        }

        /// <summary>
        /// Change the map layer (called from debounced UI event)
        /// Enhanced with performance optimization and structured logging
        /// </summary>
        public void ChangeMapLayer(string layerType)
        {
            try
            {
                using (LogContext.PushProperty("GeospatialOperation", "ChangeMapLayer"))
                using (LogContext.PushProperty("LayerType", layerType))
                {
                    CurrentMapLayer = layerType;
                    _googleEarthService.ChangeMapLayer(layerType);

                    LogUserInteraction("MapLayerChanged", new { LayerType = layerType });
                    Logger.Information("Map layer changed successfully to: {LayerType}", layerType);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to change map layer to: {LayerType}", layerType);
                ErrorMessage = $"Failed to change map layer to {layerType}. Please try again.";
            }
        }

        /// <summary>
        /// Start live tracking with background processing
        /// </summary>
        private async Task StartLiveTrackingAsync()
        {
            try
            {
                using (LogContext.PushProperty("GeospatialOperation", "StartLiveTracking"))
                {
                    await Task.Run(() => _googleEarthService.StartLiveTracking());
                    Logger.Information("Live tracking started successfully");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to start live tracking");
                ErrorMessage = "Failed to start live tracking. Please try again.";
            }
        }

        /// <summary>
        /// Stop live tracking with background processing
        /// </summary>
        private async Task StopLiveTrackingAsync()
        {
            try
            {
                using (LogContext.PushProperty("GeospatialOperation", "StopLiveTracking"))
                {
                    await Task.Run(() => _googleEarthService.StopLiveTracking());
                    Logger.Information("Live tracking stopped successfully");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to stop live tracking");
                ErrorMessage = "Failed to stop live tracking. Please try again.";
            }
        }

        /// <summary>
        /// Load bus locations from service with enhanced error handling
        /// </summary>
        private async Task LoadBusLocationsAsync()
        {
            try
            {
                using (LogContext.PushProperty("DataOperation", "LoadBusLocations"))
                {
                    var locations = await _googleEarthService.GetBusLocationsAsync();

                    ActiveBuses.Clear();
                    foreach (var location in locations)
                    {
                        ActiveBuses.Add(location);
                    }

                    Logger.Information("Bus locations loaded successfully: {Count}", locations.Count);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load bus locations");
                ErrorMessage = "Failed to load bus locations. Please refresh to try again.";
            }
        }

        /// <summary>
        /// Load sample data for demonstration
        /// Enhanced with structured logging
        /// </summary>
        private void LoadSampleData()
        {
            try
            {
                using (LogContext.PushProperty("DataOperation", "LoadSampleData"))
                {
                    ActiveBuses.Add(new BusLocation { BusNumber = "101", RouteNumber = "Route 1", Status = "Active", Latitude = 40.7128, Longitude = -74.0060 });
                    ActiveBuses.Add(new BusLocation { BusNumber = "102", RouteNumber = "Route 2", Status = "Active", Latitude = 40.7589, Longitude = -73.9851 });
                    ActiveBuses.Add(new BusLocation { BusNumber = "103", RouteNumber = "Route 3", Status = "Maintenance", Latitude = 40.7282, Longitude = -73.7949 });
                    ActiveBuses.Add(new BusLocation { BusNumber = "104", RouteNumber = "Route 1", Status = "Active", Latitude = 40.6892, Longitude = -74.0445 });
                    ActiveBuses.Add(new BusLocation { BusNumber = "105", RouteNumber = "Route 4", Status = "Active", Latitude = 40.7505, Longitude = -73.9934 });

                    Logger.Information("Sample bus location data loaded: {Count} buses", ActiveBuses.Count);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to load sample data");
            }
        }

        #endregion
    }
}
