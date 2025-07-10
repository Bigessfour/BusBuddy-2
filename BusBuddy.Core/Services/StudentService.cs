using BusBuddy.Core.Data;
using BusBuddy.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace BusBuddy.Core.Services;

/// <summary>
/// Service implementation for managing student transportation records
/// Provides CRUD operations and business logic for student management
/// </summary>
public class StudentService : IStudentService
{
    private readonly ILogger<StudentService> _logger;
    private readonly IBusBuddyDbContextFactory _contextFactory;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public StudentService(ILogger<StudentService> logger, IBusBuddyDbContextFactory contextFactory)
    {
        _logger = logger;
        _contextFactory = contextFactory;
    }

    #region Read Operations

    public async Task<List<Student>> GetAllStudentsAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            _logger.LogInformation("Retrieving all students from database");
            var context = _contextFactory.CreateDbContext();
            try
            {
                return await context.Students
                    .AsNoTracking() // Use AsNoTracking for better performance in read operations
                    .OrderBy(s => s.StudentName)
                    .ToListAsync();
            }
            finally
            {
                // Properly dispose the context when done
                await context.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all students");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<Student?> GetStudentByIdAsync(int studentId)
    {
        try
        {
            _logger.LogInformation("Retrieving student with ID: {StudentId}", studentId);
            var context = _contextFactory.CreateDbContext();
            try
            {
                return await context.Students
                    .AsNoTracking() // Use AsNoTracking for better performance in read operations
                    .FirstOrDefaultAsync(s => s.StudentId == studentId);
            }
            finally
            {
                // Properly dispose the context when done
                await context.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student with ID: {StudentId}", studentId);
            throw;
        }
    }

    public async Task<List<Student>> GetStudentsByGradeAsync(string grade)
    {
        try
        {
            _logger.LogInformation("Retrieving students in grade: {Grade}", grade);
            var context = _contextFactory.CreateDbContext();
            try
            {
                return await context.Students
                    .AsNoTracking() // Use AsNoTracking for better performance in read operations
                    .Where(s => s.Grade == grade)
                    .OrderBy(s => s.StudentName)
                    .ToListAsync();
            }
            finally
            {
                // Properly dispose the context when done
                await context.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving students by grade: {Grade}", grade);
            throw;
        }
    }

    public async Task<List<Student>> GetStudentsByRouteAsync(string routeName)
    {
        try
        {
            _logger.LogInformation("Retrieving students on route: {RouteName}", routeName);
            // Don't dispose the context here as it might be needed after the method returns
            var context = _contextFactory.CreateDbContext();
            return await context.Students
                .Where(s => s.AMRoute == routeName || s.PMRoute == routeName)
                .OrderBy(s => s.StudentName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving students by route: {RouteName}", routeName);
            throw;
        }
    }

    public async Task<List<Student>> GetActiveStudentsAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving active students");
            // Don't dispose the context here as it might be needed after the method returns
            var context = _contextFactory.CreateDbContext();
            return await context.Students
                .Where(s => s.Active)
                .OrderBy(s => s.StudentName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active students");
            throw;
        }
    }

    public async Task<List<Student>> GetStudentsBySchoolAsync(string school)
    {
        try
        {
            _logger.LogInformation("Retrieving students from school: {School}", school);
            // Don't dispose the context here as it might be needed after the method returns
            var context = _contextFactory.CreateDbContext();
            return await context.Students
                .Where(s => s.School == school)
                .OrderBy(s => s.StudentName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving students by school: {School}", school);
            throw;
        }
    }

    public async Task<List<Student>> SearchStudentsAsync(string searchTerm)
    {
        try
        {
            _logger.LogInformation("Searching students with term: {SearchTerm}", searchTerm);

            var term = searchTerm.ToLower();
            // Don't dispose the context here as it might be needed after the method returns
            var context = _contextFactory.CreateDbContext();
            return await context.Students
                .Where(s => s.StudentName.ToLower().Contains(term) ||
                           (s.StudentNumber != null && s.StudentNumber.ToLower().Contains(term)))
                .OrderBy(s => s.StudentName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching students with term: {SearchTerm}", searchTerm);
            throw;
        }
    }

    #endregion

    #region Write Operations

    public async Task<Student> AddStudentAsync(Student student)
    {
        try
        {
            _logger.LogInformation("Adding new student: {StudentName}", student.StudentName);

            // Validate student data
            var validationErrors = await ValidateStudentAsync(student);
            if (validationErrors.Count > 0)
            {
                throw new ArgumentException($"Student validation failed: {string.Join(", ", validationErrors)}");
            }

            // Set default values
            if (student.EnrollmentDate == null)
            {
                student.EnrollmentDate = DateTime.Today;
            }

            using var context = _contextFactory.CreateWriteDbContext();
            context.Students.Add(student);
            await context.SaveChangesAsync();

            _logger.LogInformation("Successfully added student with ID: {StudentId}", student.StudentId);
            return student;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding student: {StudentName}", student.StudentName);
            throw;
        }
    }

    public async Task<bool> UpdateStudentAsync(Student student)
    {
        try
        {
            _logger.LogInformation("Updating student with ID: {StudentId}", student.StudentId);

            // Validate student data
            var validationErrors = await ValidateStudentAsync(student);
            if (validationErrors.Count > 0)
            {
                throw new ArgumentException($"Student validation failed: {string.Join(", ", validationErrors)}");
            }

            using var context = _contextFactory.CreateWriteDbContext();
            context.Students.Update(student);
            var result = await context.SaveChangesAsync();

            var success = result > 0;
            if (success)
            {
                _logger.LogInformation("Successfully updated student: {StudentName}", student.StudentName);
            }
            else
            {
                _logger.LogWarning("No changes were made when updating student: {StudentId}", student.StudentId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating student with ID: {StudentId}", student.StudentId);
            throw;
        }
    }

    public async Task<bool> DeleteStudentAsync(int studentId)
    {
        try
        {
            _logger.LogInformation("Deleting student with ID: {StudentId}", studentId);

            using var context = _contextFactory.CreateWriteDbContext();
            var student = await context.Students.FindAsync(studentId);
            if (student != null)
            {
                context.Students.Remove(student);
                var result = await context.SaveChangesAsync();

                var success = result > 0;
                if (success)
                {
                    _logger.LogInformation("Successfully deleted student: {StudentName}", student.StudentName);
                }

                return success;
            }

            _logger.LogWarning("Student with ID {StudentId} not found for deletion", studentId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting student with ID: {StudentId}", studentId);
            throw;
        }
    }

    #endregion

    #region Validation and Business Logic

    public async Task<List<string>> ValidateStudentAsync(Student student)
    {
        var errors = new List<string>();

        try
        {
            // Required field validation
            if (string.IsNullOrWhiteSpace(student.StudentName))
            {
                errors.Add("Student name is required");
            }

            // Create a context that will be disposed at the end of this method,
            // since the validation results are returned as a new list (not dependent on the context)
            using var context = _contextFactory.CreateDbContext();

            // Student number uniqueness check (if provided)
            if (!string.IsNullOrWhiteSpace(student.StudentNumber))
            {
                var existingStudent = await context.Students
                    .Where(s => s.StudentNumber == student.StudentNumber && s.StudentId != student.StudentId)
                    .FirstOrDefaultAsync();

                if (existingStudent != null)
                {
                    errors.Add($"Student number '{student.StudentNumber}' is already in use");
                }
            }

            // Grade validation
            if (!string.IsNullOrWhiteSpace(student.Grade))
            {
                var validGrades = new[] { "Pre-K", "K", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" };
                if (!validGrades.Contains(student.Grade))
                {
                    errors.Add("Invalid grade level");
                }
            }

            // Phone number format validation
            if (!string.IsNullOrWhiteSpace(student.HomePhone))
            {
                var phonePattern = @"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$";
                if (!System.Text.RegularExpressions.Regex.IsMatch(student.HomePhone, phonePattern))
                {
                    errors.Add("Invalid home phone number format");
                }
            }

            if (!string.IsNullOrWhiteSpace(student.EmergencyPhone))
            {
                var phonePattern = @"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$";
                if (!System.Text.RegularExpressions.Regex.IsMatch(student.EmergencyPhone, phonePattern))
                {
                    errors.Add("Invalid emergency phone number format");
                }
            }

            // State validation
            if (!string.IsNullOrWhiteSpace(student.State))
            {
                if (student.State.Length != 2)
                {
                    errors.Add("State must be a 2-letter abbreviation");
                }
            }

            // ZIP code validation
            if (!string.IsNullOrWhiteSpace(student.Zip))
            {
                var zipPattern = @"^\d{5}(-\d{4})?$";
                if (!System.Text.RegularExpressions.Regex.IsMatch(student.Zip, zipPattern))
                {
                    errors.Add("Invalid ZIP code format");
                }
            }

            // Route validation (if routes exist in database)
            if (!string.IsNullOrWhiteSpace(student.AMRoute))
            {
                try
                {
                    var amRouteExists = await context.Routes.AnyAsync(r => r.RouteName == student.AMRoute);
                    if (!amRouteExists)
                    {
                        errors.Add($"AM Route '{student.AMRoute}' does not exist");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error validating AM route: {AMRoute}", student.AMRoute);
                    errors.Add($"AM Route '{student.AMRoute}' does not exist");
                }
            }

            if (!string.IsNullOrWhiteSpace(student.PMRoute))
            {
                try
                {
                    var pmRouteExists = await context.Routes.AnyAsync(r => r.RouteName == student.PMRoute);
                    if (!pmRouteExists)
                    {
                        errors.Add($"PM Route '{student.PMRoute}' does not exist");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error validating PM route: {PMRoute}", student.PMRoute);
                    errors.Add($"PM Route '{student.PMRoute}' does not exist");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during basic student validation");
            errors.Add("Validation error occurred");
        }

        return errors;
    }

    #endregion

    #region Statistics and Reporting

    public async Task<Dictionary<string, int>> GetStudentStatisticsAsync()
    {
        try
        {
            _logger.LogInformation("Calculating student statistics");

            using var context = _contextFactory.CreateDbContext();
            var stats = new Dictionary<string, int>
            {
                ["TotalStudents"] = await context.Students.CountAsync(),
                ["ActiveStudents"] = await context.Students.CountAsync(s => s.Active),
                ["InactiveStudents"] = await context.Students.CountAsync(s => !s.Active),
                ["StudentsWithRoutes"] = await context.Students.CountAsync(s => !string.IsNullOrEmpty(s.AMRoute) || !string.IsNullOrEmpty(s.PMRoute)),
                ["StudentsWithoutRoutes"] = await context.Students.CountAsync(s => string.IsNullOrEmpty(s.AMRoute) && string.IsNullOrEmpty(s.PMRoute))
            };

            // Grade level counts
            var gradeCounts = await context.Students
                .Where(s => !string.IsNullOrEmpty(s.Grade))
                .GroupBy(s => s.Grade)
                .Select(g => new { Grade = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var gradeCount in gradeCounts)
            {
                stats[$"Grade_{gradeCount.Grade}"] = gradeCount.Count;
            }

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating student statistics");
            throw;
        }
    }

    public async Task<List<Student>> GetStudentsWithMissingInfoAsync()
    {
        try
        {
            _logger.LogInformation("Finding students with missing required information");

            using var context = _contextFactory.CreateDbContext();
            return await context.Students
                .Where(s => string.IsNullOrEmpty(s.ParentGuardian) ||
                           string.IsNullOrEmpty(s.EmergencyPhone) ||
                           string.IsNullOrEmpty(s.HomeAddress) ||
                           string.IsNullOrEmpty(s.Grade))
                .OrderBy(s => s.StudentName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding students with missing information");
            throw;
        }
    }

    #endregion

    #region Route Assignment

    public async Task<bool> AssignStudentToRouteAsync(int studentId, string? amRoute, string? pmRoute)
    {
        try
        {
            _logger.LogInformation("Assigning student {StudentId} to routes - AM: {AMRoute}, PM: {PMRoute}",
                studentId, amRoute, pmRoute);

            using var context = _contextFactory.CreateWriteDbContext();
            var student = await context.Students.FindAsync(studentId);
            if (student == null)
            {
                _logger.LogWarning("Student with ID {StudentId} not found", studentId);
                return false;
            }

            student.AMRoute = amRoute;
            student.PMRoute = pmRoute;

            var result = await context.SaveChangesAsync();
            var success = result > 0;

            if (success)
            {
                _logger.LogInformation("Successfully assigned routes for student: {StudentName}", student.StudentName);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning routes for student {StudentId}", studentId);
            throw;
        }
    }

    public async Task<bool> UpdateStudentActiveStatusAsync(int studentId, bool isActive)
    {
        try
        {
            _logger.LogInformation("Updating active status for student {StudentId} to {IsActive}", studentId, isActive);

            using var context = _contextFactory.CreateWriteDbContext();
            var student = await context.Students.FindAsync(studentId);
            if (student == null)
            {
                _logger.LogWarning("Student with ID {StudentId} not found", studentId);
                return false;
            }

            student.Active = isActive;
            var result = await context.SaveChangesAsync();
            var success = result > 0;

            if (success)
            {
                _logger.LogInformation("Successfully updated active status for student: {StudentName}", student.StudentName);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating active status for student {StudentId}", studentId);
            throw;
        }
    }

    #endregion

    #region Export

    public async Task<string> ExportStudentsToCsvAsync()
    {
        try
        {
            _logger.LogInformation("Exporting students to CSV format");

            var students = await GetAllStudentsAsync();
            var csv = new StringBuilder();

            // CSV Header
            csv.AppendLine("Student ID,Student Number,Student Name,Grade,School,Home Address,City,State,ZIP," +
                          "Home Phone,Parent/Guardian,Emergency Phone,AM Route,PM Route,Bus Stop," +
                          "Medical Notes,Transportation Notes,Active,Enrollment Date");

            // CSV Data
            foreach (var student in students)
            {
                csv.AppendLine($"{student.StudentId}," +
                              $"\"{student.StudentNumber ?? ""}\"," +
                              $"\"{student.StudentName}\"," +
                              $"\"{student.Grade ?? ""}\"," +
                              $"\"{student.School ?? ""}\"," +
                              $"\"{student.HomeAddress ?? ""}\"," +
                              $"\"{student.City ?? ""}\"," +
                              $"\"{student.State ?? ""}\"," +
                              $"\"{student.Zip ?? ""}\"," +
                              $"\"{student.HomePhone ?? ""}\"," +
                              $"\"{student.ParentGuardian ?? ""}\"," +
                              $"\"{student.EmergencyPhone ?? ""}\"," +
                              $"\"{student.AMRoute ?? ""}\"," +
                              $"\"{student.PMRoute ?? ""}\"," +
                              $"\"{student.BusStop ?? ""}\"," +
                              $"\"{student.MedicalNotes ?? ""}\"," +
                              $"\"{student.TransportationNotes ?? ""}\"," +
                              $"{student.Active}," +
                              $"{student.EnrollmentDate?.ToString("yyyy-MM-dd") ?? ""}");
            }

            _logger.LogInformation("Successfully exported {Count} students to CSV", students.Count);
            return csv.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting students to CSV");
            throw;
        }
    }

    #endregion
}
