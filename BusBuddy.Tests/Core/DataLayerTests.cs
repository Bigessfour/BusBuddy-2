using NUnit.Framework;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using BusBuddy.Core.Data;
using BusBuddy.Core.Models;
using BusBuddy.Tests.Utilities;

namespace BusBuddy.Tests.Core;

/// <summary>
/// Basic Entity Framework CRUD operation tests for MVP
/// Tests core data persistence without complex business logic
/// </summary>
[TestFixture]
[Category("DataLayer")]
public class DataLayerTests : BaseTestFixture, IDisposable
{
    private BusBuddyDbContext _context = null!;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        var options = new DbContextOptionsBuilder<BusBuddyDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BusBuddyDbContext(options);
    }

    [TearDown]
    public override void TearDown()
    {
        _context?.Dispose();
        base.TearDown();
    }

    public void Dispose()
    {
        _context?.Dispose();
        GC.SuppressFinalize(this);
    }

    [Test]
    [Category("CRUD")]
    public async Task Driver_ShouldSaveAndRetrieve()
    {
        // Arrange - Using ACTUAL Driver model properties (scanned from Driver.cs)
        var driver = new Driver
        {
            FirstName = "John",
            LastName = "Doe",
            LicenseNumber = "D123456789",
            LicenseClass = "CDL-B",
            EmergencyContactName = "Jane Doe",
            EmergencyContactPhone = "(555) 123-4567"
        };

        // Act
        _context.Drivers.Add(driver);
        await _context.SaveChangesAsync();

        var retrievedDriver = await _context.Drivers
            .FirstOrDefaultAsync(d => d.FirstName == "John" && d.LastName == "Doe");

        // Assert
        retrievedDriver.Should().NotBeNull();
        retrievedDriver!.FirstName.Should().Be("John");
        retrievedDriver.LastName.Should().Be("Doe");
        retrievedDriver.LicenseNumber.Should().Be("D123456789");
        retrievedDriver.LicenseClass.Should().Be("CDL-B");
    }

    [Test]
    [Category("CRUD")]
    public async Task Vehicle_ShouldSaveAndRetrieve()
    {
        // Arrange
        var vehicle = new Bus
        {
            BusNumber = "BUS001",
            Make = "Blue Bird",
            Model = "Vision",
            Year = 2023,
            SeatingCapacity = 72,
            Status = "Active"
        };

        // Act
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        var retrievedVehicle = await _context.Vehicles
            .FirstOrDefaultAsync(v => v.BusNumber == "BUS001");

        // Assert
        retrievedVehicle.Should().NotBeNull();
        retrievedVehicle!.BusNumber.Should().Be("BUS001");
        retrievedVehicle.Make.Should().Be("Blue Bird");
        retrievedVehicle.Year.Should().Be(2023);
    }

    [Test]
    [Category("CRUD")]
    public async Task Activity_ShouldSaveAndRetrieve()
    {
        // Arrange
        var driver = new Driver { FirstName = "Test", LastName = "Driver", LicenseClass = "CDL-B" };  // ✅ Fixed to actual properties
        var vehicle = new Bus { BusNumber = "BUS001", Make = "Test", Model = "Bus", Year = 2023, SeatingCapacity = 50, Status = "Active" };

        _context.Drivers.Add(driver);
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        var activity = new Activity
        {
            Date = DateTime.Today,
            ActivityType = "Route",
            Description = "Morning Route 1",
            LeaveTime = TimeSpan.Parse("07:00:00"),
            EventTime = TimeSpan.Parse("08:30:00"),
            DriverId = driver.DriverId,
            AssignedVehicleId = vehicle.VehicleId
        };

        // Act
        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        var retrievedActivity = await _context.Activities
            .Include(a => a.Driver)
            .Include(a => a.AssignedVehicle)
            .FirstOrDefaultAsync(a => a.Description == "Morning Route 1");

        // Assert
        retrievedActivity.Should().NotBeNull();
        retrievedActivity!.Description.Should().Be("Morning Route 1");
        retrievedActivity.Driver.Should().NotBeNull();
        retrievedActivity.AssignedVehicle.Should().NotBeNull();
    }
}
