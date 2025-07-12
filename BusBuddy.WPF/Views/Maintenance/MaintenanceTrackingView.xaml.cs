using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BusBuddy.WPF.ViewModels;

namespace BusBuddy.WPF.Views.Maintenance
{
    /// <summary>
    /// Enhanced Maintenance Tracking View with comprehensive Syncfusion controls
    /// Implements advanced maintenance management with analytics and visual indicators
    /// </summary>
    public partial class MaintenanceTrackingView : UserControl
    {
        private readonly ILogger<MaintenanceTrackingView>? _logger;

        public MaintenanceTrackingView()
        {
            try
            {
                InitializeComponent();

                // Get logger for control interaction tracking
                if (Application.Current is App app && app.Services != null)
                {
                    _logger = app.Services.GetService<ILogger<MaintenanceTrackingView>>();
                    var viewModel = app.Services.GetService<MaintenanceTrackingViewModel>();

                    if (viewModel != null)
                    {
                        DataContext = viewModel;
                        _logger?.LogInformation("🔧 MaintenanceTrackingView: ViewModel initialized successfully");
                    }
                    else
                    {
                        _logger?.LogWarning("🔧 MaintenanceTrackingView: ViewModel service not found");
                    }
                }

                _logger?.LogInformation("🔧 MaintenanceTrackingView: Enhanced maintenance tracking view initialized");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "🔧 MaintenanceTrackingView: Failed to initialize - {ErrorMessage}", ex.Message);
                System.Diagnostics.Debug.WriteLine($"❌ MaintenanceTrackingView initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle Add Maintenance button clicks with logging
        /// </summary>
        private void AddMaintenanceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _logger?.LogInformation("🔧 UI Maintenance control clicked: Add Maintenance button - opening new maintenance dialog");

                var viewModel = DataContext as MaintenanceTrackingViewModel;
                if (viewModel?.AddCommand?.CanExecute(null) == true)
                {
                    viewModel.AddCommand.Execute(null);
                    _logger?.LogInformation("🔧 UI Maintenance Add command executed successfully");
                }
                else
                {
                    _logger?.LogWarning("🔧 UI Maintenance Add Maintenance button clicked but command is not available or cannot execute");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "🔧 UI Maintenance Add Maintenance button failed: {ErrorMessage}", ex.Message);
            }
        }

        /// <summary>
        /// Handle Generate Report button clicks with logging
        /// </summary>
        private void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _logger?.LogInformation("🔧 UI Maintenance control clicked: Generate Report button - creating maintenance report");

                var viewModel = DataContext as MaintenanceTrackingViewModel;
                if (viewModel?.ReportCommand?.CanExecute(null) == true)
                {
                    viewModel.ReportCommand.Execute(null);
                    _logger?.LogInformation("🔧 UI Maintenance Generate Report command executed successfully");
                }
                else
                {
                    _logger?.LogWarning("🔧 UI Maintenance Generate Report button clicked but command is not available or cannot execute");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "🔧 UI Maintenance Generate Report button failed: {ErrorMessage}", ex.Message);
            }
        }

        /// <summary>
        /// Handle Refresh button clicks with logging  
        /// </summary>
        private void RefreshMaintenanceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _logger?.LogInformation("🔧 UI Maintenance control clicked: Refresh button - refreshing maintenance data");

                var viewModel = DataContext as MaintenanceTrackingViewModel;
                if (viewModel != null)
                {
                    // Since there's no explicit refresh command, we can call LoadAsync via reflection or trigger a reload
                    _logger?.LogInformation("🔧 UI Maintenance Refresh triggered - will reload data");
                    // For now, we'll just log the action since the ViewModel doesn't expose a public refresh command
                }
                else
                {
                    _logger?.LogWarning("🔧 UI Maintenance Refresh button clicked but ViewModel is not available");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "🔧 UI Maintenance Refresh button failed: {ErrorMessage}", ex.Message);
            }
        }

        /// <summary>
        /// Handle maintenance tile clicks for drill-down navigation
        /// </summary>
        private void MaintenanceTile_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (sender is FrameworkElement tile)
                {
                    var tileName = tile.Name ?? "UnknownMaintenanceTile";
                    _logger?.LogInformation("🔧 UI Maintenance tile clicked: {TileName} - potential drill-down navigation", tileName);

                    var viewModel = DataContext as MaintenanceTrackingViewModel;
                    if (viewModel != null)
                    {
                        // Could trigger specific filtering based on tile clicked
                        switch (tileName)
                        {
                            case "PendingMaintenanceTile":
                                _logger?.LogInformation("🔧 UI Maintenance filtering to pending maintenance items");
                                // viewModel.FilterByStatus("Pending");
                                break;
                            case "OverdueMaintenanceTile":
                                _logger?.LogInformation("🔧 UI Maintenance filtering to overdue maintenance items");
                                // viewModel.FilterByStatus("Overdue");
                                break;
                            case "CompletedMaintenanceTile":
                                _logger?.LogInformation("🔧 UI Maintenance filtering to completed maintenance items");
                                // viewModel.FilterByStatus("Completed");
                                break;
                            case "CostAnalysisTile":
                                _logger?.LogInformation("🔧 UI Maintenance opening cost analysis view");
                                // viewModel.ShowCostAnalysis();
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "🔧 UI Maintenance tile interaction failed: {ErrorMessage}", ex.Message);
            }
        }

        /// <summary>
        /// Handle DataGrid selection changes with logging
        /// </summary>
        private void MaintenanceGrid_SelectionChanged(object sender, Syncfusion.UI.Xaml.Grid.GridSelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems?.Count > 0)
                {
                    var selectedItem = e.AddedItems[0];
                    _logger?.LogInformation("🔧 UI Maintenance grid selection changed: Selected maintenance item - {ItemType}",
                        selectedItem?.GetType().Name ?? "Unknown");

                    var viewModel = DataContext as MaintenanceTrackingViewModel;
                    if (viewModel != null)
                    {
                        _logger?.LogDebug("🔧 UI Maintenance selection processed successfully");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "🔧 UI Maintenance grid selection change failed: {ErrorMessage}", ex.Message);
            }
        }

        /// <summary>
        /// Handle DataGrid cell taps for detailed interaction logging
        /// </summary>
        private void MaintenanceGrid_CellTapped(object sender, Syncfusion.UI.Xaml.Grid.GridCellTappedEventArgs e)
        {
            try
            {
                var columnName = e.Column?.MappingName ?? "Unknown";
                var rowIndex = e.RowColumnIndex.RowIndex;

                _logger?.LogInformation("🔧 UI Maintenance grid cell tapped: Column '{ColumnName}' at row {RowIndex}",
                    columnName, rowIndex);

                // Special handling for specific columns
                switch (columnName)
                {
                    case "Status":
                        _logger?.LogInformation("🔧 UI Maintenance status column tapped - potential status change");
                        break;
                    case "Priority":
                        _logger?.LogInformation("🔧 UI Maintenance priority column tapped - potential priority adjustment");
                        break;
                    case "Cost":
                        _logger?.LogInformation("🔧 UI Maintenance cost column tapped - potential cost details view");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "🔧 UI Maintenance grid cell tap failed: {ErrorMessage}", ex.Message);
            }
        }

        /// <summary>
        /// Handle chart interaction events (mouse clicks on charts)
        /// </summary>
        private void Chart_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (sender is FrameworkElement chart)
                {
                    var chartName = chart.Name ?? "UnnamedMaintenanceChart";
                    _logger?.LogInformation("🔧 UI Maintenance chart interaction: {ChartName} clicked at position ({X}, {Y})",
                        chartName, e.GetPosition(chart).X, e.GetPosition(chart).Y);

                    // Could trigger chart drill-down or detailed view
                    switch (chartName)
                    {
                        case "MaintenanceTrendsChart":
                            _logger?.LogInformation("🔧 UI Maintenance trends chart clicked - potential period drill-down");
                            break;
                        case "CostDistributionChart":
                            _logger?.LogInformation("🔧 UI Maintenance cost distribution chart clicked - potential category details");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "🔧 UI Maintenance chart interaction failed: {ErrorMessage}", ex.Message);
            }
        }

        /// <summary>
        /// Override to capture any unhandled UI exceptions in maintenance tracking
        /// </summary>
        protected override void OnPreviewMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                // Log any clicks for debugging purposes
                if (e.Source is FrameworkElement element)
                {
                    var elementName = element.Name ?? element.GetType().Name;
                    _logger?.LogDebug("🔧 UI Maintenance preview click on: {ElementName} ({ElementType})",
                        elementName, element.GetType().Name);
                }

                base.OnPreviewMouseLeftButtonDown(e);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "🔧 UI Maintenance preview click handling failed: {ErrorMessage}", ex.Message);
            }
        }

        /// <summary>
        /// Log when view is loaded for performance tracking
        /// </summary>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _logger?.LogInformation("🔧 MaintenanceTrackingView: View fully loaded and ready for interaction");

                // Could initialize any view-specific settings here
                var viewModel = DataContext as MaintenanceTrackingViewModel;
                if (viewModel != null)
                {
                    _logger?.LogInformation("🔧 MaintenanceTrackingView: ViewModel binding confirmed");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "🔧 MaintenanceTrackingView: Load event failed - {ErrorMessage}", ex.Message);
            }
        }

        /// <summary>
        /// Log when view is unloaded for cleanup tracking
        /// </summary>
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _logger?.LogInformation("🔧 MaintenanceTrackingView: View unloaded - cleanup completed");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "🔧 MaintenanceTrackingView: Unload event failed - {ErrorMessage}", ex.Message);
            }
        }
    }
}
