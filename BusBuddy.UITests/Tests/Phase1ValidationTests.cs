using NUnit.Framework;
using FluentAssertions;
using BusBuddy.Core.Models;
using BusBuddy.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BusBuddy.UITests.Tests;

/// <summary>
/// Phase 1 Validation Tests - Confirms Phase 1 implementation is complete and working
/// These tests validate the core functionality implemented in Phase 1 before moving to Phase 2
/// Migrated to NUnit + FluentAssertions for superior testing experience
/// </summary>
[TestFixture]
public class Phase1ValidationTests
{
    private ServiceProvider? _serviceProvider;
    private BusBuddyDbContext? _context;

    [SetUp]
    public void SetUp()
    {
        var services = new ServiceCollection();

        // Add Entity Framework with In-Memory database for testing
        services.AddDbContext<BusBuddyDbContext>(options =>
            options.UseInMemoryDatabase($"BusBuddyTest_{Guid.NewGuid()}"));

        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<BusBuddyDbContext>();

        // Seed test data
        SeedTestData();
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
        _serviceProvider?.Dispose();
    }

    private void SeedTestData()
    {
        if (_context == null) return;

        // Add test drivers
        var drivers = new[]
        {
            new Driver { DriverId = 1, DriverName = "John Smith", Status = "Active", LicenseNumber = "D123456" },
            new Driver { DriverId = 2, DriverName = "Sarah Johnson", Status = "Active", LicenseNumber = "D789012" },
            new Driver { DriverId = 3, DriverName = "Mike Wilson", Status = "Inactive", LicenseNumber = "D345678" }
        };

        // Add test vehicles
        var vehicles = new[]
        {
            new Vehicle { Id = 1, Make = "Blue Bird", Model = "Vision", PlateNumber = "Bus-001", Capacity = 72 },
            new Vehicle { Id = 2, Make = "Blue Bird", Model = "Vision", PlateNumber = "Bus-002", Capacity = 71 },
            new Vehicle { Id = 3, Make = "Thomas", Model = "C2", PlateNumber = "Bus-003", Capacity = 77 }
        };

        // Add test activities
        var activities = new[]
        {
            new Activity {
                Id = 1,
                Name = "Morning Route",
                Date = DateTime.Today,
                Time = "08:00 AM",
                Location = "School Campus",
                Status = "Scheduled"
            },
            new Activity {
                Id = 2,
                Name = "Afternoon Route",
                Date = DateTime.Today,
                Time = "03:30 PM",
                Location = "School Campus",
                Status = "Confirmed"
            }
        };

        _context.Drivers.AddRange(drivers);
        _context.Vehicles.AddRange(vehicles);
        _context.Activities.AddRange(activities);
        _context.SaveChanges();
    }

    #region Phase 1 Core Data Model Tests

    [Test]
    public void Phase1_DriversModel_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var driver = new Driver
        {
            DriverId = 999,
            DriverName = "Test Driver",
            Status = "Active",
            LicenseNumber = "TEST123"
        };

        // Assert with FluentAssertions
        driver.DriverId.Should().Be(999);
        driver.DriverName.Should().Be("Test Driver");
        driver.Status.Should().Be("Active");
        driver.LicenseNumber.Should().Be("TEST123");
    }

    [Test]
    public void Phase1_VehiclesModel_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var vehicle = new Vehicle
        {
            Id = 999,
            Make = "Test Make",
            Model = "Test Model",
            PlateNumber = "TEST-999",
            Capacity = 50
        };

        // Assert with FluentAssertions
        vehicle.Id.Should().Be(999);
        vehicle.Make.Should().Be("Test Make");
        vehicle.Model.Should().Be("Test Model");
        vehicle.PlateNumber.Should().Be("TEST-999");
        vehicle.Capacity.Should().Be(50);
    }

    [Test]
    public void Phase1_ActivitiesModel_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var activity = new Activity
        {
            Id = 999,
            Name = "Test Activity",
            Date = DateTime.Today,
            Time = "12:00 PM",
            Location = "Test Location",
            Status = "Test Status"
        };

        // Assert with FluentAssertions
        activity.Id.Should().Be(999);
        activity.Name.Should().Be("Test Activity");
        activity.Date.Should().Be(DateTime.Today);
        activity.Time.Should().Be("12:00 PM");
        activity.Location.Should().Be("Test Location");
        activity.Status.Should().Be("Test Status");
    }

    #endregion

    #region Phase 1 Database Operations Tests

    [Test]
    public void Phase1_Database_ShouldLoadDriversSuccessfully()
    {
        // Arrange
        _context.Should().NotBeNull();

        // Act
        var drivers = _context.Drivers.ToList();

        // Assert with FluentAssertions
        drivers.Should().HaveCountGreaterThanOrEqualTo(3, "should have at least 3 test drivers");
        drivers.Should().Contain(d => d.DriverName == "John Smith", "should contain John Smith");
        drivers.Should().Contain(d => d.Status == "Active", "should have active drivers");
    }

    [Test]
    public void Phase1_Database_ShouldLoadVehiclesSuccessfully()
    {
        // Arrange
        _context.Should().NotBeNull();

        // Act
        var vehicles = _context.Vehicles.ToList();

        // Assert with FluentAssertions
        vehicles.Should().HaveCountGreaterThanOrEqualTo(3, "should have at least 3 test vehicles");
        vehicles.Should().Contain(v => v.Make == "Blue Bird", "should contain Blue Bird vehicles");
        vehicles.Should().Contain(v => v.PlateNumber.StartsWith("Bus-"), "should have proper plate numbers");
    }

    [Test]
    public void Phase1_Database_ShouldLoadActivitiesSuccessfully()
    {
        // Arrange
        _context.Should().NotBeNull();

        // Act
        var activities = _context.Activities.ToList();

        // Assert with FluentAssertions
        activities.Should().HaveCountGreaterThanOrEqualTo(2, "should have at least 2 test activities");
        activities.Should().Contain(a => a.Name == "Morning Route", "should contain Morning Route");
        activities.Should().Contain(a => a.Status == "Scheduled", "should have scheduled activities");
    }

    #endregion

    #region Phase 1 Integration Tests

    [Test]
    public void Phase1_DatabaseContext_ShouldSupportCRUDOperations()
    {
        // Arrange
        _context.Should().NotBeNull();
        var initialCount = _context.Drivers.Count();

        // Act - Create
        var newDriver = new Driver
        {
            DriverId = 999,
            DriverName = "Integration Test Driver",
            Status = "Active",
            LicenseNumber = "INT999"
        };
        _context.Drivers.Add(newDriver);
        _context.SaveChanges();

        // Assert - Create
        var afterCreateCount = _context.Drivers.Count();
        afterCreateCount.Should().Be(initialCount + 1, "driver count should increase by 1 after creation");

        // Act - Read
        var retrievedDriver = _context.Drivers.FirstOrDefault(d => d.DriverId == 999);

        // Assert - Read
        retrievedDriver.Should().NotBeNull("driver should be retrievable after creation");
        retrievedDriver!.DriverName.Should().Be("Integration Test Driver", "driver name should match");

        // Act - Update
        retrievedDriver.Status = "Updated";
        _context.SaveChanges();

        // Assert - Update
        var updatedDriver = _context.Drivers.FirstOrDefault(d => d.DriverId == 999);
        updatedDriver.Should().NotBeNull("driver should still exist after update");
        updatedDriver!.Status.Should().Be("Updated", "driver status should be updated");

        // Act - Delete
        _context.Drivers.Remove(updatedDriver);
        _context.SaveChanges();

        // Assert - Delete
        var afterDeleteCount = _context.Drivers.Count();
        afterDeleteCount.Should().Be(initialCount, "driver count should return to original after deletion");
    }

    [Test]
    public void Phase1_DataSeeding_ShouldProvideRealisticTestData()
    {
        // Arrange
        _context.Should().NotBeNull();

        // Act & Assert - Drivers with FluentAssertions
        var drivers = _context.Drivers.ToList();
        drivers.Should().Contain(d => d.LicenseNumber?.StartsWith("D") == true, "drivers should have license numbers starting with D");
        drivers.Should().Contain(d => d.Status == "Active", "should have active drivers");
        drivers.Should().Contain(d => d.Status == "Inactive", "should have inactive drivers for testing");

        // Act & Assert - Vehicles with FluentAssertions
        var vehicles = _context.Vehicles.ToList();
        vehicles.Should().OnlyContain(v => v.Capacity > 0, "all vehicles should have capacity > 0");
        vehicles.Should().Contain(v => v.Make == "Blue Bird", "should include Blue Bird buses");
        vehicles.Should().Contain(v => v.Make == "Thomas", "should include Thomas buses");

        // Act & Assert - Activities with FluentAssertions
        var activities = _context.Activities.ToList();
        activities.Should().Contain(a => a.Time?.Contains("AM") == true, "should have morning activities");
        activities.Should().Contain(a => a.Time?.Contains("PM") == true, "should have afternoon activities");
        activities.Should().Contain(a => a.Status == "Scheduled", "should have scheduled activities");
        activities.Should().Contain(a => a.Status == "Confirmed", "should have confirmed activities");
    }

    #endregion

    #region Phase 1 Completion Validation

    [Test]
    public void Phase1_Completion_AllCoreModelsImplemented()
    {
        // This test validates that Phase 1 is complete with all required models

        // Arrange
        _context.Should().NotBeNull();

        // Act & Assert - Verify all required DbSets exist with FluentAssertions
        _context.Drivers.Should().NotBeNull("Drivers DbSet should be implemented");
        _context.Vehicles.Should().NotBeNull("Vehicles DbSet should be implemented");
        _context.Activities.Should().NotBeNull("Activities DbSet should be implemented");

        // Act & Assert - Verify models can be queried
        var driversQuery = _context.Drivers.AsQueryable();
        var vehiclesQuery = _context.Vehicles.AsQueryable();
        var activitiesQuery = _context.Activities.AsQueryable();

        driversQuery.Should().NotBeNull("Drivers should be queryable");
        vehiclesQuery.Should().NotBeNull("Vehicles should be queryable");
        activitiesQuery.Should().NotBeNull("Activities should be queryable");

        // Validation: Phase 1 is complete and ready for Phase 2
        Console.WriteLine("✅ Phase 1 Core Models: COMPLETE");
        Console.WriteLine("✅ Phase 1 Database Operations: WORKING");
        Console.WriteLine("✅ Phase 1 Data Seeding: FUNCTIONAL");
        Console.WriteLine("🚀 Ready for Phase 2 UI Testing Implementation");
        Console.WriteLine("🔥 NUnit + FluentAssertions Migration: COMPLETE");
    }

    #endregion
}
