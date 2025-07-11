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
            System.Diagnostics.Debug.WriteLine("[DEBUG] RoutePopulationScaffold.GetOptimizedRoutesAsync START");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var routes = await _routeService.GetAllActiveRoutesAsync();
            // TODO: Process routes as needed for dashboard or caching

            stopwatch.Stop();
            System.Diagnostics.Debug.WriteLine($"[DEBUG] RoutePopulationScaffold.GetOptimizedRoutesAsync END - Retrieved {routes.Count()} routes in {stopwatch.ElapsedMilliseconds}ms");
            return routes.ToList();
        }

        public async Task PopulateRoutesAsync()
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] RoutePopulationScaffold.PopulateRoutesAsync START");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // In a real application, this would involve more complex logic
            // such as fetching from a remote source, calculating routes, etc.
            // For now, we just ensure the service is working.
            var routes = await GetOptimizedRoutesAsync();

            stopwatch.Stop();
            System.Diagnostics.Debug.WriteLine($"[DEBUG] RoutePopulationScaffold.PopulateRoutesAsync END - Completed in {stopwatch.ElapsedMilliseconds}ms");
        }

        public async Task PopulateRouteMetadataAsync()
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] RoutePopulationScaffold.PopulateRouteMetadataAsync START");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // OPTIMIZATION: This is a lightweight version that only retrieves
            // minimal route data needed for the dashboard
            try
            {
                // Direct database access could be used here for a more targeted query
                // that only fetches the exact fields needed for the dashboard
                System.Diagnostics.Debug.WriteLine("[DEBUG] RoutePopulationScaffold: Calling GetAllActiveRoutesAsync");
                var routes = await _routeService.GetAllActiveRoutesAsync();
                System.Diagnostics.Debug.WriteLine($"[DEBUG] RoutePopulationScaffold: Retrieved {routes.Count()} active routes");

                // Here we could perform any necessary processing or caching
                // but we're keeping it minimal for faster startup

                stopwatch.Stop();
                System.Diagnostics.Debug.WriteLine($"[DEBUG] RoutePopulationScaffold.PopulateRouteMetadataAsync END - Completed in {stopwatch.ElapsedMilliseconds}ms");
            }
            catch (System.Exception ex)
            {
                stopwatch.Stop();
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Error in PopulateRouteMetadataAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Failed after {stopwatch.ElapsedMilliseconds}ms");
                // Log but don't throw to avoid startup failures
            }
        }
    }
}
