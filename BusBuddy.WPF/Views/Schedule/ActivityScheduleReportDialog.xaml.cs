using BusBuddy.Core.Services;
using CoreInterfaces = BusBuddy.Core.Services.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace BusBuddy.WPF.Views.Schedule
{
    public partial class ActivityScheduleReportDialog : Window
    {
        private readonly CoreInterfaces.IActivityScheduleService _activityScheduleService;
        private ObservableCollection<StatsDataItem> _statsData = new();

        public ActivityScheduleReportDialog(CoreInterfaces.IActivityScheduleService activityScheduleService)
        {
            InitializeComponent();

            _activityScheduleService = activityScheduleService;

            // Set default date range (last 30 days)
            EndDatePicker.SelectedDate = DateTime.Today;
            StartDatePicker.SelectedDate = DateTime.Today.AddDays(-30);

            // Initialize data grid
            StatsDataGrid.ItemsSource = _statsData;
        }

        private async void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!StartDatePicker.SelectedDate.HasValue || !EndDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select both start and end dates.", "Date Selection Required",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (StartDatePicker.SelectedDate.Value > EndDatePicker.SelectedDate.Value)
            {
                MessageBox.Show("Start date must be before or equal to end date.", "Invalid Date Range",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                var startDate = StartDatePicker.SelectedDate.Value;
                var endDate = EndDatePicker.SelectedDate.Value;

                // Get statistics data
                var tripTypeStats = await _activityScheduleService.GetActivityScheduleStatisticsByTripTypeAsync(startDate, endDate);
                var driverStats = await _activityScheduleService.GetActivityScheduleStatisticsByDriverAsync(startDate, endDate);
                var vehicleStats = await _activityScheduleService.GetActivityScheduleStatisticsByVehicleAsync(startDate, endDate);
                var statusStats = await _activityScheduleService.GetActivityScheduleStatisticsByStatusAsync(startDate, endDate);

                // Update charts
                UpdateChart(TripTypeSeries, tripTypeStats);
                UpdateChart(DriverSeries, driverStats);
                UpdateChart(VehicleSeries, vehicleStats);
                UpdateChart(StatusSeries, statusStats);

                // Update data grid
                UpdateDataGrid(tripTypeStats, driverStats, vehicleStats, statusStats);

                // Switch to first tab to show results
                ReportTabControl.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating reports: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                System.Windows.Input.Mouse.OverrideCursor = null;
            }
        }

        private void UpdateChart<T>(T series, Dictionary<string, int> data) where T : Syncfusion.UI.Xaml.Charts.ChartSeries
        {
            var chartData = new ObservableCollection<KeyValuePair<string, int>>(
                data.OrderByDescending(kvp => kvp.Value));

            series.ItemsSource = chartData;
        }

        private void UpdateDataGrid(Dictionary<string, int> tripTypeStats, Dictionary<string, int> driverStats,
                                    Dictionary<string, int> vehicleStats, Dictionary<string, int> statusStats)
        {
            _statsData.Clear();

            // Calculate totals for percentage
            int tripTypeTotal = tripTypeStats.Values.Sum();
            int driverTotal = driverStats.Values.Sum();
            int vehicleTotal = vehicleStats.Values.Sum();
            int statusTotal = statusStats.Values.Sum();

            // Add trip type data
            foreach (var item in tripTypeStats.OrderByDescending(kvp => kvp.Value))
            {
                _statsData.Add(new StatsDataItem
                {
                    Category = "Trip Type",
                    Name = item.Key,
                    Count = item.Value,
                    Percentage = (double)item.Value / tripTypeTotal
                });
            }

            // Add driver data
            foreach (var item in driverStats.OrderByDescending(kvp => kvp.Value))
            {
                _statsData.Add(new StatsDataItem
                {
                    Category = "Driver",
                    Name = item.Key,
                    Count = item.Value,
                    Percentage = (double)item.Value / driverTotal
                });
            }

            // Add vehicle data
            foreach (var item in vehicleStats.OrderByDescending(kvp => kvp.Value))
            {
                _statsData.Add(new StatsDataItem
                {
                    Category = "Vehicle",
                    Name = item.Key,
                    Count = item.Value,
                    Percentage = (double)item.Value / vehicleTotal
                });
            }

            // Add status data
            foreach (var item in statusStats.OrderByDescending(kvp => kvp.Value))
            {
                _statsData.Add(new StatsDataItem
                {
                    Category = "Status",
                    Name = item.Key,
                    Count = item.Value,
                    Percentage = (double)item.Value / statusTotal
                });
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (_statsData.Count == 0)
            {
                MessageBox.Show("Please generate reports first.", "No Data",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = $"ActivityScheduleStats_{DateTime.Now:yyyyMMdd}.csv"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    // Build CSV content
                    var sb = new StringBuilder();
                    sb.AppendLine("Category,Name,Count,Percentage");

                    foreach (var item in _statsData)
                    {
                        sb.AppendLine($"\"{item.Category}\",\"{item.Name.Replace("\"", "\"\"")}\",{item.Count},{item.Percentage:P1}");
                    }

                    // Write to file
                    File.WriteAllText(saveDialog.FileName, sb.ToString());

                    MessageBox.Show($"Statistics exported to {saveDialog.FileName}", "Export Successful",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting data: {ex.Message}", "Export Failed",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    public class StatsDataItem
    {
        public string Category { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }
}
