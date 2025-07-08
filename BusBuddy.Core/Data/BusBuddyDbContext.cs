using Microsoft.EntityFrameworkCore;
using BusBuddy.Core.Models;
using BusBuddy.Core.Models.Base;
using System.Text.Json;
using System.Linq.Expressions;

namespace BusBuddy.Core.Data;
/// <summary>
/// Enhanced Entity Framework DbContext for BusBuddy application
/// Supports agile schema evolution with audit fields, soft deletes, and JSON columns
/// Optimized for Syncfusion Windows Forms with comprehensive indexing and performance features
/// </summary>
public class BusBuddyDbContext : DbContext
{
    /// <summary>
    /// Helper for test code: seed minimal data for a test scenario
    /// </summary>
    public static void SeedTestData(BusBuddyDbContext context, Action<BusBuddyDbContext> seedAction)
    {
        seedAction(context);
        context.SaveChanges();
    }

    /// <summary>
    /// Controls whether to skip global data seeding (for test isolation)
    /// </summary>
    public static bool SkipGlobalSeedData { get; set; } = false;

    private string _currentAuditUser = "System";

    protected BusBuddyDbContext() { }

    public BusBuddyDbContext(DbContextOptions<BusBuddyDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Set the current audit user for tracking purposes
    /// </summary>
    public void SetAuditUser(string userName)
    {
        _currentAuditUser = userName ?? "System";
    }

    /// <summary>
    /// Get the current audit user
    /// </summary>
    public string GetCurrentAuditUser() => _currentAuditUser;

    // DbSets for all entities
    public virtual DbSet<Bus> Vehicles { get; set; }
    public virtual DbSet<ActivityLog> ActivityLogs { get; set; }
    public virtual DbSet<Driver> Drivers { get; set; }
    public virtual DbSet<Route> Routes { get; set; }
    public virtual DbSet<Activity> Activities { get; set; }
    public virtual DbSet<Fuel> FuelRecords { get; set; }
    public virtual DbSet<Maintenance> MaintenanceRecords { get; set; }
    public virtual DbSet<Student> Students { get; set; }
    public virtual DbSet<Schedule> Schedules { get; set; }
    public virtual DbSet<RouteStop> RouteStops { get; set; }
    public virtual DbSet<SchoolCalendar> SchoolCalendar { get; set; }
    public virtual DbSet<ActivitySchedule> ActivitySchedule { get; set; }
    public virtual DbSet<Ticket> Tickets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ActivityLog entity
        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.ToTable("ActivityLogs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.Action).IsRequired().HasMaxLength(200);
            entity.Property(e => e.User).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Details).HasMaxLength(1000);
        });

        // DEBUG: Output the value of SkipGlobalSeedData to verify test isolation
        System.Diagnostics.Debug.WriteLine($"[BusBuddyDbContext] SkipGlobalSeedData: {SkipGlobalSeedData}");
        base.OnModelCreating(modelBuilder);

        // Configure global query filters for soft deletes
        ConfigureGlobalQueryFilters(modelBuilder);

        // Configure Bus (Vehicle) entity with enhanced audit and indexing
        modelBuilder.Entity<Bus>(entity =>
        {
            entity.ToTable("Vehicles");
            entity.HasKey(e => e.VehicleId);

            // Properties with validation and constraints
            entity.Property(e => e.BusNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.VINNumber).IsRequired().HasMaxLength(17);
            entity.Property(e => e.LicenseNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Make).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Model).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(e => e.FleetType).HasMaxLength(20);
            entity.Property(e => e.FuelType).HasMaxLength(20);
            entity.Property(e => e.Department).HasMaxLength(50);
            entity.Property(e => e.GPSDeviceId).HasMaxLength(100);

            // Decimal properties with precision
            entity.Property(e => e.PurchasePrice).HasColumnType("decimal(10,2)");
            entity.Property(e => e.FuelCapacity).HasColumnType("decimal(8,2)");
            entity.Property(e => e.MilesPerGallon).HasColumnType("decimal(6,2)");

            // Text properties
            entity.Property(e => e.InsurancePolicyNumber).HasMaxLength(100);
            entity.Property(e => e.SpecialEquipment).HasMaxLength(1000);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            // Audit fields
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

            // Unique constraints
            entity.HasIndex(e => e.BusNumber).IsUnique().HasDatabaseName("IX_Vehicles_BusNumber");
            entity.HasIndex(e => e.VINNumber).IsUnique().HasDatabaseName("IX_Vehicles_VINNumber");
            entity.HasIndex(e => e.LicenseNumber).IsUnique().HasDatabaseName("IX_Vehicles_LicenseNumber");

            // Performance indexes
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_Vehicles_Status");
            entity.HasIndex(e => e.DateLastInspection).HasDatabaseName("IX_Vehicles_DateLastInspection");
            entity.HasIndex(e => e.InsuranceExpiryDate).HasDatabaseName("IX_Vehicles_InsuranceExpiryDate");
            entity.HasIndex(e => e.FleetType).HasDatabaseName("IX_Vehicles_FleetType");
            entity.HasIndex(e => new { e.Make, e.Model, e.Year }).HasDatabaseName("IX_Vehicles_MakeModelYear");
        });

        // Configure Driver entity with enhanced features
        modelBuilder.Entity<Driver>(entity =>
        {
            entity.ToTable("Drivers");
            entity.HasKey(e => e.DriverId);

            // Properties
            entity.Property(e => e.DriverName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DriverPhone).HasMaxLength(20);
            entity.Property(e => e.DriverEmail).HasMaxLength(100);
            entity.Property(e => e.DriversLicenceType).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.State).HasMaxLength(20);
            entity.Property(e => e.Zip).HasMaxLength(10);
            entity.Property(e => e.EmergencyContactName).HasMaxLength(100);
            entity.Property(e => e.EmergencyContactPhone).HasMaxLength(20);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            // Audit fields
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            entity.HasIndex(e => e.DriverEmail).HasDatabaseName("IX_Drivers_Email");
            entity.HasIndex(e => e.DriverPhone).HasDatabaseName("IX_Drivers_Phone");
            entity.HasIndex(e => e.DriversLicenceType).HasDatabaseName("IX_Drivers_LicenseType");
            entity.HasIndex(e => e.LicenseExpiryDate).HasDatabaseName("IX_Drivers_LicenseExpiration");
            entity.HasIndex(e => e.TrainingComplete).HasDatabaseName("IX_Drivers_TrainingComplete");
        });

        // Configure Route entity with enhanced relationships and indexing
        modelBuilder.Entity<Route>(entity =>
        {
            entity.ToTable("Routes");
            entity.HasKey(e => e.RouteId);

            // Properties
            entity.Property(e => e.RouteName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);

            // Decimal properties
            entity.Property(e => e.AMBeginMiles).HasColumnType("decimal(10,2)");
            entity.Property(e => e.AMEndMiles).HasColumnType("decimal(10,2)");
            entity.Property(e => e.PMBeginMiles).HasColumnType("decimal(10,2)");
            entity.Property(e => e.PMEndMiles).HasColumnType("decimal(10,2)");

            // Configure AM relationships
            entity.HasOne(r => r.AMVehicle)
                  .WithMany(v => v.AMRoutes)
                  .HasForeignKey(r => r.AMVehicleId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_Routes_AMVehicle");

            entity.HasOne(r => r.AMDriver)
                  .WithMany(d => d.AMRoutes)
                  .HasForeignKey(r => r.AMDriverId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_Routes_AMDriver");

            // Configure PM relationships
            entity.HasOne(r => r.PMVehicle)
                  .WithMany(v => v.PMRoutes)
                  .HasForeignKey(r => r.PMVehicleId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_Routes_PMVehicle");

            entity.HasOne(r => r.PMDriver)
                  .WithMany(d => d.PMRoutes)
                  .HasForeignKey(r => r.PMDriverId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_Routes_PMDriver");

            // Indexes for performance
            entity.HasIndex(e => e.Date).HasDatabaseName("IX_Routes_Date");
            entity.HasIndex(e => e.RouteName).HasDatabaseName("IX_Routes_RouteName");
            entity.HasIndex(e => new { e.Date, e.RouteName }).IsUnique().HasDatabaseName("IX_Routes_DateRouteName");
            entity.HasIndex(e => e.AMVehicleId).HasDatabaseName("IX_Routes_AMVehicleId");
            entity.HasIndex(e => e.PMVehicleId).HasDatabaseName("IX_Routes_PMVehicleId");
            entity.HasIndex(e => e.AMDriverId).HasDatabaseName("IX_Routes_AMDriverId");
            entity.HasIndex(e => e.PMDriverId).HasDatabaseName("IX_Routes_PMDriverId");
        });

        // Configure Activity entity with comprehensive indexing
        modelBuilder.Entity<Activity>(entity =>
        {
            entity.ToTable("Activities");
            entity.HasKey(e => e.ActivityId);

            // Properties
            entity.Property(e => e.ActivityType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Destination).IsRequired().HasMaxLength(200);
            entity.Property(e => e.RequestedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Scheduled");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.ActivityCategory).HasMaxLength(100);
            entity.Property(e => e.ApprovedBy).HasMaxLength(100);

            // Decimal properties
            entity.Property(e => e.EstimatedCost).HasColumnType("decimal(10,2)");
            entity.Property(e => e.ActualCost).HasColumnType("decimal(10,2)");

            // Audit fields
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            entity.HasOne(a => a.AssignedVehicle)
                  .WithMany(v => v.Activities)
                  .HasForeignKey(a => a.AssignedVehicleId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_Activities_Vehicle");

            entity.HasOne(a => a.Driver)
                  .WithMany(d => d.Activities)
                  .HasForeignKey(a => a.DriverId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_Activities_Driver");

            entity.HasOne(a => a.Route)
                  .WithMany()
                  .HasForeignKey(a => a.RouteId)
                  .OnDelete(DeleteBehavior.SetNull)
                  .HasConstraintName("FK_Activities_Route");

            // Indexes for scheduling and performance
            entity.HasIndex(e => e.Date).HasDatabaseName("IX_Activities_Date");
            entity.HasIndex(e => e.ActivityType).HasDatabaseName("IX_Activities_ActivityType");
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_Activities_Status");
            entity.HasIndex(e => e.AssignedVehicleId).HasDatabaseName("IX_Activities_VehicleId");
            entity.HasIndex(e => e.DriverId).HasDatabaseName("IX_Activities_DriverId");
            entity.HasIndex(e => e.RouteId).HasDatabaseName("IX_Activities_RouteId");
            entity.HasIndex(e => new { e.Date, e.LeaveTime, e.EventTime }).HasDatabaseName("IX_Activities_DateTimeRange");
            entity.HasIndex(e => new { e.AssignedVehicleId, e.Date, e.LeaveTime }).HasDatabaseName("IX_Activities_VehicleSchedule");
            entity.HasIndex(e => new { e.DriverId, e.Date, e.LeaveTime }).HasDatabaseName("IX_Activities_DriverSchedule");
            entity.HasIndex(e => e.ApprovalRequired).HasDatabaseName("IX_Activities_ApprovalRequired");
        });

        // Configure Fuel entity
        modelBuilder.Entity<Fuel>(entity =>
        {
            entity.ToTable("Fuel");
            entity.HasKey(e => e.FuelId);

            // Properties
            entity.Property(e => e.FuelLocation).HasMaxLength(100);
            entity.Property(e => e.FuelType).HasMaxLength(20).HasDefaultValue("Gasoline");
            entity.Property(e => e.Notes).HasMaxLength(500);

            // Decimal properties with precision
            entity.Property(e => e.Gallons).HasColumnType("decimal(8,3)");
            entity.Property(e => e.PricePerGallon).HasColumnType("decimal(8,3)");
            entity.Property(e => e.TotalCost).HasColumnType("decimal(10,2)");

            // Relationships
            entity.HasOne(f => f.Vehicle)
                  .WithMany(v => v.FuelRecords)
                  .HasForeignKey(f => f.VehicleFueledId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_Fuel_Vehicle");

            // Indexes
            entity.HasIndex(e => e.FuelDate).HasDatabaseName("IX_Fuel_FuelDate");
            entity.HasIndex(e => e.VehicleFueledId).HasDatabaseName("IX_Fuel_VehicleId");
            entity.HasIndex(e => new { e.VehicleFueledId, e.FuelDate }).HasDatabaseName("IX_Fuel_VehicleDate");
            entity.HasIndex(e => e.FuelLocation).HasDatabaseName("IX_Fuel_Location");
            entity.HasIndex(e => e.FuelType).HasDatabaseName("IX_Fuel_Type");
        });

        // Configure Maintenance entity
        modelBuilder.Entity<Maintenance>(entity =>
        {
            entity.ToTable("Maintenance");
            entity.HasKey(e => e.MaintenanceId);

            // Properties
            entity.Property(e => e.MaintenanceCompleted).HasMaxLength(100);
            entity.Property(e => e.Vendor).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.Priority).HasMaxLength(20).HasDefaultValue("Normal");

            // Decimal properties
            entity.Property(e => e.RepairCost).HasColumnType("decimal(10,2)");

            // Audit fields
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            entity.HasOne(m => m.Vehicle)
                  .WithMany(v => v.MaintenanceRecords)
                  .HasForeignKey(m => m.VehicleId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_Maintenance_Vehicle");

            // Indexes
            entity.HasIndex(e => e.Date).HasDatabaseName("IX_Maintenance_Date");
            entity.HasIndex(e => e.VehicleId).HasDatabaseName("IX_Maintenance_VehicleId");
            entity.HasIndex(e => e.MaintenanceCompleted).HasDatabaseName("IX_Maintenance_Type");
            entity.HasIndex(e => new { e.VehicleId, e.Date }).HasDatabaseName("IX_Maintenance_VehicleDate");
            entity.HasIndex(e => e.Priority).HasDatabaseName("IX_Maintenance_Priority");
        });

        // Configure Student entity
        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Students");
            entity.HasKey(e => e.StudentId);

            // Properties
            entity.Property(e => e.StudentName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Grade).HasMaxLength(20);
            entity.Property(e => e.MedicalNotes).HasMaxLength(1000);
            entity.Property(e => e.TransportationNotes).HasMaxLength(1000);
            entity.Property(e => e.EmergencyPhone).HasMaxLength(20);
            entity.Property(e => e.School).HasMaxLength(100);
            entity.Property(e => e.ParentGuardian).HasMaxLength(100);
            entity.Property(e => e.HomeAddress).HasMaxLength(200);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.State).HasMaxLength(2);
            entity.Property(e => e.Zip).HasMaxLength(10);

            // Audit fields
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            entity.HasIndex(e => e.StudentName).HasDatabaseName("IX_Students_Name");
            entity.HasIndex(e => e.Grade).HasDatabaseName("IX_Students_Grade");
            entity.HasIndex(e => e.School).HasDatabaseName("IX_Students_School");
            entity.HasIndex(e => e.Active).HasDatabaseName("IX_Students_Active");
        });

        // Configure Schedule entity
        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.ToTable("Schedules");
            entity.HasKey(e => e.ScheduleId);

            // Relationships
            entity.HasOne(s => s.Bus)
                  .WithMany(b => b.Schedules)
                  .HasForeignKey(s => s.BusId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_Schedules_Bus");

            entity.HasOne(s => s.Route)
                  .WithMany(r => r.Schedules)
                  .HasForeignKey(s => s.RouteId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_Schedules_Route");

            entity.HasOne(s => s.Driver)
                  .WithMany(d => d.Schedules)
                  .HasForeignKey(s => s.DriverId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_Schedules_Driver");

            // Indexes
            entity.HasIndex(e => new { e.RouteId, e.BusId, e.DepartureTime }).IsUnique().HasDatabaseName("IX_Schedules_RouteBusDeparture");
            entity.HasIndex(e => e.ScheduleDate).HasDatabaseName("IX_Schedules_Date");
            entity.HasIndex(e => e.BusId).HasDatabaseName("IX_Schedules_BusId");
            entity.HasIndex(e => e.DriverId).HasDatabaseName("IX_Schedules_DriverId");
            entity.HasIndex(e => e.RouteId).HasDatabaseName("IX_Schedules_RouteId");
        });

        // Configure RouteStop entity
        modelBuilder.Entity<RouteStop>(entity =>
        {
            entity.ToTable("RouteStops");
            entity.HasKey(e => e.RouteStopId);

            // Properties
            entity.Property(e => e.StopName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.StopAddress).HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(500);

            // Relationships
            entity.HasOne(rs => rs.Route)
                  .WithMany()
                  .HasForeignKey(rs => rs.RouteId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_RouteStops_Route");

            // Indexes
            entity.HasIndex(e => e.RouteId).HasDatabaseName("IX_RouteStops_RouteId");
            entity.HasIndex(e => new { e.RouteId, e.StopOrder }).HasDatabaseName("IX_RouteStops_RouteOrder");
        });

        // Configure SchoolCalendar entity
        modelBuilder.Entity<SchoolCalendar>(entity =>
        {
            entity.ToTable("SchoolCalendar");
            entity.HasKey(e => e.CalendarId);

            // Properties
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.EventName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SchoolYear).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(500);

            // Indexes
            entity.HasIndex(e => e.Date).HasDatabaseName("IX_SchoolCalendar_Date");
            entity.HasIndex(e => e.EventType).HasDatabaseName("IX_SchoolCalendar_EventType");
            entity.HasIndex(e => e.SchoolYear).HasDatabaseName("IX_SchoolCalendar_SchoolYear");
            entity.HasIndex(e => e.RoutesRequired).HasDatabaseName("IX_SchoolCalendar_RoutesRequired");
        });

        // Configure ActivitySchedule entity
        modelBuilder.Entity<ActivitySchedule>(entity =>
        {
            entity.ToTable("ActivitySchedule");
            entity.HasKey(e => e.ActivityScheduleId);

            // Properties
            entity.Property(e => e.TripType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ScheduledDestination).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(500);

            // Relationships
            entity.HasOne(ash => ash.ScheduledVehicle)
                  .WithMany(v => v.ScheduledActivities)
                  .HasForeignKey(ash => ash.ScheduledVehicleId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_ActivitySchedule_Vehicle");

            entity.HasOne(ash => ash.ScheduledDriver)
                  .WithMany(d => d.ScheduledActivities)
                  .HasForeignKey(ash => ash.ScheduledDriverId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_ActivitySchedule_Driver");

            // Indexes
            entity.HasIndex(e => e.ScheduledDate).HasDatabaseName("IX_ActivitySchedule_Date");
            entity.HasIndex(e => e.TripType).HasDatabaseName("IX_ActivitySchedule_TripType");
            entity.HasIndex(e => e.ScheduledVehicleId).HasDatabaseName("IX_ActivitySchedule_VehicleId");
            entity.HasIndex(e => e.ScheduledDriverId).HasDatabaseName("IX_ActivitySchedule_DriverId");
        });

        // Configure Ticket entity
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("Tickets");
            entity.HasKey(e => e.TicketId);

            // Properties
            entity.Property(e => e.TicketType).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Valid");
            entity.Property(e => e.PaymentMethod).IsRequired().HasMaxLength(30);
            entity.Property(e => e.QRCode).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.UsedByDriver).HasMaxLength(100);

            // Decimal properties
            entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
            entity.Property(e => e.RefundAmount).HasColumnType("decimal(10,2)");

            // Date properties
            entity.Property(e => e.TravelDate).HasColumnType("date");
            entity.Property(e => e.IssuedDate).HasDefaultValueSql("GETUTCDATE()");

            // Audit fields
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            entity.HasOne(t => t.Student)
                  .WithMany()
                  .HasForeignKey(t => t.StudentId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_Tickets_Student");

            entity.HasOne(t => t.Route)
                  .WithMany()
                  .HasForeignKey(t => t.RouteId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_Tickets_Route");

            // Indexes for performance
            entity.HasIndex(e => e.StudentId).HasDatabaseName("IX_Tickets_StudentId");
            entity.HasIndex(e => e.RouteId).HasDatabaseName("IX_Tickets_RouteId");
            entity.HasIndex(e => e.TravelDate).HasDatabaseName("IX_Tickets_TravelDate");
            entity.HasIndex(e => e.IssuedDate).HasDatabaseName("IX_Tickets_IssuedDate");
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_Tickets_Status");
            entity.HasIndex(e => e.TicketType).HasDatabaseName("IX_Tickets_TicketType");
            entity.HasIndex(e => e.QRCode).IsUnique().HasDatabaseName("IX_Tickets_QRCode");
            entity.HasIndex(e => new { e.StudentId, e.RouteId, e.TravelDate }).HasDatabaseName("IX_Tickets_StudentRouteDate");
            entity.HasIndex(e => new { e.TravelDate, e.Status }).HasDatabaseName("IX_Tickets_TravelDateStatus");
        });

        // Conditionally seed initial data
        // Always skip global seed data if using in-memory provider (for test isolation)
        var isInMemory = this.Database.ProviderName != null && this.Database.ProviderName.Contains("InMemory", StringComparison.OrdinalIgnoreCase);
        if (!isInMemory && !SkipGlobalSeedData)
        {
            SeedData(modelBuilder);
        }
        // If SkipGlobalSeedData is true or using in-memory provider, do NOT call SeedData; ensures no global seed data for in-memory tests
    }

    /// <summary>
    /// Configure global query filters for soft deletes
    /// </summary>
    private void ConfigureGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        // Apply soft delete filter to all entities that inherit from BaseEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType);
                var propertyMethodInfo = typeof(EF).GetMethod("Property")?.MakeGenericMethod(typeof(bool));
                var isDeletedProperty = Expression.Call(propertyMethodInfo!, parameter, Expression.Constant("IsDeleted"));
                var compareExpression = Expression.MakeBinary(ExpressionType.Equal, isDeletedProperty, Expression.Constant(false));
                var lambda = Expression.Lambda(compareExpression, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed sample buses
        modelBuilder.Entity<Bus>().HasData(
            new Bus
            {
                VehicleId = 1,
                BusNumber = "001",
                Year = 2020,
                Make = "Blue Bird",
                Model = "Vision",
                SeatingCapacity = 72,
                VINNumber = "1BAANKCL7LF123456",
                LicenseNumber = "TX123456",
                Status = "Active",
                PurchaseDate = new DateTime(2020, 8, 15),
                PurchasePrice = 85000.00m
            },
            new Bus
            {
                VehicleId = 2,
                BusNumber = "002",
                Year = 2019,
                Make = "Thomas Built",
                Model = "Saf-T-Liner C2",
                SeatingCapacity = 66,
                VINNumber = "4DRBTAAN7KB654321",
                LicenseNumber = "TX654321",
                Status = "Active",
                PurchaseDate = new DateTime(2019, 7, 10),
                PurchasePrice = 82000.00m
            }
        );

        // Seed sample drivers
        modelBuilder.Entity<Driver>().HasData(
            new Driver
            {
                DriverId = 1,
                DriverName = "John Smith",
                DriverPhone = "555-0123",
                DriverEmail = "john.smith@school.edu",
                DriversLicenceType = "CDL",
                TrainingComplete = true
            },
            new Driver
            {
                DriverId = 2,
                DriverName = "Mary Johnson",
                DriverPhone = "555-0456",
                DriverEmail = "mary.johnson@school.edu",
                DriversLicenceType = "CDL",
                TrainingComplete = true
            }
        );
    }

    /// <summary>
    /// Override SaveChanges to apply audit fields
    /// </summary>
    public override int SaveChanges()
    {
        ApplyAuditFields();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync to apply audit fields
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Apply audit fields to entities before saving
    /// </summary>
    private void ApplyAuditFields()
    {
        var entities = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entity in entities)
        {
            var now = DateTime.UtcNow;

            if (entity.State == EntityState.Added)
            {
                entity.Entity.CreatedDate = now;
                entity.Entity.CreatedBy = _currentAuditUser;
            }

            if (entity.State == EntityState.Modified)
            {
                entity.Entity.UpdatedDate = now;
                entity.Entity.UpdatedBy = _currentAuditUser;
                // Prevent modification of CreatedDate and CreatedBy
                entity.Property(x => x.CreatedDate).IsModified = false;
                entity.Property(x => x.CreatedBy).IsModified = false;
            }

            // Call entity-specific OnSaving method
            entity.Entity.OnSaving();
        }
    }
}
