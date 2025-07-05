using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.WinForms.Controls;
using Bus_Buddy.Models;
using Bus_Buddy.Services;
using Microsoft.Extensions.Logging;

namespace Bus_Buddy.Forms
{
    public partial class EnhancedScheduleManagementForm : MetroForm
    {
        private readonly IScheduleService _scheduleService;
        private readonly IBusService _busService;
        private readonly IActivityService _activityService;
        private readonly ILogger<EnhancedScheduleManagementForm> _logger;

        private Syncfusion.WinForms.DataGrid.SfDataGrid _scheduleGrid = null!;
        private SfButton _addButton = null!;
        private SfButton _editButton = null!;
        private SfButton _deleteButton = null!;
        private SfButton _refreshButton = null!;
        private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv _startDatePicker = null!;
        private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv _endDatePicker = null!;
        private Syncfusion.Windows.Forms.Tools.AutoLabel _dateRangeLabel = null!;
        private Syncfusion.Windows.Forms.Tools.GradientPanel _toolbarPanel = null!;
        private Syncfusion.Windows.Forms.Tools.GradientPanel _gridPanel = null!;

        public EnhancedScheduleManagementForm(
            IScheduleService scheduleService,
            IBusService busService,
            IActivityService activityService,
            ILogger<EnhancedScheduleManagementForm> logger)
        {
            _scheduleService = scheduleService;
            _busService = busService;
            _activityService = activityService;
            _logger = logger;

            _logger.LogInformation("Initializing Enhanced Schedule Management Form with Syncfusion integration");

            InitializeComponent();
            InitializeSyncfusionTheme();
            SetupLayout();
            ConfigureDataGrid();
            _ = LoadScheduleDataAsync(); // Fire and forget for constructor
        }

        /// <summary>
        /// Initialize Syncfusion theme and styling - Pattern from established forms
        /// </summary>
        private void InitializeSyncfusionTheme()
        {
            try
            {
                Syncfusion.Windows.Forms.SkinManager.SetVisualStyle(this,
                    Syncfusion.Windows.Forms.VisualTheme.Office2016Colorful);

                // Apply theme colors through MetroForm properties
                this.MetroColor = System.Drawing.Color.FromArgb(52, 152, 219);
                this.CaptionForeColor = System.Drawing.Color.White;
                this.CaptionBarHeight = 40;
                this.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
                this.AutoScaleMode = AutoScaleMode.Dpi;

                _logger.LogInformation("Office2016Colorful theme applied successfully");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not apply theme, using default");
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Enhanced Schedule Management";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            // Apply Syncfusion styling
            this.CaptionBarHeight = 40;
            this.MetroColor = Color.FromArgb(0, 120, 215);
            this.CaptionForeColor = Color.White;
            this.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        }

        private void SetupLayout()
        {
            // Create toolbar panel
            _toolbarPanel = new Syncfusion.Windows.Forms.Tools.GradientPanel
            {
                Height = 80,
                Dock = DockStyle.Top,
                BackgroundColor = new Syncfusion.Drawing.BrushInfo(Color.FromArgb(250, 250, 250)),
                BorderStyle = BorderStyle.None,
                Padding = new Padding(10)
            };

            // Create grid panel
            _gridPanel = new Syncfusion.Windows.Forms.Tools.GradientPanel
            {
                Dock = DockStyle.Fill,
                BackgroundColor = new Syncfusion.Drawing.BrushInfo(Color.White),
                BorderStyle = BorderStyle.None,
                Padding = new Padding(10)
            };

            CreateToolbarControls();

            this.Controls.Add(_gridPanel);
            this.Controls.Add(_toolbarPanel);
        }

        private void CreateToolbarControls()
        {
            // Date range label
            _dateRangeLabel = new Syncfusion.Windows.Forms.Tools.AutoLabel
            {
                Text = "📅 Date Range:",
                Location = new Point(10, 15),
                Size = new Size(95, 23),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 152, 219)
            };

            // Start date picker with Office2016Colorful styling
            _startDatePicker = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv
            {
                Location = new Point(115, 12),
                Size = new Size(130, 30),
                Value = DateTime.Today.AddMonths(-1),
                Format = DateTimePickerFormat.Short,
                Style = Syncfusion.Windows.Forms.VisualStyle.Office2016Colorful
            };
            _startDatePicker.ValueChanged += DatePicker_ValueChanged;

            // End date picker with Office2016Colorful styling
            _endDatePicker = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv
            {
                Location = new Point(255, 12),
                Size = new Size(130, 30),
                Value = DateTime.Today.AddMonths(1),
                Format = DateTimePickerFormat.Short,
                Style = Syncfusion.Windows.Forms.VisualStyle.Office2016Colorful
            };
            _endDatePicker.ValueChanged += DatePicker_ValueChanged;

            // Add Button - Enhanced styling
            _addButton = new SfButton
            {
                Text = "➕ Add Schedule",
                Size = new Size(140, 35),
                Location = new Point(10, 45),
                Style = {
                    BackColor = Color.FromArgb(46, 204, 113),
                    ForeColor = Color.White,
                    HoverBackColor = Color.FromArgb(39, 174, 96)
                }
            };
            _addButton.Click += AddButton_Click;

            // Edit Button - Enhanced styling
            _editButton = new SfButton
            {
                Text = "✏️ Edit Schedule",
                Size = new Size(140, 35),
                Location = new Point(160, 45),
                Style = {
                    BackColor = Color.FromArgb(241, 196, 15),
                    ForeColor = Color.White,
                    HoverBackColor = Color.FromArgb(212, 172, 13)
                }
            };
            _editButton.Click += EditButton_Click;

            // Delete Button - Enhanced styling
            _deleteButton = new SfButton
            {
                Text = "🗑️ Delete Schedule",
                Size = new Size(140, 35),
                Location = new Point(310, 45),
                Style = {
                    BackColor = Color.FromArgb(231, 76, 60),
                    ForeColor = Color.White,
                    HoverBackColor = Color.FromArgb(192, 57, 43)
                }
            };
            _deleteButton.Click += DeleteButton_Click;

            // Refresh Button - Enhanced styling
            _refreshButton = new SfButton
            {
                Text = "🔄 Refresh",
                Size = new Size(120, 35),
                Location = new Point(460, 45),
                Style = {
                    BackColor = Color.FromArgb(52, 152, 219),
                    ForeColor = Color.White,
                    HoverBackColor = Color.FromArgb(41, 128, 185)
                }
            };
            _refreshButton.Click += RefreshButton_Click;

            // Apply consistent hover effects and font styling
            ApplyButtonStyling(_addButton, _editButton, _deleteButton, _refreshButton);

            _toolbarPanel.Controls.AddRange(new Control[]
            {
                _dateRangeLabel, _startDatePicker, _endDatePicker,
                _addButton, _editButton, _deleteButton, _refreshButton
            });
        }

        /// <summary>
        /// Apply consistent button styling and hover effects
        /// </summary>
        private void ApplyButtonStyling(params SfButton[] buttons)
        {
            foreach (var button in buttons)
            {
                button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                button.FlatStyle = FlatStyle.Flat;

                // Basic styling - avoiding properties that may not be available
                _logger.LogDebug("Applied styling to button: {ButtonText}", button.Text);
            }
        }

        private void ConfigureDataGrid()
        {
            _scheduleGrid = new Syncfusion.WinForms.DataGrid.SfDataGrid
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowEditing = false,
                AllowDeleting = false,
                AllowSorting = true,
                AllowFiltering = true,
                SelectionMode = Syncfusion.WinForms.DataGrid.Enums.GridSelectionMode.Single,
                SelectionUnit = Syncfusion.WinForms.DataGrid.Enums.SelectionUnit.Row,
                BackColor = Color.White,
                HeaderRowHeight = 35
            };

            // Configure columns using Syncfusion grid column types
            _scheduleGrid.Columns.Add(new Syncfusion.WinForms.DataGrid.GridDateTimeColumn()
            {
                HeaderText = "📅 Date",
                MappingName = "ActivityDate",
                Width = 120,
                Format = "dd/MM/yyyy",
                AllowSorting = true
            });

            _scheduleGrid.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn()
            {
                HeaderText = "🚌 Activity",
                MappingName = "ActivityType",
                Width = 140,
                AllowSorting = true
            });

            _scheduleGrid.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn()
            {
                HeaderText = "🗺️ Route",
                MappingName = "Route.RouteName",
                Width = 180,
                AllowSorting = true
            });

            _scheduleGrid.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn()
            {
                HeaderText = "⏰ Start Time",
                MappingName = "StartTime",
                Width = 110,
                Format = @"hh\:mm",
                AllowSorting = true
            });

            _scheduleGrid.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn()
            {
                HeaderText = "⏱️ End Time",
                MappingName = "EndTime",
                Width = 110,
                Format = @"hh\:mm",
                AllowSorting = true
            });

            _scheduleGrid.Columns.Add(new Syncfusion.WinForms.DataGrid.GridNumericColumn()
            {
                HeaderText = "👥 Students",
                MappingName = "StudentsCount",
                Width = 90,
                AllowSorting = true
            });

            _scheduleGrid.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn()
            {
                HeaderText = "🚐 Bus Number",
                MappingName = "Vehicle.BusNumber",
                Width = 120,
                AllowSorting = true
            });

            _scheduleGrid.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn()
            {
                HeaderText = "👨‍✈️ Driver",
                MappingName = "Driver.DriverName",
                Width = 140,
                AllowSorting = true
            });

            // Apply Office2016Colorful styling - Enhanced pattern
            ApplyDataGridStyling();

            // Setup event handlers
            _scheduleGrid.SelectionChanged += ScheduleGrid_SelectionChanged;
            _scheduleGrid.CellClick += ScheduleGrid_CellDoubleClick;

            _gridPanel.Controls.Add(_scheduleGrid);
        }

        /// <summary>
        /// Apply comprehensive Office2016Colorful styling to data grid
        /// </summary>
        private void ApplyDataGridStyling()
        {
            try
            {
                // Header styling - Office2016Colorful theme
                _scheduleGrid.Style.HeaderStyle.BackColor = System.Drawing.Color.FromArgb(52, 152, 219);
                _scheduleGrid.Style.HeaderStyle.TextColor = System.Drawing.Color.White;
                _scheduleGrid.Style.HeaderStyle.Font.Bold = true;
                _scheduleGrid.Style.HeaderStyle.Font.Size = 9.5f;

                // Selection styling
                _scheduleGrid.Style.SelectionStyle.BackColor = System.Drawing.Color.FromArgb(52, 152, 219, 50);
                _scheduleGrid.Style.SelectionStyle.TextColor = System.Drawing.Color.FromArgb(52, 152, 219);

                // Border styling
                _scheduleGrid.Style.BorderColor = System.Drawing.Color.FromArgb(200, 200, 200);

                _logger.LogInformation("Data grid styling applied successfully");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Some data grid styling properties not available");
            }
        }

        private async Task LoadScheduleDataAsync()
        {
            try
            {
                // Show loading state if on UI thread
                if (this.InvokeRequired)
                {
                    this.Invoke(() => ShowLoadingState());
                }
                else
                {
                    ShowLoadingState();
                }

                _logger.LogInformation("Loading schedule data for date range {StartDate} to {EndDate}",
                    _startDatePicker.Value.Date, _endDatePicker.Value.Date);

                var activities = await _activityService.GetActivitiesByDateRangeAsync(
                    _startDatePicker.Value.Date,
                    _endDatePicker.Value.Date);

                // Update UI on UI thread
                if (this.InvokeRequired)
                {
                    this.Invoke(() => UpdateScheduleUI(activities));
                }
                else
                {
                    UpdateScheduleUI(activities);
                }

                _logger.LogInformation("Successfully loaded {Count} activities", activities.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading schedule data");

                if (this.InvokeRequired)
                {
                    this.Invoke(() => HandleLoadingError(ex));
                }
                else
                {
                    HandleLoadingError(ex);
                }
            }
        }

        /// <summary>
        /// Show loading state with visual feedback
        /// </summary>
        private void ShowLoadingState()
        {
            _refreshButton.Text = "🔄 Loading...";
            _refreshButton.Enabled = false;
            _addButton.Enabled = false;
            _editButton.Enabled = false;
            _deleteButton.Enabled = false;
        }

        /// <summary>
        /// Update the schedule UI with loaded data
        /// </summary>
        private void UpdateScheduleUI(IEnumerable<Activity> activities)
        {
            _scheduleGrid.DataSource = activities.ToList();
            _scheduleGrid.Refresh();

            // Reset button states
            _refreshButton.Text = "🔄 Refresh";
            _refreshButton.Enabled = true;
            _addButton.Enabled = true;

            // Update button states based on selection
            UpdateButtonStates();
        }

        /// <summary>
        /// Handle loading errors with proper UI feedback
        /// </summary>
        private void HandleLoadingError(Exception ex)
        {
            // Reset button states
            _refreshButton.Text = "🔄 Refresh";
            _refreshButton.Enabled = true;
            _addButton.Enabled = true;

            ShowError($"Error loading schedule data: {ex.Message}");
        }

        #region Utility Methods - Established Pattern from other forms

        /// <summary>
        /// Show error message using Syncfusion MessageBoxAdv
        /// </summary>
        private void ShowError(string message)
        {
            _logger.LogError("Schedule Management Error: {Message}", message);
            Syncfusion.Windows.Forms.MessageBoxAdv.Show(this, message, "Schedule Management Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Show info message using Syncfusion MessageBoxAdv
        /// </summary>
        private void ShowInfo(string message)
        {
            _logger.LogInformation("Schedule Management Info: {Message}", message);
            Syncfusion.Windows.Forms.MessageBoxAdv.Show(this, message, "Schedule Management",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Show success message using Syncfusion MessageBoxAdv
        /// </summary>
        private void ShowSuccess(string message)
        {
            _logger.LogInformation("Schedule Management Success: {Message}", message);
            Syncfusion.Windows.Forms.MessageBoxAdv.Show(this, message, "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Show confirmation dialog using Syncfusion MessageBoxAdv
        /// </summary>
        private DialogResult ShowConfirmation(string message, string title = "Confirm")
        {
            return Syncfusion.Windows.Forms.MessageBoxAdv.Show(this, message, title,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        #endregion

        private void UpdateButtonStates()
        {
            var hasSelection = _scheduleGrid.SelectedItems.Count > 0;
            _editButton.Enabled = hasSelection;
            _deleteButton.Enabled = hasSelection;
        }

        // Event Handlers - Enhanced with Syncfusion patterns
        private void AddButton_Click(object? sender, EventArgs e)
        {
            try
            {
                _logger.LogInformation("Opening ActivityEditForm for new activity");

                // Create ActivityEditForm with proper dependencies
                using var form = ServiceContainer.GetService<ActivityEditForm>();
                if (form != null && form.ShowDialog(this) == DialogResult.OK)
                {
                    // Refresh the schedule to show the new activity
                    _ = Task.Run(async () =>
                    {
                        await LoadScheduleDataAsync();
                        if (this.InvokeRequired)
                        {
                            this.Invoke(() => ShowSuccess("Activity added successfully"));
                        }
                        else
                        {
                            ShowSuccess("Activity added successfully");
                        }
                    });
                }
                else
                {
                    ShowInfo("Add Activity functionality requires proper service container setup. Please ensure ActivityEditForm is registered.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening add activity form");
                ShowError($"Failed to open add activity form: {ex.Message}");
            }
        }

        private void EditButton_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_scheduleGrid.SelectedItems.Count > 0)
                {
                    var selectedActivity = (Activity)_scheduleGrid.SelectedItem;
                    _logger.LogInformation("Editing activity {ActivityId}: {ActivityType}",
                        selectedActivity.ActivityId, selectedActivity.ActivityType);

                    ShowInfo($"Edit Activity functionality needs service container integration.\nSelected: {selectedActivity.ActivityType} on {selectedActivity.ActivityDate:dd/MM/yyyy}");
                }
                else
                {
                    ShowInfo("Please select an activity to edit.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in edit activity");
                ShowError($"Edit activity failed: {ex.Message}");
            }
        }

        private async void DeleteButton_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_scheduleGrid.SelectedItems.Count > 0)
                {
                    var selectedActivity = (Activity)_scheduleGrid.SelectedItem;

                    var result = ShowConfirmation(
                        $"Are you sure you want to delete the activity for {selectedActivity.ActivityType} on {selectedActivity.ActivityDate:dd/MM/yyyy}?",
                        "Confirm Delete");

                    if (result == DialogResult.Yes)
                    {
                        _logger.LogInformation("Deleting activity {ActivityId}", selectedActivity.ActivityId);

                        // Show loading state
                        ShowLoadingState();

                        await _activityService.DeleteActivityAsync(selectedActivity.ActivityId);
                        await LoadScheduleDataAsync();

                        ShowSuccess("Activity deleted successfully");
                    }
                }
                else
                {
                    ShowInfo("Please select an activity to delete.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting activity");
                ShowError($"Delete activity failed: {ex.Message}");
            }
        }

        private async void RefreshButton_Click(object? sender, EventArgs e)
        {
            try
            {
                _logger.LogInformation("Refreshing schedule data");
                await LoadScheduleDataAsync();
                ShowInfo("Schedule refreshed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing schedule");
                ShowError($"Refresh failed: {ex.Message}");
            }
        }

        private async void DatePicker_ValueChanged(object? sender, EventArgs e)
        {
            try
            {
                _logger.LogInformation("Date range changed, reloading data");
                await LoadScheduleDataAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling date change");
                ShowError($"Date change handling failed: {ex.Message}");
            }
        }

        private void ScheduleGrid_SelectionChanged(object? sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void ScheduleGrid_CellDoubleClick(object? sender, Syncfusion.WinForms.DataGrid.Events.CellClickEventArgs e)
        {
            try
            {
                if (_scheduleGrid.SelectedItem is Activity selectedActivity)
                {
                    _logger.LogInformation("Double-clicked activity {ActivityId}: {ActivityType}",
                        selectedActivity.ActivityId, selectedActivity.ActivityType);

                    ShowInfo($"Edit Activity functionality needs service container integration.\nSelected: {selectedActivity.ActivityType} on {selectedActivity.ActivityDate:dd/MM/yyyy}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling cell double-click");
                ShowError($"Cell double-click handling failed: {ex.Message}");
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            UpdateButtonStates();
            _logger.LogInformation("Enhanced Schedule Management Form loaded successfully");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _logger.LogInformation("Disposing Enhanced Schedule Management Form");
            }
            base.Dispose(disposing);
        }
    }
}
