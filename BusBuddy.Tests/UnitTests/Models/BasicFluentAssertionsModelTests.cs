using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using FluentAssertions;
using Bus_Buddy.Models;

namespace BusBuddy.Tests.UnitTests.Models
{
    [TestFixture]
    public class BasicFluentAssertionsModelTests
    {
        [Test]
        public void Bus_Properties_ShouldBeSetCorrectly()
        {
            // Arrange - Basic Bus model test
            var bus = new Bus
            {
                BusNumber = "BUS001",
                Make = "Volvo",
                Model = "B7R",
                Year = 2020,
                SeatingCapacity = 45,
                Status = "Active"
            };

            // Assert - Basic property checks
            bus.Should().NotBeNull();
            bus.BusNumber.Should().Be("BUS001");
            bus.Make.Should().Be("Volvo");
            bus.Model.Should().Be("B7R");
            bus.Year.Should().Be(2020);
            bus.SeatingCapacity.Should().Be(45);
            bus.Status.Should().Be("Active");
        }

        [Test]
        public void Bus_SeatingCapacity_ShouldBePositive()
        {
            // Arrange
            var bus = new Bus { SeatingCapacity = 50 };

            // Assert
            bus.SeatingCapacity.Should().BeGreaterThan(0);
            bus.SeatingCapacity.Should().BeLessThan(100);
        }

        [Test]
        public void Bus_Year_ShouldBeReasonableRange()
        {
            // Arrange
            var bus = new Bus { Year = 2022 };

            // Assert
            bus.Year.Should().BeGreaterThan(1990);
            bus.Year.Should().BeLessThan(DateTime.Now.Year + 2);
        }

        [Test]
        public void Driver_Properties_ShouldBeSetCorrectly()
        {
            // Arrange
            var driver = new Driver
            {
                DriverName = "John Doe",
                LicenseNumber = "DL123456789",
                Status = "Active"
            };

            // Assert
            driver.Should().NotBeNull();
            driver.DriverName.Should().Be("John Doe");
            driver.LicenseNumber.Should().Be("DL123456789");
            driver.Status.Should().Be("Active");
        }

        [Test]
        public void Route_Properties_ShouldBeSetCorrectly()
        {
            // Arrange
            var route = new Route
            {
                RouteName = "Route 101",
                Description = "Downtown to Airport",
                Date = DateTime.Today
            };

            // Assert
            route.Should().NotBeNull();
            route.RouteName.Should().Be("Route 101");
            route.Description.Should().Be("Downtown to Airport");
            route.Date.Should().Be(DateTime.Today);
        }

        [Test]
        public void Activity_Properties_ShouldBeSetCorrectly()
        {
            // Arrange
            var activity = new Activity
            {
                StartTime = TimeSpan.FromHours(8),
                EndTime = TimeSpan.FromHours(10),
                ActivityDate = DateTime.Today
            };

            // Assert
            activity.Should().NotBeNull();
            activity.StartTime.Should().Be(TimeSpan.FromHours(8));
            activity.EndTime.Should().Be(TimeSpan.FromHours(10));
            activity.ActivityDate.Should().Be(DateTime.Today);
        }

        [Test]
        public void Student_Properties_ShouldBeSetCorrectly()
        {
            // Arrange
            var student = new Student
            {
                StudentName = "Alice Smith",
                Grade = "10th"
            };

            // Assert
            student.Should().NotBeNull();
            student.StudentName.Should().Be("Alice Smith");
            student.Grade.Should().Be("10th");
        }

        [Test]
        public void Collections_ShouldUseBasicFluentAssertions()
        {
            // Arrange
            var buses = new List<Bus>
            {
                new Bus { BusNumber = "BUS001", SeatingCapacity = 45, Status = "Active" },
                new Bus { BusNumber = "BUS002", SeatingCapacity = 50, Status = "Active" },
                new Bus { BusNumber = "BUS003", SeatingCapacity = 40, Status = "Inactive" }
            };

            // Assert
            buses.Should().NotBeNull();
            buses.Should().HaveCount(3);
            buses.Should().NotBeEmpty();
            buses.Should().Contain(b => b.BusNumber == "BUS001");
        }

        [Test]
        public void String_Properties_ShouldNotBeNullOrEmpty()
        {
            // Arrange
            var driver = new Driver
            {
                DriverName = "John Doe",
                LicenseNumber = "DL123456789"
            };

            // Assert
            driver.DriverName.Should().NotBeNullOrWhiteSpace();
            driver.LicenseNumber.Should().NotBeNullOrEmpty();
            driver.DriverName.Should().StartWith("John");
            driver.DriverName.Should().EndWith("Doe");
        }

        [Test]
        public void Numeric_Properties_ShouldHaveValidValues()
        {
            // Arrange
            var bus = new Bus
            {
                SeatingCapacity = 45,
                Year = 2020
            };

            // Assert
            bus.SeatingCapacity.Should().BeGreaterThan(0);
            bus.SeatingCapacity.Should().BeLessThan(100);
            bus.Year.Should().BeGreaterThan(1990);
            bus.Year.Should().BeLessThan(2030);
        }

        [Test]
        public void DateTime_Properties_ShouldHaveValidValues()
        {
            // Arrange
            var route = new Route
            {
                Date = DateTime.Today
            };

            // Assert
            route.Date.Should().Be(DateTime.Today);
            route.Date.Should().BeBefore(DateTime.Today.AddDays(1));
            route.Date.Should().BeAfter(DateTime.Today.AddDays(-1));
        }
    }
}
