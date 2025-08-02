using NUnit.Framework;
using FluentAssertions;
using BusBuddy.WPF.Services;
using BusBuddy.WPF.Models;
using BusBuddy.Core.Models;
using BusBuddy.Core.Data;
using BusBuddy.Core.Services;
using BusBuddy.Core.Services.Interfaces;
using BusBuddy.UITests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BusBuddy.UITests.Tests;

/// <summary>
/// Comprehensive tests for DataIntegrityService functionality
/// Tests all validation scenarios and cross-entity relationship checks
/// </summary>
[TestFixture]
public class DataIntegrityServiceTests
{
    private ServiceProvider? _serviceProvider;
    private BusBuddyDbContext? _context;
    private DataIntegrityService? _dataIntegrityService;
    private Mock<IRouteService>? _mockRouteService;
    private Mock<IDriverService>? _mockDriverService;
    private Mock<IBusService>? _mockBusService;
    private Mock<IActivityService>? _mockActivityService;
    private Mock<IStudentService>? _mockStudentService;

    [SetUp]
    public void SetUp()
    {
        // Create mock services
        _mockRouteService = new Mock<IRouteService>();
        _mockDriverService = new Mock<IDriverService>();
        _mockBusService = new Mock<IBusService>();
        _mockActivityService = new Mock<IActivityService>();
        _mockStudentService = new Mock<IStudentService>();

        // Create service provider
        var services = new ServiceCollection();
        _ = services.AddDbContext<BusBuddyDbContext>(options =>
            options.UseInMemoryDatabase($"DataIntegrityTest_{Guid.NewGuid()}"));

        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<BusBuddyDbContext>();

        // Create service with mocks
        _dataIntegrityService = new DataIntegrityService(
            _mockRouteService.Object,
            _mockDriverService.Object,
            _mockBusService.Object,
            _mockActivityService.Object,
            _mockStudentService.Object);

        // Seed test data with integrity issues
        SeedTestDataWithIntegrityIssues();
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
        _serviceProvider?.Dispose();
    }

    private void SeedTestDataWithIntegrityIssues()
    {
        if (_context == null)
        {
            return;
        }

        // Create drivers with various issues
        var drivers = new[]
        {
            new Driver { DriverId = 1, DriverName = "", Status = "Active", LicenseNumber = "D123456" }, // Missing name
            new Driver { DriverId = 2, DriverName = "Valid Driver", Status = "", LicenseNumber = "D789012" }, // Missing status
            new Driver { DriverId = 3, DriverName = "Expired License", Status = "Active", LicenseNumber = "D345678", LicenseExpiryDate = DateTime.Now.AddDays(-10) }, // Expired license
            new Driver { DriverId = 4, DriverName = "Valid Driver 2", Status = "Active", LicenseNumber = "D567890", LicenseExpiryDate = DateTime.Now.AddDays(10) } // Valid
        };

        // Create vehicles with issues
        var vehicles = new[]
        {
            new Bus { VehicleId = 1, BusNumber = "", Make = "Blue Bird", Model = "School Bus" }, // Missing number
            new Bus { VehicleId = 2, BusNumber = "Bus-002", Make = "", Model = "Transit" }, // Missing make
            new Bus { VehicleId = 3, BusNumber = "Bus-003", Make = "Thomas", Model = "EFX" }, // Valid
            new Bus { VehicleId = 4, BusNumber = "Bus-004", Make = "IC Bus", Model = "CE" } // Valid
        };

        // Create activities with scheduling conflicts
        var activities = new[]
        {
            new Activity { ActivityId = 1, ActivityType = "", LeaveTime = TimeSpan.FromHours(8), EventTime = TimeSpan.FromHours(7), Date = DateTime.Today, DriverId = 1, AssignedVehicleId = 1 }, // Missing type, end before start
            new Activity { ActivityId = 2, ActivityType = "Route A", LeaveTime = TimeSpan.FromHours(9), EventTime = TimeSpan.FromHours(11), Date = DateTime.Today, DriverId = 1, AssignedVehicleId = 2 }, // Driver conflict
            new Activity { ActivityId = 3, ActivityType = "Route B", LeaveTime = TimeSpan.FromHours(10), EventTime = TimeSpan.FromHours(12), Date = DateTime.Today, DriverId = 1, AssignedVehicleId = 1 }, // Driver and vehicle conflict
            new Activity { ActivityId = 4, ActivityType = "Valid Route", LeaveTime = TimeSpan.FromHours(13), EventTime = TimeSpan.FromHours(15), Date = DateTime.Today, DriverId = 4, AssignedVehicleId = 4 } // Valid
        };

        _context.Drivers.AddRange(drivers);
        _context.Vehicles.AddRange(vehicles);
        _context.Activities.AddRange(activities);
        _ = _context.SaveChanges();

        // Setup mock services to return test data
        _ = _mockDriverService!.Setup(s => s.GetAllDriversAsync()).ReturnsAsync(drivers.ToList());
        _ = _mockBusService!.Setup(s => s.GetAllBusesAsync()).ReturnsAsync(vehicles.ToList());
        _ = _mockActivityService!.Setup(s => s.GetAllActivitiesAsync()).ReturnsAsync(activities.ToList());
        _ = _mockRouteService!.Setup(s => s.GetAllActiveRoutesAsync()).ReturnsAsync(new List<Route>());
        _ = _mockStudentService!.Setup(s => s.GetAllStudentsAsync()).ReturnsAsync(new List<Student>());
    }

    [Test]
    [Category("DataIntegrity")]
    [Category("DriverValidation")]
    public async Task ValidateDriversAsync_ShouldIdentifyMissingRequiredData()
    {
        // Act
        var issues = await _dataIntegrityService!.ValidateDriversAsync();

        // Assert
        _ = issues.Should().NotBeEmpty("should find driver validation issues");
        _ = issues.Should().Contain(i => i.IssueType == "Missing Required Data" && i.Description.Contains("name"),
            "should identify missing driver name");
        _ = issues.Should().Contain(i => i.IssueType == "Missing Required Data" && i.Description.Contains("status"),
            "should identify missing driver status");
    }

    [Test]
    [Category("DataIntegrity")]
    [Category("DriverValidation")]
    public async Task ValidateDriversAsync_ShouldIdentifyExpiredLicenses()
    {
        // Act
        var issues = await _dataIntegrityService!.ValidateDriversAsync();

        // Assert
        _ = issues.Should().Contain(i => i.IssueType == "License Issue" && i.Severity == "Critical",
            "should identify expired driver license as critical");
        _ = issues.Should().Contain(i => i.Description.Contains("expired"),
            "should specifically mention license expiration");
    }

    [Test]
    [Category("DataIntegrity")]
    [Category("VehicleValidation")]
    public async Task ValidateVehiclesAsync_ShouldIdentifyInvalidData()
    {
        // Act
        var issues = await _dataIntegrityService!.ValidateVehiclesAsync();

        // Assert
        _ = issues.Should().NotBeEmpty("should find vehicle validation issues");
        _ = issues.Should().Contain(i => i.IssueType == "Invalid Data Format" && i.Description.Contains("negative"),
            "should identify negative mileage");
        _ = issues.Should().Contain(i => i.IssueType == "Compliance Issue" && i.Description.Contains("inspection"),
            "should identify overdue inspections");
    }

    [Test]
    [Category("DataIntegrity")]
    [Category("ActivityValidation")]
    public async Task ValidateActivitiesAsync_ShouldIdentifyBusinessLogicViolations()
    {
        // Act
        var issues = await _dataIntegrityService!.ValidateActivitiesAsync();

        // Assert
        _ = issues.Should().NotBeEmpty("should find activity validation issues");
        _ = issues.Should().Contain(i => i.IssueType == "Business Logic Violation" && i.Description.Contains("start time"),
            "should identify invalid time sequence");
        _ = issues.Should().Contain(i => i.IssueType == "Missing Required Data" && i.Description.Contains("type"),
            "should identify missing activity type");
    }

    [Test]
    [Category("DataIntegrity")]
    [Category("CrossEntityValidation")]
    public async Task ValidateCrossEntityRelationshipsAsync_ShouldIdentifySchedulingConflicts()
    {
        // Act
        var issues = await _dataIntegrityService!.ValidateCrossEntityRelationshipsAsync();

        // Assert
        _ = issues.Should().NotBeEmpty("should find scheduling conflicts");
        _ = issues.Should().Contain(i => i.IssueType == "Scheduling Conflict" && i.Description.Contains("overlapping"),
            "should identify overlapping driver assignments");
    }

    [Test]
    [Category("DataIntegrity")]
    [Category("ComprehensiveValidation")]
    public async Task ValidateAllDataAsync_ShouldReturnComprehensiveReport()
    {
        // Act
        var report = await _dataIntegrityService!.ValidateAllDataAsync();

        // Assert
        _ = report.Should().NotBeNull("should return validation report");
        _ = report.TotalIssuesFound.Should().BeGreaterThan(0, "should find validation issues");
        _ = report.DriverIssues.Should().NotBeEmpty("should include driver issues");
        _ = report.VehicleIssues.Should().NotBeEmpty("should include vehicle issues");
        _ = report.ActivityIssues.Should().NotBeEmpty("should include activity issues");
        _ = report.CrossEntityIssues.Should().NotBeEmpty("should include cross-entity issues");

        // Verify report metrics
        _ = report.CriticalIssues.Should().NotBeEmpty("should identify critical issues");
        _ = report.HighPriorityIssues.Should().NotBeEmpty("should identify high priority issues");
        _ = report.AllIssues.Count.Should().Be(report.TotalIssuesFound, "total count should match all issues count");
    }

    [Test]
    [Category("DataIntegrity")]
    [Category("EntitySpecificValidation")]
    public async Task ValidateEntityAsync_ShouldValidateSpecificDriver()
    {
        // Act
        var issues = await _dataIntegrityService!.ValidateEntityAsync("driver", 1);

        // Assert
        _ = issues.Should().NotBeEmpty("should find issues for specific driver");
        _ = issues.Should().OnlyContain(i => i.EntityId == "1", "should only return issues for specified driver");
    }

    [Test]
    [Category("DataIntegrity")]
    [Category("ErrorHandling")]
    public async Task ValidateAllDataAsync_ShouldHandleServiceExceptions()
    {
        // Arrange
        _ = _mockDriverService!.Setup(s => s.GetAllDriversAsync()).ThrowsAsync(new InvalidOperationException("Service error"));

        // Act
        var report = await _dataIntegrityService!.ValidateAllDataAsync();

        // Assert
        _ = report.Should().NotBeNull("should return report even with errors");
        _ = report.ValidationError.Should().NotBeNullOrEmpty("should capture validation error");
    }

    [Test]
    [Category("DataIntegrity")]
    [Category("PerformanceValidation")]
    public async Task ValidateAllDataAsync_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var report = await _dataIntegrityService!.ValidateAllDataAsync();

        stopwatch.Stop();

        // Assert
        _ = stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000,
            $"Validation should complete within 5 seconds. Actual: {stopwatch.ElapsedMilliseconds}ms");
        _ = report.Should().NotBeNull("should return valid report");
    }

    [Test]
    [Category("DataIntegrity")]
    [Category("SeverityClassification")]
    public async Task ValidationIssues_ShouldHaveCorrectSeverityLevels()
    {
        // Act
        var report = await _dataIntegrityService!.ValidateAllDataAsync();

        // Assert
        var criticalIssues = report.AllIssues.Where(i => i.Severity == "Critical").ToList();
        var highIssues = report.AllIssues.Where(i => i.Severity == "High").ToList();

        _ = criticalIssues.Should().NotBeEmpty("should have critical issues");
        _ = highIssues.Should().NotBeEmpty("should have high priority issues");

        // Verify critical issues are blocking
        _ = criticalIssues.Should().OnlyContain(i => i.IsBlocking, "all critical issues should be blocking");
    }

    [Test]
    [Category("DataIntegrity")]
    [Category("ReportGeneration")]
    public async Task DataIntegrityReport_ShouldProvideUsefulSummary()
    {
        // Act
        var report = await _dataIntegrityService!.ValidateAllDataAsync();

        // Assert
        _ = report.Summary.Should().NotBeNullOrEmpty("should provide summary");
        _ = report.Summary.Should().Contain("Critical:", "summary should include critical count");
        _ = report.Summary.Should().Contain("High:", "summary should include high priority count");
        _ = report.GeneratedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1), "should have recent generation time");
    }
}
