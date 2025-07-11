using BusBuddy.Core.Services.Interfaces;

namespace BusBuddy.Core.Services
{
    public class RoutePopulationScaffold
    {
        private readonly IBusService _busService;

        public RoutePopulationScaffold(IBusService busService)
        {
            _busService = busService;
        }

        /// <summary>
        /// Returns a list of routes sorted by distance (shortest first)
        /// </summary>
        public async Task<List<BusBuddy.Core.Models.Route>> GetOptimizedRoutesAsync()
        {
            var routes = await _busService.GetAllRouteEntitiesAsync();
            // Simple heuristic: sort by distance (nulls last)
            var optimized = routes
                .OrderBy(r => r.Distance ?? decimal.MaxValue)
                .ThenBy(r => r.EstimatedDuration ?? int.MaxValue)
                .ToList();
            return optimized;
        }
    }
}
