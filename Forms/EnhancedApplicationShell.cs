using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Windows.Forms;
using Bus_Buddy.Services;
using Bus_Buddy.Forms;
using Bus_Buddy.Data.Repositories;
using Bus_Buddy.Utilities;

namespace Bus_Buddy.Forms
{
    /// <summary>
    /// Enhanced Core Application Shell that serves as the main navigation hub
    /// Integrates with existing Dashboard while adding enhanced navigation and features
    /// This demonstrates the composition pattern at the application shell level
    /// </summary>
    public partial class EnhancedApplicationShell : MetroForm
    {
        private readonly ILogger<EnhancedApplicationShell> _logger;
        private readonly IBusService _busService;
        private readonly IConfigurationService _configService;
        private readonly BusRepository _busRepository;
        private readonly IServiceProvider _serviceProvider;

        // Core components
        private Dashboard? _embeddedDashboard;
        private MenuStrip? _enhancedMenuBar;
        private ToolStrip? _enhancedToolbar;
        private StatusStrip? _enhancedStatusBar;
        private Panel? _contentPanel;

        public EnhancedApplicationShell(ILogger<EnhancedApplicationShell> logger,
                                      IBusService busService,
                                      IConfigurationService configService,
                                      BusRepository busRepository,
                                      IServiceProvider serviceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _busService = busService ?? throw new ArgumentNullException(nameof(busService));
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _busRepository = busRepository ?? throw new ArgumentNullException(nameof(busRepository));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            InitializeComponent();
            InitializeEnhancedShell();
        }

        private void InitializeEnhancedShell()
        {
            try
            {
                _logger.LogInformation("Initializing Enhanced Application Shell");

                // Apply enhanced visual theming
                ApplyEnhancedTheme();

                // Create enhanced navigation components
                CreateEnhancedMenuBar();
                CreateEnhancedToolbar();
                CreateEnhancedStatusBar();

                // Embed the existing sophisticated Dashboard
                EmbedExistingDashboard();

                // Setup real-time status updates
                SetupRealTimeUpdates();

                _logger.LogInformation("Enhanced Application Shell initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Enhanced Application Shell");
                throw;
            }
        }

        private void ApplyEnhancedTheme()
        {
            try
            {
                // Apply Syncfusion visual styling
                VisualEnhancementManager.ApplyEnhancedTheme(this);

                this.Text = "Bus Buddy - Enhanced Transportation Management System";
                this.Size = new Size(1400, 900);
                this.StartPosition = FormStartPosition.CenterScreen;
                this.WindowState = FormWindowState.Maximized;

                // Enhanced MetroForm styling
                this.MetroColor = Color.FromArgb(46, 125, 185);
                this.CaptionBarColor = Color.FromArgb(46, 125, 185);
                this.CaptionForeColor = Color.White;

                // Enable high-quality rendering
                VisualEnhancementManager.EnableHighQualityFontRendering(this);

                _logger.LogDebug("Enhanced theme applied to application shell");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not apply enhanced theme");
            }
        }

        private void CreateEnhancedMenuBar()
        {
            try
            {
                _enhancedMenuBar = new MenuStrip
                {
                    BackColor = Color.FromArgb(240, 240, 240),
                    Font = new Font("Segoe UI", 9F)
                };

                // Enhanced Management menu
                var managementMenu = new ToolStripMenuItem("&Management");
                managementMenu.DropDownItems.Add(CreateMenuItem("&Bus Fleet", "Manage buses and vehicles",
                    async (s, e) => await OpenEnhancedBusManagement()));
                managementMenu.DropDownItems.Add(CreateMenuItem("&Drivers", "Manage driver information",
                    (s, e) => OpenDriverManagement()));
                managementMenu.DropDownItems.Add(CreateMenuItem("&Routes", "Manage bus routes",
                    (s, e) => OpenRouteManagement()));
                managementMenu.DropDownItems.Add(CreateMenuItem("Route &Map", "Interactive route mapping with Google Earth Engine",
                    async (s, e) => await OpenEnhancedRouteMap()));
                managementMenu.DropDownItems.Add(CreateMenuItem("&Students", "Manage student information",
                    (s, e) => OpenStudentManagement()));
                managementMenu.DropDownItems.Add(new ToolStripSeparator());
                managementMenu.DropDownItems.Add(CreateMenuItem("&Maintenance", "Vehicle maintenance tracking",
                    (s, e) => OpenMaintenanceManagement()));
                managementMenu.DropDownItems.Add(CreateMenuItem("&Fuel Tracking", "Fuel usage and costs",
                    (s, e) => OpenFuelManagement()));

                // Enhanced Reports menu
                var reportsMenu = new ToolStripMenuItem("&Reports");
                reportsMenu.DropDownItems.Add(CreateMenuItem("&Bus Fleet Reports", "Comprehensive bus reports",
                    (s, e) => OpenBusReports()));
                reportsMenu.DropDownItems.Add(CreateMenuItem("&General Reports", "System reports and analytics",
                    (s, e) => OpenGeneralReports()));
                reportsMenu.DropDownItems.Add(new ToolStripSeparator());
                reportsMenu.DropDownItems.Add(CreateMenuItem("&Export Data", "Export data to various formats",
                    (s, e) => ShowExportOptions()));

                // Enhanced Tools menu
                var toolsMenu = new ToolStripMenuItem("&Tools");
                toolsMenu.DropDownItems.Add(CreateMenuItem("&Refresh All Data", "Refresh all system data",
                    async (s, e) => await RefreshAllData()));
                toolsMenu.DropDownItems.Add(CreateMenuItem("&System Status", "View system health",
                    async (s, e) => await ShowSystemStatus()));
                toolsMenu.DropDownItems.Add(new ToolStripSeparator());
                toolsMenu.DropDownItems.Add(CreateMenuItem("&Settings", "Application settings",
                    (s, e) => OpenSettings()));

                // Enhanced Help menu
                var helpMenu = new ToolStripMenuItem("&Help");
                helpMenu.DropDownItems.Add(CreateMenuItem("&User Guide", "Open user documentation",
                    (s, e) => OpenUserGuide()));
                helpMenu.DropDownItems.Add(CreateMenuItem("&About Bus Buddy", "About this application",
                    (s, e) => ShowAbout()));

                _enhancedMenuBar.Items.AddRange(new ToolStripItem[] { managementMenu, reportsMenu, toolsMenu, helpMenu });
                this.Controls.Add(_enhancedMenuBar);
                this.MainMenuStrip = _enhancedMenuBar;

                _logger.LogDebug("Enhanced menu bar created");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not create enhanced menu bar");
            }
        }

        private void CreateEnhancedToolbar()
        {
            try
            {
                _enhancedToolbar = new ToolStrip
                {
                    Dock = DockStyle.Top,
                    BackColor = Color.FromArgb(250, 250, 250),
                    Font = new Font("Segoe UI", 9F)
                };

                // Quick access buttons for most common operations
                _enhancedToolbar.Items.Add(CreateToolStripButton("Bus Fleet", "Open enhanced bus management",
                    async (s, e) => await OpenEnhancedBusManagement()));
                _enhancedToolbar.Items.Add(CreateToolStripButton("Reports", "Open bus fleet reports",
                    (s, e) => OpenBusReports()));
                _enhancedToolbar.Items.Add(new ToolStripSeparator());
                _enhancedToolbar.Items.Add(CreateToolStripButton("Refresh", "Refresh dashboard data",
                    async (s, e) => await RefreshDashboard()));
                _enhancedToolbar.Items.Add(new ToolStripSeparator());

                // Real-time status indicators
                var statusIndicator = new ToolStripLabel("System Status: ")
                {
                    Name = "SystemStatusIndicator",
                    ForeColor = Color.FromArgb(46, 125, 185)
                };
                _enhancedToolbar.Items.Add(statusIndicator);

                var fleetSummary = new ToolStripLabel("Fleet: Loading...")
                {
                    Name = "FleetSummaryIndicator",
                    ForeColor = Color.FromArgb(95, 99, 104)
                };
                _enhancedToolbar.Items.Add(fleetSummary);

                this.Controls.Add(_enhancedToolbar);
                _logger.LogDebug("Enhanced toolbar created");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not create enhanced toolbar");
            }
        }

        private void CreateEnhancedStatusBar()
        {
            try
            {
                _enhancedStatusBar = new StatusStrip
                {
                    BackColor = Color.FromArgb(240, 240, 240),
                    Font = new Font("Segoe UI", 9F)
                };

                _enhancedStatusBar.Items.Add(new ToolStripStatusLabel("Ready") { Name = "MainStatus" });
                _enhancedStatusBar.Items.Add(new ToolStripStatusLabel() { Spring = true }); // Spacer
                _enhancedStatusBar.Items.Add(new ToolStripStatusLabel("User: System") { Name = "UserStatus" });
                _enhancedStatusBar.Items.Add(new ToolStripStatusLabel($"Version: 2.0") { Name = "VersionStatus" });
                _enhancedStatusBar.Items.Add(new ToolStripStatusLabel($"Time: {DateTime.Now:HH:mm}") { Name = "TimeStatus" });

                this.Controls.Add(_enhancedStatusBar);
                _logger.LogDebug("Enhanced status bar created");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not create enhanced status bar");
            }
        }

        private void EmbedExistingDashboard()
        {
            try
            {
                // Create main content panel with splitter for dashboard and analytics
                _contentPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White
                };
                this.Controls.Add(_contentPanel);

                // Create splitter for dashboard + analytics layout
                var mainSplitter = new SplitContainer
                {
                    Dock = DockStyle.Fill,
                    SplitterDistance = 1000, // Dashboard gets most of the space
                    FixedPanel = FixedPanel.Panel2, // Analytics panel is fixed width
                    IsSplitterFixed = false,
                    SplitterWidth = 5
                };
                _contentPanel.Controls.Add(mainSplitter);

                // Create the existing sophisticated Dashboard
                var dashboardLogger = _serviceProvider.GetRequiredService<ILogger<Dashboard>>();
                var xaiService = _serviceProvider.GetRequiredService<XAIService>();
                var geeService = _serviceProvider.GetRequiredService<GoogleEarthEngineService>();
                _embeddedDashboard = new Dashboard(dashboardLogger, _busService, _configService, xaiService, geeService, _serviceProvider);

                // Embed the dashboard in the left panel
                _embeddedDashboard.TopLevel = false;
                _embeddedDashboard.FormBorderStyle = FormBorderStyle.None;
                _embeddedDashboard.Dock = DockStyle.Fill;
                mainSplitter.Panel1.Controls.Add(_embeddedDashboard);
                _embeddedDashboard.Show();

                // Add Enhanced Dashboard Analytics to the right panel
                AddEnhancedAnalyticsPanel(mainSplitter.Panel2);

                _logger.LogDebug("Existing Dashboard embedded with enhanced analytics");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error embedding existing Dashboard with analytics");
            }
        }

        private void AddEnhancedAnalyticsPanel(Control parentPanel)
        {
            try
            {
                // Create analytics header
                var analyticsHeader = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 40,
                    BackColor = Color.FromArgb(46, 125, 185)
                };

                var headerLabel = new Label
                {
                    Text = "📊 Real-Time Analytics",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.White,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                analyticsHeader.Controls.Add(headerLabel);
                parentPanel.Controls.Add(analyticsHeader);

                // Create and add the Enhanced Dashboard Analytics
                var analyticsLogger = _serviceProvider.GetRequiredService<ILogger<EnhancedDashboardAnalytics>>();
                var analyticsPanel = new EnhancedDashboardAnalytics(analyticsLogger, _busRepository, _busService)
                {
                    Dock = DockStyle.Fill
                };
                parentPanel.Controls.Add(analyticsPanel);

                _logger.LogDebug("Enhanced Analytics Panel added to dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding Enhanced Analytics Panel");
            }
        }

        private void SetupRealTimeUpdates()
        {
            try
            {
                // Setup timer for real-time updates
                var updateTimer = new System.Windows.Forms.Timer { Interval = 30000 }; // 30 seconds
                updateTimer.Tick += async (s, e) => await UpdateRealTimeStatus();
                updateTimer.Start();

                // Initial status update
                _ = Task.Run(async () => await UpdateRealTimeStatus());

                _logger.LogDebug("Real-time updates configured");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not setup real-time updates");
            }
        }

        private async Task UpdateRealTimeStatus()
        {
            try
            {
                var totalBuses = await _busRepository.GetTotalVehicleCountAsync();
                var activeBuses = await _busRepository.GetActiveVehicleCountAsync();
                var maintenanceNeeded = (await _busRepository.GetVehiclesDueForMaintenanceAsync()).Count();

                this.Invoke(() =>
                {
                    // Update toolbar status
                    var systemStatus = _enhancedToolbar?.Items["SystemStatusIndicator"] as ToolStripLabel;
                    if (systemStatus != null)
                    {
                        systemStatus.Text = maintenanceNeeded > 0 ? "⚠ Attention Needed" : "✓ All Systems Normal";
                        systemStatus.ForeColor = maintenanceNeeded > 0 ? Color.FromArgb(255, 152, 0) : Color.FromArgb(46, 204, 113);
                    }

                    var fleetSummary = _enhancedToolbar?.Items["FleetSummaryIndicator"] as ToolStripLabel;
                    if (fleetSummary != null)
                    {
                        fleetSummary.Text = $"Fleet: {activeBuses}/{totalBuses} Active";
                    }

                    // Update status bar time
                    var timeStatus = _enhancedStatusBar?.Items["TimeStatus"] as ToolStripStatusLabel;
                    if (timeStatus != null)
                    {
                        timeStatus.Text = $"Time: {DateTime.Now:HH:mm}";
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error updating real-time status");
            }
        }

        #region Enhanced Navigation Methods

        private async Task OpenEnhancedBusManagement()
        {
            try
            {
                _logger.LogInformation("Opening Enhanced Bus Management");

                var logger = _serviceProvider.GetRequiredService<ILogger<EnhancedBusManagementForm>>();
                var enhancedBusForm = new EnhancedBusManagementForm(logger, _busService, _busRepository);
                enhancedBusForm.ShowDialog(this);

                // Refresh dashboard after closing
                await RefreshDashboard();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening Enhanced Bus Management");
                ShowErrorMessage("Error opening Enhanced Bus Management", ex.Message);
            }
        }

        private void OpenBusReports()
        {
            try
            {
                _logger.LogInformation("Opening Bus Reports");

                var busReportsForm = new BusBuddy.Forms.BusReportsForm(_busRepository);
                busReportsForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening Bus Reports");
                ShowErrorMessage("Error opening Bus Reports", ex.Message);
            }
        }

        private void OpenDriverManagement()
        {
            try
            {
                var driverForm = ServiceContainer.GetService<DriverManagementForm>();
                driverForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening Driver Management");
                ShowErrorMessage("Error opening Driver Management", ex.Message);
            }
        }

        private void OpenRouteManagement()
        {
            try
            {
                var routeForm = ServiceContainer.GetService<RouteManagementForm>();
                routeForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening Route Management");
                ShowErrorMessage("Error opening Route Management", ex.Message);
            }
        }

        private async Task OpenEnhancedRouteMap()
        {
            try
            {
                _logger.LogInformation("Opening Enhanced Route Map with Google Earth Engine");

                var logger = _serviceProvider.GetRequiredService<ILogger<EnhancedRouteMapForm>>();
                var routeRepository = _serviceProvider.GetRequiredService<RouteRepository>();
                var enhancedRouteMapForm = new EnhancedRouteMapForm(logger, routeRepository, _busRepository);
                enhancedRouteMapForm.ShowDialog(this);

                // Refresh dashboard after closing
                await RefreshDashboard();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening Enhanced Route Map");
                ShowErrorMessage("Error opening Enhanced Route Map", ex.Message);
            }
        }

        private void OpenStudentManagement()
        {
            try
            {
                var studentForm = ServiceContainer.GetService<Bus_Buddy.Forms.StudentManagementForm>();
                studentForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening Student Management");
                ShowErrorMessage("Error opening Student Management", ex.Message);
            }
        }

        private void OpenMaintenanceManagement()
        {
            try
            {
                var maintenanceForm = ServiceContainer.GetService<Bus_Buddy.Forms.MaintenanceManagementForm>();
                maintenanceForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening Maintenance Management");
                ShowErrorMessage("Error opening Maintenance Management", ex.Message);
            }
        }

        private void OpenFuelManagement()
        {
            try
            {
                var fuelForm = ServiceContainer.GetService<Bus_Buddy.Forms.FuelManagementForm>();
                fuelForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening Fuel Management");
                ShowErrorMessage("Error opening Fuel Management", ex.Message);
            }
        }

        private void OpenGeneralReports()
        {
            try
            {
                var reportsForm = new BusBuddy.Forms.ReportsForm();
                reportsForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening General Reports");
                ShowErrorMessage("Error opening General Reports", ex.Message);
            }
        }

        #endregion

        #region Helper Methods

        private ToolStripMenuItem CreateMenuItem(string text, string tooltip, EventHandler onClick)
        {
            return new ToolStripMenuItem(text)
            {
                ToolTipText = tooltip,
                Tag = onClick
            };
        }

        private ToolStripMenuItem CreateMenuItem(string text, string tooltip, Func<object, EventArgs, Task> onClickAsync)
        {
            var item = new ToolStripMenuItem(text) { ToolTipText = tooltip };
            item.Click += async (s, e) =>
            {
                if (s != null && e != null)
                    await onClickAsync(s, e);
            };
            return item;
        }

        private ToolStripButton CreateToolStripButton(string text, string tooltip, EventHandler onClick)
        {
            var button = new ToolStripButton(text) { ToolTipText = tooltip };
            button.Click += onClick;
            return button;
        }

        private ToolStripButton CreateToolStripButton(string text, string tooltip, Func<object, EventArgs, Task> onClickAsync)
        {
            var button = new ToolStripButton(text) { ToolTipText = tooltip };
            button.Click += async (s, e) =>
            {
                if (s != null && e != null)
                    await onClickAsync(s, e);
            };
            return button;
        }

        private async Task RefreshDashboard()
        {
            try
            {
                SetStatus("Refreshing dashboard data...");

                // Refresh the embedded dashboard
                if (_embeddedDashboard != null)
                {
                    // Force refresh by simulating the refresh button click
                    _embeddedDashboard.Invoke(() =>
                    {
                        var refreshMethod = _embeddedDashboard.GetType().GetMethod("RefreshButton_Click",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        refreshMethod?.Invoke(_embeddedDashboard, new object[] { this, EventArgs.Empty });
                    });
                }

                // Update real-time status
                await UpdateRealTimeStatus();

                SetStatus("Dashboard refreshed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing dashboard");
                SetStatus("Error refreshing dashboard");
            }
        }

        private async Task RefreshAllData()
        {
            await RefreshDashboard();
            SetStatus("All data refreshed");
        }

        private async Task ShowSystemStatus()
        {
            try
            {
                var totalBuses = await _busRepository.GetTotalVehicleCountAsync();
                var activeBuses = await _busRepository.GetActiveVehicleCountAsync();
                var maintenanceNeeded = (await _busRepository.GetVehiclesDueForMaintenanceAsync()).Count();
                var inspectionNeeded = (await _busRepository.GetVehiclesDueForInspectionAsync()).Count();

                var statusMessage = $"System Status Report:\n\n" +
                    $"Fleet Overview:\n" +
                    $"• Total Vehicles: {totalBuses}\n" +
                    $"• Active Vehicles: {activeBuses}\n" +
                    $"• Maintenance Needed: {maintenanceNeeded}\n" +
                    $"• Inspection Needed: {inspectionNeeded}\n\n" +
                    $"System Health: {(maintenanceNeeded + inspectionNeeded == 0 ? "All Systems Normal" : "Attention Required")}";

                MessageBox.Show(statusMessage, "System Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing system status");
                ShowErrorMessage("Error loading system status", ex.Message);
            }
        }

        private void ShowExportOptions()
        {
            MessageBox.Show("Export functionality will be implemented here.\n\nSupported formats:\n• PDF Reports\n• Excel Spreadsheets\n• CSV Data Files",
                "Export Options", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OpenSettings()
        {
            MessageBox.Show("Settings dialog will be implemented here.\n\nSettings categories:\n• Application Preferences\n• Database Configuration\n• User Management",
                "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OpenUserGuide()
        {
            MessageBox.Show("User guide will be implemented here.\n\nDocumentation will include:\n• Getting Started\n• Feature Overview\n• Best Practices",
                "User Guide", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowAbout()
        {
            MessageBox.Show($"Bus Buddy - Enhanced Transportation Management System\n\n" +
                $"Version: 2.0\n" +
                $"Built with: Syncfusion Essential Studio 30.1.37\n" +
                $"Framework: .NET with Entity Framework\n\n" +
                $"© 2025 Bus Buddy Development Team",
                "About Bus Buddy", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SetStatus(string message)
        {
            try
            {
                this.Invoke(() =>
                {
                    var mainStatus = _enhancedStatusBar?.Items["MainStatus"] as ToolStripStatusLabel;
                    if (mainStatus != null)
                    {
                        mainStatus.Text = message;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not set status message");
            }
        }

        private void ShowErrorMessage(string title, string message)
        {
            MessageBox.Show($"{title}:\n\n{message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _embeddedDashboard?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
