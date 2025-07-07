
using Moq;
using NUnit.Framework;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Bus_Buddy.Data;
using Bus_Buddy.Data.Repositories;
using Bus_Buddy.Models;
using Bus_Buddy.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace BusBuddy.Tests.UnitTests.Data.Repositories
{
    [TestFixture]
    public class RouteRepositoryTests
    {
        private Mock<BusBuddyDbContext> _mockDbContext;
        private Mock<IUserContextService> _mockUserContextService;
        private RouteRepository _repository;

        [SetUp]
        public void Setup()
        {
            // Mock the DbContext and UserContextService
            _mockDbContext = new Mock<BusBuddyDbContext>();
            _mockUserContextService = new Mock<IUserContextService>();

            // Setup repository with mocked dependencies
            _repository = new RouteRepository(_mockDbContext.Object, _mockUserContextService.Object);
        }

        /// <summary>
        /// Test case for GetTotalMileageByDateAsync to verify it returns zero when no routes are found.
        /// </summary>
        [Test]
        public async Task GetTotalMileageByDateAsync_NoRoutesFound_ShouldReturnZero()
        {
            // Arrange: Setup an empty list of routes
            var routes = new List<Route>().AsQueryable();
            var mockSet = new Mock<DbSet<Route>>();

            mockSet.As<IQueryable<Route>>().Setup(m => m.Provider).Returns(routes.Provider);
            mockSet.As<IQueryable<Route>>().Setup(m => m.Expression).Returns(routes.Expression);
            mockSet.As<IQueryable<Route>>().Setup(m => m.ElementType).Returns(routes.ElementType);
            mockSet.As<IQueryable<Route>>().Setup(m => m.GetEnumerator()).Returns(routes.GetEnumerator());

            _mockDbContext.Setup(c => c.Set<Route>()).Returns(mockSet.Object);

            // Act: Call the method with a specific date
            var result = await _repository.GetTotalMileageByDateAsync(DateTime.Now);

            // Assert: The result should be 0, as there are no routes
            result.Should().Be(0);
        }

        /// <summary>
        /// Test case for GetTotalMileageByDateAsync to verify correct mileage calculation for active routes.
        /// </summary>
        [Test]
        public async Task GetTotalMileageByDateAsync_WithActiveRoutes_ShouldReturnCorrectMileage()
        {
            // Arrange: Setup a list of routes with mileage data
            var date = DateTime.Now.Date;
            var routes = new List<Route>
            {
                new Route { Date = date, IsActive = true, AMBeginMiles = 100, AMEndMiles = 150, PMBeginMiles = 200, PMEndMiles = 275 }, // Total: 50 + 75 = 125
                new Route { Date = date, IsActive = true, AMBeginMiles = 300, AMEndMiles = 350, PMBeginMiles = 400, PMEndMiles = 450 }, // Total: 50 + 50 = 100
                new Route { Date = date, IsActive = false, AMBeginMiles = 500, AMEndMiles = 550, PMBeginMiles = 600, PMEndMiles = 650 }, // Inactive, should be ignored
                new Route { Date = date.AddDays(1), IsActive = true, AMBeginMiles = 700, AMEndMiles = 750, PMBeginMiles = 800, PMEndMiles = 850 } // Wrong date, should be ignored
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Route>>();
            mockSet.As<IQueryable<Route>>().Setup(m => m.Provider).Returns(routes.Provider);
            mockSet.As<IQueryable<Route>>().Setup(m => m.Expression).Returns(routes.Expression);
            mockSet.As<IQueryable<Route>>().Setup(m => m.ElementType).Returns(routes.ElementType);
            mockSet.As<IQueryable<Route>>().Setup(m => m.GetEnumerator()).Returns(routes.GetEnumerator());

            _mockDbContext.Setup(c => c.Set<Route>()).Returns(mockSet.Object);

            // Act: Call the method with the specified date
            var result = await _repository.GetTotalMileageByDateAsync(date);

            // Assert: The result should be the sum of mileage for active routes on the specified date (125 + 100 = 225)
            result.Should().Be(225);
        }

        /// <summary>
        /// Test case for GetTotalMileageByDateAsync to handle routes with null mileage values gracefully.
        /// </summary>
        [Test]
        public async Task GetTotalMileageByDateAsync_WithNullMileage_ShouldBeHandledGracefully()
        {
            // Arrange: Setup routes with some null mileage values
            var date = DateTime.Now.Date;
            var routes = new List<Route>
            {
                new Route { Date = date, IsActive = true, AMBeginMiles = 100, AMEndMiles = 150, PMBeginMiles = null, PMEndMiles = null }, // Total: 50
                new Route { Date = date, IsActive = true, AMBeginMiles = null, AMEndMiles = null, PMBeginMiles = 200, PMEndMiles = 250 }  // Total: 50
            }.AsQueryable();

            var mockSet = new Mock<DbSet<Route>>();
            mockSet.As<IQueryable<Route>>().Setup(m => m.Provider).Returns(routes.Provider);
            mockSet.As<IQueryable<Route>>().Setup(m => m.Expression).Returns(routes.Expression);
            mockSet.As<IQueryable<Route>>().Setup(m => m.ElementType).Returns(routes.ElementType);
            mockSet.As<IQueryable<Route>>().Setup(m => m.GetEnumerator()).Returns(routes.GetEnumerator());

            _mockDbContext.Setup(c => c.Set<Route>()).Returns(mockSet.Object);

            // Act: Call the method
            var result = await _repository.GetTotalMileageByDateAsync(date);

            // Assert: The result should correctly sum the non-null mileage (50 + 50 = 100)
            result.Should().Be(100);
        }
    }
}
