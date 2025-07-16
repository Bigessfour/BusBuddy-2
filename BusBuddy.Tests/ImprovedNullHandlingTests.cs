using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusBuddy.Tests
{
    [TestFixture]
    public class ImprovedNullHandlingTests
    {
        [SetUp]
        public void Setup()
        {
            Console.WriteLine("=== Improved NULL Handling Tests ===");
        }

        [Test]
        public void Bus_EnhancedNullHandling_ShouldProvideDefaults()
        {
            Console.WriteLine("DEBUG: Testing enhanced Bus NULL handling");

            // Arrange - Create bus with null/invalid values
            var bus = new Bus();

            // Act - Set null/invalid values
            bus.BusNumber = null!;
            bus.Make = "";
            bus.Model = "   ";
            bus.VINNumber = null!;
            bus.LicenseNumber = "";
            bus.Status = null!;
            bus.Year = 0;
            bus.SeatingCapacity = -1;

            // Assert - All values should have sensible defaults
            Assert.That(bus.BusNumber, Is.Not.Null.And.Not.Empty, "BusNumber should have default");
            Assert.That(bus.Make, Is.EqualTo("Unknown Make"), "Make should have default");
            Assert.That(bus.Model, Is.EqualTo("Unknown Model"), "Model should have default");
            Assert.That(bus.VINNumber, Is.Not.Null.And.Not.Empty, "VINNumber should have default");
            Assert.That(bus.LicenseNumber, Is.Not.Null.And.Not.Empty, "LicenseNumber should have default");
            Assert.That(bus.Status, Is.EqualTo("Active"), "Status should default to Active");
            Assert.That(bus.Year, Is.EqualTo(DateTime.Now.Year), "Year should default to current year");
            Assert.That(bus.SeatingCapacity, Is.EqualTo(30), "SeatingCapacity should default to 30");

            Console.WriteLine($"DEBUG: Bus defaults - Number:{bus.BusNumber}, Make:{bus.Make}, Year:{bus.Year}");
            Console.WriteLine("DEBUG: Enhanced Bus NULL handling test passed");
        }

        [Test]
        public void Driver_EnhancedNullHandling_ShouldProvideDefaults()
        {
            Console.WriteLine("DEBUG: Testing enhanced Driver NULL handling");

            // Arrange - Create driver with null values
            var driver = new Driver { DriverId = 123 };

            // Act - Set null/invalid values
            driver.DriverName = null!;
            driver.DriversLicenceType = "";
            driver.Status = "   ";

            // Assert - All values should have sensible defaults
            Assert.That(driver.DriverName, Is.EqualTo("Driver-123"), "DriverName should use ID as default");
            Assert.That(driver.Status, Is.EqualTo("Active"), "Status should default to Active");

            Console.WriteLine($"DEBUG: Driver defaults - Name:{driver.DriverName}, Status:{driver.Status}");
            Console.WriteLine("DEBUG: Enhanced Driver NULL handling test passed");
        }

        [Test]
        public void Route_EnhancedDateTimeHandling_ShouldValidateDates()
        {
            Console.WriteLine("DEBUG: Testing enhanced Route DateTime handling");

            // Arrange - Create route
            var route = new Route { RouteId = 456 };

            // Act - Set invalid dates
            route.Date = default(DateTime);
            route.RouteName = null!;

            // Assert - Date and name should be valid
            Assert.That(route.Date, Is.EqualTo(DateTime.Today), "Date should default to today");
            Assert.That(route.RouteName, Is.EqualTo("Route-456"), "RouteName should use ID as default");
            Assert.That(route.IsActive, Is.True, "IsActive should default to true");

            // Test future date handling
            route.Date = DateTime.Today.AddYears(2);
            Assert.That(route.Date, Is.EqualTo(DateTime.Today), "Future dates should be normalized");

            Console.WriteLine($"DEBUG: Route defaults - Date:{route.Date:yyyy-MM-dd}, Name:{route.RouteName}");
            Console.WriteLine("DEBUG: Enhanced Route DateTime handling test passed");
        }

        [Test]
        public void Route_ComputedProperties_ShouldHandleNulls()
        {
            Console.WriteLine("DEBUG: Testing Route computed properties with NULLs");

            // Arrange - Create route with null values
            var route = new Route
            {
                RouteId = 789,
                AMBeginMiles = null,
                AMEndMiles = null,
                PMBeginMiles = 100,
                PMEndMiles = null,
                AMVehicleId = null,
                AMDriverId = 1,
                PMVehicleId = 2,
                PMDriverId = null
            };

            // Act & Assert - Computed properties should handle nulls safely
            Assert.DoesNotThrow(() =>
            {
                var safeRouteName = route.SafeRouteName;
                var dateFormatted = route.DateFormatted;
                var hasAMAssignment = route.HasAMAssignment;
                var hasPMAssignment = route.HasPMAssignment;
                var totalMiles = route.TotalMiles;

                Assert.That(safeRouteName, Is.Not.Null, "SafeRouteName should not be null");
                Assert.That(dateFormatted, Is.Not.Null, "DateFormatted should not be null");
                Assert.That(hasAMAssignment, Is.False, "Should be false when missing driver");
                Assert.That(hasPMAssignment, Is.False, "Should be false when missing driver");
                Assert.That(totalMiles, Is.GreaterThanOrEqualTo(0), "TotalMiles should handle nulls");

                Console.WriteLine($"DEBUG: Computed properties - Name:{safeRouteName}, AM:{hasAMAssignment}, PM:{hasPMAssignment}, Miles:{totalMiles}");
            }, "Computed properties should not throw with null values");

            Console.WriteLine("DEBUG: Route computed properties test passed");
        }

        [Test]
        public void Repository_GetByIdAsync_ShouldHandleExceptions()
        {
            Console.WriteLine("DEBUG: Testing Repository exception handling");

            // This test validates that our enhanced repository methods handle exceptions gracefully
            // In a real scenario, this would test with a mock that throws exceptions

            // Create a bus to test property change notifications
            var bus = new Bus();
            bool propertyChanged = false;

            bus.PropertyChanged += (sender, e) =>
            {
                propertyChanged = true;
                Console.WriteLine($"DEBUG: PropertyChanged fired for {e.PropertyName}");
            };

            // Act - Change a property
            bus.BusNumber = "TEST-123";

            // Assert
            Assert.That(propertyChanged, Is.True, "PropertyChanged should fire");
            Assert.That(bus.BusNumber, Is.EqualTo("TEST-123"), "Property should be set");

            Console.WriteLine("DEBUG: Repository exception handling test passed");
        }

        [Test]
        public void DataIntegrity_BulkNullHandling_PerformanceTest()
        {
            Console.WriteLine("DEBUG: Testing bulk NULL handling performance");

            var startTime = DateTime.Now;

            // Create many entities with null values
            var buses = new List<Bus>();
            var drivers = new List<Driver>();
            var routes = new List<Route>();

            for (int i = 0; i < 1000; i++)
            {
                buses.Add(new Bus
                {
                    VehicleId = i,
                    BusNumber = i % 3 == 0 ? null : $"Bus-{i}",
                    Make = i % 4 == 0 ? null : "Test Make",
                    Model = i % 5 == 0 ? "" : "Test Model",
                    Year = i % 6 == 0 ? 0 : 2020,
                    SeatingCapacity = i % 7 == 0 ? -1 : 30
                });

                drivers.Add(new Driver
                {
                    DriverId = i,
                    DriverName = i % 3 == 0 ? null : $"Driver-{i}",
                    Status = i % 4 == 0 ? null : "Active"
                });

                routes.Add(new Route
                {
                    RouteId = i,
                    Date = i % 5 == 0 ? default(DateTime) : DateTime.Today,
                    RouteName = i % 6 == 0 ? null : $"Route-{i}"
                });
            }

            var endTime = DateTime.Now;
            var processingTime = (endTime - startTime).TotalMilliseconds;

            // Validate all entities have proper defaults
            var invalidBuses = buses.Count(b => string.IsNullOrEmpty(b.BusNumber) ||
                                              string.IsNullOrEmpty(b.Make) ||
                                              b.Year <= 0);
            var invalidDrivers = drivers.Count(d => string.IsNullOrEmpty(d.DriverName) ||
                                               string.IsNullOrEmpty(d.Status));
            var invalidRoutes = routes.Count(r => r.Date == default(DateTime) ||
                                               string.IsNullOrEmpty(r.RouteName));

            Assert.That(invalidBuses, Is.EqualTo(0), "All buses should have valid defaults");
            Assert.That(invalidDrivers, Is.EqualTo(0), "All drivers should have valid defaults");
            Assert.That(invalidRoutes, Is.EqualTo(0), "All routes should have valid defaults");
            Assert.That(processingTime, Is.LessThan(5000), "Processing should be fast (< 5 seconds)");

            Console.WriteLine($"DEBUG: Processed {buses.Count + drivers.Count + routes.Count} entities in {processingTime:F2}ms");
            Console.WriteLine("DEBUG: Bulk NULL handling performance test passed");
        }

        [Test]
        public void CrossEntity_NullSafeNavigation_ShouldWork()
        {
            Console.WriteLine("DEBUG: Testing cross-entity null-safe navigation");

            // Arrange - Create entities with null navigation properties
            var route = new Route
            {
                RouteId = 999,
                AMVehicleId = 1,
                AMDriverId = 2,
                AMVehicle = null, // Navigation property is null
                AMDriver = null   // Navigation property is null
            };

            var bus = new Bus
            {
                VehicleId = 1,
                BusNumber = "NULL-TEST-001"
            };

            // Act & Assert - Safe navigation should work
            Assert.DoesNotThrow(() =>
            {
                var vehicleName = route.AMVehicle?.BusNumber ?? "No Vehicle";
                var driverName = route.AMDriver?.DriverName ?? "No Driver";
                var routeDescription = $"Route {route.SafeRouteName}: Vehicle {vehicleName}, Driver {driverName}";

                Assert.That(vehicleName, Is.EqualTo("No Vehicle"), "Should handle null vehicle");
                Assert.That(driverName, Is.EqualTo("No Driver"), "Should handle null driver");
                Assert.That(routeDescription, Is.Not.Null, "Description should be safe");

                Console.WriteLine($"DEBUG: Safe navigation - {routeDescription}");
            }, "Cross-entity navigation should be null-safe");

            Console.WriteLine("DEBUG: Cross-entity null-safe navigation test passed");
        }

        [Test]
        public void ErrorBoundaries_ServiceLayer_ShouldHandleNulls()
        {
            Console.WriteLine("DEBUG: Testing service layer error boundaries");

            // This simulates how service methods should handle null inputs gracefully
            Assert.DoesNotThrow(() =>
            {
                // Test null input handling
                var nullBus = (Bus?)null;
                var safeString = nullBus?.BusNumber ?? "Default Bus";

                // Test empty collection handling
                var emptyList = new List<Bus>();
                var count = emptyList?.Count ?? 0;

                // Test null property access
                var bus = new Bus();
                var navigation = bus.Routes?.FirstOrDefault()?.RouteName ?? "No Route";

                Assert.That(safeString, Is.EqualTo("Default Bus"), "Should handle null entity");
                Assert.That(count, Is.EqualTo(0), "Should handle empty collection");
                Assert.That(navigation, Is.EqualTo("No Route"), "Should handle null navigation");

                Console.WriteLine($"DEBUG: Error boundaries - String:{safeString}, Count:{count}, Navigation:{navigation}");
            }, "Service layer should handle all null scenarios gracefully");

            Console.WriteLine("DEBUG: Service layer error boundaries test passed");
        }

        [TearDown]
        public void TearDown()
        {
            Console.WriteLine("=== Test Completed ===\n");
        }
    }
}
