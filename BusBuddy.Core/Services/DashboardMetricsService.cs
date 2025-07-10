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
                // Use EF Core async methods to avoid DbContext threading issues
                var busCount = await _unitOfWork.Buses.Query().CountAsync(b => b.Status == "Active");
                var driverCount = await _unitOfWork.Drivers.Query().CountAsync(d => d.Status == "Active");
                var routeCount = await _unitOfWork.Routes.Query().CountAsync(r => r.IsActive);

                result["BusCount"] = busCount;
                result["DriverCount"] = driverCount;
                result["RouteCount"] = routeCount;

                // Placeholders for future metrics
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
