using BusBuddy.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusBuddy.Core.Models
{
    public interface IActivityService
    {
        Task<IEnumerable<Activity>> GetActivitiesByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
