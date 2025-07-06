using NUnit.Framework;
using FluentAssertions;
using Bus_Buddy.Models;
using System.Threading.Tasks;

namespace BusBuddy.Tests.UnitTests.Services
{
    [TestFixture]
    public class BusServiceTests
    {
        [Test]
        public void Bus_CanBeCreated_AndPropertiesSet()
        {
            // Arrange & Act
            var bus = new Bus
            {
                BusNumber = "BUS001",
                Model = "School Bus",
                SeatingCapacity = 72,
                Year = 2023,
                Status = "Active"
            };

            // Assert
            bus.Should().NotBeNull();
            bus.BusNumber.Should().Be("BUS001");
            bus.Model.Should().Be("School Bus");
            bus.SeatingCapacity.Should().Be(72);
            bus.Year.Should().Be(2023);
            bus.Status.Should().Be("Active");
        }
    }
}
