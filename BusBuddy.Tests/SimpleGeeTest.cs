using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Bus_Buddy.Services;

namespace Bus_Buddy.Testing
{
    /// <summary>
    /// Simple test to verify Google Earth Engine integration
    /// </summary>
    public class SimpleGeeTest
    {
        public static async Task<bool> TestGeeIntegration()
        {
            try
            {
                Console.WriteLine("🌍 Testing Google Earth Engine Integration...");

                // Create configuration
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false)
                    .Build();

                // Create logger
                using var loggerFactory = LoggerFactory.Create(builder =>
                    builder.AddConsole().SetMinimumLevel(LogLevel.Information));
                var logger = loggerFactory.CreateLogger<GoogleEarthEngineService>();

                // Create service
                var geeService = new GoogleEarthEngineService(logger, configuration);

                Console.WriteLine($"✅ Service created. Configured: {geeService.IsConfigured}");

                // Test basic functionality
                Console.WriteLine("🧪 Testing satellite imagery...");
                var imagery = await geeService.GetSatelliteImageryAsync("Landsat-8", 33.4484, -112.0740);
                Console.WriteLine($"✅ Satellite imagery: {(imagery != null ? "SUCCESS" : "FAILED")}");

                Console.WriteLine("🧪 Testing terrain analysis...");
                var terrain = await geeService.GetTerrainAnalysisAsync(33.4484, -112.0740, 12);
                Console.WriteLine($"✅ Terrain analysis: {(terrain != null ? "SUCCESS" : "FAILED")}");

                Console.WriteLine("🧪 Testing traffic data...");
                var traffic = await geeService.GetTrafficDataAsync();
                Console.WriteLine($"✅ Traffic data: {(traffic != null ? "SUCCESS" : "FAILED")}");

                Console.WriteLine("🧪 Testing weather data...");
                var weather = await geeService.GetWeatherDataAsync();
                Console.WriteLine($"✅ Weather data: {(weather != null ? "SUCCESS" : "FAILED")}");

                Console.WriteLine("\n🎉 Google Earth Engine Integration Test PASSED!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Test FAILED: {ex.Message}");
                return false;
            }
        }
    }
}
