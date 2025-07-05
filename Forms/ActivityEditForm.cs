using Bus_Buddy.Models;
using Bus_Buddy.Services;
using Microsoft.Extensions.Logging;
using Syncfusion.WinForms.Controls;
using Syncfusion.Windows.Forms;
using System.ComponentModel;

namespace Bus_Buddy.Forms;

/// <summary>
/// Form for adding/editing activities using Syncfusion components
/// </summary>
public partial class ActivityEditForm : Form
{
    private readonly IActivityService _activityService;
    private readonly IBusService _busService;
    private readonly ILogger _logger;
    private Activity? _activity;

    // Controls - Using Syncfusion Components
    private Syncfusion.Windows.Forms.Tools.AutoLabel labelTitle = null!;
    private Syncfusion.Windows.Forms.Tools.AutoLabel labelActivityDate = null!;
    private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv datePickerActivityDate = null!;
    private Syncfusion.Windows.Forms.Tools.AutoLabel labelActivityType = null!;
    private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxActivityType = null!;
    private Syncfusion.Windows.Forms.Tools.AutoLabel labelVehicle = null!;
    private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxVehicle = null!;
    private Syncfusion.Windows.Forms.Tools.AutoLabel labelRoute = null!;
    private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxRoute = null!;
    private Syncfusion.Windows.Forms.Tools.AutoLabel labelDriver = null!;
    private Syncfusion.Windows.Forms.Tools.ComboBoxAdv comboBoxDriver = null!;
    private Syncfusion.Windows.Forms.Tools.AutoLabel labelStartTime = null!;
    private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv timePickerStart = null!;
    private Syncfusion.Windows.Forms.Tools.AutoLabel labelEndTime = null!;
    private Syncfusion.Windows.Forms.Tools.DateTimePickerAdv timePickerEnd = null!;
    private Syncfusion.Windows.Forms.Tools.AutoLabel labelStudentsCount = null!;
    private Syncfusion.Windows.Forms.Tools.IntegerTextBox numericStudentsCount = null!;
    private Syncfusion.Windows.Forms.Tools.AutoLabel labelStartOdometer = null!;
    private Syncfusion.Windows.Forms.Tools.IntegerTextBox numericStartOdometer = null!;
    private Syncfusion.Windows.Forms.Tools.AutoLabel labelEndOdometer = null!;
    private Syncfusion.Windows.Forms.Tools.IntegerTextBox numericEndOdometer = null!;
    private Syncfusion.Windows.Forms.Tools.AutoLabel labelNotes = null!;
    private Syncfusion.Windows.Forms.Tools.TextBoxExt textBoxNotes = null!;
    private SfButton sfButtonSave = null!;
    private SfButton sfButtonCancel = null!;

    public ActivityEditForm(
        IActivityService activityService,
        IBusService busService,
        ILogger logger,
        Activity? activity = null)
    {
        _activityService = activityService;
        _busService = busService;
        _logger = logger;
        _activity = activity;

        InitializeComponent();
        LoadComboBoxData();

        if (_activity != null)
        {
            LoadActivityData();
        }
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        // Form settings
        Text = _activity == null ? "Add Activity - Bus Buddy" : "Edit Activity - Bus Buddy";
        Size = new Size(600, 700);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        int yPos = 20;
        const int leftMargin = 20;
        const int labelWidth = 120;
        const int controlWidth = 200;
        const int spacing = 35;

        // Title
        labelTitle = new Syncfusion.Windows.Forms.Tools.AutoLabel
        {
            Text = _activity == null ? "Add New Activity" : "Edit Activity",
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            ForeColor = Color.FromArgb(46, 204, 113),
            Location = new Point(leftMargin, yPos),
            Size = new Size(400, 25),
            BackColor = Color.Transparent
        };
        Controls.Add(labelTitle);
        yPos += 40;

        // Activity Date
        labelActivityDate = new Syncfusion.Windows.Forms.Tools.AutoLabel
        {
            Text = "Activity Date:",
            Location = new Point(leftMargin, yPos + 3),
            Size = new Size(labelWidth, 20),
            Font = new Font("Segoe UI", 9F),
            BackColor = Color.Transparent
        };
        Controls.Add(labelActivityDate);

        datePickerActivityDate = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv
        {
            Location = new Point(leftMargin + labelWidth + 10, yPos),
            Size = new Size(controlWidth, 23),
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Today,
            Style = Syncfusion.Windows.Forms.VisualStyle.Office2016Colorful
        };
        Controls.Add(datePickerActivityDate);
        yPos += spacing;

        // Activity Type
        labelActivityType = new Syncfusion.Windows.Forms.Tools.AutoLabel
        {
            Text = "Activity Type:",
            Location = new Point(leftMargin, yPos + 3),
            Size = new Size(labelWidth, 20),
            Font = new Font("Segoe UI", 9F),
            BackColor = Color.Transparent
        };
        Controls.Add(labelActivityType);

        comboBoxActivityType = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv
        {
            Location = new Point(leftMargin + labelWidth + 10, yPos),
            Size = new Size(controlWidth, 23),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Style = Syncfusion.Windows.Forms.VisualStyle.Office2016Colorful
        };
        comboBoxActivityType.Items.AddRange(new[] { "Morning", "Afternoon", "Field Trip" });
        Controls.Add(comboBoxActivityType);
        yPos += spacing;

        // Vehicle
        labelVehicle = new Syncfusion.Windows.Forms.Tools.AutoLabel
        {
            Text = "Vehicle:",
            Location = new Point(leftMargin, yPos + 3),
            Size = new Size(labelWidth, 20),
            Font = new Font("Segoe UI", 9F),
            BackColor = Color.Transparent
        };
        Controls.Add(labelVehicle);

        comboBoxVehicle = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv
        {
            Location = new Point(leftMargin + labelWidth + 10, yPos),
            Size = new Size(controlWidth, 23),
            DropDownStyle = ComboBoxStyle.DropDownList,
            DisplayMember = "BusNumber",
            ValueMember = "BusId",
            Style = Syncfusion.Windows.Forms.VisualStyle.Office2016Colorful
        };
        Controls.Add(comboBoxVehicle);
        yPos += spacing;

        // Route
        labelRoute = new Syncfusion.Windows.Forms.Tools.AutoLabel
        {
            Text = "Route:",
            Location = new Point(leftMargin, yPos + 3),
            Size = new Size(labelWidth, 20),
            Font = new Font("Segoe UI", 9F),
            BackColor = Color.Transparent
        };
        Controls.Add(labelRoute);

        comboBoxRoute = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv
        {
            Location = new Point(leftMargin + labelWidth + 10, yPos),
            Size = new Size(controlWidth, 23),
            DropDownStyle = ComboBoxStyle.DropDownList,
            DisplayMember = "RouteName",
            ValueMember = "RouteId",
            Style = Syncfusion.Windows.Forms.VisualStyle.Office2016Colorful
        };
        Controls.Add(comboBoxRoute);
        yPos += spacing;

        // Driver
        labelDriver = new Syncfusion.Windows.Forms.Tools.AutoLabel
        {
            Text = "Driver:",
            Location = new Point(leftMargin, yPos + 3),
            Size = new Size(labelWidth, 20),
            Font = new Font("Segoe UI", 9F),
            BackColor = Color.Transparent
        };
        Controls.Add(labelDriver);

        comboBoxDriver = new Syncfusion.Windows.Forms.Tools.ComboBoxAdv
        {
            Location = new Point(leftMargin + labelWidth + 10, yPos),
            Size = new Size(controlWidth, 23),
            DropDownStyle = ComboBoxStyle.DropDownList,
            DisplayMember = "DriverName",
            ValueMember = "DriverId",
            Style = Syncfusion.Windows.Forms.VisualStyle.Office2016Colorful
        };
        Controls.Add(comboBoxDriver);
        yPos += spacing;

        // Start Time
        labelStartTime = new Syncfusion.Windows.Forms.Tools.AutoLabel
        {
            Text = "Start Time:",
            Location = new Point(leftMargin, yPos + 3),
            Size = new Size(labelWidth, 20),
            Font = new Font("Segoe UI", 9F),
            BackColor = Color.Transparent
        };
        Controls.Add(labelStartTime);

        timePickerStart = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv
        {
            Location = new Point(leftMargin + labelWidth + 10, yPos),
            Size = new Size(controlWidth, 23),
            Format = DateTimePickerFormat.Time,
            ShowUpDown = true,
            Value = DateTime.Today.AddHours(7),
            Style = Syncfusion.Windows.Forms.VisualStyle.Office2016Colorful
        };
        Controls.Add(timePickerStart);
        yPos += spacing;

        // End Time
        labelEndTime = new Syncfusion.Windows.Forms.Tools.AutoLabel
        {
            Text = "End Time:",
            Location = new Point(leftMargin, yPos + 3),
            Size = new Size(labelWidth, 20),
            Font = new Font("Segoe UI", 9F),
            BackColor = Color.Transparent
        };
        Controls.Add(labelEndTime);

        timePickerEnd = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv
        {
            Location = new Point(leftMargin + labelWidth + 10, yPos),
            Size = new Size(controlWidth, 23),
            Format = DateTimePickerFormat.Time,
            ShowUpDown = true,
            Value = DateTime.Today.AddHours(8),
            Style = Syncfusion.Windows.Forms.VisualStyle.Office2016Colorful
        };
        Controls.Add(timePickerEnd);
        yPos += spacing;

        // Students Count
        labelStudentsCount = new Syncfusion.Windows.Forms.Tools.AutoLabel
        {
            Text = "Students Count:",
            Location = new Point(leftMargin, yPos + 3),
            Size = new Size(labelWidth, 20),
            Font = new Font("Segoe UI", 9F),
            BackColor = Color.Transparent
        };
        Controls.Add(labelStudentsCount);

        numericStudentsCount = new Syncfusion.Windows.Forms.Tools.IntegerTextBox
        {
            Location = new Point(leftMargin + labelWidth + 10, yPos),
            Size = new Size(controlWidth, 23),
            MinValue = 0,
            MaxValue = 99,
            IntegerValue = 0
        };
        Controls.Add(numericStudentsCount);
        yPos += spacing;

        // Start Odometer
        labelStartOdometer = new Syncfusion.Windows.Forms.Tools.AutoLabel
        {
            Text = "Start Odometer:",
            Location = new Point(leftMargin, yPos + 3),
            Size = new Size(labelWidth, 20),
            Font = new Font("Segoe UI", 9F),
            BackColor = Color.Transparent
        };
        Controls.Add(labelStartOdometer);

        numericStartOdometer = new Syncfusion.Windows.Forms.Tools.IntegerTextBox
        {
            Location = new Point(leftMargin + labelWidth + 10, yPos),
            Size = new Size(controlWidth, 23),
            MinValue = 0,
            MaxValue = 999999,
            IntegerValue = 0
        };
        Controls.Add(numericStartOdometer);
        yPos += spacing;

        // End Odometer
        labelEndOdometer = new Syncfusion.Windows.Forms.Tools.AutoLabel
        {
            Text = "End Odometer:",
            Location = new Point(leftMargin, yPos + 3),
            Size = new Size(labelWidth, 20),
            Font = new Font("Segoe UI", 9F),
            BackColor = Color.Transparent
        };
        Controls.Add(labelEndOdometer);

        numericEndOdometer = new Syncfusion.Windows.Forms.Tools.IntegerTextBox
        {
            Location = new Point(leftMargin + labelWidth + 10, yPos),
            Size = new Size(controlWidth, 23),
            MinValue = 0,
            MaxValue = 999999,
            IntegerValue = 0
        };
        Controls.Add(numericEndOdometer);
        yPos += spacing;

        // Notes
        labelNotes = new Syncfusion.Windows.Forms.Tools.AutoLabel
        {
            Text = "Notes:",
            Location = new Point(leftMargin, yPos + 3),
            Size = new Size(labelWidth, 20),
            Font = new Font("Segoe UI", 9F),
            BackColor = Color.Transparent
        };
        Controls.Add(labelNotes);

        textBoxNotes = new Syncfusion.Windows.Forms.Tools.TextBoxExt
        {
            Location = new Point(leftMargin + labelWidth + 10, yPos),
            Size = new Size(controlWidth + 150, 80),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            BorderStyle = BorderStyle.FixedSingle
        };
        Controls.Add(textBoxNotes);
        yPos += 100;

        // Buttons
        sfButtonSave = new SfButton
        {
            Text = "Save",
            Location = new Point(leftMargin + labelWidth + 10, yPos),
            Size = new Size(100, 35),
            Style = { BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White }
        };
        sfButtonSave.Click += SfButtonSave_Click;
        Controls.Add(sfButtonSave);

        sfButtonCancel = new SfButton
        {
            Text = "Cancel",
            Location = new Point(leftMargin + labelWidth + 120, yPos),
            Size = new Size(100, 35),
            Style = { BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White }
        };
        sfButtonCancel.Click += SfButtonCancel_Click;
        Controls.Add(sfButtonCancel);

        ResumeLayout(false);
    }

    private async void LoadComboBoxData()
    {
        try
        {
            // Load buses
            var buses = await _busService.GetAllBusesAsync();
            comboBoxVehicle.DataSource = buses.Where(b => b.Status == "Active").ToList();

            // Load routes (you'll need to implement IRouteService)
            // var routes = await _routeService.GetAllRoutesAsync();
            // comboBoxRoute.DataSource = routes.Where(r => r.Active).ToList();

            // Load drivers (you'll need to implement IDriverService)
            // var drivers = await _driverService.GetAllDriversAsync();
            // comboBoxDriver.DataSource = drivers.Where(d => d.Active).ToList();

            _logger.LogInformation("Combo box data loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading combo box data");
            MessageBoxAdv.Show($"Error loading data: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadActivityData()
    {
        if (_activity == null) return;

        try
        {
            datePickerActivityDate.Value = _activity.ActivityDate;
            comboBoxActivityType.SelectedItem = _activity.ActivityType;

            if (_activity.VehicleId > 0)
                comboBoxVehicle.SelectedValue = _activity.VehicleId;

            if (_activity.RouteId > 0)
                comboBoxRoute.SelectedValue = _activity.RouteId;

            if (_activity.DriverId > 0)
                comboBoxDriver.SelectedValue = _activity.DriverId;

            if (_activity.StartTime.HasValue)
                timePickerStart.Value = DateTime.Today.Add(_activity.StartTime.Value);

            if (_activity.EndTime.HasValue)
                timePickerEnd.Value = DateTime.Today.Add(_activity.EndTime.Value);

            numericStudentsCount.IntegerValue = _activity.StudentsCount ?? 0;
            numericStartOdometer.IntegerValue = _activity.StartOdometer ?? 0;
            numericEndOdometer.IntegerValue = _activity.EndOdometer ?? 0;
            textBoxNotes.Text = _activity.Notes ?? string.Empty;

            _logger.LogInformation("Activity data loaded for editing");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading activity data");
            MessageBoxAdv.Show($"Error loading activity data: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void SfButtonSave_Click(object? sender, EventArgs e)
    {
        try
        {
            if (!ValidateInput())
                return;

            var activity = _activity ?? new Activity();

            activity.ActivityDate = datePickerActivityDate.Value.Date;
            activity.ActivityType = comboBoxActivityType.SelectedItem?.ToString() ?? string.Empty;
            activity.VehicleId = (int)(comboBoxVehicle.SelectedValue ?? 0);
            activity.RouteId = (int)(comboBoxRoute.SelectedValue ?? 0);
            activity.DriverId = (int)(comboBoxDriver.SelectedValue ?? 0);
            activity.StartTime = timePickerStart.Value.TimeOfDay;
            activity.EndTime = timePickerEnd.Value.TimeOfDay;
            activity.StudentsCount = (int)numericStudentsCount.IntegerValue;
            activity.StartOdometer = (int)numericStartOdometer.IntegerValue;
            activity.EndOdometer = (int)numericEndOdometer.IntegerValue;
            activity.Notes = textBoxNotes.Text.Trim();

            if (_activity == null)
            {
                await _activityService.CreateActivityAsync(activity);
                MessageBoxAdv.Show("Activity created successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                await _activityService.UpdateActivityAsync(activity);
                MessageBoxAdv.Show("Activity updated successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving activity");
            MessageBoxAdv.Show($"Error saving activity: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SfButtonCancel_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrEmpty(comboBoxActivityType.SelectedItem?.ToString()))
        {
            MessageBoxAdv.Show("Please select an activity type.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            comboBoxActivityType.Focus();
            return false;
        }

        if (comboBoxVehicle.SelectedValue == null)
        {
            MessageBoxAdv.Show("Please select a vehicle.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            comboBoxVehicle.Focus();
            return false;
        }

        if (comboBoxRoute.SelectedValue == null)
        {
            MessageBoxAdv.Show("Please select a route.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            comboBoxRoute.Focus();
            return false;
        }

        if (comboBoxDriver.SelectedValue == null)
        {
            MessageBoxAdv.Show("Please select a driver.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            comboBoxDriver.Focus();
            return false;
        }

        if (timePickerEnd.Value <= timePickerStart.Value)
        {
            MessageBoxAdv.Show("End time must be after start time.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            timePickerEnd.Focus();
            return false;
        }

        if (numericEndOdometer.IntegerValue <= numericStartOdometer.IntegerValue)
        {
            MessageBoxAdv.Show("End odometer must be greater than start odometer.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            numericEndOdometer.Focus();
            return false;
        }

        return true;
    }
}
