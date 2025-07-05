using Bus_Buddy.Models;
using Bus_Buddy.Services;
using Bus_Buddy.Utilities;
using Microsoft.Extensions.Logging;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.DataGrid.Enums;
using System.ComponentModel;

namespace Bus_Buddy.Forms;

/// <summary>
/// Student Management Form - Enhanced implementation using Syncfusion Windows Forms controls
/// Manages student records, route assignments, and transportation information
/// Integrates with Activity management for comprehensive student transportation tracking
/// </summary>
public partial class StudentManagementForm : MetroForm
{
    #region Fields and Services
    private readonly ILogger<StudentManagementForm> _logger;
    private readonly IStudentService _studentService;
    private readonly IBusService _busService;

    // Data Collections
    private BindingList<Student> _students = null!;
    private List<Route> _routes = null!;

    // Currently selected student for operations
    private Student? _selectedStudent;

    // UI Components - Header and Layout using Syncfusion
    private Syncfusion.Windows.Forms.Tools.GradientPanel panelHeader = null!;
    private Syncfusion.Windows.Forms.Tools.GradientPanel panelControls = null!;
    private Syncfusion.Windows.Forms.Tools.GradientPanel panelGrid = null!;
    private Syncfusion.Windows.Forms.Tools.AutoLabel labelTitle = null!;

    // Enhanced Data Grid with Syncfusion SfDataGrid
    private SfDataGrid studentDataGrid = null!;

    // Control Buttons using Syncfusion SfButton
    private SfButton btnAddStudent = null!;
    private SfButton btnEditStudent = null!;
    private SfButton btnDeleteStudent = null!;
    private SfButton btnRefresh = null!;
    private SfButton btnViewDetails = null!;

    // Filter and Search Controls using Syncfusion
    private Syncfusion.Windows.Forms.Tools.TextBoxExt txtSearch = null!;
    private ComboBoxAdv cmbGradeFilter = null!;
    private ComboBoxAdv cmbRouteFilter = null!;
    private ComboBoxAdv cmbActiveFilter = null!;

    // Status and Info Labels using Syncfusion
    private Syncfusion.Windows.Forms.Tools.AutoLabel lblStudentCount = null!;
    private Syncfusion.Windows.Forms.Tools.AutoLabel lblActiveCount = null!;

    #endregion

    #region Constructor
    public StudentManagementForm(
        ILogger<StudentManagementForm> logger,
        IStudentService studentService,
        IBusService busService)
    {
        _logger = logger;
        _studentService = studentService;
        _busService = busService;

        _logger.LogInformation("Initializing Student Management Form with Syncfusion controls");

        InitializeDataCollections();
        InitializeComponent();
        InitializeSyncfusionTheme();
        SetupLayout();
        ConfigureDataGrid();

        // Load data asynchronously after form is shown
        Load += async (s, e) => await LoadInitialDataAsync();
    }

    private void InitializeSyncfusionTheme()
    {
        try
        {
            // Apply enhanced visual theme system
            VisualEnhancementManager.ApplyEnhancedTheme(this);

            // Configure MetroForm properties with enhanced colors
            this.MetroColor = System.Drawing.Color.FromArgb(52, 152, 219);
            this.CaptionBarColor = System.Drawing.Color.FromArgb(52, 152, 219);
            this.CaptionForeColor = System.Drawing.Color.White;

            // Enable high DPI scaling for this form
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

            // Enable high-quality font rendering
            VisualEnhancementManager.EnableHighQualityFontRendering(this);

            _logger.LogInformation("Enhanced visual theme applied successfully to Student Management Form");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not apply enhanced theme to Student Management Form, using default styling");
        }
    }
    #endregion

    #region Initialization
    private void InitializeDataCollections()
    {
        _students = new BindingList<Student>();
        _routes = new List<Route>();
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        // Form Configuration
        Text = "Bus Buddy - Student Management";
        Size = new Size(1600, 1000);
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1200, 700);
        WindowState = FormWindowState.Maximized;

        // Syncfusion MetroForm Configuration
        BackColor = Color.FromArgb(248, 249, 250);
        ForeColor = Color.Black;

        ResumeLayout(false);
        _logger.LogInformation("Student Management Form basic initialization completed");
    }

    private void SetupLayout()
    {
        try
        {
            CreateHeaderPanel();
            CreateControlsPanel();
            CreateDataGridPanel();

            _logger.LogInformation("Student Management layout setup completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up Student Management layout");
            ShowError($"Layout setup failed: {ex.Message}");
        }
    }
    #endregion

    #region Panel Creation
    private void CreateHeaderPanel()
    {
        panelHeader = new Syncfusion.Windows.Forms.Tools.GradientPanel
        {
            Height = 80,
            Dock = DockStyle.Top,
            BackgroundColor = new Syncfusion.Drawing.BrushInfo(Color.FromArgb(248, 249, 250)),
            BorderStyle = BorderStyle.None,
            Padding = new Padding(20, 15, 20, 15)
        };

        labelTitle = new Syncfusion.Windows.Forms.Tools.AutoLabel
        {
            Text = "🎓 Student Transportation Management",
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            ForeColor = Color.FromArgb(52, 152, 219),
            AutoSize = true,
            Location = new Point(20, 25),
            BackColor = Color.Transparent
        };

        // Info labels for student counts
        lblStudentCount = new Syncfusion.Windows.Forms.Tools.AutoLabel
        {
            Text = "Total Students: 0",
            Font = new Font("Segoe UI", 10F),
            ForeColor = Color.FromArgb(100, 100, 100),
            Location = new Point(450, 25),
            Size = new Size(150, 20),
            BackColor = Color.Transparent
        };

        lblActiveCount = new Syncfusion.Windows.Forms.Tools.AutoLabel
        {
            Text = "Active: 0",
            Font = new Font("Segoe UI", 10F),
            ForeColor = Color.FromArgb(46, 125, 50),
            Location = new Point(450, 45),
            Size = new Size(150, 20),
            BackColor = Color.Transparent
        };

        panelHeader.Controls.AddRange(new Control[] { labelTitle, lblStudentCount, lblActiveCount });
        Controls.Add(panelHeader);
    }

    private void CreateControlsPanel()
    {
        panelControls = new Syncfusion.Windows.Forms.Tools.GradientPanel
        {
            Height = 120,
            Dock = DockStyle.Top,
            BackgroundColor = new Syncfusion.Drawing.BrushInfo(Color.White),
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(20, 15, 20, 15)
        };

        SetupControlButtons();
        SetupFilterControls();
        Controls.Add(panelControls);
    }

    private void CreateDataGridPanel()
    {
        panelGrid = new Syncfusion.Windows.Forms.Tools.GradientPanel
        {
            Dock = DockStyle.Fill,
            BackgroundColor = new Syncfusion.Drawing.BrushInfo(Color.White),
            BorderStyle = BorderStyle.None,
            Padding = new Padding(10)
        };

        Controls.Add(panelGrid);
    }
    #endregion

    #region Control Setup
    private void SetupControlButtons()
    {
        // Add Student Button
        btnAddStudent = new SfButton
        {
            Text = "➕ Add Student",
            Size = new Size(140, 40),
            Location = new Point(20, 15),
            AccessibleName = "Add Student"
        };
        VisualEnhancementManager.ApplyEnhancedButtonStyling(btnAddStudent, Color.FromArgb(46, 204, 113));
        btnAddStudent.Click += BtnAddStudent_Click;

        // Edit Student Button  
        btnEditStudent = new SfButton
        {
            Text = "✏️ Edit Student",
            Size = new Size(140, 40),
            Location = new Point(170, 15),
            AccessibleName = "Edit Student"
        };
        VisualEnhancementManager.ApplyEnhancedButtonStyling(btnEditStudent, Color.FromArgb(241, 196, 15));
        btnEditStudent.Click += BtnEditStudent_Click;

        // View Details Button
        btnViewDetails = new SfButton
        {
            Text = "👁️ View Details",
            Size = new Size(140, 40),
            Location = new Point(320, 15),
            AccessibleName = "View Details"
        };
        VisualEnhancementManager.ApplyEnhancedButtonStyling(btnViewDetails, Color.FromArgb(52, 152, 219));
        btnViewDetails.Click += BtnViewDetails_Click;

        // Delete Student Button
        btnDeleteStudent = new SfButton
        {
            Text = "🗑️ Delete",
            Size = new Size(120, 40),
            Location = new Point(470, 15),
            AccessibleName = "Delete Student"
        };
        VisualEnhancementManager.ApplyEnhancedButtonStyling(btnDeleteStudent, Color.FromArgb(231, 76, 60));
        btnDeleteStudent.Click += BtnDeleteStudent_Click;

        // Refresh Button
        btnRefresh = new SfButton
        {
            Text = "🔄 Refresh",
            Size = new Size(120, 40),
            Location = new Point(600, 15),
            AccessibleName = "Refresh Data"
        };
        VisualEnhancementManager.ApplyEnhancedButtonStyling(btnRefresh, Color.FromArgb(155, 89, 182));
        btnRefresh.Click += BtnRefresh_Click;

        panelControls.Controls.AddRange(new Control[]
        {
            btnAddStudent, btnEditStudent, btnViewDetails, btnDeleteStudent, btnRefresh
        });
    }

    private void SetupFilterControls()
    {
        // Search TextBox
        var lblSearch = new Syncfusion.Windows.Forms.Tools.AutoLabel
        {
            Text = "Search:",
            Location = new Point(20, 75),
            Size = new Size(60, 20),
            Font = new Font("Segoe UI", 9F),
            BackColor = Color.Transparent
        };

        txtSearch = new Syncfusion.Windows.Forms.Tools.TextBoxExt
        {
            Location = new Point(85, 70),
            Size = new Size(200, 30),
            Font = new Font("Segoe UI", 9F),
            Text = "", // TextBoxExt doesn't have PlaceholderText
            BorderStyle = BorderStyle.FixedSingle
        };
        txtSearch.TextChanged += TxtSearch_TextChanged;

        // Grade Filter
        var lblGrade = new Syncfusion.Windows.Forms.Tools.AutoLabel
        {
            Text = "Grade:",
            Location = new Point(300, 75),
            Size = new Size(50, 20),
            Font = new Font("Segoe UI", 9F),
            BackColor = Color.Transparent
        };

        cmbGradeFilter = new ComboBoxAdv
        {
            Location = new Point(355, 70),
            Size = new Size(100, 30),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 9F)
        };
        cmbGradeFilter.Items.Add("All Grades");
        cmbGradeFilter.Items.Add("Pre-K");
        cmbGradeFilter.Items.Add("K");
        for (int i = 1; i <= 12; i++)
        {
            cmbGradeFilter.Items.Add($"Grade {i}");
        }
        cmbGradeFilter.SelectedIndex = 0;
        cmbGradeFilter.SelectedIndexChanged += CmbGradeFilter_SelectedIndexChanged;

        // Route Filter
        var lblRoute = new Syncfusion.Windows.Forms.Tools.AutoLabel
        {
            Text = "Route:",
            Location = new Point(470, 75),
            Size = new Size(50, 20),
            Font = new Font("Segoe UI", 9F),
            BackColor = Color.Transparent
        };

        cmbRouteFilter = new ComboBoxAdv
        {
            Location = new Point(525, 70),
            Size = new Size(150, 30),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 9F)
        };
        cmbRouteFilter.SelectedIndexChanged += CmbRouteFilter_SelectedIndexChanged;

        // Active Status Filter
        var lblActive = new Syncfusion.Windows.Forms.Tools.AutoLabel
        {
            Text = "Status:",
            Location = new Point(690, 75),
            Size = new Size(50, 20),
            Font = new Font("Segoe UI", 9F),
            BackColor = Color.Transparent
        };

        cmbActiveFilter = new ComboBoxAdv
        {
            Location = new Point(745, 70),
            Size = new Size(100, 30),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 9F)
        };
        cmbActiveFilter.Items.Add("All Students");
        cmbActiveFilter.Items.Add("Active Only");
        cmbActiveFilter.Items.Add("Inactive Only");
        cmbActiveFilter.SelectedIndex = 1; // Default to Active Only
        cmbActiveFilter.SelectedIndexChanged += CmbActiveFilter_SelectedIndexChanged;

        panelControls.Controls.AddRange(new Control[]
        {
            lblSearch, txtSearch, lblGrade, cmbGradeFilter,
            lblRoute, cmbRouteFilter, lblActive, cmbActiveFilter
        });
    }
    #endregion

    #region Data Grid Configuration
    private void ConfigureDataGrid()
    {
        try
        {
            _logger.LogInformation("Configuring Syncfusion SfDataGrid for student data");

            // Create the SfDataGrid
            studentDataGrid = new SfDataGrid
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false
            };

            // Apply standardized configuration
            SyncfusionLayoutManager.ConfigureSfDataGrid(studentDataGrid, true, true);
            SyncfusionAdvancedManager.ApplyAdvancedConfiguration(studentDataGrid);
            VisualEnhancementManager.ApplyEnhancedGridVisuals(studentDataGrid);
            SyncfusionLayoutManager.ApplyGridStyling(studentDataGrid);

            // Configure columns
            ConfigureGridColumns();

            // Setup event handlers
            studentDataGrid.SelectionChanged += StudentDataGrid_SelectionChanged;
            studentDataGrid.QueryRowStyle += StudentDataGrid_QueryRowStyle;

            // Add to container
            panelGrid.Controls.Add(studentDataGrid);

            _logger.LogInformation("SfDataGrid configured successfully with standardized configuration");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure SfDataGrid");
            ShowError($"Data grid configuration failed: {ex.Message}");
        }
    }

    private void StudentDataGrid_QueryRowStyle(object sender, Syncfusion.WinForms.DataGrid.Events.QueryRowStyleEventArgs e)
    {
        if (e.RowData is Student student)
        {
            // Apply styling based on student status
            if (!student.Active)
            {
                // Inactive students - subtle gray styling
                e.Style.BackColor = Color.FromArgb(248, 248, 248);
                e.Style.TextColor = Color.FromArgb(128, 128, 128);
            }
            else if (string.IsNullOrEmpty(student.AMRoute) && string.IsNullOrEmpty(student.PMRoute))
            {
                // Students without route assignments - light orange warning
                e.Style.BackColor = Color.FromArgb(255, 245, 230);
                e.Style.TextColor = Color.FromArgb(184, 134, 11);
            }
            else if (string.IsNullOrEmpty(student.BusStop))
            {
                // Students without bus stop assignments - light yellow warning
                e.Style.BackColor = Color.FromArgb(254, 252, 232);
                e.Style.TextColor = Color.FromArgb(161, 98, 7);
            }
        }
    }

    private void ConfigureGridColumns()
    {
        // Clear any existing columns
        studentDataGrid.Columns.Clear();

        // Add custom columns using Syncfusion GridTextColumn with enhanced styling
        studentDataGrid.Columns.Add(new GridTextColumn()
        {
            MappingName = "StudentNumber",
            HeaderText = "Student #",
            Width = 100,
            AllowSorting = true,
            AllowEditing = false
        });

        studentDataGrid.Columns.Add(new GridTextColumn()
        {
            MappingName = "StudentName",
            HeaderText = "Student Name",
            Width = 200,
            AllowSorting = true
        });

        studentDataGrid.Columns.Add(new GridTextColumn()
        {
            MappingName = "Grade",
            HeaderText = "Grade",
            Width = 80,
            AllowSorting = true
        });

        studentDataGrid.Columns.Add(new GridTextColumn()
        {
            MappingName = "School",
            HeaderText = "School",
            Width = 150,
            AllowSorting = true
        });

        studentDataGrid.Columns.Add(new GridTextColumn()
        {
            MappingName = "AMRoute",
            HeaderText = "AM Route",
            Width = 120,
            AllowSorting = true
        });

        studentDataGrid.Columns.Add(new GridTextColumn()
        {
            MappingName = "PMRoute",
            HeaderText = "PM Route",
            Width = 120,
            AllowSorting = true
        });

        studentDataGrid.Columns.Add(new GridTextColumn()
        {
            MappingName = "BusStop",
            HeaderText = "Bus Stop",
            Width = 150,
            AllowSorting = true
        });

        studentDataGrid.Columns.Add(new GridTextColumn()
        {
            MappingName = "ParentGuardian",
            HeaderText = "Parent/Guardian",
            Width = 180,
            AllowSorting = true
        });

        studentDataGrid.Columns.Add(new GridTextColumn()
        {
            MappingName = "HomePhone",
            HeaderText = "Phone",
            Width = 120,
            AllowSorting = true
        });

        studentDataGrid.Columns.Add(new GridCheckBoxColumn()
        {
            MappingName = "Active",
            HeaderText = "Active",
            Width = 80,
            AllowSorting = true
        });

        // Enrollment Date with proper formatting
        studentDataGrid.Columns.Add(new GridDateTimeColumn()
        {
            MappingName = "EnrollmentDate",
            HeaderText = "Enrollment Date",
            Width = 140,
            Format = "MM/dd/yyyy",
            AllowSorting = true
        });
    }
    #endregion

    #region Data Loading and Management
    private async Task LoadInitialDataAsync()
    {
        try
        {
            _logger.LogInformation("Loading initial student data");

            // Update UI to show loading state
            if (this.InvokeRequired)
            {
                this.Invoke(() =>
                {
                    btnRefresh.Text = "Loading...";
                    btnRefresh.Enabled = false;
                });
            }
            else
            {
                btnRefresh.Text = "Loading...";
                btnRefresh.Enabled = false;
            }

            // Load students
            await LoadStudentsAsync();

            // Load routes for filter dropdown
            await LoadRoutesAsync();

            // Update UI on completion
            if (this.InvokeRequired)
            {
                this.Invoke(() =>
                {
                    btnRefresh.Text = "🔄 Refresh";
                    btnRefresh.Enabled = true;
                });
            }
            else
            {
                btnRefresh.Text = "🔄 Refresh";
                btnRefresh.Enabled = true;
            }

            _logger.LogInformation("Student data loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading initial student data");

            // Handle error on UI thread
            if (this.InvokeRequired)
            {
                this.Invoke(() =>
                {
                    btnRefresh.Text = "🔄 Refresh";
                    btnRefresh.Enabled = true;
                    ShowError($"Failed to load student data: {ex.Message}");
                });
            }
            else
            {
                btnRefresh.Text = "🔄 Refresh";
                btnRefresh.Enabled = true;
                ShowError($"Failed to load student data: {ex.Message}");
            }
        }
    }

    private async Task LoadStudentsAsync()
    {
        try
        {
            var students = await _studentService.GetAllStudentsAsync();

            // Update UI on main thread
            if (this.InvokeRequired)
            {
                this.Invoke(() => UpdateStudentData(students));
            }
            else
            {
                UpdateStudentData(students);
            }

            _logger.LogInformation("Loaded {Count} students", students.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading students from database");
            throw;
        }
    }

    private void UpdateStudentData(IEnumerable<Student> students)
    {
        _students.Clear();
        foreach (var student in students)
        {
            _students.Add(student);
        }

        // Set data source
        studentDataGrid.DataSource = _students;

        // Apply current filters
        ApplyFilters();

        // Update counts
        UpdateStudentCounts();
    }

    private async Task LoadRoutesAsync()
    {
        try
        {
            _routes = await _busService.GetAllRouteEntitiesAsync();

            // Update route filter dropdown
            cmbRouteFilter.Items.Clear();
            cmbRouteFilter.Items.Add("All Routes");

            foreach (var route in _routes)
            {
                cmbRouteFilter.Items.Add(route.RouteName);
            }

            cmbRouteFilter.SelectedIndex = 0;

            _logger.LogInformation("Loaded {Count} routes for filtering", _routes.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading routes");
            ShowError($"Failed to load routes: {ex.Message}");
        }
    }

    private void ApplyFilters()
    {
        try
        {
            var filteredStudents = _students.AsEnumerable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                var searchTerm = txtSearch.Text.ToLower();
                filteredStudents = filteredStudents.Where(s =>
                    s.StudentName.ToLower().Contains(searchTerm) ||
                    (s.StudentNumber?.ToLower().Contains(searchTerm) ?? false));
            }

            // Apply grade filter
            if (cmbGradeFilter.SelectedIndex > 0)
            {
                var selectedGrade = cmbGradeFilter.SelectedItem?.ToString();
                if (selectedGrade != "All Grades")
                {
                    filteredStudents = filteredStudents.Where(s => s.Grade == selectedGrade);
                }
            }

            // Apply route filter
            if (cmbRouteFilter.SelectedIndex > 0)
            {
                var selectedRoute = cmbRouteFilter.SelectedItem?.ToString();
                if (selectedRoute != "All Routes")
                {
                    filteredStudents = filteredStudents.Where(s =>
                        s.AMRoute == selectedRoute || s.PMRoute == selectedRoute);
                }
            }

            // Apply active status filter
            switch (cmbActiveFilter.SelectedIndex)
            {
                case 1: // Active Only
                    filteredStudents = filteredStudents.Where(s => s.Active);
                    break;
                case 2: // Inactive Only
                    filteredStudents = filteredStudents.Where(s => !s.Active);
                    break;
                    // case 0 (All Students) - no filter
            }

            // Create new filtered collection
            var filteredList = new BindingList<Student>(filteredStudents.ToList());
            studentDataGrid.DataSource = filteredList;

            UpdateStudentCounts();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying filters");
            ShowError($"Filter application failed: {ex.Message}");
        }
    }

    private void UpdateStudentCounts()
    {
        var currentData = studentDataGrid.DataSource as IList<Student> ?? _students;
        var totalCount = currentData.Count;
        var activeCount = currentData.Count(s => s.Active);

        lblStudentCount.Text = $"Total Students: {totalCount}";
        lblActiveCount.Text = $"Active: {activeCount}";
    }

    private void UpdateButtonStates()
    {
        var hasSelection = studentDataGrid.SelectedItems.Count > 0;
        btnEditStudent.Enabled = hasSelection;
        btnDeleteStudent.Enabled = hasSelection;
        btnViewDetails.Enabled = hasSelection;
    }
    #endregion

    #region Event Handlers
    private void BtnAddStudent_Click(object? sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Opening StudentEditForm for new student");

            using var form = ServiceContainer.GetService<StudentEditForm>();
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                // Refresh the grid to show the new student using proper async pattern
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await LoadStudentsAsync();

                        // Update UI on main thread
                        this.Invoke(() => ShowSuccess("Student added successfully"));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error refreshing data after student add");
                        this.Invoke(() => ShowError($"Student added but failed to refresh data: {ex.Message}"));
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening add student form");
            ShowError($"Failed to open add student form: {ex.Message}");
        }
    }

    private async void BtnEditStudent_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_selectedStudent == null)
            {
                ShowInfo("Please select a student to edit.");
                return;
            }

            _logger.LogInformation("Opening StudentEditForm for student {StudentId}", _selectedStudent.StudentId);

            // Visual feedback during operation
            btnEditStudent.Enabled = false;
            btnEditStudent.Text = "Loading...";

            try
            {
                // Get the full student data from the service
                var student = await _studentService.GetStudentByIdAsync(_selectedStudent.StudentId);
                if (student != null)
                {
                    using var editForm = new StudentEditForm(
                        ServiceContainer.GetService<ILogger<StudentEditForm>>(),
                        _studentService,
                        _busService,
                        student);

                    if (editForm.ShowDialog(this) == DialogResult.OK)
                    {
                        // Refresh the grid to show updated data
                        await LoadStudentsAsync();
                        ShowSuccess("Student updated successfully");
                    }
                }
                else
                {
                    ShowError("Student not found in database");
                }
            }
            finally
            {
                btnEditStudent.Enabled = true;
                btnEditStudent.Text = "✏️ Edit Student";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing student");
            ShowError($"Failed to edit student: {ex.Message}");

            // Restore button state
            btnEditStudent.Enabled = true;
            btnEditStudent.Text = "✏️ Edit Student";
        }
    }

    private void BtnViewDetails_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_selectedStudent == null)
            {
                ShowInfo("Please select a student to view details.");
                return;
            }

            _logger.LogInformation("Viewing details for student {StudentId}", _selectedStudent.StudentId);

            // Create detailed info message
            var details = $@"Student Details

Student Number: {_selectedStudent.StudentNumber}
Name: {_selectedStudent.StudentName}
Grade: {_selectedStudent.Grade}
School: {_selectedStudent.School}

Transportation:
AM Route: {_selectedStudent.AMRoute ?? "Not Assigned"}
PM Route: {_selectedStudent.PMRoute ?? "Not Assigned"}
Bus Stop: {_selectedStudent.BusStop ?? "Not Assigned"}

Contact Information:
Parent/Guardian: {_selectedStudent.ParentGuardian}
Home Phone: {_selectedStudent.HomePhone}
Emergency Phone: {_selectedStudent.EmergencyPhone}

Status: {(_selectedStudent.Active ? "Active" : "Inactive")}
Enrollment Date: {_selectedStudent.EnrollmentDate:MM/dd/yyyy}

Address:
{_selectedStudent.HomeAddress}
{_selectedStudent.City}, {_selectedStudent.State} {_selectedStudent.Zip}

Notes:
Medical: {_selectedStudent.MedicalNotes ?? "None"}
Transportation: {_selectedStudent.TransportationNotes ?? "None"}";

            Syncfusion.Windows.Forms.MessageBoxAdv.Show(this, details,
                $"Student Details - {_selectedStudent.StudentName}",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error viewing student details");
            ShowError($"Failed to view student details: {ex.Message}");
        }
    }

    private async void BtnDeleteStudent_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_selectedStudent == null)
            {
                ShowInfo("Please select a student to delete.");
                return;
            }

            var result = Syncfusion.Windows.Forms.MessageBoxAdv.Show(this,
                $"Are you sure you want to delete student '{_selectedStudent.StudentName}'?\n\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
            {
                _logger.LogInformation("Deleting student {StudentId}", _selectedStudent.StudentId);

                // Visual feedback during operation
                btnDeleteStudent.Enabled = false;
                btnDeleteStudent.Text = "Deleting...";

                try
                {
                    await _studentService.DeleteStudentAsync(_selectedStudent.StudentId);

                    // Clear selection and refresh
                    _selectedStudent = null;
                    await LoadStudentsAsync();

                    ShowSuccess("Student deleted successfully");
                }
                finally
                {
                    btnDeleteStudent.Enabled = true;
                    btnDeleteStudent.Text = "🗑️ Delete";
                    UpdateButtonStates(); // Update button states based on selection
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting student");
            ShowError($"Delete student failed: {ex.Message}");

            // Restore button state
            btnDeleteStudent.Enabled = true;
            btnDeleteStudent.Text = "🗑️ Delete";
        }
    }

    private async void BtnRefresh_Click(object? sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Refresh button clicked for Student Management");

            // Visual feedback during refresh
            btnRefresh.Enabled = false;
            btnRefresh.Text = "Refreshing...";

            // Update status on UI thread
            if (lblStudentCount != null)
            {
                lblStudentCount.Text = "Refreshing student data...";
                lblStudentCount.ForeColor = System.Drawing.Color.FromArgb(52, 152, 219);
            }

            await LoadInitialDataAsync();

            // Show success feedback
            if (lblStudentCount != null)
            {
                lblStudentCount.ForeColor = System.Drawing.Color.FromArgb(46, 204, 113);
            }

            ShowInfo("Student data refreshed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing student data");

            if (lblStudentCount != null)
            {
                lblStudentCount.Text = "Refresh failed";
                lblStudentCount.ForeColor = System.Drawing.Color.FromArgb(231, 76, 60);
            }

            ShowError($"Refresh failed: {ex.Message}");
        }
        finally
        {
            btnRefresh.Enabled = true;
            btnRefresh.Text = "🔄 Refresh";
        }
    }

    private void TxtSearch_TextChanged(object? sender, EventArgs e)
    {
        ApplyFilters();
    }

    private void CmbGradeFilter_SelectedIndexChanged(object? sender, EventArgs e)
    {
        ApplyFilters();
    }

    private void CmbRouteFilter_SelectedIndexChanged(object? sender, EventArgs e)
    {
        ApplyFilters();
    }

    private void CmbActiveFilter_SelectedIndexChanged(object? sender, EventArgs e)
    {
        ApplyFilters();
    }

    private void StudentDataGrid_SelectionChanged(object? sender, EventArgs e)
    {
        try
        {
            if (studentDataGrid.SelectedItem is Student selectedStudent)
            {
                _selectedStudent = selectedStudent;
                _logger.LogDebug("Selected student: {StudentName} (ID: {StudentId})",
                    selectedStudent.StudentName, selectedStudent.StudentId);
            }
            else
            {
                _selectedStudent = null;
            }

            UpdateButtonStates();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling student selection");
        }
    }
    #endregion

    #region Utility Methods
    private void ShowError(string message)
    {
        _logger.LogError("Student Management Error: {Message}", message);
        Syncfusion.Windows.Forms.MessageBoxAdv.Show(this, message, "Student Management Error",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void ShowInfo(string message)
    {
        _logger.LogInformation("Student Management Info: {Message}", message);
        Syncfusion.Windows.Forms.MessageBoxAdv.Show(this, message, "Student Management",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ShowSuccess(string message)
    {
        _logger.LogInformation("Student Management Success: {Message}", message);
        Syncfusion.Windows.Forms.MessageBoxAdv.Show(this, message, "Success",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Clear selected student
            _selectedStudent = null;
        }
        base.Dispose(disposing);
    }
    #endregion
}
