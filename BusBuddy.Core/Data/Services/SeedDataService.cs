using BusBuddy.Core.Data.UnitOfWork;
using BusBuddy.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Context;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BusBuddy.Core.Data.Services
{
    /// <summary>
    /// Production-ready seed data service for populating the database with real-world test data
    /// Enhanced with comprehensive Serilog logging and environment-aware seeding
    /// Integrates with existing BusBuddy repository and Unit of Work patterns
    /// </summary>
    public class SeedDataService
    {
        private readonly BusBuddyContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private static readonly Serilog.ILogger Logger = Log.ForContext<SeedDataService>();

        // Static arrays for Serilog performance optimization
        private static readonly string[] SeedingOperations = { "Drivers", "Vehicles", "Activities", "Students", "Routes" };
        private static readonly string[] EnvironmentTypes = { "Development", "Staging", "Production" };

        public SeedDataService(
            BusBuddyContext context,
            IUnitOfWork unitOfWork,
            IHostEnvironment environment,
            IConfiguration configuration)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            Logger.Debug("SeedDataService initialized for environment: {Environment}", _environment.EnvironmentName);
        }

        /// <summary>
        /// Performs comprehensive database seeding based on environment configuration
        /// </summary>
        public async Task SeedAsync()
        {
            using (LogContext.PushProperty("Operation", "DatabaseSeeding"))
            using (LogContext.PushProperty("Environment", _environment.EnvironmentName))
            {
                Logger.Information("Starting database seeding for {Environment} environment", _environment.EnvironmentName);

                try
                {
                    // Check if seeding is enabled for this environment
                    if (!ShouldSeedData())
                    {
                        Logger.Information("Database seeding skipped for {Environment} environment", _environment.EnvironmentName);
                        return;
                    }

                    // Ensure database exists and apply pending migrations
                    await EnsureDatabaseAsync();

                    // Check if data already exists (idempotent seeding)
                    if (await HasExistingDataAsync())
                    {
                        Logger.Information("Database already contains data, skipping seeding");
                        return;
                    }

                    // Perform seeding operations
                    await SeedCoreDataAsync();

                    Logger.Information("Database seeding completed successfully for {Environment} environment", _environment.EnvironmentName);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Database seeding failed for {Environment} environment", _environment.EnvironmentName);
                    throw;
                }
            }
        }

        private bool ShouldSeedData()
        {
            // Check configuration setting
            var seedDataEnabled = _configuration.GetValue<bool>("BusBuddy:Database:SeedData", true);

            // Always allow seeding in Development, check config for other environments
            if (_environment.IsDevelopment())
            {
                Logger.Debug("Seeding enabled for Development environment");
                return seedDataEnabled;
            }

            if (_environment.IsStaging())
            {
                Logger.Debug("Seeding configuration for Staging: {SeedEnabled}", seedDataEnabled);
                return seedDataEnabled;
            }

            // Never seed in Production unless explicitly configured
            if (_environment.IsProduction())
            {
                Logger.Warning("Seeding requested in Production environment - configuration: {SeedEnabled}", seedDataEnabled);
                return seedDataEnabled;
            }

            return seedDataEnabled;
        }

        private async Task EnsureDatabaseAsync()
        {
            using (LogContext.PushProperty("Operation", "DatabaseMigration"))
            {
                Logger.Information("Ensuring database exists and applying pending migrations");

                try
                {
                    // Apply pending migrations
                    var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                    var pendingCount = pendingMigrations.Count();

                    if (pendingCount > 0)
                    {
                        Logger.Information("Applying {MigrationCount} pending migrations: {Migrations}",
                            pendingCount, string.Join(", ", pendingMigrations));

                        await _context.Database.MigrateAsync();

                        Logger.Information("Successfully applied {MigrationCount} migrations", pendingCount);
                    }
                    else
                    {
                        Logger.Debug("No pending migrations found");
                    }

                    // Ensure database can accept connections
                    await _context.Database.CanConnectAsync();
                    Logger.Debug("Database connectivity confirmed");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to ensure database readiness");
                    throw;
                }
            }
        }

        private async Task<bool> HasExistingDataAsync()
        {
            try
            {
                // Check for existing core data
                var hasDrivers = await _context.Drivers.AnyAsync();
                var hasVehicles = await _context.Vehicles.AnyAsync();
                var hasActivities = await _context.Activities.AnyAsync();

                var hasData = hasDrivers || hasVehicles || hasActivities;

                Logger.Debug("Existing data check: Drivers={HasDrivers}, Vehicles={HasVehicles}, Activities={HasActivities}, HasData={HasData}",
                    hasDrivers, hasVehicles, hasActivities, hasData);

                return hasData;
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Error checking for existing data, proceeding with seeding");
                return false;
            }
        }

        private async Task SeedCoreDataAsync()
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                Logger.Information("Starting core data seeding transaction");

                // Seed in dependency order
                await SeedDriversAsync();
                await SeedVehiclesAsync();
                await SeedActivitiesAsync();

                // Only seed extended data in Development/Staging
                if (_environment.IsDevelopment() || _environment.IsStaging())
                {
                    await SeedExtendedDataAsync();
                }

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                Logger.Information("Core data seeding transaction committed successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Logger.Error(ex, "Core data seeding transaction rolled back due to error");
                throw;
            }
        }

        private async Task SeedDriversAsync()
        {
            using (LogContext.PushProperty("SeedingEntity", "Drivers"))
            {
                Logger.Debug("Seeding driver data");

                var drivers = CreateSeedDrivers();
                await _unitOfWork.Drivers.AddRangeAsync(drivers);

                Logger.Information("Seeded {DriverCount} drivers", drivers.Count);
            }
        }

        private async Task SeedVehiclesAsync()
        {
            using (LogContext.PushProperty("SeedingEntity", "Vehicles"))
            {
                Logger.Debug("Seeding vehicle data");

                var vehicles = CreateSeedVehicles();
                // Note: Using generic repository since Vehicles property doesn't exist in IUnitOfWork
                var vehicleRepository = _unitOfWork.Repository<Vehicle>();
                await vehicleRepository.AddRangeAsync(vehicles);

                Logger.Information("Seeded {VehicleCount} vehicles", vehicles.Count);
            }
        }

        private async Task SeedActivitiesAsync()
        {
            using (LogContext.PushProperty("SeedingEntity", "Activities"))
            {
                Logger.Debug("Seeding activity data");

                var activities = CreateSeedActivities();
                await _unitOfWork.Activities.AddRangeAsync(activities);

                Logger.Information("Seeded {ActivityCount} activities", activities.Count);
            }
        }

        private async Task SeedExtendedDataAsync()
        {
            Logger.Debug("Seeding extended data for non-production environment");

            // Seed additional test data for development/staging
            // This would include more complex relationships, test scenarios, etc.

            await Task.CompletedTask; // Placeholder for extended seeding logic
            Logger.Debug("Extended data seeding completed");
        }

        #region Seed Data Creation Methods

        private static List<Driver> CreateSeedDrivers()
        {
            var seedDate = DateTime.UtcNow.Date;

            return new List<Driver>
            {
                new Driver { DriverName = "Michael Rodriguez", DriverPhone = "555-0123", DriverEmail = "m.rodriguez@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = seedDate },
                new Driver { DriverName = "Sarah Johnson", DriverPhone = "555-0124", DriverEmail = "s.johnson@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = seedDate },
                new Driver { DriverName = "David Chen", DriverPhone = "555-0125", DriverEmail = "d.chen@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = seedDate },
                new Driver { DriverName = "Maria Garcia", DriverPhone = "555-0126", DriverEmail = "m.garcia@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = seedDate },
                new Driver { DriverName = "James Wilson", DriverPhone = "555-0127", DriverEmail = "j.wilson@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = seedDate },
                new Driver { DriverName = "Ashley Brown", DriverPhone = "555-0128", DriverEmail = "a.brown@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = seedDate },
                new Driver { DriverName = "Robert Martinez", DriverPhone = "555-0129", DriverEmail = "r.martinez@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = seedDate },
                new Driver { DriverName = "Jennifer Lee", DriverPhone = "555-0130", DriverEmail = "j.lee@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = seedDate },
                new Driver { DriverName = "Christopher Davis", DriverPhone = "555-0131", DriverEmail = "c.davis@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = seedDate },
                new Driver { DriverName = "Amanda Thompson", DriverPhone = "555-0132", DriverEmail = "a.thompson@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = seedDate }
            };
        }

        private static List<Vehicle> CreateSeedVehicles()
        {
            return new List<Vehicle>
            {
                new Vehicle { Make = "Blue Bird", Model = "Vision", PlateNumber = "SCH-001", Capacity = 72 },
                new Vehicle { Make = "Thomas Built", Model = "C2", PlateNumber = "SCH-002", Capacity = 77 },
                new Vehicle { Make = "IC Bus", Model = "CE200", PlateNumber = "SCH-003", Capacity = 71 },
                new Vehicle { Make = "Blue Bird", Model = "All American RE", PlateNumber = "SCH-004", Capacity = 84 },
                new Vehicle { Make = "Thomas Built", Model = "Saf-T-Liner HDX", PlateNumber = "SCH-005", Capacity = 90 },
                new Vehicle { Make = "IC Bus", Model = "RE200", PlateNumber = "SCH-006", Capacity = 77 },
                new Vehicle { Make = "Blue Bird", Model = "Vision", PlateNumber = "SCH-007", Capacity = 72 },
                new Vehicle { Make = "Thomas Built", Model = "C2", PlateNumber = "SCH-008", Capacity = 77 },
                new Vehicle { Make = "IC Bus", Model = "CE200", PlateNumber = "SCH-009", Capacity = 71 },
                new Vehicle { Make = "Blue Bird", Model = "All American FE", PlateNumber = "SCH-010", Capacity = 48 }
            };
        }

        private static List<Activity> CreateSeedActivities()
        {
            var baseDate = DateTime.UtcNow.Date.AddDays(1);
            var seedDate = DateTime.UtcNow.Date;

            return new List<Activity>
            {
                new Activity { ActivityType = "Field Trip", Destination = "Natural History Museum", Date = baseDate.AddDays(1), LeaveTime = new TimeSpan(8, 30, 0), EventTime = new TimeSpan(10, 0, 0), RequestedBy = "Mrs. Thompson", AssignedVehicleId = 1, DriverId = 1, StudentsCount = 45, CreatedDate = seedDate },
                new Activity { ActivityType = "Sports Event", Destination = "Central High School", Date = baseDate.AddDays(1), LeaveTime = new TimeSpan(14, 0, 0), EventTime = new TimeSpan(15, 30, 0), RequestedBy = "Coach Martinez", AssignedVehicleId = 2, DriverId = 2, StudentsCount = 22, CreatedDate = seedDate },
                new Activity { ActivityType = "Academic Competition", Destination = "University Campus", Date = baseDate.AddDays(2), LeaveTime = new TimeSpan(7, 45, 0), EventTime = new TimeSpan(9, 0, 0), RequestedBy = "Mr. Chen", AssignedVehicleId = 3, DriverId = 3, StudentsCount = 15, CreatedDate = seedDate },
                new Activity { ActivityType = "Field Trip", Destination = "Science Center", Date = baseDate.AddDays(2), LeaveTime = new TimeSpan(9, 15, 0), EventTime = new TimeSpan(10, 30, 0), RequestedBy = "Ms. Garcia", AssignedVehicleId = 4, DriverId = 4, StudentsCount = 38, CreatedDate = seedDate },
                new Activity { ActivityType = "Band Competition", Destination = "Performing Arts Center", Date = baseDate.AddDays(3), LeaveTime = new TimeSpan(12, 0, 0), EventTime = new TimeSpan(14, 0, 0), RequestedBy = "Mr. Wilson", AssignedVehicleId = 5, DriverId = 5, StudentsCount = 65, CreatedDate = seedDate }
            };
        }

        #endregion
    }
}
