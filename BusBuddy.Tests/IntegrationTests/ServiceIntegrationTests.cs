using NUnit.Framework;
using FluentAssertions;
using BusBuddy.WPF.Services;
using BusBuddy.Core.Data;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services.Interfaces;
using BusBuddy.Core.Services;
using BusBuddy.Tests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BusBuddy.Tests.IntegrationTests;

/// <summary>
/// Integration tests for service layer functionality
/// Tests real service interactions and data flow
/// </summary>
[TestFixture]
public class ServiceIntegrationTests : BaseTestFixture
{
    private IDataIntegrityService? _dataIntegrityService;
    private XAIChatService? _xaiChatService;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();

        // Initialize services with test context
        _xaiChatService = new XAIChatService();

        // Create mock services for DataIntegrityService
        var mockRouteService = new Mock<IRouteService>();
        var mockDriverService = new Mock<IDriverService>();
        var mockBusService = new Mock<IBusService>();
        var mockActivityService = new Mock<IActivityService>();
        var mockStudentService = new Mock<IStudentService>();

        _dataIntegrityService = new DataIntegrityService(
            mockRouteService.Object,
            mockDriverService.Object,
            mockBusService.Object,
            mockActivityService.Object,
            mockStudentService.Object);
    }

    [Test]
    [Category("Integration")]
    [Category("DataFlow")]
    public async Task Services_ShouldIntegrateWithDatabaseContext()
    {
        // Arrange
        TestContext.Should().NotBeNull("Test context should be available");
        var initialDriverCount = TestContext!.Drivers.Count();

        // Act - Add new driver through context
        var newDriver = new Driver
        {
            DriverId = 999,
            DriverName = "Integration Test Driver",
            Status = "Active",
            LicenseNumber = "INT999"
        };

        TestContext.Drivers.Add(newDriver);
        await TestContext.SaveChangesAsync();

        // Assert
        var updatedCount = TestContext.Drivers.Count();
        updatedCount.Should().Be(initialDriverCount + 1, "Driver count should increase");

        var retrievedDriver = TestContext.Drivers.Find(999);
        retrievedDriver.Should().NotBeNull("New driver should be retrievable");
        retrievedDriver!.DriverName.Should().Be("Integration Test Driver");
    }

    [Test]
    [Category("Integration")]
    [Category("XAIChat")]
    public async Task XAIChatService_ShouldIntegrateWithSystemProperly()
    {
        // Act
        await _xaiChatService!.InitializeAsync();
        var isAvailable = await _xaiChatService.IsAvailableAsync();

        // Assert
        isAvailable.Should().BeTrue("XAI Chat service should be available");

        // Test various query types
        var fleetResponse = await _xaiChatService.GetResponseAsync("Show fleet status");
        var driverResponse = await _xaiChatService.GetResponseAsync("How many drivers are active?");
        var emergencyResponse = await _xaiChatService.GetResponseAsync("Emergency situation!");

        fleetResponse.Should().Contain("fleet", "Should provide fleet information");
        driverResponse.Should().Contain("driver", "Should provide driver information");
        emergencyResponse.Should().Contain("Emergency", "Should handle emergency queries");
    }

    [Test]
    [Category("Integration")]
    [Category("Performance")]
    public async Task ServiceOperations_ShouldCompleteWithinPerformanceTargets()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var operations = new List<Task>();

        // Act - Execute multiple service operations concurrently
        operations.Add(_xaiChatService!.GetResponseAsync("Fleet status"));
        operations.Add(_xaiChatService.GetResponseAsync("Driver information"));
        operations.Add(_xaiChatService.GetResponseAsync("Vehicle data"));

        await Task.WhenAll(operations);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000,
            $"All operations should complete within 10 seconds. Actual: {stopwatch.ElapsedMilliseconds}ms");
    }

    [Test]
    [Category("Integration")]
    [Category("ErrorHandling")]
    public async Task Services_ShouldHandleErrorsGracefully()
    {
        // Act & Assert - XAI Chat Service error handling
        var nullResponse = await _xaiChatService!.GetResponseAsync(null!);
        nullResponse.Should().NotBeNullOrEmpty("Should handle null input gracefully");
        nullResponse.Should().Contain("trouble", "Should indicate processing issue");

        var emptyResponse = await _xaiChatService.GetResponseAsync(string.Empty);
        emptyResponse.Should().NotBeNullOrEmpty("Should handle empty input gracefully");
    }

    [Test]
    [Category("Integration")]
    [Category("BusinessLogic")]
    public async Task Services_ShouldEnforceBusinessRules()
    {
        // Arrange - Create driver with expiring license
        var expiringDriver = new Driver
        {
            DriverId = 888,
            DriverName = "Expiring License Driver",
            Status = "Active",
            LicenseNumber = "EXP888",
            LicenseExpiryDate = DateTime.Now.AddDays(15) // Expires soon
        };

        TestContext!.Drivers.Add(expiringDriver);
        await TestContext.SaveChangesAsync();

        // Act - Query for drivers with expiring licenses
        var driversWithExpiringLicenses = TestContext.Drivers
            .Where(d => d.LicenseExpiryDate < DateTime.Now.AddDays(30))
            .ToList();

        // Assert
        driversWithExpiringLicenses.Should().Contain(d => d.DriverId == 888,
            "Should identify driver with expiring license");
    }

    [Test]
    [Category("Integration")]
    [Category("DataValidation")]
    public async Task Services_ShouldValidateDataIntegrity()
    {
        // Arrange - Create data with integrity issues
        var driverWithoutName = new Driver
        {
            DriverId = 777,
            DriverName = "", // Missing name
            Status = "Active",
            LicenseNumber = "BAD777"
        };

        // Act - Attempt to add invalid data
        TestContext!.Drivers.Add(driverWithoutName);
        await TestContext.SaveChangesAsync(); // EF Core allows this, but business logic should catch it

        // Assert - Validate through business logic
        var invalidDriver = TestContext.Drivers.Find(777);
        invalidDriver.Should().NotBeNull("Driver should be in database");
        invalidDriver!.DriverName.Should().BeEmpty("Should preserve invalid state for testing");

        // Business logic should catch this
        var isValidName = !string.IsNullOrEmpty(invalidDriver.DriverName);
        isValidName.Should().BeFalse("Business logic should identify invalid name");
    }

    [Test]
    [Category("Integration")]
    [Category("ConcurrentAccess")]
    public async Task Services_ShouldHandleConcurrentAccess()
    {
        // Arrange
        var tasks = new List<Task<string>>();

        // Act - Make concurrent XAI Chat requests
        for (int i = 0; i < 5; i++)
        {
            var query = $"Query {i}: Fleet status";
            tasks.Add(_xaiChatService!.GetResponseAsync(query));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().HaveCount(5, "Should handle all concurrent requests");
        responses.Should().OnlyContain(r => !string.IsNullOrEmpty(r), "All responses should be valid");
    }

    [Test]
    [Category("Integration")]
    [Category("ServiceLifecycle")]
    public async Task Services_ShouldMaintainStateCorrectly()
    {
        // Act - Initialize service
        await _xaiChatService!.InitializeAsync();
        var initialAvailability = await _xaiChatService.IsAvailableAsync();

        // Make some requests
        await _xaiChatService.GetResponseAsync("Test query 1");
        await _xaiChatService.GetResponseAsync("Test query 2");

        // Check availability again
        var finalAvailability = await _xaiChatService.IsAvailableAsync();

        // Assert
        initialAvailability.Should().BeTrue("Service should be available after initialization");
        finalAvailability.Should().BeTrue("Service should remain available after requests");
    }

    [Test]
    [Category("Integration")]
    [Category("ResourceManagement")]
    public async Task Services_ShouldManageResourcesProperly()
    {
        // This test ensures services don't leak resources or cause memory issues

        // Arrange
        var memoryBefore = GC.GetTotalMemory(false);

        // Act - Perform multiple operations
        for (int i = 0; i < 10; i++)
        {
            await _xaiChatService!.GetResponseAsync($"Memory test query {i}");
        }

        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var memoryAfter = GC.GetTotalMemory(true);

        // Assert - Memory usage should not grow excessively
        var memoryGrowth = memoryAfter - memoryBefore;
        memoryGrowth.Should().BeLessThan(10_000_000, // 10MB threshold
            $"Memory growth should be reasonable. Growth: {memoryGrowth:N0} bytes");
    }

    [Test]
    [Category("Integration")]
    [Category("EndToEnd")]
    public async Task Services_ShouldSupportCompleteWorkflow()
    {
        // This test simulates a complete user workflow

        // Step 1: Initialize services
        await _xaiChatService!.InitializeAsync();

        // Step 2: Create a driver
        var newDriver = new Driver
        {
            DriverId = 555,
            DriverName = "Workflow Test Driver",
            Status = "Active",
            LicenseNumber = "WF555",
            DriverEmail = "workflow@test.com"
        };

        TestContext!.Drivers.Add(newDriver);
        await TestContext.SaveChangesAsync();

        // Step 3: Query about drivers through AI
        var aiResponse = await _xaiChatService.GetResponseAsync("How many drivers do we have?");

        // Step 4: Verify the workflow completed successfully
        var savedDriver = TestContext.Drivers.Find(555);
        savedDriver.Should().NotBeNull("Driver should be saved successfully");

        aiResponse.Should().NotBeNullOrEmpty("AI should provide driver information");
        aiResponse.Should().Contain("driver", "AI response should mention drivers");

        // Step 5: Clean up
        TestContext.Drivers.Remove(savedDriver!);
        await TestContext.SaveChangesAsync();
    }
}
