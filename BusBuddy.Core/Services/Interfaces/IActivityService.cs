namespace BusBuddy.Core.Models
{
    public interface IActivityService
    {
        Task<IEnumerable<Activity>> GetActivitiesByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
