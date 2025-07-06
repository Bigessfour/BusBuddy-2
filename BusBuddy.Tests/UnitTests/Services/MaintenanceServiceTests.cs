using NUnit.Framework;
using FluentAssertions;
using Bus_Buddy.Models;

namespace BusBuddy.Tests.UnitTests.Services
{
    [TestFixture]
    public class MaintenanceServiceTests
    {
        [Test]
        public void Maintenance_CreateNew_ShouldWork()
        {
            // Arrange & Act
            var maintenance = new Maintenance();

            // Assert - just check it exists
            maintenance.Should().NotBeNull();
        }
    }
}
