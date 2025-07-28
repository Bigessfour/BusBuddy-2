using BusBuddy.Core.Models;

namespace BusBuddy.Core.Services
{
    public interface IRouteService
    {
        Task<IEnumerable<Route>> GetAllActiveRoutesAsync();
        Task<Route> GetRouteByIdAsync(int id);
        Task<Route> CreateRouteAsync(Route route);
        Task<Route> UpdateRouteAsync(Route route);
        Task<bool> DeleteRouteAsync(int id);
        Task<IEnumerable<Route>> SearchRoutesAsync(string searchTerm);
        Task<IEnumerable<Route>> GetRoutesByBusIdAsync(int busId);
        Task<bool> IsRouteNumberUniqueAsync(string routeNumber, int? excludeId = null);
        Task<IEnumerable<RouteStop>> GetRouteStopsAsync(int routeId);
        Task<RouteStop> AddRouteStopAsync(RouteStop routeStop);
        Task<RouteStop> UpdateRouteStopAsync(RouteStop routeStop);
        Task<bool> DeleteRouteStopAsync(int routeStopId);
        Task<decimal> GetRouteTotalDistanceAsync(int routeId);
        Task<TimeSpan> GetRouteEstimatedTimeAsync(int routeId);
    }
}
