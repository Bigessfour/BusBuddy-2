using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Bus_Buddy.Models;
using Bus_Buddy.Services;
using Bus_Buddy.Data.Repositories;
using Bus_Buddy.Utilities;

namespace Bus_Buddy.Forms
{
    /// <summary>
    /// Enhanced Route Map Module with Google Earth Engine Integration
    /// Provides advanced route visualization, optimization, and geospatial analysis
    /// Uses Windows Forms controls with Google Earth Engine private license capabilities
    /// </summary>
    public partial class EnhancedRouteMapModule : MetroForm
    {
        private readonly ILogger<EnhancedRouteMapModule> _logger;
        private readonly RouteRepository _routeRepository;
        private readonly BusRepository _busRepository;
        private readonly IRouteService _routeService;
        private readonly GoogleEarthEngineService _geeService;

        // Form Components
        private TabControlAdv? _mapTabs;
        private SplitContainerAdv? _mainSplitter;
        private Panel? _mapPanel;
        private Panel? _controlPanel;

        // Google Earth Engine Integration
        private Panel? _geeControlPanel;
        private ComboBoxAdv? _satelliteLayerCombo;
        private ButtonAdv? _refreshImageryButton;
        private ButtonAdv? _analyzeTerrainButton;
        private ButtonAdv? _optimizeRoutesButton;

        // Route Management
        private ListView? _routeListView;
        private Panel? _routeDetailsPanel;
        private ButtonAdv? _addRouteButton;
        private ButtonAdv? _editRouteButton;
        private ButtonAdv? _deleteRouteButton;

        // Real-time Features
        private System.Windows.Forms.Timer? _realTimeUpdateTimer;
        private Panel? _realTimePanel;
        private Label? _trafficStatusLabel;
        private Label? _weatherStatusLabel;

        public EnhancedRouteMapModule(ILogger<EnhancedRouteMapModule> logger,
                                     RouteRepository routeRepository,
                                     BusRepository busRepository,
                                     IRouteService routeService,
                                     GoogleEarthEngineService geeService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _routeRepository = routeRepository ?? throw new ArgumentNullException(nameof(routeRepository));
            _busRepository = busRepository ?? throw new ArgumentNullException(nameof(busRepository));
            _routeService = routeService ?? throw new ArgumentNullException(nameof(routeService));
            _geeService = geeService ?? throw new ArgumentNullException(nameof(geeService));

            InitializeComponent();
            InitializeEnhancedRouteMap();
        }

        private void InitializeComponent()
        {
            // Initialize basic form
            this.SuspendLayout();
            this.Name = "EnhancedRouteMapModule";
            this.ResumeLayout(false);
        }

        private void InitializeEnhancedRouteMap()
        {
            try
            {
                _logger.LogInformation("Initializing Enhanced Route Map Module with Google Earth Engine");

                // Apply enhanced visual theming
                ApplyEnhancedTheme();

                // Create main layout with splitter
                CreateMainLayout();

                // Initialize Google Earth Engine integration
                InitializeGoogleEarthEngine();

                // Create route management controls
                CreateRouteManagementControls();

                // Setup Syncfusion Maps with satellite imagery
                InitializeSyncfusionMaps();

                // Setup real-time updates
                SetupRealTimeUpdates();

                // Load initial data
                _ = Task.Run(async () => await LoadInitialMapData());

                _logger.LogInformation("Enhanced Route Map Module initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Enhanced Route Map Module");
                throw;
            }
        }

        private void ApplyEnhancedTheme()
        {
            try
            {
                VisualEnhancementManager.ApplyEnhancedTheme(this);

                this.Text = "Bus Buddy - Enhanced Route Mapping & Optimization";
                this.Size = new Size(1600, 1000);
                this.StartPosition = FormStartPosition.CenterScreen;
                this.WindowState = FormWindowState.Maximized;

                // Enhanced MetroForm styling with mapping theme
                this.MetroColor = Color.FromArgb(34, 139, 34); // Forest Green for mapping
                this.CaptionBarColor = Color.FromArgb(34, 139, 34);
                this.CaptionForeColor = Color.White;

                VisualEnhancementManager.EnableHighQualityFontRendering(this);

                _logger.LogDebug("Enhanced mapping theme applied");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not apply enhanced theme");
            }
        }

        private void CreateMainLayout()
        {
            try
            {
                // Create main splitter for map and controls
                _mainSplitter = new SplitContainerAdv
                {
                    Dock = DockStyle.Fill,
                    SplitterDistance = 300,
                    FixedPanel = Syncfusion.Windows.Forms.Tools.Enums.FixedPanel.Panel1,
                    IsSplitterFixed = false,
                    SplitterWidth = 5,
                    BackColor = Color.FromArgb(240, 240, 240)
                };
                this.Controls.Add(_mainSplitter);

                // Create control panel (left side)
                _controlPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    Padding = new Padding(10)
                };
                _mainSplitter.Panel1.Controls.Add(_controlPanel);

                // Create map panel (right side)
                _mapPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White
                };
                _mainSplitter.Panel2.Controls.Add(_mapPanel);

                _logger.LogDebug("Main layout created for route mapping");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating main layout");
            }
        }

        private void InitializeGoogleEarthEngine()
        {
            try
            {
                // Create Google Earth Engine control panel
                _geeControlPanel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 200,
                    BackColor = Color.FromArgb(250, 250, 250),
                    Padding = new Padding(5)
                };

                // GEE Header
                var geeHeader = new Label
                {
                    Text = "🌍 Google Earth Engine Integration",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.FromArgb(34, 139, 34),
                    Dock = DockStyle.Top,
                    Height = 30,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                _geeControlPanel.Controls.Add(geeHeader);

                // Satellite Layer Selection
                var layerLabel = new Label
                {
                    Text = "Satellite Layer:",
                    Dock = DockStyle.Top,
                    Height = 20,
                    Font = new Font("Segoe UI", 9)
                };
                _geeControlPanel.Controls.Add(layerLabel);

                _satelliteLayerCombo = new ComboBoxAdv
                {
                    Dock = DockStyle.Top,
                    Height = 25
                };
                _satelliteLayerCombo.Items.AddRange(new object[] {
                    "LANDSAT/LC08/C02/T1_L2 (Landsat 8)",
                    "COPERNICUS/S2_SR (Sentinel-2)",
                    "MODIS/006/MOD13Q1 (MODIS NDVI)",
                    "GOOGLE/RESEARCH/open-buildings (Buildings)",
                    "JRC/GSW1_4/GlobalSurfaceWater (Water Bodies)"
                });
                _satelliteLayerCombo.SelectedIndex = 0;
                _geeControlPanel.Controls.Add(_satelliteLayerCombo);

                // GEE Action Buttons
                var buttonPanel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 80,
                    Padding = new Padding(0, 10, 0, 0)
                };

                _refreshImageryButton = new ButtonAdv
                {
                    Text = "🛰️ Refresh Imagery",
                    Size = new Size(120, 30),
                    Location = new Point(0, 5),
                    UseVisualStyle = true
                };
                _refreshImageryButton.Click += async (s, e) => await RefreshSatelliteImagery();
                buttonPanel.Controls.Add(_refreshImageryButton);

                _analyzeTerrainButton = new ButtonAdv
                {
                    Text = "🏔️ Analyze Terrain",
                    Size = new Size(120, 30),
                    Location = new Point(130, 5),
                    UseVisualStyle = true
                };
                _analyzeTerrainButton.Click += async (s, e) => await AnalyzeTerrain();
                buttonPanel.Controls.Add(_analyzeTerrainButton);

                _optimizeRoutesButton = new ButtonAdv
                {
                    Text = "🎯 Optimize Routes",
                    Size = new Size(120, 30),
                    Location = new Point(0, 40),
                    UseVisualStyle = true
                };
                _optimizeRoutesButton.Click += async (s, e) => await OptimizeRoutesWithGEE();
                buttonPanel.Controls.Add(_optimizeRoutesButton);

                _geeControlPanel.Controls.Add(buttonPanel);
                _controlPanel?.Controls.Add(_geeControlPanel);

                _logger.LogDebug("Google Earth Engine controls initialized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Google Earth Engine controls");
            }
        }

        private void CreateRouteManagementControls()
        {
            try
            {
                // Route management header
                var routeHeader = new Label
                {
                    Text = "🚌 Route Management",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.FromArgb(46, 125, 185),
                    Dock = DockStyle.Top,
                    Height = 30,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                _controlPanel?.Controls.Add(routeHeader);

                // Route action buttons
                var routeButtonPanel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 40,
                    Padding = new Padding(0, 5, 0, 5)
                };

                _addRouteButton = new ButtonAdv
                {
                    Text = "➕ Add Route",
                    Size = new Size(80, 30),
                    Location = new Point(0, 5),
                    UseVisualStyle = true
                };
                _addRouteButton.Click += AddRoute_Click!;
                routeButtonPanel.Controls.Add(_addRouteButton);

                _editRouteButton = new ButtonAdv
                {
                    Text = "✏️ Edit",
                    Size = new Size(60, 30),
                    Location = new Point(85, 5),
                    UseVisualStyle = true
                };
                _editRouteButton.Click += EditRoute_Click!;
                routeButtonPanel.Controls.Add(_editRouteButton);

                _deleteRouteButton = new ButtonAdv
                {
                    Text = "🗑️ Delete",
                    Size = new Size(70, 30),
                    Location = new Point(150, 5),
                    UseVisualStyle = true
                };
                _deleteRouteButton.Click += DeleteRoute_Click!;
                routeButtonPanel.Controls.Add(_deleteRouteButton);

                _controlPanel?.Controls.Add(routeButtonPanel);

                // Route list view (using standard ListView)
                _routeListView = new ListView
                {
                    Dock = DockStyle.Fill,
                    View = View.Details,
                    FullRowSelect = true,
                    GridLines = true,
                    MultiSelect = false
                };
                _routeListView.Columns.Add("Route Name", 120);
                _routeListView.Columns.Add("Date", 80);
                _routeListView.Columns.Add("AM Riders", 60);
                _routeListView.Columns.Add("PM Riders", 60);
                _routeListView.Columns.Add("Status", 80);
                _routeListView.SelectedIndexChanged += RouteListView_SelectedIndexChanged!;
                _controlPanel?.Controls.Add(_routeListView);

                _logger.LogDebug("Route management controls created");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating route management controls");
            }
        }

        private void InitializeSyncfusionMaps()
        {
            try
            {
                // Create tab control for different map views
                _mapTabs = new TabControlAdv
                {
                    Dock = DockStyle.Fill,
                    TabStyle = typeof(TabRendererMetro)
                };

                // Main route map tab
                var routeMapTab = new TabPageAdv("Route Map");
                _routeMap = new Maps
                {
                    Dock = DockStyle.Fill,
                    EnableNavigation = true,
                    EnableZooming = true,
                    EnablePanning = true
                };

                // Configure map for satellite view with Google Earth Engine
                ConfigureMapForSatelliteView();

                routeMapTab.Controls.Add(_routeMap);
                _mapTabs.TabPages.Add(routeMapTab);

                // Real-time tracking tab
                var realTimeTab = new TabPageAdv("Real-Time Tracking");
                CreateRealTimeTrackingPanel(realTimeTab);
                _mapTabs.TabPages.Add(realTimeTab);

                // Analytics tab
                var analyticsTab = new TabPageAdv("Route Analytics");
                CreateRouteAnalyticsPanel(analyticsTab);
                _mapTabs.TabPages.Add(analyticsTab);

                _mapPanel?.Controls.Add(_mapTabs);

                _logger.LogDebug("Syncfusion Maps initialized with Google Earth Engine integration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Syncfusion Maps");
            }
        }

        private void ConfigureMapForSatelliteView()
        {
            try
            {
                if (_routeMap != null)
                {
                    // Configure map layers for satellite imagery
                    _routeMap.BaseMapIndex = 0; // Start with standard map
                    _routeMap.EnableZooming = true;
                    _routeMap.EnablePanning = true;
                    _routeMap.ZoomLevel = 10;

                    // Set initial center (you can customize this based on your service area)
                    _routeMap.Latitude = 40.7128; // New York City as example
                    _routeMap.Longitude = -74.0060;

                    // Enable satellite overlay from Google Earth Engine
                    _ = Task.Run(async () => await OverlaySatelliteImagery());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configuring map for satellite view");
            }
        }

        private void CreateRealTimeTrackingPanel(TabPageAdv tab)
        {
            try
            {
                _realTimePanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    Padding = new Padding(10)
                };

                // Real-time status indicators
                var statusPanel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 100,
                    BackColor = Color.FromArgb(245, 245, 245),
                    Padding = new Padding(10)
                };

                _trafficStatusLabel = new Label
                {
                    Text = "🚦 Traffic Status: Loading...",
                    Font = new Font("Segoe UI", 10),
                    Location = new Point(10, 10),
                    Size = new Size(300, 25),
                    ForeColor = Color.FromArgb(95, 99, 104)
                };
                statusPanel.Controls.Add(_trafficStatusLabel);

                _weatherStatusLabel = new Label
                {
                    Text = "🌤️ Weather: Loading...",
                    Font = new Font("Segoe UI", 10),
                    Location = new Point(10, 40),
                    Size = new Size(300, 25),
                    ForeColor = Color.FromArgb(95, 99, 104)
                };
                statusPanel.Controls.Add(_weatherStatusLabel);

                _realTimePanel.Controls.Add(statusPanel);

                // Real-time map (clone of main map for tracking)
                var realTimeMap = new Maps
                {
                    Dock = DockStyle.Fill,
                    EnableNavigation = true,
                    EnableZooming = true,
                    EnablePanning = true
                };
                _realTimePanel.Controls.Add(realTimeMap);

                tab.Controls.Add(_realTimePanel);

                _logger.LogDebug("Real-time tracking panel created");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating real-time tracking panel");
            }
        }

        private void CreateRouteAnalyticsPanel(TabPageAdv tab)
        {
            try
            {
                var analyticsPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    Padding = new Padding(10)
                };

                // Analytics header
                var analyticsHeader = new Label
                {
                    Text = "📊 Route Performance Analytics",
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    ForeColor = Color.FromArgb(46, 125, 185),
                    Dock = DockStyle.Top,
                    Height = 40,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                analyticsPanel.Controls.Add(analyticsHeader);

                // Placeholder for analytics charts (can be enhanced with Syncfusion charts)
                var chartPlaceholder = new Label
                {
                    Text = "Analytics charts will be displayed here:\n\n" +
                           "• Route efficiency metrics\n" +
                           "• Fuel consumption analysis\n" +
                           "• Traffic pattern insights\n" +
                           "• Terrain impact on performance\n" +
                           "• Optimal route suggestions",
                    Font = new Font("Segoe UI", 11),
                    ForeColor = Color.FromArgb(95, 99, 104),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                analyticsPanel.Controls.Add(chartPlaceholder);

                tab.Controls.Add(analyticsPanel);

                _logger.LogDebug("Route analytics panel created");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating route analytics panel");
            }
        }

        private void SetupRealTimeUpdates()
        {
            try
            {
                _realTimeUpdateTimer = new Timer
                {
                    Interval = 30000 // 30 seconds
                };
                _realTimeUpdateTimer.Tick += async (s, e) => await UpdateRealTimeData();
                _realTimeUpdateTimer.Start();

                _logger.LogDebug("Real-time updates configured for route mapping");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not setup real-time updates");
            }
        }

        #region Google Earth Engine Integration Methods

        private async Task RefreshSatelliteImagery()
        {
            try
            {
                _refreshImageryButton.Text = "🔄 Refreshing...";
                _refreshImageryButton.Enabled = false;

                var selectedLayer = _satelliteLayerCombo?.SelectedItem?.ToString() ?? "";
                _logger.LogInformation($"Refreshing satellite imagery with layer: {selectedLayer}");

                // Call Google Earth Engine service to get latest imagery
                var imageryData = await _geeService.GetSatelliteImageryAsync(selectedLayer,
                    _routeMap?.Latitude ?? 40.7128,
                    _routeMap?.Longitude ?? -74.0060);

                if (imageryData != null)
                {
                    await OverlaySatelliteImagery();
                    _logger.LogInformation("Satellite imagery refreshed successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing satellite imagery");
                MessageBox.Show("Error refreshing satellite imagery. Please check your Google Earth Engine connection.",
                    "Imagery Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                _refreshImageryButton.Text = "🛰️ Refresh Imagery";
                _refreshImageryButton.Enabled = true;
            }
        }

        private async Task AnalyzeTerrain()
        {
            try
            {
                _analyzeTerrainButton.Text = "🔄 Analyzing...";
                _analyzeTerrainButton.Enabled = false;

                _logger.LogInformation("Starting terrain analysis with Google Earth Engine");

                // Get terrain data for current map view
                var terrainData = await _geeService.GetTerrainAnalysisAsync(
                    _routeMap?.Latitude ?? 40.7128,
                    _routeMap?.Longitude ?? -74.0060,
                    _routeMap?.ZoomLevel ?? 10);

                if (terrainData != null)
                {
                    // Display terrain analysis results
                    var terrainMessage = $"Terrain Analysis Results:\n\n" +
                        $"• Elevation Range: {terrainData.MinElevation:F1}m - {terrainData.MaxElevation:F1}m\n" +
                        $"• Average Slope: {terrainData.AverageSlope:F1}°\n" +
                        $"• Terrain Type: {terrainData.TerrainType}\n" +
                        $"• Route Difficulty: {terrainData.RouteDifficulty}";

                    MessageBox.Show(terrainMessage, "Terrain Analysis",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    _logger.LogInformation("Terrain analysis completed successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing terrain");
                MessageBox.Show("Error analyzing terrain data. Please try again.",
                    "Analysis Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                _analyzeTerrainButton.Text = "🏔️ Analyze Terrain";
                _analyzeTerrainButton.Enabled = true;
            }
        }

        private async Task OptimizeRoutesWithGEE()
        {
            try
            {
                _optimizeRoutesButton.Text = "🔄 Optimizing...";
                _optimizeRoutesButton.Enabled = false;

                _logger.LogInformation("Starting route optimization with Google Earth Engine");

                // Get all routes for optimization
                var routes = await _routeRepository.GetAllRoutesAsync();

                if (routes?.Any() == true)
                {
                    var optimizationResults = await _geeService.OptimizeRoutesAsync(routes);

                    if (optimizationResults?.Any() == true)
                    {
                        var resultsMessage = $"Route Optimization Results:\n\n" +
                            $"• {optimizationResults.Count} routes analyzed\n" +
                            $"• Potential fuel savings: {optimizationResults.Sum(r => r.FuelSavingsPercent):F1}%\n" +
                            $"• Time efficiency improvement: {optimizationResults.Sum(r => r.TimeEfficiencyGain):F1}%\n" +
                            $"• Terrain-optimized alternatives found: {optimizationResults.Count(r => r.HasAlternative)}";

                        MessageBox.Show(resultsMessage, "Optimization Complete",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Refresh route display with optimized data
                        await LoadRouteData();
                    }
                }
                else
                {
                    MessageBox.Show("No routes found for optimization. Please add routes first.",
                        "No Routes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing routes");
                MessageBox.Show("Error optimizing routes. Please try again.",
                    "Optimization Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                _optimizeRoutesButton.Text = "🎯 Optimize Routes";
                _optimizeRoutesButton.Enabled = true;
            }
        }

        private async Task OverlaySatelliteImagery()
        {
            try
            {
                // This method would integrate Google Earth Engine imagery with Syncfusion Maps
                // Implementation would depend on your specific GEE service setup
                _logger.LogDebug("Overlaying satellite imagery from Google Earth Engine");

                // Example: Apply satellite layer to map
                if (_routeMap != null)
                {
                    // Configure map to display satellite imagery
                    // This would be customized based on your GEE service implementation
                    await Task.Delay(1000); // Simulate imagery loading
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error overlaying satellite imagery");
            }
        }

        #endregion

        #region Route Management Event Handlers

        private async void AddRoute_Click(object sender, EventArgs e)
        {
            try
            {
                // Open route edit form for new route
                var routeEditForm = new RouteEditForm();
                if (routeEditForm.ShowDialog(this) == DialogResult.OK)
                {
                    await LoadRouteData();
                    _logger.LogInformation("New route added successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding new route");
                MessageBox.Show("Error adding new route. Please try again.",
                    "Add Route Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void EditRoute_Click(object sender, EventArgs e)
        {
            try
            {
                if (_routeListView?.SelectedItems.Count == 1)
                {
                    var selectedItem = _routeListView.SelectedItems[0];
                    var routeId = (int)selectedItem.Tag;

                    var routeEditForm = new RouteEditForm(routeId);
                    if (routeEditForm.ShowDialog(this) == DialogResult.OK)
                    {
                        await LoadRouteData();
                        _logger.LogInformation($"Route {routeId} edited successfully");
                    }
                }
                else
                {
                    MessageBox.Show("Please select a route to edit.",
                        "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing route");
                MessageBox.Show("Error editing route. Please try again.",
                    "Edit Route Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void DeleteRoute_Click(object sender, EventArgs e)
        {
            try
            {
                if (_routeListView?.SelectedItems.Count == 1)
                {
                    var selectedItem = _routeListView.SelectedItems[0];
                    var routeName = selectedItem.Text;

                    var result = MessageBox.Show($"Are you sure you want to delete route '{routeName}'?",
                        "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        var routeId = (int)selectedItem.Tag;
                        await _routeRepository.DeleteRouteAsync(routeId);
                        await LoadRouteData();
                        _logger.LogInformation($"Route {routeId} deleted successfully");
                    }
                }
                else
                {
                    MessageBox.Show("Please select a route to delete.",
                        "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting route");
                MessageBox.Show("Error deleting route. Please try again.",
                    "Delete Route Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void RouteListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (_routeListView?.SelectedItems.Count == 1)
                {
                    var selectedItem = _routeListView.SelectedItems[0];
                    var routeId = (int)selectedItem.Tag;

                    // Display route on map
                    await DisplayRouteOnMap(routeId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling route selection");
            }
        }

        #endregion

        #region Data Loading Methods

        private async Task LoadInitialMapData()
        {
            try
            {
                await LoadRouteData();
                await UpdateRealTimeData();
                _logger.LogDebug("Initial map data loaded");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading initial map data");
            }
        }

        private async Task LoadRouteData()
        {
            try
            {
                var routes = await _routeRepository.GetAllRoutesAsync();

                this.Invoke(() =>
                {
                    _routeListView?.Items.Clear();

                    if (routes?.Any() == true)
                    {
                        foreach (var route in routes)
                        {
                            var item = new ListViewItem(route.RouteName);
                            item.SubItems.Add($"{route.Distance:F1} km");
                            item.SubItems.Add(route.StopCount.ToString());
                            item.SubItems.Add(route.IsActive ? "Active" : "Inactive");
                            item.Tag = route.RouteId;
                            _routeListView?.Items.Add(item);
                        }
                    }
                });

                _logger.LogDebug($"Loaded {routes?.Count ?? 0} routes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading route data");
            }
        }

        private async Task DisplayRouteOnMap(int routeId)
        {
            try
            {
                var route = await _routeRepository.GetRouteByIdAsync(routeId);
                if (route != null)
                {
                    // Display route on the Syncfusion map
                    // This would involve plotting route points, stops, etc.
                    _logger.LogDebug($"Displaying route {routeId} on map");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error displaying route {routeId} on map");
            }
        }

        private async Task UpdateRealTimeData()
        {
            try
            {
                // Update traffic status
                var trafficData = await _geeService.GetTrafficDataAsync();
                if (trafficData != null)
                {
                    this.Invoke(() =>
                    {
                        if (_trafficStatusLabel != null)
                        {
                            _trafficStatusLabel.Text = $"🚦 Traffic Status: {trafficData.OverallCondition}";
                            _trafficStatusLabel.ForeColor = trafficData.OverallCondition == "Good"
                                ? Color.Green : Color.Orange;
                        }
                    });
                }

                // Update weather status
                var weatherData = await _geeService.GetWeatherDataAsync();
                if (weatherData != null)
                {
                    this.Invoke(() =>
                    {
                        if (_weatherStatusLabel != null)
                        {
                            _weatherStatusLabel.Text = $"🌤️ Weather: {weatherData.Condition}, {weatherData.Temperature}°C";
                        }
                    });
                }

                _logger.LogDebug("Real-time data updated");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error updating real-time data");
            }
        }

        #endregion

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

    #region Supporting Data Classes

    public class TerrainAnalysisResult
    {
        public double MinElevation { get; set; }
        public double MaxElevation { get; set; }
        public double AverageSlope { get; set; }
        public string TerrainType { get; set; } = string.Empty;
        public string RouteDifficulty { get; set; } = string.Empty;
    }

    public class RouteOptimizationResult
    {
        public int RouteId { get; set; }
        public double FuelSavingsPercent { get; set; }
        public double TimeEfficiencyGain { get; set; }
        public bool HasAlternative { get; set; }
        public string OptimizationNotes { get; set; } = string.Empty;
    }

    public class TrafficData
    {
        public string OverallCondition { get; set; } = string.Empty;
        public Dictionary<string, string> RouteConditions { get; set; } = new();
    }

    public class WeatherData
    {
        public string Condition { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public double Visibility { get; set; }
        public string WindCondition { get; set; } = string.Empty;
    }

    #endregion
}
