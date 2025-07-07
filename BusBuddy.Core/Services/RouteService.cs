using BusBuddy.Core.Data;
using BusBuddy.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusBuddy.Core.Services
{
    /// <summary>
    /// Service implementation for Route management operations
    /// Addresses critical blocker: Missing RouteService implementation
    /// Adapted to work with the actual Route model structure (daily route records)
    /// </summary>
    public class RouteService : IRouteService
    {
        private readonly BusBuddyDbContext _context;

        public RouteService(BusBuddyDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Route>> GetAllActiveRoutesAsync()
        {
            return await _context.Routes
                .Where(r => r.IsActive)
                .Include(r => r.AMVehicle)
                .Include(r => r.PMVehicle)
                .Include(r => r.AMDriver)
                .Include(r => r.PMDriver)
                .OrderBy(r => r.RouteName)
                .ToListAsync();
        }

        public async Task<Route> GetRouteByIdAsync(int id)
        {
            var route = await _context.Routes
                .Include(r => r.AMVehicle)
                .Include(r => r.PMVehicle)
                .Include(r => r.AMDriver)
                .Include(r => r.PMDriver)
                .FirstOrDefaultAsync(r => r.RouteId == id);

            if (route == null)
                throw new InvalidOperationException($"Route with ID {id} not found.");

            return route;
        }

        public async Task<Route> CreateRouteAsync(Route route)
        {
            if (route == null)
                throw new ArgumentNullException(nameof(route));

            // Validate route name uniqueness for the same date
            if (await IsRouteNameExistsForDateAsync(route.RouteName, route.Date))
                throw new InvalidOperationException($"Route '{route.RouteName}' already exists for date {route.Date:yyyy-MM-dd}.");

            route.IsActive = true;

            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            return route;
        }

        public async Task<Route> UpdateRouteAsync(Route route)
        {
            if (route == null)
                throw new ArgumentNullException(nameof(route));

            var existingRoute = await _context.Routes.FindAsync(route.RouteId);
            if (existingRoute == null)
                throw new InvalidOperationException($"Route with ID {route.RouteId} not found.");

            // Validate route name uniqueness for the same date (excluding current route)
            if (await IsRouteNameExistsForDateAsync(route.RouteName, route.Date, route.RouteId))
                throw new InvalidOperationException($"Route '{route.RouteName}' already exists for date {route.Date:yyyy-MM-dd}.");

            // Update properties
            existingRoute.Date = route.Date;
            existingRoute.RouteName = route.RouteName;
            existingRoute.Description = route.Description;
            existingRoute.IsActive = route.IsActive;

            // AM Route properties
            existingRoute.AMVehicleId = route.AMVehicleId;
            existingRoute.AMBeginMiles = route.AMBeginMiles;
            existingRoute.AMEndMiles = route.AMEndMiles;
            existingRoute.AMRiders = route.AMRiders;
            existingRoute.AMDriverId = route.AMDriverId;

            // PM Route properties
            existingRoute.PMVehicleId = route.PMVehicleId;
            existingRoute.PMBeginMiles = route.PMBeginMiles;
            existingRoute.PMEndMiles = route.PMEndMiles;
            existingRoute.PMRiders = route.PMRiders;
            existingRoute.PMDriverId = route.PMDriverId;

            await _context.SaveChangesAsync();
            return existingRoute;
        }

        public async Task<bool> DeleteRouteAsync(int id)
        {
            var route = await _context.Routes.FindAsync(id);
            if (route == null)
                return false;

            // Soft delete - mark as inactive instead of removing
            route.IsActive = false;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Route>> SearchRoutesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllActiveRoutesAsync();

            var lowerSearchTerm = searchTerm.ToLower();

            return await _context.Routes
                .Where(r => r.IsActive && (
                    r.RouteName.ToLower().Contains(lowerSearchTerm) ||
                    (r.Description != null && r.Description.ToLower().Contains(lowerSearchTerm))
                ))
                .Include(r => r.AMVehicle)
                .Include(r => r.PMVehicle)
                .Include(r => r.AMDriver)
                .Include(r => r.PMDriver)
                .OrderBy(r => r.RouteName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Route>> GetRoutesByBusIdAsync(int busId)
        {
            return await _context.Routes
                .Where(r => r.IsActive && (r.AMVehicleId == busId || r.PMVehicleId == busId))
                .Include(r => r.AMVehicle)
                .Include(r => r.PMVehicle)
                .Include(r => r.AMDriver)
                .Include(r => r.PMDriver)
                .OrderBy(r => r.Date)
                .ThenBy(r => r.RouteName)
                .ToListAsync();
        }

        public async Task<bool> IsRouteNumberUniqueAsync(string routeNumber, int? excludeId = null)
        {
            // Adapted to work with RouteName instead of RouteNumber
            var query = _context.Routes.Where(r => r.RouteName == routeNumber && r.IsActive);

            if (excludeId.HasValue)
                query = query.Where(r => r.RouteId != excludeId.Value);

            return !await query.AnyAsync();
        }

        public async Task<IEnumerable<RouteStop>> GetRouteStopsAsync(int routeId)
        {
            return await _context.RouteStops
                .Where(rs => rs.RouteId == routeId)
                .OrderBy(rs => rs.StopOrder)
                .ToListAsync();
        }

        public async Task<RouteStop> AddRouteStopAsync(RouteStop routeStop)
        {
            if (routeStop == null)
                throw new ArgumentNullException(nameof(routeStop));

            // Validate route exists
            var routeExists = await _context.Routes.AnyAsync(r => r.RouteId == routeStop.RouteId);
            if (!routeExists)
                throw new InvalidOperationException($"Route with ID {routeStop.RouteId} not found.");

            routeStop.CreatedDate = DateTime.UtcNow;
            _context.RouteStops.Add(routeStop);
            await _context.SaveChangesAsync();

            return routeStop;
        }

        public async Task<RouteStop> UpdateRouteStopAsync(RouteStop routeStop)
        {
            if (routeStop == null)
                throw new ArgumentNullException(nameof(routeStop));

            var existingStop = await _context.RouteStops.FindAsync(routeStop.RouteStopId);
            if (existingStop == null)
                throw new InvalidOperationException($"Route stop with ID {routeStop.RouteStopId} not found.");

            // Update properties
            existingStop.StopName = routeStop.StopName;
            existingStop.StopAddress = routeStop.StopAddress;
            existingStop.StopOrder = routeStop.StopOrder;
            existingStop.Latitude = routeStop.Latitude;
            existingStop.Longitude = routeStop.Longitude;
            existingStop.ScheduledArrival = routeStop.ScheduledArrival;
            existingStop.ScheduledDeparture = routeStop.ScheduledDeparture;
            existingStop.StopDuration = routeStop.StopDuration;
            existingStop.Status = routeStop.Status;
            existingStop.Notes = routeStop.Notes;
            existingStop.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingStop;
        }

        public async Task<bool> DeleteRouteStopAsync(int routeStopId)
        {
            var routeStop = await _context.RouteStops.FindAsync(routeStopId);
            if (routeStop == null)
                return false;

            _context.RouteStops.Remove(routeStop);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> GetRouteTotalDistanceAsync(int routeId)
        {
            var route = await _context.Routes.FindAsync(routeId);
            if (route == null)
                return 0;

            // Calculate distance from AM and PM miles if available
            decimal amDistance = 0;
            decimal pmDistance = 0;

            if (route.AMBeginMiles.HasValue && route.AMEndMiles.HasValue)
                amDistance = route.AMEndMiles.Value - route.AMBeginMiles.Value;

            if (route.PMBeginMiles.HasValue && route.PMEndMiles.HasValue)
                pmDistance = route.PMEndMiles.Value - route.PMBeginMiles.Value;

            return amDistance + pmDistance;
        }

        public async Task<TimeSpan> GetRouteEstimatedTimeAsync(int routeId)
        {
            // Calculate estimated time based on route stops
            var routeStops = await GetRouteStopsAsync(routeId);

            if (!routeStops.Any())
                return TimeSpan.Zero;

            var firstStop = routeStops.OrderBy(rs => rs.StopOrder).First();
            var lastStop = routeStops.OrderByDescending(rs => rs.StopOrder).First();

            return lastStop.ScheduledDeparture - firstStop.ScheduledArrival;
        }

        private async Task<bool> IsRouteNameExistsForDateAsync(string routeName, DateTime date, int? excludeId = null)
        {
            var query = _context.Routes.Where(r => r.RouteName == routeName && r.Date.Date == date.Date && r.IsActive);

            if (excludeId.HasValue)
                query = query.Where(r => r.RouteId != excludeId.Value);

            return await query.AnyAsync();
        }
    }
}
