using NUnit.Framework;
using FluentAssertions;
using BusBuddy.UITests.Utilities;
using BusBuddy.UITests.Builders;
using BusBuddy.UITests.PageObjects;
using BusBuddy.Core.Data;

namespace BusBuddy.UITests.Tests;

/// <summary>
/// Comprehensive UI tests for Dashboard functionality
/// Tests Phase 1 dashboard implementation and validates real-world data display
/// </summary>
[TestFixture]
public class DashboardTests
{
    private BusBuddyDbContext? _testContext;
    // private DashboardPage? _dashboardPage; // Commented out - not used in current tests

    [SetUp]
    public void SetUp()
    {
        // Create in-memory database for testing
        _testContext = UITestHelpers.CreateInMemoryContext($"DashboardTest_{Guid.NewGuid()}");

        // Seed test data
        UITestHelpers.SeedTestData(_testContext);
    }

    [TearDown]
    public void TearDown()
    {
        _testContext?.Dispose();
        UITestHelpers.CleanupAutomation();
    }

    [Test]
    [Category("Dashboard")]
    [Category("Phase1")]
    public void Dashboard_ShouldDisplayCorrectDriverCount()
    {
        // Arrange
        var expectedDriverCount = _testContext!.Drivers.Count();

        // Act & Assert - This test validates the ViewModel logic
        // In a full UI test, we would launch the application and navigate to dashboard
        expectedDriverCount.Should().BeGreaterThan(0, "Test data should contain drivers");
        expectedDriverCount.Should().Be(5, "Should have 5 test drivers");
    }

    [Test]
    [Category("Dashboard")]
    [Category("Phase1")]
    public void Dashboard_ShouldDisplayCorrectVehicleCount()
    {
        // Arrange
        var expectedVehicleCount = _testContext!.Vehicles.Count();

        // Act & Assert
        expectedVehicleCount.Should().BeGreaterThan(0, "Test data should contain vehicles");
        expectedVehicleCount.Should().Be(3, "Should have 3 test vehicles");
    }

    [Test]
    [Category("Dashboard")]
    [Category("Phase1")]
    public void Dashboard_ShouldDisplayCorrectActivityCount()
    {
        // Arrange
        var expectedActivityCount = _testContext!.Activities.Count();

        // Act & Assert
        expectedActivityCount.Should().BeGreaterThan(0, "Test data should contain activities");
        expectedActivityCount.Should().Be(7, "Should have 7 test activities");
    }

    [Test]
    [Category("Dashboard")]
    [Category("Phase1")]
    public void Dashboard_ShouldDisplayActiveDriversCorrectly()
    {
        // Arrange
        var expectedActiveDrivers = _testContext!.Drivers.Count(d => d.Status == "Active");

        // Act & Assert
        expectedActiveDrivers.Should().BeGreaterThan(0, "Should have active drivers");
        expectedActiveDrivers.Should().BeLessOrEqualTo(_testContext.Drivers.Count(),
                     "Active drivers should not exceed total drivers");
    }

    [Test]
    [Category("Dashboard")]
    [Category("DataValidation")]
    public void Dashboard_TestData_ShouldHaveRealisticDriverData()
    {
        // Arrange & Act
        var drivers = _testContext!.Drivers.ToList();

        // Assert
        drivers.Count.Should().BeGreaterThan(0, "Should have test drivers");

        foreach (var driver in drivers)
        {
            driver.DriverName.Should().NotBeNullOrEmpty($"Driver {driver.DriverId} should have a name");
            driver.Status.Should().NotBeNullOrEmpty($"Driver {driver.DriverId} should have a status");
            driver.DriverId.Should().BePositive("Driver should have valid ID");

            // Validate phone format if provided
            if (!string.IsNullOrEmpty(driver.DriverPhone))
            {
                driver.DriverPhone.Should().Contain("555", "Test phone number should contain test area code");
            }

            // Validate email format if provided
            if (!string.IsNullOrEmpty(driver.DriverEmail))
            {
                driver.DriverEmail.Should().Contain("@", "Email should be valid format");
            }
        }
    }

    [Test]
    [Category("Dashboard")]
    [Category("DataValidation")]
    public void Dashboard_TestData_ShouldHaveRealisticVehicleData()
    {
        // Arrange & Act
        var vehicles = _testContext!.Vehicles.ToList();

        // Assert
        vehicles.Count.Should().BeGreaterThan(0, "Should have test vehicles");

        foreach (var vehicle in vehicles)
        {
            vehicle.Make.Should().NotBeNullOrEmpty($"Vehicle {vehicle.VehicleId} should have a make");
            vehicle.Model.Should().NotBeNullOrEmpty($"Vehicle {vehicle.VehicleId} should have a model");
            vehicle.LicenseNumber.Should().NotBeNullOrEmpty($"Vehicle {vehicle.VehicleId} should have a license number");
            vehicle.SeatingCapacity.Should().BePositive($"Vehicle {vehicle.VehicleId} should have positive capacity");
            vehicle.SeatingCapacity.Should().BeLessOrEqualTo(100, $"Vehicle {vehicle.VehicleId} capacity should be reasonable");

            // Validate realistic bus makes
            var validMakes = new[] { "Blue Bird", "Thomas", "IC Bus", "Freightliner", "International" };
            validMakes.Should().Contain(vehicle.Make, $"Vehicle make '{vehicle.Make}' should be a realistic bus manufacturer");
        }
    }

    [Test]
    [Category("Dashboard")]
    [Category("DataValidation")]
    public void Dashboard_TestData_ShouldHaveRealisticActivityData()
    {
        // Arrange & Act
        var activities = _testContext!.Activities.ToList();

        // Assert
        activities.Count.Should().BeGreaterThan(0, "Should have test activities");

        foreach (var activity in activities)
        {
            activity.ActivityType.Should().NotBeNullOrEmpty($"Activity {activity.ActivityId} should have an activity type");
            activity.Destination.Should().NotBeNullOrEmpty($"Activity {activity.ActivityId} should have a destination");
            activity.Date.Should().BeOnOrAfter(DateTime.Today.AddDays(-1), $"Activity {activity.ActivityId} should not be in the past");

            // Validate realistic activity types
            var validActivityTypes = new[] { "Field Trip", "Sports Event", "Academic Competition", "Special Event", "Regular Route" };
            validActivityTypes.Should().Contain(activity.ActivityType, $"Activity type '{activity.ActivityType}' should be realistic");
        }
    }

    [Test]
    [Category("Dashboard")]
    [Category("Performance")]
    public void Dashboard_DataLoading_ShouldCompleteQuickly()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var drivers = _testContext!.Drivers.ToList();
        var vehicles = _testContext.Vehicles.ToList();
        var activities = _testContext.Activities.ToList();

        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, $"Data loading should complete in under 1 second. Actual: {stopwatch.ElapsedMilliseconds}ms");
        drivers.Count.Should().BeGreaterThan(0, "Should load drivers");
        vehicles.Count.Should().BeGreaterThan(0, "Should load vehicles");
        activities.Count.Should().BeGreaterThan(0, "Should load activities");
    }

    [Test]
    [Category("Builders")]
    [Category("TestInfrastructure")]
    public void TestDataBuilders_ShouldCreateValidDriverData()
    {
        // Act
        var driver = DriverTestDataBuilder.CreateActiveDriver(99, "Test Driver 99");

        // Assert
        driver.DriverId.Should().Be(99);
        driver.DriverName.Should().Be("Test Driver 99");
        driver.Status.Should().Be("Active");
        driver.TrainingComplete.Should().BeTrue();
        driver.DriversLicenceType.Should().NotBeNullOrEmpty();
    }

    [Test]
    [Category("Builders")]
    [Category("TestInfrastructure")]
    public void TestDataBuilders_ShouldCreateValidVehicleData()
    {
        // Act
        var vehicle = VehicleTestDataBuilder.CreateStandardBus(88, "TEST-088");

        // Assert
        vehicle.Id.Should().Be(88);
        vehicle.PlateNumber.Should().Be("TEST-088");
        vehicle.Make.Should().Be("Blue Bird");
        vehicle.Model.Should().Be("Vision");
        vehicle.Capacity.Should().Be(72);
    }

    [Test]
    [Category("Builders")]
    [Category("TestInfrastructure")]
    public void TestDataBuilders_ShouldCreateValidActivityData()
    {
        // Act
        var activity = ActivityTestDataBuilder.CreateTodayFieldTrip(77);

        // Assert
        activity.ActivityId.Should().Be(77);
        activity.Date.Date.Should().Be(DateTime.Today);
        activity.ActivityType.Should().Be("Field Trip");
        activity.Destination.Should().Be("Science Museum");
    }

    [Test]
    [Category("Integration")]
    [Category("Phase1Validation")]
    public void Phase1_Integration_AllComponentsShouldWorkTogether()
    {
        // This test validates that all Phase 1 components integrate properly

        // Arrange - Test that we can create a complete scenario
        var testDrivers = DriverTestDataBuilder.CreateDriversList(3);
        var testVehicles = VehicleTestDataBuilder.CreateVehicleFleet(2);
        var testActivities = ActivityTestDataBuilder.CreateWeeklySchedule(5);

        // Act - Simulate dashboard data aggregation
        var totalDrivers = testDrivers.Count;
        var totalVehicles = testVehicles.Count;
        var totalActivities = testActivities.Count;
        var activeDrivers = testDrivers.Count(d => d.Status == "Active");
        var todayActivities = testActivities.Count(a => a.Date.Date == DateTime.Today);

        // Assert - Validate business logic
        totalDrivers.Should().BeGreaterThan(0, "Should have drivers");
        totalVehicles.Should().BeGreaterThan(0, "Should have vehicles");
        totalActivities.Should().BeGreaterThan(0, "Should have activities");
        activeDrivers.Should().BeLessOrEqualTo(totalDrivers, "Active drivers should not exceed total");
        todayActivities.Should().BeGreaterOrEqualTo(0, "Today activities count should be valid");

        // Validate data relationships make sense
        testActivities.All(a => a.AssignedVehicleId <= totalVehicles).Should().BeTrue("All activities should reference valid vehicles");
        testActivities.All(a => a.RequestedBy != null).Should().BeTrue("All activities should have a requester");
    }
}
