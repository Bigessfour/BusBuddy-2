using BusBuddy.Core.Data;
using BusBuddy.Core.Logging;
using BusBuddy.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BusBuddy.Core.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly BusBuddyDbContext _db;
        private readonly ILogger<ActivityLogService>? _logger;

        public ActivityLogService(BusBuddyDbContext db, ILogger<ActivityLogService>? logger = null)
        {
            _db = db;
            _logger = logger;
        }

        public async Task LogAsync(string action, string user, string? details = null)
        {
#if DEBUG
            using var tracker = new ActivityLoggingPerformanceTracker(_logger ?? NullLogger<ActivityLogService>.Instance,
                "LogActivity", $"Action: {action}, User: {user}");
#endif

            try
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

#if DEBUG
                tracker?.Complete($"Added log entry with ID: {log.Id}");
#endif
            }
            catch (Exception ex)
            {
#if DEBUG
                tracker?.Error(ex);
#endif
                _logger?.LogError(ex, "Error logging activity: {Action} by {User}", action, user);
                throw;
            }
        }

        public async Task<IEnumerable<ActivityLog>> GetLogsAsync(int count = 100)
        {
#if DEBUG
            using var tracker = new ActivityLoggingPerformanceTracker(_logger ?? NullLogger<ActivityLogService>.Instance,
                "GetLogs", $"Count: {count}");
#endif

            try
            {
                var logs = await _db.ActivityLogs
                    .AsNoTracking() // Read-only query for better performance
                    .OrderByDescending(l => l.Timestamp)
                    .Take(count)
                    .ToListAsync();

#if DEBUG
                tracker?.Complete($"Retrieved {logs.Count} log entries");
#endif

                return logs;
            }
            catch (Exception ex)
            {
#if DEBUG
                tracker?.Error(ex);
#endif
                _logger?.LogError(ex, "Error retrieving activity logs");
                throw;
            }
        }

        /// <summary>
        /// Get activity logs with pagination support for better performance
        /// </summary>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of records per page (default: 50)</param>
        /// <returns>Paged activity logs</returns>
        public async Task<IEnumerable<ActivityLog>> GetLogsPagedAsync(int pageNumber = 1, int pageSize = 50)
        {
#if DEBUG
            using var tracker = new ActivityLoggingPerformanceTracker(_logger ?? NullLogger<ActivityLogService>.Instance,
                "GetLogsPaged", $"Page: {pageNumber}, Size: {pageSize}");
#endif

            try
            {
                var logs = await _db.ActivityLogs
                    .AsNoTracking() // Read-only query for better performance
                    .OrderByDescending(l => l.Timestamp)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

#if DEBUG
                tracker?.Complete($"Retrieved {logs.Count} log entries for page {pageNumber}");
#endif

                return logs;
            }
            catch (Exception ex)
            {
#if DEBUG
                tracker?.Error(ex);
#endif
                _logger?.LogError(ex, "Error retrieving paged activity logs");
                throw;
            }
        }

        public async Task<IEnumerable<ActivityLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate, int count = 1000)
        {
#if DEBUG
            using var tracker = new ActivityLoggingPerformanceTracker(_logger ?? NullLogger<ActivityLogService>.Instance,
                "GetLogsByDateRange", $"StartDate: {startDate}, EndDate: {endDate}, Count: {count}");
#endif

            try
            {
                var logs = await _db.ActivityLogs
                    .AsNoTracking() // Read-only query for better performance
                    .Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate)
                    .OrderByDescending(l => l.Timestamp)
                    .Take(count)
                    .ToListAsync();

#if DEBUG
                tracker?.Complete($"Retrieved {logs.Count} log entries");
#endif

                return logs;
            }
            catch (Exception ex)
            {
#if DEBUG
                tracker?.Error(ex);
#endif
                _logger?.LogError(ex, "Error retrieving activity logs by date range");
                throw;
            }
        }

        public async Task<IEnumerable<ActivityLog>> GetLogsByUserAsync(string user, int count = 100)
        {
#if DEBUG
            using var tracker = new ActivityLoggingPerformanceTracker(_logger ?? NullLogger<ActivityLogService>.Instance,
                "GetLogsByUser", $"User: {user}, Count: {count}");
#endif

            try
            {
                var logs = await _db.ActivityLogs
                    .AsNoTracking() // Read-only query for better performance
                    .Where(l => l.User == user)
                    .OrderByDescending(l => l.Timestamp)
                    .Take(count)
                    .ToListAsync();

#if DEBUG
                tracker?.Complete($"Retrieved {logs.Count} log entries");
#endif

                return logs;
            }
            catch (Exception ex)
            {
#if DEBUG
                tracker?.Error(ex);
#endif
                _logger?.LogError(ex, "Error retrieving activity logs by user");
                throw;
            }
        }

        public async Task<IEnumerable<ActivityLog>> GetLogsByActionAsync(string action, int count = 100)
        {
#if DEBUG
            using var tracker = new ActivityLoggingPerformanceTracker(_logger ?? NullLogger<ActivityLogService>.Instance,
                "GetLogsByAction", $"Action: {action}, Count: {count}");
#endif

            try
            {
                var logs = await _db.ActivityLogs
                    .AsNoTracking() // Read-only query for better performance
                    .Where(l => l.Action.Contains(action))
                    .OrderByDescending(l => l.Timestamp)
                    .Take(count)
                    .ToListAsync();

#if DEBUG
                tracker?.Complete($"Retrieved {logs.Count} log entries");
#endif

                return logs;
            }
            catch (Exception ex)
            {
#if DEBUG
                tracker?.Error(ex);
#endif
                _logger?.LogError(ex, "Error retrieving activity logs by action");
                throw;
            }
        }

        public async Task LogEntityActionAsync<T>(string action, string user, T entity, int? entityId = null) where T : class
        {
#if DEBUG
            using var tracker = new ActivityLoggingPerformanceTracker(_logger ?? NullLogger<ActivityLogService>.Instance,
                "LogEntityAction", $"Action: {action}, EntityType: {typeof(T).Name}");
#endif

            try
            {
                // Extract entity type name for the log
                string entityType = typeof(T).Name;

                // Try to get the entity ID through reflection if not provided
                int? id = entityId;
                if (id == null)
                {
                    var idProperty = typeof(T).GetProperty("Id");
                    if (idProperty != null)
                    {
                        id = idProperty.GetValue(entity) as int?;
                    }
                }

                // Create a structured details string with important entity info
                var details = new
                {
                    EntityType = entityType,
                    EntityId = id,
                    // Serialize at most 20 properties to avoid excessive detail
                    EntityData = SerializeEntityForLogging(entity)
                };

                // Format the action to include entity info
                string formattedAction = $"{action} {entityType}";
                if (id.HasValue)
                {
                    formattedAction += $" #{id}";
                }

                // Log with structured details
                await LogAsync(formattedAction, user, JsonSerializer.Serialize(details));

#if DEBUG
                tracker?.Complete($"Logged entity action with type {entityType} and ID {id}");
#endif
            }
            catch (Exception ex)
            {
#if DEBUG
                tracker?.Error(ex);
#endif
                _logger?.LogError(ex, "Error logging entity action for type {EntityType}", typeof(T).Name);

                // Fallback to simple logging if serialization fails
                try
                {
                    await LogAsync($"{action} {typeof(T).Name}", user,
                        $"Error creating detailed log: {ex.Message}");
                }
                catch (Exception fallbackEx)
                {
                    _logger?.LogError(fallbackEx, "Fallback logging also failed for entity action");
                    throw;
                }
            }
        }

        private string SerializeEntityForLogging<T>(T entity) where T : class
        {
#if DEBUG
            using var tracker = new ActivityLoggingPerformanceTracker(_logger ?? NullLogger<ActivityLogService>.Instance,
                "SerializeEntityForLogging", $"EntityType: {typeof(T).Name}");
#endif

            try
            {
                // Use a temporary dictionary to collect the properties we want to log
                var propertiesToLog = new Dictionary<string, object?>();

                // Get public properties (limit to reasonable number to avoid excessive details)
                var properties = typeof(T).GetProperties()
                    .Where(p => p.CanRead)
                    .Take(20);

                foreach (var prop in properties)
                {
                    // Skip complex navigation properties to avoid excessive serialization
                    if (prop.PropertyType.IsClass &&
                        prop.PropertyType != typeof(string) &&
                        !prop.PropertyType.IsPrimitive)
                    {
                        continue;
                    }

                    try
                    {
                        propertiesToLog[prop.Name] = prop.GetValue(entity);
                    }
                    catch
                    {
                        // Skip properties that can't be read
                        propertiesToLog[prop.Name] = "[error reading property]";
                    }
                }

                var result = JsonSerializer.Serialize(propertiesToLog);

#if DEBUG
                tracker?.Complete($"Serialized entity with {propertiesToLog.Count} properties");
#endif

                return result;
            }
            catch (Exception ex)
            {
#if DEBUG
                tracker?.Error(ex);
#endif
                _logger?.LogWarning(ex, "Error serializing entity of type {EntityType} for logging", typeof(T).Name);
                return "[Entity data unavailable]";
            }
        }
    }
}
