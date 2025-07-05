using NUnit.Framework;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Bus_Buddy.Data;
using Bus_Buddy.Data.Repositories;
using Bus_Buddy.Models;
using Bus_Buddy.Services;
using BusBuddy.Tests.Infrastructure;
using Moq;

namespace BusBuddy.Tests.UnitTests.Data
{
    /// <summary>
    /// Comprehensive tests for BusRepository to ensure coverage of data layer operations
    /// Tests both async and sync methods, business logic, and data retrieval scenarios
    /// </summary>
    [TestFixture]
    [NonParallelizable] // Force sequential execution
    [SingleThreaded]    // NUnit attribute for single-threaded execution
    [Order(1)]          // Control test execution order
    public class BusRepositoryTests : TestBase
    {
        private BusBuddyDbContext _context = null!;
        private BusRepository _repository = null!;
        private Mock<IUserContextService> _mockUserContextService = null!;

        [SetUp]
        public void Setup()
        {
            // Use base TestBase to initialize a fresh in-memory DbContext
            SetupTestDatabase();
            _context = DbContext;

            // Use the UserContextService from base or create a new mock for audit
            _mockUserContextService = new Mock<IUserContextService>();
            _mockUserContextService.Setup(x => x.GetCurrentUserForAudit()).Returns("test-user");

            _repository = new BusRepository(_context, _mockUserContextService.Object);

            // Seed test data
            SeedTestDataAsync().GetAwaiter().GetResult();
        }

        [TearDown]
        public async Task TearDown()
        {
            // Clean database between tests to prevent duplicate key violations
            await ClearDatabaseAsync();
        }


        private async Task SeedTestDataAsync()
        {
            var buses = new List<Bus>
            {
                new Bus
                {
                    // VehicleId removed - let SQL Server auto-increment
                    BusNumber = "BUS001",
                    Year = 2020,
                    Make = "Blue Bird",
                    Model = "Vision",
                    SeatingCapacity = 72,
                    VINNumber = "1BAKBUCL4LF123456",
                    LicenseNumber = "ABC123",
                    Status = "Active",
                    DateLastInspection = DateTime.Now.AddMonths(-6),
                    InsuranceExpiryDate = DateTime.Now.AddMonths(6),
                    FleetType = "Regular",
                    GPSTracking = true,
                    SpecialEquipment = "Air Conditioning",
                    NextMaintenanceDue = DateTime.Now.AddDays(15)
                },
                new Bus
                {
                    // VehicleId removed - let SQL Server auto-increment
                    BusNumber = "BUS002",
                    Year = 2018,
                    Make = "Thomas Built",
                    Model = "Saf-T-Liner",
                    SeatingCapacity = 54,
                    VINNumber = "1T8HFCB78J1234567",
                    LicenseNumber = "DEF456",
                    Status = "Active",
                    DateLastInspection = DateTime.Now.AddMonths(-14), // Overdue
                    InsuranceExpiryDate = DateTime.Now.AddDays(15), // Expiring soon
                    FleetType = "Special Needs",
                    GPSTracking = false,
                    SpecialEquipment = "Wheelchair Lift"
                },
                new Bus
                {
                    // VehicleId removed - let SQL Server auto-increment
                    BusNumber = "BUS003",
                    Year = 2022,
                    Make = "IC Bus",
                    Model = "CE300",
                    SeatingCapacity = 48,
                    VINNumber = "1HVBRACL9MH123789",
                    LicenseNumber = "GHI789",
                    Status = "Maintenance",
                    DateLastInspection = DateTime.Now.AddMonths(-2),
                    InsuranceExpiryDate = DateTime.Now.AddDays(-10), // Expired
                    FleetType = "Activity",
                    GPSTracking = true
                }
            }; _context.Vehicles.AddRange(buses);
            // Save buses first to get their auto-generated IDs
            await _context.SaveChangesAsync();

            // Get the first bus ID for activity relationship
            var firstBus = await _context.Vehicles.FirstAsync();

            // Add some activities for availability testing (no driver required)
            var activities = new List<Activity>
            {
                new Activity
                {
                    // ActivityId removed - let SQL Server auto-increment
                    AssignedVehicleId = firstBus.VehicleId, // Use the actual generated ID
                    // DriverId is optional - not setting it
                    Date = DateTime.Today,
                    LeaveTime = TimeSpan.FromHours(8),
                    EventTime = TimeSpan.FromHours(10),
                    ActivityType = "Regular Route",
                    Destination = "Test School",
                    RequestedBy = "Test User"
                }
            };

            _context.Activities.AddRange(activities);

            // Save activities
            await _context.SaveChangesAsync();
        }

        #region Basic CRUD Operations

        [Test]
        [Order(1)] // Run first to ensure clean state
        public async Task AddAsync_ShouldAddBusToDatabase()
        {
            // Arrange
            var newBus = new Bus
            {
                BusNumber = "BUS004",
                Year = 2023,
                Make = "Blue Bird",
                Model = "Vision",
                SeatingCapacity = 72,
                VINNumber = "1BAKBUCL4NF123456",
                LicenseNumber = "JKL012",
                Status = "Active"
            };

            // Act
            await _repository.AddAsync(newBus);
            await _context.SaveChangesAsync();

            // Assert
            var savedBus = await _repository.GetByIdAsync(newBus.VehicleId);
            savedBus.Should().NotBeNull();
            savedBus!.BusNumber.Should().Be("BUS004");
            savedBus.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }

        [Test]
        [Order(2)] // Run after AddAsync
        public async Task GetByIdAsync_ShouldReturnCorrectBus()
        {
            // Arrange - Get the first bus from our seed data
            var buses = await _repository.GetAllAsync();
            var firstBus = buses.OrderBy(b => b.BusNumber).First();

            // Act
            var bus = await _repository.GetByIdAsync(firstBus.VehicleId);

            // Assert
            bus.Should().NotBeNull();
            bus!.BusNumber.Should().Be("BUS001");
            bus.Make.Should().Be("Blue Bird");
        }

        [Test]
        [Order(3)] // Run after basic CRUD
        public async Task GetAllAsync_ShouldReturnAllBuses()
        {
            // Act
            var buses = await _repository.GetAllAsync();

            // Assert
            buses.Should().HaveCountGreaterOrEqualTo(3); // At least our 3 seed buses
            buses.Should().Contain(b => b.BusNumber == "BUS001");
            buses.Should().Contain(b => b.BusNumber == "BUS002");
            buses.Should().Contain(b => b.BusNumber == "BUS003");
        }

        [Test]
        public async Task Update_ShouldModifyBusProperties()
        {
            // Arrange - Get the first bus dynamically instead of hardcoded ID
            var buses = await _repository.GetAllAsync();
            var firstBus = buses.OrderBy(b => b.BusNumber).First();
            firstBus.Status = "Maintenance";
            firstBus.CurrentOdometer = 50000;

            // Act
            _repository.Update(firstBus);
            await _context.SaveChangesAsync();

            // Assert
            var updatedBus = await _repository.GetByIdAsync(firstBus.VehicleId);
            updatedBus!.Status.Should().Be("Maintenance");
            updatedBus.CurrentOdometer.Should().Be(50000);
        }

        [Test]
        public async Task RemoveByIdAsync_ShouldRemoveBusFromDatabase()
        {
            // Arrange - Get the last bus dynamically and capture original count
            var originalBuses = (await _repository.GetAllAsync()).ToList();
            var originalCount = originalBuses.Count;
            var lastBus = originalBuses.OrderBy(bus => bus.BusNumber).Last();
            var lastBusId = lastBus.VehicleId;

            // Act
            await _repository.RemoveByIdAsync(lastBusId);
            await _context.SaveChangesAsync();

            // Assert
            var deletedBus = await _repository.GetByIdAsync(lastBusId);
            deletedBus.Should().BeNull();

            var remainingBuses = await _repository.GetAllAsync();
            remainingBuses.Should().HaveCount(originalCount - 1);
        }

        #endregion

        #region Vehicle-Specific Operations

        [Test]
        public async Task GetActiveVehiclesAsync_ShouldReturnOnlyActiveBuses()
        {
            // Act
            var activeBuses = await _repository.GetActiveVehiclesAsync();

            // Assert
            activeBuses.Should().HaveCount(2);
            activeBuses.Should().OnlyContain(b => b.Status == "Active");
            activeBuses.Should().BeInAscendingOrder(b => b.BusNumber);
        }

        [Test]
        public async Task GetVehiclesByStatusAsync_ShouldFilterByStatus()
        {
            // Act
            var maintenanceBuses = await _repository.GetVehiclesByStatusAsync("Maintenance");

            // Assert
            maintenanceBuses.Should().HaveCount(1);
            maintenanceBuses.First().BusNumber.Should().Be("BUS003");
        }

        [Test]
        public async Task GetVehiclesByFleetTypeAsync_ShouldFilterByFleetType()
        {
            // Act
            var specialNeedsBuses = await _repository.GetVehiclesByFleetTypeAsync("Special Needs");

            // Assert
            specialNeedsBuses.Should().HaveCount(1);
            specialNeedsBuses.First().BusNumber.Should().Be("BUS002");
        }

        [Test]
        public async Task GetVehicleByBusNumberAsync_ShouldReturnCorrectBus()
        {
            // Act
            var bus = await _repository.GetVehicleByBusNumberAsync("BUS001");

            // Assert
            bus.Should().NotBeNull();
            bus!.BusNumber.Should().Be("BUS001");
            bus.Make.Should().Be("Blue Bird");
        }

        [Test]
        public async Task GetVehicleByVINAsync_ShouldReturnCorrectBus()
        {
            // Act
            var bus = await _repository.GetVehicleByVINAsync("1BAKBUCL4LF123456");

            // Assert
            bus.Should().NotBeNull();
            bus!.BusNumber.Should().Be("BUS001");
        }

        [Test]
        public async Task GetVehicleByLicenseNumberAsync_ShouldReturnCorrectBus()
        {
            // Act
            var bus = await _repository.GetVehicleByLicenseNumberAsync("ABC123");

            // Assert
            bus.Should().NotBeNull();
            bus!.BusNumber.Should().Be("BUS001");
        }

        [Test]
        public async Task GetAvailableVehiclesAsync_ShouldExcludeVehiclesWithConflictingActivities()
        {
            // Act - Check availability during conflicting time
            var availableBuses = await _repository.GetAvailableVehiclesAsync(
                DateTime.Today,
                TimeSpan.FromHours(9),
                TimeSpan.FromHours(11));

            // Assert
            availableBuses.Should().HaveCount(1); // Only BUS002 should be available
            availableBuses.First().BusNumber.Should().Be("BUS002");
        }

        [Test]
        public async Task GetAvailableVehiclesAsync_WithoutTimeFilter_ShouldReturnAllActiveVehicles()
        {
            // Act
            var availableBuses = await _repository.GetAvailableVehiclesAsync(DateTime.Today);

            // Assert
            availableBuses.Should().HaveCount(2); // Both active buses
        }

        #endregion

        #region Maintenance and Inspection

        [Test]
        public async Task GetVehiclesDueForInspectionAsync_ShouldReturnOverdueBuses()
        {
            // Act
            var overdueBuses = await _repository.GetVehiclesDueForInspectionAsync(30);

            // Assert
            overdueBuses.Should().HaveCount(1);
            overdueBuses.First().BusNumber.Should().Be("BUS002"); // 14 months old inspection
        }

        [Test]
        public async Task GetVehiclesWithExpiredInspectionAsync_ShouldReturnExpiredBuses()
        {
            // Act
            var expiredBuses = await _repository.GetVehiclesWithExpiredInspectionAsync();

            // Assert
            expiredBuses.Should().HaveCount(1);
            expiredBuses.First().BusNumber.Should().Be("BUS002");
        }

        [Test]
        public async Task GetVehiclesDueForMaintenanceAsync_ShouldReturnBusesDueSoon()
        {
            // Act
            var maintenanceDueBuses = await _repository.GetVehiclesDueForMaintenanceAsync();

            // Assert
            maintenanceDueBuses.Should().HaveCount(1);
            maintenanceDueBuses.First().BusNumber.Should().Be("BUS001"); // Due in 15 days
        }

        [Test]
        public async Task GetVehiclesWithExpiredInsuranceAsync_ShouldReturnExpiredInsurance()
        {
            // Act
            var expiredInsuranceBuses = await _repository.GetVehiclesWithExpiredInsuranceAsync();

            // Assert
            expiredInsuranceBuses.Should().HaveCount(1);
            expiredInsuranceBuses.First().BusNumber.Should().Be("BUS003");
        }

        [Test]
        public async Task GetVehiclesWithExpiringInsuranceAsync_ShouldReturnExpiringSoon()
        {
            // Act
            var expiringSoonBuses = await _repository.GetVehiclesWithExpiringInsuranceAsync(30);

            // Assert
            expiringSoonBuses.Should().HaveCount(1);
            expiringSoonBuses.First().BusNumber.Should().Be("BUS002"); // Expires in 15 days
        }

        #endregion

        #region Capacity and Features

        [Test]
        public async Task GetVehiclesBySeatingCapacityAsync_ShouldFilterByCapacity()
        {
            // Act
            var largeBuses = await _repository.GetVehiclesBySeatingCapacityAsync(60);

            // Assert
            largeBuses.Should().HaveCount(1);
            largeBuses.First().BusNumber.Should().Be("BUS001"); // 72 capacity
        }

        [Test]
        public async Task GetVehiclesBySeatingCapacityAsync_WithMaxCapacity_ShouldFilterRange()
        {
            // Act
            var mediumBuses = await _repository.GetVehiclesBySeatingCapacityAsync(40, 60);

            // Assert
            mediumBuses.Should().HaveCount(2); // BUS002 (54) and BUS003 (48)
        }

        [Test]
        public async Task GetVehiclesWithSpecialEquipmentAsync_ShouldFilterByEquipment()
        {
            // Act
            var wheelchairBuses = await _repository.GetVehiclesWithSpecialEquipmentAsync("Wheelchair");

            // Assert
            wheelchairBuses.Should().HaveCount(1);
            wheelchairBuses.First().BusNumber.Should().Be("BUS002");
        }

        [Test]
        public async Task GetVehiclesWithGPSAsync_ShouldReturnGPSEnabledBuses()
        {
            // Act
            var gpsBuses = await _repository.GetVehiclesWithGPSAsync();

            // Assert
            gpsBuses.Should().HaveCount(2); // BUS001 and BUS003
            gpsBuses.Should().OnlyContain(b => b.GPSTracking);
        }

        #endregion

        #region Statistics and Reporting

        [Test]
        public async Task GetTotalVehicleCountAsync_ShouldReturnCorrectCount()
        {
            // Act
            var count = await _repository.GetTotalVehicleCountAsync();

            // Assert
            count.Should().Be(3);
        }

        [Test]
        public async Task GetActiveVehicleCountAsync_ShouldReturnActiveCount()
        {
            // Act
            var activeCount = await _repository.GetActiveVehicleCountAsync();

            // Assert
            activeCount.Should().Be(2);
        }

        [Test]
        public async Task GetAverageVehicleAgeAsync_ShouldCalculateCorrectAge()
        {
            // Act
            var averageAge = await _repository.GetAverageVehicleAgeAsync();

            // Assert
            averageAge.Should().BeGreaterThan(0);
            averageAge.Should().BeLessThan(10); // Reasonable age range
        }

        [Test]
        public async Task GetVehicleCountByStatusAsync_ShouldGroupByStatus()
        {
            // Act
            var statusCounts = await _repository.GetVehicleCountByStatusAsync();

            // Assert
            statusCounts.Should().ContainKey("Active");
            statusCounts.Should().ContainKey("Maintenance");
            statusCounts["Active"].Should().Be(2);
            statusCounts["Maintenance"].Should().Be(1);
        }

        [Test]
        public async Task GetVehicleCountByMakeAsync_ShouldGroupByMake()
        {
            // Act
            var makeCounts = await _repository.GetVehicleCountByMakeAsync();

            // Assert
            makeCounts.Should().ContainKey("Blue Bird");
            makeCounts.Should().ContainKey("Thomas Built");
            makeCounts.Should().ContainKey("IC Bus");
            makeCounts.Values.Sum().Should().Be(3);
        }

        [Test]
        public async Task GetVehicleCountByYearAsync_ShouldGroupByYear()
        {
            // Act
            var yearCounts = await _repository.GetVehicleCountByYearAsync();

            // Assert
            yearCounts.Should().ContainKey(2020);
            yearCounts.Should().ContainKey(2018);
            yearCounts.Should().ContainKey(2022);
            yearCounts.Values.Sum().Should().Be(3);
        }

        #endregion

        #region Synchronous Methods

        [Test]
        public void GetActiveVehicles_ShouldReturnActiveBuses()
        {
            // Act
            var activeBuses = _repository.GetActiveVehicles();

            // Assert
            activeBuses.Should().HaveCount(2);
            activeBuses.Should().OnlyContain(b => b.Status == "Active");
        }

        [Test]
        public void GetVehiclesByStatus_ShouldFilterByStatus()
        {
            // Act
            var maintenanceBuses = _repository.GetVehiclesByStatus("Maintenance");

            // Assert
            maintenanceBuses.Should().HaveCount(1);
            maintenanceBuses.First().BusNumber.Should().Be("BUS003");
        }

        [Test]
        public void GetVehicleByBusNumber_ShouldReturnCorrectBus()
        {
            // Act
            var bus = _repository.GetVehicleByBusNumber("BUS001");

            // Assert
            bus.Should().NotBeNull();
            bus!.BusNumber.Should().Be("BUS001");
            bus.Make.Should().Be("Blue Bird");
        }

        [Test]
        public void GetVehiclesDueForInspection_ShouldReturnOverdueBuses()
        {
            // Act
            var overdueBuses = _repository.GetVehiclesDueForInspection(30);

            // Assert
            overdueBuses.Should().HaveCount(1);
            overdueBuses.First().BusNumber.Should().Be("BUS002");
        }

        #endregion

        #region Edge Cases and Error Handling

        [Test]
        public async Task GetVehicleByBusNumberAsync_WithNonexistentNumber_ShouldReturnNull()
        {
            // Act
            var bus = await _repository.GetVehicleByBusNumberAsync("NONEXISTENT");

            // Assert
            bus.Should().BeNull();
        }

        [Test]
        public async Task GetVehicleByVINAsync_WithNonexistentVIN_ShouldReturnNull()
        {
            // Act
            var bus = await _repository.GetVehicleByVINAsync("NONEXISTENT123456789");

            // Assert
            bus.Should().BeNull();
        }

        [Test]
        public async Task GetVehiclesByStatusAsync_WithEmptyStatus_ShouldReturnEmpty()
        {
            // Act
            var buses = await _repository.GetVehiclesByStatusAsync("NonexistentStatus");

            // Assert
            buses.Should().BeEmpty();
        }

        [Test]
        public async Task GetVehiclesBySeatingCapacityAsync_WithHighMinCapacity_ShouldReturnEmpty()
        {
            // Act
            var buses = await _repository.GetVehiclesBySeatingCapacityAsync(100);

            // Assert
            buses.Should().BeEmpty();
        }

        #endregion
    }
}
