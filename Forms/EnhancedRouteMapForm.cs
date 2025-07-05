using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Syncfusion.Windows.Forms;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.Input;
using Bus_Buddy.Models;
using Bus_Buddy.Data.Repositories;
using Bus_Buddy.Services;
using Bus_Buddy.Utilities;

namespace Bus_Buddy.Forms
{
    /// <summary>
    /// Enhanced Route Map Form with Google Earth Engine Integration
    /// Provides interactive route mapping, satellite imagery, terrain analysis,
    /// and route optimization using Google Earth Engine capabilities
    /// </summary>
    public partial class EnhancedRouteMapForm : MetroForm
    {
        private readonly ILogger<EnhancedRouteMapForm> _logger;
        private readonly RouteRepository _routeRepository;
        private readonly BusRepository _busRepository;
        private GoogleEarthEngineService? _geeService;

        // UI Components
        private SplitContainer? _mainSplitter;
        private Panel? _mapPanel;
        private Panel? _controlPanel;
        private SfDataGrid? _routeGrid;
        private Panel? _routeDetailsPanel;
        private Panel? _terrainAnalysisPanel;
        private Panel? _weatherTrafficPanel;

        // Data
        private List<Route> _routes;
        private Route? _selectedRoute;
        private System.Windows.Forms.Timer? _realTimeUpdateTimer;

        public EnhancedRouteMapForm(ILogger<EnhancedRouteMapForm> logger,
                                   RouteRepository routeRepository,
                                   BusRepository busRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _routeRepository = routeRepository ?? throw new ArgumentNullException(nameof(routeRepository));
            _busRepository = busRepository ?? throw new ArgumentNullException(nameof(busRepository));

            _routes = new List<Route>();

            InitializeComponent();
            InitializeEnhancedRouteMap();
        }

        private void InitializeEnhancedRouteMap()
        {
            try
            {
                _logger.LogInformation("Initializing Enhanced Route Map with Google Earth Engine");

                // Apply enhanced theme
                ApplyEnhancedTheme();

                // Initialize Google Earth Engine service
                InitializeGoogleEarthEngine();

                // Create the layout
                CreateMainLayout();

                // Setup controls
                SetupRouteGrid();
                SetupRouteDetailsPanel();
                SetupTerrainAnalysisPanel();
                SetupWeatherTrafficPanel();
                SetupMapPanel();

                // Load initial data
                _ = Task.Run(LoadRoutesAsync);

                // Setup real-time updates
                SetupRealTimeUpdates();

                _logger.LogInformation("Enhanced Route Map initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Enhanced Route Map");
                throw;
            }
        }

        private void ApplyEnhancedTheme()
        {
            this.Text = "🗺️ Enhanced Route Map - Google Earth Engine Integration";
            this.Size = new Size(1600, 1000);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            // Apply Syncfusion styling
            VisualEnhancementManager.ApplyEnhancedTheme(this);
            this.MetroColor = Color.FromArgb(0, 122, 204);
            this.CaptionBarColor = Color.FromArgb(0, 122, 204);
            this.CaptionForeColor = Color.White;
        }

        private void InitializeGoogleEarthEngine()
        {
            try
            {
                // Initialize Google Earth Engine service with your private license
                var logger = Microsoft.Extensions.Logging.LoggerFactory
                    .Create(builder => builder.AddConsole())
                    .CreateLogger<GoogleEarthEngineService>();

                var configBuilder = new Microsoft.Extensions.Configuration.ConfigurationBuilder();
                var config = configBuilder.Build();

                _geeService = new GoogleEarthEngineService(logger, config);

                _logger.LogInformation("Google Earth Engine service initialized");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not initialize Google Earth Engine service");
            }
        }

        private void CreateMainLayout()
        {
            // Create main splitter: Map (left) + Controls (right)
            _mainSplitter = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 1000,
                FixedPanel = FixedPanel.Panel2,
                IsSplitterFixed = false,
                SplitterWidth = 8
            };
            this.Controls.Add(_mainSplitter);

            // Map panel (left side)
            _mapPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 248, 255)
            };
            _mainSplitter.Panel1.Controls.Add(_mapPanel);

            // Control panel (right side) with vertical splitter
            var controlSplitter = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 300,
                FixedPanel = FixedPanel.Panel1
            };
            _mainSplitter.Panel2.Controls.Add(controlSplitter);

            // Route grid panel (top right)
            _controlPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            controlSplitter.Panel1.Controls.Add(_controlPanel);

            // Details and analysis panels (bottom right)
            var detailsSplitter = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 200
            };
            controlSplitter.Panel2.Controls.Add(detailsSplitter);

            // Route details panel
            _routeDetailsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 249, 250)
            };
            detailsSplitter.Panel1.Controls.Add(_routeDetailsPanel);

            // Analysis panels splitter
            var analysisSplitter = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 150
            };
            detailsSplitter.Panel2.Controls.Add(analysisSplitter);

            _terrainAnalysisPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 248, 250)
            };
            analysisSplitter.Panel1.Controls.Add(_terrainAnalysisPanel);

            _weatherTrafficPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 250, 252)
            };
            analysisSplitter.Panel2.Controls.Add(_weatherTrafficPanel);
        }

        private void SetupRouteGrid()
        {
            try
            {
                // Header
                var gridHeader = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 40,
                    BackColor = Color.FromArgb(0, 122, 204)
                };

                var headerLabel = new Label
                {
                    Text = "🚌 Bus Routes",
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    ForeColor = Color.White,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                gridHeader.Controls.Add(headerLabel);
                _controlPanel?.Controls.Add(gridHeader);

                // Route grid
                _routeGrid = new SfDataGrid
                {
                    Dock = DockStyle.Fill,
                    AllowEditing = false,
                    AllowSorting = true,
                    AllowFiltering = true,
                    ShowGroupDropArea = true,
                    AutoGenerateColumns = false
                };

                // Configure columns
                _routeGrid.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn()
                {
                    MappingName = "RouteId",
                    HeaderText = "ID",
                    Width = 60
                });

                _routeGrid.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn()
                {
                    MappingName = "RouteName",
                    HeaderText = "Route Name",
                    Width = 120
                });

                _routeGrid.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn()
                {
                    MappingName = "IsActive",
                    HeaderText = "Status",
                    Width = 80
                });

                _routeGrid.Columns.Add(new Syncfusion.WinForms.DataGrid.GridTextColumn()
                {
                    MappingName = "TotalRiders",
                    HeaderText = "Riders",
                    Width = 70
                });

                // Selection event
                _routeGrid.SelectionChanged += RouteGrid_SelectionChanged;

                _controlPanel?.Controls.Add(_routeGrid);

                _logger.LogDebug("Route grid setup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up route grid");
            }
        }

        private void SetupRouteDetailsPanel()
        {
            try
            {
                // Header
                var detailsHeader = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 35,
                    BackColor = Color.FromArgb(52, 152, 219)
                };

                var headerLabel = new Label
                {
                    Text = "📋 Route Details",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.White,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                detailsHeader.Controls.Add(headerLabel);
                _routeDetailsPanel?.Controls.Add(detailsHeader);

                // Details content
                var detailsContent = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    Padding = new Padding(10)
                };

                var detailsLabel = new Label
                {
                    Text = "Select a route to view details and Google Earth Engine analysis",
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.FromArgb(108, 117, 125),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                detailsContent.Controls.Add(detailsLabel);

                _routeDetailsPanel?.Controls.Add(detailsContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up route details panel");
            }
        }

        private void SetupTerrainAnalysisPanel()
        {
            try
            {
                // Header
                var terrainHeader = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 35,
                    BackColor = Color.FromArgb(155, 89, 182)
                };

                var headerLabel = new Label
                {
                    Text = "🏔️ Terrain Analysis (GEE)",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.White,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                terrainHeader.Controls.Add(headerLabel);
                _terrainAnalysisPanel?.Controls.Add(terrainHeader);

                // Terrain content
                var terrainContent = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    Padding = new Padding(8)
                };

                var terrainLabel = new Label
                {
                    Text = "Google Earth Engine terrain analysis will appear here",
                    Font = new Font("Segoe UI", 8),
                    ForeColor = Color.FromArgb(108, 117, 125),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                terrainContent.Controls.Add(terrainLabel);

                _terrainAnalysisPanel?.Controls.Add(terrainContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up terrain analysis panel");
            }
        }

        private void SetupWeatherTrafficPanel()
        {
            try
            {
                // Header
                var weatherHeader = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 35,
                    BackColor = Color.FromArgb(26, 188, 156)
                };

                var headerLabel = new Label
                {
                    Text = "🌤️ Weather & Traffic",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.White,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                weatherHeader.Controls.Add(headerLabel);
                _weatherTrafficPanel?.Controls.Add(weatherHeader);

                // Weather content
                var weatherContent = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    Padding = new Padding(8)
                };

                var weatherLabel = new Label
                {
                    Text = "Real-time weather and traffic data",
                    Font = new Font("Segoe UI", 8),
                    ForeColor = Color.FromArgb(108, 117, 125),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                weatherContent.Controls.Add(weatherLabel);

                _weatherTrafficPanel?.Controls.Add(weatherContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up weather/traffic panel");
            }
        }

        private void SetupMapPanel()
        {
            try
            {
                // Map header with controls
                var mapHeader = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 60,
                    BackColor = Color.FromArgb(44, 62, 80)
                };

                var headerLabel = new Label
                {
                    Text = "🌍 Interactive Route Map - Google Earth Engine",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(15, 8),
                    AutoSize = true
                };
                mapHeader.Controls.Add(headerLabel);

                // Map controls
                var controlsPanel = new Panel
                {
                    Size = new Size(400, 25),
                    Location = new Point(15, 30),
                    BackColor = Color.Transparent
                };

                var satelliteBtn = new Button
                {
                    Text = "📡 Satellite",
                    Size = new Size(90, 25),
                    Location = new Point(0, 0),
                    BackColor = Color.FromArgb(52, 73, 94),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                satelliteBtn.Click += async (s, e) => await LoadSatelliteImagery();

                var optimizeBtn = new Button
                {
                    Text = "⚡ Optimize",
                    Size = new Size(90, 25),
                    Location = new Point(100, 0),
                    BackColor = Color.FromArgb(230, 126, 34),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                optimizeBtn.Click += async (s, e) => await OptimizeRoutes();

                var refreshBtn = new Button
                {
                    Text = "🔄 Refresh",
                    Size = new Size(90, 25),
                    Location = new Point(200, 0),
                    BackColor = Color.FromArgb(46, 204, 113),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                refreshBtn.Click += async (s, e) => await RefreshMapData();

                controlsPanel.Controls.AddRange(new Control[] { satelliteBtn, optimizeBtn, refreshBtn });
                mapHeader.Controls.Add(controlsPanel);

                _mapPanel?.Controls.Add(mapHeader);

                // Map content area
                var mapContent = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(240, 248, 255)
                };

                // Placeholder for map visualization
                var mapPlaceholder = new Label
                {
                    Text = "🗺️ Interactive Route Map\n\n" +
                           "Google Earth Engine Integration:\n" +
                           "• Satellite imagery overlay\n" +
                           "• Terrain analysis visualization\n" +
                           "• Route optimization display\n" +
                           "• Real-time traffic overlay\n" +
                           "• Weather pattern visualization\n\n" +
                           "Click 'Satellite' to load imagery\n" +
                           "Click 'Optimize' to analyze routes\n" +
                           "Select routes from the list →",
                    Font = new Font("Segoe UI", 11),
                    ForeColor = Color.FromArgb(52, 73, 94),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };

                mapContent.Controls.Add(mapPlaceholder);
                _mapPanel?.Controls.Add(mapContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up map panel");
            }
        }

        private void SetupRealTimeUpdates()
        {
            try
            {
                _realTimeUpdateTimer = new System.Windows.Forms.Timer { Interval = 60000 }; // 1 minute
                _realTimeUpdateTimer.Tick += async (s, e) => await UpdateRealTimeData();
                _realTimeUpdateTimer.Start();

                _logger.LogDebug("Real-time updates configured for route map");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not setup real-time updates");
            }
        }

        #region Event Handlers

        private async void RouteGrid_SelectionChanged(object? sender, EventArgs e)
        {
            try
            {
                if (_routeGrid?.SelectedItem is Route selectedRoute)
                {
                    _selectedRoute = selectedRoute;
                    LoadRouteDetails(selectedRoute);
                    await LoadTerrainAnalysis(selectedRoute);
                    await LoadWeatherTrafficData();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling route selection");
            }
        }

        #endregion

        #region Data Loading Methods

        private async Task LoadRoutesAsync()
        {
            try
            {
                this.Invoke(() =>
                {
                    if (_routeGrid != null)
                        _routeGrid.DataSource = null;
                });

                _routes = (await _routeRepository.GetAllRoutesAsync()).ToList();

                // Enhance route data with calculated fields
                var enhancedRoutes = _routes.Select(r => new
                {
                    r.RouteId,
                    r.RouteName,
                    IsActive = r.IsActive ? "Active" : "Inactive",
                    TotalRiders = (r.AMRiders ?? 0) + (r.PMRiders ?? 0),
                    r.AMBeginTime,
                    r.PMBeginTime,
                    r.DriverName,
                    r.BusNumber
                }).ToList();

                this.Invoke(() =>
                {
                    if (_routeGrid != null)
                        _routeGrid.DataSource = enhancedRoutes;
                });

                _logger.LogInformation($"Loaded {_routes.Count} routes for mapping");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading routes");
            }
        }

        private void LoadRouteDetails(Route route)
        {
            try
            {
                if (_routeDetailsPanel == null) return;

                var detailsContent = _routeDetailsPanel.Controls.OfType<Panel>().FirstOrDefault(p => p.Dock == DockStyle.Fill);
                if (detailsContent != null)
                {
                    detailsContent.Controls.Clear();

                    var detailsText = $"Route: {route.RouteName}\n" +
                                    $"Status: {(route.IsActive ? "✅ Active" : "❌ Inactive")}\n" +
                                    $"Driver: {route.DriverName ?? "Not assigned"}\n" +
                                    $"Bus: {route.BusNumber ?? "Not assigned"}\n" +
                                    $"AM Riders: {route.AMRiders ?? 0}\n" +
                                    $"PM Riders: {route.PMRiders ?? 0}\n" +
                                    $"AM Time: {route.AMBeginTime?.ToString(@"hh\:mm") ?? "Not set"}\n" +
                                    $"PM Time: {route.PMBeginTime?.ToString(@"hh\:mm") ?? "Not set"}\n" +
                                    $"Daily Mileage: {((route.AMEndMiles ?? 0) - (route.AMBeginMiles ?? 0)) + ((route.PMEndMiles ?? 0) - (route.PMBeginMiles ?? 0)):F1} miles";

                    var detailsLabel = new Label
                    {
                        Text = detailsText,
                        Font = new Font("Segoe UI", 9),
                        ForeColor = Color.FromArgb(52, 73, 94),
                        Dock = DockStyle.Fill,
                        AutoSize = false
                    };

                    detailsContent.Controls.Add(detailsLabel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading route details");
            }
        }

        private async Task LoadTerrainAnalysis(Route route)
        {
            try
            {
                if (_geeService == null || _terrainAnalysisPanel == null) return;

                var terrainContent = _terrainAnalysisPanel.Controls.OfType<Panel>().FirstOrDefault(p => p.Dock == DockStyle.Fill);
                if (terrainContent != null)
                {
                    terrainContent.Controls.Clear();

                    // Show loading indicator
                    var loadingLabel = new Label
                    {
                        Text = "🔄 Analyzing terrain with Google Earth Engine...",
                        Font = new Font("Segoe UI", 9),
                        ForeColor = Color.FromArgb(52, 152, 219),
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter
                    };
                    terrainContent.Controls.Add(loadingLabel);

                    // Simulate coordinates for the route (in real implementation, get from route data)
                    var latitude = 40.7128 + (route.RouteId * 0.01); // Sample coordinates
                    var longitude = -74.0060 + (route.RouteId * 0.01);

                    var terrainAnalysis = await _geeService.GetTerrainAnalysisAsync(latitude, longitude, 12);

                    if (terrainAnalysis != null)
                    {
                        var analysisText = $"Terrain Type: {terrainAnalysis.TerrainType}\n" +
                                         $"Min Elevation: {terrainAnalysis.MinElevation:F0}m\n" +
                                         $"Max Elevation: {terrainAnalysis.MaxElevation:F0}m\n" +
                                         $"Avg Slope: {terrainAnalysis.AverageSlope:F1}°\n" +
                                         $"Difficulty: {terrainAnalysis.RouteDifficulty}";

                        loadingLabel.Text = analysisText;
                        loadingLabel.ForeColor = Color.FromArgb(46, 125, 50);
                    }
                    else
                    {
                        loadingLabel.Text = "❌ Terrain analysis unavailable";
                        loadingLabel.ForeColor = Color.FromArgb(211, 47, 47);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading terrain analysis");
            }
        }

        private async Task LoadWeatherTrafficData()
        {
            try
            {
                if (_geeService == null || _weatherTrafficPanel == null) return;

                var weatherContent = _weatherTrafficPanel.Controls.OfType<Panel>().FirstOrDefault(p => p.Dock == DockStyle.Fill);
                if (weatherContent != null)
                {
                    weatherContent.Controls.Clear();

                    // Get weather and traffic data
                    var weatherTask = _geeService.GetWeatherDataAsync();
                    var trafficTask = _geeService.GetTrafficDataAsync();

                    await Task.WhenAll(weatherTask, trafficTask);

                    var weather = await weatherTask;
                    var traffic = await trafficTask;

                    var dataText = $"Weather: {weather?.Condition ?? "Unknown"}\n" +
                                  $"Temperature: {weather?.Temperature:F1}°C\n" +
                                  $"Visibility: {weather?.Visibility:F1}km\n" +
                                  $"Wind: {weather?.WindCondition ?? "Unknown"}\n\n" +
                                  $"Traffic: {traffic?.OverallCondition ?? "Unknown"}";

                    var dataLabel = new Label
                    {
                        Text = dataText,
                        Font = new Font("Segoe UI", 9),
                        ForeColor = Color.FromArgb(52, 73, 94),
                        Dock = DockStyle.Fill,
                        AutoSize = false
                    };

                    weatherContent.Controls.Add(dataLabel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading weather/traffic data");
            }
        }

        #endregion

        #region Google Earth Engine Integration Methods

        private async Task LoadSatelliteImagery()
        {
            try
            {
                if (_geeService == null)
                {
                    MessageBox.Show("Google Earth Engine service not available", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _logger.LogInformation("Loading satellite imagery for all routes");

                // Show loading message
                MessageBox.Show("Loading satellite imagery from Google Earth Engine...\n\nThis may take a few moments.",
                    "Loading Satellite Data", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Process each route
                foreach (var route in _routes.Take(3)) // Limit to first 3 routes for demo
                {
                    var latitude = 40.7128 + (route.RouteId * 0.01);
                    var longitude = -74.0060 + (route.RouteId * 0.01);

                    var imagery = await _geeService.GetSatelliteImageryAsync("COPERNICUS/S2_SR_HARMONIZED", latitude, longitude);

                    if (imagery != null)
                    {
                        _logger.LogInformation($"Loaded satellite imagery for route {route.RouteName}: Quality {imagery.QualityScore:P}");
                    }
                }

                MessageBox.Show("Satellite imagery loaded successfully!", "Google Earth Engine", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading satellite imagery");
                MessageBox.Show($"Error loading satellite imagery: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task OptimizeRoutes()
        {
            try
            {
                if (_geeService == null)
                {
                    MessageBox.Show("Google Earth Engine service not available", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _logger.LogInformation("Optimizing routes with Google Earth Engine");

                var activeRoutes = _routes.Where(r => r.IsActive).ToList();

                if (!activeRoutes.Any())
                {
                    MessageBox.Show("No active routes to optimize", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Show progress
                MessageBox.Show($"Optimizing {activeRoutes.Count} active routes using Google Earth Engine...\n\nThis will analyze terrain, traffic, and weather patterns.",
                    "Route Optimization", MessageBoxButtons.OK, MessageBoxIcon.Information);

                var optimizationResults = await _geeService.OptimizeRoutesAsync(activeRoutes);

                // Display results
                var resultsText = "Route Optimization Results:\n\n";
                foreach (var result in optimizationResults.Take(5)) // Show top 5
                {
                    var route = _routes.FirstOrDefault(r => r.RouteId == result.RouteId);
                    resultsText += $"{route?.RouteName ?? $"Route {result.RouteId}"}:\n";
                    resultsText += $"  • Fuel Savings: {result.FuelSavingsPercent:F1}%\n";
                    resultsText += $"  • Time Efficiency: +{result.TimeEfficiencyGain:F1}%\n";
                    resultsText += $"  • Alternative Available: {(result.HasAlternative ? "Yes" : "No")}\n";
                    resultsText += $"  • Notes: {result.OptimizationNotes}\n\n";
                }

                MessageBox.Show(resultsText, "Google Earth Engine - Route Optimization", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing routes");
                MessageBox.Show($"Error optimizing routes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task RefreshMapData()
        {
            try
            {
                _logger.LogInformation("Refreshing map data");

                // Reload routes
                await LoadRoutesAsync();

                // Refresh selected route data
                if (_selectedRoute != null)
                {
                    LoadRouteDetails(_selectedRoute);
                    await LoadTerrainAnalysis(_selectedRoute);
                }

                // Update weather and traffic
                await LoadWeatherTrafficData();

                MessageBox.Show("Map data refreshed successfully!", "Refresh Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing map data");
                MessageBox.Show($"Error refreshing data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task UpdateRealTimeData()
        {
            try
            {
                // Update weather and traffic data every minute
                await LoadWeatherTrafficData();
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
                _realTimeUpdateTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
