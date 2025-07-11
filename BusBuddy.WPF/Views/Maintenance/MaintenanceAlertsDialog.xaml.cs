using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using BusBuddy.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace BusBuddy.WPF.Views.Maintenance
{
    public partial class MaintenanceAlertsDialog : Window, INotifyPropertyChanged
    {
        private readonly IMaintenanceService _maintenanceService;
        private readonly IBusService _busService;
        private readonly ILogger<MaintenanceAlertsDialog>? _logger;

        // Observable collections
        public ObservableCollection<MaintenanceAlertItem> OverdueItems { get; } = new();
        public ObservableCollection<MaintenanceAlertItem> UpcomingItems { get; } = new();
        public ObservableCollection<string> AlertFilters { get; } = new()
        {
            "All", "High Priority", "Next 7 Days", "Next 30 Days", "Scheduled Only"
        };

        // Selected items
        private MaintenanceAlertItem? _selectedOverdueItem;
        public MaintenanceAlertItem? SelectedOverdueItem
        {
            get => _selectedOverdueItem;
            set
            {
                _selectedOverdueItem = value;
                OnPropertyChanged();
            }
        }

        private MaintenanceAlertItem? _selectedUpcomingItem;
        public MaintenanceAlertItem? SelectedUpcomingItem
        {
            get => _selectedUpcomingItem;
            set
            {
                _selectedUpcomingItem = value;
                OnPropertyChanged();
            }
        }

        private string _selectedFilter = "All";
        public string SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                _selectedFilter = value;
                OnPropertyChanged();
                _ = LoadAlertsAsync();
            }
        }

        // Commands
        public BusBuddy.WPF.RelayCommand RefreshCommand { get; }
        public BusBuddy.WPF.RelayCommand CreateMaintenanceCommand { get; }
        public BusBuddy.WPF.RelayCommand ScheduleAlertCommand { get; }
        public BusBuddy.WPF.RelayCommand ExportCommand { get; }

        public MaintenanceAlertsDialog(IMaintenanceService maintenanceService, IBusService busService, ILogger<MaintenanceAlertsDialog>? logger = null)
        {
            _maintenanceService = maintenanceService ?? throw new ArgumentNullException(nameof(maintenanceService));
            _busService = busService ?? throw new ArgumentNullException(nameof(busService));
            _logger = logger;

            InitializeComponent();
            DataContext = this;

            // Initialize commands
            RefreshCommand = new BusBuddy.WPF.RelayCommand(_ => { _ = LoadAlertsAsync(); });
            CreateMaintenanceCommand = new BusBuddy.WPF.RelayCommand(_ => CreateNewMaintenance());
            ScheduleAlertCommand = new BusBuddy.WPF.RelayCommand(_ => ScheduleAlert());
            ExportCommand = new BusBuddy.WPF.RelayCommand(_ => ExportAlerts());

            // Load data
            _ = LoadAlertsAsync();
        }

        private async Task LoadAlertsAsync()
        {
            try
            {
                OverdueItems.Clear();
                UpcomingItems.Clear();

                // Get all maintenance records
                var records = await _maintenanceService.GetAllMaintenanceRecordsAsync();
                var now = DateTime.Now;

                // Get all buses for reference
                var buses = await _busService.GetAllBusesAsync();
                var busLookup = buses.ToDictionary(b => b.VehicleId, b => b.BusNumber);

                // Apply filters
                IEnumerable<BusBuddy.Core.Models.Maintenance> filteredRecords = records;

                switch (SelectedFilter)
                {
                    case "High Priority":
                        filteredRecords = records.Where(r => r.Priority == "High" || r.Priority == "Emergency");
                        break;
                    case "Next 7 Days":
                        filteredRecords = records.Where(r => r.NextServiceDue.HasValue &&
                            (r.NextServiceDue.Value - now).TotalDays <= 7);
                        break;
                    case "Next 30 Days":
                        filteredRecords = records.Where(r => r.NextServiceDue.HasValue &&
                            (r.NextServiceDue.Value - now).TotalDays <= 30);
                        break;
                    case "Scheduled Only":
                        filteredRecords = records.Where(r => r.Status == "Scheduled");
                        break;
                }

                // Process each record to create alert items
                foreach (var record in filteredRecords)
                {
                    if (record.NextServiceDue.HasValue)
                    {
                        var dueDate = record.NextServiceDue.Value;
                        var daysUntilDue = (dueDate - now).TotalDays;
                        var busNumber = busLookup.TryGetValue(record.VehicleId, out var num) ? num : "Unknown";

                        if (daysUntilDue < 0) // Overdue
                        {
                            OverdueItems.Add(new MaintenanceAlertItem
                            {
                                MaintenanceId = record.MaintenanceId,
                                BusNumber = busNumber,
                                MaintenanceType = record.MaintenanceCompleted,
                                DueDate = dueDate,
                                DaysOverdue = Math.Abs((int)daysUntilDue),
                                Status = record.Status,
                                Priority = record.Priority
                            });
                        }
                        else if (daysUntilDue <= 30) // Upcoming within 30 days
                        {
                            UpcomingItems.Add(new MaintenanceAlertItem
                            {
                                MaintenanceId = record.MaintenanceId,
                                BusNumber = busNumber,
                                MaintenanceType = record.MaintenanceCompleted,
                                DueDate = dueDate,
                                DaysUntilDue = (int)daysUntilDue,
                                Status = record.Status,
                                Priority = record.Priority
                            });
                        }
                    }
                }

                // Sort by priority and days
                var overdueList = OverdueItems.ToList();
                overdueList.Sort((a, b) =>
                {
                    // First by priority
                    var priorityCompare = ComparePriority(b.Priority, a.Priority);
                    if (priorityCompare != 0) return priorityCompare;

                    // Then by days overdue
                    return b.DaysOverdue.CompareTo(a.DaysOverdue);
                });

                OverdueItems.Clear();
                foreach (var item in overdueList)
                    OverdueItems.Add(item);

                var upcomingList = UpcomingItems.ToList();
                upcomingList.Sort((a, b) =>
                {
                    // First by priority
                    var priorityCompare = ComparePriority(b.Priority, a.Priority);
                    if (priorityCompare != 0) return priorityCompare;

                    // Then by days until due
                    return a.DaysUntilDue.CompareTo(b.DaysUntilDue);
                });

                UpcomingItems.Clear();
                foreach (var item in upcomingList)
                    UpcomingItems.Add(item);

                _logger?.LogInformation("Loaded {OverdueCount} overdue and {UpcomingCount} upcoming maintenance items",
                    OverdueItems.Count, UpcomingItems.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading maintenance alerts");
                MessageBox.Show($"Error loading maintenance alerts: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int ComparePriority(string a, string b)
        {
            // Return priority value with Emergency > High > Normal > Low
            int GetPriorityValue(string priority)
            {
                return priority switch
                {
                    "Emergency" => 3,
                    "High" => 2,
                    "Normal" => 1,
                    "Low" => 0,
                    _ => -1
                };
            }

            return GetPriorityValue(a).CompareTo(GetPriorityValue(b));
        }

        private void CreateNewMaintenance()
        {
            try
            {
                // Close this dialog with true result to indicate new maintenance should be created
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating new maintenance from alerts");
                MessageBox.Show($"Error creating new maintenance: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ScheduleAlert()
        {
            MessageBox.Show("Alert scheduling will be implemented in the next sprint.",
                "Coming Soon", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportAlerts()
        {
            try
            {
                // Create a Save File Dialog
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Maintenance_Alerts_{DateTime.Now:yyyyMMdd}",
                    DefaultExt = ".csv",
                    Filter = "CSV Files (*.csv)|*.csv"
                };

                var result = dialog.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    // Export data to CSV
                    using var writer = new System.IO.StreamWriter(dialog.FileName);

                    // Write header
                    writer.WriteLine("Alert Type,Bus Number,Maintenance Type,Due Date,Days Overdue/Until Due,Status,Priority");

                    // Write overdue items
                    foreach (var item in OverdueItems)
                    {
                        writer.WriteLine(
                            $"Overdue," +
                            $"\"{item.BusNumber}\"," +
                            $"\"{item.MaintenanceType}\"," +
                            $"{item.DueDate:yyyy-MM-dd}," +
                            $"{item.DaysOverdue}," +
                            $"\"{item.Status}\"," +
                            $"\"{item.Priority}\"");
                    }

                    // Write upcoming items
                    foreach (var item in UpcomingItems)
                    {
                        writer.WriteLine(
                            $"Upcoming," +
                            $"\"{item.BusNumber}\"," +
                            $"\"{item.MaintenanceType}\"," +
                            $"{item.DueDate:yyyy-MM-dd}," +
                            $"{item.DaysUntilDue}," +
                            $"\"{item.Status}\"," +
                            $"\"{item.Priority}\"");
                    }

                    MessageBox.Show($"Successfully exported {OverdueItems.Count + UpcomingItems.Count} maintenance alerts to {dialog.FileName}",
                        "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                    _logger?.LogInformation("Exported {AlertCount} maintenance alerts to CSV", OverdueItems.Count + UpcomingItems.Count);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting maintenance alerts");
                MessageBox.Show($"Error exporting alerts: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MaintenanceAlertItem
    {
        public int MaintenanceId { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public string MaintenanceType { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int DaysOverdue { get; set; }
        public int DaysUntilDue { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
    }
}
