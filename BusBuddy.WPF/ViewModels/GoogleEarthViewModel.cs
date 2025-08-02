using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using BusBuddy.WPF.Commands;
using BusBuddy.WPF.ViewModels;
using Serilog;
using Serilog.Context;

namespace BusBuddy.WPF.ViewModels
{
    /// <summary>
    /// ViewModel for Google Earth Integration View
    /// Provides geospatial mapping functionality with bus tracking
    /// </summary>
    public class GoogleEarthViewModel : INotifyPropertyChanged
    {
        private static readonly Serilog.ILogger Logger = Log.ForContext<GoogleEarthViewModel>();

        private bool _isLiveTrackingEnabled = true;
        private bool _isMapLoading;
        private object? _selectedBus;

        public GoogleEarthViewModel()
        {
            InitializeCommands();
            LoadSampleData();
        }

        #region Properties

        public bool IsLiveTrackingEnabled
        {
            get => _isLiveTrackingEnabled;
            set
            {
                _isLiveTrackingEnabled = value;
                OnPropertyChanged();
                Logger.Information("Live tracking {Status}", value ? "enabled" : "disabled");
            }
        }

        public bool IsMapLoading
        {
            get => _isMapLoading;
            set
            {
                _isMapLoading = value;
                OnPropertyChanged();
            }
        }

        public object? SelectedBus
        {
            get => _selectedBus;
            set
            {
                _selectedBus = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ActiveBusInfo> ActiveBuses { get; } = new();

        #endregion

        #region Commands

        public ICommand CenterOnFleetCommand { get; private set; } = null!;
        public ICommand ShowAllBusesCommand { get; private set; } = null!;
        public ICommand ShowRoutesCommand { get; private set; } = null!;
        public ICommand ShowSchoolsCommand { get; private set; } = null!;
        public ICommand TrackSelectedBusCommand { get; private set; } = null!;
        public ICommand InitializeMapCommand { get; private set; } = null!;
        public ICommand ZoomInCommand { get; private set; } = null!;
        public ICommand ZoomOutCommand { get; private set; } = null!;
        public ICommand ResetViewCommand { get; private set; } = null!;

        #endregion

        #region Methods

        private void InitializeCommands()
        {
            CenterOnFleetCommand = new RelayCommand(_ => CenterOnFleet());
            ShowAllBusesCommand = new RelayCommand(_ => ShowAllBuses());
            ShowRoutesCommand = new RelayCommand(_ => ShowRoutes());
            ShowSchoolsCommand = new RelayCommand(_ => ShowSchools());
            TrackSelectedBusCommand = new RelayCommand(_ => TrackSelectedBus(), _ => SelectedBus != null);
            InitializeMapCommand = new RelayCommand(async _ => await InitializeMapAsync());
            ZoomInCommand = new RelayCommand(_ => ZoomIn());
            ZoomOutCommand = new RelayCommand(_ => ZoomOut());
            ResetViewCommand = new RelayCommand(_ => ResetView());
        }

        private void LoadSampleData()
        {
            // Load sample bus data for demonstration
            ActiveBuses.Clear();
            ActiveBuses.Add(new ActiveBusInfo { BusNumber = "001", RouteNumber = "R1", Status = "En Route" });
            ActiveBuses.Add(new ActiveBusInfo { BusNumber = "002", RouteNumber = "R2", Status = "At School" });
            ActiveBuses.Add(new ActiveBusInfo { BusNumber = "003", RouteNumber = "R3", Status = "En Route" });
            ActiveBuses.Add(new ActiveBusInfo { BusNumber = "004", RouteNumber = "R4", Status = "Maintenance" });
            ActiveBuses.Add(new ActiveBusInfo { BusNumber = "005", RouteNumber = "R5", Status = "En Route" });
        }

        public void ChangeMapLayer(string layerType)
        {
            using (LogContext.PushProperty("MapOperation", "ChangeLayer"))
            {
                try
                {
                    Logger.Information("Changing map layer to {LayerType}", layerType);

                    // Simulate map layer change
                    Task.Delay(500).ContinueWith(_ =>
                    {
                        Logger.Information("Map layer changed successfully to {LayerType}", layerType);
                    });
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to change map layer to {LayerType}", layerType);
                }
            }
        }

        private void CenterOnFleet()
        {
            Logger.Information("Centering map on fleet");
            // Implementation would center the map on all active buses
        }

        private void ShowAllBuses()
        {
            Logger.Information("Showing all buses on map");
            // Implementation would display all bus markers
        }

        private void ShowRoutes()
        {
            Logger.Information("Showing routes on map");
            // Implementation would display route lines
        }

        private void ShowSchools()
        {
            Logger.Information("Showing schools on map");
            // Implementation would display school markers
        }

        private void TrackSelectedBus()
        {
            if (SelectedBus is ActiveBusInfo bus)
            {
                Logger.Information("Tracking bus {BusNumber}", bus.BusNumber);
                // Implementation would focus map on selected bus
            }
        }

        private async Task InitializeMapAsync()
        {
            using (LogContext.PushProperty("MapOperation", "Initialize"))
            {
                try
                {
                    IsMapLoading = true;
                    Logger.Information("Initializing Google Earth map");

                    // Simulate map initialization
                    await Task.Delay(2000);

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
        }

        private void ZoomIn()
        {
            Logger.Debug("Zooming in on map");
            // Implementation would increase map zoom level
        }

        private void ZoomOut()
        {
            Logger.Debug("Zooming out on map");
            // Implementation would decrease map zoom level
        }

        private void ResetView()
        {
            Logger.Information("Resetting map view to default");
            // Implementation would reset map to default view
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

    /// <summary>
    /// Information about an active bus for display in the Google Earth view
    /// </summary>
    public class ActiveBusInfo
    {
        public string BusNumber { get; set; } = string.Empty;
        public string RouteNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime LastUpdate { get; set; } = DateTime.Now;
    }
}
