using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusBuddy.Core.Models;

namespace BusBuddy.Core.Services
{
    public interface IBusService
    {
        // Legacy methods for backward compatibility
        Task<List<BusInfo>> GetAllBusesAsync();
        Task<BusInfo?> GetBusByIdAsync(int busId);
        Task<bool> AddBusAsync(BusInfo bus);
        Task<bool> UpdateBusAsync(BusInfo bus);
        Task<bool> DeleteBusAsync(int busId);
        Task<List<RouteInfo>> GetAllRoutesAsync();
        Task<List<ScheduleInfo>> GetSchedulesByRouteAsync(int routeId);

        // Entity Framework methods for direct database operations
        Task<List<Bus>> GetAllBusEntitiesAsync();
        Task<Bus?> GetBusEntityByIdAsync(int busId);
        Task<Bus> AddBusEntityAsync(Bus bus);
        Task<bool> UpdateBusEntityAsync(Bus bus);
        Task<bool> DeleteBusEntityAsync(int busId);
        Task<List<Driver>> GetAllDriversAsync();
        Task<Driver?> GetDriverEntityByIdAsync(int driverId);
        Task<Driver> AddDriverEntityAsync(Driver driver);
        Task<bool> UpdateDriverEntityAsync(Driver driver);
        Task<bool> DeleteDriverEntityAsync(int driverId);
        Task<List<Route>> GetAllRouteEntitiesAsync();
    }

    public class BusInfo
    {
        public int BusId { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime LastMaintenance { get; set; }
    }

    public class RouteInfo
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public string StartLocation { get; set; } = string.Empty;
        public string EndLocation { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
    }

    public class ScheduleInfo
    {
        public int ScheduleId { get; set; }
        public int RouteId { get; set; }
        public int BusId { get; set; }
        public TimeSpan DepartureTime { get; set; }
        public TimeSpan ArrivalTime { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
