using Microsoft.EntityFrameworkCore;
using BusBuddy.Core.Data;
using BusBuddy.Core.Models;

namespace BusBuddy.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Testing database connection and column mappings...");

            try
            {
                var connectionString = "Server=.\\SQLEXPRESS;Database=BusBuddy;Trusted_Connection=true;";
                var options = new DbContextOptionsBuilder<BusBuddyDbContext>()
                    .UseSqlServer(connectionString)
                    .Options;

                using var context = new BusBuddyDbContext(options);

                Console.WriteLine("✓ Database context created successfully");

                // Test basic connection
                var canConnect = await context.Database.CanConnectAsync();
                Console.WriteLine($"✓ Database connection: {(canConnect ? "SUCCESS" : "FAILED")}");

                if (!canConnect)
                {
                    Console.WriteLine("❌ Cannot connect to database");
                    return;
                }

                // Test Driver query
                Console.WriteLine("\nTesting Driver queries...");
                try
                {
                    var driverCount = await context.Drivers.CountAsync();
                    Console.WriteLine($"✓ Driver count query: {driverCount} drivers found");

                    var firstDriver = await context.Drivers.FirstOrDefaultAsync();
                    if (firstDriver != null)
                    {
                        Console.WriteLine($"✓ First driver: {firstDriver.DriverName} ({firstDriver.DriversLicenceType})");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Driver query failed: {ex.Message}");
                }

                // Test Bus query
                Console.WriteLine("\nTesting Bus queries...");
                try
                {
                    var busCount = await context.Vehicles.CountAsync();
                    Console.WriteLine($"✓ Bus count query: {busCount} buses found");

                    var firstBus = await context.Vehicles.FirstOrDefaultAsync();
                    if (firstBus != null)
                    {
                        Console.WriteLine($"✓ First bus: {firstBus.BusNumber} - {firstBus.Make} {firstBus.Model}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Bus query failed: {ex.Message}");
                }

                // Test Route query
                Console.WriteLine("\nTesting Route queries...");
                try
                {
                    var routeCount = await context.Routes.CountAsync();
                    Console.WriteLine($"✓ Route count query: {routeCount} routes found");

                    var firstRoute = await context.Routes.FirstOrDefaultAsync();
                    if (firstRoute != null)
                    {
                        Console.WriteLine($"✓ First route: {firstRoute.RouteName} on {firstRoute.Date:yyyy-MM-dd}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Route query failed: {ex.Message}");
                }

                Console.WriteLine("\n✅ Database connection test completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Test failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
