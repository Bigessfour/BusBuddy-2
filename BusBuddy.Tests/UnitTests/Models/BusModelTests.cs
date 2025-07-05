using NUnit.Framework;
using FluentAssertions;
using Bus_Buddy.Models;
using System.ComponentModel;

namespace BusBuddy.Tests.UnitTests.Models
{
    /// <summary>
    /// Comprehensive tests for Bus model including property validation, computed properties,
    /// and INotifyPropertyChanged implementation for Syncfusion data binding
    /// </summary>
    [TestFixture]
    public class BusModelTests
    {
        private Bus _bus = null!;

        [SetUp]
        public void Setup()
        {
            _bus = new Bus();
        }

        #region Property Change Notification Tests

        [Test]
        public void BusNumber_PropertyChanged_ShouldFireNotification()
        {
            // Arrange
            var propertyChangedFired = false;
            string? changedPropertyName = null;
            _bus.PropertyChanged += (sender, e) =>
            {
                propertyChangedFired = true;
                changedPropertyName = e.PropertyName;
            };

            // Act
            _bus.BusNumber = "BUS001";

            // Assert
            propertyChangedFired.Should().BeTrue();
            changedPropertyName.Should().Be(nameof(Bus.BusNumber));
            _bus.BusNumber.Should().Be("BUS001");
        }

        [Test]
        public void Year_PropertyChanged_ShouldFireNotificationForYearAndAge()
        {
            // Arrange
            var propertyChangedEvents = new List<string>();
            _bus.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != null)
                    propertyChangedEvents.Add(e.PropertyName);
            };

            // Act
            _bus.Year = 2020;

            // Assert
            propertyChangedEvents.Should().Contain(nameof(Bus.Year));
            propertyChangedEvents.Should().Contain(nameof(Bus.Age));
            _bus.Year.Should().Be(2020);
        }

        [Test]
        public void Make_PropertyChanged_ShouldFireNotificationForMakeAndFullDescription()
        {
            // Arrange
            var propertyChangedEvents = new List<string>();
            _bus.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != null)
                    propertyChangedEvents.Add(e.PropertyName);
            };

            // Act
            _bus.Make = "Blue Bird";

            // Assert
            propertyChangedEvents.Should().Contain(nameof(Bus.Make));
            propertyChangedEvents.Should().Contain(nameof(Bus.FullDescription));
            _bus.Make.Should().Be("Blue Bird");
        }

        [Test]
        public void Model_PropertyChanged_ShouldFireNotificationForModelAndFullDescription()
        {
            // Arrange
            var propertyChangedEvents = new List<string>();
            _bus.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != null)
                    propertyChangedEvents.Add(e.PropertyName);
            };

            // Act
            _bus.Model = "Vision";

            // Assert
            propertyChangedEvents.Should().Contain(nameof(Bus.Model));
            propertyChangedEvents.Should().Contain(nameof(Bus.FullDescription));
            _bus.Model.Should().Be("Vision");
        }

        [Test]
        public void DateLastInspection_PropertyChanged_ShouldFireNotificationForInspectionStatus()
        {
            // Arrange
            var propertyChangedEvents = new List<string>();
            _bus.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != null)
                    propertyChangedEvents.Add(e.PropertyName);
            };

            // Act
            _bus.DateLastInspection = DateTime.Now.AddMonths(-6);

            // Assert
            propertyChangedEvents.Should().Contain(nameof(Bus.DateLastInspection));
            propertyChangedEvents.Should().Contain(nameof(Bus.InspectionStatus));
        }

        [Test]
        public void InsuranceExpiryDate_PropertyChanged_ShouldFireNotificationForInsuranceStatus()
        {
            // Arrange
            var propertyChangedEvents = new List<string>();
            _bus.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != null)
                    propertyChangedEvents.Add(e.PropertyName);
            };

            // Act
            _bus.InsuranceExpiryDate = DateTime.Now.AddMonths(6);

            // Assert
            propertyChangedEvents.Should().Contain(nameof(Bus.InsuranceExpiryDate));
            propertyChangedEvents.Should().Contain(nameof(Bus.InsuranceStatus));
        }

        [Test]
        public void PropertyChanged_WithSameValue_ShouldNotFireNotification()
        {
            // Arrange
            _bus.BusNumber = "BUS001";
            var propertyChangedFired = false;
            _bus.PropertyChanged += (sender, e) => propertyChangedFired = true;

            // Act
            _bus.BusNumber = "BUS001"; // Same value

            // Assert
            propertyChangedFired.Should().BeFalse();
        }

        #endregion

        #region Computed Properties Tests

        [Test]
        public void Age_ShouldCalculateCorrectAge()
        {
            // Arrange
            var currentYear = DateTime.Now.Year;
            _bus.Year = currentYear - 5;

            // Act
            var age = _bus.Age;

            // Assert
            age.Should().Be(5);
        }

        [Test]
        public void FullDescription_ShouldCombineYearMakeModelAndBusNumber()
        {
            // Arrange
            _bus.Year = 2020;
            _bus.Make = "Blue Bird";
            _bus.Model = "Vision";
            _bus.BusNumber = "BUS001";

            // Act
            var description = _bus.FullDescription;

            // Assert
            description.Should().Be("2020 Blue Bird Vision (#BUS001)");
        }

        [Test]
        public void InspectionStatus_WithNoInspection_ShouldReturnOverdue()
        {
            // Arrange
            _bus.DateLastInspection = null;

            // Act
            var status = _bus.InspectionStatus;

            // Assert
            status.Should().Be("Overdue");
        }

        [Test]
        public void InspectionStatus_WithRecentInspection_ShouldReturnCurrent()
        {
            // Arrange
            _bus.DateLastInspection = DateTime.Now.AddMonths(-6);

            // Act
            var status = _bus.InspectionStatus;

            // Assert
            status.Should().Be("Current");
        }

        [Test]
        public void InspectionStatus_WithOverdueInspection_ShouldReturnOverdue()
        {
            // Arrange
            _bus.DateLastInspection = DateTime.Now.AddMonths(-15);

            // Act
            var status = _bus.InspectionStatus;

            // Assert
            status.Should().Be("Overdue");
        }

        [Test]
        public void InspectionStatus_WithDueSoonInspection_ShouldReturnDueSoon()
        {
            // Arrange - Exactly 11.5 months ago should be "Due Soon" (> 11 but <= 12)
            // Use 350 days to ensure we're clearly over 11 months: 350/30 = 11.67 months
            _bus.DateLastInspection = DateTime.Now.AddDays(-350);

            // Act
            var status = _bus.InspectionStatus;

            // Assert
            status.Should().Be("Due Soon");
        }

        [Test]
        public void InsuranceStatus_WithNoExpiryDate_ShouldReturnUnknown()
        {
            // Arrange
            _bus.InsuranceExpiryDate = null;

            // Act
            var status = _bus.InsuranceStatus;

            // Assert
            status.Should().Be("Unknown");
        }

        [Test]
        public void InsuranceStatus_WithCurrentInsurance_ShouldReturnCurrent()
        {
            // Arrange
            _bus.InsuranceExpiryDate = DateTime.Now.AddMonths(6);

            // Act
            var status = _bus.InsuranceStatus;

            // Assert
            status.Should().Be("Current");
        }

        [Test]
        public void InsuranceStatus_WithExpiredInsurance_ShouldReturnExpired()
        {
            // Arrange
            _bus.InsuranceExpiryDate = DateTime.Now.AddDays(-10);

            // Act
            var status = _bus.InsuranceStatus;

            // Assert
            status.Should().Be("Expired");
        }

        [Test]
        public void InsuranceStatus_WithExpiringSoonInsurance_ShouldReturnExpiringSoon()
        {
            // Arrange
            _bus.InsuranceExpiryDate = DateTime.Now.AddDays(15);

            // Act
            var status = _bus.InsuranceStatus;

            // Assert
            status.Should().Be("Expiring Soon");
        }

        [Test]
        public void IsAvailable_WithActiveStatus_ShouldReturnTrue()
        {
            // Arrange
            _bus.Status = "Active";

            // Act
            var isAvailable = _bus.IsAvailable;

            // Assert
            isAvailable.Should().BeTrue();
        }

        [Test]
        public void IsAvailable_WithNonActiveStatus_ShouldReturnFalse()
        {
            // Arrange
            _bus.Status = "Maintenance";

            // Act
            var isAvailable = _bus.IsAvailable;

            // Assert
            isAvailable.Should().BeFalse();
        }

        [Test]
        public void NeedsAttention_WithCurrentInspectionAndInsurance_ShouldReturnFalse()
        {
            // Arrange
            _bus.DateLastInspection = DateTime.Now.AddMonths(-6);
            _bus.InsuranceExpiryDate = DateTime.Now.AddMonths(6);

            // Act
            var needsAttention = _bus.NeedsAttention;

            // Assert
            needsAttention.Should().BeFalse();
        }

        [Test]
        public void NeedsAttention_WithOverdueInspection_ShouldReturnTrue()
        {
            // Arrange
            _bus.DateLastInspection = DateTime.Now.AddMonths(-15);
            _bus.InsuranceExpiryDate = DateTime.Now.AddMonths(6);

            // Act
            var needsAttention = _bus.NeedsAttention;

            // Assert
            needsAttention.Should().BeTrue();
        }

        [Test]
        public void NeedsAttention_WithExpiredInsurance_ShouldReturnTrue()
        {
            // Arrange
            _bus.DateLastInspection = DateTime.Now.AddMonths(-6);
            _bus.InsuranceExpiryDate = DateTime.Now.AddDays(-10);

            // Act
            var needsAttention = _bus.NeedsAttention;

            // Assert
            needsAttention.Should().BeTrue();
        }

        [Test]
        public void NeedsAttention_WithExpiringSoonInsurance_ShouldReturnTrue()
        {
            // Arrange
            _bus.DateLastInspection = DateTime.Now.AddMonths(-6);
            _bus.InsuranceExpiryDate = DateTime.Now.AddDays(15);

            // Act
            var needsAttention = _bus.NeedsAttention;

            // Assert
            needsAttention.Should().BeTrue();
        }

        #endregion

        #region Property Validation Tests

        [Test]
        public void BusNumber_SetToNull_ShouldSetToEmptyString()
        {
            // Act
            _bus.BusNumber = null!;

            // Assert
            _bus.BusNumber.Should().Be(string.Empty);
        }

        [Test]
        public void Make_SetToNull_ShouldSetToEmptyString()
        {
            // Act
            _bus.Make = null!;

            // Assert
            _bus.Make.Should().Be(string.Empty);
        }

        [Test]
        public void Model_SetToNull_ShouldSetToEmptyString()
        {
            // Act
            _bus.Model = null!;

            // Assert
            _bus.Model.Should().Be(string.Empty);
        }

        [Test]
        public void VINNumber_SetToNull_ShouldSetToEmptyString()
        {
            // Act
            _bus.VINNumber = null!;

            // Assert
            _bus.VINNumber.Should().Be(string.Empty);
        }

        [Test]
        public void LicenseNumber_SetToNull_ShouldSetToEmptyString()
        {
            // Act
            _bus.LicenseNumber = null!;

            // Assert
            _bus.LicenseNumber.Should().Be(string.Empty);
        }

        [Test]
        public void Status_SetToNull_ShouldSetToActive()
        {
            // Act
            _bus.Status = null!;

            // Assert
            _bus.Status.Should().Be("Active");
        }

        #endregion

        #region Default Values Tests

        [Test]
        public void CreatedDate_ShouldBeSetToUtcNow()
        {
            // Arrange & Act
            var bus = new Bus();

            // Assert
            bus.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }

        [Test]
        public void Status_ShouldDefaultToActive()
        {
            // Arrange & Act
            var bus = new Bus();

            // Assert
            bus.Status.Should().Be("Active");
        }

        [Test]
        public void GPSTracking_ShouldDefaultToFalse()
        {
            // Arrange & Act
            var bus = new Bus();

            // Assert
            bus.GPSTracking.Should().BeFalse();
        }

        [Test]
        public void NavigationProperties_ShouldBeInitialized()
        {
            // Arrange & Act
            var bus = new Bus();

            // Assert
            bus.AMRoutes.Should().NotBeNull();
            bus.PMRoutes.Should().NotBeNull();
            bus.Activities.Should().NotBeNull();
            bus.ScheduledActivities.Should().NotBeNull();
            bus.FuelRecords.Should().NotBeNull();
            bus.MaintenanceRecords.Should().NotBeNull();
        }

        #endregion

        #region Legacy Support Tests

        [Test]
        public void Routes_ShouldCombineAMAndPMRoutes()
        {
            // Arrange
            var amRoute = new Route { RouteId = 1, RouteName = "AM Route" };
            var pmRoute = new Route { RouteId = 2, RouteName = "PM Route" };

            _bus.AMRoutes.Add(amRoute);
            _bus.PMRoutes.Add(pmRoute);

            // Act
            var allRoutes = _bus.Routes;

            // Assert
            allRoutes.Should().HaveCount(2);
            allRoutes.Should().Contain(amRoute);
            allRoutes.Should().Contain(pmRoute);
        }

        #endregion

        #region Complex Scenarios Tests

        [Test]
        public void CompleteSetup_ShouldSetAllPropertiesCorrectly()
        {
            // Arrange & Act
            _bus.BusNumber = "BUS001";
            _bus.Year = 2020;
            _bus.Make = "Blue Bird";
            _bus.Model = "Vision";
            _bus.SeatingCapacity = 72;
            _bus.VINNumber = "1BAKBUCL4LF123456";
            _bus.LicenseNumber = "ABC123";
            _bus.Status = "Active";
            _bus.DateLastInspection = DateTime.Now.AddMonths(-6);
            _bus.InsuranceExpiryDate = DateTime.Now.AddMonths(6);
            _bus.CurrentOdometer = 25000;
            _bus.PurchaseDate = DateTime.Now.AddYears(-4);
            _bus.PurchasePrice = 85000m;
            _bus.FleetType = "Regular";
            _bus.FuelType = "Diesel";
            _bus.FuelCapacity = 100m;
            _bus.MilesPerGallon = 8.5m;
            _bus.GPSTracking = true;
            _bus.GPSDeviceId = "GPS001";
            _bus.SpecialEquipment = "Air Conditioning, Radio";
            _bus.Notes = "Test bus for unit testing";

            // Assert
            _bus.BusNumber.Should().Be("BUS001");
            _bus.Year.Should().Be(2020);
            _bus.Age.Should().Be(DateTime.Now.Year - 2020);
            _bus.FullDescription.Should().Be("2020 Blue Bird Vision (#BUS001)");
            _bus.InspectionStatus.Should().Be("Current");
            _bus.InsuranceStatus.Should().Be("Current");
            _bus.IsAvailable.Should().BeTrue();
            _bus.NeedsAttention.Should().BeFalse();
        }

        [Test]
        public void MultiplePropertyChanges_ShouldFireAllNotifications()
        {
            // Arrange
            var propertyChangedEvents = new List<string>();
            _bus.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != null)
                    propertyChangedEvents.Add(e.PropertyName);
            };

            // Act
            _bus.BusNumber = "BUS001";
            _bus.Year = 2020;
            _bus.Make = "Blue Bird";
            _bus.Model = "Vision";
            _bus.SeatingCapacity = 72;

            // Assert
            propertyChangedEvents.Should().Contain(nameof(Bus.BusNumber));
            propertyChangedEvents.Should().Contain(nameof(Bus.Year));
            propertyChangedEvents.Should().Contain(nameof(Bus.Age));
            propertyChangedEvents.Should().Contain(nameof(Bus.Make));
            propertyChangedEvents.Should().Contain(nameof(Bus.Model));
            propertyChangedEvents.Should().Contain(nameof(Bus.FullDescription));
            propertyChangedEvents.Should().Contain(nameof(Bus.SeatingCapacity));
        }

        #endregion
    }
}
