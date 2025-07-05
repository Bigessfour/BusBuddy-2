using NUnit.Framework;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Bus_Buddy.Services;
using Bus_Buddy.Data;
using Bus_Buddy.Models;
using BusBuddy.Tests.Infrastructure;

namespace BusBuddy.Tests.UnitTests.Services
{
    /// <summary>
    /// Tests for FuelService - Fuel record management and calculations
    /// This covers CRUD operations and fuel analytics without database dependencies
    /// </summary>
    [TestFixture]
    [NonParallelizable] // Database tests need to run sequentially

    public class FuelServiceTests : TestBase
    {
        private IFuelService _fuelService = null!;
        private Bus _testBus = null!;
        private List<Fuel> _testFuelRecords = null!;

        [SetUp]
        public async Task SetUp()
        {
            SetupTestDatabase();
            _fuelService = GetService<IFuelService>();

            // Create test bus - let EF Core handle ID assignment completely
            _testBus = new Bus
            {
                BusNumber = "TEST001",
                Make = "Blue Bird",
                Model = "Vision",
                Year = 2020,
                MilesPerGallon = 8.5m,
                Status = "Active",
                LicenseNumber = $"TST{Guid.NewGuid().ToString("N")[..6]}", // Unique license number
                VINNumber = $"VIN{Guid.NewGuid().ToString("N")[..10]}" // Unique VIN
            };

            DbContext.Vehicles.Add(_testBus);
            await DbContext.SaveChangesAsync();
            DetachAllEntities();
            _testFuelRecords = CreateFreshFuelRecords();
        }

        [TearDown]
        public void TearDown()
        {
            TearDownTestDatabase();
        }

        /// <summary>
        /// Create completely fresh fuel record instances for each test
        /// This prevents Entity Framework tracking conflicts
        /// </summary>
        private List<Fuel> CreateFreshFuelRecords()
        {
            return new List<Fuel>
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
            },
            new Fuel
            {
                VehicleFueledId = _testBus.VehicleId,
                FuelDate = DateTime.Today.AddDays(-1),
                Gallons = 48.0m,
                TotalCost = 153.60m,
                PricePerGallon = 3.20m,
                VehicleOdometerReading = 15600,
                FuelType = "Diesel",
                FuelLocation = "BP Station",
                Notes = "Recent fill-up"
            }
        };
        }

        #region GetAllFuelRecordsAsync Tests

        [Test]
        public async Task GetAllFuelRecordsAsync_WithNoRecords_ShouldReturnEmptyList()
        {
            // Act
            var result = await _fuelService.GetAllFuelRecordsAsync();

            // Assert
            using (new FluentAssertions.Execution.AssertionScope())
            {
                result.Should().NotBeNull("service should never return null");
                result.Should().BeEmpty("no records should exist in clean database");
            }
        }

        [Test]
        public async Task GetAllFuelRecordsAsync_WithRecords_ShouldReturnOrderedByDate()
        {
            // Arrange - Use fresh fuel records to avoid tracking conflicts
            var freshRecords = CreateFreshFuelRecords();
            DbContext.FuelRecords.AddRange(freshRecords);
            await DbContext.SaveChangesAsync();
            DetachAllEntities(); // Critical: Prevent tracking conflicts

            // Act
            var result = await _fuelService.GetAllFuelRecordsAsync();

            // Assert - Enhanced with assertion scope
            using (new FluentAssertions.Execution.AssertionScope())
            {
                result.Should().NotBeNull("service should never return null");
                result.Should().HaveCount(3, "all test records should be returned");
                result.Should().BeInDescendingOrder(f => f.FuelDate, "records should be ordered by date descending");
                result.First().FuelDate.Should().BeCloseTo(DateTime.Today.AddDays(-1), TimeSpan.FromHours(1), "most recent record should be first");
                result.Last().FuelDate.Should().BeCloseTo(DateTime.Today.AddDays(-10), TimeSpan.FromHours(1), "oldest record should be last");
            }
        }

        [Test]
        public async Task GetAllFuelRecordsAsync_ShouldIncludeVehicleData()
        {
            // Arrange - Use fresh entities
            var freshRecords = CreateFreshFuelRecords();
            DbContext.FuelRecords.AddRange(freshRecords);
            await DbContext.SaveChangesAsync();
            DetachAllEntities();

            // Act
            var result = await _fuelService.GetAllFuelRecordsAsync();

            // Assert
            using (new FluentAssertions.Execution.AssertionScope())
            {
                result.Should().NotBeNull("service should never return null");
                result.Should().OnlyContain(f => f.Vehicle != null, "all fuel records must have vehicle data loaded");
                result.First().Vehicle!.BusNumber.Should().Be("TEST001", "vehicle data should match test bus");
            }
        }

        #endregion

        #region GetFuelRecordByIdAsync Tests

        [Test]
        public async Task GetFuelRecordByIdAsync_WithValidId_ShouldReturnRecord()
        {
            // Arrange
            DbContext.FuelRecords.AddRange(_testFuelRecords);
            await DbContext.SaveChangesAsync();

            // Get the actual ID from the saved record
            var savedRecord = _testFuelRecords.First();

            // Act
            var result = await _fuelService.GetFuelRecordByIdAsync(savedRecord.FuelId);

            // Assert
            using (new FluentAssertions.Execution.AssertionScope())
            {
                result.Should().NotBeNull("fuel record should be found with valid ID");
                result!.FuelId.Should().Be(savedRecord.FuelId, "returned record should have correct ID");
                result.Gallons.Should().Be(50.5m, "gallons should match test data");
                result.Vehicle.Should().NotBeNull("vehicle data should be included");
            }
        }

        [Test]
        public async Task GetFuelRecordByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            DbContext.FuelRecords.AddRange(_testFuelRecords);
            await DbContext.SaveChangesAsync();

            // Act
            var result = await _fuelService.GetFuelRecordByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task GetFuelRecordByIdAsync_WithNonExistentId_ShouldReturnNull()
        {
            // Act
            var result = await _fuelService.GetFuelRecordByIdAsync(999);

            // Assert
            result.Should().BeNull("non-existent ID should return null");
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
                Notes = "New test record"
            };

            // Act
            var result = await _fuelService.CreateFuelRecordAsync(newFuel);

            // Assert
            using (new FluentAssertions.Execution.AssertionScope())
            {
                result.Should().NotBeNull("fuel record should be created successfully");
                result.FuelId.Should().BeGreaterThan(0, "new record should have valid auto-generated ID");
                result.Gallons.Should().Be(52.3m, "gallons should match input exactly");
                result.TotalCost.Should().Be(167.36m, "cost calculation should be accurate");
                result.VehicleFueledId.Should().Be(_testBus.VehicleId, "should be linked to correct vehicle");
            }

            // Verify in database with fresh context
            ClearChangeTracker();
            var dbRecord = await DbContext.FuelRecords.FindAsync(result.FuelId);
            dbRecord.Should().NotBeNull("record should be persisted in database");
            dbRecord!.Gallons.Should().Be(52.3m, "persisted data should match input");
        }

        [Test]
        public async Task CreateFuelRecordAsync_ShouldSaveToDatabase()
        {
            // Arrange
            var newFuel = new Fuel
            {
                VehicleFueledId = _testBus.VehicleId,
                FuelDate = DateTime.Today,
                Gallons = 50.0m,
                TotalCost = 150.0m,
                FuelType = "Diesel",
                FuelLocation = "Test Station",
                VehicleOdometerReading = 16000
            };

            // Act
            var result = await _fuelService.CreateFuelRecordAsync(newFuel);

            // Assert
            using (new FluentAssertions.Execution.AssertionScope())
            {
                result.FuelId.Should().BeGreaterThan(0, "database should assign valid ID");
                result.VehicleFueledId.Should().Be(_testBus.VehicleId, "vehicle association should be maintained");
            }
        }

        #endregion

        #region UpdateFuelRecordAsync Tests

        [Test]
        public async Task UpdateFuelRecordAsync_ShouldUpdateInDatabase()
        {
            // Arrange
            DbContext.FuelRecords.Add(_testFuelRecords[0]);
            await DbContext.SaveChangesAsync();
            ClearChangeTracker(); // Clear tracking to avoid conflicts

            var savedRecord = await DbContext.FuelRecords.FirstAsync();
            savedRecord.Gallons = 55.0m;
            savedRecord.TotalCost = 165.0m;
            savedRecord.Notes = "Updated notes";

            // Act
            var result = await _fuelService.UpdateFuelRecordAsync(savedRecord);

            // Assert
            using (new FluentAssertions.Execution.AssertionScope())
            {
                result.Should().NotBeNull("update should return updated record");
                result.Gallons.Should().Be(55.0m, "gallons should be updated");
                result.TotalCost.Should().Be(165.0m, "cost should be updated");
                result.Notes.Should().Be("Updated notes", "notes should be updated");
            }

            // Verify in database with fresh context
            ClearChangeTracker();
            var dbRecord = await DbContext.FuelRecords.FindAsync(savedRecord.FuelId);
            using (new FluentAssertions.Execution.AssertionScope())
            {
                dbRecord!.Gallons.Should().Be(55.0m, "database should reflect gallons update");
                dbRecord.TotalCost.Should().Be(165.0m, "database should reflect cost update");
            }
        }

        #endregion

        #region DeleteFuelRecordAsync Tests

        [Test]
        public async Task DeleteFuelRecordAsync_WithValidId_ShouldReturnTrueAndDelete()
        {
            // Arrange
            DbContext.FuelRecords.Add(_testFuelRecords[0]);
            await DbContext.SaveChangesAsync();

            var savedRecord = await DbContext.FuelRecords.FirstAsync();
            var recordId = savedRecord.FuelId;

            // Act
            var result = await _fuelService.DeleteFuelRecordAsync(recordId);

            // Assert
            using (new FluentAssertions.Execution.AssertionScope())
            {
                result.Should().BeTrue("deletion of existing record should succeed");
            }

            // Verify deletion
            ClearChangeTracker();
            var dbRecord = await DbContext.FuelRecords.FindAsync(recordId);
            dbRecord.Should().BeNull("deleted record should not exist in database");
        }

        [Test]
        public async Task DeleteFuelRecordAsync_WithInvalidId_ShouldReturnFalse()
        {
            // Act
            var result = await _fuelService.DeleteFuelRecordAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task DeleteFuelRecordAsync_WithNonExistentId_ShouldReturnFalse()
        {
            // Arrange
            DbContext.FuelRecords.Add(_testFuelRecords[0]);
            await DbContext.SaveChangesAsync();

            var savedRecord = await DbContext.FuelRecords.FirstAsync();

            // Act
            var result = await _fuelService.DeleteFuelRecordAsync(999);

            // Assert
            using (new FluentAssertions.Execution.AssertionScope())
            {
                result.Should().BeFalse("deletion of non-existent record should fail");
            }

            // Verify original record still exists
            ClearChangeTracker();
            var dbRecord = await DbContext.FuelRecords.FindAsync(savedRecord.FuelId);
            dbRecord.Should().NotBeNull("original record should remain untouched");
        }

        #endregion

        #region GetFuelRecordsByVehicleAsync Tests

        [Test]
        public async Task GetFuelRecordsByVehicleAsync_WithValidVehicleId_ShouldReturnFilteredRecords()
        {
            // Arrange
            // Add another vehicle and its fuel records (don't set explicit IDs)
            var anotherBus = new Bus
            {
                BusNumber = "TEST002",
                Make = "Thomas",
                Model = "C2",
                Year = 2019,
                Status = "Active",
                LicenseNumber = $"TST{Guid.NewGuid().ToString("N")[..6]}", // Unique license number
                VINNumber = $"VIN{Guid.NewGuid().ToString("N")[..10]}" // Unique VIN
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

            // Assert
            using (new FluentAssertions.Execution.AssertionScope())
            {
                result.Should().NotBeNull("service should never return null");
                result.Should().HaveCount(3, "should return only records for specified vehicle");
                result.Should().OnlyContain(f => f.VehicleFueledId == _testBus.VehicleId, "all records should belong to the requested vehicle");
                result.Should().BeInDescendingOrder(f => f.FuelDate, "records should be ordered by date descending");
                result.Should().AllSatisfy(fuel =>
                {
                    fuel.Gallons.Should().BeGreaterThan(0, "all fuel records should have positive gallons");
                    fuel.FuelType.Should().NotBeNullOrEmpty("all fuel records should have fuel type specified");
                });
            }
        }

        [Test]
        public async Task GetFuelRecordsByVehicleAsync_WithInvalidVehicleId_ShouldReturnEmpty()
        {
            // Arrange
            DbContext.FuelRecords.AddRange(_testFuelRecords);
            await DbContext.SaveChangesAsync();

            // Act
            var result = await _fuelService.GetFuelRecordsByVehicleAsync(999);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region GetFuelRecordsByDateRangeAsync Tests

        [Test]
        public async Task GetFuelRecordsByDateRangeAsync_WithValidRange_ShouldReturnFilteredRecords()
        {
            // Arrange
            DbContext.FuelRecords.AddRange(_testFuelRecords);
            await DbContext.SaveChangesAsync();

            var startDate = DateTime.Today.AddDays(-7);
            var endDate = DateTime.Today.AddDays(-2);

            // Act
            var result = await _fuelService.GetFuelRecordsByDateRangeAsync(startDate, endDate);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1); // Only the record from 5 days ago falls in this range
            result.First().FuelDate.Should().Be(DateTime.Today.AddDays(-5));
        }

        [Test]
        public async Task GetFuelRecordsByDateRangeAsync_WithNoRecordsInRange_ShouldReturnEmpty()
        {
            // Arrange
            DbContext.FuelRecords.AddRange(_testFuelRecords);
            await DbContext.SaveChangesAsync();

            var startDate = DateTime.Today.AddDays(-30);
            var endDate = DateTime.Today.AddDays(-20);

            // Act
            var result = await _fuelService.GetFuelRecordsByDateRangeAsync(startDate, endDate);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetFuelRecordsByDateRangeAsync_WithInclusiveDates_ShouldIncludeBoundaryRecords()
        {
            // Arrange
            DbContext.FuelRecords.AddRange(_testFuelRecords);
            await DbContext.SaveChangesAsync();

            var startDate = DateTime.Today.AddDays(-10);
            var endDate = DateTime.Today.AddDays(-1);

            // Act
            var result = await _fuelService.GetFuelRecordsByDateRangeAsync(startDate, endDate);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3); // All records should be included
        }

        #endregion

        #region GetTotalFuelCostAsync Tests

        [Test]
        public async Task GetTotalFuelCostAsync_WithValidVehicleId_ShouldReturnTotalCost()
        {
            // Arrange
            DbContext.FuelRecords.AddRange(_testFuelRecords);
            await DbContext.SaveChangesAsync();

            var expectedTotal = 151.50m + 140.12m + 153.60m; // Sum of all test records

            // Act
            var result = await _fuelService.GetTotalFuelCostAsync(_testBus.VehicleId);

            // Assert
            result.Should().Be(expectedTotal);
        }

        [Test]
        public async Task GetTotalFuelCostAsync_WithDateRange_ShouldReturnFilteredTotal()
        {
            // Arrange
            DbContext.FuelRecords.AddRange(_testFuelRecords);
            await DbContext.SaveChangesAsync();

            var startDate = DateTime.Today.AddDays(-6);
            var endDate = DateTime.Today;

            var expectedTotal = 140.12m + 153.60m; // Only last 2 records

            // Act
            var result = await _fuelService.GetTotalFuelCostAsync(_testBus.VehicleId, startDate, endDate);

            // Assert
            result.Should().Be(expectedTotal, "should sum costs for records in date range");
        }

        [Test]
        public async Task GetTotalFuelCostAsync_WithInvalidVehicleId_ShouldReturnZero()
        {
            // Arrange
            DbContext.FuelRecords.AddRange(_testFuelRecords);
            await DbContext.SaveChangesAsync();

            // Act
            var result = await _fuelService.GetTotalFuelCostAsync(999);

            // Assert
            result.Should().Be(0);
        }

        [Test]
        public async Task GetTotalFuelCostAsync_WithNullCosts_ShouldIgnoreNullValues()
        {
            // Arrange
            var recordWithNullCost = new Fuel
            {
                VehicleFueledId = _testBus.VehicleId,
                FuelDate = DateTime.Today,
                Gallons = 50.0m,
                TotalCost = null, // Null cost
                FuelType = "Diesel",
                FuelLocation = "Test Station",
                VehicleOdometerReading = 16000
            };

            DbContext.FuelRecords.Add(_testFuelRecords[0]); // Has cost
            DbContext.FuelRecords.Add(recordWithNullCost); // No cost
            await DbContext.SaveChangesAsync();

            // Act
            var result = await _fuelService.GetTotalFuelCostAsync(_testBus.VehicleId);

            // Assert
            result.Should().Be(151.50m, "should return cost of the single record with cost data"); // Only the record with cost
        }

        #endregion

        #region GetTotalGallonsAsync Tests

        [Test]
        public async Task GetTotalGallonsAsync_WithValidVehicleId_ShouldReturnTotalGallons()
        {
            // Arrange
            DbContext.FuelRecords.AddRange(_testFuelRecords);
            await DbContext.SaveChangesAsync();

            var expectedTotal = 50.5m + 45.2m + 48.0m; // Sum of all test record gallons

            // Act
            var result = await _fuelService.GetTotalGallonsAsync(_testBus.VehicleId);

            // Assert
            result.Should().Be(expectedTotal, "should sum all gallons for the vehicle");
        }

        [Test]
        public async Task GetTotalGallonsAsync_WithDateRange_ShouldReturnFilteredTotal()
        {
            // Arrange
            DbContext.FuelRecords.AddRange(_testFuelRecords);
            await DbContext.SaveChangesAsync();

            var startDate = DateTime.Today.AddDays(-6);
            var endDate = DateTime.Today;

            var expectedTotal = 45.2m + 48.0m; // Only last 2 records

            // Act
            var result = await _fuelService.GetTotalGallonsAsync(_testBus.VehicleId, startDate, endDate);

            // Assert
            result.Should().Be(expectedTotal, "should sum gallons for records in date range");
        }

        [Test]
        public async Task GetTotalGallonsAsync_WithInvalidVehicleId_ShouldReturnZero()
        {
            // Arrange
            DbContext.FuelRecords.AddRange(_testFuelRecords);
            await DbContext.SaveChangesAsync();

            // Act
            var result = await _fuelService.GetTotalGallonsAsync(999);

            // Assert
            result.Should().Be(0);
        }

        #endregion

        #region GetAverageMPGAsync Tests

        [Test]
        public async Task GetAverageMPGAsync_WithVehicleHavingMPG_ShouldReturnVehicleMPG()
        {
            // Act
            var result = await _fuelService.GetAverageMPGAsync(_testBus.VehicleId);

            // Assert
            result.Should().Be(8.5m, "should return the vehicle's configured MPG"); // The test bus MPG
        }

        [Test]
        public async Task GetAverageMPGAsync_WithVehicleWithoutMPG_ShouldReturnDefaultValue()
        {
            // Arrange
            var busWithoutMPG = new Bus
            {
                BusNumber = "TEST003",
                Make = "IC Bus",
                Model = "CE",
                Year = 2018,
                MilesPerGallon = null, // No MPG data
                Status = "Active",
                LicenseNumber = $"TST{Guid.NewGuid().ToString("N")[..6]}", // Unique license number
                VINNumber = $"VIN{Guid.NewGuid().ToString("N")[..10]}" // Unique VIN
            };
            DbContext.Vehicles.Add(busWithoutMPG);
            await DbContext.SaveChangesAsync();

            // Act
            var result = await _fuelService.GetAverageMPGAsync(busWithoutMPG.VehicleId);

            // Assert
            result.Should().Be(7.5m, "should return default MPG when vehicle has no MPG data"); // Default average bus MPG
        }

        [Test]
        public async Task GetAverageMPGAsync_WithNonExistentVehicle_ShouldReturnDefaultValue()
        {
            // Act
            var result = await _fuelService.GetAverageMPGAsync(999);

            // Assert
            result.Should().Be(7.5m); // Default average bus MPG
        }

        [Test]
        public async Task GetAverageMPGAsync_WithDateRange_ShouldReturnVehicleMPG()
        {
            // Note: Current implementation doesn't use date range for calculation
            // Act
            var result = await _fuelService.GetAverageMPGAsync(_testBus.VehicleId, DateTime.Today.AddDays(-30), DateTime.Today);

            // Assert
            result.Should().Be(8.5m, "should return the vehicle's configured MPG regardless of date range"); // The test bus MPG
        }

        #endregion

    }
}

