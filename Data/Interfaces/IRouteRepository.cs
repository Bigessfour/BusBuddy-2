using Bus_Buddy.Models;

namespace Bus_Buddy.Data.Interfaces;

/// <summary>
/// Route-specific repository interface
/// Extends generic repository with route-specific operations
/// </summary>
public interface IRouteRepository : IRepository<Route>
{
    // Route-specific queries
    Task<IEnumerable<Route>> GetRoutesByDateAsync(DateTime date);
    Task<IEnumerable<Route>> GetRoutesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Route>> GetRoutesByNameAsync(string routeName);
    Task<IEnumerable<Route>> GetActiveRoutesAsync();
    Task<Route?> GetRouteByNameAndDateAsync(string routeName, DateTime date);

    // Additional async methods for compatibility
    Task UpdateAsync(Route route);
    Task<IEnumerable<Route>> GetAllRoutesAsync();
    Task<bool> DeleteRouteAsync(int routeId);
    Task<Route?> GetRouteByIdAsync(int routeId);

    // Vehicle and driver assignments
    Task<IEnumerable<Route>> GetRoutesByVehicleAsync(int vehicleId, DateTime? date = null);
    Task<IEnumerable<Route>> GetRoutesByDriverAsync(int driverId, DateTime? date = null);
    Task<IEnumerable<Route>> GetRoutesWithoutVehicleAssignmentAsync(DateTime date);
    Task<IEnumerable<Route>> GetRoutesWithoutDriverAssignmentAsync(DateTime date);

    // Mileage and statistics
    Task<decimal> GetTotalMileageByDateAsync(DateTime date);
    Task<decimal> GetTotalMileageByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<decimal> GetAverageRidershipByRouteAsync(string routeName, DateTime? startDate = null, DateTime? endDate = null);
    Task<Dictionary<string, decimal>> GetMileageByRouteNameAsync(DateTime startDate, DateTime endDate);
    Task<Dictionary<string, int>> GetRidershipByRouteNameAsync(DateTime startDate, DateTime endDate);

    // Schedule validation
    Task<bool> ValidateRouteScheduleAsync(DateTime date);
    Task<IEnumerable<string>> GetRouteValidationErrorsAsync(DateTime date);
    Task<IEnumerable<Route>> GetRoutesWithMileageIssuesAsync(DateTime? startDate = null, DateTime? endDate = null);

    // Reporting
    Task<IEnumerable<Route>> GetMostActiveRoutesAsync(DateTime startDate, DateTime endDate, int count = 10);
    Task<IEnumerable<Route>> GetLeastActiveRoutesAsync(DateTime startDate, DateTime endDate, int count = 10);
    Task<Dictionary<DateTime, int>> GetDailyRouteCountAsync(DateTime startDate, DateTime endDate);

    // Synchronous methods for Syncfusion data binding
    IEnumerable<Route> GetRoutesByDate(DateTime date);
    IEnumerable<Route> GetRoutesByName(string routeName);
    IEnumerable<Route> GetActiveRoutes();
    IEnumerable<Route> GetRoutesByVehicle(int vehicleId, DateTime? date = null);
    IEnumerable<Route> GetRoutesByDriver(int driverId, DateTime? date = null);
    decimal GetTotalMileageByDate(DateTime date);
}
