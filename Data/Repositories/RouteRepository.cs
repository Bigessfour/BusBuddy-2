using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Bus_Buddy.Models;
using Bus_Buddy.Data;

namespace Bus_Buddy.Data.Repositories
{
    /// <summary>
    /// Repository for Route entity operations
    /// Provides data access methods for route management
    /// </summary>
    public class RouteRepository
    {
        private readonly BusBuddyDbContext _context;
        private readonly ILogger<RouteRepository> _logger;

        public RouteRepository(BusBuddyDbContext context, ILogger<RouteRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all routes from the database
        /// </summary>
        public async Task<List<Route>> GetAllRoutesAsync()
        {
            try
            {
                return await _context.Routes
                    .OrderBy(r => r.RouteName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all routes");
                return new List<Route>();
            }
        }

        /// <summary>
        /// Gets a specific route by ID
        /// </summary>
        public async Task<Route?> GetRouteByIdAsync(int routeId)
        {
            try
            {
                return await _context.Routes
                    .FirstOrDefaultAsync(r => r.RouteId == routeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving route with ID {routeId}");
                return null;
            }
        }

        /// <summary>
        /// Gets all active routes
        /// </summary>
        public async Task<List<Route>> GetActiveRoutesAsync()
        {
            try
            {
                return await _context.Routes
                    .Where(r => r.IsActive)
                    .OrderBy(r => r.RouteName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active routes");
                return new List<Route>();
            }
        }

        /// <summary>
        /// Creates a new route
        /// </summary>
        public async Task<Route?> CreateRouteAsync(Route route)
        {
            try
            {
                _context.Routes.Add(route);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Created new route: {route.RouteName}");
                return route;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating route: {route.RouteName}");
                return null;
            }
        }

        /// <summary>
        /// Updates an existing route
        /// </summary>
        public async Task<bool> UpdateRouteAsync(Route route)
        {
            try
            {
                _context.Routes.Update(route);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Updated route: {route.RouteName}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating route: {route.RouteName}");
                return false;
            }
        }

        /// <summary>
        /// Deletes a route
        /// </summary>
        public async Task<bool> DeleteRouteAsync(int routeId)
        {
            try
            {
                var route = await GetRouteByIdAsync(routeId);
                if (route != null)
                {
                    _context.Routes.Remove(route);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Deleted route: {route.RouteName}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting route with ID {routeId}");
                return false;
            }
        }

        /// <summary>
        /// Gets routes by rider count range
        /// </summary>
        public async Task<List<Route>> GetRoutesByRiderCountRangeAsync(int minRiders, int maxRiders)
        {
            try
            {
                return await _context.Routes
                    .Where(r => (r.AMRiders ?? 0) + (r.PMRiders ?? 0) >= minRiders &&
                               (r.AMRiders ?? 0) + (r.PMRiders ?? 0) <= maxRiders)
                    .OrderBy(r => r.RouteName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving routes by rider count range {minRiders}-{maxRiders}");
                return new List<Route>();
            }
        }

        /// <summary>
        /// Gets route statistics
        /// </summary>
        public async Task<RouteStatistics> GetRouteStatisticsAsync()
        {
            try
            {
                var routes = await GetAllRoutesAsync();

                return new RouteStatistics
                {
                    TotalRoutes = routes.Count,
                    ActiveRoutes = routes.Count(r => r.IsActive),
                    InactiveRoutes = routes.Count(r => !r.IsActive),
                    AverageMileage = routes.Any() ? routes.Average(r =>
                        ((r.AMEndMiles ?? 0) - (r.AMBeginMiles ?? 0)) +
                        ((r.PMEndMiles ?? 0) - (r.PMBeginMiles ?? 0))) : 0,
                    TotalMileage = routes.Sum(r =>
                        ((r.AMEndMiles ?? 0) - (r.AMBeginMiles ?? 0)) +
                        ((r.PMEndMiles ?? 0) - (r.PMBeginMiles ?? 0))),
                    AverageRiders = routes.Any() ? routes.Average(r => (r.AMRiders ?? 0) + (r.PMRiders ?? 0)) : 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating route statistics");
                return new RouteStatistics();
            }
        }
    }

    /// <summary>
    /// Route statistics data class
    /// </summary>
    public class RouteStatistics
    {
        public int TotalRoutes { get; set; }
        public int ActiveRoutes { get; set; }
        public int InactiveRoutes { get; set; }
        public decimal AverageMileage { get; set; }
        public decimal TotalMileage { get; set; }
        public double AverageRiders { get; set; }
    }
}
