using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using CoreInterfaces = BusBuddy.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BusBuddy.WPF.Extensions;
using BusType = BusBuddy.Core.Models.Bus;
using DriverType = BusBuddy.Core.Models.Driver;

namespace BusBuddy.WPF.Views.Schedule
{
    public partial class ActivityScheduleDialog : Window
    {
        private readonly CoreInterfaces.IActivityScheduleService _activityScheduleService;
        private readonly IDriverService _driverService;
        private readonly CoreInterfaces.IBusService _busService;
        private readonly ActivitySchedule _schedule;
        private readonly bool _isNewSchedule;
        private ObservableCollection<ActivitySchedule> _conflicts = new();

        public ActivitySchedule Schedule => _schedule;

        public ActivityScheduleDialog(
            CoreInterfaces.IActivityScheduleService activityScheduleService,
            IDriverService driverService,
            CoreInterfaces.IBusService busService,
            ActivitySchedule? schedule = null)
        {
            InitializeComponent();

            _activityScheduleService = activityScheduleService;
            _driverService = driverService;
            _busService = busService;

            // Initialize or create a new schedule
            if (schedule != null)
            {
                _schedule = schedule;
                _isNewSchedule = false;
                Title = "Edit Activity Schedule";
            }
            else
            {
                _schedule = new ActivitySchedule
                {
                    ScheduledDate = DateTime.Today.AddDays(1),
                    TripType = "Sports Trip",
                    Status = "Scheduled",
                    CreatedDate = DateTime.UtcNow
                };
                _isNewSchedule = true;
                Title = "Add New Activity Schedule";
            }

            // Set up event handlers
            Loaded += ActivityScheduleDialog_Loaded;
            SaveButton.Click += SaveButton_Click;
            CancelButton.Click += CancelButton_Click;
            CheckDriverButton.Click += CheckDriverButton_Click;
            CheckVehicleButton.Click += CheckVehicleButton_Click;
            CheckConflictsButton.Click += CheckConflictsButton_Click;

            // Set up conflict items control
            ConflictsItemsControl.ItemsSource = _conflicts;
        }

        private async void ActivityScheduleDialog_Loaded(object sender, RoutedEventArgs e)
        {
            // Load drivers and vehicles
            await LoadDriversAsync();
            await LoadVehiclesAsync();

            // Populate form with schedule data
            PopulateForm();
        }

        private async Task LoadDriversAsync()
        {
            try
            {
                var drivers = await _driverService.GetActiveDriversAsync();
                DriverComboBox.ItemsSource = drivers;

                if (drivers.Any() && _isNewSchedule)
                {
                    // Default to first driver for new schedules
                    DriverComboBox.SelectedIndex = 0;
                }
                else if (!_isNewSchedule)
                {
                    // Set to existing driver for edit
                    DriverComboBox.SelectedValue = _schedule.ScheduledDriverId;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading drivers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadVehiclesAsync()
        {
            try
            {
                var vehicles = await _busService.GetActiveBusesAsync();
                VehicleComboBox.ItemsSource = vehicles;

                if (vehicles.Any() && _isNewSchedule)
                {
                    // Default to first vehicle for new schedules
                    VehicleComboBox.SelectedIndex = 0;
                }
                else if (!_isNewSchedule)
                {
                    // Set to existing vehicle for edit
                    VehicleComboBox.SelectedValue = _schedule.ScheduledVehicleId;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading vehicles: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopulateForm()
        {
            // Set date and times
            ScheduleDatePicker.SelectedDate = _schedule.ScheduledDate;

            // Set times using TimeSpan
            if (_schedule.ScheduledLeaveTime != default)
            {
                LeaveTimePicker.SetTime(_schedule.ScheduledLeaveTime);
            }
            else
            {
                LeaveTimePicker.SetTime(new TimeSpan(9, 0, 0)); // Default to 9:00 AM
            }

            if (_schedule.ScheduledEventTime != default)
            {
                EventTimePicker.SetTime(_schedule.ScheduledEventTime);
            }
            else
            {
                EventTimePicker.SetTime(new TimeSpan(15, 0, 0)); // Default to 3:00 PM
            }

            // Set trip type
            if (!string.IsNullOrEmpty(_schedule.TripType))
            {
                var tripTypeItem = TripTypeComboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Content.ToString() == _schedule.TripType);
                if (tripTypeItem != null)
                {
                    TripTypeComboBox.SelectedItem = tripTypeItem;
                }
                else
                {
                    TripTypeComboBox.SelectedIndex = 0; // Default to first item
                }
            }
            else
            {
                TripTypeComboBox.SelectedIndex = 0; // Default to first item
            }

            // Set other fields
            DestinationTextBox.Text = _schedule.ScheduledDestination;
            RidersUpDown.Value = _schedule.ScheduledRiders ?? 0;
            RequestedByTextBox.Text = _schedule.RequestedBy;
            NotesTextBox.Text = _schedule.Notes;

            // Set status (read-only for existing schedules)
            if (!string.IsNullOrEmpty(_schedule.Status))
            {
                var statusItem = StatusComboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Content.ToString() == _schedule.Status);
                if (statusItem != null)
                {
                    StatusComboBox.SelectedItem = statusItem;
                }
                else
                {
                    StatusComboBox.SelectedIndex = 0; // Default to "Scheduled"
                }
            }
            else
            {
                StatusComboBox.SelectedIndex = 0; // Default to "Scheduled"
            }

            // Enable status combobox only for existing schedules
            StatusComboBox.IsEnabled = !_isNewSchedule;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateForm())
            {
                try
                {
                    // Update schedule object from form
                    UpdateScheduleFromForm();

                    // Check for conflicts before saving
                    var hasConflicts = await _activityScheduleService.HasConflictsAsync(_schedule);
                    if (hasConflicts)
                    {
                        var result = MessageBox.Show(
                            "There are scheduling conflicts with the selected driver or vehicle. Do you want to save anyway?",
                            "Scheduling Conflict",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning);

                        if (result == MessageBoxResult.No)
                        {
                            return;
                        }
                    }

                    // Save the schedule
                    if (_isNewSchedule)
                    {
                        await _activityScheduleService.CreateActivityScheduleAsync(_schedule);
                    }
                    else
                    {
                        await _activityScheduleService.UpdateActivityScheduleAsync(_schedule);
                    }

                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving schedule: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void CheckDriverButton_Click(object sender, RoutedEventArgs e)
        {
            if (DriverComboBox.SelectedItem is DriverType selectedDriver &&
                ScheduleDatePicker.SelectedDate.HasValue &&
                LeaveTimePicker.GetTime().HasValue &&
                EventTimePicker.GetTime().HasValue)
            {
                try
                {
                    var isAvailable = await _activityScheduleService.IsDriverAvailableAsync(
                        selectedDriver.DriverId,
                        ScheduleDatePicker.SelectedDate!.Value,
                        LeaveTimePicker.GetTime()!.Value,
                        EventTimePicker.GetTime()!.Value);

                    if (isAvailable)
                    {
                        MessageBox.Show("The selected driver is available for this time slot.",
                            "Driver Availability", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("The selected driver is NOT available for this time slot. Please select another driver or change the time.",
                            "Driver Availability", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error checking driver availability: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a driver, date, and time first.", "Incomplete Information", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void CheckVehicleButton_Click(object sender, RoutedEventArgs e)
        {
            if (VehicleComboBox.SelectedItem is BusType selectedVehicle &&
                ScheduleDatePicker.SelectedDate.HasValue &&
                LeaveTimePicker.GetTime().HasValue &&
                EventTimePicker.GetTime().HasValue)
            {
                try
                {
                    var isAvailable = await _activityScheduleService.IsVehicleAvailableAsync(
                        selectedVehicle.VehicleId,
                        ScheduleDatePicker.SelectedDate!.Value,
                        LeaveTimePicker.GetTime()!.Value,
                        EventTimePicker.GetTime()!.Value);

                    if (isAvailable)
                    {
                        MessageBox.Show("The selected vehicle is available for this time slot.",
                            "Vehicle Availability", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("The selected vehicle is NOT available for this time slot. Please select another vehicle or change the time.",
                            "Vehicle Availability", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error checking vehicle availability: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a vehicle, date, and time first.", "Incomplete Information", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void CheckConflictsButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScheduleDatePicker.SelectedDate.HasValue &&
                LeaveTimePicker.GetTime().HasValue &&
                EventTimePicker.GetTime().HasValue)
            {
                try
                {
                    // Clear previous conflicts
                    _conflicts.Clear();

                    // Find conflicts for this time slot
                    var conflicts = await _activityScheduleService.FindScheduleConflictsAsync(
                        ScheduleDatePicker.SelectedDate!.Value,
                        LeaveTimePicker.GetTime()!.Value,
                        EventTimePicker.GetTime()!.Value,
                        _isNewSchedule ? null : _schedule.ActivityScheduleId);

                    if (conflicts.Any())
                    {
                        // Add conflicts to observable collection
                        foreach (var conflict in conflicts)
                        {
                            _conflicts.Add(conflict);
                        }

                        // Show the conflicts panel
                        ConflictsItemsControl.Visibility = Visibility.Visible;

                        // Show warning if conflicts involve current driver or vehicle
                        var driverId = ((DriverType)DriverComboBox.SelectedItem)?.DriverId ?? 0;
                        var vehicleId = ((BusType)VehicleComboBox.SelectedItem)?.VehicleId ?? 0;

                        if (conflicts.Any(c => c.ScheduledDriverId == driverId || c.ScheduledVehicleId == vehicleId))
                        {
                            ConflictWarningTextBlock.Text = "WARNING: There are conflicts with the selected driver or vehicle!";
                            ConflictWarningTextBlock.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            ConflictWarningTextBlock.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        ConflictsItemsControl.Visibility = Visibility.Collapsed;
                        ConflictWarningTextBlock.Visibility = Visibility.Collapsed;
                        MessageBox.Show("No scheduling conflicts found for this time slot.",
                            "Conflict Check", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error checking for conflicts: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a date and time first.", "Incomplete Information", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool ValidateForm()
        {
            // Basic validation
            if (!ScheduleDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Please select a date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                ScheduleDatePicker.Focus();
                return false;
            }

            if (TripTypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a trip type.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                TripTypeComboBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(DestinationTextBox.Text))
            {
                MessageBox.Show("Please enter a destination.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                DestinationTextBox.Focus();
                return false;
            }

            if (!LeaveTimePicker.GetTime().HasValue)
            {
                MessageBox.Show("Please select a leave time.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                LeaveTimePicker.Focus();
                return false;
            }

            if (!EventTimePicker.GetTime().HasValue)
            {
                MessageBox.Show("Please select an event time.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                EventTimePicker.Focus();
                return false;
            }

            if (LeaveTimePicker.GetTime()!.Value >= EventTimePicker.GetTime()!.Value)
            {
                MessageBox.Show("Leave time must be earlier than event time.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                LeaveTimePicker.Focus();
                return false;
            }

            if (DriverComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a driver.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                DriverComboBox.Focus();
                return false;
            }

            if (VehicleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a vehicle.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                VehicleComboBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(RequestedByTextBox.Text))
            {
                MessageBox.Show("Please enter who requested this trip.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                RequestedByTextBox.Focus();
                return false;
            }

            if (StatusComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a status.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                StatusComboBox.Focus();
                return false;
            }

            return true;
        }

        private void UpdateScheduleFromForm()
        {
            // Update schedule from form values
            _schedule.ScheduledDate = ScheduleDatePicker.SelectedDate!.Value;
            _schedule.TripType = ((ComboBoxItem)TripTypeComboBox.SelectedItem).Content.ToString()!;
            _schedule.ScheduledDestination = DestinationTextBox.Text.Trim();
            _schedule.ScheduledLeaveTime = LeaveTimePicker.GetTime()!.Value;
            _schedule.ScheduledEventTime = EventTimePicker.GetTime()!.Value;
            _schedule.ScheduledDriverId = ((DriverType)DriverComboBox.SelectedItem).DriverId;
            _schedule.ScheduledVehicleId = ((BusType)VehicleComboBox.SelectedItem).VehicleId;
            _schedule.ScheduledRiders = (int)(RidersUpDown.Value ?? 0);
            _schedule.RequestedBy = RequestedByTextBox.Text.Trim();
            _schedule.Status = ((ComboBoxItem)StatusComboBox.SelectedItem).Content.ToString()!;
            _schedule.Notes = NotesTextBox.Text.Trim();

            // Set update time for existing schedules
            if (!_isNewSchedule)
            {
                _schedule.UpdatedDate = DateTime.UtcNow;
                // In a real app, you might set UpdatedBy from the current user
                _schedule.UpdatedBy = "Current User";
            }
        }
    }
}
