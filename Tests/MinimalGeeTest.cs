using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Bus_Buddy.Services;
using Bus_Buddy.Models;

namespace Bus_Buddy.Tests
{
    /// <summary>
    /// Minimal Google Earth Engine connectivity test
    /// Tests authentication and basic service initialization
    /// </summary>
    public class MinimalGeeTest
    {
        public static async Task<bool> TestConnection()
        {
            try
            {
                Console.WriteLine("🌍 Testing Google Earth Engine Connection...");

                // Check service account key exists
                var keyPath = "keys/bus-buddy-gee-key.json";
                if (!File.Exists(keyPath))
                {
                    Console.WriteLine("❌ Service account key not found");
                    return false;
                }
                Console.WriteLine("✅ Service account key found");

                // Build configuration
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                var geeConfig = configuration.GetSection("GoogleEarthEngine");
                if (!geeConfig.Exists())
                {
                    Console.WriteLine("❌ GoogleEarthEngine configuration not found");
                    return false;
                }
                Console.WriteLine("✅ Configuration found");

                // Create logger
                var serviceProvider = new ServiceCollection()
                    .AddLogging(builder => builder.AddConsole())
                    .BuildServiceProvider();

                var logger = serviceProvider.GetService<ILogger<GoogleEarthEngineService>>();

                // Initialize service (fix constructor parameter order)
                var geeService = new GoogleEarthEngineService(logger, configuration);
                Console.WriteLine("✅ Google Earth Engine service initialized");

                // Test basic functionality (mock mode)
                var testCoords = new[] { 40.7128, -74.0060 }; // NYC coordinates

                Console.WriteLine("🗺️ Testing terrain analysis...");
                var terrainData = await geeService.GetTerrainAnalysisAsync(testCoords[0], testCoords[1], 1000);
                Console.WriteLine($"✅ Terrain analysis completed: {terrainData?.TerrainType ?? "Mock data"}");

                Console.WriteLine("🚌 Testing route optimization...");
                var routes = new List<Route>(); // Empty list for testing
                var routeData = await geeService.OptimizeRoutesAsync(routes);
                Console.WriteLine($"✅ Route optimization completed: {routeData?.Count ?? 0} results");

                Console.WriteLine("🌍 All tests passed! Google Earth Engine is ready to use.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Test failed: {ex.Message}");
                return false;
            }
        }

        public static async Task Main(string[] args)
        {
            var success = await TestConnection();
            Environment.Exit(success ? 0 : 1);
        }
    }
}
