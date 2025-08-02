using NUnit.Framework;
using FluentAssertions;
using BusBuddy.Core.Models;
using BusBuddy.UITests.Builders;

namespace BusBuddy.UITests.Tests;

/// <summary>
/// Comprehensive unit tests for Driver entity validation and business logic
/// Uses FluentAssertions for readable test assertions
/// </summary>
[TestFixture]
[Category("UnitTests")]
[Category("Models")]
public class DriverModelTests
{
    [Test]
    public void Driver_DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var driver = new Driver();

        // Assert
        driver.DriverId.Should().Be(0);
        driver.DriverName.Should().NotBeNullOrEmpty(); // Has default value logic
        driver.DriverPhone.Should().BeNull();
        driver.DriverEmail.Should().BeNull();
        driver.Status.Should().Be("Active");
        driver.TrainingComplete.Should().BeFalse();
        driver.IsActive.Should().BeTrue(); // Computed from Status == "Active"
    }

    [Test]
    public void Driver_SetDriverName_ShouldUpdatePropertyAndNotifyPropertyChanged()
    {
        // Arrange
        var driver = new Driver();
        var propertyChangedEventRaised = false;
        driver.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(Driver.DriverName))
            {
                propertyChangedEventRaised = true;
            }
        };

        // Act
        driver.DriverName = "John Smith";

        // Assert
        driver.DriverName.Should().Be("John Smith");
        driver.Name.Should().Be("John Smith"); // Name is an alias for DriverName
        propertyChangedEventRaised.Should().BeTrue("PropertyChanged event should be raised");
    }

    [Test]
    public void Driver_SetDriverPhone_ShouldUpdatePropertyAndNotifyPropertyChanged()
    {
        // Arrange
        var driver = new Driver();
        var propertyChangedEventRaised = false;
        driver.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(Driver.DriverPhone))
            {
                propertyChangedEventRaised = true;
            }
        };

        // Act
        driver.DriverPhone = "555-123-4567";

        // Assert
        driver.DriverPhone.Should().Be("555-123-4567");
        propertyChangedEventRaised.Should().BeTrue("PropertyChanged event should be raised");
    }

    [Test]
    public void Driver_SetStatus_ShouldUpdateComputedProperties()
    {
        // Arrange
        var driver = new Driver();
        var propertyChangedEvents = new List<string>();
        driver.PropertyChanged += (sender, e) => propertyChangedEvents.Add(e.PropertyName ?? "");

        // Act
        driver.Status = "Inactive";

        // Assert
        driver.Status.Should().Be("Inactive");
        driver.IsActive.Should().BeFalse(); // Computed property should update
        propertyChangedEvents.Should().Contain(nameof(Driver.Status));
        propertyChangedEvents.Should().Contain(nameof(Driver.IsAvailable));
        propertyChangedEvents.Should().Contain(nameof(Driver.IsActive));
    }

    [Test]
    public void Driver_SetTrainingComplete_ShouldUpdateComputedProperties()
    {
        // Arrange
        var driver = new Driver();
        var propertyChangedEvents = new List<string>();
        driver.PropertyChanged += (sender, e) => propertyChangedEvents.Add(e.PropertyName ?? "");

        // Act
        driver.TrainingComplete = true;

        // Assert
        driver.TrainingComplete.Should().BeTrue();
        propertyChangedEvents.Should().Contain(nameof(Driver.TrainingComplete));
        propertyChangedEvents.Should().Contain(nameof(Driver.QualificationStatus));
    }

    [Test]
    [TestCase(null, ExpectedResult = "Driver-0")] // Default value logic
    [TestCase("", ExpectedResult = "Driver-0")] // Default value logic
    [TestCase("John Smith", ExpectedResult = "John Smith")]
    [TestCase("  Jane Doe  ", ExpectedResult = "Jane Doe")] // Should trim whitespace
    public string Driver_SetDriverName_WithVariousInputs_ShouldHandleCorrectly(string driverName)
    {
        // Arrange
        var driver = new Driver();

        // Act
        driver.DriverName = driverName;

        // Assert
        return driver.DriverName;
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("555-123-4567")]
    [TestCase("(555) 123-4567")]
    public void Driver_SetDriverPhone_WithVariousInputs_ShouldHandleCorrectly(string driverPhone)
    {
        // Arrange
        var driver = new Driver();

        // Act
        driver.DriverPhone = driverPhone;

        // Assert
        driver.DriverPhone.Should().Be(driverPhone);
    }

    [Test]
    public void Driver_FullName_ShouldReturnCorrectValue()
    {
        // Arrange
        var driver = new Driver
        {
            DriverName = "Driver Smith",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act & Assert
        driver.FullName.Should().Be("John Doe", "FullName should prefer FirstName + LastName when available");

        // Test fallback to DriverName
        driver.FirstName = null;
        driver.LastName = null;
        driver.FullName.Should().Be("Driver Smith", "FullName should fallback to DriverName");
    }

    [Test]
    public void Driver_FullAddress_ShouldCombineAddressComponents()
    {
        // Arrange
        var driver = new Driver
        {
            Address = "123 Main St",
            City = "Springfield",
            State = "IL",
            Zip = "62701"
        };

        // Act & Assert
        driver.FullAddress.Should().Be("123 Main St, Springfield, IL, 62701");

        // Test partial address
        driver.State = null;
        driver.FullAddress.Should().Be("123 Main St, Springfield, 62701");
    }

    [Test]
    public void Driver_QualificationStatus_ShouldReflectTrainingAndLicense()
    {
        // Arrange
        var driver = new Driver();

        // Act & Assert - Training not complete
        driver.TrainingComplete = false;
        driver.QualificationStatus.Should().Be("Training Required");

        // Training complete but no license info
        driver.TrainingComplete = true;
        driver.QualificationStatus.Should().Be("Qualified");

        // License expired
        driver.LicenseExpiryDate = DateTime.Now.AddDays(-1);
        driver.QualificationStatus.Should().Be("License Expired");

        // License expiring soon
        driver.LicenseExpiryDate = DateTime.Now.AddDays(15);
        driver.QualificationStatus.Should().Be("License Expiring");

        // Current license
        driver.LicenseExpiryDate = DateTime.Now.AddDays(60);
        driver.QualificationStatus.Should().Be("Qualified");
    }

    [Test]
    public void Driver_LicenseStatus_ShouldReflectExpiryDate()
    {
        // Arrange
        var driver = new Driver();

        // Act & Assert
        driver.LicenseExpiryDate = null;
        driver.LicenseStatus.Should().Be("Unknown");

        driver.LicenseExpiryDate = DateTime.Now.AddDays(-1);
        driver.LicenseStatus.Should().Be("Expired");

        driver.LicenseExpiryDate = DateTime.Now.AddDays(15);
        driver.LicenseStatus.Should().Be("Expiring Soon");

        driver.LicenseExpiryDate = DateTime.Now.AddDays(60);
        driver.LicenseStatus.Should().Be("Current");
    }

    [Test]
    public void Driver_IsAvailable_ShouldConsiderAllFactors()
    {
        // Arrange
        var driver = new Driver
        {
            Status = "Active",
            TrainingComplete = true,
            LicenseExpiryDate = DateTime.Now.AddDays(60)
        };

        // Act & Assert
        driver.IsAvailable.Should().BeTrue("Driver should be available when active, trained, and license current");

        // Test inactive status
        driver.Status = "Inactive";
        driver.IsAvailable.Should().BeFalse("Inactive driver should not be available");

        // Reset and test training
        driver.Status = "Active";
        driver.TrainingComplete = false;
        driver.IsAvailable.Should().BeFalse("Driver without training should not be available");

        // Reset and test expired license
        driver.TrainingComplete = true;
        driver.LicenseExpiryDate = DateTime.Now.AddDays(-1);
        driver.IsAvailable.Should().BeFalse("Driver with expired license should not be available");
    }

    [Test]
    public void Driver_NeedsAttention_ShouldIdentifyIssues()
    {
        // Arrange
        var driver = new Driver
        {
            TrainingComplete = true,
            LicenseExpiryDate = DateTime.Now.AddDays(60)
        };

        // Act & Assert
        driver.NeedsAttention.Should().BeFalse("Qualified driver should not need attention");

        // Training incomplete
        driver.TrainingComplete = false;
        driver.NeedsAttention.Should().BeTrue("Driver without training needs attention");

        // License expiring
        driver.TrainingComplete = true;
        driver.LicenseExpiryDate = DateTime.Now.AddDays(15);
        driver.NeedsAttention.Should().BeTrue("Driver with expiring license needs attention");

        // License expired
        driver.LicenseExpiryDate = DateTime.Now.AddDays(-1);
        driver.NeedsAttention.Should().BeTrue("Driver with expired license needs attention");
    }

    [Test]
    public void Driver_BusinessLogicValidation_ShouldEnforceRules()
    {
        // Arrange & Act
        var drivers = DriverTestDataBuilder.CreateDriversList(1);
        var driver = drivers.First();

        // Assert - Business rule validations
        driver.DriverId.Should().BeGreaterThan(0);
        driver.DriverName.Should().NotBeNullOrEmpty();
        driver.Status.Should().BeOneOf("Active", "Inactive", "On Leave");
        driver.Name.Should().Be(driver.DriverName); // Alias consistency
        driver.Id.Should().Be(driver.DriverId); // Alias consistency
    }

    [Test]
    public void Driver_StatusValidation_ShouldOnlyAllowValidStatuses()
    {
        // Arrange
        var driver = new Driver();
        var validStatuses = new[] { "Active", "Inactive", "On Leave", "Terminated" };

        // Act & Assert
        foreach (var status in validStatuses)
        {
            driver.Status = status;
            driver.Status.Should().Be(status, $"Status '{status}' should be valid");
        }
    }

    [Test]
    public void Driver_ContactPhone_ShouldAliasDriverPhone()
    {
        // Arrange
        var driver = new Driver();

        // Act
        driver.DriverPhone = "555-123-4567";

        // Assert
        driver.ContactPhone.Should().Be("555-123-4567");
        driver.ContactPhone.Should().Be(driver.DriverPhone);

        // Test setting through ContactPhone
        driver.ContactPhone = "555-987-6543";
        driver.DriverPhone.Should().Be("555-987-6543");
    }

    [Test]
    public void Driver_CreateFromBuilder_ShouldHaveValidProperties()
    {
        // Act
        var drivers = DriverTestDataBuilder.CreateDriversList(5);

        // Assert
        drivers.Should().HaveCount(5);
        drivers.Should().OnlyContain(d => !string.IsNullOrEmpty(d.DriverName));
        drivers.Should().OnlyContain(d => d.DriverId > 0);
        drivers.Should().OnlyContain(d => d.Status == "Active");
        drivers.Select(d => d.DriverId).Should().OnlyHaveUniqueItems("Driver IDs should be unique");
        drivers.Should().OnlyContain(d => d.IsActive, "All drivers from builder should be active");
    }

    [Test]
    public void Driver_PropertyChanged_ShouldNotRaiseForSameValue()
    {
        // Arrange
        var driver = new Driver();
        driver.DriverName = "John Smith";
        var propertyChangedEventRaised = false;
        driver.PropertyChanged += (sender, e) => propertyChangedEventRaised = true;

        // Act
        driver.DriverName = "John Smith"; // Same value

        // Assert
        propertyChangedEventRaised.Should().BeFalse("PropertyChanged should not be raised for the same value");
    }
}
