using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.DataGrid.Enums;
using Bus_Buddy.Data.Repositories;
using Bus_Buddy.Services;
using BusBuddy.Services;
using Bus_Buddy.Models;
using static Bus_Buddy.Services.SmartRouteOptimizationService;

namespace Bus_Buddy.Forms
{
    /// <summary>
    /// Enhanced Dashboard Analytics Panel with AI-powered insights for production use
    /// Provides real-time fleet management analytics and AI recommendations
    /// </summary>
    public partial class EnhancedDashboardAnalytics : UserControl
    {
        private readonly ILogger<EnhancedDashboardAnalytics> _logger;
        private readonly BusRepository _busRepository;
        private readonly IBusService _busService;
        private readonly BusBuddyAIReportingService _aiReportingService;
        private readonly SmartRouteOptimizationService _routeOptimizationService;

        // UI Components
        private SfDataGrid? _analyticsGrid;
        private Panel? _chartsPanel;
        private Panel? _kpiPanel;
        private Panel? _aiInsightsPanel;
        private Panel? _routeOptimizationPanel;
        private RichTextBox? _aiInsightsTextBox;
        private RichTextBox? _routeInsightsTextBox;
        private Button? _generateInsightsButton;
        private Button? _optimizeRoutesButton;
        private Label? _totalBusesLabel;
        private Label? _activeBusesLabel;
        private Label? _maintenanceDueLabel;
        private Label? _inspectionDueLabel;
        private System.Windows.Forms.Timer? _realTimeUpdateTimer;

        public EnhancedDashboardAnalytics(ILogger<EnhancedDashboardAnalytics> logger,
                                        BusRepository busRepository,
                                        IBusService busService,
                                        BusBuddyAIReportingService aiReportingService,
                                        SmartRouteOptimizationService routeOptimizationService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _busRepository = busRepository ?? throw new ArgumentNullException(nameof(busRepository));
            _busService = busService ?? throw new ArgumentNullException(nameof(busService));
            _aiReportingService = aiReportingService ?? throw new ArgumentNullException(nameof(aiReportingService));
            _routeOptimizationService = routeOptimizationService ?? throw new ArgumentNullException(nameof(routeOptimizationService));

            InitializeComponent();
            InitializeAnalyticsLayout();
            SetupRealTimeUpdates();
            _ = LoadDashboardDataAsync();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Name = "EnhancedDashboardAnalytics";
            this.Size = new Size(400, 600);
            this.ResumeLayout(false);
        }

        private void InitializeAnalyticsLayout()
        {
            try
            {
                this.BackColor = Color.FromArgb(248, 249, 250);
                this.Size = new Size(1200, 800);

                // Create KPI Panel at top
                CreateKPIPanel();

                // Create AI Insights Panel
                CreateAIInsightsPanel();

                // Create Route Optimization Panel
                CreateRouteOptimizationPanel();

                // Create analytics grid in the middle
                _analyticsGrid = new SfDataGrid()
                {
                    Location = new Point(10, 160),
                    Size = new Size(580, 300),
                    AllowResizingColumns = true,
                    AutoGenerateColumns = false,
                    HeaderRowHeight = 35,
                    RowHeight = 30
                };
                SetupAnalyticsGrid();

                // Simple chart panel at bottom
                _chartsPanel = new Panel()
                {
                    Location = new Point(10, 470),
                    Size = new Size(1180, 200),
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle
                };

                var chartLabel = new Label()
                {
                    Text = "📊 Fleet Analytics Charts\n🚌 Real-time Fleet Status Visualization",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.FromArgb(46, 125, 185)
                };
                _chartsPanel.Controls.Add(chartLabel);

                this.Controls.Add(_kpiPanel);
                this.Controls.Add(_analyticsGrid);
                this.Controls.Add(_aiInsightsPanel);
                this.Controls.Add(_routeOptimizationPanel);
                this.Controls.Add(_chartsPanel);

                _logger.LogDebug("Enhanced Dashboard Analytics layout initialized with AI capabilities");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing analytics layout");
            }
        }

        private void CreateKPIPanel()
        {
            _kpiPanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(1180, 80),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            var kpiTitle = new Label
            {
                Text = "Fleet Key Performance Indicators",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(10, 5),
                Size = new Size(300, 25),
                ForeColor = Color.FromArgb(46, 125, 185)
            };
            _kpiPanel.Controls.Add(kpiTitle);

            // Add refresh button
            var refreshButton = new Button
            {
                Text = "🔄 Refresh",
                Location = new Point(1090, 5),
                Size = new Size(80, 25),
                BackColor = Color.FromArgb(46, 125, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold)
            };
            refreshButton.Click += async (s, e) => await LoadDashboardDataAsync();
            _kpiPanel.Controls.Add(refreshButton);

            // KPI metrics
            _totalBusesLabel = CreateKPILabel("Total Buses", "Loading...", new Point(20, 35));
            _activeBusesLabel = CreateKPILabel("Active Buses", "Loading...", new Point(200, 35));
            _maintenanceDueLabel = CreateKPILabel("Maintenance Due", "Loading...", new Point(380, 35));
            _inspectionDueLabel = CreateKPILabel("Inspection Due", "Loading...", new Point(560, 35));

            _kpiPanel.Controls.Add(_totalBusesLabel);
            _kpiPanel.Controls.Add(_activeBusesLabel);
            _kpiPanel.Controls.Add(_maintenanceDueLabel);
            _kpiPanel.Controls.Add(_inspectionDueLabel);
        }

        private Label CreateKPILabel(string title, string value, Point location)
        {
            var panel = new Panel
            {
                Location = location,
                Size = new Size(160, 40),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 8, FontStyle.Regular),
                Location = new Point(5, 2),
                Size = new Size(150, 15),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var valueLabel = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(5, 17),
                Size = new Size(150, 20),
                ForeColor = Color.FromArgb(46, 125, 185),
                TextAlign = ContentAlignment.MiddleCenter
            };

            panel.Controls.Add(titleLabel);
            panel.Controls.Add(valueLabel);
            _kpiPanel!.Controls.Add(panel);
            return valueLabel;
        }

        private void CreateAIInsightsPanel()
        {
            _aiInsightsPanel = new Panel
            {
                Location = new Point(610, 100),
                Size = new Size(580, 360),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            var aiTitle = new Label
            {
                Text = "🤖 AI-Powered Fleet Insights",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(10, 5),
                Size = new Size(300, 25),
                ForeColor = Color.FromArgb(46, 125, 185)
            };
            _aiInsightsPanel.Controls.Add(aiTitle);

            _generateInsightsButton = new Button
            {
                Text = "Generate AI Insights",
                Location = new Point(450, 5),
                Size = new Size(120, 25),
                UseVisualStyleBackColor = true,
                BackColor = Color.FromArgb(46, 125, 185),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _generateInsightsButton.Click += GenerateAIInsights_Click;
            _aiInsightsPanel.Controls.Add(_generateInsightsButton);

            _aiInsightsTextBox = new RichTextBox
            {
                Location = new Point(10, 35),
                Size = new Size(560, 315),
                Text = "Click 'Generate AI Insights' to get intelligent recommendations for your fleet management.\n\n" +
                       "AI insights will include:\n" +
                       "• Fleet optimization opportunities\n" +
                       "• Maintenance cost reduction strategies\n" +
                       "• Risk assessment and mitigation\n" +
                       "• Operational efficiency improvements",
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(248, 248, 248),
                Font = new Font("Segoe UI", 10)
            };
            _aiInsightsPanel.Controls.Add(_aiInsightsTextBox);
        }

        private void SetupAnalyticsGrid()
        {
            if (_analyticsGrid == null) return;

            _analyticsGrid.Columns.Add(new GridTextColumn() { MappingName = "BusNumber", HeaderText = "Bus #", Width = 80 });
            _analyticsGrid.Columns.Add(new GridTextColumn() { MappingName = "Model", HeaderText = "Model", Width = 120 });
            _analyticsGrid.Columns.Add(new GridTextColumn() { MappingName = "SeatingCapacity", HeaderText = "Capacity", Width = 80 });
            _analyticsGrid.Columns.Add(new GridTextColumn() { MappingName = "Status", HeaderText = "Status", Width = 100 });
            _analyticsGrid.Columns.Add(new GridTextColumn() { MappingName = "LastServiceDate", HeaderText = "Last Service", Width = 120, Format = "yyyy-MM-dd" });
            _analyticsGrid.Columns.Add(new GridTextColumn() { MappingName = "InspectionStatus", HeaderText = "Inspection", Width = 100 });
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                _logger.LogInformation("Loading dashboard analytics data");

                // Show loading indicators
                if (_totalBusesLabel != null) _totalBusesLabel.Text = "Loading...";
                if (_activeBusesLabel != null) _activeBusesLabel.Text = "Loading...";
                if (_maintenanceDueLabel != null) _maintenanceDueLabel.Text = "Loading...";
                if (_inspectionDueLabel != null) _inspectionDueLabel.Text = "Loading...";

                // Use the Entity Framework methods instead of legacy BusInfo
                var buses = await _busService.GetAllBusEntitiesAsync();
                var busList = buses?.ToList() ?? new List<Bus>();

                // Update KPIs using Bus entity properties
                if (_totalBusesLabel != null) _totalBusesLabel.Text = busList.Count.ToString();
                if (_activeBusesLabel != null) _activeBusesLabel.Text = busList.Count(b => b.Status == "Active").ToString();
                if (_maintenanceDueLabel != null)
                {
                    var maintenanceDue = busList.Count(b => b.LastServiceDate.HasValue && (DateTime.Now - b.LastServiceDate.Value).Days > 90);
                    _maintenanceDueLabel.Text = maintenanceDue.ToString();
                }
                if (_inspectionDueLabel != null)
                {
                    var inspectionDue = busList.Count(b => b.InspectionStatus == "Overdue" || b.InspectionStatus == "Due Soon");
                    _inspectionDueLabel.Text = inspectionDue.ToString();
                }

                // Update grid with data
                if (_analyticsGrid != null)
                {
                    _analyticsGrid.DataSource = busList.Any() ? busList : new List<Bus>();
                    _analyticsGrid.Refresh();
                }

                // Update chart panel with current metrics
                if (_chartsPanel?.Controls.Count > 0 && _chartsPanel.Controls[0] is Label chartLabel)
                {
                    var activeCount = busList.Count(b => b.Status == "Active");
                    chartLabel.Text = $"📊 Fleet Analytics Overview\n" +
                                    $"🚌 {activeCount}/{busList.Count} Buses Active\n" +
                                    $"⏰ Last Updated: {DateTime.Now:HH:mm:ss}";
                }

                _logger.LogInformation($"Dashboard analytics data loaded successfully - {busList.Count} buses found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard analytics data");

                // Show error in UI
                if (_totalBusesLabel != null) _totalBusesLabel.Text = "Error";
                if (_activeBusesLabel != null) _activeBusesLabel.Text = "Error";
                if (_maintenanceDueLabel != null) _maintenanceDueLabel.Text = "Error";
                if (_inspectionDueLabel != null) _inspectionDueLabel.Text = "Error";

                if (_analyticsGrid != null)
                {
                    _analyticsGrid.DataSource = new List<Bus>();
                }

                if (_chartsPanel?.Controls.Count > 0 && _chartsPanel.Controls[0] is Label chartLabel)
                {
                    chartLabel.Text = "❌ Error loading fleet data\nPlease check logs and try again";
                }
            }
        }

        private async void GenerateAIInsights_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_generateInsightsButton != null)
                {
                    _generateInsightsButton.Enabled = false;
                    _generateInsightsButton.Text = "Generating...";
                }

                if (_aiInsightsTextBox != null)
                {
                    _aiInsightsTextBox.Text = "🤖 Generating AI insights based on your fleet data...\n\nPlease wait while our AI analyzes your fleet metrics...";
                }

                // Get current fleet data for context
                var buses = await _busService.GetAllBusEntitiesAsync();
                var busList = buses.ToList();

                // Prepare context for AI using Bus entity properties
                var activeBuses = busList.Count(b => b.Status == "Active");
                var maintenanceDue = busList.Count(b => b.LastServiceDate.HasValue && (DateTime.Now - b.LastServiceDate.Value).Days > 90);
                var averageCapacity = busList.Any() ? busList.Average(b => b.SeatingCapacity) : 0;

                var fleetSummary = $"Fleet Overview: {busList.Count} total buses, " +
                                 $"{activeBuses} active, " +
                                 $"{maintenanceDue} needing maintenance attention. " +
                                 $"Average capacity: {averageCapacity:F0} passengers.";

                var userQuery = "Analyze our bus fleet data and provide strategic insights including: " +
                               "1. Fleet optimization opportunities, " +
                               "2. Maintenance scheduling recommendations, " +
                               "3. Capacity utilization analysis, " +
                               "4. Operational efficiency improvements.";

                // Generate AI insights
                var response = await _aiReportingService.GenerateReportAsync(userQuery, fleetSummary);

                if (_aiInsightsTextBox != null)
                {
                    _aiInsightsTextBox.Text = $"🤖 AI Fleet Analysis Report\n" +
                                            $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}\n\n" +
                                            (response.Content ?? "Unable to generate insights at this time.");
                }

                _logger.LogInformation("AI insights generated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI insights");
                if (_aiInsightsTextBox != null)
                {
                    _aiInsightsTextBox.Text = "❌ Error generating AI insights. Please try again later.\n\nError details logged for support team.";
                }
            }
            finally
            {
                if (_generateInsightsButton != null)
                {
                    _generateInsightsButton.Enabled = true;
                    _generateInsightsButton.Text = "Generate AI Insights";
                }
            }
        }

        private void SetupRealTimeUpdates()
        {
            try
            {
                _realTimeUpdateTimer = new System.Windows.Forms.Timer
                {
                    Interval = 15000 // 15 seconds
                };
                _realTimeUpdateTimer.Tick += async (s, e) => await UpdateAnalyticsData();
                _realTimeUpdateTimer.Start();

                // Initial load
                _ = Task.Run(async () => await UpdateAnalyticsData());

                _logger.LogDebug("Real-time analytics updates configured");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up real-time updates");
            }
        }

        private async Task UpdateAnalyticsData()
        {
            try
            {
                var totalBuses = await _busRepository.GetTotalVehicleCountAsync();
                var activeBuses = await _busRepository.GetActiveVehicleCountAsync();

                this.Invoke(() =>
                {
                    if (_chartsPanel?.Controls.Count > 0 && _chartsPanel.Controls[0] is Label label)
                    {
                        label.Text = $"📊 Fleet Status: {activeBuses}/{totalBuses} Active\n🌍 Google Earth Engine Ready\n⏰ Updated: {DateTime.Now:HH:mm:ss}";
                    }
                });

                _logger.LogDebug($"Analytics updated: {activeBuses}/{totalBuses} buses active");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating analytics data");
            }
        }

        private void CreateRouteOptimizationPanel()
        {
            try
            {
                _routeOptimizationPanel = new Panel()
                {
                    Location = new Point(610, 160),
                    Size = new Size(580, 150),
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle
                };

                var titleLabel = new Label()
                {
                    Text = "🚗 Smart Route Optimization",
                    Location = new Point(10, 10),
                    Size = new Size(300, 25),
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.FromArgb(46, 125, 185)
                };

                _optimizeRoutesButton = new Button()
                {
                    Text = "Optimize Routes",
                    Location = new Point(320, 8),
                    Size = new Size(120, 30),
                    BackColor = Color.FromArgb(46, 125, 185),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                };
                _optimizeRoutesButton.Click += OptimizeRoutesButton_Click;

                _routeInsightsTextBox = new RichTextBox()
                {
                    Location = new Point(10, 45),
                    Size = new Size(560, 95),
                    BackColor = Color.FromArgb(248, 249, 250),
                    Font = new Font("Segoe UI", 9),
                    ReadOnly = true,
                    Text = "Click 'Optimize Routes' to get AI-powered route optimization insights and recommendations.\n\n📊 Route efficiency analysis\n🎯 Optimization recommendations\n⏱️ Time and fuel savings"
                };

                _routeOptimizationPanel.Controls.Add(titleLabel);
                _routeOptimizationPanel.Controls.Add(_optimizeRoutesButton);
                _routeOptimizationPanel.Controls.Add(_routeInsightsTextBox);

                _logger.LogDebug("Route optimization panel created");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating route optimization panel");
            }
        }

        private async void OptimizeRoutesButton_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_optimizeRoutesButton != null && _routeInsightsTextBox != null)
                {
                    _optimizeRoutesButton.Enabled = false;
                    _optimizeRoutesButton.Text = "Optimizing...";
                    _routeInsightsTextBox.Text = "🔄 Analyzing routes and generating optimization insights...\n\nPlease wait while AI processes route data...";

                    var routeAnalysis = await _routeOptimizationService.AnalyzeRouteEfficiencyAsync();

                    // Create optimization parameters with default values
                    var optimizationParams = new OptimizationParameters
                    {
                        MaxRouteDistance = 25.0,
                        MaxStudentsPerBus = 60,
                        MinStudentsPerRoute = 15,
                        MaxRideTime = TimeSpan.FromMinutes(45),
                        ConsiderTrafficPatterns = true,
                        ConsiderSpecialNeeds = true,
                        FuelCostPerGallon = 3.50m,
                        DriverCostPerHour = 25.00m
                    };

                    var optimizedRoutes = await _routeOptimizationService.GenerateOptimizedRoutesAsync(optimizationParams);

                    var insightsText = $"🎯 Route Optimization Results (Generated: {DateTime.Now:HH:mm:ss})\n\n";
                    insightsText += $"📊 Efficiency Analysis:\n{routeAnalysis}\n\n";
                    insightsText += $"🚗 Optimization Recommendations:\n{optimizedRoutes}\n\n";
                    insightsText += "💡 These insights can help improve fleet efficiency and reduce operational costs.";

                    _routeInsightsTextBox.Text = insightsText;
                    _optimizeRoutesButton.Enabled = true;
                    _optimizeRoutesButton.Text = "Optimize Routes";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during route optimization analysis");
                if (_routeInsightsTextBox != null)
                {
                    _routeInsightsTextBox.Text = $"❌ Error during route optimization: {ex.Message}\n\nPlease try again or contact support if the issue persists.";
                }
                if (_optimizeRoutesButton != null)
                {
                    _optimizeRoutesButton.Enabled = true;
                    _optimizeRoutesButton.Text = "Optimize Routes";
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _realTimeUpdateTimer?.Stop();
                _realTimeUpdateTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
