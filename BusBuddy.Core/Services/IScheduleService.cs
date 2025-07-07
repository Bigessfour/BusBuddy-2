using BusBuddy.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusBuddy.Core.Services
{
    public interface IScheduleService
    {
        Task<List<Activity>> GetAllSchedulesAsync();
        Task<Activity> GetScheduleByIdAsync(int id);
        Task<List<Activity>> GetSchedulesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Activity> AddScheduleAsync(Activity schedule);
        Task<Activity> UpdateScheduleAsync(Activity schedule);
        Task<bool> DeleteScheduleAsync(int id);
        Task<List<Activity>> GetSchedulesByVehicleAsync(string vehicleId);
        Task<List<Activity>> GetSchedulesByDriverAsync(string driverName);
        Task<bool> ValidateScheduleConflictAsync(Activity schedule);
    }
}
