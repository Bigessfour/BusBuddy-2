using Serilog;
using Syncfusion.SfSkinManager;
using Syncfusion.Themes.FluentDark.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace BusBuddy.WPF.Services
{
    /// <summary>
    /// Optimized Theme Service for centralized theme management
    /// Eliminates redundant theme operations and provides efficient fallback handling
    /// </summary>
    public class OptimizedThemeService
    {
        private static readonly ILogger Logger = Log.ForContext<OptimizedThemeService>();
        private static bool _isThemeInitialized = false;
        private static readonly object _themeLock = new object();
        private static readonly Dictionary<string, bool> _resourceCache = new();
        private static string _currentTheme = "FluentDark";

        /// <summary>
        /// Initialize theme once at application startup with comprehensive validation
        /// </summary>
        public static void InitializeApplicationTheme()
        {
            lock (_themeLock)
            {
                if (_isThemeInitialized)
                {
                    Logger.Debug("Theme already initialized, skipping redundant initialization");
                    return;
                }

                try
                {
                    Logger.Information("🎨 Initializing optimized theme system");

                    // Step 1: Configure SfSkinManager with optimal settings
                    SfSkinManager.ApplyStylesOnApplication = true;
                    SfSkinManager.ApplyThemeAsDefaultStyle = true;

                    // Step 2: Apply primary theme with validated fallback
                    bool themeApplied = ApplyThemeWithFallback("FluentDark");

                    if (themeApplied)
                    {
                        // Step 3: Validate and inject missing critical resources once
                        ValidateAndInjectCriticalResources();

                        // Step 4: Pre-warm resource cache for frequently accessed resources
                        PreWarmResourceCache();

                        _isThemeInitialized = true;
                        Logger.Information("✅ Optimized theme system initialized successfully with {Theme}", _currentTheme);
                    }
                    else
                    {
                        throw new InvalidOperationException("Failed to apply any supported theme");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "❌ Critical theme initialization failure");
                    throw;
                }
            }
        }

        /// <summary>
        /// Apply theme with intelligent fallback strategy
        /// Only uses themes available in the project (FluentDark, FluentLight)
        /// </summary>
        private static bool ApplyThemeWithFallback(string primaryTheme)
        {
            // Ordered theme fallback strategy using only available themes
            var themeStrategy = new[]
            {
                new { Name = primaryTheme, HasAssembly = true },
                new { Name = "FluentDark", HasAssembly = true },
                new { Name = "Office2019Colorful", HasAssembly = false }
                // Note: FluentLight removed from fallback due to KeyNotFoundException risk in v30.1.40
                // as noted in existing code comments
            };

            foreach (var theme in themeStrategy)
            {
                try
                {
                    Logger.Debug("Attempting to apply theme: {ThemeName}", theme.Name);

                    // For FluentDark themes, verify assembly is available
                    if (theme.HasAssembly && theme.Name.Contains("Fluent"))
                    {
                        // Verify FluentDark assembly is loaded
                        var fluentDarkType = typeof(FluentDarkThemeSettings);
                        if (fluentDarkType.Assembly == null)
                        {
                            Logger.Warning("FluentDark assembly not available for theme: {ThemeName}", theme.Name);
                            continue;
                        }
                    }

                    SfSkinManager.ApplicationTheme = new Theme(theme.Name);
                    _currentTheme = theme.Name;

                    Logger.Information("✅ Successfully applied theme: {ThemeName}", theme.Name);
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Warning(ex, "Failed to apply theme {ThemeName}, trying fallback", theme.Name);
                }
            }

            return false;
        }        /// <summary>
                 /// Validate and inject critical resources that may be missing
                 /// Optimized single-pass resource injection
                 /// </summary>
        private static void ValidateAndInjectCriticalResources()
        {
            var app = Application.Current;
            if (app?.Resources == null)
            {
                Logger.Warning("Application resources not available for injection");
                return;
            }

            // Critical resources mapping with optimized defaults
            var criticalResources = new Dictionary<string, SolidColorBrush>
            {
                ["ContentForeground"] = new SolidColorBrush(Colors.White),
                ["TextForeground"] = new SolidColorBrush(Colors.White),
                ["ControlForeground"] = new SolidColorBrush(Colors.White),
                ["SurfaceBackground"] = new SolidColorBrush(Color.FromRgb(31, 31, 31)),
                ["SurfaceBorderBrush"] = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                ["PrimaryBackground"] = new SolidColorBrush(Color.FromRgb(31, 31, 31)),
                ["ContentBackground"] = new SolidColorBrush(Color.FromRgb(37, 37, 38)),
                ["AccentBackground"] = new SolidColorBrush(Color.FromRgb(0, 120, 212)),
                ["SuccessBackground"] = new SolidColorBrush(Color.FromRgb(46, 204, 113))
            };

            int injectedCount = 0;
            foreach (var resource in criticalResources)
            {
                if (!app.Resources.Contains(resource.Key))
                {
                    app.Resources[resource.Key] = resource.Value;
                    _resourceCache[resource.Key] = true;
                    injectedCount++;
                }
                else
                {
                    _resourceCache[resource.Key] = true;
                }
            }

            if (injectedCount > 0)
            {
                Logger.Information("Injected {Count} missing critical resources", injectedCount);
            }
            else
            {
                Logger.Debug("All critical resources already present");
            }
        }

        /// <summary>
        /// Pre-warm resource cache for performance optimization
        /// </summary>
        private static void PreWarmResourceCache()
        {
            var app = Application.Current;
            if (app?.Resources == null) return;

            // Pre-warm frequently accessed resources
            var frequentResources = new[]
            {
                "BusBuddy.Brush.Primary",
                "BusBuddy.Brush.Surface.Dark",
                "BusBuddy.Brush.Text.Primary",
                "BrandPrimaryBrush",
                "SurfaceBackground",
                "ContentForeground"
            };

            foreach (var resourceKey in frequentResources)
            {
                try
                {
                    var resource = app.TryFindResource(resourceKey);
                    _resourceCache[resourceKey] = resource != null;
                }
                catch
                {
                    _resourceCache[resourceKey] = false;
                }
            }

            Logger.Debug("Pre-warmed {Count} frequently accessed resources", frequentResources.Length);
        }

        /// <summary>
        /// Fast resource existence check using cache
        /// </summary>
        public static bool IsResourceAvailable(string resourceKey)
        {
            if (_resourceCache.TryGetValue(resourceKey, out bool exists))
            {
                return exists;
            }

            // Fallback to direct check and cache result
            var app = Application.Current;
            if (app?.Resources == null) return false;

            try
            {
                bool resourceExists = app.Resources.Contains(resourceKey) ||
                                    app.Resources.MergedDictionaries.Any(dict => dict.Contains(resourceKey));
                _resourceCache[resourceKey] = resourceExists;
                return resourceExists;
            }
            catch
            {
                _resourceCache[resourceKey] = false;
                return false;
            }
        }

        /// <summary>
        /// Dynamic theme switching with validation
        /// </summary>
        public static bool SwitchTheme(string themeName)
        {
            lock (_themeLock)
            {
                try
                {
                    Logger.Information("Switching theme to: {ThemeName}", themeName);

                    if (ApplyThemeWithFallback(themeName))
                    {
                        // Clear resource cache to force re-validation
                        _resourceCache.Clear();
                        ValidateAndInjectCriticalResources();
                        PreWarmResourceCache();

                        Logger.Information("✅ Theme switch successful: {ThemeName}", _currentTheme);
                        return true;
                    }

                    Logger.Warning("Theme switch failed, reverting to: {CurrentTheme}", _currentTheme);
                    return false;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error during theme switch to {ThemeName}", themeName);
                    return false;
                }
            }
        }

        /// <summary>
        /// Get current theme status for diagnostics
        /// </summary>
        public static (string CurrentTheme, bool IsInitialized, int CachedResources) GetThemeStatus()
        {
            return (_currentTheme, _isThemeInitialized, _resourceCache.Count);
        }

        /// <summary>
        /// Validate theme health and provide diagnostics
        /// </summary>
        public static bool ValidateThemeHealth()
        {
            try
            {
                var status = GetThemeStatus();
                var currentSfTheme = SfSkinManager.ApplicationTheme?.ToString() ?? "Unknown";

                Logger.Information("Theme Health Check:");
                Logger.Information("  Current Theme: {CurrentTheme}", status.CurrentTheme);
                Logger.Information("  SfSkinManager Theme: {SfTheme}", currentSfTheme);
                Logger.Information("  Initialized: {IsInitialized}", status.IsInitialized);
                Logger.Information("  Cached Resources: {CachedResources}", status.CachedResources);
                Logger.Information("  ApplyStylesOnApplication: {ApplyStyles}", SfSkinManager.ApplyStylesOnApplication);

                return status.IsInitialized && status.CachedResources > 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Theme health validation failed");
                return false;
            }
        }
    }
}
