using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using BusBuddy.Core.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BusBuddy.Core.Services
{
    public interface IDashboardMetricsService
    {
        Task<Dictionary<string, int>> GetDashboardMetricsAsync();
    }

    public class DashboardMetricsService : IDashboardMetricsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DashboardMetricsService> _logger;

        public DashboardMetricsService(IUnitOfWork unitOfWork, ILogger<DashboardMetricsService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Dictionary<string, int>> GetDashboardMetricsAsync()
        {
            _logger.LogInformation("Fetching dashboard metrics with optimized query");
            System.Diagnostics.Debug.WriteLine("[DEBUG] DashboardMetricsService.GetDashboardMetricsAsync START");
            var result = new Dictionary<string, int>();
            var totalStopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // Use EF Core async methods to avoid DbContext threading issues
                System.Diagnostics.Debug.WriteLine("[DEBUG] DashboardMetricsService: Starting bus count query");
                var busStopwatch = System.Diagnostics.Stopwatch.StartNew();
                var busCount = await _unitOfWork.Buses.Query().CountAsync(b => b.Status == "Active");
                busStopwatch.Stop();
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Query result BusCount: {busCount} (took {busStopwatch.ElapsedMilliseconds}ms)");

                System.Diagnostics.Debug.WriteLine("[DEBUG] DashboardMetricsService: Starting driver count query");
                var driverStopwatch = System.Diagnostics.Stopwatch.StartNew();
                var driverCount = await _unitOfWork.Drivers.Query().CountAsync(d => d.Status == "Active");
                driverStopwatch.Stop();
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Query result DriverCount: {driverCount} (took {driverStopwatch.ElapsedMilliseconds}ms)");

                System.Diagnostics.Debug.WriteLine("[DEBUG] DashboardMetricsService: Starting route count query");
                var routeStopwatch = System.Diagnostics.Stopwatch.StartNew();
                // Routes are considered active based on IsActive flag
                var routeCount = await _unitOfWork.Routes.Query().CountAsync(r => r.IsActive);
                routeStopwatch.Stop();
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Query result RouteCount: {routeCount} (took {routeStopwatch.ElapsedMilliseconds}ms)");

                result["BusCount"] = busCount;
                result["DriverCount"] = driverCount;
                result["RouteCount"] = routeCount;

                // Placeholders for future metrics
                result["StudentCount"] = 0;
                result["OpenTicketCount"] = 0;

                _logger.LogInformation("Successfully fetched dashboard metrics: {Metrics}",
                    string.Join(", ", result.Select(kv => $"{kv.Key}={kv.Value}")));

                totalStopwatch.Stop();
                System.Diagnostics.Debug.WriteLine($"[DEBUG] DashboardMetricsService.GetDashboardMetricsAsync SUCCESS - Total execution time: {totalStopwatch.ElapsedMilliseconds}ms");

                return result;
            }
            catch (Exception ex)
            {
                totalStopwatch.Stop();
                _logger.LogError(ex, "Error fetching dashboard metrics");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] DashboardMetricsService.GetDashboardMetricsAsync EXCEPTION: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Failed after {totalStopwatch.ElapsedMilliseconds}ms");

                // Return empty results in case of error
                result["BusCount"] = 0;
                result["DriverCount"] = 0;
                result["RouteCount"] = 0;
                result["StudentCount"] = 0;
                result["OpenTicketCount"] = 0;

                return result;
            }
        }
    }
}
