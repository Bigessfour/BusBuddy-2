using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace BusBuddy.WPF.Views
{
    public partial class DashboardView : UserControl
    {
        private readonly string _layoutFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BusBuddy", "docking-layout.xml");

        // dockingManager is defined in XAML and generated as a field by InitializeComponent

        public DashboardView()
        {
            InitializeComponent();
            // Set DataContext using DI with null checks to avoid CS8602
            var app = Application.Current as App;
            if (app?.Services != null)
            {
                DataContext = app.Services.GetService(typeof(BusBuddy.WPF.ViewModels.DashboardViewModel));
            }
            else
            {
                // Fallback: DataContext remains null or set to a default instance if desired
            }
            this.Loaded += OnDashboardViewLoaded;
        }

        private void OnDashboardViewLoaded(object sender, RoutedEventArgs e)
        {
            //LoadLayout();
            //var window = Window.GetWindow(this);
            //if (window != null)
            //{
            //    window.Closing += OnWindowClosing;
            //}
        }

        private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            //SaveLayout();
        }

        private void SaveLayout()
        {
            //try
            //{
            //    var directory = Path.GetDirectoryName(_layoutFilePath);
            //    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            //    {
            //        Directory.CreateDirectory(directory);
            //    }
            //    using (var writer = new XmlTextWriter(_layoutFilePath, System.Text.Encoding.UTF8))
            //    {
            //        writer.Formatting = Formatting.Indented;
            //        this.dockingManager.SaveDockState(writer);
            //    }
            //}
            //catch (System.Exception ex)
            //{
            //    System.Diagnostics.Debug.WriteLine($"Failed to save layout: {ex.Message}");
            //}
        }

        private void LoadLayout()
        {
            //try
            //{
            //    if (File.Exists(_layoutFilePath))
            //    {
            //        using (var reader = new XmlTextReader(_layoutFilePath))
            //        {
            //            this.dockingManager.LoadDockState(reader);
            //        }
            //    }
            //}
            //catch (System.Exception ex)
            //{
            //    System.Diagnostics.Debug.WriteLine($"Failed to load layout: {ex.Message}");
            //}
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle sidebar visibility
            var sidebar = this.FindName("SidebarBorder") as System.Windows.Controls.Border;
            if (sidebar != null)
            {
                sidebar.Visibility = sidebar.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            }
        }
    }
}
