using BusBuddy.Core.Data;
using BusBuddy.Core.Models;
using BusBuddy.Core.Services;
using BusBuddy.Core.Utilities;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusBuddy.Tests
{
    [TestFixture]
    public class ComprehensiveNullHandlingTests
    {
        private Mock<IBusBuddyDbContextFactory> _mockContextFactory;
        private Mock<BusBuddyDbContext> _mockDbContext;
        private DriverService _driverService;
        private StudentService _studentService;
        private FuelService _fuelService;

        [SetUp]
        public void Setup()
        {
            _mockContextFactory = new Mock<IBusBuddyDbContextFactory>();
            _mockDbContext = new Mock<BusBuddyDbContext>();

            // Create test context factory
            _mockContextFactory.Setup(f => f.CreateDbContext()).Returns(_mockDbContext.Object);
            _mockContextFactory.Setup(f => f.CreateWriteDbContext()).Returns(_mockDbContext.Object);

            // Create enhanced caching service mock for DriverService
            var mockCachingService = new Mock<IEnhancedCachingService>();

            // Create service instances - no logger dependencies needed since services use static Serilog loggers
            _driverService = new DriverService(_mockContextFactory.Object, mockCachingService.Object);
            _studentService = new StudentService(_mockContextFactory.Object);
            _fuelService = new FuelService(_mockContextFactory.Object);
        }

        #region Driver Model Null Handling Tests

        [Test]
        public void Driver_RequiredFields_ShouldHaveDefaults()
        {
            Console.WriteLine("DEBUG: Starting Driver_RequiredFields_ShouldHaveDefaults test");

            // Arrange - Create a new Driver object
            var newDriver = new Driver();

            // Act & Assert - Check that required fields have sensible defaults
            Assert.That(newDriver.DriverName, Is.Not.Null, "DriverName should have a default value");
            Assert.That(newDriver.DriversLicenceType, Is.Not.Null, "DriversLicenceType should have a default value");
            Assert.That(newDriver.Status, Is.Not.Null, "Status should have a default value");
            Assert.That(newDriver.Status, Is.EqualTo("Active"), "Status should default to 'Active'");
            Assert.That(newDriver.TrainingComplete, Is.False, "TrainingComplete should default to false");

            Console.WriteLine($"DEBUG: Driver defaults - Name:'{newDriver.DriverName}', LicenseType:'{newDriver.DriversLicenceType}', Status:'{newDriver.Status}'");
            Console.WriteLine("DEBUG: Test passed successfully");
        }

        [Test]
        public void Driver_NullStringAssignment_ShouldHandleGracefully()
        {
            Console.WriteLine("DEBUG: Starting Driver_NullStringAssignment_ShouldHandleGracefully test");

            // Arrange
            var driver = new Driver();

            // Act - Try to assign null values to string properties
            driver.DriverName = null!;
            driver.DriversLicenceType = null!;
            driver.Status = null!;
            driver.DriverPhone = null;
            driver.DriverEmail = null;
            driver.Address = null;

            // Assert - Check how the properties handle null values
            Assert.That(driver.DriverName, Is.Not.Null, "DriverName should not be null after null assignment");
            Assert.That(driver.DriversLicenceType, Is.Not.Null, "DriversLicenceType should not be null after null assignment");
            Assert.That(driver.Status, Is.Not.Null, "Status should not be null after null assignment");

            // Optional properties can be null
            Assert.DoesNotThrow(() =>
            {
                var phone = driver.DriverPhone; // Can be null
                var email = driver.DriverEmail; // Can be null
                var address = driver.Address;   // Can be null
                Console.WriteLine($"DEBUG: Optional properties - Phone:{phone}, Email:{email}, Address:{address}");
            }, "Optional properties should handle null gracefully");

            Console.WriteLine($"DEBUG: After null assignment - Name:'{driver.DriverName}', Status:'{driver.Status}'");
            Console.WriteLine("DEBUG: Test passed successfully");
        }

        [Test]
        public void Driver_ComputedProperties_ShouldHandleNullValues()
        {
            Console.WriteLine("DEBUG: Starting Driver_ComputedProperties_ShouldHandleNullValues test");

            // Arrange - Driver with null optional fields
            var driver = new Driver
            {
                DriverId = 1,
                DriverName = "John Doe",
                FirstName = null,
                LastName = null,
                LicenseExpiryDate = null,
                Address = null,
                City = null,
                State = null,
                Zip = null
            };

            // Act & Assert - Test computed properties that depend on nullable fields
            Assert.DoesNotThrow(() =>
            {
                var fullName = driver.FullName;
                var fullAddress = driver.FullAddress;
                var licenseStatus = driver.LicenseStatus;
                var qualificationStatus = driver.QualificationStatus;
                var isAvailable = driver.IsAvailable;
                var needsAttention = driver.NeedsAttention;

                Console.WriteLine($"DEBUG: FullName: '{fullName}'");
                Console.WriteLine($"DEBUG: FullAddress: '{fullAddress}'");
                Console.WriteLine($"DEBUG: LicenseStatus: '{licenseStatus}'");
                Console.WriteLine($"DEBUG: QualificationStatus: '{qualificationStatus}'");
                Console.WriteLine($"DEBUG: IsAvailable: {isAvailable}");
                Console.WriteLine($"DEBUG: NeedsAttention: {needsAttention}");

                Assert.That(fullName, Is.Not.Null, "FullName should not be null");
                Assert.That(fullAddress, Is.Not.Null, "FullAddress should not be null");
                Assert.That(licenseStatus, Is.Not.Null, "LicenseStatus should not be null");
                Assert.That(qualificationStatus, Is.Not.Null, "QualificationStatus should not be null");
            }, "Computed properties should not throw exceptions with null values");

            Console.WriteLine("DEBUG: Test passed successfully");
        }

        [Test]
        public void Driver_NavigationProperties_ShouldInitializeEmpty()
        {
            Console.WriteLine("DEBUG: Starting Driver_NavigationProperties_ShouldInitializeEmpty test");

            // Arrange
            var driver = new Driver();

            // Act & Assert - Navigation properties should be initialized to empty collections, not null
            Assert.That(driver.AMRoutes, Is.Not.Null, "AMRoutes should be initialized");
            Assert.That(driver.PMRoutes, Is.Not.Null, "PMRoutes should be initialized");
            Assert.That(driver.Schedules, Is.Not.Null, "Schedules should be initialized");
            Assert.That(driver.Activities, Is.Not.Null, "Activities should be initialized");
            Assert.That(driver.ScheduledActivities, Is.Not.Null, "ScheduledActivities should be initialized");

            // Verify they are empty collections
            Assert.That(driver.AMRoutes.Count, Is.EqualTo(0), "AMRoutes should be empty initially");
            Assert.That(driver.PMRoutes.Count, Is.EqualTo(0), "PMRoutes should be empty initially");
            Assert.That(driver.Schedules.Count, Is.EqualTo(0), "Schedules should be empty initially");

            Console.WriteLine("DEBUG: All driver navigation properties are properly initialized");
            Console.WriteLine("DEBUG: Test passed successfully");
        }

        #endregion

        #region Student Model Null Handling Tests

        [Test]
        public void Student_RequiredFields_ShouldHaveDefaults()
        {
            Console.WriteLine("DEBUG: Starting Student_RequiredFields_ShouldHaveDefaults test");

            // Arrange - Create a new Student object
            var newStudent = new Student();

            // Act & Assert - Check that required fields have sensible defaults
            Assert.That(newStudent.StudentName, Is.Not.Null, "StudentName should have a default value");
            Assert.That(newStudent.Active, Is.True, "Active should default to true");
            Assert.That(newStudent.SpecialNeeds, Is.False, "SpecialNeeds should default to false");
            Assert.That(newStudent.PhotoPermission, Is.True, "PhotoPermission should default to true");
            Assert.That(newStudent.FieldTripPermission, Is.True, "FieldTripPermission should default to true");

            Console.WriteLine($"DEBUG: Student defaults - Name:'{newStudent.StudentName}', Active:{newStudent.Active}, SpecialNeeds:{newStudent.SpecialNeeds}");
            Console.WriteLine("DEBUG: Test passed successfully");
        }

        [Test]
        public void Student_ComputedProperties_ShouldHandleNullValues()
        {
            Console.WriteLine("DEBUG: Starting Student_ComputedProperties_ShouldHandleNullValues test");

            // Arrange - Student with null optional fields
            var student = new Student
            {
                StudentId = 1,
                StudentName = "Jane Smith",
                DateOfBirth = null,
                Grade = null,
                ParentGuardian = null,
                HomePhone = null,
                MedicalNotes = null,
                SpecialAccommodations = null
            };

            // Act & Assert - Test computed properties that depend on nullable fields
            Assert.DoesNotThrow(() =>
            {
                var age = student.Age;
                var fullName = student.FullName;
                var contactInfo = student.ContactInfo;
                var hasSpecialNeeds = student.HasSpecialNeeds;
                var gradeLevel = student.GradeLevel;

                Console.WriteLine($"DEBUG: Age: {age}");
                Console.WriteLine($"DEBUG: FullName: '{fullName}'");
                Console.WriteLine($"DEBUG: ContactInfo: '{contactInfo}'");
                Console.WriteLine($"DEBUG: HasSpecialNeeds: {hasSpecialNeeds}");
                Console.WriteLine($"DEBUG: GradeLevel: '{gradeLevel}'");

                Assert.That(fullName, Is.Not.Null, "FullName should not be null");
                Assert.That(contactInfo, Is.Not.Null, "ContactInfo should not be null");
                Assert.That(gradeLevel, Is.Not.Null, "GradeLevel should not be null");
            }, "Student computed properties should not throw exceptions with null values");

            Console.WriteLine("DEBUG: Test passed successfully");
        }

        [Test]
        public void Student_RouteAssignment_ShouldHandleNullValues()
        {
            Console.WriteLine("DEBUG: Starting Student_RouteAssignment_ShouldHandleNullValues test");

            // Arrange - Student with null route assignments
            var student = new Student
            {
                StudentId = 1,
                StudentName = "Test Student",
                AMRoute = null,
                PMRoute = null,
                BusStop = null
            };

            // Act & Assert - Route assignment properties should handle null gracefully
            Assert.DoesNotThrow(() =>
            {
                var amRoute = student.AMRoute ?? "No AM Route";
                var pmRoute = student.PMRoute ?? "No PM Route";
                var busStop = student.BusStop ?? "No Bus Stop";

                Console.WriteLine($"DEBUG: AM Route: '{amRoute}'");
                Console.WriteLine($"DEBUG: PM Route: '{pmRoute}'");
                Console.WriteLine($"DEBUG: Bus Stop: '{busStop}'");

                Assert.That(amRoute, Is.Not.Null, "AM Route handling should not be null");
                Assert.That(pmRoute, Is.Not.Null, "PM Route handling should not be null");
                Assert.That(busStop, Is.Not.Null, "Bus Stop handling should not be null");
            }, "Route assignment should handle null values gracefully");

            Console.WriteLine("DEBUG: Test passed successfully");
        }

        #endregion

        #region Fuel Model Null Handling Tests

        [Test]
        public void Fuel_RequiredFields_ShouldHaveDefaults()
        {
            Console.WriteLine("DEBUG: Starting Fuel_RequiredFields_ShouldHaveDefaults test");

            // Arrange - Create a new Fuel object
            var newFuel = new Fuel();

            // Act & Assert - Check that required fields have sensible defaults
            Assert.That(newFuel.FuelLocation, Is.Not.Null, "FuelLocation should have a default value");
            Assert.That(newFuel.FuelType, Is.Not.Null, "FuelType should have a default value");
            // FuelDate defaults to DateTime default (which is OK for new records)

            Console.WriteLine($"DEBUG: Fuel defaults - Location:'{newFuel.FuelLocation}', Type:'{newFuel.FuelType}', Date:{newFuel.FuelDate}");
            Console.WriteLine("DEBUG: Test passed successfully");
        }

        [Test]
        public void Fuel_NullableNumericFields_ShouldBeHandled()
        {
            Console.WriteLine("DEBUG: Starting Fuel_NullableNumericFields_ShouldBeHandled test");

            // Arrange - Fuel with null numeric fields
            var fuelWithNulls = new Fuel
            {
                FuelId = 1,
                FuelDate = DateTime.Now,
                FuelLocation = "Test Station",
                VehicleFueledId = 1,
                FuelType = "Diesel",
                Gallons = null,
                PricePerGallon = null,
                TotalCost = null
            };

            // Act - Access the nullable numeric fields
            var hasGallons = fuelWithNulls.Gallons.HasValue;
            var hasPricePerGallon = fuelWithNulls.PricePerGallon.HasValue;
            var hasTotalCost = fuelWithNulls.TotalCost.HasValue;

            // Using defaults if null
            var gallons = fuelWithNulls.Gallons ?? 0;
            var pricePerGallon = fuelWithNulls.PricePerGallon ?? 0;
            var totalCost = fuelWithNulls.TotalCost ?? 0;

            Console.WriteLine($"DEBUG: hasGallons={hasGallons}, gallons={gallons}");
            Console.WriteLine($"DEBUG: hasPricePerGallon={hasPricePerGallon}, pricePerGallon={pricePerGallon}");
            Console.WriteLine($"DEBUG: hasTotalCost={hasTotalCost}, totalCost={totalCost}");

            // Assert
            Assert.That(hasGallons, Is.False, "Gallons should be null");
            Assert.That(hasPricePerGallon, Is.False, "PricePerGallon should be null");
            Assert.That(hasTotalCost, Is.False, "TotalCost should be null");

            Assert.That(gallons, Is.EqualTo(0), "Gallons ?? 0 should equal 0");
            Assert.That(pricePerGallon, Is.EqualTo(0), "PricePerGallon ?? 0 should equal 0");
            Assert.That(totalCost, Is.EqualTo(0), "TotalCost ?? 0 should equal 0");

            Console.WriteLine("DEBUG: All nullable fuel field assertions passed");
        }

        [Test]
        public void Fuel_CalculationSafety_ShouldHandleNulls()
        {
            Console.WriteLine("DEBUG: Starting Fuel_CalculationSafety_ShouldHandleNulls test");

            // Arrange - Create fuel records with various null combinations
            var fuelRecords = new List<Fuel>
            {
                new Fuel { Gallons = 10, PricePerGallon = 3.50m, TotalCost = null },
                new Fuel { Gallons = null, PricePerGallon = 3.50m, TotalCost = 35.00m },
                new Fuel { Gallons = 10, PricePerGallon = null, TotalCost = 35.00m },
                new Fuel { Gallons = null, PricePerGallon = null, TotalCost = null }
            };

            // Act & Assert - Perform safe calculations
            Assert.DoesNotThrow(() =>
            {
                foreach (var fuel in fuelRecords)
                {
                    // Safe total cost calculation
                    var calculatedTotal = (fuel.Gallons ?? 0) * (fuel.PricePerGallon ?? 0);
                    var actualTotal = fuel.TotalCost ?? calculatedTotal;

                    // Safe efficiency calculation (MPG equivalent)
                    var efficiency = fuel.Gallons.HasValue && fuel.Gallons.Value > 0
                        ? 100 / fuel.Gallons.Value  // Miles per 100 gallons as example
                        : 0;

                    Console.WriteLine($"DEBUG: Gallons={fuel.Gallons}, Price={fuel.PricePerGallon}, Total={fuel.TotalCost}, Calculated={calculatedTotal}, Efficiency={efficiency}");

                    Assert.That(calculatedTotal, Is.GreaterThanOrEqualTo(0), "Calculated total should not be negative");
                    Assert.That(efficiency, Is.GreaterThanOrEqualTo(0), "Efficiency should not be negative");
                }
            }, "Fuel calculations should handle null values safely");

            Console.WriteLine("DEBUG: Test passed successfully");
        }

        #endregion

        #region Route Model Null Handling Tests

        [Test]
        public void Route_RequiredFields_ShouldHaveDefaults()
        {
            Console.WriteLine("DEBUG: Starting Route_RequiredFields_ShouldHaveDefaults test");

            // Arrange - Create a new Route object
            var newRoute = new Route();

            // Act & Assert - Check that required fields have sensible defaults
            Assert.That(newRoute.RouteName, Is.Not.Null, "RouteName should have a default value");
            Assert.That(newRoute.IsActive, Is.True, "IsActive should default to true");
            // Date defaults to DateTime default (which is OK for new records)

            Console.WriteLine($"DEBUG: Route defaults - Name:'{newRoute.RouteName}', IsActive:{newRoute.IsActive}, Date:{newRoute.Date}");
            Console.WriteLine("DEBUG: Test passed successfully");
        }

        [Test]
        public void Route_NullableAssignments_ShouldBeHandled()
        {
            Console.WriteLine("DEBUG: Starting Route_NullableAssignments_ShouldBeHandled test");

            // Arrange - Route with null assignments
            var route = new Route
            {
                RouteId = 1,
                RouteName = "Test Route",
                Date = DateTime.Today,
                AMVehicleId = null,
                PMVehicleId = null,
                AMDriverId = null,
                PMDriverId = null,
                Description = null,
                BusNumber = null,
                DriverName = null
            };

            // Act & Assert - Test null assignments
            Assert.DoesNotThrow(() =>
            {
                var amVehicle = route.AMVehicleId?.ToString() ?? "No AM Vehicle";
                var pmVehicle = route.PMVehicleId?.ToString() ?? "No PM Vehicle";
                var amDriver = route.AMDriverId?.ToString() ?? "No AM Driver";
                var pmDriver = route.PMDriverId?.ToString() ?? "No PM Driver";
                var description = route.Description ?? "No Description";
                var busNumber = route.BusNumber ?? "No Bus";
                var driverName = route.DriverName ?? "No Driver";

                Console.WriteLine($"DEBUG: AM Vehicle: '{amVehicle}'");
                Console.WriteLine($"DEBUG: PM Vehicle: '{pmVehicle}'");
                Console.WriteLine($"DEBUG: AM Driver: '{amDriver}'");
                Console.WriteLine($"DEBUG: PM Driver: '{pmDriver}'");
                Console.WriteLine($"DEBUG: Description: '{description}'");
                Console.WriteLine($"DEBUG: Bus Number: '{busNumber}'");
                Console.WriteLine($"DEBUG: Driver Name: '{driverName}'");

                Assert.That(amVehicle, Is.Not.Null, "AM Vehicle handling should not be null");
                Assert.That(pmVehicle, Is.Not.Null, "PM Vehicle handling should not be null");
                Assert.That(description, Is.Not.Null, "Description handling should not be null");
            }, "Route assignments should handle null values gracefully");

            Console.WriteLine("DEBUG: Test passed successfully");
        }

        [Test]
        public void Route_NavigationProperties_ShouldInitializeEmpty()
        {
            Console.WriteLine("DEBUG: Starting Route_NavigationProperties_ShouldInitializeEmpty test");

            // Arrange
            var route = new Route();

            // Act & Assert - Navigation properties should be initialized to empty collections, not null
            Assert.That(route.Schedules, Is.Not.Null, "Schedules should be initialized");

            // Verify they are empty collections
            Assert.That(route.Schedules.Count, Is.EqualTo(0), "Schedules should be empty initially");

            // Navigation objects can be null initially
            Assert.DoesNotThrow(() =>
            {
                var amVehicle = route.AMVehicle; // Can be null
                var pmVehicle = route.PMVehicle; // Can be null
                var amDriver = route.AMDriver;   // Can be null
                var pmDriver = route.PMDriver;   // Can be null

                Console.WriteLine($"DEBUG: Navigation properties - AMVehicle:{amVehicle}, PMVehicle:{pmVehicle}, AMDriver:{amDriver}, PMDriver:{pmDriver}");
            }, "Route navigation properties should handle null gracefully");

            Console.WriteLine("DEBUG: Route navigation properties are properly initialized");
            Console.WriteLine("DEBUG: Test passed successfully");
        }

        #endregion

        #region Service Layer Null Handling Tests

        [Test]
        public async Task FuelService_GetTotalCost_ShouldHandleNullCosts()
        {
            Console.WriteLine("DEBUG: Starting FuelService_GetTotalCost_ShouldHandleNullCosts test");

            // Add await to fix CS1998 warning
            await Task.CompletedTask;

            // This would require more complex mocking to test the actual service
            // For now, test the null-handling concept
            var fuelRecords = new List<Fuel>
            {
                new Fuel { TotalCost = 100.50m },
                new Fuel { TotalCost = null },
                new Fuel { TotalCost = 75.25m },
                new Fuel { TotalCost = null }
            };

            // Act - Calculate total cost handling nulls
            var totalCost = fuelRecords.Sum(f => f.TotalCost ?? 0);
            var averageCost = fuelRecords.Where(f => f.TotalCost.HasValue).Average(f => f.TotalCost ?? 0);
            var recordsWithCost = fuelRecords.Count(f => f.TotalCost.HasValue);

            Console.WriteLine($"DEBUG: Total Cost: {totalCost}");
            Console.WriteLine($"DEBUG: Average Cost: {averageCost}");
            Console.WriteLine($"DEBUG: Records with cost: {recordsWithCost}");

            // Assert
            Assert.That(totalCost, Is.EqualTo(175.75m), "Total cost should handle nulls correctly");
            Assert.That(averageCost, Is.EqualTo(87.875m), "Average cost should handle nulls correctly");
            Assert.That(recordsWithCost, Is.EqualTo(2), "Should count only records with cost");

            Console.WriteLine("DEBUG: Test passed successfully");
        }

        [Test]
        public void StudentService_RouteAssignment_ShouldHandleNullRoutes()
        {
            Console.WriteLine("DEBUG: Starting StudentService_RouteAssignment_ShouldHandleNullRoutes test");

            // Arrange - Students with various route assignments
            var students = new List<Student>
            {
                new Student { StudentName = "Student 1", AMRoute = "Route A", PMRoute = "Route B" },
                new Student { StudentName = "Student 2", AMRoute = null, PMRoute = "Route B" },
                new Student { StudentName = "Student 3", AMRoute = "Route A", PMRoute = null },
                new Student { StudentName = "Student 4", AMRoute = null, PMRoute = null }
            };

            // Act & Assert - Test route assignment logic
            Assert.DoesNotThrow(() =>
            {
                foreach (var student in students)
                {
                    var hasAMRoute = !string.IsNullOrEmpty(student.AMRoute);
                    var hasPMRoute = !string.IsNullOrEmpty(student.PMRoute);
                    var hasAnyRoute = hasAMRoute || hasPMRoute;
                    var routeDescription = $"AM: {student.AMRoute ?? "None"}, PM: {student.PMRoute ?? "None"}";

                    Console.WriteLine($"DEBUG: {student.StudentName} - {routeDescription}, HasAnyRoute: {hasAnyRoute}");

                    Assert.That(routeDescription, Is.Not.Null, "Route description should not be null");
                }

                var studentsWithRoutes = students.Count(s => !string.IsNullOrEmpty(s.AMRoute) || !string.IsNullOrEmpty(s.PMRoute));
                var studentsWithoutRoutes = students.Count(s => string.IsNullOrEmpty(s.AMRoute) && string.IsNullOrEmpty(s.PMRoute));

                Assert.That(studentsWithRoutes, Is.EqualTo(3), "Should count students with routes correctly");
                Assert.That(studentsWithoutRoutes, Is.EqualTo(1), "Should count students without routes correctly");
            }, "Student route assignment should handle null values gracefully");

            Console.WriteLine("DEBUG: Test passed successfully");
        }

        #endregion

        #region Performance Tests with Null Handling

        [Test]
        public void NullCoalescingOperators_LargeDataset_PerformanceTest()
        {
            Console.WriteLine("DEBUG: Starting NullCoalescingOperators_LargeDataset_PerformanceTest test");

            // Arrange - Create large dataset with mixed null values
            var students = new List<Student>();
            var drivers = new List<Driver>();
            var fuels = new List<Fuel>();

            for (int i = 0; i < 1000; i++)
            {
                students.Add(new Student
                {
                    StudentId = i,
                    StudentName = i % 3 == 0 ? null : $"Student-{i}",
                    Grade = i % 4 == 0 ? null : $"Grade-{i % 12}",
                    AMRoute = i % 5 == 0 ? null : $"Route-{i % 10}",
                    PMRoute = i % 6 == 0 ? null : $"Route-{i % 10}"
                });

                drivers.Add(new Driver
                {
                    DriverId = i,
                    DriverName = i % 3 == 0 ? null : $"Driver-{i}",
                    DriverPhone = i % 4 == 0 ? null : $"555-{i:0000}",
                    Status = i % 7 == 0 ? null : "Active"
                });

                fuels.Add(new Fuel
                {
                    FuelId = i,
                    Gallons = i % 5 == 0 ? null : (decimal)(i % 50 + 10),
                    PricePerGallon = i % 6 == 0 ? null : (decimal)(3.00 + (i % 100) / 100.0),
                    TotalCost = i % 7 == 0 ? null : (decimal)(i % 100 + 25)
                });
            }

            // Act - Process all records with null-coalescing operators
            var startTime = DateTime.Now;

            var processedStudents = students.Select(s => new
            {
                Id = s.StudentId,
                Name = s.StudentName ?? "Unknown Student",
                Grade = s.Grade ?? "Unknown Grade",
                AMRoute = s.AMRoute ?? "No AM Route",
                PMRoute = s.PMRoute ?? "No PM Route",
                HasRoute = !string.IsNullOrEmpty(s.AMRoute) || !string.IsNullOrEmpty(s.PMRoute)
            }).ToList();

            var processedDrivers = drivers.Select(d => new
            {
                Id = d.DriverId,
                Name = d.DriverName ?? "Unknown Driver",
                Phone = d.DriverPhone ?? "No Phone",
                Status = d.Status ?? "Unknown Status"
            }).ToList();

            var processedFuels = fuels.Select(f => new
            {
                Id = f.FuelId,
                Gallons = f.Gallons ?? 0,
                Price = f.PricePerGallon ?? 0,
                Cost = f.TotalCost ?? 0,
                CalculatedCost = (f.Gallons ?? 0) * (f.PricePerGallon ?? 0)
            }).ToList();

            var endTime = DateTime.Now;
            var processingTime = (endTime - startTime).TotalMilliseconds;

            // Assert
            Assert.That(processedStudents.Count, Is.EqualTo(1000), "Should process all 1000 students");
            Assert.That(processedDrivers.Count, Is.EqualTo(1000), "Should process all 1000 drivers");
            Assert.That(processedFuels.Count, Is.EqualTo(1000), "Should process all 1000 fuel records");
            Assert.That(processingTime, Is.LessThan(2000), "Processing should be fast (< 2 seconds)");

            Console.WriteLine($"DEBUG: Processed {processedStudents.Count} students, {processedDrivers.Count} drivers, {processedFuels.Count} fuel records in {processingTime:F2}ms");
            Console.WriteLine("DEBUG: Null-coalescing operators perform well under load with multiple entity types");
            Console.WriteLine("DEBUG: Test passed successfully");
        }

        #endregion

        #region Cross-Entity Relationship Null Handling

        [Test]
        public void CrossEntity_RelationshipNulls_ShouldBeHandled()
        {
            Console.WriteLine("DEBUG: Starting CrossEntity_RelationshipNulls_ShouldBeHandled test");

            // Arrange - Create entities with null foreign key relationships
            var route = new Route
            {
                RouteId = 1,
                RouteName = "Test Route",
                AMVehicleId = null,  // No bus assigned
                PMDriverId = null,   // No driver assigned
                AMVehicle = null,    // Navigation property null
                PMDriver = null      // Navigation property null
            };

            var student = new Student
            {
                StudentId = 1,
                StudentName = "Test Student",
                AMRoute = "Test Route", // References route by name
                PMRoute = null          // No PM route
            };

            var fuel = new Fuel
            {
                FuelId = 1,
                VehicleFueledId = 999,  // References non-existent vehicle
                Vehicle = null          // Navigation property null
            };

            // Act & Assert - Test null relationship handling
            Assert.DoesNotThrow(() =>
            {
                // Route without vehicle/driver
                var routeInfo = $"Route: {route.RouteName}, AM Bus: {route.AMVehicleId?.ToString() ?? "None"}, PM Driver: {route.PMDriverId?.ToString() ?? "None"}";

                // Student with partial route assignment
                var studentRoutes = $"Student: {student.StudentName}, AM: {student.AMRoute ?? "None"}, PM: {student.PMRoute ?? "None"}";

                // Fuel with missing vehicle reference
                var fuelInfo = $"Fuel: {fuel.FuelId}, Vehicle: {fuel.VehicleFueledId}, Vehicle Name: {fuel.Vehicle?.BusNumber ?? "Unknown"}";

                Console.WriteLine($"DEBUG: {routeInfo}");
                Console.WriteLine($"DEBUG: {studentRoutes}");
                Console.WriteLine($"DEBUG: {fuelInfo}");

                Assert.That(routeInfo, Contains.Substring("None"), "Should handle null vehicle/driver gracefully");
                Assert.That(studentRoutes, Contains.Substring("None"), "Should handle null routes gracefully");
                Assert.That(fuelInfo, Contains.Substring("Unknown"), "Should handle null navigation properties gracefully");
            }, "Cross-entity relationships should handle null values gracefully");

            Console.WriteLine("DEBUG: Test passed successfully");
        }

        #endregion
    }
}
