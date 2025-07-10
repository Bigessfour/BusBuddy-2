using System.Threading.Tasks;
using BusBuddy.Core.Services;
using BusBuddy.Core.Services.Interfaces;
using System.Collections.Generic;
using BusBuddy.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BusBuddy.WPF.Services
{
    public class RoutePopulationScaffold : IRoutePopulationScaffold
    {
        private readonly BusBuddy.Core.Services.IRouteService _routeService;

        public RoutePopulationScaffold(BusBuddy.Core.Services.IRouteService routeService)
        {
            _routeService = routeService;
        }

        public async Task<System.Collections.Generic.List<BusBuddy.Core.Models.Route>> GetOptimizedRoutesAsync()
        {
            var routes = await _routeService.GetAllActiveRoutesAsync();
            // TODO: Process routes as needed for dashboard or caching
            return routes.ToList();
        }

        public async Task PopulateRoutesAsync()
        {
            // In a real application, this would involve more complex logic
            // such as fetching from a remote source, calculating routes, etc.
            // For now, we just ensure the service is working.
            await GetOptimizedRoutesAsync();
        }

        public async Task PopulateRouteMetadataAsync()
        {
            // OPTIMIZATION: This is a lightweight version that only retrieves
            // minimal route data needed for the dashboard
            try
            {
                // Direct database access could be used here for a more targeted query
                // that only fetches the exact fields needed for the dashboard
                var routes = await _routeService.GetAllActiveRoutesAsync();

                // Here we could perform any necessary processing or caching
                // but we're keeping it minimal for faster startup
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in PopulateRouteMetadataAsync: {ex.Message}");
                // Log but don't throw to avoid startup failures
            }
        }
    }
}
