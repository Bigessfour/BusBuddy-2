using NUnit.Framework;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Bus_Buddy.Services;
using Bus_Buddy.Models;
using BusBuddy.Tests.Infrastructure;

namespace BusBuddy.Tests.UnitTests.Services
{
    /// <summary>
    /// Enhanced FuelService tests using SQLite in-memory per Microsoft EF Core recommendations
    /// 
    /// IMPROVEMENTS IMPLEMENTED:
    /// 1. SQLite in-memory instead of EF InMemory (Microsoft recommended)
    /// 2. Enhanced FluentAssertions with assertion scopes
    /// 3. Better test isolation and entity tracking
    /// 4. Comprehensive error reporting
    /// 
    /// This should resolve our Entity Framework tracking conflicts
    /// </summary>
    [TestFixture]
    [NonParallelizable] // Database tests need to run sequentially

    public class FuelServiceEnhancedTests : TestBase
    {
        private IFuelService _fuelService = null!;
        private Bus _testBus = null!;
        private List<Fuel> _testFuelRecords = null!;

        [SetUp]
        public async Task SetUp()
        {
            SetupTestDatabase();
            _fuelService = GetService<IFuelService>();

            // Create test bus (SQLite will handle auto-increment properly)
            _testBus = new Bus
            {
                BusNumber = "TEST001",
                Make = "Blue Bird",
                Model = "Vision",
                Year = 2020,
                MilesPerGallon = 8.5m,
                Status = "Active"
            };

            DbContext.Vehicles.Add(_testBus);
            await DbContext.SaveChangesAsync();
            ClearChangeTracker();

            // Create fresh test fuel records for each test
            _testFuelRecords = new List<Fuel>
            {
                new Fuel
                {
                    VehicleFueledId = _testBus.VehicleId,
                    FuelDate = DateTime.Today.AddDays(-10),
                    Gallons = 50.5m,
                    TotalCost = 151.50m,
                    PricePerGallon = 3.00m,
                    VehicleOdometerReading = 15000,
                    FuelType = "Diesel",
                    FuelLocation = "Shell Station",
                    Notes = "Regular fill-up"
                },
                new Fuel
                {
                    VehicleFueledId = _testBus.VehicleId,
                    FuelDate = DateTime.Today.AddDays(-5),
                    Gallons = 45.2m,
                    TotalCost = 140.12m,
                    PricePerGallon = 3.10m,
                    VehicleOdometerReading = 15300,
                    FuelType = "Diesel",
                    FuelLocation = "Exxon Station",
                    Notes = "Mid-week fill-up"
                }
            };
        }


        #region Enhanced Tests with FluentAssertions Best Practices

        [Test]
        public async Task GetAllFuelRecordsAsync_WithRecords_ShouldReturnOrderedByDate()
        {
            // Arrange
            DbContext.FuelRecords.AddRange(_testFuelRecords);
            await DbContext.SaveChangesAsync();

            // Act
            var result = await _fuelService.GetAllFuelRecordsAsync();

            // Assert - Using assertion scope for comprehensive error reporting
            using (new AssertionScope())
            {
                result.Should().NotBeNull("service should never return null");
                result.Should().HaveCount(2, "both test records should be returned");
                result.Should().BeInDescendingOrder(f => f.FuelDate, "records should be ordered by date descending");

                // Enhanced collection assertions
                result.Should().AllSatisfy(fuel =>
                {
                    fuel.VehicleFueledId.Should().Be(_testBus.VehicleId, "all records should belong to test vehicle");
                    fuel.Gallons.Should().BeGreaterThan(0, "gallons should be positive");
                    fuel.FuelType.Should().NotBeNullOrEmpty("fuel type should be specified");
                });

                // Fluent chaining for complex object validation
                result.First().Should().NotBeNull()
                    .And.Subject.As<Fuel>().FuelDate.Should().BeCloseTo(DateTime.Today.AddDays(-5), TimeSpan.FromHours(1));
            }
        }

        [Test]
        public async Task CreateFuelRecordAsync_WithValidRecord_ShouldCreateAndReturn()
        {
            // Arrange
            var newFuel = new Fuel
            {
                VehicleFueledId = _testBus.VehicleId,
                FuelDate = DateTime.Today,
                Gallons = 52.3m,
                TotalCost = 167.36m,
                PricePerGallon = 3.20m,
                VehicleOdometerReading = 16000,
                FuelType = "Diesel",
                FuelLocation = "Test Station",
                Notes = "Enhanced test record"
            };

            // Act
            var result = await _fuelService.CreateFuelRecordAsync(newFuel);

            // Assert - Enhanced assertion scope with business context
            using (new AssertionScope("CreateFuelRecordAsync Validation"))
            {
                result.Should().NotBeNull("fuel record creation should succeed");
                result.FuelId.Should().BeGreaterThan(0, "SQLite should assign valid auto-increment ID");
                result.Gallons.Should().Be(52.3m, "gallons should match input exactly");
                result.TotalCost.Should().Be(167.36m, "cost should be preserved exactly");
                result.VehicleFueledId.Should().Be(_testBus.VehicleId, "vehicle association should be maintained");
                result.FuelLocation.Should().Be("Test Station", "location should be preserved");
            }

            // Verify persistence with fresh context
            ClearChangeTracker();
            var dbRecord = await DbContext.FuelRecords.FindAsync(result.FuelId);
            using (new AssertionScope("Database Persistence Validation"))
            {
                dbRecord.Should().NotBeNull("record should be persisted in SQLite database");
                dbRecord!.Gallons.Should().Be(52.3m, "persisted gallons should match");
                dbRecord.Notes.Should().Be("Enhanced test record", "persisted notes should match");
            }
        }

        [Test]
        public async Task GetFuelRecordsByVehicleAsync_WithMultipleVehicles_ShouldFilterCorrectly()
        {
            // Arrange - Test data isolation
            var anotherBus = new Bus
            {
                BusNumber = "TEST002",
                Make = "Thomas",
                Model = "C2",
                Year = 2019,
                Status = "Active",
                VINNumber = "FUEL002345678901A", // Exactly 17 characters
                LicenseNumber = "FUEL002",
                SeatingCapacity = 48,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow
            };
            DbContext.Vehicles.Add(anotherBus);
            await DbContext.SaveChangesAsync();

            var fuelForAnotherBus = new Fuel
            {
                VehicleFueledId = anotherBus.VehicleId,
                FuelDate = DateTime.Today.AddDays(-3),
                Gallons = 40.0m,
                TotalCost = 120.0m,
                FuelType = "Diesel"
            };

            DbContext.FuelRecords.AddRange(_testFuelRecords);
            DbContext.FuelRecords.Add(fuelForAnotherBus);
            await DbContext.SaveChangesAsync();

            // Act
            var result = await _fuelService.GetFuelRecordsByVehicleAsync(_testBus.VehicleId);

            // Assert - Enhanced collection validation
            using (new AssertionScope("Vehicle Filtering Validation"))
            {
                result.Should().NotBeNull("service should never return null");
                result.Should().HaveCount(2, "should return only records for specified vehicle");
                result.Should().OnlyContain(f => f.VehicleFueledId == _testBus.VehicleId,
                    "all returned records must belong to the requested vehicle");
                result.Should().BeInDescendingOrder(f => f.FuelDate,
                    "records should be ordered by date descending");

                // Verify no cross-contamination from other vehicle
                result.Should().NotContain(f => f.VehicleFueledId == anotherBus.VehicleId,
                    "should not include records from other vehicles");
            }
        }

        #endregion

        [TearDown]
        public void TearDown()
        {
            TearDownTestDatabase();
        }
    }
}

