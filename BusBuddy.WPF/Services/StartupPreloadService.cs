using BusBuddy.Core.Services;
using BusBuddy.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BusBuddy.WPF.Services
{
    /// <summary>
    /// Service to preload essential data during application startup for better performance
    /// </summary>
    public class StartupPreloadService : IStartupPreloadService
    {
        private readonly IEnhancedCachingService _cachingService;
        private readonly IBusService _busService;
        private readonly IDriverService _driverService;
        private readonly IRouteService _routeService;
        private readonly IStudentService _studentService;
        private readonly ILogger<StartupPreloadService> _logger;

        public StartupPreloadService(
            IEnhancedCachingService cachingService,
            IBusService busService,
            IDriverService driverService,
            IRouteService routeService,
            IStudentService studentService,
            ILogger<StartupPreloadService> logger)
        {
            _cachingService = cachingService;
            _busService = busService;
            _driverService = driverService;
            _routeService = routeService;
            _studentService = studentService;
            _logger = logger;
        }

        /// <summary>
        /// Preload all essential data needed for the application dashboard
        /// </summary>
        public async Task PreloadEssentialDataAsync()
        {
            _logger.LogInformation("Starting essential data preload");
            var overallStopwatch = Stopwatch.StartNew();

            try
            {
                // Load data in parallel for better performance
                var preloadTasks = new[]
                {
                    PreloadBusesAsync(),
                    PreloadDriversAsync(),
                    PreloadRoutesAsync(),
                    PreloadStudentsAsync()
                };

                await Task.WhenAll(preloadTasks);

                overallStopwatch.Stop();
                _logger.LogInformation("Essential data preload completed in {ElapsedMs}ms", overallStopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                overallStopwatch.Stop();
                _logger.LogError(ex, "Error during essential data preload after {ElapsedMs}ms", overallStopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        /// <summary>
        /// Preload dashboard-specific data
        /// </summary>
        public async Task PreloadDashboardDataAsync()
        {
            _logger.LogInformation("Starting dashboard data preload");
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Load essential data for dashboard metrics
                await PreloadEssentialDataAsync();

                stopwatch.Stop();
                _logger.LogInformation("Dashboard data preload completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error during dashboard data preload after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        /// <summary>
        /// Check if essential data is already cached
        /// </summary>
        public bool IsEssentialDataCached()
        {
            // This is a simple check - in a real implementation, you might want to be more sophisticated
            try
            {
                var busesTask = _cachingService.GetAllBusesAsync(async () => await _busService.GetAllBusesAsync());
                var driversTask = _cachingService.GetAllDriversAsync(async () => await _driverService.GetAllDriversAsync());
                var routesTask = _cachingService.GetAllRoutesAsync(async () => await _routeService.GetAllActiveRoutesAsync());
                var studentsTask = _cachingService.GetAllStudentsAsync(async () => await _studentService.GetAllStudentsAsync());

                // Check if any of these complete immediately (indicating cached data)
                return busesTask.IsCompletedSuccessfully || driversTask.IsCompletedSuccessfully ||
                       routesTask.IsCompletedSuccessfully || studentsTask.IsCompletedSuccessfully;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get preload statistics
        /// </summary>
        public async Task<PreloadStatistics> GetPreloadStatisticsAsync()
        {
            var stats = new PreloadStatistics();

            try
            {
                var buses = await _cachingService.GetAllBusesAsync(async () => await _busService.GetAllBusesAsync());
                var drivers = await _cachingService.GetAllDriversAsync(async () => await _driverService.GetAllDriversAsync());
                var routes = await _cachingService.GetAllRoutesAsync(async () => await _routeService.GetAllActiveRoutesAsync());
                var students = await _cachingService.GetAllStudentsAsync(async () => await _studentService.GetAllStudentsAsync());

                stats.BusCount = buses.Count;
                stats.DriverCount = drivers.Count;
                stats.RouteCount = routes.Count;
                stats.StudentCount = students.Count;
                stats.IsDataCached = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting preload statistics");
                stats.IsDataCached = false;
            }

            return stats;
        }

        private async Task PreloadBusesAsync()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                await _cachingService.GetAllBusesAsync(async () => await _busService.GetAllBusesAsync());
                stopwatch.Stop();
                _logger.LogDebug("Preloaded buses in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preloading buses");
            }
        }

        private async Task PreloadDriversAsync()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                await _cachingService.GetAllDriversAsync(async () => await _driverService.GetAllDriversAsync());
                stopwatch.Stop();
                _logger.LogDebug("Preloaded drivers in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preloading drivers");
            }
        }

        private async Task PreloadRoutesAsync()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                await _cachingService.GetAllRoutesAsync(async () => await _routeService.GetAllActiveRoutesAsync());
                stopwatch.Stop();
                _logger.LogDebug("Preloaded routes in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preloading routes");
            }
        }

        private async Task PreloadStudentsAsync()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                await _cachingService.GetAllStudentsAsync(async () => await _studentService.GetAllStudentsAsync());
                stopwatch.Stop();
                _logger.LogDebug("Preloaded students in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preloading students");
            }
        }
    }

    /// <summary>
    /// Interface for startup preload service
    /// </summary>
    public interface IStartupPreloadService
    {
        Task PreloadEssentialDataAsync();
        Task PreloadDashboardDataAsync();
        bool IsEssentialDataCached();
        Task<PreloadStatistics> GetPreloadStatisticsAsync();
    }

    /// <summary>
    /// Statistics about preloaded data
    /// </summary>
    public class PreloadStatistics
    {
        public int BusCount { get; set; }
        public int DriverCount { get; set; }
        public int RouteCount { get; set; }
        public int StudentCount { get; set; }
        public bool IsDataCached { get; set; }
    }
}
