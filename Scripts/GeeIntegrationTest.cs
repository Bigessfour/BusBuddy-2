using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Bus_Buddy.Services;

namespace Bus_Buddy.Scripts
{
    /// <summary>
    /// Verification script to test Google Earth Engine integration
    /// Run this to ensure your GEE setup is working correctly
    /// </summary>
    public class GeeIntegrationTest
    {
        private readonly GoogleEarthEngineService _geeService;
        private readonly ILogger<GeeIntegrationTest> _logger;

        public GeeIntegrationTest(GoogleEarthEngineService geeService, ILogger<GeeIntegrationTest> logger)
        {
            _geeService = geeService;
            _logger = logger;
        }

        public async Task<bool> RunAllTestsAsync()
        {
            _logger.LogInformation("🌍 Starting Google Earth Engine Integration Tests");
            _logger.LogInformation("Project: busbuddy-465000");

            var tests = new (string, Func<Task<bool>>)[]
            {
                ("Configuration Check", TestConfiguration),
                ("Satellite Imagery", TestSatelliteImagery),
                ("Terrain Analysis", TestTerrainAnalysis),
                ("Traffic Data", TestTrafficData),
                ("Weather Data", TestWeatherData),
                ("Route Optimization", TestRouteOptimization),
                ("Route Efficiency Analysis", TestRouteEfficiencyAnalysis)
            };

            bool allTestsPassed = true;
            int passedTests = 0;

            foreach ((string testName, Func<Task<bool>> testMethod) in tests)
            {
                try
                {
                    _logger.LogInformation($"\n🔧 Running test: {testName}");
                    bool result = await testMethod();

                    if (result)
                    {
                        _logger.LogInformation($"✅ {testName}: PASSED");
                        passedTests++;
                    }
                    else
                    {
                        _logger.LogError($"❌ {testName}: FAILED");
                        allTestsPassed = false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"💥 {testName}: ERROR - {ex.Message}");
                    allTestsPassed = false;
                }

                // Small delay between tests
                await Task.Delay(500);
            }

            _logger.LogInformation($"\n📊 Test Results Summary:");
            _logger.LogInformation($"   Tests Passed: {passedTests}/{tests.Length}");
            _logger.LogInformation($"   Success Rate: {(passedTests * 100.0 / tests.Length):F1}%");
            _logger.LogInformation($"   Overall Result: {(allTestsPassed ? "✅ ALL TESTS PASSED" : "❌ SOME TESTS FAILED")}");

            if (allTestsPassed)
            {
                _logger.LogInformation("\n🎉 Google Earth Engine integration is working correctly!");
                _logger.LogInformation("You can now use the Route Map features in Bus Buddy.");
            }
            else
            {
                _logger.LogWarning("\n⚠️  Some tests failed. Please check your Google Earth Engine configuration.");
                _logger.LogInformation("Common issues:");
                _logger.LogInformation("1. Service account key file missing or invalid");
                _logger.LogInformation("2. Earth Engine API not enabled");
                _logger.LogInformation("3. Insufficient permissions on service account");
                _logger.LogInformation("4. Network connectivity issues");
            }

            return allTestsPassed;
        }

        private Task<bool> TestConfiguration()
        {
            if (!_geeService.IsConfigured)
            {
                _logger.LogError("Google Earth Engine is not configured. Check appsettings.json");
                return Task.FromResult(false);
            }

            _logger.LogInformation("Configuration appears valid");
            return Task.FromResult(true);
        }

        private async Task<bool> TestSatelliteImagery()
        {
            try
            {
                // Test with Phoenix, Arizona coordinates (typical bus service area)
                var imagery = await _geeService.GetSatelliteImageryAsync("Landsat-8", 33.4484, -112.0740);

                if (imagery == null)
                {
                    _logger.LogError("Failed to retrieve satellite imagery");
                    return false;
                }

                _logger.LogInformation($"Retrieved imagery: {imagery.LayerName} at {imagery.Resolution} resolution");
                _logger.LogInformation($"Capture date: {imagery.CaptureDate:yyyy-MM-dd}, Quality: {imagery.QualityScore:F2}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing satellite imagery");
                return false;
            }
        }

        private async Task<bool> TestTerrainAnalysis()
        {
            try
            {
                // Test terrain analysis for a typical bus route area
                var terrain = await _geeService.GetTerrainAnalysisAsync(33.4484, -112.0740, 12);

                if (terrain == null)
                {
                    _logger.LogError("Failed to retrieve terrain analysis");
                    return false;
                }

                _logger.LogInformation($"Terrain: {terrain.TerrainType}, Difficulty: {terrain.RouteDifficulty}");
                _logger.LogInformation($"Elevation: {terrain.MinElevation:F0}m - {terrain.MaxElevation:F0}m, Avg Slope: {terrain.AverageSlope:F1}°");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing terrain analysis");
                return false;
            }
        }

        private async Task<bool> TestTrafficData()
        {
            try
            {
                var traffic = await _geeService.GetTrafficDataAsync();

                if (traffic == null)
                {
                    _logger.LogError("Failed to retrieve traffic data");
                    return false;
                }

                _logger.LogInformation($"Overall traffic condition: {traffic.OverallCondition}");
                _logger.LogInformation($"Route conditions: {string.Join(", ", traffic.RouteConditions.Values)}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing traffic data");
                return false;
            }
        }

        private async Task<bool> TestWeatherData()
        {
            try
            {
                var weather = await _geeService.GetWeatherDataAsync();

                if (weather == null)
                {
                    _logger.LogError("Failed to retrieve weather data");
                    return false;
                }

                _logger.LogInformation($"Weather: {weather.Condition}, {weather.Temperature:F1}°C");
                _logger.LogInformation($"Visibility: {weather.Visibility:F1}km, Wind: {weather.WindCondition}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing weather data");
                return false;
            }
        }

        private async Task<bool> TestRouteOptimization()
        {
            try
            {
                // Create test routes for optimization
                var testRoutes = new[]
                {
                    new Bus_Buddy.Models.Route { RouteId = 1, RouteName = "Test Route 1" },
                    new Bus_Buddy.Models.Route { RouteId = 2, RouteName = "Test Route 2" }
                };

                var optimizations = await _geeService.OptimizeRoutesAsync(testRoutes);

                if (optimizations == null || optimizations.Count == 0)
                {
                    _logger.LogError("Failed to retrieve route optimizations");
                    return false;
                }

                foreach (var optimization in optimizations)
                {
                    _logger.LogInformation($"Route {optimization.RouteId}: {optimization.FuelSavingsPercent:F1}% fuel savings, {optimization.TimeEfficiencyGain:F1}% time improvement");
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing route optimization");
                return false;
            }
        }

        private async Task<bool> TestRouteEfficiencyAnalysis()
        {
            try
            {
                var analysis = await _geeService.AnalyzeRouteEfficiencyAsync(1);

                if (analysis == null)
                {
                    _logger.LogError("Failed to retrieve route efficiency analysis");
                    return false;
                }

                _logger.LogInformation($"Route efficiency: {analysis.OverallEfficiencyScore:F1}%");
                _logger.LogInformation($"Ratings - Fuel: {analysis.FuelEfficiencyRating}/10, Time: {analysis.TimeEfficiencyRating}/10, Safety: {analysis.SafetyRating}/10");
                _logger.LogInformation($"Improvements suggested: {analysis.RecommendedImprovements.Count}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing route efficiency analysis");
                return false;
            }
        }
    }

    /// <summary>
    /// Console application entry point for running GEE tests standalone
    /// </summary>
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            Console.WriteLine("🌍 Bus Buddy - Google Earth Engine Integration Test");
            Console.WriteLine("===================================================");

            try
            {
                // Setup configuration
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false)
                    .Build();

                // Setup logging
                using var loggerFactory = LoggerFactory.Create(builder =>
                    builder.AddConsole().SetMinimumLevel(LogLevel.Information));

                var geeLogger = loggerFactory.CreateLogger<GoogleEarthEngineService>();
                var testLogger = loggerFactory.CreateLogger<GeeIntegrationTest>();

                // Create services
                var geeService = new GoogleEarthEngineService(geeLogger, configuration);
                var integrationTest = new GeeIntegrationTest(geeService, testLogger);

                // Run tests
                bool success = await integrationTest.RunAllTestsAsync();

                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();

                return success ? 0 : 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Fatal error: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return 1;
            }
        }
    }
}
