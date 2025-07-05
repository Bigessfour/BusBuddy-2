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
    /// Enhanced FuelService tests using SQLite in-memory database
    /// and the consolidated test base for consistent testing.
    /// </summary>
    [TestFixture]
    [NonParallelizable] // Database tests need to run sequentially

    public class FuelServiceConsolidatedTests : ConsolidatedTestBase
    {
        private IFuelService _fuelService = null!;
        private Bus _testBus = null!;
        private List<Fuel> _testFuelRecords = null!;

        [SetUp]
        public async Task SetUp()
        {
            SetupTestDatabase();
            _fuelService = GetService<IFuelService>();

            // Create test bus with proper data
            _testBus = CreateTestBus("FUEL001", "FUEL001234567890A");

            DbContext.Vehicles.Add(_testBus);
            await DbContext.SaveChangesAsync();

            // Create test fuel records
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


        #region GetAllFuelRecordsAsync Tests

        [Test]
        public async Task GetAllFuelRecordsAsync_WithNoRecords_ShouldReturnEmptyList()
        {
            // Act
            var result = await _fuelService.GetAllFuelRecordsAsync();

            // Assert - Using assertion scope for better error reporting
            using (new AssertionScope())
            {
                result.Should().NotBeNull("service should never return null");
                result.Should().BeEmpty("no records should exist in clean database");
            }
        }

        [Test]
        public async Task GetAllFuelRecordsAsync_WithRecords_ShouldReturnOrderedByDate()
        {
            // Arrange
            DbContext.FuelRecords.AddRange(_testFuelRecords);
            await DbContext.SaveChangesAsync();
            ClearChangeTracker();

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

                // Verify correct ordering
                result.First().FuelDate.Should().BeCloseTo(DateTime.Today.AddDays(-5), TimeSpan.FromHours(1));
                result.Last().FuelDate.Should().BeCloseTo(DateTime.Today.AddDays(-10), TimeSpan.FromHours(1));
            }
        }

        #endregion

        #region CreateFuelRecordAsync Tests

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
                Notes = "Test record"
            };

            // Act
            var result = await _fuelService.CreateFuelRecordAsync(newFuel);

            // Assert - Comprehensive validation
            using (new AssertionScope("Fuel Record Creation"))
            {
                result.Should().NotBeNull("fuel record should be created successfully");
                result.FuelId.Should().BeGreaterThan(0, "new record should have valid auto-generated ID");
                result.Gallons.Should().Be(52.3m, "gallons should match input exactly");
                result.TotalCost.Should().Be(167.36m, "cost should match input exactly");
                result.FuelType.Should().Be("Diesel", "fuel type should match input");
            }

            // Verify in database with fresh context
            ClearChangeTracker();
            var dbRecord = await DbContext.FuelRecords.FindAsync(result.FuelId);

            using (new AssertionScope("Database Persistence"))
            {
                dbRecord.Should().NotBeNull("record should be persisted in database");
                dbRecord!.Gallons.Should().Be(52.3m, "persisted data should match input");
                dbRecord.Notes.Should().Be("Test record", "notes should be persisted correctly");
            }
        }

        [Test]
        public async Task CreateFuelRecordAsync_WithNegativeGallons_ShouldThrowException()
        {
            // Arrange
            var newFuel = new Fuel
            {
                VehicleFueledId = _testBus.VehicleId,
                FuelDate = DateTime.Today,
                Gallons = -10m, // Invalid negative gallons
                TotalCost = 30m,
                FuelType = "Diesel"
            };

            // Act & Assert
            await _fuelService.Invoking(s => s.CreateFuelRecordAsync(newFuel))
                .Should().ThrowAsync<ArgumentException>()
                .WithMessage("*gallons*"); // The exact message depends on your validation
        }

        #endregion

        #region GetFuelRecordsByVehicleAsync Tests

        [Test]
        public async Task GetFuelRecordsByVehicleAsync_WithMultipleVehicles_ShouldFilterCorrectly()
        {
            // Arrange - Setup multiple vehicles
            var anotherBus = CreateTestBus("FUEL002", "FUEL002345678901A");
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
            ClearChangeTracker();

            // Act
            var result = await _fuelService.GetFuelRecordsByVehicleAsync(_testBus.VehicleId);

            // Assert - Enhanced collection validation
            using (new AssertionScope("Vehicle Filtering"))
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

        #region GetTotalFuelCostAsync Tests

        [Test]
        public async Task GetTotalFuelCostAsync_WithValidVehicleId_ShouldReturnTotalCost()
        {
            // Arrange
            DbContext.FuelRecords.AddRange(_testFuelRecords);
            await DbContext.SaveChangesAsync();
            ClearChangeTracker();

            var expectedTotal = 151.50m + 140.12m; // Sum of both test record costs

            // Act
            var result = await _fuelService.GetTotalFuelCostAsync(_testBus.VehicleId);

            // Assert
            result.Should().Be(expectedTotal, "should calculate the correct total cost");
        }

        [Test]
        public async Task GetTotalFuelCostAsync_WithDateRange_ShouldReturnFilteredTotal()
        {
            // Arrange
            DbContext.FuelRecords.AddRange(_testFuelRecords);
            await DbContext.SaveChangesAsync();
            ClearChangeTracker();

            var startDate = DateTime.Today.AddDays(-6);
            var endDate = DateTime.Today;

            var expectedTotal = 140.12m; // Only the more recent record

            // Act
            var result = await _fuelService.GetTotalFuelCostAsync(_testBus.VehicleId, startDate, endDate);

            // Assert
            result.Should().Be(expectedTotal, "should sum costs for records in date range only");
        }

        #endregion

        #region UI Dialog Tests

        [Test]
        [STAThread] // Required for UI tests
        public async Task CreateFuelRecordAsync_WithInvalidData_ShouldTriggerErrorDialog()
        {
            // Arrange
            StartDialogCapture();

            // Act - Try to create with null (would normally trigger dialog in real UI)
            try
            {
                await _fuelService.CreateFuelRecordAsync(null!);
            }
            catch
            {
                // Expected exception, we're just testing dialog capture
            }

            // Assert
            var report = StopDialogCaptureAndGetReport();
            report.Should().Contain("dialog", "an error dialog should be triggered for invalid data");
        }

        #endregion

        [TearDown]
        public void TearDown()
        {
            TearDownTestDatabase();
        }
    }
}

