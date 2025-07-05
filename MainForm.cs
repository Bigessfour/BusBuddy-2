using System;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.Themes;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.Windows.Forms;
using Bus_Buddy.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace BusBuddy
{
    /// <summary>
    /// Main application form using available Syncfusion controls from version 30.1.37
    /// Uses MenuStrip + ToolStrip + TabControlAdv instead of unavailable Ribbon controls
    /// </summary>
    public class MainForm : Form
    {
        private MenuStrip? _mainMenuStrip;
        private ToolStrip? _mainToolStrip;
        private TabControlAdv? _mainTabControl;
        private StatusStrip? _statusStrip;
        private readonly IServiceProvider? _serviceProvider;

        public MainForm()
        {
            InitializeComponent();
        }

        public MainForm(IServiceProvider serviceProvider) : this()
        {
            _serviceProvider = serviceProvider;
        }

        private void InitializeComponent()
        {
            // Enable High DPI
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.Text = "Bus Buddy - Transportation Management System";
            this.MinimumSize = new Size(1024, 700);
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Apply Metro theme to form
            Syncfusion.Windows.Forms.SkinManager.SetVisualStyle(this, Syncfusion.Windows.Forms.VisualTheme.Office2016Colorful);

            // Create main menu strip
            CreateMainMenu();

            // Create toolbar
            CreateToolbar();

            // Create main tab control for different modules
            CreateMainTabControl();

            // Create status strip
            CreateStatusStrip();

            // Load Dashboard by default
            LoadDashboard();
        }

        private void CreateMainMenu()
        {
            _mainMenuStrip = new MenuStrip
            {
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(46, 125, 185),
                ForeColor = Color.White
            };

            // File Menu
            var fileMenu = new ToolStripMenuItem("&File");
            fileMenu.DropDownItems.Add(new ToolStripMenuItem("&New", null, (s, e) => { }));
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(new ToolStripMenuItem("E&xit", null, (s, e) => this.Close()));

            // Management Menu
            var managementMenu = new ToolStripMenuItem("&Management");
            managementMenu.DropDownItems.Add(new ToolStripMenuItem("&Buses", null, (s, e) => OpenBusManagement()));
            managementMenu.DropDownItems.Add(new ToolStripMenuItem("&Drivers", null, (s, e) => OpenDriverManagement()));
            managementMenu.DropDownItems.Add(new ToolStripMenuItem("&Routes", null, (s, e) => OpenRouteManagement()));
            managementMenu.DropDownItems.Add(new ToolStripMenuItem("&Students", null, (s, e) => OpenStudentManagement()));
            managementMenu.DropDownItems.Add(new ToolStripSeparator());
            managementMenu.DropDownItems.Add(new ToolStripMenuItem("&Maintenance", null, (s, e) => OpenMaintenanceManagement()));
            managementMenu.DropDownItems.Add(new ToolStripMenuItem("&Fuel", null, (s, e) => OpenFuelManagement()));

            // Reports Menu
            var reportsMenu = new ToolStripMenuItem("&Reports");
            reportsMenu.DropDownItems.Add(new ToolStripMenuItem("Bus &Reports", null, (s, e) => OpenBusReports()));
            reportsMenu.DropDownItems.Add(new ToolStripMenuItem("&Analytics", null, (s, e) => OpenAnalytics()));

            // Help Menu
            var helpMenu = new ToolStripMenuItem("&Help");
            helpMenu.DropDownItems.Add(new ToolStripMenuItem("&About", null, (s, e) => ShowAbout()));

            _mainMenuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, managementMenu, reportsMenu, helpMenu });
            this.Controls.Add(_mainMenuStrip);
            this.MainMenuStrip = _mainMenuStrip;
        }

        private void CreateToolbar()
        {
            _mainToolStrip = new ToolStrip
            {
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(240, 240, 240),
                ImageScalingSize = new Size(32, 32)
            };

            // Add toolbar buttons
            _mainToolStrip.Items.Add(new ToolStripButton("Dashboard", null, (s, e) => LoadDashboard()) { DisplayStyle = ToolStripItemDisplayStyle.ImageAndText });
            _mainToolStrip.Items.Add(new ToolStripSeparator());
            _mainToolStrip.Items.Add(new ToolStripButton("Buses", null, (s, e) => OpenBusManagement()) { DisplayStyle = ToolStripItemDisplayStyle.ImageAndText });
            _mainToolStrip.Items.Add(new ToolStripButton("Drivers", null, (s, e) => OpenDriverManagement()) { DisplayStyle = ToolStripItemDisplayStyle.ImageAndText });
            _mainToolStrip.Items.Add(new ToolStripButton("Routes", null, (s, e) => OpenRouteManagement()) { DisplayStyle = ToolStripItemDisplayStyle.ImageAndText });
            _mainToolStrip.Items.Add(new ToolStripSeparator());
            _mainToolStrip.Items.Add(new ToolStripButton("Reports", null, (s, e) => OpenBusReports()) { DisplayStyle = ToolStripItemDisplayStyle.ImageAndText });

            this.Controls.Add(_mainToolStrip);
        }

        private void CreateMainTabControl()
        {
            _mainTabControl = new TabControlAdv
            {
                Dock = DockStyle.Fill,
                TabStyle = typeof(TabRendererMetro),
                ActiveTabColor = Color.FromArgb(46, 125, 185),
                InactiveTabColor = Color.FromArgb(224, 224, 224),
                BorderVisible = false
            };

            this.Controls.Add(_mainTabControl);
        }

        private void CreateStatusStrip()
        {
            _statusStrip = new StatusStrip
            {
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            _statusStrip.Items.Add(new ToolStripStatusLabel("Ready"));
            _statusStrip.Items.Add(new ToolStripStatusLabel(""));
            _statusStrip.Items.Add(new ToolStripStatusLabel($"Bus Buddy v1.0 - {DateTime.Now:yyyy-MM-dd}"));

            this.Controls.Add(_statusStrip);
        }

        private void LoadDashboard()
        {
            try
            {
                var dashboardTab = FindTabByName("Dashboard");
                if (dashboardTab == null)
                {
                    // Create a simple dashboard placeholder
                    var dashboardPanel = new GradientPanel
                    {
                        Dock = DockStyle.Fill,
                        BorderStyle = BorderStyle.None
                    };

                    var welcomeLabel = new Label
                    {
                        Text = "Welcome to Bus Buddy\nTransportation Management System",
                        Font = new Font("Segoe UI", 16, FontStyle.Bold),
                        ForeColor = Color.FromArgb(46, 125, 185),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Fill
                    };

                    dashboardPanel.Controls.Add(welcomeLabel);

                    dashboardTab = new TabPageAdv("Dashboard");
                    dashboardTab.Controls.Add(dashboardPanel);
                    _mainTabControl?.TabPages.Add(dashboardTab);
                }
                _mainTabControl!.SelectedTab = dashboardTab;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dashboard: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Management Form Methods

        private void OpenBusManagement()
        {
            try
            {
                var existingTab = FindTabByName("Bus Management");
                if (existingTab == null)
                {
                    var form = _serviceProvider?.GetService(typeof(BusManagementForm)) as BusManagementForm;
                    if (form == null)
                    {
                        MessageBox.Show("Bus Management service not available", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    form.TopLevel = false;
                    form.FormBorderStyle = FormBorderStyle.None;
                    form.Dock = DockStyle.Fill;

                    var newTab = new TabPageAdv("Bus Management");
                    newTab.Controls.Add(form);
                    _mainTabControl?.TabPages.Add(newTab);

                    form.Show();
                    existingTab = newTab;
                }
                _mainTabControl!.SelectedTab = existingTab;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Bus Management: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenDriverManagement()
        {
            MessageBox.Show("Driver Management - Under Development", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OpenRouteManagement()
        {
            try
            {
                var existingTab = FindTabByName("Route Management");
                if (existingTab == null)
                {
                    var form = _serviceProvider?.GetService(typeof(RouteManagementForm)) as RouteManagementForm;
                    if (form == null)
                    {
                        MessageBox.Show("Route Management service not available", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    form.TopLevel = false;
                    form.FormBorderStyle = FormBorderStyle.None;
                    form.Dock = DockStyle.Fill;

                    var newTab = new TabPageAdv("Route Management");
                    newTab.Controls.Add(form);
                    _mainTabControl?.TabPages.Add(newTab);

                    form.Show();
                    existingTab = newTab;
                }
                _mainTabControl!.SelectedTab = existingTab;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Route Management: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenStudentManagement()
        {
            try
            {
                var existingTab = FindTabByName("Student Management");
                if (existingTab == null)
                {
                    var form = _serviceProvider?.GetService(typeof(StudentManagementForm)) as StudentManagementForm;
                    if (form == null)
                    {
                        MessageBox.Show("Student Management service not available", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    form.TopLevel = false;
                    form.FormBorderStyle = FormBorderStyle.None;
                    form.Dock = DockStyle.Fill;

                    var newTab = new TabPageAdv("Student Management");
                    newTab.Controls.Add(form);
                    _mainTabControl?.TabPages.Add(newTab);

                    form.Show();
                    existingTab = newTab;
                }
                _mainTabControl!.SelectedTab = existingTab;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Student Management: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenMaintenanceManagement()
        {
            MessageBox.Show("Maintenance Management - Under Development", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OpenFuelManagement()
        {
            MessageBox.Show("Fuel Management - Under Development", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OpenBusReports()
        {
            MessageBox.Show("Bus Reports - Under Development", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OpenAnalytics()
        {
            MessageBox.Show("Analytics module coming soon!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region Helper Methods

        private TabPageAdv? FindTabByName(string name)
        {
            if (_mainTabControl?.TabPages != null)
            {
                foreach (TabPageAdv tab in _mainTabControl.TabPages)
                {
                    if (tab.Text == name)
                        return tab;
                }
            }
            return null;
        }

        private void ShowAbout()
        {
            MessageBox.Show(
                "Bus Buddy - Transportation Management System\n\n" +
                "Version 1.0\n" +
                "Built with Syncfusion Essential Studio v30.1.37\n\n" +
                "© 2025 Bus Buddy Team",
                "About Bus Buddy",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        #endregion
    }
}
