using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Bus_Buddy.Services;
using Bus_Buddy.Data.Repositories;
using Bus_Buddy.Utilities;

namespace Bus_Buddy.Forms
{
    /// <summary>
    /// Enhanced Bus Management Form that enhances the existing sophisticated BusManagementForm
    /// without replacing hard-earned functionality. This approach demonstrates how to add
    /// standardized enhancements while preserving existing investments.
    /// </summary>
    public partial class EnhancedBusManagementForm : Form
    {
        private readonly ILogger<EnhancedBusManagementForm> _logger;
        private readonly IBusService _busService;
        private readonly BusRepository _busRepository;
        private BusManagementForm? _baseBusManagementForm;

        public EnhancedBusManagementForm(ILogger<EnhancedBusManagementForm> logger,
                                       IBusService busService,
                                       BusRepository busRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _busService = busService ?? throw new ArgumentNullException(nameof(busService));
            _busRepository = busRepository ?? throw new ArgumentNullException(nameof(busRepository));

            InitializeComponent();
            CreateEnhancedBusManagementForm();
        }

        /// <summary>
        /// Creates the enhanced form by composing the existing BusManagementForm with additional features
        /// This preserves all existing functionality while adding enhancements
        /// </summary>
        private void CreateEnhancedBusManagementForm()
        {
            try
            {
                // Create the existing sophisticated BusManagementForm
                var busFormLogger = ServiceContainer.GetService<ILogger<BusManagementForm>>();
                _baseBusManagementForm = new BusManagementForm(busFormLogger, _busService);

                // Embed the existing form as the main content
                _baseBusManagementForm.TopLevel = false;
                _baseBusManagementForm.FormBorderStyle = FormBorderStyle.None;
                _baseBusManagementForm.Dock = DockStyle.Fill;
                this.Controls.Add(_baseBusManagementForm);
                _baseBusManagementForm.Show();

                // Add enhanced features as overlays/extensions
                AddEnhancedToolbar();
                AddEnhancedStatusBar();

                // Set up enhanced form properties
                this.Text = "Enhanced Bus Management - Bus Buddy";
                this.Size = new System.Drawing.Size(1200, 800);
                this.StartPosition = FormStartPosition.CenterScreen;

                _logger.LogInformation("Enhanced Bus Management Form created with preserved base functionality");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating enhanced Bus Management Form");
                throw;
            }
        }

        /// <summary>
        /// Adds enhanced toolbar with repository-based features
        /// </summary>
        private void AddEnhancedToolbar()
        {
            try
            {
                var toolbar = new ToolStrip
                {
                    Dock = DockStyle.Top,
                    GripStyle = ToolStripGripStyle.Hidden,
                    BackColor = System.Drawing.Color.FromArgb(240, 240, 240)
                };

                // Add enhanced buttons that leverage existing BusRepository capabilities
                toolbar.Items.Add(CreateToolStripButton("Maintenance Due", "Show buses due for maintenance",
                    async (s, e) => await ShowMaintenanceDueBuses()));
                toolbar.Items.Add(CreateToolStripButton("Inspection Due", "Show buses due for inspection",
                    async (s, e) => await ShowInspectionDueBuses()));
                toolbar.Items.Add(CreateToolStripButton("Available Buses", "Show available buses",
                    async (s, e) => await ShowAvailableBuses()));
                toolbar.Items.Add(new ToolStripSeparator());
                toolbar.Items.Add(CreateToolStripButton("Fleet Statistics", "Show fleet statistics",
                    async (s, e) => await ShowFleetStatistics()));
                toolbar.Items.Add(CreateToolStripButton("Bus Reports", "Open bus reports",
                    (s, e) => OpenBusReportsForm()));

                this.Controls.Add(toolbar);
                _logger.LogDebug("Enhanced toolbar added");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not add enhanced toolbar");
            }
        }

        /// <summary>
        /// Adds enhanced status bar with real-time fleet information
        /// </summary>
        private void AddEnhancedStatusBar()
        {
            try
            {
                var statusBar = new StatusStrip
                {
                    Dock = DockStyle.Bottom,
                    BackColor = System.Drawing.Color.FromArgb(240, 240, 240)
                };

                statusBar.Items.Add(new ToolStripStatusLabel("Enhanced Status: Ready") { Name = "enhancedStatusLabel" });
                statusBar.Items.Add(new ToolStripStatusLabel() { Spring = true }); // Spacer
                statusBar.Items.Add(new ToolStripStatusLabel("Fleet Info Loading...") { Name = "fleetInfoLabel" });

                this.Controls.Add(statusBar);

                // Load initial fleet statistics
                _ = Task.Run(async () => await UpdateFleetStatusAsync());

                _logger.LogDebug("Enhanced status bar added");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not add enhanced status bar");
            }
        }

        private ToolStripButton CreateToolStripButton(string text, string tooltip, EventHandler onClick)
        {
            return new ToolStripButton
            {
                Text = text,
                ToolTipText = tooltip,
                AutoSize = true,
                Margin = new Padding(2)
            };
        }

        private ToolStripButton CreateToolStripButton(string text, string tooltip, Func<object, EventArgs, Task> onClickAsync)
        {
            var button = new ToolStripButton
            {
                Text = text,
                ToolTipText = tooltip,
                AutoSize = true,
                Margin = new Padding(2)
            };

            button.Click += async (s, e) =>
            {
                if (s != null && e != null)
                    await onClickAsync(s, e);
            };
            return button;
        }

        /// <summary>
        /// Shows buses due for maintenance using existing repository capabilities
        /// </summary>
        private async Task ShowMaintenanceDueBuses()
        {
            try
            {
                SetEnhancedStatus("Loading buses due for maintenance...", StatusType.Info);

                var maintenanceDueBuses = await _busRepository.GetVehiclesDueForMaintenanceAsync();

                ShowBusFilterResults(maintenanceDueBuses, "Buses Due for Maintenance");

                SetEnhancedStatus($"Showing {maintenanceDueBuses.Count()} buses due for maintenance",
                    maintenanceDueBuses.Any() ? StatusType.Warning : StatusType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing maintenance due buses");
                SetEnhancedStatus("Error loading maintenance due buses", StatusType.Error);
            }
        }

        /// <summary>
        /// Shows buses due for inspection using existing repository capabilities
        /// </summary>
        private async Task ShowInspectionDueBuses()
        {
            try
            {
                SetEnhancedStatus("Loading buses due for inspection...", StatusType.Info);

                var inspectionDueBuses = await _busRepository.GetVehiclesDueForInspectionAsync();

                ShowBusFilterResults(inspectionDueBuses, "Buses Due for Inspection");

                SetEnhancedStatus($"Showing {inspectionDueBuses.Count()} buses due for inspection",
                    inspectionDueBuses.Any() ? StatusType.Warning : StatusType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing inspection due buses");
                SetEnhancedStatus("Error loading inspection due buses", StatusType.Error);
            }
        }

        /// <summary>
        /// Shows available buses using existing repository capabilities
        /// </summary>
        private async Task ShowAvailableBuses()
        {
            try
            {
                SetEnhancedStatus("Loading available buses...", StatusType.Info);

                var availableBuses = await _busRepository.GetAvailableVehiclesAsync(DateTime.Today);

                ShowBusFilterResults(availableBuses, "Available Buses");

                SetEnhancedStatus($"Showing {availableBuses.Count()} available buses", StatusType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing available buses");
                SetEnhancedStatus("Error loading available buses", StatusType.Error);
            }
        }

        /// <summary>
        /// Shows fleet statistics using repository methods
        /// </summary>
        private async Task ShowFleetStatistics()
        {
            try
            {
                SetEnhancedStatus("Loading fleet statistics...", StatusType.Info);

                var totalVehicles = await _busRepository.GetTotalVehicleCountAsync();
                var activeVehicles = await _busRepository.GetActiveVehicleCountAsync();
                var averageAge = await _busRepository.GetAverageVehicleAgeAsync();
                var statusCounts = await _busRepository.GetVehicleCountByStatusAsync();

                var statisticsMessage = $"Fleet Statistics:\n" +
                    $"Total Vehicles: {totalVehicles}\n" +
                    $"Active Vehicles: {activeVehicles}\n" +
                    $"Average Age: {averageAge} years\n" +
                    $"Status Breakdown: {string.Join(", ", statusCounts.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}";

                MessageBox.Show(statisticsMessage, "Fleet Statistics", MessageBoxButtons.OK, MessageBoxIcon.Information);

                SetEnhancedStatus("Fleet statistics loaded", StatusType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading fleet statistics");
                SetEnhancedStatus("Error loading fleet statistics", StatusType.Error);
            }
        }

        /// <summary>
        /// Shows filtered bus results in a new window
        /// </summary>
        private void ShowBusFilterResults(System.Collections.Generic.IEnumerable<Bus_Buddy.Models.Bus> buses, string title)
        {
            try
            {
                var resultsForm = new BusBuddy.Forms.BusReportsForm(_busRepository);
                resultsForm.Text = title;
                resultsForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error showing bus filter results");
                MessageBox.Show($"Error displaying {title}: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Opens the Bus Reports Form
        /// </summary>
        private void OpenBusReportsForm()
        {
            try
            {
                var busReportsForm = new BusBuddy.Forms.BusReportsForm(_busRepository);
                busReportsForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening Bus Reports Form");
                SetEnhancedStatus("Error opening reports", StatusType.Error);
            }
        }

        /// <summary>
        /// Updates fleet status information in the status bar
        /// </summary>
        private async Task UpdateFleetStatusAsync()
        {
            try
            {
                var totalVehicles = await _busRepository.GetTotalVehicleCountAsync();
                var activeVehicles = await _busRepository.GetActiveVehicleCountAsync();
                var maintenanceNeeded = (await _busRepository.GetVehiclesDueForMaintenanceAsync()).Count();
                var inspectionNeeded = (await _busRepository.GetVehiclesDueForInspectionAsync()).Count();

                var fleetInfo = $"Total: {totalVehicles} | Active: {activeVehicles}";
                if (maintenanceNeeded > 0 || inspectionNeeded > 0)
                {
                    fleetInfo += $" | Attention Needed: {maintenanceNeeded + inspectionNeeded}";
                }

                this.Invoke(() =>
                {
                    var fleetInfoLabel = this.Controls.OfType<StatusStrip>().FirstOrDefault()?.Items["fleetInfoLabel"] as ToolStripStatusLabel;
                    if (fleetInfoLabel != null)
                    {
                        fleetInfoLabel.Text = fleetInfo;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not update fleet status information");
            }
        }

        /// <summary>
        /// Sets enhanced status message
        /// </summary>
        private void SetEnhancedStatus(string message, StatusType statusType)
        {
            try
            {
                this.Invoke(() =>
                {
                    var statusLabel = this.Controls.OfType<StatusStrip>().FirstOrDefault()?.Items["enhancedStatusLabel"] as ToolStripStatusLabel;
                    if (statusLabel != null)
                    {
                        statusLabel.Text = $"Enhanced Status: {message}";
                        statusLabel.ForeColor = statusType switch
                        {
                            StatusType.Success => System.Drawing.Color.FromArgb(46, 204, 113),
                            StatusType.Error => System.Drawing.Color.FromArgb(231, 76, 60),
                            StatusType.Warning => System.Drawing.Color.FromArgb(255, 152, 0),
                            StatusType.Info => System.Drawing.Color.FromArgb(52, 152, 219),
                            _ => System.Drawing.Color.FromArgb(52, 152, 219)
                        };
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not set enhanced status");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _baseBusManagementForm?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
