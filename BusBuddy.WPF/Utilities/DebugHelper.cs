using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BusBuddy.WPF.Utilities
{
    /// <summary>
    /// Simple debug helper for quick access to filtered debug output
    /// Enhanced with real-time streaming and UI notification integration
    /// </summary>
    public static class DebugHelper
    {
        private static readonly ILogger _logger = Log.ForContext(typeof(DebugHelper));
        private static Timer? _autoFilterTimer;
        private static bool _isAutoFilterEnabled = false;
        private static int _scanCount = 0;
        private static bool _isStreamingEnabled = false;

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
                 /// Enhanced with real-time streaming and UI notifications
                 /// </summary>
        [Conditional("DEBUG")]
        public static void StartAutoFilter()
        {
            if (_isAutoFilterEnabled) return;

            _isAutoFilterEnabled = true;

            // Start real-time streaming
            StartRealTimeStreaming();

            // Start a timer that runs every 10 seconds
            _autoFilterTimer = new Timer(AutoFilterCallback, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10));

            Debug.WriteLine("🔍 AUTO DEBUG FILTER: Started - will show actionable items every 10 seconds");
            Console.WriteLine("🔍 AUTO DEBUG FILTER: Started - will show actionable items every 10 seconds");
            _logger.Information("Auto debug filter started with real-time streaming");
        }

        /// <summary>
        /// Starts real-time streaming integration
        /// </summary>
        [Conditional("DEBUG")]
        private static void StartRealTimeStreaming()
        {
            if (_isStreamingEnabled) return;

            try
            {
                // Subscribe to high priority issue events
                DebugOutputFilter.HighPriorityIssueDetected += OnHighPriorityIssueDetected;
                DebugOutputFilter.NewEntriesFiltered += OnNewEntriesFiltered;

                // Start the streaming
                DebugOutputFilter.StartRealTimeStreaming();
                _isStreamingEnabled = true;

                Debug.WriteLine("🎯 REAL-TIME STREAMING: Started - will trigger UI notifications for priority 1-2 issues");
                Console.WriteLine("🎯 REAL-TIME STREAMING: Started - will trigger UI notifications for priority 1-2 issues");
                _logger.Information("Real-time streaming started with UI notification integration");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ STREAMING ERROR: {ex.Message}");
                Console.WriteLine($"❌ STREAMING ERROR: {ex.Message}");
                _logger.Error(ex, "Error starting real-time streaming");
            }
        }

        /// <summary>
        /// Event handler for high priority issues
        /// </summary>
        private static void OnHighPriorityIssueDetected(object? sender, DebugOutputFilter.FilteredDebugEntry issue)
        {
            try
            {
                var priorityIcon = issue.Priority switch
                {
                    1 => "🚨 CRITICAL",
                    2 => "⚠️ HIGH",
                    _ => "🔶 MEDIUM"
                };

                var notification = $"{priorityIcon} REAL-TIME ALERT: {issue.Message}";

                Debug.WriteLine($"🎯 {notification}");
                Console.ForegroundColor = issue.Priority == 1 ? ConsoleColor.Red : ConsoleColor.Yellow;
                Console.WriteLine($"🎯 {notification}");
                Console.ResetColor();

                if (!string.IsNullOrEmpty(issue.ActionableRecommendation))
                {
                    Console.WriteLine($"   🎯 ACTION: {issue.ActionableRecommendation}");
                }

                _logger.Warning("High priority issue detected in real-time: {Message} (Priority: {Priority})",
                    issue.Message, issue.Priority);

                // Trigger UI notification if possible
                TriggerUINotification(issue);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error handling high priority issue notification");
            }
        }

        /// <summary>
        /// Event handler for new filtered entries
        /// </summary>
        private static void OnNewEntriesFiltered(object? sender, System.Collections.Generic.List<DebugOutputFilter.FilteredDebugEntry> entries)
        {
            try
            {
                var highPriorityCount = entries.Count(e => e.Priority <= 2);
                if (highPriorityCount > 0)
                {
                    Debug.WriteLine($"🔍 STREAMING: {highPriorityCount} high priority issues detected in real-time");
                    Console.WriteLine($"🔍 STREAMING: {highPriorityCount} high priority issues detected in real-time");
                }

                _logger.Information("New filtered entries processed: {Count} entries, {HighPriorityCount} high priority",
                    entries.Count, highPriorityCount);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error handling new filtered entries");
            }
        }

        /// <summary>
        /// Triggers UI notification for critical issues
        /// </summary>
        private static void TriggerUINotification(DebugOutputFilter.FilteredDebugEntry issue)
        {
            try
            {
                // Only trigger UI notifications for critical issues (Priority 1)
                if (issue.Priority != 1) return;

                // Try to show in UI if running in WPF context
                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            // You can implement a toast notification or dialog here
                            Debug.WriteLine($"🚨 UI NOTIFICATION: {issue.Message}");

                            // For now, just log it - you can enhance this to show actual UI notifications
                            _logger.Warning("UI notification triggered for critical issue: {Message}", issue.Message);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Error showing UI notification");
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error triggering UI notification");
            }
        }

        /// <summary>
        /// Stops the automatic debug filtering and real-time streaming
        /// </summary>
        [Conditional("DEBUG")]
        public static void StopAutoFilter()
        {
            _autoFilterTimer?.Dispose();
            _autoFilterTimer = null;
            _isAutoFilterEnabled = false;

            // Stop real-time streaming
            if (_isStreamingEnabled)
            {
                DebugOutputFilter.StopRealTimeStreaming();
                DebugOutputFilter.HighPriorityIssueDetected -= OnHighPriorityIssueDetected;
                DebugOutputFilter.NewEntriesFiltered -= OnNewEntriesFiltered;
                _isStreamingEnabled = false;
            }

            Debug.WriteLine("🔍 AUTO DEBUG FILTER: Stopped");
            Console.WriteLine("🔍 AUTO DEBUG FILTER: Stopped");
            _logger.Information("Auto debug filter and real-time streaming stopped");
        }

        /// <summary>
        /// Exports current actionable items to JSON for VS Code integration
        /// </summary>
        public static async Task ExportToJson(string? filePath = null)
        {
#if DEBUG
            try
            {
                var result = await DebugOutputFilter.ExportActionableItemsToJsonAsync(filePath);

                if (result.Contains("Error"))
                {
                    Debug.WriteLine($"❌ JSON EXPORT ERROR: {result}");
                    Console.WriteLine($"❌ JSON EXPORT ERROR: {result}");
                }
                else
                {
                    Debug.WriteLine($"✅ JSON EXPORT: Successfully exported actionable items");
                    Console.WriteLine($"✅ JSON EXPORT: Successfully exported actionable items");
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        Console.WriteLine($"   📁 File: {filePath}");
                    }
                }

                _logger.Information("Actionable items exported to JSON");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ JSON EXPORT ERROR: {ex.Message}");
                Console.WriteLine($"❌ JSON EXPORT ERROR: {ex.Message}");
                _logger.Error(ex, "Error exporting actionable items to JSON");
            }
#endif
        }

        /// <summary>
        /// Gets recent streaming entries for analysis
        /// </summary>
        [Conditional("DEBUG")]
        public static void ShowRecentStreamingEntries(int maxEntries = 20)
        {
            try
            {
                var recentEntries = DebugOutputFilter.GetRecentStreamingEntries(maxEntries);

                if (!recentEntries.Any())
                {
                    Debug.WriteLine("✅ STREAMING: No recent entries found");
                    Console.WriteLine("✅ STREAMING: No recent entries found");
                    return;
                }

                Debug.WriteLine($"🎯 STREAMING: {recentEntries.Count} recent entries found");
                Console.WriteLine($"🎯 STREAMING: {recentEntries.Count} recent entries found");
                Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

                foreach (var entry in recentEntries.OrderBy(e => e.Priority))
                {
                    var priorityIcon = entry.Priority switch
                    {
                        1 => "🚨",
                        2 => "⚠️",
                        3 => "🔶",
                        _ => "ℹ️"
                    };

                    Console.WriteLine($"{priorityIcon} [{entry.Category}] {entry.Message}");

                    if (!string.IsNullOrEmpty(entry.ActionableRecommendation))
                    {
                        Console.WriteLine($"   🎯 ACTION: {entry.ActionableRecommendation}");
                    }

                    Console.WriteLine($"   📅 {entry.DetectedAt:HH:mm:ss}");
                    Console.WriteLine();
                }

                Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ STREAMING ERROR: {ex.Message}");
                Console.WriteLine($"❌ STREAMING ERROR: {ex.Message}");
                _logger.Error(ex, "Error showing recent streaming entries");
            }
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
