using System.Windows;

namespace BusBuddy.WPF.Views.Main
{
    /// <summary>
    /// BusBuddy MainWindow - Phase 1 Greenfield Foundation
    /// Simple, functional main window with basic navigation
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            StatusText.Text = "BusBuddy Phase 1 - Ready";
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Loading Dashboard...";
            ContentFrame.Navigate(new DashboardView());
        }

        private void DriversButton_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Loading Drivers...";
            ContentFrame.Navigate(new DriversView());
        }

        private void VehiclesButton_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Loading Vehicles...";
            ContentFrame.Navigate(new VehiclesView());
        }

        private void ActivitiesButton_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Loading Activities...";
            ContentFrame.Navigate(new ActivityScheduleView());
        }
    }
}
