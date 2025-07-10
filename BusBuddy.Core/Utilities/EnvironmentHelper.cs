using System;

namespace BusBuddy.Core.Utilities
{
    /// <summary>
    /// Helper class for environment-related utilities
    /// </summary>
    public static class EnvironmentHelper
    {
        /// <summary>
        /// Checks if the application is running in development mode
        /// </summary>
        /// <returns>True if running in development environment, false otherwise</returns>
        public static bool IsDevelopment()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            return environment != null &&
                   environment.Equals("Development", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the current environment name
        /// </summary>
        /// <returns>The environment name (Development, Staging, Production) or "Production" if not set</returns>
        public static string GetEnvironmentName()
        {
            return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        }

        /// <summary>
        /// Checks if sensitive data logging should be enabled
        /// </summary>
        /// <returns>True if sensitive data logging should be enabled, false otherwise</returns>
        public static bool IsSensitiveDataLoggingEnabled()
        {
            // Only enable sensitive data logging in Development mode
            if (IsDevelopment())
            {
                // Additionally check for an override setting that can explicitly disable it
                var disableSensitiveLogging = Environment.GetEnvironmentVariable("DISABLE_SENSITIVE_DATA_LOGGING");
                return string.IsNullOrEmpty(disableSensitiveLogging) ||
                       !disableSensitiveLogging.Equals("true", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }
    }
}
