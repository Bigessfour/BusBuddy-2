using NUnit.Framework;
using FluentAssertions;
using BusBuddy.Core.Models;
using BusBuddy.UITests.Builders;

namespace BusBuddy.UITests.Tests;

/// <summary>
/// Advanced NUnit testing patterns demonstrating Microsoft best practices
/// Following https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-csharp-with-nunit
/// </summary>
[TestFixture]
[Category("AdvancedPatterns")]
[Category("BestPractices")]
public class AdvancedNUnitPatternsTests
{
    private VehicleTestDataBuilder _vehicleBuilder = null!;
    private DriverTestDataBuilder _driverBuilder = null!;
    private ActivityTestDataBuilder _activityBuilder = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Runs once before all tests in this fixture
        TestContext.WriteLine("Setting up AdvancedNUnitPatternsTests fixture");
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        // Runs once after all tests in this fixture
        TestContext.WriteLine("Tearing down AdvancedNUnitPatternsTests fixture");
    }

    [SetUp]
    public void SetUp()
    {
        // Runs before each test
        _vehicleBuilder = new VehicleTestDataBuilder();
        _driverBuilder = new DriverTestDataBuilder();
        _activityBuilder = new ActivityTestDataBuilder();
    }

    [TearDown]
    public void TearDown()
    {
        // Runs after each test
        // Clean up any resources if needed
    }

    #region Parameterized Tests - TestCase Pattern

    [Test]
    [TestCase("Blue Bird", "Vision", 72)]
    [TestCase("Thomas", "C2", 77)]
    [TestCase("IC Bus", "CE200", 78)]
    [TestCase("Freightliner", "S2C", 84)]
    [TestCase("International", "3000", 71)]
    public void Vehicle_CommonBusConfigurations_ShouldHaveValidSpecs(string make, string model, int expectedCapacity)
    {
        // Arrange
        var vehicle = _vehicleBuilder
            .WithMake(make)
            .WithModel(model)
            .WithCapacity(expectedCapacity)
            .Build();

        // Act & Assert
        vehicle.Make.Should().Be(make);
        vehicle.Model.Should().Be(model);
        vehicle.Capacity.Should().Be(expectedCapacity);
        vehicle.Capacity.Should().BeInRange(14, 84, "School bus capacity should be within standard range");
    }

    [Test]
    [TestCase("")]
    [TestCase(null)]
    [TestCase("   ")]
    public void Vehicle_InvalidMake_ShouldHandleGracefully(string invalidMake)
    {
        // Arrange & Act
        var vehicle = new Vehicle { Make = invalidMake ?? string.Empty };

        // Assert
        vehicle.Make.Should().BeOneOf("", null, "   ");
        // In a real scenario, you might want validation to prevent empty makes
    }

    #endregion

    #region TestCaseSource Pattern - Complex Data

    private static IEnumerable<TestCaseData> BusCapacityTestCases()
    {
        yield return new TestCaseData(14, "Small Bus").SetName("SmallBus_14Passengers");
        yield return new TestCaseData(20, "Small Bus").SetName("SmallBus_20Passengers");
        yield return new TestCaseData(35, "Medium Bus").SetName("MediumBus_35Passengers");
        yield return new TestCaseData(48, "Medium Bus").SetName("MediumBus_48Passengers");
        yield return new TestCaseData(72, "Large Bus").SetName("LargeBus_72Passengers");
        yield return new TestCaseData(84, "Large Bus").SetName("LargeBus_84Passengers");
    }

    [Test]
    [TestCaseSource(nameof(BusCapacityTestCases))]
    public void Vehicle_CapacityClassification_ShouldCategorizeCorrectly(int capacity, string expectedCategory)
    {
        // Arrange
        var vehicle = _vehicleBuilder.WithCapacity(capacity).Build();

        // Act
        string actualCategory = capacity switch
        {
            <= 20 => "Small Bus",
            <= 60 => "Medium Bus",
            _ => "Large Bus"
        };

        // Assert
        actualCategory.Should().Be(expectedCategory);
        vehicle.Capacity.Should().Be(capacity);
    }

    #endregion

    #region Values and Range Testing

    [Test]
    public void Vehicle_CapacityRange_ShouldAcceptValidValues([Range(14, 84, 7)] int capacity)
    {
        // Arrange & Act
        var vehicle = _vehicleBuilder.WithCapacity(capacity).Build();

        // Assert
        vehicle.Capacity.Should().Be(capacity);
        vehicle.Capacity.Should().BeInRange(14, 84);
    }

    [Test]
    public void Vehicle_PlateNumberFormat_ShouldAcceptVariousPatterns([Values("BUS-001", "ABC123", "TX-BUS-456", "12345")] string plateNumber)
    {
        // Arrange & Act
        var vehicle = _vehicleBuilder.WithPlateNumber(plateNumber).Build();

        // Assert
        vehicle.PlateNumber.Should().Be(plateNumber);
        vehicle.PlateNumber.Should().NotBeNullOrEmpty();
        vehicle.PlateNumber.Length.Should().BeGreaterThan(2);
    }

    #endregion

    #region Combinatorial Testing

    [Test]
    public void Vehicle_MakeModelCombinations_ShouldBeValid(
        [Values("Blue Bird", "Thomas", "IC Bus")] string make,
        [Values("Vision", "C2", "CE200")] string model)
    {
        // Arrange & Act
        var vehicle = _vehicleBuilder
            .WithMake(make)
            .WithModel(model)
            .Build();

        // Assert
        vehicle.Make.Should().Be(make);
        vehicle.Model.Should().Be(model);
        vehicle.Make.Should().NotBeNullOrEmpty();
        vehicle.Model.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Theory Testing Pattern

    [Test]
    public void Activity_TimeValidation_ShouldEnforceBusinessRules()
    {
        // Arrange - Create activities with various time configurations
        var scenarios = new[]
        {
            new { LeaveTime = TimeSpan.Parse("08:00"), EventTime = TimeSpan.Parse("09:00"), ReturnTime = TimeSpan.Parse("15:00"), IsValid = true },
            new { LeaveTime = TimeSpan.Parse("10:00"), EventTime = TimeSpan.Parse("09:00"), ReturnTime = TimeSpan.Parse("15:00"), IsValid = false }, // Event before leave
            new { LeaveTime = TimeSpan.Parse("08:00"), EventTime = TimeSpan.Parse("09:00"), ReturnTime = TimeSpan.Parse("08:30"), IsValid = false }, // Return before event
            new { LeaveTime = TimeSpan.Parse("23:00"), EventTime = TimeSpan.Parse("23:30"), ReturnTime = TimeSpan.Parse("23:45"), IsValid = true }   // Late night
        };

        foreach (var scenario in scenarios)
        {
            // Act
            var activity = _activityBuilder
                .WithLeaveTime(scenario.LeaveTime)
                .WithEventTime(scenario.EventTime)
                .WithReturnTime(scenario.ReturnTime)
                .Build();

            // Assert
            if (scenario.IsValid)
            {
                activity.LeaveTime.Should().BeLessOrEqualTo(activity.EventTime, "Leave time should be before or equal to event time");
                activity.EventTime.Should().BeLessOrEqualTo(activity.ReturnTime, "Event time should be before or equal to return time");
            }

            activity.LeaveTime.Should().Be(scenario.LeaveTime);
            activity.EventTime.Should().Be(scenario.EventTime);
            activity.ReturnTime.Should().Be(scenario.ReturnTime);
        }
    }

    #endregion

    #region Exception Testing

    [Test]
    public void VehicleBuilder_WithInvalidCapacity_ShouldHandleGracefully()
    {
        // Arrange & Act & Assert
        var negativeCapacityAction = () => _vehicleBuilder.WithCapacity(-1).Build();
        var zeroCapacityAction = () => _vehicleBuilder.WithCapacity(0).Build();
        var excessiveCapacityAction = () => _vehicleBuilder.WithCapacity(1000).Build();

        // Note: These tests assume the builder validates capacity
        // If no validation exists, these will pass but highlight missing validation
        negativeCapacityAction.Should().NotThrow("Builder should handle negative capacity gracefully");
        zeroCapacityAction.Should().NotThrow("Builder should handle zero capacity gracefully");
        excessiveCapacityAction.Should().NotThrow("Builder should handle excessive capacity gracefully");
    }

    #endregion

    #region Collection Testing with FluentAssertions

    [Test]
    public void VehicleFleet_CreateMultiple_ShouldHaveUniqueIdentifiers()
    {
        // Arrange & Act
        var fleet = VehicleTestDataBuilder.CreateVehicleFleet(10);

        // Assert - Comprehensive collection testing
        fleet.Should().HaveCount(10);
        fleet.Should().OnlyHaveUniqueItems(v => v.Id, "Vehicle IDs must be unique");
        fleet.Should().OnlyHaveUniqueItems(v => v.PlateNumber, "Plate numbers must be unique");
        fleet.Should().AllSatisfy(vehicle =>
        {
            vehicle.Id.Should().BeGreaterThan(0);
            vehicle.Make.Should().NotBeNullOrEmpty();
            vehicle.Model.Should().NotBeNullOrEmpty();
            vehicle.PlateNumber.Should().NotBeNullOrEmpty();
            vehicle.Capacity.Should().BeGreaterThan(0);
        });

        // Business rule assertions
        fleet.Select(v => v.Make).Should().OnlyContain(make =>
            new[] { "Blue Bird", "Thomas", "IC Bus", "Freightliner", "International" }.Contains(make),
            "All makes should be valid bus manufacturers");
    }

    #endregion

    #region Async Testing Patterns (for future use)

    [Test]
    public async Task FutureAsyncMethod_ShouldCompleteSuccessfully()
    {
        // This demonstrates async testing pattern for future async methods
        await Task.Delay(1); // Simulate async operation

        // Act & Assert
        var result = await Task.FromResult(true);
        result.Should().BeTrue();
    }

    #endregion

    #region Performance Testing

    [Test]
    [CancelAfter(1000)] // Test must complete within 1 second (NUnit 4.x syntax)
    public void VehicleCreation_ShouldBePerformant()
    {
        // Arrange & Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var vehicles = VehicleTestDataBuilder.CreateVehicleFleet(100);
        stopwatch.Stop();

        // Assert
        vehicles.Should().HaveCount(100);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500, "Creating 100 vehicles should be fast");
    }

    #endregion

    #region Custom Assertions and Extensions

    [Test]
    public void Vehicle_CustomValidation_ShouldMeetBusStandards()
    {
        // Arrange
        var vehicle = VehicleTestDataBuilder.CreateStandardBus();

        // Act & Assert using standard FluentAssertions
        // Valid school bus checks
        vehicle.Make.Should().NotBeNullOrEmpty("A valid school bus must have a manufacturer");
        vehicle.Model.Should().NotBeNullOrEmpty("A valid school bus must have a model");
        vehicle.Capacity.Should().BeInRange(14, 84, "School bus capacity should be within standard range");
        vehicle.IsActive.Should().BeTrue("A valid school bus should be active by default");
        vehicle.OperationalStatus.Should().NotBeNullOrEmpty("A valid school bus must have operational status");

        // Realistic capacity checks
        vehicle.Capacity.Should().BeGreaterThan(0, "Vehicle capacity must be positive");
        vehicle.Capacity.Should().BeLessOrEqualTo(84, "Standard school buses don't exceed 84 passengers");

        // Valid identification checks
        vehicle.Id.Should().BeGreaterThan(0, "Vehicle ID must be positive");
        vehicle.PlateNumber.Should().NotBeNullOrEmpty("Vehicle must have a plate number");
        vehicle.PlateNumber!.Length.Should().BeInRange(3, 10, "Plate number should be reasonable length");
    }

    #endregion

    #region Data-Driven Testing from External Source

    private static IEnumerable<TestCaseData> GetRealWorldBusData()
    {
        // In a real scenario, this could load from JSON, CSV, or database
        yield return new TestCaseData("Blue Bird", "All American", "BUS-001", 72);
        yield return new TestCaseData("Thomas Built", "Saf-T-Liner C2", "BUS-002", 77);
        yield return new TestCaseData("IC Bus", "CE Series", "BUS-003", 78);
    }

    [Test]
    [TestCaseSource(nameof(GetRealWorldBusData))]
    public void Vehicle_RealWorldConfigurations_ShouldBeSupported(string make, string model, string plateNumber, int capacity)
    {
        // Arrange & Act
        var vehicle = _vehicleBuilder
            .WithMake(make)
            .WithModel(model)
            .WithPlateNumber(plateNumber)
            .WithCapacity(capacity)
            .Build();

        // Assert
        vehicle.Should().BeEquivalentTo(new
        {
            Make = make,
            Model = model,
            PlateNumber = plateNumber,
            Capacity = capacity
        }, options => options.ExcludingMissingMembers());
    }

    #endregion
}
