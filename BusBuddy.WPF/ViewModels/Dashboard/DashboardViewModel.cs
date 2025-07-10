using System.Threading.Tasks;
using BusBuddy.WPF.Services;
using BusBuddy.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using BusBuddy.WPF.Utilities;

namespace BusBuddy.WPF.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly IRoutePopulationScaffold _routePopulationScaffold;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DashboardViewModel> _logger;
        private StudentListViewModel? _studentListViewModel;

        // Performance tracking metrics
        private TimeSpan _initializationTime;
        private TimeSpan _routePopulationTime;
        private TimeSpan _studentListLoadTime;

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

        public DashboardViewModel(
            IRoutePopulationScaffold routePopulationScaffold,
            IServiceProvider serviceProvider,
            ILogger<DashboardViewModel> logger)
        {
            _routePopulationScaffold = routePopulationScaffold ?? throw new ArgumentNullException(nameof(routePopulationScaffold));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logger.LogInformation("DashboardViewModel constructor completed successfully");
        }

        // Expose performance metrics as public properties
        public string InitializationTimeFormatted => FormatUtils.FormatDuration((int)_initializationTime.TotalMilliseconds / 1000);
        public string RoutePopulationTimeFormatted => FormatUtils.FormatDuration((int)_routePopulationTime.TotalMilliseconds / 1000);
        public string StudentListLoadTimeFormatted => FormatUtils.FormatDuration((int)_studentListLoadTime.TotalMilliseconds / 1000);

        public async Task InitializeAsync()
        {
            var totalStopwatch = Stopwatch.StartNew();

            _logger.LogInformation("DashboardViewModel.InitializeAsync called");
            Debug.WriteLine("DashboardViewModel.InitializeAsync called");

            try
            {
                await LoadDashboardDataAsync();

                // Record total initialization time
                totalStopwatch.Stop();
                _initializationTime = totalStopwatch.Elapsed;

                _logger.LogInformation("DashboardViewModel.InitializeAsync completed successfully in {0}ms", _initializationTime.TotalMilliseconds);
                Debug.WriteLine($"DashboardViewModel.InitializeAsync completed successfully in {_initializationTime.TotalMilliseconds}ms");

                // Log detailed performance metrics
                _logger.LogInformation("Performance Metrics - Total: {0}ms, Routes: {1}ms, StudentList: {2}ms",
                    _initializationTime.TotalMilliseconds,
                    _routePopulationTime.TotalMilliseconds,
                    _studentListLoadTime.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                totalStopwatch.Stop();
                ErrorMessage = $"Initialize failed: {ex.Message}";
                _logger.LogError(ex, "DashboardViewModel.InitializeAsync failed with exception after {0}ms: {1}",
                    totalStopwatch.ElapsedMilliseconds, ex.Message);
                Debug.WriteLine($"InitializeAsync error after {totalStopwatch.ElapsedMilliseconds}ms: {ex}");
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "Inner exception: {0}", ex.InnerException.Message);
                    Debug.WriteLine($"Inner exception: {ex.InnerException}");
                }
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

                    await _routePopulationScaffold.PopulateRoutesAsync();

                    routeStopwatch.Stop();
                    _routePopulationTime = routeStopwatch.Elapsed;
                    _logger.LogInformation("Routes populated successfully in {0}ms", _routePopulationTime.TotalMilliseconds);
                    Debug.WriteLine($"Route population completed successfully in {_routePopulationTime.TotalMilliseconds}ms");

                    // Initialize StudentListViewModel by accessing the property
                    // This will trigger the lazy loading through the service provider
                    _logger.LogInformation("Initializing StudentListViewModel through service provider");
                    Debug.WriteLine("Attempting to initialize StudentListViewModel");

                    // Measure student list loading time
                    var studentListStopwatch = Stopwatch.StartNew();
                    var viewModel = StudentListViewModel;

                    // Wait for the view model to finish loading its data
                    if (viewModel?.Initialized != null)
                    {
                        await viewModel.Initialized;
                    }

                    studentListStopwatch.Stop();
                    _studentListLoadTime = studentListStopwatch.Elapsed;

                    _logger.LogInformation("StudentListViewModel initialized successfully in {0}ms: {1}",
                        _studentListLoadTime.TotalMilliseconds,
                        viewModel != null ? "Instance created" : "NULL");
                    Debug.WriteLine($"StudentListViewModel initialized in {_studentListLoadTime.TotalMilliseconds}ms: {(viewModel != null ? "SUCCESS" : "FAILED - NULL INSTANCE")}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading dashboard data: {0}", ex.Message);
                    Debug.WriteLine($"LoadDashboardDataAsync ERROR: {ex.Message}");
                    Debug.WriteLine($"STACK TRACE: {ex.StackTrace}");
                    throw; // Re-throw to be caught by LoadDataAsync
                }
            });
        }
    }
}
