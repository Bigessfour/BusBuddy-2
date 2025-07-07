

using Microsoft.EntityFrameworkCore;
using BusBuddy.Core.Data.Interfaces;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services;

namespace BusBuddy.Core.Data.Repositories;
/// <summary>
/// Route-specific repository implementation
/// Extends generic repository with route-specific operations
/// </summary>
public class RouteRepository : Repository<Route>, IRouteRepository
{
    public RouteRepository(BusBuddyDbContext context, IUserContextService userContextService) : base(context, userContextService)
    {
    }

    #region Async Route-Specific Operations

    public async Task<IEnumerable<Route>> GetRoutesByDateAsync(DateTime date)
    {
        return await Query()
            .Where(r => r.Date.Date == date.Date)
            .OrderBy(r => r.RouteName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Route>> GetRoutesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await Query()
            .Where(r => r.Date >= startDate.Date && r.Date <= endDate.Date)
            .OrderBy(r => r.Date)
            .ThenBy(r => r.RouteName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Route>> GetRoutesByNameAsync(string routeName)
    {
        return await Query()
            .Where(r => r.RouteName.Contains(routeName))
            .OrderByDescending(r => r.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Route>> GetActiveRoutesAsync()
    {
        return await Query()
            .Where(r => r.IsActive)
            .OrderBy(r => r.RouteName)
            .ToListAsync();
    }

    public async Task<Route?> GetRouteByNameAndDateAsync(string routeName, DateTime date)
    {
        return await Query()
            .FirstOrDefaultAsync(r => r.RouteName == routeName && r.Date.Date == date.Date);
    }

    public async Task<IEnumerable<Route>> GetRoutesByVehicleAsync(int vehicleId, DateTime? date = null)
    {
        var query = Query()
            .Where(r => r.AMVehicleId == vehicleId || r.PMVehicleId == vehicleId);

        if (date.HasValue)
            query = query.Where(r => r.Date.Date == date.Value.Date);

        return await query
            .OrderByDescending(r => r.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Route>> GetRoutesByDriverAsync(int driverId, DateTime? date = null)
    {
        var query = Query()
            .Where(r => r.AMDriverId == driverId || r.PMDriverId == driverId);

        if (date.HasValue)
            query = query.Where(r => r.Date.Date == date.Value.Date);

        return await query
            .OrderByDescending(r => r.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Route>> GetRoutesWithoutVehicleAssignmentAsync(DateTime date)
    {
        return await Query()
            .Where(r => r.Date.Date == date.Date &&
                       r.IsActive &&
                       (r.AMVehicleId == null || r.PMVehicleId == null))
            .OrderBy(r => r.RouteName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Route>> GetRoutesWithoutDriverAssignmentAsync(DateTime date)
    {
        return await Query()
            .Where(r => r.Date.Date == date.Date &&
                       r.IsActive &&
                       (r.AMDriverId == null || r.PMDriverId == null))
            .OrderBy(r => r.RouteName)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalMileageByDateAsync(DateTime date)
    {
        var query = Query();
        if (query == null)
        {
            return 0; // Return 0 if context or Routes is null
        }
        return await query
            .Where(r => r.Date.Date == date.Date && r.IsActive)
            .SumAsync(r => (r.AMEndMiles - r.AMBeginMiles ?? 0) + (r.PMEndMiles - r.PMBeginMiles ?? 0));
    }

    public async Task<decimal> GetTotalMileageByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await Query()
            .Where(r => r.Date >= startDate.Date && r.Date <= endDate.Date && r.IsActive)
            .SumAsync(r => (r.AMEndMiles - r.AMBeginMiles ?? 0) + (r.PMEndMiles - r.PMBeginMiles ?? 0));
    }

    public async Task<decimal> GetAverageRidershipByRouteAsync(string routeName, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = Query().Where(r => r.RouteName == routeName && r.IsActive);

        if (startDate.HasValue)
            query = query.Where(r => r.Date >= startDate.Value.Date);

        if (endDate.HasValue)
            query = query.Where(r => r.Date <= endDate.Value.Date);

        var routes = await query.ToListAsync();
        if (!routes.Any()) return 0;

        var totalRiders = routes.Sum(r => (r.AMRiders ?? 0) + (r.PMRiders ?? 0));
        return routes.Count > 0 ? (decimal)totalRiders / routes.Count : 0;
    }

    public async Task<Dictionary<string, decimal>> GetMileageByRouteNameAsync(DateTime startDate, DateTime endDate)
    {
        var routes = await Query()
            .Where(r => r.Date >= startDate.Date && r.Date <= endDate.Date && r.IsActive)
            .ToListAsync();

        return routes
            .GroupBy(r => r.RouteName)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(r => (r.AMEndMiles - r.AMBeginMiles ?? 0) + (r.PMEndMiles - r.PMBeginMiles ?? 0))
            );
    }

    public async Task<Dictionary<string, int>> GetRidershipByRouteNameAsync(DateTime startDate, DateTime endDate)
    {
        var routes = await Query()
            .Where(r => r.Date >= startDate.Date && r.Date <= endDate.Date && r.IsActive)
            .ToListAsync();

        return routes
            .GroupBy(r => r.RouteName)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(r => (r.AMRiders ?? 0) + (r.PMRiders ?? 0))
            );
    }

    public async Task<bool> ValidateRouteScheduleAsync(DateTime date)
    {
        var errors = await GetRouteValidationErrorsAsync(date);
        return !errors.Any();
    }

    public async Task<IEnumerable<string>> GetRouteValidationErrorsAsync(DateTime date)
    {
        var errors = new List<string>();
        var routes = await GetRoutesByDateAsync(date);

        foreach (var route in routes.Where(r => r.IsActive))
        {
            if (!route.AMVehicleId.HasValue && !route.PMVehicleId.HasValue)
                errors.Add($"Route '{route.RouteName}' has no vehicle assignments");

            if (!route.AMDriverId.HasValue && !route.PMDriverId.HasValue)
                errors.Add($"Route '{route.RouteName}' has no driver assignments");

            if (route.AMBeginMiles.HasValue && route.AMEndMiles.HasValue && route.AMEndMiles < route.AMBeginMiles)
                errors.Add($"Route '{route.RouteName}' AM: End miles less than begin miles");

            if (route.PMBeginMiles.HasValue && route.PMEndMiles.HasValue && route.PMEndMiles < route.PMBeginMiles)
                errors.Add($"Route '{route.RouteName}' PM: End miles less than begin miles");
        }

        return errors;
    }

    public async Task<IEnumerable<Route>> GetRoutesWithMileageIssuesAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = Query().Where(r => r.IsActive);

        if (startDate.HasValue)
            query = query.Where(r => r.Date >= startDate.Value.Date);

        if (endDate.HasValue)
            query = query.Where(r => r.Date <= endDate.Value.Date);

        return await query
            .Where(r =>
                (r.AMBeginMiles.HasValue && r.AMEndMiles.HasValue && r.AMEndMiles < r.AMBeginMiles) ||
                (r.PMBeginMiles.HasValue && r.PMEndMiles.HasValue && r.PMEndMiles < r.PMBeginMiles))
            .OrderByDescending(r => r.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Route>> GetMostActiveRoutesAsync(DateTime startDate, DateTime endDate, int count = 10)
    {
        var routes = await Query()
            .Where(r => r.Date >= startDate.Date && r.Date <= endDate.Date && r.IsActive)
            .ToListAsync();

        return routes
            .GroupBy(r => r.RouteName)
            .OrderByDescending(g => g.Sum(r => (r.AMRiders ?? 0) + (r.PMRiders ?? 0)))
            .Take(count)
            .SelectMany(g => g)
            .OrderBy(r => r.RouteName)
            .ThenByDescending(r => r.Date);
    }

    public async Task<IEnumerable<Route>> GetLeastActiveRoutesAsync(DateTime startDate, DateTime endDate, int count = 10)
    {
        var routes = await Query()
            .Where(r => r.Date >= startDate.Date && r.Date <= endDate.Date && r.IsActive)
            .ToListAsync();

        return routes
            .GroupBy(r => r.RouteName)
            .OrderBy(g => g.Sum(r => (r.AMRiders ?? 0) + (r.PMRiders ?? 0)))
            .Take(count)
            .SelectMany(g => g)
            .OrderBy(r => r.RouteName)
            .ThenByDescending(r => r.Date);
    }

    public async Task<Dictionary<DateTime, int>> GetDailyRouteCountAsync(DateTime startDate, DateTime endDate)
    {
        var routes = await Query()
            .Where(r => r.Date >= startDate.Date && r.Date <= endDate.Date && r.IsActive)
            .ToListAsync();

        return routes
            .GroupBy(r => r.Date.Date)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    #endregion

    #region Synchronous Methods for Syncfusion Data Binding

    public IEnumerable<Route> GetRoutesByDate(DateTime date)
    {
        return Query()
            .Where(r => r.Date.Date == date.Date)
            .OrderBy(r => r.RouteName)
            .ToList();
    }

    public IEnumerable<Route> GetRoutesByName(string routeName)
    {
        return Query()
            .Where(r => r.RouteName.Contains(routeName))
            .OrderByDescending(r => r.Date)
            .ToList();
    }

    public IEnumerable<Route> GetActiveRoutes()
    {
        return Query()
            .Where(r => r.IsActive)
            .OrderBy(r => r.RouteName)
            .ToList();
    }

    public IEnumerable<Route> GetRoutesByVehicle(int vehicleId, DateTime? date = null)
    {
        var query = Query()
            .Where(r => r.AMVehicleId == vehicleId || r.PMVehicleId == vehicleId);

        if (date.HasValue)
            query = query.Where(r => r.Date.Date == date.Value.Date);

        return query
            .OrderByDescending(r => r.Date)
            .ToList();
    }

    public IEnumerable<Route> GetRoutesByDriver(int driverId, DateTime? date = null)
    {
        var query = Query()
            .Where(r => r.AMDriverId == driverId || r.PMDriverId == driverId);

        if (date.HasValue)
            query = query.Where(r => r.Date.Date == date.Value.Date);

        return query
            .OrderByDescending(r => r.Date)
            .ToList();
    }

    public decimal GetTotalMileageByDate(DateTime date)
    {
        var routes = Query()
            .Where(r => r.Date.Date == date.Date && r.IsActive)
            .ToList();

        return routes.Sum(r =>
            (r.AMEndMiles - r.AMBeginMiles ?? 0) +
            (r.PMEndMiles - r.PMBeginMiles ?? 0));
    }

    #endregion

    #region Additional Async Methods for Compatibility

    public async Task UpdateAsync(Route route)
    {
        Update(route);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Route>> GetAllRoutesAsync()
    {
        return await GetAllAsync();
    }

    public async Task<bool> DeleteRouteAsync(int routeId)
    {
        return await RemoveByIdAsync(routeId);
    }

    public async Task<Route?> GetRouteByIdAsync(int routeId)
    {
        return await GetByIdAsync(routeId);
    }

    #endregion
}
