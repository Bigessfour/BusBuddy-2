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
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)); // Add 10-second timeout
            var ct = cts.Token;

            _logger.LogInformation("DashboardViewModel.InitializeAsync called");
            Debug.WriteLine("DashboardViewModel.InitializeAsync called");

            try
            {
                // Use LogContext to add structured properties to all log entries within this scope
                using (LogContext.PushProperty("StartupStep", "DashboardInitialization"))
                {
                    // CRITICAL OPTIMIZATION: Always show UI first, then load data
                    // This ensures the dashboard is visible even if data takes time to load
                    IsCriticalDataLoaded = true;

                    // First try quick cache load - avoid any DB hits if possible
                    if (await TryLoadFromCacheAsync())
                    {
                        _logger.LogInformation("Loaded dashboard data from cache in {DurationMs}ms",
                            totalStopwatch.ElapsedMilliseconds);

                        // Still load remaining data, but don't block UI on it
                        _ = LoadRemainingDataInBackgroundAsync(totalStopwatch);
                        return;
                    }

                    // Use a Task.Delay to ensure UI responsiveness even if data loading is in progress
                    await Task.Delay(50, ct);

                    // Phase 1: Quick load of critical metrics with timeout protection
                    Task criticalDataTask = Task.Run(async () =>
                    {
                        await LoadCriticalDataAsync(ct);
                    }, ct);

                    // Use a timeout to ensure we don't block the UI for too long
                    if (await Task.WhenAny(criticalDataTask, Task.Delay(2000, ct)) != criticalDataTask)
                    {
                        _logger.LogWarning("Critical data load taking too long, continuing with UI display");
                        // Continue anyway - we'll show default values
                    }

                    // Record time for critical path
                    var criticalPathTime = totalStopwatch.Elapsed;
                    _logger.LogInformation("Critical dashboard data loaded in {DurationMs}ms",
                        criticalPathTime.TotalMilliseconds);

                    // Phase 2: Load remaining data in low-priority background thread
                    // Using ConfigureAwait(false) to avoid blocking the UI thread
                    _ = LoadRemainingDataInBackgroundAsync(totalStopwatch).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Dashboard initialization was canceled due to timeout after {ElapsedMs}ms",
                    totalStopwatch.ElapsedMilliseconds);

                // Still mark as loaded so UI is visible
                IsCriticalDataLoaded = true;
            }
            catch (Exception ex)
            {
                totalStopwatch.Stop();
                ErrorMessage = $"Initialize failed: {ex.Message}";
                _logger.LogError(ex, "DashboardViewModel.InitializeAsync failed with exception after {ElapsedMs}ms: {ErrorMessage}",
                    totalStopwatch.ElapsedMilliseconds, ex.Message);

                // Still mark as loaded so UI is visible with error state
                IsCriticalDataLoaded = true;

                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "Inner exception: {ErrorMessage}", ex.InnerException.Message);
                }
            }
        }

        // Extracted background loading into a separate method for clarity
        private async Task LoadRemainingDataInBackgroundAsync(Stopwatch totalStopwatch)
        {
            // Use a background CancellationTokenSource with a much longer timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var ct = cts.Token;

            try
            {
                // Lower thread priority to avoid impacting UI responsiveness
                Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

                // OPTIMIZATION: Longer delay for non-critical loading to ensure UI is fully rendered
                await Task.Delay(500, ct);

                // OPTIMIZATION: Load minimal data for display first, then incrementally load more
                // Create multiple stages with increasing timeouts to prioritize important data

                // Stage 1: Minimal route data - 5 second timeout (highest priority)
                var routeMetadataTask = Task.Run(async () =>
                {
                    try
                    {
                        var routeStopwatch = Stopwatch.StartNew();
                        await LoadMinimalRouteDataAsync(ct);
                        routeStopwatch.Stop();
                        _routePopulationTime = routeStopwatch.Elapsed;
                        _logger.LogInformation("Minimal route data loaded in {ElapsedMs}ms",
                            _routePopulationTime.TotalMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error loading minimal route data");
                    }
                }, ct);

                // Wait for route metadata with a reasonable timeout
                if (!await WaitForTaskWithTimeout(routeMetadataTask, 5000, "route metadata loading"))
                {
                    _logger.LogWarning("Route metadata loading timed out, continuing with other initializations");
                }

                // Mark progress to update UI
                IsFullyInitialized = true;

                // Stage 2: Perform remaining non-critical initialization
                // Use Task.WhenAny instead of Task.WhenAll to avoid waiting for slow tasks
                await Task.Run(async () =>
                {
                    try
                    {
                        await LoadNonCriticalDataAsync(ct);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogWarning("Non-critical data loading canceled due to timeout");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error during non-critical data loading");
                    }
                }, ct);

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
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Background initialization was canceled due to timeout after {ElapsedMs}ms",
                    totalStopwatch.ElapsedMilliseconds);
                // Still mark as initialized even if we had to cancel
                IsFullyInitialized = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during background initialization: {ErrorMessage}", ex.Message);
                // We don't set error message here since critical data is already loaded
                IsFullyInitialized = true; // Mark as initialized even with errors
            }
        }

        // Helper method to wait for a task with a timeout
        private async Task<bool> WaitForTaskWithTimeout(Task task, int timeoutMs, string operationName)
        {
            if (await Task.WhenAny(task, Task.Delay(timeoutMs)) == task)
            {
                return true;
            }
            _logger.LogWarning("Operation '{Operation}' timed out after {Timeout}ms", operationName, timeoutMs);
            return false;
        }

        // Load minimal route data for initial display
        private async Task LoadMinimalRouteDataAsync(CancellationToken cancellationToken)
        {
            try
            {
                // OPTIMIZATION: Only load essential route data needed for dashboard display
                // This avoids loading full route details during startup
                await _routePopulationScaffold.PopulateRouteMetadataAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error loading minimal route data");
                // Don't rethrow - allow other initialization to continue
            }
        }

        // Try to load dashboard data from cache to avoid DB hits
        private async Task<bool> TryLoadFromCacheAsync()
        {
            try
            {
                // OPTIMIZATION: Use a timeout to avoid blocking on cache access
                var cacheTask = Task.Run(async () =>
                {
                    return await _cachingService.GetCachedDashboardMetricsAsync();
                });

                // Use a very short timeout since this is supposed to be fast
                if (await Task.WhenAny(cacheTask, Task.Delay(300)) != cacheTask)
                {
                    _logger.LogWarning("Cache access timed out after 300ms, falling back to database");
                    return false;
                }

                var metrics = await cacheTask;

                // Check if we got meaningful cache data
                if (metrics != null && metrics.Count > 0)
                {
                    _logger.LogDebug("Found cached metrics with {Count} values", metrics.Count);
                    bool hasValidData = false;

                    // Update metrics properties from cache
                    if (metrics.TryGetValue("BusCount", out int busCount))
                    {
                        TotalBuses = busCount;
                        hasValidData = true;
                    }

                    if (metrics.TryGetValue("DriverCount", out int driverCount))
                    {
                        TotalDrivers = driverCount;
                        hasValidData = true;
                    }

                    if (metrics.TryGetValue("RouteCount", out int routeCount))
                    {
                        TotalActiveRoutes = routeCount;
                        hasValidData = true;
                    }

                    if (hasValidData)
                    {
                        _logger.LogInformation("Successfully loaded dashboard metrics from cache: Buses={Buses}, Drivers={Drivers}, Routes={Routes}",
                            TotalBuses, TotalDrivers, TotalActiveRoutes);
                        return true;
                    }
                }

                _logger.LogDebug("No usable cache data found, falling back to database");
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
        private async Task LoadCriticalDataAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting LoadCriticalDataAsync - fast path for dashboard");

            try
            {
                // Use the optimized metrics service to get all counts in one query
                var metricsStopwatch = Stopwatch.StartNew();

                // OPTIMIZATION: Skip caching mechanisms during startup, go directly to metrics service
                // Wrap in Task.Run with timeout to prevent blocking
                var metricsTask = Task.Run(async () =>
                {
                    return await _dashboardMetricsService.GetDashboardMetricsAsync();
                });

                // Add timeout protection within the method as well
                if (await Task.WhenAny(metricsTask, Task.Delay(1500, cancellationToken)) != metricsTask)
                {
                    _logger.LogWarning("Dashboard metrics query taking too long, using default values");
                    // Use default values rather than waiting
                    TotalBuses = 0;
                    TotalDrivers = 0;
                    TotalActiveRoutes = 0;
                    return;
                }

                var metrics = await metricsTask;
                cancellationToken.ThrowIfCancellationRequested();

                // Store metrics for future cache use - we'll let the background process handle this
                metricsStopwatch.Stop();

                // Update metrics properties - check for cancellation between operations
                if (!cancellationToken.IsCancellationRequested && metrics.TryGetValue("BusCount", out int busCount))
                    TotalBuses = busCount;

                if (!cancellationToken.IsCancellationRequested && metrics.TryGetValue("DriverCount", out int driverCount))
                    TotalDrivers = driverCount;

                if (!cancellationToken.IsCancellationRequested && metrics.TryGetValue("RouteCount", out int routeCount))
                    TotalActiveRoutes = routeCount;

                _logger.LogInformation("Critical dashboard metrics loaded in {ElapsedMs}ms - " +
                    "Buses: {TotalBuses}, Drivers: {TotalDrivers}, Routes: {TotalActiveRoutes}",
                    metricsStopwatch.ElapsedMilliseconds, TotalBuses, TotalDrivers, TotalActiveRoutes);

                // Add to cache immediately to speed up future startups
                await Task.Run(() =>
                {
                    try
                    {
                        var cacheMetrics = new Dictionary<string, int>
                        {
                            ["BusCount"] = TotalBuses,
                            ["DriverCount"] = TotalDrivers,
                            ["RouteCount"] = TotalActiveRoutes
                        };
                        _cachingService.SetDashboardMetricsDirectly(cacheMetrics);
                        _logger.LogDebug("Dashboard metrics cached during critical data loading");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Non-critical failure caching metrics");
                    }
                }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Critical dashboard data loading was canceled");
                throw; // Propagate cancellation
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
        private async Task LoadNonCriticalDataAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting LoadNonCriticalDataAsync - background initialization");

            try
            {
                // Note: Route metadata now loaded in LoadMinimalRouteDataAsync
                // to ensure core data loads faster

                // OPTIMIZATION: Add metrics to cache for future use
                if (TotalBuses > 0 || TotalDrivers > 0 || TotalActiveRoutes > 0)
                {
                    var metrics = new Dictionary<string, int>
                    {
                        ["BusCount"] = TotalBuses,
                        ["DriverCount"] = TotalDrivers,
                        ["RouteCount"] = TotalActiveRoutes
                    };

                    // Cache the metrics we already have - but don't block on it
                    Task cacheTask = Task.Run(() =>
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
                    }, cancellationToken);

                    // Don't await - let it run in background
                }

                cancellationToken.ThrowIfCancellationRequested();

                // Initialize StudentListViewModel lazily with a stricter timeout
                var studentListStopwatch = Stopwatch.StartNew();

                // OPTIMIZATION: Use TaskCompletionSource with timeout to limit waiting time
                var taskCompletionSource = new TaskCompletionSource<bool>();
                var timeoutTask = Task.Delay(2000, cancellationToken); // Reduced to 2 second timeout

                var studentInitTask = Task.Run(async () =>
                {
                    try
                    {
                        // Use a separate cancellation token with shorter timeout
                        using var localCts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                            localCts.Token, cancellationToken);

                        // Try to get StudentListViewModel but with timeout protection
                        var getViewModelTask = Task.Run(() =>
                        {
                            return StudentListViewModel;
                        });

                        // Apply a short timeout to just getting the view model
                        if (await Task.WhenAny(getViewModelTask, Task.Delay(1000, linkedCts.Token)) != getViewModelTask)
                        {
                            _logger.LogWarning("Getting StudentListViewModel timed out");
                            taskCompletionSource.TrySetResult(false);
                            return;
                        }

                        var studentViewModel = await getViewModelTask;
                        if (studentViewModel?.Initialized != null)
                        {
                            // Only wait up to 1 second for initialization
                            if (await Task.WhenAny(studentViewModel.Initialized, Task.Delay(1000, linkedCts.Token)) != studentViewModel.Initialized)
                            {
                                _logger.LogWarning("StudentListViewModel.Initialized task timed out");
                            }
                        }
                        taskCompletionSource.TrySetResult(true);
                    }
                    catch (OperationCanceledException)
                    {
                        taskCompletionSource.TrySetCanceled();
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
                    _logger.LogWarning("StudentListViewModel initialization timed out after 2 seconds, continuing startup");
                }

                studentListStopwatch.Stop();
                _studentListLoadTime = studentListStopwatch.Elapsed;
                _logger.LogInformation("StudentListViewModel initialization process took {ElapsedMs}ms",
                    _studentListLoadTime.TotalMilliseconds);

                _logger.LogInformation("Background initialization completed, remaining ViewModels will be initialized on-demand");

                // DO NOT initialize other view models eagerly - they will be initialized on-demand
                // when their tab is selected, which significantly improves startup time
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Non-critical data initialization was canceled");
                throw; // Propagate cancellation
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
