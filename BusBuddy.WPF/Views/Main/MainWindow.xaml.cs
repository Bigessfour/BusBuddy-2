using System;
using System.Windows;
using System.Windows.Controls;
using BusBuddy.WPF.Views; // Added reference to Views namespace

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
            StatusText.Text = "BusBuddy Ready - Select a section to begin";
        }

        /// <summary>
        /// Loads the default dashboard view on startup
        /// </summary>
        private void LoadDefaultView()
        {
            try
            {
                StatusText.Text = "Loading Dashboard...";
                ContentFrame.Navigate(new DashboardView());
                StatusText.Text = "Dashboard loaded successfully";
                UpdateNavigationSelection(DashboardButton);
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error loading default view";
                System.Diagnostics.Debug.WriteLine($"Error loading default view: {ex.Message}");
            }
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Loading Dashboard...";
                ContentFrame.Navigate(new DashboardView());
                StatusText.Text = "Dashboard loaded successfully";
                UpdateNavigationSelection(sender as Button);
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error loading Dashboard";
                MessageBox.Show($"Error loading Dashboard view: {ex.Message}", "Navigation Error");
            }
        }

        private void AnalyticsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Loading Analytics Dashboard...";

                // Try to navigate with full path if there's an issue
                try
                {
                    ContentFrame.Navigate(new BusBuddy.WPF.Views.Analytics.AnalyticsDashboardView());
                }
                catch
                {
                    // Fallback to Dashboard for MVP
                    ContentFrame.Navigate(new DashboardView());
                    StatusText.Text = "Analytics not available in MVP - showing Dashboard";
                    UpdateNavigationSelection(sender as Button);
                    return;
                }

                StatusText.Text = "Analytics Dashboard loaded successfully";
                UpdateNavigationSelection(sender as Button);
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error loading Analytics";
                MessageBox.Show($"Error loading Analytics view: {ex.Message}", "Navigation Error");
            }
        }

        private void DriversButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Loading Drivers...";
                ContentFrame.Navigate(new DriversView());
                StatusText.Text = "Drivers view loaded successfully";
                UpdateNavigationSelection(sender as Button);
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error loading Drivers";
                MessageBox.Show($"Error loading Drivers view: {ex.Message}", "Navigation Error");
            }
        }

        private void VehiclesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Loading Vehicles...";
                ContentFrame.Navigate(new VehiclesView());
                StatusText.Text = "Vehicles view loaded successfully";
                UpdateNavigationSelection(sender as Button);
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error loading Vehicles";
                MessageBox.Show($"Error loading Vehicles view: {ex.Message}", "Navigation Error");
            }
        }

        private void ActivitiesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Loading Activities...";
                ContentFrame.Navigate(new ActivityScheduleView());
                StatusText.Text = "Activities view loaded successfully";
                UpdateNavigationSelection(sender as Button);
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error loading Activities";
                MessageBox.Show($"Error loading Activities view: {ex.Message}", "Navigation Error");
            }
        }

        private void RouteManagementButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Loading Route Management...";
                // Check if Route Management view exists
                try
                {
                    ContentFrame.Navigate(new BusBuddy.WPF.Views.Route.RouteManagementView());
                    StatusText.Text = "Route Management loaded successfully";
                }
                catch
                {
                    // Fallback to Dashboard if not available
                    ContentFrame.Navigate(new DashboardView());
                    StatusText.Text = "Route Management not available - showing Dashboard";
                }
                UpdateNavigationSelection(sender as Button);
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error loading Route Management";
                MessageBox.Show($"Error loading Route Management view: {ex.Message}", "Navigation Error");
            }
        }

        private void FuelManagementButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Loading Fuel Management...";
                // For MVP, show dashboard with fuel management message
                ContentFrame.Navigate(new DashboardView());
                StatusText.Text = "Fuel Management - Available in full version";
                UpdateNavigationSelection(sender as Button);
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error loading Fuel Management";
                MessageBox.Show($"Error loading Fuel Management view: {ex.Message}", "Navigation Error");
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Loading Settings...";
                // For MVP, show dashboard with settings message
                ContentFrame.Navigate(new DashboardView());
                StatusText.Text = "Settings - Available in full version";
                UpdateNavigationSelection(sender as Button);
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error loading Settings";
                MessageBox.Show($"Error loading Settings view: {ex.Message}", "Navigation Error");
            }
        }

        /// <summary>
        /// Updates the visual selection state of navigation buttons
        /// </summary>
        private void UpdateNavigationSelection(Button selectedButton)
        {
            try
            {
                // Reset all navigation buttons to default state
                var buttons = new[]
                {
                    DashboardButton, AnalyticsButton, DriversButton,
                    VehiclesButton, ActivitiesButton, RouteManagementButton,
                    FuelManagementButton, SettingsButton
                };

                foreach (var button in buttons)
                {
                    if (button != null)
                    {
                        button.Background = System.Windows.Media.Brushes.White;
                        button.BorderBrush = System.Windows.Media.Brushes.LightGray;
                    }
                }

                // Highlight the selected button
                if (selectedButton != null)
                {
                    selectedButton.Background = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(240, 247, 255)); // #F0F7FF
                    selectedButton.BorderBrush = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(49, 130, 206)); // #3182CE
                }
            }
            catch (Exception ex)
            {
                // Log error but don't show to user as this is just visual feedback
                System.Diagnostics.Debug.WriteLine($"Error updating navigation selection: {ex.Message}");
            }
        }
    }
}
