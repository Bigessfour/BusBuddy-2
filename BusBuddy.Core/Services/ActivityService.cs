using BusBuddy.Core.Data;
using BusBuddy.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BusBuddy.Core.Services;

/// <summary>
/// Service for managing Activity/Schedule operations
/// </summary>
public class ActivityService : IActivityService
{
    private readonly BusBuddyDbContext _context;
    private readonly ILogger<ActivityService> _logger;

    public ActivityService(BusBuddyDbContext context, ILogger<ActivityService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Activity>> GetAllActivitiesAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all activities"); return await _context.Activities
            .Include(a => a.AssignedVehicle)
            .Include(a => a.Route)
            .Include(a => a.Driver)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activities");
            throw;
        }
    }

    public async Task<Activity?> GetActivityByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Retrieving activity with ID: {ActivityId}", id); return await _context.Activities
            .Include(a => a.AssignedVehicle)
            .Include(a => a.Route)
            .Include(a => a.Driver)
            .FirstOrDefaultAsync(a => a.ActivityId == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activity with ID: {ActivityId}", id);
            throw;
        }
    }

    public async Task<Activity> CreateActivityAsync(Activity activity)
    {
        try
        {
            _logger.LogInformation("Creating new activity for date: {ActivityDate}", activity.Date);

            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully created activity with ID: {ActivityId}", activity.ActivityId);
            return activity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating activity");
            throw;
        }
    }

    public async Task<Activity> UpdateActivityAsync(Activity activity)
    {
        try
        {
            _logger.LogInformation("Updating activity with ID: {ActivityId}", activity.ActivityId);

            _context.Activities.Update(activity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated activity with ID: {ActivityId}", activity.ActivityId);
            return activity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating activity with ID: {ActivityId}", activity.ActivityId);
            throw;
        }
    }

    public async Task<bool> DeleteActivityAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting activity with ID: {ActivityId}", id);

            var activity = await _context.Activities.FindAsync(id);
            if (activity == null)
            {
                _logger.LogWarning("Activity with ID {ActivityId} not found for deletion", id);
                return false;
            }

            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted activity with ID: {ActivityId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting activity with ID: {ActivityId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Activity>> GetActivitiesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            _logger.LogInformation("Retrieving activities between {StartDate} and {EndDate}", startDate, endDate);
            return await _context.Activities
                .Include(a => a.AssignedVehicle)
                .Include(a => a.Route)
                .Include(a => a.Driver)
                .Where(a => a.Date >= startDate && a.Date <= endDate)
                .OrderBy(a => a.Date)
                .ThenBy(a => a.LeaveTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activities for date range");
            throw;
        }
    }

    public async Task<IEnumerable<Activity>> GetActivitiesByRouteAsync(int routeId)
    {
        try
        {
            _logger.LogInformation("Retrieving activities for route ID: {RouteId}", routeId);
            return await _context.Activities
                .Include(a => a.AssignedVehicle)
                .Include(a => a.Route)
                .Include(a => a.Driver)
                .Where(a => a.RouteId == routeId)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activities for route ID: {RouteId}", routeId);
            throw;
        }
    }

    public async Task<IEnumerable<Activity>> GetActivitiesByDriverAsync(int driverId)
    {
        try
        {
            _logger.LogInformation("Retrieving activities for driver ID: {DriverId}", driverId);
            return await _context.Activities
                .Include(a => a.AssignedVehicle)
                .Include(a => a.Route)
                .Include(a => a.Driver)
                .Where(a => a.DriverId == driverId)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activities for driver ID: {DriverId}", driverId);
            throw;
        }
    }

    public async Task<IEnumerable<Activity>> GetActivitiesByVehicleAsync(int vehicleId)
    {
        try
        {
            _logger.LogInformation("Retrieving activities for vehicle ID: {VehicleId}", vehicleId);
            return await _context.Activities
                .Include(a => a.AssignedVehicle)
                .Include(a => a.Route)
                .Include(a => a.Driver)
                .Where(a => a.AssignedVehicleId == vehicleId)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activities for vehicle ID: {VehicleId}", vehicleId);
            throw;
        }
    }
}
