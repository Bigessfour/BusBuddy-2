using NUnit.Framework;
using FluentAssertions;
using BusBuddy.Core.Models;
using BusBuddy.UITests.Builders;

namespace BusBuddy.UITests.Tests;

/// <summary>
/// Comprehensive unit tests for Test Data Builders
/// Ensures that test data builders create consistent, valid test data
/// </summary>
[TestFixture]
[Category("UnitTests")]
[Category("TestInfrastructure")]
[Category("Builders")]
public class TestDataBuilderTests
{
    #region ActivityTestDataBuilder Tests

    [Test]
    public void ActivityTestDataBuilder_DefaultConstructor_ShouldCreateValidActivity()
    {
        // Act
        var activity = new ActivityTestDataBuilder().Build();

        // Assert
        activity.Should().NotBeNull();
        activity.ActivityId.Should().BeGreaterThan(0);
        activity.Date.Should().Be(DateTime.Today);
        activity.ActivityType.Should().Be("Field Trip");
        activity.Destination.Should().Be("Science Museum");
        activity.RequestedBy.Should().Be("Principal Johnson");
        activity.AssignedVehicleId.Should().BeGreaterThan(0);
        activity.DriverId.Should().BeGreaterThan(0);
        activity.StudentsCount.Should().BeGreaterThan(0);
        activity.LeaveTime.Should().BeLessThan(activity.EventTime);
        activity.EventTime.Should().BeLessThan(activity.ReturnTime);
    }

    [Test]
    public void ActivityTestDataBuilder_WithId_ShouldSetCorrectId()
    {
        // Act
        var activity = new ActivityTestDataBuilder()
            .WithId(42)
            .Build();

        // Assert
        activity.ActivityId.Should().Be(42);
    }

    [Test]
    public void ActivityTestDataBuilder_WithDate_ShouldSetCorrectDate()
    {
        // Arrange
        var testDate = DateTime.Today.AddDays(7);

        // Act
        var activity = new ActivityTestDataBuilder()
            .WithDate(testDate)
            .Build();

        // Assert
        activity.Date.Should().Be(testDate);
    }

    [Test]
    public void ActivityTestDataBuilder_AsFieldTrip_ShouldConfigureFieldTripActivity()
    {
        // Act
        var activity = new ActivityTestDataBuilder()
            .AsFieldTrip()
            .Build();

        // Assert
        activity.ActivityType.Should().Be("Field Trip");
        activity.Destination.Should().Be("Science Museum");
        activity.LeaveTime.Should().Be(TimeSpan.Parse("08:00"));
        activity.EventTime.Should().Be(TimeSpan.Parse("09:30"));
        activity.ReturnTime.Should().Be(TimeSpan.Parse("15:00"));
    }

    [Test]
    public void ActivityTestDataBuilder_AsSportsEvent_ShouldConfigureSportsActivity()
    {
        // Act
        var activity = new ActivityTestDataBuilder()
            .AsSportsEvent()
            .Build();

        // Assert
        activity.ActivityType.Should().Be("Sports Event");
        activity.Destination.Should().Be("Regional Stadium");
        activity.LeaveTime.Should().Be(TimeSpan.Parse("14:00"));
        activity.EventTime.Should().Be(TimeSpan.Parse("16:00"));
        activity.ReturnTime.Should().Be(TimeSpan.Parse("20:00"));
    }

    [Test]
    public void ActivityTestDataBuilder_AsAcademicCompetition_ShouldConfigureAcademicActivity()
    {
        // Act
        var activity = new ActivityTestDataBuilder()
            .AsAcademicCompetition()
            .Build();

        // Assert
        activity.ActivityType.Should().Be("Academic Competition");
        activity.Destination.Should().Be("University Campus");
        activity.LeaveTime.Should().Be(TimeSpan.Parse("07:30"));
        activity.EventTime.Should().Be(TimeSpan.Parse("09:00"));
        activity.ReturnTime.Should().Be(TimeSpan.Parse("16:30"));
    }

    [Test]
    public void ActivityTestDataBuilder_ForToday_ShouldSetTodayDate()
    {
        // Act
        var activity = new ActivityTestDataBuilder()
            .ForToday()
            .Build();

        // Assert
        activity.Date.Should().Be(DateTime.Today);
    }

    [Test]
    public void ActivityTestDataBuilder_ForTomorrow_ShouldSetTomorrowDate()
    {
        // Act
        var activity = new ActivityTestDataBuilder()
            .ForTomorrow()
            .Build();

        // Assert
        activity.Date.Should().Be(DateTime.Today.AddDays(1));
    }

    [Test]
    public void ActivityTestDataBuilder_ForNextWeek_ShouldSetNextWeekDate()
    {
        // Act
        var activity = new ActivityTestDataBuilder()
            .ForNextWeek()
            .Build();

        // Assert
        activity.Date.Should().Be(DateTime.Today.AddDays(7));
    }

    [Test]
    public void ActivityTestDataBuilder_CreateTestActivities_ShouldCreateValidList()
    {
        // Act
        var activities = ActivityTestDataBuilder.CreateTestActivities(5);

        // Assert
        activities.Should().HaveCount(5);
        activities.Should().OnlyContain(a => a.ActivityId > 0);
        activities.Should().OnlyContain(a => !string.IsNullOrEmpty(a.ActivityType));
        activities.Should().OnlyContain(a => !string.IsNullOrEmpty(a.Destination));
        activities.Should().OnlyContain(a => !string.IsNullOrEmpty(a.RequestedBy));
        activities.Should().OnlyContain(a => a.AssignedVehicleId > 0);
        activities.Should().OnlyContain(a => a.DriverId > 0);
        activities.Should().OnlyContain(a => a.StudentsCount > 0);
        activities.Select(a => a.ActivityId).Should().OnlyHaveUniqueItems();
    }

    [Test]
    public void ActivityTestDataBuilder_CreateTodayFieldTrip_ShouldCreateFieldTripForToday()
    {
        // Act
        var activity = ActivityTestDataBuilder.CreateTodayFieldTrip(100);

        // Assert
        activity.ActivityId.Should().Be(100);
        activity.Date.Should().Be(DateTime.Today);
        activity.ActivityType.Should().Be("Field Trip");
        activity.StudentsCount.Should().Be(30);
        activity.RequestedBy.Should().Be("Ms. Davis");
        activity.AssignedVehicleId.Should().Be(2);
        activity.DriverId.Should().Be(2);
    }

    [Test]
    public void ActivityTestDataBuilder_CreateWeeklySchedule_ShouldCreateWeekSchedule()
    {
        // Act
        var activities = ActivityTestDataBuilder.CreateWeeklySchedule(2);

        // Assert
        activities.Should().HaveCount(14); // 7 days * 2 activities per day
        activities.Should().OnlyContain(a => a.ActivityId > 0);
        activities.Should().OnlyContain(a => !string.IsNullOrEmpty(a.ActivityType));
        activities.Should().OnlyContain(a => a.AssignedVehicleId <= 2); // Should respect vehicle limits
        activities.Should().OnlyContain(a => a.DriverId <= 2); // Should respect driver limits
        activities.Select(a => a.ActivityId).Should().OnlyHaveUniqueItems();

        // Should span 7 days
        var uniqueDates = activities.Select(a => a.Date.Date).Distinct().ToList();
        uniqueDates.Should().HaveCount(7);
    }

    #endregion

    #region DriverTestDataBuilder Tests

    [Test]
    public void DriverTestDataBuilder_CreateDriversList_ShouldCreateValidDrivers()
    {
        // Act
        var drivers = DriverTestDataBuilder.CreateDriversList(3);

        // Assert
        drivers.Should().HaveCount(3);
        drivers.Should().OnlyContain(d => d.DriverId > 0);
        drivers.Should().OnlyContain(d => !string.IsNullOrEmpty(d.DriverName));
        drivers.Should().OnlyContain(d => d.Status == "Active");
        drivers.Should().OnlyContain(d => d.IsActive);
        drivers.Select(d => d.DriverId).Should().OnlyHaveUniqueItems();
        drivers.Select(d => d.DriverName).Should().OnlyHaveUniqueItems();
    }

    [Test]
    public void DriverTestDataBuilder_CreateDriversList_ShouldHaveReasonableProperties()
    {
        // Act
        var drivers = DriverTestDataBuilder.CreateDriversList(5);

        // Assert
        foreach (var driver in drivers)
        {
            driver.DriverName.Should().NotBeNullOrEmpty();
            driver.DriverName.Should().MatchRegex(@"^[A-Za-z\s]+$", "Driver names should contain only letters and spaces");

            if (driver.DriverPhone != null)
            {
                driver.DriverPhone.Should().MatchRegex(@"[\d\-\(\)\s\+\.]+", "Phone numbers should contain valid phone characters");
            }

            if (driver.DriverEmail != null)
            {
                driver.DriverEmail.Should().Contain("@", "Email should contain @ symbol");
            }

            driver.Status.Should().Be("Active");
            driver.TrainingComplete.Should().BeTrue("Test drivers should have completed training");
        }
    }

    #endregion

    #region VehicleTestDataBuilder Tests

    [Test]
    public void VehicleTestDataBuilder_CreateVehicleFleet_ShouldCreateValidVehicles()
    {
        // Act
        var vehicles = VehicleTestDataBuilder.CreateVehicleFleet(3);

        // Assert
        vehicles.Should().HaveCount(3);
        vehicles.Should().OnlyContain(v => v.Id > 0);
        vehicles.Should().OnlyContain(v => !string.IsNullOrEmpty(v.Make));
        vehicles.Should().OnlyContain(v => !string.IsNullOrEmpty(v.Model));
        vehicles.Should().OnlyContain(v => !string.IsNullOrEmpty(v.PlateNumber));
        vehicles.Should().OnlyContain(v => v.Capacity > 0);
        vehicles.Should().OnlyContain(v => v.IsActive); // Use IsActive instead of Status and IsAvailable
        vehicles.Should().OnlyContain(v => !string.IsNullOrEmpty(v.OperationalStatus));
        vehicles.Select(v => v.Id).Should().OnlyHaveUniqueItems();
        vehicles.Select(v => v.PlateNumber).Should().OnlyHaveUniqueItems();
    }

    [Test]
    public void VehicleTestDataBuilder_CreateVehicleFleet_ShouldHaveReasonableProperties()
    {
        // Act
        var vehicles = VehicleTestDataBuilder.CreateVehicleFleet(5);

        // Assert
        var currentYear = DateTime.Now.Year;
        var commonBusMakes = new[] { "Blue Bird", "International", "Thomas Built Buses", "Freightliner", "Ford" };

        foreach (var vehicle in vehicles)
        {
            vehicle.Make.Should().NotBeNullOrEmpty();
            // Uncomment if you want to enforce specific bus makes
            // commonBusMakes.Should().Contain(vehicle.Make, "Should use common bus manufacturers");

            vehicle.Model.Should().NotBeNullOrEmpty();
            // Note: Vehicle model doesn't have Year property, so we skip year validation

            vehicle.PlateNumber.Should().NotBeNullOrEmpty();
            vehicle.PlateNumber.Should().MatchRegex(@"^[A-Z0-9\-]+$", "Plate numbers should be alphanumeric with hyphens");

            vehicle.Capacity.Should().BeGreaterOrEqualTo(12, "Buses should have minimum capacity");
            vehicle.Capacity.Should().BeLessOrEqualTo(84, "Buses should have reasonable maximum capacity");
            vehicle.Capacity.Should().Match(c => c % 4 == 0 || c % 6 == 0, "Bus capacity should be reasonable for seating arrangements");
        }
    }

    #endregion

    #region Cross-Builder Integration Tests

    [Test]
    public void TestDataBuilders_ShouldCreateConsistentTestData()
    {
        // Act
        var drivers = DriverTestDataBuilder.CreateDriversList(3);
        var vehicles = VehicleTestDataBuilder.CreateVehicleFleet(2);
        var activities = ActivityTestDataBuilder.CreateTestActivities(5);

        // Assert - Data should be internally consistent
        drivers.Should().HaveCount(3);
        vehicles.Should().HaveCount(2);
        activities.Should().HaveCount(5);

        // All activities should reference valid vehicle IDs (1-2 based on our fleet)
        activities.Should().OnlyContain(a => a.AssignedVehicleId >= 1 && a.AssignedVehicleId <= 2);

        // All activities should reference valid driver IDs (1-2 based on our drivers, but limited to vehicle count)
        activities.Should().OnlyContain(a => a.DriverId >= 1 && a.DriverId <= 2);

        // Verify no duplicate IDs
        drivers.Select(d => d.DriverId).Should().OnlyHaveUniqueItems();
        vehicles.Select(v => v.Id).Should().OnlyHaveUniqueItems();
        activities.Select(a => a.ActivityId).Should().OnlyHaveUniqueItems();
    }

    [Test]
    public void TestDataBuilders_ShouldCreateRealisticBusinessScenario()
    {
        // Arrange & Act - Create a realistic school scenario
        var drivers = DriverTestDataBuilder.CreateDriversList(5);
        var vehicles = VehicleTestDataBuilder.CreateVehicleFleet(3);
        var activities = ActivityTestDataBuilder.CreateWeeklySchedule(2); // 2 activities per day for a week

        // Assert - Business scenario validations
        drivers.Count.Should().BeGreaterOrEqualTo(vehicles.Count, "Should have at least as many drivers as vehicles");

        activities.Should().HaveCount(14); // 7 days * 2 activities

        // All activities should be realistically scheduled
        activities.Should().OnlyContain(a => a.LeaveTime < a.EventTime);
        activities.Should().OnlyContain(a => a.EventTime < a.ReturnTime);
        activities.Should().OnlyContain(a => a.StudentsCount > 0 && a.StudentsCount <= 84);

        // Activities should span a week starting from today
        var startDate = DateTime.Today;
        var endDate = startDate.AddDays(6);
        activities.Should().OnlyContain(a => a.Date >= startDate && a.Date <= endDate);

        // Should have variety in activity types
        var activityTypes = activities.Select(a => a.ActivityType).Distinct().ToList();
        activityTypes.Should().HaveCountGreaterOrEqualTo(2, "Should have variety in activity types");
    }

    [Test]
    public void TestDataBuilders_ShouldSupportLargeDataSets()
    {
        // Act - Create large test datasets
        var drivers = DriverTestDataBuilder.CreateDriversList(100);
        var vehicles = VehicleTestDataBuilder.CreateVehicleFleet(50);
        var activities = ActivityTestDataBuilder.CreateTestActivities(200);

        // Assert - Large dataset validations
        drivers.Should().HaveCount(100);
        vehicles.Should().HaveCount(50);
        activities.Should().HaveCount(200);

        // All IDs should still be unique
        drivers.Select(d => d.DriverId).Should().OnlyHaveUniqueItems();
        vehicles.Select(v => v.Id).Should().OnlyHaveUniqueItems();
        activities.Select(a => a.ActivityId).Should().OnlyHaveUniqueItems();

        // Performance check - should complete reasonably quickly
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        DriverTestDataBuilder.CreateDriversList(100);
        VehicleTestDataBuilder.CreateVehicleFleet(50);
        ActivityTestDataBuilder.CreateTestActivities(200);
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "Large dataset creation should be performant");
    }

    #endregion

    #region Builder Validation Tests

    [Test]
    public void TestDataBuilders_ShouldProduceValidEntitiesForEntityFramework()
    {
        // Act
        var driver = DriverTestDataBuilder.CreateDriversList(1).First();
        var vehicle = VehicleTestDataBuilder.CreateVehicleFleet(1).First();
        var activity = ActivityTestDataBuilder.CreateTestActivities(1).First();

        // Assert - Entity Framework compatibility
        // Check required fields are populated
        driver.DriverName.Should().NotBeNullOrEmpty();
        driver.Status.Should().NotBeNullOrEmpty();

        vehicle.Make.Should().NotBeNullOrEmpty();
        vehicle.Model.Should().NotBeNullOrEmpty();
        vehicle.PlateNumber.Should().NotBeNullOrEmpty();

        activity.ActivityType.Should().NotBeNullOrEmpty();
        activity.Destination.Should().NotBeNullOrEmpty();
        activity.RequestedBy.Should().NotBeNullOrEmpty();

        // Check foreign key references are valid
        activity.AssignedVehicleId.Should().BeGreaterThan(0);
        activity.DriverId.Should().BeGreaterThan(0);
    }

    #endregion
}
