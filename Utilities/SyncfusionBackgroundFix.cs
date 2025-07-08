using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Bus_Buddy.Utilities
{
    /// <summary>
    /// Syncfusion Background Fix for WPF - eliminates background issues in WPF dashboards
    /// </summary>
    public static class SyncfusionBackgroundFixWpf
    {
        public static void FixDashboardBackground(Window dashboardWindow, Panel mainPanel)
        {
            if (dashboardWindow == null || mainPanel == null) return;
            // Set window background
            dashboardWindow.Background = new SolidColorBrush(Color.FromRgb(240, 246, 252));
            // Set main panel background
            mainPanel.Background = new SolidColorBrush(Color.FromRgb(240, 246, 252));
        }

        public static void FixPanelBackgrounds(Panel parent)
        {
            foreach (var child in parent.Children)
            {
                if (child is Panel panel)
                {
                    // Main panel (full coverage)
                    if (panel.Name.ToLower().Contains("main") || panel.Name.ToLower().Contains("dashboard"))
                    {
                        panel.Background = new SolidColorBrush(Color.FromRgb(240, 246, 252));
                    }
                    // Content panels
                    else if (panel.Name.ToLower().Contains("content"))
                    {
                        panel.Background = new SolidColorBrush(Color.FromRgb(248, 250, 252));
                        panel.Margin = new Thickness(20);
                    }
                    // Header panels
                    else if (panel.Name.ToLower().Contains("header"))
                    {
                        // Optionally set header background
                        panel.Background = new SolidColorBrush(Color.FromRgb(225, 225, 225));
                    }
                    else
                    {
                        panel.Background = new SolidColorBrush(Colors.White);
                    }
                    // Recursively fix child panels
                    FixPanelBackgrounds(panel);
                }
            }
        }
    }
}
