using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bus_Buddy.Data.Repositories;
using Bus_Buddy.Models;

using FluentAssertions;
using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Bus_Buddy.Data;
using Bus_Buddy.Services;
using Microsoft.Extensions.Logging;
using Moq;


/*
 * =============================================================
 * EF CORE IN-MEMORY TEST ISOLATION & REPOSITORY TEST TEMPLATE
 * =============================================================
 *
 * Use this pattern for all repository/service tests that require
 * a fresh, isolated in-memory database context per test.
 *
 * 1. Add these usings:
 *    using Microsoft.EntityFrameworkCore;
 *    using Microsoft.Extensions.Logging;
 *    using Moq;
 *
 * 2. In each test, create a new context and mock dependencies:
 *
 *    var options = new DbContextOptionsBuilder<BusBuddyDbContext>()
 *        .UseInMemoryDatabase(Guid.NewGuid().ToString())
 *        .EnableSensitiveDataLogging()
 *        .EnableDetailedErrors()
 *        .Options;
 *    using var context = new BusBuddyDbContext(options);
 *    var loggerMock = new Mock<ILogger<UserContextService>>();
 *    var userContext = new UserContextService(loggerMock.Object);
 *
 * 3. Seed test data:
 *    context.Vehicles.AddRange(testData);
 *    context.SaveChanges();
 *
 * 4. Instantiate repository/service and run assertions:
 *    var repo = new BusRepository(context, userContext);
 *    var result = await repo.MethodUnderTest(...);
 *    result.Should()...;
 *
 * 5. Dispose context at the end of each test (using 'using').
 *
 * 6. Do NOT use a shared base class property for DbContext in these tests.
 *
 * 7. See: https://learn.microsoft.com/en-us/ef/core/testing/testing-with-the-in-memory-provider
 *
 * TEMPLATE FOR FUTURE TESTS:
 *
 * [Test]
 * public async Task MyRepositoryMethod_WorksAsExpected()
 * {
 *     var options = new DbContextOptionsBuilder<BusBuddyDbContext>()
 *         .UseInMemoryDatabase(Guid.NewGuid().ToString())
 *         .EnableSensitiveDataLogging()
 *         .EnableDetailedErrors()
 *         .Options;
 *     using var context = new BusBuddyDbContext(options);
 *     var loggerMock = new Mock<ILogger<UserContextService>>();
 *     var userContext = new UserContextService(loggerMock.Object);
 *     context.Vehicles.AddRange(...); // or other DbSet
 *     context.SaveChanges();
 *     var repo = new BusRepository(context, userContext);
 *     var result = await repo.MethodUnderTest(...);
 *     result.Should()...;
 * }
 *
 * This ensures reliable, isolated, and repeatable tests for all repository/service coverage.
 */

namespace BusBuddy.Tests.UnitTests.Repositories
{
    [TestFixture]
    public class BusRepositoryTests
    {
        private List<Bus> GetSeedData() => new List<Bus>
        {
            new Bus { VehicleId = 1, BusNumber = "BUS001", Status = "Active", FleetType = "A", Year = 2020, VINNumber = "VIN1", LicenseNumber = "LIC1", SeatingCapacity = 50, GPSTracking = true, SpecialEquipment = "Lift", DateLastInspection = DateTime.Today.AddDays(-400), NextMaintenanceDue = DateTime.Today.AddDays(10), InsuranceExpiryDate = DateTime.Today.AddDays(20), Make = "Ford", PurchasePrice = 100000 },
            new Bus { VehicleId = 2, BusNumber = "BUS002", Status = "Inactive", FleetType = "B", Year = 2018, VINNumber = "VIN2", LicenseNumber = "LIC2", SeatingCapacity = 40, GPSTracking = false, SpecialEquipment = null, DateLastInspection = DateTime.Today.AddDays(-200), NextMaintenanceDue = DateTime.Today.AddDays(40), InsuranceExpiryDate = DateTime.Today.AddDays(-5), Make = "Chevy", PurchasePrice = 80000 },
            new Bus { VehicleId = 3, BusNumber = "BUS003", Status = "Active", FleetType = "A", Year = 2022, VINNumber = "VIN3", LicenseNumber = "LIC3", SeatingCapacity = 60, GPSTracking = true, SpecialEquipment = "Ramp", DateLastInspection = null, NextMaintenanceDue = null, InsuranceExpiryDate = null, Make = "BlueBird", PurchasePrice = 120000 }
        };

        private BusBuddyDbContext CreateInMemoryDbContext()
        {
            var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<BusBuddyDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .Options;
            return new BusBuddyDbContext(options);
        }

        [Test]
        public async Task GetActiveVehiclesAsync_ReturnsOnlyActive()
        {
            using var context = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<UserContextService>>();
            var userContext = new UserContextService(loggerMock.Object);
            context.Vehicles.AddRange(GetSeedData());
            context.SaveChanges();
            var repository = new BusRepository(context, userContext);
            var result = await repository.GetActiveVehiclesAsync();
            result.Should().OnlyContain(b => b.Status == "Active");
        }

        [Test]
        public async Task GetVehiclesByStatusAsync_ReturnsCorrectStatus()
        {
            using var context = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<UserContextService>>();
            var userContext = new UserContextService(loggerMock.Object);
            context.Vehicles.AddRange(GetSeedData());
            context.SaveChanges();
            var repository = new BusRepository(context, userContext);
            var result = await repository.GetVehiclesByStatusAsync("Inactive");
            result.Should().OnlyContain(b => b.Status == "Inactive");
        }

        [Test]
        public async Task GetVehicleByBusNumberAsync_ReturnsCorrectBus()
        {
            using var context = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<UserContextService>>();
            var userContext = new UserContextService(loggerMock.Object);
            context.Vehicles.AddRange(GetSeedData());
            context.SaveChanges();
            var repository = new BusRepository(context, userContext);
            var result = await repository.GetVehicleByBusNumberAsync("BUS002");
            result.Should().NotBeNull();
            result!.BusNumber.Should().Be("BUS002");
        }

        [Test]
        public async Task GetVehiclesDueForInspectionAsync_ReturnsDue()
        {
            using var context = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<UserContextService>>();
            var userContext = new UserContextService(loggerMock.Object);
            context.Vehicles.AddRange(GetSeedData());
            context.SaveChanges();
            var repository = new BusRepository(context, userContext);
            var result = await repository.GetVehiclesDueForInspectionAsync();
            result.Should().Contain(b => b.DateLastInspection == null || b.DateLastInspection <= DateTime.Today.AddDays(-335));
        }

        [Test]
        public async Task GetVehiclesWithGPSAsync_ReturnsOnlyWithGPS()
        {
            using var context = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<UserContextService>>();
            var userContext = new UserContextService(loggerMock.Object);
            context.Vehicles.AddRange(GetSeedData());
            context.SaveChanges();
            var repository = new BusRepository(context, userContext);
            var result = await repository.GetVehiclesWithGPSAsync();
            result.Should().OnlyContain(b => b.GPSTracking);
        }

        [Test]
        public async Task GetTotalVehicleCountAsync_ReturnsCorrectCount()
        {
            using var context = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<UserContextService>>();
            var userContext = new UserContextService(loggerMock.Object);
            context.Vehicles.AddRange(GetSeedData());
            context.SaveChanges();
            var repository = new BusRepository(context, userContext);
            var count = await repository.GetTotalVehicleCountAsync();
            count.Should().Be(GetSeedData().Count);
        }
    }
}
