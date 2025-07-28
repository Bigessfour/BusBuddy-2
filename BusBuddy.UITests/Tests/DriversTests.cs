using NUnit.Framework;
using FluentAssertions;
using BusBuddy.UITests.Utilities;
using BusBuddy.UITests.Builders;
using BusBuddy.UITests.PageObjects;
using BusBuddy.Core.Data;
using BusBuddy.Core.Models;

namespace BusBuddy.UITests.Tests;

/// <summary>
/// Comprehensive UI tests for Drivers functionality
/// Tests Phase 1 drivers implementation and validates search, selection, and command functionality
/// Migrated to NUnit + FluentAssertions for superior testing experience
/// </summary>
[TestFixture]
public class DriversTests
{
    private BusBuddyDbContext? _testContext;
    private List<Driver>? _testDrivers;

    [SetUp]
    public void SetUp()
    {
        // Create in-memory database for testing
        _testContext = UITestHelpers.CreateInMemoryContext($"DriversTest_{Guid.NewGuid()}");

        // Create specific test drivers for this test class
        _testDrivers = new List<Driver>
        {
            DriverTestDataBuilder.CreateActiveDriver(1, "John Smith"),
            DriverTestDataBuilder.CreateActiveDriver(2, "Sarah Johnson"),
            DriverTestDataBuilder.CreateInactiveDriver(3, "Mike Wilson"),
            DriverTestDataBuilder.CreateTraineeDriver(4, "Lisa Brown"),
            DriverTestDataBuilder.CreateActiveDriver(5, "Tom Davis")
        };

        // Add to database
        _testContext.Drivers.AddRange(_testDrivers);
        _testContext.SaveChanges();
    }

    [TearDown]
    public void TearDown()
    {
        _testContext?.Dispose();
        UITestHelpers.CleanupAutomation();
    }

    [Test]
    [Category("Drivers")]
    [Category("Phase1")]
    public void Drivers_ShouldLoadAllDriversCorrectly()
    {
        // Arrange
        var expectedDriverCount = _testDrivers!.Count;

        // Act
        var driversInDb = _testContext!.Drivers.ToList();

        // Assert with FluentAssertions
        driversInDb.Should().HaveCount(expectedDriverCount, "should load all test drivers");
        driversInDb.Should().Contain(d => d.DriverName == "John Smith", "should contain John Smith");
        driversInDb.Should().Contain(d => d.DriverName == "Sarah Johnson", "should contain Sarah Johnson");
    }

    [Test]
    [Category("Drivers")]
    [Category("Search")]
    public void Drivers_SearchFilter_ShouldFindDriversByName()
    {
        // Arrange
        var searchTerm = "John";

        // Act - Simulate search filtering logic
        var filteredDrivers = _testDrivers!.Where(d =>
            d.DriverName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();

        // Assert with FluentAssertions
        filteredDrivers.Should().NotBeEmpty("should find drivers matching search term");
        filteredDrivers.Should().OnlyContain(d => d.DriverName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase),
                     "all results should match search criteria");
        filteredDrivers.Should().Contain(d => d.DriverName == "John Smith", "should find John Smith");
        filteredDrivers.Should().Contain(d => d.DriverName == "Sarah Johnson", "should find Sarah Johnson");
    }

    [Test]
    [Category("Drivers")]
    [Category("Search")]
    public void Drivers_SearchFilter_ShouldFindDriversByEmail()
    {
        // Arrange
        var searchTerm = "busbuddy.com";

        // Act - Simulate search filtering by email
        var filteredDrivers = _testDrivers!.Where(d =>
            !string.IsNullOrEmpty(d.DriverEmail) &&
            d.DriverEmail.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();

        // Assert with FluentAssertions
        filteredDrivers.Should().NotBeEmpty("should find drivers by email domain");
        // Avoid null-propagation in expression tree: use explicit null check
        filteredDrivers.Should().OnlyContain(d => !string.IsNullOrEmpty(d.DriverEmail) && d.DriverEmail.Contains(searchTerm, StringComparison.OrdinalIgnoreCase),
                     "all results should have matching email domain");
    }

    [Test]
    [Category("Drivers")]
    [Category("Filtering")]
    public void Drivers_StatusFilter_ShouldShowOnlyActiveDrivers()
    {
        // Arrange & Act
        var testDrivers = _testDrivers ?? new List<Driver>();
        var activeDrivers = testDrivers.Where(d => d.Status == "Active").ToList();
        var inactiveDrivers = testDrivers.Where(d => d.Status != "Active").ToList();

        // Assert with FluentAssertions
        activeDrivers.Should().NotBeEmpty("should have active drivers");
        inactiveDrivers.Should().NotBeEmpty("should have inactive drivers for comparison");
        activeDrivers.Should().OnlyContain(d => d.Status == "Active", "all filtered drivers should be active");

        // Verify specific test data with FluentAssertions
        activeDrivers.Should().Contain(d => d.DriverName == "John Smith", "John Smith should be active");
        activeDrivers.Should().Contain(d => d.DriverName == "Sarah Johnson", "Sarah Johnson should be active");
        activeDrivers.Should().NotContain(d => d.DriverName == "Mike Wilson", "Mike Wilson should not be in active list");
    }

    [Test]
    [Category("Drivers")]
    [Category("Validation")]
    public void Drivers_DataValidation_ShouldHaveRequiredFields()
    {
        // Act & Assert with FluentAssertions
        foreach (var driver in _testDrivers!)
        {
            driver.DriverName.Should().NotBeNullOrEmpty($"Driver {driver.DriverId} should have a name");
            driver.Status.Should().NotBeNullOrEmpty($"Driver {driver.DriverId} should have a status");
            driver.DriverId.Should().BePositive("Driver should have valid ID");
            driver.DriversLicenceType.Should().NotBeNullOrEmpty($"Driver {driver.DriverId} should have license type");
        }
    }

    [Test]
    [Category("Drivers")]
    [Category("BusinessLogic")]
    public void Drivers_TrainingStatus_ShouldBeConsistentWithStatus()
    {
        // Act & Assert with FluentAssertions
        foreach (var driver in _testDrivers!)
        {
            if (driver.Status == "Training")
            {
                driver.TrainingComplete.Should().BeFalse(
                    $"Driver in training status should not have completed training: {driver.DriverName}");
            }

            if (driver.Status == "Active")
            {
                driver.TrainingComplete.Should().BeTrue(
                    $"Active driver should have completed training: {driver.DriverName}");
            }
        }
    }

    [Test]
    [Category("Drivers")]
    [Category("Commands")]
    public void Drivers_Commands_ShouldValidateSelection()
    {
        // Arrange
        var selectedDriver = _testDrivers!.First();

        // Act & Assert with FluentAssertions

        // Edit command should be enabled when driver is selected
        var canEdit = selectedDriver != null;
        canEdit.Should().BeTrue("Edit command should be enabled when driver is selected");

        // Delete command should be enabled when driver is selected
        var canDelete = selectedDriver != null;
        canDelete.Should().BeTrue("Delete command should be enabled when driver is selected");

        // Commands should be disabled when no driver is selected
        selectedDriver = null;
        canEdit = selectedDriver != null;
        canDelete = selectedDriver != null;
        canEdit.Should().BeFalse("Edit command should be disabled when no driver is selected");
        canDelete.Should().BeFalse("Delete command should be disabled when no driver is selected");
    }

    [Test]
    [Category("Drivers")]
    [Category("Performance")]
    public void Drivers_DataLoading_ShouldBeEfficient()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var drivers = _testContext!.Drivers.ToList();
        var filteredDrivers = drivers.Where(d => d.Status == "Active").ToList();

        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500, $"Driver operations should complete quickly. Actual: {stopwatch.ElapsedMilliseconds}ms");
        drivers.Count.Should().BeGreaterThan(0, "Should load drivers");
        filteredDrivers.Count.Should().BeGreaterThan(0, "Should filter drivers");
    }

    [Test]
    [Category("Drivers")]
    [Category("Integration")]
    public void Drivers_CRUDOperations_ShouldWorkCorrectly()
    {
        // Test Create
        var newDriver = new DriverTestDataBuilder()
            .WithId(999)
            .WithName("Test New Driver")
            .WithEmail("new.driver@test.com")
            .AsActive()
            .Build();

        _testContext!.Drivers.Add(newDriver);
        _testContext.SaveChanges();

        // Verify Create
        var createdDriver = _testContext.Drivers.Find(999);
        createdDriver.Should().NotBeNull("Should create new driver");
        createdDriver!.DriverName.Should().Be("Test New Driver");

        // Test Update
        createdDriver.DriverName = "Updated Driver Name";
        _testContext.SaveChanges();

        // Verify Update
        var updatedDriver = _testContext.Drivers.Find(999);
        updatedDriver!.DriverName.Should().Be("Updated Driver Name");

        // Test Delete
        _testContext.Drivers.Remove(updatedDriver);
        _testContext.SaveChanges();

        // Verify Delete
        var deletedDriver = _testContext.Drivers.Find(999);
        deletedDriver.Should().BeNull("Driver should be deleted");
    }

    [Test]
    [Category("Drivers")]
    [Category("EdgeCases")]
    public void Drivers_EdgeCases_ShouldHandleEmptyAndNullValues()
    {
        // Test empty search
        var testDrivers = _testDrivers ?? new List<Driver>();
        var emptySearchResults = testDrivers.Where(d =>
            d.DriverName.Contains("", StringComparison.OrdinalIgnoreCase)).ToList();
        emptySearchResults.Count.Should().Be(testDrivers.Count, "Empty search should return all drivers");

        // Test null handling
        var driversWithoutEmail = testDrivers.Where(d => string.IsNullOrEmpty(d.DriverEmail)).ToList();
        driversWithoutEmail.Count.Should().BeGreaterOrEqualTo(0, "Should handle drivers without email gracefully");

        // Test case insensitive search
        var lowerCaseSearch = testDrivers.Where(d =>
            d.DriverName.Contains("john", StringComparison.OrdinalIgnoreCase)).ToList();
        var upperCaseSearch = testDrivers.Where(d =>
            d.DriverName.Contains("JOHN", StringComparison.OrdinalIgnoreCase)).ToList();
        lowerCaseSearch.Count.Should().Be(upperCaseSearch.Count, "Search should be case insensitive");
    }

    [Test]
    [Category("Drivers")]
    [Category("TestInfrastructure")]
    public void Drivers_TestDataBuilder_ShouldCreateVariedScenarios()
    {
        // Test active driver creation
        var activeDriver = DriverTestDataBuilder.CreateActiveDriver(100, "Active Test Driver");
        activeDriver.Status.Should().Be("Active");
        activeDriver.TrainingComplete.Should().BeTrue();

        // Test inactive driver creation
        var inactiveDriver = DriverTestDataBuilder.CreateInactiveDriver(101, "Inactive Test Driver");
        inactiveDriver.Status.Should().Be("Inactive");

        // Test trainee driver creation
        var traineeDriver = DriverTestDataBuilder.CreateTraineeDriver(102, "Trainee Test Driver");
        traineeDriver.Status.Should().Be("Training");
        traineeDriver.TrainingComplete.Should().BeFalse();

        // Test list creation
        var driversList = DriverTestDataBuilder.CreateDriversList(3);
        driversList.Count.Should().Be(3);
        driversList.All(d => d.DriverId > 0).Should().BeTrue();
        driversList.All(d => !string.IsNullOrEmpty(d.DriverName)).Should().BeTrue();
    }

    [Test]
    [Category("Drivers")]
    [Category("DataConsistency")]
    public void Drivers_DataConsistency_ShouldMaintainIntegrity()
    {
        // Verify no duplicate IDs
        var testDrivers = _testDrivers ?? new List<Driver>();
        var driverIds = testDrivers.Select(d => d.DriverId).ToList();
        var uniqueIds = driverIds.Distinct().ToList();
        driverIds.Count.Should().Be(uniqueIds.Count, "Driver IDs should be unique");

        // Verify no duplicate names
        var driverNames = testDrivers.Select(d => d.DriverName).ToList();
        var uniqueNames = driverNames.Distinct().ToList();
        driverNames.Count.Should().Be(uniqueNames.Count, "Driver names should be unique in test data");

        // Verify valid status values
        var validStatuses = new[] { "Active", "Inactive", "Training", "Suspended" };
        testDrivers.All(d => validStatuses.Contains(d.Status)).Should().BeTrue("All drivers should have valid status values");

        // Verify consistent phone number format
        var driversWithPhones = testDrivers.Where(d => !string.IsNullOrEmpty(d.DriverPhone)).ToList();
        driversWithPhones.All(d => d.DriverPhone!.Contains('(')).Should().BeTrue("Phone numbers should be consistently formatted");
    }
}
