using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Serilog;
using Serilog.Context;
using Syncfusion.Windows.Tools.Controls;

namespace BusBuddy.WPF.Utilities
{
    /// <summary>
    /// Standardization utility for DockingManager across the application
    /// Provides consistent sizing, TDI mode enforcement, and state persistence management
    /// </summary>
    public static class DockingManagerStandardization
    {
        private static readonly ILogger Logger = Log.ForContext(typeof(DockingManagerStandardization));

        // Standard panel sizing constants
        public const double StandardSidePanelWidth = 300.0;
        public const double StandardBottomPanelHeight = 200.0;
        public const double StandardTopPanelHeight = 64.0;
        public const double MinimumPanelWidth = 200.0;
        public const double MinimumPanelHeight = 150.0;
        public const double MaximumPanelWidth = 600.0;
        public const double MaximumPanelHeight = 400.0;

        // State persistence limits for performance
        public const long MaxStatePersistenceSize = 1024 * 1024; // 1MB limit

        /// <summary>
        /// Apply standard configuration to a DockingManager
        /// This ensures consistent TDI mode, sizing, and behavior across the application
        /// </summary>
        /// <param name="dockingManager">The DockingManager to standardize</param>
        /// <param name="windowName">Name for logging purposes</param>
        public static void ApplyStandardConfiguration(DockingManager dockingManager, string windowName = "Unknown")
        {
            if (dockingManager == null)
            {
                Logger.Warning("Cannot apply standard configuration to null DockingManager");
                return;
            }

            using (LogContext.PushProperty("DockingManagerConfig", windowName))
            {
                try
                {
                    Logger.Information("Applying standard DockingManager configuration for {WindowName}", windowName);

                    // Enforce TDI (Tabbed Document Interface) mode globally
                    dockingManager.UseDocumentContainer = true;
                    dockingManager.ContainerMode = ContainerMode.TDI;
                    dockingManager.DockBehavior = DockBehavior.VS2010;
                    dockingManager.DockFill = true;

                    // Enable professional features
                    dockingManager.EnableAutoAdjustment = true;
                    dockingManager.ShowTabItemsInTitleBar = true;

                    // State persistence with size limits
                    dockingManager.PersistState = true;

                    // Subscribe to events for size validation and state management
                    dockingManager.ActiveWindowChanged += OnActiveWindowChanged;
                    dockingManager.WindowClosing += OnWindowClosing;
                    dockingManager.DockStateChanged += OnDockStateChanged;

                    Logger.Information("Standard DockingManager configuration applied successfully for {WindowName}", windowName);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to apply standard DockingManager configuration for {WindowName}", windowName);
                }
            }
        }

        /// <summary>
        /// Configure standard panel sizing for a docked element
        /// </summary>
        /// <param name="element">The element to configure</param>
        /// <param name="side">The side where the panel will be docked</param>
        /// <param name="customWidth">Optional custom width (uses standard if not provided)</param>
        /// <param name="customHeight">Optional custom height (uses standard if not provided)</param>
        public static void ConfigureStandardPanelSizing(FrameworkElement element, Dock side,
            double? customWidth = null, double? customHeight = null)
        {
            if (element == null)
            {
                Logger.Warning("Cannot configure sizing for null element");
                return;
            }

            try
            {
                var elementName = element.Name ?? element.GetType().Name;

                using (LogContext.PushProperty("PanelSizing", elementName))
                {
                    switch (side)
                    {
                        case Dock.Left:
                        case Dock.Right:
                            var width = customWidth ?? StandardSidePanelWidth;
                            DockingManager.SetDesiredWidthInDockedMode(element, ValidateWidth(width));
                            element.MinWidth = MinimumPanelWidth;
                            element.MaxWidth = MaximumPanelWidth;
                            Logger.Debug("Configured side panel width: {Width} for {ElementName}", width, elementName);
                            break;

                        case Dock.Top:
                            var topHeight = customHeight ?? StandardTopPanelHeight;
                            DockingManager.SetDesiredHeightInDockedMode(element, ValidateHeight(topHeight));
                            element.MinHeight = 32; // Minimum for toolbars
                            element.MaxHeight = 120; // Maximum for toolbars
                            Logger.Debug("Configured top panel height: {Height} for {ElementName}", topHeight, elementName);
                            break;

                        case Dock.Bottom:
                            var bottomHeight = customHeight ?? StandardBottomPanelHeight;
                            DockingManager.SetDesiredHeightInDockedMode(element, ValidateHeight(bottomHeight));
                            element.MinHeight = MinimumPanelHeight;
                            element.MaxHeight = MaximumPanelHeight;
                            Logger.Debug("Configured bottom panel height: {Height} for {ElementName}", bottomHeight, elementName);
                            break;
                    }

                    // Set standard docking properties
                    DockingManager.SetCanClose(element, true);
                    DockingManager.SetCanFloat(element, true);
                    DockingManager.SetCanSerialize(element, true);
                    DockingManager.SetDockAbility(element, DockAbility.All);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to configure standard panel sizing for {ElementName}", element.Name ?? "Unknown");
            }
        }

        /// <summary>
        /// Reset DockingManager to default layout with standard panel sizes
        /// </summary>
        /// <param name="dockingManager">The DockingManager to reset</param>
        public static void ResetToStandardLayout(DockingManager dockingManager)
        {
            if (dockingManager == null)
            {
                Logger.Warning("Cannot reset layout for null DockingManager");
                return;
            }

            try
            {
                Logger.Information("Resetting DockingManager to standard layout");

                // Reset all panels to standard dock state and sizing
                foreach (FrameworkElement child in dockingManager.Children)
                {
                    if (child != null)
                    {
                        var state = DockingManager.GetState(child);
                        var side = DockingManager.GetSideInDockedMode(child);

                        // Reset to docked state if it's not a document
                        if (state != DockState.Document)
                        {
                            DockingManager.SetState(child, DockState.Dock);
                        }

                        // Reapply standard sizing
                        ConfigureStandardPanelSizing(child, side);

                        Logger.Debug("Reset panel {PanelName} to standard layout", child.Name ?? "Unknown");
                    }
                }

                Logger.Information("DockingManager layout reset completed");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to reset DockingManager layout");
            }
        }

        /// <summary>
        /// Validate and clean up state persistence files to prevent performance issues
        /// </summary>
        /// <param name="applicationName">Application name for state file identification</param>
        public static void ValidateStatePersistence(string applicationName = "BusBuddy")
        {
            try
            {
                Logger.Debug("Validating DockingManager state persistence files");

                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var syncfusionPath = Path.Combine(appDataPath, "Syncfusion");

                if (Directory.Exists(syncfusionPath))
                {
                    var stateFiles = Directory.GetFiles(syncfusionPath, "*.xml", SearchOption.AllDirectories);

                    foreach (var file in stateFiles)
                    {
                        var fileInfo = new FileInfo(file);
                        if (fileInfo.Length > MaxStatePersistenceSize)
                        {
                            Logger.Warning("State file {FileName} exceeds size limit ({Size} bytes), removing for performance",
                                         fileInfo.Name, fileInfo.Length);

                            try
                            {
                                File.Delete(file);
                                Logger.Information("Removed oversized state file: {FileName}", fileInfo.Name);
                            }
                            catch (Exception deleteEx)
                            {
                                Logger.Warning(deleteEx, "Failed to delete oversized state file: {FileName}", fileInfo.Name);
                            }
                        }
                    }
                }

                Logger.Debug("State persistence validation completed");
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Error during state persistence validation");
            }
        }

        #region Event Handlers

        /// <summary>
        /// Handle active window changes with size validation
        /// </summary>
        private static void OnActiveWindowChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (e.NewValue is FrameworkElement activeWindow)
                {
                    var windowName = activeWindow.Name ?? activeWindow.GetType().Name;
                    Logger.Debug("DockingManager active window changed to: {WindowName}", windowName);

                    // Validate and correct sizing if necessary
                    ValidateWindowSizing(activeWindow);
                }
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Error handling DockingManager active window change");
            }
        }

        /// <summary>
        /// Handle window closing events with validation
        /// </summary>
        private static void OnWindowClosing(object sender, WindowClosingEventArgs e)
        {
            try
            {
                if (e.TargetItem is FrameworkElement window)
                {
                    var windowName = window.Name ?? window.GetType().Name;
                    var canClose = DockingManager.GetCanClose(window);

                    // Prevent closing of critical windows
                    if (IsCriticalWindow(windowName))
                    {
                        e.Cancel = true;
                        Logger.Information("Prevented critical window from closing: {WindowName}", windowName);
                    }
                    else
                    {
                        Logger.Debug("Allowing window to close: {WindowName}", windowName);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Error handling DockingManager window closing");
            }
        }

        /// <summary>
        /// Handle dock state changes to maintain standard configurations
        /// </summary>
        private static void OnDockStateChanged(object sender, DockStateChangedEventArgs e)
        {
            try
            {
                if (e.TargetItem is FrameworkElement element)
                {
                    var elementName = element.Name ?? element.GetType().Name;
                    Logger.Debug("Dock state changed for {ElementName}: {OldState} -> {NewState}",
                               elementName, e.OldState, e.NewState);

                    // Reapply size constraints when docking state changes
                    if (e.NewState == DockState.Dock || e.NewState == DockState.Float)
                    {
                        var side = DockingManager.GetSideInDockedMode(element);
                        ConfigureStandardPanelSizing(element, side);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Error handling DockingManager dock state change");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Validate window sizing and correct if necessary
        /// </summary>
        private static void ValidateWindowSizing(FrameworkElement window)
        {
            try
            {
                var currentWidth = window.ActualWidth;
                var currentHeight = window.ActualHeight;
                var state = DockingManager.GetState(window);

                if (state == DockState.Dock || state == DockState.Float)
                {
                    // Check if width is below minimum
                    if (currentWidth < MinimumPanelWidth)
                    {
                        var side = DockingManager.GetSideInDockedMode(window);
                        if (side == Dock.Left || side == Dock.Right)
                        {
                            DockingManager.SetDesiredWidthInDockedMode(window, StandardSidePanelWidth);
                            Logger.Information("Reset panel width from {CurrentWidth} to {StandardWidth} for {WindowName}",
                                             currentWidth, StandardSidePanelWidth, window.Name ?? "Unknown");
                        }
                    }

                    // Check if height is below minimum
                    if (currentHeight < MinimumPanelHeight)
                    {
                        var side = DockingManager.GetSideInDockedMode(window);
                        if (side == Dock.Top || side == Dock.Bottom)
                        {
                            var standardHeight = side == Dock.Top ? StandardTopPanelHeight : StandardBottomPanelHeight;
                            DockingManager.SetDesiredHeightInDockedMode(window, standardHeight);
                            Logger.Information("Reset panel height from {CurrentHeight} to {StandardHeight} for {WindowName}",
                                             currentHeight, standardHeight, window.Name ?? "Unknown");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Error validating window sizing for {WindowName}", window.Name ?? "Unknown");
            }
        }

        /// <summary>
        /// Check if a window is critical and should not be closed
        /// </summary>
        private static bool IsCriticalWindow(string windowName)
        {
            return windowName switch
            {
                "MainDashboardView" => true,
                "NavigationDrawer" => true,
                "HeaderToolbar" => true,
                _ => false
            };
        }

        /// <summary>
        /// Validate width within acceptable bounds
        /// </summary>
        private static double ValidateWidth(double width)
        {
            return Math.Max(MinimumPanelWidth, Math.Min(MaximumPanelWidth, width));
        }

        /// <summary>
        /// Validate height within acceptable bounds
        /// </summary>
        private static double ValidateHeight(double height)
        {
            return Math.Max(MinimumPanelHeight, Math.Min(MaximumPanelHeight, height));
        }

        #endregion
    }
}
