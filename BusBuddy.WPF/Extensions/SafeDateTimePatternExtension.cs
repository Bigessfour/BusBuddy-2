using System;
using System.Windows.Markup;
using BusBuddy.WPF.Utilities;

namespace BusBuddy.WPF.Utilities
{
    /// <summary>
    /// Custom markup extension to safely handle DateTimePattern values.
    /// This prevents FullDate enum parsing errors in Syncfusion controls.
    /// </summary>
    public class SafeDateTimePatternExtension : MarkupExtension
    {
        /// <summary>
        /// Gets or sets the pattern to be validated.
        /// </summary>
        public string Pattern { get; set; } = "ShortDate";

        /// <summary>
        /// Provides the safe date time pattern value.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>A safe date time pattern string.</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // Validate and return safe pattern
            var safePattern = SyncfusionValidationUtility.IsValidDateTimePattern(Pattern)
                ? Pattern
                : SyncfusionValidationUtility.GetSafeDateTimePattern(Pattern);

            return safePattern;
        }
    }
}
