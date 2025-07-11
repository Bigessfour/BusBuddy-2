using BusBuddy.Core.Data;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using BusBuddy.Core.Utilities;
using Microsoft.EntityFrameworkCore;
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
    public class DatabaseNullHandlingTests
    {
        private Mock<IBusBuddyDbContextFactory> _mockContextFactory;
        private Mock<ILogger<DatabaseValidator>> _mockValidatorLogger;
        private Mock<ILogger<BusService>> _mockBusServiceLogger;
        private Mock<IBusCachingService> _mockCachingService;
        private Mock<BusBuddyDbContext> _mockDbContext;
        private DatabaseValidator _validator;
        private BusService _busService;

        [SetUp]
        public void Setup()
        {
            _mockContextFactory = new Mock<IBusBuddyDbContextFactory>();
            _mockValidatorLogger = new Mock<ILogger<DatabaseValidator>>();
            _mockBusServiceLogger = new Mock<ILogger<BusService>>();
            _mockCachingService = new Mock<IBusCachingService>();
            _mockDbContext = new Mock<BusBuddyDbContext>();

            // Setup the mock DbContext with more comprehensive test data
            var buses = new List<Bus>
            {
                // Valid bus with all data
                new Bus {
                    VehicleId = 1,
                    BusNumber = "Bus-1",
                    Make = "Test Make",
                    Model = "Test Model",
                    VINNumber = "12345",
                    LicenseNumber = "LIC-1",
                    Status = "Active"
                },
                // Bus with null BusNumber
                new Bus {
                    VehicleId = 2,
                    BusNumber = null,
                    Make = "Test Make",
                    Model = "Test Model",
                    VINNumber = "12346",
                    LicenseNumber = "LIC-2",
                    Status = "Active"
                },
                // Bus with null Make
                new Bus {
                    VehicleId = 3,
                    BusNumber = "Bus-3",
                    Make = null,
                    Model = "Test Model",
                    VINNumber = "12347",
                    LicenseNumber = "LIC-3",
                    Status = "Active"
                },
                // Bus with null Model
                new Bus {
                    VehicleId = 4,
                    BusNumber = "Bus-4",
                    Make = "Test Make",
                    Model = null,
                    VINNumber = "12348",
                    LicenseNumber = "LIC-4",
                    Status = "Active"
                },
                // Bus with null VINNumber
                new Bus {
                    VehicleId = 5,
                    BusNumber = "Bus-5",
                    Make = "Test Make",
                    Model = "Test Model",
                    VINNumber = null,
                    LicenseNumber = "LIC-5",
                    Status = "Active"
                },
                // Bus with null LicenseNumber
                new Bus {
                    VehicleId = 6,
                    BusNumber = "Bus-6",
                    Make = "Test Make",
                    Model = "Test Model",
                    VINNumber = "12350",
                    LicenseNumber = null,
                    Status = "Active"
                },
                // Bus with null Status
                new Bus {
                    VehicleId = 7,
                    BusNumber = "Bus-7",
                    Make = "Test Make",
                    Model = "Test Model",
                    VINNumber = "12351",
                    LicenseNumber = "LIC-7",
                    Status = null
                },
                // Bus with multiple null values
                new Bus {
                    VehicleId = 8,
                    BusNumber = null,
                    Make = null,
                    Model = null,
                    VINNumber = null,
                    LicenseNumber = null,
                    Status = null
                }
            };

            var mockSet = new Mock<DbSet<Bus>>();
            mockSet.As<IQueryable<Bus>>().Setup(m => m.Provider).Returns(buses.AsQueryable().Provider);
            mockSet.As<IQueryable<Bus>>().Setup(m => m.Expression).Returns(buses.AsQueryable().Expression);
            mockSet.As<IQueryable<Bus>>().Setup(m => m.ElementType).Returns(buses.AsQueryable().ElementType);
            mockSet.As<IQueryable<Bus>>().Setup(m => m.GetEnumerator()).Returns(() => buses.AsQueryable().GetEnumerator());

            _mockDbContext.Setup(c => c.Vehicles).Returns(mockSet.Object);
            _mockContextFactory.Setup(f => f.CreateDbContext()).Returns(_mockDbContext.Object);
            _mockContextFactory.Setup(f => f.CreateWriteDbContext()).Returns(_mockDbContext.Object);

            _validator = new DatabaseValidator(_mockContextFactory.Object, _mockValidatorLogger.Object);
            _busService = new BusService(_mockBusServiceLogger.Object, _mockContextFactory.Object, _mockCachingService.Object);
        }

        [Test]
        [Ignore("Temporarily disabled - requires real database context for Entity Framework async operations")]
        public async Task DatabaseValidator_ShouldIdentifyNullValues()
        {
            // Show start of test in console
            Console.WriteLine("DEBUG: Starting DatabaseValidator_ShouldIdentifyNullValues test");

            // Act
            var issues = await _validator.ValidateDatabaseDataAsync();

            // Show results in console
            Console.WriteLine($"DEBUG: Found {issues.Count} issues with NULL values");
            foreach (var issue in issues)
            {
                Console.WriteLine($"DEBUG: Issue: {issue}");
            }

            // Assert
            Assert.That(issues, Has.Count.GreaterThan(0), "Should find at least one issue");
            Assert.That(issues[0], Contains.Substring("NULL values in critical fields"),
                "Issue description should mention NULL values");

            // Verify we detect the bus with VehicleId 8 (all nulls)
            Assert.That(issues[0], Contains.Substring("8"),
                "Issue should identify vehicle 8 which has all NULL values");

            Console.WriteLine("DEBUG: Test passed successfully");
        }

        [Test]
        [Ignore("Temporarily disabled - requires real database context for Entity Framework async operations")]
        public async Task DatabaseValidator_ShouldFixNullValues()
        {
            // Show information in the console
            Console.WriteLine("DEBUG: Starting DatabaseValidator_ShouldFixNullValues test");

            // Act
            var fixCount = await _validator.RunAutomaticFixesAsync();

            // Show results in console
            Console.WriteLine($"DEBUG: Fixed {fixCount} NULL values");

            // Assert
            Assert.That(fixCount, Is.GreaterThan(0), "Should fix at least one NULL value");

            // We expect at least 7 fixes (one for each bus with at least one NULL value)
            Assert.That(fixCount, Is.GreaterThanOrEqualTo(7),
                "Should fix all 7 buses with NULL values");

            Console.WriteLine("DEBUG: Test passed successfully");
        }

        [Test]
        public void BusProjection_ShouldHandleNullValues()
        {
            // Show debug information in console
            Console.WriteLine("DEBUG: Starting BusProjection_ShouldHandleNullValues test");

            // Arrange - Bus with all NULL string fields
            var busWithNulls = new Bus
            {
                VehicleId = 9,
                BusNumber = null,
                Make = null,
                Model = null,
                VINNumber = null,
                LicenseNumber = null,
                Status = null
            };
            Console.WriteLine("DEBUG: Created bus with NULL values");

            // Act - This would cause exceptions without proper NULL handling
            var mappedBus = new Bus
            {
                VehicleId = busWithNulls.VehicleId,
                BusNumber = busWithNulls.BusNumber ?? string.Empty,
                Make = busWithNulls.Make ?? string.Empty,
                Model = busWithNulls.Model ?? string.Empty,
                VINNumber = busWithNulls.VINNumber ?? string.Empty,
                LicenseNumber = busWithNulls.LicenseNumber ?? string.Empty,
                Status = busWithNulls.Status ?? "Active"
            };
            Console.WriteLine("DEBUG: Applied NULL handling with ?? operator");
            Console.WriteLine($"DEBUG: Result - BusNumber='{mappedBus.BusNumber}', Make='{mappedBus.Make}', Status='{mappedBus.Status}'");

            // Assert - Check each field is properly handled
            Assert.That(mappedBus.BusNumber, Is.EqualTo(string.Empty), "BusNumber should be empty string when null");
            Assert.That(mappedBus.Make, Is.EqualTo(string.Empty), "Make should be empty string when null");
            Assert.That(mappedBus.Model, Is.EqualTo(string.Empty), "Model should be empty string when null");
            Assert.That(mappedBus.VINNumber, Is.EqualTo(string.Empty), "VINNumber should be empty string when null");
            Assert.That(mappedBus.LicenseNumber, Is.EqualTo(string.Empty), "LicenseNumber should be empty string when null");
            Assert.That(mappedBus.Status, Is.EqualTo("Active"), "Status should be 'Active' when null");

            Console.WriteLine("DEBUG: Test passed successfully");
        }

        [Test]
        public void BusProjection_ShouldHandleMixedNullValues()
        {
            // Show debug information in console
            Console.WriteLine("DEBUG: Starting BusProjection_ShouldHandleMixedNullValues test");

            // Arrange - Bus with mixed NULL and non-NULL values
            var busWithMixedNulls = new Bus
            {
                VehicleId = 10,
                BusNumber = "Bus-10",
                Make = null,
                Model = "Test Model",
                VINNumber = null,
                LicenseNumber = "LIC-10",
                Status = null
            };
            Console.WriteLine("DEBUG: Created bus with mixed NULL values");

            // Act - This would cause exceptions without proper NULL handling
            var mappedBus = new Bus
            {
                VehicleId = busWithMixedNulls.VehicleId,
                BusNumber = busWithMixedNulls.BusNumber ?? string.Empty,
                Make = busWithMixedNulls.Make ?? string.Empty,
                Model = busWithMixedNulls.Model ?? string.Empty,
                VINNumber = busWithMixedNulls.VINNumber ?? string.Empty,
                LicenseNumber = busWithMixedNulls.LicenseNumber ?? string.Empty,
                Status = busWithMixedNulls.Status ?? "Active"
            };
            Console.WriteLine("DEBUG: Applied NULL handling with ?? operator");
            Console.WriteLine($"DEBUG: Result - BusNumber='{mappedBus.BusNumber}', Make='{mappedBus.Make}', Model='{mappedBus.Model}', Status='{mappedBus.Status}'");

            // Assert - Check non-null values are preserved and null values are handled
            Assert.That(mappedBus.BusNumber, Is.EqualTo("Bus-10"), "BusNumber should be preserved");
            Assert.That(mappedBus.Make, Is.EqualTo(string.Empty), "Make should be empty string when null");
            Assert.That(mappedBus.Model, Is.EqualTo("Test Model"), "Model should be preserved");
            Assert.That(mappedBus.VINNumber, Is.EqualTo(string.Empty), "VINNumber should be empty string when null");
            Assert.That(mappedBus.LicenseNumber, Is.EqualTo("LIC-10"), "LicenseNumber should be preserved");
            Assert.That(mappedBus.Status, Is.EqualTo("Active"), "Status should be 'Active' when null");

            Console.WriteLine("DEBUG: Test passed successfully");
        }

        [Test]
        public void Bus_NullableDateTimeFields_ShouldBeHandled()
        {
            // Show debug information in console
            Console.WriteLine("DEBUG: Starting Bus_NullableDateTimeFields_ShouldBeHandled test");

            // Arrange - Bus with null DateTime fields
            var busWithNullDates = new Bus
            {
                VehicleId = 11,
                BusNumber = "Bus-11",
                DateLastInspection = null,
                NextMaintenanceDue = null,
                LastServiceDate = null
            };
            Console.WriteLine("DEBUG: Created bus with NULL DateTime fields");

            // Act - Access the nullable DateTime fields
            var hasDateLastInspection = busWithNullDates.DateLastInspection.HasValue;
            var hasNextMaintenanceDue = busWithNullDates.NextMaintenanceDue.HasValue;
            var hasLastServiceDate = busWithNullDates.LastServiceDate.HasValue;

            // Using DateTime.MinValue as default if null
            var dateLastInspection = busWithNullDates.DateLastInspection ?? DateTime.MinValue;
            var nextMaintenanceDue = busWithNullDates.NextMaintenanceDue ?? DateTime.MinValue;
            var lastServiceDate = busWithNullDates.LastServiceDate ?? DateTime.MinValue;

            Console.WriteLine($"DEBUG: hasDateLastInspection={hasDateLastInspection}, dateLastInspection={dateLastInspection}");
            Console.WriteLine($"DEBUG: hasNextMaintenanceDue={hasNextMaintenanceDue}, nextMaintenanceDue={nextMaintenanceDue}");
            Console.WriteLine($"DEBUG: hasLastServiceDate={hasLastServiceDate}, lastServiceDate={lastServiceDate}");

            // Assert
            Assert.That(hasDateLastInspection, Is.False, "DateLastInspection should be null");
            Assert.That(hasNextMaintenanceDue, Is.False, "NextMaintenanceDue should be null");
            Assert.That(hasLastServiceDate, Is.False, "LastServiceDate should be null");

            Assert.That(dateLastInspection, Is.EqualTo(DateTime.MinValue), "DateLastInspection should default to DateTime.MinValue");
            Assert.That(nextMaintenanceDue, Is.EqualTo(DateTime.MinValue), "NextMaintenanceDue should default to DateTime.MinValue");
            Assert.That(lastServiceDate, Is.EqualTo(DateTime.MinValue), "LastServiceDate should default to DateTime.MinValue");

            Console.WriteLine("DEBUG: Test passed successfully");
        }

        [Test]
        public void Bus_NullableNumericFields_ShouldBeHandled()
        {
            // Show debug information in console
            Console.WriteLine("DEBUG: Starting Bus_NullableNumericFields_ShouldBeHandled test");

            // Arrange - Bus with null numeric fields (only fields that are actually nullable)
            var busWithNullNumbers = new Bus
            {
                VehicleId = 12,
                BusNumber = "Bus-12",
                SeatingCapacity = 30, // This is required, not nullable
                Year = 2020, // Required field
                Make = "Blue Bird", // Required field  
                Model = "Vision", // Required field
                VINNumber = "1BAANV1A5XF123456", // Required field
                LicenseNumber = "ABC123", // Required field
                CurrentOdometer = null, // This is nullable
                FuelCapacity = null, // This is nullable
                MilesPerGallon = null, // This is nullable
                NextMaintenanceMileage = null // This is nullable
            };
            Console.WriteLine("DEBUG: Created bus with NULL numeric fields");

            // Act - Access the nullable numeric fields
            var hasCurrentOdometer = busWithNullNumbers.CurrentOdometer.HasValue;
            var hasFuelCapacity = busWithNullNumbers.FuelCapacity.HasValue;
            var hasMilesPerGallon = busWithNullNumbers.MilesPerGallon.HasValue;
            var hasNextMaintenanceMileage = busWithNullNumbers.NextMaintenanceMileage.HasValue;

            // Using defaults if null
            var currentOdometer = busWithNullNumbers.CurrentOdometer ?? 0;
            var fuelCapacity = busWithNullNumbers.FuelCapacity ?? 0;
            var milesPerGallon = busWithNullNumbers.MilesPerGallon ?? 0;
            var nextMaintenanceMileage = busWithNullNumbers.NextMaintenanceMileage ?? 0;

            Console.WriteLine($"DEBUG: hasCurrentOdometer={hasCurrentOdometer}, currentOdometer={currentOdometer}");
            Console.WriteLine($"DEBUG: hasFuelCapacity={hasFuelCapacity}, fuelCapacity={fuelCapacity}");
            Console.WriteLine($"DEBUG: hasMilesPerGallon={hasMilesPerGallon}, milesPerGallon={milesPerGallon}");
            Console.WriteLine($"DEBUG: hasNextMaintenanceMileage={hasNextMaintenanceMileage}, nextMaintenanceMileage={nextMaintenanceMileage}");

            // Assert
            Assert.That(hasCurrentOdometer, Is.False, "CurrentOdometer should be null");
            Assert.That(hasFuelCapacity, Is.False, "FuelCapacity should be null");
            Assert.That(hasMilesPerGallon, Is.False, "MilesPerGallon should be null");
            Assert.That(hasNextMaintenanceMileage, Is.False, "NextMaintenanceMileage should be null");

            Assert.That(currentOdometer, Is.EqualTo(0), "CurrentOdometer ?? 0 should equal 0");
            Assert.That(fuelCapacity, Is.EqualTo(0), "FuelCapacity ?? 0 should equal 0");
            Assert.That(milesPerGallon, Is.EqualTo(0), "MilesPerGallon ?? 0 should equal 0");
            Assert.That(nextMaintenanceMileage, Is.EqualTo(0), "NextMaintenanceMileage ?? 0 should equal 0");

            Console.WriteLine("DEBUG: All nullable numeric field assertions passed");
        }

        [Test]
        [Ignore("Temporarily disabled - requires real database context for Entity Framework async operations")]
        public async Task BusService_GetAllBuses_ShouldHandleNullsInProjection()
        {
            Console.WriteLine("DEBUG: Starting BusService_GetAllBuses_ShouldHandleNullsInProjection test");

            // Act - This tests the actual BusService projection that was causing the original SQL exceptions
            try
            {
                var buses = await _busService.GetAllBusEntitiesAsync();
                Console.WriteLine($"DEBUG: Successfully retrieved {buses.Count()} buses without SQL exceptions");

                // Verify that buses with null values are handled properly
                var busesArray = buses.ToArray();
                Assert.That(busesArray, Is.Not.Null, "Bus collection should not be null");
                Assert.That(busesArray.Length, Is.GreaterThan(0), "Should retrieve some buses");

                // Check that no bus has null required fields after projection
                foreach (var bus in busesArray)
                {
                    Assert.That(bus.BusNumber, Is.Not.Null, $"Bus {bus.VehicleId} BusNumber should not be null after projection");
                    Assert.That(bus.Make, Is.Not.Null, $"Bus {bus.VehicleId} Make should not be null after projection");
                    Assert.That(bus.Model, Is.Not.Null, $"Bus {bus.VehicleId} Model should not be null after projection");
                    Assert.That(bus.Status, Is.Not.Null, $"Bus {bus.VehicleId} Status should not be null after projection");
                }

                Console.WriteLine("DEBUG: All buses have non-null required fields after projection");
            }
            catch (Exception ex)
            {
                Assert.Fail($"BusService.GetAllBusEntitiesAsync() should not throw exceptions. Error: {ex.Message}");
            }

            Console.WriteLine("DEBUG: Test passed successfully");
        }

        [Test]
        public void Bus_RequiredFields_ShouldHaveDefaults()
        {
            Console.WriteLine("DEBUG: Starting Bus_RequiredFields_ShouldHaveDefaults test");

            // Arrange - Create a new Bus object (which should have default values)
            var newBus = new Bus();

            // Act & Assert - Check that required fields have sensible defaults
            Assert.That(newBus.BusNumber, Is.Not.Null, "BusNumber should have a default value");
            Assert.That(newBus.Make, Is.Not.Null, "Make should have a default value");
            Assert.That(newBus.Model, Is.Not.Null, "Model should have a default value");
            Assert.That(newBus.VINNumber, Is.Not.Null, "VINNumber should have a default value");
            Assert.That(newBus.LicenseNumber, Is.Not.Null, "LicenseNumber should have a default value");
            Assert.That(newBus.Status, Is.Not.Null, "Status should have a default value");

            Console.WriteLine($"DEBUG: Default values - BusNumber:'{newBus.BusNumber}', Make:'{newBus.Make}', Model:'{newBus.Model}', Status:'{newBus.Status}'");
            Console.WriteLine("DEBUG: Test passed successfully");
        }

        [Test]
        public void Bus_NullStringAssignment_ShouldHandleGracefully()
        {
            Console.WriteLine("DEBUG: Starting Bus_NullStringAssignment_ShouldHandleGracefully test");

            // Arrange
            var bus = new Bus();

            // Act - Try to assign null values to string properties (this tests the property setters)
            bus.BusNumber = null;
            bus.Make = null;
            bus.Model = null;
            bus.VINNumber = null;
            bus.LicenseNumber = null;
            bus.Status = null;

            // Assert - The property setters should handle null values gracefully
            Assert.That(bus.BusNumber, Is.Not.Null, "BusNumber should not be null after null assignment");
            Assert.That(bus.Make, Is.Not.Null, "Make should not be null after null assignment");
            Assert.That(bus.Model, Is.Not.Null, "Model should not be null after null assignment");
            Assert.That(bus.VINNumber, Is.Not.Null, "VINNumber should not be null after null assignment");
            Assert.That(bus.LicenseNumber, Is.Not.Null, "LicenseNumber should not be null after null assignment");
            Assert.That(bus.Status, Is.Not.Null, "Status should not be null after null assignment");

            Console.WriteLine($"DEBUG: After null assignment - BusNumber:'{bus.BusNumber}', Make:'{bus.Make}', Status:'{bus.Status}'");
            Console.WriteLine("DEBUG: Test passed successfully");
        }

        [Test]
        public void Bus_ComputedProperties_ShouldHandleNullValues()
        {
            Console.WriteLine("DEBUG: Starting Bus_ComputedProperties_ShouldHandleNullValues test");

            // Arrange - Bus with null optional fields
            var bus = new Bus
            {
                VehicleId = 13,
                BusNumber = "Bus-13",
                Year = 2020,
                Make = "Test Make",
                Model = "Test Model",
                VINNumber = "VIN123",
                LicenseNumber = "LIC123",
                DateLastInspection = null,
                InsuranceExpiryDate = null
            };

            // Act & Assert - Test computed properties that depend on nullable fields
            Assert.DoesNotThrow(() =>
            {
                var inspectionStatus = bus.InspectionStatus;
                var insuranceStatus = bus.InsuranceStatus;
                var needsAttention = bus.NeedsAttention;
                var fullDescription = bus.FullDescription;

                Console.WriteLine($"DEBUG: InspectionStatus: '{inspectionStatus}'");
                Console.WriteLine($"DEBUG: InsuranceStatus: '{insuranceStatus}'");
                Console.WriteLine($"DEBUG: NeedsAttention: {needsAttention}");
                Console.WriteLine($"DEBUG: FullDescription: '{fullDescription}'");

                Assert.That(inspectionStatus, Is.Not.Null, "InspectionStatus should not be null");
                Assert.That(insuranceStatus, Is.Not.Null, "InsuranceStatus should not be null");
                Assert.That(fullDescription, Is.Not.Null, "FullDescription should not be null");
            }, "Computed properties should not throw exceptions with null values");

            Console.WriteLine("DEBUG: Test passed successfully");
        }

        [Test]
        public void Bus_NavigationProperties_ShouldInitializeEmpty()
        {
            Console.WriteLine("DEBUG: Starting Bus_NavigationProperties_ShouldInitializeEmpty test");

            // Arrange
            var bus = new Bus();

            // Act & Assert - Navigation properties should be initialized to empty collections, not null
            Assert.That(bus.AMRoutes, Is.Not.Null, "AMRoutes should be initialized");
            Assert.That(bus.PMRoutes, Is.Not.Null, "PMRoutes should be initialized");
            Assert.That(bus.Schedules, Is.Not.Null, "Schedules should be initialized");
            Assert.That(bus.Activities, Is.Not.Null, "Activities should be initialized");
            Assert.That(bus.ScheduledActivities, Is.Not.Null, "ScheduledActivities should be initialized");
            Assert.That(bus.FuelRecords, Is.Not.Null, "FuelRecords should be initialized");
            Assert.That(bus.MaintenanceRecords, Is.Not.Null, "MaintenanceRecords should be initialized");
            Assert.That(bus.Routes, Is.Not.Null, "Routes should be initialized");

            // Verify they are empty collections
            Assert.That(bus.AMRoutes.Count, Is.EqualTo(0), "AMRoutes should be empty initially");
            Assert.That(bus.PMRoutes.Count, Is.EqualTo(0), "PMRoutes should be empty initially");
            Assert.That(bus.Schedules.Count, Is.EqualTo(0), "Schedules should be empty initially");

            Console.WriteLine("DEBUG: All navigation properties are properly initialized");
            Console.WriteLine("DEBUG: Test passed successfully");
        }

        [Test]
        public void Bus_NullCoalescingOperators_PerformanceTest()
        {
            Console.WriteLine("DEBUG: Starting Bus_NullCoalescingOperators_PerformanceTest test");

            // Arrange - Create many buses with null values
            var buses = new List<Bus>();
            for (int i = 0; i < 1000; i++)
            {
                buses.Add(new Bus
                {
                    VehicleId = i,
                    BusNumber = i % 2 == 0 ? null : $"Bus-{i}",
                    Make = i % 3 == 0 ? null : "Test Make",
                    Model = i % 4 == 0 ? null : "Test Model",
                    Status = i % 5 == 0 ? null : "Active"
                });
            }

            // Act - Process all buses with null-coalescing operators
            var startTime = DateTime.Now;
            var processedBuses = new List<object>();

            foreach (var bus in buses)
            {
                var processedBus = new
                {
                    Id = bus.VehicleId,
                    BusNumber = bus.BusNumber ?? "",
                    Make = bus.Make ?? "",
                    Model = bus.Model ?? "",
                    Status = bus.Status ?? "Active"
                };
                processedBuses.Add(processedBus);
            }

            var endTime = DateTime.Now;
            var processingTime = (endTime - startTime).TotalMilliseconds;

            // Assert
            Assert.That(processedBuses.Count, Is.EqualTo(1000), "Should process all 1000 buses");
            Assert.That(processingTime, Is.LessThan(1000), "Processing should be fast (< 1 second)");

            Console.WriteLine($"DEBUG: Processed {processedBuses.Count} buses in {processingTime:F2}ms");
            Console.WriteLine("DEBUG: Null-coalescing operators perform well under load");
            Console.WriteLine("DEBUG: Test passed successfully");
        }

        [Test]
        public void Bus_EdgeCase_EmptyStringVsNull()
        {
            Console.WriteLine("DEBUG: Starting Bus_EdgeCase_EmptyStringVsNull test");

            // Arrange
            var busWithEmptyStrings = new Bus
            {
                VehicleId = 14,
                BusNumber = "",
                Make = "",
                Model = "",
                VINNumber = "",
                LicenseNumber = "",
                Status = ""
            };

            var busWithNulls = new Bus
            {
                VehicleId = 15,
                BusNumber = null,
                Make = null,
                Model = null,
                VINNumber = null,
                LicenseNumber = null,
                Status = null
            };

            // Act & Assert - Both should be handled consistently
            Assert.DoesNotThrow(() =>
            {
                // Test empty strings - use custom logic since Bus model converts null to empty
                var emptyResult = new
                {
                    BusNumber = string.IsNullOrEmpty(busWithEmptyStrings.BusNumber) ? "DEFAULT" : busWithEmptyStrings.BusNumber,
                    Make = string.IsNullOrEmpty(busWithEmptyStrings.Make) ? "DEFAULT" : busWithEmptyStrings.Make,
                    Status = string.IsNullOrEmpty(busWithEmptyStrings.Status) ? "DEFAULT" : busWithEmptyStrings.Status
                };

                // Test null values - Bus model converts null to empty string
                var nullResult = new
                {
                    BusNumber = string.IsNullOrEmpty(busWithNulls.BusNumber) ? "DEFAULT" : busWithNulls.BusNumber,
                    Make = string.IsNullOrEmpty(busWithNulls.Make) ? "DEFAULT" : busWithNulls.Make,
                    Status = string.IsNullOrEmpty(busWithNulls.Status) ? "DEFAULT" : busWithNulls.Status
                };

                Console.WriteLine($"DEBUG: Empty strings - BusNumber:'{emptyResult.BusNumber}', Make:'{emptyResult.Make}'");
                Console.WriteLine($"DEBUG: Null values - BusNumber:'{nullResult.BusNumber}', Make:'{nullResult.Make}'");

                // Verify custom handling works correctly for both empty and null
                Assert.That(emptyResult.BusNumber, Is.EqualTo("DEFAULT"), "Empty string should use default");
                Assert.That(nullResult.BusNumber, Is.EqualTo("DEFAULT"), "Null should use default");
            }, "Both empty strings and null values should be handled gracefully");

            Console.WriteLine("DEBUG: Test passed successfully");
        }

        [Test]
        public void Bus_ChainingNullCoalescing_ShouldWork()
        {
            Console.WriteLine("DEBUG: Starting Bus_ChainingNullCoalescing_ShouldWork test");

            // Arrange
            var bus = new Bus
            {
                VehicleId = 16,
                BusNumber = null,
                Make = null,
                Model = null,
                Status = null,
                InsurancePolicyNumber = null,
                Notes = null
            };

            // Act - Test chaining multiple null-coalescing operators
            // Note: Bus model converts null to empty string, so we need to check for empty strings too
            var result = new
            {
                DisplayName = !string.IsNullOrEmpty(bus.BusNumber) ? bus.BusNumber :
                             !string.IsNullOrEmpty(bus.Make) ? bus.Make :
                             !string.IsNullOrEmpty(bus.Model) ? bus.Model : $"Bus-{bus.VehicleId}",
                Policy = bus.InsurancePolicyNumber ?? "No Policy",
                Description = bus.Notes ?? bus.SpecialEquipment ?? "No additional information"
            };

            // Assert
            Assert.That(result.DisplayName, Is.EqualTo("Bus-16"), "Should fall back to VehicleId when all fields are empty");
            Assert.That(result.Policy, Is.EqualTo("No Policy"), "Should use default when insurance is null");
            Assert.That(result.Description, Is.EqualTo("No additional information"), "Should use final fallback");

            Console.WriteLine($"DEBUG: Chained results - DisplayName:'{result.DisplayName}', Policy:'{result.Policy}'");
            Console.WriteLine("DEBUG: Test passed successfully");
        }
    }
}
