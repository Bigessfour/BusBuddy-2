using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Serilog;

namespace BusBuddy.WPF.Utilities
{
    /// <summary>
    /// Utility for validating and diagnosing theme consistency issues
    /// Specifically designed to identify FluentDark theme violations and UI inconsistencies
    /// </summary>
    public static class ThemeValidationHelper
    {
        private static readonly ILogger _logger = Log.ForContext("SourceContext", "ThemeValidationHelper");

        /// <summary>
        /// Validates the current theme application and reports any inconsistencies
        /// Call this during debug to identify theme-related issues
        /// </summary>
        [Conditional("DEBUG")]
        public static void ValidateThemeConsistency()
        {
            try
            {
                Console.WriteLine("🎨 THEME VALIDATION: Starting comprehensive theme consistency check...");
                Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

                var issues = new List<string>();

                // Check resource dictionary availability
                ValidateResourceDictionaries(issues);

                // Check color scheme consistency
                ValidateColorScheme(issues);

                // Check Syncfusion theme application
                ValidateSyncfusionTheme(issues);

                // Check control styling
                ValidateControlStyling(issues);

                // Report results
                ReportValidationResults(issues);

                Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ THEME VALIDATION ERROR: {ex.Message}");
                _logger.Error(ex, "Theme validation failed");
            }
        }

        /// <summary>
        /// Validates that all required resource dictionaries are loaded
        /// </summary>
        private static void ValidateResourceDictionaries(List<string> issues)
        {
            try
            {
                Console.WriteLine("🔍 Checking Resource Dictionaries...");

                var app = Application.Current;
                if (app?.Resources == null)
                {
                    issues.Add("🚨 CRITICAL: Application resources not found");
                    return;
                }

                // Check for required theme resources
                var requiredResources = new[]
                {
                    "SurfaceBackground", "SurfaceForeground", "SurfaceBorderBrush",
                    "PrimaryForeground", "PrimaryBorder", "TextPrimary", "TextSecondary",
                    "AccentBackground", "AccentForeground"
                };

                foreach (var resource in requiredResources)
                {
                    if (!app.Resources.Contains(resource))
                    {
                        issues.Add($"⚠️ HIGH: Missing theme resource: {resource}");
                    }
                }

                Console.WriteLine($"✅ Resource Dictionary Check: {requiredResources.Length - issues.Count(i => i.Contains("Missing theme resource"))}/{requiredResources.Length} resources found");
            }
            catch (Exception ex)
            {
                issues.Add($"❌ Resource dictionary validation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates color scheme consistency for FluentDark theme
        /// </summary>
        private static void ValidateColorScheme(List<string> issues)
        {
            try
            {
                Console.WriteLine("🔍 Checking Color Scheme Consistency...");

                var app = Application.Current;
                if (app?.Resources == null) return;

                // Check FluentDark specific colors
                var fluentDarkColors = new Dictionary<string, Color>
                {
                    {"SurfaceDark", Color.FromRgb(0x1E, 0x1E, 0x1E)},
                    {"SurfaceMedium", Color.FromRgb(0x25, 0x25, 0x26)},
                    {"SurfaceLight", Color.FromRgb(0x2D, 0x2D, 0x30)},
                    {"TextPrimary", Color.FromRgb(0xFF, 0xFF, 0xFF)},
                    {"TextSecondary", Color.FromRgb(0xCC, 0xCC, 0xCC)}
                };

                foreach (var expectedColor in fluentDarkColors)
                {
                    if (app.Resources.Contains(expectedColor.Key))
                    {
                        var actualColor = app.Resources[expectedColor.Key];
                        if (actualColor is Color color && color != expectedColor.Value)
                        {
                            issues.Add($"🔶 MEDIUM: Color mismatch for {expectedColor.Key}: expected {expectedColor.Value}, got {color}");
                        }
                    }
                }

                Console.WriteLine($"✅ Color Scheme Check: {fluentDarkColors.Count - issues.Count(i => i.Contains("Color mismatch"))}/{fluentDarkColors.Count} colors correct");
            }
            catch (Exception ex)
            {
                issues.Add($"❌ Color scheme validation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates Syncfusion theme application
        /// </summary>
        private static void ValidateSyncfusionTheme(List<string> issues)
        {
            try
            {
                Console.WriteLine("🔍 Checking Syncfusion Theme Application...");

                // Check if Syncfusion theme is properly applied
                var syncfusionThemeApplied = CheckSyncfusionThemeApplication();
                if (!syncfusionThemeApplied)
                {
                    issues.Add("⚠️ HIGH: Syncfusion FluentDark theme not properly applied");
                }

                // Check for theme-specific Syncfusion resources
                var app = Application.Current;
                if (app?.Resources != null)
                {
                    var syncfusionResources = app.Resources.MergedDictionaries
                        .Where(rd => rd.Source?.ToString().Contains("Syncfusion") == true)
                        .ToList();

                    if (!syncfusionResources.Any())
                    {
                        issues.Add("⚠️ HIGH: No Syncfusion theme resources found in merged dictionaries");
                    }
                    else
                    {
                        Console.WriteLine($"✅ Found {syncfusionResources.Count} Syncfusion theme resource dictionaries");
                    }
                }

                Console.WriteLine("✅ Syncfusion Theme Check: Completed");
            }
            catch (Exception ex)
            {
                issues.Add($"❌ Syncfusion theme validation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates control styling consistency
        /// </summary>
        private static void ValidateControlStyling(List<string> issues)
        {
            try
            {
                Console.WriteLine("🔍 Checking Control Styling...");

                var app = Application.Current;
                if (app?.Resources == null) return;

                // Check for common control styles
                var expectedStyles = new[]
                {
                    "ProfessionalButtonStyle", "ProfessionalTextBoxStyle",
                    "ProfessionalComboBoxStyle", "ProfessionalDataGridStyle"
                };

                foreach (var style in expectedStyles)
                {
                    if (!app.Resources.Contains(style))
                    {
                        issues.Add($"🔶 MEDIUM: Missing control style: {style}");
                    }
                }

                Console.WriteLine($"✅ Control Styling Check: {expectedStyles.Length - issues.Count(i => i.Contains("Missing control style"))}/{expectedStyles.Length} styles found");
            }
            catch (Exception ex)
            {
                issues.Add($"❌ Control styling validation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if Syncfusion theme is properly applied
        /// </summary>
        private static bool CheckSyncfusionThemeApplication()
        {
            try
            {
                // This is a simplified check - in a real scenario, you'd check Syncfusion-specific properties
                var app = Application.Current;
                return app?.Resources?.MergedDictionaries?.Any(rd =>
                    rd.Source?.ToString().Contains("Syncfusion") == true) == true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Reports validation results to the console
        /// </summary>
        private static void ReportValidationResults(List<string> issues)
        {
            if (!issues.Any())
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ THEME VALIDATION COMPLETE: No issues found - theme is consistent!");
                Console.ResetColor();
                return;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠️ THEME VALIDATION COMPLETE: {issues.Count} issues found");
            Console.ResetColor();

            foreach (var issue in issues.OrderBy(i => i))
            {
                if (issue.Contains("🚨 CRITICAL"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else if (issue.Contains("⚠️ HIGH"))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                else if (issue.Contains("🔶 MEDIUM"))
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }

                Console.WriteLine($"  {issue}");
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"🎨 RECOMMENDATION: Address the {issues.Count} theme issues above to ensure UI consistency");
            Console.ResetColor();
        }

        /// <summary>
        /// Quick theme check - call this to get a summary of theme status
        /// </summary>
        [Conditional("DEBUG")]
        public static void QuickThemeCheck()
        {
            try
            {
                Console.WriteLine("🎨 QUICK THEME CHECK:");

                var app = Application.Current;
                if (app?.Resources == null)
                {
                    Console.WriteLine("❌ Application resources not available");
                    return;
                }

                var resourceCount = app.Resources.Count;
                var mergedDictionaries = app.Resources.MergedDictionaries?.Count ?? 0;
                var syncfusionResources = app.Resources.MergedDictionaries?
                    .Count(rd => rd.Source?.ToString().Contains("Syncfusion") == true) ?? 0;

                Console.WriteLine($"  📊 Resources: {resourceCount} direct, {mergedDictionaries} merged dictionaries");
                Console.WriteLine($"  🔧 Syncfusion: {syncfusionResources} theme resource dictionaries");
                Console.WriteLine($"  ✅ Status: {(syncfusionResources > 0 ? "Theme resources loaded" : "⚠️ No theme resources found")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Quick theme check failed: {ex.Message}");
            }
        }
    }
}
