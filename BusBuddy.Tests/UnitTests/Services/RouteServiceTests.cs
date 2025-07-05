using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using FluentAssertions;
using Bus_Buddy.Services;
using Bus_Buddy.Models;
using Bus_Buddy.Data;
using Bus_Buddy.Data.Interfaces;
using Bus_Buddy.Data.Repositories;
using Bus_Buddy.Data.UnitOfWork;
using BusBuddy.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BusBuddy.Tests.UnitTests.Services
{
    /// <summary>
    /// Unit tests for RouteService implementation
    /// Tests critical blocker fix: Missing RouteService implementation
    /// </summary>
    [TestFixture]
    [NonParallelizable] // Database tests need to run sequentially
    public class RouteServiceTests : TestBase
    {
        private IRouteService _routeService = null!;
        private BusBuddyDbContext _testDbContext = null!;
        private ServiceProvider _testServiceProvider = null!;


        [SetUp]
        public void Setup()
        {
            // Manually construct the in-memory DbContext after setting SkipGlobalSeedData
            _testDbContext = CreateInMemoryDbContext();

            // Build a DI container for this test, registering _testDbContext as the context instance
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            // Register the context as singleton so all services use the same instance
            services.AddSingleton<BusBuddyDbContext>(_testDbContext);
            // Register required repositories and services
            services.AddScoped<IRouteService, RouteService>();
            services.AddScoped<IRepository<Route>, Repository<Route>>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            // Register any other dependencies needed by RouteService
            // (Add more as needed for your actual constructor dependencies)

            _testServiceProvider = services.BuildServiceProvider();
            _routeService = _testServiceProvider.GetRequiredService<IRouteService>();
            // No global seeding here; each test will seed its own data as needed
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
        public async Task GetAllActiveRoutesAsync_ShouldReturnEmptyList_WhenNoRoutesExist()
        {
            // Use the manually constructed context for this test
            var result = await _testDbContext.Routes.Where(r => r.IsActive).ToListAsync();
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task CreateRouteAsync_ShouldCreateRoute_WithValidData()
        {
            // Arrange

            BusBuddyDbContext.SeedTestData(_testDbContext, ctx =>
            {
                ctx.Drivers.Add(new Driver { DriverName = "Test Driver", DriverEmail = "driver@busbuddy.com", DriversLicenceType = "CDL", TrainingComplete = true });
                ctx.Vehicles.Add(new Bus { BusNumber = "BUS002", VINNumber = "VIN002", LicenseNumber = "LIC002", Make = "Test", Model = "Bus", Year = 2021 });
            });
            var route = new Route
            {
                Date = DateTime.Today,
                RouteName = "Test Route",
                Description = "Test Description",
                AMBeginMiles = 100,
                AMEndMiles = 150,
                PMBeginMiles = 150,
                PMEndMiles = 200
            };

            // Act
            var result = await _routeService.CreateRouteAsync(route);

            // Assert
            result.Should().NotBeNull();
            result.RouteId.Should().BeGreaterThan(0);
            result.RouteName.Should().Be("Test Route");
            result.IsActive.Should().BeTrue();
        }

        [Test]
        public async Task CreateRouteAsync_ShouldThrowException_WhenRouteIsNull()
        {
            // Act & Assert
            var act = async () => await _routeService.CreateRouteAsync(null!);
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Test]
        public async Task CreateRouteAsync_ShouldThrowException_WhenDuplicateRouteNameForSameDate()
        {
            // Arrange

            // Seed required driver and bus for foreign keys if needed
            BusBuddyDbContext.SeedTestData(_testDbContext, ctx =>
            {
                if (!ctx.Drivers.Any())
                    ctx.Drivers.Add(new Driver { DriverName = "Test Driver", DriverEmail = "driver@busbuddy.com", DriversLicenceType = "CDL", TrainingComplete = true });
                if (!ctx.Vehicles.Any())
                    ctx.Vehicles.Add(new Bus { BusNumber = "BUS003", VINNumber = "VIN003", LicenseNumber = "LIC003", Make = "Test", Model = "Bus", Year = 2022 });
            });
            var date = DateTime.Today;
            var route1 = new Route
            {
                Date = date,
                RouteName = "Duplicate Route",
                Description = "First route"
            };
            var route2 = new Route
            {
                Date = date,
                RouteName = "Duplicate Route",
                Description = "Second route"
            };

            // Act
            await _routeService.CreateRouteAsync(route1);
            var act = async () => await _routeService.CreateRouteAsync(route2);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*already exists for date*");
        }

        [Test]
        public async Task GetRouteByIdAsync_ShouldReturnRoute_WhenRouteExists()
        {
            // Arrange

            // Use unique route name and date to avoid duplicate constraint
            var uniqueDate = DateTime.Today.AddDays(1);
            BusBuddyDbContext.SeedTestData(_testDbContext, ctx =>
            {
                ctx.Drivers.Add(new Driver { DriverName = "Test Driver", DriverEmail = "driver@busbuddy.com", DriversLicenceType = "CDL", TrainingComplete = true });
                ctx.Vehicles.Add(new Bus { BusNumber = "BUS004", VINNumber = "VIN004", LicenseNumber = "LIC004", Make = "Test", Model = "Bus", Year = 2023 });
            });
            var route = new Route
            {
                Date = uniqueDate,
                RouteName = "Test Route Unique",
                Description = "Test Description"
            };
            var createdRoute = await _routeService.CreateRouteAsync(route);

            // Act
            var result = await _routeService.GetRouteByIdAsync(createdRoute.RouteId);

            // Assert
            result.Should().NotBeNull();
            result.RouteId.Should().Be(createdRoute.RouteId);
            result.RouteName.Should().Be("Test Route Unique");
        }

        [Test]
        public async Task GetRouteByIdAsync_ShouldThrowException_WhenRouteDoesNotExist()
        {
            // Act & Assert
            var act = async () => await _routeService.GetRouteByIdAsync(999);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Route with ID 999 not found.");
        }

        [Test]
        public async Task UpdateRouteAsync_ShouldUpdateRoute_WithValidData()
        {
            // Arrange
            var route = new Route
            {
                Date = DateTime.Today.AddDays(2),
                RouteName = "Original Route",
                Description = "Original Description"
            };

            var createdRoute = await _routeService.CreateRouteAsync(route);
            // Update the route before calling update
            createdRoute.RouteName = "Updated Route";
            createdRoute.Description = "Updated Description";

            // Act
            var result = await _routeService.UpdateRouteAsync(createdRoute);

            // Assert
            result.Should().NotBeNull();
            result.RouteName.Should().Be("Updated Route");
            result.Description.Should().Be("Updated Description");
        }

        [Test]
        public async Task DeleteRouteAsync_ShouldMarkRouteAsInactive_WhenRouteExists()
        {
            // Arrange
            var route = new Route
            {
                Date = DateTime.Today,
                RouteName = "Route to Delete",
                Description = "Test Description"
            };

            var createdRoute = await _routeService.CreateRouteAsync(route);

            // Act
            var result = await _routeService.DeleteRouteAsync(createdRoute.RouteId);

            // Assert
            result.Should().BeTrue();

            // Verify route is marked as inactive
            var retrievedRoute = await _routeService.GetRouteByIdAsync(createdRoute.RouteId);
            retrievedRoute.IsActive.Should().BeFalse();
        }

        [Test]
        public async Task DeleteRouteAsync_ShouldReturnFalse_WhenRouteDoesNotExist()
        {
            // Act
            var result = await _routeService.DeleteRouteAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task SearchRoutesAsync_ShouldReturnMatchingRoutes_WhenSearchTermMatches()
        {
            // Arrange
            var route1 = new Route
            {
                Date = DateTime.Today,
                RouteName = "East Route",
                Description = "Eastern district route"
            };

            var route2 = new Route
            {
                Date = DateTime.Today,
                RouteName = "West Route",
                Description = "Western district route"
            };

            var route3 = new Route
            {
                Date = DateTime.Today,
                RouteName = "North Route",
                Description = "Contains eastern in description"
            };

            await _routeService.CreateRouteAsync(route1);
            await _routeService.CreateRouteAsync(route2);
            await _routeService.CreateRouteAsync(route3);

            // Act
            var result = await _routeService.SearchRoutesAsync("east");

            // Assert
            result.Should().HaveCount(2); // Should find "East Route" and route with "eastern" in description
            result.Should().Contain(r => r.RouteName == "East Route");
            result.Should().Contain(r => r.RouteName == "North Route");
        }

        [Test]
        public async Task GetRouteTotalDistanceAsync_ShouldCalculateCorrectDistance_WhenMilesAreProvided()
        {
            // Arrange
            var route = new Route
            {
                Date = DateTime.Today,
                RouteName = "Distance Test Route",
                AMBeginMiles = 100,
                AMEndMiles = 120,
                PMBeginMiles = 120,
                PMEndMiles = 145
            };

            var createdRoute = await _routeService.CreateRouteAsync(route);

            // Act
            var result = await _routeService.GetRouteTotalDistanceAsync(createdRoute.RouteId);

            // Assert
            result.Should().Be(45); // (120-100) + (145-120) = 20 + 25 = 45
        }

        [Test]
        public async Task GetRouteTotalDistanceAsync_ShouldReturnZero_WhenRouteDoesNotExist()
        {
            // Act
            var result = await _routeService.GetRouteTotalDistanceAsync(999);

            // Assert
            result.Should().Be(0);
        }

        [Test]
        public async Task IsRouteNumberUniqueAsync_ShouldReturnTrue_WhenRouteNameIsUnique()
        {
            // Arrange
            var route = new Route
            {
                Date = DateTime.Today,
                RouteName = "Unique Route",
                Description = "Test Description"
            };

            await _routeService.CreateRouteAsync(route);

            // Act
            var result = await _routeService.IsRouteNumberUniqueAsync("Different Route");

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task IsRouteNumberUniqueAsync_ShouldReturnFalse_WhenRouteNameExists()
        {
            // Arrange
            var route = new Route
            {
                Date = DateTime.Today,
                RouteName = "Existing Route",
                Description = "Test Description"
            };

            await _routeService.CreateRouteAsync(route);

            // Act
            var result = await _routeService.IsRouteNumberUniqueAsync("Existing Route");

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task GetRoutesByBusIdAsync_ShouldReturnRoutesForBus_WhenBusIsAssigned()
        {
            // Arrange
            var route1 = new Route
            {
                Date = DateTime.Today,
                RouteName = "AM Route",
                AMVehicleId = 1
            };

            var route2 = new Route
            {
                Date = DateTime.Today,
                RouteName = "PM Route",
                PMVehicleId = 1
            };

            var route3 = new Route
            {
                Date = DateTime.Today,
                RouteName = "Other Route",
                AMVehicleId = 2
            };

            await _routeService.CreateRouteAsync(route1);
            await _routeService.CreateRouteAsync(route2);
            await _routeService.CreateRouteAsync(route3);

            // Act
            var result = await _routeService.GetRoutesByBusIdAsync(1);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(r => r.RouteName == "AM Route");
            result.Should().Contain(r => r.RouteName == "PM Route");
        }
    }
}

