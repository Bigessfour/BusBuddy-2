using NUnit.Framework;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Bus_Buddy.Data;
using Bus_Buddy.Models;
using Bus_Buddy.Services;
using BusBuddy.Tests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusBuddy.Tests.UnitTests.Services
{
    [TestFixture]
    [NonParallelizable] // Database tests need to run sequentially
    public class ScheduleServiceConsolidatedTests : ConsolidatedTestBase
    {
        private IScheduleService _scheduleService = null!;

        [SetUp]
        public async Task Setup()
        {
            await ClearDatabaseAsync();
            _scheduleService = GetService<IScheduleService>();
        }

        #region GetAllSchedulesAsync Tests

        [Test]
        public async Task GetAllSchedulesAsync_ShouldReturnEmptyList_WhenNoSchedulesExist()
        {
            // Act
            var result = await _scheduleService.GetAllSchedulesAsync();

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull("service should never return null");
                result.Should().BeEmpty("no schedules should exist in clean database");
            }
        }

        [Test]
        public async Task GetAllSchedulesAsync_ShouldReturnOrderedSchedules_WhenSchedulesExist()
        {
            // Arrange
            var vehicle = CreateTestBus("SCH001", "SCHED01234567890");
            var driver = CreateTestDriver("Schedule Driver");
            var route = new Route { RouteName = "Schedule Route 1", Date = DateTime.Today, Description = "Test Route" };

            DbContext.Vehicles.Add(vehicle);
            DbContext.Drivers.Add(driver);
            DbContext.Routes.Add(route);
            await DbContext.SaveChangesAsync();
            ClearChangeTracker();

            var activities = new List<Activity>
            {
                new Activity
                {
                    ActivityDate = DateTime.Today.AddDays(1),
                    StartTime = TimeSpan.FromHours(10),
                    EndTime = TimeSpan.FromHours(11),
                    VehicleId = vehicle.VehicleId,
                    DriverId = driver.DriverId,
                    RouteId = route.RouteId
                },
                new Activity
                {
                    ActivityDate = DateTime.Today,
                    StartTime = TimeSpan.FromHours(8),
                    EndTime = TimeSpan.FromHours(9),
                    VehicleId = vehicle.VehicleId,
                    DriverId = driver.DriverId,
                    RouteId = route.RouteId
                }
            };

            DbContext.Activities.AddRange(activities);
            await DbContext.SaveChangesAsync();
            ClearChangeTracker();

            // Act
            var result = await _scheduleService.GetAllSchedulesAsync();

            // Assert
            using (new AssertionScope())
            {
                result.Should().HaveCount(2, "both activities should be returned");
                result.Should().BeInAscendingOrder(a => a.ActivityDate).And.ThenBeInAscendingOrder(a => a.StartTime);
                result.First().ActivityDate.Should().Be(DateTime.Today, "earlier activity should be first");
                result.First().StartTime.Should().Be(TimeSpan.FromHours(8), "correct start time should be returned");
            }
        }

        #endregion

        #region GetScheduleByIdAsync Tests

        [Test]
        public async Task GetScheduleByIdAsync_ShouldReturnSchedule_WhenScheduleExists()
        {
            // Arrange
            var vehicle = CreateTestBus("SCH001", "SCHED01234567890");
            var driver = CreateTestDriver("Schedule Driver");
            var route = new Route { RouteName = "Schedule Route 1", Date = DateTime.Today, Description = "Test Route" };

            DbContext.Vehicles.Add(vehicle);
            DbContext.Drivers.Add(driver);
            DbContext.Routes.Add(route);
            await DbContext.SaveChangesAsync();
            ClearChangeTracker();

            var activity = new Activity
            {
                ActivityDate = DateTime.Today,
                StartTime = TimeSpan.FromHours(8),
                EndTime = TimeSpan.FromHours(9),
                VehicleId = vehicle.VehicleId,
                DriverId = driver.DriverId,
                RouteId = route.RouteId
            };

            DbContext.Activities.Add(activity);
            await DbContext.SaveChangesAsync();
            ClearChangeTracker();

            // Act
            var result = await _scheduleService.GetScheduleByIdAsync(activity.ActivityId);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull("schedule should be found with valid ID");
                result.ActivityId.Should().Be(activity.ActivityId, "returned schedule should have correct ID");
                result.Vehicle.Should().NotBeNull("vehicle data should be included");
                result.Driver.Should().NotBeNull("driver data should be included");
                result.Route.Should().NotBeNull("route data should be included");
            }
        }

        [Test]
        public async Task GetScheduleByIdAsync_ShouldThrowException_WhenScheduleDoesNotExist()
        {
            // Act & Assert
            await _scheduleService.Invoking(s => s.GetScheduleByIdAsync(999))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Activity with ID 999 not found*");
        }

        #endregion

        #region AddScheduleAsync Tests

        [Test]
        public async Task AddScheduleAsync_ShouldAddSchedule_WhenNoConflictExists()
        {
            // Arrange
            var vehicle = CreateTestBus("SCH001", "SCHED01234567890");
            var driver = CreateTestDriver("Schedule Driver");
            var route = new Route { RouteName = "Schedule Route 1", Date = DateTime.Today, Description = "Test Route" };

            DbContext.Vehicles.Add(vehicle);
            DbContext.Drivers.Add(driver);
            DbContext.Routes.Add(route);
            await DbContext.SaveChangesAsync();
            ClearChangeTracker();

            var newActivity = new Activity
            {
                ActivityDate = DateTime.Today,
                StartTime = TimeSpan.FromHours(8),
                EndTime = TimeSpan.FromHours(9),
                VehicleId = vehicle.VehicleId,
                DriverId = driver.DriverId,
                RouteId = route.RouteId
            };

            // Act
            var result = await _scheduleService.AddScheduleAsync(newActivity);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull("schedule should be added successfully");
                result.ActivityId.Should().BeGreaterThan(0, "new schedule should have valid ID");

                // Verify in database
                var savedActivity = await DbContext.Activities.FindAsync(result.ActivityId);
                savedActivity.Should().NotBeNull("activity should be persisted in database");
            }
        }

        [Test]
        public async Task AddScheduleAsync_ShouldThrowException_WhenVehicleConflictExists()
        {
            // Arrange
            var vehicle = CreateTestBus("SCH001", "SCHED01234567890");
            var driver1 = CreateTestDriver("Driver 1");
            var driver2 = CreateTestDriver("Driver 2");
            var route = new Route { RouteName = "Schedule Route", Date = DateTime.Today, Description = "Test Route" };

            DbContext.Vehicles.Add(vehicle);
            DbContext.Drivers.AddRange(driver1, driver2);
            DbContext.Routes.Add(route);
            await DbContext.SaveChangesAsync();
            ClearChangeTracker();

            // Add existing activity
            var existingActivity = new Activity
            {
                ActivityDate = DateTime.Today,
                StartTime = TimeSpan.FromHours(8),
                EndTime = TimeSpan.FromHours(10),
                VehicleId = vehicle.VehicleId,
                DriverId = driver1.DriverId,
                RouteId = route.RouteId
            };
            DbContext.Activities.Add(existingActivity);
            await DbContext.SaveChangesAsync();
            ClearChangeTracker();

            // Create conflicting activity (same vehicle, overlapping time)
            var conflictingActivity = new Activity
            {
                ActivityDate = DateTime.Today,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(11),
                VehicleId = vehicle.VehicleId,
                DriverId = driver2.DriverId,
                RouteId = route.RouteId
            };

            // Act & Assert
            await _scheduleService.Invoking(s => s.AddScheduleAsync(conflictingActivity))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Schedule conflict detected*");
        }

        #endregion

        #region ValidateScheduleConflictAsync Tests

        [Test]
        public async Task ValidateScheduleConflictAsync_ShouldReturnFalse_WhenNoConflictExists()
        {
            // Arrange
            var vehicle = CreateTestBus("SCH001", "SCHED01234567890");
            var driver = CreateTestDriver("Schedule Driver");
            var route = new Route { RouteName = "Schedule Route", Date = DateTime.Today, Description = "Test Route" };

            DbContext.Vehicles.Add(vehicle);
            DbContext.Drivers.Add(driver);
            DbContext.Routes.Add(route);
            await DbContext.SaveChangesAsync();
            ClearChangeTracker();

            var newActivity = new Activity
            {
                ActivityDate = DateTime.Today,
                StartTime = TimeSpan.FromHours(8),
                EndTime = TimeSpan.FromHours(9),
                VehicleId = vehicle.VehicleId,
                DriverId = driver.DriverId,
                RouteId = route.RouteId
            };

            // Act
            var result = await _scheduleService.ValidateScheduleConflictAsync(newActivity);

            // Assert
            result.Should().BeFalse("there should be no conflict with no existing activities");
        }

        [Test]
        public async Task ValidateScheduleConflictAsync_ShouldReturnTrue_WhenVehicleConflictExists()
        {
            // Arrange
            var vehicle = CreateTestBus("SCH001", "SCHED01234567890");
            var driver1 = CreateTestDriver("Driver 1");
            var driver2 = CreateTestDriver("Driver 2");
            var route = new Route { RouteName = "Schedule Route", Date = DateTime.Today, Description = "Test Route" };

            DbContext.Vehicles.Add(vehicle);
            DbContext.Drivers.AddRange(driver1, driver2);
            DbContext.Routes.Add(route);
            await DbContext.SaveChangesAsync();
            ClearChangeTracker();

            // Add existing activity
            var existingActivity = new Activity
            {
                ActivityDate = DateTime.Today,
                StartTime = TimeSpan.FromHours(8),
                EndTime = TimeSpan.FromHours(10),
                VehicleId = vehicle.VehicleId,
                DriverId = driver1.DriverId,
                RouteId = route.RouteId
            };
            DbContext.Activities.Add(existingActivity);
            await DbContext.SaveChangesAsync();
            ClearChangeTracker();

            // Create conflicting activity (same vehicle, overlapping time)
            var conflictingActivity = new Activity
            {
                ActivityDate = DateTime.Today,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(11),
                VehicleId = vehicle.VehicleId,
                DriverId = driver2.DriverId,
                RouteId = route.RouteId
            };

            // Act
            var result = await _scheduleService.ValidateScheduleConflictAsync(conflictingActivity);

            // Assert
            result.Should().BeTrue("vehicle conflict should be detected");
        }

        [Test]
        public async Task ValidateScheduleConflictAsync_ShouldReturnFalse_WhenNoTimeOverlap()
        {
            // Arrange
            var vehicle = CreateTestBus("SCH001", "SCHED01234567890");
            var driver = CreateTestDriver("Schedule Driver");
            var route = new Route { RouteName = "Schedule Route", Date = DateTime.Today, Description = "Test Route" };

            DbContext.Vehicles.Add(vehicle);
            DbContext.Drivers.Add(driver);
            DbContext.Routes.Add(route);
            await DbContext.SaveChangesAsync();
            ClearChangeTracker();

            // Add existing activity
            var existingActivity = new Activity
            {
                ActivityDate = DateTime.Today,
                StartTime = TimeSpan.FromHours(8),
                EndTime = TimeSpan.FromHours(10),
                VehicleId = vehicle.VehicleId,
                DriverId = driver.DriverId,
                RouteId = route.RouteId
            };
            DbContext.Activities.Add(existingActivity);
            await DbContext.SaveChangesAsync();
            ClearChangeTracker();

            // Create activity with no time overlap (same vehicle, driver, but different time)
            var nonConflictingActivity = new Activity
            {
                ActivityDate = DateTime.Today,
                StartTime = TimeSpan.FromHours(11), // Starts after previous activity ends
                EndTime = TimeSpan.FromHours(12),
                VehicleId = vehicle.VehicleId,
                DriverId = driver.DriverId,
                RouteId = route.RouteId
            };

            // Act
            var result = await _scheduleService.ValidateScheduleConflictAsync(nonConflictingActivity);

            // Assert
            result.Should().BeFalse("non-overlapping times should not create conflict");
        }

        #endregion

        [TearDown]
        public void TearDown()
        {
            // Cleanup handled by base class
        }
    }
}

