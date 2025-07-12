using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using BusBuddy.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// Diagnostic tool to isolate and identify startup exceptions
/// </summary>
public class DiagnosticRunner
{
    public static async Task RunDiagnosticsAsync()
    {
        Console.WriteLine("=== BusBuddy Startup Diagnostics ===");

        // 1. Test Configuration Loading
        await TestConfigurationAsync();

        // 2. Test Syncfusion License
        TestSyncfusionLicense();

        // 3. Test Database Connection
        await TestDatabaseConnectionAsync();

        // 4. Test EF Core Context
        await TestEFCoreContextAsync();

        Console.WriteLine("=== Diagnostics Complete ===");
    }

    private static async Task TestConfigurationAsync()
    {
        Console.WriteLine("\n1. Testing Configuration Loading...");
        try
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");
            var syncfusionKey = config["SyncfusionLicenseKey"];

            Console.WriteLine($"   ✓ Configuration loaded successfully");
            Console.WriteLine($"   ✓ Connection string: {(string.IsNullOrEmpty(connectionString) ? "MISSING" : "Found")}");
            Console.WriteLine($"   ✓ Syncfusion key: {(string.IsNullOrEmpty(syncfusionKey) ? "MISSING" : "Found")}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ✗ Configuration error: {ex.Message}");
        }
    }

    private static void TestSyncfusionLicense()
    {
        Console.WriteLine("\n2. Testing Syncfusion License...");
        try
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var licenseKey = config["SyncfusionLicenseKey"];
            if (!string.IsNullOrEmpty(licenseKey))
            {
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(licenseKey);
                Console.WriteLine("   ✓ Syncfusion license registered successfully");
            }
            else
            {
                Console.WriteLine("   ✗ Syncfusion license key not found");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ✗ Syncfusion license error: {ex.Message}");
        }
    }

    private static async Task TestDatabaseConnectionAsync()
    {
        Console.WriteLine("\n3. Testing Database Connection...");
        try
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");

            var options = new DbContextOptionsBuilder<BusBuddyDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            using var context = new BusBuddyDbContext(options);

            var canConnect = await context.Database.CanConnectAsync();
            Console.WriteLine($"   {(canConnect ? "✓" : "✗")} Database connection: {(canConnect ? "Success" : "Failed")}");

            if (canConnect)
            {
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                Console.WriteLine($"   ✓ Pending migrations: {pendingMigrations.Count()}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ✗ Database connection error: {ex.Message}");
        }
    }

    private static async Task TestEFCoreContextAsync()
    {
        Console.WriteLine("\n4. Testing EF Core Context Operations...");
        try
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");

            var options = new DbContextOptionsBuilder<BusBuddyDbContext>()
                .UseSqlServer(connectionString)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .LogTo(Console.WriteLine, LogLevel.Warning)
                .Options;

            using var context = new BusBuddyDbContext(options);

            // Test basic queries
            var vehicleCount = await context.Vehicles.CountAsync();
            var driverCount = await context.Drivers.CountAsync();
            var routeCount = await context.Routes.CountAsync();

            Console.WriteLine($"   ✓ Vehicles: {vehicleCount}");
            Console.WriteLine($"   ✓ Drivers: {driverCount}");
            Console.WriteLine($"   ✓ Routes: {routeCount}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ✗ EF Core context error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"      Inner: {ex.InnerException.Message}");
            }
        }
    }
}
