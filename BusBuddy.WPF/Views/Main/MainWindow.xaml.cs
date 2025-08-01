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
            StatusText.Text = "BusBuddy Phase 1 - Ready";
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Loading Dashboard...";
                ContentFrame.Navigate(new DashboardView());
                StatusText.Text = "Dashboard loaded successfully";
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
                    return;
                }

                StatusText.Text = "Analytics Dashboard loaded successfully";
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
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error loading Activities";
                MessageBox.Show($"Error loading Activities view: {ex.Message}", "Navigation Error");
            }
        }
    }
}
