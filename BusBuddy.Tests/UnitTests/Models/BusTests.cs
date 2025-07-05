using NUnit.Framework;
using FluentAssertions;
using Bus_Buddy.Models;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace BusBuddy.Tests.UnitTests.Models
{
    /// <summary>
    /// Comprehensive tests for the Bus model
    /// Following Coverage Improvement Roadmap - Week 1: Models & Core Logic
    /// Target: Achieve 25% coverage by testing all Bus model functionality
    /// </summary>
    [TestFixture]
    [NonParallelizable] // For thread safety with property change events
    [Category("Unit")]
    [Category("Models")]
    [Category("Coverage-Week1")]
    public class BusTests
    {
        private Bus _bus;

        [SetUp]
        public void SetUp()
        {
            _bus = new Bus();
        }

        #region Constructor and Default Values Tests

        [Test]
        public void Bus_Constructor_ShouldSetDefaultValues()
        {
            // Arrange & Act
            var bus = new Bus();

            // Assert
            bus.BusNumber.Should().Be(string.Empty);
            bus.Make.Should().Be(string.Empty);
            bus.Model.Should().Be(string.Empty);
            bus.VINNumber.Should().Be(string.Empty);
            bus.LicenseNumber.Should().Be(string.Empty);
            bus.Status.Should().Be("Active");
            bus.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            bus.GPSTracking.Should().BeFalse();
            bus.AMRoutes.Should().NotBeNull().And.BeEmpty();
            bus.PMRoutes.Should().NotBeNull().And.BeEmpty();
            bus.Activities.Should().NotBeNull().And.BeEmpty();
            bus.ScheduledActivities.Should().NotBeNull().And.BeEmpty();
            bus.FuelRecords.Should().NotBeNull().And.BeEmpty();
            bus.MaintenanceRecords.Should().NotBeNull().And.BeEmpty();
        }

        #endregion

        #region Property Setter Tests with INotifyPropertyChanged

        [Test]
        public void BusNumber_WhenSet_ShouldTriggerPropertyChanged()
        {
            // Arrange
            var propertyChangedTriggered = false;
            string? changedPropertyName = null;
            _bus.PropertyChanged += (sender, e) =>
            {
                propertyChangedTriggered = true;
                changedPropertyName = e.PropertyName;
            };

            // Act
            _bus.BusNumber = "BUS-001";

            // Assert
            propertyChangedTriggered.Should().BeTrue();
            changedPropertyName.Should().Be(nameof(Bus.BusNumber));
            _bus.BusNumber.Should().Be("BUS-001");
        }

        [Test]
        public void BusNumber_WhenSetToNull_ShouldSetToEmptyString()
        {
            // Arrange & Act
            _bus.BusNumber = null!;

            // Assert
            _bus.BusNumber.Should().Be(string.Empty);
        }

        [Test]
        public void BusNumber_WhenSetToSameValue_ShouldNotTriggerPropertyChanged()
        {
            // Arrange
            _bus.BusNumber = "BUS-001";
            var propertyChangedTriggered = false;
            _bus.PropertyChanged += (sender, e) => propertyChangedTriggered = true;

            // Act
            _bus.BusNumber = "BUS-001";

            // Assert
            propertyChangedTriggered.Should().BeFalse();
        }

        [Test]
        public void Year_WhenSet_ShouldTriggerPropertyChangedForYearAndAge()
        {
            // Arrange
            var changedProperties = new List<string>();
            _bus.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != null)
                    changedProperties.Add(e.PropertyName);
            };

            // Act
            _bus.Year = 2020;

            // Assert
            changedProperties.Should().Contain(nameof(Bus.Year));
            changedProperties.Should().Contain(nameof(Bus.Age));
            _bus.Year.Should().Be(2020);
        }

        [Test]
        public void Make_WhenSet_ShouldTriggerPropertyChangedForMakeAndFullDescription()
        {
            // Arrange
            var changedProperties = new List<string>();
            _bus.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != null)
                    changedProperties.Add(e.PropertyName);
            };

            // Act
            _bus.Make = "Blue Bird";

            // Assert
            changedProperties.Should().Contain(nameof(Bus.Make));
            changedProperties.Should().Contain(nameof(Bus.FullDescription));
            _bus.Make.Should().Be("Blue Bird");
        }

        [Test]
        public void Model_WhenSet_ShouldTriggerPropertyChangedForModelAndFullDescription()
        {
            // Arrange
            var changedProperties = new List<string>();
            _bus.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != null)
                    changedProperties.Add(e.PropertyName);
            };

            // Act
            _bus.Model = "Vision";

            // Assert
            changedProperties.Should().Contain(nameof(Bus.Model));
            changedProperties.Should().Contain(nameof(Bus.FullDescription));
            _bus.Model.Should().Be("Vision");
        }

        [Test]
        public void SeatingCapacity_WhenSet_ShouldTriggerPropertyChanged()
        {
            // Arrange
            var propertyChangedTriggered = false;
            _bus.PropertyChanged += (sender, e) => propertyChangedTriggered = true;

            // Act
            _bus.SeatingCapacity = 72;

            // Assert
            propertyChangedTriggered.Should().BeTrue();
            _bus.SeatingCapacity.Should().Be(72);
        }

        [Test]
        public void DateLastInspection_WhenSet_ShouldTriggerPropertyChangedForDateAndStatus()
        {
            // Arrange
            var changedProperties = new List<string>();
            _bus.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != null)
                    changedProperties.Add(e.PropertyName);
            };

            // Act
            _bus.DateLastInspection = DateTime.Now.AddMonths(-6);

            // Assert
            changedProperties.Should().Contain(nameof(Bus.DateLastInspection));
            changedProperties.Should().Contain(nameof(Bus.InspectionStatus));
        }

        [Test]
        public void InsuranceExpiryDate_WhenSet_ShouldTriggerPropertyChangedForDateAndStatus()
        {
            // Arrange
            var changedProperties = new List<string>();
            _bus.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != null)
                    changedProperties.Add(e.PropertyName);
            };

            // Act
            _bus.InsuranceExpiryDate = DateTime.Now.AddMonths(6);

            // Assert
            changedProperties.Should().Contain(nameof(Bus.InsuranceExpiryDate));
            changedProperties.Should().Contain(nameof(Bus.InsuranceStatus));
        }

        #endregion

        #region Computed Properties Tests

        [Test]
        public void Age_ShouldCalculateCorrectly()
        {
            // Arrange
            var currentYear = DateTime.Now.Year;
            _bus.Year = currentYear - 8;

            // Act
            var age = _bus.Age;

            // Assert
            age.Should().Be(8);
        }

        [Test]
        public void FullDescription_ShouldFormatCorrectly()
        {
            // Arrange
            _bus.Year = 2020;
            _bus.Make = "Blue Bird";
            _bus.Model = "Vision";
            _bus.BusNumber = "001";

            // Act
            var description = _bus.FullDescription;

            // Assert
            description.Should().Be("2020 Blue Bird Vision (#001)");
        }

        [Test]
        public void FullDescription_ShouldHandleEmptyValues()
        {
            // Arrange
            _bus.Year = 2020;
            _bus.Make = "";
            _bus.Model = "";
            _bus.BusNumber = "";

            // Act
            var description = _bus.FullDescription;

            // Assert
            description.Should().Be("2020   (#)");
        }

        [Test]
        public void InspectionStatus_WhenDateIsNull_ShouldReturnOverdue()
        {
            // Arrange
            _bus.DateLastInspection = null;

            // Act
            var status = _bus.InspectionStatus;

            // Assert
            status.Should().Be("Overdue");
        }

        [Test]
        public void InspectionStatus_WhenInspectionIsRecent_ShouldReturnCurrent()
        {
            // Arrange
            _bus.DateLastInspection = DateTime.Now.AddMonths(-6);

            // Act
            var status = _bus.InspectionStatus;

            // Assert
            status.Should().Be("Current");
        }

        [Test]
        public void InspectionStatus_WhenInspectionIsAlmostDue_ShouldReturnDueSoon()
        {
            // Arrange - Set inspection to exactly 370 days ago (370/30 = 12.33 months, which is > 11 but <= 12)
            // 345 days = 11.5 months (between 11 and 12, should be "Due Soon")
            _bus.DateLastInspection = DateTime.Now.AddDays(-345);

            // Act
            var status = _bus.InspectionStatus;

            // Assert
            status.Should().Be("Due Soon");
        }

        [Test]
        public void InspectionStatus_WhenInspectionIsOverdue_ShouldReturnOverdue()
        {
            // Arrange
            _bus.DateLastInspection = DateTime.Now.AddMonths(-13);

            // Act
            var status = _bus.InspectionStatus;

            // Assert
            status.Should().Be("Overdue");
        }

        [Test]
        public void InsuranceStatus_WhenDateIsNull_ShouldReturnUnknown()
        {
            // Arrange
            _bus.InsuranceExpiryDate = null;

            // Act
            var status = _bus.InsuranceStatus;

            // Assert
            status.Should().Be("Unknown");
        }

        [Test]
        public void InsuranceStatus_WhenInsuranceIsCurrent_ShouldReturnCurrent()
        {
            // Arrange
            _bus.InsuranceExpiryDate = DateTime.Now.AddMonths(6);

            // Act
            var status = _bus.InsuranceStatus;

            // Assert
            status.Should().Be("Current");
        }

        [Test]
        public void InsuranceStatus_WhenInsuranceIsExpiringSoon_ShouldReturnExpiringSoon()
        {
            // Arrange
            _bus.InsuranceExpiryDate = DateTime.Now.AddDays(15);

            // Act
            var status = _bus.InsuranceStatus;

            // Assert
            status.Should().Be("Expiring Soon");
        }

        [Test]
        public void InsuranceStatus_WhenInsuranceIsExpired_ShouldReturnExpired()
        {
            // Arrange
            _bus.InsuranceExpiryDate = DateTime.Now.AddDays(-5);

            // Act
            var status = _bus.InsuranceStatus;

            // Assert
            status.Should().Be("Expired");
        }

        [Test]
        public void IsAvailable_WhenStatusIsActive_ShouldReturnTrue()
        {
            // Arrange
            _bus.Status = "Active";

            // Act
            var isAvailable = _bus.IsAvailable;

            // Assert
            isAvailable.Should().BeTrue();
        }

        [Test]
        public void IsAvailable_WhenStatusIsNotActive_ShouldReturnFalse()
        {
            // Arrange
            _bus.Status = "Out of Service";

            // Act
            var isAvailable = _bus.IsAvailable;

            // Assert
            isAvailable.Should().BeFalse();
        }

        [Test]
        public void NeedsAttention_WhenInspectionIsOverdueAndInsuranceExpired_ShouldReturnTrue()
        {
            // Arrange
            _bus.DateLastInspection = DateTime.Now.AddMonths(-15);
            _bus.InsuranceExpiryDate = DateTime.Now.AddDays(-10);

            // Act
            var needsAttention = _bus.NeedsAttention;

            // Assert
            needsAttention.Should().BeTrue();
        }

        [Test]
        public void NeedsAttention_WhenEverythingIsCurrent_ShouldReturnFalse()
        {
            // Arrange
            _bus.DateLastInspection = DateTime.Now.AddMonths(-6);
            _bus.InsuranceExpiryDate = DateTime.Now.AddMonths(6);

            // Act
            var needsAttention = _bus.NeedsAttention;

            // Assert
            needsAttention.Should().BeFalse();
        }

        #endregion

        #region Navigation Properties Tests

        [Test]
        public void Routes_ShouldCombineAMAndPMRoutes()
        {
            // Arrange
            var amRoute = new Route { RouteName = "AM-001" };
            var pmRoute = new Route { RouteName = "PM-001" };
            _bus.AMRoutes.Add(amRoute);
            _bus.PMRoutes.Add(pmRoute);

            // Act
            var allRoutes = _bus.Routes;

            // Assert
            allRoutes.Should().HaveCount(2);
            allRoutes.Should().Contain(amRoute);
            allRoutes.Should().Contain(pmRoute);
        }

        [Test]
        public void Routes_WhenNoRoutesExist_ShouldReturnEmptyList()
        {
            // Arrange & Act
            var allRoutes = _bus.Routes;

            // Assert
            allRoutes.Should().NotBeNull().And.BeEmpty();
        }

        #endregion

        #region Validation Attribute Tests

        [Test]
        public void Bus_WithValidData_ShouldPassValidation()
        {
            // Arrange
            _bus.BusNumber = "BUS-001";
            _bus.Year = 2020;
            _bus.Make = "Blue Bird";
            _bus.Model = "Vision";
            _bus.SeatingCapacity = 72;
            _bus.VINNumber = "1HGCM82633A123456";
            _bus.LicenseNumber = "ABC123";

            // Act
            var validationResults = ValidateModel(_bus);

            // Assert
            validationResults.Should().BeEmpty();
        }

        [Test]
        public void Bus_WithMissingRequiredFields_ShouldFailValidation()
        {
            // Arrange
            var bus = new Bus(); // Default empty bus

            // Act
            var validationResults = ValidateModel(bus);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Select(v => v.MemberNames.First()).Should().Contain(
                nameof(Bus.BusNumber),
                nameof(Bus.Make),
                nameof(Bus.Model),
                nameof(Bus.VINNumber),
                nameof(Bus.LicenseNumber)
            );
        }

        [Test]
        public void Bus_WithInvalidYear_ShouldFailValidation()
        {
            // Arrange
            _bus.Year = 1800; // Invalid year

            // Act
            var validationResults = ValidateModel(_bus);

            // Assert
            validationResults.Should().Contain(v => v.MemberNames.Contains(nameof(Bus.Year)));
        }

        [Test]
        public void Bus_WithInvalidSeatingCapacity_ShouldFailValidation()
        {
            // Arrange
            _bus.SeatingCapacity = 0; // Invalid capacity

            // Act
            var validationResults = ValidateModel(_bus);

            // Assert
            validationResults.Should().Contain(v => v.MemberNames.Contains(nameof(Bus.SeatingCapacity)));
        }

        [Test]
        public void Bus_WithTooLongBusNumber_ShouldFailValidation()
        {
            // Arrange
            _bus.BusNumber = new string('A', 25); // Exceeds 20 char limit

            // Act
            var validationResults = ValidateModel(_bus);

            // Assert
            validationResults.Should().Contain(v => v.MemberNames.Contains(nameof(Bus.BusNumber)));
        }

        #endregion

        #region Status and Null Handling Tests

        [Test]
        public void Status_WhenSetToNull_ShouldDefaultToActive()
        {
            // Arrange & Act
            _bus.Status = null!;

            // Assert
            _bus.Status.Should().Be("Active");
        }

        [Test]
        public void VINNumber_WhenSetToNull_ShouldSetToEmptyString()
        {
            // Arrange & Act
            _bus.VINNumber = null!;

            // Assert
            _bus.VINNumber.Should().Be(string.Empty);
        }

        [Test]
        public void LicenseNumber_WhenSetToNull_ShouldSetToEmptyString()
        {
            // Arrange & Act
            _bus.LicenseNumber = null!;

            // Assert
            _bus.LicenseNumber.Should().Be(string.Empty);
        }

        #endregion

        #region Extended Properties Tests

        [Test]
        public void ExtendedProperties_ShouldBeSettableAndGettable()
        {
            // Arrange & Act
            _bus.Department = "Transportation";
            _bus.FleetType = "Regular";
            _bus.FuelCapacity = 100.5m;
            _bus.FuelType = "Diesel";
            _bus.MilesPerGallon = 8.5m;
            _bus.GPSTracking = true;
            _bus.GPSDeviceId = "GPS-001";
            _bus.SpecialEquipment = "Wheelchair lift, Air conditioning";
            _bus.Notes = "Test notes";

            // Assert
            _bus.Department.Should().Be("Transportation");
            _bus.FleetType.Should().Be("Regular");
            _bus.FuelCapacity.Should().Be(100.5m);
            _bus.FuelType.Should().Be("Diesel");
            _bus.MilesPerGallon.Should().Be(8.5m);
            _bus.GPSTracking.Should().BeTrue();
            _bus.GPSDeviceId.Should().Be("GPS-001");
            _bus.SpecialEquipment.Should().Be("Wheelchair lift, Air conditioning");
            _bus.Notes.Should().Be("Test notes");
        }

        [Test]
        public void MaintenanceProperties_ShouldBeSettableAndGettable()
        {
            // Arrange
            var nextMaintenanceDate = DateTime.Now.AddMonths(3);
            var lastServiceDate = DateTime.Now.AddMonths(-1);

            // Act
            _bus.NextMaintenanceDue = nextMaintenanceDate;
            _bus.NextMaintenanceMileage = 15000;
            _bus.LastServiceDate = lastServiceDate;

            // Assert
            _bus.NextMaintenanceDue.Should().Be(nextMaintenanceDate);
            _bus.NextMaintenanceMileage.Should().Be(15000);
            _bus.LastServiceDate.Should().Be(lastServiceDate);
        }

        [Test]
        public void AuditProperties_ShouldBeSettableAndGettable()
        {
            // Arrange
            var updateDate = DateTime.UtcNow;

            // Act
            _bus.UpdatedDate = updateDate;
            _bus.CreatedBy = "TestUser";
            _bus.UpdatedBy = "TestUser2";

            // Assert
            _bus.UpdatedDate.Should().Be(updateDate);
            _bus.CreatedBy.Should().Be("TestUser");
            _bus.UpdatedBy.Should().Be("TestUser2");
        }

        #endregion

        #region Helper Methods

        private static List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model);
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            return validationResults;
        }

        #endregion
    }
}
