using System.Threading.Tasks;
using BusBuddy.WPF.Services;
using BusBuddy.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using BusBuddy.WPF.Utilities;
using Serilog.Context;
using BusBuddy.Core.Services;

namespace BusBuddy.WPF.ViewModels
{
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
        private TicketManagementViewModel? _ticketManagementViewModel;

        // Performance tracking metrics
        private TimeSpan _initializationTime;
        private TimeSpan _routePopulationTime;
        private TimeSpan _studentListLoadTime;

        // Dashboard metrics
        private int _totalBuses;
        private int _totalDrivers;
        private int _totalActiveRoutes;

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
                try
                {
                    if (_busManagementViewModel == null)
                    {
                        _logger.LogInformation("Resolving BusManagementViewModel from service provider");
                        _busManagementViewModel = _serviceProvider.GetRequiredService<BusManagementViewModel>();
                        _logger.LogInformation("Successfully resolved BusManagementViewModel");
                    }
                    return _busManagementViewModel;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to resolve BusManagementViewModel: {0}", ex.Message);
                    throw;
                }
            }
        }

        public DriverManagementViewModel DriverManagementViewModel
        {
            get
            {
                try
                {
                    if (_driverManagementViewModel == null)
                    {
                        _logger.LogInformation("Resolving DriverManagementViewModel from service provider");
                        _driverManagementViewModel = _serviceProvider.GetRequiredService<DriverManagementViewModel>();
                        _logger.LogInformation("Successfully resolved DriverManagementViewModel");
                    }
                    return _driverManagementViewModel;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to resolve DriverManagementViewModel: {0}", ex.Message);
                    throw;
                }
            }
        }

        public RouteManagementViewModel RouteManagementViewModel
        {
            get
            {
                try
                {
                    if (_routeManagementViewModel == null)
                    {
                        _logger.LogInformation("Resolving RouteManagementViewModel from service provider");
                        _routeManagementViewModel = _serviceProvider.GetRequiredService<RouteManagementViewModel>();
                        _logger.LogInformation("Successfully resolved RouteManagementViewModel");
                    }
                    return _routeManagementViewModel;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to resolve RouteManagementViewModel: {0}", ex.Message);
                    throw;
                }
            }
        }

        // Properties for in-development modules
        public ScheduleManagementViewModel ScheduleManagementViewModel
        {
            get
            {
                try
                {
                    if (_scheduleManagementViewModel == null)
                    {
                        _logger.LogInformation("Resolving ScheduleManagementViewModel from service provider");
                        _scheduleManagementViewModel = _serviceProvider.GetRequiredService<ScheduleManagementViewModel>();
                        _logger.LogInformation("Successfully resolved ScheduleManagementViewModel");
                    }
                    return _scheduleManagementViewModel;
                }
                catch (Exception ex)
                {
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
                        _fuelManagementViewModel = _serviceProvider.GetRequiredService<FuelManagementViewModel>();
                        _logger.LogInformation("Successfully resolved FuelManagementViewModel");
                    }
                    return _fuelManagementViewModel;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to resolve FuelManagementViewModel: {0}", ex.Message);
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

        public TicketManagementViewModel TicketManagementViewModel
        {
            get
            {
                try
                {
                    if (_ticketManagementViewModel == null)
                    {
                        _logger.LogInformation("Resolving TicketManagementViewModel from service provider");
                        _ticketManagementViewModel = _serviceProvider.GetRequiredService<TicketManagementViewModel>();
                        _logger.LogInformation("Successfully resolved TicketManagementViewModel");
                    }
                    return _ticketManagementViewModel;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to resolve TicketManagementViewModel: {0}", ex.Message);
                    throw;
                }
            }
        }

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
        }

        public string InitializationTimeFormatted => FormatUtils.FormatDuration((int)_initializationTime.TotalMilliseconds / 1000);
        public string RoutePopulationTimeFormatted => FormatUtils.FormatDuration((int)_routePopulationTime.TotalMilliseconds / 1000);
        public string StudentListLoadTimeFormatted => FormatUtils.FormatDuration((int)_studentListLoadTime.TotalMilliseconds / 1000);

        // Dashboard metrics properties
        public int TotalBuses
        {
            get => _totalBuses;
            private set
            {
                if (_totalBuses != value)
                {
                    _totalBuses = value;
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
                    OnPropertyChanged();
                }
            }
        }

        public async Task InitializeAsync()
        {
            var totalStopwatch = Stopwatch.StartNew();

            _logger.LogInformation("DashboardViewModel.InitializeAsync called");
            Debug.WriteLine("DashboardViewModel.InitializeAsync called");

            try
            {
                // Use LogContext to add structured properties to all log entries within this scope
                using (LogContext.PushProperty("StartupStep", "DashboardInitialization"))
                {
                    // OPTIMIZATION: Check for cached metrics first before any database operations
                    if (await TryLoadFromCacheAsync())
                    {
                        _logger.LogInformation("Loaded dashboard data from cache in {DurationMs}ms",
                            totalStopwatch.ElapsedMilliseconds);

                        // Mark critical data as loaded immediately from cache
                        IsCriticalDataLoaded = true;

                        // Start loading full data in background but don't await it
                        _ = LoadRemainingDataInBackgroundAsync(totalStopwatch);

                        return;
                    }

                    // Phase 1: Only load critical overview data first (minimal DB queries)
                    await LoadCriticalDataAsync();

                    // Mark critical data as loaded
                    IsCriticalDataLoaded = true;

                    // Record time for critical path
                    var criticalPathTime = totalStopwatch.Elapsed;
                    _logger.LogInformation("Critical dashboard data loaded in {DurationMs}ms",
                        criticalPathTime.TotalMilliseconds);

                    // Phase 2: Load remaining data in low-priority background thread
                    _ = LoadRemainingDataInBackgroundAsync(totalStopwatch);
                }
            }
            catch (Exception ex)
            {
                totalStopwatch.Stop();
                ErrorMessage = $"Initialize failed: {ex.Message}";
                _logger.LogError(ex, "DashboardViewModel.InitializeAsync failed with exception after {ElapsedMs}ms: {ErrorMessage}",
                    totalStopwatch.ElapsedMilliseconds, ex.Message);
                Debug.WriteLine($"InitializeAsync error after {totalStopwatch.ElapsedMilliseconds}ms: {ex}");
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "Inner exception: {ErrorMessage}", ex.InnerException.Message);
                    Debug.WriteLine($"Inner exception: {ex.InnerException}");
                }
            }
        }

        // Extracted background loading into a separate method for clarity
        private async Task LoadRemainingDataInBackgroundAsync(Stopwatch totalStopwatch)
        {
            try
            {
                // Lower thread priority to avoid impacting UI responsiveness
                Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

                // OPTIMIZATION: Delay non-critical loading to give UI time to render and respond
                await Task.Delay(300);

                await LoadNonCriticalDataAsync();

                // Record total initialization time
                totalStopwatch.Stop();
                _initializationTime = totalStopwatch.Elapsed;

                _logger.LogInformation("Full dashboard initialization completed in {DurationMs}ms",
                    _initializationTime.TotalMilliseconds);

                // Log detailed performance metrics with structured duration fields
                _logger.LogInformation(
                    "Performance Metrics - Total:{TotalMs}ms Routes:{RoutesMs}ms StudentList:{StudentListMs}ms",
                    _initializationTime.TotalMilliseconds,
                    _routePopulationTime.TotalMilliseconds,
                    _studentListLoadTime.TotalMilliseconds);

                // Also log using the PerformanceMonitor for consistent tracking
                _performanceMonitor.RecordMetric("DashboardViewModel.Initialize", _initializationTime);
                _performanceMonitor.RecordMetric("DashboardViewModel.RoutePopulation", _routePopulationTime);
                _performanceMonitor.RecordMetric("DashboardViewModel.StudentListLoad", _studentListLoadTime);

                // Mark as fully initialized
                IsFullyInitialized = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during background initialization: {ErrorMessage}", ex.Message);
                // We don't set error message here since critical data is already loaded
            }
        }

        // Try to load dashboard data from cache to avoid DB hits
        private async Task<bool> TryLoadFromCacheAsync()
        {
            try
            {
                // Check if metrics are cached
                var metrics = await _cachingService.GetCachedDashboardMetricsAsync();
                if (metrics != null && metrics.Count > 0)
                {
                    // Update metrics properties from cache
                    if (metrics.TryGetValue("BusCount", out int busCount))
                        TotalBuses = busCount;

                    if (metrics.TryGetValue("DriverCount", out int driverCount))
                        TotalDrivers = driverCount;

                    if (metrics.TryGetValue("RouteCount", out int routeCount))
                        TotalActiveRoutes = routeCount;

                    _logger.LogInformation("Successfully loaded dashboard metrics from cache");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load from cache, will load from database");
                return false;
            }
        }

        private async Task LoadDashboardDataAsync()
        {
            _logger.LogInformation("Starting LoadDashboardDataAsync");
            Debug.WriteLine("Starting LoadDashboardDataAsync");

            await LoadDataAsync(async () =>
            {
                try
                {
                    // Measure route population time
                    var routeStopwatch = Stopwatch.StartNew();
                    _logger.LogInformation("Populating routes via _routePopulationScaffold.PopulateRoutesAsync()");
                    Debug.WriteLine("Starting route population");

                    // Use LogContext to track this specific operation
                    using (LogContext.PushProperty("Operation", "RoutePopulation"))
                    {
                        await _routePopulationScaffold.PopulateRoutesAsync();
                    }

                    routeStopwatch.Stop();
                    _routePopulationTime = routeStopwatch.Elapsed;
                    _logger.LogInformation("Routes populated successfully in {DurationMs}ms", _routePopulationTime.TotalMilliseconds);
                    Debug.WriteLine($"Route population completed successfully in {_routePopulationTime.TotalMilliseconds}ms");

                    // Load dashboard metrics
                    await LoadDashboardMetricsAsync();

                    // Initialize ViewModels
                    _logger.LogInformation("Initializing module ViewModels");
                    Debug.WriteLine("Initializing module ViewModels");

                    // Initialize each view model in parallel for better performance
                    var initializationTasks = new List<Task>();

                    // Initialize StudentListViewModel by accessing the property
                    _logger.LogInformation("Initializing StudentListViewModel");
                    Debug.WriteLine("Attempting to initialize StudentListViewModel");
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
                    Debug.WriteLine("Attempting to initialize BusManagementViewModel");
                    using (LogContext.PushProperty("Operation", "BusManagementLoading"))
                    {
                        var busViewModel = BusManagementViewModel;
                        // Add any initialization task if available
                    }

                    // Initialize DriverManagementViewModel
                    _logger.LogInformation("Initializing DriverManagementViewModel");
                    Debug.WriteLine("Attempting to initialize DriverManagementViewModel");
                    using (LogContext.PushProperty("Operation", "DriverManagementLoading"))
                    {
                        var driverViewModel = DriverManagementViewModel;
                        // Add any initialization task if available
                    }

                    // Initialize RouteManagementViewModel
                    _logger.LogInformation("Initializing RouteManagementViewModel");
                    Debug.WriteLine("Attempting to initialize RouteManagementViewModel");
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

                    // Initialize TicketManagementViewModel
                    using (LogContext.PushProperty("Operation", "TicketManagementLoading"))
                    {
                        try
                        {
                            var ticketViewModel = TicketManagementViewModel;
                            _logger.LogInformation("Successfully initialized TicketManagementViewModel");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Could not initialize TicketManagementViewModel: {Message}", ex.Message);
                        }
                    }

                    // Wait for all initialization tasks to complete
                    await Task.WhenAll(initializationTasks);

                    studentListStopwatch.Stop();
                    _studentListLoadTime = studentListStopwatch.Elapsed;

                    _logger.LogInformation("All ViewModels initialized successfully");
                    Debug.WriteLine("All ViewModels initialized successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading dashboard data: {ErrorMessage}", ex.Message);
                    Debug.WriteLine($"LoadDashboardDataAsync ERROR: {ex.Message}");
                    Debug.WriteLine($"STACK TRACE: {ex.StackTrace}");
                    throw; // Re-throw to be caught by LoadDataAsync
                }
            });
        }

        /// <summary>
        /// Phase 1: Loads only the critical dashboard data for fast initial display
        /// </summary>
        private async Task LoadCriticalDataAsync()
        {
            _logger.LogInformation("Starting LoadCriticalDataAsync - fast path for dashboard");

            try
            {
                // Use the optimized metrics service to get all counts in one query
                var metricsStopwatch = Stopwatch.StartNew();

                // OPTIMIZATION: Skip caching mechanisms during startup, go directly to metrics service
                var metrics = await _dashboardMetricsService.GetDashboardMetricsAsync();

                // Store metrics for future cache use - we'll let the background process handle this
                metricsStopwatch.Stop();

                // Update metrics properties
                if (metrics.TryGetValue("BusCount", out int busCount))
                    TotalBuses = busCount;

                if (metrics.TryGetValue("DriverCount", out int driverCount))
                    TotalDrivers = driverCount;

                if (metrics.TryGetValue("RouteCount", out int routeCount))
                    TotalActiveRoutes = routeCount;

                _logger.LogInformation("Critical dashboard metrics loaded in {ElapsedMs}ms - " +
                    "Buses: {TotalBuses}, Drivers: {TotalDrivers}, Routes: {TotalActiveRoutes}",
                    metricsStopwatch.ElapsedMilliseconds, TotalBuses, TotalDrivers, TotalActiveRoutes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading critical dashboard data: {ErrorMessage}", ex.Message);
                // Set default values for metrics
                TotalBuses = 0;
                TotalDrivers = 0;
                TotalActiveRoutes = 0;

                // We don't rethrow here to ensure the dashboard can still load
            }
        }

        /// <summary>
        /// Phase 2: Loads all remaining dashboard data in the background
        /// </summary>
        private async Task LoadNonCriticalDataAsync()
        {
            _logger.LogInformation("Starting LoadNonCriticalDataAsync - background initialization");

            try
            {
                // OPTIMIZATION: Prioritize route population as it's needed for map display
                var routeStopwatch = Stopwatch.StartNew();

                // OPTIMIZATION: Only fetch route metadata instead of full route data
                using (LogContext.PushProperty("Operation", "BackgroundRouteMetadataPopulation"))
                {
                    // Call a more targeted method that only loads essential route information
                    await _routePopulationScaffold.PopulateRouteMetadataAsync();
                }

                routeStopwatch.Stop();
                _routePopulationTime = routeStopwatch.Elapsed;
                _logger.LogInformation("Background route metadata population completed in {ElapsedMs}ms",
                    _routePopulationTime.TotalMilliseconds);

                // OPTIMIZATION: Add metrics to cache for future use
                if (TotalBuses > 0 || TotalDrivers > 0 || TotalActiveRoutes > 0)
                {
                    var metrics = new Dictionary<string, int>
                    {
                        ["BusCount"] = TotalBuses,
                        ["DriverCount"] = TotalDrivers,
                        ["RouteCount"] = TotalActiveRoutes
                    };

                    // Cache the metrics we already have
                    await Task.Run(() =>
                    {
                        try
                        {
                            // Use direct caching to avoid hitting database again
                            _cachingService.SetDashboardMetricsDirectly(metrics);
                            _logger.LogInformation("Dashboard metrics cached for future use");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to cache dashboard metrics");
                        }
                    });
                }

                // Initialize StudentListViewModel lazily with a timeout
                var studentListStopwatch = Stopwatch.StartNew();

                // OPTIMIZATION: Use TaskCompletionSource with timeout to limit waiting time
                var taskCompletionSource = new TaskCompletionSource<bool>();
                var timeoutTask = Task.Delay(3000); // 3 second timeout

                var studentInitTask = Task.Run(async () =>
                {
                    try
                    {
                        var studentViewModel = StudentListViewModel;
                        if (studentViewModel?.Initialized != null)
                        {
                            await studentViewModel.Initialized;
                        }
                        taskCompletionSource.TrySetResult(true);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to initialize StudentListViewModel, will continue with other components");
                        taskCompletionSource.TrySetResult(false);
                    }
                });

                // Wait for either completion or timeout
                if (await Task.WhenAny(taskCompletionSource.Task, timeoutTask) == timeoutTask)
                {
                    _logger.LogWarning("StudentListViewModel initialization timed out after 3 seconds, continuing startup");
                }

                studentListStopwatch.Stop();
                _studentListLoadTime = studentListStopwatch.Elapsed;
                _logger.LogInformation("StudentListViewModel initialization process took {ElapsedMs}ms",
                    _studentListLoadTime.TotalMilliseconds);

                _logger.LogInformation("Background initialization completed, remaining ViewModels will be initialized on-demand");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in background data initialization: {ErrorMessage}", ex.Message);
                // We don't rethrow here since this is background initialization
            }
        }

        private async Task LoadDashboardMetricsAsync()
        {
            try
            {
                _logger.LogInformation("Loading dashboard metrics");

                // Start stopwatch to measure metric loading time
                var metricsStopwatch = Stopwatch.StartNew();

                // Load metrics in parallel for better performance
                var busTask = _busService.GetAllBusEntitiesAsync();
                var driverTask = _driverService.GetAllDriversAsync();
                var routeTask = _routeService.GetAllActiveRoutesAsync();

                // Wait for all tasks to complete
                await Task.WhenAll(busTask, driverTask, routeTask);

                // Update properties with the results
                TotalBuses = busTask.Result.Count;
                TotalDrivers = driverTask.Result.Count;
                TotalActiveRoutes = (await routeTask).Count();

                metricsStopwatch.Stop();

                _logger.LogInformation("Dashboard metrics loaded successfully in {DurationMs}ms",
                    metricsStopwatch.Elapsed.TotalMilliseconds);
                _logger.LogInformation("Metrics - Buses: {TotalBuses}, Drivers: {TotalDrivers}, Active Routes: {TotalActiveRoutes}",
                    TotalBuses, TotalDrivers, TotalActiveRoutes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard metrics: {ErrorMessage}", ex.Message);
                // Set default values in case of error
                TotalBuses = 0;
                TotalDrivers = 0;
                TotalActiveRoutes = 0;
            }
        }
    }
}
