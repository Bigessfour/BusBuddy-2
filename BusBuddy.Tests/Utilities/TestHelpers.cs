using NUnit.Framework;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using BusBuddy.Core.Data;
using BusBuddy.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BusBuddy.Tests.Utilities
{
    /// <summary>
    /// Common utilities and helpers for NUnit + FluentAssertions testing
    /// Provides standardized test data creation and database setup
    /// </summary>
    public static class TestHelpers
    {
        /// <summary>
        /// Creates an in-memory Entity Framework context for testing
        /// </summary>
        /// <param name="dbName">Optional database name (generates GUID if not provided)</param>
        /// <returns>Configured test database context</returns>
        public static BusBuddyDbContext CreateInMemoryContext(string? dbName = null)
        {
            var options = new DbContextOptionsBuilder<BusBuddyDbContext>()
                .UseInMemoryDatabase(dbName ?? $"TestDb_{Guid.NewGuid()}")
                .EnableSensitiveDataLogging()
                .Options;

            var context = new BusBuddyDbContext(options);

            // Ensure database is created
            context.Database.EnsureCreated();

            return context;
        }

        /// <summary>
        /// Seeds test context with standard test data for consistent testing
        /// </summary>
        /// <param name="context">The database context to seed</param>
        public static void SeedTestData(BusBuddyDbContext context)
        {
            // Clear existing data
            context.Activities.RemoveRange(context.Activities);
            context.Drivers.RemoveRange(context.Drivers);
            context.Vehicles.RemoveRange(context.Vehicles);

            // Add test drivers
            var drivers = CreateTestDrivers();
            context.Drivers.AddRange(drivers);

            // Add test vehicles
            var vehicles = CreateTestVehicles();
            // Convert vehicles to buses for the context
            var buses = vehicles.Select(v => new Bus
            {
                BusNumber = v.BusNumber,
                Make = v.Make,
                Model = v.Model,
                SeatingCapacity = v.Capacity,
                VINNumber = v.PlateNumber,
                Status = v.IsActive ? "Active" : "Inactive"
            }).ToList();
            context.Vehicles.AddRange(buses);

            // Add test activities
            var activities = CreateTestActivities(drivers, vehicles);
            context.ActivitySchedule.AddRange(activities);

            context.SaveChanges();
        }

        /// <summary>
        /// Creates standard test drivers for testing scenarios
        /// </summary>
        public static List<Driver> CreateTestDrivers()
        {
            return new List<Driver>
            {
                new Driver
                {
                    DriverId = 1,
                    DriverName = "John Smith", // Corrected property name
                    LicenseNumber = "DL123456",
                    DriverEmail = "john.smith@busbuddy.com",
                    DriverPhone = "555-0101", // Corrected property name
                    HireDate = DateTime.Now.AddYears(-2),
                    Status = "Active" // Corrected property name (bool -> string)
                },
                new Driver
                {
                    DriverId = 2,
                    DriverName = "Sarah Johnson", // Corrected property name
                    LicenseNumber = "DL789012",
                    DriverEmail = "sarah.johnson@busbuddy.com",
                    DriverPhone = "555-0102", // Corrected property name
                    HireDate = DateTime.Now.AddYears(-1),
                    Status = "Active" // Corrected property name (bool -> string)
                },
                new Driver
                {
                    DriverId = 3,
                    DriverName = "Mike Wilson", // Corrected property name
                    LicenseNumber = "DL345678",
                    DriverEmail = "mike.wilson@busbuddy.com",
                    DriverPhone = "555-0103", // Corrected property name
                    HireDate = DateTime.Now.AddMonths(-6),
                    Status = "Inactive" // Corrected property name (bool -> string)
                }
            };
        }

        /// <summary>
        /// Creates standard test vehicles for testing scenarios
        /// </summary>
        public static List<Vehicle> CreateTestVehicles()
        {
            return new List<Vehicle>
            {
                new Vehicle
                {
                    Id = 1,
                    PlateNumber = "BUS001",
                    Model = "Blue Bird Vision",
                    Make = "Blue Bird",
                    Capacity = 72
                },
                new Vehicle
                {
                    Id = 2,
                    PlateNumber = "BUS002",
                    Model = "Thomas Built C2",
                    Make = "Thomas",
                    Capacity = 90
                },
                new Vehicle
                {
                    Id = 3,
                    PlateNumber = "BUS003",
                    Model = "IC Bus CE Series",
                    Make = "IC Bus",
                    Capacity = 84
                }
            };
        }

        /// <summary>
        /// Creates test activities linking drivers and vehicles
        /// </summary>
        public static List<ActivitySchedule> CreateTestActivities(List<Driver> drivers, List<Vehicle> vehicles)
        {
            return new List<ActivitySchedule>
            {
                new ActivitySchedule
                {
                    ActivityScheduleId = 1,
                    ScheduledDate = DateTime.Today,
                    TripType = "Activity Trip",
                    ScheduledVehicleId = vehicles[0].Id,
                    ScheduledDestination = "Elementary School",
                    ScheduledLeaveTime = new TimeSpan(7, 0, 0),
                    ScheduledEventTime = new TimeSpan(9, 0, 0),
                    ScheduledDriverId = drivers[0].DriverId,
                    Status = "Scheduled",
                    RequestedBy = "Principal Smith"
                },
                new ActivitySchedule
                {
                    ActivityScheduleId = 2,
                    ScheduledDate = DateTime.Today,
                    TripType = "Activity Trip",
                    ScheduledVehicleId = vehicles[1].Id,
                    ScheduledDestination = "High School",
                    ScheduledLeaveTime = new TimeSpan(15, 0, 0),
                    ScheduledEventTime = new TimeSpan(17, 0, 0),
                    ScheduledDriverId = drivers[1].DriverId,
                    Status = "Scheduled",
                    RequestedBy = "Principal Johnson"
                },
                new ActivitySchedule
                {
                    ActivityScheduleId = 3,
                    ScheduledDate = DateTime.Today,
                    TripType = "Field Trip",
                    ScheduledVehicleId = vehicles[0].Id,
                    ScheduledDestination = "Science Museum",
                    ScheduledLeaveTime = new TimeSpan(10, 0, 0),
                    ScheduledEventTime = new TimeSpan(14, 0, 0),
                    ScheduledDriverId = drivers[0].DriverId,
                    Status = "Completed",
                    RequestedBy = "Teacher Wilson"
                }
            };
        }

        /// <summary>
        /// Assertion helper for validating driver data
        /// </summary>
        public static void AssertValidDriver(Driver driver, string because = "driver should be valid")
        {
            driver.Should().NotBeNull(because);
            driver.DriverName.Should().NotBeNullOrEmpty($"{because} and should have a name");
            driver.LicenseNumber.Should().NotBeNullOrEmpty($"{because} and should have a license number");
            driver.DriverEmail.Should().NotBeNullOrEmpty($"{because} and should have an email");
            driver.DriverEmail.Should().Contain("@", $"{because} and email should be valid");
        }

        /// <summary>
        /// Assertion helper for validating vehicle data
        /// </summary>
        public static void AssertValidVehicle(Vehicle vehicle, string because = "vehicle should be valid")
        {
            vehicle.Should().NotBeNull(because);
            vehicle.PlateNumber.Should().NotBeNullOrEmpty($"{because} and should have a plate number");
            vehicle.Model.Should().NotBeNullOrEmpty($"{because} and should have a model");
            vehicle.Make.Should().NotBeNullOrEmpty($"{because} and should have a make");
            vehicle.Capacity.Should().BeGreaterThan(0, $"{because} and should have positive capacity");
        }

        /// <summary>
        /// Assertion helper for validating activity data
        /// </summary>
        public static void AssertValidActivity(Activity activity, string because = "activity should be valid")
        {
            activity.Should().NotBeNull(because);
            activity.Destination.Should().NotBeNullOrEmpty($"{because} and should have a destination");
            activity.ActivityType.Should().NotBeNullOrEmpty($"{because} and should have an activity type");
            activity.Status.Should().NotBeNullOrEmpty($"{because} and should have a status");
            activity.LeaveTime.Should().BeLessThan(activity.EventTime, $"{because} and leave time should be before event time");
        }

        /// <summary>
        /// Validates that an ActivitySchedule has all required properties set correctly
        /// </summary>
        public static void AssertValidActivitySchedule(ActivitySchedule activity, string because = "activity should be valid")
        {
            activity.Should().NotBeNull(because);
            activity.ScheduledDestination.Should().NotBeNullOrEmpty($"{because} and should have a destination");
            activity.TripType.Should().NotBeNullOrEmpty($"{because} and should have a trip type");
            activity.Status.Should().NotBeNullOrEmpty($"{because} and should have a status");
            activity.ScheduledLeaveTime.Should().BeLessThan(activity.ScheduledEventTime, $"{because} and leave time should be before event time");
        }

        /// <summary>
        /// Creates a mock logger for testing (avoids real logging in tests)
        /// </summary>
        public static Mock<ILogger<T>> CreateMockLogger<T>()
        {
            return new Mock<ILogger<T>>();
        }

        /// <summary>
        /// Verifies that a mock logger was called with expected log level
        /// </summary>
        public static void VerifyLoggerCalled<T>(Mock<ILogger<T>> mockLogger, LogLevel level, string expectedMessage)
        {
            mockLogger.Verify(
                x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    /// <summary>
    /// Base test fixture providing common setup for Phase 2 tests
    /// </summary>
    public abstract class BaseTestFixture
    {
        protected BusBuddyDbContext? TestContext { get; private set; }

        [SetUp]
        public virtual void SetUp()
        {
            TestContext = TestHelpers.CreateInMemoryContext();
            TestHelpers.SeedTestData(TestContext);
        }

        [TearDown]
        public virtual void TearDown()
        {
            TestContext?.Dispose();
        }
    }
}
