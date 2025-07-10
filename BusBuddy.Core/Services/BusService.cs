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
        private readonly IBusBuddyDbContextFactory _contextFactory;
        private readonly IBusCachingService _cacheService;
        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        // Removed unused lists that were previously used for sample data fallback

        public BusService(
            ILogger<BusService> logger,
            IBusBuddyDbContextFactory contextFactory,
            IBusCachingService cacheService)
        {
            _logger = logger;
            _contextFactory = contextFactory;
            _cacheService = cacheService;

            // No sample data initialization - all data comes from the database
            // with proper error handling
        }

        // Entity Framework methods for actual database operations using caching
        public async Task<List<Bus>> GetAllBusEntitiesAsync()
        {
            await _semaphore.WaitAsync();
            try
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
                            // Create a fresh context to avoid concurrency issues
                            var context = _contextFactory.CreateDbContext();
                            try
                            {
                                return await context.Vehicles
                                    .AsNoTracking() // Use AsNoTracking for better performance in read operations
                                    .Include(v => v.AMRoutes)
                                    .Include(v => v.PMRoutes)
                                    .ToListAsync();
                            }
                            finally
                            {
                                // Properly dispose the context when done
                                await context.DisposeAsync();
                            }
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
                        throw; // Propagate exception to caller - no fallback to sample data
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<(List<Bus> Buses, int TotalCount)> GetBusesPaginatedAsync(int pageNumber, int pageSize, string? sortColumn = null, bool isAscending = true)
        {
            using (LogContext.PushProperty("QueryType", "GetBusesPaginated"))
            using (LogContext.PushProperty("OperationName", "DatabaseQuery"))
            using (LogContext.PushProperty("PageNumber", pageNumber))
            using (LogContext.PushProperty("PageSize", pageSize))
            using (LogContext.PushProperty("SortColumn", sortColumn ?? "BusNumber"))
            using (LogContext.PushProperty("SortDirection", isAscending ? "Ascending" : "Descending"))
            {
                var stopwatch = Stopwatch.StartNew();
                _logger.LogInformation("Retrieving paginated bus entities (page {PageNumber}, size {PageSize})", pageNumber, pageSize);

                try
                {
                    using var context = _contextFactory.CreateDbContext();

                    // Get total count for pagination
                    var totalCount = await context.Vehicles.CountAsync();

                    // Start with base query
                    var query = context.Vehicles.AsNoTracking();

                    // Apply sorting
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        // Apply ordering based on the column name
                        query = sortColumn.ToLower() switch
                        {
                            "busnumber" => isAscending
                                ? query.OrderBy(v => v.BusNumber)
                                : query.OrderByDescending(v => v.BusNumber),
                            "year" => isAscending
                                ? query.OrderBy(v => v.Year)
                                : query.OrderByDescending(v => v.Year),
                            "make" => isAscending
                                ? query.OrderBy(v => v.Make)
                                : query.OrderByDescending(v => v.Make),
                            "model" => isAscending
                                ? query.OrderBy(v => v.Model)
                                : query.OrderByDescending(v => v.Model),
                            "seatingcapacity" => isAscending
                                ? query.OrderBy(v => v.SeatingCapacity)
                                : query.OrderByDescending(v => v.SeatingCapacity),
                            "status" => isAscending
                                ? query.OrderBy(v => v.Status)
                                : query.OrderByDescending(v => v.Status),
                            "datelastinspection" => isAscending
                                ? query.OrderBy(v => v.DateLastInspection)
                                : query.OrderByDescending(v => v.DateLastInspection),
                            "currentodometer" => isAscending
                                ? query.OrderBy(v => v.CurrentOdometer)
                                : query.OrderByDescending(v => v.CurrentOdometer),
                            _ => isAscending
                                ? query.OrderBy(v => v.BusNumber)
                                : query.OrderByDescending(v => v.BusNumber), // Default sort by BusNumber
                        };
                    }
                    else
                    {
                        // Default sorting by BusNumber if no sort column specified
                        query = isAscending
                            ? query.OrderBy(v => v.BusNumber)
                            : query.OrderByDescending(v => v.BusNumber);
                    }

                    // Apply pagination using Skip/Take
                    var buses = await query
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    stopwatch.Stop();
                    _logger.LogInformation("Retrieved {BusCount} of {TotalCount} bus entities in {Duration}ms (page {PageNumber})",
                        buses.Count, totalCount, stopwatch.ElapsedMilliseconds, pageNumber);

                    return (buses, totalCount);
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    _logger.LogError(ex, "Error retrieving paginated bus entities after {Duration}ms",
                        stopwatch.ElapsedMilliseconds);
                    throw; // Propagate the exception to the caller
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
                        // Create a fresh context to avoid concurrency issues
                        var context = _contextFactory.CreateDbContext();
                        try
                        {
                            return await context.Vehicles
                                .AsNoTracking() // Use AsNoTracking for better performance in read operations
                                .Include(v => v.AMRoutes)
                                .Include(v => v.PMRoutes)
                                .Include(v => v.Activities)
                                .Include(v => v.FuelRecords)
                                .Include(v => v.MaintenanceRecords)
                                .FirstOrDefaultAsync(v => v.VehicleId == id);
                        }
                        finally
                        {
                            // Properly dispose the context when done
                            await context.DisposeAsync();
                        }
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
                    throw; // Propagate exception to caller - no fallback to sample data
                }
            }
        }

        public async Task<Bus> AddBusEntityAsync(Bus bus)
        {
            using (LogContext.PushProperty("OperationType", "AddBusEntity"))
            using (LogContext.PushProperty("BusNumber", bus.BusNumber))
            {
                _logger.LogInformation("Adding new bus entity: {BusNumber}", bus.BusNumber);

                using var context = _contextFactory.CreateWriteDbContext();
                context.Vehicles.Add(bus);
                await context.SaveChangesWithLoggingAsync(_logger, "AddBus", "BusNumber", bus.BusNumber);
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

                using var context = _contextFactory.CreateWriteDbContext();
                context.Vehicles.Update(bus);
                var result = await context.SaveChangesWithLoggingAsync(_logger, "UpdateBus", "BusId", bus.VehicleId);

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

                using var context = _contextFactory.CreateWriteDbContext();
                var bus = await context.Vehicles.FindAsync(busId);
                if (bus != null)
                {
                    using (LogContext.PushProperty("BusNumber", bus.BusNumber))
                    {
                        _logger.LogInformation("Found bus to delete: {BusNumber} (ID: {BusId})",
                            bus.BusNumber, busId);

                        context.Vehicles.Remove(bus);
                        var result = await context.SaveChangesWithLoggingAsync(_logger, "DeleteBus", "BusId", busId);

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
                    try
                    {
                        _logger.LogInformation("Retrieving all drivers from database using projection");
                        using var context = _contextFactory.CreateDbContext();

                        // Only select required fields instead of the whole entity
                        var drivers = await context.Drivers
                            .AsNoTracking()
                            .Select(d => new Driver
                            {
                                DriverId = d.DriverId,
                                DriverName = d.DriverName,
                                DriverPhone = d.DriverPhone,
                                DriverEmail = d.DriverEmail,
                                DriversLicenceType = d.DriversLicenceType,
                                Status = d.Status,
                                TrainingComplete = d.TrainingComplete,
                                LicenseExpiryDate = d.LicenseExpiryDate
                                // Add more fields as needed by the UI
                            })
                            .ToListAsync();

                        _logger.LogInformation("Retrieved {DriverCount} drivers from database using projection", drivers.Count);
                        return drivers;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to retrieve drivers from database");
                        throw; // Propagate exception to caller - no fallback to sample data
                    }
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
                    using var context = _contextFactory.CreateDbContext();
                    return await context.Drivers
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
            using var context = _contextFactory.CreateWriteDbContext();
            context.Drivers.Add(driver);
            await context.SaveChangesAsync();
            return driver;
        }

        public async Task<bool> UpdateDriverEntityAsync(Driver driver)
        {
            _logger.LogInformation("Updating driver entity with ID: {DriverId}", driver.DriverId);
            using var context = _contextFactory.CreateWriteDbContext();
            context.Drivers.Update(driver);
            var result = await context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteDriverEntityAsync(int driverId)
        {
            _logger.LogInformation("Deleting driver entity with ID: {DriverId}", driverId);
            using var context = _contextFactory.CreateWriteDbContext();
            var driver = await context.Drivers.FindAsync(driverId);
            if (driver != null)
            {
                context.Drivers.Remove(driver);
                var result = await context.SaveChangesAsync();
                return result > 0;
            }
            return false;
        }

        public async Task<List<Route>> GetAllRouteEntitiesAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all route entities from database using projection");
                using var context = _contextFactory.CreateDbContext();

                // Use projection to select only the fields needed
                var routes = await context.Routes
                    .AsNoTracking()
                    .Select(r => new Route
                    {
                        RouteId = r.RouteId,
                        RouteName = r.RouteName,
                        Date = r.Date,
                        IsActive = r.IsActive,
                        Description = r.Description,
                        Distance = r.Distance,
                        EstimatedDuration = r.EstimatedDuration,
                        StudentCount = r.StudentCount,
                        StopCount = r.StopCount,

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

                        // Include basic vehicle and driver information 
                        BusNumber = r.BusNumber,
                        DriverName = r.DriverName

                        // Relations are excluded in this projection for performance
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {RouteCount} route entities from database using projection", routes.Count);
                return routes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve route entities from database");
                throw; // Propagate exception to caller - no fallback to sample data
            }
        }

        public async Task<List<ActivityType>> GetActivitiesByDateAsync(DateTime date)
        {
            using (LogContext.PushProperty("QueryType", "GetActivitiesByDate"))
            using (LogContext.PushProperty("ActivityDate", date.ToString("yyyy-MM-dd")))
            {
                return await _logger.TrackPerformanceAsync("GetActivitiesByDate", async () =>
                {
                    _logger.LogInformation("Retrieving activities for date: {Date}", date.ToShortDateString());

                    using var context = _contextFactory.CreateDbContext();
                    return await context.Activities
                        .Include(a => a.Vehicle)
                        .Include(a => a.Driver)
                        .Include(a => a.Route)
                        .Where(a => a.ActivityDate.Date == date.Date)
                        .ToListAsync();
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
                    _logger.LogInformation("Retrieving all buses (legacy method - using projection)");

                    try
                    {
                        // Create a fresh context for this operation
                        var context = _contextFactory.CreateDbContext();
                        try
                        {
                            // Use projection to select only the fields we need
                            var result = await context.Vehicles
                                .AsNoTracking()
                                .Select(b => new BusInfo
                                {
                                    BusId = b.VehicleId,
                                    BusNumber = b.BusNumber,
                                    Model = b.Make + " " + b.Model,
                                    Capacity = b.SeatingCapacity,
                                    Status = b.Status,
                                    LastMaintenance = b.DateLastInspection ?? DateTime.MinValue
                                })
                                .ToListAsync();

                            _logger.LogInformation("Retrieved {BusCount} buses using projection", result.Count);
                            return result;
                        }
                        finally
                        {
                            // Properly dispose the context when done
                            await context.DisposeAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to retrieve buses from database");
                        throw; // Notify caller instead of using sample data
                    }
                });
            }
        }

        public async Task<BusInfo?> GetBusByIdAsync(int busId)
        {
            try
            {
                _logger.LogInformation("Retrieving bus with ID: {BusId}", busId);

                // Create a fresh context for this operation
                var context = _contextFactory.CreateDbContext();
                try
                {
                    // Use projection to select only the fields we need
                    var bus = await context.Vehicles
                        .AsNoTracking()
                        .Where(v => v.VehicleId == busId)
                        .Select(b => new BusInfo
                        {
                            BusId = b.VehicleId,
                            BusNumber = b.BusNumber,
                            Model = b.Make + " " + b.Model,
                            Capacity = b.SeatingCapacity,
                            Status = b.Status,
                            LastMaintenance = b.DateLastInspection ?? DateTime.MinValue
                        })
                        .FirstOrDefaultAsync();

                    if (bus == null)
                    {
                        _logger.LogWarning("Bus with ID: {BusId} not found", busId);
                    }

                    return bus;
                }
                finally
                {
                    // Properly dispose the context when done
                    await context.DisposeAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve bus with ID: {BusId}", busId);
                throw; // Propagate exception to caller - no fallback to sample data
            }
        }

        public Task<bool> AddBusAsync(BusInfo bus)
        {
            _logger.LogInformation("Adding new bus: {BusNumber} (legacy method - deprecated)", bus.BusNumber);

            // This method should be replaced with proper entity framework operations
            // Use entity-based methods instead of BusInfo DTOs
            throw new NotImplementedException(
                "AddBusAsync is deprecated. Use entity-based methods for database operations.");
        }

        public Task<bool> UpdateBusAsync(BusInfo bus)
        {
            _logger.LogInformation("Updating bus with ID: {BusId} (legacy method - deprecated)", bus.BusId);

            // This method should be replaced with proper entity framework operations
            // Use entity-based methods instead of BusInfo DTOs
            throw new NotImplementedException(
                "UpdateBusAsync is deprecated. Use entity-based methods for database operations.");
        }

        public Task<bool> DeleteBusAsync(int busId)
        {
            _logger.LogInformation("Deleting bus with ID: {BusId} (legacy method - deprecated)", busId);

            // This method should be replaced with proper entity framework operations
            // Use entity-based methods instead of BusInfo DTOs
            throw new NotImplementedException(
                "DeleteBusAsync is deprecated. Use entity-based methods for database operations.");
        }

        public Task<List<RouteInfo>> GetAllRoutesAsync()
        {
            _logger.LogInformation("Retrieving all routes (legacy method - deprecated)");

            // This method should be replaced with proper route service calls
            // For now, throw an exception to indicate this method should not be used
            throw new NotImplementedException(
                "GetAllRoutesAsync is deprecated. Use IRouteService.GetAllActiveRoutesAsync() instead.");
        }

        public Task<List<ScheduleInfo>> GetSchedulesByRouteAsync(int routeId)
        {
            _logger.LogInformation("Retrieving schedules for route ID: {RouteId} (legacy method - deprecated)", routeId);

            // This method should be replaced with proper schedule service calls
            // For now, throw an exception to indicate this method should not be used
            throw new NotImplementedException(
                "GetSchedulesByRouteAsync is deprecated. Use IScheduleService methods instead.");
        }

    }
}
