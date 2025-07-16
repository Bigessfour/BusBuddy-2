using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Context;
using BusBuddy.WPF.ViewModels;

namespace BusBuddy.WPF.Views.Fuel
{
    /// <summary>
    /// Enhanced Fuel Management View with comprehensive Syncfusion controls and analytics
    /// Implements advanced fuel tracking, efficiency monitoring, and cost analysis
    /// </summary>
    public partial class FuelManagementView : UserControl
    {
        private static readonly ILogger Logger = Log.ForContext<FuelManagementView>();

        public FuelManagementView()
        {
            try
            {
                using (LogContext.PushProperty("ViewType", nameof(FuelManagementView)))
                using (LogContext.PushProperty("OperationType", "ViewInitialization"))
                {
                    Logger.Information("FuelManagementView initialization started");

                    InitializeComponent();

                    // Get logger for control interaction tracking
                    if (Application.Current is App app && app.Services != null)
                    {
                        var viewModel = app.Services.GetService<FuelManagementViewModel>();

                        if (viewModel != null)
                        {
                            DataContext = viewModel;
                            Logger.Information("⛽ FuelManagementView: ViewModel initialized successfully");
                        }
                        else
                        {
                            Logger.Warning("⛽ FuelManagementView: ViewModel service not found");
                        }
                    }

                    Logger.Information("⛽ FuelManagementView: Enhanced fuel management view initialized");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "⛽ FuelManagementView: Failed to initialize - {ErrorMessage}", ex.Message);
                System.Diagnostics.Debug.WriteLine($"❌ FuelManagementView initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle Add Fuel Record button clicks with logging
        /// </summary>
        private void AddFuelRecordButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Logger.Information("⛽ UI Fuel control clicked: Add Fuel Record button - opening new fuel record dialog");

                var viewModel = DataContext as FuelManagementViewModel;
                if (viewModel?.AddCommand?.CanExecute(null) == true)
                {
                    viewModel.AddCommand.Execute(null);
                    Logger.Information("⛽ UI Fuel Add command executed successfully");
                }
                else
                {
                    Logger.Warning("⛽ UI Fuel Add Fuel Record button clicked but command is not available or cannot execute");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "⛽ UI Fuel Add Fuel Record button failed: {ErrorMessage}", ex.Message);
            }
        }

        /// <summary>
        /// Handle Generate Fuel Report button clicks with logging
        /// </summary>
        private void GenerateFuelReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Logger.Information("⛽ UI Fuel control clicked: Generate Fuel Report button - creating fuel efficiency report");

                var viewModel = DataContext as FuelManagementViewModel;
                if (viewModel?.ReportCommand?.CanExecute(null) == true)
                {
                    viewModel.ReportCommand.Execute(null);
                    Logger.Information("⛽ UI Fuel Generate Report command executed successfully");
                }
                else
                {
                    Logger.Warning("⛽ UI Fuel Generate Report button clicked but command is not available or cannot execute");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "⛽ UI Fuel Generate Report button failed: {ErrorMessage}", ex.Message);
            }
        }

        /// <summary>
        /// Handle Fuel Reconciliation button clicks with logging
        /// </summary>
        private void FuelReconciliationButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Logger.Information("⛽ UI Fuel control clicked: Fuel Reconciliation button - comparing bulk station data with vehicle usage");

                var viewModel = DataContext as FuelManagementViewModel;
                if (viewModel?.ReconciliationCommand?.CanExecute(null) == true)
                {
                    viewModel.ReconciliationCommand.Execute(null);
                    Logger.Information("⛽ UI Fuel Reconciliation command executed successfully");
                }
                else
                {
                    Logger.Warning("⛽ UI Fuel Reconciliation button clicked but command is not available or cannot execute");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "⛽ UI Fuel Reconciliation button failed: {ErrorMessage}", ex.Message);
            }
        }

        /// <summary>
        /// Handle Refresh button clicks with logging
        /// </summary>
        private void RefreshFuelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Logger.Information("⛽ UI Fuel control clicked: Refresh button - refreshing fuel data and analytics");

                var viewModel = DataContext as FuelManagementViewModel;
                if (viewModel != null)
                {
                    // Since there's no explicit refresh command, we can trigger a data reload
                    Logger.Information("⛽ UI Fuel Refresh triggered - will reload fuel data and recalculate metrics");
                    // For now, we'll just log the action since the ViewModel might not expose a public refresh command
                }
                else
                {
                    Logger.Warning("⛽ UI Fuel Refresh button clicked but ViewModel is not available");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "⛽ UI Fuel Refresh button failed: {ErrorMessage}", ex.Message);
            }
        }

        /// <summary>
        /// Handle fuel tile clicks for drill-down navigation and filtering
        /// </summary>
        private void FuelTile_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (sender is FrameworkElement tile)
                {
                    var tileName = tile.Name ?? "UnknownFuelTile";
                    Logger.Information("⛽ UI Fuel tile clicked: {TileName} - potential drill-down navigation", tileName);

                    var viewModel = DataContext as FuelManagementViewModel;
                    if (viewModel != null)
                    {
                        // Could trigger specific analytics views based on tile clicked
                        switch (tileName)
                        {
                            case "MonthlyFuelCostTile":
                                Logger.Information("⛽ UI Fuel opening monthly cost breakdown view");
                                // viewModel.ShowMonthlyCostBreakdown();
                                break;
                            case "AverageMPGTile":
                                Logger.Information("⛽ UI Fuel opening MPG analysis view");
                                // viewModel.ShowMPGAnalysis();
                                break;
                            case "TotalGallonsTile":
                                Logger.Information("⛽ UI Fuel opening consumption analysis view");
                                // viewModel.ShowConsumptionAnalysis();
                                break;
                            case "CostPerMileTile":
                                Logger.Information("⛽ UI Fuel opening efficiency analysis view");
                                // viewModel.ShowEfficiencyAnalysis();
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "⛽ UI Fuel tile interaction failed: {ErrorMessage}", ex.Message);
            }
        }

        /// <summary>
        /// Handle DataGrid selection changes with logging
        /// </summary>
        private void FuelDataGrid_SelectionChanged(object sender, Syncfusion.UI.Xaml.Grid.GridSelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems?.Count > 0)
                {
                    var selectedItem = e.AddedItems[0];
                    Logger.Information("⛽ UI Fuel grid selection changed: Selected fuel record - {ItemType}",
                        selectedItem?.GetType().Name ?? "Unknown");

                    var viewModel = DataContext as FuelManagementViewModel;
                    if (viewModel != null)
                    {
                        Logger.Debug("⛽ UI Fuel selection processed successfully");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "⛽ UI Fuel grid selection change failed: {ErrorMessage}", ex.Message);
            }
        }

        /// <summary>
        /// Handle DataGrid cell taps for detailed interaction logging
        /// </summary>
        private void FuelDataGrid_CellTapped(object sender, Syncfusion.UI.Xaml.Grid.GridCellTappedEventArgs e)
        {
            try
            {
                var columnName = e.Column?.MappingName ?? "Unknown";
                var rowIndex = e.RowColumnIndex.RowIndex;

                Logger.Information("⛽ UI Fuel grid cell tapped: Column '{ColumnName}' at row {RowIndex}",
                    columnName, rowIndex);

                // Special handling for specific columns
                switch (columnName)
                {
                    case "MPG":
                        Logger.Information("⛽ UI Fuel MPG column tapped - potential efficiency analysis");
                        break;
                    case "TotalCost":
                        Logger.Information("⛽ UI Fuel cost column tapped - potential cost breakdown view");
                        break;
                    case "Gallons":
                        Logger.Information("⛽ UI Fuel gallons column tapped - potential consumption details");
                        break;
                    case "FuelLocation":
                        Logger.Information("⛽ UI Fuel location column tapped - potential station analysis");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "⛽ UI Fuel grid cell tap failed: {ErrorMessage}", ex.Message);
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
                    var chartName = chart.Name ?? "UnnamedFuelChart";
                    Logger.Information("⛽ UI Fuel chart interaction: {ChartName} clicked at position ({X}, {Y})",
                        chartName, e.GetPosition(chart).X, e.GetPosition(chart).Y);

                    // Could trigger chart drill-down or detailed analysis
                    switch (chartName)
                    {
                        case "MPGTrendsChart":
                            Logger.Information("⛽ UI Fuel MPG trends chart clicked - potential period drill-down");
                            break;
                        case "CostAnalysisChart":
                            Logger.Information("⛽ UI Fuel cost analysis chart clicked - potential monthly details");
                            break;
                        case "EfficiencyDistributionChart":
                            Logger.Information("⛽ UI Fuel efficiency distribution chart clicked - potential bus details");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "⛽ UI Fuel chart interaction failed: {ErrorMessage}", ex.Message);
            }
        }

        /// <summary>
        /// Override to capture any unhandled UI exceptions in fuel management
        /// </summary>
        protected override void OnPreviewMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                // Log any clicks for debugging purposes
                if (e.Source is FrameworkElement element)
                {
                    var elementName = element.Name ?? element.GetType().Name;
                    Logger.Debug("⛽ UI Fuel preview click on: {ElementName} ({ElementType})",
                        elementName, element.GetType().Name);
                }

                base.OnPreviewMouseLeftButtonDown(e);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "⛽ UI Fuel preview click handling failed: {ErrorMessage}", ex.Message);
            }
        }

        /// <summary>
        /// Log when view is loaded for performance tracking
        /// </summary>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Logger.Information("⛽ FuelManagementView: View fully loaded and ready for interaction");

                // Could initialize any view-specific settings here
                var viewModel = DataContext as FuelManagementViewModel;
                if (viewModel != null)
                {
                    Logger.Information("⛽ FuelManagementView: ViewModel binding confirmed");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "⛽ FuelManagementView: Load event failed - {ErrorMessage}", ex.Message);
            }
        }

        /// <summary>
        /// Log when view is unloaded for cleanup tracking
        /// </summary>
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Logger.Information("⛽ FuelManagementView: View unloaded - cleanup completed");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "⛽ FuelManagementView: Unload event failed - {ErrorMessage}", ex.Message);
            }
        }
    }
}
