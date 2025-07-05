using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Syncfusion.WinForms.DataGrid;
using Bus_Buddy.Data.Repositories;
using Bus_Buddy.Services;

namespace Bus_Buddy.Forms
{
    /// <summary>
    /// Enhanced Dashboard Analytics Panel - Simplified for Google Earth Engine testing
    /// </summary>
    public partial class EnhancedDashboardAnalytics : UserControl
    {
        private readonly ILogger<EnhancedDashboardAnalytics> _logger;
        private readonly BusRepository _busRepository;
        private readonly IBusService _busService;

        private SfDataGrid? _analyticsGrid;
        private Panel? _chartsPanel;
        private System.Windows.Forms.Timer? _realTimeUpdateTimer;

        public EnhancedDashboardAnalytics(ILogger<EnhancedDashboardAnalytics> logger,
                                        BusRepository busRepository,
                                        IBusService busService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _busRepository = busRepository ?? throw new ArgumentNullException(nameof(busRepository));
            _busService = busService ?? throw new ArgumentNullException(nameof(busService));

            InitializeComponent();
            InitializeAnalyticsLayout();
            SetupRealTimeUpdates();
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

                // Simple chart panel
                _chartsPanel = new Panel()
                {
                    Dock = DockStyle.Bottom,
                    Height = 200,
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle
                };

                var chartLabel = new Label()
                {
                    Text = "📊 Analytics Chart\n🌍 Google Earth Engine Ready",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(46, 125, 185)
                };
                _chartsPanel.Controls.Add(chartLabel);

                // Create analytics grid
                _analyticsGrid = new SfDataGrid()
                {
                    Dock = DockStyle.Fill,
                    AllowResizingColumns = true,
                    AutoGenerateColumns = false,
                    HeaderRowHeight = 35,
                    RowHeight = 30
                };

                this.Controls.Add(_analyticsGrid);
                this.Controls.Add(_chartsPanel);

                _logger.LogDebug("Enhanced Dashboard Analytics layout initialized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing analytics layout");
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
