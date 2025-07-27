using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using BusBuddy.Core.Models;

namespace BusBuddy.Core.Services
{
    /// <summary>
    /// Interface for the enhanced JSON data loading service.
    /// </summary>
    public interface IEnhancedDataLoaderService
    {
        /// <summary>
        /// Loads enhanced data from JSON file asynchronously.
        /// </summary>
        /// <param name="filePath">Path to the enhanced JSON data file.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LoadFromJsonAsync(string filePath);

        /// <summary>
        /// Loads the default enhanced real-world data.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LoadDefaultEnhancedDataAsync();
    }

    /// <summary>
    /// Service for loading enhanced Phase 2 data from JSON files.
    /// Handles the comprehensive real-world transportation data with geographical coordinates,
    /// realistic scheduling patterns, and proper entity relationships.
    /// </summary>
    public class EnhancedDataLoaderService : IEnhancedDataLoaderService
    {
        // LoggerMessage delegates for performance
        private static readonly Action<ILogger, string, Exception?> _driverAdded = LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(1001, nameof(_driverAdded)),
            "Added driver: {DriverName}");

        private static readonly Action<ILogger, string, string, string, Exception?> _vehicleAdded = LoggerMessage.Define<string, string, string>(
            LogLevel.Debug,
            new EventId(1002, nameof(_vehicleAdded)),
            "Added vehicle: {Make} {Model} ({PlateNumber})");

        // LoggerMessage delegates for performance
        private static readonly Action<ILogger, string, Exception?> _info = LoggerMessage.Define<string>(
            LogLevel.Information, new EventId(2001, nameof(_info)), "{Message}");
        private static readonly Action<ILogger, string, Exception?> _success = LoggerMessage.Define<string>(
            LogLevel.Information, new EventId(2002, nameof(_success)), "{Message}");
        private static readonly Action<ILogger, string, Exception?> _warn = LoggerMessage.Define<string>(
            LogLevel.Warning, new EventId(2003, nameof(_warn)), "{Message}");
        private static readonly Action<ILogger, string, Exception?> _error = LoggerMessage.Define<string>(
            LogLevel.Error, new EventId(2004, nameof(_error)), "{Message}");
        private static readonly Action<ILogger, string, Exception?> _debug = LoggerMessage.Define<string>(
            LogLevel.Debug, new EventId(2005, nameof(_debug)), "{Message}");
        // LoggerMessage delegates for exception logging
        private static readonly Action<ILogger, Exception, string, Exception?> _errorEx = LoggerMessage.Define<Exception, string>(
            LogLevel.Error, new EventId(3001, nameof(_errorEx)), "{Exception} ❌ {Message}");
        private static readonly Action<ILogger, Exception, string, Exception?> _warnEx = LoggerMessage.Define<Exception, string>(
            LogLevel.Warning, new EventId(3002, nameof(_warnEx)), "{Exception} ⚠️ {Message}");

        private readonly BusBuddyContext _context;
        private readonly ILogger<EnhancedDataLoaderService> _logger;

        public EnhancedDataLoaderService(BusBuddyContext context, ILogger<EnhancedDataLoaderService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task LoadFromJsonAsync(string filePath)
        {
            try
            {
                _info(_logger, $"📂 Loading enhanced data from JSON file: {filePath}", null);

                if (!File.Exists(filePath))
                {
                    _error(_logger, $"JSON file not found: {filePath}", null);
                    throw new FileNotFoundException($"Enhanced data file not found: {filePath}");
                }

                var jsonContent = await File.ReadAllTextAsync(filePath);
                var jsonData = JObject.Parse(jsonContent);

                // Load metadata
                var metadata = jsonData["metadata"];
                if (metadata != null)
                {
                    _info(_logger, $"📊 Loading data: {metadata["description"]}", null);
                    _info(_logger, $"🏫 District: {metadata["district"]}", null);
                    _info(_logger, $"📍 Location: {metadata["location"]}", null);
                }

                // Load data in order: Drivers → Vehicles → Routes → Activities
                await LoadDriversFromJsonAsync(jsonData["drivers"] ?? new JArray());
                await LoadVehiclesFromJsonAsync(jsonData["vehicles"] ?? new JArray());
                await LoadRoutesFromJsonAsync(jsonData["routes"] ?? new JArray());
                await LoadActivitiesFromJsonAsync(jsonData["activities"] ?? new JArray());

                await _context.SaveChangesAsync();

                _success(_logger, "✅ Enhanced data loading completed successfully!", null);
            }
            catch (Exception ex)
            {
                _errorEx(_logger, ex, $"Error loading enhanced data from JSON file: {filePath}", null);
                throw;
            }
        }

        public async Task LoadDefaultEnhancedDataAsync()
        {
            // Path to the default enhanced data file
            var dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            var defaultFilePath = Path.Combine(dataDirectory, "enhanced-realworld-data.json");

            // If not found in Data directory, try relative path
            if (!File.Exists(defaultFilePath))
            {
                defaultFilePath = Path.Combine("BusBuddy.Core", "Data", "enhanced-realworld-data.json");
            }

            // If still not found, try current directory
            if (!File.Exists(defaultFilePath))
            {
                defaultFilePath = "enhanced-realworld-data.json";
            }

            await LoadFromJsonAsync(defaultFilePath);
        }

        private async Task LoadDriversFromJsonAsync(JToken driversJson)
        {
            if (driversJson == null) { return; }

            _info(_logger, "👥 Loading drivers from JSON...", null);

            foreach (var driverJson in driversJson)
            {
                try
                {
                    // Check if driver already exists
                    var licenseNumber = driverJson["licenseNumber"]?.ToString();
                    if (string.IsNullOrEmpty(licenseNumber)) { continue; }

                    var existingDriver = await _context.Drivers
                        .FirstOrDefaultAsync(d => d.LicenseNumber == licenseNumber);

                    if (existingDriver != null)
                    {
                        _debug(_logger, $"Driver with license {licenseNumber} already exists, skipping", null);
                        continue;
                    }

                    var driver = new Driver
                    {
                        DriverName = $"{driverJson["firstName"]?.ToString()} {driverJson["lastName"]?.ToString()}".Trim(),
                        DriverPhone = driverJson["phoneNumber"]?.ToString(),
                        DriverEmail = driverJson["email"]?.ToString(),
                        Address = driverJson["address"]?.ToString(),
                        DriversLicenceType = licenseNumber ?? "CDL",
                        TrainingComplete = true, // Assume all JSON drivers are trained
                        Status = driverJson["isActive"]?.ToObject<bool>() == true ? "Active" : "Inactive"
                    };

                    await _context.Drivers.AddAsync(driver);
                    _driverAdded(_logger, driver.DriverName, null);
                }
                catch (Exception ex)
                {
                    _warnEx(_logger, ex, $"Error processing driver data: {driverJson}", null);
                }
            }

            await _context.SaveChangesAsync();
            _success(_logger, "✅ Drivers loaded successfully", null);
        }

        private async Task LoadVehiclesFromJsonAsync(JToken vehiclesJson)
        {
            if (vehiclesJson == null) { return; }

            _info(_logger, "🚌 Loading vehicles from JSON...", null);

            foreach (var vehicleJson in vehiclesJson)
            {
                try
                {
                    // Check if vehicle already exists
                    var licensePlate = vehicleJson["licensePlate"]?.ToString();
                    if (string.IsNullOrEmpty(licensePlate)) { continue; }

                    var existingVehicle = await _context.Vehicles
                        .FirstOrDefaultAsync(v => v.LicensePlate == licensePlate);

                    if (existingVehicle != null)
                    {
                        _debug(_logger, $"Vehicle with plate {licensePlate} already exists, skipping", null);
                        continue;
                    }

                    var vehicle = new Vehicle
                    {
                        Make = vehicleJson["make"]?.ToString() ?? "Unknown",
                        Model = vehicleJson["model"]?.ToString() ?? "Unknown",
                        PlateNumber = licensePlate ?? "UNKNOWN",
                        Capacity = vehicleJson["capacity"]?.ToObject<int>() ?? 50,
                        BusNumber = licensePlate ?? "UNKNOWN",
                        IsActive = vehicleJson["status"]?.ToString() == "Active",
                        OperationalStatus = vehicleJson["status"]?.ToString() ?? "Operational"
                    };

                    await _context.Vehicles.AddAsync(vehicle);
                    _vehicleAdded(_logger, vehicle.Make, vehicle.Model, vehicle.PlateNumber, null);
                }
                catch (Exception ex)
                {
                    _warnEx(_logger, ex, $"Error processing vehicle data: {vehicleJson}", null);
                }
            }

            await _context.SaveChangesAsync();
            _success(_logger, "✅ Vehicles loaded successfully", null);
        }

        private async Task LoadRoutesFromJsonAsync(JToken routesJson)
        {
            if (routesJson == null) { return; }

            _info(_logger, "🗺️ Loading routes from JSON...", null);

            foreach (var routeJson in routesJson)
            {
                try
                {
                    // Check if route already exists
                    var routeName = routeJson["name"]?.ToString();
                    if (string.IsNullOrEmpty(routeName)) { continue; }

                    var existingRoute = await _context.Routes
                        .FirstOrDefaultAsync(r => r.RouteName == routeName);

                    if (existingRoute != null)
                    {
                        _debug(_logger, $"Route {routeName} already exists, skipping", null);
                        continue;
                    }

                    var route = new Route
                    {
                        RouteName = routeName ?? "Unknown Route",
                        Description = routeJson["description"]?.ToString(),
                        Date = DateTime.Today, // Use today's date for new routes
                        IsActive = routeJson["isActive"]?.ToObject<bool>() ?? true
                    };

                    await _context.Routes.AddAsync(route);
                    _debug(_logger, $"Added route: {route.RouteName}", null);
                }
                catch (Exception ex)
                {
                    _warnEx(_logger, ex, $"Error processing route data: {routeJson}", null);
                }
            }

            await _context.SaveChangesAsync();
            _success(_logger, "✅ Routes loaded successfully", null);
        }

        private async Task LoadActivitiesFromJsonAsync(JToken activitiesJson)
        {
            if (activitiesJson == null) { return; }

            _info(_logger, "📅 Loading activities from JSON...", null);

            // Get existing entities for lookups
            var drivers = await _context.Drivers.ToListAsync();
            var vehicles = await _context.Vehicles.ToListAsync();
            var routes = await _context.Routes.ToListAsync();

            foreach (var activityJson in activitiesJson)
            {
                try
                {
                    var activityName = activityJson["name"]?.ToString();
                    if (string.IsNullOrEmpty(activityName)) { continue; }

                    // Check if activity already exists
                    var existingActivity = await _context.Activities
                        .FirstOrDefaultAsync(a => a.ActivityType == activityName);

                    if (existingActivity != null)
                    {
                        _debug(_logger, $"Activity {activityName} already exists, skipping", null);
                        continue;
                    }

                    // Find related entities
                    var driverName = activityJson["driverName"]?.ToString();
                    var driver = drivers.FirstOrDefault(d => $"{d.FirstName} {d.LastName}" == driverName);

                    var vehiclePlate = activityJson["vehiclePlate"]?.ToString();
                    var vehicle = vehicles.FirstOrDefault(v => v.LicensePlate == vehiclePlate);

                    var routeName = activityJson["routeName"]?.ToString();
                    var route = routes.FirstOrDefault(r => r.RouteName == routeName);

                    var activity = new Activity
                    {
                        ActivityType = activityName,
                        Description = activityJson["description"]?.ToString(),
                        Date = ParseDateSafely(activityJson["date"]?.ToString() ?? string.Empty) ?? DateTime.Now,
                        StartTime = ParseTimeSafely(activityJson["startTime"]?.ToString() ?? string.Empty) ?? TimeSpan.FromHours(8),
                        EndTime = ParseTimeSafely(activityJson["endTime"]?.ToString() ?? string.Empty) ?? TimeSpan.FromHours(9),
                        DriverId = driver?.DriverId,
                        VehicleId = vehicle?.Id ?? 0, // VehicleId property maps to AssignedVehicleId in Activity
                        RouteId = route?.RouteId,
                        Status = activityJson["status"]?.ToString() ?? "Scheduled",
                        ExpectedPassengers = activityJson["maxPassengers"]?.ToObject<int>(),
                        StudentsCount = activityJson["actualPassengers"]?.ToObject<int>(),
                        Notes = activityJson["notes"]?.ToString(),
                        CreatedDate = DateTime.Now
                    };

                    await _context.Activities.AddAsync(activity);
                    _debug(_logger, $"Added activity: {activity.ActivityType}", null);
                }
                catch (Exception ex)
                {
                    _warnEx(_logger, ex, $"Error processing activity data: {activityJson}", null);
                }
            }

            await _context.SaveChangesAsync();
            _success(_logger, "✅ Activities loaded successfully", null);
        }

        #region Helper Methods

        private DateTime? ParseDateSafely(string dateString)
        {
            if (string.IsNullOrEmpty(dateString)) { return null; }

            if (DateTime.TryParse(dateString, out var result))
            {
                return result;
            }

            _warn(_logger, $"Could not parse date: {dateString}", null);
            return null;
        }

        private TimeSpan? ParseTimeSafely(string timeString)
        {
            if (string.IsNullOrEmpty(timeString)) { return null; }

            if (TimeSpan.TryParse(timeString, out var result))
            {
                return result;
            }

            _warn(_logger, $"Could not parse time: {timeString}", null);
            return null;
        }

        #endregion
    }
}
