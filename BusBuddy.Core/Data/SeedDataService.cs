using System.IO;
using BusBuddy.Core.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BusBuddy.Core.Data;

/// <summary>
/// Service for seeding real-world transportation data into BusBuddy database
/// Provides infrastructure for importing, validating, and managing seed data
/// </summary>
public class SeedDataService
{
    private readonly IBusBuddyDbContextFactory _contextFactory;
    private static readonly ILogger Logger = Log.ForContext<SeedDataService>();

    public SeedDataService(IBusBuddyDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <summary>
    /// Analyzes current data structure requirements for real-world data integration
    /// </summary>
    public async Task<DataStructureAnalysis> AnalyzeDataStructureAsync()
    {
        using var context = _contextFactory.CreateDbContext();
        Logger.Information("Analyzing data structure for real-world transportation data");

        var analysis = new DataStructureAnalysis
        {
            AnalysisDate = DateTime.UtcNow,
            DriverRequirements = await AnalyzeDriverRequirementsAsync(context),
            VehicleRequirements = await AnalyzeVehicleRequirementsAsync(context),
            ActivityScheduleRequirements = await AnalyzeActivityScheduleRequirementsAsync(context)
        };

        Logger.Information("Data structure analysis completed. Ready for real-world data integration");
        return analysis;
    }

    /// <summary>
    /// Prepares database for real-world data seeding
    /// </summary>
    public async Task<bool> PrepareForRealWorldDataAsync()
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            Logger.Information("Preparing database for real-world transportation data");

            // Ensure database is created and migrated
            await context.Database.MigrateAsync();

            // Validate table structures
            var hasDrivers = await context.Drivers.AnyAsync();
            var hasVehicles = await context.Vehicles.AnyAsync();
            var hasActivities = await context.ActivitySchedule.AnyAsync();

            Logger.Information("Database preparation complete. Ready for real-world data: Drivers={HasDrivers}, Vehicles={HasVehicles}, Activities={HasActivities}",
                hasDrivers, hasVehicles, hasActivities);

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to prepare database for real-world data");
            return false;
        }
    }

    /// <summary>
    /// Seeds real-world transportation data from provided JSON file
    /// </summary>
    public async Task<SeedDataResult> SeedRealWorldDataAsync(string jsonFilePath)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            Logger.Information("Seeding real-world transportation data from {FilePath}", jsonFilePath);

            if (!File.Exists(jsonFilePath))
            {
                Logger.Error("Real-world data file not found: {FilePath}", jsonFilePath);
                return new SeedDataResult { Success = false, ErrorMessage = $"File not found: {jsonFilePath}" };
            }

            var result = new SeedDataResult { StartTime = DateTime.UtcNow };
            var jsonContent = await File.ReadAllTextAsync(jsonFilePath);

            // Parse and validate the JSON structure
            var realWorldData = System.Text.Json.JsonSerializer.Deserialize<RealWorldTransportationData>(jsonContent);
            if (realWorldData == null)
            {
                Logger.Error("Failed to parse real-world data from JSON file");
                return new SeedDataResult { Success = false, ErrorMessage = "Invalid JSON format" };
            }

            // Seed real-world data in correct order (drivers and vehicles first, then activities)
            result.DriversSeeded = await SeedRealWorldDriversAsync(context, realWorldData.Drivers);
            result.VehiclesSeeded = await SeedRealWorldVehiclesAsync(context, realWorldData.Vehicles);

            // Save drivers and vehicles first to ensure IDs are available for activities
            await context.SaveChangesAsync();

            result.ActivitiesSeeded = await SeedRealWorldActivitiesAsync(context, realWorldData.Activities);

            await context.SaveChangesAsync();
            result.EndTime = DateTime.UtcNow;
            result.Success = true;

            Logger.Information("Real-world data seeding completed: {DriversSeeded} drivers, {VehiclesSeeded} vehicles, {ActivitiesSeeded} activities",
                result.DriversSeeded, result.VehiclesSeeded, result.ActivitiesSeeded);

            return result;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to seed real-world transportation data");
            return new SeedDataResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    /// <summary>
    /// Seeds sample transportation data for development and testing
    /// </summary>
    public async Task<SeedDataResult> SeedSampleDataAsync()
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            Logger.Information("Seeding sample transportation data");

            var result = new SeedDataResult { StartTime = DateTime.UtcNow };

            // Seed Drivers (15-20 real-world driver profiles)
            result.DriversSeeded = await SeedSampleDriversAsync(context);

            // Seed Vehicles (10-15 realistic bus fleet)
            result.VehiclesSeeded = await SeedSampleVehiclesAsync(context);

            // Seed Activity Schedules (25-30 realistic transportation activities)
            result.ActivitiesSeeded = await SeedSampleActivitiesAsync(context);

            await context.SaveChangesAsync();
            result.EndTime = DateTime.UtcNow;
            result.Success = true;

            Logger.Information("Sample data seeding completed: {DriversSeeded} drivers, {VehiclesSeeded} vehicles, {ActivitiesSeeded} activities",
                result.DriversSeeded, result.VehiclesSeeded, result.ActivitiesSeeded);

            return result;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to seed sample transportation data");
            return new SeedDataResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    private async Task<DriverRequirements> AnalyzeDriverRequirementsAsync(BusBuddyDbContext context)
    {
        var existingCount = await context.Drivers.CountAsync();
        return new DriverRequirements
        {
            ExistingCount = existingCount,
            RequiredFields = new[] { "DriverName", "DriverPhone", "DriversLicenceType", "TrainingComplete" },
            OptionalFields = new[] { "DriverEmail", "Address", "City", "State", "Zip" },
            DataValidationRules = new[]
            {
                "DriverName: Required, Max 100 characters",
                "DriverPhone: Optional, Max 20 characters",
                "DriverEmail: Optional, Valid email format",
                "DriversLicenceType: Required, Valid license type (CDL-A, CDL-B, etc.)",
                "TrainingComplete: Required boolean"
            }
        };
    }

    private async Task<VehicleRequirements> AnalyzeVehicleRequirementsAsync(BusBuddyDbContext context)
    {
        var existingCount = await context.Vehicles.CountAsync();
        return new VehicleRequirements
        {
            ExistingCount = existingCount,
            RequiredFields = new[] { "BusNumber", "Year", "Make", "Model", "SeatingCapacity", "VinNumber" },
            OptionalFields = new[] { "LicenseNumber", "DateLastInspection", "CurrentOdometer", "PurchaseDate", "PurchasePrice" },
            DataValidationRules = new[]
            {
                "BusNumber: Required, Max 20 characters, Unique",
                "Year: Required, Range 1990-2030",
                "Make: Required, Max 50 characters",
                "Model: Required, Max 50 characters",
                "SeatingCapacity: Required, Range 1-100",
                "VinNumber: Required, Max 50 characters, Unique",
                "LicenseNumber: Optional, Max 20 characters"
            }
        };
    }

    private async Task<ActivityScheduleRequirements> AnalyzeActivityScheduleRequirementsAsync(BusBuddyDbContext context)
    {
        var existingCount = await context.ActivitySchedule.CountAsync();
        return new ActivityScheduleRequirements
        {
            ExistingCount = existingCount,
            RequiredFields = new[] { "ScheduledDate", "TripType", "ScheduledVehicleId", "ScheduledDestination", "ScheduledDriverId", "RequestedBy" },
            OptionalFields = new[] { "ScheduledRiders", "Notes", "Status" },
            DataValidationRules = new[]
            {
                "ScheduledDate: Required, Valid future date",
                "TripType: Required, Max 50 characters (Sports Trip, Field Trip, etc.)",
                "ScheduledDestination: Required, Max 200 characters",
                "ScheduledLeaveTime: Required, Valid time",
                "ScheduledEventTime: Required, Valid time",
                "ScheduledVehicleId: Required, Must reference existing vehicle",
                "ScheduledDriverId: Required, Must reference existing driver"
            }
        };
    }

    private Task<int> SeedSampleDriversAsync(BusBuddyDbContext context)
    {
        // Implementation will be provided when real-world data is available
        // This method will create realistic driver profiles based on provided data
        Logger.Information("Sample driver seeding ready for real-world data integration");
        return Task.FromResult(0); // Will return actual count when implemented
    }

    private Task<int> SeedSampleVehiclesAsync(BusBuddyDbContext context)
    {
        // Implementation will be provided when real-world data is available
        // This method will create realistic vehicle fleet based on provided data
        Logger.Information("Sample vehicle seeding ready for real-world data integration");
        return Task.FromResult(0); // Will return actual count when implemented
    }

    private Task<int> SeedSampleActivitiesAsync(BusBuddyDbContext context)
    {
        // Implementation will be provided when real-world data is available
        // This method will create realistic activity schedules based on provided data
        Logger.Information("Sample activity seeding ready for real-world data integration");
        return Task.FromResult(0); // Will return actual count when implemented
    }

    private Task<int> SeedRealWorldDriversAsync(BusBuddyDbContext context, RealWorldDriver[] drivers)
    {
        if (drivers == null || drivers.Length == 0)
        {
            return Task.FromResult(0);
        }

        var count = 0;
        foreach (var driverData in drivers)
        {
            var driver = new Driver
            {
                DriverName = driverData.DriverName,
                DriverPhone = driverData.DriverPhone,
                DriverEmail = driverData.DriverEmail,
                Address = driverData.Address,
                City = driverData.City,
                State = driverData.State,
                Zip = driverData.Zip,
                DriversLicenceType = driverData.DriversLicenceType,
                TrainingComplete = driverData.TrainingComplete,
                Status = "Active"
            };

            context.Drivers.Add(driver);
            count++;
        }

        Logger.Information("Prepared {Count} real-world drivers for seeding", count);
        return Task.FromResult(count);
    }

    private Task<int> SeedRealWorldVehiclesAsync(BusBuddyDbContext context, RealWorldVehicle[] vehicles)
    {
        if (vehicles == null || vehicles.Length == 0)
        {
            return Task.FromResult(0);
        }

        var count = 0;
        foreach (var vehicleData in vehicles)
        {
            var vehicle = new Bus
            {
                BusNumber = vehicleData.BusNumber,
                Year = vehicleData.Year,
                Make = vehicleData.Make,
                Model = vehicleData.Model,
                SeatingCapacity = vehicleData.SeatingCapacity,
                VINNumber = vehicleData.VinNumber,
                LicenseNumber = vehicleData.LicenseNumber ?? string.Empty,
                DateLastInspection = vehicleData.DateLastInspection,
                CurrentOdometer = vehicleData.CurrentOdometer,
                PurchaseDate = vehicleData.PurchaseDate,
                Status = "Active"
            };

            context.Vehicles.Add(vehicle);
            count++;
        }

        Logger.Information("Prepared {Count} real-world vehicles for seeding", count);
        return Task.FromResult(count);
    }

    private async Task<int> SeedRealWorldActivitiesAsync(BusBuddyDbContext context, RealWorldActivity[] activities)
    {
        if (activities == null || activities.Length == 0)
        {
            return 0;
        }

        var count = 0;
        foreach (var activityData in activities)
        {
            // Find the referenced driver and vehicle by their identifiers
            var driver = await context.Drivers.FirstOrDefaultAsync(d => d.DriverId == activityData.ScheduledDriverId);
            var vehicle = await context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == activityData.ScheduledVehicleId);

            if (driver == null || vehicle == null)
            {
                Logger.Warning("Skipping activity {Destination} - referenced driver or vehicle not found", activityData.ScheduledDestination);
                continue;
            }

            var activity = new ActivitySchedule
            {
                ScheduledDate = activityData.ScheduledDate,
                TripType = activityData.TripType,
                ScheduledVehicleId = vehicle.VehicleId,
                ScheduledDestination = activityData.ScheduledDestination,
                ScheduledLeaveTime = activityData.ScheduledLeaveTime,
                ScheduledEventTime = activityData.ScheduledEventTime,
                ScheduledRiders = activityData.ScheduledRiders,
                ScheduledDriverId = driver.DriverId,
                RequestedBy = activityData.RequestedBy,
                Status = activityData.Status ?? "Scheduled",
                Notes = activityData.Notes
            };

            context.ActivitySchedule.Add(activity);
            count++;
        }

        Logger.Information("Prepared {Count} real-world activities for seeding", count);
        return count;
    }
}

/// <summary>
/// Analysis of data structure requirements for real-world transportation data
/// </summary>
public class DataStructureAnalysis
{
    public DateTime AnalysisDate { get; set; }
    public DriverRequirements DriverRequirements { get; set; } = new();
    public VehicleRequirements VehicleRequirements { get; set; } = new();
    public ActivityScheduleRequirements ActivityScheduleRequirements { get; set; } = new();
}

/// <summary>
/// Requirements analysis for driver data structure
/// </summary>
public class DriverRequirements
{
    public int ExistingCount { get; set; }
    public string[] RequiredFields { get; set; } = Array.Empty<string>();
    public string[] OptionalFields { get; set; } = Array.Empty<string>();
    public string[] DataValidationRules { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Requirements analysis for vehicle data structure
/// </summary>
public class VehicleRequirements
{
    public int ExistingCount { get; set; }
    public string[] RequiredFields { get; set; } = Array.Empty<string>();
    public string[] OptionalFields { get; set; } = Array.Empty<string>();
    public string[] DataValidationRules { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Requirements analysis for activity schedule data structure
/// </summary>
public class ActivityScheduleRequirements
{
    public int ExistingCount { get; set; }
    public string[] RequiredFields { get; set; } = Array.Empty<string>();
    public string[] OptionalFields { get; set; } = Array.Empty<string>();
    public string[] DataValidationRules { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Result of data seeding operation
/// </summary>
public class SeedDataResult
{
    public bool Success { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int DriversSeeded { get; set; }
    public int VehiclesSeeded { get; set; }
    public int ActivitiesSeeded { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan Duration => EndTime - StartTime;
}

/// <summary>
/// Real-world transportation data structure for JSON import
/// </summary>
public class RealWorldTransportationData
{
    public RealWorldDriver[] Drivers { get; set; } = Array.Empty<RealWorldDriver>();
    public RealWorldVehicle[] Vehicles { get; set; } = Array.Empty<RealWorldVehicle>();
    public RealWorldActivity[] Activities { get; set; } = Array.Empty<RealWorldActivity>();
}

/// <summary>
/// Real-world driver data structure
/// </summary>
public class RealWorldDriver
{
    public string DriverName { get; set; } = string.Empty;
    public string? DriverPhone { get; set; }
    public string? DriverEmail { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Zip { get; set; }
    public string DriversLicenceType { get; set; } = string.Empty;
    public bool TrainingComplete { get; set; }
}

/// <summary>
/// Real-world vehicle data structure
/// </summary>
public class RealWorldVehicle
{
    public string BusNumber { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int SeatingCapacity { get; set; }
    public string VinNumber { get; set; } = string.Empty;
    public string? LicenseNumber { get; set; }
    public DateTime? DateLastInspection { get; set; }
    public int? CurrentOdometer { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
}

/// <summary>
/// Real-world activity/trip data structure
/// </summary>
public class RealWorldActivity
{
    public DateTime ScheduledDate { get; set; }
    public string TripType { get; set; } = string.Empty;
    public int ScheduledVehicleId { get; set; }
    public string ScheduledDestination { get; set; } = string.Empty;
    public TimeSpan ScheduledLeaveTime { get; set; }
    public TimeSpan ScheduledEventTime { get; set; }
    public int? ScheduledRiders { get; set; }
    public int ScheduledDriverId { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
    public string? Status { get; set; }
    public string? Notes { get; set; }
}
