using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using BusBuddy.Core.Models;
using Serilog;
using Serilog.Context;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BusBuddy.Core
{
    /// <summary>
    /// Production-ready BusBuddy Entity Framework Context
    /// Enhanced with comprehensive Serilog logging, error handling, performance optimization, and Azure support
    /// Maintains Phase 1 compatibility while adding enterprise-grade features
    /// </summary>
    public class BusBuddyContext : DbContext
    {
        private static readonly ILogger Logger = Log.ForContext<BusBuddyContext>();

        // Static seed dates for deterministic migrations
        private static readonly DateTime SeedDate = new DateTime(2025, 7, 25);
        private static readonly DateTime BaseActivityDate = new DateTime(2025, 8, 1);

        private readonly IConfiguration? _configuration;

        // Static readonly arrays for Serilog performance optimization
        private static readonly string[] IgnoredModels = { "Bus", "Route" };
        private static readonly string[] IgnoredDriverProperties = { "AMRoutes", "PMRoutes" };

        // Core entities (Phase 1 compatible)
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Activity> Activities { get; set; }

        // Sports scheduling entities (Phase 2)
        public DbSet<SportsEvent> SportsEvents { get; set; }

        // Extended entities (Production ready)
        public DbSet<Bus> Buses { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<RouteStop> RouteStops { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Fuel> Fuels { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<ActivitySchedule> ActivitySchedules { get; set; }
        public DbSet<Destination> Destinations { get; set; }

        // Default constructor for design-time and Phase 1 compatibility
        public BusBuddyContext() { }

        // Constructor for dependency injection with configuration
        public BusBuddyContext(DbContextOptions<BusBuddyContext> options) : base(options) { }

        // Constructor for dependency injection with configuration and IConfiguration
        public BusBuddyContext(DbContextOptions<BusBuddyContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                using (LogContext.PushProperty("DatabaseOperation", "Configuration"))
                using (LogContext.PushProperty("ContextType", "BusBuddyContext"))
                {
                    Logger.Information("Configuring BusBuddyContext for production environment");

                    try
                    {
                        // Get connection string using the appropriate environment helper
                        var connectionString = GetAppropriateConnectionString();

                        var databaseProvider = _configuration?["DatabaseProvider"] ?? "LocalDB";

                        Logger.Information("Using database provider: {Provider}", databaseProvider);
                        Logger.Information("Using connection string type: {ConnectionType}",
                            connectionString.Contains("(localdb)") ? "LocalDB" :
                            connectionString.Contains(".database.windows.net") ? "Azure SQL" :
                            connectionString.Contains("Data Source=") ? "SQLite" : "SQL Server");

                        if (connectionString.Contains("(localdb)"))
                        {
                            // SQL LocalDB configuration for development
                            optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
                            {
                                sqlOptions.EnableRetryOnFailure(
                                    maxRetryCount: 3,
                                    maxRetryDelay: TimeSpan.FromSeconds(10),
                                    errorNumbersToAdd: null);
                                sqlOptions.CommandTimeout(30);
                            });
                            Logger.Information("Configured SQL LocalDB with retry policy: {MaxRetries} retries, {MaxDelay}s max delay",
                                3, 10);
                        }
                        else if (connectionString.Contains("Data Source="))
                        {
                            // SQLite configuration for Phase 1 compatibility
                            optionsBuilder.UseSqlite(connectionString, options =>
                            {
                                options.CommandTimeout(30);
                            });
                            Logger.Information("Configured SQLite database with {Timeout}s timeout", 30);
                        }
                        else if (connectionString.Contains("database.windows.net"))
                        {
                            // Azure SQL Server configuration for production
                            optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
                            {
                                sqlOptions.EnableRetryOnFailure(
                                    maxRetryCount: 5,
                                    maxRetryDelay: TimeSpan.FromSeconds(30),
                                    errorNumbersToAdd: null);
                                sqlOptions.CommandTimeout(60);
                            });
                            Logger.Information("Configured Azure SQL with retry policy: {MaxRetries} retries, {MaxDelay}s max delay",
                                5, 30);
                        }
                        else
                        {
                            // Standard SQL Server configuration for on-premises SQL
                            optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
                            {
                                sqlOptions.EnableRetryOnFailure(
                                    maxRetryCount: 3,
                                    maxRetryDelay: TimeSpan.FromSeconds(15),
                                    errorNumbersToAdd: null);
                                sqlOptions.CommandTimeout(45);
                            });
                            Logger.Information("Configured standard SQL Server with retry policy: {MaxRetries} retries, {MaxDelay}s max delay",
                                3, 15);
                        }

                        // Enhanced logging and monitoring
                        optionsBuilder.EnableDetailedErrors();
                        optionsBuilder.EnableSensitiveDataLogging(isDevelopment());

                        // Add Serilog as EF Core logger
                        optionsBuilder.LogTo(message => Logger.Debug("EF Core: {Message}", message));

                        // Performance and diagnostic configuration
                        optionsBuilder.ConfigureWarnings(warnings =>
                        {
                            warnings.Ignore(RelationalEventId.PendingModelChangesWarning);
                            warnings.Log(CoreEventId.FirstWithoutOrderByAndFilterWarning);
                        });

                        Logger.Information("Database context configuration completed successfully");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Failed to configure BusBuddyContext");
                        throw;
                    }
                }
            }
            else
            {
                Logger.Debug("DbContext already configured, skipping OnConfiguring");
            }
        }

        private bool isDevelopment()
        {
            return Utilities.EnvironmentHelper.IsDevelopment(_configuration);
        }

        private string GetAppropriateConnectionString()
        {
            if (_configuration == null)
            {
                return "Data Source=BusBuddy.db";
            }

            return Utilities.EnvironmentHelper.GetConnectionString(_configuration);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            using (LogContext.PushProperty("DatabaseOperation", "ModelCreation"))
            using (LogContext.PushProperty("Phase", "Phase1"))
            {
                Logger.Information("Starting model creation for BusBuddyContext");

                try
                {
                    // Phase 1: Ignore complex models to avoid navigation property issues
                    modelBuilder.Ignore<Bus>();
                    modelBuilder.Ignore<Route>();
                    Logger.Debug("Ignored complex models {Models} for Phase 1 compatibility", IgnoredModels);

                    // Configure Vehicle entity
                    modelBuilder.Entity<Vehicle>(entity =>
                    {
                        entity.HasKey(e => e.Id);
                        entity.Property(e => e.Make).IsRequired().HasMaxLength(50);
                        entity.Property(e => e.Model).IsRequired().HasMaxLength(50);
                        entity.Property(e => e.PlateNumber).IsRequired().HasMaxLength(20);
                    });
                    Logger.Debug("Configured Vehicle entity with required properties and constraints");

                    // Configure Driver entity and ignore navigation properties
                    modelBuilder.Entity<Driver>(entity =>
                    {
                        entity.HasKey(e => e.DriverId);
                        entity.Property(e => e.DriverName).IsRequired().HasMaxLength(100);
                        entity.Property(e => e.DriverPhone).HasMaxLength(20);
                        entity.Property(e => e.DriverEmail).HasMaxLength(100);
                        entity.Property(e => e.DriversLicenceType).HasMaxLength(50);
                        entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");

                        // Ignore navigation properties for Phase 1
                        entity.Ignore(d => d.AMRoutes);
                        entity.Ignore(d => d.PMRoutes);
                    });
                    Logger.Debug("Configured Driver entity with {IgnoredProperties} navigation properties ignored for Phase 1",
                        IgnoredDriverProperties);

                    // Configure Activity entity
                    modelBuilder.Entity<Activity>(entity =>
                    {
                        entity.HasKey(e => e.ActivityId);
                        entity.Property(e => e.ActivityType).IsRequired().HasMaxLength(50);
                        entity.Property(e => e.Destination).IsRequired().HasMaxLength(200);
                        entity.Property(e => e.RequestedBy).HasMaxLength(100);
                        entity.Property(e => e.DestinationOverride).HasMaxLength(200);

                        // Configure relationship to Destination entity
                        entity.HasOne(a => a.DestinationEntity)
                              .WithMany(d => d.Activities)
                              .HasForeignKey(a => a.DestinationId)
                              .OnDelete(DeleteBehavior.SetNull);
                    });
                    Logger.Debug("Configured Activity entity with required properties and Destination relationship");

                    // Configure SportsEvent entity (Phase 2 Sports Scheduling)
                    modelBuilder.Entity<SportsEvent>(entity =>
                    {
                        entity.HasKey(e => e.Id);
                        entity.Property(e => e.EventName).IsRequired().HasMaxLength(200);
                        entity.Property(e => e.Location).IsRequired().HasMaxLength(500);
                        entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Pending");
                        entity.Property(e => e.SafetyNotes).HasMaxLength(1000);
                        entity.Property(e => e.Sport).HasMaxLength(100);
                        entity.Property(e => e.EmergencyContact).HasMaxLength(500);
                        entity.Property(e => e.WeatherConditions).HasMaxLength(200);

                        // Configure relationships to Vehicle and Driver
                        entity.HasOne(s => s.Vehicle)
                              .WithMany()
                              .HasForeignKey(s => s.VehicleId)
                              .OnDelete(DeleteBehavior.SetNull);

                        entity.HasOne(s => s.Driver)
                              .WithMany()
                              .HasForeignKey(s => s.DriverId)
                              .OnDelete(DeleteBehavior.SetNull);

                        // Indexes for performance
                        entity.HasIndex(e => e.StartTime).HasDatabaseName("IX_SportsEvents_StartTime");
                        entity.HasIndex(e => e.Status).HasDatabaseName("IX_SportsEvents_Status");
                        entity.HasIndex(e => e.Sport).HasDatabaseName("IX_SportsEvents_Sport");
                    });
                    Logger.Debug("Configured SportsEvent entity with Vehicle and Driver relationships for Phase 2");

                    // Configure Destination entity
                    modelBuilder.Entity<Destination>(entity =>
                    {
                        entity.HasKey(e => e.DestinationId);
                        entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                        entity.Property(e => e.Address).IsRequired().HasMaxLength(300);
                        entity.Property(e => e.City).IsRequired().HasMaxLength(100);
                        entity.Property(e => e.State).IsRequired().HasMaxLength(2);
                        entity.Property(e => e.ZipCode).IsRequired().HasMaxLength(10);
                        entity.Property(e => e.ContactName).HasMaxLength(100);
                        entity.Property(e => e.ContactPhone).HasMaxLength(20);
                        entity.Property(e => e.ContactEmail).HasMaxLength(100);
                        entity.Property(e => e.DestinationType).IsRequired().HasMaxLength(50);
                        entity.Property(e => e.SpecialRequirements).HasMaxLength(500);
                        entity.Property(e => e.CreatedBy).HasMaxLength(100);
                        entity.Property(e => e.UpdatedBy).HasMaxLength(100);
                        entity.Property(e => e.Latitude).HasColumnType("decimal(10,8)");
                        entity.Property(e => e.Longitude).HasColumnType("decimal(11,8)");

                        // Indexes for performance
                        entity.HasIndex(e => e.Name).HasDatabaseName("IX_Destinations_Name");
                        entity.HasIndex(e => e.DestinationType).HasDatabaseName("IX_Destinations_Type");
                        entity.HasIndex(e => e.IsActive).HasDatabaseName("IX_Destinations_IsActive");
                    });
                    Logger.Debug("Configured Destination entity with constraints and indexes");

                    // Seed real-world data with logging
                    Logger.Information("Seeding database with real-world data: {DriverCount} drivers, {VehicleCount} vehicles, {ActivityCount} activities, {DestinationCount} destinations",
                        20, 15, 30, 15);

                    seedDriverData(modelBuilder);
                    seedVehicleData(modelBuilder);
                    seedDestinationData(modelBuilder);
                    seedActivityData(modelBuilder);

                    Logger.Information("Model creation completed successfully");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed during model creation");
                    throw;
                }
            }
        }

        private void seedDriverData(ModelBuilder modelBuilder)
        {
            Logger.Debug("Seeding {Count} CDL-certified drivers with seed date {SeedDate}", 20, SeedDate);

            modelBuilder.Entity<Driver>().HasData(
                new Driver { DriverId = 1, DriverName = "Michael Rodriguez", DriverPhone = "555-0123", DriverEmail = "m.rodriguez@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = SeedDate },
                new Driver { DriverId = 2, DriverName = "Sarah Johnson", DriverPhone = "555-0124", DriverEmail = "s.johnson@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = SeedDate },
                new Driver { DriverId = 3, DriverName = "David Chen", DriverPhone = "555-0125", DriverEmail = "d.chen@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = SeedDate },
                new Driver { DriverId = 4, DriverName = "Maria Garcia", DriverPhone = "555-0126", DriverEmail = "m.garcia@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = SeedDate },
                new Driver { DriverId = 5, DriverName = "James Wilson", DriverPhone = "555-0127", DriverEmail = "j.wilson@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = SeedDate },
                new Driver { DriverId = 6, DriverName = "Ashley Brown", DriverPhone = "555-0128", DriverEmail = "a.brown@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = SeedDate },
                new Driver { DriverId = 7, DriverName = "Robert Martinez", DriverPhone = "555-0129", DriverEmail = "r.martinez@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = SeedDate },
                new Driver { DriverId = 8, DriverName = "Jennifer Lee", DriverPhone = "555-0130", DriverEmail = "j.lee@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = SeedDate },
                new Driver { DriverId = 9, DriverName = "Christopher Davis", DriverPhone = "555-0131", DriverEmail = "c.davis@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = SeedDate },
                new Driver { DriverId = 10, DriverName = "Amanda Thompson", DriverPhone = "555-0132", DriverEmail = "a.thompson@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = SeedDate },
                new Driver { DriverId = 11, DriverName = "Kevin Anderson", DriverPhone = "555-0133", DriverEmail = "k.anderson@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = SeedDate },
                new Driver { DriverId = 12, DriverName = "Lisa White", DriverPhone = "555-0134", DriverEmail = "l.white@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = SeedDate },
                new Driver { DriverId = 13, DriverName = "Daniel Moore", DriverPhone = "555-0135", DriverEmail = "d.moore@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = SeedDate },
                new Driver { DriverId = 14, DriverName = "Nicole Taylor", DriverPhone = "555-0136", DriverEmail = "n.taylor@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = SeedDate },
                new Driver { DriverId = 15, DriverName = "Brandon Jackson", DriverPhone = "555-0137", DriverEmail = "b.jackson@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = SeedDate },
                new Driver { DriverId = 16, DriverName = "Stephanie Miller", DriverPhone = "555-0138", DriverEmail = "s.miller@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = SeedDate },
                new Driver { DriverId = 17, DriverName = "Andrew Harris", DriverPhone = "555-0139", DriverEmail = "a.harris@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = SeedDate },
                new Driver { DriverId = 18, DriverName = "Michelle Clark", DriverPhone = "555-0140", DriverEmail = "m.clark@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = SeedDate },
                new Driver { DriverId = 19, DriverName = "Ryan Lewis", DriverPhone = "555-0141", DriverEmail = "r.lewis@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = SeedDate },
                new Driver { DriverId = 20, DriverName = "Samantha Walker", DriverPhone = "555-0142", DriverEmail = "s.walker@busbuddy.edu", DriversLicenceType = "CDL Class B", TrainingComplete = true, Status = "Active", CreatedDate = SeedDate }
            );
        }

        private void seedVehicleData(ModelBuilder modelBuilder)
        {
            Logger.Debug("Seeding {Count} school buses from major manufacturers", 15);

            modelBuilder.Entity<Vehicle>().HasData(
                new Vehicle { Id = 1, Make = "Blue Bird", Model = "Vision", PlateNumber = "SCH-001", Capacity = 72 },
                new Vehicle { Id = 2, Make = "Thomas Built", Model = "C2", PlateNumber = "SCH-002", Capacity = 77 },
                new Vehicle { Id = 3, Make = "IC Bus", Model = "CE200", PlateNumber = "SCH-003", Capacity = 71 },
                new Vehicle { Id = 4, Make = "Blue Bird", Model = "All American RE", PlateNumber = "SCH-004", Capacity = 84 },
                new Vehicle { Id = 5, Make = "Thomas Built", Model = "Saf-T-Liner HDX", PlateNumber = "SCH-005", Capacity = 90 },
                new Vehicle { Id = 6, Make = "IC Bus", Model = "RE200", PlateNumber = "SCH-006", Capacity = 77 },
                new Vehicle { Id = 7, Make = "Blue Bird", Model = "Vision", PlateNumber = "SCH-007", Capacity = 72 },
                new Vehicle { Id = 8, Make = "Thomas Built", Model = "C2", PlateNumber = "SCH-008", Capacity = 77 },
                new Vehicle { Id = 9, Make = "IC Bus", Model = "CE200", PlateNumber = "SCH-009", Capacity = 71 },
                new Vehicle { Id = 10, Make = "Blue Bird", Model = "All American FE", PlateNumber = "SCH-010", Capacity = 48 },
                new Vehicle { Id = 11, Make = "Thomas Built", Model = "Minotour", PlateNumber = "SCH-011", Capacity = 24 },
                new Vehicle { Id = 12, Make = "IC Bus", Model = "AC200", PlateNumber = "SCH-012", Capacity = 90 },
                new Vehicle { Id = 13, Make = "Blue Bird", Model = "Micro Bird", PlateNumber = "SCH-013", Capacity = 30 },
                new Vehicle { Id = 14, Make = "Thomas Built", Model = "EFX", PlateNumber = "SCH-014", Capacity = 35 },
                new Vehicle { Id = 15, Make = "IC Bus", Model = "TE200", PlateNumber = "SCH-015", Capacity = 77 }
            );
        }

        private void seedDestinationData(ModelBuilder modelBuilder)
        {
            Logger.Debug("Seeding {Count} real-world destinations with seed date {SeedDate}", 15, SeedDate); modelBuilder.Entity<Destination>().HasData(
                    new Destination { DestinationId = 1, Name = "Natural History Museum", Address = "200 Central Park West", City = "New York", State = "NY", ZipCode = "10024", ContactName = "Education Director", ContactPhone = "212-769-5100", ContactEmail = "education@amnh.org", DestinationType = "Field Trip", MaxCapacity = 200, Latitude = 40.7813M, Longitude = -73.9740M, CreatedDate = SeedDate, CreatedBy = "System" },
                    new Destination { DestinationId = 2, Name = "Central High School", Address = "1234 Education Blvd", City = "Springfield", State = "IL", ZipCode = "62701", ContactName = "Athletic Director", ContactPhone = "217-555-0200", ContactEmail = "athletics@centralhs.edu", DestinationType = "Sports Event", MaxCapacity = 500, Latitude = 39.7817M, Longitude = -89.6501M, CreatedDate = SeedDate, CreatedBy = "System" },
                    new Destination { DestinationId = 3, Name = "University Campus Science Building", Address = "456 University Ave", City = "Champaign", State = "IL", ZipCode = "61820", ContactName = "Competition Coordinator", ContactPhone = "217-555-0300", ContactEmail = "competitions@university.edu", DestinationType = "Academic Competition", MaxCapacity = 150, Latitude = 40.1020M, Longitude = -88.2272M, CreatedDate = SeedDate, CreatedBy = "System" },
                    new Destination { DestinationId = 4, Name = "Regional Science Center", Address = "789 Discovery Lane", City = "Peoria", State = "IL", ZipCode = "61602", ContactName = "Program Manager", ContactPhone = "309-555-0400", ContactEmail = "programs@sciencecenter.org", DestinationType = "Field Trip", MaxCapacity = 180, Latitude = 40.6936M, Longitude = -89.5890M, CreatedDate = SeedDate, CreatedBy = "System" },
                    new Destination { DestinationId = 5, Name = "Performing Arts Center", Address = "321 Arts Plaza", City = "Bloomington", State = "IL", ZipCode = "61701", ContactName = "Event Coordinator", ContactPhone = "309-555-0500", ContactEmail = "events@pacenter.org", DestinationType = "Band Competition", MaxCapacity = 300, Latitude = 40.4842M, Longitude = -88.9937M, CreatedDate = SeedDate, CreatedBy = "System" },
                    new Destination { DestinationId = 6, Name = "Regional Stadium", Address = "654 Sports Complex Dr", City = "Normal", State = "IL", ZipCode = "61761", ContactName = "Stadium Manager", ContactPhone = "309-555-0600", ContactEmail = "manager@regionalstadium.com", DestinationType = "Sports Event", MaxCapacity = 1000, Latitude = 40.5142M, Longitude = -88.9906M, CreatedDate = SeedDate, CreatedBy = "System" },
                    new Destination { DestinationId = 7, Name = "Community Food Bank", Address = "987 Volunteer Way", City = "Decatur", State = "IL", ZipCode = "62521", ContactName = "Volunteer Coordinator", ContactPhone = "217-555-0700", ContactEmail = "volunteers@foodbank.org", DestinationType = "Community Service", MaxCapacity = 50, Latitude = 39.8403M, Longitude = -88.9548M, CreatedDate = SeedDate, CreatedBy = "System" },
                    new Destination { DestinationId = 8, Name = "Metropolitan Art Museum", Address = "147 Culture Street", City = "Chicago", State = "IL", ZipCode = "60601", ContactName = "Education Specialist", ContactPhone = "312-555-0800", ContactEmail = "education@metart.org", DestinationType = "Field Trip", MaxCapacity = 250, Latitude = 41.8781M, Longitude = -87.6298M, CreatedDate = SeedDate, CreatedBy = "System" },
                    new Destination { DestinationId = 9, Name = "Convention Center East", Address = "258 Conference Blvd", City = "Rockford", State = "IL", ZipCode = "61101", ContactName = "Event Services", ContactPhone = "815-555-0900", ContactEmail = "services@conventioncenter.com", DestinationType = "Academic Competition", MaxCapacity = 400, Latitude = 42.2711M, Longitude = -89.0940M, CreatedDate = SeedDate, CreatedBy = "System" },
                    new Destination { DestinationId = 10, Name = "Aquatic Center", Address = "369 Pool Lane", City = "Carbondale", State = "IL", ZipCode = "62901", ContactName = "Facility Manager", ContactPhone = "618-555-1000", ContactEmail = "manager@aquaticcenter.com", DestinationType = "Sports Event", MaxCapacity = 200, Latitude = 37.7273M, Longitude = -89.2167M, CreatedDate = SeedDate, CreatedBy = "System" },
                    new Destination { DestinationId = 11, Name = "Planetarium & Space Center", Address = "741 Galaxy Road", City = "Aurora", State = "IL", ZipCode = "60506", ContactName = "Education Director", ContactPhone = "630-555-1100", ContactEmail = "education@planetarium.org", DestinationType = "Field Trip", MaxCapacity = 120, Latitude = 41.7606M, Longitude = -88.3201M, CreatedDate = SeedDate, CreatedBy = "System" },
                    new Destination { DestinationId = 12, Name = "Community Theater", Address = "852 Stage Avenue", City = "Joliet", State = "IL", ZipCode = "60431", ContactName = "Theater Manager", ContactPhone = "815-555-1200", ContactEmail = "manager@communitytheatre.org", DestinationType = "Drama Performance", MaxCapacity = 180, Latitude = 41.5250M, Longitude = -88.0817M, CreatedDate = SeedDate, CreatedBy = "System" },
                    new Destination { DestinationId = 13, Name = "Prairie Nature Reserve", Address = "963 Conservation Trail", City = "Champaign", State = "IL", ZipCode = "61822", ContactName = "Park Ranger", ContactPhone = "217-555-1300", ContactEmail = "ranger@prairiepark.gov", DestinationType = "Field Trip", MaxCapacity = 80, SpecialRequirements = "Weather-dependent, hiking boots recommended", Latitude = 40.1164M, Longitude = -88.2434M, CreatedDate = SeedDate, CreatedBy = "System" },
                    new Destination { DestinationId = 14, Name = "Business Innovation District", Address = "159 Enterprise Parkway", City = "Schaumburg", State = "IL", ZipCode = "60173", ContactName = "Career Coordinator", ContactPhone = "847-555-1400", ContactEmail = "careers@bizdistrict.com", DestinationType = "Career Fair", MaxCapacity = 300, Latitude = 42.0334M, Longitude = -88.0834M, CreatedDate = SeedDate, CreatedBy = "System" },
                    new Destination { DestinationId = 15, Name = "Memorial Auditorium", Address = "753 Graduation Circle", City = "Springfield", State = "IL", ZipCode = "62703", ContactName = "Event Coordinator", ContactPhone = "217-555-1500", ContactEmail = "events@memorialaud.gov", DestinationType = "Graduation Ceremony", MaxCapacity = 800, SpecialRequirements = "Formal dress code, restricted parking", Latitude = 39.7990M, Longitude = -89.6439M, CreatedDate = SeedDate, CreatedBy = "System" }
                );
        }

        private void seedActivityData(ModelBuilder modelBuilder)
        {
            Logger.Debug("Seeding {Count} realistic school activities starting from {BaseActivityDate}", 30, BaseActivityDate); modelBuilder.Entity<Activity>().HasData(
                    new Activity { ActivityId = 1, ActivityType = "Field Trip", Destination = "Natural History Museum", Date = BaseActivityDate.AddDays(1), LeaveTime = new TimeSpan(8, 30, 0), EventTime = new TimeSpan(10, 0, 0), RequestedBy = "Mrs. Thompson", AssignedVehicleId = 1, DriverId = 1, StudentsCount = 45, CreatedDate = SeedDate },
                    new Activity { ActivityId = 2, ActivityType = "Sports Event", Destination = "Central High School", Date = BaseActivityDate.AddDays(1), LeaveTime = new TimeSpan(14, 0, 0), EventTime = new TimeSpan(15, 30, 0), RequestedBy = "Coach Martinez", AssignedVehicleId = 2, DriverId = 2, StudentsCount = 22, CreatedDate = SeedDate },
                    new Activity { ActivityId = 3, ActivityType = "Academic Competition", Destination = "University Campus", Date = BaseActivityDate.AddDays(2), LeaveTime = new TimeSpan(7, 45, 0), EventTime = new TimeSpan(9, 0, 0), RequestedBy = "Mr. Chen", AssignedVehicleId = 3, DriverId = 3, StudentsCount = 15, CreatedDate = SeedDate },
                    new Activity { ActivityId = 4, ActivityType = "Field Trip", Destination = "Science Center", Date = BaseActivityDate.AddDays(2), LeaveTime = new TimeSpan(9, 15, 0), EventTime = new TimeSpan(10, 30, 0), RequestedBy = "Ms. Garcia", AssignedVehicleId = 4, DriverId = 4, StudentsCount = 38, CreatedDate = SeedDate },
                    new Activity { ActivityId = 5, ActivityType = "Band Competition", Destination = "Performing Arts Center", Date = BaseActivityDate.AddDays(3), LeaveTime = new TimeSpan(12, 0, 0), EventTime = new TimeSpan(14, 0, 0), RequestedBy = "Mr. Wilson", AssignedVehicleId = 5, DriverId = 5, StudentsCount = 65, CreatedDate = SeedDate },
                    new Activity { ActivityId = 6, ActivityType = "Sports Event", Destination = "Regional Stadium", Date = BaseActivityDate.AddDays(3), LeaveTime = new TimeSpan(16, 30, 0), EventTime = new TimeSpan(18, 0, 0), RequestedBy = "Coach Brown", AssignedVehicleId = 6, DriverId = 6, StudentsCount = 28, CreatedDate = SeedDate },
                    new Activity { ActivityId = 7, ActivityType = "Community Service", Destination = "Local Food Bank", Date = BaseActivityDate.AddDays(4), LeaveTime = new TimeSpan(8, 0, 0), EventTime = new TimeSpan(9, 0, 0), RequestedBy = "Mrs. Martinez", AssignedVehicleId = 7, DriverId = 7, StudentsCount = 20, CreatedDate = SeedDate },
                    new Activity { ActivityId = 8, ActivityType = "Field Trip", Destination = "Art Museum", Date = BaseActivityDate.AddDays(4), LeaveTime = new TimeSpan(10, 45, 0), EventTime = new TimeSpan(12, 0, 0), RequestedBy = "Ms. Lee", AssignedVehicleId = 8, DriverId = 8, StudentsCount = 32, CreatedDate = SeedDate },
                    new Activity { ActivityId = 9, ActivityType = "Academic Competition", Destination = "Convention Center", Date = BaseActivityDate.AddDays(5), LeaveTime = new TimeSpan(7, 30, 0), EventTime = new TimeSpan(8, 45, 0), RequestedBy = "Mr. Davis", AssignedVehicleId = 9, DriverId = 9, StudentsCount = 12, CreatedDate = SeedDate },
                    new Activity { ActivityId = 10, ActivityType = "Sports Event", Destination = "Aquatic Center", Date = BaseActivityDate.AddDays(5), LeaveTime = new TimeSpan(13, 15, 0), EventTime = new TimeSpan(15, 0, 0), RequestedBy = "Coach Thompson", AssignedVehicleId = 10, DriverId = 10, StudentsCount = 18, CreatedDate = SeedDate },
                    new Activity { ActivityId = 11, ActivityType = "Field Trip", Destination = "Planetarium", Date = BaseActivityDate.AddDays(6), LeaveTime = new TimeSpan(9, 30, 0), EventTime = new TimeSpan(11, 0, 0), RequestedBy = "Dr. Anderson", AssignedVehicleId = 11, DriverId = 11, StudentsCount = 25, CreatedDate = SeedDate },
                    new Activity { ActivityId = 12, ActivityType = "Drama Performance", Destination = "Community Theater", Date = BaseActivityDate.AddDays(6), LeaveTime = new TimeSpan(17, 0, 0), EventTime = new TimeSpan(19, 0, 0), RequestedBy = "Ms. White", AssignedVehicleId = 12, DriverId = 12, StudentsCount = 35, CreatedDate = SeedDate },
                    new Activity { ActivityId = 13, ActivityType = "Environmental Study", Destination = "Nature Reserve", Date = BaseActivityDate.AddDays(7), LeaveTime = new TimeSpan(8, 15, 0), EventTime = new TimeSpan(9, 30, 0), RequestedBy = "Mr. Moore", AssignedVehicleId = 13, DriverId = 13, StudentsCount = 28, CreatedDate = SeedDate },
                    new Activity { ActivityId = 14, ActivityType = "Career Fair", Destination = "Business District", Date = BaseActivityDate.AddDays(7), LeaveTime = new TimeSpan(11, 30, 0), EventTime = new TimeSpan(13, 0, 0), RequestedBy = "Ms. Taylor", AssignedVehicleId = 14, DriverId = 14, StudentsCount = 42, CreatedDate = SeedDate },
                    new Activity { ActivityId = 15, ActivityType = "Sports Event", Destination = "Golf Course", Date = BaseActivityDate.AddDays(8), LeaveTime = new TimeSpan(6, 45, 0), EventTime = new TimeSpan(8, 0, 0), RequestedBy = "Coach Jackson", AssignedVehicleId = 15, DriverId = 15, StudentsCount = 8, CreatedDate = SeedDate },
                    new Activity { ActivityId = 16, ActivityType = "Field Trip", Destination = "Historical Society", Date = BaseActivityDate.AddDays(8), LeaveTime = new TimeSpan(10, 0, 0), EventTime = new TimeSpan(11, 30, 0), RequestedBy = "Mrs. Miller", AssignedVehicleId = 1, DriverId = 16, StudentsCount = 30, CreatedDate = SeedDate },
                    new Activity { ActivityId = 17, ActivityType = "Academic Competition", Destination = "Tech Campus", Date = BaseActivityDate.AddDays(9), LeaveTime = new TimeSpan(7, 15, 0), EventTime = new TimeSpan(8, 30, 0), RequestedBy = "Mr. Harris", AssignedVehicleId = 2, DriverId = 17, StudentsCount = 16, CreatedDate = SeedDate },
                    new Activity { ActivityId = 18, ActivityType = "Community Event", Destination = "City Hall", Date = BaseActivityDate.AddDays(9), LeaveTime = new TimeSpan(14, 30, 0), EventTime = new TimeSpan(16, 0, 0), RequestedBy = "Ms. Clark", AssignedVehicleId = 3, DriverId = 18, StudentsCount = 50, CreatedDate = SeedDate },
                    new Activity { ActivityId = 19, ActivityType = "Sports Event", Destination = "Tennis Complex", Date = BaseActivityDate.AddDays(10), LeaveTime = new TimeSpan(15, 45, 0), EventTime = new TimeSpan(17, 15, 0), RequestedBy = "Coach Lewis", AssignedVehicleId = 4, DriverId = 19, StudentsCount = 12, CreatedDate = SeedDate },
                    new Activity { ActivityId = 20, ActivityType = "Field Trip", Destination = "Botanical Gardens", Date = BaseActivityDate.AddDays(10), LeaveTime = new TimeSpan(9, 0, 0), EventTime = new TimeSpan(10, 15, 0), RequestedBy = "Mrs. Walker", AssignedVehicleId = 5, DriverId = 20, StudentsCount = 40, CreatedDate = SeedDate },
                    new Activity { ActivityId = 21, ActivityType = "Music Festival", Destination = "Concert Hall", Date = BaseActivityDate.AddDays(11), LeaveTime = new TimeSpan(13, 30, 0), EventTime = new TimeSpan(15, 30, 0), RequestedBy = "Mr. Rodriguez", AssignedVehicleId = 6, DriverId = 1, StudentsCount = 55, CreatedDate = SeedDate },
                    new Activity { ActivityId = 22, ActivityType = "Science Fair", Destination = "Exhibition Center", Date = BaseActivityDate.AddDays(11), LeaveTime = new TimeSpan(8, 45, 0), EventTime = new TimeSpan(10, 0, 0), RequestedBy = "Dr. Johnson", AssignedVehicleId = 7, DriverId = 2, StudentsCount = 33, CreatedDate = SeedDate },
                    new Activity { ActivityId = 23, ActivityType = "Sports Event", Destination = "Basketball Arena", Date = BaseActivityDate.AddDays(12), LeaveTime = new TimeSpan(17, 15, 0), EventTime = new TimeSpan(19, 0, 0), RequestedBy = "Coach Chen", AssignedVehicleId = 8, DriverId = 3, StudentsCount = 24, CreatedDate = SeedDate },
                    new Activity { ActivityId = 24, ActivityType = "Cultural Exchange", Destination = "International Center", Date = BaseActivityDate.AddDays(12), LeaveTime = new TimeSpan(10, 30, 0), EventTime = new TimeSpan(12, 0, 0), RequestedBy = "Ms. Garcia", AssignedVehicleId = 9, DriverId = 4, StudentsCount = 38, CreatedDate = SeedDate },
                    new Activity { ActivityId = 25, ActivityType = "Field Trip", Destination = "Space Center", Date = BaseActivityDate.AddDays(13), LeaveTime = new TimeSpan(7, 0, 0), EventTime = new TimeSpan(9, 0, 0), RequestedBy = "Mr. Wilson", AssignedVehicleId = 10, DriverId = 5, StudentsCount = 48, CreatedDate = SeedDate },
                    new Activity { ActivityId = 26, ActivityType = "Volunteer Work", Destination = "Senior Center", Date = BaseActivityDate.AddDays(13), LeaveTime = new TimeSpan(14, 0, 0), EventTime = new TimeSpan(15, 30, 0), RequestedBy = "Mrs. Brown", AssignedVehicleId = 11, DriverId = 6, StudentsCount = 22, CreatedDate = SeedDate },
                    new Activity { ActivityId = 27, ActivityType = "Academic Competition", Destination = "Library", Date = BaseActivityDate.AddDays(14), LeaveTime = new TimeSpan(9, 45, 0), EventTime = new TimeSpan(11, 15, 0), RequestedBy = "Mr. Martinez", AssignedVehicleId = 12, DriverId = 7, StudentsCount = 14, CreatedDate = SeedDate },
                    new Activity { ActivityId = 28, ActivityType = "Sports Event", Destination = "Soccer Complex", Date = BaseActivityDate.AddDays(14), LeaveTime = new TimeSpan(16, 0, 0), EventTime = new TimeSpan(18, 30, 0), RequestedBy = "Coach Lee", AssignedVehicleId = 13, DriverId = 8, StudentsCount = 26, CreatedDate = SeedDate },
                    new Activity { ActivityId = 29, ActivityType = "Field Trip", Destination = "Observatory", Date = BaseActivityDate.AddDays(15), LeaveTime = new TimeSpan(19, 30, 0), EventTime = new TimeSpan(21, 0, 0), RequestedBy = "Dr. Davis", AssignedVehicleId = 14, DriverId = 9, StudentsCount = 20, CreatedDate = SeedDate },
            new Activity { ActivityId = 30, ActivityType = "Graduation Ceremony", Destination = "Auditorium", Date = BaseActivityDate.AddDays(15), LeaveTime = new TimeSpan(17, 30, 0), EventTime = new TimeSpan(19, 0, 0), RequestedBy = "Principal Thompson", AssignedVehicleId = 15, DriverId = 10, StudentsCount = 75, CreatedDate = SeedDate }
        );
        }
    }
}
