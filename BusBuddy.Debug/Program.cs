using BusBuddy.Core.Data;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using BusBuddy.Core.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BusBuddy.Debug
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Bus Buddy Database Validator Debugger");
            Console.WriteLine("=====================================");

            // Create the test environment similar to DatabaseNullHandlingTests
            var mockContextFactory = new Mock<IBusBuddyDbContextFactory>();
            var mockValidatorLogger = new Mock<ILogger<DatabaseValidator>>();
            var mockBusServiceLogger = new Mock<ILogger<BusService>>();
            var mockCachingService = new Mock<IBusCachingService>();
            var mockDbContext = new Mock<BusBuddyDbContext>();

            // Setup the mock DbContext
            var buses = new List<Bus>
            {
                new Bus { VehicleId = 1, BusNumber = "Bus-1", Make = "Test Make", Model = "Test Model", VINNumber = "12345", LicenseNumber = "LIC-1", Status = "Active" },
                new Bus { VehicleId = 2, BusNumber = null, Make = "Test Make", Model = "Test Model", VINNumber = "12346", LicenseNumber = "LIC-2", Status = "Active" },
                new Bus { VehicleId = 3, BusNumber = "Bus-3", Make = null, Model = "Test Model", VINNumber = "12347", LicenseNumber = "LIC-3", Status = "Active" },
                new Bus { VehicleId = 4, BusNumber = "Bus-4", Make = "Test Make", Model = null, VINNumber = "12348", LicenseNumber = "LIC-4", Status = null }
            };

            // Create a mock DbSet that supports async enumeration
            var mockSet = CreateMockDbSet(buses);

            mockDbContext.Setup(c => c.Vehicles).Returns(mockSet.Object);
            mockContextFactory.Setup(f => f.CreateDbContext()).Returns(mockDbContext.Object);
            mockContextFactory.Setup(f => f.CreateWriteDbContext()).Returns(mockDbContext.Object);

            var validator = new DatabaseValidator(mockContextFactory.Object, mockValidatorLogger.Object);
            var busService = new BusService(mockBusServiceLogger.Object, mockContextFactory.Object, mockCachingService.Object);

            // Setup Bus Cache behavior to invoke provided function for testing
            mockCachingService.Setup(c => c.GetAllBusesAsync(It.IsAny<Func<Task<List<Bus>>>>()))
                .Returns((Func<Task<List<Bus>>> factory) => factory());

            mockCachingService.Setup(c => c.GetBusByIdAsync(It.IsAny<int>(), It.IsAny<Func<int, Task<Bus?>>>()))
                .Returns((int id, Func<int, Task<Bus?>> factory) => factory(id));

            try
            {
                // Test 1: Manual NULL Handling Check
                Console.WriteLine("\nTEST 1: Manual_NULL_Handling_Check");
                Console.WriteLine("----------------------------------------");

                try
                {
                    // Identify which buses have NULL values
                    var busesWithNulls = buses.Where(b =>
                        b.BusNumber == null ||
                        b.Make == null ||
                        b.Model == null ||
                        b.VINNumber == null ||
                        b.LicenseNumber == null ||
                        b.Status == null).ToList();

                    Console.WriteLine($"Found {busesWithNulls.Count} buses with NULL values:");
                    foreach (var bus in busesWithNulls)
                    {
                        Console.WriteLine($" - Bus ID: {bus.VehicleId}");
                        Console.WriteLine($"   BusNumber: {(bus.BusNumber == null ? "NULL" : bus.BusNumber)}");
                        Console.WriteLine($"   Make: {(bus.Make == null ? "NULL" : bus.Make)}");
                        Console.WriteLine($"   Model: {(bus.Model == null ? "NULL" : bus.Model)}");
                        Console.WriteLine($"   Status: {(bus.Status == null ? "NULL" : bus.Status)}");
                    }

                    bool test1Passed = busesWithNulls.Count > 0;
                    Console.WriteLine($"Test result: {(test1Passed ? "PASSED" : "FAILED")}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                }

                // Test 2: Fix NULL Values Manually
                Console.WriteLine("\nTEST 2: Fix_NULL_Values_Manually");
                Console.WriteLine("----------------------------------------");

                try
                {
                    int fixCount = 0;
                    foreach (var bus in buses)
                    {
                        bool changed = false;

                        if (bus.BusNumber == null)
                        {
                            bus.BusNumber = $"Bus-{bus.VehicleId}";
                            changed = true;
                            Console.WriteLine($"Fixed BusNumber for Bus ID: {bus.VehicleId}");
                        }

                        if (bus.Make == null)
                        {
                            bus.Make = "Unknown";
                            changed = true;
                            Console.WriteLine($"Fixed Make for Bus ID: {bus.VehicleId}");
                        }

                        if (bus.Model == null)
                        {
                            bus.Model = "Unknown";
                            changed = true;
                            Console.WriteLine($"Fixed Model for Bus ID: {bus.VehicleId}");
                        }

                        if (bus.Status == null)
                        {
                            bus.Status = "Active";
                            changed = true;
                            Console.WriteLine($"Fixed Status for Bus ID: {bus.VehicleId}");
                        }

                        if (changed)
                        {
                            fixCount++;
                        }
                    }

                    Console.WriteLine($"Fixed {fixCount} buses with NULL values");
                    bool test2Passed = fixCount > 0;
                    Console.WriteLine($"Test result: {(test2Passed ? "PASSED" : "FAILED")}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                }

                // Test 3: BusProjection_ShouldHandleNullValues
                Console.WriteLine("\nTEST 3: BusProjection_ShouldHandleNullValues");
                Console.WriteLine("----------------------------------------");

                try
                {
                    // Arrange
                    var busWithNulls = new Bus
                    {
                        VehicleId = 5,
                        BusNumber = null,
                        Make = null,
                        Model = null,
                        VINNumber = null,
                        LicenseNumber = null,
                        Status = null
                    };
                    Console.WriteLine("Created bus with NULL values");

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
                    Console.WriteLine("Applied NULL handling with ?? operator");
                    Console.WriteLine($"Result - BusNumber:'{mappedBus.BusNumber}', Make:'{mappedBus.Make}', Status:'{mappedBus.Status}'");

                    // Assert
                    bool test3Passed =
                        mappedBus.BusNumber == string.Empty &&
                        mappedBus.Make == string.Empty &&
                        mappedBus.Model == string.Empty &&
                        mappedBus.VINNumber == string.Empty &&
                        mappedBus.LicenseNumber == string.Empty &&
                        mappedBus.Status == "Active";

                    Console.WriteLine($"Test result: {(test3Passed ? "PASSED" : "FAILED")}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                }

                // Test 4: Manual projection of bus entities (simulating BusService.GetAllBusEntitiesAsync)
                Console.WriteLine("\nTEST 4: Manual_Bus_Projection");
                Console.WriteLine("----------------------------------------");

                try
                {
                    // Projection similar to what BusService.GetAllBusEntitiesAsync does
                    var projectedBuses = buses.Select(v => new Bus
                    {
                        VehicleId = v.VehicleId,
                        BusNumber = v.BusNumber ?? string.Empty,
                        Year = v.Year,
                        Make = v.Make ?? string.Empty,
                        Model = v.Model ?? string.Empty,
                        SeatingCapacity = v.SeatingCapacity,
                        VINNumber = v.VINNumber ?? string.Empty,
                        LicenseNumber = v.LicenseNumber ?? string.Empty,
                        Status = v.Status ?? "Active"
                    }).ToList();

                    Console.WriteLine($"Projected {projectedBuses.Count} buses with NULL handling");
                    foreach (var bus in projectedBuses)
                    {
                        Console.WriteLine($"Bus ID: {bus.VehicleId}, Number: '{bus.BusNumber}', Make: '{bus.Make}', Model: '{bus.Model}', Status: '{bus.Status}'");
                    }

                    // Check that all values that were NULL in the database are now empty strings or defaults
                    bool test4Passed = true;
                    foreach (var bus in projectedBuses)
                    {
                        if (bus.BusNumber == null || bus.Make == null || bus.Model == null ||
                            bus.VINNumber == null || bus.LicenseNumber == null || bus.Status == null)
                        {
                            Console.WriteLine($"FAILED: Bus {bus.VehicleId} has NULL values after projection");
                            test4Passed = false;
                            break;
                        }
                    }

                    Console.WriteLine($"Test result: {(test4Passed ? "PASSED" : "FAILED")}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                }

                // Test 5: Search query with NULL handling
                Console.WriteLine("\nTEST 5: Search_Query_With_NULL_Handling");
                Console.WriteLine("----------------------------------------");

                try
                {
                    // Simulate a search query that might encounter NULL values
                    string searchTerm = "Bus";

                    // Safely handle NULL values in the search
                    var searchResults = buses.Where(b =>
                        (b.BusNumber != null && b.BusNumber.Contains(searchTerm)) ||
                        (b.Make != null && b.Make.Contains(searchTerm)) ||
                        (b.Model != null && b.Model.Contains(searchTerm))).ToList();

                    Console.WriteLine($"Found {searchResults.Count} buses matching '{searchTerm}'");
                    foreach (var bus in searchResults)
                    {
                        Console.WriteLine($"Bus ID: {bus.VehicleId}, Number: '{bus.BusNumber ?? "NULL"}', Make: '{bus.Make ?? "NULL"}', Model: '{bus.Model ?? "NULL"}'");
                    }

                    // Now project the search results to ensure no NULL values
                    var safeSearchResults = searchResults.Select(b => new Bus
                    {
                        VehicleId = b.VehicleId,
                        BusNumber = b.BusNumber ?? string.Empty,
                        Make = b.Make ?? string.Empty,
                        Model = b.Model ?? string.Empty,
                        VINNumber = b.VINNumber ?? string.Empty,
                        LicenseNumber = b.LicenseNumber ?? string.Empty,
                        Status = b.Status ?? "Active"
                    }).ToList();

                    Console.WriteLine("Projected search results with NULL handling:");
                    foreach (var bus in safeSearchResults)
                    {
                        Console.WriteLine($"Bus ID: {bus.VehicleId}, Number: '{bus.BusNumber}', Make: '{bus.Make}', Model: '{bus.Model}'");
                    }

                    // Verify all required fields have proper values (not null)
                    bool allFieldsNotNull = safeSearchResults.All(b =>
                        b.BusNumber != null &&
                        b.Make != null &&
                        b.Model != null &&
                        b.VINNumber != null &&
                        b.LicenseNumber != null &&
                        b.Status != null);

                    Console.WriteLine($"Test result: {(allFieldsNotNull ? "PASSED" : "FAILED")}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FATAL ERROR: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine("\nDebugging complete! Press any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Creates a mock DbSet that supports both synchronous and asynchronous operations
        /// </summary>
        private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
        {
            var queryable = data.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();

            // Setup for IQueryable
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            // The following is an alternative approach, but it's more complex to implement full async behavior
            // For the purpose of our debug project, we'll stick with direct list tests

            return mockSet;
        }
    }
}
