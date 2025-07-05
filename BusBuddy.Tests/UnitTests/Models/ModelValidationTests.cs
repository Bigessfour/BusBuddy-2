using NUnit.Framework;
using FluentAssertions;
using Bus_Buddy.Models;
using System;

namespace BusBuddy.Tests.UnitTests.Models
{
    /// <summary>
    /// Simple model validation tests to increase coverage
    /// These tests validate basic model properties and computed values
    /// </summary>
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    [Category("Unit")]
    [Category("Models")]
    public class ModelValidationTests
    {
        [Test]
        public void Bus_Age_ShouldCalculateCorrectly()
        {
            // Arrange
            var bus = new Bus
            {
                Year = DateTime.Now.Year - 5
            };

            // Act
            var age = bus.Age;

            // Assert
            age.Should().Be(5);
        }

        [Test]
        public void Bus_FullDescription_ShouldFormatCorrectly()
        {
            // Arrange
            var bus = new Bus
            {
                Year = 2020,
                Make = "Blue Bird",
                Model = "Vision",
                BusNumber = "001"
            };

            // Act
            var description = bus.FullDescription;

            // Assert
            description.Should().Be("2020 Blue Bird Vision (#001)");
        }

        [Test]
        public void Bus_InspectionStatus_ShouldReturnOverdueWhenNull()
        {
            // Arrange
            var bus = new Bus();

            // Act
            var status = bus.InspectionStatus;

            // Assert
            status.Should().Be("Overdue");
        }

        [Test]
        public void Bus_InsuranceStatus_ShouldReturnUnknownWhenNull()
        {
            // Arrange
            var bus = new Bus();

            // Act
            var status = bus.InsuranceStatus;

            // Assert
            status.Should().Be("Unknown");
        }

        [Test]
        public void Student_FullName_ShouldReturnStudentName()
        {
            // Arrange
            var student = new Student
            {
                StudentName = "John Doe"
            };

            // Act
            var fullName = student.FullName;

            // Assert
            fullName.Should().Be("John Doe");
        }

        [Test]
        public void Student_GradeLevel_ShouldReturnUnknownWhenGradeIsNull()
        {
            // Arrange
            var student = new Student
            {
                Grade = null
            };

            // Act
            var gradeLevel = student.GradeLevel;

            // Assert
            gradeLevel.Should().Be("Unknown");
        }

        [Test]
        public void Student_GradeLevel_ShouldReturnGradeWhenNotNull()
        {
            // Arrange
            var student = new Student
            {
                Grade = "5"
            };

            // Act
            var gradeLevel = student.GradeLevel;

            // Assert
            gradeLevel.Should().Be("5");
        }

        [Test]
        public void Student_ContactInfo_ShouldFormatCorrectly()
        {
            // Arrange
            var student = new Student
            {
                ParentGuardian = "Jane Doe",
                HomePhone = "(555) 123-4567"
            };

            // Act
            var contactInfo = student.ContactInfo;

            // Assert
            contactInfo.Should().Be("Jane Doe - (555) 123-4567");
        }

        [Test]
        public void Student_ContactInfo_ShouldHandleNullValues()
        {
            // Arrange
            var student = new Student
            {
                ParentGuardian = null,
                HomePhone = null
            };

            // Act
            var contactInfo = student.ContactInfo;

            // Assert
            contactInfo.Should().Be("");
        }

        [Test]
        public void Route_ShouldHaveDefaultActiveStatus()
        {
            // Arrange & Act
            var route = new Route();

            // Assert
            route.IsActive.Should().BeTrue();
        }

        [Test]
        public void Driver_ShouldHaveDefaultTrainingStatus()
        {
            // Arrange & Act
            var driver = new Driver();

            // Assert
            driver.TrainingComplete.Should().BeFalse();
        }
    }
}
