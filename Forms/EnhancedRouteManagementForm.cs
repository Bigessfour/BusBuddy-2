using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Syncfusion.Windows.Forms;
using Syncfusion.WinForms.Controls;
using Bus_Buddy.Services;
using Bus_Buddy.Models;

// Type aliases to resolve namespace conflicts
using ServiceWeatherData = Bus_Buddy.Services.WeatherData;
using ServiceTerrainAnalysisResult = Bus_Buddy.Services.TerrainAnalysisResult;
using ServiceTrafficData = Bus_Buddy.Services.TrafficData;

namespace Bus_Buddy.Forms
{
    /// <summary>
    /// Enhanced Route Management Form with Google Earth Engine Integration
    /// Demonstrates real-world satellite data integration for route optimization
    /// </summary>
    public partial class EnhancedRouteManagementForm : MetroForm
    {
        private readonly ILogger<EnhancedRouteManagementForm> _logger;
        private readonly IBusService _busService;
        private readonly GoogleEarthEngineService _geeService;

        // Enhanced UI controls for GEE integration
        private SfButton? optimizeRouteButton;
        private SfButton? weatherAnalysisButton;
        private SfButton? terrainAnalysisButton;
        private Panel? satelliteImagePanel;
        private Label? optimizationResultsLabel;

        public EnhancedRouteManagementForm(
            ILogger<EnhancedRouteManagementForm> logger,
            IBusService busService,
            GoogleEarthEngineService geeService)
        {
            _logger = logger;
            _busService = busService;
            _geeService = geeService;

            InitializeGeeControls();
        }

        private void InitializeGeeControls()
        {
            // Optimize Route Button with Google Earth Engine
            optimizeRouteButton = new SfButton
            {
                Text = "🌍 AI Optimize Route",
                Size = new Size(150, 35),
                Location = new Point(20, 50),
                BackColor = Color.FromArgb(34, 139, 34) // Direct property instead of Style.BackColor
            };
            optimizeRouteButton.Click += OptimizeRouteWithGEE_Click;
            Controls.Add(optimizeRouteButton);

            // Weather Analysis Button
            weatherAnalysisButton = new SfButton
            {
                Text = "🌦️ Weather Impact",
                Size = new Size(150, 35),
                Location = new Point(180, 50),
                BackColor = Color.FromArgb(30, 144, 255) // Direct property instead of Style.BackColor
            };
            weatherAnalysisButton.Click += WeatherAnalysis_Click;
            Controls.Add(weatherAnalysisButton);

            // Terrain Analysis Button
            terrainAnalysisButton = new SfButton
            {
                Text = "🏔️ Terrain Analysis",
                Size = new Size(150, 35),
                Location = new Point(340, 50),
                BackColor = Color.FromArgb(139, 69, 19) // Direct property instead of Style.BackColor
            };
            terrainAnalysisButton.Click += TerrainAnalysis_Click;
            Controls.Add(terrainAnalysisButton);

            // Satellite Image Display Panel
            satelliteImagePanel = new Panel
            {
                Size = new Size(300, 200),
                Location = new Point(500, 50),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightGray
            };
            Controls.Add(satelliteImagePanel);

            // Optimization Results Display
            optimizationResultsLabel = new Label
            {
                Size = new Size(400, 100),
                Location = new Point(20, 300),
                BackColor = Color.FromArgb(240, 248, 255),
                BorderStyle = BorderStyle.FixedSingle,
                Text = "Select a route and click AI Optimize to see Google Earth Engine analysis",
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9)
            };
            Controls.Add(optimizationResultsLabel);
        }

        private async void OptimizeRouteWithGEE_Click(object sender, EventArgs e)
        {
            if (!_geeService.IsConfigured)
            {
                MessageBoxAdv.Show("Google Earth Engine not configured. Please check appsettings.json",
                    "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                optimizeRouteButton.Text = "🔄 Analyzing...";
                optimizeRouteButton.Enabled = false;

                _logger.LogInformation("Starting Google Earth Engine route optimization");

                // Simulate getting selected route (replace with actual grid selection)
                var selectedRoute = GetSelectedRoute();
                if (selectedRoute == null)
                {
                    MessageBoxAdv.Show("Please select a route to optimize",
                        "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Perform comprehensive route analysis using Google Earth Engine
                // Since routes are daily fixed paths, use default school coordinates for terrain analysis
                // For activity-specific analysis with destinations, use Activity model instead
                var schoolLat = 40.7128; // Replace with actual school coordinates
                var schoolLon = -74.0060;

                var efficiencyTask = _geeService.AnalyzeRouteEfficiencyAsync(selectedRoute.RouteId);
                var terrainTask = _geeService.GetTerrainAnalysisAsync(schoolLat, schoolLon, 1000);
                var weatherTask = _geeService.GetWeatherDataAsync();
                var trafficTask = _geeService.GetTrafficDataAsync();

                await Task.WhenAll(efficiencyTask, terrainTask, weatherTask, trafficTask);

                var efficiency = await efficiencyTask;
                var terrain = await terrainTask;
                var weather = await weatherTask;
                var traffic = await trafficTask;

                // Display comprehensive optimization results
                var optimizationSummary = $@"🌍 GOOGLE EARTH ENGINE ROUTE ANALYSIS

📊 EFFICIENCY METRICS:
• Overall Efficiency Score: {efficiency?.OverallEfficiencyScore:F1}%
• Fuel Efficiency Rating: {efficiency?.FuelEfficiencyRating}/10
• Time Efficiency Rating: {efficiency?.TimeEfficiencyRating}/10
• Safety Rating: {efficiency?.SafetyRating}/10

🏔️ TERRAIN ANALYSIS:
• Elevation Range: {terrain?.MinElevation:F0}m - {terrain?.MaxElevation:F0}m
• Average Slope: {terrain?.AverageSlope:F1}°
• Terrain Type: {terrain?.TerrainType}
• Route Difficulty: {terrain?.RouteDifficulty}

🌦️ WEATHER CONDITIONS:
• Current Condition: {weather?.Condition}
• Temperature: {weather?.Temperature:F1}°C
• Visibility: {weather?.Visibility:F1} km
• Wind: {weather?.WindCondition}

🚦 TRAFFIC STATUS:
• Overall Condition: {traffic?.OverallCondition}

💡 RECOMMENDATIONS:
{string.Join(Environment.NewLine, efficiency?.RecommendedImprovements?.Take(3) ?? new[] { "Optimize during off-peak hours" })}";

                optimizationResultsLabel.Text = optimizationSummary;
                optimizationResultsLabel.BackColor = Color.FromArgb(240, 255, 240); // Light green for success

                // Load satellite imagery for the route
                await LoadSatelliteImagery(selectedRoute);

                _logger.LogInformation("Google Earth Engine route optimization completed successfully");

                // Show detailed results dialog
                var detailsForm = new RouteOptimizationDetailsForm(efficiency, terrain, weather, traffic);
                detailsForm.ShowDialog();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google Earth Engine route optimization");
                optimizationResultsLabel.Text = $"❌ Optimization failed: {ex.Message}";
                optimizationResultsLabel.BackColor = Color.FromArgb(255, 240, 240); // Light red for error

                MessageBoxAdv.Show($"Optimization error: {ex.Message}",
                    "Google Earth Engine Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                optimizeRouteButton.Text = "🌍 AI Optimize Route";
                optimizeRouteButton.Enabled = true;
            }
        }

        private async void WeatherAnalysis_Click(object sender, EventArgs e)
        {
            try
            {
                weatherAnalysisButton.Text = "🔄 Loading...";
                weatherAnalysisButton.Enabled = false;

                var weather = await _geeService.GetWeatherDataAsync();
                var weatherAlert = $@"🌦️ CURRENT WEATHER ANALYSIS

Condition: {weather?.Condition}
Temperature: {weather?.Temperature:F1}°C
Visibility: {weather?.Visibility:F1} km
Wind: {weather?.WindCondition}

🚨 TRANSPORTATION IMPACT:
{GetWeatherImpactMessage(weather)}";

                MessageBoxAdv.Show(weatherAlert, "Weather Analysis",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting weather analysis");
                MessageBoxAdv.Show($"Weather analysis error: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                weatherAnalysisButton.Text = "🌦️ Weather Impact";
                weatherAnalysisButton.Enabled = true;
            }
        }

        private async void TerrainAnalysis_Click(object sender, EventArgs e)
        {
            var selectedRoute = GetSelectedRoute();
            if (selectedRoute == null)
            {
                MessageBoxAdv.Show("Please select a route for terrain analysis",
                    "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                terrainAnalysisButton.Text = "🔄 Analyzing...";
                terrainAnalysisButton.Enabled = false;

                // Use school coordinates for route terrain analysis since routes are fixed daily paths
                var schoolLat = 40.7128; // Replace with actual school coordinates
                var schoolLon = -74.0060;

                var terrain = await _geeService.GetTerrainAnalysisAsync(schoolLat, schoolLon, 1000);

                var terrainReport = $@"🏔️ TERRAIN ANALYSIS REPORT

Route: {selectedRoute.RouteName}
Analysis Area: School vicinity (daily route)
Elevation Range: {terrain?.MinElevation:F0}m - {terrain?.MaxElevation:F0}m
Average Slope: {terrain?.AverageSlope:F1}°
Terrain Type: {terrain?.TerrainType}
Difficulty Rating: {terrain?.RouteDifficulty}

💡 RECOMMENDATIONS:
{GetTerrainRecommendations(terrain)}";

                MessageBoxAdv.Show(terrainReport, "Terrain Analysis",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting terrain analysis");
                MessageBoxAdv.Show($"Terrain analysis error: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                terrainAnalysisButton.Text = "🏔️ Terrain Analysis";
                terrainAnalysisButton.Enabled = true;
            }
        }

        private async Task LoadSatelliteImagery(Route route)
        {
            try
            {
                // Use school coordinates for route imagery since routes are fixed daily paths
                var schoolLat = 40.7128; // Replace with actual school coordinates
                var schoolLon = -74.0060;

                var imagery = await _geeService.GetSatelliteImageryAsync("roads", schoolLat, schoolLon);

                // Create a placeholder satellite image visualization
                var graphics = satelliteImagePanel.CreateGraphics();
                graphics.Clear(Color.LightBlue);

                // Draw route representation
                var pen = new Pen(Color.Red, 3);
                graphics.DrawLine(pen, 10, 10, 290, 190);

                // Add imagery info
                var font = new Font("Arial", 8);
                var brush = new SolidBrush(Color.Black);
                graphics.DrawString($"Satellite View\n{route.RouteName}\n(School Area)", font, brush, 10, 10);
                graphics.DrawString($"Resolution: {imagery?.Resolution}\nCapture: {imagery?.CaptureDate:yyyy-MM-dd}",
                    font, brush, 10, 150);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading satellite imagery");
            }
        }

        private Route GetSelectedRoute()
        {
            // Mock route for demonstration - replace with actual grid selection
            // Routes are daily fixed paths, not geographic routes
            return new Route
            {
                RouteId = 1,
                RouteName = "Elementary Route 1",
                Date = DateTime.Today,
                Description = "Daily elementary school route",
                IsActive = true,
                AMVehicleId = 1,
                PMVehicleId = 1
            };
        }

        private string GetWeatherImpactMessage(ServiceWeatherData? weather)
        {
            if (weather == null) return "Weather data unavailable";

            return weather.Condition?.ToLower() switch
            {
                "clear" => "✅ Excellent conditions for transportation",
                "partly cloudy" => "✅ Good conditions, minimal impact expected",
                "cloudy" => "⚠️ Reduced visibility, exercise caution",
                "light rain" => "⚠️ Wet roads, allow extra travel time",
                "heavy rain" => "🚨 Hazardous conditions, consider route delays",
                "snow" => "🚨 Winter conditions, implement safety protocols",
                _ => "ℹ️ Monitor conditions throughout the day"
            };
        }

        private string GetTerrainRecommendations(ServiceTerrainAnalysisResult? terrain)
        {
            if (terrain == null) return "Terrain analysis unavailable";

            var recommendations = new List<string>();

            if (terrain.AverageSlope > 5.0)
                recommendations.Add("• Consider engine braking on steep descents");

            if (terrain.RouteDifficulty == "Challenging" || terrain.RouteDifficulty == "Difficult")
                recommendations.Add("• Schedule additional maintenance for this route");

            if (terrain.TerrainType == "Mountainous")
                recommendations.Add("• Monitor weather conditions closely");

            if (terrain.TerrainType == "Urban")
                recommendations.Add("• Optimize for traffic pattern efficiency");

            return recommendations.Any() ? string.Join("\n", recommendations) : "• Route appears optimal for standard operations";
        }
    }

    /// <summary>
    /// Detailed optimization results dialog
    /// </summary>
    public class RouteOptimizationDetailsForm : MetroForm
    {
        public RouteOptimizationDetailsForm(
            RouteEfficiencyAnalysis? efficiency,
            ServiceTerrainAnalysisResult? terrain,
            ServiceWeatherData? weather,
            ServiceTrafficData? traffic)
        {
            Text = "Google Earth Engine Route Optimization Details";
            Size = new Size(600, 500);
            StartPosition = FormStartPosition.CenterParent;

            var textBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9),
                Text = GenerateDetailedReport(efficiency, terrain, weather, traffic)
            };

            Controls.Add(textBox);
        }

        private string GenerateDetailedReport(
            RouteEfficiencyAnalysis? efficiency,
            ServiceTerrainAnalysisResult? terrain,
            ServiceWeatherData? weather,
            ServiceTrafficData? traffic)
        {
            return $@"🌍 GOOGLE EARTH ENGINE COMPREHENSIVE ROUTE ANALYSIS
================================================================

📊 EFFICIENCY ANALYSIS:
Overall Score: {efficiency?.OverallEfficiencyScore:F2}%
Fuel Efficiency: {efficiency?.FuelEfficiencyRating}/10
Time Efficiency: {efficiency?.TimeEfficiencyRating}/10
Safety Rating: {efficiency?.SafetyRating}/10
Environmental Impact: {efficiency?.EnvironmentalImpactScore:F1}%

🏔️ TERRAIN ANALYSIS:
Minimum Elevation: {terrain?.MinElevation:F1}m
Maximum Elevation: {terrain?.MaxElevation:F1}m
Elevation Change: {(terrain?.MaxElevation - terrain?.MinElevation):F1}m
Average Slope: {terrain?.AverageSlope:F2}°
Terrain Classification: {terrain?.TerrainType}
Difficulty Assessment: {terrain?.RouteDifficulty}

🌦️ WEATHER CONDITIONS:
Current Condition: {weather?.Condition}
Temperature: {weather?.Temperature:F1}°C
Visibility: {weather?.Visibility:F1} kilometers
Wind Conditions: {weather?.WindCondition}

🚦 TRAFFIC ANALYSIS:
Overall Traffic: {traffic?.OverallCondition}
Route-Specific Conditions:
{string.Join("\n", traffic?.RouteConditions?.Select(kvp => $"  {kvp.Key}: {kvp.Value}") ?? new[] { "  No specific data available" })}

💡 OPTIMIZATION RECOMMENDATIONS:
{string.Join("\n", efficiency?.RecommendedImprovements?.Select(r => $"• {r}") ?? new[] { "• No specific recommendations at this time" })}

📈 EXPECTED BENEFITS:
• Fuel savings potential through route optimization
• Reduced wear and tear based on terrain analysis
• Improved safety through weather awareness
• Enhanced efficiency through traffic pattern recognition

Analysis completed at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Powered by Google Earth Engine";
        }
    }
}
