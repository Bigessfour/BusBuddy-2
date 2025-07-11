using System;
using System.Windows;
using System.Windows.Controls;
using Syncfusion.Windows.Tools.Controls;
using BusBuddy.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BusBuddy.WPF.Views.Dashboard
{
    /// <summary>
    /// Enhanced Ribbon Window implementing Development Plan Phase 6A requirements
    /// Provides professional interface with Syncfusion Ribbon and dashboard switching
    /// </summary>
    public partial class EnhancedRibbonWindow : RibbonWindow
    {
        public EnhancedRibbonWindow()
        {
            InitializeComponent();
            InitializeWindow();
        }

        /// <summary>
        /// Initialize window settings and data context
        /// </summary>
        private void InitializeWindow()
        {
            try
            {
                // Set the data context to DashboardViewModel
                // Get DashboardViewModel from DI container if available
                if (Application.Current is App app && app.Services != null)
                {
                    try
                    {
                        this.DataContext = app.Services.GetService(typeof(DashboardViewModel));
                        System.Diagnostics.Debug.WriteLine("EnhancedRibbonWindow: Successfully set DashboardViewModel from DI");
                    }
                    catch (Exception diEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"EnhancedRibbonWindow: DI error - {diEx.Message}");
                        // Continue without DataContext for now
                    }
                }

                // Set initial focus to Enhanced Dashboard
                ShowEnhancedDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing Enhanced Ribbon Window: {ex.Message}",
                              "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Dashboard Navigation Event Handlers

        /// <summary>
        /// Show Dashboard Overview (same as Enhanced View for now)
        /// </summary>
        private void OverviewButton_Click(object sender, RoutedEventArgs e)
        {
            ShowEnhancedDashboard();
        }

        /// <summary>
        /// Switch to Enhanced Dashboard with DockingManager
        /// </summary>
        private void EnhancedViewButton_Click(object sender, RoutedEventArgs e)
        {
            ShowEnhancedDashboard();
        }

        /// <summary>
        /// Switch to Simple Dashboard with TabControl
        /// </summary>
        private void SimpleViewButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSimpleDashboard();
        }

        #endregion

        #region Data Management Event Handlers

        /// <summary>
        /// Refresh all dashboard data
        /// </summary>
        private async void RefreshAllButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var viewModel = this.DataContext as DashboardViewModel;
                if (viewModel != null)
                {
                    // Trigger data refresh asynchronously
                    await viewModel.InitializeAsync();
                    
                    MessageBox.Show("Data refresh completed successfully!",
                                  "Refresh", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Dashboard not ready for refresh. Please wait for initialization to complete.",
                                  "Refresh", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing data: {ex.Message}",
                              "Refresh Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Layout Management Event Handlers

        /// <summary>
        /// Reset dashboard layout to default
        /// </summary>
        private void ResetLayoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var enhancedDashboard = FindName("EnhancedDashboard") as EnhancedDashboardView;
                if (enhancedDashboard != null && enhancedDashboard.Visibility == Visibility.Visible)
                {
                    // Reset Enhanced Dashboard layout
                    enhancedDashboard.ResetToDefaultLayout();
                    MessageBox.Show("Enhanced Dashboard layout reset to default.",
                                  "Layout Reset", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Layout reset is only available for Enhanced Dashboard view.",
                                  "Layout Reset", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error resetting layout: {ex.Message}",
                              "Layout Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Operations Event Handlers

        /// <summary>
        /// Navigate to Bus Management
        /// </summary>
        private void BusManagementButton_Click(object sender, RoutedEventArgs e)
        {
            // For now, show Enhanced Dashboard and focus on Bus Management panel
            ShowEnhancedDashboard();
            MessageBox.Show("Bus Management module activated.",
                          "Bus Management", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Navigate to Driver Management
        /// </summary>
        private void DriverManagementButton_Click(object sender, RoutedEventArgs e)
        {
            ShowEnhancedDashboard();
            MessageBox.Show("Driver Management module activated.",
                          "Driver Management", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Navigate to Route Management
        /// </summary>
        private void RouteManagementButton_Click(object sender, RoutedEventArgs e)
        {
            ShowEnhancedDashboard();
            MessageBox.Show("Route Management module activated.",
                          "Route Management", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Navigate to Maintenance Tracking
        /// </summary>
        private void MaintenanceButton_Click(object sender, RoutedEventArgs e)
        {
            ShowEnhancedDashboard();
            MessageBox.Show("Maintenance Tracking module activated.",
                          "Maintenance", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Dashboard View Management

        /// <summary>
        /// Show the Enhanced Dashboard with DockingManager
        /// </summary>
        private void ShowEnhancedDashboard()
        {
            try
            {
                var enhancedDashboard = FindName("EnhancedDashboard") as EnhancedDashboardView;
                var simpleDashboard = FindName("SimpleDashboard") as SimpleDashboardView;

                if (enhancedDashboard != null)
                    enhancedDashboard.Visibility = Visibility.Visible;
                if (simpleDashboard != null)
                    simpleDashboard.Visibility = Visibility.Collapsed;

                // Update window title
                this.Title = "Bus Buddy - Enhanced Interface";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing Enhanced Dashboard: {ex.Message}",
                              "Display Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Show the Simple Dashboard with TabControl
        /// </summary>
        private void ShowSimpleDashboard()
        {
            try
            {
                var enhancedDashboard = FindName("EnhancedDashboard") as EnhancedDashboardView;
                var simpleDashboard = FindName("SimpleDashboard") as SimpleDashboardView;

                if (enhancedDashboard != null)
                    enhancedDashboard.Visibility = Visibility.Collapsed;
                if (simpleDashboard != null)
                    simpleDashboard.Visibility = Visibility.Visible;

                // Update window title
                this.Title = "Bus Buddy - Simple Interface";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing Simple Dashboard: {ex.Message}",
                              "Display Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Window Event Handlers

        /// <summary>
        /// Handle window closing to save layout state
        /// </summary>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                // Save Enhanced Dashboard layout if visible
                var enhancedDashboard = FindName("EnhancedDashboard") as EnhancedDashboardView;
                if (enhancedDashboard != null && enhancedDashboard.Visibility == Visibility.Visible)
                {
                    // Layout state is automatically saved by DockingManager PersistState=True
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving layout on close: {ex.Message}");
            }

            base.OnClosing(e);
        }

        /// <summary>
        /// Handle window loaded event
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Additional initialization after window is loaded
                System.Diagnostics.Debug.WriteLine("Enhanced Ribbon Window loaded successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in window loaded event: {ex.Message}");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get current dashboard layout statistics
        /// Used for performance monitoring as per development plan
        /// </summary>
        public string GetDashboardStatistics()
        {
            try
            {
                var enhancedDashboard = FindName("EnhancedDashboard") as EnhancedDashboardView;
                if (enhancedDashboard != null && enhancedDashboard.Visibility == Visibility.Visible)
                {
                    return $"Enhanced Dashboard: {enhancedDashboard.GetLayoutStatistics()}";
                }
                else
                {
                    return "Simple Dashboard: TabControl layout active";
                }
            }
            catch (Exception ex)
            {
                return $"Statistics error: {ex.Message}";
            }
        }

        /// <summary>
        /// Switch dashboard view programmatically
        /// </summary>
        /// <param name="useEnhanced">True for Enhanced Dashboard, False for Simple</param>
        public void SwitchDashboardView(bool useEnhanced)
        {
            if (useEnhanced)
            {
                ShowEnhancedDashboard();
            }
            else
            {
                ShowSimpleDashboard();
            }
        }

        #endregion
    }
}
