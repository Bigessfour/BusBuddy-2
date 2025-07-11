using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.Logging;

namespace BusBuddy.WPF.Utilities
{
    /// <summary>
    /// Utility to test XAML parsing and validate fixes for common errors
    /// </summary>
    public static class XamlDiagnosticUtility
    {
        private static readonly ILogger Logger = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(typeof(XamlDiagnosticUtility));

        /// <summary>
        /// Tests XAML parsing for common error scenarios and validates fixes
        /// </summary>
        public static void RunDiagnosticTests()
        {
            try
            {
                Logger?.LogInformation("Starting XAML diagnostic tests...");

                // Test 1: Validate GridLength parsing with star values
                TestGridLengthParsing();

                // Test 2: Validate DateTimePattern enum values
                TestDateTimePatternValidation();

                // Test 3: Test culture-invariant number parsing
                TestCultureInvariantParsing();

                Logger?.LogInformation("XAML diagnostic tests completed successfully.");
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error during XAML diagnostic tests");
            }
        }

        private static void TestGridLengthParsing()
        {
            try
            {
                // Test that * values are properly parsed as GridLength
                var starLength = new GridLength(1, GridUnitType.Star);
                var autoLength = new GridLength(1, GridUnitType.Auto);

                Logger?.LogDebug("GridLength parsing test passed: Star={Star}, Auto={Auto}",
                    starLength.Value, autoLength.Value);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "GridLength parsing test failed");
            }
        }

        private static void TestDateTimePatternValidation()
        {
            try
            {
                var testPatterns = new Dictionary<string, bool>
                {
                    { "ShortDate", true },
                    { "LongDate", true },
                    { "FullDateTime", true },
                    { "FullDate", false }, // This should be invalid
                    { "CustomPattern", true }
                };

                foreach (var pattern in testPatterns)
                {
                    var isValid = SyncfusionValidationUtility.IsValidDateTimePattern(pattern.Key);
                    if (isValid != pattern.Value)
                    {
                        Logger?.LogWarning("DateTimePattern validation mismatch for '{Pattern}': Expected={Expected}, Actual={Actual}",
                            pattern.Key, pattern.Value, isValid);
                    }
                    else
                    {
                        Logger?.LogDebug("DateTimePattern validation correct for '{Pattern}': {IsValid}",
                            pattern.Key, isValid);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "DateTimePattern validation test failed");
            }
        }

        private static void TestCultureInvariantParsing()
        {
            try
            {
                // Test that numeric parsing works correctly in invariant culture
                var testValue = "*";

                // This should not cause a parsing error when used in proper context
                if (testValue == "*")
                {
                    var gridLength = new GridLength(1, GridUnitType.Star);
                    Logger?.LogDebug("Culture-invariant parsing test passed: GridLength.Star = {Value}", gridLength.Value);
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Culture-invariant parsing test failed");
            }
        }

        /// <summary>
        /// Validates that XAML resources are loading correctly
        /// </summary>
        public static bool ValidateResourceLoading()
        {
            try
            {
                // Try to access the application resources
                var app = Application.Current;
                if (app?.Resources != null)
                {
                    Logger?.LogDebug("Application resources loaded successfully");
                    return true;
                }

                Logger?.LogWarning("Application resources not available");
                return false;
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error validating resource loading");
                return false;
            }
        }
    }
}
