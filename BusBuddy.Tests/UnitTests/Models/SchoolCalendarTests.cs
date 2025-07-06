using NUnit.Framework;
using FluentAssertions;
using Bus_Buddy.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BusBuddy.Tests.UnitTests.Models
{
    [TestFixture]
    public class SchoolCalendarTests
    {
        [Test]
        public void SchoolCalendar_SmokeTest_CanBeCreatedAndPropertiesSet()
        {
            // Act & Assert - if this doesn't throw, basic model construction works
            Assert.DoesNotThrow(() =>
            {
                var calendar = new SchoolCalendar();
                calendar.Should().NotBeNull();
                // Set required properties
                calendar.Date = new DateTime(2025, 9, 1);
                calendar.EventType = "Holiday";
                calendar.EventName = "Labor Day";
                calendar.SchoolYear = "2025-2026";
                calendar.RoutesRequired = false;
                // Set optional properties
                calendar.StartDate = new DateTime(2025, 8, 31);
                calendar.EndDate = new DateTime(2025, 9, 2);
                calendar.Description = "Long weekend";
                calendar.Notes = "No school for students";
                calendar.UpdatedDate = DateTime.UtcNow;
                // Basic property validation
                calendar.EventType.Should().Be("Holiday");
                calendar.EventName.Should().Be("Labor Day");
                calendar.SchoolYear.Should().Be("2025-2026");
                calendar.Description.Should().Be("Long weekend");
                calendar.Notes.Should().Be("No school for students");
            });
        }

        [Test]
        public void SchoolCalendar_EventType_Should_Enforce_MaxLength_50()
        {
            // Arrange - Use explicit 51-character string to ensure validation failure
            var tooLongEventType = new string('A', 51); // Exactly 51 chars, exceeds 50 limit
            var calendar = new SchoolCalendar
            {
                Date = DateTime.Today,
                EventType = tooLongEventType, // This should fail validation
                EventName = "Test Event",
                SchoolYear = "2025-2026",
                RoutesRequired = true
            };
            var context = new ValidationContext(calendar);
            var results = new System.Collections.Generic.List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(calendar, context, results, true);

            // Assert
            isValid.Should().BeFalse("Object should fail validation with 51-character EventType");
            results.Should().NotBeEmpty("There should be at least one validation error");

            bool found = false;
            foreach (var r in results)
            {
                if (r != null && r.MemberNames != null && r.MemberNames.Contains("EventType") && r.ErrorMessage != null && r.ErrorMessage.Contains("maximum length of 50"))
                {
                    found = true;
                    break;
                }
            }
            found.Should().BeTrue("Expected a validation error for EventType max length");
        }
    }
}
