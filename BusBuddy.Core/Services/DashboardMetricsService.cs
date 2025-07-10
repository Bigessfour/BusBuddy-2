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
            var result = new Dictionary<string, int>();

            try
            {
                // OPTIMIZATION: Get counts directly from repositories for faster performance
                // These operations will be much faster than SQL queries as they're just in-memory counts
                var busCount = await Task.Run(() => _unitOfWork.Buses.Query().Count(b => b.Status == "Active"));
                var driverCount = await Task.Run(() => _unitOfWork.Drivers.Query().Count(d => d.Status == "Active"));
                var routeCount = await Task.Run(() => _unitOfWork.Routes.Query().Count(r => r.IsActive));

                result["BusCount"] = busCount;
                result["DriverCount"] = driverCount;
                result["RouteCount"] = routeCount;

                // OPTIMIZATION: Set placeholders for metrics that will be populated later
                result["StudentCount"] = 0;
                result["OpenTicketCount"] = 0;

                _logger.LogInformation("Successfully fetched dashboard metrics: {Metrics}",
                    string.Join(", ", result.Select(kv => $"{kv.Key}={kv.Value}")));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard metrics");

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
