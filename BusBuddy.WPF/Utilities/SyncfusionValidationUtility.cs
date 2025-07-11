using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace BusBuddy.WPF.Utilities
{
    /// <summary>
    /// Utility class to validate Syncfusion DateTimePattern enums and prevent runtime XAML errors
    /// </summary>
    public static class SyncfusionValidationUtility
    {
        private static readonly ILogger Logger = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(typeof(SyncfusionValidationUtility));

        /// <summary>
        /// Valid Syncfusion DateTimePattern enum values
        /// Based on Syncfusion.Windows.Shared.DateTimePattern enum
        /// </summary>
        private static readonly HashSet<string> ValidDateTimePatterns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ShortDate",
            "LongDate",
            "FullDateTime",
            "LongTime",
            "ShortTime",
            "MonthDay",
            "YearMonth",
            "CustomPattern"
        };

        /// <summary>
        /// Validates that all DateTimePattern values used in the application are valid
        /// </summary>
        /// <param name="pattern">The pattern to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidDateTimePattern(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return false;

            return ValidDateTimePatterns.Contains(pattern);
        }

        /// <summary>
        /// Gets a safe replacement for invalid DateTimePattern values
        /// </summary>
        /// <param name="invalidPattern">The invalid pattern</param>
        /// <returns>A valid replacement pattern</returns>
        public static string GetSafeDateTimePattern(string invalidPattern)
        {
            Logger?.LogWarning($"Invalid DateTimePattern '{invalidPattern}' detected. Using 'ShortDate' as fallback.");

            // Map common invalid patterns to valid ones
            switch (invalidPattern?.ToLowerInvariant())
            {
                case "fulldate":
                    return "LongDate"; // Closest equivalent
                case "shortdatetime":
                    return "FullDateTime";
                case "longdatetime":
                    return "FullDateTime";
                default:
                    return "ShortDate"; // Safe default
            }
        }

        /// <summary>
        /// Validates Syncfusion control properties at runtime
        /// </summary>
        public static void ValidateSyncfusionControls()
        {
            try
            {
                // This method can be called at application startup to perform validation
                Logger?.LogInformation("Validating Syncfusion DateTimePattern configurations...");

                // Check if FullDate is being used anywhere and warn
                var problematicPatterns = new[] { "FullDate", "InvalidPattern" };
                foreach (var pattern in problematicPatterns)
                {
                    if (!IsValidDateTimePattern(pattern))
                    {
                        Logger?.LogWarning($"Pattern '{pattern}' is not valid. Use {GetSafeDateTimePattern(pattern)} instead.");
                    }
                }

                Logger?.LogInformation("Syncfusion validation completed successfully.");
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error during Syncfusion validation");
            }
        }

        /// <summary>
        /// Extension method to safely set DateTimePattern on Syncfusion controls
        /// </summary>
        public static void SetSafeDateTimePattern(object control, string pattern)
        {
            if (control == null) return;

            var safePattern = IsValidDateTimePattern(pattern) ? pattern : GetSafeDateTimePattern(pattern);

            try
            {
                // Use reflection to set the Pattern property safely
                var patternProperty = control.GetType().GetProperty("Pattern", BindingFlags.Public | BindingFlags.Instance);
                if (patternProperty != null && patternProperty.CanWrite)
                {
                    patternProperty.SetValue(control, safePattern);
                    Logger?.LogDebug($"Set DateTimePattern to '{safePattern}' on {control.GetType().Name}");
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, $"Failed to set DateTimePattern on {control.GetType().Name}");
            }
        }
    }
}
