using NUnit.Framework;
using FluentAssertions;
using Bus_Buddy.Services;
using Bus_Buddy.Data;
using Bus_Buddy.Data.Interfaces;
using Bus_Buddy.Data.Repositories;
using Bus_Buddy.Data.UnitOfWork;
using Bus_Buddy.Models;
using BusBuddy.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BusBuddy.Tests.UnitTests.Services
{
    /// <summary>
    /// Unit tests for BusService implementation
    /// Tests existing service functionality to ensure regression testing
    /// </summary>
    [TestFixture]
    [NonParallelizable] // Database tests need to run sequentially
    [Parallelizable(ParallelScope.Children)]
    [Category("Unit")]
    [Category("Services")]
    public class BusServiceTests : TestBase
    {
        private IBusService _busService = null!;
        private BusBuddyDbContext _testDbContext = null!;
        private ServiceProvider _testServiceProvider = null!;

        [SetUp]
        public void Setup()
        {
            // Manually construct the in-memory DbContext after setting SkipGlobalSeedData
            _testDbContext = CreateInMemoryDbContext();

            // Build a DI container for this test, registering _testDbContext as the context instance
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddLogging();
            services.AddSingleton<BusBuddyDbContext>(_testDbContext);
            services.AddScoped<IBusService, BusService>();
            services.AddScoped<IRepository<Bus>, Repository<Bus>>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            // Register any other dependencies needed by BusService
            _testServiceProvider = services.BuildServiceProvider();
            _busService = _testServiceProvider.GetRequiredService<IBusService>();
        }

        [TearDown]
        public void TearDown()
        {
            _testDbContext?.Dispose();
            _testDbContext = null!;
            if (_testServiceProvider != null)
            {
                _testServiceProvider.Dispose();
                _testServiceProvider = null!;
            }
        }

        [Test]
        public async Task GetAllBusEntitiesAsync_ShouldReturnEmptyList_WhenNoBusesExist()
        {
            // Act
            var result = await _busService.GetAllBusEntitiesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task AddBusEntityAsync_ShouldCreateBus_WithValidData()
        {
            // Arrange
            var bus = new Bus
            {
                BusNumber = "BUS001",
                Model = "School Bus",
                SeatingCapacity = 72,
                Year = 2023,
                Status = "Active"
            };

            // Act
            var result = await _busService.AddBusEntityAsync(bus);

            // Assert
            result.Should().NotBeNull();
            result.VehicleId.Should().BeGreaterThan(0);
            result.BusNumber.Should().Be("BUS001");
            result.IsAvailable.Should().BeTrue();
        }

        [Test]
        public async Task GetBusEntityByIdAsync_ShouldReturnBus_WhenBusExists()
        {
            // Arrange
            var bus = new Bus
            {
                BusNumber = "BUS002",
                Model = "School Bus",
                SeatingCapacity = 72,
                Year = 2023,
                Status = "Active"
            };

            var createdBus = await _busService.AddBusEntityAsync(bus);

            // Act
            var result = await _busService.GetBusEntityByIdAsync(createdBus.VehicleId);

            // Assert
            result.Should().NotBeNull();
            result!.VehicleId.Should().Be(createdBus.VehicleId);
            result.BusNumber.Should().Be("BUS002");
        }

        [Test]
        public async Task GetBusEntityByIdAsync_ShouldReturnNull_WhenBusDoesNotExist()
        {
            // Act
            var result = await _busService.GetBusEntityByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task UpdateBusEntityAsync_ShouldUpdateBus_WithValidData()
        {
            // Arrange
            var bus = new Bus
            {
                BusNumber = "BUS003",
                Model = "School Bus",
                SeatingCapacity = 72,
                Year = 2023,
                Status = "Active"
            };

            var createdBus = await _busService.AddBusEntityAsync(bus);
            createdBus.Model = "Updated School Bus";
            createdBus.SeatingCapacity = 80;

            // Act
            var result = await _busService.UpdateBusEntityAsync(createdBus);

            // Assert
            result.Should().BeTrue();

            // Verify the update
            var updatedBus = await _busService.GetBusEntityByIdAsync(createdBus.VehicleId);
            updatedBus!.Model.Should().Be("Updated School Bus");
            updatedBus.SeatingCapacity.Should().Be(80);
        }

        [Test]
        public async Task DeleteBusEntityAsync_ShouldMarkBusAsInactive_WhenBusExists()
        {
            // Arrange
            var bus = new Bus
            {
                BusNumber = "BUS004",
                Model = "School Bus",
                SeatingCapacity = 72,
                Year = 2023,
                Status = "Active"
            };

            var createdBus = await _busService.AddBusEntityAsync(bus);

            // Act
            var result = await _busService.DeleteBusEntityAsync(createdBus.VehicleId);

            // Assert
            result.Should().BeTrue();

            // Verify the bus is marked as inactive (soft delete)
            var deletedBus = await _busService.GetBusEntityByIdAsync(createdBus.VehicleId);
            deletedBus?.IsAvailable.Should().BeFalse();
        }

        [Test]
        public async Task DeleteBusEntityAsync_ShouldReturnFalse_WhenBusDoesNotExist()
        {
            // Act
            var result = await _busService.DeleteBusEntityAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task GetAllDriversAsync_ShouldReturnEmptyList_WhenNoDriversExist()
        {
            // Act
            var result = await _busService.GetAllDriversAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task AddDriverEntityAsync_ShouldCreateDriver_WithValidData()
        {
            // Arrange
            var driver = new Driver
            {
                FirstName = "John",
                LastName = "Doe",
                LicenseNumber = "DL123456",
                DriverPhone = "555-1234",
                Status = "Active",
                TrainingComplete = true,
                LicenseExpiryDate = DateTime.Now.AddYears(2) // Valid license for 2 years
            };

            // Act
            var result = await _busService.AddDriverEntityAsync(driver);

            // Assert
            result.Should().NotBeNull();
            result.DriverId.Should().BeGreaterThan(0);
            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Doe");
            result.IsAvailable.Should().BeTrue();
        }

        [Test]
        public async Task GetAllRouteEntitiesAsync_ShouldReturnEmptyList_WhenNoRoutesExist()
        {
            // Act
            var result = await _busService.GetAllRouteEntitiesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetAllBusesAsync_ShouldReturnLegacyBusInfoList()
        {
            // Arrange
            var bus = new Bus
            {
                BusNumber = "BUS005",
                Model = "School Bus",
                SeatingCapacity = 72,
                Year = 2023,
                Status = "Active"
            };

            await _busService.AddBusEntityAsync(bus);

            // Act
            var result = await _busService.GetAllBusesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().BusNumber.Should().Be("BUS005");
        }
    }
}

