using System;
using System.Windows.Forms;
using NUnit.Framework;
using FluentAssertions;
using Bus_Buddy.Forms;

namespace BusBuddy.Tests.UnitTests.Forms
{
    [TestFixture]
    [Apartment(System.Threading.ApartmentState.STA)]
    public class UtilityMethodsTests
    {
        [Test]
        public void HasProperty_WithExistingProperty_ShouldReturnTrue()
        {
            // Arrange
            var testObject = new { Name = "Test", Value = 42 };

            // Act
            var result = testObject.GetType().GetProperty("Name") != null;

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void HasProperty_WithNonExistingProperty_ShouldReturnFalse()
        {
            // Arrange
            var testObject = new { Name = "Test", Value = 42 };

            // Act
            var result = testObject.GetType().GetProperty("NonExistent") != null;

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void SetProperty_WithValidProperty_ShouldNotThrow()
        {
            // Arrange
            var testObject = new TestClass { Name = "Original" };
            var property = testObject.GetType().GetProperty("Name");

            // Act & Assert
            Assert.DoesNotThrow(() => property?.SetValue(testObject, "Updated"));
            testObject.Name.Should().Be("Updated");
        }

        [Test]
        public void SetProperty_WithReadOnlyProperty_ShouldNotThrow()
        {
            // Arrange
            var testObject = new TestClass();
            var property = testObject.GetType().GetProperty("ReadOnlyProperty");

            // Act & Assert - Handle read-only property gracefully
            if (property != null && property.CanWrite)
            {
                Assert.DoesNotThrow(() => property.SetValue(testObject, "NewValue"));
            }
            else
            {
                // For read-only properties, we expect it to be read-only
                property.Should().NotBeNull();
                property!.CanWrite.Should().BeFalse("ReadOnlyProperty should not have a setter");
            }
        }

        [Test]
        public void StatusType_EnumValues_ShouldHaveExpectedValues()
        {
            // Assert
            Enum.GetValues<StatusType>().Should().Contain(new[] {
                StatusType.Info,
                StatusType.Success,
                StatusType.Warning,
                StatusType.Error
            });
        }

        [Test]
        public void StatusType_DefaultValue_ShouldBeInfo()
        {
            // Arrange & Act
            StatusType defaultValue = default;

            // Assert
            defaultValue.Should().Be(StatusType.Info);
        }

        private class TestClass
        {
            public string Name { get; set; } = "Default";
            public string ReadOnlyProperty { get; } = "ReadOnly";
        }
    }
}
