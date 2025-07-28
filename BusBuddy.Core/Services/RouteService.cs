using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BusBuddy.Core.Data;
using BusBuddy.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace BusBuddy.Core.Services
{
    /// <summary>
    /// Service implementation for Route management operations
    /// Addresses critical blocker: Missing RouteService implementation
    /// Adapted to work with the actual Route model structure (daily route records)
    /// </summary>
    public class RouteService : IRouteService
    {
        private readonly IBusBuddyDbContextFactory _contextFactory;

        public RouteService(IBusBuddyDbContextFactory contextFactory)
        {
            ArgumentNullException.ThrowIfNull(contextFactory);
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<Route>> GetAllActiveRoutesAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Routes
                    .Where(r => r.IsActive)
                    .AsNoTracking()
                    .Select(r => new Route
                    {
                        RouteId = r.RouteId,
                        RouteName = r.RouteName,
                        Date = r.Date,
                        Description = r.Description,
                        IsActive = r.IsActive,
                        Distance = r.Distance,
                        EstimatedDuration = r.EstimatedDuration,
                        StudentCount = r.StudentCount,

                        // AM details
                        AMVehicleId = r.AMVehicleId,
                        AMDriverId = r.AMDriverId,
                        AMBeginMiles = r.AMBeginMiles,
                        AMEndMiles = r.AMEndMiles,
                        AMRiders = r.AMRiders,
                        AMBeginTime = r.AMBeginTime,

                        // PM details
                        PMVehicleId = r.PMVehicleId,
                        PMDriverId = r.PMDriverId,
                        PMBeginMiles = r.PMBeginMiles,
                        PMEndMiles = r.PMEndMiles,
                        PMRiders = r.PMRiders,
                        PMBeginTime = r.PMBeginTime,

                        // Basic reference data for display
                        BusNumber = r.BusNumber,
                        DriverName = r.DriverName,

                        // Join to minimal vehicle info
                        AMVehicle = r.AMVehicleId != null ? new Bus
                        {
                            VehicleId = (int)r.AMVehicleId,
                            BusNumber = r.AMVehicle != null ? r.AMVehicle.BusNumber : string.Empty,
                            Status = r.AMVehicle != null ? r.AMVehicle.Status : "Unknown"
                        } : null,

                        // Join to minimal driver info
                        AMDriver = r.AMDriverId != null ? new Driver
                        {
                            DriverId = (int)r.AMDriverId,
                            DriverName = r.AMDriver != null ? r.AMDriver.DriverName : string.Empty,
                            Status = r.AMDriver != null ? r.AMDriver.Status : "Unknown"
                        } : null,

                        // Join to minimal vehicle info for PM
                        PMVehicle = r.PMVehicleId != null ? new Bus
                        {
                            VehicleId = (int)r.PMVehicleId,
                            BusNumber = r.PMVehicle != null ? r.PMVehicle.BusNumber : string.Empty,
                            Status = r.PMVehicle != null ? r.PMVehicle.Status : "Unknown"
                        } : null,

                        // Join to minimal driver info for PM
                        PMDriver = r.PMDriverId != null ? new Driver
                        {
                            DriverId = (int)r.PMDriverId,
                            DriverName = r.PMDriver != null ? r.PMDriver.DriverName : string.Empty,
                            Status = r.PMDriver != null ? r.PMDriver.Status : "Unknown"
                        } : null
                    })
                    .OrderBy(r => r.RouteName)
                    .ToListAsync();
            }
            catch (System.InvalidCastException ex) when (ex.Message.Contains("DateTime", StringComparison.OrdinalIgnoreCase))
            {
                // Handle datetime conversion errors by returning empty list
                return new List<Route>();
            }
            catch (System.Data.SqlTypes.SqlNullValueException)
            {
                // Handle null value exceptions by returning empty list
                return new List<Route>();
            }
        }

        public async Task<Route> GetRouteByIdAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var route = await context.Routes
                .Include(r => r.AMVehicle)
                .Include(r => r.PMVehicle)
                .Include(r => r.AMDriver)
                .Include(r => r.PMDriver)
                .FirstOrDefaultAsync(r => r.RouteId == id);

            if (route == null)
            {
                throw new InvalidOperationException($"Route with ID {id} not found.");
            }

            return route;
        }

        public async Task<Route> CreateRouteAsync(Route route)
        {
            ArgumentNullException.ThrowIfNull(route);

            using var context = _contextFactory.CreateWriteDbContext();
            // Validate route name uniqueness for the same date
            if (await IsRouteNameExistsForDateAsync(route.RouteName, route.Date))
            {
                throw new InvalidOperationException($"Route '{route.RouteName}' already exists for date {route.Date:yyyy-MM-dd}.");
            }

            route.IsActive = true;

            context.Routes.Add(route);
            await context.SaveChangesAsync();

            return route;
        }

        public async Task<Route> UpdateRouteAsync(Route route)
        {
            ArgumentNullException.ThrowIfNull(route);

            using var context = _contextFactory.CreateWriteDbContext();
            var existingRoute = await context.Routes.FindAsync(route.RouteId);
            if (existingRoute == null)
            {
                throw new InvalidOperationException($"Route with ID {route.RouteId} not found.");
            }

            // Validate route name uniqueness for the same date (excluding current route)
            if (await IsRouteNameExistsForDateAsync(route.RouteName, route.Date, route.RouteId))
            {
                throw new InvalidOperationException($"Route '{route.RouteName}' already exists for date {route.Date:yyyy-MM-dd}.");
            }

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

            await context.SaveChangesAsync();
            return existingRoute;
        }

        public async Task<bool> DeleteRouteAsync(int id)
        {
            using var context = _contextFactory.CreateWriteDbContext();
            var route = await context.Routes.FindAsync(id);
            if (route == null)
            {
                return false;
            }

            // Soft delete - mark as inactive instead of removing
            route.IsActive = false;

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Route>> SearchRoutesAsync(string searchTerm)
        {
            using var context = _contextFactory.CreateDbContext();
            IQueryable<Route> query;

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                query = context.Routes
                    .Where(r => r.IsActive)
                    .AsNoTracking();
            }
            else
            {
                query = context.Routes
                    .Where(r => r.IsActive && (
                        r.RouteName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        (r.Description != null && r.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    ))
                    .AsNoTracking();
            }

            // Apply projection to select only needed fields
            return await query
                .Select(r => new Route
                {
                    RouteId = r.RouteId,
                    RouteName = r.RouteName,
                    Date = r.Date,
                    Description = r.Description,
                    IsActive = r.IsActive,
                    Distance = r.Distance,
                    EstimatedDuration = r.EstimatedDuration,
                    StudentCount = r.StudentCount,

                    // AM/PM basics
                    AMVehicleId = r.AMVehicleId,
                    AMDriverId = r.AMDriverId,
                    PMVehicleId = r.PMVehicleId,
                    PMDriverId = r.PMDriverId,

                    // Display info
                    BusNumber = r.BusNumber,
                    DriverName = r.DriverName,

                    // Minimal navigation properties with null checks
                    AMVehicle = r.AMVehicleId != null ? new Bus
                    {
                        VehicleId = (int)r.AMVehicleId,
                        BusNumber = r.AMVehicle != null ? r.AMVehicle.BusNumber : string.Empty
                    } : null,

                    AMDriver = r.AMDriverId != null ? new Driver
                    {
                        DriverId = (int)r.AMDriverId,
                        DriverName = r.AMDriver != null ? r.AMDriver.DriverName : string.Empty
                    } : null,

                    PMVehicle = r.PMVehicleId != null ? new Bus
                    {
                        VehicleId = (int)r.PMVehicleId,
                        BusNumber = r.PMVehicle != null ? r.PMVehicle.BusNumber : string.Empty
                    } : null,

                    PMDriver = r.PMDriverId != null ? new Driver
                    {
                        DriverId = (int)r.PMDriverId,
                        DriverName = r.PMDriver != null ? r.PMDriver.DriverName : string.Empty
                    } : null
                })
                .OrderBy(r => r.RouteName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Route>> GetRoutesByBusIdAsync(int busId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Routes
                .Where(r => r.IsActive && (r.AMVehicleId == busId || r.PMVehicleId == busId))
                .AsNoTracking()
                .Select(r => new Route
                {
                    RouteId = r.RouteId,
                    RouteName = r.RouteName,
                    Date = r.Date,
                    Description = r.Description,
                    IsActive = r.IsActive,
                    Distance = r.Distance,
                    EstimatedDuration = r.EstimatedDuration,

                    // AM/PM basics - focus on the bus we're filtering by
                    AMVehicleId = r.AMVehicleId,
                    AMBeginMiles = r.AMVehicleId == busId ? r.AMBeginMiles : null,
                    AMEndMiles = r.AMVehicleId == busId ? r.AMEndMiles : null,
                    AMDriverId = r.AMVehicleId == busId ? r.AMDriverId : null,

                    PMVehicleId = r.PMVehicleId,
                    PMBeginMiles = r.PMVehicleId == busId ? r.PMBeginMiles : null,
                    PMEndMiles = r.PMVehicleId == busId ? r.PMEndMiles : null,
                    PMDriverId = r.PMVehicleId == busId ? r.PMDriverId : null,

                    // Minimal navigation properties
                    AMDriver = r.AMVehicleId == busId && r.AMDriverId != null ? new Driver
                    {
                        DriverId = (int)r.AMDriverId,
                        DriverName = r.AMDriver != null ? r.AMDriver.DriverName : string.Empty
                    } : null,

                    PMDriver = r.PMVehicleId == busId && r.PMDriverId != null ? new Driver
                    {
                        DriverId = (int)r.PMDriverId,
                        DriverName = r.PMDriver != null ? r.PMDriver.DriverName : string.Empty
                    } : null
                })
                .OrderBy(r => r.Date)
                .ThenBy(r => r.RouteName)
                .ToListAsync();
        }

        public async Task<bool> IsRouteNumberUniqueAsync(string routeNumber, int? excludeId = null)
        {
            using var context = _contextFactory.CreateDbContext();
            // Adapted to work with RouteName instead of RouteNumber
            var query = context.Routes.Where(r => r.RouteName == routeNumber && r.IsActive);

            if (excludeId.HasValue)
            {
                query = query.Where(r => r.RouteId != excludeId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<IEnumerable<RouteStop>> GetRouteStopsAsync(int routeId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.RouteStops
                .Where(rs => rs.RouteId == routeId)
                .OrderBy(rs => rs.StopOrder)
                .ToListAsync();
        }

        public async Task<RouteStop> AddRouteStopAsync(RouteStop routeStop)
        {
            ArgumentNullException.ThrowIfNull(routeStop);

            using var context = _contextFactory.CreateWriteDbContext();
            // Validate route exists
            var routeExists = await context.Routes.AnyAsync(r => r.RouteId == routeStop.RouteId);
            if (!routeExists)
            {
                throw new InvalidOperationException($"Route with ID {routeStop.RouteId} not found.");
            }

            routeStop.CreatedDate = DateTime.UtcNow;
            context.RouteStops.Add(routeStop);
            await context.SaveChangesAsync();

            return routeStop;
        }

        public async Task<RouteStop> UpdateRouteStopAsync(RouteStop routeStop)
        {
            ArgumentNullException.ThrowIfNull(routeStop);

            using var context = _contextFactory.CreateWriteDbContext();
            var existingStop = await context.RouteStops.FindAsync(routeStop.RouteStopId);
            if (existingStop == null)
            {
                throw new InvalidOperationException($"Route stop with ID {routeStop.RouteStopId} not found.");
            }

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

            await context.SaveChangesAsync();
            return existingStop;
        }

        public async Task<bool> DeleteRouteStopAsync(int routeStopId)
        {
            using var context = _contextFactory.CreateWriteDbContext();
            var routeStop = await context.RouteStops.FindAsync(routeStopId);
            if (routeStop == null)
            {
                return false;
            }

            context.RouteStops.Remove(routeStop);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> GetRouteTotalDistanceAsync(int routeId)
        {
            using var context = _contextFactory.CreateDbContext();
            var route = await context.Routes.FindAsync(routeId);
            if (route == null)
            {
                return 0;
            }

            // Calculate distance from AM and PM miles if available
            decimal amDistance = 0;
            decimal pmDistance = 0;

            if (route.AMBeginMiles.HasValue && route.AMEndMiles.HasValue)
            {
                amDistance = route.AMEndMiles.Value - route.AMBeginMiles.Value;
            }

            if (route.PMBeginMiles.HasValue && route.PMEndMiles.HasValue)
            {
                pmDistance = route.PMEndMiles.Value - route.PMBeginMiles.Value;
            }

            return amDistance + pmDistance;
        }

        public async Task<TimeSpan> GetRouteEstimatedTimeAsync(int routeId)
        {
            // Calculate estimated time based on route stops
            var routeStops = await GetRouteStopsAsync(routeId);

            if (!routeStops.Any())
            {
                return TimeSpan.Zero;
            }

            var firstStop = routeStops.OrderBy(rs => rs.StopOrder).First();
            var lastStop = routeStops.OrderByDescending(rs => rs.StopOrder).First();

            return lastStop.ScheduledDeparture - firstStop.ScheduledArrival;
        }

        private async Task<bool> IsRouteNameExistsForDateAsync(string routeName, DateTime date, int? excludeId = null)
        {
            using var context = _contextFactory.CreateDbContext();
            var query = context.Routes.Where(r => r.RouteName == routeName && r.Date.Date == date.Date && r.IsActive);

            if (excludeId.HasValue)
            {
                query = query.Where(r => r.RouteId != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
