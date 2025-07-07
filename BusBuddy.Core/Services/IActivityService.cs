using BusBuddy.Core.Models;

namespace BusBuddy.Core.Services;

/// <summary>
/// Interface for Activity/Schedule management service
/// </summary>
public interface IActivityService
{
    Task<IEnumerable<Activity>> GetAllActivitiesAsync();
    Task<Activity?> GetActivityByIdAsync(int id);
    Task<Activity> CreateActivityAsync(Activity activity);
    Task<Activity> UpdateActivityAsync(Activity activity);
    Task<bool> DeleteActivityAsync(int id);
    Task<IEnumerable<Activity>> GetActivitiesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Activity>> GetActivitiesByRouteAsync(int routeId);
    Task<IEnumerable<Activity>> GetActivitiesByDriverAsync(int driverId);
    Task<IEnumerable<Activity>> GetActivitiesByVehicleAsync(int vehicleId);
}
