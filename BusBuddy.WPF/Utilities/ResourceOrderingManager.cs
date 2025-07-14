// Obsolete: All resource ordering is now handled by SfSkinManager/NuGet/XAML theme references only

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.Logging;
#nullable disable
#pragma warning disable CS8625, CS8602

namespace BusBuddy.WPF.Utilities
{
    /// <summary>
    /// Helper utility that works alongside Syncfusion.SfSkinManager to ensure proper resource dictionary ordering.
    /// This class does not replace or compete with SfSkinManager - it enhances it by ensuring resource dictionaries
    /// are in the correct order for SfSkinManager to apply themes correctly.
    /// </summary>
    public static class ResourceOrderingManager
    {
        private static readonly Dictionary<string, int> _resourcePriorities = new Dictionary<string, int>
        {
            // Higher numbers = higher priority (loaded later to override earlier resources)
            // IMPORTANT: SfSkinManager themes must have highest priority
            { "Syncfusion.Themes.FluentDark.WPF", 110 },
            { "Syncfusion.Themes.FluentLight.WPF", 109 },
            { "Syncfusion.SfSkinManager", 105 },
            { "FluentDarkTheme", 100 },
            { "SyncfusionControls", 90 },
            { "ApplicationStyles", 80 },
            { "CustomControls", 70 },
            { "DefaultStyles", 60 }
        };

        private static bool _safeModeEnabled = false;

        /// <summary>
        /// Enables safe mode for resource loading. This should be called when resource conflicts
        /// or XamlParseExceptions occur. Safe mode ensures SfSkinManager resources take priority.
        /// </summary>
        public static void EnableSafeMode()
        {
            _safeModeEnabled = true;

            // When in safe mode, attempt to reorder resources immediately
            try
            {
                if (Application.Current != null)
                {
                    EnsureResourceOrder(Application.Current);
                }
            }
            catch
            {
                // Ignore exceptions in safe mode - we're already handling an error
            }
        }

        /// <summary>
        /// Ensures resources are ordered correctly to complement SfSkinManager theme application.
        /// This method helps SfSkinManager by making sure theme resources are properly ordered.
        /// </summary>
        /// <param name="application">The application whose resources need ordering</param>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public static void EnsureResourceOrder(Application application, ILogger logger = null)
        {
            try
            {
                if (application?.Resources == null)
                {
                    logger?.LogWarning("Cannot order resources: Application or Resources is null");
                    return;
                }

                // Get merged dictionaries
                var mergedDictionaries = application.Resources.MergedDictionaries;
                if (mergedDictionaries == null || mergedDictionaries.Count <= 1)
                {
                    // Nothing to reorder if there's 0 or 1 dictionary
                    return;
                }

                // Analyze dictionaries to assign priorities
                var dictionariesWithPriority = new List<(ResourceDictionary Dictionary, int Priority)>();
                foreach (var dictionary in mergedDictionaries)
                {
                    int priority = DeterminePriority(dictionary);
                    dictionariesWithPriority.Add((dictionary, priority));
                }

                // Sort by priority (lower first, higher last)
                // This ensures SfSkinManager theme resources are loaded last to take precedence
                var orderedDictionaries = dictionariesWithPriority
                    .OrderBy(x => x.Priority)
                    .Select(x => x.Dictionary)
                    .ToList();

                // Clear and re-add in correct order
                mergedDictionaries.Clear();
                foreach (var dictionary in orderedDictionaries)
                {
                    mergedDictionaries.Add(dictionary);
                }

                logger?.LogInformation("Resource dictionaries reordered successfully to support SfSkinManager");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error ordering resource dictionaries: {ErrorMessage}", ex.Message);
                // Continue execution - this is a non-critical enhancement
            }
        }

        /// <summary>
        /// Determines the priority of a resource dictionary based on its source and contents.
        /// Ensures SfSkinManager theme resources are given highest priority.
        /// </summary>
        private static int DeterminePriority(ResourceDictionary dictionary)
        {
            // Default priority if we can't determine anything specific
            int priority = 50;

            // Check source URI if available
            if (dictionary.Source != null)
            {
                string sourceString = dictionary.Source.ToString();

                // First check for SfSkinManager theme resources - these must have highest priority
                if (sourceString.Contains("Syncfusion.Themes.", StringComparison.OrdinalIgnoreCase))
                {
                    // SfSkinManager theme resources should have absolute highest priority
                    return 110;
                }

                // Check against known resource types
                foreach (var priorityEntry in _resourcePriorities)
                {
                    if (sourceString.Contains(priorityEntry.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        return priorityEntry.Value;
                    }
                }

                // Adjust priority based on source path components
                if (sourceString.Contains("Syncfusion", StringComparison.OrdinalIgnoreCase))
                {
                    priority = 85; // Syncfusion resources generally high priority but below theme
                }
                else if (sourceString.Contains("Theme", StringComparison.OrdinalIgnoreCase))
                {
                    priority = 95; // Theme resources are high priority
                }
                else if (sourceString.Contains("Styles", StringComparison.OrdinalIgnoreCase))
                {
                    priority = 75; // Style resources medium-high priority
                }
            }

            // If no source URI, examine dictionary keys
            else if (dictionary.Count > 0)
            {
                // Check if this dictionary contains SfSkinManager theme resources
                bool hasSkinManagerResources = dictionary.Keys.OfType<object>()
                    .Any(k => k.ToString().Contains("SfSkinManager") ||
                              k.ToString().Contains("FluentDark") ||
                              k.ToString().Contains("FluentLight"));

                if (hasSkinManagerResources)
                {
                    priority = 105; // SfSkinManager resources should be very high priority
                }
                // Check if this dictionary contains theme-related resources
                else if (dictionary.Keys.OfType<object>()
                    .Any(k => k.ToString().Contains("Theme") || k.ToString().Contains("Color")))
                {
                    priority = 90; // Theme resources are high priority
                }
            }

            // In safe mode, boost priority of any dictionaries with Syncfusion resources
            if (_safeModeEnabled && IsSyncfusionDictionary(dictionary))
            {
                priority = Math.Max(priority, 100);
            }

            return priority;
        }

        /// <summary>
        /// Determines if a resource dictionary contains Syncfusion-related resources
        /// </summary>
        private static bool IsSyncfusionDictionary(ResourceDictionary dictionary)
        {
            if (dictionary.Source != null &&
                dictionary.Source.ToString().Contains("Syncfusion", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Check for Syncfusion resources within the dictionary
            return dictionary.Keys.OfType<object>()
                .Any(k => k.ToString().Contains("Syncfusion") ||
                          k.ToString().Contains("Sf") ||
                          k.ToString().Contains("Fluent"));
        }

        /// <summary>
        /// Repairs common resource conflicts in the application's resource dictionaries.
        /// This method helps SfSkinManager by resolving conflicts that might prevent correct theme application.
        /// </summary>
        /// <param name="application">The application whose resources need repair</param>
        /// <param name="logger">Optional logger for diagnostic information</param>
        public static void RepairResourceConflicts(Application application, ILogger logger = null)
        {
            try
            {
                if (application?.Resources == null)
                {
                    logger?.LogWarning("Cannot repair resources: Application or Resources is null");
                    return;
                }

                // Identify duplicate keys across dictionaries
                var keyOccurrences = new Dictionary<object, int>();
                var keyLocations = new Dictionary<object, List<ResourceDictionary>>();

                foreach (var dictionary in application.Resources.MergedDictionaries)
                {
                    foreach (var key in dictionary.Keys)
                    {
                        if (!keyOccurrences.ContainsKey(key))
                        {
                            keyOccurrences[key] = 0;
                            keyLocations[key] = new List<ResourceDictionary>();
                        }

                        keyOccurrences[key]++;
                        keyLocations[key].Add(dictionary);
                    }
                }

                // Find duplicates
                var duplicateKeys = keyOccurrences.Where(kv => kv.Value > 1).Select(kv => kv.Key).ToList();

                // Log information about duplicates
                if (duplicateKeys.Any())
                {
                    logger?.LogWarning("Found {Count} duplicate resource keys", duplicateKeys.Count);

                    // For now, we'll just log and not modify, as automatic resolution could cause issues
                    foreach (var key in duplicateKeys)
                    {
                        logger?.LogDebug("Duplicate key: {Key} found in {Count} dictionaries",
                            key, keyLocations[key].Count);
                    }
                }
                else
                {
                    logger?.LogInformation("No duplicate resource keys found");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error repairing resource conflicts: {ErrorMessage}", ex.Message);
                // Continue execution - this is a non-critical enhancement
            }
        }
    }
}
