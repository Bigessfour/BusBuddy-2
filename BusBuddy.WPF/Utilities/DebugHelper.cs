using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace BusBuddy.WPF.Utilities
{
    /// <summary>
    /// Simple debug helper for quick access to filtered debug output
    /// Use this during debug sessions to see only actionable items
    /// </summary>
    public static class DebugHelper
    {
        private static Timer? _autoFilterTimer;
        private static bool _isAutoFilterEnabled = false;
        private static int _scanCount = 0;

        /// <summary>
        /// Test method to verify auto-filter is working
        /// </summary>
        [Conditional("DEBUG")]
        public static void TestAutoFilter()
        {
            Console.WriteLine("🧪 TESTING AUTO DEBUG FILTER:");
            Console.WriteLine($"   - Auto filter enabled: {_isAutoFilterEnabled}");
            Console.WriteLine($"   - Timer active: {_autoFilterTimer != null}");

            if (_isAutoFilterEnabled)
            {
                Console.WriteLine("✅ Auto filter is running - you should see periodic scans");
            }
            else
            {
                Console.WriteLine("❌ Auto filter is not running - call StartAutoFilter() first");
            }

            // Force a manual scan
            Console.WriteLine("🔍 Running manual scan now...");
            ShowDetailedIssues();

            // Run theme validation
            Console.WriteLine("🎨 Running theme validation...");
            ThemeValidationHelper.QuickThemeCheck();
        }        /// <summary>
                 /// Automatically starts filtering debug output every 10 seconds during debug sessions
                 /// Call this once during application startup to enable auto-filtering
                 /// </summary>
        [Conditional("DEBUG")]
        public static void StartAutoFilter()
        {
            if (_isAutoFilterEnabled) return;

            _isAutoFilterEnabled = true;

            // Start a timer that runs every 10 seconds
            _autoFilterTimer = new Timer(AutoFilterCallback, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10));

            Debug.WriteLine("🔍 AUTO DEBUG FILTER: Started - will show actionable items every 10 seconds");
            Console.WriteLine("🔍 AUTO DEBUG FILTER: Started - will show actionable items every 10 seconds");
        }

        /// <summary>
        /// Stops the automatic debug filtering
        /// </summary>
        [Conditional("DEBUG")]
        public static void StopAutoFilter()
        {
            _autoFilterTimer?.Dispose();
            _autoFilterTimer = null;
            _isAutoFilterEnabled = false;

            Debug.WriteLine("🔍 AUTO DEBUG FILTER: Stopped");
            Console.WriteLine("🔍 AUTO DEBUG FILTER: Stopped");
        }

        /// <summary>
        /// Timer callback for automatic filtering
        /// </summary>
        private static void AutoFilterCallback(object? state)
        {
            try
            {
                _scanCount++;
                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                Debug.WriteLine($"🔍 AUTO FILTER SCAN #{_scanCount}: {timestamp}");
                Console.WriteLine($"🔍 AUTO FILTER SCAN #{_scanCount}: {timestamp}");
                Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

                // Run theme validation every 5 scans
                if (_scanCount % 5 == 0)
                {
                    Console.WriteLine("🎨 Running theme validation (every 5 scans)...");
                    ThemeValidationHelper.QuickThemeCheck();
                    Console.WriteLine();
                }

                // Check for critical issues first
                if (HasCriticalIssues())
                {
                    Debug.WriteLine("🚨 CRITICAL ISSUES DETECTED - AUTO FILTER RESULTS:");
                    Console.WriteLine("🚨 CRITICAL ISSUES DETECTED - AUTO FILTER RESULTS:");
                    ShowDetailedIssues();
                }
                else
                {
                    // Check for any actionable items with enhanced detail
                    var issues = DebugOutputFilter.ShowOnlyIssues();
                    if (issues.Contains("No actionable items found"))
                    {
                        Debug.WriteLine("✅ AUTO FILTER: No issues found - application running cleanly");
                        Console.WriteLine("✅ AUTO FILTER: No issues found - application running cleanly");
                        Console.WriteLine($"📊 STATUS: System healthy at {timestamp}");
                    }
                    else
                    {
                        Debug.WriteLine("⚠️ AUTO FILTER: Issues found - displaying filtered results:");
                        Console.WriteLine("⚠️ AUTO FILTER: Issues found - displaying filtered results:");
                        ShowDetailedIssues();
                    }
                }

                Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ AUTO FILTER ERROR: {ex.Message}");
                Console.WriteLine($"❌ AUTO FILTER ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// Shows detailed issues with enhanced formatting and context
        /// </summary>
        [Conditional("DEBUG")]
        private static void ShowDetailedIssues()
        {
            try
            {
                var actionableItems = DebugOutputFilter.GetActionableItemsAsync().Result;

                if (actionableItems.Contains("No actionable items found"))
                {
                    Console.WriteLine("✅ DETAILED SCAN: No actionable items found - application running cleanly!");
                    return;
                }

                // Split into lines and format with better structure
                var lines = actionableItems.Split('\n');
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // Enhanced formatting for different types of issues
                    if (line.Contains("🚨 CRITICAL"))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"  {line}");
                        Console.ResetColor();
                    }
                    else if (line.Contains("⚠️ HIGH"))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"  {line}");
                        Console.ResetColor();
                    }
                    else if (line.Contains("🔶 MEDIUM"))
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"  {line}");
                        Console.ResetColor();
                    }
                    else if (line.Contains("UI Theme") || line.Contains("Theme") || line.Contains("Color") || line.Contains("Style"))
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine($"  🎨 THEME: {line}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"  {line}");
                    }
                }

                // Add summary information
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"📋 SCAN COMPLETE: {DateTime.Now:HH:mm:ss} - Check output above for actionable items");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ DETAILED SCAN ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// Runs a comprehensive theme validation check
        /// Usage: DebugHelper.ValidateTheme();
        /// </summary>
        [Conditional("DEBUG")]
        public static void ValidateTheme()
        {
            try
            {
                Console.WriteLine("🎨 COMPREHENSIVE THEME VALIDATION:");
                Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

                ThemeValidationHelper.ValidateThemeConsistency();

                Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                Console.WriteLine("🎨 THEME VALIDATION COMPLETE - Check output above for details");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ THEME VALIDATION ERROR: {ex.Message}");
                Debug.WriteLine($"❌ THEME VALIDATION ERROR: {ex.Message}");
            }
        }

        /// <summary>
        /// Quick debug method - shows only actionable items
        /// Usage: DebugHelper.ShowIssues();
        /// </summary>
        [Conditional("DEBUG")]
        public static void ShowIssues()
        {
            try
            {
                var issues = DebugOutputFilter.ShowOnlyIssues();
                Console.WriteLine(issues);

                // Also show in debug output window
                Debug.WriteLine("🔍 FILTERED DEBUG OUTPUT:");
                Debug.WriteLine(issues);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Debug filter error: {ex.Message}");
            }
        }

        /// <summary>
        /// Quick check for critical issues
        /// Usage: if (DebugHelper.HasCriticalIssues()) { ... }
        /// </summary>
        public static bool HasCriticalIssues()
        {
#if DEBUG
            return DebugOutputFilter.HasCriticalIssues();
#else
            return false;
#endif
        }

        /// <summary>
        /// Show only errors and warnings, no info/debug messages
        /// Usage: DebugHelper.ShowErrorsOnly();
        /// </summary>
        [Conditional("DEBUG")]
        public static void ShowErrorsOnly()
        {
            try
            {
                var task = DebugOutputFilter.FilterDebugOutputAsync(
                    DebugOutputFilter.FilterCategory.ActionableErrors,
                    true,
                    true,
                    20);

                var errors = task.GetAwaiter().GetResult();

                if (!errors.Any())
                {
                    Debug.WriteLine("✅ No errors found in debug output");
                    return;
                }

                Debug.WriteLine($"🚨 ERRORS FOUND ({errors.Count} items):");
                foreach (var error in errors.OrderBy(e => e.Priority))
                {
                    var priorityIcon = error.Priority switch
                    {
                        1 => "🚨",
                        2 => "⚠️",
                        3 => "🔶",
                        _ => "ℹ️"
                    };

                    Debug.WriteLine($"{priorityIcon} {error.Message}");
                    if (!string.IsNullOrEmpty(error.ActionableRecommendation))
                    {
                        Debug.WriteLine($"   🎯 ACTION: {error.ActionableRecommendation}");
                    }
                    Debug.WriteLine(new string('-', 50));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error filtering debug output: {ex.Message}");
            }
        }

        /// <summary>
        /// Show only sports scheduling related issues
        /// Usage: DebugHelper.ShowSportsSchedulingIssues();
        /// </summary>
        [Conditional("DEBUG")]
        public static void ShowSportsSchedulingIssues()
        {
            try
            {
                var task = DebugOutputFilter.FilterDebugOutputAsync(
                    DebugOutputFilter.FilterCategory.SportsSchedulingErrors,
                    true,
                    true,
                    15);

                var issues = task.GetAwaiter().GetResult();

                if (!issues.Any())
                {
                    Debug.WriteLine("✅ No sports scheduling issues found");
                    return;
                }

                Debug.WriteLine($"🏐 SPORTS SCHEDULING ISSUES ({issues.Count} items):");
                foreach (var issue in issues.OrderBy(e => e.Priority))
                {
                    Debug.WriteLine($"⚠️ {issue.Message}");
                    if (!string.IsNullOrEmpty(issue.ActionableRecommendation))
                    {
                        Debug.WriteLine($"   🎯 ACTION: {issue.ActionableRecommendation}");
                    }
                    Debug.WriteLine(new string('-', 50));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error filtering sports scheduling issues: {ex.Message}");
            }
        }

        /// <summary>
        /// Show only XAML and UI related issues
        /// Usage: DebugHelper.ShowUIIssues();
        /// </summary>
        [Conditional("DEBUG")]
        public static void ShowUIIssues()
        {
            try
            {
                var xamlTask = DebugOutputFilter.FilterDebugOutputAsync(
                    DebugOutputFilter.FilterCategory.XamlErrors,
                    true,
                    true,
                    10);

                var syncfusionTask = DebugOutputFilter.FilterDebugOutputAsync(
                    DebugOutputFilter.FilterCategory.SyncfusionIssues,
                    true,
                    true,
                    10);

                var xamlIssues = xamlTask.GetAwaiter().GetResult();
                var syncfusionIssues = syncfusionTask.GetAwaiter().GetResult();

                var allUIIssues = xamlIssues.Concat(syncfusionIssues)
                    .OrderBy(i => i.Priority)
                    .ToList();

                if (!allUIIssues.Any())
                {
                    Debug.WriteLine("✅ No UI/XAML issues found");
                    return;
                }

                Debug.WriteLine($"🎨 UI/XAML ISSUES ({allUIIssues.Count} items):");
                foreach (var issue in allUIIssues)
                {
                    Debug.WriteLine($"⚠️ {issue.Message}");
                    if (!string.IsNullOrEmpty(issue.ActionableRecommendation))
                    {
                        Debug.WriteLine($"   🎯 ACTION: {issue.ActionableRecommendation}");
                    }
                    Debug.WriteLine(new string('-', 50));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error filtering UI issues: {ex.Message}");
            }
        }

        /// <summary>
        /// Simple method to check if everything is running smoothly
        /// Usage: DebugHelper.HealthCheck();
        /// </summary>
        [Conditional("DEBUG")]
        public static void HealthCheck()
        {
            try
            {
                var hasCritical = DebugOutputFilter.HasCriticalIssues();

                if (hasCritical)
                {
                    Debug.WriteLine("🚨 HEALTH CHECK: CRITICAL ISSUES DETECTED");
                    ShowIssues();
                }
                else
                {
                    Debug.WriteLine("✅ HEALTH CHECK: Application running smoothly");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Health check error: {ex.Message}");
            }
        }
    }
}
