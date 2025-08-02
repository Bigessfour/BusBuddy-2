using NUnit.Framework;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using BusBuddy.Core.Data;
using BusBuddy.Core.Models;
using BusBuddy.UITests.Utilities;
using BusBuddy.UITests.Builders;

namespace BusBuddy.UITests.Tests;

/// <summary>
/// Integration testing patterns using NUnit with FluentAssertions
/// Demonstrates testing with database context and complex scenarios
/// Following Microsoft best practices for integration testing
/// </summary>
[TestFixture]
[Category("IntegrationTests")]
[Category("Database")]
public class IntegrationTestingPatternsTests
{
    private BusBuddyDbContext _context = null!;
    private string _databaseName = null!;

    [SetUp]
    public void SetUp()
    {
        // Create a unique database name for each test to ensure isolation
        _databaseName = $"TestDb_{Guid.NewGuid()}";
        _context = UITestHelpers.CreateInMemoryContext(_databaseName);
        UITestHelpers.SeedTestData(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
    }

    #region Entity Framework Integration Tests

    [Test]
    public void Context_SeedData_ShouldCreateAllRequiredEntities()
    {
        // Act
        var drivers = _context.Drivers.ToList();
        var vehicles = _context.Vehicles.ToList();
        var activities = _context.Activities.ToList();

        // Assert - FluentAssertions for collection testing
        drivers.Should().NotBeEmpty("Seed data should create drivers");
        vehicles.Should().NotBeEmpty("Seed data should create vehicles");
        activities.Should().NotBeEmpty("Seed data should create activities");

        // Verify relationships and data integrity
        drivers.Should().AllSatisfy(driver =>
        {
            driver.Id.Should().BeGreaterThan(0);
            driver.Name.Should().NotBeNullOrEmpty();
            driver.LicenseNumber.Should().NotBeNullOrEmpty();
        });

        vehicles.Should().AllSatisfy(vehicle =>
        {
            vehicle.VehicleId.Should().BeGreaterThan(0);
            vehicle.Make.Should().NotBeNullOrEmpty();
            vehicle.Model.Should().NotBeNullOrEmpty();
        });

        activities.Should().AllSatisfy(activity =>
        {
            activity.ActivityId.Should().BeGreaterThan(0);
            activity.ActivityType.Should().NotBeNullOrEmpty();
            activity.Destination.Should().NotBeNullOrEmpty();
        });
    }

    [Test]
    public void Activities_VehicleReferences_ShouldPointToExistingVehicles()
    {
        // Arrange
        var activities = _context.Activities.ToList();
        var vehicleIds = _context.Vehicles.Select(v => v.VehicleId).ToList();

        // Act & Assert
        activities.Should().AllSatisfy(activity =>
        {
            vehicleIds.Should().Contain(activity.AssignedVehicleId,
                $"Activity {activity.ActivityId} references non-existent vehicle {activity.AssignedVehicleId}");
        });
    }

    [Test]
    public void Activities_DriverReferences_ShouldPointToExistingDrivers()
    {
        // Arrange
        var activities = _context.Activities.ToList();
        var driverIds = _context.Drivers.Select(d => d.Id).ToList();

        // Act & Assert
        activities.Should().AllSatisfy(activity =>
        {
            if (activity.DriverId.HasValue)
            {
                driverIds.Should().Contain(activity.DriverId.Value,
                    $"Activity {activity.ActivityId} references non-existent driver {activity.DriverId}");
            }
        });
    }

    #endregion

    #region Business Logic Integration Tests

    [Test]
    public void VehicleCapacity_vs_ActivityPassengers_ShouldBeValidCombinations()
    {
        // Arrange - Get activities with their assigned vehicles
        var activitiesWithVehicles = _context.Activities
            .Join(_context.Vehicles,
                activity => activity.AssignedVehicleId,
                vehicle => vehicle.VehicleId,
                (activity, vehicle) => new { Activity = activity, Vehicle = vehicle })
            .ToList();

        // Act & Assert
        activitiesWithVehicles.Should().AllSatisfy(combo =>
        {
            var expectedPassengers = combo.Activity.ExpectedPassengers ?? 0;
            var vehicleCapacity = combo.Vehicle.SeatingCapacity;

            expectedPassengers.Should().BeLessOrEqualTo(vehicleCapacity,
                $"Activity {combo.Activity.ActivityId} expects {expectedPassengers} passengers " +
                $"but vehicle {combo.Vehicle.VehicleId} only has capacity for {vehicleCapacity}");
        });
    }

    [Test]
    public void ActivityScheduling_SameDay_ShouldNotDoubleBookVehicles()
    {
        // This test checks for potential scheduling conflicts
        // Note: Current test data may not have conflicts, but this shows the pattern

        // Arrange - Group activities by date and vehicle
        var activitiesByDateAndVehicle = _context.Activities
            .GroupBy(a => new { a.Date.Date, a.AssignedVehicleId })
            .Where(g => g.Count() > 1)
            .ToList();

        // Act & Assert
        foreach (var group in activitiesByDateAndVehicle)
        {
            var activities = group.OrderBy(a => a.LeaveTime).ToList();

            for (int i = 0; i < activities.Count - 1; i++)
            {
                var current = activities[i];
                var next = activities[i + 1];

                // Check for time overlap
                var currentEnd = current.ReturnTime;
                var nextStart = next.LeaveTime;

                nextStart.Should().BeGreaterOrEqualTo(currentEnd,
                    $"Vehicle {group.Key.AssignedVehicleId} on {group.Key.Date:yyyy-MM-dd} " +
                    $"has overlapping activities: {current.ActivityId} and {next.ActivityId}");
            }
        }
    }

    #endregion

    #region Query Performance Tests

    [Test]
    public void DatabaseQueries_BasicOperations_ShouldPerformAdequately()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act - Simulate common dashboard queries
        var driverCount = _context.Drivers.Count();
        var vehicleCount = _context.Vehicles.Count();
        var todayActivities = _context.Activities.Where(a => a.Date.Date == DateTime.Today).ToList();
        var activeDrivers = _context.Drivers.Where(d => d.Status == "Active").ToList();

        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "Basic queries should complete quickly");
        driverCount.Should().BeGreaterThan(0);
        vehicleCount.Should().BeGreaterThan(0);
        todayActivities.Should().NotBeNull();
        activeDrivers.Should().NotBeNull();
    }

    #endregion

    #region Data Validation Integration Tests

    [Test]
    public void Entities_AllStringProperties_ShouldNotExceedMaxLengths()
    {
        // This test validates that seeded data respects entity constraints

        // Test Driver constraints
        var drivers = _context.Drivers.ToList();
        drivers.Should().AllSatisfy(driver =>
        {
            driver.Name.Length.Should().BeLessOrEqualTo(100, "Driver name should not exceed max length");
            driver.LicenseNumber.Length.Should().BeLessOrEqualTo(50, "License number should not exceed max length");
        });

        // Test Vehicle constraints
        var vehicles = _context.Vehicles.ToList();
        vehicles.Should().AllSatisfy(vehicle =>
        {
            vehicle.Make.Length.Should().BeLessOrEqualTo(50, "Vehicle make should not exceed max length");
            vehicle.Model.Length.Should().BeLessOrEqualTo(50, "Vehicle model should not exceed max length");
            vehicle.LicenseNumber.Length.Should().BeLessOrEqualTo(20, "License number should not exceed max length");
        });

        // Test Activity constraints
        var activities = _context.Activities.ToList();
        activities.Should().AllSatisfy(activity =>
        {
            activity.ActivityType.Length.Should().BeLessOrEqualTo(50, "Activity type should not exceed max length");
            activity.Destination.Length.Should().BeLessOrEqualTo(200, "Destination should not exceed max length");
            activity.RequestedBy.Length.Should().BeLessOrEqualTo(100, "Requested by should not exceed max length");
        });
    }

    [Test]
    public void Entities_RequiredProperties_ShouldNotBeNullOrEmpty()
    {
        // Test required properties on all entities

        var drivers = _context.Drivers.ToList();
        drivers.Should().AllSatisfy(driver =>
        {
            driver.Name.Should().NotBeNullOrEmpty("Driver name is required");
            driver.LicenseNumber.Should().NotBeNullOrEmpty("Driver license number is required");
        });

        var activities = _context.Activities.ToList();
        activities.Should().AllSatisfy(activity =>
        {
            activity.ActivityType.Should().NotBeNullOrEmpty("Activity type is required");
            activity.Destination.Should().NotBeNullOrEmpty("Activity destination is required");
            activity.RequestedBy.Should().NotBeNullOrEmpty("Activity requester is required");
        });
    }

    #endregion

    #region Complex Scenario Testing

    [Test]
    public void CompleteWorkflow_CreateActivity_ShouldMaintainDataIntegrity()
    {
        // This test simulates creating a complete activity with all relationships

        // Arrange - Get existing entities for references
        var availableVehicle = _context.Vehicles.First();
        var availableDriver = _context.Drivers.First();

        var newActivity = new Activity
        {
            Date = DateTime.Today.AddDays(1),
            ActivityType = "Field Trip",
            Destination = "Science Center",
            LeaveTime = TimeSpan.Parse("09:00"),
            EventTime = TimeSpan.Parse("10:00"),
            ReturnTime = TimeSpan.Parse("15:00"),
            RequestedBy = "Ms. Teacher",
            AssignedVehicleId = availableVehicle.VehicleId,
            DriverId = availableDriver.Id,
            StudentsCount = 30,
            ExpectedPassengers = 30,
            Status = "Scheduled"
        };

        // Act
        _context.Activities.Add(newActivity);
        _context.SaveChanges();

        // Assert
        var savedActivity = _context.Activities
            .FirstOrDefault(a => a.Destination == "Science Center");

        savedActivity.Should().NotBeNull("Activity should be saved to database");
        savedActivity!.ActivityId.Should().BeGreaterThan(0, "Activity should get a valid ID");
        savedActivity.AssignedVehicleId.Should().Be(availableVehicle.VehicleId);
        savedActivity.DriverId.Should().Be(availableDriver.Id);

        // Verify the activity is properly linked
        _context.Vehicles.Any(v => v.VehicleId == savedActivity.AssignedVehicleId)
            .Should().BeTrue("Referenced vehicle should exist");
        _context.Drivers.Any(d => d.Id == savedActivity.DriverId)
            .Should().BeTrue("Referenced driver should exist");
    }

    [Test]
    public void MultiEntityQueries_JoinOperations_ShouldReturnExpectedResults()
    {
        // Test complex queries that join multiple entities

        // Act - Query activities with their vehicle and driver information
        var activitiesWithDetails = _context.Activities
            .Join(_context.Vehicles,
                activity => activity.AssignedVehicleId,
                vehicle => vehicle.VehicleId,
                (activity, vehicle) => new { Activity = activity, Vehicle = vehicle })
            .Join(_context.Drivers,
                combined => combined.Activity.DriverId,
                driver => driver.Id,
                (combined, driver) => new
                {
                    ActivityId = combined.Activity.ActivityId,
                    ActivityType = combined.Activity.ActivityType,
                    Destination = combined.Activity.Destination,
                    VehicleMake = combined.Vehicle.Make,
                    VehicleModel = combined.Vehicle.Model,
                    DriverName = driver.Name
                })
            .ToList();

        // Assert
        activitiesWithDetails.Should().NotBeEmpty("Join query should return results");
        activitiesWithDetails.Should().AllSatisfy(detail =>
        {
            detail.ActivityId.Should().BeGreaterThan(0);
            detail.ActivityType.Should().NotBeNullOrEmpty();
            detail.Destination.Should().NotBeNullOrEmpty();
            detail.VehicleMake.Should().NotBeNullOrEmpty();
            detail.VehicleModel.Should().NotBeNullOrEmpty();
            detail.DriverName.Should().NotBeNullOrEmpty();
        });
    }

    #endregion

    #region Error Handling Integration Tests

    [Test]
    public void Database_InvalidOperations_ShouldHandleGracefully()
    {
        // Test database error scenarios

        // Act & Assert - Test referential integrity
        var invalidActivity = new Activity
        {
            Date = DateTime.Today,
            ActivityType = "Test",
            Destination = "Test Location",
            LeaveTime = TimeSpan.Parse("09:00"),
            EventTime = TimeSpan.Parse("10:00"),
            ReturnTime = TimeSpan.Parse("15:00"),
            RequestedBy = "Test User",
            AssignedVehicleId = 99999, // Non-existent vehicle
            DriverId = 99999,          // Non-existent driver
            Status = "Scheduled"
        };

        // In a real scenario with proper foreign key constraints, this should fail
        // For in-memory database, it might succeed but violate business rules
        Action addInvalidActivity = () =>
        {
            _context.Activities.Add(invalidActivity);
            _context.SaveChanges();
        };

        // This test documents the current behavior
        // In production, you'd want proper validation
        addInvalidActivity.Should().NotThrow("In-memory database allows invalid references");

        // But we can validate business rules
        var savedActivity = _context.Activities.FirstOrDefault(a => a.AssignedVehicleId == 99999);
        if (savedActivity != null)
        {
            _context.Vehicles.Any(v => v.VehicleId == savedActivity.AssignedVehicleId)
                .Should().BeFalse("Invalid vehicle reference should not exist");
        }
    }

    #endregion
}
