// Artifact: Updated DashboardViewModel.cs
// Changes:
// - Added support for SfHubTile integration as per reference.md recommendations.
// - Introduced DashboardTileModel class (implements INotifyPropertyChanged) for tile data.
// - Added DashboardTiles collection and initialization.
// - Added UpdateDashboardTiles() method called during RefreshDashboardDataAsync() to keep tiles in sync with metrics.
// - Included a simple RelayCommand class for NavigateCommand (assuming no existing command infrastructure; can be replaced if a more robust one exists).
// - Updated RefreshDashboardDataAsync() to call UpdateDashboardTiles().
// - No changes to docking events as they belong in the view code-behind.
// - Ensured compatibility with existing performance optimizations and auto-refresh.

using System.Threading.Tasks;
using System.Windows.Input;
using BusBuddy.WPF.Services;
using BusBuddy.WPF.ViewModels;
using BusBuddy.WPF.ViewModels.Schedule;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using BusBuddy.WPF.Utilities;
using Serilog.Context;
using BusBuddy.Core.Services;
using BusBuddy.Core.Services.Interfaces;
using System.Threading;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

// Disable obsolete warnings for the entire file
#pragma warning disable CS0618 // Type or member is obsolete

namespace BusBuddy.WPF.ViewModels
{
    public class DashboardTileModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private string _header = string.Empty;
        public string Header
        {
            get => _header;
            set { _header = value; OnPropertyChanged(nameof(Header)); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }

        private string _imageSource = string.Empty;
        public string ImageSource
        {
            get => _imageSource;
            set { _imageSource = value; OnPropertyChanged(nameof(ImageSource)); }
        }

        private int _notificationCount;
        public int NotificationCount
        {
            get => _notificationCount;
            set { _notificationCount = value; OnPropertyChanged(nameof(NotificationCount)); }
        }

        private string _tileType = string.Empty;
        public string TileType
        {
            get => _tileType;
            set { _tileType = value; OnPropertyChanged(nameof(TileType)); }
        }

        public ICommand? NavigateCommand { get; set; }
    }

    // Syncfusion-style RelayCommand implementation with proper nullable reference types
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _execute(parameter);
    }

    public class DashboardViewModel : BaseViewModel
    {
        private readonly IRoutePopulationScaffold _routePopulationScaffold;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DashboardViewModel> _logger;
        private readonly PerformanceMonitor _performanceMonitor;
        private readonly IBusService _busService;
        private readonly IDriverService _driverService;
        private readonly IRouteService _routeService;
        private readonly IDashboardMetricsService _dashboardMetricsService;
        private readonly IEnhancedCachingService _cachingService;
        private StudentListViewModel? _studentListViewModel;
        private BusManagementViewModel? _busManagementViewModel;
        private DriverManagementViewModel? _driverManagementViewModel;
        private RouteManagementViewModel? _routeManagementViewModel;
        // In-development module view models
        private ScheduleManagementViewModel? _scheduleManagementViewModel;
        private StudentManagementViewModel? _studentManagementViewModel;
        private MaintenanceTrackingViewModel? _maintenanceTrackingViewModel;
        private FuelManagementViewModel? _fuelManagementViewModel;
        private ActivityLogViewModel? _activityLogViewModel;
        // REMOVED: _ticketManagementViewModel - deprecated module

        // Performance tracking metrics
        private TimeSpan _initializationTime;
        private TimeSpan _routePopulationTime;
        private TimeSpan _studentListLoadTime;

        // Dashboard metrics
        private int _totalBuses;
        private int _totalDrivers;
        private int _totalActiveRoutes;

        // Real-time dashboard data
        private int _activeBusCount;
        private int _availableDriverCount;
        private double _fleetActivePercentage;
        private double _driverAvailabilityPercentage;
        private double _routeCoveragePercentage;
        private string _nextUpdateTime = "Calculating...";
        private System.Threading.Timer? _refreshTimer;
        private volatile bool _isRefreshing = false; // State guard to prevent concurrent refreshes

        // Hub Tile support
        private ObservableCollection<DashboardTileModel> _dashboardTiles;

        protected override ILogger? GetLogger() => _logger;

        public StudentListViewModel StudentListViewModel
        {
            get
            {
                try
                {
                    if (_studentListViewModel == null)
                    {
                        _logger.LogInformation("Resolving StudentListViewModel from service provider");
                        _studentListViewModel = _serviceProvider.GetRequiredService<StudentListViewModel>();
                        _logger.LogInformation("Successfully resolved StudentListViewModel");
                    }
                    return _studentListViewModel;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to resolve StudentListViewModel: {0}", ex.Message);
                    // Re-throw to ensure the error is caught by LoadDataAsync
                    throw;
                }
            }
        }

        public BusManagementViewModel BusManagementViewModel
        {
            get
            {
                if (_busManagementViewModel == null)
                {
#if DEBUG
#if DEBUG
#endif
#endif
                    _busManagementViewModel = _serviceProvider.GetRequiredService<BusManagementViewModel>();
#if DEBUG
#if DEBUG
#endif
#endif
                }
                return _busManagementViewModel;
            }
        }

        public DriverManagementViewModel DriverManagementViewModel
        {
            get
            {
                if (_driverManagementViewModel == null)
                {
#if DEBUG
#endif
                    _driverManagementViewModel = _serviceProvider.GetRequiredService<DriverManagementViewModel>();
#if DEBUG
#endif
                }
                return _driverManagementViewModel;
            }
        }

        public RouteManagementViewModel RouteManagementViewModel
        {
            get
            {
                if (_routeManagementViewModel == null)
                {
#if DEBUG
#endif
                    _routeManagementViewModel = _serviceProvider.GetRequiredService<RouteManagementViewModel>();
#if DEBUG
#endif
                }
                return _routeManagementViewModel;
            }
        }

        public ScheduleManagementViewModel ScheduleManagementViewModel
        {
            get
            {
                try
                {
                    if (_scheduleManagementViewModel == null)
                    {
#if DEBUG
#endif
                        _scheduleManagementViewModel = _serviceProvider.GetRequiredService<ScheduleManagementViewModel>();
#if DEBUG
#endif
                    }
                    return _scheduleManagementViewModel;
                }
                catch (Exception ex)
                {
#if DEBUG
#endif
                    _logger.LogError(ex, "Failed to resolve ScheduleManagementViewModel: {0}", ex.Message);
                    throw;
                }
            }
        }

        public StudentManagementViewModel StudentManagementViewModel
        {
            get
            {
                try
                {
                    if (_studentManagementViewModel == null)
                    {
                        _logger.LogInformation("Resolving StudentManagementViewModel from service provider");
                        _studentManagementViewModel = _serviceProvider.GetRequiredService<StudentManagementViewModel>();
                        _logger.LogInformation("Successfully resolved StudentManagementViewModel");
                    }
                    return _studentManagementViewModel;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to resolve StudentManagementViewModel: {0}", ex.Message);
                    throw;
                }
            }
        }

        public MaintenanceTrackingViewModel MaintenanceTrackingViewModel
        {
            get
            {
                try
                {
                    if (_maintenanceTrackingViewModel == null)
                    {
                        _logger.LogInformation("Resolving MaintenanceTrackingViewModel from service provider");
                        _maintenanceTrackingViewModel = _serviceProvider.GetRequiredService<MaintenanceTrackingViewModel>();
                        _logger.LogInformation("Successfully resolved MaintenanceTrackingViewModel");
                    }
                    return _maintenanceTrackingViewModel;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to resolve MaintenanceTrackingViewModel: {0}", ex.Message);
                    throw;
                }
            }
        }

        public FuelManagementViewModel FuelManagementViewModel
        {
            get
            {
                try
                {
                    if (_fuelManagementViewModel == null)
                    {
                        _logger.LogInformation("Resolving FuelManagementViewModel from service provider");
#if DEBUG
#endif
                        _fuelManagementViewModel = _serviceProvider.GetRequiredService<FuelManagementViewModel>();
                        _logger.LogInformation("Successfully resolved FuelManagementViewModel");
#if DEBUG
#endif
                    }
                    return _fuelManagementViewModel;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to resolve FuelManagementViewModel: {0}", ex.Message);
#if DEBUG
#endif
#if DEBUG
#endif
                    throw;
                }
            }
        }

        public ActivityLogViewModel ActivityLogViewModel
        {
            get
            {
                try
                {
                    if (_activityLogViewModel == null)
                    {
                        _logger.LogInformation("Resolving ActivityLogViewModel from service provider");
                        _activityLogViewModel = _serviceProvider.GetRequiredService<ActivityLogViewModel>();
                        _logger.LogInformation("Successfully resolved ActivityLogViewModel");
                    }
                    return _activityLogViewModel;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to resolve ActivityLogViewModel: {0}", ex.Message);
                    throw;
                }
            }
        }

        // REMOVED: TicketManagementViewModel property - deprecated module completely removed

        public DashboardViewModel(
            IRoutePopulationScaffold routePopulationScaffold,
            IServiceProvider serviceProvider,
            ILogger<DashboardViewModel> logger,
            PerformanceMonitor performanceMonitor,
            IBusService busService,
            IDriverService driverService,
            IRouteService routeService,
            IDashboardMetricsService dashboardMetricsService,
            IEnhancedCachingService cachingService)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine("[DEBUG] DashboardViewModel Constructor");
#endif
            _routePopulationScaffold = routePopulationScaffold ?? throw new ArgumentNullException(nameof(routePopulationScaffold));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _performanceMonitor = performanceMonitor ?? throw new ArgumentNullException(nameof(performanceMonitor));
            _busService = busService ?? throw new ArgumentNullException(nameof(busService));
            _driverService = driverService ?? throw new ArgumentNullException(nameof(driverService));
            _routeService = routeService ?? throw new ArgumentNullException(nameof(routeService));
            _dashboardMetricsService = dashboardMetricsService ?? throw new ArgumentNullException(nameof(dashboardMetricsService));
            _cachingService = cachingService ?? throw new ArgumentNullException(nameof(cachingService));

            _logger.LogInformation("DashboardViewModel constructor completed successfully");

            // Initialize auto-refresh timer (5-second intervals as per development plan)
            InitializeRefreshTimer();

            // Initialize dashboard tiles for SfHubTile support
            _dashboardTiles = InitializeDashboardTiles();
        }

        private ObservableCollection<DashboardTileModel> InitializeDashboardTiles()
        {
            var tiles = new ObservableCollection<DashboardTileModel>
            {
                new DashboardTileModel { Header = "Bus Fleet", Title = $"{ActiveBusCount} Active", ImageSource = "/Assets/Icons/bus_icon.png", NotificationCount = 0, TileType = "Flip", NavigateCommand = new RelayCommand(o => NavigateToBusManagement()) },
                new DashboardTileModel { Header = "Drivers", Title = $"{AvailableDriverCount} Available", ImageSource = "/Assets/Icons/driver_icon.png", NotificationCount = 0, TileType = "Flip", NavigateCommand = new RelayCommand(o => NavigateToDriverManagement()) },
                new DashboardTileModel { Header = "Active Routes", Title = $"{TotalActiveRoutes} Routes", ImageSource = "/Assets/Icons/route_icon.png", NotificationCount = 0, TileType = "Flip", NavigateCommand = new RelayCommand(o => NavigateToRouteManagement()) }
            };
            return tiles;
        }

        public ObservableCollection<DashboardTileModel> DashboardTiles => _dashboardTiles;

        private void UpdateDashboardTiles()
        {
            var fleetTile = _dashboardTiles.FirstOrDefault(t => t.Header == "Bus Fleet");
            if (fleetTile != null) fleetTile.Title = $"{ActiveBusCount} Active";

            var driverTile = _dashboardTiles.FirstOrDefault(t => t.Header == "Drivers");
            if (driverTile != null) driverTile.Title = $"{AvailableDriverCount} Available";

            var routeTile = _dashboardTiles.FirstOrDefault(t => t.Header == "Active Routes");
            if (routeTile != null) routeTile.Title = $"{TotalActiveRoutes} Routes";
        }

        private void NavigateToBusManagement()
        {
            // Implement navigation logic, e.g., switch to BusManagement panel
            _logger.LogInformation("Navigating to Bus Management");
        }

        private void NavigateToDriverManagement()
        {
            _logger.LogInformation("Navigating to Driver Management");
        }

        private void NavigateToRouteManagement()
        {
            _logger.LogInformation("Navigating to Route Management");
        }

        private void InitializeRefreshTimer()
        {
            _logger.LogInformation("Initializing dashboard auto-refresh timer (5-second intervals)");

            // Set initial next update time
            UpdateNextUpdateTime();

            // Create timer that fires every 5 seconds
            _refreshTimer = new System.Threading.Timer(
                callback: TimerCallback,
                state: null,
                dueTime: TimeSpan.FromSeconds(5),
                period: TimeSpan.FromSeconds(5));
        }

        private void TimerCallback(object? state)
        {
            // Use fire-and-forget pattern to avoid blocking timer thread
            _ = Task.Run(async () => await PerformTimedRefreshAsync());
        }

        private void UpdateNextUpdateTime()
        {
            NextUpdateTime = DateTime.Now.AddSeconds(5).ToString("HH:mm:ss");
        }

        private async Task PerformTimedRefreshAsync()
        {
            // Prevent concurrent refreshes
            if (_isRefreshing)
            {
                _logger.LogDebug("Skipping timed refresh - already in progress");
                return;
            }

            try
            {
                _isRefreshing = true;
                _logger.LogDebug("Performing timed dashboard refresh");

                // Update next refresh time
                UpdateNextUpdateTime();

                // Perform lightweight refresh (avoid blocking operations)
                await RefreshDashboardDataAsync();

                _logger.LogDebug("Timed dashboard refresh completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during timed dashboard refresh: {ErrorMessage}", ex.Message);
                // Continue running timer even if refresh fails
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        protected void DisposeTimer()
        {
            _refreshTimer?.Dispose();
            _refreshTimer = null;
            _logger.LogInformation("Dashboard refresh timer disposed");
        }

        public string InitializationTimeFormatted => FormatUtils.FormatDuration((int)_initializationTime.TotalMilliseconds / 1000);
        public string RoutePopulationTimeFormatted => FormatUtils.FormatDuration((int)_routePopulationTime.TotalMilliseconds / 1000);
        public string StudentListLoadTimeFormatted => FormatUtils.FormatDuration((int)_studentListLoadTime.TotalMilliseconds / 1000);

        /// <summary>
        /// Refresh dashboard data - called by UI refresh actions
        /// Implements lightweight refresh for tile updates
        /// </summary>
        public async Task RefreshDashboardDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting dashboard data refresh");

                // Quick metrics refresh without full initialization
                await LoadCriticalDataAsync();

                // Calculate additional real-time metrics
                await CalculateRealTimeMetricsAsync();

                // Update hub tiles
                UpdateDashboardTiles();

                // Trigger property change notifications for UI updates
                OnPropertyChanged(nameof(TotalBuses));
                OnPropertyChanged(nameof(TotalDrivers));
                OnPropertyChanged(nameof(TotalActiveRoutes));
                OnPropertyChanged(nameof(InitializationTimeFormatted));
                OnPropertyChanged(nameof(ActiveBusCount));
                OnPropertyChanged(nameof(AvailableDriverCount));
                OnPropertyChanged(nameof(FleetActivePercentageFormatted));
                OnPropertyChanged(nameof(DriverAvailabilityPercentageFormatted));
                OnPropertyChanged(nameof(RouteCoveragePercentageFormatted));
                OnPropertyChanged(nameof(DashboardTiles));  // Ensure tiles update

                _logger.LogInformation("Dashboard data refresh completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during dashboard refresh: {ErrorMessage}", ex.Message);
                throw;
            }
        }

        private async Task CalculateRealTimeMetricsAsync()
        {
            try
            {
                _logger.LogDebug("Calculating real-time dashboard metrics");

                // Calculate actual metrics based on real data
                await CalculateActualFleetMetrics();
                await CalculateActualDriverMetrics();
                await CalculateActualRouteMetrics();

                _logger.LogDebug("Real-time metrics calculated - Active: {ActiveBuses}/{TotalBuses} ({FleetPercentage:F1}%), Available: {AvailableDrivers}/{TotalDrivers} ({DriverPercentage:F1}%), Coverage: {RoutePercentage:F1}%",
                    ActiveBusCount, TotalBuses, FleetActivePercentage,
                    AvailableDriverCount, TotalDrivers, DriverAvailabilityPercentage,
                    RouteCoveragePercentage);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error calculating real-time metrics: {ErrorMessage}", ex.Message);
                // Set safe defaults on error
                FleetActivePercentage = 0.0;
                DriverAvailabilityPercentage = 0.0;
                RouteCoveragePercentage = 0.0;
                ActiveBusCount = 0;
                AvailableDriverCount = 0;
            }
        }

        private async Task CalculateActualFleetMetrics()
        {
            try
            {
                if (TotalBuses > 0)
                {
                    // Get actual active bus count from the bus service
                    var buses = await _busService.GetAllBusesAsync();
                    var activeBuses = buses.Count(b => b.Status == "Active" || b.Status == "In Service");

                    ActiveBusCount = activeBuses;
                    FleetActivePercentage = TotalBuses > 0 ? (double)ActiveBusCount / TotalBuses * 100.0 : 0.0;
                }
                else
                {
                    FleetActivePercentage = 0.0;
                    ActiveBusCount = 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error calculating fleet metrics, using fallback");
                // Fallback to reasonable estimate if data unavailable
                if (TotalBuses > 0)
                {
                    FleetActivePercentage = 90.0; // Conservative estimate
                    ActiveBusCount = (int)(TotalBuses * 0.90);
                }
            }
        }

        private async Task CalculateActualDriverMetrics()
        {
            try
            {
                if (TotalDrivers > 0)
                {
                    // Get actual available driver count from the driver service
                    var drivers = await _driverService.GetAllDriversAsync();
                    var availableDrivers = drivers.Count(d => d.Status == "Active");

                    AvailableDriverCount = availableDrivers;
                    DriverAvailabilityPercentage = TotalDrivers > 0 ? (double)AvailableDriverCount / TotalDrivers * 100.0 : 0.0;
                }
                else
                {
                    DriverAvailabilityPercentage = 0.0;
                    AvailableDriverCount = 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error calculating driver metrics, using fallback");
                // Fallback to reasonable estimate if data unavailable
                if (TotalDrivers > 0)
                {
                    DriverAvailabilityPercentage = 85.0; // Conservative estimate
                    AvailableDriverCount = (int)(TotalDrivers * 0.85);
                }
            }
        }

        private async Task CalculateActualRouteMetrics()
        {
            try
            {
                if (TotalActiveRoutes > 0)
                {
                    // Get actual route coverage from the route service
                    var routes = await _routeService.GetAllActiveRoutesAsync();
                    var coveredRoutes = routes.Count(r => r.AMVehicleId.HasValue || r.PMVehicleId.HasValue);

                    RouteCoveragePercentage = TotalActiveRoutes > 0 ? (double)coveredRoutes / TotalActiveRoutes * 100.0 : 0.0;
                }
                else
                {
                    RouteCoveragePercentage = 0.0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error calculating route metrics, using fallback");
                // Fallback to reasonable estimate if data unavailable
                if (TotalActiveRoutes > 0)
                {
                    RouteCoveragePercentage = 95.0; // Conservative estimate
                }
            }
        }

        // Dashboard metrics properties
        public int TotalBuses
        {
            get => _totalBuses;
            private set
            {
                if (_totalBuses != value)
                {
                    _totalBuses = value;
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] TotalBuses set to: {value}");
#endif
                    OnPropertyChanged();
                }
            }
        }

        public int TotalDrivers
        {
            get => _totalDrivers;
            private set
            {
                if (_totalDrivers != value)
                {
                    _totalDrivers = value;
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] TotalDrivers set to: {value}");
#endif
                    OnPropertyChanged();
                }
            }
        }

        public int TotalActiveRoutes
        {
            get => _totalActiveRoutes;
            private set
            {
                if (_totalActiveRoutes != value)
                {
                    _totalActiveRoutes = value;
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] TotalActiveRoutes set to: {value}");
#endif
                    OnPropertyChanged();
                }
            }
        }

        // Real-time dashboard properties
        public int ActiveBusCount
        {
            get => _activeBusCount;
            private set
            {
                if (_activeBusCount != value)
                {
                    _activeBusCount = value;
                    OnPropertyChanged();
                }
            }
        }

        public int AvailableDriverCount
        {
            get => _availableDriverCount;
            private set
            {
                if (_availableDriverCount != value)
                {
                    _availableDriverCount = value;
                    OnPropertyChanged();
                }
            }
        }

        public double FleetActivePercentage
        {
            get => _fleetActivePercentage;
            private set
            {
                if (Math.Abs(_fleetActivePercentage - value) > 0.01)
                {
                    _fleetActivePercentage = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FleetActivePercentageFormatted));
                }
            }
        }

        public string FleetActivePercentageFormatted => $"{FleetActivePercentage:F1}%";

        public double DriverAvailabilityPercentage
        {
            get => _driverAvailabilityPercentage;
            private set
            {
                if (Math.Abs(_driverAvailabilityPercentage - value) > 0.01)
                {
                    _driverAvailabilityPercentage = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DriverAvailabilityPercentageFormatted));
                }
            }
        }

        public string DriverAvailabilityPercentageFormatted => $"{DriverAvailabilityPercentage:F1}%";

        public double RouteCoveragePercentage
        {
            get => _routeCoveragePercentage;
            private set
            {
                if (Math.Abs(_routeCoveragePercentage - value) > 0.01)
                {
                    _routeCoveragePercentage = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(RouteCoveragePercentageFormatted));
                }
            }
        }

        public string RouteCoveragePercentageFormatted => $"{RouteCoveragePercentage:F1}%";

        public string NextUpdateTime
        {
            get => _nextUpdateTime;
            private set
            {
                if (_nextUpdateTime != value)
                {
                    _nextUpdateTime = value;
                    OnPropertyChanged();
                }
            }
        }

        // Track initialization state
        private bool _isCriticalDataLoaded;
        private bool _isFullyInitialized;

        public bool IsCriticalDataLoaded
        {
            get => _isCriticalDataLoaded;
            private set
            {
                if (_isCriticalDataLoaded != value)
                {
                    _isCriticalDataLoaded = value;
#if DEBUG
#endif
                    OnPropertyChanged();
                }
            }
        }

        public bool IsFullyInitialized
        {
            get => _isFullyInitialized;
            private set
            {
                if (_isFullyInitialized != value)
                {
                    _isFullyInitialized = value;
#if DEBUG
#endif
                    OnPropertyChanged();
                }
            }
        }

        public async Task InitializeAsync()
        {
            var totalStopwatch = Stopwatch.StartNew();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)); // Add 10-second timeout
            var ct = cts.Token;

            _logger.LogInformation("DashboardViewModel.InitializeAsync called");
#if DEBUG
#endif

            try
            {
#if DEBUG
#endif
#if DEBUG
#endif
                // Use LogContext to add structured properties to all log entries within this scope
                using (LogContext.PushProperty("StartupStep", "DashboardInitialization"))
                {
#if DEBUG
#endif
                    // CRITICAL OPTIMIZATION: Always show UI first, then load data
                    // This ensures the dashboard is visible even if data takes time to load
                    IsCriticalDataLoaded = true;
#if DEBUG
#endif

#if DEBUG
#endif
                    // First try quick cache load - avoid any DB hits if possible
                    if (await TryLoadFromCacheAsync())
                    {
                        _logger.LogInformation("Loaded dashboard data from cache in {DurationMs}ms",
                            totalStopwatch.ElapsedMilliseconds);
#if DEBUG
#endif

                        // Still load remaining data, but don't block UI on it
                        _ = LoadRemainingDataInBackgroundAsync(totalStopwatch);
#if DEBUG
                        Debug.WriteLine("[DEBUG] DashboardViewModel.InitializeAsync END (cache path)");
#endif
                        return;
                    }

#if DEBUG
#endif
                    Debug.WriteLine("[DEBUG] DashboardViewModel.InitializeAsync: Adding Task.Delay(50ms) for UI responsiveness");
                    // Use a Task.Delay to ensure UI responsiveness even if data loading is in progress
                    await Task.Delay(50, ct);

#if DEBUG
#endif
                    // Phase 1: Quick load of critical metrics with timeout protection
                    Task criticalDataTask = Task.Run(async () =>
                    {
#if DEBUG
#endif
                        await LoadCriticalDataAsync(ct);
#if DEBUG
#endif
                    }, ct);

#if DEBUG
#endif
                    // Use a timeout to ensure we don't block the UI for too long
                    if (await Task.WhenAny(criticalDataTask, Task.Delay(2000, ct)) != criticalDataTask)
                    {
                        _logger.LogWarning("Critical data load taking too long, continuing with UI display");
#if DEBUG
#endif
                        // Continue anyway - we'll show default values
                    }
                    else
                    {
#if DEBUG
#endif
                    }

                    // Record time for critical path
                    var criticalPathTime = totalStopwatch.Elapsed;
                    _logger.LogInformation("Critical dashboard data loaded in {DurationMs}ms",
                        criticalPathTime.TotalMilliseconds);
#if DEBUG
#endif

#if DEBUG
#endif
                    // Phase 2: Load remaining data in low-priority background thread
                    // Using ConfigureAwait(false) to avoid blocking the UI thread
                    _ = LoadRemainingDataInBackgroundAsync(totalStopwatch).ConfigureAwait(false);
#if DEBUG
#endif
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Dashboard initialization was canceled due to timeout after {ElapsedMs}ms",
                    totalStopwatch.ElapsedMilliseconds);

#if DEBUG
#endif
                // Still mark as loaded so UI is visible
                IsCriticalDataLoaded = true;
#if DEBUG
#endif
            }
            catch (Exception ex)
            {
                totalStopwatch.Stop();
                ErrorMessage = $"Initialize failed: {ex.Message}";
                _logger.LogError(ex, "DashboardViewModel.InitializeAsync failed with exception after {ElapsedMs}ms: {ErrorMessage}",
                    totalStopwatch.ElapsedMilliseconds, ex.Message);

#if DEBUG
#endif
#if DEBUG
#endif
                // Still mark as loaded so UI is visible with error state
                IsCriticalDataLoaded = true;
#if DEBUG
#endif

                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "Inner exception: {ErrorMessage}", ex.InnerException.Message);
#if DEBUG
#endif
                }
            }
#if DEBUG
#endif
        }

        // Extracted background loading into a separate method for clarity
        private async Task LoadRemainingDataInBackgroundAsync(Stopwatch totalStopwatch)
        {
#if DEBUG
#endif
#if DEBUG
#endif
            // Use a background CancellationTokenSource with a much longer timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var ct = cts.Token;

            try
            {
#if DEBUG
#endif
#if DEBUG
#endif
                // Lower thread priority to avoid impacting UI responsiveness
                Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

#if DEBUG
#endif
                // OPTIMIZATION: Longer delay for non-critical loading to ensure UI is fully rendered
                await Task.Delay(500, ct);

#if DEBUG
#endif
                // OPTIMIZATION: Load minimal data for display first, then incrementally load more
                // Create multiple stages with increasing timeouts to prioritize important data

#if DEBUG
#endif
                // Stage 1: Minimal route data - 5 second timeout (highest priority)
                var routeMetadataTask = Task.Run(async () =>
                {
                    try
                    {
#if DEBUG
#endif
                        var routeStopwatch = Stopwatch.StartNew();
#if DEBUG
#endif
                        await LoadMinimalRouteDataAsync(ct);
                        routeStopwatch.Stop();
                        _routePopulationTime = routeStopwatch.Elapsed;
                        _logger.LogInformation("Minimal route data loaded in {ElapsedMs}ms",
                            _routePopulationTime.TotalMilliseconds);
#if DEBUG
#endif
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error loading minimal route data");
#if DEBUG
#endif
                    }
                }, ct);

#if DEBUG
#endif
                // Wait for route metadata with a reasonable timeout
                if (!await WaitForTaskWithTimeout(routeMetadataTask, 5000, "route metadata loading"))
                {
                    _logger.LogWarning("Route metadata loading timed out, continuing with other initializations");
#if DEBUG
#endif
                }
                else
                {
#if DEBUG
#endif
                }

#if DEBUG
#endif
                // Mark progress to update UI
                IsFullyInitialized = true;

#if DEBUG
#endif
                // Stage 2: Perform remaining non-critical initialization
                // Use Task.WhenAny instead of Task.WhenAll to avoid waiting for slow tasks
                await Task.Run(async () =>
                {
                    try
                    {
#if DEBUG
#endif
                        await LoadNonCriticalDataAsync(ct);
#if DEBUG
#endif
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogWarning("Non-critical data loading canceled due to timeout");
#if DEBUG
#endif
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error during non-critical data loading");
#if DEBUG
#endif
                    }
                }, ct);

#if DEBUG
#endif
                // Record total initialization time
                totalStopwatch.Stop();
                _initializationTime = totalStopwatch.Elapsed;

                _logger.LogInformation("Full dashboard initialization completed in {DurationMs}ms",
                    _initializationTime.TotalMilliseconds);
#if DEBUG
#endif

#if DEBUG
#endif
                // Log detailed performance metrics with structured duration fields
                _logger.LogInformation(
                    "Performance Metrics - Total:{TotalMs}ms Routes:{RoutesMs}ms StudentList:{StudentListMs}ms",
                    _initializationTime.TotalMilliseconds,
                    _routePopulationTime.TotalMilliseconds,
                    _studentListLoadTime.TotalMilliseconds);

#if DEBUG
#endif
                // Also log using the PerformanceMonitor for consistent tracking
                _performanceMonitor.RecordMetric("DashboardViewModel.Initialize", _initializationTime);
                _performanceMonitor.RecordMetric("DashboardViewModel.RoutePopulation", _routePopulationTime);
                _performanceMonitor.RecordMetric("DashboardViewModel.StudentListLoad", _studentListLoadTime);
#if DEBUG
#endif
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Background initialization was canceled due to timeout after {ElapsedMs}ms",
                    totalStopwatch.ElapsedMilliseconds);
#if DEBUG
#endif
                // Still mark as initialized even if we had to cancel
                IsFullyInitialized = true;
#if DEBUG
#endif
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during background initialization: {ErrorMessage}", ex.Message);
#if DEBUG
#endif
#if DEBUG
#endif
                // We don't set error message here since critical data is already loaded
                IsFullyInitialized = true; // Mark as initialized even with errors
#if DEBUG
#endif
            }
        }

        // Helper method to wait for a task with a timeout
        private async Task<bool> WaitForTaskWithTimeout(Task task, int timeoutMs, string operationName)
        {
#if DEBUG
#endif
            if (await Task.WhenAny(task, Task.Delay(timeoutMs)) == task)
            {
#if DEBUG
#endif
                return true;
            }
            _logger.LogWarning("Operation '{Operation}' timed out after {Timeout}ms", operationName, timeoutMs);
#if DEBUG
#endif
            return false;
        }

        // Load minimal route data for initial display
        private async Task LoadMinimalRouteDataAsync(CancellationToken cancellationToken)
        {
#if DEBUG
#endif
            try
            {
#if DEBUG
#endif
                // OPTIMIZATION: Only load essential route data needed for dashboard display
                // This avoids loading full route details during startup
                await _routePopulationScaffold.PopulateRouteMetadataAsync();
#if DEBUG
#endif
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error loading minimal route data");
#if DEBUG
#endif
                // Don't rethrow - allow other initialization to continue
            }
#if DEBUG
#endif
        }

        // Try to load dashboard data from cache to avoid DB hits
        private async Task<bool> TryLoadFromCacheAsync()
        {
#if DEBUG
#endif
            try
            {
#if DEBUG
#endif
#if DEBUG
#endif
                // OPTIMIZATION: Use a timeout to avoid blocking on cache access
                var cacheTask = Task.Run(async () =>
                {
#if DEBUG
#endif
                    var result = await _cachingService.GetCachedDashboardMetricsAsync();
#if DEBUG
#endif
                    return result;
                });

#if DEBUG
#endif
                // Use a very short timeout since this is supposed to be fast
                if (await Task.WhenAny(cacheTask, Task.Delay(300)) != cacheTask)
                {
                    _logger.LogWarning("Cache access timed out after 300ms, falling back to database");
#if DEBUG
#endif
                    return false;
                }

#if DEBUG
#endif
                var metrics = await cacheTask;

                // Check if we got meaningful cache data
                if (metrics != null && metrics.Count > 0)
                {
                    _logger.LogDebug("Found cached metrics with {Count} values", metrics.Count);
#if DEBUG
#endif
                    bool hasValidData = false;

#if DEBUG
#endif
                    // Update metrics properties from cache
                    if (metrics.TryGetValue("BusCount", out int busCount))
                    {
                        TotalBuses = busCount;
                        hasValidData = true;
#if DEBUG
#endif
                    }

                    if (metrics.TryGetValue("DriverCount", out int driverCount))
                    {
                        TotalDrivers = driverCount;
                        hasValidData = true;
#if DEBUG
#endif
                    }

                    if (metrics.TryGetValue("RouteCount", out int routeCount))
                    {
                        TotalActiveRoutes = routeCount;
                        hasValidData = true;
#if DEBUG
#endif
                    }

                    if (hasValidData)
                    {
                        _logger.LogInformation("Successfully loaded dashboard metrics from cache: Buses={Buses}, Drivers={Drivers}, Routes={Routes}",
                            TotalBuses, TotalDrivers, TotalActiveRoutes);
#if DEBUG
#endif
                        Debug.WriteLine("[DEBUG] TryLoadFromCacheAsync END (cache hit)");
                        return true;
                    }
                    else
                    {
#if DEBUG
#endif
                    }
                }
                else
                {
#if DEBUG
#endif
                }

                _logger.LogDebug("No usable cache data found, falling back to database");
                Debug.WriteLine("[DEBUG] TryLoadFromCacheAsync END (no cache)");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load from cache, will load from database");
#if DEBUG
#endif
#if DEBUG
#endif
                return false;
            }
        }

        private async Task LoadDashboardDataAsync()
        {
            _logger.LogInformation("Starting LoadDashboardDataAsync");
#if DEBUG
#endif

            await LoadDataAsync(async () =>
            {
                try
                {
                    // Measure route population time
                    var routeStopwatch = Stopwatch.StartNew();
                    _logger.LogInformation("Populating routes via _routePopulationScaffold.PopulateRoutesAsync()");
#if DEBUG
#endif

                    // Use LogContext to track this specific operation
                    using (LogContext.PushProperty("Operation", "RoutePopulation"))
                    {
                        await _routePopulationScaffold.PopulateRoutesAsync();
                    }

                    routeStopwatch.Stop();
                    _routePopulationTime = routeStopwatch.Elapsed;
                    _logger.LogInformation("Routes populated successfully in {DurationMs}ms", _routePopulationTime.TotalMilliseconds);
#if DEBUG
#endif

                    // Load dashboard metrics
                    await LoadDashboardMetricsAsync();

                    // Initialize ViewModels
                    _logger.LogInformation("Initializing module ViewModels");
#if DEBUG
#endif

                    // Initialize each view model in parallel for better performance
                    var initializationTasks = new List<Task>();

                    // Initialize StudentListViewModel by accessing the property
                    _logger.LogInformation("Initializing StudentListViewModel");
#if DEBUG
#endif
                    var studentListStopwatch = Stopwatch.StartNew();
                    using (LogContext.PushProperty("Operation", "StudentListLoading"))
                    {
                        var studentViewModel = StudentListViewModel;
                        if (studentViewModel?.Initialized != null)
                        {
                            initializationTasks.Add(studentViewModel.Initialized);
                        }
                    }

                    // Initialize BusManagementViewModel
                    _logger.LogInformation("Initializing BusManagementViewModel");
#if DEBUG
#endif
                    using (LogContext.PushProperty("Operation", "BusManagementLoading"))
                    {
                        var busViewModel = BusManagementViewModel;
                        // Add any initialization task if available
                    }

                    // Initialize DriverManagementViewModel
                    _logger.LogInformation("Initializing DriverManagementViewModel");
#if DEBUG
#endif
                    using (LogContext.PushProperty("Operation", "DriverManagementLoading"))
                    {
                        var driverViewModel = DriverManagementViewModel;
                        // Add any initialization task if available
                    }

                    // Initialize RouteManagementViewModel
                    _logger.LogInformation("Initializing RouteManagementViewModel");
#if DEBUG
#endif
                    using (LogContext.PushProperty("Operation", "RouteManagementLoading"))
                    {
                        var routeViewModel = RouteManagementViewModel;
                        // Add any initialization task if available
                    }

                    // Initialize in-development module view models
                    _logger.LogInformation("Initializing in-development module ViewModels");

                    // Initialize ScheduleManagementViewModel
                    using (LogContext.PushProperty("Operation", "ScheduleManagementLoading"))
                    {
                        try
                        {
                            var scheduleViewModel = ScheduleManagementViewModel;
                            _logger.LogInformation("Successfully initialized ScheduleManagementViewModel");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Could not initialize ScheduleManagementViewModel: {Message}", ex.Message);
                        }
                    }

                    // Initialize StudentManagementViewModel
                    using (LogContext.PushProperty("Operation", "StudentManagementLoading"))
                    {
                        try
                        {
                            var studentMgmtViewModel = StudentManagementViewModel;
                            _logger.LogInformation("Successfully initialized StudentManagementViewModel");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Could not initialize StudentManagementViewModel: {Message}", ex.Message);
                        }
                    }

                    // Initialize MaintenanceTrackingViewModel
                    using (LogContext.PushProperty("Operation", "MaintenanceTrackingLoading"))
                    {
                        try
                        {
                            var maintenanceViewModel = MaintenanceTrackingViewModel;
                            _logger.LogInformation("Successfully initialized MaintenanceTrackingViewModel");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Could not initialize MaintenanceTrackingViewModel: {Message}", ex.Message);
                        }
                    }

                    // Initialize FuelManagementViewModel
                    using (LogContext.PushProperty("Operation", "FuelManagementLoading"))
                    {
                        try
                        {
                            var fuelViewModel = FuelManagementViewModel;
                            _logger.LogInformation("Successfully initialized FuelManagementViewModel");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Could not initialize FuelManagementViewModel: {Message}", ex.Message);
                        }
                    }

                    // Initialize ActivityLogViewModel
                    using (LogContext.PushProperty("Operation", "ActivityLogLoading"))
                    {
                        try
                        {
                            var activityViewModel = ActivityLogViewModel;
                            _logger.LogInformation("Successfully initialized ActivityLogViewModel");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Could not initialize ActivityLogViewModel: {Message}", ex.Message);
                        }
                    }

                    // REMOVED: TicketManagementViewModel initialization - deprecated module

                    // Wait for all initialization tasks to complete
                    await Task.WhenAll(initializationTasks);

                    studentListStopwatch.Stop();
                    _studentListLoadTime = studentListStopwatch.Elapsed;

                    _logger.LogInformation("All ViewModels initialized successfully");
#if DEBUG
#endif
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading dashboard data: {ErrorMessage}", ex.Message);
#if DEBUG
#endif
#if DEBUG
#endif
                    throw; // Re-throw to be caught by LoadDataAsync
                }
            });
#if DEBUG
#endif
        }

        /// <summary>
        /// Phase 1: Loads only the critical dashboard data for fast initial display
        /// </summary>
        private async Task LoadCriticalDataAsync(CancellationToken cancellationToken = default)
        {
            // Prevent concurrent calls to avoid DbContext threading issues
            if (_isRefreshing)
            {
                _logger.LogDebug("Skipping LoadCriticalDataAsync - already in progress");
                return;
            }

            try
            {
                _isRefreshing = true;
#if DEBUG
#endif
                _logger.LogInformation("Starting LoadCriticalDataAsync - fast path for dashboard");

#if DEBUG
#endif
#if DEBUG
#endif
                // Use the optimized metrics service to get all counts in one query
                var metricsStopwatch = Stopwatch.StartNew();

#if DEBUG
#endif
                // OPTIMIZATION: Skip caching mechanisms during startup, go directly to metrics service
                // Create cancellation token for timeout
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(1.5));
                using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                try
                {
                    Debug.WriteLine("[DEBUG] LoadCriticalDataAsync: Calling _dashboardMetricsService.GetDashboardMetricsAsync()");
                    var metrics = await _dashboardMetricsService.GetDashboardMetricsAsync();

                    // Only update values if not cancelled
                    if (!combinedCts.Token.IsCancellationRequested && metrics != null)
                    {
                        if (metrics.TryGetValue("BusCount", out int busCount))
                        {
                            TotalBuses = busCount;
                        }
                        if (metrics.TryGetValue("DriverCount", out int driverCount))
                        {
                            TotalDrivers = driverCount;
                        }
                        if (metrics.TryGetValue("RouteCount", out int routeCount))
                        {
                            TotalActiveRoutes = routeCount;
                        }
                    }
                }
                catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
                {
                    _logger.LogWarning("Dashboard metrics query timed out after 1.5 seconds, using default values");
                    // Use default values on timeout
                    TotalBuses = 0;
                    TotalDrivers = 0;
                    TotalActiveRoutes = 0;
                }

                // Stop the stopwatch for metrics
                metricsStopwatch.Stop();

                _logger.LogInformation("Critical dashboard metrics loaded in {ElapsedMs}ms - " +
                    "Buses: {TotalBuses}, Drivers: {TotalDrivers}, Routes: {TotalActiveRoutes}",
                    metricsStopwatch.ElapsedMilliseconds, TotalBuses, TotalDrivers, TotalActiveRoutes);
#if DEBUG
#endif

#if DEBUG
#endif
                // Add to cache immediately to speed up future startups
                await Task.Run(() =>
                {
                    try
                    {
#if DEBUG
#endif
                        var cacheMetrics = new Dictionary<string, int>
                        {
                            ["BusCount"] = TotalBuses,
                            ["DriverCount"] = TotalDrivers,
                            ["RouteCount"] = TotalActiveRoutes
                        };
#if DEBUG
#endif
                        _cachingService.SetDashboardMetricsDirectly(cacheMetrics);
                        _logger.LogDebug("Dashboard metrics cached during critical data loading");
#if DEBUG
#endif
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Non-critical failure caching metrics");
#if DEBUG
#endif
                    }
                }, cancellationToken);
#if DEBUG
#endif
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Critical dashboard data loading was canceled");
#if DEBUG
#endif
                throw; // Propagate cancellation
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading critical dashboard data: {ErrorMessage}", ex.Message);
#if DEBUG
#endif
#if DEBUG
#endif
                // Set default values for metrics
                TotalBuses = 0;
                TotalDrivers = 0;
                TotalActiveRoutes = 0;
#if DEBUG
#endif

                // We don't rethrow here to ensure the dashboard can still load
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        /// <summary>
        /// Phase 2: Loads all remaining dashboard data in the background
        /// </summary>
        private async Task LoadNonCriticalDataAsync(CancellationToken cancellationToken = default)
        {
#if DEBUG
#endif
            _logger.LogInformation("Starting LoadNonCriticalDataAsync - background initialization");

            try
            {
#if DEBUG
#endif
#if DEBUG
#endif
                // Note: Route metadata now loaded in LoadMinimalRouteDataAsync
                // to ensure core data loads faster

#if DEBUG
#endif
                // OPTIMIZATION: Add metrics to cache for future use
                if (TotalBuses > 0 || TotalDrivers > 0 || TotalActiveRoutes > 0)
                {
#if DEBUG
#endif
                    var metrics = new Dictionary<string, int>
                    {
                        ["BusCount"] = TotalBuses,
                        ["DriverCount"] = TotalDrivers,
                        ["RouteCount"] = TotalActiveRoutes
                    };

                    Debug.WriteLine("[DEBUG] LoadNonCriticalDataAsync: Starting cache task (non-blocking)");
                    // Cache the metrics we already have - but don't block on it
                    Task cacheTask = Task.Run(() =>
                    {
                        try
                        {
#if DEBUG
#endif
                            // Use direct caching to avoid hitting database again
                            _cachingService.SetDashboardMetricsDirectly(metrics);
                            _logger.LogInformation("Dashboard metrics cached for future use");
#if DEBUG
#endif
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to cache dashboard metrics");
#if DEBUG
#endif
                        }
                    }, cancellationToken);

#if DEBUG
#endif
                    // Don't await - let it run in background
                }
                else
                {
                    Debug.WriteLine("[DEBUG] LoadNonCriticalDataAsync: No metrics to cache (all values are 0)");
                }

#if DEBUG
#endif
                cancellationToken.ThrowIfCancellationRequested();

#if DEBUG
#endif
                // Initialize StudentListViewModel lazily with a stricter timeout
                var studentListStopwatch = Stopwatch.StartNew();

#if DEBUG
#endif
                // OPTIMIZATION: Use TaskCompletionSource with timeout to limit waiting time
                var taskCompletionSource = new TaskCompletionSource<bool>();
                var timeoutTask = Task.Delay(2000, cancellationToken); // Reduced to 2 second timeout
#if DEBUG
#endif

                var studentInitTask = Task.Run(async () =>
                {
                    try
                    {
#if DEBUG
#endif
                        // Use a separate cancellation token with shorter timeout
                        using var localCts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                            localCts.Token, cancellationToken);

#if DEBUG
#endif
                        // Try to get StudentListViewModel but with timeout protection
                        var getViewModelTask = Task.Run(() =>
                        {
#if DEBUG
#endif
                            return StudentListViewModel;
                        });

#if DEBUG
#endif
                        // Apply a short timeout to just getting the view model
                        if (await Task.WhenAny(getViewModelTask, Task.Delay(1000, linkedCts.Token)) != getViewModelTask)
                        {
                            _logger.LogWarning("Getting StudentListViewModel timed out");
#if DEBUG
#endif
                            taskCompletionSource.TrySetResult(false);
                            return;
                        }

#if DEBUG
#endif
                        var studentViewModel = await getViewModelTask;
#if DEBUG
#endif

                        if (studentViewModel?.Initialized != null)
                        {
#if DEBUG
#endif
                            // Only wait up to 1 second for initialization
                            if (await Task.WhenAny(studentViewModel.Initialized, Task.Delay(1000, linkedCts.Token)) != studentViewModel.Initialized)
                            {
                                _logger.LogWarning("StudentListViewModel.Initialized task timed out");
#if DEBUG
#endif
                            }
                            else
                            {
#if DEBUG
#endif
                            }
                        }
                        else
                        {
#if DEBUG
#endif
                        }

                        taskCompletionSource.TrySetResult(true);
#if DEBUG
#endif
                    }
                    catch (OperationCanceledException)
                    {
#if DEBUG
#endif
                        taskCompletionSource.TrySetCanceled();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to initialize StudentListViewModel, will continue with other components");
#if DEBUG
#endif
                        taskCompletionSource.TrySetResult(false);
                    }
                });

#if DEBUG
#endif
                // Wait for either completion or timeout
                if (await Task.WhenAny(taskCompletionSource.Task, timeoutTask) == timeoutTask)
                {
                    _logger.LogWarning("StudentListViewModel initialization timed out after 2 seconds, continuing startup");
#if DEBUG
#endif
                }
                else
                {
#if DEBUG
#endif
                }

                studentListStopwatch.Stop();
                _studentListLoadTime = studentListStopwatch.Elapsed;
                _logger.LogInformation("StudentListViewModel initialization process took {ElapsedMs}ms",
                    _studentListLoadTime.TotalMilliseconds);
#if DEBUG
#endif

                _logger.LogInformation("Background initialization completed, remaining ViewModels will be initialized on-demand");
#if DEBUG
#endif

#if DEBUG
#endif
                // DO NOT initialize other view models eagerly - they will be initialized on-demand
                // when their tab is selected, which significantly improves startup time
#if DEBUG
#endif
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Non-critical data initialization was canceled");
#if DEBUG
#endif
                throw; // Propagate cancellation
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in background data initialization: {ErrorMessage}", ex.Message);
#if DEBUG
#endif
#if DEBUG
#endif
                // We don't rethrow here since this is background initialization
            }
        }

        private async Task LoadDashboardMetricsAsync()
        {
#if DEBUG
#endif
            try
            {
#if DEBUG
#endif
                _logger.LogInformation("Loading dashboard metrics");

                // Start stopwatch to measure metric loading time
                var metricsStopwatch = Stopwatch.StartNew();

                // Load metrics in parallel for better performance
                var busTask = _busService.GetAllBusesAsync();
                var driverTask = _driverService.GetAllDriversAsync();
                var routeTask = _routeService.GetAllActiveRoutesAsync();

                // Wait for all tasks to complete
                await Task.WhenAll(busTask, driverTask, routeTask);

                // Update properties with the results
                TotalBuses = busTask.Result.ToList().Count;
#if DEBUG
#endif
                TotalDrivers = driverTask.Result.ToList().Count;
#if DEBUG
#endif
                TotalActiveRoutes = (await routeTask).ToList().Count;
#if DEBUG
#endif

                metricsStopwatch.Stop();

                _logger.LogInformation("Dashboard metrics loaded successfully in {DurationMs}ms",
                    metricsStopwatch.Elapsed.TotalMilliseconds);
                _logger.LogInformation("Metrics - Buses: {TotalBuses}, Drivers: {TotalDrivers}, Active Routes: {TotalActiveRoutes}",
                    TotalBuses, TotalDrivers, TotalActiveRoutes);
#if DEBUG
#endif
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard metrics: {ErrorMessage}", ex.Message);
                // Set default values in case of error
                TotalBuses = 0;
                TotalDrivers = 0;
                TotalActiveRoutes = 0;
#if DEBUG
#endif
            }
        }
    }
}