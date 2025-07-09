using BusBuddy.Core.Data;
using BusBuddy.Core.Extensions;
using BusBuddy.Core.Models;
using BusBuddy.Core.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System.Diagnostics;
using ActivityType = BusBuddy.Core.Models.Activity;

namespace BusBuddy.Core.Services
{
    public class BusService : IBusService
    {
        private readonly ILogger<BusService> _logger;
        private readonly BusBuddyDbContext _context;
        private readonly IBusCachingService _cacheService;
        private readonly List<BusInfo> _buses;
        private readonly List<RouteInfo> _routes;
        private readonly List<ScheduleInfo> _schedules;

        public BusService(
            ILogger<BusService> logger,
            BusBuddyDbContext context,
            IBusCachingService cacheService)
        {
            _logger = logger;
            _context = context;
            _cacheService = cacheService;
            _buses = new List<BusInfo>();
            _routes = new List<RouteInfo>();
            _schedules = new List<ScheduleInfo>();

            // Initialize with sample data for backward compatibility
            InitializeSampleData();
        }

        // Entity Framework methods for actual database operations using caching
        public async Task<List<Bus>> GetAllBusEntitiesAsync()
        {
            using (LogContext.PushProperty("QueryType", "GetAllBusEntities"))
            using (LogContext.PushProperty("OperationName", "DatabaseQuery"))
            {
                var stopwatch = Stopwatch.StartNew();
                _logger.LogInformation("Retrieving all bus entities (with caching)");

                try
                {
                    var result = await _cacheService.GetAllBusesAsync(async () =>
                    {
                        _logger.LogInformation("Cache miss - retrieving all bus entities from database");
                        return await _context.Vehicles
                            .Include(v => v.AMRoutes)
                            .Include(v => v.PMRoutes)
                            .ToListAsync();
                    });

                    stopwatch.Stop();
                    _logger.LogInformation("Retrieved {BusCount} bus entities in {Duration}ms",
                        result.Count, stopwatch.ElapsedMilliseconds);

                    return result;
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    _logger.LogError(ex, "Error retrieving all bus entities after {Duration}ms",
                        stopwatch.ElapsedMilliseconds);
                    throw;
                }
            }
        }

        public async Task<Bus?> GetBusEntityByIdAsync(int busId)
        {
            using (LogContext.PushProperty("QueryType", "GetBusEntityById"))
            using (LogContext.PushProperty("BusId", busId))
            using (LogContext.PushProperty("OperationName", "DatabaseQuery"))
            {
                var stopwatch = Stopwatch.StartNew();
                _logger.LogInformation("Retrieving bus entity with ID: {BusId} (with caching)", busId);

                try
                {
                    var result = await _cacheService.GetBusByIdAsync(busId, async (id) =>
                    {
                        _logger.LogInformation("Cache miss - retrieving bus entity with ID: {BusId} from database", id);
                        return await _context.Vehicles
                            .Include(v => v.AMRoutes)
                            .Include(v => v.PMRoutes)
                            .Include(v => v.Activities)
                            .Include(v => v.FuelRecords)
                            .Include(v => v.MaintenanceRecords)
                            .FirstOrDefaultAsync(v => v.VehicleId == id);
                    });

                    stopwatch.Stop();
                    if (result != null)
                    {
                        _logger.LogInformation("Retrieved bus entity {BusId} in {Duration}ms",
                            busId, stopwatch.ElapsedMilliseconds);
                    }
                    else
                    {
                        _logger.LogWarning("Bus entity {BusId} not found after {Duration}ms",
                            busId, stopwatch.ElapsedMilliseconds);
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    _logger.LogError(ex, "Error retrieving bus entity {BusId} after {Duration}ms",
                        busId, stopwatch.ElapsedMilliseconds);
                    throw;
                }
            }
        }

        public async Task<Bus> AddBusEntityAsync(Bus bus)
        {
            using (LogContext.PushProperty("OperationType", "AddBusEntity"))
            using (LogContext.PushProperty("BusNumber", bus.BusNumber))
            {
                _logger.LogInformation("Adding new bus entity: {BusNumber}", bus.BusNumber);

                _context.Vehicles.Add(bus);
                await _context.SaveChangesWithLoggingAsync(_logger, "AddBus", "BusNumber", bus.BusNumber);
                _cacheService.InvalidateAllBusCache();

                _logger.LogInformation("Successfully added bus: {BusNumber} with ID {BusId}",
                    bus.BusNumber, bus.VehicleId);

                return bus;
            }
        }

        public async Task<bool> UpdateBusEntityAsync(Bus bus)
        {
            using (LogContext.PushProperty("OperationType", "UpdateBusEntity"))
            using (LogContext.PushProperty("BusId", bus.VehicleId))
            using (LogContext.PushProperty("BusNumber", bus.BusNumber))
            {
                _logger.LogInformation("Updating bus entity with ID: {BusId}, Number: {BusNumber}",
                    bus.VehicleId, bus.BusNumber);

                _context.Vehicles.Update(bus);
                var result = await _context.SaveChangesWithLoggingAsync(_logger, "UpdateBus", "BusId", bus.VehicleId);

                if (result > 0)
                {
                    _cacheService.InvalidateBusCache(bus.VehicleId);
                    _logger.LogInformation("Successfully updated bus with ID: {BusId}", bus.VehicleId);
                }
                else
                {
                    _logger.LogWarning("No changes detected when updating bus with ID: {BusId}", bus.VehicleId);
                }

                return result > 0;
            }
        }

        public async Task<bool> DeleteBusEntityAsync(int busId)
        {
            using (LogContext.PushProperty("OperationType", "DeleteBusEntity"))
            using (LogContext.PushProperty("BusId", busId))
            {
                _logger.LogInformation("Deleting bus entity with ID: {BusId}", busId);

                var bus = await _context.Vehicles.FindAsync(busId);
                if (bus != null)
                {
                    using (LogContext.PushProperty("BusNumber", bus.BusNumber))
                    {
                        _logger.LogInformation("Found bus to delete: {BusNumber} (ID: {BusId})",
                            bus.BusNumber, busId);

                        _context.Vehicles.Remove(bus);
                        var result = await _context.SaveChangesWithLoggingAsync(_logger, "DeleteBus", "BusId", busId);

                        if (result > 0)
                        {
                            _cacheService.InvalidateBusCache(busId);
                            _logger.LogInformation("Successfully deleted bus with ID: {BusId}", busId);
                        }
                        else
                        {
                            _logger.LogWarning("No changes detected when deleting bus with ID: {BusId}", busId);
                        }

                        return result > 0;
                    }
                }

                _logger.LogWarning("Bus with ID: {BusId} not found for deletion", busId);
                return false;
            }
        }

        public async Task<List<Driver>> GetAllDriversAsync()
        {
            using (LogContext.PushProperty("QueryType", "GetAllDrivers"))
            {
                return await _logger.TrackPerformanceAsync("GetAllDrivers", async () =>
                {
                    _logger.LogInformation("Retrieving all drivers from database");
                    return await _context.Drivers.ToListAsync();
                });
            }
        }

        public async Task<Driver?> GetDriverEntityByIdAsync(int driverId)
        {
            using (LogContext.PushProperty("QueryType", "GetDriverEntityById"))
            using (LogContext.PushProperty("DriverId", driverId))
            {
                return await _logger.TrackPerformanceAsync("GetDriverById", async () =>
                {
                    _logger.LogInformation("Retrieving driver entity with ID: {DriverId}", driverId);
                    return await _context.Drivers
                        .Include(d => d.AMRoutes)
                        .Include(d => d.PMRoutes)
                        .Include(d => d.Activities)
                        .FirstOrDefaultAsync(d => d.DriverId == driverId);
                }, "DriverId", driverId);
            }
        }

        public async Task<Driver> AddDriverEntityAsync(Driver driver)
        {
            _logger.LogInformation("Adding new driver entity: {DriverName}", driver.DriverName);
            _context.Drivers.Add(driver);
            await _context.SaveChangesAsync();
            return driver;
        }

        public async Task<bool> UpdateDriverEntityAsync(Driver driver)
        {
            _logger.LogInformation("Updating driver entity with ID: {DriverId}", driver.DriverId);
            _context.Drivers.Update(driver);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteDriverEntityAsync(int driverId)
        {
            _logger.LogInformation("Deleting driver entity with ID: {DriverId}", driverId);
            var driver = await _context.Drivers.FindAsync(driverId);
            if (driver != null)
            {
                _context.Drivers.Remove(driver);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            return false;
        }

        public async Task<List<Route>> GetAllRouteEntitiesAsync()
        {
            _logger.LogInformation("Retrieving all route entities from database");
            return await _context.Routes
                .Include(r => r.AMVehicle)
                .Include(r => r.PMVehicle)
                .Include(r => r.AMDriver)
                .Include(r => r.PMDriver)
                .ToListAsync();
        }

        public async Task<List<ActivityType>> GetActivitiesByDateAsync(DateTime date)
        {
            using (LogContext.PushProperty("QueryType", "GetActivitiesByDate"))
            using (LogContext.PushProperty("ActivityDate", date.ToString("yyyy-MM-dd")))
            {
                return await _logger.TrackPerformanceAsync("GetActivitiesByDate", async () =>
                {
                    _logger.LogInformation("Retrieving activities for date: {Date}", date.ToShortDateString());

                    return await _context.ExecuteWithLoggingAsync(_logger, "GetActivitiesByDate", async () =>
                    {
                        return await _context.Activities
                            .Include(a => a.Vehicle)
                            .Include(a => a.Driver)
                            .Include(a => a.Route)
                            .Where(a => a.ActivityDate.Date == date.Date)
                            .ToListAsync();
                    }, "ActivityDate", date.ToString("yyyy-MM-dd"));
                });
            }
        }

        // Legacy methods for backward compatibility
        public async Task<List<BusInfo>> GetAllBusesAsync()
        {
            using (LogContext.PushProperty("QueryType", "GetAllBuses"))
            using (LogContext.PushProperty("LegacyMethod", true))
            {
                return await _logger.TrackPerformanceAsync("GetAllBuses_Legacy", async () =>
                {
                    _logger.LogInformation("Retrieving all buses (legacy method - using cached entities)");

                    // Try to get from database first
                    try
                    {
                        var buses = await GetAllBusEntitiesAsync();
                        var result = buses.Select(b => new BusInfo
                        {
                            BusId = b.VehicleId,
                            BusNumber = b.BusNumber,
                            Model = $"{b.Make} {b.Model}",
                            Capacity = b.SeatingCapacity,
                            Status = b.Status,
                            LastMaintenance = b.DateLastInspection ?? DateTime.MinValue
                        }).ToList();

                        _logger.LogInformation("Retrieved {BusCount} buses from database entities", result.Count);
                        return result;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not retrieve from database, using sample data");
                        // If DB unavailable, return in-memory sample data (already logged above)
                        return new List<BusInfo>(_buses);
                    }
                });
            }
        }

        public async Task<BusInfo?> GetBusByIdAsync(int busId)
        {
            _logger.LogInformation("Retrieving bus with ID: {BusId}", busId);
            await Task.Delay(10); // Simulate async operation
            return _buses.FirstOrDefault(b => b.BusId == busId);
        }

        public async Task<bool> AddBusAsync(BusInfo bus)
        {
            _logger.LogInformation("Adding new bus: {BusNumber}", bus.BusNumber);
            await Task.Delay(10); // Simulate async operation

            bus.BusId = _buses.Count > 0 ? _buses.Max(b => b.BusId) + 1 : 1;
            _buses.Add(bus);
            return true;
        }

        public async Task<bool> UpdateBusAsync(BusInfo bus)
        {
            _logger.LogInformation("Updating bus with ID: {BusId}", bus.BusId);
            await Task.Delay(10); // Simulate async operation

            var existingBus = _buses.FirstOrDefault(b => b.BusId == bus.BusId);
            if (existingBus != null)
            {
                var index = _buses.IndexOf(existingBus);
                _buses[index] = bus;
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteBusAsync(int busId)
        {
            _logger.LogInformation("Deleting bus with ID: {BusId}", busId);
            await Task.Delay(10); // Simulate async operation

            var bus = _buses.FirstOrDefault(b => b.BusId == busId);
            if (bus != null)
            {
                _buses.Remove(bus);
                return true;
            }
            return false;
        }

        public async Task<List<RouteInfo>> GetAllRoutesAsync()
        {
            _logger.LogInformation("Retrieving all routes (legacy method)");
            await Task.Delay(10); // Simulate async operation
            return new List<RouteInfo>(_routes);
        }

        public async Task<List<ScheduleInfo>> GetSchedulesByRouteAsync(int routeId)
        {
            _logger.LogInformation("Retrieving schedules for route ID: {RouteId}", routeId);
            await Task.Delay(10); // Simulate async operation
            return _schedules.Where(s => s.RouteId == routeId).ToList();
        }

        private void InitializeSampleData()
        {
            // Sample buses
            _buses.AddRange(new[]
            {
                new BusInfo { BusId = 1, BusNumber = "001", Model = "Blue Bird Vision", Capacity = 72, Status = "Active", LastMaintenance = DateTime.Now.AddDays(-30) },
                new BusInfo { BusId = 2, BusNumber = "002", Model = "Thomas Saf-T-Liner C2", Capacity = 66, Status = "Active", LastMaintenance = DateTime.Now.AddDays(-15) },
                new BusInfo { BusId = 3, BusNumber = "003", Model = "IC Bus CE Series", Capacity = 90, Status = "Maintenance", LastMaintenance = DateTime.Now.AddDays(-60) }
            });

            // Sample routes
            _routes.AddRange(new[]
            {
                new RouteInfo { RouteId = 1, RouteName = "Elementary Route 1", StartLocation = "School", EndLocation = "Neighborhoods A-C", Distance = 12.5m, EstimatedDuration = TimeSpan.FromMinutes(45) },
                new RouteInfo { RouteId = 2, RouteName = "Middle School Express", StartLocation = "School", EndLocation = "Neighborhoods D-F", Distance = 8.5m, EstimatedDuration = TimeSpan.FromMinutes(30) },
                new RouteInfo { RouteId = 3, RouteName = "High School Route", StartLocation = "School", EndLocation = "Neighborhoods G-J", Distance = 15.0m, EstimatedDuration = TimeSpan.FromMinutes(50) }
            });

            // Sample schedules
            _schedules.AddRange(new[]
            {
                new ScheduleInfo { ScheduleId = 1, RouteId = 1, BusId = 1, DepartureTime = TimeSpan.FromHours(7), ArrivalTime = TimeSpan.FromHours(7.75), Status = "On Time" },
                new ScheduleInfo { ScheduleId = 2, RouteId = 2, BusId = 2, DepartureTime = TimeSpan.FromHours(7.5), ArrivalTime = TimeSpan.FromHours(8.0), Status = "On Time" },
                new ScheduleInfo { ScheduleId = 3, RouteId = 3, BusId = 3, DepartureTime = TimeSpan.FromHours(8), ArrivalTime = TimeSpan.FromHours(8.83), Status = "Delayed" }
            });
        }
    }
}
