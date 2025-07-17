using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Serilog;
using Serilog.Context;

namespace BusBuddy.WPF.Utilities
{
    /// <summary>
    /// Utility class for filtering debug output and extracting actionable errors/warnings
    /// </summary>
    public class DebugOutputFilter
    {
        private static readonly ILogger _logger = Log.ForContext<DebugOutputFilter>();

        /// <summary>
        /// Filter categories for debug output analysis
        /// </summary>
        public enum FilterCategory
        {
            ActionableErrors,
            ActionableWarnings,
            CompilationErrors,
            RuntimeExceptions,
            XamlErrors,
            SyncfusionIssues,
            DatabaseErrors,
            NetworkErrors,
            PerformanceWarnings,
            SecurityWarnings,
            StartupErrors,
            NavigationErrors,
            SportsSchedulingErrors,
            UIThemeIssues,
            ResourceDictionaryErrors,
            StyleApplicationErrors,
            ColorSchemeProblems,
            FluentDarkThemeViolations,
            All
        }

        /// <summary>
        /// Represents a filtered debug entry with context
        /// </summary>
        public class FilteredDebugEntry
        {
            public string Timestamp { get; set; } = string.Empty;
            public FilterCategory Category { get; set; }
            public string Level { get; set; } = string.Empty;
            public string Source { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public string StackTrace { get; set; } = string.Empty;
            public string ActionableRecommendation { get; set; } = string.Empty;
            public string Context { get; set; } = string.Empty;
            public int Priority { get; set; } // 1 = Critical, 2 = High, 3 = Medium, 4 = Low
            public bool IsResolved { get; set; } = false;
        }

        /// <summary>
        /// Predefined filter patterns for different error types
        /// </summary>
        private static readonly Dictionary<FilterCategory, List<Regex>> FilterPatterns = new()
        {
            [FilterCategory.ActionableErrors] = new List<Regex>
            {
                new Regex(@"🚨 ACTIONABLE.*EXCEPTION", RegexOptions.IgnoreCase),
                new Regex(@"ERROR.*\[.*\].*", RegexOptions.IgnoreCase),
                new Regex(@"CRITICAL.*ERROR", RegexOptions.IgnoreCase),
                new Regex(@"FATAL.*", RegexOptions.IgnoreCase)
            },
            [FilterCategory.ActionableWarnings] = new List<Regex>
            {
                new Regex(@"WARNING.*actionable", RegexOptions.IgnoreCase),
                new Regex(@"⚠️.*", RegexOptions.IgnoreCase),
                new Regex(@"WARN.*\[.*\]", RegexOptions.IgnoreCase)
            },
            [FilterCategory.CompilationErrors] = new List<Regex>
            {
                new Regex(@"error CS\d+", RegexOptions.IgnoreCase),
                new Regex(@"Build FAILED", RegexOptions.IgnoreCase),
                new Regex(@"compilation error", RegexOptions.IgnoreCase)
            },
            [FilterCategory.RuntimeExceptions] = new List<Regex>
            {
                new Regex(@"Exception.*at.*line", RegexOptions.IgnoreCase),
                new Regex(@"System\..*Exception", RegexOptions.IgnoreCase),
                new Regex(@"Unhandled exception", RegexOptions.IgnoreCase)
            },
            [FilterCategory.XamlErrors] = new List<Regex>
            {
                new Regex(@"XamlParseException", RegexOptions.IgnoreCase),
                new Regex(@"XAML.*error", RegexOptions.IgnoreCase),
                new Regex(@"StaticResource.*not found", RegexOptions.IgnoreCase),
                new Regex(@"binding.*error", RegexOptions.IgnoreCase)
            },
            [FilterCategory.SyncfusionIssues] = new List<Regex>
            {
                new Regex(@"Syncfusion.*error", RegexOptions.IgnoreCase),
                new Regex(@"ButtonAdv.*TargetType", RegexOptions.IgnoreCase),
                new Regex(@"FluentDark.*theme", RegexOptions.IgnoreCase),
                new Regex(@"SfSkinManager", RegexOptions.IgnoreCase)
            },
            [FilterCategory.DatabaseErrors] = new List<Regex>
            {
                new Regex(@"Database.*connection", RegexOptions.IgnoreCase),
                new Regex(@"SQL.*error", RegexOptions.IgnoreCase),
                new Regex(@"DbContext.*disposed", RegexOptions.IgnoreCase),
                new Regex(@"Entity Framework", RegexOptions.IgnoreCase)
            },
            [FilterCategory.NetworkErrors] = new List<Regex>
            {
                new Regex(@"Network.*error", RegexOptions.IgnoreCase),
                new Regex(@"Connection.*timeout", RegexOptions.IgnoreCase),
                new Regex(@"HTTP.*error", RegexOptions.IgnoreCase)
            },
            [FilterCategory.PerformanceWarnings] = new List<Regex>
            {
                new Regex(@"Performance.*warning", RegexOptions.IgnoreCase),
                new Regex(@"slow.*operation", RegexOptions.IgnoreCase),
                new Regex(@"timeout.*exceeded", RegexOptions.IgnoreCase),
                new Regex(@"memory.*usage", RegexOptions.IgnoreCase)
            },
            [FilterCategory.SecurityWarnings] = new List<Regex>
            {
                new Regex(@"Security.*warning", RegexOptions.IgnoreCase),
                new Regex(@"sensitive.*data", RegexOptions.IgnoreCase),
                new Regex(@"authentication.*failed", RegexOptions.IgnoreCase)
            },
            [FilterCategory.StartupErrors] = new List<Regex>
            {
                new Regex(@"STARTUP.*error", RegexOptions.IgnoreCase),
                new Regex(@"initialization.*failed", RegexOptions.IgnoreCase),
                new Regex(@"startup.*exception", RegexOptions.IgnoreCase)
            },
            [FilterCategory.NavigationErrors] = new List<Regex>
            {
                new Regex(@"Navigation.*error", RegexOptions.IgnoreCase),
                new Regex(@"ViewModel.*not found", RegexOptions.IgnoreCase),
                new Regex(@"View.*binding.*error", RegexOptions.IgnoreCase)
            },
            [FilterCategory.SportsSchedulingErrors] = new List<Regex>
            {
                new Regex(@"Schedules Log.*error", RegexOptions.IgnoreCase),
                new Regex(@"ScheduleView.*error", RegexOptions.IgnoreCase),
                new Regex(@"Sports.*scheduling.*error", RegexOptions.IgnoreCase),
                new Regex(@"Schedule.*Category.*error", RegexOptions.IgnoreCase)
            },
            [FilterCategory.UIThemeIssues] = new List<Regex>
            {
                new Regex(@"theme.*not.*applied", RegexOptions.IgnoreCase),
                new Regex(@"FluentDark.*theme.*error", RegexOptions.IgnoreCase),
                new Regex(@"Office2019Colorful.*theme.*error", RegexOptions.IgnoreCase),
                new Regex(@"SfSkinManager.*error", RegexOptions.IgnoreCase),
                new Regex(@"ApplyStylesOnApplication.*error", RegexOptions.IgnoreCase),
                new Regex(@"theme.*resource.*not.*found", RegexOptions.IgnoreCase)
            },
            [FilterCategory.ResourceDictionaryErrors] = new List<Regex>
            {
                new Regex(@"ResourceDictionary.*error", RegexOptions.IgnoreCase),
                new Regex(@"DeferrableContent.*threw.*exception", RegexOptions.IgnoreCase),
                new Regex(@"Item has already been added.*Key in dictionary", RegexOptions.IgnoreCase),
                new Regex(@"resource.*not.*found", RegexOptions.IgnoreCase),
                new Regex(@"StaticResource.*not.*found", RegexOptions.IgnoreCase),
                new Regex(@"DynamicResource.*not.*found", RegexOptions.IgnoreCase)
            },
            [FilterCategory.StyleApplicationErrors] = new List<Regex>
            {
                new Regex(@"Style.*not.*applied", RegexOptions.IgnoreCase),
                new Regex(@"TargetType.*mismatch", RegexOptions.IgnoreCase),
                new Regex(@"Style.*resource.*not.*found", RegexOptions.IgnoreCase),
                new Regex(@"Setter.*Property.*not.*found", RegexOptions.IgnoreCase),
                new Regex(@"Template.*not.*found", RegexOptions.IgnoreCase)
            },
            [FilterCategory.ColorSchemeProblems] = new List<Regex>
            {
                new Regex(@"color.*scheme.*error", RegexOptions.IgnoreCase),
                new Regex(@"SolidColorBrush.*not.*found", RegexOptions.IgnoreCase),
                new Regex(@"Color.*resource.*not.*found", RegexOptions.IgnoreCase),
                new Regex(@"foreground.*background.*contrast", RegexOptions.IgnoreCase),
                new Regex(@"accessibility.*color.*contrast", RegexOptions.IgnoreCase)
            },
            [FilterCategory.FluentDarkThemeViolations] = new List<Regex>
            {
                new Regex(@"FluentDark.*violation", RegexOptions.IgnoreCase),
                new Regex(@"theme.*consistency.*error", RegexOptions.IgnoreCase),
                new Regex(@"Surface.*color.*incorrect", RegexOptions.IgnoreCase),
                new Regex(@"Primary.*color.*incorrect", RegexOptions.IgnoreCase),
                new Regex(@"Accent.*color.*incorrect", RegexOptions.IgnoreCase),
                new Regex(@"Text.*color.*incorrect", RegexOptions.IgnoreCase),
                new Regex(@"Border.*color.*incorrect", RegexOptions.IgnoreCase)
            }
        };

        /// <summary>
        /// Quick debug filter - call this in debug mode to get only actionable items
        /// Usage: var actionableItems = await DebugOutputFilter.GetActionableItemsAsync();
        /// </summary>
        public static async Task<string> GetActionableItemsAsync()
        {
            var actionableErrors = await FilterDebugOutputAsync(FilterCategory.ActionableErrors, true, true, 20);
            var actionableWarnings = await FilterDebugOutputAsync(FilterCategory.ActionableWarnings, true, true, 20);
            var runtimeExceptions = await FilterDebugOutputAsync(FilterCategory.RuntimeExceptions, true, true, 10);
            var xamlErrors = await FilterDebugOutputAsync(FilterCategory.XamlErrors, true, true, 10);
            var sportsSchedulingErrors = await FilterDebugOutputAsync(FilterCategory.SportsSchedulingErrors, true, true, 10);
            var uiThemeIssues = await FilterDebugOutputAsync(FilterCategory.UIThemeIssues, true, true, 15);
            var resourceDictionaryErrors = await FilterDebugOutputAsync(FilterCategory.ResourceDictionaryErrors, true, true, 15);
            var styleApplicationErrors = await FilterDebugOutputAsync(FilterCategory.StyleApplicationErrors, true, true, 15);
            var colorSchemeProblems = await FilterDebugOutputAsync(FilterCategory.ColorSchemeProblems, true, true, 15);
            var fluentDarkThemeViolations = await FilterDebugOutputAsync(FilterCategory.FluentDarkThemeViolations, true, true, 15);

            var allActionableItems = new List<FilteredDebugEntry>();
            allActionableItems.AddRange(actionableErrors);
            allActionableItems.AddRange(actionableWarnings);
            allActionableItems.AddRange(runtimeExceptions);
            allActionableItems.AddRange(xamlErrors);
            allActionableItems.AddRange(sportsSchedulingErrors);
            allActionableItems.AddRange(uiThemeIssues);
            allActionableItems.AddRange(resourceDictionaryErrors);
            allActionableItems.AddRange(styleApplicationErrors);
            allActionableItems.AddRange(colorSchemeProblems);
            allActionableItems.AddRange(fluentDarkThemeViolations);

            // Remove duplicates and sort by priority
            var uniqueItems = allActionableItems
                .GroupBy(x => x.Message)
                .Select(g => g.First())
                .OrderBy(x => x.Priority)
                .ThenByDescending(x => x.Timestamp)
                .Take(30)
                .ToList();

            if (!uniqueItems.Any())
            {
                return "✅ No actionable items found in debug output - application is running cleanly!";
            }

            var output = new System.Text.StringBuilder();
            output.AppendLine($"🔍 ACTIONABLE DEBUG ITEMS ({uniqueItems.Count} items found)");
            output.AppendLine($"Generated: {DateTime.Now:HH:mm:ss}");
            output.AppendLine(new string('=', 80));

            foreach (var item in uniqueItems)
            {
                var priorityIcon = item.Priority switch
                {
                    1 => "🚨 CRITICAL",
                    2 => "⚠️  HIGH",
                    3 => "🔶 MEDIUM",
                    _ => "ℹ️  LOW"
                };

                output.AppendLine($"{priorityIcon} [{item.Category}] {item.Level}");
                output.AppendLine($"📝 {item.Message}");

                if (!string.IsNullOrEmpty(item.ActionableRecommendation))
                {
                    output.AppendLine($"🎯 ACTION: {item.ActionableRecommendation}");
                }

                if (!string.IsNullOrEmpty(item.StackTrace))
                {
                    output.AppendLine($"📍 LOCATION: {item.StackTrace}");
                }

                output.AppendLine(new string('-', 40));
            }

            return output.ToString();
        }

        /// <summary>
        /// Quick console output for debugging - prints actionable items to console
        /// Usage: DebugOutputFilter.PrintActionableItems();
        /// </summary>
        public static void PrintActionableItems()
        {
            try
            {
                var task = GetActionableItemsAsync();
                var result = task.GetAwaiter().GetResult();

                Console.WriteLine(result);
                System.Diagnostics.Debug.WriteLine(result);

                // Also log to Serilog if available
                _logger.Information("Debug output filter results:\n{FilterResults}", result);
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error filtering debug output: {ex.Message}";
                Console.WriteLine(errorMsg);
                System.Diagnostics.Debug.WriteLine(errorMsg);
                _logger.Error(ex, "Error in PrintActionableItems");
            }
        }

        /// <summary>
        /// Real-time debug filter for current debug session
        /// Monitors debug output and shows only actionable items
        /// </summary>
        public static async Task<List<string>> GetCurrentSessionActionableItemsAsync()
        {
            var actionableItems = new List<string>();

            try
            {
                // Check recent log entries (last 5 minutes)
                var recentTime = DateTime.Now.AddMinutes(-5);
                var allEntries = await FilterDebugOutputAsync(FilterCategory.All, true, true, 50);

                var recentEntries = allEntries.Where(e =>
                {
                    if (DateTime.TryParse(e.Timestamp, out var timestamp))
                    {
                        return timestamp >= recentTime;
                    }
                    return true; // Include if we can't parse timestamp
                }).ToList();

                foreach (var entry in recentEntries.OrderBy(e => e.Priority))
                {
                    var priorityIcon = entry.Priority switch
                    {
                        1 => "🚨",
                        2 => "⚠️",
                        3 => "🔶",
                        _ => "ℹ️"
                    };

                    var actionableItem = $"{priorityIcon} [{entry.Category}] {entry.Message}";

                    if (!string.IsNullOrEmpty(entry.ActionableRecommendation))
                    {
                        actionableItem += $"\n   🎯 ACTION: {entry.ActionableRecommendation}";
                    }

                    actionableItems.Add(actionableItem);
                }
            }
            catch (Exception ex)
            {
                actionableItems.Add($"Error filtering current session: {ex.Message}");
            }

            return actionableItems;
        }

        /// <summary>
        /// Filters debug output from multiple sources (console, files, etc.)
        /// </summary>
        public static async Task<List<FilteredDebugEntry>> FilterDebugOutputAsync(
            FilterCategory category = FilterCategory.ActionableErrors,
            bool includeContext = true,
            bool includeRecommendations = true,
            int maxEntries = 100)
        {
            using (LogContext.PushProperty("LogCategory", "Debug Filter"))
            {
                _logger.Information("Starting debug output filtering for category: {Category}", category);

                var results = new List<FilteredDebugEntry>();

                try
                {
                    // Get logs directory
                    string solutionRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName
                                       ?? Directory.GetCurrentDirectory();
                    string logsDirectory = Path.Combine(solutionRoot, "logs");

                    if (!Directory.Exists(logsDirectory))
                    {
                        _logger.Warning("Logs directory not found: {LogsDirectory}", logsDirectory);
                        return results;
                    }

                    // Get all log files
                    var logFiles = Directory.GetFiles(logsDirectory, "*.log", SearchOption.TopDirectoryOnly)
                                            .OrderByDescending(f => File.GetLastWriteTime(f))
                                            .Take(5); // Only check recent log files

                    foreach (var logFile in logFiles)
                    {
                        var fileEntries = await FilterLogFileAsync(logFile, category, includeContext, includeRecommendations);
                        results.AddRange(fileEntries);
                    }

                    // Also check console output if available
                    var consoleEntries = await FilterConsoleOutputAsync(category, includeContext, includeRecommendations);
                    results.AddRange(consoleEntries);

                    // Sort by priority and timestamp
                    results = results.OrderBy(e => e.Priority)
                                   .ThenByDescending(e => e.Timestamp)
                                   .Take(maxEntries)
                                   .ToList();

                    _logger.Information("Debug output filtering completed. Found {Count} entries for category: {Category}",
                        results.Count, category);

                    return results;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error filtering debug output");
                    return results;
                }
            }
        }

        /// <summary>
        /// Filters a specific log file
        /// </summary>
        private static async Task<List<FilteredDebugEntry>> FilterLogFileAsync(
            string logFilePath,
            FilterCategory category,
            bool includeContext,
            bool includeRecommendations)
        {
            var entries = new List<FilteredDebugEntry>();

            try
            {
                if (!File.Exists(logFilePath))
                    return entries;

                var lines = await File.ReadAllLinesAsync(logFilePath);
                var patterns = GetPatternsForCategory(category);

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];

                    foreach (var pattern in patterns)
                    {
                        if (pattern.IsMatch(line))
                        {
                            var entry = ParseLogEntry(line, i, lines, includeContext, includeRecommendations);
                            entry.Category = category;
                            entry.Source = Path.GetFileName(logFilePath);
                            entries.Add(entry);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error filtering log file: {LogFile}", logFilePath);
            }

            return entries;
        }

        /// <summary>
        /// Filters console output (if available)
        /// </summary>
        private static async Task<List<FilteredDebugEntry>> FilterConsoleOutputAsync(
            FilterCategory category,
            bool includeContext,
            bool includeRecommendations)
        {
            var entries = new List<FilteredDebugEntry>();

            try
            {
                // This would capture console output if we have a way to access it
                // For now, we'll focus on log files
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error filtering console output");
            }

            return entries;
        }

        /// <summary>
        /// Parses a log entry and extracts actionable information
        /// </summary>
        private static FilteredDebugEntry ParseLogEntry(
            string line,
            int lineIndex,
            string[] allLines,
            bool includeContext,
            bool includeRecommendations)
        {
            var entry = new FilteredDebugEntry();

            // Extract timestamp
            var timestampMatch = Regex.Match(line, @"\[(.*?)\]");
            if (timestampMatch.Success)
            {
                entry.Timestamp = timestampMatch.Groups[1].Value;
            }

            // Extract level
            var levelMatch = Regex.Match(line, @"\[(ERROR|WARN|INFO|DEBUG|FATAL)\]", RegexOptions.IgnoreCase);
            if (levelMatch.Success)
            {
                entry.Level = levelMatch.Groups[1].Value.ToUpper();
            }

            // Extract message
            entry.Message = line;

            // Set priority based on keywords
            entry.Priority = GetPriorityFromMessage(line);

            // Extract context (surrounding lines)
            if (includeContext)
            {
                var contextLines = new List<string>();
                int start = Math.Max(0, lineIndex - 2);
                int end = Math.Min(allLines.Length - 1, lineIndex + 2);

                for (int i = start; i <= end; i++)
                {
                    contextLines.Add($"{i + 1}: {allLines[i]}");
                }

                entry.Context = string.Join("\n", contextLines);
            }

            // Extract actionable recommendation
            if (includeRecommendations)
            {
                entry.ActionableRecommendation = GetActionableRecommendationFromMessage(line);
            }

            // Extract stack trace if available
            if (lineIndex + 1 < allLines.Length && allLines[lineIndex + 1].Contains("at "))
            {
                entry.StackTrace = allLines[lineIndex + 1];
            }

            return entry;
        }

        /// <summary>
        /// Gets filter patterns for a specific category
        /// </summary>
        private static List<Regex> GetPatternsForCategory(FilterCategory category)
        {
            if (category == FilterCategory.All)
            {
                return FilterPatterns.Values.SelectMany(p => p).ToList();
            }

            return FilterPatterns.ContainsKey(category) ? FilterPatterns[category] : new List<Regex>();
        }

        /// <summary>
        /// Determines priority based on message content
        /// </summary>
        private static int GetPriorityFromMessage(string message)
        {
            if (message.Contains("CRITICAL") || message.Contains("FATAL"))
                return 1; // Critical

            if (message.Contains("ERROR") || message.Contains("🚨"))
                return 2; // High

            if (message.Contains("WARNING") || message.Contains("⚠️"))
                return 3; // Medium

            return 4; // Low
        }

        /// <summary>
        /// Extracts actionable recommendation from message
        /// </summary>
        private static string GetActionableRecommendationFromMessage(string message)
        {
            // Look for existing recommendations in the message
            var recommendationMatch = Regex.Match(message, @"ActionableRecommendation.*?:(.*?)(?:\||$)", RegexOptions.IgnoreCase);
            if (recommendationMatch.Success)
            {
                return recommendationMatch.Groups[1].Value.Trim();
            }

            // Generate recommendations based on message content
            if (message.Contains("XAML"))
                return "Check XAML syntax, resource references, and binding expressions";

            if (message.Contains("Syncfusion"))
                return "Verify Syncfusion license, theme settings, and control properties";

            if (message.Contains("Database") || message.Contains("SQL"))
                return "Check database connection, query syntax, and DbContext lifecycle";

            if (message.Contains("Navigation"))
                return "Verify ViewModel registration, view mapping, and navigation parameters";

            if (message.Contains("Sports") || message.Contains("Schedule"))
                return "Check sports scheduling data, category filters, and CRUD operations";

            return "Review stack trace and implement appropriate error handling";
        }

        /// <summary>
        /// Exports filtered results to a readable format
        /// </summary>
        public static async Task<string> ExportFilteredResultsAsync(
            List<FilteredDebugEntry> entries,
            string format = "text")
        {
            using (LogContext.PushProperty("LogCategory", "Debug Filter"))
            {
                try
                {
                    if (format.ToLower() == "markdown")
                    {
                        return await ExportToMarkdownAsync(entries);
                    }
                    else
                    {
                        return await ExportToTextAsync(entries);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error exporting filtered results");
                    return $"Error exporting results: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Exports results to markdown format
        /// </summary>
        private static async Task<string> ExportToMarkdownAsync(List<FilteredDebugEntry> entries)
        {
            var markdown = new System.Text.StringBuilder();

            markdown.AppendLine("# 🔍 Debug Output Analysis Report");
            markdown.AppendLine($"**Generated:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            markdown.AppendLine($"**Total Entries:** {entries.Count}");
            markdown.AppendLine();

            var groupedEntries = entries.GroupBy(e => e.Category);

            foreach (var group in groupedEntries)
            {
                markdown.AppendLine($"## {group.Key}");
                markdown.AppendLine();

                foreach (var entry in group.OrderBy(e => e.Priority))
                {
                    var priorityIcon = entry.Priority switch
                    {
                        1 => "🚨",
                        2 => "⚠️",
                        3 => "🔶",
                        _ => "ℹ️"
                    };

                    markdown.AppendLine($"### {priorityIcon} {entry.Level} - {entry.Timestamp}");
                    markdown.AppendLine($"**Source:** {entry.Source}");
                    markdown.AppendLine($"**Message:** {entry.Message}");

                    if (!string.IsNullOrEmpty(entry.ActionableRecommendation))
                    {
                        markdown.AppendLine($"**🎯 Action Required:** {entry.ActionableRecommendation}");
                    }

                    if (!string.IsNullOrEmpty(entry.StackTrace))
                    {
                        markdown.AppendLine($"**Stack Trace:** `{entry.StackTrace}`");
                    }

                    markdown.AppendLine();
                }
            }

            return await Task.FromResult(markdown.ToString());
        }

        /// <summary>
        /// Exports results to text format
        /// </summary>
        private static async Task<string> ExportToTextAsync(List<FilteredDebugEntry> entries)
        {
            var text = new System.Text.StringBuilder();

            text.AppendLine("=== DEBUG OUTPUT ANALYSIS REPORT ===");
            text.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            text.AppendLine($"Total Entries: {entries.Count}");
            text.AppendLine();

            foreach (var entry in entries.OrderBy(e => e.Priority))
            {
                text.AppendLine($"[{entry.Priority}] {entry.Level} - {entry.Timestamp}");
                text.AppendLine($"Source: {entry.Source}");
                text.AppendLine($"Category: {entry.Category}");
                text.AppendLine($"Message: {entry.Message}");

                if (!string.IsNullOrEmpty(entry.ActionableRecommendation))
                {
                    text.AppendLine($"Action Required: {entry.ActionableRecommendation}");
                }

                if (!string.IsNullOrEmpty(entry.StackTrace))
                {
                    text.AppendLine($"Stack Trace: {entry.StackTrace}");
                }

                text.AppendLine(new string('-', 80));
            }

            return await Task.FromResult(text.ToString());
        }

        /// <summary>
        /// Debug-friendly method to quickly show only what needs attention
        /// Add this to your debug code: var issues = DebugOutputFilter.ShowOnlyIssues();
        /// </summary>
        public static string ShowOnlyIssues()
        {
            try
            {
                var task = GetActionableItemsAsync();
                var result = task.GetAwaiter().GetResult();

                // Also write to debug output for immediate visibility
                System.Diagnostics.Debug.WriteLine("=== FILTERED DEBUG OUTPUT ===");
                System.Diagnostics.Debug.WriteLine(result);

                return result;
            }
            catch (Exception ex)
            {
                var error = $"Filter error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(error);
                return error;
            }
        }

        /// <summary>
        /// Quick method to check if there are any critical issues
        /// Returns true if critical issues found, false otherwise
        /// </summary>
        public static bool HasCriticalIssues()
        {
            try
            {
                var task = FilterDebugOutputAsync(FilterCategory.ActionableErrors, false, false, 5);
                var criticalErrors = task.GetAwaiter().GetResult();

                var hasCritical = criticalErrors.Any(e => e.Priority <= 2);

                if (hasCritical)
                {
                    System.Diagnostics.Debug.WriteLine("🚨 CRITICAL ISSUES DETECTED - Run DebugOutputFilter.ShowOnlyIssues() for details");
                }

                return hasCritical;
            }
            catch
            {
                return false;
            }
        }
    }
}
