using NUnit.Framework;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Bus_Buddy.Data;
using Bus_Buddy.Models;
using Bus_Buddy.Services;
using BusBuddy.Tests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusBuddy.Tests.UnitTests
{
    [TestFixture]
    [Category("Services")]
    [NonParallelizable] // Database tests need to run sequentially to avoid DbContext conflicts
    public class ScheduleServiceTests
    {
        private BusBuddyDbContext _context;
        private ScheduleService _scheduleService;

        [SetUp]
        public void Setup()
        {
            // Create a unique in-memory database for each test
            var options = new DbContextOptionsBuilder<BusBuddyDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new BusBuddyDbContext(options);
            _scheduleService = new ScheduleService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }

        #region GetAllSchedulesAsync Tests

        [Test]
        public async Task GetAllSchedulesAsync_ShouldReturnEmptyList_WhenNoSchedulesExist()
        {
            // Act
            var result = await _scheduleService.GetAllSchedulesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetAllSchedulesAsync_ShouldReturnOrderedSchedules_WhenSchedulesExist()
        {
            // Arrange
            var vehicle = new Bus { BusNumber = "001", Make = "Test", Model = "Bus", SeatingCapacity = 50 };
            var driver = new Driver { DriverName = "John Doe", LicenseNumber = "12345" };
            var route = new Route { RouteName = "Route 1", Date = DateTime.Today, Description = "Test Route" };

            _context.Vehicles.Add(vehicle);
            _context.Drivers.Add(driver);
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

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

            _context.Activities.AddRange(activities);
            await _context.SaveChangesAsync();

            // Act
            var result = await _scheduleService.GetAllSchedulesAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeInAscendingOrder(a => a.ActivityDate).And.ThenBeInAscendingOrder(a => a.StartTime);
            result.First().ActivityDate.Should().Be(DateTime.Today);
        }

        #endregion

        #region GetScheduleByIdAsync Tests

        [Test]
        public async Task GetScheduleByIdAsync_ShouldReturnSchedule_WhenScheduleExists()
        {
            // Arrange
            var vehicle = new Bus { BusNumber = "001", Make = "Test", Model = "Bus", SeatingCapacity = 50 };
            var driver = new Driver { DriverName = "John Doe", LicenseNumber = "12345" };
            var route = new Route { RouteName = "Route 1", Date = DateTime.Today, Description = "Test Route" };

            _context.Vehicles.Add(vehicle);
            _context.Drivers.Add(driver);
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            var activity = new Activity
            {
                ActivityDate = DateTime.Today,
                StartTime = TimeSpan.FromHours(8),
                EndTime = TimeSpan.FromHours(9),
                VehicleId = vehicle.VehicleId,
                DriverId = driver.DriverId,
                RouteId = route.RouteId
            };

            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            // Act
            var result = await _scheduleService.GetScheduleByIdAsync(activity.ActivityId);

            // Assert
            result.Should().NotBeNull();
            result.ActivityId.Should().Be(activity.ActivityId);
            result.Vehicle.Should().NotBeNull();
            result.Driver.Should().NotBeNull();
            result.Route.Should().NotBeNull();
        }

        [Test]
        public async Task GetScheduleByIdAsync_ShouldThrowException_WhenScheduleDoesNotExist()
        {
            // Act & Assert
            try
            {
                await _scheduleService.GetScheduleByIdAsync(999);
                Assert.Fail("Expected InvalidOperationException was not thrown");
            }
            catch (InvalidOperationException ex)
            {
                ex.Message.Should().Contain("Activity with ID 999 not found");
            }
        }

        #endregion

        #region GetSchedulesByDateRangeAsync Tests

        [Test]
        public async Task GetSchedulesByDateRangeAsync_ShouldReturnSchedulesInRange_WhenSchedulesExist()
        {
            // Arrange
            var vehicle = new Bus { BusNumber = "001", Make = "Test", Model = "Bus", SeatingCapacity = 50 };
            var driver = new Driver { DriverName = "John Doe", LicenseNumber = "12345" };
            var route = new Route { RouteName = "Route 1", Date = DateTime.Today, Description = "Test Route" };

            _context.Vehicles.Add(vehicle);
            _context.Drivers.Add(driver);
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            var activities = new List<Activity>
            {
                new Activity
                {
                    ActivityDate = DateTime.Today,
                    StartTime = TimeSpan.FromHours(8),
                    EndTime = TimeSpan.FromHours(9),
                    VehicleId = vehicle.VehicleId,
                    DriverId = driver.DriverId,
                    RouteId = route.RouteId
                },
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
                    ActivityDate = DateTime.Today.AddDays(5),
                    StartTime = TimeSpan.FromHours(12),
                    EndTime = TimeSpan.FromHours(13),
                    VehicleId = vehicle.VehicleId,
                    DriverId = driver.DriverId,
                    RouteId = route.RouteId
                }
            };

            _context.Activities.AddRange(activities);
            await _context.SaveChangesAsync();

            // Act
            var result = await _scheduleService.GetSchedulesByDateRangeAsync(DateTime.Today, DateTime.Today.AddDays(2));

            // Assert
            result.Should().HaveCount(2);
            result.All(a => a.ActivityDate >= DateTime.Today && a.ActivityDate <= DateTime.Today.AddDays(2)).Should().BeTrue();
        }

        [Test]
        public async Task GetSchedulesByDateRangeAsync_ShouldReturnEmptyList_WhenNoSchedulesInRange()
        {
            // Act
            var result = await _scheduleService.GetSchedulesByDateRangeAsync(DateTime.Today.AddDays(10), DateTime.Today.AddDays(15));

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region AddScheduleAsync Tests

        [Test]
        public async Task AddScheduleAsync_ShouldAddSchedule_WhenNoConflictExists()
        {
            // Arrange
            var vehicle = new Bus { BusNumber = "001", Make = "Test", Model = "Bus", SeatingCapacity = 50 };
            var driver = new Driver { DriverName = "John Doe", LicenseNumber = "12345" };
            var route = new Route { RouteName = "Route 1", Date = DateTime.Today, Description = "Test Route" };

            _context.Vehicles.Add(vehicle);
            _context.Drivers.Add(driver);
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

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
            result.Should().NotBeNull();
            result.ActivityId.Should().BeGreaterThan(0);

            var addedActivity = await _context.Activities.FindAsync(result.ActivityId);
            addedActivity.Should().NotBeNull();
        }

        [Test]
        public async Task AddScheduleAsync_ShouldThrowException_WhenVehicleConflictExists()
        {
            // Arrange
            var vehicle = new Bus { BusNumber = "001", Make = "Test", Model = "Bus", SeatingCapacity = 50 };
            var driver1 = new Driver { DriverName = "John Doe", LicenseNumber = "12345" };
            var driver2 = new Driver { DriverName = "Jane Smith", LicenseNumber = "67890" };
            var route = new Route { RouteName = "Route 1", Date = DateTime.Today, Description = "Test Route" };

            _context.Vehicles.Add(vehicle);
            _context.Drivers.AddRange(driver1, driver2);
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

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
            _context.Activities.Add(existingActivity);
            await _context.SaveChangesAsync();

            // Try to add conflicting activity (same vehicle, overlapping time)
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
            try
            {
                await _scheduleService.AddScheduleAsync(conflictingActivity);
                Assert.Fail("Expected InvalidOperationException was not thrown");
            }
            catch (InvalidOperationException ex)
            {
                ex.Message.Should().Contain("Schedule conflict detected");
            }
        }

        [Test]
        public async Task AddScheduleAsync_ShouldThrowException_WhenDriverConflictExists()
        {
            // Arrange
            var vehicle1 = new Bus { BusNumber = "001", Make = "Test", Model = "Bus", SeatingCapacity = 50 };
            var vehicle2 = new Bus { BusNumber = "002", Make = "Test", Model = "Bus", SeatingCapacity = 60 };
            var driver = new Driver { DriverName = "John Doe", LicenseNumber = "12345" };
            var route = new Route { RouteName = "Route 1", Date = DateTime.Today, Description = "Test Route" };

            _context.Vehicles.AddRange(vehicle1, vehicle2);
            _context.Drivers.Add(driver);
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            // Add existing activity
            var existingActivity = new Activity
            {
                ActivityDate = DateTime.Today,
                StartTime = TimeSpan.FromHours(8),
                EndTime = TimeSpan.FromHours(10),
                VehicleId = vehicle1.VehicleId,
                DriverId = driver.DriverId,
                RouteId = route.RouteId
            };
            _context.Activities.Add(existingActivity);
            await _context.SaveChangesAsync();

            // Try to add conflicting activity (same driver, overlapping time)
            var conflictingActivity = new Activity
            {
                ActivityDate = DateTime.Today,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(11),
                VehicleId = vehicle2.VehicleId,
                DriverId = driver.DriverId,
                RouteId = route.RouteId
            };

            // Act & Assert
            try
            {
                await _scheduleService.AddScheduleAsync(conflictingActivity);
                Assert.Fail("Expected InvalidOperationException was not thrown");
            }
            catch (InvalidOperationException ex)
            {
                ex.Message.Should().Contain("Schedule conflict detected");
            }
        }

        #endregion

        #region UpdateScheduleAsync Tests

        [Test]
        public async Task UpdateScheduleAsync_ShouldUpdateSchedule_WhenNoConflictExists()
        {
            // Arrange
            var vehicle = new Bus { BusNumber = "001", Make = "Test", Model = "Bus", SeatingCapacity = 50 };
            var driver = new Driver { DriverName = "John Doe", LicenseNumber = "12345" };
            var route = new Route { RouteName = "Route 1", Date = DateTime.Today, Description = "Test Route" };

            _context.Vehicles.Add(vehicle);
            _context.Drivers.Add(driver);
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            var activity = new Activity
            {
                ActivityDate = DateTime.Today,
                StartTime = TimeSpan.FromHours(8),
                EndTime = TimeSpan.FromHours(9),
                VehicleId = vehicle.VehicleId,
                DriverId = driver.DriverId,
                RouteId = route.RouteId
            };
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            // Modify the activity
            activity.StartTime = TimeSpan.FromHours(10);
            activity.EndTime = TimeSpan.FromHours(11);

            // Act
            var result = await _scheduleService.UpdateScheduleAsync(activity);

            // Assert
            result.Should().NotBeNull();
            result.StartTime.Should().Be(TimeSpan.FromHours(10));
            result.EndTime.Should().Be(TimeSpan.FromHours(11));
        }

        [Test]
        public async Task UpdateScheduleAsync_ShouldThrowException_WhenConflictExists()
        {
            // Arrange
            var vehicle = new Bus { BusNumber = "001", Make = "Test", Model = "Bus", SeatingCapacity = 50 };
            var driver = new Driver { DriverName = "John Doe", LicenseNumber = "12345" };
            var route = new Route { RouteName = "Route 1", Date = DateTime.Today, Description = "Test Route" };

            _context.Vehicles.Add(vehicle);
            _context.Drivers.Add(driver);
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            var activity1 = new Activity
            {
                ActivityDate = DateTime.Today,
                StartTime = TimeSpan.FromHours(8),
                EndTime = TimeSpan.FromHours(9),
                VehicleId = vehicle.VehicleId,
                DriverId = driver.DriverId,
                RouteId = route.RouteId
            };

            var activity2 = new Activity
            {
                ActivityDate = DateTime.Today,
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(11),
                VehicleId = vehicle.VehicleId,
                DriverId = driver.DriverId,
                RouteId = route.RouteId
            };

            _context.Activities.AddRange(activity1, activity2);
            await _context.SaveChangesAsync();

            // Try to update activity2 to conflict with activity1
            activity2.StartTime = TimeSpan.FromHours(8.5);
            activity2.EndTime = TimeSpan.FromHours(9.5);

            // Act & Assert
            try
            {
                await _scheduleService.UpdateScheduleAsync(activity2);
                Assert.Fail("Expected InvalidOperationException was not thrown");
            }
            catch (InvalidOperationException ex)
            {
                ex.Message.Should().Contain("Schedule conflict detected");
            }
        }

        #endregion

        #region DeleteScheduleAsync Tests

        [Test]
        public async Task DeleteScheduleAsync_ShouldReturnTrue_WhenScheduleExists()
        {
            // Arrange
            var vehicle = new Bus { BusNumber = "001", Make = "Test", Model = "Bus", SeatingCapacity = 50 };
            var driver = new Driver { DriverName = "John Doe", LicenseNumber = "12345" };
            var route = new Route { RouteName = "Route 1", Date = DateTime.Today, Description = "Test Route" };

            _context.Vehicles.Add(vehicle);
            _context.Drivers.Add(driver);
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            var activity = new Activity
            {
                ActivityDate = DateTime.Today,
                StartTime = TimeSpan.FromHours(8),
                EndTime = TimeSpan.FromHours(9),
                VehicleId = vehicle.VehicleId,
                DriverId = driver.DriverId,
                RouteId = route.RouteId
            };
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            // Act
            var result = await _scheduleService.DeleteScheduleAsync(activity.ActivityId);

            // Assert
            result.Should().BeTrue();

            var deletedActivity = await _context.Activities.FindAsync(activity.ActivityId);
            deletedActivity.Should().BeNull();
        }

        [Test]
        public async Task DeleteScheduleAsync_ShouldReturnFalse_WhenScheduleDoesNotExist()
        {
            // Act
            var result = await _scheduleService.DeleteScheduleAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region GetSchedulesByVehicleAsync Tests

        [Test]
        public async Task GetSchedulesByVehicleAsync_ShouldReturnSchedules_WhenValidVehicleId()
        {
            // Arrange
            var vehicle = new Bus { BusNumber = "001", Make = "Test", Model = "Bus", SeatingCapacity = 50 };
            var driver = new Driver { DriverName = "John Doe", LicenseNumber = "12345" };
            var route = new Route { RouteName = "Route 1", Date = DateTime.Today, Description = "Test Route" };

            _context.Vehicles.Add(vehicle);
            _context.Drivers.Add(driver);
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            var activity = new Activity
            {
                ActivityDate = DateTime.Today,
                StartTime = TimeSpan.FromHours(8),
                EndTime = TimeSpan.FromHours(9),
                VehicleId = vehicle.VehicleId,
                DriverId = driver.DriverId,
                RouteId = route.RouteId
            };
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            // Act
            var result = await _scheduleService.GetSchedulesByVehicleAsync(vehicle.VehicleId.ToString());

            // Assert
            result.Should().HaveCount(1);
            result.First().VehicleId.Should().Be(vehicle.VehicleId);
        }

        [Test]
        public async Task GetSchedulesByVehicleAsync_ShouldReturnEmptyList_WhenInvalidVehicleId()
        {
            // Act
            var result = await _scheduleService.GetSchedulesByVehicleAsync("invalid");

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetSchedulesByDriverAsync Tests

        [Test]
        public async Task GetSchedulesByDriverAsync_ShouldReturnSchedules_WhenDriverExists()
        {
            // Arrange
            var vehicle = new Bus { BusNumber = "001", Make = "Test", Model = "Bus", SeatingCapacity = 50 };
            var driver = new Driver { DriverName = "John Doe", LicenseNumber = "12345" };
            var route = new Route { RouteName = "Route 1", Date = DateTime.Today, Description = "Test Route" };

            _context.Vehicles.Add(vehicle);
            _context.Drivers.Add(driver);
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            var activity = new Activity
            {
                ActivityDate = DateTime.Today,
                StartTime = TimeSpan.FromHours(8),
                EndTime = TimeSpan.FromHours(9),
                VehicleId = vehicle.VehicleId,
                DriverId = driver.DriverId,
                RouteId = route.RouteId
            };
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            // Act
            var result = await _scheduleService.GetSchedulesByDriverAsync("John Doe");

            // Assert
            result.Should().HaveCount(1);
            var firstResult = result.FirstOrDefault();
            Assert.That(firstResult, Is.Not.Null);
            Assert.That(firstResult!.Driver, Is.Not.Null);
            Assert.That(firstResult.Driver!.DriverName, Is.EqualTo("John Doe"));
        }

        [Test]
        public async Task GetSchedulesByDriverAsync_ShouldReturnEmptyList_WhenDriverDoesNotExist()
        {
            // Act
            var result = await _scheduleService.GetSchedulesByDriverAsync("Nonexistent Driver");

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region ValidateScheduleConflictAsync Tests - CRITICAL BUSINESS LOGIC

        [Test]
        public async Task ValidateScheduleConflictAsync_ShouldReturnFalse_WhenNoConflictExists()
        {
            // Arrange
            var vehicle = new Bus { BusNumber = "001", Make = "Test", Model = "Bus", SeatingCapacity = 50 };
            var driver = new Driver { DriverName = "John Doe", LicenseNumber = "12345" };
            var route = new Route { RouteName = "Route 1", Date = DateTime.Today, Description = "Test Route" };

            _context.Vehicles.Add(vehicle);
            _context.Drivers.Add(driver);
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

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
            result.Should().BeFalse();
        }

        [Test]
        public async Task ValidateScheduleConflictAsync_ShouldReturnTrue_WhenVehicleConflictExists_StartTimeOverlap()
        {
            // Arrange
            var vehicle = new Bus { BusNumber = "001", Make = "Test", Model = "Bus", SeatingCapacity = 50 };
            var driver1 = new Driver { DriverName = "John Doe", LicenseNumber = "12345" };
            var driver2 = new Driver { DriverName = "Jane Smith", LicenseNumber = "67890" };
            var route = new Route { RouteName = "Route 1", Date = DateTime.Today, Description = "Test Route" };

            _context.Vehicles.Add(vehicle);
            _context.Drivers.AddRange(driver1, driver2);
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

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
            _context.Activities.Add(existingActivity);
            await _context.SaveChangesAsync();

            // Create conflicting activity (same vehicle, overlapping start time)
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
            result.Should().BeTrue();
        }

        [Test]
        public async Task ValidateScheduleConflictAsync_ShouldReturnTrue_WhenVehicleConflictExists_EndTimeOverlap()
        {
            // Arrange
            var vehicle = new Bus { BusNumber = "001", Make = "Test", Model = "Bus", SeatingCapacity = 50 };
            var driver1 = new Driver { DriverName = "John Doe", LicenseNumber = "12345" };
            var driver2 = new Driver { DriverName = "Jane Smith", LicenseNumber = "67890" };
            var route = new Route { RouteName = "Route 1", Date = DateTime.Today, Description = "Test Route" };

            _context.Vehicles.Add(vehicle);
            _context.Drivers.AddRange(driver1, driver2);
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            // Add existing activity
            var existingActivity = new Activity
            {
                ActivityDate = DateTime.Today,
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(12),
                VehicleId = vehicle.VehicleId,
                DriverId = driver1.DriverId,
                RouteId = route.RouteId
            };
            _context.Activities.Add(existingActivity);
            await _context.SaveChangesAsync();

            // Create conflicting activity (same vehicle, overlapping end time)
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
            result.Should().BeTrue();
        }

        [Test]
        public async Task ValidateScheduleConflictAsync_ShouldReturnTrue_WhenVehicleConflictExists_CompleteOverlap()
        {
            // Arrange
            var vehicle = new Bus { BusNumber = "001", Make = "Test", Model = "Bus", SeatingCapacity = 50 };
            var driver1 = new Driver { DriverName = "John Doe", LicenseNumber = "12345" };
            var driver2 = new Driver { DriverName = "Jane Smith", LicenseNumber = "67890" };
            var route = new Route { RouteName = "Route 1", Date = DateTime.Today, Description = "Test Route" };

            _context.Vehicles.Add(vehicle);
            _context.Drivers.AddRange(driver1, driver2);
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            // Add existing activity
            var existingActivity = new Activity
            {
                ActivityDate = DateTime.Today,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(11),
                VehicleId = vehicle.VehicleId,
                DriverId = driver1.DriverId,
                RouteId = route.RouteId
            };
            _context.Activities.Add(existingActivity);
            await _context.SaveChangesAsync();

            // Create conflicting activity (same vehicle, complete overlap)
            var conflictingActivity = new Activity
            {
                ActivityDate = DateTime.Today,
                StartTime = TimeSpan.FromHours(8),
                EndTime = TimeSpan.FromHours(12),
                VehicleId = vehicle.VehicleId,
                DriverId = driver2.DriverId,
                RouteId = route.RouteId
            };

            // Act
            var result = await _scheduleService.ValidateScheduleConflictAsync(conflictingActivity);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task ValidateScheduleConflictAsync_ShouldReturnTrue_WhenDriverConflictExists()
        {
            // Arrange
            var vehicle1 = new Bus { BusNumber = "001", Make = "Test", Model = "Bus", SeatingCapacity = 50 };
            var vehicle2 = new Bus { BusNumber = "002", Make = "Test", Model = "Bus", SeatingCapacity = 60 };
            var driver = new Driver { DriverName = "John Doe", LicenseNumber = "12345" };
            var route = new Route { RouteName = "Route 1", Date = DateTime.Today, Description = "Test Route" };

            _context.Vehicles.AddRange(vehicle1, vehicle2);
            _context.Drivers.Add(driver);
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            // Add existing activity
            var existingActivity = new Activity
            {
                ActivityDate = DateTime.Today,
                StartTime = TimeSpan.FromHours(8),
                EndTime = TimeSpan.FromHours(10),
                VehicleId = vehicle1.VehicleId,
                DriverId = driver.DriverId,
                RouteId = route.RouteId
            };
            _context.Activities.Add(existingActivity);
            await _context.SaveChangesAsync();

            // Create conflicting activity (same driver, different vehicle, overlapping time)
            var conflictingActivity = new Activity
            {
                ActivityDate = DateTime.Today,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(11),
                VehicleId = vehicle2.VehicleId,
                DriverId = driver.DriverId,
                RouteId = route.RouteId
            };

            // Act
            var result = await _scheduleService.ValidateScheduleConflictAsync(conflictingActivity);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task ValidateScheduleConflictAsync_ShouldReturnFalse_WhenDifferentDate()
        {
            // Arrange
            var vehicle = new Bus { BusNumber = "001", Make = "Test", Model = "Bus", SeatingCapacity = 50 };
            var driver = new Driver { DriverName = "John Doe", LicenseNumber = "12345" };
            var route = new Route { RouteName = "Route 1", Date = DateTime.Today, Description = "Test Route" };

            _context.Vehicles.Add(vehicle);
            _context.Drivers.Add(driver);
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

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
            _context.Activities.Add(existingActivity);
            await _context.SaveChangesAsync();

            // Create activity on different date (same vehicle, driver, overlapping time)
            var nonConflictingActivity = new Activity
            {
                ActivityDate = DateTime.Today.AddDays(1),
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(11),
                VehicleId = vehicle.VehicleId,
                DriverId = driver.DriverId,
                RouteId = route.RouteId
            };

            // Act
            var result = await _scheduleService.ValidateScheduleConflictAsync(nonConflictingActivity);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task ValidateScheduleConflictAsync_ShouldReturnFalse_WhenNoTimeOverlap()
        {
            // Arrange
            var vehicle = new Bus { BusNumber = "001", Make = "Test", Model = "Bus", SeatingCapacity = 50 };
            var driver = new Driver { DriverName = "John Doe", LicenseNumber = "12345" };
            var route = new Route { RouteName = "Route 1", Date = DateTime.Today, Description = "Test Route" };

            _context.Vehicles.Add(vehicle);
            _context.Drivers.Add(driver);
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

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
            _context.Activities.Add(existingActivity);
            await _context.SaveChangesAsync();

            // Create activity with no time overlap (same vehicle, driver, but different time)
            var nonConflictingActivity = new Activity
            {
                ActivityDate = DateTime.Today,
                StartTime = TimeSpan.FromHours(11),
                EndTime = TimeSpan.FromHours(12),
                VehicleId = vehicle.VehicleId,
                DriverId = driver.DriverId,
                RouteId = route.RouteId
            };

            // Act
            var result = await _scheduleService.ValidateScheduleConflictAsync(nonConflictingActivity);

            // Assert
            result.Should().BeFalse();
        }

        #endregion
    }
}
