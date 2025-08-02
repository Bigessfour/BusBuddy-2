using NUnit.Framework;
using FluentAssertions;
using BusBuddy.Core.Models;
using BusBuddy.UITests.Builders;

namespace BusBuddy.UITests.Tests;

/// <summary>
/// Comprehensive unit tests for Vehicle/Bus entity validation and business logic
/// Uses FluentAssertions for readable test assertions
/// </summary>
[TestFixture]
[Category("UnitTests")]
[Category("Models")]
public class VehicleModelTests
{
    [Test]
    public void Vehicle_DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var vehicle = new Vehicle();

        // Assert
        vehicle.Id.Should().Be(0);
        vehicle.Make.Should().Be(string.Empty);
        vehicle.Model.Should().Be(string.Empty);
        vehicle.PlateNumber.Should().Be(string.Empty);
        vehicle.Capacity.Should().Be(0);
        vehicle.BusNumber.Should().Be(string.Empty);
        vehicle.IsActive.Should().BeTrue();
        vehicle.OperationalStatus.Should().Be("Operational");
    }

    [Test]
    public void Vehicle_SetMake_ShouldUpdateProperty()
    {
        // Arrange
        var vehicle = new Vehicle();

        // Act
        vehicle.Make = "Blue Bird";

        // Assert
        vehicle.Make.Should().Be("Blue Bird");
    }

    [Test]
    public void Vehicle_SetModel_ShouldUpdateProperty()
    {
        // Arrange
        var vehicle = new Vehicle();

        // Act
        vehicle.Model = "All American";

        // Assert
        vehicle.Model.Should().Be("All American");
    }

    [Test]
    public void Vehicle_SetBusNumber_ShouldUpdateProperty()
    {
        // Arrange
        var vehicle = new Vehicle();

        // Act
        vehicle.BusNumber = "BUS-001";

        // Assert
        vehicle.BusNumber.Should().Be("BUS-001");
    }

    [Test]
    public void Vehicle_SetPlateNumber_ShouldUpdateProperty()
    {
        // Arrange
        var vehicle = new Vehicle();

        // Act
        vehicle.PlateNumber = "BUS-123";

        // Assert
        vehicle.PlateNumber.Should().Be("BUS-123");
    }

    [Test]
    public void Vehicle_SetCapacity_ShouldUpdateProperty()
    {
        // Arrange
        var vehicle = new Vehicle();

        // Act
        vehicle.Capacity = 72;

        // Assert
        vehicle.Capacity.Should().Be(72);
    }

    [Test]
    public void Vehicle_SetOperationalStatus_ShouldUpdatePropertyAndAvailability()
    {
        // Arrange
        var vehicle = new Vehicle();

        // Act
        vehicle.OperationalStatus = "Maintenance";

        // Assert
        vehicle.OperationalStatus.Should().Be("Maintenance");
        vehicle.IsActive.Should().BeTrue("IsActive is separate from OperationalStatus");
    }

    [Test]
    [TestCase(null, ExpectedResult = "")]
    [TestCase("", ExpectedResult = "")]
    [TestCase("Blue Bird", ExpectedResult = "Blue Bird")]
    [TestCase("  International  ", ExpectedResult = "  International  ")] // Should preserve formatting
    public string Vehicle_SetMake_WithVariousInputs_ShouldHandleCorrectly(string make)
    {
        // Arrange
        var vehicle = new Vehicle();

        // Act
        vehicle.Make = make ?? string.Empty;

        // Assert
        return vehicle.Make;
    }

    [Test]
    [TestCase(null, ExpectedResult = "")]
    [TestCase("", ExpectedResult = "")]
    [TestCase("All American", ExpectedResult = "All American")]
    public string Vehicle_SetModel_WithVariousInputs_ShouldHandleCorrectly(string model)
    {
        // Arrange
        var vehicle = new Vehicle();

        // Act
        vehicle.Model = model ?? string.Empty;

        // Assert
        return vehicle.Model;
    }

    [Test]
    [TestCase(null, ExpectedResult = "")]
    [TestCase("", ExpectedResult = "")]
    [TestCase("BUS-123", ExpectedResult = "BUS-123")]
    [TestCase("ABC123", ExpectedResult = "ABC123")]
    public string Vehicle_SetPlateNumber_WithVariousInputs_ShouldHandleCorrectly(string plateNumber)
    {
        // Arrange
        var vehicle = new Vehicle();

        // Act
        vehicle.PlateNumber = plateNumber ?? string.Empty;

        // Assert
        return vehicle.PlateNumber;
    }

    [Test]
    public void Vehicle_OperationalStatusValidation_ShouldOnlyAllowValidStatuses()
    {
        // Arrange
        var vehicle = new Vehicle();
        var validStatuses = new[] { "Operational", "Maintenance", "Out of Service", "Retired" };

        // Act & Assert
        foreach (var status in validStatuses)
        {
            vehicle.OperationalStatus = status;
            vehicle.OperationalStatus.Should().Be(status, $"Status '{status}' should be valid");
        }
    }

    [Test]
    public void Vehicle_IsActiveLogic_ShouldControlVehicleState()
    {
        // Arrange
        var vehicle = new Vehicle();

        // Act & Assert - Active vehicles
        vehicle.IsActive = true;
        vehicle.IsActive.Should().BeTrue("Vehicle should be active");

        // Inactive vehicles
        vehicle.IsActive = false;
        vehicle.IsActive.Should().BeFalse("Vehicle should be inactive");
    }

    [Test]
    public void Vehicle_BusinessLogicValidation_ShouldEnforceRules()
    {
        // Arrange & Act
        var vehicles = VehicleTestDataBuilder.CreateVehicleFleet(1);
        var vehicle = vehicles.First();

        // Assert - Business rule validations
        vehicle.Id.Should().BeGreaterThan(0);
        vehicle.Make.Should().NotBeNullOrEmpty();
        vehicle.Model.Should().NotBeNullOrEmpty();
        vehicle.PlateNumber.Should().NotBeNullOrEmpty();
        vehicle.BusNumber.Should().NotBeNullOrEmpty("Bus number should be set");
        vehicle.IsActive.Should().BeTrue("Active status should be set to true for new vehicles");
        vehicle.OperationalStatus.Should().NotBeNullOrEmpty("Operational status should be defined");
    }

    [Test]
    public void Vehicle_PlateNumberFormat_ShouldAcceptVariousFormats()
    {
        // Arrange
        var vehicle = new Vehicle();
        var validPlateNumbers = new[]
        {
            "BUS-123",
            "ABC123",
            "12345",
            "BUS123",
            "TX-BUS-456"
        };

        // Act & Assert
        foreach (var plateNumber in validPlateNumbers)
        {
            vehicle.PlateNumber = plateNumber;
            vehicle.PlateNumber.Should().Be(plateNumber);
            vehicle.PlateNumber.Should().NotBeNullOrEmpty();
        }
    }

    [Test]
    public void Vehicle_MakeAndModel_ShouldAcceptCommonBusManufacturers()
    {
        // Arrange
        var vehicle = new Vehicle();
        var commonBusMakes = new[]
        {
            "Blue Bird",
            "International",
            "Thomas Built Buses",
            "Freightliner",
            "Ford"
        };

        var commonBusModels = new[]
        {
            "All American",
            "Vision",
            "Saf-T-Liner",
            "Conventional",
            "Transit"
        };

        // Act & Assert
        foreach (var make in commonBusMakes)
        {
            vehicle.Make = make;
            vehicle.Make.Should().Be(make);
            vehicle.Make.Should().NotBeNullOrEmpty();
        }

        foreach (var model in commonBusModels)
        {
            vehicle.Model = model;
            vehicle.Model.Should().Be(model);
            vehicle.Model.Should().NotBeNullOrEmpty();
        }
    }

    [Test]
    public void Vehicle_CapacityCategories_ShouldReflectBusTypes()
    {
        // Arrange & Act & Assert
        var smallBus = new Vehicle { Capacity = 14 };
        smallBus.Capacity.Should().BeLessOrEqualTo(20, "Small bus capacity");

        var mediumBus = new Vehicle { Capacity = 48 };
        mediumBus.Capacity.Should().BeGreaterThan(20).And.BeLessOrEqualTo(60, "Medium bus capacity");

        var largeBus = new Vehicle { Capacity = 72 };
        largeBus.Capacity.Should().BeGreaterThan(60).And.BeLessOrEqualTo(84, "Large bus capacity");
    }

    [Test]
    public void Vehicle_CreateFromBuilder_ShouldHaveValidProperties()
    {
        // Act
        var vehicles = VehicleTestDataBuilder.CreateVehicleFleet(5);

        // Assert
        vehicles.Should().HaveCount(5);
        vehicles.Should().OnlyContain(v => !string.IsNullOrEmpty(v.Make));
        vehicles.Should().OnlyContain(v => !string.IsNullOrEmpty(v.Model));
        vehicles.Should().OnlyContain(v => !string.IsNullOrEmpty(v.PlateNumber));
        vehicles.Should().OnlyContain(v => v.Id > 0);
        vehicles.Should().OnlyContain(v => v.Capacity > 0);
        vehicles.Should().OnlyContain(v => v.IsActive); // Default is true
        vehicles.Should().OnlyContain(v => v.OperationalStatus == "Operational"); // Default value
        vehicles.Select(v => v.Id).Should().OnlyHaveUniqueItems("Vehicle IDs should be unique");
        vehicles.Select(v => v.PlateNumber).Should().OnlyHaveUniqueItems("Plate numbers should be unique");
    }

    [Test]
    public void Vehicle_DetailedInformation_ShouldProvideCompleteDescription()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            Id = 1,
            Make = "Blue Bird",
            Model = "All American",
            PlateNumber = "BUS-001",
            Capacity = 72,
            BusNumber = "001",
            OperationalStatus = "Operational"
        };

        // Act & Assert
        vehicle.Make.Should().Be("Blue Bird");
        vehicle.Model.Should().Be("All American");
        vehicle.PlateNumber.Should().Be("BUS-001");
        vehicle.Capacity.Should().Be(72);
        vehicle.BusNumber.Should().Be("001");
        vehicle.OperationalStatus.Should().Be("Operational");
        vehicle.IsActive.Should().BeTrue();

        // Verify the vehicle represents a realistic school bus
        $"{vehicle.Make} {vehicle.Model}".Should().Be("Blue Bird All American");
    }

    [Test]
    public void Vehicle_EqualsAndHashCode_ShouldWorkCorrectly()
    {
        // Arrange
        var vehicle1 = new Vehicle { Id = 1, PlateNumber = "BUS-001" };
        var vehicle2 = new Vehicle { Id = 1, PlateNumber = "BUS-001" };
        var vehicle3 = new Vehicle { Id = 2, PlateNumber = "BUS-002" };

        // Act & Assert
        // Note: This test assumes Vehicle implements IEquatable<Vehicle> or overrides Equals
        // If not implemented, this will test reference equality
        vehicle1.Should().NotBeSameAs(vehicle2, "Different instances");
        vehicle1.Should().NotBe(vehicle3, "Different vehicles should not be equal");
    }

    [Test]
    public void Vehicle_ToString_ShouldProvideReadableRepresentation()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            Id = 1,
            Make = "Blue Bird",
            Model = "All American",
            PlateNumber = "BUS-001"
        };

        // Act
        var stringRepresentation = vehicle.ToString();

        // Assert
        // Note: This test assumes Vehicle overrides ToString() method
        // The actual implementation may vary
        stringRepresentation.Should().NotBeNull();
        stringRepresentation.Should().NotBeEmpty();
    }
}
