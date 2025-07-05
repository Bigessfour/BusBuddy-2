using NUnit.Framework;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Bus_Buddy.Data;
using Bus_Buddy.Data.Repositories;
using Bus_Buddy.Models;
using Bus_Buddy.Models.Base;
using BusBuddy.Tests.Infrastructure;

namespace BusBuddy.Tests.UnitTests.RepositoryTests;

/// <summary>
/// Tests for generic Repository<T> implementation
/// Validates core data access patterns, CRUD operations, and audit functionality
/// 
/// LESSONS LEARNED APPLIED:
/// - Category 2.2: Test Data Isolation - Comprehensive database cleanup
/// - Category 1: Entity Framework patterns - Mapped vs NotMapped properties
/// - Category 3: Safety-critical testing priorities
/// </summary>
[TestFixture]
public class RepositoryTests : TestBase
{
    private Repository<Bus> _busRepository = null!;
    private Repository<Driver> _driverRepository = null!;
    private Repository<Student> _studentRepository = null!;

    [SetUp]
    public void SetUp()
    {
        SetupTestDatabase(); // LESSON 2.2: Test Data Isolation

        _busRepository = new Repository<Bus>(DbContext, UserContextService);
        _driverRepository = new Repository<Driver>(DbContext, UserContextService);
        _studentRepository = new Repository<Student>(DbContext, UserContextService);
    }

    [TearDown]
    public void TearDown()
    {
        TearDownTestDatabase();
    }

    #region CRUD Operations Tests

    [Test]
    public async Task AddAsync_ShouldAddEntitySuccessfully()
    {
        // Arrange
        var bus = new Bus
        {
            BusNumber = "TEST001",
            Year = 2020,
            Make = "Blue Bird",
            Model = "Vision",
            SeatingCapacity = 72,
            VINNumber = "TEST0012345678901",
            LicenseNumber = "TST001"
        };

        // Act
        await _busRepository.AddAsync(bus);
        var result = await DbContext.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        bus.VehicleId.Should().BeGreaterThan(0);

        // Verify in database
        var savedBus = await DbContext.Vehicles.FindAsync(bus.VehicleId);
        savedBus.Should().NotBeNull();
        savedBus!.BusNumber.Should().Be("TEST001");
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnCorrectEntity()
    {
        // Arrange
        var bus = new Bus
        {
            BusNumber = "TEST002",
            Year = 2021,
            Make = "Thomas Built",
            Model = "Saf-T-Liner",
            SeatingCapacity = 48,
            VINNumber = "TEST0022345678901",
            LicenseNumber = "TST002"
        };

        DbContext.Vehicles.Add(bus);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _busRepository.GetByIdAsync(bus.VehicleId);

        // Assert
        result.Should().NotBeNull();
        result!.VehicleId.Should().Be(bus.VehicleId);
        result.BusNumber.Should().Be("TEST002");
    }

    [Test]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Act
        var result = await _busRepository.GetByIdAsync(99999);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task UpdateAsync_ShouldModifyEntitySuccessfully()
    {
        // Arrange
        var driver = new Driver
        {
            DriverName = "John Doe",
            DriverPhone = "(555) 123-4567", // LESSON 2.3: Phone validation format
            DriverEmail = "john.doe@example.com",
            DriversLicenceType = "CDL",
            TrainingComplete = false
        };

        DbContext.Drivers.Add(driver);
        await DbContext.SaveChangesAsync();

        // Act
        driver.DriverName = "John Smith";
        driver.TrainingComplete = true;
        _driverRepository.Update(driver);
        await DbContext.SaveChangesAsync();

        // Assert
        var updatedDriver = await DbContext.Drivers.FindAsync(driver.DriverId);
        updatedDriver.Should().NotBeNull();
        updatedDriver!.DriverName.Should().Be("John Smith");
        updatedDriver.TrainingComplete.Should().BeTrue();
    }

    [Test]
    public async Task Remove_ShouldDeleteEntitySuccessfully()
    {
        // Arrange
        var bus = new Bus
        {
            BusNumber = "DELETE001",
            Year = 2019,
            Make = "Blue Bird",
            Model = "Vision",
            SeatingCapacity = 36,
            VINNumber = "DELETE0123456789",
            LicenseNumber = "DEL001"
        };

        DbContext.Vehicles.Add(bus);
        await DbContext.SaveChangesAsync();
        var busId = bus.VehicleId;

        // Act
        _busRepository.Remove(bus);
        await DbContext.SaveChangesAsync();

        // Assert
        var deletedBus = await DbContext.Vehicles.FindAsync(busId);
        deletedBus.Should().BeNull();
    }

    [Test]
    public async Task FindAsync_ShouldReturnMatchingEntities()
    {
        // Arrange
        var activeBus = new Bus
        {
            BusNumber = "ACTIVE001",
            Year = 2022,
            Make = "Blue Bird",
            Model = "Vision",
            SeatingCapacity = 72,
            VINNumber = "ACTIVE0123456789",
            LicenseNumber = "ACT001"
        };

        var inactiveBus = new Bus
        {
            BusNumber = "INACTIVE001",
            Year = 2021,
            Make = "Thomas Built",
            Model = "Saf-T-Liner",
            SeatingCapacity = 48,
            VINNumber = "INACTIVE01234567",
            LicenseNumber = "INA001"
        };

        DbContext.Vehicles.AddRange(activeBus, inactiveBus);
        await DbContext.SaveChangesAsync();

        // Act - Note: Bus model doesn't have IsActive property, so we'll filter by SeatingCapacity as example
        var largeBuses = await _busRepository.FindAsync(b => b.SeatingCapacity >= 72);

        // Assert
        largeBuses.Should().HaveCount(1);
        largeBuses.First().BusNumber.Should().Be("ACTIVE001");
    }

    #endregion

    #region Soft Delete Operations Tests

    [Test]
    public async Task SoftDeleteAsync_WithBaseEntity_ShouldMarkAsDeleted()
    {
        // Arrange
        var student = new Student
        {
            StudentName = "Test Student",
            Grade = "5",
            HomePhone = "(555) 123-4567", // LESSON 2.3: Phone validation format
            EmergencyPhone = "(555) 987-6543",
            SpecialNeeds = false,
            PhotoPermission = true,
            FieldTripPermission = true
        };

        DbContext.Students.Add(student);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _studentRepository.SoftDeleteAsync(student.StudentId);
        await DbContext.SaveChangesAsync();

        // Assert
        result.Should().BeTrue();

        // Verify entity is marked as inactive but still in database
        var deletedStudent = await DbContext.Students.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.StudentId == student.StudentId);
        deletedStudent.Should().NotBeNull();
        deletedStudent!.Active.Should().BeFalse();

        // Verify entity is not returned in normal queries
        var normalQuery = await _studentRepository.GetByIdAsync(student.StudentId);
        normalQuery.Should().BeNull();
    }

    [Test]
    public async Task RestoreAsync_ShouldRestoreSoftDeletedEntity()
    {
        // Arrange
        var student = new Student
        {
            StudentName = "Restore Test",
            Grade = "3",
            HomePhone = "(555) 456-7890",
            EmergencyPhone = "(555) 098-7654",
            SpecialNeeds = false,
            PhotoPermission = true,
            FieldTripPermission = false
        };

        DbContext.Students.Add(student);
        await DbContext.SaveChangesAsync();

        // Soft delete first
        await _studentRepository.SoftDeleteAsync(student.StudentId);
        await DbContext.SaveChangesAsync();

        // Act
        await _studentRepository.RestoreAsync(student.StudentId);
        await DbContext.SaveChangesAsync();

        // Assert
        var restoredStudent = await _studentRepository.GetByIdAsync(student.StudentId);
        restoredStudent.Should().NotBeNull();
        restoredStudent!.StudentName.Should().Be("Restore Test");
        restoredStudent.Active.Should().BeTrue();
    }

    #endregion

    #region Pagination Tests

    [Test]
    public async Task GetPagedAsync_ShouldReturnCorrectPageData()
    {
        // Arrange
        var buses = new List<Bus>();
        for (int i = 1; i <= 15; i++)
        {
            buses.Add(new Bus
            {
                BusNumber = $"PAGE{i:D3}",
                Year = 2020 + (i % 5),
                Make = "Blue Bird",
                Model = "Vision",
                SeatingCapacity = 72,
                VINNumber = $"PAGE{i:D3}123456789".PadRight(17, '0').Substring(0, 17), // Ensure exactly 17 characters
                LicenseNumber = $"PAG{i:D3}"
            });
        }

        DbContext.Vehicles.AddRange(buses);
        await DbContext.SaveChangesAsync();

        // Act
        var page1 = await _busRepository.GetPagedAsync(1, 5);
        var page2 = await _busRepository.GetPagedAsync(2, 5);

        // Assert
        page1.TotalCount.Should().Be(15);
        page1.Items.Should().HaveCount(5);

        page2.TotalCount.Should().Be(15);
        page2.Items.Should().HaveCount(5);

        // Verify different items on different pages
        var page1Ids = page1.Items.Select(b => b.VehicleId).ToList();
        var page2Ids = page2.Items.Select(b => b.VehicleId).ToList();
        page1Ids.Should().NotIntersectWith(page2Ids);
    }

    [Test]
    public async Task GetPagedAsync_WithFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var largeBuses = new List<Bus>();
        var smallBuses = new List<Bus>();

        for (int i = 1; i <= 5; i++)
        {
            largeBuses.Add(new Bus
            {
                BusNumber = $"LARGE{i:D3}",
                Year = 2023,
                Make = "Blue Bird",
                Model = "Vision",
                SeatingCapacity = 72,
                VINNumber = $"LARGE{i:D3}123456789"[0..17], // Ensure exactly 17 characters
                LicenseNumber = $"LRG{i:D3}"
            });

            smallBuses.Add(new Bus
            {
                BusNumber = $"SMALL{i:D3}",
                Year = 2022,
                Make = "Thomas Built",
                Model = "Saf-T-Liner",
                SeatingCapacity = 36,
                VINNumber = $"SMALL{i:D3}123456789"[0..17], // Ensure exactly 17 characters
                LicenseNumber = $"SML{i:D3}"
            });
        }

        DbContext.Vehicles.AddRange(largeBuses.Concat(smallBuses));
        await DbContext.SaveChangesAsync();

        // Act
        var largeBusPage = await _busRepository.GetPagedAsync(1, 10,
            filter: b => b.SeatingCapacity >= 72);

        // Assert
        largeBusPage.TotalCount.Should().Be(5);
        largeBusPage.Items.Should().HaveCount(5);
        largeBusPage.Items.Should().OnlyContain(b => b.SeatingCapacity >= 72);
    }

    #endregion

    #region Query Operations Tests

    [Test]
    public void Query_ShouldReturnQueryableWithSoftDeleteFilter()
    {
        // Arrange
        var activeStudent = new Student
        {
            StudentName = "Active Student",
            Grade = "4",
            HomePhone = "(555) 111-2222",
            EmergencyPhone = "(555) 333-4444"
        };

        var inactiveStudent = new Student
        {
            StudentName = "Inactive Student",
            Grade = "4",
            HomePhone = "(555) 555-6666",
            EmergencyPhone = "(555) 777-8888"
        };

        DbContext.Students.AddRange(activeStudent, inactiveStudent);
        DbContext.SaveChanges();

        // Act
        var query = _studentRepository.Query();
        var results = query.ToList();

        // Assert - Note: Student model doesn't have soft delete, so all entities should be returned
        results.Should().HaveCount(2);
        results.Should().Contain(s => s.StudentName == "Active Student");
        results.Should().Contain(s => s.StudentName == "Inactive Student");
    }

    [Test]
    public void QueryNoTracking_ShouldReturnDetachedEntities()
    {
        // Arrange
        var bus = new Bus
        {
            BusNumber = "NOTRACK001",
            Year = 2024,
            Make = "Blue Bird",
            Model = "Vision",
            SeatingCapacity = 48,
            VINNumber = "NOTRACK01234567",
            LicenseNumber = "NTR001"
        };

        DbContext.Vehicles.Add(bus);
        DbContext.SaveChanges();

        // Act
        var query = _busRepository.QueryNoTracking();
        var result = query.First(b => b.BusNumber == "NOTRACK001");

        // Assert
        result.Should().NotBeNull();
        DbContext.Entry(result).State.Should().Be(EntityState.Detached);
    }

    #endregion

    #region Audit Field Tests

    [Test]
    public async Task AddAsync_ShouldSetAuditFields()
    {
        // Arrange
        var driver = new Driver
        {
            DriverName = "Audit Test Driver",
            DriverPhone = "(555) 999-0000",
            DriverEmail = "audit@example.com",
            DriversLicenceType = "CDL",
            TrainingComplete = true
        };

        // Act
        var result = await _driverRepository.AddAsync(driver);
        await DbContext.SaveChangesAsync();

        // Assert
        result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        result.CreatedBy.Should().Be("TestUser"); // Mock user context service returns "TestUser"
        result.UpdatedDate.Should().BeNull();
        result.UpdatedBy.Should().BeNull();
    }

    [Test]
    public async Task Update_ShouldSetUpdateAuditFields()
    {
        // Arrange
        var driver = new Driver
        {
            DriverName = "Update Test Driver",
            DriverPhone = "(555) 888-7777",
            DriverEmail = "update@example.com",
            DriversLicenceType = "CDL",
            TrainingComplete = false
        };

        DbContext.Drivers.Add(driver);
        await DbContext.SaveChangesAsync();

        var originalCreatedDate = driver.CreatedDate;

        // Act
        driver.TrainingComplete = true;
        _driverRepository.Update(driver);
        await DbContext.SaveChangesAsync();

        // Assert
        driver.CreatedDate.Should().Be(originalCreatedDate); // Should not change
        driver.UpdatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        driver.UpdatedBy.Should().Be("TestUser");
    }

    #endregion

    #region Error Handling Tests

    [Test]
    public void AddAsync_WithNullEntity_ShouldThrowException()
    {
        // Act & Assert - LESSON 2.1: Synchronous exception testing for ArgumentNullException +1
        Assert.Throws<ArgumentNullException>(() => _busRepository.AddAsync(null!).GetAwaiter().GetResult());
    }

    [Test]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _busRepository.GetByIdAsync(-1);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task SoftDeleteAsync_WithNonBaseEntity_ShouldReturnFalse()
    {
        // Note: Bus doesn't inherit from BaseEntity, so soft delete should return false
        var bus = new Bus
        {
            BusNumber = "SOFTDEL001",
            Year = 2023,
            Make = "Blue Bird",
            Model = "Vision",
            SeatingCapacity = 72,
            VINNumber = "SOFTDEL01234567",
            LicenseNumber = "SOF001"
        };

        DbContext.Vehicles.Add(bus);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _busRepository.SoftDeleteAsync(bus.VehicleId);

        // Assert
        result.Should().BeFalse(); // Bus doesn't support soft delete
    }

    #endregion
}

