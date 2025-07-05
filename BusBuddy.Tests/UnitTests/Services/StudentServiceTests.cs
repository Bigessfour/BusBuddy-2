using Bus_Buddy.Data;
using Bus_Buddy.Data.Interfaces;
using Bus_Buddy.Data.Repositories;
using Bus_Buddy.Data.UnitOfWork;
using Bus_Buddy.Models;
using Bus_Buddy.Services;
using BusBuddy.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace BusBuddy.Tests.UnitTests.Services
{
    /// <summary>
    /// Comprehensive unit tests for StudentService implementation
    /// Tests critical business logic: student management, route assignment, validation
    /// PRIORITY: CRITICAL - Core student safety and transportation management
    /// </summary>
    [TestFixture]
    [NonParallelizable] // Database tests need to run sequentially
    public class StudentServiceTests : TestBase
    {
        private IStudentService _studentService = null!;
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
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IRepository<Student>, Repository<Student>>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            // Register any other dependencies needed by StudentService
            _testServiceProvider = services.BuildServiceProvider();
            _studentService = _testServiceProvider.GetRequiredService<IStudentService>();
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

        #region Read Operations Tests

        [Test]
        public async Task GetAllStudentsAsync_ShouldReturnAllStudents_OrderedByName()
        {
            // Arrange
            var students = new List<Student>
            {
                new Student
                {
                    StudentName = "Charlie Brown",
                    Grade = "3",
                    Active = true,
                    EnrollmentDate = DateTime.Today.AddDays(-30)
                },
                new Student
                {
                    StudentName = "Alice Smith",
                    Grade = "2",
                    Active = true,
                    EnrollmentDate = DateTime.Today.AddDays(-20)
                },
                new Student
                {
                    StudentName = "Bob Johnson",
                    Grade = "4",
                    Active = false,
                    EnrollmentDate = DateTime.Today.AddDays(-10)
                }
            };

            _testDbContext.Students.AddRange(students);
            await _testDbContext.SaveChangesAsync();

            // Act
            var result = await _studentService.GetAllStudentsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeInAscendingOrder(s => s.StudentName);
            result.First().StudentName.Should().Be("Alice Smith");
            result.Last().StudentName.Should().Be("Charlie Brown");
        }

        [Test]
        public async Task GetAllStudentsAsync_ShouldReturnEmptyList_WhenNoStudentsExist()
        {
            // Act
            var result = await _studentService.GetAllStudentsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetStudentByIdAsync_ShouldReturnStudent_WhenStudentExists()
        {
            // Arrange
            var student = new Student
            {
                StudentName = "Test Student",
                StudentNumber = "S12345",
                Grade = "5",
                School = "Elementary School",
                Active = true,
                EnrollmentDate = DateTime.Today
            };

            _testDbContext.Students.Add(student);
            await _testDbContext.SaveChangesAsync();

            // Act
            var result = await _studentService.GetStudentByIdAsync(student.StudentId);

            // Assert
            result.Should().NotBeNull();
            result!.StudentId.Should().Be(student.StudentId);
            result.StudentName.Should().Be("Test Student");
            result.StudentNumber.Should().Be("S12345");
            result.Grade.Should().Be("5");
        }

        [Test]
        public async Task GetStudentByIdAsync_ShouldReturnNull_WhenStudentDoesNotExist()
        {
            // Act
            var result = await _studentService.GetStudentByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task GetStudentsByGradeAsync_ShouldReturnStudentsInSpecificGrade()
        {
            // Arrange
            var students = new List<Student>
            {
                new Student { StudentName = "Grade 3 Student 1", Grade = "3", Active = true },
                new Student { StudentName = "Grade 3 Student 2", Grade = "3", Active = true },
                new Student { StudentName = "Grade 4 Student", Grade = "4", Active = true },
                new Student { StudentName = "Grade 5 Student", Grade = "5", Active = true }
            };

            _testDbContext.Students.AddRange(students);
            await _testDbContext.SaveChangesAsync();

            // Act
            var result = await _studentService.GetStudentsByGradeAsync("3");

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().OnlyContain(s => s.Grade == "3");
            result.Should().BeInAscendingOrder(s => s.StudentName);
        }

        [Test]
        public async Task GetStudentsByRouteAsync_ShouldReturnStudentsOnRoute_AMOrPM()
        {
            // Arrange
            var route = new Route { RouteName = "Route A", Date = DateTime.Today, Description = "Test Route A" };
            _testDbContext.Routes.Add(route);
            await _testDbContext.SaveChangesAsync();

            var students = new List<Student>
            {
                new Student
                {
                    StudentName = "AM Route Student",
                    AMRoute = "Route A",
                    PMRoute = "Route B",
                    Active = true
                },
                new Student
                {
                    StudentName = "PM Route Student",
                    AMRoute = "Route B",
                    PMRoute = "Route A",
                    Active = true
                },
                new Student
                {
                    StudentName = "Different Route Student",
                    AMRoute = "Route C",
                    PMRoute = "Route D",
                    Active = true
                }
            };

            _testDbContext.Students.AddRange(students);
            await _testDbContext.SaveChangesAsync();

            // Act
            var result = await _studentService.GetStudentsByRouteAsync("Route A");

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().OnlyContain(s => s.AMRoute == "Route A" || s.PMRoute == "Route A");
            result.Should().Contain(s => s.StudentName == "AM Route Student");
            result.Should().Contain(s => s.StudentName == "PM Route Student");
        }

        [Test]
        public async Task GetActiveStudentsAsync_ShouldReturnOnlyActiveStudents()
        {
            // Arrange
            var students = new List<Student>
            {
                new Student { StudentName = "Active Student 1", Active = true },
                new Student { StudentName = "Active Student 2", Active = true },
                new Student { StudentName = "Inactive Student", Active = false }
            };

            _testDbContext.Students.AddRange(students);
            await _testDbContext.SaveChangesAsync();

            // Act
            var result = await _studentService.GetActiveStudentsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().OnlyContain(s => s.Active == true);
        }

        [Test]
        public async Task GetStudentsBySchoolAsync_ShouldReturnStudentsFromSpecificSchool()
        {
            // Arrange
            var students = new List<Student>
            {
                new Student { StudentName = "Elementary Student 1", School = "Lincoln Elementary", Active = true },
                new Student { StudentName = "Elementary Student 2", School = "Lincoln Elementary", Active = true },
                new Student { StudentName = "Middle School Student", School = "Washington Middle", Active = true }
            };

            _testDbContext.Students.AddRange(students);
            await _testDbContext.SaveChangesAsync();

            // Act
            var result = await _studentService.GetStudentsBySchoolAsync("Lincoln Elementary");

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().OnlyContain(s => s.School == "Lincoln Elementary");
        }

        [Test]
        public async Task SearchStudentsAsync_ShouldFindStudentsByNameOrNumber()
        {
            // Arrange
            var students = new List<Student>
            {
                new Student { StudentName = "John Smith", StudentNumber = "S001", Active = true },
                new Student { StudentName = "Jane Doe", StudentNumber = "S002", Active = true },
                new Student { StudentName = "Bob Johnson", StudentNumber = "S003", Active = true }
            };

            _testDbContext.Students.AddRange(students);
            await _testDbContext.SaveChangesAsync();

            // Act - Search by name
            var nameResult = await _studentService.SearchStudentsAsync("john");

            // Act - Search by student number
            var numberResult = await _studentService.SearchStudentsAsync("S002");

            // Assert - Name search
            nameResult.Should().NotBeNull();
            nameResult.Should().HaveCount(2); // John Smith and Bob Johnson
            nameResult.Should().Contain(s => s.StudentName == "John Smith");
            nameResult.Should().Contain(s => s.StudentName == "Bob Johnson");

            // Assert - Number search
            numberResult.Should().NotBeNull();
            numberResult.Should().HaveCount(1);
            numberResult.First().StudentNumber.Should().Be("S002");
        }

        #endregion

        #region Write Operations Tests

        [Test]
        public async Task AddStudentAsync_ShouldCreateStudent_WithValidData()
        {
            // Arrange
            var student = new Student
            {
                StudentName = "New Student",
                StudentNumber = "S100",
                Grade = "6",
                School = "Test School",
                HomeAddress = "123 Main St",
                City = "Test City",
                State = "TX",
                Zip = "12345",
                HomePhone = "(555) 123-4567", // Fixed: proper phone format
                ParentGuardian = "Parent Name",
                EmergencyPhone = "(555) 987-6543", // Fixed: proper phone format
                Active = true
            };

            // Act
            var result = await _studentService.AddStudentAsync(student);

            // Assert
            result.Should().NotBeNull();
            result.StudentId.Should().BeGreaterThan(0);
            result.StudentName.Should().Be("New Student");
            result.EnrollmentDate.Should().BeCloseTo(DateTime.Today, TimeSpan.FromDays(1));
            result.Active.Should().BeTrue();
        }
        [Test]
        public async Task AddStudentAsync_ShouldThrowException_WhenValidationFails()
        {
            // Arrange
            var student = new Student
            {
                StudentName = "", // Invalid - required field
                Grade = "Invalid Grade", // Invalid grade
                HomePhone = "invalid-phone", // Invalid phone format
                State = "INVALID", // Invalid state format
                Zip = "invalid-zip" // Invalid ZIP format
            };

            // Act & Assert
            try
            {
                await _studentService.AddStudentAsync(student);
                Assert.Fail("Expected ArgumentException was not thrown");
            }
            catch (ArgumentException ex)
            {
                ex.Message.Should().Contain("Student validation failed");
            }
        }

        [Test]
        public async Task UpdateStudentAsync_ShouldUpdateStudent_WithValidData()
        {
            // Arrange
            var student = new Student
            {
                StudentName = "Original Name",
                StudentNumber = "S200",
                Grade = "7",
                Active = true,
                EnrollmentDate = DateTime.Today
            };

            _testDbContext.Students.Add(student);
            await _testDbContext.SaveChangesAsync();

            // Modify student
            student.StudentName = "Updated Name";
            student.Grade = "8";
            student.HomePhone = "(555) 999-9999"; // Fixed: proper phone format

            // Act
            var result = await _studentService.UpdateStudentAsync(student);

            // Assert
            result.Should().BeTrue();

            var updatedStudent = await _testDbContext.Students.FindAsync(student.StudentId);
            updatedStudent!.StudentName.Should().Be("Updated Name");
            updatedStudent.Grade.Should().Be("8");
            updatedStudent.HomePhone.Should().Be("(555) 999-9999");
        }

        [Test]
        public async Task DeleteStudentAsync_ShouldDeleteStudent_WhenStudentExists()
        {
            // Arrange
            var student = new Student
            {
                StudentName = "Delete Test Student",
                Active = true,
                EnrollmentDate = DateTime.Today
            };

            _testDbContext.Students.Add(student);
            await _testDbContext.SaveChangesAsync();

            // Act
            var result = await _studentService.DeleteStudentAsync(student.StudentId);

            // Assert
            result.Should().BeTrue();

            // ITERATION 8 FIX: Clear change tracker to get fresh entity state
            _testDbContext.ChangeTracker.Clear();
            var deletedStudent = await _testDbContext.Students.FindAsync(student.StudentId);
            deletedStudent.Should().BeNull();
        }

        [Test]
        public async Task DeleteStudentAsync_ShouldReturnFalse_WhenStudentDoesNotExist()
        {
            // Act
            var result = await _studentService.DeleteStudentAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Validation Tests

        [Test]
        public async Task ValidateStudentAsync_ShouldReturnErrors_WhenRequiredFieldsMissing()
        {
            // Arrange
            var student = new Student
            {
                StudentName = "", // Required field missing
                Grade = "Invalid Grade",
                HomePhone = "invalid-phone",
                EmergencyPhone = "invalid-emergency",
                State = "INVALID",
                Zip = "invalid-zip"
            };

            // Act
            var result = await _studentService.ValidateStudentAsync(student);

            // Assert
            result.Should().NotBeNull();
            result.Should().Contain("Student name is required");
            result.Should().Contain("Invalid grade level");
            result.Should().Contain("Invalid home phone number format");
            result.Should().Contain("Invalid emergency phone number format");
            result.Should().Contain("State must be a 2-letter abbreviation");
            result.Should().Contain("Invalid ZIP code format");
        }

        [Test]
        public async Task ValidateStudentAsync_ShouldReturnEmpty_WhenStudentIsValid()
        {
            // Arrange
            var route = new Route { RouteName = "Valid Route", Date = DateTime.Today, Description = "Test Route" };
            _testDbContext.Routes.Add(route);
            await _testDbContext.SaveChangesAsync();

            var student = new Student
            {
                StudentName = "Valid Student",
                StudentNumber = "S300",
                Grade = "9",
                HomePhone = "(555) 123-4567", // Fixed: proper phone format
                EmergencyPhone = "(555) 987-6543", // Fixed: proper phone format
                State = "TX",
                Zip = "12345",
                AMRoute = "Valid Route",
                Active = true
            };

            // Act
            var result = await _studentService.ValidateStudentAsync(student);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task ValidateStudentAsync_ShouldDetectDuplicateStudentNumber()
        {
            // Arrange
            var existingStudent = new Student
            {
                StudentName = "Existing Student",
                StudentNumber = "S400",
                Active = true,
                EnrollmentDate = DateTime.Today
            };

            _testDbContext.Students.Add(existingStudent);
            await _testDbContext.SaveChangesAsync();

            var newStudent = new Student
            {
                StudentName = "New Student",
                StudentNumber = "S400", // Duplicate number
                Active = true
            };

            // Act
            var result = await _studentService.ValidateStudentAsync(newStudent);

            // Assert
            result.Should().NotBeNull();
            result.Should().Contain("Student number 'S400' is already in use");
        }

        [Test]
        public async Task ValidateStudentAsync_ShouldValidateRouteExistence()
        {
            // Arrange
            var student = new Student
            {
                StudentName = "Route Test Student",
                AMRoute = "Nonexistent AM Route",
                PMRoute = "Nonexistent PM Route",
                Active = true
            };

            // Act
            var result = await _studentService.ValidateStudentAsync(student);

            // Assert
            result.Should().NotBeNull();
            result.Should().Contain("AM Route 'Nonexistent AM Route' does not exist");
            result.Should().Contain("PM Route 'Nonexistent PM Route' does not exist");
        }

        #endregion

        #region Business Logic Tests

        [Test]
        public async Task AssignStudentToRouteAsync_ShouldAssignRoutes_WhenStudentExists()
        {
            // Arrange
            var student = new Student
            {
                StudentName = "Route Assignment Student",
                Active = true,
                EnrollmentDate = DateTime.Today
            };

            _testDbContext.Students.Add(student);
            await _testDbContext.SaveChangesAsync();

            // Act
            var result = await _studentService.AssignStudentToRouteAsync(
                student.StudentId, "Morning Route", "Evening Route");

            // Assert
            result.Should().BeTrue();

            // ITERATION 8 FIX: Clear change tracker to get fresh entity state
            _testDbContext.ChangeTracker.Clear();
            var updatedStudent = await _testDbContext.Students.FindAsync(student.StudentId);
            updatedStudent!.AMRoute.Should().Be("Morning Route");
            updatedStudent.PMRoute.Should().Be("Evening Route");
        }

        [Test]
        public async Task AssignStudentToRouteAsync_ShouldReturnFalse_WhenStudentDoesNotExist()
        {
            // Act
            var result = await _studentService.AssignStudentToRouteAsync(999, "Route A", "Route B");

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task UpdateStudentActiveStatusAsync_ShouldUpdateStatus_WhenStudentExists()
        {
            // Arrange
            var student = new Student
            {
                StudentName = "Status Test Student",
                Active = true,
                EnrollmentDate = DateTime.Today
            };

            _testDbContext.Students.Add(student);
            await _testDbContext.SaveChangesAsync();

            // Act
            var result = await _studentService.UpdateStudentActiveStatusAsync(student.StudentId, false);

            // Assert
            result.Should().BeTrue();

            // ITERATION 8 FIX: Clear change tracker to get fresh entity state
            _testDbContext.ChangeTracker.Clear();
            var updatedStudent = await _testDbContext.Students.FindAsync(student.StudentId);
            updatedStudent!.Active.Should().BeFalse();
        }

        [Test]
        public async Task GetStudentStatisticsAsync_ShouldCalculateCorrectStatistics()
        {
            // Arrange
            var students = new List<Student>
            {
                new Student { StudentName = "Active Student 1", Active = true, Grade = "3", AMRoute = "Route A" },
                new Student { StudentName = "Active Student 2", Active = true, Grade = "3", PMRoute = "Route B" },
                new Student { StudentName = "Inactive Student", Active = false, Grade = "4" },
                new Student { StudentName = "No Route Student", Active = true, Grade = "5" }
            };

            _testDbContext.Students.AddRange(students);
            await _testDbContext.SaveChangesAsync();

            // Act
            var result = await _studentService.GetStudentStatisticsAsync();

            // Assert
            result.Should().NotBeNull();
            result["TotalStudents"].Should().Be(4);
            result["ActiveStudents"].Should().Be(3);
            result["InactiveStudents"].Should().Be(1);
            result["StudentsWithRoutes"].Should().Be(2); // Students with AM or PM routes
            result["StudentsWithoutRoutes"].Should().Be(2); // Students with no routes
            result["Grade_3"].Should().Be(2);
            result["Grade_4"].Should().Be(1);
            result["Grade_5"].Should().Be(1);
        }

        [Test]
        public async Task GetStudentsWithMissingInfoAsync_ShouldReturnStudentsWithMissingRequiredFields()
        {
            // Arrange
            var students = new List<Student>
            {
                new Student
                {
                    StudentName = "Complete Student",
                    ParentGuardian = "Parent",
                    EmergencyPhone = "(555) 123-4567", // Fixed: proper phone format
                    HomeAddress = "123 Main St",
                    Grade = "6",
                    Active = true
                },
                new Student
                {
                    StudentName = "Missing Parent",
                    ParentGuardian = "", // Missing
                    EmergencyPhone = "(555) 123-4567", // Fixed: proper phone format
                    HomeAddress = "123 Main St",
                    Grade = "7",
                    Active = true
                },
                new Student
                {
                    StudentName = "Missing Emergency Phone",
                    ParentGuardian = "Parent",
                    EmergencyPhone = "", // Missing
                    HomeAddress = "123 Main St",
                    Grade = "8",
                    Active = true
                }
            };

            _testDbContext.Students.AddRange(students);
            await _testDbContext.SaveChangesAsync();

            // Act
            var result = await _studentService.GetStudentsWithMissingInfoAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(s => s.StudentName == "Missing Parent");
            result.Should().Contain(s => s.StudentName == "Missing Emergency Phone");
            result.Should().NotContain(s => s.StudentName == "Complete Student");
        }

        [Test]
        public async Task ExportStudentsToCsvAsync_ShouldGenerateCorrectCsvFormat()
        {
            // Arrange
            var student = new Student
            {
                StudentName = "CSV Test Student",
                StudentNumber = "S500",
                Grade = "10",
                School = "Test High School",
                HomeAddress = "456 Oak Street",
                City = "Test City",
                State = "CA",
                Zip = "90210",
                HomePhone = "(555) 111-1111", // Fixed: proper phone format
                ParentGuardian = "Test Parent",
                EmergencyPhone = "(555) 222-2222", // Fixed: proper phone format
                AMRoute = "Morning Route",
                PMRoute = "Evening Route",
                BusStop = "Oak & Main",
                MedicalNotes = "No allergies",
                TransportationNotes = "Wheelchair accessible",
                Active = true,
                EnrollmentDate = DateTime.Today
            };

            _testDbContext.Students.Add(student);
            await _testDbContext.SaveChangesAsync();

            // Act
            var result = await _studentService.ExportStudentsToCsvAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();

            var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            lines.Should().HaveCountGreaterThanOrEqualTo(2); // Header + at least one data row

            // Check header
            lines[0].Should().Contain("Student ID,Student Number,Student Name,Grade");

            // Check data row contains student information
            var dataRow = lines[1];
            dataRow.Should().Contain("CSV Test Student");
            dataRow.Should().Contain("S500");
            dataRow.Should().Contain("10");
            dataRow.Should().Contain("Test High School");
        }

        #endregion
    }
}

