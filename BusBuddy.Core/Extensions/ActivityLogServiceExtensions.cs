using BusBuddy.Core.Services;
using System;
using System.Threading.Tasks;

namespace BusBuddy.Core.Extensions
{
    /// <summary>
    /// Extension methods for IActivityLogService to handle common logging scenarios
    /// </summary>
    public static class ActivityLogServiceExtensions
    {
        /// <summary>
        /// Logs an exception with properly formatted and truncated details
        /// </summary>
        public static async Task LogExceptionAsync(this IActivityLogService logService,
            string action,
            string user,
            Exception exception)
        {
            // Create a well-structured exception detail that focuses on the most important information
            // while staying within database column size limits
            string details = FormatExceptionForLogging(exception);

            await logService.LogAsync(action, user, details);
        }

        /// <summary>
        /// Formats an exception for database logging with size constraints in mind
        /// </summary>
        private static string FormatExceptionForLogging(Exception exception)
        {
            // Max length is 990 to leave room for "[...]" if needed
            const int maxLength = 990;

            // Start with the most important information
            var formatted = $"Type: {exception.GetType().Name}\n" +
                            $"Message: {exception.Message}\n";

            // Add stack trace information if there's room (first 3 frames are usually most relevant)
            var stackTrace = exception.StackTrace;
            if (stackTrace != null)
            {
                var frames = stackTrace.Split('\n');
                var relevantFrames = frames.Take(Math.Min(3, frames.Length));
                formatted += $"Stack: {string.Join(" | ", relevantFrames)}\n";
            }

            // Add inner exception info if present
            if (exception.InnerException != null)
            {
                formatted += $"Inner: {exception.InnerException.GetType().Name}: {exception.InnerException.Message}";
            }

            // Truncate if necessary
            if (formatted.Length > maxLength)
            {
                return formatted.Substring(0, maxLength) + "[...]";
            }

            return formatted;
        }
    }
}
