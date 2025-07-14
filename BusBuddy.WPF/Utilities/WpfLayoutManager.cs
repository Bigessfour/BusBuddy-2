using Syncfusion.SfSkinManager;
using Syncfusion.Windows.Tools.Controls;
using System.Windows;

namespace BusBuddy.WPF.Utilities
{
    /// <summary>
    /// WPF Layout Manager - 100% Syncfusion DockingManager Compliant
    /// Reference: https://help.syncfusion.com/wpf/docking/overview
    /// </summary>
    public static class WpfLayoutManager
    {
        /// <summary>
        /// Configure DockingManager with official Syncfusion settings
        /// Per Syncfusion documentation: https://help.syncfusion.com/wpf/docking/getting-started
        /// </summary>
        public static void ConfigureDockingManager(DockingManager dockingManager)
        {
            // Official Syncfusion DockingManager configuration
            dockingManager.DockFill = true;
            dockingManager.PersistState = true;
            dockingManager.UseDocumentContainer = true;
            dockingManager.ContainerMode = DocumentContainerMode.TDI;

            // Apply theme using SfSkinManager
            SfSkinManager.SetTheme(dockingManager, new Theme("FluentDark"));
        }

        /// <summary>
        /// Add dockable content using official Syncfusion attached properties
        /// Per Syncfusion documentation: https://help.syncfusion.com/wpf/docking/docking-window
        /// </summary>
        public static void AddDockableContent(DockingManager dockingManager,
            FrameworkElement content, string header, DockState state)
        {
            // Use official Syncfusion attached properties
            DockingManager.SetHeader(content, header);
            DockingManager.SetState(content, state);
            DockingManager.SetSideInDockedMode(content, DockSide.Tabbed);
            DockingManager.SetDesiredWidthInDockedMode(content, 200);
            DockingManager.SetDesiredHeightInDockedMode(content, 200);

            // Add to DockingManager
            dockingManager.Children.Add(content);
        }

        /// <summary>
        /// Add document content using official Syncfusion method
        /// Per Syncfusion documentation: https://help.syncfusion.com/wpf/docking/document-container
        /// </summary>
        public static void AddDocumentContent(DockingManager dockingManager,
            FrameworkElement content, string header)
        {
            // Configure for document container
            DockingManager.SetHeader(content, header);
            DockingManager.SetState(content, DockState.Document);

            // Add to DockingManager
            dockingManager.Children.Add(content);
        }
    }
}
