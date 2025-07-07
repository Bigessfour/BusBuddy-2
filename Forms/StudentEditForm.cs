using Bus_Buddy.Models;
using Bus_Buddy.Services;
using Microsoft.Extensions.Logging;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.ListView;
using Syncfusion.WinForms.ListView.Enums;
using System.ComponentModel.DataAnnotations;

namespace Bus_Buddy.Forms;

/// <summary>
/// Student Add/Edit Form for creating and editing student records
/// Demonstrates comprehensive data entry form with validation using Syncfusion controls
/// </summary>
public partial class StudentEditForm : MetroForm
{
    #region Fields and Services
    private readonly ILogger<StudentEditForm> _logger;
    private readonly IStudentService _studentService;
    private readonly IBusService _busService;
    private readonly Student? _existingStudent;
    private readonly bool _isEditMode;

    // Form result properties
    public Student? EditedStudent { get; private set; }
    public bool IsDataSaved { get; private set; }

    // Available data for dropdowns
    private List<Route> _routes = null!;

    #endregion

    #region Constructor
    public StudentEditForm(
        ILogger<StudentEditForm> logger,
        IStudentService studentService,
        IBusService busService,
        Student? student = null)
    {
        _logger = logger;
        _studentService = studentService;
        _busService = busService;
        _existingStudent = student;
        _isEditMode = student != null;

        _logger.LogInformation(_isEditMode ?
            "Initializing StudentEditForm for editing student {StudentId}" :
            "Initializing StudentEditForm for new student",
            student?.StudentId);

        InitializeComponent(); // This will now call the method from the .Designer.cs file
        InitializeSyncfusionTheme();

        if (_isEditMode && _existingStudent != null)
        {
            Text = $"Edit Student - {_existingStudent.StudentName}";
        }
        else
        {
            Text = "Add New Student";
        }

        // Load data asynchronously after form is shown
        Load += async (s, e) => await LoadInitialDataAsync();
    }

    private void InitializeSyncfusionTheme()
    {
        try
        {
            // Apply Syncfusion theme integration
            Syncfusion.Windows.Forms.SkinManager.SetVisualStyle(this, Syncfusion.Windows.Forms.VisualTheme.Office2016Colorful);

            // Configure MetroForm properties
            this.MetroColor = System.Drawing.Color.FromArgb(52, 152, 219);
            this.CaptionBarColor = System.Drawing.Color.FromArgb(52, 152, 219);
            this.CaptionForeColor = System.Drawing.Color.White;

            // Enable high DPI scaling for this form
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

            _logger.LogInformation("Syncfusion theme applied successfully to Student Edit Form");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not apply Office2016 theme to Student Edit Form, using default styling");
        }
    }
    #endregion

    #region Initialization
    /*
    private void InitializeComponent()
    {
        SuspendLayout();

        // Form Configuration
        Text = _isEditMode ? $"Edit Student - {_existingStudent?.StudentName}" : "Add New Student";
        Size = new Size(900, 700);
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(850, 650);
        MaximizeBox = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;

        // Syncfusion MetroForm Styling
        BorderColor = Color.FromArgb(52, 152, 219);
        CaptionBarColor = Color.FromArgb(52, 152, 219);
        CaptionForeColor = Color.White;
        BackColor = Color.FromArgb(248, 249, 250);

        ResumeLayout(false);
        _logger.LogInformation("StudentEditForm basic initialization completed");
    }
    */

    private void SetupLayout()
    {
        try
        {
            CreatePersonalInfoPanel();
            CreateContactInfoPanel();
            CreateTransportationPanel();
            CreateNotesPanel();
            CreateButtonPanel();

            _logger.LogInformation("StudentEditForm layout setup completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up StudentEditForm layout");
            ShowError($"Layout setup failed: {ex.Message}");
        }
    }
    #endregion

    #region Panel Creation
    private void CreatePersonalInfoPanel()
    {
        panelPersonal = new Syncfusion.Windows.Forms.Tools.GradientPanel
        {
            Location = new Point(20, 20),
            Size = new Size(420, 280),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            BorderSingle = ButtonBorderStyle.Solid
        };

        lblPersonalInfo = new Syncfusion.Windows.Forms.Tools.AutoLabel
        {
            Text = "👤 Personal Information",
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = Color.FromArgb(52, 152, 219),
            Location = new Point(10, 10),
            Size = new Size(200, 25),
            BackColor = Color.Transparent
        };

        // Student Number
        var lblStudentNumber = CreateLabel("Student Number:", 10, 50);
        txtStudentNumber = CreateTextBox(140, 45, 250, "Enter student number (optional)");

        // Student Name (Required)
        var lblStudentName = CreateLabel("Student Name: *", 10, 85);
        txtStudentName = CreateTextBox(140, 80, 250, "Enter full student name");

        // Grade
        var lblGrade = CreateLabel("Grade:", 10, 120);
        cmbGrade = CreateComboBox(140, 115, 120);
        PopulateGradeDropdown();

        // School
        var lblSchool = CreateLabel("School:", 10, 155);
        txtSchool = CreateTextBox(140, 150, 250, "Enter school name");

        // Enrollment Date
        var lblEnrollmentDate = CreateLabel("Enrollment Date:", 10, 190);
        dtpEnrollmentDate = new Syncfusion.Windows.Forms.Tools.DateTimePickerAdv
        {
            Location = new Point(140, 185),
            Size = new Size(150, 30),
            Value = DateTime.Today,
            Format = DateTimePickerFormat.Short,
            Style = Syncfusion.Windows.Forms.VisualStyle.Office2016Colorful
        };

        // Active Status
        chkActive = new CheckBoxAdv
        {
            Text = "Active Student",
            Location = new Point(10, 225),
            Size = new Size(150, 25),
            Checked = true,
            ForeColor = Color.FromArgb(46, 204, 113)
        };

        panelPersonal.Controls.AddRange(new Control[]
        {
            lblPersonalInfo, lblStudentNumber, txtStudentNumber, lblStudentName, txtStudentName,
            lblGrade, cmbGrade, lblSchool, txtSchool, lblEnrollmentDate, dtpEnrollmentDate, chkActive
        });

        Controls.Add(panelPersonal);
    }

    private void CreateContactInfoPanel()
    {
        panelContact = new Syncfusion.Windows.Forms.Tools.GradientPanel
        {
            Location = new Point(460, 20),
            Size = new Size(420, 280),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            BorderSingle = ButtonBorderStyle.Solid
        };

        lblContactInfo = new Syncfusion.Windows.Forms.Tools.AutoLabel
        {
            Text = "📞 Contact Information",
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = Color.FromArgb(52, 152, 219),
            Location = new Point(10, 10),
            Size = new Size(200, 25),
            BackColor = Color.Transparent
        };

        // Home Address
        var lblHomeAddress = CreateLabel("Home Address:", 10, 50);
        txtHomeAddress = CreateTextBox(140, 45, 250, "Enter home address");

        // City, State, ZIP
        var lblCity = CreateLabel("City:", 10, 85);
        txtCity = CreateTextBox(50, 80, 120, "City");

        var lblState = CreateLabel("State:", 180, 85);
        txtState = CreateTextBox(220, 80, 50, "TX");

        var lblZip = CreateLabel("ZIP:", 280, 85);
        txtZip = CreateTextBox(310, 80, 80, "12345");

        // Home Phone
        var lblHomePhone = CreateLabel("Home Phone:", 10, 120);
        txtHomePhone = CreateTextBox(140, 115, 150, "(555) 123-4567");

        // Parent/Guardian
        var lblParentGuardian = CreateLabel("Parent/Guardian:", 10, 155);
        txtParentGuardian = CreateTextBox(140, 150, 250, "Enter parent or guardian name");

        // Emergency Phone
        var lblEmergencyPhone = CreateLabel("Emergency Phone:", 10, 190);
        txtEmergencyPhone = CreateTextBox(140, 185, 150, "(555) 987-6543");

        panelContact.Controls.AddRange(new Control[]
        {
            lblContactInfo, lblHomeAddress, txtHomeAddress, lblCity, txtCity, lblState, txtState,
            lblZip, txtZip, lblHomePhone, txtHomePhone, lblParentGuardian, txtParentGuardian,
            lblEmergencyPhone, txtEmergencyPhone
        });

        Controls.Add(panelContact);
    }

    private void CreateTransportationPanel()
    {
        panelTransport = new Syncfusion.Windows.Forms.Tools.GradientPanel
        {
            Location = new Point(20, 320),
            Size = new Size(420, 200),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            BorderSingle = ButtonBorderStyle.Solid
        };

        lblTransportInfo = new Syncfusion.Windows.Forms.Tools.AutoLabel
        {
            Text = "🚌 Transportation Information",
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = Color.FromArgb(52, 152, 219),
            Location = new Point(10, 10),
            Size = new Size(250, 25),
            BackColor = Color.Transparent
        };

        // AM Route
        var lblAMRoute = CreateLabel("AM Route:", 10, 50);
        cmbAMRoute = CreateComboBox(100, 45, 150);

        // PM Route
        var lblPMRoute = CreateLabel("PM Route:", 260, 50);
        cmbPMRoute = CreateComboBox(330, 45, 150);

        // Bus Stop
        var lblBusStop = CreateLabel("Bus Stop:", 10, 85);
        txtBusStop = CreateTextBox(100, 80, 280, "Enter bus stop location");

        // Transportation Notes
        var lblTransportNotes = CreateLabel("Transportation Notes:", 10, 120);
        txtTransportationNotes = new Syncfusion.Windows.Forms.Tools.TextBoxExt
        {
            Location = new Point(10, 145),
            Size = new Size(380, 45),
            Multiline = true,
            Font = new Font("Segoe UI", 9F),
            BorderStyle = BorderStyle.FixedSingle
        };

        panelTransport.Controls.AddRange(new Control[]
        {
            lblTransportInfo, lblAMRoute, cmbAMRoute, lblPMRoute, cmbPMRoute,
            lblBusStop, txtBusStop, lblTransportNotes, txtTransportationNotes
        });

        Controls.Add(panelTransport);
    }

    private void CreateNotesPanel()
    {
        panelNotes = new Syncfusion.Windows.Forms.Tools.GradientPanel
        {
            Location = new Point(460, 320),
            Size = new Size(420, 200),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            BorderSingle = ButtonBorderStyle.Solid
        };

        lblNotesInfo = new Syncfusion.Windows.Forms.Tools.AutoLabel
        {
            Text = "📋 Medical & Additional Notes",
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            ForeColor = Color.FromArgb(52, 152, 219),
            Location = new Point(10, 10),
            Size = new Size(250, 25),
            BackColor = Color.Transparent
        };

        // Medical Notes
        var lblMedicalNotes = CreateLabel("Medical Notes:", 10, 50);
        txtMedicalNotes = new Syncfusion.Windows.Forms.Tools.TextBoxExt
        {
            Location = new Point(10, 75),
            Size = new Size(380, 110),
            Multiline = true,
            Font = new Font("Segoe UI", 9F),
            BorderStyle = BorderStyle.FixedSingle
        };

        panelNotes.Controls.AddRange(new Control[]
        {
            lblNotesInfo, lblMedicalNotes, txtMedicalNotes
        });

        Controls.Add(panelNotes);
    }

    private void CreateButtonPanel()
    {
        panelButtons = new Syncfusion.Windows.Forms.Tools.GradientPanel
        {
            Location = new Point(20, 540),
            Size = new Size(860, 60),
            BackColor = Color.FromArgb(248, 249, 250),
            BorderStyle = BorderStyle.None
        };

        // Validate Button
        btnValidate = new SfButton
        {
            Text = "✓ Validate",
            Size = new Size(120, 40),
            Location = new Point(540, 10),
            Style =
            {
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White
            }
        };
        btnValidate.Click += BtnValidate_Click;

        // Save Button
        btnSave = new SfButton
        {
            Text = _isEditMode ? "💾 Update Student" : "💾 Save Student",
            Size = new Size(140, 40),
            Location = new Point(670, 10),
            Style =
            {
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White
            }
        };
        btnSave.Click += BtnSave_Click;

        // Cancel Button
        btnCancel = new SfButton
        {
            Text = "✖ Cancel",
            Size = new Size(100, 40),
            Location = new Point(20, 10),
            Style =
            {
                BackColor = Color.FromArgb(189, 195, 199),
                ForeColor = Color.White
            }
        };
        btnCancel.Click += BtnCancel_Click;

        panelButtons.Controls.AddRange(new Control[]
        {
            btnCancel, btnValidate, btnSave
        });

        Controls.Add(panelButtons);
    }
    #endregion

    #region Helper Methods for Control Creation
    private Syncfusion.Windows.Forms.Tools.AutoLabel CreateLabel(string text, int x, int y)
    {
        return new Syncfusion.Windows.Forms.Tools.AutoLabel
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(125, 20),
            Font = new Font("Segoe UI", 9F),
            ForeColor = Color.FromArgb(85, 85, 85),
            BackColor = Color.Transparent
        };
    }

    private Syncfusion.Windows.Forms.Tools.TextBoxExt CreateTextBox(int x, int y, int width, string placeholder = "")
    {
        var textBox = new Syncfusion.Windows.Forms.Tools.TextBoxExt
        {
            Location = new Point(x, y),
            Size = new Size(width, 30),
            Font = new Font("Segoe UI", 9F),
            BorderStyle = BorderStyle.FixedSingle
        };

        if (!string.IsNullOrEmpty(placeholder))
        {
            // Note: TextBoxExt doesn't have built-in placeholder support
            // We'll handle this through tooltip or other means if needed
            textBox.Text = "";
        }

        return textBox;
    }

    private SfComboBox CreateComboBox(int x, int y, int width)
    {
        return new SfComboBox
        {
            Location = new Point(x, y),
            Size = new Size(width, 30),
            DropDownStyle = DropDownStyle.DropDownList,
            Font = new Font("Segoe UI", 9F)
        };
    }

    private void PopulateGradeDropdown()
    {
        var grades = new List<string> { "", "Pre-K", "K" };
        for (int i = 1; i <= 12; i++)
        {
            grades.Add(i.ToString());
        }
        cmbGrade.DataSource = grades;
    }
    #endregion

    #region Data Loading and Population
    private async Task LoadInitialDataAsync()
    {
        try
        {
            _logger.LogInformation("Loading initial data for StudentEditForm");

            // Load routes for dropdowns
            await LoadRoutesAsync();

            // Populate form if editing existing student
            if (_isEditMode && _existingStudent != null)
            {
                PopulateFormFromStudent(_existingStudent);
            }

            _logger.LogInformation("StudentEditForm initial data loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading initial data for StudentEditForm");
            ShowError($"Failed to load form data: {ex.Message}");
        }
    }

    private async Task LoadRoutesAsync()
    {
        try
        {
            _routes = await _busService.GetAllRouteEntitiesAsync();

            var routeNames = new List<string> { "" };
            routeNames.AddRange(_routes.Select(r => r.RouteName));

            // Populate route dropdowns
            cmbAMRoute.DataSource = new List<string>(routeNames);
            cmbPMRoute.DataSource = new List<string>(routeNames);

            _logger.LogInformation("Loaded {Count} routes for dropdowns", _routes.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading routes");
            ShowError($"Failed to load routes: {ex.Message}");
        }
    }

    private void PopulateFormFromStudent(Student student)
    {
        try
        {
            txtStudentNumber.Text = student.StudentNumber ?? "";
            txtStudentName.Text = student.StudentName;

            // Set grade dropdown
            cmbGrade.SelectedItem = student.Grade ?? "";

            txtSchool.Text = student.School ?? "";
            dtpEnrollmentDate.Value = student.EnrollmentDate ?? DateTime.Today;
            chkActive.Checked = student.Active;

            // Contact information
            txtHomeAddress.Text = student.HomeAddress ?? "";
            txtCity.Text = student.City ?? "";
            txtState.Text = student.State ?? "";
            txtZip.Text = student.Zip ?? "";
            txtHomePhone.Text = student.HomePhone ?? "";
            txtParentGuardian.Text = student.ParentGuardian ?? "";
            txtEmergencyPhone.Text = student.EmergencyPhone ?? "";

            // Transportation
            cmbAMRoute.SelectedItem = student.AMRoute ?? "";
            cmbPMRoute.SelectedItem = student.PMRoute ?? "";

            txtBusStop.Text = student.BusStop ?? "";
            txtTransportationNotes.Text = student.TransportationNotes ?? "";

            // Notes
            txtMedicalNotes.Text = student.MedicalNotes ?? "";

            _logger.LogInformation("Form populated with data for student: {StudentName}", student.StudentName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error populating form from student data");
            ShowError($"Failed to populate form: {ex.Message}");
        }
    }
    #endregion

    #region Form Validation and Data Creation
    private Student CreateStudentFromForm()
    {
        var student = _isEditMode ? _existingStudent!.Clone() : new Student();

        student.StudentNumber = string.IsNullOrWhiteSpace(txtStudentNumber.Text) ? null : txtStudentNumber.Text.Trim();
        student.StudentName = txtStudentName.Text.Trim();
        student.Grade = cmbGrade.SelectedItem?.ToString() == "" ? null : cmbGrade.SelectedItem?.ToString();
        student.School = string.IsNullOrWhiteSpace(txtSchool.Text) ? null : txtSchool.Text.Trim();
        student.EnrollmentDate = dtpEnrollmentDate.Value.Date;
        student.Active = chkActive.Checked;

        // Contact information
        student.HomeAddress = string.IsNullOrWhiteSpace(txtHomeAddress.Text) ? null : txtHomeAddress.Text.Trim();
        student.City = string.IsNullOrWhiteSpace(txtCity.Text) ? null : txtCity.Text.Trim();
        student.State = string.IsNullOrWhiteSpace(txtState.Text) ? null : txtState.Text.Trim().ToUpper();
        student.Zip = string.IsNullOrWhiteSpace(txtZip.Text) ? null : txtZip.Text.Trim();
        student.HomePhone = string.IsNullOrWhiteSpace(txtHomePhone.Text) ? null : txtHomePhone.Text.Trim();
        student.ParentGuardian = string.IsNullOrWhiteSpace(txtParentGuardian.Text) ? null : txtParentGuardian.Text.Trim();
        student.EmergencyPhone = string.IsNullOrWhiteSpace(txtEmergencyPhone.Text) ? null : txtEmergencyPhone.Text.Trim();

        // Transportation
        student.AMRoute = cmbAMRoute.SelectedItem?.ToString() == "" ? null : cmbAMRoute.SelectedItem?.ToString();
        student.PMRoute = cmbPMRoute.SelectedItem?.ToString() == "" ? null : cmbPMRoute.SelectedItem?.ToString();
        student.BusStop = string.IsNullOrWhiteSpace(txtBusStop.Text) ? null : txtBusStop.Text.Trim();
        student.TransportationNotes = string.IsNullOrWhiteSpace(txtTransportationNotes.Text) ? null : txtTransportationNotes.Text.Trim();

        // Notes
        student.MedicalNotes = string.IsNullOrWhiteSpace(txtMedicalNotes.Text) ? null : txtMedicalNotes.Text.Trim();

        return student;
    }

    private bool ValidateForm()
    {
        var errors = new List<string>();

        // Required field validation
        if (string.IsNullOrWhiteSpace(txtStudentName.Text))
        {
            errors.Add("Student name is required");
        }

        // Basic validation for other fields
        if (!string.IsNullOrWhiteSpace(txtState.Text) && txtState.Text.Trim().Length != 2)
        {
            errors.Add("State must be a 2-letter abbreviation");
        }

        if (!string.IsNullOrWhiteSpace(txtZip.Text))
        {
            var zipPattern = @"^\d{5}(-\d{4})?$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(txtZip.Text.Trim(), zipPattern))
            {
                errors.Add("Invalid ZIP code format");
            }
        }

        if (errors.Count > 0)
        {
            ShowError($"Please correct the following errors:\n\n• {string.Join("\n• ", errors)}");
            return false;
        }

        return true;
    }
    #endregion

    #region Event Handlers
    private async void BtnValidate_Click(object? sender, EventArgs e)
    {
        try
        {
            if (!ValidateForm())
                return;

            btnValidate.Enabled = false;
            btnValidate.Text = "Validating...";

            var student = CreateStudentFromForm();
            var validationErrors = await _studentService.ValidateStudentAsync(student);

            if (validationErrors.Count == 0)
            {
                ShowSuccess("Student data validation passed successfully!");
            }
            else
            {
                ShowError($"Validation errors found:\n\n• {string.Join("\n• ", validationErrors)}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during student validation");
            ShowError($"Validation failed: {ex.Message}");
        }
        finally
        {
            btnValidate.Enabled = true;
            btnValidate.Text = "✓ Validate";
        }
    }

    private async void BtnSave_Click(object? sender, EventArgs e)
    {
        try
        {
            if (!ValidateForm())
                return;

            _logger.LogInformation("Attempting to save student data");

            btnSave.Enabled = false;
            btnSave.Text = "Saving...";

            var student = CreateStudentFromForm();

            bool success;
            if (_isEditMode)
            {
                success = await _studentService.UpdateStudentAsync(student);
                _logger.LogInformation("Updated student: {StudentName}", student.StudentName);
            }
            else
            {
                await _studentService.AddStudentAsync(student);
                success = true;
                _logger.LogInformation("Added new student: {StudentName}", student.StudentName);
            }

            if (success || !_isEditMode)
            {
                EditedStudent = student;
                IsDataSaved = true;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                ShowError("Failed to save student data");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving student data");
            ShowError($"Save failed: {ex.Message}");
        }
        finally
        {
            btnSave.Enabled = true;
            btnSave.Text = _isEditMode ? "💾 Update Student" : "💾 Save Student";
        }
    }

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
    #endregion

    #region Utility Methods
    private void ShowError(string message)
    {
        _logger.LogError("StudentEditForm Error: {Message}", message);
        Syncfusion.Windows.Forms.MessageBoxAdv.Show(this, message, "Student Edit Error",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void ShowSuccess(string message)
    {
        _logger.LogInformation("StudentEditForm Success: {Message}", message);
        Syncfusion.Windows.Forms.MessageBoxAdv.Show(this, message, "Success",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    #endregion
}

/// <summary>
/// Extension method for cloning Student objects
/// </summary>
public static class StudentExtensions
{
    public static Student Clone(this Student original)
    {
        return new Student
        {
            StudentId = original.StudentId,
            StudentNumber = original.StudentNumber,
            StudentName = original.StudentName,
            Grade = original.Grade,
            School = original.School,
            HomeAddress = original.HomeAddress,
            City = original.City,
            State = original.State,
            Zip = original.Zip,
            HomePhone = original.HomePhone,
            ParentGuardian = original.ParentGuardian,
            EmergencyPhone = original.EmergencyPhone,
            AMRoute = original.AMRoute,
            PMRoute = original.PMRoute,
            BusStop = original.BusStop,
            MedicalNotes = original.MedicalNotes,
            TransportationNotes = original.TransportationNotes,
            Active = original.Active,
            EnrollmentDate = original.EnrollmentDate
        };
    }
}
