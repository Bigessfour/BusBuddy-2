using NUnit.Framework;
using FluentAssertions;
using Bus_Buddy.Models;

namespace BusBuddy.Tests.UnitTests.Models
{
    [TestFixture]
    public class BusModelTests
    {
        [Test]
        public void Bus_CreateNew_ShouldWork()
        {
            // Arrange & Act
            var bus = new Bus();

            // Assert - just check it exists
            bus.Should().NotBeNull();
        }

        [Test]
        public void Bus_SetProperties_ShouldWork()
        {
            // Arrange & Act
            var bus = new Bus
            {
                BusNumber = "001",
                Model = "School Bus"
            };

            // Assert - just check basic properties work
            bus.BusNumber.Should().Be("001");
            bus.Model.Should().Be("School Bus");
        }
    }
}
