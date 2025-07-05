using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Bus_Buddy.Services;
using Bus_Buddy.Forms;
using Bus_Buddy.Utilities;
using Bus_Buddy.Data.Interfaces;
using Bus_Buddy.Data.Repositories;
using BusBuddy.Forms;
using BusBuddy.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bus_Buddy;

public partial class Dashboard : MetroForm
{
    private readonly ILogger<Dashboard> _logger;
    private readonly IBusService _busService;
    private readonly IConfigurationService _configService;
    private readonly XAIService _xaiService;
    private readonly GoogleEarthEngineService _geeService;
    private readonly BusBuddyAIReportingService _aiReportingService;
    private readonly IServiceProvider _serviceProvider;
    private AIAssistantPanel? _aiAssistantPanel;

    // Main tab control for dashboard sections
    private TabControlAdv? mainTabControl;
    private TabPageAdv? fleetTab;
    private TabPageAdv? routesTab;
    private TabPageAdv? maintenanceTab;
    private TabPageAdv? studentsTab;
    private TabPageAdv? reportsTab;
    private TabPageAdv? aiAssistantTab;
    private TabPageAdv? analyticsTab;

    // Dashboard controls
    private Label? subtitleLabel;

    public Dashboard(ILogger<Dashboard> logger, IBusService busService, IConfigurationService configService,
                     XAIService xaiService, GoogleEarthEngineService geeService, BusBuddyAIReportingService aiReportingService, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _busService = busService;
        _configService = configService;
        _xaiService = xaiService;
        _geeService = geeService;
        _aiReportingService = aiReportingService;
        _serviceProvider = serviceProvider;

        _logger.LogInformation("Initializing Dashboard form with AI capabilities");

        InitializeComponent();

        // FIX: Eliminate background bleeding behind dashboard cards
        SyncfusionBackgroundFix.FixDashboardBackground(this);

        InitializeDashboard();
    }

    private void InitializeDashboard()
    {
        // Apply enhanced visual theme system
        VisualEnhancementManager.ApplyEnhancedTheme(this);

        // Set Syncfusion MetroForm styles with enhanced colors
        this.MetroColor = System.Drawing.Color.FromArgb(46, 125, 185);
        this.CaptionBarColor = System.Drawing.Color.FromArgb(46, 125, 185);
        this.CaptionForeColor = System.Drawing.Color.White;
        this.Text = "BusBuddy - Dashboard";

        // Enable high DPI scaling for this form
        this.AutoScaleMode = AutoScaleMode.Dpi;
        this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);

        // Create main tab control
        InitializeTabControl();

        // Initialize subtitle label
        InitializeSubtitleLabel();

        // Initialize AI Assistant Panel
        InitializeAIAssistantPanel();

        _logger.LogInformation("Dashboard form initialized with tab-based layout and AI capabilities");

        // Load initial data
        LoadDashboardDataAsync();
    }

    private void InitializeTabControl()
    {
        // Create main tab control
        mainTabControl = new TabControlAdv
        {
            Location = new System.Drawing.Point(10, 10),
            Size = new System.Drawing.Size(1180, 700),
            Dock = DockStyle.Fill,
            TabStyle = typeof(TabRendererMetro),
            ThemeName = "Metro"
        };

        // Create Fleet Management tab
        fleetTab = new TabPageAdv("Fleet Management");
        var fleetForm = new BusManagementForm(
            _serviceProvider.GetRequiredService<ILogger<BusManagementForm>>(),
            _busService);
        fleetForm.TopLevel = false;
        fleetForm.FormBorderStyle = FormBorderStyle.None;
        fleetForm.Dock = DockStyle.Fill;
        fleetTab.Controls.Add(fleetForm);
        fleetForm.Show();

        // Create Routes tab
        routesTab = new TabPageAdv("Routes");
        var routeForm = new RouteManagementForm(
            _serviceProvider.GetRequiredService<ILogger<RouteManagementForm>>(),
            _busService);
        routeForm.TopLevel = false;
        routeForm.FormBorderStyle = FormBorderStyle.None;
        routeForm.Dock = DockStyle.Fill;
        routesTab.Controls.Add(routeForm);
        routeForm.Show();

        // Create Maintenance tab
        maintenanceTab = new TabPageAdv("Maintenance");
        var maintenanceForm = new MaintenanceManagementForm(
            _busService,
            _serviceProvider.GetRequiredService<IMaintenanceService>(),
            _serviceProvider.GetRequiredService<ILogger<MaintenanceManagementForm>>());
        maintenanceForm.TopLevel = false;
        maintenanceForm.FormBorderStyle = FormBorderStyle.None;
        maintenanceForm.Dock = DockStyle.Fill;
        maintenanceTab.Controls.Add(maintenanceForm);
        maintenanceForm.Show();

        // Create Students tab
        studentsTab = new TabPageAdv("Students");
        var studentForm = new StudentManagementForm(
            _serviceProvider.GetRequiredService<ILogger<StudentManagementForm>>(),
            _serviceProvider.GetRequiredService<IStudentService>(),
            _busService);
        studentForm.TopLevel = false;
        studentForm.FormBorderStyle = FormBorderStyle.None;
        studentForm.Dock = DockStyle.Fill;
        studentsTab.Controls.Add(studentForm);
        studentForm.Show();

        // Create Reports tab
        reportsTab = new TabPageAdv("Reports");
        var reportsForm = new BusReportsForm(_serviceProvider.GetRequiredService<BusRepository>());
        reportsForm.TopLevel = false;
        reportsForm.FormBorderStyle = FormBorderStyle.None;
        reportsForm.Dock = DockStyle.Fill;
        reportsTab.Controls.Add(reportsForm);
        reportsForm.Show();

        // Create Analytics tab with Enhanced Dashboard Analytics
        analyticsTab = new TabPageAdv("Analytics");
        var analyticsPanel = _serviceProvider.GetRequiredService<EnhancedDashboardAnalytics>();
        analyticsPanel.Dock = DockStyle.Fill;
        analyticsTab.Controls.Add(analyticsPanel);

        // Create AI Assistant tab
        aiAssistantTab = new TabPageAdv("AI Assistant");

        // Add tabs to control (Analytics first for prominence)
        mainTabControl.TabPages.Add(analyticsTab);
        mainTabControl.TabPages.Add(fleetTab);
        mainTabControl.TabPages.Add(routesTab);
        mainTabControl.TabPages.Add(maintenanceTab);
        mainTabControl.TabPages.Add(studentsTab);
        mainTabControl.TabPages.Add(reportsTab);
        mainTabControl.TabPages.Add(aiAssistantTab);

        // Add tab control to form
        this.Controls.Add(mainTabControl);
    }

    private void InitializeAIAssistantPanel()
    {
        try
        {
            // Create AI Assistant Panel with integrated xAI chat
            _aiAssistantPanel = new AIAssistantPanel(
                ServiceContainer.GetService<ILogger<AIAssistantPanel>>(),
                _xaiService,
                _geeService
            );

            // Position the AI Assistant Panel where the old summary panel was
            _aiAssistantPanel.Location = new System.Drawing.Point(750, 30);
            _aiAssistantPanel.Size = new System.Drawing.Size(420, 420);

            // Add to form's controls
            this.Controls.Add(_aiAssistantPanel);

            _logger.LogInformation("AI Assistant Panel initialized and added to dashboard successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing AI Assistant Panel");
            // Create a simple fallback label if AI initialization fails
            var fallbackLabel = new Label
            {
                Text = "AI Assistant Panel\nInitialization Error\n\nCheck logs for details",
                Location = new System.Drawing.Point(750, 30),
                Size = new System.Drawing.Size(420, 420),
                BackColor = System.Drawing.Color.FromArgb(248, 249, 250),
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(158, 158, 158)
            };
            this.Controls.Add(fallbackLabel);
        }
    }

    private void InitializeSubtitleLabel()
    {
        subtitleLabel = new Label
        {
            Text = "Loading fleet information...",
            Location = new System.Drawing.Point(20, 50),
            Size = new System.Drawing.Size(700, 30),
            Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular),
            ForeColor = System.Drawing.Color.FromArgb(102, 102, 102),
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft
        };

        this.Controls.Add(subtitleLabel);
    }

    private async void LoadDashboardDataAsync()
    {
        try
        {
            _logger.LogInformation("Loading dashboard data");

            // Load buses and routes data
            var buses = await _busService.GetAllBusesAsync();
            var routes = await _busService.GetAllRoutesAsync();

            _logger.LogInformation("Loaded {BusCount} buses and {RouteCount} routes", buses.Count, routes.Count);

            // Update UI with loaded data using Syncfusion dashboard components
            UpdateDashboardSummary(buses, routes);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard data");
            MessageBoxAdv.Show($"Error loading dashboard data: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Update dashboard summary using the AI Assistant Panel
    /// </summary>
    private void UpdateDashboardSummary(System.Collections.Generic.List<Bus_Buddy.Services.BusInfo> buses, System.Collections.Generic.List<Bus_Buddy.Services.RouteInfo> routes)
    {
        try
        {
            // Calculate summary statistics
            var activeBuses = buses.Count(b => b.Status == "Active");
            var inServiceBuses = buses.Count(b => b.Status == "In Service");
            var maintenanceBuses = buses.Count(b => b.Status == "Maintenance");
            var totalCapacity = buses.Sum(b => b.Capacity);

            // Update the subtitle with current statistics
            if (subtitleLabel != null)
            {
                subtitleLabel.Text = $"Fleet: {buses.Count} buses | Routes: {routes.Count} | Active: {activeBuses} | Total Capacity: {totalCapacity:N0} passengers | AI: {(_xaiService.IsConfigured ? "✅ Live" : "⚠️ Mock")}";
            }

            // Update AI Assistant Panel with fleet summary
            _aiAssistantPanel?.UpdateFleetSummary(buses.Count, routes.Count, activeBuses, maintenanceBuses, totalCapacity);

            _logger.LogInformation("Dashboard summary updated successfully with AI integration");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating dashboard summary: {Message}", ex.Message);
        }
    }

    #region Button Event Handlers

    // Management Module Event Handlers
    private void BusManagementButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Bus Management button clicked");

            // Open Bus Management form using the service container
            var busManagementForm = ServiceContainer.GetService<BusManagementForm>();
            busManagementForm.ShowDialog(this);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening Bus Management");
            MessageBoxAdv.Show($"Error opening Bus Management: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DriverManagementButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Driver Management button clicked");

            // Open Driver Management form using the service container
            var driverManagementForm = ServiceContainer.GetService<DriverManagementForm>();
            driverManagementForm.ShowDialog(this);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening Driver Management");
            MessageBoxAdv.Show($"Error opening Driver Management: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RouteManagementButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Route Management button clicked");

            // Open Route Management form using the service container
            var routeManagementForm = ServiceContainer.GetService<RouteManagementForm>();
            routeManagementForm.ShowDialog(this);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening Route Management");
            MessageBoxAdv.Show($"Error opening Route Management: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ScheduleManagementButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Schedule Management button clicked");

            // Open Schedule Management form
            var scheduleForm = ServiceContainer.GetService<ScheduleManagementForm>();
            scheduleForm.ShowDialog();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening Schedule Management");
            MessageBoxAdv.Show($"Error opening Schedule Management: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void PassengerManagementButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Passenger Management button clicked");

            // Open Passenger Management form using the service container
            var passengerManagementForm = ServiceContainer.GetService<Bus_Buddy.Forms.PassengerManagementForm>();
            passengerManagementForm.ShowDialog(this);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening Passenger Management");
            MessageBoxAdv.Show($"Error opening Passenger Management: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void StudentManagementButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Student Management button clicked");

            // Open Student Management form using the service container
            var studentManagementForm = ServiceContainer.GetService<Bus_Buddy.Forms.StudentManagementForm>();
            studentManagementForm.ShowDialog(this);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening Student Management");
            MessageBoxAdv.Show($"Error opening Student Management: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void MaintenanceButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Maintenance button clicked");

            // Open Maintenance Management form using the service container
            var maintenanceForm = ServiceContainer.GetService<Bus_Buddy.Forms.MaintenanceManagementForm>();
            maintenanceForm.ShowDialog(this);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening Maintenance Management");
            MessageBoxAdv.Show($"Error opening Maintenance Management: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void FuelTrackingButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Fuel Tracking button clicked");

            // Open Fuel Management form using the service container
            var fuelForm = ServiceContainer.GetService<Bus_Buddy.Forms.FuelManagementForm>();
            fuelForm.ShowDialog(this);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening Fuel Tracking");
            MessageBoxAdv.Show($"Error opening Fuel Tracking: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ActivityLogButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Activity Log button clicked");

            // TODO: Open Activity Log form
            MessageBoxAdv.Show("Activity Log functionality will be implemented here.\n\nThis will open a form to view:\n• System activities\n• User actions\n• Audit trails",
                "Activity Log", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening Activity Log");
            MessageBoxAdv.Show($"Error opening Activity Log: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void TicketManagementButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Ticket Management button clicked");

            // Open Ticket Management form using the service container
            var ticketForm = ServiceContainer.GetService<Bus_Buddy.Forms.TicketManagementForm>();
            ticketForm.ShowDialog(this);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening Ticket Management");
            MessageBoxAdv.Show($"Error opening Ticket Management: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // Quick Action Event Handlers
    private async void RefreshButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Refresh button clicked");

            // Reload dashboard data
            await Task.Run(() => LoadDashboardDataAsync());

            MessageBoxAdv.Show("Data refreshed successfully", "Refresh Complete",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during refresh operation");
            MessageBoxAdv.Show($"Refresh error: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ReportsButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Reports button clicked");

            // TODO: Open Reports form or dialog
            MessageBoxAdv.Show("Reports functionality will be implemented here.\n\nThis will provide:\n• Fleet reports\n• Financial reports\n• Performance analytics",
                "Reports", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening Reports");
            MessageBoxAdv.Show($"Reports error: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AIAssistantButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("AI Assistant button clicked");

            // Switch to AI Chat tab in the assistant panel
            _aiAssistantPanel?.ShowAIChat();

            MessageBoxAdv.Show($"🤖 AI Assistant Status: {(_xaiService.IsConfigured ? "Live xAI (Grok) Ready" : "Mock Mode")}\n\n" +
                              "The AI chat interface is located in the right panel.\n" +
                              "You can ask about:\n" +
                              "• Route optimization\n" +
                              "• Maintenance predictions\n" +
                              "• Safety analysis\n" +
                              "• Student assignments\n" +
                              "• Fleet performance",
                              "AI Assistant", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error with AI Assistant");
            MessageBoxAdv.Show($"AI Assistant error: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SettingsButton_Click(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Settings button clicked");

            // TODO: Open settings dialog
            MessageBoxAdv.Show("Settings functionality will be implemented here.\n\nThis will provide:\n• Application settings\n• Database configuration\n• User preferences",
                "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening settings");
            MessageBoxAdv.Show($"Settings error: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Opens the Enhanced Grid Layout Demo form to showcase improved Syncfusion grid formatting
    /// </summary>
    private void OpenEnhancedGridDemo()
    {
        try
        {
            _logger.LogInformation("Opening Enhanced Grid Layout Demo");

            var demoForm = new EnhancedGridDemoForm();
            demoForm.Show(); // Use Show() instead of ShowDialog() to allow multiple instances
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening Enhanced Grid Demo");
            MessageBoxAdv.Show($"Error opening Enhanced Grid Demo: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Opens the Visual Enhancement Showcase to demonstrate comprehensive visual improvements
    /// </summary>
    private void OpenVisualEnhancementShowcase()
    {
        try
        {
            _logger.LogInformation("Opening Visual Enhancement Showcase");

            var showcaseForm = ServiceContainer.GetService<VisualEnhancementShowcaseForm>();
            showcaseForm.Show(); // Use Show() to allow multiple instances
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening Visual Enhancement Showcase");
            MessageBoxAdv.Show($"Error opening Visual Enhancement Showcase: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    #endregion
}
