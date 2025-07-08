using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusBuddy.Core.Models;

namespace BusBuddy.Core.Services
{
    public interface IActivityLogService
    {
        Task LogAsync(string action, string user, string? details = null);
        Task<IEnumerable<ActivityLog>> GetLogsAsync(int count = 100);
    }
}
