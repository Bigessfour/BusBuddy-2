using System.Windows;
using System.Windows.Media;
using Syncfusion.UI.Xaml.DockingManager;
using Syncfusion.UI.Xaml.Grid;

namespace Bus_Buddy.Utilities
{
    /// <summary>
    /// Enhanced Syncfusion UI configuration for Bus Buddy (WPF)
    /// Implements WPF best practices for professional UI appearance
    /// </summary>
    public static class SyncfusionUIEnhancerWpf
    {
        public static void ApplyDataGridTheme(SfDataGrid dataGrid)
        {
            if (dataGrid == null) return;
            // Example: Set a modern theme and basic styling
            dataGrid.Style.AlternatingRowBackground = new SolidColorBrush(Color.FromRgb(240, 246, 252));
            dataGrid.Style.RowBackground = new SolidColorBrush(Colors.White);
            dataGrid.Style.HeaderBackground = new SolidColorBrush(Color.FromRgb(0, 120, 215));
            dataGrid.Style.HeaderForeground = new SolidColorBrush(Colors.White);
            dataGrid.Style.SelectionBackground = new SolidColorBrush(Color.FromRgb(51, 153, 255));
            dataGrid.Style.SelectionForeground = new SolidColorBrush(Colors.White);
        }

        public static void ApplyDockingManagerTheme(SfDockingManager dockingManager)
        {
            if (dockingManager == null) return;
            // Example: Set a modern theme
            dockingManager.ThemeName = "Office2019Colorful";
        }

        public static void ApplyWindowTheme(Window window)
        {
            if (window == null) return;
            window.Background = new SolidColorBrush(Color.FromRgb(250, 249, 248));
        }
    }
}
