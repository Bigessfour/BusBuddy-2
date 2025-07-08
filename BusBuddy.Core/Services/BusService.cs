using BusBuddy.Core.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using BusBuddy.Core.Models;

namespace BusBuddy.Core.Services
{
    public class BusService : IBusService
    {
        private readonly ILogger<BusService> _logger;
        private readonly BusBuddyDbContext _context;
        private readonly List<BusInfo> _buses;
        private readonly List<RouteInfo> _routes;
        private readonly List<ScheduleInfo> _schedules;

        public BusService(ILogger<BusService> logger, BusBuddyDbContext context)
        {
            _logger = logger;
            _context = context;
            _buses = new List<BusInfo>();
            _routes = new List<RouteInfo>();
            _schedules = new List<ScheduleInfo>();

            // Initialize with sample data for backward compatibility
            InitializeSampleData();
        }

        // Entity Framework methods for actual database operations
        public async Task<List<Bus>> GetAllBusEntitiesAsync()
        {
            _logger.LogInformation("Retrieving all bus entities from database");
            return await _context.Vehicles
                .Include(v => v.AMRoutes)
                .Include(v => v.PMRoutes)
                .ToListAsync();
        }

        public async Task<Bus?> GetBusEntityByIdAsync(int busId)
        {
            _logger.LogInformation("Retrieving bus entity with ID: {BusId}", busId);
            return await _context.Vehicles
                .Include(v => v.AMRoutes)
                .Include(v => v.PMRoutes)
                .Include(v => v.Activities)
                .Include(v => v.FuelRecords)
                .Include(v => v.MaintenanceRecords)
                .FirstOrDefaultAsync(v => v.VehicleId == busId);
        }

        public async Task<Bus> AddBusEntityAsync(Bus bus)
        {
            _logger.LogInformation("Adding new bus entity: {BusNumber}", bus.BusNumber);
            _context.Vehicles.Add(bus);
            await _context.SaveChangesAsync();
            return bus;
        }

        public async Task<bool> UpdateBusEntityAsync(Bus bus)
        {
            _logger.LogInformation("Updating bus entity with ID: {BusId}", bus.VehicleId);
            _context.Vehicles.Update(bus);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteBusEntityAsync(int busId)
        {
            _logger.LogInformation("Deleting bus entity with ID: {BusId}", busId);
            var bus = await _context.Vehicles.FindAsync(busId);
            if (bus != null)
            {
                _context.Vehicles.Remove(bus);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            return false;
        }

        public async Task<List<Driver>> GetAllDriversAsync()
        {
            _logger.LogInformation("Retrieving all drivers from database");
            return await _context.Drivers.ToListAsync();
        }

        public async Task<Driver?> GetDriverEntityByIdAsync(int driverId)
        {
            _logger.LogInformation("Retrieving driver entity with ID: {DriverId}", driverId);
            return await _context.Drivers
                .Include(d => d.AMRoutes)
                .Include(d => d.PMRoutes)
                .Include(d => d.Activities)
                .FirstOrDefaultAsync(d => d.DriverId == driverId);
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

        public async Task<List<Activity>> GetActivitiesByDateAsync(DateTime date)
        {
            _logger.LogInformation("Retrieving activities for date: {Date}", date.ToShortDateString());
            return await _context.Activities
                .Include(a => a.Vehicle)
                .Include(a => a.Driver)
                .Include(a => a.Route)
                .Where(a => a.ActivityDate.Date == date.Date)
                .ToListAsync();
        }

        // Legacy methods for backward compatibility with existing interface
        public async Task<List<BusInfo>> GetAllBusesAsync()
        {
            _logger.LogInformation("Retrieving all buses (legacy method)");

            // Try to get from database first
            try
            {
                var buses = await GetAllBusEntitiesAsync();
                return buses.Select(b => new BusInfo
                {
                    BusId = b.VehicleId,
                    BusNumber = b.BusNumber,
                    Model = $"{b.Make} {b.Model}",
                    Capacity = b.SeatingCapacity,
                    Status = b.Status,
                    LastMaintenance = b.DateLastInspection ?? DateTime.MinValue
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not retrieve from database, using sample data");
                // If DB unavailable, return in-memory sample data (already logged above)
                return new List<BusInfo>(_buses);
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
