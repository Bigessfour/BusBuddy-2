using BusBuddy.Core.Data;
using BusBuddy.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace BusBuddy.Core.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly BusBuddyDbContext _db;
        public ActivityLogService(BusBuddyDbContext db)
        {
            _db = db;
        }

        public async Task LogAsync(string action, string user, string? details = null)
        {
            // Truncate details if they exceed the database column size (1000 chars)
            string? truncatedDetails = details;
            if (details != null && details.Length > 995)
            {
                truncatedDetails = details.Substring(0, 990) + "[...]";
            }

            var log = new ActivityLog
            {
                Timestamp = DateTime.Now,
                Action = action,
                User = user,
                Details = truncatedDetails
            };
            _db.ActivityLogs.Add(log);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<ActivityLog>> GetLogsAsync(int count = 100)
        {
            return await _db.ActivityLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(count)
                .ToListAsync();
        }
    }
}
