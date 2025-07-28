using NUnit.Framework;
using FluentAssertions;
using System.Threading.Tasks;
using BusBuddy.WPF.ViewModels;
using BusBuddy.WPF.ViewModels.Vehicle;
using BusBuddy.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace BusBuddy.Tests
{
    [TestFixture]
    public class UITest_NavigationAndDisplay : IDisposable
    {
        private BusBuddyDbContext? _context;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<BusBuddyDbContext>()
                .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                .Options;
            _context = new BusBuddyDbContext(options);
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }

        public void Dispose()
        {
            _context?.Dispose();
            GC.SuppressFinalize(this);
        }

        [Test]
        public async Task DriversViewModel_LoadsDrivers()
        {
            // Arrange
            var vm = new DriversViewModel(_context!);

            // Act
            await vm.LoadDriversAsync();

            // Assert - Should handle empty database gracefully
            vm.Drivers.Should().NotBeNull("because the Drivers collection should be initialized");
        }

        [Test]
        public void VehiclesViewModel_InitializesCorrectly()
        {
            // Arrange & Act
            var vm = new BusBuddy.WPF.ViewModels.Vehicle.VehiclesViewModel();

            // Assert - Should initialize collections properly
            vm.Vehicles.Should().NotBeNull("because the Vehicles collection should be initialized");
        }

        [Test]
        public void ActivityScheduleViewModel_InitializesCorrectly()
        {
            // Arrange & Act
            using var vm = new ActivityScheduleViewModel();

            // Assert - Should initialize collections properly
            vm.ActivitySchedules.Should().NotBeNull("because the ActivitySchedules collection should be initialized");
        }
    }
}
