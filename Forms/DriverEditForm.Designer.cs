using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.WinForms.Controls;
using System.Drawing;
using System.Windows.Forms;

namespace Bus_Buddy.Forms;

partial class DriverEditForm
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    // Declare Syncfusion controls
    private GradientPanel mainPanel;
    private GradientPanel headerPanel;
    private GradientPanel contentPanel;
    private GradientPanel buttonPanel;
    private AutoLabel titleLabel;
    private GradientPanel personalInfoGroupBox;
    private GradientPanel contactInfoGroupBox;
    private GradientPanel licenseInfoGroupBox;
    private GradientPanel employmentInfoGroupBox;

    // Personal Information Controls
    private AutoLabel firstNameLabel;
    private TextBoxExt firstNameTextBox;
    private AutoLabel lastNameLabel;
    private TextBoxExt lastNameTextBox;
    private AutoLabel driverNameLabel;
    private TextBoxExt driverNameTextBox;

    // Contact Information Controls
    private AutoLabel phoneLabel;
    private TextBoxExt phoneTextBox;
    private AutoLabel emailLabel;
    private TextBoxExt emailTextBox;
    private AutoLabel addressLabel;
    private TextBoxExt addressTextBox;
    private AutoLabel cityLabel;
    private TextBoxExt cityTextBox;
    private AutoLabel stateLabel;
    private ComboBoxAdv stateComboBox;
    private AutoLabel zipLabel;
    private TextBoxExt zipTextBox;

    // License Information Controls
    private AutoLabel licenseNumberLabel;
    private TextBoxExt licenseNumberTextBox;
    private AutoLabel licenseTypeLabel;
    private ComboBoxAdv licenseTypeComboBox;
    private AutoLabel licenseClassLabel;
    private ComboBoxAdv licenseClassComboBox;
    private AutoLabel licenseIssueDateLabel;
    private DateTimePickerAdv licenseIssueDatePicker;
    private AutoLabel licenseExpiryDateLabel;
    private DateTimePickerAdv licenseExpiryDatePicker;
    private AutoLabel endorsementsLabel;
    private TextBoxExt endorsementsTextBox;

    // Employment Information Controls
    private AutoLabel hireDateLabel;
    private DateTimePickerAdv hireDatePicker;
    private AutoLabel statusLabel;
    private ComboBoxAdv statusComboBox;
    private CheckBoxAdv trainingCompleteCheckBox;

    // Action buttons
    private SfButton saveButton;
    private SfButton cancelButton;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();

        // Apply Office2016Colorful theme consistently
        Syncfusion.Windows.Forms.SkinManager.SetVisualStyle(this, Syncfusion.Windows.Forms.VisualTheme.Office2016Colorful);

        // Suspend layout for better performance during initialization
        this.SuspendLayout();

        // Initialize Syncfusion components
        this.mainPanel = new GradientPanel();
        this.headerPanel = new GradientPanel();
        this.contentPanel = new GradientPanel();
        this.buttonPanel = new GradientPanel();
        this.titleLabel = new AutoLabel();

        // Initialize group boxes
        this.personalInfoGroupBox = new GradientPanel();
        this.contactInfoGroupBox = new GradientPanel();
        this.licenseInfoGroupBox = new GradientPanel();
        this.employmentInfoGroupBox = new GradientPanel();

        // Initialize personal information controls
        this.firstNameLabel = new AutoLabel();
        this.firstNameTextBox = new TextBoxExt();
        this.lastNameLabel = new AutoLabel();
        this.lastNameTextBox = new TextBoxExt();
        this.driverNameLabel = new AutoLabel();
        this.driverNameTextBox = new TextBoxExt();

        // Initialize contact information controls
        this.phoneLabel = new AutoLabel();
        this.phoneTextBox = new TextBoxExt();
        this.emailLabel = new AutoLabel();
        this.emailTextBox = new TextBoxExt();
        this.addressLabel = new AutoLabel();
        this.addressTextBox = new TextBoxExt();
        this.cityLabel = new AutoLabel();
        this.cityTextBox = new TextBoxExt();
        this.stateLabel = new AutoLabel();
        this.stateComboBox = new ComboBoxAdv();
        this.zipLabel = new AutoLabel();
        this.zipTextBox = new TextBoxExt();

        // Initialize license information controls
        this.licenseNumberLabel = new AutoLabel();
        this.licenseNumberTextBox = new TextBoxExt();
        this.licenseTypeLabel = new AutoLabel();
        this.licenseTypeComboBox = new ComboBoxAdv();
        this.licenseClassLabel = new AutoLabel();
        this.licenseClassComboBox = new ComboBoxAdv();
        this.licenseIssueDateLabel = new AutoLabel();
        this.licenseIssueDatePicker = new DateTimePickerAdv();
        this.licenseExpiryDateLabel = new AutoLabel();
        this.licenseExpiryDatePicker = new DateTimePickerAdv();
        this.endorsementsLabel = new AutoLabel();
        this.endorsementsTextBox = new TextBoxExt();

        // Initialize employment information controls
        this.hireDateLabel = new AutoLabel();
        this.hireDatePicker = new DateTimePickerAdv();
        this.statusLabel = new AutoLabel();
        this.statusComboBox = new ComboBoxAdv();
        this.trainingCompleteCheckBox = new CheckBoxAdv();

        // Initialize buttons
        this.saveButton = new SfButton();
        this.cancelButton = new SfButton();

        // Main Panel
        this.mainPanel.BorderStyle = BorderStyle.None;
        this.mainPanel.Dock = DockStyle.Fill;
        this.mainPanel.Location = new Point(0, 0);
        this.mainPanel.Name = "mainPanel";
        this.mainPanel.Size = new Size(900, 750);
        this.mainPanel.TabIndex = 0;

        // Header Panel with Office2016 styling
        this.headerPanel.BorderStyle = BorderStyle.None;
        this.headerPanel.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Color.FromArgb(52, 152, 219));
        this.headerPanel.Dock = DockStyle.Top;
        this.headerPanel.Location = new Point(0, 0);
        this.headerPanel.Name = "headerPanel";
        this.headerPanel.Size = new Size(900, 50);
        this.headerPanel.TabIndex = 1;

        // Title Label
        this.titleLabel.BackColor = Color.Transparent;
        this.titleLabel.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        this.titleLabel.ForeColor = Color.White;
        this.titleLabel.Location = new Point(20, 15);
        this.titleLabel.Name = "titleLabel";
        this.titleLabel.Size = new Size(250, 25);
        this.titleLabel.TabIndex = 0;
        this.titleLabel.Text = "Driver Information";

        // Button Panel
        this.buttonPanel.BorderStyle = BorderStyle.None;
        this.buttonPanel.Dock = DockStyle.Bottom;
        this.buttonPanel.Location = new Point(0, 690);
        this.buttonPanel.Name = "buttonPanel";
        this.buttonPanel.Size = new Size(900, 60);
        this.buttonPanel.TabIndex = 2;
        this.buttonPanel.Padding = new Padding(10);

        // Content Panel
        this.contentPanel.BorderStyle = BorderStyle.None;
        this.contentPanel.Dock = DockStyle.Fill;
        this.contentPanel.Location = new Point(0, 50);
        this.contentPanel.Name = "contentPanel";
        this.contentPanel.Size = new Size(900, 640);
        this.contentPanel.TabIndex = 3;
        this.contentPanel.Padding = new Padding(20);

        // Personal Information Group Panel with Office2016 styling
        this.personalInfoGroupBox.BorderStyle = BorderStyle.FixedSingle;
        this.personalInfoGroupBox.Location = new Point(20, 20);
        this.personalInfoGroupBox.Name = "personalInfoGroupBox";
        this.personalInfoGroupBox.Size = new Size(420, 140);
        this.personalInfoGroupBox.TabIndex = 0;
        this.personalInfoGroupBox.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Color.FromArgb(248, 249, 250));

        // Contact Information Group Panel
        this.contactInfoGroupBox.BorderStyle = BorderStyle.FixedSingle;
        this.contactInfoGroupBox.Location = new Point(460, 20);
        this.contactInfoGroupBox.Name = "contactInfoGroupBox";
        this.contactInfoGroupBox.Size = new Size(400, 280);
        this.contactInfoGroupBox.TabIndex = 1;
        this.contactInfoGroupBox.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Color.FromArgb(248, 249, 250));

        // License Information Group Panel
        this.licenseInfoGroupBox.BorderStyle = BorderStyle.FixedSingle;
        this.licenseInfoGroupBox.Location = new Point(20, 180);
        this.licenseInfoGroupBox.Name = "licenseInfoGroupBox";
        this.licenseInfoGroupBox.Size = new Size(420, 280);
        this.licenseInfoGroupBox.TabIndex = 2;
        this.licenseInfoGroupBox.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Color.FromArgb(248, 249, 250));

        // Employment Information Group Panel
        this.employmentInfoGroupBox.BorderStyle = BorderStyle.FixedSingle;
        this.employmentInfoGroupBox.Location = new Point(20, 480);
        this.employmentInfoGroupBox.Name = "employmentInfoGroupBox";
        this.employmentInfoGroupBox.Size = new Size(840, 120);
        this.employmentInfoGroupBox.TabIndex = 3;
        this.employmentInfoGroupBox.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Color.FromArgb(248, 249, 250));

        // Configure Personal Information Controls
        ConfigurePersonalInfoControls();

        // Configure Contact Information Controls
        ConfigureContactInfoControls();

        // Configure License Information Controls
        ConfigureLicenseInfoControls();

        // Configure Employment Information Controls
        ConfigureEmploymentInfoControls();

        // Configure action buttons with Office2016 colors
        ConfigureActionButton(this.saveButton, "Save", 710, 15, 19, Color.FromArgb(76, 175, 80));
        ConfigureActionButton(this.cancelButton, "Cancel", 820, 15, 20, Color.FromArgb(158, 158, 158));

        // Add event handlers
        this.saveButton.Click += new System.EventHandler(this.SaveButton_Click);
        this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);

        // Add controls to panels
        this.headerPanel.Controls.Add(this.titleLabel);

        this.contentPanel.Controls.Add(this.personalInfoGroupBox);
        this.contentPanel.Controls.Add(this.contactInfoGroupBox);
        this.contentPanel.Controls.Add(this.licenseInfoGroupBox);
        this.contentPanel.Controls.Add(this.employmentInfoGroupBox);

        this.buttonPanel.Controls.Add(this.saveButton);
        this.buttonPanel.Controls.Add(this.cancelButton);

        this.mainPanel.Controls.Add(this.contentPanel);
        this.mainPanel.Controls.Add(this.buttonPanel);
        this.mainPanel.Controls.Add(this.headerPanel);

        // Form settings
        this.AutoScaleMode = AutoScaleMode.Dpi;
        this.AutoScaleDimensions = new SizeF(96F, 96F);
        this.ClientSize = new Size(900, 750);
        this.Controls.Add(this.mainPanel);
        this.Text = "Driver Information";
        this.MinimumSize = new Size(900, 750);
        this.MaximumSize = new Size(900, 750);
        this.StartPosition = FormStartPosition.CenterParent;
        this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MetroColor = Color.FromArgb(52, 152, 219);
        this.CaptionBarColor = Color.FromArgb(52, 152, 219);
        this.CaptionForeColor = Color.White;

        // Set up date pickers with current date as default
        hireDatePicker.Value = DateTime.Now;
        licenseIssueDatePicker.Value = DateTime.Now.AddYears(-1);
        licenseExpiryDatePicker.Value = DateTime.Now.AddYears(5);

        // Set up combo boxes with enhanced styling
        licenseTypeComboBox.Items.AddRange(new[] { "CDL", "Passenger", "Regular", "Chauffeur" });
        licenseTypeComboBox.SelectedIndex = 0;

        licenseClassComboBox.Items.AddRange(new[] { "A", "B", "C", "CDL-A", "CDL-B", "CDL-C" });
        licenseClassComboBox.SelectedIndex = 3; // CDL-B is common for school buses

        statusComboBox.Items.AddRange(new[] { "Active", "Inactive", "Suspended", "Terminated", "On Leave" });
        statusComboBox.SelectedIndex = 0;

        stateComboBox.Items.AddRange(new[] { "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA",
            "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MD", "MA", "MI", "MN", "MS", "MO",
            "MT", "NE", "NV", "NH", "NJ", "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC",
            "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY" });

        // Resume layout
        this.ResumeLayout(false);
    }

    private void ConfigurePersonalInfoControls()
    {
        // First Name
        this.firstNameLabel.Font = new Font("Segoe UI", 9F);
        this.firstNameLabel.Location = new Point(15, 25);
        this.firstNameLabel.Size = new Size(80, 20);
        this.firstNameLabel.Text = "First Name:";

        this.firstNameTextBox.Location = new Point(105, 25);
        this.firstNameTextBox.Size = new Size(150, 23);
        this.firstNameTextBox.Font = new Font("Segoe UI", 9F);
        this.firstNameTextBox.TabIndex = 0;

        // Last Name
        this.lastNameLabel.Font = new Font("Segoe UI", 9F);
        this.lastNameLabel.Location = new Point(270, 25);
        this.lastNameLabel.Size = new Size(80, 20);
        this.lastNameLabel.Text = "Last Name:";

        this.lastNameTextBox.Location = new Point(355, 25);
        this.lastNameTextBox.Size = new Size(150, 23);
        this.lastNameTextBox.Font = new Font("Segoe UI", 9F);
        this.lastNameTextBox.TabIndex = 1;

        // Driver Name
        this.driverNameLabel.Font = new Font("Segoe UI", 9F);
        this.driverNameLabel.Location = new Point(15, 65);
        this.driverNameLabel.Size = new Size(80, 20);
        this.driverNameLabel.Text = "Driver Name:";

        this.driverNameTextBox.Location = new Point(105, 65);
        this.driverNameTextBox.Size = new Size(200, 23);
        this.driverNameTextBox.Font = new Font("Segoe UI", 9F);
        this.driverNameTextBox.TabIndex = 2;

        // Add controls to personal info group
        this.personalInfoGroupBox.Controls.Add(this.firstNameLabel);
        this.personalInfoGroupBox.Controls.Add(this.firstNameTextBox);
        this.personalInfoGroupBox.Controls.Add(this.lastNameLabel);
        this.personalInfoGroupBox.Controls.Add(this.lastNameTextBox);
        this.personalInfoGroupBox.Controls.Add(this.driverNameLabel);
        this.personalInfoGroupBox.Controls.Add(this.driverNameTextBox);
    }

    private void ConfigureContactInfoControls()
    {
        // Phone
        this.phoneLabel.Font = new Font("Segoe UI", 9F);
        this.phoneLabel.Location = new Point(15, 25);
        this.phoneLabel.Size = new Size(50, 20);
        this.phoneLabel.Text = "Phone:";

        this.phoneTextBox.Location = new Point(75, 25);
        this.phoneTextBox.Size = new Size(150, 23);
        this.phoneTextBox.Font = new Font("Segoe UI", 9F);
        this.phoneTextBox.TabIndex = 3;

        // Email
        this.emailLabel.Font = new Font("Segoe UI", 9F);
        this.emailLabel.Location = new Point(15, 55);
        this.emailLabel.Size = new Size(50, 20);
        this.emailLabel.Text = "Email:";

        this.emailTextBox.Location = new Point(75, 55);
        this.emailTextBox.Size = new Size(250, 23);
        this.emailTextBox.Font = new Font("Segoe UI", 9F);
        this.emailTextBox.TabIndex = 4;

        // Address
        this.addressLabel.Font = new Font("Segoe UI", 9F);
        this.addressLabel.Location = new Point(15, 85);
        this.addressLabel.Size = new Size(60, 20);
        this.addressLabel.Text = "Address:";

        this.addressTextBox.Location = new Point(75, 85);
        this.addressTextBox.Size = new Size(300, 23);
        this.addressTextBox.Font = new Font("Segoe UI", 9F);
        this.addressTextBox.TabIndex = 5;

        // City
        this.cityLabel.Font = new Font("Segoe UI", 9F);
        this.cityLabel.Location = new Point(15, 115);
        this.cityLabel.Size = new Size(40, 20);
        this.cityLabel.Text = "City:";

        this.cityTextBox.Location = new Point(75, 115);
        this.cityTextBox.Size = new Size(150, 23);
        this.cityTextBox.Font = new Font("Segoe UI", 9F);
        this.cityTextBox.TabIndex = 6;

        // State
        this.stateLabel.Font = new Font("Segoe UI", 9F);
        this.stateLabel.Location = new Point(240, 115);
        this.stateLabel.Size = new Size(40, 20);
        this.stateLabel.Text = "State:";

        this.stateComboBox.Location = new Point(285, 115);
        this.stateComboBox.Size = new Size(60, 23);
        this.stateComboBox.Font = new Font("Segoe UI", 9F);
        this.stateComboBox.Style = Syncfusion.Windows.Forms.VisualStyle.Office2016Colorful;
        this.stateComboBox.TabIndex = 7;

        // Zip
        this.zipLabel.Font = new Font("Segoe UI", 9F);
        this.zipLabel.Location = new Point(15, 145);
        this.zipLabel.Size = new Size(30, 20);
        this.zipLabel.Text = "Zip:";

        this.zipTextBox.Location = new Point(75, 145);
        this.zipTextBox.Size = new Size(100, 23);
        this.zipTextBox.Font = new Font("Segoe UI", 9F);
        this.zipTextBox.TabIndex = 8;

        // Add controls to contact info group
        this.contactInfoGroupBox.Controls.Add(this.phoneLabel);
        this.contactInfoGroupBox.Controls.Add(this.phoneTextBox);
        this.contactInfoGroupBox.Controls.Add(this.emailLabel);
        this.contactInfoGroupBox.Controls.Add(this.emailTextBox);
        this.contactInfoGroupBox.Controls.Add(this.addressLabel);
        this.contactInfoGroupBox.Controls.Add(this.addressTextBox);
        this.contactInfoGroupBox.Controls.Add(this.cityLabel);
        this.contactInfoGroupBox.Controls.Add(this.cityTextBox);
        this.contactInfoGroupBox.Controls.Add(this.stateLabel);
        this.contactInfoGroupBox.Controls.Add(this.stateComboBox);
        this.contactInfoGroupBox.Controls.Add(this.zipLabel);
        this.contactInfoGroupBox.Controls.Add(this.zipTextBox);
    }

    private void ConfigureLicenseInfoControls()
    {
        // License Number
        this.licenseNumberLabel.Font = new Font("Segoe UI", 9F);
        this.licenseNumberLabel.Location = new Point(15, 25);
        this.licenseNumberLabel.Size = new Size(90, 20);
        this.licenseNumberLabel.Text = "License Number:";

        this.licenseNumberTextBox.Location = new Point(115, 25);
        this.licenseNumberTextBox.Size = new Size(150, 23);
        this.licenseNumberTextBox.Font = new Font("Segoe UI", 9F);
        this.licenseNumberTextBox.TabIndex = 9;

        // License Type
        this.licenseTypeLabel.Font = new Font("Segoe UI", 9F);
        this.licenseTypeLabel.Location = new Point(15, 55);
        this.licenseTypeLabel.Size = new Size(85, 20);
        this.licenseTypeLabel.Text = "License Type:";

        this.licenseTypeComboBox.Location = new Point(115, 55);
        this.licenseTypeComboBox.Size = new Size(120, 23);
        this.licenseTypeComboBox.Font = new Font("Segoe UI", 9F);
        this.licenseTypeComboBox.Style = Syncfusion.Windows.Forms.VisualStyle.Office2016Colorful;
        this.licenseTypeComboBox.TabIndex = 10;

        // License Class
        this.licenseClassLabel.Font = new Font("Segoe UI", 9F);
        this.licenseClassLabel.Location = new Point(250, 55);
        this.licenseClassLabel.Size = new Size(85, 20);
        this.licenseClassLabel.Text = "License Class:";

        this.licenseClassComboBox.Location = new Point(340, 55);
        this.licenseClassComboBox.Size = new Size(70, 23);
        this.licenseClassComboBox.Font = new Font("Segoe UI", 9F);
        this.licenseClassComboBox.Style = Syncfusion.Windows.Forms.VisualStyle.Office2016Colorful;
        this.licenseClassComboBox.TabIndex = 11;

        // License Issue Date
        this.licenseIssueDateLabel.Font = new Font("Segoe UI", 9F);
        this.licenseIssueDateLabel.Location = new Point(15, 85);
        this.licenseIssueDateLabel.Size = new Size(95, 20);
        this.licenseIssueDateLabel.Text = "Issue Date:";

        this.licenseIssueDatePicker.Location = new Point(115, 85);
        this.licenseIssueDatePicker.Size = new Size(120, 23);
        this.licenseIssueDatePicker.Font = new Font("Segoe UI", 9F);
        this.licenseIssueDatePicker.Style = Syncfusion.Windows.Forms.VisualStyle.Office2016Colorful;
        this.licenseIssueDatePicker.TabIndex = 12;

        // License Expiry Date
        this.licenseExpiryDateLabel.Font = new Font("Segoe UI", 9F);
        this.licenseExpiryDateLabel.Location = new Point(250, 85);
        this.licenseExpiryDateLabel.Size = new Size(85, 20);
        this.licenseExpiryDateLabel.Text = "Expiry Date:";

        this.licenseExpiryDatePicker.Location = new Point(340, 85);
        this.licenseExpiryDatePicker.Size = new Size(120, 23);
        this.licenseExpiryDatePicker.Font = new Font("Segoe UI", 9F);
        this.licenseExpiryDatePicker.Style = Syncfusion.Windows.Forms.VisualStyle.Office2016Colorful;
        this.licenseExpiryDatePicker.TabIndex = 13;

        // Endorsements
        this.endorsementsLabel.Font = new Font("Segoe UI", 9F);
        this.endorsementsLabel.Location = new Point(15, 115);
        this.endorsementsLabel.Size = new Size(85, 20);
        this.endorsementsLabel.Text = "Endorsements:";

        this.endorsementsTextBox.Location = new Point(115, 115);
        this.endorsementsTextBox.Size = new Size(280, 23);
        this.endorsementsTextBox.Font = new Font("Segoe UI", 9F);
        this.endorsementsTextBox.TabIndex = 14;

        // Add controls to license info group
        this.licenseInfoGroupBox.Controls.Add(this.licenseNumberLabel);
        this.licenseInfoGroupBox.Controls.Add(this.licenseNumberTextBox);
        this.licenseInfoGroupBox.Controls.Add(this.licenseTypeLabel);
        this.licenseInfoGroupBox.Controls.Add(this.licenseTypeComboBox);
        this.licenseInfoGroupBox.Controls.Add(this.licenseClassLabel);
        this.licenseInfoGroupBox.Controls.Add(this.licenseClassComboBox);
        this.licenseInfoGroupBox.Controls.Add(this.licenseIssueDateLabel);
        this.licenseInfoGroupBox.Controls.Add(this.licenseIssueDatePicker);
        this.licenseInfoGroupBox.Controls.Add(this.licenseExpiryDateLabel);
        this.licenseInfoGroupBox.Controls.Add(this.licenseExpiryDatePicker);
        this.licenseInfoGroupBox.Controls.Add(this.endorsementsLabel);
        this.licenseInfoGroupBox.Controls.Add(this.endorsementsTextBox);
    }

    private void ConfigureEmploymentInfoControls()
    {
        // Hire Date
        this.hireDateLabel.Font = new Font("Segoe UI", 9F);
        this.hireDateLabel.Location = new Point(15, 25);
        this.hireDateLabel.Size = new Size(70, 20);
        this.hireDateLabel.Text = "Hire Date:";

        this.hireDatePicker.Location = new Point(95, 25);
        this.hireDatePicker.Size = new Size(120, 23);
        this.hireDatePicker.Font = new Font("Segoe UI", 9F);
        this.hireDatePicker.Style = Syncfusion.Windows.Forms.VisualStyle.Office2016Colorful;
        this.hireDatePicker.TabIndex = 15;

        // Status
        this.statusLabel.Font = new Font("Segoe UI", 9F);
        this.statusLabel.Location = new Point(235, 25);
        this.statusLabel.Size = new Size(50, 20);
        this.statusLabel.Text = "Status:";

        this.statusComboBox.Location = new Point(295, 25);
        this.statusComboBox.Size = new Size(120, 23);
        this.statusComboBox.Font = new Font("Segoe UI", 9F);
        this.statusComboBox.Style = Syncfusion.Windows.Forms.VisualStyle.Office2016Colorful;
        this.statusComboBox.TabIndex = 16;

        // Training Complete
        this.trainingCompleteCheckBox.Font = new Font("Segoe UI", 9F);
        this.trainingCompleteCheckBox.Location = new Point(15, 65);
        this.trainingCompleteCheckBox.Size = new Size(150, 20);
        this.trainingCompleteCheckBox.Text = "Training Complete";
        this.trainingCompleteCheckBox.Style = CheckBoxAdvStyle.Office2016Colorful;
        this.trainingCompleteCheckBox.TabIndex = 17;

        // Add controls to employment info group
        this.employmentInfoGroupBox.Controls.Add(this.hireDateLabel);
        this.employmentInfoGroupBox.Controls.Add(this.hireDatePicker);
        this.employmentInfoGroupBox.Controls.Add(this.statusLabel);
        this.employmentInfoGroupBox.Controls.Add(this.statusComboBox);
        this.employmentInfoGroupBox.Controls.Add(this.trainingCompleteCheckBox);
    }

    private void ConfigureActionButton(SfButton button, string title, int x, int y, int tabIndex, Color color)
    {
        button.AccessibleName = title;
        button.BackColor = color;
        button.ForeColor = Color.White;
        button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        button.Location = new Point(x, y);
        button.Name = title.Replace(" ", "").ToLower() + "Button";
        button.Size = new Size(90, 35);
        button.TabIndex = tabIndex;
        button.Text = title;
        button.UseVisualStyleBackColor = false;

        // Add hover effects for better user experience
        button.MouseEnter += (s, e) =>
        {
            if (button.Enabled)
                button.BackColor = Color.FromArgb(Math.Min(255, color.R + 20),
                                                Math.Min(255, color.G + 20),
                                                Math.Min(255, color.B + 20));
        };
        button.MouseLeave += (s, e) =>
        {
            if (button.Enabled)
                button.BackColor = color;
        };
    }

    #endregion
}
