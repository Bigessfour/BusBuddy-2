using Bus_Buddy.Models;
using Bus_Buddy.Services;
using Microsoft.Extensions.Logging;
using Syncfusion.WinForms.Controls;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Schedule;
using Syncfusion.Schedule;
using Syncfusion.Windows.Forms.Tools;
using System.ComponentModel;

namespace Bus_Buddy.Forms;

/// <summary>
/// Schedule Management Form - Enhanced implementation using local Syncfusion ScheduleControl
/// Integrates with Activity management for comprehensive bus scheduling
/// </summary>
public partial class ScheduleManagementForm : MetroForm
{
    #region Fields and Services
    private readonly IActivityService _activityService;
    private readonly IBusService _busService;
    private readonly ILogger<ScheduleManagementForm> _logger;
    private readonly BusBuddyScheduleDataProvider _scheduleDataProvider;

    // Data Collections
    private BindingList<Activity> _activities = null!;

    // Currently selected appointment for operations
    private BusBuddyScheduleAppointment? _selectedAppointment;

    // UI Components
    private Syncfusion.Windows.Forms.Tools.GradientPanel panelHeader = null!;
    private Syncfusion.Windows.Forms.Tools.GradientPanel panelControls = null!;
    private Syncfusion.Windows.Forms.Tools.GradientPanel panelSchedule = null!;
    private Syncfusion.Windows.Forms.Tools.AutoLabel labelTitle = null!;

    // Enhanced Schedule Control with proper data binding
    private ScheduleControl scheduleControl = null!;

    // Step 2: Control Buttons (to be added)
    private SfButton btnAddActivity = null!;
    private SfButton btnEditActivity = null!;
    private SfButton btnDeleteActivity = null!;
    private SfButton btnRefresh = null!;

    // Step 3: View Controls (to be added)
    private ComboBoxAdv cmbViewType = null!;
    private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv dtpNavigate = null!;

    #endregion

    #region Constructor
    public ScheduleManagementForm(
        IActivityService activityService,
        IBusService busService,
        ILogger<ScheduleManagementForm> logger,
        BusBuddyScheduleDataProvider scheduleDataProvider)
    {
        _activityService = activityService;
        _busService = busService;
        _logger = logger;
        _scheduleDataProvider = scheduleDataProvider;

        _logger.LogInformation("Initializing Schedule Management Form with Syncfusion Schedule integration");

        InitializeDataCollections();
        InitializeComponent();
        SetupLayout();
        InitializeScheduleControl();

        // Load data asynchronously after form is shown
        Load += async (s, e) => await LoadInitialDataAsync();
    }
    #endregion

    #region Step 1: Basic Setup
    private void InitializeDataCollections()
    {
        _activities = new BindingList<Activity>();
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        // Form Configuration
        Text = "Bus Buddy - Activity Schedule Management";
        Size = new Size(1400, 900);
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1000, 600);
        WindowState = FormWindowState.Maximized;

        // Syncfusion Styling
        this.MetroColor = Color.FromArgb(46, 204, 113);
        this.CaptionForeColor = Color.White;
        this.BackColor = Color.FromArgb(248, 249, 250);

        ResumeLayout(false);
        _logger.LogInformation("Form basic initialization completed");
    }

    private void SetupLayout()
    {
        try
        {
            CreateHeaderPanel();
            CreateControlsPanel();
            CreateSchedulePanel();

            _logger.LogInformation("Layout setup completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up layout");
            ShowError($"Layout setup failed: {ex.Message}");
        }
    }
    #endregion

    #region Step 2: Panel Creation
    private void CreateHeaderPanel()
    {
        panelHeader = new GradientPanel
        {
            Height = 70,
            Dock = DockStyle.Top,
            BackColor = Color.FromArgb(248, 249, 250),
            Padding = new Padding(20, 15, 20, 15)
        };

        labelTitle = new AutoLabel
        {
            Text = "📅 Activity Schedule Management",
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            ForeColor = Color.FromArgb(46, 204, 113),
            AutoSize = true,
            Location = new Point(20, 20)
        };

        panelHeader.Controls.Add(labelTitle);
        Controls.Add(panelHeader);
    }

    private void CreateControlsPanel()
    {
        panelControls = new GradientPanel
        {
            Height = 80,
            Dock = DockStyle.Top,
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(20, 15, 20, 15)
        };

        SetupControlButtons();
        Controls.Add(panelControls);
    }

    private void CreateSchedulePanel()
    {
        panelSchedule = new GradientPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(10)
        };

        Controls.Add(panelSchedule);
    }
    #endregion

    #region Step 3: Schedule Control Initialization
    private void InitializeScheduleControl()
    {
        try
        {
            _logger.LogInformation("Initializing Syncfusion Schedule Control with BusBuddyScheduleDataProvider");

            // Create the schedule control
            scheduleControl = new ScheduleControl
            {
                Dock = DockStyle.Fill,
                ScheduleType = ScheduleViewType.Month,
                DataSource = _scheduleDataProvider
            };

            // Configure basic properties
            ConfigureScheduleProperties();

            // Setup event handlers
            SetupScheduleEvents();

            // Add to container
            panelSchedule.Controls.Add(scheduleControl);

            _logger.LogInformation("Schedule control initialized successfully with data provider");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize schedule control");
            ShowError($"Schedule control initialization failed: {ex.Message}");
        }
    }

    private void ConfigureScheduleProperties()
    {
        try
        {
            // Configure culture and appearance
            scheduleControl.Culture = System.Globalization.CultureInfo.CurrentCulture;

            // Enable navigation panel with calendar
            scheduleControl.NavigationPanelFillWithCalendar = true;

            // Configure appearance if available
            if (scheduleControl.Appearance != null)
            {
                // Set colors for prime time cells
                scheduleControl.Appearance.PrimeTimeCellColor = Color.White;
                scheduleControl.Appearance.NonPrimeTimeCellColor = Color.FromArgb(248, 249, 250);
                scheduleControl.Appearance.CaptionBackColor = Color.FromArgb(46, 204, 113);
            }

            _logger.LogDebug("Schedule properties configured");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Some schedule properties not available in this version");
        }
    }

    private void SetupScheduleEvents()
    {
        try
        {
            // Event handlers for schedule interactions
            scheduleControl.MouseClick += ScheduleControl_MouseClick;

            // Add appointment click handler if available
            scheduleControl.ScheduleAppointmentClick += (sender, e) =>
            {
                try
                {
                    _logger.LogInformation("Appointment clicked: {Subject}", e.Item?.Subject);
                    // Handle appointment selection for editing
                    HandleAppointmentClick(e.Item);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling appointment click");
                }
            };

            _logger.LogInformation("Schedule event handlers configured");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring schedule events");
        }
    }
    #endregion

    #region Step 4: Control Buttons Setup
    private void SetupControlButtons()
    {
        // Add Activity Button
        btnAddActivity = new SfButton
        {
            Text = "➕ Add Activity",
            Size = new Size(130, 40),
            Location = new Point(20, 15),
            Style =
            {
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White
            }
        };
        btnAddActivity.Click += BtnAddActivity_Click;

        // Edit Activity Button  
        btnEditActivity = new SfButton
        {
            Text = "✏️ Edit Activity",
            Size = new Size(130, 40),
            Location = new Point(160, 15),
            Style =
            {
                BackColor = Color.FromArgb(241, 196, 15),
                ForeColor = Color.White
            }
        };
        btnEditActivity.Click += BtnEditActivity_Click;

        // Delete Activity Button
        btnDeleteActivity = new SfButton
        {
            Text = "🗑️ Delete",
            Size = new Size(100, 40),
            Location = new Point(300, 15),
            Style =
            {
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White
            }
        };
        btnDeleteActivity.Click += BtnDeleteActivity_Click;

        // Refresh Button
        btnRefresh = new SfButton
        {
            Text = "🔄 Refresh",
            Size = new Size(100, 40),
            Location = new Point(410, 15),
            Style =
            {
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White
            }
        };
        btnRefresh.Click += BtnRefresh_Click;

        // View Type Selector
        var lblView = new AutoLabel
        {
            Text = "View:",
            Location = new Point(530, 25),
            Size = new Size(40, 20),
            Font = new Font("Segoe UI", 9F)
        };

        cmbViewType = new ComboBoxAdv
        {
            Location = new Point(575, 20),
            Size = new Size(100, 30),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbViewType.Items.AddRange(new[] { "Day", "Week", "Month" });
        cmbViewType.SelectedIndex = 2; // Default to Month
        cmbViewType.SelectedIndexChanged += CmbViewType_SelectedIndexChanged;

        // Date Navigator
        var lblNavigate = new AutoLabel
        {
            Text = "Go to:",
            Location = new Point(690, 25),
            Size = new Size(45, 20),
            Font = new Font("Segoe UI", 9F)
        };

        dtpNavigate = new DateTimePickerAdv
        {
            Location = new Point(740, 20),
            Size = new Size(150, 30),
            Value = DateTime.Today
        };
        dtpNavigate.ValueChanged += DtpNavigate_ValueChanged;

        // Add all controls to panel
        panelControls.Controls.AddRange(new Control[]
        {
            btnAddActivity, btnEditActivity, btnDeleteActivity, btnRefresh,
            lblView, cmbViewType, lblNavigate, dtpNavigate
        });
    }
    #endregion

    #region Step 5: Data Loading and Management
    private async Task LoadInitialDataAsync()
    {
        try
        {
            _logger.LogInformation("Loading initial activity data");

            // Load activities for current month and surrounding dates
            var startDate = DateTime.Today.AddMonths(-1).AddDays(-DateTime.Today.Day + 1);
            var endDate = DateTime.Today.AddMonths(2);

            // Use the data provider to load activities
            await _scheduleDataProvider.LoadActivitiesAsync(startDate, endDate);

            // Refresh the schedule to show the new data
            scheduleControl.Refresh();

            _logger.LogInformation("Loaded activities into schedule");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading initial data");
            ShowError($"Failed to load activity data: {ex.Message}");
        }
    }

    private async void HandleAppointmentClick(Syncfusion.Schedule.IScheduleAppointment? appointment)
    {
        try
        {
            if (appointment is BusBuddyScheduleAppointment busAppointment)
            {
                // Track the selected appointment
                _selectedAppointment = busAppointment;

                _logger.LogInformation("Opening edit form for activity {ActivityId}", busAppointment.ActivityId);

                // Load the full activity from the database
                var activity = await _activityService.GetActivityByIdAsync(busAppointment.ActivityId);
                if (activity != null)
                {
                    // Create ActivityEditForm with the selected activity
                    var editForm = new ActivityEditForm(
                        _activityService,
                        _busService,
                        ServiceContainer.GetService<ILogger<ActivityEditForm>>(),
                        activity);

                    if (editForm.ShowDialog(this) == DialogResult.OK)
                    {
                        // Refresh the schedule to show updated data
                        await LoadInitialDataAsync();
                        ShowSuccess("Activity updated successfully");
                    }

                    editForm.Dispose();
                }
                else
                {
                    ShowError("Activity not found in database");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling appointment click");
            ShowError($"Failed to open activity for editing: {ex.Message}");
        }
    }
    #endregion

    #region Step 6: Event Handlers  
    private void BtnAddActivity_Click(object? sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Opening ActivityEditForm for new activity");

            using var form = ServiceContainer.GetService<ActivityEditForm>();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                // Refresh the schedule to show the new activity
                _ = Task.Run(async () =>
                {
                    await LoadInitialDataAsync();
                    Invoke(() => ShowSuccess("Activity added successfully"));
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening add activity form");
            ShowError($"Failed to open add activity form: {ex.Message}");
        }
    }

    private void BtnEditActivity_Click(object? sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Edit activity requested");

            // For now, show info until we implement selected appointment tracking
            ShowInfo("Please click on an appointment in the schedule to edit it, or use the appointment click feature.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in edit activity");
            ShowError($"Edit activity failed: {ex.Message}");
        }
    }

    private async void BtnDeleteActivity_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_selectedAppointment == null)
            {
                ShowInfo("Please click on an appointment in the schedule to select it for deletion.");
                return;
            }

            var result = Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                $"Are you sure you want to delete the activity '{_selectedAppointment.Subject}'?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _logger.LogInformation("Deleting activity {ActivityId}", _selectedAppointment.ActivityId);

                await _activityService.DeleteActivityAsync(_selectedAppointment.ActivityId);

                // Clear selection and refresh
                _selectedAppointment = null;
                await LoadInitialDataAsync();

                ShowSuccess("Activity deleted successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting activity");
            ShowError($"Delete activity failed: {ex.Message}");
        }
    }

    private async void BtnRefresh_Click(object? sender, EventArgs e)
    {
        try
        {
            // Clear existing activities and reload
            _activities.Clear();

            await LoadInitialDataAsync();
            ShowInfo("Schedule refreshed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing schedule");
            ShowError($"Refresh failed: {ex.Message}");
        }
    }

    private void CmbViewType_SelectedIndexChanged(object? sender, EventArgs e)
    {
        try
        {
            var selectedView = cmbViewType.SelectedItem?.ToString();
            scheduleControl.ScheduleType = selectedView switch
            {
                "Day" => ScheduleViewType.Day,
                "Week" => ScheduleViewType.Week,
                "Month" => ScheduleViewType.Month,
                _ => ScheduleViewType.Month
            };

            _logger.LogInformation("Schedule view changed to {View}", selectedView);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing schedule view");
            ShowError($"View change failed: {ex.Message}");
        }
    }

    private void DtpNavigate_ValueChanged(object? sender, EventArgs e)
    {
        try
        {
            // Navigate the schedule to the selected date
            // Note: Different Syncfusion versions may have different properties for navigation
            // Try common navigation properties
            if (HasProperty(scheduleControl, "CalendarDate"))
            {
                SetProperty(scheduleControl, "CalendarDate", dtpNavigate.Value);
            }
            else if (HasProperty(scheduleControl, "Date"))
            {
                SetProperty(scheduleControl, "Date", dtpNavigate.Value);
            }
            else if (HasProperty(scheduleControl, "CurrentDate"))
            {
                SetProperty(scheduleControl, "CurrentDate", dtpNavigate.Value);
            }

            _logger.LogInformation("Schedule navigated to {Date}", dtpNavigate.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating schedule");
            ShowError($"Navigation failed: {ex.Message}");
        }
    }

    private void ScheduleControl_MouseClick(object? sender, MouseEventArgs e)
    {
        try
        {
            // Handle schedule clicks for context menus or selection
            _logger.LogDebug("Schedule clicked at {Location}", e.Location);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling schedule click");
        }
    }
    #endregion

    #region Utility Methods
    private void ShowError(string message)
    {
        _logger.LogError("Schedule Management Error: {Message}", message);
        Syncfusion.Windows.Forms.MessageBoxAdv.Show(this, message, "Schedule Management Error",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void ShowInfo(string message)
    {
        _logger.LogInformation("Schedule Management Info: {Message}", message);
        Syncfusion.Windows.Forms.MessageBoxAdv.Show(this, message, "Schedule Management",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ShowSuccess(string message)
    {
        _logger.LogInformation("Schedule Management Success: {Message}", message);
        Syncfusion.Windows.Forms.MessageBoxAdv.Show(this, message, "Success",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static bool HasProperty(object obj, string propertyName)
    {
        return obj.GetType().GetProperty(propertyName) != null;
    }

    private static void SetProperty(object obj, string propertyName, object value)
    {
        var property = obj.GetType().GetProperty(propertyName);
        if (property != null && property.CanWrite)
        {
            property.SetValue(obj, value);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Clear selected appointment
            _selectedAppointment = null;
        }
        base.Dispose(disposing);
    }
    #endregion
}
