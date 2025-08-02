using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusBuddy.Core.Data;
using BusBuddy.Core.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace BusBuddy.Tests.Core
{
    /// <summary>
    /// Simple, focused data layer tests for BusBuddy transportation models
    /// Critical for kids waiting on their route schedules - keep it simple and working
    /// </summary>
    public class DataLayerTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly BusBuddyDbContext _context;
        private readonly IServiceProvider _serviceProvider;
        private bool _disposed;

        public DataLayerTests(ITestOutputHelper output)
        {
            _output = output;

            // Simple in-memory database setup
            var services = new ServiceCollection();
            services.AddDbContext<BusBuddyDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

            services.AddLogging();

            _serviceProvider = services.BuildServiceProvider();
            _context = _serviceProvider.GetRequiredService<BusBuddyDbContext>();

            // Ensure database is created
            _context.Database.EnsureCreated();
        }

        #region Basic Vehicle Tests

        [Fact]
        public async Task Vehicle_CanCreate_BasicTest()
        {
            // Arrange
            var vehicle = new Vehicle
            {
                BusNumber = "BUS001",
                Make = "Blue Bird",
                Model = "Vision",
                Status = VehicleStatus.Active,
                Capacity = 72
            };

            // Act
            _context.Vehicles.Add(vehicle);
            var result = await _context.SaveChangesAsync();

            // Assert
            Assert.Equal(1, result);
            Assert.True(vehicle.VehicleId > 0);
            _output.WriteLine($"✅ Created vehicle: {vehicle.BusNumber}");
        }

        [Fact]
        public async Task Vehicle_StatusCheck_WorksCorrectly()
        {
            // Arrange
            var vehicle = new Bus
            {
                BusNumber = "TEST-001",
                Make = "Test Make",
                Model = "Test Model",
                Year = 2020,
                SeatingCapacity = 30,
                VINNumber = "TEST123456789",
                LicenseNumber = "TEST-LIC-001",
                Status = "Active"
            };

            // Act
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            // Assert
            var savedVehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.BusNumber == "TEST-001");
            Assert.NotNull(savedVehicle);
            Assert.Equal("Active", savedVehicle.Status);
        }

        #endregion

        #region Basic Driver Tests

        [Fact]
        public async Task Driver_CanCreate_BasicTest()
        {
            // Arrange
            var driver = new Driver
            {
                DriverName = "John Smith",
                DriverPhone = "555-0123",
                DriversLicenceType = "CDL-B",
                Status = DriverStatus.Active
            };

            // Act
            _context.Drivers.Add(driver);
            var result = await _context.SaveChangesAsync();

            // Assert
            Assert.Equal(1, result);
            Assert.True(driver.DriverId > 0);
            Assert.Equal("CDL-B", driver.DriversLicenceType);
            _output.WriteLine($"Created driver: {driver.DriverName}");
        }

        [Fact]
        public async Task Driver_Creation_WorksCorrectly()
        {
            // Arrange
            var driver = new Driver
            {
                DriverName = "Test Driver",
                DriversLicenceType = "CDL",
                Status = "Active",
                TrainingComplete = true
            };

            // Act
            _context.Drivers.Add(driver);
            await _context.SaveChangesAsync();

            // Assert
            var savedDriver = await _context.Drivers.FirstOrDefaultAsync(d => d.DriverName == "Test Driver");
            Assert.NotNull(savedDriver);
            Assert.True(savedDriver.TrainingComplete);
        }

        #endregion

        #region Basic Route Tests

        [Fact]
        public async Task Route_CanCreate_BasicTest()
        {
            // Arrange
            var route = new Route
            {
                RouteName = "Route 1 - Elementary",
                Description = "Morning pickup",
                StartTime = new TimeSpan(7, 30, 0),
                EndTime = new TimeSpan(8, 15, 0),
                Status = RouteStatus.Active,
                School = "Lincoln Elementary"
            };

            // Act
            _context.Routes.Add(route);
            var result = await _context.SaveChangesAsync();

            // Assert
            Assert.Equal(1, result);
            Assert.True(route.RouteId > 0);
            _output.WriteLine($"Created route: {route.RouteName}");
        }

        [Fact]
        public async Task Route_Creation_WorksCorrectly()
        {
            // Arrange
            var route = new Route
            {
                RouteName = "Test Route",
                Date = DateTime.Today,
                School = "Test School",
                IsActive = true
            };

            // Act
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            // Assert
            var savedRoute = await _context.Routes.FirstOrDefaultAsync(r => r.RouteName == "Test Route");
            Assert.NotNull(savedRoute);
            Assert.True(savedRoute.IsActive);
        }

        #endregion

        #region Basic Student Tests

        [Fact]
        public async Task Student_CanCreate_BasicTest()
        {
            // Arrange
            var student = new Student
            {
                StudentNumber = "STU001",
                StudentName = "Alice Johnson",
                Grade = "3rd",
                School = "Lincoln Elementary"
            };

            // Act
            _context.Students.Add(student);
            var result = await _context.SaveChangesAsync();

            // Assert
            Assert.Equal(1, result);
            Assert.True(student.StudentId > 0);
            _output.WriteLine($"Created student: {student.StudentName}");
        }

        [Fact]
        public async Task Student_Creation_WorksCorrectly()
        {
            // Arrange
            var student = new Student
            {
                StudentName = "Test Student",
                Grade = "5",
                School = "Test School",
                Active = true
            };

            // Act
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            // Assert
            var savedStudent = await _context.Students.FirstOrDefaultAsync(s => s.StudentName == "Test Student");
            Assert.NotNull(savedStudent);
            Assert.True(savedStudent.Active);
        }

        #endregion

        #region Integration Test - The One That Matters

        [Fact]
        public async Task CompleteRouteSetup_ForKidsWaitingOnSchedules()
        {
            // Arrange - Real scenario: get kids to school safely
            var vehicle = new Vehicle
            {
                BusNumber = "BUS101",
                Make = "Blue Bird",
                Status = VehicleStatus.Active,
                Capacity = 72
            };

            var driver = new Driver
            {
                DriverName = "Sarah Connor",
                DriversLicenceType = "CDL-B",
                Status = DriverStatus.Active
            };

            var route = new Route
            {
                RouteName = "Morning Elementary Route",
                StartTime = new TimeSpan(7, 30, 0),
                EndTime = new TimeSpan(8, 15, 0),
                Status = RouteStatus.Active,
                School = "Lincoln Elementary"
            };

            var students = new List<Student>
            {
                new Student { StudentNumber = "LIN001", StudentName = "Emma", Grade = "3rd", School = "Lincoln Elementary" },
                new Student { StudentNumber = "LIN002", StudentName = "Mason", Grade = "2nd", School = "Lincoln Elementary" },
                new Student { StudentNumber = "LIN003", StudentName = "Sophia", Grade = "4th", School = "Lincoln Elementary" }
            };

            // Act - Set up the complete route
            _context.Vehicles.Add(vehicle);
            _context.Drivers.Add(driver);
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            // Assign vehicle and driver to route
            route.VehicleId = vehicle.VehicleId;
            route.DriverId = driver.DriverId;

            // Assign students to route
            foreach (var student in students)
            {
                student.RouteId = route.RouteId;
                _context.Students.Add(student);
            }

            await _context.SaveChangesAsync();

            // Assert - Verify everything is connected properly
            var completeRoute = await _context.Routes
                .Include(r => r.Vehicle)
                .Include(r => r.Driver)
                .Include(r => r.Students)
                .FirstAsync(r => r.RouteId == route.RouteId);

            Assert.NotNull(completeRoute.Vehicle);
            Assert.NotNull(completeRoute.Driver);
            Assert.Equal(3, completeRoute.Students.Count);
            Assert.Equal("BUS101", completeRoute.Vehicle.BusNumber);
            Assert.Equal("Sarah Connor", completeRoute.Driver.DriverName);

            _output.WriteLine("🚌 Complete route setup verified:");
            _output.WriteLine($"  Route: {completeRoute.RouteName}");
            _output.WriteLine($"  Vehicle: {completeRoute.Vehicle.BusNumber}");
            _output.WriteLine($"  Driver: {completeRoute.Driver.DriverName}");
            _output.WriteLine($"  Students: {completeRoute.Students.Count} kids ready for school!");
            _output.WriteLine($"  Schedule: {completeRoute.StartTime} - {completeRoute.EndTime}");
        }

        [Fact]
        public async Task Integration_CompleteWorkflow_WorksCorrectly()
        {
            // This is the main integration test that validates the core workflow

            // Arrange - Create a complete transportation scenario
            var bus = new Bus
            {
                BusNumber = "INT-001",
                Make = "Integration",
                Model = "Test Bus",
                Year = 2023,
                SeatingCapacity = 40,
                VINNumber = "INT123456789",
                LicenseNumber = "INT-LIC-001",
                Status = "Active"
            };

            var driver = new Driver
            {
                DriverName = "Integration Driver",
                DriversLicenceType = "CDL",
                Status = "Active",
                TrainingComplete = true
            };

            var route = new Route
            {
                RouteName = "Integration Route",
                Date = DateTime.Today,
                School = "Integration School",
                IsActive = true
            };

            var student = new Student
            {
                StudentName = "Integration Student",
                Grade = "5",
                School = "Integration School",
                Active = true
            };

            // Act - Save all entities
            _context.Vehicles.Add(bus);
            _context.Drivers.Add(driver);
            _context.Routes.Add(route);
            _context.Students.Add(student);

            await _context.SaveChangesAsync();

            // Assign driver and bus to route
            route.AMVehicleId = bus.VehicleId;
            route.AMDriverId = driver.DriverId;

            await _context.SaveChangesAsync();

            // Assert - Verify complete integration
            var savedBus = await _context.Vehicles.FirstOrDefaultAsync(b => b.BusNumber == "INT-001");
            var savedDriver = await _context.Drivers.FirstOrDefaultAsync(d => d.DriverName == "Integration Driver");
            var savedRoute = await _context.Routes
                .Include(r => r.AMVehicle)
                .Include(r => r.AMDriver)
                .FirstOrDefaultAsync(r => r.RouteName == "Integration Route");
            var savedStudent = await _context.Students.FirstOrDefaultAsync(s => s.StudentName == "Integration Student");

            // Verify all entities exist
            Assert.NotNull(savedBus);
            Assert.NotNull(savedDriver);
            Assert.NotNull(savedRoute);
            Assert.NotNull(savedStudent);

            // Verify relationships
            Assert.Equal(bus.VehicleId, savedRoute.AMVehicleId);
            Assert.Equal(driver.DriverId, savedRoute.AMDriverId);

            _output.WriteLine($"Integration test completed successfully:");
            _output.WriteLine($"- Bus: {savedBus.BusNumber}");
            _output.WriteLine($"- Driver: {savedDriver.DriverName}");
            _output.WriteLine($"- Route: {savedRoute.RouteName}");
            _output.WriteLine($"- Student: {savedStudent.StudentName}");
        }

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context?.Dispose();
                _serviceProvider?.GetService<IServiceScope>()?.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
