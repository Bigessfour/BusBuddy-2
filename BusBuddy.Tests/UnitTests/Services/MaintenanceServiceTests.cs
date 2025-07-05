using NUnit.Framework;
using FluentAssertions;
using Bus_Buddy.Services;
using Bus_Buddy.Models;
using Microsoft.EntityFrameworkCore;
using Bus_Buddy.Data;
using Microsoft.Extensions.DependencyInjection;
using BusBuddy.Tests.Infrastructure;

namespace BusBuddy.Tests.UnitTests.Services;

/// <summary>
/// Comprehensive test suite for MaintenanceService
/// Tests all 9 methods defined in IMaintenanceService interface
/// Created following 9-step testing methodology from TEST_COVERAGE_ANALYSIS.md
/// </summary>
[TestFixture]

public class MaintenanceServiceTests : TestBase
{
    private IMaintenanceService _maintenanceService = null!;
    private BusBuddyDbContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        SetupTestDatabase();
        _context = GetService<BusBuddyDbContext>();
        _maintenanceService = GetService<IMaintenanceService>();
    }

    [TearDown]
    public void TearDown()
    {
        TearDownTestDatabase();
    }

    #region GetAllMaintenanceRecordsAsync Tests

    [Test]
    public async Task GetAllMaintenanceRecordsAsync_ShouldReturnEmptyList_WhenNoMaintenanceRecordsExist()
    {
        // Act
        var result = await _maintenanceService.GetAllMaintenanceRecordsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetAllMaintenanceRecordsAsync_ShouldReturnMaintenanceRecords_WithVehicleIncluded()
    {
        // Arrange
        var bus = new Bus
        {
            BusNumber = "001",
            LicenseNumber = "ABC123",
            Model = "Test Model",
            Year = 2020,
            SeatingCapacity = 50,
            Status = "Active",
            CurrentOdometer = 10000
        };
        await _context.Vehicles.AddAsync(bus);
        await _context.SaveChangesAsync();

        var maintenance = new Maintenance
        {
            Date = DateTime.UtcNow.AddDays(-1),
            VehicleId = bus.VehicleId,
            OdometerReading = 10500,
            MaintenanceCompleted = "Oil Change",
            Vendor = "Test Vendor",
            RepairCost = 50.00m,
            Priority = "Normal"
        };
        await _context.MaintenanceRecords.AddAsync(maintenance);
        await _context.SaveChangesAsync();

        // Act
        var result = await _maintenanceService.GetAllMaintenanceRecordsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        var record = result.First();
        record.Vehicle.Should().NotBeNull();
        record.Vehicle.BusNumber.Should().Be("001");
        record.MaintenanceCompleted.Should().Be("Oil Change");
    }

    [Test]
    public async Task GetAllMaintenanceRecordsAsync_ShouldReturnRecordsOrderedByDateDescending()
    {
        // Arrange
        var bus = new Bus
        {
            BusNumber = "001",
            LicenseNumber = "ABC123",
            Model = "Test Model",
            Year = 2020,
            SeatingCapacity = 50,
            Status = "Active",
            CurrentOdometer = 10000
        };
        await _context.Vehicles.AddAsync(bus);
        await _context.SaveChangesAsync();

        var maintenance1 = new Maintenance
        {
            Date = DateTime.UtcNow.AddDays(-2),
            VehicleId = bus.VehicleId,
            OdometerReading = 10000,
            MaintenanceCompleted = "Older Service",
            Vendor = "Test Vendor",
            RepairCost = 50.00m
        };

        var maintenance2 = new Maintenance
        {
            Date = DateTime.UtcNow.AddDays(-1),
            VehicleId = bus.VehicleId,
            OdometerReading = 10500,
            MaintenanceCompleted = "Newer Service",
            Vendor = "Test Vendor",
            RepairCost = 75.00m
        };

        await _context.MaintenanceRecords.AddRangeAsync(maintenance1, maintenance2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _maintenanceService.GetAllMaintenanceRecordsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().MaintenanceCompleted.Should().Be("Newer Service");
        result.Last().MaintenanceCompleted.Should().Be("Older Service");
    }

    #endregion

    #region GetMaintenanceRecordByIdAsync Tests

    [Test]
    public async Task GetMaintenanceRecordByIdAsync_ShouldReturnNull_WhenRecordDoesNotExist()
    {
        // Act
        var result = await _maintenanceService.GetMaintenanceRecordByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task GetMaintenanceRecordByIdAsync_ShouldReturnRecord_WithVehicleIncluded_WhenRecordExists()
    {
        // Arrange
        var bus = new Bus
        {
            BusNumber = "001",
            LicenseNumber = "ABC123",
            Model = "Test Model",
            Year = 2020,
            SeatingCapacity = 50,
            Status = "Active",
            CurrentOdometer = 10000
        };
        await _context.Vehicles.AddAsync(bus);
        await _context.SaveChangesAsync();

        var maintenance = new Maintenance
        {
            Date = DateTime.UtcNow,
            VehicleId = bus.VehicleId,
            OdometerReading = 10500,
            MaintenanceCompleted = "Brake Service",
            Vendor = "Brake Specialists",
            RepairCost = 250.00m,
            Priority = "High"
        };
        await _context.MaintenanceRecords.AddAsync(maintenance);
        await _context.SaveChangesAsync();

        // Act
        var result = await _maintenanceService.GetMaintenanceRecordByIdAsync(maintenance.MaintenanceId);

        // Assert
        result.Should().NotBeNull();
        result!.MaintenanceCompleted.Should().Be("Brake Service");
        result.Vehicle.Should().NotBeNull();
        result.Vehicle.BusNumber.Should().Be("001");
        result.RepairCost.Should().Be(250.00m);
        result.Priority.Should().Be("High");
    }

    #endregion

    #region CreateMaintenanceRecordAsync Tests

    [Test]
    public async Task CreateMaintenanceRecordAsync_ShouldCreateRecord_WithCreatedDateSet()
    {
        // Arrange
        var bus = new Bus
        {
            BusNumber = "001",
            LicenseNumber = "ABC123",
            Model = "Test Model",
            Year = 2020,
            SeatingCapacity = 50,
            Status = "Active",
            CurrentOdometer = 10000
        };
        await _context.Vehicles.AddAsync(bus);
        await _context.SaveChangesAsync();

        var maintenance = new Maintenance
        {
            Date = DateTime.UtcNow,
            VehicleId = bus.VehicleId,
            OdometerReading = 10500,
            MaintenanceCompleted = "New Service",
            Vendor = "Service Center",
            RepairCost = 100.00m,
            Priority = "Normal"
        };

        var beforeCreate = DateTime.UtcNow;

        // Act
        var result = await _maintenanceService.CreateMaintenanceRecordAsync(maintenance);

        // Assert
        result.Should().NotBeNull();
        result.MaintenanceId.Should().BeGreaterThan(0);
        result.CreatedDate.Should().BeOnOrAfter(beforeCreate);
        result.MaintenanceCompleted.Should().Be("New Service");

        // Verify in database
        var dbRecord = await _context.MaintenanceRecords.FindAsync(result.MaintenanceId);
        dbRecord.Should().NotBeNull();
        dbRecord!.CreatedDate.Should().BeOnOrAfter(beforeCreate);
    }

    [Test]
    public async Task CreateMaintenanceRecordAsync_ShouldPersistAllProperties()
    {
        // Arrange
        var bus = new Bus
        {
            BusNumber = "002",
            LicenseNumber = "XYZ789",
            Model = "Test Model",
            Year = 2021,
            SeatingCapacity = 40,
            Status = "Active",
            CurrentOdometer = 5000
        };
        await _context.Vehicles.AddAsync(bus);
        await _context.SaveChangesAsync();

        var maintenance = new Maintenance
        {
            Date = DateTime.UtcNow.AddDays(-1),
            VehicleId = bus.VehicleId,
            OdometerReading = 5500,
            MaintenanceCompleted = "Comprehensive Service",
            Vendor = "Premium Service",
            RepairCost = 500.00m,
            Description = "Full service including filters",
            PerformedBy = "John Doe",
            Priority = "High",
            Status = "Completed",
            WorkOrderNumber = "WO-12345",
            LaborHours = 4.5m,
            LaborCost = 225.00m,
            PartsCost = 275.00m
        };

        // Act
        var result = await _maintenanceService.CreateMaintenanceRecordAsync(maintenance);

        // Assert
        result.Should().NotBeNull();
        result.Description.Should().Be("Full service including filters");
        result.PerformedBy.Should().Be("John Doe");
        result.Priority.Should().Be("High");
        result.WorkOrderNumber.Should().Be("WO-12345");
        result.LaborHours.Should().Be(4.5m);
        result.LaborCost.Should().Be(225.00m);
        result.PartsCost.Should().Be(275.00m);
        result.TotalCost.Should().Be(1000.00m); // RepairCost + LaborCost + PartsCost
    }

    #endregion

    #region UpdateMaintenanceRecordAsync Tests

    [Test]
    public async Task UpdateMaintenanceRecordAsync_ShouldUpdateRecord_WithUpdatedDateSet()
    {
        // Arrange
        var bus = new Bus
        {
            BusNumber = "003",
            LicenseNumber = "DEF456",
            Model = "Test Model",
            Year = 2019,
            SeatingCapacity = 45,
            Status = "Active",
            CurrentOdometer = 15000
        };
        await _context.Vehicles.AddAsync(bus);
        await _context.SaveChangesAsync();

        var maintenance = new Maintenance
        {
            Date = DateTime.UtcNow.AddDays(-2),
            VehicleId = bus.VehicleId,
            OdometerReading = 15200,
            MaintenanceCompleted = "Original Service",
            Vendor = "Original Vendor",
            RepairCost = 150.00m,
            Priority = "Normal"
        };
        await _context.MaintenanceRecords.AddAsync(maintenance);
        await _context.SaveChangesAsync();

        // Update the record
        maintenance.MaintenanceCompleted = "Updated Service";
        maintenance.RepairCost = 200.00m;
        maintenance.Priority = "High";

        var beforeUpdate = DateTime.UtcNow;

        // Act
        var result = await _maintenanceService.UpdateMaintenanceRecordAsync(maintenance);

        // Assert
        result.Should().NotBeNull();
        result.MaintenanceCompleted.Should().Be("Updated Service");
        result.RepairCost.Should().Be(200.00m);
        result.Priority.Should().Be("High");
        result.UpdatedDate.Should().BeOnOrAfter(beforeUpdate);

        // Verify in database
        var dbRecord = await _context.MaintenanceRecords.FindAsync(maintenance.MaintenanceId);
        dbRecord.Should().NotBeNull();
        dbRecord!.MaintenanceCompleted.Should().Be("Updated Service");
        dbRecord.UpdatedDate.Should().BeOnOrAfter(beforeUpdate);
    }

    #endregion

    #region DeleteMaintenanceRecordAsync Tests

    [Test]
    public async Task DeleteMaintenanceRecordAsync_ShouldReturnFalse_WhenRecordDoesNotExist()
    {
        // Act
        var result = await _maintenanceService.DeleteMaintenanceRecordAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    [Category("Integration")] // Mark as integration test - requires real database for proper deletion behavior
    [Ignore("Integration test requires SQL Server database for proper deletion verification. InMemory provider has known limitations with entity deletion tracking.")]
    public async Task DeleteMaintenanceRecordAsync_ShouldReturnTrue_AndRemoveRecord_WhenRecordExists()
    {
        // Arrange
        var bus = new Bus
        {
            BusNumber = "004",
            LicenseNumber = "GHI789",
            Model = "Test Model",
            Year = 2018,
            SeatingCapacity = 35,
            Status = "Active",
            CurrentOdometer = 20000
        };
        await _context.Vehicles.AddAsync(bus);
        await _context.SaveChangesAsync();

        var maintenance = new Maintenance
        {
            Date = DateTime.UtcNow.AddDays(-3),
            VehicleId = bus.VehicleId,
            OdometerReading = 20500,
            MaintenanceCompleted = "To Be Deleted",
            Vendor = "Test Vendor",
            RepairCost = 75.00m
        };
        await _context.MaintenanceRecords.AddAsync(maintenance);
        await _context.SaveChangesAsync();

        var maintenanceId = maintenance.MaintenanceId;

        // Act
        var result = await _maintenanceService.DeleteMaintenanceRecordAsync(maintenanceId);

        // Assert
        result.Should().BeTrue();

        // Verify record is removed by checking the service can't find it
        var deletedRecord = await _maintenanceService.GetMaintenanceRecordByIdAsync(maintenanceId);
        deletedRecord.Should().BeNull();

        // Also verify it's not in the list of all records
        var allRecords = await _maintenanceService.GetAllMaintenanceRecordsAsync();
        allRecords.Should().NotContain(r => r.MaintenanceId == maintenanceId);
    }

    #endregion

    #region GetMaintenanceRecordsByVehicleAsync Tests

    [Test]
    public async Task GetMaintenanceRecordsByVehicleAsync_ShouldReturnEmptyList_WhenNoRecordsForVehicle()
    {
        // Act
        var result = await _maintenanceService.GetMaintenanceRecordsByVehicleAsync(999);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetMaintenanceRecordsByVehicleAsync_ShouldReturnRecordsForSpecificVehicle_OrderedByDateDescending()
    {
        // Arrange
        var bus1 = new Bus
        {
            BusNumber = "005",
            LicenseNumber = "JKL123",
            Model = "Test Model",
            Year = 2020,
            SeatingCapacity = 50,
            Status = "Active",
            CurrentOdometer = 8000
        };

        var bus2 = new Bus
        {
            BusNumber = "006",
            LicenseNumber = "MNO456",
            Model = "Test Model",
            Year = 2021,
            SeatingCapacity = 45,
            Status = "Active",
            CurrentOdometer = 3000
        };

        await _context.Vehicles.AddRangeAsync(bus1, bus2);
        await _context.SaveChangesAsync();

        var maintenance1 = new Maintenance
        {
            Date = DateTime.UtcNow.AddDays(-2),
            VehicleId = bus1.VehicleId,
            OdometerReading = 8100,
            MaintenanceCompleted = "Bus1 Older Service",
            Vendor = "Test Vendor",
            RepairCost = 100.00m
        };

        var maintenance2 = new Maintenance
        {
            Date = DateTime.UtcNow.AddDays(-1),
            VehicleId = bus1.VehicleId,
            OdometerReading = 8200,
            MaintenanceCompleted = "Bus1 Newer Service",
            Vendor = "Test Vendor",
            RepairCost = 150.00m
        };

        var maintenance3 = new Maintenance
        {
            Date = DateTime.UtcNow,
            VehicleId = bus2.VehicleId,
            OdometerReading = 3100,
            MaintenanceCompleted = "Bus2 Service",
            Vendor = "Test Vendor",
            RepairCost = 75.00m
        };

        await _context.MaintenanceRecords.AddRangeAsync(maintenance1, maintenance2, maintenance3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _maintenanceService.GetMaintenanceRecordsByVehicleAsync(bus1.VehicleId);

        // Assert
        result.Should().HaveCount(2);
        result.First().MaintenanceCompleted.Should().Be("Bus1 Newer Service");
        result.Last().MaintenanceCompleted.Should().Be("Bus1 Older Service");
        result.All(r => r.VehicleId == bus1.VehicleId).Should().BeTrue();
        result.All(r => r.Vehicle != null).Should().BeTrue();
    }

    #endregion

    #region GetMaintenanceRecordsByDateRangeAsync Tests

    [Test]
    public async Task GetMaintenanceRecordsByDateRangeAsync_ShouldReturnRecordsWithinDateRange()
    {
        // Arrange
        var bus = new Bus
        {
            BusNumber = "007",
            LicenseNumber = "PQR789",
            Model = "Test Model",
            Year = 2020,
            SeatingCapacity = 40,
            Status = "Active",
            CurrentOdometer = 12000
        };
        await _context.Vehicles.AddAsync(bus);
        await _context.SaveChangesAsync();

        var baseDate = DateTime.UtcNow.Date;
        var maintenance1 = new Maintenance
        {
            Date = baseDate.AddDays(-10), // Outside range
            VehicleId = bus.VehicleId,
            OdometerReading = 12100,
            MaintenanceCompleted = "Outside Range Service",
            Vendor = "Test Vendor",
            RepairCost = 50.00m
        };

        var maintenance2 = new Maintenance
        {
            Date = baseDate.AddDays(-3), // Inside range
            VehicleId = bus.VehicleId,
            OdometerReading = 12200,
            MaintenanceCompleted = "Inside Range Service 1",
            Vendor = "Test Vendor",
            RepairCost = 100.00m
        };

        var maintenance3 = new Maintenance
        {
            Date = baseDate.AddDays(-1), // Inside range
            VehicleId = bus.VehicleId,
            OdometerReading = 12300,
            MaintenanceCompleted = "Inside Range Service 2",
            Vendor = "Test Vendor",
            RepairCost = 75.00m
        };

        await _context.MaintenanceRecords.AddRangeAsync(maintenance1, maintenance2, maintenance3);
        await _context.SaveChangesAsync();

        var startDate = baseDate.AddDays(-5);
        var endDate = baseDate;

        // Act
        var result = await _maintenanceService.GetMaintenanceRecordsByDateRangeAsync(startDate, endDate);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.MaintenanceCompleted == "Inside Range Service 1");
        result.Should().Contain(r => r.MaintenanceCompleted == "Inside Range Service 2");
        result.Should().NotContain(r => r.MaintenanceCompleted == "Outside Range Service");
        result.All(r => r.Date >= startDate && r.Date <= endDate).Should().BeTrue();
        result.All(r => r.Vehicle != null).Should().BeTrue();
    }

    [Test]
    public async Task GetMaintenanceRecordsByDateRangeAsync_ShouldReturnEmptyList_WhenNoRecordsInRange()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-5);
        var endDate = DateTime.UtcNow.AddDays(-3);

        // Act
        var result = await _maintenanceService.GetMaintenanceRecordsByDateRangeAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region GetMaintenanceRecordsByPriorityAsync Tests

    [Test]
    public async Task GetMaintenanceRecordsByPriorityAsync_ShouldReturnRecordsWithSpecificPriority()
    {
        // Arrange
        var bus = new Bus
        {
            BusNumber = "008",
            LicenseNumber = "STU123",
            Model = "Test Model",
            Year = 2019,
            SeatingCapacity = 45,
            Status = "Active",
            CurrentOdometer = 18000
        };
        await _context.Vehicles.AddAsync(bus);
        await _context.SaveChangesAsync();

        var maintenance1 = new Maintenance
        {
            Date = DateTime.UtcNow.AddDays(-2),
            VehicleId = bus.VehicleId,
            OdometerReading = 18100,
            MaintenanceCompleted = "High Priority Service 1",
            Vendor = "Test Vendor",
            RepairCost = 200.00m,
            Priority = "High"
        };

        var maintenance2 = new Maintenance
        {
            Date = DateTime.UtcNow.AddDays(-1),
            VehicleId = bus.VehicleId,
            OdometerReading = 18200,
            MaintenanceCompleted = "Normal Priority Service",
            Vendor = "Test Vendor",
            RepairCost = 100.00m,
            Priority = "Normal"
        };

        var maintenance3 = new Maintenance
        {
            Date = DateTime.UtcNow,
            VehicleId = bus.VehicleId,
            OdometerReading = 18300,
            MaintenanceCompleted = "High Priority Service 2",
            Vendor = "Test Vendor",
            RepairCost = 300.00m,
            Priority = "High"
        };

        await _context.MaintenanceRecords.AddRangeAsync(maintenance1, maintenance2, maintenance3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _maintenanceService.GetMaintenanceRecordsByPriorityAsync("High");

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.MaintenanceCompleted == "High Priority Service 1");
        result.Should().Contain(r => r.MaintenanceCompleted == "High Priority Service 2");
        result.Should().NotContain(r => r.MaintenanceCompleted == "Normal Priority Service");
        result.All(r => r.Priority == "High").Should().BeTrue();
        result.All(r => r.Vehicle != null).Should().BeTrue();
        // Should be ordered by date descending
        result.First().MaintenanceCompleted.Should().Be("High Priority Service 2");
        result.Last().MaintenanceCompleted.Should().Be("High Priority Service 1");
    }

    [Test]
    public async Task GetMaintenanceRecordsByPriorityAsync_ShouldReturnEmptyList_WhenNoPriorityMatches()
    {
        // Act
        var result = await _maintenanceService.GetMaintenanceRecordsByPriorityAsync("Emergency");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region GetMaintenanceCostTotalAsync Tests

    [Test]
    public async Task GetMaintenanceCostTotalAsync_ShouldReturnZero_WhenNoRecordsForVehicle()
    {
        // Act
        var result = await _maintenanceService.GetMaintenanceCostTotalAsync(999);

        // Assert
        result.Should().Be(0);
    }

    [Test]
    public async Task GetMaintenanceCostTotalAsync_ShouldCalculateTotalCost_ForVehicle()
    {
        // Arrange
        var bus = new Bus
        {
            BusNumber = "009",
            LicenseNumber = "VWX456",
            Model = "Test Model",
            Year = 2020,
            SeatingCapacity = 50,
            Status = "Active",
            CurrentOdometer = 25000
        };
        await _context.Vehicles.AddAsync(bus);
        await _context.SaveChangesAsync();

        var maintenance1 = new Maintenance
        {
            Date = DateTime.UtcNow.AddDays(-5),
            VehicleId = bus.VehicleId,
            OdometerReading = 25100,
            MaintenanceCompleted = "Service 1",
            Vendor = "Test Vendor",
            RepairCost = 150.00m
        };

        var maintenance2 = new Maintenance
        {
            Date = DateTime.UtcNow.AddDays(-3),
            VehicleId = bus.VehicleId,
            OdometerReading = 25200,
            MaintenanceCompleted = "Service 2",
            Vendor = "Test Vendor",
            RepairCost = 250.00m
        };

        var maintenance3 = new Maintenance
        {
            Date = DateTime.UtcNow.AddDays(-1),
            VehicleId = bus.VehicleId,
            OdometerReading = 25300,
            MaintenanceCompleted = "Free Service",
            Vendor = "Test Vendor",
            RepairCost = 0.00m // Should be excluded from total
        };

        await _context.MaintenanceRecords.AddRangeAsync(maintenance1, maintenance2, maintenance3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _maintenanceService.GetMaintenanceCostTotalAsync(bus.VehicleId);

        // Assert
        result.Should().Be(400.00m); // 150.00 + 250.00 (excluding 0.00)
    }

    [Test]
    public async Task GetMaintenanceCostTotalAsync_ShouldCalculateTotalCost_WithDateRange()
    {
        // Arrange
        var bus = new Bus
        {
            BusNumber = "010",
            LicenseNumber = "YZA789",
            Model = "Test Model",
            Year = 2021,
            SeatingCapacity = 40,
            Status = "Active",
            CurrentOdometer = 5000
        };
        await _context.Vehicles.AddAsync(bus);
        await _context.SaveChangesAsync();

        var baseDate = DateTime.UtcNow.Date;
        var maintenance1 = new Maintenance
        {
            Date = baseDate.AddDays(-10), // Outside range
            VehicleId = bus.VehicleId,
            OdometerReading = 5100,
            MaintenanceCompleted = "Outside Range Service",
            Vendor = "Test Vendor",
            RepairCost = 100.00m
        };

        var maintenance2 = new Maintenance
        {
            Date = baseDate.AddDays(-3), // Inside range
            VehicleId = bus.VehicleId,
            OdometerReading = 5200,
            MaintenanceCompleted = "Inside Range Service 1",
            Vendor = "Test Vendor",
            RepairCost = 150.00m
        };

        var maintenance3 = new Maintenance
        {
            Date = baseDate.AddDays(-1), // Inside range
            VehicleId = bus.VehicleId,
            OdometerReading = 5300,
            MaintenanceCompleted = "Inside Range Service 2",
            Vendor = "Test Vendor",
            RepairCost = 200.00m
        };

        await _context.MaintenanceRecords.AddRangeAsync(maintenance1, maintenance2, maintenance3);
        await _context.SaveChangesAsync();

        var startDate = baseDate.AddDays(-5);
        var endDate = baseDate;

        // Act
        var result = await _maintenanceService.GetMaintenanceCostTotalAsync(bus.VehicleId, startDate, endDate);

        // Assert
        result.Should().Be(350.00m); // 150.00 + 200.00 (excluding 100.00 outside range)
    }

    [Test]
    public async Task GetMaintenanceCostTotalAsync_ShouldCalculateTotalCost_WithStartDateOnly()
    {
        // Arrange
        var bus = new Bus
        {
            BusNumber = "011",
            LicenseNumber = "BCD123",
            Model = "Test Model",
            Year = 2022,
            SeatingCapacity = 35,
            Status = "Active",
            CurrentOdometer = 1000
        };
        await _context.Vehicles.AddAsync(bus);
        await _context.SaveChangesAsync();

        var baseDate = DateTime.UtcNow.Date;
        var maintenance1 = new Maintenance
        {
            Date = baseDate.AddDays(-10), // Before start date
            VehicleId = bus.VehicleId,
            OdometerReading = 1100,
            MaintenanceCompleted = "Before Start Service",
            Vendor = "Test Vendor",
            RepairCost = 100.00m
        };

        var maintenance2 = new Maintenance
        {
            Date = baseDate.AddDays(-2), // After start date
            VehicleId = bus.VehicleId,
            OdometerReading = 1200,
            MaintenanceCompleted = "After Start Service",
            Vendor = "Test Vendor",
            RepairCost = 200.00m
        };

        await _context.MaintenanceRecords.AddRangeAsync(maintenance1, maintenance2);
        await _context.SaveChangesAsync();

        var startDate = baseDate.AddDays(-5);

        // Act
        var result = await _maintenanceService.GetMaintenanceCostTotalAsync(bus.VehicleId, startDate, null);

        // Assert
        result.Should().Be(200.00m); // Only maintenance2
    }

    [Test]
    public async Task GetMaintenanceCostTotalAsync_ShouldCalculateTotalCost_WithEndDateOnly()
    {
        // Arrange
        var bus = new Bus
        {
            BusNumber = "012",
            LicenseNumber = "EFG456",
            Model = "Test Model",
            Year = 2023,
            SeatingCapacity = 30,
            Status = "Active",
            CurrentOdometer = 500
        };
        await _context.Vehicles.AddAsync(bus);
        await _context.SaveChangesAsync();

        var baseDate = DateTime.UtcNow.Date;
        var maintenance1 = new Maintenance
        {
            Date = baseDate.AddDays(-5), // Before end date
            VehicleId = bus.VehicleId,
            OdometerReading = 600,
            MaintenanceCompleted = "Before End Service",
            Vendor = "Test Vendor",
            RepairCost = 150.00m
        };

        var maintenance2 = new Maintenance
        {
            Date = baseDate.AddDays(1), // After end date
            VehicleId = bus.VehicleId,
            OdometerReading = 700,
            MaintenanceCompleted = "After End Service",
            Vendor = "Test Vendor",
            RepairCost = 300.00m
        };

        await _context.MaintenanceRecords.AddRangeAsync(maintenance1, maintenance2);
        await _context.SaveChangesAsync();

        var endDate = baseDate;

        // Act
        var result = await _maintenanceService.GetMaintenanceCostTotalAsync(bus.VehicleId, null, endDate);

        // Assert
        result.Should().Be(150.00m); // Only maintenance1
    }

    #endregion
}
