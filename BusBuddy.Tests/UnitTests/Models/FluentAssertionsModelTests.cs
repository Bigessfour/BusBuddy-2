using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using FluentAssertions;
using Bus_Buddy.Models;

namespace BusBuddy.Tests.UnitTests.Models
{
    [TestFixture]
    public class FluentAssertionsModelTests
    {
        [Test]
        public void Bus_Properties_ShouldBeSetCorrectly()
        {
            // Arrange - Basic Bus model test with expected dates
            var expectedDate = new DateTime(2020, 1, 15);
            var expectedInsuranceExpiry = DateTime.Today.AddYears(1);

            var bus = new Bus
            {
                BusNumber = "BUS001",
                Make = "Volvo",
                Model = "B7R",
                Year = 2020,
                SeatingCapacity = 45,
                Status = "Active",
                VINNumber = "1HGBH41JXMN109186",
                LicenseNumber = "ABC123",
                PurchaseDate = expectedDate,
                InsuranceExpiryDate = expectedInsuranceExpiry
            };

            // Assert - Basic property checks
            bus.Should().NotBeNull();
            bus.BusNumber.Should().Be("BUS001");
            bus.Make.Should().Be("Volvo");
            bus.Model.Should().Be("B7R");
            bus.Year.Should().Be(2020);
            bus.SeatingCapacity.Should().Be(45);
            bus.Status.Should().Be("Active");

            bus.Make.Should().Be("Volvo")
                .And.NotBeNullOrEmpty("Make is required for vehicle identification");

            bus.Model.Should().Be("B7R")
                .And.NotBeNullOrEmpty("Model is required for vehicle specification");

            // Numeric properties with validation ranges
            bus.Year.Should().Be(2020)
                .And.BeInRange(1900, 2030, "Year should be within reasonable vehicle age range");

            bus.SeatingCapacity.Should().Be(45)
                .And.BeInRange(1, 90, "Seating capacity should be within bus standards");

            // VIN Number validation
            bus.VINNumber.Should().Be("1HGBH41JXMN109186")
                .And.HaveLength(17, "VIN should be exactly 17 characters")
                .And.NotContain(" ", "VIN should not contain spaces");

            bus.LicenseNumber.Should().Be("ABC123")
                .And.NotBeNullOrWhiteSpace("License number is required");

            // Date properties
            bus.PurchaseDate.Should().Be(expectedDate)
                .And.BeBefore(DateTime.Today, "Purchase date should be in the past");

            bus.InsuranceExpiryDate.Should().Be(expectedInsuranceExpiry)
                .And.BeAfter(DateTime.Today, "Insurance should be valid for future dates");

            bus.Status.Should().Be("Active")
                .And.BeOneOf("Active", "Inactive", "Maintenance", "Retired");
        }

        [Test]
        public void Bus_SeatingCapacity_ShouldBePositive()
        {
            // Arrange
            var bus = new Bus { SeatingCapacity = 50 };

            // Assert - Using correct FluentAssertions 8.4.0 syntax
            bus.SeatingCapacity.Should().BePositive()
                .And.BeGreaterThan(0)
                .And.BeLessThan(91); // Fixed: BeLessOrEqualTo → BeLessThan
        }

        [Test]
        public void Bus_Year_ShouldBeReasonableRange()
        {
            // Arrange
            var bus = new Bus { Year = 2022 };

            // Assert
            bus.Year.Should().BeInRange(1900, DateTime.Now.Year + 1);
        }

        [Test]
        public void Driver_Properties_ShouldBeSetCorrectly()
        {
            // Arrange - Using actual Driver model properties
            var driver = new Driver
            {
                DriverName = "John Doe",
                LicenseNumber = "DL123456789",
                DriverPhone = "555-1234", // Correct property name
                HireDate = new DateTime(2020, 6, 1)
            };

            // Assert using FluentAssertions with correct properties
            driver.DriverName.Should().NotBeNullOrWhiteSpace()
                .And.Be("John Doe")
                .And.HaveLength(8);

            driver.LicenseNumber.Should().NotBeNullOrEmpty()
                .And.StartWith("DL")
                .And.HaveLength(11);

            driver.DriverPhone.Should().Be("555-1234")
                .And.Contain("-");

            driver.HireDate.Should().Be(new DateTime(2020, 6, 1))
                .And.BeBefore(DateTime.Now);
        }

        [Test]
        public void Route_Description_ShouldNotBeEmpty()
        {
            // Arrange
            var route = new Route
            {
                RouteName = "Route 101",
                Description = "Downtown to Airport",
                Date = DateTime.Today
            };

            // Assert
            route.RouteName.Should().NotBeNullOrWhiteSpace();
            route.Description.Should().NotBeNullOrEmpty()
                .And.Contain("Downtown")
                .And.EndWith("Airport");
            route.Date.Should().Be(DateTime.Today);
        }

        [Test]
        public void Activity_TimeSpan_ShouldBeValid()
        {
            // Arrange
            var activity = new Activity
            {
                StartTime = TimeSpan.FromHours(8),
                EndTime = TimeSpan.FromHours(10),
                ActivityDate = DateTime.Today
            };

            // Assert
            activity.StartTime.Should().Be(TimeSpan.FromHours(8));
            activity.EndTime.Should().Be(TimeSpan.FromHours(10));
            activity.EndTime.Should().BeGreaterThan(activity.StartTime.Value); // Fixed nullable TimeSpan comparison

            var duration = activity.EndTime - activity.StartTime;
            duration.Should().Be(TimeSpan.FromHours(2))
                .And.BePositive();

            activity.ActivityDate.Should().Be(DateTime.Today);
        }

        [Test]
        public void Student_Properties_ShouldFollowBusinessRules()
        {
            // Arrange - Using correct Student model properties
            var student = new Student
            {
                StudentName = "Alice Smith",
                Grade = "10th"
            };

            // Assert - Using actual Student properties
            student.StudentName.Should().NotBeNullOrWhiteSpace()
                .And.Be("Alice Smith")
                .And.Contain(" "); // Contains space between first and last name

            student.Grade.Should().NotBeNullOrWhiteSpace()
                .And.Be("10th")
                .And.EndWith("th");
        }

        [Test]
        public void Collections_ShouldUseFluentAssertions()
        {
            // Arrange - Using correct Bus model properties
            var buses = new List<Bus>
            {
                new Bus { BusNumber = "BUS001", SeatingCapacity = 45, Status = "Active" },
                new Bus { BusNumber = "BUS002", SeatingCapacity = 50, Status = "Active" },
                new Bus { BusNumber = "BUS003", SeatingCapacity = 40, Status = "Inactive" }
            };

            // Assert - Using working FluentAssertions methods
            buses.Should().NotBeNull()
                .And.HaveCount(3)
                .And.NotBeEmpty();

            buses.Should().OnlyContain(b => !string.IsNullOrEmpty(b.BusNumber));
            buses.Should().Contain(b => b.BusNumber == "BUS001");
            buses.Should().NotContain(b => b.SeatingCapacity < 30);

            // Fixed: Use Status instead of IsActive
            buses.Where(b => b.Status == "Active").Should().HaveCount(2);
            buses.Select(b => b.SeatingCapacity).Should().OnlyContain(x => x > 0); // Fixed: AllBe → OnlyContain

            var activeBuses = buses.Where(b => b.Status == "Active").ToList();
            activeBuses.Should().OnlyContain(b => b.Status == "Active");
        }

        [Test]
        public void Exceptions_ShouldUseFluentAssertions()
        {
            // Arrange
            var bus = new Bus();

            // Assert
            Action act = () => ValidateBus(bus);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*BusNumber*")
                .And.ParamName.Should().Be("bus");
        }

        [Test]
        public void Nullable_Properties_ShouldBeHandledCorrectly()
        {
            // Arrange
            Bus? nullBus = null;
            Bus validBus = new Bus { BusNumber = "BUS001" };

            // Assert
            nullBus.Should().BeNull();
            validBus.Should().NotBeNull();
            validBus.BusNumber.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void DateTime_ShouldUseFluentAssertions()
        {
            // Arrange
            var bus = new Bus
            {
                PurchaseDate = new DateTime(2020, 1, 15),
                InsuranceExpiryDate = DateTime.Today.AddYears(1)
            };

            // Assert
            bus.PurchaseDate.Should().Be(new DateTime(2020, 1, 15));
            bus.PurchaseDate.Should().BeBefore(DateTime.Now);
            bus.PurchaseDate.Should().BeAfter(new DateTime(2019, 12, 31));
            bus.PurchaseDate.Should().BeOnOrBefore(DateTime.Today);

            bus.InsuranceExpiryDate.Should().BeAfter(DateTime.Today);
            bus.InsuranceExpiryDate.Should().BeCloseTo(DateTime.Today.AddYears(1), TimeSpan.FromDays(1));
        }

        [Test]
        public void Numeric_Properties_ShouldUseFluentAssertions()
        {
            // Arrange
            var bus = new Bus
            {
                SeatingCapacity = 45,
                Year = 2020
            };

            // Assert - Fixed FluentAssertions API calls
            bus.SeatingCapacity.Should().BePositive()
                .And.BeGreaterThan(0)
                .And.BeLessThan(101) // Fixed: BeLessOrEqualTo → BeLessThan
                .And.BeInRange(20, 80);

            bus.Year.Should().BeGreaterThan(1989) // Fixed: BeGreaterOrEqualTo → BeGreaterThan
                .And.BeLessThan(DateTime.Now.Year + 2); // Fixed: BeLessOrEqualTo → BeLessThan
        }

        [Test]
        public void String_Properties_ShouldUseFluentAssertions()
        {
            // Arrange
            var driver = new Driver
            {
                DriverName = "John Doe",
                LicenseNumber = "DL123456789",
                DriverPhone = "555-123-4567" // Fixed: PhoneNumber → DriverPhone
            };

            // Assert
            driver.DriverName.Should().NotBeNullOrWhiteSpace()
                .And.StartWith("John")
                .And.EndWith("Doe")
                .And.Contain(" ")
                .And.HaveLength(8)
                .And.Match("J* D*");

            driver.LicenseNumber.Should().StartWith("DL")
                .And.MatchRegex(@"^DL\d{9}$");

            driver.DriverPhone.Should().Match("555-123-4567") // Fixed: PhoneNumber → DriverPhone
                .And.ContainAll("555", "123", "4567");
        }

        private void ValidateBus(Bus bus)
        {
            if (string.IsNullOrEmpty(bus.BusNumber))
                throw new ArgumentException("BusNumber is required", nameof(bus));
        }
    }
}
